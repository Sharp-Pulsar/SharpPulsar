using System;
using System.Security.Policy;
using System.Text.Json.Serialization;
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
	/// Represents OAuth 2.0 Server Metadata.
	/// </summary>
	public class Metadata
	{

        [JsonPropertyName("issuer")]
        public Uri Issuer { get; set; }

        [JsonPropertyName("authorization_endpoint")]
        public Uri AuthorizationEndpoint { get; set; }

        [JsonPropertyName("token_endpoint")]
        public Uri TokenEndpoint { get; set; }

        [JsonPropertyName("userinfo_endpoint")]
        public Uri UserInfoEndpoint { get; set; }

        [JsonPropertyName("revocation_endpoint")]
        public Uri RevocationEndpoint { get; set; }

        [JsonPropertyName("jwks_uri")]
        public Uri JwksUri { get; set; }

        [JsonPropertyName("device_authorization_endpoint")]
        public Uri DeviceAuthorizationEndpoint { get; set; }
    }

}