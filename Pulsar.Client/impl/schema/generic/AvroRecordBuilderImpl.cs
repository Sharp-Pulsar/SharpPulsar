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
	using GenericRecordBuilder = org.apache.pulsar.client.api.schema.GenericRecordBuilder;

	/// <summary>
	/// Builder to build <seealso cref="org.apache.pulsar.client.api.schema.GenericRecord"/>.
	/// </summary>
	internal class AvroRecordBuilderImpl : GenericRecordBuilder
	{

		private readonly GenericSchemaImpl genericSchema;
		private readonly org.apache.avro.generic.GenericRecordBuilder avroRecordBuilder;

		internal AvroRecordBuilderImpl(GenericSchemaImpl genericSchema)
		{
			this.genericSchema = genericSchema;
			this.avroRecordBuilder = new org.apache.avro.generic.GenericRecordBuilder(genericSchema.AvroSchema);
		}

		/// <summary>
		/// Sets the value of a field.
		/// </summary>
		/// <param name="fieldName"> the name of the field to set. </param>
		/// <param name="value"> the value to set. </param>
		/// <returns> a reference to the RecordBuilder. </returns>
		public override GenericRecordBuilder set(string fieldName, object value)
		{
			if (value is GenericRecord)
			{
				if (value is GenericAvroRecord)
				{
					avroRecordBuilder.set(fieldName, ((GenericAvroRecord)value).AvroRecord);
				}
				else
				{
					throw new System.ArgumentException("Avro Record Builder doesn't support non-avro record as a field");
				}
			}
			else
			{
				avroRecordBuilder.set(fieldName, value);
			}
			return this;
		}

		/// <summary>
		/// Sets the value of a field.
		/// </summary>
		/// <param name="field"> the field to set. </param>
		/// <param name="value"> the value to set. </param>
		/// <returns> a reference to the RecordBuilder. </returns>
		public override GenericRecordBuilder set(Field field, object value)
		{
			set(field.Index, value);
			return this;
		}

		/// <summary>
		/// Sets the value of a field.
		/// </summary>
		/// <param name="index"> the field to set. </param>
		/// <param name="value"> the value to set. </param>
		/// <returns> a reference to the RecordBuilder. </returns>
		protected internal virtual GenericRecordBuilder set(int index, object value)
		{
			if (value is GenericRecord)
			{
				if (value is GenericAvroRecord)
				{
					avroRecordBuilder.set(genericSchema.AvroSchema.Fields.get(index), ((GenericAvroRecord) value).AvroRecord);
				}
				else
				{
					throw new System.ArgumentException("Avro Record Builder doesn't support non-avro record as a field");
				}
			}
			else
			{
				avroRecordBuilder.set(genericSchema.AvroSchema.Fields.get(index), value);
			}
			return this;
		}

		/// <summary>
		/// Clears the value of the given field.
		/// </summary>
		/// <param name="fieldName"> the name of the field to clear. </param>
		/// <returns> a reference to the RecordBuilder. </returns>
		public override GenericRecordBuilder clear(string fieldName)
		{
			avroRecordBuilder.clear(fieldName);
			return this;
		}

		/// <summary>
		/// Clears the value of the given field.
		/// </summary>
		/// <param name="field"> the field to clear. </param>
		/// <returns> a reference to the RecordBuilder. </returns>
		public override GenericRecordBuilder clear(Field field)
		{
			return clear(field.Index);
		}

		/// <summary>
		/// Clears the value of the given field.
		/// </summary>
		/// <param name="index"> the index of the field to clear. </param>
		/// <returns> a reference to the RecordBuilder. </returns>
		protected internal virtual GenericRecordBuilder clear(int index)
		{
			avroRecordBuilder.clear(genericSchema.AvroSchema.Fields.get(index));
			return this;
		}

		public override GenericRecord build()
		{
			return new GenericAvroRecord(null, genericSchema.AvroSchema, genericSchema.Fields, avroRecordBuilder.build());
		}
	}

}