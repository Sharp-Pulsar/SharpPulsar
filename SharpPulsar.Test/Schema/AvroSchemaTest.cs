using AvroSchemaGenerator;
using NodaTime;
using SharpPulsar.Extension;
using SharpPulsar.Interfaces.ISchema;
using SharpPulsar.Schemas;
using SharpPulsar.Schemas.Reader;
using SharpPulsar.Schemas.Writer;
using System;
using System.Text;
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
namespace SharpPulsar.Test.Schema
{
    public class AvroSchemaTest
    {
        private class DefaultStruct
        {
            public int Field1 { get; set; }
            public string Field2 { get; set; }
            public long? Field3 { get; set; }
        }

        private class StructWithAnnotations
        {
            public int Field1 { get; set; }
            public string Field2 { get; set; }
            public long? Field3 { get; set; }
        }

        private class SchemaLogicalType
        {
            public double Decimal { get; set; }
            public long Date { get; set; }
            public long TimestampMillis { get; set; }
            public long TimeMillis { get; set; }
            public long TimestampMicros { get; set; }
            public long TimeMicros { get; set; }
        }

        [Fact]
        public virtual void TestSchemaDefinition()
        {
            var schema1 = typeof(DefaultStruct).GetSchema();
            var schema2 = AvroSchema<StructWithAnnotations>.Of(typeof(StructWithAnnotations));

            string schemaDef1 = schema1.ToString();
            string schemaDef2 = Encoding.UTF8.GetString(schema2.SchemaInfo.Schema.ToBytes());
            Assert.NotEqual(schemaDef1, schemaDef2);

            AvroSchema<StructWithAnnotations> schema3 = AvroSchema<StructWithAnnotations>.Of(ISchemaDefinition<StructWithAnnotations>.Builder().WithJsonDef(schemaDef1).Build());
            string schemaDef3 = Encoding.UTF8.GetString(schema3.SchemaInfo.Schema.ToBytes());
            Assert.True(schemaDef1.Contains("DefaultStruct") && schemaDef3.Contains("DefaultStruct"));
            Assert.NotEqual(schemaDef2, schemaDef3);

            StructWithAnnotations @struct = new StructWithAnnotations
            {
                Field1 = 5678
            };
            // schema2 is using the schema generated from POJO,
            // it allows field2 to be nullable, and field3 has default value.
            var e = schema2.Encode(@struct);
            // schema3 is using the schema passed in, which doesn't allow nullable
            var d = schema3.Encode(@struct);
            var f = e;
        }
        /// <summary>
        /// NodaType does not work with AvroSchemaGenerator
        /// </summary>
        [Fact]
        public void TestLogicalType()
        {
            AvroSchema<SchemaLogicalType> avroSchema = AvroSchema<SchemaLogicalType>.Of(ISchemaDefinition<SchemaLogicalType>.Builder().WithPojo(typeof(SchemaLogicalType)).WithJSR310ConversionEnabled(true).Build());
            
            SchemaLogicalType schemaLogicalType = new SchemaLogicalType
            {
                TimestampMicros = DateTimeHelper.CurrentUnixTimeMillis() * 1000,
                TimestampMillis = DateTime.Parse("2019-03-26T04:39:58.469Z").Ticks,
                Decimal = 12.34D,
                Date = DateTimeOffset.Now.ToUnixTimeMilliseconds(),
                TimeMicros = DateTimeHelper.CurrentUnixTimeMillis() * 1000,
                TimeMillis = (DateTime.Now - DateTime.Today).Ticks
            };

            sbyte[] bytes1 = avroSchema.Encode(schemaLogicalType);
            Assert.True(bytes1.Length > 0);

            SchemaLogicalType object1 = avroSchema.Decode(bytes1);

            Assert.Equal(schemaLogicalType.Decimal, object1.Decimal);
            Assert.Equal(0, schemaLogicalType.Date);

        }
        [Fact]
        public void TestAvroSchemaUserDefinedReadAndWriter()
        {
            var reader = new JsonReader<Foo>();
            var writer = new JsonWriter<Foo>();
            var schemaDefinition = ISchemaDefinition<Foo>.Builder().WithPojo(typeof(Bar)).WithSchemaReader(reader).WithSchemaWriter(writer).Build();

            AvroSchema<Foo> schema = AvroSchema<Foo>.Of(schemaDefinition);
            Foo foo = new Foo();
            foo.Color = Color.RED;
            string field1 = "test";
            foo.Field1 = field1;
            var encoded = schema.Encode(foo);
            foo = schema.Decode(encoded);
            Assert.Equal(Color.RED, foo.Color);
            Assert.Equal(field1, foo.Field1);
        }

    }
    public class Bar
    {

    }
    [Serializable]
    public class Foo
    {
        public Color Color { get; set; }
        public string Field1 { get; set; }

    }
    public enum Color
    {
        RED,
        BLUE,
        GREEN
    }
}