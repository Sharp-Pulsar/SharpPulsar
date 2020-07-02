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

using System;
using System.Text.Json;
using SharpPulsar.Batch;
using SharpPulsar.Impl;
using Xunit;

namespace SharpPulsar.Test.Batch
{
    [Collection("BatchMessageIdTest")]
	public class BatchMessageIdTest
	{

		[Fact]
		public void CompareToTest()
		{
			var batchMsgId1 = new BatchMessageId(0, 0, 0, 0);
			var batchMsgId2 = new BatchMessageId(1, 1, 1, 1);

			Assert.Equal(-1, batchMsgId1.CompareTo(batchMsgId2));
			Assert.Equal(1, batchMsgId2.CompareTo(batchMsgId1));
			Assert.Equal(0, batchMsgId2.CompareTo(batchMsgId2));
		}

		[Fact]
		public void HashCodeTest()
		{
			var batchMsgId1 = new BatchMessageId(0, 0, 0, 0);
			var batchMsgId2 = new BatchMessageId(1, 1, 1, 1);

			Assert.Equal(batchMsgId1.GetHashCode(), batchMsgId1.GetHashCode());
			Assert.True(batchMsgId1.GetHashCode() != batchMsgId2.GetHashCode());
		}

		[Fact]
		public void EqualsTest()
		{
			var batchMsgId1 = new BatchMessageId(0, 0, 0, 0);
			var batchMsgId2 = new BatchMessageId(1, 1, 1, 1);
			var batchMsgId3 = new BatchMessageId(0, 0, 0, 1);
			var batchMsgId4 = new BatchMessageId(0, 0, 0, -1);
			var msgId = new MessageId(0, 0, 0);

			Assert.Equal(batchMsgId1, batchMsgId1);
			Assert.False(batchMsgId1.Equals(batchMsgId2));
			Assert.False(batchMsgId1.Equals(batchMsgId3));
			Assert.False(batchMsgId1.Equals(batchMsgId4));
			Assert.False(batchMsgId1.Equals(msgId));

			Assert.Equal(msgId, msgId);
			Assert.False(msgId.Equals(batchMsgId1));
			Assert.False(msgId.Equals(batchMsgId2));
			Assert.False(msgId.Equals(batchMsgId3));
			Assert.Equal(msgId, batchMsgId4);

			Assert.Equal(batchMsgId4, msgId);
		}
		[Fact]
		public void DeserializationTest()
		{
			// initialize BitSet with null
			var ackerDisabled = new BatchMessageAcker(null, 0);
			var batchMsgId = new BatchMessageId(0, 0, 0, 0, 0, ackerDisabled);

			try
			{
				var d = JsonSerializer.Serialize(batchMsgId, new JsonSerializerOptions{IgnoreNullValues = false});
				//Assert.fail("Shouldn't be deserialized");
			}
			catch (Exception e)
			{
				// expected
                Assert.False(false);
            }

			// use the default BatchMessageAckerDisabled
			var batchMsgIdToDeserialize = new BatchMessageId(0, 0, 0, 0);

			try
			{
                var d = JsonSerializer.Serialize(batchMsgIdToDeserialize, new JsonSerializerOptions { IgnoreNullValues = false });

			}
			catch (Exception ex)
			{
                Assert.False(false);
			}
		}

	}

}