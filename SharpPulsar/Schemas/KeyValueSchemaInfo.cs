using SharpPulsar.Extension;
using SharpPulsar.Interfaces;
using SharpPulsar.Interfaces.ISchema;
using SharpPulsar.Precondition;
using SharpPulsar.Shared;
using System;
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
namespace SharpPulsar.Schemas
{

	/// <summary>
	/// Util class for processing key/value schema info.
	/// </summary>
	public sealed class KeyValueSchemaInfo
	{

		private static readonly ISchema<ISchemaInfo> _schemaInfoWriter = new SchemaAnonymousInnerClass();

		private class SchemaAnonymousInnerClass : ISchema<ISchemaInfo>
		{
			public byte[] Encode(ISchemaInfo si)
			{
				return si.Schema;
			}

			public ISchemaInfo SchemaInfo
			{
				get
				{
					return BytesSchema.Of().SchemaInfo;
				}
			}

			public ISchema<ISchemaInfo> Clone()
			{
				return this;
			}

            object System.ICloneable.Clone()
            {
                throw new System.NotImplementedException();
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
		public static KeyValueEncodingType DecodeKeyValueEncodingType(ISchemaInfo schemaInfo)
		{
			Condition.CheckArgument(SchemaType.KeyValue == schemaInfo.Type, "Not a KeyValue schema");

			if (!schemaInfo.Properties.TryGetValue(KV_ENCODING_TYPE, out var encodingTypeStr))
			{
				return KeyValueEncodingType.INLINE;
			}
			else
			{
				return (KeyValueEncodingType)Enum.Parse(typeof(KeyValueEncodingType), encodingTypeStr);
			}
		}

		/// <summary>
		/// Encode key & value into schema into a KeyValue schema.
		/// </summary>
		/// <param name="keySchema"> the key schema </param>
		/// <param name="valueSchema"> the value schema </param>
		/// <param name="keyValueEncodingType"> the encoding type to encode and decode key value pair </param>
		/// <returns> the final schema info </returns>
		public static ISchemaInfo EncodeKeyValueSchemaInfo<K, V>(ISchema<K> KeySchema, ISchema<V> ValueSchema, KeyValueEncodingType KeyValueEncodingType)
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
		public static ISchemaInfo EncodeKeyValueSchemaInfo<K, V>(string SchemaName, ISchema<K> KeySchema, ISchema<V> ValueSchema, KeyValueEncodingType KeyValueEncodingType)
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
		public static ISchemaInfo EncodeKeyValueSchemaInfo(string schemaName, ISchemaInfo keySchemaInfo, ISchemaInfo valueSchemaInfo, KeyValueEncodingType keyValueEncodingType)
		{
			Condition.CheckNotNull(keyValueEncodingType, "Null encoding type is provided");

			if (keySchemaInfo == null || valueSchemaInfo == null)
			{
				// schema is not ready
				return null;
			}

			// process key/value schema data
			byte[] schemaData = KeyValue<ISchemaInfo, ISchemaInfo>.Encode(keySchemaInfo, _schemaInfoWriter, valueSchemaInfo, _schemaInfoWriter);

			// process key/value schema properties
			IDictionary<string, string> Properties = new Dictionary<string, string>();
			EncodeSubSchemaInfoToParentSchemaProperties(keySchemaInfo, KEY_SCHEMA_NAME, KEY_SCHEMA_TYPE, KEY_SCHEMA_PROPS, Properties);

			EncodeSubSchemaInfoToParentSchemaProperties(valueSchemaInfo, VALUE_SCHEMA_NAME, VALUE_SCHEMA_TYPE, VALUE_SCHEMA_PROPS, Properties);
			Properties[KV_ENCODING_TYPE] = keyValueEncodingType.ToString();

			// generate the final schema info
			return new SchemaInfo
			{
				Name = schemaName,
				Type = SchemaType.KeyValue,
				Schema = schemaData,
				Properties = Properties
			};
		}

		private static void EncodeSubSchemaInfoToParentSchemaProperties(ISchemaInfo schemaInfo, string schemaNameProperty, string schemaTypeProperty, string schemaPropsProperty, IDictionary<string, string> parentSchemaProperties)
		{
			parentSchemaProperties[schemaNameProperty] = schemaInfo.Name;
			parentSchemaProperties[schemaTypeProperty] = schemaInfo.Type.ToString();
			parentSchemaProperties[schemaPropsProperty] = SchemaUtils.SerializeSchemaProperties(schemaInfo.Properties);
		}

		/// <summary>
		/// Decode the key/value schema info to get key schema info and value schema info.
		/// </summary>
		/// <param name="schemaInfo"> key/value schema info. </param>
		/// <returns> the pair of key schema info and value schema info </returns>
		public static KeyValue<ISchemaInfo, ISchemaInfo> DecodeKeyValueSchemaInfo(ISchemaInfo SchemaInfo)
		{
			Condition.CheckArgument(SchemaType.KeyValue == SchemaInfo.Type, "Not a KeyValue schema");

			return KeyValue<ISchemaInfo, ISchemaInfo>.Decode(SchemaInfo.Schema, (keyBytes, valueBytes) =>
			{
				ISchemaInfo KeySchemaInfo = DecodeSubSchemaInfo(SchemaInfo, KEY_SCHEMA_NAME, KEY_SCHEMA_TYPE, KEY_SCHEMA_PROPS, keyBytes);
				ISchemaInfo ValueSchemaInfo = DecodeSubSchemaInfo(SchemaInfo, VALUE_SCHEMA_NAME, VALUE_SCHEMA_TYPE, VALUE_SCHEMA_PROPS, valueBytes);
				return new KeyValue<ISchemaInfo, ISchemaInfo>(KeySchemaInfo, ValueSchemaInfo);
			});
		}

		private static ISchemaInfo DecodeSubSchemaInfo(ISchemaInfo parentSchemaInfo, string schemaNameProperty, string schemaTypeProperty, string schemaPropsProperty, byte[] schemaData)
		{
			var parentSchemaProps = parentSchemaInfo.Properties;
			var schemaName = parentSchemaProps.GetOrDefault(schemaNameProperty, "");
			var schemaType = SchemaType.ValueOf(parentSchemaProps.GetOrDefault(schemaTypeProperty, SchemaType.BYTES.Name));
			IDictionary<string, string> schemaProps;
			if (!parentSchemaProps.TryGetValue(schemaPropsProperty, out var schemaPropsStr))
			{
				schemaProps = new Dictionary<string, string>();
			}
			else
			{
				schemaProps = SchemaUtils.DeserializeSchemaProperties(schemaPropsStr);
			}
			
			return new SchemaInfo 
			{ 
				Name = schemaName,
				Type = schemaType,
				Schema = schemaData,
				Properties = schemaProps
			};
		}

		private KeyValueSchemaInfo()
		{
		}
	}
}
