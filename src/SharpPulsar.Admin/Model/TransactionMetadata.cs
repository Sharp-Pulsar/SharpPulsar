/**
 * Licensed to the Apache Software Foundation (ASF) under one
 * or more contributor license agreements.  See the NOTICE file
 * distributed with this work for additional information
 * regarding copyright ownership.  The ASF licenses this file
 * to you under the Apache License, Version 2.0 (the
 * "License"); you may not use this file except in compliance
 * with the License.  You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing,
 * software distributed under the License is distributed on an
 * "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
 * KIND, either express or implied.  See the License for the
 * specific language governing permissions and limitations
 * under the License.
 */

using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SharpPulsar.Admin.Model
{
    public class TransactionMetadata
    {
        
        /// <summary>
        /// The txnId of this transaction. </summary>
        [JsonPropertyName("txnId")]
        public string TxnId { get; set; }

        /// <summary>
        /// The status of this transaction. </summary>
        [JsonPropertyName("status")]
        public string Status { get; set; }

        /// <summary>
        /// The open time of this transaction. </summary>
        [JsonPropertyName("openTimestamp")]
        public long? OpenTimestamp { get; set; }

        /// <summary>
        /// The timeout of this transaction. </summary>
        [JsonPropertyName("timeoutAt")]
        public long? TimeoutAt { get; set; }

        /// <summary>
        /// The producedPartitions of this transaction. </summary>
        [JsonPropertyName("producedPartitions")]
        public IDictionary<string, TransactionInBufferStats> ProducedPartitions { get; set; }

        /// <summary>
        /// The ackedPartitions of this transaction. </summary>
        [JsonPropertyName("ackedPartitions")]
        public IDictionary<string, IDictionary<string, TransactionInPendingAckStats>> AckedPartitions { get; set; }

    }
}
