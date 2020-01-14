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
namespace org.apache.pulsar.client.impl.schema.writer
{
	using Schema = org.apache.avro.Schema;
	using BinaryEncoder = org.apache.avro.io.BinaryEncoder;
	using EncoderFactory = org.apache.avro.io.EncoderFactory;
	using ReflectDatumWriter = org.apache.avro.reflect.ReflectDatumWriter;
	using SchemaSerializationException = org.apache.pulsar.client.api.SchemaSerializationException;
	using SchemaWriter = org.apache.pulsar.client.api.schema.SchemaWriter;

	public class AvroWriter<T> : SchemaWriter<T>
	{
		private readonly ReflectDatumWriter<T> writer;
		private BinaryEncoder encoder;
		private MemoryStream byteArrayOutputStream;

		public AvroWriter(Schema schema)
		{
			this.byteArrayOutputStream = new MemoryStream();
			this.encoder = EncoderFactory.get().binaryEncoder(this.byteArrayOutputStream, this.encoder);
			this.writer = new ReflectDatumWriter<T>(schema);
		}

		public override sbyte[] write(T message)
		{
			lock (this)
			{
				try
				{
					writer.write(message, this.encoder);
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