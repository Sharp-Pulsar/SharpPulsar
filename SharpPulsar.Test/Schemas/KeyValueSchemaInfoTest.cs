using SharpPulsar.Common.Enum;
using SharpPulsar.Common.Schema;
using SharpPulsar.Interfaces;
using SharpPulsar.Interfaces.ISchema;
using SharpPulsar.Schema;
using SharpPulsar.Shared;
using System.Collections;
using System.Collections.Generic;
using Xunit;
using static SharpPulsar.Test.Schema.SchemaTestUtils;

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

	/// <summary>
	/// Unit test <seealso cref="KeyValueSchemaInfoTest"/>.
	/// </summary>
	public class KeyValueSchemaInfoTest
	{

		private static readonly IDictionary<string, string> _fooProperties = new Dictionary<string, string> 
		{
			{"serialVersionUID", "58641844834472929" },
			{"foo1", "foo-value1" },
			{"foo2", "foo-value2" },
			{"foo3", "foo-value3" }
		
		};


		private static readonly IDictionary<string, string> _barProperties = new Dictionary<string, string>
		{
			{"serialVersionUID", "58641844834472929" },
			{"bar1", "bar-value1" },
			{"bar2", "bar-value2" },
			{"bar3", "bar-value3" }

		};

		public static readonly ISchema<Foo> fooSchema = ISchema<Foo>.Avro(ISchemaDefinition<Foo>.Builder().WithAlwaysAllowNull(false).WithPojo(typeof(Foo)).WithProperties(_fooProperties).Build());
		public static readonly ISchema<Bar> barSchema = ISchema<Bar>.Json(ISchemaDefinition<Bar>.Builder().WithAlwaysAllowNull(true).WithPojo(typeof(Bar)).WithProperties(_barProperties).Build());

		[Fact]
		public void TestDecodeNonKeyValueSchemaInfo()
		{
			DefaultImplementation.DecodeKeyValueSchemaInfo(fooSchema.SchemaInfo);
		}

		public virtual object[][] EncodingTypes()
		{
			return new object[][]
			{
				new object[] {KeyValueEncodingType.INLINE},
				new object[] {KeyValueEncodingType.SEPARATED}
			};
		}
		[Fact]
		public void EncodeDecodeKeyValueSchemaInfoInline()
        {
			EncodeDecodeKeyValueSchemaInfo(KeyValueEncodingType.INLINE);
		}
		[Fact]
		public void EncodeDecodeKeyValueSchemaInfoSeparated()
        {
			EncodeDecodeKeyValueSchemaInfo(KeyValueEncodingType.SEPARATED);
		}
		private void EncodeDecodeKeyValueSchemaInfo(KeyValueEncodingType encodingType)
		{
			ISchema<KeyValue<Foo, Bar>> kvSchema = ISchema<KeyValue<Foo,Bar>>.KeyValue(fooSchema, barSchema, encodingType);
			ISchemaInfo kvSchemaInfo = kvSchema.SchemaInfo;
			Assert.Equal(DefaultImplementation.DecodeKeyValueEncodingType(kvSchemaInfo), encodingType);

			ISchemaInfo encodedSchemaInfo = DefaultImplementation.EncodeKeyValueSchemaInfo(fooSchema, barSchema, encodingType);
			Assert.Equal(encodedSchemaInfo, kvSchemaInfo);
			Assert.Equal(DefaultImplementation.DecodeKeyValueEncodingType(encodedSchemaInfo), encodingType);

			var schemaInfoKeyValue = DefaultImplementation.DecodeKeyValueSchemaInfo(kvSchemaInfo);

			Assert.Equal(schemaInfoKeyValue.Key, fooSchema.SchemaInfo);
			Assert.Equal(schemaInfoKeyValue.Value, barSchema.SchemaInfo);
		}

		[Fact]
		public void EncodeDecodeNestedKeyValueSchemaInfoInline()
		{
			EncodeDecodeNestedKeyValueSchemaInfo(KeyValueEncodingType.INLINE);
		}
		[Fact]
		public void EncodeDecodeNestedKeyValueSchemaInfoSeparated()
		{
			EncodeDecodeNestedKeyValueSchemaInfo(KeyValueEncodingType.SEPARATED);
		}
		private void EncodeDecodeNestedKeyValueSchemaInfo(KeyValueEncodingType encodingType)
		{
			var nestedSchema = ISchema<KeyValue<string, Bar>>.KeyValue(ISchema<object>.String, barSchema, KeyValueEncodingType.INLINE);
			var kvSchema = ISchema<KeyValue<string, Bar>>.KeyValue(fooSchema, nestedSchema, encodingType);
			var kvSchemaInfo = kvSchema.SchemaInfo;
			Assert.Equal(DefaultImplementation.DecodeKeyValueEncodingType(kvSchemaInfo), encodingType);

			var encodedSchemaInfo = DefaultImplementation.EncodeKeyValueSchemaInfo(fooSchema, nestedSchema, encodingType);
			Assert.Equal(encodedSchemaInfo, kvSchemaInfo);
			Assert.Equal(DefaultImplementation.DecodeKeyValueEncodingType(encodedSchemaInfo), encodingType);

			var schemaInfoKeyValue = DefaultImplementation.DecodeKeyValueSchemaInfo(kvSchemaInfo);

			Assert.Equal(schemaInfoKeyValue.Key, fooSchema.SchemaInfo);
			Assert.Equal(schemaInfoKeyValue.Value.Type, SchemaType.KeyValue);
			var nestedSchemaInfoKeyValue = DefaultImplementation.DecodeKeyValueSchemaInfo(schemaInfoKeyValue.Value);

			Assert.Equal(nestedSchemaInfoKeyValue.Key, ISchema<object>.String.SchemaInfo);
			Assert.Equal(nestedSchemaInfoKeyValue.Value, barSchema.SchemaInfo);
		}
		[Fact]
		private void TestKeyValueSchemaInfoBackwardCompatibility()
		{
			var kvSchema = ISchema<KeyValue<Foo, Bar>>.KeyValue(fooSchema, barSchema, KeyValueEncodingType.SEPARATED);

            var oldSchemaInfo = new SchemaInfo
            {
                Name = "",
                Type = SchemaType.KeyValue,
                Schema = kvSchema.SchemaInfo.Schema,
                Properties = new Dictionary<string, string>()
            };
            Assert.Equal(KeyValueEncodingType.INLINE, DefaultImplementation.DecodeKeyValueEncodingType(oldSchemaInfo));

			var schemaInfoKeyValue = DefaultImplementation.DecodeKeyValueSchemaInfo(oldSchemaInfo);
			// verify the key schema
			ISchemaInfo keySchemaInfo = schemaInfoKeyValue.Key;
			Assert.Equal(SchemaType.BYTES, keySchemaInfo.Type);
			Assert.Equal(fooSchema.SchemaInfo.Schema, keySchemaInfo.Schema);//"Expected schema = " + fooSchema.SchemaInfo.SchemaDefinition + " but found " + keySchemaInfo.SchemaDefinition
			Assert.False(fooSchema.SchemaInfo.Properties.Count == 0);
			Assert.True(keySchemaInfo.Properties.Count == 0);
			// verify the value schema
			var valueSchemaInfo = schemaInfoKeyValue.Value;
			Assert.Equal(SchemaType.BYTES, valueSchemaInfo.Type);
			Assert.Equal(barSchema.SchemaInfo.Schema, valueSchemaInfo.Schema);
			Assert.False(barSchema.SchemaInfo.Properties.Count == 0);
			Assert.True(valueSchemaInfo.Properties.Count == 0);
		}
		[Fact]
		public void TestKeyValueSchemaInfoToString()
		{
			var havePrimitiveType = DefaultImplementation.ConvertKeyValueSchemaInfoDataToString(KeyValueSchemaInfo.DecodeKeyValueSchemaInfo(ISchema<object>.KeyValue(ISchema<Foo>.Avro(typeof(Foo)), ISchema<object>.String).SchemaInfo));
			Assert.Equal(havePrimitiveType, KEY_VALUE_SCHEMA_INFO_INCLUDE_PRIMITIVE);

			var notHavePrimitiveType = DefaultImplementation.ConvertKeyValueSchemaInfoDataToString(KeyValueSchemaInfo.DecodeKeyValueSchemaInfo(ISchema<object>.KeyValue(ISchema<Foo>.Avro(typeof(Foo)), ISchema<Foo>.Avro(typeof(Foo))).SchemaInfo));
			Assert.Equal(notHavePrimitiveType, KEY_VALUE_SCHEMA_INFO_NOT_INCLUDE_PRIMITIVE);
		}

	}

}