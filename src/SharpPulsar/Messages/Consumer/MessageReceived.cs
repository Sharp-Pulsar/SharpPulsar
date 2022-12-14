using System.Buffers;
using SharpPulsar.Protocol.Proto;

namespace SharpPulsar.Messages.Consumer
{
    public record struct MessageReceived(MessageMetadata Metadata, BrokerEntryMetadata BrokerEntryMetadata, ReadOnlySequence<byte> Payload, MessageIdData MessageId, int RedeliveryCount, bool HasValidCheckSum, bool HasMagicNumber, long ConsumerEpoch, bool HasConsumerEpoch);
}
