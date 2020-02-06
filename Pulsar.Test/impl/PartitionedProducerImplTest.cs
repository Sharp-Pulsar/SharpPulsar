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
	using EventLoopGroup = io.netty.channel.EventLoopGroup;
	using Timer = io.netty.util.Timer;

	using DefaultThreadFactory = io.netty.util.concurrent.DefaultThreadFactory;
	using Message = api.Message;
	using MessageRouter = api.MessageRouter;
	using MessageRoutingMode = api.MessageRoutingMode;
	using Producer = api.Producer;
	using Schema = api.Schema;
	using TopicMetadata = api.TopicMetadata;
	using ClientConfigurationData = conf.ClientConfigurationData;
	using ProducerConfigurationData = conf.ProducerConfigurationData;
	using EventLoopUtil = common.util.netty.EventLoopUtil;
	using BeforeTest = org.testng.annotations.BeforeTest;
	using Test = org.testng.annotations.Test;


//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.testng.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.testng.Assert.assertNotNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.testng.Assert.assertTrue;

	/// <summary>
	/// Unit Tests of <seealso cref="PartitionedProducerImpl"/>.
	/// </summary>
	public class PartitionedProducerImplTest
	{

		private const string TOPIC_NAME = "testTopicName";
		private PulsarClientImpl client;
		private ProducerBuilderImpl producerBuilderImpl;
		private Schema schema;
		private ProducerInterceptors producerInterceptors;
		private CompletableFuture<Producer> producerCreatedFuture;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @BeforeTest public void setup()
		public virtual void setup()
		{
			client = mock(typeof(PulsarClientImpl));
			schema = mock(typeof(Schema));
			producerInterceptors = mock(typeof(ProducerInterceptors));
			producerCreatedFuture = mock(typeof(CompletableFuture));
			ClientConfigurationData clientConfigurationData = mock(typeof(ClientConfigurationData));
			Timer timer = mock(typeof(Timer));

			producerBuilderImpl = new ProducerBuilderImpl(client, Schema.BYTES);

			when(client.Configuration).thenReturn(clientConfigurationData);
			when(client.timer()).thenReturn(timer);
			when(client.newProducer()).thenReturn(producerBuilderImpl);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testSinglePartitionMessageRouterImplInstance() throws NoSuchFieldException, IllegalAccessException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		public virtual void testSinglePartitionMessageRouterImplInstance()
		{
			ProducerConfigurationData producerConfigurationData = new ProducerConfigurationData();
			producerConfigurationData.MessageRoutingMode = MessageRoutingMode.SinglePartition;

			MessageRouter messageRouter = getMessageRouter(producerConfigurationData);
			assertTrue(messageRouter is SinglePartitionMessageRouterImpl);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testRoundRobinPartitionMessageRouterImplInstance() throws NoSuchFieldException, IllegalAccessException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		public virtual void testRoundRobinPartitionMessageRouterImplInstance()
		{
			ProducerConfigurationData producerConfigurationData = new ProducerConfigurationData();
			producerConfigurationData.MessageRoutingMode = MessageRoutingMode.RoundRobinPartition;

			MessageRouter messageRouter = getMessageRouter(producerConfigurationData);
			assertTrue(messageRouter is RoundRobinPartitionMessageRouterImpl);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testCustomMessageRouterInstance() throws NoSuchFieldException, IllegalAccessException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		public virtual void testCustomMessageRouterInstance()
		{
			ProducerConfigurationData producerConfigurationData = new ProducerConfigurationData();
			producerConfigurationData.MessageRoutingMode = MessageRoutingMode.CustomPartition;
			producerConfigurationData.CustomMessageRouter = new CustomMessageRouter(this);

			MessageRouter messageRouter = getMessageRouter(producerConfigurationData);
			assertTrue(messageRouter is CustomMessageRouter);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.apache.pulsar.client.api.MessageRouter getMessageRouter(org.apache.pulsar.client.impl.conf.ProducerConfigurationData producerConfigurationData) throws NoSuchFieldException, IllegalAccessException
		private MessageRouter getMessageRouter(ProducerConfigurationData producerConfigurationData)
		{
			PartitionedProducerImpl impl = new PartitionedProducerImpl(client, TOPIC_NAME, producerConfigurationData, 2, producerCreatedFuture, schema, producerInterceptors);

			System.Reflection.FieldInfo routerPolicy = impl.GetType().getDeclaredField("routerPolicy");
			routerPolicy.Accessible = true;
			MessageRouter messageRouter = (MessageRouter) routerPolicy.get(impl);
			assertNotNull(messageRouter);
			return messageRouter;
		}

		private class CustomMessageRouter : MessageRouter
		{
			private readonly PartitionedProducerImplTest outerInstance;

			public CustomMessageRouter(PartitionedProducerImplTest outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public override int choosePartition<T1>(Message<T1> msg, TopicMetadata metadata)
			{
				int partitionIndex = int.Parse(msg.Key) % metadata.numPartitions();
				return partitionIndex;
			}
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testGetStats() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		public virtual void testGetStats()
		{
			string topicName = "test-stats";
			ClientConfigurationData conf = new ClientConfigurationData();
			conf.ServiceUrl = "pulsar://localhost:6650";
			conf.StatsIntervalSeconds = 100;

			ThreadFactory threadFactory = new DefaultThreadFactory("client-test-stats", Thread.CurrentThread.Daemon);
			EventLoopGroup eventLoopGroup = EventLoopUtil.newEventLoopGroup(conf.NumIoThreads, threadFactory);

			PulsarClientImpl clientImpl = new PulsarClientImpl(conf, eventLoopGroup);

			ProducerConfigurationData producerConfData = new ProducerConfigurationData();
			producerConfData.MessageRoutingMode = MessageRoutingMode.CustomPartition;
			producerConfData.CustomMessageRouter = new CustomMessageRouter(this);

			assertEquals(long.Parse("100"), clientImpl.Configuration.StatsIntervalSeconds);

			PartitionedProducerImpl impl = new PartitionedProducerImpl(clientImpl, topicName, producerConfData, 1, null, null, null);

			impl.Stats;
		}

	}

}