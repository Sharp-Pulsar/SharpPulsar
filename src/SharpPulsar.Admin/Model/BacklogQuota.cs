using System;
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
	/// Unit of a backlog quota configuration for a scoped resource in a Pulsar instance.
	/// 
	/// <para>A scoped resource is identified by a <seealso cref="BacklogQuotaType"/> enumeration type which is containing two attributes:
	/// <code>limit</code> representing a quota limit in bytes and <code>policy</code> for backlog retention policy.
	/// </para>
	/// </summary>
	public class BacklogQuota
	{
        /// <summary>
        /// </summary>
        [JsonPropertyName("limitSize")]
        public long? LimitSize { get; set; }

        /// <summary>
        /// </summary>
        [JsonPropertyName("limitTime")]
        public int? LimitTime { get; set; }

        /// <summary>
        /// Gets or sets possible values include: 'producer_request_hold',
        /// 'producer_exception', 'consumer_backlog_eviction'
        /// </summary>
        [JsonPropertyName("policy")]
        public RetentionPolicy Policy { get; set; }


        public enum BacklogQuotaType
        {
            DestinationStorage,
            MessageAge,
        }

        public enum RetentionPolicy
        {
            /// <summary>
            /// Policy which holds producer's send request until the resource becomes available (or holding times out). </summary>
            ProducerRequestHold,

            /// <summary>
            /// Policy which throws javax.jms.ResourceAllocationException to the producer. </summary>
            ProducerException,

            /// <summary>
            /// Policy which evicts the oldest message from the slowest consumer's backlog. </summary>
            ConsumerBacklogEviction,
        }
    }

    
}