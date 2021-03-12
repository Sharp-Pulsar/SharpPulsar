using System;
using System.Collections.Generic;
using System.Linq;
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
	public class PatternTopicsConsumerTest
	{
		private const long TestTimeout = 90000; // 1.5 min
		private static readonly Logger _log = LoggerFactory.getLogger(typeof(PatternTopicsConsumerTest));
		private readonly long _ackTimeOutMillis = TimeUnit.SECONDS.toMillis(2);
		public override void Setup()
		{
			// set isTcpLookup = true, to use BinaryProtoLookupService to get topics for a pattern.
			IsTcpLookup = true;
			base.InternalSetup();
			base.ProducerBaseSetup();
		}

		public virtual void TestPatternTopicsSubscribeWithBuilderFail()
		{
			string key = "PatternTopicsSubscribeWithBuilderFail";
			string subscriptionName = "my-ex-subscription-" + key;
			string topicName1 = "persistent://my-property/my-ns/topic-1-" + key;

			string topicName2 = "persistent://my-property/my-ns/topic-2-" + key;

			string topicName3 = "persistent://my-property/my-ns/topic-3-" + key;

			string topicName4 = "non-persistent://my-property/my-ns/topic-4-" + key;
			IList<string> topicNames = Lists.newArrayList(topicName1, topicName2, topicName3, topicName4);
			const string patternString = "persistent://my-property/my-ns/pattern-topic.*";
			Pattern pattern = Pattern.compile(patternString);

			TenantInfo tenantInfo = CreateDefaultTenantInfo();
			Admin.Tenants().CreateTenant("prop", tenantInfo);
			Admin.Topics().CreatePartitionedTopic(topicName2, 2);
			Admin.Topics().CreatePartitionedTopic(topicName3, 3);

			// test failing builder with pattern and topic should fail
			try
			{
				PulsarClient.NewConsumer().TopicsPattern(pattern).Topic(topicName1).SubscriptionName(subscriptionName).SubscriptionType(SubscriptionType.Shared).AckTimeout(_ackTimeOutMillis, TimeUnit.MILLISECONDS).Subscribe();
				fail("subscribe1 with pattern and topic should fail.");
			}
			catch(PulsarClientException)
			{
				// expected
			}

			// test failing builder with pattern and topics should fail
			try
			{
				PulsarClient.NewConsumer().TopicsPattern(pattern).Topics(topicNames).SubscriptionName(subscriptionName).SubscriptionType(SubscriptionType.Shared).AckTimeout(_ackTimeOutMillis, TimeUnit.MILLISECONDS).Subscribe();
				fail("subscribe2 with pattern and topics should fail.");
			}
			catch(PulsarClientException)
			{
				// expected
			}

			// test failing builder with pattern and patternString should fail
			try
			{
				PulsarClient.NewConsumer().TopicsPattern(pattern).TopicsPattern(patternString).SubscriptionName(subscriptionName).SubscriptionType(SubscriptionType.Shared).AckTimeout(_ackTimeOutMillis, TimeUnit.MILLISECONDS).Subscribe();
				fail("subscribe3 with pattern and patternString should fail.");
			}
			catch(System.ArgumentException)
			{
				// expected
			}
		}

		public virtual void TestBinaryProtoToGetTopicsOfNamespacePersistent()
		{
			string key = "BinaryProtoToGetTopics";
			string subscriptionName = "my-ex-subscription-" + key;
			string topicName1 = "persistent://my-property/my-ns/pattern-topic-1-" + key;
			string topicName2 = "persistent://my-property/my-ns/pattern-topic-2-" + key;
			string topicName3 = "persistent://my-property/my-ns/pattern-topic-3-" + key;
			string topicName4 = "non-persistent://my-property/my-ns/pattern-topic-4-" + key;
			Pattern pattern = Pattern.compile("my-property/my-ns/pattern-topic.*");

			// 1. create partition
			TenantInfo tenantInfo = CreateDefaultTenantInfo();
			Admin.Tenants().CreateTenant("prop", tenantInfo);
			Admin.Topics().CreatePartitionedTopic(topicName2, 2);
			Admin.Topics().CreatePartitionedTopic(topicName3, 3);

			// 2. create producer
			string messagePredicate = "my-message-" + key + "-";
			int totalMessages = 30;

			Producer<sbyte[]> producer1 = PulsarClient.NewProducer().Topic(topicName1).EnableBatching(false).MessageRoutingMode(MessageRoutingMode.SinglePartition).Create();
			Producer<sbyte[]> producer2 = PulsarClient.NewProducer().Topic(topicName2).EnableBatching(false).MessageRoutingMode(MessageRoutingMode.RoundRobinPartition).Create();
			Producer<sbyte[]> producer3 = PulsarClient.NewProducer().Topic(topicName3).EnableBatching(false).MessageRoutingMode(MessageRoutingMode.RoundRobinPartition).Create();
			Producer<sbyte[]> producer4 = PulsarClient.NewProducer().Topic(topicName4).EnableBatching(false).Create();

			Consumer<sbyte[]> consumer = PulsarClient.NewConsumer().TopicsPattern(pattern).PatternAutoDiscoveryPeriod(2).SubscriptionName(subscriptionName).SubscriptionType(SubscriptionType.Shared).AckTimeout(_ackTimeOutMillis, TimeUnit.MILLISECONDS).ReceiverQueueSize(4).Subscribe();
			assertTrue(consumer.Topic.StartsWith(PatternMultiTopicsConsumerImpl.DummyTopicNamePrefix, StringComparison.Ordinal));

			// 4. verify consumer get methods, to get right number of partitions and topics.
			assertSame(pattern, ((PatternMultiTopicsConsumerImpl<object>) consumer).Pattern);

			IList<string> topics = ((PatternMultiTopicsConsumerImpl<object>) consumer).PartitionedTopics;
			IList<ConsumerImpl<sbyte[]>> consumers = ((PatternMultiTopicsConsumerImpl<sbyte[]>) consumer).Consumers;

			assertEquals(topics.Count, 6);
			assertEquals(consumers.Count, 6);

			assertEquals(((PatternMultiTopicsConsumerImpl<object>) consumer).Topics.Count, 3);

			topics.ForEach(topic => _log.debug("topic: {}", topic));
			consumers.ForEach(c => _log.debug("consumer: {}", c.Topic));

			Enumerable.Range(0, topics.Count).ForEach(index => assertEquals(consumers[index].Topic, topics[index]));

			((PatternMultiTopicsConsumerImpl<object>) consumer).Topics.ForEach(topic => _log.debug("getTopics topic: {}", topic));

			// 5. produce data
			for(int i = 0; i < totalMessages / 3; i++)
			{
				producer1.send((messagePredicate + "producer1-" + i).GetBytes());
				producer2.send((messagePredicate + "producer2-" + i).GetBytes());
				producer3.send((messagePredicate + "producer3-" + i).GetBytes());
				producer4.send((messagePredicate + "producer4-" + i).GetBytes());
			}

			// 6. should receive all the message
			int messageSet = 0;
			Message<sbyte[]> message = consumer.Receive();
			do
			{
				assertTrue(message is TopicMessageImpl);
				messageSet++;
				consumer.Acknowledge(message);
				_log.debug("Consumer acknowledged : " + new string(message.Data));
				message = consumer.Receive(500, TimeUnit.MILLISECONDS);
			} while(message != null);
			assertEquals(messageSet, totalMessages);

			consumer.Unsubscribe();
			consumer.close();
			producer1.close();
			producer2.close();
			producer3.close();
			producer4.close();
		}

		public virtual void TestPubRateOnNonPersistent()
		{
			InternalCleanup();
			Conf.MaxPublishRatePerTopicInBytes = 10000L;
			Conf.MaxPublishRatePerTopicInMessages = 100;
			Thread.Sleep(500);
			IsTcpLookup = true;
			base.InternalSetup();
			base.ProducerBaseSetup();
			TestBinaryProtoToGetTopicsOfNamespaceNonPersistent();
		}

	}

}