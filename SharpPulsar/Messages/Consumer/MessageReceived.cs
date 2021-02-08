using SharpPulsar.Protocol.Proto;
using System.Buffers;

namespace SharpPulsar.Messages.Consumer
{
    public class MessageReceived
    {
        public MessageReceived(ReadOnlySequence<byte> data, MessageIdData messageId, int redeliveryCount)
        {
            MessageId = messageId;
            Data = data;
            RedeliveryCount = redeliveryCount;
        }

        public MessageIdData MessageId { get; }
        public ReadOnlySequence<byte> Data { get; }
        public int RedeliveryCount { get; }
    }
}
