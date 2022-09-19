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
namespace SharpPulsar.Admin.Model
{
    public partial class TransactionPendingAckInternalStats
    {
        /// <summary>
        /// Initializes a new instance of the ReplicatorStats class.
        /// </summary>
        public TransactionPendingAckInternalStats()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the ReplicatorStats class.
        /// </summary>
        public TransactionPendingAckInternalStats(TransactionLogStats transactionLogStats = default)
        {
            this.PendingAckLogStats = transactionLogStats;
            CustomInit();
        }
        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// The transaction coordinator log stats </summary>

        [JsonPropertyName("pendingAckLogStats")]
        public TransactionLogStats PendingAckLogStats { get; set; }
    }
    public partial class TransactionCoordinatorInternalStats
    {
        /// <summary>
        /// Initializes a new instance of the ReplicatorStats class.
        /// </summary>
        public TransactionCoordinatorInternalStats()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the ReplicatorStats class.
        /// </summary>
        public TransactionCoordinatorInternalStats(TransactionLogStats transactionLogStats = default)
        {
            this.TransactionLogStats = transactionLogStats;
            CustomInit();
        }
        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// The transaction coordinator log stats </summary>

        [JsonPropertyName("transactionLogStats")]
        public TransactionLogStats TransactionLogStats { get; set; }
    }
    public partial class TransactionLogStats
    {
        /// <summary>
        /// Initializes a new instance of the ReplicatorStats class.
        /// </summary>
        public TransactionLogStats()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the ReplicatorStats class.
        /// </summary>
        public TransactionLogStats(string managedLedgerName = default(string), ManagedLedgerInternalStats managedLedgerInternalStats = default)
        {
            this.ManagedLedgerName = managedLedgerName;
            this.ManagedLedgerInternalStats = managedLedgerInternalStats;
            CustomInit();
        }
        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// The managed ledger name </summary>
        
        [JsonPropertyName("managedLedgerName")]
        public string ManagedLedgerName { get; set; }

        /// <summary>
        /// The manage ledger internal stats </summary>
        [JsonPropertyName("managedLedgerInternalStats")]
        public ManagedLedgerInternalStats ManagedLedgerInternalStats { get; set; }
    }
    
    public partial class TransactionInPendingAckStats
    {
        /// <summary>
        /// Initializes a new instance of the ReplicatorStats class.
        /// </summary>
        public TransactionInPendingAckStats()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the ReplicatorStats class.
        /// </summary>
        public TransactionInPendingAckStats(string cumulativeAckPosition = default(string))
        {
            this.CumulativeAckPosition = cumulativeAckPosition;
            CustomInit();
        }
        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// The position of this transaction cumulative ack.</summary>
        [JsonPropertyName("cumulativeAckPosition")]
        public string CumulativeAckPosition { get; set; }
    }
    public partial class TransactionPendingAckStats
    {
        /// <summary>
        /// Initializes a new instance of the ReplicatorStats class.
        /// </summary>
        public TransactionPendingAckStats()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the ReplicatorStats class.
        /// </summary>
        public TransactionPendingAckStats(string state = default(string))
        {
            this.State = state;
            CustomInit();
        }
        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// The position of this transaction cumulative ack.</summary>
        [JsonPropertyName("state")]
        public string State { get; set; }
    }
    public partial class TransactionInBufferStats
    {
        /// <summary>
        /// Initializes a new instance of the ReplicatorStats class.
        /// </summary>
        public TransactionInBufferStats()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the ReplicatorStats class.
        /// </summary>
        public TransactionInBufferStats(string startPosition = default(string), bool? aborted = default(bool?))
        {
            this.StartPosition = startPosition;
            this.Aborted = aborted;
            CustomInit();
        }
        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// The start position of this transaction in transaction buffer. </summary>
        [JsonPropertyName("startPosition")]
        public string StartPosition { get; set; }

        /// <summary>
        /// The flag of this transaction have been aborted. </summary>
        [JsonPropertyName("aborted")]
        public bool? Aborted { get; set; }
    }
    public partial class TransactionCoordinatorStats
    {
        /// <summary>
        /// Initializes a new instance of the ReplicatorStats class.
        /// </summary>
        public TransactionCoordinatorStats()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the ReplicatorStats class.
        /// </summary>
        public TransactionCoordinatorStats(string state = default(string), long? leastSigBits = default(long?), long? lowWaterMark = default(long?))
        {
            this.State = state;
            this.LeastSigBits = leastSigBits;
            this.LowWaterMark = lowWaterMark;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// The state of this transaction metadataStore. </summary>
        [JsonPropertyName("state")]
        public string State { get; set; }

        /// <summary>
        /// The sequenceId of transaction metadataStore. </summary>
        [JsonPropertyName("leastSigBits")]
        public long? LeastSigBits { get; set; }

        /// <summary>
        /// The low water mark of transaction metadataStore. </summary>
        [JsonPropertyName("lowWaterMark")]
        public long? LowWaterMark { get; set; }
    }
    public partial class TransactionBufferStats
    {
        /// <summary>
        /// Initializes a new instance of the ReplicatorStats class.
        /// </summary>
        public TransactionBufferStats()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the ReplicatorStats class.
        /// </summary>
        public TransactionBufferStats(string state = default(string), string maxReadPosition = default(string), long? lastSnapshotTimestamps = default(long?))
        {
            this.State = state;
            this.MaxReadPosition = maxReadPosition;
            this.LastSnapshotTimestamps = lastSnapshotTimestamps;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// The state of this transaction metadataStore. </summary>
        [JsonPropertyName("state")]
        public string State { get; set; }

        /// <summary>
        /// The max read position of this transaction buffer. </summary>
        [JsonPropertyName("maxReadPosition")]
        public string MaxReadPosition { get; set; }

        /// <summary>
        /// The last snapshot timestamps of this transaction buffer. </summary>
        [JsonPropertyName("lastSnapshotTimestamps")]
        public long? LastSnapshotTimestamps { get; set; }
    }
    public partial class TransactionMetadata
    {
        /// <summary>
        /// Initializes a new instance of the ReplicatorStats class.
        /// </summary>
        public TransactionMetadata()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the ReplicatorStats class.
        /// </summary>
        public TransactionMetadata(string txnid = default(string), string status = default(string), long? openTimestamp = default(long?), long? timeoutAt = default(long?), IDictionary<string, TransactionInBufferStats> producedPartitions = default, IDictionary<string, IDictionary<string, TransactionInPendingAckStats>> ackedPartitions = default)
        {
            this.TxnId = txnid;
            this.Status = status;
            this.OpenTimestamp = openTimestamp;
            this.TimeoutAt = timeoutAt;
            this.ProducedPartitions = producedPartitions;
            this.AckedPartitions = ackedPartitions;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();
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
