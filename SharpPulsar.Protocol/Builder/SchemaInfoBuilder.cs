using SharpPulsar.Common.Schema;
using SharpPulsar.Shared;
using System.Collections.Generic;

namespace SharpPulsar.Protocol.Builder
{
    public class SchemaInfoBuilder
    {
        private SchemaInfo _info;
        public SchemaInfoBuilder()
        {
            _info = new SchemaInfo();
        }
        public SchemaInfoBuilder SetName(string name)
        {
            _info.Name = name;
            return this;
        }
        public SchemaInfoBuilder SetProperties(IDictionary<string, string> props)
        {
            _info.Properties = props;
            return this;
        }
        public SchemaInfoBuilder SetSchema(sbyte[] schema)
        {
            _info.Schema = schema;
            return this;
        }
        public SchemaInfoBuilder SetType(SchemaType type)
        {
            _info.Type = type;
            return this;
        }
        public SchemaInfo Build()
        {
            return _info;
        }
    }
}