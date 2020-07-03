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

	public class PartitionedProducerConsumerTest : ProducerConsumerBase
	{
		private static readonly Logger log = LoggerFactory.getLogger(typeof(PartitionedProducerConsumerTest));

		private ExecutorService executor;


		public override void setup()
		{
			base.internalSetup();
			base.producerBaseSetup();

			executor = Executors.newFixedThreadPool(1, new DefaultThreadFactory("PartitionedProducerConsumerTest"));
		}


		public override void cleanup()
		{
			base.internalCleanup();
			executor.shutdown();
		}


		public virtual void testRoundRobinProducer()
		{
			log.info("-- Starting {} test --", methodName);
			PulsarClient pulsarClient = newPulsarClient(lookupUrl.ToString(), 0); // Creates new client connection

			int numPartitions = 4;
			TopicName topicName = TopicName.get("persistent://my-property/my-ns/my-partitionedtopic1-" + DateTimeHelper.CurrentUnixTimeMillis());

			admin.topics().createPartitionedTopic(topicName.ToString(), numPartitions);

			Producer<sbyte[]> producer = pulsarClient.newProducer().topic(topicName.ToString()).enableBatching(false).messageRoutingMode(MessageRoutingMode.RoundRobinPartition).create();

			Consumer<sbyte[]> consumer = pulsarClient.newConsumer().topic(topicName.ToString()).subscriptionName("my-partitioned-subscriber").subscribe();
			assertEquals(consumer.Topic, topicName.ToString());

			for (int i = 0; i < 10; i++)
			{
				string message = "my-message-" + i;
				producer.send(message.GetBytes());
			}

			Message<sbyte[]> msg;
			ISet<string> messageSet = Sets.newHashSet();
			for (int i = 0; i < 10; i++)
			{
				msg = consumer.receive(5, TimeUnit.SECONDS);
				Assert.assertNotNull(msg, "Message should not be null");
				consumer.acknowledge(msg);
				string receivedMessage = new string(msg.Data);
				log.debug("Received message: [{}]", receivedMessage);
				Assert.assertTrue(messageSet.Add(receivedMessage), "Message " + receivedMessage + " already received");
			}

			producer.close();
			consumer.unsubscribe();
			consumer.close();
			pulsarClient.Dispose();
			admin.topics().deletePartitionedTopic(topicName.ToString());

			log.info("-- Exiting {} test --", methodName);
		}


		public virtual void testPartitionedTopicNameWithSpecialCharacter()
		{
			log.info("-- Starting {} test --", methodName);

			int numPartitions = 4;
			const string specialCharacter = @"! * ' ( ) ; : @ & = + $ , \ ? % # [ ]";
			TopicName topicName = TopicName.get("persistent://my-property/my-ns/my-partitionedtopic1-" + DateTimeHelper.CurrentUnixTimeMillis() + specialCharacter);
			admin.topics().createPartitionedTopic(topicName.ToString(), numPartitions);

			// Try to create producer which does lookup and create connection with broker
			Producer<sbyte[]> producer = pulsarClient.newProducer().topic(topicName.ToString()).messageRoutingMode(MessageRoutingMode.RoundRobinPartition).create();
			producer.close();
			admin.topics().deletePartitionedTopic(topicName.ToString());
			log.info("-- Exiting {} test --", methodName);
		}


		public virtual void testCustomPartitionProducer()
		{
			PulsarClient pulsarClient = newPulsarClient(lookupUrl.ToString(), 0); // Creates new client connection
			TopicName topicName = null;
			Producer<sbyte[]> producer = null;
			Consumer<sbyte[]> consumer = null;
			const int MESSAGE_COUNT = 16;
			try
			{
				log.info("-- Starting {} test --", methodName);

				int numPartitions = 4;
				topicName = TopicName.get("persistent://my-property/my-ns/my-partitionedtopic1-" + DateTimeHelper.CurrentUnixTimeMillis());

				admin.topics().createPartitionedTopic(topicName.ToString(), numPartitions);

				producer = pulsarClient.newProducer().topic(topicName.ToString()).messageRouter(new AlwaysTwoMessageRouter(this)).create();

				consumer = pulsarClient.newConsumer().topic(topicName.ToString()).subscriptionName("my-partitioned-subscriber").subscribe();

				for (int i = 0; i < MESSAGE_COUNT; i++)
				{
					string message = "my-message-" + i;
					producer.newMessage().key(i.ToString()).value(message.GetBytes()).send();
				}

				Message<sbyte[]> msg;
				ISet<string> messageSet = Sets.newHashSet();

				for (int i = 0; i < MESSAGE_COUNT; i++)
				{
					msg = consumer.receive(5, TimeUnit.SECONDS);
					Assert.assertNotNull(msg, "Message should not be null");
					consumer.acknowledge(msg);
					string receivedMessage = new string(msg.Data);
					log.debug("Received message: [{}]", receivedMessage);
					string expectedMessage = "my-message-" + i;
					testMessageOrderAndDuplicates(messageSet, receivedMessage, expectedMessage);
				}
			}
			finally
			{
				producer.close();
				consumer.unsubscribe();
				consumer.close();
				pulsarClient.Dispose();
				admin.topics().deletePartitionedTopic(topicName.ToString());

				log.info("-- Exiting {} test --", methodName);
			}
		}


		public virtual void testSinglePartitionProducer()
		{
			log.info("-- Starting {} test --", methodName);
			PulsarClient pulsarClient = newPulsarClient(lookupUrl.ToString(), 0); // Creates new client connection

			int numPartitions = 4;
			TopicName topicName = TopicName.get("persistent://my-property/my-ns/my-partitionedtopic2-" + DateTimeHelper.CurrentUnixTimeMillis());

			admin.topics().createPartitionedTopic(topicName.ToString(), numPartitions);

			Producer<sbyte[]> producer = pulsarClient.newProducer().topic(topicName.ToString()).messageRoutingMode(MessageRoutingMode.SinglePartition).create();

			Consumer<sbyte[]> consumer = pulsarClient.newConsumer().topic(topicName.ToString()).subscriptionName("my-partitioned-subscriber").subscribe();

			for (int i = 0; i < 10; i++)
			{
				string message = "my-message-" + i;
				producer.send(message.GetBytes());
			}

			Message<sbyte[]> msg;
			ISet<string> messageSet = Sets.newHashSet();

			for (int i = 0; i < 10; i++)
			{
				msg = consumer.receive(5, TimeUnit.SECONDS);
				Assert.assertNotNull(msg, "Message should not be null");
				consumer.acknowledge(msg);
				string receivedMessage = new string(msg.Data);
				log.debug("Received message: [{}]", receivedMessage);
				string expectedMessage = "my-message-" + i;
				testMessageOrderAndDuplicates(messageSet, receivedMessage, expectedMessage);
			}

			producer.close();
			consumer.unsubscribe();
			consumer.close();
			pulsarClient.Dispose();
			admin.topics().deletePartitionedTopic(topicName.ToString());

			log.info("-- Exiting {} test --", methodName);
		}


		public virtual void testKeyBasedProducer()
		{
			log.info("-- Starting {} test --", methodName);
			PulsarClient pulsarClient = newPulsarClient(lookupUrl.ToString(), 0); // Creates new client connection

			int numPartitions = 4;
			TopicName topicName = TopicName.get("persistent://my-property/my-ns/my-partitionedtopic3-" + DateTimeHelper.CurrentUnixTimeMillis());
			string dummyKey1 = "dummykey1";
			string dummyKey2 = "dummykey2";

			admin.topics().createPartitionedTopic(topicName.ToString(), numPartitions);

			Producer<sbyte[]> producer = pulsarClient.newProducer().topic(topicName.ToString()).create();
			Consumer<sbyte[]> consumer = pulsarClient.newConsumer().topic(topicName.ToString()).subscriptionName("my-partitioned-subscriber").subscribe();

			for (int i = 0; i < 5; i++)
			{
				string message = "my-message-" + i;
				producer.newMessage().key(dummyKey1).value(message.GetBytes()).send();
			}
			for (int i = 5; i < 10; i++)
			{
				string message = "my-message-" + i;
				producer.newMessage().key(dummyKey2).value(message.GetBytes()).send();
			}

			ISet<string> messageSet = Sets.newHashSet();
			for (int i = 0; i < 10; i++)
			{
				Message<sbyte[]> msg = consumer.receive(5, TimeUnit.SECONDS);
				Assert.assertNotNull(msg, "Message should not be null");
				consumer.acknowledge(msg);
				string receivedMessage = new string(msg.Data);
				log.debug("Received message: [{}]", receivedMessage);
				testKeyBasedOrder(messageSet, receivedMessage);

			}

			producer.close();
			consumer.unsubscribe();
			consumer.close();
			pulsarClient.Dispose();
			admin.topics().deletePartitionedTopic(topicName.ToString());

			log.info("-- Exiting {} test --", methodName);
		}

		private void testKeyBasedOrder(ISet<string> messageSet, string message)
		{
			int index = int.Parse(message.Substring(message.LastIndexOf('-') + 1));
			if (index != 0 && index != 5)
			{
				Assert.assertTrue(messageSet.Contains("my-message-" + (index - 1)), "Message my-message-" + (index - 1) + " should come before my-message-" + index);
			}
			Assert.assertTrue(messageSet.Add(message), "Received duplicate message " + message);
		}


		public virtual void testPauseAndResume()
		{
			log.info("-- Starting {} test --", methodName);
			PulsarClient pulsarClient = newPulsarClient(lookupUrl.ToString(), 0); // Creates new client connection

			int numPartitions = 2;
			string topicName = TopicName.get("persistent://my-property/my-ns/my-partitionedtopic-pr-" + DateTimeHelper.CurrentUnixTimeMillis()).ToString();

			admin.topics().createPartitionedTopic(topicName, numPartitions);

			int receiverQueueSize = 20; // number of permits broker has per partition when consumer initially subscribes
			int numMessages = receiverQueueSize * numPartitions;

			AtomicReference<System.Threading.CountdownEvent> latch = new AtomicReference<System.Threading.CountdownEvent>(new System.Threading.CountdownEvent(numMessages));
			AtomicInteger received = new AtomicInteger();

			Consumer<sbyte[]> consumer = pulsarClient.newConsumer().receiverQueueSize(receiverQueueSize).topic(topicName).subscriptionName("my-partitioned-subscriber").messageListener((c1, msg) =>
			{
			Assert.assertNotNull(msg, "Message cannot be null");
			string receivedMessage = new string(msg.Data);
			log.debug("Received message [{}] in the listener", receivedMessage);
			c1.acknowledgeAsync(msg);
			received.incrementAndGet();
			latch.get().countDown();
			}).subscribe();

			Producer<sbyte[]> producer = pulsarClient.newProducer().topic(topicName).create();

			consumer.pause();

			for (int i = 0; i < numMessages * 2; i++)
			{
				producer.send(("my-message-" + i).GetBytes());
			}

			log.info("Waiting for message listener to ack " + numMessages + " messages");
			assertTrue(latch.get().await(numMessages, TimeUnit.SECONDS), "Timed out waiting for message listener acks");

			log.info("Giving message listener an opportunity to receive messages while paused");
			Thread.Sleep(2000); // hopefully this is long enough
			assertEquals(received.intValue(), numMessages, "Consumer received messages while paused");

			latch.set(new System.Threading.CountdownEvent(numMessages));

			consumer.resume();

			log.info("Waiting for message listener to ack all messages");
			assertTrue(latch.get().await(numMessages, TimeUnit.SECONDS), "Timed out waiting for message listener acks");

			consumer.close();
			producer.close();
			pulsarClient.Dispose();
			log.info("-- Exiting {} test --", methodName);
		}


		public virtual void testInvalidSequence()
		{
			log.info("-- Starting {} test --", methodName);

			int numPartitions = 4;
			TopicName topicName = TopicName.get("persistent://my-property/my-ns/my-partitionedtopic4-" + DateTimeHelper.CurrentUnixTimeMillis());
			admin.topics().createPartitionedTopic(topicName.ToString(), numPartitions);

			Consumer<sbyte[]> consumer = pulsarClient.newConsumer().topic(topicName.ToString()).subscriptionName("my-subscriber-name").subscribe();

			Producer<sbyte[]> producer = pulsarClient.newProducer().topic(topicName.ToString()).enableBatching(false).messageRoutingMode(MessageRoutingMode.SinglePartition).create();

			try
			{
				TypedMessageBuilderImpl<sbyte[]> mb = (TypedMessageBuilderImpl<sbyte[]>) producer.newMessage().value("InvalidMessage".GetBytes());
				consumer.acknowledge(mb.Message);
			}
			catch (PulsarClientException.InvalidMessageException)
			{
				// ok
			}

			consumer.close();

			try
			{
				consumer.receive();
				Assert.fail("Should fail");
			}
			catch (PulsarClientException.AlreadyClosedException)
			{
				// ok
			}

			try
			{
				consumer.unsubscribe();
				Assert.fail("Should fail");
			}
			catch (PulsarClientException.AlreadyClosedException)
			{
				// ok
			}


			producer.close();

			try
			{
				producer.send("message".GetBytes());
				Assert.fail("Should fail");
			}
			catch (PulsarClientException.AlreadyClosedException)
			{
				// ok
			}

			admin.topics().deletePartitionedTopic(topicName.ToString());

		}


		public virtual void testSillyUser()
		{

			int numPartitions = 4;
			TopicName topicName = TopicName.get("persistent://my-property/my-ns/my-partitionedtopic5-" + DateTimeHelper.CurrentUnixTimeMillis());
			admin.topics().createPartitionedTopic(topicName.ToString(), numPartitions);

			Producer<sbyte[]> producer = null;
			Consumer<sbyte[]> consumer = null;

			try
			{
				pulsarClient.newProducer().messageRouter(null);
				Assert.fail("should fail");
			}
			catch (System.NullReferenceException)
			{
				// ok
			}

			try
			{
				pulsarClient.newProducer().messageRoutingMode(null);
				Assert.fail("should fail");
			}
			catch (System.NullReferenceException)
			{
				// ok
			}

			try
			{
				producer = pulsarClient.newProducer().topic(topicName.ToString()).enableBatching(false).messageRoutingMode(MessageRoutingMode.SinglePartition).create();
				consumer = pulsarClient.newConsumer().topic(topicName.ToString()).subscriptionName("my-sub").subscribe();
				producer.send("message1".GetBytes());
				producer.send("message2".GetBytes());
				/* Message<byte[]> msg1 = */
				consumer.receive();
				Message<sbyte[]> msg2 = consumer.receive();
				consumer.acknowledgeCumulative(msg2);
			}
			finally
			{
				producer.close();
				consumer.unsubscribe();
				consumer.close();
			}

			admin.topics().deletePartitionedTopic(topicName.ToString());

		}

		public virtual void testDeletePartitionedTopic()
		{
			int numPartitions = 4;
			TopicName topicName = TopicName.get("persistent://my-property/my-ns/my-partitionedtopic6-" + DateTimeHelper.CurrentUnixTimeMillis());
			admin.topics().createPartitionedTopic(topicName.ToString(), numPartitions);

			Producer<sbyte[]> producer = pulsarClient.newProducer().topic(topicName.ToString()).create();
			Consumer<sbyte[]> consumer = pulsarClient.newConsumer().topic(topicName.ToString()).subscriptionName("my-sub").subscribe();
			consumer.unsubscribe();
			consumer.close();
			producer.close();

			admin.topics().deletePartitionedTopic(topicName.ToString());

			Producer<sbyte[]> producer1 = pulsarClient.newProducer().topic(topicName.ToString()).create();
			if (producer1 is PartitionedProducerImpl)
			{
				Assert.fail("should fail since partitioned topic was deleted");
			}
		}

		public virtual void testAsyncPartitionedProducerConsumer()
		{
			log.info("-- Starting {} test --", methodName);

			const int totalMsg = 100;

			ISet<string> produceMsgs = Sets.newHashSet();

			ISet<string> consumeMsgs = Sets.newHashSet();

			int numPartitions = 4;
			TopicName topicName = TopicName.get("persistent://my-property/my-ns/my-partitionedtopic1-" + DateTimeHelper.CurrentUnixTimeMillis());

			admin.topics().createPartitionedTopic(topicName.ToString(), numPartitions);
			Producer<sbyte[]> producer = pulsarClient.newProducer().topic(topicName.ToString()).enableBatching(false).messageRoutingMode(MessageRoutingMode.RoundRobinPartition).create();

			Consumer<sbyte[]> consumer = pulsarClient.newConsumer().topic(topicName.ToString()).subscriptionName("my-partitioned-subscriber").subscriptionType(SubscriptionType.Shared).subscribe();

			// produce messages
			for (int i = 0; i < totalMsg; i++)
			{
				string message = "my-message-" + i;
				produceMsgs.Add(message);
				producer.send(message.GetBytes());
			}

			log.info(" start receiving messages :");

			// receive messages
			System.Threading.CountdownEvent latch = new System.Threading.CountdownEvent(totalMsg);
			receiveAsync(consumer, totalMsg, 0, latch, consumeMsgs, executor);

			latch.await();

			// verify message produced correctly
			assertEquals(produceMsgs.Count, totalMsg);
			// verify produced and consumed messages must be exactly same

			produceMsgs.removeAll(consumeMsgs);
			assertTrue(produceMsgs.Count == 0);

			producer.close();
			consumer.unsubscribe();
			consumer.close();
			admin.topics().deletePartitionedTopic(topicName.ToString());

			log.info("-- Exiting {} test --", methodName);
		}


		public virtual void testAsyncPartitionedProducerConsumerQueueSizeOne()
		{
			log.info("-- Starting {} test --", methodName);
			PulsarClient pulsarClient = newPulsarClient(lookupUrl.ToString(), 0); // Creates new client connection

			const int totalMsg = 100;

			ISet<string> produceMsgs = Sets.newHashSet();

			ISet<string> consumeMsgs = Sets.newHashSet();

			int numPartitions = 4;
			TopicName topicName = TopicName.get("persistent://my-property/my-ns/my-partitionedtopic1-" + DateTimeHelper.CurrentUnixTimeMillis());

			admin.topics().createPartitionedTopic(topicName.ToString(), numPartitions);

			Producer<sbyte[]> producer = pulsarClient.newProducer().topic(topicName.ToString()).enableBatching(false).messageRoutingMode(MessageRoutingMode.RoundRobinPartition).create();

			Consumer<sbyte[]> consumer = pulsarClient.newConsumer().topic(topicName.ToString()).subscriptionName("my-partitioned-subscriber").receiverQueueSize(1).subscribe();

			// produce messages
			for (int i = 0; i < totalMsg; i++)
			{
				string message = "my-message-" + i;
				produceMsgs.Add(message);
				producer.send(message.GetBytes());
			}

			log.info(" start receiving messages :");

			// receive messages
			System.Threading.CountdownEvent latch = new System.Threading.CountdownEvent(totalMsg);
			receiveAsync(consumer, totalMsg, 0, latch, consumeMsgs, executor);

			latch.await();

			// verify message produced correctly
			assertEquals(produceMsgs.Count, totalMsg);
			// verify produced and consumed messages must be exactly same

			produceMsgs.removeAll(consumeMsgs);
			assertTrue(produceMsgs.Count == 0);

			producer.close();
			consumer.unsubscribe();
			consumer.close();
			pulsarClient.Dispose();
			admin.topics().deletePartitionedTopic(topicName.ToString());

			log.info("-- Exiting {} test --", methodName);
		}

		/// <summary>
		/// It verifies that consumer consumes from all the partitions fairly.
		/// </summary>
		/// <exception cref="Exception"> </exception>
		/// 
		public virtual void testFairDistributionForPartitionConsumers()
		{
			log.info("-- Starting {} test --", methodName);
			PulsarClient pulsarClient = newPulsarClient(lookupUrl.ToString(), 0); // Creates new client connection

			const int numPartitions = 2;

			string topicName = "persistent://my-property/my-ns/my-topic-" + DateTimeHelper.CurrentUnixTimeMillis();
			const string producer1Msg = "producer1";
			const string producer2Msg = "producer2";
			const int queueSize = 10;

			admin.topics().createPartitionedTopic(topicName, numPartitions);

			Producer<sbyte[]> producer1 = pulsarClient.newProducer().topic(topicName + "-partition-0").messageRoutingMode(MessageRoutingMode.RoundRobinPartition).create();
			Producer<sbyte[]> producer2 = pulsarClient.newProducer().topic(topicName + "-partition-1").messageRoutingMode(MessageRoutingMode.RoundRobinPartition).create();

			Consumer<sbyte[]> consumer = pulsarClient.newConsumer().topic(topicName).subscriptionName("my-partitioned-subscriber").receiverQueueSize(queueSize).subscribe();

			int partition2Msgs = 0;

			// produce messages on Partition-1: which will makes partitioned-consumer's queue full
			for (int i = 0; i < queueSize - 1; i++)
			{
				producer1.send((producer1Msg + "-" + i).GetBytes());
			}

			Thread.Sleep(1000);

			// now queue is full : so, partition-2 consumer will be pushed to paused-consumer list
			for (int i = 0; i < 5; i++)
			{
				producer2.send((producer2Msg + "-" + i).GetBytes());
			}

			// now, Queue should take both partition's messages
			// also: we will keep producing messages to partition-1
			int produceMsgInPartition1AfterNumberOfConsumeMessages = 2;
			for (int i = 0; i < 3 * queueSize; i++)
			{
				Message<sbyte[]> msg = consumer.receive();
				partition2Msgs += (new string(msg.Data)).StartsWith(producer2Msg, StringComparison.Ordinal) ? 1 : 0;
				if (i >= produceMsgInPartition1AfterNumberOfConsumeMessages)
				{
					producer1.send(producer1Msg.GetBytes());
					Thread.Sleep(100);
				}

			}

			assertTrue(partition2Msgs >= 4);
			producer1.close();
			producer2.close();
			consumer.unsubscribe();
			consumer.close();
			pulsarClient.Dispose();
			admin.topics().deletePartitionedTopic(topicName);

			log.info("-- Exiting {} test --", methodName);
		}

        private void receiveAsync(Consumer<sbyte[]> consumer, int totalMessage, int currentMessage, System.Threading.CountdownEvent latch, in ISet<string> consumeMsg, ExecutorService executor)
		{
			if (currentMessage < totalMessage)
			{
				CompletableFuture<Message<sbyte[]>> future = consumer.receiveAsync();
				future.handle((msg, exception) =>
				{
				if (exception == null)
				{
					consumeMsg.Add(new string(msg.Data));
					try
					{
						consumer.acknowledge(msg);
					}
					catch (PulsarClientException e1)
					{
						fail("message acknowledge failed", e1);
					}
					executor.execute(() =>
					{
						try
						{
							receiveAsync(consumer, totalMessage, currentMessage + 1, latch, consumeMsg, executor);
						}
						catch (PulsarClientException e)
						{
							fail("message receive failed", e);
						}
					});
					latch.Signal();
				}
				return null;
				});
			}
		}


		public virtual void testGetPartitionsForTopic()
		{
			int numPartitions = 4;
			string topic = "persistent://my-property/my-ns/my-partitionedtopic1-" + DateTimeHelper.CurrentUnixTimeMillis();

			admin.topics().createPartitionedTopic(topic, numPartitions);

			IList<string> expectedPartitions = new List<string>();
			for (int i = 0; i < numPartitions; i++)
			{
				expectedPartitions.Add(topic + "-partition-" + i);
			}

			assertEquals(pulsarClient.getPartitionsForTopic(topic).join(), expectedPartitions);

			string nonPartitionedTopic = "persistent://my-property/my-ns/my-non-partitionedtopic1";

			assertEquals(pulsarClient.getPartitionsForTopic(nonPartitionedTopic).join(), Collections.singletonList(nonPartitionedTopic));
		}


		public virtual void testMessageIdForSubscribeToSinglePartition()
		{
			PulsarClient pulsarClient = newPulsarClient(lookupUrl.ToString(), 0); // Creates new client connection
			TopicName topicName = null;
			TopicName partition2TopicName = null;
			Producer<sbyte[]> producer = null;
			Consumer<sbyte[]> consumer1 = null;
			Consumer<sbyte[]> consumer2 = null;
			const int numPartitions = 4;
			const int totalMessages = 30;

			try
			{
				log.info("-- Starting {} test --", methodName);

				topicName = TopicName.get("persistent://my-property/my-ns/my-topic-" + DateTimeHelper.CurrentUnixTimeMillis());
				partition2TopicName = topicName.getPartition(2);

				admin.topics().createPartitionedTopic(topicName.ToString(), numPartitions);

				producer = pulsarClient.newProducer().topic(topicName.ToString()).messageRouter(new AlwaysTwoMessageRouter(this)).create();

				consumer1 = pulsarClient.newConsumer().topic(topicName.ToString()).subscriptionName("subscriber-partitioned").subscriptionInitialPosition(SubscriptionInitialPosition.Earliest).subscriptionType(SubscriptionType.Exclusive).subscribe();

				consumer2 = pulsarClient.newConsumer().topic(partition2TopicName.ToString()).subscriptionName("subscriber-single").subscriptionInitialPosition(SubscriptionInitialPosition.Earliest).subscriptionType(SubscriptionType.Exclusive).subscribe();

				for (int i = 0; i < totalMessages; i++)
				{
					string message = "my-message-" + i;
					producer.newMessage().key(i.ToString()).value(message.GetBytes()).send();
				}
				producer.flush();

				Message<sbyte[]> msg;

				for (int i = 0; i < totalMessages; i++)
				{
					msg = consumer1.receive(5, TimeUnit.SECONDS);
					Assert.assertEquals(((MessageIdImpl)((TopicMessageIdImpl)msg.MessageId).InnerMessageId).PartitionIndex, 2);
					consumer1.acknowledge(msg);
				}

				for (int i = 0; i < totalMessages; i++)
				{
					msg = consumer2.receive(5, TimeUnit.SECONDS);
					Assert.assertEquals(((MessageIdImpl)msg.MessageId).PartitionIndex, 2);
					consumer2.acknowledge(msg);
				}

			}
			finally
			{
				producer.close();
				consumer1.unsubscribe();
				consumer1.close();
				consumer2.unsubscribe();
				consumer2.close();
				pulsarClient.Dispose();
				admin.topics().deletePartitionedTopic(topicName.ToString());

				log.info("-- Exiting {} test --", methodName);
			}
		}


		/// <summary>
		/// It verifies that consumer producer auto update for partitions extend.
		/// 
		/// Steps:
		/// 1. create topic with 2 partitions, and producer consumer
		/// 2. update partition from 2 to 3.
		/// 3. trigger auto update in producer, after produce, consumer will only get messages from 2 partitions.
		/// 4. trigger auto update in consumer, after produce, consumer will get all messages from 3 partitions.
		/// </summary>
		/// <exception cref="Exception"> </exception>
		/// 
		public virtual void testAutoUpdatePartitionsForProducerConsumer()
		{
			log.info("-- Starting {} test --", methodName);
			PulsarClient pulsarClient = newPulsarClient(lookupUrl.ToString(), 0); // Creates new client connection

			const int numPartitions = 2;

			string topicName = "persistent://my-property/my-ns/my-topic-" + DateTimeHelper.CurrentUnixTimeMillis();
			const string producerMsg = "producerMsg";
			const int totalMessages = 30;

			admin.topics().createPartitionedTopic(topicName, numPartitions);

			Producer<sbyte[]> producer = pulsarClient.newProducer().topic(topicName).messageRoutingMode(MessageRoutingMode.RoundRobinPartition).enableBatching(false).autoUpdatePartitions(true).create();

			Consumer<sbyte[]> consumer = pulsarClient.newConsumer().topic(topicName).subscriptionName("my-partitioned-subscriber").subscriptionType(SubscriptionType.Shared).autoUpdatePartitions(true).subscribe();

			// 1. produce and consume 2 partitions
			for (int i = 0; i < totalMessages; i++)
			{
				producer.send((producerMsg + " first round " + "message index: " + i).GetBytes());
			}
			int messageSet = 0;
			Message<sbyte[]> message = consumer.receive();
			do
			{
				messageSet++;
				consumer.acknowledge(message);
				log.info("Consumer acknowledged : " + new string(message.Data));
				message = consumer.receive(200, TimeUnit.MILLISECONDS);
			} while (message != null);
			assertEquals(messageSet, totalMessages);

			// 2. update partition from 2 to 3.
			admin.topics().updatePartitionedTopic(topicName,3);

			// 3. trigger auto update in producer, after produce, consumer will get 2/3 messages.
			log.info("trigger partitionsAutoUpdateTimerTask for producer");
			Timeout timeout = ((PartitionedProducerImpl<sbyte[]>)producer).PartitionsAutoUpdateTimeout;
			timeout.task().run(timeout);
			Thread.Sleep(200);

			for (int i = 0; i < totalMessages; i++)
			{
				producer.send((producerMsg + " second round " + "message index: " + i).GetBytes());
			}
			messageSet = 0;
			message = consumer.receive();
			do
			{
				messageSet++;
				consumer.acknowledge(message);
				log.info("Consumer acknowledged : " + new string(message.Data));
				message = consumer.receive(200, TimeUnit.MILLISECONDS);
			} while (message != null);
			assertEquals(messageSet, totalMessages * 2 / 3);

			// 4. trigger auto update in consumer, after produce, consumer will get all messages.
			log.info("trigger partitionsAutoUpdateTimerTask for consumer");
			timeout = ((MultiTopicsConsumerImpl<sbyte[]>)consumer).PartitionsAutoUpdateTimeout;
			timeout.task().run(timeout);
			Thread.Sleep(200);

			// former produced messages
			messageSet = 0;
			message = consumer.receive();
			do
			{
				messageSet++;
				consumer.acknowledge(message);
				log.info("Consumer acknowledged : " + new string(message.Data));
				message = consumer.receive(200, TimeUnit.MILLISECONDS);
			} while (message != null);
			assertEquals(messageSet, totalMessages / 3);

			// former produced messages
			for (int i = 0; i < totalMessages; i++)
			{
				producer.send((producerMsg + " third round " + "message index: " + i).GetBytes());
			}
			messageSet = 0;
			message = consumer.receive();
			do
			{
				messageSet++;
				consumer.acknowledge(message);
				log.info("Consumer acknowledged : " + new string(message.Data));
				message = consumer.receive(200, TimeUnit.MILLISECONDS);
			} while (message != null);
			assertEquals(messageSet, totalMessages);

			pulsarClient.Dispose();
			admin.topics().deletePartitionedTopic(topicName);

			log.info("-- Exiting {} test --", methodName);
		}


		[Serializable]
		public class AlwaysTwoMessageRouter : MessageRouter
		{
			private readonly PartitionedProducerConsumerTest outerInstance;

			public AlwaysTwoMessageRouter(PartitionedProducerConsumerTest outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public virtual int choosePartition<T1>(Message<T1> msg, TopicMetadata metadata)
			{
				return 2;
			}
		}
	}

}