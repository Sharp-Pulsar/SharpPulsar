using System;
using SharpPulsar.Api.Schema;
using DotNetty.Buffers;
using SharpPulsar.Api;
using SchemaSerializationException = SharpPulsar.Exceptions.SchemaSerializationException;

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
    using SharpPulsar.Common.Schema;

    public abstract class AbstractSchema : ISchema
	{
		public abstract ISchema Auto();

		public abstract ISchema Json(ISchemaDefinition schemaDefinition);
		public abstract ISchema Json(object pojo);

		public abstract void ConfigureSchemaInfo(string topic, string componentName, SchemaInfo schemaInfo);
		public abstract bool RequireFetchingSchemaInfo();

		public abstract ISchemaInfo SchemaInfo {get;}
		public abstract ISchemaInfoProvider SchemaInfoProvider {set;}

		public abstract bool SupportSchemaVersioning();
		public abstract sbyte[] Encode(object message);
		public abstract void Validate(sbyte[] message);

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
		public virtual void Validate(IByteBuffer byteBuf)
		{
			throw new SchemaSerializationException("This method is not supported");
		}

		/// <summary>
		/// Decode a byteBuf into an object using the schema definition and deserializer implementation
		/// </summary>
		/// <param name="byteBuf">
		///            the byte buffer to decode </param>
		/// <returns> the deserialized object </returns>
		public abstract object Decode(IByteBuffer byteBuf, Type returnType);
		/// <summary>
		/// Decode a byteBuf into an object using a given version.
		/// </summary>
		/// <param name="byteBuf">
		///            the byte array to decode </param>
		/// <param name="schemaVersion">
		///            the schema version to decode the object. null indicates using latest version. </param>
		/// <returns> the deserialized object </returns>
		public virtual object Decode(IByteBuffer byteBuf, sbyte[] schemaVersion, Type returnType)
		{
			// ignore version by default (most of the primitive schema implementations ignore schema version)
			return Decode(byteBuf, returnType);
		}
        
        public abstract object Decode(sbyte[] bytes, Type returnType);
	}

}