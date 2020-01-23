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
	using Slf4j = lombok.@extern.slf4j.Slf4j;
	using Schema = org.apache.avro.Schema;
	using GenericRecord = SharpPulsar.Api.Schema.GenericRecord;
	using GenericRecordBuilder = SharpPulsar.Api.Schema.GenericRecordBuilder;
	using SharpPulsar.Api.Schema;
	using BytesSchemaVersion = Org.Apache.Pulsar.Common.Protocol.Schema.BytesSchemaVersion;
	using SchemaInfo = Org.Apache.Pulsar.Common.Schema.SchemaInfo;

	/// <summary>
	/// A generic avro schema.
	/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Slf4j public class GenericAvroSchema extends GenericSchemaImpl
	public class GenericAvroSchema : GenericSchemaImpl
	{

		public GenericAvroSchema(SchemaInfo SchemaInfo) : this(SchemaInfo, true)
		{
		}

		public GenericAvroSchema(SchemaInfo SchemaInfo, bool UseProvidedSchemaAsReaderSchema) : base(SchemaInfo, UseProvidedSchemaAsReaderSchema)
		{
			Reader = new GenericAvroReader(schema);
			Writer = new GenericAvroWriter(schema);
		}

		public override GenericRecordBuilder NewRecordBuilder()
		{
			return new AvroRecordBuilderImpl(this);
		}

		public override bool SupportSchemaVersioning()
		{
			return true;
		}

		public override SchemaReader<GenericRecord> LoadReader(BytesSchemaVersion SchemaVersion)
		{
			 SchemaInfo SchemaInfo = getSchemaInfoByVersion(SchemaVersion.get());
			 if (SchemaInfo != null)
			 {
				 log.info("Load schema reader for version({}), schema is : {}", SchemaUtils.getStringSchemaVersion(SchemaVersion.get()), SchemaInfo);
				 Schema WriterSchema = parseAvroSchema(SchemaInfo.SchemaDefinition);
				 Schema ReaderSchema = UseProvidedSchemaAsReaderSchema ? schema : WriterSchema;
				 return new GenericAvroReader(WriterSchema, ReaderSchema, SchemaVersion.get());
			 }
			 else
			 {
				 log.warn("No schema found for version({}), use latest schema : {}", SchemaUtils.getStringSchemaVersion(SchemaVersion.get()), this.schemaInfo);
				 return reader;
			 }
		}

	}

}