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
namespace SharpPulsar.Impl.Schema
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.google.common.@base.Preconditions.checkArgument;

	using Getter = lombok.Getter;
	using Slf4j = lombok.@extern.slf4j.Slf4j;
	using SharpPulsar.Api;
	using SchemaSerializationException = SharpPulsar.Api.SchemaSerializationException;
	using SchemaInfoProvider = SharpPulsar.Api.Schema.SchemaInfoProvider;
	using Org.Apache.Pulsar.Common.Schema;
	using KeyValueEncodingType = Org.Apache.Pulsar.Common.Schema.KeyValueEncodingType;
	using SchemaInfo = Org.Apache.Pulsar.Common.Schema.SchemaInfo;
	using SchemaType = Org.Apache.Pulsar.Common.Schema.SchemaType;

	/// <summary>
	/// [Key, Value] pair schema definition
	/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Slf4j public class KeyValueSchema<K, V> implements SharpPulsar.api.Schema<org.apache.pulsar.common.schema.KeyValue<K, V>>
	public class KeyValueSchema<K, V> : Schema<KeyValue<K, V>>
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Getter private final SharpPulsar.api.Schema<K> keySchema;
		private readonly Schema<K> keySchema;
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Getter private final SharpPulsar.api.Schema<V> valueSchema;
		private readonly Schema<V> valueSchema;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Getter private final org.apache.pulsar.common.schema.KeyValueEncodingType keyValueEncodingType;
		private readonly KeyValueEncodingType keyValueEncodingType;

		// schemaInfo combined by KeySchemaInfo and ValueSchemaInfo:
		//   [keyInfo.length][keyInfo][valueInfo.length][ValueInfo]
		private SchemaInfo schemaInfo;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		protected internal SchemaInfoProvider SchemaInfoProviderConflict;

		/// <summary>
		/// Key Value Schema using passed in schema type, support JSON and AVRO currently.
		/// </summary>
		public static Schema<KeyValue<K, V>> Of<K, V>(Type Key, Type Value, SchemaType Type)
		{
			checkArgument(SchemaType.JSON == Type || SchemaType.AVRO == Type);
			if (SchemaType.JSON == Type)
			{
				return new KeyValueSchema<KeyValue<K, V>>(JSONSchema.Of(Key), JSONSchema.Of(Value), KeyValueEncodingType.INLINE);
			}
			else
			{
				// AVRO
				return new KeyValueSchema<KeyValue<K, V>>(AvroSchema.Of(Key), AvroSchema.Of(Value), KeyValueEncodingType.INLINE);
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

		private static readonly Schema<KeyValue<sbyte[], sbyte[]>> KV_BYTES = new KeyValueSchema<KeyValue<sbyte[], sbyte[]>>(BytesSchema.Of(), BytesSchema.Of());

		public static Schema<KeyValue<sbyte[], sbyte[]>> KvBytes()
		{
			return KV_BYTES;
		}

		public override bool SupportSchemaVersioning()
		{
			return keySchema.SupportSchemaVersioning() || valueSchema.SupportSchemaVersioning();
		}

		private KeyValueSchema(Schema<K> KeySchema, Schema<V> ValueSchema) : this(KeySchema, ValueSchema, KeyValueEncodingType.INLINE)
		{
		}

		private KeyValueSchema(Schema<K> KeySchema, Schema<V> ValueSchema, KeyValueEncodingType KeyValueEncodingType)
		{
			this.keySchema = KeySchema;
			this.valueSchema = ValueSchema;
			this.keyValueEncodingType = KeyValueEncodingType;
			this.SchemaInfoProviderConflict = new SchemaInfoProviderAnonymousInnerClass(this);
			// if either key schema or value schema requires fetching schema info,
			// we don't need to configure the key/value schema info right now.
			// defer configuring the key/value schema info until `configureSchemaInfo` is called.
			if (!RequireFetchingSchemaInfo())
			{
				ConfigureKeyValueSchemaInfo();
			}
		}

		public class SchemaInfoProviderAnonymousInnerClass : SchemaInfoProvider
		{
			private readonly KeyValueSchema<K, V> outerInstance;

			public SchemaInfoProviderAnonymousInnerClass(KeyValueSchema<K, V> OuterInstance)
			{
				this.outerInstance = OuterInstance;
			}

			public CompletableFuture<SchemaInfo> getSchemaByVersion(sbyte[] SchemaVersion)
			{
				return CompletableFuture.completedFuture(outerInstance.schemaInfo);
			}

			public CompletableFuture<SchemaInfo> LatestSchema
			{
				get
				{
					return CompletableFuture.completedFuture(outerInstance.schemaInfo);
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
		public virtual sbyte[] Encode(KeyValue<K, V> Message)
		{
			if (keyValueEncodingType != null && keyValueEncodingType == KeyValueEncodingType.INLINE)
			{
				return KeyValue.encode(Message.Key, keySchema, Message.Value, valueSchema);
			}
			else
			{
				return valueSchema.Encode(Message.Value);
			}
		}

		public virtual KeyValue<K, V> Decode(sbyte[] Bytes)
		{
			return Decode(Bytes, null);
		}

		public virtual KeyValue<K, V> Decode(sbyte[] Bytes, sbyte[] SchemaVersion)
		{
			if (this.keyValueEncodingType == KeyValueEncodingType.SEPARATED)
			{
				throw new SchemaSerializationException("This method cannot be used under this SEPARATED encoding type");
			}

			return KeyValue.decode(Bytes, (keyBytes, valueBytes) => Decode(keyBytes, valueBytes, SchemaVersion));
		}

		public virtual KeyValue<K, V> Decode(sbyte[] KeyBytes, sbyte[] ValueBytes, sbyte[] SchemaVersion)
		{
			K K;
			if (keySchema.SupportSchemaVersioning() && SchemaVersion != null)
			{
				K = keySchema.Decode(KeyBytes, SchemaVersion);
			}
			else
			{
				K = keySchema.Decode(KeyBytes);
			}
			V V;
			if (valueSchema.SupportSchemaVersioning() && SchemaVersion != null)
			{
				V = valueSchema.Decode(ValueBytes, SchemaVersion);
			}
			else
			{
				V = valueSchema.Decode(ValueBytes);
			}
			return new KeyValue<K, V>(K, V);
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
				this.SchemaInfoProviderConflict = value;
			}
		}

		public override bool RequireFetchingSchemaInfo()
		{
			return keySchema.RequireFetchingSchemaInfo() || valueSchema.RequireFetchingSchemaInfo();
		}

		public override void ConfigureSchemaInfo(string TopicName, string ComponentName, SchemaInfo SchemaInfo)
		{
			KeyValue<SchemaInfo, SchemaInfo> KvSchemaInfo = KeyValueSchemaInfo.DecodeKeyValueSchemaInfo(SchemaInfo);
			keySchema.ConfigureSchemaInfo(TopicName, "key", KvSchemaInfo.Key);
			valueSchema.ConfigureSchemaInfo(TopicName, "value", KvSchemaInfo.Value);
			ConfigureKeyValueSchemaInfo();

			if (null == this.schemaInfo)
			{
				throw new Exception("No key schema info or value schema info : key = " + keySchema.SchemaInfo + ", value = " + valueSchema.SchemaInfo);
			}
		}

		private void ConfigureKeyValueSchemaInfo()
		{
			this.schemaInfo = KeyValueSchemaInfo.EncodeKeyValueSchemaInfo(keySchema, valueSchema, keyValueEncodingType);

			this.keySchema.SchemaInfoProvider = new SchemaInfoProviderAnonymousInnerClass2(this);

			this.valueSchema.SchemaInfoProvider = new SchemaInfoProviderAnonymousInnerClass3(this);
		}

		public class SchemaInfoProviderAnonymousInnerClass2 : SchemaInfoProvider
		{
			private readonly KeyValueSchema<K, V> outerInstance;

			public SchemaInfoProviderAnonymousInnerClass2(KeyValueSchema<K, V> OuterInstance)
			{
				this.outerInstance = OuterInstance;
			}

			public CompletableFuture<SchemaInfo> getSchemaByVersion(sbyte[] SchemaVersion)
			{
				return outerInstance.SchemaInfoProviderConflict.getSchemaByVersion(SchemaVersion).thenApply(si => KeyValueSchemaInfo.DecodeKeyValueSchemaInfo(si).Key);
			}

			public CompletableFuture<SchemaInfo> LatestSchema
			{
				get
				{
					return CompletableFuture.completedFuture(((StructSchema<K>) outerInstance.keySchema).SchemaInfoConflict);
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

		public class SchemaInfoProviderAnonymousInnerClass3 : SchemaInfoProvider
		{
			private readonly KeyValueSchema<K, V> outerInstance;

			public SchemaInfoProviderAnonymousInnerClass3(KeyValueSchema<K, V> OuterInstance)
			{
				this.outerInstance = OuterInstance;
			}

			public CompletableFuture<SchemaInfo> getSchemaByVersion(sbyte[] SchemaVersion)
			{
				return outerInstance.SchemaInfoProviderConflict.getSchemaByVersion(SchemaVersion).thenApply(si => KeyValueSchemaInfo.DecodeKeyValueSchemaInfo(si).Value);
			}

			public CompletableFuture<SchemaInfo> LatestSchema
			{
				get
				{
					return CompletableFuture.completedFuture(((StructSchema<V>) outerInstance.valueSchema).SchemaInfoConflict);
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