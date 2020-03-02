using System;

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
namespace SharpPulsar.Impl
{
	public class BatchMessageIdImpl : MessageId
	{
		private const int NoBatch = -1;
		public int BatchIndex {get;}

		public BatchMessageAcker Acker {get;}

		public BatchMessageIdImpl(long ledgerId, long entryId, int partitionIndex, int batchIndex) : this(ledgerId, entryId, partitionIndex, batchIndex, BatchMessageAckerDisabled.Instance)
		{
		}

		public BatchMessageIdImpl(long ledgerId, long entryId, int partitionIndex, int batchIndex, BatchMessageAcker acker) : base(ledgerId, entryId, partitionIndex)
		{
			BatchIndex = batchIndex;
			Acker = acker;
		}

		public BatchMessageIdImpl(MessageId other) : base(other.LedgerId, other.EntryId, other.PartitionIndex)
		{
			if (other is BatchMessageIdImpl otherId)
			{
                BatchIndex = otherId.BatchIndex;
				Acker = otherId.Acker;
			}
			else
			{
				BatchIndex = NoBatch;
				Acker = BatchMessageAckerDisabled.Instance;
			}
		}

		public override int GetHashCode()
		{
			return (int)(31 * (LedgerId + 31 * EntryId) + (31 * PartitionIndex) + BatchIndex);
		}

		public override bool Equals(object obj)
		{
			if (obj is BatchMessageIdImpl other1)
			{
                return LedgerId == other1.LedgerId && EntryId == other1.EntryId && PartitionIndex == other1.PartitionIndex && BatchIndex == other1.BatchIndex;
			}

            if (obj is MessageId other)
            {
                return LedgerId == other.LedgerId && EntryId == other.EntryId && PartitionIndex == other.PartitionIndex && BatchIndex == NoBatch;
            }
            return false;
		}

		public override string ToString()
		{
			return $"{LedgerId:D}:{EntryId:D}:{PartitionIndex:D}:{BatchIndex:D}";
		}

		// Serialization
		public new sbyte[] ToByteArray()
		{
			return ToByteArray(BatchIndex);
		}

		public virtual bool AckIndividual()
		{
			return Acker.AckIndividual(BatchIndex);
		}

		public virtual bool AckCumulative()
		{
			return Acker.AckCumulative(BatchIndex);
		}

		public virtual int OutstandingAcksInSameBatch => Acker.OutstandingAcks;

        public virtual int BatchSize => Acker.BatchSize;

        public virtual MessageId PrevBatchMessageId()
		{
			return new MessageId(LedgerId, EntryId - 1, PartitionIndex);
		}


	}

}