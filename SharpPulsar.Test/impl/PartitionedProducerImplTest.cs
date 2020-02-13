using System;
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
	using EventLoopGroup = io.netty.channel.EventLoopGroup;
	using Timer = io.netty.util.Timer;

	using DefaultThreadFactory = io.netty.util.concurrent.DefaultThreadFactory;
	using Org.Apache.Pulsar.Client.Api;
	using MessageRouter = Org.Apache.Pulsar.Client.Api.MessageRouter;
	using MessageRoutingMode = Org.Apache.Pulsar.Client.Api.MessageRoutingMode;
	using Producer = Org.Apache.Pulsar.Client.Api.Producer;
	using Org.Apache.Pulsar.Client.Api;
	using TopicMetadata = Org.Apache.Pulsar.Client.Api.TopicMetadata;
	using ClientConfigurationData = Org.Apache.Pulsar.Client.Impl.Conf.ClientConfigurationData;
	using ProducerConfigurationData = Org.Apache.Pulsar.Client.Impl.Conf.ProducerConfigurationData;
	using EventLoopUtil = Org.Apache.Pulsar.Common.Util.Netty.EventLoopUtil;
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

		private const string TopicName = "testTopicName";
		private PulsarClientImpl client;
		private ProducerBuilderImpl producerBuilderImpl;
		private Schema schema;
		private ProducerInterceptors producerInterceptors;
		private CompletableFuture<Producer> producerCreatedFuture;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @BeforeTest public void setup()
		public virtual void Setup()
		{
			client = mock(typeof(PulsarClientImpl));
			schema = mock(typeof(Schema));
			producerInterceptors = mock(typeof(ProducerInterceptors));
			producerCreatedFuture = mock(typeof(CompletableFuture));
			ClientConfigurationData ClientConfigurationData = mock(typeof(ClientConfigurationData));
			Timer Timer = mock(typeof(Timer));

			producerBuilderImpl = new ProducerBuilderImpl(client, SchemaFields.BYTES);

			when(client.Configuration).thenReturn(ClientConfigurationData);
			when(client.Timer()).thenReturn(Timer);
			when(client.NewProducer()).thenReturn(producerBuilderImpl);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testSinglePartitionMessageRouterImplInstance() throws NoSuchFieldException, IllegalAccessException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		public virtual void TestSinglePartitionMessageRouterImplInstance()
		{
			ProducerConfigurationData ProducerConfigurationData = new ProducerConfigurationData();
			ProducerConfigurationData.MessageRoutingMode = MessageRoutingMode.SinglePartition;

			MessageRouter MessageRouter = GetMessageRouter(ProducerConfigurationData);
			assertTrue(MessageRouter is SinglePartitionMessageRouterImpl);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testRoundRobinPartitionMessageRouterImplInstance() throws NoSuchFieldException, IllegalAccessException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		public virtual void TestRoundRobinPartitionMessageRouterImplInstance()
		{
			ProducerConfigurationData ProducerConfigurationData = new ProducerConfigurationData();
			ProducerConfigurationData.MessageRoutingMode = MessageRoutingMode.RoundRobinPartition;

			MessageRouter MessageRouter = GetMessageRouter(ProducerConfigurationData);
			assertTrue(MessageRouter is RoundRobinPartitionMessageRouterImpl);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testCustomMessageRouterInstance() throws NoSuchFieldException, IllegalAccessException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		public virtual void TestCustomMessageRouterInstance()
		{
			ProducerConfigurationData ProducerConfigurationData = new ProducerConfigurationData();
			ProducerConfigurationData.MessageRoutingMode = MessageRoutingMode.CustomPartition;
			ProducerConfigurationData.CustomMessageRouter = new CustomMessageRouter(this);

			MessageRouter MessageRouter = GetMessageRouter(ProducerConfigurationData);
			assertTrue(MessageRouter is CustomMessageRouter);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.apache.pulsar.client.api.MessageRouter getMessageRouter(org.apache.pulsar.client.impl.conf.ProducerConfigurationData producerConfigurationData) throws NoSuchFieldException, IllegalAccessException
		private MessageRouter GetMessageRouter(ProducerConfigurationData ProducerConfigurationData)
		{
			PartitionedProducerImpl Impl = new PartitionedProducerImpl(client, TopicName, ProducerConfigurationData, 2, producerCreatedFuture, schema, producerInterceptors);

			System.Reflection.FieldInfo RouterPolicy = Impl.GetType().getDeclaredField("routerPolicy");
			RouterPolicy.Accessible = true;
			MessageRouter MessageRouter = (MessageRouter) RouterPolicy.get(Impl);
			assertNotNull(MessageRouter);
			return MessageRouter;
		}

		[Serializable]
		public class CustomMessageRouter : MessageRouter
		{
			private readonly PartitionedProducerImplTest outerInstance;

			public CustomMessageRouter(PartitionedProducerImplTest outerInstance)
			{
				this.outerInstance = OuterInstance;
			}

			public override int ChoosePartition<T1>(Message<T1> Msg, TopicMetadata Metadata)
			{
				int PartitionIndex = int.Parse(Msg.Key) % Metadata.numPartitions();
				return PartitionIndex;
			}
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testGetStats() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		public virtual void TestGetStats()
		{
			string TopicName = "test-stats";
			ClientConfigurationData Conf = new ClientConfigurationData();
			Conf.ServiceUrl = "pulsar://localhost:6650";
			Conf.StatsIntervalSeconds = 100;

			ThreadFactory ThreadFactory = new DefaultThreadFactory("client-test-stats", Thread.CurrentThread.Daemon);
			EventLoopGroup EventLoopGroup = EventLoopUtil.newEventLoopGroup(Conf.NumIoThreads, ThreadFactory);

			PulsarClientImpl ClientImpl = new PulsarClientImpl(Conf, EventLoopGroup);

			ProducerConfigurationData ProducerConfData = new ProducerConfigurationData();
			ProducerConfData.MessageRoutingMode = MessageRoutingMode.CustomPartition;
			ProducerConfData.CustomMessageRouter = new CustomMessageRouter(this);

			assertEquals(long.Parse("100"), ClientImpl.Configuration.StatsIntervalSeconds);

			PartitionedProducerImpl Impl = new PartitionedProducerImpl(ClientImpl, TopicName, ProducerConfData, 1, null, null, null);

			Impl.Stats;
		}

	}

}