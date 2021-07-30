using Avro;
using Avro.Generic;
using SharpPulsar.Extension;
using SharpPulsar.Interfaces;
using SharpPulsar.Interfaces.ISchema;
using SharpPulsar.Shared;
using System;
using System.Collections.Generic;
using System.Text;

namespace SharpPulsar.Schemas.Generic
{
    public class GenericAvroSchema : GenericSchema
    {
        private readonly ISchemaInfo _schemaInfo;
        private readonly string _stringSchema;
        private readonly RecordSchema _avroSchema;
        private readonly GenericDatumReader<GenericRecord> _avroReader;
        private readonly List<Field> _schemaFields;
        public GenericAvroSchema(ISchemaInfo schemaInfo):base(schemaInfo)
        {
            _schemaInfo = schemaInfo;
            _stringSchema = Encoding.UTF8.GetString(_schemaInfo.Schema);
            _avroSchema = (RecordSchema)Avro.Schema.Parse(_stringSchema);
            _avroReader = new GenericDatumReader<GenericRecord>(_avroSchema, _avroSchema);
            _schemaFields = _avroSchema.Fields;
        }
        public override ISchemaInfo SchemaInfo => new SchemaInfo 
        { 
            Name = "",
            Type = SchemaType.AVRO,
            Schema = _schemaInfo.Schema,
            Properties = new Dictionary<string, string>()
        };

        public override IGenericRecordBuilder NewRecordBuilder()
        {
            throw new NotImplementedException();
        }

        public override IGenericSchema<IGenericRecord> Generic(ISchemaInfo schemaInfo)
        {
            throw new NotImplementedException();
        }

        public override ISchema<object> GetSchema(ISchemaInfo schemaInfo)
        {
            throw new NotImplementedException();
        }

        public override ISchema<byte[]> AutoProduceBytes<T>(ISchema<T> schema)
        {
            throw new NotImplementedException();
        }

        public override ISchema<byte[]> AutoProduceBytes()
        {
            throw new NotImplementedException();
        }

        public override ISchema<IGenericRecord> AutoConsume()
        {
            throw new NotImplementedException();
        }

        public override ISchema<IGenericRecord> Auto()
        {
            throw new NotImplementedException();
        }

        public override ISchema<KeyValue<K, V>> KeyValue<K, V>(ISchema<K> key, ISchema<V> value, KeyValueEncodingType keyValueEncodingType)
        {
            throw new NotImplementedException();
        }

        public override ISchema<KeyValue<K, V>> KeyValue<K, V>(ISchema<K> key, ISchema<V> value)
        {
            throw new NotImplementedException();
        }

        public override ISchema<KeyValue<K, V>> KeyValue<K, V>(Type key, Type value)
        {
            throw new NotImplementedException();
        }

        public override ISchema<KeyValue<byte[], byte[]>> KvBytes()
        {
            throw new NotImplementedException();
        }

        public override ISchema<KeyValue<K, V>> KeyValue<K, V>(Type key, Type value, SchemaType type)
        {
            throw new NotImplementedException();
        }

        public override ISchema<T> Json<T>(ISchemaDefinition<T> schemaDefinition)
        {
            throw new NotImplementedException();
        }

        public override ISchema<T> Json<T>(Type pojo)
        {
            throw new NotImplementedException();
        }

        public override ISchema<T> AVRO<T>(ISchemaDefinition<T> schemaDefinition)
        {
            throw new NotImplementedException();
        }

        public override ISchema<T> AVRO<T>(Type pojo)
        {
            throw new NotImplementedException();
        }

        public override void ConfigureSchemaInfo(string topic, string componentName, SchemaInfo schemaInfo)
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

        public override void Validate(byte[] message)
        {
            throw new NotImplementedException();
        }
    }
}
