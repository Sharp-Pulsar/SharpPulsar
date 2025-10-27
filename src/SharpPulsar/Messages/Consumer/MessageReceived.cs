using System.Buffers;
using DotNetty.Buffers;
using Pulsar.Proto;

namespace SharpPulsar.Messages.Consumer
{
    public record MessageReceived(MessageMetadata Metadata, BrokerEntryMetadata BrokerEntryMetadata, AbstractByteBuffer Payload, MessageIdData MessageId, int RedeliveryCount, bool HasValidCheckSum, bool HasMagicNumber, long ConsumerEpoch, bool HasConsumerEpoch);
}
