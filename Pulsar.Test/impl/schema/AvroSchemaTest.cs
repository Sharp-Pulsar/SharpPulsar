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
namespace org.apache.pulsar.client.impl.schema
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


	using ByteBuf = io.netty.buffer.ByteBuf;
	using ByteBufAllocator = io.netty.buffer.ByteBufAllocator;
	using Data = lombok.Data;
	using Slf4j = lombok.@extern.slf4j.Slf4j;

	using Schema = apache.avro.Schema;
	using SchemaSerializationException = api.SchemaSerializationException;
	using SchemaDefinition = api.schema.SchemaDefinition;
	using SchemaValidationException = apache.avro.SchemaValidationException;
	using SchemaValidator = apache.avro.SchemaValidator;
	using SchemaValidatorBuilder = apache.avro.SchemaValidatorBuilder;
	using AvroDefault = apache.avro.reflect.AvroDefault;
	using Nullable = apache.avro.reflect.Nullable;
	using ReflectData = apache.avro.reflect.ReflectData;
	using RecordSchemaBuilder = api.schema.RecordSchemaBuilder;
	using SchemaBuilder = api.schema.SchemaBuilder;
	using NasaMission = client.avro.generated.NasaMission;
	using Bar = SchemaTestUtils.Bar;
	using Foo = SchemaTestUtils.Foo;
	using SchemaInfo = common.schema.SchemaInfo;
	using SchemaType = common.schema.SchemaType;
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
			internal int field1;
			internal string field2;
			internal long? field3;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Data private static class StructWithAnnotations
		private class StructWithAnnotations
		{
			internal int field1;
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Nullable String field2;
			internal string field2;
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @AvroDefault("\"1000\"") System.Nullable<long> field3;
			internal long? field3;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Data private static class SchemaLogicalType
		private class SchemaLogicalType
		{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @org.apache.avro.reflect.AvroSchema("{\n" + "  \"type\": \"bytes\",\n" + "  \"logicalType\": \"decimal\",\n" + "  \"precision\": 4,\n" + "  \"scale\": 2\n" + "}") java.math.BigDecimal decimal;
			internal decimal @decimal;
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @org.apache.avro.reflect.AvroSchema("{\"type\":\"int\",\"logicalType\":\"date\"}") java.time.LocalDate date;
			internal LocalDate date;
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @org.apache.avro.reflect.AvroSchema("{\"type\":\"long\",\"logicalType\":\"timestamp-millis\"}") java.time.Instant timestampMillis;
			internal Instant timestampMillis;
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @org.apache.avro.reflect.AvroSchema("{\"type\":\"int\",\"logicalType\":\"time-millis\"}") java.time.LocalTime timeMillis;
			internal LocalTime timeMillis;
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @org.apache.avro.reflect.AvroSchema("{\"type\":\"long\",\"logicalType\":\"timestamp-micros\"}") long timestampMicros;
			internal long timestampMicros;
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @org.apache.avro.reflect.AvroSchema("{\"type\":\"long\",\"logicalType\":\"time-micros\"}") long timeMicros;
			internal long timeMicros;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testSchemaDefinition() throws org.apache.avro.SchemaValidationException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		public virtual void testSchemaDefinition()
		{
			Schema schema1 = ReflectData.get().getSchema(typeof(DefaultStruct));
			AvroSchema<StructWithAnnotations> schema2 = AvroSchema.of(typeof(StructWithAnnotations));

			string schemaDef1 = schema1.ToString();
			string schemaDef2 = new string(schema2.SchemaInfo.Schema, UTF_8);
			assertNotEquals(schemaDef1, schemaDef2, "schema1 = " + schemaDef1 + ", schema2 = " + schemaDef2);

			SchemaValidator validator = (new SchemaValidatorBuilder()).mutualReadStrategy().validateLatest();
			try
			{
				validator.validate(schema1, Arrays.asList((new Schema.Parser()).setValidateDefaults(false).parse(schemaDef2)));
				fail("Should fail on validating incompatible schemas");
			}
			catch (SchemaValidationException)
			{
				// expected
			}

			AvroSchema<StructWithAnnotations> schema3 = AvroSchema.of(SchemaDefinition.builder<StructWithAnnotations>().withJsonDef(schemaDef1).build());
			string schemaDef3 = new string(schema3.SchemaInfo.Schema, UTF_8);
			assertEquals(schemaDef1, schemaDef3);
			assertNotEquals(schemaDef2, schemaDef3);

			StructWithAnnotations @struct = new StructWithAnnotations();
			@struct.Field1 = 5678;
			// schema2 is using the schema generated from POJO,
			// it allows field2 to be nullable, and field3 has default value.
			schema2.encode(@struct);
			try
			{
				// schema3 is using the schema passed in, which doesn't allow nullable
				schema3.encode(@struct);
				fail("Should fail to write the record since the provided schema is incompatible");
			}
			catch (SchemaSerializationException)
			{
				// expected
			}
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testNotAllowNullSchema()
		public virtual void testNotAllowNullSchema()
		{
			AvroSchema<Foo> avroSchema = AvroSchema.of(SchemaDefinition.builder<Foo>().withPojo(typeof(Foo)).withAlwaysAllowNull(false).build());
			assertEquals(avroSchema.SchemaInfo.Type, SchemaType.AVRO);
			Schema.Parser parser = new Schema.Parser();
			string schemaJson = new string(avroSchema.SchemaInfo.Schema);
			assertEquals(schemaJson, SCHEMA_AVRO_NOT_ALLOW_NULL);
			Schema schema = parser.parse(schemaJson);

			foreach (string fieldName in FOO_FIELDS)
			{
				Schema.Field field = schema.getField(fieldName);
				Assert.assertNotNull(field);

				if (field.name().Equals("field4"))
				{
					Assert.assertNotNull(field.schema().Types.get(1).getField("field1"));
				}
				if (field.name().Equals("fieldUnableNull"))
				{
					Assert.assertNotNull(field.schema().Type);
				}
			}
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testAllowNullSchema()
		public virtual void testAllowNullSchema()
		{
			AvroSchema<Foo> avroSchema = AvroSchema.of(SchemaDefinition.builder<Foo>().withPojo(typeof(Foo)).build());
			assertEquals(avroSchema.SchemaInfo.Type, SchemaType.AVRO);
			Schema.Parser parser = new Schema.Parser();
			parser.ValidateDefaults = false;
			string schemaJson = new string(avroSchema.SchemaInfo.Schema);
			assertEquals(schemaJson, SCHEMA_AVRO_ALLOW_NULL);
			Schema schema = parser.parse(schemaJson);

			foreach (string fieldName in FOO_FIELDS)
			{
				Schema.Field field = schema.getField(fieldName);
				Assert.assertNotNull(field);

				if (field.name().Equals("field4"))
				{
					Assert.assertNotNull(field.schema().Types.get(1).getField("field1"));
				}
				if (field.name().Equals("fieldUnableNull"))
				{
					Assert.assertNotNull(field.schema().Type);
				}
			}
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testNotAllowNullEncodeAndDecode()
		public virtual void testNotAllowNullEncodeAndDecode()
		{
			AvroSchema<Foo> avroSchema = AvroSchema.of(SchemaDefinition.builder<Foo>().withPojo(typeof(Foo)).withAlwaysAllowNull(false).build());

			Foo foo1 = new Foo();
			foo1.Field1 = "foo1";
			foo1.Field2 = "bar1";
			foo1.Field4 = new Bar();
			foo1.FieldUnableNull = "notNull";

			Foo foo2 = new Foo();
			foo2.Field1 = "foo2";
			foo2.Field2 = "bar2";

			sbyte[] bytes1 = avroSchema.encode(foo1);
			Foo object1 = avroSchema.decode(bytes1);
			Assert.assertTrue(bytes1.Length > 0);
			assertEquals(object1, foo1);

			try
			{

				avroSchema.encode(foo2);

			}
			catch (Exception e)
			{
				Assert.assertTrue(e is SchemaSerializationException);
			}

		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testAllowNullEncodeAndDecode()
		public virtual void testAllowNullEncodeAndDecode()
		{
			AvroSchema<Foo> avroSchema = AvroSchema.of(SchemaDefinition.builder<Foo>().withPojo(typeof(Foo)).build());

			Foo foo1 = new Foo();
			foo1.Field1 = "foo1";
			foo1.Field2 = "bar1";
			foo1.Field4 = new Bar();

			Foo foo2 = new Foo();
			foo2.Field1 = "foo2";
			foo2.Field2 = "bar2";

			sbyte[] bytes1 = avroSchema.encode(foo1);
			Assert.assertTrue(bytes1.Length > 0);

			sbyte[] bytes2 = avroSchema.encode(foo2);
			Assert.assertTrue(bytes2.Length > 0);

			Foo object1 = avroSchema.decode(bytes1);
			Foo object2 = avroSchema.decode(bytes2);

			assertEquals(object1, foo1);
			assertEquals(object2, foo2);

		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testLogicalType()
		public virtual void testLogicalType()
		{
			AvroSchema<SchemaLogicalType> avroSchema = AvroSchema.of(SchemaDefinition.builder<SchemaLogicalType>().withPojo(typeof(SchemaLogicalType)).build());

			SchemaLogicalType schemaLogicalType = new SchemaLogicalType();
			schemaLogicalType.TimestampMicros = DateTimeHelper.CurrentUnixTimeMillis() * 1000;
			schemaLogicalType.TimestampMillis = Instant.parse("2019-03-26T04:39:58.469Z");
			schemaLogicalType.Decimal = new decimal("12.34");
			schemaLogicalType.Date = LocalDate.now();
			schemaLogicalType.TimeMicros = DateTimeHelper.CurrentUnixTimeMillis() * 1000;
			schemaLogicalType.TimeMillis = LocalTime.now();

			sbyte[] bytes1 = avroSchema.encode(schemaLogicalType);
			Assert.assertTrue(bytes1.Length > 0);

			SchemaLogicalType object1 = avroSchema.decode(bytes1);

			assertEquals(object1, schemaLogicalType);

		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testDateAndTimestamp()
	  public virtual void testDateAndTimestamp()
	  {
		RecordSchemaBuilder recordSchemaBuilder = SchemaBuilder.record("org.apache.pulsar.client.avro.generated.NasaMission");
		recordSchemaBuilder.field("id").type(SchemaType.INT32);
		recordSchemaBuilder.field("name").type(SchemaType.STRING);
		recordSchemaBuilder.field("create_year").type(SchemaType.DATE);
		recordSchemaBuilder.field("create_time").type(SchemaType.TIME);
		recordSchemaBuilder.field("create_timestamp").type(SchemaType.TIMESTAMP);
		SchemaInfo schemaInfo = recordSchemaBuilder.build(SchemaType.AVRO);

		Schema recordSchema = (new Schema.Parser()).parse(new string(schemaInfo.Schema, UTF_8)
	   );
		AvroSchema<NasaMission> avroSchema = AvroSchema.of(SchemaDefinition.builder<NasaMission>().withPojo(typeof(NasaMission)).build());
		assertEquals(recordSchema, avroSchema.schema);

		NasaMission nasaMission = NasaMission.newBuilder().setId(1001).setName("one").setCreateYear(LocalDate.now()).setCreateTime(LocalTime.now()).setCreateTimestamp(Instant.now()).build();

		sbyte[] bytes = avroSchema.encode(nasaMission);
		Assert.assertTrue(bytes.Length > 0);

		NasaMission @object = avroSchema.decode(bytes);
		assertEquals(@object, nasaMission);
	  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testDecodeByteBuf()
		public virtual void testDecodeByteBuf()
		{
			AvroSchema<Foo> avroSchema = AvroSchema.of(SchemaDefinition.builder<Foo>().withPojo(typeof(Foo)).build());

			Foo foo1 = new Foo();
			foo1.Field1 = "foo1";
			foo1.Field2 = "bar1";
			foo1.Field4 = new Bar();
			foo1.FieldUnableNull = "notNull";

			Foo foo2 = new Foo();
			foo2.Field1 = "foo2";
			foo2.Field2 = "bar2";

			sbyte[] bytes1 = avroSchema.encode(foo1);
			ByteBuf byteBuf = ByteBufAllocator.DEFAULT.buffer(bytes1.Length);
			byteBuf.writeBytes(bytes1);

			Foo object1 = avroSchema.decode(byteBuf);
			Assert.assertTrue(bytes1.Length > 0);
			assertEquals(object1, foo1);

		}

	}

}