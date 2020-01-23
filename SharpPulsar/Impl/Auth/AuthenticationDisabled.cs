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

	[Serializable]
	public class AuthenticationDisabled : Authentication, EncodedAuthenticationParameterSupport
	{

		protected internal readonly AuthenticationDataProvider NullData = new AuthenticationDataNull();
		/// 
		private const long SerialVersionUID = 1L;

		public AuthenticationDisabled()
		{
		}

		public virtual string AuthMethodName
		{
			get
			{
				return "none";
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public SharpPulsar.api.AuthenticationDataProvider getAuthData() throws SharpPulsar.api.PulsarClientException
		public virtual AuthenticationDataProvider AuthData
		{
			get
			{
				return NullData;
			}
		}

		public override void Configure(string EncodedAuthParamString)
		{
		}

		[Obsolete]
		public override void Configure(IDictionary<string, string> AuthParams)
		{
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void start() throws SharpPulsar.api.PulsarClientException
		public override void Start()
		{
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void close() throws java.io.IOException
		public override void Close()
		{
			// Do nothing
		}
	}

}