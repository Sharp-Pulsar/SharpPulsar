
namespace SharpPulsar.Messages
{
    public class Partitions
    {
        public Partitions(int partition, long requestId, string topic = "")
        {
            Partition = partition;
            RequestId = requestId;
            Topic = topic;
        }
        public long RequestId { get; }
        public int Partition { get; }
        public string Topic { get; }
    }
}
