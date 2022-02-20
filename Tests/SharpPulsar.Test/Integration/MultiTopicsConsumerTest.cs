using SharpPulsar.Configuration;
using SharpPulsar.User;
using System.Collections.Generic;
using System.Text;
using Xunit;
using Xunit.Abstractions;
using System;
using System.Threading;
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


    /// <summary>
    /// Unit Tests of <seealso cref="MultiTopicsConsumer{T}"/>.
    /// </summary>
    [Collection(nameof(IntegrationCollection))]
    public class MultiTopicsConsumerTest
    {
        private const string Subscription = "reader-multi-topics-sub";
        private readonly ITestOutputHelper _output;
        private readonly PulsarClient _client;

        public MultiTopicsConsumerTest(ITestOutputHelper output, PulsarFixture fixture)
        {
            _output = output;
            _client = fixture.Client;
        }
        [Fact]
        public async Task TestMultiTopicConsumer()
        {
            var messageCount = 5;
            var first = $"one-topic-{Guid.NewGuid()}";
            var second = $"two-topic-{Guid.NewGuid()}";
            var third = $"three-topic-{Guid.NewGuid()}";

            await PublishMessages(first, messageCount, "hello Toba");
            await PublishMessages(third, messageCount, "hello Toba");
            await PublishMessages(second, messageCount, "hello Toba");
            var builder = new ConsumerConfigBuilder<byte[]>()
                .Topic(first, second, third)
                .ForceTopicCreation(true)
                .SubscriptionName("multi-topic-sub");

            var consumer = await _client.NewConsumerAsync(builder);
            await Task.Delay(TimeSpan.FromSeconds(30));
            var received = 0;
            for (var i = 0; i < messageCount; i++)
            {
                var message = (TopicMessage<byte[]>)await consumer.ReceiveAsync();
                if (message != null)
                {
                    await consumer.AcknowledgeAsync(message);
                    _output.WriteLine($"message from topic: {message.Topic}");
                    received++;
                }
            }
            for (var i = 0; i < messageCount; i++)
            {
                var message = (TopicMessage<byte[]>)await consumer.ReceiveAsync();
                if (message != null)
                {
                    await consumer.AcknowledgeAsync(message);
                    _output.WriteLine($"message from topic: {message.Topic}");
                    received++;
                }
            }
            for (var i = 0; i < messageCount; i++)
            {
                var message = (TopicMessage<byte[]>)await consumer.ReceiveAsync();
                if (message != null)
                {
                    await consumer.AcknowledgeAsync(message);
                    _output.WriteLine($"message from topic: {message.Topic}");
                    received++;
                }
            }
            Assert.True(received > 0);
            await consumer.CloseAsync();
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
            await producer.CloseAsync();
            return keys;
        }
    }

}