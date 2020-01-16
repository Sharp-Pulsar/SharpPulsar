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
namespace org.apache.pulsar.common.protocol.schema
{
	using AllArgsConstructor = lombok.AllArgsConstructor;
	using Data = lombok.Data;
	using NoArgsConstructor = lombok.NoArgsConstructor;

	/// <summary>
	/// Payload with information about a schema.
	/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Data @AllArgsConstructor @NoArgsConstructor public class PostSchemaPayload
	public class PostSchemaPayload
	{
		private string type;
		private string schema;
		private IDictionary<string, string> properties;
	}

}