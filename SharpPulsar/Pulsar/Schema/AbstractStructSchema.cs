using SharpPulsar.Api;
using SharpPulsar.Common.Schema;
using SharpPulsar.Pulsar.Api.Schema;
using System;
using System.Collections.Generic;
using System.Text;

namespace SharpPulsar.Pulsar.Schema
{
    internal class AbstractStructSchema : AbstractSchema
    {
        protected internal static readonly Logger LOG = LoggerFactory.getLogger(typeof(AbstractStructSchema));

        private readonly ISchemaInfo _schemaInfo;
        private ISchemaReader _reader;
        private ISchemaWriter _writer;
        private ISchemaInfoProvider _schemaInfoProvider;

        public AbstractStructSchema(ISchemaInfo schemaInfo)
        {
            _schemaInfo = schemaInfo;
        }
        public override ISchemaInfo SchemaInfo => _schemaInfo;

        public override ISchema Auto()
        {
            throw new NotImplementedException();
        }

        public override void ConfigureSchemaInfo(string topic, string componentName, SchemaInfo schemaInfo)
        {
            throw new NotImplementedException();
        }

        public override object Decode(byte[] byteBuf, Type returnType)
        {
            throw new NotImplementedException();
        }

        public override object Decode(sbyte[] bytes, Type returnType)
        {
            throw new NotImplementedException();
        }

        public override sbyte[] Encode(object message)
        {
            throw new NotImplementedException();
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
