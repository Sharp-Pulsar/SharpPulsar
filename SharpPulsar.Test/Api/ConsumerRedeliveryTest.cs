using System;
using System.Text;
using System.Text.Json;
using System.Threading;
using BAMCIS.Util.Concurrent;
using SharpPulsar.Configuration;
using SharpPulsar.Extension;
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
        private readonly PulsarSystem _system;
        private readonly PulsarClient _client;

        public ConsumerRedeliveryTest(ITestOutputHelper output, PulsarSystemFixture fixture)
        {
            _output = output;
            _output = output;
            _system = fixture.System;
            _client = _system.NewClient();
        }

        [Fact (Skip = "Not ready")]
        public void TestUnAckMessageRedeliveryWithReceive()
        {
            var topic = $"persistent://public/default/async-unack-redelivery-{Guid.NewGuid()}";
            var builder = new ConsumerConfigBuilder<sbyte[]>();
            builder.Topic(topic);
            builder.SubscriptionName("sub-TestUnAckMessageRedeliveryWithReceive");
            builder.AckTimeout(30000, TimeUnit.MILLISECONDS);
            builder.ForceTopicCreation(true);
            builder.AcknowledgmentGroupTime(0);
            var consumer = _client.NewConsumer(builder);

            var pBuilder = new ProducerConfigBuilder<sbyte[]>();
            pBuilder.Topic(topic);
            var producer = _client.NewProducer(pBuilder);

            const int messageCount = 10;

            for (var i = 0; i < messageCount; i++)
            {
                var receipt = producer.Send(Encoding.UTF8.GetBytes("my-message-" + i).ToSBytes());
                _output.WriteLine(JsonSerializer.Serialize(receipt, new JsonSerializerOptions { WriteIndented = true }));
            }

            var messageReceived = 0;
            for (var i = 0; i < messageCount; i++)
            {
                var m = consumer.Receive();
                var receivedMessage = Encoding.UTF8.GetString((byte[])(Array)m.Data);
                _output.WriteLine($"Received message: [{receivedMessage}]");
                Assert.NotNull(receivedMessage);
                messageReceived++;
            }
			
			Assert.Equal(10, messageReceived);
            Thread.Sleep(31000);


            for (var i = 0; i < messageCount; i++)
            {
                var receipt = producer.Send(Encoding.UTF8.GetBytes("my-message-" + i).ToSBytes());
                _output.WriteLine(JsonSerializer.Serialize(receipt, new JsonSerializerOptions { WriteIndented = true }));
            }

            for (var i = 0; i < messageCount; i++)
            {
                var m = consumer.Receive();
                var receivedMessage = Encoding.UTF8.GetString((byte[])(Array)m.Data);
                _output.WriteLine($"Received message: [{receivedMessage}]");
                Assert.NotNull(receivedMessage);
                messageReceived++;
            }
            Assert.Equal(20, messageReceived);
        }

	}

}