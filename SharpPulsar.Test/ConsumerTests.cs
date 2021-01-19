using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json;
using Akka.Actor;
using Samples;
using SharpPulsar.Akka;
using SharpPulsar.Akka.Configuration;
using SharpPulsar.Messages;
using SharpPulsar.Messages.Consumer;
using SharpPulsar.Messages.Producer;
using SharpPulsar.Akka.Network;
using SharpPulsar.Api;
using SharpPulsar.Handlers;
using SharpPulsar.Auth;
using SharpPulsar.Configuration;
using SharpPulsar.Impl.Schema;
using SharpPulsar.Protocol.Proto;
using Xunit;
using Xunit.Abstractions;

namespace SharpPulsar.Test
{
    public class ConsumerTests
    {
        private readonly ITestOutputHelper _output;
        private readonly PulsarSystem _pulsarSystem;
        private string _topic;
        private int _amount;
        private IActorRef _producer;
        public ConsumerTests(ITestOutputHelper output)
        {
            _topic = $"persistent://public/default/{Guid.NewGuid()}";
            _output = output;
            var clientConfig = new PulsarClientConfigBuilder()
                .ServiceUrl("pulsar://localhost:6650")
                .ConnectionsPerBroker(1)
                .UseProxy(false)
                .OperationTimeout(10000)
                .Authentication(new AuthenticationDisabled())
                //.Authentication(AuthenticationFactory.Token("eyJhbGciOiJSUzI1NiJ9.eyJzdWIiOiJzaGFycHB1bHNhci1jbGllbnQtNWU3NzY5OWM2M2Y5MCJ9.lbwoSdOdBoUn3yPz16j3V7zvkUx-Xbiq0_vlSvklj45Bo7zgpLOXgLDYvY34h4MX8yHB4ynBAZEKG1ySIv76DPjn6MIH2FTP_bpI4lSvJxF5KsuPlFHsj8HWTmk57TeUgZ1IOgQn0muGLK1LhrRzKOkdOU6VBV_Hu0Sas0z9jTZL7Xnj1pTmGAn1hueC-6NgkxaZ-7dKqF4BQrr7zNt63_rPZi0ev47vcTV3ga68NUYLH5PfS8XIqJ_OV7ylouw1qDrE9SVN8a5KRrz8V3AokjThcsJvsMQ8C1MhbEm88QICdNKF5nu7kPYR6SsOfJJ1HYY-QBX3wf6YO3VAF_fPpQ"))
                .ClientConfigurationData;

            _pulsarSystem = PulsarSystem.GetInstance(clientConfig);
            _amount = 100;
            ProduceMessages();
        }
        [Fact]
        private  void Consume_Queue_With_Take()
        {
            var replayed = 0;
            var consumerListener = new DefaultConsumerEventListener(Console.WriteLine);
            var messageListener = new DefaultMessageListener(null, null);
            var jsonSchem = new AutoConsumeSchema();//AvroSchema.Of(typeof(JournalEntry));
            var topicLast = _topic.Split("/").Last();
            var consumerConfig = new ConsumerConfigBuilder()
                .ConsumerName(topicLast)
                .ForceTopicCreation(true)
                .SubscriptionName($"{topicLast}-Subscription")
                .Topic(_topic)
                .SetConsumptionType(ConsumptionType.Queue)
                .ConsumerEventListener(consumerListener)
                .SubscriptionType(CommandSubscribe.SubType.Exclusive)
                .Schema(jsonSchem)
                .MessageListener(messageListener)
                .SubscriptionInitialPosition(SubscriptionInitialPosition.Earliest)
                .ConsumerConfigurationData;
            _pulsarSystem.PulsarConsumer(new CreateConsumer(jsonSchem, consumerConfig));
            foreach (var msg in _pulsarSystem.Messages<Students>(topicLast,true, 70))
            {
                replayed++;
                _output.WriteLine(JsonSerializer.Serialize(msg, new JsonSerializerOptions { WriteIndented = true }));
            }
            Assert.Equal(70, replayed);
        }
        
        [Fact]
        private  void Consume_Queue_With_Take_100_Custom_Handler()
        {
            var replayed = 0;
            var consumerListener = new DefaultConsumerEventListener(Console.WriteLine);
            var messageListener = new DefaultMessageListener(null, null);
            var jsonSchem = new AutoConsumeSchema();//AvroSchema.Of(typeof(JournalEntry));
            var topicLast = _topic.Split("/").Last();
            var consumerConfig = new ConsumerConfigBuilder()
                .ConsumerName(topicLast)
                .ForceTopicCreation(true)
                .SubscriptionName($"{topicLast}-Subscription")
                .Topic(_topic)
                .SetConsumptionType(ConsumptionType.Queue)
                .ConsumerEventListener(consumerListener)
                .SubscriptionType(CommandSubscribe.SubType.Exclusive)
                .Schema(jsonSchem)
                .MessageListener(messageListener)
                .SubscriptionInitialPosition(SubscriptionInitialPosition.Earliest)
                .ConsumerConfigurationData;
            _pulsarSystem.PulsarConsumer(new CreateConsumer(jsonSchem, consumerConfig));
            foreach (var msg in _pulsarSystem.Messages(topicLast,true, 70, customHander: message =>
            {
                var m = message.Message.ToTypeOf<Students>();
                _output.WriteLine($"Sequence Id: {message.Message.SequenceId}");
                return m;
            } ))
            {
                replayed++;
                _output.WriteLine(JsonSerializer.Serialize(msg, new JsonSerializerOptions { WriteIndented = true }));
            }
            Assert.Equal(70, replayed);
        }

        private void ProduceMessages()
        {
            var jsonSchem = AvroSchema.Of(typeof(Students));
            var producerListener = new DefaultProducerListener((o) =>
            {
                Console.WriteLine(o.ToString());
            }, s =>
            {

            });
            var producerConfig = new ProducerConfigBuilder()
                .ProducerName(_topic)
                .Topic(_topic)
                .Schema(jsonSchem)
                .EventListener(producerListener)
                .ProducerConfigurationData;

            var t = _producer ?? _pulsarSystem.PulsarProducer(new CreateProducer(jsonSchem, producerConfig)).Producer;

            var sends = new List<Send>();
            for (var i = 0; i < _amount; i++)
            {
                var student = new Students
                {
                    Name = $"#LockDown Ebere: {DateTimeOffset.Now.ToUnixTimeMilliseconds()} - test {DateTime.Now.ToString(CultureInfo.InvariantCulture)}",
                    Age = 2019 + i,
                    School = "Akka-Pulsar university"
                };
                var metadata = new Dictionary<string, object>
                {
                    ["Key"] = "Bulk",
                    ["Properties"] = new Dictionary<string, string>
                    {
                        { "Tick", DateTime.Now.Ticks.ToString() },
                        {"Week-Day", "Saturday" }
                    }
                };
                sends.Add(new Send(student, metadata.ToImmutableDictionary(), $"{DateTimeOffset.Now.ToUnixTimeMilliseconds()}"));
            }
            var bulk = new BulkSend(sends, _topic);
            _pulsarSystem.BulkSend(bulk, t);
        }
    }
}
