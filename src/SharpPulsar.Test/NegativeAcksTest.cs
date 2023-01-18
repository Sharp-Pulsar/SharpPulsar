
using System.Text;
using SharpPulsar.Protocol.Proto;
using Xunit.Abstractions;
using SharpPulsar.TestContainer;
using SharpPulsar.Builder;
using SharpPulsar.Test.Fixture;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using SharpPulsar.Tracker;
using Akka.Actor;
using SharpPulsar.Tracker.Messages;
using Xunit;
using System.Text.Json;

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
    public class NegativeAcksTest : IAsyncLifetime
    {
        private PulsarClient _client;
        private readonly ITestOutputHelper _output;
        private PulsarSystem _system;
        private PulsarClientConfigBuilder _configBuilder;

        public NegativeAcksTest(ITestOutputHelper output, PulsarFixture fixture)
        {
            _output = output;
            _configBuilder = fixture.ConfigBuilder;
            _system = fixture.System;
        }

        [Fact]
        public async Task TestNegativeAcksBatch()
        {
            await TestNegativeAcks(true, false, CommandSubscribe.SubType.Exclusive, 10000, 8000).ConfigureAwait(false);
        }

        [Fact]
        public async Task TestNegativeAcksNoBatch()
        {
            await TestNegativeAcks(false, false, CommandSubscribe.SubType.Exclusive, 5000, 8000).ConfigureAwait(false);
        }
        [Fact]
        public async Task TestAddAndRemove()
        {
            var builder = new ConsumerConfigBuilder<byte[]>();
            builder.Topic("TestAckTracker");
            builder.SubscriptionName("TestAckTracker-sub");
            builder.AckTimeout(TimeSpan.FromSeconds(2));
            builder.AckTimeoutTickTime(TimeSpan.FromMilliseconds(50));
            var consumer = await _client.NewConsumerAsync(builder).ConfigureAwait(false);
            var unack = _client.ActorSystem.ActorOf(UnAckedChunckedMessageIdSequenceMap.Prop());
            var tracker = _client.ActorSystem.ActorOf(UnAckedMessageTracker<byte[]>.Prop(consumer.ConsumerActor, unack, builder.ConsumerConfigurationData));

            var empty = await tracker.Ask<bool>(Empty.Instance).ConfigureAwait(false);
            Assert.True(empty);

            var size = await tracker.Ask<long>(Size.Instance).ConfigureAwait(false);
            Assert.Equal(0, size);

            var mid = new MessageId(1L, 1L, -1);
            var added = await tracker.Ask<bool>(new Add(mid)).ConfigureAwait(false);
            Assert.True(added);
            added = await tracker.Ask<bool>(new Add(mid)).ConfigureAwait(false);
            Assert.False(added);
            size = await tracker.Ask<long>(Size.Instance).ConfigureAwait(false);
            Assert.Equal(1, size);

            tracker.Tell(Clear.Instance);

            added = await tracker.Ask<bool>(new Add(mid)).ConfigureAwait(false);
            Assert.True(added);

            size = await tracker.Ask<long>(Size.Instance).ConfigureAwait(false);
            Assert.Equal(1, size);

            var removed = await tracker.Ask<bool>(new Remove(mid)).ConfigureAwait(false);

            Assert.True(removed);

            empty = await tracker.Ask<bool>(Empty.Instance).ConfigureAwait(false);
            Assert.True(empty);

            size = await tracker.Ask<long>(Size.Instance).ConfigureAwait(false);
            Assert.Equal(0, size);
        }

        private async Task TestNegativeAcks(bool batching, bool usePartition, CommandSubscribe.SubType subscriptionType, int negAcksDelayMillis, int ackTimeout)
        {
            _output.WriteLine($"Test negative acks batching={batching} partitions={usePartition} subType={subscriptionType} negAckDelayMs={negAcksDelayMillis}");
            var topic = "testNegativeAcks-" + DateTime.Now.Ticks;

            var builder = new ConsumerConfigBuilder<byte[]>()
                .Topic(topic)
                .SubscriptionName($"sub1-{Guid.NewGuid()}")
                .AckTimeout(TimeSpan.FromMilliseconds(ackTimeout))
                .ForceTopicCreation(true)
                .AcknowledgmentGroupTime(TimeSpan.Zero)
                .NegativeAckRedeliveryDelay(TimeSpan.FromMilliseconds(negAcksDelayMillis))
                .SubscriptionType(subscriptionType);
            var consumer = await _client.NewConsumerAsync(builder).ConfigureAwait(false);

            var pBuilder = new ProducerConfigBuilder<byte[]>();
            pBuilder.Topic(topic);
            if (batching)
            {
                pBuilder.EnableBatching(batching);
                pBuilder.BatchingMaxPublishDelay(TimeSpan.FromMilliseconds(negAcksDelayMillis));
                pBuilder.BatchingMaxMessages(10);
            }
            var producer = await _client.NewProducerAsync(pBuilder).ConfigureAwait(false);

            ISet<string> sentMessages = new HashSet<string>();

            const int n = 10;
            for (var i = 0; i < n; i++)
            {
                var value = "test-" + i;
                await producer.SendAsync(Encoding.UTF8.GetBytes(value)).ConfigureAwait(false);
                sentMessages.Add(value);
            }
            await Task.Delay(TimeSpan.FromSeconds(1));
            for (var i = 0; i < n; i++)
            {
                var msg = await consumer.ReceiveAsync().ConfigureAwait(false);
                if (msg != null)
                {
                    var ms = Encoding.UTF8.GetString(msg.Data);
                    await consumer.NegativeAcknowledgeAsync(msg).ConfigureAwait(false);
                    _output.WriteLine(ms);
                }
            }

            ISet<string> receivedMessages = new HashSet<string>();

            await Task.Delay(TimeSpan.FromSeconds(1));
            // All the messages should be received again
            for (var i = 0; i < n; i++)
            {
                var msg = await consumer.ReceiveAsync().ConfigureAwait(false);
                if (msg != null)
                {
                    var ms = Encoding.UTF8.GetString(msg.Data);
                    _output.WriteLine(ms);
                    receivedMessages.Add(ms);
                    await consumer.AcknowledgeAsync(msg).ConfigureAwait(false);
                }
            }
            _output.WriteLine(JsonSerializer.Serialize(receivedMessages));
            //Assert.True(receivedMessages.Count > 3);
            //var nu = await consumer.ReceiveAsync();
            // There should be no more messages
            //Assert.Null(nu);
            await producer.CloseAsync().ConfigureAwait(false);
            await consumer.CloseAsync().ConfigureAwait(false);
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