using System;
using System.Linq;
using System.Text.Json;
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
using Xunit.Abstractions;

namespace SharpPulsar.Test.Tracker
{
    public class TrackerTestFixture : IDisposable
    {
        private readonly PulsarSystem _pulsarSystem;
        internal readonly ITestOutputHelper Output;
        internal readonly TestObject TestObject;
        public TrackerTestFixture(ITestOutputHelper output)
        {
            Output = output;
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
            TestObject = _pulsarSystem.GeTestObject();
        }
        public void Dispose()
        {
            _pulsarSystem.Stop();
        }

        private CreateConsumer CreateConsumer()
        {
            var consumerListener = new DefaultConsumerEventListener(Console.WriteLine);
            var messageListener = new DefaultMessageListener((a, m, st) =>
            {
                var students = m.ToTypeOf<Students>();
                var s = JsonSerializer.Serialize(students);
                Output.WriteLine(s);
                if (m.MessageId is MessageId mi)
                {
                    a.Tell(new AckMessage(new MessageIdReceived(mi.LedgerId, mi.EntryId, -1, mi.PartitionIndex, st.ToArray())));
                    Output.WriteLine($"Consumer >> {students.Name}- partition: {mi.PartitionIndex}");
                }
                else if (m.MessageId is BatchMessageId b)
                {
                    a.Tell(new AckMessage(new MessageIdReceived(b.LedgerId, b.EntryId, b.BatchIndex, b.PartitionIndex, st.ToArray())));
                    Output.WriteLine($"Consumer >> {students.Name}- partition: {b.PartitionIndex}");
                }
                else
                    Output.WriteLine($"Unknown messageid: {m.MessageId.GetType().Name}");
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

        private CreateProducer CreateProducer()
        {
            var jsonSchem = AvroSchema.Of(typeof(Students));
            var producerListener = new DefaultProducerListener((o) =>
            {
                Output.WriteLine(o.ToString());
            }, s =>
            {
                Output.WriteLine(s);
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
    }
}
