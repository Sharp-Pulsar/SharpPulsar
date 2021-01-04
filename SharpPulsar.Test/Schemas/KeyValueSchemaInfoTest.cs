using System.Collections;
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
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.apache.pulsar.client.impl.schema.SchemaTestUtils.KEY_VALUE_SCHEMA_INFO_INCLUDE_PRIMITIVE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.apache.pulsar.client.impl.schema.SchemaTestUtils.KEY_VALUE_SCHEMA_INFO_NOT_INCLUDE_PRIMITIVE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.testng.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.testng.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.testng.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.testng.@internal.junit.ArrayAsserts.assertArrayEquals;

	using Slf4j = lombok.@extern.slf4j.Slf4j;
	using Schema = org.apache.pulsar.client.api.Schema;
	using SchemaDefinition = org.apache.pulsar.client.api.schema.SchemaDefinition;
	using Bar = Org.Apache.Pulsar.Client.Impl.Schema.SchemaTestUtils.Bar;
	using Foo = Org.Apache.Pulsar.Client.Impl.Schema.SchemaTestUtils.Foo;
	using DefaultImplementation = org.apache.pulsar.client.@internal.DefaultImplementation;
	using KeyValue = org.apache.pulsar.common.schema.KeyValue;
	using KeyValueEncodingType = org.apache.pulsar.common.schema.KeyValueEncodingType;
	using SchemaInfo = org.apache.pulsar.common.schema.SchemaInfo;
	using SchemaType = org.apache.pulsar.common.schema.SchemaType;
	using JSONException = org.json.JSONException;
	using DataProvider = org.testng.annotations.DataProvider;
	using Test = org.testng.annotations.Test;

	/// <summary>
	/// Unit test <seealso cref="KeyValueSchemaInfoTest"/>.
	/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Slf4j public class KeyValueSchemaInfoTest
	public class KeyValueSchemaInfoTest
	{

		private static readonly IDictionary<string, string> _fooProperties = new HashMapAnonymousInnerClass();

		private class HashMapAnonymousInnerClass : Hashtable
		{
			public HashMapAnonymousInnerClass()
			{
				_serialVersionUID = 58641844834472929L;

				this.put("foo1", "foo-value1");
				this.put("foo2", "foo-value2");
				this.put("foo3", "foo-value3");
			}


			private static readonly long _serialVersionUID;

		}

		private static readonly IDictionary<string, string> _barProperties = new HashMapAnonymousInnerClass2();

		private class HashMapAnonymousInnerClass2 : Hashtable
		{
			public HashMapAnonymousInnerClass2()
			{
				_serialVersionUID = 58641844834472929L;

				this.put("bar1", "bar-value1");
				this.put("bar2", "bar-value2");
				this.put("bar3", "bar-value3");
			}


			private static readonly long _serialVersionUID;

		}

		public static readonly Schema<Foo> FooSchema = Schema.AVRO(SchemaDefinition.builder<Foo>().withAlwaysAllowNull(false).withPojo(typeof(Foo)).withProperties(_fooProperties).build());
		public static readonly Schema<Bar> BarSchema = Schema.JSON(SchemaDefinition.builder<Bar>().withAlwaysAllowNull(true).withPojo(typeof(Bar)).withProperties(_barProperties).build());

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expectedExceptions = IllegalArgumentException.class) public void testDecodeNonKeyValueSchemaInfo()
		public virtual void TestDecodeNonKeyValueSchemaInfo()
		{
			DefaultImplementation.decodeKeyValueSchemaInfo(FooSchema.SchemaInfo);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @DataProvider(name = "encodingTypes") public Object[][] encodingTypes()
		public virtual object[][] EncodingTypes()
		{
			return new object[][]
			{
				new object[] {KeyValueEncodingType.INLINE},
				new object[] {KeyValueEncodingType.SEPARATED}
			};
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(dataProvider = "encodingTypes") public void encodeDecodeKeyValueSchemaInfo(org.apache.pulsar.common.schema.KeyValueEncodingType encodingType)
		public virtual void EncodeDecodeKeyValueSchemaInfo(KeyValueEncodingType EncodingType)
		{
			Schema<KeyValue<Foo, Bar>> KvSchema = Schema.KeyValue(FooSchema, BarSchema, EncodingType);
			SchemaInfo KvSchemaInfo = KvSchema.SchemaInfo;
			assertEquals(DefaultImplementation.decodeKeyValueEncodingType(KvSchemaInfo), EncodingType);

			SchemaInfo EncodedSchemaInfo = DefaultImplementation.encodeKeyValueSchemaInfo(FooSchema, BarSchema, EncodingType);
			assertEquals(EncodedSchemaInfo, KvSchemaInfo);
			assertEquals(DefaultImplementation.decodeKeyValueEncodingType(EncodedSchemaInfo), EncodingType);

			KeyValue<SchemaInfo, SchemaInfo> SchemaInfoKeyValue = DefaultImplementation.decodeKeyValueSchemaInfo(KvSchemaInfo);

			assertEquals(SchemaInfoKeyValue.Key, FooSchema.SchemaInfo);
			assertEquals(SchemaInfoKeyValue.Value, BarSchema.SchemaInfo);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(dataProvider = "encodingTypes") public void encodeDecodeNestedKeyValueSchemaInfo(org.apache.pulsar.common.schema.KeyValueEncodingType encodingType)
		public virtual void EncodeDecodeNestedKeyValueSchemaInfo(KeyValueEncodingType EncodingType)
		{
			Schema<KeyValue<string, Bar>> NestedSchema = Schema.KeyValue(Schema.STRING, BarSchema, KeyValueEncodingType.INLINE);
			Schema<KeyValue<Foo, KeyValue<string, Bar>>> KvSchema = Schema.KeyValue(FooSchema, NestedSchema, EncodingType);
			SchemaInfo KvSchemaInfo = KvSchema.SchemaInfo;
			assertEquals(DefaultImplementation.decodeKeyValueEncodingType(KvSchemaInfo), EncodingType);

			SchemaInfo EncodedSchemaInfo = DefaultImplementation.encodeKeyValueSchemaInfo(FooSchema, NestedSchema, EncodingType);
			assertEquals(EncodedSchemaInfo, KvSchemaInfo);
			assertEquals(DefaultImplementation.decodeKeyValueEncodingType(EncodedSchemaInfo), EncodingType);

			KeyValue<SchemaInfo, SchemaInfo> SchemaInfoKeyValue = DefaultImplementation.decodeKeyValueSchemaInfo(KvSchemaInfo);

			assertEquals(SchemaInfoKeyValue.Key, FooSchema.SchemaInfo);
			assertEquals(SchemaInfoKeyValue.Value.Type, SchemaType.KEY_VALUE);
			KeyValue<SchemaInfo, SchemaInfo> NestedSchemaInfoKeyValue = DefaultImplementation.decodeKeyValueSchemaInfo(SchemaInfoKeyValue.Value);

			assertEquals(NestedSchemaInfoKeyValue.Key, Schema.STRING.SchemaInfo);
			assertEquals(NestedSchemaInfoKeyValue.Value, BarSchema.SchemaInfo);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testKeyValueSchemaInfoBackwardCompatibility()
		public virtual void TestKeyValueSchemaInfoBackwardCompatibility()
		{
			Schema<KeyValue<Foo, Bar>> KvSchema = Schema.KeyValue(FooSchema, BarSchema, KeyValueEncodingType.SEPARATED);

			SchemaInfo OldSchemaInfo = (new SchemaInfo()).setName("").setType(SchemaType.KEY_VALUE).setSchema(KvSchema.SchemaInfo.Schema).setProperties(Collections.emptyMap());

			assertEquals(DefaultImplementation.decodeKeyValueEncodingType(OldSchemaInfo), KeyValueEncodingType.INLINE);

			KeyValue<SchemaInfo, SchemaInfo> SchemaInfoKeyValue = DefaultImplementation.decodeKeyValueSchemaInfo(OldSchemaInfo);
			// verify the key schema
			SchemaInfo KeySchemaInfo = SchemaInfoKeyValue.Key;
			assertEquals(SchemaType.BYTES, KeySchemaInfo.Type);
			assertArrayEquals("Expected schema = " + FooSchema.SchemaInfo.SchemaDefinition + " but found " + KeySchemaInfo.SchemaDefinition, FooSchema.SchemaInfo.Schema, KeySchemaInfo.Schema);
			assertFalse(FooSchema.SchemaInfo.Properties.Empty);
			assertTrue(KeySchemaInfo.Properties.Empty);
			// verify the value schema
			SchemaInfo ValueSchemaInfo = SchemaInfoKeyValue.Value;
			assertEquals(SchemaType.BYTES, ValueSchemaInfo.Type);
			assertArrayEquals(BarSchema.SchemaInfo.Schema, ValueSchemaInfo.Schema);
			assertFalse(BarSchema.SchemaInfo.Properties.Empty);
			assertTrue(ValueSchemaInfo.Properties.Empty);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testKeyValueSchemaInfoToString() throws org.json.JSONException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		public virtual void TestKeyValueSchemaInfoToString()
		{
			string HavePrimitiveType = DefaultImplementation.convertKeyValueSchemaInfoDataToString(KeyValueSchemaInfo.decodeKeyValueSchemaInfo(Schema.KeyValue(Schema.AVRO(typeof(Foo)), Schema.STRING).SchemaInfo));
			JSONSchemaTest.AssertJSONEqual(HavePrimitiveType, KEY_VALUE_SCHEMA_INFO_INCLUDE_PRIMITIVE);

			string NotHavePrimitiveType = DefaultImplementation.convertKeyValueSchemaInfoDataToString(KeyValueSchemaInfo.decodeKeyValueSchemaInfo(Schema.KeyValue(Schema.AVRO(typeof(Foo)), Schema.AVRO(typeof(Foo))).SchemaInfo));
			JSONSchemaTest.AssertJSONEqual(NotHavePrimitiveType, KEY_VALUE_SCHEMA_INFO_NOT_INCLUDE_PRIMITIVE);
		}

	}

}