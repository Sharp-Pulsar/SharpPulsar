
using SharpPulsar.Interfaces;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace SharpPulsar.Messages.Consumer
{
    public record struct RedeliverUnacknowledgedMessageIds(ISet<IMessageId> messageIds) 
    {
        public ImmutableHashSet<IMessageId> MessageIds => messageIds.ToImmutableHashSet();   
    }
}
