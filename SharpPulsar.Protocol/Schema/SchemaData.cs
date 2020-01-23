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
namespace SharpPulsar.Protocol.Schema
{
	using Builder = lombok.Builder;
	using Data = lombok.Data;
	using ToString = lombok.ToString;
	using SchemaInfo = SharpPulsar.Schema.SchemaInfo;
	using SchemaType = SharpPulsar.Schema.SchemaType;

	/// <summary>
	/// Schema data.
	/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Builder @Data @ToString public class SchemaData
	public class SchemaData
	{
		private readonly SchemaType type;
		private readonly bool isDeleted;
		private readonly long timestamp;
		private readonly string user;
		private readonly sbyte[] data;
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Builder.Default private java.util.Map<String, String> props = new java.util.HashMap<>();
		private IDictionary<string, string> props = new Dictionary<string, string>();

		/// <summary>
		/// Convert a schema data to a schema info.
		/// </summary>
		/// <returns> the converted schema info. </returns>
		public virtual SchemaInfo ToSchemaInfo()
		{
			return SchemaInfo.builder().name("").type(type).schema(data).properties(props).build();
		}

		/// <summary>
		/// Convert a schema info to a schema data.
		/// </summary>
		/// <param name="schemaInfo"> schema info </param>
		/// <returns> the converted schema schema data </returns>
		public static SchemaData FromSchemaInfo(SchemaInfo SchemaInfo)
		{
			return SchemaData.builder().type(SchemaInfo.Type).data(SchemaInfo.Schema).props(SchemaInfo.Properties).build();
		}

	}
}