using SharpPulsar.Exceptions;
using SharpPulsar.Interfaces;
using SharpPulsar.Interfaces.ISchema;
using SharpPulsar.Schemas;
using SharpPulsar.Shared;
using System.Collections.Generic;
using System.Text;

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
namespace SharpPulsar.Test.API.Schema
{
    
    public class KeyValueSchemaTest
    {
        [Fact]
        public virtual void TestFillParametersToSchemainfo()
        {
            var fooSchema = AvroSchema<Foo>.Of(ISchemaDefinition<Foo>.Builder().WithPojo(typeof(Foo)).Build());
            var barSchema = AvroSchema<Bar>.Of(ISchemaDefinition<Bar>.Builder().WithPojo(typeof(Bar)).Build());

            fooSchema.SchemaInfo.Name = "foo";
            fooSchema.SchemaInfo.Type = SchemaType.AVRO;
            IDictionary<string, string> keyProperties = new Dictionary<string, string>
            {
                ["foo.key1"] = "value",
                ["foo.key2"] = "value"
            };
            fooSchema.SchemaInfo.Properties = keyProperties;
            barSchema.SchemaInfo.Name = "bar";
            barSchema.SchemaInfo.Type = SchemaType.AVRO;
            IDictionary<string, string> valueProperties = new Dictionary<string, string>
            {
                ["bar.key"] = "key"
            };
            barSchema.SchemaInfo.Properties = valueProperties;
            var keyValueSchema1 = ISchema<object>.KeyValue(fooSchema, barSchema);

            Assert.Equal("foo", keyValueSchema1.SchemaInfo.Properties["key.schema.name"]);
            Assert.Equal(SchemaType.AVRO.ToString(), keyValueSchema1.SchemaInfo.Properties["key.schema.type"]);
            Assert.Equal(@"{""foo.key1"":""value"",""foo.key2"":""value""}", keyValueSchema1.SchemaInfo.Properties["key.schema.properties"]);
            Assert.Equal("bar", keyValueSchema1.SchemaInfo.Properties["value.schema.name"]);
            Assert.Equal(SchemaType.AVRO.ToString(), keyValueSchema1.SchemaInfo.Properties["value.schema.type"]);
            Assert.Equal(@"{""bar.key"":""key""}", keyValueSchema1.SchemaInfo.Properties["value.schema.properties"]);
        }

        [Fact]
        public virtual void TestAvroSchemaCreate()
        {
            var fooSchema = AvroSchema<Foo>.Of(ISchemaDefinition<Foo>.Builder().WithPojo(typeof(Foo)).WithAlwaysAllowNull(false).Build());
            var barSchema = AvroSchema<Bar>.Of(ISchemaDefinition<Bar>.Builder().WithPojo(typeof(Bar)).WithAlwaysAllowNull(false).Build());

            var keyValueSchema1 = ISchema<object>.KeyValue(fooSchema, barSchema);
            var keyValueSchema2 = ISchema<object>.KeyValue(AvroSchema<Foo>.Of(ISchemaDefinition<Foo>.Builder().WithPojo(typeof(Foo)).WithAlwaysAllowNull(false).Build()), AvroSchema<Bar>.Of(ISchemaDefinition<Bar>.Builder().WithPojo(typeof(Bar)).WithAlwaysAllowNull(false).Build()));

            Assert.Equal(keyValueSchema1.SchemaInfo.Type, SchemaType.KeyValue);
            Assert.Equal(keyValueSchema2.SchemaInfo.Type, SchemaType.KeyValue);

            Assert.Equal(((KeyValueSchema<Foo, Bar>)keyValueSchema1).KeySchema.SchemaInfo.Type, SchemaType.AVRO);
            Assert.Equal(((KeyValueSchema<Foo, Bar>)keyValueSchema1).ValueSchema.SchemaInfo.Type, SchemaType.AVRO);
            Assert.Equal(((KeyValueSchema<Foo, Bar>)keyValueSchema2).KeySchema.SchemaInfo.Type, SchemaType.AVRO);
            Assert.Equal(((KeyValueSchema<Foo, Bar>)keyValueSchema2).ValueSchema.SchemaInfo.Type, SchemaType.AVRO);

            var schemaInfo1 = Encoding.UTF8.GetString(keyValueSchema1.SchemaInfo.Schema);
            var schemaInfo2 = Encoding.UTF8.GetString(keyValueSchema2.SchemaInfo.Schema);
            Assert.Equal(schemaInfo1, schemaInfo2);
        }

        [Fact]
        public virtual void TestJsonSchemaCreate()
        {
            var fooSchema = JSONSchema<Foo>.Of(ISchemaDefinition<Foo>.Builder().WithPojo(typeof(Foo)).WithAlwaysAllowNull(false).Build());
            var barSchema = JSONSchema<Bar>.Of(ISchemaDefinition<Bar>.Builder().WithPojo(typeof(Bar)).WithAlwaysAllowNull(false).Build());

            var keyValueSchema1 = ISchema<object>.KeyValue(fooSchema, barSchema);
            var keyValueSchema2 = ISchema<object>.KeyValue(JSONSchema<Foo>.Of(ISchemaDefinition<Foo>.Builder().WithPojo(typeof(Foo)).WithAlwaysAllowNull(false).Build()), JSONSchema<Bar>.Of(ISchemaDefinition<Bar>.Builder().WithPojo(typeof(Bar)).WithAlwaysAllowNull(false).Build()));

            var keyValueSchema3 = ISchema<object>.KeyValue(JSONSchema<Foo>.Of(ISchemaDefinition<Foo>.Builder().WithPojo(typeof(Foo)).WithAlwaysAllowNull(false).Build()), JSONSchema<Bar>.Of(ISchemaDefinition<Bar>.Builder().WithPojo(typeof(Bar)).WithAlwaysAllowNull(false).Build()));

            Assert.Equal(keyValueSchema1.SchemaInfo.Type, SchemaType.KeyValue);
            Assert.Equal(keyValueSchema2.SchemaInfo.Type, SchemaType.KeyValue);
            Assert.Equal(keyValueSchema3.SchemaInfo.Type, SchemaType.KeyValue);

            Assert.Equal(((KeyValueSchema<Foo, Bar>)keyValueSchema1).KeySchema.SchemaInfo.Type, SchemaType.JSON);
            Assert.Equal(((KeyValueSchema<Foo, Bar>)keyValueSchema1).ValueSchema.SchemaInfo.Type, SchemaType.JSON);
            Assert.Equal(((KeyValueSchema<Foo, Bar>)keyValueSchema2).KeySchema.SchemaInfo.Type, SchemaType.JSON);
            Assert.Equal(((KeyValueSchema<Foo, Bar>)keyValueSchema2).ValueSchema.SchemaInfo.Type, SchemaType.JSON);
            Assert.Equal(((KeyValueSchema<Foo, Bar>)keyValueSchema3).KeySchema.SchemaInfo.Type, SchemaType.JSON);
            Assert.Equal(((KeyValueSchema<Foo, Bar>)keyValueSchema3).ValueSchema.SchemaInfo.Type, SchemaType.JSON);

            var schemaInfo1 = Encoding.UTF8.GetString(keyValueSchema1.SchemaInfo.Schema);
            var schemaInfo2 = Encoding.UTF8.GetString(keyValueSchema2.SchemaInfo.Schema);
            var schemaInfo3 = Encoding.UTF8.GetString(keyValueSchema3.SchemaInfo.Schema);
            Assert.Equal(schemaInfo1, schemaInfo2);
            Assert.Equal(schemaInfo1, schemaInfo3);
        }
        [Fact]
        public virtual void TestAllowNullSchemaEncodeAndDecode()
        {
            var keyValueSchema = ISchema<object>.KeyValue<Foo, Bar>(typeof(Foo), typeof(Bar));

            var bar = new Bar
            {
                Field1 = true
            };

            var foo = new Foo
            {
                Field1 = "field1",
                Field2 = "field2",
                Field3 = "3",
                Field4 = bar,
                Color = Color.RED
            };

            var encodeBytes = keyValueSchema.Encode(new KeyValue<Foo, Bar>(foo, bar));
            Assert.True(encodeBytes.Length > 0);

            var keyValue = keyValueSchema.Decode(encodeBytes);
            var fooBack = keyValue.Key;
            var barBack = keyValue.Value;

            Assert.True(foo.Equals(fooBack));
            Assert.True(bar.Equals(barBack));
        }
        [Fact]
        public virtual void TestNotAllowNullSchemaEncodeAndDecode()
        {
            var keyValueSchema = ISchema<object>.KeyValue(JSONSchema<Foo>.Of(ISchemaDefinition<Foo>.Builder().WithPojo(typeof(Foo)).WithAlwaysAllowNull(false).Build())
                , JSONSchema<Bar>.Of(ISchemaDefinition<Bar>.Builder().WithPojo(typeof(Bar)).WithAlwaysAllowNull(false).Build()));

            var bar = new Bar
            {
                Field1 = true
            };

            var foo = new Foo
            {
                Field1 = "field1",
                Field2 = "field2",
                Field3 = "3",
                Field4 = bar,
                Color = Color.RED
            };

            var encodeBytes = keyValueSchema.Encode(new KeyValue<Foo, Bar>(foo, bar));
            Assert.True(encodeBytes.Length > 0);

            var keyValue = keyValueSchema.Decode(encodeBytes);
            var fooBack = keyValue.Key;
            var barBack = keyValue.Value;

            Assert.True(foo.Equals(fooBack));
            Assert.True(bar.Equals(barBack));
        }
        [Fact]
        public virtual void TestDefaultKeyValueEncodingTypeSchemaEncodeAndDecode()
        {
            var fooSchema = AvroSchema<Foo>.Of(ISchemaDefinition<Foo>.Builder().WithPojo(typeof(Foo)).Build());
            var barSchema = AvroSchema<Bar>.Of(ISchemaDefinition<Bar>.Builder().WithPojo(typeof(Bar)).Build());

            var keyValueSchema = ISchema<object>.KeyValue(fooSchema, barSchema);

            var bar = new Bar
            {
                Field1 = true
            };

            var foo = new Foo
            {
                Field1 = "field1",
                Field2 = "field2",
                Field3 = "3",
                Field4 = bar,
                Color = Color.RED
            };

            // Check kv.encoding.type default not set value
            var encodeBytes = keyValueSchema.Encode(new KeyValue<Foo, Bar>(foo, bar));
            Assert.True(encodeBytes.Length > 0);

            var keyValue = keyValueSchema.Decode(encodeBytes);
            var fooBack = keyValue.Key;
            var barBack = keyValue.Value;

            Assert.True(foo.Equals(fooBack));
            Assert.True(bar.Equals(barBack));
        }
        [Fact]
        public virtual void TestInlineKeyValueEncodingTypeSchemaEncodeAndDecode()
        {

            var fooSchema = AvroSchema<Foo>.Of(ISchemaDefinition<Foo>.Builder().WithPojo(typeof(Foo)).Build());
            var barSchema = AvroSchema<Bar>.Of(ISchemaDefinition<Bar>.Builder().WithPojo(typeof(Bar)).Build());

            var keyValueSchema = ISchema<object>.KeyValue(fooSchema, barSchema, KeyValueEncodingType.INLINE);


            var bar = new Bar
            {
                Field1 = true
            };

            var foo = new Foo
            {
                Field1 = "field1",
                Field2 = "field2",
                Field3 = "3",
                Field4 = bar,
                Color = Color.RED
            };

            // Check kv.encoding.type INLINE
            var encodeBytes = keyValueSchema.Encode(new KeyValue<Foo, Bar>(foo, bar));
            Assert.True(encodeBytes.Length > 0);
            var keyValue = keyValueSchema.Decode(encodeBytes);
            var fooBack = keyValue.Key;
            var barBack = keyValue.Value;
            Assert.True(foo.Equals(fooBack));
            Assert.True(bar.Equals(barBack));

        }
        [Fact]
        public virtual void TestSeparatedKeyValueEncodingTypeSchemaEncodeAndDecode()
        {
            var fooSchema = AvroSchema<Foo>.Of(ISchemaDefinition<Foo>.Builder().WithPojo(typeof(Foo)).Build());
            var barSchema = AvroSchema<Bar>.Of(ISchemaDefinition<Bar>.Builder().WithPojo(typeof(Bar)).Build());


            var keyValueSchema = ISchema<object>.KeyValue(fooSchema, barSchema, KeyValueEncodingType.SEPARATED);

            var bar = new Bar
            {
                Field1 = true
            };

            var foo = new Foo
            {
                Field1 = "field1",
                Field2 = "field2",
                Field3 = "3",
                Field4 = bar,
                Color = Color.RED
            };

            // Check kv.encoding.type SEPARATED
            var encodeBytes = keyValueSchema.Encode(new KeyValue<Foo, Bar>(foo, bar));
            Assert.True(encodeBytes.Length > 0);
            try
            {
                keyValueSchema.Decode(encodeBytes);
                Assert.True(false, "This method cannot be used under this SEPARATED encoding type");
            }
            catch (SchemaSerializationException e)
            {
                Assert.Contains("This method cannot be used under this SEPARATED encoding type", e.Message);
            }
            var keyValue = ((KeyValueSchema<Foo, Bar>)keyValueSchema).Decode(fooSchema.Encode(foo), encodeBytes, null);
            var fooBack = keyValue.Key;
            var barBack = keyValue.Value;
            Assert.True(foo.Equals(fooBack));
            Assert.True(bar.Equals(barBack));
        }

        [Fact]
        public virtual void TestAllowNullBytesSchemaEncodeAndDecode()
        {
            var fooAvroSchema = AvroSchema<Foo>.Of(ISchemaDefinition<Foo>.Builder().WithPojo(typeof(Foo)).Build());
            var barAvroSchema = AvroSchema<Bar>.Of(ISchemaDefinition<Bar>.Builder().WithPojo(typeof(Bar)).Build());

            var bar = new Bar
            {
                Field1 = true
            };

            var foo = new Foo
            {
                Field1 = "field1",
                Field2 = "field2",
                Field3 = "3",
                Field4 = bar,
                Color = Color.RED
            };

            var fooBytes = fooAvroSchema.Encode(foo);
            var barBytes = barAvroSchema.Encode(bar);

            var encodeBytes = ISchema<object>.KvBytes().Encode(new KeyValue<byte[], byte[]>(fooBytes, barBytes));
            var decodeKV = ISchema<object>.KvBytes().Decode(encodeBytes);

            var fooBack = fooAvroSchema.Decode(decodeKV.Key);
            var barBack = barAvroSchema.Decode(decodeKV.Value);

            Assert.True(foo.Equals(fooBack));
            Assert.True(bar.Equals(barBack));
        }

    }

}