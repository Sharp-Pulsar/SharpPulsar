using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Text;
using System.Threading;
using Akka.Actor;
using PulsarAdmin.Models;
using Samples;
using SharpPulsar.Akka;
using SharpPulsar.Akka.Admin;
using SharpPulsar.Akka.Configuration;
using SharpPulsar.Akka.EventSource;
using SharpPulsar.Akka.InternalCommands;
using SharpPulsar.Akka.InternalCommands.Producer;
using SharpPulsar.Akka.Network;
using SharpPulsar.Handlers;
using SharpPulsar.Impl.Auth;
using SharpPulsar.Impl.Schema;
using Xunit;
using Xunit.Abstractions;

namespace SharpPulsar.Test
{
    public class ComputeMessageIdTests
    {
        private readonly ITestOutputHelper _output;
        private readonly PulsarSystem _pulsarSystem;
        private string _topic;
        private int _amount;
        private IActorRef _producer;
        public ComputeMessageIdTests(ITestOutputHelper output)
        {
            _topic = Guid.NewGuid().ToString();
            _output = output;
            var clientConfig = new PulsarClientConfigBuilder()
                .ServiceUrl("pulsar://localhost:6650")
                .ConnectionsPerBroker(1)
                .UseProxy(false)
                .OperationTimeout(10000)
                .Authentication(new AuthenticationDisabled())
                //.Authentication(AuthenticationFactory.Token("eyJhbGciOiJSUzI1NiJ9.eyJzdWIiOiJzaGFycHB1bHNhci1jbGllbnQtNWU3NzY5OWM2M2Y5MCJ9.lbwoSdOdBoUn3yPz16j3V7zvkUx-Xbiq0_vlSvklj45Bo7zgpLOXgLDYvY34h4MX8yHB4ynBAZEKG1ySIv76DPjn6MIH2FTP_bpI4lSvJxF5KsuPlFHsj8HWTmk57TeUgZ1IOgQn0muGLK1LhrRzKOkdOU6VBV_Hu0Sas0z9jTZL7Xnj1pTmGAn1hueC-6NgkxaZ-7dKqF4BQrr7zNt63_rPZi0ev47vcTV3ga68NUYLH5PfS8XIqJ_OV7ylouw1qDrE9SVN8a5KRrz8V3AokjThcsJvsMQ8C1MhbEm88QICdNKF5nu7kPYR6SsOfJJ1HYY-QBX3wf6YO3VAF_fPpQ"))
                .ClientConfigurationData;

            _pulsarSystem = new PulsarSystem(clientConfig);
            _amount = 100;
            ProduceMessages();
        }
        [Fact]
        private void To_Should_Be_Set_To_100()
        {
            var to = 0L;
            _pulsarSystem.PulsarAdmin(new Admin(AdminCommands.GetInternalStatsPersistent, new object[] { "public", "default", _topic, false }, e =>
            {
                if (e != null)
                {
                    var data = (PersistentTopicInternalStats)e;
                    var compute = new ComputeMessageId(data, 1, 200, 100);
                    var result = compute.GetFrom();
                   to = result.To.Value;
                    _output.WriteLine(result.To.Value.ToString());
                }
                else
                {
                    Assert.Null(e);
                }
            }, e => _output.WriteLine(e.ToString()), "http://localhost:8080", l => _output.WriteLine(l)));
            while (to == 0)
            {
                Thread.Sleep(100);
            }
            Assert.Equal(100, to);
        }
        [Fact]
        private void Negative_Max()
        {
            var max = 0L;
            _pulsarSystem.PulsarAdmin(new Admin(AdminCommands.GetInternalStatsPersistent, new object[] { "public", "default", _topic, false }, e =>
            {
                if (e != null)
                {
                    var data = (PersistentTopicInternalStats)e;
                    var compute = new ComputeMessageId(data, 1, 100, -2);
                    var result = compute.GetFrom();
                    max = result.Max.Value;
                    _output.WriteLine(result.Entry.Value.ToString());
                }
                else
                {
                    Assert.Null(e);
                }
            }, e => _output.WriteLine(e.ToString()), "http://localhost:8080", l => _output.WriteLine(l)));
            while (max == 0)
            {
                Thread.Sleep(100);
            }
            Assert.Equal(99, max);
        }
        [Fact]
        private void Max_Should_Be_Set_To_The_Difference_Between_To_From_If_Greater()
        {
            var max = 0L;
            _pulsarSystem.PulsarAdmin(new Admin(AdminCommands.GetInternalStatsPersistent, new object[] { "public", "default", _topic, false }, e =>
            {
                if (e != null)
                {
                    var data = (PersistentTopicInternalStats)e;
                    var compute = new ComputeMessageId(data, 1, 100, 150);
                    var result = compute.GetFrom();
                    max = result.Max.Value;
                    _output.WriteLine(result.Entry.Value.ToString());
                }
                else
                {
                    Assert.Null(e);
                }
            }, e => _output.WriteLine(e.ToString()), "http://localhost:8080", l => _output.WriteLine(l)));
            while (max == 0)
            {
                Thread.Sleep(100);
            }
            Assert.Equal(99, max);
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
                sends.Add(new Send(student, _topic, metadata.ToImmutableDictionary(), $"{DateTimeOffset.Now.ToUnixTimeMilliseconds()}"));
            }
            var bulk = new BulkSend(sends, _topic);
            _pulsarSystem.BulkSend(bulk, t);
        }
    }
}
