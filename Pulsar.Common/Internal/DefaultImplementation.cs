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
namespace Pulsar.client.@internal
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.apache.pulsar.client.@internal.ReflectionUtils.catchExceptions;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.apache.pulsar.client.@internal.ReflectionUtils.getConstructor;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.apache.pulsar.client.@internal.ReflectionUtils.getStaticMethod;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.apache.pulsar.client.@internal.ReflectionUtils.newClassInstance;

	using UtilityClass = lombok.experimental.UtilityClass;
	using Authentication = Client.Api.Authentication;
	using BatcherBuilder = org.apache.pulsar.client.api.BatcherBuilder;
	using ClientBuilder = org.apache.pulsar.client.api.ClientBuilder;
	using MessageId = org.apache.pulsar.client.api.MessageId;
	using UnsupportedAuthenticationException = org.apache.pulsar.client.api.PulsarClientException.UnsupportedAuthenticationException;
	using org.apache.pulsar.client.api;
	using GenericRecord = org.apache.pulsar.client.api.schema.GenericRecord;
	using org.apache.pulsar.client.api.schema;
	using RecordSchemaBuilder = org.apache.pulsar.client.api.schema.RecordSchemaBuilder;
	using org.apache.pulsar.client.api.schema;
	using org.apache.pulsar.client.api.schema;
	using org.apache.pulsar.common.schema;
	using KeyValueEncodingType = org.apache.pulsar.common.schema.KeyValueEncodingType;
	using SchemaInfo = org.apache.pulsar.common.schema.SchemaInfo;
	using SchemaInfoWithVersion = org.apache.pulsar.common.schema.SchemaInfoWithVersion;
	using SchemaType = org.apache.pulsar.common.schema.SchemaType;

	/// <summary>
	/// Helper class for class instantiations and it also contains methods to work with schemas.
	/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") @UtilityClass public class DefaultImplementation
	public class DefaultImplementation
	{

		private static readonly Type CLIENT_BUILDER_IMPL = newClassInstance("org.apache.pulsar.client.impl.ClientBuilderImpl");

		private static readonly System.Reflection.ConstructorInfo<MessageId> MESSAGE_ID_IMPL_long_long_int = getConstructor("org.apache.pulsar.client.impl.MessageIdImpl", Long.TYPE, Long.TYPE, Integer.TYPE);

		private static readonly System.Reflection.MethodInfo MESSAGE_ID_IMPL_fromByteArray = getStaticMethod("org.apache.pulsar.client.impl.MessageIdImpl", "fromByteArray", typeof(sbyte[]));
		private static readonly System.Reflection.MethodInfo MESSAGE_ID_IMPL_fromByteArrayWithTopic = getStaticMethod("org.apache.pulsar.client.impl.MessageIdImpl", "fromByteArrayWithTopic", typeof(sbyte[]), typeof(string));

		private static readonly System.Reflection.ConstructorInfo<Authentication> AUTHENTICATION_TOKEN_String = getConstructor("org.apache.pulsar.client.impl.auth.AuthenticationToken", typeof(string));

		private static readonly System.Reflection.ConstructorInfo<Authentication> AUTHENTICATION_TOKEN_Supplier = getConstructor("org.apache.pulsar.client.impl.auth.AuthenticationToken", typeof(System.Func));

		private static readonly System.Reflection.ConstructorInfo<Authentication> AUTHENTICATION_TLS_String_String = getConstructor("org.apache.pulsar.client.impl.auth.AuthenticationTls", typeof(string), typeof(string));

		private static readonly System.Reflection.ConstructorInfo<SchemaDefinitionBuilder> SCHEMA_DEFINITION_BUILDER_CONSTRUCTOR = getConstructor("org.apache.pulsar.client.impl.schema.SchemaDefinitionBuilderImpl");

		public static SchemaDefinitionBuilder<T> newSchemaDefinitionBuilder<T>()
		{
			return catchExceptions(() => (SchemaDefinitionBuilder<T>) SCHEMA_DEFINITION_BUILDER_CONSTRUCTOR.newInstance());
		}

		public static ClientBuilder newClientBuilder()
		{
			return catchExceptions(() => System.Activator.CreateInstance(CLIENT_BUILDER_IMPL));
		}

		public static MessageId newMessageId(long ledgerId, long entryId, int partitionIndex)
		{
			return catchExceptions(() => MESSAGE_ID_IMPL_long_long_int.newInstance(ledgerId, entryId, partitionIndex));
		}

		public static MessageId newMessageIdFromByteArray(sbyte[] data)
		{
			return catchExceptions(() => (MessageId) MESSAGE_ID_IMPL_fromByteArray.invoke(null, data));
		}

		public static MessageId newMessageIdFromByteArrayWithTopic(sbyte[] data, string topicName)
		{
			return catchExceptions(() => (MessageId) MESSAGE_ID_IMPL_fromByteArrayWithTopic.invoke(null, data, topicName));
		}

		public static Authentication newAuthenticationToken(string token)
		{
			return catchExceptions(() => (Authentication) AUTHENTICATION_TOKEN_String.newInstance(token));
		}

		public static Authentication newAuthenticationToken(System.Func<string> supplier)
		{
			return catchExceptions(() => (Authentication) AUTHENTICATION_TOKEN_Supplier.newInstance(supplier));
		}

		public static Authentication newAuthenticationTLS(string certFilePath, string keyFilePath)
		{
			return catchExceptions(() => (Authentication) AUTHENTICATION_TLS_String_String.newInstance(certFilePath, keyFilePath));
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static org.apache.pulsar.client.api.Authentication createAuthentication(String authPluginClassName, String authParamsString) throws org.apache.pulsar.client.api.PulsarClientException.UnsupportedAuthenticationException
		public static Authentication createAuthentication(string authPluginClassName, string authParamsString)
		{
			return catchExceptions(() => (Authentication) getStaticMethod("org.apache.pulsar.client.impl.AuthenticationUtil", "create", typeof(string), typeof(string)).invoke(null, authPluginClassName, authParamsString));
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static org.apache.pulsar.client.api.Authentication createAuthentication(String authPluginClassName, java.util.Map<String, String> authParams) throws org.apache.pulsar.client.api.PulsarClientException.UnsupportedAuthenticationException
		public static Authentication createAuthentication(string authPluginClassName, IDictionary<string, string> authParams)
		{
			return catchExceptions(() => (Authentication) getStaticMethod("org.apache.pulsar.client.impl.AuthenticationUtil", "create", typeof(string), typeof(System.Collections.IDictionary)).invoke(null, authPluginClassName, authParams));
		}

		public static Schema<sbyte[]> newBytesSchema()
		{
			return catchExceptions(() => (Schema<sbyte[]>) newClassInstance("org.apache.pulsar.client.impl.schema.BytesSchema").newInstance());
		}

		public static Schema<string> newStringSchema()
		{
			return catchExceptions(() => (Schema<string>) newClassInstance("org.apache.pulsar.client.impl.schema.StringSchema").newInstance());
		}

		public static Schema<string> newStringSchema(Charset charset)
		{
			return catchExceptions(() => (Schema<string>) getConstructor("org.apache.pulsar.client.impl.schema.StringSchema", typeof(Charset)).newInstance(charset));
		}

		public static Schema<sbyte> newByteSchema()
		{
			return catchExceptions(() => (Schema<sbyte>) newClassInstance("org.apache.pulsar.client.impl.schema.ByteSchema").newInstance());
		}

		public static Schema<short> newShortSchema()
		{
			return catchExceptions(() => (Schema<short>) newClassInstance("org.apache.pulsar.client.impl.schema.ShortSchema").newInstance());
		}

		public static Schema<int> newIntSchema()
		{
			return catchExceptions(() => (Schema<int>) newClassInstance("org.apache.pulsar.client.impl.schema.IntSchema").newInstance());
		}

		public static Schema<long> newLongSchema()
		{
			return catchExceptions(() => (Schema<long>) newClassInstance("org.apache.pulsar.client.impl.schema.LongSchema").newInstance());
		}

		public static Schema<bool> newBooleanSchema()
		{
			return catchExceptions(() => (Schema<bool>) newClassInstance("org.apache.pulsar.client.impl.schema.BooleanSchema").newInstance());
		}

		public static Schema<ByteBuffer> newByteBufferSchema()
		{
			return catchExceptions(() => (Schema<ByteBuffer>) newClassInstance("org.apache.pulsar.client.impl.schema.ByteBufferSchema").newInstance());
		}

		public static Schema<float> newFloatSchema()
		{
			return catchExceptions(() => (Schema<float>) newClassInstance("org.apache.pulsar.client.impl.schema.FloatSchema").newInstance());
		}

		public static Schema<double> newDoubleSchema()
		{
			return catchExceptions(() => (Schema<double>) newClassInstance("org.apache.pulsar.client.impl.schema.DoubleSchema").newInstance());
		}

		public static Schema<DateTime> newDateSchema()
		{
			return catchExceptions(() => (Schema<DateTime>) getStaticMethod("org.apache.pulsar.client.impl.schema.DateSchema", "of", null).invoke(null, null));
		}

		public static Schema<Time> newTimeSchema()
		{
			return catchExceptions(() => (Schema<Time>) getStaticMethod("org.apache.pulsar.client.impl.schema.TimeSchema", "of", null).invoke(null, null));
		}

		public static Schema<Timestamp> newTimestampSchema()
		{
			return catchExceptions(() => (Schema<Timestamp>) getStaticMethod("org.apache.pulsar.client.impl.schema.TimestampSchema", "of", null).invoke(null, null));
		}

		public static Schema<T> newAvroSchema<T>(SchemaDefinition schemaDefinition)
		{
			return catchExceptions(() => (Schema<T>) getStaticMethod("org.apache.pulsar.client.impl.schema.AvroSchema", "of", typeof(SchemaDefinition)).invoke(null, schemaDefinition));
		}

		public static Schema<T> newProtobufSchema<T>(SchemaDefinition schemaDefinition) where T : com.google.protobuf.GeneratedMessageV3
		{
			return catchExceptions(() => (Schema<T>) getStaticMethod("org.apache.pulsar.client.impl.schema.ProtobufSchema", "of", typeof(SchemaDefinition)).invoke(null, schemaDefinition));
		}

		public static Schema<T> newJSONSchema<T>(SchemaDefinition schemaDefinition)
		{
			return catchExceptions(() => (Schema<T>) getStaticMethod("org.apache.pulsar.client.impl.schema.JSONSchema", "of", typeof(SchemaDefinition)).invoke(null, schemaDefinition));
		}

		public static Schema<GenericRecord> newAutoConsumeSchema()
		{
			return catchExceptions(() => (Schema<GenericRecord>) newClassInstance("org.apache.pulsar.client.impl.schema.AutoConsumeSchema").newInstance());
		}

		public static Schema<sbyte[]> newAutoProduceSchema()
		{
			return catchExceptions(() => (Schema<sbyte[]>) newClassInstance("org.apache.pulsar.client.impl.schema.AutoProduceBytesSchema").newInstance());
		}

		public static Schema<sbyte[]> newAutoProduceSchema<T1>(Schema<T1> schema)
		{
			return catchExceptions(() => (Schema<sbyte[]>) getConstructor("org.apache.pulsar.client.impl.schema.AutoProduceBytesSchema", typeof(Schema)).newInstance(schema));
		}

		public static Schema<KeyValue<sbyte[], sbyte[]>> newKeyValueBytesSchema()
		{
			return catchExceptions(() => (Schema<KeyValue<sbyte[], sbyte[]>>) getStaticMethod("org.apache.pulsar.client.impl.schema.KeyValueSchema", "kvBytes").invoke(null));
		}

		public static Schema<KeyValue<K, V>> newKeyValueSchema<K, V>(Schema<K> keySchema, Schema<V> valueSchema)
		{
			return catchExceptions(() => (Schema<KeyValue<K, V>>) getStaticMethod("org.apache.pulsar.client.impl.schema.KeyValueSchema", "of", typeof(Schema), typeof(Schema)).invoke(null, keySchema, valueSchema));
		}

		public static Schema<KeyValue<K, V>> newKeyValueSchema<K, V>(Schema<K> keySchema, Schema<V> valueSchema, KeyValueEncodingType keyValueEncodingType)
		{
			return catchExceptions(() => (Schema<KeyValue<K, V>>) getStaticMethod("org.apache.pulsar.client.impl.schema.KeyValueSchema", "of", typeof(Schema), typeof(Schema), typeof(KeyValueEncodingType)).invoke(null, keySchema, valueSchema, keyValueEncodingType));
		}

		public static Schema<KeyValue<K, V>> newKeyValueSchema<K, V>(Type key, Type value, SchemaType type)
		{
			return catchExceptions(() => (Schema<KeyValue<K, V>>) getStaticMethod("org.apache.pulsar.client.impl.schema.KeyValueSchema", "of", typeof(Type), typeof(Type), typeof(SchemaType)).invoke(null, key, value, type));
		}

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: public static org.apache.pulsar.client.api.Schema<?> getSchema(org.apache.pulsar.common.schema.SchemaInfo schemaInfo)
		public static Schema<object> getSchema(SchemaInfo schemaInfo)
		{
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: return catchExceptions(() -> (org.apache.pulsar.client.api.Schema<?>) getStaticMethod("org.apache.pulsar.client.impl.schema.AutoConsumeSchema", "getSchema", org.apache.pulsar.common.schema.SchemaInfo.class).invoke(null, schemaInfo));
			return catchExceptions(() => (Schema<object>) getStaticMethod("org.apache.pulsar.client.impl.schema.AutoConsumeSchema", "getSchema", typeof(SchemaInfo)).invoke(null, schemaInfo));
		}

		public static GenericSchema<GenericRecord> getGenericSchema(SchemaInfo schemaInfo)
		{
			return catchExceptions(() => (GenericSchema) getStaticMethod("org.apache.pulsar.client.impl.schema.generic.GenericSchemaImpl", "of", typeof(SchemaInfo)).invoke(null, schemaInfo));
		}

		public static RecordSchemaBuilder newRecordSchemaBuilder(string name)
		{
			return catchExceptions(() => (RecordSchemaBuilder) getConstructor("org.apache.pulsar.client.impl.schema.RecordSchemaBuilderImpl", typeof(string)).newInstance(name));
		}

		/// <summary>
		/// Decode the kv encoding type from the schema info.
		/// </summary>
		/// <param name="schemaInfo"> the schema info </param>
		/// <returns> the kv encoding type </returns>
		public static KeyValueEncodingType decodeKeyValueEncodingType(SchemaInfo schemaInfo)
		{
			return catchExceptions(() => (KeyValueEncodingType) getStaticMethod("org.apache.pulsar.client.impl.schema.KeyValueSchemaInfo", "decodeKeyValueEncodingType", typeof(SchemaInfo)).invoke(null, schemaInfo));
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
			return catchExceptions(() => (SchemaInfo) getStaticMethod("org.apache.pulsar.client.impl.schema.KeyValueSchemaInfo", "encodeKeyValueSchemaInfo", typeof(string), typeof(Schema), typeof(Schema), typeof(KeyValueEncodingType)).invoke(null, schemaName, keySchema, valueSchema, keyValueEncodingType));
		}

		/// <summary>
		/// Decode the key/value schema info to get key schema info and value schema info.
		/// </summary>
		/// <param name="schemaInfo"> key/value schema info. </param>
		/// <returns> the pair of key schema info and value schema info </returns>
		public static KeyValue<SchemaInfo, SchemaInfo> decodeKeyValueSchemaInfo(SchemaInfo schemaInfo)
		{
			return catchExceptions(() => (KeyValue<SchemaInfo, SchemaInfo>) getStaticMethod("org.apache.pulsar.client.impl.schema.KeyValueSchemaInfo", "decodeKeyValueSchemaInfo", typeof(SchemaInfo)).invoke(null, schemaInfo));
		}

		/// <summary>
		/// Jsonify the schema info.
		/// </summary>
		/// <param name="schemaInfo"> the schema info </param>
		/// <returns> the jsonified schema info </returns>
		public static string jsonifySchemaInfo(SchemaInfo schemaInfo)
		{
			return catchExceptions(() => (string) getStaticMethod("org.apache.pulsar.client.impl.schema.SchemaUtils", "jsonifySchemaInfo", typeof(SchemaInfo)).invoke(null, schemaInfo));
		}

		/// <summary>
		/// Jsonify the schema info with version.
		/// </summary>
		/// <param name="schemaInfoWithVersion"> the schema info with version </param>
		/// <returns> the jsonified schema info with version </returns>
		public static string jsonifySchemaInfoWithVersion(SchemaInfoWithVersion schemaInfoWithVersion)
		{
			return catchExceptions(() => (string) getStaticMethod("org.apache.pulsar.client.impl.schema.SchemaUtils", "jsonifySchemaInfoWithVersion", typeof(SchemaInfoWithVersion)).invoke(null, schemaInfoWithVersion));
		}

		/// <summary>
		/// Jsonify the key/value schema info.
		/// </summary>
		/// <param name="kvSchemaInfo"> the key/value schema info </param>
		/// <returns> the jsonified schema info </returns>
		public static string jsonifyKeyValueSchemaInfo(KeyValue<SchemaInfo, SchemaInfo> kvSchemaInfo)
		{
			return catchExceptions(() => (string) getStaticMethod("org.apache.pulsar.client.impl.schema.SchemaUtils", "jsonifyKeyValueSchemaInfo", typeof(KeyValue)).invoke(null, kvSchemaInfo));
		}

		/// <summary>
		/// Convert the key/value schema data.
		/// </summary>
		/// <param name="kvSchemaInfo"> the key/value schema info </param>
		/// <returns> the convert key/value schema data string </returns>
		public static string convertKeyValueSchemaInfoDataToString(KeyValue<SchemaInfo, SchemaInfo> kvSchemaInfo)
		{
			return catchExceptions(() => (string) getStaticMethod("org.apache.pulsar.client.impl.schema.SchemaUtils", "convertKeyValueSchemaInfoDataToString", typeof(KeyValue)).invoke(null, kvSchemaInfo));
		}

		/// <summary>
		/// Convert the key/value schema info data json bytes to key/value schema info data bytes.
		/// </summary>
		/// <param name="keyValueSchemaInfoDataJsonBytes"> the key/value schema info data json bytes </param>
		/// <returns> the key/value schema info data bytes </returns>
		public static sbyte[] convertKeyValueDataStringToSchemaInfoSchema(sbyte[] keyValueSchemaInfoDataJsonBytes)
		{
			return catchExceptions(() => (sbyte[]) getStaticMethod("org.apache.pulsar.client.impl.schema.SchemaUtils", "convertKeyValueDataStringToSchemaInfoSchema", typeof(sbyte[])).invoke(null, keyValueSchemaInfoDataJsonBytes));
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