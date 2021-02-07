using SharpPulsar.Api;
using SharpPulsar.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;

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

	public class AuthenticationDataToken : IAuthenticationDataProvider
	{
		public const string HttpHeaderName = "Authorization";

		private readonly Func<string> _tokenSupplier;

		public AuthenticationDataToken(Func<string> tokenSupplier)
		{
			this._tokenSupplier = tokenSupplier;
		}

		public bool HasDataForHttp()
		{
			return true;
		}

		public ISet<KeyValuePair<string, string>> HttpHeaders
		{
			get
			{
				var set = new HashSet<KeyValuePair<string, string>>
				{
					new KeyValuePair<string, string>(HttpHeaderName, "Bearer " + Token)
				};
				return set;
			}
		}

		public bool HasDataFromCommand()
		{
			return true;
		}

		public string CommandData => Token;

        private string Token
		{
			get
			{
				try
				{
					return _tokenSupplier.Invoke();
				}
				catch (Exception t)
				{
					throw new IOException("failed to get client token", t);
				}
			}
		}
	}

}