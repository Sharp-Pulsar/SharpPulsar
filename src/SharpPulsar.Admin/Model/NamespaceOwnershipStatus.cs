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
	/// Information about the namespace's ownership.
	/// </summary>
	public class NamespaceOwnershipStatus
	{
        /// <summary>
        /// Gets or sets possible values include: 'primary', 'secondary',
        /// 'shared'
        /// </summary>
        [JsonPropertyName("broker_assignment")]
        public BrokerAssignment BrokerAssignment { get; set; }

        /// <summary>
        /// </summary>
        [JsonPropertyName("is_controlled")]
        public bool? IsControlled { get; set; }

        /// <summary>
        /// </summary>
        [JsonPropertyName("is_active")]
        public bool? IsActive { get; set; }
        public override string ToString()
		{
			return string.Format("[broker_assignment={0} is_controlled={1} is_active={2}]", BrokerAssignment, IsControlled, IsActive);
		}
	}

}