using System;
using System.Collections.Generic;
using System.Text;

namespace SharpPulsar.Messages.Consumer
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
