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

using System;
using SharpPulsar.Api;
using SharpPulsar.Api.Schema;
using SharpPulsar.Shared;

namespace SharpPulsar.Impl.Schema
{
	/// <summary>
	/// Auto detect schema.
	/// </summary>
	public class AutoProduceBytesSchema<T> : ISchema<sbyte[]>
	{

		private bool _requireSchemaValidation = true;
		private ISchema<T> _schema;

		public AutoProduceBytesSchema()
		{
		}

		public AutoProduceBytesSchema(ISchema<T> schema)
		{
			_schema = schema;
			var schemaInfo = schema.SchemaInfo;
			_requireSchemaValidation = schemaInfo != null && schemaInfo.Type != SchemaType.Bytes && schemaInfo.Type != SchemaType.None;
		}

		public virtual ISchema<T> Schema
		{
            get => _schema;
            set
			{
				_schema = value;
				_requireSchemaValidation = value.SchemaInfo != null && SchemaType.Bytes != value.SchemaInfo.Type && SchemaType.None != value.SchemaInfo.Type;
			}
		}

		private void EnsureSchemaInitialized()
		{
			if(!SchemaInitialized())
                throw new ArgumentException("Schema is not initialized before used");
		}

		public virtual bool SchemaInitialized()
		{
			return _schema != null;
		}

		public void Validate(sbyte[] message)
		{
			EnsureSchemaInitialized();

			_schema.Validate(message);
		}

		public sbyte[] Encode(sbyte[] message)
		{
			EnsureSchemaInitialized();

			if (_requireSchemaValidation)
			{
				// verify if the message can be decoded by the underlying schema
				_schema.Validate(message);
			}

			return message;
		}

		public sbyte[] Decode(sbyte[] bytes, sbyte[] schemaVersion)
		{
			EnsureSchemaInitialized();

			if (_requireSchemaValidation)
			{
				// verify the message can be detected by the underlying schema
				_schema.Decode(bytes, schemaVersion);
			}

			return bytes;
		}

		public virtual ISchemaInfo SchemaInfo
		{
			get
			{
				EnsureSchemaInitialized();
    
				return _schema.SchemaInfo;
			}
		}
	}

}