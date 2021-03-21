using Akka.Actor;

namespace SharpPulsar.FSM
{
    public sealed class DTO
    {
        public IActorRef Sender { get; }
        public object Message { get; set; }
        public DTO(IActorRef sender, object message)
        {
            Sender = sender;
            Message = message;
        }
    }
}
