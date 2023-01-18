using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using Xunit.Abstractions;
using SharpPulsar.Interfaces;
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
    public class MessageChunkingTest : IAsyncLifetime
    {
        private PulsarClient _client;
        private readonly ITestOutputHelper _output;
        private PulsarSystem _system;
        private PulsarClientConfigBuilder _configBuilder;
        public MessageChunkingTest(ITestOutputHelper output, PulsarFixture fixture)
        {
            _output = output;
            _configBuilder = fixture.ConfigBuilder;
            _system = fixture.System;
        }

        [Fact]
        public async Task TestLargeMessage()
        {
            //this.conf.MaxMessageSize = 5;
            const int totalMessages = 3;
            var topicName = $"persistent://public/default/my-topic1-{DateTimeHelper.CurrentUnixTimeMillis()}";

            var pBuilder = new ProducerConfigBuilder<byte[]>()
                .Topic(topicName)
                .EnableChunking(true)
                .ChunkMaxMessageSize(5);
            var producer = await _client.NewProducerAsync(pBuilder);

            IList<string> publishedMessages = new List<string>();
            for (var i = 1; i < totalMessages; i++)
            {
                var message = CreateMessagePayload(i * 10);
                publishedMessages.Add(message);
                await producer.SendAsync(Encoding.UTF8.GetBytes(message));
            }
            var builder = new ConsumerConfigBuilder<byte[]>()
                .Topic(topicName)
                .SubscriptionName("my-subscriber-name");
            var consumer = await _client.NewConsumerAsync(builder);
            IMessage<byte[]> msg = null;
            ISet<string> messageSet = new HashSet<string>();
            IList<IMessage<byte[]>> msgIds = new List<IMessage<byte[]>>();
            await Task.Delay(TimeSpan.FromSeconds(1));
            for (var i = 0; i < totalMessages - 1; i++)
            {
                try
                {
                    msg = await consumer.ReceiveAsync(TimeSpan.FromMicroseconds(5000));
                    var receivedMessage = Encoding.UTF8.GetString(msg.Data);
                    _output.WriteLine($"[{i}] - Published [{publishedMessages[i]}] Received message: [{receivedMessage}]");
                    var expectedMessage = publishedMessages[i];
                    TestMessageOrderAndDuplicates(messageSet, receivedMessage, expectedMessage);
                    msgIds.Add(msg);
                }
                catch (Exception ex) 
                {
                    _output.WriteLine(ex.ToString());
                }

            }

            foreach (var msgId in msgIds)
            {
                await consumer.AcknowledgeAsync(msgId);
            }

        }
        private void TestMessageOrderAndDuplicates<T>(ISet<T> messagesReceived, T receivedMessage, T expectedMessage)
        {
            // Make sure that messages are received in order
            Assert.True(receivedMessage.Equals(expectedMessage), "Received message " + receivedMessage + " did not match the expected message " + expectedMessage);

            // Make sure that there are no duplicates
            Assert.True(messagesReceived.Add(receivedMessage), "Received duplicate message " + receivedMessage);
        }
        private string CreateMessagePayload(int size)
        {
            var str = new StringBuilder();
            var rand = new Random();
            for (var i = 0; i < size; i++)
            {
                str.Append(rand.Next(10));
            }
            return str.ToString();
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