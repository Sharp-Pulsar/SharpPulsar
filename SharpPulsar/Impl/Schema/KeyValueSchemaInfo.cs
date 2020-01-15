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
namespace org.apache.pulsar.client.impl.schema
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.google.common.@base.Preconditions.checkArgument;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.google.common.@base.Preconditions.checkNotNull;

	using Slf4j = lombok.@extern.slf4j.Slf4j;
	using StringUtils = org.apache.commons.lang3.StringUtils;
	using Schema = org.apache.pulsar.client.api.Schema;
	using KeyValue = org.apache.pulsar.common.schema.KeyValue;
	using KeyValueEncodingType = org.apache.pulsar.common.schema.KeyValueEncodingType;
	using SchemaInfo = org.apache.pulsar.common.schema.SchemaInfo;
	using SchemaType = org.apache.pulsar.common.schema.SchemaType;

	/// <summary>
	/// Util class for processing key/value schema info.
	/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Slf4j public final class KeyValueSchemaInfo
	public sealed class KeyValueSchemaInfo
	{

		private static readonly Schema<SchemaInfo> SCHEMA_INFO_WRITER = new SchemaAnonymousInnerClass();

		private class SchemaAnonymousInnerClass : Schema<SchemaInfo>
		{
			public override sbyte[] encode(SchemaInfo si)
			{
				return si.Schema;
			}

			public override SchemaInfo SchemaInfo
			{
				get
				{
					return BytesSchema.BYTES.SchemaInfo;
				}
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
		public static KeyValueEncodingType decodeKeyValueEncodingType(SchemaInfo schemaInfo)
		{
			checkArgument(SchemaType.KEY_VALUE == schemaInfo.Type, "Not a KeyValue schema");

			string encodingTypeStr = schemaInfo.Properties.get(KV_ENCODING_TYPE);
			if (StringUtils.isEmpty(encodingTypeStr))
			{
				return KeyValueEncodingType.INLINE;
			}
			else
			{
				return KeyValueEncodingType.valueOf(encodingTypeStr);
			}
		}

		/// <summary>
		/// Encode key & value into schema into a KeyValue schema.
		/// </summary>
		/// <param name="keySchema"> the key schema </param>
		/// <param name="valueSchema"> the value schema </param>
		/// <param name="keyValueEncodingType"> the encoding type to encode and decode key value pair </param>
		/// <returns> the final schema info </returns>
		public static SchemaInfo encodeKeyValueSchemaInfo<K, V>(Schema<K> keySchema, Schema<V> valueSchema, KeyValueEncodingType keyValueEncodingType)
		{
			return encodeKeyValueSchemaInfo("KeyValue", keySchema, valueSchema, keyValueEncodingType);
		}

		/// <summary>
		/// Encode key & value into schema into a KeyValue schema.
		/// </summary>
		/// <param name="schemaName"> the final schema name </param>
		/// <param name="keySchema"> the key schema </param>
		/// <param name="valueSchema"> the value schema </param>
		/// <param name="keyValueEncodingType"> the encoding type to encode and decode key value pair </param>
		/// <returns> the final schema info </returns>
		public static SchemaInfo encodeKeyValueSchemaInfo<K, V>(string schemaName, Schema<K> keySchema, Schema<V> valueSchema, KeyValueEncodingType keyValueEncodingType)
		{
			return encodeKeyValueSchemaInfo(schemaName, keySchema.SchemaInfo, valueSchema.SchemaInfo, keyValueEncodingType);
		}

		/// <summary>
		/// Encode key & value into schema into a KeyValue schema.
		/// </summary>
		/// <param name="schemaName"> the final schema name </param>
		/// <param name="keySchemaInfo"> the key schema info </param>
		/// <param name="valueSchemaInfo"> the value schema info </param>
		/// <param name="keyValueEncodingType"> the encoding type to encode and decode key value pair </param>
		/// <returns> the final schema info </returns>
		public static SchemaInfo encodeKeyValueSchemaInfo(string schemaName, SchemaInfo keySchemaInfo, SchemaInfo valueSchemaInfo, KeyValueEncodingType keyValueEncodingType)
		{
			checkNotNull(keyValueEncodingType, "Null encoding type is provided");

			if (keySchemaInfo == null || valueSchemaInfo == null)
			{
				// schema is not ready
				return null;
			}

			// process key/value schema data
			sbyte[] schemaData = KeyValue.encode(keySchemaInfo, SCHEMA_INFO_WRITER, valueSchemaInfo, SCHEMA_INFO_WRITER);

			// process key/value schema properties
			IDictionary<string, string> properties = new Dictionary<string, string>();
			encodeSubSchemaInfoToParentSchemaProperties(keySchemaInfo, KEY_SCHEMA_NAME, KEY_SCHEMA_TYPE, KEY_SCHEMA_PROPS, properties);

			encodeSubSchemaInfoToParentSchemaProperties(valueSchemaInfo, VALUE_SCHEMA_NAME, VALUE_SCHEMA_TYPE, VALUE_SCHEMA_PROPS, properties);
			properties[KV_ENCODING_TYPE] = keyValueEncodingType.ToString();

			// generate the final schema info
			return (new SchemaInfo()).setName(schemaName).setType(SchemaType.KEY_VALUE).setSchema(schemaData).setProperties(properties);
		}

		private static void encodeSubSchemaInfoToParentSchemaProperties(SchemaInfo schemaInfo, string schemaNameProperty, string schemaTypeProperty, string schemaPropsProperty, IDictionary<string, string> parentSchemaProperties)
		{
			parentSchemaProperties[schemaNameProperty] = schemaInfo.Name;
			parentSchemaProperties[schemaTypeProperty] = schemaInfo.Type.ToString();
			parentSchemaProperties[schemaPropsProperty] = SchemaUtils.serializeSchemaProperties(schemaInfo.Properties);
		}

		/// <summary>
		/// Decode the key/value schema info to get key schema info and value schema info.
		/// </summary>
		/// <param name="schemaInfo"> key/value schema info. </param>
		/// <returns> the pair of key schema info and value schema info </returns>
		public static KeyValue<SchemaInfo, SchemaInfo> decodeKeyValueSchemaInfo(SchemaInfo schemaInfo)
		{
			checkArgument(SchemaType.KEY_VALUE == schemaInfo.Type, "Not a KeyValue schema");

			return KeyValue.decode(schemaInfo.Schema, (keyBytes, valueBytes) =>
			{
			SchemaInfo keySchemaInfo = decodeSubSchemaInfo(schemaInfo, KEY_SCHEMA_NAME, KEY_SCHEMA_TYPE, KEY_SCHEMA_PROPS, keyBytes);
			SchemaInfo valueSchemaInfo = decodeSubSchemaInfo(schemaInfo, VALUE_SCHEMA_NAME, VALUE_SCHEMA_TYPE, VALUE_SCHEMA_PROPS, valueBytes);
			return new KeyValue<org.apache.pulsar.common.schema.SchemaInfo, org.apache.pulsar.common.schema.SchemaInfo>(keySchemaInfo, valueSchemaInfo);
			});
		}

		private static SchemaInfo decodeSubSchemaInfo(SchemaInfo parentSchemaInfo, string schemaNameProperty, string schemaTypeProperty, string schemaPropsProperty, sbyte[] schemaData)
		{
			IDictionary<string, string> parentSchemaProps = parentSchemaInfo.Properties;
			string schemaName = parentSchemaProps.getOrDefault(schemaNameProperty, "");
			SchemaType schemaType = SchemaType.valueOf(parentSchemaProps.getOrDefault(schemaTypeProperty, SchemaType.BYTES.name()));
			IDictionary<string, string> schemaProps;
			string schemaPropsStr = parentSchemaProps[schemaPropsProperty];
			if (StringUtils.isEmpty(schemaPropsStr))
			{
				schemaProps = Collections.emptyMap();
			}
			else
			{
				schemaProps = SchemaUtils.deserializeSchemaProperties(schemaPropsStr);
			}
			return SchemaInfo.builder().name(schemaName).type(schemaType).schema(schemaData).properties(schemaProps).build();
		}

		private KeyValueSchemaInfo()
		{
		}
	}

}