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
namespace SharpPulsar.Test
{
	public class ReaderTest
	{

		private const string Subscription = "reader-sub";

		private ISet<string> PublishMessages(string topic, int count, bool enableBatch)
		{
			ISet<string> keys = new HashSet<string>();
			ProducerBuilder<sbyte[]> builder = PulsarClient.NewProducer();
			builder.MessageRoutingMode(MessageRoutingMode.SinglePartition);
			builder.MaxPendingMessages(count);
			// disable periodical flushing
			builder.BatchingMaxPublishDelay(1, TimeUnit.DAYS);
			builder.Topic(topic);
			if (enableBatch)
			{
				builder.EnableBatching(true);
				builder.BatchingMaxMessages(count);
			}
			else
			{
				builder.EnableBatching(false);
			}
			using (Producer<sbyte[]> producer = builder.Create())
			{
				Future<object> lastFuture = null;
				for (int i = 0; i < count; i++)
				{
					string key = "key" + i;
					sbyte[] data = ("my-message-" + i).GetBytes();
					lastFuture = producer.NewMessage().Key(key).Value(data).SendAsync();
					keys.Add(key);
				}
				producer.Flush();
				lastFuture.get();
			}
			return keys;
		}

		public virtual void TestReadMessageWithoutBatching()
		{
			string topic = "persistent://my-property/my-ns/my-reader-topic";
			TestReadMessages(topic, false);
		}

		
		public virtual void TestReadMessageWithoutBatchingWithMessageInclusive()
		{
			string topic = "persistent://my-property/my-ns/my-reader-topic-inclusive";
			ISet<string> keys = PublishMessages(topic, 10, false);

			Reader<sbyte[]> reader = PulsarClient.NewReader().Topic(topic).StartMessageId(MessageId.latest).StartMessageIdInclusive().ReaderName(Subscription).Create();

			Assert.assertTrue(reader.HasMessageAvailable());
			Assert.assertTrue(keys.remove(reader.ReadNext().Key));
			Assert.assertFalse(reader.HasMessageAvailable());
		}

		public virtual void TestReadMessageWithBatching()
		{
			string topic = "persistent://my-property/my-ns/my-reader-topic-with-batching";
			TestReadMessages(topic, true);
		}

		public virtual void TestReadMessageWithBatchingWithMessageInclusive()
		{
			string topic = "persistent://my-property/my-ns/my-reader-topic-with-batching-inclusive";
			ISet<string> keys = PublishMessages(topic, 10, true);

			Reader<sbyte[]> reader = PulsarClient.NewReader().Topic(topic).StartMessageId(MessageId.latest).StartMessageIdInclusive().ReaderName(Subscription).Create();

			while (reader.HasMessageAvailable())
			{
				Assert.assertTrue(keys.remove(reader.ReadNext().Key));
			}
			// start from latest with start message inclusive should only read the last message in batch
			Assert.assertTrue(keys.Count == 9);
			Assert.assertFalse(keys.Contains("key9"));
			Assert.assertFalse(reader.HasMessageAvailable());
		}

		private void TestReadMessages(string topic, bool enableBatch)
		{
			int numKeys = 10;

			ISet<string> keys = PublishMessages(topic, numKeys, enableBatch);
			Reader<sbyte[]> reader = PulsarClient.NewReader().Topic(topic).StartMessageId(MessageId.earliest).ReaderName(Subscription).Create();

			while (reader.HasMessageAvailable())
			{
				Message<sbyte[]> message = reader.ReadNext();
				Assert.assertTrue(keys.remove(message.Key));
			}
			Assert.assertTrue(keys.Count == 0);

			Reader<sbyte[]> readLatest = PulsarClient.NewReader().Topic(topic).StartMessageId(MessageId.latest).ReaderName(Subscription + "latest").Create();
			Assert.assertFalse(readLatest.HasMessageAvailable());
		}

		public virtual void TestReadFromPartition()
		{
			string topic = "persistent://my-property/my-ns/testReadFromPartition";
			string partition0 = topic + "-partition-0";
			Admin.Topics().CreatePartitionedTopic(topic, 4);
			int numKeys = 10;

			ISet<string> keys = PublishMessages(partition0, numKeys, false);
			Reader<sbyte[]> reader = PulsarClient.NewReader().Topic(partition0).StartMessageId(MessageId.earliest).Create();

			while (reader.HasMessageAvailable())
			{
				Message<sbyte[]> message = reader.ReadNext();
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
		
		public virtual void TestReaderWithTimeLong()
		{
			string ns = "my-property/my-ns";
			string topic = "persistent://" + ns + "/testReadFromPartition";
			RetentionPolicies retention = new RetentionPolicies(-1, -1);
			Admin.Namespaces().SetRetention(ns, retention);

			ProducerBuilder<sbyte[]> produceBuilder = PulsarClient.NewProducer();
			produceBuilder.Topic(topic);
			produceBuilder.EnableBatching(false);
			Producer<sbyte[]> producer = produceBuilder.Create();
			MessageId lastMsgId = null;
			int totalMsg = 10;
			// (1) Publish 10 messages with publish-time 5 HOUR back
			long oldMsgPublishTime = DateTimeHelper.CurrentUnixTimeMillis() - TimeUnit.HOURS.toMillis(5); // 5 hours old
			for (int i = 0; i < totalMsg; i++)
			{
				TypedMessageBuilderImpl<sbyte[]> msg = (TypedMessageBuilderImpl<sbyte[]>)producer.NewMessage().Value(("old" + i).GetBytes());
				Builder metadataBuilder = msg.MetadataBuilder;
				metadataBuilder.setPublishTime(oldMsgPublishTime).setSequenceId(i);
				metadataBuilder.setProducerName(producer.ProducerName).setReplicatedFrom("us-west1");
				lastMsgId = msg.Send();
			}

			// (2) Publish 10 messages with publish-time 1 HOUR back
			long newMsgPublishTime = DateTimeHelper.CurrentUnixTimeMillis() - TimeUnit.HOURS.toMillis(1); // 1 hour old
			MessageId firstMsgId = null;
			for (int i = 0; i < totalMsg; i++)
			{
				TypedMessageBuilderImpl<sbyte[]> msg = (TypedMessageBuilderImpl<sbyte[]>)producer.NewMessage().Value(("new" + i).GetBytes());
				Builder metadataBuilder = msg.MetadataBuilder;
				metadataBuilder.PublishTime = newMsgPublishTime;
				metadataBuilder.setProducerName(producer.ProducerName).setReplicatedFrom("us-west1");
				MessageId msgId = msg.Send();
				if (firstMsgId == null)
				{
					firstMsgId = msgId;
				}
			}

			// (3) Create reader and set position 1 hour back so, it should only read messages which are 2 hours old which
			// published on step 2
			Reader<sbyte[]> reader = PulsarClient.NewReader().Topic(topic).StartMessageFromRollbackDuration(2, TimeUnit.HOURS).Create();

			IList<MessageId> receivedMessageIds = Lists.newArrayList();

			while (reader.HasMessageAvailable())
			{
				Message<sbyte[]> msg = reader.ReadNext(1, TimeUnit.SECONDS);
				if (msg == null)
				{
					break;
				}
				Console.WriteLine("msg.getMessageId()=" + msg.MessageId + ", data=" + (new string(msg.Data)));
				receivedMessageIds.Add(msg.MessageId);
			}

			assertEquals(receivedMessageIds.Count, totalMsg);
			assertEquals(receivedMessageIds[0], firstMsgId);

			RestartBroker();

			assertFalse(reader.HasMessageAvailable());
		}

		/// <summary>
		/// We need to ensure that delete subscription of read also need to delete the
		/// non-durable cursor, because data deletion depends on the mark delete position of all cursors.
		/// </summary>
		
		public virtual void TestRemoveSubscriptionForReaderNeedRemoveCursor()
		{

			const string topic = "persistent://my-property/my-ns/testRemoveSubscriptionForReaderNeedRemoveCursor";

			
			Reader<sbyte[]> reader1 = PulsarClient.NewReader().Topic(topic).StartMessageId(MessageId.earliest).Create();

			
			Reader<sbyte[]> reader2 = PulsarClient.NewReader().Topic(topic).StartMessageId(MessageId.earliest).Create();

			Assert.assertEquals(Admin.Topics().GetStats(topic).Subscriptions.Count, 2);
			Assert.assertEquals(Admin.Topics().GetInternalStats(topic, false).Cursors.Count, 2);

			reader1.close();

			Assert.assertEquals(Admin.Topics().GetStats(topic).Subscriptions.Count, 1);
			Assert.assertEquals(Admin.Topics().GetInternalStats(topic, false).Cursors.Count, 1);

			reader2.close();

			Assert.assertEquals(Admin.Topics().GetStats(topic).Subscriptions.Count, 0);
			Assert.assertEquals(Admin.Topics().GetInternalStats(topic, false).Cursors.Count, 0);

		}

		public virtual void TestKeyHashRangeReader()
		{			
			IList<string> keys = new List<string> { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };
			const string topic = "persistent://my-property/my-ns/testKeyHashRangeReader";

			try
			{
				PulsarClient.NewReader().Topic(topic).StartMessageId(MessageId.earliest).KeyHashRange(Range.Of(0, 10000), Range.Of(8000, 12000)).create();
				fail("should failed with unexpected key hash range");
			}
			catch (System.ArgumentException e)
			{
				log.error("Create key hash range failed", e);
			}

			try
			{
				PulsarClient.NewReader().Topic(topic).StartMessageId(MessageId.earliest).KeyHashRange(Range.Of(30000, 20000)).Create();
				fail("should failed with unexpected key hash range");
			}
			catch (System.ArgumentException e)
			{
				log.error("Create key hash range failed", e);
			}

			try
			{
				PulsarClient.NewReader().Topic(topic).StartMessageId(MessageId.earliest).KeyHashRange(Range.Of(80000, 90000)).Create();
				fail("should failed with unexpected key hash range");
			}
			catch (System.ArgumentException e)
			{
				log.error("Create key hash range failed", e);
			}

			Reader<string> reader = PulsarClient.NewReader(Schema.STRING).Topic(topic).StartMessageId(MessageId.earliest).KeyHashRange(Range.Of(0, StickyKeyConsumerSelector.DEFAULT_RANGE_SIZE / 2)).Create();

			
			Producer<string> producer = PulsarClient.NewProducer(Schema.STRING).Topic(topic).EnableBatching(false).Create();

			int expectedMessages = 0;
			foreach (string key in keys)
			{
				int slot = Murmur3_32Hash.Instance.makeHash(key.GetBytes()) % StickyKeyConsumerSelector.DEFAULT_RANGE_SIZE;
				if (slot <= StickyKeyConsumerSelector.DEFAULT_RANGE_SIZE / 2)
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
				assertTrue(Convert.ToInt32(receivedMessage) <= StickyKeyConsumerSelector.DEFAULT_RANGE_SIZE / 2);
			}

		}
	}

}