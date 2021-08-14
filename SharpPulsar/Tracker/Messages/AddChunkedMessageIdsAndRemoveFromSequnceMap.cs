using System.Collections.Generic;
using System.Collections.Immutable;
using SharpPulsar.Interfaces;

namespace SharpPulsar.Tracker.Messages
{
    public sealed class AddChunkedMessageIdsAndRemoveFromSequnceMap
    {
        public AddChunkedMessageIdsAndRemoveFromSequnceMap(List<IMessageId> messageId)
        {
            MessageIds = messageId.ToImmutableList();
        }

        public ImmutableList<IMessageId> MessageIds { get; } 
    }
    public sealed class AddChunkedMessageIdsAndRemoveFromSequnceMapResponse
    {
        public AddChunkedMessageIdsAndRemoveFromSequnceMapResponse(IImmutableSet<IMessageId> messageIds)
        {
            MessageIds = messageIds;
        }

        public IImmutableSet<IMessageId> MessageIds { get; }
    }
}
