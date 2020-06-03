using System;
using System.Collections.Generic;
using System.Text;

namespace SharpPulsar.Akka.InternalCommands.Consumer
{
    public sealed class SendFlow
    {
        public SendFlow(long? size)
        {
            Size = size;
        }

        public long? Size { get; }
    }
}
