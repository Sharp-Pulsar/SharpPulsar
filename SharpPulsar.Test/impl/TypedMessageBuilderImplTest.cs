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
namespace Org.Apache.Pulsar.Client.Impl
{
	using Org.Apache.Pulsar.Client.Api;
	using Org.Apache.Pulsar.Client.Api.Schema;
	using Org.Apache.Pulsar.Client.Impl.Schema;
	using SchemaTestUtils = Org.Apache.Pulsar.Client.Impl.Schema.SchemaTestUtils;
	using Org.Apache.Pulsar.Common.Schema;
	using KeyValueEncodingType = Org.Apache.Pulsar.Common.Schema.KeyValueEncodingType;
	using Mock = org.mockito.Mock;
	using Test = org.testng.annotations.Test;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.testng.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.testng.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.testng.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.testng.Assert.fail;

	/// <summary>
	/// Unit test of <seealso cref="TypedMessageBuilderImpl"/>.
	/// </summary>
	public class TypedMessageBuilderImplTest
	{

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Mock protected ProducerBase producerBase;
		protected internal ProducerBase ProducerBase;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testDefaultValue()
		public virtual void TestDefaultValue()
		{
			ProducerBase = mock(typeof(ProducerBase));

			AvroSchema<SchemaTestUtils.Foo> FooSchema = AvroSchema.of(SchemaDefinition.builder<SchemaTestUtils.Foo>().withPojo(typeof(SchemaTestUtils.Foo)).build());
			AvroSchema<SchemaTestUtils.Bar> BarSchema = AvroSchema.of(SchemaDefinition.builder<SchemaTestUtils.Bar>().withPojo(typeof(SchemaTestUtils.Bar)).build());

			Schema<KeyValue<SchemaTestUtils.Foo, SchemaTestUtils.Bar>> KeyValueSchema = Schema.KeyValue(FooSchema, BarSchema);
			TypedMessageBuilderImpl TypedMessageBuilderImpl = new TypedMessageBuilderImpl(ProducerBase, KeyValueSchema);

			SchemaTestUtils.Foo Foo = new SchemaTestUtils.Foo();
			Foo.Field1 = "field1";
			Foo.Field2 = "field2";
			SchemaTestUtils.Bar Bar = new SchemaTestUtils.Bar();
			Bar.Field1 = true;
			KeyValue<SchemaTestUtils.Foo, SchemaTestUtils.Bar> KeyValue = new KeyValue<SchemaTestUtils.Foo, SchemaTestUtils.Bar>(Foo, Bar);

			// Check kv.encoding.type default, not set value
			TypedMessageBuilderImpl<KeyValue> TypedMessageBuilder = (TypedMessageBuilderImpl)TypedMessageBuilderImpl.value(KeyValue);
			ByteBuffer Content = TypedMessageBuilder.Content;
			sbyte[] ContentByte = new sbyte[Content.remaining()];
			Content.get(ContentByte);
			KeyValue<SchemaTestUtils.Foo, SchemaTestUtils.Bar> DecodeKeyValue = KeyValueSchema.decode(ContentByte);
			assertEquals(DecodeKeyValue.Key, Foo);
			assertEquals(DecodeKeyValue.Value, Bar);
			assertFalse(TypedMessageBuilderImpl.hasKey());
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testInlineValue()
		public virtual void TestInlineValue()
		{
			ProducerBase = mock(typeof(ProducerBase));

			AvroSchema<SchemaTestUtils.Foo> FooSchema = AvroSchema.of(SchemaDefinition.builder<SchemaTestUtils.Foo>().withPojo(typeof(SchemaTestUtils.Foo)).build());
			AvroSchema<SchemaTestUtils.Bar> BarSchema = AvroSchema.of(SchemaDefinition.builder<SchemaTestUtils.Bar>().withPojo(typeof(SchemaTestUtils.Bar)).build());

			Schema<KeyValue<SchemaTestUtils.Foo, SchemaTestUtils.Bar>> KeyValueSchema = Schema.KeyValue(FooSchema, BarSchema, KeyValueEncodingType.INLINE);
			TypedMessageBuilderImpl TypedMessageBuilderImpl = new TypedMessageBuilderImpl(ProducerBase, KeyValueSchema);

			SchemaTestUtils.Foo Foo = new SchemaTestUtils.Foo();
			Foo.Field1 = "field1";
			Foo.Field2 = "field2";
			SchemaTestUtils.Bar Bar = new SchemaTestUtils.Bar();
			Bar.Field1 = true;
			KeyValue<SchemaTestUtils.Foo, SchemaTestUtils.Bar> KeyValue = new KeyValue<SchemaTestUtils.Foo, SchemaTestUtils.Bar>(Foo, Bar);

			// Check kv.encoding.type INLINE
			TypedMessageBuilderImpl<KeyValue> TypedMessageBuilder = (TypedMessageBuilderImpl)TypedMessageBuilderImpl.value(KeyValue);
			ByteBuffer Content = TypedMessageBuilder.Content;
			sbyte[] ContentByte = new sbyte[Content.remaining()];
			Content.get(ContentByte);
			KeyValue<SchemaTestUtils.Foo, SchemaTestUtils.Bar> DecodeKeyValue = KeyValueSchema.decode(ContentByte);
			assertEquals(DecodeKeyValue.Key, Foo);
			assertEquals(DecodeKeyValue.Value, Bar);
			assertFalse(TypedMessageBuilderImpl.hasKey());
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testSeparatedValue()
		public virtual void TestSeparatedValue()
		{
			ProducerBase = mock(typeof(ProducerBase));

			AvroSchema<SchemaTestUtils.Foo> FooSchema = AvroSchema.of(SchemaDefinition.builder<SchemaTestUtils.Foo>().withPojo(typeof(SchemaTestUtils.Foo)).build());
			AvroSchema<SchemaTestUtils.Bar> BarSchema = AvroSchema.of(SchemaDefinition.builder<SchemaTestUtils.Bar>().withPojo(typeof(SchemaTestUtils.Bar)).build());

			Schema<KeyValue<SchemaTestUtils.Foo, SchemaTestUtils.Bar>> KeyValueSchema = Schema.KeyValue(FooSchema, BarSchema, KeyValueEncodingType.SEPARATED);
			TypedMessageBuilderImpl TypedMessageBuilderImpl = new TypedMessageBuilderImpl(ProducerBase, KeyValueSchema);

			SchemaTestUtils.Foo Foo = new SchemaTestUtils.Foo();
			Foo.Field1 = "field1";
			Foo.Field2 = "field2";
			SchemaTestUtils.Bar Bar = new SchemaTestUtils.Bar();
			Bar.Field1 = true;
			KeyValue<SchemaTestUtils.Foo, SchemaTestUtils.Bar> KeyValue = new KeyValue<SchemaTestUtils.Foo, SchemaTestUtils.Bar>(Foo, Bar);

			// Check kv.encoding.type SEPARATED
			TypedMessageBuilderImpl TypedMessageBuilder = (TypedMessageBuilderImpl)TypedMessageBuilderImpl.value(KeyValue);
			ByteBuffer Content = TypedMessageBuilder.Content;
			sbyte[] ContentByte = new sbyte[Content.remaining()];
			Content.get(ContentByte);
			assertTrue(TypedMessageBuilderImpl.hasKey());
			assertEquals(TypedMessageBuilderImpl.Key, Base64.Encoder.encodeToString(FooSchema.encode(Foo)));
			assertEquals(BarSchema.decode(ContentByte), Bar);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testSetKeyEncodingTypeDefault()
		public virtual void TestSetKeyEncodingTypeDefault()
		{
			ProducerBase = mock(typeof(ProducerBase));

			AvroSchema<SchemaTestUtils.Foo> FooSchema = AvroSchema.of(SchemaDefinition.builder<SchemaTestUtils.Foo>().withPojo(typeof(SchemaTestUtils.Foo)).build());
			AvroSchema<SchemaTestUtils.Bar> BarSchema = AvroSchema.of(SchemaDefinition.builder<SchemaTestUtils.Bar>().withPojo(typeof(SchemaTestUtils.Bar)).build());

			Schema<KeyValue<SchemaTestUtils.Foo, SchemaTestUtils.Bar>> KeyValueSchema = Schema.KeyValue(FooSchema, BarSchema);
			TypedMessageBuilderImpl TypedMessageBuilderImpl = new TypedMessageBuilderImpl(ProducerBase, KeyValueSchema);

			TypedMessageBuilderImpl TypedMessageBuilder = (TypedMessageBuilderImpl)TypedMessageBuilderImpl.key("default");
			assertEquals(TypedMessageBuilder.Key, "default");
			assertFalse(TypedMessageBuilder.MetadataBuilder.PartitionKeyB64Encoded);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testSetKeyEncodingTypeInline()
		public virtual void TestSetKeyEncodingTypeInline()
		{
			ProducerBase = mock(typeof(ProducerBase));

			AvroSchema<SchemaTestUtils.Foo> FooSchema = AvroSchema.of(SchemaDefinition.builder<SchemaTestUtils.Foo>().withPojo(typeof(SchemaTestUtils.Foo)).build());
			AvroSchema<SchemaTestUtils.Bar> BarSchema = AvroSchema.of(SchemaDefinition.builder<SchemaTestUtils.Bar>().withPojo(typeof(SchemaTestUtils.Bar)).build());

			Schema<KeyValue<SchemaTestUtils.Foo, SchemaTestUtils.Bar>> KeyValueSchema = Schema.KeyValue(FooSchema, BarSchema, KeyValueEncodingType.INLINE);
			TypedMessageBuilderImpl TypedMessageBuilderImpl = new TypedMessageBuilderImpl(ProducerBase, KeyValueSchema);

			TypedMessageBuilderImpl TypedMessageBuilder = (TypedMessageBuilderImpl)TypedMessageBuilderImpl.key("inline");
			assertEquals(TypedMessageBuilder.Key, "inline");
			assertFalse(TypedMessageBuilder.MetadataBuilder.PartitionKeyB64Encoded);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testSetKeyEncodingTypeSeparated()
		public virtual void TestSetKeyEncodingTypeSeparated()
		{
			ProducerBase = mock(typeof(ProducerBase));

			AvroSchema<SchemaTestUtils.Foo> FooSchema = AvroSchema.of(SchemaDefinition.builder<SchemaTestUtils.Foo>().withPojo(typeof(SchemaTestUtils.Foo)).build());
			AvroSchema<SchemaTestUtils.Bar> BarSchema = AvroSchema.of(SchemaDefinition.builder<SchemaTestUtils.Bar>().withPojo(typeof(SchemaTestUtils.Bar)).build());

			Schema<KeyValue<SchemaTestUtils.Foo, SchemaTestUtils.Bar>> KeyValueSchema = Schema.KeyValue(FooSchema, BarSchema, KeyValueEncodingType.SEPARATED);
			TypedMessageBuilderImpl TypedMessageBuilderImpl = new TypedMessageBuilderImpl(ProducerBase, KeyValueSchema);


			try
			{
				TypedMessageBuilderImpl TypedMessageBuilder = (TypedMessageBuilderImpl)TypedMessageBuilderImpl.key("separated");
				fail("This should fail");
			}
			catch (System.ArgumentException E)
			{
				assertTrue(E.Message.contains("This method is not allowed to set keys when in encoding type is SEPARATED"));
			}
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testSetKeyBytesEncodingTypeDefault()
		public virtual void TestSetKeyBytesEncodingTypeDefault()
		{
			ProducerBase = mock(typeof(ProducerBase));

			AvroSchema<SchemaTestUtils.Foo> FooSchema = AvroSchema.of(SchemaDefinition.builder<SchemaTestUtils.Foo>().withPojo(typeof(SchemaTestUtils.Foo)).build());
			AvroSchema<SchemaTestUtils.Bar> BarSchema = AvroSchema.of(SchemaDefinition.builder<SchemaTestUtils.Bar>().withPojo(typeof(SchemaTestUtils.Bar)).build());

			Schema<KeyValue<SchemaTestUtils.Foo, SchemaTestUtils.Bar>> KeyValueSchema = Schema.KeyValue(FooSchema, BarSchema);
			TypedMessageBuilderImpl TypedMessageBuilderImpl = new TypedMessageBuilderImpl(ProducerBase, KeyValueSchema);

			TypedMessageBuilderImpl TypedMessageBuilder = (TypedMessageBuilderImpl)TypedMessageBuilderImpl.keyBytes("default".GetBytes());
			assertEquals(TypedMessageBuilder.Key, Base64.Encoder.encodeToString("default".GetBytes()));
			assertTrue(TypedMessageBuilder.MetadataBuilder.PartitionKeyB64Encoded);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testSetKeyBytesEncodingTypeInline()
		public virtual void TestSetKeyBytesEncodingTypeInline()
		{
			ProducerBase = mock(typeof(ProducerBase));

			AvroSchema<SchemaTestUtils.Foo> FooSchema = AvroSchema.of(SchemaDefinition.builder<SchemaTestUtils.Foo>().withPojo(typeof(SchemaTestUtils.Foo)).build());
			AvroSchema<SchemaTestUtils.Bar> BarSchema = AvroSchema.of(SchemaDefinition.builder<SchemaTestUtils.Bar>().withPojo(typeof(SchemaTestUtils.Bar)).build());

			Schema<KeyValue<SchemaTestUtils.Foo, SchemaTestUtils.Bar>> KeyValueSchema = Schema.KeyValue(FooSchema, BarSchema, KeyValueEncodingType.INLINE);
			TypedMessageBuilderImpl TypedMessageBuilderImpl = new TypedMessageBuilderImpl(ProducerBase, KeyValueSchema);

			TypedMessageBuilderImpl TypedMessageBuilder = (TypedMessageBuilderImpl)TypedMessageBuilderImpl.keyBytes("inline".GetBytes());
			assertEquals(TypedMessageBuilder.Key, Base64.Encoder.encodeToString("inline".GetBytes()));
			assertTrue(TypedMessageBuilder.MetadataBuilder.PartitionKeyB64Encoded);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testSetKeyBytesEncodingTypeSeparated()
		public virtual void TestSetKeyBytesEncodingTypeSeparated()
		{
			ProducerBase = mock(typeof(ProducerBase));

			AvroSchema<SchemaTestUtils.Foo> FooSchema = AvroSchema.of(SchemaDefinition.builder<SchemaTestUtils.Foo>().withPojo(typeof(SchemaTestUtils.Foo)).build());
			AvroSchema<SchemaTestUtils.Bar> BarSchema = AvroSchema.of(SchemaDefinition.builder<SchemaTestUtils.Bar>().withPojo(typeof(SchemaTestUtils.Bar)).build());

			Schema<KeyValue<SchemaTestUtils.Foo, SchemaTestUtils.Bar>> KeyValueSchema = Schema.KeyValue(FooSchema, BarSchema, KeyValueEncodingType.SEPARATED);
			TypedMessageBuilderImpl TypedMessageBuilderImpl = new TypedMessageBuilderImpl(ProducerBase, KeyValueSchema);


			try
			{
				TypedMessageBuilderImpl TypedMessageBuilder = (TypedMessageBuilderImpl)TypedMessageBuilderImpl.keyBytes("separated".GetBytes());
				fail("This should fail");
			}
			catch (System.ArgumentException E)
			{
				assertTrue(E.Message.contains("This method is not allowed to set keys when in encoding type is SEPARATED"));
			}
		}


	}

}