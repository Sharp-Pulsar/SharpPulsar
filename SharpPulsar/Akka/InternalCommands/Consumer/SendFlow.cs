using System;
using System.Collections.Generic;
using System.Text;

namespace SharpPulsar.Akka.InternalCommands.Consumer
{
    public sealed class SendFlow
    {
        public SendFlow(int size)
        {
            Size = size;
        }

        public int Size { get; }
    }
}
