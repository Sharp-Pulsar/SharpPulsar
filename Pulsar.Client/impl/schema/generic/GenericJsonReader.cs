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
	using JsonNode = com.fasterxml.jackson.databind.JsonNode;
	using ObjectMapper = com.fasterxml.jackson.databind.ObjectMapper;
	using SchemaSerializationException = org.apache.pulsar.client.api.SchemaSerializationException;
	using Field = org.apache.pulsar.client.api.schema.Field;
	using GenericRecord = org.apache.pulsar.client.api.schema.GenericRecord;
	using SchemaReader = org.apache.pulsar.client.api.schema.SchemaReader;

	using Logger = org.slf4j.Logger;
	using LoggerFactory = org.slf4j.LoggerFactory;


	public class GenericJsonReader : SchemaReader<GenericRecord>
	{

		private readonly ObjectMapper objectMapper;
		private readonly sbyte[] schemaVersion;
		private readonly IList<Field> fields;
		public GenericJsonReader(IList<Field> fields)
		{
			this.fields = fields;
			this.schemaVersion = null;
			this.objectMapper = new ObjectMapper();
		}

		public GenericJsonReader(sbyte[] schemaVersion, IList<Field> fields)
		{
			this.objectMapper = new ObjectMapper();
			this.fields = fields;
			this.schemaVersion = schemaVersion;
		}
		public override GenericJsonRecord read(sbyte[] bytes, int offset, int length)
		{
			try
			{
				JsonNode jn = objectMapper.readTree(StringHelper.NewString(bytes, offset, length, UTF_8));
				return new GenericJsonRecord(schemaVersion, fields, jn);
			}
			catch (IOException ioe)
			{
				throw new SchemaSerializationException(ioe);
			}
		}

		public override GenericRecord read(Stream inputStream)
		{
			try
			{
				JsonNode jn = objectMapper.readTree(inputStream);
				return new GenericJsonRecord(schemaVersion, fields, jn);
			}
			catch (IOException ioe)
			{
				throw new SchemaSerializationException(ioe);
			}
			finally
			{
				try
				{
					inputStream.Close();
				}
				catch (IOException e)
				{
					log.error("GenericJsonReader close inputStream close error", e.Message);
				}
			}
		}

		private static readonly Logger log = LoggerFactory.getLogger(typeof(GenericJsonReader));
	}

}