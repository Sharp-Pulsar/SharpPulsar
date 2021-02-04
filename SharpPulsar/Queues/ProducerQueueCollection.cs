using SharpPulsar.Messages.Producer;
using System.Collections.Concurrent;

namespace SharpPulsar.Queues
{
    public class ProducerQueueCollection<T>
    {
        public BlockingCollection<SentMessage<T>> SentMessage { get; } = new BlockingCollection<SentMessage<T>>();
        public BlockingCollection<ProducerCreation> Producer { get; } = new BlockingCollection<ProducerCreation>();
        public BlockingCollection<ProducerCreation> PartitionedProducer { get; } = new BlockingCollection<ProducerCreation>();
    }
}
