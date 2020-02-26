using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Akka.Actor;
using SharpPulsar.Akka.Consumer;
using SharpPulsar.Akka.InternalCommands;
using SharpPulsar.Akka.Producer;
using SharpPulsar.Api;
using SharpPulsar.Impl.Conf;

namespace SharpPulsar.Akka
{
    public class PulsarSystem: IAsyncDisposable
    {
        private ActorSystem _actorSystem;
        private IPulsarClientBuilder _clientBuilder;
        private ClientConfigurationData _conf;
        private IPulsarClient _client;
        private bool _producerManagerCreated;
        private IActorRef _producerManager;
        private bool _consumerManagerCreated;
        private IActorRef _consumerManager;

        public PulsarSystem(IPulsarClientBuilder clientBuilder, ClientConfigurationData conf)
        {
            _actorSystem = ActorSystem.Create("Pulsar");
            _conf = conf;
            _clientBuilder = clientBuilder;
        }
        public void StartProducerManager<T>(ISchema<T> schema)
        {
            if (!_producerManagerCreated)
            {
                _producerManager =_actorSystem.ActorOf(ProducerManager<T>.Prop(), "ProducerManager");
                _producerManagerCreated = true;
            }
        }
        public void StartConsumerManager<T>(ISchema<T> schema)
        {
            if (!_consumerManagerCreated)
            {
                _consumerManager = _actorSystem.ActorOf(ConsumerManager<T>.Prop(), "ConsumerManager");
                _consumerManagerCreated = true;
            }
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
        public void CreateProducer<T>(Create<T> create)
        {
            if(_producerManagerCreated)
                _producerManager.Tell(create);
        }
        //constructor consumer manager actor
        public void SubscribeConsumer<T>(Subscribe<T> subscribe)
        {
            if(_consumerManagerCreated)
                _consumerManager.Tell(subscribe);
        }

        public void Send(Send send)
        {
           _producerManager.Tell(send);
        }
        public void Send(List<object> messages)
        {
            var tran = new Transactional(messages.ToImmutableList(), _clientBuilder.Build());
            _producerManager.Tell(tran);
        }
        public async ValueTask DisposeAsync()
        {
           await _actorSystem.Terminate();
        }
    }
}
