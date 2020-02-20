using SharpPulsar.Api;
using SharpPulsar.Api.Schema;
using SharpPulsar.Common.Schema;
using SharpPulsar.Impl.Auth;
using SharpPulsar.Impl.Schema;
using SharpPulsar.Impl.Schema.Generic;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using SharpPulsar.Common.Enum;
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
namespace SharpPulsar.Impl
{
	/// <summary>
	/// Helper class for class instantiations and it also contains methods to work with schemas.
	/// </summary>
	public class DefaultImplementation
	{

		//private static readonly Type CLIENT_BUILDER_IMPL = new ClientBuilderImpl();

		public static ISchemaDefinitionBuilder<T> NewSchemaDefinitionBuilder<T>()
		{
			return new SchemaDefinitionBuilderImpl<T>();
		}

		public static IPulsarClientBuilder NewClientBuilder()
		{
			return new PulsarClientBuilderImpl();
		}

		public static IMessageId NewMessageId(long ledgerId, long entryId, int partitionIndex)
		{
			return new MessageIdImpl(ledgerId, entryId, partitionIndex);
		}
        /*public static ISchema<KeyValue<TK, TV>> NewKeyValueSchema<TK, TV>(ISchema<TK> keySchema, ISchema<TV> valueSchema)
        {
            var k = KeyValueSchema<TK, TV>.Of(keySchema, valueSchema);
            return k;
        }
        public static ISchema<KeyValue<sbyte[], sbyte[]>> NewKeyValueBytesSchema()
        {
            return KeyValueSchema<sbyte[], sbyte[]>.KvBytes();
        }

        
        public static ISchema<KeyValue<TK, TV>> NewKeyValueSchema<TK, TV>(ISchema<TK> keySchema, ISchema<TV> valueSchema, KeyValueEncodingType keyValueEncodingType)
        {
            return KeyValueSchema<TK, TV>.Of(keySchema, valueSchema, keyValueEncodingType);
        }

        public static ISchema<KeyValue<TK, TV>> NewKeyValueSchema<TK, TV>(TK key, TV value, SchemaType type)
        {
            return KeyValueSchema<TK, TV>.Of(key, value, type);
        }*/
		public static IMessageId NewMessageIdFromByteArray(sbyte[] data)
		{
			return MessageIdImpl.FromByteArray(data);
		}

		public static IMessageId NewMessageIdFromByteArrayWithTopic(sbyte[] data, string topicName)
		{
			return MessageIdImpl.FromByteArrayWithTopic(data, topicName);
		}

		public static IAuthentication NewAuthenticationToken(string token)
		{
			return new AuthenticationToken(token);
		}

		public static IAuthentication NewAuthenticationToken(Func<string> supplier)
		{
			return new AuthenticationToken(supplier);
		}

		public static IAuthentication NewAuthenticationTls(string certFilePath, string keyFilePath)
		{
			return new AuthenticationTls(certFilePath, keyFilePath);
		}

		public static IAuthentication CreateAuthentication(string authPluginClassName, string authParamsString)
		{
			return AuthenticationUtil.Create(authPluginClassName, authParamsString);
		}

		public static IAuthentication CreateAuthentication(string authPluginClassName, IDictionary<string, string> authParams)
		{
			return AuthenticationUtil.Create(authPluginClassName, authParams);
		}

		public static ISchema<sbyte[]> NewBytesSchema()
		{
			return new BytesSchema();
		}

		public static ISchema<string> NewStringSchema()
		{
			return new StringSchema();
		}

		public static ISchema<string> NewStringSchema(string charset)
		{
			return new StringSchema(charset);
		}
		
		public static ISchema<T> NewJsonSchema<T>(ISchemaDefinition<T> schemaDefinition)
		{
			return JsonSchema<T>.Of(schemaDefinition);
		}

		public static ISchema<IGenericRecord> NewAutoConsumeSchema()
		{
			return new AutoConsumeSchema();
		}

		public static ISchema<sbyte[]> NewAutoProduceSchema()
		{
			return new AutoProduceBytesSchema<sbyte[]>();
		}

		public static ISchema<sbyte[]> NewAutoProduceSchema<T1>(ISchema<T1> schema)
		{
			return new AutoProduceBytesSchema<T1>(schema);
			//return catchExceptions(() => (Schema<sbyte[]>) getConstructor("SharpPulsar.Impl.Schema.AutoProduceBytesSchema", typeof(Schema)).newInstance(schema));
		}

		public static ISchema<T> GetSchema<T>(ISchemaInfo schemaInfo)
		{
			return AutoConsumeSchema.GetSchema<T>((SchemaInfo)schemaInfo);
		}

		public static IGenericSchema<IGenericRecord> GetGenericSchema(ISchemaInfo schemaInfo)
		{
			return GenericSchemaImpl.Of((SchemaInfo)schemaInfo);
		}


		/// <summary>
		/// Decode the kv encoding type from the schema info.
		/// </summary>
		/// <param name="schemaInfo"> the schema info </param>
		/// <returns> the kv encoding type </returns>
		/*public static KeyValueEncodingType DecodeKeyValueEncodingType(SchemaInfo schemaInfo)
		{
			return KeyValueSchemaInfo.DecodeKeyValueEncodingType(schemaInfo);
		}*/

		/// <summary>
		/// Encode key & value into schema into a KeyValue schema.
		/// </summary>
		/// <param name="keySchema"> the key schema </param>
		/// <param name="valueSchema"> the value schema </param>
		/// <param name="keyValueEncodingType"> the encoding type to encode and decode key value pair </param>
		/// <returns> the final schema info </returns>
		/*public static SchemaInfo EncodeKeyValueSchemaInfo<TK, TV>(ISchema<TK> keySchema, ISchema<TV> valueSchema, KeyValueEncodingType keyValueEncodingType)
		{
			return EncodeKeyValueSchemaInfo("KeyValue", keySchema, valueSchema, keyValueEncodingType);
		}
		*/
		/// <summary>
		/// Encode key & value into schema into a KeyValue schema.
		/// </summary>
		/// <param name="schemaName"> the final schema name </param>
		/// <param name="keySchema"> the key schema </param>
		/// <param name="valueSchema"> the value schema </param>
		/// <param name="keyValueEncodingType"> the encoding type to encode and decode key value pair </param>
		/// <returns> the final schema info </returns>
		/*public static SchemaInfo EncodeKeyValueSchemaInfo<TK, TV>(string schemaName, ISchema<TK> keySchema, ISchema<TV> valueSchema, KeyValueEncodingType keyValueEncodingType)
		{
			return KeyValueSchemaInfo.EncodeKeyValueSchemaInfo(schemaName, keySchema, valueSchema, keyValueEncodingType);
		}

		/// <summary>
		/// Decode the key/value schema info to get key schema info and value schema info.
		/// </summary>
		/// <param name="schemaInfo"> key/value schema info. </param>
		/// <returns> the pair of key schema info and value schema info </returns>
		public static KeyValue<SchemaInfo, SchemaInfo> DecodeKeyValueSchemaInfo(SchemaInfo schemaInfo)
		{
			return KeyValueSchemaInfo.DecodeKeyValueSchemaInfo(schemaInfo);
		}*/

		/// <summary>
		/// Jsonify the schema info.
		/// </summary>
		/// <param name="schemaInfo"> the schema info </param>
		/// <returns> the jsonified schema info </returns>
		public static string JsonifySchemaInfo(SchemaInfo schemaInfo)
		{
			return SchemaUtils.JsonifySchemaInfo(schemaInfo);
		}

		/// <summary>
		/// Jsonify the schema info with version.
		/// </summary>
		/// <param name="schemaInfoWithVersion"> the schema info with version </param>
		/// <returns> the jsonified schema info with version </returns>
		public static string JsonifySchemaInfoWithVersion(SchemaInfoWithVersion schemaInfoWithVersion)
		{
			return SchemaUtils.JsonifySchemaInfoWithVersion(schemaInfoWithVersion);
		}

		/// <summary>
		/// Jsonify the key/value schema info.
		/// </summary>
		/// <param name="kvSchemaInfo"> the key/value schema info </param>
		/// <returns> the jsonified schema info </returns>
		public static string JsonifyKeyValueSchemaInfo(KeyValue<SchemaInfo, SchemaInfo> kvSchemaInfo)
		{
			return SchemaUtils.JsonifyKeyValueSchemaInfo(kvSchemaInfo);
		}

		/// <summary>
		/// Convert the key/value schema data.
		/// </summary>
		/// <param name="kvSchemaInfo"> the key/value schema info </param>
		/// <returns> the convert key/value schema data string </returns>
		public static string ConvertKeyValueSchemaInfoDataToString(KeyValue<SchemaInfo, SchemaInfo> kvSchemaInfo)
		{
			return SchemaUtils.ConvertKeyValueSchemaInfoDataToString(kvSchemaInfo);
		}

		/// <summary>
		/// Convert the key/value schema info data json bytes to key/value schema info data bytes.
		/// </summary>
		/// <param name="keyValueSchemaInfoDataJsonBytes"> the key/value schema info data json bytes </param>
		/// <returns> the key/value schema info data bytes </returns>
		public static sbyte[] ConvertKeyValueDataStringToSchemaInfoSchema(sbyte[] keyValueSchemaInfoDataJsonBytes)
		{
			return SchemaUtils.ConvertKeyValueDataStringToSchemaInfoSchema(keyValueSchemaInfoDataJsonBytes);
		}

		public static BatcherBuilder NewDefaultBatcherBuilder()
		{
			return new DefaultBatcherBuilder();
		}

		public static BatcherBuilder NewKeyBasedBatcherBuilder()
		{
			return new KeyBasedBatcherBuilder();
		}
	}

}