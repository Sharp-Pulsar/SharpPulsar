using System;

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
namespace Org.Apache.Pulsar.Client.Impl.Schema
{
	//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
	//	import static com.google.common.@base.Preconditions.checkArgument;

	using Getter = lombok.Getter;
	using Slf4j = lombok.@extern.slf4j.Slf4j;
	using Schema = org.apache.pulsar.client.api.Schema;
	using SchemaSerializationException = org.apache.pulsar.client.api.SchemaSerializationException;
	using SchemaInfoProvider = org.apache.pulsar.client.api.schema.SchemaInfoProvider;
	using KeyValue = org.apache.pulsar.common.schema.KeyValue;
	using KeyValueEncodingType = org.apache.pulsar.common.schema.KeyValueEncodingType;
	using SchemaInfo = org.apache.pulsar.common.schema.SchemaInfo;
	using SchemaType = org.apache.pulsar.common.schema.SchemaType;

	/// <summary>
	/// [Key, Value] pair schema definition
	/// </summary>
	//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
	//ORIGINAL LINE: @Slf4j public class KeyValueSchema<K, V> implements org.apache.pulsar.client.api.Schema<org.apache.pulsar.common.schema.KeyValue<K, V>>
	public class KeyValueSchema<K, V> : Schema<KeyValue<K, V>>
	{
		//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
		//ORIGINAL LINE: @Getter private final org.apache.pulsar.client.api.Schema<K> keySchema;
		private readonly Schema<K> _keySchema;
		//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
		//ORIGINAL LINE: @Getter private final org.apache.pulsar.client.api.Schema<V> valueSchema;
		private readonly Schema<V> _valueSchema;

		//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
		//ORIGINAL LINE: @Getter private final org.apache.pulsar.common.schema.KeyValueEncodingType keyValueEncodingType;
		private readonly KeyValueEncodingType _keyValueEncodingType;

		// schemaInfo combined by KeySchemaInfo and ValueSchemaInfo:
		//   [keyInfo.length][keyInfo][valueInfo.length][ValueInfo]
		private SchemaInfo _schemaInfo;
		//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods of the current type:
		protected internal SchemaInfoProvider SchemaInfoProviderConflict;

		/// <summary>
		/// Key Value Schema using passed in schema type, support JSON and AVRO currently.
		/// </summary>
		public static Schema<KeyValue<K, V>> Of<K, V>(Type Key, Type Value, SchemaType Type)
		{
			checkArgument(SchemaType.JSON == Type || SchemaType.AVRO == Type);
			if (SchemaType.JSON == Type)
			{
				return new KeyValueSchema<KeyValue<K, V>>(JSONSchema.of(Key), JSONSchema.of(Value), KeyValueEncodingType.INLINE);
			}
			else
			{
				// AVRO
				return new KeyValueSchema<KeyValue<K, V>>(AvroSchema.of(Key), AvroSchema.of(Value), KeyValueEncodingType.INLINE);
			}
		}


		public static Schema<KeyValue<K, V>> Of<K, V>(Schema<K> KeySchema, Schema<V> ValueSchema)
		{
			return new KeyValueSchema<KeyValue<K, V>>(KeySchema, ValueSchema, KeyValueEncodingType.INLINE);
		}

		public static Schema<KeyValue<K, V>> Of<K, V>(Schema<K> KeySchema, Schema<V> ValueSchema, KeyValueEncodingType KeyValueEncodingType)
		{
			return new KeyValueSchema<KeyValue<K, V>>(KeySchema, ValueSchema, KeyValueEncodingType);
		}

		private static readonly Schema<KeyValue<sbyte[], sbyte[]>> _kvBytes = new KeyValueSchema<KeyValue<sbyte[], sbyte[]>>(BytesSchema.of(), BytesSchema.of());

		public static Schema<KeyValue<sbyte[], sbyte[]>> KvBytes()
		{
			return _kvBytes;
		}

		public override bool SupportSchemaVersioning()
		{
			return _keySchema.supportSchemaVersioning() || _valueSchema.supportSchemaVersioning();
		}

		private KeyValueSchema(Schema<K> KeySchema, Schema<V> ValueSchema) : this(KeySchema, ValueSchema, KeyValueEncodingType.INLINE)
		{
		}

		private KeyValueSchema(Schema<K> KeySchema, Schema<V> ValueSchema, KeyValueEncodingType KeyValueEncodingType)
		{
			this._keySchema = KeySchema;
			this._valueSchema = ValueSchema;
			this._keyValueEncodingType = KeyValueEncodingType;
			this.SchemaInfoProviderConflict = new SchemaInfoProviderAnonymousInnerClass(this);
			// if either key schema or value schema requires fetching schema info,
			// we don't need to configure the key/value schema info right now.
			// defer configuring the key/value schema info until `configureSchemaInfo` is called.
			if (!RequireFetchingSchemaInfo())
			{
				ConfigureKeyValueSchemaInfo();
			}
		}

		private class SchemaInfoProviderAnonymousInnerClass : SchemaInfoProvider
		{
			private readonly KeyValueSchema<K, V> _outerInstance;

			public SchemaInfoProviderAnonymousInnerClass(KeyValueSchema<K, V> OuterInstance)
			{
				this._outerInstance = OuterInstance;
			}

			public override CompletableFuture<SchemaInfo> GetSchemaByVersion(sbyte[] SchemaVersion)
			{
				return CompletableFuture.completedFuture(outerInstance._schemaInfo);
			}

			public override CompletableFuture<SchemaInfo> LatestSchema
			{
				get
				{
					return CompletableFuture.completedFuture(outerInstance._schemaInfo);
				}
			}

			public override string TopicName
			{
				get
				{
					return "key-value-schema";
				}
			}
		}

		// encode as bytes: [key.length][key.bytes][value.length][value.bytes] or [value.bytes]
		public virtual sbyte[] Encode(KeyValue<K, V> Message)
		{
			if (_keyValueEncodingType != null && _keyValueEncodingType == KeyValueEncodingType.INLINE)
			{
				return KeyValue.encode(Message.Key, _keySchema, Message.Value, _valueSchema);
			}
			else
			{
				if (Message.Value == null)
				{
					return null;
				}
				return _valueSchema.encode(Message.Value);
			}
		}

		public virtual KeyValue<K, V> Decode(sbyte[] Bytes)
		{
			return Decode(Bytes, null);
		}

		public virtual KeyValue<K, V> Decode(sbyte[] Bytes, sbyte[] SchemaVersion)
		{
			if (this._keyValueEncodingType == KeyValueEncodingType.SEPARATED)
			{
				throw new SchemaSerializationException("This method cannot be used under this SEPARATED encoding type");
			}

			return KeyValue.decode(Bytes, (keyBytes, valueBytes) => Decode(keyBytes, valueBytes, SchemaVersion));
		}

		public virtual KeyValue<K, V> Decode(sbyte[] KeyBytes, sbyte[] ValueBytes, sbyte[] SchemaVersion)
		{
			K K;
			if (KeyBytes == null)
			{
				K = default(K);
			}
			else
			{
				if (_keySchema.supportSchemaVersioning() && SchemaVersion != null)
				{
					K = _keySchema.decode(KeyBytes, SchemaVersion);
				}
				else
				{
					K = _keySchema.decode(KeyBytes);
				}
			}

			V V;
			if (ValueBytes == null)
			{
				V = default(V);
			}
			else
			{
				if (_valueSchema.supportSchemaVersioning() && SchemaVersion != null)
				{
					V = _valueSchema.decode(ValueBytes, SchemaVersion);
				}
				else
				{
					V = _valueSchema.decode(ValueBytes);
				}
			}
			return new KeyValue<K, V>(K, V);
		}

		public virtual SchemaInfo SchemaInfo
		{
			get
			{
				return this._schemaInfo;
			}
		}

		public virtual SchemaInfoProvider SchemaInfoProvider
		{
			set
			{
				this.SchemaInfoProviderConflict = value;
			}
		}

		public override bool RequireFetchingSchemaInfo()
		{
			return _keySchema.requireFetchingSchemaInfo() || _valueSchema.requireFetchingSchemaInfo();
		}

		public override void ConfigureSchemaInfo(string TopicName, string ComponentName, SchemaInfo SchemaInfo)
		{
			KeyValue<SchemaInfo, SchemaInfo> KvSchemaInfo = KeyValueSchemaInfo.decodeKeyValueSchemaInfo(SchemaInfo);
			_keySchema.configureSchemaInfo(TopicName, "key", KvSchemaInfo.Key);
			_valueSchema.configureSchemaInfo(TopicName, "value", KvSchemaInfo.Value);
			ConfigureKeyValueSchemaInfo();

			if (null == this._schemaInfo)
			{
				throw new Exception("No key schema info or value schema info : key = " + _keySchema.SchemaInfo + ", value = " + _valueSchema.SchemaInfo);
			}
		}

		public override Schema<KeyValue<K, V>> Clone()
		{
			return KeyValueSchema.of(_keySchema.clone(), _valueSchema.clone(), _keyValueEncodingType);
		}

		private void ConfigureKeyValueSchemaInfo()
		{
			this._schemaInfo = KeyValueSchemaInfo.encodeKeyValueSchemaInfo(_keySchema, _valueSchema, _keyValueEncodingType);

			this._keySchema.SchemaInfoProvider = new SchemaInfoProviderAnonymousInnerClass2(this);

			this._valueSchema.SchemaInfoProvider = new SchemaInfoProviderAnonymousInnerClass3(this);
		}

		private class SchemaInfoProviderAnonymousInnerClass2 : SchemaInfoProvider
		{
			private readonly KeyValueSchema<K, V> _outerInstance;

			public SchemaInfoProviderAnonymousInnerClass2(KeyValueSchema<K, V> OuterInstance)
			{
				this._outerInstance = OuterInstance;
			}

			public override CompletableFuture<SchemaInfo> GetSchemaByVersion(sbyte[] SchemaVersion)
			{
				return outerInstance.SchemaInfoProviderConflict.getSchemaByVersion(SchemaVersion).thenApply(si => KeyValueSchemaInfo.decodeKeyValueSchemaInfo(si).Key);
			}

			public override CompletableFuture<SchemaInfo> LatestSchema
			{
				get
				{
					return CompletableFuture.completedFuture(((AbstractStructSchema<K>)outerInstance._keySchema).schemaInfo);
				}
			}

			public override string TopicName
			{
				get
				{
					return "key-schema";
				}
			}
		}

		private class SchemaInfoProviderAnonymousInnerClass3 : SchemaInfoProvider
		{
			private readonly KeyValueSchema<K, V> _outerInstance;

			public SchemaInfoProviderAnonymousInnerClass3(KeyValueSchema<K, V> OuterInstance)
			{
				this._outerInstance = OuterInstance;
			}

			public override CompletableFuture<SchemaInfo> GetSchemaByVersion(sbyte[] SchemaVersion)
			{
				return outerInstance.SchemaInfoProviderConflict.getSchemaByVersion(SchemaVersion).thenApply(si => KeyValueSchemaInfo.decodeKeyValueSchemaInfo(si).Value);
			}

			public override CompletableFuture<SchemaInfo> LatestSchema
			{
				get
				{
					return CompletableFuture.completedFuture(((AbstractStructSchema<V>)outerInstance._valueSchema).schemaInfo);
				}
			}

			public override string TopicName
			{
				get
				{
					return "value-schema";
				}
			}
		}
	}
}
