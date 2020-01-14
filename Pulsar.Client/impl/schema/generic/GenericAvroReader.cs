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
namespace org.apache.pulsar.client.impl.schema.generic
{
	using Schema = org.apache.avro.Schema;
	using GenericDatumReader = org.apache.avro.generic.GenericDatumReader;
	using BinaryEncoder = org.apache.avro.io.BinaryEncoder;
	using Decoder = org.apache.avro.io.Decoder;
	using DecoderFactory = org.apache.avro.io.DecoderFactory;
	using EncoderFactory = org.apache.avro.io.EncoderFactory;
	using SchemaSerializationException = org.apache.pulsar.client.api.SchemaSerializationException;
	using Field = org.apache.pulsar.client.api.schema.Field;
	using GenericRecord = org.apache.pulsar.client.api.schema.GenericRecord;
	using SchemaReader = org.apache.pulsar.client.api.schema.SchemaReader;

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
		public GenericAvroReader(Schema schema) : this(null, schema, null)
		{
		}

		public GenericAvroReader(Schema writerSchema, Schema readerSchema, sbyte[] schemaVersion)
		{
			this.schema = readerSchema;
			this.fields = schema.Fields.Select(f => new Field(f.name(), f.pos())).ToList();
			this.schemaVersion = schemaVersion;
			if (writerSchema == null)
			{
				this.reader = new GenericDatumReader<GenericAvroRecord>(readerSchema);
			}
			else
			{
				this.reader = new GenericDatumReader<GenericAvroRecord>(writerSchema, readerSchema);
			}
			this.byteArrayOutputStream = new MemoryStream();
			this.encoder = EncoderFactory.get().binaryEncoder(this.byteArrayOutputStream, encoder);
		}

		public override GenericAvroRecord read(sbyte[] bytes, int offset, int length)
		{
			try
			{
				Decoder decoder = DecoderFactory.get().binaryDecoder(bytes, offset, length, null);
				org.apache.avro.generic.GenericRecord avroRecord = (org.apache.avro.generic.GenericRecord)reader.read(null, decoder);
				return new GenericAvroRecord(schemaVersion, schema, fields, avroRecord);
			}
			catch (IOException e)
			{
				throw new SchemaSerializationException(e);
			}
		}

		public override GenericRecord read(Stream inputStream)
		{
			try
			{
				Decoder decoder = DecoderFactory.get().binaryDecoder(inputStream, null);
				org.apache.avro.generic.GenericRecord avroRecord = (org.apache.avro.generic.GenericRecord)reader.read(null, decoder);
				return new GenericAvroRecord(schemaVersion, schema, fields, avroRecord);
			}
			catch (IOException e)
			{
				throw new SchemaSerializationException(e);
			}
			finally
			{
				try
				{
					inputStream.Close();
				}
				catch (IOException e)
				{
					log.error("GenericAvroReader close inputStream close error", e.Message);
				}
			}
		}

		private static readonly Logger log = LoggerFactory.getLogger(typeof(GenericAvroReader));
	}

}