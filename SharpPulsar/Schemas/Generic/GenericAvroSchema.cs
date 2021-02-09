using Avro;
using Avro.Generic;
using Avro.IO;
using SharpPulsar.Exceptions;
using SharpPulsar.Interfaces;
using SharpPulsar.Interfaces.ISchema;
using SharpPulsar.Shared;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SharpPulsar.Schemas.Generic
{
    public class GenericAvroSchema : AbstractSchema<GenericRecord>
    {
        private readonly ISchemaInfo _schemaInfo;
        private readonly string _stringSchema;
        private readonly RecordSchema _avroSchema;
        private readonly GenericDatumReader<GenericRecord> _avroReader;
        private readonly List<Field> _schemaFields;
        public GenericAvroSchema(ISchemaInfo schemaInfo)
        {
            _schemaInfo = schemaInfo;
            _stringSchema = Encoding.UTF8.GetString((byte[])(object)_schemaInfo.Schema);
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

        public override sbyte[] Encode(GenericRecord message)
        {
            throw new SchemaSerializationException("GenericAvroSchema is for consuming only!");
        }
        public override GenericRecord Decode(sbyte[] bytes)
        {
            using var stream = new MemoryStream((byte[])(object)bytes);
            var record = _avroReader.Read(null, new BinaryDecoder(stream));
            return record;
        }
    }
}
