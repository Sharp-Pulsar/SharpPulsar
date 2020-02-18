using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BAMCIS.Util.Concurrent;
using Moq;
using SharpPulsar.Api;
using SharpPulsar.Impl;
using SharpPulsar.Impl.Conf;
using SharpPulsar.Impl.Schema;
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
	/// Unit tests of <seealso cref="ProducerBuilderImpl{T}"/>.
	/// </summary>
	public class ProducerBuilderImplTest
	{

		private const string TopicName = "testTopicName";
		private PulsarClientImpl _client;
		private ProducerBuilderImpl<sbyte[]> _producerBuilderImpl;

		public ProducerBuilderImplTest()
		{
			var producer = new Mock<IProducer<sbyte[]>>().Object;
            var mock = new Mock<PulsarClientImpl>();
			_client = mock.Object;
			_producerBuilderImpl = new ProducerBuilderImpl<sbyte[]>(_client, SchemaFields.Bytes);
			mock.Setup(x => x.NewProducer()).Returns(_producerBuilderImpl);

            mock.Setup(x =>x.CreateProducerAsync(It.IsAny<ProducerConfigurationData>(), It.IsAny<ISchema<sbyte[]>>(),  It.Is<ProducerInterceptors>(y =>  y == null))).Returns(new ValueTask<IProducer<sbyte[]>>(producer));
		}

		[Fact]
		public  void TestProducerBuilderImpl()
		{
			IDictionary<string, string> properties = new Dictionary<string, string>();
			properties["Test-Key2"] = "Test-Value2";

			_producerBuilderImpl = new ProducerBuilderImpl<sbyte[]>(_client, SchemaFields.Bytes);
			var producer = _producerBuilderImpl.Topic(TopicName).ProducerName("Test-Producer").MaxPendingMessages(2).AddEncryptionKey("Test-EncryptionKey").Property("Test-Key", "Test-Value").Properties(properties).Create();

			Assert.NotNull(producer);
		}

		[Fact]
		public  void TestProducerBuilderImplWhenMessageRoutingModeAndMessageRouterAreNotSet()
		{
			_producerBuilderImpl = new ProducerBuilderImpl<sbyte[]>(_client, SchemaFields.Bytes);
			var producer = _producerBuilderImpl.Topic(TopicName).Create();
            Assert.NotNull(producer);
		}
        [Fact]
		public  void TestProducerBuilderImplWhenMessageRoutingModeIsSinglePartition()
		{
			_producerBuilderImpl = new ProducerBuilderImpl<sbyte[]>(_client, SchemaFields.Bytes);
			var producer = _producerBuilderImpl.Topic(TopicName).MessageRoutingMode(MessageRoutingMode.SinglePartition).Create();
            Assert.NotNull(producer);
		}
        [Fact]
		public  void TestProducerBuilderImplWhenMessageRoutingModeIsRoundRobinPartition()
		{
            _producerBuilderImpl = new ProducerBuilderImpl<sbyte[]>(_client, SchemaFields.Bytes);
			var producer = _producerBuilderImpl.Topic(TopicName).MessageRoutingMode(MessageRoutingMode.RoundRobinPartition).Create();
            Assert.NotNull(producer);
		}
		[Fact]
		public  void TestProducerBuilderImplWhenMessageRoutingIsSetImplicitly()
		{
            _producerBuilderImpl = new ProducerBuilderImpl<sbyte[]>(_client, SchemaFields.Bytes);
			var producer = _producerBuilderImpl.Topic(TopicName).MessageRouter(new CustomMessageRouter(this)).Create();
            Assert.NotNull(producer);
		}
        [Fact]
		public  void TestProducerBuilderImplWhenMessageRoutingIsCustomPartition()
		{
            _producerBuilderImpl = new ProducerBuilderImpl<sbyte[]>(_client, SchemaFields.Bytes);
			var producer = _producerBuilderImpl.Topic(TopicName).MessageRoutingMode(MessageRoutingMode.CustomPartition).MessageRouter(new CustomMessageRouter(this)).Create();
            Assert.NotNull(producer);
		}
        [Fact]
		public  void TestProducerBuilderImplWhenMessageRoutingModeIsSinglePartitionAndMessageRouterIsSet()
		{
            _producerBuilderImpl = new ProducerBuilderImpl<sbyte[]>(_client, SchemaFields.Bytes);
			_producerBuilderImpl.Topic(TopicName).MessageRoutingMode(MessageRoutingMode.SinglePartition).MessageRouter(new CustomMessageRouter(this)).Create();
		}
        [Fact]
		public  void TestProducerBuilderImplWhenMessageRoutingModeIsRoundRobinPartitionAndMessageRouterIsSet()
		{
            _producerBuilderImpl = new ProducerBuilderImpl<sbyte[]>(_client, SchemaFields.Bytes);
			_producerBuilderImpl.Topic(TopicName).MessageRoutingMode(MessageRoutingMode.RoundRobinPartition).MessageRouter(new CustomMessageRouter(this)).Create();
		}
        [Fact]
		public  void TestProducerBuilderImplWhenMessageRoutingModeIsCustomPartitionAndMessageRouterIsNotSet()
		{
            _producerBuilderImpl = new ProducerBuilderImpl<sbyte[]>(_client, SchemaFields.Bytes);
			_producerBuilderImpl.Topic(TopicName).MessageRoutingMode(MessageRoutingMode.CustomPartition).Create();
		}
        [Fact]
		public  void TestProducerBuilderImplWhenTopicNameIsNull()
		{
            _producerBuilderImpl = new ProducerBuilderImpl<sbyte[]>(_client, SchemaFields.Bytes);
			_producerBuilderImpl.Topic(null).Create();
		}
        [Fact]
		public  void TestProducerBuilderImplWhenTopicNameIsBlank()
		{
            _producerBuilderImpl = new ProducerBuilderImpl<sbyte[]>(_client, SchemaFields.Bytes);
			_producerBuilderImpl.Topic("   ").Create();
		}
		[Fact]
		public  void TestProducerBuilderImplWhenProducerNameIsNull()
		{
            _producerBuilderImpl = new ProducerBuilderImpl<sbyte[]>(_client, SchemaFields.Bytes);
			_producerBuilderImpl.Topic(TopicName).ProducerName(null).Create();
		}
		[Fact]
		public  void TestProducerBuilderImplWhenProducerNameIsBlank()
		{
            _producerBuilderImpl = new ProducerBuilderImpl<sbyte[]>(_client, SchemaFields.Bytes);
			_producerBuilderImpl.Topic(TopicName).ProducerName("   ").Create();
		}

		[Fact]
		public  void TestProducerBuilderImplWhenSendTimeoutIsNegative()
		{
            _producerBuilderImpl = new ProducerBuilderImpl<sbyte[]>(_client, SchemaFields.Bytes);
			_producerBuilderImpl.Topic(TopicName).ProducerName("Test-Producer").SendTimeout(-1, TimeUnit.MILLISECONDS).Create();
		}

		[Fact]
		public  void TestProducerBuilderImplWhenMaxPendingMessagesIsNegative()
		{
            _producerBuilderImpl = new ProducerBuilderImpl<sbyte[]>(_client, SchemaFields.Bytes);
			_producerBuilderImpl.Topic(TopicName).ProducerName("Test-Producer").MaxPendingMessages(-1).Create();
		}

		[Fact]
		public  void TestProducerBuilderImplWhenEncryptionKeyIsNull()
		{
            _producerBuilderImpl = new ProducerBuilderImpl<sbyte[]>(_client, SchemaFields.Bytes);
			_producerBuilderImpl.Topic(TopicName).AddEncryptionKey(null).Create();
		}
		[Fact]
		public  void TestProducerBuilderImplWhenEncryptionKeyIsBlank()
		{
            _producerBuilderImpl = new ProducerBuilderImpl<sbyte[]>(_client, SchemaFields.Bytes);
			_producerBuilderImpl.Topic(TopicName).AddEncryptionKey("   ").Create();
		}
		[Fact]
		public  void TestProducerBuilderImplWhenPropertyKeyIsNull()
		{
            _producerBuilderImpl = new ProducerBuilderImpl<sbyte[]>(_client, SchemaFields.Bytes);
			_producerBuilderImpl.Topic(TopicName).Property(null, "Test-Value").Create();
		}
		[Fact]
		public  void TestProducerBuilderImplWhenPropertyKeyIsBlank()
		{
            _producerBuilderImpl = new ProducerBuilderImpl<sbyte[]>(_client, SchemaFields.Bytes);
			_producerBuilderImpl.Topic(TopicName).Property("   ", "Test-Value").Create();
		}
		[Fact]
		public  void TestProducerBuilderImplWhenPropertyValueIsNull()
		{
            _producerBuilderImpl = new ProducerBuilderImpl<sbyte[]>(_client, SchemaFields.Bytes);
			_producerBuilderImpl.Topic(TopicName).Property("Test-Key", null).Create();
		}
		[Fact]
		public  void TestProducerBuilderImplWhenPropertyValueIsBlank()
		{
            _producerBuilderImpl = new ProducerBuilderImpl<sbyte[]>(_client, SchemaFields.Bytes);
			_producerBuilderImpl.Topic(TopicName).Property("Test-Key", "   ").Create();
		}
		[Fact]
		public  void TestProducerBuilderImplWhenPropertiesIsNull()
		{
            _producerBuilderImpl = new ProducerBuilderImpl<sbyte[]>(_client, SchemaFields.Bytes);
			_producerBuilderImpl.Topic(TopicName).Properties(null).Create();
		}
		[Fact]
		public  void TestProducerBuilderImplWhenPropertiesKeyIsNull()
		{
			IDictionary<string, string> properties = new Dictionary<string, string>();
			properties[null] = "Test-Value";

            _producerBuilderImpl = new ProducerBuilderImpl<sbyte[]>(_client, SchemaFields.Bytes);
			_producerBuilderImpl.Topic(TopicName).Properties(properties).Create();
		}
		[Fact]
		public  void TestProducerBuilderImplWhenPropertiesKeyIsBlank()
		{
			IDictionary<string, string> properties = new Dictionary<string, string>();
			properties["   "] = "Test-Value";

            _producerBuilderImpl = new ProducerBuilderImpl<sbyte[]>(_client, SchemaFields.Bytes);
			_producerBuilderImpl.Topic(TopicName).Properties(properties).Create();
		}
		[Fact]
		public  void TestProducerBuilderImplWhenPropertiesValueIsNull()
		{
			IDictionary<string, string> properties = new Dictionary<string, string>();
			properties["Test-Key"] = null;

            _producerBuilderImpl = new ProducerBuilderImpl<sbyte[]>(_client, SchemaFields.Bytes);
			_producerBuilderImpl.Topic(TopicName).Properties(properties).Create();
		}
		[Fact]
		public  void TestProducerBuilderImplWhenPropertiesValueIsBlank()
		{
			IDictionary<string, string> properties = new Dictionary<string, string>();
			properties["Test-Key"] = "   ";

            _producerBuilderImpl = new ProducerBuilderImpl<sbyte[]>(_client, SchemaFields.Bytes);
			_producerBuilderImpl.Topic(TopicName).Properties(properties).Create();
		}
		[Fact]
		public  void TestProducerBuilderImplWhenPropertiesIsEmpty()
		{
			IDictionary<string, string> properties = new Dictionary<string, string>();

            _producerBuilderImpl = new ProducerBuilderImpl<sbyte[]>(_client, SchemaFields.Bytes);
			_producerBuilderImpl.Topic(TopicName).Properties(properties).Create();
		}
		[Fact]
		public  void TestProducerBuilderImplWhenBatchingMaxPublishDelayPropertyIsNegative()
		{
			_producerBuilderImpl.BatchingMaxPublishDelay(-1, TimeUnit.MILLISECONDS);
		}
		[Fact]
		public  void TestProducerBuilderImplWhenSendTimeoutPropertyIsNegative()
		{
			_producerBuilderImpl.SendTimeout(-1, TimeUnit.SECONDS);
		}
		[Fact]
		public  void TestProducerBuilderImplWhenMaxPendingMessagesAcrossPartitionsPropertyIsInvalid()
		{
			_producerBuilderImpl.MaxPendingMessagesAcrossPartitions(999);
		}

		[Fact]
		public  void TestProducerBuilderImplWhenNumericPropertiesAreValid()
		{
			_producerBuilderImpl.BatchingMaxPublishDelay(1, TimeUnit.SECONDS);
			_producerBuilderImpl.BatchingMaxMessages(2);
			_producerBuilderImpl.SendTimeout(1, TimeUnit.SECONDS);
			_producerBuilderImpl.MaxPendingMessagesAcrossPartitions(1000);
		}

		[Serializable]
		public class CustomMessageRouter : IMessageRouter
		{
			private readonly ProducerBuilderImplTest _outerInstance;

			public CustomMessageRouter(ProducerBuilderImplTest outerInstance)
			{
				this._outerInstance = outerInstance;
			}

			public  int ChoosePartition<T1>(IMessage<T1> msg, ITopicMetadata metadata)
			{
				int partitionIndex = int.Parse(msg.Key) % metadata.NumPartitions();
				return partitionIndex;
			}
		}
	}

}