
using SharpPulsar.Interfaces;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace SharpPulsar.Messages.Consumer
{
    public sealed class RedeliverUnacknowledgedMessageIds
    {        
        public ImmutableHashSet<IMessageId> MessageIds { get; }
        public RedeliverUnacknowledgedMessageIds(ISet<IMessageId> messageIds)
        {
            MessageIds = messageIds.ToImmutableHashSet();
        }
    } 
}
