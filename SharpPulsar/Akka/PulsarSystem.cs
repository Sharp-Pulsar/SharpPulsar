using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Akka.Actor;
using SharpPulsar.Akka.InternalCommands;
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
        
        
        public void CreateProducer(NewProducer producer)
        {
            _pulsarManager.Tell(producer);
        }
        
        public void CreateConsumer(NewConsumer consumer)
        {
            _pulsarManager.Tell(consumer);
        }

        public void Send(Send send)
        {
           _pulsarManager.Tell(send);
        }
        
        public async ValueTask DisposeAsync()
        {
           await _actorSystem.Terminate();
        }
    }
}
