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
namespace SharpPulsar.Interfaces.Schema
{


	/// <summary>
	/// Builder to build schema definition <seealso cref="SchemaDefinition"/>.
	/// </summary>
	public interface ISchemaDefinitionBuilder
	{

		/// <summary>
		/// Set schema whether always allow null or not.
		/// </summary>
		/// <param name="alwaysAllowNull"> definition null or not </param>
		/// <returns> schema definition builder </returns>
		ISchemaDefinitionBuilder WithAlwaysAllowNull(bool alwaysAllowNull);

		/// <summary>
		/// Set schema info properties.
		/// </summary>
		/// <param name="properties"> schema info properties </param>
		/// <returns> schema definition builder </returns>
		ISchemaDefinitionBuilder WithProperties(IDictionary<string, string> properties);

		/// <summary>
		/// Set schema info properties.
		/// </summary>
		/// <param name="key"> property key </param>
		/// <param name="value"> property value
		/// </param>
		/// <returns> schema definition builder </returns>
		ISchemaDefinitionBuilder AddProperty(string key, string value);

		/// <summary>
		/// Set schema of pojo definition.
		/// </summary>
		/// <param name="pojo"> pojo schema definition
		/// </param>
		/// <returns> schema definition builder </returns>
		ISchemaDefinitionBuilder WithPojo(Type pojo);

		/// <summary>
		/// Set schema of json definition.
		/// </summary>
		/// <param name="jsonDefinition"> json schema definition
		/// </param>
		/// <returns> schema definition builder </returns>
		ISchemaDefinitionBuilder WithJsonDef(string jsonDefinition);

		/// <summary>
		/// Set schema whether decode by schema version.
		/// </summary>
		/// <param name="supportSchemaVersioning"> decode by version
		/// </param>
		/// <returns> schema definition builder </returns>
		ISchemaDefinitionBuilder WithSupportSchemaVersioning(bool supportSchemaVersioning);

        ISchemaDefinitionBuilder WithJsr310ConversionEnabled(bool jsr310ConversionEnabled);

		/// <summary>
		/// Build the schema definition.
		/// </summary>
		/// <returns> the schema definition. </returns>
		ISchemaDefinition Build();

	}

}