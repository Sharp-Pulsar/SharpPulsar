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
namespace org.apache.pulsar.client.impl.auth
{
	using Authentication = api.Authentication;
	using AuthenticationDataProvider = api.AuthenticationDataProvider;
	using PulsarClientException = api.PulsarClientException;


	public class MockAuthentication : Authentication
	{
		public IDictionary<string, string> authParamsMap = new Dictionary<string, string>();

		public override string AuthMethodName
		{
			get
			{
				return null;
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public org.apache.pulsar.client.api.AuthenticationDataProvider getAuthData() throws org.apache.pulsar.client.api.PulsarClientException
		public override AuthenticationDataProvider AuthData
		{
			get
			{
				return null;
			}
		}

		public override void configure(IDictionary<string, string> authParams)
		{
			authParamsMap = authParams;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void start() throws org.apache.pulsar.client.api.PulsarClientException
		public override void start()
		{

		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void close() throws java.io.IOException
		public override void close()
		{

		}
	}

}