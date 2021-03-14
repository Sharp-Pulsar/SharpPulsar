using SharpPulsar.Configuration;
using SharpPulsar.Test.Fixtures;
using SharpPulsar.User;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using Xunit.Abstractions;
using SharpPulsar.Extension;
using SharpPulsar.Interfaces;
using BAMCIS.Util.Concurrent;
using SharpPulsar.Common.Util;
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
	[Collection(nameof(PulsarTests))]
	public class ReaderTest
	{

		private const string Subscription = "reader-sub";
		private readonly ITestOutputHelper _output;
		private readonly PulsarClient _client;

		public ReaderTest(ITestOutputHelper output, PulsarStandaloneClusterFixture fixture)
		{
			_output = output;
			_client = fixture.Client;
		}
		private ISet<string> PublishMessages(string topic, int count, bool enableBatch)
		{
			ISet<string> keys = new HashSet<string>();
			var builder = new ProducerConfigBuilder<sbyte[]>()
				.Topic(topic)
				.MessageRoutingMode(Common.MessageRoutingMode.RoundRobinMode)
				.MaxPendingMessages(count)
				.BatchingMaxPublishDelay(86400000);
			if (enableBatch)
			{
				builder.EnableBatching(true);
				builder.BatchingMaxMessages(count);
			}
			else
			{
				builder.EnableBatching(false);
			}
			
			var producer = _client.NewProducer(builder); 
			for (int i = 0; i < count; i++)
			{
				string key = "key" + i;
				sbyte[] data = Encoding.UTF8.GetBytes("my-message-" + i).ToSBytes();
				producer.NewMessage().Key(key).Value(data).Send();
				keys.Add(key);
			}
			producer.Flush();
			return keys;
		}
		[Fact]
		public virtual void TestReadMessageWithoutBatching()
		{
			string topic = $"my-reader-topic-{Guid.NewGuid()}";
			TestReadMessages(topic, false);
		}

		[Fact]
		public virtual void TestReadMessageWithoutBatchingWithMessageInclusive()
		{
			string topic = $"my-reader-topic-inclusive-{Guid.NewGuid()}";
			ISet<string> keys = PublishMessages(topic, 10, false);
			var builder = new ReaderConfigBuilder<sbyte[]>()
				.Topic(topic)
				.StartMessageId(IMessageId.Latest)
				.StartMessageIdInclusive()
				.ReaderName(Subscription);
			var reader = _client.NewReader(builder);
			var available = reader.HasMessageAvailable();
			var removed = keys.Remove(reader.ReadNext().Key);
			available = reader.HasMessageAvailable();
			Assert.True(available);
			Assert.True(removed);
			Assert.False(available);
		}
		[Fact]
		public virtual void TestReadMessageWithBatching()
		{
			string topic = $"my-reader-topic-with-batching-{Guid.NewGuid()}";
			TestReadMessages(topic, true);
		}
		[Fact]
		public virtual void TestReadMessageWithBatchingWithMessageInclusive()
		{
			string topic = $"my-reader-topic-with-batching-inclusive-{Guid.NewGuid()}";
			ISet<string> keys = PublishMessages(topic, 10, true);
			var builder = new ReaderConfigBuilder<sbyte[]>()
				.Topic(topic)
				.StartMessageId(IMessageId.Latest)
				.StartMessageIdInclusive()
				.ReaderName(Subscription);
			var reader = _client.NewReader(builder);
			Thread.Sleep(TimeSpan.FromSeconds(30));
			while (reader.HasMessageAvailable())
			{
				var removed = keys.Remove(reader.ReadNext().Key);
				Assert.True(removed);
			}
			// start from latest with start message inclusive should only read the last message in batch
			Assert.True(keys.Count == 9);
			Assert.False(keys.Contains("key9"));
			Assert.False(reader.HasMessageAvailable());
		}

		private void TestReadMessages(string topic, bool enableBatch)
		{
			int numKeys = 10;

			ISet<string> keys = PublishMessages(topic, numKeys, enableBatch);
			var builder = new ReaderConfigBuilder<sbyte[]>()
				.Topic(topic)
				.StartMessageId(IMessageId.Earliest)
				.ReaderName(Subscription);
			var reader = _client.NewReader(builder);
			Thread.Sleep(TimeSpan.FromSeconds(30));
			while (reader.HasMessageAvailable())
			{
				var message = (Message<sbyte[]>)reader.ReadNext();
				_output.WriteLine($"{message.Key}:{message.MessageId}:{Encoding.UTF8.GetString(message.Data.ToBytes())}");
				Assert.True(keys.Remove(message.Key));
			}
			Assert.True(keys.Count == 0);

			var builderLatest = new ReaderConfigBuilder<sbyte[]>()
				.Topic(topic)
				.StartMessageId(IMessageId.Latest)
				.ReaderName(Subscription + "latest");
			var readerLatest = _client.NewReader(builderLatest);
			Assert.False(readerLatest.HasMessageAvailable());
		}
		[Fact]
		public virtual void TestReadFromPartition()
		{
			string topic = "testReadFromPartition";
			string partition0 = topic + "-partition-0";
			int numKeys = 10;

			ISet<string> keys = PublishMessages(partition0, numKeys, false);
			var builder = new ReaderConfigBuilder<sbyte[]>()
				.Topic(partition0)
				.StartMessageId(IMessageId.Earliest)
				.ReaderName(Subscription);
			var reader = _client.NewReader(builder);

			while (reader.HasMessageAvailable())
			{
				var message = reader.ReadNext();
				Assert.True(keys.Remove(message.Key));
			}
			Assert.True(keys.Count == 0);
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
		[Fact]
		public virtual void TestReaderWithTimeLong()
		{
			string ns = "public/default";
			string topic = "persistent://" + ns + "/testReadFromPartition";
			var builder = new ProducerConfigBuilder<sbyte[]>()
				.Topic(topic)
				.EnableBatching(false);

			var producer = _client.NewProducer(builder);
			MessageId lastMsgId = null;
			int totalMsg = 10;
			// (1) Publish 10 messages with publish-time 5 HOUR back
			long oldMsgPublishTime = DateTimeHelper.CurrentUnixTimeMillis() - TimeUnit.HOURS.ToMilliseconds(5); // 5 hours old
			for (int i = 0; i < totalMsg; i++)
			{
				var val = Encoding.UTF8.GetBytes("old" + i).ToSBytes();
				var msg = (TypedMessageBuilder<sbyte[]>)producer.NewMessage().Value(val);
				var metadata = msg.Metadata;
				metadata.PublishTime = (ulong)oldMsgPublishTime;
				metadata.SequenceId = (ulong)i;
				metadata.ProducerName = producer.ProducerName;
				metadata.ReplicatedFrom = "us-west1";
				msg.Send();
				var receipt = producer.SendReceipt();
				lastMsgId = new MessageId(receipt.LedgerId, receipt.EntryId, 0);
			}

			// (2) Publish 10 messages with publish-time 1 HOUR back
			long newMsgPublishTime = DateTimeHelper.CurrentUnixTimeMillis() - TimeUnit.HOURS.ToMilliseconds(1); // 1 hour old
			MessageId firstMsgId = null;
			for (int i = 0; i < totalMsg; i++)
			{
				var val = Encoding.UTF8.GetBytes("new" + i).ToSBytes();
				var msg = (TypedMessageBuilder<sbyte[]>)producer.NewMessage().Value(val);
				var metadata = msg.Metadata;
				metadata.PublishTime = (ulong)newMsgPublishTime;
				metadata.ProducerName = producer.ProducerName;
				metadata.ReplicatedFrom = "us-west1";
				msg.Send();
				var receipt = producer.SendReceipt();
				if (firstMsgId == null)
				{
					firstMsgId = new MessageId(receipt.LedgerId, receipt.EntryId, 0);
				}
			}

			// (3) Create reader and set position 1 hour back so, it should only read messages which are 2 hours old which
			// published on step 2
			var rbuilder = new ReaderConfigBuilder<sbyte[]>()
				.Topic(topic)
				.StartMessageId(IMessageId.Earliest)
				.StartMessageFromRollbackDuration((int)TimeSpan.FromHours(2).TotalMilliseconds);
			var reader = _client.NewReader(rbuilder);

			var receivedMessageIds = new List<IMessageId>();

			while (reader.HasMessageAvailable())
			{
				var msg = reader.ReadNext(1, TimeUnit.SECONDS);
				if (msg == null)
				{
					break;
				}
				_output.WriteLine("msg.getMessageId()=" + msg.MessageId + ", data=" + (Encoding.UTF8.GetString(msg.Data.ToBytes())));
				receivedMessageIds.Add(msg.MessageId);
			}

			Assert.Equal(receivedMessageIds.Count, totalMsg);
			Assert.Equal(receivedMessageIds[0], firstMsgId);
		}
		[Fact]
		public virtual void TestKeyHashRangeReader()
		{
			var rangeSize = (2 << 15);
			IList<string> keys = new List<string> { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };
			string topic = $"testKeyHashRangeReader-{Guid.NewGuid()}";

			try
			{
				_ = _client.NewReader(new ReaderConfigBuilder<sbyte[]>()
					.Topic(topic)
					.StartMessageId(IMessageId.Earliest)
					.KeyHashRange(Common.Range.Of(0, 10000), Common.Range.Of(8000, 12000)));
				Assert.False(false, "should failed with unexpected key hash range");
			}
			catch (ArgumentException e)
			{
				_output.WriteLine("Create key hash range failed", e);
			}

			try
			{
				_ = _client.NewReader(new ReaderConfigBuilder<sbyte[]>()
					.Topic(topic)
					.StartMessageId(IMessageId.Earliest)
					.KeyHashRange(Common.Range.Of(30000, 20000)));
				Assert.False(false, "should failed with unexpected key hash range");
			}
			catch (ArgumentException e)
			{
				_output.WriteLine("Create key hash range failed", e);
			}

			try
			{

				_ = _client.NewReader(new ReaderConfigBuilder<sbyte[]>()
					.Topic(topic)
					.StartMessageId(IMessageId.Earliest)
					.KeyHashRange(Common.Range.Of(80000, 90000)));

				Assert.False(false, "should failed with unexpected key hash range");
			}
			catch (ArgumentException e)
			{
				_output.WriteLine("Create key hash range failed", e);
			}
			var reader = _client.NewReader(ISchema<object>.String, new ReaderConfigBuilder<string>()
					.Topic(topic)
					.StartMessageId(IMessageId.Earliest)
					.KeyHashRange(Common.Range.Of(0, (rangeSize / 2))));
						
			var producer = _client.NewProducer(ISchema<object>.String, new ProducerConfigBuilder<string>()
				.Topic(topic).EnableBatching(false));

			int expectedMessages = 0;
			foreach (string key in keys)
			{
				int slot = Murmur332Hash.Instance.MakeHash(Encoding.UTF8.GetBytes(key).ToSBytes()) % rangeSize;
				if (slot <= rangeSize / 2)
				{
					expectedMessages++;
				}
				producer.NewMessage().Key(key).Value(key).Send();
				_output.WriteLine($"Publish message to slot {slot}");
			}

			IList<string> receivedMessages = new List<string>();

			IMessage<string> msg;
			do
			{
				msg = reader.ReadNext(1, TimeUnit.SECONDS);
				if (msg != null)
				{
					receivedMessages.Add(msg.Value);
				}
			} while (msg != null);

			Assert.True(expectedMessages > 0);
			Assert.Equal(receivedMessages.Count, expectedMessages);
			foreach (string receivedMessage in receivedMessages)
			{
				_output.WriteLine($"Receive message {receivedMessage}");
				Assert.True(Convert.ToInt32(receivedMessage) <= rangeSize / 2);
			}

		}
	}

}