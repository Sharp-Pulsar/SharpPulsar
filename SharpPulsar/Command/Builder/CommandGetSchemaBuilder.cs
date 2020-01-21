using SharpPulsar.Common.Protocol.Schema;
using SharpPulsar.Common.PulsarApi;

namespace SharpPulsar.Command.Builder
{
    public class CommandGetSchemaBuilder
    {
        private CommandGetSchema _schema;
        public CommandGetSchemaBuilder()
        {
            _schema = new CommandGetSchema();
        }
        
        public CommandGetSchemaBuilder SetRequestId(long requestId)
        {
            _schema.RequestId = (ulong)requestId;
            return this;
        }
        public CommandGetSchemaBuilder SetSchemaVersion(SchemaVersion version)
        {
            _schema.SchemaVersion = (byte[])(object)version.Bytes();
            return this;
        }
        public CommandGetSchemaBuilder SetTopic(string topic)
        {
            _schema.Topic = topic;
            return this;
        }
        public CommandGetSchema Build()
        {
            return _schema;
        }
    }
}
