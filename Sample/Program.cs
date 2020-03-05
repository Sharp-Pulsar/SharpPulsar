using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.IO;
using BAMCIS.Util.Concurrent;
using Samples.Consumer;
using Samples.Producer;
using Samples.Reader;
using SharpPulsar.Akka;
using SharpPulsar.Akka.Configuration;
using SharpPulsar.Akka.InternalCommands;
using SharpPulsar.Akka.InternalCommands.Consumer;
using SharpPulsar.Akka.InternalCommands.Producer;
using SharpPulsar.Akka.Network;
using SharpPulsar.Api.Schema;
using SharpPulsar.Impl.Conf;
using SharpPulsar.Impl.Schema;

namespace Producer
{
    class Program
    {
        static Task Main(string[] args)
        {
            var jsonSchema = JsonSchema.Of(ISchemaDefinition.Builder().WithPojo(typeof(Students)).WithAlwaysAllowNull(false).Build());
            var producer = new ProducerListener();
            //var jsonSchem = JsonSchema.Of(typeof(Students));
            var clientConfig = new PulsarClientConfigBuilder()
                .ServiceUrl("pulsar://localhost:6650")
                .ConnectionsPerBroker(1)
                .KeepAliveInterval(30, TimeUnit.SECONDS)
                .IoThreads(1)
                .ClientConfigurationData;
            var pulsarSystem = new PulsarSystem(clientConfig);
            var producerConfig = new ProducerConfigBuilder()
                .ProducerName("Students")
                .Topic("students")
                .Schema(jsonSchema)
                .EventListener(producer)
                .AddEncryptionKey("sessions")
                .EnableBatching(false)
                .ProducerConfigurationData;

            var consumerConfig = new ConsumerConfigBuilder()
                .ConsumerName("Student")
                .Topic("students")
                .Schema(jsonSchema)
                .MessageListener(new ConsumerMessageListener())
                .ConsumerEventListener(new ConsumerEventListener())
                .ConsumerConfigurationData;

            var readerConfig = new ReaderConfigBuilder()
                .ReaderName("Students")
                .Schema(jsonSchema)
                .ReaderListener(new ReaderMessageListener())
                .Topic("students")
                .ReaderConfigurationData;
            //Thread.Sleep(5000);
            //Console.WriteLine("Creating Producer");
            var topic = pulsarSystem.CreateProducer(new CreateProducer(jsonSchema, producerConfig));
            //Thread.Sleep(5000);
            //Console.WriteLine("Creating Consumer");
            //pulsarSystem.CreateConsumer(new CreateConsumer(jsonSchema, consumerConfig, ConsumerType.Single));
            //Thread.Sleep(5000);
            //Console.WriteLine("Creating Reader");
            //pulsarSystem.CreateReader(new CreateReader(jsonSchema, readerConfig));
            var students = new Students
            {
                Name = "Ebere",
                Age = 2020,
                School = "Akka-Pulsar university"
            };
            //Thread.Sleep(5000);
            //Console.WriteLine("Sending Producer");
            IActorRef produce = null;
            while (produce == null)
            {
                produce = producer.GetProducer(topic);
                Thread.Sleep(100);
            }
            Console.WriteLine($"Acquired producer for topic: {topic}");
            pulsarSystem.Send(new Send(students,topic, ImmutableDictionary<string, object>.Empty), produce);
            //pulsarSystem.BatchSend(new BatchSend(new List<object>{ new Foo() }, "Test"));
            
            while (true)
            {
                Console.Write(".");
            }
            //system.Tcp().Tell(new Tcp.Connect(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 6650)));
        }
    }

    public class Students
    {
        public string Name { get; set; }
        public int Age { get; set; }
        public string School { get; set; }
    }
    public class Act:UntypedActor
    {
        public Act()
        {
            Context.System.Tcp().Tell(new Tcp.Connect(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 6650)));
        }
        protected override void OnReceive(object message)
        {
            if (message is Tcp.Connected)
            {
                var connected = message as Tcp.Connected;
                Console.WriteLine("Connected to {0}", connected.RemoteAddress);

                // Register self as connection handler
                Sender.Tell(new Tcp.Register(Self));
                ReadConsoleAsync();
                Become(Connected(Sender));
            }
            else if (message is Tcp.CommandFailed)
            {
                Console.WriteLine("Connection failed");
            }
            else Unhandled(message);
        }

        public static Props Prop()
        {
            return Props.Create(() => new Act());
        }
        private void ReadConsoleAsync()
        {
            Task.Factory.StartNew(self => Console.In.ReadLineAsync().PipeTo((ICanTell)self), Self);
        }
        private UntypedReceive Connected(IActorRef connection)
        {
            return message =>
            {
                if (message is Tcp.Received received)  // data received from network
                {
                    Console.WriteLine(Encoding.ASCII.GetString(received.Data.ToArray()));
                }
                else if (message is string)   // data received from console
                {
                    connection.Tell(Tcp.Write.Create(ByteString.FromString((string)message + "\n")));
                    ReadConsoleAsync();
                }
                else if (message is Tcp.PeerClosed)
                {
                    Console.WriteLine("Connection closed");
                }
                else Unhandled(message);
            };
        }
    }
    
}
