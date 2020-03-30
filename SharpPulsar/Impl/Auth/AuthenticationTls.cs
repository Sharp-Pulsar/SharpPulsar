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

		private const long SerialVersionUid = 1L;

		private string _certFilePath;
		private string _keyFilePath;
		
		public AuthenticationTls(string certFilePath, string keyFilePath)
		{
			this._certFilePath = certFilePath;
			this._keyFilePath = keyFilePath;
		}
		public void Close()
		{
			// noop
		}

		public string AuthMethodName => "tls";


        public IAuthenticationDataProvider AuthData
		{
			get
			{
				try
				{
					return new AuthenticationDataTls(_certFilePath, _keyFilePath);
				}
				catch (System.Exception e)
				{
					throw new PulsarClientException(e.Message);
				}
			}
		}

		public void Configure(string encodedAuthParamString)
		{
			//format = tlsCertFile,tlsKeyFile
			if (!string.IsNullOrWhiteSpace(encodedAuthParamString))
            {
                var @params = encodedAuthParamString.Split(',');
                _certFilePath = @params[0];
                _keyFilePath = @params[1];
            }
		}

		public void Start()
		{
			// noop
		}

		public ValueTask DisposeAsync()
		{
			throw new NotImplementedException();
		}

		public void Configure(IDictionary<string, string> authParams)
		{
			throw new NotImplementedException();
		}

		public void Dispose()
		{
			throw new NotImplementedException();
		}

		public virtual string CertFilePath => _certFilePath;

        public virtual string KeyFilePath => _keyFilePath;
    }

}