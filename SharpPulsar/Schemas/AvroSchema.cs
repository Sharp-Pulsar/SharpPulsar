using System;
using System.Collections.Generic;
using SharpPulsar.Interfaces.ISchema;
using SharpPulsar.Shared;

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
	public class AvroSchema<T> : AvroBaseStructSchema<T>
	{
		private object _classInstance = typeof(T).IsClass;
		private AvroSchema(ISchemaInfo schemaInfo) : base(schemaInfo)
		{
			SchemaInfo = schemaInfo;
		}
		private AvroSchema(ISchemaReader<T> reader, ISchemaWriter<T> writer, ISchemaInfo schemaInfo) : base(schemaInfo)
		{
			Reader = reader;
			Writer = writer;
		}

		public override ISchemaInfo SchemaInfo { get; }

		public static AvroSchema<T> Of(ISchemaDefinition<T> schemaDefinition)
		{
			if(schemaDefinition.SchemaReaderOpt.HasValue && schemaDefinition.SchemaWriterOpt.HasValue)
				return new AvroSchema<T>(schemaDefinition.SchemaReaderOpt.Value, schemaDefinition.SchemaWriterOpt.Value, SchemaUtils.ParseSchemaInfo(schemaDefinition, SchemaType.AVRO));
			
			return new AvroSchema<T>(SchemaUtils.ParseSchemaInfo(schemaDefinition, SchemaType.AVRO));
		}

		public static AvroSchema<T> Of(Type pojo)
		{
			return Of(ISchemaDefinition<T>.Builder().WithPojo(pojo).Build());
		}

		public static AvroSchema<T> Of(Type pojo, IDictionary<string, string> properties)
		{
			return Of(ISchemaDefinition<T>.Builder().WithPojo(pojo).WithProperties(properties).Build());
		}

	}

}