using System;
using System.Buffers;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using crypto;
using DotNet.Testcontainers.Builders;
using SharpPulsar;
using SharpPulsar.Configuration;
using SharpPulsar.Interfaces;
using SharpPulsar.Schemas;
using SharpPulsar.Sql.Client;
using SharpPulsar.Sql.Message;
using SharpPulsar.User;
using Tutorials.PulsarTestContainer;

namespace Tutorials
{
    //https://helm.kafkaesque.io/#accessing-the-pulsar-cluster-on-localhost
    class Program
    {
        //static string myTopic = $"persistent://public/default/mytopic-2";
        private static TestContainer _container;
        private static TestcontainerConfiguration _configuration = new("apachepulsar/pulsar-all:2.9.1", 6650);
        static string myTopic = $"persistent://public/default/mytopic-{Guid.NewGuid()}";
        private static PulsarClient _client;
        static async Task Main(string[] args)
        {
            await StartContainer();
            var url = "pulsar://127.0.0.1:6650";
            //pulsar client settings builder
            Console.WriteLine("Welcome!!");
            Console.WriteLine("Select 0(none-tls) 1(tls)");
            var selections = new List<string> {"0","1" };
            var selection = Console.ReadLine();
            var selected = selections.Contains(selection);
            while (!selected)
            {
                Console.WriteLine($"Invalid selection: expected 0 or 1. Retry!![{selection}]");
                selection = Console.ReadLine();
                selected = selection != "0";
            }
            if(selection.Equals("1"))
                url = "pulsar+ssl://127.0.0.1:6651";

            
            var clientConfig = new PulsarClientConfigBuilder()
                .ServiceUrl(url);

            if (selection.Equals("1"))
            {
                var ca = new System.Security.Cryptography.X509Certificates.X509Certificate2(@"certs/ca.cert.pem");
                clientConfig.EnableTls(true);
                clientConfig.AddTrustedAuthCert(ca);
            }
            Console.WriteLine("Please, time to execute some command, which do you want?");
            var cmd = Console.ReadLine();

            if (cmd.Equals("txn", StringComparison.OrdinalIgnoreCase) || cmd.Equals("txnunack", StringComparison.OrdinalIgnoreCase))
                clientConfig.EnableTransaction(true);

            //pulsar actor system
            var pulsarSystem = PulsarSystem.GetInstance(clientConfig);

            var pulsarClient = pulsarSystem.NewClient();
            _client = pulsarClient;
            if (cmd.Equals("txn", StringComparison.OrdinalIgnoreCase))
                Transaction(pulsarClient);
            else if (cmd.Equals("exclusive", StringComparison.OrdinalIgnoreCase))
                ExclusiveProduceConsumer(pulsarClient);
            else if (cmd.Equals("exclusive2", StringComparison.OrdinalIgnoreCase))
                ExclusiveProduceNoneConsumer(pulsarClient);
            else if (cmd.Equals("batch", StringComparison.OrdinalIgnoreCase))
                BatchProduceConsumer(pulsarClient);
            else if (cmd.Equals("multi", StringComparison.OrdinalIgnoreCase))
                MultiConsumer(pulsarClient);
            else if (cmd.Equals("avro", StringComparison.OrdinalIgnoreCase))
                PlainAvroProducer(pulsarClient, "plain-avro");
            else if (cmd.Equals("keyvalue", StringComparison.OrdinalIgnoreCase))
                PlainKeyValueProducer(pulsarClient, "keyvalue");
            else if (cmd.Equals("txnunack", StringComparison.OrdinalIgnoreCase))
                TxnRedeliverUnack(pulsarClient);
            else if (cmd.Equals("sql", StringComparison.OrdinalIgnoreCase))
                TestQuerySql();
            else
                ProduceConsumer(pulsarClient);

            Console.ReadKey();
            await _container.StopAsync();
        }
        private static async ValueTask StartContainer()
        {
            _container = BuildContainer()
              .WithCleanUp(true)
              .Build();

            await _container
                .StartAsync()
                .PulsarWait("http://127.0.0.1:8080/metrics/");

            await _container.ExecAsync(new List<string> { @"./bin/pulsar", "sql-worker", "start" })
                .PulsarWait("http://127.0.0.1:8081/"); ;
        }
        private static void ProduceConsumer(PulsarClient pulsarClient)
        {

            var consumer = pulsarClient.NewConsumer(new ConsumerConfigBuilder<byte[]>()
                .Topic(myTopic)
                .ForceTopicCreation(true)
                .SubscriptionName($"sub-{Guid.NewGuid()}")
                .IsAckReceiptEnabled(true));

            var producer = pulsarClient.NewProducer(new ProducerConfigBuilder<byte[]>()
                .Topic(myTopic));
            

            for (var i = 0; i < 1000; i++)
            {
                var data = Encoding.UTF8.GetBytes($"tuts-{i}");
                var id = producer.NewMessage().Value(data).Send();
                Console.WriteLine($"Message Id({id.LedgerId}:{id.EntryId})");
            }

            var pool = ArrayPool<byte>.Shared;

            Thread.Sleep(TimeSpan.FromSeconds(10));
            for (var i = 0; i < 1000; i++)
            {
                var message = (Message<byte[]>)consumer.Receive();
                if (message != null)
                {
                    var payload = pool.Rent((int)message.Data.Length);
                    Array.Copy(sourceArray: message.Data.ToArray(), destinationArray: payload, length: (int)message.Data.Length);

                    consumer.Acknowledge(message);
                    var res = Encoding.UTF8.GetString(message.Data);
                    Console.WriteLine($"message '{res}' from topic: {message.Topic}");
                }
            }
        }
        private static void BatchProduceConsumer(PulsarClient pulsarClient)
        {

            var consumer = pulsarClient.NewConsumer(new ConsumerConfigBuilder<byte[]>()
                .Topic(myTopic)
                .ForceTopicCreation(true)
                .SubscriptionName($"myTopic-sub-{Guid.NewGuid()}")
                .SubscriptionInitialPosition(SharpPulsar.Common.SubscriptionInitialPosition.Earliest));

            var producer = pulsarClient.NewProducer(new ProducerConfigBuilder<byte[]>()
                .Topic(myTopic)
                .EnableBatching(true)
                .BatchingMaxMessages(50));
            

            for (var i = 0; i < 100; i++)
            {
                var data = Encoding.UTF8.GetBytes($"batched-tuts-{i}");
                var noId = producer.NewMessage().Value(data).Send();
                Console.WriteLine(i.ToString());
            }

            var pool = ArrayPool<byte>.Shared;

            for (var i = 0; i < 100; i++)
            {
                var message = (Message<byte[]>)consumer.Receive();
                if (message != null)
                {
                    var payload = pool.Rent((int)message.Data.Length);
                    Array.Copy(sourceArray: message.Data.ToArray(), destinationArray: payload, length: (int)message.Data.Length);

                    consumer.Acknowledge(message);
                    var res = Encoding.UTF8.GetString(message.Data);
                    Console.WriteLine($"[Batched] message '{res}' from topic: {message.Topic}");
                }
            }
        }
        private static void MultiConsumer(PulsarClient client)
        {
            var messageCount = 5;
            var first = $"one-topic-{Guid.NewGuid()}";
            var second = $"two-topic-{Guid.NewGuid()}";
            var third = $"three-topic-{Guid.NewGuid()}";
            var builder = new ConsumerConfigBuilder<byte[]>()
                .Topic(first, second, third)
                .ForceTopicCreation(true)
                .SubscriptionName("multi-topic-sub");

            var consumer = client.NewConsumer(builder);

            PublishMessages(first, messageCount, "hello Toba", client);
            PublishMessages(third, messageCount, "hello Toba", client);
            PublishMessages(second, messageCount, "hello Toba", client);

            for (var i = 0; i < messageCount; i++)
            {
                var message = (TopicMessage<byte[]>)consumer.Receive();
                if(message != null)
                {
                    consumer.Acknowledge(message);
                    Console.WriteLine($"message from topic: {message.Topic}");
                }
            }
            for (var i = 0; i < messageCount; i++)
            {
                var message = (TopicMessage<byte[]>)consumer.Receive();
                if (message != null)
                {
                    consumer.Acknowledge(message);
                    Console.WriteLine($"message from topic: {message.Topic}");
                }
            }
            for (var i = 0; i < messageCount; i++)
            {
                var message = (TopicMessage<byte[]>)consumer.Receive(); if (message != null)
                {
                    consumer.Acknowledge(message);
                    Console.WriteLine($"message from topic: {message.Topic}");
                }
            }
        }
        private static List<MessageId> PublishMessages(string topic, int count, string message, PulsarClient client)
        {
            var keys = new List<MessageId>();
            var builder = new ProducerConfigBuilder<byte[]>()
                .Topic(topic);
            var producer = client.NewProducer(builder);
            for (var i = 0; i < count; i++)
            {
                var key = "key" + i;
                var data = Encoding.UTF8.GetBytes($"{message}-{i}");
                var id = producer.NewMessage().Key(key).Value(data).Send();
                keys.Add(id);
            }
            return keys;
        }
        private static void ExclusiveProduceNoneConsumer(PulsarClient pulsarClient)
        {

            var consumer = pulsarClient.NewConsumer(new ConsumerConfigBuilder<byte[]>()
                .Topic(myTopic)
                .ForceTopicCreation(true)
                .SubscriptionName("myTopic-sub-Exclusive")
                .SubscriptionInitialPosition(SharpPulsar.Common.SubscriptionInitialPosition.Earliest));

            var producer = pulsarClient.NewProducer(new ProducerConfigBuilder<byte[]>()
                .AccessMode(SharpPulsar.Common.ProducerAccessMode.Exclusive)
                .Topic(myTopic));

            try
            {
                var producerNone = pulsarClient.NewProducer(new ProducerConfigBuilder<byte[]>()
                .Topic(myTopic));

            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            for (var i = 0; i < 1000; i++)
            {
                var data = Encoding.UTF8.GetBytes($"tuts-{i}");
                producer.NewMessage().Value(data).Send();
            }

            var pool = ArrayPool<byte>.Shared;

            Thread.Sleep(TimeSpan.FromSeconds(10));
            for (var i = 0; i < 1000; i++)
            {
                var message = (Message<byte[]>)consumer.Receive();
                if (message != null)
                {
                    var payload = pool.Rent((int)message.Data.Length);
                    Array.Copy(sourceArray: message.Data.ToArray(), destinationArray: payload, length: (int)message.Data.Length);

                    consumer.Acknowledge(message);
                    var res = Encoding.UTF8.GetString(message.Data);
                    Console.WriteLine($"message '{res}' from topic: {message.Topic}");
                }
            }
        }
        private static void ExclusiveProduceConsumer(PulsarClient pulsarClient)
        {

            var consumer = pulsarClient.NewConsumer(new ConsumerConfigBuilder<byte[]>()
                .Topic(myTopic)
                .ForceTopicCreation(true)
                .SubscriptionName("myTopic-sub-Exclusive")
                .SubscriptionInitialPosition(SharpPulsar.Common.SubscriptionInitialPosition.Earliest));


            var producer = pulsarClient.NewProducer(new ProducerConfigBuilder<byte[]>()
                .AccessMode(SharpPulsar.Common.ProducerAccessMode.Exclusive)
                .Topic(myTopic));;
            

            for (var i = 0; i < 100; i++)
            {
                var data = Encoding.UTF8.GetBytes($"tuts-{i}");
                producer.NewMessage().Value(data).Send();
            }

            var pool = ArrayPool<byte>.Shared;

            for (var i = 0; i < 100; i++)
            {
                var message = (Message<byte[]>)consumer.Receive();
                if (message != null)
                {
                    var payload = pool.Rent((int)message.Data.Length);
                    Array.Copy(sourceArray: message.Data.ToArray(), destinationArray: payload, length: (int)message.Data.Length);

                    consumer.Acknowledge(message);
                    var res = Encoding.UTF8.GetString(message.Data);
                    Console.WriteLine($"message '{res}' from topic: {message.Topic}");
                }
            }
        }
        private static void Transaction(PulsarClient pulsarClient)
        {
            var txn = (Transaction)pulsarClient.NewTransaction().WithTransactionTimeout(TimeSpan.FromMinutes(5)).Build();

            var producer = pulsarClient.NewProducer(new ProducerConfigBuilder<byte[]>()
                .SendTimeout(TimeSpan.Zero)
                .Topic(myTopic));

            var consumer = pulsarClient.NewConsumer(new ConsumerConfigBuilder<byte[]>()
                .Topic(myTopic)
                .ForceTopicCreation(true)
                .SubscriptionName("myTopic-sub"));

            for (var i = 0; i < 10; i++)
            {
                var text = $"tuts-{i}";
                var data = Encoding.UTF8.GetBytes(text);
                producer.NewMessage(txn).Value(data).Send();
                Console.WriteLine($"produced: {text}");
            }

            var pool = ArrayPool<byte>.Shared;
            //Should not consume messages as the transaction is not committed yet
            for (var i = 0; i < 10; ++i)
            {
                var message = (Message<byte[]>)consumer.Receive();
                if (message != null)
                {
                    var payload = pool.Rent((int)message.Data.Length);
                    Array.Copy(sourceArray: message.Data.ToArray(), destinationArray: payload, length: (int)message.Data.Length);

                    consumer.Acknowledge(message);
                    var res = Encoding.UTF8.GetString(message.Data);
                    Console.WriteLine($"[1] message '{res}' from topic: {message.Topic}");
                }
            }

            txn.Commit();

            for (var i = 0; i < 10; i++)
            {
                var message = (Message<byte[]>)consumer.Receive();
                if (message != null)
                {
                    var payload = pool.Rent((int)message.Data.Length);
                    Array.Copy(sourceArray: message.Data.ToArray(), destinationArray: payload, length: (int)message.Data.Length);

                    consumer.Acknowledge(message);
                    var res = Encoding.UTF8.GetString(message.Data);
                    Console.WriteLine($"[2] message '{res}' from topic: {message.Topic}");
                }
            }
        }
        private static void TlsProduceConsumer(PulsarClient pulsarClient)
        {

            var consumer = pulsarClient.NewConsumer(new ConsumerConfigBuilder<byte[]>()
                .Topic(myTopic)
                .ForceTopicCreation(true)
                .SubscriptionName($"sub-{Guid.NewGuid()}")
                .IsAckReceiptEnabled(true));

            var producer = pulsarClient.NewProducer(new ProducerConfigBuilder<byte[]>()
                .Topic(myTopic));


            for (var i = 0; i < 1000; i++)
            {
                var data = Encoding.UTF8.GetBytes($"tuts-{i}");
                var id = producer.NewMessage().Value(data).Send();
                Console.WriteLine($"Message Id({id.LedgerId}:{id.EntryId})");
            }

            var pool = ArrayPool<byte>.Shared;

            Thread.Sleep(TimeSpan.FromSeconds(10));
            for (var i = 0; i < 1000; i++)
            {
                var message = (Message<byte[]>)consumer.Receive();
                if (message != null)
                {
                    var payload = pool.Rent((int)message.Data.Length);
                    Array.Copy(sourceArray: message.Data.ToArray(), destinationArray: payload, length: (int)message.Data.Length);

                    consumer.Acknowledge(message);
                    var res = Encoding.UTF8.GetString(message.Data);
                    Console.WriteLine($"message '{res}' from topic: {message.Topic}");
                }
            }
        }

        private static void PlainAvroProducer(PulsarClient client, string topic)
        {
            var jsonSchem = AvroSchema<JournalEntry>.Of(typeof(JournalEntry));
            var builder = new ConsumerConfigBuilder<JournalEntry>()
                .Topic(topic)
                .SubscriptionName($"my-subscriber-name-{DateTimeHelper.CurrentUnixTimeMillis()}")
                .AckTimeout(TimeSpan.FromMilliseconds(20000))
                .ForceTopicCreation(true)
                .AcknowledgmentGroupTime(TimeSpan.Zero);
            var consumer = client.NewConsumer(jsonSchem, builder);
            var producerConfig = new ProducerConfigBuilder<JournalEntry>()
                .ProducerName(topic.Split("/").Last())
                .Topic(topic)
                .Schema(jsonSchem)
                .SendTimeout(TimeSpan.FromMilliseconds(10000));

            var producer = client.NewProducer(jsonSchem, producerConfig);

            for (var i = 0; i < 10; i++)
            {
                var student = new Students
                {
                    Name = $"[{i}] Ebere: {DateTimeOffset.Now.ToUnixTimeMilliseconds()} - presto-ed {DateTime.Now.ToString(CultureInfo.InvariantCulture)}",
                    Age = 202 + i,
                    School = "Akka-Pulsar university"
                };
                var journal = new JournalEntry
                {
                    Id = $"[{i}]Ebere: {DateTimeOffset.Now.ToUnixTimeMilliseconds()}",
                    PersistenceId = "sampleActor",
                    IsDeleted = false,
                    Ordering = 0,
                    Payload = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(student)),
                    SequenceNr = 0,
                    Tags = "root"
                };
                var metadata = new Dictionary<string, string>
                {
                    ["Key"] = "Single",
                    ["Properties"] = JsonSerializer.Serialize(new Dictionary<string, string> { { "Tick", DateTime.Now.Ticks.ToString() } }, new JsonSerializerOptions { WriteIndented = true })
                };
                var id = producer.NewMessage().Properties(metadata).Value(journal).Send();
            }
            Thread.Sleep(TimeSpan.FromSeconds(5));
            for (var i = 0; i < 10; i++)
            {
                var msg = consumer.Receive();
                if (msg != null)
                {
                    var receivedMessage = msg.Value;
                    Console.WriteLine(JsonSerializer.Serialize(receivedMessage, new JsonSerializerOptions { WriteIndented = true }));
                }

            }
        }

        private static void TxnRedeliverUnack(PulsarClient client)
        {
            var topic = $"TxnRedeliverUnack_{Guid.NewGuid()}";
            var subName = $"RedeliverUnack-{Guid.NewGuid()}";

            var consumerBuilder = new ConsumerConfigBuilder<byte[]>()
                .Topic(topic)
                .SubscriptionName(subName)
                .ForceTopicCreation(true)
                .EnableBatchIndexAcknowledgment(true)
                .AcknowledgmentGroupTime(TimeSpan.Zero);

            var consumer = client.NewConsumer(consumerBuilder);

            var producerBuilder = new ProducerConfigBuilder<byte[]>()
                .Topic(topic)
                .EnableBatching(false)
                .SendTimeout(TimeSpan.Zero);

            var producer = client.NewProducer(producerBuilder);

            var txn = Txn;

            var messageCnt = 10;
            for (var i = 0; i < messageCnt; i++)
            {
                producer.NewMessage(txn).Value(Encoding.UTF8.GetBytes("Hello Txn - " + i)).Send();
            }
            Console.WriteLine("produce transaction messages finished");

            // Can't receive transaction messages before commit.
            var message = consumer.Receive();
            if(message == null)
                Console.WriteLine("transaction messages can't be received before transaction committed");

            txn.Commit();
            
            Thread.Sleep(TimeSpan.FromSeconds(5));
            for (var i = 0; i < messageCnt; i++)
            {
                message = consumer.Receive();
                if (message == null) continue;
                var msg = Encoding.UTF8.GetString(message.Data);
                Console.WriteLine($"[1] {msg}");
            }
            
            consumer.RedeliverUnacknowledgedMessages();
            Thread.Sleep(TimeSpan.FromSeconds(10));
            for (var i = 0; i < messageCnt; i++)
            {
                message = consumer.Receive();
                if (message != null)
                {
                    consumer.Acknowledge(message);
                    var msg = Encoding.UTF8.GetString(message.Data);
                    Console.WriteLine($"[2] {msg}");
                }
                
            }
            
        }
        private static Transaction Txn
        {

            get
            {
                return (Transaction)_client.NewTransaction().WithTransactionTimeout(TimeSpan.FromMinutes(5)).Build();
            }
        }
        private static void PlainKeyValueProducer(PulsarClient client, string topic)
        {
            //var jsonSchem = AvroSchema<JournalEntry>.Of(typeof(JournalEntry));
            var jsonSchem = KeyValueSchema<string, string>.Of(ISchema<string>.String, ISchema<string>.String);
            var builder = new ConsumerConfigBuilder<KeyValue<string, string>>()
                .Topic(topic)
                .SubscriptionName($"subscriber-name-{DateTimeHelper.CurrentUnixTimeMillis()}")
                .AckTimeout(TimeSpan.FromMilliseconds(20000))
                .ForceTopicCreation(true)
                .AcknowledgmentGroupTime(TimeSpan.Zero);
            var consumer = client.NewConsumer(jsonSchem, builder);
            var producerConfig = new ProducerConfigBuilder<KeyValue<string, string>>()
                .ProducerName(topic.Split("/").Last())
                .Topic(topic)
                .Schema(jsonSchem)
                .SendTimeout(TimeSpan.FromMilliseconds(10000));

            var producer = client.NewProducer(jsonSchem, producerConfig);

            for (var i = 0; i < 10; i++)
            {
                var metadata = new Dictionary<string, string>
                {
                    ["Key"] = "Single",
                    ["Properties"] = JsonSerializer.Serialize(new Dictionary<string, string> { { "Tick", DateTime.Now.Ticks.ToString() } }, new JsonSerializerOptions { WriteIndented = true })
                };
                var id = producer.NewMessage().Properties(metadata).Value<string, string>(new KeyValue<string, string>("Ebere", $"[{i}]Ebere")).Send();
                Console.WriteLine(id.ToString());
            }
            Thread.Sleep(TimeSpan.FromSeconds(5));
            for (var i = 0; i < 10; i++)
            {
                var msg = consumer.Receive();
                if (msg != null)
                {
                    var kv = msg.Value;
                    Console.WriteLine($"key:{kv.Key}, value:{kv.Value}");
                }

            }
        }
        private static void TestQuerySql()
        {
            var topic = $"query_topics_avro_{Guid.NewGuid()}";
            PublishMessages(topic, 5);
            var option = new ClientOptions { Server = "http://127.0.0.1:8081", Execute = @$"select * from ""{topic}""", Catalog = "pulsar", Schema = "public/default" };

            var sql = PulsarSystem.Sql(option);

            Thread.Sleep(TimeSpan.FromSeconds(10));

            var response = sql.Execute(TimeSpan.FromSeconds(30));
            if (response != null)
            {
                var data = response.Response;
                switch (data)
                {
                    case DataResponse dr:
                    {
                        for (var i = 0; i < dr.Data.Count; i++)
                        {
                            var ob = dr.Data.ElementAt(i)["text"].ToString();
                            Console.WriteLine(ob);
                        }
                        Console.WriteLine(JsonSerializer.Serialize(dr.StatementStats, new JsonSerializerOptions { WriteIndented = true }));
                    }
                        break;
                    case StatsResponse sr:
                        Console.WriteLine(JsonSerializer.Serialize(sr.Stats, new JsonSerializerOptions { WriteIndented = true }));
                        break;
                    case ErrorResponse er:
                        Console.WriteLine(JsonSerializer.Serialize(er, new JsonSerializerOptions { WriteIndented = true }));
                        break;
                }
            }
            
        }
        private static ISet<string> PublishMessages(string topic, int count)
        {
            ISet<string> keys = new HashSet<string>();
            var builder = new ProducerConfigBuilder<DataOp>()
                .Topic(topic);
            var producer = _client.NewProducer(AvroSchema<DataOp>.Of(typeof(DataOp)), builder);
            for (var i = 0; i < count; i++)
            {
                var key = "key" + i;
                producer.NewMessage().Key(key).Value(new DataOp { Text = "my-sql-message-" + i }).Send();
                keys.Add(key);
            }
            return keys;
        }
        private static TestcontainersBuilder<TestContainer> BuildContainer()
        {
            return (TestcontainersBuilder<TestContainer>)new TestcontainersBuilder<TestContainer>()
              .WithName("pulsar-console")
              .WithPulsar(_configuration)
              .WithPortBinding(6650, 6650)
              .WithPortBinding(8080, 8080)
              .WithPortBinding(8081, 8081)
              .WithExposedPort(6650)
              .WithExposedPort(8080)
              .WithExposedPort(8081);
        }
    }
    public class Students
    {
        public string Name { get; set; }
        public int Age { get; set; }
        public string School { get; set; }
    }
    public class DataOp
    {
        public string Text { get; set; }
    }
    public class JournalEntry
    {
        public string Id { get; set; }

        public string PersistenceId { get; set; }

        public long SequenceNr { get; set; }

        public bool IsDeleted { get; set; }

        public byte[] Payload { get; set; }
        public long Ordering { get; set; }
        public string Tags { get; set; }
    }
}
