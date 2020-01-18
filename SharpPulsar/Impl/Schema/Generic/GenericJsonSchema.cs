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
    using SharpPulsar.Common.Protocol.Schema;
    using SharpPulsar.Common.Schema;
    using SharpPulsar.Impl.Schema;
    using SharpPulsar.Interface.Schema;
    using Schema = Avro.Schema;

    /// <summary>
    /// A generic json schema.
    /// </summary>
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

		protected internal ISchemaReader<IGenericRecord> LoadReader(BytesSchemaVersion schemaVersion)
		{
			SchemaInfo schemaInfo = GetSchemaInfoByVersion(schemaVersion.Get());
			if (schemaInfo != null)
			{
				log.info("Load schema reader for version({}), schema is : {}", SchemaUtils.GetStringSchemaVersion(schemaVersion.get()), schemaInfo.SchemaDefinition);
				Schema readerSchema;
				if (useProvidedSchemaAsReaderSchema)
				{
					readerSchema = schema;
				}
				else
				{
					readerSchema = ParseAvroSchema(schemaInfo.SchemaDefinition);
				}
				return new GenericJsonReader(schemaVersion.Get(), readerSchema.Fields.Select(f => new Field(f.name(), f.pos())).ToList());
			}
			else
			{
				log.warn("No schema found for version({}), use latest schema : {}", SchemaUtils.GetStringSchemaVersion(schemaVersion.get()), this.schemaInfo.SchemaDefinition);
				return reader;
			}
		}

		public IGenericRecordBuilder NewRecordBuilder()
		{
			throw new System.NotSupportedException("Json Schema doesn't support record builder yet");
		}
	}

}