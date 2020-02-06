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
namespace org.apache.pulsar.client.impl.schema.generic
{
	using GenericRecord = api.schema.GenericRecord;
	using SchemaDefinition = api.schema.SchemaDefinition;
	using org.apache.pulsar.client.impl.schema;
	using Foo = SchemaTestUtils.Foo;
	using FooV2 = SchemaTestUtils.FooV2;
	using Assert = org.testng.Assert;
	using BeforeMethod = org.testng.annotations.BeforeMethod;
	using Test = org.testng.annotations.Test;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;

	public class GenericAvroSchemaTest
	{

		private GenericAvroSchema writerSchema;
		private GenericAvroSchema readerSchema;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @BeforeMethod public void init()
		public virtual void init()
		{
			AvroSchema<FooV2> avroFooV2Schema = AvroSchema.of(SchemaDefinition.builder<FooV2>().withAlwaysAllowNull(false).withPojo(typeof(FooV2)).build());
			AvroSchema<Foo> avroFooSchema = AvroSchema.of(SchemaDefinition.builder<Foo>().withAlwaysAllowNull(false).withPojo(typeof(Foo)).build());
			writerSchema = new GenericAvroSchema(avroFooV2Schema.SchemaInfo);
			readerSchema = new GenericAvroSchema(avroFooSchema.SchemaInfo);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testSupportMultiVersioningSupportByDefault()
		public virtual void testSupportMultiVersioningSupportByDefault()
		{
			Assert.assertTrue(writerSchema.supportSchemaVersioning());
			Assert.assertTrue(readerSchema.supportSchemaVersioning());
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expectedExceptions = org.apache.pulsar.client.api.SchemaSerializationException.class) public void testFailDecodeWithoutMultiVersioningSupport()
		public virtual void testFailDecodeWithoutMultiVersioningSupport()
		{
			GenericRecord dataForWriter = writerSchema.newRecordBuilder().set("field1", SchemaTestUtils.TEST_MULTI_VERSION_SCHEMA_STRING).set("field3", 0).build();
			readerSchema.decode(writerSchema.encode(dataForWriter));
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testDecodeWithMultiVersioningSupport()
		public virtual void testDecodeWithMultiVersioningSupport()
		{
			MultiVersionSchemaInfoProvider provider = mock(typeof(MultiVersionSchemaInfoProvider));
			readerSchema.SchemaInfoProvider = provider;
			when(provider.getSchemaByVersion(any(typeof(sbyte[])))).thenReturn(CompletableFuture.completedFuture(writerSchema.SchemaInfo));
			GenericRecord dataForWriter = writerSchema.newRecordBuilder().set("field1", SchemaTestUtils.TEST_MULTI_VERSION_SCHEMA_STRING).set("field3", 0).build();
			GenericRecord record = readerSchema.decode(writerSchema.encode(dataForWriter), new sbyte[10]);
			Assert.assertEquals(SchemaTestUtils.TEST_MULTI_VERSION_SCHEMA_STRING, record.getField("field1"));
			Assert.assertEquals(0, record.getField("field3"));
			Assert.assertEquals(SchemaTestUtils.TEST_MULTI_VERSION_SCHEMA_DEFAULT_STRING, record.getField("fieldUnableNull"));
		}
	}

}