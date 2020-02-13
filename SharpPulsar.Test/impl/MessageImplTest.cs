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
			MessageMetadata.Builder Builder = MessageMetadata.newBuilder();
			ByteBuffer Payload = ByteBuffer.wrap(new sbyte[0]);
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: MessageImpl<?> msg = MessageImpl.create(builder, payload, org.apache.pulsar.client.api.Schema_Fields.BYTES);
			MessageImpl<object> Msg = MessageImpl.Create(Builder, Payload, SchemaFields.BYTES);

			assertEquals(-1, Msg.SequenceId);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testGetSequenceIdAssociated()
		public virtual void TestGetSequenceIdAssociated()
		{
			MessageMetadata.Builder Builder = MessageMetadata.newBuilder().setSequenceId(1234);

			ByteBuffer Payload = ByteBuffer.wrap(new sbyte[0]);
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: MessageImpl<?> msg = MessageImpl.create(builder, payload, org.apache.pulsar.client.api.Schema_Fields.BYTES);
			MessageImpl<object> Msg = MessageImpl.Create(Builder, Payload, SchemaFields.BYTES);

			assertEquals(1234, Msg.SequenceId);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testGetProducerNameNotAssigned()
		public virtual void TestGetProducerNameNotAssigned()
		{
			MessageMetadata.Builder Builder = MessageMetadata.newBuilder();
			ByteBuffer Payload = ByteBuffer.wrap(new sbyte[0]);
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: MessageImpl<?> msg = MessageImpl.create(builder, payload, org.apache.pulsar.client.api.Schema_Fields.BYTES);
			MessageImpl<object> Msg = MessageImpl.Create(Builder, Payload, SchemaFields.BYTES);

			assertNull(Msg.ProducerName);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testGetProducerNameAssigned()
		public virtual void TestGetProducerNameAssigned()
		{
			MessageMetadata.Builder Builder = MessageMetadata.newBuilder().setProducerName("test-producer");

			ByteBuffer Payload = ByteBuffer.wrap(new sbyte[0]);
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: MessageImpl<?> msg = MessageImpl.create(builder, payload, org.apache.pulsar.client.api.Schema_Fields.BYTES);
			MessageImpl<object> Msg = MessageImpl.Create(Builder, Payload, SchemaFields.BYTES);

			assertEquals("test-producer", Msg.ProducerName);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testDefaultGetProducerDataAssigned()
		public virtual void TestDefaultGetProducerDataAssigned()
		{
			AvroSchema<SchemaTestUtils.Foo> FooSchema = AvroSchema.of(SchemaDefinition.builder<SchemaTestUtils.Foo>().withPojo(typeof(SchemaTestUtils.Foo)).build());
			AvroSchema<SchemaTestUtils.Bar> BarSchema = AvroSchema.of(SchemaDefinition.builder<SchemaTestUtils.Bar>().withPojo(typeof(SchemaTestUtils.Bar)).build());

			Schema<KeyValue<SchemaTestUtils.Foo, SchemaTestUtils.Bar>> KeyValueSchema = Schema.KeyValue(FooSchema, BarSchema);
			SchemaTestUtils.Foo Foo = new SchemaTestUtils.Foo();
			Foo.Field1 = "field1";
			Foo.Field2 = "field2";
			Foo.Field3 = 3;
			SchemaTestUtils.Bar Bar = new SchemaTestUtils.Bar();
			Bar.Field1 = true;

			// // Check kv.encoding.type default, not set value
			sbyte[] EncodeBytes = KeyValueSchema.encode(new KeyValue(Foo, Bar));
			MessageMetadata.Builder Builder = MessageMetadata.newBuilder().setProducerName("default");
			MessageImpl<KeyValue<SchemaTestUtils.Foo, SchemaTestUtils.Bar>> Msg = MessageImpl.Create(Builder, ByteBuffer.wrap(EncodeBytes), KeyValueSchema);
			KeyValue<SchemaTestUtils.Foo, SchemaTestUtils.Bar> KeyValue = Msg.Value;
			assertEquals(KeyValue.Key, Foo);
			assertEquals(KeyValue.Value, Bar);
			assertFalse(Builder.hasPartitionKey());
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testInlineGetProducerDataAssigned()
		public virtual void TestInlineGetProducerDataAssigned()
		{

			AvroSchema<SchemaTestUtils.Foo> FooSchema = AvroSchema.of(SchemaDefinition.builder<SchemaTestUtils.Foo>().withPojo(typeof(SchemaTestUtils.Foo)).build());
			AvroSchema<SchemaTestUtils.Bar> BarSchema = AvroSchema.of(SchemaDefinition.builder<SchemaTestUtils.Bar>().withPojo(typeof(SchemaTestUtils.Bar)).build());

			Schema<KeyValue<SchemaTestUtils.Foo, SchemaTestUtils.Bar>> KeyValueSchema = Schema.KeyValue(FooSchema, BarSchema, KeyValueEncodingType.INLINE);
			SchemaTestUtils.Foo Foo = new SchemaTestUtils.Foo();
			Foo.Field1 = "field1";
			Foo.Field2 = "field2";
			Foo.Field3 = 3;
			SchemaTestUtils.Bar Bar = new SchemaTestUtils.Bar();
			Bar.Field1 = true;

			// Check kv.encoding.type INLINE
			sbyte[] EncodeBytes = KeyValueSchema.encode(new KeyValue(Foo, Bar));
			MessageMetadata.Builder Builder = MessageMetadata.newBuilder().setProducerName("inline");
			MessageImpl<KeyValue<SchemaTestUtils.Foo, SchemaTestUtils.Bar>> Msg = MessageImpl.Create(Builder, ByteBuffer.wrap(EncodeBytes), KeyValueSchema);
			KeyValue<SchemaTestUtils.Foo, SchemaTestUtils.Bar> KeyValue = Msg.Value;
			assertEquals(KeyValue.Key, Foo);
			assertEquals(KeyValue.Value, Bar);
			assertFalse(Builder.hasPartitionKey());
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testSeparatedGetProducerDataAssigned()
		public virtual void TestSeparatedGetProducerDataAssigned()
		{
			AvroSchema<SchemaTestUtils.Foo> FooSchema = AvroSchema.of(SchemaDefinition.builder<SchemaTestUtils.Foo>().withPojo(typeof(SchemaTestUtils.Foo)).build());
			AvroSchema<SchemaTestUtils.Bar> BarSchema = AvroSchema.of(SchemaDefinition.builder<SchemaTestUtils.Bar>().withPojo(typeof(SchemaTestUtils.Bar)).build());

			Schema<KeyValue<SchemaTestUtils.Foo, SchemaTestUtils.Bar>> KeyValueSchema = Schema.KeyValue(FooSchema, BarSchema, KeyValueEncodingType.SEPARATED);
			SchemaTestUtils.Foo Foo = new SchemaTestUtils.Foo();
			Foo.Field1 = "field1";
			Foo.Field2 = "field2";
			Foo.Field3 = 3;
			SchemaTestUtils.Bar Bar = new SchemaTestUtils.Bar();
			Bar.Field1 = true;

			// Check kv.encoding.type SPRAERATE
			sbyte[] EncodeBytes = KeyValueSchema.encode(new KeyValue(Foo, Bar));
			MessageMetadata.Builder Builder = MessageMetadata.newBuilder().setProducerName("separated");
			Builder.setPartitionKey(Base64.Encoder.encodeToString(FooSchema.encode(Foo)));
			Builder.PartitionKeyB64Encoded = true;
			MessageImpl<KeyValue<SchemaTestUtils.Foo, SchemaTestUtils.Bar>> Msg = MessageImpl.Create(Builder, ByteBuffer.wrap(EncodeBytes), KeyValueSchema);
			KeyValue<SchemaTestUtils.Foo, SchemaTestUtils.Bar> KeyValue = Msg.Value;
			assertEquals(KeyValue.Key, Foo);
			assertEquals(KeyValue.Value, Bar);
			assertTrue(Builder.hasPartitionKey());
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testDefaultAVROVersionGetProducerDataAssigned()
		public virtual void TestDefaultAVROVersionGetProducerDataAssigned()
		{
			AvroSchema<SchemaTestUtils.Foo> FooSchema = AvroSchema.of(SchemaDefinition.builder<SchemaTestUtils.Foo>().withPojo(typeof(SchemaTestUtils.Foo)).build());
			AvroSchema<SchemaTestUtils.Bar> BarSchema = AvroSchema.of(SchemaDefinition.builder<SchemaTestUtils.Bar>().withPojo(typeof(SchemaTestUtils.Bar)).build());

			MultiVersionSchemaInfoProvider MultiVersionSchemaInfoProvider = mock(typeof(MultiVersionSchemaInfoProvider));
			Schema<KeyValue<SchemaTestUtils.Foo, SchemaTestUtils.Bar>> KeyValueSchema = Schema.KeyValue(FooSchema, BarSchema);
			KeyValueSchema.SchemaInfoProvider = MultiVersionSchemaInfoProvider;
			when(MultiVersionSchemaInfoProvider.getSchemaByVersion(any(typeof(sbyte[])))).thenReturn(CompletableFuture.completedFuture(KeyValueSchema.SchemaInfo));

			SchemaTestUtils.Foo Foo = new SchemaTestUtils.Foo();
			Foo.Field1 = "field1";
			Foo.Field2 = "field2";
			Foo.Field3 = 3;
			SchemaTestUtils.Bar Bar = new SchemaTestUtils.Bar();
			Bar.Field1 = true;

			sbyte[] EncodeBytes = KeyValueSchema.encode(new KeyValue(Foo, Bar));
			MessageMetadata.Builder Builder = MessageMetadata.newBuilder().setProducerName("default");
			ByteString ByteString = ByteString.copyFrom(new sbyte[10]);
			Builder.SchemaVersion = ByteString;
			MessageImpl<KeyValue<SchemaTestUtils.Foo, SchemaTestUtils.Bar>> Msg = MessageImpl.Create(Builder, ByteBuffer.wrap(EncodeBytes), KeyValueSchema);
			KeyValue<SchemaTestUtils.Foo, SchemaTestUtils.Bar> KeyValue = Msg.Value;
			assertEquals(KeyValue.Key, Foo);
			assertEquals(KeyValue.Value, Bar);
			assertFalse(Builder.hasPartitionKey());
			Assert.assertEquals(Enum.Parse(typeof(KeyValueEncodingType), KeyValueSchema.SchemaInfo.Properties.get("kv.encoding.type")), KeyValueEncodingType.INLINE);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testSeparatedAVROVersionGetProducerDataAssigned()
		public virtual void TestSeparatedAVROVersionGetProducerDataAssigned()
		{
			AvroSchema<SchemaTestUtils.Foo> FooSchema = AvroSchema.of(SchemaDefinition.builder<SchemaTestUtils.Foo>().withPojo(typeof(SchemaTestUtils.Foo)).build());
			AvroSchema<SchemaTestUtils.Bar> BarSchema = AvroSchema.of(SchemaDefinition.builder<SchemaTestUtils.Bar>().withPojo(typeof(SchemaTestUtils.Bar)).build());

			MultiVersionSchemaInfoProvider MultiVersionSchemaInfoProvider = mock(typeof(MultiVersionSchemaInfoProvider));
			Schema<KeyValue<SchemaTestUtils.Foo, SchemaTestUtils.Bar>> KeyValueSchema = Schema.KeyValue(FooSchema, BarSchema, KeyValueEncodingType.SEPARATED);
			KeyValueSchema.SchemaInfoProvider = MultiVersionSchemaInfoProvider;
			when(MultiVersionSchemaInfoProvider.getSchemaByVersion(any(typeof(sbyte[])))).thenReturn(CompletableFuture.completedFuture(KeyValueSchema.SchemaInfo));

			SchemaTestUtils.Foo Foo = new SchemaTestUtils.Foo();
			Foo.Field1 = "field1";
			Foo.Field2 = "field2";
			Foo.Field3 = 3;
			SchemaTestUtils.Bar Bar = new SchemaTestUtils.Bar();
			Bar.Field1 = true;

			sbyte[] EncodeBytes = KeyValueSchema.encode(new KeyValue(Foo, Bar));
			MessageMetadata.Builder Builder = MessageMetadata.newBuilder().setProducerName("separated");
			ByteString ByteString = ByteString.copyFrom(new sbyte[10]);
			Builder.SchemaVersion = ByteString;
			Builder.setPartitionKey(Base64.Encoder.encodeToString(FooSchema.encode(Foo)));
			Builder.PartitionKeyB64Encoded = true;
			MessageImpl<KeyValue<SchemaTestUtils.Foo, SchemaTestUtils.Bar>> Msg = MessageImpl.Create(Builder, ByteBuffer.wrap(EncodeBytes), KeyValueSchema);
			KeyValue<SchemaTestUtils.Foo, SchemaTestUtils.Bar> KeyValue = Msg.Value;
			assertEquals(KeyValue.Key, Foo);
			assertEquals(KeyValue.Value, Bar);
			assertTrue(Builder.hasPartitionKey());
			Assert.assertEquals(Enum.Parse(typeof(KeyValueEncodingType), KeyValueSchema.SchemaInfo.Properties.get("kv.encoding.type")), KeyValueEncodingType.SEPARATED);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testDefaultJSONVersionGetProducerDataAssigned()
		public virtual void TestDefaultJSONVersionGetProducerDataAssigned()
		{
			JSONSchema<SchemaTestUtils.Foo> FooSchema = JSONSchema.of(SchemaDefinition.builder<SchemaTestUtils.Foo>().withPojo(typeof(SchemaTestUtils.Foo)).build());
			JSONSchema<SchemaTestUtils.Bar> BarSchema = JSONSchema.of(SchemaDefinition.builder<SchemaTestUtils.Bar>().withPojo(typeof(SchemaTestUtils.Bar)).build());

			MultiVersionSchemaInfoProvider MultiVersionSchemaInfoProvider = mock(typeof(MultiVersionSchemaInfoProvider));
			Schema<KeyValue<SchemaTestUtils.Foo, SchemaTestUtils.Bar>> KeyValueSchema = Schema.KeyValue(FooSchema, BarSchema);
			KeyValueSchema.SchemaInfoProvider = MultiVersionSchemaInfoProvider;
			when(MultiVersionSchemaInfoProvider.getSchemaByVersion(any(typeof(sbyte[])))).thenReturn(CompletableFuture.completedFuture(KeyValueSchema.SchemaInfo));

			SchemaTestUtils.Foo Foo = new SchemaTestUtils.Foo();
			Foo.Field1 = "field1";
			Foo.Field2 = "field2";
			Foo.Field3 = 3;
			SchemaTestUtils.Bar Bar = new SchemaTestUtils.Bar();
			Bar.Field1 = true;

			sbyte[] EncodeBytes = KeyValueSchema.encode(new KeyValue(Foo, Bar));
			MessageMetadata.Builder Builder = MessageMetadata.newBuilder().setProducerName("default");
			ByteString ByteString = ByteString.copyFrom(new sbyte[10]);
			Builder.SchemaVersion = ByteString;
			MessageImpl<KeyValue<SchemaTestUtils.Foo, SchemaTestUtils.Bar>> Msg = MessageImpl.Create(Builder, ByteBuffer.wrap(EncodeBytes), KeyValueSchema);
			KeyValue<SchemaTestUtils.Foo, SchemaTestUtils.Bar> KeyValue = Msg.Value;
			assertEquals(KeyValue.Key, Foo);
			assertEquals(KeyValue.Value, Bar);
			assertFalse(Builder.hasPartitionKey());
			Assert.assertEquals(Enum.Parse(typeof(KeyValueEncodingType), KeyValueSchema.SchemaInfo.Properties.get("kv.encoding.type")), KeyValueEncodingType.INLINE);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testSeparatedJSONVersionGetProducerDataAssigned()
		public virtual void TestSeparatedJSONVersionGetProducerDataAssigned()
		{
			JSONSchema<SchemaTestUtils.Foo> FooSchema = JSONSchema.of(SchemaDefinition.builder<SchemaTestUtils.Foo>().withPojo(typeof(SchemaTestUtils.Foo)).build());
			JSONSchema<SchemaTestUtils.Bar> BarSchema = JSONSchema.of(SchemaDefinition.builder<SchemaTestUtils.Bar>().withPojo(typeof(SchemaTestUtils.Bar)).build());

			MultiVersionSchemaInfoProvider MultiVersionSchemaInfoProvider = mock(typeof(MultiVersionSchemaInfoProvider));
			Schema<KeyValue<SchemaTestUtils.Foo, SchemaTestUtils.Bar>> KeyValueSchema = Schema.KeyValue(FooSchema, BarSchema, KeyValueEncodingType.SEPARATED);
			KeyValueSchema.SchemaInfoProvider = MultiVersionSchemaInfoProvider;
			when(MultiVersionSchemaInfoProvider.getSchemaByVersion(any(typeof(sbyte[])))).thenReturn(CompletableFuture.completedFuture(KeyValueSchema.SchemaInfo));

			SchemaTestUtils.Foo Foo = new SchemaTestUtils.Foo();
			Foo.Field1 = "field1";
			Foo.Field2 = "field2";
			Foo.Field3 = 3;
			SchemaTestUtils.Bar Bar = new SchemaTestUtils.Bar();
			Bar.Field1 = true;

			sbyte[] EncodeBytes = KeyValueSchema.encode(new KeyValue(Foo, Bar));
			MessageMetadata.Builder Builder = MessageMetadata.newBuilder().setProducerName("separated");
			ByteString ByteString = ByteString.copyFrom(new sbyte[10]);
			Builder.SchemaVersion = ByteString;
			Builder.setPartitionKey(Base64.Encoder.encodeToString(FooSchema.encode(Foo)));
			Builder.PartitionKeyB64Encoded = true;
			MessageImpl<KeyValue<SchemaTestUtils.Foo, SchemaTestUtils.Bar>> Msg = MessageImpl.Create(Builder, ByteBuffer.wrap(EncodeBytes), KeyValueSchema);
			KeyValue<SchemaTestUtils.Foo, SchemaTestUtils.Bar> KeyValue = Msg.Value;
			assertEquals(KeyValue.Key, Foo);
			assertEquals(KeyValue.Value, Bar);
			assertTrue(Builder.hasPartitionKey());
			Assert.assertEquals(Enum.Parse(typeof(KeyValueEncodingType), KeyValueSchema.SchemaInfo.Properties.get("kv.encoding.type")), KeyValueEncodingType.SEPARATED);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testDefaultAVROJSONVersionGetProducerDataAssigned()
		public virtual void TestDefaultAVROJSONVersionGetProducerDataAssigned()
		{
			AvroSchema<SchemaTestUtils.Foo> FooSchema = AvroSchema.of(SchemaDefinition.builder<SchemaTestUtils.Foo>().withPojo(typeof(SchemaTestUtils.Foo)).build());
			JSONSchema<SchemaTestUtils.Bar> BarSchema = JSONSchema.of(SchemaDefinition.builder<SchemaTestUtils.Bar>().withPojo(typeof(SchemaTestUtils.Bar)).build());

			MultiVersionSchemaInfoProvider MultiVersionSchemaInfoProvider = mock(typeof(MultiVersionSchemaInfoProvider));
			Schema<KeyValue<SchemaTestUtils.Foo, SchemaTestUtils.Bar>> KeyValueSchema = Schema.KeyValue(FooSchema, BarSchema);
			KeyValueSchema.SchemaInfoProvider = MultiVersionSchemaInfoProvider;
			when(MultiVersionSchemaInfoProvider.getSchemaByVersion(any(typeof(sbyte[])))).thenReturn(CompletableFuture.completedFuture(KeyValueSchema.SchemaInfo));

			SchemaTestUtils.Foo Foo = new SchemaTestUtils.Foo();
			Foo.Field1 = "field1";
			Foo.Field2 = "field2";
			Foo.Field3 = 3;
			SchemaTestUtils.Bar Bar = new SchemaTestUtils.Bar();
			Bar.Field1 = true;

			sbyte[] EncodeBytes = KeyValueSchema.encode(new KeyValue(Foo, Bar));
			MessageMetadata.Builder Builder = MessageMetadata.newBuilder().setProducerName("default");
			ByteString ByteString = ByteString.copyFrom(new sbyte[10]);
			Builder.SchemaVersion = ByteString;
			MessageImpl<KeyValue<SchemaTestUtils.Foo, SchemaTestUtils.Bar>> Msg = MessageImpl.Create(Builder, ByteBuffer.wrap(EncodeBytes), KeyValueSchema);
			KeyValue<SchemaTestUtils.Foo, SchemaTestUtils.Bar> KeyValue = Msg.Value;
			assertEquals(KeyValue.Key, Foo);
			assertEquals(KeyValue.Value, Bar);
			assertFalse(Builder.hasPartitionKey());
			Assert.assertEquals(Enum.Parse(typeof(KeyValueEncodingType), KeyValueSchema.SchemaInfo.Properties.get("kv.encoding.type")), KeyValueEncodingType.INLINE);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testSeparatedAVROJSONVersionGetProducerDataAssigned()
		public virtual void TestSeparatedAVROJSONVersionGetProducerDataAssigned()
		{
			AvroSchema<SchemaTestUtils.Foo> FooSchema = AvroSchema.of(SchemaDefinition.builder<SchemaTestUtils.Foo>().withPojo(typeof(SchemaTestUtils.Foo)).build());
			JSONSchema<SchemaTestUtils.Bar> BarSchema = JSONSchema.of(SchemaDefinition.builder<SchemaTestUtils.Bar>().withPojo(typeof(SchemaTestUtils.Bar)).build());

			MultiVersionSchemaInfoProvider MultiVersionSchemaInfoProvider = mock(typeof(MultiVersionSchemaInfoProvider));
			Schema<KeyValue<SchemaTestUtils.Foo, SchemaTestUtils.Bar>> KeyValueSchema = Schema.KeyValue(FooSchema, BarSchema, KeyValueEncodingType.SEPARATED);
			KeyValueSchema.SchemaInfoProvider = MultiVersionSchemaInfoProvider;
			when(MultiVersionSchemaInfoProvider.getSchemaByVersion(any(typeof(sbyte[])))).thenReturn(CompletableFuture.completedFuture(KeyValueSchema.SchemaInfo));

			SchemaTestUtils.Foo Foo = new SchemaTestUtils.Foo();
			Foo.Field1 = "field1";
			Foo.Field2 = "field2";
			Foo.Field3 = 3;
			SchemaTestUtils.Bar Bar = new SchemaTestUtils.Bar();
			Bar.Field1 = true;

			sbyte[] EncodeBytes = KeyValueSchema.encode(new KeyValue(Foo, Bar));
			MessageMetadata.Builder Builder = MessageMetadata.newBuilder().setProducerName("separated");
			ByteString ByteString = ByteString.copyFrom(new sbyte[10]);
			Builder.SchemaVersion = ByteString;
			Builder.setPartitionKey(Base64.Encoder.encodeToString(FooSchema.encode(Foo)));
			Builder.PartitionKeyB64Encoded = true;
			MessageImpl<KeyValue<SchemaTestUtils.Foo, SchemaTestUtils.Bar>> Msg = MessageImpl.Create(Builder, ByteBuffer.wrap(EncodeBytes), KeyValueSchema);
			KeyValue<SchemaTestUtils.Foo, SchemaTestUtils.Bar> KeyValue = Msg.Value;
			assertEquals(KeyValue.Key, Foo);
			assertEquals(KeyValue.Value, Bar);
			assertTrue(Builder.hasPartitionKey());
			Assert.assertEquals(Enum.Parse(typeof(KeyValueEncodingType), KeyValueSchema.SchemaInfo.Properties.get("kv.encoding.type")), KeyValueEncodingType.SEPARATED);
		}
	}

}