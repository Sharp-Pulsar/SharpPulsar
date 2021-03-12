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

			AvroSchema<SchemaTestUtils.Foo> fooSchema = AvroSchema.of(SchemaDefinition.builder<SchemaTestUtils.Foo>().withPojo(typeof(SchemaTestUtils.Foo)).build());
			AvroSchema<SchemaTestUtils.Bar> barSchema = AvroSchema.of(SchemaDefinition.builder<SchemaTestUtils.Bar>().withPojo(typeof(SchemaTestUtils.Bar)).build());

			Schema<KeyValue<SchemaTestUtils.Foo, SchemaTestUtils.Bar>> keyValueSchema = Schema.KeyValue(fooSchema, barSchema);
			TypedMessageBuilderImpl typedMessageBuilderImpl = new TypedMessageBuilderImpl(ProducerBase, keyValueSchema);

			SchemaTestUtils.Foo foo = new SchemaTestUtils.Foo();
			foo.Field1 = "field1";
			foo.Field2 = "field2";
			SchemaTestUtils.Bar bar = new SchemaTestUtils.Bar();
			bar.Field1 = true;
			KeyValue<SchemaTestUtils.Foo, SchemaTestUtils.Bar> keyValue = new KeyValue<SchemaTestUtils.Foo, SchemaTestUtils.Bar>(foo, bar);

			// Check kv.encoding.type default, not set value
			TypedMessageBuilderImpl<KeyValue> typedMessageBuilder = (TypedMessageBuilderImpl)typedMessageBuilderImpl.value(keyValue);
			ByteBuffer content = typedMessageBuilder.Content;
			sbyte[] contentByte = new sbyte[content.remaining()];
			content.get(contentByte);
			KeyValue<SchemaTestUtils.Foo, SchemaTestUtils.Bar> decodeKeyValue = keyValueSchema.Decode(contentByte);
			assertEquals(decodeKeyValue.Key, foo);
			assertEquals(decodeKeyValue.Value, bar);
			assertFalse(typedMessageBuilderImpl.hasKey());
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testInlineValue()
		public virtual void TestInlineValue()
		{
			ProducerBase = mock(typeof(ProducerBase));

			AvroSchema<SchemaTestUtils.Foo> fooSchema = AvroSchema.of(SchemaDefinition.builder<SchemaTestUtils.Foo>().withPojo(typeof(SchemaTestUtils.Foo)).build());
			AvroSchema<SchemaTestUtils.Bar> barSchema = AvroSchema.of(SchemaDefinition.builder<SchemaTestUtils.Bar>().withPojo(typeof(SchemaTestUtils.Bar)).build());

			Schema<KeyValue<SchemaTestUtils.Foo, SchemaTestUtils.Bar>> keyValueSchema = Schema.KeyValue(fooSchema, barSchema, KeyValueEncodingType.INLINE);
			TypedMessageBuilderImpl typedMessageBuilderImpl = new TypedMessageBuilderImpl(ProducerBase, keyValueSchema);

			SchemaTestUtils.Foo foo = new SchemaTestUtils.Foo();
			foo.Field1 = "field1";
			foo.Field2 = "field2";
			SchemaTestUtils.Bar bar = new SchemaTestUtils.Bar();
			bar.Field1 = true;
			KeyValue<SchemaTestUtils.Foo, SchemaTestUtils.Bar> keyValue = new KeyValue<SchemaTestUtils.Foo, SchemaTestUtils.Bar>(foo, bar);

			// Check kv.encoding.type INLINE
			TypedMessageBuilderImpl<KeyValue> typedMessageBuilder = (TypedMessageBuilderImpl)typedMessageBuilderImpl.value(keyValue);
			ByteBuffer content = typedMessageBuilder.Content;
			sbyte[] contentByte = new sbyte[content.remaining()];
			content.get(contentByte);
			KeyValue<SchemaTestUtils.Foo, SchemaTestUtils.Bar> decodeKeyValue = keyValueSchema.Decode(contentByte);
			assertEquals(decodeKeyValue.Key, foo);
			assertEquals(decodeKeyValue.Value, bar);
			assertFalse(typedMessageBuilderImpl.hasKey());
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testSeparatedValue()
		public virtual void TestSeparatedValue()
		{
			ProducerBase = mock(typeof(ProducerBase));

			AvroSchema<SchemaTestUtils.Foo> fooSchema = AvroSchema.of(SchemaDefinition.builder<SchemaTestUtils.Foo>().withPojo(typeof(SchemaTestUtils.Foo)).build());
			AvroSchema<SchemaTestUtils.Bar> barSchema = AvroSchema.of(SchemaDefinition.builder<SchemaTestUtils.Bar>().withPojo(typeof(SchemaTestUtils.Bar)).build());

			Schema<KeyValue<SchemaTestUtils.Foo, SchemaTestUtils.Bar>> keyValueSchema = Schema.KeyValue(fooSchema, barSchema, KeyValueEncodingType.SEPARATED);
			TypedMessageBuilderImpl typedMessageBuilderImpl = new TypedMessageBuilderImpl(ProducerBase, keyValueSchema);

			SchemaTestUtils.Foo foo = new SchemaTestUtils.Foo();
			foo.Field1 = "field1";
			foo.Field2 = "field2";
			SchemaTestUtils.Bar bar = new SchemaTestUtils.Bar();
			bar.Field1 = true;
			KeyValue<SchemaTestUtils.Foo, SchemaTestUtils.Bar> keyValue = new KeyValue<SchemaTestUtils.Foo, SchemaTestUtils.Bar>(foo, bar);

			// Check kv.encoding.type SEPARATED
			TypedMessageBuilderImpl typedMessageBuilder = (TypedMessageBuilderImpl)typedMessageBuilderImpl.value(keyValue);
			ByteBuffer content = typedMessageBuilder.Content;
			sbyte[] contentByte = new sbyte[content.remaining()];
			content.get(contentByte);
			assertTrue(typedMessageBuilderImpl.hasKey());
			assertEquals(typedMessageBuilderImpl.Key, Base64.Encoder.encodeToString(fooSchema.Encode(foo)));
			assertEquals(barSchema.Decode(contentByte), bar);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testSetKeyEncodingTypeDefault()
		public virtual void TestSetKeyEncodingTypeDefault()
		{
			ProducerBase = mock(typeof(ProducerBase));

			AvroSchema<SchemaTestUtils.Foo> fooSchema = AvroSchema.of(SchemaDefinition.builder<SchemaTestUtils.Foo>().withPojo(typeof(SchemaTestUtils.Foo)).build());
			AvroSchema<SchemaTestUtils.Bar> barSchema = AvroSchema.of(SchemaDefinition.builder<SchemaTestUtils.Bar>().withPojo(typeof(SchemaTestUtils.Bar)).build());

			Schema<KeyValue<SchemaTestUtils.Foo, SchemaTestUtils.Bar>> keyValueSchema = Schema.KeyValue(fooSchema, barSchema);
			TypedMessageBuilderImpl typedMessageBuilderImpl = new TypedMessageBuilderImpl(ProducerBase, keyValueSchema);

			TypedMessageBuilderImpl typedMessageBuilder = (TypedMessageBuilderImpl)typedMessageBuilderImpl.key("default");
			assertEquals(typedMessageBuilder.Key, "default");
			assertFalse(typedMessageBuilder.MetadataBuilder.PartitionKeyB64Encoded);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testSetKeyEncodingTypeInline()
		public virtual void TestSetKeyEncodingTypeInline()
		{
			ProducerBase = mock(typeof(ProducerBase));

			AvroSchema<SchemaTestUtils.Foo> fooSchema = AvroSchema.of(SchemaDefinition.builder<SchemaTestUtils.Foo>().withPojo(typeof(SchemaTestUtils.Foo)).build());
			AvroSchema<SchemaTestUtils.Bar> barSchema = AvroSchema.of(SchemaDefinition.builder<SchemaTestUtils.Bar>().withPojo(typeof(SchemaTestUtils.Bar)).build());

			Schema<KeyValue<SchemaTestUtils.Foo, SchemaTestUtils.Bar>> keyValueSchema = Schema.KeyValue(fooSchema, barSchema, KeyValueEncodingType.INLINE);
			TypedMessageBuilderImpl typedMessageBuilderImpl = new TypedMessageBuilderImpl(ProducerBase, keyValueSchema);

			TypedMessageBuilderImpl typedMessageBuilder = (TypedMessageBuilderImpl)typedMessageBuilderImpl.key("inline");
			assertEquals(typedMessageBuilder.Key, "inline");
			assertFalse(typedMessageBuilder.MetadataBuilder.PartitionKeyB64Encoded);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testSetKeyEncodingTypeSeparated()
		public virtual void TestSetKeyEncodingTypeSeparated()
		{
			ProducerBase = mock(typeof(ProducerBase));

			AvroSchema<SchemaTestUtils.Foo> fooSchema = AvroSchema.of(SchemaDefinition.builder<SchemaTestUtils.Foo>().withPojo(typeof(SchemaTestUtils.Foo)).build());
			AvroSchema<SchemaTestUtils.Bar> barSchema = AvroSchema.of(SchemaDefinition.builder<SchemaTestUtils.Bar>().withPojo(typeof(SchemaTestUtils.Bar)).build());

			Schema<KeyValue<SchemaTestUtils.Foo, SchemaTestUtils.Bar>> keyValueSchema = Schema.KeyValue(fooSchema, barSchema, KeyValueEncodingType.SEPARATED);
			TypedMessageBuilderImpl typedMessageBuilderImpl = new TypedMessageBuilderImpl(ProducerBase, keyValueSchema);


			try
			{
				TypedMessageBuilderImpl typedMessageBuilder = (TypedMessageBuilderImpl)typedMessageBuilderImpl.key("separated");
				fail("This should fail");
			}
			catch(System.ArgumentException e)
			{
				assertTrue(e.Message.contains("This method is not allowed to set keys when in encoding type is SEPARATED"));
			}
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testSetKeyBytesEncodingTypeDefault()
		public virtual void TestSetKeyBytesEncodingTypeDefault()
		{
			ProducerBase = mock(typeof(ProducerBase));

			AvroSchema<SchemaTestUtils.Foo> fooSchema = AvroSchema.of(SchemaDefinition.builder<SchemaTestUtils.Foo>().withPojo(typeof(SchemaTestUtils.Foo)).build());
			AvroSchema<SchemaTestUtils.Bar> barSchema = AvroSchema.of(SchemaDefinition.builder<SchemaTestUtils.Bar>().withPojo(typeof(SchemaTestUtils.Bar)).build());

			Schema<KeyValue<SchemaTestUtils.Foo, SchemaTestUtils.Bar>> keyValueSchema = Schema.KeyValue(fooSchema, barSchema);
			TypedMessageBuilderImpl typedMessageBuilderImpl = new TypedMessageBuilderImpl(ProducerBase, keyValueSchema);

			TypedMessageBuilderImpl typedMessageBuilder = (TypedMessageBuilderImpl)typedMessageBuilderImpl.keyBytes("default".GetBytes());
			assertEquals(typedMessageBuilder.Key, Base64.Encoder.encodeToString("default".GetBytes()));
			assertTrue(typedMessageBuilder.MetadataBuilder.PartitionKeyB64Encoded);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testSetKeyBytesEncodingTypeInline()
		public virtual void TestSetKeyBytesEncodingTypeInline()
		{
			ProducerBase = mock(typeof(ProducerBase));

			AvroSchema<SchemaTestUtils.Foo> fooSchema = AvroSchema.of(SchemaDefinition.builder<SchemaTestUtils.Foo>().withPojo(typeof(SchemaTestUtils.Foo)).build());
			AvroSchema<SchemaTestUtils.Bar> barSchema = AvroSchema.of(SchemaDefinition.builder<SchemaTestUtils.Bar>().withPojo(typeof(SchemaTestUtils.Bar)).build());

			Schema<KeyValue<SchemaTestUtils.Foo, SchemaTestUtils.Bar>> keyValueSchema = Schema.KeyValue(fooSchema, barSchema, KeyValueEncodingType.INLINE);
			TypedMessageBuilderImpl typedMessageBuilderImpl = new TypedMessageBuilderImpl(ProducerBase, keyValueSchema);

			TypedMessageBuilderImpl typedMessageBuilder = (TypedMessageBuilderImpl)typedMessageBuilderImpl.keyBytes("inline".GetBytes());
			assertEquals(typedMessageBuilder.Key, Base64.Encoder.encodeToString("inline".GetBytes()));
			assertTrue(typedMessageBuilder.MetadataBuilder.PartitionKeyB64Encoded);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testSetKeyBytesEncodingTypeSeparated()
		public virtual void TestSetKeyBytesEncodingTypeSeparated()
		{
			ProducerBase = mock(typeof(ProducerBase));

			AvroSchema<SchemaTestUtils.Foo> fooSchema = AvroSchema.of(SchemaDefinition.builder<SchemaTestUtils.Foo>().withPojo(typeof(SchemaTestUtils.Foo)).build());
			AvroSchema<SchemaTestUtils.Bar> barSchema = AvroSchema.of(SchemaDefinition.builder<SchemaTestUtils.Bar>().withPojo(typeof(SchemaTestUtils.Bar)).build());

			Schema<KeyValue<SchemaTestUtils.Foo, SchemaTestUtils.Bar>> keyValueSchema = Schema.KeyValue(fooSchema, barSchema, KeyValueEncodingType.SEPARATED);
			TypedMessageBuilderImpl typedMessageBuilderImpl = new TypedMessageBuilderImpl(ProducerBase, keyValueSchema);


			try
			{
				TypedMessageBuilderImpl typedMessageBuilder = (TypedMessageBuilderImpl)typedMessageBuilderImpl.keyBytes("separated".GetBytes());
				fail("This should fail");
			}
			catch(System.ArgumentException e)
			{
				assertTrue(e.Message.contains("This method is not allowed to set keys when in encoding type is SEPARATED"));
			}
		}


	}

}