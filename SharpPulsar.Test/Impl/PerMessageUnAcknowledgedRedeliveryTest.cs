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
namespace SharpPulsar.Test.Impl
{


    public class PerMessageUnAcknowledgedRedeliveryTest : BrokerTestBase
	{
		private const long testTimeout = 90000; // 1.5 min
		private static readonly Logger log = LoggerFactory.getLogger(typeof(PerMessageUnAcknowledgedRedeliveryTest));
		private readonly long ackTimeOutMillis = TimeUnit.SECONDS.toMillis(2);


		public override void setup()
		{
			base.internalSetup();
		}


		public override void cleanup()
		{
			base.internalCleanup();
		}


		public virtual void testSharedAckedNormalTopic()
		{
			string key = "testSharedAckedNormalTopic";

			string topicName = "persistent://prop/use/ns-abc/topic-" + key;

			string subscriptionName = "my-ex-subscription-" + key;

			string messagePredicate = "my-message-" + key + "-";
			const int totalMessages = 15;

			// 1. producer connect
			Producer<sbyte[]> producer = pulsarClient.newProducer().topic(topicName).enableBatching(false).messageRoutingMode(MessageRoutingMode.SinglePartition).create();

			// 2. Create consumer
			Consumer<sbyte[]> consumer = pulsarClient.newConsumer().topic(topicName).subscriptionName(subscriptionName).receiverQueueSize(50).ackTimeout(ackTimeOutMillis, TimeUnit.MILLISECONDS).subscriptionType(SubscriptionType.Shared).subscribe();

			// 3. producer publish messages
			for (int i = 0; i < totalMessages / 3; i++)
			{
				string message = messagePredicate + i;
				log.info("Producer produced: " + message);
				producer.send(message.GetBytes());
			}

			// 4. Receiver receives the message, doesn't ack
			Message<sbyte[]> message = consumer.receive();
			while (message != null)
			{
				string data = new string(message.Data);
				log.info("Consumer received : " + data);
				message = consumer.receive(100, TimeUnit.MILLISECONDS);
			}
			long size = ((ConsumerImpl<sbyte[]>) consumer).UnAckedMessageTracker.size();
			log.info(key + " Unacked Message Tracker size is " + size);
			assertEquals(size, 5);

			// 5. producer publish more messages
			for (int i = 0; i < totalMessages / 3; i++)
			{
				string m = messagePredicate + i;
				log.info("Producer produced: " + m);
				producer.send(m.GetBytes());
			}

			// 6. Receiver receives the message, ack them
			message = consumer.receive();
			int received = 0;
			while (message != null)
			{
				received++;
				string data = new string(message.Data);
				log.info("Consumer received : " + data);
				consumer.acknowledge(message);
				message = consumer.receive(100, TimeUnit.MILLISECONDS);
			}
			size = ((ConsumerImpl<sbyte[]>) consumer).UnAckedMessageTracker.size();
			log.info(key + " Unacked Message Tracker size is " + size);
			assertEquals(size, 5);
			assertEquals(received, 5);

			// 7. Simulate ackTimeout
			Thread.Sleep(ackTimeOutMillis);

			// 8. producer publish more messages
			for (int i = 0; i < totalMessages / 3; i++)
			{
				string m = messagePredicate + i;
				log.info("Producer produced: " + m);
				producer.send(m.GetBytes());
			}

			// 9. Receiver receives the message, doesn't ack
			message = consumer.receive();
			while (message != null)
			{
				string data = new string(message.Data);
				log.info("Consumer received : " + data);
				message = consumer.receive(100, TimeUnit.MILLISECONDS);
			}
			size = ((ConsumerImpl<sbyte[]>) consumer).UnAckedMessageTracker.size();
			log.info(key + " Unacked Message Tracker size is " + size);
			assertEquals(size, 10);

			Thread.Sleep(ackTimeOutMillis);

			// 10. Receiver receives redelivered messages
			message = consumer.receive();
			int redelivered = 0;
			while (message != null)
			{
				redelivered++;
				string data = new string(message.Data);
				log.info("Consumer received : " + data);
				consumer.acknowledge(message);
				message = consumer.receive(100, TimeUnit.MILLISECONDS);
			}
			assertEquals(redelivered, 5);
			size = ((ConsumerImpl<sbyte[]>) consumer).UnAckedMessageTracker.size();
			log.info(key + " Unacked Message Tracker size is " + size);
			assertEquals(size, 5);
		}


		public virtual void testUnAckedMessageTrackerSize()
		{
			string key = "testUnAckedMessageTrackerSize";

			string topicName = "persistent://prop/use/ns-abc/topic-" + key;

			string subscriptionName = "my-ex-subscription-" + key;

			string messagePredicate = "my-message-" + key + "-";
			const int totalMessages = 15;

			// 1. producer connect
			Producer<sbyte[]> producer = pulsarClient.newProducer().topic(topicName).enableBatching(false).messageRoutingMode(MessageRoutingMode.SinglePartition).create();

			// 2. Create consumer,doesn't set the ackTimeout
			Consumer<sbyte[]> consumer = pulsarClient.newConsumer().topic(topicName).subscriptionName(subscriptionName).receiverQueueSize(50).subscriptionType(SubscriptionType.Shared).subscribe();

			// 3. producer publish messages
			for (int i = 0; i < totalMessages / 3; i++)
			{
				string message = messagePredicate + i;
				log.info("Producer produced: " + message);
				producer.send(message.GetBytes());
			}

			// 4. Receiver receives the message, doesn't ack
			Message<sbyte[]> message = consumer.receive();
			while (message != null)
			{
				string data = new string(message.Data);
				log.info("Consumer received : " + data);
				message = consumer.receive(100, TimeUnit.MILLISECONDS);
			}
			UnAckedMessageTracker unAckedMessageTracker = ((ConsumerImpl<sbyte[]>) consumer).UnAckedMessageTracker;
			long size = unAckedMessageTracker.size();
			log.info(key + " Unacked Message Tracker size is " + size);
			// 5. If ackTimeout is not set, UnAckedMessageTracker is a disabled method
			assertEquals(size, 0);
			assertTrue(unAckedMessageTracker.add(null));
			assertTrue(unAckedMessageTracker.remove(null));
			assertEquals(unAckedMessageTracker.removeMessagesTill(null), 0);
		}


		public virtual void testExclusiveAckedNormalTopic()
		{
			string key = "testExclusiveAckedNormalTopic";

			string topicName = "persistent://prop/use/ns-abc/topic-" + key;

			string subscriptionName = "my-ex-subscription-" + key;

			string messagePredicate = "my-message-" + key + "-";
			const int totalMessages = 15;

			// 1. producer connect
			Producer<sbyte[]> producer = pulsarClient.newProducer().topic(topicName).enableBatching(false).messageRoutingMode(MessageRoutingMode.SinglePartition).create();

			// 2. Create consumer
			Consumer<sbyte[]> consumer = pulsarClient.newConsumer().topic(topicName).subscriptionName(subscriptionName).receiverQueueSize(50).ackTimeout(ackTimeOutMillis, TimeUnit.MILLISECONDS).subscriptionType(SubscriptionType.Exclusive).subscribe();

			// 3. producer publish messages
			for (int i = 0; i < totalMessages / 3; i++)
			{
				string message = messagePredicate + i;
				log.info("Producer produced: " + message);
				producer.send(message.GetBytes());
			}

			// 4. Receiver receives the message, doesn't ack
			Message<sbyte[]> message = consumer.receive();
			while (message != null)
			{
				string data = new string(message.Data);
				log.info("Consumer received : " + data);
				message = consumer.receive(100, TimeUnit.MILLISECONDS);
			}
			long size = ((ConsumerImpl<sbyte[]>) consumer).UnAckedMessageTracker.size();
			log.info(key + " Unacked Message Tracker size is " + size);
			assertEquals(size, 5);

			// 5. producer publish more messages
			for (int i = 0; i < totalMessages / 3; i++)
			{
				string m = messagePredicate + i;
				log.info("Producer produced: " + m);
				producer.send(m.GetBytes());
			}

			// 6. Receiver receives the message, ack them
			message = consumer.receive();
			int received = 0;
			while (message != null)
			{
				received++;
				string data = new string(message.Data);
				log.info("Consumer received : " + data);
				consumer.acknowledge(message);
				message = consumer.receive(100, TimeUnit.MILLISECONDS);
			}
			size = ((ConsumerImpl<sbyte[]>) consumer).UnAckedMessageTracker.size();
			log.info(key + " Unacked Message Tracker size is " + size);
			assertEquals(size, 5);
			assertEquals(received, 5);

			// 7. Simulate ackTimeout
			Thread.Sleep(ackTimeOutMillis);

			// 8. producer publish more messages
			for (int i = 0; i < totalMessages / 3; i++)
			{
				string m = messagePredicate + i;
				log.info("Producer produced: " + m);
				producer.send(m.GetBytes());
			}

			// 9. Receiver receives the message, doesn't ack
			message = consumer.receive();
			while (message != null)
			{
				string data = new string(message.Data);
				log.info("Consumer received : " + data);
				message = consumer.receive(100, TimeUnit.MILLISECONDS);
			}
			size = ((ConsumerImpl<sbyte[]>) consumer).UnAckedMessageTracker.size();
			log.info(key + " Unacked Message Tracker size is " + size);
			assertEquals(size, 10);

			Thread.Sleep(ackTimeOutMillis);

			// 10. Receiver receives redelivered messages
			message = consumer.receive();
			int redelivered = 0;
			while (message != null)
			{
				redelivered++;
				string data = new string(message.Data);
				log.info("Consumer received : " + data);
				consumer.acknowledge(message);
				message = consumer.receive(100, TimeUnit.MILLISECONDS);
			}
			assertEquals(redelivered, 10);
			size = ((ConsumerImpl<sbyte[]>) consumer).UnAckedMessageTracker.size();
			log.info(key + " Unacked Message Tracker size is " + size);
			assertEquals(size, 0);
		}


		public virtual void testFailoverAckedNormalTopic()
		{
			string key = "testFailoverAckedNormalTopic";

			string topicName = "persistent://prop/use/ns-abc/topic-" + key;

			string subscriptionName = "my-ex-subscription-" + key;

			string messagePredicate = "my-message-" + key + "-";
			const int totalMessages = 15;

			// 1. producer connect
			Producer<sbyte[]> producer = pulsarClient.newProducer().topic(topicName).enableBatching(false).messageRoutingMode(MessageRoutingMode.SinglePartition).create();

			// 2. Create consumer
			Consumer<sbyte[]> consumer = pulsarClient.newConsumer().topic(topicName).subscriptionName(subscriptionName).receiverQueueSize(50).ackTimeout(ackTimeOutMillis, TimeUnit.MILLISECONDS).subscriptionType(SubscriptionType.Failover).subscribe();

			// 3. producer publish messages
			for (int i = 0; i < totalMessages / 3; i++)
			{
				string message = messagePredicate + i;
				log.info("Producer produced: " + message);
				producer.send(message.GetBytes());
			}

			// 4. Receiver receives the message, doesn't ack
			Message<sbyte[]> message = consumer.receive();
			while (message != null)
			{
				string data = new string(message.Data);
				log.info("Consumer received : " + data);
				message = consumer.receive(100, TimeUnit.MILLISECONDS);
			}
			long size = ((ConsumerImpl<sbyte[]>) consumer).UnAckedMessageTracker.size();
			log.info(key + " Unacked Message Tracker size is " + size);
			assertEquals(size, 5);

			// 5. producer publish more messages
			for (int i = 0; i < totalMessages / 3; i++)
			{
				string m = messagePredicate + i;
				log.info("Producer produced: " + m);
				producer.send(m.GetBytes());
			}

			// 6. Receiver receives the message, ack them
			message = consumer.receive();
			int received = 0;
			while (message != null)
			{
				received++;
				string data = new string(message.Data);
				log.info("Consumer received : " + data);
				consumer.acknowledge(message);
				message = consumer.receive(100, TimeUnit.MILLISECONDS);
			}
			size = ((ConsumerImpl<sbyte[]>) consumer).UnAckedMessageTracker.size();
			log.info(key + " Unacked Message Tracker size is " + size);
			assertEquals(size, 5);
			assertEquals(received, 5);

			// 7. Simulate ackTimeout
			Thread.Sleep(ackTimeOutMillis);

			// 8. producer publish more messages
			for (int i = 0; i < totalMessages / 3; i++)
			{
				string m = messagePredicate + i;
				log.info("Producer produced: " + m);
				producer.send(m.GetBytes());
			}

			// 9. Receiver receives the message, doesn't ack
			message = consumer.receive();
			while (message != null)
			{
				string data = new string(message.Data);
				log.info("Consumer received : " + data);
				message = consumer.receive(100, TimeUnit.MILLISECONDS);
			}
			size = ((ConsumerImpl<sbyte[]>) consumer).UnAckedMessageTracker.size();
			log.info(key + " Unacked Message Tracker size is " + size);
			assertEquals(size, 10);

			Thread.Sleep(ackTimeOutMillis);

			// 10. Receiver receives redelivered messages
			message = consumer.receive();
			int redelivered = 0;
			while (message != null)
			{
				redelivered++;
				string data = new string(message.Data);
				log.info("Consumer received : " + data);
				consumer.acknowledge(message);
				message = consumer.receive(100, TimeUnit.MILLISECONDS);
			}
			assertEquals(redelivered, 10);
			size = ((ConsumerImpl<sbyte[]>) consumer).UnAckedMessageTracker.size();
			log.info(key + " Unacked Message Tracker size is " + size);
			assertEquals(size, 0);
		}

		private static long getUnackedMessagesCountInPartitionedConsumer(Consumer<sbyte[]> c)
		{
			MultiTopicsConsumerImpl<sbyte[]> pc = (MultiTopicsConsumerImpl<sbyte[]>) c;
			return pc.UnAckedMessageTracker.size() + pc.Consumers.Select(consumer => consumer.UnAckedMessageTracker.size()).Sum();
		}


		public virtual void testSharedAckedPartitionedTopic()
		{
			string key = "testSharedAckedPartitionedTopic";

			string topicName = "persistent://prop/use/ns-abc/topic-" + key;

			string subscriptionName = "my-ex-subscription-" + key;

			string messagePredicate = "my-message-" + key + "-";
			const int totalMessages = 15;
			const int numberOfPartitions = 3;
			TenantInfo tenantInfo = createDefaultTenantInfo();
			admin.tenants().createTenant("prop", tenantInfo);
			admin.topics().createPartitionedTopic(topicName, numberOfPartitions);

			// 1. producer connect
			Producer<sbyte[]> producer = pulsarClient.newProducer().topic(topicName).enableBatching(false).messageRoutingMode(MessageRoutingMode.RoundRobinPartition).create();

			// 2. Create consumer
			Consumer<sbyte[]> consumer = pulsarClient.newConsumer().topic(topicName).subscriptionName(subscriptionName).receiverQueueSize(50).ackTimeout(ackTimeOutMillis, TimeUnit.MILLISECONDS).subscriptionType(SubscriptionType.Shared).subscribe();

			// 3. producer publish messages
			for (int i = 0; i < totalMessages / 3; i++)
			{
				string message = messagePredicate + i;
				log.info("Producer produced: " + message);
				producer.send(message.GetBytes());
			}

			// 4. Receiver receives the message, doesn't ack
			Message<sbyte[]> message = consumer.receive();
			while (message != null)
			{
				string data = new string(message.Data);
				log.info("Consumer received : " + data);
				message = consumer.receive(100, TimeUnit.MILLISECONDS);
			}

			long size = getUnackedMessagesCountInPartitionedConsumer(consumer);
			log.info(key + " Unacked Message Tracker size is " + size);
			assertEquals(size, 5);

			// 5. producer publish more messages
			for (int i = 0; i < totalMessages / 3; i++)
			{
				string m = messagePredicate + i;
				log.info("Producer produced: " + m);
				producer.send(m.GetBytes());
			}

			// 6. Receiver receives the message, ack them
			message = consumer.receive();
			int received = 0;
			while (message != null)
			{
				received++;
				string data = new string(message.Data);
				log.info("Consumer received : " + data);
				consumer.acknowledge(message);
				message = consumer.receive(100, TimeUnit.MILLISECONDS);
			}
			size = getUnackedMessagesCountInPartitionedConsumer(consumer);
			log.info(key + " Unacked Message Tracker size is " + size);
			assertEquals(size, 5);
			assertEquals(received, 5);

			// 7. Simulate ackTimeout
			Thread.Sleep(ackTimeOutMillis);

			// 8. producer publish more messages
			for (int i = 0; i < totalMessages / 3; i++)
			{
				string m = messagePredicate + i;
				log.info("Producer produced: " + m);
				producer.send(m.GetBytes());
			}

			// 9. Receiver receives the message, doesn't ack
			message = consumer.receive();
			while (message != null)
			{
				string data = new string(message.Data);
				log.info("Consumer received : " + data);
				message = consumer.receive(100, TimeUnit.MILLISECONDS);
			}
			size = getUnackedMessagesCountInPartitionedConsumer(consumer);
			log.info(key + " Unacked Message Tracker size is " + size);
			assertEquals(size, 10);

			Thread.Sleep(ackTimeOutMillis);

			// 10. Receiver receives redelivered messages
			message = consumer.receive();
			int redelivered = 0;
			while (message != null)
			{
				redelivered++;
				string data = new string(message.Data);
				log.info("Consumer received : " + data);
				consumer.acknowledge(message);
				message = consumer.receive(100, TimeUnit.MILLISECONDS);
			}
			assertEquals(redelivered, 5);
			size = getUnackedMessagesCountInPartitionedConsumer(consumer);
			log.info(key + " Unacked Message Tracker size is " + size);
			assertEquals(size, 5);
		}
	}

}