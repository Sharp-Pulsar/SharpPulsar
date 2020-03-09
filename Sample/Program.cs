using System;
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
using SharpPulsar.Api;
using SharpPulsar.Api.Schema;
using SharpPulsar.Impl.Schema;

namespace Samples
{
    class Program
    {
        //I think, the substitution of Linux command $(pwd) in Windows is "%cd%".
        static Task Main(string[] args)
        {
            var jsonSchema = JsonSchema.Of(ISchemaDefinition.Builder().WithPojo(typeof(Students)).WithAlwaysAllowNull(false).Build());
            var producer = new ProducerListener();
            //var jsonSchem = JsonSchema.Of(typeof(Students));
            var clientConfig = new PulsarClientConfigBuilder()
                .ServiceUrl("pulsar://localhost:6650")
                .ConnectionsPerBroker(1)
                .ClientConfigurationData;
            var pulsarSystem = new PulsarSystem(clientConfig);
            var producerConfig = new ProducerConfigBuilder()
                .ProducerName("Staff")
                .Topic("staff")
                .Schema(jsonSchema)
                .EventListener(producer)
                .AddEncryptionKey("sessions")
                .EnableBatching(false)
                .BatchingMaxMessages(3)
                .ProducerConfigurationData;
            //Thread.Sleep(5000);
            //Console.WriteLine("Creating Producer");
            var topic = pulsarSystem.CreateProducer(new CreateProducer(jsonSchema, producerConfig));


            var readerConfig = new ReaderConfigBuilder()
                .ReaderName("Staff")
                .Schema(jsonSchema)
                .ReaderListener(new ReaderMessageListener())
                .Topic(topic)
                .StartMessageId(MessageIdFields.Latest)
                .EventListener(new ConsumerEventListener())
                .ReaderConfigurationData;
            var consumerConfig = new ConsumerConfigBuilder()
                .ConsumerName("Staff")
                .ForceTopicCreation(false)
                .SubscriptionName("Staff-Subscription")
                .Topic(topic)
                .Schema(jsonSchema)
                .MessageListener(new ConsumerMessageListener())
                .ConsumerEventListener(new ConsumerEventListener())
                .SubscriptionInitialPosition(SubscriptionInitialPosition.Latest)
                .ConsumerConfigurationData;
          
            pulsarSystem.CreateConsumer(new CreateConsumer(jsonSchema, consumerConfig, ConsumerType.Single));
            //Thread.Sleep(5000);
            //Console.WriteLine("Creating Reader");
            pulsarSystem.CreateReader(new CreateReader(jsonSchema, readerConfig));

            //Thread.Sleep(5000);
            //Console.WriteLine("Sending Producer");
            IActorRef produce = null;
            while (produce == null)
            {
                produce = producer.GetProducer(topic);
                Thread.Sleep(100);
            }
            Console.WriteLine($"Acquired producer for topic: {topic}");
            //pulsarSystem.BatchSend(new BatchSend(new List<object>{ new Foo() }, "Test"));
            
            while (true)
            {
                var read = Console.ReadLine();
                if (read == "s")
                {
                    var students = new Students
                    {
                        Name = $"Ebere: {DateTimeOffset.Now.Millisecond}",
                        Age = 2020,
                        School = "Akka-Pulsar university"
                    };
                    pulsarSystem.Send(new Send(students, topic, ImmutableDictionary<string, object>.Empty), produce);
                    /*for (var i = 0; i < 150; i++)
                    {
                        var students = new Students
                        {
                            Name = $"Ebere {i}",
                            Age = 2020 + i,
                            School = "Akka-Pulsar university"
                        };
                        pulsarSystem.Send(new Send(students, topic, ImmutableDictionary<string, object>.Empty), produce);
                    }*/
                }
                //Console.Write(".");
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
