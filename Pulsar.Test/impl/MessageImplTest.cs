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
namespace org.apache.pulsar.client.impl
{

	using Schema = org.apache.pulsar.client.api.Schema;
	using SchemaDefinition = org.apache.pulsar.client.api.schema.SchemaDefinition;
	using org.apache.pulsar.client.impl.schema;
	using org.apache.pulsar.client.impl.schema;
	using SchemaTestUtils = org.apache.pulsar.client.impl.schema.SchemaTestUtils;
	using MultiVersionSchemaInfoProvider = org.apache.pulsar.client.impl.schema.generic.MultiVersionSchemaInfoProvider;
	using MessageMetadata = org.apache.pulsar.common.api.proto.PulsarApi.MessageMetadata;
	using KeyValue = org.apache.pulsar.common.schema.KeyValue;
	using KeyValueEncodingType = org.apache.pulsar.common.schema.KeyValueEncodingType;
	using ByteString = org.apache.pulsar.shaded.com.google.protobuf.v241.ByteString;
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
		public virtual void testGetSequenceIdNotAssociated()
		{
			MessageMetadata.Builder builder = MessageMetadata.newBuilder();
			ByteBuffer payload = ByteBuffer.wrap(new sbyte[0]);
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: MessageImpl<?> msg = MessageImpl.create(builder, payload, org.apache.pulsar.client.api.Schema.BYTES);
			MessageImpl<object> msg = MessageImpl.create(builder, payload, Schema.BYTES);

			assertEquals(-1, msg.SequenceId);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testGetSequenceIdAssociated()
		public virtual void testGetSequenceIdAssociated()
		{
			MessageMetadata.Builder builder = MessageMetadata.newBuilder().setSequenceId(1234);

			ByteBuffer payload = ByteBuffer.wrap(new sbyte[0]);
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: MessageImpl<?> msg = MessageImpl.create(builder, payload, org.apache.pulsar.client.api.Schema.BYTES);
			MessageImpl<object> msg = MessageImpl.create(builder, payload, Schema.BYTES);

			assertEquals(1234, msg.SequenceId);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testGetProducerNameNotAssigned()
		public virtual void testGetProducerNameNotAssigned()
		{
			MessageMetadata.Builder builder = MessageMetadata.newBuilder();
			ByteBuffer payload = ByteBuffer.wrap(new sbyte[0]);
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: MessageImpl<?> msg = MessageImpl.create(builder, payload, org.apache.pulsar.client.api.Schema.BYTES);
			MessageImpl<object> msg = MessageImpl.create(builder, payload, Schema.BYTES);

			assertNull(msg.ProducerName);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testGetProducerNameAssigned()
		public virtual void testGetProducerNameAssigned()
		{
			MessageMetadata.Builder builder = MessageMetadata.newBuilder().setProducerName("test-producer");

			ByteBuffer payload = ByteBuffer.wrap(new sbyte[0]);
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: MessageImpl<?> msg = MessageImpl.create(builder, payload, org.apache.pulsar.client.api.Schema.BYTES);
			MessageImpl<object> msg = MessageImpl.create(builder, payload, Schema.BYTES);

			assertEquals("test-producer", msg.ProducerName);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testDefaultGetProducerDataAssigned()
		public virtual void testDefaultGetProducerDataAssigned()
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
			sbyte[] encodeBytes = keyValueSchema.encode(new KeyValue(foo, bar));
			MessageMetadata.Builder builder = MessageMetadata.newBuilder().setProducerName("default");
			MessageImpl<KeyValue<SchemaTestUtils.Foo, SchemaTestUtils.Bar>> msg = MessageImpl.create(builder, ByteBuffer.wrap(encodeBytes), keyValueSchema);
			KeyValue<SchemaTestUtils.Foo, SchemaTestUtils.Bar> keyValue = msg.Value;
			assertEquals(keyValue.Key, foo);
			assertEquals(keyValue.Value, bar);
			assertFalse(builder.hasPartitionKey());
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testInlineGetProducerDataAssigned()
		public virtual void testInlineGetProducerDataAssigned()
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
			sbyte[] encodeBytes = keyValueSchema.encode(new KeyValue(foo, bar));
			MessageMetadata.Builder builder = MessageMetadata.newBuilder().setProducerName("inline");
			MessageImpl<KeyValue<SchemaTestUtils.Foo, SchemaTestUtils.Bar>> msg = MessageImpl.create(builder, ByteBuffer.wrap(encodeBytes), keyValueSchema);
			KeyValue<SchemaTestUtils.Foo, SchemaTestUtils.Bar> keyValue = msg.Value;
			assertEquals(keyValue.Key, foo);
			assertEquals(keyValue.Value, bar);
			assertFalse(builder.hasPartitionKey());
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testSeparatedGetProducerDataAssigned()
		public virtual void testSeparatedGetProducerDataAssigned()
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
			sbyte[] encodeBytes = keyValueSchema.encode(new KeyValue(foo, bar));
			MessageMetadata.Builder builder = MessageMetadata.newBuilder().setProducerName("separated");
			builder.PartitionKey = Base64.Encoder.encodeToString(fooSchema.encode(foo));
			builder.PartitionKeyB64Encoded = true;
			MessageImpl<KeyValue<SchemaTestUtils.Foo, SchemaTestUtils.Bar>> msg = MessageImpl.create(builder, ByteBuffer.wrap(encodeBytes), keyValueSchema);
			KeyValue<SchemaTestUtils.Foo, SchemaTestUtils.Bar> keyValue = msg.Value;
			assertEquals(keyValue.Key, foo);
			assertEquals(keyValue.Value, bar);
			assertTrue(builder.hasPartitionKey());
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testDefaultAVROVersionGetProducerDataAssigned()
		public virtual void testDefaultAVROVersionGetProducerDataAssigned()
		{
			AvroSchema<SchemaTestUtils.Foo> fooSchema = AvroSchema.of(SchemaDefinition.builder<SchemaTestUtils.Foo>().withPojo(typeof(SchemaTestUtils.Foo)).build());
			AvroSchema<SchemaTestUtils.Bar> barSchema = AvroSchema.of(SchemaDefinition.builder<SchemaTestUtils.Bar>().withPojo(typeof(SchemaTestUtils.Bar)).build());

			MultiVersionSchemaInfoProvider multiVersionSchemaInfoProvider = mock(typeof(MultiVersionSchemaInfoProvider));
			Schema<KeyValue<SchemaTestUtils.Foo, SchemaTestUtils.Bar>> keyValueSchema = Schema.KeyValue(fooSchema, barSchema);
			keyValueSchema.SchemaInfoProvider = multiVersionSchemaInfoProvider;
			when(multiVersionSchemaInfoProvider.getSchemaByVersion(any(typeof(sbyte[])))).thenReturn(CompletableFuture.completedFuture(keyValueSchema.SchemaInfo));

			SchemaTestUtils.Foo foo = new SchemaTestUtils.Foo();
			foo.Field1 = "field1";
			foo.Field2 = "field2";
			foo.Field3 = 3;
			SchemaTestUtils.Bar bar = new SchemaTestUtils.Bar();
			bar.Field1 = true;

			sbyte[] encodeBytes = keyValueSchema.encode(new KeyValue(foo, bar));
			MessageMetadata.Builder builder = MessageMetadata.newBuilder().setProducerName("default");
			ByteString byteString = ByteString.copyFrom(new sbyte[10]);
			builder.SchemaVersion = byteString;
			MessageImpl<KeyValue<SchemaTestUtils.Foo, SchemaTestUtils.Bar>> msg = MessageImpl.create(builder, ByteBuffer.wrap(encodeBytes), keyValueSchema);
			KeyValue<SchemaTestUtils.Foo, SchemaTestUtils.Bar> keyValue = msg.Value;
			assertEquals(keyValue.Key, foo);
			assertEquals(keyValue.Value, bar);
			assertFalse(builder.hasPartitionKey());
			Assert.assertEquals(KeyValueEncodingType.valueOf(keyValueSchema.SchemaInfo.Properties.get("kv.encoding.type")), KeyValueEncodingType.INLINE);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testSeparatedAVROVersionGetProducerDataAssigned()
		public virtual void testSeparatedAVROVersionGetProducerDataAssigned()
		{
			AvroSchema<SchemaTestUtils.Foo> fooSchema = AvroSchema.of(SchemaDefinition.builder<SchemaTestUtils.Foo>().withPojo(typeof(SchemaTestUtils.Foo)).build());
			AvroSchema<SchemaTestUtils.Bar> barSchema = AvroSchema.of(SchemaDefinition.builder<SchemaTestUtils.Bar>().withPojo(typeof(SchemaTestUtils.Bar)).build());

			MultiVersionSchemaInfoProvider multiVersionSchemaInfoProvider = mock(typeof(MultiVersionSchemaInfoProvider));
			Schema<KeyValue<SchemaTestUtils.Foo, SchemaTestUtils.Bar>> keyValueSchema = Schema.KeyValue(fooSchema, barSchema, KeyValueEncodingType.SEPARATED);
			keyValueSchema.SchemaInfoProvider = multiVersionSchemaInfoProvider;
			when(multiVersionSchemaInfoProvider.getSchemaByVersion(any(typeof(sbyte[])))).thenReturn(CompletableFuture.completedFuture(keyValueSchema.SchemaInfo));

			SchemaTestUtils.Foo foo = new SchemaTestUtils.Foo();
			foo.Field1 = "field1";
			foo.Field2 = "field2";
			foo.Field3 = 3;
			SchemaTestUtils.Bar bar = new SchemaTestUtils.Bar();
			bar.Field1 = true;

			sbyte[] encodeBytes = keyValueSchema.encode(new KeyValue(foo, bar));
			MessageMetadata.Builder builder = MessageMetadata.newBuilder().setProducerName("separated");
			ByteString byteString = ByteString.copyFrom(new sbyte[10]);
			builder.SchemaVersion = byteString;
			builder.PartitionKey = Base64.Encoder.encodeToString(fooSchema.encode(foo));
			builder.PartitionKeyB64Encoded = true;
			MessageImpl<KeyValue<SchemaTestUtils.Foo, SchemaTestUtils.Bar>> msg = MessageImpl.create(builder, ByteBuffer.wrap(encodeBytes), keyValueSchema);
			KeyValue<SchemaTestUtils.Foo, SchemaTestUtils.Bar> keyValue = msg.Value;
			assertEquals(keyValue.Key, foo);
			assertEquals(keyValue.Value, bar);
			assertTrue(builder.hasPartitionKey());
			Assert.assertEquals(KeyValueEncodingType.valueOf(keyValueSchema.SchemaInfo.Properties.get("kv.encoding.type")), KeyValueEncodingType.SEPARATED);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testDefaultJSONVersionGetProducerDataAssigned()
		public virtual void testDefaultJSONVersionGetProducerDataAssigned()
		{
			JSONSchema<SchemaTestUtils.Foo> fooSchema = JSONSchema.of(SchemaDefinition.builder<SchemaTestUtils.Foo>().withPojo(typeof(SchemaTestUtils.Foo)).build());
			JSONSchema<SchemaTestUtils.Bar> barSchema = JSONSchema.of(SchemaDefinition.builder<SchemaTestUtils.Bar>().withPojo(typeof(SchemaTestUtils.Bar)).build());

			MultiVersionSchemaInfoProvider multiVersionSchemaInfoProvider = mock(typeof(MultiVersionSchemaInfoProvider));
			Schema<KeyValue<SchemaTestUtils.Foo, SchemaTestUtils.Bar>> keyValueSchema = Schema.KeyValue(fooSchema, barSchema);
			keyValueSchema.SchemaInfoProvider = multiVersionSchemaInfoProvider;
			when(multiVersionSchemaInfoProvider.getSchemaByVersion(any(typeof(sbyte[])))).thenReturn(CompletableFuture.completedFuture(keyValueSchema.SchemaInfo));

			SchemaTestUtils.Foo foo = new SchemaTestUtils.Foo();
			foo.Field1 = "field1";
			foo.Field2 = "field2";
			foo.Field3 = 3;
			SchemaTestUtils.Bar bar = new SchemaTestUtils.Bar();
			bar.Field1 = true;

			sbyte[] encodeBytes = keyValueSchema.encode(new KeyValue(foo, bar));
			MessageMetadata.Builder builder = MessageMetadata.newBuilder().setProducerName("default");
			ByteString byteString = ByteString.copyFrom(new sbyte[10]);
			builder.SchemaVersion = byteString;
			MessageImpl<KeyValue<SchemaTestUtils.Foo, SchemaTestUtils.Bar>> msg = MessageImpl.create(builder, ByteBuffer.wrap(encodeBytes), keyValueSchema);
			KeyValue<SchemaTestUtils.Foo, SchemaTestUtils.Bar> keyValue = msg.Value;
			assertEquals(keyValue.Key, foo);
			assertEquals(keyValue.Value, bar);
			assertFalse(builder.hasPartitionKey());
			Assert.assertEquals(KeyValueEncodingType.valueOf(keyValueSchema.SchemaInfo.Properties.get("kv.encoding.type")), KeyValueEncodingType.INLINE);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testSeparatedJSONVersionGetProducerDataAssigned()
		public virtual void testSeparatedJSONVersionGetProducerDataAssigned()
		{
			JSONSchema<SchemaTestUtils.Foo> fooSchema = JSONSchema.of(SchemaDefinition.builder<SchemaTestUtils.Foo>().withPojo(typeof(SchemaTestUtils.Foo)).build());
			JSONSchema<SchemaTestUtils.Bar> barSchema = JSONSchema.of(SchemaDefinition.builder<SchemaTestUtils.Bar>().withPojo(typeof(SchemaTestUtils.Bar)).build());

			MultiVersionSchemaInfoProvider multiVersionSchemaInfoProvider = mock(typeof(MultiVersionSchemaInfoProvider));
			Schema<KeyValue<SchemaTestUtils.Foo, SchemaTestUtils.Bar>> keyValueSchema = Schema.KeyValue(fooSchema, barSchema, KeyValueEncodingType.SEPARATED);
			keyValueSchema.SchemaInfoProvider = multiVersionSchemaInfoProvider;
			when(multiVersionSchemaInfoProvider.getSchemaByVersion(any(typeof(sbyte[])))).thenReturn(CompletableFuture.completedFuture(keyValueSchema.SchemaInfo));

			SchemaTestUtils.Foo foo = new SchemaTestUtils.Foo();
			foo.Field1 = "field1";
			foo.Field2 = "field2";
			foo.Field3 = 3;
			SchemaTestUtils.Bar bar = new SchemaTestUtils.Bar();
			bar.Field1 = true;

			sbyte[] encodeBytes = keyValueSchema.encode(new KeyValue(foo, bar));
			MessageMetadata.Builder builder = MessageMetadata.newBuilder().setProducerName("separated");
			ByteString byteString = ByteString.copyFrom(new sbyte[10]);
			builder.SchemaVersion = byteString;
			builder.PartitionKey = Base64.Encoder.encodeToString(fooSchema.encode(foo));
			builder.PartitionKeyB64Encoded = true;
			MessageImpl<KeyValue<SchemaTestUtils.Foo, SchemaTestUtils.Bar>> msg = MessageImpl.create(builder, ByteBuffer.wrap(encodeBytes), keyValueSchema);
			KeyValue<SchemaTestUtils.Foo, SchemaTestUtils.Bar> keyValue = msg.Value;
			assertEquals(keyValue.Key, foo);
			assertEquals(keyValue.Value, bar);
			assertTrue(builder.hasPartitionKey());
			Assert.assertEquals(KeyValueEncodingType.valueOf(keyValueSchema.SchemaInfo.Properties.get("kv.encoding.type")), KeyValueEncodingType.SEPARATED);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testDefaultAVROJSONVersionGetProducerDataAssigned()
		public virtual void testDefaultAVROJSONVersionGetProducerDataAssigned()
		{
			AvroSchema<SchemaTestUtils.Foo> fooSchema = AvroSchema.of(SchemaDefinition.builder<SchemaTestUtils.Foo>().withPojo(typeof(SchemaTestUtils.Foo)).build());
			JSONSchema<SchemaTestUtils.Bar> barSchema = JSONSchema.of(SchemaDefinition.builder<SchemaTestUtils.Bar>().withPojo(typeof(SchemaTestUtils.Bar)).build());

			MultiVersionSchemaInfoProvider multiVersionSchemaInfoProvider = mock(typeof(MultiVersionSchemaInfoProvider));
			Schema<KeyValue<SchemaTestUtils.Foo, SchemaTestUtils.Bar>> keyValueSchema = Schema.KeyValue(fooSchema, barSchema);
			keyValueSchema.SchemaInfoProvider = multiVersionSchemaInfoProvider;
			when(multiVersionSchemaInfoProvider.getSchemaByVersion(any(typeof(sbyte[])))).thenReturn(CompletableFuture.completedFuture(keyValueSchema.SchemaInfo));

			SchemaTestUtils.Foo foo = new SchemaTestUtils.Foo();
			foo.Field1 = "field1";
			foo.Field2 = "field2";
			foo.Field3 = 3;
			SchemaTestUtils.Bar bar = new SchemaTestUtils.Bar();
			bar.Field1 = true;

			sbyte[] encodeBytes = keyValueSchema.encode(new KeyValue(foo, bar));
			MessageMetadata.Builder builder = MessageMetadata.newBuilder().setProducerName("default");
			ByteString byteString = ByteString.copyFrom(new sbyte[10]);
			builder.SchemaVersion = byteString;
			MessageImpl<KeyValue<SchemaTestUtils.Foo, SchemaTestUtils.Bar>> msg = MessageImpl.create(builder, ByteBuffer.wrap(encodeBytes), keyValueSchema);
			KeyValue<SchemaTestUtils.Foo, SchemaTestUtils.Bar> keyValue = msg.Value;
			assertEquals(keyValue.Key, foo);
			assertEquals(keyValue.Value, bar);
			assertFalse(builder.hasPartitionKey());
			Assert.assertEquals(KeyValueEncodingType.valueOf(keyValueSchema.SchemaInfo.Properties.get("kv.encoding.type")), KeyValueEncodingType.INLINE);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testSeparatedAVROJSONVersionGetProducerDataAssigned()
		public virtual void testSeparatedAVROJSONVersionGetProducerDataAssigned()
		{
			AvroSchema<SchemaTestUtils.Foo> fooSchema = AvroSchema.of(SchemaDefinition.builder<SchemaTestUtils.Foo>().withPojo(typeof(SchemaTestUtils.Foo)).build());
			JSONSchema<SchemaTestUtils.Bar> barSchema = JSONSchema.of(SchemaDefinition.builder<SchemaTestUtils.Bar>().withPojo(typeof(SchemaTestUtils.Bar)).build());

			MultiVersionSchemaInfoProvider multiVersionSchemaInfoProvider = mock(typeof(MultiVersionSchemaInfoProvider));
			Schema<KeyValue<SchemaTestUtils.Foo, SchemaTestUtils.Bar>> keyValueSchema = Schema.KeyValue(fooSchema, barSchema, KeyValueEncodingType.SEPARATED);
			keyValueSchema.SchemaInfoProvider = multiVersionSchemaInfoProvider;
			when(multiVersionSchemaInfoProvider.getSchemaByVersion(any(typeof(sbyte[])))).thenReturn(CompletableFuture.completedFuture(keyValueSchema.SchemaInfo));

			SchemaTestUtils.Foo foo = new SchemaTestUtils.Foo();
			foo.Field1 = "field1";
			foo.Field2 = "field2";
			foo.Field3 = 3;
			SchemaTestUtils.Bar bar = new SchemaTestUtils.Bar();
			bar.Field1 = true;

			sbyte[] encodeBytes = keyValueSchema.encode(new KeyValue(foo, bar));
			MessageMetadata.Builder builder = MessageMetadata.newBuilder().setProducerName("separated");
			ByteString byteString = ByteString.copyFrom(new sbyte[10]);
			builder.SchemaVersion = byteString;
			builder.PartitionKey = Base64.Encoder.encodeToString(fooSchema.encode(foo));
			builder.PartitionKeyB64Encoded = true;
			MessageImpl<KeyValue<SchemaTestUtils.Foo, SchemaTestUtils.Bar>> msg = MessageImpl.create(builder, ByteBuffer.wrap(encodeBytes), keyValueSchema);
			KeyValue<SchemaTestUtils.Foo, SchemaTestUtils.Bar> keyValue = msg.Value;
			assertEquals(keyValue.Key, foo);
			assertEquals(keyValue.Value, bar);
			assertTrue(builder.hasPartitionKey());
			Assert.assertEquals(KeyValueEncodingType.valueOf(keyValueSchema.SchemaInfo.Properties.get("kv.encoding.type")), KeyValueEncodingType.SEPARATED);
		}
	}

}