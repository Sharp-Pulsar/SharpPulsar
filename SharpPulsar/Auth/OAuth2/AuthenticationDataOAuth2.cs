using System;
using System.Collections.Generic;
using SharpPulsar.Interfaces;

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
namespace SharpPulsar.Auth.OAuth2
{
	/// <summary>
	/// Provide OAuth 2.0 authentication data.
	/// </summary>
	[Serializable]
	internal class AuthenticationDataOAuth2 : IAuthenticationDataProvider
	{
		public const string HttpHeaderName = "Authorization";

		private readonly string _accessToken;
		private readonly ISet<KeyValuePair<string, string>> _headers = new HashSet<KeyValuePair<string, string>>() ;

		public AuthenticationDataOAuth2(string accessToken)
		{
			_accessToken = accessToken;
			_headers.Add(new KeyValuePair<string, string>(HttpHeaderName, "Bearer " + accessToken));
		}

		public virtual bool HasDataForHttp()
		{
			return true;
		}

		public virtual ISet<KeyValuePair<string, string>> HttpHeaders
		{
			get
			{
				return _headers;
			}
		}

		public virtual bool HasDataFromCommand()
		{
			return true;
		}

		public virtual string CommandData
		{
			get
			{
				return _accessToken;
			}
		}

	}

}