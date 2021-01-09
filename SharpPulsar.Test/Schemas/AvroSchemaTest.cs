using AvroSchemaGenerator;
using NodaTime;
using SharpPulsar.Interfaces.ISchema;
using SharpPulsar.Schema;
using SharpPulsar.Schema.Reader;
using SharpPulsar.Schema.Writer;
using System;
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
			internal int Field1;
			internal string Field2;
			internal long? Field3;
		}

		private class StructWithAnnotations
		{
			internal int Field1;
			internal string Field2;
			internal long? Field3;
		}

		private class SchemaLogicalType
		{
			internal decimal Decimal;
			internal LocalDate Date;
			internal Instant TimestampMillis;
			internal LocalTime TimeMillis;
			internal long TimestampMicros;
			internal long TimeMicros;
		}

		private class NodaTimeLogicalType
		{
			internal LocalDate Date;
			internal DateTime TimestampMillis;
			internal LocalTime TimeMillis;
			internal long TimestampMicros;
			internal long TimeMicros;
		}
		[Fact]
		public virtual void TestLogicalType()
		{
			AvroSchema<SchemaLogicalType> avroSchema = AvroSchema<SchemaLogicalType>.Of(ISchemaDefinition<SchemaLogicalType>.Builder().WithPojo(typeof(SchemaLogicalType)).WithJSR310ConversionEnabled(true).Build());

            SchemaLogicalType schemaLogicalType = new SchemaLogicalType
            {
                TimestampMicros = DateTimeHelper.CurrentUnixTimeMillis() * 1000,
                TimestampMillis = Instant.FromDateTimeOffset(DateTimeOffset.Parse("2019-03-26T04:39:58.469Z")),
                Decimal = new decimal(12.34D),
                Date = LocalDate.FromDateTime(DateTime.Now),
                TimeMicros = DateTimeHelper.CurrentUnixTimeMillis() * 1000,
                TimeMillis = LocalTime.FromNanosecondsSinceMidnight(DateTimeOffset.UtcNow.Ticks)
            };

            sbyte[] bytes1 = avroSchema.Encode(schemaLogicalType);
			Assert.True(bytes1.Length > 0);

			SchemaLogicalType object1 = avroSchema.Decode(bytes1);

			Assert.Equal(object1, schemaLogicalType);

		}

		[Fact]
		public void TestNodaTimeLogicalType()
		{
			AvroSchema<NodaTimeLogicalType> avroSchema = AvroSchema<NodaTimeLogicalType>.Of(ISchemaDefinition<NodaTimeLogicalType>.Builder().WithPojo(typeof(NodaTimeLogicalType)).Build());
            NodaTimeLogicalType schemaLogicalType = new NodaTimeLogicalType
            {
                TimestampMicros = DateTimeHelper.CurrentUnixTimeMillis() * 1000,
                TimestampMillis = new DateTime(DateTime.Parse("2019-03-26T04:39:58.469Z").Ticks, DateTimeKind.Utc),
                Date = LocalDate.FromDateTime(DateTime.Now),
                TimeMicros = DateTimeHelper.CurrentUnixTimeMillis() * 1000,
                TimeMillis = LocalTime.FromNanosecondsSinceMidnight(DateTimeOffset.UtcNow.Ticks)
            };
            sbyte[] Bytes1 = avroSchema.Encode(schemaLogicalType);
			Assert.True(Bytes1.Length > 0);

			NodaTimeLogicalType Object1 = avroSchema.Decode(Bytes1);

			Assert.Equal(Object1, schemaLogicalType);
		}

		[Fact]
		public virtual void TestAvroSchemaUserDefinedReadAndWriter()
		{
			Avro.Schema avroSchema = Avro.Schema.Parse(typeof(SchemaTestUtils.Foo).GetSchema());
			ISchemaReader<SchemaTestUtils.Foo> reader = new AvroReader<SchemaTestUtils.Foo>(avroSchema);
			ISchemaWriter<SchemaTestUtils.Foo> writer = new AvroWriter<SchemaTestUtils.Foo>(avroSchema);
			ISchemaDefinition<SchemaTestUtils.Foo> schemaDefinition = ISchemaDefinition<SchemaTestUtils.Foo>.Builder().WithPojo(typeof(SchemaTestUtils.Bar)).WithSchemaReader(reader).WithSchemaWriter(writer).Build();
		
			AvroSchema<SchemaTestUtils.Foo> schema = AvroSchema<SchemaTestUtils.Foo>.Of(schemaDefinition);
            var foo = new SchemaTestUtils.Foo

			{
                Color = SchemaTestUtils.Color.RED
            };
            string Field1 = "test";
			foo.Field1 = Field1;
			schema.Encode(foo);
			foo = schema.Decode(schema.Encode(foo));
			Assert.Equal(SchemaTestUtils.Color.RED, foo.Color);
			Assert.Equal(Field1, foo.Field1);
		}

	}

}