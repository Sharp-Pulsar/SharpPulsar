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
namespace SharpPulsar.Impl
{

	using EventLoopGroup = io.netty.channel.EventLoopGroup;
	using HttpRequest = io.netty.handler.codec.http.HttpRequest;
	using HttpResponse = io.netty.handler.codec.http.HttpResponse;
	using SslContext = io.netty.handler.ssl.SslContext;
	using Slf4j = lombok.@extern.slf4j.Slf4j;
	using Authentication = SharpPulsar.Api.Authentication;
	using AuthenticationDataProvider = SharpPulsar.Api.AuthenticationDataProvider;
	using PulsarClientException = SharpPulsar.Api.PulsarClientException;
	using NotFoundException = SharpPulsar.Api.PulsarClientException.NotFoundException;
	using ObjectMapperFactory = Org.Apache.Pulsar.Common.Util.ObjectMapperFactory;
	using SecurityUtility = Org.Apache.Pulsar.Common.Util.SecurityUtility;
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

		protected internal const int DefaultConnectTimeoutInSeconds = 10;
		protected internal const int DefaultReadTimeoutInSeconds = 30;

//JAVA TO C# CONVERTER NOTE: Members cannot have the same name as their enclosing type:
		protected internal readonly AsyncHttpClient HttpClientConflict;
		protected internal readonly ServiceNameResolver ServiceNameResolver;
		protected internal readonly Authentication Authentication;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected HttpClient(String serviceUrl, SharpPulsar.api.Authentication authentication, io.netty.channel.EventLoopGroup eventLoopGroup, boolean tlsAllowInsecureConnection, String tlsTrustCertsFilePath) throws SharpPulsar.api.PulsarClientException
		public HttpClient(string ServiceUrl, Authentication Authentication, EventLoopGroup EventLoopGroup, bool TlsAllowInsecureConnection, string TlsTrustCertsFilePath) : this(ServiceUrl, Authentication, EventLoopGroup, TlsAllowInsecureConnection, TlsTrustCertsFilePath, DefaultConnectTimeoutInSeconds, DefaultReadTimeoutInSeconds)
		{
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected HttpClient(String serviceUrl, SharpPulsar.api.Authentication authentication, io.netty.channel.EventLoopGroup eventLoopGroup, boolean tlsAllowInsecureConnection, String tlsTrustCertsFilePath, int connectTimeoutInSeconds, int readTimeoutInSeconds) throws SharpPulsar.api.PulsarClientException
		public HttpClient(string ServiceUrl, Authentication Authentication, EventLoopGroup EventLoopGroup, bool TlsAllowInsecureConnection, string TlsTrustCertsFilePath, int ConnectTimeoutInSeconds, int ReadTimeoutInSeconds)
		{
			this.Authentication = Authentication;
			this.ServiceNameResolver = new PulsarServiceNameResolver();
			this.ServiceNameResolver.updateServiceUrl(ServiceUrl);

			DefaultAsyncHttpClientConfig.Builder ConfBuilder = new DefaultAsyncHttpClientConfig.Builder();
			ConfBuilder.FollowRedirect = true;
			ConfBuilder.ConnectTimeout = ConnectTimeoutInSeconds * 1000;
			ConfBuilder.ReadTimeout = ReadTimeoutInSeconds * 1000;
			ConfBuilder.UserAgent = string.Format("Pulsar-Java-v{0}", PulsarVersion.Version);
			ConfBuilder.KeepAliveStrategy = new DefaultKeepAliveStrategyAnonymousInnerClass(this);

			if ("https".Equals(ServiceNameResolver.ServiceUri.ServiceName))
			{
				try
				{
					SslContext SslCtx = null;

					// Set client key and certificate if available
					AuthenticationDataProvider AuthData = Authentication.AuthData;
					if (AuthData.hasDataForTls())
					{
						SslCtx = SecurityUtility.createNettySslContextForClient(TlsAllowInsecureConnection, TlsTrustCertsFilePath, AuthData.TlsCertificates, AuthData.TlsPrivateKey);
					}
					else
					{
						SslCtx = SecurityUtility.createNettySslContextForClient(TlsAllowInsecureConnection, TlsTrustCertsFilePath);
					}

					ConfBuilder.SslContext = SslCtx;
					ConfBuilder.UseInsecureTrustManager = TlsAllowInsecureConnection;
				}
				catch (Exception E)
				{
					throw new PulsarClientException.InvalidConfigurationException(E);
				}
			}
			ConfBuilder.EventLoopGroup = EventLoopGroup;
			AsyncHttpClientConfig Config = ConfBuilder.build();
			HttpClientConflict = new DefaultAsyncHttpClient(Config);

			log.debug("Using HTTP url: {}", ServiceUrl);
		}

		public class DefaultKeepAliveStrategyAnonymousInnerClass : DefaultKeepAliveStrategy
		{
			private readonly HttpClient outerInstance;

			public DefaultKeepAliveStrategyAnonymousInnerClass(HttpClient OuterInstance)
			{
				this.outerInstance = OuterInstance;
			}

			public override bool keepAlive(Request AhcRequest, HttpRequest Request, HttpResponse Response)
			{
				// Close connection upon a server error or per HTTP spec
				return (Response.status().code() / 100 != 5) && base.keepAlive(AhcRequest, Request, Response);
			}
		}

		public virtual string ServiceUrl
		{
			get
			{
				return this.ServiceNameResolver.ServiceUrl;
			}
			set
			{
				this.ServiceNameResolver.updateServiceUrl(value);
			}
		}


//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void close() throws java.io.IOException
		public override void Close()
		{
			HttpClientConflict.close();
		}

		public virtual CompletableFuture<T> Get<T>(string Path, Type Clazz)
		{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.CompletableFuture<T> future = new java.util.concurrent.CompletableFuture<>();
			CompletableFuture<T> Future = new CompletableFuture<T>();
			try
			{
				URI HostUri = ServiceNameResolver.resolveHostUri();
				string RequestUrl = (new URL(HostUri.toURL(), Path)).ToString();
				string RemoteHostName = HostUri.Host;
				AuthenticationDataProvider AuthData = Authentication.getAuthData(RemoteHostName);

				CompletableFuture<IDictionary<string, string>> AuthFuture = new CompletableFuture<IDictionary<string, string>>();

				// bring a authenticationStage for sasl auth.
				if (AuthData.hasDataForHttp())
				{
					Authentication.authenticationStage(RequestUrl, AuthData, null, AuthFuture);
				}
				else
				{
					AuthFuture.complete(null);
				}

				// auth complete, do real request
				AuthFuture.whenComplete((respHeaders, ex) =>
				{
				if (ex != null)
				{
					log.warn("[{}] Failed to perform http request at authentication stage: {}", RequestUrl, ex.Message);
					Future.completeExceptionally(new PulsarClientException(ex));
					return;
				}
				BoundRequestBuilder Builder = HttpClientConflict.prepareGet(RequestUrl).setHeader("Accept", "application/json");
				if (AuthData.hasDataForHttp())
				{
					ISet<Entry<string, string>> Headers;
					try
					{
						Headers = Authentication.newRequestHeader(RequestUrl, AuthData, respHeaders);
					}
					catch (Exception E)
					{
						log.warn("[{}] Error during HTTP get headers: {}", RequestUrl, E.Message);
						Future.completeExceptionally(new PulsarClientException(E));
						return;
					}
					if (Headers != null)
					{
						Headers.forEach(entry => Builder.addHeader(entry.Key, entry.Value));
					}
				}
				Builder.execute().toCompletableFuture().whenComplete((response2, t) =>
				{
					if (t != null)
					{
						log.warn("[{}] Failed to perform http request: {}", RequestUrl, t.Message);
						Future.completeExceptionally(new PulsarClientException(t));
						return;
					}
					if (response2.StatusCode != HttpURLConnection.HTTP_OK)
					{
						log.warn("[{}] HTTP get request failed: {}", RequestUrl, response2.StatusText);
						Exception E;
						if (response2.StatusCode == HttpURLConnection.HTTP_NOT_FOUND)
						{
							E = new NotFoundException("Not found: " + response2.StatusText);
						}
						else
						{
							E = new PulsarClientException("HTTP get request failed: " + response2.StatusText);
						}
						Future.completeExceptionally(E);
						return;
					}
					try
					{
						T Data = ObjectMapperFactory.ThreadLocal.readValue(response2.ResponseBodyAsBytes, Clazz);
						Future.complete(Data);
					}
					catch (Exception E)
					{
						log.warn("[{}] Error during HTTP get request: {}", RequestUrl, E.Message);
						Future.completeExceptionally(new PulsarClientException(E));
					}
				});
				});
			}
			catch (Exception E)
			{
				log.warn("[{}]PulsarClientImpl: {}", Path, E.Message);
				if (E is PulsarClientException)
				{
					Future.completeExceptionally(E);
				}
				else
				{
					Future.completeExceptionally(new PulsarClientException(E));
				}
			}

			return Future;
		}
	}

}