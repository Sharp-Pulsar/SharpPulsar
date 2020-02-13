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
namespace Org.Apache.Pulsar.Client.Admin.@internal
{


	using ConflictException = Org.Apache.Pulsar.Client.Admin.PulsarAdminException.ConflictException;
	using ConnectException = Org.Apache.Pulsar.Client.Admin.PulsarAdminException.ConnectException;
	using GettingAuthenticationDataException = Org.Apache.Pulsar.Client.Admin.PulsarAdminException.GettingAuthenticationDataException;
	using NotAllowedException = Org.Apache.Pulsar.Client.Admin.PulsarAdminException.NotAllowedException;
	using NotAuthorizedException = Org.Apache.Pulsar.Client.Admin.PulsarAdminException.NotAuthorizedException;
	using NotFoundException = Org.Apache.Pulsar.Client.Admin.PulsarAdminException.NotFoundException;
	using PreconditionFailedException = Org.Apache.Pulsar.Client.Admin.PulsarAdminException.PreconditionFailedException;
	using ServerSideErrorException = Org.Apache.Pulsar.Client.Admin.PulsarAdminException.ServerSideErrorException;
	using Authentication = Org.Apache.Pulsar.Client.Api.Authentication;
	using AuthenticationDataProvider = Org.Apache.Pulsar.Client.Api.AuthenticationDataProvider;
	using PulsarClientException = Org.Apache.Pulsar.Client.Api.PulsarClientException;
	using ErrorData = Org.Apache.Pulsar.Common.Policies.Data.ErrorData;
	using FutureUtil = Org.Apache.Pulsar.Common.Util.FutureUtil;
	using Logger = org.slf4j.Logger;
	using LoggerFactory = org.slf4j.LoggerFactory;

	public abstract class BaseResource
	{
		private static readonly Logger log = LoggerFactory.getLogger(typeof(BaseResource));

		protected internal readonly Authentication Auth;
		protected internal readonly long ReadTimeoutMs;

		public BaseResource(Authentication Auth, long ReadTimeoutMs)
		{
			this.Auth = Auth;
			this.ReadTimeoutMs = ReadTimeoutMs;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public javax.ws.rs.client.Invocation.Builder request(final javax.ws.rs.client.WebTarget target) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual Builder Request(in WebTarget Target)
		{
			try
			{
				return RequestAsync(Target).get();
			}
			catch (Exception E)
			{
				throw new PulsarAdminException.GettingAuthenticationDataException(E);
			}
		}

		// do the authentication stage, and once authentication completed return a Builder
		public virtual CompletableFuture<Builder> RequestAsync(in WebTarget Target)
		{
			CompletableFuture<Builder> BuilderFuture = new CompletableFuture<Builder>();
			CompletableFuture<IDictionary<string, string>> AuthFuture = new CompletableFuture<IDictionary<string, string>>();
			try
			{
				AuthenticationDataProvider AuthData = Auth.getAuthData(Target.Uri.Host);

				if (AuthData.hasDataForHttp())
				{
					Auth.authenticationStage(Target.Uri.ToString(), AuthData, null, AuthFuture);
				}
				else
				{
					AuthFuture.complete(null);
				}

				// auth complete, return a new Builder
				AuthFuture.whenComplete((respHeaders, ex) =>
				{
				if (ex != null)
				{
					log.warn("[{}] Failed to perform http request at authn stage: {}", ex.Message);
					BuilderFuture.completeExceptionally(new PulsarClientException(ex));
					return;
				}
				try
				{
					Builder Builder = Target.request(MediaType.APPLICATION_JSON);
					if (AuthData.hasDataForHttp())
					{
						ISet<Entry<string, string>> Headers = Auth.newRequestHeader(Target.Uri.ToString(), AuthData, respHeaders);
						if (Headers != null)
						{
							Headers.forEach(entry => Builder.header(entry.Key, entry.Value));
						}
					}
					BuilderFuture.complete(Builder);
				}
				catch (Exception T)
				{
					BuilderFuture.completeExceptionally(new GettingAuthenticationDataException(T));
				}
				});
			}
			catch (Exception T)
			{
				BuilderFuture.completeExceptionally(new PulsarAdminException.GettingAuthenticationDataException(T));
			}

			return BuilderFuture;
		}

		public virtual CompletableFuture<Void> AsyncPutRequest<T>(in WebTarget Target, Entity<T> Entity)
		{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.CompletableFuture<Void> future = new java.util.concurrent.CompletableFuture<>();
			CompletableFuture<Void> Future = new CompletableFuture<Void>();
			try
			{
				Request(Target).async().put(Entity, new InvocationCallbackAnonymousInnerClass(this, Target, Future));
			}
			catch (PulsarAdminException Cae)
			{
				Future.completeExceptionally(Cae);
			}
			return Future;
		}

		public class InvocationCallbackAnonymousInnerClass : InvocationCallback<ErrorData>
		{
			private readonly BaseResource outerInstance;

			private WebTarget target;
			private CompletableFuture<Void> future;

			public InvocationCallbackAnonymousInnerClass(BaseResource OuterInstance, WebTarget Target, CompletableFuture<Void> Future)
			{
				this.outerInstance = OuterInstance;
				this.target = Target;
				this.future = Future;
			}


			public override void completed(ErrorData Response)
			{
				future.complete(null);
			}

			public override void failed(Exception Throwable)
			{
				log.warn("[{}] Failed to perform http put request: {}", target.Uri, Throwable.Message);
				future.completeExceptionally(outerInstance.GetApiException(Throwable.InnerException));
			}

		}

		public virtual CompletableFuture<Void> AsyncPostRequest<T>(in WebTarget Target, Entity<T> Entity)
		{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.CompletableFuture<Void> future = new java.util.concurrent.CompletableFuture<>();
			CompletableFuture<Void> Future = new CompletableFuture<Void>();
			try
			{
				Request(Target).async().post(Entity, new InvocationCallbackAnonymousInnerClass2(this, Target, Future));
			}
			catch (PulsarAdminException Cae)
			{
				Future.completeExceptionally(Cae);
			}
			return Future;
		}

		public class InvocationCallbackAnonymousInnerClass2 : InvocationCallback<ErrorData>
		{
			private readonly BaseResource outerInstance;

			private WebTarget target;
			private CompletableFuture<Void> future;

			public InvocationCallbackAnonymousInnerClass2(BaseResource OuterInstance, WebTarget Target, CompletableFuture<Void> Future)
			{
				this.outerInstance = OuterInstance;
				this.target = Target;
				this.future = Future;
			}


			public override void completed(ErrorData Response)
			{
				future.complete(null);
			}

			public override void failed(Exception Throwable)
			{
				log.warn("[{}] Failed to perform http post request: {}", target.Uri, Throwable.Message);
				future.completeExceptionally(outerInstance.GetApiException(Throwable.InnerException));
			}

		}

		public virtual Future<T> AsyncGetRequest<T>(in WebTarget Target, InvocationCallback<T> Callback)
		{
			try
			{
				return Request(Target).async().get(Callback);
			}
			catch (PulsarAdminException Cae)
			{
				return FutureUtil.failedFuture(Cae);
			}
		}

		public virtual CompletableFuture<Void> AsyncDeleteRequest(in WebTarget Target)
		{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.CompletableFuture<Void> future = new java.util.concurrent.CompletableFuture<>();
			CompletableFuture<Void> Future = new CompletableFuture<Void>();
			try
			{
				Request(Target).async().delete(new InvocationCallbackAnonymousInnerClass3(this, Target, Future));
			}
			catch (PulsarAdminException Cae)
			{
				Future.completeExceptionally(Cae);
			}
			return Future;
		}

		public class InvocationCallbackAnonymousInnerClass3 : InvocationCallback<ErrorData>
		{
			private readonly BaseResource outerInstance;

			private WebTarget target;
			private CompletableFuture<Void> future;

			public InvocationCallbackAnonymousInnerClass3(BaseResource OuterInstance, WebTarget Target, CompletableFuture<Void> Future)
			{
				this.outerInstance = OuterInstance;
				this.target = Target;
				this.future = Future;
			}


			public override void completed(ErrorData Response)
			{
				future.complete(null);
			}

			public override void failed(Exception Throwable)
			{
				log.warn("[{}] Failed to perform http delete request: {}", target.Uri, Throwable.Message);
				future.completeExceptionally(outerInstance.GetApiException(Throwable.InnerException));
			}
		}

		public virtual PulsarAdminException GetApiException(Exception E)
		{
			if (E is ServiceUnavailableException)
			{
				if (E.InnerException is java.net.ConnectException)
				{
					return new PulsarAdminException.ConnectException(E.InnerException);
				}
				else
				{
					return new PulsarAdminException((ServerErrorException) E);
				}
			}
			else if (E is WebApplicationException)
			{
				// Handle 5xx exceptions
				if (E is ServerErrorException)
				{
					ServerErrorException See = (ServerErrorException) E;
					return new PulsarAdminException.ServerSideErrorException(See, E.Message);
				}
				else if (E is ClientErrorException)
				{
					// Handle 4xx exceptions
					ClientErrorException Cee = (ClientErrorException) E;
					int StatusCode = Cee.Response.Status;
					switch (StatusCode)
					{
						case 401:
						case 403:
							return new PulsarAdminException.NotAuthorizedException(Cee);
						case 404:
							return new PulsarAdminException.NotFoundException(Cee);
						case 405:
							return new PulsarAdminException.NotAllowedException(Cee);
						case 409:
							return new PulsarAdminException.ConflictException(Cee);
						case 412:
							return new PulsarAdminException.PreconditionFailedException(Cee);
						default:
							return new PulsarAdminException(Cee);
					}
				}
				else
				{
					return new PulsarAdminException((WebApplicationException) E);
				}
			}
			else
			{
				return new PulsarAdminException(E);
			}
		}

		public virtual WebApplicationException GetApiException(Response Response)
		{
			if (Response.StatusInfo.Equals(Response.Status.OK))
			{
				return null;
			}
			if (Response.Status >= 500)
			{
				throw new ServerErrorException(Response);
			}
			else if (Response.Status >= 400)
			{
				throw new ClientErrorException(Response);
			}
			else
			{
				throw new WebApplicationException(Response);
			}
		}
	}

}