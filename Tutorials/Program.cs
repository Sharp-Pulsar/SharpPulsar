using System;
using System.Text;
using SharpPulsar;
using SharpPulsar.Configuration;
using SharpPulsar.Extension;

namespace Tutorials
{
    class Program
    {
        const string myTopic = "persistent://public/default/mytopic";
        static void Main(string[] args)
        {
            //pulsar client settings builder
            var clientConfig = new PulsarClientConfigBuilder()
                .ServiceUrl("pulsar://localhost:6650");

            //pulsar actor system
            var pulsarSystem = PulsarSystem.GetInstance(clientConfig);

            var pulsarClient = pulsarSystem.NewClient();



            var producer = pulsarClient.NewProducer(new ProducerConfigBuilder<byte[]>()
                .Topic(myTopic));

            var consumer = pulsarClient.NewConsumer(new ConsumerConfigBuilder<byte[]>()
                .Topic(myTopic)
                .ForceTopicCreation(true)
                .SubscriptionName("myTopic-sub"));

            for (var i = 0; i < 10; i++)
            {
                var data = Encoding.UTF8.GetBytes($"tuts-{i}");
                producer.NewMessage().Value(data).Send();
            }
            for (var i = 0; i < 10; i++)
            {
                var message = (Message<byte[]>)consumer.Receive(TimeSpan.FromSeconds(30));
                consumer.Acknowledge(message);
                var res = Encoding.UTF8.GetString(message.Data);
                Console.WriteLine($"message '{res}' from topic: {message.TopicName}");
            }
        }
    }
}
