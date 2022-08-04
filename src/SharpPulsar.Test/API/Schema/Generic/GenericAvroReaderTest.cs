using System.IO;
using SharpPulsar.Interfaces.ISchema;
using SharpPulsar.Schemas;
using SharpPulsar.Schemas.Generic;
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

    public class GenericAvroReaderTest
    {

        private readonly SchemaTestUtils.Foo foo;
        private readonly SchemaTestUtils.FooV2 fooV2;
        private readonly AvroSchema<SchemaTestUtils.Foo> fooSchemaNotNull;
        private readonly AvroSchema<SchemaTestUtils.Foo> fooSchema;
        private readonly AvroSchema<SchemaTestUtils.FooV2> fooV2Schema;
        private readonly AvroSchema<SchemaTestUtils.Foo> fooOffsetSchema;
        public GenericAvroReaderTest()
        {
            fooSchema = AvroSchema<SchemaTestUtils.Foo>.Of(typeof(SchemaTestUtils.Foo));

            fooV2Schema = AvroSchema<SchemaTestUtils.FooV2>.Of(typeof(SchemaTestUtils.FooV2));
            fooSchemaNotNull = AvroSchema<SchemaTestUtils.Foo>.Of(ISchemaDefinition<SchemaTestUtils.Foo>.Builder().WithAlwaysAllowNull(false).WithPojo(typeof(SchemaTestUtils.Foo)).Build());

            fooOffsetSchema = AvroSchema<SchemaTestUtils.Foo>.Of(typeof(SchemaTestUtils.Foo));
            fooOffsetSchema.SchemaInfo.Properties.Add(GenericAvroSchema.OFFSET_PROP, "5");
            var prop = fooOffsetSchema.AvroSchema.GetProperty(GenericAvroSchema.OFFSET_PROP);
            foo = new SchemaTestUtils.Foo
            {
                Field1 = "foo1",
                Field2 = "bar1",
                Field4 = new SchemaTestUtils.Bar(),
                FieldUnableNull = "notNull"
            };

            fooV2 = new SchemaTestUtils.FooV2
            {
                Field1 = "foo1",
                Field3 = 10
            };

        }
        [Fact]
        public virtual void TestGenericAvroReaderByWriterSchema()
        {
            var fooBytes = fooSchema.Encode(foo);

            var genericAvroSchemaByWriterSchema = new GenericAvroReader(fooSchema.AvroSchema);
            var genericRecordByWriterSchema = genericAvroSchemaByWriterSchema.Read(new MemoryStream(fooBytes));
            Assert.Equal("foo1", genericRecordByWriterSchema.GetField("Field1"));
            Assert.Equal("bar1", genericRecordByWriterSchema.GetField("Field2"));
            Assert.Equal("notNull", genericRecordByWriterSchema.GetField("FieldUnableNull"));
        }
        [Fact]
        public virtual void TestGenericAvroReaderByReaderSchema()
        {
            var fooV2Bytes = fooV2Schema.Encode(fooV2);

            var genericAvroSchemaByReaderSchema = new GenericAvroReader(fooV2Schema.AvroSchema, fooV2Schema.AvroSchema, new byte[10]);
            var genericRecordByReaderSchema = genericAvroSchemaByReaderSchema.Read(new MemoryStream(fooV2Bytes));
            Assert.Equal("foo1", genericRecordByReaderSchema.GetField("Field1"));
            Assert.Equal(10, genericRecordByReaderSchema.GetField("Field3"));
        }

    }
}
