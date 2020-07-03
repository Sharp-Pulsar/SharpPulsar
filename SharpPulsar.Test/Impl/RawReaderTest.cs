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


    public class RawReaderTest 
	{
		private const int BATCH_MAX_MESSAGES = 10;
		private const string subscription = "foobar-sub";


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


		private ISet<string> publishMessages(string topic, int count)
		{
			return publishMessages(topic, count, false);
		}


		private ISet<string> publishMessages(string topic, int count, bool batching)
		{
			ISet<string> keys = new HashSet<string>();

			using (Producer<sbyte[]> producer = pulsarClient.newProducer().enableBatching(batching).batchingMaxMessages(10).messageRoutingMode(MessageRoutingMode.SinglePartition).maxPendingMessages(count).topic(topic).create())
			{

				Future<object> lastFuture = null;
				for (int i = 0; i < count; i++)
				{
					string key = "key" + i;
					sbyte[] data = ("my-message-" + i).GetBytes();
					lastFuture = producer.newMessage().key(key).value(data).sendAsync();
					keys.Add(key);
				}
				lastFuture.get();
			}
			return keys;
		}


		public static string extractKey(RawMessage m)
		{
			ByteBuf headersAndPayload = m.HeadersAndPayload;
			MessageMetadata msgMetadata = Commands.parseMessageMetadata(headersAndPayload);
			return msgMetadata.PartitionKey;
		}


		public virtual void testRawReader()
		{
			int numKeys = 10;

			string topic = "persistent://my-property/my-ns/my-raw-topic";

			ISet<string> keys = publishMessages(topic, numKeys);

			RawReader reader = RawReader.create(pulsarClient, topic, subscription).get();

			MessageId lastMessageId = reader.LastMessageIdAsync.get();
			while (true)
			{
				using (RawMessage m = reader.readNextAsync().get())
				{
					Assert.assertTrue(keys.remove(extractKey(m)));
					if (lastMessageId.CompareTo(m.MessageId) == 0)
					{
						break;
					}
				}
			}
			Assert.assertTrue(keys.Count == 0);
		}


		public virtual void testSeekToStart()
		{
			int numKeys = 10;
			string topic = "persistent://my-property/my-ns/my-raw-topic";

			publishMessages(topic, numKeys);

			ISet<string> readKeys = new HashSet<string>();
			RawReader reader = RawReader.create(pulsarClient, topic, subscription).get();
			MessageId lastMessageId = reader.LastMessageIdAsync.get();
			while (true)
			{
				using (RawMessage m = reader.readNextAsync().get())
				{
					readKeys.Add(extractKey(m));
					if (lastMessageId.CompareTo(m.MessageId) == 0)
					{
						break;
					}
				}
			}
			Assert.assertEquals(readKeys.Count, numKeys);

			// seek to start, read all keys again,
			// assert that we read all keys we had read previously
			reader.seekAsync(MessageId_Fields.earliest).get();
			while (true)
			{
				using (RawMessage m = reader.readNextAsync().get())
				{
					Assert.assertTrue(readKeys.remove(extractKey(m)));
					if (lastMessageId.CompareTo(m.MessageId) == 0)
					{
						break;
					}
				}
			}
			Assert.assertTrue(readKeys.Count == 0);
		}


		public virtual void testSeekToMiddle()
		{
			int numKeys = 10;
			string topic = "persistent://my-property/my-ns/my-raw-topic";

			publishMessages(topic, numKeys);

			ISet<string> readKeys = new HashSet<string>();
			RawReader reader = RawReader.create(pulsarClient, topic, subscription).get();
			int i = 0;
			MessageId seekTo = null;
			MessageId lastMessageId = reader.LastMessageIdAsync.get();

			while (true)
			{
				using (RawMessage m = reader.readNextAsync().get())
				{
					i++;
					if (i > numKeys / 2)
					{
						if (seekTo == null)
						{
							seekTo = m.MessageId;
						}
						readKeys.Add(extractKey(m));
					}
					if (lastMessageId.CompareTo(m.MessageId) == 0)
					{
						break;
					}
				}
			}
			Assert.assertEquals(readKeys.Count, numKeys / 2);

			// seek to middle, read all keys again,
			// assert that we read all keys we had read previously
			reader.seekAsync(seekTo).get();
			while (true)
			{ // should break out with TimeoutException
				using (RawMessage m = reader.readNextAsync().get())
				{
					Assert.assertTrue(readKeys.remove(extractKey(m)));
					if (lastMessageId.CompareTo(m.MessageId) == 0)
					{
						break;
					}
				}
			}
			Assert.assertTrue(readKeys.Count == 0);
		}

		/// <summary>
		/// Try to fill the receiver queue, and drain it multiple times
		/// </summary>
		/// 
		public virtual void testFlowControl()
		{
			int numMessages = RawReaderImpl.DEFAULT_RECEIVER_QUEUE_SIZE * 5;
			string topic = "persistent://my-property/my-ns/my-raw-topic";

			publishMessages(topic, numMessages);

			RawReader reader = RawReader.create(pulsarClient, topic, subscription).get();
			IList<Future<RawMessage>> futures = new List<Future<RawMessage>>();
			ISet<string> keys = new HashSet<string>();

			// +1 to make sure we read past the end
			for (int i = 0; i < numMessages + 1; i++)
			{
				futures.Add(reader.readNextAsync());
			}
			int timeouts = 0;
			foreach (Future<RawMessage> f in futures)
			{
				try
				{
						using (RawMessage m = f.get(1, TimeUnit.SECONDS))
						{
						// Assert each key is unique
						string key = extractKey(m);
						Assert.assertTrue(keys.Add(key), "Received duplicated key '" + key + "' : already received keys = " + keys);
						}
				}
				catch (TimeoutException)
				{
					timeouts++;
				}
			}
			Assert.assertEquals(timeouts, 1);
			Assert.assertEquals(keys.Count, numMessages);
		}


		public virtual void testFlowControlBatch()
		{
			int numMessages = RawReaderImpl.DEFAULT_RECEIVER_QUEUE_SIZE * 5;
			string topic = "persistent://my-property/my-ns/my-raw-topic";

			publishMessages(topic, numMessages, true);

			RawReader reader = RawReader.create(pulsarClient, topic, subscription).get();
			ISet<string> keys = new HashSet<string>();

			while (true)
			{
				try
				{
						using (RawMessage m = reader.readNextAsync().get(1, TimeUnit.SECONDS))
						{
						Assert.assertTrue(RawBatchConverter.isReadableBatch(m));
						IList<ImmutableTriple<MessageId, string, int>> batchKeys = RawBatchConverter.extractIdsAndKeysAndSize(m);
						// Assert each key is unique
						foreach (ImmutableTriple<MessageId, string, int> pair in batchKeys)
						{
							string key = pair.middle;
							Assert.assertTrue(keys.Add(key), "Received duplicated key '" + key + "' : already received keys = " + keys);
						}
						}
				}
				catch (TimeoutException)
				{
					break;
				}
			}
			Assert.assertEquals(keys.Count, numMessages);
		}


		public virtual void testBatchingExtractKeysAndIds()
		{
			string topic = "persistent://my-property/my-ns/my-raw-topic";

			using (Producer<sbyte[]> producer = pulsarClient.newProducer().topic(topic).maxPendingMessages(3).enableBatching(true).batchingMaxMessages(3).batchingMaxPublishDelay(1, TimeUnit.HOURS).messageRoutingMode(MessageRoutingMode.SinglePartition).create())
			{
				producer.newMessage().key("key1").value("my-content-1".GetBytes()).sendAsync();
				producer.newMessage().key("key2").value("my-content-2".GetBytes()).sendAsync();
				producer.newMessage().key("key3").value("my-content-3".GetBytes()).send();
			}

			RawReader reader = RawReader.create(pulsarClient, topic, subscription).get();
			try
			{
					using (RawMessage m = reader.readNextAsync().get())
					{
					IList<ImmutableTriple<MessageId, string, int>> idsAndKeys = RawBatchConverter.extractIdsAndKeysAndSize(m);
        
					Assert.assertEquals(idsAndKeys.Count, 3);
        
					// assert message ids are in correct order
					Assert.assertTrue(idsAndKeys[0].Left.compareTo(idsAndKeys[1].Left) < 0);
					Assert.assertTrue(idsAndKeys[1].Left.compareTo(idsAndKeys[2].Left) < 0);
        
					// assert keys are as expected
					Assert.assertEquals(idsAndKeys[0].Middle, "key1");
					Assert.assertEquals(idsAndKeys[1].Middle, "key2");
					Assert.assertEquals(idsAndKeys[2].Middle, "key3");
					}
			}
			finally
			{
				reader.closeAsync().get();
			}
		}


		public virtual void testBatchingRebatch()
		{
			string topic = "persistent://my-property/my-ns/my-raw-topic";

			using (Producer<sbyte[]> producer = pulsarClient.newProducer().topic(topic).maxPendingMessages(3).enableBatching(true).batchingMaxMessages(3).batchingMaxPublishDelay(1, TimeUnit.HOURS).messageRoutingMode(MessageRoutingMode.SinglePartition).create())
			{
				producer.newMessage().key("key1").value("my-content-1".GetBytes()).sendAsync();
				producer.newMessage().key("key2").value("my-content-2".GetBytes()).sendAsync();
				producer.newMessage().key("key3").value("my-content-3".GetBytes()).send();
			}

			RawReader reader = RawReader.create(pulsarClient, topic, subscription).get();
			try
			{
					using (RawMessage m1 = reader.readNextAsync().get())
					{
					RawMessage m2 = RawBatchConverter.rebatchMessage(m1, (key, id) => key.Equals("key2")).get();
					IList<ImmutableTriple<MessageId, string, int>> idsAndKeys = RawBatchConverter.extractIdsAndKeysAndSize(m2);
					Assert.assertEquals(idsAndKeys.Count, 1);
					Assert.assertEquals(idsAndKeys[0].Middle, "key2");
					m2.close();
					Assert.assertEquals(m1.HeadersAndPayload.refCnt(), 1);
					}
			}
			finally
			{
				reader.closeAsync().get();
			}
		}


		public virtual void testAcknowledgeWithProperties()
		{
			int numKeys = 10;

			string topic = "persistent://my-property/my-ns/my-raw-topic";

			ISet<string> keys = publishMessages(topic, numKeys);

			RawReader reader = RawReader.create(pulsarClient, topic, subscription).get();
			MessageId lastMessageId = reader.LastMessageIdAsync.get();

			while (true)
			{
				using (RawMessage m = reader.readNextAsync().get())
				{
					Assert.assertTrue(keys.remove(extractKey(m)));

					if (lastMessageId.CompareTo(m.MessageId) == 0)
					{
						break;
					}
				}
			}
			Assert.assertTrue(keys.Count == 0);

			IDictionary<string, long> properties = new Dictionary<string, long>();
			properties["foobar"] = 0xdeadbeefdecaL;
			reader.acknowledgeCumulativeAsync(lastMessageId, properties).get();

			PersistentTopic topicRef = (PersistentTopic) pulsar.BrokerService.getTopicReference(topic).get();
			ManagedLedger ledger = topicRef.ManagedLedger;
			for (int i = 0; i < 30; i++)
			{
				if (ledger.openCursor(subscription).Properties["foobar"] == Convert.ToInt64(0xdeadbeefdecaL))
				{
					break;
				}
				Thread.Sleep(100);
			}
			Assert.assertEquals(ledger.openCursor(subscription).Properties["foobar"], Convert.ToInt64(0xdeadbeefdecaL));
		}


		public virtual void testReadCancellationOnClose()
		{
			int numKeys = 10;

			string topic = "persistent://my-property/my-ns/my-raw-topic";
			publishMessages(topic, numKeys / 2);

			RawReader reader = RawReader.create(pulsarClient, topic, subscription).get();
			IList<Future<RawMessage>> futures = new List<Future<RawMessage>>();
			for (int i = 0; i < numKeys; i++)
			{
				futures.Add(reader.readNextAsync());
			}

			for (int i = 0; i < numKeys / 2; i++)
			{
				futures.RemoveAt(0).get(); // complete successfully
			}
			reader.closeAsync().get();
			while (futures.Count > 0)
			{
				try
				{
					futures.RemoveAt(0).get();
					Assert.fail("Should have been cancelled");
				}
				catch (CancellationException)
				{
					// correct behaviour
				}
			}
		}
	}

}