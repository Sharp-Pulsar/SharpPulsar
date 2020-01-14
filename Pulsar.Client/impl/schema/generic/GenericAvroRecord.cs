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
	using Slf4j = lombok.@extern.slf4j.Slf4j;
	using Utf8 = org.apache.avro.util.Utf8;
	using Field = org.apache.pulsar.client.api.schema.Field;

	/// <summary>
	/// A generic avro record.
	/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Slf4j public class GenericAvroRecord extends VersionedGenericRecord
	public class GenericAvroRecord : VersionedGenericRecord
	{

		private readonly org.apache.avro.Schema schema;
		private readonly org.apache.avro.generic.GenericRecord record;

		public GenericAvroRecord(sbyte[] schemaVersion, org.apache.avro.Schema schema, IList<Field> fields, org.apache.avro.generic.GenericRecord record) : base(schemaVersion, fields)
		{
			this.schema = schema;
			this.record = record;
		}

		public override object getField(string fieldName)
		{
			object value = record.get(fieldName);
			if (value is Utf8)
			{
				return ((Utf8) value).ToString();
			}
			else if (value is org.apache.avro.generic.GenericRecord)
			{
				org.apache.avro.generic.GenericRecord avroRecord = (org.apache.avro.generic.GenericRecord) value;
				org.apache.avro.Schema recordSchema = avroRecord.Schema;
				IList<Field> fields = recordSchema.Fields.Select(f => new Field(f.name(), f.pos())).ToList();
				return new GenericAvroRecord(schemaVersion, schema, fields, avroRecord);
			}
			else
			{
				return value;
			}
		}

		public virtual org.apache.avro.generic.GenericRecord AvroRecord
		{
			get
			{
				return record;
			}
		}

	}

}