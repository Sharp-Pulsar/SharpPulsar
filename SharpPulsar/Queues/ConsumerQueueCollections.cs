
using SharpPulsar.Interfaces;
using SharpPulsar.Messages;
using SharpPulsar.Stats.Consumer.Api;
using System.Collections.Concurrent;
using System.Threading.Tasks.Dataflow;

namespace SharpPulsar.Queues
{
    public class ConsumerQueueCollections<T>
    {
        public BufferBlock<IMessage<T>> IncomingMessages { get; }
        public BlockingCollection<ClientExceptions> AcknowledgeException { get; } 
        public BlockingCollection<ClientExceptions> NegativeAcknowledgeException { get; }
        public BlockingCollection<ClientExceptions> ReconsumeLaterException { get; }
        public BlockingCollection<ClientExceptions> RedeliverUnacknowledgedException { get; }
        public BlockingCollection<ClientExceptions> UnsubscribeException { get; }
        public BlockingCollection<ClientExceptions> SeekException { get; }
        public BlockingCollection<ClientExceptions> AcknowledgeCumulativeException { get; }
        public BlockingCollection<string> ConsumerName { get; }
        public BlockingCollection<ClientExceptions> ConsumerCreation { get; }
        public BlockingCollection<string> Subscription { get; }
        public BlockingCollection<string> Topic { get; }
        public BlockingCollection<long> LastDisconnectedTimestamp { get; }
        public BlockingCollection<bool> HasReachedEndOfTopic { get; }
        public BlockingCollection<bool> HasMessageAvailable { get; }
        public BlockingCollection<bool> Connected { get; }
        public BlockingCollection<IMessageId> LastMessageId { get; }
        public BlockingCollection<IConsumerStats> Stats { get; }

        public ConsumerQueueCollections()
        {
            IncomingMessages = new BufferBlock<IMessage<T>>();
            AcknowledgeException = new BlockingCollection<ClientExceptions>();
            NegativeAcknowledgeException = new BlockingCollection<ClientExceptions>();
            ReconsumeLaterException = new BlockingCollection<ClientExceptions>();
            RedeliverUnacknowledgedException = new BlockingCollection<ClientExceptions>();
            UnsubscribeException = new BlockingCollection<ClientExceptions>();
            SeekException = new BlockingCollection<ClientExceptions>();
            AcknowledgeCumulativeException = new BlockingCollection<ClientExceptions>();
            ConsumerName = new BlockingCollection<string>();
            ConsumerCreation = new BlockingCollection<ClientExceptions>();
            Subscription = new BlockingCollection<string>();
            Topic = new BlockingCollection<string>();
            LastDisconnectedTimestamp = new BlockingCollection<long>();
            HasReachedEndOfTopic = new BlockingCollection<bool>();
            HasMessageAvailable = new BlockingCollection<bool>();
            Connected = new BlockingCollection<bool>();
            LastMessageId = new BlockingCollection<IMessageId>();
            Stats = new BlockingCollection<IConsumerStats>();
        }
    }
}
