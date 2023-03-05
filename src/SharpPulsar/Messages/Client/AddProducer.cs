using Akka.Actor;


namespace SharpPulsar.Messages.Client
{
     public record struct AddProducer(IActorRef Producer);
}
