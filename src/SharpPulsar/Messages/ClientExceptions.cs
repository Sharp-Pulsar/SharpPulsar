using SharpPulsar.Exceptions;

namespace SharpPulsar.Messages
{
    public sealed class ClientExceptions
    {
        public PulsarClientException Exception { get; }
        public ClientExceptions(PulsarClientException exception)
        {
            Exception = exception;
        }
    }
}
