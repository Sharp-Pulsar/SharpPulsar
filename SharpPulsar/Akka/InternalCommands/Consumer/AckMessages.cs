using System.Collections.Generic;
using SharpPulsar.Api;
using SharpPulsar.Impl;

namespace SharpPulsar.Akka.InternalCommands.Consumer
{
    public class AckMessages
    {
        public AckMessages(MessageId messageId, IList<long> ackSets)
        {
            MessageId = messageId;
            AckSets = ackSets;
        }

        public MessageId MessageId { get; }
        public IList<long> AckSets { get; }
    }
}
