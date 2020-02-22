using System;
using System.Collections.Generic;
using Avro.Generic;

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


	/// <summary>
	/// Builder to build <seealso cref="GenericRecord"/>.
	/// </summary>
	public class SchemaDefinitionBuilderImpl<T> : ISchemaDefinitionBuilder<T>
	{

		public const string AlwaysAllowNull = "__alwaysAllowNull";

		/// <summary>
		/// the schema definition class
		/// </summary>
		private Type _clazz;
		/// <summary>
		/// The flag of schema type always allow null
		/// 
		/// If it's true, will make all of the pojo field generate schema
		/// define default can be null,false default can't be null, but it's
		/// false you can define the field by yourself by the annotation@Nullable
		/// 
		/// </summary>
		private bool _alwaysAllowNull = true;

		/// <summary>
		/// The schema info properties
		/// </summary>
		private IDictionary<string, string> _properties = new Dictionary<string, string>();

		/// <summary>
		/// The json schema definition
		/// </summary>
		private string _jsonDef;

		/// <summary>
		/// The flag of message decode whether by schema version
		/// </summary>
		private bool _supportSchemaVersioning = false;

		public ISchemaDefinitionBuilder<T> WithAlwaysAllowNull(bool alwaysAllowNull)
		{
			_alwaysAllowNull = alwaysAllowNull;
			return this;
		}

		public ISchemaDefinitionBuilder<T> AddProperty(string key, string value)
		{
			_properties[key] = value;
			return this;
		}

		public ISchemaDefinitionBuilder<T> WithPojo(Type clazz)
		{
			_clazz = clazz;
			return this;
		}

		public ISchemaDefinitionBuilder<T> WithJsonDef(string jsonDef)
		{
			_jsonDef = jsonDef;
			return this;
		}

		public  ISchemaDefinitionBuilder<T> WithSupportSchemaVersioning(bool supportSchemaVersioning)
		{
			_supportSchemaVersioning = supportSchemaVersioning;
			return this;
		}

		public  ISchemaDefinitionBuilder<T> WithProperties(IDictionary<string, string> properties)
		{
			_properties = properties;
			return this;
		}

		public  ISchemaDefinition<T> Build()
		{
			_properties[AlwaysAllowNull] = _alwaysAllowNull ? "true" : "false";
			return new SchemaDefinitionImpl<T>(_clazz, _jsonDef, _alwaysAllowNull, _properties, _supportSchemaVersioning);

		}
	}

}