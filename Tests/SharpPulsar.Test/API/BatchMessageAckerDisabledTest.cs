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
using SharpPulsar.Test.Fixture;
using Xunit;

namespace SharpPulsar.Test.API
{
    [Collection(nameof(IntegrationCollection))]
    public class BatchMessageAckerDisabledTest
    {
        [Fact]
        public void TestAckIndividual()
        {
            for (var i = 0; i < 10; i++)
            {
                Assert.True(BatchMessageAckerDisabled.Instance.AckIndividual(i));
            }
        }
        [Fact]
        public void TestAckCumulative()
        {
            for (var i = 0; i < 10; i++)
            {
                Assert.True(BatchMessageAckerDisabled.Instance.AckCumulative(i));
            }
        }
        [Fact]
        public void TestGetOutstandingAcks()
        {
            Assert.Equal(0, BatchMessageAckerDisabled.Instance.OutstandingAcks);
        }
    }

}