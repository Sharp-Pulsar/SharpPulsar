
using SharpPulsar.Interfaces;
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
namespace SharpPulsar.Auth
{

	public class AuthenticationDisabled : IAuthentication, IEncodedAuthenticationParameterSupport
	{

		protected internal readonly IAuthenticationDataProvider nullData = new AuthenticationDataNull();

        public AuthenticationDisabled()
		{
		}

		public string AuthMethodName => "none";

        public IAuthenticationDataProvider GetAuthData()
        {
            return nullData;
        }

        public void Configure(string encodedAuthParamString)
		{
		}

		public void Start()
		{
		}

		public ValueTask DisposeAsync()
		{
			throw new NotImplementedException();
		}

		public void Configure(IDictionary<string, string> AuthParams)
		{
			throw new NotImplementedException();
		}

		public void Dispose()
		{
			throw new NotImplementedException();
		}
	}

}