using System;
using SharpPulsar.Impl;

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
        private const int NoBatch = -1;
        public int BatchIndex { get; }
        private readonly int _batchSize;

        public BatchMessageAcker Acker { get; }

		// Private constructor used only for json deserialization
		private BatchMessageId() : this(-1, -1, -1, -1)
		{
		}

		public BatchMessageId(long ledgerId, long entryId, int partitionIndex, int batchIndex) : this(ledgerId, entryId, partitionIndex, batchIndex, 0, BatchMessageAckerDisabled.Instance)
		{
		}

		public BatchMessageId(long ledgerId, long entryId, int partitionIndex, int batchIndex, int batchSize, BatchMessageAcker acker) : base(ledgerId, entryId, partitionIndex)
		{
			BatchIndex = batchIndex;
			_batchSize = batchSize;
			Acker = acker;
		}

		public BatchMessageId(MessageId other) : base(other.LedgerId, other.EntryId, other.PartitionIndex)
		{
			if (other is BatchMessageId otherId)
			{
                BatchIndex = otherId.BatchIndex;
				_batchSize = otherId.BatchSize;
				Acker = otherId.Acker;
			}
			else
			{
				BatchIndex = NoBatch;
				_batchSize = 0;
				Acker = BatchMessageAckerDisabled.Instance;
			}
		}

		public virtual int CompareTo(object o)
        {
            if (o is BatchMessageId other)
			{
                return Compare(other);
            }

            if (o is MessageId id)
            {
                int res = base.CompareTo(id);
                if (res == 0 && BatchIndex > NoBatch)
                {
                    return 1;
                }

                return res;
            }
            else if (o is TopicMessageId)
            {
                return CompareTo(((TopicMessageId) o).InnerMessageId);
            }
            else
            {
                throw new ArgumentException("expected BatchMessageId object. Got instance of " + o.GetType().FullName);
            }
        }

		public override int GetHashCode()
		{
			return (int)(31 * (LedgerId + 31 * EntryId) + (31 * PartitionIndex) + BatchIndex);
		}

		public override bool Equals(object obj)
		{
			if (obj is BatchMessageId other1)
			{
                return LedgerId == other1.LedgerId && EntryId == other1.EntryId && PartitionIndex == other1.PartitionIndex && BatchIndex == other1.BatchIndex && BatchSize == other1.BatchSize;
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
		public override sbyte[] ToByteArray()
		{
			return ToByteArray(BatchIndex);
			//return ToByteArray(BatchIndex, BatchSize);
		}

		public virtual bool AckIndividual()
		{
			return Acker.AckIndividual(BatchIndex);
		}

		public virtual bool AckCumulative()
		{
			return Acker.AckCumulative(BatchIndex);
		}
		public virtual bool AckCumulative(int batchsize)
		{
			return Acker.AckCumulative(batchsize);
		}

		public virtual int OutstandingAcksInSameBatch => Acker.OutstandingAcks;

        public virtual int BatchSize => Acker.BatchSize;

        public virtual MessageId PrevBatchMessageId()
		{
			return new MessageId(LedgerId, EntryId - 1, PartitionIndex);
		}

        private int Compare(BatchMessageId m)
        {
            var ledgercompare = LedgerId.CompareTo(m.LedgerId);
            if (ledgercompare != 0)
                return ledgercompare;

            var entryCompare = EntryId.CompareTo(m.EntryId);
            if (entryCompare != 0)
                return entryCompare;

			var batchCompare = BatchIndex.CompareTo(m.BatchIndex);
			if (batchCompare != 0)
				return batchCompare;

			var partitionCompare = PartitionIndex.CompareTo(m.PartitionIndex);
            if (partitionCompare != 0)
                return partitionCompare;

            return 0;
        }

	}

}