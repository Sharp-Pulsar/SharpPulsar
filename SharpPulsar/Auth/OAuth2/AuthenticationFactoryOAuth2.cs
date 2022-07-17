using System;
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
	/// Factory class that allows to create <seealso cref="Authentication"/> instances
	/// for OAuth 2.0 authentication methods.
	/// </summary>
	public sealed class AuthenticationFactoryOAuth2
	{

		/// <summary>
		/// Authenticate with client credentials.
		/// </summary>
		/// <param name="issuerUrl"> the issuer URL </param>
		/// <param name="credentialsUrl"> the credentials URL </param>
		/// <param name="audience"> An optional field. The audience identifier used by some Identity Providers, like Auth0. </param>
		/// <returns> an Authentication object </returns>
		public static IAuthentication ClientCredentials(Uri issuerUrl, Uri credentialsUrl, string audience)
		{
			return ClientCredentials(issuerUrl, credentialsUrl, audience, null);
		}

		/// <summary>
		/// Authenticate with client credentials.
		/// </summary>
		/// <param name="issuerUrl"> the issuer URL </param>
		/// <param name="credentialsUrl"> the credentials URL </param>
		/// <param name="audience"> An optional field. The audience identifier used by some Identity Providers, like Auth0. </param>
		/// <param name="scope"> An optional field. The value of the scope parameter is expressed as a list of space-delimited,
		///              case-sensitive strings. The strings are defined by the authorization server.
		///              If the value contains multiple space-delimited strings, their order does not matter,
		///              and each string adds an additional access range to the requested scope.
		///              From here: https://datatracker.ietf.org/doc/html/rfc6749#section-4.4.2 </param>
		/// <returns> an Authentication object </returns>
		public static IAuthentication ClientCredentials(Uri issuerUrl, Uri credentialsUrl, string audience, string scope)
		{
            var flow = new ClientCredentialsFlow(issuerUrl, audience, credentialsUrl.LocalPath, scope);
            return new AuthenticationOAuth2(flow, DateTime.Now);
		}
	}

}