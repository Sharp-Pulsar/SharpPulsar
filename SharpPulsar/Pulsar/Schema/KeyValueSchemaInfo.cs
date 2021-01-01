using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using SharpPulsar.Api;
using SharpPulsar.Api.Schema;
using SharpPulsar.Common.Enum;
using SharpPulsar.Common.Schema;
using SharpPulsar.Protocol.Builder;
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
	/// Util class for processing key/value schema info.
	/// </summary>
	public sealed class KeyValueSchemaInfo
	{

		private static readonly ISchema<ISchemaInfo> SchemaInfoWriter = new SchemaAnonymousInnerClass();

		public class SchemaAnonymousInnerClass : ISchema<ISchemaInfo>
		{
			public sbyte[] Encode(SchemaInfo si)
			{
				return si.Schema;
			}

            public sbyte[] Encode(ISchemaInfo message)
            {
                throw new NotImplementedException();
            }

            public ISchemaInfo SchemaInfo => BytesSchema.Of().SchemaInfo;
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
		public static KeyValueEncodingType? DecodeKeyValueEncodingType(SchemaInfo schemaInfo)
		{
			Precondition.Condition.CheckArgument(SchemaType.KeyValue == schemaInfo.Type, "Not a KeyValue schema");

			string encodingTypeStr = schemaInfo.Properties[KvEncodingType];
			if (string.IsNullOrWhiteSpace(encodingTypeStr))
			{
				return KeyValueEncodingType.Inline;
			}
			else
			{
				return Enum.GetValues(typeof(KeyValueEncodingType)).Cast<KeyValueEncodingType>().ToList().FirstOrDefault(x => x.ToString() == encodingTypeStr);
			}
		}

		/// <summary>
		/// Encode key & value into schema into a KeyValue schema.
		/// </summary>
		/// <param name="keySchema"> the key schema </param>
		/// <param name="valueSchema"> the value schema </param>
		/// <param name="keyValueEncodingType"> the encoding type to encode and decode key value pair </param>
		/// <returns> the final schema info </returns>
		public static SchemaInfo EncodeKeyValueSchemaInfo<TK, TV>(ISchema<TK> keySchema, ISchema<TV> valueSchema, KeyValueEncodingType keyValueEncodingType)
		{
			return EncodeKeyValueSchemaInfo("KeyValue", keySchema, valueSchema, keyValueEncodingType);
		}

		/// <summary>
		/// Encode key & value into schema into a KeyValue schema.
		/// </summary>
		/// <param name="schemaName"> the final schema name </param>
		/// <param name="keySchema"> the key schema </param>
		/// <param name="valueSchema"> the value schema </param>
		/// <param name="keyValueEncodingType"> the encoding type to encode and decode key value pair </param>
		/// <returns> the final schema info </returns>
		public static SchemaInfo EncodeKeyValueSchemaInfo<TK, TV>(string schemaName, ISchema<TK> keySchema, ISchema<TV> valueSchema, KeyValueEncodingType keyValueEncodingType)
		{
            if (keySchema.SchemaInfo == null || valueSchema.SchemaInfo == null)
            {
                // schema is not ready
                return null;
            }
			
            // process key/value schema data
            sbyte[] schemaData = KeyValue<TK, TV>.Encode(keySchema, SchemaInfoWriter, valueSchema.SchemaInfo.Schema, SchemaInfoWriter);

            // process key/value schema properties
            IDictionary<string, string> properties = new Dictionary<string, string>();
            EncodeSubSchemaInfoToParentSchemaProperties((SchemaInfo)keySchema.SchemaInfo, KeySchemaName, KeySchemaType, KeySchemaProps, properties);

            EncodeSubSchemaInfoToParentSchemaProperties((SchemaInfo)valueSchema.SchemaInfo, ValueSchemaName, ValueSchemaType, ValueSchemaProps, properties);
            properties[KvEncodingType] = keyValueEncodingType.ToString();
			SchemaInfoBuilder buildinfo = new SchemaInfoBuilder().SetName(schemaName).SetType(SchemaType.KeyValue).SetSchema(schemaData).SetProperties(properties);
            // generate the final schema info
            return buildinfo.Build();
        }
		
		private static void EncodeSubSchemaInfoToParentSchemaProperties(SchemaInfo schemaInfo, string schemaNameProperty, string schemaTypeProperty, string schemaPropsProperty, IDictionary<string, string> parentSchemaProperties)
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
		public static KeyValue<SchemaInfo, SchemaInfo> DecodeKeyValueSchemaInfo(SchemaInfo schemaInfo)
		{
			Precondition.Condition.CheckArgument(SchemaType.KeyValue == schemaInfo.Type, "Not a KeyValue schema");

			return KeyValue<SchemaInfo, SchemaInfo>.Decode(schemaInfo.Schema, (keyBytes, valueBytes) =>
			{
			SchemaInfo keySchemaInfo = DecodeSubSchemaInfo(schemaInfo, KeySchemaName, KeySchemaType, KeySchemaProps, keyBytes);
			SchemaInfo valueSchemaInfo = DecodeSubSchemaInfo(schemaInfo, ValueSchemaName, ValueSchemaType, ValueSchemaProps, valueBytes);
			return new KeyValue<SchemaInfo, SchemaInfo>(keySchemaInfo, valueSchemaInfo);
			});
		}

		private static SchemaInfo DecodeSubSchemaInfo(SchemaInfo parentSchemaInfo, string schemaNameProperty, string schemaTypeProperty, string schemaPropsProperty, sbyte[] schemaData)
		{
			IDictionary<string, string> parentSchemaProps = parentSchemaInfo.Properties;
			string schemaName = parentSchemaProps.ToList().FirstOrDefault(x => x.Key == schemaNameProperty).Value;
			SchemaType schemaType = schemaType.ValueOf(parentSchemaProps.getOrDefault(schemaTypeProperty, schemaType.BYTES.name()));
			IDictionary<string, string> schemaProps;
			string schemaPropsStr = parentSchemaProps[schemaPropsProperty];
			if (string.IsNullOrWhiteSpace(schemaPropsStr))
			{
				schemaProps = new Dictionary<string, string>();
			}
			else
			{
				schemaProps = SchemaUtils.DeserializeSchemaProperties(schemaPropsStr);
			}
			return new SchemaInfoBuilder().SetName(schemaName).SetType(schemaType).SetSchema(schemaData).SetProperties(schemaProps).Build();
		}

		private KeyValueSchemaInfo()
		{
		}
	}

}