using System.Security.Policy;
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
        //JsonProperty("issuer") private java.net.URL authorizationEndpoint
        public Url issuer;
        //JsonProperty("authorization_endpoint") private java.net.URL authorizationEndpoint
        public Url authorizationEndpoint;
        //JsonProperty("token_endpoint") private java.net.URL tokenEndpoint
        public Url tokenEndpoint;
        //JsonProperty("userinfo_endpoint") private java.net.URL userInfoEndpoint;
        public Url userInfoEndpoint;
        //JsonProperty("revocation_endpoint") private java.net.URL revocationEndpoint;
        public Url revocationEndpoint;
        //JsonProperty("jwks_uri") private java.net.URL jwksUri
        public Url jwksUri;
        //JsonProperty("device_authorization_endpoint") private java.net.URL deviceAuthorizationEndpoint;
        public Url deviceAuthorizationEndpoint;
	}

}