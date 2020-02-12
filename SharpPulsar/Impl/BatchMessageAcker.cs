using System.Collections;

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
	
	public class BatchMessageAcker
	{

		internal static BatchMessageAcker NewAcker(int batchSize)
		{
			var bitSet = new BitArray(batchSize);
			bitSet.Set(batchSize, true);
			return new BatchMessageAcker(bitSet, batchSize);
		}

		// bitset shared across messages in the same batch.
		private int _batchSize;
		private readonly BitArray bitSet;

        public BatchMessageAcker(BitArray bitSet, int batchSize)
		{
			this.bitSet = bitSet;
			_batchSize = batchSize;
		}

		public virtual BitArray BitSet => bitSet;

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

		public virtual bool AckIndividual(int batchIndex)
		{
			lock (this)
			{
				bitSet.Set(batchIndex, false);
				return bitSet.Get(batchIndex);
			}
		}

		public virtual bool AckCumulative(int batchIndex)
		{
			lock (this)
			{
				// +1 since to argument is exclusive
				//ORIGINAL FROM JAVA
				//bitSet.Clear(0, BatchIndex + 1);
				//return bitSet.Empty;
                bitSet.Set(batchIndex + 1, false);
                return bitSet.Get(batchIndex);
			}
		}

		// debug purpose
		public virtual int OutstandingAcks
		{
			get
			{
				lock (this)
				{
					return bitSet.Count;
				}
			}
		}

		public virtual bool PrevBatchCumulativelyAcked { set; get; } = false;
    }

}