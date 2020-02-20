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
            var config = A.Fake<ConsumerConfigurationData<sbyte[]>>();
            var clientConfig = A.Fake<ClientConfigurationData>(x=> x.ConfigureFake(c=> c.ServiceUrl= "pulsar://localhost:6650"));
            var pulsarClientclient = A.Fake<PulsarClientImpl>(x=> x.WithArgumentsForConstructor(()=> new PulsarClientImpl(clientConfig)));
			
			A.CallToSet(() => config.TopicsPattern).To(new Regex(@"\w+"));
            A.CallToSet(() => config.SubscriptionName).To("testSubscriptionName");
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
			_consumerBuilderImpl.Topic(null);
		}
        [Fact]
		public void TestConsumerBuilderImplWhenTopicNamesVarargsHasNullTopic()
		{
			_consumerBuilderImpl.Topic("my-topic", null);
		}
        [Fact]
		public void TestConsumerBuilderImplWhenTopicNamesVarargsHasBlankTopic()
		{
			_consumerBuilderImpl.Topic("my-topic", "  ");
		}
        [Fact]
		public void TestConsumerBuilderImplWhenTopicNamesIsNull()
		{
			_consumerBuilderImpl.Topics(null);
		}
        [Fact]
		public  void TestConsumerBuilderImplWhenTopicNamesIsEmpty()
		{
			_consumerBuilderImpl.Topics(new List<string>());
		}
        [Fact]
		public void TestConsumerBuilderImplWhenTopicNamesHasBlankTopic()
		{
			IList<string> topicNames = new List<string>(){"my-topic", " "};
			_consumerBuilderImpl.Topics(topicNames);
		}
        [Fact]
		public void TestConsumerBuilderImplWhenTopicNamesHasNullTopic()
		{
			IList<string> topicNames = new List<string>(){"my-topic", null};
			_consumerBuilderImpl.Topics(topicNames);
		}

		[Fact]
		public void TestConsumerBuilderImplWhenSubscriptionNameIsNull()
		{
			_consumerBuilderImpl.Topic(TopicName).SubscriptionName(null);
		}

		[Fact]
		public void TestConsumerBuilderImplWhenSubscriptionNameIsBlank()
		{
			_consumerBuilderImpl.Topic(TopicName).SubscriptionName(" ");
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
			_consumerBuilderImpl.Topic(TopicName).ConsumerName(null);
		}

        [Fact]
		public void TestConsumerBuilderImplWhenConsumerNameIsBlank()
		{
			_consumerBuilderImpl.Topic(TopicName).ConsumerName(" ");
		}
        [Fact]
		public void TestConsumerBuilderImplWhenPropertyKeyIsNull()
		{
			_consumerBuilderImpl.Topic(TopicName).Property(null, "Test-Value");
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
		public void TestConsumerBuilderImplWhenPropertiesKeyIsNull()
		{
			IDictionary<string, string> properties = new Dictionary<string, string>();
			properties[null] = "Test-Value";

			_consumerBuilderImpl.Topic(TopicName).Properties(properties);
		}
        [Fact]
		public void TestConsumerBuilderImplWhenPropertiesKeyIsBlank()
		{
			IDictionary<string, string> properties = new Dictionary<string, string>();
			properties["  "] = "Test-Value";

			_consumerBuilderImpl.Topic(TopicName).Properties(properties);
		}
        [Fact]
		public void TestConsumerBuilderImplWhenPropertiesValueIsNull()
		{
			IDictionary<string, string> properties = new Dictionary<string, string>();
			properties["Test-Key"] = null;

			_consumerBuilderImpl.Topic(TopicName).Properties(properties);
		}
        [Fact]
		public void TestConsumerBuilderImplWhenPropertiesValueIsBlank()
		{
			IDictionary<string, string> properties = new Dictionary<string, string>();
			properties["Test-Key"] = "   ";

			_consumerBuilderImpl.Topic(TopicName).Properties(properties);
		}
        [Fact]
		public void TestConsumerBuilderImplWhenPropertiesIsEmpty()
		{
			IDictionary<string, string> properties = new Dictionary<string, string>();

			_consumerBuilderImpl.Topic(TopicName).Properties(properties);
		}
        [Fact]
		public void TestConsumerBuilderImplWhenPropertiesIsNull()
		{
			_consumerBuilderImpl.Topic(TopicName).Properties(null);
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
			_consumerBuilderImpl.NegativeAckRedeliveryDelay(-1, TimeUnit.MILLISECONDS);
		}
        [Fact]
		public void TestConsumerBuilderImplWhenPriorityLevelPropertyIsNegative()
		{
			_consumerBuilderImpl.PriorityLevel(-1);
		}
        [Fact]
		public void TestConsumerBuilderImplWhenMaxTotalReceiverQueueSizeAcrossPartitionsPropertyIsNegative()
		{
			_consumerBuilderImpl.MaxTotalReceiverQueueSizeAcrossPartitions(-1);
		}
        [Fact]
		public void TestConsumerBuilderImplWhenPatternAutoDiscoveryPeriodPropertyIsNegative()
		{
			_consumerBuilderImpl.PatternAutoDiscoveryPeriod(-1);
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