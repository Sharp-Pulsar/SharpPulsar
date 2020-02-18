using System;
using System.Threading;
using System.Threading.Tasks;
using DotNetty.Common.Utilities;
using DotNetty.Transport.Channels;
using SharpPulsar.Api;
using SharpPulsar.Impl;
using SharpPulsar.Impl.Conf;
using SharpPulsar.Impl.Schema;
using SharpPulsar.Utility.Netty;
using Xunit;

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


	/// <summary>
	/// Unit Tests of <seealso cref="PartitionedProducerImpl{T}"/>.
	/// </summary>
	public class PartitionedProducerImplTest
	{

		private const string TopicName = "testTopicName";
		private PulsarClientImpl _client;
		private ProducerBuilderImpl<sbyte[]> _producerBuilderImpl;
		private ISchema<sbyte[]> _schema;
		private ProducerInterceptors _producerInterceptors;
		private TaskCompletionSource<IProducer<sbyte[]>> _producerCreatedTask;

		public PartitionedProducerImplTest()
		{
			var mock = new Moq.Mock<PulsarClientImpl>();
			_client = mock.Object;
            _schema = new Moq.Mock<ISchema<sbyte[]>>().Object;
			_producerInterceptors = new Moq.Mock<ProducerInterceptors>().Object; 
			_producerCreatedTask = new Moq.Mock<TaskCompletionSource<IProducer<sbyte[]>>>().Object;
			ClientConfigurationData clientConfigurationData = new Moq.Mock<ClientConfigurationData>().Object;
			HashedWheelTimer timer = new Moq.Mock<HashedWheelTimer>().Object;

			_producerBuilderImpl = new ProducerBuilderImpl<sbyte[]>(_client, SchemaFields.Bytes); 

			mock.Setup(x => x.Configuration).Returns(clientConfigurationData);
            mock.Setup(x => x.Timer).Returns(timer);
            mock.Setup(x =>x.NewProducer()).Returns(_producerBuilderImpl);
		}
		[Fact]
		public void TestSinglePartitionMessageRouterImplInstance()
		{
			ProducerConfigurationData producerConfigurationData = new ProducerConfigurationData();
			producerConfigurationData.MessageRoutingMode = MessageRoutingMode.SinglePartition;

			var messageRouter = GetMessageRouter(producerConfigurationData);
			Assert.True(messageRouter is SinglePartitionMessageRouterImpl);
		}
		[Fact]
		public void TestRoundRobinPartitionMessageRouterImplInstance()
		{
			ProducerConfigurationData producerConfigurationData = new ProducerConfigurationData();
			producerConfigurationData.MessageRoutingMode = MessageRoutingMode.RoundRobinPartition;

			var messageRouter = GetMessageRouter(producerConfigurationData);
			Assert.True(messageRouter is RoundRobinPartitionMessageRouterImpl);
		}
		[Fact]
		public void TestCustomMessageRouterInstance()
		{
			ProducerConfigurationData producerConfigurationData = new ProducerConfigurationData();
			producerConfigurationData.MessageRoutingMode = MessageRoutingMode.CustomPartition;
			producerConfigurationData.CustomMessageRouter = new CustomMessageRouter(this);

			var messageRouter = GetMessageRouter(producerConfigurationData);
			Assert.True(messageRouter is CustomMessageRouter);
		}

		private IMessageRouter GetMessageRouter(ProducerConfigurationData producerConfigurationData)
		{
			var impl = new PartitionedProducerImpl<sbyte[]>(_client, TopicName, producerConfigurationData, 2, _producerCreatedTask, _schema, _producerInterceptors);

			System.Reflection.FieldInfo routerPolicy = impl.GetType().GetField("_routerPolicy");
			//routerPolicy.se.Accessible = true;
			var messageRouter = (IMessageRouter) routerPolicy.GetValue(impl);
			Assert.NotNull(messageRouter);
			return messageRouter;
		}

		[Serializable]
		public class CustomMessageRouter : IMessageRouter
		{
			private readonly PartitionedProducerImplTest _outerInstance;

			public CustomMessageRouter(PartitionedProducerImplTest outerInstance)
			{
				this._outerInstance = outerInstance;
			}

			public int ChoosePartition<T1>(IMessage<T1> msg, ITopicMetadata metadata)
			{
				int partitionIndex = int.Parse(msg.Key) % metadata.NumPartitions();
				return partitionIndex;
			}
		}
		[Fact]
		public void TestGetStats()
		{
			string topicName = "test-stats";
			ClientConfigurationData conf = new ClientConfigurationData();
			conf.ServiceUrl = "pulsar://localhost:6650";
			conf.StatsIntervalSeconds = 100;

			
			var eventLoopGroup = new MultithreadEventLoopGroup(conf.NumIoThreads);

			PulsarClientImpl clientImpl = new PulsarClientImpl(conf, eventLoopGroup);

			ProducerConfigurationData producerConfData = new ProducerConfigurationData();
			producerConfData.MessageRoutingMode = MessageRoutingMode.CustomPartition;
			producerConfData.CustomMessageRouter = new CustomMessageRouter(this);

			Assert.Equal(clientImpl.Configuration.StatsIntervalSeconds,long.Parse("100"));

			var impl = new PartitionedProducerImpl<sbyte[]>(clientImpl, topicName, producerConfData, 1, null, null, null);

			var s = impl.Stats;
		}

	}

}