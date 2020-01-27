using SharpPulsar.Api.Schema;
using Org.Apache.Pulsar.Common.Schema;

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
namespace SharpPulsar.Impl.Schema
{
	using ByteBuf = io.netty.buffer.ByteBuf;
	using SharpPulsar.Api;
	using SchemaSerializationException = SharpPulsar.Api.SchemaSerializationException;

	public abstract class AbstractSchema<T> : ISchema<T>
	{
		public abstract GenericSchema<GenericRecord> Generic(SchemaInfo SchemaInfo);
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: public abstract SharpPulsar.api.Schema<JavaToDotNetGenericWildcard> getSchema(org.apache.pulsar.common.schema.SchemaInfo schemaInfo);
		public abstract ISchema<object> GetSchema(SchemaInfo SchemaInfo);
		public abstract ISchema<sbyte[]> AUTO_PRODUCE_BYTES<T1>(ISchema<T1> Schema);
		public abstract ISchema<sbyte[]> AutoProduceBytes();
		public abstract ISchema<GenericRecord> AutoConsume();
		public abstract ISchema<GenericRecord> AUTO();
		public abstract ISchema<KeyValue<K, V>> KeyValue(ISchema<K> Key, ISchema<V> Value, KeyValueEncodingType KeyValueEncodingType);
		public abstract ISchema<KeyValue<K, V>> KeyValue(ISchema<K> Key, ISchema<V> Value);
		public abstract ISchema<KeyValue<K, V>> KeyValue(Type Key, Type Value);
		public abstract ISchema<KeyValue<sbyte[], sbyte[]>> KvBytes();
		public abstract ISchema<KeyValue<K, V>> KeyValue(Type Key, Type Value, SchemaType Type);
		public abstract ISchema<T> JSON(SchemaDefinition SchemaDefinition);
		public abstract ISchema<T> JSON(Type Pojo);
		public abstract ISchema<T> AVRO(SchemaDefinition<T> SchemaDefinition);
		public abstract ISchema<T> AVRO(Type Pojo);
		public abstract ISchema<T> PROTOBUF(SchemaDefinition<T> SchemaDefinition);
		public abstract ISchema<T> PROTOBUF(Type Clazz);
		public abstract void ConfigureSchemaInfo(string Topic, string ComponentName, SchemaInfo SchemaInfo);
		public abstract bool RequireFetchingSchemaInfo();
		public abstract SchemaInfo SchemaInfo {get;}
		public abstract T Decode(sbyte[] Bytes, sbyte[] SchemaVersion);
		public abstract T Decode(sbyte[] Bytes);
		public abstract SchemaInfoProvider SchemaInfoProvider {set;}
		public abstract bool SupportSchemaVersioning();
		public abstract sbyte[] Encode(T Message);
		public abstract void Validate(sbyte[] Message);

		/// <summary>
		/// Check if the message read able length length is a valid object for this schema.
		/// 
		/// <para>The implementation can choose what its most efficient approach to validate the schema.
		/// If the implementation doesn't provide it, it will attempt to use <seealso cref="decode(ByteBuf)"/>
		/// to see if this schema can decode this message or not as a validation mechanism to verify
		/// the bytes.
		/// 
		/// </para>
		/// </summary>
		/// <param name="byteBuf"> the messages to verify </param>
		/// <returns> true if it is a valid message </returns>
		/// <exception cref="SchemaSerializationException"> if it is not a valid message </exception>
		public virtual void Validate(ByteBuf ByteBuf)
		{
			throw new SchemaSerializationException("This method is not supported");
		};

		/// <summary>
		/// Decode a byteBuf into an object using the schema definition and deserializer implementation
		/// </summary>
		/// <param name="byteBuf">
		///            the byte buffer to decode </param>
		/// <returns> the deserialized object </returns>
		public abstract T Decode(ByteBuf ByteBuf);
		/// <summary>
		/// Decode a byteBuf into an object using a given version.
		/// </summary>
		/// <param name="byteBuf">
		///            the byte array to decode </param>
		/// <param name="schemaVersion">
		///            the schema version to decode the object. null indicates using latest version. </param>
		/// <returns> the deserialized object </returns>
		public virtual T Decode(ByteBuf ByteBuf, sbyte[] SchemaVersion)
		{
			// ignore version by default (most of the primitive schema implementations ignore schema version)
			return Decode(ByteBuf);
		}
	}

}