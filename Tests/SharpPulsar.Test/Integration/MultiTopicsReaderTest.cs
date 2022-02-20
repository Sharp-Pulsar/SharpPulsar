using SharpPulsar.Configuration;
using SharpPulsar.User;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using Xunit.Abstractions;
using System.Threading;
using SharpPulsar.Interfaces;
using SharpPulsar.TestContainer;
using System.Threading.Tasks;
using SharpPulsar.Test.Fixture;
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
namespace SharpPulsar.Test.Integration
{

    [Collection(nameof(IntegrationCollection))]
	public class MultiTopicsReaderTest
	{

		private const string Subscription = "reader-multi-topics-sub";
		private readonly ITestOutputHelper _output;
		private readonly PulsarClient _client;

		public MultiTopicsReaderTest(ITestOutputHelper output, PulsarFixture fixture)
		{
			_output = output;
			_client = fixture.Client;
		}
		[Fact]
		public virtual async Task TestReadMessageWithoutBatching()
		{
			var topic = "ReadMessageWithoutBatching";
			await TestReadMessages(topic, false);
		}
		[Fact]
		public virtual async Task TestReadMessageWithBatching()
		{
			var topic = $"ReadMessageWithBatching_{Guid.NewGuid()}";
			await TestReadMessages(topic, true);
		}
		[Fact(Timeout = 1000)]
		public async Task TestMultiTopic()
		{
			var topic = "persistent://public/default/topic" + Guid.NewGuid();

			var topic2 = "persistent://public/default/topic2" + Guid.NewGuid();

			var topic3 = "persistent://public/default/topic3" + Guid.NewGuid();
			IList<string> topics = new List<string> { topic, topic2, topic3 };
			var builder = new ReaderConfigBuilder<string>()
				.Topics(topics)
				.StartMessageId(IMessageId.Earliest)
                
				.ReaderName("my-reader");

			var reader = await _client.NewReaderAsync(ISchema<object>.String, builder);
			// create producer and send msg
			IList<Producer<string>> producerList = new List<Producer<string>>();
			foreach (var topicName in topics)
			{
                var producer = await _client.NewProducerAsync(ISchema<object>.String, new ProducerConfigBuilder<string>().Topic(topicName));

				producerList.Add(producer);
			}
			var msgNum = 10;
			ISet<string> messages = new HashSet<string>();
			for (var i = 0; i < producerList.Count; i++)
			{
				var producer = producerList[i];
				for (var j = 0; j < msgNum; j++)
				{
					var msg = i + "msg" + j;
					await producer.SendAsync(msg);
					messages.Add(msg);
				}
			}
			// receive messagesS
			var message = await reader.ReadNextAsync(TimeSpan.FromSeconds(5));
			while (message != null)
			{
				var value = message.Value;
				_output.WriteLine(value);
				Assert.True(messages.Remove(value));
				message = await reader.ReadNextAsync(TimeSpan.FromSeconds(5));
			}
			Assert.True(messages.Count == 0 || messages.Count == 1);
			// clean up
			foreach (var producer in producerList)
			{
				await producer.CloseAsync();
			}
			await reader.CloseAsync();
		}
		private async Task TestReadMessages(string topic, bool enableBatch)
		{
			var numKeys = 10;
            var builder = new ReaderConfigBuilder<byte[]>()
                .Topic(topic)
                
                .StartMessageId(IMessageId.Earliest)
                .ReaderName(Subscription);
            var reader = await _client.NewReaderAsync(builder);

            var keys = await PublishMessages(topic, numKeys, enableBatch);
			await Task.Delay(TimeSpan.FromSeconds(5));
			for (var i = 0; i < numKeys; i++)
			{
				var message = await reader.ReadNextAsync();
				if (message != null)
				{
					_output.WriteLine($"{message.Key}:{message.MessageId}:{Encoding.UTF8.GetString(message.Data)}");
					Assert.True(keys.Remove(message.Key));
				}
			}
			Assert.True(keys.Count == 0);
		}
		private async Task<ISet<string>> PublishMessages(string topic, int count, bool enableBatch)
		{
			ISet<string> keys = new HashSet<string>();
			var builder = new ProducerConfigBuilder<byte[]>()
				.Topic(topic)
				.MessageRoutingMode(Common.MessageRoutingMode.RoundRobinMode)
				.MaxPendingMessages(count)
				.BatchingMaxPublishDelay(TimeSpan.FromMilliseconds(80000));
			if (enableBatch)
			{
				builder.EnableBatching(true);
				builder.BatchingMaxMessages(count);
			}
			else
			{
				builder.EnableBatching(false);
			}

			var producer = await _client.NewProducerAsync(builder);
			for (var i = 0; i < count; i++)
			{
				var key = "key" + i;
				var data = Encoding.UTF8.GetBytes("my-message-" + i);
				await producer.NewMessage().Key(key).Value(data).SendAsync();
				keys.Add(key);
			}
			producer.Flush();
			return keys;
		}
	}

}