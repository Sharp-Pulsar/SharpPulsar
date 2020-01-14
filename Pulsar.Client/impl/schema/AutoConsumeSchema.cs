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
namespace org.apache.pulsar.client.impl.schema
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.google.common.@base.Preconditions.checkState;

	using Slf4j = lombok.@extern.slf4j.Slf4j;
	using Schema = org.apache.pulsar.client.api.Schema;
	using SchemaSerializationException = org.apache.pulsar.client.api.SchemaSerializationException;
	using GenericRecord = org.apache.pulsar.client.api.schema.GenericRecord;
	using GenericSchema = org.apache.pulsar.client.api.schema.GenericSchema;
	using SchemaInfoProvider = org.apache.pulsar.client.api.schema.SchemaInfoProvider;
	using GenericSchemaImpl = org.apache.pulsar.client.impl.schema.generic.GenericSchemaImpl;
	using KeyValue = org.apache.pulsar.common.schema.KeyValue;
	using SchemaInfo = org.apache.pulsar.common.schema.SchemaInfo;
	using SchemaType = org.apache.pulsar.common.schema.SchemaType;

	/// <summary>
	/// Auto detect schema.
	/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Slf4j public class AutoConsumeSchema implements org.apache.pulsar.client.api.Schema<org.apache.pulsar.client.api.schema.GenericRecord>
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

		private void ensureSchemaInitialized()
		{
			checkState(null != schema, "Schema is not initialized before used");
		}

		public override void validate(sbyte[] message)
		{
			ensureSchemaInitialized();

			schema.validate(message);
		}

		public override bool supportSchemaVersioning()
		{
			return true;
		}

		public override sbyte[] encode(GenericRecord message)
		{
			ensureSchemaInitialized();

			return schema.encode(message);
		}

		public override GenericRecord decode(sbyte[] bytes, sbyte[] schemaVersion)
		{
			if (schema == null)
			{
				SchemaInfo schemaInfo = null;
				try
				{
					schemaInfo = schemaInfoProvider.LatestSchema.get();
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
				schema = generateSchema(schemaInfo);
				schema.SchemaInfoProvider = schemaInfoProvider;
				log.info("Configure {} schema for topic {} : {}", componentName, topicName, schemaInfo.SchemaDefinition);
			}
			ensureSchemaInitialized();
			return schema.decode(bytes, schemaVersion);
		}

		public override SchemaInfoProvider SchemaInfoProvider
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

		public override SchemaInfo SchemaInfo
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

		public override bool requireFetchingSchemaInfo()
		{
			return true;
		}

		public override void configureSchemaInfo(string topicName, string componentName, SchemaInfo schemaInfo)
		{
			this.topicName = topicName;
			this.componentName = componentName;
			if (schemaInfo != null)
			{
				GenericSchema genericSchema = generateSchema(schemaInfo);
				Schema = genericSchema;
				log.info("Configure {} schema for topic {} : {}", componentName, topicName, schemaInfo.SchemaDefinition);
			}
		}

		private GenericSchema generateSchema(SchemaInfo schemaInfo)
		{
			if (schemaInfo.Type != SchemaType.AVRO && schemaInfo.Type != SchemaType.JSON)
			{
				throw new Exception("Currently auto consume only works for topics with avro or json schemas");
			}
			// when using `AutoConsumeSchema`, we use the schema associated with the messages as schema reader
			// to decode the messages.
			return GenericSchemaImpl.of(schemaInfo, false);
		}

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: public static org.apache.pulsar.client.api.Schema<?> getSchema(org.apache.pulsar.common.schema.SchemaInfo schemaInfo)
		public static Schema<object> getSchema(SchemaInfo schemaInfo)
		{
			switch (schemaInfo.Type)
			{
				case INT8:
					return ByteSchema.of();
				case INT16:
					return ShortSchema.of();
				case INT32:
					return IntSchema.of();
				case INT64:
					return LongSchema.of();
				case STRING:
					return StringSchema.utf8();
				case FLOAT:
					return FloatSchema.of();
				case DOUBLE:
					return DoubleSchema.of();
				case BOOLEAN:
					return BooleanSchema.of();
				case BYTES:
					return BytesSchema.of();
				case DATE:
					return DateSchema.of();
				case TIME:
					return TimeSchema.of();
				case TIMESTAMP:
					return TimestampSchema.of();
				case JSON:
				case AVRO:
					return GenericSchemaImpl.of(schemaInfo);
				case KEY_VALUE:
					KeyValue<SchemaInfo, SchemaInfo> kvSchemaInfo = KeyValueSchemaInfo.decodeKeyValueSchemaInfo(schemaInfo);
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.apache.pulsar.client.api.Schema<?> keySchema = getSchema(kvSchemaInfo.getKey());
					Schema<object> keySchema = getSchema(kvSchemaInfo.Key);
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.apache.pulsar.client.api.Schema<?> valueSchema = getSchema(kvSchemaInfo.getValue());
					Schema<object> valueSchema = getSchema(kvSchemaInfo.Value);
					return KeyValueSchema.of(keySchema, valueSchema);
				default:
					throw new System.ArgumentException("Retrieve schema instance from schema info for type '" + schemaInfo.Type + "' is not supported yet");
			}
		}
	}

}