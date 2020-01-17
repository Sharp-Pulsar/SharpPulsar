﻿using System;
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
	using JsonInclude = com.fasterxml.jackson.annotation.JsonInclude;
	using JsonProcessingException = com.fasterxml.jackson.core.JsonProcessingException;
	using DeserializationFeature = com.fasterxml.jackson.databind.DeserializationFeature;
	using ObjectMapper = com.fasterxml.jackson.databind.ObjectMapper;
	using JsonSchema = com.fasterxml.jackson.module.jsonSchema.JsonSchema;
	using JsonSchemaGenerator = com.fasterxml.jackson.module.jsonSchema.JsonSchemaGenerator;
    using System.Threading;
    using Pulsar.Client.Impl.Schema.Writer;
    using Pulsar.Client.Impl.Schema.Reader;
    using SharpPulsar.Interface.Schema;
    using SharpPulsar.Common.Schema;
    using SharpPulsar.Common.Protocol.Schema;

    /// <summary>
    /// A schema implementation to deal with json data.
    /// </summary>
    //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
    //ORIGINAL LINE: @Slf4j public class JSONSchema<T> extends StructSchema<T>
    public class JSONSchema<T> : StructSchema<T>
	{
		// Cannot use org.apache.pulsar.common.util.ObjectMapperFactory.getThreadLocal() because it does not
		// return shaded version of object mapper
		private static readonly ThreadLocal<ObjectMapper> JSON_MAPPER = ThreadLocal.withInitial(() =>
		{
			ObjectMapper mapper = new ObjectMapper();
			mapper.configure(DeserializationFeature.FAIL_ON_UNKNOWN_PROPERTIES, false);
			mapper.SerializationInclusion = JsonInclude.Include.NON_NULL;
			return mapper;
		});

		private readonly Type pojo = typeof(T);

		private JSONSchema(SchemaInfo schemaInfo, Type pojo) : base(schemaInfo)
		{
			this.pojo = pojo;
			Writer = new JsonWriter<T>(JSON_MAPPER.get());
			Reader = new JsonReader<T>(JSON_MAPPER.get(), pojo);
		}

		protected internal ISchemaReader<T> LoadReader(BytesSchemaVersion schemaVersion)
		{
			throw new Exception("JSONSchema don't support schema versioning");
		}

		/// <summary>
		/// Implemented for backwards compatibility reasons
		/// since the original schema generated by JSONSchema was based off the json schema standard
		/// since then we have standardized on Avro
		/// 
		/// @return
		/// </summary>
		public virtual SchemaInfo BackwardsCompatibleJsonSchemaInfo
		{
			get
			{
				SchemaInfo backwardsCompatibleSchemaInfo;
				try
				{
					ObjectMapper objectMapper = new ObjectMapper();
					JsonSchemaGenerator schemaGen = new JsonSchemaGenerator(objectMapper);
					JsonSchema jsonBackwardsCompatibleSchema = schemaGen.generateSchema(pojo);
					backwardsCompatibleSchemaInfo = new SchemaInfo();
					backwardsCompatibleSchemaInfo.Name = "";
					backwardsCompatibleSchemaInfo.Properties = schemaInfo.Properties;
					backwardsCompatibleSchemaInfo.Type = SchemaType.JSON;
					backwardsCompatibleSchemaInfo.Schema = objectMapper.writeValueAsBytes(jsonBackwardsCompatibleSchema);
				}
				catch (JsonProcessingException ex)
				{
					throw new Exception(ex);
				}
				return backwardsCompatibleSchemaInfo;
			}
		}

		public static JSONSchema<T> Of<T>(ISchemaDefinition<T> schemaDefinition)
		{
			return new JSONSchema<T>(ParseSchemaInfo(schemaDefinition, SchemaType.JSON), schemaDefinition.Pojo);
		}

		public static JSONSchema<T> Of<T>(Type pojo)
		{
			return JSONSchema<T>.Of(ISchemaDefinition<T>.Builder().WithPojo(pojo).Build());
		}

		public static JSONSchema<T> Of<T>(Type pojo, IDictionary<string, string> properties)
		{
			return JSONSchema<T>.Of(ISchemaDefinition<T>.Builder().WithPojo(pojo).WithProperties(properties).Build());
		}

	}

}