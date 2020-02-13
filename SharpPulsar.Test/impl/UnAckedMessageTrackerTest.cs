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
namespace Org.Apache.Pulsar.Client.Impl
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

	using MessageId = Org.Apache.Pulsar.Client.Api.MessageId;
	using Org.Apache.Pulsar.Common.Util.Collections;
	using Test = org.testng.annotations.Test;

	public class UnAckedMessageTrackerTest
	{

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testAddAndRemove() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		public virtual void TestAddAndRemove()
		{
			PulsarClientImpl Client = mock(typeof(PulsarClientImpl));
			Timer Timer = new HashedWheelTimer(new DefaultThreadFactory("pulsar-timer", Thread.CurrentThread.Daemon), 1, TimeUnit.MILLISECONDS);
			when(Client.timer()).thenReturn(Timer);

			ConsumerBase<sbyte[]> Consumer = mock(typeof(ConsumerBase));
			doNothing().when(Consumer).onAckTimeoutSend(any());
			doNothing().when(Consumer).redeliverUnacknowledgedMessages(any());

			UnAckedMessageTracker Tracker = new UnAckedMessageTracker(Client, Consumer, 1000000, 100000);
			Tracker.Dispose();

			assertTrue(Tracker.Empty);
			assertEquals(Tracker.size(), 0);

			MessageIdImpl Mid = new MessageIdImpl(1L, 1L, -1);
			assertTrue(Tracker.add(Mid));
			assertFalse(Tracker.add(Mid));
			assertEquals(Tracker.size(), 1);

			ConcurrentOpenHashSet<MessageId> HeadPartition = Tracker.TimePartitions.RemoveFirst();
			HeadPartition.clear();
			Tracker.TimePartitions.AddLast(HeadPartition);

			assertFalse(Tracker.add(Mid));
			assertEquals(Tracker.size(), 1);

			assertTrue(Tracker.remove(Mid));
			assertTrue(Tracker.Empty);
			assertEquals(Tracker.size(), 0);

			Timer.stop();
		}

	}

}