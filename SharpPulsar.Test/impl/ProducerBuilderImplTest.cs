using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BAMCIS.Util.Concurrent;
using FakeItEasy;
using SharpPulsar.Api;
using SharpPulsar.Exceptions;
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
			var producer = A.Fake<IProducer<sbyte[]>>();
			var clientConfigurationData = A.Fake<ClientConfigurationData>(x => x.ConfigureFake(c => c.ServiceUrl = "pulsar://localhost:6650"));
            _client = A.Fake<PulsarClientImpl>(x => x.WithArgumentsForConstructor(() => new PulsarClientImpl(clientConfigurationData)));

			_producerBuilderImpl = A.Fake<ProducerBuilderImpl<sbyte[]>>(x=> x.WithArgumentsForConstructor(()=> new ProducerBuilderImpl<sbyte[]>(_client, SchemaFields.Bytes)));
			A.CallTo(() => _client.NewProducer()).Returns(_producerBuilderImpl);

            A.CallTo(() => _client.CreateProducerAsync(A<ProducerConfigurationData>._, A<ISchema<sbyte[]>>._,  A<ProducerInterceptors>._)).Returns(new ValueTask<IProducer<sbyte[]>>(producer));
		}

		[Fact]
		public  void TestProducerBuilderImpl()
		{
			IDictionary<string, string> properties = new Dictionary<string, string>();
			properties["Test-Key2"] = "Test-Value2";
			var producer = _producerBuilderImpl.Topic(TopicName).ProducerName("Test-Producer").MaxPendingMessages(2).AddEncryptionKey("Test-EncryptionKey").Property("Test-Key", "Test-Value").Properties(properties).Create();

			Assert.NotNull(producer);
		}

		[Fact]
		public  void TestProducerBuilderImplWhenMessageRoutingModeAndMessageRouterAreNotSet()
		{
			var producer = _producerBuilderImpl.Topic(TopicName).Create();
            Assert.NotNull(producer);
		}
        [Fact]
		public  void TestProducerBuilderImplWhenMessageRoutingModeIsSinglePartition()
		{
			var producer = _producerBuilderImpl.Topic(TopicName).MessageRoutingMode(MessageRoutingMode.SinglePartition).Create();
            Assert.NotNull(producer);
		}
        [Fact]
		public  void TestProducerBuilderImplWhenMessageRoutingModeIsRoundRobinPartition()
		{
			var producer = _producerBuilderImpl.Topic(TopicName).MessageRoutingMode(MessageRoutingMode.RoundRobinPartition).Create();
            Assert.NotNull(producer);
		}
		[Fact]
		public  void TestProducerBuilderImplWhenMessageRoutingIsSetImplicitly()
		{
			var producer = _producerBuilderImpl.Topic(TopicName).MessageRouter(new CustomMessageRouter(this)).Create();
            Assert.NotNull(producer);
		}
        [Fact]
		public  void TestProducerBuilderImplWhenMessageRoutingIsCustomPartition()
		{
			var producer = _producerBuilderImpl.Topic(TopicName).MessageRoutingMode(MessageRoutingMode.CustomPartition).MessageRouter(new CustomMessageRouter(this)).Create();
            Assert.NotNull(producer);
		}
        [Fact]
		public  void TestProducerBuilderImplWhenMessageRoutingModeIsSinglePartitionAndMessageRouterIsSet()
        {
            var exception = Assert.Throws<PulsarClientException>(() => _producerBuilderImpl.Topic(TopicName).MessageRoutingMode(MessageRoutingMode.SinglePartition).MessageRouter(new CustomMessageRouter(this)).Create());
			Assert.Equal("When 'messageRouter' is set, 'messageRoutingMode' should be set as CustomPartition", exception.Message);
		}
        [Fact]
		public  void TestProducerBuilderImplWhenMessageRoutingModeIsRoundRobinPartitionAndMessageRouterIsSet()
		{
            var exception = Assert.Throws<PulsarClientException>(() => _producerBuilderImpl.Topic(TopicName).MessageRoutingMode(MessageRoutingMode.RoundRobinPartition).MessageRouter(new CustomMessageRouter(this)).Create());
            Assert.Equal("When 'messageRouter' is set, 'messageRoutingMode' should be set as CustomPartition", exception.Message);
			
		}
        [Fact]
		public  void TestProducerBuilderImplWhenMessageRoutingModeIsCustomPartitionAndMessageRouterIsNotSet()
		{
            var exception = Assert.Throws<PulsarClientException>(() => _producerBuilderImpl.Topic(TopicName).MessageRoutingMode(MessageRoutingMode.CustomPartition).Create());
            Assert.Equal("When 'messageRouter' is set, 'messageRoutingMode' should be set as CustomPartition", exception.Message);
			
		}
        [Fact]
		public  void TestProducerBuilderImplWhenTopicNameIsNull()
		{
            var exception = Assert.Throws<ArgumentException>(() => _producerBuilderImpl.Topic(null).Create());
            Assert.Equal("topicName cannot be blank or null", exception.Message);
			
		}
        [Fact]
		public  void TestProducerBuilderImplWhenTopicNameIsBlank()
		{
            var exception = Assert.Throws<ArgumentException>(() => _producerBuilderImpl.Topic("   ").Create());
            Assert.Equal("topicName cannot be blank or null", exception.Message);
		}
		[Fact]
		public  void TestProducerBuilderImplWhenProducerNameIsNull()
		{
            var exception = Assert.Throws<ArgumentException>(() => _producerBuilderImpl.Topic(TopicName).ProducerName(null).Create());
            Assert.Equal("producerName cannot be blank or null", exception.Message);
        }
		[Fact]
		public  void TestProducerBuilderImplWhenProducerNameIsBlank()
		{
            var exception = Assert.Throws<ArgumentException>(() => _producerBuilderImpl.Topic(TopicName).ProducerName("   ").Create());
            Assert.Equal("producerName cannot be blank or null", exception.Message);
        }

		[Fact]
		public  void TestProducerBuilderImplWhenSendTimeoutIsNegative()
		{
            var exception = Assert.Throws<ArgumentException>(() => _producerBuilderImpl.Topic(TopicName).ProducerName("Test-Producer").SendTimeout(-1, TimeUnit.MILLISECONDS).Create());
            Assert.Equal("sendTimeout needs to be >= 0", exception.Message);
			
		}

		[Fact]
		public  void TestProducerBuilderImplWhenMaxPendingMessagesIsNegative()
		{
            var exception = Assert.Throws<ArgumentException>(() => _producerBuilderImpl.Topic(TopicName).ProducerName("Test-Producer").MaxPendingMessages(-1).Create());
            Assert.Equal("maxPendingMessages needs to be > 0", exception.Message);
			
		}

		[Fact]
		public  void TestProducerBuilderImplWhenEncryptionKeyIsNull()
		{
            var exception = Assert.Throws<ArgumentException>(() => _producerBuilderImpl.Topic(TopicName).AddEncryptionKey(null).Create());
            Assert.Equal("Encryption key cannot be blank or null", exception.Message);
			
		}
		[Fact]
		public  void TestProducerBuilderImplWhenEncryptionKeyIsBlank()
		{
            var exception = Assert.Throws<ArgumentException>(() => _producerBuilderImpl.Topic(TopicName).AddEncryptionKey("   ").Create());
            Assert.Equal("Encryption key cannot be blank or null", exception.Message);
		}
		[Fact]
		public  void TestProducerBuilderImplWhenPropertyKeyIsNull()
		{
            var exception = Assert.Throws<ArgumentException>(() => _producerBuilderImpl.Topic(TopicName).Property(null, "Test-Value").Create());
            Assert.Equal("property key cannot be blank or null", exception.Message);
			
		}
		[Fact]
		public  void TestProducerBuilderImplWhenPropertyKeyIsBlank()
		{
            var exception = Assert.Throws<ArgumentException>(() => _producerBuilderImpl.Topic(TopicName).Property("   ", "Test-Value").Create());
            Assert.Equal("property key cannot be blank or null", exception.Message);
		}
		[Fact]
		public  void TestProducerBuilderImplWhenPropertyValueIsNull()
		{
            var exception = Assert.Throws<ArgumentException>(() => _producerBuilderImpl.Topic(TopicName).Property("Test-Key", null).Create());
            Assert.Equal("property value cannot be blank or null", exception.Message);
		}
		[Fact]
		public  void TestProducerBuilderImplWhenPropertyValueIsBlank()
		{
            var exception = Assert.Throws<ArgumentException>(() => _producerBuilderImpl.Topic(TopicName).Property("Test-Key", "   ").Create());
            Assert.Equal("property value cannot be blank or null", exception.Message);
		}
		[Fact]
		public  void TestProducerBuilderImplWhenPropertiesIsNull()
		{
            var exception = Assert.Throws<ArgumentException>(() => _producerBuilderImpl.Topic(TopicName).Properties(null).Create());
            Assert.Equal("properties cannot be null", exception.Message);
		}
		
		[Fact]
		public  void TestProducerBuilderImplWhenPropertiesKeyIsBlank()
		{
			IDictionary<string, string> properties = new Dictionary<string, string>
			{
				["   "] = "Test-Value"
			};
            var exception = Assert.Throws<ArgumentException>(() => _producerBuilderImpl.Topic(TopicName).Properties(properties).Create());
            Assert.Equal("properties' key/value cannot be blank", exception.Message);
		}
		[Fact]
		public  void TestProducerBuilderImplWhenPropertiesValueIsNull()
		{
			IDictionary<string, string> properties = new Dictionary<string, string>
			{
				["Test-Key"] = null
			};
            var exception = Assert.Throws<ArgumentException>(() => _producerBuilderImpl.Topic(TopicName).Properties(properties).Create());
            Assert.Equal("properties' key/value cannot be blank", exception.Message);
		}
		[Fact]
		public  void TestProducerBuilderImplWhenPropertiesValueIsBlank()
		{
			IDictionary<string, string> properties = new Dictionary<string, string>
			{
				["Test-Key"] = "   "
			};
            var exception = Assert.Throws<ArgumentException>(() => _producerBuilderImpl.Topic(TopicName).Properties(properties).Create());
            Assert.Equal("properties' key/value cannot be blank", exception.Message);
		}
		[Fact]
		public  void TestProducerBuilderImplWhenPropertiesIsEmpty()
		{
			IDictionary<string, string> properties = new Dictionary<string, string>();
            var exception = Assert.Throws<ArgumentException>(() => _producerBuilderImpl.Topic(TopicName).Properties(properties).Create());
            Assert.Equal("properties cannot be empty", exception.Message);
		}
		[Fact]
		public  void TestProducerBuilderImplWhenBatchingMaxPublishDelayPropertyIsNegative()
        {
            var exception = Assert.Throws<ArgumentException>(() => _producerBuilderImpl.BatchingMaxPublishDelay(-1, TimeUnit.MILLISECONDS));
			Assert.Equal("configured value for batch delay must be at least 1ms", exception.Message);
        }
		[Fact]
		public  void TestProducerBuilderImplWhenSendTimeoutPropertyIsNegative()
		{
            var exception = Assert.Throws<ArgumentException>(() => _producerBuilderImpl.SendTimeout(-1, TimeUnit.SECONDS));
            Assert.Equal("sendTimeout needs to be >= 0", exception.Message);
			
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