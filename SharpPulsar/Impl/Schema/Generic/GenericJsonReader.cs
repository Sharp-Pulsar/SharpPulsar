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
	using JsonNode = com.fasterxml.jackson.databind.JsonNode;
	using ObjectMapper = com.fasterxml.jackson.databind.ObjectMapper;
	using SchemaSerializationException = SharpPulsar.Api.SchemaSerializationException;
	using Field = SharpPulsar.Api.Schema.Field;
	using IGenericRecord = SharpPulsar.Api.Schema.IGenericRecord;
	using SharpPulsar.Api.Schema;

	using Logger = org.slf4j.Logger;
	using LoggerFactory = org.slf4j.LoggerFactory;


	public class GenericJsonReader : ISchemaReader<IGenericRecord>
	{

		private readonly ObjectMapper objectMapper;
		private readonly sbyte[] schemaVersion;
		private readonly IList<Field> fields;
		public GenericJsonReader(IList<Field> Fields)
		{
			this.fields = Fields;
			this.schemaVersion = null;
			this.objectMapper = new ObjectMapper();
		}

		public GenericJsonReader(sbyte[] SchemaVersion, IList<Field> Fields)
		{
			this.objectMapper = new ObjectMapper();
			this.fields = Fields;
			this.schemaVersion = SchemaVersion;
		}
		public override GenericJsonRecord Read(sbyte[] Bytes, int Offset, int Length)
		{
			try
			{
				JsonNode Jn = objectMapper.readTree(StringHelper.NewString(Bytes, Offset, Length, UTF_8));
				return new GenericJsonRecord(schemaVersion, fields, Jn);
			}
			catch (IOException Ioe)
			{
				throw new SchemaSerializationException(Ioe);
			}
		}

		public override IGenericRecord Read(Stream InputStream)
		{
			try
			{
				JsonNode Jn = objectMapper.readTree(InputStream);
				return new GenericJsonRecord(schemaVersion, fields, Jn);
			}
			catch (IOException Ioe)
			{
				throw new SchemaSerializationException(Ioe);
			}
			finally
			{
				try
				{
					InputStream.Close();
				}
				catch (IOException E)
				{
					log.error("GenericJsonReader close inputStream close error", E.Message);
				}
			}
		}

		private static readonly Logger log = LoggerFactory.getLogger(typeof(GenericJsonReader));
	}

}