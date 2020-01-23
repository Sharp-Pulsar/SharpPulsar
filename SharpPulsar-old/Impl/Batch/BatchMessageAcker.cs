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
namespace SharpPulsar.Impl.Batch
{

	public class BatchMessageAcker
	{

		internal static BatchMessageAcker NewAcker(int batchSize)
		{
			BitArray bitSet = new BitArray(batchSize);
			bitSet.Set(0, batchSize);
			return new BatchMessageAcker(bitSet, batchSize);
		}

		// bitset shared across messages in the same batch.
		private readonly int batchSize;
		private readonly BitArray bitSet;
		private bool prevBatchCumulativelyAcked = false;

		internal BatchMessageAcker(BitArray bitSet, int batchSize)
		{
			this.bitSet = bitSet;
			this.batchSize = batchSize;
		}

		internal virtual BitArray BitSet
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
					return batchSize;
				}
			}
		}

		public virtual bool AckIndividual(int batchIndex)
		{
			lock (this)
			{
				bitSet.Set(batchIndex, false);
				return bitSet.Count > 1;
			}
		}

		public virtual bool ackCumulative(int batchIndex)
		{
			lock (this)
			{
				// +1 since to argument is exclusive
				bitSet.Clear(0, batchIndex + 1);
				return bitSet.Count > 1;
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