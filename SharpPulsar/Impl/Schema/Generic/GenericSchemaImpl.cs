using SharpPulsar.Api;
using Org.Apache.Pulsar.Common.Schema;

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
namespace SharpPulsar.Impl.Schema.Generic
{

	using Field = SharpPulsar.Api.Schema.Field;
	using GenericRecord = SharpPulsar.Api.Schema.GenericRecord;
	using SharpPulsar.Api.Schema;
	using SharpPulsar.Impl.Schema;
	using SchemaInfo = Org.Apache.Pulsar.Common.Schema.SchemaInfo;

	/// <summary>
	/// A generic schema representation.
	/// </summary>
	public abstract class GenericSchemaImpl : StructSchema<GenericRecord>, GenericSchema<GenericRecord>
	{
		public abstract GenericRecordBuilder NewRecordBuilder();
		public override abstract GenericSchema<GenericRecord> Generic(SchemaInfo SchemaInfo);
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: public abstract Schema<JavaToDotNetGenericWildcard> getSchema(org.apache.pulsar.common.schema.SchemaInfo schemaInfo);
		public override abstract Schema<object> GetSchema(SchemaInfo SchemaInfo);
		public override abstract Schema<sbyte[]> AUTO_PRODUCE_BYTES<T1>(Schema<T1> Schema);
		public override abstract Schema<sbyte[]> AutoProduceBytes();
		public override abstract Schema<GenericRecord> AutoConsume();
		public override abstract Schema<GenericRecord> AUTO();
		public override abstract Schema<KeyValue<K, V>> KeyValue(Schema<K> Key, Schema<V> Value, KeyValueEncodingType KeyValueEncodingType);
		public override abstract Schema<KeyValue<K, V>> KeyValue(Schema<K> Key, Schema<V> Value);
		public override abstract Schema<KeyValue<K, V>> KeyValue(Type Key, Type Value);
		public override abstract Schema<KeyValue<sbyte[], sbyte[]>> KvBytes();
		public override abstract Schema<KeyValue<K, V>> KeyValue(Type Key, Type Value, SchemaType Type);
		public override abstract Schema<T> JSON(SchemaDefinition SchemaDefinition);
		public override abstract Schema<T> JSON(Type Pojo);
		public override abstract Schema<T> AVRO(SchemaDefinition<T> SchemaDefinition);
		public override abstract Schema<T> AVRO(Type Pojo);
		public override abstract Schema<T> PROTOBUF(SchemaDefinition<T> SchemaDefinition);
		public override abstract Schema<T> PROTOBUF(Type Clazz);
		public override abstract void ConfigureSchemaInfo(string Topic, string ComponentName, SchemaInfo SchemaInfo);
		public override abstract bool RequireFetchingSchemaInfo();
		public override abstract bool SupportSchemaVersioning();
		public override abstract void Validate(sbyte[] Message);

//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		protected internal readonly IList<Field> FieldsConflict;
		// the flag controls whether to use the provided schema as reader schema
		// to decode the messages. In `AUTO_CONSUME` mode, setting this flag to `false`
		// allows decoding the messages using the schema associated with the messages.
		protected internal readonly bool UseProvidedSchemaAsReaderSchema;

		public GenericSchemaImpl(SchemaInfo SchemaInfo, bool UseProvidedSchemaAsReaderSchema) : base(SchemaInfo)
		{

			this.FieldsConflict = Schema.Fields.Select(f => new Field(f.name(), f.pos())).ToList();
			this.UseProvidedSchemaAsReaderSchema = UseProvidedSchemaAsReaderSchema;
		}

		public virtual IList<Field> Fields
		{
			get
			{
				return FieldsConflict;
			}
		}

		/// <summary>
		/// Create a generic schema out of a <tt>SchemaInfo</tt>.
		/// </summary>
		/// <param name="schemaInfo"> schema info </param>
		/// <returns> a generic schema instance </returns>
		public static GenericSchemaImpl Of(SchemaInfo SchemaInfo)
		{
			return Of(SchemaInfo, true);
		}

		public static GenericSchemaImpl Of(SchemaInfo SchemaInfo, bool UseProvidedSchemaAsReaderSchema)
		{
			switch (SchemaInfo.Type)
			{
				case AVRO:
					return new GenericAvroSchema(SchemaInfo, UseProvidedSchemaAsReaderSchema);
				case JSON:
					return new GenericJsonSchema(SchemaInfo, UseProvidedSchemaAsReaderSchema);
				default:
					throw new System.NotSupportedException("Generic schema is not supported on schema type " + SchemaInfo.Type + "'");
			}
		}

	}

}