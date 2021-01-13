using Akka.Actor;

namespace SharpPulsar.Messages
{
    public sealed class RegisterProducer
    {
        public long Id { get; }
        public IActorRef Producer { get; }
        public RegisterProducer(long id, IActorRef producer)
        {
            Id = id;
            Producer = producer;
        }
    }
    public sealed class RegisterConsumer
    {
        public long Id { get; }
        public IActorRef Consumer { get; }
        public RegisterConsumer(long id, IActorRef consumer)
        {
            Id = id;
            Consumer = consumer;
        }
    }
    public sealed class RegisterTransactionMetaStoreHandler
    {
        public long Id { get; }
        public IActorRef Handler { get; }
        public RegisterTransactionMetaStoreHandler(long id, IActorRef handler)
        {
            Id = id;
            Handler = handler;
        }
    }
}
