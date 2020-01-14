using System;

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
namespace Pulsar.Api
{
	using DefaultImplementation = org.apache.pulsar.client.@internal.DefaultImplementation;
	using org.apache.pulsar.common.schema;
	using KeyValueEncodingType = org.apache.pulsar.common.schema.KeyValueEncodingType;
	using SchemaInfo = org.apache.pulsar.common.schema.SchemaInfo;
	using SchemaType = org.apache.pulsar.common.schema.SchemaType;

	/// <summary>
	/// Message schema definition.
	/// </summary>
	public interface Schema<T>
	{

		/// <summary>
		/// Check if the message is a valid object for this schema.
		/// 
		/// <para>The implementation can choose what its most efficient approach to validate the schema.
		/// If the implementation doesn't provide it, it will attempt to use <seealso cref="decode(sbyte[])"/>
		/// to see if this schema can decode this message or not as a validation mechanism to verify
		/// the bytes.
		/// 
		/// </para>
		/// </summary>
		/// <param name="message"> the messages to verify </param>
		/// <exception cref="SchemaSerializationException"> if it is not a valid message </exception>
//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods unless the C#8 option for this is selected:
//		default void validate(byte[] message)
	//	{
	//		decode(message);
	//	}

		/// <summary>
		/// Encode an object representing the message content into a byte array.
		/// </summary>
		/// <param name="message">
		///            the message object </param>
		/// <returns> a byte array with the serialized content </returns>
		/// <exception cref="SchemaSerializationException">
		///             if the serialization fails </exception>
		sbyte[] Encode(T message);

		/// <summary>
		/// Returns whether this schema supports versioning.
		/// 
		/// <para>Most of the schema implementations don't really support schema versioning, or it just doesn't
		/// make any sense to support schema versionings (e.g. primitive schemas). Only schema returns
		/// <seealso cref="GenericRecord"/> should support schema versioning.
		/// 
		/// </para>
		/// <para>If a schema implementation returns <tt>false</tt>, it should implement <seealso cref="decode(sbyte[])"/>;
		/// while a schema implementation returns <tt>true</tt>, it should implement <seealso cref="decode(sbyte[], sbyte[])"/>
		/// instead.
		/// 
		/// </para>
		/// </summary>
		/// <returns> true if this schema implementation supports schema versioning; otherwise returns false. </returns>
//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods unless the C#8 option for this is selected:
//		default boolean supportSchemaVersioning()
	//	{
	//		return false;
	//	}

//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods unless the C#8 option for this is selected:
//		default void setSchemaInfoProvider(org.apache.pulsar.client.api.schema.SchemaInfoProvider schemaInfoProvider)
	//	{
	//	}

		/// <summary>
		/// Decode a byte array into an object using the schema definition and deserializer implementation.
		/// </summary>
		/// <param name="bytes">
		///            the byte array to decode </param>
		/// <returns> the deserialized object </returns>
//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods unless the C#8 option for this is selected:
//		default T decode(byte[] bytes)
	//	{
	//		// use `null` to indicate ignoring schema version
	//		return decode(bytes, null);
	//	}

		/// <summary>
		/// Decode a byte array into an object using a given version.
		/// </summary>
		/// <param name="bytes">
		///            the byte array to decode </param>
		/// <param name="schemaVersion">
		///            the schema version to decode the object. null indicates using latest version. </param>
		/// <returns> the deserialized object </returns>
//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods unless the C#8 option for this is selected:
//		default T decode(byte[] bytes, byte[] schemaVersion)
	//	{
	//		// ignore version by default (most of the primitive schema implementations ignore schema version)
	//		return decode(bytes);
	//	}

		/// <returns> an object that represents the Schema associated metadata </returns>
		SchemaInfo SchemaInfo {get;}

		/// <summary>
		/// Check if this schema requires fetching schema info to configure the schema.
		/// </summary>
		/// <returns> true if the schema requires fetching schema info to configure the schema,
		///         otherwise false. </returns>
//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods unless the C#8 option for this is selected:
//		default boolean requireFetchingSchemaInfo()
	//	{
	//		return false;
	//	}

		/// <summary>
		/// Configure the schema to use the provided schema info.
		/// </summary>
		/// <param name="topic"> topic name </param>
		/// <param name="componentName"> component name </param>
		/// <param name="schemaInfo"> schema info </param>
//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods unless the C#8 option for this is selected:
//		default void configureSchemaInfo(String topic, String componentName, org.apache.pulsar.common.schema.SchemaInfo schemaInfo)
	//	{
	//		// no-op
	//	}

		/// <summary>
		/// Schema that doesn't perform any encoding on the message payloads. Accepts a byte array and it passes it through.
		/// </summary>

		/// <summary>
		/// ByteBuffer Schema.
		/// </summary>

		/// <summary>
		/// Schema that can be used to encode/decode messages whose values are String. The payload is encoded with UTF-8.
		/// </summary>

		/// <summary>
		/// INT8 Schema.
		/// </summary>

		/// <summary>
		/// INT16 Schema.
		/// </summary>

		/// <summary>
		/// INT32 Schema.
		/// </summary>

		/// <summary>
		/// INT64 Schema.
		/// </summary>

		/// <summary>
		/// Boolean Schema.
		/// </summary>

		/// <summary>
		/// Float Schema.
		/// </summary>

		/// <summary>
		/// Double Schema.
		/// </summary>

		/// <summary>
		/// Date Schema.
		/// </summary>

		/// <summary>
		/// Time Schema.
		/// </summary>

		/// <summary>
		/// Timestamp Schema.
		/// </summary>

		// CHECKSTYLE.OFF: MethodName

		/// <summary>
		/// Create a Protobuf schema type by extracting the fields of the specified class.
		/// </summary>
		/// <param name="clazz"> the Protobuf generated class to be used to extract the schema </param>
		/// <returns> a Schema instance </returns>
//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java static interface methods unless the C#8 option for this is selected:
//		static <T> Schema<T> PROTOBUF(Class<T> clazz)
	//	{
	//		return DefaultImplementation.newProtobufSchema(SchemaDefinition.builder().withPojo(clazz).build());
	//	}

		/// <summary>
		/// Create a Protobuf schema type with schema definition.
		/// </summary>
		/// <param name="schemaDefinition"> schemaDefinition the definition of the schema </param>
		/// <returns> a Schema instance </returns>
//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java static interface methods unless the C#8 option for this is selected:
//		static <T> Schema<T> PROTOBUF(org.apache.pulsar.client.api.schema.SchemaDefinition<T> schemaDefinition)
	//	{
	//		return DefaultImplementation.newProtobufSchema(schemaDefinition);
	//	}

		/// <summary>
		/// Create a  Avro schema type by default configuration of the class.
		/// </summary>
		/// <param name="pojo"> the POJO class to be used to extract the Avro schema </param>
		/// <returns> a Schema instance </returns>
//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java static interface methods unless the C#8 option for this is selected:
//		static <T> Schema<T> AVRO(Class<T> pojo)
	//	{
	//		return DefaultImplementation.newAvroSchema(SchemaDefinition.builder().withPojo(pojo).build());
	//	}

		/// <summary>
		/// Create a Avro schema type with schema definition.
		/// </summary>
		/// <param name="schemaDefinition"> the definition of the schema </param>
		/// <returns> a Schema instance </returns>
//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java static interface methods unless the C#8 option for this is selected:
//		static <T> Schema<T> AVRO(org.apache.pulsar.client.api.schema.SchemaDefinition<T> schemaDefinition)
	//	{
	//		return DefaultImplementation.newAvroSchema(schemaDefinition);
	//	}

		/// <summary>
		/// Create a JSON schema type by extracting the fields of the specified class.
		/// </summary>
		/// <param name="pojo"> the POJO class to be used to extract the JSON schema </param>
		/// <returns> a Schema instance </returns>
//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java static interface methods unless the C#8 option for this is selected:
//		static <T> Schema<T> JSON(Class<T> pojo)
	//	{
	//		return DefaultImplementation.newJSONSchema(SchemaDefinition.builder().withPojo(pojo).build());
	//	}

		/// <summary>
		/// Create a JSON schema type with schema definition.
		/// </summary>
		/// <param name="schemaDefinition"> the definition of the schema </param>
		/// <returns> a Schema instance </returns>
//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java static interface methods unless the C#8 option for this is selected:
//		static <T> Schema<T> JSON(org.apache.pulsar.client.api.schema.SchemaDefinition schemaDefinition)
	//	{
	//		return DefaultImplementation.newJSONSchema(schemaDefinition);
	//	}

		/// <summary>
		/// Key Value Schema using passed in schema type, support JSON and AVRO currently.
		/// </summary>
//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java static interface methods unless the C#8 option for this is selected:
//		static <K, V> Schema<org.apache.pulsar.common.schema.KeyValue<K, V>> KeyValue(Class<K> key, Class<V> value, org.apache.pulsar.common.schema.SchemaType type)
	//	{
	//		return DefaultImplementation.newKeyValueSchema(key, value, type);
	//	}

		/// <summary>
		/// Schema that can be used to encode/decode KeyValue.
		/// </summary>
//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java static interface methods unless the C#8 option for this is selected:
//		static Schema<org.apache.pulsar.common.schema.KeyValue<byte[], byte[]>> KV_BYTES()
	//	{
	//		return DefaultImplementation.newKeyValueBytesSchema();
	//	}

		/// <summary>
		/// Key Value Schema whose underneath key and value schemas are JSONSchema.
		/// </summary>
//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java static interface methods unless the C#8 option for this is selected:
//		static <K, V> Schema<org.apache.pulsar.common.schema.KeyValue<K, V>> KeyValue(Class<K> key, Class<V> value)
	//	{
	//		return DefaultImplementation.newKeyValueSchema(key, value, SchemaType.JSON);
	//	}

		/// <summary>
		/// Key Value Schema using passed in key and value schemas.
		/// </summary>
//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java static interface methods unless the C#8 option for this is selected:
//		static <K, V> Schema<org.apache.pulsar.common.schema.KeyValue<K, V>> KeyValue(Schema<K> key, Schema<V> value)
	//	{
	//		return DefaultImplementation.newKeyValueSchema(key, value);
	//	}

		/// <summary>
		/// Key Value Schema using passed in key, value and encoding type schemas.
		/// </summary>
//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java static interface methods unless the C#8 option for this is selected:
//		static <K, V> Schema<org.apache.pulsar.common.schema.KeyValue<K, V>> KeyValue(Schema<K> key, Schema<V> value, org.apache.pulsar.common.schema.KeyValueEncodingType keyValueEncodingType)
	//	{
	//		return DefaultImplementation.newKeyValueSchema(key, value, keyValueEncodingType);
	//	}

//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java static interface methods unless the C#8 option for this is selected:
//[Obsolete]
//		static Schema<org.apache.pulsar.client.api.schema.GenericRecord> AUTO()
	//	{
	//		return AUTO_CONSUME();
	//	}

		/// <summary>
		/// Create a schema instance that automatically deserialize messages
		/// based on the current topic schema.
		/// 
		/// <para>The messages values are deserialized into a <seealso cref="GenericRecord"/> object.
		/// 
		/// </para>
		/// <para>Currently this is only supported with Avro and JSON schema types.
		/// 
		/// </para>
		/// </summary>
		/// <returns> the auto schema instance </returns>
//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java static interface methods unless the C#8 option for this is selected:
//		static Schema<org.apache.pulsar.client.api.schema.GenericRecord> AUTO_CONSUME()
	//	{
	//		return DefaultImplementation.newAutoConsumeSchema();
	//	}

		/// <summary>
		/// Create a schema instance that accepts a serialized payload
		/// and validates it against the topic schema.
		/// 
		/// <para>Currently this is only supported with Avro and JSON schema types.
		/// 
		/// </para>
		/// <para>This method can be used when publishing a raw JSON payload,
		/// for which the format is known and a POJO class is not available.
		/// 
		/// </para>
		/// </summary>
		/// <returns> the auto schema instance </returns>
//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java static interface methods unless the C#8 option for this is selected:
//		static Schema<byte[]> AUTO_PRODUCE_BYTES()
	//	{
	//		return DefaultImplementation.newAutoProduceSchema();
	//	}

		/// <summary>
		/// Create a schema instance that accepts a serialized payload
		/// and validates it against the schema specified.
		/// </summary>
		/// <returns> the auto schema instance
		/// @since 2.5.0 </returns>
		/// <seealso cref= #AUTO_PRODUCE_BYTES() </seealso>
//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java static interface methods unless the C#8 option for this is selected:
//		static Schema<byte[]> AUTO_PRODUCE_BYTES(Schema<JavaToDotNetGenericWildcard> schema)
	//	{
	//		return DefaultImplementation.newAutoProduceSchema(schema);
	//	}

		// CHECKSTYLE.ON: MethodName

//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java static interface methods unless the C#8 option for this is selected:
//		static Schema<JavaToDotNetGenericWildcard> getSchema(org.apache.pulsar.common.schema.SchemaInfo schemaInfo)
	//	{
	//		return DefaultImplementation.getSchema(schemaInfo);
	//	}

		/// <summary>
		/// Returns a generic schema of existing schema info.
		/// 
		/// <para>Only supports AVRO and JSON.
		/// 
		/// </para>
		/// </summary>
		/// <param name="schemaInfo"> schema info </param>
		/// <returns> a generic schema instance </returns>
//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java static interface methods unless the C#8 option for this is selected:
//		static org.apache.pulsar.client.api.schema.GenericSchema<org.apache.pulsar.client.api.schema.GenericRecord> generic(org.apache.pulsar.common.schema.SchemaInfo schemaInfo)
	//	{
	//		return DefaultImplementation.getGenericSchema(schemaInfo);
	//	}
	}

	public static class Schema_Fields
	{
		public static readonly Schema<sbyte[]> BYTES = DefaultImplementation.newBytesSchema();
		public static readonly Schema<ByteBuffer> BYTEBUFFER = DefaultImplementation.newByteBufferSchema();
		public static readonly Schema<string> STRING = DefaultImplementation.newStringSchema();
		public static readonly Schema<sbyte> INT8 = DefaultImplementation.newByteSchema();
		public static readonly Schema<short> INT16 = DefaultImplementation.newShortSchema();
		public static readonly Schema<int> INT32 = DefaultImplementation.newIntSchema();
		public static readonly Schema<long> INT64 = DefaultImplementation.newLongSchema();
		public static readonly Schema<bool> BOOL = DefaultImplementation.newBooleanSchema();
		public static readonly Schema<float> FLOAT = DefaultImplementation.newFloatSchema();
		public static readonly Schema<double> DOUBLE = DefaultImplementation.newDoubleSchema();
		public static readonly Schema<DateTime> DATE = DefaultImplementation.newDateSchema();
		public static readonly Schema<Time> TIME = DefaultImplementation.newTimeSchema();
		public static readonly Schema<Timestamp> TIMESTAMP = DefaultImplementation.newTimestampSchema();
	}

}