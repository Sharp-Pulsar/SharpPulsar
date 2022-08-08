namespace SharpPulsar.Messages
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
