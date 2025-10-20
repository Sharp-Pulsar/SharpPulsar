using System.Collections.Generic;
using System.Collections.Immutable;

namespace SharpPulsar.Tracker.Messages
{
    public record AddChunkedMessageIdsAndRemoveFromSequnceMap
    {
        public AddChunkedMessageIdsAndRemoveFromSequnceMap(List<IMessageId> messageId)
        {
            MessageIds = messageId.ToImmutableList();
        }

        public ImmutableList<IMessageId> MessageIds { get; } 
    }
    public record AddChunkedMessageIdsAndRemoveFromSequnceMapResponse
    {
        public AddChunkedMessageIdsAndRemoveFromSequnceMapResponse(IImmutableSet<IMessageId> messageIds)
        {
            MessageIds = messageIds;
        }

        public IImmutableSet<IMessageId> MessageIds { get; }
    }
}
