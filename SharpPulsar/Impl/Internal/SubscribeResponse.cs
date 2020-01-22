namespace SharpPulsar.Impl.Internal
{
    public sealed class SubscribeResponse
    {
        public SubscribeResponse(ulong consumerId) => ConsumerId = consumerId;

        public ulong ConsumerId { get; }
    }
}
