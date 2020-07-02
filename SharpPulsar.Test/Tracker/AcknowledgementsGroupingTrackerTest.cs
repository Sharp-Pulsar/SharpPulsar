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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Akka.Actor;
using Akka.TestKit.Xunit2;
using Samples;
using SharpPulsar.Akka;
using SharpPulsar.Akka.Configuration;
using SharpPulsar.Akka.Consumer;
using SharpPulsar.Akka.InternalCommands.Consumer;
using SharpPulsar.Akka.InternalCommands.Producer;
using SharpPulsar.Akka.Network;
using SharpPulsar.Api;
using SharpPulsar.Batch;
using SharpPulsar.Handlers;
using SharpPulsar.Impl.Auth;
using SharpPulsar.Impl.Conf;
using SharpPulsar.Impl.Schema;
using SharpPulsar.Protocol.Proto;
using SharpPulsar.Tracker;
using SharpPulsar.Utils;
using Xunit;
using Xunit.Abstractions;
using MessageId = SharpPulsar.Impl.MessageId;

namespace SharpPulsar.Test.Tracker
{
    [Collection("AcknowledgementsGroupingTrackerTest")]
	public class AcknowledgementsGroupingTrackerTest
    {
        private readonly PulsarSystem _pulsarSystem;
        private readonly ITestOutputHelper _output;
        private readonly TestObject _testObject;
		public AcknowledgementsGroupingTrackerTest(ITestOutputHelper output)
        {
            _output = output;
            var clientConfig = new PulsarClientConfigBuilder()
                .ServiceUrl("pulsar://localhost:6650")
                .ConnectionsPerBroker(1)
                .UseProxy(false)
                .OperationTimeout(60000)
                .Authentication(new AuthenticationDisabled())
                //.Authentication(AuthenticationFactory.Token("eyJhbGciOiJSUzI1NiJ9.eyJzdWIiOiJzaGFycHB1bHNhci1jbGllbnQtNWU3NzY5OWM2M2Y5MCJ9.lbwoSdOdBoUn3yPz16j3V7zvkUx-Xbiq0_vlSvklj45Bo7zgpLOXgLDYvY34h4MX8yHB4ynBAZEKG1ySIv76DPjn6MIH2FTP_bpI4lSvJxF5KsuPlFHsj8HWTmk57TeUgZ1IOgQn0muGLK1LhrRzKOkdOU6VBV_Hu0Sas0z9jTZL7Xnj1pTmGAn1hueC-6NgkxaZ-7dKqF4BQrr7zNt63_rPZi0ev47vcTV3ga68NUYLH5PfS8XIqJ_OV7ylouw1qDrE9SVN8a5KRrz8V3AokjThcsJvsMQ8C1MhbEm88QICdNKF5nu7kPYR6SsOfJJ1HYY-QBX3wf6YO3VAF_fPpQ"))
                .ClientConfigurationData;
			_pulsarSystem = PulsarSystem.GetInstance(clientConfig, SystemMode.Test);
            _pulsarSystem.PulsarProducer(CreateProducer());
            _pulsarSystem.PulsarConsumer(CreateConsumer());
            _testObject = _pulsarSystem.GeTestObject();
        }
		[Fact]
		public void TestAckTracker()
        {
            var conf = new ConsumerConfigurationData
            {
                AcknowledgementsGroupTimeMicros = (long)ConvertTimeUnits.ConvertMillisecondsToMicroseconds(10000)
            };
            var tracker = new PersistentAcknowledgmentsGroupingTracker(_testObject.ActorSystem, _testObject.ConsumerBroker, _testObject.Consumer, 1, conf);

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
            var tracker = new PersistentAcknowledgmentsGroupingTracker(_testObject.ActorSystem, _testObject.ConsumerBroker, _testObject.Consumer, 1, conf);
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
            var tracker = new PersistentAcknowledgmentsGroupingTracker(_testObject.ActorSystem, _testObject.ConsumerBroker, _testObject.Consumer, 1, conf);
			
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

        private CreateProducer CreateProducer()
        {
            var jsonSchem = AvroSchema.Of(typeof(Students));
            var producerListener = new DefaultProducerListener((o) =>
            {
                _output.WriteLine(o.ToString());
            }, s =>
            {
                _output.WriteLine(s);
            });
            //var compression = (ICompressionType)Enum.GetValues(typeof(ICompressionType)).GetValue(comp);
            var producerConfig = new ProducerConfigBuilder()
                .ProducerName("student-tester")
                .Topic("student-test")
                .Schema(jsonSchem)
                //.CompressionType(compression)
                .EventListener(producerListener)
                .ProducerConfigurationData;

            return new CreateProducer(jsonSchem, producerConfig);
        }

        private CreateConsumer CreateConsumer()
        {
            var consumerListener = new DefaultConsumerEventListener(Console.WriteLine);
            var messageListener = new DefaultMessageListener((a, m, st) =>
            {
                var students = m.ToTypeOf<Students>();
                var s = JsonSerializer.Serialize(students);
                _output.WriteLine(s);
                if (m.MessageId is MessageId mi)
                {
                    a.Tell(new AckMessage(new MessageIdReceived(mi.LedgerId, mi.EntryId, -1, mi.PartitionIndex, st.ToArray())));
                    _output.WriteLine($"Consumer >> {students.Name}- partition: {mi.PartitionIndex}");
                }
                else if (m.MessageId is BatchMessageId b)
                {
                    a.Tell(new AckMessage(new MessageIdReceived(b.LedgerId, b.EntryId, b.BatchIndex, b.PartitionIndex, st.ToArray())));
                    _output.WriteLine($"Consumer >> {students.Name}- partition: {b.PartitionIndex}");
                }
                else
                    _output.WriteLine($"Unknown messageid: {m.MessageId.GetType().Name}");
            }, null);
            var jsonSchem = new AutoConsumeSchema();//AvroSchema.Of(typeof(JournalEntry));
            var consumerConfig = new ConsumerConfigBuilder()
                .ConsumerName("student-test-consumer")
                .ForceTopicCreation(true)
                .SubscriptionName("student-test-Subscription")
                .Topic("student-test")

                .ConsumerEventListener(consumerListener)
                .SubscriptionType(CommandSubscribe.SubType.Exclusive)
                .Schema(jsonSchem)
                .MessageListener(messageListener)
                .SubscriptionInitialPosition(SubscriptionInitialPosition.Earliest)
                .ConsumerConfigurationData;
            return new CreateConsumer(jsonSchem, consumerConfig, ConsumerType.Single);
        }
	}

}