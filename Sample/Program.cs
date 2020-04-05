using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.IO;
using System.Text;
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
using SharpPulsar.Impl.Auth;
using SharpPulsar.Impl.Schema;
using SharpPulsar.Protocol.Proto;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Samples
{
    //curl -H "Content-Type: application/json" -X PUT http://{IP_ADDRESS}:7750/pulsar-manager/users/superuser -d '{"name": "platform", "password": "platform", "description": "test1", "email": "username1@test.org"}
    //bin/pulsar sql-worker run -D "java.vendor"="Oracle Corporation"
    //https://developer.ibm.com/articles/kubernetes-networking-what-you-need-to-know/
    //https://streamnative.io/docs/v1.0.0/get-started/helm/
    //https://streamnative.io/docs/v1.0.0/install-and-upgrade/helm/install/deployment/
    //https://pixelrobots.co.uk/2019/06/use-a-static-public-ip-address-outside-of-the-node-resource-group-with-the-azure-kubernetes-service-aks-load-balancer/
    //https://kubernetes.io/docs/tasks/debug-application-cluster/get-shell-running-container/
    //https://docs.microsoft.com/en-us/azure/aks/azure-disk-volume?WT.mc_id=medium-blog-abhishgu
    //https://docs.microsoft.com/en-us/azure/aks/use-multiple-node-pools#assign-a-public-ip-per-node-in-a-node-pool
    //mkdir apache-pulsar && tar xvzf apache-pulsar-2.5.0-bin.tar.gz -C apache-pulsar --strip-components 1
    //https://docs.microsoft.com/en-us/azure/virtual-machines/windows/create-portal-availability-zone
    //https://docs.microsoft.com/en-us/azure/load-balancer/quickstart-load-balancer-standard-public-portal
    //https://pulsar.apache.org/docs/en/deploy-bare-metal-multi-cluster/
    //https://linuxize.com/post/install-java-on-ubuntu-18-04/
    //https://vitux.com/how-to-install-notepad-on-ubuntu/
    //https://blog.alexellis.io/kubernetes-in-10-minutes/
    //https://docs.microsoft.com/en-us/azure/virtual-machines/linux/use-remote-desktop
    //https://docs.microsoft.com/en-us/azure/virtual-machines/linux/ssh-from-windows
    //https://pulsar.apache.org/docs/ja/next/administration-upgrade/
    //https://jack-vanlightly.com/blog/2018/10/21/how-to-not-lose-messages-on-an-apache-pulsar-cluster
    //https://medium.com/capital-one-tech/apache-pulsar-one-cluster-for-the-entire-enterprise-using-multi-tenancy-ac0bd925fbdf
    class Program
    {
        //I think, the substitution of Linux command $(pwd) in Windows is "%cd%".
        public static readonly ConcurrentDictionary<string, IActorRef> Producers = new ConcurrentDictionary<string, IActorRef>();
        public static readonly ConcurrentBag<string> Receipts = new ConcurrentBag<string>();
        public static readonly ConcurrentBag<string> Messages = new ConcurrentBag<string>();
        
        public static readonly Dictionary<string, IActorRef> Consumers = new Dictionary<string, IActorRef>();
        public static readonly Dictionary<string, LastMessageIdResponse> LastMessageId = new Dictionary<string, LastMessageIdResponse>();
        static Task Main(string[] args)
        {
            //var jsonSchema = JsonSchema.Of(ISchemaDefinition.Builder().WithPojo(typeof(Students)).WithAlwaysAllowNull(false).Build());
            //var bytSchema = BytesSchema.Of();
            var producerListener = new DefaultProducerListener((o) =>
            {
                Console.WriteLine(o.ToString());
            }, (s, p) => Producers.TryAdd(s, p), s =>
            {
                Receipts.Add(s);
            });
            var consumerListener = new DefaultConsumerEventListener(Console.WriteLine, (s, c) =>
            {
                if(!Consumers.ContainsKey(s))
                    Consumers.Add(s, c);
                //c.Tell(new TimestampSeek(DateTimeOffset.Now.AddDays(-2).ToUnixTimeMilliseconds()));
            }, (s, response) => LastMessageId.Add(s, response));
            var jsonSchem = JsonSchema.Of(typeof(Students));

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
                var s = JsonSerializer.Serialize(message.Data);
                var students = JsonSerializer.Deserialize<Students>(s); //message.ToTypeOf<Students>();
                Console.WriteLine(JsonSerializer.Serialize(students));
            });

            #endregion
            var clientConfig = new PulsarClientConfigBuilder()
                //.ServiceUrl("pulsar://pulsar-proxy.eastus2.cloudapp.azure.com:6650")
                .ServiceUrl("pulsar://40.65.213.73:6650")//testing purposes only
                .ServiceUrlProvider(new ServiceUrlProviderImpl("pulsar://40.65.213.73:6650"))//testing purposes only
                .ConnectionsPerBroker(1)
                .UseProxy(true)
                .Authentication( new AuthenticationDisabled())
                //.Authentication(AuthenticationFactory.Token("eyJhbGciOiJSUzI1NiJ9.eyJzdWIiOiJzaGFycHB1bHNhci1jbGllbnQtNWU3NzY5OWM2M2Y5MCJ9.lbwoSdOdBoUn3yPz16j3V7zvkUx-Xbiq0_vlSvklj45Bo7zgpLOXgLDYvY34h4MX8yHB4ynBAZEKG1ySIv76DPjn6MIH2FTP_bpI4lSvJxF5KsuPlFHsj8HWTmk57TeUgZ1IOgQn0muGLK1LhrRzKOkdOU6VBV_Hu0Sas0z9jTZL7Xnj1pTmGAn1hueC-6NgkxaZ-7dKqF4BQrr7zNt63_rPZi0ev47vcTV3ga68NUYLH5PfS8XIqJ_OV7ylouw1qDrE9SVN8a5KRrz8V3AokjThcsJvsMQ8C1MhbEm88QICdNKF5nu7kPYR6SsOfJJ1HYY-QBX3wf6YO3VAF_fPpQ"))
                .ClientConfigurationData;

            var pulsarSystem = new PulsarSystem(clientConfig);

            var producerConfig = new ProducerConfigBuilder()
                .ProducerName("students")
                .Topic("students")
                //presto cannot parse encrypted messages
                //.CryptoKeyReader(new RawFileKeyReader("pulsar_client.pem", "pulsar_client_priv.pem"))
                .Schema(jsonSchem)
                
                //.AddEncryptionKey("Crypto3")
                .SendTimeout(10000)
                .EventListener(producerListener)
                .ProducerConfigurationData;

            var topic = pulsarSystem.CreateProducer(new CreateProducer(jsonSchem, producerConfig));


            var readerConfig = new ReaderConfigBuilder()
                .ReaderName("student")
                .Schema(jsonSchem)
                .EventListener(consumerListener)
                .ReaderListener(messageListener)
                .Topic(topic)
                .StartMessageId(MessageIdFields.Latest)
                .ReaderConfigurationData;

            var consumerConfig = new ConsumerConfigBuilder()
                .ConsumerName("student")
                .ForceTopicCreation(true)
                .SubscriptionName("students-Subscription")
                //.CryptoKeyReader(new RawFileKeyReader("pulsar_client.pem", "pulsar_client_priv.pem"))
                //.TopicsPattern(new Regex("persistent://public/default/.*"))
                .Topic(topic)
                
                .ConsumerEventListener(consumerListener)
                .SubscriptionType(CommandSubscribe.SubType.Shared)
                .Schema(jsonSchem)
                .MessageListener(messageListener)
                .SubscriptionInitialPosition(SubscriptionInitialPosition.Latest)
                .ConsumerConfigurationData;

           

            IActorRef produce = null;
            while (produce == null)
            {
                Producers.TryGetValue(topic, out produce);
                Thread.Sleep(100);
            }
            Console.WriteLine($"Acquired producer for topic: {topic}");
            pulsarSystem.CreateConsumer(new CreateConsumer(jsonSchem, consumerConfig, ConsumerType.Single));
            //pulsarSystem.CreateReader(new CreateReader(jsonSchema, readerConfig));
            //pulsarSystem.BatchSend(new BatchSend(new List<object>{ new Foo() }, "Test"));

            while (true)
            {
                var read = Console.ReadLine();
                if (read == "s")
                {
                    var sends = new List<Send>();
                    for (var i = 0; i < 25; i++)
                    {
                        var student = new Students
                        {
                            Name = $"Ebere: {DateTimeOffset.Now.ToUnixTimeMilliseconds()} - presto-ed {DateTime.Now.ToString(CultureInfo.InvariantCulture)}",
                            Age = 2019+i,
                            School = "Akka-Pulsar university"
                        };
                        //var s = JsonSerializer.Serialize(student);
                        sends.Add(new Send(student, topic, ImmutableDictionary<string, object>.Empty, $"{DateTimeOffset.Now.ToUnixTimeMilliseconds()}"));
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
