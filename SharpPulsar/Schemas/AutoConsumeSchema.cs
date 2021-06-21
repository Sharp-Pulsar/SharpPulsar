using System;
using Microsoft.Extensions.Logging;
using SharpPulsar.Shared;
using SharpPulsar.Schemas;
using SharpPulsar.Interfaces.ISchema;
using SharpPulsar.Interfaces;
using SharpPulsar.Schemas.Generic;
using SharpPulsar.Exceptions;

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

		public void Validate(byte[] message)
		{
			EnsureSchemaInitialized();

			_schema.Validate(message);
		}

		public bool SupportSchemaVersioning()
		{
			return true;
		}

		public byte[] Encode(IGenericRecord message)
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
        public virtual ISchema<IGenericRecord> AtSchemaVersion(byte[] schemaVersion)
        {
            FetchSchemaIfNeeded();
            EnsureSchemaInitialized();
            if (_schema.SupportSchemaVersioning() && _schema is AbstractSchema<IGenericRecord>)
            {
                return ((AbstractSchema<IGenericRecord>)_schema).AtSchemaVersion(schemaVersion);
            }
            else
            {
                return _schema;
            }
        }
        public IGenericRecord Decode(byte[] bytes, byte[] schemaVersion)
        {
            FetchSchemaIfNeeded();
            EnsureSchemaInitialized();
            return Adapt(_schema.Decode(bytes, schemaVersion), schemaVersion);
        }
        public object NativeSchema
        {
            get
            {
                EnsureSchemaInitialized();
                if (_schema == null)
                {
                    return null;
                }
                else
                {
                    return _schema.NativeSchema();
                }
            }
        }
        public  void ConfigureSchemaInfo(string topicName, string componentName, SchemaInfo schemaInfo)
		{
			_topicName = topicName;
			_componentName = componentName;
            if (schemaInfo == null) return;
            var genericSchema = GenerateSchema(schemaInfo);
            Schema = genericSchema;
            //Log.LogInformation("Configure {} schema for topic {} : {}", componentName, topicName, schemaInfo.SchemaDefinition);
        }

		private IGenericSchema<IGenericRecord> GenerateSchema(ISchemaInfo schemaInfo)
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
					return GenericSchema.Of(schemaInfo, false);
				case SchemaType.InnerEnum.KeyValue:
					KeyValue<ISchemaInfo, ISchemaInfo> kvSchemaInfo = KeyValueSchemaInfo.DecodeKeyValueSchemaInfo(schemaInfo);
					var keySchema = (ISchema<object>)GetSchema(kvSchemaInfo.Key);
					var valueSchema = (ISchema<object>)GetSchema(kvSchemaInfo.Value);
					return KeyValueSchema<object, object>.Of(keySchema, valueSchema);
				default:
					throw new ArgumentException("Retrieve schema instance from schema info for type '" + schemaInfo.Type + "' is not supported yet");
			}
		}
        protected internal virtual IGenericRecord Adapt(object value, byte[] schemaVersion)
        {
            if (value is IGenericRecord)
            {
                return (IGenericRecord)value;
            }
            if (_schema == null)
            {
                throw new System.InvalidOperationException("Cannot decode a message without schema");
            }
            return WrapPrimitiveObject(value, _schema.SchemaInfo.Type, schemaVersion);
        }

        public static IGenericRecord WrapPrimitiveObject(object value, SchemaType type, byte[] schemaVersion)
        {
            return GenericObjectWrapper.Of(value, type, schemaVersion);
        }
        object ICloneable.Clone()
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// It may happen that the schema is not loaded but we need it, for instance in order to call getSchemaInfo()
        /// We cannot call this method in getSchemaInfo, because getSchemaInfo is called in many
        /// places and we will introduce lots of deadlocks.
        /// </summary>
        public virtual void FetchSchemaIfNeeded()
        {
            if (_schema == null)
            {
                if (_schemaInfoProvider == null)
                {
                    throw new SchemaSerializationException("Can't get accurate schema information for topic " + _topicName + "using AutoConsumeSchema because SchemaInfoProvider is not set yet");
                }
                else
                {
                    ISchemaInfo schemaInfo = null;
                    try
                    {
                        schemaInfo = _schemaInfoProvider.LatestSchema().GetAwaiter().GetResult();
                        if (schemaInfo == null)
                        {
                            // schemaless topic
                            schemaInfo = BytesSchema.Of().SchemaInfo;
                        }
                    }
                    catch (Exception e) 
                    {
                        throw new SchemaSerializationException(e.Message);
                    }
                    // schemaInfo null means that there is no schema attached to the topic.
                    _schema = GenerateSchema(schemaInfo);
                    _schema.SchemaInfoProvider = _schemaInfoProvider;
                    //log.info("Configure {} schema for topic {} : {}", _componentName, _topicName, schemaInfo.SchemaDefinition);
                }
            }
        }
    }

}