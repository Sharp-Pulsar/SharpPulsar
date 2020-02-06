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
namespace org.apache.pulsar.client.admin.@internal.http
{
	using HttpRequest = io.netty.handler.codec.http.HttpRequest;
	using HttpResponse = io.netty.handler.codec.http.HttpResponse;
	using SslContext = io.netty.handler.ssl.SslContext;



	using Getter = lombok.Getter;
	using SneakyThrows = lombok.SneakyThrows;
	using Slf4j = lombok.@extern.slf4j.Slf4j;

	using StringUtils = apache.commons.lang3.StringUtils;
	using AuthenticationDataProvider = client.api.AuthenticationDataProvider;
	using PulsarServiceNameResolver = client.impl.PulsarServiceNameResolver;
	using ClientConfigurationData = client.impl.conf.ClientConfigurationData;
	using SecurityUtility = pulsar.common.util.SecurityUtility;
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

		public AsyncHttpConnector(Client client, ClientConfigurationData conf) : this((int) client.Configuration.getProperty(ClientProperties.CONNECT_TIMEOUT), (int) client.Configuration.getProperty(ClientProperties.READ_TIMEOUT), PulsarAdmin.DEFAULT_REQUEST_TIMEOUT_SECONDS * 1000, conf)
		{
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SneakyThrows public AsyncHttpConnector(int connectTimeoutMs, int readTimeoutMs, int requestTimeoutMs, org.apache.pulsar.client.impl.conf.ClientConfigurationData conf)
		public AsyncHttpConnector(int connectTimeoutMs, int readTimeoutMs, int requestTimeoutMs, ClientConfigurationData conf)
		{
			DefaultAsyncHttpClientConfig.Builder confBuilder = new DefaultAsyncHttpClientConfig.Builder();
			confBuilder.FollowRedirect = true;
			confBuilder.RequestTimeout = conf.RequestTimeoutMs;
			confBuilder.ConnectTimeout = connectTimeoutMs;
			confBuilder.ReadTimeout = readTimeoutMs;
			confBuilder.UserAgent = string.Format("Pulsar-Java-v{0}", PulsarVersion.Version);
			confBuilder.RequestTimeout = requestTimeoutMs;
			confBuilder.KeepAliveStrategy = new DefaultKeepAliveStrategyAnonymousInnerClass(this);

			serviceNameResolver = new PulsarServiceNameResolver();
			if (conf != null && StringUtils.isNotBlank(conf.ServiceUrl))
			{
				serviceNameResolver.updateServiceUrl(conf.ServiceUrl);
				if (conf.ServiceUrl.StartsWith("https://"))
				{

					SslContext sslCtx = null;

					// Set client key and certificate if available
					AuthenticationDataProvider authData = conf.Authentication.AuthData;
					if (authData.hasDataForTls())
					{
						sslCtx = SecurityUtility.createNettySslContextForClient(conf.TlsAllowInsecureConnection || !conf.TlsHostnameVerificationEnable, conf.TlsTrustCertsFilePath, authData.TlsCertificates, authData.TlsPrivateKey);
					}
					else
					{
						sslCtx = SecurityUtility.createNettySslContextForClient(conf.TlsAllowInsecureConnection || !conf.TlsHostnameVerificationEnable, conf.TlsTrustCertsFilePath);
					}

					confBuilder.SslContext = sslCtx;
				}
			}
			httpClient = new DefaultAsyncHttpClient(confBuilder.build());
		}

		private class DefaultKeepAliveStrategyAnonymousInnerClass : DefaultKeepAliveStrategy
		{
			private readonly AsyncHttpConnector outerInstance;

			public DefaultKeepAliveStrategyAnonymousInnerClass(AsyncHttpConnector outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public override bool keepAlive(Request ahcRequest, HttpRequest request, HttpResponse response)
			{
				// Close connection upon a server error or per HTTP spec
				return (response.status().code() / 100 != 5) && base.keepAlive(ahcRequest, request, response);
			}
		}

		public virtual ClientResponse apply(ClientRequest jerseyRequest)
		{

			CompletableFuture<ClientResponse> future = new CompletableFuture<ClientResponse>();
			long startTime = DateTimeHelper.CurrentUnixTimeMillis();
			Exception lastException = null;
			ISet<InetSocketAddress> triedAddresses = new HashSet<InetSocketAddress>();

			while (true)
			{
				InetSocketAddress address = serviceNameResolver.resolveHost();
				if (triedAddresses.Contains(address))
				{
					// We already tried all available addresses
					throw new ProcessingException((lastException.Message), lastException);
				}

				triedAddresses.Add(address);
				URI requestUri = replaceWithNew(address, jerseyRequest.Uri);
				jerseyRequest.Uri = requestUri;
				CompletableFuture<ClientResponse> tempFuture = new CompletableFuture<ClientResponse>();
				try
				{
					resolveRequest(tempFuture, jerseyRequest);
					if (DateTimeHelper.CurrentUnixTimeMillis() - startTime > httpClient.Config.RequestTimeout)
					{
						throw new ProcessingException("Request timeout, the last try service url is : " + jerseyRequest.Uri.ToString());
					}
				}
				catch (ExecutionException ex)
				{
					Exception e = ex.InnerException == null ? ex : ex.InnerException;
					if (DateTimeHelper.CurrentUnixTimeMillis() - startTime > httpClient.Config.RequestTimeout)
					{
						throw new ProcessingException((e.Message), e);
					}
					lastException = e;
					continue;
				}
				catch (Exception e)
				{
					if (DateTimeHelper.CurrentUnixTimeMillis() - startTime > httpClient.Config.RequestTimeout)
					{
						throw new ProcessingException(e.Message, e);
					}
					lastException = e;
					continue;
				}
				future = tempFuture;
				break;
			}

			return future.join();
		}

		private URI replaceWithNew(InetSocketAddress address, URI uri)
		{
			string originalUri = uri.ToString();
			string newUri = (originalUri.Split(":", true)[0] + "://") + address.HostName + ":" + address.Port + uri.RawPath;
			if (uri.RawQuery != null)
			{
				newUri += "?" + uri.RawQuery;
			}
			return URI.create(newUri);
		}



//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void resolveRequest(java.util.concurrent.CompletableFuture<org.glassfish.jersey.client.ClientResponse> future, org.glassfish.jersey.client.ClientRequest jerseyRequest) throws InterruptedException, java.util.concurrent.ExecutionException, java.util.concurrent.TimeoutException
		private void resolveRequest(CompletableFuture<ClientResponse> future, ClientRequest jerseyRequest)
		{
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.concurrent.Future<?> resultFuture = apply(jerseyRequest, new org.glassfish.jersey.client.spi.AsyncConnectorCallback()
			Future<object> resultFuture = apply(jerseyRequest, new AsyncConnectorCallbackAnonymousInnerClass(this, future));

			int? timeout = httpClient.Config.RequestTimeout / 3;

			object result = null;
			if (timeout != null && timeout > 0)
			{
				result = resultFuture.get(timeout, TimeUnit.MILLISECONDS);
			}
			else
			{
				result = resultFuture.get();
			}

			if (result != null && result is Exception)
			{
				throw new ExecutionException((Exception) result);
			}
		}

		private class AsyncConnectorCallbackAnonymousInnerClass : AsyncConnectorCallback
		{
			private readonly AsyncHttpConnector outerInstance;

			private CompletableFuture<ClientResponse> future;

			public AsyncConnectorCallbackAnonymousInnerClass(AsyncHttpConnector outerInstance, CompletableFuture<ClientResponse> future)
			{
				this.outerInstance = outerInstance;
				this.future = future;
			}

			public override void response(ClientResponse response)
			{
				future.complete(response);
			}
			public override void failure(Exception failure)
			{
				future.completeExceptionally(failure);
			}
		}

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: @Override public java.util.concurrent.Future<?> apply(org.glassfish.jersey.client.ClientRequest jerseyRequest, org.glassfish.jersey.client.spi.AsyncConnectorCallback callback)
		public override Future<object> apply(ClientRequest jerseyRequest, AsyncConnectorCallback callback)
		{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.CompletableFuture<Object> future = new java.util.concurrent.CompletableFuture<>();
			CompletableFuture<object> future = new CompletableFuture<object>();

			BoundRequestBuilder builder = httpClient.prepare(jerseyRequest.Method, jerseyRequest.Uri.ToString());

			if (jerseyRequest.hasEntity())
			{
				MemoryStream outStream = new MemoryStream();
				jerseyRequest.StreamProvider = contentLength => outStream;
				try
				{
					jerseyRequest.writeEntity();
				}
				catch (IOException e)
				{
					future.completeExceptionally(e);
					return future;
				}

				builder.Body = outStream.toByteArray();
			}

			jerseyRequest.Headers.forEach((key, headers) =>
			{
			if (!HttpHeaders.USER_AGENT.Equals(key))
			{
				builder.addHeader(key, headers);
			}
			});

			builder.execute(new AsyncCompletionHandlerAnonymousInnerClass(this, jerseyRequest, callback, future));

			return future;
		}

		private class AsyncCompletionHandlerAnonymousInnerClass : AsyncCompletionHandler<Response>
		{
			private readonly AsyncHttpConnector outerInstance;

			private ClientRequest jerseyRequest;
			private AsyncConnectorCallback callback;
			private CompletableFuture<object> future;

			public AsyncCompletionHandlerAnonymousInnerClass(AsyncHttpConnector outerInstance, ClientRequest jerseyRequest, AsyncConnectorCallback callback, CompletableFuture<object> future)
			{
				this.outerInstance = outerInstance;
				this.jerseyRequest = jerseyRequest;
				this.callback = callback;
				this.future = future;
			}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public org.asynchttpclient.Response onCompleted(org.asynchttpclient.Response response) throws Exception
			public override Response onCompleted(Response response)
			{
				ClientResponse jerseyResponse = new ClientResponse(Status.fromStatusCode(response.StatusCode), jerseyRequest);
				response.Headers.forEach(e => jerseyResponse.header(e.Key, e.Value));
				if (response.hasResponseBody())
				{
					jerseyResponse.EntityStream = response.ResponseBodyAsStream;
				}
				callback.response(jerseyResponse);
				future.complete(jerseyResponse);
				return response;
			}

			public override void onThrowable(Exception t)
			{
				callback.failure(t);
				future.completeExceptionally(t);
			}
		}

		public override string Name
		{
			get
			{
				return "Pulsar-Admin";
			}
		}

		public override void close()
		{
			try
			{
				httpClient.close();
			}
			catch (IOException e)
			{
				log.warn("Failed to close http client", e);
			}
		}

	}

}