using Akka.Actor;
using SharpPulsar.Impl;
using SharpPulsar.Impl.Conf;

namespace SharpPulsar.Akka.Producer
{
    public class Producer: ReceiveActor
    {
        private IActorRef _broker;

        public Producer(ProducerConfigurationData configuration, long producerid)
        {
            
        }

        public static Props Prop(ProducerConfigurationData configuration, long producerid)
        {
            return Props.Create(()=> new Producer(configuration, producerid));
        }
    }
}
