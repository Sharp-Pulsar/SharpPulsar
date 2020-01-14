using System;
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
	using GenericDatumWriter = org.apache.avro.generic.GenericDatumWriter;
	using BinaryEncoder = org.apache.avro.io.BinaryEncoder;
	using EncoderFactory = org.apache.avro.io.EncoderFactory;
	using SchemaSerializationException = org.apache.pulsar.client.api.SchemaSerializationException;
	using GenericRecord = org.apache.pulsar.client.api.schema.GenericRecord;
	using SchemaWriter = org.apache.pulsar.client.api.schema.SchemaWriter;

	public class GenericAvroWriter : SchemaWriter<GenericRecord>
	{

		private readonly GenericDatumWriter<org.apache.avro.generic.GenericRecord> writer;
		private BinaryEncoder encoder;
		private readonly MemoryStream byteArrayOutputStream;

		public GenericAvroWriter(Schema schema)
		{
			this.writer = new GenericDatumWriter<org.apache.avro.generic.GenericRecord>(schema);
			this.byteArrayOutputStream = new MemoryStream();
			this.encoder = EncoderFactory.get().binaryEncoder(this.byteArrayOutputStream, encoder);
		}

		public override sbyte[] write(GenericRecord message)
		{
			lock (this)
			{
				try
				{
					writer.write(((GenericAvroRecord)message).AvroRecord, this.encoder);
					this.encoder.flush();
					return this.byteArrayOutputStream.toByteArray();
				}
				catch (Exception e)
				{
					throw new SchemaSerializationException(e);
				}
				finally
				{
					this.byteArrayOutputStream.reset();
				}
			}
		}
	}

}