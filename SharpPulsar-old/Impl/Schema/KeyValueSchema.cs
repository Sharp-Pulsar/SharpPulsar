using SharpPulsar.Common.Schema;
using SharpPulsar.Enum;
using SharpPulsar.Exception;
using SharpPulsar.Interface.Schema;
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
namespace SharpPulsar.Impl.Schema
{
	/// <summary>
	/// [Key, Value] pair schema definition
	/// </summary>
	public class KeyValueSchema<K, V> : ISchema<KeyValue<K, V>>
	{
		private readonly ISchema<K> keySchema;
		private readonly ISchema<V> valueSchema;
		private readonly KeyValueEncodingType keyValueEncodingType;

		// schemaInfo combined by KeySchemaInfo and ValueSchemaInfo:
		//   [keyInfo.length][keyInfo][valueInfo.length][ValueInfo]
		private SchemaInfo schemaInfo;
		protected internal ISchemaInfoProvider schemaInfoProvider;

		/// <summary>
		/// Key Value Schema using passed in schema type, support JSON and AVRO currently.
		/// </summary>
		public static ISchema<KeyValue<K, V>> Of(Type key, Type value, SchemaType type)
		{
			checkArgument(SchemaType.JSON == type || SchemaType.AVRO == type);
			if (SchemaType.JSON == type)
			{
				return new KeyValueSchema<KeyValue<K, V>>(JSONSchema.of(key), JSONSchema.of(value), KeyValueEncodingType.INLINE);
			}
			else
			{
				// AVRO
				return new KeyValueSchema<KeyValue<K, V>>(AvroSchema.of(key), AvroSchema.of(value), KeyValueEncodingType.INLINE);
			}
		}


		public static ISchema<KeyValue<K, V>> Of<K, V>(ISchema<K> keySchema, ISchema<V> valueSchema)
		{
			return new KeyValueSchema<KeyValue<K, V>>(keySchema, valueSchema, KeyValueEncodingType.INLINE);
		}

		public static ISchema<KeyValue<K, V>> Of<K, V>(ISchema<K> keySchema, ISchema<V> valueSchema, KeyValueEncodingType keyValueEncodingType)
		{
			return new KeyValueSchema<KeyValue<K, V>>(keySchema, valueSchema, keyValueEncodingType);
		}

		private static readonly ISchema<KeyValue<sbyte[], sbyte[]>> KV_BYTES = new KeyValueSchema<KeyValue<sbyte[], sbyte[]>>(BytesSchema.Of(), BytesSchema.Of());

		public static ISchema<KeyValue<sbyte[], sbyte[]>> KvBytes()
		{
			return KV_BYTES;
		}

		public bool SupportSchemaVersioning()
		{
			return keySchema.SupportSchemaVersioning() || valueSchema.SupportSchemaVersioning();
		}

		private KeyValueSchema(ISchema<K> keySchema, ISchema<V> valueSchema) : this(keySchema, valueSchema, KeyValueEncodingType.INLINE)
		{
		}

		private KeyValueSchema(ISchema<K> keySchema, ISchema<V> valueSchema, KeyValueEncodingType keyValueEncodingType)
		{
			this.keySchema = keySchema;
			this.valueSchema = valueSchema;
			this.keyValueEncodingType = keyValueEncodingType;
			this.schemaInfoProvider = new SchemaInfoProviderAnonymousInnerClass(this);
			// if either key schema or value schema requires fetching schema info,
			// we don't need to configure the key/value schema info right now.
			// defer configuring the key/value schema info until `configureSchemaInfo` is called.
			if (!RequireFetchingSchemaInfo())
			{
				ConfigureKeyValueSchemaInfo();
			}
		}

		private class SchemaInfoProviderAnonymousInnerClass : ISchemaInfoProvider
		{
			private readonly KeyValueSchema<K, V> outerInstance;

			public SchemaInfoProviderAnonymousInnerClass(KeyValueSchema<K, V> outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public async ValueTask<SchemaInfo> GetSchemaByVersion(sbyte[] schemaVersion)
			{
				return await Task.FromResult(outerInstance.schemaInfo);
				//return CompletableFuture.completedFuture(outerInstance.schemaInfo);
			}

			public SchemaInfo LatestSchema
			{
				get
				{
					return outerInstance.schemaInfo;
				}
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
		public virtual sbyte[] Encode(KeyValue<K, V> message)
		{
			if (keyValueEncodingType != null && keyValueEncodingType == KeyValueEncodingType.INLINE)
			{
				return KeyValue<K, V>.Encode(message.Key, keySchema, message.Value, valueSchema);
			}
			else
			{
				return valueSchema.Encode(message.Value);
			}
		}

		public virtual KeyValue<K, V> Decode(sbyte[] bytes)
		{
			return Decode(bytes, null);
		}

		public virtual KeyValue<K, V> Decode(sbyte[] bytes, sbyte[] schemaVersion)
		{
			if (this.keyValueEncodingType == KeyValueEncodingType.SEPARATED)
			{
				throw new SchemaSerializationException("This method cannot be used under this SEPARATED encoding type");
			}

			return KeyValue<K,V>.Decode(bytes, (keyBytes, valueBytes) => Decode(keyBytes, valueBytes, schemaVersion));
		}

		public virtual KeyValue<K, V> Decode(sbyte[] keyBytes, sbyte[] valueBytes, sbyte[] schemaVersion)
		{
			K k;
			if (keySchema.SupportSchemaVersioning() && schemaVersion != null)
			{
				k = keySchema.Decode((byte[])(Array)keyBytes, (byte[])(Array)schemaVersion);
			}
			else
			{
				k = keySchema.Decode((byte[])(Array)keyBytes);
			}
			V v;
			if (valueSchema.SupportSchemaVersioning() && schemaVersion != null)
			{
				v = valueSchema.Decode((byte[])(Array)valueBytes, (byte[])(Array)schemaVersion);
			}
			else
			{
				v = valueSchema.Decode((byte[])(Array)valueBytes);
			}
			return new KeyValue<K, V>(k, v);
		}

		public virtual SchemaInfo SchemaInfo
		{
			get
			{
				return this.schemaInfo;
			}
		}

		public virtual ISchemaInfoProvider SchemaInfoProvider
		{
			set
			{
				this.schemaInfoProvider = value;
			}
		}

		public bool RequireFetchingSchemaInfo()
		{
			return keySchema.RequireFetchingSchemaInfo() || valueSchema.RequireFetchingSchemaInfo();
		}

		public void ConfigureSchemaInfo(string topicName, string componentName, SchemaInfo schemaInfo)
		{
			KeyValue<SchemaInfo, SchemaInfo> kvSchemaInfo = KeyValueSchemaInfo.DecodeKeyValueSchemaInfo(schemaInfo);
			keySchema.ConfigureSchemaInfo(topicName, "key", kvSchemaInfo.Key);
			valueSchema.ConfigureSchemaInfo(topicName, "value", kvSchemaInfo.Value);
			configureKeyValueSchemaInfo();

			if (null == this.schemaInfo)
			{
				throw new System.Exception("No key schema info or value schema info : key = " + keySchema.SchemaInfo + ", value = " + valueSchema.SchemaInfo);
			}
		}

		private void ConfigureKeyValueSchemaInfo()
		{
			this.schemaInfo = KeyValueSchemaInfo.EncodeKeyValueSchemaInfo(keySchema, valueSchema, keyValueEncodingType);

			this.keySchema.SchemaInfoProvider = new SchemaInfoProviderAnonymousInnerClass2(this);

			this.valueSchema.SchemaInfoProvider = new SchemaInfoProviderAnonymousInnerClass3(this);
		}

		private class SchemaInfoProviderAnonymousInnerClass2 : ISchemaInfoProvider
		{
			private readonly KeyValueSchema<K, V> outerInstance;

			public SchemaInfoProviderAnonymousInnerClass2(KeyValueSchema<K, V> outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public ValueTask<SchemaInfo> GetSchemaByVersion(sbyte[] schemaVersion)
			{
				return outerInstance.schemaInfoProvider.GetSchemaByVersion(schemaVersion).thenApply(si => KeyValueSchemaInfo.decodeKeyValueSchemaInfo(si).Key);
			}

			public CompletableFuture<SchemaInfo> LatestSchema
			{
				get
				{
					return CompletableFuture.completedFuture(((StructSchema<K>) outerInstance.keySchema).schemaInfo);
				}
			}

			public string TopicName
			{
				get
				{
					return "key-schema";
				}
			}
		}

		private class SchemaInfoProviderAnonymousInnerClass3 : ISchemaInfoProvider
		{
			private readonly KeyValueSchema<K, V> outerInstance;

			public SchemaInfoProviderAnonymousInnerClass3(KeyValueSchema<K, V> outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public ValueTask<SchemaInfo> GetSchemaByVersion(sbyte[] schemaVersion)
			{
				return outerInstance.schemaInfoProvider.GetSchemaByVersion(schemaVersion).thenApply(si => KeyValueSchemaInfo.decodeKeyValueSchemaInfo(si).Value);
			}

			public ValueTask<SchemaInfo> LatestSchema
			{
				get
				{
					return CompletableFuture.completedFuture(((StructSchema<V>) outerInstance.valueSchema).schemaInfo);
				}
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