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
namespace SharpPulsar.Impl.Schema.Reader
{
	using Schema = org.apache.avro.Schema;
	using BinaryDecoder = org.apache.avro.io.BinaryDecoder;
	using DecoderFactory = org.apache.avro.io.DecoderFactory;
	using ReflectDatumReader = org.apache.avro.reflect.ReflectDatumReader;
	using SchemaSerializationException = Api.SchemaSerializationException;
	using SharpPulsar.Api.Schema;

	using Logger = org.slf4j.Logger;
	using LoggerFactory = org.slf4j.LoggerFactory;


	public class AvroReader<T> : ISchemaReader<T>
	{

		private ReflectDatumReader<T> reader;
		private static readonly ThreadLocal<BinaryDecoder> decoders = new ThreadLocal<BinaryDecoder>();

		public AvroReader(Schema Schema)
		{
			this.reader = new ReflectDatumReader<T>(Schema);
		}

		public AvroReader(Schema WriterSchema, Schema ReaderSchema)
		{
			this.reader = new ReflectDatumReader<T>(WriterSchema, ReaderSchema);
		}

		public override T Read(sbyte[] Bytes, int Offset, int Length)
		{
			try
			{
				BinaryDecoder DecoderFromCache = decoders.get();
				BinaryDecoder Decoder = DecoderFactory.get().binaryDecoder(Bytes, Offset, Length, DecoderFromCache);
				if (DecoderFromCache == null)
				{
					decoders.set(Decoder);
				}
				return reader.read(null, DecoderFactory.get().binaryDecoder(Bytes, Offset, Length, Decoder));
			}
			catch (IOException E)
			{
				throw new SchemaSerializationException(E);
			}
		}

		public override T Read(Stream InputStream)
		{
			try
			{
				BinaryDecoder DecoderFromCache = decoders.get();
				BinaryDecoder Decoder = DecoderFactory.get().binaryDecoder(InputStream, DecoderFromCache);
				if (DecoderFromCache == null)
				{
					decoders.set(Decoder);
				}
				return reader.read(null, DecoderFactory.get().binaryDecoder(InputStream, Decoder));
			}
			catch (Exception E)
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
					log.error("AvroReader close inputStream close error", E.Message);
				}
			}
		}

		private static readonly Logger log = LoggerFactory.getLogger(typeof(AvroReader));

	}

}