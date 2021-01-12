using System;
using Microsoft.Extensions.Logging;
using SharpPulsar.Common.Schema;
using SharpPulsar.Impl.Schema.Generic;
using SharpPulsar.Shared;
using SharpPulsar.Interfaces.Interceptor;
using SharpPulsar.Schema;
using SharpPulsar.Interfaces.ISchema;
using SharpPulsar.Interfaces;

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
	/// Auto detect schema.
	/// </summary>
	public class AutoConsumeSchema : ISchema<IGenericRecord>
	{

		private ISchema<IGenericRecord> _schema;

		private string _topicName;

		private string _componentName;

		private ISchemaInfoProvider _schemaInfoProvider;

		public virtual ISchema<IGenericRecord> Schema
		{
			set => this._schema = value;
        }

		private void EnsureSchemaInitialized()
		{
			if(null == _schema)
                throw new NullReferenceException("Schema is not initialized before used");
		}

		public void Validate(sbyte[] message)
		{
			EnsureSchemaInitialized();

			_schema.Validate(message);
		}

		public bool SupportSchemaVersioning()
		{
			return true;
		}

		public sbyte[] Encode(IGenericRecord message)
		{
			if(!(message is IGenericRecord))
				throw  new ArgumentException($"{message.GetType()} is not IGenericRecord");
			EnsureSchemaInitialized();

			return _schema.Encode(message);
		}
		
		public virtual ISchemaInfoProvider SchemaInfoProvider
		{
			set
			{
				if (_schema == null)
				{
					this._schemaInfoProvider = value;
				}
				else
				{
					_schema.SchemaInfoProvider = value;
				}
			}
		}

		public virtual ISchemaInfo SchemaInfo => _schema?.SchemaInfo;

        public bool RequireFetchingSchemaInfo()
		{
			return true;
		}

		public  void ConfigureSchemaInfo(string topicName, string componentName, SchemaInfo schemaInfo)
		{
			this._topicName = topicName;
			this._componentName = componentName;
            if (schemaInfo == null) return;
            var genericSchema = GenerateSchema(schemaInfo);
            Schema = genericSchema;
            Log.LogInformation("Configure {} schema for topic {} : {}", componentName, topicName, schemaInfo.SchemaDefinition);
        }

		private IGenericSchema<IGenericRecord> GenerateSchema(SchemaInfo schemaInfo)
		{
			if (schemaInfo.Type != SchemaType.AVRO && schemaInfo.Type != SchemaType.JSON)
			{
				throw new System.Exception("Currently auto consume only works for topics with avro or json schemas");
			}
			// when using `AutoConsumeSchema`, we use the schema associated with the messages as schema reader
			// to decode the messages.
			return GenericSchemaImpl.Of(schemaInfo, false);
		}
		public ISchema<IGenericRecord> Clone()
		{
			ISchema<IGenericRecord> schema = ISchema<IGenericRecord>.AutoConsume();
			if (_schema != null)
			{
				schema.ConfigureSchemaInfo(_topicName, _componentName, _schema.SchemaInfo);
			}
			else
			{
				schema.ConfigureSchemaInfo(_topicName, _componentName, null);
			}
			if (_schemaInfoProvider != null)
			{
				schema.SchemaInfoProvider = _schemaInfoProvider;
			}
			return schema;
		}
		public static object GetSchema(SchemaInfo schemaInfo)
		{
			switch (schemaInfo.Type.InnerEnumValue)
			{ 
				case SchemaType.InnerEnum.INT32:
					return BytesSchema.Of();
				case SchemaType.InnerEnum.AVRO:
				case SchemaType.InnerEnum.JSON:
					return GenericSchemaImpl.Of(schemaInfo);
				default:
					throw new ArgumentException("Retrieve schema instance from schema info for type '" + schemaInfo.Type + "' is not supported yet");
			}
			/*switch (schemaInfo.Type)
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
			case INSTANT:
				return InstantSchema.of();
			case LOCAL_DATE:
				return LocalDateSchema.of();
			case LOCAL_TIME:
				return LocalTimeSchema.of();
			case LOCAL_DATE_TIME:
				return LocalDateTimeSchema.of();
			case JSON:
				return GenericJsonSchema.of(schemaInfo);
			case AVRO:
				return GenericAvroSchema.of(schemaInfo);
			case PROTOBUF_NATIVE:
				return GenericProtobufNativeSchema.of(schemaInfo);
			case KEY_VALUE:
				KeyValue<SchemaInfo, SchemaInfo> kvSchemaInfo = KeyValueSchemaInfo.decodeKeyValueSchemaInfo(schemaInfo);
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in C#:
//ORIGINAL LINE: Schema<?> keySchema = getSchema(kvSchemaInfo.getKey());
				Schema<object> keySchema = getSchema(kvSchemaInfo.Key);
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in C#:
//ORIGINAL LINE: Schema<?> valueSchema = getSchema(kvSchemaInfo.getValue());
				Schema<object> valueSchema = getSchema(kvSchemaInfo.Value);
				return KeyValueSchema.of(keySchema, valueSchema);
			default:
				throw new System.ArgumentException("Retrieve schema instance from schema info for type '" + schemaInfo.Type + "' is not supported yet");
		}
*/
		}

        object ICloneable.Clone()
        {
            throw new NotImplementedException();
        }

        private static readonly ILogger Log = Utility.Log.Logger.CreateLogger<AutoConsumeSchema>();
	}

}