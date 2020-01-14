using Pulsar.Api.Schema;
using System;
using System.IO;
using System.Threading;

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
namespace Pulsar.Client.Impl.Schema.Reader
{
	using Schema = Avro.Schema;
	using BinaryDecoder = Avro.IO.BinaryDecoder;
	using DecoderFactory = Avro.IO.DecoderFactory;
	using SchemaSerializationException = Api.SchemaSerializationException;

	using Logger = org.slf4j.Logger;
	using LoggerFactory = org.slf4j.LoggerFactory;


	public class AvroReader<T> : SchemaReader<T>
	{

		private ReflectDatumReader<T> reader;
		private static readonly ThreadLocal<BinaryDecoder> decoders = new ThreadLocal<BinaryDecoder>();

		public AvroReader(Schema schema)
		{
			this.reader = new ReflectDatumReader<T>(schema);
		}

		public AvroReader(Schema writerSchema, Schema readerSchema)
		{
			this.reader = new ReflectDatumReader<T>(writerSchema, readerSchema);
		}

		public T Read(sbyte[] bytes, int offset, int length)
		{
			try
			{
				BinaryDecoder decoderFromCache = decoders.Get();
				BinaryDecoder decoder = DecoderFactory.get().binaryDecoder(bytes, offset, length, decoderFromCache);
				if (decoderFromCache == null)
				{
					decoders.set(decoder);
				}
				return reader.read(null, DecoderFactory.get().binaryDecoder(bytes, offset, length, decoder));
			}
			catch (IOException e)
			{
				throw new SchemaSerializationException(e);
			}
		}

		public T Read(Stream inputStream)
		{
			try
			{
				BinaryDecoder decoderFromCache = decoders.get();
				BinaryDecoder decoder = DecoderFactory.get().binaryDecoder(inputStream, decoderFromCache);
				if (decoderFromCache == null)
				{
					decoders.set(decoder);
				}
				return reader.read(null, DecoderFactory.get().binaryDecoder(inputStream, decoder));
			}
			catch (Exception e)
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
					log.error("AvroReader close inputStream close error", e.Message);
				}
			}
		}

		private static readonly Logger log = LoggerFactory.getLogger(typeof(AvroReader));

	}

}