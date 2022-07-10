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
	/// A token request based on the exchange of client credentials.
	/// </summary>
	/// <seealso cref="<a href="https://tools.ietf.org/html/rfc6749.section-4.4">OAuth 2.0 RFC 6749, section 4.4</a>"/>
	public class ClientCredentialsExchangeRequest
	{
//  @JsonProperty("client_id") private String clientId;
		private string clientId;

// JsonProperty("client_secret") private String clientSecret;
		private string clientSecret;

// JsonProperty("audience") private String audience;
		private string audience;

// JsonProperty("scope") private String scope;
		private string scope;
	}

}