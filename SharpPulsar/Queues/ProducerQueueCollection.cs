using SharpPulsar.Messages;
using SharpPulsar.Messages.Producer;
using System.Collections.Concurrent;

namespace SharpPulsar.Queues
{
    public class ProducerQueueCollection<T>
    {
        public BlockingCollection<AckReceived> Receipt { get; }
        public BlockingCollection<ProducerCreation> Producer { get; }
        public BlockingCollection<ProducerCreation> PartitionedProducer { get; }
        public ProducerQueueCollection()
        {
            Receipt = new BlockingCollection<AckReceived>();
            Producer = new BlockingCollection<ProducerCreation>();
            PartitionedProducer = new BlockingCollection<ProducerCreation>();
        }
    }
}
