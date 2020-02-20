using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BAMCIS.Util.Concurrent;
using FakeItEasy;
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
	/// Unit tests of <seealso cref="ConsumerBuilderImpl{T}"/>.
	/// </summary>
	public class ConsumerBuilderImplTest
	{

		private const string TopicName = "testTopicName";
		private ConsumerBuilderImpl<sbyte[]> _consumerBuilderImpl;

		public ConsumerBuilderImplTest()
        {
            var config = A.Fake<ConsumerConfigurationData<sbyte[]>>(x=> x.ConfigureFake(c=> c.TopicsPattern = new Regex(@"\w+")).ConfigureFake(d=> d.SubscriptionName = "testSubscriptionName"));
            var clientConfig = A.Fake<ClientConfigurationData>(x=> x.ConfigureFake(c=> c.ServiceUrl= "pulsar://localhost:6650"));
            var pulsarClientclient = A.Fake<PulsarClientImpl>(x=> x.WithArgumentsForConstructor(()=> new PulsarClientImpl(clientConfig)));
			
			_consumerBuilderImpl =  A.Fake<ConsumerBuilderImpl<sbyte[]>>(x=>x.WithArgumentsForConstructor(()=> new ConsumerBuilderImpl<sbyte[]>(pulsarClientclient, config, SchemaFields.Bytes)));
		}

		[Fact]
		public  void TestConsumerBuilderImpl()
		{
			var consumer = A.Fake<IConsumer<sbyte[]>>();
            A.CallTo(() => _consumerBuilderImpl.SubscribeAsync()).Returns(new ValueTask<IConsumer<sbyte[]>>(consumer));
			Assert.NotNull(_consumerBuilderImpl.Topic(TopicName).Subscribe());
		}
        [Fact]
		public void TestConsumerBuilderImplWhenTopicNamesVarargsIsNull()
		{
            Assert.Throws<ArgumentException>(() => _consumerBuilderImpl.Topic(null));
            //Assert.Equal("Value cannot be null. (Parameter 'topicNames')", exception.Message);
		}
        [Fact]
		public void TestConsumerBuilderImplWhenTopicNamesVarargsHasNullTopic()
		{
            var exception = Assert.Throws<ArgumentException>(() => _consumerBuilderImpl.Topic("my-topic", null));
            Assert.Equal("topicNames cannot have blank topic", exception.Message);
		}
        [Fact]
		public void TestConsumerBuilderImplWhenTopicNamesVarargsHasBlankTopic()
		{
            var exception = Assert.Throws<ArgumentException>(() => _consumerBuilderImpl.Topic("my-topic", "  "));
            Assert.Equal("topicNames cannot have blank topic", exception.Message);
		}
        [Fact]
		public void TestConsumerBuilderImplWhenTopicNamesIsNull()
		{
            Assert.Throws<ArgumentException>(() => _consumerBuilderImpl.Topics(null));
            //Assert.Equal("Value cannot be null. (Parameter 'topicNames')", exception.Message);
		}
        [Fact]
		public  void TestConsumerBuilderImplWhenTopicNamesIsEmpty()
		{
            var exception = Assert.Throws<ArgumentException>(() => _consumerBuilderImpl.Topics(new List<string>()));
			Assert.Equal("Passed in topicNames should not be null or empty.", exception.Message);
        }
        [Fact]
		public void TestConsumerBuilderImplWhenTopicNamesHasBlankTopic()
		{
			IList<string> topicNames = new List<string>(){"my-topic", " "};
			var exception = Assert.Throws<ArgumentException>(() => _consumerBuilderImpl.Topics(topicNames));
            Assert.Equal("topicNames cannot have blank topic", exception.Message);
		}
        [Fact]
		public void TestConsumerBuilderImplWhenTopicNamesHasNullTopic()
		{
			IList<string> topicNames = new List<string>(){"my-topic", null};
            var exception = Assert.Throws<ArgumentException>(() => _consumerBuilderImpl.Topics(topicNames));
            Assert.Equal("topicNames cannot have blank topic", exception.Message);
		}

		[Fact]
		public void TestConsumerBuilderImplWhenSubscriptionNameIsNull()
		{
            var exception = Assert.Throws<NullReferenceException>(() => _consumerBuilderImpl.Topic(TopicName).SubscriptionName(null));
            Assert.Equal("subscriptionName cannot be blank", exception.Message);
		}

		[Fact]
		public void TestConsumerBuilderImplWhenSubscriptionNameIsBlank()
		{
            var exception = Assert.Throws<NullReferenceException>(() => _consumerBuilderImpl.Topic(TopicName).SubscriptionName(" "));
            Assert.Equal("subscriptionName cannot be blank", exception.Message);
		}
        [Fact]
		public void TestConsumerBuilderImplWhenConsumerEventListenerIsNull()
		{
			_consumerBuilderImpl.Topic(TopicName).SubscriptionName("subscriptionName").ConsumerEventListener(null);
		}

        [Fact]
		public void TestConsumerBuilderImplWhenCryptoKeyReaderIsNull()
		{
			_consumerBuilderImpl.Topic(TopicName).SubscriptionName("subscriptionName").CryptoKeyReader(null);
		}

        [Fact]
		public void TestConsumerBuilderImplWhenCryptoFailureActionIsNull()
		{
			_consumerBuilderImpl.Topic(TopicName).SubscriptionName("subscriptionName").CryptoFailureAction(null);
		}
        [Fact]
		public void TestConsumerBuilderImplWhenConsumerNameIsNull()
		{
            var exception = Assert.Throws<ArgumentException>(() => _consumerBuilderImpl.Topic(TopicName).ConsumerName(null));
			Assert.Equal("consumerName cannot be blank", exception.Message);
		}

        [Fact]
		public void TestConsumerBuilderImplWhenConsumerNameIsBlank()
		{
			var exception = Assert.Throws<ArgumentException>(()=>_consumerBuilderImpl.Topic(TopicName).ConsumerName(" "));
			Assert.Equal("consumerName cannot be blank", exception.Message);
		}
        [Fact]
		public void TestConsumerBuilderImplWhenPropertyKeyIsNull()
		{
            var exception = Assert.Throws<ArgumentNullException>(() => _consumerBuilderImpl.Topic(TopicName).Property(null, "Test-Value"));
            Assert.Equal("Value cannot be null. (Parameter 'key')", exception.Message);
		}
        [Fact]
		public void TestConsumerBuilderImplWhenPropertyKeyIsBlank()
		{
			_consumerBuilderImpl.Topic(TopicName).Property("   ", "Test-Value");
		}
        [Fact]
		public void TestConsumerBuilderImplWhenPropertyValueIsNull()
		{
			_consumerBuilderImpl.Topic(TopicName).Property("Test-Key", null);
		}

        [Fact]
		public void TestConsumerBuilderImplWhenPropertyValueIsBlank()
		{
			_consumerBuilderImpl.Topic(TopicName).Property("Test-Key", "   ");
		}
        [Fact]
		public void TestConsumerBuilderImplWhenPropertiesAreCorrect()
		{
			IDictionary<string, string> properties = new Dictionary<string, string>();
			properties["Test-Key"] = "Test-Value";
			properties["Test-Key2"] = "Test-Value2";

			_consumerBuilderImpl.Topic(TopicName).Properties(properties);
		}
        
        [Fact]
		public void TestConsumerBuilderImplWhenPropertiesKeyIsBlank()
		{
			IDictionary<string, string> properties = new Dictionary<string, string>();
			properties["  "] = "Test-Value";
            var exception = Assert.Throws<ArgumentException>(() => _consumerBuilderImpl.Topic(TopicName).Properties(properties));
			Assert.Equal("properties' key/value cannot be blank", exception.Message);
        }
        [Fact]
		public void TestConsumerBuilderImplWhenPropertiesValueIsNull()
		{
			IDictionary<string, string> properties = new Dictionary<string, string>();
			properties["Test-Key"] = null;
            var exception = Assert.Throws<ArgumentException>(() => _consumerBuilderImpl.Topic(TopicName).Properties(properties));
            Assert.Equal("properties' key/value cannot be blank", exception.Message);
		}
        [Fact]
		public void TestConsumerBuilderImplWhenPropertiesValueIsBlank()
		{
			IDictionary<string, string> properties = new Dictionary<string, string>();
			properties["Test-Key"] = "   ";
            var exception = Assert.Throws<ArgumentException>(() => _consumerBuilderImpl.Topic(TopicName).Properties(properties));
            Assert.Equal("properties' key/value cannot be blank", exception.Message);
		}
        [Fact]
		public void TestConsumerBuilderImplWhenPropertiesIsEmpty()
		{
			IDictionary<string, string> properties = new Dictionary<string, string>();
            var exception = Assert.Throws<ArgumentException>(() => _consumerBuilderImpl.Topic(TopicName).Properties(properties));
            Assert.Equal("properties cannot be empty", exception.Message);
		}
        [Fact]
		public void TestConsumerBuilderImplWhenPropertiesIsNull()
		{
            Assert.Throws<NullReferenceException>(() => _consumerBuilderImpl.Topic(TopicName).Properties(null));
           
		}
        [Fact]
		public void TestConsumerBuilderImplWhenSubscriptionInitialPositionIsNull()
		{
			_consumerBuilderImpl.Topic(TopicName).SubscriptionInitialPosition(null);
		}
        [Fact]
		public void TestConsumerBuilderImplWhenSubscriptionTopicsModeIsNull()
		{
			//_consumerBuilderImpl.Topic(TopicName).SubscriptionTopicsMode(null);
		}
        [Fact]
		public void TestConsumerBuilderImplWhenNegativeAckRedeliveryDelayPropertyIsNegative()
		{
            var exception = Assert.Throws<ArgumentException>(() => _consumerBuilderImpl.NegativeAckRedeliveryDelay(-1, TimeUnit.MILLISECONDS));
            Assert.Equal("redeliveryDelay needs to be >= 0", exception.Message);
		}
        [Fact]
		public void TestConsumerBuilderImplWhenPriorityLevelPropertyIsNegative()
		{
            var exception = Assert.Throws<ArgumentException>(() => _consumerBuilderImpl.PriorityLevel(-1));
            Assert.Equal("priorityLevel needs to be >= 0", exception.Message);
		}
        [Fact]
		public void TestConsumerBuilderImplWhenMaxTotalReceiverQueueSizeAcrossPartitionsPropertyIsNegative()
		{
			var exception = Assert.Throws<ArgumentException>(()=>_consumerBuilderImpl.MaxTotalReceiverQueueSizeAcrossPartitions(-1));
			Assert.Equal("maxTotalReceiverQueueSizeAcrossPartitions needs to be >= 0", exception.Message);
        }
        [Fact]
		public void TestConsumerBuilderImplWhenPatternAutoDiscoveryPeriodPropertyIsNegative()
		{
            var exception = Assert.Throws<ArgumentException>(() => _consumerBuilderImpl.PatternAutoDiscoveryPeriod(-1));
            Assert.Equal("periodInMinutes needs to be >= 0", exception.Message);
		}
        [Fact]
		public void TestConsumerBuilderImplWhenBatchReceivePolicyIsNull()
		{
			_consumerBuilderImpl.BatchReceivePolicy(null);
		}
        [Fact]
		public void TestConsumerBuilderImplWhenBatchReceivePolicyIsNotValid()
		{
			_consumerBuilderImpl.BatchReceivePolicy(new BatchReceivePolicy.Builder().MaxNumMessages(0).MaxNumBytes(0).Timeout(0, TimeUnit.MILLISECONDS).Build());
		}
        [Fact]
		public void TestConsumerBuilderImplWhenNumericPropertiesAreValid()
		{
			_consumerBuilderImpl.NegativeAckRedeliveryDelay(1, TimeUnit.MILLISECONDS);
			_consumerBuilderImpl.PriorityLevel(1);
			_consumerBuilderImpl.MaxTotalReceiverQueueSizeAcrossPartitions(1);
			_consumerBuilderImpl.PatternAutoDiscoveryPeriod(1);
		}

	}

}