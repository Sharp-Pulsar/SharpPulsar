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
	using DotNetty.Buffers;
	using SharpPulsar.Common.Protocol.Schema;
    using SharpPulsar.Common.Schema;
    using SharpPulsar.Impl.Schema;
    using SharpPulsar.Interface.Schema;
    using Schema = Avro.Schema;

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

		public override IGenericRecord Decode(IByteBuffer byteBuf)
		{
			throw new System.NotImplementedException();
		}

		public IGenericRecordBuilder NewRecordBuilder()
		{
			return new AvroRecordBuilderImpl(this);
		}

		public bool SupportSchemaVersioning()
		{
			return true;
		}

		protected internal override ISchemaReader<IGenericRecord> LoadReader(BytesSchemaVersion schemaVersion)
		{
			 SchemaInfo schemaInfo = GetSchemaInfoByVersion(schemaVersion.Get());
			 if (schemaInfo != null)
			 {
				 //log.info("Load schema reader for version({}), schema is : {}", SchemaUtils.GetStringSchemaVersion(schemaVersion.Get()), schemaInfo);
				 Schema writerSchema = ParseAvroSchema(schemaInfo.SchemaDefinition);
				 Schema readerSchema = useProvidedSchemaAsReaderSchema ? schema : writerSchema;
				 return new GenericAvroReader(writerSchema, readerSchema, schemaVersion.Get());
			 }
			 else
			 {
				 //log.warn("No schema found for version({}), use latest schema : {}", SchemaUtils.GetStringSchemaVersion(schemaVersion.Get()), this.schemaInfo);
				 return reader;
			 }
		}

	}

}