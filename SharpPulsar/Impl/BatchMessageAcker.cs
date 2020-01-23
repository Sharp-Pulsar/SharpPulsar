﻿using System.Collections;

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
	using VisibleForTesting = com.google.common.annotations.VisibleForTesting;

	public class BatchMessageAcker
	{

		internal static BatchMessageAcker NewAcker(int BatchSize)
		{
			BitArray BitSet = new BitArray(BatchSize);
			BitSet.Set(0, BatchSize);
			return new BatchMessageAcker(BitSet, BatchSize);
		}

		// bitset shared across messages in the same batch.
		public virtual BatchSize {get;}
		private readonly BitArray bitSet;
		private bool prevBatchCumulativelyAcked = false;

		public BatchMessageAcker(BitArray BitSet, int BatchSize)
		{
			this.bitSet = BitSet;
			this.BatchSize = BatchSize;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @VisibleForTesting BitSet getBitSet()
		public virtual BitArray BitSet
		{
			get
			{
				return bitSet;
			}
		}

		public virtual int BatchSize
		{
			get
			{
				lock (this)
				{
					return BatchSize;
				}
			}
		}

		public virtual bool AckIndividual(int BatchIndex)
		{
			lock (this)
			{
				bitSet.Set(BatchIndex, false);
				return bitSet.Empty;
			}
		}

		public virtual bool AckCumulative(int BatchIndex)
		{
			lock (this)
			{
				// +1 since to argument is exclusive
				bitSet.clear(0, BatchIndex + 1);
				return bitSet.Empty;
			}
		}

		// debug purpose
		public virtual int OutstandingAcks
		{
			get
			{
				lock (this)
				{
					return bitSet.cardinality();
				}
			}
		}

		public virtual bool PrevBatchCumulativelyAcked
		{
			set
			{
				this.prevBatchCumulativelyAcked = value;
			}
			get
			{
				return prevBatchCumulativelyAcked;
			}
		}


	}

}