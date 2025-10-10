using System;
using SharpPulsar.Shared;
using SharpPulsar.Schemas.Generic;
using SharpPulsar.API;
using SharpPulsar.API.Schema;
using SharpPulsar.Common.Schema;
using SharpPulsar.Shared.Exceptions;
using System.Collections.Concurrent;
using SharpPulsar.Protocol.Schema;
using SharpPulsar.Admin.v2;
using System.Threading;
using SharpPulsar.Common.Precondition;
using System.Text.Unicode;
using Xunit;
using Avro.Generic;
using System.Text;

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
namespace SharpPulsar.Schemas
{
    /// <summary>
    /// Auto detect schema.
    /// </summary>
    public class AutoConsumeSchema : ISchema<IGenericRecord>
    {
        private bool InstanceFieldsInitialized = false;

        public AutoConsumeSchema()
        {
            if (!InstanceFieldsInitialized)
            {
                InitializeInstanceFields();
                InstanceFieldsInitialized = true;
            }
        }

        private void InitializeInstanceFields()
        {
            _schemaMap = InitSchemaMap();
        }
        private ConcurrentDictionary<ISchemaVersion, ISchema<object>> InitSchemaMap()
        {
            var schemaMap = new ConcurrentDictionary<ISchemaVersion, ISchema<object>>();
            // The Schema.BYTES will not be uploaded to the broker and store in the schema storage,
            // if the schema version in the message metadata is empty byte[], it means its schema is Schema.BYTES.
            schemaMap.TryAdd(BytesSchemaVersion.Of(new byte[0]), ISchema<object>.Bytes);
            return schemaMap;
        }
        public virtual void SetSchema(ISchemaVersion schemaVersion, ISchema<object> schema)
        {
            _schemaMap.TryAdd(schemaVersion, schema);
        }

        public virtual void SetSchema(ISchema<object> schema)
        {
            _schemaMap.TryAdd(ISchemaVersion.Latest, schema);
        }

        private void EnsureSchemaInitialized(ISchemaVersion schemaVersion)
        {
            Condition.CheckState(_schemaMap.ContainsKey(schemaVersion), "Schema version " + schemaVersion + " is not initialized before used");
        }

        private ConcurrentDictionary<ISchemaVersion, ISchema<object>> _schemaMap;

        public static readonly ISchemaInfo SCHEMA_INFO = SchemaInfo.Bui.builder().name("AutoConsume").type(SchemaType.AUTO_CONSUME).schema(new sbyte[0]).build();


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
            if (null == _schema)
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
            if (!(message is IGenericRecord))
                throw new ArgumentException($"{message.GetType()} is not IGenericRecord");
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
        public virtual ISchema<object> AtSchemaVersion(byte[] schemaVersion)
        {
            var sv = GetSchemaVersion(schemaVersion);
            FetchSchemaIfNeeded(sv);
            EnsureSchemaInitialized(sv);

            _schemaMap.TryGetValue(sv, out var topicVersionedSchema);
            if (topicVersionedSchema.SupportSchemaVersioning() && topicVersionedSchema is AbstractSchema<object>)
            {
                return ((AbstractSchema<object>)topicVersionedSchema).AtSchemaVersion(schemaVersion);
            }
            else
            {
                return topicVersionedSchema;
            }

        }
        public IGenericRecord Decode(byte[] bytes, byte[] schemaVersion)
        {
            var sv = GetSchemaVersion(schemaVersion);
            FetchSchemaIfNeeded(sv);
            EnsureSchemaInitialized(sv);
            _schemaMap.TryGetValue(sv, out var t);
            return Adapt(t.Decode(bytes, schemaVersion), schemaVersion);
        }

        public IGenericRecord Decode(ByteBuffer buffer, byte[] schemaVersion)
        {
            var sv = GetSchemaVersion(schemaVersion);
            FetchSchemaIfNeeded(sv);
            EnsureSchemaInitialized(sv);
            _schemaMap.TryGetValue(sv, out var t);
            return Adapt(t.Decode(buffer, schemaVersion), schemaVersion);
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
        public void ConfigureSchemaInfo(string topicName, string componentName, SchemaInfo schemaInfo)
        {
            _topicName = topicName;
            _componentName = componentName;
            if (schemaInfo == null) return;
            var genericSchema = GenerateSchema(schemaInfo);
            SetSchema(ISchemaVersion.Latest, genericSchema);
            //Log.LogInformation("Configure {} schema for topic {} : {}", componentName, topicName, schemaInfo.SchemaDefinition);
        }
        
        private static ISchema<object> GenerateSchema(ISchemaInfo schemaInfo)
        {
            // when using `AutoConsumeSchema`, we use the schema associated with the messages as schema reader
            // to decode the messages.
            const bool useProvidedSchemaAsReaderSchema = false;

            if (schemaInfo.Type != SchemaType.AVRO && schemaInfo.Type != SchemaType.JSON)
            {
                return ExtractFromAvroSchema(schemaInfo, useProvidedSchemaAsReaderSchema);
            }
            return GetSchema(schemaInfo);
        }
        private static ISchema<object> ExtractFromAvroSchema(ISchemaInfo schemaInfo, in bool useProvidedSchemaAsReaderSchema)
        {
            var avroSchema = SchemaUtils.ParseAvroSchema(Encoding.UTF8.GetString(schemaInfo.Schema));
            // if avroSchema type is RECORD we can use GenericSchema, otherwise use its own schema and decode return
            // `GenericObjectWrapper`
            if (avroSchema.Type == RECORD)
            {
                return GenericSchema.Of(schemaInfo, useProvidedSchemaAsReaderSchema);
            }
            else
            {
                // because of we use json primitive schema or avro primitive schema generated data
                // different from the data generated using the primitive schema of pulsar itself.
                // so we should use the original schema of this data
                if (schemaInfo.Type == SchemaType.JSON)
                {
                    // It should be generated and used POJO, otherwise json cannot be parsed correctly
                    return ISchema.JSON(SchemaDefinition.builder().withPojo(ReflectData.get().getClass(avroSchema)).build());
                }
                else
                {
                    return Schema.AVRO(SchemaDefinition.builder().withJsonDef(new string(schemaInfo.getSchema(), UTF_8)).build());
                }
            }
        }

        public ISchema<IGenericRecord> Clone()
        {
            var schema = ISchema<IGenericRecord>.AutoConsume();
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
                    var kvSchemaInfo = KeyValueSchemaInfo.DecodeKeyValueSchemaInfo(schemaInfo);
                    var keySchema = (ISchema<object>)GetSchema(kvSchemaInfo.Key);
                    var valueSchema = (ISchema<object>)GetSchema(kvSchemaInfo.Value);
                    return KeyValueSchema<object, object>.Of(keySchema, valueSchema, KeyValueSchemaInfo.DecodeKeyValueEncodingType(schemaInfo));
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
                throw new InvalidOperationException("Cannot decode a message without schema");
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

        public virtual ISchema<object> InternalSchema
        {
            get
            {
                _schemaMap.TryGetValue(ISchemaVersion.Latest, out var schema);
                return schema;
            }
        }

        public virtual ISchema<object> GetInternalSchema(byte[] schemaVersion)
        {
            _schemaMap.TryGetValue(GetSchemaVersion(schemaVersion), out var schema);
            return schema;
        }

        /// <summary>
        /// Get a specific schema version, fetching from the Registry if it is not loaded yet.
        /// This method is not intended to be used by applications. </summary>
        /// <param name="schemaVersion"> the version </param>
        /// <returns> the Schema at the specific version </returns>
        /// <seealso cref=".atSchemaVersion(byte[])"/>
        public virtual ISchema<object> UnwrapInternalSchema(byte[] schemaVersion)
        {
            FetchSchemaIfNeeded(BytesSchemaVersion.Of(schemaVersion));
            return GetInternalSchema(schemaVersion);
        }
        private static ISchemaVersion GetSchemaVersion(byte[] schemaVersion)
        {
            if (schemaVersion != null)
            {
                return BytesSchemaVersion.Of(schemaVersion);
            }
            return BytesSchemaVersion.Of(new byte[0]);
        }

        /// <summary>
        /// It may happen that the schema is not loaded but we need it, for instance in order to call getSchemaInfo()
        /// We cannot call this method in getSchemaInfo, because getSchemaInfo is called in many
        /// places and we will introduce lots of deadlocks.
        /// </summary>
        public virtual void FetchSchemaIfNeeded(ISchemaVersion schemaVersion)
        {
            if (schemaVersion == null)
            {
                schemaVersion = BytesSchemaVersion.Of(new byte[0]);
            }
            if (!_schemaMap.ContainsKey(schemaVersion))
            {
                if (_schemaInfoProvider == null)
                {
                    throw new SchemaSerializationException("Can't get accurate schema information for topic " + _topicName + "using AutoConsumeSchema because SchemaInfoProvider is not set yet");
                }
                else
                {
                    SchemaInfo schemaInfo = null;
                    try
                    {
                        schemaInfo = (SchemaInfo)_schemaInfoProvider.GetSchemaByVersion(schemaVersion.Bytes());
                        if (schemaInfo == null)
                        {
                            // schemaless topic
                            schemaInfo = (SchemaInfo)BytesSchema.Of().SchemaInfo;
                        }
                    }
                    catch (Exception e)
                    {
                        //log.error("Can't get last schema for topic {} using AutoConsumeSchema", topicName);
                        throw new SchemaSerializationException(e);
                    }
                    // schemaInfo null means that there is no schema attached to the topic.
                    var schema = GenerateSchema(schemaInfo);
                    schema.SchemaInfoProvider = _schemaInfoProvider;
                    SetSchema(schemaVersion, schema);
                    //log.info("Configure {} schema {} for topic {} : {}", componentName, schemaVersion, topicName, schemaInfo.getSchemaDefinition());
                }
            }
        }
    }

}