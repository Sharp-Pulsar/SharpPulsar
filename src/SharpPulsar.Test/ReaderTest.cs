﻿using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using Xunit.Abstractions;
using SharpPulsar.Interfaces;
using SharpPulsar.Common.Util;
using SharpPulsar.TestContainer;
using System.Threading.Tasks;
using SharpPulsar.Test.Fixture;
using SharpPulsar.Builder;

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
    [Collection(nameof(PulsarCollection))]
    public class ReaderTest : IAsyncLifetime
    {

        private const string Subscription = "reader-sub";
        private readonly ITestOutputHelper _output;

        private PulsarClient _client;
        private PulsarSystem _system;
        private PulsarClientConfigBuilder _configBuilder;

        public ReaderTest(ITestOutputHelper output, PulsarFixture fixture)
        {
            _output = output;
            _configBuilder = fixture.ConfigBuilder;
            _system = fixture.System;
        }
        private async Task<ISet<string>> PublishMessages(string topic, int count, bool enableBatch)
        {
            ISet<string> keys = new HashSet<string>();
            var builder = new ProducerConfigBuilder<byte[]>()
                .Topic(topic)
                .MessageRoutingMode(Common.MessageRoutingMode.RoundRobinMode)
                .MaxPendingMessages(count)
                .BatchingMaxPublishDelay(TimeSpan.FromMilliseconds(800000));
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
        [Fact]
        public virtual async Task TestReadMessageWithoutBatching()
        {
            var topic = $"my-reader-topic-{Guid.NewGuid()}";
            await TestReadMessages(topic, false);
            _client.Dispose();
        }

        [Fact]
        public virtual async Task TestReadMessageWithBatching()
        {
            var topic = $"my-reader-topic-with-batching-{Guid.NewGuid()}";
            await TestReadMessages(topic, true);
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
            await Task.Delay(TimeSpan.FromSeconds(1));
            for (var i = 0; i < numKeys-2; i++)
            {
                var message = (Message<byte[]>)await reader.ReadNextAsync();
                if (message != null)
                {
                    _output.WriteLine($"{message.Key}:{message.MessageId}:{Encoding.UTF8.GetString(message.Data)}");
                    Assert.True(keys.Remove(message.Key));
                }
                else
                    break;
            }
            Assert.True(keys.Count == 2);
        }
        [Fact]
        public virtual async Task TestReadFromPartition()
        {
            var topic = "testReadFromPartition";
            var partition0 = topic + "-partition-0";
            var numKeys = 10;

            var keys = await PublishMessages(partition0, numKeys, false);

            var builder = new ReaderConfigBuilder<byte[]>()
                .Topic(partition0)
                .StartMessageId(IMessageId.Earliest)
                .ReaderName(Subscription);
            var reader = await _client.NewReaderAsync(builder);

            await Task.Delay(TimeSpan.FromSeconds(2));
            for (var i = 0; i < numKeys; i++)
            {
                var message = await reader.ReadNextAsync();
                Assert.True(keys.Remove(message.Key));
                _output.WriteLine($"{message.Key}");
            }
            Assert.True(keys.Count == 0);
        }

        [Fact]
        public virtual async Task TestKeyHashRangeReader()
        {
            var rangeSize = 2 << 15;
            IList<string> keys = new List<string> { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };
            var topic = $"testKeyHashRangeReader-{Guid.NewGuid()}";

            try
            {
                _ = await _client.NewReaderAsync(new ReaderConfigBuilder<byte[]>()
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
                _ = await _client.NewReaderAsync(new ReaderConfigBuilder<byte[]>()
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

                _ = await _client.NewReaderAsync(new ReaderConfigBuilder<byte[]>()
                    .Topic(topic)
                    .StartMessageId(IMessageId.Earliest)
                    .KeyHashRange(Common.Range.Of(80000, 90000)));

                Assert.False(false, "should failed with unexpected key hash range");
            }
            catch (ArgumentException e)
            {
                _output.WriteLine("Create key hash range failed", e);
            }

            var reader = await _client.NewReaderAsync(ISchema<object>.String, new ReaderConfigBuilder<string>()
                    .Topic(topic)
                    .StartMessageId(IMessageId.Earliest)
                    .KeyHashRange(Common.Range.Of(0, rangeSize / 2)));

            var producer = await _client.NewProducerAsync(ISchema<object>.String, new ProducerConfigBuilder<string>()
                .Topic(topic).EnableBatching(false));

            foreach (var key in keys)
            {
                var slot = Murmur332Hash.Instance.MakeHash(Encoding.UTF8.GetBytes(key)) % rangeSize;
                await producer.NewMessage().Key(key).Value(key).SendAsync();
                _output.WriteLine($"Publish message to slot {slot}");
            }

            IList<string> receivedMessages = new List<string>();

            IMessage<string> msg;
            await Task.Delay(TimeSpan.FromSeconds(5));
            for (var i = 0; i < 3; i++)
            {
                msg = await reader.ReadNextAsync(TimeSpan.FromSeconds(1)); 
                receivedMessages.Add(msg.Value);
            }
            Assert.True(receivedMessages.Count > 0);

            foreach (var receivedMessage in receivedMessages)
            {
                _output.WriteLine($"Receive message {receivedMessage}");
                Assert.True(Convert.ToInt32(receivedMessage) <= rangeSize / 2);
            }
        }
        public async Task InitializeAsync()
        {

            _client = await _system.NewClient(_configBuilder);
        }

        public async Task DisposeAsync()
        {
            await _client.ShutdownAsync();
        }
    }

}