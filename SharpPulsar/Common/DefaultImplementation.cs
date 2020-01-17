using Pulsar.Api.Schema;
using SharpPulsar.Impl;
using SharpPulsar.Impl.Auth;
using SharpPulsar.Impl.Message;
using SharpPulsar.Impl.Schema;
using SharpPulsar.Interface;
using SharpPulsar.Interface.Auth;
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

		private static readonly System.Reflection.ConstructorInfo<MessageId> MESSAGE_ID_IMPL_long_long_int = getConstructor("org.apache.pulsar.client.impl.MessageIdImpl", Long.TYPE, Long.TYPE, Integer.TYPE);

		private static readonly System.Reflection.MethodInfo MESSAGE_ID_IMPL_fromByteArray = getStaticMethod("org.apache.pulsar.client.impl.MessageIdImpl", "fromByteArray", typeof(sbyte[]));
		private static readonly System.Reflection.MethodInfo MESSAGE_ID_IMPL_fromByteArrayWithTopic = getStaticMethod("org.apache.pulsar.client.impl.MessageIdImpl", "fromByteArrayWithTopic", typeof(sbyte[]), typeof(string));

		private static readonly System.Reflection.ConstructorInfo<Authentication> AUTHENTICATION_TOKEN_String = getConstructor("org.apache.pulsar.client.impl.auth.AuthenticationToken", typeof(string));

		private static readonly System.Reflection.ConstructorInfo<Authentication> AUTHENTICATION_TOKEN_Supplier = getConstructor("org.apache.pulsar.client.impl.auth.AuthenticationToken", typeof(System.Func));

		private static readonly System.Reflection.ConstructorInfo<Authentication> AUTHENTICATION_TLS_String_String = getConstructor("org.apache.pulsar.client.impl.auth.AuthenticationTls", typeof(string), typeof(string));

		

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

		public static ISchema<T> NewProtobufSchema<T>(ISchemaDefinition schemaDefinition) where T : com.google.protobuf.GeneratedMessageV3
		{
			return catchExceptions(() => (Schema<T>) getStaticMethod("SharpPulsar.Impl.Schema.ProtobufSchema", "of", typeof(SchemaDefinition)).invoke(null, schemaDefinition));
		}

		public static Schema<T> newJSONSchema<T>(SchemaDefinition schemaDefinition)
		{
			return catchExceptions(() => (Schema<T>) getStaticMethod("SharpPulsar.Impl.Schema.JSONSchema", "of", typeof(SchemaDefinition)).invoke(null, schemaDefinition));
		}

		public static Schema<GenericRecord> newAutoConsumeSchema()
		{
			return catchExceptions(() => (Schema<GenericRecord>) newClassInstance("SharpPulsar.Impl.Schema.AutoConsumeSchema").newInstance());
		}

		public static Schema<sbyte[]> newAutoProduceSchema()
		{
			return catchExceptions(() => (Schema<sbyte[]>) newClassInstance("SharpPulsar.Impl.Schema.AutoProduceBytesSchema").newInstance());
		}

		public static Schema<sbyte[]> newAutoProduceSchema<T1>(Schema<T1> schema)
		{
			return catchExceptions(() => (Schema<sbyte[]>) getConstructor("SharpPulsar.Impl.Schema.AutoProduceBytesSchema", typeof(Schema)).newInstance(schema));
		}

		public static Schema<KeyValue<sbyte[], sbyte[]>> newKeyValueBytesSchema()
		{
			return catchExceptions(() => (Schema<KeyValue<sbyte[], sbyte[]>>) getStaticMethod("SharpPulsar.Impl.Schema.KeyValueSchema", "kvBytes").invoke(null));
		}

		public static Schema<KeyValue<K, V>> newKeyValueSchema<K, V>(Schema<K> keySchema, Schema<V> valueSchema)
		{
			return catchExceptions(() => (Schema<KeyValue<K, V>>) getStaticMethod("SharpPulsar.Impl.Schema.KeyValueSchema", "of", typeof(Schema), typeof(Schema)).invoke(null, keySchema, valueSchema));
		}

		public static Schema<KeyValue<K, V>> newKeyValueSchema<K, V>(Schema<K> keySchema, Schema<V> valueSchema, KeyValueEncodingType keyValueEncodingType)
		{
			return catchExceptions(() => (Schema<KeyValue<K, V>>) getStaticMethod("SharpPulsar.Impl.Schema.KeyValueSchema", "of", typeof(Schema), typeof(Schema), typeof(KeyValueEncodingType)).invoke(null, keySchema, valueSchema, keyValueEncodingType));
		}

		public static Schema<KeyValue<K, V>> newKeyValueSchema<K, V>(Type key, Type value, SchemaType type)
		{
			return catchExceptions(() => (Schema<KeyValue<K, V>>) getStaticMethod("SharpPulsar.Impl.Schema.KeyValueSchema", "of", typeof(Type), typeof(Type), typeof(SchemaType)).invoke(null, key, value, type));
		}

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: public static org.apache.pulsar.client.api.Schema<?> getSchema(org.apache.pulsar.common.schema.SchemaInfo schemaInfo)
		public static Schema<object> getSchema(SchemaInfo schemaInfo)
		{
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: return catchExceptions(() -> (org.apache.pulsar.client.api.Schema<?>) getStaticMethod("SharpPulsar.Impl.Schema.AutoConsumeSchema", "getSchema", org.apache.pulsar.common.schema.SchemaInfo.class).invoke(null, schemaInfo));
			return catchExceptions(() => (Schema<object>) getStaticMethod("SharpPulsar.Impl.Schema.AutoConsumeSchema", "getSchema", typeof(SchemaInfo)).invoke(null, schemaInfo));
		}

		public static GenericSchema<GenericRecord> getGenericSchema(SchemaInfo schemaInfo)
		{
			return catchExceptions(() => (GenericSchema) getStaticMethod("SharpPulsar.Impl.Schema.generic.GenericSchemaImpl", "of", typeof(SchemaInfo)).invoke(null, schemaInfo));
		}

		public static RecordSchemaBuilder newRecordSchemaBuilder(string name)
		{
			return catchExceptions(() => (RecordSchemaBuilder) getConstructor("SharpPulsar.Impl.Schema.RecordSchemaBuilderImpl", typeof(string)).newInstance(name));
		}

		/// <summary>
		/// Decode the kv encoding type from the schema info.
		/// </summary>
		/// <param name="schemaInfo"> the schema info </param>
		/// <returns> the kv encoding type </returns>
		public static KeyValueEncodingType decodeKeyValueEncodingType(SchemaInfo schemaInfo)
		{
			return catchExceptions(() => (KeyValueEncodingType) getStaticMethod("SharpPulsar.Impl.Schema.KeyValueSchemaInfo", "decodeKeyValueEncodingType", typeof(SchemaInfo)).invoke(null, schemaInfo));
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
			return catchExceptions(() => (SchemaInfo) getStaticMethod("SharpPulsar.Impl.Schema.KeyValueSchemaInfo", "encodeKeyValueSchemaInfo", typeof(string), typeof(Schema), typeof(Schema), typeof(KeyValueEncodingType)).invoke(null, schemaName, keySchema, valueSchema, keyValueEncodingType));
		}

		/// <summary>
		/// Decode the key/value schema info to get key schema info and value schema info.
		/// </summary>
		/// <param name="schemaInfo"> key/value schema info. </param>
		/// <returns> the pair of key schema info and value schema info </returns>
		public static KeyValue<SchemaInfo, SchemaInfo> decodeKeyValueSchemaInfo(SchemaInfo schemaInfo)
		{
			return catchExceptions(() => (KeyValue<SchemaInfo, SchemaInfo>) getStaticMethod("SharpPulsar.Impl.Schema.KeyValueSchemaInfo", "decodeKeyValueSchemaInfo", typeof(SchemaInfo)).invoke(null, schemaInfo));
		}

		/// <summary>
		/// Jsonify the schema info.
		/// </summary>
		/// <param name="schemaInfo"> the schema info </param>
		/// <returns> the jsonified schema info </returns>
		public static string jsonifySchemaInfo(SchemaInfo schemaInfo)
		{
			return catchExceptions(() => (string) getStaticMethod("SharpPulsar.Impl.Schema.SchemaUtils", "jsonifySchemaInfo", typeof(SchemaInfo)).invoke(null, schemaInfo));
		}

		/// <summary>
		/// Jsonify the schema info with version.
		/// </summary>
		/// <param name="schemaInfoWithVersion"> the schema info with version </param>
		/// <returns> the jsonified schema info with version </returns>
		public static string jsonifySchemaInfoWithVersion(SchemaInfoWithVersion schemaInfoWithVersion)
		{
			return catchExceptions(() => (string) getStaticMethod("SharpPulsar.Impl.Schema.SchemaUtils", "jsonifySchemaInfoWithVersion", typeof(SchemaInfoWithVersion)).invoke(null, schemaInfoWithVersion));
		}

		/// <summary>
		/// Jsonify the key/value schema info.
		/// </summary>
		/// <param name="kvSchemaInfo"> the key/value schema info </param>
		/// <returns> the jsonified schema info </returns>
		public static string jsonifyKeyValueSchemaInfo(KeyValue<SchemaInfo, SchemaInfo> kvSchemaInfo)
		{
			return catchExceptions(() => (string) getStaticMethod("SharpPulsar.Impl.Schema.SchemaUtils", "jsonifyKeyValueSchemaInfo", typeof(KeyValue)).invoke(null, kvSchemaInfo));
		}

		/// <summary>
		/// Convert the key/value schema data.
		/// </summary>
		/// <param name="kvSchemaInfo"> the key/value schema info </param>
		/// <returns> the convert key/value schema data string </returns>
		public static string convertKeyValueSchemaInfoDataToString(KeyValue<SchemaInfo, SchemaInfo> kvSchemaInfo)
		{
			return catchExceptions(() => (string) getStaticMethod("SharpPulsar.Impl.Schema.SchemaUtils", "convertKeyValueSchemaInfoDataToString", typeof(KeyValue)).invoke(null, kvSchemaInfo));
		}

		/// <summary>
		/// Convert the key/value schema info data json bytes to key/value schema info data bytes.
		/// </summary>
		/// <param name="keyValueSchemaInfoDataJsonBytes"> the key/value schema info data json bytes </param>
		/// <returns> the key/value schema info data bytes </returns>
		public static sbyte[] convertKeyValueDataStringToSchemaInfoSchema(sbyte[] keyValueSchemaInfoDataJsonBytes)
		{
			return catchExceptions(() => (sbyte[]) getStaticMethod("SharpPulsar.Impl.Schema.SchemaUtils", "convertKeyValueDataStringToSchemaInfoSchema", typeof(sbyte[])).invoke(null, keyValueSchemaInfoDataJsonBytes));
		}

		public static BatcherBuilder newDefaultBatcherBuilder()
		{
			return catchExceptions(() => (BatcherBuilder) getConstructor("org.apache.pulsar.client.impl.DefaultBatcherBuilder").newInstance());
		}

		public static BatcherBuilder newKeyBasedBatcherBuilder()
		{
			return catchExceptions(() => (BatcherBuilder) getConstructor("org.apache.pulsar.client.impl.KeyBasedBatcherBuilder").newInstance());
		}
	}

}