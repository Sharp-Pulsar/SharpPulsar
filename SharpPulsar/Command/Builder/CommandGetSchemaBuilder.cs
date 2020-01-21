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
        private CommandGetSchemaBuilder(CommandGetSchema schema)
        {
            _schema = schema;
        }
        public CommandGetSchemaBuilder SetRequestId(long requestId)
        {
            _schema.RequestId = (ulong)requestId;
            return new CommandGetSchemaBuilder(_schema);
        }
        public CommandGetSchemaBuilder SetSchemaVersion(SchemaVersion version)
        {
            _schema.SchemaVersion = (byte[])(object)version.Bytes();
            return new CommandGetSchemaBuilder(_schema);
        }
        public CommandGetSchemaBuilder SetTopic(string topic)
        {
            _schema.Topic = topic;
            return new CommandGetSchemaBuilder(_schema);
        }
        public CommandGetSchema Build()
        {
            return _schema;
        }
    }
}
