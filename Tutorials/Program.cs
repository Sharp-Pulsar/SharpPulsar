using System;
using System.Buffers;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using crypto;
using SharpPulsar;
using SharpPulsar.Configuration;
using SharpPulsar.Interfaces;
using SharpPulsar.Schemas;
using SharpPulsar.User;

namespace Tutorials
{
    //https://helm.kafkaesque.io/#accessing-the-pulsar-cluster-on-localhost
    class Program
    {
        //static string myTopic = $"persistent://public/default/mytopic-2";
        static string myTopic = $"persistent://public/default/mytopic-{Guid.NewGuid()}";
        private static PulsarClient _client;
        static void Main(string[] args)
        {
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
            else
                ProduceConsumer(pulsarClient);

            Console.ReadKey();
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
                    Console.WriteLine($"message '{res}' from topic: {message.TopicName}");
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
                    Console.WriteLine($"[Batched] message '{res}' from topic: {message.TopicName}");
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
                    Console.WriteLine($"message from topic: {message.TopicName}");
                }
            }
            for (var i = 0; i < messageCount; i++)
            {
                var message = (TopicMessage<byte[]>)consumer.Receive();
                if (message != null)
                {
                    consumer.Acknowledge(message);
                    Console.WriteLine($"message from topic: {message.TopicName}");
                }
            }
            for (var i = 0; i < messageCount; i++)
            {
                var message = (TopicMessage<byte[]>)consumer.Receive(); if (message != null)
                {
                    consumer.Acknowledge(message);
                    Console.WriteLine($"message from topic: {message.TopicName}");
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
            var consumer = pulsarClient.NewConsumer(new ConsumerConfigBuilder<byte[]>()
                .Topic(myTopic)
                .ForceTopicCreation(true)
                .SubscriptionName("myTopic-sub-Exclusive")
                .SubscriptionInitialPosition(SharpPulsar.Common.SubscriptionInitialPosition.Earliest));

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
                    Console.WriteLine($"message '{res}' from topic: {message.TopicName}");
                }
            }
        }
        private static void ExclusiveProduceConsumer(PulsarClient pulsarClient)
        {
            var producer = pulsarClient.NewProducer(new ProducerConfigBuilder<byte[]>()
                .AccessMode(SharpPulsar.Common.ProducerAccessMode.Exclusive)
                .Topic(myTopic));;
            

            for (var i = 0; i < 100; i++)
            {
                var data = Encoding.UTF8.GetBytes($"tuts-{i}");
                producer.NewMessage().Value(data).Send();
            }

            var pool = ArrayPool<byte>.Shared;
            var consumer = pulsarClient.NewConsumer(new ConsumerConfigBuilder<byte[]>()
                .Topic(myTopic)
                .ForceTopicCreation(true)
                .SubscriptionName("myTopic-sub-Exclusive")
                .SubscriptionInitialPosition(SharpPulsar.Common.SubscriptionInitialPosition.Earliest));

            for (var i = 0; i < 100; i++)
            {
                var message = (Message<byte[]>)consumer.Receive();
                if (message != null)
                {
                    var payload = pool.Rent((int)message.Data.Length);
                    Array.Copy(sourceArray: message.Data.ToArray(), destinationArray: payload, length: (int)message.Data.Length);

                    consumer.Acknowledge(message);
                    var res = Encoding.UTF8.GetString(message.Data);
                    Console.WriteLine($"message '{res}' from topic: {message.TopicName}");
                }
            }
        }
        private static void Transaction(PulsarClient pulsarClient)
        {
            var txn = (Transaction)pulsarClient.NewTransaction().WithTransactionTimeout(TimeSpan.FromMinutes(5)).Build();

            var producer = pulsarClient.NewProducer(new ProducerConfigBuilder<byte[]>()
                .SendTimeout(0)
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
                    Console.WriteLine($"[1] message '{res}' from topic: {message.TopicName}");
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
                    Console.WriteLine($"[2] message '{res}' from topic: {message.TopicName}");
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
                    Console.WriteLine($"message '{res}' from topic: {message.TopicName}");
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
                .AcknowledgmentGroupTime(0);
            var consumer = client.NewConsumer(jsonSchem, builder);
            var producerConfig = new ProducerConfigBuilder<JournalEntry>()
                .ProducerName(topic.Split("/").Last())
                .Topic(topic)
                .Schema(jsonSchem)
                .SendTimeout(10000);

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
                .AcknowledgmentGroupTime(0);

            var consumer = client.NewConsumer(consumerBuilder);

            var producerBuilder = new ProducerConfigBuilder<byte[]>()
                .Topic(topic)
                .EnableBatching(false)
                .SendTimeout(0);

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
                .AcknowledgmentGroupTime(0);
            var consumer = client.NewConsumer(jsonSchem, builder);
            var producerConfig = new ProducerConfigBuilder<KeyValue<string, string>>()
                .ProducerName(topic.Split("/").Last())
                .Topic(topic)
                .Schema(jsonSchem)
                .SendTimeout(10000);

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
