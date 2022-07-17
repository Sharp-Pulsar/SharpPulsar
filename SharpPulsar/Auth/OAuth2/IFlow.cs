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
	/// An OAuth 2.0 authorization flow.
	/// </summary>
	internal interface IFlow //: AutoCloseable
	{

		/// <summary>
		/// Initializes the authorization flow. </summary>
		/// <exception cref="PulsarClientException"> if the flow could not be initialized. </exception>
        /// 
		void Initialize();

        /// <summary>
        /// Acquires an access token from the OAuth 2.0 authorization server. </summary>
        /// <returns> a token result including an access token and optionally a refresh token. </returns>
        /// <exception cref="PulsarClientException"> if authentication failed. </exception>() throws org.apache.pulsar.client.api.PulsarClientException;
        TokenResult Authenticate();

		/// <summary>
		/// Closes the authorization flow.
		/// </summary>
		void Close();
	}

}