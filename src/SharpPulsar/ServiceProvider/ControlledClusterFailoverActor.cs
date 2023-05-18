using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using Akka.Actor;
using Akka.Annotations;
using Akka.Util.Internal;
using SharpPulsar.Auth;
using SharpPulsar.Builder;
using SharpPulsar.Interfaces;
using SharpPulsar.Protocol;
using SharpPulsar.ServiceProvider.Messages;

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
    [InternalApi]
    public class ControlledClusterFailoverActor : ReceiveActor
    {
        private const int DefaultConnectTimeoutInSeconds = 10;
        private const int DefaultMaxRedirects = 20;
        private PulsarClient pulsarClient;
        private volatile string _currentPulsarServiceUrl;
        private volatile ControlledConfiguration currentControlledConfiguration;
        private ICancelable executor;
        private readonly TimeSpan interval;
        private readonly HttpClient httpClient;
        private readonly ILoggingAdapter _log;

        private ControlledClusterFailoverActor(ControlledClusterFailoverBuilder builder)
        {
            _log = Context.GetLogger();
            _currentPulsarServiceUrl = builder.defaultServiceUrl;
            interval = builder.Interval;

            httpClient = BuildHttpClient();
            httpClient.BaseAddress = new Uri($"{builder.urlProvider}/admin/v2/");
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (builder.Header != null && builder.Header.Count > 0)
            {
                builder.Header.ForEach(h=> httpClient.DefaultRequestHeaders.Add(h.Key, h.Value));
            }
            Receive<Initialize>(_ =>
            {
                Initialize(_.Client);
            });
            Receive<GetServiceUrl>(_ =>
            {
                Sender.Tell(_currentPulsarServiceUrl);
            });
        }

        public static Props Prop(ControlledClusterFailoverBuilder builder)
        {
            return Props.Create(() => new ControlledClusterFailoverActor(builder));
        }
        private HttpClient BuildHttpClient()
        {
            var handler = new HttpClientHandler
            {
                AllowAutoRedirect = true,
                MaxAutomaticRedirections = DefaultMaxRedirects
            };
            var client = new HttpClient(handler)
            {
                Timeout = TimeSpan.FromMilliseconds(DefaultConnectTimeoutInSeconds * 1000)
            };

            client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue(new ProductHeaderValue($"Sharp-Pulsar-v{Commands.CurrentProtocolVersion}")));
            client.DefaultRequestHeaders.Connection.Add("keep-alive");
            return client;
        }

        public virtual void Initialize(PulsarClient client)
        {
            pulsarClient = client;

            // start to check service url every 30 seconds
            executor = Context.System.Scheduler.Advanced.ScheduleRepeatedlyCancelable(interval, interval, () => 
            {
                ControlledConfiguration controlledConfiguration = null;
                try
                {
                    controlledConfiguration = FetchControlledConfiguration();
                    if (controlledConfiguration != null && !string.IsNullOrWhiteSpace(controlledConfiguration.ServiceUrl) && !controlledConfiguration.Equals(currentControlledConfiguration))
                    {
                        _log.Info($"Switch Pulsar service url from {currentControlledConfiguration} to {controlledConfiguration}");
                        IAuthentication authentication = null;
                        if (!string.IsNullOrWhiteSpace(controlledConfiguration.AuthPluginClassName) && !string.IsNullOrWhiteSpace(controlledConfiguration.AuthParamsString))
                        {
                            authentication = AuthenticationFactory.Create(controlledConfiguration.AuthPluginClassName, controlledConfiguration.AuthParamsString);
                        }
                        var tlsTrustCertsFilePath = controlledConfiguration.TlsTrustCertsFilePath;
                        var ServiceUrl = controlledConfiguration.ServiceUrl;
                        if (authentication != null)
                        {
                            pulsarClient.UpdateAuthentication(authentication);
                        }
                        if (!string.IsNullOrWhiteSpace(tlsTrustCertsFilePath))
                        {
                            pulsarClient.UpdateTlsTrustCertsFilePath(tlsTrustCertsFilePath);
                        }
                        pulsarClient.UpdateServiceUrl(ServiceUrl);
                        _currentPulsarServiceUrl = ServiceUrl;
                        currentControlledConfiguration = controlledConfiguration;
                    }
                }
                catch (Exception e)
                {
                    _log.Error($"Failed to switch new Pulsar url, current: {currentControlledConfiguration}, new: {controlledConfiguration} => {e}");
                }
            });
        }

        protected internal virtual ControlledConfiguration FetchControlledConfiguration()
        {
            // call the service to get service URL
            try
            {
                var response = httpClient.GetAsync("").GetAwaiter().GetResult();
                var statusCode = ((int)response.StatusCode);
                if (statusCode == 200)
                {
                    var content = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    return JsonSerializer.Deserialize<ControlledConfiguration>(content);
                }
                _log.Warning($"Failed to fetch controlled configuration, status code: {statusCode}");
            }
            catch (Exception e)
            {
                _log.Error($"Failed to fetch controlled configuration: {e}");
            }

            return null;
        }

        protected internal class ControlledConfiguration
        {
            internal string ServiceUrl;
            internal string TlsTrustCertsFilePath;

            internal string AuthPluginClassName;
            internal string AuthParamsString;

            public virtual string ToJson()
            {
                try
                {
                    return JsonSerializer.Serialize(this);
                }
                catch (Exception E)
                {
                    Context.System.Log.Warning("Failed to write as json. ", E);
                    return null;
                }
            }
        }

        public virtual string ServiceUrl
        {
            get
            {
                return _currentPulsarServiceUrl;
            }
        }
        protected override void PostStop()
        {
            executor.Cancel();
            if (httpClient != null)
            {
                try
                {
                    httpClient.Dispose();
                }
                catch (Exception ex)
                {
                    _log.Error($"Failed to close http client.{ex}");
                }
            }
            base.PostStop();
        }

        public static ControlledClusterFailoverBuilder Builder()
        {
            return new ControlledClusterFailoverBuilder();
        }
    }

}