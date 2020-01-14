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
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.testng.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.testng.Assert.assertTrue;

	using ChannelHandlerContext = io.netty.channel.ChannelHandlerContext;
	using EventLoopGroup = io.netty.channel.EventLoopGroup;
	using NioEventLoopGroup = io.netty.channel.nio.NioEventLoopGroup;


	using org.apache.pulsar.client.impl.conf;
	using AckType = org.apache.pulsar.common.api.proto.PulsarApi.CommandAck.AckType;
	using ProtocolVersion = org.apache.pulsar.common.api.proto.PulsarApi.ProtocolVersion;
	using AfterClass = org.testng.annotations.AfterClass;
	using BeforeClass = org.testng.annotations.BeforeClass;
	using Test = org.testng.annotations.Test;

	public class AcknowledgementsGroupingTrackerTest
	{

		private ClientCnx cnx;
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private ConsumerImpl<?> consumer;
		private ConsumerImpl<object> consumer;
		private EventLoopGroup eventLoopGroup;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @BeforeClass public void setup()
		public virtual void setup()
		{
			eventLoopGroup = new NioEventLoopGroup(1);
			consumer = mock(typeof(ConsumerImpl));
			cnx = mock(typeof(ClientCnx));
			ChannelHandlerContext ctx = mock(typeof(ChannelHandlerContext));
			when(cnx.ctx()).thenReturn(ctx);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @AfterClass public void teardown()
		public virtual void teardown()
		{
			eventLoopGroup.shutdownGracefully();
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testAckTracker() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		public virtual void testAckTracker()
		{
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.apache.pulsar.client.impl.conf.ConsumerConfigurationData<?> conf = new org.apache.pulsar.client.impl.conf.ConsumerConfigurationData<>();
			ConsumerConfigurationData<object> conf = new ConsumerConfigurationData<object>();
			conf.AcknowledgementsGroupTimeMicros = TimeUnit.SECONDS.toMicros(10);
			PersistentAcknowledgmentsGroupingTracker tracker = new PersistentAcknowledgmentsGroupingTracker(consumer, conf, eventLoopGroup);

			MessageIdImpl msg1 = new MessageIdImpl(5, 1, 0);
			MessageIdImpl msg2 = new MessageIdImpl(5, 2, 0);
			MessageIdImpl msg3 = new MessageIdImpl(5, 3, 0);
			MessageIdImpl msg4 = new MessageIdImpl(5, 4, 0);
			MessageIdImpl msg5 = new MessageIdImpl(5, 5, 0);
			MessageIdImpl msg6 = new MessageIdImpl(5, 6, 0);

			assertFalse(tracker.isDuplicate(msg1));

			tracker.addAcknowledgment(msg1, AckType.Individual, Collections.emptyMap());
			assertTrue(tracker.isDuplicate(msg1));

			assertFalse(tracker.isDuplicate(msg2));

			tracker.addAcknowledgment(msg5, AckType.Cumulative, Collections.emptyMap());
			assertTrue(tracker.isDuplicate(msg1));
			assertTrue(tracker.isDuplicate(msg2));
			assertTrue(tracker.isDuplicate(msg3));

			assertTrue(tracker.isDuplicate(msg4));
			assertTrue(tracker.isDuplicate(msg5));
			assertFalse(tracker.isDuplicate(msg6));

			// Flush while disconnected. the internal tracking will not change
			tracker.flush();

			assertTrue(tracker.isDuplicate(msg1));
			assertTrue(tracker.isDuplicate(msg2));
			assertTrue(tracker.isDuplicate(msg3));

			assertTrue(tracker.isDuplicate(msg4));
			assertTrue(tracker.isDuplicate(msg5));
			assertFalse(tracker.isDuplicate(msg6));

			tracker.addAcknowledgment(msg6, AckType.Individual, Collections.emptyMap());
			assertTrue(tracker.isDuplicate(msg6));

			when(consumer.ClientCnx).thenReturn(cnx);

			tracker.flush();

			assertTrue(tracker.isDuplicate(msg1));
			assertTrue(tracker.isDuplicate(msg2));
			assertTrue(tracker.isDuplicate(msg3));

			assertTrue(tracker.isDuplicate(msg4));
			assertTrue(tracker.isDuplicate(msg5));
			assertFalse(tracker.isDuplicate(msg6));

			tracker.close();
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testImmediateAckingTracker() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		public virtual void testImmediateAckingTracker()
		{
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.apache.pulsar.client.impl.conf.ConsumerConfigurationData<?> conf = new org.apache.pulsar.client.impl.conf.ConsumerConfigurationData<>();
			ConsumerConfigurationData<object> conf = new ConsumerConfigurationData<object>();
			conf.AcknowledgementsGroupTimeMicros = 0;
			PersistentAcknowledgmentsGroupingTracker tracker = new PersistentAcknowledgmentsGroupingTracker(consumer, conf, eventLoopGroup);

			MessageIdImpl msg1 = new MessageIdImpl(5, 1, 0);
			MessageIdImpl msg2 = new MessageIdImpl(5, 2, 0);

			assertFalse(tracker.isDuplicate(msg1));

			when(consumer.ClientCnx).thenReturn(null);

			tracker.addAcknowledgment(msg1, AckType.Individual, Collections.emptyMap());
			assertFalse(tracker.isDuplicate(msg1));

			when(consumer.ClientCnx).thenReturn(cnx);

			tracker.flush();
			assertFalse(tracker.isDuplicate(msg1));

			tracker.addAcknowledgment(msg2, AckType.Individual, Collections.emptyMap());
			// Since we were connected, the ack went out immediately
			assertFalse(tracker.isDuplicate(msg2));
			tracker.close();
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testAckTrackerMultiAck() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		public virtual void testAckTrackerMultiAck()
		{
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.apache.pulsar.client.impl.conf.ConsumerConfigurationData<?> conf = new org.apache.pulsar.client.impl.conf.ConsumerConfigurationData<>();
			ConsumerConfigurationData<object> conf = new ConsumerConfigurationData<object>();
			conf.AcknowledgementsGroupTimeMicros = TimeUnit.SECONDS.toMicros(10);
			PersistentAcknowledgmentsGroupingTracker tracker = new PersistentAcknowledgmentsGroupingTracker(consumer, conf, eventLoopGroup);

			when(cnx.RemoteEndpointProtocolVersion).thenReturn(ProtocolVersion.v12_VALUE);

			MessageIdImpl msg1 = new MessageIdImpl(5, 1, 0);
			MessageIdImpl msg2 = new MessageIdImpl(5, 2, 0);
			MessageIdImpl msg3 = new MessageIdImpl(5, 3, 0);
			MessageIdImpl msg4 = new MessageIdImpl(5, 4, 0);
			MessageIdImpl msg5 = new MessageIdImpl(5, 5, 0);
			MessageIdImpl msg6 = new MessageIdImpl(5, 6, 0);

			assertFalse(tracker.isDuplicate(msg1));

			tracker.addAcknowledgment(msg1, AckType.Individual, Collections.emptyMap());
			assertTrue(tracker.isDuplicate(msg1));

			assertFalse(tracker.isDuplicate(msg2));

			tracker.addAcknowledgment(msg5, AckType.Cumulative, Collections.emptyMap());
			assertTrue(tracker.isDuplicate(msg1));
			assertTrue(tracker.isDuplicate(msg2));
			assertTrue(tracker.isDuplicate(msg3));

			assertTrue(tracker.isDuplicate(msg4));
			assertTrue(tracker.isDuplicate(msg5));
			assertFalse(tracker.isDuplicate(msg6));

			// Flush while disconnected. the internal tracking will not change
			tracker.flush();

			assertTrue(tracker.isDuplicate(msg1));
			assertTrue(tracker.isDuplicate(msg2));
			assertTrue(tracker.isDuplicate(msg3));

			assertTrue(tracker.isDuplicate(msg4));
			assertTrue(tracker.isDuplicate(msg5));
			assertFalse(tracker.isDuplicate(msg6));

			tracker.addAcknowledgment(msg6, AckType.Individual, Collections.emptyMap());
			assertTrue(tracker.isDuplicate(msg6));

			when(consumer.ClientCnx).thenReturn(cnx);

			tracker.flush();

			assertTrue(tracker.isDuplicate(msg1));
			assertTrue(tracker.isDuplicate(msg2));
			assertTrue(tracker.isDuplicate(msg3));

			assertTrue(tracker.isDuplicate(msg4));
			assertTrue(tracker.isDuplicate(msg5));
			assertFalse(tracker.isDuplicate(msg6));

			tracker.close();
		}
	}

}