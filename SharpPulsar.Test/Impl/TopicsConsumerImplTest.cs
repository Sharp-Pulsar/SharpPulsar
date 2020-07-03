using System;
using System.Collections.Generic;
using System.Threading;
using ProducerConsumerBase = SharpPulsar.Test.Api.ProducerConsumerBase;

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
    public class TopicsConsumerImplTest : ProducerConsumerBase
	{
		private const long testTimeout = 90000; // 1.5 min
		private static readonly Logger log = LoggerFactory.getLogger(typeof(TopicsConsumerImplTest));
		private readonly long ackTimeOutMillis = TimeUnit.SECONDS.toMillis(2);


		public override void setup()
		{
			base.internalSetup();
		}


		public override void cleanup()
		{
			base.internalCleanup();
		}

		// Verify subscribe topics from different namespace should return error.

		public virtual void testDifferentTopicsNameSubscribe()
		{
			string key = "TopicsFromDifferentNamespace";

			string subscriptionName = "my-ex-subscription-" + key;

			string topicName1 = "persistent://prop/use/ns-abc1/topic-1-" + key;
            ;
			string topicName2 = "persistent://prop/use/ns-abc2/topic-2-" + key;

			string topicName3 = "persistent://prop/use/ns-abc3/topic-3-" + key;
			IList<string> topicNames = Lists.newArrayList(topicName1, topicName2, topicName3);

			TenantInfo tenantInfo = createDefaultTenantInfo();
			admin.tenants().createTenant("prop", tenantInfo);
			admin.topics().createPartitionedTopic(topicName2, 2);
			admin.topics().createPartitionedTopic(topicName3, 3);

			// 2. Create consumer
			try
			{
				Consumer consumer = pulsarClient.newConsumer().topics(topicNames).subscriptionName(subscriptionName).subscriptionType(SubscriptionType.Shared).ackTimeout(ackTimeOutMillis, TimeUnit.MILLISECONDS).subscribe();
				assertTrue(consumer is MultiTopicsConsumerImpl);
			}
			catch (System.ArgumentException)
			{
				// expected for have different namespace
			}
		}


		public virtual void testGetConsumersAndGetTopics()
		{
			string key = "TopicsConsumerGet";

			string subscriptionName = "my-ex-subscription-" + key;


			string topicName1 = "persistent://prop/use/ns-abc/topic-1-" + key;

			string topicName2 = "persistent://prop/use/ns-abc/topic-2-" + key;

			string topicName3 = "persistent://prop/use/ns-abc/topic-3-" + key;
			IList<string> topicNames = Lists.newArrayList(topicName1, topicName2);

			TenantInfo tenantInfo = createDefaultTenantInfo();
			admin.tenants().createTenant("prop", tenantInfo);
			admin.topics().createPartitionedTopic(topicName2, 2);
			admin.topics().createPartitionedTopic(topicName3, 3);

			// 2. Create consumer
			Consumer<sbyte[]> consumer = pulsarClient.newConsumer().topics(topicNames).topic(topicName3).subscriptionName(subscriptionName).subscriptionType(SubscriptionType.Shared).ackTimeout(ackTimeOutMillis, TimeUnit.MILLISECONDS).receiverQueueSize(4).subscribe();
			assertTrue(consumer is MultiTopicsConsumerImpl);
			assertTrue(consumer.Topic.StartsWith(MultiTopicsConsumerImpl.DUMMY_TOPIC_NAME_PREFIX, StringComparison.Ordinal));

			IList<string> topics = ((MultiTopicsConsumerImpl<sbyte[]>) consumer).PartitionedTopics;
			IList<ConsumerImpl<sbyte[]>> consumers = ((MultiTopicsConsumerImpl) consumer).Consumers;

			topics.ForEach(topic => log.info("topic: {}", topic));
			consumers.ForEach(c => log.info("consumer: {}", c.Topic));

			IntStream.range(0, 6).forEach(index => assertEquals(consumers[index].Topic, topics[index]));

			assertEquals(((MultiTopicsConsumerImpl<sbyte[]>) consumer).Topics.Count, 3);

			consumer.unsubscribe();
			consumer.close();
		}


		public virtual void testSyncProducerAndConsumer()
		{
			string key = "TopicsConsumerSyncTest";

			string subscriptionName = "my-ex-subscription-" + key;

			string messagePredicate = "my-message-" + key + "-";
			const int totalMessages = 30;


			string topicName1 = "persistent://prop/use/ns-abc/topic-1-" + key;

			string topicName2 = "persistent://prop/use/ns-abc/topic-2-" + key;

			string topicName3 = "persistent://prop/use/ns-abc/topic-3-" + key;
			IList<string> topicNames = Lists.newArrayList(topicName1, topicName2, topicName3);

			TenantInfo tenantInfo = createDefaultTenantInfo();
			admin.tenants().createTenant("prop", tenantInfo);
			admin.topics().createPartitionedTopic(topicName2, 2);
			admin.topics().createPartitionedTopic(topicName3, 3);

			// 1. producer connect
			Producer<sbyte[]> producer1 = pulsarClient.newProducer().topic(topicName1).enableBatching(false).messageRoutingMode(MessageRoutingMode.SinglePartition).create();
			Producer<sbyte[]> producer2 = pulsarClient.newProducer().topic(topicName2).enableBatching(false).messageRoutingMode(MessageRoutingMode.RoundRobinPartition).create();
			Producer<sbyte[]> producer3 = pulsarClient.newProducer().topic(topicName3).enableBatching(false).messageRoutingMode(MessageRoutingMode.RoundRobinPartition).create();

			// 2. Create consumer
			Consumer<sbyte[]> consumer = pulsarClient.newConsumer().topics(topicNames).subscriptionName(subscriptionName).subscriptionType(SubscriptionType.Shared).ackTimeout(ackTimeOutMillis, TimeUnit.MILLISECONDS).receiverQueueSize(4).subscribe();
			assertTrue(consumer is MultiTopicsConsumerImpl);

			// 3. producer publish messages
			for (int i = 0; i < totalMessages / 3; i++)
			{
				producer1.send((messagePredicate + "producer1-" + i).GetBytes());
				producer2.send((messagePredicate + "producer2-" + i).GetBytes());
				producer3.send((messagePredicate + "producer3-" + i).GetBytes());
			}

			int messageSet = 0;
			Message<sbyte[]> message = consumer.receive();
			do
			{
				assertTrue(message is TopicMessageImpl);
				messageSet++;
				consumer.acknowledge(message);
				log.debug("Consumer acknowledged : " + new string(message.Data));
				message = consumer.receive(500, TimeUnit.MILLISECONDS);
			} while (message != null);
			assertEquals(messageSet, totalMessages);

			consumer.unsubscribe();
			consumer.close();
			producer1.close();
			producer2.close();
			producer3.close();
		}



		public virtual void testAsyncConsumer()
		{
			string key = "TopicsConsumerAsyncTest";

			string subscriptionName = "my-ex-subscription-" + key;

			string messagePredicate = "my-message-" + key + "-";
			const int totalMessages = 30;


			string topicName1 = "persistent://prop/use/ns-abc/topic-1-" + key;

			string topicName2 = "persistent://prop/use/ns-abc/topic-2-" + key;

			string topicName3 = "persistent://prop/use/ns-abc/topic-3-" + key;
			IList<string> topicNames = Lists.newArrayList(topicName1, topicName2, topicName3);

			TenantInfo tenantInfo = createDefaultTenantInfo();
			admin.tenants().createTenant("prop", tenantInfo);
			admin.topics().createPartitionedTopic(topicName2, 2);
			admin.topics().createPartitionedTopic(topicName3, 3);

			// 1. producer connect
			Producer<sbyte[]> producer1 = pulsarClient.newProducer().topic(topicName1).enableBatching(false).messageRoutingMode(MessageRoutingMode.SinglePartition).create();
			Producer<sbyte[]> producer2 = pulsarClient.newProducer().topic(topicName2).enableBatching(false).messageRoutingMode(MessageRoutingMode.RoundRobinPartition).create();
			Producer<sbyte[]> producer3 = pulsarClient.newProducer().topic(topicName3).enableBatching(false).messageRoutingMode(MessageRoutingMode.RoundRobinPartition).create();

			// 2. Create consumer
			Consumer<sbyte[]> consumer = pulsarClient.newConsumer().topics(topicNames).subscriptionName(subscriptionName).subscriptionType(SubscriptionType.Shared).ackTimeout(ackTimeOutMillis, TimeUnit.MILLISECONDS).receiverQueueSize(4).subscribe();
			assertTrue(consumer is MultiTopicsConsumerImpl);

			// Asynchronously produce messages
			IList<Future<MessageId>> futures = Lists.newArrayList();
			for (int i = 0; i < totalMessages / 3; i++)
			{
				futures.Add(producer1.sendAsync((messagePredicate + "producer1-" + i).GetBytes()));
				futures.Add(producer2.sendAsync((messagePredicate + "producer2-" + i).GetBytes()));
				futures.Add(producer3.sendAsync((messagePredicate + "producer3-" + i).GetBytes()));
			}
			log.info("Waiting for async publish to complete : {}", futures.Count);
			foreach (Future<MessageId> future in futures)
			{
				future.get();
			}

			log.info("start async consume");
			System.Threading.CountdownEvent latch = new System.Threading.CountdownEvent(totalMessages);
			ExecutorService executor = Executors.newFixedThreadPool(1);
			executor.execute(() => IntStream.range(0, totalMessages).forEach(index => consumer.receiveAsync().thenAccept(msg =>
			{
			assertTrue(msg is TopicMessageImpl);
			try
			{
				consumer.acknowledge(msg);
			}
			catch (PulsarClientException e1)
			{
				fail("message acknowledge failed", e1);
			}
			latch.Signal();
			log.info("receive index: {}, latch countDown: {}", index, latch.CurrentCount);
			}).exceptionally(ex =>
			{
			log.warn("receive index: {}, failed receive message {}", index, ex.Message);
			ex.printStackTrace();
			return null;
		})));

			latch.await();
			log.info("success latch wait");

			consumer.unsubscribe();
			consumer.close();
			producer1.close();
			producer2.close();
			producer3.close();
			executor.shutdownNow();
		}


		public virtual void testConsumerUnackedRedelivery()
		{
			string key = "TopicsConsumerRedeliveryTest";

			string subscriptionName = "my-ex-subscription-" + key;

			string messagePredicate = "my-message-" + key + "-";
			const int totalMessages = 30;


			string topicName1 = "persistent://prop/use/ns-abc/topic-1-" + key;

			string topicName2 = "persistent://prop/use/ns-abc/topic-2-" + key;

			string topicName3 = "persistent://prop/use/ns-abc/topic-3-" + key;
			IList<string> topicNames = Lists.newArrayList(topicName1, topicName2, topicName3);

			TenantInfo tenantInfo = createDefaultTenantInfo();
			admin.tenants().createTenant("prop", tenantInfo);
			admin.topics().createPartitionedTopic(topicName2, 2);
			admin.topics().createPartitionedTopic(topicName3, 3);

			// 1. producer connect
			Producer<sbyte[]> producer1 = pulsarClient.newProducer().topic(topicName1).enableBatching(false).messageRoutingMode(MessageRoutingMode.SinglePartition).create();
			Producer<sbyte[]> producer2 = pulsarClient.newProducer().topic(topicName2).enableBatching(false).messageRoutingMode(MessageRoutingMode.RoundRobinPartition).create();
			Producer<sbyte[]> producer3 = pulsarClient.newProducer().topic(topicName3).enableBatching(false).messageRoutingMode(MessageRoutingMode.RoundRobinPartition).create();

			// 2. Create consumer
			Consumer<sbyte[]> consumer = pulsarClient.newConsumer().topics(topicNames).subscriptionName(subscriptionName).subscriptionType(SubscriptionType.Shared).ackTimeout(ackTimeOutMillis, TimeUnit.MILLISECONDS).receiverQueueSize(4).subscribe();
			assertTrue(consumer is MultiTopicsConsumerImpl);

			// 3. producer publish messages
			for (int i = 0; i < totalMessages / 3; i++)
			{
				producer1.send((messagePredicate + "producer1-" + i).GetBytes());
				producer2.send((messagePredicate + "producer2-" + i).GetBytes());
				producer3.send((messagePredicate + "producer3-" + i).GetBytes());
			}

			// 4. Receiver receives the message, not ack, Unacked Message Tracker size should be totalMessages.
			Message<sbyte[]> message = consumer.receive();
			while (message != null)
			{
				assertTrue(message is TopicMessageImpl);
				log.debug("Consumer received : " + new string(message.Data));
				message = consumer.receive(500, TimeUnit.MILLISECONDS);
			}
			long size = ((MultiTopicsConsumerImpl<sbyte[]>) consumer).UnAckedMessageTracker.size();
			log.debug(key + " Unacked Message Tracker size is " + size);
			assertEquals(size, totalMessages);

			// 5. Blocking call, redeliver should kick in, after receive and ack, Unacked Message Tracker size should be 0.
			message = consumer.receive();
			HashSet<string> hSet = new HashSet<string>();
			do
			{
				assertTrue(message is TopicMessageImpl);
				hSet.Add(new string(message.Data));
				consumer.acknowledge(message);
				log.debug("Consumer acknowledged : " + new string(message.Data));
				message = consumer.receive(500, TimeUnit.MILLISECONDS);
			} while (message != null);

			size = ((MultiTopicsConsumerImpl<sbyte[]>) consumer).UnAckedMessageTracker.size();
			log.debug(key + " Unacked Message Tracker size is " + size);
			assertEquals(size, 0);
			assertEquals(hSet.Count, totalMessages);

			// 6. producer publish more messages
			for (int i = 0; i < totalMessages / 3; i++)
			{
				producer1.send((messagePredicate + "producer1-round2" + i).GetBytes());
				producer2.send((messagePredicate + "producer2-round2" + i).GetBytes());
				producer3.send((messagePredicate + "producer3-round2" + i).GetBytes());
			}

			// 7. Receiver receives the message, ack them
			message = consumer.receive();
			int received = 0;
			while (message != null)
			{
				assertTrue(message is TopicMessageImpl);
				received++;
				string data = new string(message.Data);
				log.debug("Consumer received : " + data);
				consumer.acknowledge(message);
				message = consumer.receive(100, TimeUnit.MILLISECONDS);
			}
			size = ((MultiTopicsConsumerImpl<sbyte[]>) consumer).UnAckedMessageTracker.size();
			log.debug(key + " Unacked Message Tracker size is " + size);
			assertEquals(size, 0);
			assertEquals(received, totalMessages);

			// 8. Simulate ackTimeout
			Thread.Sleep(ackTimeOutMillis);

			// 9. producer publish more messages
			for (int i = 0; i < totalMessages / 3; i++)
			{
				producer1.send((messagePredicate + "producer1-round3" + i).GetBytes());
				producer2.send((messagePredicate + "producer2-round3" + i).GetBytes());
				producer3.send((messagePredicate + "producer3-round3" + i).GetBytes());
			}

			// 10. Receiver receives the message, doesn't ack
			message = consumer.receive();
			while (message != null)
			{
				string data = new string(message.Data);
				log.debug("Consumer received : " + data);
				message = consumer.receive(100, TimeUnit.MILLISECONDS);
			}
			size = ((MultiTopicsConsumerImpl<sbyte[]>) consumer).UnAckedMessageTracker.size();
			log.debug(key + " Unacked Message Tracker size is " + size);
			assertEquals(size, 30);

			Thread.Sleep(ackTimeOutMillis);

			// 11. Receiver receives redelivered messages
			message = consumer.receive();
			int redelivered = 0;
			while (message != null)
			{
				assertTrue(message is TopicMessageImpl);
				redelivered++;
				string data = new string(message.Data);
				log.debug("Consumer received : " + data);
				consumer.acknowledge(message);
				message = consumer.receive(100, TimeUnit.MILLISECONDS);
			}
			assertEquals(redelivered, 30);
			size = ((MultiTopicsConsumerImpl<sbyte[]>) consumer).UnAckedMessageTracker.size();
			log.info(key + " Unacked Message Tracker size is " + size);
			assertEquals(size, 0);

			consumer.unsubscribe();
			consumer.close();
			producer1.close();
			producer2.close();
			producer3.close();
		}


		public virtual void testSubscribeUnsubscribeSingleTopic()
		{
			string key = "TopicsConsumerSubscribeUnsubscribeSingleTopicTest";

			string subscriptionName = "my-ex-subscription-" + key;

			string messagePredicate = "my-message-" + key + "-";
			const int totalMessages = 30;


			string topicName1 = "persistent://prop/use/ns-abc/topic-1-" + key;

			string topicName2 = "persistent://prop/use/ns-abc/topic-2-" + key;

			string topicName3 = "persistent://prop/use/ns-abc/topic-3-" + key;
			IList<string> topicNames = Lists.newArrayList(topicName1, topicName2, topicName3);

			TenantInfo tenantInfo = createDefaultTenantInfo();
			admin.tenants().createTenant("prop", tenantInfo);
			admin.topics().createPartitionedTopic(topicName2, 2);
			admin.topics().createPartitionedTopic(topicName3, 3);

			// 1. producer connect
			Producer<sbyte[]> producer1 = pulsarClient.newProducer().topic(topicName1).enableBatching(false).messageRoutingMode(MessageRoutingMode.SinglePartition).create();
			Producer<sbyte[]> producer2 = pulsarClient.newProducer().topic(topicName2).enableBatching(false).messageRoutingMode(MessageRoutingMode.RoundRobinPartition).create();
			Producer<sbyte[]> producer3 = pulsarClient.newProducer().topic(topicName3).enableBatching(false).messageRoutingMode(MessageRoutingMode.RoundRobinPartition).create();

			// 2. Create consumer
			Consumer<sbyte[]> consumer = pulsarClient.newConsumer().topics(topicNames).subscriptionName(subscriptionName).subscriptionType(SubscriptionType.Shared).ackTimeout(ackTimeOutMillis, TimeUnit.MILLISECONDS).receiverQueueSize(4).subscribe();
			assertTrue(consumer is MultiTopicsConsumerImpl);

			// 3. producer publish messages
			for (int i = 0; i < totalMessages / 3; i++)
			{
				producer1.send((messagePredicate + "producer1-" + i).GetBytes());
				producer2.send((messagePredicate + "producer2-" + i).GetBytes());
				producer3.send((messagePredicate + "producer3-" + i).GetBytes());
			}

			int messageSet = 0;
			Message<sbyte[]> message = consumer.receive();
			do
			{
				assertTrue(message is TopicMessageImpl);
				messageSet++;
				consumer.acknowledge(message);
				log.debug("Consumer acknowledged : " + new string(message.Data));
				message = consumer.receive(500, TimeUnit.MILLISECONDS);
			} while (message != null);
			assertEquals(messageSet, totalMessages);

			// 4, unsubscribe topic3
			CompletableFuture<Void> unsubFuture = ((MultiTopicsConsumerImpl<sbyte[]>) consumer).unsubscribeAsync(topicName3);
			unsubFuture.get();

			// 5. producer publish messages
			for (int i = 0; i < totalMessages / 3; i++)
			{
				producer1.send((messagePredicate + "producer1-round2" + i).GetBytes());
				producer2.send((messagePredicate + "producer2-round2" + i).GetBytes());
				producer3.send((messagePredicate + "producer3-round2" + i).GetBytes());
			}

			// 6. should not receive messages from topic3, verify get 2/3 of all messages
			messageSet = 0;
			message = consumer.receive();
			do
			{
				assertTrue(message is TopicMessageImpl);
				messageSet++;
				consumer.acknowledge(message);
				log.debug("Consumer acknowledged : " + new string(message.Data));
				message = consumer.receive(500, TimeUnit.MILLISECONDS);
			} while (message != null);
			assertEquals(messageSet, totalMessages * 2 / 3);

			// 7. use getter to verify internal topics number after un-subscribe topic3
			IList<string> topics = ((MultiTopicsConsumerImpl<sbyte[]>) consumer).PartitionedTopics;
			IList<ConsumerImpl<sbyte[]>> consumers = ((MultiTopicsConsumerImpl) consumer).Consumers;

			assertEquals(topics.Count, 3);
			assertEquals(consumers.Count, 3);
			assertEquals(((MultiTopicsConsumerImpl<sbyte[]>) consumer).Topics.Count, 2);

			// 8. re-subscribe topic3
			CompletableFuture<Void> subFuture = ((MultiTopicsConsumerImpl<sbyte[]>)consumer).subscribeAsync(topicName3, true);
			subFuture.get();

			// 9. producer publish messages
			for (int i = 0; i < totalMessages / 3; i++)
			{
				producer1.send((messagePredicate + "producer1-round3" + i).GetBytes());
				producer2.send((messagePredicate + "producer2-round3" + i).GetBytes());
				producer3.send((messagePredicate + "producer3-round3" + i).GetBytes());
			}

			// 10. should receive messages from all 3 topics
			messageSet = 0;
			message = consumer.receive();
			do
			{
				assertTrue(message is TopicMessageImpl);
				messageSet++;
				consumer.acknowledge(message);
				log.debug("Consumer acknowledged : " + new string(message.Data));
				message = consumer.receive(500, TimeUnit.MILLISECONDS);
			} while (message != null);
			assertEquals(messageSet, totalMessages);

			// 11. use getter to verify internal topics number after subscribe topic3
			topics = ((MultiTopicsConsumerImpl<sbyte[]>) consumer).PartitionedTopics;
			consumers = ((MultiTopicsConsumerImpl) consumer).Consumers;

			assertEquals(topics.Count, 6);
			assertEquals(consumers.Count, 6);
			assertEquals(((MultiTopicsConsumerImpl<sbyte[]>) consumer).Topics.Count, 3);

			consumer.unsubscribe();
			consumer.close();
			producer1.close();
			producer2.close();
			producer3.close();
		}


		public virtual void testTopicsNameSubscribeWithBuilderFail()
		{
			string key = "TopicsNameSubscribeWithBuilder";

			string subscriptionName = "my-ex-subscription-" + key;


			string topicName2 = "persistent://prop/use/ns-abc/topic-2-" + key;

			string topicName3 = "persistent://prop/use/ns-abc/topic-3-" + key;

			TenantInfo tenantInfo = createDefaultTenantInfo();
			admin.tenants().createTenant("prop", tenantInfo);
			admin.topics().createPartitionedTopic(topicName2, 2);
			admin.topics().createPartitionedTopic(topicName3, 3);

			// test failing builder with empty topics
			try
			{
				pulsarClient.newConsumer().subscriptionName(subscriptionName).subscriptionType(SubscriptionType.Shared).ackTimeout(ackTimeOutMillis, TimeUnit.MILLISECONDS).subscribe();
				fail("subscribe1 with no topicName should fail.");
			}
			catch (PulsarClientException)
			{
				// expected
			}

			try
			{
				pulsarClient.newConsumer().topic().subscriptionName(subscriptionName).subscriptionType(SubscriptionType.Shared).ackTimeout(ackTimeOutMillis, TimeUnit.MILLISECONDS).subscribe();
				fail("subscribe2 with no topicName should fail.");
			}
			catch (System.ArgumentException)
			{
				// expected
			}

			try
			{
				pulsarClient.newConsumer().topics(null).subscriptionName(subscriptionName).subscriptionType(SubscriptionType.Shared).ackTimeout(ackTimeOutMillis, TimeUnit.MILLISECONDS).subscribe();
				fail("subscribe3 with no topicName should fail.");
			}
			catch (System.ArgumentException)
			{
				// expected
			}

			try
			{
				pulsarClient.newConsumer().topics(Lists.newArrayList()).subscriptionName(subscriptionName).subscriptionType(SubscriptionType.Shared).ackTimeout(ackTimeOutMillis, TimeUnit.MILLISECONDS).subscribe();
				fail("subscribe4 with no topicName should fail.");
			}
			catch (System.ArgumentException)
			{
				// expected
			}
		}

		/// <summary>
		/// Test Listener for github issue #2547
		/// </summary>
		/// 
		public virtual void testMultiTopicsMessageListener()
		{
			string key = "MultiTopicsMessageListenerTest";

			string subscriptionName = "my-ex-subscription-" + key;

			string messagePredicate = "my-message-" + key + "-";
			const int totalMessages = 6;

			// set latch larger than totalMessages, so timeout message get resend
			System.Threading.CountdownEvent latch = new System.Threading.CountdownEvent(totalMessages * 3);


			string topicName1 = "persistent://prop/use/ns-abc/topic-1-" + key;
			IList<string> topicNames = Lists.newArrayList(topicName1);

			TenantInfo tenantInfo = createDefaultTenantInfo();
			admin.tenants().createTenant("prop", tenantInfo);
			admin.topics().createPartitionedTopic(topicName1, 2);

			// 1. producer connect
			Producer<sbyte[]> producer1 = pulsarClient.newProducer().topic(topicName1).enableBatching(false).messageRoutingMode(MessageRoutingMode.SinglePartition).create();

			// 2. Create consumer, set not ack in message listener, so time-out message will resend
			Consumer<sbyte[]> consumer = pulsarClient.newConsumer().topics(topicNames).subscriptionName(subscriptionName).subscriptionType(SubscriptionType.Shared).ackTimeout(1000, TimeUnit.MILLISECONDS).receiverQueueSize(100).messageListener((c1, msg) =>
			{
			assertNotNull(msg, "Message cannot be null");
			string receivedMessage = new string(msg.Data);
			latch.Signal();
			log.info("Received message [{}] in the listener, latch: {}", receivedMessage, latch.CurrentCount);
			}).subscribe();
			assertTrue(consumer is MultiTopicsConsumerImpl);

			MultiTopicsConsumerImpl topicsConsumer = (MultiTopicsConsumerImpl) consumer;

			// 3. producer publish messages
			for (int i = 0; i < totalMessages; i++)
			{
				producer1.send((messagePredicate + "producer1-" + i).GetBytes());
			}

			// verify should not time out, because of message redelivered several times.
			latch.await();

			consumer.close();
		}

		/// <summary>
		/// Test topic partitions auto subscribed.
		/// 
		/// Steps:
		/// 1. Create a consumer with 2 topics, and each topic has 2 partitions: xx-partition-0, xx-partition-1.
		/// 2. update topics to have 3 partitions.
		/// 3. trigger partitionsAutoUpdate. this should be done automatically, this is to save time to manually trigger.
		/// 4. produce message to xx-partition-2 again,  and verify consumer could receive message.
		/// 
		/// </summary>
		/// 
		public virtual void testTopicAutoUpdatePartitions()
		{
			string key = "TestTopicAutoUpdatePartitions";

			string subscriptionName = "my-ex-subscription-" + key;

			string messagePredicate = "my-message-" + key + "-";
			const int totalMessages = 6;


			string topicName1 = "persistent://prop/use/ns-abc/topic-1-" + key;

			string topicName2 = "persistent://prop/use/ns-abc/topic-2-" + key;
			IList<string> topicNames = Lists.newArrayList(topicName1, topicName2);

			TenantInfo tenantInfo = createDefaultTenantInfo();
			admin.tenants().createTenant("prop", tenantInfo);
			admin.topics().createPartitionedTopic(topicName1, 2);
			admin.topics().createPartitionedTopic(topicName2, 2);

			// 1. Create a  consumer
			Consumer<sbyte[]> consumer = pulsarClient.newConsumer().topics(topicNames).subscriptionName(subscriptionName).subscriptionType(SubscriptionType.Shared).ackTimeout(ackTimeOutMillis, TimeUnit.MILLISECONDS).receiverQueueSize(4).autoUpdatePartitions(true).subscribe();
			assertTrue(consumer is MultiTopicsConsumerImpl);

			MultiTopicsConsumerImpl topicsConsumer = (MultiTopicsConsumerImpl) consumer;

			// 2. update to 3 partitions
			admin.topics().updatePartitionedTopic(topicName1, 3);
			admin.topics().updatePartitionedTopic(topicName2, 3);

			// 3. trigger partitionsAutoUpdate. this should be done automatically in 1 minutes,
			// this is to save time to manually trigger.
			log.info("trigger partitionsAutoUpdateTimerTask");
			Timeout timeout = topicsConsumer.PartitionsAutoUpdateTimeout;
			timeout.task().run(timeout);
			Thread.Sleep(200);

			// 4. produce message to xx-partition-2,  and verify consumer could receive message.
			Producer<sbyte[]> producer1 = pulsarClient.newProducer().topic(topicName1 + "-partition-2").enableBatching(false).create();
			Producer<sbyte[]> producer2 = pulsarClient.newProducer().topic(topicName2 + "-partition-2").enableBatching(false).create();
			for (int i = 0; i < totalMessages; i++)
			{
				producer1.send((messagePredicate + "topic1-partition-2 index:" + i).GetBytes());
				producer2.send((messagePredicate + "topic2-partition-2 index:" + i).GetBytes());
				log.info("produce message to partition-2 again. messageindex: {}", i);
			}
			int messageSet = 0;
			Message<sbyte[]> message = consumer.receive();
			do
			{
				messageSet++;
				consumer.acknowledge(message);
				log.info("4 Consumer acknowledged : " + new string(message.Data));
				message = consumer.receive(200, TimeUnit.MILLISECONDS);
			} while (message != null);
			assertEquals(messageSet, 2 * totalMessages);

			consumer.close();
		}


		public virtual void testConsumerDistributionInFailoverSubscriptionWhenUpdatePartitions()
		{
			const string topicName = "persistent://prop/use/ns-abc/testConsumerDistributionInFailoverSubscriptionWhenUpdatePartitions";
			const string subName = "failover-test";
			TenantInfo tenantInfo = createDefaultTenantInfo();
			admin.tenants().createTenant("prop", tenantInfo);
			admin.topics().createPartitionedTopic(topicName, 2);
			assertEquals(admin.topics().getPartitionedTopicMetadata(topicName).partitions, 2);
			Consumer<string> consumer_1 = pulsarClient.newConsumer(Schema_Fields.STRING).topic(topicName).subscriptionType(SubscriptionType.Failover).subscriptionName(subName).subscribe();
			assertTrue(consumer_1 is MultiTopicsConsumerImpl);

			assertEquals(((MultiTopicsConsumerImpl) consumer_1).allTopicPartitionsNumber.get(), 2);

            Producer<string> producer = pulsarClient.newProducer(Schema_Fields.STRING).topic(topicName).messageRouter(new MessageRouterAnonymousInnerClass(this))int messages = 20;
				   .create();
			for (int i = 0; i < messages; i++)
			{
				producer.newMessage().key(i.ToString()).value("message - " + i).send();
			}

			int received = 0;
			Message lastMessage = null;
			for (int i = 0; i < messages; i++)
			{
				lastMessage = consumer_1.receive();
				received++;
			}
			assertEquals(received, messages);
			consumer_1.acknowledgeCumulative(lastMessage);

			// 1.Update partition and check message consumption
			admin.topics().updatePartitionedTopic(topicName, 4);
			log.info("trigger partitionsAutoUpdateTimerTask");
			Timeout timeout = ((MultiTopicsConsumerImpl) consumer_1).PartitionsAutoUpdateTimeout;
			timeout.task().run(timeout);
			Thread.Sleep(200);

			assertEquals(((MultiTopicsConsumerImpl) consumer_1).allTopicPartitionsNumber.get(), 4);
			for (int i = 0; i < messages; i++)
			{
				producer.newMessage().key(i.ToString()).value("message - " + i).send();
			}

			received = 0;
			lastMessage = null;
			for (int i = 0; i < messages; i++)
			{
				lastMessage = consumer_1.receive();
				received++;
			}
			assertEquals(received, messages);
			consumer_1.acknowledgeCumulative(lastMessage);

			// 2.Create a new consumer and check active consumer changed
			Consumer<string> consumer_2 = pulsarClient.newConsumer(Schema_Fields.STRING).topic(topicName).subscriptionType(SubscriptionType.Failover).subscriptionName(subName).subscribe();
			assertTrue(consumer_2 is MultiTopicsConsumerImpl);
			assertEquals(((MultiTopicsConsumerImpl) consumer_1).allTopicPartitionsNumber.get(), 4);

			for (int i = 0; i < messages; i++)
			{
				producer.newMessage().key(i.ToString()).value("message - " + i).send();
			}

			IDictionary<string, AtomicInteger> activeConsumers = new Dictionary<string, AtomicInteger>();
			PartitionedTopicStats stats = admin.topics().getPartitionedStats(topicName, true);
			foreach (TopicStats value in stats.partitions.Values)
			{
				foreach (SubscriptionStats subscriptionStats in value.subscriptions.Values)
				{
					assertTrue(subscriptionStats.activeConsumerName.Equals(consumer_1.ConsumerName) || subscriptionStats.activeConsumerName.Equals(consumer_2.ConsumerName));
					if (!activeConsumers.ContainsKey(subscriptionStats.activeConsumerName)) activeConsumers.Add(subscriptionStats.activeConsumerName, new AtomicInteger(0));
					activeConsumers[subscriptionStats.activeConsumerName].incrementAndGet();
				}
			}
			assertEquals(activeConsumers[consumer_1.ConsumerName].get(), 2);
			assertEquals(activeConsumers[consumer_2.ConsumerName].get(), 2);

			// 4.Check new consumer can receive half of total messages
			received = 0;
			lastMessage = null;
			for (int i = 0; i < messages / 2; i++)
			{
				lastMessage = consumer_1.receive();
				received++;
			}
			assertEquals(received, messages / 2);
			consumer_1.acknowledgeCumulative(lastMessage);

			received = 0;
			lastMessage = null;
			for (int i = 0; i < messages / 2; i++)
			{
				lastMessage = consumer_2.receive();
				received++;
			}
			assertEquals(received, messages / 2);
			consumer_2.acknowledgeCumulative(lastMessage);
		}

		public class MessageRouterAnonymousInnerClass : MessageRouter
		{
			private readonly TopicsConsumerImplTest outerInstance;

			public MessageRouterAnonymousInnerClass(TopicsConsumerImplTest outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public int choosePartition<T1>(Message<T1> msg, TopicMetadata metadata)
			{
				return int.Parse(msg.Key) % metadata.numPartitions();
			}
		}


		public virtual void testDefaultBacklogTTL()
		{

			int defaultTTLSec = 5;
			int totalMessages = 10;
			this.conf.TtlDurationDefaultInSeconds = defaultTTLSec;

			const string @namespace = "prop/use/expiry";

			string topicName = "persistent://" + @namespace + "/expiry";
			const string subName = "expiredSub";

			admin.clusters().createCluster("use", new ClusterData(brokerUrl.ToString()));

			admin.tenants().createTenant("prop", new TenantInfo(null, Sets.newHashSet("use")));
			admin.namespaces().createNamespace(@namespace);

			Consumer<sbyte[]> consumer = pulsarClient.newConsumer().topic(topicName).subscriptionName(subName).subscriptionType(SubscriptionType.Shared).ackTimeout(ackTimeOutMillis, TimeUnit.MILLISECONDS).subscribe();
			consumer.close();

			Producer<sbyte[]> producer = pulsarClient.newProducer().topic(topicName).enableBatching(false).create();
			for (int i = 0; i < totalMessages; i++)
			{
				producer.send(("" + i).GetBytes());
			}

			Optional<Topic> topic = pulsar.BrokerService.getTopic(topicName, false).get();
			assertTrue(topic.Present);
			PersistentSubscription subscription = (PersistentSubscription) topic.get().getSubscription(subName);

			Thread.Sleep((defaultTTLSec - 1) * 1000);
			topic.get().checkMessageExpiry();
			// Wait the message expire task done and make sure the message does not expire early.
			Thread.Sleep(1000);
			assertEquals(subscription.getNumberOfEntriesInBacklog(false), 10);
			Thread.Sleep(2000);
			topic.get().checkMessageExpiry();
			// Wait the message expire task done and make sure the message expired.
			retryStrategically((test) => subscription.getNumberOfEntriesInBacklog(false) == 0, 5, 200);
			assertEquals(subscription.getNumberOfEntriesInBacklog(false), 0);
		}


		public virtual void testGetLastMessageId()
		{
			string key = "TopicGetLastMessageId";

			string subscriptionName = "my-ex-subscription-" + key;

			string messagePredicate = "my-message-" + key + "-";
			const int totalMessages = 30;


			string topicName1 = "persistent://prop/use/ns-abc/topic-1-" + key;

			string topicName2 = "persistent://prop/use/ns-abc/topic-2-" + key;

			string topicName3 = "persistent://prop/use/ns-abc/topic-3-" + key;
			IList<string> topicNames = Lists.newArrayList(topicName1, topicName2, topicName3);

			TenantInfo tenantInfo = createDefaultTenantInfo();
			admin.tenants().createTenant("prop", tenantInfo);
			admin.topics().createPartitionedTopic(topicName2, 2);
			admin.topics().createPartitionedTopic(topicName3, 3);

			// 1. producer connect
			Producer<sbyte[]> producer1 = pulsarClient.newProducer().topic(topicName1).enableBatching(false).messageRoutingMode(MessageRoutingMode.SinglePartition).create();
			Producer<sbyte[]> producer2 = pulsarClient.newProducer().topic(topicName2).enableBatching(false).messageRoutingMode(MessageRoutingMode.RoundRobinPartition).create();
			Producer<sbyte[]> producer3 = pulsarClient.newProducer().topic(topicName3).enableBatching(false).messageRoutingMode(MessageRoutingMode.RoundRobinPartition).create();

			// 2. Create consumer
			Consumer<sbyte[]> consumer = pulsarClient.newConsumer().topics(topicNames).subscriptionName(subscriptionName).subscriptionType(SubscriptionType.Shared).ackTimeout(ackTimeOutMillis, TimeUnit.MILLISECONDS).receiverQueueSize(4).subscribe();
			assertTrue(consumer is MultiTopicsConsumerImpl);

			// 3. producer publish messages
			for (int i = 0; i < totalMessages; i++)
			{
				producer1.send((messagePredicate + "producer1-" + i).GetBytes());
				producer2.send((messagePredicate + "producer2-" + i).GetBytes());
				producer3.send((messagePredicate + "producer3-" + i).GetBytes());
			}

			MessageId messageId = consumer.LastMessageId;
			assertTrue(messageId is MultiMessageIdImpl);
			MultiMessageIdImpl multiMessageId = (MultiMessageIdImpl) messageId;
			IDictionary<string, MessageId> map = multiMessageId.Map;
			assertEquals(map.Count, 6);
			map.forEach((k, v) =>
			{
			log.info("topic: {}, messageId:{} ", k, v.ToString());
			assertTrue(v is MessageIdImpl);
			MessageIdImpl messageId1 = (MessageIdImpl) v;
			if (k.contains(topicName1))
			{
				assertEquals(messageId1.entryId, totalMessages - 1);
			}
			else if (k.contains(topicName2))
			{
				assertEquals(messageId1.entryId, totalMessages / 2 - 1);
			}
			else
			{
				assertEquals(messageId1.entryId, totalMessages / 3 - 1);
			}
			});

			for (int i = 0; i < totalMessages; i++)
			{
				producer1.send((messagePredicate + "producer1-" + i).GetBytes());
				producer2.send((messagePredicate + "producer2-" + i).GetBytes());
				producer3.send((messagePredicate + "producer3-" + i).GetBytes());
			}

			messageId = consumer.LastMessageId;
			assertTrue(messageId is MultiMessageIdImpl);
			MultiMessageIdImpl multiMessageId2 = (MultiMessageIdImpl) messageId;
			IDictionary<string, MessageId> map2 = multiMessageId2.Map;
			assertEquals(map2.Count, 6);
			map2.forEach((k, v) =>
			{
			log.info("topic: {}, messageId:{} ", k, v.ToString());
			assertTrue(v is MessageIdImpl);
			MessageIdImpl messageId1 = (MessageIdImpl) v;
			if (k.contains(topicName1))
			{
				assertEquals(messageId1.entryId, totalMessages * 2 - 1);
			}
			else if (k.contains(topicName2))
			{
				assertEquals(messageId1.entryId, totalMessages - 1);
			}
			else
			{
				assertEquals(messageId1.entryId, totalMessages * 2 / 3 - 1);
			}
			});

			consumer.unsubscribe();
			consumer.close();
			producer1.close();
			producer2.close();
			producer3.close();
		}


		public virtual void multiTopicsInDifferentNameSpace()
		{
			IList<string> topics = new List<string>();
			topics.Add("persistent://prop/use/ns-abc/topic-1");
			topics.Add("persistent://prop/use/ns-abc/topic-2");
			topics.Add("persistent://prop/use/ns-abc1/topic-3");
			admin.clusters().createCluster("use", new ClusterData(brokerUrl.ToString()));
			admin.tenants().createTenant("prop", new TenantInfo(null, Sets.newHashSet("use")));
			admin.namespaces().createNamespace("prop/use/ns-abc");
			admin.namespaces().createNamespace("prop/use/ns-abc1");
			Consumer consumer = pulsarClient.newConsumer().topics(topics).subscriptionName("multiTopicSubscription").subscriptionType(SubscriptionType.Exclusive).subscribe();
			// create Producer
			Producer<string> producer = pulsarClient.newProducer(Schema_Fields.STRING).topic("persistent://prop/use/ns-abc/topic-1").producerName("producer").create();
			Producer<string> producer1 = pulsarClient.newProducer(Schema_Fields.STRING).topic("persistent://prop/use/ns-abc/topic-2").producerName("producer1").create();
			Producer<string> producer2 = pulsarClient.newProducer(Schema_Fields.STRING).topic("persistent://prop/use/ns-abc1/topic-3").producerName("producer2").create();
			//send message
			producer.send("ns-abc/topic-1-Message1");

			producer1.send("ns-abc/topic-2-Message1");

			producer2.send("ns-abc1/topic-3-Message1");

			int messageSet = 0;
			Message<sbyte[]> message = consumer.receive();
			do
			{
				messageSet++;
				consumer.acknowledge(message);
				log.info("Consumer acknowledged : " + new string(message.Data));
				message = consumer.receive(200, TimeUnit.MILLISECONDS);
			} while (message != null);
			assertEquals(messageSet, 3);

			consumer.unsubscribe();
			consumer.Dispose();
			producer.close();
			producer1.close();
			producer2.close();
		}

	}

}