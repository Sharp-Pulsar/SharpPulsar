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
namespace SharpPulsar.Test
{

	/// <summary>
	/// Unit Tests of <seealso cref="PartitionedProducer"/>.
	/// </summary>
	public class PartitionedProducerTest
	{

		private const string TopicName = "testTopicName";
		private PulsarClientImpl _client;
		private ProducerBuilderImpl _producerBuilderImpl;
		private Schema _schema;
		private ProducerInterceptors _producerInterceptors;
		private CompletableFuture<Producer> _producerCreatedFuture;

		public virtual void Setup()
		{
			_client = mock(typeof(PulsarClientImpl));
			_schema = mock(typeof(Schema));
			_producerInterceptors = mock(typeof(ProducerInterceptors));
			_producerCreatedFuture = mock(typeof(CompletableFuture));
			ClientConfigurationData clientConfigurationData = mock(typeof(ClientConfigurationData));
			Timer timer = mock(typeof(Timer));

			_producerBuilderImpl = new ProducerBuilderImpl(_client, Schema.BYTES);

			when(_client.Configuration).thenReturn(clientConfigurationData);
			when(_client.Timer()).thenReturn(timer);
			when(_client.NewProducer()).thenReturn(_producerBuilderImpl);
		}

		public virtual void TestSinglePartitionMessageRouterImplInstance()
		{
			ProducerConfigurationData producerConfigurationData = new ProducerConfigurationData();
			producerConfigurationData.MessageRoutingMode = MessageRoutingMode.SinglePartition;

			MessageRouter messageRouter = GetMessageRouter(producerConfigurationData);
			assertTrue(messageRouter is SinglePartitionMessageRouterImpl);
		}

		public virtual void TestRoundRobinPartitionMessageRouterImplInstance()
		{
			ProducerConfigurationData producerConfigurationData = new ProducerConfigurationData();
			producerConfigurationData.MessageRoutingMode = MessageRoutingMode.RoundRobinPartition;

			MessageRouter messageRouter = GetMessageRouter(producerConfigurationData);
			assertTrue(messageRouter is RoundRobinPartitionMessageRouterImpl);
		}

		public virtual void TestCustomMessageRouterInstance()
		{
			ProducerConfigurationData producerConfigurationData = new ProducerConfigurationData();
			producerConfigurationData.MessageRoutingMode = MessageRoutingMode.CustomPartition;
			producerConfigurationData.CustomMessageRouter = new CustomMessageRouter(this);

			MessageRouter messageRouter = GetMessageRouter(producerConfigurationData);
			assertTrue(messageRouter is CustomMessageRouter);
		}

		private MessageRouter GetMessageRouter(ProducerConfigurationData producerConfigurationData)
		{
			PartitionedProducerImpl impl = new PartitionedProducerImpl(_client, TopicName, producerConfigurationData, 2, _producerCreatedFuture, _schema, _producerInterceptors);

			System.Reflection.FieldInfo routerPolicy = impl.GetType().getDeclaredField("routerPolicy");
			routerPolicy.Accessible = true;
			MessageRouter messageRouter = (MessageRouter) routerPolicy.get(impl);
			assertNotNull(messageRouter);
			return messageRouter;
		}

		[Serializable]
		private class CustomMessageRouter : MessageRouter
		{
			private readonly PartitionedProducerTest _outerInstance;

			public CustomMessageRouter(PartitionedProducerTest outerInstance)
			{
				this._outerInstance = outerInstance;
			}

			public virtual int ChoosePartition<T1>(Message<T1> msg, TopicMetadata metadata)
			{
				int partitionIndex = int.Parse(msg.Key) % metadata.NumPartitions();
				return partitionIndex;
			}
		}

		public virtual void TestGetStats()
		{
			string topicName = "test-stats";
			ClientConfigurationData conf = new ClientConfigurationData();
			conf.ServiceUrl = "pulsar://localhost:6650";
			conf.StatsIntervalSeconds = 100;

			ThreadFactory threadFactory = new DefaultThreadFactory("client-test-stats", Thread.CurrentThread.Daemon);
			EventLoopGroup eventLoopGroup = EventLoopUtil.NewEventLoopGroup(conf.NumIoThreads, threadFactory);

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