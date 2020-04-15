using System;
using System.Collections.Generic;
using System.IO;
using Avro.IO;
using Avro.Schemas;
using Avro.Reflect;
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
namespace SharpPulsar.Impl.Schema
{
    using Api;
	using SharpPulsar.Api.Schema;

    /// <summary>
	/// A schema implementation to deal with json data.
	/// </summary>
	public class JsonSchema : StructSchema
	{

        private JsonSchema(SchemaInfo schemaInfo) : base(schemaInfo)
        {
            SchemaInfo = schemaInfo;
        }

		public override GenericAvroReader LoadReader(BytesSchemaVersion schemaVersion)
		{
			throw new System.Exception("JSONSchema don't support schema versioning");
		}


		public override ISchemaInfo SchemaInfo { get; }

        public static JsonSchema Of(ISchemaDefinition schemaDefinition)
		{
			return new JsonSchema(ParseSchemaInfo(schemaDefinition, SchemaType.Avro));
		}

		public static JsonSchema Of(Type pojo)
		{
			return Of(ISchemaDefinition.Builder().WithPojo(pojo).Build());
		}

		public static JsonSchema Of(Type pojo, IDictionary<string, string> properties)
		{
			return Of(ISchemaDefinition.Builder().WithPojo(pojo).WithProperties(properties).Build());
		}
        
		public override ISchema Auto()
		{
			throw new NotImplementedException();
		}

		public override ISchema Json(ISchemaDefinition schemaDefinition)
		{
			throw new NotImplementedException();
		}

        public override ISchema Json(object pojo)
        {
            throw new NotImplementedException();
        }


        public override void ConfigureSchemaInfo(string topic, string componentName, SchemaInfo schemaInfo)
		{
			throw new NotImplementedException();
		}

		public override bool RequireFetchingSchemaInfo()
        {
            return true;
        }

		public override bool SupportSchemaVersioning()
		{
			return true;
		}

		public override void Validate(sbyte[] message, Type returnType)
		{
			throw new NotImplementedException();
		}
        
		public override object Decode(byte[] byteBuf, Type returnType)
		{
            if (Reader == null)
                Reader = new GenericAvroReader(Schema, new sbyte[] { 0 });
            return Reader.Read(byteBuf, returnType);
		}

	}

}