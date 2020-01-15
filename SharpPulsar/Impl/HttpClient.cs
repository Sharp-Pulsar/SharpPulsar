using System;
using System.Collections.Generic;

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
namespace org.apache.pulsar.client.impl
{

	using EventLoopGroup = io.netty.channel.EventLoopGroup;
	using HttpRequest = io.netty.handler.codec.http.HttpRequest;
	using HttpResponse = io.netty.handler.codec.http.HttpResponse;
	using SslContext = io.netty.handler.ssl.SslContext;
	using Slf4j = lombok.@extern.slf4j.Slf4j;
	using Authentication = org.apache.pulsar.client.api.Authentication;
	using AuthenticationDataProvider = org.apache.pulsar.client.api.AuthenticationDataProvider;
	using PulsarClientException = org.apache.pulsar.client.api.PulsarClientException;
	using NotFoundException = org.apache.pulsar.client.api.PulsarClientException.NotFoundException;
	using ObjectMapperFactory = org.apache.pulsar.common.util.ObjectMapperFactory;
	using SecurityUtility = org.apache.pulsar.common.util.SecurityUtility;
	using AsyncHttpClient = org.asynchttpclient.AsyncHttpClient;
	using AsyncHttpClientConfig = org.asynchttpclient.AsyncHttpClientConfig;
	using BoundRequestBuilder = org.asynchttpclient.BoundRequestBuilder;
	using DefaultAsyncHttpClient = org.asynchttpclient.DefaultAsyncHttpClient;
	using DefaultAsyncHttpClientConfig = org.asynchttpclient.DefaultAsyncHttpClientConfig;
	using Request = org.asynchttpclient.Request;
	using DefaultKeepAliveStrategy = org.asynchttpclient.channel.DefaultKeepAliveStrategy;


//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Slf4j public class HttpClient implements java.io.Closeable
	public class HttpClient : System.IDisposable
	{

		protected internal const int DEFAULT_CONNECT_TIMEOUT_IN_SECONDS = 10;
		protected internal const int DEFAULT_READ_TIMEOUT_IN_SECONDS = 30;

		protected internal readonly AsyncHttpClient httpClient;
		protected internal readonly ServiceNameResolver serviceNameResolver;
		protected internal readonly Authentication authentication;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected HttpClient(String serviceUrl, org.apache.pulsar.client.api.Authentication authentication, io.netty.channel.EventLoopGroup eventLoopGroup, boolean tlsAllowInsecureConnection, String tlsTrustCertsFilePath) throws org.apache.pulsar.client.api.PulsarClientException
		protected internal HttpClient(string serviceUrl, Authentication authentication, EventLoopGroup eventLoopGroup, bool tlsAllowInsecureConnection, string tlsTrustCertsFilePath) : this(serviceUrl, authentication, eventLoopGroup, tlsAllowInsecureConnection, tlsTrustCertsFilePath, DEFAULT_CONNECT_TIMEOUT_IN_SECONDS, DEFAULT_READ_TIMEOUT_IN_SECONDS)
		{
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected HttpClient(String serviceUrl, org.apache.pulsar.client.api.Authentication authentication, io.netty.channel.EventLoopGroup eventLoopGroup, boolean tlsAllowInsecureConnection, String tlsTrustCertsFilePath, int connectTimeoutInSeconds, int readTimeoutInSeconds) throws org.apache.pulsar.client.api.PulsarClientException
		protected internal HttpClient(string serviceUrl, Authentication authentication, EventLoopGroup eventLoopGroup, bool tlsAllowInsecureConnection, string tlsTrustCertsFilePath, int connectTimeoutInSeconds, int readTimeoutInSeconds)
		{
			this.authentication = authentication;
			this.serviceNameResolver = new PulsarServiceNameResolver();
			this.serviceNameResolver.updateServiceUrl(serviceUrl);

			DefaultAsyncHttpClientConfig.Builder confBuilder = new DefaultAsyncHttpClientConfig.Builder();
			confBuilder.FollowRedirect = true;
			confBuilder.ConnectTimeout = connectTimeoutInSeconds * 1000;
			confBuilder.ReadTimeout = readTimeoutInSeconds * 1000;
			confBuilder.UserAgent = string.Format("Pulsar-Java-v{0}", PulsarVersion.Version);
			confBuilder.KeepAliveStrategy = new DefaultKeepAliveStrategyAnonymousInnerClass(this);

			if ("https".Equals(serviceNameResolver.ServiceUri.ServiceName))
			{
				try
				{
					SslContext sslCtx = null;

					// Set client key and certificate if available
					AuthenticationDataProvider authData = authentication.AuthData;
					if (authData.hasDataForTls())
					{
						sslCtx = SecurityUtility.createNettySslContextForClient(tlsAllowInsecureConnection, tlsTrustCertsFilePath, authData.TlsCertificates, authData.TlsPrivateKey);
					}
					else
					{
						sslCtx = SecurityUtility.createNettySslContextForClient(tlsAllowInsecureConnection, tlsTrustCertsFilePath);
					}

					confBuilder.SslContext = sslCtx;
					confBuilder.UseInsecureTrustManager = tlsAllowInsecureConnection;
				}
				catch (Exception e)
				{
					throw new PulsarClientException.InvalidConfigurationException(e);
				}
			}
			confBuilder.EventLoopGroup = eventLoopGroup;
			AsyncHttpClientConfig config = confBuilder.build();
			httpClient = new DefaultAsyncHttpClient(config);

			log.debug("Using HTTP url: {}", serviceUrl);
		}

		private class DefaultKeepAliveStrategyAnonymousInnerClass : DefaultKeepAliveStrategy
		{
			private readonly HttpClient outerInstance;

			public DefaultKeepAliveStrategyAnonymousInnerClass(HttpClient outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public override bool keepAlive(Request ahcRequest, HttpRequest request, HttpResponse response)
			{
				// Close connection upon a server error or per HTTP spec
				return (response.status().code() / 100 != 5) && base.keepAlive(ahcRequest, request, response);
			}
		}

		internal virtual string ServiceUrl
		{
			get
			{
				return this.serviceNameResolver.ServiceUrl;
			}
			set
			{
				this.serviceNameResolver.updateServiceUrl(value);
			}
		}


//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void close() throws java.io.IOException
		public virtual void Dispose()
		{
			httpClient.close();
		}

		public virtual CompletableFuture<T> get<T>(string path, Type clazz)
		{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.CompletableFuture<T> future = new java.util.concurrent.CompletableFuture<>();
			CompletableFuture<T> future = new CompletableFuture<T>();
			try
			{
				URI hostUri = serviceNameResolver.resolveHostUri();
				string requestUrl = (new URL(hostUri.toURL(), path)).ToString();
				string remoteHostName = hostUri.Host;
				AuthenticationDataProvider authData = authentication.getAuthData(remoteHostName);

				CompletableFuture<IDictionary<string, string>> authFuture = new CompletableFuture<IDictionary<string, string>>();

				// bring a authenticationStage for sasl auth.
				if (authData.hasDataForHttp())
				{
					authentication.authenticationStage(requestUrl, authData, null, authFuture);
				}
				else
				{
					authFuture.complete(null);
				}

				// auth complete, do real request
				authFuture.whenComplete((respHeaders, ex) =>
				{
				if (ex != null)
				{
					log.warn("[{}] Failed to perform http request at authentication stage: {}", requestUrl, ex.Message);
					future.completeExceptionally(new PulsarClientException(ex));
					return;
				}
				BoundRequestBuilder builder = httpClient.prepareGet(requestUrl).setHeader("Accept", "application/json");
				if (authData.hasDataForHttp())
				{
					ISet<Entry<string, string>> headers;
					try
					{
						headers = authentication.newRequestHeader(requestUrl, authData, respHeaders);
					}
					catch (Exception e)
					{
						log.warn("[{}] Error during HTTP get headers: {}", requestUrl, e.Message);
						future.completeExceptionally(new PulsarClientException(e));
						return;
					}
					if (headers != null)
					{
						headers.forEach(entry => builder.addHeader(entry.Key, entry.Value));
					}
				}
				builder.execute().toCompletableFuture().whenComplete((response2, t) =>
				{
					if (t != null)
					{
						log.warn("[{}] Failed to perform http request: {}", requestUrl, t.Message);
						future.completeExceptionally(new PulsarClientException(t));
						return;
					}
					if (response2.StatusCode != HttpURLConnection.HTTP_OK)
					{
						log.warn("[{}] HTTP get request failed: {}", requestUrl, response2.StatusText);
						Exception e;
						if (response2.StatusCode == HttpURLConnection.HTTP_NOT_FOUND)
						{
							e = new NotFoundException("Not found: " + response2.StatusText);
						}
						else
						{
							e = new PulsarClientException("HTTP get request failed: " + response2.StatusText);
						}
						future.completeExceptionally(e);
						return;
					}
					try
					{
						T data = ObjectMapperFactory.ThreadLocal.readValue(response2.ResponseBodyAsBytes, clazz);
						future.complete(data);
					}
					catch (Exception e)
					{
						log.warn("[{}] Error during HTTP get request: {}", requestUrl, e.Message);
						future.completeExceptionally(new PulsarClientException(e));
					}
				});
				});
			}
			catch (Exception e)
			{
				log.warn("[{}]PulsarClientImpl: {}", path, e.Message);
				if (e is PulsarClientException)
				{
					future.completeExceptionally(e);
				}
				else
				{
					future.completeExceptionally(new PulsarClientException(e));
				}
			}

			return future;
		}
	}

}