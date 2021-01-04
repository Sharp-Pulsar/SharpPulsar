using NodaTime;
using SharpPulsar.Schema;
using System;

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

		private class JodaTimeLogicalType
		{
			internal LocalDate Date;
			internal DateTime TimestampMillis;
			internal LocalTime TimeMillis;
			internal long TimestampMicros;
			internal long TimeMicros;
		}

		public virtual void TestLogicalType()
		{
			AvroSchema<SchemaLogicalType> AvroSchema = AvroSchema.of(SchemaDefinition.builder<SchemaLogicalType>().withPojo(typeof(SchemaLogicalType)).withJSR310ConversionEnabled(true).build());

			SchemaLogicalType SchemaLogicalType = new SchemaLogicalType();
			SchemaLogicalType.TimestampMicros = DateTimeHelper.CurrentUnixTimeMillis() * 1000;
			SchemaLogicalType.TimestampMillis = Instant.parse("2019-03-26T04:39:58.469Z");
			SchemaLogicalType.Decimal = new decimal("12.34");
			SchemaLogicalType.Date = LocalDate.now();
			SchemaLogicalType.TimeMicros = DateTimeHelper.CurrentUnixTimeMillis() * 1000;
			SchemaLogicalType.TimeMillis = LocalTime.now();

			sbyte[] Bytes1 = AvroSchema.encode(SchemaLogicalType);
			Assert.assertTrue(Bytes1.Length > 0);

			SchemaLogicalType Object1 = AvroSchema.decode(Bytes1);

			assertEquals(Object1, SchemaLogicalType);

		}

		public virtual void TestNodaTimeLogicalType()
		{
			AvroSchema<JodaTimeLogicalType> AvroSchema = AvroSchema.of(SchemaDefinition.builder<JodaTimeLogicalType>().withPojo(typeof(JodaTimeLogicalType)).build());
			JodaTimeLogicalType SchemaLogicalType = new JodaTimeLogicalType();
			SchemaLogicalType.TimestampMicros = DateTimeHelper.CurrentUnixTimeMillis() * 1000;
			SchemaLogicalType.TimestampMillis = new DateTime("2019-03-26T04:39:58.469Z", ISOChronology.InstanceUTC);
			SchemaLogicalType.Date = LocalDate.now();
			SchemaLogicalType.TimeMicros = DateTimeHelper.CurrentUnixTimeMillis() * 1000;
			SchemaLogicalType.TimeMillis = LocalTime.now();

			sbyte[] Bytes1 = AvroSchema.encode(SchemaLogicalType);
			Assert.assertTrue(Bytes1.Length > 0);

			JodaTimeLogicalType Object1 = AvroSchema.decode(Bytes1);

			assertEquals(Object1, SchemaLogicalType);
		}

		public virtual void DiscardBufferIfBadAvroData()
		{
			AvroWriter<NasaMission> AvroWriter = new AvroWriter<NasaMission>(ReflectData.AllowNull.get().getSchema(typeof(NasaMission)));

			NasaMission BadNasaMissionData = new NasaMission();
			BadNasaMissionData.Id = 1;
			// set null in the non-null field. The java set will accept it but going ahead, the avro encode will crash.
			BadNasaMissionData.Name = null;

			// Because data does not conform to schema expect a crash
			Assert.assertThrows(typeof(SchemaSerializationException), () => AvroWriter.write(BadNasaMissionData));

			// Get the buffered data using powermock
			BinaryEncoder Encoder = Whitebox.getInternalState(AvroWriter, "encoder");

			// Assert that the buffer position is reset to zero
			Assert.assertEquals(((BufferedBinaryEncoder)Encoder).bytesBuffered(), 0);
		}

		public virtual void TestAvroSchemaUserDefinedReadAndWriter()
		{
			SchemaReader<Foo> Reader = new JacksonJsonReader<Foo>(new ObjectMapper(), typeof(Foo));
			SchemaWriter<Foo> Writer = new JacksonJsonWriter<Foo>(new ObjectMapper());
			SchemaDefinition<Foo> SchemaDefinition = SchemaDefinition.builder<Foo>().withPojo(typeof(Bar)).withSchemaReader(Reader).withSchemaWriter(Writer).build();

			AvroSchema<Foo> Schema = AvroSchema.of(SchemaDefinition);
			Foo Foo = new Foo();
			Foo.Color = SchemaTestUtils.Color.RED;
			string Field1 = "test";
			Foo.Field1 = Field1;
			Schema.encode(Foo);
			Foo = Schema.decode(Schema.encode(Foo));
			assertEquals(Foo.Color, SchemaTestUtils.Color.RED);
			assertEquals(Field1, Foo.Field1);
		}

	}

}