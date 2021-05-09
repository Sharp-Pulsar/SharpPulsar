using SharpPulsar.Protocol.Schema;
using SharpPulsar.Shared;
using System.Collections.Generic;

namespace SharpPulsar.Protocol.Builder
{
    public class SchemaDataBuilder
    {
        private SchemaData _data;
        public SchemaDataBuilder()
        {
            _data = new SchemaData();
        }
        public SchemaDataBuilder SetData(byte[] data)
        {
            _data.Data = data;
            return this;
        }
        public SchemaDataBuilder SetDeleted(bool isDeleted)
        {
            _data.IsDeleted = isDeleted;
            return this;
        }
        public SchemaDataBuilder SetProperties(IDictionary<string, string> props)
        {
            _data.Properties = props;
            return this;
        }
        public SchemaDataBuilder SetTimeStamp(long timeStamp)
        {
            _data.Timestamp = timeStamp;
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