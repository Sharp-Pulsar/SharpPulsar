
using SharpPulsar.Interfaces.ISchema;
namespace SharpPulsar.Schema
{
    public class AbstractStructSchema<T> : AbstractSchema<T>
    {
        private readonly ISchemaInfo _schemaInfo;
        private ISchemaReader<T> _reader;
        private ISchemaWriter<T> _writer;
        public AbstractStructSchema(ISchemaInfo schemaInfo)
        {
            _schemaInfo = schemaInfo;
        }
        public override ISchemaInfo SchemaInfo => _schemaInfo;

        public override sbyte[] Encode(T message)
        {
            return _writer.Write(message);
        }

        public override T Decode(sbyte[] bytes)
        {
            return _reader.Read(bytes);
        }

        public override T Decode(sbyte[] bytes, sbyte[] schemaVersion)
        {
            return _reader.Read(bytes, schemaVersion);
        }

        public override ISchemaInfoProvider SchemaInfoProvider
        {
            set
            {
                if (_reader != null)
                {
                    _reader.SchemaInfoProvider = value;
                }
            }
        }

        protected internal virtual ISchemaWriter<T> Writer
        {
            set
            {
                _writer = value;
            }
        }

        protected internal virtual ISchemaReader<T> Reader
        {
            set
            {
                _reader = value;
            }
            get
            {
                return _reader;
            }
        }
    }
}
