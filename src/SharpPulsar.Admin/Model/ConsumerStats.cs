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
	/// Consumer statistics.
	/// </summary>
	public class ConsumerStats
	{
        /// <summary>
        /// Total rate of message ack(msg/s).
        /// </summary>
        [JsonPropertyName("messageAckRate")]
        public double MessageAckRate { get; set; }


        /// <summary>
        /// </summary>
        [JsonPropertyName("msgOutCounter")]
        public long? MsgOutCounter { get; set; }

        /// <summary>
        /// </summary>
        [JsonPropertyName("bytesOutCounter")]
        public long? BytesOutCounter { get; set; }

        /// <summary>
        /// </summary>
        [JsonPropertyName("msgRateOut")]
        public double? MsgRateOut { get; set; }

        /// <summary>
        /// </summary>
        [JsonPropertyName("msgThroughputOut")]
        public double? MsgThroughputOut { get; set; }

        /// <summary>
        /// </summary>
        [JsonPropertyName("connectedSince")]
        public string ConnectedSince { get; set; }

        /// <summary>
        /// </summary>
        [JsonPropertyName("chunkedMessageRate")]
        public double? ChunkedMessageRate { get; set; }

        /// <summary>
        /// </summary>
        [JsonPropertyName("msgRateRedeliver")]
        public double? MsgRateRedeliver { get; set; }

        /// <summary>
        /// </summary>
        [JsonPropertyName("clientVersion")]
        public string ClientVersion { get; set; }

        /// <summary>
        /// </summary>
        [JsonPropertyName("unackedMessages")]
        public int? UnackedMessages { get; set; }

        /// <summary>
        /// </summary>
        [JsonPropertyName("lastConsumedTimestamp")]
        public long? LastConsumedTimestamp { get; set; }

        /// <summary>
        /// </summary>
        [JsonPropertyName("lastAckedTimestamp")]
        public long? LastAckedTimestamp { get; set; }

        /// <summary>
        /// </summary>
        [JsonPropertyName("consumerName")]
        public string ConsumerName { get; set; }

        /// <summary>
        /// </summary>
        [JsonPropertyName("avgMessagesPerEntry")]
        public int? AvgMessagesPerEntry { get; set; }

        /// <summary>
        /// </summary>
        [JsonPropertyName("blockedConsumerOnUnackedMsgs")]
        public bool? BlockedConsumerOnUnackedMsgs { get; set; }

        /// <summary>
        /// </summary>
        [JsonPropertyName("readPositionWhenJoining")]
        public string ReadPositionWhenJoining { get; set; }

        /// <summary>
        /// </summary>
        [JsonPropertyName("keyHashRanges")]
        public IList<string> KeyHashRanges { get; set; }

        /// <summary>
        /// </summary>
        [JsonPropertyName("availablePermits")]
        public int? AvailablePermits { get; set; }

        /// <summary>
        /// </summary>
        [JsonPropertyName("metadata")]
        public IDictionary<string, string> Metadata { get; set; }

        /// <summary>
        /// </summary>
        [JsonPropertyName("address")]
        public string Address { get; set; }

    }

}