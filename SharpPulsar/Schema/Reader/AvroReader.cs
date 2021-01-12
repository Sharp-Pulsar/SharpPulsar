using Avro.IO;
using Avro.Reflect;
using Avro.Specific;
using SharpPulsar.Extension;
using SharpPulsar.Interfaces.ISchema;
using System.IO;

namespace SharpPulsar.Schema.Reader
{
    public class AvroReader<T> : ISchemaReader<T>
    {
        private SpecificDatumReader<T> _reader;
        private ClassCache _classCache;
        private ReflectDefaultReader _defaultReader;

        public AvroReader(Avro.Schema avroSchema)
        {
            _classCache = new ClassCache();
            _reader = new SpecificDatumReader<T>(avroSchema, avroSchema);
            _defaultReader = new ReflectDefaultReader(typeof(T), avroSchema, avroSchema, _classCache);
        }

        public AvroReader(Avro.Schema writeSchema, Avro.Schema readSchema)
        {
            _classCache = new ClassCache();
            _reader = new SpecificDatumReader<T>(writeSchema, readSchema);
            _defaultReader = new ReflectDefaultReader(typeof(T), writeSchema, readSchema, _classCache);
        }

        public T Read(Stream stream)
        {
            return (T)_defaultReader.Read(default(object), new BinaryDecoder(stream));
        }

        public T Read(sbyte[] bytes, int offset, int length)
        {
            using var stream = new MemoryStream(bytes.ToBytes());
            return (T)_defaultReader.Read(default(object), new BinaryDecoder(stream));
        }
    }
}
