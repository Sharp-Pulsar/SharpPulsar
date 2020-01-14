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

namespace org.apache.pulsar.client.impl.auth
{
	using AuthenticationDataProvider = org.apache.pulsar.client.api.AuthenticationDataProvider;


	public class AuthenticationDataBasic : AuthenticationDataProvider
	{
		private const string HTTP_HEADER_NAME = "Authorization";
		private string httpAuthToken;
		private string commandAuthToken;

		public AuthenticationDataBasic(string userId, string password)
		{
			httpAuthToken = "Basic " + Base64.Encoder.encodeToString((userId + ":" + password).GetBytes());
			commandAuthToken = userId + ":" + password;
		}

		public override bool hasDataForHttp()
		{
			return true;
		}

		public override ISet<KeyValuePair<string, string>> HttpHeaders
		{
			get
			{
				IDictionary<string, string> headers = new Dictionary<string, string>();
				headers[HTTP_HEADER_NAME] = httpAuthToken;
				return headers.SetOfKeyValuePairs();
			}
		}

		public override bool hasDataFromCommand()
		{
			return true;
		}

		public override string CommandData
		{
			get
			{
				return commandAuthToken;
			}
		}
	}

}