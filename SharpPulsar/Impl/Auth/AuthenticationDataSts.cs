using SharpPulsar.Api;
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

namespace SharpPulsar.Impl.Auth
{

	public class AuthenticationDataSts : IAuthenticationDataProvider
	{
        private readonly Func<string> _tokenSupplier;
        private string _token;

		public AuthenticationDataSts(Func<string> tokenSupplier)
		{
			this._tokenSupplier = tokenSupplier;
		}

        public AuthenticationDataSts(string token)
        {
            _token = token;
        }
		
		public bool HasDataFromCommand()
		{
			return true;
		}

		public string CommandData => string.IsNullOrWhiteSpace(_token)? Token: _token;

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