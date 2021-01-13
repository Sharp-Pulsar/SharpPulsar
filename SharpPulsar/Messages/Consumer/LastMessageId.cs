
namespace SharpPulsar.Messages.Consumer
{
    public sealed class LastMessageId
    {
    }
    public sealed class LastMessageIdResponse
    {
        public LastMessageIdResponse(long ledgerId, long entryId, int partition, int batchIndex, int batchSize, long[] ackSets)
        {
            LedgerId = ledgerId;
            EntryId = entryId;
            Partition = partition;
            BatchIndex = batchIndex;
            BatchSize = batchSize;
            AckSets = ackSets;
        }

        public long LedgerId { get; }
        public long EntryId { get; }
        public int Partition { get; }
        public int BatchIndex { get; }
        public int BatchSize { get; }
        public long[] AckSets { get; }

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
