﻿using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json;
using Avro;
using DotNetty.Buffers;
using SharpPulsar.Api.Schema;
using SharpPulsar.Exceptions;
using SharpPulsar.Impl.Schema;
using SharpPulsar.Shared;
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
namespace SharpPulsar.Test.Impl.schema
{
    using Bar = SchemaTestUtils.Bar;
	using DerivedFoo = SchemaTestUtils.DerivedFoo;
	using Foo = SchemaTestUtils.Foo;
	using NestedBar = SchemaTestUtils.NestedBar;
	using NestedBarList = SchemaTestUtils.NestedBarList;

	public class JsonSchemaTest
	{
		[Fact]
		public void TestNotAllowNullSchema()
		{
			var jsonSchema = JsonSchema<Foo>.Of(ISchemaDefinition<Foo>.Builder().WithPojo(new Foo()).WithAlwaysAllowNull(false).Build());
			Assert.Equal(SchemaType.Json,jsonSchema.SchemaInfo.Type);
			
			var schemaJson = new string(Encoding.UTF8.GetString((byte[])(object)jsonSchema.SchemaInfo.Schema));
            Assert.Equal(SchemaTestUtils.SchemaJsonNotAllowNull, schemaJson);
			var schema = Schema.Parse(schemaJson);

			foreach (var fieldName in SchemaTestUtils.FooFields)
			{
				var field = schema.GetProperty(fieldName);
				Assert.NotNull(field);
				
				/*if (fieldName.Equals("field4"))
				{
					Assert.NotNull(field.schema().Types.get(1).getField("field1"));
				}
				if (field.name().Equals("fieldUnableNull"))
				{
					Assert.assertNotNull(field.schema().Type);
				}*/
			}
		}

		[Fact]
		public void TestAllowNullSchema()
		{
            var jsonSchema = JsonSchema<Foo>.Of(ISchemaDefinition<Foo>.Builder().WithPojo(new Foo()).Build());
			Assert.Equal(SchemaType.Json, jsonSchema.SchemaInfo.Type);
			var schemaJson = new string(Encoding.UTF8.GetString((byte[])(object)jsonSchema.SchemaInfo.Schema));
			Assert.Equal(schemaJson, SchemaTestUtils.SchemaJsonAllowNull);
			var schema = Schema.Parse(schemaJson);

			foreach (var fieldName in SchemaTestUtils.FooFields)
			{
                var field = schema.GetProperty(fieldName);
                Assert.NotNull(field);

				/*if (field.name().Equals("field4"))
				{
					Assert.assertNotNull(field.schema().Types.get(1).getField("field1"));
				}
				if (field.name().Equals("fieldUnableNull"))
				{
					Assert.assertNotNull(field.schema().Type);
				}*/
			}
		}

		[Fact]
		public void TestAllowNullEncodeAndDecode()
		{
			var jsonSchema = JsonSchema<Foo>.Of(ISchemaDefinition<Foo>.Builder().WithPojo(new Foo()).Build());

            var bar = new Bar {Field1 = true};

            var foo1 = new Foo {Field1 = "foo1", Field2 = "bar1", Field4 = bar, Color = SchemaTestUtils.Color.Blue};

            var foo2 = new Foo {Field1 = "foo2", Field2 = "bar2"};

            var bytes1 = jsonSchema.Encode(foo1);
			Assert.True(bytes1.Length > 0);

			var bytes2 = jsonSchema.Encode(foo2);
			Assert.True(bytes2.Length > 0);

			var object1 = jsonSchema.Decode(bytes1);
			var object2 = jsonSchema.Decode(bytes2);

			Assert.Equal(foo1,object1);
			Assert.Equal(foo2, object2);
		}

		[Fact]
		public void TestNotAllowNullEncodeAndDecode()
		{
			JsonSchema<Foo> jsonSchema = JsonSchema<Foo>.Of(ISchemaDefinition<Foo>.Builder().WithPojo(new Foo()).WithAlwaysAllowNull(false).Build());

			var foo1 = new Foo();
			foo1.Field1 = "foo1";
			foo1.Field2 = "bar1";
			foo1.Field4 = new Bar();
			foo1.FieldUnableNull = "notNull";

			var foo2 = new Foo();
			foo2.Field1 = "foo2";
			foo2.Field2 = "bar2";

			sbyte[] bytes1 = jsonSchema.Encode(foo1);
			Foo object1 = jsonSchema.Decode(bytes1);
			Assert.True(bytes1.Length > 0);
			Assert.Equal(foo1, object1);

			try
			{

				jsonSchema.Encode(foo2);

			}
			catch (System.Exception e)
			{
				Assert.True(e is SchemaSerializationException);
			}

		}

		[Fact]
		public void TestAllowNullNestedClasses()
		{
			JsonSchema<NestedBar> jsonSchema = JsonSchema<NestedBar>.Of(ISchemaDefinition<NestedBar>.Builder().WithPojo(new NestedBar()).Build());
			JsonSchema<NestedBarList> listJsonSchema = JsonSchema<NestedBarList>.Of(ISchemaDefinition<NestedBarList>.Builder().WithPojo(new NestedBarList()).Build());

            var bar = new Bar {Field1 = true};

            var nested = new NestedBar {Field1 = true, Nested = bar};

            sbyte[] bytes = jsonSchema.Encode(nested);
			Assert.True(bytes.Length > 0);
			Assert.Equal( nested, jsonSchema.Decode(bytes));

            List<Bar> list = new List<Bar>{ bar};
            var nestedList = new NestedBarList {Field1 = true, List = list};

            bytes = listJsonSchema.Encode(nestedList);
			Assert.True(bytes.Length > 0);

			Assert.Equal( nestedList, listJsonSchema.Decode(bytes));
		}
		[Fact]
		public void TestNotAllowNullNestedClasses()
		{
			JsonSchema<NestedBar> jsonSchema = JsonSchema<NestedBar>.Of(ISchemaDefinition<NestedBar>.Builder().WithPojo(new NestedBar()).WithAlwaysAllowNull(false).Build());
			JsonSchema<NestedBarList> listJsonSchema = JsonSchema<NestedBarList>.Of(ISchemaDefinition<NestedBarList>.Builder().WithPojo(new NestedBarList()).WithAlwaysAllowNull(false).Build());

            var bar = new Bar {Field1 = true};

            var nested = new NestedBar {Field1 = true, Nested = bar};

            sbyte[] bytes = jsonSchema.Encode(nested);
			Assert.True(bytes.Length > 0);
			Assert.Equal( nested, jsonSchema.Decode(bytes));

			List<Bar> list = new List<Bar> { bar };
            var nestedList = new NestedBarList {Field1 = true, List = list};

            bytes = listJsonSchema.Encode(nestedList);
			Assert.True(bytes.Length > 0);

			Assert.Equal( nestedList, listJsonSchema.Decode(bytes));
		}
        [Fact]
		public void TestNotAllowNullCorrectPolymorphism()
		{
            var bar = new Bar {Field1 = true};

            var derivedFoo = new DerivedFoo
            {
                Field1 = "foo1",
                Field2 = "bar2",
                Field3 = 4,
                Field4 = bar,
                Field5 = "derived1",
                Field6 = 2
            };

            var foo = new Foo {Field1 = "foo1", Field2 = "bar2", Field3 = 4, Field4 = bar};

            var derivedDerivedFoo = new SchemaTestUtils.DerivedDerivedFoo
            {
                Field1 = "foo1",
                Field2 = "bar2",
                Field3 = 4,
                Field4 = bar,
                Field5 = "derived1",
                Field6 = 2,
                Foo2 = foo,
                DerivedFoo = derivedFoo
            };

            // schema for base class
			JsonSchema<Foo> baseJsonSchema = JsonSchema<Foo>.Of(ISchemaDefinition<Foo>.Builder().WithPojo(new Foo()).Build());
			Assert.Equal(foo, baseJsonSchema.Decode(baseJsonSchema.Encode(foo)));
            Assert.Equal(foo,baseJsonSchema.Decode(baseJsonSchema.Encode(derivedFoo)));
            Assert.Equal(foo,baseJsonSchema.Decode(baseJsonSchema.Encode(derivedDerivedFoo)));

			// schema for derived class
			JsonSchema<DerivedFoo> derivedJsonSchema = JsonSchema<DerivedFoo>.Of(ISchemaDefinition<DerivedFoo>.Builder().WithPojo(new DerivedFoo()).Build());
            Assert.Equal(derivedFoo,derivedJsonSchema.Decode(derivedJsonSchema.Encode(derivedFoo)));
            Assert.Equal(derivedFoo,derivedJsonSchema.Decode(derivedJsonSchema.Encode(derivedDerivedFoo)));

			//schema for derived derived class
			JsonSchema<SchemaTestUtils.DerivedDerivedFoo> derivedDerivedJsonSchema = JsonSchema<SchemaTestUtils.DerivedDerivedFoo>.Of(ISchemaDefinition<SchemaTestUtils.DerivedDerivedFoo>.Builder().WithPojo(new SchemaTestUtils.DerivedDerivedFoo()).Build());
			Assert.Equal(derivedDerivedJsonSchema.Decode(derivedDerivedJsonSchema.Encode(derivedDerivedFoo)), derivedDerivedFoo);
		}
		[Fact]
		public void TestAllowNullDecodeWithInvalidContentWithJsonDef()
		{
             const string _logMessageSchemaV1 = @"
            {
                ""namespace"": ""MessageTypes"",
                ""type"": ""record"",
                ""doc"": ""A simple log message type as used by this blog post."",
                ""name"": ""LogMessage"",
                ""fields"": [
                    { ""name"": ""IP"", ""type"": ""string"" },
                    { ""name"": ""Message"", ""type"": ""string"" },
                    { ""name"": ""TimeStamp"", ""type"": ""long"" },
                    { ""name"": ""Tags"",""type"":
                        { ""type"": ""map"",
                            ""values"": ""string""},
                            ""default"": {}},
                    { ""name"": ""Severity"",
                    ""type"": { ""namespace"": ""MessageTypes"",
                        ""type"": ""enum"",
                        ""doc"": ""Enumerates the set of allowable log levels."",
                        ""name"": ""LogLevel"",
                        ""symbols"": [""None"", ""Verbose"", ""Info"", ""Warning"", ""Error""]}}
                ]
            }";
		    JsonSchema<Foo> jsonSchema = JsonSchema<Foo>.Of(ISchemaDefinition<Foo>.Builder().WithJsonDef(_logMessageSchemaV1).Build());
			Assert.Throws<SerializationException>(()=>jsonSchema.Decode(new sbyte[0]));
		}
        [Fact]
        public void TestAllowNullDecodeWithInvalidContentWithJsonPojo()
        {
            Assert.Throws<Exception>(() => JsonSchema<Foo>.Of(ISchemaDefinition<Foo>.Builder().WithPojo(new Foo()).Build()));
        }

		[Fact]
		public void TestDecodeByteBuf()
		{
			JsonSchema<Foo> jsonSchema = JsonSchema<Foo>.Of(ISchemaDefinition<Foo>.Builder().WithPojo(new Foo()).WithAlwaysAllowNull(false).Build());

			var foo1 = new Foo();
			foo1.Field1 = "foo1";
			foo1.Field2 = "bar1";
			foo1.Field4 = new Bar();
			foo1.FieldUnableNull = "notNull";

			var foo2 = new Foo();
			foo2.Field1 = "foo2";
			foo2.Field2 = "bar2";

			sbyte[] bytes1 = jsonSchema.Encode(foo1);
			var byteBuf = UnpooledByteBufferAllocator.Default.Buffer(bytes1.Length);
			byteBuf.WriteBytes((byte[])(object)bytes1);
			Assert.True(bytes1.Length > 0);
			Assert.Equal( foo1, jsonSchema.Decode(byteBuf));

		}
	}

}