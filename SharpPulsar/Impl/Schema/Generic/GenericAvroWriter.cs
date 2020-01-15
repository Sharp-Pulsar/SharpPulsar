using Avro.Generic;
using Pulsar.Api.Schema;
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
namespace Pulsar.Client.Impl.Schema.Generic
{
	using Schema = Avro.Schema;
	using BinaryEncoder = Avro.IO.BinaryEncoder;
	using EncoderFactory = Avro.IO.EncoderFactory;
	using SchemaSerializationException = Api.SchemaSerializationException;
	using GenericRecord = Api.Schema.GenericRecord;

	public class GenericAvroWriter : SchemaWriter<GenericRecord>
	{

		private readonly GenericDatumWriter<Avro.Generic.GenericRecord> writer;
		private BinaryEncoder encoder;
		private readonly MemoryStream byteArrayOutputStream;

		public GenericAvroWriter(Schema schema)
		{
			this.writer = new GenericDatumWriter<Avro.Generic.GenericRecord>(schema);
			this.byteArrayOutputStream = new MemoryStream();
			this.encoder = EncoderFactory.get().binaryEncoder(this.byteArrayOutputStream, encoder);
		}

		public sbyte[]Write(GenericRecord message)
		{
			lock (this)
			{
				try
				{
					writer.Write(((GenericAvroRecord)message).AvroRecord, this.encoder);
					this.encoder.Flush();
					return this.byteArrayOutputStream.ToByteArray();
				}
				catch (Exception e)
				{
					throw new SchemaSerializationException(e);
				}
				finally
				{
					this.byteArrayOutputStream.Reset();
				}
			}
		}
	}

}