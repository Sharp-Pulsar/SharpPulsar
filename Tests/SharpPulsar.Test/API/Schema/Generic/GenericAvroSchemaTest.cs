using Avro.Generic;
using SharpPulsar.Interfaces.ISchema;
using SharpPulsar.Schemas;
using SharpPulsar.Schemas.Generic;
using SharpPulsar.Test.API;
using SharpPulsar.Test.Fixture;
using Xunit;
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
namespace SharpPulsar.Test.API.Schema.Generic
{
    [Collection(nameof(IntegrationCollection))]
    public class GenericAvroSchemaTest
    {

        private GenericAvroSchema writerSchema;
        private GenericAvroSchema readerSchema;
        public GenericAvroSchemaTest()
        {
            var avroFooV2Schema = AvroSchema<SchemaTestUtils.FooV2>.Of(ISchemaDefinition<SchemaTestUtils.FooV2>.Builder().WithAlwaysAllowNull(false).WithPojo(typeof(SchemaTestUtils.FooV2)).Build());
            //var avroFooSchema = AvroSchema<SchemaTestUtils.Foo>.Of(ISchemaDefinition<SchemaTestUtils.Foo>.Builder().WithAlwaysAllowNull(false).WithPojo(typeof(SchemaTestUtils.Foo)).Build());
            writerSchema = new GenericAvroSchema(avroFooV2Schema.SchemaInfo);
            readerSchema = new GenericAvroSchema(avroFooV2Schema.SchemaInfo);
        }
        [Fact]
        public virtual void TestSupportMultiVersioningSupportByDefault()
        {
            Assert.True(writerSchema.SupportSchemaVersioning());
            Assert.True(readerSchema.SupportSchemaVersioning());
        }
        [Fact]
        public virtual void TestFailDecodeWithoutMultiVersioningSupport()
        {
            var dataForWriter = new GenericRecord((Avro.RecordSchema)writerSchema.AvroSchema);
            dataForWriter.Add("Field1", SchemaTestUtils.TestMultiVersionSchemaString);
            dataForWriter.Add("Field3", 0);
            var fields = writerSchema.Fields;
            readerSchema.Decode(writerSchema.Encode(new GenericAvroRecord(null, writerSchema.AvroSchema, fields, dataForWriter)));
        }

        [Fact]
        public virtual void TestDecodeWithMultiVersioningSupport()
        {
            var dataForWriter = new GenericRecord((Avro.RecordSchema)writerSchema.AvroSchema);
            dataForWriter.Add("Field1", SchemaTestUtils.TestMultiVersionSchemaString);
            dataForWriter.Add("Field3", 0);
            var fields = writerSchema.Fields;
            var ec = writerSchema.Encode(new GenericAvroRecord(null, writerSchema.AvroSchema, fields, dataForWriter));
            var record = readerSchema.Decode(ec, new byte[10]); ;
            Assert.Equal(SchemaTestUtils.TestMultiVersionSchemaString, record.GetField("Field1"));
            Assert.Equal(0, record.GetField("Field3"));
        }
    }
}
