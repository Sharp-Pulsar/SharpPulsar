using Avro.Generic;
using Avro.IO;
using Avro.Reflect;
using Avro.Specific;
using SharpPulsar.Extension;
using SharpPulsar.Interfaces.ISchema;
using System.IO;

namespace SharpPulsar.Schemas.Reader
{
    public class AvroReader<T> : ISchemaReader<T>
    {

        private readonly Avro.Schema _schema;
        private DatumReader<T> _reader;

        public AvroReader(Avro.Schema avroSchema)
        {
            _schema = avroSchema;
            var type = typeof(T);
            if (typeof(ISpecificRecord).IsAssignableFrom(type))
                _reader = new SpecificDatumReader<T>(avroSchema, avroSchema);
            else
                _reader = new ReflectReader<T>(_schema, _schema);
        }

        public AvroReader(Avro.Schema writeSchema, Avro.Schema readSchema)
        {
            var type = typeof(T);
            if (typeof(ISpecificRecord).IsAssignableFrom(type))
                _reader = new SpecificDatumReader<T>(writeSchema, readSchema);
            else
                _reader = new ReflectReader<T>(writeSchema, readSchema);
        }

        public T Read(Stream stream)
        {
            stream.Seek(0, SeekOrigin.Begin);
            return _reader.Read(default, new BinaryDecoder(stream));
        }

        public T Read(sbyte[] bytes, int offset, int length)
        {
            using var stream = new MemoryStream(bytes.ToBytes());
            stream.Seek(offset, SeekOrigin.Begin);
            return _reader.Read(default, new BinaryDecoder(stream));
        }
    }
}
