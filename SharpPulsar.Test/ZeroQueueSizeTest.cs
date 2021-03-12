using System.Collections.Generic;
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
	public class ZeroQueueSizeTest
	{
		private static readonly Logger _log = LoggerFactory.getLogger(typeof(ZeroQueueSizeTest));
		private readonly int _totalMessages = 10;

		public override void Setup()
		{
			BaseSetup();
		}


		protected internal override void Cleanup()
		{
			InternalCleanup();
		}


		public virtual void ValidQueueSizeConfig()
		{
			PulsarClient.NewConsumer().ReceiverQueueSize(0);
		}


		public virtual void InvalidQueueSizeConfig()
		{
			PulsarClient.NewConsumer().ReceiverQueueSize(-1);
		}

		public virtual void ZeroQueueSizeReceieveAsyncInCompatibility()
		{
			string key = "zeroQueueSizeReceieveAsyncInCompatibility";

			string topicName = "persistent://prop/use/ns-abc/topic-" + key;

			string subscriptionName = "my-ex-subscription-" + key;

			Consumer<sbyte[]> consumer = PulsarClient.NewConsumer().Topic(topicName).SubscriptionName(subscriptionName).ReceiverQueueSize(0).Subscribe();
			consumer.Receive(10, TimeUnit.SECONDS);
		}
		public virtual void ZeroQueueSizePartitionedTopicInCompatibility()
		{
			string key = "zeroQueueSizePartitionedTopicInCompatibility";

			string topicName = "persistent://prop/use/ns-abc/topic-" + key;

			string subscriptionName = "my-ex-subscription-" + key;
			int numberOfPartitions = 3;
			Admin.Topics().CreatePartitionedTopic(topicName, numberOfPartitions);
			PulsarClient.NewConsumer().Topic(topicName).SubscriptionName(subscriptionName).ReceiverQueueSize(0).Subscribe();
		}

		public virtual void ZeroQueueSizeNormalConsumer()
		{
			string key = "nonZeroQueueSizeNormalConsumer";

			// 1. Config

			string topicName = "persistent://prop/use/ns-abc/topic-" + key;

			string subscriptionName = "my-ex-subscription-" + key;

			string messagePredicate = "my-message-" + key + "-";

			// 2. Create Producer
			Producer<sbyte[]> producer = PulsarClient.NewProducer().Topic(topicName).EnableBatching(false).MessageRoutingMode(MessageRoutingMode.SinglePartition).Create();

			// 3. Create Consumer
			ConsumerImpl<sbyte[]> consumer = (ConsumerImpl<sbyte[]>) PulsarClient.NewConsumer().Topic(topicName).SubscriptionName(subscriptionName).ReceiverQueueSize(0).Subscribe();

			// 3. producer publish messages
			for(int i = 0; i < _totalMessages; i++)
			{
				string message = messagePredicate + i;
				_log.info("Producer produced: " + message);
				producer.send(message.GetBytes());
			}

			// 4. Receiver receives the message
			Message<sbyte[]> message;
			for(int i = 0; i < _totalMessages; i++)
			{
				assertEquals(consumer.NumMessagesInQueue(), 0);
				message = consumer.Receive();
				assertEquals(new string(message.Data), messagePredicate + i);
				assertEquals(consumer.NumMessagesInQueue(), 0);
				_log.info("Consumer received : " + new string(message.Data));
			}
		}


		public virtual void ZeroQueueSizeConsumerListener()
		{
			string key = "zeroQueueSizeConsumerListener";

			// 1. Config

			string topicName = "persistent://prop/use/ns-abc/topic-" + key;

			string subscriptionName = "my-ex-subscription-" + key;

			string messagePredicate = "my-message-" + key + "-";

			// 2. Create Producer
			Producer<sbyte[]> producer = PulsarClient.NewProducer().Topic(topicName).EnableBatching(false).MessageRoutingMode(MessageRoutingMode.SinglePartition).Create();

			// 3. Create Consumer
			IList<Message<sbyte[]>> messages = Lists.newArrayList();
			System.Threading.CountdownEvent latch = new System.Threading.CountdownEvent(_totalMessages);
			ConsumerImpl<sbyte[]> consumer = (ConsumerImpl<sbyte[]>) PulsarClient.NewConsumer().Topic(topicName).SubscriptionName(subscriptionName).ReceiverQueueSize(0).MessageListener((cons, msg) =>
			{
			assertEquals(((ConsumerImpl) cons).numMessagesInQueue(), 0);
			lock(messages)
			{
				messages.Add(msg);
			}
			_log.info("Consumer received: " + new string(msg.Data));
			latch.Signal();
			}).Subscribe();

			// 3. producer publish messages
			for(int i = 0; i < _totalMessages; i++)
			{
				string message = messagePredicate + i;
				_log.info("Producer produced: " + message);
				producer.send(message.GetBytes());
			}

			// 4. Receiver receives the message
			latch.await();
			assertEquals(consumer.NumMessagesInQueue(), 0);
			assertEquals(messages.Count, _totalMessages);
			for(int i = 0; i < messages.Count; i++)
			{
				assertEquals(new string(messages[i].Data), messagePredicate + i);
			}
		}


		public virtual void ZeroQueueSizeSharedSubscription()
		{
			string key = "zeroQueueSizeSharedSubscription";

			// 1. Config

			string topicName = "persistent://prop/use/ns-abc/topic-" + key;

			string subscriptionName = "my-ex-subscription-" + key

			string messagePredicate = "my-message-" + key + "-";

			// 2. Create Producer
			Producer<sbyte[]> producer = PulsarClient.NewProducer().Topic(topicName).EnableBatching(false).MessageRoutingMode(MessageRoutingMode.SinglePartition).Create();

			// 3. Create Consumer
			int numOfSubscribers = 4;

			ConsumerImpl<object>[] consumers = new ConsumerImpl[numOfSubscribers];
			for(int i = 0; i < numOfSubscribers; i++)
			{
				consumers[i] = (ConsumerImpl<sbyte[]>) PulsarClient.NewConsumer().Topic(topicName).SubscriptionName(subscriptionName).ReceiverQueueSize(0).SubscriptionType(SubscriptionType.Shared).Subscribe();
			}

			// 4. Produce Messages
			for(int i = 0; i < _totalMessages; i++)
			{
				string message = messagePredicate + i;
				producer.send(message.GetBytes());
			}

			// 5. Consume messages

			Message<object> message;
			for(int i = 0; i < _totalMessages; i++)
			{
				assertEquals(consumers[i % numOfSubscribers].NumMessagesInQueue(), 0);
				message = consumers[i % numOfSubscribers].Receive();
				assertEquals(new string(message.Data), messagePredicate + i);
				assertEquals(consumers[i % numOfSubscribers].NumMessagesInQueue(), 0);
				_log.info("Consumer received : " + new string(message.Data));
			}
		}

		public virtual void ZeroQueueSizeFailoverSubscription()
		{
			string key = "zeroQueueSizeFailoverSubscription";

			// 1. Config

			string topicName = "persistent://prop/use/ns-abc/topic-" + key;

			string subscriptionName = "my-ex-subscription-" + key;

			string messagePredicate = "my-message-" + key + "-";

			// 2. Create Producer
			Producer<sbyte[]> producer = PulsarClient.NewProducer().Topic(topicName).EnableBatching(false).MessageRoutingMode(MessageRoutingMode.SinglePartition).Create();

			// 3. Create Consumer
			ConsumerImpl<sbyte[]> consumer1 = (ConsumerImpl<sbyte[]>) PulsarClient.NewConsumer().Topic(topicName).SubscriptionName(subscriptionName).ReceiverQueueSize(0).SubscriptionType(SubscriptionType.Failover).ConsumerName("consumer-1").Subscribe();
			ConsumerImpl<sbyte[]> consumer2 = (ConsumerImpl<sbyte[]>) PulsarClient.NewConsumer().Topic(topicName).SubscriptionName(subscriptionName).ReceiverQueueSize(0).SubscriptionType(SubscriptionType.Failover).ConsumerName("consumer-2").Subscribe();

			// 4. Produce Messages
			for(int i = 0; i < _totalMessages; i++)
			{
				string message = messagePredicate + i;
				producer.send(message.GetBytes());
			}

			// 5. Consume messages
			Message<sbyte[]> message;
			for(int i = 0; i < _totalMessages / 2; i++)
			{
				assertEquals(consumer1.NumMessagesInQueue(), 0);
				message = consumer1.Receive();
				assertEquals(new string(message.Data), messagePredicate + i);
				assertEquals(consumer1.NumMessagesInQueue(), 0);
				_log.info("Consumer received : " + new string(message.Data));
			}

			// 6. Trigger redelivery
			consumer1.RedeliverUnacknowledgedMessages();

			// 7. Trigger Failover
			consumer1.close();

			// 8. Receive messages on failed over consumer
			for(int i = 0; i < _totalMessages / 2; i++)
			{
				assertEquals(consumer2.NumMessagesInQueue(), 0);
				message = consumer2.Receive();
				assertEquals(new string(message.Data), messagePredicate + i);
				assertEquals(consumer2.NumMessagesInQueue(), 0);
				_log.info("Consumer received : " + new string(message.Data));
			}
		}

		public virtual void TestFailedZeroQueueSizeBatchMessage()
		{

			int batchMessageDelayMs = 100;
			Consumer<sbyte[]> consumer = PulsarClient.NewConsumer().Topic("persistent://prop-xyz/use/ns-abc/topic1").SubscriptionName("my-subscriber-name").SubscriptionType(SubscriptionType.Shared).ReceiverQueueSize(0).Subscribe();

			ProducerBuilder<sbyte[]> producerBuilder = PulsarClient.NewProducer().Topic("persistent://prop-xyz/use/ns-abc/topic1").MessageRoutingMode(MessageRoutingMode.SinglePartition);

			if(batchMessageDelayMs != 0)
			{
				producerBuilder.EnableBatching(true).BatchingMaxPublishDelay(batchMessageDelayMs, TimeUnit.MILLISECONDS).BatchingMaxMessages(5);
			}
			else
			{
				producerBuilder.EnableBatching(false);
			}

			Producer<sbyte[]> producer = producerBuilder.Create();
			for(int i = 0; i < 10; i++)
			{
				string message = "my-message-" + i;
				producer.send(message.GetBytes());
			}

			try
			{
				consumer.ReceiveAsync().handle((ok, e) =>
				{
				if(e == null)
				{
					Assert.fail();
				}
				return null;
				});
			}
			finally
			{
				consumer.close();
			}
		}

		public virtual void TestZeroQueueSizeMessageRedelivery()
		{
			const string topic = "persistent://prop/ns-abc/testZeroQueueSizeMessageRedelivery";
			Consumer<int> consumer = PulsarClient.NewConsumer(Schema.INT32).Topic(topic).ReceiverQueueSize(0).SubscriptionName("sub").SubscriptionType(SubscriptionType.Shared).AckTimeout(1, TimeUnit.SECONDS).Subscribe();

			const int messages = 10;
			Producer<int> producer = PulsarClient.NewProducer(Schema.INT32).Topic(topic).EnableBatching(false).Create();

			for(int i = 0; i < messages; i++)
			{
				producer.send(i);
			}

			ISet<int> receivedMessages = new HashSet<int>();
			for(int i = 0; i < messages * 2; i++)
			{
				receivedMessages.Add(consumer.Receive().Value);
			}

			Assert.assertEquals(receivedMessages.Count, messages);

			consumer.close();
			producer.close();
		}

		public virtual void TestZeroQueueSizeMessageRedeliveryForListener()
		{
			const string topic = "persistent://prop/ns-abc/testZeroQueueSizeMessageRedeliveryForListener";
			const int messages = 10;
			System.Threading.CountdownEvent latch = new System.Threading.CountdownEvent(messages * 2);
			ISet<int> receivedMessages = new HashSet<int>();
			Consumer<int> consumer = PulsarClient.NewConsumer(Schema.INT32).Topic(topic).ReceiverQueueSize(0).SubscriptionName("sub").SubscriptionType(SubscriptionType.Shared).AckTimeout(1, TimeUnit.SECONDS).MessageListener((MessageListener<int>)(c, msg) =>
			{
			try
			{
				receivedMessages.Add(msg.Value);
			}
			finally
			{
				latch.Signal();
			}
			}).Subscribe();

			Producer<int> producer = PulsarClient.NewProducer(Schema.INT32).Topic(topic).EnableBatching(false).Create();

			for(int i = 0; i < messages; i++)
			{
				producer.send(i);
			}

			latch.await();
			Assert.assertEquals(receivedMessages.Count, messages);

			consumer.close();
			producer.close();
		}

		public virtual void TestZeroQueueSizeMessageRedeliveryForAsyncReceive()
		{
			const string topic = "persistent://prop/ns-abc/testZeroQueueSizeMessageRedeliveryForAsyncReceive";
			Consumer<int> consumer = PulsarClient.NewConsumer(Schema.INT32).Topic(topic).ReceiverQueueSize(0).SubscriptionName("sub").SubscriptionType(SubscriptionType.Shared).AckTimeout(1, TimeUnit.SECONDS).Subscribe();

			const int messages = 10;
			Producer<int> producer = PulsarClient.NewProducer(Schema.INT32).Topic(topic).EnableBatching(false).Create();

			for(int i = 0; i < messages; i++)
			{
				producer.send(i);
			}

			ISet<int> receivedMessages = new HashSet<int>();
			IList<CompletableFuture<Message<int>>> futures = new List<CompletableFuture<Message<int>>>(20);
			for(int i = 0; i < messages * 2; i++)
			{
				futures.Add(consumer.ReceiveAsync());
			}
			foreach(CompletableFuture<Message<int>> future in futures)
			{
				receivedMessages.Add(future.get().Value);
			}

			Assert.assertEquals(receivedMessages.Count, messages);

			consumer.close();
			producer.close();
		}

		public virtual void TestPauseAndResume()
		{
			const string topicName = "persistent://prop/ns-abc/zero-queue-pause-and-resume";
			const string subName = "sub";

			AtomicReference<System.Threading.CountdownEvent> latch = new AtomicReference<System.Threading.CountdownEvent>(new System.Threading.CountdownEvent(1));
			AtomicInteger received = new AtomicInteger();

			Consumer<sbyte[]> consumer = PulsarClient.NewConsumer().Topic(topicName).SubscriptionName(subName).ReceiverQueueSize(0).MessageListener((c1, msg) =>
			{
			assertNotNull(msg, "Message cannot be null");
			c1.acknowledgeAsync(msg);
			received.incrementAndGet();
			latch.get().countDown();
			}).Subscribe();
			consumer.Pause();

			Producer<sbyte[]> producer = PulsarClient.NewProducer().Topic(topicName).EnableBatching(false).Create();

			for(int i = 0; i < 2; i++)
			{
				producer.send(("my-message-" + i).GetBytes());
			}

			// Paused consumer receives only one message
			assertTrue(latch.get().await(2, TimeUnit.SECONDS), "Timed out waiting for message listener acks");
			Thread.Sleep(2000);
			assertEquals(received.intValue(), 1, "Consumer received messages while paused");

			latch.set(new System.Threading.CountdownEvent(1));
			consumer.Resume();
			assertTrue(latch.get().await(2, TimeUnit.SECONDS), "Timed out waiting for message listener acks");

			consumer.Unsubscribe();
			producer.close();
		}

		public virtual void TestPauseAndResumeWithUnloading()
		{
			const string topicName = "persistent://prop/ns-abc/zero-queue-pause-and-resume-with-unloading";
			const string subName = "sub";

			AtomicReference<System.Threading.CountdownEvent> latch = new AtomicReference<System.Threading.CountdownEvent>(new System.Threading.CountdownEvent(1));
			AtomicInteger received = new AtomicInteger();

			Consumer<sbyte[]> consumer = PulsarClient.NewConsumer().Topic(topicName).SubscriptionName(subName).ReceiverQueueSize(0).MessageListener((c1, msg) =>
			{
			assertNotNull(msg, "Message cannot be null");
			c1.acknowledgeAsync(msg);
			received.incrementAndGet();
			latch.get().countDown();
			}).Subscribe();
			consumer.Pause();

			Producer<sbyte[]> producer = PulsarClient.NewProducer().Topic(topicName).EnableBatching(false).Create();

			for(int i = 0; i < 2; i++)
			{
				producer.send(("my-message-" + i).GetBytes());
			}

			// Paused consumer receives only one message
			assertTrue(latch.get().await(2, TimeUnit.SECONDS), "Timed out waiting for message listener acks");

			// Make sure no flow permits are sent when the consumer reconnects to the topic
			Admin.Topics().Unload(topicName);
			Thread.Sleep(2000);
			assertEquals(received.intValue(), 1, "Consumer received messages while paused");

			latch.set(new System.Threading.CountdownEvent(1));
			consumer.Resume();
			assertTrue(latch.get().await(2, TimeUnit.SECONDS), "Timed out waiting for message listener acks");

			consumer.Unsubscribe();
			producer.close();
		}
	}

}