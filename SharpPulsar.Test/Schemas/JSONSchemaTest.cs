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
namespace SharpPulsar.Test.Schema
{
	public class JSONSchemaTest
	{

		public static void AssertJSONEqual(string S1, string S2)
		{
			JSONAssert.assertEquals(S1, S2, false);
		}
		
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

		public virtual void TestNotAllowNullDecodeWithInvalidContent()
		{
			JSONSchema<Foo> JsonSchema = JSONSchema.of(SchemaDefinition.builder<Foo>().withPojo(typeof(Foo)).withAlwaysAllowNull(false).build());
			JsonSchema.decode(new sbyte[0]);
		}

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