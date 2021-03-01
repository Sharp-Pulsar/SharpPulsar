using System;
using System.Collections.Concurrent;

namespace SharpPulsar.Queues
{
    public class TransactionQueueCollection
    {
        public BlockingCollection<Exception> Exceptions { get; }
        public TransactionQueueCollection()
        {
            Exceptions = new BlockingCollection<Exception>();
        }
    }
}
