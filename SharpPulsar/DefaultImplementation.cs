using SharpPulsar.Auth;
using SharpPulsar.Impl.Schema;
using SharpPulsar.Impl.Schema.Generic;
using System;
using System.Collections.Generic;
using Akka.Actor;
using SharpPulsar.Batch;
using SharpPulsar.Batch.Api;
using SharpPulsar.Interfaces.Interceptor;
using SharpPulsar.Schemas;
using SharpPulsar.Interfaces.ISchema;
using SharpPulsar.Interfaces;
using System.Text;
using NodaTime;
using SharpPulsar.Shared;
using SharpPulsar.Schema;
using SharpPulsar.Schemas.Generic;

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
namespace SharpPulsar
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


		public static IMessageId NewMessageId(long ledgerId, long entryId, int partitionIndex, int batch)
		{
			return new BatchMessageId(ledgerId, entryId, partitionIndex, batch);
		}

		public static IMessageId NewMessageIdFromByteArray(sbyte[] data)
		{
			return MessageId.FromByteArray(data);
		}

		public static IMessageId NewMessageIdFromByteArrayWithTopic(sbyte[] data, string topicName)
		{
			return MessageId.FromByteArrayWithTopic(data, topicName);
		}

		public static IAuthentication NewAuthenticationToken(string token)
		{
			return new AuthenticationToken(token);
		}
		public static IAuthentication NewAuthenticationSts(string client, string secret, string authority)
		{
			return new AuthenticationOAuth2(client, secret, authority);
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

		public static ISchema<T> NewAvroSchema<T>(ISchemaDefinition<T> schemaDefinition)
		{
			return AvroSchema<T>.Of(schemaDefinition);
		}

		public static ISchema<IGenericRecord> NewAutoConsumeSchema()
		{
			return new AutoConsumeSchema();
		}

		public static ISchema<byte[]> NewAutoProduceSchema()
		{
			return new AutoProduceBytesSchema<byte[]>();
		}
		public static ISchema<string> NewStringSchema()
		{
			return new StringSchema();
			//return catchExceptions(()-> (Schema<String>) newClassInstance("org.apache.pulsar.client.impl.schema.StringSchema")
							//.newInstance());
		}

		public static ISchema<string> NewStringSchema(Encoding encoding)
		{
			return new StringSchema(encoding);
			/*return catchExceptions(
					()-> (Schema<String>) getConstructor(
						"org.apache.pulsar.client.impl.schema.StringSchema", Charset.class)
                        .newInstance(charset));*/
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
			return new BooleanSchema();
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
			return new DateSchema();
		}
		public static ISchema<Instant> NewInstantSchema()
		{
			return new InstantSchema();
		}
		public static ISchema<LocalDate> NewLocalDateSchema()
		{
			return new LocalDateSchema();
		}
		public static ISchema<LocalTime> NewLocalTimeSchema()
		{
			return new LocalTimeSchema();
		}
		public static ISchema<LocalDateTime> NewLocalDateTimeSchema()
		{
			return new LocalDateTimeSchema();
		}
		public static ISchema<T> NewJsonSchema<T>(ISchemaDefinition<T> schemaDefinition)
		{
			return JSONSchema<T>.Of(schemaDefinition);
		}
		public static ISchema<byte[]> NewAutoProduceSchema<T>(ISchema<T> schema)
		{
			return new AutoProduceBytesSchema<T>(schema);
			//return catchExceptions(() => (Schema<sbyte[]>) getConstructor("SharpPulsar.Impl.Schema.AutoProduceBytesSchema", typeof(Schema)).newInstance(schema));
		}

		public static ISchema<object> GetSchema(ISchemaInfo schemaInfo)
		{
			return (ISchema<object>)AutoConsumeSchema.GetSchema((SchemaInfo)schemaInfo);
		}

		public static GenericSchema GetGenericSchema(ISchemaInfo schemaInfo)
		{
			return GenericSchema.Of((SchemaInfo)schemaInfo);
		}

		public static ISchema<KeyValue<K, V>> NewKeyValueSchema<K, V>(ISchema<K> keySchema, ISchema<V> valueSchema)
		{
			//return catchExceptions(() => (Schema<KeyValue<K, V>>)getStaticMethod("org.apache.pulsar.client.impl.schema.KeyValueSchema", "of", typeof(Schema), typeof(Schema)).invoke(null, KeySchema, ValueSchema));
			return KeyValueSchema<K, V>.Of(keySchema, valueSchema);
		}

		public static ISchema<KeyValue<K, V>> NewKeyValueSchema<K, V>(ISchema<K> keySchema, ISchema<V> valueSchema, KeyValueEncodingType keyValueEncodingType)
		{
			return KeyValueSchema<K, V>.Of(keySchema, valueSchema, keyValueEncodingType);
		}

		public static ISchema<KeyValue<K, V>> NewKeyValueSchema<K, V>(Type key, Type value, SchemaType type)
		{
			return KeyValueSchema<K, V>.Of(key, value, type);
		}
		public static ISchema<KeyValue<sbyte[], sbyte[]>> NewKeyValueBytesSchema()
		{
			return KeyValueSchema<sbyte[], sbyte[]>.KvBytes();
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
		public static string JsonifyKeyValueSchemaInfo(KeyValue<ISchemaInfo, ISchemaInfo> kvSchemaInfo)
		{
			return SchemaUtils.JsonifyKeyValueSchemaInfo(kvSchemaInfo);
		}

		/// <summary>
		/// Convert the key/value schema data.
		/// </summary>
		/// <param name="kvSchemaInfo"> the key/value schema info </param>
		/// <returns> the convert key/value schema data string </returns>
		public static string ConvertKeyValueSchemaInfoDataToString(KeyValue<ISchemaInfo, ISchemaInfo> kvSchemaInfo)
		{
			return SchemaUtils.ConvertKeyValueSchemaInfoDataToString(kvSchemaInfo);
		}

		public static IBatcherBuilder NewDefaultBatcherBuilder(ActorSystem system)
		{
			return new DefaultBatcherBuilder(system);
		}
		/// <summary>
		/// Decode the kv encoding type from the schema info.
		/// </summary>
		/// <param name="schemaInfo"> the schema info </param>
		/// <returns> the kv encoding type </returns>
		public static KeyValueEncodingType DecodeKeyValueEncodingType(ISchemaInfo schemaInfo)
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
		public static ISchemaInfo EncodeKeyValueSchemaInfo<K, V>(ISchema<K> keySchema, ISchema<V> valueSchema, KeyValueEncodingType keyValueEncodingType)
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
		public static ISchemaInfo EncodeKeyValueSchemaInfo<K, V>(string schemaName, ISchema<K> keySchema, ISchema<V> valueSchema, KeyValueEncodingType keyValueEncodingType)
		{
			return KeyValueSchemaInfo.EncodeKeyValueSchemaInfo(schemaName, keySchema, valueSchema, keyValueEncodingType);
			//return catchExceptions(() => (SchemaInfo)getStaticMethod("org.apache.pulsar.client.impl.schema.KeyValueSchemaInfo", "encodeKeyValueSchemaInfo", typeof(string), typeof(Schema), typeof(Schema), typeof(KeyValueEncodingType)).invoke(null, schemaName, keySchema, valueSchema, keyValueEncodingType));
		}

		/// <summary>
		/// Decode the key/value schema info to get key schema info and value schema info.
		/// </summary>
		/// <param name="schemaInfo"> key/value schema info. </param>
		/// <returns> the pair of key schema info and value schema info </returns>
		public static KeyValue<ISchemaInfo, ISchemaInfo> DecodeKeyValueSchemaInfo(ISchemaInfo schemaInfo)
		{
			return KeyValueSchemaInfo.DecodeKeyValueSchemaInfo(schemaInfo);
		}

		public static IBatcherBuilder NewKeyBasedBatcherBuilder(ActorSystem system)
        {
            return new KeyBasedBatcherBuilder(system);
        }
	}

}