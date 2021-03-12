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


	public class MultiTopicsReaderTest
	{

		private const string Subscription = "reader-multi-topics-sub";

		protected internal override void Setup()
		{
			base.InternalSetup();

			Admin.Clusters().CreateCluster("test", new ClusterData(Pulsar.WebServiceAddress));
			Admin.Tenants().CreateTenant("my-property", new TenantInfo(Sets.newHashSet("appid1", "appid2"), Sets.newHashSet("test")));
			Admin.Namespaces().createNamespace("my-property/my-ns", Sets.newHashSet("test"));
		}


		protected internal override void Cleanup()
		{
			base.InternalCleanup();
		}

		public virtual void TestReadMessageWithoutBatching()
		{
			string topic = "persistent://my-property/my-ns/my-reader-topic";
			Admin.Topics().CreatePartitionedTopic(topic, 3);
			TestReadMessages(topic, false);
		}

		public virtual void TestReadMessageWithoutBatchingWithMessageInclusive()
		{
			string topic = "persistent://my-property/my-ns/my-reader-topic-inclusive";
			int topicNum = 3;
			Admin.Topics().CreatePartitionedTopic(topic, topicNum);
			ISet<string> keys = PublishMessages(topic, 10, false);

			Reader<sbyte[]> reader = PulsarClient.NewReader().Topic(topic).StartMessageId(MessageId.latest).StartMessageIdInclusive().ReaderName(Subscription).Create();
			int count = 0;
			while(reader.HasMessageAvailable())
			{
				Assert.assertTrue(keys.remove(reader.ReadNext(1, TimeUnit.SECONDS).Key));
				count++;
			}
			Assert.assertEquals(count, topicNum);
			Assert.assertFalse(reader.HasMessageAvailable());
		}

		public virtual void TestReadMessageWithBatching()
		{
			string topic = "persistent://my-property/my-ns/my-reader-topic-with-batching";
			Admin.Topics().CreatePartitionedTopic(topic, 3);
			TestReadMessages(topic, true);
		}

		public virtual void TestReadMessageWithBatchingWithMessageInclusive()
		{
			string topic = "persistent://my-property/my-ns/my-reader-topic-with-batching-inclusive";
			int topicNum = 3;
			int msgNum = 15;
			Admin.Topics().CreatePartitionedTopic(topic, topicNum);
			ISet<string> keys = PublishMessages(topic, msgNum, true);

			Reader<sbyte[]> reader = PulsarClient.NewReader().Topic(topic).StartMessageId(MessageId.latest).StartMessageIdInclusive().ReaderName(Subscription).Create();

			while(reader.HasMessageAvailable())
			{
				Assert.assertTrue(keys.remove(reader.ReadNext(2, TimeUnit.SECONDS).Key));
			}
			// start from latest with start message inclusive should only read the last 3 message from 3 partition
			Assert.assertEquals(keys.Count, msgNum - topicNum);
			Assert.assertFalse(keys.Contains("key14"));
			Assert.assertFalse(keys.Contains("key13"));
			Assert.assertFalse(keys.Contains("key12"));
			Assert.assertFalse(reader.HasMessageAvailable());
		}

		public virtual void TestReaderWithTimeLong()
		{
			string ns = "my-property/my-ns";
			string topic = "persistent://" + ns + "/testReadFromPartition";
			Admin.Topics().CreatePartitionedTopic(topic, 3);
			RetentionPolicies retention = new RetentionPolicies(-1, -1);
			Admin.Namespaces().SetRetention(ns, retention);

			ProducerBuilder<sbyte[]> produceBuilder = PulsarClient.NewProducer();
			produceBuilder.Topic(topic);
			produceBuilder.EnableBatching(false);
			Producer<sbyte[]> producer = produceBuilder.Create();
			int totalMsg = 10;
			// (1) Publish 10 messages with publish-time 5 HOUR back
			long oldMsgPublishTime = DateTimeHelper.CurrentUnixTimeMillis() - TimeUnit.HOURS.toMillis(5); // 5 hours old
			for(int i = 0; i < totalMsg; i++)
			{
				TypedMessageBuilderImpl<sbyte[]> msg = (TypedMessageBuilderImpl<sbyte[]>) producer.NewMessage().Value(("old" + i).GetBytes());
				PulsarApi.MessageMetadata.Builder metadataBuilder = msg.MetadataBuilder;
				metadataBuilder.setPublishTime(oldMsgPublishTime).setSequenceId(i);
				metadataBuilder.setProducerName(producer.ProducerName).setReplicatedFrom("us-west1");
			}

			// (2) Publish 10 messages with publish-time 1 HOUR back
			long newMsgPublishTime = DateTimeHelper.CurrentUnixTimeMillis() - TimeUnit.HOURS.toMillis(1); // 1 hour old
			MessageId firstMsgId = null;
			for(int i = 0; i < totalMsg; i++)
			{
				TypedMessageBuilderImpl<sbyte[]> msg = (TypedMessageBuilderImpl<sbyte[]>) producer.NewMessage().Value(("new" + i).GetBytes());
				PulsarApi.MessageMetadata.Builder metadataBuilder = msg.MetadataBuilder;
				metadataBuilder.PublishTime = newMsgPublishTime;
				metadataBuilder.setProducerName(producer.ProducerName).setReplicatedFrom("us-west1");
				MessageId msgId = msg.Send();
				if(firstMsgId == null)
				{
					firstMsgId = msgId;
				}
			}

			// (3) Create reader and set position 1 hour back so, it should only read messages which are 2 hours old which
			// published on step 2
			Reader<sbyte[]> reader = PulsarClient.NewReader().Topic(topic).StartMessageFromRollbackDuration(2, TimeUnit.HOURS).Create();

			IList<MessageId> receivedMessageIds = Lists.newArrayList();

			while(reader.HasMessageAvailable())
			{
				Message<sbyte[]> msg = reader.ReadNext(1, TimeUnit.SECONDS);
				if(msg == null)
				{
					break;
				}
				receivedMessageIds.Add(msg.MessageId);
			}

			assertEquals(receivedMessageIds.Count, totalMsg);

			RestartBroker();

			assertFalse(reader.HasMessageAvailable());
		}

		public virtual void TestRemoveSubscriptionForReaderNeedRemoveCursor()
		{

			const string topic = "persistent://my-property/my-ns/testRemoveSubscriptionForReaderNeedRemoveCursor";
			Admin.Topics().CreatePartitionedTopic(topic, 3);
			Reader<sbyte[]> reader1 = PulsarClient.NewReader().Topic(topic).StartMessageId(MessageId.earliest).Create();

			Reader<sbyte[]> reader2 = PulsarClient.NewReader().Topic(topic).StartMessageId(MessageId.earliest).Create();

			Assert.assertEquals(Admin.Topics().GetSubscriptions(topic).Count, 2);
			foreach(PersistentTopicInternalStats value in Admin.Topics().GetPartitionedInternalStats(topic).Partitions.Values)
			{
				Assert.assertEquals(value.Cursors.Count, 2);
			}

			reader1.close();

			Assert.assertEquals(Admin.Topics().GetSubscriptions(topic).Count, 1);
			foreach(PersistentTopicInternalStats value in Admin.Topics().GetPartitionedInternalStats(topic).Partitions.Values)
			{
				Assert.assertEquals(value.Cursors.Count, 1);
			}

			reader2.close();

			Assert.assertEquals(Admin.Topics().GetSubscriptions(topic).Count, 0);
			foreach(PersistentTopicInternalStats value in Admin.Topics().GetPartitionedInternalStats(topic).Partitions.Values)
			{
				Assert.assertEquals(value.Cursors.Count, 0);
			}

		}

		public virtual void TestMultiReaderSeek()
		{
			string topic = "persistent://my-property/my-ns/testKeyHashRangeReader";
			Admin.Topics().CreatePartitionedTopic(topic, 3);
			ISet<string> ids = PublishMessages(topic,100,false);
			IList<string> idList = new List<string>(ids);
			idList.Sort();
		}

		public virtual void TestKeyHashRangeReader()
		{
			IList<string> keys = new List<string> {"0", "1", "2", "3", "4", "5", "6", "7", "8", "9"};
			const string topic = "persistent://my-property/my-ns/testKeyHashRangeReader";
			Admin.Topics().CreatePartitionedTopic(topic, 3);

			try
			{
				PulsarClient.NewReader().Topic(topic).StartMessageId(MessageId.earliest).KeyHashRange(Range.Of(0, 10000), Range.Of(8000, 12000)).create();
				fail("should failed with unexpected key hash range");
			}
			catch(System.ArgumentException)
			{
			}

			try
			{
				PulsarClient.NewReader().Topic(topic).StartMessageId(MessageId.earliest).KeyHashRange(Range.Of(30000, 20000)).Create();
				fail("should failed with unexpected key hash range");
			}
			catch(System.ArgumentException)
			{
			}

			try
			{
				PulsarClient.NewReader().Topic(topic).StartMessageId(MessageId.earliest).KeyHashRange(Range.Of(80000, 90000)).Create();
				fail("should failed with unexpected key hash range");
			}
			catch(System.ArgumentException)
			{
			}

			Reader<string> reader = PulsarClient.NewReader(Schema.STRING).Topic(topic).StartMessageId(MessageId.earliest).KeyHashRange(Range.Of(0, StickyKeyConsumerSelector.DEFAULT_RANGE_SIZE / 2)).Create();

			Producer<string> producer = PulsarClient.NewProducer(Schema.STRING).Topic(topic).EnableBatching(false).Create();

			int expectedMessages = 0;
			foreach(string key in keys)
			{
				int slot = Murmur3_32Hash.Instance.makeHash(key.GetBytes()) % StickyKeyConsumerSelector.DEFAULT_RANGE_SIZE;
				if(slot <= StickyKeyConsumerSelector.DEFAULT_RANGE_SIZE / 2)
				{
					expectedMessages++;
				}
				producer.newMessage().key(key).value(key).send();
			}

			IList<string> receivedMessages = new List<string>();

			Message<string> msg;
			do
			{
				msg = reader.readNext(1, TimeUnit.SECONDS);
				if(msg != null)
				{
					receivedMessages.Add(msg.Value);
				}
			} while(msg != null);

			assertTrue(expectedMessages > 0);
			assertEquals(receivedMessages.Count, expectedMessages);
			foreach(string receivedMessage in receivedMessages)
			{
				assertTrue(Convert.ToInt32(receivedMessage) <= StickyKeyConsumerSelector.DEFAULT_RANGE_SIZE / 2);
			}

		}

		private void TestReadMessages(string topic, bool enableBatch)
		{
			int numKeys = 9;

			ISet<string> keys = PublishMessages(topic, numKeys, enableBatch);
			Reader<sbyte[]> reader = PulsarClient.NewReader().Topic(topic).StartMessageId(MessageId.earliest).ReaderName(Subscription).Create();

			while(reader.HasMessageAvailable())
			{
				Message<sbyte[]> message = reader.ReadNext();
				Assert.assertTrue(keys.remove(message.Key));
			}
			Assert.assertEquals(keys.Count, 0);

			Reader<sbyte[]> readLatest = PulsarClient.NewReader().Topic(topic).StartMessageId(MessageId.latest).ReaderName(Subscription + "latest").Create();
			Assert.assertFalse(readLatest.HasMessageAvailable());
		}

		private ISet<string> PublishMessages(string topic, int count, bool enableBatch)
		{
			ISet<string> keys = new HashSet<string>();
			ProducerBuilder<sbyte[]> builder = PulsarClient.NewProducer();
			builder.MessageRoutingMode(MessageRoutingMode.RoundRobinPartition);
			builder.MaxPendingMessages(count);
			// disable periodical flushing
			builder.BatchingMaxPublishDelay(1, TimeUnit.DAYS);
			builder.Topic(topic);
			if(enableBatch)
			{
				builder.EnableBatching(true);
				builder.BatchingMaxMessages(count);
			}
			else
			{
				builder.EnableBatching(false);
			}
			using(Producer<sbyte[]> producer = builder.Create())
			{

				Future<object> lastFuture = null;
				for(int i = 0; i < count; i++)
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

	}

}