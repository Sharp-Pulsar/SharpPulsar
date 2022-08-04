using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using SharpPulsar.Auth.OAuth2.Protocol;
using SharpPulsar.Exceptions;

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
	/// Implementation of OAuth 2.0 Client Credentials flow.
	/// </summary>
	/// <seealso cref="<a href="https://tools.ietf.org/html/rfc6749.section-4.4">OAuth 2.0 RFC 6749, section 4.4</a>"/>
    /// 
	[Serializable]
	internal class ClientCredentialsFlow : FlowBase
	{
		public const string ConfigParamIssuerUrl = "issuerUrl";
		public const string ConfigParamAudience = "audience";
		public const string ConfigParamKeyFile = "privateKey";
		public const string ConfigParamScope = "scope";

		private const long SerialVersionUID = 1L;

		private readonly string _audience;
		private readonly string _privateKey;
		private readonly string _scope;

		[NonSerialized]
		private IClientCredentialsExchanger _exchanger;

		private bool initialized = false;

		public ClientCredentialsFlow(Uri issuerUrl, string audience, string privateKey, string scope) : base(issuerUrl)
		{
			_audience = audience;
			_privateKey = privateKey;
			_scope = scope;
		}

		public override void Initialize()
		{
			base.Initialize();
			Debug.Assert(Metadata != null);

			var tokenUrl = Metadata.TokenEndpoint;
			_exchanger = new TokenClient(tokenUrl);
			initialized = true;
		}

		public override TokenResult Authenticate()
		{
			// read the private key from storage
			KeyFile keyFile;
			try
			{
				keyFile = LoadPrivateKey(_privateKey);
			}
			catch (IOException e)
			{
				throw new PulsarClientException.AuthenticationException("Unable to read private key: " + e.Message);
			}

			// request an access token using client credentials
			var req = new ClientCredentialsExchangeRequest 
            {
                ClientId = keyFile.ClientId,
                ClientSecret = keyFile.ClientSecret,
                Audience = _audience,
                Scope = _scope

            };
			TokenResult tr;
			if (!initialized)
			{
				Initialize();
			}
			try
			{
				tr = _exchanger.ExchangeClientCredentials(req).Result;
			}
			catch (Exception e) when (e is TokenExchangeException || e is IOException)
			{
				throw new PulsarClientException.AuthenticationException("Unable to obtain an access token: " + e.Message);
			}

			return tr;
		}

		public override void Close()
		{
			//_exchanger.Close();
		}

		/// <summary>
		/// Constructs a <seealso cref="ClientCredentialsFlow"/> from configuration parameters. </summary>
		/// <param name="params">
		/// @return </param>
		public static ClientCredentialsFlow FromParameters(IDictionary<string, string> prams)
		{
			var issuerUrl = ParseParameterUrl(prams, ConfigParamIssuerUrl);
			var privateKeyUrl = ParseParameterString(prams, ConfigParamKeyFile);
			// These are optional parameters, so we only perform a get
			var scope = prams[ConfigParamScope];
			var audience = prams[ConfigParamAudience];
            return new ClientCredentialsFlow(issuerUrl, audience, privateKeyUrl,scope);
		}

		/// <summary>
		/// Loads the private key from the given URL. </summary>
		/// <param name="privateKeyURL">
		/// @return </param>
		/// <exception cref="IOException"> </exception>
        /// 
		private static KeyFile LoadPrivateKey(string privateKeyURL)
		{
			try
			{
                var uri = new Uri(privateKeyURL);
                var fs = new FileStream(privateKeyURL, FileMode.Open, FileAccess.Read);
                var temp = JsonSerializer.DeserializeAsync<KeyFile>(fs).Result;
                return temp;
			}
			catch (Exception e)
			{
				throw new IOException("Invalid privateKey format", e);
			}
		}
	}

}