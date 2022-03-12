using System;
using System.Collections.Generic;
using Akka.Actor;
using SharpPulsar.Common;
using SharpPulsar.Interfaces;

/// <summary>
/// Licensed to the Apache Software Foundation (ASF) under one
/// or more contributor license agreements.  See the NOTICE file
/// distributed with this work for additional information
/// regarding copyright ownership.  The ASF licenses this file
/// to you under the Apache License, Version 2.0 (the
/// "License"); you may not use this file except in compliance
/// with the License.  You may obtain a copy of the License at
/// 
///   http://www.apache.org/licenses/LICENSE-2.0
/// 
/// Unless required by applicable law or agreed to in writing,
/// software distributed under the License is distributed on an
/// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
/// KIND, either express or implied.  See the License for the
/// specific language governing permissions and limitations
/// under the License.
/// </summary>
namespace SharpPulsar.ServiceProvider
{
    using JsonProcessingException = com.fasterxml.jackson.core.JsonProcessingException;
    using ObjectMapper = com.fasterxml.jackson.databind.ObjectMapper;
    using VisibleForTesting = com.google.common.annotations.VisibleForTesting;
    using Strings = com.google.common.@base.Strings;
    using HttpRequest = io.netty.handler.codec.http.HttpRequest;
    using HttpResponse = io.netty.handler.codec.http.HttpResponse;
    using DefaultThreadFactory = io.netty.util.concurrent.DefaultThreadFactory;
    using Data = lombok.Data;
    using NonNull = lombok.NonNull;
    using Slf4j = lombok.@extern.slf4j.Slf4j;
    using PulsarVersion = Org.Apache.Pulsar.PulsarVersion;
    using Authentication = Org.Apache.Pulsar.Client.Api.Authentication;
    using AuthenticationFactory = Org.Apache.Pulsar.Client.Api.AuthenticationFactory;
    using ControlledClusterFailoverBuilder = Org.Apache.Pulsar.Client.Api.ControlledClusterFailoverBuilder;
    using PulsarClient = Org.Apache.Pulsar.Client.Api.PulsarClient;
    using ServiceUrlProvider = Org.Apache.Pulsar.Client.Api.ServiceUrlProvider;
    using ObjectMapperFactory = Org.Apache.Pulsar.Common.Util.ObjectMapperFactory;
    using AsyncHttpClient = org.asynchttpclient.AsyncHttpClient;
    using AsyncHttpClientConfig = org.asynchttpclient.AsyncHttpClientConfig;
    using BoundRequestBuilder = org.asynchttpclient.BoundRequestBuilder;
    using DefaultAsyncHttpClient = org.asynchttpclient.DefaultAsyncHttpClient;
    using DefaultAsyncHttpClientConfig = org.asynchttpclient.DefaultAsyncHttpClientConfig;
    using Request = org.asynchttpclient.Request;
    using Response = org.asynchttpclient.Response;
    using DefaultKeepAliveStrategy = org.asynchttpclient.channel.DefaultKeepAliveStrategy;

    public class ControlledClusterFailover : ReceiveActor, IServiceUrlProvider
    {
        private const int DefaultConnectTimeoutInSeconds = 10;
        private const int DefaultReadTimeoutInSeconds = 30;
        private const int DefaultMaxRedirects = 20;

        private PulsarClientImpl pulsarClient;
        private volatile string currentPulsarServiceUrl;
        private volatile ControlledConfiguration currentControlledConfiguration;
        private readonly ScheduledExecutorService executor;
        private readonly long interval;
        private ObjectMapper objectMapper = null;
        private readonly AsyncHttpClient httpClient;
        private readonly BoundRequestBuilder requestBuilder;

        private ControlledClusterFailover(ControlledClusterFailoverBuilder Builder)
        {
            currentPulsarServiceUrl = Builder.defaultServiceUrl;
            interval = Builder.Interval;
            executor = Executors.newSingleThreadScheduledExecutor(new DefaultThreadFactory("pulsar-service-provider"));

            httpClient = BuildHttpClient();
            requestBuilder = httpClient.prepareGet(Builder.urlProvider).addHeader("Accept", "application/json");

            if (Builder.Header != null && Builder.Header.Count > 0)
            {
                Builder.Header.forEach(requestBuilder.addHeader);
            }
        }

        private AsyncHttpClient BuildHttpClient()
        {
            var ConfBuilder = new DefaultAsyncHttpClientConfig.Builder();
            ConfBuilder.setFollowRedirect(true);
            ConfBuilder.setMaxRedirects(DefaultMaxRedirects);
            ConfBuilder.setConnectTimeout(DefaultConnectTimeoutInSeconds * 1000);
            ConfBuilder.setReadTimeout(DefaultReadTimeoutInSeconds * 1000);
            ConfBuilder.setUserAgent(string.Format("Pulsar-Java-v{0}", PulsarVersion.Version));
            ConfBuilder.setKeepAliveStrategy(new DefaultKeepAliveStrategyAnonymousInnerClass(this));
            AsyncHttpClientConfig Config = ConfBuilder.build();
            return new DefaultAsyncHttpClient(Config);
        }

        private class DefaultKeepAliveStrategyAnonymousInnerClass : DefaultKeepAliveStrategy
        {
            private readonly ControlledClusterFailover outerInstance;

            public DefaultKeepAliveStrategyAnonymousInnerClass(ControlledClusterFailover OuterInstance)
            {
                outerInstance = OuterInstance;
            }

            public override bool keepAlive(InetSocketAddress RemoteAddress, Request AhcRequest, HttpRequest Request, HttpResponse Response)
            {
                // Close connection upon a server error or per HTTP spec
                return Response.status().code() / 100 != 5 && base.keepAlive(RemoteAddress, AhcRequest, Request, Response);
            }
        }

        public virtual void Initialize(PulsarClient Client)
        {
            pulsarClient = (PulsarClientImpl)Client;

            // start to check service url every 30 seconds
            executor.scheduleAtFixedRate(catchingAndLoggingThrowables(() =>
            {
                ControlledConfiguration ControlledConfiguration = null;
                try
                {
                    ControlledConfiguration = FetchControlledConfiguration();
                    if (ControlledConfiguration != null && !Strings.isNullOrEmpty(ControlledConfiguration.getServiceUrl()) && !ControlledConfiguration.Equals(currentControlledConfiguration))
                    {
                        log.info("Switch Pulsar service url from {} to {}", currentControlledConfiguration, ControlledConfiguration.ToString());
                        Authentication Authentication = null;
                        if (!Strings.isNullOrEmpty(ControlledConfiguration.AuthPluginClassName) && !Strings.isNullOrEmpty(ControlledConfiguration.getAuthParamsString()))
                        {
                            Authentication = AuthenticationFactory.create(ControlledConfiguration.getAuthPluginClassName(), ControlledConfiguration.getAuthParamsString());
                        }
                        string TlsTrustCertsFilePath = ControlledConfiguration.getTlsTrustCertsFilePath();
                        string ServiceUrl = ControlledConfiguration.getServiceUrl();
                        if (Authentication != null)
                        {
                            pulsarClient.UpdateAuthentication(Authentication);
                        }
                        if (!Strings.isNullOrEmpty(TlsTrustCertsFilePath))
                        {
                            pulsarClient.UpdateTlsTrustCertsFilePath(TlsTrustCertsFilePath);
                        }
                        pulsarClient.UpdateServiceUrl(ServiceUrl);
                        currentPulsarServiceUrl = ServiceUrl;
                        currentControlledConfiguration = ControlledConfiguration;
                    }
                }
                catch (IOException E)
                {
                    log.error("Failed to switch new Pulsar url, current: {}, new: {}", currentControlledConfiguration, ControlledConfiguration, E);
                }
            }), interval, interval, TimeUnit.MILLISECONDS);
        }

        public virtual string CurrentPulsarServiceUrl
        {
            get
            {
                return currentPulsarServiceUrl;
            }
        }

        // JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
        // ORIGINAL LINE: @VisibleForTesting protected org.asynchttpclient.BoundRequestBuilder getRequestBuilder()
        protected internal virtual BoundRequestBuilder RequestBuilder
        {
            get
            {
                return requestBuilder;
            }
        }

        // JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
        // ORIGINAL LINE: protected ControlledConfiguration fetchControlledConfiguration() throws java.io.IOException
        protected internal virtual ControlledConfiguration FetchControlledConfiguration()
        {
            // call the service to get service URL
            try
            {
                Response Response = requestBuilder.execute().get();
                int StatusCode = Response.getStatusCode();
                if (StatusCode == 200)
                {
                    string Content = Response.getResponseBody(StandardCharsets.UTF_8);
                    return ObjectMapper.readValue(Content, typeof(ControlledConfiguration));
                }
                log.warn("Failed to fetch controlled configuration, status code: {}", StatusCode);
            }
            catch (Exception e) when (e is InterruptedException || e is ExecutionException)
            {
                log.error("Failed to fetch controlled configuration ", e);
            }

            return null;
        }

        private ObjectMapper ObjectMapper
        {
            get
            {
                if (objectMapper == null)
                {
                    objectMapper = new ObjectMapper();
                }
                return objectMapper;
            }
        }

        // JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
        // ORIGINAL LINE: @Data protected static class ControlledConfiguration
        protected internal class ControlledConfiguration
        {
            internal string ServiceUrl;
            internal string TlsTrustCertsFilePath;

            internal string AuthPluginClassName;
            internal string AuthParamsString;

            public virtual string ToJson()
            {
                ObjectMapper ObjectMapper = ObjectMapperFactory.ThreadLocal;
                try
                {
                    return ObjectMapper.writeValueAsString(this);
                }
                catch (JsonProcessingException E)
                {
                    log.warn("Failed to write as json. ", E);
                    return null;
                }
            }
        }

        public virtual string ServiceUrl
        {
            get
            {
                return currentPulsarServiceUrl;
            }
        }

        public virtual void close()
        {
            executor.shutdown();
            if (httpClient != null)
            {
                try
                {
                    httpClient.close();
                }
                catch (IOException)
                {
                    log.error("Failed to close http client.");
                }
            }
        }

        public class ControlledClusterFailoverBuilder : IControlledClusterFailoverBuilder
        {
            internal string defaultServiceUrl;
            internal string urlProvider;
            internal IDictionary<string, string> Header = null;
            internal long Interval = 30_000;

            // JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
            // ORIGINAL LINE: @Override public org.apache.pulsar.client.api.ControlledClusterFailoverBuilder defaultServiceUrl(@NonNull String serviceUrl)
            public virtual ControlledClusterFailoverBuilder DefaultServiceUrl(string ServiceUrl)
            {
                defaultServiceUrl = ServiceUrl;
                return this;
            }

            // JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
            // ORIGINAL LINE: @Override public org.apache.pulsar.client.api.ControlledClusterFailoverBuilder urlProvider(@NonNull String urlProvider)
            public virtual ControlledClusterFailoverBuilder UrlProvider(string UrlProvider)
            {
                urlProvider = UrlProvider;
                return this;
            }

            public virtual ControlledClusterFailoverBuilder UrlProviderHeader(IDictionary<string, string> header)
            {
                Header = header;
                return this;
            }

            // JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
            // ORIGINAL LINE: @Override public org.apache.pulsar.client.api.ControlledClusterFailoverBuilder checkInterval(long interval, @NonNull TimeUnit timeUnit)
            public virtual ControlledClusterFailoverBuilder CheckInterval(long interval, TimeUnit TimeUnit)
            {
                Interval = TimeUnit.toMillis(interval);
                return this;
            }

            // JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
            // ORIGINAL LINE: @Override public org.apache.pulsar.client.api.ServiceUrlProvider build() throws java.io.IOException
            public virtual ServiceUrlProvider Build()
            {
                Objects.requireNonNull(defaultServiceUrl, "default service url shouldn't be null");
                Objects.requireNonNull(urlProvider, "urlProvider shouldn't be null");
                CheckArgument(Interval > 0, "checkInterval should > 0");

                return new ControlledClusterFailover(this);
            }

            // JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
            // ORIGINAL LINE: public static void checkArgument(boolean expression, @Nullable Object errorMessage)
            public static void CheckArgument(bool Expression, object ErrorMessage)
            {
                if (!Expression)
                {
                    throw new ArgumentException(ErrorMessage.ToString());
                }
            }
        }

        public static ControlledClusterFailoverBuilder Builder()
        {
            return new ControlledClusterFailoverBuilder();
        }
    }

}