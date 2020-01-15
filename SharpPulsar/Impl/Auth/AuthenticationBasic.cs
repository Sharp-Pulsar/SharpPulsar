using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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

namespace Pulsar.Client.Impl.Auth
{
	using Gson = com.google.gson.Gson;
	using JsonObject = com.google.gson.JsonObject;
	using Authentication = Api.Authentication;
	using AuthenticationDataProvider = Api.AuthenticationDataProvider;
	using EncodedAuthenticationParameterSupport = Api.EncodedAuthenticationParameterSupport;
	using PulsarClientException = Api.PulsarClientException;


	public class AuthenticationBasic : Authentication, EncodedAuthenticationParameterSupport
	{
		private string userId;
		private string password;

		public string AuthMethodName
		{
			get
			{
				return "basic";
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public org.apache.pulsar.client.api.AuthenticationDataProvider getAuthData() throws org.apache.pulsar.client.api.PulsarClientException
		public  AuthenticationDataProvider AuthData
		{
			get
			{
				try
				{
					return new AuthenticationDataBasic(userId, password);
				}
				catch (Exception e)
				{
					throw PulsarClientException.Unwrap(e);
				}
			}
		}

		public void Configure(IDictionary<string, string> authParams)
		{
			Configure((new Gson()).toJson(authParams));
		}

		public void Configure(string encodedAuthParamString)
		{
			JsonObject @params = (new Gson()).fromJson(encodedAuthParamString, typeof(JsonObject));
			userId = @params.get("userId").AsString;
			password = @params.get("password").AsString;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void start() throws org.apache.pulsar.client.api.PulsarClientException
		
		public void Start()
		{
			throw new NotImplementedException();
		}

		public ValueTask DisposeAsync()
		{
			throw new NotImplementedException();
		}
	}

}