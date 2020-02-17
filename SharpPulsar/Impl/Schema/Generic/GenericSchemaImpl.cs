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

	using Field = Api.Schema.Field;
	using IGenericRecord = Api.Schema.IGenericRecord;
	using SharpPulsar.Api.Schema;
	using Schema;

	/// <summary>
	/// A generic schema representation.
	/// </summary>
	public abstract class GenericSchemaImpl : StructSchema<IGenericRecord>, IGenericSchema<IGenericRecord>
	{
		public abstract IGenericRecordBuilder NewRecordBuilder();
		public abstract override ISchema<sbyte[]> AUTO_PRODUCE_BYTES<T>(ISchema<T> schema);
		public abstract override ISchema<sbyte[]> AutoProduceBytes();
		public abstract override ISchema<IGenericRecord> AutoConsume();
		public abstract override ISchema<IGenericRecord> Auto();
		
        public abstract ISchema<T> Json<T>(T pojo);
        public abstract override bool RequireFetchingSchemaInfo();
		public abstract override bool SupportSchemaVersioning();
		public abstract override void Validate(sbyte[] message);


        // the flag controls whether to use the provided schema as reader schema
		// to decode the messages. In `AUTO_CONSUME` mode, setting this flag to `false`
		// allows decoding the messages using the schema associated with the messages.
		protected internal readonly bool UseProvidedSchemaAsReaderSchema;

        protected GenericSchemaImpl(SchemaInfo schemaInfo, bool useProvidedSchemaAsReaderSchema) : base(schemaInfo)
		{

			Fields = ((RecordSchema)Schema).Fields.Select(f => new Field() { Name = f.Name, Index = f.Pos}).ToList();
			UseProvidedSchemaAsReaderSchema = useProvidedSchemaAsReaderSchema;
		}

		public  IList<Field> Fields { get; }

		//IList<Field> IGenericSchema<IGenericRecord>.Fields => throw new NotImplementedException();

		/// <summary>
		/// Create a generic schema out of a <tt>SchemaInfo</tt>.
		/// </summary>
		/// <param name="schemaInfo"> schema info </param>
		/// <returns> a generic schema instance </returns>
		public static GenericSchemaImpl Of(SchemaInfo schemaInfo)
		{
			return Of(schemaInfo, true);
		}

		public static GenericSchemaImpl Of(SchemaInfo schemaInfo, bool useProvidedSchemaAsReaderSchema)
        {
            var ty = schemaInfo.Type;
			switch (ty.Value)
			{
				
				case 2:
					return new GenericJsonSchema(schemaInfo, useProvidedSchemaAsReaderSchema);
				default:
					throw new NotSupportedException("Generic schema is not supported on schema type " + schemaInfo.Type + "'");
			}
		}

		IGenericRecordBuilder IGenericSchema<IGenericRecord>.NewRecordBuilder()
		{
			throw new NotImplementedException();
		}
	}

}