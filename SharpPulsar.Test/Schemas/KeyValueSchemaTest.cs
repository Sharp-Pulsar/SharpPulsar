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
namespace SharpPulsar.Test.Schema
{
	public class KeyValueSchemaTest
	{
		public virtual void TestAllowNullAvroSchemaCreate()
		{
			AvroSchema<Foo> FooSchema = AvroSchema.of(SchemaDefinition.builder<Foo>().withPojo(typeof(Foo)).build());
			AvroSchema<Bar> BarSchema = AvroSchema.of(SchemaDefinition.builder<Bar>().withPojo(typeof(Bar)).build());

			Schema<KeyValue<Foo, Bar>> KeyValueSchema1 = Schema.KeyValue(FooSchema, BarSchema);
			Schema<KeyValue<Foo, Bar>> KeyValueSchema2 = Schema.KeyValue(typeof(Foo), typeof(Bar), SchemaType.AVRO);

			assertEquals(KeyValueSchema1.SchemaInfo.Type, SchemaType.KEY_VALUE);
			assertEquals(KeyValueSchema2.SchemaInfo.Type, SchemaType.KEY_VALUE);

			assertEquals(((KeyValueSchema<Foo, Bar>) KeyValueSchema1).KeySchema.SchemaInfo.Type, SchemaType.AVRO);
			assertEquals(((KeyValueSchema<Foo, Bar>) KeyValueSchema1).ValueSchema.SchemaInfo.Type, SchemaType.AVRO);
			assertEquals(((KeyValueSchema<Foo, Bar>) KeyValueSchema2).KeySchema.SchemaInfo.Type, SchemaType.AVRO);
			assertEquals(((KeyValueSchema<Foo, Bar>) KeyValueSchema2).ValueSchema.SchemaInfo.Type, SchemaType.AVRO);

			string SchemaInfo1 = new string(KeyValueSchema1.SchemaInfo.Schema);
			string SchemaInfo2 = new string(KeyValueSchema2.SchemaInfo.Schema);
			assertEquals(SchemaInfo1, SchemaInfo2);
		}

		public virtual void TestFillParametersToSchemainfo()
		{
			AvroSchema<Foo> FooSchema = AvroSchema.of(SchemaDefinition.builder<Foo>().withPojo(typeof(Foo)).build());
			AvroSchema<Bar> BarSchema = AvroSchema.of(SchemaDefinition.builder<Bar>().withPojo(typeof(Bar)).build());

			FooSchema.SchemaInfo.Name = "foo";
			FooSchema.SchemaInfo.Type = SchemaType.AVRO;
			IDictionary<string, string> KeyProperties = Maps.newTreeMap();
			KeyProperties["foo.key1"] = "value";
			KeyProperties["foo.key2"] = "value";
			FooSchema.SchemaInfo.Properties = KeyProperties;
			BarSchema.SchemaInfo.Name = "bar";
			BarSchema.SchemaInfo.Type = SchemaType.AVRO;
			IDictionary<string, string> ValueProperties = Maps.newTreeMap();
			ValueProperties["bar.key"] = "key";
			BarSchema.SchemaInfo.Properties = ValueProperties;
			Schema<KeyValue<Foo, Bar>> KeyValueSchema1 = Schema.KeyValue(FooSchema, BarSchema);

			assertEquals(KeyValueSchema1.SchemaInfo.Properties.get("key.schema.name"), "foo");
			assertEquals(KeyValueSchema1.SchemaInfo.Properties.get("key.schema.type"), SchemaType.AVRO.ToString());
			assertEquals(KeyValueSchema1.SchemaInfo.Properties.get("key.schema.properties"), "{\"foo.key1\":\"value\",\"foo.key2\":\"value\"}");
			assertEquals(KeyValueSchema1.SchemaInfo.Properties.get("value.schema.name"), "bar");
			assertEquals(KeyValueSchema1.SchemaInfo.Properties.get("value.schema.type"), SchemaType.AVRO.ToString());
			assertEquals(KeyValueSchema1.SchemaInfo.Properties.get("value.schema.properties"), "{\"bar.key\":\"key\"}");
		}

		public virtual void TestNotAllowNullAvroSchemaCreate()
		{
			AvroSchema<Foo> FooSchema = AvroSchema.of(SchemaDefinition.builder<Foo>().withPojo(typeof(Foo)).withAlwaysAllowNull(false).build());
			AvroSchema<Bar> BarSchema = AvroSchema.of(SchemaDefinition.builder<Bar>().withPojo(typeof(Bar)).withAlwaysAllowNull(false).build());

			Schema<KeyValue<Foo, Bar>> KeyValueSchema1 = Schema.KeyValue(FooSchema, BarSchema);
			Schema<KeyValue<Foo, Bar>> KeyValueSchema2 = Schema.KeyValue(AvroSchema.of(SchemaDefinition.builder<Foo>().withPojo(typeof(Foo)).withAlwaysAllowNull(false).build()), AvroSchema.of(SchemaDefinition.builder<Bar>().withPojo(typeof(Bar)).withAlwaysAllowNull(false).build()));

			assertEquals(KeyValueSchema1.SchemaInfo.Type, SchemaType.KEY_VALUE);
			assertEquals(KeyValueSchema2.SchemaInfo.Type, SchemaType.KEY_VALUE);

			assertEquals(((KeyValueSchema<Foo, Bar>) KeyValueSchema1).KeySchema.SchemaInfo.Type, SchemaType.AVRO);
			assertEquals(((KeyValueSchema<Foo, Bar>) KeyValueSchema1).ValueSchema.SchemaInfo.Type, SchemaType.AVRO);
			assertEquals(((KeyValueSchema<Foo, Bar>) KeyValueSchema2).KeySchema.SchemaInfo.Type, SchemaType.AVRO);
			assertEquals(((KeyValueSchema<Foo, Bar>) KeyValueSchema2).ValueSchema.SchemaInfo.Type, SchemaType.AVRO);

			string SchemaInfo1 = new string(KeyValueSchema1.SchemaInfo.Schema);
			string SchemaInfo2 = new string(KeyValueSchema2.SchemaInfo.Schema);
			assertEquals(SchemaInfo1, SchemaInfo2);
		}


		public virtual void TestAllowNullJsonSchemaCreate()
		{
			JSONSchema<Foo> FooSchema = JSONSchema.of(SchemaDefinition.builder<Foo>().withPojo(typeof(Foo)).build());
			JSONSchema<Bar> BarSchema = JSONSchema.of(SchemaDefinition.builder<Bar>().withPojo(typeof(Bar)).build());

			Schema<KeyValue<Foo, Bar>> KeyValueSchema1 = Schema.KeyValue(FooSchema, BarSchema);
			Schema<KeyValue<Foo, Bar>> KeyValueSchema2 = Schema.KeyValue(typeof(Foo), typeof(Bar), SchemaType.JSON);
			Schema<KeyValue<Foo, Bar>> KeyValueSchema3 = Schema.KeyValue(typeof(Foo), typeof(Bar));

			assertEquals(KeyValueSchema1.SchemaInfo.Type, SchemaType.KEY_VALUE);
			assertEquals(KeyValueSchema2.SchemaInfo.Type, SchemaType.KEY_VALUE);
			assertEquals(KeyValueSchema3.SchemaInfo.Type, SchemaType.KEY_VALUE);

			assertEquals(((KeyValueSchema<Foo, Bar>) KeyValueSchema1).KeySchema.SchemaInfo.Type, SchemaType.JSON);
			assertEquals(((KeyValueSchema<Foo, Bar>) KeyValueSchema1).ValueSchema.SchemaInfo.Type, SchemaType.JSON);
			assertEquals(((KeyValueSchema<Foo, Bar>) KeyValueSchema2).KeySchema.SchemaInfo.Type, SchemaType.JSON);
			assertEquals(((KeyValueSchema<Foo, Bar>) KeyValueSchema2).ValueSchema.SchemaInfo.Type, SchemaType.JSON);
			assertEquals(((KeyValueSchema<Foo, Bar>) KeyValueSchema3).KeySchema.SchemaInfo.Type, SchemaType.JSON);
			assertEquals(((KeyValueSchema<Foo, Bar>) KeyValueSchema3).ValueSchema.SchemaInfo.Type, SchemaType.JSON);

			string SchemaInfo1 = new string(KeyValueSchema1.SchemaInfo.Schema);
			string SchemaInfo2 = new string(KeyValueSchema2.SchemaInfo.Schema);
			string SchemaInfo3 = new string(KeyValueSchema3.SchemaInfo.Schema);
			assertEquals(SchemaInfo1, SchemaInfo2);
			assertEquals(SchemaInfo1, SchemaInfo3);
		}


		public virtual void TestNotAllowNullJsonSchemaCreate()
		{
			JSONSchema<Foo> FooSchema = JSONSchema.of(SchemaDefinition.builder<Foo>().withPojo(typeof(Foo)).withAlwaysAllowNull(false).build());
			JSONSchema<Bar> BarSchema = JSONSchema.of(SchemaDefinition.builder<Bar>().withPojo(typeof(Bar)).withAlwaysAllowNull(false).build());

			Schema<KeyValue<Foo, Bar>> KeyValueSchema1 = Schema.KeyValue(FooSchema, BarSchema);
			Schema<KeyValue<Foo, Bar>> KeyValueSchema2 = Schema.KeyValue(JSONSchema.of(SchemaDefinition.builder<Foo>().withPojo(typeof(Foo)).withAlwaysAllowNull(false).build()), JSONSchema.of(SchemaDefinition.builder<Bar>().withPojo(typeof(Bar)).withAlwaysAllowNull(false).build()));

			Schema<KeyValue<Foo, Bar>> KeyValueSchema3 = Schema.KeyValue(JSONSchema.of(SchemaDefinition.builder<Foo>().withPojo(typeof(Foo)).withAlwaysAllowNull(false).build()), JSONSchema.of(SchemaDefinition.builder<Bar>().withPojo(typeof(Bar)).withAlwaysAllowNull(false).build()));

			assertEquals(KeyValueSchema1.SchemaInfo.Type, SchemaType.KEY_VALUE);
			assertEquals(KeyValueSchema2.SchemaInfo.Type, SchemaType.KEY_VALUE);
			assertEquals(KeyValueSchema3.SchemaInfo.Type, SchemaType.KEY_VALUE);

			assertEquals(((KeyValueSchema<Foo, Bar>) KeyValueSchema1).KeySchema.SchemaInfo.Type, SchemaType.JSON);
			assertEquals(((KeyValueSchema<Foo, Bar>) KeyValueSchema1).ValueSchema.SchemaInfo.Type, SchemaType.JSON);
			assertEquals(((KeyValueSchema<Foo, Bar>) KeyValueSchema2).KeySchema.SchemaInfo.Type, SchemaType.JSON);
			assertEquals(((KeyValueSchema<Foo, Bar>) KeyValueSchema2).ValueSchema.SchemaInfo.Type, SchemaType.JSON);
			assertEquals(((KeyValueSchema<Foo, Bar>) KeyValueSchema3).KeySchema.SchemaInfo.Type, SchemaType.JSON);
			assertEquals(((KeyValueSchema<Foo, Bar>) KeyValueSchema3).ValueSchema.SchemaInfo.Type, SchemaType.JSON);

			string SchemaInfo1 = new string(KeyValueSchema1.SchemaInfo.Schema);
			string SchemaInfo2 = new string(KeyValueSchema2.SchemaInfo.Schema);
			string SchemaInfo3 = new string(KeyValueSchema3.SchemaInfo.Schema);
			assertEquals(SchemaInfo1, SchemaInfo2);
			assertEquals(SchemaInfo1, SchemaInfo3);
		}


		public virtual void TestAllowNullSchemaEncodeAndDecode()
		{
			Schema KeyValueSchema = Schema.KeyValue(typeof(Foo), typeof(Bar));

			Bar Bar = new Bar();
			Bar.Field1 = true;

			Foo Foo = new Foo();
			Foo.Field1 = "field1";
			Foo.Field2 = "field2";
			Foo.Field3 = 3;
			Foo.Field4 = Bar;
			Foo.Color = Color.RED;

			sbyte[] EncodeBytes = KeyValueSchema.encode(new KeyValue(Foo, Bar));
			Assert.assertTrue(EncodeBytes.Length > 0);

			KeyValue<Foo, Bar> KeyValue = (KeyValue<Foo, Bar>) KeyValueSchema.decode(EncodeBytes);
			Foo FooBack = KeyValue.Key;
			Bar BarBack = KeyValue.Value;

			assertEquals(Foo, FooBack);
			assertEquals(Bar, BarBack);
		}


		public virtual void TestNotAllowNullSchemaEncodeAndDecode()
		{
			Schema KeyValueSchema = Schema.KeyValue(JSONSchema.of(SchemaDefinition.builder<Foo>().withPojo(typeof(Foo)).withAlwaysAllowNull(false).build()), JSONSchema.of(SchemaDefinition.builder<Bar>().withPojo(typeof(Bar)).withAlwaysAllowNull(false).build()));

			Bar Bar = new Bar();
			Bar.Field1 = true;

			Foo Foo = new Foo();
			Foo.Field1 = "field1";
			Foo.Field2 = "field2";
			Foo.Field3 = 3;
			Foo.Field4 = Bar;
			Foo.Color = Color.RED;

			sbyte[] EncodeBytes = KeyValueSchema.encode(new KeyValue(Foo, Bar));
			Assert.assertTrue(EncodeBytes.Length > 0);

			KeyValue<Foo, Bar> KeyValue = (KeyValue<Foo, Bar>) KeyValueSchema.decode(EncodeBytes);
			Foo FooBack = KeyValue.Key;
			Bar BarBack = KeyValue.Value;

			assertEquals(Foo, FooBack);
			assertEquals(Bar, BarBack);
		}


		public virtual void TestDefaultKeyValueEncodingTypeSchemaEncodeAndDecode()
		{
			AvroSchema<Foo> FooSchema = AvroSchema.of(SchemaDefinition.builder<Foo>().withPojo(typeof(Foo)).build());
			AvroSchema<Bar> BarSchema = AvroSchema.of(SchemaDefinition.builder<Bar>().withPojo(typeof(Bar)).build());

			Schema<KeyValue<Foo, Bar>> KeyValueSchema = Schema.KeyValue(FooSchema, BarSchema);

			Bar Bar = new Bar();
			Bar.Field1 = true;

			Foo Foo = new Foo();
			Foo.Field1 = "field1";
			Foo.Field2 = "field2";
			Foo.Field3 = 3;
			Foo.Field4 = Bar;
			Foo.Color = Color.RED;

			// Check kv.encoding.type default not set value
			sbyte[] EncodeBytes = KeyValueSchema.encode(new KeyValue(Foo, Bar));
			Assert.assertTrue(EncodeBytes.Length > 0);

			KeyValue<Foo, Bar> KeyValue = (KeyValue<Foo, Bar>) KeyValueSchema.decode(EncodeBytes);
			Foo FooBack = KeyValue.Key;
			Bar BarBack = KeyValue.Value;

			assertEquals(Foo, FooBack);
			assertEquals(Bar, BarBack);
		}


		public virtual void TestInlineKeyValueEncodingTypeSchemaEncodeAndDecode()
		{

			AvroSchema<Foo> FooSchema = AvroSchema.of(SchemaDefinition.builder<Foo>().withPojo(typeof(Foo)).build());
			AvroSchema<Bar> BarSchema = AvroSchema.of(SchemaDefinition.builder<Bar>().withPojo(typeof(Bar)).build());

			Schema<KeyValue<Foo, Bar>> KeyValueSchema = Schema.KeyValue(FooSchema, BarSchema, KeyValueEncodingType.INLINE);


			Bar Bar = new Bar();
			Bar.Field1 = true;

			Foo Foo = new Foo();
			Foo.Field1 = "field1";
			Foo.Field2 = "field2";
			Foo.Field3 = 3;
			Foo.Field4 = Bar;
			Foo.Color = Color.RED;

			// Check kv.encoding.type INLINE
			sbyte[] EncodeBytes = KeyValueSchema.encode(new KeyValue(Foo, Bar));
			Assert.assertTrue(EncodeBytes.Length > 0);
			KeyValue<Foo, Bar> KeyValue = (KeyValue<Foo, Bar>) KeyValueSchema.decode(EncodeBytes);
			Foo FooBack = KeyValue.Key;
			Bar BarBack = KeyValue.Value;
			assertEquals(Foo, FooBack);
			assertEquals(Bar, BarBack);

		}


		public virtual void TestSeparatedKeyValueEncodingTypeSchemaEncodeAndDecode()
		{
			AvroSchema<Foo> FooSchema = AvroSchema.of(SchemaDefinition.builder<Foo>().withPojo(typeof(Foo)).build());
			AvroSchema<Bar> BarSchema = AvroSchema.of(SchemaDefinition.builder<Bar>().withPojo(typeof(Bar)).build());

			Schema<KeyValue<Foo, Bar>> KeyValueSchema = Schema.KeyValue(FooSchema, BarSchema, KeyValueEncodingType.SEPARATED);

			Bar Bar = new Bar();
			Bar.Field1 = true;

			Foo Foo = new Foo();
			Foo.Field1 = "field1";
			Foo.Field2 = "field2";
			Foo.Field3 = 3;
			Foo.Field4 = Bar;
			Foo.Color = Color.RED;

			// Check kv.encoding.type SEPARATED
			sbyte[] EncodeBytes = KeyValueSchema.encode(new KeyValue(Foo, Bar));
			Assert.assertTrue(EncodeBytes.Length > 0);
			try
			{
				KeyValueSchema.decode(EncodeBytes);
				Assert.fail("This method cannot be used under this SEPARATED encoding type");
			}
			catch (SchemaSerializationException E)
			{
				Assert.assertTrue(E.Message.contains("This method cannot be used under this SEPARATED encoding type"));
			}
			KeyValue<Foo, Bar> KeyValue = ((KeyValueSchema)KeyValueSchema).decode(FooSchema.encode(Foo), EncodeBytes, null);
			Foo FooBack = KeyValue.Key;
			Bar BarBack = KeyValue.Value;
			assertEquals(Foo, FooBack);
			assertEquals(Bar, BarBack);
		}


		public virtual void TestAllowNullBytesSchemaEncodeAndDecode()
		{
			AvroSchema<Foo> FooAvroSchema = AvroSchema.of(SchemaDefinition.builder<Foo>().withPojo(typeof(Foo)).build());
			AvroSchema<Bar> BarAvroSchema = AvroSchema.of(SchemaDefinition.builder<Bar>().withPojo(typeof(Bar)).build());

			Bar Bar = new Bar();
			Bar.Field1 = true;

			Foo Foo = new Foo();
			Foo.Field1 = "field1";
			Foo.Field2 = "field2";
			Foo.Field3 = 3;
			Foo.Field4 = Bar;
			Foo.Color = Color.RED;
			Foo.FieldUnableNull = "notNull";

			sbyte[] FooBytes = FooAvroSchema.encode(Foo);
			sbyte[] BarBytes = BarAvroSchema.encode(Bar);

			sbyte[] EncodeBytes = Schema.KV_BYTES().encode(new KeyValue<>(FooBytes, BarBytes));
			KeyValue<sbyte[], sbyte[]> DecodeKV = Schema.KV_BYTES().decode(EncodeBytes);

			Foo FooBack = FooAvroSchema.decode(DecodeKV.Key);
			Bar BarBack = BarAvroSchema.decode(DecodeKV.Value);

			assertEquals(Foo, FooBack);
			assertEquals(Bar, BarBack);
		}


		public virtual void TestNotAllowNullBytesSchemaEncodeAndDecode()
		{
			AvroSchema<Foo> FooAvroSchema = AvroSchema.of(SchemaDefinition.builder<Foo>().withPojo(typeof(Foo)).withAlwaysAllowNull(false).build());
			AvroSchema<Bar> BarAvroSchema = AvroSchema.of(SchemaDefinition.builder<Bar>().withPojo(typeof(Bar)).withAlwaysAllowNull(false).build());

			Bar Bar = new Bar();
			Bar.Field1 = true;

			Foo Foo = new Foo();
			Foo.Field1 = "field1";
			Foo.Field2 = "field2";
			Foo.Field3 = 3;
			Foo.Field4 = Bar;
			Foo.Color = Color.RED;
			Foo.FieldUnableNull = "notNull";

			sbyte[] FooBytes = FooAvroSchema.encode(Foo);
			sbyte[] BarBytes = BarAvroSchema.encode(Bar);

			sbyte[] EncodeBytes = Schema.KV_BYTES().encode(new KeyValue<>(FooBytes, BarBytes));
			KeyValue<sbyte[], sbyte[]> DecodeKV = Schema.KV_BYTES().decode(EncodeBytes);

			Foo FooBack = FooAvroSchema.decode(DecodeKV.Key);
			Bar BarBack = BarAvroSchema.decode(DecodeKV.Value);

			assertEquals(Foo, FooBack);
			assertEquals(Bar, BarBack);
		}
	}

}