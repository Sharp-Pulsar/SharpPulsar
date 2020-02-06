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
	using IGenericRecord = SharpPulsar.Api.Schema.IGenericRecord;
	using IGenericRecordBuilder = SharpPulsar.Api.Schema.IGenericRecordBuilder;

	/// <summary>
	/// Builder to build <seealso cref="SharpPulsar.api.schema.GenericRecord"/>.
	/// </summary>
	public class AvroRecordBuilderImpl : IGenericRecordBuilder
	{

		private readonly GenericSchemaImpl genericSchema;
		private readonly org.apache.avro.generic.GenericRecordBuilder avroRecordBuilder;

		public AvroRecordBuilderImpl(GenericSchemaImpl GenericSchema)
		{
			this.genericSchema = GenericSchema;
			this.avroRecordBuilder = new org.apache.avro.generic.GenericRecordBuilder(GenericSchema.AvroSchema);
		}

		/// <summary>
		/// Sets the value of a field.
		/// </summary>
		/// <param name="fieldName"> the name of the field to set. </param>
		/// <param name="value"> the value to set. </param>
		/// <returns> a reference to the RecordBuilder. </returns>
		public override IGenericRecordBuilder Set(string FieldName, object Value)
		{
			if (Value is IGenericRecord)
			{
				if (Value is GenericAvroRecord)
				{
					avroRecordBuilder.set(FieldName, ((GenericAvroRecord)Value).AvroRecord);
				}
				else
				{
					throw new System.ArgumentException("Avro Record Builder doesn't support non-avro record as a field");
				}
			}
			else
			{
				avroRecordBuilder.set(FieldName, Value);
			}
			return this;
		}

		/// <summary>
		/// Sets the value of a field.
		/// </summary>
		/// <param name="field"> the field to set. </param>
		/// <param name="value"> the value to set. </param>
		/// <returns> a reference to the RecordBuilder. </returns>
		public override IGenericRecordBuilder Set(Field Field, object Value)
		{
			set(Field.Index, Value);
			return this;
		}

		/// <summary>
		/// Sets the value of a field.
		/// </summary>
		/// <param name="index"> the field to set. </param>
		/// <param name="value"> the value to set. </param>
		/// <returns> a reference to the RecordBuilder. </returns>
		public virtual IGenericRecordBuilder Set(int Index, object Value)
		{
			if (Value is IGenericRecord)
			{
				if (Value is GenericAvroRecord)
				{
					avroRecordBuilder.set(genericSchema.AvroSchema.Fields.get(Index), ((GenericAvroRecord) Value).AvroRecord);
				}
				else
				{
					throw new System.ArgumentException("Avro Record Builder doesn't support non-avro record as a field");
				}
			}
			else
			{
				avroRecordBuilder.set(genericSchema.AvroSchema.Fields.get(Index), Value);
			}
			return this;
		}

		/// <summary>
		/// Clears the value of the given field.
		/// </summary>
		/// <param name="fieldName"> the name of the field to clear. </param>
		/// <returns> a reference to the RecordBuilder. </returns>
		public override IGenericRecordBuilder Clear(string FieldName)
		{
			avroRecordBuilder.clear(FieldName);
			return this;
		}

		/// <summary>
		/// Clears the value of the given field.
		/// </summary>
		/// <param name="field"> the field to clear. </param>
		/// <returns> a reference to the RecordBuilder. </returns>
		public override IGenericRecordBuilder Clear(Field Field)
		{
			return clear(Field.Index);
		}

		/// <summary>
		/// Clears the value of the given field.
		/// </summary>
		/// <param name="index"> the index of the field to clear. </param>
		/// <returns> a reference to the RecordBuilder. </returns>
		public virtual IGenericRecordBuilder Clear(int Index)
		{
			avroRecordBuilder.clear(genericSchema.AvroSchema.Fields.get(Index));
			return this;
		}

		public override IGenericRecord Build()
		{
			return new GenericAvroRecord(null, genericSchema.AvroSchema, genericSchema.Fields, avroRecordBuilder.build());
		}
	}

}