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
	using Slf4j = lombok.@extern.slf4j.Slf4j;
	using Utf8 = org.apache.avro.util.Utf8;
	using Field = SharpPulsar.Api.Schema.Field;

	/// <summary>
	/// A generic avro record.
	/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Slf4j public class GenericAvroRecord extends VersionedGenericRecord
	public class GenericAvroRecord : VersionedGenericRecord
	{

		private readonly org.apache.avro.Schema schema;
		public virtual AvroRecord {get;}

		public GenericAvroRecord(sbyte[] SchemaVersion, org.apache.avro.Schema Schema, IList<Field> Fields, org.apache.avro.generic.GenericRecord Record) : base(SchemaVersion, Fields)
		{
			this.schema = Schema;
			this.AvroRecord = Record;
		}

		public override object GetField(string FieldName)
		{
			object Value = AvroRecord.get(FieldName);
			if (Value is Utf8)
			{
				return ((Utf8) Value).ToString();
			}
			else if (Value is org.apache.avro.generic.GenericRecord)
			{
				org.apache.avro.generic.GenericRecord AvroRecord = (org.apache.avro.generic.GenericRecord) Value;
				org.apache.avro.Schema RecordSchema = AvroRecord.Schema;
				IList<Field> Fields = RecordSchema.Fields.Select(f => new Field(f.name(), f.pos())).ToList();
				return new GenericAvroRecord(SchemaVersionConflict, schema, Fields, AvroRecord);
			}
			else
			{
				return Value;
			}
		}


	}

}