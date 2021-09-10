using System;
using SharpPulsar.Interfaces;
using SharpPulsar.Interfaces.ISchema;
using SharpPulsar.Protocol.Schema;
using SharpPulsar.Schemas.Reader;
using SchemaSerializationException = SharpPulsar.Exceptions.SchemaSerializationException;
namespace SharpPulsar.Schemas
{
    public class AbstractStructSchema<T> : AbstractSchema<T>
    {
        private readonly ISchemaInfo _schemaInfo;
        protected internal ISchemaInfoProvider _schemaInfoProvider;
        private ISchemaReader<T> _reader;
        private ISchemaWriter<T> _writer;
        public AbstractStructSchema(ISchemaInfo schemaInfo)
        {
            _schemaInfo = schemaInfo;
        }
        public override ISchemaInfo SchemaInfo => _schemaInfo;

        public override byte[] Encode(T message)
        {
            return _writer.Write(message);
        }

        public override T Decode(byte[] bytes)
        {
            return _reader.Read(bytes);
        }

        public override T Decode(byte[] bytes, byte[] schemaVersion)
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
                _schemaInfoProvider = value;
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
        public override ISchema<T> AtSchemaVersion(byte[] schemaVersion)
        {
            Precondition.Condition.RequireNonNull(schemaVersion, "schemaVersion");
            if (_schemaInfoProvider == null)
            {
                // this schema is not downloaded from the registry
                return this;
            }
            try
            {
                var schemaInfo = _schemaInfoProvider.GetSchemaByVersion(schemaVersion);
                if (schemaInfo == null)
                {
                    throw new SchemaSerializationException("Unknown version " + BytesSchemaVersion.Of(schemaVersion));
                }
                return GetAbstractStructSchemaAtVersion(schemaVersion, schemaInfo);
            }
            catch (Exception err)
            {
                throw new SchemaSerializationException(err);
            }
        }

        private class WrappedVersionedSchema<S> : AbstractStructSchema<S>
        {
            internal readonly byte[] SchemaVersion;
            internal readonly AbstractStructSchema<S> Parent;
            public WrappedVersionedSchema(ISchemaInfo schemaInfo, in byte[] schemaVersion, AbstractStructSchema<S> parent) : base(schemaInfo)
            {
                SchemaVersion = schemaVersion;
                Writer = null;
                Reader = parent.Reader;
                SchemaInfoProvider = parent._schemaInfoProvider;
                Parent = parent;
            }

            public bool RequireFetchingSchemaInfo()
            {
                return true;
            }

            public override S Decode(byte[] bytes)
            {
                return Decode(bytes, SchemaVersion);
            }

            public sbyte[] Encode(T message)
            {
                throw new System.NotSupportedException("This schema is not meant to be used for encoding");
            }
            
            public object NativeSchema
            {
                get
                {
                    if (_reader is AbstractMultiVersionReader<T> abstractMultiVersionReader)
                    {
                        try
                        {
                            var schemaReader = abstractMultiVersionReader.GetSchemaReader(SchemaVersion);
                            return schemaReader.NativeSchema;
                        }
                        catch (Exception err)
                        {
                            throw;
                        }
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            
        }

        private AbstractStructSchema<T> GetAbstractStructSchemaAtVersion(byte[] schemaVersion, ISchemaInfo schemaInfo)
        {
            return new WrappedVersionedSchema<T>(schemaInfo, schemaVersion, this);
        }
    }
}
