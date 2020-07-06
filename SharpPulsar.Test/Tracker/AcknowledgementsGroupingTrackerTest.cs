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
using SharpPulsar.Impl.Conf;
using SharpPulsar.Protocol.Proto;
using SharpPulsar.Tracker;
using SharpPulsar.Utils;
using Xunit;
using Xunit.Abstractions;
using MessageId = SharpPulsar.Impl.MessageId;

namespace SharpPulsar.Test.Tracker
{
    [Collection("AcknowledgementsGroupingTrackerTest")]
	public class AcknowledgementsGroupingTrackerTest : IClassFixture<TrackerTestFixture>
	{
        private readonly ITestOutputHelper _output;
        private TrackerTestFixture _data;
		public AcknowledgementsGroupingTrackerTest(ITestOutputHelper output, TrackerTestFixture data)
        {
            _output = output;
            _data = data;

            _data.PulsarSystem.PulsarProducer(data.CreateProducer(output));
            _data.PulsarSystem.PulsarConsumer(data.CreateConsumer(output));
            _data.TestObject = _data.PulsarSystem.GeTestObject();
		}
		
		[Fact]
		public void TestAckTracker()
		{
			var conf = new ConsumerConfigurationData
            {
                AcknowledgementsGroupTimeMicros = (long)ConvertTimeUnits.ConvertMillisecondsToMicroseconds(10000)
            };
            var tracker = new PersistentAcknowledgmentsGroupingTracker(_data.TestObject.ActorSystem, _data.TestObject.ConsumerBroker, _data.TestObject.Consumer, 1, conf);

			var msg1 = new MessageId(5, 1, 0);
			var msg2 = new MessageId(5, 2, 0);
			var msg3 = new MessageId(5, 3, 0);
			var msg4 = new MessageId(5, 4, 0);
			var msg5 = new MessageId(5, 5, 0);
			var msg6 = new MessageId(5, 6, 0);
			Assert.False(tracker.IsDuplicate(msg1));
			tracker.AddAcknowledgment(msg1, CommandAck.AckType.Individual, new Dictionary<string, long>());
			Assert.True(tracker.IsDuplicate(msg1));

			Assert.False(tracker.IsDuplicate(msg2));

			tracker.AddAcknowledgment(msg5, CommandAck.AckType.Cumulative, new Dictionary<string, long>());
			Assert.True(tracker.IsDuplicate(msg1));
			Assert.True(tracker.IsDuplicate(msg2));
			Assert.True(tracker.IsDuplicate(msg3));

			Assert.True(tracker.IsDuplicate(msg4));
			Assert.True(tracker.IsDuplicate(msg5));
			Assert.False(tracker.IsDuplicate(msg6));

			// Flush while disconnected. the internal tracking will not change
			tracker.Flush();

			Assert.True(tracker.IsDuplicate(msg1));
			Assert.True(tracker.IsDuplicate(msg2));
			Assert.True(tracker.IsDuplicate(msg3));

			Assert.True(tracker.IsDuplicate(msg4));
			Assert.True(tracker.IsDuplicate(msg5));
			Assert.False(tracker.IsDuplicate(msg6));

			tracker.AddAcknowledgment(msg6, CommandAck.AckType.Individual, new Dictionary<string, long>());
			Assert.True(tracker.IsDuplicate(msg6));
			
			tracker.Flush();

			Assert.True(tracker.IsDuplicate(msg1));
			Assert.True(tracker.IsDuplicate(msg2));
			Assert.True(tracker.IsDuplicate(msg3));

			Assert.True(tracker.IsDuplicate(msg4));
			Assert.True(tracker.IsDuplicate(msg5));
			Assert.False(tracker.IsDuplicate(msg6));

			tracker.Close();
		}

		[Fact]
		public  void TestImmediateAckingTracker()
		{
			var conf = new ConsumerConfigurationData {AcknowledgementsGroupTimeMicros = 0};
            var tracker = new PersistentAcknowledgmentsGroupingTracker(_data.TestObject.ActorSystem, _data.TestObject.ConsumerBroker, _data.TestObject.Consumer, 1, conf);
			var msg1 = new MessageId(5, 1, 0);
			var msg2 = new MessageId(5, 2, 0);

			Assert.False(tracker.IsDuplicate(msg1));


			tracker.AddAcknowledgment(msg1, CommandAck.AckType.Individual, new Dictionary<string, long>());
			Assert.False(tracker.IsDuplicate(msg1));
			
			tracker.Flush();
			Assert.False(tracker.IsDuplicate(msg1));

			tracker.AddAcknowledgment(msg2, CommandAck.AckType.Individual, new Dictionary<string, long>());
			// Since we were connected, the ack went out immediately
			Assert.False(tracker.IsDuplicate(msg2));
			tracker.Close();
		}

		[Fact]
		public void TestAckTrackerMultiAck()
		{
			var conf = new ConsumerConfigurationData
            {
                AcknowledgementsGroupTimeMicros = (long) ConvertTimeUnits.ConvertMillisecondsToMicroseconds(10000)
            };
            var tracker = new PersistentAcknowledgmentsGroupingTracker(_data.TestObject.ActorSystem, _data.TestObject.ConsumerBroker, _data.TestObject.Consumer, 1, conf);
			
			var msg1 = new MessageId(5, 1, 0);
			var msg2 = new MessageId(5, 2, 0);
			var msg3 = new MessageId(5, 3, 0);
			var msg4 = new MessageId(5, 4, 0);
			var msg5 = new MessageId(5, 5, 0);
			var msg6 = new MessageId(5, 6, 0);

			Assert.False(tracker.IsDuplicate(msg1));

			tracker.AddAcknowledgment(msg1, CommandAck.AckType.Individual, new Dictionary<string, long>());
			Assert.True(tracker.IsDuplicate(msg1));

			Assert.False(tracker.IsDuplicate(msg2));

			tracker.AddAcknowledgment(msg5, CommandAck.AckType.Cumulative, new Dictionary<string, long>());
			Assert.True(tracker.IsDuplicate(msg1));
			Assert.True(tracker.IsDuplicate(msg2));
			Assert.True(tracker.IsDuplicate(msg3));

			Assert.True(tracker.IsDuplicate(msg4));
			Assert.True(tracker.IsDuplicate(msg5));
			Assert.False(tracker.IsDuplicate(msg6));

			// Flush while disconnected. the internal tracking will not change
			tracker.Flush();

			Assert.True(tracker.IsDuplicate(msg1));
			Assert.True(tracker.IsDuplicate(msg2));
			Assert.True(tracker.IsDuplicate(msg3));

			Assert.True(tracker.IsDuplicate(msg4));
			Assert.True(tracker.IsDuplicate(msg5));
			Assert.False(tracker.IsDuplicate(msg6));

			tracker.AddAcknowledgment(msg6, CommandAck.AckType.Individual, new Dictionary<string, long>());
			Assert.True(tracker.IsDuplicate(msg6));


			tracker.Flush();

			Assert.True(tracker.IsDuplicate(msg1));
			Assert.True(tracker.IsDuplicate(msg2));
			Assert.True(tracker.IsDuplicate(msg3));

			Assert.True(tracker.IsDuplicate(msg4));
			Assert.True(tracker.IsDuplicate(msg5));
			Assert.False(tracker.IsDuplicate(msg6));

			tracker.Close();
		}

	}

}