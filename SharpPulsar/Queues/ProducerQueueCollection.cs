using SharpPulsar.Messages.Producer;
using System.Collections.Concurrent;

namespace SharpPulsar.Queues
{
    public class ProducerQueueCollection<T>
    {
        public BlockingCollection<SentMessage<T>> SentMessage { get; }
        public BlockingCollection<ProducerCreation> Producer { get; }
        public BlockingCollection<ProducerCreation> PartitionedProducer { get; }
        public ProducerQueueCollection()
        {
            SentMessage = new BlockingCollection<SentMessage<T>>();
            Producer = new BlockingCollection<ProducerCreation>();
            PartitionedProducer = new BlockingCollection<ProducerCreation>();
        }
    }
}
