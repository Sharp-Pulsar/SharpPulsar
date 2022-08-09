﻿using System;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using SharpPulsar.Builder;
using SharpPulsar.Test.Fixture;
using SharpPulsar.TestContainer;
using SharpPulsar.User;
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
    public class PartitionedProducerTest
    {
        private readonly ITestOutputHelper _output;
        private readonly PulsarClient _client;
        private readonly Admin.Public.Admin _admin;
        public PartitionedProducerTest(ITestOutputHelper output, PulsarFixture fixture)
        {
            _admin = new Admin.Public.Admin("http://localhost:8080/", new HttpClient()); ;
            _output = output;
            _client = fixture.Client;
        }
        [Fact]
        public virtual async Task TestGetNumOfPartitions()
        {
            var topicName = "partitioned-topic-" + Guid.NewGuid();
            var key = Guid.NewGuid().ToString();
            var subscriptionName = "partitioned-subscription";
            var pattern = new Regex("public/default/partitioned-topic-*");

            // create partitioned topic
            try
            {
                var asf = await _admin.CreatePartitionedTopicAsync("public", "default", topicName, 1);
            }
            catch (Exception ex)
            {
                var ss = ex.Message;
                Assert.Equal("Operation returned an invalid status code 'NoContent'", ss);
            }
            var partitions = await _admin.GetPartitionedMetadataAsync("public", "default", topicName);
            var s = partitions.Body.Partitions;
            Assert.Equal(1, s);

            // 2. create producer
            var messagePredicate = "partitioned-producer" + Guid.NewGuid() + "-";
            var partitioProducer = await _client.NewPartitionedProducerAsync(new ProducerConfigBuilder<byte[]>()
                .Topic(topicName)
                .EnableLazyStartPartitionedProducers(true));

            var producers = await partitioProducer.Producers();

            // 5. produce data
            foreach (var producer in producers)
            {
                await producer.SendAsync(Encoding.UTF8.GetBytes(messagePredicate + "producer1-"));
            }
            var messageSet = 0;
            var consumer = await _client.NewConsumerAsync(new ConsumerConfigBuilder<byte[]>()
                .TopicsPattern(pattern)
                .PatternAutoDiscoveryPeriod(2)
                .ForceTopicCreation(true)
                .SubscriptionName(subscriptionName)
                .SubscriptionType(SubType.Shared)
                .AckTimeout(TimeSpan.FromMilliseconds(60000)));
            await Task.Delay(TimeSpan.FromSeconds(10));
            var message = await consumer.ReceiveAsync(TimeSpan.FromMilliseconds(5100));
            if (message == null)
            {
                await Task.Delay(TimeSpan.FromSeconds(10));
                message = await consumer.ReceiveAsync();
            }

            while (message != null)
            {
                var m = (TopicMessage<byte[]>)message;
                messageSet++;
                await consumer.AcknowledgeAsync(m);
                _output.WriteLine($"Consumer acknowledged : {Encoding.UTF8.GetString(message.Data)} from topic: {m.Topic}");
                message = await consumer.ReceiveAsync();
            }
            consumer.Unsubscribe();
            foreach (var producer in producers)
            {
                await producer.CloseAsync();
            }

            Assert.True(producers.Count > 0);
        }
        [Fact]
        public virtual async Task TestBinaryProtoToGetTopicsOfNamespacePersistent()
        {
            var key = Guid.NewGuid().ToString();
            var subscriptionName = "regex-subscription";
            var topicName1 = "persistent://public/default/reg-topic-1-" + key;
            var topicName2 = "persistent://public/default/reg-topic-2-" + key;
            var topicName3 = "persistent://public/default/reg-topic-3-" + key;
            var topicName4 = "non-persistent://public/default/reg-topic-4-" + key;
            var pattern = new Regex("public/default/reg-topic*");


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
            await Task.Delay(TimeSpan.FromSeconds(10));
            var message = await consumer.ReceiveAsync(TimeSpan.FromMilliseconds(1100));
            if (message == null)
            {
                await Task.Delay(TimeSpan.FromSeconds(10));
                message = await consumer.ReceiveAsync();
            }

            while (message != null)
            {
                var m = (TopicMessage<byte[]>)message;
                messageSet++;
                await consumer.AcknowledgeAsync(m);
                _output.WriteLine($"Consumer acknowledged : {Encoding.UTF8.GetString(message.Data)} from topic: {m.Topic}");
                message = await consumer.ReceiveAsync();
            }
            consumer.Unsubscribe();
            await consumer.CloseAsync();
            await producer1.CloseAsync();
            await producer2.CloseAsync();
            await producer3.CloseAsync();
            await producer4.CloseAsync();
            Assert.True(messageSet > 0);
        }

    }
}