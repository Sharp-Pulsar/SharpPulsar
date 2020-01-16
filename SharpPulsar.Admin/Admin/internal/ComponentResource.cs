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
	using Authentication = org.apache.pulsar.client.api.Authentication;
	using AuthenticationDataProvider = org.apache.pulsar.client.api.AuthenticationDataProvider;
	using SaslConstants = org.apache.pulsar.common.sasl.SaslConstants;
	using RequestBuilder = org.asynchttpclient.RequestBuilder;


	public class ComponentResource : BaseResource
	{

		protected internal ComponentResource(Authentication auth, long readTimeoutMs) : base(auth, readTimeoutMs)
		{
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.asynchttpclient.RequestBuilder addAuthHeaders(javax.ws.rs.client.WebTarget target, org.asynchttpclient.RequestBuilder requestBuilder) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual RequestBuilder addAuthHeaders(WebTarget target, RequestBuilder requestBuilder)
		{

			try
			{
				if (auth != null)
				{
					ISet<KeyValuePair<string, string>> headers = getAuthHeaders(target);
					if (headers != null && headers.Count > 0)
					{
						headers.forEach(header => requestBuilder.addHeader(header.Key, header.Value));
					}
				}
				return requestBuilder;
			}
			catch (Exception t)
			{
				throw new PulsarAdminException.GettingAuthenticationDataException(t);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private java.util.Set<java.util.Map.Entry<String, String>> getAuthHeaders(javax.ws.rs.client.WebTarget target) throws Exception
		private ISet<KeyValuePair<string, string>> getAuthHeaders(WebTarget target)
		{
			AuthenticationDataProvider authData = auth.getAuthData(target.Uri.Host);
			string targetUrl = target.Uri.ToString();
			if (auth.AuthMethodName.equalsIgnoreCase(SaslConstants.AUTH_METHOD_NAME))
			{
				CompletableFuture<IDictionary<string, string>> authFuture = new CompletableFuture<IDictionary<string, string>>();
				auth.authenticationStage(targetUrl, authData, null, authFuture);
				return auth.newRequestHeader(targetUrl, authData, authFuture.get());
			}
			else if (authData.hasDataForHttp())
			{
				return auth.newRequestHeader(targetUrl, authData, null);
			}
			else
			{
				return null;
			}
		}
	}

}