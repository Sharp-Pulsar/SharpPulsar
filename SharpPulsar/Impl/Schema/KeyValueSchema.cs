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
namespace org.apache.pulsar.client.impl.schema
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
		private readonly Schema<K> keySchema;
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Getter private final org.apache.pulsar.client.api.Schema<V> valueSchema;
		private readonly Schema<V> valueSchema;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Getter private final org.apache.pulsar.common.schema.KeyValueEncodingType keyValueEncodingType;
		private readonly KeyValueEncodingType keyValueEncodingType;

		// schemaInfo combined by KeySchemaInfo and ValueSchemaInfo:
		//   [keyInfo.length][keyInfo][valueInfo.length][ValueInfo]
		private SchemaInfo schemaInfo;
		protected internal SchemaInfoProvider schemaInfoProvider;

		/// <summary>
		/// Key Value Schema using passed in schema type, support JSON and AVRO currently.
		/// </summary>
		public static Schema<KeyValue<K, V>> of<K, V>(Type key, Type value, SchemaType type)
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


		public static Schema<KeyValue<K, V>> of<K, V>(Schema<K> keySchema, Schema<V> valueSchema)
		{
			return new KeyValueSchema<KeyValue<K, V>>(keySchema, valueSchema, KeyValueEncodingType.INLINE);
		}

		public static Schema<KeyValue<K, V>> of<K, V>(Schema<K> keySchema, Schema<V> valueSchema, KeyValueEncodingType keyValueEncodingType)
		{
			return new KeyValueSchema<KeyValue<K, V>>(keySchema, valueSchema, keyValueEncodingType);
		}

		private static readonly Schema<KeyValue<sbyte[], sbyte[]>> KV_BYTES = new KeyValueSchema<KeyValue<sbyte[], sbyte[]>>(BytesSchema.of(), BytesSchema.of());

		public static Schema<KeyValue<sbyte[], sbyte[]>> kvBytes()
		{
			return KV_BYTES;
		}

		public override bool supportSchemaVersioning()
		{
			return keySchema.supportSchemaVersioning() || valueSchema.supportSchemaVersioning();
		}

		private KeyValueSchema(Schema<K> keySchema, Schema<V> valueSchema) : this(keySchema, valueSchema, KeyValueEncodingType.INLINE)
		{
		}

		private KeyValueSchema(Schema<K> keySchema, Schema<V> valueSchema, KeyValueEncodingType keyValueEncodingType)
		{
			this.keySchema = keySchema;
			this.valueSchema = valueSchema;
			this.keyValueEncodingType = keyValueEncodingType;
			this.schemaInfoProvider = new SchemaInfoProviderAnonymousInnerClass(this);
			// if either key schema or value schema requires fetching schema info,
			// we don't need to configure the key/value schema info right now.
			// defer configuring the key/value schema info until `configureSchemaInfo` is called.
			if (!requireFetchingSchemaInfo())
			{
				configureKeyValueSchemaInfo();
			}
		}

		private class SchemaInfoProviderAnonymousInnerClass : SchemaInfoProvider
		{
			private readonly KeyValueSchema<K, V> outerInstance;

			public SchemaInfoProviderAnonymousInnerClass(KeyValueSchema<K, V> outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public override CompletableFuture<SchemaInfo> getSchemaByVersion(sbyte[] schemaVersion)
			{
				return CompletableFuture.completedFuture(outerInstance.schemaInfo);
			}

			public override CompletableFuture<SchemaInfo> LatestSchema
			{
				get
				{
					return CompletableFuture.completedFuture(outerInstance.schemaInfo);
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
		public virtual sbyte[] encode(KeyValue<K, V> message)
		{
			if (keyValueEncodingType != null && keyValueEncodingType == KeyValueEncodingType.INLINE)
			{
				return KeyValue.encode(message.Key, keySchema, message.Value, valueSchema);
			}
			else
			{
				return valueSchema.encode(message.Value);
			}
		}

		public virtual KeyValue<K, V> decode(sbyte[] bytes)
		{
			return decode(bytes, null);
		}

		public virtual KeyValue<K, V> decode(sbyte[] bytes, sbyte[] schemaVersion)
		{
			if (this.keyValueEncodingType == KeyValueEncodingType.SEPARATED)
			{
				throw new SchemaSerializationException("This method cannot be used under this SEPARATED encoding type");
			}

			return KeyValue.decode(bytes, (keyBytes, valueBytes) => decode(keyBytes, valueBytes, schemaVersion));
		}

		public virtual KeyValue<K, V> decode(sbyte[] keyBytes, sbyte[] valueBytes, sbyte[] schemaVersion)
		{
			K k;
			if (keySchema.supportSchemaVersioning() && schemaVersion != null)
			{
				k = keySchema.decode(keyBytes, schemaVersion);
			}
			else
			{
				k = keySchema.decode(keyBytes);
			}
			V v;
			if (valueSchema.supportSchemaVersioning() && schemaVersion != null)
			{
				v = valueSchema.decode(valueBytes, schemaVersion);
			}
			else
			{
				v = valueSchema.decode(valueBytes);
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

		public virtual SchemaInfoProvider SchemaInfoProvider
		{
			set
			{
				this.schemaInfoProvider = value;
			}
		}

		public override bool requireFetchingSchemaInfo()
		{
			return keySchema.requireFetchingSchemaInfo() || valueSchema.requireFetchingSchemaInfo();
		}

		public override void configureSchemaInfo(string topicName, string componentName, SchemaInfo schemaInfo)
		{
			KeyValue<SchemaInfo, SchemaInfo> kvSchemaInfo = KeyValueSchemaInfo.decodeKeyValueSchemaInfo(schemaInfo);
			keySchema.configureSchemaInfo(topicName, "key", kvSchemaInfo.Key);
			valueSchema.configureSchemaInfo(topicName, "value", kvSchemaInfo.Value);
			configureKeyValueSchemaInfo();

			if (null == this.schemaInfo)
			{
				throw new Exception("No key schema info or value schema info : key = " + keySchema.SchemaInfo + ", value = " + valueSchema.SchemaInfo);
			}
		}

		private void configureKeyValueSchemaInfo()
		{
			this.schemaInfo = KeyValueSchemaInfo.encodeKeyValueSchemaInfo(keySchema, valueSchema, keyValueEncodingType);

			this.keySchema.SchemaInfoProvider = new SchemaInfoProviderAnonymousInnerClass2(this);

			this.valueSchema.SchemaInfoProvider = new SchemaInfoProviderAnonymousInnerClass3(this);
		}

		private class SchemaInfoProviderAnonymousInnerClass2 : SchemaInfoProvider
		{
			private readonly KeyValueSchema<K, V> outerInstance;

			public SchemaInfoProviderAnonymousInnerClass2(KeyValueSchema<K, V> outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public override CompletableFuture<SchemaInfo> getSchemaByVersion(sbyte[] schemaVersion)
			{
				return outerInstance.schemaInfoProvider.getSchemaByVersion(schemaVersion).thenApply(si => KeyValueSchemaInfo.decodeKeyValueSchemaInfo(si).Key);
			}

			public override CompletableFuture<SchemaInfo> LatestSchema
			{
				get
				{
					return CompletableFuture.completedFuture(((StructSchema<K>) outerInstance.keySchema).schemaInfo);
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
			private readonly KeyValueSchema<K, V> outerInstance;

			public SchemaInfoProviderAnonymousInnerClass3(KeyValueSchema<K, V> outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public override CompletableFuture<SchemaInfo> getSchemaByVersion(sbyte[] schemaVersion)
			{
				return outerInstance.schemaInfoProvider.getSchemaByVersion(schemaVersion).thenApply(si => KeyValueSchemaInfo.decodeKeyValueSchemaInfo(si).Value);
			}

			public override CompletableFuture<SchemaInfo> LatestSchema
			{
				get
				{
					return CompletableFuture.completedFuture(((StructSchema<V>) outerInstance.valueSchema).schemaInfo);
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