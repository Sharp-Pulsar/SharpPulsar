using System;
using System.Collections.Generic;
using System.Text.Json;
using Akka.Actor;
using Samples;
using SharpPulsar.Akka;
using SharpPulsar.Akka.Configuration;
using SharpPulsar.Messages.Consumer;
using SharpPulsar.Messages.Producer;
using SharpPulsar.Akka.Network;
using SharpPulsar.Api;
using SharpPulsar.Handlers;
using SharpPulsar.Auth;
using SharpPulsar.Impl.Schema;
using SharpPulsar.Protocol.Proto;
using Xunit.Abstractions;

namespace SharpPulsar.Test.Tracker
{
    public class TrackerTestFixture : IDisposable
    {
        internal readonly PulsarSystem PulsarSystem;
        private ITestOutputHelper _output;


		public TrackerTestFixture()
        {
            var clientConfig = new PulsarClientConfigBuilder()
                .ServiceUrl("pulsar://localhost:6650")
                .ConnectionsPerBroker(1)
                .UseProxy(false)
                .OperationTimeout(30000)
                .Authentication(new AuthenticationDisabled())
                //.Authentication(AuthenticationFactory.Token("eyJhbGciOiJSUzI1NiJ9.eyJzdWIiOiJzaGFycHB1bHNhci1jbGllbnQtNWU3NzY5OWM2M2Y5MCJ9.lbwoSdOdBoUn3yPz16j3V7zvkUx-Xbiq0_vlSvklj45Bo7zgpLOXgLDYvY34h4MX8yHB4ynBAZEKG1ySIv76DPjn6MIH2FTP_bpI4lSvJxF5KsuPlFHsj8HWTmk57TeUgZ1IOgQn0muGLK1LhrRzKOkdOU6VBV_Hu0Sas0z9jTZL7Xnj1pTmGAn1hueC-6NgkxaZ-7dKqF4BQrr7zNt63_rPZi0ev47vcTV3ga68NUYLH5PfS8XIqJ_OV7ylouw1qDrE9SVN8a5KRrz8V3AokjThcsJvsMQ8C1MhbEm88QICdNKF5nu7kPYR6SsOfJJ1HYY-QBX3wf6YO3VAF_fPpQ"))
                .ClientConfigurationData;
            PulsarSystem = PulsarSystem.GetInstance(clientConfig);
        }
        public void Dispose()
        {
            PulsarSystem.Stop();
        }
		public CreateConsumer CreateConsumer(ITestOutputHelper output)
        {
            _output = output;
			var consumerListener = new DefaultConsumerEventListener(Console.WriteLine);
			var messageListener = new DefaultMessageListener(Handler, null);
			var jsonSchem = new AutoConsumeSchema();//AvroSchema.Of(typeof(JournalEntry));
			var consumerConfig = new ConsumerConfigBuilder()
				.ConsumerName($"student-test-consumer-{Guid.NewGuid()}")
				.ForceTopicCreation(true)
				.SubscriptionName("student-test-Subscription")
				.Topic("student-test")

				.ConsumerEventListener(consumerListener)
				.SubscriptionType(CommandSubscribe.SubType.Exclusive)
				.Schema(jsonSchem)
				.MessageListener(messageListener)
				.SubscriptionInitialPosition(SubscriptionInitialPosition.Earliest)
				.ConsumerConfigurationData;
			return new CreateConsumer(jsonSchem, consumerConfig);
		}

		public CreateProducer CreateProducer(ITestOutputHelper output)
		{
			var jsonSchem = AvroSchema.Of(typeof(Students));
			var producerListener = new DefaultProducerListener((o) =>
			{
				output.WriteLine(o.ToString());
			}, s =>
			{
				output.WriteLine(s);
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

        private void Handler(IActorRef a, IMessage m)
        {
            var students = m.ToTypeOf<Students>();
            var s = JsonSerializer.Serialize(students);
            _output.WriteLine(s); a.Tell(new AckMessage(m.MessageId));
            _output.WriteLine($"Consumer >> {students.Name}");
		}
	}
}
