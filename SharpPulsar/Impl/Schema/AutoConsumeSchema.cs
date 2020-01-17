using Pulsar.Client.Impl.Schema.Generic;
using SharpPulsar.Common.Schema;
using SharpPulsar.Impl.Schema;
using SharpPulsar.Interface.Schema;
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
namespace Pulsar.Client.Impl.Schema
{
	using GenericSchemaImpl = GenericSchemaImpl;

	/// <summary>
	/// Auto detect schema.
	/// </summary>
	public class AutoConsumeSchema : ISchema<IGenericRecord>
	{

		private ISchema<IGenericRecord> schema;

		private string topicName;

		private string componentName;

		private ISchemaInfoProvider schemaInfoProvider;

		public virtual ISchema<IGenericRecord> Schema
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

		public void Validate(sbyte[] message)
		{
			EnsureSchemaInitialized();

			schema.Validate(message);
		}

		public bool SupportSchemaVersioning()
		{
			return true;
		}

		public sbyte[] Encode(IGenericRecord message)
		{
			EnsureSchemaInitialized();

			return schema.Encode(message);
		}

		public IGenericRecord Decode(sbyte[] bytes, sbyte[] schemaVersion)
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
				schema = GenerateSchema(schemaInfo);
				schema.SchemaInfoProvider = schemaInfoProvider;
				log.info("Configure {} schema for topic {} : {}", componentName, topicName, schemaInfo.SchemaDefinition);
			}
			EnsureSchemaInitialized();
			return schema.Decode(bytes, schemaVersion);
		}

		public ISchemaInfoProvider SchemaInfoProvider
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

		public SchemaInfo SchemaInfo
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

		public  bool RequireFetchingSchemaInfo()
		{
			return true;
		}

		public void ConfigureSchemaInfo(string topicName, string componentName, SchemaInfo schemaInfo)
		{
			this.topicName = topicName;
			this.componentName = componentName;
			if (schemaInfo != null)
			{
				IGenericSchema genericSchema = GenerateSchema(schemaInfo);
				Schema = genericSchema;
				log.info("Configure {} schema for topic {} : {}", componentName, topicName, schemaInfo.SchemaDefinition);
			}
		}

		private IGenericSchema<T> GenerateSchema(SchemaInfo schemaInfo)
		{
			if (schemaInfo.Type != SchemaType.AVRO && schemaInfo.Type != SchemaType.JSON)
			{
				throw new Exception("Currently auto consume only works for topics with avro or json schemas");
			}
			// when using `AutoConsumeSchema`, we use the schema associated with the messages as schema reader
			// to decode the messages.
			return GenericSchemaImpl.of(schemaInfo, false);
		}
		public static ISchema<T> GetSchema(SchemaInfo schemaInfo)
		{
			switch (schemaInfo.Type)
			{
				case INT8:
					return ByteSchema.Of();
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
					return BoolSchema.of();
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
					return GenericSchemaImpl.Of(schemaInfo);
				case KEY_VALUE:
					KeyValue<SchemaInfo, SchemaInfo> kvSchemaInfo = KeyValueSchemaInfo.decodeKeyValueSchemaInfo(schemaInfo);
					ISchema<object> keySchema = GetSchema(kvSchemaInfo.Key);
					ISchema<object> valueSchema = GetSchema(kvSchemaInfo.Value);
					return KeyValueSchema<K,V>.Of(keySchema, valueSchema);
				default:
					throw new System.ArgumentException("Retrieve schema instance from schema info for type '" + schemaInfo.Type + "' is not supported yet");
			}
		}
	}

}