using System.Collections.Immutable;
using SharpPulsar.Api;

namespace SharpPulsar.Tracker.Messages
{
    public sealed class AddChunkedMessageIdsAndRemoveFromSequnceMap
    {
        public AddChunkedMessageIdsAndRemoveFromSequnceMap(IMessageId messageId, IImmutableSet<IMessageId> messageIds)
        {
            MessageId = messageId;
            MessageIds = messageIds;
        }

        public IMessageId MessageId { get; } 
        public IImmutableSet<IMessageId> MessageIds { get; }
    }
}
