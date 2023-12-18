using System.Collections.Generic;
using System.Text;
using Xunit.Abstractions;
using System;
using SharpPulsar.TestContainer;
using SharpPulsar.Builder;
using SharpPulsar.Test.Fixture;
using System.Threading.Tasks;
using SharpPulsar.Interfaces;
using Xunit;

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


    /// <summary>
    /// Unit Tests of <seealso cref="MultiTopicsConsumer{T}"/>.
    /// </summary>
    [Collection(nameof(PulsarCollection))]
    public class MultiTopicsConsumerTest : IAsyncLifetime
    {
        private const string Subscription = "reader-multi-topics-sub";
        private PulsarClient _client;
        private readonly ITestOutputHelper _output;
        private PulsarSystem _system;
        private PulsarClientConfigBuilder _configBuilder;
        public MultiTopicsConsumerTest(ITestOutputHelper output, PulsarFixture fixture)
        {
            _output = output;
            _configBuilder = fixture.ConfigBuilder;
            _system = fixture.System;
        }
        [Fact]
        public async Task TestMultiTopicConsumer()
        {
            var messageCount = 5;
            var first = $"one-topic-{Guid.NewGuid()}";
            var second = $"two-topic-{Guid.NewGuid()}";
            var third = $"three-topic-{Guid.NewGuid()}";

            await PublishMessages(first, messageCount, "hello Toba");//.ConfigureAwait(false); https://xunit.net/xunit.analyzers/rules/xUnit1030
            await PublishMessages(third, messageCount, "hello Toba");//.ConfigureAwait(false); https://xunit.net/xunit.analyzers/rules/xUnit1030
            await PublishMessages(second, messageCount, "hello Toba");//.ConfigureAwait(false); https://xunit.net/xunit.analyzers/rules/xUnit1030
            var builder = new ConsumerConfigBuilder<byte[]>()
                .Topic(first, second, third)
                .ForceTopicCreation(true)
                .SubscriptionName("multi-topic-sub");

            var consumer = await _client.NewConsumerAsync(builder);//.ConfigureAwait(false); https://xunit.net/xunit.analyzers/rules/xUnit1030
            await Task.Delay(TimeSpan.FromSeconds(1));
            var received = 0;
            for (var i = 0; i < messageCount; i++)
            {
                var message = (TopicMessage<byte[]>)await consumer.ReceiveAsync(TimeSpan.FromMilliseconds(1000));//.ConfigureAwait(false); https://xunit.net/xunit.analyzers/rules/xUnit1030
                if (message != null)
                {
                    await consumer.AcknowledgeAsync(message);//.ConfigureAwait(false); https://xunit.net/xunit.analyzers/rules/xUnit1030
                    _output.WriteLine($"message from topic: {message.Topic}");
                    received++;
                }
            }
            for (var i = 0; i < messageCount; i++)
            {
                var message = (TopicMessage<byte[]>)await consumer.ReceiveAsync(TimeSpan.FromMicroseconds(5000));//.ConfigureAwait(false); https://xunit.net/xunit.analyzers/rules/xUnit1030
                if (message != null)
                {
                    await consumer.AcknowledgeAsync(message);//.ConfigureAwait(false); https://xunit.net/xunit.analyzers/rules/xUnit1030
                    _output.WriteLine($"message from topic: {message.Topic}");
                    received++;
                }
            }
            for (var i = 0; i < messageCount; i++)
            {
                var message = (TopicMessage<byte[]>)await consumer.ReceiveAsync(TimeSpan.FromMicroseconds(5000));//.ConfigureAwait(false); https://xunit.net/xunit.analyzers/rules/xUnit1030
                if (message != null)
                {
                    await consumer.AcknowledgeAsync(message);//.ConfigureAwait(false); https://xunit.net/xunit.analyzers/rules/xUnit1030
                    _output.WriteLine($"message from topic: {message.Topic}");
                    received++;
                }
            }
            Assert.True(received > 0);
            
            await consumer.CloseAsync();//.ConfigureAwait(false); https://xunit.net/xunit.analyzers/rules/xUnit1030
        }

        private async Task<List<MessageId>> PublishMessages(string topic, int count, string message)
        {
            var keys = new List<MessageId>();
            var builder = new ProducerConfigBuilder<byte[]>()
                .Topic(topic);
            var producer = await _client.NewProducerAsync(builder);
            for (var i = 0; i < count; i++)
            {
                var key = "key" + i;
                var data = Encoding.UTF8.GetBytes($"{message}-{i}");
                var id = await producer.NewMessage().Key(key).Value(data).SendAsync();
                keys.Add(id);
            }
            
            return keys;
        }
        [Fact]
        public virtual async Task TestReadMessageWithoutBatching()
        {
            var topic = "ReadMessageWithoutBatching";
            await TestReadMessages(topic, false);//.ConfigureAwait(false); https://xunit.net/xunit.analyzers/rules/xUnit1030
        }
        [Fact]
        public virtual async Task TestReadMessageWithBatching()
        {
            var topic = $"ReadMessageWithBatching_{Guid.NewGuid()}";
            await TestReadMessages(topic, true);//.ConfigureAwait(false); https://xunit.net/xunit.analyzers/rules/xUnit1030
        }
        [Fact]
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

            var reader = await _client.NewReaderAsync(ISchema<object>.String, builder);//.ConfigureAwait(false); https://xunit.net/xunit.analyzers/rules/xUnit1030
            // create producer and send msg
            IList<Producer<string>> producerList = new List<Producer<string>>();
            foreach (var topicName in topics)
            {
                var producer = await _client.NewProducerAsync(ISchema<object>.String, new ProducerConfigBuilder<string>().Topic(topicName));//.ConfigureAwait(false); https://xunit.net/xunit.analyzers/rules/xUnit1030

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
                    await producer.SendAsync(msg);//.ConfigureAwait(false); https://xunit.net/xunit.analyzers/rules/xUnit1030
                    messages.Add(msg);
                }
            }
            // receive messagesS
            var message = await reader.ReadNextAsync(TimeSpan.FromSeconds(5));//.ConfigureAwait(false); https://xunit.net/xunit.analyzers/rules/xUnit1030
            while (message != null)
            {
                var value = message.Value;
                _output.WriteLine(value);
                Assert.True(messages.Remove(value));
                message = await reader.ReadNextAsync(TimeSpan.FromSeconds(5));//.ConfigureAwait(false); https://xunit.net/xunit.analyzers/rules/xUnit1030
            }
            Assert.True(messages.Count == 0 || messages.Count == 1);
            // clean up
            foreach (var producer in producerList)
            {
                await producer.CloseAsync();//.ConfigureAwait(false); https://xunit.net/xunit.analyzers/rules/xUnit1030
            }
            
        }
        private async Task TestReadMessages(string topic, bool enableBatch)
        {
            var numKeys = 10;
            var builder = new ReaderConfigBuilder<byte[]>()
                .Topic(topic)

                .StartMessageId(IMessageId.Earliest)
                .ReaderName(Subscription);
            var reader = await _client.NewReaderAsync(builder);//.ConfigureAwait(false); https://xunit.net/xunit.analyzers/rules/xUnit1030

            var keys = await PublishMessages(topic, numKeys, enableBatch);
            await Task.Delay(TimeSpan.FromSeconds(1));
            for (var i = 0; i < numKeys; i++)
            {
                var message = await reader.ReadNextAsync(TimeSpan.FromSeconds(60));//.ConfigureAwait(false); https://xunit.net/xunit.analyzers/rules/xUnit1030
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
                await producer.NewMessage().Key(key).Value(data).SendAsync();//.ConfigureAwait(false); https://xunit.net/xunit.analyzers/rules/xUnit1030
                keys.Add(key);
            }
            producer.Flush();
            return keys;
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