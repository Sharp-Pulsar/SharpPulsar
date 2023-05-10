using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using SharpPulsar;
using SharpPulsar.Auth.OAuth2;
using SharpPulsar.Builder;
using SharpPulsar.Interfaces;
using SharpPulsar.Schemas;
using SharpPulsar.TransactionImpl;
using SharpPulsar.Trino;
using SharpPulsar.Trino.Message;
using Spectre.Console;
using Testcontainers.Pulsar;
using static SharpPulsar.Protocol.Proto.CommandSubscribe;

namespace Tutorials
{
    //https://helm.kafkaesque.io/#accessing-the-pulsar-cluster-on-localhost
    //docker run -it --name ebere -e PULSAR_STANDALONE_USE_ZOOKEEPER=1 -e PULSAR_PREFIX_acknowledgmentAtBatchIndexLevelEnabled=true -e PULSAR_PREFIX_nettyMaxFrameSizeBytes=5253120 -e PULSAR_PREFIX_transactionCoordinatorEnabled=true -e PULSAR_PREFIX_brokerDeleteInactiveTopicsEnabled=true -e PULSAR_PREFIX_exposingBrokerEntryMetadataToClientEnabled=true -e PULSAR_PREFIX_webSocketServiceEnabled=true -e PULSAR_PREFIX_brokerEntryMetadataInterceptors=org.apache.pulsar.common.intercept.AppendBrokerTimestampMetadataInterceptor,org.apache.pulsar.common.intercept.AppendIndexMetadataInterceptor -p 6650:6650  -p 8080:8080 -p 8081:8081 apachepulsar/pulsar-all:2.11.1 sh -c "bin/apply-config-from-env.py conf/standalone.conf && bin/pulsar standalone"

    class Program
    {
        //static string myTopic = $"persistent://public/default/mytopic-2";
        private static PulsarContainer _container;
        //private static TestcontainerConfiguration _configuration = new("apachepulsar/pulsar-all:2.10.0", 6650);

        static string myTopic = $"persistent://public/default/mytopic-{Guid.NewGuid()}";
        //static string myTopic = $"persistent://public/default/mytopic-pulsar";
        private static PulsarClient _client;
        static async Task Main(string[] args)
        {
            await StartContainer();
            var url = "pulsar://127.0.0.1:6650";
            //pulsar client settings builder
            Console.WriteLine("Welcome!!");
            Console.WriteLine("Select 0(none-tls) 1(tls)");
            var selections = new List<string> { "0", "1" };
            var selection = Console.ReadLine();
            var selected = selections.Contains(selection);
            while (!selected)
            {
                Console.WriteLine($"Invalid selection: expected 0 or 1. Retry!![{selection}]");
                selection = Console.ReadLine();
                selected = selection != "0";
            }
            if (selection.Equals("1"))
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

            clientConfig.EnableTransaction(true);

            //pulsar actor system
            var pulsarSystem = PulsarSystem.GetInstance(actorSystemName: "program");

            var pulsarClient = await pulsarSystem.NewClient(clientConfig);
            _client = pulsarClient;

            while (cmd.ToLower() != "exit")
            {
                await HandleCmd(cmd, pulsarClient);
                Console.WriteLine(".....waiting");
                cmd = Console.ReadLine();
            }
            Console.ReadKey();
            await _container.StopAsync();
        }
        private static async ValueTask HandleCmd(string cmd, PulsarClient pulsarClient)
        {
            if (cmd.Equals("txn", StringComparison.OrdinalIgnoreCase))
                await Transaction(pulsarClient);
            else if (cmd.Equals("exclusive", StringComparison.OrdinalIgnoreCase))
                await ExclusiveProduceConsumer(pulsarClient);
            else if (cmd.Equals("exclusive2", StringComparison.OrdinalIgnoreCase))
                await ExclusiveProduceNoneConsumer(pulsarClient);
            else if (cmd.Equals("batch", StringComparison.OrdinalIgnoreCase))
                await BatchProduceConsumer(pulsarClient);
            else if (cmd.Equals("multi", StringComparison.OrdinalIgnoreCase))
                await MultiConsumer(pulsarClient);
            else if (cmd.Equals("avro", StringComparison.OrdinalIgnoreCase))
                await PlainAvroProducer(pulsarClient, "plain-avro");
            else if (cmd.Equals("keyvalue", StringComparison.OrdinalIgnoreCase))
                await PlainKeyValueProducer(pulsarClient, "keyvalue");
            else if (cmd.Equals("txnunack", StringComparison.OrdinalIgnoreCase))
                await TxnRedeliverUnack(pulsarClient);
            else if (cmd.Equals("sql", StringComparison.OrdinalIgnoreCase))
                await TestQuerySql();
            else if (cmd.Equals("live", StringComparison.OrdinalIgnoreCase))
                await LiveProduceConsumer(pulsarClient);
            else
                await ProduceConsumer(pulsarClient);
        }
        private static async ValueTask StartContainer()
        {
            _container = BuildContainer()
              .WithCleanUp(true)
              .Build();

            await _container.StartAsync();
            Console.WriteLine("Start Test Container");
            await AwaitPortReadiness($"http://127.0.0.1:8080/metrics/");
            await _container.ExecAsync(new List<string> { @"./bin/pulsar", "sql-worker", "start" });
            await AwaitPortReadiness($"http://127.0.0.1:8081/");
            Console.WriteLine("AwaitPortReadiness Test Container");
        }
        private static async ValueTask ProduceConsumer(PulsarClient pulsarClient)
        {
            var consumer = await pulsarClient
                .NewConsumerAsync(new ConsumerConfigBuilder<byte[]>()
                .Topic(myTopic)
                .ForceTopicCreation(true)
                .SubscriptionName($"sub-{Guid.NewGuid()}")
                .IsAckReceiptEnabled(true));

            var producer = await pulsarClient
                .NewProducerAsync(new ProducerConfigBuilder<byte[]>()
                .Topic(myTopic));
            

            for (var i = 0; i < 1000; i++)
            {
                var data = Encoding.UTF8.GetBytes($"tuts-{i}");
                var id = await producer.NewMessage().Value(data).SendAsync();
            }

            await Task.Delay(TimeSpan.FromSeconds(10));
            var table = new Table().Expand().BorderColor(Color.Grey);
            table.AddColumn("[yellow]Messageid[/]");
            table.AddColumn("[yellow]Producer Name[/]");
            table.AddColumn("[yellow]Topic[/]");
            table.AddColumn("[yellow]Message[/]");
            await AnsiConsole.Live(table)
            .AutoClear(false)
            .Overflow(VerticalOverflow.Ellipsis)
            .Cropping(VerticalOverflowCropping.Bottom)
            .StartAsync(async ctx =>
            {

                for (var i = 0; i < 1000; i++)
                {
                    var message = (Message<byte[]>)await consumer.ReceiveAsync();
                    if (message != null)
                    {
                        await consumer.AcknowledgeAsync(message);
                        var res = Encoding.UTF8.GetString(message.Data);
                        table.AddRow(message.GetMessageId().ToString(), message.ProducerName, message.Topic, res);
                        ctx.Refresh();
                    }
                }
            });
            AnsiConsole.MarkupLine("Press [yellow]CTRL+C[/] to exit");
        }
        private static async ValueTask LiveProduceConsumer(PulsarClient pulsarClient)
        {
            var consumer = await pulsarClient
                .NewConsumerAsync(new ConsumerConfigBuilder<byte[]>()
                .Topic(myTopic)
                .ForceTopicCreation(true)
                .SubscriptionName($"sub-{Guid.NewGuid()}")
                .IsAckReceiptEnabled(true));

            var producer = await pulsarClient
                .NewProducerAsync(new ProducerConfigBuilder<byte[]>()
                .Topic(myTopic));

            AnsiConsole.MarkupLine("Press [yellow]CTRL+C[/] to exit");

            var table = new Table().Expand().BorderColor(Color.Grey);
            table.AddColumn("[yellow]Messageid[/]");
            table.AddColumn("[yellow]Producer Name[/]");
            table.AddColumn("[yellow]Topic[/]");
            table.AddColumn("[yellow]Message[/]");
            await AnsiConsole.Live(table)
            .AutoClear(true)
            .Overflow(VerticalOverflow.Ellipsis)
            .Cropping(VerticalOverflowCropping.Bottom)
            .StartAsync(async ctx =>
            {

                while (true)
                {
                    var data = Encoding.UTF8.GetBytes($"living-{DateTime.Now.ToLongDateString()} {DateTime.Now.ToLongTimeString()}");
                    await producer.NewMessage().Value(data).SendAsync();
                    await Task.Delay(500);
                    var message = (Message<byte[]>)await consumer.ReceiveAsync();
                    if (message != null)
                    {
                        if (table.Rows.Count > 45)
                        {
                            // Remove the first one
                            table.Rows.RemoveAt(0);
                        }
                        await consumer.AcknowledgeAsync(message);
                        var res = Encoding.UTF8.GetString(message.Data);
                        table.AddRow(message.GetMessageId().ToString(), message.ProducerName, message.Topic, res);
                        ctx.Refresh();
                        await Task.Delay(100);
                    }
                }
            });
        }
        private static async ValueTask BatchProduceConsumer(PulsarClient pulsarClient)
        {

            var consumer = await pulsarClient
                .NewConsumerAsync(new ConsumerConfigBuilder<byte[]>()
                .Topic(myTopic)
                .ForceTopicCreation(true)
                .SubscriptionName($"myTopic-sub-{Guid.NewGuid()}")
                .SubscriptionInitialPosition(SharpPulsar.Common.SubscriptionInitialPosition.Earliest));

            var producer = await pulsarClient
                .NewProducerAsync(new ProducerConfigBuilder<byte[]>()
                .Topic(myTopic)
                .EnableBatching(true)
                .BatchingMaxMessages(50));
            

            for (var i = 0; i < 100; i++)
            {
                var data = Encoding.UTF8.GetBytes($"batched-tuts-{i}");
                await producer.NewMessage().Value(data).SendAsync();
            }

            var table = new Table().Expand().BorderColor(Color.Grey);
            table.AddColumn("[yellow]Messageid[/]");
            table.AddColumn("[yellow]Producer Name[/]");
            table.AddColumn("[yellow]Topic[/]");
            table.AddColumn("[yellow]Message[/]");

            await AnsiConsole.Live(table)
            .AutoClear(false)
            .Overflow(VerticalOverflow.Ellipsis)
            .Cropping(VerticalOverflowCropping.Bottom)
            .StartAsync(async ctx =>
            {
                for (var i = 0; i < 100; i++)
                {
                    var message = (Message<byte[]>)await consumer.ReceiveAsync();
                    if (message != null)
                    {
                        await consumer.AcknowledgeAsync(message);
                        var res = Encoding.UTF8.GetString(message.Data);
                        table.AddRow(message.GetMessageId().ToString(), message.ProducerName, message.Topic, res);
                        ctx.Refresh();
                    }
                }

            });
        }
        private static async ValueTask MultiConsumer(PulsarClient client)
        {
            var messageCount = 5;
            var first = $"one-topic-{Guid.NewGuid()}";
            var second = $"two-topic-{Guid.NewGuid()}";
            var third = $"three-topic-{Guid.NewGuid()}";
            var builder = new ConsumerConfigBuilder<byte[]>()
                .Topic(first, second, third)
                .ForceTopicCreation(true)
                .SubscriptionName("multi-topic-sub");

            var consumer = await client.NewConsumerAsync(builder);

            await PublishMessages(first, messageCount, "hello Toba", client);
            await PublishMessages(third, messageCount, "hello Toba", client);
            await PublishMessages(second, messageCount, "hello Toba", client);

            var table = new Table().Expand().BorderColor(Color.Grey);
            table.AddColumn("[yellow]Messageid[/]");
            table.AddColumn("[yellow]Producer Name[/]");
            table.AddColumn("[yellow]Topic[/]");
            table.AddColumn("[yellow]Message[/]");

            await AnsiConsole.Live(table)
            .AutoClear(false)
            .Overflow(VerticalOverflow.Ellipsis)
            .Cropping(VerticalOverflowCropping.Bottom)
            .StartAsync(async ctx =>
            {
                for (var i = 0; i < messageCount; i++)
                {
                    var message = (TopicMessage<byte[]>)await consumer.ReceiveAsync();
                    if (message != null)
                    {
                        var res = Encoding.UTF8.GetString(message.Data);
                        await consumer.AcknowledgeAsync(message);
                        table.AddRow(message.MessageId.ToString(), message.ProducerName, message.Topic, res);
                        ctx.Refresh();
                    }
                }
                for (var i = 0; i < messageCount; i++)
                {
                    var message = (TopicMessage<byte[]>)await consumer.ReceiveAsync();
                    if (message != null)
                    {
                        var res = Encoding.UTF8.GetString(message.Data);
                        await consumer.AcknowledgeAsync(message);
                        table.AddRow(message.MessageId.ToString(), message.ProducerName, message.Topic, res);
                        ctx.Refresh();
                    }
                }
                for (var i = 0; i < messageCount; i++)
                {
                    var message = (TopicMessage<byte[]>)await consumer.ReceiveAsync();
                    if (message != null)
                    {
                        var res = Encoding.UTF8.GetString(message.Data);
                        await consumer.AcknowledgeAsync(message);
                        table.AddRow(message.MessageId.ToString(), message.ProducerName, message.Topic, res);
                        ctx.Refresh();
                    }
                }

            });
        }
        private static async ValueTask<List<MessageId>> PublishMessages(string topic, int count, string message, PulsarClient client)
        {
            var keys = new List<MessageId>();
            var builder = new ProducerConfigBuilder<byte[]>()
                .Topic(topic);
            var producer = await client.NewProducerAsync(builder);
            for (var i = 0; i < count; i++)
            {
                var key = "key" + i;
                var data = Encoding.UTF8.GetBytes($"{message}-{i}");
                var id = await producer.NewMessage().Key(key).Value(data).SendAsync();
                keys.Add(id);
            }
            return keys;
        }
        private static async ValueTask ExclusiveProduceNoneConsumer(PulsarClient pulsarClient)
        {

            var consumer = await pulsarClient.NewConsumerAsync(new ConsumerConfigBuilder<byte[]>()
                .Topic(myTopic)
                .ForceTopicCreation(true)
                .SubscriptionName("myTopic-sub-Exclusive")
                .SubscriptionInitialPosition(SharpPulsar.Common.SubscriptionInitialPosition.Earliest));

            var producer = await pulsarClient.NewProducerAsync(new ProducerConfigBuilder<byte[]>()
                .AccessMode(SharpPulsar.Common.ProducerAccessMode.Exclusive)
                .Topic(myTopic));

            try
            {
                var producerNone = await pulsarClient
                    .NewProducerAsync(new ProducerConfigBuilder<byte[]>()
                    .Topic(myTopic));

            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            for (var i = 0; i < 1000; i++)
            {
                var data = Encoding.UTF8.GetBytes($"tuts-{i}");
                await producer.NewMessage().Value(data).SendAsync();
            }

            await Task.Delay(TimeSpan.FromSeconds(10));
            var table = new Table().Expand().BorderColor(Color.Grey);
            table.AddColumn("[yellow]Messageid[/]");
            table.AddColumn("[yellow]Producer Name[/]");
            table.AddColumn("[yellow]Topic[/]");
            table.AddColumn("[yellow]Message[/]");

            await AnsiConsole.Live(table)
            .AutoClear(false)
            .Overflow(VerticalOverflow.Ellipsis)
            .Cropping(VerticalOverflowCropping.Bottom)
            .StartAsync(async ctx =>
            {
                for (var i = 0; i < 1000; i++)
                {
                    var message = (Message<byte[]>)await consumer.ReceiveAsync();
                    if (message != null)
                    {
                        await consumer.AcknowledgeAsync(message);
                        var res = Encoding.UTF8.GetString(message.Data);
                        table.AddRow(message.GetMessageId().ToString(), message.ProducerName, message.Topic, res);
                        ctx.Refresh();
                    }
                }

            });
        }
        private static async ValueTask ExclusiveProduceConsumer(PulsarClient pulsarClient)
        {

            var consumer = await pulsarClient.NewConsumerAsync(new ConsumerConfigBuilder<byte[]>()
                .Topic(myTopic)
                .ForceTopicCreation(true)
                .SubscriptionName("myTopic-sub-Exclusive")
                .SubscriptionInitialPosition(SharpPulsar.Common.SubscriptionInitialPosition.Earliest));


            var producer = await pulsarClient.NewProducerAsync(new ProducerConfigBuilder<byte[]>()
                .AccessMode(SharpPulsar.Common.ProducerAccessMode.Exclusive)
                .Topic(myTopic));;
            

            for (var i = 0; i < 100; i++)
            {
                var data = Encoding.UTF8.GetBytes($"tuts-{i}");
                await producer.NewMessage().Value(data).SendAsync();
            }
            var table = new Table().Expand().BorderColor(Color.Grey);
            table.AddColumn("[yellow]Messageid[/]");
            table.AddColumn("[yellow]Producer Name[/]");
            table.AddColumn("[yellow]Topic[/]");
            table.AddColumn("[yellow]Message[/]");

            await AnsiConsole.Live(table)
            .AutoClear(false)
            .Overflow(VerticalOverflow.Ellipsis)
            .Cropping(VerticalOverflowCropping.Bottom)
            .StartAsync(async ctx =>
            {
                for (var i = 0; i < 100; i++)
                {
                    var message = (Message<byte[]>)await consumer.ReceiveAsync();
                    if (message != null)
                    {
                        await consumer.AcknowledgeAsync(message);
                        var res = Encoding.UTF8.GetString(message.Data);
                        table.AddRow(message.GetMessageId().ToString(), message.ProducerName, message.Topic, res);
                        ctx.Refresh();
                    }
                }

            });
        }
        private static async ValueTask Transaction(PulsarClient pulsarClient)
        {
            var txn = (Transaction) await pulsarClient
                .NewTransaction()
                .WithTransactionTimeout(TimeSpan.FromMinutes(5))
                .BuildAsync();

            var producer = await pulsarClient
                .NewProducerAsync(new ProducerConfigBuilder<byte[]>()
                .SendTimeout(TimeSpan.Zero)
                .Topic(myTopic));

            var consumer = await pulsarClient
                .NewConsumerAsync(new ConsumerConfigBuilder<byte[]>()
                .Topic(myTopic)
                .ForceTopicCreation(true)
                .SubscriptionName("myTopic-sub"));

            for (var i = 0; i < 10; i++)
            {
                var text = $"tuts-{i}";
                var data = Encoding.UTF8.GetBytes(text);
                await producer.NewMessage(txn).Value(data).SendAsync();
            }

            var pool = ArrayPool<byte>.Shared;
            //Should not consume messages as the transaction is not committed yet
            for (var i = 0; i < 10; ++i)
            {
                var message = (Message<byte[]>)await consumer.ReceiveAsync();
                if (message != null)
                {
                    var payload = pool.Rent((int)message.Data.Length);
                    Array.Copy(sourceArray: message.Data.ToArray(), destinationArray: payload, length: (int)message.Data.Length);

                    await consumer.AcknowledgeAsync(message);
                    var res = Encoding.UTF8.GetString(message.Data);
                    Console.WriteLine($"[1] message '{res}' from topic: {message.Topic}");
                }
            }

            await txn.CommitAsync();
            var table = new Table().Expand().BorderColor(Color.Grey);
            table.AddColumn("[yellow]Messageid[/]");
            table.AddColumn("[yellow]Producer Name[/]");
            table.AddColumn("[yellow]Topic[/]");
            table.AddColumn("[yellow]Message[/]");

            await AnsiConsole.Live(table)
            .AutoClear(false)
            .Overflow(VerticalOverflow.Ellipsis)
            .Cropping(VerticalOverflowCropping.Bottom)
            .StartAsync(async ctx => 
            {
                for (var i = 0; i < 10; i++)
                {
                    var message = (Message<byte[]>)await consumer.ReceiveAsync();
                    if (message != null)
                    {
                        await consumer.AcknowledgeAsync(message);
                        var res = Encoding.UTF8.GetString(message.Data);
                        table.AddRow(message.GetMessageId().ToString(), message.ProducerName, message.Topic, res);
                        ctx.Refresh();
                    }
                }

            });            
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

        private static async ValueTask PlainAvroProducer(PulsarClient client, string topic)
        {
            var jsonSchem = AvroSchema<JournalEntry>.Of(typeof(JournalEntry));
            var builder = new ConsumerConfigBuilder<JournalEntry>()
                .Topic(topic)
                .SubscriptionName($"my-subscriber-name-{DateTimeHelper.CurrentUnixTimeMillis()}")
                .AckTimeout(TimeSpan.FromMilliseconds(20000))
                .ForceTopicCreation(true)
                .AcknowledgmentGroupTime(TimeSpan.Zero);

            var consumer = await client.NewConsumerAsync(jsonSchem, builder);
            var producerConfig = new ProducerConfigBuilder<JournalEntry>()
                .ProducerName(topic.Split("/").Last())
                .Topic(topic)
                .Schema(jsonSchem)
                .SendTimeout(TimeSpan.FromMilliseconds(10000));

            var producer = await client.NewProducerAsync(jsonSchem, producerConfig);

            for (var i = 0; i < 10; i++)
            {
                var student = new Students
                {
                    Name = $"Ebere {i}",
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
                await producer.NewMessage().Properties(metadata).Value(journal).SendAsync();
            }
            await Task.Delay(TimeSpan.FromSeconds(5));
            var table = new Table().Expand().BorderColor(Color.Grey);
            table.AddColumn("[yellow]Name[/]");
            table.AddColumn("[yellow]Age[/]");
            table.AddColumn("[yellow]School[/]");

            await AnsiConsole.Live(table)
            .AutoClear(false)
            .Overflow(VerticalOverflow.Ellipsis)
            .Cropping(VerticalOverflowCropping.Bottom)
            .StartAsync(async ctx =>
            {
                for (var i = 0; i < 10; i++)
                {
                    var msg = await consumer.ReceiveAsync();
                    if (msg != null)
                    {
                        var message = msg.Value;
                        var student = JsonSerializer.Deserialize<Students>(Encoding.UTF8.GetString(message.Payload));
                        table.AddRow(student.Name, student.Age.ToString(), student.School);
                        ctx.Refresh();
                    }

                }

            });
        }

        private static async ValueTask TxnRedeliverUnack(PulsarClient client)
        {
            var topic = $"TxnRedeliverUnack_{Guid.NewGuid()}";
            var subName = $"RedeliverUnack-{Guid.NewGuid()}";

            var consumerBuilder = new ConsumerConfigBuilder<byte[]>()
                .Topic(topic)
                .SubscriptionName(subName)
                .ForceTopicCreation(true)
                .EnableBatchIndexAcknowledgment(true)
                .AcknowledgmentGroupTime(TimeSpan.Zero);

            var consumer = await client.NewConsumerAsync(consumerBuilder);

            var producerBuilder = new ProducerConfigBuilder<byte[]>()
                .Topic(topic)
                .EnableBatching(false)
                .SendTimeout(TimeSpan.Zero);

            var producer = await client.NewProducerAsync(producerBuilder);

            var txn = await Txn();

            var messageCnt = 10;
            for (var i = 0; i < messageCnt; i++)
            {
                await producer.NewMessage(txn).Value(Encoding.UTF8.GetBytes("Hello Txn - " + i)).SendAsync();
            }
            Console.WriteLine("produce transaction messages finished");
            var table = new Table().Expand().BorderColor(Color.Grey);
            table.AddColumn("[yellow]Messageid[/]");
            table.AddColumn("[yellow]Producer Name[/]");
            table.AddColumn("[yellow]Topic[/]");
            table.AddColumn("[yellow]Message[/]");
            await AnsiConsole.Live(table)
            .AutoClear(false)
            .Overflow(VerticalOverflow.Ellipsis)
            .Cropping(VerticalOverflowCropping.Bottom)
            .StartAsync(async ctx =>
            {
                // Can't receive transaction messages before commit.
                var message = await consumer.ReceiveAsync();
                if (message == null)
                    Console.WriteLine("transaction messages can't be received before transaction committed");

                await txn.CommitAsync();

                await Task.Delay(TimeSpan.FromSeconds(5));
                for (var i = 0; i < messageCnt; i++)
                {
                    message = await consumer.ReceiveAsync();
                    if (message == null) continue;
                    var msg = Encoding.UTF8.GetString(message.Data);
                    table.AddRow(message.MessageId.ToString(), message.ProducerName, message.Topic, $"[1] {msg}");
                    ctx.Refresh();
                }

                await consumer.RedeliverUnacknowledgedMessagesAsync();
                await Task.Delay(TimeSpan.FromSeconds(10));
                for (var i = 0; i < messageCnt; i++)
                {
                    message = await consumer.ReceiveAsync();
                    if (message != null)
                    {
                        await consumer.AcknowledgeAsync(message);
                        var msg = Encoding.UTF8.GetString(message.Data);
                        table.AddRow(message.MessageId.ToString(), message.ProducerName, message.Topic, $"[2] {msg}");
                        ctx.Refresh();
                    }

                }
            });
            
            AnsiConsole.MarkupLine("Press [yellow]CTRL+C[/] to exit");            
            
        }
        private static async ValueTask<Transaction> Txn()
        {
                return (Transaction)await _client.NewTransaction().WithTransactionTimeout(TimeSpan.FromMinutes(5)).BuildAsync();
        }
        private static async ValueTask PlainKeyValueProducer(PulsarClient client, string topic)
        {
            //var jsonSchem = AvroSchema<JournalEntry>.Of(typeof(JournalEntry));
            var jsonSchem = KeyValueSchema<string, string>.Of(ISchema<string>.String, ISchema<string>.String);

            var builder = new ConsumerConfigBuilder<KeyValue<string, string>>()
                .Topic(topic)
                .SubscriptionName($"subscriber-name-{DateTimeHelper.CurrentUnixTimeMillis()}")
                .AckTimeout(TimeSpan.FromMilliseconds(20000))
                .ForceTopicCreation(true)
                .AcknowledgmentGroupTime(TimeSpan.Zero);

            var consumer = await client.NewConsumerAsync(jsonSchem, builder);
            var producerConfig = new ProducerConfigBuilder<KeyValue<string, string>>()
                .ProducerName(topic.Split("/").Last())
                .Topic(topic)
                .Schema(jsonSchem)
                .SendTimeout(TimeSpan.FromMilliseconds(10000));

            var producer = await client.NewProducerAsync(jsonSchem, producerConfig);

            for (var i = 0; i < 10; i++)
            {
                var metadata = new Dictionary<string, string>
                {
                    ["Key"] = "Single",
                    ["Properties"] = JsonSerializer.Serialize(new Dictionary<string, string> { { "Tick", DateTime.Now.Ticks.ToString() } }, new JsonSerializerOptions { WriteIndented = true })
                };
                await producer.NewMessage().Properties(metadata).Value<string, string>(new KeyValue<string, string>("Ebere", $"Abanonu {i}")).SendAsync();
                
            }
            await Task.Delay(TimeSpan.FromSeconds(5));
            var table = new Table().Expand().BorderColor(Color.Grey);
            table.AddColumn("[yellow]Messageid[/]");
            table.AddColumn("[yellow]Producer Name[/]");
            table.AddColumn("[yellow]Topic[/]");
            table.AddColumn("[yellow]Message[/]");
            await AnsiConsole.Live(table)
            .AutoClear(false)
            .Overflow(VerticalOverflow.Ellipsis)
            .Cropping(VerticalOverflowCropping.Bottom)
            .StartAsync(async ctx =>
            {
                for (var i = 0; i < 10; i++)
                {
                    var message = (Message<KeyValue<string, string>>)await consumer.ReceiveAsync();
                    if (message != null)
                    {
                        await consumer.AcknowledgeAsync(message);
                        var kv = message.Value;
                        table.AddRow(message.GetMessageId().ToString(), message.ProducerName, message.Topic, $"{kv.Key}:{kv.Value.RemoveMarkup()}");
                        ctx.Refresh();
                    }
                }
            });
        }
        private static async ValueTask TestQuerySql()
        {
            var topic = $"query_topics_avro_{Guid.NewGuid()}";
            PublishMessages(topic, 5);
            var option = new ClientOptions { Server = "http://127.0.0.1:8081", Execute = @$"select * from ""{topic}""", Catalog = "pulsar", Schema = "public/default" };

            var sql = PulsarSystem.Sql(option);

            await Task.Delay(TimeSpan.FromSeconds(10));
            var table = new Table().Expand().BorderColor(Color.Grey);
            table.AddColumn("[yellow]Message[/]");
            await AnsiConsole.Live(table)
            .AutoClear(false)
            .Overflow(VerticalOverflow.Ellipsis)
            .Cropping(VerticalOverflowCropping.Bottom)
            .StartAsync(async ctx =>
            {
                var response = await sql.ExecuteAsync(TimeSpan.FromSeconds(30));
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
                                    table.AddRow(ob);
                                    ctx.Refresh();
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
            });            
            
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
        private static PulsarBuilder BuildContainer()
        {
            return new PulsarBuilder();
        }
        internal static async Task RunOauth()
        {
            var fileUri = new Uri(GetConfigFilePath());
            var issuerUrl = new Uri("https://auth.streamnative.cloud/");
            var audience = "urn:sn:pulsar:o-r7y4o:sharp";

            var serviceUrl = "pulsar://localhost:6650";
            var subscriptionName = "my-subscription";
            var topicName = $"my-topic-%{DateTime.Now.Ticks}";

            var clientConfig = new PulsarClientConfigBuilder()
                .ServiceUrl(serviceUrl)
                //.AddTlsCerts()
                .Authentication(AuthenticationFactoryOAuth2.ClientCredentials(issuerUrl, fileUri, audience));

            //pulsar actor system
            var pulsarSystem = PulsarSystem.GetInstance();
            var pulsarClient = await pulsarSystem.NewClient(clientConfig);

            var producer = pulsarClient.NewProducer(new ProducerConfigBuilder<byte[]>()
                .Topic(topicName));

            var consumer = pulsarClient.NewConsumer(new ConsumerConfigBuilder<byte[]>()
                .Topic(topicName)
                .SubscriptionName(subscriptionName)
                .SubscriptionType(SubType.Exclusive));

            var messageId = await producer.SendAsync(Encoding.UTF8.GetBytes($"Sent from C# at '{DateTime.Now}'"));
            Console.WriteLine($"MessageId is: '{messageId}'");

            var message = await consumer.ReceiveAsync();
            Console.WriteLine($"Received: {Encoding.UTF8.GetString(message.Data)}");

            await consumer.AcknowledgeAsync(message.MessageId);
        }
        static string GetConfigFilePath()
        {
            var configFolderName = "Oauth2Files";
            var privateKeyFileName = "o-r7y4o-eabanonu.json";
            var startup = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var indexOfConfigDir = startup.IndexOf(startup, StringComparison.Ordinal);
            var examplesFolder = startup.Substring(0, startup.Length - indexOfConfigDir);
            var configFolder = Path.Combine(examplesFolder, configFolderName);
            var ret = Path.Combine(configFolder, privateKeyFileName);
            if (!File.Exists(ret)) throw new FileNotFoundException("can't find credentials file");
            return ret;
        }
        private static async ValueTask AwaitPortReadiness(string address)
        {
            var waitTries = 20;

            using var handler = new HttpClientHandler
            {
                AllowAutoRedirect = true
            };

            using var client = new HttpClient(handler);

            while (waitTries > 0)
            {
                try
                {
                    await client.GetAsync(address).ConfigureAwait(false);
                    return;
                }
                catch
                {
                    waitTries--;
                    await Task.Delay(5000).ConfigureAwait(false);
                }
            }

            throw new Exception("Unable to confirm Pulsar has initialized");
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
