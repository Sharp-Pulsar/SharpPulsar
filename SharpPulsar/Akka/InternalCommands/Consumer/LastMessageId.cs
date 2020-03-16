
namespace SharpPulsar.Akka.InternalCommands.Consumer
{
    public sealed class LastMessageId
    {
    }
    public sealed class LastMessageIdResponse
    {
        public LastMessageIdResponse(long ledgerId, long entryId, int partition, int batchIndex)
        {
            LedgerId = ledgerId;
            EntryId = entryId;
            Partition = partition;
            BatchIndex = batchIndex;
        }

        public long LedgerId { get; }
        public long EntryId { get; }
        public int Partition { get; }
        public int BatchIndex { get; }
    }

    public sealed class LastMessageIdReceived
    {
        public LastMessageIdReceived(long consumerId, string topic, LastMessageIdResponse response)
        {
            ConsumerId = consumerId;
            Topic = topic;
            Response = response;
        }

        public LastMessageIdResponse Response { get; }
        public string Topic { get; }
        public long ConsumerId { get; }
    }
}
