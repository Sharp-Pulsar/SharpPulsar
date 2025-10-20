namespace SharpPulsar.Messages
{
    public record PulsarResponse
    {
        public object Message { get; }

        public PulsarResponse(object message)
        {
            Message = message;
        }
    }
}
