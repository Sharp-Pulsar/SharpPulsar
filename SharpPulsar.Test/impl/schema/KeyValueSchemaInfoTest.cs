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
namespace SharpPulsar.Test.Impl.schema
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

using Bar = SchemaTestUtils.Bar;
	using Foo = SchemaTestUtils.Foo;
using KeyValueEncodingType = Org.Apache.Pulsar.Common.Schema.KeyValueEncodingType;
	using SchemaInfo = Org.Apache.Pulsar.Common.Schema.SchemaInfo;

/// <summary>
	/// Unit test <seealso cref="KeyValueSchemaInfoTest"/>.
	/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Slf4j public class KeyValueSchemaInfoTest
	public class KeyValueSchemaInfoTest
	{

		private static readonly IDictionary<string, string> FOO_PROPERTIES = new HashMapAnonymousInnerClass();

		public class HashMapAnonymousInnerClass : Hashtable
		{

			private const long serialVersionUID = 58641844834472929L;

	//		{
	//			put("foo1", "foo-value1");
	//			put("foo2", "foo-value2");
	//			put("foo3", "foo-value3");
	//		}

		}

		private static readonly IDictionary<string, string> BAR_PROPERTIES = new HashMapAnonymousInnerClass2();

		public class HashMapAnonymousInnerClass2 : Hashtable
		{

			private const long serialVersionUID = 58641844834472929L;

	//		{
	//			put("bar1", "bar-value1");
	//			put("bar2", "bar-value2");
	//			put("bar3", "bar-value3");
	//		}

		}

		public static readonly Schema<Foo> FooSchema = Schema.AVRO(SchemaDefinition.builder<Foo>().withAlwaysAllowNull(false).withPojo(typeof(Foo)).withProperties(FOO_PROPERTIES).build());
		public static readonly Schema<Bar> BarSchema = Schema.JSON(SchemaDefinition.builder<Bar>().withAlwaysAllowNull(true).withPojo(typeof(Bar)).withProperties(BAR_PROPERTIES).build());

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
			Schema<KeyValue<string, Bar>> NestedSchema = Schema.KeyValue(SchemaFields.STRING, BarSchema, KeyValueEncodingType.INLINE);
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

			assertEquals(NestedSchemaInfoKeyValue.Key, SchemaFields.STRING.SchemaInfo);
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
//ORIGINAL LINE: @Test public void testKeyValueSchemaInfoToString()
		public virtual void TestKeyValueSchemaInfoToString()
		{
			string HavePrimitiveType = DefaultImplementation.convertKeyValueSchemaInfoDataToString(KeyValueSchemaInfo.DecodeKeyValueSchemaInfo(Schema.KeyValue(Schema.AVRO(typeof(Foo)), SchemaFields.STRING).SchemaInfo));
			assertEquals(HavePrimitiveType, KEY_VALUE_SCHEMA_INFO_INCLUDE_PRIMITIVE);

			string NotHavePrimitiveType = DefaultImplementation.convertKeyValueSchemaInfoDataToString(KeyValueSchemaInfo.DecodeKeyValueSchemaInfo(Schema.KeyValue(Schema.AVRO(typeof(Foo)), Schema.AVRO(typeof(Foo))).SchemaInfo));
			assertEquals(NotHavePrimitiveType, KEY_VALUE_SCHEMA_INFO_NOT_INCLUDE_PRIMITIVE);
		}

	}

}