using SharpPulsar.Batch;
using SharpPulsar.Interfaces;
using System.Collections.Generic;
using Xunit;

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
namespace SharpPulsar.Test
{


	/// <summary>
	/// Test compareTo method in MessageId and BatchMessageId
	/// </summary>
	public class MessageIdCompareToTest
	{
		[Fact]
		public virtual void TestEqual()
		{
			MessageId MessageId1 = new MessageId(123L, 345L, 567);
			MessageId MessageId2 = new MessageId(123L, 345L, 567);

			BatchMessageId batchMessageId1 = new BatchMessageId(234L, 345L, 456, 567);
			BatchMessageId batchMessageId2 = new BatchMessageId(234L, 345L, 456, 567);

			Assert.Equal(0, MessageId1.CompareTo(MessageId2));
			Assert.Equal(0, batchMessageId1.CompareTo(batchMessageId2));
		}
		[Fact]
		public virtual void TestGreaterThan()
		{
			MessageId MessageId1 = new MessageId(124L, 345L, 567);
			MessageId MessageId2 = new MessageId(123L, 345L, 567);
			MessageId MessageId3 = new MessageId(123L, 344L, 567);
			MessageId MessageId4 = new MessageId(123L, 344L, 566);

			BatchMessageId batchMessageId1 = new BatchMessageId(235L, 345L, 456, 567);
			BatchMessageId batchMessageId2 = new BatchMessageId(234L, 346L, 456, 567);
			BatchMessageId batchMessageId3 = new BatchMessageId(234L, 345L, 456, 568);
			BatchMessageId batchMessageId4 = new BatchMessageId(234L, 345L, 457, 567);
			BatchMessageId batchMessageId5 = new BatchMessageId(234L, 345L, 456, 567);

			Assert.True(MessageId1.CompareTo(MessageId2) > 0, "Expected to be greater than");
			Assert.True(MessageId1.CompareTo(MessageId3) > 0, "Expected to be greater than");
			Assert.True(MessageId1.CompareTo(MessageId4) > 0, "Expected to be greater than");
			Assert.True(MessageId2.CompareTo(MessageId3) > 0, "Expected to be greater than");
			Assert.True(MessageId2.CompareTo(MessageId4) > 0, "Expected to be greater than");
			Assert.True(MessageId3.CompareTo(MessageId4) > 0, "Expected to be greater than");

			Assert.True(batchMessageId1.CompareTo(batchMessageId2) > 0, "Expected to be greater than");
			Assert.True(batchMessageId1.CompareTo(batchMessageId3) > 0, "Expected to be greater than");
			Assert.True(batchMessageId1.CompareTo(batchMessageId4) > 0, "Expected to be greater than");
			Assert.True(batchMessageId1.CompareTo(batchMessageId5) > 0, "Expected to be greater than");
			Assert.True(batchMessageId2.CompareTo(batchMessageId3) > 0, "Expected to be greater than");
			Assert.True(batchMessageId2.CompareTo(batchMessageId4) > 0, "Expected to be greater than");
			Assert.True(batchMessageId2.CompareTo(batchMessageId5) > 0, "Expected to be greater than");
			Assert.True(batchMessageId3.CompareTo(batchMessageId4) > 0, "Expected to be greater than");
			Assert.True(batchMessageId3.CompareTo(batchMessageId5) > 0, "Expected to be greater than");
			Assert.True(batchMessageId4.CompareTo(batchMessageId5) > 0, "Expected to be greater than");
		}
		[Fact]
		public virtual void TestLessThan()
		{
			MessageId MessageId1 = new MessageId(124L, 345L, 567);
			MessageId MessageId2 = new MessageId(123L, 345L, 567);
			MessageId MessageId3 = new MessageId(123L, 344L, 567);
			MessageId MessageId4 = new MessageId(123L, 344L, 566);

			BatchMessageId batchMessageId1 = new BatchMessageId(235L, 345L, 456, 567);
			BatchMessageId batchMessageId2 = new BatchMessageId(234L, 346L, 456, 567);
			BatchMessageId batchMessageId3 = new BatchMessageId(234L, 345L, 456, 568);
			BatchMessageId batchMessageId4 = new BatchMessageId(234L, 345L, 457, 567);
			BatchMessageId batchMessageId5 = new BatchMessageId(234L, 345L, 456, 567);

			Assert.True(MessageId2.CompareTo(MessageId1) < 0, "Expected to be less than");
			Assert.True(MessageId3.CompareTo(MessageId1) < 0, "Expected to be less than");
			Assert.True(MessageId4.CompareTo(MessageId1) < 0, "Expected to be less than");
			Assert.True(MessageId3.CompareTo(MessageId2) < 0, "Expected to be less than");
			Assert.True(MessageId4.CompareTo(MessageId2) < 0, "Expected to be less than");
			Assert.True(MessageId4.CompareTo(MessageId3) < 0, "Expected to be less than");

			Assert.True(batchMessageId2.CompareTo(batchMessageId1) < 0, "Expected to be less than");
			Assert.True(batchMessageId3.CompareTo(batchMessageId1) < 0, "Expected to be less than");
			Assert.True(batchMessageId4.CompareTo(batchMessageId1) < 0, "Expected to be less than");
			Assert.True(batchMessageId5.CompareTo(batchMessageId1) < 0, "Expected to be less than");
			Assert.True(batchMessageId3.CompareTo(batchMessageId2) < 0, "Expected to be less than");
			Assert.True(batchMessageId4.CompareTo(batchMessageId2) < 0, "Expected to be less than");
			Assert.True(batchMessageId5.CompareTo(batchMessageId2) < 0, "Expected to be less than");
			Assert.True(batchMessageId4.CompareTo(batchMessageId3) < 0, "Expected to be less than");
			Assert.True(batchMessageId5.CompareTo(batchMessageId3) < 0, "Expected to be less than");
			Assert.True(batchMessageId5.CompareTo(batchMessageId4) < 0, "Expected to be less than");
		}
		[Fact]
		public virtual void TestCompareDifferentType()
		{
			MessageId MessageId = new MessageId(123L, 345L, 567);
			BatchMessageId batchMessageId1 = new BatchMessageId(123L, 345L, 566, 789);
			BatchMessageId batchMessageId2 = new BatchMessageId(123L, 345L, 567, 789);
			BatchMessageId batchMessageId3 = new BatchMessageId(MessageId);
			Assert.True(MessageId.CompareTo(batchMessageId1) > 0, "Expected to be greater than");
			Assert.True(MessageId.CompareTo(batchMessageId2) < 0, "Expected to be less than");
			Assert.Equal(0, MessageId.CompareTo(batchMessageId3));
			Assert.True(batchMessageId1.CompareTo(MessageId) < 0, "Expected to be less than");
			Assert.True(batchMessageId2.CompareTo(MessageId) > 0, "Expected to be greater than");
			Assert.Equal(0, batchMessageId3.CompareTo(MessageId));
		}
		[Fact]
		public virtual void CompareToSymmetricTest()
		{
			MessageId simpleMessageId = new MessageId(123L, 345L, 567);
			// batchIndex is -1 if message is non-batched message and has the batchIndex for a batch message
			BatchMessageId batchMessageId1 = new BatchMessageId(123L, 345L, 567, -1);
			BatchMessageId batchMessageId2 = new BatchMessageId(123L, 345L, 567, 1);
			BatchMessageId batchMessageId3 = new BatchMessageId(123L, 345L, 566, 1);
			BatchMessageId batchMessageId4 = new BatchMessageId(123L, 345L, 566, -1);

			Assert.Equal(0, simpleMessageId.CompareTo(batchMessageId1));
			Assert.Equal(0, batchMessageId1.CompareTo(simpleMessageId));
			Assert.True(batchMessageId2.CompareTo(simpleMessageId) > 0, "Expected to be greater than");
			Assert.True(simpleMessageId.CompareTo(batchMessageId2) < 0, "Expected to be less than");
			Assert.True(simpleMessageId.CompareTo(batchMessageId3) > 0, "Expected to be greater than");
			Assert.True(batchMessageId3.CompareTo(simpleMessageId) < 0, "Expected to be less than");
			Assert.True(simpleMessageId.CompareTo(batchMessageId4) > 0, "Expected to be greater than");
			Assert.True(batchMessageId4.CompareTo(simpleMessageId) < 0, "Expected to be less than");
		}
		[Fact]
		public virtual void TestMessageIdCompareToTopicMessageId()
		{
			MessageId MessageId = new MessageId(123L, 345L, 567);
			TopicMessageId topicMessageId1 = new TopicMessageId("test-topic-partition-0", "test-topic", new BatchMessageId(123L, 345L, 566, 789));
			TopicMessageId topicMessageId2 = new TopicMessageId("test-topic-partition-0", "test-topic", new BatchMessageId(123L, 345L, 567, 789));
			TopicMessageId topicMessageId3 = new TopicMessageId("test-topic-partition-0", "test-topic", new BatchMessageId(MessageId));
			Assert.True(MessageId.CompareTo(topicMessageId1) > 0, "Expected to be greater than");
			Assert.True(MessageId.CompareTo(topicMessageId2) < 0, "Expected to be less than");
			Assert.Equal(0, MessageId.CompareTo(topicMessageId3));
			Assert.True(topicMessageId1.CompareTo(MessageId) < 0, "Expected to be less than");
			Assert.True(topicMessageId2.CompareTo(MessageId) > 0, "Expected to be greater than");
			Assert.Equal(0, topicMessageId3.CompareTo(MessageId));
		}
		[Fact]
		public virtual void TestBatchMessageIdCompareToTopicMessageId()
		{
			BatchMessageId MessageId1 = new BatchMessageId(123L, 345L, 567, 789);
			BatchMessageId MessageId2 = new BatchMessageId(123L, 345L, 567, 0);
			BatchMessageId MessageId3 = new BatchMessageId(123L, 345L, 567, -1);
			TopicMessageId topicMessageId1 = new TopicMessageId("test-topic-partition-0", "test-topic", new MessageId(123L, 345L, 566));
			TopicMessageId topicMessageId2 = new TopicMessageId("test-topic-partition-0", "test-topic", new MessageId(123L, 345L, 567));
			Assert.True(MessageId1.CompareTo(topicMessageId1) > 0, "Expected to be greater than");
			Assert.True(MessageId1.CompareTo(topicMessageId2) > 0, "Expected to be greater than");
			Assert.True(MessageId2.CompareTo(topicMessageId2) > 0, "Expected to be greater than");
			Assert.Equal(0, MessageId3.CompareTo(topicMessageId2));
			Assert.True(topicMessageId1.CompareTo(MessageId1) < 0, "Expected to be less than");
			Assert.True(topicMessageId2.CompareTo(MessageId1) < 0, "Expected to be less than");
			Assert.True(topicMessageId2.CompareTo(MessageId2) < 0, "Expected to be less than");
			Assert.True(topicMessageId2.CompareTo(MessageId2) < 0, "Expected to be less than");
		}
		[Fact]
		public virtual void TestMultiMessageIdEqual()
		{
			// null
			MultiMessageId null1 = new MultiMessageId(null);
			MultiMessageId null2 = new MultiMessageId(null);
			Assert.Equal(null1, null2);

			// empty
			MultiMessageId empty1 = new MultiMessageId(new Dictionary<string, IMessageId>());
			MultiMessageId empty2 = new MultiMessageId(new Dictionary<string, IMessageId>());
			Assert.Equal(empty1, empty2);

			// null empty
			Assert.Equal(null1, empty2);
			Assert.Equal(empty2, null1);

			// 1 item
			string topic1 = "topicName1";
			MessageId MessageId1 = new MessageId(123L, 345L, 567);
			MessageId MessageId2 = new MessageId(123L, 345L, 567);
			MessageId MessageId3 = new MessageId(345L, 456L, 567);

			MultiMessageId item1 = new MultiMessageId(new Dictionary<string, IMessageId> { { topic1, MessageId1 } });
			MultiMessageId item2 = new MultiMessageId(new Dictionary<string, IMessageId> { { topic1, MessageId2 } });
			Assert.Equal(item1, item2);

			// 1 item, empty not equal
			Assert.NotEqual(item1, null1);
			Assert.NotEqual(null1, item1);

			// key not equal
			string topic2 = "topicName2";
			MultiMessageId item3 = new MultiMessageId(new Dictionary<string, IMessageId> { { topic2, MessageId2 } });
			Assert.NotEqual(item1, item3);
			Assert.NotEqual(item3, item1);

			// value not equal
			MultiMessageId item4 = new MultiMessageId(new Dictionary<string, IMessageId> { { topic1, MessageId3 } });
			Assert.NotEqual(item1, item4);
			Assert.NotEqual(item4, item1);

			// key value not equal
			Assert.NotEqual(item3, item4);
			Assert.NotEqual(item4, item3);

			// 2 items
			IDictionary<string, IMessageId> map1 = new Dictionary<string, IMessageId>();
			IDictionary<string, IMessageId> map2 = new Dictionary<string, IMessageId>();
			map1[topic1] = MessageId1;
			map1[topic2] = MessageId2;
			map2[topic2] = MessageId2;
			map2[topic1] = MessageId1;

			MultiMessageId item5 = new MultiMessageId(map1);
			MultiMessageId item6 = new MultiMessageId(map2);

			Assert.Equal(item5, item6);

			Assert.NotEqual(item5, null1);
			Assert.NotEqual(item5, empty1);
			Assert.NotEqual(item5, item1);
			Assert.NotEqual(item5, item3);
			Assert.NotEqual(item5, item4);

			Assert.NotEqual(null1, item5);
			Assert.NotEqual(empty1, item5);
			Assert.NotEqual(item1, item5);
			Assert.NotEqual(item3, item5);
			Assert.NotEqual(item4, item5);

			map2[topic1] = MessageId3;
			MultiMessageId item7 = new MultiMessageId(map2);
			Assert.NotEqual(item5, item7);
			Assert.NotEqual(item7, item5);
		}
		[Fact]
		public virtual void TestMultiMessageIdCompareto()
		{
			// null
			MultiMessageId null1 = new MultiMessageId(null);
			MultiMessageId null2 = new MultiMessageId(null);
			Assert.Equal(0, null1.CompareTo(null2));

			// empty
			MultiMessageId empty1 = new MultiMessageId(new Dictionary<string, IMessageId>());
			MultiMessageId empty2 = new MultiMessageId(new Dictionary<string, IMessageId>());
			Assert.Equal(0, empty1.CompareTo(empty2));

			// null empty
			Assert.Equal(0, null1.CompareTo(empty2));
			Assert.Equal(0, empty2.CompareTo(null1));

			// 1 item
			string topic1 = "topicName1";
			MessageId MessageId1 = new MessageId(123L, 345L, 567);
			MessageId MessageId2 = new MessageId(123L, 345L, 567);
			MessageId MessageId3 = new MessageId(345L, 456L, 567);

			MultiMessageId item1 = new MultiMessageId(new Dictionary<string, IMessageId> { { topic1, MessageId1 } });
			MultiMessageId item2 = new MultiMessageId(new Dictionary<string, IMessageId> { { topic1, MessageId2 } });
			Assert.Equal(0, item1.CompareTo(item2));

			// 1 item, empty not equal
			try
			{
				item1.CompareTo(null1);
				Assert.False(false, "should throw exception for not comparable");
			}
			catch(System.ArgumentException)
			{
				// expected
			}
			try
			{
				null1.CompareTo(item1);
				Assert.False(false, "should throw exception for not comparable");
			}
			catch(System.ArgumentException)
			{
				// expected
			}

			// key not equal
			string topic2 = "topicName2";
			MultiMessageId item3 = new MultiMessageId(new Dictionary<string, IMessageId> { { topic2, MessageId2 } });
			try
			{
				item1.CompareTo(item3);
				Assert.False(false, "should throw exception for not comparable");
			}
			catch(System.ArgumentException)
			{
				// expected
			}
			try
			{
				item3.CompareTo(item1);
				Assert.False(false, "should throw exception for not comparable");
			}
			catch(System.ArgumentException)
			{
				// expected
			}

			// value not equal
			MultiMessageId item4 = new MultiMessageId(new Dictionary<string, IMessageId> { { topic1, MessageId3 } });
			Assert.True(item1.CompareTo(item4) < 0);
			Assert.True(item4.CompareTo(item1) > 0);

			// key value not equal
			try
			{
				item3.CompareTo(item4);
				Assert.False(false, "should throw exception for not comparable");
			}
			catch(System.ArgumentException)
			{
				// expected
			}
			try
			{
				item4.CompareTo(item3);
				Assert.False(false, "should throw exception for not comparable");
			}
			catch(System.ArgumentException)
			{
				// expected
			}

			// 2 items
			IDictionary<string, IMessageId> map1 = new Dictionary<string, IMessageId>();
			IDictionary<string, IMessageId> map2 = new Dictionary<string, IMessageId>();
			map1[topic1] = MessageId1;
			map1[topic2] = MessageId2;
			map2[topic2] = MessageId2;
			map2[topic1] = MessageId1;

			MultiMessageId item5 = new MultiMessageId(map1);
			MultiMessageId item6 = new MultiMessageId(map2);

			Assert.True(item5.CompareTo(item6) == 0);

			try
			{
				item5.CompareTo(null1);
				Assert.False(false, "should throw exception for not comparable");
			}
			catch(System.ArgumentException)
			{
				// expected
			}

			try
			{
				item5.CompareTo(empty1);
				Assert.False(false, "should throw exception for not comparable");
			}
			catch(System.ArgumentException)
			{
				// expected
			}

			try
			{
				item5.CompareTo(item1);
				Assert.False(false, "should throw exception for not comparable");
			}
			catch(System.ArgumentException)
			{
				// expected
			}

			try
			{
				item5.CompareTo(item3);
				Assert.False(false, "should throw exception for not comparable");
			}
			catch(System.ArgumentException)
			{
				// expected
			}

			try
			{
				item5.CompareTo(item4);
				Assert.False(false, "should throw exception for not comparable");
			}
			catch(System.ArgumentException)
			{
				// expected
			}

			map2[topic1] = MessageId3;
			MultiMessageId item7 = new MultiMessageId(map2);

			Assert.True(item7.CompareTo(item5) > 0);
			Assert.True(item5.CompareTo(item7) < 0);

			IDictionary<string, IMessageId> map3 = new Dictionary<string, IMessageId>();
			map3[topic1] = MessageId3;
			map3[topic2] = MessageId3;
			MultiMessageId item8 = new MultiMessageId(map3);
			Assert.True(item8.CompareTo(item5) > 0);
			Assert.True(item8.CompareTo(item7) > 0);

			Assert.True(item5.CompareTo(item8) < 0);
			Assert.True(item7.CompareTo(item8) < 0);
		}
	}

}