using System;
using System.Collections;
using DotNetty.Common.Utilities;
using Org.BouncyCastle.Bcpg;
using SharpPulsar.API;

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
namespace SharpPulsar.Batch
{
    public class BatchMessageId : MessageId
	{
        public int BatchIndex { get; }
        private readonly int _batchSize;

        private readonly BitArray _ackSet;

		// Private constructor used only for json deserialization
		private BatchMessageId() : this(-1, -1, -1, -1)
		{
		}

		public BatchMessageId(long ledgerId, long entryId, int partitionIndex, int batchIndex) : this(ledgerId, entryId, partitionIndex, batchIndex, 0, null)
		{
		}

		public BatchMessageId(long ledgerId, long entryId, int partitionIndex, int batchIndex, int batchSize, BitArray ackSet) : base(ledgerId, entryId, partitionIndex)
		{
			BatchIndex = batchIndex;
			_batchSize = batchSize;
			_ackSet = ackSet;
		}

		public BatchMessageId(IMessageIdAdv other) : this(other.LedgerId, other.EntryId, other.PartitionIndex, other.BatchIndex, other.BatchSize, other.AckSet)
		{
		}

		public override int GetHashCode()
		{
			return (int)(31 * (LedgerId + 31 * EntryId) + (31 * PartitionIndex) + BatchIndex);
		}

		public override bool Equals(object obj)
		{
            return MessageIdAdvUtils.Equals(this, obj);
        }

		public override string ToString()
		{
			return $"{LedgerId:D}:{EntryId:D}:{PartitionIndex:D}:{BatchIndex:D}";
		}

		// Serialization
		public override byte[] ToByteArray()
		{
			return ToByteArray(BatchIndex, BatchSize);
		}

        
		public virtual bool AckIndividual()
		{
            return MessageIdAdvUtils.Acknowledge(this, true);
        }

		public virtual bool AckCumulative()
		{
            return MessageIdAdvUtils.Acknowledge(this, true);
        }
		
		public virtual int OutstandingAcksInSameBatch => 0;

        public virtual int BatchSize => BatchSize;

        public virtual MessageId PrevBatchMessageId()
		{
            return (MessageId)MessageIdAdvUtils.PrevMessageId(this);
        }

        public BitArray GetAckSet()
        {
            return _ackSet;
        }

        public static BitArray NewAckSet(int batchSize)
        {
            var ackSet = new BitArray(batchSize);
            ackSet.Set(batchSize, true);
            return ackSet;
        }

    }

}