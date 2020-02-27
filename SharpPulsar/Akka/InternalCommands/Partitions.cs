
namespace SharpPulsar.Akka.InternalCommands
{
    public class Partitions
    {
        public Partitions(int partition, long requestId)
        {
            Partition = partition;
            RequestId = requestId;
        }
        public long RequestId { get; }
        public int Partition { get; }
    }
}
