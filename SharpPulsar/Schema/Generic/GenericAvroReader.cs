using System;
using System.Collections.Generic;
using System.IO;
using Avro.Generic;
using Avro.IO;
using Avro.Reflect;
using Microsoft.Extensions.Logging;
using SharpPulsar.Common;
using SharpPulsar.Exceptions;
using SharpPulsar.Interfaces.ISchema;

/// <summary>
/// Licensed to the Apache Software Foundation (ASF) under one
/// or more contributor license agreements.  See the NOTICE file
/// distributed with this work for additional information
/// regarding copyright ownership.  The ASF licenses this file
/// to you under the Apache License, Version 2.0 (the
/// "License"); you may not use this file except in compliance
/// with the License.  You may obtain a copy of the License at
/// 
///   http://www.apache.org/licenses/LICENSE-2.0
/// 
/// Unless required by applicable law or agreed to in writing,
/// software distributed under the License is distributed on an
/// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
/// KIND, either express or implied.  See the License for the
/// specific language governing permissions and limitations
/// under the License.
/// </summary>
namespace SharpPulsar.Impl.Schema.Generic
{
    public class GenericAvroReader: ISchemaReader<GenericRecord>
	{
        private readonly Avro.Schema _schema;
		private readonly sbyte[] _schemaVersion;
        private readonly GenericDatumReader<GenericRecord> _reader;
        private BinaryEncoder _encoder;
        private readonly MemoryStream _byteArrayOutputStream;
        private readonly IList<Field> _fields;
        private int _offset;
		public GenericAvroReader(Avro.Schema schema) : this(null, schema, null)
		{
		}

		public GenericAvroReader(Avro.Schema writerSchema, Avro.Schema readerSchema, sbyte[] schemaVersion)
		{
			_schema = readerSchema;
			_fields = _schema.Fields.Select(f => new Field(f.name(), f.pos())).ToList();
			_schemaVersion = schemaVersion;
			if (writerSchema == null)
			{
				_reader = new GenericDatumReader<GenericRecord>(null, readerSchema);
			}
			else
			{
				_reader = new GenericDatumReader<GenericRecord>(writerSchema, readerSchema);
			}
			_byteArrayOutputStream = new MemoryStream();
			_encoder = new BinaryEncoder(_byteArrayOutputStream);

			if (_schema.GetProperty(GenericAvroSchema.OFFSET_PROP) != null)
			{
				_offset = int.Parse(_schema.GetProperty(GenericAvroSchema.OFFSET_PROP).ToString());
			}
			else
			{
				_offset = 0;
			}

		}
		public GenericAvroReader(Avro.Schema schema, sbyte[] schemaVersion)
        {
            _schema = schema;
            _schemaVersion = schemaVersion;
        }

        public GenericAvroReader(Avro.Schema schema)
        {
            _schema = schema;
        }
		public GenericRecord Read(sbyte[] bytes, int Offset, int Length)
		{
			try
			{
				if (Offset == 0 && this._offset > 0)
				{
					Offset = this._offset;
				}
				Decoder Decoder = new BinaryDecoder(bytes);
				org.apache.avro.generic.GenericRecord AvroRecord = (org.apache.avro.generic.GenericRecord)_reader.read(null, Decoder);
				return new GenericAvroRecord(_schemaVersion, _schema, _fields, AvroRecord);
			}
			catch (IOException E)
			{
				throw new SchemaSerializationException(E);
			}
		}

		public override GenericRecord Read(Stream InputStream)
		{
			try
			{
				Decoder Decoder = DecoderFactory.get().binaryDecoder(InputStream, null);
				org.apache.avro.generic.GenericRecord AvroRecord = (org.apache.avro.generic.GenericRecord)_reader.read(null, Decoder);
				return new GenericAvroRecord(_schemaVersion, _schema, _fields, AvroRecord);
			}
			catch (IOException E)
			{
				throw new SchemaSerializationException(E);
			}
			finally
			{
				try
				{
					InputStream.Close();
				}
				catch (IOException E)
				{
					_log.error("GenericAvroReader close inputStream close error", E);
				}
			}
		}
		public T Read(byte[] message)
        {
            var r = new ReflectDefaultReader(typeof(T), _schema, _schema, new ClassCache());
            using var stream = new MemoryStream(message);
            return (T)r.Read(default(object), new BinaryDecoder(stream));
        }
		
	}

}