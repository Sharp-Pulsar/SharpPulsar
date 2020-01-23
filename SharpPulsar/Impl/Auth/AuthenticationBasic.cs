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

namespace SharpPulsar.Impl.Auth
{
	using Gson = com.google.gson.Gson;
	using JsonObject = com.google.gson.JsonObject;
	using Authentication = SharpPulsar.Api.Authentication;
	using AuthenticationDataProvider = SharpPulsar.Api.AuthenticationDataProvider;
	using EncodedAuthenticationParameterSupport = SharpPulsar.Api.EncodedAuthenticationParameterSupport;
	using PulsarClientException = SharpPulsar.Api.PulsarClientException;


	[Serializable]
	public class AuthenticationBasic : Authentication, EncodedAuthenticationParameterSupport
	{
		private string userId;
		private string password;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void close() throws java.io.IOException
		public override void Close()
		{
			// noop
		}

		public virtual string AuthMethodName
		{
			get
			{
				return "basic";
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public SharpPulsar.api.AuthenticationDataProvider getAuthData() throws SharpPulsar.api.PulsarClientException
		public virtual AuthenticationDataProvider AuthData
		{
			get
			{
				try
				{
					return new AuthenticationDataBasic(userId, password);
				}
				catch (Exception E)
				{
					throw PulsarClientException.unwrap(E);
				}
			}
		}

		public override void Configure(IDictionary<string, string> AuthParams)
		{
			configure((new Gson()).toJson(AuthParams));
		}

		public override void Configure(string EncodedAuthParamString)
		{
			JsonObject Params = (new Gson()).fromJson(EncodedAuthParamString, typeof(JsonObject));
			userId = Params.get("userId").AsString;
			password = Params.get("password").AsString;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void start() throws SharpPulsar.api.PulsarClientException
		public override void Start()
		{
			// noop
		}

	}

}