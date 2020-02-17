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
namespace SharpPulsar.Test.Impl
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.testng.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.testng.Assert.assertTrue;

	using ChannelHandlerContext = io.netty.channel.ChannelHandlerContext;
	using EventLoopGroup = io.netty.channel.EventLoopGroup;
	using NioEventLoopGroup = io.netty.channel.nio.NioEventLoopGroup;

    public class AcknowledgementsGroupingTrackerTest
	{

		private ClientCnx cnx;
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private ConsumerImpl<?> consumer;
		private ConsumerImpl<object> consumer;
		private EventLoopGroup eventLoopGroup;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @BeforeClass public void setup()
		public virtual void Setup()
		{
			eventLoopGroup = new NioEventLoopGroup(1);
			consumer = mock(typeof(ConsumerImpl));
			cnx = mock(typeof(ClientCnx));
			ChannelHandlerContext Ctx = mock(typeof(ChannelHandlerContext));
			when(cnx.Ctx()).thenReturn(Ctx);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @AfterClass public void teardown()
		public virtual void Teardown()
		{
			eventLoopGroup.shutdownGracefully();
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testAckTracker() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		public virtual void TestAckTracker()
		{
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.apache.pulsar.client.impl.conf.ConsumerConfigurationData<?> conf = new org.apache.pulsar.client.impl.conf.ConsumerConfigurationData<>();
			ConsumerConfigurationData<object> Conf = new ConsumerConfigurationData<object>();
			Conf.AcknowledgementsGroupTimeMicros = TimeUnit.SECONDS.toMicros(10);
			PersistentAcknowledgmentsGroupingTracker Tracker = new PersistentAcknowledgmentsGroupingTracker(consumer, Conf, eventLoopGroup);

			MessageIdImpl Msg1 = new MessageIdImpl(5, 1, 0);
			MessageIdImpl Msg2 = new MessageIdImpl(5, 2, 0);
			MessageIdImpl Msg3 = new MessageIdImpl(5, 3, 0);
			MessageIdImpl Msg4 = new MessageIdImpl(5, 4, 0);
			MessageIdImpl Msg5 = new MessageIdImpl(5, 5, 0);
			MessageIdImpl Msg6 = new MessageIdImpl(5, 6, 0);

			assertFalse(Tracker.isDuplicate(Msg1));

			Tracker.addAcknowledgment(Msg1, AckType.Individual, Collections.emptyMap());
			assertTrue(Tracker.isDuplicate(Msg1));

			assertFalse(Tracker.isDuplicate(Msg2));

			Tracker.addAcknowledgment(Msg5, AckType.Cumulative, Collections.emptyMap());
			assertTrue(Tracker.isDuplicate(Msg1));
			assertTrue(Tracker.isDuplicate(Msg2));
			assertTrue(Tracker.isDuplicate(Msg3));

			assertTrue(Tracker.isDuplicate(Msg4));
			assertTrue(Tracker.isDuplicate(Msg5));
			assertFalse(Tracker.isDuplicate(Msg6));

			// Flush while disconnected. the internal tracking will not change
			Tracker.flush();

			assertTrue(Tracker.isDuplicate(Msg1));
			assertTrue(Tracker.isDuplicate(Msg2));
			assertTrue(Tracker.isDuplicate(Msg3));

			assertTrue(Tracker.isDuplicate(Msg4));
			assertTrue(Tracker.isDuplicate(Msg5));
			assertFalse(Tracker.isDuplicate(Msg6));

			Tracker.addAcknowledgment(Msg6, AckType.Individual, Collections.emptyMap());
			assertTrue(Tracker.isDuplicate(Msg6));

			when(consumer.ClientCnx).thenReturn(cnx);

			Tracker.flush();

			assertTrue(Tracker.isDuplicate(Msg1));
			assertTrue(Tracker.isDuplicate(Msg2));
			assertTrue(Tracker.isDuplicate(Msg3));

			assertTrue(Tracker.isDuplicate(Msg4));
			assertTrue(Tracker.isDuplicate(Msg5));
			assertFalse(Tracker.isDuplicate(Msg6));

			Tracker.close();
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testImmediateAckingTracker() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		public virtual void TestImmediateAckingTracker()
		{
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.apache.pulsar.client.impl.conf.ConsumerConfigurationData<?> conf = new org.apache.pulsar.client.impl.conf.ConsumerConfigurationData<>();
			ConsumerConfigurationData<object> Conf = new ConsumerConfigurationData<object>();
			Conf.AcknowledgementsGroupTimeMicros = 0;
			PersistentAcknowledgmentsGroupingTracker Tracker = new PersistentAcknowledgmentsGroupingTracker(consumer, Conf, eventLoopGroup);

			MessageIdImpl Msg1 = new MessageIdImpl(5, 1, 0);
			MessageIdImpl Msg2 = new MessageIdImpl(5, 2, 0);

			assertFalse(Tracker.isDuplicate(Msg1));

			when(consumer.ClientCnx).thenReturn(null);

			Tracker.addAcknowledgment(Msg1, AckType.Individual, Collections.emptyMap());
			assertFalse(Tracker.isDuplicate(Msg1));

			when(consumer.ClientCnx).thenReturn(cnx);

			Tracker.flush();
			assertFalse(Tracker.isDuplicate(Msg1));

			Tracker.addAcknowledgment(Msg2, AckType.Individual, Collections.emptyMap());
			// Since we were connected, the ack went out immediately
			assertFalse(Tracker.isDuplicate(Msg2));
			Tracker.close();
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testAckTrackerMultiAck() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		public virtual void TestAckTrackerMultiAck()
		{
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.apache.pulsar.client.impl.conf.ConsumerConfigurationData<?> conf = new org.apache.pulsar.client.impl.conf.ConsumerConfigurationData<>();
			ConsumerConfigurationData<object> Conf = new ConsumerConfigurationData<object>();
			Conf.AcknowledgementsGroupTimeMicros = TimeUnit.SECONDS.toMicros(10);
			PersistentAcknowledgmentsGroupingTracker Tracker = new PersistentAcknowledgmentsGroupingTracker(consumer, Conf, eventLoopGroup);

			when(cnx.RemoteEndpointProtocolVersion).thenReturn(ProtocolVersion.v12_VALUE);

			MessageIdImpl Msg1 = new MessageIdImpl(5, 1, 0);
			MessageIdImpl Msg2 = new MessageIdImpl(5, 2, 0);
			MessageIdImpl Msg3 = new MessageIdImpl(5, 3, 0);
			MessageIdImpl Msg4 = new MessageIdImpl(5, 4, 0);
			MessageIdImpl Msg5 = new MessageIdImpl(5, 5, 0);
			MessageIdImpl Msg6 = new MessageIdImpl(5, 6, 0);

			assertFalse(Tracker.isDuplicate(Msg1));

			Tracker.addAcknowledgment(Msg1, AckType.Individual, Collections.emptyMap());
			assertTrue(Tracker.isDuplicate(Msg1));

			assertFalse(Tracker.isDuplicate(Msg2));

			Tracker.addAcknowledgment(Msg5, AckType.Cumulative, Collections.emptyMap());
			assertTrue(Tracker.isDuplicate(Msg1));
			assertTrue(Tracker.isDuplicate(Msg2));
			assertTrue(Tracker.isDuplicate(Msg3));

			assertTrue(Tracker.isDuplicate(Msg4));
			assertTrue(Tracker.isDuplicate(Msg5));
			assertFalse(Tracker.isDuplicate(Msg6));

			// Flush while disconnected. the internal tracking will not change
			Tracker.flush();

			assertTrue(Tracker.isDuplicate(Msg1));
			assertTrue(Tracker.isDuplicate(Msg2));
			assertTrue(Tracker.isDuplicate(Msg3));

			assertTrue(Tracker.isDuplicate(Msg4));
			assertTrue(Tracker.isDuplicate(Msg5));
			assertFalse(Tracker.isDuplicate(Msg6));

			Tracker.addAcknowledgment(Msg6, AckType.Individual, Collections.emptyMap());
			assertTrue(Tracker.isDuplicate(Msg6));

			when(consumer.ClientCnx).thenReturn(cnx);

			Tracker.flush();

			assertTrue(Tracker.isDuplicate(Msg1));
			assertTrue(Tracker.isDuplicate(Msg2));
			assertTrue(Tracker.isDuplicate(Msg3));

			assertTrue(Tracker.isDuplicate(Msg4));
			assertTrue(Tracker.isDuplicate(Msg5));
			assertFalse(Tracker.isDuplicate(Msg6));

			Tracker.close();
		}
	}

}