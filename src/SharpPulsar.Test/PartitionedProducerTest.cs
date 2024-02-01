using System;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using SharpPulsar.Admin.v2;
using SharpPulsar.Builder;
using SharpPulsar.Test.Fixture;
using SharpPulsar.TestContainer;
using Xunit;
using Xunit.Abstractions;
using static SharpPulsar.Protocol.Proto.CommandSubscribe;

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
    public class PartitionedProducerTest : IAsyncLifetime
    {
        private PulsarClient _client;
        private readonly ITestOutputHelper _output;
        private PulsarSystem _system;
        private PulsarClientConfigBuilder _configBuilder;
        private readonly PulsarAdminRESTAPIClient _admin;
        public PartitionedProducerTest(ITestOutputHelper output, PulsarFixture fixture)
        {
            var http = new HttpClient
            {
                BaseAddress = new Uri("http://localhost:8080/admin/v2/")
            };
            _admin = new PulsarAdminRESTAPIClient(http);
            _output = output;
            _configBuilder = fixture.ConfigBuilder;
            _system = fixture.System;
        }
        [Fact]
        public virtual async Task TestGetNumOfPartitions()
        {
            var topicName = "pulsar1-partitioned-topic-" + Guid.NewGuid();
            var key = Guid.NewGuid().ToString();
            var subscriptionName = "partitioned-subscription";
            var pattern = new Regex("public/default/pulsar1-partitioned-topic.*");

            // create partitioned topic
            try
            {
                var part = new PartitionedTopicMetadata
                {
                    Partitions = 4
                };
                await _admin.CreatePartitionedTopic2Async("public", "default", topicName, part, true);
            }
            catch (Exception ex)
            {
                var ss = ex.Message;
                //Assert.Equal("Operation returned an invalid status code 'NoContent'", ss);
            }
            var partitions = await _admin.GetPartitionedMetadata2Async("public", "default", topicName, false, false);
            var s = partitions.Partitions;
            Assert.Equal(4, s);

            var consumer = await _client.NewConsumerAsync(new ConsumerConfigBuilder<byte[]>()
                .TopicsPattern(pattern)
                .PatternAutoDiscoveryPeriod(2)
                .ForceTopicCreation(true)
                .SubscriptionName(subscriptionName)
                .SubscriptionType(SubType.Shared)
                .AckTimeout(TimeSpan.FromMilliseconds(60000)));
            // 2. create producer
            var messagePredicate = "partitioned-producer" + Guid.NewGuid() + "-";
            var partitioProducer = await _client.NewPartitionedProducerAsync(new ProducerConfigBuilder<byte[]>()
                .Topic(topicName)
                .EnableLazyStartPartitionedProducers(true));

           var producers = await partitioProducer.Producers();

            // 5. produce data
            foreach (var producer in producers)
            {
                await producer.SendAsync(Encoding.UTF8.GetBytes(messagePredicate + "producer1-0"));
                //await producer.SendAsync(Encoding.UTF8.GetBytes(messagePredicate + "producer1-1"));
                //await producer.SendAsync(Encoding.UTF8.GetBytes(messagePredicate + "producer1-2"));
            }

            var messageSet = 0;
            
            await Task.Delay(TimeSpan.FromSeconds(1));
            var message = await consumer.ReceiveAsync(TimeSpan.FromMilliseconds(5100));//.ConfigureAwait(false); https://xunit.net/xunit.analyzers/rules/xUnit1030
            if (message == null)
            {
                await Task.Delay(TimeSpan.FromSeconds(1));
                message = await consumer.ReceiveAsync(TimeSpan.FromMicroseconds(5000));
            }

            while (message != null)
            {
                var m = (TopicMessage<byte[]>)message;
                messageSet++;
                await consumer.AcknowledgeAsync(m);
                _output.WriteLine($"Consumer acknowledged : {Encoding.UTF8.GetString(message.Data)} from topic: {m.Topic}");
                message = await consumer.ReceiveAsync(TimeSpan.FromMicroseconds(5000));
            }
            consumer.Unsubscribe(true);
            foreach (var producer in producers)
            {
                await producer.CloseAsync();
            }

           Assert.True(producers.Count > 0);
        }
        [Fact]
        public virtual async Task TestRouteGetNumOfPartitions()
        {
            var topicName = "abanonu1-partitioned-topic-" + Guid.NewGuid();
            var key = Guid.NewGuid().ToString();
            var subscriptionName = "partitioned-subscription";
            var pattern = new Regex("public/default/abanonu1-partitioned-topic.*");

            // create partitioned topic
            try
            {
                var part = new PartitionedTopicMetadata
                {
                    Partitions = 4
                };
                await _admin.CreatePartitionedTopic2Async("public", "default", topicName, part, true);
            }
            catch (Exception ex)
            {
                var ss = ex.Message;
                //Assert.Equal("Operation returned an invalid status code 'NoContent'", ss);
            }
            var partitions = await _admin.GetPartitionedMetadata2Async("public", "default", topicName, false, false);
            var s = partitions.Partitions;
            Assert.Equal(4, s);

            var consumer = await _client.NewConsumerAsync(new ConsumerConfigBuilder<byte[]>()
                .TopicsPattern(pattern)
                .PatternAutoDiscoveryPeriod(2)
                .ForceTopicCreation(true)
                .SubscriptionName(subscriptionName)
                .SubscriptionType(SubType.Shared)
                .AckTimeout(TimeSpan.FromMilliseconds(60000)));
            // 2. create producer
            var messagePredicate = "partitioned-producer" + Guid.NewGuid() + "-";
            var partitioProducer = await _client.NewPartitionedProducerAsync(new ProducerConfigBuilder<byte[]>()
                .Topic(topicName)
                .EnableLazyStartPartitionedProducers(true));

            var a = await partitioProducer.SendAsync(Encoding.UTF8.GetBytes(messagePredicate + "producer1-0"));
            var b = await partitioProducer.SendAsync(Encoding.UTF8.GetBytes(messagePredicate + "producer1-1"));
            var c = await partitioProducer.SendAsync(Encoding.UTF8.GetBytes(messagePredicate + "producer1-2"));
            

            var messageSet = 0;

            await Task.Delay(TimeSpan.FromSeconds(1));
            var message = await consumer.ReceiveAsync(TimeSpan.FromMilliseconds(5100));//.ConfigureAwait(false); https://xunit.net/xunit.analyzers/rules/xUnit1030
            if (message == null)
            {
                await Task.Delay(TimeSpan.FromSeconds(1));
                message = await consumer.ReceiveAsync(TimeSpan.FromMicroseconds(5000));
            }

            while (message != null)
            {
                var m = (TopicMessage<byte[]>)message;
                messageSet++;
                await consumer.AcknowledgeAsync(m);
                _output.WriteLine($"Consumer acknowledged : {Encoding.UTF8.GetString(message.Data)} from topic: {m.Topic}");
                message = await consumer.ReceiveAsync(TimeSpan.FromMicroseconds(5000));
            }
            consumer.Unsubscribe();
            // foreach (var producer in producers)
            //  {
            //     await producer.CloseAsync();
            //}

            // Assert.True(producers.Count > 0);
            await partitioProducer.CloseAsync();
        }
        [Fact]
        public virtual async Task TestBinaryProtoToGetTopicsOfNamespacePersistent()
        {
            var key = Guid.NewGuid().ToString();
            var subscriptionName = "ebere1-regex-subscription";
            var topicName1 = "persistent://public/default/ebere1-reg-topic-1-" + key;
            var topicName2 = "persistent://public/default/ebere1-reg-topic-2-" + key;
            var topicName3 = "persistent://public/default/ebere1-reg-topic-3-" + key;
            var topicName4 = "non-persistent://public/default/ebere1-reg-topic-4-" + key;
            var pattern = new Regex("public/default/ebere1-reg-topic.*");


            // 2. create producer
            var messagePredicate = "my-message-" + key + "-";

            var producer1 = await _client.NewProducerAsync(new ProducerConfigBuilder<byte[]>()
                .Topic(topicName1));

            var producer2 = await _client.NewProducerAsync(new ProducerConfigBuilder<byte[]>()
                .Topic(topicName2));

            var producer3 = await _client.NewProducerAsync(new ProducerConfigBuilder<byte[]>()
                .Topic(topicName3));

            var producer4 = await _client.NewProducerAsync(new ProducerConfigBuilder<byte[]>()
                .Topic(topicName4));

            // 5. produce data
            for (var i = 0; i < 10; i++)
            {
                await producer1.SendAsync(Encoding.UTF8.GetBytes(messagePredicate + "producer1-" + i));
                await producer2.SendAsync(Encoding.UTF8.GetBytes(messagePredicate + "producer2-" + i));
                await producer3.SendAsync(Encoding.UTF8.GetBytes(messagePredicate + "producer3-" + i));
                await producer4.SendAsync(Encoding.UTF8.GetBytes(messagePredicate + "producer4-" + i));
            }


            // 6. should receive all the message
            var messageSet = 0;
            var consumer = await _client.NewConsumerAsync(new ConsumerConfigBuilder<byte[]>()
                .TopicsPattern(pattern)
                .PatternAutoDiscoveryPeriod(2)
                .ForceTopicCreation(true)
                .SubscriptionName(subscriptionName)
                .SubscriptionType(SubType.Shared)
                .AckTimeout(TimeSpan.FromMilliseconds(60000)));
            await Task.Delay(TimeSpan.FromSeconds(1));
            var message = await consumer.ReceiveAsync(TimeSpan.FromMilliseconds(1100));
            if (message == null)
            {
                await Task.Delay(TimeSpan.FromSeconds(1));
                message = await consumer.ReceiveAsync(TimeSpan.FromMicroseconds(5000));
            }

            while (message != null)
            {
                var m = (TopicMessage<byte[]>)message;
                messageSet++;
                await consumer.AcknowledgeAsync(m);
                _output.WriteLine($"Consumer acknowledged : {Encoding.UTF8.GetString(message.Data)} from topic: {m.Topic}");
                message = await consumer.ReceiveAsync(TimeSpan.FromMicroseconds(5000));
            }
            consumer.Unsubscribe(false);
            await consumer.CloseAsync();
            await producer1.CloseAsync();
            await producer2.CloseAsync();
            await producer3.CloseAsync();
            await producer4.CloseAsync();
            Assert.True(messageSet > 0);
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
