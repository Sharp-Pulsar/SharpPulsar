using System;
using SharpPulsar.Impl;

namespace SharpPulsar.Akka.Consumer
{
	public sealed class OpBatchReceive
    {

        internal Messages Messages;
        internal long CreatedAt;

        public OpBatchReceive(Messages messages)
        {
            Messages = messages;
            CreatedAt = DateTime.Now.Ticks;
        }

        internal static OpBatchReceive Of(Messages messages)
        {
            return new OpBatchReceive(messages);
        }
    }
}
