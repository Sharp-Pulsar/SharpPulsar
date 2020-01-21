using SharpPulsar.Common.PulsarApi;
using SharpPulsar.Common.Schema;

namespace SharpPulsar.Command.Builder
{
    public class CommandGetOrCreateSchemaBuilder
    {
        private CommandGetOrCreateSchema _schema;
        public CommandGetOrCreateSchemaBuilder()
        {
            _schema = new CommandGetOrCreateSchema();
        }
        private CommandGetOrCreateSchemaBuilder(CommandGetOrCreateSchema schema)
        {
            _schema = schema;
        }
        public CommandGetOrCreateSchemaBuilder SetRequestId(long requestId)
        {
            _schema.RequestId = (ulong)requestId;
            return new CommandGetOrCreateSchemaBuilder(_schema);
        }
        public CommandGetOrCreateSchemaBuilder SetSchema(SchemaInfo schema)
        {
            _schema.Schema = GetSchema(schema);
            return new CommandGetOrCreateSchemaBuilder(_schema);
        }
        public CommandGetOrCreateSchemaBuilder SetTopic(string topic)
        {
            _schema.Topic = topic;
            return new CommandGetOrCreateSchemaBuilder(_schema);
        }
        public CommandGetOrCreateSchema Build()
        {
            return _schema;
        }
        private Schema GetSchema(SchemaInfo schemaInfo)
        {
            return new SchemaBuilder()
                .SetName(schemaInfo)
                .SetType(schemaInfo)
                .SetSchemaData(schemaInfo).Build();
        }
    }
}
