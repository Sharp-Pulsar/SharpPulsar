using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Akka.Actor;
using SharpPulsar.Akka;
using SharpPulsar.Akka.Configuration;
using SharpPulsar.Akka.Consumer;
using SharpPulsar.Akka.InternalCommands;
using SharpPulsar.Akka.InternalCommands.Consumer;
using SharpPulsar.Akka.InternalCommands.Producer;
using SharpPulsar.Akka.Network;
using SharpPulsar.Api;
using SharpPulsar.Api.Schema;
using SharpPulsar.Handlers;
using SharpPulsar.Impl;
using SharpPulsar.Impl.Schema;
using SharpPulsar.Protocol.Proto;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Samples
{
    class Program
    {
        //I think, the substitution of Linux command $(pwd) in Windows is "%cd%".
        public static readonly Dictionary<string, IActorRef> Producers = new Dictionary<string, IActorRef>();
        public static readonly HashSet<string> Receipts = new HashSet<string>();
        public static readonly HashSet<string> Messages = new HashSet<string>();
        
        public static readonly Dictionary<string, IActorRef> Consumers = new Dictionary<string, IActorRef>();
        public static readonly Dictionary<string, LastMessageIdResponse> LastMessageId = new Dictionary<string, LastMessageIdResponse>();
        static Task Main(string[] args)
        {
            var jsonSchema = JsonSchema.Of(ISchemaDefinition.Builder().WithPojo(typeof(Students)).WithAlwaysAllowNull(false).Build());
            var producerListener = new DefaultProducerListener((o) =>
            {
                Console.WriteLine(o.ToString());
            }, (s, p) => Producers.Add(s, p), s =>
            {
                Receipts.Add(s);
            });
            var consumerListener = new DefaultConsumerEventListener(Console.WriteLine, (s, c) =>
            {
                if(!Consumers.ContainsKey(s))
                    Consumers.Add(s, c);
                //c.Tell(new TimestampSeek(DateTimeOffset.Now.AddDays(-2).ToUnixTimeMilliseconds()));
            }, (s, response) => LastMessageId.Add(s, response));
            //var jsonSchem = JsonSchema.Of(typeof(Students));

            #region messageListener

            var messageListener = new DefaultMessageListener((a, m) =>
            {
                var students = m.ToTypeOf<Students>();
                var s = JsonSerializer.Serialize(students);
                Messages.Add(s);
                Console.WriteLine(s);
                if (m.MessageId is MessageId mi)
                {
                    a.Tell(new AckMessage(new MessageIdReceived(mi.LedgerId, mi.EntryId, -1, mi.PartitionIndex)));
                    Console.WriteLine($"Consumer >> {students.Name}- partition: {mi.PartitionIndex}");
                }
                else if (m.MessageId is BatchMessageId b)
                {
                    a.Tell(new AckMessage(new MessageIdReceived(b.LedgerId, b.EntryId, b.BatchIndex, b.PartitionIndex)));
                    Console.WriteLine($"Consumer >> {students.Name}- partition: {b.PartitionIndex}");
                }
                else
                    Console.WriteLine($"Unknown messageid: {m.MessageId.GetType().Name}");
            }, message =>
            {
                var students = message.ToTypeOf<Students>();
                Console.WriteLine(JsonSerializer.Serialize(students));
            });

            #endregion
            var clientConfig = new PulsarClientConfigBuilder()
                .ServiceUrl("pulsar://pulsar-proxy.eastus2.cloudapp.azure.com:6650")
                .ConnectionsPerBroker(1)
                .ClientConfigurationData;

            var pulsarSystem = new PulsarSystem(clientConfig);

            var producerConfig = new ProducerConfigBuilder()
                .ProducerName("partitioned-topic")
                .Topic("persistent://public/default/partitioned-topic")
                .CryptoKeyReader(new RawFileKeyReader("pulsar_client.pem", "pulsar_client_priv.pem"))
                .Schema(jsonSchema)
                .AddEncryptionKey("Crypto3")
                .SendTimeout(10000)
                .EventListener(producerListener)
                .ProducerConfigurationData;

            var topic = pulsarSystem.CreateProducer(new CreateProducer(jsonSchema, producerConfig));


            var readerConfig = new ReaderConfigBuilder()
                .ReaderName("partitioned-topic")
                .Schema(jsonSchema)
                .EventListener(consumerListener)
                .ReaderListener(messageListener)
                .Topic(topic)
                .CryptoKeyReader(new RawFileKeyReader("pulsar_client.pem", "pulsar_client_priv.pem"))
                .StartMessageId(MessageIdFields.Latest)
                .ReaderConfigurationData;

            var consumerConfig = new ConsumerConfigBuilder()
                .ConsumerName("pattern")
                .ForceTopicCreation(true)
                .SubscriptionName("pattern-Subscription")
                .CryptoKeyReader(new RawFileKeyReader("pulsar_client.pem", "pulsar_client_priv.pem"))
                //.TopicsPattern(new Regex("persistent://public/default/.*"))
                .Topic(topic)
                .ConsumerEventListener(consumerListener)
                .SubscriptionType(CommandSubscribe.SubType.Shared)
                .Schema(jsonSchema)
                .MessageListener(messageListener)
                .SubscriptionInitialPosition(SubscriptionInitialPosition.Latest)
                .ConsumerConfigurationData;

            //pulsarSystem.CreateReader(new CreateReader(jsonSchema, readerConfig));

            IActorRef produce = null;
            while (produce == null)
            {
                Producers.TryGetValue(topic, out produce);
                Thread.Sleep(100);
            }
            Console.WriteLine($"Acquired producer for topic: {topic}");
            pulsarSystem.CreateConsumer(new CreateConsumer(jsonSchema, consumerConfig, ConsumerType.Multi));

            //pulsarSystem.BatchSend(new BatchSend(new List<object>{ new Foo() }, "Test"));

            while (true)
            {
                var read = Console.ReadLine();
                if (read == "s")
                {
                    var sends = new List<Send>();
                    for (var i = 0; i < 26; i++)
                    {
                        var student = new Students
                        {
                            Name = $"Ebere: {DateTimeOffset.Now.ToUnixTimeMilliseconds()} - Decrypted {DateTime.Now.ToString(CultureInfo.InvariantCulture)}",
                            Age = 2019+i,
                            School = "Akka-Pulsar university"
                        }; sends.Add(new Send(student, topic, ImmutableDictionary<string, object>.Empty, $"{DateTime.Now.Millisecond}"));
                    }
                    var bulk = new BulkSend(sends, topic);
                    pulsarSystem.BulkSend(bulk, produce);
                    Task.Delay(5000).Wait();
                    File.AppendAllLines("receipts.txt", Receipts);
                    File.AppendAllLines("messages.txt", Messages);
                }
            }
           
        }
    }

    public class Students
    {
        public string Name { get; set; }
        public int Age { get; set; }
        public string School { get; set; }
    }
    
}
