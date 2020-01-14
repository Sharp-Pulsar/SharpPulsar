using System.Collections.Generic;

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
namespace org.apache.pulsar.common.schema
{


	using AllArgsConstructor = lombok.AllArgsConstructor;
	using Builder = lombok.Builder;
	using Data = lombok.Data;
	using EqualsAndHashCode = lombok.EqualsAndHashCode;
	using NoArgsConstructor = lombok.NoArgsConstructor;
	using Accessors = lombok.experimental.Accessors;
	using DefaultImplementation = org.apache.pulsar.client.@internal.DefaultImplementation;

	/// <summary>
	/// Information about the schema.
	/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Data @AllArgsConstructor @NoArgsConstructor @Accessors(chain = true) @Builder public class SchemaInfo
	public class SchemaInfo
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @EqualsAndHashCode.Exclude private String name;
		private string name;

		/// <summary>
		/// The schema data in AVRO JSON format.
		/// </summary>
		private sbyte[] schema;

		/// <summary>
		/// The type of schema (AVRO, JSON, PROTOBUF, etc..).
		/// </summary>
		private SchemaType type;

		/// <summary>
		/// Additional properties of the schema definition (implementation defined).
		/// </summary>
		private IDictionary<string, string> properties = Collections.emptyMap();

		public virtual string SchemaDefinition
		{
			get
			{
				if (null == schema)
				{
					return "";
				}
    
				switch (type.innerEnumValue)
				{
					case org.apache.pulsar.common.schema.SchemaType.InnerEnum.AVRO:
					case org.apache.pulsar.common.schema.SchemaType.InnerEnum.JSON:
					case org.apache.pulsar.common.schema.SchemaType.InnerEnum.PROTOBUF:
						return StringHelper.NewString(schema, UTF_8);
					case org.apache.pulsar.common.schema.SchemaType.InnerEnum.KEY_VALUE:
						KeyValue<SchemaInfo, SchemaInfo> schemaInfoKeyValue = DefaultImplementation.decodeKeyValueSchemaInfo(this);
						return DefaultImplementation.jsonifyKeyValueSchemaInfo(schemaInfoKeyValue);
					default:
						return Base64.Encoder.encodeToString(schema);
				}
			}
		}

		public override string ToString()
		{
			return DefaultImplementation.jsonifySchemaInfo(this);
		}

	}

}