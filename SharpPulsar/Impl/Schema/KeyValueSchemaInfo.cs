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
namespace SharpPulsar.Impl.Schema
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.google.common.@base.Preconditions.checkArgument;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.google.common.@base.Preconditions.checkNotNull;

	using Slf4j = lombok.@extern.slf4j.Slf4j;
	using StringUtils = org.apache.commons.lang3.StringUtils;
	using SharpPulsar.Api;
	using Org.Apache.Pulsar.Common.Schema;
	using KeyValueEncodingType = Org.Apache.Pulsar.Common.Schema.KeyValueEncodingType;
	using SchemaInfo = Org.Apache.Pulsar.Common.Schema.SchemaInfo;
	using SchemaType = Org.Apache.Pulsar.Common.Schema.SchemaType;

	/// <summary>
	/// Util class for processing key/value schema info.
	/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Slf4j public final class KeyValueSchemaInfo
	public sealed class KeyValueSchemaInfo
	{

		private static readonly Schema<SchemaInfo> SCHEMA_INFO_WRITER = new SchemaAnonymousInnerClass();

		public class SchemaAnonymousInnerClass : Schema<SchemaInfo>
		{
			public sbyte[] encode(SchemaInfo Si)
			{
				return Si.Schema;
			}

			public SchemaInfo SchemaInfo
			{
				get
				{
					return BytesSchema.BYTES.SchemaInfo;
				}
			}
		}

		private const string KeySchemaName = "key.schema.name";
		private const string KeySchemaType = "key.schema.type";
		private const string KeySchemaProps = "key.schema.properties";
		private const string ValueSchemaName = "value.schema.name";
		private const string ValueSchemaType = "value.schema.type";
		private const string ValueSchemaProps = "value.schema.properties";
		private const string KvEncodingType = "kv.encoding.type";

		/// <summary>
		/// Decode the kv encoding type from the schema info.
		/// </summary>
		/// <param name="schemaInfo"> the schema info </param>
		/// <returns> the kv encoding type </returns>
		public static KeyValueEncodingType? DecodeKeyValueEncodingType(SchemaInfo SchemaInfo)
		{
			checkArgument(SchemaType.KEY_VALUE == SchemaInfo.Type, "Not a KeyValue schema");

			string EncodingTypeStr = SchemaInfo.Properties.get(KvEncodingType);
			if (StringUtils.isEmpty(EncodingTypeStr))
			{
				return KeyValueEncodingType.INLINE;
			}
			else
			{
				return Enum.Parse(typeof(KeyValueEncodingType), EncodingTypeStr);
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
			return EncodeKeyValueSchemaInfo(SchemaName, KeySchema.SchemaInfo, ValueSchema.SchemaInfo, KeyValueEncodingType);
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
			sbyte[] SchemaData = KeyValue.encode(KeySchemaInfo, SCHEMA_INFO_WRITER, ValueSchemaInfo, SCHEMA_INFO_WRITER);

			// process key/value schema properties
			IDictionary<string, string> Properties = new Dictionary<string, string>();
			EncodeSubSchemaInfoToParentSchemaProperties(KeySchemaInfo, KeySchemaName, KeySchemaType, KeySchemaProps, Properties);

			EncodeSubSchemaInfoToParentSchemaProperties(ValueSchemaInfo, ValueSchemaName, ValueSchemaType, ValueSchemaProps, Properties);
			Properties[KvEncodingType] = KeyValueEncodingType.ToString();

			// generate the final schema info
			return (new SchemaInfo()).setName(SchemaName).setType(SchemaType.KEY_VALUE).setSchema(SchemaData).setProperties(Properties);
		}

		private static void EncodeSubSchemaInfoToParentSchemaProperties(SchemaInfo SchemaInfo, string SchemaNameProperty, string SchemaTypeProperty, string SchemaPropsProperty, IDictionary<string, string> ParentSchemaProperties)
		{
			ParentSchemaProperties[SchemaNameProperty] = SchemaInfo.Name;
			ParentSchemaProperties[SchemaTypeProperty] = SchemaInfo.Type.ToString();
			ParentSchemaProperties[SchemaPropsProperty] = SchemaUtils.SerializeSchemaProperties(SchemaInfo.Properties);
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
			SchemaInfo KeySchemaInfo = DecodeSubSchemaInfo(SchemaInfo, KeySchemaName, KeySchemaType, KeySchemaProps, keyBytes);
			SchemaInfo ValueSchemaInfo = DecodeSubSchemaInfo(SchemaInfo, ValueSchemaName, ValueSchemaType, ValueSchemaProps, valueBytes);
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
				SchemaProps = SchemaUtils.DeserializeSchemaProperties(SchemaPropsStr);
			}
			return SchemaInfo.builder().name(SchemaName).type(SchemaType).schema(SchemaData).properties(SchemaProps).build();
		}

		private KeyValueSchemaInfo()
		{
		}
	}

}