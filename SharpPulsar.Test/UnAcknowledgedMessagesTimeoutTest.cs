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
namespace SharpPulsar.Test
{

	public class UnAcknowledgedMessagesTimeoutTest : BrokerTestBase
	{
		private static readonly Logger _log = LoggerFactory.getLogger(typeof(UnAcknowledgedMessagesTimeoutTest));
		private readonly long _ackTimeOutMillis = TimeUnit.SECONDS.toMillis(2);

		public override void Setup()
		{
			base.BaseSetup();
		}


		public override void Cleanup()
		{
			base.InternalCleanup();
		}


		public virtual void TestExclusiveSingleAckedNormalTopic()
		{
			string key = "testExclusiveSingleAckedNormalTopic";

			string topicName = "persistent://prop/ns-abc/topic-" + key;

			string subscriptionName = "my-ex-subscription-" + key;

			string messagePredicate = "my-message-" + key + "-";
			const int totalMessages = 10;

			// 1. producer connect
			Producer<sbyte[]> producer = PulsarClient.NewProducer().Topic(topicName).EnableBatching(false).MessageRoutingMode(MessageRoutingMode.SinglePartition).Create();

			// 2. Create consumer
			Consumer<sbyte[]> consumer = PulsarClient.NewConsumer().Topic(topicName).SubscriptionName(subscriptionName).ReceiverQueueSize(7).AckTimeout(_ackTimeOutMillis, TimeUnit.MILLISECONDS).Subscribe();

			// 3. producer publish messages
			for(int i = 0; i < totalMessages / 2; i++)
			{
				string message = messagePredicate + i;
				_log.info("Producer produced: " + message);
				producer.send(message.GetBytes());
			}

			// 4. Receiver receives the message
			Message<sbyte[]> message = consumer.Receive();
			while(message != null)
			{
				_log.info("Consumer received : " + new string(message.Data));
				message = consumer.Receive(500, TimeUnit.MILLISECONDS);
			}

			long size = ((ConsumerImpl<object>) consumer).UnAckedMessageTracker.size();
			_log.info(key + " Unacked Message Tracker size is " + size);
			assertEquals(size, totalMessages / 2);

			// Blocking call, redeliver should kick in
			message = consumer.Receive();
			_log.info("Consumer received : " + new string(message.Data));

			HashSet<string> hSet = new HashSet<string>();
			for(int i = totalMessages / 2; i < totalMessages; i++)
			{
				string messageString = messagePredicate + i;
				producer.send(messageString.GetBytes());
			}

			do
			{
				hSet.Add(new string(message.Data));
				consumer.Acknowledge(message);
				_log.info("Consumer acknowledged : " + new string(message.Data));
				message = consumer.Receive(500, TimeUnit.MILLISECONDS);
			} while(message != null);


			size = ((ConsumerImpl<object>) consumer).UnAckedMessageTracker.size();
			_log.info(key + " Unacked Message Tracker size is " + size);
			assertEquals(size, 0);
			assertEquals(hSet.Count, totalMessages);
		}


		public virtual void TestExclusiveCumulativeAckedNormalTopic()
		{
			string key = "testExclusiveCumulativeAckedNormalTopic";

			string topicName = "persistent://prop/use/ns-abc/topic-" + key;

			string subscriptionName = "my-ex-subscription-" + key;

			string messagePredicate = "my-message-" + key + "-";
			const int totalMessages = 10;

			// 1. producer connect
			Producer<sbyte[]> producer = PulsarClient.NewProducer().Topic(topicName).EnableBatching(false).MessageRoutingMode(MessageRoutingMode.SinglePartition).Create();

			// 2. Create consumer
			Consumer<sbyte[]> consumer = PulsarClient.NewConsumer().Topic(topicName).SubscriptionName(subscriptionName).ReceiverQueueSize(7).AckTimeout(_ackTimeOutMillis, TimeUnit.MILLISECONDS).Subscribe();

			// 3. producer publish messages
			for(int i = 0; i < totalMessages; i++)
			{
				string message = messagePredicate + i;
				producer.send(message.GetBytes());
			}

			// 4. Receiver receives the message
			HashSet<string> hSet = new HashSet<string>();
			Message<sbyte[]> message = consumer.Receive();
			Message<sbyte[]> lastMessage = message;
			while(message != null)
			{
				lastMessage = message;
				hSet.Add(new string(message.Data));
				_log.info("Consumer received " + new string(message.Data));
				_log.info("Message ID details " + message.MessageId.ToString());
				message = consumer.Receive(500, TimeUnit.MILLISECONDS);
			}

			long size = ((ConsumerImpl<object>) consumer).UnAckedMessageTracker.size();
			assertEquals(size, totalMessages);
			_log.info("Comulative Ack sent for " + new string(lastMessage.Data));
			_log.info("Message ID details " + lastMessage.MessageId.ToString());
			consumer.AcknowledgeCumulative(lastMessage);

			size = ((ConsumerImpl<object>) consumer).UnAckedMessageTracker.size();
			assertEquals(size, 0);
			message = consumer.Receive((int)(2 * _ackTimeOutMillis), TimeUnit.MILLISECONDS);
			assertNull(message);
		}


		public virtual void TestSharedSingleAckedPartitionedTopic()
		{
			string key = "testSharedSingleAckedPartitionedTopic";

			string topicName = "persistent://prop/ns-abc/topic-" + key;

			string subscriptionName = "my-shared-subscription-" + key;

			string messagePredicate = "my-message-" + key + "-";
			const int totalMessages = 20;
			const int numberOfPartitions = 3;
			Admin.Topics().CreatePartitionedTopic(topicName, numberOfPartitions);
			// Special step to create partitioned topic

			// 1. producer connect
			Producer<sbyte[]> producer = PulsarClient.NewProducer().Topic(topicName).EnableBatching(false).MessageRoutingMode(MessageRoutingMode.RoundRobinPartition).Create();

			// 2. Create consumer
			Consumer<sbyte[]> consumer1 = PulsarClient.NewConsumer().Topic(topicName).SubscriptionName(subscriptionName).ReceiverQueueSize(100).SubscriptionType(SubscriptionType.Shared).AckTimeout(_ackTimeOutMillis, TimeUnit.MILLISECONDS).ConsumerName("Consumer-1").Subscribe();
			Consumer<sbyte[]> consumer2 = PulsarClient.NewConsumer().Topic(topicName).SubscriptionName(subscriptionName).ReceiverQueueSize(100).SubscriptionType(SubscriptionType.Shared).AckTimeout(_ackTimeOutMillis, TimeUnit.MILLISECONDS).ConsumerName("Consumer-2").Subscribe();

			// 3. producer publish messages
			for(int i = 0; i < totalMessages; i++)
			{
				string message = messagePredicate + i;
				MessageId msgId = producer.send(message.GetBytes());
				_log.info("Message produced: {} -- msgId: {}", message, msgId);
			}

			// 4. Receive messages
			int messageCount1 = ReceiveAllMessage(consumer1, false);
			int messageCount2 = ReceiveAllMessage(consumer2, true);
			int ackCount1 = 0;
			int ackCount2 = messageCount2;

			_log.info(key + " messageCount1 = " + messageCount1);
			_log.info(key + " messageCount2 = " + messageCount2);
			_log.info(key + " ackCount1 = " + ackCount1);
			_log.info(key + " ackCount2 = " + ackCount2);
			assertEquals(messageCount1 + messageCount2, totalMessages);

			// 5. Check if Messages redelivered again
			// Since receive is a blocking call hoping that timeout will kick in
			Thread.Sleep((int)(_ackTimeOutMillis * 1.1));
			_log.info(key + " Timeout should be triggered now");
			messageCount1 = ReceiveAllMessage(consumer1, true);
			messageCount2 += ReceiveAllMessage(consumer2, false);

			ackCount1 = messageCount1;

			_log.info(key + " messageCount1 = " + messageCount1);
			_log.info(key + " messageCount2 = " + messageCount2);
			_log.info(key + " ackCount1 = " + ackCount1);
			_log.info(key + " ackCount2 = " + ackCount2);
			assertEquals(messageCount1 + messageCount2, totalMessages);
			assertEquals(ackCount1 + messageCount2, totalMessages);

			Thread.Sleep((int)(_ackTimeOutMillis * 1.1));

			// Since receive is a blocking call hoping that timeout will kick in
			_log.info(key + " Timeout should be triggered again");
			ackCount1 += ReceiveAllMessage(consumer1, true);
			ackCount2 += ReceiveAllMessage(consumer2, true);
			_log.info(key + " ackCount1 = " + ackCount1);
			_log.info(key + " ackCount2 = " + ackCount2);
			assertEquals(ackCount1 + ackCount2, totalMessages);
		}

		private static int ReceiveAllMessage<T1>(Consumer<T1> consumer, bool ackMessages)
		{
			int messagesReceived = 0;
			Message<object> msg = consumer.Receive(1, TimeUnit.SECONDS);
			while(msg != null)
			{
				++messagesReceived;
				_log.info("Consumer received {}", new string(msg.Data));

				if(ackMessages)
				{
					consumer.Acknowledge(msg);
				}

				msg = consumer.Receive(1, TimeUnit.SECONDS);
			}

			return messagesReceived;
		}

		public virtual void TestFailoverSingleAckedPartitionedTopic()
		{
			string key = "testFailoverSingleAckedPartitionedTopic";
			string topicName = "persistent://prop/ns-abc/topic-" + key + System.Guid.randomUUID().ToString();

			string subscriptionName = "my-failover-subscription-" + key;

			string messagePredicate = "my-message-" + key + "-";
			const int totalMessages = 10;
			const int numberOfPartitions = 3;
			Admin.Topics().CreatePartitionedTopic(topicName, numberOfPartitions);
			// Special step to create partitioned topic

			// 1. producer connect
			Producer<sbyte[]> producer = PulsarClient.NewProducer().Topic(topicName).EnableBatching(false).MessageRoutingMode(MessageRoutingMode.RoundRobinPartition).Create();

			// 2. Create consumer
			Consumer<sbyte[]> consumer1 = PulsarClient.NewConsumer().Topic(topicName).SubscriptionName(subscriptionName).ReceiverQueueSize(7).SubscriptionType(SubscriptionType.Shared).AckTimeout(_ackTimeOutMillis, TimeUnit.MILLISECONDS).AcknowledgmentGroupTime(0, TimeUnit.SECONDS).ConsumerName("Consumer-1").Subscribe();
			Consumer<sbyte[]> consumer2 = PulsarClient.NewConsumer().Topic(topicName).SubscriptionName(subscriptionName).ReceiverQueueSize(7).SubscriptionType(SubscriptionType.Shared).AckTimeout(_ackTimeOutMillis, TimeUnit.MILLISECONDS).AcknowledgmentGroupTime(0, TimeUnit.SECONDS).ConsumerName("Consumer-2").Subscribe();

			// 3. producer publish messages
			for(int i = 0; i < totalMessages; i++)
			{
				string message = messagePredicate + i;
				_log.info("Message produced: " + message);
				producer.send(message.GetBytes());
			}

			// 4. Receive messages
			int messagesReceived = 0;
			while(true)
			{
				Message<sbyte[]> message1 = consumer1.Receive(500, TimeUnit.MILLISECONDS);
				if(message1 == null)
				{
					break;
				}

				++messagesReceived;
			}

			int ackCount = 0;
			while(true)
			{
				Message<sbyte[]> message2 = consumer2.Receive(500, TimeUnit.MILLISECONDS);
				if(message2 == null)
				{
					break;
				}

				consumer2.Acknowledge(message2);
				++messagesReceived;
				++ackCount;
			}

			assertEquals(messagesReceived, totalMessages);

			// 5. Check if Messages redelivered again
			Thread.Sleep(_ackTimeOutMillis);
			_log.info(key + " Timeout should be triggered now");
			messagesReceived = 0;
			while(true)
			{
				Message<sbyte[]> message1 = consumer1.Receive(500, TimeUnit.MILLISECONDS);
				if(message1 == null)
				{
					break;
				}

				++messagesReceived;
			}

			while(true)
			{
				Message<sbyte[]> message2 = consumer2.Receive(500, TimeUnit.MILLISECONDS);
				if(message2 == null)
				{
					break;
				}

				++messagesReceived;
			}

			assertEquals(messagesReceived + ackCount, totalMessages);
		}

		public virtual void TestAckTimeoutMinValue()
		{
			try
			{
				PulsarClient.NewConsumer().AckTimeout(999, TimeUnit.MILLISECONDS);
				Assert.fail("Exception should have been thrown since the set timeout is less than min timeout.");
			}
			catch(Exception)
			{
				// Ok
			}
		}

		public virtual void TestCheckUnAcknowledgedMessageTimer()
		{
			string key = "testCheckUnAcknowledgedMessageTimer";

			string topicName = "persistent://prop/ns-abc/topic-" + key;

			string subscriptionName = "my-ex-subscription-" + key;

			string messagePredicate = "my-message-" + key + "-";
			const int totalMessages = 3;

			// 1. producer connect
			Producer<sbyte[]> producer = PulsarClient.NewProducer().Topic(topicName).EnableBatching(false).MessageRoutingMode(MessageRoutingMode.SinglePartition).Create();

			// 2. Create consumer
			ConsumerImpl<sbyte[]> consumer = (ConsumerImpl<sbyte[]>) PulsarClient.NewConsumer().Topic(topicName).SubscriptionName(subscriptionName).ReceiverQueueSize(7).SubscriptionType(SubscriptionType.Shared).AckTimeout(_ackTimeOutMillis, TimeUnit.MILLISECONDS).Subscribe();

			// 3. producer publish messages
			for(int i = 0; i < totalMessages; i++)
			{
				string message = messagePredicate + i;
				_log.info("Producer produced: " + message);
				producer.send(message.GetBytes());
			}

			Thread.Sleep((long)(_ackTimeOutMillis * 1.1));

			for(int i = 0; i < totalMessages; i++)
			{
				Message<sbyte[]> msg = consumer.Receive();
				if(i != totalMessages - 1)
				{
					consumer.Acknowledge(msg);
				}
			}

			assertEquals(consumer.UnAckedMessageTracker.size(), 1);

			Message<sbyte[]> msg = consumer.Receive();
			consumer.Acknowledge(msg);
			assertEquals(consumer.UnAckedMessageTracker.size(), 0);

			Thread.Sleep((long)(_ackTimeOutMillis * 1.1));

			assertEquals(consumer.UnAckedMessageTracker.size(), 0);
		}


		public virtual void TestSingleMessageBatch()
		{
			string topicName = "prop/ns-abc/topic-estSingleMessageBatch";

			Producer<string> producer = PulsarClient.NewProducer(Schema.STRING).Topic(topicName).EnableBatching(true).BatchingMaxPublishDelay(10, TimeUnit.SECONDS).Create();

			Consumer<string> consumer = PulsarClient.NewConsumer(Schema.STRING).Topic(topicName).SubscriptionName("subscription").AckTimeout(1, TimeUnit.HOURS).Subscribe();

			// Force the creation of a batch with a single message
			producer.sendAsync("hello");
			producer.Flush();

			Message<string> message = consumer.Receive();

			assertFalse(((ConsumerImpl<object>) consumer).UnAckedMessageTracker.Empty);

			consumer.Acknowledge(message);

			assertTrue(((ConsumerImpl<object>) consumer).UnAckedMessageTracker.Empty);
		}
	}

}