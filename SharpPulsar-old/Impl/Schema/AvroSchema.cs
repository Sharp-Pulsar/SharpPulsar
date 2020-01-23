using Pulsar.Client.Impl.Schema.Reader;
using Pulsar.Client.Impl.Schema.Writer;
using SharpPulsar.Common.Protocol.Schema;
using SharpPulsar.Common.Schema;
using SharpPulsar.Interface.Schema;
using System;
using System.Collections.Generic;

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
namespace SharpPulsar.Impl.Schema
{
	/// <summary>
	/// An AVRO schema implementation.
	/// </summary>
	public class AvroSchema<T> : StructSchema<T>
	{
		private new static readonly Logger LOG = LoggerFactory.getLogger(typeof(AvroSchema));

		// the aim to fix avro's bug
		// https://issues.apache.org/jira/browse/AVRO-1891 bug address explain
		// fix the avro logical type read and write
		static AvroSchema()
		{
			ReflectData reflectDataAllowNull = ReflectData.AllowNull.get();

			reflectDataAllowNull.addLogicalTypeConversion(new Conversions.DecimalConversion());
			reflectDataAllowNull.addLogicalTypeConversion(new TimeConversions.DateConversion());
			reflectDataAllowNull.addLogicalTypeConversion(new TimeConversions.TimeMillisConversion());
			reflectDataAllowNull.addLogicalTypeConversion(new TimeConversions.TimeMicrosConversion());
			reflectDataAllowNull.addLogicalTypeConversion(new TimeConversions.TimestampMillisConversion());
			reflectDataAllowNull.addLogicalTypeConversion(new TimeConversions.TimestampMicrosConversion());

			ReflectData reflectDataNotAllowNull = ReflectData.get();

			reflectDataNotAllowNull.addLogicalTypeConversion(new Conversions.DecimalConversion());
			reflectDataNotAllowNull.addLogicalTypeConversion(new TimeConversions.DateConversion());
			reflectDataNotAllowNull.addLogicalTypeConversion(new TimeConversions.TimeMillisConversion());
			reflectDataNotAllowNull.addLogicalTypeConversion(new TimeConversions.TimeMicrosConversion());
			reflectDataNotAllowNull.addLogicalTypeConversion(new TimeConversions.TimestampMillisConversion());
			reflectDataNotAllowNull.addLogicalTypeConversion(new TimeConversions.TimestampMicrosConversion());
		}

		private AvroSchema(SchemaInfo schemaInfo) : base(schemaInfo)
		{
			Reader = new AvroReader<T>(schema);
			Writer = new AvroWriter<T>(schema);
		}

		public bool SupportSchemaVersioning()
		{
			return true;
		}

		public static AvroSchema<T> Of<T>(ISchemaDefinition<T> schemaDefinition)
		{
			return new AvroSchema<T>(ParseSchemaInfo(schemaDefinition, SchemaType.AVRO));
		}

		public static AvroSchema<T> Of<T>(Type pojo)
		{
			return AvroSchema.Of(ISchemaDefinition<T>.Builder().WithPojo(pojo).Build());
		}

		public static AvroSchema<T> Of<T>(Type pojo, IDictionary<string, string> properties)
		{
			ISchemaDefinition<T> schemaDefinition = ISchemaDefinition<T>.Builder().WithPojo(pojo).WithProperties(properties).Build();
			return new AvroSchema<T>(ParseSchemaInfo(schemaDefinition, SchemaType.AVRO));
		}

		protected internal ISchemaReader<T> LoadReader(BytesSchemaVersion schemaVersion)
		{
			SchemaInfo schemaInfo = getSchemaInfoByVersion(schemaVersion.Get());
			if (schemaInfo != null)
			{
				log.info("Load schema reader for version({}), schema is : {}", SchemaUtils.getStringSchemaVersion(schemaVersion.get()), schemaInfo.SchemaDefinition);
				return new AvroReader<T>(parseAvroSchema(schemaInfo.SchemaDefinition), schema);
			}
			else
			{
				log.warn("No schema found for version({}), use latest schema : {}", SchemaUtils.getStringSchemaVersion(schemaVersion.get()), this.schemaInfo.SchemaDefinition);
				return reader;
			}
		}

	}

}