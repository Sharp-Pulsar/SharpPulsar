using SharpPulsar.Configuration;
using SharpPulsar.Interfaces;
using SharpPulsar.Schemas;
using SharpPulsar.TestContainer;
using SharpPulsar.User;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
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
namespace SharpPulsar.Test.Integration
{
    [Collection(nameof(PulsarTests))]
    public class ByteKeysTest
    {
        private readonly ITestOutputHelper _output;
        private readonly PulsarClient _client;
        private readonly string _topic;

        public ByteKeysTest(ITestOutputHelper output, PulsarFixture fixture)
        {
            _output = output;
            _client = fixture.Client;
            _topic = $"persistent://public/default/my-topic-Batch-{Guid.NewGuid()}";
            //_topic = "my-topic-batch-bf719df3";
        }

        [Fact]
        public async Task ProducerInstantiation()
        {
            var producer = new ProducerConfigBuilder<string>();
            producer.Topic("ProducerInstantiation");
            var stringProducerBuilder = await _client.NewProducerAsync(new StringSchema(), producer);
            Assert.NotNull(stringProducerBuilder);
            await stringProducerBuilder.CloseAsync();
        }
        [Fact]
        public async Task ConsumerInstantiation()
        {
            var consumer = new ConsumerConfigBuilder<string>();
            consumer.Topic("ConsumerInstantiation");
            consumer.SubscriptionName($"test-sub-{Guid.NewGuid()}");
            var stringConsumerBuilder = await _client.NewConsumerAsync(new StringSchema(), consumer);
            Assert.NotNull(stringConsumerBuilder);
            stringConsumerBuilder.Close();
        }
        [Fact]
        public async Task ReaderInstantiation()
        {
            var reader = new ReaderConfigBuilder<string>();
            reader.Topic("ReaderInstantiation");
            reader.StartMessageId(IMessageId.Earliest);
            var stringReaderBuilder = await _client.NewReaderAsync(new StringSchema(), reader);
            Assert.NotNull(stringReaderBuilder);
            await stringReaderBuilder.CloseAsync();
        }
        [Fact]
        public async Task ProduceAndConsume()
        {
            var topic = $"persistent://public/default/produce-consume-3";

            var r = new Random(0);
            var byteKey = new byte[1000];
            r.NextBytes(byteKey);

            var producerBuilder = new ProducerConfigBuilder<byte[]>();
            producerBuilder.Topic(topic);
            var producer = await _client.NewProducerAsync(producerBuilder);

            await producer.NewMessage().KeyBytes(byteKey)
               .Properties(new Dictionary<string, string> { { "KeyBytes", Encoding.UTF8.GetString(byteKey) } })
               .Value(Encoding.UTF8.GetBytes("TestMessage"))
               .SendAsync();

            var consumerBuilder = new ConsumerConfigBuilder<byte[]>()
                .Topic(topic)
                //.StartMessageId(77L, 0L, -1, 0)
                .SubscriptionInitialPosition(Common.SubscriptionInitialPosition.Earliest)
                .SubscriptionName($"ByteKeysTest-subscriber-{Guid.NewGuid()}");
            var consumer = await _client.NewConsumerAsync(consumerBuilder);

            await Task.Delay(TimeSpan.FromSeconds(10));
            var message = (Message<byte[]>)await consumer.ReceiveAsync();

            if (message != null)
                _output.WriteLine($"BrokerEntryMetadata[timestamp:{message.BrokerEntryMetadata?.BrokerTimestamp} index: {message.BrokerEntryMetadata?.Index.ToString()}");

            Assert.Equal(byteKey, message.KeyBytes);

            Assert.True(message.HasBase64EncodedKey());
            var receivedMessage = Encoding.UTF8.GetString(message.Data);
            _output.WriteLine($"Received message: [{receivedMessage}]");
            Assert.Equal("TestMessage", receivedMessage);
            //producer.Close();
            await consumer.CloseAsync();
        }
        [Fact]
        public async Task ProduceAndConsumeBatch()
		{

            var r = new Random(0);
            var byteKey = new byte[1000];
            r.NextBytes(byteKey);

            var consumerBuilder = new ConsumerConfigBuilder<byte[]>()
                .Topic(_topic)
                .ForceTopicCreation(true)
                .SubscriptionName($"Batch-subscriber-{Guid.NewGuid()}");
            var consumer = await _client.NewConsumerAsync(consumerBuilder);


            var producerBuilder = new ProducerConfigBuilder<byte[]>()
                .Topic(_topic)
                .SendTimeout(TimeSpan.FromMilliseconds(10000))
                .EnableBatching(true)
                .BatchingMaxPublishDelay(TimeSpan.FromMilliseconds(120000))
                .BatchingMaxMessages(5);

            var producer = await _client.NewProducerAsync(producerBuilder);

            for (var i = 0; i < 5; i++)
            {
                 var id = await producer.NewMessage().KeyBytes(byteKey)
                    .Properties(new Dictionary<string, string> { { "KeyBytes", Encoding.UTF8.GetString(byteKey) } })
                    .Value(Encoding.UTF8.GetBytes($"TestMessage-{i}"))
                    .SendAsync();
                if(id == null)
                    _output.WriteLine($"Id is null");
                else
                    _output.WriteLine($"Id: {id}");
            }
            producer.Flush();
            await Task.Delay(TimeSpan.FromSeconds(10));
            for (var i = 0; i < 5; i++)
            {
                var message = (Message<byte[]>) await consumer.ReceiveAsync();
                if (message != null)
                    _output.WriteLine($"BrokerEntryMetadata[timestamp:{message.BrokerEntryMetadata.BrokerTimestamp} index: {message.BrokerEntryMetadata?.Index.ToString()}");

                Assert.Equal(byteKey, message.KeyBytes);
                Assert.True(message.HasBase64EncodedKey());
                var receivedMessage = Encoding.UTF8.GetString(message.Data);
                _output.WriteLine($"Received message: [{receivedMessage}]");
                Assert.Equal($"TestMessage-{i}", receivedMessage);
            }

            await producer.CloseAsync();
            await consumer.CloseAsync();
        }
        
    }

}