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
    using Avro.Generic;
    using SharpPulsar.Entity;
    using SharpPulsar.Interface.Schema;

	/// <summary>
	/// Builder to build <seealso cref="org.apache.pulsar.client.api.schema.GenericRecord"/>.
	/// </summary>
	internal class AvroRecordBuilderImpl : IGenericRecordBuilder
	{

		private readonly GenericSchemaImpl genericSchema;
		private readonly IGenericRecordBuilder avroRecordBuilder;

		internal AvroRecordBuilderImpl(GenericSchemaImpl genericSchema)
		{
			this.genericSchema = genericSchema;
			this.avroRecordBuilder = GenericRecordBuilder(genericSchema.AvroSchema);
		}

		/// <summary>
		/// Sets the value of a field.
		/// </summary>
		/// <param name="fieldName"> the name of the field to set. </param>
		/// <param name="value"> the value to set. </param>
		/// <returns> a reference to the RecordBuilder. </returns>
		public IGenericRecordBuilder Set(string fieldName, object value)
		{
			if (value is GenericRecord)
			{
				if (value is GenericAvroRecord)
				{
					avroRecordBuilder.Set(fieldName, ((GenericAvroRecord)value).AvroRecord);
				}
				else
				{
					throw new System.ArgumentException("Avro Record Builder doesn't support non-avro record as a field");
				}
			}
			else
			{
				avroRecordBuilder.Set(fieldName, value);
			}
			return this;
		}

		/// <summary>
		/// Sets the value of a field.
		/// </summary>
		/// <param name="field"> the field to set. </param>
		/// <param name="value"> the value to set. </param>
		/// <returns> a reference to the RecordBuilder. </returns>
		public IGenericRecordBuilder Set(Field field, object value)
		{
			Set(field.Index, value);
			return this;
		}

		/// <summary>
		/// Sets the value of a field.
		/// </summary>
		/// <param name="index"> the field to set. </param>
		/// <param name="value"> the value to set. </param>
		/// <returns> a reference to the RecordBuilder. </returns>
		protected internal virtual IGenericRecordBuilder Set(int index, object value)
		{
			if (value is GenericRecord)
			{
				if (value is GenericAvroRecord)
				{
					avroRecordBuilder.Set(genericSchema.AvroSchema.Fields.get(index), ((GenericAvroRecord) value).AvroRecord);
				}
				else
				{
					throw new System.ArgumentException("Avro Record Builder doesn't support non-avro record as a field");
				}
			}
			else
			{
				avroRecordBuilder.Set(genericSchema.AvroSchema.Fields.get(index), value);
			}
			return this;
		}

		/// <summary>
		/// Clears the value of the given field.
		/// </summary>
		/// <param name="fieldName"> the name of the field to clear. </param>
		/// <returns> a reference to the RecordBuilder. </returns>
		public IGenericRecordBuilder Clear(string fieldName)
		{
			avroRecordBuilder.Clear(fieldName);
			return this;
		}

		/// <summary>
		/// Clears the value of the given field.
		/// </summary>
		/// <param name="field"> the field to clear. </param>
		/// <returns> a reference to the RecordBuilder. </returns>
		public IGenericRecordBuilder Clear(Field field)
		{
			return Clear(field.Index);
		}

		/// <summary>
		/// Clears the value of the given field.
		/// </summary>
		/// <param name="index"> the index of the field to clear. </param>
		/// <returns> a reference to the RecordBuilder. </returns>
		protected internal virtual IGenericRecordBuilder Clear(int index)
		{
			avroRecordBuilder.Clear(genericSchema.AvroSchema.Fields.get(index));
			return this;
		}

		public IGenericRecord Build()
		{
			return new GenericAvroRecord(null, genericSchema.AvroSchema, genericSchema.Fields, avroRecordBuilder.Build());
		}

	}

}