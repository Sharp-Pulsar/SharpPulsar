using System;
using SchemaSerializationException = SharpPulsar.Exceptions.SchemaSerializationException;
using SharpPulsar.Interfaces;
using SharpPulsar.Interfaces.Schema;

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
namespace SharpPulsar.Schema
{
	public abstract class AbstractSchema<T> : ISchema<T>
	{
		public virtual ISchema<T> Auto()
        {
			throw new NotImplementedException();
        }

		public virtual ISchema<T> Json(ISchemaDefinition schemaDefinition)
		{
			throw new NotImplementedException();
		}
		public virtual ISchema<T> Json(object pojo)
		{
			throw new NotImplementedException();
		}

		public virtual void ConfigureSchemaInfo(string topic, string componentName, ISchemaInfo schemaInfo)
		{
			throw new NotImplementedException();
		}
		public virtual bool RequireFetchingSchemaInfo()
		{
			return false;
		}

		public virtual ISchemaInfo SchemaInfo {get;}
		public virtual ISchemaInfoProvider SchemaInfoProvider {
			set { 
			}
		}

		public virtual bool SupportSchemaVersioning()
		{
			return true;
		}
		public abstract sbyte[] Encode(T message);
		public virtual void Validate(sbyte[] message)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Check if the message read able Length Length is a valid object for this schema.
		/// 
		/// <para>The implementation can choose what its most efficient approach to validate the schema.
		/// If the implementation doesn't provide it, it will attempt to use <seealso cref="Decode(ByteBuf)"/>
		/// to see if this schema can decode this message or not as a validation mechanism to verify
		/// the bytes.
		/// 
		/// </para>
		/// </summary>
		/// <param name="byteBuf"> the messages to verify </param>
		/// <returns> true if it is a valid message </returns>
		/// <exception cref="SchemaSerializationException"> if it is not a valid message </exception>
		public virtual void Validate(byte[] byteBuf)
		{
			throw new SchemaSerializationException("This method is not supported");
		}

		/// <summary>
		/// Decode a byteBuf into an object using the schema definition and deserializer implementation
		/// </summary>
		/// <param name="byteBuf">
		///            the byte buffer to decode </param>
		/// <returns> the deserialized object </returns>
		public abstract T Decode(byte[] byteBuf);
		/// <summary>
		/// Decode a byteBuf into an object using a given version.
		/// </summary>
		/// <param name="byteBuf">
		///            the byte array to decode </param>
		/// <param name="schemaVersion">
		///            the schema version to decode the object. null indicates using latest version. </param>
		/// <returns> the deserialized object </returns>
		public virtual T Decode(byte[] byteBuf, byte[] schemaVersion)
		{
			// ignore version by default (most of the primitive schema implementations ignore schema version)
			return Decode(byteBuf);
		}
        
		public ISchema<T> Clone()
		{
			return this;
		}
        
        object ICloneable.Clone()
        {
            throw new NotImplementedException();
        }
    }

}