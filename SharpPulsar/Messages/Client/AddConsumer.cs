using Akka.Actor;


namespace SharpPulsar.Messages.Client
{
    public sealed class AddConsumer
    {
        public IActorRef Consumer { get; }
        public AddConsumer(IActorRef consumer)
        {
            Consumer = consumer;
        }
    }
}
