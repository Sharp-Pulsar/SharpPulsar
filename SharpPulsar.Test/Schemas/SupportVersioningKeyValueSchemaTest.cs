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
	using Schema = org.apache.pulsar.client.api.Schema;
	using SchemaDefinition = org.apache.pulsar.client.api.schema.SchemaDefinition;
	using MultiVersionSchemaInfoProvider = Org.Apache.Pulsar.Client.Impl.Schema.Generic.MultiVersionSchemaInfoProvider;
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
		public virtual void TestKeyValueVersioningEncodeDecode()
		{
			MultiVersionSchemaInfoProvider MultiVersionSchemaInfoProvider = mock(typeof(MultiVersionSchemaInfoProvider));
			AvroSchema<SchemaTestUtils.Foo> FooSchema = AvroSchema.of(SchemaDefinition.builder<SchemaTestUtils.Foo>().withPojo(typeof(SchemaTestUtils.Foo)).build());
			AvroSchema<SchemaTestUtils.Bar> BarSchema = AvroSchema.of(SchemaDefinition.builder<SchemaTestUtils.Bar>().withPojo(typeof(SchemaTestUtils.Bar)).build());
			Schema<KeyValue<SchemaTestUtils.Foo, SchemaTestUtils.Bar>> KeyValueSchema = KeyValueSchema.of(FooSchema, BarSchema);
			KeyValueSchema.SchemaInfoProvider = MultiVersionSchemaInfoProvider;

			when(MultiVersionSchemaInfoProvider.getSchemaByVersion(any(typeof(sbyte[])))).thenReturn(CompletableFuture.completedFuture(KeyValueSchema.SchemaInfo));

			SchemaTestUtils.Bar Bar = new SchemaTestUtils.Bar();
			Bar.Field1 = true;

			SchemaTestUtils.Foo Foo = new SchemaTestUtils.Foo();
			Foo.Field1 = "field1";
			Foo.Field2 = "field2";
			Foo.Field3 = 3;
			Foo.Field4 = Bar;
			Foo.Color = SchemaTestUtils.Color.RED;

			sbyte[] EncodeBytes = KeyValueSchema.encode(new KeyValue(Foo, Bar));
			KeyValue<SchemaTestUtils.Foo, SchemaTestUtils.Bar> KeyValue = KeyValueSchema.decode(EncodeBytes, new sbyte[10]);
			Assert.assertEquals(KeyValue.Key.Field1, Foo.Field1);
			Assert.assertEquals(KeyValue.Key.Field2, Foo.Field2);
			Assert.assertEquals(KeyValue.Key.Field3, Foo.Field3);
			Assert.assertEquals(KeyValue.Key.Field4, Foo.Field4);
			Assert.assertEquals(KeyValue.Key.Color, Foo.Color);
			Assert.assertTrue(KeyValue.Value.Field1);
			Assert.assertEquals(KeyValueEncodingType.valueOf(KeyValueSchema.SchemaInfo.Properties.get("kv.encoding.type")), KeyValueEncodingType.INLINE);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testSeparateKeyValueVersioningEncodeDecode()
		public virtual void TestSeparateKeyValueVersioningEncodeDecode()
		{
			MultiVersionSchemaInfoProvider MultiVersionSchemaInfoProvider = mock(typeof(MultiVersionSchemaInfoProvider));
			AvroSchema<SchemaTestUtils.Foo> FooSchema = AvroSchema.of(SchemaDefinition.builder<SchemaTestUtils.Foo>().withPojo(typeof(SchemaTestUtils.Foo)).build());
			AvroSchema<SchemaTestUtils.Bar> BarSchema = AvroSchema.of(SchemaDefinition.builder<SchemaTestUtils.Bar>().withPojo(typeof(SchemaTestUtils.Bar)).build());
			Schema<KeyValue<SchemaTestUtils.Foo, SchemaTestUtils.Bar>> KeyValueSchema = KeyValueSchema.of(FooSchema, BarSchema, KeyValueEncodingType.SEPARATED);
			KeyValueSchema.SchemaInfoProvider = MultiVersionSchemaInfoProvider;

			when(MultiVersionSchemaInfoProvider.getSchemaByVersion(any(typeof(sbyte[])))).thenReturn(CompletableFuture.completedFuture(KeyValueSchema.SchemaInfo));

			SchemaTestUtils.Bar Bar = new SchemaTestUtils.Bar();
			Bar.Field1 = true;

			SchemaTestUtils.Foo Foo = new SchemaTestUtils.Foo();
			Foo.Field1 = "field1";
			Foo.Field2 = "field2";
			Foo.Field3 = 3;
			Foo.Field4 = Bar;
			Foo.Color = SchemaTestUtils.Color.RED;

			sbyte[] EncodeBytes = KeyValueSchema.encode(new KeyValue(Foo, Bar));
			KeyValue<SchemaTestUtils.Foo, SchemaTestUtils.Bar> KeyValue = ((KeyValueSchema)KeyValueSchema).decode(FooSchema.encode(Foo), EncodeBytes, new sbyte[10]);
			Assert.assertTrue(KeyValue.Value.Field1);
			Assert.assertEquals(KeyValueEncodingType.valueOf(KeyValueSchema.SchemaInfo.Properties.get("kv.encoding.type")), KeyValueEncodingType.SEPARATED);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testKeyValueDefaultVersioningEncodeDecode()
		public virtual void TestKeyValueDefaultVersioningEncodeDecode()
		{
			AvroSchema<SchemaTestUtils.Foo> FooSchema = AvroSchema.of(SchemaDefinition.builder<SchemaTestUtils.Foo>().withPojo(typeof(SchemaTestUtils.Foo)).build());
			AvroSchema<SchemaTestUtils.Bar> BarSchema = AvroSchema.of(SchemaDefinition.builder<SchemaTestUtils.Bar>().withPojo(typeof(SchemaTestUtils.Bar)).build());
			Schema<KeyValue<SchemaTestUtils.Foo, SchemaTestUtils.Bar>> KeyValueSchema = KeyValueSchema.of(FooSchema, BarSchema);

			SchemaTestUtils.Bar Bar = new SchemaTestUtils.Bar();
			Bar.Field1 = true;

			SchemaTestUtils.Foo Foo = new SchemaTestUtils.Foo();
			Foo.Field1 = "field1";
			Foo.Field2 = "field2";
			Foo.Field3 = 3;
			Foo.Field4 = Bar;
			Foo.Color = SchemaTestUtils.Color.RED;

			sbyte[] EncodeBytes = KeyValueSchema.encode(new KeyValue(Foo, Bar));
			KeyValue<SchemaTestUtils.Foo, SchemaTestUtils.Bar> KeyValue = KeyValueSchema.decode(EncodeBytes, new sbyte[10]);
			Assert.assertEquals(KeyValue.Key.Field1, Foo.Field1);
			Assert.assertEquals(KeyValue.Key.Field2, Foo.Field2);
			Assert.assertEquals(KeyValue.Key.Field3, Foo.Field3);
			Assert.assertEquals(KeyValue.Key.Field4, Foo.Field4);
			Assert.assertEquals(KeyValue.Key.Color, Foo.Color);
			Assert.assertTrue(KeyValue.Value.Field1);
			Assert.assertEquals(KeyValueEncodingType.valueOf(KeyValueSchema.SchemaInfo.Properties.get("kv.encoding.type")), KeyValueEncodingType.INLINE);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testKeyValueLatestVersioningEncodeDecode()
		public virtual void TestKeyValueLatestVersioningEncodeDecode()
		{
			AvroSchema<SchemaTestUtils.Foo> FooSchema = AvroSchema.of(SchemaDefinition.builder<SchemaTestUtils.Foo>().withPojo(typeof(SchemaTestUtils.Foo)).build());
			AvroSchema<SchemaTestUtils.Bar> BarSchema = AvroSchema.of(SchemaDefinition.builder<SchemaTestUtils.Bar>().withPojo(typeof(SchemaTestUtils.Bar)).build());
			Schema<KeyValue<SchemaTestUtils.Foo, SchemaTestUtils.Bar>> KeyValueSchema = KeyValueSchema.of(FooSchema, BarSchema, KeyValueEncodingType.SEPARATED);

			SchemaTestUtils.Bar Bar = new SchemaTestUtils.Bar();
			Bar.Field1 = true;

			SchemaTestUtils.Foo Foo = new SchemaTestUtils.Foo();
			Foo.Field1 = "field1";
			Foo.Field2 = "field2";
			Foo.Field3 = 3;
			Foo.Field4 = Bar;
			Foo.Color = SchemaTestUtils.Color.RED;

			sbyte[] EncodeBytes = KeyValueSchema.encode(new KeyValue(Foo, Bar));
			KeyValue<SchemaTestUtils.Foo, SchemaTestUtils.Bar> KeyValue = ((KeyValueSchema)KeyValueSchema).decode(FooSchema.encode(Foo), EncodeBytes, new sbyte[10]);
			Assert.assertTrue(KeyValue.Value.Field1);
			Assert.assertEquals(KeyValueEncodingType.valueOf(KeyValueSchema.SchemaInfo.Properties.get("kv.encoding.type")), KeyValueEncodingType.SEPARATED);
		}
	}

}