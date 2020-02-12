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

	[Serializable]
	public class BatchMessageIdImpl : MessageIdImpl
	{
		private const int NoBatch = -1;
		public int BatchIndex {get;}

		public BatchMessageAcker Acker {get;}

		public BatchMessageIdImpl(long LedgerId, long EntryId, int PartitionIndex, int BatchIndex) : this(LedgerId, EntryId, PartitionIndex, BatchIndex, BatchMessageAckerDisabled.Instance)
		{
		}

		public BatchMessageIdImpl(long LedgerId, long EntryId, int PartitionIndex, int BatchIndex, BatchMessageAcker Acker) : base(LedgerId, EntryId, PartitionIndex)
		{
			this.BatchIndex = BatchIndex;
			this.Acker = Acker;
		}

		public BatchMessageIdImpl(MessageIdImpl Other) : base(Other.LedgerIdConflict, Other.EntryIdConflict, Other.PartitionIndexConflict)
		{
			if (Other is BatchMessageIdImpl)
			{
				BatchMessageIdImpl OtherId = (BatchMessageIdImpl) Other;
				this.BatchIndex = OtherId.BatchIndex;
				this.Acker = OtherId.Acker;
			}
			else
			{
				this.BatchIndex = NoBatch;
				this.Acker = BatchMessageAckerDisabled.Instance;
			}
		}

		public override int GetHashCode()
		{
			return (int)(31 * (LedgerIdConflict + 31 * EntryIdConflict) + (31 * PartitionIndexConflict) + BatchIndex);
		}

		public override bool Equals(object Obj)
		{
			if (Obj is BatchMessageIdImpl)
			{
				BatchMessageIdImpl Other = (BatchMessageIdImpl) Obj;
				return LedgerIdConflict == Other.LedgerIdConflict && EntryIdConflict == Other.EntryIdConflict && PartitionIndexConflict == Other.PartitionIndexConflict && BatchIndex == Other.BatchIndex;
			}
			else if (Obj is MessageIdImpl)
			{
				MessageIdImpl Other = (MessageIdImpl) Obj;
				return LedgerIdConflict == Other.LedgerIdConflict && EntryIdConflict == Other.EntryIdConflict && PartitionIndexConflict == Other.PartitionIndexConflict && BatchIndex == NoBatch;
			}
			return false;
		}

		public override string ToString()
		{
			return string.Format("{0:D}:{1:D}:{2:D}:{3:D}", LedgerIdConflict, EntryIdConflict, PartitionIndexConflict, BatchIndex);
		}

		// Serialization
		public override sbyte[] ToByteArray()
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

		public virtual int OutstandingAcksInSameBatch
		{
			get
			{
				return Acker.OutstandingAcks;
			}
		}

		public virtual int BatchSize
		{
			get
			{
				return Acker.BatchSize;
			}
		}

		public virtual MessageIdImpl PrevBatchMessageId()
		{
			return new MessageIdImpl(LedgerIdConflict, EntryIdConflict - 1, PartitionIndexConflict);
		}


	}

}