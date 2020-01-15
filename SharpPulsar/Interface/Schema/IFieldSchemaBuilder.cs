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
namespace Pulsar.Api.Schema
{
	using SchemaType = org.apache.pulsar.common.schema.SchemaType;

	/// <summary>
	/// Build a field for a record.
	/// </summary>
	public interface IFieldSchemaBuilder<T> where T : IFieldSchemaBuilder<T>
	{

		/// <summary>
		/// Set name-value pair properties for this field.
		/// </summary>
		/// <param name="name"> name of the property </param>
		/// <param name="val"> value of the property </param>
		/// <returns> field schema builder </returns>
		T Property(string name, string val);

		/// <summary>
		/// The documentation of this field.
		/// </summary>
		/// <param name="doc"> documentation </param>
		/// <returns> field schema builder </returns>
		T Doc(string doc);

		/// <summary>
		/// The optional name aliases of this field.
		/// </summary>
		/// <param name="aliases"> the name aliases of this field </param>
		/// <returns> field schema builder </returns>
		T Aliases(params string[] aliases);

		/// <summary>
		/// The type of this field.
		/// 
		/// <para>Currently only primitive types are supported.
		/// 
		/// </para>
		/// </summary>
		/// <param name="type"> schema type of this field </param>
		/// <returns> field schema builder </returns>
		T Type(SchemaType type);

		/// <summary>
		/// Make this field optional.
		/// </summary>
		/// <returns> field schema builder </returns>
		T Optional();

		/// <summary>
		/// Make this field required.
		/// </summary>
		/// <returns> field schema builder </returns>
		T Required();

		/// <summary>
		/// Set the default value of this field.
		/// 
		/// <para>The value is validated against the schema type.
		/// 
		/// </para>
		/// </summary>
		/// <returns> value </returns>
		T DefaultValue(object value);

	}

}