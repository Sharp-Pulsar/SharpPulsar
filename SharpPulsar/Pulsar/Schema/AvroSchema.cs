using System;
using System.Collections.Generic;
using SharpPulsar.Common.Schema;
using SharpPulsar.Impl.Schema.Generic;
using SharpPulsar.Protocol.Schema;
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
namespace SharpPulsar.Pulsar.Schema
{
	using Api;
	using SharpPulsar.Pulsar.Api.Schema;

	/// <summary>
	/// A schema implementation to deal with json data.
	/// </summary>
	public class AvroSchema : AvroBaseStructSchema
	{
		private object _classInstance;
		private AvroSchema(SchemaInfo schemaInfo) : base(schemaInfo)
		{
			SchemaInfo = schemaInfo;
		}
		private AvroSchema(ISchemaReader reader, ISchemaWriter writer, SchemaInfo schemaInfo) : base(schemaInfo)
		{
			Reader = reader;
			Writer = writer;
		}

		public override ISchemaInfo SchemaInfo { get; }

		public static AvroSchema Of(ISchemaDefinition schemaDefinition)
		{
			if(schemaDefinition.SchemaReaderOpt.HasValue && schemaDefinition.SchemaWriterOpt.HasValue)
				return new AvroSchema(schemaDefinition.SchemaReaderOpt.Value, schemaDefinition.SchemaWriterOpt.Value, SchemaUtils.ParseSchemaInfo(schemaDefinition, SchemaType.Avro));
			
			return new AvroSchema(SchemaUtils.ParseSchemaInfo(schemaDefinition, SchemaType.Avro));
		}

		public static AvroSchema Of(Type pojo)
		{
			return Of(ISchemaDefinition.Builder().WithPojo(pojo).Build());
		}

		public static AvroSchema Of(Type pojo, IDictionary<string, string> properties)
		{
			return Of(ISchemaDefinition.Builder().WithPojo(pojo).WithProperties(properties).Build());
		}

	}

}