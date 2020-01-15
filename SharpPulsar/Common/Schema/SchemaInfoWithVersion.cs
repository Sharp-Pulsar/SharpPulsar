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
	using NoArgsConstructor = lombok.NoArgsConstructor;
	using Accessors = lombok.experimental.Accessors;
	using DefaultImplementation = org.apache.pulsar.client.@internal.DefaultImplementation;

	/// <summary>
	/// Data structure representing a schema information including its version.
	/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Data @AllArgsConstructor @NoArgsConstructor @Accessors(chain = true) @Builder public class SchemaInfoWithVersion
	public class SchemaInfoWithVersion
	{

		private long version;

		private SchemaInfo schemaInfo;

		public override string ToString()
		{
			return DefaultImplementation.jsonifySchemaInfoWithVersion(this);
		}
	}

}