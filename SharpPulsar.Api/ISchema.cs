using SharpPulsar.Api.Schema;
using SharpPulsar.Shared;
using System;
using DotNetty.Buffers;
using SharpPulsar.Exception;

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
		sbyte[] Encode(T message);

		/// <summary>
		/// Returns whether this schema supports versioning.
		/// 
		/// <para>Most of the schema implementations don't really support schema versioning, or it just doesn't
		/// make any sense to support schema versionings (e.g. primitive schemas). Only schema returns
		/// <seealso cref="IGenericRecord"/> should support schema versioning.
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
		virtual T Decode(sbyte[] bytes, sbyte[] schemaVersion)
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
		virtual void ConfigureSchemaInfo(string topic, string componentName, ISchemaInfo schemaInfo)
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
		/// Create a JSON schema type by extracting the fields of the specified class.
		/// </summary>
		/// <param name="pojo"> the POJO class to be used to extract the JSON schema </param>
		/// <returns> a Schema instance </returns>
		static ISchema<T> Json(T pojo)
		{
			return DefaultImplementation.newJSONSchema(ISchemaDefinition<T>.Builder().WithPojo(pojo).Build());
		}

		/// <summary>
		/// Create a JSON schema type with schema definition.
		/// </summary>
		/// <param name="schemaDefinition"> the definition of the schema </param>
		/// <returns> a Schema instance </returns>
		static ISchema<T> Json(ISchemaDefinition<T> schemaDefinition)
		{
			return DefaultImplementation.newJSONSchema(schemaDefinition);
		}

		/// <summary>
		/// Create a schema instance that automatically deserialize messages
		/// based on the current topic schema.
		/// 
		/// <para>The messages values are deserialized into a <seealso cref="IGenericRecord"/> object.
		/// 
		/// </para>
		/// <para>Currently this is only supported with Avro and JSON schema types.
		/// 
		/// </para>
		/// </summary>
		/// <returns> the auto schema instance </returns>
		static ISchema<IGenericRecord> AutoConsume()
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
		static ISchema<sbyte[]> AutoProduceBytes<T1>(ISchema<T1> schema)
		{
			return DefaultImplementation.newAutoProduceSchema(schema);
		}

		static ISchema<T> GetSchema(ISchemaInfo schemaInfo)
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
		static IGenericSchema<IGenericRecord> Generic(ISchemaInfo schemaInfo)
		{
			return DefaultImplementation.getGenericSchema(schemaInfo);
		}
	}

	

}