namespace SharpPulsar.Messages.Consumer
{
    public record struct AckError(long RequestId, PulsarClientException Exception);
    
}
