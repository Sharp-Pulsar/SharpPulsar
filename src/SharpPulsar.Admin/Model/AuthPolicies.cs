using System.Collections.Generic;
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
namespace SharpPulsar.Admin.Model
{
	/// <summary>
	/// Authentication policies.
	/// </summary>
	public class AuthPolicies
	{
        /// <summary>
        /// </summary>
        [JsonPropertyName("namespaceAuthentication")]
        public IDictionary<string, IList<string>> NamespaceAuthentication { get; set; }

        /// <summary>
        /// </summary>
        [JsonPropertyName("topicAuthentication")]
        public IDictionary<string, IDictionary<string, IList<string>>> TopicAuthentication { get; set; }

        /// <summary>
        /// </summary>
        [JsonPropertyName("subscriptionAuthentication")]
        public IDictionary<string, IList<string>> SubscriptionAuthentication { get; set; }


    }

}