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
namespace Org.Apache.Pulsar.Client.Impl.Schema
{

	using ByteBuf = io.netty.buffer.ByteBuf;
	using ByteBufAllocator = io.netty.buffer.ByteBufAllocator;
	using Slf4j = lombok.@extern.slf4j.Slf4j;
	using Schema = org.apache.avro.Schema;
	using SchemaSerializationException = org.apache.pulsar.client.api.SchemaSerializationException;
	using SchemaDefinition = org.apache.pulsar.client.api.schema.SchemaDefinition;
	using Bar = Org.Apache.Pulsar.Client.Impl.Schema.SchemaTestUtils.Bar;
	using DerivedFoo = Org.Apache.Pulsar.Client.Impl.Schema.SchemaTestUtils.DerivedFoo;
	using Foo = Org.Apache.Pulsar.Client.Impl.Schema.SchemaTestUtils.Foo;
	using NestedBar = Org.Apache.Pulsar.Client.Impl.Schema.SchemaTestUtils.NestedBar;
	using NestedBarList = Org.Apache.Pulsar.Client.Impl.Schema.SchemaTestUtils.NestedBarList;
	using SchemaType = org.apache.pulsar.common.schema.SchemaType;
	using JSONAssert = org.skyscreamer.jsonassert.JSONAssert;
	using Assert = org.testng.Assert;
	using Test = org.testng.annotations.Test;
	using JSONException = org.json.JSONException;

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

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static void assertJSONEqual(String s1, String s2) throws org.json.JSONException
		public static void AssertJSONEqual(string S1, string S2)
		{
			JSONAssert.assertEquals(S1, S2, false);
		}
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testNotAllowNullSchema() throws org.json.JSONException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		public virtual void TestNotAllowNullSchema()
		{
			JSONSchema<Foo> JsonSchema = JSONSchema.of(SchemaDefinition.builder<Foo>().withPojo(typeof(Foo)).withAlwaysAllowNull(false).build());
			Assert.assertEquals(JsonSchema.SchemaInfo.Type, SchemaType.JSON);
			Schema.Parser Parser = new Schema.Parser();
			string SchemaJson = new string(JsonSchema.SchemaInfo.Schema);
			AssertJSONEqual(SchemaJson, SCHEMA_JSON_NOT_ALLOW_NULL);
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
			JSONSchema<Foo> JsonSchema = JSONSchema.of(SchemaDefinition.builder<Foo>().withPojo(typeof(Foo)).build());
			Assert.assertEquals(JsonSchema.SchemaInfo.Type, SchemaType.JSON);
			Schema.Parser Parser = new Schema.Parser();
			Parser.ValidateDefaults = false;
			string SchemaJson = new string(JsonSchema.SchemaInfo.Schema);
			AssertJSONEqual(SchemaJson, SCHEMA_JSON_ALLOW_NULL);
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
//ORIGINAL LINE: @Test public void testAllowNullEncodeAndDecode()
		public virtual void TestAllowNullEncodeAndDecode()
		{
			JSONSchema<Foo> JsonSchema = JSONSchema.of(SchemaDefinition.builder<Foo>().withPojo(typeof(Foo)).build());

			Bar Bar = new Bar();
			Bar.Field1 = true;

			Foo Foo1 = new Foo();
			Foo1.Field1 = "foo1";
			Foo1.Field2 = "bar1";
			Foo1.Field4 = Bar;
			Foo1.Color = SchemaTestUtils.Color.BLUE;

			Foo Foo2 = new Foo();
			Foo2.Field1 = "foo2";
			Foo2.Field2 = "bar2";

			sbyte[] Bytes1 = JsonSchema.encode(Foo1);
			Assert.assertTrue(Bytes1.Length > 0);

			sbyte[] Bytes2 = JsonSchema.encode(Foo2);
			Assert.assertTrue(Bytes2.Length > 0);

			Foo Object1 = JsonSchema.decode(Bytes1);
			Foo Object2 = JsonSchema.decode(Bytes2);

			Assert.assertEquals(Object1, Foo1);
			Assert.assertEquals(Object2, Foo2);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testNotAllowNullEncodeAndDecode()
		public virtual void TestNotAllowNullEncodeAndDecode()
		{
			JSONSchema<Foo> JsonSchema = JSONSchema.of(SchemaDefinition.builder<Foo>().withPojo(typeof(Foo)).withAlwaysAllowNull(false).build());

			Foo Foo1 = new Foo();
			Foo1.Field1 = "foo1";
			Foo1.Field2 = "bar1";
			Foo1.Field4 = new Bar();
			Foo1.FieldUnableNull = "notNull";

			Foo Foo2 = new Foo();
			Foo2.Field1 = "foo2";
			Foo2.Field2 = "bar2";

			sbyte[] Bytes1 = JsonSchema.encode(Foo1);
			Foo Object1 = JsonSchema.decode(Bytes1);
			Assert.assertTrue(Bytes1.Length > 0);
			assertEquals(Object1, Foo1);

			try
			{

				JsonSchema.encode(Foo2);

			}
			catch (Exception E)
			{
				Assert.assertTrue(E is SchemaSerializationException);
			}

		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testAllowNullNestedClasses()
		public virtual void TestAllowNullNestedClasses()
		{
			JSONSchema<NestedBar> JsonSchema = JSONSchema.of(SchemaDefinition.builder<NestedBar>().withPojo(typeof(NestedBar)).build());
			JSONSchema<NestedBarList> ListJsonSchema = JSONSchema.of(SchemaDefinition.builder<NestedBarList>().withPojo(typeof(NestedBarList)).build());

			Bar Bar = new Bar();
			Bar.Field1 = true;

			NestedBar Nested = new NestedBar();
			Nested.Field1 = true;
			Nested.Nested = Bar;

			sbyte[] Bytes = JsonSchema.encode(Nested);
			Assert.assertTrue(Bytes.Length > 0);
			Assert.assertEquals(JsonSchema.decode(Bytes), Nested);

			IList<Bar> List = Collections.singletonList(Bar);
			NestedBarList NestedList = new NestedBarList();
			NestedList.Field1 = true;
			NestedList.List = List;

			Bytes = ListJsonSchema.encode(NestedList);
			Assert.assertTrue(Bytes.Length > 0);

			Assert.assertEquals(ListJsonSchema.decode(Bytes), NestedList);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testNotAllowNullNestedClasses()
		public virtual void TestNotAllowNullNestedClasses()
		{
			JSONSchema<NestedBar> JsonSchema = JSONSchema.of(SchemaDefinition.builder<NestedBar>().withPojo(typeof(NestedBar)).withAlwaysAllowNull(false).build());
			JSONSchema<NestedBarList> ListJsonSchema = JSONSchema.of(SchemaDefinition.builder<NestedBarList>().withPojo(typeof(NestedBarList)).withAlwaysAllowNull(false).build());

			Bar Bar = new Bar();
			Bar.Field1 = true;

			NestedBar Nested = new NestedBar();
			Nested.Field1 = true;
			Nested.Nested = Bar;

			sbyte[] Bytes = JsonSchema.encode(Nested);
			Assert.assertTrue(Bytes.Length > 0);
			Assert.assertEquals(JsonSchema.decode(Bytes), Nested);

			IList<Bar> List = Collections.singletonList(Bar);
			NestedBarList NestedList = new NestedBarList();
			NestedList.Field1 = true;
			NestedList.List = List;

			Bytes = ListJsonSchema.encode(NestedList);
			Assert.assertTrue(Bytes.Length > 0);

			Assert.assertEquals(ListJsonSchema.decode(Bytes), NestedList);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testNotAllowNullCorrectPolymorphism()
		public virtual void TestNotAllowNullCorrectPolymorphism()
		{
			Bar Bar = new Bar();
			Bar.Field1 = true;

			DerivedFoo DerivedFoo = new DerivedFoo();
			DerivedFoo.Field1 = "foo1";
			DerivedFoo.Field2 = "bar2";
			DerivedFoo.Field3 = 4;
			DerivedFoo.Field4 = Bar;
			DerivedFoo.Field5 = "derived1";
			DerivedFoo.Field6 = 2;

			Foo Foo = new Foo();
			Foo.Field1 = "foo1";
			Foo.Field2 = "bar2";
			Foo.Field3 = 4;
			Foo.Field4 = Bar;

			SchemaTestUtils.DerivedDerivedFoo DerivedDerivedFoo = new SchemaTestUtils.DerivedDerivedFoo();
			DerivedDerivedFoo.Field1 = "foo1";
			DerivedDerivedFoo.Field2 = "bar2";
			DerivedDerivedFoo.Field3 = 4;
			DerivedDerivedFoo.Field4 = Bar;
			DerivedDerivedFoo.Field5 = "derived1";
			DerivedDerivedFoo.Field6 = 2;
			DerivedDerivedFoo.Foo2 = Foo;
			DerivedDerivedFoo.DerivedFoo = DerivedFoo;

			// schema for base class
			JSONSchema<Foo> BaseJsonSchema = JSONSchema.of(SchemaDefinition.builder<Foo>().withPojo(typeof(Foo)).build());
			Assert.assertEquals(BaseJsonSchema.decode(BaseJsonSchema.encode(Foo)), Foo);
			Assert.assertEquals(BaseJsonSchema.decode(BaseJsonSchema.encode(DerivedFoo)), Foo);
			Assert.assertEquals(BaseJsonSchema.decode(BaseJsonSchema.encode(DerivedDerivedFoo)), Foo);

			// schema for derived class
			JSONSchema<DerivedFoo> DerivedJsonSchema = JSONSchema.of(SchemaDefinition.builder<DerivedFoo>().withPojo(typeof(DerivedFoo)).build());
			Assert.assertEquals(DerivedJsonSchema.decode(DerivedJsonSchema.encode(DerivedFoo)), DerivedFoo);
			Assert.assertEquals(DerivedJsonSchema.decode(DerivedJsonSchema.encode(DerivedDerivedFoo)), DerivedFoo);

			//schema for derived derived class
			JSONSchema<SchemaTestUtils.DerivedDerivedFoo> DerivedDerivedJsonSchema = JSONSchema.of(SchemaDefinition.builder<SchemaTestUtils.DerivedDerivedFoo>().withPojo(typeof(SchemaTestUtils.DerivedDerivedFoo)).build());
			Assert.assertEquals(DerivedDerivedJsonSchema.decode(DerivedDerivedJsonSchema.encode(DerivedDerivedFoo)), DerivedDerivedFoo);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expectedExceptions = org.apache.pulsar.client.api.SchemaSerializationException.class) public void testAllowNullDecodeWithInvalidContent()
		public virtual void TestAllowNullDecodeWithInvalidContent()
		{
			JSONSchema<Foo> JsonSchema = JSONSchema.of(SchemaDefinition.builder<Foo>().withPojo(typeof(Foo)).build());
			JsonSchema.decode(new sbyte[0]);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testAllowNullCorrectPolymorphism()
		public virtual void TestAllowNullCorrectPolymorphism()
		{
			Bar Bar = new Bar();
			Bar.Field1 = true;

			DerivedFoo DerivedFoo = new DerivedFoo();
			DerivedFoo.Field1 = "foo1";
			DerivedFoo.Field2 = "bar2";
			DerivedFoo.Field3 = 4;
			DerivedFoo.Field4 = Bar;
			DerivedFoo.Field5 = "derived1";
			DerivedFoo.Field6 = 2;

			Foo Foo = new Foo();
			Foo.Field1 = "foo1";
			Foo.Field2 = "bar2";
			Foo.Field3 = 4;
			Foo.Field4 = Bar;

			SchemaTestUtils.DerivedDerivedFoo DerivedDerivedFoo = new SchemaTestUtils.DerivedDerivedFoo();
			DerivedDerivedFoo.Field1 = "foo1";
			DerivedDerivedFoo.Field2 = "bar2";
			DerivedDerivedFoo.Field3 = 4;
			DerivedDerivedFoo.Field4 = Bar;
			DerivedDerivedFoo.Field5 = "derived1";
			DerivedDerivedFoo.Field6 = 2;
			DerivedDerivedFoo.Foo2 = Foo;
			DerivedDerivedFoo.DerivedFoo = DerivedFoo;

			// schema for base class
			JSONSchema<Foo> BaseJsonSchema = JSONSchema.of(SchemaDefinition.builder<Foo>().withPojo(typeof(Foo)).withAlwaysAllowNull(false).build());
			Assert.assertEquals(BaseJsonSchema.decode(BaseJsonSchema.encode(Foo)), Foo);
			Assert.assertEquals(BaseJsonSchema.decode(BaseJsonSchema.encode(DerivedFoo)), Foo);
			Assert.assertEquals(BaseJsonSchema.decode(BaseJsonSchema.encode(DerivedDerivedFoo)), Foo);

			// schema for derived class
			JSONSchema<DerivedFoo> DerivedJsonSchema = JSONSchema.of(SchemaDefinition.builder<DerivedFoo>().withPojo(typeof(DerivedFoo)).withAlwaysAllowNull(false).build());
			Assert.assertEquals(DerivedJsonSchema.decode(DerivedJsonSchema.encode(DerivedFoo)), DerivedFoo);
			Assert.assertEquals(DerivedJsonSchema.decode(DerivedJsonSchema.encode(DerivedDerivedFoo)), DerivedFoo);

			//schema for derived derived class
			JSONSchema<SchemaTestUtils.DerivedDerivedFoo> DerivedDerivedJsonSchema = JSONSchema.of(SchemaDefinition.builder<SchemaTestUtils.DerivedDerivedFoo>().withPojo(typeof(SchemaTestUtils.DerivedDerivedFoo)).withAlwaysAllowNull(false).build());
			Assert.assertEquals(DerivedDerivedJsonSchema.decode(DerivedDerivedJsonSchema.encode(DerivedDerivedFoo)), DerivedDerivedFoo);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expectedExceptions = org.apache.pulsar.client.api.SchemaSerializationException.class) public void testNotAllowNullDecodeWithInvalidContent()
		public virtual void TestNotAllowNullDecodeWithInvalidContent()
		{
			JSONSchema<Foo> JsonSchema = JSONSchema.of(SchemaDefinition.builder<Foo>().withPojo(typeof(Foo)).withAlwaysAllowNull(false).build());
			JsonSchema.decode(new sbyte[0]);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testDecodeByteBuf()
		public virtual void TestDecodeByteBuf()
		{
			JSONSchema<Foo> JsonSchema = JSONSchema.of(SchemaDefinition.builder<Foo>().withPojo(typeof(Foo)).withAlwaysAllowNull(false).build());

			Foo Foo1 = new Foo();
			Foo1.Field1 = "foo1";
			Foo1.Field2 = "bar1";
			Foo1.Field4 = new Bar();
			Foo1.FieldUnableNull = "notNull";

			Foo Foo2 = new Foo();
			Foo2.Field1 = "foo2";
			Foo2.Field2 = "bar2";

			sbyte[] Bytes1 = JsonSchema.encode(Foo1);
			ByteBuf ByteBuf = ByteBufAllocator.DEFAULT.buffer(Bytes1.Length);
			ByteBuf.writeBytes(Bytes1);
			Assert.assertTrue(Bytes1.Length > 0);
			assertEquals(JsonSchema.decode(ByteBuf), Foo1);

		}
	}

}