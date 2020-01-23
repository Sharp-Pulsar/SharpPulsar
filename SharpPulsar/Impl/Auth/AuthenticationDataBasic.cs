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
	using AuthenticationDataProvider = SharpPulsar.Api.AuthenticationDataProvider;


	[Serializable]
	public class AuthenticationDataBasic : AuthenticationDataProvider
	{
		private const string HttpHeaderName = "Authorization";
		private string httpAuthToken;
		public virtual CommandData {get;}

		public AuthenticationDataBasic(string UserId, string Password)
		{
			httpAuthToken = "Basic " + Base64.Encoder.encodeToString((UserId + ":" + Password).GetBytes());
			CommandData = UserId + ":" + Password;
		}

		public override bool HasDataForHttp()
		{
			return true;
		}

		public virtual ISet<KeyValuePair<string, string>> HttpHeaders
		{
			get
			{
				IDictionary<string, string> Headers = new Dictionary<string, string>();
				Headers[HttpHeaderName] = httpAuthToken;
				return Headers.SetOfKeyValuePairs();
			}
		}

		public override bool HasDataFromCommand()
		{
			return true;
		}

	}

}