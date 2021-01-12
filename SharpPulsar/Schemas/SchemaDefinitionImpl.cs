using Akka.Util;
using SharpPulsar.Interfaces.ISchema;
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
namespace SharpPulsar.Schemas
{

	/// <summary>
	/// A json schema definition
	/// <seealso cref="ISchemaDefinition<T>"/> for the json schema definition.
	/// </summary>
	public class SchemaDefinitionImpl<T> : ISchemaDefinition<T>
	{

		/// <summary>
		/// the schema definition class
		/// </summary>
		private Type _pojo = typeof(T);
		/// <summary>
		/// The flag of schema type always allow null
		/// 
		/// If it's true, will make all of the pojo field generate schema
		/// define default can be null,false default can't be null, but it's
		/// false you can define the field by yourself by the annotation@Nullable
		/// 
		/// </summary>
		private readonly bool _alwaysAllowNull;

		private readonly IDictionary<string, string> _properties;

		private readonly string _jsonDef;

		private readonly bool _supportSchemaVersioning;

		private readonly bool _jsr310ConversionEnabled;

		private readonly ISchemaReader<T> _reader;

		private readonly ISchemaWriter<T> _writer;

		public SchemaDefinitionImpl(Type pojo, string jsonDef, bool alwaysAllowNull, IDictionary<string, string> properties, bool supportSchemaVersioning, bool jsr310ConversionEnabled, ISchemaReader<T> reader, ISchemaWriter<T> writer)
		{
			this._alwaysAllowNull = alwaysAllowNull;
			this._properties = properties;
			this._jsonDef = jsonDef;
			this._pojo = pojo;
			this._supportSchemaVersioning = supportSchemaVersioning;
			this._jsr310ConversionEnabled = jsr310ConversionEnabled;
			this._reader = reader;
			this._writer = writer;
		}

		/// <summary>
		/// get schema whether always allow null or not
		/// </summary>
		/// <returns> schema always null or not </returns>
		public virtual bool AlwaysAllowNull
		{
			get
			{
				return _alwaysAllowNull;
			}
		}

		public bool Jsr310ConversionEnabled
		{
			get
			{
				return _jsr310ConversionEnabled;
			}
		}

		/// <summary>
		/// Get json schema definition
		/// </summary>
		/// <returns> schema class </returns>
		public virtual string JsonDef
		{
			get
			{
				return _jsonDef;
			}
		}

		/// <summary>
		/// Get pojo schema definition
		/// </summary>
		/// <returns> pojo class </returns>
		public Type Pojo
		{
			get
			{
				return _pojo;
			}
		}

		public bool SupportSchemaVersioning
		{
			get
			{
				return _supportSchemaVersioning;
			}
		}

		public Option<ISchemaReader<T>> SchemaReaderOpt
		{
			get
			{
				return new Option<ISchemaReader<T>>(_reader);
			}
		}

		public Option<ISchemaWriter<T>> SchemaWriterOpt
		{
			get
			{
				return new Option<ISchemaWriter<T>>(_writer);
			}
		}

		/// <summary>
		/// Get schema class
		/// </summary>
		/// <returns> schema class </returns>
		public virtual IDictionary<string, string> Properties
		{
			get
			{
				return new Dictionary<string, string>(_properties);
			}
		}

	}
}
