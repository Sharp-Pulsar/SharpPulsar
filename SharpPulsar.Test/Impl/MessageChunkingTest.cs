using System;
using System.Collections.Generic;
using System.Text;
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


    public class MessageChunkingTest : ProducerConsumerBase
	{
		private static readonly Logger log = LoggerFactory.getLogger(typeof(MessageChunkingTest));


		public override void setup()
		{
			base.internalSetup();
			base.ProducerBaseSetup();
		}


		public override void cleanup()
		{
			base.internalCleanup();
		}


		public virtual void testInvalidConfig()
		{
			const string topicName = "persistent://my-property/my-ns/my-topic1";
			ProducerBuilder<sbyte[]> producerBuilder = pulsarClient.newProducer().topic(topicName);
			// batching and chunking can't be enabled together
			try
			{
				Producer<sbyte[]> producer = producerBuilder.enableChunking(true).enableBatching(true).create();
				fail("producer creation should have fail");
			}
			catch (System.ArgumentException)
			{
				// Ok
			}
		}


		public virtual void testLargeMessage()
		{

			log.info("-- Starting {} test --", methodName);
			this.conf.MaxMessageSize = 5;
			const int totalMessages = 5;
			const string topicName = "persistent://my-property/my-ns/my-topic1";

			Consumer<sbyte[]> consumer = pulsarClient.newConsumer().topic(topicName).subscriptionName("my-subscriber-name").acknowledgmentGroupTime(0, TimeUnit.SECONDS).subscribe();

			ProducerBuilder<sbyte[]> producerBuilder = pulsarClient.newProducer().topic(topicName);

			Producer<sbyte[]> producer = producerBuilder.compressionType(CompressionType.LZ4).enableChunking(true).enableBatching(false).create();

			PersistentTopic topic = (PersistentTopic) pulsar.BrokerService.getTopicIfExists(topicName).get().get();

			IList<string> publishedMessages = Lists.newArrayList();
			for (int i = 0; i < totalMessages; i++)
			{
				string message = createMessagePayload(i * 10);
				publishedMessages.Add(message);
				producer.send(message.GetBytes());
			}

			Message<sbyte[]> msg = null;
			ISet<string> messageSet = Sets.newHashSet();
			IList<MessageId> msgIds = Lists.newArrayList();
			for (int i = 0; i < totalMessages; i++)
			{
				msg = consumer.receive(5, TimeUnit.SECONDS);
				string receivedMessage = new string(msg.Data);
				log.info("[{}] - Published [{}] Received message: [{}]", i, publishedMessages[i], receivedMessage);
				string expectedMessage = publishedMessages[i];
				testMessageOrderAndDuplicates(messageSet, receivedMessage, expectedMessage);
				msgIds.Add(msg.MessageId);
			}

			pulsar.BrokerService.updateRates();

			PublisherStats producerStats = topic.getStats(false).publishers[0];

			assertTrue(producerStats.chunkedMessageRate > 0);

			ManagedCursorImpl mcursor = (ManagedCursorImpl) topic.ManagedLedger.Cursors.GetEnumerator().next();
			PositionImpl readPosition = (PositionImpl) mcursor.ReadPosition;

			foreach (MessageId msgId in msgIds)
			{
				consumer.acknowledge(msgId);
			}

			retryStrategically((test) =>
			{
			return mcursor.MarkDeletedPosition.Next.Equals(readPosition);
			}, 5, 200);

			assertEquals(readPosition, mcursor.MarkDeletedPosition.Next);

			assertEquals(readPosition.EntryId, ((ConsumerImpl) consumer).AvailablePermits);

			consumer.close();
			producer.close();
			log.info("-- Exiting {} test --", methodName);

		}


		public virtual void testLargeMessageAckTimeOut()
		{

			log.info("-- Starting {} test --", methodName);
			this.conf.MaxMessageSize = 5;
			const int totalMessages = 5;
			const string topicName = "persistent://my-property/my-ns/my-topic1";

			ConsumerImpl<sbyte[]> consumer = (ConsumerImpl<sbyte[]>) pulsarClient.newConsumer().topic(topicName).subscriptionName("my-subscriber-name").acknowledgmentGroupTime(0, TimeUnit.SECONDS).ackTimeout(5, TimeUnit.SECONDS).subscribe();

			ProducerBuilder<sbyte[]> producerBuilder = pulsarClient.newProducer().topic(topicName);

			Producer<sbyte[]> producer = producerBuilder.enableChunking(true).enableBatching(false).create();

			PersistentTopic topic = (PersistentTopic) pulsar.BrokerService.getTopicIfExists(topicName).get().get();

			IList<string> publishedMessages = Lists.newArrayList();
			for (int i = 0; i < totalMessages; i++)
			{
				string message = createMessagePayload(i * 10);
				publishedMessages.Add(message);
				producer.send(message.GetBytes());
			}

			Message<sbyte[]> msg = null;
			ISet<string> messageSet = Sets.newHashSet();
			for (int i = 0; i < totalMessages; i++)
			{
				msg = consumer.receive(5, TimeUnit.SECONDS);
				string receivedMessage = new string(msg.Data);
				log.info("Received message: [{}]", receivedMessage);
				string expectedMessage = publishedMessages[i];
				testMessageOrderAndDuplicates(messageSet, receivedMessage, expectedMessage);
			}

			retryStrategically((test) => consumer.UnAckedMessageTracker.messageIdPartitionMap.IsEmpty, 10, TimeUnit.SECONDS.toMillis(1));

			msg = null;
			messageSet.Clear();
			MessageId lastMsgId = null;
			for (int i = 0; i < totalMessages; i++)
			{
				msg = consumer.receive(5, TimeUnit.SECONDS);
				lastMsgId = msg.MessageId;
				string receivedMessage = new string(msg.Data);
				log.info("Received message: [{}]", receivedMessage);
				string expectedMessage = publishedMessages[i];
				testMessageOrderAndDuplicates(messageSet, receivedMessage, expectedMessage);
			}

			ManagedCursorImpl mcursor = (ManagedCursorImpl) topic.ManagedLedger.Cursors.GetEnumerator().next();
			PositionImpl readPosition = (PositionImpl) mcursor.ReadPosition;

			consumer.acknowledgeCumulative(lastMsgId);

			retryStrategically((test) =>
			{
			return mcursor.MarkDeletedPosition.Next.Equals(readPosition);
			}, 5, 200);

			assertEquals(readPosition, mcursor.MarkDeletedPosition.Next);

			consumer.close();
			producer.close();
			log.info("-- Exiting {} test --", methodName);

		}


		public virtual void testPublishWithFailure()
		{
			log.info("-- Starting {} test --", methodName);
			this.conf.MaxMessageSize = 5;
			const string topicName = "persistent://my-property/my-ns/my-topic1";

			ProducerBuilder<sbyte[]> producerBuilder = pulsarClient.newProducer().topic(topicName);

			Producer<sbyte[]> producer = producerBuilder.enableChunking(true).enableBatching(false).create();

			stopBroker();

			try
			{
				producer.send(createMessagePayload(100).GetBytes());
				fail("should have failed with timeout exception");
			}
			catch (PulsarClientException.TimeoutException)
			{
				// Ok
			}
			producer.close();
		}


		public virtual void testMaxPendingChunkMessages()
		{

			log.info("-- Starting {} test --", methodName);
			this.conf.MaxMessageSize = 10;
			const int totalMessages = 25;
			const string topicName = "persistent://my-property/my-ns/maxPending";
			const int totalProducers = 25;
			ExecutorService executor = Executors.newFixedThreadPool(totalProducers);

			try
			{
				ConsumerImpl<sbyte[]> consumer = (ConsumerImpl<sbyte[]>) pulsarClient.newConsumer().topic(topicName).subscriptionName("my-subscriber-name").acknowledgmentGroupTime(0, TimeUnit.SECONDS).maxPendingChuckedMessage(1).autoAckOldestChunkedMessageOnQueueFull(true).ackTimeout(5, TimeUnit.SECONDS).subscribe();

				ProducerBuilder<sbyte[]> producerBuilder = pulsarClient.newProducer().topic(topicName);

				Producer<sbyte[]>[] producers = new Producer[totalProducers];
				int totalPublishedMessages = totalProducers;
				IList<CompletableFuture<MessageId>> futures = Lists.newArrayList();
				for (int i = 0; i < totalProducers; i++)
				{
					producers[i] = producerBuilder.enableChunking(true).enableBatching(false).create();
					int index = i;
					executor.submit(() =>
					{
					futures.Add(producers[index].sendAsync(createMessagePayload(45).GetBytes()));
					});
				}

				FutureUtil.waitForAll(futures).get();
				PersistentTopic topic = (PersistentTopic) pulsar.BrokerService.getTopicIfExists(topicName).get().get();

				Message<sbyte[]> msg = null;
				ISet<string> messageSet = Sets.newHashSet();
				for (int i = 0; i < totalMessages; i++)
				{
					msg = consumer.receive(1, TimeUnit.SECONDS);
					if (msg == null)
					{
						break;
					}
					string receivedMessage = new string(msg.Data);
					log.info("Received message: [{}]", receivedMessage);
					messageSet.Add(receivedMessage);
					consumer.acknowledge(msg);
				}

				assertNotEquals(messageSet.Count, totalPublishedMessages);
			}
			finally
			{
				executor.shutdown();
			}

		}

		/// <summary>
		/// Validate that chunking is not supported with batching and non-persistent topic
		/// </summary>
		/// <exception cref="Exception"> </exception>
		/// 
		public virtual void testInvalidUseCaseForChunking()
		{

			log.info("-- Starting {} test --", methodName);
			this.conf.MaxMessageSize = 5;
			const string topicName = "persistent://my-property/my-ns/my-topic1";

			ProducerBuilder<sbyte[]> producerBuilder = pulsarClient.newProducer().topic(topicName);

			try
			{
				Producer<sbyte[]> producer = producerBuilder.enableChunking(true).enableBatching(true).create();
				fail("it should have failied because chunking can't be used with batching enabled");
			}
			catch (System.ArgumentException)
			{
				// Ok
			}

			log.info("-- Exiting {} test --", methodName);
		}


		public virtual void testExpireIncompleteChunkMessage()
		{
			const string topicName = "persistent://prop/use/ns-abc/expireMsg";

			// 1. producer connect
			ProducerImpl<sbyte[]> producer = (ProducerImpl<sbyte[]>) pulsarClient.newProducer().topic(topicName).enableBatching(false).messageRoutingMode(MessageRoutingMode.SinglePartition).sendTimeout(10, TimeUnit.MINUTES).create();
			System.Reflection.FieldInfo producerIdField = typeof(ProducerImpl).getDeclaredField("producerId");
			producerIdField.Accessible = true;
			long producerId = (long) producerIdField.get(producer);
			producer.cnx().registerProducer(producerId, producer); // registered spy ProducerImpl
			ConsumerImpl<sbyte[]> consumer = (ConsumerImpl<sbyte[]>) pulsarClient.newConsumer().topic(topicName).subscriptionName("my-sub").subscribe();

			TypedMessageBuilderImpl<sbyte[]> msg = (TypedMessageBuilderImpl<sbyte[]>) producer.newMessage().value("message-1".GetBytes());
			ByteBuf payload = Unpooled.wrappedBuffer(msg.Content);
			MessageMetadata.Builder metadataBuilder = ((TypedMessageBuilderImpl<sbyte[]>) msg).MetadataBuilder;
			MessageMetadata msgMetadata = metadataBuilder.setProducerName("test").setSequenceId(1).setPublishTime(10L).setUuid("123").setNumChunksFromMsg(2).setChunkId(0).setTotalChunkMsgSize(100).build();
			ByteBufPair cmd = Commands.newSend(producerId, 1, 1, Commands.ChecksumType.Crc32c, msgMetadata, payload);
			MessageImpl msgImpl = ((MessageImpl<sbyte[]>) msg.Message);
			msgImpl.setSchemaState(SchemaState.Ready);
			OpSendMsg op = OpSendMsg.create(msgImpl, cmd, 1, null);
			producer.processOpSendMsg(op);

			retryStrategically((test) =>
			{
			return consumer.chunkedMessagesMap.size() > 0;
			}, 5, 500);
			assertEquals(consumer.chunkedMessagesMap.size(), 1);

			consumer.expireTimeOfIncompleteChunkedMessageMillis = 1;
			Thread.Sleep(10);
			consumer.removeExpireIncompleteChunkedMessages();
			assertEquals(consumer.chunkedMessagesMap.size(), 0);

			producer.close();
			consumer.close();
			producer = null; // clean reference of mocked producer
		}

		private string createMessagePayload(int size)
		{
			StringBuilder str = new StringBuilder();
			Random rand = new Random();
			for (int i = 0; i < size; i++)
			{
				str.Append(rand.Next(10));
			}
			return str.ToString();
		}

	}

}