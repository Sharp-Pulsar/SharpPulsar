using System;
using System.Text;
using System.Text.Json;
using System.Threading;
using SharpPulsar.Configuration;
using SharpPulsar.Test.Fixtures;
using SharpPulsar.User;
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
namespace SharpPulsar.Test.Api
{
    [Collection(nameof(PulsarTests))]
    public class ConsumerRedeliveryTest
    {
        private readonly ITestOutputHelper _output;
        private readonly PulsarClient _client;

        public ConsumerRedeliveryTest(ITestOutputHelper output, PulsarStandaloneClusterFixture fixture)
        {
            _output = output;
            _client = fixture.Client;
        }

        [Fact]
        public void TestUnAckMessageRedeliveryWithReceive()
        {
            var topic = $"persistent://public/default/async-unack-redelivery-{Guid.NewGuid()}";
            var builder = new ConsumerConfigBuilder<byte[]>();
            builder.Topic(topic);
            builder.SubscriptionName("sub-TestUnAckMessageRedeliveryWithReceive");
            builder.AckTimeout(TimeSpan.FromMilliseconds(8000));
            builder.ForceTopicCreation(true);
            builder.AcknowledgmentGroupTime(0);
            builder.SubscriptionType(Protocol.Proto.CommandSubscribe.SubType.Shared);
            var consumer = _client.NewConsumer(builder);

            var pBuilder = new ProducerConfigBuilder<byte[]>();
            pBuilder.Topic(topic);
            var producer = _client.NewProducer(pBuilder);

            const int messageCount = 10;

            for (var i = 0; i < messageCount; i++)
            {
                var receipt = producer.Send(Encoding.UTF8.GetBytes("my-message-" + i));
                _output.WriteLine(JsonSerializer.Serialize(receipt, new JsonSerializerOptions { WriteIndented = true }));
            }

            var messageReceived = 0;
            Thread.Sleep(TimeSpan.FromSeconds(5));
            for (var i = 0; i < messageCount; ++i)
            {
                var m = consumer.Receive();
                var receivedMessage = Encoding.UTF8.GetString(m.Data);
                _output.WriteLine($"Received message: [{receivedMessage}]");
                Assert.NotNull(receivedMessage);
                messageReceived++;
            }
			
			Assert.Equal(10, messageReceived);
            Thread.Sleep(TimeSpan.FromSeconds(10));
            for (var i = 0; i < messageCount; i++)
            {
                var m = consumer.Receive();
                var receivedMessage = Encoding.UTF8.GetString(m.Data);
                _output.WriteLine($"Received message: [{receivedMessage}]");
                Assert.NotNull(receivedMessage);
                messageReceived++;
            }
            Assert.Equal(20, messageReceived);
            producer.Close();
            consumer.Close();
        }

	}

}