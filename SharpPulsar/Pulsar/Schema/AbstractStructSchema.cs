using SharpPulsar.Api;
using SharpPulsar.Common.Schema;
using SharpPulsar.Pulsar.Api;
using SharpPulsar.Pulsar.Api.Schema;
using System;
using System.Collections.Generic;
using System.Text;

namespace SharpPulsar.Pulsar.Schema
{
    public class AbstractStructSchema : AbstractSchema
    {
        private readonly ISchemaInfo _schemaInfo;
        private ISchemaReader _reader;
        private ISchemaWriter _writer;
        private ISchemaInfoProvider _schemaInfoProvider;

        public AbstractStructSchema(ISchemaInfo schemaInfo)
        {
            _schemaInfo = schemaInfo;
        }
        public override ISchemaInfo SchemaInfo => _schemaInfo;

        public override sbyte[] Encode(object message)
        {
            return _writer.Write(message);
        }

        public override T Decode<T>(sbyte[] bytes, T returnType = default)
        {
            return _reader.Read(bytes, returnType);
        }
        public override T Decode<T>(byte[] bytes, T returnType = default)
        {
            return _reader.Read((sbyte[])(object)bytes, returnType);
        }

        public override T Decode<T>(byte[] bytes, sbyte[] schemaVersion, T returnType = default)
        {
            return _reader.Read((sbyte[])(object)bytes, schemaVersion, returnType);
        }

        public override ISchema Json(ISchemaDefinition schemaDefinition)
        {
            throw new NotImplementedException();
        }

        public override ISchema Json(object pojo)
        {
            throw new NotImplementedException();
        }

        public override bool RequireFetchingSchemaInfo()
        {
            return false;
        }

        public override bool SupportSchemaVersioning()
        {
            return false;
        }

        public override void Validate(sbyte[] message)
        {
            throw new NotImplementedException();
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

        protected internal virtual ISchemaWriter Writer
        {
            set
            {
                _writer = value;
            }
        }

        protected internal virtual ISchemaReader Reader
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
