
using SharpPulsar.Exceptions;

namespace SharpPulsar.Messages.Consumer
{
    public sealed class AckError
    {
       public long RequestId { get; }
        public PulsarClientException Exception { get; }
        public AckError(long requestid, PulsarClientException exception)
        {
            RequestId = requestid;
            Exception = exception;
        }
    }
}
