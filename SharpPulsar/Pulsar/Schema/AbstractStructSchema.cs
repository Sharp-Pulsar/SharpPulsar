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


        public override void ConfigureSchemaInfo(string topic, string componentName, SchemaInfo schemaInfo)
        {
            throw new NotImplementedException();
        }

        public override sbyte[] Encode(object message)
        {
            return _writer.Write(message);
        }

        public override object Decode(sbyte[] bytes, Type returnType)
        {
            return _reader.Read(bytes);
        }
        public override object Decode(byte[] bytes, Type returnType)
        {
            return _reader.Read((sbyte[])(object)bytes);
        }

        public override object Decode(byte[] bytes, sbyte[] schemaVersion, Type returnType)
        {
            return _reader.Read((sbyte[])(object)bytes, schemaVersion);
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
            throw new NotImplementedException();
        }

        public override bool SupportSchemaVersioning()
        {
            throw new NotImplementedException();
        }

        public override void Validate(sbyte[] message, Type returnType)
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
