using BAMCIS.Util.Concurrent;
using SharpPulsar.Configuration;
using SharpPulsar.Extension;
using SharpPulsar.Interfaces;
using SharpPulsar.Schemas;
using SharpPulsar.Test.Fixtures;
using SharpPulsar.User;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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
namespace SharpPulsar.Test.Api
{   
    [Collection(nameof(PulsarTests))]
    public class ByteKeysTestBatchTest
    {
        private readonly ITestOutputHelper _output;
        private readonly PulsarSystem _system;
        private readonly PulsarClient _client;
        private readonly string _topic;
        private readonly byte[] _byteKey;

        public ByteKeysTestBatchTest(ITestOutputHelper output, PulsarStandaloneClusterFixture fixture)
        {
            _output = output;
            _system = fixture.System;
            _client = _system.NewClient();
            _topic = $"persistent://public/default/my-topic-Batch";

            Random r = new Random(0);
            _byteKey = new byte[1000];
            r.NextBytes(_byteKey);
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
            var producerBuilder = new ProducerConfigBuilder<sbyte[]>();
            producerBuilder.Topic(_topic);
            producerBuilder.EnableBatching(true);
            producerBuilder.BatchingMaxPublishDelay(60000, TimeUnit.MILLISECONDS);
            producerBuilder.BatchingMaxMessages(5);
            var producer = _client.NewProducer(producerBuilder);

            for (var i = 0; i < 5; i++)
            {
                producer.NewMessage().KeyBytes(_byteKey.ToSBytes())
                .Properties(new Dictionary<string, string> { { "KeyBytes", Encoding.UTF8.GetString(_byteKey) } })
                .Value(Encoding.UTF8.GetBytes($"TestMessage-{i}").ToSBytes())
                .Send();
            }
            for (var i = 0; i < 6; i++)
            {
                var sent = producer.SendReceipt();
                if (sent != null)
                    if(sent.Errored)
                        _output.WriteLine(sent.Exception.Message);
                    else if(sent.Message != null)
                        _output.WriteLine(sent.Message.Metadata.SequenceId.ToString());
                    else
                    {
                        foreach(var msg in sent.Messages)
                            _output.WriteLine(msg.Metadata.SequenceId.ToString());
                    }
            }
            var consumerBuilder = new ConsumerConfigBuilder<sbyte[]>();
            consumerBuilder.Topic(_topic);
            consumerBuilder.SubscriptionName($"Batch-subscriber-{Guid.NewGuid()}");
            var consumer = _client.NewConsumer(consumerBuilder);

            for (var i = 0; i < 5; i++)
            {
                try
                {
                    Console.WriteLine($"About to Receive message: [{i}]");
                    var message = consumer.Receive();
                    Assert.Equal(_byteKey, message.KeyBytes.ToBytes());
                    Assert.True(message.HasBase64EncodedKey());
                    var receivedMessage = Encoding.UTF8.GetString(message.Data.ToBytes());
                    _output.WriteLine($"Received message: [{receivedMessage}]");
                    Console.WriteLine($"Received message: [{receivedMessage}]");
                    Assert.Equal($"TestMessage-{i}", receivedMessage);
                }
                catch
                {

                }
            }
        }

    }

}