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
namespace SharpPulsar.Batch
{

    public class BatchMessageAcker
	{
		public static BatchMessageAcker NewAcker(int batchSize)
		{
			var bitSet = new BitArray(batchSize, true);
			//bitSet.Set(batchSize-1, false);
			return new BatchMessageAcker(bitSet, batchSize);
		}
        public static BatchMessageAcker NewAcker(BitArray bitSet)
        {
            return new BatchMessageAcker(bitSet, -1);
        }
        // bitset shared across messages in the same batch.
        private readonly BitArray _bitSet;
        private int _unackedCount;


        public BatchMessageAcker(BitArray bitSet, int batchSize)
		{
			_bitSet = bitSet;
			BatchSize = batchSize;
            _unackedCount = batchSize;
		}

		public virtual BitArray BitSet => _bitSet;

		public virtual int BatchSize { get; }

		public virtual bool AckIndividual(int batchIndex)
		{
            var previous = _bitSet[batchIndex];
            if (previous)
            {
                _bitSet.Set(batchIndex, false);
                _unackedCount = _unackedCount - 1;
            }
            return _unackedCount == 0;
		}

		public virtual bool AckCumulative(int batchIndex)
		{
			// +1 since to argument is exclusive
            for(var i = 0; i < batchIndex; i++)
            {
                if(_bitSet[i])
                {
                    _bitSet[i] = false;
                    _unackedCount = _unackedCount - 1;
                }
            }
            return _unackedCount == 0;
        }

		// debug purpose
		public virtual int OutstandingAcks => _unackedCount;

		public virtual bool PrevBatchCumulativelyAcked { set; get; } = false;

        public override string ToString()
        {
            return "BatchMessageAcker{" + "batchSize=" + BatchSize + ", bitSet=" + _bitSet + ", prevBatchCumulativelyAcked=" + PrevBatchCumulativelyAcked + '}';
        }

    }

}