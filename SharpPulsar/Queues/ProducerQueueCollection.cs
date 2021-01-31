
using SharpPulsar.Interfaces;
using SharpPulsar.Messages;
using System.Collections.Concurrent;

namespace SharpPulsar.Queues
{
    public class ProducerQueueCollection
    {
        public BlockingCollection<IMessageId> MessageId { get; } = new BlockingCollection<IMessageId>();
        public BlockingCollection<object> CreateProducer { get; } = new BlockingCollection<object>();
    }
}
