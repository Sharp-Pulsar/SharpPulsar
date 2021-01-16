using Avro.IO;
using Avro.Reflect;
using Avro.Specific;
using SharpPulsar.Exceptions;
using SharpPulsar.Extension;
using SharpPulsar.Interfaces.ISchema;
using System;
using System.IO;

namespace SharpPulsar.Schemas.Writer
{
    public class AvroWriter<T> : ISchemaWriter<T>
    {
        private SpecificDatumWriter<T> _writer;
        private ClassCache _classCache;
        private ReflectDefaultWriter _defaultWriter;
        private BinaryEncoder _encoder;
        private MemoryStream _byteArrayOutputStream;



        public AvroWriter(Avro.Schema avroSchema)
        {
            _byteArrayOutputStream = new MemoryStream();
            _encoder = new BinaryEncoder(_byteArrayOutputStream);
            _classCache = new ClassCache();
            _writer = new SpecificDatumWriter<T>(avroSchema);
            _defaultWriter = new ReflectDefaultWriter(typeof(T), avroSchema, _classCache);
        }

		public sbyte[] Write(T message)
		{
			lock (this)
			{
				sbyte[] OutputBytes = null;
				try
				{
					_defaultWriter.Write(message, _encoder);
				}
				catch (Exception E)
				{
					throw new SchemaSerializationException(E);
				}
				finally
				{
					try
					{
						_encoder.Flush();
						OutputBytes = _byteArrayOutputStream.ToArray().ToSBytes();
					}
					catch (Exception Ex)
					{
						throw new SchemaSerializationException(Ex);
					}
					_byteArrayOutputStream.Position = 0;
					_byteArrayOutputStream.SetLength(0);
				}
				return OutputBytes;
			}
		}
	}
}
