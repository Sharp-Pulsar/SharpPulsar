using System;
using System.Collections.Generic;
using System.IO;
using NodaTime;
using SharpPulsar.Auth.OAuth2.Protocol;
using SharpPulsar.Extension;
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
	/// Pulsar client authentication provider based on OAuth 2.0.
	/// </summary>
	[Serializable]
	public class AuthenticationOAuth2 : IAuthentication, IEncodedAuthenticationParameterSupport
	{

		public const string ConfigParamType = "type";
		public const string TypeClientCredentials = "client_credentials";
		public const string AUTH_METHOD_NAME = "token";
		public const double ExpiryAdjustment = 0.9;
		private const long SerialVersionUID = 1L;

		internal readonly DateTime Date;
		internal IFlow Flow;
		[NonSerialized]
		internal CachedToken cachedToken;

		public AuthenticationOAuth2()
		{
			Date = DateTime.Now;
		}

		internal AuthenticationOAuth2(IFlow flow, DateTime date)
		{
			Flow = flow;
			Date = date;
		}

		public virtual string AuthMethodName
		{
			get
			{
				return AUTH_METHOD_NAME;
			}
		}

		public virtual void Configure(string encodedAuthParamString)
		{
			if (string.IsNullOrWhiteSpace(encodedAuthParamString))
			{
				throw new System.ArgumentException("No authentication parameters were provided");
			}
			IDictionary<string, string> prams;
			try
			{
				prams = AuthenticationUtil.ConfigureFromJsonString(encodedAuthParamString);
			}
			catch (IOException E)
			{
				throw new System.ArgumentException("Malformed authentication parameters", E);
			}

			string Type = prams.GetOrDefault(ConfigParamType, TypeClientCredentials);
			switch (Type)
			{
				case TypeClientCredentials:
					this.Flow = ClientCredentialsFlow.FromParameters(prams);
					break;
				default:
					throw new System.ArgumentException("Unsupported authentication type: " + Type);
			}
		}

		[Obsolete]
		public virtual void Configure(IDictionary<string, string> AuthParams)
		{
			throw new NotImplementedException("Deprecated; use EncodedAuthenticationParameterSupport");
		}

		public virtual void Start()
		{
			Flow.Initialize();
		}

		public virtual IAuthenticationDataProvider GetAuthData()
		{
            if (this.cachedToken == null || this.cachedToken.Expired)
            {
                TokenResult Tr = this.Flow.Authenticate();
                this.cachedToken = new CachedToken(this, Tr);
            }
            return this.cachedToken.AuthData;
        }

		public virtual void Dispose()
		{
			try
			{
				Flow.Close();
			}
			catch (Exception e)
			{
				throw e;
			}
		}

		internal class CachedToken
		{
			private readonly AuthenticationOAuth2 _outerInstance;

			internal readonly TokenResult Latest;
			internal readonly DateTime ExpiresAt;
			internal readonly AuthenticationDataOAuth2 AuthData;

			public CachedToken(AuthenticationOAuth2 outerInstance, TokenResult latest)
			{
				_outerInstance = outerInstance;
				Latest = latest;
				var adjustedExpiresIn = (int)(latest.ExpiresIn * ExpiryAdjustment);
				ExpiresAt = _outerInstance.Date.AddSeconds(adjustedExpiresIn);
				AuthData = new AuthenticationDataOAuth2(latest.AccessToken);
			}

			public virtual bool Expired
			{
				get
				{
					return _outerInstance.Date == ExpiresAt;
				}
			}
		}
	}


}