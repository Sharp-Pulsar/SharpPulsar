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
	/// Builder to build schema definition <seealso cref="ISchemaDefinition<T>"/>.
	/// </summary>
	//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
	//ORIGINAL LINE: @InterfaceAudience.Public @InterfaceStability.Stable public interface SchemaDefinitionBuilder<T>
	public interface ISchemaDefinitionBuilder<T>
	{

		/// <summary>
		/// Set schema whether always allow null or not.
		/// </summary>
		/// <param name="alwaysAllowNull"> definition null or not </param>
		/// <returns> schema definition builder </returns>
		ISchemaDefinitionBuilder<T> WithAlwaysAllowNull(bool alwaysAllowNull);

		/// <summary>
		/// Set schema use JRS310 conversion or not.
		/// 
		/// <para>Before Avro 1.9 the Joda time library was used for handling the logical date(time) values.
		/// But since the introduction of Java8 the Java Specification Request (JSR) 310 has been included,
		/// which greatly improves the handling of date and time natively. To keep forwarding compatibility,
		/// default is use Joda time conversion.
		/// 
		/// </para>
		/// <para>JSR310 conversion is recommended here. Joda time conversion is has been marked deprecated.
		/// In future versions, joda time conversion may be removed
		/// 
		/// </para>
		/// </summary>
		/// <param name="jsr310ConversionEnabled"> use JRS310 conversion or not, default is false for keep forwarding compatibility </param>
		/// <returns> schema definition builder </returns>
		ISchemaDefinitionBuilder<T> WithJSR310ConversionEnabled(bool jsr310ConversionEnabled);

		/// <summary>
		/// Set schema info properties.
		/// </summary>
		/// <param name="properties"> schema info properties </param>
		/// <returns> schema definition builder </returns>
		ISchemaDefinitionBuilder<T> WithProperties(IDictionary<string, string> properties);

		/// <summary>
		/// Set schema info properties.
		/// </summary>
		/// <param name="key"> property key </param>
		/// <param name="value"> property value
		/// </param>
		/// <returns> schema definition builder </returns>
		ISchemaDefinitionBuilder<T> AddProperty(string key, string value);

		/// <summary>
		/// Set schema of pojo definition.
		/// </summary>
		/// <param name="pojo"> pojo schema definition
		/// </param>
		/// <returns> schema definition builder </returns>
		ISchemaDefinitionBuilder<T> WithPojo(Type pojo);

		/// <summary>
		/// Set schema of json definition.
		/// </summary>
		/// <param name="jsonDefinition"> json schema definition
		/// </param>
		/// <returns> schema definition builder </returns>
		ISchemaDefinitionBuilder<T> WithJsonDef(string jsonDefinition);

		/// <summary>
		/// Set schema whether decode by schema version.
		/// </summary>
		/// <param name="supportSchemaVersioning"> decode by version
		/// </param>
		/// <returns> schema definition builder </returns>
		ISchemaDefinitionBuilder<T> WithSupportSchemaVersioning(bool supportSchemaVersioning);

		/// <summary>
		/// Set schema reader for deserialization of object data.
		/// </summary>
		/// <param name="reader"> reader for object data
		/// </param>
		/// <returns> schema definition builder </returns>
		ISchemaDefinitionBuilder<T> WithSchemaReader(ISchemaReader<T> reader);

		/// <summary>
		/// Set schema writer for serialization of objects.
		/// </summary>
		/// <param name="writer"> writer for objects
		/// </param>
		/// <returns> schema definition builder </returns>
		ISchemaDefinitionBuilder<T> WithSchemaWriter(ISchemaWriter<T> writer);

		/// <summary>
		/// Build the schema definition.
		/// </summary>
		/// <returns> the schema definition. </returns>
		ISchemaDefinition<T> Build();

	}
}