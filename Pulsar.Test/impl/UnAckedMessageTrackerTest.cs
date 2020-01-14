using System.Threading;

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
//	import static org.testng.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.testng.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.testng.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.doNothing;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;

	using HashedWheelTimer = io.netty.util.HashedWheelTimer;
	using Timer = io.netty.util.Timer;
	using DefaultThreadFactory = io.netty.util.concurrent.DefaultThreadFactory;

	using MessageId = org.apache.pulsar.client.api.MessageId;
	using ConcurrentOpenHashSet = org.apache.pulsar.common.util.collections.ConcurrentOpenHashSet;
	using Test = org.testng.annotations.Test;

	public class UnAckedMessageTrackerTest
	{

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testAddAndRemove() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		public virtual void testAddAndRemove()
		{
			PulsarClientImpl client = mock(typeof(PulsarClientImpl));
			Timer timer = new HashedWheelTimer(new DefaultThreadFactory("pulsar-timer", Thread.CurrentThread.Daemon), 1, TimeUnit.MILLISECONDS);
			when(client.timer()).thenReturn(timer);

			ConsumerBase<sbyte[]> consumer = mock(typeof(ConsumerBase));
			doNothing().when(consumer).onAckTimeoutSend(any());
			doNothing().when(consumer).redeliverUnacknowledgedMessages(any());

			UnAckedMessageTracker tracker = new UnAckedMessageTracker(client, consumer, 1000000, 100000);
			tracker.Dispose();

			assertTrue(tracker.Empty);
			assertEquals(tracker.size(), 0);

			MessageIdImpl mid = new MessageIdImpl(1L, 1L, -1);
			assertTrue(tracker.add(mid));
			assertFalse(tracker.add(mid));
			assertEquals(tracker.size(), 1);

			ConcurrentOpenHashSet<MessageId> headPartition = tracker.timePartitions.RemoveFirst();
			headPartition.clear();
			tracker.timePartitions.AddLast(headPartition);

			assertFalse(tracker.add(mid));
			assertEquals(tracker.size(), 1);

			assertTrue(tracker.remove(mid));
			assertTrue(tracker.Empty);
			assertEquals(tracker.size(), 0);

			timer.stop();
		}

	}

}