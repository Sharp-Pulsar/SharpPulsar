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
        private SchemaDataBuilder(SchemaData data)
        {
            _data = data;
        }
        public SchemaDataBuilder SetData(sbyte[] data)
        {
            _data.Data = data;
            return new SchemaDataBuilder(_data);
        }
        public SchemaDataBuilder SetIsDeleted(bool deleted)
        {
            _data.IsDeleted = deleted;
            return new SchemaDataBuilder(_data);
        }
        public SchemaDataBuilder SetProps(IDictionary<string, string> props)
        {
            _data.Props = props;
            return new SchemaDataBuilder(_data);
        }
        public SchemaDataBuilder SetTimestamp(long timestamp)
        {
            _data.Timestamp = timestamp;
            return new SchemaDataBuilder(_data);
        }
        public SchemaDataBuilder SetType(SchemaType type)
        {
            _data.Type = type;
            return new SchemaDataBuilder(_data);
        }
        public SchemaDataBuilder SetUser(string user)
        {
            _data.User = user;
            return new SchemaDataBuilder(_data);
        }

        public SchemaData Build()
        {
            return _data;
        }
    }
}
