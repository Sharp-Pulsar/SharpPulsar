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
namespace org.apache.pulsar.client.impl.schema
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
	using Bar = org.apache.pulsar.client.impl.schema.SchemaTestUtils.Bar;
	using Foo = org.apache.pulsar.client.impl.schema.SchemaTestUtils.Foo;
	using DefaultImplementation = org.apache.pulsar.client.@internal.DefaultImplementation;
	using KeyValue = org.apache.pulsar.common.schema.KeyValue;
	using KeyValueEncodingType = org.apache.pulsar.common.schema.KeyValueEncodingType;
	using SchemaInfo = org.apache.pulsar.common.schema.SchemaInfo;
	using SchemaType = org.apache.pulsar.common.schema.SchemaType;
	using DataProvider = org.testng.annotations.DataProvider;
	using Test = org.testng.annotations.Test;

	/// <summary>
	/// Unit test <seealso cref="KeyValueSchemaInfoTest"/>.
	/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Slf4j public class KeyValueSchemaInfoTest
	public class KeyValueSchemaInfoTest
	{

		private static readonly IDictionary<string, string> FOO_PROPERTIES = new HashMapAnonymousInnerClass();

		private class HashMapAnonymousInnerClass : Hashtable
		{

			private const long serialVersionUID = 58641844834472929L;

	//		{
	//			put("foo1", "foo-value1");
	//			put("foo2", "foo-value2");
	//			put("foo3", "foo-value3");
	//		}

		}

		private static readonly IDictionary<string, string> BAR_PROPERTIES = new HashMapAnonymousInnerClass2();

		private class HashMapAnonymousInnerClass2 : Hashtable
		{

			private const long serialVersionUID = 58641844834472929L;

	//		{
	//			put("bar1", "bar-value1");
	//			put("bar2", "bar-value2");
	//			put("bar3", "bar-value3");
	//		}

		}

		public static readonly Schema<Foo> FOO_SCHEMA = Schema.AVRO(SchemaDefinition.builder<Foo>().withAlwaysAllowNull(false).withPojo(typeof(Foo)).withProperties(FOO_PROPERTIES).build());
		public static readonly Schema<Bar> BAR_SCHEMA = Schema.JSON(SchemaDefinition.builder<Bar>().withAlwaysAllowNull(true).withPojo(typeof(Bar)).withProperties(BAR_PROPERTIES).build());

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expectedExceptions = IllegalArgumentException.class) public void testDecodeNonKeyValueSchemaInfo()
		public virtual void testDecodeNonKeyValueSchemaInfo()
		{
			DefaultImplementation.decodeKeyValueSchemaInfo(FOO_SCHEMA.SchemaInfo);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @DataProvider(name = "encodingTypes") public Object[][] encodingTypes()
		public virtual object[][] encodingTypes()
		{
			return new object[][]
			{
				new object[] {KeyValueEncodingType.INLINE},
				new object[] {KeyValueEncodingType.SEPARATED}
			};
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(dataProvider = "encodingTypes") public void encodeDecodeKeyValueSchemaInfo(org.apache.pulsar.common.schema.KeyValueEncodingType encodingType)
		public virtual void encodeDecodeKeyValueSchemaInfo(KeyValueEncodingType encodingType)
		{
			Schema<KeyValue<Foo, Bar>> kvSchema = Schema.KeyValue(FOO_SCHEMA, BAR_SCHEMA, encodingType);
			SchemaInfo kvSchemaInfo = kvSchema.SchemaInfo;
			assertEquals(DefaultImplementation.decodeKeyValueEncodingType(kvSchemaInfo), encodingType);

			SchemaInfo encodedSchemaInfo = DefaultImplementation.encodeKeyValueSchemaInfo(FOO_SCHEMA, BAR_SCHEMA, encodingType);
			assertEquals(encodedSchemaInfo, kvSchemaInfo);
			assertEquals(DefaultImplementation.decodeKeyValueEncodingType(encodedSchemaInfo), encodingType);

			KeyValue<SchemaInfo, SchemaInfo> schemaInfoKeyValue = DefaultImplementation.decodeKeyValueSchemaInfo(kvSchemaInfo);

			assertEquals(schemaInfoKeyValue.Key, FOO_SCHEMA.SchemaInfo);
			assertEquals(schemaInfoKeyValue.Value, BAR_SCHEMA.SchemaInfo);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(dataProvider = "encodingTypes") public void encodeDecodeNestedKeyValueSchemaInfo(org.apache.pulsar.common.schema.KeyValueEncodingType encodingType)
		public virtual void encodeDecodeNestedKeyValueSchemaInfo(KeyValueEncodingType encodingType)
		{
			Schema<KeyValue<string, Bar>> nestedSchema = Schema.KeyValue(Schema.STRING, BAR_SCHEMA, KeyValueEncodingType.INLINE);
			Schema<KeyValue<Foo, KeyValue<string, Bar>>> kvSchema = Schema.KeyValue(FOO_SCHEMA, nestedSchema, encodingType);
			SchemaInfo kvSchemaInfo = kvSchema.SchemaInfo;
			assertEquals(DefaultImplementation.decodeKeyValueEncodingType(kvSchemaInfo), encodingType);

			SchemaInfo encodedSchemaInfo = DefaultImplementation.encodeKeyValueSchemaInfo(FOO_SCHEMA, nestedSchema, encodingType);
			assertEquals(encodedSchemaInfo, kvSchemaInfo);
			assertEquals(DefaultImplementation.decodeKeyValueEncodingType(encodedSchemaInfo), encodingType);

			KeyValue<SchemaInfo, SchemaInfo> schemaInfoKeyValue = DefaultImplementation.decodeKeyValueSchemaInfo(kvSchemaInfo);

			assertEquals(schemaInfoKeyValue.Key, FOO_SCHEMA.SchemaInfo);
			assertEquals(schemaInfoKeyValue.Value.Type, SchemaType.KEY_VALUE);
			KeyValue<SchemaInfo, SchemaInfo> nestedSchemaInfoKeyValue = DefaultImplementation.decodeKeyValueSchemaInfo(schemaInfoKeyValue.Value);

			assertEquals(nestedSchemaInfoKeyValue.Key, Schema.STRING.SchemaInfo);
			assertEquals(nestedSchemaInfoKeyValue.Value, BAR_SCHEMA.SchemaInfo);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testKeyValueSchemaInfoBackwardCompatibility()
		public virtual void testKeyValueSchemaInfoBackwardCompatibility()
		{
			Schema<KeyValue<Foo, Bar>> kvSchema = Schema.KeyValue(FOO_SCHEMA, BAR_SCHEMA, KeyValueEncodingType.SEPARATED);

			SchemaInfo oldSchemaInfo = (new SchemaInfo()).setName("").setType(SchemaType.KEY_VALUE).setSchema(kvSchema.SchemaInfo.Schema).setProperties(Collections.emptyMap());

			assertEquals(DefaultImplementation.decodeKeyValueEncodingType(oldSchemaInfo), KeyValueEncodingType.INLINE);

			KeyValue<SchemaInfo, SchemaInfo> schemaInfoKeyValue = DefaultImplementation.decodeKeyValueSchemaInfo(oldSchemaInfo);
			// verify the key schema
			SchemaInfo keySchemaInfo = schemaInfoKeyValue.Key;
			assertEquals(SchemaType.BYTES, keySchemaInfo.Type);
			assertArrayEquals("Expected schema = " + FOO_SCHEMA.SchemaInfo.SchemaDefinition + " but found " + keySchemaInfo.SchemaDefinition, FOO_SCHEMA.SchemaInfo.Schema, keySchemaInfo.Schema);
			assertFalse(FOO_SCHEMA.SchemaInfo.Properties.Empty);
			assertTrue(keySchemaInfo.Properties.Empty);
			// verify the value schema
			SchemaInfo valueSchemaInfo = schemaInfoKeyValue.Value;
			assertEquals(SchemaType.BYTES, valueSchemaInfo.Type);
			assertArrayEquals(BAR_SCHEMA.SchemaInfo.Schema, valueSchemaInfo.Schema);
			assertFalse(BAR_SCHEMA.SchemaInfo.Properties.Empty);
			assertTrue(valueSchemaInfo.Properties.Empty);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testKeyValueSchemaInfoToString()
		public virtual void testKeyValueSchemaInfoToString()
		{
			string havePrimitiveType = DefaultImplementation.convertKeyValueSchemaInfoDataToString(KeyValueSchemaInfo.decodeKeyValueSchemaInfo(Schema.KeyValue(Schema.AVRO(typeof(Foo)), Schema.STRING).SchemaInfo));
			assertEquals(havePrimitiveType, KEY_VALUE_SCHEMA_INFO_INCLUDE_PRIMITIVE);

			string notHavePrimitiveType = DefaultImplementation.convertKeyValueSchemaInfoDataToString(KeyValueSchemaInfo.decodeKeyValueSchemaInfo(Schema.KeyValue(Schema.AVRO(typeof(Foo)), Schema.AVRO(typeof(Foo))).SchemaInfo));
			assertEquals(notHavePrimitiveType, KEY_VALUE_SCHEMA_INFO_NOT_INCLUDE_PRIMITIVE);
		}

	}

}