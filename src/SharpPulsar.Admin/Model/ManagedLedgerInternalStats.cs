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
    /// <summary>
    /// ManagedLedger internal statistics.
    /// </summary>
    public class ManagedLedgerInternalStats
    {

        /// <summary>
        /// Messages published since this broker loaded this managedLedger. </summary>
        [JsonPropertyName("entriesAddedCounter")]
        public long EntriesAddedCounter { get; set; }

        /// <summary>
        /// The total number of entries being tracked. </summary>
        [JsonPropertyName("numberOfEntries")]
        public long NumberOfEntries { get; set; }

        /// <summary>
        /// The total storage size of all messages (in bytes). </summary>
        [JsonPropertyName("totalSize")]
        public long TotalSize { get; set; }

        /// <summary>
        /// The count of messages written to the ledger that is currently open for writing. </summary>
        [JsonPropertyName("currentLedgerEntries")]
        public long CurrentLedgerEntries { get; set; }

        /// <summary>
        /// The size of messages written to the ledger that is currently open for writing (in bytes). </summary>
        [JsonPropertyName("currentLedgerSize")]
        public long CurrentLedgerSize { get; set; }

        /// <summary>
        /// The time when the last ledger is created. </summary>
        [JsonPropertyName("lastLedgerCreatedTimestamp")]
        public string LastLedgerCreatedTimestamp { get; set; }

        /// <summary>
        /// The time when the last ledger failed. </summary>
        [JsonPropertyName("lastLedgerCreationFailureTimestamp")]
        public string LastLedgerCreationFailureTimestamp { get; set; }

        /// <summary>
        /// The number of cursors that are "caught up" and waiting for a new message to be published. </summary>
        [JsonPropertyName("waitingCursorsCount")]
        public int WaitingCursorsCount { get; set; }

        /// <summary>
        /// The number of messages that complete (asynchronous) write requests. </summary>
        [JsonPropertyName("pendingAddEntriesCount")]
        public int PendingAddEntriesCount { get; set; }

        /// <summary>
        /// The ledgerid: entryid of the last message that is written successfully.
        /// If the entryid is -1, then the ledger is open, yet no entries are written. 
        /// </summary>
        [JsonPropertyName("lastConfirmedEntry")]
        public string LastConfirmedEntry { get; set; }

        /// <summary>
        /// The state of this ledger for writing.
        /// The state LedgerOpened means that a ledger is open for saving published messages. 
        /// </summary>
        [JsonPropertyName("state")]
        public string State { get; set; }

        /// <summary>
        /// The ordered list of all ledgers for this topic holding messages. </summary>
        [JsonPropertyName("ledgers")]
        public IList<LedgerInfo> Ledgers { get; set; }

        /// <summary>
        /// The list of all cursors on this topic. Each subscription in the topic stats has a cursor. </summary>
        [JsonPropertyName("cursors")]
        public IDictionary<string, CursorStats> Cursors { get; set; }

        /// <summary>
        /// Ledger information.
        /// </summary>
        public class LedgerInfo
        {
            [JsonPropertyName("ledgerId")]
            public long LedgerId { get; set; }

            [JsonPropertyName("entries")]
            public long Entries { get; set; }

            [JsonPropertyName("size")]
            public long Size { get; set; }

            [JsonPropertyName("offloaded")]
            public bool Offloaded { get; set; }

            [JsonPropertyName("metadata")]
            public string Metadata { get; set; }

            [JsonPropertyName("underReplicated")]
            public bool UnderReplicated { get; set; }
        }

        /// <summary>
        /// Pulsar cursor statistics.
        /// </summary>
        public class CursorStats
        {
            [JsonPropertyName("markDeletePosition")]
            public string MarkDeletePosition { get; set; }

            [JsonPropertyName("readPosition")]
            public string ReadPosition { get; set; }

            [JsonPropertyName("waitingReadOp")]
            public bool WaitingReadOp { get; set; }

            [JsonPropertyName("pendingReadOps")]
            public int PendingReadOps { get; set; }

            [JsonPropertyName("messagesConsumedCounter")]
            public long MessagesConsumedCounter { get; set; }

            [JsonPropertyName("cursorLedger")]
            public long CursorLedger { get; set; }

            [JsonPropertyName("cursorLedgerLastEntry")]
            public long CursorLedgerLastEntry { get; set; }

            [JsonPropertyName("individuallyDeletedMessages")]
            public string IndividuallyDeletedMessages { get; set; }

            [JsonPropertyName("lastLedgerSwitchTimestamp")]
            public string LastLedgerSwitchTimestamp { get; set; }

            [JsonPropertyName("state")]
            public string State { get; set; }

            [JsonPropertyName("numberOfEntriesSinceFirstNotAckedMessage")]
            public long NumberOfEntriesSinceFirstNotAckedMessage { get; set; }

            [JsonPropertyName("totalNonContiguousDeletedMessagesRange")]
            public int TotalNonContiguousDeletedMessagesRange { get; set; }

            [JsonPropertyName("openTimestamp")]
            public bool SubscriptionHavePendingRead { get; set; }

            [JsonPropertyName("subscriptionHavePendingReplayRead")]
            public bool SubscriptionHavePendingReplayRead { get; set; }

            [JsonPropertyName("properties")]
            public IDictionary<string, long> Properties { get; set; }
        }
    }
}
