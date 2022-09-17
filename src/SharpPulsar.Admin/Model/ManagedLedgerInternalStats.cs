using System.Collections.Generic;

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
	/// ManagedLedger internal statistics.
	/// </summary>
	public class ManagedLedgerInternalStats
	{

		/// <summary>
		/// Messages published since this broker loaded this managedLedger. </summary>
		public long EntriesAddedCounter;

		/// <summary>
		/// The total number of entries being tracked. </summary>
		public long NumberOfEntries;

		/// <summary>
		/// The total storage size of all messages (in bytes). </summary>
		public long TotalSize;

		/// <summary>
		/// The count of messages written to the ledger that is currently open for writing. </summary>
		public long CurrentLedgerEntries;

		/// <summary>
		/// The size of messages written to the ledger that is currently open for writing (in bytes). </summary>
		public long CurrentLedgerSize;

		/// <summary>
		/// The time when the last ledger is created. </summary>
		public string LastLedgerCreatedTimestamp;

		/// <summary>
		/// The time when the last ledger failed. </summary>
		public string LastLedgerCreationFailureTimestamp;

		/// <summary>
		/// The number of cursors that are "caught up" and waiting for a new message to be published. </summary>
		public int WaitingCursorsCount;

		/// <summary>
		/// The number of messages that complete (asynchronous) write requests. </summary>
		public int PendingAddEntriesCount;

		/// <summary>
		/// The ledgerid: entryid of the last message that is written successfully.
		/// If the entryid is -1, then the ledger is open, yet no entries are written. 
		/// </summary>
		public string LastConfirmedEntry;

		/// <summary>
		/// The state of this ledger for writing.
		/// The state LedgerOpened means that a ledger is open for saving published messages. 
		/// </summary>
		public string State;

		/// <summary>
		/// The ordered list of all ledgers for this topic holding messages. </summary>
		public IList<LedgerInfo> Ledgers;

		/// <summary>
		/// The list of all cursors on this topic. Each subscription in the topic stats has a cursor. </summary>
		public IDictionary<string, CursorStats> Cursors;

		/// <summary>
		/// Ledger information.
		/// </summary>
		public class LedgerInfo
		{
			public long LedgerId;
			public long Entries;
			public long Size;
			public bool Offloaded;
			public string Metadata;
			public bool UnderReplicated;
		}

		/// <summary>
		/// Pulsar cursor statistics.
		/// </summary>
		public class CursorStats
		{
			public string MarkDeletePosition;
			public string ReadPosition;
			public bool WaitingReadOp;
			public int PendingReadOps;

			public long MessagesConsumedCounter;
			public long CursorLedger;
			public long CursorLedgerLastEntry;
			public string IndividuallyDeletedMessages;
			public string LastLedgerSwitchTimestamp;
			public string State;
			public long NumberOfEntriesSinceFirstNotAckedMessage;
			public int TotalNonContiguousDeletedMessagesRange;
			public bool SubscriptionHavePendingRead;
			public bool SubscriptionHavePendingReplayRead;

			public IDictionary<string, long> Properties;
		}
	}

}