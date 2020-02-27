using System;
using System.Collections.Generic;
using System.Text;

namespace SharpPulsar.Akka.InternalCommands
{
    public class ProducerCreated
    {
        public ProducerCreated(string name, long requestId, long lastSequenceId, byte[] schemaVersion)
        {
            Name = name;
            RequestId = requestId;
            LastSequenceId = lastSequenceId;
            SchemaVersion = schemaVersion;
        }

        public string Name { get; }
        public long RequestId { get; }
        public long LastSequenceId { get; }
        public byte[] SchemaVersion { get; }
    }
}
