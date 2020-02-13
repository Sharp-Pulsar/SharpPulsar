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
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.powermock.api.mockito.PowerMockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.testng.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.testng.Assert.assertTrue;


	using HashingScheme = Org.Apache.Pulsar.Client.Api.HashingScheme;
	using Org.Apache.Pulsar.Client.Api;
	using Test = org.testng.annotations.Test;

	/// <summary>
	/// Unit test of <seealso cref="RoundRobinPartitionMessageRouterImpl"/>.
	/// </summary>
	public class RoundRobinPartitionMessageRouterImplTest
	{

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testChoosePartitionWithoutKey()
		public virtual void TestChoosePartitionWithoutKey()
		{
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.apache.pulsar.client.api.Message<?> msg = mock(org.apache.pulsar.client.api.Message.class);
			Message<object> Msg = mock(typeof(Message));
			when(Msg.Key).thenReturn(null);

			RoundRobinPartitionMessageRouterImpl Router = new RoundRobinPartitionMessageRouterImpl(HashingScheme.JavaStringHash, 0, false, 0);
			for (int I = 0; I < 10; I++)
			{
				assertEquals(I % 5, Router.choosePartition(Msg, new TopicMetadataImpl(5)));
			}
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testChoosePartitionWithoutKeyWithBatching()
		public virtual void TestChoosePartitionWithoutKeyWithBatching()
		{
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.apache.pulsar.client.api.Message<?> msg = mock(org.apache.pulsar.client.api.Message.class);
			Message<object> Msg = mock(typeof(Message));
			when(Msg.Key).thenReturn(null);

			// Fake clock, simulate 1 millisecond passes for each invocation
			Clock Clock = new ClockAnonymousInnerClass(this);

			RoundRobinPartitionMessageRouterImpl Router = new RoundRobinPartitionMessageRouterImpl(HashingScheme.JavaStringHash, 0, true, 5, Clock);

			// Since the batching time is 5millis, first 5 messages will go on partition 0 and next five would go on
			// partition 1
			for (int I = 0; I < 5; I++)
			{
				assertEquals(0, Router.choosePartition(Msg, new TopicMetadataImpl(5)));
			}

			for (int I = 5; I < 10; I++)
			{
				assertEquals(1, Router.choosePartition(Msg, new TopicMetadataImpl(5)));
			}
		}

		public class ClockAnonymousInnerClass : Clock
		{
			private readonly RoundRobinPartitionMessageRouterImplTest outerInstance;

			public ClockAnonymousInnerClass(RoundRobinPartitionMessageRouterImplTest OuterInstance)
			{
				this.outerInstance = OuterInstance;
				current = 0;
			}

			private long current;

			public override Clock withZone(ZoneId Zone)
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
		public virtual void TestChoosePartitionWithNegativeTime()
		{
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.apache.pulsar.client.api.Message<?> msg = mock(org.apache.pulsar.client.api.Message.class);
			Message<object> Msg = mock(typeof(Message));
			when(Msg.Key).thenReturn(null);

			// Fake clock, simulate timestamp that resolves into a negative Integer value
			Clock Clock = mock(typeof(Clock));
			when(Clock.millis()).thenReturn((long) int.MaxValue);

			RoundRobinPartitionMessageRouterImpl Router = new RoundRobinPartitionMessageRouterImpl(HashingScheme.JavaStringHash, 3, true, 5, Clock);

			int Idx = Router.choosePartition(Msg, new TopicMetadataImpl(5));
			assertTrue(Idx >= 0);
			assertTrue(Idx < 5);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testChoosePartitionWithKey()
		public virtual void TestChoosePartitionWithKey()
		{
			string Key1 = "key1";
			string Key2 = "key2";
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.apache.pulsar.client.api.Message<?> msg1 = mock(org.apache.pulsar.client.api.Message.class);
			Message<object> Msg1 = mock(typeof(Message));
			when(Msg1.hasKey()).thenReturn(true);
			when(Msg1.Key).thenReturn(Key1);
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.apache.pulsar.client.api.Message<?> msg2 = mock(org.apache.pulsar.client.api.Message.class);
			Message<object> Msg2 = mock(typeof(Message));
			when(Msg2.hasKey()).thenReturn(true);
			when(Msg2.Key).thenReturn(Key2);

			RoundRobinPartitionMessageRouterImpl Router = new RoundRobinPartitionMessageRouterImpl(HashingScheme.JavaStringHash, 0, false, 0);
			TopicMetadataImpl Metadata = new TopicMetadataImpl(100);

			assertEquals(Key1.GetHashCode() % 100, Router.choosePartition(Msg1, Metadata));
			assertEquals(Key2.GetHashCode() % 100, Router.choosePartition(Msg2, Metadata));
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testBatchingAwareness() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		public virtual void TestBatchingAwareness()
		{
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.apache.pulsar.client.api.Message<?> msg = mock(org.apache.pulsar.client.api.Message.class);
			Message<object> Msg = mock(typeof(Message));
			when(Msg.Key).thenReturn(null);

			Clock Clock = mock(typeof(Clock));

			RoundRobinPartitionMessageRouterImpl Router = new RoundRobinPartitionMessageRouterImpl(HashingScheme.JavaStringHash, 0, true, 10, Clock);
			TopicMetadataImpl Metadata = new TopicMetadataImpl(100);

			// time at `12345*` milliseconds
			for (int I = 0; I < 10; I++)
			{
				when(Clock.millis()).thenReturn(123450L + I);

				assertEquals(45, Router.choosePartition(Msg, Metadata));
			}

			// time at `12346*` milliseconds
			for (int I = 0; I < 10; I++)
			{
				when(Clock.millis()).thenReturn(123460L + I);

				assertEquals(46, Router.choosePartition(Msg, Metadata));
			}
		}
	}

}