
using System.Runtime.InteropServices;
using SharpPulsar.API.Schema;
using SharpPulsar.Shared;
using NodaTime;

/*
 * Licensed to the Apache Software Foundation (ASF) under one
 * or more contributor license agreements.  See the NOTICE file
 * distributed with this work for additional information
 * regarding copyright ownership.  The ASF licenses this file
 * to you under the Apache License, Version 2.0 (the
 * "License"); you may not use this file except in compliance
 * with the License.  You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing,
 * software distributed under the License is distributed on an
 * "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
 * KIND, either express or implied.  See the License for the
 * specific language governing permissions and limitations
 * under the License.
 */


namespace SharpPulsar.API.Internal
{
    /// <summary>
    /// Helper class for class instantiations and it also contains methods to work with schemas.
    /// This interface allows you to not depend on the Implementation classes directly.
    /// The actual implementation of this class is loaded from <seealso cref="DefaultImplementation"/>.
    /// </summary>
    public interface IPulsarClientImplementationBinding
    {
        ISchemaDefinitionBuilder<T> NewSchemaDefinitionBuilder<T>();

        IClientBuilder NewClientBuilder();

        IMessageId NewMessageId(long ledgerId, long entryId, int partitionIndex);

        IMessageId NewMessageIdFromByteArray(byte[] data);

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
        //ORIGINAL LINE: org.apache.pulsar.client.api.MessageId newMessageIdFromByteArrayWithTopic(byte[] data, String topicName) throws java.io.IOException;
        IMessageId NewMessageIdFromByteArrayWithTopic(byte[] data, string topicName);

        IAuthentication NewAuthenticationToken(string token);

        IAuthentication NewAuthenticationToken(System.Func<string> supplier);

        IAuthentication NewAuthenticationTLS(string certFilePath, string keyFilePath);

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
        //ORIGINAL LINE: org.apache.pulsar.client.api.Authentication createAuthentication(String authPluginClassName, String authParamsString) throws org.apache.pulsar.client.api.PulsarClientException.UnsupportedAuthenticationException;
        IAuthentication CreateAuthentication(string authPluginClassName, string authParamsString);

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
        //ORIGINAL LINE: org.apache.pulsar.client.api.Authentication createAuthentication(String authPluginClassName, java.util.Map<String, String> authParams) throws org.apache.pulsar.client.api.PulsarClientException.UnsupportedAuthenticationException;
        IAuthentication CreateAuthentication(string authPluginClassName, IDictionary<string, string> authParams);

        ISchema<byte[]> NewBytesSchema();

        ISchema<string> NewStringSchema();

        ISchema<string> NewStringSchema(CharSet charset);

        ISchema<byte> NewByteSchema();
        
        ISchema<short> NewShortSchema();

        ISchema<int> NewIntSchema();

        ISchema<long> NewLongSchema();

        ISchema<bool> NewBoolSchema();

        ISchema<ByteBuffer> NewByteBufferSchema();

        ISchema<float> NewFloatSchema();

        ISchema<double> NewDoubleSchema();

        ISchema<DateTime> NewDateSchema();

        ISchema<Time> NewTimeSchema();

        ISchema<TimeStamp> NewTimestampSchema();

        ISchema<Instant> NewInstantSchema();

        ISchema<LocalDate> NewLocalDateSchema();

        ISchema<LocalTime> NewLocalTimeSchema();

        ISchema<LocalDateTime> NewLocalDateTimeSchema();

        ISchema<T> NewAvroSchema<T>(ISchemaDefinition<T> schemaDefinition);

        ISchema<T> NewProtobufSchema<T>(ISchemaDefinition<T> schemaDefinition);

        ISchema<T> NewProtobufNativeSchema<T>(ISchemaDefinition<T> schemaDefinition);

        ISchema<T> NewJSONSchema<T>(ISchemaDefinition<T> schemaDefinition);

        ISchema<IGenericRecord> NewAutoConsumeSchema();

        ISchema<byte[]> NewAutoProduceSchema();

        //JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in C#:
        //ORIGINAL LINE: org.apache.pulsar.client.api.Schema<byte[]> newAutoProduceSchema(org.apache.pulsar.client.api.Schema<?> schema);
        ISchema<byte[]> NewAutoProduceSchema<T1>(ISchema<T1> schema);

        ISchema<byte[]> NewAutoProduceValidatedAvroSchema(object schema);

        ISchema<IKeyValue<byte[], byte[]>> NewKeyValueBytesSchema();

        ISchema<IKeyValue<K, V>> NewKeyValueSchema<K, V>(ISchema<K> keySchema, ISchema<V> valueSchema, KeyValueEncodingType keyValueEncodingType);

        ISchema<IKeyValue<K, V>> NewKeyValueSchema<K, V>(Type key, Type value, SchemaType type);

        //JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in C#:
        //ORIGINAL LINE: org.apache.pulsar.client.api.Schema<?> getSchema(org.apache.pulsar.common.schema.SchemaInfo schemaInfo);
        ISchema<object> GetSchema(ISchemaInfo schemaInfo);

        IGenericSchema<IGenericRecord> GetGenericSchema(ISchemaInfo schemaInfo);

        IRecordSchemaBuilder NewRecordSchemaBuilder(string name);

        /// <summary>
        /// Decode the kv encoding type from the schema info.
        /// </summary>
        /// <param name="schemaInfo"> the schema info </param>
        /// <returns> the kv encoding type </returns>
        KeyValueEncodingType DecodeKeyValueEncodingType(ISchemaInfo schemaInfo);

        /// <summary>
        /// Encode key & value into schema into a KeyValue schema.
        /// </summary>
        /// <param name="keySchema">            the key schema </param>
        /// <param name="valueSchema">          the value schema </param>
        /// <param name="keyValueEncodingType"> the encoding type to encode and decode key value pair </param>
        /// <returns> the final schema info </returns>
        ISchemaInfo EncodeKeyValueSchemaInfo<K, V>(ISchema<K> keySchema, ISchema<V> valueSchema, KeyValueEncodingType keyValueEncodingType);

        /// <summary>
        /// Encode key & value into schema into a KeyValue schema.
        /// </summary>
        /// <param name="schemaName">           the final schema name </param>
        /// <param name="keySchema">            the key schema </param>
        /// <param name="valueSchema">          the value schema </param>
        /// <param name="keyValueEncodingType"> the encoding type to encode and decode key value pair </param>
        /// <returns> the final schema info </returns>
        ISchemaInfo EncodeKeyValueSchemaInfo<K, V>(string schemaName, ISchema<K> keySchema, ISchema<V> valueSchema, KeyValueEncodingType keyValueEncodingType);

        /// <summary>
        /// Decode the key/value schema info to get key schema info and value schema info.
        /// </summary>
        /// <param name="schemaInfo"> key/value schema info. </param>
        /// <returns> the pair of key schema info and value schema info </returns>
        KeyValuePair<ISchemaInfo, ISchemaInfo> DecodeKeyValueSchemaInfo(ISchemaInfo schemaInfo);

        /// <summary>
        /// Jsonify the schema info.
        /// </summary>
        /// <param name="schemaInfo"> the schema info </param>
        /// <returns> the jsonified schema info </returns>
        string JsonifySchemaInfo(ISchemaInfo schemaInfo);

        /// <summary>
        /// Jsonify the schema info with version.
        /// </summary>
        /// <param name="schemaInfoWithVersion"> the schema info with version </param>
        /// <returns> the jsonified schema info with version </returns>
        string JsonifySchemaInfoWithVersion(ISchemaInfoWithVersion schemaInfoWithVersion);

        /// <summary>
        /// Jsonify the key/value schema info.
        /// </summary>
        /// <param name="kvSchemaInfo"> the key/value schema info </param>
        /// <returns> the jsonified schema info </returns>
        string JsonifyKeyValueSchemaInfo(KeyValuePair<ISchemaInfo, ISchemaInfo> kvSchemaInfo);

        /// <summary>
        /// Convert the key/value schema data.
        /// </summary>
        /// <param name="kvSchemaInfo"> the key/value schema info </param>
        /// <returns> the convert key/value schema data string </returns>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
        //ORIGINAL LINE: String convertKeyValueSchemaInfoDataToString(org.apache.pulsar.common.schema.KeyValue<org.apache.pulsar.common.schema.SchemaInfo, org.apache.pulsar.common.schema.SchemaInfo> kvSchemaInfo) throws java.io.IOException;
        string ConvertKeyValueSchemaInfoDataToString(KeyValuePair<ISchemaInfo, ISchemaInfo> kvSchemaInfo);

        /// <summary>
        /// Convert the key/value schema info data json bytes to key/value schema info data bytes.
        /// </summary>
        /// <param name="keyValueSchemaInfoDataJsonBytes"> the key/value schema info data json bytes </param>
        /// <returns> the key/value schema info data bytes </returns>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
        //ORIGINAL LINE: byte[] convertKeyValueDataStringToSchemaInfoSchema(byte[] keyValueSchemaInfoDataJsonBytes) throws java.io.IOException;
        byte[] ConvertKeyValueDataStringToSchemaInfoSchema(byte[] keyValueSchemaInfoDataJsonBytes);

        IBatcherBuilder NewDefaultBatcherBuilder();

        IBatcherBuilder NewKeyBasedBatcherBuilder();

        IMessagePayloadFactory NewDefaultMessagePayloadFactory();

        /// <summary>
        /// Retrieves ByteBuffer data into byte[].
        /// </summary>
        /// <param name="byteBuffer">
        /// @return </param>
        static byte[] GetBytes(ByteBuffer byteBuffer)
        {
            if (byteBuffer == null)
            {
                return null;
            }
            if (byteBuffer.HasArray() && byteBuffer.ArrayOffset() == 0 && byteBuffer.ToArray().Length == byteBuffer.Remaining())
            {
                return byteBuffer.ToArray();
            }
            // Direct buffer is not backed by array and it needs to be read from direct memory
            byte[] array = new byte[byteBuffer.Remaining()];
            byteBuffer.Get(array);
            return array;
        }

        ISchemaInfo NewSchemaInfoImpl(string name, byte[] schema, SchemaType type, long timestamp, IDictionary<string, string> propertiesValue);

        ITopicMessageId NewTopicMessageId(string topic, IMessageId messageId);
    }
}
