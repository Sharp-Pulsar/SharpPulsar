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
		public virtual void TestReadMessageWithBatching()
		{
			string topic = $"my-reader-topic-with-batching-{Guid.NewGuid()}";
			TestReadMessages(topic, true);
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
			for (var i = 0; i < numKeys; i++)
			{
				var message = (Message<sbyte[]>)reader.ReadNext();
				if(message != null)
				{
					_output.WriteLine($"{message.Key}:{message.MessageId}:{Encoding.UTF8.GetString(message.Data.ToBytes())}");
					Assert.True(keys.Remove(message.Key));
				}
				else
					break;
			}
			Assert.True(keys.Count == 0);
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

			for (var i = 0; i < numKeys; i++)
			{
				var message = reader.ReadNext();
				Assert.True(keys.Remove(message.Key));
			}
			Assert.True(keys.Count == 0);
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
			
			foreach (string key in keys)
			{
				int slot = Murmur332Hash.Instance.MakeHash(Encoding.UTF8.GetBytes(key).ToSBytes()) % rangeSize;
				producer.NewMessage().Key(key).Value(key).Send();
				_output.WriteLine($"Publish message to slot {slot}");
			}

			IList<string> receivedMessages = new List<string>();

			IMessage<string> msg;
			Thread.Sleep(TimeSpan.FromSeconds(30));
			do
			{
				msg = reader.ReadNext(TimeSpan.FromSeconds(1));
				if (msg != null)
				{
					receivedMessages.Add(msg.Value);
				}
			} while (msg != null);

			Assert.True(receivedMessages.Count > 0);

			foreach (string receivedMessage in receivedMessages)
			{
				_output.WriteLine($"Receive message {receivedMessage}");
				Assert.True(Convert.ToInt32(receivedMessage) <= rangeSize / 2);
			}

		}
	}

}