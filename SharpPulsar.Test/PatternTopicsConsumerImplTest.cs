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
	public class PatternTopicsConsumerImplTest
	{
		private const long TestTimeout = 90000; // 1.5 min
		private static readonly Logger _log = LoggerFactory.getLogger(typeof(PatternTopicsConsumerImplTest));
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

		// verify consumer create success, and works well.
		public virtual void TestBinaryProtoToGetTopicsOfNamespaceNonPersistent()
		{
			string key = "BinaryProtoToGetTopics";
			string subscriptionName = "my-ex-subscription-" + key;
			string topicName1 = "persistent://my-property/my-ns/np-pattern-topic-1-" + key;
			string topicName2 = "persistent://my-property/my-ns/np-pattern-topic-2-" + key;
			string topicName3 = "persistent://my-property/my-ns/np-pattern-topic-3-" + key;
			string topicName4 = "non-persistent://my-property/my-ns/np-pattern-topic-4-" + key;
			Pattern pattern = Pattern.compile("my-property/my-ns/np-pattern-topic.*");

			// 1. create partition
			TenantInfo tenantInfo = CreateDefaultTenantInfo();
			Admin.Tenants().CreateTenant("prop", tenantInfo);
			Admin.Topics().CreatePartitionedTopic(topicName2, 2);
			Admin.Topics().CreatePartitionedTopic(topicName3, 3);

			// 2. create producer
			string messagePredicate = "my-message-" + key + "-";
			int totalMessages = 40;

			Producer<sbyte[]> producer1 = PulsarClient.NewProducer().Topic(topicName1).EnableBatching(false).MessageRoutingMode(MessageRoutingMode.SinglePartition).Create();
			Producer<sbyte[]> producer2 = PulsarClient.NewProducer().Topic(topicName2).EnableBatching(false).MessageRoutingMode(MessageRoutingMode.RoundRobinPartition).Create();
			Producer<sbyte[]> producer3 = PulsarClient.NewProducer().Topic(topicName3).EnableBatching(false).MessageRoutingMode(MessageRoutingMode.RoundRobinPartition).Create();
			Producer<sbyte[]> producer4 = PulsarClient.NewProducer().Topic(topicName4).EnableBatching(false).Create();

			Consumer<sbyte[]> consumer = PulsarClient.NewConsumer().TopicsPattern(pattern).PatternAutoDiscoveryPeriod(2).SubscriptionName(subscriptionName).SubscriptionType(SubscriptionType.Shared).AckTimeout(_ackTimeOutMillis, TimeUnit.MILLISECONDS).SubscriptionTopicsMode(RegexSubscriptionMode.NonPersistentOnly).Subscribe();

			// 4. verify consumer get methods, to get right number of partitions and topics.

			assertSame(pattern, ((PatternMultiTopicsConsumerImpl<object>) consumer).Pattern);

			IList<string> topics = ((PatternMultiTopicsConsumerImpl<object>) consumer).PartitionedTopics;
			IList<ConsumerImpl<sbyte[]>> consumers = ((PatternMultiTopicsConsumerImpl<sbyte[]>) consumer).Consumers;

			assertEquals(topics.Count, 1);
			assertEquals(consumers.Count, 1);

			assertEquals(((PatternMultiTopicsConsumerImpl<object>) consumer).Topics.Count, 1);

			topics.ForEach(topic => _log.debug("topic: {}", topic));
			consumers.ForEach(c => _log.debug("consumer: {}", c.Topic));

			Enumerable.Range(0, topics.Count).ForEach(index => assertEquals(consumers[index].Topic, topics[index]));
			((PatternMultiTopicsConsumerImpl<object>) consumer).Topics.ForEach(topic => _log.debug("getTopics topic: {}", topic));

			// 5. produce data
			for(int i = 0; i < totalMessages / 4; i++)
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
			assertEquals(messageSet, totalMessages / 4);

			consumer.Unsubscribe();
			consumer.close();
			producer1.close();
			producer2.close();
			producer3.close();
			producer4.close();
		}

		// verify consumer create success, and works well.

		public virtual void TestBinaryProtoToGetTopicsOfNamespaceAll()
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
			int totalMessages = 40;

			Producer<sbyte[]> producer1 = PulsarClient.NewProducer().Topic(topicName1).EnableBatching(false).MessageRoutingMode(MessageRoutingMode.SinglePartition).Create();
			Producer<sbyte[]> producer2 = PulsarClient.NewProducer().Topic(topicName2).EnableBatching(false).MessageRoutingMode(MessageRoutingMode.RoundRobinPartition).Create();
			Producer<sbyte[]> producer3 = PulsarClient.NewProducer().Topic(topicName3).EnableBatching(false).MessageRoutingMode(MessageRoutingMode.RoundRobinPartition).Create();
			Producer<sbyte[]> producer4 = PulsarClient.NewProducer().Topic(topicName4).EnableBatching(false).Create();

			Consumer<sbyte[]> consumer = PulsarClient.NewConsumer().TopicsPattern(pattern).PatternAutoDiscoveryPeriod(2).SubscriptionName(subscriptionName).SubscriptionType(SubscriptionType.Shared).SubscriptionTopicsMode(RegexSubscriptionMode.AllTopics).AckTimeout(_ackTimeOutMillis, TimeUnit.MILLISECONDS).Subscribe();

			// 4. verify consumer get methods, to get right number of partitions and topics.

			assertSame(pattern, ((PatternMultiTopicsConsumerImpl<object>) consumer).Pattern);

			IList<string> topics = ((PatternMultiTopicsConsumerImpl<object>) consumer).PartitionedTopics;
			IList<ConsumerImpl<sbyte[]>> consumers = ((PatternMultiTopicsConsumerImpl<sbyte[]>) consumer).Consumers;

			assertEquals(topics.Count, 7);
			assertEquals(consumers.Count, 7);

			assertEquals(((PatternMultiTopicsConsumerImpl<object>) consumer).Topics.Count, 4);

			topics.ForEach(topic => _log.debug("topic: {}", topic));
			consumers.ForEach(c => _log.debug("consumer: {}", c.Topic));

			Enumerable.Range(0, topics.Count).ForEach(index => assertEquals(consumers[index].Topic, topics[index]));

			((PatternMultiTopicsConsumerImpl<object>) consumer).Topics.ForEach(topic => _log.debug("getTopics topic: {}", topic));

			// 5. produce data
			for(int i = 0; i < totalMessages / 4; i++)
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


		public virtual void TestTopicsPatternFilter()
		{
			string topicName1 = "persistent://my-property/my-ns/pattern-topic-1";
			string topicName2 = "persistent://my-property/my-ns/pattern-topic-2";
			string topicName3 = "persistent://my-property/my-ns/hello-3";
			string topicName4 = "non-persistent://my-property/my-ns/hello-4";

			IList<string> topicsNames = Lists.newArrayList(topicName1, topicName2, topicName3, topicName4);

			Pattern pattern1 = Pattern.compile("persistent://my-property/my-ns/pattern-topic.*");
			IList<string> result1 = PulsarClientImpl.TopicsPatternFilter(topicsNames, pattern1);
			assertTrue(result1.Count == 2 && result1.Contains(topicName1) && result1.Contains(topicName2));

			Pattern pattern2 = Pattern.compile("persistent://my-property/my-ns/.*");
			IList<string> result2 = PulsarClientImpl.TopicsPatternFilter(topicsNames, pattern2);
			assertTrue(result2.Count == 4 && Stream.of(topicName1, topicName2, topicName3, topicName4).allMatch(result2.contains));
		}

		public virtual void TestTopicsListMinus()
		{
			string topicName1 = "persistent://my-property/my-ns/pattern-topic-1";
			string topicName2 = "persistent://my-property/my-ns/pattern-topic-2";
			string topicName3 = "persistent://my-property/my-ns/pattern-topic-3";
			string topicName4 = "persistent://my-property/my-ns/pattern-topic-4";
			string topicName5 = "persistent://my-property/my-ns/pattern-topic-5";
			string topicName6 = "persistent://my-property/my-ns/pattern-topic-6";

			IList<string> oldNames = Lists.newArrayList(topicName1, topicName2, topicName3, topicName4);
			IList<string> newNames = Lists.newArrayList(topicName3, topicName4, topicName5, topicName6);

			IList<string> addedNames = PatternMultiTopicsConsumerImpl.TopicsListsMinus(newNames, oldNames);
			IList<string> removedNames = PatternMultiTopicsConsumerImpl.TopicsListsMinus(oldNames, newNames);

			assertTrue(addedNames.Count == 2 && addedNames.Contains(topicName5) && addedNames.Contains(topicName6));
			assertTrue(removedNames.Count == 2 && removedNames.Contains(topicName1) && removedNames.Contains(topicName2));

			// totally 2 different list, should return content of first lists.
			IList<string> addedNames2 = PatternMultiTopicsConsumerImpl.TopicsListsMinus(addedNames, removedNames);
			assertTrue(addedNames2.Count == 2 && addedNames2.Contains(topicName5) && addedNames2.Contains(topicName6));

			// 2 same list, should return empty list.
			IList<string> addedNames3 = PatternMultiTopicsConsumerImpl.TopicsListsMinus(addedNames, addedNames);
			assertEquals(addedNames3.Count, 0);

			// empty list minus: addedNames2.size = 2, addedNames3.size = 0
			IList<string> addedNames4 = PatternMultiTopicsConsumerImpl.TopicsListsMinus(addedNames2, addedNames3);
			assertEquals(addedNames2.Count, addedNames4.Count);
			addedNames4.ForEach(name => assertTrue(addedNames2.Contains(name)));

			IList<string> addedNames5 = PatternMultiTopicsConsumerImpl.TopicsListsMinus(addedNames3, addedNames2);
			assertEquals(addedNames5.Count, 0);
		}

		// simulate subscribe a pattern which has no topics, but then matched topics added in.

		public virtual void TestStartEmptyPatternConsumer()
		{
			string key = "StartEmptyPatternConsumerTest";
			string subscriptionName = "my-ex-subscription-" + key;
			string topicName1 = "persistent://my-property/my-ns/pattern-topic-1-" + key;
			string topicName2 = "persistent://my-property/my-ns/pattern-topic-2-" + key;
			string topicName3 = "persistent://my-property/my-ns/pattern-topic-3-" + key;
			Pattern pattern = Pattern.compile("persistent://my-property/my-ns/pattern-topic.*");

			// 1. create partition
			TenantInfo tenantInfo = CreateDefaultTenantInfo();
			Admin.Tenants().CreateTenant("prop", tenantInfo);
			Admin.Topics().CreatePartitionedTopic(topicName2, 2);
			Admin.Topics().CreatePartitionedTopic(topicName3, 3);

			// 2. Create consumer, this should success, but with empty sub-consumser internal
			Consumer<sbyte[]> consumer = PulsarClient.NewConsumer().TopicsPattern(pattern).PatternAutoDiscoveryPeriod(2).SubscriptionName(subscriptionName).SubscriptionType(SubscriptionType.Shared).AckTimeout(_ackTimeOutMillis, TimeUnit.MILLISECONDS).ReceiverQueueSize(4).Subscribe();

			// 3. verify consumer get methods, to get 5 number of partitions and topics.

			assertSame(pattern, ((PatternMultiTopicsConsumerImpl<object>) consumer).Pattern);

			assertEquals(((PatternMultiTopicsConsumerImpl<object>) consumer).PartitionedTopics.Count, 5);

			assertEquals(((PatternMultiTopicsConsumerImpl<object>) consumer).Consumers.Count, 5);

			assertEquals(((PatternMultiTopicsConsumerImpl<object>) consumer).Topics.Count, 2);

			// 4. create producer
			string messagePredicate = "my-message-" + key + "-";
			int totalMessages = 30;

			Producer<sbyte[]> producer1 = PulsarClient.NewProducer().Topic(topicName1).EnableBatching(false).MessageRoutingMode(MessageRoutingMode.SinglePartition).Create();
			Producer<sbyte[]> producer2 = PulsarClient.NewProducer().Topic(topicName2).EnableBatching(false).MessageRoutingMode(MessageRoutingMode.RoundRobinPartition).Create();
			Producer<sbyte[]> producer3 = PulsarClient.NewProducer().Topic(topicName3).EnableBatching(false).MessageRoutingMode(MessageRoutingMode.RoundRobinPartition).Create();

			// 5. call recheckTopics to subscribe each added topics above
			_log.debug("recheck topics change");
			PatternMultiTopicsConsumerImpl<sbyte[]> consumer1 = ((PatternMultiTopicsConsumerImpl<sbyte[]>) consumer);
			consumer1.Run(consumer1.RecheckPatternTimeout);
			Thread.Sleep(100);

			// 6. verify consumer get methods, to get number of partitions and topics, value 6=1+2+3.

			assertSame(pattern, ((PatternMultiTopicsConsumerImpl<object>) consumer).Pattern);

			assertEquals(((PatternMultiTopicsConsumerImpl<object>) consumer).PartitionedTopics.Count, 6);

			assertEquals(((PatternMultiTopicsConsumerImpl<object>) consumer).Consumers.Count, 6);

			assertEquals(((PatternMultiTopicsConsumerImpl<object>) consumer).Topics.Count, 3);


			// 7. produce data
			for(int i = 0; i < totalMessages / 3; i++)
			{
				producer1.send((messagePredicate + "producer1-" + i).GetBytes());
				producer2.send((messagePredicate + "producer2-" + i).GetBytes());
				producer3.send((messagePredicate + "producer3-" + i).GetBytes());
			}

			// 8. should receive all the message
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
		}

		// simulate subscribe a pattern which has 3 topics, but then matched topic added in.

		public virtual void TestAutoSubscribePatternConsumer()
		{
			string key = "AutoSubscribePatternConsumer";
			string subscriptionName = "my-ex-subscription-" + key;
			string topicName1 = "persistent://my-property/my-ns/pattern-topic-1-" + key;
			string topicName2 = "persistent://my-property/my-ns/pattern-topic-2-" + key;
			string topicName3 = "persistent://my-property/my-ns/pattern-topic-3-" + key;
			Pattern pattern = Pattern.compile("persistent://my-property/my-ns/pattern-topic.*");

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

			Consumer<sbyte[]> consumer = PulsarClient.NewConsumer().TopicsPattern(pattern).PatternAutoDiscoveryPeriod(2).SubscriptionName(subscriptionName).SubscriptionType(SubscriptionType.Shared).AckTimeout(_ackTimeOutMillis, TimeUnit.MILLISECONDS).ReceiverQueueSize(4).Subscribe();

			assertTrue(consumer is PatternMultiTopicsConsumerImpl);

			// 4. verify consumer get methods, to get 6 number of partitions and topics: 6=1+2+3

			assertSame(pattern, ((PatternMultiTopicsConsumerImpl<object>) consumer).Pattern);

			assertEquals(((PatternMultiTopicsConsumerImpl<object>) consumer).PartitionedTopics.Count, 6);

			assertEquals(((PatternMultiTopicsConsumerImpl<object>) consumer).Consumers.Count, 6);

			assertEquals(((PatternMultiTopicsConsumerImpl<object>) consumer).Topics.Count, 3);

			// 5. produce data to topic 1,2,3; verify should receive all the message
			for(int i = 0; i < totalMessages / 3; i++)
			{
				producer1.send((messagePredicate + "producer1-" + i).GetBytes());
				producer2.send((messagePredicate + "producer2-" + i).GetBytes());
				producer3.send((messagePredicate + "producer3-" + i).GetBytes());
			}

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

			// 6. create another producer with 4 partitions
			string topicName4 = "persistent://my-property/my-ns/pattern-topic-4-" + key;
			Admin.Topics().CreatePartitionedTopic(topicName4, 4);
			Producer<sbyte[]> producer4 = PulsarClient.NewProducer().Topic(topicName4).EnableBatching(false).MessageRoutingMode(MessageRoutingMode.RoundRobinPartition).Create();

			// 7. call recheckTopics to subscribe each added topics above, verify topics number: 10=1+2+3+4
			_log.debug("recheck topics change");
			PatternMultiTopicsConsumerImpl<sbyte[]> consumer1 = ((PatternMultiTopicsConsumerImpl<sbyte[]>) consumer);
			consumer1.Run(consumer1.RecheckPatternTimeout);
			Thread.Sleep(100);

			assertEquals(((PatternMultiTopicsConsumerImpl<object>) consumer).PartitionedTopics.Count, 10);

			assertEquals(((PatternMultiTopicsConsumerImpl<object>) consumer).Consumers.Count, 10);

			assertEquals(((PatternMultiTopicsConsumerImpl<object>) consumer).Topics.Count, 4);

			// 8. produce data to topic3 and topic4, verify should receive all the message
			for(int i = 0; i < totalMessages / 2; i++)
			{
				producer3.send((messagePredicate + "round2-producer4-" + i).GetBytes());
				producer4.send((messagePredicate + "round2-producer4-" + i).GetBytes());
			}

			messageSet = 0;
			message = consumer.Receive();
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


		public virtual void TestAutoUnbubscribePatternConsumer()
		{
			string key = "AutoUnsubscribePatternConsumer";
			string subscriptionName = "my-ex-subscription-" + key;
			string topicName1 = "persistent://my-property/my-ns/pattern-topic-1-" + key;
			string topicName2 = "persistent://my-property/my-ns/pattern-topic-2-" + key;
			string topicName3 = "persistent://my-property/my-ns/pattern-topic-3-" + key;
			Pattern pattern = Pattern.compile("persistent://my-property/my-ns/pattern-topic.*");

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

			Consumer<sbyte[]> consumer = PulsarClient.NewConsumer().TopicsPattern(pattern).PatternAutoDiscoveryPeriod(10, TimeUnit.SECONDS).SubscriptionName(subscriptionName).SubscriptionType(SubscriptionType.Shared).AckTimeout(_ackTimeOutMillis, TimeUnit.MILLISECONDS).ReceiverQueueSize(4).Subscribe();

			assertTrue(consumer is PatternMultiTopicsConsumerImpl);

			// 4. verify consumer get methods, to get 0 number of partitions and topics: 6=1+2+3

			assertSame(pattern, ((PatternMultiTopicsConsumerImpl<object>) consumer).Pattern);

			assertEquals(((PatternMultiTopicsConsumerImpl<object>) consumer).PartitionedTopics.Count, 6);

			assertEquals(((PatternMultiTopicsConsumerImpl<object>) consumer).Consumers.Count, 6);

			assertEquals(((PatternMultiTopicsConsumerImpl<object>) consumer).Topics.Count, 3);

			// 5. produce data to topic 1,2,3; verify should receive all the message
			for(int i = 0; i < totalMessages / 3; i++)
			{
				producer1.send((messagePredicate + "producer1-" + i).GetBytes());
				producer2.send((messagePredicate + "producer2-" + i).GetBytes());
				producer3.send((messagePredicate + "producer3-" + i).GetBytes());
			}

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

			// 6. remove producer 1,3; verify only consumer 2 left
			// seems no direct way to verify auto-unsubscribe, because this patternConsumer also referenced the topic.
			IList<string> topicNames = Lists.newArrayList(topicName2);
			NamespaceService nss = Pulsar.NamespaceService;
			doReturn(CompletableFuture.completedFuture(topicNames)).when(nss).getListOfPersistentTopics(NamespaceName.Get("my-property/my-ns"));

			// 7. call recheckTopics to unsubscribe topic 1,3 , verify topics number: 2=6-1-3
			_log.debug("recheck topics change");
			PatternMultiTopicsConsumerImpl<sbyte[]> consumer1 = ((PatternMultiTopicsConsumerImpl<sbyte[]>) consumer);
			consumer1.Run(consumer1.RecheckPatternTimeout);
			Thread.Sleep(100);
			assertEquals(((PatternMultiTopicsConsumerImpl<sbyte[]>) consumer).PartitionedTopics.Count, 2);
			assertEquals(((PatternMultiTopicsConsumerImpl<sbyte[]>) consumer).Consumers.Count, 2);
			assertEquals(((PatternMultiTopicsConsumerImpl<sbyte[]>) consumer).Topics.Count, 1);

			// 8. produce data to topic2, verify should receive all the message
			for(int i = 0; i < totalMessages; i++)
			{
				producer2.send((messagePredicate + "round2-producer2-" + i).GetBytes());
			}

			messageSet = 0;
			message = consumer.Receive();
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
		}


		public virtual void TestTopicDeletion()
		{
			string baseTopicName = "persistent://my-property/my-ns/pattern-topic-" + DateTimeHelper.CurrentUnixTimeMillis();
			Pattern pattern = Pattern.compile(baseTopicName + ".*");

			// Create 2 topics
			Producer<string> producer1 = PulsarClient.NewProducer(Schema.STRING).Topic(baseTopicName + "-1").Create();
			Producer<string> producer2 = PulsarClient.NewProducer(Schema.STRING).Topic(baseTopicName + "-2").Create();

			Consumer<string> consumer = PulsarClient.NewConsumer(Schema.STRING).TopicsPattern(pattern).PatternAutoDiscoveryPeriod(1).SubscriptionName("sub").Subscribe();

			assertTrue(consumer is PatternMultiTopicsConsumerImpl);
			PatternMultiTopicsConsumerImpl<string> consumerImpl = (PatternMultiTopicsConsumerImpl<string>) consumer;

			// 4. verify consumer get methods
			assertSame(consumerImpl.Pattern, pattern);
			assertEquals(consumerImpl.Topics.Count, 2);

			producer1.send("msg-1");

			producer1.close();

			Message<string> message = consumer.Receive();
			assertEquals(message.Value, "msg-1");
			consumer.Acknowledge(message);

			// Force delete the topic while the regex consumer is connected
			Admin.Topics().Delete(baseTopicName + "-1", true);

			producer2.send("msg-2");

			message = consumer.Receive();
			assertEquals(message.Value, "msg-2");
			consumer.Acknowledge(message);

			assertEquals(Pulsar.BrokerService.getTopicIfExists(baseTopicName + "-1").join(), null);
			assertTrue(Pulsar.BrokerService.getTopicIfExists(baseTopicName + "-2").join().Present);
		}
	}

}