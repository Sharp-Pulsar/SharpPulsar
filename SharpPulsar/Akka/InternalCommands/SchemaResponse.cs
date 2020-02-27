using System.Collections.Immutable;
using SharpPulsar.Protocol.Proto;

namespace SharpPulsar.Akka.InternalCommands
{
    public sealed class SchemaResponse
    {
        public long RequestId { get; }
        public byte[] Schema { get; }
        public string Name { get; }
        public ImmutableDictionary<string, string> Properties { get; }
        public Schema.Types.Type Type { get; }


        public SchemaResponse(byte[] schema, string name, ImmutableDictionary<string, string> properties, Schema.Types.Type type, long requestId)
        {
            Schema = schema;
            Name = name;
            Properties = properties;
            Type = type;
            RequestId = requestId;
        }
    }
}
