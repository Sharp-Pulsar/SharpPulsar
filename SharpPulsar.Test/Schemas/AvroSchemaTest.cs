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
namespace Org.Apache.Pulsar.Client.Impl.Schema
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.apache.pulsar.client.impl.schema.SchemaTestUtils.FOO_FIELDS;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.apache.pulsar.client.impl.schema.SchemaTestUtils.SCHEMA_AVRO_NOT_ALLOW_NULL;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.apache.pulsar.client.impl.schema.SchemaTestUtils.SCHEMA_AVRO_ALLOW_NULL;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.testng.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.testng.Assert.assertNotEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.testng.Assert.fail;


	using ObjectMapper = com.fasterxml.jackson.databind.ObjectMapper;
	using ByteBuf = io.netty.buffer.ByteBuf;
	using ByteBufAllocator = io.netty.buffer.ByteBufAllocator;
	using Data = lombok.Data;
	using Slf4j = lombok.@extern.slf4j.Slf4j;

	using Schema = org.apache.avro.Schema;
	using BinaryEncoder = org.apache.avro.io.BinaryEncoder;
	using BufferedBinaryEncoder = org.apache.avro.io.BufferedBinaryEncoder;
	using SchemaSerializationException = org.apache.pulsar.client.api.SchemaSerializationException;
	using SchemaDefinition = org.apache.pulsar.client.api.schema.SchemaDefinition;
	using SchemaValidationException = org.apache.avro.SchemaValidationException;
	using SchemaValidator = org.apache.avro.SchemaValidator;
	using SchemaValidatorBuilder = org.apache.avro.SchemaValidatorBuilder;
	using AvroDefault = org.apache.avro.reflect.AvroDefault;
	using Nullable = org.apache.avro.reflect.Nullable;
	using ReflectData = org.apache.avro.reflect.ReflectData;
	using RecordSchemaBuilder = org.apache.pulsar.client.api.schema.RecordSchemaBuilder;
	using SchemaBuilder = org.apache.pulsar.client.api.schema.SchemaBuilder;
	using SchemaDefinitionBuilder = org.apache.pulsar.client.api.schema.SchemaDefinitionBuilder;
	using SchemaReader = org.apache.pulsar.client.api.schema.SchemaReader;
	using SchemaWriter = org.apache.pulsar.client.api.schema.SchemaWriter;
	using NasaMission = org.apache.pulsar.client.avro.generated.NasaMission;
	using Bar = Org.Apache.Pulsar.Client.Impl.Schema.SchemaTestUtils.Bar;
	using Foo = Org.Apache.Pulsar.Client.Impl.Schema.SchemaTestUtils.Foo;
	using JacksonJsonReader = Org.Apache.Pulsar.Client.Impl.Schema.reader.JacksonJsonReader;
	using AvroWriter = Org.Apache.Pulsar.Client.Impl.Schema.writer.AvroWriter;
	using JacksonJsonWriter = Org.Apache.Pulsar.Client.Impl.Schema.writer.JacksonJsonWriter;
	using SchemaInfo = org.apache.pulsar.common.schema.SchemaInfo;
	using SchemaType = org.apache.pulsar.common.schema.SchemaType;
	using DateTime = org.joda.time.DateTime;
	using ISOChronology = org.joda.time.chrono.ISOChronology;
	using JSONException = org.json.JSONException;
	using JSONAssert = org.skyscreamer.jsonassert.JSONAssert;
	using Whitebox = org.powermock.reflect.Whitebox;
	using Assert = org.testng.Assert;
	using Test = org.testng.annotations.Test;


//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Slf4j public class AvroSchemaTest
	public class AvroSchemaTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Data private static class DefaultStruct
		private class DefaultStruct
		{
			internal int Field1;
			internal string Field2;
			internal long? Field3;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Data private static class StructWithAnnotations
		private class StructWithAnnotations
		{
			internal int Field1;
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Nullable String field2;
			internal string Field2;
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @AvroDefault("\"1000\"") System.Nullable<long> field3;
			internal long? Field3;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Data private static class SchemaLogicalType
		private class SchemaLogicalType
		{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @org.apache.avro.reflect.AvroSchema("{\n" + "  \"type\": \"bytes\",\n" + "  \"logicalType\": \"decimal\",\n" + "  \"precision\": 4,\n" + "  \"scale\": 2\n" + "}") java.math.BigDecimal decimal;
			internal decimal Decimal;
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @org.apache.avro.reflect.AvroSchema("{\"type\":\"int\",\"logicalType\":\"date\"}") java.time.LocalDate date;
			internal LocalDate Date;
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @org.apache.avro.reflect.AvroSchema("{\"type\":\"long\",\"logicalType\":\"timestamp-millis\"}") java.time.Instant timestampMillis;
			internal Instant TimestampMillis;
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @org.apache.avro.reflect.AvroSchema("{\"type\":\"int\",\"logicalType\":\"time-millis\"}") java.time.LocalTime timeMillis;
			internal LocalTime TimeMillis;
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @org.apache.avro.reflect.AvroSchema("{\"type\":\"long\",\"logicalType\":\"timestamp-micros\"}") long timestampMicros;
			internal long TimestampMicros;
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @org.apache.avro.reflect.AvroSchema("{\"type\":\"long\",\"logicalType\":\"time-micros\"}") long timeMicros;
			internal long TimeMicros;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Data private static class JodaTimeLogicalType
		private class JodaTimeLogicalType
		{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @org.apache.avro.reflect.AvroSchema("{\"type\":\"int\",\"logicalType\":\"date\"}") java.time.LocalDate date;
			internal LocalDate Date;
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @org.apache.avro.reflect.AvroSchema("{\"type\":\"long\",\"logicalType\":\"timestamp-millis\"}") org.joda.time.DateTime timestampMillis;
			internal DateTime TimestampMillis;
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @org.apache.avro.reflect.AvroSchema("{\"type\":\"int\",\"logicalType\":\"time-millis\"}") java.time.LocalTime timeMillis;
			internal LocalTime TimeMillis;
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @org.apache.avro.reflect.AvroSchema("{\"type\":\"long\",\"logicalType\":\"timestamp-micros\"}") long timestampMicros;
			internal long TimestampMicros;
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @org.apache.avro.reflect.AvroSchema("{\"type\":\"long\",\"logicalType\":\"time-micros\"}") long timeMicros;
			internal long TimeMicros;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testSchemaDefinition() throws org.apache.avro.SchemaValidationException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		public virtual void TestSchemaDefinition()
		{
			Schema Schema1 = ReflectData.get().getSchema(typeof(DefaultStruct));
			AvroSchema<StructWithAnnotations> Schema2 = AvroSchema.of(typeof(StructWithAnnotations));

			string SchemaDef1 = Schema1.ToString();
			string SchemaDef2 = new string(Schema2.SchemaInfo.Schema, UTF_8);
			assertNotEquals(SchemaDef1, SchemaDef2, "schema1 = " + SchemaDef1 + ", schema2 = " + SchemaDef2);

			SchemaValidator Validator = (new SchemaValidatorBuilder()).mutualReadStrategy().validateLatest();
			try
			{
				Validator.validate(Schema1, Arrays.asList((new Schema.Parser()).setValidateDefaults(false).parse(SchemaDef2)));
				fail("Should fail on validating incompatible schemas");
			}
			catch (SchemaValidationException)
			{
				// expected
			}

			AvroSchema<StructWithAnnotations> Schema3 = AvroSchema.of(SchemaDefinition.builder<StructWithAnnotations>().withJsonDef(SchemaDef1).build());
			string SchemaDef3 = new string(Schema3.SchemaInfo.Schema, UTF_8);
			assertEquals(SchemaDef1, SchemaDef3);
			assertNotEquals(SchemaDef2, SchemaDef3);

			StructWithAnnotations Struct = new StructWithAnnotations();
			Struct.Field1 = 5678;
			// schema2 is using the schema generated from POJO,
			// it allows field2 to be nullable, and field3 has default value.
			Schema2.encode(Struct);
			try
			{
				// schema3 is using the schema passed in, which doesn't allow nullable
				Schema3.encode(Struct);
				fail("Should fail to write the record since the provided schema is incompatible");
			}
			catch (SchemaSerializationException)
			{
				// expected
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void assertJSONEquals(String s1, String s2) throws org.json.JSONException
		public virtual void AssertJSONEquals(string S1, string S2)
		{
			JSONAssert.assertEquals(S1, S2, false);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testNotAllowNullSchema() throws org.json.JSONException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		public virtual void TestNotAllowNullSchema()
		{
			AvroSchema<Foo> AvroSchema = AvroSchema.of(SchemaDefinition.builder<Foo>().withPojo(typeof(Foo)).withAlwaysAllowNull(false).build());
			assertEquals(AvroSchema.SchemaInfo.Type, SchemaType.AVRO);
			Schema.Parser Parser = new Schema.Parser();
			string SchemaJson = new string(AvroSchema.SchemaInfo.Schema);
			AssertJSONEquals(SchemaJson, SCHEMA_AVRO_NOT_ALLOW_NULL);
			Schema Schema = Parser.parse(SchemaJson);

			foreach (string FieldName in FOO_FIELDS)
			{
				Schema.Field Field = Schema.getField(FieldName);
				Assert.assertNotNull(Field);

				if (Field.name().Equals("field4"))
				{
					Assert.assertNotNull(Field.schema().Types.get(1).getField("field1"));
				}
				if (Field.name().Equals("fieldUnableNull"))
				{
					Assert.assertNotNull(Field.schema().Type);
				}
			}
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testAllowNullSchema() throws org.json.JSONException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		public virtual void TestAllowNullSchema()
		{
			AvroSchema<Foo> AvroSchema = AvroSchema.of(SchemaDefinition.builder<Foo>().withPojo(typeof(Foo)).build());
			assertEquals(AvroSchema.SchemaInfo.Type, SchemaType.AVRO);
			Schema.Parser Parser = new Schema.Parser();
			Parser.ValidateDefaults = false;
			string SchemaJson = new string(AvroSchema.SchemaInfo.Schema);
			AssertJSONEquals(SchemaJson, SCHEMA_AVRO_ALLOW_NULL);
			Schema Schema = Parser.parse(SchemaJson);

			foreach (string FieldName in FOO_FIELDS)
			{
				Schema.Field Field = Schema.getField(FieldName);
				Assert.assertNotNull(Field);

				if (Field.name().Equals("field4"))
				{
					Assert.assertNotNull(Field.schema().Types.get(1).getField("field1"));
				}
				if (Field.name().Equals("fieldUnableNull"))
				{
					Assert.assertNotNull(Field.schema().Type);
				}
			}
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testNotAllowNullEncodeAndDecode()
		public virtual void TestNotAllowNullEncodeAndDecode()
		{
			AvroSchema<Foo> AvroSchema = AvroSchema.of(SchemaDefinition.builder<Foo>().withPojo(typeof(Foo)).withAlwaysAllowNull(false).build());

			Foo Foo1 = new Foo();
			Foo1.Field1 = "foo1";
			Foo1.Field2 = "bar1";
			Foo1.Field4 = new Bar();
			Foo1.FieldUnableNull = "notNull";

			Foo Foo2 = new Foo();
			Foo2.Field1 = "foo2";
			Foo2.Field2 = "bar2";

			sbyte[] Bytes1 = AvroSchema.encode(Foo1);
			Foo Object1 = AvroSchema.decode(Bytes1);
			Assert.assertTrue(Bytes1.Length > 0);
			assertEquals(Object1, Foo1);

			try
			{

				AvroSchema.encode(Foo2);

			}
			catch (Exception E)
			{
				Assert.assertTrue(E is SchemaSerializationException);
			}

		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testAllowNullEncodeAndDecode()
		public virtual void TestAllowNullEncodeAndDecode()
		{
			AvroSchema<Foo> AvroSchema = AvroSchema.of(SchemaDefinition.builder<Foo>().withPojo(typeof(Foo)).build());

			Foo Foo1 = new Foo();
			Foo1.Field1 = "foo1";
			Foo1.Field2 = "bar1";
			Foo1.Field4 = new Bar();

			Foo Foo2 = new Foo();
			Foo2.Field1 = "foo2";
			Foo2.Field2 = "bar2";

			sbyte[] Bytes1 = AvroSchema.encode(Foo1);
			Assert.assertTrue(Bytes1.Length > 0);

			sbyte[] Bytes2 = AvroSchema.encode(Foo2);
			Assert.assertTrue(Bytes2.Length > 0);

			Foo Object1 = AvroSchema.decode(Bytes1);
			Foo Object2 = AvroSchema.decode(Bytes2);

			assertEquals(Object1, Foo1);
			assertEquals(Object2, Foo2);

		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testLogicalType()
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

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testJodaTimeLogicalType()
		public virtual void TestJodaTimeLogicalType()
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

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testDateAndTimestamp()
	  public virtual void TestDateAndTimestamp()
	  {
		RecordSchemaBuilder RecordSchemaBuilder = SchemaBuilder.record("org.apache.pulsar.client.avro.generated.NasaMission");
		RecordSchemaBuilder.field("id").type(SchemaType.INT32);
		RecordSchemaBuilder.field("name").type(SchemaType.STRING);
		RecordSchemaBuilder.field("create_year").type(SchemaType.DATE);
		RecordSchemaBuilder.field("create_time").type(SchemaType.TIME);
		RecordSchemaBuilder.field("create_timestamp").type(SchemaType.TIMESTAMP);
		SchemaInfo SchemaInfo = RecordSchemaBuilder.build(SchemaType.AVRO);

		Schema RecordSchema = (new Schema.Parser()).parse(new string(SchemaInfo.Schema, UTF_8));
		AvroSchema<NasaMission> AvroSchema = AvroSchema.of(SchemaDefinition.builder<NasaMission>().withPojo(typeof(NasaMission)).build());
		assertEquals(RecordSchema, AvroSchema.schema);

		NasaMission NasaMission = NasaMission.newBuilder().setId(1001).setName("one").setCreateYear(LocalDate.now()).setCreateTime(LocalTime.now()).setCreateTimestamp(Instant.now()).build();

		sbyte[] Bytes = AvroSchema.encode(NasaMission);
		Assert.assertTrue(Bytes.Length > 0);

		NasaMission Object = AvroSchema.decode(Bytes);
		assertEquals(Object, NasaMission);
	  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testDecodeByteBuf()
		public virtual void TestDecodeByteBuf()
		{
			AvroSchema<Foo> AvroSchema = AvroSchema.of(SchemaDefinition.builder<Foo>().withPojo(typeof(Foo)).build());

			Foo Foo1 = new Foo();
			Foo1.Field1 = "foo1";
			Foo1.Field2 = "bar1";
			Foo1.Field4 = new Bar();
			Foo1.FieldUnableNull = "notNull";

			Foo Foo2 = new Foo();
			Foo2.Field1 = "foo2";
			Foo2.Field2 = "bar2";

			sbyte[] Bytes1 = AvroSchema.encode(Foo1);
			ByteBuf ByteBuf = ByteBufAllocator.DEFAULT.buffer(Bytes1.Length);
			ByteBuf.writeBytes(Bytes1);

			Foo Object1 = AvroSchema.decode(ByteBuf);
			Assert.assertTrue(Bytes1.Length > 0);
			assertEquals(Object1, Foo1);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void discardBufferIfBadAvroData()
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

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testAvroSchemaUserDefinedReadAndWriter()
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