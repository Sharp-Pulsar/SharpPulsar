using SharpPulsar.API;
using SharpPulsar.API.Internal;
using SharpPulsar.API.Schema;
using SharpPulsar.Common.Precondition;
using SharpPulsar.Common.Schema;
using SharpPulsar.Protocol.Schema;
using SharpPulsar.Shared;
using SharpPulsar.Shared.Buf;
using SharpPulsar.Shared.Exceptions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

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
    /// [Key, Value] pair schema definition
    /// </summary>
    public class KeyValueSchema<K, V> : AbstractSchema<KeyValue<K, V>>, IKeyValueSchema<K, V>
	{
		private readonly ISchema<K> _keySchema;

		private readonly ISchema<V> _valueSchema;

		private readonly KeyValueEncodingType _keyValueEncodingType;
        private readonly IDictionary<ISchemaVersion, ISchema<object>> _schemaMap = new ConcurrentDictionary<ISchemaVersion, ISchema<object>>();


        // schemaInfo combined by KeySchemaInfo and ValueSchemaInfo:
        //   [keyInfo.length][keyInfo][valueInfo.length][ValueInfo]
        private ISchemaInfo _schemaInfo;

		private ISchemaInfoProvider _schemaInfoProvider;

		/// <summary>
		/// Key Value Schema using passed in schema type, support JSON and AVRO currently.
		/// </summary>
		public static ISchema<KeyValue<K, V>> Of(Type Key, Type Value, SchemaType Type)
		{
			Condition.CheckArgument(SchemaType.JSON == Type || SchemaType.AVRO == Type);
			if (SchemaType.JSON == Type)
			{
				return new KeyValueSchema<K, V>(JSONSchema<K>.Of(Key), JSONSchema<V>.Of(Value), KeyValueEncodingType.INLINE);
			}
			else
			{
				// AVRO
				return new KeyValueSchema<K, V>(AvroSchema<K>.Of(Key), AvroSchema<V>.Of(Value), KeyValueEncodingType.INLINE);
			}
		}


		public static ISchema<KeyValue<K, V>> Of(ISchema<K> KeySchema, ISchema<V> ValueSchema)
		{
			return new KeyValueSchema<K, V>(KeySchema, ValueSchema, KeyValueEncodingType.INLINE);
		}

		public static ISchema<KeyValue<K, V>> Of(ISchema<K> KeySchema, ISchema<V> ValueSchema, KeyValueEncodingType KeyValueEncodingType)
		{
			return new KeyValueSchema<K, V>(KeySchema, ValueSchema, KeyValueEncodingType);
		}

		private static readonly ISchema<KeyValue<byte[], byte[]>> _kvBytes = new KeyValueSchema<byte[], byte[]>(BytesSchema.Of(), BytesSchema.Of());

		public static ISchema<KeyValue<byte[], byte[]>> KvBytes()
		{
			return _kvBytes;
		}

		public override bool SupportSchemaVersioning()
		{
			return _keySchema.SupportSchemaVersioning() || _valueSchema.SupportSchemaVersioning();
		}

		private KeyValueSchema(ISchema<K> KeySchema, ISchema<V> ValueSchema) : this(KeySchema, ValueSchema, KeyValueEncodingType.INLINE)
		{
		}

		private KeyValueSchema(ISchema<K> KeySchema, ISchema<V> ValueSchema, KeyValueEncodingType KeyValueEncodingType)
		{
            SchemaType keySchemaType = null;
            if (KeySchema != null && KeySchema.SchemaInfo != null)
            {
                keySchemaType = KeySchema.SchemaInfo.Type;
            }
            SchemaType valueSchemaType = null;
            if (ValueSchema != null && ValueSchema.SchemaInfo != null)
            {
                valueSchemaType = ValueSchema.SchemaInfo.Type;
            }
            if ((SchemaType.External.Equals(keySchemaType) && valueSchemaType != null && SchemaType.IsStructType(valueSchemaType)) || (SchemaType.External.Equals(valueSchemaType) && keySchemaType != null && SchemaType.IsStructType(keySchemaType)))
            {
                throw new System.ArgumentException("External schema cannot be used with other Pulsar struct schema types," + "keySchemaType: " + keySchemaType + ", valueSchemaType: " + valueSchemaType);
            }

            _keySchema = KeySchema;
			_valueSchema = ValueSchema;
			_keyValueEncodingType = KeyValueEncodingType;
			_schemaInfoProvider = new InfoSchemaInfoProvider(this);
			// if either key schema or value schema requires fetching schema info,
			// we don't need to configure the key/value schema info right now.
			// defer configuring the key/value schema info until `configureSchemaInfo` is called.
			if (!RequireFetchingSchemaInfo())
			{
				ConfigureKeyValueSchemaInfo();
			}
            else
            {
                BuildKeyValueSchemaInfo();
            }

        }
        private void BuildKeyValueSchemaInfo()
        {
            _schemaInfo = KeyValueSchemaInfo.EncodeKeyValueSchemaInfo(_keySchema, _valueSchema, _keyValueEncodingType);
        }

        private class InfoSchemaInfoProvider : ISchemaInfoProvider
		{
			private readonly KeyValueSchema<K, V> _outerInstance;

			public InfoSchemaInfoProvider(KeyValueSchema<K, V> OuterInstance)
			{
				_outerInstance = OuterInstance;
			}

			public ISchemaInfo GetSchemaByVersion(byte[] SchemaVersion)
			{
				return _outerInstance._schemaInfo;
			}

			public async ValueTask<ISchemaInfo> LatestSchema()
			{
				return await Task.FromResult(_outerInstance._schemaInfo);
			}

			public string TopicName
			{
				get
				{
					return "key-value-schema";
				}
			}
		}

        // encode as bytes: [key.length][key.bytes][value.length][value.bytes] or [value.bytes]
        public override byte[] Encode(KeyValue<K, V> message)
        {
            return Encode(null, message).Data;
        }
        public virtual EncodeData Encode(string topic, KeyValue<K, V> Message)
		{
			if (_keyValueEncodingType == KeyValueEncodingType.INLINE)
			{
				return KeyValue<K, V>.Encode(topic, Message.Key, _keySchema, Message.Value, _valueSchema);
			}
			else
			{
				if (Message.Value == null)
				{
					return null;
				}
				return _valueSchema.Encode(topic, Message.Value);
			}
		}

		public virtual KeyValue<K, V> Decode(byte[] Bytes)
		{
			return Decode(Bytes, null);
		}

		public virtual KeyValue<K, V> Decode(byte[] Bytes, byte[] SchemaVersion)
		{
			if (_keyValueEncodingType == KeyValueEncodingType.SEPARATED)
			{
				throw new SchemaSerializationException("This method cannot be used under this SEPARATED encoding type");
			}

			return KeyValue<K, V>.Decode(Bytes, (keyBytes, valueBytes) => Decode(keyBytes, valueBytes, SchemaVersion));
		}

		public virtual KeyValue<K, V> Decode(byte[] KeyBytes, byte[] ValueBytes, byte[] SchemaVersion)
		{
			K K;
			if (KeyBytes == null)
			{
				K = default(K);
			}
			else
			{
				if (_keySchema.SupportSchemaVersioning() && SchemaVersion != null)
				{
					K = _keySchema.Decode(KeyBytes, SchemaVersion);
				}
				else
				{
					K = _keySchema.Decode(KeyBytes);
				}
			}

			V V;
			if (ValueBytes == null)
			{
				V = default(V);
			}
			else
			{
				if (_valueSchema.SupportSchemaVersioning() && SchemaVersion != null)
				{
					V = _valueSchema.Decode(ValueBytes, SchemaVersion);
				}
				else
				{
					V = _valueSchema.Decode(ValueBytes);
				}
			}
			return new KeyValue<K, V>(K, V);
		}
        public virtual KeyValue<K, V> Decode(string topic, byte[] keyBytes, byte[] valueBytes, byte[] schemaId)
        {
            K k = default(K);
            byte[] keySchemaId = null;
            byte[] valueSchemaId = null;
            if (EncodeData.IsValidSchemaId(schemaId))
            {
                var kvSchemaId = GetKeyValueSchemaId(schemaId);
                keySchemaId = kvSchemaId.Key;
                valueSchemaId = kvSchemaId.Value;
            }

            if (keyBytes != null)
            {
                if (_keySchema.SupportSchemaVersioning() && EncodeData.IsValidSchemaId(keySchemaId))
                {
                    k = _keySchema.Decode(topic, keyBytes, keySchemaId);
                }
                else
                {
                    k = _keySchema.Decode(keyBytes);
                }
            }

            V v = default(V);
            if (valueBytes != null)
            {
                if (_valueSchema.SupportSchemaVersioning() && EncodeData.IsValidSchemaId(valueSchemaId))
                {
                    v = _valueSchema.Decode(topic, valueBytes, valueSchemaId);
                }
                else
                {
                    v = _valueSchema.Decode(valueBytes);
                }
            }
            return new KeyValue<K, V>(k, v);
        }

        private KeyValue<byte[], byte[]> GetKeyValueSchemaId(byte[] schemaId)
        {
            if (!SchemaType.External.Equals(_valueSchema.SchemaInfo.Type))
            {
                return new KeyValue<byte[], byte[]>(schemaId, schemaId);
            }
            return KeyValue<byte[], byte[]>.GetSchemaId(schemaId);
        }
        /// <summary>
        /// It may happen that the schema is not loaded but we need it, for instance in order to call getSchemaInfo()
        /// We cannot call this method in getSchemaInfo. </summary>
        /// <seealso cref="AutoConsumeSchema.fetchSchemaIfNeeded(SchemaVersion)"/>
        public virtual void FetchSchemaIfNeeded(string topicName, ISchemaVersion schemaVersion)
        {
            if (_schemaInfo != null)
            {
                if (_keySchema is AutoConsumeSchema)
                {
                    ((AutoConsumeSchema)_keySchema).FetchSchemaIfNeeded(schemaVersion);
                }
                if (_valueSchema is AutoConsumeSchema)
                {
                    ((AutoConsumeSchema)_valueSchema).FetchSchemaIfNeeded(schemaVersion);
                }
                return;
            }
            SchemaInfoProviderOnSubschemas();
            if (schemaVersion == null)
            {
                schemaVersion = BytesSchemaVersion.Of(new byte[0]);
            }
            if (_schemaInfoProvider == null)
            {
                throw new SchemaSerializationException("Can't get accurate schema information for " + topicName + " " + "using KeyValueSchemaImpl because SchemaInfoProvider is not set yet");
            }
            else
            {
                SchemaInfo schemaInfo;
                try
                {
                    schemaInfo = (SchemaInfo)_schemaInfoProvider.GetSchemaByVersion(schemaVersion.Bytes());
                    if (schemaInfo == null)
                    {
                        // schemaless topic
                        schemaInfo = (SchemaInfo)BytesSchema.Of().SchemaInfo;
                    }
                    ConfigureSchemaInfo(topicName, "topic", schemaInfo);
                }
                catch (Exception e) { 
                    //log.error("Can't get last schema for topic {} using KeyValueSchemaImpl", topicName);
                    throw new SchemaSerializationException(e);
                }
                //log.info("Configure schema {} for topic {} : {}", schemaVersion, topicName, schemaInfo.getSchemaDefinition());
            }

        }
        private void SchemaInfoProviderOnSubschemas()
        {
            _keySchema.SchemaInfoProvider = new KeySchemaInfoProvider(this);

            _valueSchema.SchemaInfoProvider = new KeySchemaInfoProvider(this);
        }

        public override ISchemaInfo SchemaInfo
		{
			get
			{
				return _schemaInfo;
			}
		}

		public override ISchemaInfoProvider SchemaInfoProvider
		{
			set
			{
				_schemaInfoProvider = value;
			}
		}

		public virtual bool RequireFetchingSchemaInfo()
		{
			return _keySchema.RequireFetchingSchemaInfo() || _valueSchema.RequireFetchingSchemaInfo();
		}
		public ISchema<K> KeySchema
        {
			get => _keySchema;
        }
		public ISchema<V> ValueSchema
        {
			get => _valueSchema;
        }
		public virtual void ConfigureSchemaInfo(string TopicName, string ComponentName, ISchemaInfo schemaInfo)
		{
			var KvSchemaInfo = KeyValueSchemaInfo.DecodeKeyValueSchemaInfo(schemaInfo);
			_keySchema.ConfigureSchemaInfo(TopicName, "key", KvSchemaInfo.Key);
			_valueSchema.ConfigureSchemaInfo(TopicName, "value", KvSchemaInfo.Value);
			ConfigureKeyValueSchemaInfo();

			if (null == _schemaInfo)
			{
				throw new Exception("No key schema info or value schema info : key = " + _keySchema.SchemaInfo + ", value = " + _valueSchema.SchemaInfo);
			}
		}

		public ISchema<KeyValue<K, V>> Clone()
		{
			return Of(_keySchema.Clone(), _valueSchema.Clone(), _keyValueEncodingType);
		}

		private void ConfigureKeyValueSchemaInfo()
		{
			_schemaInfo = KeyValueSchemaInfo.EncodeKeyValueSchemaInfo(_keySchema, _valueSchema, _keyValueEncodingType);

			_keySchema.SchemaInfoProvider = new KeySchemaInfoProvider(this);

			_valueSchema.SchemaInfoProvider = new ValueSchemaInfoProvider(this);
		}
		
        object ICloneable.Clone()
        {
            return Clone();
        }
        public override ISchema<KeyValue<K, V>> AtSchemaVersion(byte[] schemaVersion)
        {
		    if(!SupportSchemaVersioning())
		    {
			    return this;
		    }
		    else
		    {
                    var keySchema = _keySchema is AbstractSchema<K> ? ((AbstractSchema<K>)_keySchema).AtSchemaVersion(schemaVersion) : _keySchema;
                    var valueSchema = _valueSchema is AbstractSchema<V> ? ((AbstractSchema<V>)_valueSchema).AtSchemaVersion(schemaVersion) : _valueSchema;
			        return Of(keySchema, valueSchema, _keyValueEncodingType);
		    }
        }
        
        ISchema<IKeyValue<K, V>> ISchema<IKeyValue<K, V>>.Clone()
        {
            throw new NotImplementedException();
        }

        public byte[] Encode(IKeyValue<K, V> message)
        {
            throw new NotImplementedException();
        }

        public override KeyValue<K, V> Decode(ByteBuf byteBuf)
        {
            throw new NotImplementedException();
        }

        private class KeySchemaInfoProvider : ISchemaInfoProvider
		{
			private readonly KeyValueSchema<K, V> _outerInstance;

			public KeySchemaInfoProvider(KeyValueSchema<K, V> OuterInstance)
			{
				_outerInstance = OuterInstance;
			}

			public ISchemaInfo GetSchemaByVersion(byte[] SchemaVersion)
			{
				var si = _outerInstance._schemaInfoProvider.GetSchemaByVersion(SchemaVersion);
				return KeyValueSchemaInfo.DecodeKeyValueSchemaInfo(si).Key;
			}

			public async ValueTask<ISchemaInfo> LatestSchema()
			{
				return await Task.FromResult(_outerInstance._keySchema.SchemaInfo);
			}
			public string TopicName
			{
				get
				{
					return "key-schema";
				}
			}
		}
		public KeyValueEncodingType KeyValueEncodingType
        {
			get => _keyValueEncodingType;
        }

        Shared.KeyValueEncodingType IKeyValueSchema<K, V>.KeyValueEncodingType => throw new NotImplementedException();

        private class ValueSchemaInfoProvider : ISchemaInfoProvider
		{
			private readonly KeyValueSchema<K, V> _outerInstance;

			public ValueSchemaInfoProvider(KeyValueSchema<K, V> OuterInstance)
			{
				_outerInstance = OuterInstance;
			}

			public ISchemaInfo GetSchemaByVersion(byte[] SchemaVersion)
			{
				var si = _outerInstance._schemaInfoProvider.GetSchemaByVersion(SchemaVersion);
				return KeyValueSchemaInfo.DecodeKeyValueSchemaInfo(si).Value;
			}

			public async ValueTask<ISchemaInfo> LatestSchema()
			{
				return await Task.FromResult(_outerInstance._valueSchema.SchemaInfo);
			}
			public string TopicName
			{
				get
				{
					return "value-schema";
				}
			}
		}
	}
}
