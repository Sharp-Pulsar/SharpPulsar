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
using SharpPulsar.Configuration;
using SharpPulsar.Protocol.Proto;
using SharpPulsar.Test.Fixtures;
using SharpPulsar.Tracker;
using SharpPulsar.Tracker.Messages;
using SharpPulsar.User;
using SharpPulsar.Extension;
using SharpPulsar.Utils;
using Xunit;
using Xunit.Abstractions;
using Akka.Actor;
using System;
using System.Threading.Tasks;

namespace SharpPulsar.Test.Tracker
{
	[Collection(nameof(PulsarTests))]
	public class AcknowledgementsGroupingTrackerTest
	{
        private readonly ITestOutputHelper _output;
		private readonly PulsarClient _client;
		public AcknowledgementsGroupingTrackerTest(ITestOutputHelper output, PulsarStandaloneClusterFixture fixture)
		{
			_output = output;
			_client = fixture.Client;
		}

		[Fact]
		public void TestAckTracker()
		{
			var builder = new ConsumerConfigBuilder<sbyte[]>();
			builder.AcknowledgmentGroupTime(10000);
			builder.Topic($"TestAckTracker-{Guid.NewGuid()}");
			builder.SubscriptionName($"TestAckTracker-sub-{Guid.NewGuid()}");
			var conf = builder.ConsumerConfigurationData;
			var consumer = _client.NewConsumer(builder);
            var tracker = _client.ActorSystem.ActorOf(PersistentAcknowledgmentsGroupingTracker<sbyte[]>.Prop(consumer.ConsumerActor, 1, consumer.ConsumerActor, conf));

			var msg1 = new MessageId(5, 1, 0);
			var msg2 = new MessageId(5, 2, 0);
			var msg3 = new MessageId(5, 3, 0);
			var msg4 = new MessageId(5, 4, 0);
			var msg5 = new MessageId(5, 5, 0);
			var msg6 = new MessageId(5, 6, 0);
			var isDuplicate = tracker.AskFor<bool>(new IsDuplicate(msg1)).GetAwaiter().GetResult();
			Assert.False(isDuplicate);
			tracker.Tell(new AddAcknowledgment(msg1, CommandAck.AckType.Individual, new Dictionary<string, long>(), null));
			isDuplicate = tracker.AskFor<bool>(new IsDuplicate(msg1)).GetAwaiter().GetResult();
			Assert.True(isDuplicate);

			isDuplicate = tracker.AskFor<bool>(new IsDuplicate(msg2)).GetAwaiter().GetResult();
			Assert.False(isDuplicate);

			tracker.Tell(new AddAcknowledgment(msg5, CommandAck.AckType.Cumulative, new Dictionary<string, long>(), null));
			isDuplicate = tracker.AskFor<bool>(new IsDuplicate(msg1)).GetAwaiter().GetResult();
			Assert.True(isDuplicate);
			isDuplicate = tracker.AskFor<bool>(new IsDuplicate(msg2)).GetAwaiter().GetResult();
			Assert.True(isDuplicate);
			isDuplicate = tracker.AskFor<bool>(new IsDuplicate(msg3)).GetAwaiter().GetResult();
			Assert.True(isDuplicate);

			isDuplicate = tracker.AskFor<bool>(new IsDuplicate(msg4)).GetAwaiter().GetResult();
			Assert.True(isDuplicate);
			isDuplicate = tracker.AskFor<bool>(new IsDuplicate(msg5)).GetAwaiter().GetResult();
			Assert.True(isDuplicate);
			isDuplicate = tracker.AskFor<bool>(new IsDuplicate(msg6)).GetAwaiter().GetResult();
			Assert.False(isDuplicate);

			// Flush while disconnected. the internal tracking will not change
			tracker.Tell(FlushPending.Instance);

			isDuplicate = tracker.AskFor<bool>(new IsDuplicate(msg1)).GetAwaiter().GetResult();
			Assert.True(isDuplicate);
			isDuplicate = tracker.AskFor<bool>(new IsDuplicate(msg2)).GetAwaiter().GetResult();
			Assert.True(isDuplicate);
			isDuplicate = tracker.AskFor<bool>(new IsDuplicate(msg3)).GetAwaiter().GetResult();
			Assert.True(isDuplicate);

			isDuplicate = tracker.AskFor<bool>(new IsDuplicate(msg4)).GetAwaiter().GetResult();
			Assert.True(isDuplicate);
			isDuplicate = tracker.AskFor<bool>(new IsDuplicate(msg5)).GetAwaiter().GetResult();
			Assert.True(isDuplicate);
			isDuplicate = tracker.AskFor<bool>(new IsDuplicate(msg6)).GetAwaiter().GetResult();
			Assert.False(isDuplicate);

			tracker.Tell(new AddAcknowledgment(msg6, CommandAck.AckType.Individual, new Dictionary<string, long>(), null));
			isDuplicate = tracker.AskFor<bool>(new IsDuplicate(msg6)).GetAwaiter().GetResult();
			Assert.True(isDuplicate);
			
			tracker.Tell(FlushPending.Instance);

			isDuplicate = tracker.AskFor<bool>(new IsDuplicate(msg1)).GetAwaiter().GetResult();
			Assert.True(isDuplicate);
			isDuplicate = tracker.AskFor<bool>(new IsDuplicate(msg2)).GetAwaiter().GetResult();
			Assert.True(isDuplicate);
			isDuplicate = tracker.AskFor<bool>(new IsDuplicate(msg3)).GetAwaiter().GetResult();
			Assert.True(isDuplicate);

			isDuplicate = tracker.AskFor<bool>(new IsDuplicate(msg4)).GetAwaiter().GetResult();
			Assert.True(isDuplicate);
			isDuplicate = tracker.AskFor<bool>(new IsDuplicate(msg5)).GetAwaiter().GetResult();
			Assert.True(isDuplicate);
			isDuplicate = tracker.AskFor<bool>(new IsDuplicate(msg6)).GetAwaiter().GetResult();
			Assert.False(isDuplicate);

			tracker.GracefulStop(TimeSpan.FromSeconds(1));
		}

		[Fact]
		public  void TestImmediateAckingTracker()
		{
			var builder = new ConsumerConfigBuilder<sbyte[]>();
			builder.AcknowledgmentGroupTime(0);
			builder.Topic($"TestAckTracker-{Guid.NewGuid()}");
			builder.SubscriptionName($"TestAckTracker-sub-{Guid.NewGuid()}");
			var conf = builder.ConsumerConfigurationData;
			var consumer = _client.NewConsumer(builder);
			var tracker = _client.ActorSystem.ActorOf(PersistentAcknowledgmentsGroupingTracker<sbyte[]>.Prop(consumer.ConsumerActor, 1, consumer.ConsumerActor, conf));

			var msg1 = new MessageId(5, 1, 0);
			var msg2 = new MessageId(5, 2, 0);

			var isDuplicate = tracker.AskFor<bool>(new IsDuplicate(msg1)).GetAwaiter().GetResult();
			Assert.False(isDuplicate);


			tracker.Tell(new AddAcknowledgment(msg1, CommandAck.AckType.Individual, new Dictionary<string, long>(), null));
			isDuplicate = tracker.AskFor<bool>(new IsDuplicate(msg1)).GetAwaiter().GetResult();
			Assert.False(isDuplicate);
			
			tracker.Tell(FlushPending.Instance);
			isDuplicate = tracker.AskFor<bool>(new IsDuplicate(msg1)).GetAwaiter().GetResult();
			Assert.False(isDuplicate);

			tracker.Tell(new AddAcknowledgment(msg2, CommandAck.AckType.Individual, new Dictionary<string, long>(), null));
			// Since we were connected, the ack went out immediately
			isDuplicate = tracker.AskFor<bool>(new IsDuplicate(msg2)).GetAwaiter().GetResult();
			Assert.False(isDuplicate);
			tracker.GracefulStop(TimeSpan.FromSeconds(1));
		}

		[Fact]
		public void TestAckTrackerMultiAck()
		{
			var builder = new ConsumerConfigBuilder<sbyte[]>();
			builder.AcknowledgmentGroupTime((long)ConvertTimeUnits.ConvertMillisecondsToMicroseconds(10000));
			builder.Topic($"TestAckTracker-{Guid.NewGuid()}");
			builder.SubscriptionName($"TestAckTracker-sub-{Guid.NewGuid()}");
			var conf = builder.ConsumerConfigurationData;
			var consumer = _client.NewConsumer(builder);
			var tracker = _client.ActorSystem.ActorOf(PersistentAcknowledgmentsGroupingTracker<sbyte[]>.Prop(consumer.ConsumerActor, 1, consumer.ConsumerActor, conf));

			var msg1 = new MessageId(5, 1, 0);
			var msg2 = new MessageId(5, 2, 0);
			var msg3 = new MessageId(5, 3, 0);
			var msg4 = new MessageId(5, 4, 0);
			var msg5 = new MessageId(5, 5, 0);
			var msg6 = new MessageId(5, 6, 0);

			var isDuplicate = tracker.AskFor<bool>(new IsDuplicate(msg1)).GetAwaiter().GetResult();
			Assert.False(isDuplicate);

			tracker.Tell(new AddAcknowledgment(msg1, CommandAck.AckType.Individual, new Dictionary<string, long>(), null));
			isDuplicate = tracker.AskFor<bool>(new IsDuplicate(msg1)).GetAwaiter().GetResult();
			Assert.True(isDuplicate);

			isDuplicate = tracker.AskFor<bool>(new IsDuplicate(msg2)).GetAwaiter().GetResult();
			Assert.False(isDuplicate);

			tracker.Tell(new AddAcknowledgment(msg5, CommandAck.AckType.Cumulative, new Dictionary<string, long>(), null));

			isDuplicate = tracker.AskFor<bool>(new IsDuplicate(msg1)).GetAwaiter().GetResult();
			Assert.True(isDuplicate);
			isDuplicate = tracker.AskFor<bool>(new IsDuplicate(msg2)).GetAwaiter().GetResult();
			Assert.True(isDuplicate);
			isDuplicate = tracker.AskFor<bool>(new IsDuplicate(msg3)).GetAwaiter().GetResult();
			Assert.True(isDuplicate);

			isDuplicate = tracker.AskFor<bool>(new IsDuplicate(msg4)).GetAwaiter().GetResult();
			Assert.True(isDuplicate);
			isDuplicate = tracker.AskFor<bool>(new IsDuplicate(msg5)).GetAwaiter().GetResult();
			Assert.True(isDuplicate);
			isDuplicate = tracker.AskFor<bool>(new IsDuplicate(msg6)).GetAwaiter().GetResult();
			Assert.False(isDuplicate);

			// Flush while disconnected. the internal tracking will not change
			tracker.Tell(FlushPending.Instance);

			isDuplicate = tracker.AskFor<bool>(new IsDuplicate(msg1)).GetAwaiter().GetResult();
			Assert.True(isDuplicate);
			isDuplicate = tracker.AskFor<bool>(new IsDuplicate(msg2)).GetAwaiter().GetResult();
			Assert.True(isDuplicate);
			isDuplicate = tracker.AskFor<bool>(new IsDuplicate(msg3)).GetAwaiter().GetResult();
			Assert.True(isDuplicate);

			isDuplicate = tracker.AskFor<bool>(new IsDuplicate(msg4)).GetAwaiter().GetResult();
			Assert.True(isDuplicate);
			isDuplicate = tracker.AskFor<bool>(new IsDuplicate(msg5)).GetAwaiter().GetResult();
			Assert.True(isDuplicate);
			isDuplicate = tracker.AskFor<bool>(new IsDuplicate(msg6)).GetAwaiter().GetResult();
			Assert.False(isDuplicate);

			tracker.Tell(new AddAcknowledgment(msg6, CommandAck.AckType.Individual, new Dictionary<string, long>(), null));
			isDuplicate = tracker.AskFor<bool>(new IsDuplicate(msg6)).GetAwaiter().GetResult();
			Assert.True(isDuplicate);


			tracker.Tell(FlushPending.Instance);

			isDuplicate = tracker.AskFor<bool>(new IsDuplicate(msg1)).GetAwaiter().GetResult();
			Assert.True(isDuplicate);
			isDuplicate = tracker.AskFor<bool>(new IsDuplicate(msg2)).GetAwaiter().GetResult();
			Assert.True(isDuplicate);
			isDuplicate = tracker.AskFor<bool>(new IsDuplicate(msg3)).GetAwaiter().GetResult();
			Assert.True(isDuplicate);

			isDuplicate = tracker.AskFor<bool>(new IsDuplicate(msg4)).GetAwaiter().GetResult();
			Assert.True(isDuplicate);
			isDuplicate = tracker.AskFor<bool>(new IsDuplicate(msg5)).GetAwaiter().GetResult();
			Assert.True(isDuplicate);
			isDuplicate = tracker.AskFor<bool>(new IsDuplicate(msg6)).GetAwaiter().GetResult();
			Assert.False(isDuplicate);

			tracker.GracefulStop(TimeSpan.FromSeconds(1));
		}

	}

}