using SharpPulsar.Interfaces;
using SharpPulsar.Interfaces.ISchema;
using SharpPulsar.Schemas;
using SharpPulsar.Shared;
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
    [Collection("SchemaSpec")]
    /// <summary>
    /// Unit test <seealso cref="KeyValueSchemaInfoTest"/>.
    /// </summary>
    public class KeyValueSchemaInfoTest
    {

        private static readonly IDictionary<string, string> _fooProperties = new Dictionary<string, string>
        {
            {"foo1", "foo-value1" }, {"foo2", "foo-value2"}, {"foo3", "foo-value3"}
        };

        private static readonly IDictionary<string, string> _barProperties = new Dictionary<string, string>
        {
            {"bar1", "bar-value1" }, {"bar2", "bar-value2"}, {"bar3", "bar-value3"}
        };

        public static readonly ISchema<Foo> FooSchema = ISchema<object>.Avro(ISchemaDefinition<Foo>.Builder().WithAlwaysAllowNull(false).WithPojo(typeof(Foo)).WithProperties(_fooProperties).Build());
        public static readonly ISchema<Bar> BarSchema = ISchema<object>.Json(ISchemaDefinition<Bar>.Builder().WithAlwaysAllowNull(true).WithPojo(typeof(Bar)).WithProperties(_barProperties).Build());

        [Fact]
        public void EncodeDecodeKeyValueSchemaInfoINLINE()
        {
            var encodingType = KeyValueEncodingType.INLINE;
            ISchema<KeyValue<Foo, Bar>> kvSchema = ISchema<object>.KeyValue(FooSchema, BarSchema, encodingType);
            ISchemaInfo kvSchemaInfo = kvSchema.SchemaInfo;
            Assert.Equal(encodingType, DefaultImplementation.DecodeKeyValueEncodingType(kvSchemaInfo));

            ISchemaInfo encodedSchemaInfo = DefaultImplementation.EncodeKeyValueSchemaInfo(FooSchema, BarSchema, encodingType);
            for (var i = 0; i < kvSchemaInfo.Schema.Length; i++)
            {
                var expected = kvSchemaInfo.Schema[i];
                var actual = encodedSchemaInfo.Schema[i];
                Assert.Equal(expected, actual);
            }
            Assert.Equal(encodingType, DefaultImplementation.DecodeKeyValueEncodingType(encodedSchemaInfo));

            KeyValue<ISchemaInfo, ISchemaInfo> schemaInfoKeyValue = DefaultImplementation.DecodeKeyValueSchemaInfo(kvSchemaInfo);

            for (var i = 0; i < FooSchema.SchemaInfo.Schema.Length; i++)
            {
                var expected = FooSchema.SchemaInfo.Schema[i];
                var actual = schemaInfoKeyValue.Key.Schema[i];
                Assert.Equal(expected, actual);
            }

            for (var i = 0; i < BarSchema.SchemaInfo.Schema.Length; i++)
            {
                var expected = BarSchema.SchemaInfo.Schema[i];
                var actual = schemaInfoKeyValue.Value.Schema[i];
                Assert.Equal(expected, actual);
            }
        }
        
        [Fact]
        public void EncodeDecodeKeyValueSchemaInfoSEPARATED()
        {
            var encodingType = KeyValueEncodingType.SEPARATED;
            ISchema<KeyValue<Foo, Bar>> kvSchema = ISchema<object>.KeyValue(FooSchema, BarSchema, encodingType);
            ISchemaInfo kvSchemaInfo = kvSchema.SchemaInfo;
            Assert.Equal(encodingType, DefaultImplementation.DecodeKeyValueEncodingType(kvSchemaInfo));

            ISchemaInfo encodedSchemaInfo = DefaultImplementation.EncodeKeyValueSchemaInfo(FooSchema, BarSchema, encodingType);
            for (var i = 0; i < kvSchemaInfo.Schema.Length; i++)
            {
                var expected = kvSchemaInfo.Schema[i];
                var actual = encodedSchemaInfo.Schema[i];
                Assert.Equal(expected, actual);
            }
            Assert.Equal(encodingType, DefaultImplementation.DecodeKeyValueEncodingType(encodedSchemaInfo));

            KeyValue<ISchemaInfo, ISchemaInfo> schemaInfoKeyValue = DefaultImplementation.DecodeKeyValueSchemaInfo(kvSchemaInfo);

            for (var i = 0; i < FooSchema.SchemaInfo.Schema.Length; i++)
            {
                var expected = FooSchema.SchemaInfo.Schema[i];
                var actual = schemaInfoKeyValue.Key.Schema[i];
                Assert.Equal(expected, actual);
            }

            for (var i = 0; i < BarSchema.SchemaInfo.Schema.Length; i++)
            {
                var expected = BarSchema.SchemaInfo.Schema[i];
                var actual = schemaInfoKeyValue.Value.Schema[i];
                Assert.Equal(expected, actual);
            }
        }


        [Fact]
        public void EncodeDecodeNestedKeyValueSchemaInfo()
        {
            var encodingType = KeyValueEncodingType.INLINE;
            ISchema<KeyValue<string, Bar>> nestedSchema = ISchema<object>.KeyValue(ISchema<object>.String, BarSchema, KeyValueEncodingType.INLINE);
            ISchema<KeyValue<Foo, KeyValue<string, Bar>>> kvSchema = ISchema<object>.KeyValue(FooSchema, nestedSchema, encodingType);
            ISchemaInfo kvSchemaInfo = kvSchema.SchemaInfo;
            Assert.Equal(encodingType, DefaultImplementation.DecodeKeyValueEncodingType(kvSchemaInfo));

            ISchemaInfo encodedSchemaInfo = DefaultImplementation.EncodeKeyValueSchemaInfo(FooSchema, nestedSchema, encodingType);
            for (var i = 0; i < kvSchemaInfo.Schema.Length; i++)
            {
                var expected = kvSchemaInfo.Schema[i];
                var actual = encodedSchemaInfo.Schema[i];
                Assert.Equal(expected, actual);
            }
            Assert.Equal(encodingType, DefaultImplementation.DecodeKeyValueEncodingType(encodedSchemaInfo));

            KeyValue<ISchemaInfo, ISchemaInfo> schemaInfoKeyValue = DefaultImplementation.DecodeKeyValueSchemaInfo(kvSchemaInfo);

            
            for (var i = 0; i < FooSchema.SchemaInfo.Schema.Length; i++)
            {
                var expected = FooSchema.SchemaInfo.Schema[i];
                var actual = schemaInfoKeyValue.Key.Schema[i];
                Assert.Equal(expected, actual);
            }
            Assert.Equal(schemaInfoKeyValue.Value.Type, SchemaType.KeyValue);
            KeyValue<ISchemaInfo, ISchemaInfo> nestedSchemaInfoKeyValue = DefaultImplementation.DecodeKeyValueSchemaInfo(schemaInfoKeyValue.Value);
                        
            var stringSchema = ISchema<object>.String.SchemaInfo.Schema;
            for (var i = 0; i < stringSchema.Length; i++)
            {
                var expected = stringSchema[i];
                var actual = nestedSchemaInfoKeyValue.Key.Schema[i];
                Assert.Equal(expected, actual);
            }
            for (var i = 0; i < BarSchema.SchemaInfo.Schema.Length; i++)
            {
                var expected = BarSchema.SchemaInfo.Schema[i];
                var actual = nestedSchemaInfoKeyValue.Value.Schema[i];
                Assert.Equal(expected, actual);
            }
        }

        [Fact]
        public void EncodeDecodeNestedKeyValueSchemaInfoSEPARATED()
        {
            var encodingType = KeyValueEncodingType.SEPARATED;
            ISchema<KeyValue<string, Bar>> nestedSchema = ISchema<object>.KeyValue(ISchema<object>.String, BarSchema, KeyValueEncodingType.INLINE);
            ISchema<KeyValue<Foo, KeyValue<string, Bar>>> kvSchema = ISchema<object>.KeyValue(FooSchema, nestedSchema, encodingType);
            ISchemaInfo kvSchemaInfo = kvSchema.SchemaInfo;
            Assert.Equal(encodingType, DefaultImplementation.DecodeKeyValueEncodingType(kvSchemaInfo));

            ISchemaInfo encodedSchemaInfo = DefaultImplementation.EncodeKeyValueSchemaInfo(FooSchema, nestedSchema, encodingType);
            for (var i = 0; i < kvSchemaInfo.Schema.Length; i++)
            {
                var expected = kvSchemaInfo.Schema[i];
                var actual = encodedSchemaInfo.Schema[i];
                Assert.Equal(expected, actual);
            }
            Assert.Equal(encodingType, DefaultImplementation.DecodeKeyValueEncodingType(encodedSchemaInfo));

            KeyValue<ISchemaInfo, ISchemaInfo> schemaInfoKeyValue = DefaultImplementation.DecodeKeyValueSchemaInfo(kvSchemaInfo);

            for (var i = 0; i < FooSchema.SchemaInfo.Schema.Length; i++)
            {
                var expected = FooSchema.SchemaInfo.Schema[i];
                var actual = schemaInfoKeyValue.Key.Schema[i];
                Assert.Equal(expected, actual);
            }
            Assert.Equal(schemaInfoKeyValue.Value.Type, SchemaType.KeyValue);
            KeyValue<ISchemaInfo, ISchemaInfo> nestedSchemaInfoKeyValue = DefaultImplementation.DecodeKeyValueSchemaInfo(schemaInfoKeyValue.Value);

            var stringSchema = ISchema<object>.String.SchemaInfo.Schema;
            for (var i = 0; i < stringSchema.Length; i++)
            {
                var expected = stringSchema[i];
                var actual = nestedSchemaInfoKeyValue.Key.Schema[i];
                Assert.Equal(expected, actual);
            }
            for (var i = 0; i < BarSchema.SchemaInfo.Schema.Length; i++)
            {
                var expected = BarSchema.SchemaInfo.Schema[i];
                var actual = nestedSchemaInfoKeyValue.Value.Schema[i];
                Assert.Equal(expected, actual);
            }
        }

        [Fact]
        public virtual void TestKeyValueSchemaInfoBackwardCompatibility()
        {
            ISchema<KeyValue<Foo, Bar>> kvSchema = ISchema<object>.KeyValue(FooSchema, BarSchema, KeyValueEncodingType.SEPARATED);

            SchemaInfo oldSchemaInfo = new SchemaInfo {
            
                Name = "",
                Type = SchemaType.KeyValue,
                Schema = kvSchema.SchemaInfo.Schema,
                Properties = new Dictionary<string, string>()
            };

            Assert.Equal(KeyValueEncodingType.INLINE, DefaultImplementation.DecodeKeyValueEncodingType(oldSchemaInfo));

            KeyValue<ISchemaInfo, ISchemaInfo> schemaInfoKeyValue = DefaultImplementation.DecodeKeyValueSchemaInfo(oldSchemaInfo);
            // verify the key schemaS
            ISchemaInfo keySchemaInfo = schemaInfoKeyValue.Key;
            Assert.Equal(SchemaType.BYTES, keySchemaInfo.Type);

            for(var i = 0; i < FooSchema.SchemaInfo.Schema.Length; i++)
            {
                var expected = FooSchema.SchemaInfo.Schema[i];
                var actual = keySchemaInfo.Schema[i];
                Assert.Equal(expected, actual);
            }
            
            Assert.False(FooSchema.SchemaInfo.Properties.Count == 0);
            Assert.True(keySchemaInfo.Properties.Count == 0);
            // verify the value schema
            ISchemaInfo valueSchemaInfo = schemaInfoKeyValue.Value;
            Assert.Equal(SchemaType.BYTES, valueSchemaInfo.Type);
            for (var i = 0; i < BarSchema.SchemaInfo.Schema.Length; i++)
            {
                var expected = BarSchema.SchemaInfo.Schema[i];
                var actual = valueSchemaInfo.Schema[i];
                Assert.Equal(expected, actual);
            }
            Assert.False(BarSchema.SchemaInfo.Properties.Count == 0);
            Assert.True(valueSchemaInfo.Properties.Count == 0);
        }

    }

}