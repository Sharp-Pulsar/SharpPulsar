namespace SharpPulsar.Akka.InternalCommands
{
    public class PulsarResponse
    {
        public object Message { get; }

        public PulsarResponse(object message)
        {
            Message = message;
        }
    }
}
