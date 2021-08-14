using Akka.Actor;


namespace SharpPulsar.Messages.Client
{
    public sealed class AddProducer
    {
        public IActorRef Producer { get; }
        public AddProducer(IActorRef producer)
        {
            Producer = producer;
        }
    }
}
