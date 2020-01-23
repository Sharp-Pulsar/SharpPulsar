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

	using Authentication = SharpPulsar.Api.Authentication;
	using AuthenticationDataProvider = SharpPulsar.Api.AuthenticationDataProvider;
	using EncodedAuthenticationParameterSupport = SharpPulsar.Api.EncodedAuthenticationParameterSupport;
	using PulsarClientException = SharpPulsar.Api.PulsarClientException;

	using VisibleForTesting = com.google.common.annotations.VisibleForTesting;

	/// 
	/// <summary>
	/// This plugin requires these parameters
	/// 
	/// tlsCertFile: A file path for a client certificate. tlsKeyFile: A file path for a client private key.
	/// 
	/// </summary>
	[Serializable]
	public class AuthenticationTls : Authentication, EncodedAuthenticationParameterSupport
	{

		private const long SerialVersionUID = 1L;

		public virtual CertFilePath {get;}
		public virtual KeyFilePath {get;}

		// Load Bouncy Castle
		static AuthenticationTls()
		{
			Security.addProvider(new org.bouncycastle.jce.provider.BouncyCastleProvider());
		}

		public AuthenticationTls()
		{
		}

		public AuthenticationTls(string CertFilePath, string KeyFilePath)
		{
			this.CertFilePath = CertFilePath;
			this.KeyFilePath = KeyFilePath;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void close() throws java.io.IOException
		public override void Close()
		{
			// noop
		}

		public virtual string AuthMethodName
		{
			get
			{
				return "tls";
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public SharpPulsar.api.AuthenticationDataProvider getAuthData() throws SharpPulsar.api.PulsarClientException
		public virtual AuthenticationDataProvider AuthData
		{
			get
			{
				try
				{
					return new AuthenticationDataTls(CertFilePath, KeyFilePath);
				}
				catch (Exception E)
				{
					throw new PulsarClientException(E);
				}
			}
		}

		public override void Configure(string EncodedAuthParamString)
		{
			IDictionary<string, string> AuthParamsMap = null;
			try
			{
				AuthParamsMap = AuthenticationUtil.configureFromJsonString(EncodedAuthParamString);
			}
			catch (Exception)
			{
				// auth-param is not in json format
			}
			AuthParamsMap = (AuthParamsMap == null || AuthParamsMap.Count == 0) ? AuthenticationUtil.configureFromPulsar1AuthParamString(EncodedAuthParamString) : AuthParamsMap;
			AuthParams = AuthParamsMap;
		}

		[Obsolete]
		public override void Configure(IDictionary<string, string> AuthParams)
		{
			AuthParams = AuthParams;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void start() throws SharpPulsar.api.PulsarClientException
		public override void Start()
		{
			// noop
		}

		private IDictionary<string, string> AuthParams
		{
			set
			{
				CertFilePath = value["tlsCertFile"];
				KeyFilePath = value["tlsKeyFile"];
			}
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @VisibleForTesting public String getCertFilePath()

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @VisibleForTesting public String getKeyFilePath()

	}

}