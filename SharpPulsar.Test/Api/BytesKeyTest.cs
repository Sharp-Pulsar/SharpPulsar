using BAMCIS.Util.Concurrent;
using SharpPulsar.Configuration;
using SharpPulsar.Extension;
using SharpPulsar.Schemas;
using SharpPulsar.Test.Fixtures;
using SharpPulsar.User;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
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
    public class BytesKeyTest
	{
        private readonly ITestOutputHelper _output;
        private readonly PulsarSystem _system;
        private readonly PulsarClient _client;

        public BytesKeyTest(ITestOutputHelper output, PulsarSystemFixture fixture)
        {
            _output = output;
            _system = fixture.System;
            _client = _system.NewClient();
        }
        [Theory]
        //[InlineData(true)]
        [InlineData(false)]
		public void ByteKeysTest(bool batching)
		{
            var topic = "persistent://public/default/my-topic-keys8783uuiu";


            Random r = new Random(0);
            byte[] byteKey = new byte[1000];
            r.NextBytes(byteKey);

            var producerBuilder = new ProducerConfigBuilder<sbyte[]>();
            producerBuilder.Topic(topic);
            if (batching)
            {
                producerBuilder.EnableBatching(true);
                producerBuilder.BatchingMaxPublishDelay(30000, TimeUnit.MILLISECONDS);
                producerBuilder.BatchingMaxMessages(5);
            }
            var producer = _client.NewProducer(producerBuilder);

            var consumerBuilder = new ConsumerConfigBuilder<sbyte[]>();
            consumerBuilder.Topic(topic);
            consumerBuilder.SubscriptionName("ByteKeysTest-subscriber");
            var consumer = _client.NewConsumer(consumerBuilder);

            if(batching)
            {
                for(var i = 0; i < 7; i++)
                {

                    producer.NewMessage().KeyBytes(byteKey.ToSBytes())
                    .Properties(new Dictionary<string, string> { { "KeyBytes", Encoding.UTF8.GetString(byteKey) } })
                    .Value(Encoding.UTF8.GetBytes($"TestMessage-{i}").ToSBytes())
                    .Send();
                }
            }
            else
            {
                producer.NewMessage().KeyBytes(byteKey.ToSBytes())
                .Properties(new Dictionary<string, string> { { "KeyBytes", Encoding.UTF8.GetString(byteKey) } })
                .Value(Encoding.UTF8.GetBytes("TestMessage").ToSBytes())
                .Send();
            }

            //two ways:
            //msg.Send() = pull sendreceipt in a different way like in the background, good when batching is enabled
            //producer.Send(Encoding.UTF8.GetBytes("TestMessage").ToSBytes()); waits for the sentrecepit

            var sent = producer.SendReceipt();
            if(batching)
            {
                for(var i = 0; i < 6; i++)
                {
                    var message = consumer.Receive();
                    Assert.Equal(byteKey, (byte[])(object)message.KeyBytes);
                    Assert.True(message.HasBase64EncodedKey());
                    var receivedMessage = Encoding.UTF8.GetString((byte[])(Array)message.Data);
                    _output.WriteLine($"Received message: [{receivedMessage}]");
                    Assert.Equal($"TestMessage-{i}", receivedMessage);
                }
            }
            else
            {
                var message = consumer.Receive();
                Assert.Equal(byteKey, (byte[])(object)message.KeyBytes);
                Assert.True(message.HasBase64EncodedKey());
                var receivedMessage = Encoding.UTF8.GetString((byte[])(Array)message.Data);
                _output.WriteLine($"Received message: [{receivedMessage}]");
                Assert.Equal("TestMessage", receivedMessage);
            }
        }
	}

}