﻿using AvroSchemaGenerator;
using SharpPulsar.Common.Schema;
using SharpPulsar.Configuration;
using SharpPulsar.Interfaces.ISchema;
using SharpPulsar.Schemas.Reader;
using SharpPulsar.Schemas.Writer;
using SharpPulsar.Shared;
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
	/// A schema implementation to deal with json data.
	/// </summary>
	public class JSONSchema<T> : AvroBaseStructSchema<T>
	{
		// Cannot use org.apache.pulsar.common.util.ObjectMapperFactory.getThreadLocal() because it does not
		// return shaded version of object mapper
		private static readonly ObjectMapper _jsonMapper = new ObjectMapper();

		private readonly Type _pojo = typeof(T);

		private JSONSchema(ISchemaInfo SchemaInfo, Type pojo, ISchemaReader<T> reader, ISchemaWriter<T> writer) : base(SchemaInfo)
		{
			_pojo = pojo;
			Writer = writer;
			Reader = reader;
		}

		/// <summary>
		/// Implemented for backwards compatibility reasons
		/// since the original schema generated by JSONSchema was based off the json schema standard
		/// since then we have standardized on Avro
		/// 
		/// @return
		/// </summary>
		public virtual ISchemaInfo BackwardsCompatibleJsonSchemaInfo
		{
			get
			{
				ISchemaInfo BackwardsCompatibleSchemaInfo;
				try
				{
					var jsonBackwardsCompatibleSchema = _pojo.GetSchema();
                    BackwardsCompatibleSchemaInfo = new SchemaInfo
                    {
                        Name = "",
                        Properties = SchemaInfo.Properties,
                        Type = SchemaType.JSON,
                        Schema = (sbyte[])(object)_jsonMapper.WriteValueAsBytes(jsonBackwardsCompatibleSchema)
                    };
                }
				catch (Exception ex)
				{
					throw ex;
				}
				return BackwardsCompatibleSchemaInfo;
			}
		}

		public static JSONSchema<T> Of(ISchemaDefinition<T> schemaDefinition)
		{
			ISchemaReader<T> Reader = schemaDefinition.SchemaReaderOpt.GetOrElse(new JsonReader<T>(_jsonMapper));
			ISchemaWriter<T> Writer = schemaDefinition.SchemaWriterOpt.GetOrElse(new JsonWriter<T>(_jsonMapper));
			return new JSONSchema<T>(SchemaUtils.ParseSchemaInfo(schemaDefinition, SchemaType.JSON), schemaDefinition.Pojo, Reader, Writer);
		}

		public static JSONSchema<T> Of(Type Pojo)
		{
			return JSONSchema<T>.Of(ISchemaDefinition<T>.Builder().WithPojo(Pojo).Build());
		}

		public static JSONSchema<T> Of<T>(Type Pojo, IDictionary<string, string> Properties)
		{
			return JSONSchema<T>.Of(ISchemaDefinition<T>.Builder().WithPojo(Pojo).WithProperties(Properties).Build());
		}

	}
}
