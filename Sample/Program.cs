using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
using SharpPulsar.Handlers;
using SharpPulsar.Impl;
using SharpPulsar.Impl.Auth;
using SharpPulsar.Impl.Schema;
using SharpPulsar.Protocol.Proto;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Samples
{
    public class Program
    {
        //I think, the substitution of Linux command $(pwd) in Windows is "%cd%".
        public static readonly ConcurrentDictionary<string, IActorRef> Producers = new ConcurrentDictionary<string, IActorRef>();
        public static readonly ConcurrentBag<string> Receipts = new ConcurrentBag<string>();
        public static readonly ConcurrentBag<string> Messages = new ConcurrentBag<string>();
        
        public static readonly Dictionary<string, IActorRef> Consumers = new Dictionary<string, IActorRef>();
        public static readonly Dictionary<string, LastMessageIdResponse> LastMessageId = new Dictionary<string, LastMessageIdResponse>();
        static void Main(string[] args)
        {
            var clientConfig = new PulsarClientConfigBuilder()
                //.ServiceUrl("pulsar://pulsar-proxy.eastus2.cloudapp.azure.com:6650")
                .ServiceUrl("pulsar://40.70.228.154:6650")//testing purposes only
                .ServiceUrlProvider(new ServiceUrlProviderImpl("pulsar://40.70.228.154:6650"))//testing purposes only
                .ConnectionsPerBroker(1)
                .UseProxy(true)
                .Authentication( new AuthenticationDisabled())
                //.Authentication(AuthenticationFactory.Token("eyJhbGciOiJSUzI1NiJ9.eyJzdWIiOiJzaGFycHB1bHNhci1jbGllbnQtNWU3NzY5OWM2M2Y5MCJ9.lbwoSdOdBoUn3yPz16j3V7zvkUx-Xbiq0_vlSvklj45Bo7zgpLOXgLDYvY34h4MX8yHB4ynBAZEKG1ySIv76DPjn6MIH2FTP_bpI4lSvJxF5KsuPlFHsj8HWTmk57TeUgZ1IOgQn0muGLK1LhrRzKOkdOU6VBV_Hu0Sas0z9jTZL7Xnj1pTmGAn1hueC-6NgkxaZ-7dKqF4BQrr7zNt63_rPZi0ev47vcTV3ga68NUYLH5PfS8XIqJ_OV7ylouw1qDrE9SVN8a5KRrz8V3AokjThcsJvsMQ8C1MhbEm88QICdNKF5nu7kPYR6SsOfJJ1HYY-QBX3wf6YO3VAF_fPpQ"))
                .ClientConfigurationData;

            var pulsarSystem = new PulsarSystem(clientConfig);
            while (true)
            {
                var cmd = Console.ReadLine();
                switch (cmd)
                {
                    #region producers

                    case "0":
                        Console.WriteLine("[PlainAvroBulkSendProducer] Enter topic: ");
                        var t = Console.ReadLine();
                        PlainAvroBulkSendProducer(pulsarSystem, t);
                        break;
                    case "1":
                        Console.WriteLine("[PlainAvroProducer] Enter topic: ");
                        var t1 = Console.ReadLine();
                        PlainAvroProducer(pulsarSystem, t1);
                        break;
                    case "2":
                        Console.WriteLine("[PlainByteBulkSendProducer] Enter topic: ");
                        var t2 = Console.ReadLine();
                        PlainByteBulkSendProducer(pulsarSystem, t2);
                        break;
                    case "3":
                        Console.WriteLine("[PlainByteProducer] Enter topic: ");
                        var t3 = Console.ReadLine();
                        PlainByteProducer(pulsarSystem, t3);
                        break;
                    case "4":
                        Console.WriteLine("[EncryptedAvroBulkSendProducer] Enter topic: ");
                        var t4 = Console.ReadLine();
                        EncryptedAvroBulkSendProducer(pulsarSystem, t4);
                        break;
                    case "5":
                        Console.WriteLine("[EncryptedAvroProducer] Enter topic: ");
                        var t5 = Console.ReadLine();
                        EncryptedAvroProducer(pulsarSystem, t5);
                        break;
                    case "6":
                        Console.WriteLine("[EncryptedByteBulkSendProducer] Enter topic: ");
                        var t6 = Console.ReadLine();
                        EncryptedByteBulkSendProducer(pulsarSystem, t6);
                        break;
                    case "7":
                        Console.WriteLine("[EncryptedByteProducer] Enter topic: ");
                        var t7 = Console.ReadLine();
                        EncryptedByteProducer(pulsarSystem, t7);
                        break;

                    #endregion

                    #region consumers
                    case "8":
                        Console.WriteLine("[PlainAvroBulkSendProducer] Enter topic: ");
                        var t8 = Console.ReadLine();
                        PlainAvroBulkSendProducer(pulsarSystem, t);
                        break;

                    #endregion
                }
            }

            
        }

        #region Producers
        private static void PlainAvroBulkSendProducer(PulsarSystem system, string topic)
        {
            var jsonSchem = JsonSchema.Of(typeof(Students));
            var producerListener = new DefaultProducerListener((o) =>
            {
                Console.WriteLine(o.ToString());
            }, (s, p) => Producers.TryAdd(s, p), s =>
            {
                Receipts.Add(s);
            });
            var producerConfig = new ProducerConfigBuilder()
                .ProducerName(topic.Split("/").Last())
                .Topic(topic)
                .Schema(jsonSchem)
                .SendTimeout(10000)
                .EventListener(producerListener)
                .ProducerConfigurationData;

            var t = system.CreateProducer(new CreateProducer(jsonSchem, producerConfig));
            Console.WriteLine(t);
            IActorRef produce = null;
            while (produce == null)
            {
                Producers.TryGetValue(topic, out produce);
                Thread.Sleep(100);
            }
            Console.WriteLine($"Acquired producer for topic: {topic}");
            while (true)
            {
                var read = Console.ReadLine();
                if (read == "s")
                {
                    var sends = new List<Send>();
                    for (var i = 0; i < 50; i++)
                    {
                        var student = new Students
                        {
                            Name = $"Ebere: {DateTimeOffset.Now.ToUnixTimeMilliseconds()} - presto-ed {DateTime.Now.ToString(CultureInfo.InvariantCulture)}",
                            Age = 2019 + i,
                            School = "Akka-Pulsar university"
                        };
                        var metadata = new Dictionary<string, object>
                        {
                            ["Key"] = "Bulk",
                            ["Properties"] = new Dictionary<string, object> { { "Tick", DateTime.Now.Ticks } }
                        };
                        //var s = JsonSerializer.Serialize(student);
                        sends.Add(new Send(student, topic, metadata.ToImmutableDictionary(), $"{DateTimeOffset.Now.ToUnixTimeMilliseconds()}"));
                    }
                    var bulk = new BulkSend(sends, topic);
                    system.BulkSend(bulk, produce);
                    Task.Delay(5000).Wait();
                    File.AppendAllLines("receipts-bulk.txt", Receipts);
                }

                if (read == "e")
                {
                    return;
                }
            }
        }
        private static void PlainAvroProducer(PulsarSystem system, string topic)
        {
            var jsonSchem = JsonSchema.Of(typeof(Students));
            var producerListener = new DefaultProducerListener((o) =>
            {
                Console.WriteLine(o.ToString());
            }, (s, p) => Producers.TryAdd(s, p), s =>
            {
                Receipts.Add(s);
            });
            var producerConfig = new ProducerConfigBuilder()
                .ProducerName(topic.Split("/").Last())
                .Topic(topic)
                .Schema(jsonSchem)
                .SendTimeout(10000)
                .EventListener(producerListener)
                .ProducerConfigurationData;

            var t = system.CreateProducer(new CreateProducer(jsonSchem, producerConfig));
            Console.WriteLine(t);
            IActorRef produce = null;
            while (produce == null)
            {
                Producers.TryGetValue(topic, out produce);
                Thread.Sleep(100);
            }
            Console.WriteLine($"Acquired producer for topic: {topic}");
            while (true)
            {
                var read = Console.ReadLine();
                if (read == "s")
                {
                    var student = new Students
                    {
                        Name = $"Ebere: {DateTimeOffset.Now.ToUnixTimeMilliseconds()} - presto-ed {DateTime.Now.ToString(CultureInfo.InvariantCulture)}",
                        Age = DateTime.Now.Millisecond,
                        School = "Akka-Pulsar university"
                    };
                    var metadata = new Dictionary<string, object>
                    {
                        ["Key"] = "Single",
                        ["Properties"] = new Dictionary<string, object> { { "Tick", DateTime.Now.Ticks } }
                    };
                    var send = new Send(student, topic, metadata.ToImmutableDictionary(), $"{DateTimeOffset.Now.ToUnixTimeMilliseconds()}");
                    //var s = JsonSerializer.Serialize(student);
                    system.Send(send, produce);
                    Task.Delay(1000).Wait();
                    File.AppendAllLines("receipts.txt", Receipts);
                }

                if (read == "e")
                {
                    return;
                }
            }
        }
        private static void PlainByteBulkSendProducer(PulsarSystem system, string topic)
        {
            var byteSchem = BytesSchema.Of();
            var producerListener = new DefaultProducerListener((o) =>
            {
                Console.WriteLine(o.ToString());
            }, (s, p) => Producers.TryAdd(s, p), s =>
            {
                Receipts.Add(s);
            });
            var producerConfig = new ProducerConfigBuilder()
                .ProducerName(topic.Split("/").Last())
                .Topic(topic)
                .Schema(byteSchem)
                .SendTimeout(10000)
                .EventListener(producerListener)
                .ProducerConfigurationData;

            var t = system.CreateProducer(new CreateProducer(byteSchem, producerConfig));
            Console.WriteLine(t);
            IActorRef produce = null;
            while (produce == null)
            {
                Producers.TryGetValue(topic, out produce);
                Thread.Sleep(100);
            }
            Console.WriteLine($"Acquired producer for topic: {topic}");
            while (true)
            {
                var read = Console.ReadLine();
                if (read == "s")
                {
                    var sends = new List<Send>();
                    for (var i = 0; i < 50; i++)
                    {
                        var student = new Students
                        {
                            Name = $"Ebere: {DateTimeOffset.Now.ToUnixTimeMilliseconds()} - presto-ed {DateTime.Now.ToString(CultureInfo.InvariantCulture)}",
                            Age = 2019 + i,
                            School = "Akka-Pulsar university"
                        };
                        var metadata = new Dictionary<string, object>
                        {
                            ["Key"] = "Bulk",
                            ["Properties"] = new Dictionary<string, object> { { "Tick", DateTime.Now.Ticks } }
                        };
                        var s = JsonSerializer.Serialize(student);
                        sends.Add(new Send(Encoding.UTF8.GetBytes(s), topic, metadata.ToImmutableDictionary(), $"{DateTimeOffset.Now.ToUnixTimeMilliseconds()}"));
                    }
                    var bulk = new BulkSend(sends, topic);
                    system.BulkSend(bulk, produce);
                    Task.Delay(5000).Wait();
                    File.AppendAllLines("receipts-bulk.txt", Receipts);
                }

                if (read == "e")
                {
                    return;
                }
            }
        }
        private static void PlainByteProducer(PulsarSystem system, string topic)
        {
            var byteSchem = BytesSchema.Of();
            var producerListener = new DefaultProducerListener((o) =>
            {
                Console.WriteLine(o.ToString());
            }, (s, p) => Producers.TryAdd(s, p), s =>
            {
                Receipts.Add(s);
            });
            var producerConfig = new ProducerConfigBuilder()
                .ProducerName(topic.Split("/").Last())
                .Topic(topic)
                .Schema(byteSchem)
                .SendTimeout(10000)
                .EventListener(producerListener)
                .ProducerConfigurationData;

            var t = system.CreateProducer(new CreateProducer(byteSchem, producerConfig));
            Console.WriteLine(t);
            IActorRef produce = null;
            while (produce == null)
            {
                Producers.TryGetValue(topic, out produce);
                Thread.Sleep(100);
            }
            Console.WriteLine($"Acquired producer for topic: {topic}");
            while (true)
            {
                var read = Console.ReadLine();
                if (read == "s")
                {
                    var student = new Students
                    {
                        Name = $"Ebere: {DateTimeOffset.Now.ToUnixTimeMilliseconds()} - presto-ed {DateTime.Now.ToString(CultureInfo.InvariantCulture)}",
                        Age = DateTime.Now.Millisecond,
                        School = "Akka-Pulsar university"
                    };
                    var metadata = new Dictionary<string, object>
                    {
                        ["Key"] = "Single",
                        ["Properties"] = new Dictionary<string, object> { { "Tick", DateTime.Now.Ticks } }
                    };
                    var s = JsonSerializer.Serialize(student);
                    var send = new Send(Encoding.UTF8.GetBytes(s), topic, metadata.ToImmutableDictionary(), $"{DateTimeOffset.Now.ToUnixTimeMilliseconds()}");
                    system.Send(send, produce);
                    Task.Delay(1000).Wait();
                    File.AppendAllLines("receipts.txt", Receipts);
                }

                if (read == "e")
                {
                    return;
                }
            }
        }

        private static void EncryptedAvroBulkSendProducer(PulsarSystem system, string topic)
        {
            var jsonSchem = JsonSchema.Of(typeof(Students));
            var producerListener = new DefaultProducerListener((o) =>
            {
                Console.WriteLine(o.ToString());
            }, (s, p) => Producers.TryAdd(s, p), s =>
            {
                Receipts.Add(s);
            });
            var producerConfig = new ProducerConfigBuilder()
                .ProducerName(topic.Split("/").Last())
                .Topic(topic)
                //presto cannot parse encrypted messages
                .CryptoKeyReader(new RawFileKeyReader("pulsar_client.pem", "pulsar_client_priv.pem"))
                .Schema(jsonSchem)
                .AddEncryptionKey("Crypto")
                .SendTimeout(10000)
                .EventListener(producerListener)
                .ProducerConfigurationData;

            var t = system.CreateProducer(new CreateProducer(jsonSchem, producerConfig));
            Console.WriteLine(t);
            IActorRef produce = null;
            while (produce == null)
            {
                Producers.TryGetValue(topic, out produce);
                Thread.Sleep(100);
            }
            Console.WriteLine($"Acquired producer for topic: {topic}");
            while (true)
            {
                var read = Console.ReadLine();
                if (read == "s")
                {
                    var sends = new List<Send>();
                    for (var i = 0; i < 50; i++)
                    {
                        var student = new Students
                        {
                            Name = $"Ebere: {DateTimeOffset.Now.ToUnixTimeMilliseconds()} - presto-ed {DateTime.Now.ToString(CultureInfo.InvariantCulture)}",
                            Age = 2019 + i,
                            School = "Akka-Pulsar university"
                        };
                        var metadata = new Dictionary<string, object>
                        {
                            ["Key"] = "Bulk",
                            ["Properties"] = new Dictionary<string, object> { { "Tick", DateTime.Now.Ticks } }
                        };
                        //var s = JsonSerializer.Serialize(student);
                        sends.Add(new Send(student, topic, metadata.ToImmutableDictionary(), $"{DateTimeOffset.Now.ToUnixTimeMilliseconds()}"));
                    }
                    var bulk = new BulkSend(sends, topic);
                    system.BulkSend(bulk, produce);
                    Task.Delay(5000).Wait();
                    File.AppendAllLines("receipts-bulk.txt", Receipts);
                }

                if (read == "e")
                {
                    return;
                }
            }
        }
        private static void EncryptedAvroProducer(PulsarSystem system, string topic)
        {
            var jsonSchem = JsonSchema.Of(typeof(Students));
            var producerListener = new DefaultProducerListener((o) =>
            {
                Console.WriteLine(o.ToString());
            }, (s, p) => Producers.TryAdd(s, p), s =>
            {
                Receipts.Add(s);
            });
            var producerConfig = new ProducerConfigBuilder()
                .ProducerName(topic.Split("/").Last())
                .Topic(topic)
                //presto cannot parse encrypted messages
                .CryptoKeyReader(new RawFileKeyReader("pulsar_client.pem", "pulsar_client_priv.pem"))
                .Schema(jsonSchem)
                .AddEncryptionKey("Crypto")
                .SendTimeout(10000)
                .EventListener(producerListener)
                .ProducerConfigurationData;

            var t = system.CreateProducer(new CreateProducer(jsonSchem, producerConfig));
            Console.WriteLine(t);
            IActorRef produce = null;
            while (produce == null)
            {
                Producers.TryGetValue(topic, out produce);
                Thread.Sleep(100);
            }
            Console.WriteLine($"Acquired producer for topic: {topic}");
            while (true)
            {
                var read = Console.ReadLine();
                if (read == "s")
                {
                    var student = new Students
                    {
                        Name = $"Ebere: {DateTimeOffset.Now.ToUnixTimeMilliseconds()} - presto-ed {DateTime.Now.ToString(CultureInfo.InvariantCulture)}",
                        Age = DateTime.Now.Millisecond,
                        School = "Akka-Pulsar university"
                    };
                    var metadata = new Dictionary<string, object>
                    {
                        ["Key"] = "Single",
                        ["Properties"] = new Dictionary<string, object> { { "Tick", DateTime.Now.Ticks } }
                    };
                    var send = new Send(student, topic, metadata.ToImmutableDictionary(), $"{DateTimeOffset.Now.ToUnixTimeMilliseconds()}");
                    //var s = JsonSerializer.Serialize(student);
                    system.Send(send, produce);
                    Task.Delay(1000).Wait();
                    File.AppendAllLines("receipts.txt", Receipts);
                }

                if (read == "e")
                {
                    return;
                }
            }
        }

        private static void EncryptedByteBulkSendProducer(PulsarSystem system, string topic)
        {
            var byteSchem = BytesSchema.Of();
            var producerListener = new DefaultProducerListener((o) =>
            {
                Console.WriteLine(o.ToString());
            }, (s, p) => Producers.TryAdd(s, p), s =>
            {
                Receipts.Add(s);
            });
            var producerConfig = new ProducerConfigBuilder()
                .ProducerName(topic.Split("/").Last())
                .Topic(topic)
                //presto cannot parse encrypted messages
                .CryptoKeyReader(new RawFileKeyReader("pulsar_client.pem", "pulsar_client_priv.pem"))
                .Schema(byteSchem)
                .AddEncryptionKey("Crypto")
                .SendTimeout(10000)
                .EventListener(producerListener)
                .ProducerConfigurationData;

            var t = system.CreateProducer(new CreateProducer(byteSchem, producerConfig));
            Console.WriteLine(t);
            IActorRef produce = null;
            while (produce == null)
            {
                Producers.TryGetValue(topic, out produce);
                Thread.Sleep(100);
            }
            Console.WriteLine($"Acquired producer for topic: {topic}");
            while (true)
            {
                var read = Console.ReadLine();
                if (read == "s")
                {
                    var sends = new List<Send>();
                    for (var i = 0; i < 50; i++)
                    {
                        var student = new Students
                        {
                            Name = $"Ebere: {DateTimeOffset.Now.ToUnixTimeMilliseconds()} - presto-ed {DateTime.Now.ToString(CultureInfo.InvariantCulture)}",
                            Age = 2019 + i,
                            School = "Akka-Pulsar university"
                        };
                        var metadata = new Dictionary<string, object>
                        {
                            ["Key"] = "Bulk",
                            ["Properties"] = new Dictionary<string, object> { { "Tick", DateTime.Now.Ticks } }
                        };
                        var s = JsonSerializer.Serialize(student);
                        sends.Add(new Send(Encoding.UTF8.GetBytes(s), topic, metadata.ToImmutableDictionary(), $"{DateTimeOffset.Now.ToUnixTimeMilliseconds()}"));
                    }
                    var bulk = new BulkSend(sends, topic);
                    system.BulkSend(bulk, produce);
                    Task.Delay(5000).Wait();
                    File.AppendAllLines("receipts-bulk.txt", Receipts);
                }

                if (read == "e")
                {
                    return;
                }
            }
        }
        private static void EncryptedByteProducer(PulsarSystem system, string topic)
        {
            var byteSchem = BytesSchema.Of();
            var producerListener = new DefaultProducerListener((o) =>
            {
                Console.WriteLine(o.ToString());
            }, (s, p) => Producers.TryAdd(s, p), s =>
            {
                Receipts.Add(s);
            });
            var producerConfig = new ProducerConfigBuilder()
                .ProducerName(topic.Split("/").Last())
                .Topic(topic)
                //presto cannot parse encrypted messages
                .CryptoKeyReader(new RawFileKeyReader("pulsar_client.pem", "pulsar_client_priv.pem"))
                .Schema(byteSchem)
                .AddEncryptionKey("Crypto")
                .SendTimeout(10000)
                .EventListener(producerListener)
                .ProducerConfigurationData;

            var t = system.CreateProducer(new CreateProducer(byteSchem, producerConfig));
            Console.WriteLine(t);
            IActorRef produce = null;
            while (produce == null)
            {
                Producers.TryGetValue(topic, out produce);
                Thread.Sleep(100);
            }
            Console.WriteLine($"Acquired producer for topic: {topic}");
            while (true)
            {
                var read = Console.ReadLine();
                if (read == "s")
                {
                    var student = new Students
                    {
                        Name = $"Ebere: {DateTimeOffset.Now.ToUnixTimeMilliseconds()} - presto-ed {DateTime.Now.ToString(CultureInfo.InvariantCulture)}",
                        Age = DateTime.Now.Millisecond,
                        School = "Akka-Pulsar university"
                    };
                    var metadata = new Dictionary<string, object>
                    {
                        ["Key"] = "Single",
                        ["Properties"] = new Dictionary<string, object> { { "Tick", DateTime.Now.Ticks } }
                    };
                    var s = JsonSerializer.Serialize(student);
                    var send = new Send(Encoding.UTF8.GetBytes(s), topic, metadata.ToImmutableDictionary(), $"{DateTimeOffset.Now.ToUnixTimeMilliseconds()}");
                    system.Send(send, produce);
                    Task.Delay(1000).Wait();
                    File.AppendAllLines("receipts.txt", Receipts);
                }

                if (read == "e")
                {
                    return;
                }
            }
        }
        #endregion

        #region Consumers

        private static void PlainAvroConsumer(PulsarSystem system,  string topic)
        {
            var consumerListener = new DefaultConsumerEventListener(Console.WriteLine, (s, c) =>
            {
                if (!Consumers.ContainsKey(s))
                    Consumers.Add(s, c);
            }, (s, response) => LastMessageId.Add(s, response));
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
            }, null);
            var jsonSchem = JsonSchema.Of(typeof(Students));
            var topicLast = topic.Split("/").Last();
            var consumerConfig = new ConsumerConfigBuilder()
                .ConsumerName(topicLast)
                .ForceTopicCreation(true)
                .SubscriptionName($"{topicLast}-Subscription")
                .Topic(topic)

                .ConsumerEventListener(consumerListener)
                .SubscriptionType(CommandSubscribe.SubType.Exclusive)
                .Schema(jsonSchem)
                .MessageListener(messageListener)
                .SubscriptionInitialPosition(SubscriptionInitialPosition.Latest)
                .ConsumerConfigurationData;
            system.CreateConsumer(new CreateConsumer(jsonSchem, consumerConfig, ConsumerType.Single));

        }
        private static void DecryptConsumer(PulsarSystem system, string topic)
        {
            var consumerListener = new DefaultConsumerEventListener(Console.WriteLine, (s, c) =>
            {
                if (!Consumers.ContainsKey(s))
                    Consumers.Add(s, c);
            }, (s, response) => LastMessageId.Add(s, response));
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
            }, null);
            var jsonSchem = JsonSchema.Of(typeof(Students));
            var topicLast = topic.Split("/").Last();
            var consumerConfig = new ConsumerConfigBuilder()
                .ConsumerName(topicLast)
                .ForceTopicCreation(true)
                .SubscriptionName($"{topicLast}-Subscription")
                .CryptoKeyReader(new RawFileKeyReader("pulsar_client.pem", "pulsar_client_priv.pem"))
                .Topic(topic)

                .ConsumerEventListener(consumerListener)
                .SubscriptionType(CommandSubscribe.SubType.Exclusive)
                .Schema(jsonSchem)
                .MessageListener(messageListener)
                .SubscriptionInitialPosition(SubscriptionInitialPosition.Latest)
                .ConsumerConfigurationData;
            system.CreateConsumer(new CreateConsumer(jsonSchem, consumerConfig, ConsumerType.Single));
        }
        private static void DecryptConsumerSeek(PulsarSystem system, string topic)
        {
            var consumerListener = new DefaultConsumerEventListener(Console.WriteLine, (s, c) =>
            {
                if (!Consumers.ContainsKey(s))
                    Consumers.Add(s, c);
            }, (s, response) => LastMessageId.Add(s, response));
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
            },null);
            var jsonSchem = JsonSchema.Of(typeof(Students));
            var topicLast = topic.Split("/").Last();
            var consumerConfig = new ConsumerConfigBuilder()
                .ConsumerName(topicLast)
                .ForceTopicCreation(true)
                .SubscriptionName($"{topicLast}-Subscription")
                .CryptoKeyReader(new RawFileKeyReader("pulsar_client.pem", "pulsar_client_priv.pem"))
                .Topic(topic)

                .ConsumerEventListener(consumerListener)
                .SubscriptionType(CommandSubscribe.SubType.Exclusive)
                .Schema(jsonSchem)
                .MessageListener(messageListener)
                .SubscriptionInitialPosition(SubscriptionInitialPosition.Latest)
                .ConsumerConfigurationData;
            system.CreateConsumer(new CreateConsumer(jsonSchem, consumerConfig, ConsumerType.Single, new Seek(SeekType.Timestamp, DateTime.Now.AddHours(-10))));
        }
        private static void DecryptAvroPatternConsumer(PulsarSystem system, string regex)
        {
            var consumerListener = new DefaultConsumerEventListener(Console.WriteLine, (s, c) =>
            {
                if (!Consumers.ContainsKey(s))
                    Consumers.Add(s, c);
            }, (s, response) => LastMessageId.Add(s, response));
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
            }, null);
            var jsonSchem = JsonSchema.Of(typeof(Students));

            var consumerConfig = new ConsumerConfigBuilder()
                .ConsumerName("pattern-consumer")
                .ForceTopicCreation(true)
                .SubscriptionName("pattern-consumer-Subscription")
                .CryptoKeyReader(new RawFileKeyReader("pulsar_client.pem", "pulsar_client_priv.pem"))
                .TopicsPattern(new Regex(/*"persistent://public/default/.*"*/ regex))

                .ConsumerEventListener(consumerListener)
                .SubscriptionType(CommandSubscribe.SubType.Shared)
                .Schema(jsonSchem)
                .MessageListener(messageListener)
                .SubscriptionInitialPosition(SubscriptionInitialPosition.Latest)
                .ConsumerConfigurationData;
            system.CreateConsumer(new CreateConsumer(jsonSchem, consumerConfig, ConsumerType.Pattern));

        }
        private static void DecryptAvroMultiConsumer(PulsarSystem system, string[] topics)
        {
            var consumerListener = new DefaultConsumerEventListener(Console.WriteLine, (s, c) =>
            {
                if (!Consumers.ContainsKey(s))
                    Consumers.Add(s, c);
            }, (s, response) => LastMessageId.Add(s, response));
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
            }, null);
            var jsonSchem = JsonSchema.Of(typeof(Students));

            var consumerConfig = new ConsumerConfigBuilder()
                .ConsumerName("topics-consumer")
                .ForceTopicCreation(true)
                .SubscriptionName("topics-consumer-Subscription")
                .CryptoKeyReader(new RawFileKeyReader("pulsar_client.pem", "pulsar_client_priv.pem"))
                .Topic(topics)

                .ConsumerEventListener(consumerListener)
                .SubscriptionType(CommandSubscribe.SubType.Shared)
                .Schema(jsonSchem)
                .MessageListener(messageListener)
                .SubscriptionInitialPosition(SubscriptionInitialPosition.Latest)
                .ConsumerConfigurationData;
            system.CreateConsumer(new CreateConsumer(jsonSchem, consumerConfig, ConsumerType.Multi));

        }
        private static void PlainBytesConsumer(PulsarSystem system, string topic)
        {
            var consumerListener = new DefaultConsumerEventListener(Console.WriteLine, (s, c) =>
            {
                if (!Consumers.ContainsKey(s))
                    Consumers.Add(s, c);
            }, (s, response) => LastMessageId.Add(s, response));
            var messageListener = new DefaultMessageListener((a, m) =>
            {
                var s = Encoding.UTF8.GetString((byte[])(object)m.Data);
                var students = JsonSerializer.Deserialize<Students>(s);
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
            }, null);
            var byteSchem = BytesSchema.Of();
            var topicLast = topic.Split("/").Last();
            var consumerConfig = new ConsumerConfigBuilder()
                .ConsumerName(topicLast)
                .ForceTopicCreation(true)
                .SubscriptionName($"{topicLast}-Subscription")
                .Topic(topic)

                .ConsumerEventListener(consumerListener)
                .SubscriptionType(CommandSubscribe.SubType.Exclusive)
                .Schema(byteSchem)
                .MessageListener(messageListener)
                .SubscriptionInitialPosition(SubscriptionInitialPosition.Latest)
                .ConsumerConfigurationData;
            system.CreateConsumer(new CreateConsumer(byteSchem, consumerConfig, ConsumerType.Single));

        }
        private static void DecryptBytesConsumer(PulsarSystem system, string topic)
        {
            var consumerListener = new DefaultConsumerEventListener(Console.WriteLine, (s, c) =>
            {
                if (!Consumers.ContainsKey(s))
                    Consumers.Add(s, c);
            }, (s, response) => LastMessageId.Add(s, response));
            var messageListener = new DefaultMessageListener((a, m) =>
            {
                var s = Encoding.UTF8.GetString((byte[])(object)m.Data);
                var students = JsonSerializer.Deserialize<Students>(s);
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
            }, null);
            var byteSchem = BytesSchema.Of();
            var topicLast = topic.Split("/").Last();
            var consumerConfig = new ConsumerConfigBuilder()
                .ConsumerName(topicLast)
                .ForceTopicCreation(true)
                .SubscriptionName($"{topicLast}-Subscription")
                .CryptoKeyReader(new RawFileKeyReader("pulsar_client.pem", "pulsar_client_priv.pem"))
                .Topic(topic)

                .ConsumerEventListener(consumerListener)
                .SubscriptionType(CommandSubscribe.SubType.Exclusive)
                .Schema(byteSchem)
                .MessageListener(messageListener)
                .SubscriptionInitialPosition(SubscriptionInitialPosition.Latest)
                .ConsumerConfigurationData;
            system.CreateConsumer(new CreateConsumer(byteSchem, consumerConfig, ConsumerType.Single));

        }

        #endregion

        private void PlainAvroReader(PulsarSystem system,  string topic)
        {
            var consumerListener = new DefaultConsumerEventListener(Console.WriteLine, (s, c) =>
            {
                if (!Consumers.ContainsKey(s))
                    Consumers.Add(s, c);
            }, (s, response) => LastMessageId.Add(s, response));
            var readerListener = new DefaultMessageListener(null, message =>
            {
                var students = message.ToTypeOf<Students>();
                Console.WriteLine(JsonSerializer.Serialize(students));
            });
            var jsonSchem = JsonSchema.Of(typeof(Students));
            var readerConfig = new ReaderConfigBuilder()
                .ReaderName("avro-plain-students-reader")
                .Schema(jsonSchem)
                .EventListener(consumerListener)
                .ReaderListener(readerListener)
                .Topic(topic)
                .StartMessageId(MessageIdFields.Latest)
                .ReaderConfigurationData;
            system.CreateReader(new CreateReader(jsonSchem, readerConfig));
        }
        private void PlainBytesReader(PulsarSystem system, string topic)
        {
            var consumerListener = new DefaultConsumerEventListener(Console.WriteLine, (s, c) =>
            {
                if (!Consumers.ContainsKey(s))
                    Consumers.Add(s, c);
            }, (s, response) => LastMessageId.Add(s, response));
            var readerListener = new DefaultMessageListener(null, message =>
            {
                var students = message.ToTypeOf<Students>();
                Console.WriteLine(JsonSerializer.Serialize(students));
            });
            var byteSchem = BytesSchema.Of();
            var readerConfig = new ReaderConfigBuilder()
                .ReaderName("byte-plain-students-reader")
                .Schema(byteSchem)
                .EventListener(consumerListener)
                .ReaderListener(readerListener)
                .Topic(topic)
                .StartMessageId(MessageIdFields.Latest)
                .ReaderConfigurationData;
            system.CreateReader(new CreateReader(byteSchem, readerConfig));
        }

        /// <summary>
        /// If you are going to use SQL, presto cannot parse encrypted messages
        /// </summary>
        /// <param name="system"></param>
        /// <param name="producerEventListener"></param>
        /// <param name="consumerEventListener"></param>
        /// <param name="readerListener"></param>
        private void DecryptAvroReader(PulsarSystem system, string topic)
        {
            var consumerListener = new DefaultConsumerEventListener(Console.WriteLine, (s, c) =>
            {
                if (!Consumers.ContainsKey(s))
                    Consumers.Add(s, c);
            }, (s, response) => LastMessageId.Add(s, response));
            var readerListener = new DefaultMessageListener(null, message =>
            {
                var students = message.ToTypeOf<Students>();
                Console.WriteLine(JsonSerializer.Serialize(students));
            });
            var jsonSchem = JsonSchema.Of(typeof(Students));
            var readerConfig = new ReaderConfigBuilder()
                .ReaderName("avro-crypto-students-reader")
                .CryptoKeyReader(new RawFileKeyReader("pulsar_client.pem", "pulsar_client_priv.pem"))
                .Schema(jsonSchem)
                .EventListener(consumerListener)
                .ReaderListener(readerListener)
                .Topic(topic)
                .StartMessageId(MessageIdFields.Latest)
                .ReaderConfigurationData;
            system.CreateReader(new CreateReader(jsonSchem, readerConfig));
        }
        private void DecryptAvroReaderSeek(PulsarSystem system, string topic)
        {
            var consumerListener = new DefaultConsumerEventListener(Console.WriteLine, (s, c) =>
            {
                if (!Consumers.ContainsKey(s))
                    Consumers.Add(s, c);
            }, (s, response) => LastMessageId.Add(s, response));
            var readerListener = new DefaultMessageListener(null, message =>
            {
                var students = message.ToTypeOf<Students>();
                Console.WriteLine(JsonSerializer.Serialize(students));
            });
            var jsonSchem = JsonSchema.Of(typeof(Students));
            var readerConfig = new ReaderConfigBuilder()
                .ReaderName("avro-crypto-students-reader")
                .CryptoKeyReader(new RawFileKeyReader("pulsar_client.pem", "pulsar_client_priv.pem"))
                .Schema(jsonSchem)
                .EventListener(consumerListener)
                .ReaderListener(readerListener)
                .Topic(topic)
                .StartMessageId(MessageIdFields.Latest)
                .ReaderConfigurationData;
            system.CreateReader(new CreateReader(jsonSchem, readerConfig, new Seek(SeekType.Timestamp, DateTime.Now.AddHours(-10))));
        }

        private void DecryptByteReader(PulsarSystem system, string topic)
        {
            var consumerListener = new DefaultConsumerEventListener(Console.WriteLine, (s, c) =>
            {
                if (!Consumers.ContainsKey(s))
                    Consumers.Add(s, c);
            }, (s, response) => LastMessageId.Add(s, response));
            var readerListener = new DefaultMessageListener(null, message =>
            {
                var students = message.ToTypeOf<Students>();
                Console.WriteLine(JsonSerializer.Serialize(students));
            });
            var byteSchem = BytesSchema.Of();
            var readerConfig = new ReaderConfigBuilder()
                .ReaderName("byte-crypto-students-reader")
                .CryptoKeyReader(new RawFileKeyReader("pulsar_client.pem", "pulsar_client_priv.pem"))
                .Schema(byteSchem)
                .EventListener(consumerListener)
                .ReaderListener(readerListener)
                .Topic(topic)
                .StartMessageId(MessageIdFields.Latest)
                .ReaderConfigurationData;
            system.CreateReader(new CreateReader(byteSchem, readerConfig));
        }

        private void Sql(PulsarSystem system)
        {
            //First, we need to setup connection to presto server(s)
            var servers = new List<string>();
            system.SetupSqlServers(new SqlServers(servers.ToImmutableList()));
            //then we can begin querying
            system.QueryData(new QueryData("", d =>
            {

            }, e =>
            {

            }, "", true));
        }
    }
    public class Students
    {
        public string Name { get; set; }
        public int Age { get; set; }
        public string School { get; set; }
    }
    
}
