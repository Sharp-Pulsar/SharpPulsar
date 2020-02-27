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
        private IActorRef _pulsarManager;

        public PulsarSystem(ClientConfigurationData conf)
        {
            _actorSystem = ActorSystem.Create("Pulsar");
            _pulsarManager = _actorSystem.ActorOf(PulsarManager.Prop(conf), "PulsarManager");
        }
        public void StartProducerManager(ISchema schema)
        {
            if (!_producerManagerCreated)
            {
                _producerManager =_actorSystem.ActorOf(ProducerManager.Prop(), "ProducerManager");
                _producerManagerCreated = true;
            }
        }
        public void StartConsumerManager<T>(ISchema schema)
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
