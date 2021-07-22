using System;
using System.Buffers;
using System.Text;
using System.Threading;
using SharpPulsar;
using SharpPulsar.Configuration;
using SharpPulsar.User;

namespace Tutorials
{
    //https://helm.kafkaesque.io/#accessing-the-pulsar-cluster-on-localhost
    class Program
    {
        //static string myTopic = $"persistent://public/default/mytopic-2";
        static string myTopic = $"persistent://public/default/mytopic-{Guid.NewGuid()}";
        static void Main(string[] args)
        {
            //pulsar client settings builder
            Console.WriteLine("Please enter cmd");
            var cmd = Console.ReadLine();
            var clientConfig = new PulsarClientConfigBuilder()
                //.ProxyServiceUrl("pulsar+ssl://pulsar-azure-westus2.streaming.datastax.com:6551", SharpPulsar.Common.ProxyProtocol.SNI)
                .ServiceUrl("pulsar://localhost:6650");
                //.ServiceUrl("pulsar+ssl://pulsar-azure-westus2.streaming.datastax.com:6551")
                //.Authentication(new AuthenticationToken("eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJjbGllbnQ7NjBlZTBjZGUtOWU3Zi00MzJmLTk3ZmEtYzA1NGFhYjgwODQwO2JXVnpkR2xqWVd3PSJ9.tIi1s_PQ6AZNxBr9rEqCoPk34mG-M6BXL8DwZpgh1Yr_nR4S5UpHG79_aYTvz902GA8PB8R7DJw6qknSdlCdXhPiDsxSPPnx4sylFz9QFCvvY6Q1JE38jVJDMMvHnm-_rggGkCPk_MFZxh1yxtamQ_QYcZQa-aq3rvFZmcJbG_jtcfmOT3TEZradN7FOiztfJDRP0YLVgnh0CJFxC36C0S4UaORllQN11i0KIgasF5dbLSidt70nwNgt6PZHDykEdhV6OC473U_4y7rM0gX7SNR3IiFMsAb7jD4CKIGG876J20aU67jXkHj08-QW2Ut38rJi5u4WxKgNTTfWOBSlQQ"));
            if (cmd.Equals("txn", StringComparison.OrdinalIgnoreCase))
                clientConfig.EnableTransaction(true);

            //pulsar actor system
            var pulsarSystem = PulsarSystem.GetInstance(clientConfig);

            var pulsarClient = pulsarSystem.NewClient();
            if (cmd.Equals("txn", StringComparison.OrdinalIgnoreCase))
                Transaction(pulsarClient);
            else if (cmd.Equals("exc", StringComparison.OrdinalIgnoreCase))
                ExclusiveProduceConsumer(pulsarClient);
            else if (cmd.Equals("exc2", StringComparison.OrdinalIgnoreCase))
                ExclusiveProduceNoneConsumer(pulsarClient);
            else if (cmd.Equals("bat", StringComparison.OrdinalIgnoreCase))
                BatchProduceConsumer(pulsarClient);
            else
                ProduceConsumer(pulsarClient);

            Console.ReadKey();
        }
        private static void ProduceConsumer(PulsarClient pulsarClient)
        {
            var producer = pulsarClient.NewProducer(new ProducerConfigBuilder<byte[]>()
                .Topic(myTopic));
            

            for (var i = 0; i < 100; i++)
            {
                var data = Encoding.UTF8.GetBytes($"tuts-{i}");
                var id = producer.NewMessage().Value(data).Send();
                Console.WriteLine($"Message Id({id.LedgerId}:{id.EntryId})");
            }

            var pool = ArrayPool<byte>.Shared;
            var consumer = pulsarClient.NewConsumer(new ConsumerConfigBuilder<byte[]>()
                .Topic(myTopic)
                .ForceTopicCreation(true)
                .SubscriptionName($"myTopic-sub-{Guid.NewGuid()}")
                .ReceiverQueueSize(4)
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
        private static void BatchProduceConsumer(PulsarClient pulsarClient)
        {
            var producer = pulsarClient.NewProducer(new ProducerConfigBuilder<byte[]>()
                .Topic(myTopic)
                .EnableBatching(true)
                .BatchingMaxMessages(50));
            

            for (var i = 0; i < 50; i++)
            {
                var data = Encoding.UTF8.GetBytes($"batched-tuts-{i}");
                var noId = producer.NewMessage().Value(data).Send();
                Console.WriteLine(i.ToString());
            }

            var pool = ArrayPool<byte>.Shared;
            var consumer = pulsarClient.NewConsumer(new ConsumerConfigBuilder<byte[]>()
                .Topic(myTopic)
                .ForceTopicCreation(true)
                .SubscriptionName($"myTopic-sub-{Guid.NewGuid()}")
                .SubscriptionInitialPosition(SharpPulsar.Common.SubscriptionInitialPosition.Earliest));

            Thread.Sleep(TimeSpan.FromSeconds(5));

            for (var i = 0; i < 50; i++)
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

            for (var i = 0; i < 10; i++)
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

            for (var i = 0; i < 10; i++)
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
            

            for (var i = 0; i < 10; i++)
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

            for (var i = 0; i < 10; i++)
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
    }
}
