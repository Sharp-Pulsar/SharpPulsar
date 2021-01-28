
using SharpPulsar.Interfaces;
using System.Collections.Concurrent;

namespace SharpPulsar.Queues
{
    public class ProducerQueueCollection
    {
        public BlockingCollection<IMessageId> MessageId { get; } = new BlockingCollection<IMessageId>();
    }
}
