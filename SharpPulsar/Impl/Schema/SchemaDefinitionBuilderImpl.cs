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
namespace org.apache.pulsar.client.impl.schema
{
	using SchemaDefinition = org.apache.pulsar.client.api.schema.SchemaDefinition;
	using SchemaDefinitionBuilder = org.apache.pulsar.client.api.schema.SchemaDefinitionBuilder;


	/// <summary>
	/// Builder to build <seealso cref="org.apache.pulsar.client.api.schema.GenericRecord"/>.
	/// </summary>
	public class SchemaDefinitionBuilderImpl<T> : SchemaDefinitionBuilder<T>
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

		public override SchemaDefinitionBuilder<T> withAlwaysAllowNull(bool alwaysAllowNull)
		{
			this.alwaysAllowNull = alwaysAllowNull;
			return this;
		}

		public override SchemaDefinitionBuilder<T> addProperty(string key, string value)
		{
			this.properties[key] = value;
			return this;
		}

		public override SchemaDefinitionBuilder<T> withPojo(Type clazz)
		{
			this.clazz = clazz;
			return this;
		}

		public override SchemaDefinitionBuilder<T> withJsonDef(string jsonDef)
		{
			this.jsonDef = jsonDef;
			return this;
		}

		public override SchemaDefinitionBuilder<T> withSupportSchemaVersioning(bool supportSchemaVersioning)
		{
			this.supportSchemaVersioning = supportSchemaVersioning;
			return this;
		}

		public override SchemaDefinitionBuilder<T> withProperties(IDictionary<string, string> properties)
		{
			this.properties = properties;
			return this;
		}

		public override SchemaDefinition<T> build()
		{
			properties[ALWAYS_ALLOW_NULL] = this.alwaysAllowNull ? "true" : "false";
			return new SchemaDefinitionImpl(clazz, jsonDef, alwaysAllowNull, properties, supportSchemaVersioning);

		}
	}

}