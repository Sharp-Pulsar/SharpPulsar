using System;
using System.Collections.Generic;

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

	public class ReaderTest 
	{

		private const string subscription = "reader-sub";

		public override void setup()
		{
			base.internalSetup();

			admin.clusters().createCluster("test", new ClusterData(pulsar.WebServiceAddress));
			admin.tenants().createTenant("my-property", new TenantInfo(Sets.newHashSet("appid1", "appid2"), Sets.newHashSet("test")));
			admin.namespaces().createNamespace("my-property/my-ns", Sets.newHashSet("test"));
		}


		public override void cleanup()
		{
			base.internalCleanup();
		}


		private ISet<string> publishMessages(string topic, int count, bool enableBatch)
		{
			ISet<string> keys = new HashSet<string>();
			ProducerBuilder<sbyte[]> builder = pulsarClient.newProducer();
			builder.messageRoutingMode(MessageRoutingMode.SinglePartition);
			builder.maxPendingMessages(count);
			// disable periodical flushing
			builder.batchingMaxPublishDelay(1, TimeUnit.DAYS);
			builder.topic(topic);
			if (enableBatch)
			{
				builder.enableBatching(true);
				builder.batchingMaxMessages(count);
			}
			else
			{
				builder.enableBatching(false);
			}
			using (Producer<sbyte[]> producer = builder.create())
			{

				Future<object> lastFuture = null;
				for (int i = 0; i < count; i++)
				{
					string key = "key" + i;
					sbyte[] data = ("my-message-" + i).GetBytes();
					lastFuture = producer.newMessage().key(key).value(data).sendAsync();
					keys.Add(key);
				}
				producer.flush();
				lastFuture.get();
			}
			return keys;
		}


		public virtual void testReadMessageWithoutBatching()
		{
			string topic = "persistent://my-property/my-ns/my-reader-topic";
			testReadMessages(topic, false);
		}


		public virtual void testReadMessageWithoutBatchingWithMessageInclusive()
		{
			string topic = "persistent://my-property/my-ns/my-reader-topic-inclusive";
			ISet<string> keys = publishMessages(topic, 10, false);

			Reader<sbyte[]> reader = pulsarClient.newReader().topic(topic).startMessageId(MessageId_Fields.latest).startMessageIdInclusive().readerName(subscription).create();

			Assert.assertTrue(reader.hasMessageAvailable());
			Assert.assertTrue(keys.remove(reader.readNext().Key));
			Assert.assertFalse(reader.hasMessageAvailable());
		}


		public virtual void testReadMessageWithBatching()
		{
			string topic = "persistent://my-property/my-ns/my-reader-topic-with-batching";
			testReadMessages(topic, true);
		}


		public virtual void testReadMessageWithBatchingWithMessageInclusive()
		{
			string topic = "persistent://my-property/my-ns/my-reader-topic-with-batching-inclusive";
			ISet<string> keys = publishMessages(topic, 10, true);

			Reader<sbyte[]> reader = pulsarClient.newReader().topic(topic).startMessageId(MessageId_Fields.latest).startMessageIdInclusive().readerName(subscription).create();

			while (reader.hasMessageAvailable())
			{
				Assert.assertTrue(keys.remove(reader.readNext().Key));
			}
			// start from latest with start message inclusive should only read the last message in batch
			Assert.assertTrue(keys.Count == 9);
			Assert.assertFalse(keys.Contains("key9"));
			Assert.assertFalse(reader.hasMessageAvailable());
		}


		private void testReadMessages(string topic, bool enableBatch)
		{
			int numKeys = 10;

			ISet<string> keys = publishMessages(topic, numKeys, enableBatch);
			Reader<sbyte[]> reader = pulsarClient.newReader().topic(topic).startMessageId(MessageId_Fields.earliest).readerName(subscription).create();

			while (reader.hasMessageAvailable())
			{
				Message<sbyte[]> message = reader.readNext();
				Assert.assertTrue(keys.remove(message.Key));
			}
			Assert.assertTrue(keys.Count == 0);

			Reader<sbyte[]> readLatest = pulsarClient.newReader().topic(topic).startMessageId(MessageId_Fields.latest).readerName(subscription + "latest").create();
			Assert.assertFalse(readLatest.hasMessageAvailable());
		}



		public virtual void testReadFromPartition()
		{
			string topic = "persistent://my-property/my-ns/testReadFromPartition";
			string partition0 = topic + "-partition-0";
			admin.topics().createPartitionedTopic(topic, 4);
			int numKeys = 10;

			ISet<string> keys = publishMessages(partition0, numKeys, false);
			Reader<sbyte[]> reader = pulsarClient.newReader().topic(partition0).startMessageId(MessageId_Fields.earliest).create();

			while (reader.hasMessageAvailable())
			{
				Message<sbyte[]> message = reader.readNext();
				Assert.assertTrue(keys.remove(message.Key));
			}
			Assert.assertTrue(keys.Count == 0);
		}

		/// <summary>
		/// It verifies that reader can set initial position based on provided rollback time.
		/// <pre>
		/// 1. publish messages which are 5 hour old
		/// 2. publish messages which are 1 hour old
		/// 3. Create reader with rollback time 2 hours
		/// 4. Reader should be able to read only messages which are only 2 hours old
		/// </pre> </summary>
		/// <exception cref="Exception"> </exception>
		/// 
		public virtual void testReaderWithTimeLong()
		{
			string ns = "my-property/my-ns";
			string topic = "persistent://" + ns + "/testReadFromPartition";
			RetentionPolicies retention = new RetentionPolicies(-1, -1);
			admin.namespaces().setRetention(ns, retention);

			ProducerBuilder<sbyte[]> produceBuilder = pulsarClient.newProducer();
			produceBuilder.topic(topic);
			produceBuilder.enableBatching(false);
			Producer<sbyte[]> producer = produceBuilder.create();
			MessageId lastMsgId = null;
			int totalMsg = 10;
			// (1) Publish 10 messages with publish-time 5 HOUR back
			long oldMsgPublishTime = DateTimeHelper.CurrentUnixTimeMillis() - TimeUnit.HOURS.toMillis(5); // 5 hours old
			for (int i = 0; i < totalMsg; i++)
			{
				TypedMessageBuilderImpl<sbyte[]> msg = (TypedMessageBuilderImpl<sbyte[]>) producer.newMessage().value(("old" + i).GetBytes());
				Builder metadataBuilder = msg.MetadataBuilder;
				metadataBuilder.setPublishTime(oldMsgPublishTime).setSequenceId(i);
				metadataBuilder.setProducerName(producer.ProducerName).setReplicatedFrom("us-west1");
				lastMsgId = msg.send();
			}

			// (2) Publish 10 messages with publish-time 1 HOUR back
			long newMsgPublishTime = DateTimeHelper.CurrentUnixTimeMillis() - TimeUnit.HOURS.toMillis(1); // 1 hour old
			MessageId firstMsgId = null;
			for (int i = 0; i < totalMsg; i++)
			{
				TypedMessageBuilderImpl<sbyte[]> msg = (TypedMessageBuilderImpl<sbyte[]>) producer.newMessage().value(("new" + i).GetBytes());
				Builder metadataBuilder = msg.MetadataBuilder;
				metadataBuilder.PublishTime = newMsgPublishTime;
				metadataBuilder.setProducerName(producer.ProducerName).setReplicatedFrom("us-west1");
				MessageId msgId = msg.send();
				if (firstMsgId == null)
				{
					firstMsgId = msgId;
				}
			}

			// (3) Create reader and set position 1 hour back so, it should only read messages which are 2 hours old which
			// published on step 2
			Reader<sbyte[]> reader = pulsarClient.newReader().topic(topic).startMessageFromRollbackDuration(2, TimeUnit.HOURS).create();

			IList<MessageId> receivedMessageIds = Lists.newArrayList();

			while (reader.hasMessageAvailable())
			{
				Message<sbyte[]> msg = reader.readNext(1, TimeUnit.SECONDS);
				if (msg == null)
				{
					break;
				}
				Console.WriteLine("msg.getMessageId()=" + msg.MessageId + ", data=" + (new string(msg.Data)));
				receivedMessageIds.Add(msg.MessageId);
			}

			assertEquals(receivedMessageIds.Count, totalMsg);
			assertEquals(receivedMessageIds[0], firstMsgId);

			restartBroker();

			assertFalse(reader.hasMessageAvailable());
		}

		/// <summary>
		/// We need to ensure that delete subscription of read also need to delete the
		/// non-durable cursor, because data deletion depends on the mark delete position of all cursors.
		/// </summary>
		/// 
		public virtual void testRemoveSubscriptionForReaderNeedRemoveCursor()
		{

			const string topic = "persistent://my-property/my-ns/testRemoveSubscriptionForReaderNeedRemoveCursor";

            Reader<sbyte[]> reader1 = pulsarClient.newReader().topic(topic).startMessageId(MessageId_Fields.earliest).create();

            Reader<sbyte[]> reader2 = pulsarClient.newReader().topic(topic).startMessageId(MessageId_Fields.earliest).create();

			Assert.assertEquals(admin.topics().getStats(topic).subscriptions.Count, 2);
			Assert.assertEquals(admin.topics().getInternalStats(topic).cursors.Count, 2);

			reader1.close();

			Assert.assertEquals(admin.topics().getStats(topic).subscriptions.Count, 1);
			Assert.assertEquals(admin.topics().getInternalStats(topic).cursors.Count, 1);

			reader2.close();

			Assert.assertEquals(admin.topics().getStats(topic).subscriptions.Count, 0);
			Assert.assertEquals(admin.topics().getInternalStats(topic).cursors.Count, 0);

		}

        public virtual void testKeyHashRangeReader()
		{
            IList<string> keys = Arrays.asList("0", "1", "2", "3", "4", "5", "6", "7", "8", "9");
			const string topic = "persistent://my-property/my-ns/testKeyHashRangeReader";

			try
			{
				pulsarClient.newReader().topic(topic).startMessageId(MessageId_Fields.earliest).keyHashRange(Range.of(0, 10000), Range.of(8000, 12000)).create();
				fail("should failed with unexpected key hash range");
			}
			catch (System.ArgumentException e)
			{
				log.error("Create key hash range failed", e);
			}

			try
			{
				pulsarClient.newReader().topic(topic).startMessageId(MessageId_Fields.earliest).keyHashRange(Range.of(30000, 20000)).create();
				fail("should failed with unexpected key hash range");
			}
			catch (System.ArgumentException e)
			{
				log.error("Create key hash range failed", e);
			}

			try
			{
				pulsarClient.newReader().topic(topic).startMessageId(MessageId_Fields.earliest).keyHashRange(Range.of(80000, 90000)).create();
				fail("should failed with unexpected key hash range");
			}
			catch (System.ArgumentException e)
			{
				log.error("Create key hash range failed", e);
			}

            Reader<string> reader = pulsarClient.newReader(Schema_Fields.STRING).topic(topic).startMessageId(MessageId_Fields.earliest).keyHashRange(Range.of(0, StickyKeyConsumerSelector_Fields.DEFAULT_RANGE_SIZE / 2)).create();

            Producer<string> producer = pulsarClient.newProducer(Schema_Fields.STRING).topic(topic).enableBatching(false).create();

			int expectedMessages = 0;
			foreach (string key in keys)
			{
				int slot = Murmur3_32Hash.Instance.makeHash(key.GetBytes()) % StickyKeyConsumerSelector_Fields.DEFAULT_RANGE_SIZE;
				if (slot <= StickyKeyConsumerSelector_Fields.DEFAULT_RANGE_SIZE / 2)
				{
					expectedMessages++;
				}
				producer.newMessage().key(key).value(key).send();
				log.info("Publish message to slot {}", slot);
			}

			IList<string> receivedMessages = new List<string>();

			Message<string> msg;
			do
			{
				msg = reader.readNext(1, TimeUnit.SECONDS);
				if (msg != null)
				{
					receivedMessages.Add(msg.Value);
				}
			} while (msg != null);

			assertTrue(expectedMessages > 0);
			assertEquals(receivedMessages.Count, expectedMessages);
			foreach (string receivedMessage in receivedMessages)
			{
				log.info("Receive message {}", receivedMessage);
				assertTrue(Convert.ToInt32(receivedMessage) <= StickyKeyConsumerSelector_Fields.DEFAULT_RANGE_SIZE / 2);
			}

		}
	}

}