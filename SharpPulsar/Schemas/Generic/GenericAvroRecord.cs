using System.Collections.Generic;
using System.Linq;
using SharpPulsar.Interfaces.Schema;
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
namespace SharpPulsar.Schemas.Generic
{
    /// <summary>
    /// A generic avro record.
    /// </summary>
    public class GenericAvroRecord : VersionedGenericRecord
    {
        private readonly Avro.Schema _schema;
        private readonly Avro.Generic.GenericRecord _record;

        public GenericAvroRecord(byte[] schemaVersion, Avro.Schema schema, IList<Field> fields, Avro.Generic.GenericRecord record) : base(schemaVersion, fields)
        {
            _schema = schema;
            _record = record;
        }

        public override object GetField(int pos)
        {
            var value = _record.GetValue(pos);
            //I am not sure how to port this
            /*if (value is Utf8)
            {
                return ((Utf8)value).ToString();
            }*/

            if (value is string utf8)
            {
                return utf8;
            }
            else if (value is Avro.Generic.GenericRecord avroRecord)
            {
                var recordSchema = avroRecord.Schema;
                IList<Field> fields = recordSchema.Fields.Select(f => new Field(f.Name, f.Pos)).ToList();
                return new GenericAvroRecord(SchemaVersion, _schema, fields, avroRecord);
            }
            else
            {
                return value;
            }
        }
        public override object GetField(string fieldName)
        {
            var value = _record[fieldName];
            //I am not sure how to port this
            /*if (value is Utf8)
            {
                return ((Utf8)value).ToString();
            }*/

            if (value is string utf8)
            {
                return utf8;
            }
            else if (value is Avro.Generic.GenericRecord avroRecord)
            {
                var recordSchema = avroRecord.Schema;
                IList<Field> fields = recordSchema.Fields.Select(f => new Field(f.Name, f.Pos)).ToList();
                return new GenericAvroRecord(SchemaVersion, _schema, fields, avroRecord);
            }
            else
            {
                return value;
            }
        }

        public virtual Avro.Generic.GenericRecord AvroRecord
        {
            get
            {
                return _record;
            }
        }

        public object NativeObject
        {
            get
            {
                return _record;
            }
        }

        public SchemaType SchemaType
        {
            get
            {
                return SchemaType.AVRO;
            }
        }
    }
}
