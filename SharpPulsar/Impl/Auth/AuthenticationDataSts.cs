using SharpPulsar.Api;
using System;
using System.IO;
using System.Net.Http;
using IdentityModel.Client;

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
        private string _clientId;
        private string _clientSecret;
        private string _authority;
		public AuthenticationDataSts(string clientid, string secret, string authority)
        {
            _clientId = clientid;
            _clientSecret = secret;
            _authority = authority;
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
                    var client = new HttpClient();

                    var disco = client.GetDiscoveryDocumentAsync(_authority).GetAwaiter().GetResult();
                    if (disco.IsError) throw new Exception(disco.Error);

                    var response = client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
                    {
                        Address = disco.TokenEndpoint,

                        ClientId = _clientId,
                        ClientSecret = _clientSecret,
                    }).GetAwaiter().GetResult();

                    if (response.IsError) throw new Exception(response.Error);
                    return response.AccessToken;
				}
				catch (Exception t)
				{
					throw new IOException("failed to get client token", t);
				}
			}
		}
	}

}