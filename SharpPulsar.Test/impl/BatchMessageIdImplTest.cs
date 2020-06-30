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
using SharpPulsar.Impl;
using Xunit;

namespace SharpPulsar.Test.Impl
{
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
		public virtual void HashCodeTest()
		{
			var batchMsgId1 = new BatchMessageId(0, 0, 0, 0);
			var batchMsgId2 = new BatchMessageId(1, 1, 1, 1);

			Assert.Equal(batchMsgId1.GetHashCode(), batchMsgId1.GetHashCode());
			Assert.True(batchMsgId1.GetHashCode() != batchMsgId2.GetHashCode());
		}
		[Fact]
		public virtual void EqualsTest()
		{
			var batchMsgId1 = new BatchMessageId(0, 0, 0, 0);
			var batchMsgId2 = new BatchMessageId(1, 1, 1, 1);
			var batchMsgId3 = new BatchMessageId(0, 0, 0, 1);
			var batchMsgId4 = new BatchMessageId(0, 0, 0, -1);
			var msgId = new MessageId(0, 0, 0);

			Assert.Equal(batchMsgId1, batchMsgId1);
			Assert.NotEqual(batchMsgId1, batchMsgId2);
			Assert.NotEqual(batchMsgId1, batchMsgId3);
			Assert.NotEqual(batchMsgId1, batchMsgId4);
			Assert.NotEqual(batchMsgId1, msgId);

			Assert.Equal(msgId, msgId);
			Assert.NotEqual(msgId, batchMsgId1);
			Assert.NotEqual(msgId, batchMsgId2);
			Assert.NotEqual(msgId, batchMsgId3);
			Assert.Equal(msgId, batchMsgId4);

			Assert.Equal(msgId, batchMsgId4);
		}

	}

}