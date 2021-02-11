using SharpPulsar.Exceptions;
using SharpPulsar.Interfaces;
using SharpPulsar.Interfaces.ISchema;
using SharpPulsar.Schemas;
using SharpPulsar.Shared;
using System.Collections.Generic;
using System.Text;
using Xunit;
using SharpPulsar.Extension;

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
    public class KeyValueSchemaTest
    {
        [Fact]
        public virtual void TestFillParametersToSchemainfo()
        {
            AvroSchema<Foo> fooSchema = AvroSchema<Foo>.Of(ISchemaDefinition<Foo>.Builder().WithPojo(typeof(Foo)).Build());
            AvroSchema<Bar> barSchema = AvroSchema<Bar>.Of(ISchemaDefinition<Bar>.Builder().WithPojo(typeof(Bar)).Build());

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
            ISchema<KeyValue<Foo, Bar>> keyValueSchema1 = ISchema<object>.KeyValue(fooSchema, barSchema);

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
            AvroSchema<Foo> fooSchema = AvroSchema<Foo>.Of(ISchemaDefinition<Foo>.Builder().WithPojo(typeof(Foo)).WithAlwaysAllowNull(false).Build());
            AvroSchema<Bar> barSchema = AvroSchema<Bar>.Of(ISchemaDefinition<Bar>.Builder().WithPojo(typeof(Bar)).WithAlwaysAllowNull(false).Build());

            ISchema<KeyValue<Foo, Bar>> keyValueSchema1 = ISchema<object>.KeyValue(fooSchema, barSchema);
            ISchema<KeyValue<Foo, Bar>> keyValueSchema2 = ISchema<object>.KeyValue(AvroSchema<Foo>.Of(ISchemaDefinition<Foo>.Builder().WithPojo(typeof(Foo)).WithAlwaysAllowNull(false).Build()), AvroSchema<Bar>.Of(ISchemaDefinition<Bar>.Builder().WithPojo(typeof(Bar)).WithAlwaysAllowNull(false).Build()));

            Assert.Equal(keyValueSchema1.SchemaInfo.Type, SchemaType.KeyValue);
            Assert.Equal(keyValueSchema2.SchemaInfo.Type, SchemaType.KeyValue);

            Assert.Equal(((KeyValueSchema<Foo, Bar>)keyValueSchema1).KeySchema.SchemaInfo.Type, SchemaType.AVRO);
            Assert.Equal(((KeyValueSchema<Foo, Bar>)keyValueSchema1).ValueSchema.SchemaInfo.Type, SchemaType.AVRO);
            Assert.Equal(((KeyValueSchema<Foo, Bar>)keyValueSchema2).KeySchema.SchemaInfo.Type, SchemaType.AVRO);
            Assert.Equal(((KeyValueSchema<Foo, Bar>)keyValueSchema2).ValueSchema.SchemaInfo.Type, SchemaType.AVRO);

            string schemaInfo1 = Encoding.UTF8.GetString(keyValueSchema1.SchemaInfo.Schema.ToBytes());
            string schemaInfo2 = Encoding.UTF8.GetString(keyValueSchema2.SchemaInfo.Schema.ToBytes());
            Assert.Equal(schemaInfo1, schemaInfo2);
        }

        [Fact]
        public virtual void TestJsonSchemaCreate()
        {
            JSONSchema<Foo> fooSchema = JSONSchema<Foo>.Of(ISchemaDefinition<Foo>.Builder().WithPojo(typeof(Foo)).WithAlwaysAllowNull(false).Build());
            JSONSchema<Bar> barSchema = JSONSchema<Bar>.Of(ISchemaDefinition<Bar>.Builder().WithPojo(typeof(Bar)).WithAlwaysAllowNull(false).Build());

            ISchema<KeyValue<Foo, Bar>> keyValueSchema1 = ISchema<object>.KeyValue(fooSchema, barSchema);
            ISchema<KeyValue<Foo, Bar>> keyValueSchema2 = ISchema<object>.KeyValue(JSONSchema<Foo>.Of(ISchemaDefinition<Foo>.Builder().WithPojo(typeof(Foo)).WithAlwaysAllowNull(false).Build()), JSONSchema<Bar>.Of(ISchemaDefinition<Bar>.Builder().WithPojo(typeof(Bar)).WithAlwaysAllowNull(false).Build()));

            ISchema<KeyValue<Foo, Bar>> keyValueSchema3 = ISchema<object>.KeyValue(JSONSchema<Foo>.Of(ISchemaDefinition<Foo>.Builder().WithPojo(typeof(Foo)).WithAlwaysAllowNull(false).Build()), JSONSchema<Bar>.Of(ISchemaDefinition<Bar>.Builder().WithPojo(typeof(Bar)).WithAlwaysAllowNull(false).Build()));

            Assert.Equal(keyValueSchema1.SchemaInfo.Type, SchemaType.KeyValue);
            Assert.Equal(keyValueSchema2.SchemaInfo.Type, SchemaType.KeyValue);
            Assert.Equal(keyValueSchema3.SchemaInfo.Type, SchemaType.KeyValue);

            Assert.Equal(((KeyValueSchema<Foo, Bar>)keyValueSchema1).KeySchema.SchemaInfo.Type, SchemaType.JSON);
            Assert.Equal(((KeyValueSchema<Foo, Bar>)keyValueSchema1).ValueSchema.SchemaInfo.Type, SchemaType.JSON);
            Assert.Equal(((KeyValueSchema<Foo, Bar>)keyValueSchema2).KeySchema.SchemaInfo.Type, SchemaType.JSON);
            Assert.Equal(((KeyValueSchema<Foo, Bar>)keyValueSchema2).ValueSchema.SchemaInfo.Type, SchemaType.JSON);
            Assert.Equal(((KeyValueSchema<Foo, Bar>)keyValueSchema3).KeySchema.SchemaInfo.Type, SchemaType.JSON);
            Assert.Equal(((KeyValueSchema<Foo, Bar>)keyValueSchema3).ValueSchema.SchemaInfo.Type, SchemaType.JSON);

            string schemaInfo1 = Encoding.UTF8.GetString(keyValueSchema1.SchemaInfo.Schema.ToBytes());
            string schemaInfo2 = Encoding.UTF8.GetString(keyValueSchema2.SchemaInfo.Schema.ToBytes());
            string schemaInfo3 = Encoding.UTF8.GetString(keyValueSchema3.SchemaInfo.Schema.ToBytes());
            Assert.Equal(schemaInfo1, schemaInfo2);
            Assert.Equal(schemaInfo1, schemaInfo3);
        }
        [Fact]
        public virtual void TestAllowNullSchemaEncodeAndDecode()
        {
            var keyValueSchema = ISchema<object>.KeyValue<Foo, Bar>(typeof(Foo), typeof(Bar));

            Bar bar = new Bar
            {
                Field1 = true
            };

            Foo foo = new Foo
            {
                Field1 = "field1",
                Field2 = "field2",
                Field3 = "3",
                Field4 = bar,
                Color = Color.RED
            };

            sbyte[] encodeBytes = keyValueSchema.Encode(new KeyValue<Foo, Bar>(foo, bar));
            Assert.True(encodeBytes.Length > 0);

            KeyValue<Foo, Bar> keyValue = keyValueSchema.Decode(encodeBytes);
            Foo fooBack = keyValue.Key;
            Bar barBack = keyValue.Value;

            Assert.True(foo.Equals(fooBack));
            Assert.True(bar.Equals(barBack));
        }
        [Fact]        
        public virtual void TestNotAllowNullSchemaEncodeAndDecode()
        {
            var keyValueSchema = ISchema<object>.KeyValue(JSONSchema<Foo>.Of(ISchemaDefinition<Foo>.Builder().WithPojo(typeof(Foo)).WithAlwaysAllowNull(false).Build())
                , JSONSchema<Bar>.Of(ISchemaDefinition<Bar>.Builder().WithPojo(typeof(Bar)).WithAlwaysAllowNull(false).Build()));

            Bar bar = new Bar
            {
                Field1 = true
            };

            Foo foo = new Foo
            {
                Field1 = "field1",
                Field2 = "field2",
                Field3 = "3",
                Field4 = bar,
                Color = Color.RED
            };

            sbyte[] encodeBytes = keyValueSchema.Encode(new KeyValue<Foo, Bar>(foo, bar));
            Assert.True(encodeBytes.Length > 0);

            KeyValue<Foo, Bar> keyValue = keyValueSchema.Decode(encodeBytes);
            Foo fooBack = keyValue.Key;
            Bar barBack = keyValue.Value;

            Assert.True(foo.Equals(fooBack));
            Assert.True(bar.Equals(barBack));
        }
        [Fact]
        public virtual void TestDefaultKeyValueEncodingTypeSchemaEncodeAndDecode()
        {
            AvroSchema<Foo> fooSchema = AvroSchema<Foo>.Of(ISchemaDefinition<Foo>.Builder().WithPojo(typeof(Foo)).Build());
            AvroSchema<Bar> barSchema = AvroSchema<Bar>.Of(ISchemaDefinition<Bar>.Builder().WithPojo(typeof(Bar)).Build());

            ISchema<KeyValue<Foo, Bar>> keyValueSchema = ISchema<object>.KeyValue(fooSchema, barSchema);

            Bar bar = new Bar
            {
                Field1 = true
            };

            Foo foo = new Foo
            {
                Field1 = "field1",
                Field2 = "field2",
                Field3 = "3",
                Field4 = bar,
                Color = Color.RED
            };

            // Check kv.encoding.type default not set value
            sbyte[] encodeBytes = keyValueSchema.Encode(new KeyValue<Foo, Bar>(foo, bar));
            Assert.True(encodeBytes.Length > 0);

            KeyValue<Foo, Bar> keyValue = keyValueSchema.Decode(encodeBytes);
            Foo fooBack = keyValue.Key;
            Bar barBack = keyValue.Value;

            Assert.True(foo.Equals(fooBack));
            Assert.True(bar.Equals(barBack));
        }
        [Fact]
        public virtual void TestInlineKeyValueEncodingTypeSchemaEncodeAndDecode()
        {

            AvroSchema<Foo> fooSchema = AvroSchema<Foo>.Of(ISchemaDefinition<Foo>.Builder().WithPojo(typeof(Foo)).Build());
            AvroSchema<Bar> barSchema = AvroSchema<Bar>.Of(ISchemaDefinition<Bar>.Builder().WithPojo(typeof(Bar)).Build());

            ISchema<KeyValue<Foo, Bar>> keyValueSchema = ISchema<object>.KeyValue(fooSchema, barSchema, KeyValueEncodingType.INLINE);


            Bar bar = new Bar
            {
                Field1 = true
            };

            Foo foo = new Foo
            {
                Field1 = "field1",
                Field2 = "field2",
                Field3 = "3",
                Field4 = bar,
                Color = Color.RED
            };

            // Check kv.encoding.type INLINE
            sbyte[] encodeBytes = keyValueSchema.Encode(new KeyValue<Foo, Bar>(foo, bar));
            Assert.True(encodeBytes.Length > 0);
            KeyValue<Foo, Bar> keyValue = keyValueSchema.Decode(encodeBytes);
            Foo fooBack = keyValue.Key;
            Bar barBack = keyValue.Value;
            Assert.True(foo.Equals(fooBack));
            Assert.True(bar.Equals(barBack));

        }
        [Fact]
        public virtual void TestSeparatedKeyValueEncodingTypeSchemaEncodeAndDecode()
        {
            AvroSchema<Foo> fooSchema = AvroSchema<Foo>.Of(ISchemaDefinition<Foo>.Builder().WithPojo(typeof(Foo)).Build());
            AvroSchema<Bar> barSchema = AvroSchema<Bar>.Of(ISchemaDefinition<Bar>.Builder().WithPojo(typeof(Bar)).Build());


            ISchema<KeyValue<Foo, Bar>> keyValueSchema = ISchema<object>.KeyValue(fooSchema, barSchema, KeyValueEncodingType.SEPARATED);

            Bar bar = new Bar
            {
                Field1 = true
            };

            Foo foo = new Foo
            {
                Field1 = "field1",
                Field2 = "field2",
                Field3 = "3",
                Field4 = bar,
                Color = Color.RED
            };

            // Check kv.encoding.type SEPARATED
            sbyte[] encodeBytes = keyValueSchema.Encode(new KeyValue<Foo, Bar>(foo, bar));
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
            KeyValue<Foo, Bar> keyValue = ((KeyValueSchema<Foo, Bar>)keyValueSchema).Decode(fooSchema.Encode(foo), encodeBytes, null);
            Foo fooBack = keyValue.Key;
            Bar barBack = keyValue.Value;
            Assert.True(foo.Equals(fooBack));
            Assert.True(bar.Equals(barBack));
        }

        [Fact]
        public virtual void TestAllowNullBytesSchemaEncodeAndDecode()
        {
            AvroSchema<Foo> fooAvroSchema = AvroSchema<Foo>.Of(ISchemaDefinition<Foo>.Builder().WithPojo(typeof(Foo)).Build());
            AvroSchema<Bar> barAvroSchema = AvroSchema<Bar>.Of(ISchemaDefinition<Bar>.Builder().WithPojo(typeof(Bar)).Build());

            Bar bar = new Bar
            {
                Field1 = true
            };

            Foo foo = new Foo
            {
                Field1 = "field1",
                Field2 = "field2",
                Field3 = "3",
                Field4 = bar,
                Color = Color.RED
            };

            sbyte[] fooBytes = fooAvroSchema.Encode(foo);
            sbyte[] barBytes = barAvroSchema.Encode(bar);

            sbyte[] encodeBytes = ISchema<object>.KvBytes().Encode(new KeyValue<sbyte[], sbyte[]>(fooBytes, barBytes));
            KeyValue<sbyte[], sbyte[]> decodeKV = ISchema<object>.KvBytes().Decode(encodeBytes);

            Foo fooBack = fooAvroSchema.Decode(decodeKV.Key);
            Bar barBack = barAvroSchema.Decode(decodeKV.Value);

            Assert.True(foo.Equals(fooBack));
            Assert.True(bar.Equals(barBack));
        }

    }

}