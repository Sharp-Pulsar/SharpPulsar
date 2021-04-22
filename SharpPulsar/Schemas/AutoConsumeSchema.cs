using System;
using Microsoft.Extensions.Logging;
using SharpPulsar.Shared;
using SharpPulsar.Schemas;
using SharpPulsar.Interfaces.ISchema;
using SharpPulsar.Interfaces;
using SharpPulsar.Schemas.Generic;

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
namespace SharpPulsar.Schema
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
			set => _schema = value;
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
					_schemaInfoProvider = value;
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
			_topicName = topicName;
			_componentName = componentName;
            if (schemaInfo == null) return;
            var genericSchema = GenerateSchema(schemaInfo);
            Schema = genericSchema;
            Log.LogInformation("Configure {} schema for topic {} : {}", componentName, topicName, schemaInfo.SchemaDefinition);
        }

		private IGenericSchema<IGenericRecord> GenerateSchema(SchemaInfo schemaInfo)
		{
			if (schemaInfo.Type != SchemaType.AVRO && schemaInfo.Type != SchemaType.JSON)
			{
				throw new Exception("Currently auto consume only works for topics with avro or json schemas");
			}
			// when using `AutoConsumeSchema`, we use the schema associated with the messages as schema reader
			// to decode the messages.
			return GenericSchema.Of(schemaInfo, false);
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
		public static object GetSchema(ISchemaInfo schemaInfo)
		{
			switch (schemaInfo.Type.InnerEnumValue)
			{
				case SchemaType.InnerEnum.INT8:
					return ByteSchema.Of();
				case SchemaType.InnerEnum.INT16:
					return ShortSchema.Of();
				case SchemaType.InnerEnum.INT32:
					return IntSchema.Of();
				case SchemaType.InnerEnum.INT64:
					return LongSchema.Of();
				case SchemaType.InnerEnum.STRING:
					return StringSchema.Utf8();
				case SchemaType.InnerEnum.FLOAT:
					return FloatSchema.Of();
				case SchemaType.InnerEnum.DOUBLE:
					return DoubleSchema.Of();
				case SchemaType.InnerEnum.BOOLEAN:
					return BooleanSchema.Of();
				case SchemaType.InnerEnum.BYTES:
				case SchemaType.InnerEnum.NONE:
					return BytesSchema.Of();
				case SchemaType.InnerEnum.DATE:
					return DateSchema.Of();
				case SchemaType.InnerEnum.TIME:
					return TimeSchema.Of();
				case SchemaType.InnerEnum.TIMESTAMP:
					return TimestampSchema.Of();
				case SchemaType.InnerEnum.INSTANT:
					return InstantSchema.Of();
				case SchemaType.InnerEnum.LocalDate:
					return LocalDateSchema.Of();
				case SchemaType.InnerEnum.LocalTime:
					return LocalTimeSchema.Of();
				case SchemaType.InnerEnum.LocalDateTime:
					return LocalDateTimeSchema.Of();
				case SchemaType.InnerEnum.JSON:
				case SchemaType.InnerEnum.AVRO:
					return GenericAvroSchema.Of(schemaInfo);
				case SchemaType.InnerEnum.KeyValue:
					KeyValue<ISchemaInfo, ISchemaInfo> kvSchemaInfo = KeyValueSchemaInfo.DecodeKeyValueSchemaInfo(schemaInfo);
					var keySchema = (ISchema<object>)GetSchema(kvSchemaInfo.Key);
					var valueSchema = (ISchema<object>)GetSchema(kvSchemaInfo.Value);
					return KeyValueSchema<object, object>.Of(keySchema, valueSchema);
				default:
					throw new ArgumentException("Retrieve schema instance from schema info for type '" + schemaInfo.Type + "' is not supported yet");
			}
		}

        object ICloneable.Clone()
        {
            throw new NotImplementedException();
        }

        private static readonly ILogger Log = Utility.Log.Logger.CreateLogger<AutoConsumeSchema>();
	}

}