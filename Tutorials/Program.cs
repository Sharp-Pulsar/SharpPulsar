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

            var consumer = pulsarClient.NewConsumer(new ConsumerConfigBuilder<sbyte[]>()
                .Topic(myTopic)
                .ForceTopicCreation(true)
                .SubscriptionName("myTopic-sub"));

            var producer = pulsarClient.NewProducer(new ProducerConfigBuilder<sbyte[]>()
                .Topic(myTopic));

            for (var i = 0; i < 10; i++)
            {
                var data = (sbyte[])(object)Encoding.UTF8.GetBytes($"tuts-{i}");
                producer.NewMessage().Value(data).Send();
            }
            for (var i = 0; i < 10; i++)
            {
                var message = (Message<sbyte[]>)consumer.Receive(TimeSpan.FromSeconds(30));
                consumer.Acknowledge(message);
                var res = Encoding.UTF8.GetString((byte[])(object)message.Data);
                Console.WriteLine($"message '{res}' from topic: {message.TopicName}");
            }
        }
    }
}
