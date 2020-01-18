using SharpPulsar.Common.Schema;
using SharpPulsar.Interface.Schema;
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

	/// <summary>
	/// Auto detect schema.
	/// </summary>
	public class AutoProduceBytesSchema<T> : ISchema<sbyte[]>
	{

		private bool requireSchemaValidation = true;
		private ISchema<T> schema;

		public AutoProduceBytesSchema()
		{
		}

		public AutoProduceBytesSchema(ISchema<T> schema)
		{
			this.schema = schema;
			SchemaInfo schemaInfo = schema.SchemaInfo;
			this.requireSchemaValidation = schemaInfo != null && schemaInfo.Type != SchemaType.BYTES && schemaInfo.Type != SchemaType.NONE;
		}

		public virtual ISchema<T> Schema
		{
			set
			{
				this.schema = value;
				this.requireSchemaValidation = value.SchemaInfo != null && SchemaType.BYTES != value.SchemaInfo.Type && SchemaType.NONE != value.SchemaInfo.Type;
			}
		}

		private void EnsureSchemaInitialized()
		{
			checkState(SchemaInitialized(), "Schema is not initialized before used");
		}

		public virtual bool SchemaInitialized()
		{
			return schema != null;
		}

		public void Validate(sbyte[] message)
		{
			EnsureSchemaInitialized();

			schema.Validate((byte[])(Array)message);
		}

		public sbyte[] Encode(sbyte[] message)
		{
			EnsureSchemaInitialized();

			if (requireSchemaValidation)
			{
				// verify if the message can be decoded by the underlying schema
				schema.Validate((byte[])(Array)message);
			}

			return message;
		}

		public sbyte[] Decode(sbyte[] bytes, sbyte[] schemaVersion)
		{
			EnsureSchemaInitialized();

			if (requireSchemaValidation)
			{
				// verify the message can be detected by the underlying schema
				schema.Decode((byte[])(Array)bytes, (byte[])(Array)schemaVersion);
			}

			return bytes;
		}

		public SchemaInfo SchemaInfo
		{
			get
			{
				EnsureSchemaInitialized();
    
				return schema.SchemaInfo;
			}
		}
	}

}