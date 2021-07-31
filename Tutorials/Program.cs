using System;
using System.Buffers;
using System.Collections.Generic;
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
                .ServiceUrl("pulsar://localhost:6650");
            if (cmd.Equals("txn", StringComparison.OrdinalIgnoreCase))
                clientConfig.EnableTransaction(true);

            //pulsar actor system
            var pulsarSystem = PulsarSystem.GetInstance(clientConfig);

            var pulsarClient = pulsarSystem.NewClient();
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
    }
}
