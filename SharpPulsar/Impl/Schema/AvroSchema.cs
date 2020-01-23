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
	using Slf4j = lombok.@extern.slf4j.Slf4j;
	using Conversions = org.apache.avro.Conversions;
	using TimeConversions = org.apache.avro.data.TimeConversions;
	using ReflectData = org.apache.avro.reflect.ReflectData;
	using SharpPulsar.Api.Schema;
	using SharpPulsar.Api.Schema;
	using SharpPulsar.Impl.Schema.Reader;
	using SharpPulsar.Impl.Schema.Writer;
	using BytesSchemaVersion = Org.Apache.Pulsar.Common.Protocol.Schema.BytesSchemaVersion;
	using SchemaInfo = Org.Apache.Pulsar.Common.Schema.SchemaInfo;
	using SchemaType = Org.Apache.Pulsar.Common.Schema.SchemaType;
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

		//      the aim to fix avro's bug
	//      https://issues.apache.org/jira/browse/AVRO-1891  bug address explain
	//      fix the avro logical type read and write
		static AvroSchema()
		{
			try
			{
				ReflectData ReflectDataAllowNull = ReflectData.AllowNull.get();

				ReflectDataAllowNull.addLogicalTypeConversion(new Conversions.DecimalConversion());
				ReflectDataAllowNull.addLogicalTypeConversion(new TimeConversions.DateConversion());
				ReflectDataAllowNull.addLogicalTypeConversion(new TimeConversions.LossyTimeMicrosConversion());
				ReflectDataAllowNull.addLogicalTypeConversion(new TimeConversions.LossyTimestampMicrosConversion());
				ReflectDataAllowNull.addLogicalTypeConversion(new TimeConversions.TimeMicrosConversion());
				ReflectDataAllowNull.addLogicalTypeConversion(new TimeConversions.TimestampMicrosConversion());
				ReflectDataAllowNull.addLogicalTypeConversion(new TimeConversions.TimestampConversion());
				ReflectDataAllowNull.addLogicalTypeConversion(new TimeConversions.TimeConversion());

				ReflectData ReflectDataNotAllowNull = ReflectData.get();

				ReflectDataNotAllowNull.addLogicalTypeConversion(new Conversions.DecimalConversion());
				ReflectDataNotAllowNull.addLogicalTypeConversion(new TimeConversions.DateConversion());
				ReflectDataNotAllowNull.addLogicalTypeConversion(new TimeConversions.TimestampConversion());
				ReflectDataNotAllowNull.addLogicalTypeConversion(new TimeConversions.LossyTimeMicrosConversion());
				ReflectDataNotAllowNull.addLogicalTypeConversion(new TimeConversions.LossyTimestampMicrosConversion());
				ReflectDataNotAllowNull.addLogicalTypeConversion(new TimeConversions.TimeMicrosConversion());
				ReflectDataNotAllowNull.addLogicalTypeConversion(new TimeConversions.TimestampMicrosConversion());
				ReflectDataNotAllowNull.addLogicalTypeConversion(new TimeConversions.TimeConversion());
			}
			catch (Exception)
			{
				if (LOG.DebugEnabled)
				{
					LOG.debug("Avro logical types are not available. If you are going to use avro logical types, " + "you can include `joda-time` in your dependency.");
				}
			}
		}

		private AvroSchema(SchemaInfo SchemaInfo) : base(SchemaInfo)
		{
			Reader = new AvroReader<>(Schema);
			Writer = new AvroWriter<>(Schema);
		}

		public override bool SupportSchemaVersioning()
		{
			return true;
		}

		public static AvroSchema<T> Of<T>(SchemaDefinition<T> SchemaDefinition)
		{
			return new AvroSchema<T>(ParseSchemaInfo(SchemaDefinition, SchemaType.AVRO));
		}

		public static AvroSchema<T> Of<T>(Type Pojo)
		{
			return AvroSchema.Of(SchemaDefinition.builder<T>().withPojo(Pojo).build());
		}

		public static AvroSchema<T> Of<T>(Type Pojo, IDictionary<string, string> Properties)
		{
			SchemaDefinition<T> SchemaDefinition = SchemaDefinition.builder<T>().withPojo(Pojo).withProperties(Properties).build();
			return new AvroSchema<T>(ParseSchemaInfo(SchemaDefinition, SchemaType.AVRO));
		}

		public override SchemaReader<T> LoadReader(BytesSchemaVersion SchemaVersion)
		{
			SchemaInfo SchemaInfo = GetSchemaInfoByVersion(SchemaVersion.get());
			if (SchemaInfo != null)
			{
				log.info("Load schema reader for version({}), schema is : {}", SchemaUtils.GetStringSchemaVersion(SchemaVersion.get()), SchemaInfo.SchemaDefinition);
				return new AvroReader<T>(ParseAvroSchema(SchemaInfo.SchemaDefinition), Schema);
			}
			else
			{
				log.warn("No schema found for version({}), use latest schema : {}", SchemaUtils.GetStringSchemaVersion(SchemaVersion.get()), this.SchemaInfoConflict.SchemaDefinition);
				return ReaderConflict;
			}
		}

	}

}