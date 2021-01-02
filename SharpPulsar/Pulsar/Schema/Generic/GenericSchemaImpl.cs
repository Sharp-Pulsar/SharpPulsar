using SharpPulsar.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using Avro;
using SharpPulsar.Common.Schema;

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
namespace SharpPulsar.Impl.Schema.Generic
{
    using SharpPulsar.Pulsar.Api.Schema;
	using Schema;
    using SharpPulsar.Pulsar.Schema;
    using SharpPulsar.Shared;

    /// <summary>
    /// A generic schema representation for AvroBasedGenericSchema .
    /// warning :
    /// we suggest migrate GenericSchemaImpl.of() to  <GenericSchema Implementor>.of() method (e.g. GenericJsonSchema ã€�GenericAvroSchema )
    /// </summary>
    public abstract class GenericSchemaImpl : AvroBaseStructSchema<IGenericRecord>, IGenericSchema<IGenericRecord>
	{
		public abstract IGenericRecordBuilder NewRecordBuilder();
		protected internal readonly IList<Field> fields;

		protected internal GenericSchemaImpl(SchemaInfo schemaInfo) : base(schemaInfo)
		{
			fields = ((RecordSchema)schema).Fields.Select(f => new Field() { Name = f.Name, Index = f.Pos }).ToList();
		}

		public IList<Field> Fields
		{
			get
			{
				return fields;
			}
		}

		/// <summary>
		/// Create a generic schema out of a <tt>SchemaInfo</tt>.
		///  warning : we suggest migrate GenericSchemaImpl.of() to  <GenericSchema Implementor>.of() method (e.g. GenericJsonSchema ã€�GenericAvroSchema ) </summary>
		/// <param name="schemaInfo"> schema info </param>
		/// <returns> a generic schema instance </returns>
		public static GenericSchemaImpl Of(SchemaInfo schemaInfo)
		{
			return Of(schemaInfo, true);
		}

		/// <summary>
		/// warning :
		/// we suggest migrate GenericSchemaImpl.of() to  <GenericSchema Implementor>.of() method (e.g. GenericJsonSchema ã€�GenericAvroSchema ) </summary>
		/// <param name="schemaInfo"> <seealso cref="SchemaInfo"/> </param>
		/// <param name="useProvidedSchemaAsReaderSchema"> <seealso cref="Boolean"/> </param>
		/// <returns> generic schema implementation </returns>
		public static GenericSchemaImpl Of(SchemaInfo schemaInfo, bool useProvidedSchemaAsReaderSchema)
		{
			switch (schemaInfo.Type.InnerEnumValue)
			{
				case SchemaType.InnerEnum.Avro:
					return new GenericAvroSchema(schemaInfo, useProvidedSchemaAsReaderSchema);
				default:
					throw new System.NotSupportedException("Generic schema is not supported on schema type " + schemaInfo.Type + "'");
			}
		}
	}
}

