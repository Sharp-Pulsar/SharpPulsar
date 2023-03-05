namespace SharpPulsar.Messages
{
    public readonly record struct PulsarResponse
    {
        public object Message { get; }

        public PulsarResponse(object message)
        {
            Message = message;
        }
    }
}
