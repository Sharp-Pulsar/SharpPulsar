﻿/// <summary>
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
	/// Represents an error returned from an OAuth 2.0 token endpoint.
	/// </summary>
    /// 
	public class TokenError
	{
        //JsonProperty("error") private String error;
        public string Error;
        //JsonProperty("error_description") private String errorDescription;
        public string ErrorDescription;
        //JsonProperty("error_uri") private String errorUri;
        public string ErrorUri;
	}

}