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
namespace org.apache.pulsar.client.admin.@internal
{


	using ConflictException = PulsarAdminException.ConflictException;
	using ConnectException = PulsarAdminException.ConnectException;
	using GettingAuthenticationDataException = PulsarAdminException.GettingAuthenticationDataException;
	using NotAllowedException = PulsarAdminException.NotAllowedException;
	using NotAuthorizedException = PulsarAdminException.NotAuthorizedException;
	using NotFoundException = PulsarAdminException.NotFoundException;
	using PreconditionFailedException = PulsarAdminException.PreconditionFailedException;
	using ServerSideErrorException = PulsarAdminException.ServerSideErrorException;
	using Authentication = client.api.Authentication;
	using AuthenticationDataProvider = client.api.AuthenticationDataProvider;
	using PulsarClientException = client.api.PulsarClientException;
	using ErrorData = pulsar.common.policies.data.ErrorData;
	using FutureUtil = pulsar.common.util.FutureUtil;
	using Logger = org.slf4j.Logger;
	using LoggerFactory = org.slf4j.LoggerFactory;

	public abstract class BaseResource
	{
		private static readonly Logger log = LoggerFactory.getLogger(typeof(BaseResource));

		protected internal readonly Authentication auth;
		protected internal readonly long readTimeoutMs;

		protected internal BaseResource(Authentication auth, long readTimeoutMs)
		{
			this.auth = auth;
			this.readTimeoutMs = readTimeoutMs;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public javax.ws.rs.client.Invocation.Builder request(final javax.ws.rs.client.WebTarget target) throws org.apache.pulsar.client.admin.PulsarAdminException
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
		public virtual Builder request(WebTarget target)
		{
			try
			{
				return requestAsync(target).get();
			}
			catch (Exception e)
			{
				throw new GettingAuthenticationDataException(e);
			}
		}

		// do the authentication stage, and once authentication completed return a Builder
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public java.util.concurrent.CompletableFuture<javax.ws.rs.client.Invocation.Builder> requestAsync(final javax.ws.rs.client.WebTarget target)
		public virtual CompletableFuture<Builder> requestAsync(WebTarget target)
		{
			CompletableFuture<Builder> builderFuture = new CompletableFuture<Builder>();
			CompletableFuture<IDictionary<string, string>> authFuture = new CompletableFuture<IDictionary<string, string>>();
			try
			{
				AuthenticationDataProvider authData = auth.getAuthData(target.Uri.Host);

				if (authData.hasDataForHttp())
				{
					auth.authenticationStage(target.Uri.ToString(), authData, null, authFuture);
				}
				else
				{
					authFuture.complete(null);
				}

				// auth complete, return a new Builder
				authFuture.whenComplete((respHeaders, ex) =>
				{
				if (ex != null)
				{
					log.warn("[{}] Failed to perform http request at authn stage: {}", ex.Message);
					builderFuture.completeExceptionally(new PulsarClientException(ex));
					return;
				}
				try
				{
					Builder builder = target.request(MediaType.APPLICATION_JSON);
					if (authData.hasDataForHttp())
					{
						ISet<Entry<string, string>> headers = auth.newRequestHeader(target.Uri.ToString(), authData, respHeaders);
						if (headers != null)
						{
							headers.forEach(entry => builder.header(entry.Key, entry.Value));
						}
					}
					builderFuture.complete(builder);
				}
				catch (Exception t)
				{
					builderFuture.completeExceptionally(new GettingAuthenticationDataException(t));
				}
				});
			}
			catch (Exception t)
			{
				builderFuture.completeExceptionally(new GettingAuthenticationDataException(t));
			}

			return builderFuture;
		}

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public <T> java.util.concurrent.CompletableFuture<Void> asyncPutRequest(final javax.ws.rs.client.WebTarget target, javax.ws.rs.client.Entity<T> entity)
		public virtual CompletableFuture<Void> asyncPutRequest<T>(WebTarget target, Entity<T> entity)
		{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.CompletableFuture<Void> future = new java.util.concurrent.CompletableFuture<>();
			CompletableFuture<Void> future = new CompletableFuture<Void>();
			try
			{
				request(target).async().put(entity, new InvocationCallbackAnonymousInnerClass(this, target, future));
			}
			catch (PulsarAdminException cae)
			{
				future.completeExceptionally(cae);
			}
			return future;
		}

		private class InvocationCallbackAnonymousInnerClass : InvocationCallback<ErrorData>
		{
			private readonly BaseResource outerInstance;

			private WebTarget target;
			private CompletableFuture<Void> future;

			public InvocationCallbackAnonymousInnerClass(BaseResource outerInstance, WebTarget target, CompletableFuture<Void> future)
			{
				this.outerInstance = outerInstance;
				this.target = target;
				this.future = future;
			}


			public override void completed(ErrorData response)
			{
				future.complete(null);
			}

			public override void failed(Exception throwable)
			{
				log.warn("[{}] Failed to perform http put request: {}", target.Uri, throwable.Message);
				future.completeExceptionally(outerInstance.getApiException(throwable.InnerException));
			}

		}

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public <T> java.util.concurrent.CompletableFuture<Void> asyncPostRequest(final javax.ws.rs.client.WebTarget target, javax.ws.rs.client.Entity<T> entity)
		public virtual CompletableFuture<Void> asyncPostRequest<T>(WebTarget target, Entity<T> entity)
		{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.CompletableFuture<Void> future = new java.util.concurrent.CompletableFuture<>();
			CompletableFuture<Void> future = new CompletableFuture<Void>();
			try
			{
				request(target).async().post(entity, new InvocationCallbackAnonymousInnerClass2(this, target, future));
			}
			catch (PulsarAdminException cae)
			{
				future.completeExceptionally(cae);
			}
			return future;
		}

		private class InvocationCallbackAnonymousInnerClass2 : InvocationCallback<ErrorData>
		{
			private readonly BaseResource outerInstance;

			private WebTarget target;
			private CompletableFuture<Void> future;

			public InvocationCallbackAnonymousInnerClass2(BaseResource outerInstance, WebTarget target, CompletableFuture<Void> future)
			{
				this.outerInstance = outerInstance;
				this.target = target;
				this.future = future;
			}


			public override void completed(ErrorData response)
			{
				future.complete(null);
			}

			public override void failed(Exception throwable)
			{
				log.warn("[{}] Failed to perform http post request: {}", target.Uri, throwable.Message);
				future.completeExceptionally(outerInstance.getApiException(throwable.InnerException));
			}

		}

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public <T> java.util.concurrent.Future<T> asyncGetRequest(final javax.ws.rs.client.WebTarget target, javax.ws.rs.client.InvocationCallback<T> callback)
		public virtual Future<T> asyncGetRequest<T>(WebTarget target, InvocationCallback<T> callback)
		{
			try
			{
				return request(target).async().get(callback);
			}
			catch (PulsarAdminException cae)
			{
				return FutureUtil.failedFuture(cae);
			}
		}

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public java.util.concurrent.CompletableFuture<Void> asyncDeleteRequest(final javax.ws.rs.client.WebTarget target)
		public virtual CompletableFuture<Void> asyncDeleteRequest(WebTarget target)
		{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.CompletableFuture<Void> future = new java.util.concurrent.CompletableFuture<>();
			CompletableFuture<Void> future = new CompletableFuture<Void>();
			try
			{
				request(target).async().delete(new InvocationCallbackAnonymousInnerClass3(this, target, future));
			}
			catch (PulsarAdminException cae)
			{
				future.completeExceptionally(cae);
			}
			return future;
		}

		private class InvocationCallbackAnonymousInnerClass3 : InvocationCallback<ErrorData>
		{
			private readonly BaseResource outerInstance;

			private WebTarget target;
			private CompletableFuture<Void> future;

			public InvocationCallbackAnonymousInnerClass3(BaseResource outerInstance, WebTarget target, CompletableFuture<Void> future)
			{
				this.outerInstance = outerInstance;
				this.target = target;
				this.future = future;
			}


			public override void completed(ErrorData response)
			{
				future.complete(null);
			}

			public override void failed(Exception throwable)
			{
				log.warn("[{}] Failed to perform http delete request: {}", target.Uri, throwable.Message);
				future.completeExceptionally(outerInstance.getApiException(throwable.InnerException));
			}
		}

		public virtual PulsarAdminException getApiException(Exception e)
		{
			if (e is ServiceUnavailableException)
			{
				if (e.InnerException is java.net.ConnectException)
				{
					return new ConnectException(e.InnerException);
				}
				else
				{
					return new PulsarAdminException((ServerErrorException) e);
				}
			}
			else if (e is WebApplicationException)
			{
				// Handle 5xx exceptions
				if (e is ServerErrorException)
				{
					ServerErrorException see = (ServerErrorException) e;
					return new ServerSideErrorException(see, e.Message);
				}
				else if (e is ClientErrorException)
				{
					// Handle 4xx exceptions
					ClientErrorException cee = (ClientErrorException) e;
					int statusCode = cee.Response.Status;
					switch (statusCode)
					{
						case 401:
						case 403:
							return new NotAuthorizedException(cee);
						case 404:
							return new NotFoundException(cee);
						case 405:
							return new NotAllowedException(cee);
						case 409:
							return new ConflictException(cee);
						case 412:
							return new PreconditionFailedException(cee);
						default:
							return new PulsarAdminException(cee);
					}
				}
				else
				{
					return new PulsarAdminException((WebApplicationException) e);
				}
			}
			else
			{
				return new PulsarAdminException(e);
			}
		}

		public virtual WebApplicationException getApiException(Response response)
		{
			if (response.StatusInfo.Equals(Response.Status.OK))
			{
				return null;
			}
			if (response.Status >= 500)
			{
				throw new ServerErrorException(response);
			}
			else if (response.Status >= 400)
			{
				throw new ClientErrorException(response);
			}
			else
			{
				throw new WebApplicationException(response);
			}
		}
	}

}