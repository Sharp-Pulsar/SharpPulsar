using SharpPulsar.Interfaces.Interceptor.Schema;
using SharpPulsar.Shared;
using System;
using System.Collections.Generic;
using System.Text;
using SharpPulsar.Impl;

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
namespace SharpPulsar.Common.Schema
{


	/// <summary>
	/// Information about the schema.
	/// </summary>
	public class SchemaInfo : ISchemaInfo
	{
		public string Name { get; set; }

		/// <summary>
		/// The schema data in AVRO JSON format.
		/// </summary>
		public sbyte[] Schema { get; set; }

		/// <summary>
		/// The type of schema (AVRO, JSON, PROTOBUF, etc..).
		/// </summary>
		public SchemaType Type { get; set; }

		/// <summary>
		/// Additional properties of the schema definition (implementation defined).
		/// </summary>
		public IDictionary<string, string> Properties = new Dictionary<string, string>();

		public virtual string SchemaDefinition
		{
			get
			{
				if (null == Schema)
				{
					return "";
				}

				switch (Type.InnerEnumValue)
				{
					case SchemaType.InnerEnum.Avro:
					case SchemaType.InnerEnum.Json:
					case SchemaType.InnerEnum.Protobuf:
						return StringHelper.NewString(Schema, Encoding.UTF8.WebName);
					//case SchemaType.InnerEnum.KeyValue:
						//KeyValue<SchemaInfo, SchemaInfo> schemaInfoKeyValue = DefaultImplementation.d.DecodeKeyValueSchemaInfo(this);
						//return DefaultImplementation.JsonifyKeyValueSchemaInfo(schemaInfoKeyValue);
					default:
						return Convert.ToBase64String((byte[])(object)Schema);
				}
			}
		}

		public override string ToString()
		{
			return DefaultImplementation.JsonifySchemaInfo(this);
		}

	}

}