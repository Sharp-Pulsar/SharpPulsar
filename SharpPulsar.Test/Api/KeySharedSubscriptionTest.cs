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
namespace SharpPulsar.Test.Api
{
using PositionImpl = org.apache.bookkeeper.mledger.impl.PositionImpl;
using Topic = org.apache.pulsar.broker.service.Topic;
using PersistentSubscription = org.apache.pulsar.broker.service.persistent.PersistentSubscription;
using Logger = org.slf4j.Logger;

public class KeySharedSubscriptionTest : ProducerConsumerBase
	{

		private static readonly Logger log = LoggerFactory.getLogger(typeof(KeySharedSubscriptionTest));
		private static readonly IList<string> keys = Arrays.asList("0", "1", "2", "3", "4", "5", "6", "7", "8", "9");


		public virtual object[][] batchProvider()
		{
			return new object[][]
			{
				new object[] {false},
				new object[] {true}
			};
		}



		public override void setup()
		{
			base.internalSetup();
			base.producerBaseSetup();
			this.conf.SubscriptionKeySharedUseConsistentHashing = true;
		}

		public override void cleanup()
		{
			base.internalCleanup();
		}

		private static readonly Random random = new Random(System.nanoTime());
		private const int NUMBER_OF_KEYS = 300;

		public virtual void testSendAndReceiveWithHashRangeAutoSplitStickyKeyConsumerSelector(string topicType, bool enableBatch)
		{
			this.conf.SubscriptionKeySharedEnable = true;
			string topic = topicType + "://public/default/key_shared-" + System.Guid.randomUUID();

			Consumer<int> consumer1 = createConsumer(topic);

			Consumer<int> consumer2 = createConsumer(topic);

			Consumer<int> consumer3 = createConsumer(topic);

			Producer<int> producer = createProducer(topic, enableBatch);

			for (int i = 0; i < 1000; i++)
			{
				producer.newMessage().key(random.Next(NUMBER_OF_KEYS).ToString()).value(i).send();
			}

			receiveAndCheckDistribution(Lists.newArrayList(consumer1, consumer2, consumer3));
		}


		public virtual void testSendAndReceiveWithBatching(string topicType, bool enableBatch)
		{
			this.conf.SubscriptionKeySharedEnable = true;
			string topic = topicType + "://public/default/key_shared-" + System.Guid.randomUUID();


			Consumer<int> consumer1 = createConsumer(topic);


			Consumer<int> consumer2 = createConsumer(topic);


			Consumer<int> consumer3 = createConsumer(topic);


			Producer<int> producer = createProducer(topic, enableBatch);

			for (int i = 0; i < 1000; i++)
			{
				// Send the same key twice so that we'll have a batch message
				string key = random.Next(NUMBER_OF_KEYS).ToString();
				producer.newMessage().key(key).value(i).sendAsync();

				producer.newMessage().key(key).value(i).sendAsync();
			}

			producer.flush();

			receiveAndCheckDistribution(Lists.newArrayList(consumer1, consumer2, consumer3));
		}


		public virtual void testSendAndReceiveWithHashRangeExclusiveStickyKeyConsumerSelector(bool enableBatch)
		{
			this.conf.SubscriptionKeySharedEnable = true;
			string topic = "persistent://public/default/key_shared_exclusive-" + System.Guid.randomUUID();


			Consumer<int> consumer1 = createConsumer(topic, KeySharedPolicy.stickyHashRange().ranges(Range.of(0, 20000)));


			Consumer<int> consumer2 = createConsumer(topic, KeySharedPolicy.stickyHashRange().ranges(Range.of(20001, 40000)));

            Consumer<int> consumer3 = createConsumer(topic, KeySharedPolicy.stickyHashRange().ranges(Range.of(40001, KeySharedPolicy.DEFAULT_HASH_RANGE_SIZE)));

            Producer<int> producer = createProducer(topic, enableBatch);

			int consumer1ExpectMessages = 0;
			int consumer2ExpectMessages = 0;
			int consumer3ExpectMessages = 0;

			for (int i = 0; i < 10; i++)
			{
				foreach (string key in keys)
				{
					int slot = Murmur3_32Hash.Instance.makeHash(key.GetBytes()) % KeySharedPolicy.DEFAULT_HASH_RANGE_SIZE;
					if (slot <= 20000)
					{
						consumer1ExpectMessages++;
					}
					else if (slot <= 40000)
					{
						consumer2ExpectMessages++;
					}
					else
					{
						consumer3ExpectMessages++;
					}
					producer.newMessage().key(key).value(i).send();
				}
			}

			IList<KeyValue<Consumer<int>, int>> checkList = new List<KeyValue<Consumer<int>, int>>();
			checkList.Add(new KeyValue<>(consumer1, consumer1ExpectMessages));
			checkList.Add(new KeyValue<>(consumer2, consumer2ExpectMessages));
			checkList.Add(new KeyValue<>(consumer3, consumer3ExpectMessages));

			receiveAndCheck(checkList);

		}

        public virtual void testConsumerCrashSendAndReceiveWithHashRangeAutoSplitStickyKeyConsumerSelector(string topicType, bool enableBatch)
		{

			this.conf.SubscriptionKeySharedEnable = true;
			string topic = topicType + "://public/default/key_shared_consumer_crash-" + System.Guid.randomUUID();

            Consumer<int> consumer1 = createConsumer(topic);

            Consumer<int> consumer2 = createConsumer(topic);

            Consumer<int> consumer3 = createConsumer(topic);

            Producer<int> producer = createProducer(topic, enableBatch);

			for (int i = 0; i < 1000; i++)
			{
				producer.newMessage().key(random.Next(NUMBER_OF_KEYS).ToString()).value(i).send();
			}

			receiveAndCheckDistribution(Lists.newArrayList(consumer1, consumer2, consumer3));

			// wait for consumer grouping acking send.
			Thread.Sleep(1000);

			consumer1.close();
			consumer2.close();

			for (int i = 0; i < 10; i++)
			{
				producer.newMessage().key(random.Next(NUMBER_OF_KEYS).ToString()).value(i).send();
			}

			receiveAndCheckDistribution(Lists.newArrayList(consumer3));
		}

        public virtual void testNonKeySendAndReceiveWithHashRangeAutoSplitStickyKeyConsumerSelector(string topicType, bool enableBatch)
		{
			this.conf.SubscriptionKeySharedEnable = true;
			string topic = topicType + "://public/default/key_shared_none_key-" + System.Guid.randomUUID();

            Consumer<int> consumer1 = createConsumer(topic);

            Consumer<int> consumer2 = createConsumer(topic);

            Consumer<int> consumer3 = createConsumer(topic);

            Producer<int> producer = createProducer(topic, enableBatch);

			for (int i = 0; i < 100; i++)
			{
				producer.newMessage().value(i).send();
			}

			receive(Lists.newArrayList(consumer1, consumer2, consumer3));
		}

        public virtual void testNonKeySendAndReceiveWithHashRangeExclusiveStickyKeyConsumerSelector(bool enableBatch)
		{
			this.conf.SubscriptionKeySharedEnable = true;
			string topic = "persistent://public/default/key_shared_none_key_exclusive-" + System.Guid.randomUUID();

            Consumer<int> consumer1 = createConsumer(topic, KeySharedPolicy.stickyHashRange().ranges(Range.of(0, 20000)));

            Consumer<int> consumer2 = createConsumer(topic, KeySharedPolicy.stickyHashRange().ranges(Range.of(20001, 40000)));

            Consumer<int> consumer3 = createConsumer(topic, KeySharedPolicy.stickyHashRange().ranges(Range.of(40001, KeySharedPolicy.DEFAULT_HASH_RANGE_SIZE)));

            Producer<int> producer = createProducer(topic, enableBatch);

			for (int i = 0; i < 100; i++)
			{
				producer.newMessage().value(i).send();
			}
			int slot = Murmur3_32Hash.Instance.makeHash(PersistentStickyKeyDispatcherMultipleConsumers.NONE_KEY.GetBytes()) % KeySharedPolicy.DEFAULT_HASH_RANGE_SIZE;
			IList<KeyValue<Consumer<int>, int>> checkList = new List<KeyValue<Consumer<int>, int>>();
			if (slot <= 20000)
			{
				checkList.Add(new KeyValue<>(consumer1, 100));
			}
			else if (slot <= 40000)
			{
				checkList.Add(new KeyValue<>(consumer2, 100));
			}
			else
			{
				checkList.Add(new KeyValue<>(consumer3, 100));
			}
			receiveAndCheck(checkList);
		}

        public virtual void testOrderingKeyWithHashRangeAutoSplitStickyKeyConsumerSelector(bool enableBatch)
		{
			this.conf.SubscriptionKeySharedEnable = true;
			string topic = "persistent://public/default/key_shared_ordering_key-" + System.Guid.randomUUID();

            Consumer<int> consumer1 = createConsumer(topic);

            Consumer<int> consumer2 = createConsumer(topic);

            Consumer<int> consumer3 = createConsumer(topic);

            Producer<int> producer = createProducer(topic, enableBatch);

			for (int i = 0; i < 1000; i++)
			{
				producer.newMessage().key("any key").orderingKey(random.Next(NUMBER_OF_KEYS).ToString().GetBytes()).value(i).send();
			}

			receiveAndCheckDistribution(Lists.newArrayList(consumer1, consumer2, consumer3));
		}

        public virtual void testOrderingKeyWithHashRangeExclusiveStickyKeyConsumerSelector(bool enableBatch)
		{
			this.conf.SubscriptionKeySharedEnable = true;
			string topic = "persistent://public/default/key_shared_exclusive_ordering_key-" + System.Guid.randomUUID();

            Consumer<int> consumer1 = createConsumer(topic, KeySharedPolicy.stickyHashRange().ranges(Range.of(0, 20000)));

           
            Consumer<int> consumer3 = createConsumer(topic, KeySharedPolicy.stickyHashRange().ranges(Range.of(40001, KeySharedPolicy.DEFAULT_HASH_RANGE_SIZE)));

            Producer<int> producer = createProducer(topic, enableBatch);

			int consumer1ExpectMessages = 0;
			int consumer2ExpectMessages = 0;
			int consumer3ExpectMessages = 0;

			for (int i = 0; i < 10; i++)
			{
				foreach (string key in keys)
				{
					int slot = Murmur3_32Hash.Instance.makeHash(key.GetBytes()) % KeySharedPolicy.DEFAULT_HASH_RANGE_SIZE;
					if (slot <= 20000)
					{
						consumer1ExpectMessages++;
					}
					else if (slot <= 40000)
					{
						consumer2ExpectMessages++;
					}
					else
					{
						consumer3ExpectMessages++;
					}
					producer.newMessage().key("any key").orderingKey(key.GetBytes()).value(i).send();
				}
			}

			IList<KeyValue<Consumer<int>, int>> checkList = new List<KeyValue<Consumer<int>, int>>();
			checkList.Add(new KeyValue<>(consumer1, consumer1ExpectMessages));
			checkList.Add(new KeyValue<>(consumer2, consumer2ExpectMessages));
			checkList.Add(new KeyValue<>(consumer3, consumer3ExpectMessages));

			receiveAndCheck(checkList);
		}

        public virtual void testDisableKeySharedSubscription()
		{
			this.conf.SubscriptionKeySharedEnable = false;
			string topic = "persistent://public/default/key_shared_disabled";
			pulsarClient.newConsumer().topic(topic).subscriptionName("key_shared").subscriptionType(SubscriptionType.Key_Shared).ackTimeout(10, TimeUnit.SECONDS).subscribe();
		}

        public virtual void testCannotUseAcknowledgeCumulative()
		{
			this.conf.SubscriptionKeySharedEnable = true;
			string topic = "persistent://public/default/key_shared_ack_cumulative-" + System.Guid.randomUUID();

            Producer<int> producer = createProducer(topic, false);

            Consumer<int> consumer = createConsumer(topic);

			for (int i = 0; i < 10; i++)
			{
				producer.send(i);
			}

			for (int i = 0; i < 10; i++)
			{
				Message<int> message = consumer.receive();
				if (i == 9)
				{
					try
					{
						consumer.acknowledgeCumulative(message);
						fail("should have failed");
					}
					catch (PulsarClientException.InvalidConfigurationException)
					{
					}
				}
			}
		}

        public virtual void testMakingProgressWithSlowerConsumer(bool enableBatch)
		{
			this.conf.SubscriptionKeySharedEnable = true;
			string topic = "testMakingProgressWithSlowerConsumer-" + System.Guid.randomUUID();

			string slowKey = "slowKey";

			IList<PulsarClient> clients = new List<PulsarClient>();

			AtomicInteger receivedMessages = new AtomicInteger();

			for (int i = 0; i < 10; i++)
			{
				PulsarClient client = PulsarClient.builder().serviceUrl(brokerUrl.ToString()).build();
				clients.Add(client);

				client.newConsumer(Schema_Fields.INT32).topic(topic).subscriptionName("key_shared").subscriptionType(SubscriptionType.Key_Shared).receiverQueueSize(1).messageListener((consumer, msg) =>
				{
				try
				{
					if (slowKey.Equals(msg.Key))
					{
						Thread.Sleep(10000);
					}
					receivedMessages.incrementAndGet();
					consumer.acknowledge(msg);
				}
				catch (Exception e)
				{
					Console.WriteLine(e.ToString());
					Console.Write(e.StackTrace);
				}
				}).subscribe();
			}

            Producer<int> producer = createProducer(topic, enableBatch);

			// First send the "slow key" so that 1 consumer will get stuck
			producer.newMessage().key(slowKey).value(-1).send();

			int N = 1000;

			// Then send all the other keys
			for (int i = 0; i < N; i++)
			{
				producer.newMessage().key(random.Next(NUMBER_OF_KEYS).ToString()).value(i).send();
			}

			// Since only 1 out of 10 consumers is stuck, we should be able to receive ~90% messages,
			// plus or minus for some skew in the key distribution.
			Thread.Sleep(5000);

			assertEquals((double) receivedMessages.get(), N * 0.9, N * 0.3);

			foreach (PulsarClient c in clients)
			{
				c.Dispose();
			}
		}

        public virtual void testOrderingWhenAddingConsumers()
		{
			this.conf.SubscriptionKeySharedEnable = true;
			string topic = "testOrderingWhenAddingConsumers-" + System.Guid.randomUUID();

            Producer<int> producer = createProducer(topic, false);

            Consumer<int> c1 = createConsumer(topic);

			for (int i = 0; i < 10; i++)
			{
				producer.newMessage().key(random.Next(NUMBER_OF_KEYS).ToString()).value(i).send();
			}

			// All the already published messages will be pre-fetched by C1.

			// Adding a new consumer.

            Consumer<int> c2 = createConsumer(topic);

			for (int i = 10; i < 20; i++)
			{
				producer.newMessage().key(random.Next(NUMBER_OF_KEYS).ToString()).value(i).send();
			}

			// Closing c1, would trigger all messages to go to c2
			c1.close();

			for (int i = 0; i < 20; i++)
			{
				Message<int> msg = c2.receive();
				assertEquals(msg.Value, i);

				c2.acknowledge(msg);
			}
		}


        public virtual void testReadAheadWhenAddingConsumers()
		{
			this.conf.SubscriptionKeySharedEnable = true;
			string topic = "testReadAheadWhenAddingConsumers-" + System.Guid.randomUUID();

            Producer<int> producer = createProducer(topic, false);

            Consumer<int> c1 = pulsarClient.newConsumer(Schema_Fields.INT32).topic(topic).subscriptionName("key_shared").subscriptionType(SubscriptionType.Key_Shared).receiverQueueSize(10).subscribe();

			for (int i = 0; i < 10; i++)
			{
				producer.newMessage().key(random.Next(NUMBER_OF_KEYS).ToString()).value(i).send();
			}

			// All the already published messages will be pre-fetched by C1.

	
            Consumer<int> c2 = pulsarClient.newConsumer(Schema_Fields.INT32).topic(topic).subscriptionName("key_shared").subscriptionType(SubscriptionType.Key_Shared).receiverQueueSize(10).subscribe();

			// C2 will not be able to receive any messages until C1 is done processing whatever he got prefetched

			for (int i = 10; i < 1000; i++)
			{
				producer.newMessage().key(random.Next(NUMBER_OF_KEYS).ToString()).value(i).sendAsync();
			}

			producer.flush();
			Thread.Sleep(1000);

			Topic t = org.apache.pulsar.BrokerService.getTopicIfExists(topic).get().get();
			PersistentSubscription sub = (PersistentSubscription) t.getSubscription("key_shared");

			// We need to ensure that dispatcher does not keep to look ahead in the topic,
			PositionImpl readPosition = (PositionImpl) sub.Cursor.ReadPosition;
			assertTrue(readPosition.EntryId < 1000);
		}

        public virtual void testRemoveFirstConsumer()
		{
			this.conf.SubscriptionKeySharedEnable = true;
			string topic = "testReadAheadWhenAddingConsumers-" + System.Guid.randomUUID();

            Producer<int> producer = createProducer(topic, false);

            Consumer<int> c1 = pulsarClient.newConsumer(Schema_Fields.INT32).topic(topic).subscriptionName("key_shared").subscriptionType(SubscriptionType.Key_Shared).receiverQueueSize(10).consumerName("c1").subscribe();

			for (int i = 0; i < 10; i++)
			{
				producer.newMessage().key(random.Next(NUMBER_OF_KEYS).ToString()).value(i).send();
			}

			// All the already published messages will be pre-fetched by C1.

			// Adding a new consumer.

            Consumer<int> c2 = pulsarClient.newConsumer(Schema_Fields.INT32).topic(topic).subscriptionName("key_shared").subscriptionType(SubscriptionType.Key_Shared).receiverQueueSize(10).consumerName("c2").subscribe();

			for (int i = 10; i < 20; i++)
			{
				producer.newMessage().key(random.Next(NUMBER_OF_KEYS).ToString()).value(i).send();
			}

			// C2 will not be able to receive any messages until C1 is done processing whatever he got prefetched
			assertNull(c2.receive(100, TimeUnit.MILLISECONDS));

			c1.close();

			// Now C2 will get all messages
			for (int i = 0; i < 20; i++)
			{
				Message<int> msg = c2.receive();
				assertEquals(msg.Value, i);
				c2.acknowledge(msg);
			}
		}

        private Producer<int> createProducer(string topic, bool enableBatch)
		{
			Producer<int> producer = null;
			if (enableBatch)
			{
				producer = pulsarClient.newProducer(Schema_Fields.INT32).topic(topic).enableBatching(true).batcherBuilder(BatcherBuilder_Fields.KEY_BASED).create();
			}
			else
			{
				producer = pulsarClient.newProducer(Schema_Fields.INT32).topic(topic).enableBatching(false).create();
			}
			return producer;
		}

        private Consumer<int> createConsumer(string topic)
		{
			return createConsumer(topic, null);
		}

        private Consumer<int> createConsumer(string topic, KeySharedPolicy keySharedPolicy)
		{
			ConsumerBuilder<int> builder = pulsarClient.newConsumer(Schema_Fields.INT32);
			builder.topic(topic).subscriptionName("key_shared").subscriptionType(SubscriptionType.Key_Shared).ackTimeout(3, TimeUnit.SECONDS);
			if (keySharedPolicy != null)
			{
				builder.keySharedPolicy(keySharedPolicy);
			}
			return builder.subscribe();
		}

        private void receive<T1>(IList<T1> consumers)
		{
			// Add a key so that we know this key was already assigned to one consumer

            IDictionary<string, Consumer<object>> keyToConsumer = new Dictionary<string, Consumer<object>>();

            foreach (Consumer<object> c in consumers)
			{
				while (true)
				{
                    Message<object> msg = c.receive(100, TimeUnit.MILLISECONDS);
					if (msg == null)
					{
						// Go to next consumer
						break;
					}

					c.acknowledge(msg);

					if (msg.hasKey())
					{

                        Consumer<object> assignedConsumer = keyToConsumer[msg.Key];
						if (assignedConsumer == null)
						{
							// This is a new key
							keyToConsumer[msg.Key] = c;
						}
						else
						{
							// The consumer should be the same
							assertEquals(c, assignedConsumer);
						}
					}
				}
			}
		}

		/// <summary>
		/// Check that every consumer receives a fair number of messages and that same key is delivered to only 1 consumer
		/// </summary>
        private void receiveAndCheckDistribution<T1>(IList<T1> consumers)
		{
			// Add a key so that we know this key was already assigned to one consumer

            IDictionary<string, Consumer<object>> keyToConsumer = new Dictionary<string, Consumer<object>>();

            IDictionary<Consumer<object>, int> messagesPerConsumer = new Dictionary<Consumer<object>, int>();

			int totalMessages = 0;


            foreach (Consumer<object> c in consumers)
			{
				int messagesForThisConsumer = 0;
				while (true)
				{

                    Message<object> msg = c.receive(100, TimeUnit.MILLISECONDS);
					if (msg == null)
					{
						// Go to next consumer
						messagesPerConsumer[c] = messagesForThisConsumer;
						break;
					}

					++totalMessages;
					++messagesForThisConsumer;
					c.acknowledge(msg);

					if (msg.hasKey() || msg.hasOrderingKey())
					{
						string key = msg.hasOrderingKey() ? new string(msg.OrderingKey) : msg.Key;

                        Consumer<object> assignedConsumer = keyToConsumer[key];
						if (assignedConsumer == null)
						{
							// This is a new key
							keyToConsumer[key] = c;
						}
						else
						{
							// The consumer should be the same
							assertEquals(c, assignedConsumer);
						}
					}
				}
			}

			const double PERCENT_ERROR = 0.40; // 40 %

			double expectedMessagesPerConsumer = totalMessages / consumers.Count;

			Console.Error.WriteLine(messagesPerConsumer);
			foreach (int count in messagesPerConsumer.Values)
			{
				Assert.assertEquals(count, expectedMessagesPerConsumer, expectedMessagesPerConsumer * PERCENT_ERROR);
			}
		}

        private void receiveAndCheck(IList<KeyValue<Consumer<int>, int>> checkList)
		{
			IDictionary<Consumer, ISet<string>> consumerKeys = new Dictionary<Consumer, ISet<string>>();
			foreach (KeyValue<Consumer<int>, int> check in checkList)
			{
				if (check.Value % 2 != 0)
				{
					throw new System.ArgumentException();
				}
				int received = 0;
				IDictionary<string, Message<int>> lastMessageForKey = new Dictionary<string, Message<int>>();
				for (int? i = 0; i.Value < check.Value; i.Value++)
				{
					Message<int> message = check.Key.receive();
					if (i % 2 == 0)
					{
						check.Key.acknowledge(message);
					}
					string key = message.hasOrderingKey() ? new string(message.OrderingKey) : message.Key;
					log.info("[{}] Receive message key: {} value: {} messageId: {}", check.Key.ConsumerName, key, message.Value, message.MessageId);
					// check messages is order by key
					if (lastMessageForKey[key] == null)
					{
						Assert.assertNotNull(message);
					}
					else
					{
						Assert.assertTrue(message.Value.compareTo(lastMessageForKey[key].Value) > 0);
					}
					lastMessageForKey[key] = message;
					if (!consumerKeys.ContainsKey(check.Key)) consumerKeys.Add(check.Key, Sets.newHashSet());
					consumerKeys[check.Key].Add(key);
					received++;
				}
				Assert.assertEquals(check.Value, received);
				int redeliveryCount = check.Value / 2;
				log.info("[{}] Consumer wait for {} messages redelivery ...", redeliveryCount);
				// messages not acked, test redelivery
				lastMessageForKey = new Dictionary<string, Message<int>>();
				for (int i = 0; i < redeliveryCount; i++)
				{
					Message<int> message = check.Key.receive();
					received++;
					check.Key.acknowledge(message);
					string key = message.hasOrderingKey() ? new string(message.OrderingKey) : message.Key;
					log.info("[{}] Receive redeliver message key: {} value: {} messageId: {}", check.Key.ConsumerName, key, message.Value, message.MessageId);
					// check redelivery messages is order by key
					if (lastMessageForKey[key] == null)
					{
						Assert.assertNotNull(message);
					}
					else
					{
						Assert.assertTrue(message.Value.compareTo(lastMessageForKey[key].Value) > 0);
					}
					lastMessageForKey[key] = message;
				}
				Message noMessages = null;
				try
				{
					noMessages = check.Key.receive(100, TimeUnit.MILLISECONDS);
				}
				catch (PulsarClientException)
				{
				}
				Assert.assertNull(noMessages, "redeliver too many messages.");
				Assert.assertEquals((check.Value + redeliveryCount), received);
			}
			ISet<string> allKeys = Sets.newHashSet();
			consumerKeys.forEach((k, v) => v.forEach(key =>
			{
			assertTrue(allKeys.Add(key), "Key " + key + "is distributed to multiple consumers.");
			}));
		}
	}

}