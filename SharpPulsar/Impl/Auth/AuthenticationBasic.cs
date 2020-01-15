using Pulsar.Client.Impl.Auth;
using SharpPulsar.Exception;
using SharpPulsar.Interface;
using SharpPulsar.Interface.Auth;
using System;
using System.Collections.Generic;
using System.Text.Json;
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

namespace SharpPulsar.Impl.Auth
{
	public class AuthenticationBasic : IAuthentication, IEncodedAuthenticationParameterSupport
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

		public  IAuthenticationDataProvider AuthData
		{
			get
			{
				try
				{
					return new AuthenticationDataBasic(userId, password);
				}
				catch (System.Exception e)
				{
					throw PulsarClientException.Unwrap(e);
				}
			}
		}

		public void Configure(IDictionary<string, string> authParams)
		{
			var jsonString = JsonSerializer.Serialize(authParams);
			Configure(jsonString);
		}

		public void Configure(string encodedAuthParamString)
		{
			var authParams = JsonSerializer.Deserialize<IDictionary<string, string>>(encodedAuthParamString);
			userId = authParams["userId"];
			password = authParams["password"];
		}

		
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