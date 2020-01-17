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
	using Field = Api.Schema.Field;
	using GenericRecord = Api.Schema.GenericRecord;
	using GenericRecordBuilder = Api.Schema.GenericRecordBuilder;
	using BytesSchemaVersion = org.apache.pulsar.common.protocol.schema.BytesSchemaVersion;
	using ISchemaInfo = org.apache.pulsar.common.schema.SchemaInfo;
    using Pulsar.Api.Schema;

    /// <summary>
    /// A generic json schema.
    /// </summary>
    //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
    //ORIGINAL LINE: @Slf4j class GenericJsonSchema extends GenericSchemaImpl
    internal class GenericJsonSchema : GenericSchemaImpl
	{

		public GenericJsonSchema(SchemaInfo schemaInfo) : this(schemaInfo, true)
		{
		}

		internal GenericJsonSchema(SchemaInfo schemaInfo, bool useProvidedSchemaAsReaderSchema) : base(schemaInfo, useProvidedSchemaAsReaderSchema)
		{
			Writer = new GenericJsonWriter();
			Reader = new GenericJsonReader(fields);
		}

		protected internal SchemaReader<GenericRecord> LoadReader(BytesSchemaVersion schemaVersion)
		{
			SchemaInfo schemaInfo = getSchemaInfoByVersion(schemaVersion.get());
			if (schemaInfo != null)
			{
				log.info("Load schema reader for version({}), schema is : {}", SchemaUtils.getStringSchemaVersion(schemaVersion.get()), schemaInfo.SchemaDefinition);
				Schema readerSchema;
				if (useProvidedSchemaAsReaderSchema)
				{
					readerSchema = schema;
				}
				else
				{
					readerSchema = ParseAvroSchema(schemaInfo.SchemaDefinition);
				}
				return new GenericJsonReader(schemaVersion.get(), readerSchema.Fields.Select(f => new Field(f.name(), f.pos())).ToList());
			}
			else
			{
				log.warn("No schema found for version({}), use latest schema : {}", SchemaUtils.getStringSchemaVersion(schemaVersion.get()), this.schemaInfo.SchemaDefinition);
				return reader;
			}
		}

		public GenericRecordBuilder NewRecordBuilder()
		{
			throw new System.NotSupportedException("Json Schema doesn't support record builder yet");
		}
	}

}