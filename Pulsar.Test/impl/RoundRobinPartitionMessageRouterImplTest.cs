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
namespace org.apache.pulsar.client.impl
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.powermock.api.mockito.PowerMockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.testng.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.testng.Assert.assertTrue;


	using HashingScheme = api.HashingScheme;
	using Message = api.Message;
	using Test = org.testng.annotations.Test;

	/// <summary>
	/// Unit test of <seealso cref="RoundRobinPartitionMessageRouterImpl"/>.
	/// </summary>
	public class RoundRobinPartitionMessageRouterImplTest
	{

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testChoosePartitionWithoutKey()
		public virtual void testChoosePartitionWithoutKey()
		{
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.apache.pulsar.client.api.Message<?> msg = mock(org.apache.pulsar.client.api.Message.class);
			Message<object> msg = mock(typeof(Message));
			when(msg.Key).thenReturn(null);

			RoundRobinPartitionMessageRouterImpl router = new RoundRobinPartitionMessageRouterImpl(HashingScheme.JavaStringHash, 0, false, 0);
			for (int i = 0; i < 10; i++)
			{
				assertEquals(i % 5, router.choosePartition(msg, new TopicMetadataImpl(5)));
			}
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testChoosePartitionWithoutKeyWithBatching()
		public virtual void testChoosePartitionWithoutKeyWithBatching()
		{
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.apache.pulsar.client.api.Message<?> msg = mock(org.apache.pulsar.client.api.Message.class);
			Message<object> msg = mock(typeof(Message));
			when(msg.Key).thenReturn(null);

			// Fake clock, simulate 1 millisecond passes for each invocation
			Clock clock = new ClockAnonymousInnerClass(this);

			RoundRobinPartitionMessageRouterImpl router = new RoundRobinPartitionMessageRouterImpl(HashingScheme.JavaStringHash, 0, true, 5, clock);

			// Since the batching time is 5millis, first 5 messages will go on partition 0 and next five would go on
			// partition 1
			for (int i = 0; i < 5; i++)
			{
				assertEquals(0, router.choosePartition(msg, new TopicMetadataImpl(5)));
			}

			for (int i = 5; i < 10; i++)
			{
				assertEquals(1, router.choosePartition(msg, new TopicMetadataImpl(5)));
			}
		}

		private class ClockAnonymousInnerClass : Clock
		{
			private readonly RoundRobinPartitionMessageRouterImplTest outerInstance;

			public ClockAnonymousInnerClass(RoundRobinPartitionMessageRouterImplTest outerInstance)
			{
				this.outerInstance = outerInstance;
				current = 0;
			}

			private long current;

			public override Clock withZone(ZoneId zone)
			{
				return null;
			}

			public override long millis()
			{
				return current++;
			}

			public override Instant instant()
			{
				return Instant.ofEpochMilli(millis());
			}

			public override ZoneId Zone
			{
				get
				{
					return ZoneId.systemDefault();
				}
			}
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testChoosePartitionWithNegativeTime()
		public virtual void testChoosePartitionWithNegativeTime()
		{
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.apache.pulsar.client.api.Message<?> msg = mock(org.apache.pulsar.client.api.Message.class);
			Message<object> msg = mock(typeof(Message));
			when(msg.Key).thenReturn(null);

			// Fake clock, simulate timestamp that resolves into a negative Integer value
			Clock clock = mock(typeof(Clock));
			when(clock.millis()).thenReturn((long) int.MaxValue);

			RoundRobinPartitionMessageRouterImpl router = new RoundRobinPartitionMessageRouterImpl(HashingScheme.JavaStringHash, 3, true, 5, clock);

			int idx = router.choosePartition(msg, new TopicMetadataImpl(5));
			assertTrue(idx >= 0);
			assertTrue(idx < 5);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testChoosePartitionWithKey()
		public virtual void testChoosePartitionWithKey()
		{
			string key1 = "key1";
			string key2 = "key2";
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.apache.pulsar.client.api.Message<?> msg1 = mock(org.apache.pulsar.client.api.Message.class);
			Message<object> msg1 = mock(typeof(Message));
			when(msg1.hasKey()).thenReturn(true);
			when(msg1.Key).thenReturn(key1);
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.apache.pulsar.client.api.Message<?> msg2 = mock(org.apache.pulsar.client.api.Message.class);
			Message<object> msg2 = mock(typeof(Message));
			when(msg2.hasKey()).thenReturn(true);
			when(msg2.Key).thenReturn(key2);

			RoundRobinPartitionMessageRouterImpl router = new RoundRobinPartitionMessageRouterImpl(HashingScheme.JavaStringHash, 0, false, 0);
			TopicMetadataImpl metadata = new TopicMetadataImpl(100);

			assertEquals(key1.GetHashCode() % 100, router.choosePartition(msg1, metadata));
			assertEquals(key2.GetHashCode() % 100, router.choosePartition(msg2, metadata));
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testBatchingAwareness() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		public virtual void testBatchingAwareness()
		{
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.apache.pulsar.client.api.Message<?> msg = mock(org.apache.pulsar.client.api.Message.class);
			Message<object> msg = mock(typeof(Message));
			when(msg.Key).thenReturn(null);

			Clock clock = mock(typeof(Clock));

			RoundRobinPartitionMessageRouterImpl router = new RoundRobinPartitionMessageRouterImpl(HashingScheme.JavaStringHash, 0, true, 10, clock);
			TopicMetadataImpl metadata = new TopicMetadataImpl(100);

			// time at `12345*` milliseconds
			for (int i = 0; i < 10; i++)
			{
				when(clock.millis()).thenReturn(123450L + i);

				assertEquals(45, router.choosePartition(msg, metadata));
			}

			// time at `12346*` milliseconds
			for (int i = 0; i < 10; i++)
			{
				when(clock.millis()).thenReturn(123460L + i);

				assertEquals(46, router.choosePartition(msg, metadata));
			}
		}
	}

}