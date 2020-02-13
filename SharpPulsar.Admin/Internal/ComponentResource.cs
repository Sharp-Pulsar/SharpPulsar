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
	using Authentication = Org.Apache.Pulsar.Client.Api.Authentication;
	using AuthenticationDataProvider = Org.Apache.Pulsar.Client.Api.AuthenticationDataProvider;
	using SaslConstants = Org.Apache.Pulsar.Common.Sasl.SaslConstants;
	using RequestBuilder = org.asynchttpclient.RequestBuilder;


	public class ComponentResource : BaseResource
	{

		public ComponentResource(Authentication Auth, long ReadTimeoutMs) : base(Auth, ReadTimeoutMs)
		{
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.asynchttpclient.RequestBuilder addAuthHeaders(javax.ws.rs.client.WebTarget target, org.asynchttpclient.RequestBuilder requestBuilder) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual RequestBuilder AddAuthHeaders(WebTarget Target, RequestBuilder RequestBuilder)
		{

			try
			{
				if (Auth != null)
				{
					ISet<KeyValuePair<string, string>> Headers = GetAuthHeaders(Target);
					if (Headers != null && Headers.Count > 0)
					{
						Headers.forEach(header => RequestBuilder.addHeader(header.Key, header.Value));
					}
				}
				return RequestBuilder;
			}
			catch (Exception T)
			{
				throw new PulsarAdminException.GettingAuthenticationDataException(T);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private java.util.Set<java.util.Map.Entry<String, String>> getAuthHeaders(javax.ws.rs.client.WebTarget target) throws Exception
		private ISet<KeyValuePair<string, string>> GetAuthHeaders(WebTarget Target)
		{
			AuthenticationDataProvider AuthData = Auth.getAuthData(Target.Uri.Host);
			string TargetUrl = Target.Uri.ToString();
			if (Auth.AuthMethodName.Equals(SaslConstants.AUTH_METHOD_NAME, StringComparison.OrdinalIgnoreCase))
			{
				CompletableFuture<IDictionary<string, string>> AuthFuture = new CompletableFuture<IDictionary<string, string>>();
				Auth.authenticationStage(TargetUrl, AuthData, null, AuthFuture);
				return Auth.newRequestHeader(TargetUrl, AuthData, AuthFuture.get());
			}
			else if (AuthData.hasDataForHttp())
			{
				return Auth.newRequestHeader(TargetUrl, AuthData, null);
			}
			else
			{
				return null;
			}
		}
	}

}