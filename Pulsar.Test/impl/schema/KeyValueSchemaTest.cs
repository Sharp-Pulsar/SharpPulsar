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
//	import static org.testng.Assert.assertEquals;

	using Maps = com.google.common.collect.Maps;
	using Slf4j = lombok.@extern.slf4j.Slf4j;
	using Schema = api.Schema;
	using SchemaSerializationException = api.SchemaSerializationException;
	using SchemaDefinition = api.schema.SchemaDefinition;
	using Bar = SchemaTestUtils.Bar;
	using Color = SchemaTestUtils.Color;
	using Foo = SchemaTestUtils.Foo;
	using KeyValue = common.schema.KeyValue;
	using KeyValueEncodingType = common.schema.KeyValueEncodingType;
	using SchemaType = common.schema.SchemaType;
	using Assert = org.testng.Assert;
	using Test = org.testng.annotations.Test;


//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Slf4j public class KeyValueSchemaTest
	public class KeyValueSchemaTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testAllowNullAvroSchemaCreate()
		public virtual void testAllowNullAvroSchemaCreate()
		{
			AvroSchema<Foo> fooSchema = AvroSchema.of(SchemaDefinition.builder<Foo>().withPojo(typeof(Foo)).build());
			AvroSchema<Bar> barSchema = AvroSchema.of(SchemaDefinition.builder<Bar>().withPojo(typeof(Bar)).build());

			Schema<KeyValue<Foo, Bar>> keyValueSchema1 = Schema.KeyValue(fooSchema, barSchema);
			Schema<KeyValue<Foo, Bar>> keyValueSchema2 = Schema.KeyValue(typeof(Foo), typeof(Bar), SchemaType.AVRO);

			assertEquals(keyValueSchema1.SchemaInfo.Type, SchemaType.KEY_VALUE);
			assertEquals(keyValueSchema2.SchemaInfo.Type, SchemaType.KEY_VALUE);

			assertEquals(((KeyValueSchema<Foo, Bar>) keyValueSchema1).KeySchema.SchemaInfo.Type, SchemaType.AVRO);
			assertEquals(((KeyValueSchema<Foo, Bar>) keyValueSchema1).ValueSchema.SchemaInfo.Type, SchemaType.AVRO);
			assertEquals(((KeyValueSchema<Foo, Bar>) keyValueSchema2).KeySchema.SchemaInfo.Type, SchemaType.AVRO);
			assertEquals(((KeyValueSchema<Foo, Bar>) keyValueSchema2).ValueSchema.SchemaInfo.Type, SchemaType.AVRO);

			string schemaInfo1 = new string(keyValueSchema1.SchemaInfo.Schema);
			string schemaInfo2 = new string(keyValueSchema2.SchemaInfo.Schema);
			assertEquals(schemaInfo1, schemaInfo2);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testFillParametersToSchemainfo()
		public virtual void testFillParametersToSchemainfo()
		{
			AvroSchema<Foo> fooSchema = AvroSchema.of(SchemaDefinition.builder<Foo>().withPojo(typeof(Foo)).build());
			AvroSchema<Bar> barSchema = AvroSchema.of(SchemaDefinition.builder<Bar>().withPojo(typeof(Bar)).build());

			fooSchema.SchemaInfo.Name = "foo";
			fooSchema.SchemaInfo.Type = SchemaType.AVRO;
			IDictionary<string, string> keyProperties = Maps.newTreeMap();
			keyProperties["foo.key1"] = "value";
			keyProperties["foo.key2"] = "value";
			fooSchema.SchemaInfo.Properties = keyProperties;
			barSchema.SchemaInfo.Name = "bar";
			barSchema.SchemaInfo.Type = SchemaType.AVRO;
			IDictionary<string, string> valueProperties = Maps.newTreeMap();
			valueProperties["bar.key"] = "key";
			barSchema.SchemaInfo.Properties = valueProperties;
			Schema<KeyValue<Foo, Bar>> keyValueSchema1 = Schema.KeyValue(fooSchema, barSchema);

			assertEquals(keyValueSchema1.SchemaInfo.Properties.get("key.schema.name"), "foo");
			assertEquals(keyValueSchema1.SchemaInfo.Properties.get("key.schema.type"), SchemaType.AVRO.ToString());
			assertEquals(keyValueSchema1.SchemaInfo.Properties.get("key.schema.properties"), "{\"foo.key1\":\"value\",\"foo.key2\":\"value\"}");
			assertEquals(keyValueSchema1.SchemaInfo.Properties.get("value.schema.name"), "bar");
			assertEquals(keyValueSchema1.SchemaInfo.Properties.get("value.schema.type"), SchemaType.AVRO.ToString());
			assertEquals(keyValueSchema1.SchemaInfo.Properties.get("value.schema.properties"), "{\"bar.key\":\"key\"}");
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testNotAllowNullAvroSchemaCreate()
		public virtual void testNotAllowNullAvroSchemaCreate()
		{
			AvroSchema<Foo> fooSchema = AvroSchema.of(SchemaDefinition.builder<Foo>().withPojo(typeof(Foo)).withAlwaysAllowNull(false).build());
			AvroSchema<Bar> barSchema = AvroSchema.of(SchemaDefinition.builder<Bar>().withPojo(typeof(Bar)).withAlwaysAllowNull(false).build());

			Schema<KeyValue<Foo, Bar>> keyValueSchema1 = Schema.KeyValue(fooSchema, barSchema);
			Schema<KeyValue<Foo, Bar>> keyValueSchema2 = Schema.KeyValue(AvroSchema.of(SchemaDefinition.builder<Foo>().withPojo(typeof(Foo)).withAlwaysAllowNull(false).build()), AvroSchema.of(SchemaDefinition.builder<Bar>().withPojo(typeof(Bar)).withAlwaysAllowNull(false).build()));

			assertEquals(keyValueSchema1.SchemaInfo.Type, SchemaType.KEY_VALUE);
			assertEquals(keyValueSchema2.SchemaInfo.Type, SchemaType.KEY_VALUE);

			assertEquals(((KeyValueSchema<Foo, Bar>) keyValueSchema1).KeySchema.SchemaInfo.Type, SchemaType.AVRO);
			assertEquals(((KeyValueSchema<Foo, Bar>) keyValueSchema1).ValueSchema.SchemaInfo.Type, SchemaType.AVRO);
			assertEquals(((KeyValueSchema<Foo, Bar>) keyValueSchema2).KeySchema.SchemaInfo.Type, SchemaType.AVRO);
			assertEquals(((KeyValueSchema<Foo, Bar>) keyValueSchema2).ValueSchema.SchemaInfo.Type, SchemaType.AVRO);

			string schemaInfo1 = new string(keyValueSchema1.SchemaInfo.Schema);
			string schemaInfo2 = new string(keyValueSchema2.SchemaInfo.Schema);
			assertEquals(schemaInfo1, schemaInfo2);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testAllowNullJsonSchemaCreate()
		public virtual void testAllowNullJsonSchemaCreate()
		{
			JSONSchema<Foo> fooSchema = JSONSchema.of(SchemaDefinition.builder<Foo>().withPojo(typeof(Foo)).build());
			JSONSchema<Bar> barSchema = JSONSchema.of(SchemaDefinition.builder<Bar>().withPojo(typeof(Bar)).build());

			Schema<KeyValue<Foo, Bar>> keyValueSchema1 = Schema.KeyValue(fooSchema, barSchema);
			Schema<KeyValue<Foo, Bar>> keyValueSchema2 = Schema.KeyValue(typeof(Foo), typeof(Bar), SchemaType.JSON);
			Schema<KeyValue<Foo, Bar>> keyValueSchema3 = Schema.KeyValue(typeof(Foo), typeof(Bar));

			assertEquals(keyValueSchema1.SchemaInfo.Type, SchemaType.KEY_VALUE);
			assertEquals(keyValueSchema2.SchemaInfo.Type, SchemaType.KEY_VALUE);
			assertEquals(keyValueSchema3.SchemaInfo.Type, SchemaType.KEY_VALUE);

			assertEquals(((KeyValueSchema<Foo, Bar>) keyValueSchema1).KeySchema.SchemaInfo.Type, SchemaType.JSON);
			assertEquals(((KeyValueSchema<Foo, Bar>) keyValueSchema1).ValueSchema.SchemaInfo.Type, SchemaType.JSON);
			assertEquals(((KeyValueSchema<Foo, Bar>) keyValueSchema2).KeySchema.SchemaInfo.Type, SchemaType.JSON);
			assertEquals(((KeyValueSchema<Foo, Bar>) keyValueSchema2).ValueSchema.SchemaInfo.Type, SchemaType.JSON);
			assertEquals(((KeyValueSchema<Foo, Bar>) keyValueSchema3).KeySchema.SchemaInfo.Type, SchemaType.JSON);
			assertEquals(((KeyValueSchema<Foo, Bar>) keyValueSchema3).ValueSchema.SchemaInfo.Type, SchemaType.JSON);

			string schemaInfo1 = new string(keyValueSchema1.SchemaInfo.Schema);
			string schemaInfo2 = new string(keyValueSchema2.SchemaInfo.Schema);
			string schemaInfo3 = new string(keyValueSchema3.SchemaInfo.Schema);
			assertEquals(schemaInfo1, schemaInfo2);
			assertEquals(schemaInfo1, schemaInfo3);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testNotAllowNullJsonSchemaCreate()
		public virtual void testNotAllowNullJsonSchemaCreate()
		{
			JSONSchema<Foo> fooSchema = JSONSchema.of(SchemaDefinition.builder<Foo>().withPojo(typeof(Foo)).withAlwaysAllowNull(false).build());
			JSONSchema<Bar> barSchema = JSONSchema.of(SchemaDefinition.builder<Bar>().withPojo(typeof(Bar)).withAlwaysAllowNull(false).build());

			Schema<KeyValue<Foo, Bar>> keyValueSchema1 = Schema.KeyValue(fooSchema, barSchema);
			Schema<KeyValue<Foo, Bar>> keyValueSchema2 = Schema.KeyValue(JSONSchema.of(SchemaDefinition.builder<Foo>().withPojo(typeof(Foo)).withAlwaysAllowNull(false).build()), JSONSchema.of(SchemaDefinition.builder<Bar>().withPojo(typeof(Bar)).withAlwaysAllowNull(false).build()));

			Schema<KeyValue<Foo, Bar>> keyValueSchema3 = Schema.KeyValue(JSONSchema.of(SchemaDefinition.builder<Foo>().withPojo(typeof(Foo)).withAlwaysAllowNull(false).build()), JSONSchema.of(SchemaDefinition.builder<Bar>().withPojo(typeof(Bar)).withAlwaysAllowNull(false).build()));

			assertEquals(keyValueSchema1.SchemaInfo.Type, SchemaType.KEY_VALUE);
			assertEquals(keyValueSchema2.SchemaInfo.Type, SchemaType.KEY_VALUE);
			assertEquals(keyValueSchema3.SchemaInfo.Type, SchemaType.KEY_VALUE);

			assertEquals(((KeyValueSchema<Foo, Bar>) keyValueSchema1).KeySchema.SchemaInfo.Type, SchemaType.JSON);
			assertEquals(((KeyValueSchema<Foo, Bar>) keyValueSchema1).ValueSchema.SchemaInfo.Type, SchemaType.JSON);
			assertEquals(((KeyValueSchema<Foo, Bar>) keyValueSchema2).KeySchema.SchemaInfo.Type, SchemaType.JSON);
			assertEquals(((KeyValueSchema<Foo, Bar>) keyValueSchema2).ValueSchema.SchemaInfo.Type, SchemaType.JSON);
			assertEquals(((KeyValueSchema<Foo, Bar>) keyValueSchema3).KeySchema.SchemaInfo.Type, SchemaType.JSON);
			assertEquals(((KeyValueSchema<Foo, Bar>) keyValueSchema3).ValueSchema.SchemaInfo.Type, SchemaType.JSON);

			string schemaInfo1 = new string(keyValueSchema1.SchemaInfo.Schema);
			string schemaInfo2 = new string(keyValueSchema2.SchemaInfo.Schema);
			string schemaInfo3 = new string(keyValueSchema3.SchemaInfo.Schema);
			assertEquals(schemaInfo1, schemaInfo2);
			assertEquals(schemaInfo1, schemaInfo3);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testAllowNullSchemaEncodeAndDecode()
		public virtual void testAllowNullSchemaEncodeAndDecode()
		{
			Schema keyValueSchema = Schema.KeyValue(typeof(Foo), typeof(Bar));

			Bar bar = new Bar();
			bar.Field1 = true;

			Foo foo = new Foo();
			foo.Field1 = "field1";
			foo.Field2 = "field2";
			foo.Field3 = 3;
			foo.Field4 = bar;
			foo.Color = Color.RED;

			sbyte[] encodeBytes = keyValueSchema.encode(new KeyValue(foo, bar));
			Assert.assertTrue(encodeBytes.Length > 0);

			KeyValue<Foo, Bar> keyValue = (KeyValue<Foo, Bar>) keyValueSchema.decode(encodeBytes);
			Foo fooBack = keyValue.Key;
			Bar barBack = keyValue.Value;

			assertEquals(foo, fooBack);
			assertEquals(bar, barBack);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testNotAllowNullSchemaEncodeAndDecode()
		public virtual void testNotAllowNullSchemaEncodeAndDecode()
		{
			Schema keyValueSchema = Schema.KeyValue(JSONSchema.of(SchemaDefinition.builder<Foo>().withPojo(typeof(Foo)).withAlwaysAllowNull(false).build()), JSONSchema.of(SchemaDefinition.builder<Bar>().withPojo(typeof(Bar)).withAlwaysAllowNull(false).build()));

			Bar bar = new Bar();
			bar.Field1 = true;

			Foo foo = new Foo();
			foo.Field1 = "field1";
			foo.Field2 = "field2";
			foo.Field3 = 3;
			foo.Field4 = bar;
			foo.Color = Color.RED;

			sbyte[] encodeBytes = keyValueSchema.encode(new KeyValue(foo, bar));
			Assert.assertTrue(encodeBytes.Length > 0);

			KeyValue<Foo, Bar> keyValue = (KeyValue<Foo, Bar>) keyValueSchema.decode(encodeBytes);
			Foo fooBack = keyValue.Key;
			Bar barBack = keyValue.Value;

			assertEquals(foo, fooBack);
			assertEquals(bar, barBack);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testDefaultKeyValueEncodingTypeSchemaEncodeAndDecode()
		public virtual void testDefaultKeyValueEncodingTypeSchemaEncodeAndDecode()
		{
			AvroSchema<Foo> fooSchema = AvroSchema.of(SchemaDefinition.builder<Foo>().withPojo(typeof(Foo)).build());
			AvroSchema<Bar> barSchema = AvroSchema.of(SchemaDefinition.builder<Bar>().withPojo(typeof(Bar)).build());

			Schema<KeyValue<Foo, Bar>> keyValueSchema = Schema.KeyValue(fooSchema, barSchema);

			Bar bar = new Bar();
			bar.Field1 = true;

			Foo foo = new Foo();
			foo.Field1 = "field1";
			foo.Field2 = "field2";
			foo.Field3 = 3;
			foo.Field4 = bar;
			foo.Color = Color.RED;

			// Check kv.encoding.type default not set value
			sbyte[] encodeBytes = keyValueSchema.encode(new KeyValue(foo, bar));
			Assert.assertTrue(encodeBytes.Length > 0);

			KeyValue<Foo, Bar> keyValue = (KeyValue<Foo, Bar>) keyValueSchema.decode(encodeBytes);
			Foo fooBack = keyValue.Key;
			Bar barBack = keyValue.Value;

			assertEquals(foo, fooBack);
			assertEquals(bar, barBack);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testInlineKeyValueEncodingTypeSchemaEncodeAndDecode()
		public virtual void testInlineKeyValueEncodingTypeSchemaEncodeAndDecode()
		{

			AvroSchema<Foo> fooSchema = AvroSchema.of(SchemaDefinition.builder<Foo>().withPojo(typeof(Foo)).build());
			AvroSchema<Bar> barSchema = AvroSchema.of(SchemaDefinition.builder<Bar>().withPojo(typeof(Bar)).build());

			Schema<KeyValue<Foo, Bar>> keyValueSchema = Schema.KeyValue(fooSchema, barSchema, KeyValueEncodingType.INLINE);


			Bar bar = new Bar();
			bar.Field1 = true;

			Foo foo = new Foo();
			foo.Field1 = "field1";
			foo.Field2 = "field2";
			foo.Field3 = 3;
			foo.Field4 = bar;
			foo.Color = Color.RED;

			// Check kv.encoding.type INLINE
			sbyte[] encodeBytes = keyValueSchema.encode(new KeyValue(foo, bar));
			Assert.assertTrue(encodeBytes.Length > 0);
			KeyValue<Foo, Bar> keyValue = (KeyValue<Foo, Bar>) keyValueSchema.decode(encodeBytes);
			Foo fooBack = keyValue.Key;
			Bar barBack = keyValue.Value;
			assertEquals(foo, fooBack);
			assertEquals(bar, barBack);

		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testSeparatedKeyValueEncodingTypeSchemaEncodeAndDecode()
		public virtual void testSeparatedKeyValueEncodingTypeSchemaEncodeAndDecode()
		{
			AvroSchema<Foo> fooSchema = AvroSchema.of(SchemaDefinition.builder<Foo>().withPojo(typeof(Foo)).build());
			AvroSchema<Bar> barSchema = AvroSchema.of(SchemaDefinition.builder<Bar>().withPojo(typeof(Bar)).build());

			Schema<KeyValue<Foo, Bar>> keyValueSchema = Schema.KeyValue(fooSchema, barSchema, KeyValueEncodingType.SEPARATED);

			Bar bar = new Bar();
			bar.Field1 = true;

			Foo foo = new Foo();
			foo.Field1 = "field1";
			foo.Field2 = "field2";
			foo.Field3 = 3;
			foo.Field4 = bar;
			foo.Color = Color.RED;

			// Check kv.encoding.type SEPARATED
			sbyte[] encodeBytes = keyValueSchema.encode(new KeyValue(foo, bar));
			Assert.assertTrue(encodeBytes.Length > 0);
			try
			{
				keyValueSchema.decode(encodeBytes);
				Assert.fail("This method cannot be used under this SEPARATED encoding type");
			}
			catch (SchemaSerializationException e)
			{
				Assert.assertTrue(e.Message.contains("This method cannot be used under this SEPARATED encoding type"));
			}
			KeyValue<Foo, Bar> keyValue = ((KeyValueSchema)keyValueSchema).decode(fooSchema.encode(foo), encodeBytes, null);
			Foo fooBack = keyValue.Key;
			Bar barBack = keyValue.Value;
			assertEquals(foo, fooBack);
			assertEquals(bar, barBack);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testAllowNullBytesSchemaEncodeAndDecode()
		public virtual void testAllowNullBytesSchemaEncodeAndDecode()
		{
			AvroSchema<Foo> fooAvroSchema = AvroSchema.of(SchemaDefinition.builder<Foo>().withPojo(typeof(Foo)).build());
			AvroSchema<Bar> barAvroSchema = AvroSchema.of(SchemaDefinition.builder<Bar>().withPojo(typeof(Bar)).build());

			Bar bar = new Bar();
			bar.Field1 = true;

			Foo foo = new Foo();
			foo.Field1 = "field1";
			foo.Field2 = "field2";
			foo.Field3 = 3;
			foo.Field4 = bar;
			foo.Color = Color.RED;
			foo.FieldUnableNull = "notNull";

			sbyte[] fooBytes = fooAvroSchema.encode(foo);
			sbyte[] barBytes = barAvroSchema.encode(bar);

			sbyte[] encodeBytes = Schema.KV_BYTES().encode(new KeyValue<>(fooBytes, barBytes));
			KeyValue<sbyte[], sbyte[]> decodeKV = Schema.KV_BYTES().decode(encodeBytes);

			Foo fooBack = fooAvroSchema.decode(decodeKV.Key);
			Bar barBack = barAvroSchema.decode(decodeKV.Value);

			assertEquals(foo, fooBack);
			assertEquals(bar, barBack);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testNotAllowNullBytesSchemaEncodeAndDecode()
		public virtual void testNotAllowNullBytesSchemaEncodeAndDecode()
		{
			AvroSchema<Foo> fooAvroSchema = AvroSchema.of(SchemaDefinition.builder<Foo>().withPojo(typeof(Foo)).withAlwaysAllowNull(false).build());
			AvroSchema<Bar> barAvroSchema = AvroSchema.of(SchemaDefinition.builder<Bar>().withPojo(typeof(Bar)).withAlwaysAllowNull(false).build());

			Bar bar = new Bar();
			bar.Field1 = true;

			Foo foo = new Foo();
			foo.Field1 = "field1";
			foo.Field2 = "field2";
			foo.Field3 = 3;
			foo.Field4 = bar;
			foo.Color = Color.RED;
			foo.FieldUnableNull = "notNull";

			sbyte[] fooBytes = fooAvroSchema.encode(foo);
			sbyte[] barBytes = barAvroSchema.encode(bar);

			sbyte[] encodeBytes = Schema.KV_BYTES().encode(new KeyValue<>(fooBytes, barBytes));
			KeyValue<sbyte[], sbyte[]> decodeKV = Schema.KV_BYTES().decode(encodeBytes);

			Foo fooBack = fooAvroSchema.decode(decodeKV.Key);
			Bar barBack = barAvroSchema.decode(decodeKV.Value);

			assertEquals(foo, fooBack);
			assertEquals(bar, barBack);
		}
	}

}