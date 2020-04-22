using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Akka.Actor;
using Newtonsoft.Json;
using PulsarAdmin.Models;
using SharpPulsar.Akka;
using SharpPulsar.Akka.Admin;
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
        //https://www.splunk.com/en_us/blog/it/effectively-once-semantics-in-apache-pulsar.html
        //I think, the substitution of Linux command $(pwd) in Windows is "%cd%".
        public static readonly ConcurrentDictionary<string, Dictionary<string, IActorRef>> Producers = new ConcurrentDictionary<string, Dictionary<string, IActorRef>>();
        public static readonly ConcurrentBag<string> Receipts = new ConcurrentBag<string>();
        public static readonly ConcurrentBag<string> Messages = new ConcurrentBag<string>();
        
        public static readonly Dictionary<string, IActorRef> Consumers = new Dictionary<string, IActorRef>();
        public static readonly Dictionary<string, LastMessageIdResponse> LastMessageId = new Dictionary<string, LastMessageIdResponse>();
        private static bool _queryRunning = true;
        private static long _sequencee = 0;
        static void Main(string[] args)
        {
            Console.WriteLine("Welcome. Enter Pulsar server endpoint");
            var endPoint = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(endPoint))
                 throw new ArgumentException("endpoint cannot be null");
            var uri = new Uri(endPoint);
            if(uri.Scheme.ToLower() != "pulsar")
                throw new ArgumentException("endpoint scheme is invalid");
            Console.WriteLine("Is Broker behind a pulsar proxy(Y/N)");
            var proxy = Console.ReadLine();
            var useProxy = proxy.ToLower() == "y";

            var clientConfig = new PulsarClientConfigBuilder()
                .ServiceUrl(endPoint)
                .ConnectionsPerBroker(1)
                .UseProxy(useProxy)
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
                        Console.WriteLine("[PlainAvroConsumer] Enter topic: ");
                        var t8 = Console.ReadLine();
                        PlainAvroConsumer(pulsarSystem, t8);
                        break;
                    case "9":
                        Console.WriteLine("[DecryptAvroConsumer] Enter topic: ");
                        var t9 = Console.ReadLine();
                        DecryptAvroConsumer(pulsarSystem, t9);
                        break;
                    case "10":
                        Console.WriteLine("[DecryptAvroConsumerSeek] Enter topic: ");
                        var t10 = Console.ReadLine();
                        DecryptAvroConsumerSeek(pulsarSystem, t10);
                        break;
                    case "11":
                        Console.WriteLine("[DecryptAvroPatternConsumer] Enter topic: ");
                        var t11 = Console.ReadLine();
                        DecryptAvroPatternConsumer(pulsarSystem, t11);
                        break;
                    case "12":
                        Console.WriteLine("[DecryptAvroMultiConsumer] Enter comm delimited topics: ");
                        var topics = Console.ReadLine();
                        var t12 = topics.Split(",");
                        DecryptAvroMultiConsumer(pulsarSystem, t12);
                        break;
                    case "13":
                        Console.WriteLine("[PlainBytesConsumer] Enter topic: ");
                        var t13 = Console.ReadLine();
                        PlainBytesConsumer(pulsarSystem, t13);
                        break;
                    case "14":
                        Console.WriteLine("[DecryptBytesConsumer] Enter topic: ");
                        var t14 = Console.ReadLine();
                        DecryptBytesConsumer(pulsarSystem, t14);
                        break;
                    case "22":
                        Console.WriteLine("[PlainAvroConsumerSeek] Enter topic: ");
                        var t22 = Console.ReadLine();
                        PlainAvroConsumerSeek(pulsarSystem, t22);
                        break;
                    #endregion
                    #region Readers

                    case "15":
                        Console.WriteLine("[DecryptAvroReader] Enter topic: ");
                        var t15 = Console.ReadLine();
                        DecryptAvroReader(pulsarSystem, t15);
                        break;
                    case "16":
                        Console.WriteLine("[DecryptAvroReaderSeek] Enter topic: ");
                        var t16 = Console.ReadLine();
                        DecryptAvroReaderSeek(pulsarSystem, t16);
                        break;
                    case "17":
                        Console.WriteLine("[PlainAvroReader] Enter topic: ");
                        var t17 = Console.ReadLine();
                        PlainAvroReader(pulsarSystem, t17);
                        break;
                    case "18":
                        Console.WriteLine("[PlainBytesReader] Enter topic: ");
                        var t18 = Console.ReadLine();
                        PlainBytesReader(pulsarSystem, t18);
                        break;
                    case "19":
                        Console.WriteLine("[DecryptByteReader] Enter topic: ");
                        var t19 = Console.ReadLine();
                        DecryptByteReader(pulsarSystem, t19);
                        break;
                    case "23":
                        Console.WriteLine("[PlainAvroReaderSeek] Enter topic: ");
                        var t23 = Console.ReadLine();
                        PlainAvroReaderSeek(pulsarSystem, t23);
                        break;

                    #endregion
                    #region Sql

                    case "20":
                        Console.WriteLine("[SqlServers] Enter comma delimited servers: ");
                        var servers = Console.ReadLine();
                        var t20 = servers.Split(",");
                        SqlServers(pulsarSystem, t20);
                        break;
                    case "21":
                        Console.WriteLine("[SqlQuery] Enter destination server: ");
                        var server = Console.ReadLine();
                        Console.WriteLine("[SqlQuery] Enter query statement: ");
                        var query = Console.ReadLine();
                        SqlQuery(pulsarSystem, query, server);
                        break;
                    #endregion
                    #region Admin
                    case "24":
                        Console.WriteLine("[GetAllSchemas] Enter destination server: ");
                        var adminserver = Console.ReadLine();
                        GetAllSchemas(pulsarSystem, adminserver);
                        break;
                    case "25":
                        Console.WriteLine("[CreateTenant] Enter destination server: ");
                        CreateTenant(pulsarSystem, Console.ReadLine());
                        break;
                    case "39":
                        Console.WriteLine("[UpdateTenant] Enter destination server: ");
                        UpdateTenant(pulsarSystem, Console.ReadLine());
                        break;
                    case "26":
                        Console.WriteLine("[GetTenants] Enter destination server: ");
                        GetTenants(pulsarSystem, Console.ReadLine());
                        break;
                    case "27":
                        Console.WriteLine("[CreateNamespace] Enter destination server: ");
                        CreateNamespace(pulsarSystem, Console.ReadLine());
                        break;
                    case "41":
                        Console.WriteLine("[DeleteNamespace] Enter destination server: ");
                        DeleteNamespace(pulsarSystem, Console.ReadLine());
                        break;
                    case "28":
                        Console.WriteLine("[CreateNonPartitionedTopic] Enter destination server: ");
                        CreateNonPartitionedTopic(pulsarSystem, Console.ReadLine());
                        break;
                    case "43":
                        Console.WriteLine("[CreateNonPartitionedPersistentTopic] Enter destination server: ");
                        CreateNonPartitionedPersistentTopic(pulsarSystem, Console.ReadLine());
                        break;
                    case "29":
                        Console.WriteLine("[CreatePartitionedTopic] Enter destination server: ");
                        CreatePartitionedTopic(pulsarSystem, Console.ReadLine());
                        break;
                    case "42":
                        Console.WriteLine("[GetPartitionedTopicMetadata] Enter destination server: ");
                        GetPartitionedTopicMetadata(pulsarSystem, Console.ReadLine());
                        break;
                    case "44":
                        Console.WriteLine("[GetPartitionedPersistentTopicMetadata] Enter destination server: ");
                        GetPartitionedPersistentTopicMetadata(pulsarSystem, Console.ReadLine());
                        break;
                    case "30":
                        Console.WriteLine("[GetTopics] Enter destination server: ");
                        GetTopics(pulsarSystem, Console.ReadLine());
                        break;
                    case "31":
                        Console.WriteLine("[GetTopics2] Enter destination server: ");
                        GetTopics2(pulsarSystem, Console.ReadLine());
                        break;
                    case "32":
                        Console.WriteLine("[GetPartitionedTopics] Enter destination server: ");
                        GetPartitionedTopics(pulsarSystem, Console.ReadLine());
                        break;
                    case "33":
                        Console.WriteLine("[SetRetention] Enter destination server: ");
                        SetRetention(pulsarSystem, Console.ReadLine());
                        break;
                    case "34":
                        Console.WriteLine("[GetRetention] Enter destination server: ");
                        GetRetention(pulsarSystem, Console.ReadLine());
                        break;
                    case "35":
                        Console.WriteLine("[SetPersistence] Enter destination server: ");
                        SetPersistence(pulsarSystem, Console.ReadLine());
                        break;
                    case "36":
                        Console.WriteLine("[GetPersistence] Enter destination server: ");
                        GetPersistence(pulsarSystem, Console.ReadLine());
                        break;
                    case "37":
                        Console.WriteLine("[GetList] Enter destination server: ");
                        GetList(pulsarSystem, Console.ReadLine());
                        break;
                    case "38":
                        Console.WriteLine("[GetTenantNamespace] Enter destination server: ");
                        GetTenantNamespace(pulsarSystem, Console.ReadLine());
                        break;
                    case "40":
                        Console.WriteLine("[DeleteTenant] Enter destination server: ");
                        DeleteTenant(pulsarSystem, Console.ReadLine());
                        break;
                    #endregion
                    case "exit":
                        return;
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
            }, (to, n,p) =>
            {
                if(Producers.ContainsKey(to))
                  Producers[to].Add(n, p);
                else
                {
                    Producers[to] = new Dictionary<string, IActorRef> {{n, p}};
                }
            }, s =>
            {
                Receipts.Add(s);
            });
            var producerConfig = new ProducerConfigBuilder()
                .ProducerName(topic)
                .Topic(topic)
                .Schema(jsonSchem)
                .EventListener(producerListener)
                .ProducerConfigurationData;

            var t = system.CreateProducer(new CreateProducer(jsonSchem, producerConfig));
            Console.WriteLine(t);
            IActorRef produce = null;
            while (produce == null)
            {
                produce = Producers.FirstOrDefault(x=> x.Key == t && x.Value.ContainsKey(producerConfig.ProducerName)).Value?.Values.FirstOrDefault();
                Thread.Sleep(1000);
            }
            Console.WriteLine($"Acquired producer for topic: {topic}");
            var sends = new List<Send>();
            for (var i = 0; i < 25; i++)
            {
                var student = new Students
                {
                    Name = $"#LockDown Ebere: {DateTimeOffset.Now.ToUnixTimeMilliseconds()} - presto-ed {DateTime.Now.ToString(CultureInfo.InvariantCulture)}",
                    Age = 2019 + i,
                    School = "Akka-Pulsar university"
                };
                var metadata = new Dictionary<string, object>
                {
                    ["Key"] = "Bulk",
                    ["Properties"] = new Dictionary<string, string> { { "Tick", DateTime.Now.Ticks.ToString() } }
                };
                //var s = JsonSerializer.Serialize(student);
                sends.Add(new Send(student, topic, metadata.ToImmutableDictionary(), $"{DateTimeOffset.Now.ToUnixTimeMilliseconds()}"));
            }
            var bulk = new BulkSend(sends, topic);
            system.BulkSend(bulk, produce);
            Task.Delay(5000).Wait();
            File.AppendAllLines("receipts-bulk.txt", Receipts);
        }
        private static void PlainAvroProducer(PulsarSystem system, string topic)
        {
            var jsonSchem = JsonSchema.Of(typeof(JournalEntry));
            var producerListener = new DefaultProducerListener((o) =>
            {
                Console.WriteLine(o.ToString());
            }, (to,n, p) =>
            {
                if (Producers.ContainsKey(to))
                    Producers[to].Add(n, p);
                else
                {
                    Producers[to] = new Dictionary<string, IActorRef> { { n, p } };
                }
            }, s =>
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
                produce = Producers.FirstOrDefault(x => x.Key == t && x.Value.ContainsKey(producerConfig.ProducerName)).Value?.Values.FirstOrDefault();
                Thread.Sleep(1000);
            }
            Console.WriteLine($"Acquired producer for topic: {topic}");
            var student = new Students
            {
                Name = $"Ebere: {DateTimeOffset.Now.ToUnixTimeMilliseconds()} - presto-ed {DateTime.Now.ToString(CultureInfo.InvariantCulture)}",
                Age = 2020,
                School = "Akka-Pulsar university"
            };
            var journal = new JournalEntry
            {
                Id = $"Ebere: {DateTimeOffset.Now.ToUnixTimeMilliseconds()}",
                PersistenceId = "sampleActor",
                IsDeleted = false,
                Ordering =  0,
                Payload = JsonSerializer.Serialize(student),
                SequenceNr = _sequencee,
                Tags = "root"
            };
            var metadata = new Dictionary<string, object>
            {
                ["Key"] = "Single",
                ["Properties"] = new Dictionary<string, string> { { "Tick", DateTime.Now.Ticks.ToString() } }
            };
            var send = new Send(journal, topic, metadata.ToImmutableDictionary(), $"{DateTimeOffset.Now.ToUnixTimeMilliseconds()}");
            //var s = JsonSerializer.Serialize(student);
            system.Send(send, produce);
            Task.Delay(1000).Wait();
            File.AppendAllLines("receipts.txt", Receipts);
            _sequencee++;
        }
        private static void PlainByteBulkSendProducer(PulsarSystem system, string topic)
        {
            var byteSchem = BytesSchema.Of();
            var producerListener = new DefaultProducerListener((o) =>
            {
                Console.WriteLine(o.ToString());
            }, (to, n, p) =>
            {
                if (Producers.ContainsKey(to))
                    Producers[to].Add(n, p);
                else
                {
                    Producers[to] = new Dictionary<string, IActorRef> { { n, p } };
                }
            }, s =>
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
                produce = Producers.FirstOrDefault(x => x.Key == t && x.Value.ContainsKey(producerConfig.ProducerName)).Value?.Values.FirstOrDefault();
                Thread.Sleep(1000);
            }
            Console.WriteLine($"Acquired producer for topic: {topic}");
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
                    ["Properties"] = new Dictionary<string, string> { { "Tick", DateTime.Now.Ticks.ToString() } }
                };
                var s = JsonSerializer.Serialize(student);
                sends.Add(new Send(Encoding.UTF8.GetBytes(s), topic, metadata.ToImmutableDictionary(), $"{DateTimeOffset.Now.ToUnixTimeMilliseconds()}"));
            }
            var bulk = new BulkSend(sends, topic);
            system.BulkSend(bulk, produce);
            Task.Delay(5000).Wait();
            File.AppendAllLines("receipts-bulk.txt", Receipts);
        }
        private static void PlainByteProducer(PulsarSystem system, string topic)
        {
            var byteSchem = BytesSchema.Of();
            var producerListener = new DefaultProducerListener((o) =>
            {
                Console.WriteLine(o.ToString());
            }, (to, n, p) =>
            {
                if (Producers.ContainsKey(to))
                    Producers[to].Add(n, p);
                else
                {
                    Producers[to] = new Dictionary<string, IActorRef> { { n, p } };
                }
            }, r =>
            {
                Receipts.Add(r);
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
                produce = Producers.FirstOrDefault(x => x.Key == t && x.Value.ContainsKey(producerConfig.ProducerName)).Value?.Values.FirstOrDefault();
                Thread.Sleep(1000);
            }
            Console.WriteLine($"Acquired producer for topic: {topic}");
            var student = new Students
            {
                Name = $"Ebere: {DateTimeOffset.Now.ToUnixTimeMilliseconds()} - presto-ed {DateTime.Now.ToString(CultureInfo.InvariantCulture)}",
                Age = DateTime.Now.Millisecond,
                School = "Akka-Pulsar university"
            };
            var metadata = new Dictionary<string, object>
            {
                ["Key"] = "Single",
                ["Properties"] = new Dictionary<string, string> { { "Tick", DateTime.Now.Ticks.ToString() } }
            };
            var s = JsonSerializer.Serialize(student);
            var send = new Send(Encoding.UTF8.GetBytes(s), topic, metadata.ToImmutableDictionary(), $"{DateTimeOffset.Now.ToUnixTimeMilliseconds()}");
            system.Send(send, produce);
            Task.Delay(1000).Wait();
            File.AppendAllLines("receipts.txt", Receipts);
        }

        private static void EncryptedAvroBulkSendProducer(PulsarSystem system, string topic)
        {
            var jsonSchem = JsonSchema.Of(typeof(Students));
            var producerListener = new DefaultProducerListener((o) =>
            {
                Console.WriteLine(o.ToString());
            }, (to, n, p) =>
            {
                if (Producers.ContainsKey(to))
                    Producers[to].Add(n, p);
                else
                {
                    Producers[to] = new Dictionary<string, IActorRef> { { n, p } };
                }
            }, s =>
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
                produce = Producers.FirstOrDefault(x => x.Key == t && x.Value.ContainsKey(producerConfig.ProducerName)).Value?.Values.FirstOrDefault();
                Thread.Sleep(1000);
            }
            Console.WriteLine($"Acquired producer for topic: {topic}");
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
                    ["Properties"] = new Dictionary<string, string> { { "Tick", DateTime.Now.Ticks.ToString() } }
                };
                //var s = JsonSerializer.Serialize(student);
                sends.Add(new Send(student, topic, metadata.ToImmutableDictionary(), $"{DateTimeOffset.Now.ToUnixTimeMilliseconds()}"));
            }
            var bulk = new BulkSend(sends, topic);
            system.BulkSend(bulk, produce);
            Task.Delay(5000).Wait();
            File.AppendAllLines("receipts-bulk.txt", Receipts);
        }
        private static void EncryptedAvroProducer(PulsarSystem system, string topic)
        {
            var jsonSchem = JsonSchema.Of(typeof(Students));
            var producerListener = new DefaultProducerListener((o) =>
            {
                Console.WriteLine(o.ToString());
            }, (to, n, p) =>
            {
                if (Producers.ContainsKey(to))
                    Producers[to].Add(n, p);
                else
                {
                    Producers[to] = new Dictionary<string, IActorRef> { { n, p } };
                }
            }, s =>
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
                produce = Producers.FirstOrDefault(x => x.Key == t && x.Value.ContainsKey(producerConfig.ProducerName)).Value?.Values.FirstOrDefault();
                Thread.Sleep(1000);
            }
            Console.WriteLine($"Acquired producer for topic: {topic}");
            var student = new Students
            {
                Name = $"Ebere: {DateTimeOffset.Now.ToUnixTimeMilliseconds()} - presto-ed {DateTime.Now.ToString(CultureInfo.InvariantCulture)}",
                Age = DateTime.Now.Millisecond,
                School = "Akka-Pulsar university"
            };
            var metadata = new Dictionary<string, object>
            {
                ["Key"] = "Single",
                ["Properties"] = new Dictionary<string, string> { { "Tick", DateTime.Now.Ticks.ToString() } }
            };
            var send = new Send(student, topic, metadata.ToImmutableDictionary(), $"{DateTimeOffset.Now.ToUnixTimeMilliseconds()}");
            //var s = JsonSerializer.Serialize(student);
            system.Send(send, produce);
            Task.Delay(1000).Wait();
            File.AppendAllLines("receipts.txt", Receipts);
        }

        private static void EncryptedByteBulkSendProducer(PulsarSystem system, string topic)
        {
            var byteSchem = BytesSchema.Of();
            var producerListener = new DefaultProducerListener((o) =>
            {
                Console.WriteLine(o.ToString());
            }, (to, n, p) =>
            {
                if (Producers.ContainsKey(to))
                    Producers[to].Add(n, p);
                else
                {
                    Producers[to] = new Dictionary<string, IActorRef> { { n, p } };
                }
            }, s =>
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
                produce = Producers.FirstOrDefault(x => x.Key == t && x.Value.ContainsKey(producerConfig.ProducerName)).Value?.Values.FirstOrDefault();
                Thread.Sleep(1000);
            }
            Console.WriteLine($"Acquired producer for topic: {topic}");
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
                    ["Properties"] = new Dictionary<string, string> { { "Tick", DateTime.Now.Ticks.ToString() } }
                };
                var s = JsonSerializer.Serialize(student);
                sends.Add(new Send(Encoding.UTF8.GetBytes(s), topic, metadata.ToImmutableDictionary(), $"{DateTimeOffset.Now.ToUnixTimeMilliseconds()}"));
            }
            var bulk = new BulkSend(sends, topic);
            system.BulkSend(bulk, produce);
            Task.Delay(5000).Wait();
            File.AppendAllLines("receipts-bulk.txt", Receipts);
        }
        private static void EncryptedByteProducer(PulsarSystem system, string topic)
        {
            var byteSchem = BytesSchema.Of();
            var producerListener = new DefaultProducerListener((o) =>
            {
                Console.WriteLine(o.ToString());
            }, (to, n, p) =>
            {
                if (Producers.ContainsKey(to))
                    Producers[to].Add(n, p);
                else
                {
                    Producers[to] = new Dictionary<string, IActorRef> { { n, p } };
                }
            }, r =>
            {
                Receipts.Add(r);
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
                produce = Producers.FirstOrDefault(x => x.Key == t && x.Value.ContainsKey(producerConfig.ProducerName)).Value?.Values.FirstOrDefault();
                Thread.Sleep(1000);
            }
            Console.WriteLine($"Acquired producer for topic: {topic}");
            var student = new Students
            {
                Name = $"Ebere: {DateTimeOffset.Now.ToUnixTimeMilliseconds()} - presto-ed {DateTime.Now.ToString(CultureInfo.InvariantCulture)}",
                Age = DateTime.Now.Millisecond,
                School = "Akka-Pulsar university"
            };
            var metadata = new Dictionary<string, object>
            {
                ["Key"] = "Single",
                ["Properties"] = new Dictionary<string, string> { { "Tick", DateTime.Now.Ticks.ToString() } }
            };
            var s = JsonSerializer.Serialize(student);
            var send = new Send(Encoding.UTF8.GetBytes(s), topic, metadata.ToImmutableDictionary(), $"{DateTimeOffset.Now.ToUnixTimeMilliseconds()}");
            system.Send(send, produce);
            Task.Delay(1000).Wait();
            File.AppendAllLines("receipts.txt", Receipts);
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
                var students = m.ToTypeOf<JournalEntry>();
                var s = JsonSerializer.Serialize(students);
                Messages.Add(s);
                Console.WriteLine(s);
                if (m.MessageId is MessageId mi)
                {
                    a.Tell(new AckMessage(new MessageIdReceived(mi.LedgerId, mi.EntryId, -1, mi.PartitionIndex)));
                    Console.WriteLine($"Consumer >> {students.Id}- partition: {mi.PartitionIndex}");
                }
                else if (m.MessageId is BatchMessageId b)
                {
                    a.Tell(new AckMessage(new MessageIdReceived(b.LedgerId, b.EntryId, b.BatchIndex, b.PartitionIndex)));
                    Console.WriteLine($"Consumer >> {students.Id}- partition: {b.PartitionIndex}");
                }
                else
                    Console.WriteLine($"Unknown messageid: {m.MessageId.GetType().Name}");
            }, null);
            var jsonSchem = JsonSchema.Of(typeof(JournalEntry));
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
                .SubscriptionInitialPosition(SubscriptionInitialPosition.Earliest)
                .ConsumerConfigurationData;
            system.CreateConsumer(new CreateConsumer(jsonSchem, consumerConfig, ConsumerType.Single));

        }
        private static void DecryptAvroConsumer(PulsarSystem system, string topic)
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
        private static void PlainAvroConsumerSeek(PulsarSystem system, string topic)
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
            system.CreateConsumer(new CreateConsumer(jsonSchem, consumerConfig, ConsumerType.Single, /*new Seek(SeekType.MessageId, "58,1")*/ new Seek(SeekType.Timestamp, DateTimeOffset.Now.AddDays(-15).ToUnixTimeMilliseconds())));
        }
        private static void DecryptAvroConsumerSeek(PulsarSystem system, string topic)
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

        private static void PlainAvroReader(PulsarSystem system,  string topic)
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
        private static void PlainBytesReader(PulsarSystem system, string topic)
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
        private static void DecryptAvroReader(PulsarSystem system, string topic)
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
        private static void DecryptAvroReaderSeek(PulsarSystem system, string topic)
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
        private static void PlainAvroReaderSeek(PulsarSystem system, string topic)
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
                .ReaderName("avro-students-reader")
                .Schema(jsonSchem)
                .EventListener(consumerListener)
                .ReaderListener(readerListener)
                .Topic(topic)
                .StartMessageId(MessageIdFields.Latest)
                .ReaderConfigurationData;
            system.CreateReader(new CreateReader(jsonSchem, readerConfig, new Seek(SeekType.Timestamp, DateTimeOffset.Now.AddDays(-20).ToUnixTimeMilliseconds())));
        }

        private static void DecryptByteReader(PulsarSystem system, string topic)
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

        private static void SqlServers(PulsarSystem system, string[] servers)
        {
            //First, we need to setup connection to presto server(s)
            system.SetupSqlServers(new SqlServers(servers.ToImmutableList()));
            
        }
        private static void SqlQuery(PulsarSystem system, string query, string server)
        {
            //then we can begin querying
            _queryRunning = true;
            system.PulsarSql(new Sql(query, d =>
            {
                if (d.ContainsKey("Finished"))
                {
                    _queryRunning = false;
                    return;
                }
                Console.WriteLine(d["Message"]);
                var metadata = JsonSerializer.Deserialize<Dictionary<string, object>>(d["Metadata"]);
                //Console.WriteLine(d["Metadata"]);
                foreach (var m in metadata)
                {
                    //Convert.ChangeType(m.Value, m.Value.GetType())
                    if(m.Value != null)
                      Console.WriteLine($"{m.Key} : {JsonSerializer.Serialize(m.Value, new JsonSerializerOptions{WriteIndented = true})}");
                }
            }, e =>
            {
                Console.WriteLine(e.ToString());
                _queryRunning = false;
            }, server, Console.WriteLine, true));
            while (_queryRunning)
            {
                Thread.Sleep(500);
            }
            Console.WriteLine("FINISHED!!!!!!!");
        }
        private static void GetAllSchemas(PulsarSystem system, string server)
        {
            system.PulsarAdmin(new Admin(AdminCommands.GetAllSchemas, new object[]{ "public", "default", "journal", false}, e =>
            {
                var data = JsonSerializer.Serialize(e, new JsonSerializerOptions {WriteIndented = true});
                Console.WriteLine(data);
            }, e=> Console.WriteLine(e.ToString()), server, l=> Console.WriteLine(l)));
        }
        private static void CreateTenant(PulsarSystem system, string server)
        {
            system.PulsarAdmin(new Admin(AdminCommands.CreateTenant, new object[] { "whitepurple", new TenantInfo { AdminRoles = new List<string> { "Journal", "Query", "Create" }, AllowedClusters = new List<string> { "pulsar" } } }, e =>
            {
                var data = JsonSerializer.Serialize(e, new JsonSerializerOptions { WriteIndented = true });
                Console.WriteLine(data);
            }, e => Console.WriteLine(e.ToString()), server, Console.WriteLine));
        }
        private static void UpdateTenant(PulsarSystem system, string server)
        {
            system.PulsarAdmin(new Admin(AdminCommands.UpdateTenant, new object[] { "whitepurple", new TenantInfo{AdminRoles = new List<string>{"Journal", "Query", "Create"}, AllowedClusters = new List<string>{"pulsar"}} }, e =>
            {
                var data = JsonSerializer.Serialize(e, new JsonSerializerOptions { WriteIndented = true });
                Console.WriteLine(data);
            }, e => Console.WriteLine(e.ToString()), server, Console.WriteLine));
        }
        private static void GetTenants(PulsarSystem system, string server)
        {
            system.PulsarAdmin(new Admin(AdminCommands.GetTenants, new object[] { }, e =>
            {
                var data = JsonSerializer.Serialize(e, new JsonSerializerOptions { WriteIndented = true });
                Console.WriteLine(data);
            }, e => Console.WriteLine(e.ToString()), server, Console.WriteLine));
        }
        private static void CreateNamespace(PulsarSystem system, string server)
        {
            system.PulsarAdmin(new Admin(AdminCommands.CreateNamespace, new object[] {"whitepurple","akka" }, e =>
            {
                var data = JsonSerializer.Serialize(e, new JsonSerializerOptions { WriteIndented = true });
                Console.WriteLine(data);
            }, e => Console.WriteLine(e.ToString()), server, Console.WriteLine));
        }
        private static void DeleteNamespace(PulsarSystem system, string server)
        {
            system.PulsarAdmin(new Admin(AdminCommands.DeleteNamespace, new object[] {"whitepurple","akka",false }, e =>
            {
                var data = JsonSerializer.Serialize(e, new JsonSerializerOptions { WriteIndented = true });
                Console.WriteLine(data);
            }, e => Console.WriteLine(e.ToString()), server, Console.WriteLine));
        }
        private static void CreateNonPartitionedTopic(PulsarSystem system, string server)
        {
            system.PulsarAdmin(new Admin(AdminCommands.CreateNonPartitionedTopic, new object[] { "events", "akka", "journal", false}, e =>
            {
                var data = JsonSerializer.Serialize(e, new JsonSerializerOptions { WriteIndented = true });
                Console.WriteLine(data);
            }, e => Console.WriteLine(e.ToString()), server, Console.WriteLine));
        }
        private static void CreateNonPartitionedPersistentTopic(PulsarSystem system, string server)
        {
            system.PulsarAdmin(new Admin(AdminCommands.CreateNonPartitionedPersistentTopic, new object[] { "whitepurple", "akka", "journal", false}, e =>
            {
                var data = JsonSerializer.Serialize(e, new JsonSerializerOptions { WriteIndented = true });
                Console.WriteLine(data);
            }, e => Console.WriteLine(e.ToString()), server, Console.WriteLine));
        }
        private static void CreatePartitionedTopic(PulsarSystem system, string server)
        {
            system.PulsarAdmin(new Admin(AdminCommands.CreatePartitionedTopic, new object[] { "whitepurple", "akka", "iots", 4 }, e =>
            {
                var data = JsonSerializer.Serialize(e, new JsonSerializerOptions { WriteIndented = true });
                Console.WriteLine(data);
            }, e => Console.WriteLine(e.ToString()), server, Console.WriteLine));
        }
        private static void GetPartitionedTopicMetadata(PulsarSystem system, string server)
        {
            system.PulsarAdmin(new Admin(AdminCommands.GetPartitionedMetadata, new object[] { "whitepurple", "akka", "iots", false,false }, e =>
            {
                var data = JsonSerializer.Serialize(e, new JsonSerializerOptions { WriteIndented = true });
                Console.WriteLine(data);
            }, e => Console.WriteLine(e.ToString()), server, Console.WriteLine));
        }
        private static void GetPartitionedPersistentTopicMetadata(PulsarSystem system, string server)
        {
            system.PulsarAdmin(new Admin(AdminCommands.GetPartitionedMetadataPersistence, new object[] { "whitepurple", "akka", "iots", false,false }, e =>
            {
                var data = JsonSerializer.Serialize(e, new JsonSerializerOptions { WriteIndented = true });
                Console.WriteLine(data);
            }, e => Console.WriteLine(e.ToString()), server, Console.WriteLine));
        }
        private static void GetTopics(PulsarSystem system, string server)
        {
            system.PulsarAdmin(new Admin(AdminCommands.GetTopics, new object[] { "events", "akka", "ALL"}, e =>
            {
                var data = JsonConvert.SerializeObject(e, Formatting.Indented);
                Console.WriteLine(data);
            }, e => Console.WriteLine(e.ToString()), server, Console.WriteLine));
        }
        private static void SetPersistence(PulsarSystem system, string server)
        {
            system.PulsarAdmin(new Admin(AdminCommands.SetPersistence, new object[] { "events", "akka", "ALL"}, e =>
            {
                var data = JsonConvert.SerializeObject(e, Formatting.Indented);
                Console.WriteLine(data);
            }, e => Console.WriteLine(e.ToString()), server, Console.WriteLine));
        }
        private static void GetPersistence(PulsarSystem system, string server)
        {
            system.PulsarAdmin(new Admin(AdminCommands.GetPersistence, new object[] { "events", "akka", "ALL"}, e =>
            {
                var data = JsonConvert.SerializeObject(e, Formatting.Indented);
                Console.WriteLine(data);
            }, e => Console.WriteLine(e.ToString()), server, Console.WriteLine));
        }
        private static void GetList(PulsarSystem system, string server)
        {
            system.PulsarAdmin(new Admin(AdminCommands.GetList, new object[] { "events", "akka", "ALL"}, e =>
            {
                var data = JsonConvert.SerializeObject(e, Formatting.Indented);
                Console.WriteLine(data);
            }, e => Console.WriteLine(e.ToString()), server, Console.WriteLine));
        }
        private static void GetTenantNamespace(PulsarSystem system, string server)
        {
            system.PulsarAdmin(new Admin(AdminCommands.GetTenantNamespaces, new object[] { "whitepurple", "akka", "ALL"}, e =>
            {
                var data = JsonConvert.SerializeObject(e, Formatting.Indented);
                Console.WriteLine(data);
            }, e => Console.WriteLine(e.ToString()), server, Console.WriteLine));
        }
        private static void GetPartitionedTopics(PulsarSystem system, string server)
        {
            system.PulsarAdmin(new Admin(AdminCommands.GetPartitionedTopicList, new object[] { "events", "akka", "ALL"}, e =>
            {
                var data = JsonConvert.SerializeObject(e, Formatting.Indented);
                Console.WriteLine(data);
            }, e => Console.WriteLine(e.ToString()), server, Console.WriteLine));
        }
        private static void SetRetention(PulsarSystem system, string server)
        {
            system.PulsarAdmin(new Admin(AdminCommands.SetRetention, new object[] { "whitepurple", "akka",  new RetentionPolicies(-1, -1)}, e =>
            {
                var data = JsonConvert.SerializeObject(e, Formatting.Indented);
                Console.WriteLine(data);
            }, e => Console.WriteLine(e.ToString()), server, Console.WriteLine));
        }
        private static void GetRetention(PulsarSystem system, string server)
        {
            system.PulsarAdmin(new Admin(AdminCommands.GetRetention, new object[] { "whitepurple", "akka", "ALL"}, e =>
            {
                var data = JsonConvert.SerializeObject(e, Formatting.Indented);
                Console.WriteLine(data);
            }, e => Console.WriteLine(e.ToString()), server, Console.WriteLine));
        }
        private static void DeleteTenant(PulsarSystem system, string server)
        {
            system.PulsarAdmin(new Admin(AdminCommands.DeleteTenant, new object[] { "whitepurple"}, e =>
            {
                var data = JsonConvert.SerializeObject(e, Formatting.Indented);
                Console.WriteLine(data);
            }, e => Console.WriteLine(e.ToString()), server, Console.WriteLine));
        }
        private static void GetTopics2(PulsarSystem system, string server)
        {
            system.PulsarAdmin(new Admin(AdminCommands.GetTopics2, new object[] { "events", "akka", "ALL" }, e =>
            {
                var data = JsonConvert.SerializeObject(e, Formatting.Indented);
                Console.WriteLine(data);
            }, e => Console.WriteLine(e.ToString()), server, Console.WriteLine));
        }
    }
    public class Students
    {
        public string Name { get; set; }
        public int Age { get; set; }
        public string School { get; set; }
    }
    public class Churches
    {
        public string Name { get; set; }
        public int Age { get; set; }
        public string School { get; set; }
    }
    public class JournalEntry
    {
        public string Id { get; set; }

        public string PersistenceId { get; set; }

        public long SequenceNr { get; set; }

        public bool IsDeleted { get; set; }

        public string Payload { get; set; }
        public long Ordering { get; set; }
        public string Tags { get; set; }
    }
    /*pulsar://52.177.109.178:6650
http://40.65.210.106:8081
select * from pulsar."public/default".students ORDER BY __sequence_id__ ASC
select * from pulsar."public/default"."avro-bulk-send-plain" ORDER BY __sequence_id__ ASC
avro-bulk-send-plain
persistent://public/default/avro-bulk-send-plain
persistent://public/default/students
persistent://public/default/avro-bulk*/
}
