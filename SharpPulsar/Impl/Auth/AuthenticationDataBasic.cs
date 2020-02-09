using SharpPulsar.Api;
using System;
using System.Collections.Generic;
using System.Text;

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
	public class AuthenticationDataBasic : IAuthenticationDataProvider
	{
		private const string HttpHeaderName = "Authorization";
		private readonly string _httpAuthToken;

        public AuthenticationDataBasic(string userId, string password)
		{
			_httpAuthToken = "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(userId + ":" + password));
			CommandData = userId + ":" + password;
		}

		public bool HasDataForHttp()
		{
			return true;
		}

		public ISet<KeyValuePair<string, string>> HttpHeaders
		{
			get
			{
				IDictionary<string, string> headers = new Dictionary<string, string>
				{
					[HttpHeaderName] = _httpAuthToken
				};
				return new HashSet<KeyValuePair<string, string>>(headers);
			}
		}

		public bool HasDataFromCommand()
		{
			return true;
		}

		public string CommandData { get; }
    }

}