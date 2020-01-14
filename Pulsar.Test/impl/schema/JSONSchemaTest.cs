using System;
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
namespace org.apache.pulsar.client.impl.schema
{

	using ByteBuf = io.netty.buffer.ByteBuf;
	using ByteBufAllocator = io.netty.buffer.ByteBufAllocator;
	using Slf4j = lombok.@extern.slf4j.Slf4j;
	using Schema = org.apache.avro.Schema;
	using SchemaSerializationException = org.apache.pulsar.client.api.SchemaSerializationException;
	using SchemaDefinition = org.apache.pulsar.client.api.schema.SchemaDefinition;
	using Bar = org.apache.pulsar.client.impl.schema.SchemaTestUtils.Bar;
	using DerivedFoo = org.apache.pulsar.client.impl.schema.SchemaTestUtils.DerivedFoo;
	using Foo = org.apache.pulsar.client.impl.schema.SchemaTestUtils.Foo;
	using NestedBar = org.apache.pulsar.client.impl.schema.SchemaTestUtils.NestedBar;
	using NestedBarList = org.apache.pulsar.client.impl.schema.SchemaTestUtils.NestedBarList;
	using SchemaType = org.apache.pulsar.common.schema.SchemaType;
	using Assert = org.testng.Assert;
	using Test = org.testng.annotations.Test;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.apache.pulsar.client.impl.schema.SchemaTestUtils.FOO_FIELDS;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.apache.pulsar.client.impl.schema.SchemaTestUtils.SCHEMA_JSON_NOT_ALLOW_NULL;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.apache.pulsar.client.impl.schema.SchemaTestUtils.SCHEMA_JSON_ALLOW_NULL;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.testng.Assert.assertEquals;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Slf4j public class JSONSchemaTest
	public class JSONSchemaTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testNotAllowNullSchema()
		public virtual void testNotAllowNullSchema()
		{
			JSONSchema<Foo> jsonSchema = JSONSchema.of(SchemaDefinition.builder<Foo>().withPojo(typeof(Foo)).withAlwaysAllowNull(false).build());
			Assert.assertEquals(jsonSchema.SchemaInfo.Type, SchemaType.JSON);
			Schema.Parser parser = new Schema.Parser();
			string schemaJson = new string(jsonSchema.SchemaInfo.Schema);
			Assert.assertEquals(schemaJson, SCHEMA_JSON_NOT_ALLOW_NULL);
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
			JSONSchema<Foo> jsonSchema = JSONSchema.of(SchemaDefinition.builder<Foo>().withPojo(typeof(Foo)).build());
			Assert.assertEquals(jsonSchema.SchemaInfo.Type, SchemaType.JSON);
			Schema.Parser parser = new Schema.Parser();
			parser.ValidateDefaults = false;
			string schemaJson = new string(jsonSchema.SchemaInfo.Schema);
			Assert.assertEquals(schemaJson, SCHEMA_JSON_ALLOW_NULL);
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
//ORIGINAL LINE: @Test public void testAllowNullEncodeAndDecode()
		public virtual void testAllowNullEncodeAndDecode()
		{
			JSONSchema<Foo> jsonSchema = JSONSchema.of(SchemaDefinition.builder<Foo>().withPojo(typeof(Foo)).build());

			Bar bar = new Bar();
			bar.Field1 = true;

			Foo foo1 = new Foo();
			foo1.Field1 = "foo1";
			foo1.Field2 = "bar1";
			foo1.Field4 = bar;
			foo1.Color = SchemaTestUtils.Color.BLUE;

			Foo foo2 = new Foo();
			foo2.Field1 = "foo2";
			foo2.Field2 = "bar2";

			sbyte[] bytes1 = jsonSchema.encode(foo1);
			Assert.assertTrue(bytes1.Length > 0);

			sbyte[] bytes2 = jsonSchema.encode(foo2);
			Assert.assertTrue(bytes2.Length > 0);

			Foo object1 = jsonSchema.decode(bytes1);
			Foo object2 = jsonSchema.decode(bytes2);

			Assert.assertEquals(object1, foo1);
			Assert.assertEquals(object2, foo2);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testNotAllowNullEncodeAndDecode()
		public virtual void testNotAllowNullEncodeAndDecode()
		{
			JSONSchema<Foo> jsonSchema = JSONSchema.of(SchemaDefinition.builder<Foo>().withPojo(typeof(Foo)).withAlwaysAllowNull(false).build());

			Foo foo1 = new Foo();
			foo1.Field1 = "foo1";
			foo1.Field2 = "bar1";
			foo1.Field4 = new Bar();
			foo1.FieldUnableNull = "notNull";

			Foo foo2 = new Foo();
			foo2.Field1 = "foo2";
			foo2.Field2 = "bar2";

			sbyte[] bytes1 = jsonSchema.encode(foo1);
			Foo object1 = jsonSchema.decode(bytes1);
			Assert.assertTrue(bytes1.Length > 0);
			assertEquals(object1, foo1);

			try
			{

				jsonSchema.encode(foo2);

			}
			catch (Exception e)
			{
				Assert.assertTrue(e is SchemaSerializationException);
			}

		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testAllowNullNestedClasses()
		public virtual void testAllowNullNestedClasses()
		{
			JSONSchema<NestedBar> jsonSchema = JSONSchema.of(SchemaDefinition.builder<NestedBar>().withPojo(typeof(NestedBar)).build());
			JSONSchema<NestedBarList> listJsonSchema = JSONSchema.of(SchemaDefinition.builder<NestedBarList>().withPojo(typeof(NestedBarList)).build());

			Bar bar = new Bar();
			bar.Field1 = true;

			NestedBar nested = new NestedBar();
			nested.Field1 = true;
			nested.Nested = bar;

			sbyte[] bytes = jsonSchema.encode(nested);
			Assert.assertTrue(bytes.Length > 0);
			Assert.assertEquals(jsonSchema.decode(bytes), nested);

			IList<Bar> list = Collections.singletonList(bar);
			NestedBarList nestedList = new NestedBarList();
			nestedList.Field1 = true;
			nestedList.List = list;

			bytes = listJsonSchema.encode(nestedList);
			Assert.assertTrue(bytes.Length > 0);

			Assert.assertEquals(listJsonSchema.decode(bytes), nestedList);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testNotAllowNullNestedClasses()
		public virtual void testNotAllowNullNestedClasses()
		{
			JSONSchema<NestedBar> jsonSchema = JSONSchema.of(SchemaDefinition.builder<NestedBar>().withPojo(typeof(NestedBar)).withAlwaysAllowNull(false).build());
			JSONSchema<NestedBarList> listJsonSchema = JSONSchema.of(SchemaDefinition.builder<NestedBarList>().withPojo(typeof(NestedBarList)).withAlwaysAllowNull(false).build());

			Bar bar = new Bar();
			bar.Field1 = true;

			NestedBar nested = new NestedBar();
			nested.Field1 = true;
			nested.Nested = bar;

			sbyte[] bytes = jsonSchema.encode(nested);
			Assert.assertTrue(bytes.Length > 0);
			Assert.assertEquals(jsonSchema.decode(bytes), nested);

			IList<Bar> list = Collections.singletonList(bar);
			NestedBarList nestedList = new NestedBarList();
			nestedList.Field1 = true;
			nestedList.List = list;

			bytes = listJsonSchema.encode(nestedList);
			Assert.assertTrue(bytes.Length > 0);

			Assert.assertEquals(listJsonSchema.decode(bytes), nestedList);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testNotAllowNullCorrectPolymorphism()
		public virtual void testNotAllowNullCorrectPolymorphism()
		{
			Bar bar = new Bar();
			bar.Field1 = true;

			DerivedFoo derivedFoo = new DerivedFoo();
			derivedFoo.Field1 = "foo1";
			derivedFoo.Field2 = "bar2";
			derivedFoo.Field3 = 4;
			derivedFoo.Field4 = bar;
			derivedFoo.Field5 = "derived1";
			derivedFoo.Field6 = 2;

			Foo foo = new Foo();
			foo.Field1 = "foo1";
			foo.Field2 = "bar2";
			foo.Field3 = 4;
			foo.Field4 = bar;

			SchemaTestUtils.DerivedDerivedFoo derivedDerivedFoo = new SchemaTestUtils.DerivedDerivedFoo();
			derivedDerivedFoo.Field1 = "foo1";
			derivedDerivedFoo.Field2 = "bar2";
			derivedDerivedFoo.Field3 = 4;
			derivedDerivedFoo.Field4 = bar;
			derivedDerivedFoo.Field5 = "derived1";
			derivedDerivedFoo.Field6 = 2;
			derivedDerivedFoo.Foo2 = foo;
			derivedDerivedFoo.DerivedFoo = derivedFoo;

			// schema for base class
			JSONSchema<Foo> baseJsonSchema = JSONSchema.of(SchemaDefinition.builder<Foo>().withPojo(typeof(Foo)).build());
			Assert.assertEquals(baseJsonSchema.decode(baseJsonSchema.encode(foo)), foo);
			Assert.assertEquals(baseJsonSchema.decode(baseJsonSchema.encode(derivedFoo)), foo);
			Assert.assertEquals(baseJsonSchema.decode(baseJsonSchema.encode(derivedDerivedFoo)), foo);

			// schema for derived class
			JSONSchema<DerivedFoo> derivedJsonSchema = JSONSchema.of(SchemaDefinition.builder<DerivedFoo>().withPojo(typeof(DerivedFoo)).build());
			Assert.assertEquals(derivedJsonSchema.decode(derivedJsonSchema.encode(derivedFoo)), derivedFoo);
			Assert.assertEquals(derivedJsonSchema.decode(derivedJsonSchema.encode(derivedDerivedFoo)), derivedFoo);

			//schema for derived derived class
			JSONSchema<SchemaTestUtils.DerivedDerivedFoo> derivedDerivedJsonSchema = JSONSchema.of(SchemaDefinition.builder<SchemaTestUtils.DerivedDerivedFoo>().withPojo(typeof(SchemaTestUtils.DerivedDerivedFoo)).build());
			Assert.assertEquals(derivedDerivedJsonSchema.decode(derivedDerivedJsonSchema.encode(derivedDerivedFoo)), derivedDerivedFoo);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expectedExceptions = org.apache.pulsar.client.api.SchemaSerializationException.class) public void testAllowNullDecodeWithInvalidContent()
		public virtual void testAllowNullDecodeWithInvalidContent()
		{
			JSONSchema<Foo> jsonSchema = JSONSchema.of(SchemaDefinition.builder<Foo>().withPojo(typeof(Foo)).build());
			jsonSchema.decode(new sbyte[0]);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testAllowNullCorrectPolymorphism()
		public virtual void testAllowNullCorrectPolymorphism()
		{
			Bar bar = new Bar();
			bar.Field1 = true;

			DerivedFoo derivedFoo = new DerivedFoo();
			derivedFoo.Field1 = "foo1";
			derivedFoo.Field2 = "bar2";
			derivedFoo.Field3 = 4;
			derivedFoo.Field4 = bar;
			derivedFoo.Field5 = "derived1";
			derivedFoo.Field6 = 2;

			Foo foo = new Foo();
			foo.Field1 = "foo1";
			foo.Field2 = "bar2";
			foo.Field3 = 4;
			foo.Field4 = bar;

			SchemaTestUtils.DerivedDerivedFoo derivedDerivedFoo = new SchemaTestUtils.DerivedDerivedFoo();
			derivedDerivedFoo.Field1 = "foo1";
			derivedDerivedFoo.Field2 = "bar2";
			derivedDerivedFoo.Field3 = 4;
			derivedDerivedFoo.Field4 = bar;
			derivedDerivedFoo.Field5 = "derived1";
			derivedDerivedFoo.Field6 = 2;
			derivedDerivedFoo.Foo2 = foo;
			derivedDerivedFoo.DerivedFoo = derivedFoo;

			// schema for base class
			JSONSchema<Foo> baseJsonSchema = JSONSchema.of(SchemaDefinition.builder<Foo>().withPojo(typeof(Foo)).withAlwaysAllowNull(false).build());
			Assert.assertEquals(baseJsonSchema.decode(baseJsonSchema.encode(foo)), foo);
			Assert.assertEquals(baseJsonSchema.decode(baseJsonSchema.encode(derivedFoo)), foo);
			Assert.assertEquals(baseJsonSchema.decode(baseJsonSchema.encode(derivedDerivedFoo)), foo);

			// schema for derived class
			JSONSchema<DerivedFoo> derivedJsonSchema = JSONSchema.of(SchemaDefinition.builder<DerivedFoo>().withPojo(typeof(DerivedFoo)).withAlwaysAllowNull(false).build());
			Assert.assertEquals(derivedJsonSchema.decode(derivedJsonSchema.encode(derivedFoo)), derivedFoo);
			Assert.assertEquals(derivedJsonSchema.decode(derivedJsonSchema.encode(derivedDerivedFoo)), derivedFoo);

			//schema for derived derived class
			JSONSchema<SchemaTestUtils.DerivedDerivedFoo> derivedDerivedJsonSchema = JSONSchema.of(SchemaDefinition.builder<SchemaTestUtils.DerivedDerivedFoo>().withPojo(typeof(SchemaTestUtils.DerivedDerivedFoo)).withAlwaysAllowNull(false).build());
			Assert.assertEquals(derivedDerivedJsonSchema.decode(derivedDerivedJsonSchema.encode(derivedDerivedFoo)), derivedDerivedFoo);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expectedExceptions = org.apache.pulsar.client.api.SchemaSerializationException.class) public void testNotAllowNullDecodeWithInvalidContent()
		public virtual void testNotAllowNullDecodeWithInvalidContent()
		{
			JSONSchema<Foo> jsonSchema = JSONSchema.of(SchemaDefinition.builder<Foo>().withPojo(typeof(Foo)).withAlwaysAllowNull(false).build());
			jsonSchema.decode(new sbyte[0]);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testDecodeByteBuf()
		public virtual void testDecodeByteBuf()
		{
			JSONSchema<Foo> jsonSchema = JSONSchema.of(SchemaDefinition.builder<Foo>().withPojo(typeof(Foo)).withAlwaysAllowNull(false).build());

			Foo foo1 = new Foo();
			foo1.Field1 = "foo1";
			foo1.Field2 = "bar1";
			foo1.Field4 = new Bar();
			foo1.FieldUnableNull = "notNull";

			Foo foo2 = new Foo();
			foo2.Field1 = "foo2";
			foo2.Field2 = "bar2";

			sbyte[] bytes1 = jsonSchema.encode(foo1);
			ByteBuf byteBuf = ByteBufAllocator.DEFAULT.buffer(bytes1.Length);
			byteBuf.writeBytes(bytes1);
			Assert.assertTrue(bytes1.Length > 0);
			assertEquals(jsonSchema.decode(byteBuf), foo1);

		}
	}

}