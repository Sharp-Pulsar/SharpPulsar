using Akka.Actor;


namespace SharpPulsar.Messages.Client
{
    public record struct AddConsumer(IActorRef Consumer);
}
