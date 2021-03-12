using System.Collections.Generic;

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
namespace Org.Apache.Pulsar.Client.Impl
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.testng.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.testng.Assert.assertNotEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.testng.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.testng.Assert.fail;

	using Maps = com.google.common.collect.Maps;
	using MessageId = Org.Apache.Pulsar.Client.Api.MessageId;
	using Test = org.testng.annotations.Test;

	/// <summary>
	/// Test compareTo method in MessageIdImpl and BatchMessageIdImpl
	/// </summary>
	public class MessageIdCompareToTest
	{

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testEqual()
		public virtual void TestEqual()
		{
			MessageIdImpl messageIdImpl1 = new MessageIdImpl(123L, 345L, 567);
			MessageIdImpl messageIdImpl2 = new MessageIdImpl(123L, 345L, 567);

			BatchMessageIdImpl batchMessageId1 = new BatchMessageIdImpl(234L, 345L, 456, 567);
			BatchMessageIdImpl batchMessageId2 = new BatchMessageIdImpl(234L, 345L, 456, 567);

			assertEquals(messageIdImpl1.CompareTo(messageIdImpl2), 0, "Expected to be equal");
			assertEquals(batchMessageId1.CompareTo(batchMessageId2), 0, "Expected to be equal");
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testGreaterThan()
		public virtual void TestGreaterThan()
		{
			MessageIdImpl messageIdImpl1 = new MessageIdImpl(124L, 345L, 567);
			MessageIdImpl messageIdImpl2 = new MessageIdImpl(123L, 345L, 567);
			MessageIdImpl messageIdImpl3 = new MessageIdImpl(123L, 344L, 567);
			MessageIdImpl messageIdImpl4 = new MessageIdImpl(123L, 344L, 566);

			BatchMessageIdImpl batchMessageId1 = new BatchMessageIdImpl(235L, 345L, 456, 567);
			BatchMessageIdImpl batchMessageId2 = new BatchMessageIdImpl(234L, 346L, 456, 567);
			BatchMessageIdImpl batchMessageId3 = new BatchMessageIdImpl(234L, 345L, 456, 568);
			BatchMessageIdImpl batchMessageId4 = new BatchMessageIdImpl(234L, 345L, 457, 567);
			BatchMessageIdImpl batchMessageId5 = new BatchMessageIdImpl(234L, 345L, 456, 567);

			assertTrue(messageIdImpl1.CompareTo(messageIdImpl2) > 0, "Expected to be greater than");
			assertTrue(messageIdImpl1.CompareTo(messageIdImpl3) > 0, "Expected to be greater than");
			assertTrue(messageIdImpl1.CompareTo(messageIdImpl4) > 0, "Expected to be greater than");
			assertTrue(messageIdImpl2.CompareTo(messageIdImpl3) > 0, "Expected to be greater than");
			assertTrue(messageIdImpl2.CompareTo(messageIdImpl4) > 0, "Expected to be greater than");
			assertTrue(messageIdImpl3.CompareTo(messageIdImpl4) > 0, "Expected to be greater than");

			assertTrue(batchMessageId1.CompareTo(batchMessageId2) > 0, "Expected to be greater than");
			assertTrue(batchMessageId1.CompareTo(batchMessageId3) > 0, "Expected to be greater than");
			assertTrue(batchMessageId1.CompareTo(batchMessageId4) > 0, "Expected to be greater than");
			assertTrue(batchMessageId1.CompareTo(batchMessageId5) > 0, "Expected to be greater than");
			assertTrue(batchMessageId2.CompareTo(batchMessageId3) > 0, "Expected to be greater than");
			assertTrue(batchMessageId2.CompareTo(batchMessageId4) > 0, "Expected to be greater than");
			assertTrue(batchMessageId2.CompareTo(batchMessageId5) > 0, "Expected to be greater than");
			assertTrue(batchMessageId3.CompareTo(batchMessageId4) > 0, "Expected to be greater than");
			assertTrue(batchMessageId3.CompareTo(batchMessageId5) > 0, "Expected to be greater than");
			assertTrue(batchMessageId4.CompareTo(batchMessageId5) > 0, "Expected to be greater than");
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testLessThan()
		public virtual void TestLessThan()
		{
			MessageIdImpl messageIdImpl1 = new MessageIdImpl(124L, 345L, 567);
			MessageIdImpl messageIdImpl2 = new MessageIdImpl(123L, 345L, 567);
			MessageIdImpl messageIdImpl3 = new MessageIdImpl(123L, 344L, 567);
			MessageIdImpl messageIdImpl4 = new MessageIdImpl(123L, 344L, 566);

			BatchMessageIdImpl batchMessageId1 = new BatchMessageIdImpl(235L, 345L, 456, 567);
			BatchMessageIdImpl batchMessageId2 = new BatchMessageIdImpl(234L, 346L, 456, 567);
			BatchMessageIdImpl batchMessageId3 = new BatchMessageIdImpl(234L, 345L, 456, 568);
			BatchMessageIdImpl batchMessageId4 = new BatchMessageIdImpl(234L, 345L, 457, 567);
			BatchMessageIdImpl batchMessageId5 = new BatchMessageIdImpl(234L, 345L, 456, 567);

			assertTrue(messageIdImpl2.CompareTo(messageIdImpl1) < 0, "Expected to be less than");
			assertTrue(messageIdImpl3.CompareTo(messageIdImpl1) < 0, "Expected to be less than");
			assertTrue(messageIdImpl4.CompareTo(messageIdImpl1) < 0, "Expected to be less than");
			assertTrue(messageIdImpl3.CompareTo(messageIdImpl2) < 0, "Expected to be less than");
			assertTrue(messageIdImpl4.CompareTo(messageIdImpl2) < 0, "Expected to be less than");
			assertTrue(messageIdImpl4.CompareTo(messageIdImpl3) < 0, "Expected to be less than");

			assertTrue(batchMessageId2.CompareTo(batchMessageId1) < 0, "Expected to be less than");
			assertTrue(batchMessageId3.CompareTo(batchMessageId1) < 0, "Expected to be less than");
			assertTrue(batchMessageId4.CompareTo(batchMessageId1) < 0, "Expected to be less than");
			assertTrue(batchMessageId5.CompareTo(batchMessageId1) < 0, "Expected to be less than");
			assertTrue(batchMessageId3.CompareTo(batchMessageId2) < 0, "Expected to be less than");
			assertTrue(batchMessageId4.CompareTo(batchMessageId2) < 0, "Expected to be less than");
			assertTrue(batchMessageId5.CompareTo(batchMessageId2) < 0, "Expected to be less than");
			assertTrue(batchMessageId4.CompareTo(batchMessageId3) < 0, "Expected to be less than");
			assertTrue(batchMessageId5.CompareTo(batchMessageId3) < 0, "Expected to be less than");
			assertTrue(batchMessageId5.CompareTo(batchMessageId4) < 0, "Expected to be less than");
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testCompareDifferentType()
		public virtual void TestCompareDifferentType()
		{
			MessageIdImpl messageIdImpl = new MessageIdImpl(123L, 345L, 567);
			BatchMessageIdImpl batchMessageId1 = new BatchMessageIdImpl(123L, 345L, 566, 789);
			BatchMessageIdImpl batchMessageId2 = new BatchMessageIdImpl(123L, 345L, 567, 789);
			BatchMessageIdImpl batchMessageId3 = new BatchMessageIdImpl(messageIdImpl);
			assertTrue(messageIdImpl.CompareTo(batchMessageId1) > 0, "Expected to be greater than");
			assertTrue(messageIdImpl.CompareTo(batchMessageId2) < 0, "Expected to be less than");
			assertEquals(messageIdImpl.CompareTo(batchMessageId3), 0, "Expected to be equal");
			assertTrue(batchMessageId1.CompareTo(messageIdImpl) < 0, "Expected to be less than");
			assertTrue(batchMessageId2.CompareTo(messageIdImpl) > 0, "Expected to be greater than");
			assertEquals(batchMessageId3.CompareTo(messageIdImpl), 0, "Expected to be equal");
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void compareToSymmetricTest()
		public virtual void CompareToSymmetricTest()
		{
			MessageIdImpl simpleMessageId = new MessageIdImpl(123L, 345L, 567);
			// batchIndex is -1 if message is non-batched message and has the batchIndex for a batch message
			BatchMessageIdImpl batchMessageId1 = new BatchMessageIdImpl(123L, 345L, 567, -1);
			BatchMessageIdImpl batchMessageId2 = new BatchMessageIdImpl(123L, 345L, 567, 1);
			BatchMessageIdImpl batchMessageId3 = new BatchMessageIdImpl(123L, 345L, 566, 1);
			BatchMessageIdImpl batchMessageId4 = new BatchMessageIdImpl(123L, 345L, 566, -1);

			assertEquals(simpleMessageId.CompareTo(batchMessageId1), 0, "Expected to be equal");
			assertEquals(batchMessageId1.CompareTo(simpleMessageId), 0, "Expected to be equal");
			assertTrue(batchMessageId2.CompareTo(simpleMessageId) > 0, "Expected to be greater than");
			assertTrue(simpleMessageId.CompareTo(batchMessageId2) < 0, "Expected to be less than");
			assertTrue(simpleMessageId.CompareTo(batchMessageId3) > 0, "Expected to be greater than");
			assertTrue(batchMessageId3.CompareTo(simpleMessageId) < 0, "Expected to be less than");
			assertTrue(simpleMessageId.CompareTo(batchMessageId4) > 0, "Expected to be greater than");
			assertTrue(batchMessageId4.CompareTo(simpleMessageId) < 0, "Expected to be less than");
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testMessageIdImplCompareToTopicMessageId()
		public virtual void TestMessageIdImplCompareToTopicMessageId()
		{
			MessageIdImpl messageIdImpl = new MessageIdImpl(123L, 345L, 567);
			TopicMessageIdImpl topicMessageId1 = new TopicMessageIdImpl("test-topic-partition-0", "test-topic", new BatchMessageIdImpl(123L, 345L, 566, 789));
			TopicMessageIdImpl topicMessageId2 = new TopicMessageIdImpl("test-topic-partition-0", "test-topic", new BatchMessageIdImpl(123L, 345L, 567, 789));
			TopicMessageIdImpl topicMessageId3 = new TopicMessageIdImpl("test-topic-partition-0", "test-topic", new BatchMessageIdImpl(messageIdImpl));
			assertTrue(messageIdImpl.CompareTo(topicMessageId1) > 0, "Expected to be greater than");
			assertTrue(messageIdImpl.CompareTo(topicMessageId2) < 0, "Expected to be less than");
			assertEquals(messageIdImpl.CompareTo(topicMessageId3), 0, "Expected to be equal");
			assertTrue(topicMessageId1.CompareTo(messageIdImpl) < 0, "Expected to be less than");
			assertTrue(topicMessageId2.CompareTo(messageIdImpl) > 0, "Expected to be greater than");
			assertEquals(topicMessageId3.CompareTo(messageIdImpl), 0, "Expected to be equal");
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testBatchMessageIdImplCompareToTopicMessageId()
		public virtual void TestBatchMessageIdImplCompareToTopicMessageId()
		{
			BatchMessageIdImpl messageIdImpl1 = new BatchMessageIdImpl(123L, 345L, 567, 789);
			BatchMessageIdImpl messageIdImpl2 = new BatchMessageIdImpl(123L, 345L, 567, 0);
			BatchMessageIdImpl messageIdImpl3 = new BatchMessageIdImpl(123L, 345L, 567, -1);
			TopicMessageIdImpl topicMessageId1 = new TopicMessageIdImpl("test-topic-partition-0", "test-topic", new MessageIdImpl(123L, 345L, 566));
			TopicMessageIdImpl topicMessageId2 = new TopicMessageIdImpl("test-topic-partition-0", "test-topic", new MessageIdImpl(123L, 345L, 567));
			assertTrue(messageIdImpl1.CompareTo(topicMessageId1) > 0, "Expected to be greater than");
			assertTrue(messageIdImpl1.CompareTo(topicMessageId2) > 0, "Expected to be greater than");
			assertTrue(messageIdImpl2.CompareTo(topicMessageId2) > 0, "Expected to be greater than");
			assertEquals(messageIdImpl3.CompareTo(topicMessageId2), 0, "Expected to be equal");
			assertTrue(topicMessageId1.CompareTo(messageIdImpl1) < 0, "Expected to be less than");
			assertTrue(topicMessageId2.CompareTo(messageIdImpl1) < 0, "Expected to be less than");
			assertTrue(topicMessageId2.CompareTo(messageIdImpl2) < 0, "Expected to be less than");
			assertTrue(topicMessageId2.CompareTo(messageIdImpl2) < 0, "Expected to be less than");
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testMultiMessageIdEqual()
		public virtual void TestMultiMessageIdEqual()
		{
			// null
			MultiMessageIdImpl null1 = new MultiMessageIdImpl(null);
			MultiMessageIdImpl null2 = new MultiMessageIdImpl(null);
			assertEquals(null1, null2);

			// empty
			MultiMessageIdImpl empty1 = new MultiMessageIdImpl(Collections.emptyMap());
			MultiMessageIdImpl empty2 = new MultiMessageIdImpl(Collections.emptyMap());
			assertEquals(empty1, empty2);

			// null empty
			assertEquals(null1, empty2);
			assertEquals(empty2, null1);

			// 1 item
			string topic1 = "topicName1";
			MessageIdImpl messageIdImpl1 = new MessageIdImpl(123L, 345L, 567);
			MessageIdImpl messageIdImpl2 = new MessageIdImpl(123L, 345L, 567);
			MessageIdImpl messageIdImpl3 = new MessageIdImpl(345L, 456L, 567);

			MultiMessageIdImpl item1 = new MultiMessageIdImpl(Collections.singletonMap(topic1, messageIdImpl1));
			MultiMessageIdImpl item2 = new MultiMessageIdImpl(Collections.singletonMap(topic1, messageIdImpl2));
			assertEquals(item1, item2);

			// 1 item, empty not equal
			assertNotEquals(item1, null1);
			assertNotEquals(null1, item1);

			// key not equal
			string topic2 = "topicName2";
			MultiMessageIdImpl item3 = new MultiMessageIdImpl(Collections.singletonMap(topic2, messageIdImpl2));
			assertNotEquals(item1, item3);
			assertNotEquals(item3, item1);

			// value not equal
			MultiMessageIdImpl item4 = new MultiMessageIdImpl(Collections.singletonMap(topic1, messageIdImpl3));
			assertNotEquals(item1, item4);
			assertNotEquals(item4, item1);

			// key value not equal
			assertNotEquals(item3, item4);
			assertNotEquals(item4, item3);

			// 2 items
			IDictionary<string, MessageId> map1 = Maps.newHashMap();
			IDictionary<string, MessageId> map2 = Maps.newHashMap();
			map1[topic1] = messageIdImpl1;
			map1[topic2] = messageIdImpl2;
			map2[topic2] = messageIdImpl2;
			map2[topic1] = messageIdImpl1;

			MultiMessageIdImpl item5 = new MultiMessageIdImpl(map1);
			MultiMessageIdImpl item6 = new MultiMessageIdImpl(map2);

			assertEquals(item5, item6);

			assertNotEquals(item5, null1);
			assertNotEquals(item5, empty1);
			assertNotEquals(item5, item1);
			assertNotEquals(item5, item3);
			assertNotEquals(item5, item4);

			assertNotEquals(null1, item5);
			assertNotEquals(empty1, item5);
			assertNotEquals(item1, item5);
			assertNotEquals(item3, item5);
			assertNotEquals(item4, item5);

			map2[topic1] = messageIdImpl3;
			MultiMessageIdImpl item7 = new MultiMessageIdImpl(map2);
			assertNotEquals(item5, item7);
			assertNotEquals(item7, item5);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testMultiMessageIdCompareto()
		public virtual void TestMultiMessageIdCompareto()
		{
			// null
			MultiMessageIdImpl null1 = new MultiMessageIdImpl(null);
			MultiMessageIdImpl null2 = new MultiMessageIdImpl(null);
			assertEquals(0, null1.CompareTo(null2));

			// empty
			MultiMessageIdImpl empty1 = new MultiMessageIdImpl(Collections.emptyMap());
			MultiMessageIdImpl empty2 = new MultiMessageIdImpl(Collections.emptyMap());
			assertEquals(0, empty1.CompareTo(empty2));

			// null empty
			assertEquals(0, null1.CompareTo(empty2));
			assertEquals(0, empty2.CompareTo(null1));

			// 1 item
			string topic1 = "topicName1";
			MessageIdImpl messageIdImpl1 = new MessageIdImpl(123L, 345L, 567);
			MessageIdImpl messageIdImpl2 = new MessageIdImpl(123L, 345L, 567);
			MessageIdImpl messageIdImpl3 = new MessageIdImpl(345L, 456L, 567);

			MultiMessageIdImpl item1 = new MultiMessageIdImpl(Collections.singletonMap(topic1, messageIdImpl1));
			MultiMessageIdImpl item2 = new MultiMessageIdImpl(Collections.singletonMap(topic1, messageIdImpl2));
			assertEquals(0, item1.CompareTo(item2));

			// 1 item, empty not equal
			try
			{
				item1.CompareTo(null1);
				fail("should throw exception for not comparable");
			}
			catch(System.ArgumentException)
			{
				// expected
			}
			try
			{
				null1.CompareTo(item1);
				fail("should throw exception for not comparable");
			}
			catch(System.ArgumentException)
			{
				// expected
			}

			// key not equal
			string topic2 = "topicName2";
			MultiMessageIdImpl item3 = new MultiMessageIdImpl(Collections.singletonMap(topic2, messageIdImpl2));
			try
			{
				item1.CompareTo(item3);
				fail("should throw exception for not comparable");
			}
			catch(System.ArgumentException)
			{
				// expected
			}
			try
			{
				item3.CompareTo(item1);
				fail("should throw exception for not comparable");
			}
			catch(System.ArgumentException)
			{
				// expected
			}

			// value not equal
			MultiMessageIdImpl item4 = new MultiMessageIdImpl(Collections.singletonMap(topic1, messageIdImpl3));
			assertTrue(item1.CompareTo(item4) < 0);
			assertTrue(item4.CompareTo(item1) > 0);

			// key value not equal
			try
			{
				item3.CompareTo(item4);
				fail("should throw exception for not comparable");
			}
			catch(System.ArgumentException)
			{
				// expected
			}
			try
			{
				item4.CompareTo(item3);
				fail("should throw exception for not comparable");
			}
			catch(System.ArgumentException)
			{
				// expected
			}

			// 2 items
			IDictionary<string, MessageId> map1 = Maps.newHashMap();
			IDictionary<string, MessageId> map2 = Maps.newHashMap();
			map1[topic1] = messageIdImpl1;
			map1[topic2] = messageIdImpl2;
			map2[topic2] = messageIdImpl2;
			map2[topic1] = messageIdImpl1;

			MultiMessageIdImpl item5 = new MultiMessageIdImpl(map1);
			MultiMessageIdImpl item6 = new MultiMessageIdImpl(map2);

			assertTrue(item5.CompareTo(item6) == 0);

			try
			{
				item5.CompareTo(null1);
				fail("should throw exception for not comparable");
			}
			catch(System.ArgumentException)
			{
				// expected
			}

			try
			{
				item5.CompareTo(empty1);
				fail("should throw exception for not comparable");
			}
			catch(System.ArgumentException)
			{
				// expected
			}

			try
			{
				item5.CompareTo(item1);
				fail("should throw exception for not comparable");
			}
			catch(System.ArgumentException)
			{
				// expected
			}

			try
			{
				item5.CompareTo(item3);
				fail("should throw exception for not comparable");
			}
			catch(System.ArgumentException)
			{
				// expected
			}

			try
			{
				item5.CompareTo(item4);
				fail("should throw exception for not comparable");
			}
			catch(System.ArgumentException)
			{
				// expected
			}

			map2[topic1] = messageIdImpl3;
			MultiMessageIdImpl item7 = new MultiMessageIdImpl(map2);

			assertTrue(item7.CompareTo(item5) > 0);
			assertTrue(item5.CompareTo(item7) < 0);

			IDictionary<string, MessageId> map3 = Maps.newHashMap();
			map3[topic1] = messageIdImpl3;
			map3[topic2] = messageIdImpl3;
			MultiMessageIdImpl item8 = new MultiMessageIdImpl(map3);
			assertTrue(item8.CompareTo(item5) > 0);
			assertTrue(item8.CompareTo(item7) > 0);

			assertTrue(item5.CompareTo(item8) < 0);
			assertTrue(item7.CompareTo(item8) < 0);
		}
	}

}