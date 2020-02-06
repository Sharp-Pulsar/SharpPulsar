using SharpPulsar.Api.Schema;
using SharpPulsar.Shared;
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
namespace SharpPulsar.Api
{
	/// <summary>
	/// Message schema definition.
	/// </summary>
	public interface ISchema<T>
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
		virtual void Validate(sbyte[] message)
		{
			Decode(message);
		}

		/// <summary>
		/// Encode an object representing the message content into a byte array.
		/// </summary>
		/// <param name="message">
		///            the message object </param>
		/// <returns> a byte array with the serialized content </returns>
		/// <exception cref="SchemaSerializationException">
		///             if the serialization fails </exception>
		sbyte[] Encode(T Message);

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
		virtual bool SupportSchemaVersioning()
		{
			return false;
		}

		virtual SchemaInfoProvider SchemaInfoProvider
		{
			set
			{
			}
		}

		/// <summary>
		/// Decode a byte array into an object using the schema definition and deserializer implementation.
		/// </summary>
		/// <param name="bytes">
		///            the byte array to decode </param>
		/// <returns> the deserialized object </returns>
		virtual T Decode(sbyte[] bytes)
		{
			// use `null` to indicate ignoring schema version
			return Decode(bytes, null);
		}

		/// <summary>
		/// Decode a byte array into an object using a given version.
		/// </summary>
		/// <param name="bytes">
		///            the byte array to decode </param>
		/// <param name="schemaVersion">
		///            the schema version to decode the object. null indicates using latest version. </param>
		/// <returns> the deserialized object </returns>
		virtual T Decode(sbyte[] bytes, sbyte[] SchemaVersion)
		{
			// ignore version by default (most of the primitive schema implementations ignore schema version)
			return Decode(bytes);
		}

		/// <returns> an object that represents the Schema associated metadata </returns>
		ISchemaInfo SchemaInfo {get;}

		/// <summary>
		/// Check if this schema requires fetching schema info to configure the schema.
		/// </summary>
		/// <returns> true if the schema requires fetching schema info to configure the schema,
		///         otherwise false. </returns>
		virtual bool RequireFetchingSchemaInfo()
		{
			return false;
		}

		/// <summary>
		/// Configure the schema to use the provided schema info.
		/// </summary>
		/// <param name="topic"> topic name </param>
		/// <param name="componentName"> component name </param>
		/// <param name="schemaInfo"> schema info </param>
		virtual void ConfigureSchemaInfo(string Topic, string ComponentName, ISchemaInfo SchemaInfo)
		{
			// no-op
		}

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
		static ISchema<T> PROTOBUF(Type Clazz) where T : com.google.protobuf.GeneratedMessageV3
		{
			return DefaultImplementation.newProtobufSchema(SchemaDefinition.builder().withPojo(clazz).build());
		}

		/// <summary>
		/// Create a Protobuf schema type with schema definition.
		/// </summary>
		/// <param name="schemaDefinition"> schemaDefinition the definition of the schema </param>
		/// <returns> a Schema instance </returns>
		static ISchema<T> PROTOBUF<T>(SchemaDefinition<T> SchemaDefinition) where T : com.google.protobuf.GeneratedMessageV3
		{
			return DefaultImplementation.newProtobufSchema(schemaDefinition);
		}

		/// <summary>
		/// Create a  Avro schema type by default configuration of the class.
		/// </summary>
		/// <param name="pojo"> the POJO class to be used to extract the Avro schema </param>
		/// <returns> a Schema instance </returns>
		static ISchema<T> AVRO(Type Pojo)
		{
			return DefaultImplementation.newAvroSchema(SchemaDefinition.builder().withPojo(pojo).build());
		}

		/// <summary>
		/// Create a Avro schema type with schema definition.
		/// </summary>
		/// <param name="schemaDefinition"> the definition of the schema </param>
		/// <returns> a Schema instance </returns>
		static ISchema<T> AVRO(SchemaDefinition<T> SchemaDefinition)
		{
			return DefaultImplementation.newAvroSchema(schemaDefinition);
		}

		/// <summary>
		/// Create a JSON schema type by extracting the fields of the specified class.
		/// </summary>
		/// <param name="pojo"> the POJO class to be used to extract the JSON schema </param>
		/// <returns> a Schema instance </returns>
		static ISchema<T> JSON(Type Pojo)
		{
			return DefaultImplementation.newJSONSchema(SchemaDefinition.builder().withPojo(pojo).build());
		}

		/// <summary>
		/// Create a JSON schema type with schema definition.
		/// </summary>
		/// <param name="schemaDefinition"> the definition of the schema </param>
		/// <returns> a Schema instance </returns>
		static ISchema<T> JSON<T>(SchemaDefinition SchemaDefinition)
		{
			return DefaultImplementation.newJSONSchema(schemaDefinition);
		}

		/// <summary>
		/// Key Value Schema using passed in schema type, support JSON and AVRO currently.
		/// </summary>
		static ISchema<KeyValue<K, V>> KeyValue<K, V>(Type Key, Type Value, SchemaType Type)
		{
			return DefaultImplementation.newKeyValueSchema(key, value, type);
		}

		/// <summary>
		/// Schema that can be used to encode/decode KeyValue.
		/// </summary>
		static ISchema<KeyValue<sbyte[], sbyte[]>> KvBytes()
		{
			return DefaultImplementation.newKeyValueBytesSchema();
		}

		/// <summary>
		/// Key Value Schema whose underneath key and value schemas are JSONSchema.
		/// </summary>
		static ISchema<KeyValue<K, V>> KeyValue<K, V>(Type Key, Type Value)
		{
			return DefaultImplementation.newKeyValueSchema(key, value, SchemaType.JSON);
		}

		/// <summary>
		/// Key Value Schema using passed in key and value schemas.
		/// </summary>
		static ISchema<KeyValue<K, V>> KeyValue<K, V>(ISchema<K> Key, ISchema<V> Value)
		{
			return DefaultImplementation.newKeyValueSchema(key, value);
		}

		/// <summary>
		/// Key Value Schema using passed in key, value and encoding type schemas.
		/// </summary>
		static ISchema<KeyValue<K, V>> KeyValue<K, V>(ISchema<K> Key, ISchema<V> Value, KeyValueEncodingType KeyValueEncodingType)
		{
			return DefaultImplementation.newKeyValueSchema(key, value, keyValueEncodingType);
		}

		[Obsolete]
		static ISchema<GenericRecord> AUTO()
		{
			return AUTO_CONSUME();
		}

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
		static ISchema<GenericRecord> AutoConsume()
		{
			return DefaultImplementation.newAutoConsumeSchema();
		}

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
		static ISchema<sbyte[]> AutoProduceBytes()
		{
			return DefaultImplementation.newAutoProduceSchema();
		}

		/// <summary>
		/// Create a schema instance that accepts a serialized payload
		/// and validates it against the schema specified.
		/// </summary>
		/// <returns> the auto schema instance
		/// @since 2.5.0 </returns>
		/// <seealso cref= #AUTO_PRODUCE_BYTES() </seealso>
		static ISchema<sbyte[]> AutoProduceBytes<T1>(ISchema<T1> Schema)
		{
			return DefaultImplementation.newAutoProduceSchema(schema);
		}

		static ISchema<T> GetSchema(ISchemaInfo SchemaInfo)
		{
			return DefaultImplementation.getSchema(schemaInfo);
		}

		/// <summary>
		/// Returns a generic schema of existing schema info.
		/// 
		/// <para>Only supports AVRO and JSON.
		/// 
		/// </para>
		/// </summary>
		/// <param name="schemaInfo"> schema info </param>
		/// <returns> a generic schema instance </returns>
		static GenericSchema<GenericRecord> Generic(SchemaInfo SchemaInfo)
		{
			return DefaultImplementation.getGenericSchema(schemaInfo);
		}
	}

	public static class SchemaFields
	{
		public static readonly ISchema<sbyte[]> BYTES = DefaultImplementation.newBytesSchema();
		public static readonly ISchema<IByteBuffer> BYTEBUFFER = DefaultImplementation.newByteBufferSchema();
		public static readonly ISchema<string> STRING = DefaultImplementation.newStringSchema();
		public static readonly ISchema<sbyte> INT8 = DefaultImplementation.newByteSchema();
		public static readonly ISchema<short> INT16 = DefaultImplementation.newShortSchema();
		public static readonly ISchema<int> INT32 = DefaultImplementation.newIntSchema();
		public static readonly ISchema<long> INT64 = DefaultImplementation.newLongSchema();
		public static readonly ISchema<bool> BOOL = DefaultImplementation.newBooleanSchema();
		public static readonly ISchema<float> FLOAT = DefaultImplementation.newFloatSchema();
		public static readonly ISchema<double> DOUBLE = DefaultImplementation.newDoubleSchema();
		public static readonly ISchema<DateTime> DATE = DefaultImplementation.newDateSchema();
		public static readonly ISchema<Time> TIME = DefaultImplementation.newTimeSchema();
		public static readonly ISchema<Timestamp> TIMESTAMP = DefaultImplementation.newTimestampSchema();
	}

}