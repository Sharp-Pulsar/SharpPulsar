using SharpPulsar.Configuration;
using SharpPulsar.Extension;
using SharpPulsar.Interfaces;
using SharpPulsar.Schemas;
using SharpPulsar.Test.Fixtures;
using SharpPulsar.User;
using System;
using System.Collections.Generic;
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
    public class ByteKeysTestBatchTest
    {
        private readonly ITestOutputHelper _output;
        private readonly PulsarClient _client;
        private readonly string _topic;

        public ByteKeysTestBatchTest(ITestOutputHelper output, PulsarStandaloneClusterFixture fixture)
        {
            _output = output;
            _client = fixture.Client;
            _topic = $"persistent://public/default/my-topic-Batch-{Guid.NewGuid()}";
        }

        [Fact]
        public virtual void ProducerInstantiation()
        {
            var producer = new ProducerConfigBuilder<string>();
            producer.Topic("ProducerInstantiation");
            var stringProducerBuilder = _client.NewProducer(new StringSchema(), producer);
            Assert.NotNull(stringProducerBuilder);
        }
        [Fact]
        public virtual void ConsumerInstantiation()
        {
            var consumer = new ConsumerConfigBuilder<string>();
            consumer.Topic("ConsumerInstantiation");
            consumer.SubscriptionName($"test-sub-{Guid.NewGuid()}");
            var stringConsumerBuilder = _client.NewConsumer(new StringSchema(), consumer);
            Assert.NotNull(stringConsumerBuilder);
        }
        [Fact]
        public virtual void ReaderInstantiation()
        {
            var reader = new ReaderConfigBuilder<string>();
            reader.Topic("ReaderInstantiation");
            reader.StartMessageId(IMessageId.Earliest);
            var stringReaderBuilder = _client.NewReader(new StringSchema(), reader);
            Assert.NotNull(stringReaderBuilder);
        }
        [Fact]
        public void ProduceAndConsume()
        {
            var topic = $"persistent://public/default/my-topic-{Guid.NewGuid()}";

            Random r = new Random(0);
            byte[] byteKey = new byte[1000];
            r.NextBytes(byteKey);

            var producerBuilder = new ProducerConfigBuilder<sbyte[]>();
            producerBuilder.Topic(topic);
            var producer = _client.NewProducer(producerBuilder);

            producer.NewMessage().KeyBytes(byteKey.ToSBytes())
               .Properties(new Dictionary<string, string> { { "KeyBytes", Encoding.UTF8.GetString(byteKey) } })
               .Value(Encoding.UTF8.GetBytes("TestMessage").ToSBytes())
               .Send();



            var consumerBuilder = new ConsumerConfigBuilder<sbyte[]>();
            consumerBuilder.Topic(topic);
            consumerBuilder.SubscriptionName($"ByteKeysTest-subscriber-{Guid.NewGuid()}");
            var consumer = _client.NewConsumer(consumerBuilder);
            var message = consumer.Receive();

            Assert.Equal(byteKey, message.KeyBytes.ToBytes());

            Assert.True(message.HasBase64EncodedKey());
            var receivedMessage = Encoding.UTF8.GetString((byte[])(Array)message.Data);
            _output.WriteLine($"Received message: [{receivedMessage}]");
            Assert.Equal("TestMessage", receivedMessage);
        }
        [Fact]
        public void ProduceAndConsumeBatch()
		{

            Random r = new Random(0);
            var byteKey = new byte[1000];
            r.NextBytes(byteKey);

            var consumerBuilder = new ConsumerConfigBuilder<sbyte[]>();
            consumerBuilder.Topic(_topic);
            consumerBuilder.SubscriptionName($"Batch-subscriber-{Guid.NewGuid()}");
            var consumer = _client.NewConsumer(consumerBuilder);


            var producerBuilder = new ProducerConfigBuilder<sbyte[]>();
            producerBuilder.Topic(_topic);
            producerBuilder.EnableBatching(true);
            producerBuilder.BatchingMaxPublishDelay(60000);
            producerBuilder.BatchingMaxMessages(5);
            var producer = _client.NewProducer(producerBuilder);

            producer.NewMessage().KeyBytes(byteKey.ToSBytes())
                .Properties(new Dictionary<string, string> { { "KeyBytes", Encoding.UTF8.GetString(byteKey) } })
                .Value(Encoding.UTF8.GetBytes($"TestMessage-0").ToSBytes())
                .Send();

            producer.NewMessage().KeyBytes(byteKey.ToSBytes())
                .Properties(new Dictionary<string, string> { { "KeyBytes", Encoding.UTF8.GetString(byteKey) } })
                .Value(Encoding.UTF8.GetBytes($"TestMessage-1").ToSBytes())
                .Send();

            producer.NewMessage().KeyBytes(byteKey.ToSBytes())
                .Properties(new Dictionary<string, string> { { "KeyBytes", Encoding.UTF8.GetString(byteKey) } })
                .Value(Encoding.UTF8.GetBytes($"TestMessage-2").ToSBytes())
                .Send();

            producer.NewMessage().KeyBytes(byteKey.ToSBytes())
                .Properties(new Dictionary<string, string> { { "KeyBytes", Encoding.UTF8.GetString(byteKey) } })
                .Value(Encoding.UTF8.GetBytes($"TestMessage-3").ToSBytes())
                .Send();

            producer.NewMessage().KeyBytes(byteKey.ToSBytes())
                .Properties(new Dictionary<string, string> { { "KeyBytes", Encoding.UTF8.GetString(byteKey) } })
                .Value(Encoding.UTF8.GetBytes($"TestMessage-4").ToSBytes())
                .Send();

            var sent = producer.SendReceipt();
            if (sent != null)
                _output.WriteLine($"Highest Sequence Id => {sent.SequenceId}:{sent.HighestSequenceId}");


            var message = consumer.Receive();
            Assert.Equal(byteKey, message.KeyBytes.ToBytes());
            Assert.True(message.HasBase64EncodedKey());
            var receivedMessage = Encoding.UTF8.GetString(message.Data.ToBytes());
            _output.WriteLine($"Received message: [{receivedMessage}]");
            Assert.Equal($"TestMessage-0", receivedMessage);

            message = consumer.Receive();
            Assert.Equal(byteKey, message.KeyBytes.ToBytes());
            Assert.True(message.HasBase64EncodedKey());
            receivedMessage = Encoding.UTF8.GetString(message.Data.ToBytes());
            _output.WriteLine($"Received message: [{receivedMessage}]");
            Assert.Equal($"TestMessage-1", receivedMessage);

            message = consumer.Receive();
            Assert.Equal(byteKey, message.KeyBytes.ToBytes());
            Assert.True(message.HasBase64EncodedKey());
            receivedMessage = Encoding.UTF8.GetString(message.Data.ToBytes());
            _output.WriteLine($"Received message: [{receivedMessage}]");
            Assert.Equal($"TestMessage-2", receivedMessage);

            message = consumer.Receive();
            Assert.Equal(byteKey, message.KeyBytes.ToBytes());
            Assert.True(message.HasBase64EncodedKey());
            receivedMessage = Encoding.UTF8.GetString(message.Data.ToBytes());
            _output.WriteLine($"Received message: [{receivedMessage}]");
            Assert.Equal($"TestMessage-3", receivedMessage);

            message = consumer.Receive();
            Assert.Equal(byteKey, message.KeyBytes.ToBytes());
            Assert.True(message.HasBase64EncodedKey());
            receivedMessage = Encoding.UTF8.GetString(message.Data.ToBytes());
            _output.WriteLine($"Received message: [{receivedMessage}]");
            Assert.Equal($"TestMessage-4", receivedMessage);
        }

    }

}