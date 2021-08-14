
using SharpPulsar.Interfaces;

namespace SharpPulsar.Messages.Consumer
{
    public sealed class SeekMessageId
    {
        public IMessageId MessageId { get; }
        public SeekMessageId(IMessageId messageId)
        {
            MessageId = messageId;
        }
    } 
}
