﻿using System.Collections.Concurrent;
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


	/// <summary>
	/// A json schema definition
	/// <seealso cref="ISchemaDefinition<T>"/> for the json schema definition.
	/// </summary>
	public class SchemaDefinitionImpl<T> : ISchemaDefinition<T>
	{

		/// <summary>
		/// the schema definition class
		/// </summary>
		private T _pojo;
		/// <summary>
		/// The flag of schema type always allow null
		/// 
		/// If it's true, will make all of the pojo field generate schema
		/// define default can be null,false default can't be null, but it's
		/// false you can define the field by yourself by the annotation@Nullable
		/// 
		/// </summary>
		public virtual bool AlwaysAllowNull {get;}

		private IDictionary<string, string> _properties;

		public virtual string JsonDef {get;}

		public virtual bool SupportSchemaVersioning {get;}

		public SchemaDefinitionImpl(T pojo, string jsonDef, bool alwaysAllowNull, IDictionary<string, string> properties, bool supportSchemaVersioning)
		{
			AlwaysAllowNull = alwaysAllowNull;
			_properties = properties;
			JsonDef = jsonDef;
			_pojo = pojo;
			SupportSchemaVersioning = supportSchemaVersioning;
		}
		/// <summary>
		/// get schema whether always allow null or not
		/// </summary>
		/// <returns> schema always null or not </returns>

		/// <summary>
		/// Get json schema definition
		/// </summary>
		/// <returns> schema class </returns>
		/// <summary>
		/// Get pojo schema definition
		/// </summary>
		/// <returns> pojo class </returns>
		public virtual T Pojo => _pojo;


        /// <summary>
		/// Get schema class
		/// </summary>
		/// <returns> schema class </returns>
		public virtual IDictionary<string, string> Properties => new ConcurrentDictionary<string, string>(_properties);
    }

}