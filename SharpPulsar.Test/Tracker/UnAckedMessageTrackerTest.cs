using SharpPulsar.Configuration;
using SharpPulsar.Impl;
using SharpPulsar.Test.Fixtures;
using SharpPulsar.Tracker;
using SharpPulsar.Tracker.Messages;
using SharpPulsar.User;
using SharpPulsar.Extension;
using Xunit;
using Xunit.Abstractions;
using Akka.Actor;

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
namespace SharpPulsar.Test.Tracker
{
	[Collection(nameof(PulsarTests))]
	public class UnAckedMessageTrackerTest
    {

		private readonly ITestOutputHelper _output;
		private readonly PulsarSystem _system;
		private readonly PulsarClient _client;
		public UnAckedMessageTrackerTest(ITestOutputHelper output, PulsarStandaloneClusterFixture fixture)
		{
			_output = output;
			_system = fixture.System;
			_client = _system.NewClient();
		}
		[Fact]
        public void TestAddAndRemove()
		{
			var builder = new ConsumerConfigBuilder<sbyte[]>();
			builder.Topic("TestAckTracker");
			builder.SubscriptionName("TestAckTracker-sub");
			var consumer = _client.NewConsumer(builder);
			var tracker = _client.ActorSystem.ActorOf(UnAckedMessageTracker.Prop (1000000, 100000, consumer.ConsumerActor));

			var empty = tracker.AskFor<bool>(Empty.Instance);
			Assert.True(empty);

			var size = tracker.AskFor<long>(Size.Instance);
			Assert.Equal(0, size);

			var mid = new MessageId(1L, 1L, -1);
			var added = tracker.AskFor<bool>(new Add(mid));
			Assert.True(added);
			added = tracker.AskFor<bool>(new Add(mid));
			Assert.False(added);
			size = tracker.AskFor<long>(Size.Instance);
			Assert.Equal(1, size);

			tracker.Tell(Clear.Instance);

			added = tracker.AskFor<bool>(new Add(mid));
			Assert.True(added);

			size = tracker.AskFor<long>(Size.Instance);
			Assert.Equal(1, size);

			var removed = tracker.AskFor<bool>(new Remove(mid));

			Assert.True(removed);

			empty = tracker.AskFor<bool>(Empty.Instance);
			Assert.True(empty);

			size = tracker.AskFor<long>(Size.Instance);
			Assert.Equal(0, size);
		}

    }

}