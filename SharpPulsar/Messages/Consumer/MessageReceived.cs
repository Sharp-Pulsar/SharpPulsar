using System.Buffers;
using SharpPulsar.Protocol.Proto;

namespace SharpPulsar.Messages.Consumer
{
    public sealed class MessageReceived
    {
        public MessageReceived(MessageMetadata metadata, BrokerEntryMetadata brokerEntryMetadata, ReadOnlySequence<byte> payload, MessageIdData messageId, int redeliveryCount, bool hasValidCheckSum, bool hasMagicNumber, long consumerEpoch, bool hasConsumerEpoch)
        {
            MessageId = messageId;
            Payload = payload;
            RedeliveryCount = redeliveryCount;
            Metadata = metadata;
            HasValidCheckSum = hasValidCheckSum;
            HasMagicNumber = hasMagicNumber;
            BrokerEntryMetadata = brokerEntryMetadata;
            ConsumerEpoch = consumerEpoch;
            HasConsumerEpoch = hasConsumerEpoch;    
        }
        public BrokerEntryMetadata BrokerEntryMetadata { get; }
        public MessageMetadata Metadata { get; }
        public MessageIdData MessageId { get; }
        public ReadOnlySequence<byte> Payload { get; }
        public int RedeliveryCount { get; }
        public long ConsumerEpoch { get; }
        public bool HasConsumerEpoch { get; }
        public bool HasValidCheckSum { get; }
        public bool HasMagicNumber { get; }
    }
}
