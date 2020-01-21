using SharpPulsar.Common.Schema;
using System;
using System.Collections.Generic;
using System.Text;

namespace SharpPulsar.Common.Protocol.Schema.Builder
{
    public class SchemaInfoBuilder
    {
        private readonly SchemaInfo _info;
        public SchemaInfoBuilder()
        {
            _info = new SchemaInfo();
        }
        private SchemaInfoBuilder(SchemaInfo info)
        {
            _info = info;
        }
        public SchemaInfoBuilder SetName(string name)
        {
            _info.Name = name;
            return new SchemaInfoBuilder(_info);
        }
        public SchemaInfoBuilder SetProperties(IDictionary<string, string> properties)
        {
            _info.Properties = properties;
            return new SchemaInfoBuilder(_info);
        }
        public SchemaInfoBuilder SetSchema(sbyte[] schema)
        {
            _info.Schema = schema;
            return new SchemaInfoBuilder(_info);
        }
        public SchemaInfoBuilder SetType(SchemaType type)
        {
            _info.Type = type;
            return new SchemaInfoBuilder(_info);
        }
        public SchemaInfo Build()
        {
            return _info;
        }
    }
}
