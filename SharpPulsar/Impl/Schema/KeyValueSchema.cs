using System;
using System.Threading.Tasks;
using SharpPulsar.Api;
using SharpPulsar.Api.Schema;
using SharpPulsar.Common.Enum;
using SharpPulsar.Common.Schema;
using SharpPulsar.Exceptions;
using SharpPulsar.Protocol.Proto;
using SharpPulsar.Shared;

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
	public class KeyValueSchema<TK, TV> : ISchema<KeyValue<TK, TV>>
	{
		private readonly ISchema<TK> keySchema;
		private readonly ISchema<TV> valueSchema;

		private readonly KeyValueEncodingType keyValueEncodingType;

		// schemaInfo combined by KeySchemaInfo and ValueSchemaInfo:
		//   [keyInfo.length][keyInfo][valueInfo.length][ValueInfo]
		private SchemaInfo schemaInfo;
		protected internal ISchemaInfoProvider _schemaInfoProvider;

		/// <summary>
		/// Key Value Schema using passed in schema type, support JSON and AVRO currently.
		/// </summary>
		public static ISchema<KeyValue<TK, TV>> Of(TK key, TV value, SchemaType type)
		{
			Precondition.Condition.CheckArgument(SchemaType.Json == type || SchemaType.Avro == type);
			if (SchemaType.Json == type)
			{
				return new KeyValueSchema<TK, TV>(JsonSchema<TK>.Of(key), JsonSchema<TV>.Of(value), KeyValueEncodingType.Inline);
			}
			else
            {
                return null;
                // AVRO
                //return new KeyValueSchema<TK, TV>(AvroSchema.Of(key), AvroSchema.Of(value), KeyValueEncodingType.Inline);
            }
		}


		public static ISchema<KeyValue<TK, TV>> Of<TK, TV>(ISchema<TK> keySchema, ISchema<TV> valueSchema)
		{
			return new KeyValueSchema<TK, TV>(keySchema, valueSchema, KeyValueEncodingType.Inline);
		}

		public static ISchema<KeyValue<TK, TV>> Of<TK, TV>(ISchema<TK> keySchema, ISchema<TV> valueSchema, KeyValueEncodingType keyValueEncodingType)
		{
			return new KeyValueSchema<TK, TV>(keySchema, valueSchema, keyValueEncodingType);
		}

		private static readonly ISchema<KeyValue<sbyte[], sbyte[]>> KV_BYTES = new KeyValueSchema<sbyte[], sbyte[]>(BytesSchema.Of(), BytesSchema.Of());

		public static ISchema<KeyValue<sbyte[], sbyte[]>> KvBytes()
		{
			return KV_BYTES;
		}

		public  bool SupportSchemaVersioning()
		{
			return keySchema.SupportSchemaVersioning() || valueSchema.SupportSchemaVersioning();
		}

		private KeyValueSchema(ISchema<TK> keySchema, ISchema<TV> valueSchema) : this(keySchema, valueSchema, KeyValueEncodingType.Inline)
		{
		}

		private KeyValueSchema(ISchema<TK> keySchema, ISchema<TV> valueSchema, KeyValueEncodingType keyValueEncodingType)
		{
			this.keySchema = keySchema;
			this.valueSchema = valueSchema;
			this.keyValueEncodingType = keyValueEncodingType;
			this._schemaInfoProvider = new SchemaInfoProviderAnonymousInnerClass(this);
			// if either key schema or value schema requires fetching schema info,
			// we don't need to configure the key/value schema info right now.
			// defer configuring the key/value schema info until `configureSchemaInfo` is called.
			if (!RequireFetchingSchemaInfo())
			{
				ConfigureKeyValueSchemaInfo();
			}
		}

		public class SchemaInfoProviderAnonymousInnerClass : ISchemaInfoProvider
		{
			private readonly KeyValueSchema<TK, TV> outerInstance;

			public SchemaInfoProviderAnonymousInnerClass(KeyValueSchema<TK, TV> outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public ValueTask<ISchemaInfo> GetSchemaByVersion(sbyte[] schemaVersion)
			{
				return new ValueTask<ISchemaInfo>(outerInstance.schemaInfo);
			}

			public ValueTask<ISchemaInfo> LatestSchema => new ValueTask<ISchemaInfo>(outerInstance.schemaInfo);

            public string TopicName => "key-value-schema";
        }

		// encode as bytes: [key.length][key.bytes][value.length][value.bytes] or [value.bytes]
		public virtual sbyte[] Encode(KeyValue<TK, TV> message)
		{
			if (keyValueEncodingType == KeyValueEncodingType.Inline)
			{
				return KeyValue<TK, TV>.Encode(message.Key, keySchema, message.Value, valueSchema);
			}
			else
			{
				return valueSchema.Encode(message.Value);
			}
		}

		public virtual KeyValue<TK, TV> Decode(sbyte[] bytes)
		{
			return Decode(bytes, null);
		}

		public virtual KeyValue<TK, TV> Decode(sbyte[] bytes, sbyte[] schemaVersion)
		{
			if (this.keyValueEncodingType == KeyValueEncodingType.Separated)
			{
				throw new SchemaSerializationException("This method cannot be used under this SEPARATED encoding type");
			}

			return KeyValue<TK, TV>.Decode(bytes, (keyBytes, valueBytes) => Decode(keyBytes, valueBytes, schemaVersion));
		}

		public virtual KeyValue<TK, TV> Decode(sbyte[] keyBytes, sbyte[] valueBytes, sbyte[] schemaVersion)
		{
			TK k;
			if (keySchema.SupportSchemaVersioning() && schemaVersion != null)
			{
				k = keySchema.Decode(keyBytes, schemaVersion);
			}
			else
			{
				k = keySchema.Decode(keyBytes);
			}
			TV v;
			if (valueSchema.SupportSchemaVersioning() && schemaVersion != null)
			{
				v = valueSchema.Decode(valueBytes, schemaVersion);
			}
			else
			{
				v = valueSchema.Decode(valueBytes);
			}
			return new KeyValue<TK, TV>(k, v);
		}

		public ISchemaInfo SchemaInfo => this.schemaInfo;

        public  ISchemaInfoProvider SchemaInfoProvider
		{
			set => this._schemaInfoProvider = value;
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

		public class SchemaInfoProviderAnonymousInnerClass2 : ISchemaInfoProvider
		{
			private readonly KeyValueSchema<TK, TV> outerInstance;

			public SchemaInfoProviderAnonymousInnerClass2(KeyValueSchema<TK, TV> outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public ValueTask<ISchemaInfo> GetSchemaByVersion(sbyte[] schemaVersion)
			{
				var oy =  outerInstance._schemaInfoProvider.GetSchemaByVersion(schemaVersion).AsTask().ContinueWith(si => new ValueTask<ISchemaInfo>(KeyValueSchemaInfo.DecodeKeyValueSchemaInfo((SchemaInfo)si.Result).Key));
				return oy.Result;
            }

			public ValueTask<ISchemaInfo> LatestSchema => new ValueTask<ISchemaInfo>(((StructSchema<TK>) outerInstance.keySchema).SchemaInfo);

            public string TopicName => "key-schema";
        }

		public class SchemaInfoProviderAnonymousInnerClass3 : ISchemaInfoProvider
		{
			private readonly KeyValueSchema<TK, TV> _outerInstance;

			public SchemaInfoProviderAnonymousInnerClass3(KeyValueSchema<TK, TV> outerInstance)
			{
				this._outerInstance = outerInstance;
			}

			public ValueTask<ISchemaInfo> GetSchemaByVersion(sbyte[] schemaVersion)
			{
                var oy = _outerInstance._schemaInfoProvider.GetSchemaByVersion(schemaVersion).AsTask().ContinueWith(si => new ValueTask<ISchemaInfo>(KeyValueSchemaInfo.DecodeKeyValueSchemaInfo((SchemaInfo)si.Result).Key));
                return oy.Result;
			}

			public ValueTask<ISchemaInfo> LatestSchema => new ValueTask<ISchemaInfo>(((StructSchema<TV>) _outerInstance.valueSchema).SchemaInfo);

            public string TopicName => "value-schema";
        }
	}

}