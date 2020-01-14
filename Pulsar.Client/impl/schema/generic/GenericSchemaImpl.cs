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
namespace org.apache.pulsar.client.impl.schema.generic
{

	using Field = org.apache.pulsar.client.api.schema.Field;
	using GenericRecord = org.apache.pulsar.client.api.schema.GenericRecord;
	using GenericSchema = org.apache.pulsar.client.api.schema.GenericSchema;
	using org.apache.pulsar.client.impl.schema;
	using SchemaInfo = org.apache.pulsar.common.schema.SchemaInfo;

	/// <summary>
	/// A generic schema representation.
	/// </summary>
	public abstract class GenericSchemaImpl : StructSchema<GenericRecord>, GenericSchema<GenericRecord>
	{

		protected internal readonly IList<Field> fields;
		// the flag controls whether to use the provided schema as reader schema
		// to decode the messages. In `AUTO_CONSUME` mode, setting this flag to `false`
		// allows decoding the messages using the schema associated with the messages.
		protected internal readonly bool useProvidedSchemaAsReaderSchema;

		protected internal GenericSchemaImpl(SchemaInfo schemaInfo, bool useProvidedSchemaAsReaderSchema) : base(schemaInfo)
		{

			this.fields = schema.Fields.Select(f => new Field(f.name(), f.pos())).ToList();
			this.useProvidedSchemaAsReaderSchema = useProvidedSchemaAsReaderSchema;
		}

		public override IList<Field> Fields
		{
			get
			{
				return fields;
			}
		}

		/// <summary>
		/// Create a generic schema out of a <tt>SchemaInfo</tt>.
		/// </summary>
		/// <param name="schemaInfo"> schema info </param>
		/// <returns> a generic schema instance </returns>
		public static GenericSchemaImpl of(SchemaInfo schemaInfo)
		{
			return of(schemaInfo, true);
		}

		public static GenericSchemaImpl of(SchemaInfo schemaInfo, bool useProvidedSchemaAsReaderSchema)
		{
			switch (schemaInfo.Type)
			{
				case AVRO:
					return new GenericAvroSchema(schemaInfo, useProvidedSchemaAsReaderSchema);
				case JSON:
					return new GenericJsonSchema(schemaInfo, useProvidedSchemaAsReaderSchema);
				default:
					throw new System.NotSupportedException("Generic schema is not supported on schema type " + schemaInfo.Type + "'");
			}
		}

	}

}