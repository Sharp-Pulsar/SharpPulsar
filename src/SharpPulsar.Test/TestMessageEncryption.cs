﻿using SharpPulsar.Builder;
using SharpPulsar.Test.Fixture;
using SharpPulsar.TestContainer;
using System;
using System.Text;
using System.Threading.Tasks;
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
    public class TestMessageEncryption : IAsyncLifetime
    {
        private PulsarClient _client;
        private readonly ITestOutputHelper _output;
        private PulsarSystem _system;
        private PulsarClientConfigBuilder _configBuilder;
        public TestMessageEncryption(ITestOutputHelper output, PulsarFixture fixture)
        {
            _output = output;
            _configBuilder = fixture.ConfigBuilder;
            _system = fixture.System;
        }
        //[Fact(Skip = "Encrpted Produce Consume")  ]
        [Fact]
        public async Task TestEncrptedProduceConsume()
        {
            var messageCount = 10;
            var topic = $"encrypted-messages-{Guid.NewGuid()}";

            
            var producer = await _client.NewProducerAsync(new ProducerConfigBuilder<byte[]>()
                .Topic(topic)
                .CryptoKeyReader(new RawFileKeyReader("Certs/SharpPulsar_pub.pem", "Certs/SharpPulsar_private.pem"))
                .AddEncryptionKey("Ebere"));

            for (var i = 0; i < messageCount; i++)
            {
                await producer.SendAsync(Encoding.UTF8.GetBytes($"Shhhh, a secret: my is Ebere Abanonu and am a Nigerian based in Abeokuta, Ogun (a neighbouring State to Lagos - about 2 hours drive) [{i}]"));
            }
            var receivedCount = 0;

            var consumer = await _client.NewConsumerAsync(new ConsumerConfigBuilder<byte[]>()
                .Topic(topic)
                .ForceTopicCreation(true)
                .CryptoKeyReader(new RawFileKeyReader("Certs/SharpPulsar_pub.pem", "Certs/SharpPulsar_private.pem"))
                .SubscriptionName("encrypted-sub")
                .SubscriptionInitialPosition(Common.SubscriptionInitialPosition.Earliest));
            await Task.Delay(TimeSpan.FromSeconds(1));
            for (var i = 0; i < messageCount; i++)
            {
                var message = await consumer.ReceiveAsync(TimeSpan.FromMicroseconds(5000));
                if (message != null)
                {
                    var decrypted = Encoding.UTF8.GetString(message.Data);
                    if (decrypted == null) continue;
                    _output.WriteLine(decrypted);
                    //Assert.Equal($"Shhhh, a secret: my is Ebere Abanonu and am a Nigerian based in Abeokuta, Ogun (a neighbouring State to Lagos - about 2 hours drive) [{i}]", decrypted);
                    await consumer.AcknowledgeAsync(message);
                    receivedCount++;
                }
            }
            Assert.True(receivedCount > 6);
            await producer.CloseAsync();
            await consumer.CloseAsync();
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
