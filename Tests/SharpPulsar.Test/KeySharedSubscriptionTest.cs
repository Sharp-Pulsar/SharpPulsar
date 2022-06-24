using System;
using System.Collections.Generic;
using System.Text;
using Akka.Util.Internal;
using SharpPulsar.Batch.Api;
using SharpPulsar.Common;
using SharpPulsar.Exceptions;
using SharpPulsar.Protocol.Proto;
using SharpPulsar.Schemas;
using SharpPulsar.User;
using Xunit;
using Xunit.Abstractions;
using SharpPulsar.TestContainer;
using System.Threading.Tasks;
using SharpPulsar.Test.Fixture;
using SharpPulsar.Builder;

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
    public class KeySharedSubscriptionTest
    {
        private readonly ITestOutputHelper _output;
        private readonly PulsarClient _client;
        public KeySharedSubscriptionTest(ITestOutputHelper output, PulsarFixture fixture)
        {
            _output = output;
            _client = fixture.Client;
        }

        private static readonly IList<string> Keys = new List<string> { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };

        private static readonly Random Random = new Random(DateTimeOffset.Now.Millisecond);

        private async Task NonKeySendAndReceiveWithHashRangeAutoSplitStickyKeyConsumerSelector(string topicType, bool enableBatch)
        {
            //this.conf.SubscriptionKeySharedEnable = true;
            var topic = topicType + "://public/default/key_shared_none_key-" + Guid.NewGuid();

            var consumer = await CreateConsumer(topic, $"consumer1-{Guid.NewGuid()}");

            //var consumer2 = await CreateConsumer(topic, $"consumer2-{Guid.NewGuid()}");

            //var consumer3 = await CreateConsumer(topic, $"consumer3-{Guid.NewGuid()}");


            var producer = await CreateProducer(topic, enableBatch);
            for (var i = 0; i < 1000; i++)
            {
                await producer.NewMessage().Value(i.ToString().GetBytes())
                    .SendAsync();
            }
            IDictionary<string, Consumer<byte[]>> keyToConsumer = new Dictionary<string, Consumer<byte[]>>();
            for (var i = 0; i < 998; i++)
            {
                var msg = await consumer.ReceiveAsync();
                if (msg == null)
                {
                    // Go to next consumer
                    break;
                }
                _output.WriteLine(Encoding.UTF8.GetString(msg.Data));

                await consumer.AcknowledgeAsync(msg);

                if (msg.HasKey())
                {
                    var assignedConsumer = keyToConsumer[msg.Key];
                    if (!keyToConsumer.ContainsKey(msg.Key))
                    {
                        // This is a new key
                        keyToConsumer[msg.Key] = consumer;
                    }
                    else
                    {
                        // The consumer should be the same
                        Assert.Equal(consumer, assignedConsumer);
                    }
                }
            }
            await producer.CloseAsync();
            await consumer.CloseAsync();
            //await consumer2.CloseAsync();
            //await consumer3.CloseAsync();
        }

        [Fact(Skip = "TestNonKeySendAndReceiveWithHashRangeAutoSplitStickyKeyConsumerSelectorNoBatch")]
        public async Task TestNonKeySendAndReceiveWithHashRangeAutoSplitStickyKeyConsumerSelectorNoBatch()
        {
            await NonKeySendAndReceiveWithHashRangeAutoSplitStickyKeyConsumerSelector("persistent", false);
        }
        [Fact(Skip = "TestNonKeySendAndReceiveWithHashRangeAutoSplitStickyKeyConsumerSelectorBatch)]
        public async Task TestNonKeySendAndReceiveWithHashRangeAutoSplitStickyKeyConsumerSelectorBatch()
        {

            await NonKeySendAndReceiveWithHashRangeAutoSplitStickyKeyConsumerSelector("persistent", true);
        }
        private async Task<Producer<byte[]>> CreateProducer(string topic, bool enableBatch, int batchSize = 500)
        {
            var pBuilder = new ProducerConfigBuilder<byte[]>();
            pBuilder.Topic(topic);
            if (enableBatch)
            {
                pBuilder.EnableBatching(true);
                pBuilder.BatchBuilder(IBatcherBuilder.KeyBased(_client.Log));
                pBuilder.BatchingMaxMessages(batchSize);
                pBuilder.BatchingMaxPublishDelay(TimeSpan.FromMilliseconds(5000));
            }

            return await _client.NewProducerAsync(pBuilder);
        }

        private async Task<Consumer<byte[]>> CreateConsumer(string topic, string consumerSub, KeySharedPolicy keySharedPolicy = null)
        {
            var builder = new ConsumerConfigBuilder<byte[]>();
            builder.Topic(topic);
            builder.SubscriptionName(consumerSub);
            builder.AckTimeout(TimeSpan.FromSeconds(5));
            builder.ForceTopicCreation(true);
            if (keySharedPolicy != null)
                builder.KeySharedPolicy(keySharedPolicy);
            builder.SubscriptionType(CommandSubscribe.SubType.KeyShared);
            return await _client.NewConsumerAsync(builder);
        }

        private async Task Receive(Consumer<byte[]> c)
        {
            // Add a key so that we know this key was already assigned to one consumer

            IDictionary<string, Consumer<byte[]>> keyToConsumer = new Dictionary<string, Consumer<byte[]>>();
            for (var i = 0; i < 998; i++)
            {
                var msg = await c.ReceiveAsync();
                if (msg == null)
                {
                    // Go to next consumer
                    break;
                }
                _output.WriteLine(Encoding.UTF8.GetString(msg.Data));

                await c.AcknowledgeAsync(msg);

                if (msg.HasKey())
                {
                    var assignedConsumer = keyToConsumer[msg.Key];
                    if (!keyToConsumer.ContainsKey(msg.Key))
                    {
                        // This is a new key
                        keyToConsumer[msg.Key] = c;
                    }
                    else
                    {
                        // The consumer should be the same
                        Assert.Equal(c, assignedConsumer);
                    }
                }
            }
        }

    }

}