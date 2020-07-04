using System;
using SharpPulsar.Akka;
using SharpPulsar.Akka.Configuration;
using SharpPulsar.Akka.InternalCommands.Consumer;
using SharpPulsar.Akka.InternalCommands.Producer;
using SharpPulsar.Akka.Network;
using SharpPulsar.Api;
using SharpPulsar.Handlers;
using SharpPulsar.Impl.Conf;
using SharpPulsar.Protocol.Proto;
using Xunit.Abstractions;

namespace SharpPulsar.Test.TestCommon
{
    public class Common
    {
        private readonly ITestOutputHelper _output;

        public Common(ITestOutputHelper output)
        {
            _output = output;
        }

        public void GetPulsarSystem(IAuthentication auth, int operationTime = 0, bool useProxy = false)
        {
            var builder = new PulsarClientConfigBuilder()
                .ServiceUrl("pulsar://localhost:6650")
                .ConnectionsPerBroker(1)
                .UseProxy(useProxy)
                .StatsInterval(0)
                .Authentication(auth)
                .AllowTlsInsecureConnection(true)
                .EnableTls(true);
            if (operationTime > 0)
                builder.OperationTimeout(operationTime);

            var clientConfig = builder.ClientConfigurationData;

            PulsarSystem = PulsarSystem.GetInstance(clientConfig);
        }

        public CreateProducer CreateProducer(ISchema schema, string topic, string producername, int compression = 0, long batchMessageDelayMs = 0, int batchingMaxMessages = 5)
        {
            var producerListener = new DefaultProducerListener((o) =>
            {
                _output.WriteLine(o.ToString());
            }, s =>
            {
                _output.WriteLine(s);
            });
            var builder = new ProducerConfigBuilder()
                .ProducerName(producername)
                .Topic(topic)
                .Schema(schema)
                .EventListener(producerListener);
            if (compression > 0)
                builder.CompressionType((ICompressionType)Enum.GetValues(typeof(ICompressionType)).GetValue(compression));
            if (batchMessageDelayMs != 0)
            {
                builder.EnableBatching(true);
                builder.BatchingMaxPublishDelay(batchMessageDelayMs);
                builder.BatchingMaxMessages(5);
            }
            var producerConfig = builder.ProducerConfigurationData;


            return new CreateProducer(schema, producerConfig);
        }

        public CreateConsumer CreateConsumer(ISchema schema, string topic, string consumername, string subscription,int compression = 0, bool forceTopic = false, int ackTimeout = 0)
        {
            var consumerListener = new DefaultConsumerEventListener(l => _output.WriteLine(l.ToString()));
            var messageListener = new DefaultMessageListener(null, null);
            var consumerC = new ConsumerConfigBuilder()
                .ConsumerName(consumername)
                .ForceTopicCreation(forceTopic)
                .SubscriptionName(subscription)
                .Topic(topic)
                .SetConsumptionType(ConsumptionType.Queue)
                .ConsumerEventListener(consumerListener)
                .SubscriptionType(CommandSubscribe.SubType.Exclusive)
                .Schema(schema)
                .MessageListener(messageListener)
                .SubscriptionInitialPosition(SubscriptionInitialPosition.Earliest);
            if (ackTimeout > 0)
                consumerC.AckTimeout(ackTimeout);
            var consumerConfig = consumerC.ConsumerConfigurationData;
            return new CreateConsumer(schema, consumerConfig, ConsumerType.Single);
        }

        public PulsarSystem PulsarSystem { get; set; }

    }
}
