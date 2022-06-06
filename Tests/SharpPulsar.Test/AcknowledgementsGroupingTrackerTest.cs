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

using System.Collections.Generic;
using SharpPulsar.Protocol.Proto;
using SharpPulsar.Tracker;
using SharpPulsar.Tracker.Messages;
using SharpPulsar.User;
using Xunit;
using Xunit.Abstractions;
using Akka.Actor;
using System;
using System.Threading.Tasks;
using SharpPulsar.TestContainer;
using SharpPulsar.Test.Fixture;
using SharpPulsar.Builder;

namespace SharpPulsar.Test
{
    [Collection(nameof(PulsarCollection))]
    public class AcknowledgementsGroupingTrackerTest
    {
        private readonly ITestOutputHelper _output;
        private  PulsarClient _client;
        private  ActorSystem _system;
        public AcknowledgementsGroupingTrackerTest(ITestOutputHelper output, PulsarFixture fixture)
        {
            _output = output;
            _client = fixture.Client;
            _system = fixture.PulsarSystem.System;
        }

        [Fact]
        public async Task TestAckTracker()
        {
            var builder = new ConsumerConfigBuilder<byte[]>();
            builder.AcknowledgmentGroupTime(TimeSpan.FromMilliseconds(5000));
            builder.Topic($"TestAckTracker-{Guid.NewGuid()}");
            builder.SubscriptionName($"TestAckTracker-sub-{Guid.NewGuid()}");
            var conf = builder.ConsumerConfigurationData;
            var consumer = await _client.NewConsumerAsync(builder);
            var unack = _system.ActorOf(UnAckedChunckedMessageIdSequenceMap.Prop());
            var tracker = _client.ActorSystem.ActorOf(PersistentAcknowledgmentsGroupingTracker<byte[]>.Prop(unack, consumer.ConsumerActor, consumer.ConsumerActor/*dummy*/, 1, consumer.ConsumerActor, conf));

            var msg1 = new MessageId(5, 1, 0);
            var msg2 = new MessageId(5, 2, 0);
            var msg3 = new MessageId(5, 3, 0);
            var msg4 = new MessageId(5, 4, 0);
            var msg5 = new MessageId(5, 5, 0);
            var msg6 = new MessageId(5, 6, 0);
            var isDuplicate = await tracker.Ask<bool>(new IsDuplicate(msg1));
            Assert.False(isDuplicate);
            tracker.Tell(new AddAcknowledgment(msg1, CommandAck.AckType.Individual, new Dictionary<string, long>()));
            isDuplicate = await tracker.Ask<bool>(new IsDuplicate(msg1));
            Assert.True(isDuplicate);

            isDuplicate = await tracker.Ask<bool>(new IsDuplicate(msg2));
            Assert.False(isDuplicate);

            tracker.Tell(new AddAcknowledgment(msg5, CommandAck.AckType.Cumulative, new Dictionary<string, long>()));
            isDuplicate = await tracker.Ask<bool>(new IsDuplicate(msg1));
            Assert.True(isDuplicate);
            isDuplicate = await tracker.Ask<bool>(new IsDuplicate(msg2));
            Assert.True(isDuplicate);
            isDuplicate = await tracker.Ask<bool>(new IsDuplicate(msg3));
            Assert.True(isDuplicate);

            isDuplicate = await tracker.Ask<bool>(new IsDuplicate(msg4));
            Assert.True(isDuplicate);
            isDuplicate = await tracker.Ask<bool>(new IsDuplicate(msg5));
            Assert.True(isDuplicate);
            isDuplicate = await tracker.Ask<bool>(new IsDuplicate(msg6));
            Assert.False(isDuplicate);

            // Flush while disconnected. the internal tracking will not change
            tracker.Tell(FlushPending.Instance);

            isDuplicate = await tracker.Ask<bool>(new IsDuplicate(msg1));
            Assert.True(isDuplicate);
            isDuplicate = await tracker.Ask<bool>(new IsDuplicate(msg2));
            Assert.True(isDuplicate);
            isDuplicate = await tracker.Ask<bool>(new IsDuplicate(msg3));
            Assert.True(isDuplicate);

            isDuplicate = await tracker.Ask<bool>(new IsDuplicate(msg4));
            Assert.True(isDuplicate);
            isDuplicate = await tracker.Ask<bool>(new IsDuplicate(msg5));
            Assert.True(isDuplicate);
            isDuplicate = await tracker.Ask<bool>(new IsDuplicate(msg6));
            Assert.False(isDuplicate);

            tracker.Tell(new AddAcknowledgment(msg6, CommandAck.AckType.Individual, new Dictionary<string, long>()));
            isDuplicate = await tracker.Ask<bool>(new IsDuplicate(msg6));
            Assert.True(isDuplicate);

            tracker.Tell(FlushPending.Instance);

            isDuplicate = await tracker.Ask<bool>(new IsDuplicate(msg1));
            Assert.True(isDuplicate);
            isDuplicate = await tracker.Ask<bool>(new IsDuplicate(msg2));
            Assert.True(isDuplicate);
            isDuplicate = await tracker.Ask<bool>(new IsDuplicate(msg3));
            Assert.True(isDuplicate);

            isDuplicate = await tracker.Ask<bool>(new IsDuplicate(msg4));
            Assert.True(isDuplicate);
            isDuplicate = await tracker.Ask<bool>(new IsDuplicate(msg5));
            Assert.True(isDuplicate);
            isDuplicate = await tracker.Ask<bool>(new IsDuplicate(msg6));
            Assert.False(isDuplicate);

            await tracker.GracefulStop(TimeSpan.FromSeconds(1));
        }

        [Fact]
        public async Task TestImmediateAckingTracker()
        {
            var builder = new ConsumerConfigBuilder<byte[]>();
            builder.AcknowledgmentGroupTime(TimeSpan.Zero);
            builder.Topic($"TestAckTracker-{Guid.NewGuid()}");
            builder.SubscriptionName($"TestAckTracker-sub-{Guid.NewGuid()}");
            var conf = builder.ConsumerConfigurationData;
            var consumer = await _client.NewConsumerAsync(builder);
            var unack = _system.ActorOf(UnAckedChunckedMessageIdSequenceMap.Prop());
            var tracker = _client.ActorSystem.ActorOf(PersistentAcknowledgmentsGroupingTracker<byte[]>.Prop(unack, consumer.ConsumerActor, consumer.ConsumerActor/*dummy*/, 1, consumer.ConsumerActor, conf));

            var msg1 = new MessageId(5, 1, 0);
            var msg2 = new MessageId(5, 2, 0);

            var isDuplicate = await tracker.Ask<bool>(new IsDuplicate(msg1));
            Assert.False(isDuplicate);


            tracker.Tell(new AddAcknowledgment(msg1, CommandAck.AckType.Individual, new Dictionary<string, long>()));
            isDuplicate = await tracker.Ask<bool>(new IsDuplicate(msg1));
            Assert.False(isDuplicate);

            tracker.Tell(FlushPending.Instance);
            isDuplicate = await tracker.Ask<bool>(new IsDuplicate(msg1));
            Assert.False(isDuplicate);

            tracker.Tell(new AddAcknowledgment(msg2, CommandAck.AckType.Individual, new Dictionary<string, long>()));
            // Since we were connected, the ack went out immediately
            isDuplicate = await tracker.Ask<bool>(new IsDuplicate(msg2));
            Assert.False(isDuplicate);
            await tracker.GracefulStop(TimeSpan.FromSeconds(1));
        }

        [Fact]
        public async Task TestAckTrackerMultiAck()
        {
            var builder = new ConsumerConfigBuilder<byte[]>();
            builder.AcknowledgmentGroupTime(TimeSpan.FromMilliseconds(5000));
            builder.Topic($"TestAckTracker-{Guid.NewGuid()}");
            builder.SubscriptionName($"TestAckTracker-sub-{Guid.NewGuid()}");
            var conf = builder.ConsumerConfigurationData;
            var consumer = await _client.NewConsumerAsync(builder);
            var unack = _system.ActorOf(UnAckedChunckedMessageIdSequenceMap.Prop());
            var tracker = _client.ActorSystem.ActorOf(PersistentAcknowledgmentsGroupingTracker<byte[]>.Prop(unack, consumer.ConsumerActor, consumer.ConsumerActor/*dummy*/, 1, consumer.ConsumerActor, conf));

            var msg1 = new MessageId(5, 1, 0);
            var msg2 = new MessageId(5, 2, 0);
            var msg3 = new MessageId(5, 3, 0);
            var msg4 = new MessageId(5, 4, 0);
            var msg5 = new MessageId(5, 5, 0);
            var msg6 = new MessageId(5, 6, 0);

            var isDuplicate = await tracker.Ask<bool>(new IsDuplicate(msg1));
            Assert.False(isDuplicate);

            tracker.Tell(new AddAcknowledgment(msg1, CommandAck.AckType.Individual, new Dictionary<string, long>()));
            isDuplicate = await tracker.Ask<bool>(new IsDuplicate(msg1));
            Assert.True(isDuplicate);

            isDuplicate = await tracker.Ask<bool>(new IsDuplicate(msg2));
            Assert.False(isDuplicate);

            tracker.Tell(new AddAcknowledgment(msg5, CommandAck.AckType.Cumulative, new Dictionary<string, long>()));

            isDuplicate = await tracker.Ask<bool>(new IsDuplicate(msg1));
            Assert.True(isDuplicate);
            isDuplicate = await tracker.Ask<bool>(new IsDuplicate(msg2));
            Assert.True(isDuplicate);
            isDuplicate = await tracker.Ask<bool>(new IsDuplicate(msg3));
            Assert.True(isDuplicate);

            isDuplicate = await tracker.Ask<bool>(new IsDuplicate(msg4));
            Assert.True(isDuplicate);
            isDuplicate = await tracker.Ask<bool>(new IsDuplicate(msg5));
            Assert.True(isDuplicate);
            isDuplicate = await tracker.Ask<bool>(new IsDuplicate(msg6));
            Assert.False(isDuplicate);

            // Flush while disconnected. the internal tracking will not change
            tracker.Tell(FlushPending.Instance);

            isDuplicate = await tracker.Ask<bool>(new IsDuplicate(msg1));
            Assert.True(isDuplicate);
            isDuplicate = await tracker.Ask<bool>(new IsDuplicate(msg2));
            Assert.True(isDuplicate);
            isDuplicate = await tracker.Ask<bool>(new IsDuplicate(msg3));
            Assert.True(isDuplicate);

            isDuplicate = await tracker.Ask<bool>(new IsDuplicate(msg4));
            Assert.True(isDuplicate);
            isDuplicate = await tracker.Ask<bool>(new IsDuplicate(msg5));
            Assert.True(isDuplicate);
            isDuplicate = await tracker.Ask<bool>(new IsDuplicate(msg6));
            Assert.False(isDuplicate);

            tracker.Tell(new AddAcknowledgment(msg6, CommandAck.AckType.Individual, new Dictionary<string, long>()));
            isDuplicate = await tracker.Ask<bool>(new IsDuplicate(msg6));
            Assert.True(isDuplicate);


            tracker.Tell(FlushPending.Instance);

            isDuplicate = await tracker.Ask<bool>(new IsDuplicate(msg1));
            Assert.True(isDuplicate);
            isDuplicate = await tracker.Ask<bool>(new IsDuplicate(msg2));
            Assert.True(isDuplicate);
            isDuplicate = await tracker.Ask<bool>(new IsDuplicate(msg3));
            Assert.True(isDuplicate);

            isDuplicate = await tracker.Ask<bool>(new IsDuplicate(msg4));
            Assert.True(isDuplicate);
            isDuplicate = await tracker.Ask<bool>(new IsDuplicate(msg5));
            Assert.True(isDuplicate);
            isDuplicate = await tracker.Ask<bool>(new IsDuplicate(msg6));
            Assert.False(isDuplicate);

            await tracker.GracefulStop(TimeSpan.FromSeconds(1));
        }
        
    }

}