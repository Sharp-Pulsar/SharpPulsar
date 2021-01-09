using SharpPulsar.Interfaces.ISchema;
using SharpPulsar.Schema;
using System;
using System.Collections.Generic;
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
	public class JSONSchemaTest
	{
		[Fact]
		public void TestAllowNullCorrectPolymorphism()
		{
            SchemaTestUtils.Bar bar = new SchemaTestUtils.Bar
            {
                Field1 = true
            };

            SchemaTestUtils.DerivedFoo derivedFoo = new SchemaTestUtils.DerivedFoo
            {
                Field1 = "foo1",
                Field2 = "bar2",
                Field3 = 4,
                Field4 = bar,
                Field5 = "derived1",
                Field6 = 2
            };

            SchemaTestUtils.Foo foo = new SchemaTestUtils.Foo
            {
                Field1 = "foo1",
                Field2 = "bar2",
                Field3 = 4,
                Field4 = bar
            };

            SchemaTestUtils.DerivedDerivedFoo derivedDerivedFoo = new SchemaTestUtils.DerivedDerivedFoo
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
            JSONSchema<SchemaTestUtils.Foo> baseJsonSchema = JSONSchema< SchemaTestUtils.Foo>.Of(ISchemaDefinition<SchemaTestUtils.Foo>.Builder().WithPojo(typeof(SchemaTestUtils.Foo)).WithAlwaysAllowNull(false).Build());
			Assert.Equal(baseJsonSchema.Decode(baseJsonSchema.Encode(foo)), foo);
			Assert.Equal(baseJsonSchema.Decode(baseJsonSchema.Encode(derivedFoo)), foo);
			Assert.Equal(baseJsonSchema.Decode(baseJsonSchema.Encode(derivedDerivedFoo)), foo);

			// schema for derived class
			JSONSchema<SchemaTestUtils.DerivedFoo> derivedJsonSchema = JSONSchema<SchemaTestUtils.DerivedFoo>.Of(ISchemaDefinition<SchemaTestUtils.DerivedFoo>.Builder().WithPojo(typeof(SchemaTestUtils.DerivedFoo)).WithAlwaysAllowNull(false).Build());
			Assert.Equal(derivedJsonSchema.Decode(derivedJsonSchema.Encode(derivedFoo)), derivedFoo);
			Assert.Equal(derivedJsonSchema.Decode(derivedJsonSchema.Encode(derivedDerivedFoo)), derivedFoo);

			//schema for derived derived class
			JSONSchema<SchemaTestUtils.DerivedDerivedFoo> derivedDerivedJsonSchema = JSONSchema<SchemaTestUtils.DerivedDerivedFoo>.Of(ISchemaDefinition<SchemaTestUtils.DerivedDerivedFoo>.Builder().WithPojo(typeof(SchemaTestUtils.DerivedDerivedFoo)).WithAlwaysAllowNull(false).Build());
			Assert.Equal(derivedDerivedJsonSchema.Decode(derivedDerivedJsonSchema.Encode(derivedDerivedFoo)), derivedDerivedFoo);
		}

	}

}