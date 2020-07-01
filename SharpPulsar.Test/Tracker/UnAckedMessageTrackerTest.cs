using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using Akka.Actor;
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
using SharpPulsar.Impl;
using SharpPulsar.Impl.Auth;
using SharpPulsar.Impl.Schema;
using SharpPulsar.Protocol.Proto;
using SharpPulsar.Tracker;
using Xunit;
using Xunit.Abstractions;

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
    public class UnAckedMessageTrackerTest
	{
        private readonly PulsarSystem _pulsarSystem;
        private readonly ITestOutputHelper _output;
        private readonly TestObject _testObject;
		public UnAckedMessageTrackerTest(ITestOutputHelper output)
        {
            _output = output;
            var clientConfig = new PulsarClientConfigBuilder()
                .ServiceUrl("pulsar://localhost:6650")
                .ConnectionsPerBroker(1)
                .UseProxy(false)
                .OperationTimeout(30000)
                .Authentication(new AuthenticationDisabled())
                //.Authentication(AuthenticationFactory.Token("eyJhbGciOiJSUzI1NiJ9.eyJzdWIiOiJzaGFycHB1bHNhci1jbGllbnQtNWU3NzY5OWM2M2Y5MCJ9.lbwoSdOdBoUn3yPz16j3V7zvkUx-Xbiq0_vlSvklj45Bo7zgpLOXgLDYvY34h4MX8yHB4ynBAZEKG1ySIv76DPjn6MIH2FTP_bpI4lSvJxF5KsuPlFHsj8HWTmk57TeUgZ1IOgQn0muGLK1LhrRzKOkdOU6VBV_Hu0Sas0z9jTZL7Xnj1pTmGAn1hueC-6NgkxaZ-7dKqF4BQrr7zNt63_rPZi0ev47vcTV3ga68NUYLH5PfS8XIqJ_OV7ylouw1qDrE9SVN8a5KRrz8V3AokjThcsJvsMQ8C1MhbEm88QICdNKF5nu7kPYR6SsOfJJ1HYY-QBX3wf6YO3VAF_fPpQ"))
                .ClientConfigurationData;
            _pulsarSystem = PulsarSystem.GetInstance(clientConfig, SystemMode.Test);
            _pulsarSystem.PulsarProducer(CreateProducer());
            _pulsarSystem.PulsarConsumer(CreateConsumer());
            _testObject = _pulsarSystem.GeTestObject();
		}
        [Fact]
        public void TestAddAndRemove()
		{
			var tracker = new UnAckedMessageTracker(_testObject.Consumer, 1000000, 100000, _testObject.ActorSystem);
			tracker.Stop();

			Assert.True(tracker.Empty);
			Assert.Equal(0, tracker.Size());

			var mid = new MessageId(1L, 1L, -1);
			Assert.True(tracker.Add(mid));
			Assert.False(tracker.Add(mid));
			Assert.Equal(1, tracker.Size());

			var headPartition = tracker.TimePartitions.Dequeue();
			headPartition.Clear();
			tracker.TimePartitions.Enqueue(headPartition);

			Assert.False(tracker.Add(mid));
			Assert.Equal(1, tracker.Size());

			Assert.True(tracker.Remove(mid));
			Assert.True(tracker.Empty);
			Assert.Equal(0, tracker.Size());
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