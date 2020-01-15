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
namespace org.apache.pulsar.client.impl
{
	using ComparisonChain = com.google.common.collect.ComparisonChain;
	using MessageId = org.apache.pulsar.client.api.MessageId;

	public class BatchMessageIdImpl : MessageIdImpl
	{
		private const int NO_BATCH = -1;
		private readonly int batchIndex;

		private readonly BatchMessageAcker acker;

		public BatchMessageIdImpl(long ledgerId, long entryId, int partitionIndex, int batchIndex) : this(ledgerId, entryId, partitionIndex, batchIndex, BatchMessageAckerDisabled.INSTANCE)
		{
		}

		public BatchMessageIdImpl(long ledgerId, long entryId, int partitionIndex, int batchIndex, BatchMessageAcker acker) : base(ledgerId, entryId, partitionIndex)
		{
			this.batchIndex = batchIndex;
			this.acker = acker;
		}

		public BatchMessageIdImpl(MessageIdImpl other) : base(other.ledgerId, other.entryId, other.partitionIndex)
		{
			if (other is BatchMessageIdImpl)
			{
				BatchMessageIdImpl otherId = (BatchMessageIdImpl) other;
				this.batchIndex = otherId.batchIndex;
				this.acker = otherId.acker;
			}
			else
			{
				this.batchIndex = NO_BATCH;
				this.acker = BatchMessageAckerDisabled.INSTANCE;
			}
		}

		public virtual int BatchIndex
		{
			get
			{
				return batchIndex;
			}
		}

		public override int compareTo(MessageId o)
		{
			if (o is BatchMessageIdImpl)
			{
				BatchMessageIdImpl other = (BatchMessageIdImpl) o;
				return ComparisonChain.start().compare(this.ledgerId, other.ledgerId).compare(this.entryId, other.entryId).compare(this.batchIndex, other.batchIndex).compare(this.PartitionIndex, other.PartitionIndex).result();
			}
			else if (o is MessageIdImpl)
			{
				int res = base.compareTo(o);
				if (res == 0 && batchIndex > NO_BATCH)
				{
					return 1;
				}
				else
				{
					return res;
				}
			}
			else if (o is TopicMessageIdImpl)
			{
				return compareTo(((TopicMessageIdImpl) o).InnerMessageId);
			}
			else
			{
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
				throw new System.ArgumentException("expected BatchMessageIdImpl object. Got instance of " + o.GetType().FullName);
			}
		}

		public override int GetHashCode()
		{
			return (int)(31 * (ledgerId + 31 * entryId) + (31 * partitionIndex) + batchIndex);
		}

		public override bool Equals(object obj)
		{
			if (obj is BatchMessageIdImpl)
			{
				BatchMessageIdImpl other = (BatchMessageIdImpl) obj;
				return ledgerId == other.ledgerId && entryId == other.entryId && partitionIndex == other.partitionIndex && batchIndex == other.batchIndex;
			}
			else if (obj is MessageIdImpl)
			{
				MessageIdImpl other = (MessageIdImpl) obj;
				return ledgerId == other.ledgerId && entryId == other.entryId && partitionIndex == other.partitionIndex && batchIndex == NO_BATCH;
			}
			return false;
		}

		public override string ToString()
		{
			return string.Format("{0:D}:{1:D}:{2:D}:{3:D}", ledgerId, entryId, partitionIndex, batchIndex);
		}

		// Serialization
		public override sbyte[] toByteArray()
		{
			return toByteArray(batchIndex);
		}

		public virtual bool ackIndividual()
		{
			return acker.ackIndividual(batchIndex);
		}

		public virtual bool ackCumulative()
		{
			return acker.ackCumulative(batchIndex);
		}

		public virtual int OutstandingAcksInSameBatch
		{
			get
			{
				return acker.OutstandingAcks;
			}
		}

		public virtual int BatchSize
		{
			get
			{
				return acker.BatchSize;
			}
		}

		public virtual MessageIdImpl prevBatchMessageId()
		{
			return new MessageIdImpl(ledgerId, entryId - 1, partitionIndex);
		}

		public virtual BatchMessageAcker Acker
		{
			get
			{
				return acker;
			}
		}

	}

}