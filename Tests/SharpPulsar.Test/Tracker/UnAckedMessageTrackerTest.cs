using SharpPulsar.Configuration;
using SharpPulsar.Test.Fixtures;
using SharpPulsar.Tracker;
using SharpPulsar.Tracker.Messages;
using SharpPulsar.User;
using Xunit;
using Xunit.Abstractions;
using Akka.Actor;
using System;

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
		private readonly PulsarClient _client;
        private readonly ActorSystem _system;
		public UnAckedMessageTrackerTest(ITestOutputHelper output, PulsarStandaloneClusterFixture fixture)
		{
			_output = output;
			_client = fixture.Client;
            _system = fixture.PulsarSystem.System;
        }

		[Fact]
        public void TestAddAndRemove()
		{
			var builder = new ConsumerConfigBuilder<byte[]>();
			builder.Topic("TestAckTracker");
			builder.SubscriptionName("TestAckTracker-sub");
			var consumer = _client.NewConsumer(builder);
            var unack = _system.ActorOf(UnAckedChunckedMessageIdSequenceMap.Prop());
            var tracker = _client.ActorSystem.ActorOf(UnAckedMessageTracker.Prop (TimeSpan.FromSeconds(1000000), TimeSpan.FromSeconds(1000000), consumer.ConsumerActor, unack));

			var empty = tracker.Ask<bool>(Empty.Instance).GetAwaiter().GetResult();
			Assert.True(empty);

			var size = tracker.Ask<long>(Size.Instance).GetAwaiter().GetResult();
			Assert.Equal(0, size);

			var mid = new MessageId(1L, 1L, -1);
			var added = tracker.Ask<bool>(new Add(mid)).GetAwaiter().GetResult();
			Assert.True(added);
			added = tracker.Ask<bool>(new Add(mid)).GetAwaiter().GetResult();
			Assert.False(added);
			size = tracker.Ask<long>(Size.Instance).GetAwaiter().GetResult();
			Assert.Equal(1, size);

			tracker.Tell(Clear.Instance);

			added = tracker.Ask<bool>(new Add(mid)).GetAwaiter().GetResult();
			Assert.True(added);

			size = tracker.Ask<long>(Size.Instance).GetAwaiter().GetResult();
			Assert.Equal(1, size);

			var removed = tracker.Ask<bool>(new Remove(mid)).GetAwaiter().GetResult();

			Assert.True(removed);

			empty = tracker.Ask<bool>(Empty.Instance).GetAwaiter().GetResult();
			Assert.True(empty);

			size = tracker.Ask<long>(Size.Instance).GetAwaiter().GetResult();
			Assert.Equal(0, size);
		}

    }

}