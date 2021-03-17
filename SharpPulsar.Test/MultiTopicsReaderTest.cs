using SharpPulsar.Configuration;
using SharpPulsar.Test.Fixtures;
using SharpPulsar.User;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using Xunit.Abstractions;
using SharpPulsar.Extension;
using System.Threading;
using SharpPulsar.Interfaces;
using BAMCIS.Util.Concurrent;
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
	public class MultiTopicsReaderTest
	{

		private const string Subscription = "reader-multi-topics-sub";
		private readonly ITestOutputHelper _output;
		private readonly PulsarClient _client;

		public MultiTopicsReaderTest(ITestOutputHelper output, PulsarStandaloneClusterFixture fixture)
		{
			_output = output;
			_client = fixture.Client;
		}
		[Fact]
		public virtual void TestReadMessageWithoutBatching()
		{
			string topic = "TestReadMessageWithoutBatching";
			TestReadMessages(topic, false);
		}
		[Fact]
		public virtual void TestReadMessageWithoutBatchingWithMessageInclusive()
		{
			string topic = "TestReadMessageWithoutBatchingWithMessageInclusive";
			ISet<string> keys = PublishMessages(topic, 10, false);

			var builder = new ReaderConfigBuilder<sbyte[]>()
				.Topic(topic)
				.StartMessageId(IMessageId.Latest)
				.StartMessageIdInclusive()
				.ReaderName(Subscription);
			var reader = _client.NewReader(builder); int count = 0;
			while(reader.HasMessageAvailable())
			{
				var message = reader.ReadNext(1, TimeUnit.SECONDS);
				Assert.True(keys.Remove(message.Key));
				count++;
			}
			Assert.Equal(10, count);
			Assert.False(reader.HasMessageAvailable());
		}
		[Fact]
		public virtual void TestReadMessageWithBatching()
		{
			string topic = "TestReadMessageWithBatching";
			TestReadMessages(topic, true);
		}
		[Fact]
		public virtual void TestReadMessageWithBatchingWithMessageInclusive()
		{
			string topic = "TestReadMessageWithBatchingWithMessageInclusive";
			int topicNum = 3;
			int msgNum = 15;
			ISet<string> keys = PublishMessages(topic, msgNum, true);

			var builder = new ReaderConfigBuilder<sbyte[]>()
				.Topic(topic)
				.StartMessageId(IMessageId.Latest)
				.StartMessageIdInclusive()
				.ReaderName(Subscription);
			var reader = _client.NewReader(builder);
			while (reader.HasMessageAvailable())
			{
				var message = reader.ReadNext(2, TimeUnit.SECONDS);
				if (message != null)
				{
					var removed = keys.Remove(message.Key);
					Assert.True(removed);
				}
				else
					break;
			}

			// start from latest with start message inclusive should only read the last 3 message from 3 partition
			Assert.Equal(keys.Count, msgNum - topicNum);
			Assert.False(keys.Contains("key14"));
			Assert.False(keys.Contains("key13"));
			Assert.False(keys.Contains("key12"));
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
			for (var i = 0; i < numKeys; i++)
			{
				var message = (Message<sbyte[]>)reader.ReadNext();
				if (message != null)
				{
					_output.WriteLine($"{message.Key}:{message.MessageId}:{Encoding.UTF8.GetString(message.Data.ToBytes())}");
					Assert.True(keys.Remove(message.Key));
				}
				else
					break;
			}
			Assert.True(keys.Count == 0);
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
	}

}