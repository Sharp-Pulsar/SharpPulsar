using SharpPulsar.Interface.Schema;
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
namespace SharpPulsar.Impl.Schema
{
	/// <summary>
	/// Builder to build <seealso cref="IGenericRecord"/>.
	/// </summary>
	public class SchemaDefinitionBuilderImpl<T> : ISchemaDefinitionBuilder<T>
	{

		public const string ALWAYS_ALLOW_NULL = "__alwaysAllowNull";

		/// <summary>
		/// the schema definition class
		/// </summary>
		private Type clazz = typeof(T);
		/// <summary>
		/// The flag of schema type always allow null
		/// 
		/// If it's true, will make all of the pojo field generate schema
		/// define default can be null,false default can't be null, but it's
		/// false you can define the field by yourself by the annotation@Nullable
		/// 
		/// </summary>
		private bool alwaysAllowNull = true;

		/// <summary>
		/// The schema info properties
		/// </summary>
		private IDictionary<string, string> properties = new Dictionary<string, string>();

		/// <summary>
		/// The json schema definition
		/// </summary>
		private string jsonDef;

		/// <summary>
		/// The flag of message decode whether by schema version
		/// </summary>
		private bool supportSchemaVersioning = false;

		public ISchemaDefinitionBuilder<T> WithAlwaysAllowNull(bool alwaysAllowNull)
		{
			this.alwaysAllowNull = alwaysAllowNull;
			return this;
		}

		public ISchemaDefinitionBuilder<T> AddProperty(string key, string value)
		{
			this.properties[key] = value;
			return this;
		}

		public ISchemaDefinitionBuilder<T> WithPojo(Type clazz)
		{
			this.clazz = clazz;
			return this;
		}

		public ISchemaDefinitionBuilder<T> WithJsonDef(string jsonDef)
		{
			this.jsonDef = jsonDef;
			return this;
		}

		public ISchemaDefinitionBuilder<T> WithSupportSchemaVersioning(bool supportSchemaVersioning)
		{
			this.supportSchemaVersioning = supportSchemaVersioning;
			return this;
		}

		public ISchemaDefinitionBuilder<T> WithProperties(IDictionary<string, string> properties)
		{
			this.properties = properties;
			return this;
		}

		public ISchemaDefinition<T> Build()
		{
			properties[ALWAYS_ALLOW_NULL] = this.alwaysAllowNull ? "true" : "false";
			return new SchemaDefinitionImpl<T>(clazz, jsonDef, alwaysAllowNull, properties, supportSchemaVersioning);

		}
	}

}