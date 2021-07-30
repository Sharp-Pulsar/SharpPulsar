using NodaTime;
using SharpPulsar.Interfaces.ISchema;
using SharpPulsar.Schemas;
using SharpPulsar.Shared;
using System;
using System.Diagnostics.CodeAnalysis;

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
namespace SharpPulsar.Interfaces
{ 
	/// <summary>
	/// Message schema definition.
	/// </summary>
	public interface ISchema<T>:ICloneable
	{/// <summary>
	 /// Check if the message is a valid object for this schema.
	 /// 
	 /// <para>The implementation can choose what its most efficient approach to validate the schema.
	 /// If the implementation doesn't provide it, it will attempt to use <seealso cref="decode(byte[])"/>
	 /// to see if this schema can decode this message or not as a validation mechanism to verify
	 /// the bytes.
	 /// 
	 /// </para>
	 /// </summary>
	 /// <param name="message"> the messages to verify </param>
	 /// <exception cref="SchemaSerializationException"> if it is not a valid message </exception>
		virtual void Validate([NotNull] byte[] message)
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
		byte[] Encode([NotNull]T message);

		/// <summary>
		/// Returns whether this schema supports versioning.
		/// 
		/// <para>Most of the schema implementations don't really support schema versioning, or it just doesn't
		/// make any sense to support schema versionings (e.g. primitive schemas). Only schema returns
		/// <seealso cref="GenericRecord"/> should support schema versioning.
		/// 
		/// </para>
		/// <para>If a schema implementation returns <tt>false</tt>, it should implement <seealso cref="decode(byte[])"/>;
		/// while a schema implementation returns <tt>true</tt>, it should implement <seealso cref="decode(byte[], byte[])"/>
		/// instead.
		/// 
		/// </para>
		/// </summary>
		/// <returns> true if this schema implementation supports schema versioning; otherwise returns false. </returns>
		virtual bool SupportSchemaVersioning()
		{
			return false;
		}

		virtual ISchemaInfoProvider SchemaInfoProvider
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
		virtual T Decode([NotNull]byte[] bytes)
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
		virtual T Decode([NotNull]byte[] bytes, [NotNull]byte[] schemaVersion)
		{
			// ignore version by default (most of the primitive schema implementations ignore schema version)
			return Decode(bytes);
		}

		/// <returns> an object that represents the Schema associated metadata </returns>
		ISchemaInfo SchemaInfo { get; }

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
		virtual void ConfigureSchemaInfo(string topic, string componentName, ISchemaInfo schemaInfo)
		{
			// no-op
		}

		/// <summary>
		/// Duplicates the schema.
		/// </summary>
		/// <returns> The duplicated schema. </returns>
		ISchema<T> Clone();

		/// <summary>
		/// Schema that doesn't perform any encoding on the message payloads. Accepts a byte array and it passes it through.
		/// </summary>
		public static ISchema<byte[]> Bytes = DefaultImplementation.NewBytesSchema();


		/// <summary>
		/// Schema that can be used to encode/decode messages whose values are String. The payload is encoded with UTF-8.
		/// </summary>
		public static ISchema<string> String = DefaultImplementation.NewStringSchema();

		/// <summary>
		/// INT8 Schema.
		/// </summary>
		public static ISchema<byte> Int8 = DefaultImplementation.NewByteSchema();

		/// <summary>
		/// INT16 Schema.
		/// </summary>
		public static ISchema<short> Int16 = DefaultImplementation.NewShortSchema();

		/// <summary>
		/// INT32 Schema.
		/// </summary>
		public static ISchema<int> Int32 = DefaultImplementation.NewIntSchema();

		/// <summary>
		/// INT64 Schema.
		/// </summary>
		public static ISchema<long> Int64 = DefaultImplementation.NewLongSchema();

		/// <summary>
		/// Boolean Schema.
		/// </summary>
		public static ISchema<bool> Bool = DefaultImplementation.NewBooleanSchema();

		/// <summary>
		/// Float Schema.
		/// </summary>
		public static ISchema<float> Float = DefaultImplementation.NewFloatSchema();

		/// <summary>
		/// Double Schema.
		/// </summary>
		public static ISchema<double> Double = DefaultImplementation.NewDoubleSchema();

		/// <summary>
		/// Date Schema.
		/// </summary>
		public static ISchema<DateTime> Date = DefaultImplementation.NewDateSchema();

		/// <summary>
		/// Instant Schema.
		/// </summary>
		public static ISchema<Instant> Instant = DefaultImplementation.NewInstantSchema();
		/// <summary>
		/// LocalDate Schema.
		/// </summary>
		public static ISchema<LocalDate> LocalDate = DefaultImplementation.NewLocalDateSchema();
		/// <summary>
		/// LocalTime Schema.
		/// </summary>
		public static ISchema<LocalTime> LocalTime = DefaultImplementation.NewLocalTimeSchema();
		/// <summary>
		/// LocalDateTime Schema.
		/// </summary>
		public static ISchema<LocalDateTime> LocalDateTime = DefaultImplementation.NewLocalDateTimeSchema();

		/// <summary>
		/// Create a  Avro schema type by default configuration of the class.
		/// </summary>
		/// <param name="pojo"> the POJO class to be used to extract the Avro schema </param>
		/// <returns> a Schema instance </returns>
		static ISchema<T> Avro(Type pojo)
		{
			return DefaultImplementation.NewAvroSchema(ISchemaDefinition<T>.Builder().WithPojo(pojo).Build());
		}

		/// <summary>
		/// Create a Avro schema type with schema definition.
		/// </summary>
		/// <param name="schemaDefinition"> the definition of the schema </param>
		/// <returns> a Schema instance </returns>
		static ISchema<O> Avro<O>(ISchemaDefinition<O> schemaDefinition)
		{
			return DefaultImplementation.NewAvroSchema(schemaDefinition);
		}

		/// <summary>
		/// Create a JSON schema type by extracting the fields of the specified class.
		/// </summary>
		/// <param name="pojo"> the POJO class to be used to extract the JSON schema </param>
		/// <returns> a Schema instance </returns>
		static ISchema<T> Json<T>(Type pojo)
		{
			return DefaultImplementation.NewJsonSchema(ISchemaDefinition<T>.Builder().WithPojo(pojo).Build());
		}

		/// <summary>
		/// Create a JSON schema type with schema definition.
		/// </summary>
		/// <param name="schemaDefinition"> the definition of the schema </param>
		/// <returns> a Schema instance </returns>
		static ISchema<T> Json<T>(ISchemaDefinition<T> schemaDefinition)
		{
			return DefaultImplementation.NewJsonSchema(schemaDefinition);
		}

		/// <summary>
		/// Key Value Schema using passed in schema type, support JSON and AVRO currently.
		/// </summary>
		static ISchema<KeyValue<K, V>> KeyValue<K, V>(Type key, Type value, SchemaType type)
		{
			return DefaultImplementation.NewKeyValueSchema<K, V>(key, value, type);
		}

		/// <summary>
		/// Schema that can be used to encode/decode KeyValue.
		/// </summary>
		static ISchema<KeyValue<byte[], byte[]>> KvBytes()
		{
			return DefaultImplementation.NewKeyValueBytesSchema();
		}

		/// <summary>
		/// Key Value Schema whose underneath key and value schemas are JSONSchema.
		/// </summary>
		static ISchema<KeyValue<K, V>> KeyValue<K, V>(Type key, Type value)
		{
			return DefaultImplementation.NewKeyValueSchema<K, V>(key, value, SchemaType.JSON);
		}

		/// <summary>
		/// Key Value Schema using passed in key and value schemas.
		/// </summary>
		static ISchema<KeyValue<K, V>> KeyValue<K, V>(ISchema<K> key, ISchema<V> value)
		{
			return DefaultImplementation.NewKeyValueSchema(key, value);
		}

		/// <summary>
		/// Key Value Schema using passed in key, value and encoding type schemas.
		/// </summary>
		static ISchema<KeyValue<K, V>> KeyValue<K, V>(ISchema<K> key, ISchema<V> value, KeyValueEncodingType keyValueEncodingType)
		{
			return DefaultImplementation.NewKeyValueSchema(key, value, keyValueEncodingType);
		}

		[Obsolete]
		static ISchema<IGenericRecord> Auto()
		{
			return AutoConsume();
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
		static ISchema<IGenericRecord> AutoConsume()
		{
			return DefaultImplementation.NewAutoConsumeSchema();
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
		static ISchema<byte[]> AutoProduceBytes()
		{
			return DefaultImplementation.NewAutoProduceSchema();
		}

		/// <summary>
		/// Create a schema instance that accepts a serialized payload
		/// and validates it against the schema specified.
		/// </summary>
		/// <returns> the auto schema instance
		/// @since 2.5.0 </returns>
		/// <seealso cref= #AUTO_PRODUCE_BYTES() </seealso>
		static ISchema<byte[]> AutoProduceBytes<T1>(ISchema<T1> schema)
		{
			return DefaultImplementation.NewAutoProduceSchema(schema);
		}

		static ISchema<object> GetSchema(ISchemaInfo schemaInfo)
		{
			return DefaultImplementation.GetSchema(schemaInfo);
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
		static IGenericSchema<IGenericRecord> Generic(ISchemaInfo schemaInfo)
		{
			return DefaultImplementation.GetGenericSchema(schemaInfo);
		}
	}

}