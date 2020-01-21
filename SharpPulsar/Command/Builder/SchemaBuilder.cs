using SharpPulsar.Common.PulsarApi;
using SharpPulsar.Common.Schema;
using System.Linq;

namespace SharpPulsar.Command.Builder
{
    public class SchemaBuilder
    {
        private readonly Schema _schema;
        public SchemaBuilder()
        {
            _schema = new Schema();
        }
        private SchemaBuilder(Schema schema)
        {
            _schema = schema;
        }
        public SchemaBuilder SetName(SchemaInfo schemaInfo)
        {
            _schema.Name = schemaInfo.Name;
            return new SchemaBuilder(_schema);
        }
        public SchemaBuilder SetSchemaData(SchemaInfo schemaInfo)
        {
            _schema.SchemaData = (byte[])(object)schemaInfo.Schema;
            return new SchemaBuilder(_schema);
        }
        public SchemaBuilder SetType(SchemaInfo schemaInfo)
        {
            _schema.type = GetSchemaType(schemaInfo.Type);
            return new SchemaBuilder(_schema);
        }
        public SchemaBuilder AddAllProperties(SchemaInfo schemaInfo)
        {
            _schema.Properties.AddRange(schemaInfo.Properties.Select(x => new KeyValue { Key = x.Key, Value = x.Value}));
            return new SchemaBuilder(_schema);
        }
        public Schema Build()
        {
            return _schema;
        }
        private Schema.Type GetSchemaType(SchemaType type)
        {
            if (type.Value < 0)
            {
                return Schema.Type.None;
            }
            else
            {
                var schema = System.Enum.GetValues(typeof(Schema.Type)).Cast<Schema.Type>().ToList()[type.Value];
                return schema;
            }
        }
    }
    
}
