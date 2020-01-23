using System;
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
namespace SharpPulsar.Api.Schema
{
	using DefaultImplementation = Org.Apache.Pulsar.Client.@internal.DefaultImplementation;

	/// <summary>
	/// Interface for schema definition.
	/// </summary>
	public interface SchemaDefinition<T>
	{

		/// <summary>
		/// Get a new builder instance that can used to configure and build a <seealso cref="SchemaDefinition"/> instance.
		/// </summary>
		/// <returns> the <seealso cref="SchemaDefinition"/> </returns>
		static SchemaDefinitionBuilder<T> Builder<T>()
		{
			return DefaultImplementation.newSchemaDefinitionBuilder();
		}

		/// <summary>
		/// Get schema whether always allow null or not.
		/// </summary>
		/// <returns> schema always null or not </returns>
		bool AlwaysAllowNull {get;}

		/// <summary>
		/// Get schema class.
		/// </summary>
		/// <returns> schema class </returns>
		IDictionary<string, string> Properties {get;}

		/// <summary>
		/// Get json schema definition.
		/// </summary>
		/// <returns> schema class </returns>
		string JsonDef {get;}

		/// <summary>
		/// Get pojo schema definition.
		/// </summary>
		/// <returns> pojo schema </returns>
		Type<T> Pojo {get;}

		/// <summary>
		/// Get supportSchemaVersioning schema definition.
		/// </summary>
		/// <returns> the flag of supportSchemaVersioning </returns>
		bool SupportSchemaVersioning {get;}
	}

}