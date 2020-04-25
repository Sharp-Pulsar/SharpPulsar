
using SharpPulsar.Api;
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

namespace SharpPulsar.Impl.Auth
{

	/// <summary>
	/// STS Token based authentication provider.
	/// </summary>
	public class AuthenticationSts : IAuthentication, IEncodedAuthenticationParameterSupport
	{

        private string _clientId;
        private string _clientSecret;
        private string _authority;
        public AuthenticationSts(string clientid, string secret, string authority)
        {
            _clientId = clientid;
            _clientSecret = secret;
            _authority = authority;
        }

		public void Close()
		{
			// noop
		}

		public string AuthMethodName => "sts";

		public IAuthenticationDataProvider AuthData => new AuthenticationDataSts(_clientId, _clientSecret, _authority);

		public void Configure(string encodedAuthParamString)
		{
			// Interpret the whole param string as the token. If the string contains the notation `token:xxxxx` then strip
			// the prefix
            var authP = encodedAuthParamString.Split(",");
            _clientId = authP[0];
            _clientSecret = authP[1];
            _authority = authP[2];
        }

		public void Configure(IDictionary<string, string> authParams)
		{
			// noop
		}

		public void Start()
		{
			// noop
		}

		public ValueTask DisposeAsync()
		{
			Dispose();
			return new ValueTask(Task.CompletedTask);
		}

		public void Dispose()
		{
		}
	}

}