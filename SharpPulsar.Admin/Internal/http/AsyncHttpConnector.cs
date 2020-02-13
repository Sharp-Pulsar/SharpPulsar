using System;
using System.Collections.Generic;
using System.IO;

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
namespace Org.Apache.Pulsar.Client.Admin.@internal.Http
{
	using HttpRequest = io.netty.handler.codec.http.HttpRequest;
	using HttpResponse = io.netty.handler.codec.http.HttpResponse;
	using SslContext = io.netty.handler.ssl.SslContext;



	using Getter = lombok.Getter;
	using SneakyThrows = lombok.SneakyThrows;
	using Slf4j = lombok.@extern.slf4j.Slf4j;

	using StringUtils = org.apache.commons.lang3.StringUtils;
	using AuthenticationDataProvider = Org.Apache.Pulsar.Client.Api.AuthenticationDataProvider;
	using PulsarServiceNameResolver = Org.Apache.Pulsar.Client.Impl.PulsarServiceNameResolver;
	using ClientConfigurationData = Org.Apache.Pulsar.Client.Impl.Conf.ClientConfigurationData;
	using SecurityUtility = Org.Apache.Pulsar.Common.Util.SecurityUtility;
	using AsyncCompletionHandler = org.asynchttpclient.AsyncCompletionHandler;
	using AsyncHttpClient = org.asynchttpclient.AsyncHttpClient;
	using BoundRequestBuilder = org.asynchttpclient.BoundRequestBuilder;
	using DefaultAsyncHttpClient = org.asynchttpclient.DefaultAsyncHttpClient;
	using DefaultAsyncHttpClientConfig = org.asynchttpclient.DefaultAsyncHttpClientConfig;
	using Request = org.asynchttpclient.Request;
	using Response = org.asynchttpclient.Response;
	using DefaultKeepAliveStrategy = org.asynchttpclient.channel.DefaultKeepAliveStrategy;
	using ClientProperties = org.glassfish.jersey.client.ClientProperties;
	using ClientRequest = org.glassfish.jersey.client.ClientRequest;
	using ClientResponse = org.glassfish.jersey.client.ClientResponse;
	using AsyncConnectorCallback = org.glassfish.jersey.client.spi.AsyncConnectorCallback;
	using Connector = org.glassfish.jersey.client.spi.Connector;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Slf4j public class AsyncHttpConnector implements org.glassfish.jersey.client.spi.Connector
	public class AsyncHttpConnector : Connector
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Getter private final org.asynchttpclient.AsyncHttpClient httpClient;
		private readonly AsyncHttpClient httpClient;
		private readonly PulsarServiceNameResolver serviceNameResolver;

		public AsyncHttpConnector(Client Client, ClientConfigurationData Conf) : this((int) Client.Configuration.getProperty(ClientProperties.CONNECT_TIMEOUT), (int) Client.Configuration.getProperty(ClientProperties.READ_TIMEOUT), PulsarAdmin.DEFAULT_REQUEST_TIMEOUT_SECONDS * 1000, Conf)
		{
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SneakyThrows public AsyncHttpConnector(int connectTimeoutMs, int readTimeoutMs, int requestTimeoutMs, org.apache.pulsar.client.impl.conf.ClientConfigurationData conf)
		public AsyncHttpConnector(int ConnectTimeoutMs, int ReadTimeoutMs, int RequestTimeoutMs, ClientConfigurationData Conf)
		{
			DefaultAsyncHttpClientConfig.Builder ConfBuilder = new DefaultAsyncHttpClientConfig.Builder();
			ConfBuilder.FollowRedirect = true;
			ConfBuilder.RequestTimeout = Conf.RequestTimeoutMs;
			ConfBuilder.ConnectTimeout = ConnectTimeoutMs;
			ConfBuilder.ReadTimeout = ReadTimeoutMs;
			ConfBuilder.UserAgent = string.Format("Pulsar-Java-v{0}", PulsarVersion.Version);
			ConfBuilder.RequestTimeout = RequestTimeoutMs;
			ConfBuilder.KeepAliveStrategy = new DefaultKeepAliveStrategyAnonymousInnerClass(this);

			serviceNameResolver = new PulsarServiceNameResolver();
			if (Conf != null && StringUtils.isNotBlank(Conf.ServiceUrl))
			{
				serviceNameResolver.UpdateServiceUrl(Conf.ServiceUrl);
				if (Conf.ServiceUrl.StartsWith("https://"))
				{

					SslContext SslCtx = null;

					// Set client key and certificate if available
					AuthenticationDataProvider AuthData = Conf.Authentication.AuthData;
					if (AuthData.hasDataForTls())
					{
						SslCtx = SecurityUtility.createNettySslContextForClient(Conf.TlsAllowInsecureConnection || !Conf.TlsHostnameVerificationEnable, Conf.TlsTrustCertsFilePath, AuthData.TlsCertificates, AuthData.TlsPrivateKey);
					}
					else
					{
						SslCtx = SecurityUtility.createNettySslContextForClient(Conf.TlsAllowInsecureConnection || !Conf.TlsHostnameVerificationEnable, Conf.TlsTrustCertsFilePath);
					}

					ConfBuilder.SslContext = SslCtx;
				}
			}
			httpClient = new DefaultAsyncHttpClient(ConfBuilder.build());
		}

		public class DefaultKeepAliveStrategyAnonymousInnerClass : DefaultKeepAliveStrategy
		{
			private readonly AsyncHttpConnector outerInstance;

			public DefaultKeepAliveStrategyAnonymousInnerClass(AsyncHttpConnector OuterInstance)
			{
				this.outerInstance = OuterInstance;
			}

			public override bool keepAlive(Request AhcRequest, HttpRequest Request, HttpResponse Response)
			{
				// Close connection upon a server error or per HTTP spec
				return (Response.status().code() / 100 != 5) && base.keepAlive(AhcRequest, Request, Response);
			}
		}

		public virtual ClientResponse Apply(ClientRequest JerseyRequest)
		{

			CompletableFuture<ClientResponse> Future = new CompletableFuture<ClientResponse>();
			long StartTime = DateTimeHelper.CurrentUnixTimeMillis();
			Exception LastException = null;
			ISet<InetSocketAddress> TriedAddresses = new HashSet<InetSocketAddress>();

			while (true)
			{
				InetSocketAddress Address = serviceNameResolver.ResolveHost();
				if (TriedAddresses.Contains(Address))
				{
					// We already tried all available addresses
					throw new ProcessingException((LastException.Message), LastException);
				}

				TriedAddresses.Add(Address);
				URI RequestUri = ReplaceWithNew(Address, JerseyRequest.Uri);
				JerseyRequest.Uri = RequestUri;
				CompletableFuture<ClientResponse> TempFuture = new CompletableFuture<ClientResponse>();
				try
				{
					ResolveRequest(TempFuture, JerseyRequest);
					if (DateTimeHelper.CurrentUnixTimeMillis() - StartTime > httpClient.Config.RequestTimeout)
					{
						throw new ProcessingException("Request timeout, the last try service url is : " + JerseyRequest.Uri.ToString());
					}
				}
				catch (ExecutionException Ex)
				{
					Exception E = Ex.InnerException == null ? Ex : Ex.InnerException;
					if (DateTimeHelper.CurrentUnixTimeMillis() - StartTime > httpClient.Config.RequestTimeout)
					{
						throw new ProcessingException((E.Message), E);
					}
					LastException = E;
					continue;
				}
				catch (Exception E)
				{
					if (DateTimeHelper.CurrentUnixTimeMillis() - StartTime > httpClient.Config.RequestTimeout)
					{
						throw new ProcessingException(E.Message, E);
					}
					LastException = E;
					continue;
				}
				Future = TempFuture;
				break;
			}

			return Future.join();
		}

		private URI ReplaceWithNew(InetSocketAddress Address, URI Uri)
		{
			string OriginalUri = Uri.ToString();
			string NewUri = (OriginalUri.Split(":", true)[0] + "://") + Address.HostName + ":" + Address.Port + Uri.RawPath;
			if (Uri.RawQuery != null)
			{
				NewUri += "?" + Uri.RawQuery;
			}
			return URI.create(NewUri);
		}



//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void resolveRequest(java.util.concurrent.CompletableFuture<org.glassfish.jersey.client.ClientResponse> future, org.glassfish.jersey.client.ClientRequest jerseyRequest) throws InterruptedException, java.util.concurrent.ExecutionException, java.util.concurrent.TimeoutException
		private void ResolveRequest(CompletableFuture<ClientResponse> Future, ClientRequest JerseyRequest)
		{
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.concurrent.Future<?> resultFuture = apply(jerseyRequest, new org.glassfish.jersey.client.spi.AsyncConnectorCallback()
			Future<object> ResultFuture = Apply(JerseyRequest, new AsyncConnectorCallbackAnonymousInnerClass(this, Future));

			int? Timeout = httpClient.Config.RequestTimeout / 3;

			object Result = null;
			if (Timeout != null && Timeout > 0)
			{
				Result = ResultFuture.get(Timeout, TimeUnit.MILLISECONDS);
			}
			else
			{
				Result = ResultFuture.get();
			}

			if (Result != null && Result is Exception)
			{
				throw new ExecutionException((Exception) Result);
			}
		}

		public class AsyncConnectorCallbackAnonymousInnerClass : AsyncConnectorCallback
		{
			private readonly AsyncHttpConnector outerInstance;

			private CompletableFuture<ClientResponse> future;

			public AsyncConnectorCallbackAnonymousInnerClass(AsyncHttpConnector OuterInstance, CompletableFuture<ClientResponse> Future)
			{
				this.outerInstance = OuterInstance;
				this.future = Future;
			}

			public override void response(ClientResponse Response)
			{
				future.complete(Response);
			}
			public override void failure(Exception Failure)
			{
				future.completeExceptionally(Failure);
			}
		}

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: @Override public java.util.concurrent.Future<?> apply(org.glassfish.jersey.client.ClientRequest jerseyRequest, org.glassfish.jersey.client.spi.AsyncConnectorCallback callback)
		public override Future<object> Apply(ClientRequest JerseyRequest, AsyncConnectorCallback Callback)
		{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.CompletableFuture<Object> future = new java.util.concurrent.CompletableFuture<>();
			CompletableFuture<object> Future = new CompletableFuture<object>();

			BoundRequestBuilder Builder = httpClient.prepare(JerseyRequest.Method, JerseyRequest.Uri.ToString());

			if (JerseyRequest.hasEntity())
			{
				MemoryStream OutStream = new MemoryStream();
				JerseyRequest.StreamProvider = contentLength => OutStream;
				try
				{
					JerseyRequest.writeEntity();
				}
				catch (IOException E)
				{
					Future.completeExceptionally(E);
					return Future;
				}

				Builder.Body = OutStream.toByteArray();
			}

			JerseyRequest.Headers.forEach((key, headers) =>
			{
			if (!HttpHeaders.USER_AGENT.Equals(key))
			{
				Builder.addHeader(key, headers);
			}
			});

			Builder.execute(new AsyncCompletionHandlerAnonymousInnerClass(this, JerseyRequest, Callback, Future));

			return Future;
		}

		public class AsyncCompletionHandlerAnonymousInnerClass : AsyncCompletionHandler<Response>
		{
			private readonly AsyncHttpConnector outerInstance;

			private ClientRequest jerseyRequest;
			private AsyncConnectorCallback callback;
			private CompletableFuture<object> future;

			public AsyncCompletionHandlerAnonymousInnerClass(AsyncHttpConnector OuterInstance, ClientRequest JerseyRequest, AsyncConnectorCallback Callback, CompletableFuture<object> Future)
			{
				this.outerInstance = OuterInstance;
				this.jerseyRequest = JerseyRequest;
				this.callback = Callback;
				this.future = Future;
			}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public org.asynchttpclient.Response onCompleted(org.asynchttpclient.Response response) throws Exception
			public override Response onCompleted(Response Response)
			{
				ClientResponse JerseyResponse = new ClientResponse(Status.fromStatusCode(Response.StatusCode), jerseyRequest);
				Response.Headers.forEach(e => JerseyResponse.header(e.Key, e.Value));
				if (Response.hasResponseBody())
				{
					JerseyResponse.EntityStream = Response.ResponseBodyAsStream;
				}
				callback.response(JerseyResponse);
				future.complete(JerseyResponse);
				return Response;
			}

			public override void onThrowable(Exception T)
			{
				callback.failure(T);
				future.completeExceptionally(T);
			}
		}

		public override string Name
		{
			get
			{
				return "Pulsar-Admin";
			}
		}

		public override void Close()
		{
			try
			{
				httpClient.close();
			}
			catch (IOException E)
			{
				log.warn("Failed to close http client", E);
			}
		}

	}

}