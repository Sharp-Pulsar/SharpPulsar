using Avro;
using Avro.Specific;
using AvroSchemaGenerator;
using AvroSchemaGenerator.Attributes;
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
    [Collection("SchemaSpec")]
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

        private class SchemaLogicalType: IEquatable<SchemaLogicalType>
        {
            public double Decimal { get; set; }
            public long Date { get; set; }
            public long TimestampMillis { get; set; }
            public long TimeMillis { get; set; }
            public long TimestampMicros { get; set; }
            public long TimeMicros { get; set; }

            public bool Equals(SchemaLogicalType other)
            {
                if (Decimal == other.Decimal && Date == other.Date && TimestampMillis == other.TimestampMillis
                    && TimeMillis == other.TimeMillis && TimestampMicros == other.TimestampMicros && TimeMicros == other.TimeMicros)
                    return true;
                return false;
            }
        }

        [Fact]
        public virtual void TestSchemaDefinition()
        {
            var schema1 = typeof(DefaultStruct).GetSchema();
            var schema2 = AvroSchema<StructWithAnnotations>.Of(typeof(StructWithAnnotations));

            string schemaDef1 = schema1.ToString();
            string schemaDef2 = Encoding.UTF8.GetString(schema2.SchemaInfo.Schema);
            Assert.NotEqual(schemaDef1, schemaDef2);

            AvroSchema<StructWithAnnotations> schema3 = AvroSchema<StructWithAnnotations>.Of(ISchemaDefinition<StructWithAnnotations>.Builder().WithJsonDef(schemaDef1).Build());
            string schemaDef3 = Encoding.UTF8.GetString(schema3.SchemaInfo.Schema);
            Assert.True(schemaDef1.Contains("DefaultStruct") && schemaDef3.Contains("DefaultStruct"));
            Assert.NotEqual(schemaDef2, schemaDef3);

            StructWithAnnotations @struct = new StructWithAnnotations
            {
                Field1 = 5678
            };
            // schema2 is using the schema generated from POJO,
            // it allows field2 to be nullable, and field3 has default value.
            schema2.Encode(@struct);
            // schema3 is using the schema passed in, which doesn't allow nullable
            schema3.Encode(@struct);
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

            byte[] bytes1 = avroSchema.Encode(schemaLogicalType);
            Assert.True(bytes1.Length > 0);

            SchemaLogicalType object1 = avroSchema.Decode(bytes1);

            Assert.True(schemaLogicalType.Equals(object1));

        }
        [Fact]
        public void TestDateTimeDecimalLogicalType()
        {
            AvroSchema<LogicalMessage> avroSchema = AvroSchema<LogicalMessage>.Of(ISchemaDefinition<LogicalMessage>.Builder().WithPojo(typeof(LogicalMessage)).WithJSR310ConversionEnabled(true).Build());

            var logMsg = new LogicalMessage { Schema = Avro.Schema.Parse(avroSchema.SchemaInfo.SchemaDefinition), CreatedTime = DateTime.Now, DayOfWeek = "Saturday", Size = new AvroDecimal(102.65M) };

            byte[] bytes1 = avroSchema.Encode(logMsg);
            Assert.True(bytes1.Length > 0);

            var msg = avroSchema.Decode(bytes1);
            Assert.NotNull(msg);
            Assert.True(msg.Size == 102.65M);
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
    [Serializable]
    public class Bar: IEquatable<Bar>
    {
        public bool Field1 { get; set; }

        public bool Equals(Bar other)
        {
            if (Field1 == other.Field1)
                return true;
            return false;
        }
    }
    [Serializable]
    public class Foo: IEquatable<Foo>
    {
        public Color Color { get; set; }
        public string Field1 { get; set; }
        public string Field2 { get; set; }
        public string Field3 { get; set; }
        public Bar Field4 { get; set; }
        public string Field5 { get; set; }

        public bool Equals(Foo other)
        {
            if (Field1 == other.Field1 && Field2 == other.Field2 && Field3 == other.Field3 
                && Field4?.Field1 == other.Field4?.Field1 && Field5 == other.Field5)
                return true;
            return false;
        }
    }
    [Serializable]
    public enum Color
    {
        RED,
        BLUE,
        GREEN
    }
    public class LogicalMessage : ISpecificRecord
    {
        [LogicalType(LogicalTypeKind.Date)]
        public DateTime CreatedTime { get; set; }
        
        public AvroDecimal Size { get; set; }
        public string DayOfWeek { get; set; }

        [Ignore]
        public Avro.Schema Schema { get; set; }

        public object Get(int fieldPos)
        {
            switch (fieldPos)
            {
                case 0: return CreatedTime;
                case 1: return Size;
                case 2: return DayOfWeek;
                default: throw new AvroRuntimeException("Bad index " + fieldPos + " in Get()");
            };
        }

        public void Put(int fieldPos, object fieldValue)
        {
            switch (fieldPos)
            {
                case 0: CreatedTime = (DateTime)fieldValue; break;
                case 1: Size = (AvroDecimal)fieldValue; break;
                case 2: DayOfWeek = (String)fieldValue; break;
                default: throw new AvroRuntimeException("Bad index " + fieldPos + " in Put()");
            };
        }
    }
}