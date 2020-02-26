using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Akka.Actor;
using Avro;
using SharpPulsar.Akka.Producer;
using SharpPulsar.Api;
using SharpPulsar.Common.Schema;
using SharpPulsar.Impl;
using SharpPulsar.Impl.Conf;
using SharpPulsar.Shared;

namespace SharpPulsar.Akka
{
    public class PulsarSystem: IAsyncDisposable
    {
        private ActorSystem _actorSystem;
        private IPulsarClientBuilder _clientBuilder;
        private ClientConfigurationData _conf;
        private IPulsarClient _client;

        public PulsarSystem(IPulsarClientBuilder clientBuilder, ClientConfigurationData conf)
        {
            _actorSystem = ActorSystem.Create("Pulsar");
            _conf = conf;
            _clientBuilder = clientBuilder;
        }

        public IProducerBuilder<T> GetProducerBuilder<T>(ISchema<T> schema)
        {
            return _clientBuilder.Build().NewProducer(schema);
        }

        public IConsumerBuilder<T> GetConsumerBuilder<T>(ISchema<T> schema)
        {
            return _clientBuilder.Build().NewConsumer(schema);
        }
        //constructor producer manager actor
        public IActorRef StartProducer<T>(IProducerBuilder<T> producer, ProducerConfigurationData conf)
        {
            var serviceNameResolver = new PulsarServiceNameResolver();
            serviceNameResolver.UpdateServiceUrl(_conf.ServiceUrl);
            var client = _clientBuilder.Build();
            return _actorSystem.ActorOf(ProducerManager<T>.Prop(producer, conf));
        }
        //constructor consumer manager actor
        public void StartConsumer<T>(IConsumerBuilder<T> consumer)
        {
            consumer.Subscribe();
        }

        public void Produce<T>(T message, IActorRef manager)
        {
            var m = new { message, message.GetType()};
            manager.Tell(m);
        }
        //Multiple producer scenario
        public void Produce<T>(T message, string topic)
        {

        }
        public void ReadMessages()
        {
            //read messages from IMessageReceivedHandler handler
        }
        public async ValueTask DisposeAsync()
        {
           await _actorSystem.Terminate();
        }
    }
}
