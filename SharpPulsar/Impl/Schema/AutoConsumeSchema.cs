using System;
using System.Threading;

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
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.google.common.@base.Preconditions.checkState;

	using Slf4j = lombok.@extern.slf4j.Slf4j;
	using SharpPulsar.Api;
	using SchemaSerializationException = SharpPulsar.Api.SchemaSerializationException;
	using GenericRecord = SharpPulsar.Api.Schema.GenericRecord;
	using SharpPulsar.Api.Schema;
	using SchemaInfoProvider = SharpPulsar.Api.Schema.SchemaInfoProvider;
	using GenericSchemaImpl = SharpPulsar.Impl.Schema.Generic.GenericSchemaImpl;
	using Org.Apache.Pulsar.Common.Schema;
	using SchemaInfo = Org.Apache.Pulsar.Common.Schema.SchemaInfo;
	using SchemaType = Org.Apache.Pulsar.Common.Schema.SchemaType;

	/// <summary>
	/// Auto detect schema.
	/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Slf4j public class AutoConsumeSchema implements SharpPulsar.api.Schema<SharpPulsar.api.schema.GenericRecord>
	public class AutoConsumeSchema : Schema<GenericRecord>
	{

		private Schema<GenericRecord> schema;

		private string topicName;

		private string componentName;

		private SchemaInfoProvider schemaInfoProvider;

		public virtual Schema<GenericRecord> Schema
		{
			set
			{
				this.schema = value;
			}
		}

		private void EnsureSchemaInitialized()
		{
			checkState(null != schema, "Schema is not initialized before used");
		}

		public override void Validate(sbyte[] Message)
		{
			EnsureSchemaInitialized();

			schema.Validate(Message);
		}

		public override bool SupportSchemaVersioning()
		{
			return true;
		}

		public override sbyte[] Encode(GenericRecord Message)
		{
			EnsureSchemaInitialized();

			return schema.Encode(Message);
		}

		public override GenericRecord Decode(sbyte[] Bytes, sbyte[] SchemaVersion)
		{
			if (schema == null)
			{
				SchemaInfo SchemaInfo = null;
				try
				{
					SchemaInfo = schemaInfoProvider.LatestSchema.get();
				}
				catch (Exception e) when (e is InterruptedException || e is ExecutionException)
				{
					if (e is InterruptedException)
					{
						Thread.CurrentThread.Interrupt();
					}
					log.error("Con't get last schema for topic {} use AutoConsumeSchema", topicName);
					throw new SchemaSerializationException(e.Cause);
				}
				schema = GenerateSchema(SchemaInfo);
				schema.SchemaInfoProvider = schemaInfoProvider;
				log.info("Configure {} schema for topic {} : {}", componentName, topicName, SchemaInfo.SchemaDefinition);
			}
			EnsureSchemaInitialized();
			return schema.Decode(Bytes, SchemaVersion);
		}

		public virtual SchemaInfoProvider SchemaInfoProvider
		{
			set
			{
				if (schema == null)
				{
					this.schemaInfoProvider = value;
				}
				else
				{
					schema.SchemaInfoProvider = value;
				}
			}
		}

		public virtual SchemaInfo SchemaInfo
		{
			get
			{
				if (schema == null)
				{
					return null;
				}
				return schema.SchemaInfo;
			}
		}

		public override bool RequireFetchingSchemaInfo()
		{
			return true;
		}

		public override void ConfigureSchemaInfo(string TopicName, string ComponentName, SchemaInfo SchemaInfo)
		{
			this.topicName = TopicName;
			this.componentName = ComponentName;
			if (SchemaInfo != null)
			{
				GenericSchema GenericSchema = GenerateSchema(SchemaInfo);
				Schema = GenericSchema;
				log.info("Configure {} schema for topic {} : {}", ComponentName, TopicName, SchemaInfo.SchemaDefinition);
			}
		}

		private GenericSchema GenerateSchema(SchemaInfo SchemaInfo)
		{
			if (SchemaInfo.Type != SchemaType.AVRO && SchemaInfo.Type != SchemaType.JSON)
			{
				throw new Exception("Currently auto consume only works for topics with avro or json schemas");
			}
			// when using `AutoConsumeSchema`, we use the schema associated with the messages as schema reader
			// to decode the messages.
			return GenericSchemaImpl.of(SchemaInfo, false);
		}

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: public static SharpPulsar.api.Schema<?> getSchema(org.apache.pulsar.common.schema.SchemaInfo schemaInfo)
		public static Schema<object> GetSchema(SchemaInfo SchemaInfo)
		{
			switch (SchemaInfo.Type)
			{
				case SchemaFields.INT8:
					return ByteSchema.Of();
				case SchemaFields.INT16:
					return ShortSchema.Of();
				case SchemaFields.INT32:
					return IntSchema.Of();
				case SchemaFields.INT64:
					return LongSchema.Of();
				case SchemaFields.STRING:
					return StringSchema.Utf8();
				case SchemaFields.FLOAT:
					return FloatSchema.Of();
				case SchemaFields.DOUBLE:
					return DoubleSchema.Of();
				case BOOLEAN:
					return BooleanSchema.Of();
				case SchemaFields.BYTES:
					return BytesSchema.Of();
				case SchemaFields.DATE:
					return DateSchema.Of();
				case SchemaFields.TIME:
					return TimeSchema.Of();
				case SchemaFields.TIMESTAMP:
					return TimestampSchema.Of();
				case JSON:
				case AVRO:
					return GenericSchemaImpl.of(SchemaInfo);
				case KEY_VALUE:
					KeyValue<SchemaInfo, SchemaInfo> KvSchemaInfo = KeyValueSchemaInfo.DecodeKeyValueSchemaInfo(SchemaInfo);
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: SharpPulsar.api.Schema<?> keySchema = getSchema(kvSchemaInfo.getKey());
					Schema<object> KeySchema = GetSchema(KvSchemaInfo.Key);
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: SharpPulsar.api.Schema<?> valueSchema = getSchema(kvSchemaInfo.getValue());
					Schema<object> ValueSchema = GetSchema(KvSchemaInfo.Value);
					return KeyValueSchema.Of(KeySchema, ValueSchema);
				default:
					throw new System.ArgumentException("Retrieve schema instance from schema info for type '" + SchemaInfo.Type + "' is not supported yet");
			}
		}
	}

}