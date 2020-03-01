using Akka.Actor;
using SharpPulsar.Akka.InternalCommands.Consumer;
using SharpPulsar.Api;
using SharpPulsar.Impl.Conf;

namespace SharpPulsar.Akka.Consumer
{   /// <summary>
    /// Agregates consumers
    /// </summary>
    public class MultiTopicsManager:ReceiveActor
    {
        private long _consumerid;
        private IMessageListener _listener;
        private IConsumerEventListener _event;
        private bool _hasParentConsumer;
        public MultiTopicsManager(ClientConfigurationData clientConfiguration, ConsumerConfigurationData configuration, IActorRef network, bool hasParentConsumer)
        {
            _hasParentConsumer = hasParentConsumer;
            _listener = configuration.MessageListener;
            _event = configuration.ConsumerEventListener;
            foreach (var topic in configuration.TopicNames)
            {
                Context.ActorOf(Consumer.Prop(clientConfiguration, topic, configuration, _consumerid++, network, true));
            }

            Receive<ConsumedMessage>(m =>
            {
                if(_hasParentConsumer)
                    Context.Parent.Tell(m);
                else
                    _listener.Received(m.Consumer, m.Message);
            });
            ReceiveAny(a =>
            {
                _event.Log($"{a.GetType()}, unhandled");
            });
        }

        public static Props Prop(ClientConfigurationData clientConfiguration, ConsumerConfigurationData configuration, IActorRef network, bool hasParentConsumer)
        {
            return Props.Create(()=> new MultiTopicsManager(clientConfiguration, configuration, network, hasParentConsumer));
        }
    }
}
