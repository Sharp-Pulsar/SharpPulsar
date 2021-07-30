using Avro.Generic;
using Avro.IO;
using Avro.Reflect;
using Avro.Specific;
using SharpPulsar.Extension;
using SharpPulsar.Interfaces.ISchema;
using System.IO;

namespace SharpPulsar.Schemas.Writer
{
    public class AvroWriter<T> : ISchemaWriter<T>
    {
        private readonly Avro.Schema _schema;
        private DatumWriter<T> _writer;

        public AvroWriter(Avro.Schema avroSchema)
        {
            _schema = avroSchema;
            var type = typeof(T);
            if(typeof(ISpecificRecord).IsAssignableFrom(type))
                _writer = new SpecificDatumWriter<T>(avroSchema);
            else
                _writer = new ReflectWriter<T>(_schema);
        }

		public byte[] Write(T message)
		{
            var ms = new MemoryStream();
            Encoder e = new BinaryEncoder(ms);
            _writer.Write(message, e);
            ms.Flush();
            ms.Position = 0;
            var b = ms.ToArray();
            return b;
        }
	}
}
