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
        public Url Issuer { get; set; }

        [JsonPropertyName("authorization_endpoint")]
        public Url AuthorizationEndpoint { get; set; }

        [JsonPropertyName("token_endpoint")]
        public Url TokenEndpoint { get; set; }

        [JsonPropertyName("userinfo_endpoint")]
        public Url UserInfoEndpoint { get; set; }

        [JsonPropertyName("revocation_endpoint")]
        public Url RevocationEndpoint { get; set; }

        [JsonPropertyName("jwks_uri")]
        public Url JwksUri { get; set; }

        [JsonPropertyName("device_authorization_endpoint")]
        public Url DeviceAuthorizationEndpoint { get; set; }
    }

}