﻿using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using SharpPulsar.Builder;
using SharpPulsar.Test.Fixture;
using SharpPulsar.TestContainer;
using Xunit;
using Xunit.Abstractions;

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
    public class ConsumerRedeliveryTest : IAsyncLifetime
    {
        private PulsarClient _client;
        private readonly ITestOutputHelper _output;
        //private TaskCompletionSource<PulsarClient> _tcs;
        private PulsarSystem _system;
        private PulsarClientConfigBuilder _configBuilder;
        public ConsumerRedeliveryTest(ITestOutputHelper output, PulsarFixture fixture)
        {
            _output = output;
            _configBuilder = fixture.ConfigBuilder;
            _system = fixture.System;
        }

        [Fact]
        public async Task TestUnAckMessageRedeliveryWithReceive()
        {
            var topic = $"persistent://public/default/async-unack-redelivery-{Guid.NewGuid()}";

            var pBuilder = new ProducerConfigBuilder<byte[]>();
            pBuilder.Topic(topic);
            var producer = await _client.NewProducerAsync(pBuilder);

            const int messageCount = 10;

            for (var i = 0; i < messageCount; i++)
            {
                var receipt = await producer.SendAsync(Encoding.UTF8.GetBytes("my-message-" + i));
                _output.WriteLine(JsonSerializer.Serialize(receipt, new JsonSerializerOptions { WriteIndented = true }));
            }

            var builder = new ConsumerConfigBuilder<byte[]>();
            builder.Topic(topic);
            builder.SubscriptionName("sub-TestUnAckMessageRedeliveryWithReceive");
            builder.AckTimeout(TimeSpan.FromMilliseconds(5000));
            builder.ForceTopicCreation(true);
            builder.AcknowledgmentGroupTime(TimeSpan.Zero);
            builder.SubscriptionType(Protocol.Proto.CommandSubscribe.SubType.Shared);
            var consumer = await _client.NewConsumerAsync(builder);
            var messageReceived = 0;
            await Task.Delay(TimeSpan.FromMilliseconds(1000));
            for (var i = 0; i < messageCount - 2; ++i)
            {
                var m = (Message<byte[]>)await consumer.ReceiveAsync(TimeSpan.FromMicroseconds(5000));

                _output.WriteLine($"BrokerEntryMetadata[timestamp:{m.BrokerEntryMetadata.BrokerTimestamp} index: {m.BrokerEntryMetadata?.Index.ToString()}");
                var receivedMessage = Encoding.UTF8.GetString(m.Data);
                _output.WriteLine($"Received message: [{receivedMessage}]");
                messageReceived++;
            }

            Assert.True(messageReceived > 0);
            await Task.Delay(TimeSpan.FromSeconds(1));
            for (var i = 0; i < messageCount - 5; i++)
            {
                var m = (Message<byte[]>)await consumer.ReceiveAsync(TimeSpan.FromMicroseconds(5000));
                if(m != null)
                {
                    var receivedMessage = Encoding.UTF8.GetString(m.Data);
                    _output.WriteLine($"Received message: [{receivedMessage}]");
                    messageReceived++;
                }
               
            }
            await producer.CloseAsync();
            await consumer.CloseAsync();
            Assert.True(messageReceived > 5);
        }
        public async Task InitializeAsync()
        {
            /*_tcs = new TaskCompletionSource<PulsarClient>(TaskCreationOptions.RunContinuationsAsynchronously);
            //_client = fixture.System.NewClient(fixture.ConfigBuilder).AsTask().GetAwaiter().GetResult();
            new Action(async () =>
            {
                var client = await _system.NewClient(_configBuilder);
                _tcs.TrySetResult(client);
            })();
           _client = await _tcs.Task; */
            _client = await _system.NewClient(_configBuilder);
        }

        public async Task DisposeAsync()
        {
            await _client.ShutdownAsync();
        }

    }

}