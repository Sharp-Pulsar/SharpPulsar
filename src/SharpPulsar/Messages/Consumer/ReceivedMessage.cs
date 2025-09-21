namespace SharpPulsar.Messages.Consumer
{
    public record struct ReceivedMessage<T>(IMessage<T> Message);
}
