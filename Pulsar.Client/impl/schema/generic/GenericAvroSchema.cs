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
	using Slf4j = lombok.@extern.slf4j.Slf4j;
	using Schema = org.apache.avro.Schema;
	using GenericRecord = org.apache.pulsar.client.api.schema.GenericRecord;
	using GenericRecordBuilder = org.apache.pulsar.client.api.schema.GenericRecordBuilder;
	using SchemaReader = org.apache.pulsar.client.api.schema.SchemaReader;
	using BytesSchemaVersion = org.apache.pulsar.common.protocol.schema.BytesSchemaVersion;
	using SchemaInfo = org.apache.pulsar.common.schema.SchemaInfo;

	/// <summary>
	/// A generic avro schema.
	/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Slf4j public class GenericAvroSchema extends GenericSchemaImpl
	public class GenericAvroSchema : GenericSchemaImpl
	{

		public GenericAvroSchema(SchemaInfo schemaInfo) : this(schemaInfo, true)
		{
		}

		internal GenericAvroSchema(SchemaInfo schemaInfo, bool useProvidedSchemaAsReaderSchema) : base(schemaInfo, useProvidedSchemaAsReaderSchema)
		{
			Reader = new GenericAvroReader(schema);
			Writer = new GenericAvroWriter(schema);
		}

		public override GenericRecordBuilder newRecordBuilder()
		{
			return new AvroRecordBuilderImpl(this);
		}

		public override bool supportSchemaVersioning()
		{
			return true;
		}

		protected internal override SchemaReader<GenericRecord> loadReader(BytesSchemaVersion schemaVersion)
		{
			 SchemaInfo schemaInfo = getSchemaInfoByVersion(schemaVersion.get());
			 if (schemaInfo != null)
			 {
				 log.info("Load schema reader for version({}), schema is : {}", SchemaUtils.getStringSchemaVersion(schemaVersion.get()), schemaInfo);
				 Schema writerSchema = parseAvroSchema(schemaInfo.SchemaDefinition);
				 Schema readerSchema = useProvidedSchemaAsReaderSchema ? schema : writerSchema;
				 return new GenericAvroReader(writerSchema, readerSchema, schemaVersion.get());
			 }
			 else
			 {
				 log.warn("No schema found for version({}), use latest schema : {}", SchemaUtils.getStringSchemaVersion(schemaVersion.get()), this.schemaInfo);
				 return reader;
			 }
		}

	}

}