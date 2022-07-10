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
namespace SharpPulsar.Auth.OAuth2.Protocol
{

	/// <summary>
	/// An interface for exchanging client credentials for an access token.
	/// </summary>
	public interface ClientCredentialsExchanger : AutoCloseable
	{
		/// <summary>
		/// Requests an exchange of client credentials for an access token. </summary>
		/// <param name="req"> the request details. </param>
		/// <returns> an access token. </returns>
		/// <exception cref="TokenExchangeException"> if the OAuth server returned a detailed error. </exception>
		/// <exception cref="IOException"> if a general IO error occurred. </exception>
		TokenResult ExchangeClientCredentials(ClientCredentialsExchangeRequest req);
	}

}