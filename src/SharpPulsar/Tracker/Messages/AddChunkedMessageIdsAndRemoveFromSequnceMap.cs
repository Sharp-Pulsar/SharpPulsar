using System.Collections.Generic;
using System.Collections.Immutable;
using SharpPulsar.Interfaces;

namespace SharpPulsar.Tracker.Messages
{
    public readonly record struct AddChunkedMessageIdsAndRemoveFromSequnceMap
    {
        public AddChunkedMessageIdsAndRemoveFromSequnceMap(List<IMessageId> messageId)
        {
            MessageIds = messageId.ToImmutableList();
        }

        public ImmutableList<IMessageId> MessageIds { get; } 
    }
    public readonly record struct AddChunkedMessageIdsAndRemoveFromSequnceMapResponse
    {
        public AddChunkedMessageIdsAndRemoveFromSequnceMapResponse(IImmutableSet<IMessageId> messageIds)
        {
            MessageIds = messageIds;
        }

        public IImmutableSet<IMessageId> MessageIds { get; }
    }
}
