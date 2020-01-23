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
	using SharpPulsar.Api.Schema;
	using SharpPulsar.Api.Schema;


	/// <summary>
	/// Builder to build <seealso cref="SharpPulsar.api.schema.GenericRecord"/>.
	/// </summary>
	public class SchemaDefinitionBuilderImpl<T> : SchemaDefinitionBuilder<T>
	{

		public const string AlwaysAllowNull = "__alwaysAllowNull";

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

		public override SchemaDefinitionBuilder<T> WithAlwaysAllowNull(bool AlwaysAllowNull)
		{
			this.alwaysAllowNull = AlwaysAllowNull;
			return this;
		}

		public override SchemaDefinitionBuilder<T> AddProperty(string Key, string Value)
		{
			this.properties[Key] = Value;
			return this;
		}

		public override SchemaDefinitionBuilder<T> WithPojo(Type Clazz)
		{
			this.clazz = Clazz;
			return this;
		}

		public override SchemaDefinitionBuilder<T> WithJsonDef(string JsonDef)
		{
			this.jsonDef = JsonDef;
			return this;
		}

		public override SchemaDefinitionBuilder<T> WithSupportSchemaVersioning(bool SupportSchemaVersioning)
		{
			this.supportSchemaVersioning = SupportSchemaVersioning;
			return this;
		}

		public override SchemaDefinitionBuilder<T> WithProperties(IDictionary<string, string> Properties)
		{
			this.properties = Properties;
			return this;
		}

		public override SchemaDefinition<T> Build()
		{
			properties[AlwaysAllowNull] = this.alwaysAllowNull ? "true" : "false";
			return new SchemaDefinitionImpl(clazz, jsonDef, alwaysAllowNull, properties, supportSchemaVersioning);

		}
	}

}