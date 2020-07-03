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



public class TopicReaderTest : ProducerConsumerBase
	{
		private static readonly Logger log = LoggerFactory.getLogger(typeof(TopicReaderTest));


		public override void setup()
		{
			base.internalSetup();
			base.producerBaseSetup();
		}


		public override void cleanup()
		{
			base.internalCleanup();
		}


		public static object[][] variationsForExpectedPos()
		{
			return new object[][]
			{
				new object[] {true, true, 10},
				new object[] {true, false, 10},
				new object[] {false, true, 10},
				new object[] {false, false, 10},
				new object[] {true, true, 100},
				new object[] {true, false, 100},
				new object[] {false, true, 100},
				new object[] {false, false, 100}
			};
		}


		public static object[][] variationsForResetOnLatestMsg()
		{
			return new object[][]
			{
				new object[] {true, 20},
				new object[] {false, 20}
			};
		}


		public static object[][] variationsForHasMessageAvailable()
		{
			return new object[][]
			{
				new object[] {true, true},
				new object[] {true, false},
				new object[] {false, true},
				new object[] {false, false}
			};
		}


		public virtual void testSimpleReader()
		{
			Reader<sbyte[]> reader = pulsarClient.newReader().topic("persistent://my-property/my-ns/testSimpleReader").startMessageId(MessageId_Fields.earliest).create();

			Producer<sbyte[]> producer = pulsarClient.newProducer().topic("persistent://my-property/my-ns/testSimpleReader").create();
			for (int i = 0; i < 10; i++)
			{
				string message = "my-message-" + i;
				producer.send(message.GetBytes());
			}

			Message<sbyte[]> msg = null;
			ISet<string> messageSet = Sets.newHashSet();
			for (int i = 0; i < 10; i++)
			{
				msg = reader.readNext(1, TimeUnit.SECONDS);

				string receivedMessage = new string(msg.Data);
				log.debug("Received message: [{}]", receivedMessage);
				string expectedMessage = "my-message-" + i;
				testMessageOrderAndDuplicates(messageSet, receivedMessage, expectedMessage);
			}

			// Acknowledge the consumption of all messages at once
			reader.close();
			producer.close();
		}


		public virtual void testReaderAfterMessagesWerePublished()
		{
			Producer<sbyte[]> producer = pulsarClient.newProducer().topic("persistent://my-property/my-ns/testReaderAfterMessagesWerePublished").create();
			for (int i = 0; i < 10; i++)
			{
				string message = "my-message-" + i;
				producer.send(message.GetBytes());
			}

			Reader<sbyte[]> reader = pulsarClient.newReader().topic("persistent://my-property/my-ns/testReaderAfterMessagesWerePublished").startMessageId(MessageId_Fields.earliest).create();

			Message<sbyte[]> msg = null;
			ISet<string> messageSet = Sets.newHashSet();
			for (int i = 0; i < 10; i++)
			{
				msg = reader.readNext(1, TimeUnit.SECONDS);

				string receivedMessage = new string(msg.Data);
				log.debug("Received message: [{}]", receivedMessage);
				string expectedMessage = "my-message-" + i;
				testMessageOrderAndDuplicates(messageSet, receivedMessage, expectedMessage);
			}

			// Acknowledge the consumption of all messages at once
			reader.close();
			producer.close();
		}


		public virtual void testMultipleReaders()
		{
			Producer<sbyte[]> producer = pulsarClient.newProducer().topic("persistent://my-property/my-ns/testMultipleReaders").create();
			for (int i = 0; i < 10; i++)
			{
				string message = "my-message-" + i;
				producer.send(message.GetBytes());
			}

			Reader<sbyte[]> reader1 = pulsarClient.newReader().topic("persistent://my-property/my-ns/testMultipleReaders").startMessageId(MessageId_Fields.earliest).create();

			Reader<sbyte[]> reader2 = pulsarClient.newReader().topic("persistent://my-property/my-ns/testMultipleReaders").startMessageId(MessageId_Fields.earliest).create();

			Message<sbyte[]> msg = null;
			ISet<string> messageSet1 = Sets.newHashSet();
			for (int i = 0; i < 10; i++)
			{
				msg = reader1.readNext(1, TimeUnit.SECONDS);

				string receivedMessage = new string(msg.Data);
				log.debug("Received message: [{}]", receivedMessage);
				string expectedMessage = "my-message-" + i;
				testMessageOrderAndDuplicates(messageSet1, receivedMessage, expectedMessage);
			}

			ISet<string> messageSet2 = Sets.newHashSet();
			for (int i = 0; i < 10; i++)
			{
				msg = reader2.readNext(1, TimeUnit.SECONDS);

				string receivedMessage = new string(msg.Data);
				log.debug("Received message: [{}]", receivedMessage);
				string expectedMessage = "my-message-" + i;
				testMessageOrderAndDuplicates(messageSet2, receivedMessage, expectedMessage);
			}

			reader1.close();
			reader2.close();
			producer.close();
		}

		public virtual void testTopicStats()
		{
			string topicName = "persistent://my-property/my-ns/testTopicStats";

			Reader<sbyte[]> reader1 = pulsarClient.newReader().topic(topicName).startMessageId(MessageId_Fields.earliest).create();

			Reader<sbyte[]> reader2 = pulsarClient.newReader().topic(topicName).startMessageId(MessageId_Fields.earliest).create();

			TopicStats stats = admin.topics().getStats(topicName);
			assertEquals(stats.subscriptions.Count, 2);

			reader1.close();
			stats = admin.topics().getStats(topicName);
			assertEquals(stats.subscriptions.Count, 1);

			reader2.close();

			stats = admin.topics().getStats(topicName);
			assertEquals(stats.subscriptions.Count, 0);
		}


		public virtual void testReaderOnLatestMessage(bool startInclusive, int numOfMessages)
		{
			const string topicName = "persistent://my-property/my-ns/ReaderOnLatestMessage";

			int halfOfMsgs = numOfMessages / 2;

			Producer<sbyte[]> producer = pulsarClient.newProducer().topic(topicName).create();

			for (int i = 0; i < halfOfMsgs; i++)
			{
				producer.send(string.Format("my-message-{0:D}", i).Bytes);
			}

			ReaderBuilder<sbyte[]> readerBuilder = pulsarClient.newReader().topic(topicName).startMessageId(MessageId_Fields.latest);

			if (startInclusive)
			{
				readerBuilder.startMessageIdInclusive();
			}

			Reader<sbyte[]> reader = readerBuilder.create();

			for (int i = halfOfMsgs; i < numOfMessages; i++)
			{
				producer.send(string.Format("my-message-{0:D}", i).Bytes);
			}

			// Publish more messages and verify the readers only sees new messages
			ISet<string> messageSet = Sets.newHashSet();
			for (int i = halfOfMsgs; i < numOfMessages; i++)
			{
				Message<sbyte[]> message = reader.readNext();
				string receivedMessage = new string(message.Data);
				string expectedMessage = string.Format("my-message-{0:D}", i);
				testMessageOrderAndDuplicates(messageSet, receivedMessage, expectedMessage);
			}

			assertTrue(reader.Connected);
			assertEquals(((ReaderImpl) reader).Consumer.numMessagesInQueue(), 0);
			assertEquals(messageSet.Count, halfOfMsgs);

			// Acknowledge the consumption of all messages at once
			reader.close();
			producer.close();
		}


		public virtual void testReaderOnSpecificMessage()
		{
			Producer<sbyte[]> producer = pulsarClient.newProducer().topic("persistent://my-property/my-ns/testReaderOnSpecificMessage").create();
			IList<MessageId> messageIds = new List<MessageId>();
			for (int i = 0; i < 10; i++)
			{
				string message = "my-message-" + i;
				messageIds.Add(producer.send(message.GetBytes()));
			}

			Reader<sbyte[]> reader = pulsarClient.newReader().topic("persistent://my-property/my-ns/testReaderOnSpecificMessage").startMessageId(messageIds[4]).create();

			// Publish more messages and verify the readers only sees messages starting from the intended message
			Message<sbyte[]> msg = null;
			ISet<string> messageSet = Sets.newHashSet();
			for (int i = 5; i < 10; i++)
			{
				msg = reader.readNext(1, TimeUnit.SECONDS);

				string receivedMessage = new string(msg.Data);
				log.debug("Received message: [{}]", receivedMessage);
				string expectedMessage = "my-message-" + i;
				testMessageOrderAndDuplicates(messageSet, receivedMessage, expectedMessage);
			}

			// Acknowledge the consumption of all messages at once
			reader.close();
			producer.close();
		}


		public virtual void testReaderOnSpecificMessageWithBatches()
		{
			Producer<sbyte[]> producer = pulsarClient.newProducer().topic("persistent://my-property/my-ns/testReaderOnSpecificMessageWithBatches").enableBatching(true).batchingMaxPublishDelay(100, TimeUnit.MILLISECONDS).create();
			for (int i = 0; i < 10; i++)
			{
				string message = "my-message-" + i;
				producer.sendAsync(message.GetBytes());
			}

			// Write one sync message to ensure everything before got persistend
			producer.send("my-message-10".GetBytes());
			Reader<sbyte[]> reader1 = pulsarClient.newReader().topic("persistent://my-property/my-ns/testReaderOnSpecificMessageWithBatches").startMessageId(MessageId_Fields.earliest).create();

			MessageId lastMessageId = null;
			for (int i = 0; i < 5; i++)
			{
				Message<sbyte[]> msg = reader1.readNext();
				lastMessageId = msg.MessageId;
			}

			assertEquals(lastMessageId.GetType(), typeof(BatchMessageIdImpl));

			Console.WriteLine("CREATING READER ON MSG ID: " + lastMessageId);

			Reader<sbyte[]> reader2 = pulsarClient.newReader().topic("persistent://my-property/my-ns/testReaderOnSpecificMessageWithBatches").startMessageId(lastMessageId).create();

			for (int i = 5; i < 11; i++)
			{
				Message<sbyte[]> msg = reader2.readNext(1, TimeUnit.SECONDS);

				string receivedMessage = new string(msg.Data);
				log.debug("Received message: [{}]", receivedMessage);
				string expectedMessage = "my-message-" + i;
				assertEquals(receivedMessage, expectedMessage);
			}

			producer.close();
		}


		public virtual void testECDSAEncryption()
		{
			log.info("-- Starting {} test --", methodName);


//			class EncKeyReader implements CryptoKeyReader
	//		{
	//
	//			EncryptionKeyInfo keyInfo = new EncryptionKeyInfo();
	//
	//			@@Override public EncryptionKeyInfo getPublicKey(String keyName, Map<String, String> keyMeta)
	//			{
	//				String CERT_FILE_PATH = "./src/test/resources/certificate/public-key." + keyName;
	//				if (Files.isReadable(Paths.get(CERT_FILE_PATH)))
	//				{
	//					try
	//					{
	//						keyInfo.setKey(Files.readAllBytes(Paths.get(CERT_FILE_PATH)));
	//						return keyInfo;
	//					}
	//					catch (IOException e)
	//					{
	//						Assert.fail("Failed to read certificate from " + CERT_FILE_PATH);
	//					}
	//				}
	//				else
	//				{
	//					Assert.fail("Certificate file " + CERT_FILE_PATH + " is not present or not readable.");
	//				}
	//				return null;
	//			}
	//
	//			@@Override public EncryptionKeyInfo getPrivateKey(String keyName, Map<String, String> keyMeta)
	//			{
	//				String CERT_FILE_PATH = "./src/test/resources/certificate/private-key." + keyName;
	//				if (Files.isReadable(Paths.get(CERT_FILE_PATH)))
	//				{
	//					try
	//					{
	//						keyInfo.setKey(Files.readAllBytes(Paths.get(CERT_FILE_PATH)));
	//						return keyInfo;
	//					}
	//					catch (IOException e)
	//					{
	//						Assert.fail("Failed to read certificate from " + CERT_FILE_PATH);
	//					}
	//				}
	//				else
	//				{
	//					Assert.fail("Certificate file " + CERT_FILE_PATH + " is not present or not readable.");
	//				}
	//				return null;
	//			}
	//		}

			const int totalMsg = 10;

			ISet<string> messageSet = Sets.newHashSet();
			Reader<sbyte[]> reader = pulsarClient.newReader().topic("persistent://my-property/my-ns/test-reader-myecdsa-topic1").startMessageId(MessageId_Fields.latest).cryptoKeyReader(new EncKeyReader()).create();

			Producer<sbyte[]> producer = pulsarClient.newProducer().topic("persistent://my-property/my-ns/test-reader-myecdsa-topic1").addEncryptionKey("client-ecdsa.pem").cryptoKeyReader(new EncKeyReader()).create();
			for (int i = 0; i < totalMsg; i++)
			{
				string message = "my-message-" + i;
				producer.send(message.GetBytes());
			}

			Message<sbyte[]> msg = null;

			for (int i = 0; i < totalMsg; i++)
			{
				msg = reader.readNext(5, TimeUnit.SECONDS);
				string receivedMessage = new string(msg.Data);
				log.debug("Received message: [{}]", receivedMessage);
				string expectedMessage = "my-message-" + i;
				testMessageOrderAndDuplicates(messageSet, receivedMessage, expectedMessage);
			}
			producer.close();
			reader.close();
			log.info("-- Exiting {} test --", methodName);
		}


		public virtual void testSimpleReaderReachEndOfTopic()
		{
			Reader<sbyte[]> reader = pulsarClient.newReader().topic("persistent://my-property/my-ns/testSimpleReaderReachEndOfTopic").startMessageId(MessageId_Fields.earliest).create();
			Producer<sbyte[]> producer = pulsarClient.newProducer().topic("persistent://my-property/my-ns/testSimpleReaderReachEndOfTopic").create();

			// no data write, should return false
			assertFalse(reader.hasMessageAvailable());

			// produce message 0 -- 99
			for (int i = 0; i < 100; i++)
			{
				string message = "my-message-" + i;
				producer.send(message.GetBytes());
			}

			MessageImpl<sbyte[]> msg = null;
			ISet<string> messageSet = Sets.newHashSet();
			int index = 0;

			// read message till end.
			while (reader.hasMessageAvailable())
			{
				msg = (MessageImpl<sbyte[]>) reader.readNext(1, TimeUnit.SECONDS);
				string receivedMessage = new string(msg.Data);
				log.debug("Received message: [{}]", receivedMessage);
				string expectedMessage = "my-message-" + (index++);
				testMessageOrderAndDuplicates(messageSet, receivedMessage, expectedMessage);
			}

			assertEquals(index, 100);
			// readNext should return null, after reach the end of topic.
			assertNull(reader.readNext(1, TimeUnit.SECONDS));

			// produce message again.
			for (int i = 100; i < 200; i++)
			{
				string message = "my-message-" + i;
				producer.send(message.GetBytes());
			}

			// read message till end again.
			while (reader.hasMessageAvailable())
			{
				msg = (MessageImpl<sbyte[]>) reader.readNext(1, TimeUnit.SECONDS);
				string receivedMessage = new string(msg.Data);
				log.debug("Received message: [{}]", receivedMessage);
				string expectedMessage = "my-message-" + (index++);
				testMessageOrderAndDuplicates(messageSet, receivedMessage, expectedMessage);
			}

			assertEquals(index, 200);
			// readNext should return null, after reach the end of topic.
			assertNull(reader.readNext(1, TimeUnit.SECONDS));

			reader.close();
			producer.close();
		}

		public virtual void testReaderReachEndOfTopicOnMessageWithBatches()
		{
			Reader<sbyte[]> reader = pulsarClient.newReader().topic("persistent://my-property/my-ns/testReaderReachEndOfTopicOnMessageWithBatches").startMessageId(MessageId_Fields.earliest).create();

			Producer<sbyte[]> producer = pulsarClient.newProducer().topic("persistent://my-property/my-ns/testReaderReachEndOfTopicOnMessageWithBatches").enableBatching(true).batchingMaxPublishDelay(100, TimeUnit.MILLISECONDS).create();

			// no data write, should return false
			assertFalse(reader.hasMessageAvailable());

			for (int i = 0; i < 100; i++)
			{
				string message = "my-message-" + i;
				producer.sendAsync(message.GetBytes());
			}

			// Write one sync message to ensure everything before got persistend
			producer.send("my-message-10".GetBytes());

			MessageId lastMessageId = null;
			int index = 0;
			assertTrue(reader.hasMessageAvailable());

			if (reader.hasMessageAvailable())
			{
				Message<sbyte[]> msg = reader.readNext();
				lastMessageId = msg.MessageId;
				assertEquals(lastMessageId.GetType(), typeof(BatchMessageIdImpl));

				while (msg != null)
				{
					index++;
					msg = reader.readNext(100, TimeUnit.MILLISECONDS);
				}
				assertEquals(index, 101);
			}

			assertFalse(reader.hasMessageAvailable());

			reader.close();
			producer.close();
		}


		public virtual void testMessageAvailableAfterRestart()
		{
			string topic = "persistent://my-property/use/my-ns/testMessageAvailableAfterRestart";
			string content = "my-message-1";

			// stop retention from cleaning up
			pulsarClient.newConsumer().topic(topic).subscriptionName("sub1").subscribe().close();

			using (Reader<sbyte[]> reader = pulsarClient.newReader().topic(topic).startMessageId(MessageId_Fields.earliest).create())
			{
				assertFalse(reader.hasMessageAvailable());
			}

			using (Producer<sbyte[]> producer = pulsarClient.newProducer().topic(topic).create())
			{
				producer.send(content.GetBytes());
			}

			using (Reader<sbyte[]> reader = pulsarClient.newReader().topic(topic).startMessageId(MessageId_Fields.earliest).create())
			{
				assertTrue(reader.hasMessageAvailable());
			}

			// cause broker to drop topic. Will be loaded next time we access it
			org.apache.pulsar.BrokerService.getTopicReference(topic).get().close(false).get();

			using (Reader<sbyte[]> reader = pulsarClient.newReader().topic(topic).startMessageId(MessageId_Fields.earliest).create())
			{
				assertTrue(reader.hasMessageAvailable());

				string readOut = new string(reader.readNext().Data);
				assertEquals(content, readOut);
				assertFalse(reader.hasMessageAvailable());
			}

		}


		public virtual void testHasMessageAvailable(bool enableBatch, bool startInclusive)
		{
			const string topicName = "persistent://my-property/my-ns/HasMessageAvailable";
			const int numOfMessage = 100;

			ProducerBuilder<sbyte[]> producerBuilder = pulsarClient.newProducer().topic(topicName);

			if (enableBatch)
			{
				producerBuilder.enableBatching(true).batchingMaxMessages(10);
			}
			else
			{
				producerBuilder.enableBatching(false);
			}

			Producer<sbyte[]> producer = producerBuilder.create();

			System.Threading.CountdownEvent latch = new System.Threading.CountdownEvent(numOfMessage);

			IList<MessageId> allIds = Collections.synchronizedList(new List<MessageId>());

			for (int i = 0; i < numOfMessage; i++)
			{
				producer.sendAsync(string.Format("msg num {0:D}", i).Bytes).whenComplete((mid, e) =>
				{
				if (e != null)
				{
					Assert.fail();
				}
				else
				{
					allIds.Add(mid);
				}
				latch.Signal();
				});
			}

			latch.await();

			allIds.sort(null); // make sure the largest mid appears at last.

			foreach (MessageId id in allIds)
			{
				Reader<sbyte[]> reader;

				if (startInclusive)
				{
					reader = pulsarClient.newReader().topic(topicName).startMessageId(id).startMessageIdInclusive().create();
				}
				else
				{
					reader = pulsarClient.newReader().topic(topicName).startMessageId(id).create();
				}

				if (startInclusive)
				{
					assertTrue(reader.hasMessageAvailable());
				}
				else if (id != allIds[allIds.Count - 1])
				{
					assertTrue(reader.hasMessageAvailable());
				}
				else
				{
					assertFalse(reader.hasMessageAvailable());
				}
				reader.close();
			}

			producer.close();
		}


		public virtual void testReaderNonDurableIsAbleToSeekRelativeTime()
		{
			const int numOfMessage = 10;
			const string topicName = "persistent://my-property/my-ns/ReaderNonDurableIsAbleToSeekRelativeTime";

			Producer<sbyte[]> producer = pulsarClient.newProducer().topic(topicName).create();

			for (int i = 0; i < numOfMessage; i++)
			{
				producer.send(string.Format("msg num {0:D}", i).Bytes);
			}

			Reader<sbyte[]> reader = pulsarClient.newReader().topic(topicName).startMessageId(MessageId_Fields.earliest).create();
			assertTrue(reader.hasMessageAvailable());

			reader.seek(RelativeTimeUtil.parseRelativeTimeInSeconds("-1m"));

			assertTrue(reader.hasMessageAvailable());

			reader.close();
			producer.close();
		}


		public virtual void testReaderIsAbleToSeekWithTimeOnBeginningOfTopic()
		{
			const string topicName = "persistent://my-property/my-ns/ReaderSeekWithTimeOnBeginningOfTopic";
			const int numOfMessage = 10;

			Producer<sbyte[]> producer = pulsarClient.newProducer().topic(topicName).create();

			for (int i = 0; i < numOfMessage; i++)
			{
				producer.send(string.Format("msg num {0:D}", i).Bytes);
			}

			Reader<sbyte[]> reader = pulsarClient.newReader().topic(topicName).startMessageId(MessageId_Fields.earliest).create();

			assertTrue(reader.hasMessageAvailable());

			// Read all messages the first time
			ISet<string> messageSetA = Sets.newHashSet();
			for (int i = 0; i < numOfMessage; i++)
			{
				Message<sbyte[]> message = reader.readNext();
				string receivedMessage = new string(message.Data);
				string expectedMessage = string.Format("msg num {0:D}", i);
				testMessageOrderAndDuplicates(messageSetA, receivedMessage, expectedMessage);
			}

			assertFalse(reader.hasMessageAvailable());

			// Perform cursor reset by time
			reader.seek(RelativeTimeUtil.parseRelativeTimeInSeconds("-1m"));

			// Read all messages a second time after seek()
			ISet<string> messageSetB = Sets.newHashSet();
			for (int i = 0; i < numOfMessage; i++)
			{
				Message<sbyte[]> message = reader.readNext();
				string receivedMessage = new string(message.Data);
				string expectedMessage = string.Format("msg num {0:D}", i);
				testMessageOrderAndDuplicates(messageSetB, receivedMessage, expectedMessage);
			}

			// Reader should be finished
			assertTrue(reader.Connected);
			assertFalse(reader.hasMessageAvailable());
			assertEquals(((ReaderImpl) reader).Consumer.numMessagesInQueue(), 0);

			reader.close();
			producer.close();
		}


		public virtual void testReaderIsAbleToSeekWithMessageIdOnMiddleOfTopic()
		{
			const string topicName = "persistent://my-property/my-ns/ReaderSeekWithMessageIdOnMiddleOfTopic";
			const int numOfMessage = 100;

			int halfMessages = numOfMessage / 2;

			Producer<sbyte[]> producer = pulsarClient.newProducer().topic(topicName).create();

			for (int i = 0; i < numOfMessage; i++)
			{
				producer.send(string.Format("msg num {0:D}", i).Bytes);
			}

			Reader<sbyte[]> reader = pulsarClient.newReader().topic(topicName).startMessageId(MessageId_Fields.earliest).create();

			assertTrue(reader.hasMessageAvailable());

			// Read all messages the first time
			MessageId midmessageToSeek = null;
			ISet<string> messageSetA = Sets.newHashSet();
			for (int i = 0; i < numOfMessage; i++)
			{
				Message<sbyte[]> message = reader.readNext();
				string receivedMessage = new string(message.Data);
				string expectedMessage = string.Format("msg num {0:D}", i);
				testMessageOrderAndDuplicates(messageSetA, receivedMessage, expectedMessage);

				if (i == halfMessages)
				{
					midmessageToSeek = message.MessageId;
				}
			}

			assertFalse(reader.hasMessageAvailable());

			// Perform cursor reset by MessageId to half of the topic
			reader.seek(midmessageToSeek);

			// Read all halved messages after seek()
			ISet<string> messageSetB = Sets.newHashSet();
			for (int i = halfMessages + 1; i < numOfMessage; i++)
			{
				Message<sbyte[]> message = reader.readNext();
				string receivedMessage = new string(message.Data);
				string expectedMessage = string.Format("msg num {0:D}", i);
				testMessageOrderAndDuplicates(messageSetB, receivedMessage, expectedMessage);
			}

			// Reader should be finished
			assertTrue(reader.Connected);
			assertFalse(reader.hasMessageAvailable());
			assertEquals(((ReaderImpl) reader).Consumer.numMessagesInQueue(), 0);

			reader.close();
			producer.close();
		}


		public virtual void testReaderIsAbleToSeekWithTimeOnMiddleOfTopic()
		{
			const string topicName = "persistent://my-property/my-ns/ReaderIsAbleToSeekWithTimeOnMiddleOfTopic";
			const int numOfMessage = 10;

			int halfMessages = numOfMessage / 2;

			Producer<sbyte[]> producer = pulsarClient.newProducer().topic(topicName).create();

			long l = DateTimeHelper.CurrentUnixTimeMillis();
			for (int i = 0; i < numOfMessage; i++)
			{
				producer.send(string.Format("msg num {0:D}", i).Bytes);
				Thread.Sleep(100);
			}

			Reader<sbyte[]> reader = pulsarClient.newReader().topic(topicName).startMessageId(MessageId_Fields.earliest).create();

			int plusTime = (halfMessages + 1) * 100;
			reader.seek(l + plusTime);

			ISet<string> messageSet = Sets.newHashSet();
			for (int i = halfMessages + 1; i < numOfMessage; i++)
			{
				Message<sbyte[]> message = reader.readNext();
				string receivedMessage = new string(message.Data);
				string expectedMessage = string.Format("msg num {0:D}", i);
				testMessageOrderAndDuplicates(messageSet, receivedMessage, expectedMessage);
			}

			reader.close();
			producer.close();
		}


		public virtual void testReaderStartMessageIdAtExpectedPos(bool batching, bool startInclusive, int numOfMessages)
		{
			const string topicName = "persistent://my-property/my-ns/ReaderStartMessageIdAtExpectedPos";

			int resetIndex = (new Random()).Next(numOfMessages); // Choose some random index to reset

			int firstMessage = startInclusive ? resetIndex : resetIndex + 1; // First message of reset

			Producer<sbyte[]> producer = pulsarClient.newProducer().topic(topicName).enableBatching(batching).create();

			System.Threading.CountdownEvent latch = new System.Threading.CountdownEvent(numOfMessages);

            AtomicReference<MessageId> resetPos = new AtomicReference<MessageId>();

			for (int i = 0; i < numOfMessages; i++)
			{


				int j = i;

				producer.sendAsync(string.Format("msg num {0:D}", i).Bytes).thenCompose(messageId => FutureUtils.value(Pair.of(j, messageId))).whenComplete((p, e) =>
				{
				if (e != null)
				{
					fail("send msg failed due to " + e.Message);
				}
				else
				{
					if (p.Left == resetIndex)
					{
						resetPos.set(p.Right);
					}
				}
				latch.Signal();
				});
			}

			latch.await();

			ReaderBuilder<sbyte[]> readerBuilder = pulsarClient.newReader().topic(topicName).startMessageId(resetPos.get());

			if (startInclusive)
			{
				readerBuilder.startMessageIdInclusive();
			}

			Reader<sbyte[]> reader = readerBuilder.create();
			ISet<string> messageSet = Sets.newHashSet();
			for (int i = firstMessage; i < numOfMessages; i++)
			{
				Message<sbyte[]> message = reader.readNext();
				string receivedMessage = new string(message.Data);
				string expectedMessage = string.Format("msg num {0:D}", i);
				testMessageOrderAndDuplicates(messageSet, receivedMessage, expectedMessage);
			}

			assertTrue(reader.Connected);
			assertEquals(((ReaderImpl) reader).Consumer.numMessagesInQueue(), 0);

			// Processed messages should be the number of messages in the range: [FirstResetMessage..TotalNumOfMessages]
			assertEquals(messageSet.Count, numOfMessages - firstMessage);

			reader.close();
			producer.close();
		}


		public virtual void testReaderBuilderConcurrentCreate()
		{
			string topicName = "persistent://my-property/my-ns/testReaderBuilderConcurrentCreate_";
			int numTopic = 30;
			ReaderBuilder<sbyte[]> builder = pulsarClient.newReader().startMessageId(MessageId_Fields.earliest);

			IList<CompletableFuture<Reader<sbyte[]>>> readers = Lists.newArrayListWithExpectedSize(numTopic);
			IList<Producer<sbyte[]>> producers = Lists.newArrayListWithExpectedSize(numTopic);
			// create producer firstly
			for (int i = 0; i < numTopic; i++)
			{
				producers.Add(pulsarClient.newProducer().topic(topicName + i).create());
			}

			// create reader concurrently
			for (int i = 0; i < numTopic; i++)
			{
				readers.Add(builder.clone().topic(topicName + i).createAsync());
			}

			// verify readers config are different for topic name.
			for (int i = 0; i < numTopic; i++)
			{
				assertEquals(readers[i].get().Topic, topicName + i);
				readers[i].get().close();
				producers[i].close();
			}
		}


		public virtual void testReaderStartInMiddleOfBatch()
		{
			const string topicName = "persistent://my-property/my-ns/ReaderStartInMiddleOfBatch";
			const int numOfMessage = 100;

			Producer<sbyte[]> producer = pulsarClient.newProducer().topic(topicName).enableBatching(true).batchingMaxMessages(10).create();

			System.Threading.CountdownEvent latch = new System.Threading.CountdownEvent(numOfMessage);

			IList<MessageId> allIds = Collections.synchronizedList(new List<MessageId>());

			for (int i = 0; i < numOfMessage; i++)
			{
				producer.sendAsync(string.Format("msg num {0:D}", i).Bytes).whenComplete((mid, e) =>
				{
				if (e != null)
				{
					fail();
				}
				else
				{
					allIds.Add(mid);
				}
				latch.Signal();
				});
			}

			latch.await();

			foreach (MessageId id in allIds)
			{
				Reader<sbyte[]> reader = pulsarClient.newReader().topic(topicName).startMessageId(id).startMessageIdInclusive().create();
				MessageId idGot = reader.readNext().MessageId;
				assertEquals(idGot, id);
				reader.close();
			}

			producer.close();
		}
	}

}