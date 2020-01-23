using System.Collections.Generic;
using System.IO;

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
	using Schema = org.apache.avro.Schema;
	using GenericDatumReader = org.apache.avro.generic.GenericDatumReader;
	using BinaryEncoder = org.apache.avro.io.BinaryEncoder;
	using Decoder = org.apache.avro.io.Decoder;
	using DecoderFactory = org.apache.avro.io.DecoderFactory;
	using EncoderFactory = org.apache.avro.io.EncoderFactory;
	using SchemaSerializationException = SharpPulsar.Api.SchemaSerializationException;
	using Field = SharpPulsar.Api.Schema.Field;
	using GenericRecord = SharpPulsar.Api.Schema.GenericRecord;
	using SharpPulsar.Api.Schema;

	using Logger = org.slf4j.Logger;
	using LoggerFactory = org.slf4j.LoggerFactory;



	public class GenericAvroReader : SchemaReader<GenericRecord>
	{

		private readonly GenericDatumReader<GenericAvroRecord> reader;
		private BinaryEncoder encoder;
		private readonly MemoryStream byteArrayOutputStream;
		private readonly IList<Field> fields;
		private readonly Schema schema;
		private readonly sbyte[] schemaVersion;
		public GenericAvroReader(Schema Schema) : this(null, Schema, null)
		{
		}

		public GenericAvroReader(Schema WriterSchema, Schema ReaderSchema, sbyte[] SchemaVersion)
		{
			this.schema = ReaderSchema;
			this.fields = schema.Fields.Select(f => new Field(f.name(), f.pos())).ToList();
			this.schemaVersion = SchemaVersion;
			if (WriterSchema == null)
			{
				this.reader = new GenericDatumReader<GenericAvroRecord>(ReaderSchema);
			}
			else
			{
				this.reader = new GenericDatumReader<GenericAvroRecord>(WriterSchema, ReaderSchema);
			}
			this.byteArrayOutputStream = new MemoryStream();
			this.encoder = EncoderFactory.get().binaryEncoder(this.byteArrayOutputStream, encoder);
		}

		public override GenericAvroRecord Read(sbyte[] Bytes, int Offset, int Length)
		{
			try
			{
				Decoder Decoder = DecoderFactory.get().binaryDecoder(Bytes, Offset, Length, null);
				org.apache.avro.generic.GenericRecord AvroRecord = (org.apache.avro.generic.GenericRecord)reader.read(null, Decoder);
				return new GenericAvroRecord(schemaVersion, schema, fields, AvroRecord);
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
				org.apache.avro.generic.GenericRecord AvroRecord = (org.apache.avro.generic.GenericRecord)reader.read(null, Decoder);
				return new GenericAvroRecord(schemaVersion, schema, fields, AvroRecord);
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
					log.error("GenericAvroReader close inputStream close error", E.Message);
				}
			}
		}

		private static readonly Logger log = LoggerFactory.getLogger(typeof(GenericAvroReader));
	}

}