using SharpPulsar.Exceptions;
using SharpPulsar.Interfaces;
using SharpPulsar.Interfaces.ISchema;
using SharpPulsar.Precondition;
using SharpPulsar.Shared;
using System;
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
	public class KeyValueSchema<K, V> : ISchema<KeyValue<K, V>>
	{
		private readonly ISchema<K> _keySchema;

		private readonly ISchema<V> _valueSchema;

		private readonly KeyValueEncodingType _keyValueEncodingType;

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

		public virtual bool SupportSchemaVersioning()
		{
			return _keySchema.SupportSchemaVersioning() || _valueSchema.SupportSchemaVersioning();
		}

		private KeyValueSchema(ISchema<K> KeySchema, ISchema<V> ValueSchema) : this(KeySchema, ValueSchema, KeyValueEncodingType.INLINE)
		{
		}

		private KeyValueSchema(ISchema<K> KeySchema, ISchema<V> ValueSchema, KeyValueEncodingType KeyValueEncodingType)
		{
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
		public virtual byte[] Encode(KeyValue<K, V> Message)
		{
			if (_keyValueEncodingType == KeyValueEncodingType.INLINE)
			{
				return KeyValue<K, V>.Encode(Message.Key, _keySchema, Message.Value, _valueSchema);
			}
			else
			{
				if (Message.Value == null)
				{
					return null;
				}
				return _valueSchema.Encode(Message.Value);
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

		public virtual ISchemaInfo SchemaInfo
		{
			get
			{
				return _schemaInfo;
			}
		}

		public virtual ISchemaInfoProvider SchemaInfoProvider
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
