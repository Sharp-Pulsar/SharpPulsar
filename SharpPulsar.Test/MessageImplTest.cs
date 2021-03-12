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
	using BooleanSchema = Org.Apache.Pulsar.Client.Impl.Schema.BooleanSchema;
	using Org.Apache.Pulsar.Client.Impl.Schema;
	using SchemaTestUtils = Org.Apache.Pulsar.Client.Impl.Schema.SchemaTestUtils;
	using MultiVersionSchemaInfoProvider = Org.Apache.Pulsar.Client.Impl.Schema.Generic.MultiVersionSchemaInfoProvider;
	using MessageMetadata = Org.Apache.Pulsar.Common.Api.Proto.PulsarApi.MessageMetadata;
	using Org.Apache.Pulsar.Common.Schema;
	using KeyValueEncodingType = Org.Apache.Pulsar.Common.Schema.KeyValueEncodingType;
	using ByteString = Org.Apache.Pulsar.shaded.com.google.protobuf.v241.ByteString;
	using Assert = org.testng.Assert;
	using Test = org.testng.annotations.Test;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.powermock.api.mockito.PowerMockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.testng.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.testng.Assert.assertNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.testng.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.testng.Assert.assertTrue;

	/// <summary>
	/// Unit test of <seealso cref="MessageImpl"/>.
	/// </summary>
	public class MessageImplTest
	{

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testGetSequenceIdNotAssociated()
		public virtual void TestGetSequenceIdNotAssociated()
		{
			MessageMetadata.Builder builder = MessageMetadata.NewBuilder();
			ByteBuffer payload = ByteBuffer.wrap(new sbyte[0]);
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in C#:
//ORIGINAL LINE: MessageImpl<?> msg = MessageImpl.create(builder, payload, org.apache.pulsar.client.api.Schema.BYTES);
			MessageImpl<object> msg = MessageImpl.Create(builder, payload, Schema.BYTES);

			assertEquals(-1, msg.SequenceId);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testSetDuplicatePropertiesKey()
		public virtual void TestSetDuplicatePropertiesKey()
		{
			MessageMetadata.Builder builder = MessageMetadata.NewBuilder();
			builder.AddProperties(Org.Apache.Pulsar.Common.Api.Proto.PulsarApi.KeyValue.NewBuilder().setKey("key1").setValue("value1").Build());
			builder.AddProperties(Org.Apache.Pulsar.Common.Api.Proto.PulsarApi.KeyValue.NewBuilder().setKey("key1").setValue("value2").Build());
			builder.AddProperties(Org.Apache.Pulsar.Common.Api.Proto.PulsarApi.KeyValue.NewBuilder().setKey("key3").setValue("value3").Build());
			ByteBuffer payload = ByteBuffer.wrap(new sbyte[0]);
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in C#:
//ORIGINAL LINE: MessageImpl<?> msg = MessageImpl.create(builder, payload, org.apache.pulsar.client.api.Schema.BYTES);
			MessageImpl<object> msg = MessageImpl.Create(builder, payload, Schema.BYTES);
			assertEquals("value2", msg.GetProperty("key1"));
			assertEquals("value3", msg.GetProperty("key3"));
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testGetSequenceIdAssociated()
		public virtual void TestGetSequenceIdAssociated()
		{
			MessageMetadata.Builder builder = MessageMetadata.NewBuilder().setSequenceId(1234);

			ByteBuffer payload = ByteBuffer.wrap(new sbyte[0]);
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in C#:
//ORIGINAL LINE: MessageImpl<?> msg = MessageImpl.create(builder, payload, org.apache.pulsar.client.api.Schema.BYTES);
			MessageImpl<object> msg = MessageImpl.Create(builder, payload, Schema.BYTES);

			assertEquals(1234, msg.SequenceId);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testGetProducerNameNotAssigned()
		public virtual void TestGetProducerNameNotAssigned()
		{
			MessageMetadata.Builder builder = MessageMetadata.NewBuilder();
			ByteBuffer payload = ByteBuffer.wrap(new sbyte[0]);
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in C#:
//ORIGINAL LINE: MessageImpl<?> msg = MessageImpl.create(builder, payload, org.apache.pulsar.client.api.Schema.BYTES);
			MessageImpl<object> msg = MessageImpl.Create(builder, payload, Schema.BYTES);

			assertNull(msg.ProducerName);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testGetProducerNameAssigned()
		public virtual void TestGetProducerNameAssigned()
		{
			MessageMetadata.Builder builder = MessageMetadata.NewBuilder().setProducerName("test-producer");

			ByteBuffer payload = ByteBuffer.wrap(new sbyte[0]);
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in C#:
//ORIGINAL LINE: MessageImpl<?> msg = MessageImpl.create(builder, payload, org.apache.pulsar.client.api.Schema.BYTES);
			MessageImpl<object> msg = MessageImpl.Create(builder, payload, Schema.BYTES);

			assertEquals("test-producer", msg.ProducerName);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testDefaultGetProducerDataAssigned()
		public virtual void TestDefaultGetProducerDataAssigned()
		{
			AvroSchema<SchemaTestUtils.Foo> fooSchema = AvroSchema.of(SchemaDefinition.builder<SchemaTestUtils.Foo>().withPojo(typeof(SchemaTestUtils.Foo)).build());
			AvroSchema<SchemaTestUtils.Bar> barSchema = AvroSchema.of(SchemaDefinition.builder<SchemaTestUtils.Bar>().withPojo(typeof(SchemaTestUtils.Bar)).build());

			Schema<KeyValue<SchemaTestUtils.Foo, SchemaTestUtils.Bar>> keyValueSchema = Schema.KeyValue(fooSchema, barSchema);
			SchemaTestUtils.Foo foo = new SchemaTestUtils.Foo();
			foo.Field1 = "field1";
			foo.Field2 = "field2";
			foo.Field3 = 3;
			SchemaTestUtils.Bar bar = new SchemaTestUtils.Bar();
			bar.Field1 = true;

			// // Check kv.encoding.type default, not set value
			sbyte[] encodeBytes = keyValueSchema.Encode(new KeyValue(foo, bar));
			MessageMetadata.Builder builder = MessageMetadata.NewBuilder().setProducerName("default");
			MessageImpl<KeyValue<SchemaTestUtils.Foo, SchemaTestUtils.Bar>> msg = MessageImpl.Create(builder, ByteBuffer.wrap(encodeBytes), keyValueSchema);
			KeyValue<SchemaTestUtils.Foo, SchemaTestUtils.Bar> keyValue = msg.Value;
			assertEquals(keyValue.Key, foo);
			assertEquals(keyValue.Value, bar);
			assertFalse(builder.HasPartitionKey());
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testInlineGetProducerDataAssigned()
		public virtual void TestInlineGetProducerDataAssigned()
		{

			AvroSchema<SchemaTestUtils.Foo> fooSchema = AvroSchema.of(SchemaDefinition.builder<SchemaTestUtils.Foo>().withPojo(typeof(SchemaTestUtils.Foo)).build());
			AvroSchema<SchemaTestUtils.Bar> barSchema = AvroSchema.of(SchemaDefinition.builder<SchemaTestUtils.Bar>().withPojo(typeof(SchemaTestUtils.Bar)).build());

			Schema<KeyValue<SchemaTestUtils.Foo, SchemaTestUtils.Bar>> keyValueSchema = Schema.KeyValue(fooSchema, barSchema, KeyValueEncodingType.INLINE);
			SchemaTestUtils.Foo foo = new SchemaTestUtils.Foo();
			foo.Field1 = "field1";
			foo.Field2 = "field2";
			foo.Field3 = 3;
			SchemaTestUtils.Bar bar = new SchemaTestUtils.Bar();
			bar.Field1 = true;

			// Check kv.encoding.type INLINE
			sbyte[] encodeBytes = keyValueSchema.Encode(new KeyValue(foo, bar));
			MessageMetadata.Builder builder = MessageMetadata.NewBuilder().setProducerName("inline");
			MessageImpl<KeyValue<SchemaTestUtils.Foo, SchemaTestUtils.Bar>> msg = MessageImpl.Create(builder, ByteBuffer.wrap(encodeBytes), keyValueSchema);
			KeyValue<SchemaTestUtils.Foo, SchemaTestUtils.Bar> keyValue = msg.Value;
			assertEquals(keyValue.Key, foo);
			assertEquals(keyValue.Value, bar);
			assertFalse(builder.HasPartitionKey());
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testSeparatedGetProducerDataAssigned()
		public virtual void TestSeparatedGetProducerDataAssigned()
		{
			AvroSchema<SchemaTestUtils.Foo> fooSchema = AvroSchema.of(SchemaDefinition.builder<SchemaTestUtils.Foo>().withPojo(typeof(SchemaTestUtils.Foo)).build());
			AvroSchema<SchemaTestUtils.Bar> barSchema = AvroSchema.of(SchemaDefinition.builder<SchemaTestUtils.Bar>().withPojo(typeof(SchemaTestUtils.Bar)).build());

			Schema<KeyValue<SchemaTestUtils.Foo, SchemaTestUtils.Bar>> keyValueSchema = Schema.KeyValue(fooSchema, barSchema, KeyValueEncodingType.SEPARATED);
			SchemaTestUtils.Foo foo = new SchemaTestUtils.Foo();
			foo.Field1 = "field1";
			foo.Field2 = "field2";
			foo.Field3 = 3;
			SchemaTestUtils.Bar bar = new SchemaTestUtils.Bar();
			bar.Field1 = true;

			// Check kv.encoding.type SPRAERATE
			sbyte[] encodeBytes = keyValueSchema.Encode(new KeyValue(foo, bar));
			MessageMetadata.Builder builder = MessageMetadata.NewBuilder().setProducerName("separated");
			builder.setPartitionKey(Base64.Encoder.encodeToString(fooSchema.Encode(foo)));
			builder.PartitionKeyB64Encoded = true;
			MessageImpl<KeyValue<SchemaTestUtils.Foo, SchemaTestUtils.Bar>> msg = MessageImpl.Create(builder, ByteBuffer.wrap(encodeBytes), keyValueSchema);
			KeyValue<SchemaTestUtils.Foo, SchemaTestUtils.Bar> keyValue = msg.Value;
			assertEquals(keyValue.Key, foo);
			assertEquals(keyValue.Value, bar);
			assertTrue(builder.HasPartitionKey());
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testDefaultAVROVersionGetProducerDataAssigned()
		public virtual void TestDefaultAVROVersionGetProducerDataAssigned()
		{
			AvroSchema<SchemaTestUtils.Foo> fooSchema = AvroSchema.of(SchemaDefinition.builder<SchemaTestUtils.Foo>().withPojo(typeof(SchemaTestUtils.Foo)).build());
			AvroSchema<SchemaTestUtils.Bar> barSchema = AvroSchema.of(SchemaDefinition.builder<SchemaTestUtils.Bar>().withPojo(typeof(SchemaTestUtils.Bar)).build());

			MultiVersionSchemaInfoProvider multiVersionSchemaInfoProvider = mock(typeof(MultiVersionSchemaInfoProvider));
			Schema<KeyValue<SchemaTestUtils.Foo, SchemaTestUtils.Bar>> keyValueSchema = Schema.KeyValue(fooSchema, barSchema);
			keyValueSchema.SchemaInfoProvider = multiVersionSchemaInfoProvider;
			when(multiVersionSchemaInfoProvider.GetSchemaByVersion(any(typeof(sbyte[])))).thenReturn(CompletableFuture.completedFuture(keyValueSchema.SchemaInfo));

			SchemaTestUtils.Foo foo = new SchemaTestUtils.Foo();
			foo.Field1 = "field1";
			foo.Field2 = "field2";
			foo.Field3 = 3;
			SchemaTestUtils.Bar bar = new SchemaTestUtils.Bar();
			bar.Field1 = true;

			sbyte[] encodeBytes = keyValueSchema.Encode(new KeyValue(foo, bar));
			MessageMetadata.Builder builder = MessageMetadata.NewBuilder().setProducerName("default");
			ByteString byteString = ByteString.copyFrom(new sbyte[10]);
			builder.SchemaVersion = byteString;
			MessageImpl<KeyValue<SchemaTestUtils.Foo, SchemaTestUtils.Bar>> msg = MessageImpl.Create(builder, ByteBuffer.wrap(encodeBytes), keyValueSchema);
			KeyValue<SchemaTestUtils.Foo, SchemaTestUtils.Bar> keyValue = msg.Value;
			assertEquals(keyValue.Key, foo);
			assertEquals(keyValue.Value, bar);
			assertFalse(builder.HasPartitionKey());
			Assert.assertEquals((KeyValueEncodingType)Enum.Parse(typeof(KeyValueEncodingType), keyValueSchema.SchemaInfo.Properties.get("kv.encoding.type")), KeyValueEncodingType.INLINE);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testSeparatedAVROVersionGetProducerDataAssigned()
		public virtual void TestSeparatedAVROVersionGetProducerDataAssigned()
		{
			AvroSchema<SchemaTestUtils.Foo> fooSchema = AvroSchema.of(SchemaDefinition.builder<SchemaTestUtils.Foo>().withPojo(typeof(SchemaTestUtils.Foo)).build());
			AvroSchema<SchemaTestUtils.Bar> barSchema = AvroSchema.of(SchemaDefinition.builder<SchemaTestUtils.Bar>().withPojo(typeof(SchemaTestUtils.Bar)).build());

			MultiVersionSchemaInfoProvider multiVersionSchemaInfoProvider = mock(typeof(MultiVersionSchemaInfoProvider));
			Schema<KeyValue<SchemaTestUtils.Foo, SchemaTestUtils.Bar>> keyValueSchema = Schema.KeyValue(fooSchema, barSchema, KeyValueEncodingType.SEPARATED);
			keyValueSchema.SchemaInfoProvider = multiVersionSchemaInfoProvider;
			when(multiVersionSchemaInfoProvider.GetSchemaByVersion(any(typeof(sbyte[])))).thenReturn(CompletableFuture.completedFuture(keyValueSchema.SchemaInfo));

			SchemaTestUtils.Foo foo = new SchemaTestUtils.Foo();
			foo.Field1 = "field1";
			foo.Field2 = "field2";
			foo.Field3 = 3;
			SchemaTestUtils.Bar bar = new SchemaTestUtils.Bar();
			bar.Field1 = true;

			sbyte[] encodeBytes = keyValueSchema.Encode(new KeyValue(foo, bar));
			MessageMetadata.Builder builder = MessageMetadata.NewBuilder().setProducerName("separated");
			ByteString byteString = ByteString.copyFrom(new sbyte[10]);
			builder.SchemaVersion = byteString;
			builder.setPartitionKey(Base64.Encoder.encodeToString(fooSchema.Encode(foo)));
			builder.PartitionKeyB64Encoded = true;
			MessageImpl<KeyValue<SchemaTestUtils.Foo, SchemaTestUtils.Bar>> msg = MessageImpl.Create(builder, ByteBuffer.wrap(encodeBytes), keyValueSchema);
			KeyValue<SchemaTestUtils.Foo, SchemaTestUtils.Bar> keyValue = msg.Value;
			assertEquals(keyValue.Key, foo);
			assertEquals(keyValue.Value, bar);
			assertTrue(builder.HasPartitionKey());
			Assert.assertEquals((KeyValueEncodingType)Enum.Parse(typeof(KeyValueEncodingType), keyValueSchema.SchemaInfo.Properties.get("kv.encoding.type")), KeyValueEncodingType.SEPARATED);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testDefaultJSONVersionGetProducerDataAssigned()
		public virtual void TestDefaultJSONVersionGetProducerDataAssigned()
		{
			JSONSchema<SchemaTestUtils.Foo> fooSchema = JSONSchema.of(SchemaDefinition.builder<SchemaTestUtils.Foo>().withPojo(typeof(SchemaTestUtils.Foo)).build());
			JSONSchema<SchemaTestUtils.Bar> barSchema = JSONSchema.of(SchemaDefinition.builder<SchemaTestUtils.Bar>().withPojo(typeof(SchemaTestUtils.Bar)).build());

			MultiVersionSchemaInfoProvider multiVersionSchemaInfoProvider = mock(typeof(MultiVersionSchemaInfoProvider));
			Schema<KeyValue<SchemaTestUtils.Foo, SchemaTestUtils.Bar>> keyValueSchema = Schema.KeyValue(fooSchema, barSchema);
			keyValueSchema.SchemaInfoProvider = multiVersionSchemaInfoProvider;
			when(multiVersionSchemaInfoProvider.GetSchemaByVersion(any(typeof(sbyte[])))).thenReturn(CompletableFuture.completedFuture(keyValueSchema.SchemaInfo));

			SchemaTestUtils.Foo foo = new SchemaTestUtils.Foo();
			foo.Field1 = "field1";
			foo.Field2 = "field2";
			foo.Field3 = 3;
			SchemaTestUtils.Bar bar = new SchemaTestUtils.Bar();
			bar.Field1 = true;

			sbyte[] encodeBytes = keyValueSchema.Encode(new KeyValue(foo, bar));
			MessageMetadata.Builder builder = MessageMetadata.NewBuilder().setProducerName("default");
			ByteString byteString = ByteString.copyFrom(new sbyte[10]);
			builder.SchemaVersion = byteString;
			MessageImpl<KeyValue<SchemaTestUtils.Foo, SchemaTestUtils.Bar>> msg = MessageImpl.Create(builder, ByteBuffer.wrap(encodeBytes), keyValueSchema);
			KeyValue<SchemaTestUtils.Foo, SchemaTestUtils.Bar> keyValue = msg.Value;
			assertEquals(keyValue.Key, foo);
			assertEquals(keyValue.Value, bar);
			assertFalse(builder.HasPartitionKey());
			Assert.assertEquals((KeyValueEncodingType)Enum.Parse(typeof(KeyValueEncodingType), keyValueSchema.SchemaInfo.Properties.get("kv.encoding.type")), KeyValueEncodingType.INLINE);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testSeparatedJSONVersionGetProducerDataAssigned()
		public virtual void TestSeparatedJSONVersionGetProducerDataAssigned()
		{
			JSONSchema<SchemaTestUtils.Foo> fooSchema = JSONSchema.of(SchemaDefinition.builder<SchemaTestUtils.Foo>().withPojo(typeof(SchemaTestUtils.Foo)).build());
			JSONSchema<SchemaTestUtils.Bar> barSchema = JSONSchema.of(SchemaDefinition.builder<SchemaTestUtils.Bar>().withPojo(typeof(SchemaTestUtils.Bar)).build());

			MultiVersionSchemaInfoProvider multiVersionSchemaInfoProvider = mock(typeof(MultiVersionSchemaInfoProvider));
			Schema<KeyValue<SchemaTestUtils.Foo, SchemaTestUtils.Bar>> keyValueSchema = Schema.KeyValue(fooSchema, barSchema, KeyValueEncodingType.SEPARATED);
			keyValueSchema.SchemaInfoProvider = multiVersionSchemaInfoProvider;
			when(multiVersionSchemaInfoProvider.GetSchemaByVersion(any(typeof(sbyte[])))).thenReturn(CompletableFuture.completedFuture(keyValueSchema.SchemaInfo));

			SchemaTestUtils.Foo foo = new SchemaTestUtils.Foo();
			foo.Field1 = "field1";
			foo.Field2 = "field2";
			foo.Field3 = 3;
			SchemaTestUtils.Bar bar = new SchemaTestUtils.Bar();
			bar.Field1 = true;

			sbyte[] encodeBytes = keyValueSchema.Encode(new KeyValue(foo, bar));
			MessageMetadata.Builder builder = MessageMetadata.NewBuilder().setProducerName("separated");
			ByteString byteString = ByteString.copyFrom(new sbyte[10]);
			builder.SchemaVersion = byteString;
			builder.setPartitionKey(Base64.Encoder.encodeToString(fooSchema.Encode(foo)));
			builder.PartitionKeyB64Encoded = true;
			MessageImpl<KeyValue<SchemaTestUtils.Foo, SchemaTestUtils.Bar>> msg = MessageImpl.Create(builder, ByteBuffer.wrap(encodeBytes), keyValueSchema);
			KeyValue<SchemaTestUtils.Foo, SchemaTestUtils.Bar> keyValue = msg.Value;
			assertEquals(keyValue.Key, foo);
			assertEquals(keyValue.Value, bar);
			assertTrue(builder.HasPartitionKey());
			Assert.assertEquals((KeyValueEncodingType)Enum.Parse(typeof(KeyValueEncodingType), keyValueSchema.SchemaInfo.Properties.get("kv.encoding.type")), KeyValueEncodingType.SEPARATED);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testDefaultAVROJSONVersionGetProducerDataAssigned()
		public virtual void TestDefaultAVROJSONVersionGetProducerDataAssigned()
		{
			AvroSchema<SchemaTestUtils.Foo> fooSchema = AvroSchema.of(SchemaDefinition.builder<SchemaTestUtils.Foo>().withPojo(typeof(SchemaTestUtils.Foo)).build());
			JSONSchema<SchemaTestUtils.Bar> barSchema = JSONSchema.of(SchemaDefinition.builder<SchemaTestUtils.Bar>().withPojo(typeof(SchemaTestUtils.Bar)).build());

			MultiVersionSchemaInfoProvider multiVersionSchemaInfoProvider = mock(typeof(MultiVersionSchemaInfoProvider));
			Schema<KeyValue<SchemaTestUtils.Foo, SchemaTestUtils.Bar>> keyValueSchema = Schema.KeyValue(fooSchema, barSchema);
			keyValueSchema.SchemaInfoProvider = multiVersionSchemaInfoProvider;
			when(multiVersionSchemaInfoProvider.GetSchemaByVersion(any(typeof(sbyte[])))).thenReturn(CompletableFuture.completedFuture(keyValueSchema.SchemaInfo));

			SchemaTestUtils.Foo foo = new SchemaTestUtils.Foo();
			foo.Field1 = "field1";
			foo.Field2 = "field2";
			foo.Field3 = 3;
			SchemaTestUtils.Bar bar = new SchemaTestUtils.Bar();
			bar.Field1 = true;

			sbyte[] encodeBytes = keyValueSchema.Encode(new KeyValue(foo, bar));
			MessageMetadata.Builder builder = MessageMetadata.NewBuilder().setProducerName("default");
			ByteString byteString = ByteString.copyFrom(new sbyte[10]);
			builder.SchemaVersion = byteString;
			MessageImpl<KeyValue<SchemaTestUtils.Foo, SchemaTestUtils.Bar>> msg = MessageImpl.Create(builder, ByteBuffer.wrap(encodeBytes), keyValueSchema);
			KeyValue<SchemaTestUtils.Foo, SchemaTestUtils.Bar> keyValue = msg.Value;
			assertEquals(keyValue.Key, foo);
			assertEquals(keyValue.Value, bar);
			assertFalse(builder.HasPartitionKey());
			Assert.assertEquals((KeyValueEncodingType)Enum.Parse(typeof(KeyValueEncodingType), keyValueSchema.SchemaInfo.Properties.get("kv.encoding.type")), KeyValueEncodingType.INLINE);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testSeparatedAVROJSONVersionGetProducerDataAssigned()
		public virtual void TestSeparatedAVROJSONVersionGetProducerDataAssigned()
		{
			AvroSchema<SchemaTestUtils.Foo> fooSchema = AvroSchema.of(SchemaDefinition.builder<SchemaTestUtils.Foo>().withPojo(typeof(SchemaTestUtils.Foo)).build());
			JSONSchema<SchemaTestUtils.Bar> barSchema = JSONSchema.of(SchemaDefinition.builder<SchemaTestUtils.Bar>().withPojo(typeof(SchemaTestUtils.Bar)).build());

			MultiVersionSchemaInfoProvider multiVersionSchemaInfoProvider = mock(typeof(MultiVersionSchemaInfoProvider));
			Schema<KeyValue<SchemaTestUtils.Foo, SchemaTestUtils.Bar>> keyValueSchema = Schema.KeyValue(fooSchema, barSchema, KeyValueEncodingType.SEPARATED);
			keyValueSchema.SchemaInfoProvider = multiVersionSchemaInfoProvider;
			when(multiVersionSchemaInfoProvider.GetSchemaByVersion(any(typeof(sbyte[])))).thenReturn(CompletableFuture.completedFuture(keyValueSchema.SchemaInfo));

			SchemaTestUtils.Foo foo = new SchemaTestUtils.Foo();
			foo.Field1 = "field1";
			foo.Field2 = "field2";
			foo.Field3 = 3;
			SchemaTestUtils.Bar bar = new SchemaTestUtils.Bar();
			bar.Field1 = true;

			sbyte[] encodeBytes = keyValueSchema.Encode(new KeyValue(foo, bar));
			MessageMetadata.Builder builder = MessageMetadata.NewBuilder().setProducerName("separated");
			ByteString byteString = ByteString.copyFrom(new sbyte[10]);
			builder.SchemaVersion = byteString;
			builder.setPartitionKey(Base64.Encoder.encodeToString(fooSchema.Encode(foo)));
			builder.PartitionKeyB64Encoded = true;
			MessageImpl<KeyValue<SchemaTestUtils.Foo, SchemaTestUtils.Bar>> msg = MessageImpl.Create(builder, ByteBuffer.wrap(encodeBytes), keyValueSchema);
			KeyValue<SchemaTestUtils.Foo, SchemaTestUtils.Bar> keyValue = msg.Value;
			assertEquals(keyValue.Key, foo);
			assertEquals(keyValue.Value, bar);
			assertTrue(builder.HasPartitionKey());
			Assert.assertEquals((KeyValueEncodingType)Enum.Parse(typeof(KeyValueEncodingType), keyValueSchema.SchemaInfo.Properties.get("kv.encoding.type")), KeyValueEncodingType.SEPARATED);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testTypedSchemaGetNullValue()
		public virtual void TestTypedSchemaGetNullValue()
		{
			sbyte[] encodeBytes = new sbyte[0];
			MessageMetadata.Builder builder = MessageMetadata.NewBuilder().setProducerName("valueNotSet");
			ByteString byteString = ByteString.copyFrom(new sbyte[0]);
			builder.SchemaVersion = byteString;
			builder.setPartitionKey(Base64.Encoder.encodeToString(encodeBytes));
			builder.PartitionKeyB64Encoded = true;
			builder.NullValue = true;
			MessageImpl<bool> msg = MessageImpl.Create(builder, ByteBuffer.wrap(encodeBytes), BooleanSchema.Of());
			assertNull(msg.Value);
		}
	}

}