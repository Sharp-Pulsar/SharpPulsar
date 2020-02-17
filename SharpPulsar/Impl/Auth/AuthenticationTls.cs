using SharpPulsar.Api;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PulsarClientException = SharpPulsar.Exceptions.PulsarClientException;

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
	
	/// 
	/// <summary>
	/// This plugin requires these parameters
	/// 
	/// tlsCertFile: A file path for a client certificate. tlsKeyFile: A file path for a client private key.
	/// 
	/// </summary>
	public class AuthenticationTls : IAuthentication, IEncodedAuthenticationParameterSupport
	{

		private const long serialVersionUID = 1L;

		private string certFilePath;
		private string keyFilePath;

		// Load Bouncy Castle
		static AuthenticationTls()
		{
			//Security.addProvider(new org.bouncycastle.jce.provider.BouncyCastleProvider());
		}

		public AuthenticationTls()
		{
		}

		public AuthenticationTls(string certFilePath, string keyFilePath)
		{
			this.certFilePath = certFilePath;
			this.keyFilePath = keyFilePath;
		}
		public void Close()
		{
			// noop
		}

		public string AuthMethodName
		{
			get
			{
				return "tls";
			}
		}


		public IAuthenticationDataProvider AuthData
		{
			get
			{
				try
				{
					return new AuthenticationDataTls(certFilePath, keyFilePath);
				}
				catch (System.Exception e)
				{
					throw new PulsarClientException(e.Message);
				}
			}
		}

		public void Configure(string encodedAuthParamString)
		{
			IDictionary<string, string> authParamsMap = null;
			try
			{
				authParamsMap = AuthenticationUtil.ConfigureFromJsonString(encodedAuthParamString);
			}
			catch (System.Exception)
			{
				// auth-param is not in json format
			}
			authParamsMap = (authParamsMap == null || authParamsMap.Count == 0) ? AuthenticationUtil.ConfigureFromPulsar1AuthParamString(encodedAuthParamString) : authParamsMap;
			AuthParams = authParamsMap;
		}

		public void Start()
		{
			// noop
		}

		public ValueTask DisposeAsync()
		{
			throw new NotImplementedException();
		}

		public void Configure(IDictionary<string, string> AuthParams)
		{
			throw new NotImplementedException();
		}

		public void Dispose()
		{
			throw new NotImplementedException();
		}

		private IDictionary<string, string> AuthParams
		{
			set
			{
				certFilePath = value["tlsCertFile"];
				keyFilePath = value["tlsKeyFile"];
			}
		}

		public virtual string CertFilePath
		{
			get
			{
				return certFilePath;
			}
		}
		public virtual string KeyFilePath
		{
			get
			{
				return keyFilePath;
			}
		}

	}

}