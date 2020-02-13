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
			MessageIdImpl MessageIdImpl1 = new MessageIdImpl(123L, 345L, 567);
			MessageIdImpl MessageIdImpl2 = new MessageIdImpl(123L, 345L, 567);

			BatchMessageIdImpl BatchMessageId1 = new BatchMessageIdImpl(234L, 345L, 456, 567);
			BatchMessageIdImpl BatchMessageId2 = new BatchMessageIdImpl(234L, 345L, 456, 567);

			assertEquals(MessageIdImpl1.CompareTo(MessageIdImpl2), 0, "Expected to be equal");
			assertEquals(BatchMessageId1.CompareTo(BatchMessageId2), 0, "Expected to be equal");
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testGreaterThan()
		public virtual void TestGreaterThan()
		{
			MessageIdImpl MessageIdImpl1 = new MessageIdImpl(124L, 345L, 567);
			MessageIdImpl MessageIdImpl2 = new MessageIdImpl(123L, 345L, 567);
			MessageIdImpl MessageIdImpl3 = new MessageIdImpl(123L, 344L, 567);
			MessageIdImpl MessageIdImpl4 = new MessageIdImpl(123L, 344L, 566);

			BatchMessageIdImpl BatchMessageId1 = new BatchMessageIdImpl(235L, 345L, 456, 567);
			BatchMessageIdImpl BatchMessageId2 = new BatchMessageIdImpl(234L, 346L, 456, 567);
			BatchMessageIdImpl BatchMessageId3 = new BatchMessageIdImpl(234L, 345L, 456, 568);
			BatchMessageIdImpl BatchMessageId4 = new BatchMessageIdImpl(234L, 345L, 457, 567);
			BatchMessageIdImpl BatchMessageId5 = new BatchMessageIdImpl(234L, 345L, 456, 567);

			assertTrue(MessageIdImpl1.CompareTo(MessageIdImpl2) > 0, "Expected to be greater than");
			assertTrue(MessageIdImpl1.CompareTo(MessageIdImpl3) > 0, "Expected to be greater than");
			assertTrue(MessageIdImpl1.CompareTo(MessageIdImpl4) > 0, "Expected to be greater than");
			assertTrue(MessageIdImpl2.CompareTo(MessageIdImpl3) > 0, "Expected to be greater than");
			assertTrue(MessageIdImpl2.CompareTo(MessageIdImpl4) > 0, "Expected to be greater than");
			assertTrue(MessageIdImpl3.CompareTo(MessageIdImpl4) > 0, "Expected to be greater than");

			assertTrue(BatchMessageId1.CompareTo(BatchMessageId2) > 0, "Expected to be greater than");
			assertTrue(BatchMessageId1.CompareTo(BatchMessageId3) > 0, "Expected to be greater than");
			assertTrue(BatchMessageId1.CompareTo(BatchMessageId4) > 0, "Expected to be greater than");
			assertTrue(BatchMessageId1.CompareTo(BatchMessageId5) > 0, "Expected to be greater than");
			assertTrue(BatchMessageId2.CompareTo(BatchMessageId3) > 0, "Expected to be greater than");
			assertTrue(BatchMessageId2.CompareTo(BatchMessageId4) > 0, "Expected to be greater than");
			assertTrue(BatchMessageId2.CompareTo(BatchMessageId5) > 0, "Expected to be greater than");
			assertTrue(BatchMessageId3.CompareTo(BatchMessageId4) > 0, "Expected to be greater than");
			assertTrue(BatchMessageId3.CompareTo(BatchMessageId5) > 0, "Expected to be greater than");
			assertTrue(BatchMessageId4.CompareTo(BatchMessageId5) > 0, "Expected to be greater than");
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testLessThan()
		public virtual void TestLessThan()
		{
			MessageIdImpl MessageIdImpl1 = new MessageIdImpl(124L, 345L, 567);
			MessageIdImpl MessageIdImpl2 = new MessageIdImpl(123L, 345L, 567);
			MessageIdImpl MessageIdImpl3 = new MessageIdImpl(123L, 344L, 567);
			MessageIdImpl MessageIdImpl4 = new MessageIdImpl(123L, 344L, 566);

			BatchMessageIdImpl BatchMessageId1 = new BatchMessageIdImpl(235L, 345L, 456, 567);
			BatchMessageIdImpl BatchMessageId2 = new BatchMessageIdImpl(234L, 346L, 456, 567);
			BatchMessageIdImpl BatchMessageId3 = new BatchMessageIdImpl(234L, 345L, 456, 568);
			BatchMessageIdImpl BatchMessageId4 = new BatchMessageIdImpl(234L, 345L, 457, 567);
			BatchMessageIdImpl BatchMessageId5 = new BatchMessageIdImpl(234L, 345L, 456, 567);

			assertTrue(MessageIdImpl2.CompareTo(MessageIdImpl1) < 0, "Expected to be less than");
			assertTrue(MessageIdImpl3.CompareTo(MessageIdImpl1) < 0, "Expected to be less than");
			assertTrue(MessageIdImpl4.CompareTo(MessageIdImpl1) < 0, "Expected to be less than");
			assertTrue(MessageIdImpl3.CompareTo(MessageIdImpl2) < 0, "Expected to be less than");
			assertTrue(MessageIdImpl4.CompareTo(MessageIdImpl2) < 0, "Expected to be less than");
			assertTrue(MessageIdImpl4.CompareTo(MessageIdImpl3) < 0, "Expected to be less than");

			assertTrue(BatchMessageId2.CompareTo(BatchMessageId1) < 0, "Expected to be less than");
			assertTrue(BatchMessageId3.CompareTo(BatchMessageId1) < 0, "Expected to be less than");
			assertTrue(BatchMessageId4.CompareTo(BatchMessageId1) < 0, "Expected to be less than");
			assertTrue(BatchMessageId5.CompareTo(BatchMessageId1) < 0, "Expected to be less than");
			assertTrue(BatchMessageId3.CompareTo(BatchMessageId2) < 0, "Expected to be less than");
			assertTrue(BatchMessageId4.CompareTo(BatchMessageId2) < 0, "Expected to be less than");
			assertTrue(BatchMessageId5.CompareTo(BatchMessageId2) < 0, "Expected to be less than");
			assertTrue(BatchMessageId4.CompareTo(BatchMessageId3) < 0, "Expected to be less than");
			assertTrue(BatchMessageId5.CompareTo(BatchMessageId3) < 0, "Expected to be less than");
			assertTrue(BatchMessageId5.CompareTo(BatchMessageId4) < 0, "Expected to be less than");
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testCompareDifferentType()
		public virtual void TestCompareDifferentType()
		{
			MessageIdImpl MessageIdImpl = new MessageIdImpl(123L, 345L, 567);
			BatchMessageIdImpl BatchMessageId1 = new BatchMessageIdImpl(123L, 345L, 566, 789);
			BatchMessageIdImpl BatchMessageId2 = new BatchMessageIdImpl(123L, 345L, 567, 789);
			BatchMessageIdImpl BatchMessageId3 = new BatchMessageIdImpl(MessageIdImpl);
			assertTrue(MessageIdImpl.CompareTo(BatchMessageId1) > 0, "Expected to be greater than");
			assertEquals(MessageIdImpl.CompareTo(BatchMessageId2), 0, "Expected to be equal");
			assertEquals(MessageIdImpl.CompareTo(BatchMessageId3), 0, "Expected to be equal");
			assertTrue(BatchMessageId1.CompareTo(MessageIdImpl) < 0, "Expected to be less than");
			assertTrue(BatchMessageId2.CompareTo(MessageIdImpl) > 0, "Expected to be greater than");
			assertEquals(BatchMessageId3.CompareTo(MessageIdImpl), 0, "Expected to be equal");
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testMessageIdImplCompareToTopicMessageId()
		public virtual void TestMessageIdImplCompareToTopicMessageId()
		{
			MessageIdImpl MessageIdImpl = new MessageIdImpl(123L, 345L, 567);
			TopicMessageIdImpl TopicMessageId1 = new TopicMessageIdImpl("test-topic-partition-0", "test-topic", new BatchMessageIdImpl(123L, 345L, 566, 789));
			TopicMessageIdImpl TopicMessageId2 = new TopicMessageIdImpl("test-topic-partition-0", "test-topic", new BatchMessageIdImpl(123L, 345L, 567, 789));
			TopicMessageIdImpl TopicMessageId3 = new TopicMessageIdImpl("test-topic-partition-0", "test-topic", new BatchMessageIdImpl(MessageIdImpl));
			assertTrue(MessageIdImpl.CompareTo(TopicMessageId1) > 0, "Expected to be greater than");
			assertEquals(MessageIdImpl.CompareTo(TopicMessageId2), 0, "Expected to be equal");
			assertEquals(MessageIdImpl.CompareTo(TopicMessageId3), 0, "Expected to be equal");
			assertTrue(TopicMessageId1.CompareTo(MessageIdImpl) < 0, "Expected to be less than");
			assertTrue(TopicMessageId2.CompareTo(MessageIdImpl) > 0, "Expected to be greater than");
			assertEquals(TopicMessageId3.CompareTo(MessageIdImpl), 0, "Expected to be equal");
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testBatchMessageIdImplCompareToTopicMessageId()
		public virtual void TestBatchMessageIdImplCompareToTopicMessageId()
		{
			BatchMessageIdImpl MessageIdImpl1 = new BatchMessageIdImpl(123L, 345L, 567, 789);
			BatchMessageIdImpl MessageIdImpl2 = new BatchMessageIdImpl(123L, 345L, 567, 0);
			BatchMessageIdImpl MessageIdImpl3 = new BatchMessageIdImpl(123L, 345L, 567, -1);
			TopicMessageIdImpl TopicMessageId1 = new TopicMessageIdImpl("test-topic-partition-0", "test-topic", new MessageIdImpl(123L, 345L, 566));
			TopicMessageIdImpl TopicMessageId2 = new TopicMessageIdImpl("test-topic-partition-0", "test-topic", new MessageIdImpl(123L, 345L, 567));
			assertTrue(MessageIdImpl1.CompareTo(TopicMessageId1) > 0, "Expected to be greater than");
			assertTrue(MessageIdImpl1.CompareTo(TopicMessageId2) > 0, "Expected to be greater than");
			assertTrue(MessageIdImpl2.CompareTo(TopicMessageId2) > 0, "Expected to be greater than");
			assertEquals(MessageIdImpl3.CompareTo(TopicMessageId2), 0, "Expected to be equal");
			assertTrue(TopicMessageId1.CompareTo(MessageIdImpl1) < 0, "Expected to be less than");
			assertEquals(TopicMessageId2.CompareTo(MessageIdImpl1), 0, "Expected to be equal");
			assertEquals(TopicMessageId2.CompareTo(MessageIdImpl2), 0, "Expected to be equal");
			assertEquals(TopicMessageId2.CompareTo(MessageIdImpl2), 0, "Expected to be equal");
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testMultiMessageIdEqual()
		public virtual void TestMultiMessageIdEqual()
		{
			// null
			MultiMessageIdImpl Null1 = new MultiMessageIdImpl(null);
			MultiMessageIdImpl Null2 = new MultiMessageIdImpl(null);
			assertEquals(Null1, Null2);

			// empty
			MultiMessageIdImpl Empty1 = new MultiMessageIdImpl(Collections.emptyMap());
			MultiMessageIdImpl Empty2 = new MultiMessageIdImpl(Collections.emptyMap());
			assertEquals(Empty1, Empty2);

			// null empty
			assertEquals(Null1, Empty2);
			assertEquals(Empty2, Null1);

			// 1 item
			string Topic1 = "topicName1";
			MessageIdImpl MessageIdImpl1 = new MessageIdImpl(123L, 345L, 567);
			MessageIdImpl MessageIdImpl2 = new MessageIdImpl(123L, 345L, 567);
			MessageIdImpl MessageIdImpl3 = new MessageIdImpl(345L, 456L, 567);

			MultiMessageIdImpl Item1 = new MultiMessageIdImpl(Collections.singletonMap(Topic1, MessageIdImpl1));
			MultiMessageIdImpl Item2 = new MultiMessageIdImpl(Collections.singletonMap(Topic1, MessageIdImpl2));
			assertEquals(Item1, Item2);

			// 1 item, empty not equal
			assertNotEquals(Item1, Null1);
			assertNotEquals(Null1, Item1);

			// key not equal
			string Topic2 = "topicName2";
			MultiMessageIdImpl Item3 = new MultiMessageIdImpl(Collections.singletonMap(Topic2, MessageIdImpl2));
			assertNotEquals(Item1, Item3);
			assertNotEquals(Item3, Item1);

			// value not equal
			MultiMessageIdImpl Item4 = new MultiMessageIdImpl(Collections.singletonMap(Topic1, MessageIdImpl3));
			assertNotEquals(Item1, Item4);
			assertNotEquals(Item4, Item1);

			// key value not equal
			assertNotEquals(Item3, Item4);
			assertNotEquals(Item4, Item3);

			// 2 items
			IDictionary<string, MessageId> Map1 = Maps.newHashMap();
			IDictionary<string, MessageId> Map2 = Maps.newHashMap();
			Map1[Topic1] = MessageIdImpl1;
			Map1[Topic2] = MessageIdImpl2;
			Map2[Topic2] = MessageIdImpl2;
			Map2[Topic1] = MessageIdImpl1;

			MultiMessageIdImpl Item5 = new MultiMessageIdImpl(Map1);
			MultiMessageIdImpl Item6 = new MultiMessageIdImpl(Map2);

			assertEquals(Item5, Item6);

			assertNotEquals(Item5, Null1);
			assertNotEquals(Item5, Empty1);
			assertNotEquals(Item5, Item1);
			assertNotEquals(Item5, Item3);
			assertNotEquals(Item5, Item4);

			assertNotEquals(Null1, Item5);
			assertNotEquals(Empty1, Item5);
			assertNotEquals(Item1, Item5);
			assertNotEquals(Item3, Item5);
			assertNotEquals(Item4, Item5);

			Map2[Topic1] = MessageIdImpl3;
			MultiMessageIdImpl Item7 = new MultiMessageIdImpl(Map2);
			assertNotEquals(Item5, Item7);
			assertNotEquals(Item7, Item5);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testMultiMessageIdCompareto()
		public virtual void TestMultiMessageIdCompareto()
		{
			// null
			MultiMessageIdImpl Null1 = new MultiMessageIdImpl(null);
			MultiMessageIdImpl Null2 = new MultiMessageIdImpl(null);
			assertEquals(0, Null1.CompareTo(Null2));

			// empty
			MultiMessageIdImpl Empty1 = new MultiMessageIdImpl(Collections.emptyMap());
			MultiMessageIdImpl Empty2 = new MultiMessageIdImpl(Collections.emptyMap());
			assertEquals(0, Empty1.CompareTo(Empty2));

			// null empty
			assertEquals(0, Null1.CompareTo(Empty2));
			assertEquals(0, Empty2.CompareTo(Null1));

			// 1 item
			string Topic1 = "topicName1";
			MessageIdImpl MessageIdImpl1 = new MessageIdImpl(123L, 345L, 567);
			MessageIdImpl MessageIdImpl2 = new MessageIdImpl(123L, 345L, 567);
			MessageIdImpl MessageIdImpl3 = new MessageIdImpl(345L, 456L, 567);

			MultiMessageIdImpl Item1 = new MultiMessageIdImpl(Collections.singletonMap(Topic1, MessageIdImpl1));
			MultiMessageIdImpl Item2 = new MultiMessageIdImpl(Collections.singletonMap(Topic1, MessageIdImpl2));
			assertEquals(0, Item1.CompareTo(Item2));

			// 1 item, empty not equal
			try
			{
				Item1.CompareTo(Null1);
				fail("should throw exception for not comparable");
			}
			catch (System.ArgumentException)
			{
				// expected
			}
			try
			{
				Null1.CompareTo(Item1);
				fail("should throw exception for not comparable");
			}
			catch (System.ArgumentException)
			{
				// expected
			}

			// key not equal
			string Topic2 = "topicName2";
			MultiMessageIdImpl Item3 = new MultiMessageIdImpl(Collections.singletonMap(Topic2, MessageIdImpl2));
			try
			{
				Item1.CompareTo(Item3);
				fail("should throw exception for not comparable");
			}
			catch (System.ArgumentException)
			{
				// expected
			}
			try
			{
				Item3.CompareTo(Item1);
				fail("should throw exception for not comparable");
			}
			catch (System.ArgumentException)
			{
				// expected
			}

			// value not equal
			MultiMessageIdImpl Item4 = new MultiMessageIdImpl(Collections.singletonMap(Topic1, MessageIdImpl3));
			assertTrue(Item1.CompareTo(Item4) < 0);
			assertTrue(Item4.CompareTo(Item1) > 0);

			// key value not equal
			try
			{
				Item3.CompareTo(Item4);
				fail("should throw exception for not comparable");
			}
			catch (System.ArgumentException)
			{
				// expected
			}
			try
			{
				Item4.CompareTo(Item3);
				fail("should throw exception for not comparable");
			}
			catch (System.ArgumentException)
			{
				// expected
			}

			// 2 items
			IDictionary<string, MessageId> Map1 = Maps.newHashMap();
			IDictionary<string, MessageId> Map2 = Maps.newHashMap();
			Map1[Topic1] = MessageIdImpl1;
			Map1[Topic2] = MessageIdImpl2;
			Map2[Topic2] = MessageIdImpl2;
			Map2[Topic1] = MessageIdImpl1;

			MultiMessageIdImpl Item5 = new MultiMessageIdImpl(Map1);
			MultiMessageIdImpl Item6 = new MultiMessageIdImpl(Map2);

			assertTrue(Item5.CompareTo(Item6) == 0);

			try
			{
				Item5.CompareTo(Null1);
				fail("should throw exception for not comparable");
			}
			catch (System.ArgumentException)
			{
				// expected
			}

			try
			{
				Item5.CompareTo(Empty1);
				fail("should throw exception for not comparable");
			}
			catch (System.ArgumentException)
			{
				// expected
			}

			try
			{
				Item5.CompareTo(Item1);
				fail("should throw exception for not comparable");
			}
			catch (System.ArgumentException)
			{
				// expected
			}

			try
			{
				Item5.CompareTo(Item3);
				fail("should throw exception for not comparable");
			}
			catch (System.ArgumentException)
			{
				// expected
			}

			try
			{
				Item5.CompareTo(Item4);
				fail("should throw exception for not comparable");
			}
			catch (System.ArgumentException)
			{
				// expected
			}

			Map2[Topic1] = MessageIdImpl3;
			MultiMessageIdImpl Item7 = new MultiMessageIdImpl(Map2);

			assertTrue(Item7.CompareTo(Item5) > 0);
			assertTrue(Item5.CompareTo(Item7) < 0);

			IDictionary<string, MessageId> Map3 = Maps.newHashMap();
			Map3[Topic1] = MessageIdImpl3;
			Map3[Topic2] = MessageIdImpl3;
			MultiMessageIdImpl Item8 = new MultiMessageIdImpl(Map3);
			assertTrue(Item8.CompareTo(Item5) > 0);
			assertTrue(Item8.CompareTo(Item7) > 0);

			assertTrue(Item5.CompareTo(Item8) < 0);
			assertTrue(Item7.CompareTo(Item8) < 0);
		}
	}

}