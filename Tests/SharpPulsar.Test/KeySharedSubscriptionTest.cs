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

            var consumer1 = await CreateConsumer(topic, $"consumer1-{Guid.NewGuid()}");

            var consumer2 = await CreateConsumer(topic, $"consumer2-{Guid.NewGuid()}");

            var consumer3 = await CreateConsumer(topic, $"consumer3-{Guid.NewGuid()}");

            var producer = await CreateProducer(topic, enableBatch);

            for (var i = 0; i < 50; i++)
            {
                await producer.NewMessage().Key(i.ToString()).Value(i.ToString().GetBytes())
                    .SendAsync();
            }
            //producer.Flush();
            //await Task.Delay(3000);
               
            await Receive(new List<Consumer<byte[]>> { consumer1, consumer2, consumer3 });
            await producer.CloseAsync();
            await consumer1.CloseAsync();
            await consumer2.CloseAsync();
            await consumer3.CloseAsync();
        }

        [Fact]
        public async Task TestNonKeySendAndReceiveWithHashRangeAutoSplitStickyKeyConsumerSelectorNoBatch()
        {
            await NonKeySendAndReceiveWithHashRangeAutoSplitStickyKeyConsumerSelector("persistent", false);
        }
        [Fact]
        public async Task TestNonKeySendAndReceiveWithHashRangeAutoSplitStickyKeyConsumerSelectorBatch()
        {
            await NonKeySendAndReceiveWithHashRangeAutoSplitStickyKeyConsumerSelector("persistent", true);
        }
        private async Task<Producer<byte[]>> CreateProducer(string topic, bool enableBatch, int batchSize = 25)
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
            builder.AckTimeout(TimeSpan.FromSeconds(10));
            builder.ForceTopicCreation(true);
            builder.BatchReceivePolicy(new BatchReceivePolicy.Builder().MaxNumMessages(41).Build());
            if (keySharedPolicy != null)
                builder.KeySharedPolicy(keySharedPolicy);
            builder.SubscriptionType(CommandSubscribe.SubType.KeyShared);
            return await _client.NewConsumerAsync(builder);
        }

        private async Task Receive(IList<Consumer<byte[]>> consumers)
        {
            // Add a key so that we know this key was already assigned to one consumer

            IDictionary<string, Consumer<byte[]>> keyToConsumer = new Dictionary<string, Consumer<byte[]>>();

            foreach (var c in consumers)
            {
                while (true)
                {
                    var msg = await c.ReceiveAsync(TimeSpan.FromSeconds(10));
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

        private async Task ReceiveAndCheck(IEnumerable<KeyValue<Consumer<byte[]>, int>> checkList)
        {
            var consumerKeys = new Dictionary<Consumer<byte[]>, ISet<string>>();
            foreach (var check in checkList)
            {
                if (check.Value % 2 != 0)
                {
                    throw new ArgumentException();
                }
                var received = 0;
                var lastMessageForKey = new Dictionary<string, Message<byte[]>>();
                for (int? i = 0; i.Value < check.Value; i++)
                {
                    var message = await check.Key.ReceiveAsync();
                    if (i % 2 == 0)
                    {
                        await check.Key.AcknowledgeAsync(message);
                    }
                    var key = message.HasOrderingKey() ? Encoding.UTF8.GetString((byte[])(Array)message.OrderingKey) : message.Key;
                    _output.WriteLine($"[{check.Key}] Receive message key: {key} value: {Encoding.UTF8.GetString(message.Data)} messageId: {message.MessageId}");
                    // check messages is order by key
                    if (!lastMessageForKey.TryGetValue(key, out var msgO))
                    {
                        Assert.NotNull(message);
                    }
                    else
                    {
                        var l = Convert.ToInt32(Encoding.UTF8.GetString(msgO.Data));
                        var o = Convert.ToInt32(Encoding.UTF8.GetString(message.Data));
                        Assert.True(o.CompareTo(l) > 0);
                    }
                    lastMessageForKey[key] = (Message<byte[]>)message;
                    if (!consumerKeys.ContainsKey(check.Key))
                        consumerKeys.Add(check.Key, new HashSet<string>());
                    consumerKeys[check.Key].Add(key);
                    received++;
                }
                Assert.Equal(check.Value, received);
                var redeliveryCount = check.Value / 2;
                _output.WriteLine($"[{check.Key}] Consumer wait for {redeliveryCount} messages redelivery ...");
                await Task.Delay(TimeSpan.FromSeconds(redeliveryCount));
                // messages not acked, test redelivery
                lastMessageForKey = new Dictionary<string, Message<byte[]>>();
                for (var i = 0; i < redeliveryCount; i++)
                {
                    var message = await check.Key.ReceiveAsync();
                    received++;
                    await check.Key.AcknowledgeAsync(message);
                    var key = message.HasOrderingKey() ? Encoding.UTF8.GetString((byte[])(Array)message.OrderingKey) : message.Key;
                    _output.WriteLine($"[{check.Key}] Receive message key: {key} value: {Encoding.UTF8.GetString(message.Data)} messageId: {message.MessageId}");
                    // check redelivery messages is order by key
                    if (!lastMessageForKey.TryGetValue(key, out var msgO))
                    {
                        Assert.NotNull(message);
                    }
                    else
                    {
                        var l = Convert.ToInt32(Encoding.UTF8.GetString(msgO.Data));
                        var o = Convert.ToInt32(Encoding.UTF8.GetString(message.Data));
                        Assert.True(o.CompareTo(l) > 0);
                    }
                    lastMessageForKey[key] = (Message<byte[]>)message;
                }
                Message<byte[]> noMessages = null;
                try
                {
                    noMessages = (Message<byte[]>)await check.Key.ReceiveAsync();
                }
                catch (PulsarClientException)
                {
                }
                Assert.Null(noMessages);//, "redeliver too many messages.");
                Assert.Equal(check.Value + redeliveryCount, received);
            }
            ISet<string> allKeys = new HashSet<string>();
            consumerKeys.ForEach(x =>
            {
                x.Value.ForEach(key =>
                {
                    Assert.True(allKeys.Add(key), "Key " + key + "is distributed to multiple consumers.");
                });
            });
        }
    }

}