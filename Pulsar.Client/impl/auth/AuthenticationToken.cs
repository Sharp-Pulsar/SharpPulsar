using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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

namespace Pulsar.Client.Impl.Auth
{
	using Charsets = com.google.common.@base.Charsets;


	using Authentication = Api.Authentication;
	using AuthenticationDataProvider = Api.AuthenticationDataProvider;
	using EncodedAuthenticationParameterSupport = Api.EncodedAuthenticationParameterSupport;
	using PulsarClientException = Api.PulsarClientException;

	/// <summary>
	/// Token based authentication provider.
	/// </summary>
	public class AuthenticationToken : Authentication, EncodedAuthenticationParameterSupport
	{

		private System.Func<string> tokenSupplier;

		public AuthenticationToken()
		{
		}

		public AuthenticationToken(string token) : this(() => token)
		{
		}

		public AuthenticationToken(System.Func<string> tokenSupplier)
		{
			this.tokenSupplier = tokenSupplier;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @public void close() throws java.io.IOException
		public void Close()
		{
			// noop
		}

		public string AuthMethodName
		{
			get
			{
				return "token";
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @public org.apache.pulsar.client.api.AuthenticationDataProvider getAuthData() throws org.apache.pulsar.client.api.PulsarClientException
		public AuthenticationDataProvider AuthData
		{
			get
			{
				return new AuthenticationDataToken(tokenSupplier);
			}
		}

		public void Configure(string encodedAuthParamString)
		{
			// Interpret the whole param string as the token. If the string contains the notation `token:xxxxx` then strip
			// the prefix
			if (encodedAuthParamString.StartsWith("token:", StringComparison.Ordinal))
			{
				this.tokenSupplier = () => encodedAuthParamString.Substring("token:".Length);
			}
			else if (encodedAuthParamString.StartsWith("file:", StringComparison.Ordinal))
			{
				// Read token from a file
				URI filePath = URI.create(encodedAuthParamString);
				this.tokenSupplier = () =>
				{
				try
				{
					return (new string(Files.readAllBytes(Paths.get(filePath)), Charsets.UTF_8)).Trim();
				}
				catch (IOException e)
				{
					throw new Exception("Failed to read token from file", e);
				}
				};
			}
			else
			{
				this.tokenSupplier = () => encodedAuthParamString;
			}
		}

		public void Configure(IDictionary<string, string> authParams)
		{
			// noop
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @public void start() throws org.apache.pulsar.client.api.PulsarClientException
		public void Start()
		{
			// noop
		}

		public ValueTask DisposeAsync()
		{
			throw new NotImplementedException();
		}
	}

}