
namespace SharpPulsar.Messages.Producer
{
    /// <summary>
    /// When sending large messages that needs to be chunked or when batching is enable
    /// NoMessageIdYet will be returned because we do not want to block while chunking or batching.
    /// Use 'GetReceivedAcks' to get queued up acks with which you can construct MessageId at the calling end.
    /// </summary>
    public sealed class NoMessageIdYet
    {
        public static NoMessageIdYet Instance = new NoMessageIdYet();
    }
}
