using System;
using System.Collections.Generic;
using System.Text;

namespace SharpPulsar.Akka.InternalCommands
{
    internal sealed class TopicEntries
    {
        public TopicEntries(long? entries)
        {
            Entries = entries;
        }

        public long? Entries { get; }
    }
}
