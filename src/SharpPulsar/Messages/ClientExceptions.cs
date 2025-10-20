namespace SharpPulsar.Messages
{
    public record ClientExceptions
    {
        public PulsarClientException Exception { get; }
        public ClientExceptions(PulsarClientException exception)
        {
            Exception = exception;
        }
    }
}
