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

	using IAuthenticationDataProvider = SharpPulsar.Api.IAuthenticationDataProvider;

	[Serializable]
	public class AuthenticationDataToken : IAuthenticationDataProvider
	{
		public const string HttpHeaderName = "Authorization";

		private readonly System.Func<string> tokenSupplier;

		public AuthenticationDataToken(System.Func<string> TokenSupplier)
		{
			this.tokenSupplier = TokenSupplier;
		}

		public override bool HasDataForHttp()
		{
			return true;
		}

		public virtual ISet<KeyValuePair<string, string>> HttpHeaders
		{
			get
			{
				return Collections.singletonMap(HttpHeaderName, "Bearer " + Token).entrySet();
			}
		}

		public override bool HasDataFromCommand()
		{
			return true;
		}

		public virtual string CommandData
		{
			get
			{
				return Token;
			}
		}

		private string Token
		{
			get
			{
				try
				{
					return tokenSupplier.get();
				}
				catch (Exception T)
				{
					throw new Exception("failed to get client token", T);
				}
			}
		}
	}

}