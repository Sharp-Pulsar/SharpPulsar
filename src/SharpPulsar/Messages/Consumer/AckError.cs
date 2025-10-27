using SharpPulsar.Shared.Exceptions;

namespace SharpPulsar.Messages.Consumer
{
    public record AckError(long RequestId, PulsarClientException Exception);
    
}
