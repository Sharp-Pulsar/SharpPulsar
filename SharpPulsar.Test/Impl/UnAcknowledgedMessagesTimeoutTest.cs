using System;
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
namespace SharpPulsar.Test.Impl
{
	

    public class UnAcknowledgedMessagesTimeoutTest 
	{
		private static readonly Logger log = LoggerFactory.getLogger(typeof(UnAcknowledgedMessagesTimeoutTest));
		private readonly long ackTimeOutMillis = TimeUnit.SECONDS.toMillis(2);


		public override void setup()
		{
			base.baseSetup();
		}


		public override void cleanup()
		{
			base.internalCleanup();
		}


		public virtual void testExclusiveSingleAckedNormalTopic()
		{
			string key = "testExclusiveSingleAckedNormalTopic";

			string topicName = "persistent://prop/ns-abc/topic-" + key;

			string subscriptionName = "my-ex-subscription-" + key;

			string messagePredicate = "my-message-" + key + "-";
			const int totalMessages = 10;

			// 1. producer connect
			Producer<sbyte[]> producer = pulsarClient.newProducer().topic(topicName).enableBatching(false).messageRoutingMode(MessageRoutingMode.SinglePartition).create();

			// 2. Create consumer
			Consumer<sbyte[]> consumer = pulsarClient.newConsumer().topic(topicName).subscriptionName(subscriptionName).receiverQueueSize(7).ackTimeout(ackTimeOutMillis, TimeUnit.MILLISECONDS).subscribe();

			// 3. producer publish messages
			for (int i = 0; i < totalMessages / 2; i++)
			{
				string message = messagePredicate + i;
				log.info("Producer produced: " + message);
				producer.send(message.GetBytes());
			}

			// 4. Receiver receives the message
			Message<sbyte[]> message = consumer.receive();
			while (message != null)
			{
				log.info("Consumer received : " + new string(message.Data));
				message = consumer.receive(500, TimeUnit.MILLISECONDS);
			}

			long size = ((ConsumerImpl<object>) consumer).UnAckedMessageTracker.size();
			log.info(key + " Unacked Message Tracker size is " + size);
			assertEquals(size, totalMessages / 2);

			// Blocking call, redeliver should kick in
			message = consumer.receive();
			log.info("Consumer received : " + new string(message.Data));

			HashSet<string> hSet = new HashSet<string>();
			for (int i = totalMessages / 2; i < totalMessages; i++)
			{
				string messageString = messagePredicate + i;
				producer.send(messageString.GetBytes());
			}

			do
			{
				hSet.Add(new string(message.Data));
				consumer.acknowledge(message);
				log.info("Consumer acknowledged : " + new string(message.Data));
				message = consumer.receive(500, TimeUnit.MILLISECONDS);
			} while (message != null);


			size = ((ConsumerImpl<object>) consumer).UnAckedMessageTracker.size();
			log.info(key + " Unacked Message Tracker size is " + size);
			assertEquals(size, 0);
			assertEquals(hSet.Count, totalMessages);
		}


		public virtual void testExclusiveCumulativeAckedNormalTopic()
		{
			string key = "testExclusiveCumulativeAckedNormalTopic";

			string topicName = "persistent://prop/use/ns-abc/topic-" + key;

			string subscriptionName = "my-ex-subscription-" + key;

			string messagePredicate = "my-message-" + key + "-";
			const int totalMessages = 10;

			// 1. producer connect
			Producer<sbyte[]> producer = pulsarClient.newProducer().topic(topicName).enableBatching(false).messageRoutingMode(MessageRoutingMode.SinglePartition).create();

			// 2. Create consumer
			Consumer<sbyte[]> consumer = pulsarClient.newConsumer().topic(topicName).subscriptionName(subscriptionName).receiverQueueSize(7).ackTimeout(ackTimeOutMillis, TimeUnit.MILLISECONDS).subscribe();

			// 3. producer publish messages
			for (int i = 0; i < totalMessages; i++)
			{
				string message = messagePredicate + i;
				producer.send(message.GetBytes());
			}

			// 4. Receiver receives the message
			HashSet<string> hSet = new HashSet<string>();
			Message<sbyte[]> message = consumer.receive();
			Message<sbyte[]> lastMessage = message;
			while (message != null)
			{
				lastMessage = message;
				hSet.Add(new string(message.Data));
				log.info("Consumer received " + new string(message.Data));
				log.info("Message ID details " + message.MessageId.ToString());
				message = consumer.receive(500, TimeUnit.MILLISECONDS);
			}

			long size = ((ConsumerImpl<object>) consumer).UnAckedMessageTracker.size();
			assertEquals(size, totalMessages);
			log.info("Comulative Ack sent for " + new string(lastMessage.Data));
			log.info("Message ID details " + lastMessage.MessageId.ToString());
			consumer.acknowledgeCumulative(lastMessage);

			size = ((ConsumerImpl<object>) consumer).UnAckedMessageTracker.size();
			assertEquals(size, 0);
			message = consumer.receive((int)(2 * ackTimeOutMillis), TimeUnit.MILLISECONDS);
			assertNull(message);
		}


		public virtual void testSharedSingleAckedPartitionedTopic()
		{
			string key = "testSharedSingleAckedPartitionedTopic";

			string topicName = "persistent://prop/ns-abc/topic-" + key;

			string subscriptionName = "my-shared-subscription-" + key;

			string messagePredicate = "my-message-" + key + "-";
			const int totalMessages = 20;
			const int numberOfPartitions = 3;
			admin.topics().createPartitionedTopic(topicName, numberOfPartitions);
			// Special step to create partitioned topic

			// 1. producer connect
			Producer<sbyte[]> producer = pulsarClient.newProducer().topic(topicName).enableBatching(false).messageRoutingMode(MessageRoutingMode.RoundRobinPartition).create();

			// 2. Create consumer
			Consumer<sbyte[]> consumer1 = pulsarClient.newConsumer().topic(topicName).subscriptionName(subscriptionName).receiverQueueSize(100).subscriptionType(SubscriptionType.Shared).ackTimeout(ackTimeOutMillis, TimeUnit.MILLISECONDS).consumerName("Consumer-1").subscribe();
			Consumer<sbyte[]> consumer2 = pulsarClient.newConsumer().topic(topicName).subscriptionName(subscriptionName).receiverQueueSize(100).subscriptionType(SubscriptionType.Shared).ackTimeout(ackTimeOutMillis, TimeUnit.MILLISECONDS).consumerName("Consumer-2").subscribe();

			// 3. producer publish messages
			for (int i = 0; i < totalMessages; i++)
			{
				string message = messagePredicate + i;
				MessageId msgId = producer.send(message.GetBytes());
				log.info("Message produced: {} -- msgId: {}", message, msgId);
			}

			// 4. Receive messages
			int messageCount1 = receiveAllMessage(consumer1, false);
			int messageCount2 = receiveAllMessage(consumer2, true);
			int ackCount1 = 0;
			int ackCount2 = messageCount2;

			log.info(key + " messageCount1 = " + messageCount1);
			log.info(key + " messageCount2 = " + messageCount2);
			log.info(key + " ackCount1 = " + ackCount1);
			log.info(key + " ackCount2 = " + ackCount2);
			assertEquals(messageCount1 + messageCount2, totalMessages);

			// 5. Check if Messages redelivered again
			// Since receive is a blocking call hoping that timeout will kick in
			Thread.Sleep((int)(ackTimeOutMillis * 1.1));
			log.info(key + " Timeout should be triggered now");
			messageCount1 = receiveAllMessage(consumer1, true);
			messageCount2 += receiveAllMessage(consumer2, false);

			ackCount1 = messageCount1;

			log.info(key + " messageCount1 = " + messageCount1);
			log.info(key + " messageCount2 = " + messageCount2);
			log.info(key + " ackCount1 = " + ackCount1);
			log.info(key + " ackCount2 = " + ackCount2);
			assertEquals(messageCount1 + messageCount2, totalMessages);
			assertEquals(ackCount1 + messageCount2, totalMessages);

			Thread.Sleep((int)(ackTimeOutMillis * 1.1));

			// Since receive is a blocking call hoping that timeout will kick in
			log.info(key + " Timeout should be triggered again");
			ackCount1 += receiveAllMessage(consumer1, true);
			ackCount2 += receiveAllMessage(consumer2, true);
			log.info(key + " ackCount1 = " + ackCount1);
			log.info(key + " ackCount2 = " + ackCount2);
			assertEquals(ackCount1 + ackCount2, totalMessages);
		}

        private static int receiveAllMessage<T1>(Consumer<T1> consumer, bool ackMessages)
		{
			int messagesReceived = 0;
            Message<object> msg = consumer.receive(1, TimeUnit.SECONDS);
			while (msg != null)
			{
				++messagesReceived;
				log.info("Consumer received {}", new string(msg.Data));

				if (ackMessages)
				{
					consumer.acknowledge(msg);
				}

				msg = consumer.receive(1, TimeUnit.SECONDS);
			}

			return messagesReceived;
		}


		public virtual void testFailoverSingleAckedPartitionedTopic()
		{
			string key = "testFailoverSingleAckedPartitionedTopic";
            string topicName = "persistent://prop/ns-abc/topic-" + key + System.Guid.randomUUID().ToString();

            string subscriptionName = "my-failover-subscription-" + key;

            string messagePredicate = "my-message-" + key + "-";
			const int totalMessages = 10;
			const int numberOfPartitions = 3;
			admin.topics().createPartitionedTopic(topicName, numberOfPartitions);
			// Special step to create partitioned topic

			// 1. producer connect
			Producer<sbyte[]> producer = pulsarClient.newProducer().topic(topicName).enableBatching(false).messageRoutingMode(MessageRoutingMode.RoundRobinPartition).create();

			// 2. Create consumer
			Consumer<sbyte[]> consumer1 = pulsarClient.newConsumer().topic(topicName).subscriptionName(subscriptionName).receiverQueueSize(7).subscriptionType(SubscriptionType.Shared).ackTimeout(ackTimeOutMillis, TimeUnit.MILLISECONDS).acknowledgmentGroupTime(0, TimeUnit.SECONDS).consumerName("Consumer-1").subscribe();
			Consumer<sbyte[]> consumer2 = pulsarClient.newConsumer().topic(topicName).subscriptionName(subscriptionName).receiverQueueSize(7).subscriptionType(SubscriptionType.Shared).ackTimeout(ackTimeOutMillis, TimeUnit.MILLISECONDS).acknowledgmentGroupTime(0, TimeUnit.SECONDS).consumerName("Consumer-2").subscribe();

			// 3. producer publish messages
			for (int i = 0; i < totalMessages; i++)
			{
				string message = messagePredicate + i;
				log.info("Message produced: " + message);
				producer.send(message.GetBytes());
			}

			// 4. Receive messages
			int messagesReceived = 0;
			while (true)
			{
				Message<sbyte[]> message1 = consumer1.receive(500, TimeUnit.MILLISECONDS);
				if (message1 == null)
				{
					break;
				}

				++messagesReceived;
			}

			int ackCount = 0;
			while (true)
			{
				Message<sbyte[]> message2 = consumer2.receive(500, TimeUnit.MILLISECONDS);
				if (message2 == null)
				{
					break;
				}

				consumer2.acknowledge(message2);
				++messagesReceived;
				++ackCount;
			}

			assertEquals(messagesReceived, totalMessages);

			// 5. Check if Messages redelivered again
			Thread.Sleep(ackTimeOutMillis);
			log.info(key + " Timeout should be triggered now");
			messagesReceived = 0;
			while (true)
			{
				Message<sbyte[]> message1 = consumer1.receive(500, TimeUnit.MILLISECONDS);
				if (message1 == null)
				{
					break;
				}

				++messagesReceived;
			}

			while (true)
			{
				Message<sbyte[]> message2 = consumer2.receive(500, TimeUnit.MILLISECONDS);
				if (message2 == null)
				{
					break;
				}

				++messagesReceived;
			}

			assertEquals(messagesReceived + ackCount, totalMessages);
		}

        public virtual void testAckTimeoutMinValue()
		{
			try
			{
				pulsarClient.newConsumer().ackTimeout(999, TimeUnit.MILLISECONDS);
				Assert.fail("Exception should have been thrown since the set timeout is less than min timeout.");
			}
			catch (Exception)
			{
				// Ok
			}
		}

        public virtual void testCheckUnAcknowledgedMessageTimer()
		{
			string key = "testCheckUnAcknowledgedMessageTimer";

			string topicName = "persistent://prop/ns-abc/topic-" + key;

			string subscriptionName = "my-ex-subscription-" + key;

			string messagePredicate = "my-message-" + key + "-";
			const int totalMessages = 3;

			// 1. producer connect
			Producer<sbyte[]> producer = pulsarClient.newProducer().topic(topicName).enableBatching(false).messageRoutingMode(MessageRoutingMode.SinglePartition).create();

			// 2. Create consumer
			ConsumerImpl<sbyte[]> consumer = (ConsumerImpl<sbyte[]>) pulsarClient.newConsumer().topic(topicName).subscriptionName(subscriptionName).receiverQueueSize(7).subscriptionType(SubscriptionType.Shared).ackTimeout(ackTimeOutMillis, TimeUnit.MILLISECONDS).subscribe();

			// 3. producer publish messages
			for (int i = 0; i < totalMessages; i++)
			{
				string message = messagePredicate + i;
				log.info("Producer produced: " + message);
				producer.send(message.GetBytes());
			}

			Thread.Sleep((long)(ackTimeOutMillis * 1.1));

			for (int i = 0; i < totalMessages; i++)
			{
				Message<sbyte[]> msg = consumer.receive();
				if (i != totalMessages - 1)
				{
					consumer.acknowledge(msg);
				}
			}

			assertEquals(consumer.UnAckedMessageTracker.size(), 1);

			Message<sbyte[]> msg = consumer.receive();
			consumer.acknowledge(msg);
			assertEquals(consumer.UnAckedMessageTracker.size(), 0);

			Thread.Sleep((long)(ackTimeOutMillis * 1.1));

			assertEquals(consumer.UnAckedMessageTracker.size(), 0);
		}


		public virtual void testSingleMessageBatch()
		{
			string topicName = "prop/ns-abc/topic-estSingleMessageBatch";

			Producer<string> producer = pulsarClient.newProducer(Schema_Fields.STRING).topic(topicName).enableBatching(true).batchingMaxPublishDelay(10, TimeUnit.SECONDS).create();

			Consumer<string> consumer = pulsarClient.newConsumer(Schema_Fields.STRING).topic(topicName).subscriptionName("subscription").ackTimeout(1, TimeUnit.HOURS).subscribe();

			// Force the creation of a batch with a single message
			producer.sendAsync("hello");
			producer.flush();

			Message<string> message = consumer.receive();

			assertFalse(((ConsumerImpl<object>) consumer).UnAckedMessageTracker.Empty);

			consumer.acknowledge(message);

			assertTrue(((ConsumerImpl<object>) consumer).UnAckedMessageTracker.Empty);
		}
	}

}