using System.Collections.Generic;

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
	/// Util class for processing key/value schema info.
	/// </summary>
	//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
	//ORIGINAL LINE: @Slf4j public final class KeyValueSchemaInfo
	public sealed class KeyValueSchemaInfo
	{

		private static readonly Interfaces.Interceptor.ISchema<SchemaInfo> _schemaInfoWriter = new SchemaAnonymousInnerClass();

		private class SchemaAnonymousInnerClass : Schema<SchemaInfo>
		{
			public override sbyte[] Encode(SchemaInfo Si)
			{
				return Si.Schema;
			}

			public override SchemaInfo SchemaInfo
			{
				get
				{
					return BytesSchema.BYTES.SchemaInfo;
				}
			}

			public override Schema<SchemaInfo> Clone()
			{
				return this;
			}
		}

		private const string KEY_SCHEMA_NAME = "key.schema.name";
		private const string KEY_SCHEMA_TYPE = "key.schema.type";
		private const string KEY_SCHEMA_PROPS = "key.schema.properties";
		private const string VALUE_SCHEMA_NAME = "value.schema.name";
		private const string VALUE_SCHEMA_TYPE = "value.schema.type";
		private const string VALUE_SCHEMA_PROPS = "value.schema.properties";
		private const string KV_ENCODING_TYPE = "kv.encoding.type";

		/// <summary>
		/// Decode the kv encoding type from the schema info.
		/// </summary>
		/// <param name="schemaInfo"> the schema info </param>
		/// <returns> the kv encoding type </returns>
		public static KeyValueEncodingType DecodeKeyValueEncodingType(SchemaInfo SchemaInfo)
		{
			checkArgument(SchemaType.KEY_VALUE == SchemaInfo.Type, "Not a KeyValue schema");

			string EncodingTypeStr = SchemaInfo.Properties.get(KV_ENCODING_TYPE);
			if (StringUtils.isEmpty(EncodingTypeStr))
			{
				return KeyValueEncodingType.INLINE;
			}
			else
			{
				return KeyValueEncodingType.valueOf(EncodingTypeStr);
			}
		}

		/// <summary>
		/// Encode key & value into schema into a KeyValue schema.
		/// </summary>
		/// <param name="keySchema"> the key schema </param>
		/// <param name="valueSchema"> the value schema </param>
		/// <param name="keyValueEncodingType"> the encoding type to encode and decode key value pair </param>
		/// <returns> the final schema info </returns>
		public static SchemaInfo EncodeKeyValueSchemaInfo<K, V>(Schema<K> KeySchema, Schema<V> ValueSchema, KeyValueEncodingType KeyValueEncodingType)
		{
			return EncodeKeyValueSchemaInfo("KeyValue", KeySchema, ValueSchema, KeyValueEncodingType);
		}

		/// <summary>
		/// Encode key & value into schema into a KeyValue schema.
		/// </summary>
		/// <param name="schemaName"> the final schema name </param>
		/// <param name="keySchema"> the key schema </param>
		/// <param name="valueSchema"> the value schema </param>
		/// <param name="keyValueEncodingType"> the encoding type to encode and decode key value pair </param>
		/// <returns> the final schema info </returns>
		public static SchemaInfo EncodeKeyValueSchemaInfo<K, V>(string SchemaName, Schema<K> KeySchema, Schema<V> ValueSchema, KeyValueEncodingType KeyValueEncodingType)
		{
			return encodeKeyValueSchemaInfo(SchemaName, KeySchema.SchemaInfo, ValueSchema.SchemaInfo, KeyValueEncodingType);
		}

		/// <summary>
		/// Encode key & value into schema into a KeyValue schema.
		/// </summary>
		/// <param name="schemaName"> the final schema name </param>
		/// <param name="keySchemaInfo"> the key schema info </param>
		/// <param name="valueSchemaInfo"> the value schema info </param>
		/// <param name="keyValueEncodingType"> the encoding type to encode and decode key value pair </param>
		/// <returns> the final schema info </returns>
		public static SchemaInfo EncodeKeyValueSchemaInfo(string SchemaName, SchemaInfo KeySchemaInfo, SchemaInfo ValueSchemaInfo, KeyValueEncodingType KeyValueEncodingType)
		{
			checkNotNull(KeyValueEncodingType, "Null encoding type is provided");

			if (KeySchemaInfo == null || ValueSchemaInfo == null)
			{
				// schema is not ready
				return null;
			}

			// process key/value schema data
			sbyte[] SchemaData = KeyValue.encode(KeySchemaInfo, _schemaInfoWriter, ValueSchemaInfo, _schemaInfoWriter);

			// process key/value schema properties
			IDictionary<string, string> Properties = new Dictionary<string, string>();
			EncodeSubSchemaInfoToParentSchemaProperties(KeySchemaInfo, KEY_SCHEMA_NAME, KEY_SCHEMA_TYPE, KEY_SCHEMA_PROPS, Properties);

			EncodeSubSchemaInfoToParentSchemaProperties(ValueSchemaInfo, VALUE_SCHEMA_NAME, VALUE_SCHEMA_TYPE, VALUE_SCHEMA_PROPS, Properties);
			Properties[KV_ENCODING_TYPE] = KeyValueEncodingType.ToString();

			// generate the final schema info
			return (new SchemaInfo()).setName(SchemaName).setType(SchemaType.KEY_VALUE).setSchema(SchemaData).setProperties(Properties);
		}

		private static void EncodeSubSchemaInfoToParentSchemaProperties(SchemaInfo SchemaInfo, string SchemaNameProperty, string SchemaTypeProperty, string SchemaPropsProperty, IDictionary<string, string> ParentSchemaProperties)
		{
			ParentSchemaProperties[SchemaNameProperty] = SchemaInfo.Name;
			ParentSchemaProperties[SchemaTypeProperty] = SchemaInfo.Type.ToString();
			ParentSchemaProperties[SchemaPropsProperty] = SchemaUtils.serializeSchemaProperties(SchemaInfo.Properties);
		}

		/// <summary>
		/// Decode the key/value schema info to get key schema info and value schema info.
		/// </summary>
		/// <param name="schemaInfo"> key/value schema info. </param>
		/// <returns> the pair of key schema info and value schema info </returns>
		public static KeyValue<SchemaInfo, SchemaInfo> DecodeKeyValueSchemaInfo(SchemaInfo SchemaInfo)
		{
			checkArgument(SchemaType.KEY_VALUE == SchemaInfo.Type, "Not a KeyValue schema");

			return KeyValue.decode(SchemaInfo.Schema, (keyBytes, valueBytes) =>
			{
				SchemaInfo KeySchemaInfo = DecodeSubSchemaInfo(SchemaInfo, KEY_SCHEMA_NAME, KEY_SCHEMA_TYPE, KEY_SCHEMA_PROPS, keyBytes);
				SchemaInfo ValueSchemaInfo = DecodeSubSchemaInfo(SchemaInfo, VALUE_SCHEMA_NAME, VALUE_SCHEMA_TYPE, VALUE_SCHEMA_PROPS, valueBytes);
				return new KeyValue<SchemaInfo, SchemaInfo>(KeySchemaInfo, ValueSchemaInfo);
			});
		}

		private static SchemaInfo DecodeSubSchemaInfo(SchemaInfo ParentSchemaInfo, string SchemaNameProperty, string SchemaTypeProperty, string SchemaPropsProperty, sbyte[] SchemaData)
		{
			IDictionary<string, string> ParentSchemaProps = ParentSchemaInfo.Properties;
			string SchemaName = ParentSchemaProps.getOrDefault(SchemaNameProperty, "");
			SchemaType SchemaType = SchemaType.valueOf(ParentSchemaProps.getOrDefault(SchemaTypeProperty, SchemaType.BYTES.name()));
			IDictionary<string, string> SchemaProps;
			string SchemaPropsStr = ParentSchemaProps[SchemaPropsProperty];
			if (StringUtils.isEmpty(SchemaPropsStr))
			{
				SchemaProps = Collections.emptyMap();
			}
			else
			{
				SchemaProps = SchemaUtils.deserializeSchemaProperties(SchemaPropsStr);
			}
			return SchemaInfo.builder().name(SchemaName).type(SchemaType).schema(SchemaData).properties(SchemaProps).build();
		}

		private KeyValueSchemaInfo()
		{
		}
	}
}
