using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using SharpPulsar.Akka;
using SharpPulsar.Akka.Configuration;
using SharpPulsar.Messages.Consumer;
using SharpPulsar.Messages.Producer;
using SharpPulsar.Akka.Network;
using SharpPulsar.Api;
using SharpPulsar.Batch.Api;
using SharpPulsar.Handlers;
using SharpPulsar.Configuration;
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

        public void GetPulsarSystem(IAuthentication auth, int operationTime = 0, bool useProxy = false, string authPath = "", bool enableTls = false, bool allowTlsInsecureConnection = false, string brokerService = "")
        {
            var builder = new PulsarClientConfigBuilder()
                .ServiceUrl(!string.IsNullOrWhiteSpace(brokerService)?brokerService: "pulsar://localhost:6650")
                .ConnectionsPerBroker(1)
                .UseProxy(useProxy)
                .StatsInterval(0)
                .Authentication(auth)
                .AllowTlsInsecureConnection(allowTlsInsecureConnection)
                .EnableTls(enableTls);
            if (!string.IsNullOrWhiteSpace(authPath))
                builder.AddTrustedAuthCert(new X509Certificate2(File.ReadAllBytes(authPath)));
            if (operationTime > 0)
                builder.OperationTimeout(operationTime);

            var clientConfig = builder.ClientConfigurationData;

            PulsarSystem = PulsarSystem.GetInstance(clientConfig);
        }

        public CreateProducer CreateProducer(ISchema schema, string topic, string producername, int compression = 0, long batchMessageDelayMs = 0, int batchingMaxMessages = 5, IBatcherBuilder batcherBuilder = null, int maxMessageSize = 0, bool enableChunking = false, bool enableBatching = false)
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
                .EventListener(producerListener)
                .EnableChunking(enableChunking);
            if (compression > 0)
                builder.CompressionType((ICompressionType)Enum.GetValues(typeof(ICompressionType)).GetValue(compression));
            if (enableBatching)
            {
                builder.EnableBatching(true);
                builder.BatchingMaxPublishDelay(batchMessageDelayMs);
                builder.BatchingMaxMessages(batchingMaxMessages);
                if (batcherBuilder != null)
                    builder.BatchBuilder(batcherBuilder);

            }

            if (maxMessageSize > 0)
                builder.MaxMessageSize(maxMessageSize);
            var producerConfig = builder.ProducerConfigurationData;


            return new CreateProducer(schema, producerConfig);
        }

        public CreateConsumer CreateConsumer(ISchema schema, string topic, string consumername, string subscription,int compression = 0, bool forceTopic = false, int ackTimeout = 0, KeySharedPolicy keySharedPolicy = null, CommandSubscribe.SubType subType = CommandSubscribe.SubType.Exclusive, int acknowledgmentGroupTime = -1, long negativeAckRedeliveryDelay = 0)
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
                .SubscriptionType(subType)
                .Schema(schema)
                .MessageListener(messageListener)
                .SubscriptionInitialPosition(SubscriptionInitialPosition.Earliest);
            if (ackTimeout > 0)
                consumerC.AckTimeout(ackTimeout);
            if (keySharedPolicy != null)
                consumerC.KeySharedPolicy(keySharedPolicy);
            if (acknowledgmentGroupTime > -1)
                consumerC.AcknowledgmentGroupTime(acknowledgmentGroupTime);
            if (negativeAckRedeliveryDelay > 0)
                consumerC.NegativeAckRedeliveryDelay(negativeAckRedeliveryDelay);

            var consumerConfig = consumerC.ConsumerConfigurationData;
            return new CreateConsumer(schema, consumerConfig);
        }

        public PulsarSystem PulsarSystem { get; set; }

    }
}
