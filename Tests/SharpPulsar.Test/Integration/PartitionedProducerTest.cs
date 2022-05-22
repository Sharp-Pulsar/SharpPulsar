using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using SharpPulsar.Builder;
using SharpPulsar.Common;
using SharpPulsar.Configuration;
using SharpPulsar.Test.Fixture;
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
namespace SharpPulsar.Test.Integration
{
    [Collection(nameof(MultiTopicIntegrationCollection))]
    public class PartitionedProducerTest
    {
        private readonly ITestOutputHelper _output;
        private readonly PulsarClient _client;
        private readonly Admin.Public.Admin _admin;
        public PartitionedProducerTest(ITestOutputHelper output, PulsarMultiTopicFixture fixture)
        {
            _admin = new Admin.Public.Admin("http://localhost:8080/", new HttpClient()); ;
            _output = output;
            _client = fixture.Client;
        }
        [Fact]
        public virtual async Task TestGetNumOfPartitions()
        {
            const string topicName = "persistent://my-property/my-ns/one-partitioned-topic";
            const string subscriptionName = "my-sub-";

            // create partitioned topic
            await _admin.CreatePartitionedTopicAsync("persistent", "public/default", topicName, 5) ;
            var partitions = await _admin.GetPartitionedMetadataAsync("persistent", "public/default", topicName);
            var s = partitions.Body.Partitions;
            Assert.Equal(5, s);

                       // 2. create producer
            var messagePredicate = "my-message-" + key + "-";

            var partitioProducer = await _client.NewProducerAsync(new ProducerConfigBuilder<byte[]>()
                .Topic(topicName)
                .EnableLazyStartPartitionedProducers(true));

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
            var message = await consumer.ReceiveAsync();
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
