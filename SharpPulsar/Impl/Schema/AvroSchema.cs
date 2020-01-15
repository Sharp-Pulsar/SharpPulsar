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
namespace org.apache.pulsar.client.impl.schema
{
	using Slf4j = lombok.@extern.slf4j.Slf4j;
	using Conversions = org.apache.avro.Conversions;
	using TimeConversions = org.apache.avro.data.TimeConversions;
	using ReflectData = org.apache.avro.reflect.ReflectData;
	using SchemaDefinition = org.apache.pulsar.client.api.schema.SchemaDefinition;
	using SchemaReader = org.apache.pulsar.client.api.schema.SchemaReader;
	using org.apache.pulsar.client.impl.schema.reader;
	using org.apache.pulsar.client.impl.schema.writer;
	using BytesSchemaVersion = org.apache.pulsar.common.protocol.schema.BytesSchemaVersion;
	using SchemaInfo = org.apache.pulsar.common.schema.SchemaInfo;
	using SchemaType = org.apache.pulsar.common.schema.SchemaType;
	using Logger = org.slf4j.Logger;
	using LoggerFactory = org.slf4j.LoggerFactory;

	/// <summary>
	/// An AVRO schema implementation.
	/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Slf4j public class AvroSchema<T> extends StructSchema<T>
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
			Reader = new AvroReader<>(schema);
			Writer = new AvroWriter<>(schema);
		}

		public override bool supportSchemaVersioning()
		{
			return true;
		}

		public static AvroSchema<T> of<T>(SchemaDefinition<T> schemaDefinition)
		{
			return new AvroSchema<T>(parseSchemaInfo(schemaDefinition, SchemaType.AVRO));
		}

		public static AvroSchema<T> of<T>(Type pojo)
		{
			return AvroSchema.of(SchemaDefinition.builder<T>().withPojo(pojo).build());
		}

		public static AvroSchema<T> of<T>(Type pojo, IDictionary<string, string> properties)
		{
			SchemaDefinition<T> schemaDefinition = SchemaDefinition.builder<T>().withPojo(pojo).withProperties(properties).build();
			return new AvroSchema<T>(parseSchemaInfo(schemaDefinition, SchemaType.AVRO));
		}

		protected internal override SchemaReader<T> loadReader(BytesSchemaVersion schemaVersion)
		{
			SchemaInfo schemaInfo = getSchemaInfoByVersion(schemaVersion.get());
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