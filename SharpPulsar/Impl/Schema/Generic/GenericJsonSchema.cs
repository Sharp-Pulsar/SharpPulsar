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

using System.Linq;
using Avro;
using Microsoft.Extensions.Logging;
using SharpPulsar.Common.Schema;
using SharpPulsar.Protocol.Schema;

namespace SharpPulsar.Impl.Schema.Generic
{
	using Field = Api.Schema.Field;
	using IGenericRecord = Api.Schema.IGenericRecord;
	using IGenericRecordBuilder = Api.Schema.IGenericRecordBuilder;
	using SharpPulsar.Api.Schema;

	/// <summary>
	/// A generic json schema.
	/// </summary>
	public abstract class GenericJsonSchema : GenericSchemaImpl
	{
		private static readonly ILogger _log = new LoggerFactory().CreateLogger(typeof(GenericJsonSchema));

        protected GenericJsonSchema(SchemaInfo schemaInfo) : this(schemaInfo, true)
		{
		}

        protected GenericJsonSchema(SchemaInfo schemaInfo, bool useProvidedSchemaAsReaderSchema) : base(schemaInfo, useProvidedSchemaAsReaderSchema)
		{
			Writer = new GenericJsonWriter();
			
			Reader = new GenericJsonReader(Fields);
		}

		public override ISchemaReader<IGenericRecord> LoadReader(BytesSchemaVersion schemaVersion)
		{
			var schemaInfo = GetSchemaInfoByVersion(schemaVersion.Get());
			if (schemaInfo != null)
			{
				_log.LogInformation("Load schema reader for version({}), schema is : {}", SchemaUtils.GetStringSchemaVersion(schemaVersion.Get()), schemaInfo.SchemaDefinition);
				RecordSchema readerSchema;
				if (UseProvidedSchemaAsReaderSchema)
				{
					readerSchema = (RecordSchema)Schema;
				}
				else
				{
					readerSchema = (RecordSchema)ParseAvroSchema(schemaInfo.SchemaDefinition);
				}
				return new GenericJsonReader(schemaVersion.Get(), readerSchema.Fields.Select(f => new Field(){Name = f.Name, Index = f.Pos}).ToList());
			}
			else
			{
				_log.LogWarning("No schema found for version({}), use latest schema : {}", SchemaUtils.GetStringSchemaVersion(schemaVersion.Get()), SchemaInfo.SchemaDefinition);
				return Reader;
			}
		}

		public override IGenericRecordBuilder NewRecordBuilder()
		{
			throw new System.NotSupportedException("Json Schema doesn't support record builder yet");
		}
	}

}