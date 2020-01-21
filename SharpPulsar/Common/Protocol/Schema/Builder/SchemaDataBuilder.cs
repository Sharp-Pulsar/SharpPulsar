using SharpPulsar.Common.Schema;
using System;
using System.Collections.Generic;
using System.Text;

namespace SharpPulsar.Common.Protocol.Schema.Builder
{
    public class SchemaDataBuilder
    {
        private SchemaData _data;
        public SchemaDataBuilder()
        {
            _data = new SchemaData();
        }
        
        public SchemaDataBuilder SetData(sbyte[] data)
        {
            _data.Data = data;
            return this;
        }
        public SchemaDataBuilder SetIsDeleted(bool deleted)
        {
            _data.IsDeleted = deleted;
            return this;
        }
        public SchemaDataBuilder SetProps(IDictionary<string, string> props)
        {
            _data.Props = props;
            return this;
        }
        public SchemaDataBuilder SetTimestamp(long timestamp)
        {
            _data.Timestamp = timestamp;
            return this;
        }
        public SchemaDataBuilder SetType(SchemaType type)
        {
            _data.Type = type;
            return this;
        }
        public SchemaDataBuilder SetUser(string user)
        {
            _data.User = user;
            return this;
        }

        public SchemaData Build()
        {
            return _data;
        }
    }
}
