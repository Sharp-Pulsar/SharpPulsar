using SharpPulsar.Exceptions;

namespace SharpPulsar.Messages
{
    public readonly record struct ClientExceptions
    {
        public PulsarClientException Exception { get; }
        public ClientExceptions(PulsarClientException exception)
        {
            Exception = exception;
        }
    }
}
