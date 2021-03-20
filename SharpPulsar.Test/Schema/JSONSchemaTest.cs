using SharpPulsar.Exceptions;
using SharpPulsar.Interfaces.ISchema;
using SharpPulsar.Schemas;
using SharpPulsar.Shared;
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
    [Collection("SchemaSpec")]
    public class JSONSchemaTest
    {
        [Fact]
        public virtual void TestNotAllowNullSchema()
        {
            JSONSchema<Foo> jsonSchema = JSONSchema<Foo>.Of(ISchemaDefinition<Foo>.Builder().WithPojo(typeof(Foo)).WithAlwaysAllowNull(false).Build());
            Assert.Equal(jsonSchema.SchemaInfo.Type, SchemaType.JSON);
            string schemaJson = jsonSchema.SchemaInfo.SchemaDefinition;
            Avro.Schema schema = Avro.Schema.Parse(schemaJson);
            Assert.NotNull(schema);
        }
        [Fact]
        public virtual void TestAllowNullEncodeAndDecode()
        {
            JSONSchema<Foo> jsonSchema = JSONSchema<Foo>.Of(ISchemaDefinition<Foo>.Builder().WithPojo(typeof(Foo)).Build());

            Bar bar = new Bar
            {
                Field1 = true
            };

            Foo foo1 = new Foo
            {
                Field1 = "foo1",
                Field2 = "bar1",
                Field4 = bar,
                Color = Color.BLUE
            };

            Foo foo2 = new Foo
            {
                Field1 = "foo2",
                Field2 = "bar2"
            };

            sbyte[] bytes1 = jsonSchema.Encode(foo1);
            Assert.True(bytes1.Length > 0);

            sbyte[] bytes2 = jsonSchema.Encode(foo2);
            Assert.True(bytes2.Length > 0);

            Foo object1 = jsonSchema.Decode(bytes1);
            Foo object2 = jsonSchema.Decode(bytes2);

            Assert.True(object1.Equals(foo1));
            Assert.True(object2.Equals(foo2));
        }
        [Fact]
        public virtual void TestNotAllowNullEncodeAndDecode()
        {
            JSONSchema<Foo> jsonSchema = JSONSchema<Foo>.Of(ISchemaDefinition<Foo>.Builder().WithPojo(typeof(Foo)).WithAlwaysAllowNull(false).Build());

            Foo foo1 = new Foo
            {
                Field1 = "foo1",
                Field2 = "bar1",
                Field4 = new Bar()
            };

            Foo foo2 = new Foo
            {
                Field1 = "foo2",
                Field2 = "bar2"
            };

            sbyte[] bytes1 = jsonSchema.Encode(foo1);
            Foo object1 = jsonSchema.Decode(bytes1);
            Assert.True(bytes1.Length > 0);
            Assert.True(object1.Equals(foo1));

            try
            {

                jsonSchema.Encode(foo2);

            }
            catch (Exception e)
            {
                Assert.True(e is SchemaSerializationException);
            }

        }

    }

}