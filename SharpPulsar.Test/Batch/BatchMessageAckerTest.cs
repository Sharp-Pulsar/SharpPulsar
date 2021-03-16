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

using SharpPulsar.Batch;
using Xunit;

namespace SharpPulsar.Test.Batch
{
    [Collection("SharpPulsar")]
    public class BatchMessageAckerTest
    {

        private const int BatchSize = 10;

        private BatchMessageAcker acker;

        public BatchMessageAckerTest()
        {
            acker = BatchMessageAcker.NewAcker(10);
        }

        [Fact]
        public void TestAckers()
        {
            Assert.Equal(BatchSize, acker.OutstandingAcks);
            Assert.Equal(BatchSize, acker.BatchSize);

            Assert.False(acker.AckIndividual(4));
            for (var i = 0; i < BatchSize; i++)
            {
                if (4 == i)
                {
                    Assert.False(acker.BitSet.Get(i));
                }
                else
                {
                    Assert.True(acker.BitSet.Get(i));
                }
            }

            Assert.False(acker.AckCumulative(6));
            for (var i = 0; i < BatchSize; i++)
            {
                if (i <= 6)
                {
                    Assert.False(acker.BitSet.Get(i));
                }
                else
                {
                    Assert.True(acker.BitSet.Get(i));
                }
            }

            for (var i = BatchSize - 1; i >= 8; i--)
            {
                Assert.False(acker.AckIndividual(i));
                Assert.False(acker.BitSet.Get(i));
            }

            Assert.True(acker.AckIndividual(7));
            Assert.Equal(0, acker.OutstandingAcks);
        }

    }

}