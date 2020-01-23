using SharpPulsar.Common.Schema;
using SharpPulsar.Impl;
using SharpPulsar.Impl.Auth;
using SharpPulsar.Impl.Batch;
using SharpPulsar.Impl.Message;
using SharpPulsar.Impl.Schema;
using SharpPulsar.Impl.Schema.Generic;
using SharpPulsar.Interface;
using SharpPulsar.Interface.Auth;
using SharpPulsar.Interface.Batch;
using SharpPulsar.Interface.Message;
using SharpPulsar.Interface.Schema;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

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
namespace SharpPulsar.Common
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

		public static IClientBuilder NewClientBuilder()
		{
			return new ClientBuilderImpl();
		}

		public static IMessageId NewMessageId(long ledgerId, long entryId, int partitionIndex)
		{
			return new MessageIdImpl(ledgerId, entryId, partitionIndex);
		}

		public static IMessageId NewMessageIdFromByteArray(sbyte[] data)
		{
			return MessageIdImpl.FromByteArray(data);
		}

		public static IMessageId NewMessageIdFromByteArrayWithTopic(sbyte[] data, string topicName)
		{
			return MessageIdImpl.FromByteArrayWithTopic(data, topicName);
		}

		public static IAuthentication NwAuthenticationToken(string token)
		{
			return new AuthenticationToken(token);
		}

		public static IAuthentication NewAuthenticationToken(Func<string> supplier)
		{
			return new AuthenticationToken(supplier);
		}

		public static IAuthentication NewAuthenticationTLS(string certFilePath, string keyFilePath)
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

		public static ISchema<string> NewStringSchema(CharSet charset)
		{
			return new StringSchema(charset);
		}

		public static ISchema<sbyte> NewByteSchema()
		{
			return new ByteSchema();
		}

		public static ISchema<short> NewShortSchema()
		{
			return new ShortSchema();
		}

		public static ISchema<int> NewIntSchema()
		{
			return new IntSchema();
		}

		public static ISchema<long> NewLongSchema()
		{
			return new LongSchema();
		}

		public static ISchema<bool> NewBooleanSchema()
		{
			return new BoolSchema();
		}

		public static ISchema<ByteBuffer> NewByteBufferSchema()
		{
			return new ByteBufferSchema();
		}

		public static ISchema<float> NewFloatSchema()
		{
			return new FloatSchema();
		}

		public static ISchema<double> NewDoubleSchema()
		{
			return new DoubleSchema();
		}

		public static ISchema<DateTime> NewDateSchema()
		{
			return DateSchema.Of();
		}

		/*public static ISchema<Time> NewTimeSchema()
		{
			return catchExceptions(() => (Schema<Time>) getStaticMethod("SharpPulsar.Impl.Schema.TimeSchema", "of", null).invoke(null, null));
		}

		public static ISchema<Timestamp> NewTimestampSchema()
		{
			return catchExceptions(() => (Schema<Timestamp>) getStaticMethod("SharpPulsar.Impl.Schema.TimestampSchema", "of", null).invoke(null, null));
		}*/

		public static ISchema<T> NewAvroSchema<T>(ISchemaDefinition<T> schemaDefinition)
		{
			return AvroSchema<T>.Of(schemaDefinition);
		}

		public static ISchema<T> NewProtobufSchema<T>(ISchemaDefinition<T> schemaDefinition) //where T : com.google.protobuf.GeneratedMessageV3
		{
			return ProtobufSchema<T>.Of<T>(schemaDefinition.GetType());
			//return catchExceptions(() => (Schema<T>) getStaticMethod("SharpPulsar.Impl.Schema.ProtobufSchema", "of", typeof(SchemaDefinition)).invoke(null, schemaDefinition));
		}

		public static ISchema<T> NewJSONSchema<T>(ISchemaDefinition<T> schemaDefinition)
		{
			return JSONSchema<T>.Of(schemaDefinition);
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
			return new AutoProduceBytesSchema<sbyte[]>(schema);
			//return catchExceptions(() => (Schema<sbyte[]>) getConstructor("SharpPulsar.Impl.Schema.AutoProduceBytesSchema", typeof(Schema)).newInstance(schema));
		}

		public static ISchema<KeyValue<sbyte[], sbyte[]>> NewKeyValueBytesSchema()
		{
			return KeyValueSchema<sbyte[], sbyte[]>.KvBytes();
			//return catchExceptions(() => (Schema<KeyValue<sbyte[], sbyte[]>>) getStaticMethod("SharpPulsar.Impl.Schema.KeyValueSchema", "kvBytes").invoke(null));
		}

		public static ISchema<KeyValue<K, V>> NewKeyValueSchema<K, V>(ISchema<K> keySchema, ISchema<V> valueSchema)
		{
			return KeyValueSchema<K, V>.Of(keySchema, valueSchema);
			//return catchExceptions(() => (Schema<KeyValue<K, V>>) getStaticMethod("SharpPulsar.Impl.Schema.KeyValueSchema", "of", typeof(Schema), typeof(Schema)).invoke(null, keySchema, valueSchema));
		}

		public static ISchema<KeyValue<K, V>> NewKeyValueSchema<K, V>(ISchema<K> keySchema, ISchema<V> valueSchema, KeyValueEncodingType keyValueEncodingType)
		{
			return KeyValueSchema<K, V>.Of(keySchema, valueSchema, keyValueEncodingType);
			//return catchExceptions(() => (Schema<KeyValue<K, V>>) getStaticMethod("SharpPulsar.Impl.Schema.KeyValueSchema", "of", typeof(Schema), typeof(Schema), typeof(KeyValueEncodingType)).invoke(null, keySchema, valueSchema, keyValueEncodingType));
		}

		public static ISchema<KeyValue<K, V>> NewKeyValueSchema<K, V>(Type key, Type value, SchemaType type)
		{
			return KeyValueSchema<K, V>.Of(key, value, type);
			//return catchExceptions(() => (Schema<KeyValue<K, V>>) getStaticMethod("SharpPulsar.Impl.Schema.KeyValueSchema", "of", typeof(Type), typeof(Type), typeof(SchemaType)).invoke(null, key, value, type));
		}
		public static ISchema<object> GetSchema(SchemaInfo schemaInfo)
		{
			return (ISchema<object>)AutoConsumeSchema.GetSchema(schemaInfo);
		}

		public static IGenericSchema<IGenericRecord> GetGenericSchema(SchemaInfo schemaInfo)
		{
			return GenericSchemaImpl.Of(schemaInfo);
		}

		public static IRecordSchemaBuilder NewRecordSchemaBuilder(string name)
		{
			return new RecordSchemaBuilderImpl(name);
		}

		/// <summary>
		/// Decode the kv encoding type from the schema info.
		/// </summary>
		/// <param name="schemaInfo"> the schema info </param>
		/// <returns> the kv encoding type </returns>
		public static KeyValueEncodingType DecodeKeyValueEncodingType(SchemaInfo schemaInfo)
		{
			return KeyValueSchemaInfo.DecodeKeyValueEncodingType(schemaInfo);
		}

		/// <summary>
		/// Encode key & value into schema into a KeyValue schema.
		/// </summary>
		/// <param name="keySchema"> the key schema </param>
		/// <param name="valueSchema"> the value schema </param>
		/// <param name="keyValueEncodingType"> the encoding type to encode and decode key value pair </param>
		/// <returns> the final schema info </returns>
		public static SchemaInfo EncodeKeyValueSchemaInfo<K, V>(ISchema<K> keySchema, ISchema<V> valueSchema, KeyValueEncodingType keyValueEncodingType)
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
		public static SchemaInfo EncodeKeyValueSchemaInfo<K, V>(string schemaName, ISchema<K> keySchema, ISchema<V> valueSchema, KeyValueEncodingType keyValueEncodingType)
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
		}

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

		public static IBatcherBuilder NewDefaultBatcherBuilder()
		{
			return new DefaultBatcherBuilder();
		}

		public static IBatcherBuilder NewKeyBasedBatcherBuilder()
		{
			return new KeyBasedBatcherBuilder();
		}
	}

}