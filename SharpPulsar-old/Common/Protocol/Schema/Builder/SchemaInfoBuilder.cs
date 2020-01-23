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
        
        public SchemaInfoBuilder SetName(string name)
        {
            _info.Name = name;
            return this;
        }
        public SchemaInfoBuilder SetProperties(IDictionary<string, string> properties)
        {
            _info.Properties = properties;
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
