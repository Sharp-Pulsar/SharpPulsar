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
	using Schema = org.apache.pulsar.client.api.Schema;
	using SchemaDefinition = org.apache.pulsar.client.api.schema.SchemaDefinition;
	using MultiVersionSchemaInfoProvider = org.apache.pulsar.client.impl.schema.generic.MultiVersionSchemaInfoProvider;
	using KeyValue = org.apache.pulsar.common.schema.KeyValue;
	using KeyValueEncodingType = org.apache.pulsar.common.schema.KeyValueEncodingType;
	using Assert = org.testng.Assert;
	using Test = org.testng.annotations.Test;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.powermock.api.mockito.PowerMockito.when;

	public class SupportVersioningKeyValueSchemaTest
	{

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testKeyValueVersioningEncodeDecode()
		public virtual void testKeyValueVersioningEncodeDecode()
		{
			MultiVersionSchemaInfoProvider multiVersionSchemaInfoProvider = mock(typeof(MultiVersionSchemaInfoProvider));
			AvroSchema<SchemaTestUtils.Foo> fooSchema = AvroSchema.of(SchemaDefinition.builder<SchemaTestUtils.Foo>().withPojo(typeof(SchemaTestUtils.Foo)).build());
			AvroSchema<SchemaTestUtils.Bar> barSchema = AvroSchema.of(SchemaDefinition.builder<SchemaTestUtils.Bar>().withPojo(typeof(SchemaTestUtils.Bar)).build());
			Schema<KeyValue<SchemaTestUtils.Foo, SchemaTestUtils.Bar>> keyValueSchema = KeyValueSchema.of(fooSchema, barSchema);
			keyValueSchema.SchemaInfoProvider = multiVersionSchemaInfoProvider;

			when(multiVersionSchemaInfoProvider.getSchemaByVersion(any(typeof(sbyte[])))).thenReturn(CompletableFuture.completedFuture(keyValueSchema.SchemaInfo));

			SchemaTestUtils.Bar bar = new SchemaTestUtils.Bar();
			bar.Field1 = true;

			SchemaTestUtils.Foo foo = new SchemaTestUtils.Foo();
			foo.Field1 = "field1";
			foo.Field2 = "field2";
			foo.Field3 = 3;
			foo.Field4 = bar;
			foo.Color = SchemaTestUtils.Color.RED;

			sbyte[] encodeBytes = keyValueSchema.encode(new KeyValue(foo, bar));
			KeyValue<SchemaTestUtils.Foo, SchemaTestUtils.Bar> keyValue = keyValueSchema.decode(encodeBytes, new sbyte[10]);
			Assert.assertEquals(keyValue.Key.Field1, foo.Field1);
			Assert.assertEquals(keyValue.Key.Field2, foo.Field2);
			Assert.assertEquals(keyValue.Key.Field3, foo.Field3);
			Assert.assertEquals(keyValue.Key.Field4, foo.Field4);
			Assert.assertEquals(keyValue.Key.Color, foo.Color);
			Assert.assertTrue(keyValue.Value.Field1);
			Assert.assertEquals(KeyValueEncodingType.valueOf(keyValueSchema.SchemaInfo.Properties.get("kv.encoding.type")), KeyValueEncodingType.INLINE);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testSeparateKeyValueVersioningEncodeDecode()
		public virtual void testSeparateKeyValueVersioningEncodeDecode()
		{
			MultiVersionSchemaInfoProvider multiVersionSchemaInfoProvider = mock(typeof(MultiVersionSchemaInfoProvider));
			AvroSchema<SchemaTestUtils.Foo> fooSchema = AvroSchema.of(SchemaDefinition.builder<SchemaTestUtils.Foo>().withPojo(typeof(SchemaTestUtils.Foo)).build());
			AvroSchema<SchemaTestUtils.Bar> barSchema = AvroSchema.of(SchemaDefinition.builder<SchemaTestUtils.Bar>().withPojo(typeof(SchemaTestUtils.Bar)).build());
			Schema<KeyValue<SchemaTestUtils.Foo, SchemaTestUtils.Bar>> keyValueSchema = KeyValueSchema.of(fooSchema, barSchema, KeyValueEncodingType.SEPARATED);
			keyValueSchema.SchemaInfoProvider = multiVersionSchemaInfoProvider;

			when(multiVersionSchemaInfoProvider.getSchemaByVersion(any(typeof(sbyte[])))).thenReturn(CompletableFuture.completedFuture(keyValueSchema.SchemaInfo));

			SchemaTestUtils.Bar bar = new SchemaTestUtils.Bar();
			bar.Field1 = true;

			SchemaTestUtils.Foo foo = new SchemaTestUtils.Foo();
			foo.Field1 = "field1";
			foo.Field2 = "field2";
			foo.Field3 = 3;
			foo.Field4 = bar;
			foo.Color = SchemaTestUtils.Color.RED;

			sbyte[] encodeBytes = keyValueSchema.encode(new KeyValue(foo, bar));
			KeyValue<SchemaTestUtils.Foo, SchemaTestUtils.Bar> keyValue = ((KeyValueSchema)keyValueSchema).decode(fooSchema.encode(foo), encodeBytes, new sbyte[10]);
			Assert.assertTrue(keyValue.Value.Field1);
			Assert.assertEquals(KeyValueEncodingType.valueOf(keyValueSchema.SchemaInfo.Properties.get("kv.encoding.type")), KeyValueEncodingType.SEPARATED);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testKeyValueDefaultVersioningEncodeDecode()
		public virtual void testKeyValueDefaultVersioningEncodeDecode()
		{
			AvroSchema<SchemaTestUtils.Foo> fooSchema = AvroSchema.of(SchemaDefinition.builder<SchemaTestUtils.Foo>().withPojo(typeof(SchemaTestUtils.Foo)).build());
			AvroSchema<SchemaTestUtils.Bar> barSchema = AvroSchema.of(SchemaDefinition.builder<SchemaTestUtils.Bar>().withPojo(typeof(SchemaTestUtils.Bar)).build());
			Schema<KeyValue<SchemaTestUtils.Foo, SchemaTestUtils.Bar>> keyValueSchema = KeyValueSchema.of(fooSchema, barSchema);

			SchemaTestUtils.Bar bar = new SchemaTestUtils.Bar();
			bar.Field1 = true;

			SchemaTestUtils.Foo foo = new SchemaTestUtils.Foo();
			foo.Field1 = "field1";
			foo.Field2 = "field2";
			foo.Field3 = 3;
			foo.Field4 = bar;
			foo.Color = SchemaTestUtils.Color.RED;

			sbyte[] encodeBytes = keyValueSchema.encode(new KeyValue(foo, bar));
			KeyValue<SchemaTestUtils.Foo, SchemaTestUtils.Bar> keyValue = keyValueSchema.decode(encodeBytes, new sbyte[10]);
			Assert.assertEquals(keyValue.Key.Field1, foo.Field1);
			Assert.assertEquals(keyValue.Key.Field2, foo.Field2);
			Assert.assertEquals(keyValue.Key.Field3, foo.Field3);
			Assert.assertEquals(keyValue.Key.Field4, foo.Field4);
			Assert.assertEquals(keyValue.Key.Color, foo.Color);
			Assert.assertTrue(keyValue.Value.Field1);
			Assert.assertEquals(KeyValueEncodingType.valueOf(keyValueSchema.SchemaInfo.Properties.get("kv.encoding.type")), KeyValueEncodingType.INLINE);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testKeyValueLatestVersioningEncodeDecode()
		public virtual void testKeyValueLatestVersioningEncodeDecode()
		{
			AvroSchema<SchemaTestUtils.Foo> fooSchema = AvroSchema.of(SchemaDefinition.builder<SchemaTestUtils.Foo>().withPojo(typeof(SchemaTestUtils.Foo)).build());
			AvroSchema<SchemaTestUtils.Bar> barSchema = AvroSchema.of(SchemaDefinition.builder<SchemaTestUtils.Bar>().withPojo(typeof(SchemaTestUtils.Bar)).build());
			Schema<KeyValue<SchemaTestUtils.Foo, SchemaTestUtils.Bar>> keyValueSchema = KeyValueSchema.of(fooSchema, barSchema, KeyValueEncodingType.SEPARATED);

			SchemaTestUtils.Bar bar = new SchemaTestUtils.Bar();
			bar.Field1 = true;

			SchemaTestUtils.Foo foo = new SchemaTestUtils.Foo();
			foo.Field1 = "field1";
			foo.Field2 = "field2";
			foo.Field3 = 3;
			foo.Field4 = bar;
			foo.Color = SchemaTestUtils.Color.RED;

			sbyte[] encodeBytes = keyValueSchema.encode(new KeyValue(foo, bar));
			KeyValue<SchemaTestUtils.Foo, SchemaTestUtils.Bar> keyValue = ((KeyValueSchema)keyValueSchema).decode(fooSchema.encode(foo), encodeBytes, new sbyte[10]);
			Assert.assertTrue(keyValue.Value.Field1);
			Assert.assertEquals(KeyValueEncodingType.valueOf(keyValueSchema.SchemaInfo.Properties.get("kv.encoding.type")), KeyValueEncodingType.SEPARATED);
		}
	}

}