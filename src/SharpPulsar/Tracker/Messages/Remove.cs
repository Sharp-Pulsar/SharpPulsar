using SharpPulsar.Interfaces;

namespace SharpPulsar.Tracker.Messages
{
    public readonly record struct Remove
    {
        public Remove(IMessageId messageId)
        {
            MessageId = messageId;
        }

        public IMessageId MessageId { get; }
    }
   
}
