﻿/// <summary>
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
namespace Org.Apache.Pulsar.Client.Impl.Schema.Generic
{
	using GenericRecord = org.apache.pulsar.client.api.schema.GenericRecord;
	using SchemaDefinition = org.apache.pulsar.client.api.schema.SchemaDefinition;
	using AvroSchema = Org.Apache.Pulsar.Client.Impl.Schema.AvroSchema;
	using SchemaTestUtils = Org.Apache.Pulsar.Client.Impl.Schema.SchemaTestUtils;
	using Foo = Org.Apache.Pulsar.Client.Impl.Schema.SchemaTestUtils.Foo;
	using FooV2 = Org.Apache.Pulsar.Client.Impl.Schema.SchemaTestUtils.FooV2;
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

		private GenericAvroSchema _writerSchema;
		private GenericAvroSchema _readerSchema;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @BeforeMethod public void init()
		public virtual void Init()
		{
			AvroSchema<SchemaTestUtils.FooV2> AvroFooV2Schema = AvroSchema.of(SchemaDefinition.builder<SchemaTestUtils.FooV2>().withAlwaysAllowNull(false).withPojo(typeof(SchemaTestUtils.FooV2)).build());
			AvroSchema<SchemaTestUtils.Foo> AvroFooSchema = AvroSchema.of(SchemaDefinition.builder<SchemaTestUtils.Foo>().withAlwaysAllowNull(false).withPojo(typeof(SchemaTestUtils.Foo)).build());
			_writerSchema = new GenericAvroSchema(AvroFooV2Schema.SchemaInfo);
			_readerSchema = new GenericAvroSchema(AvroFooSchema.SchemaInfo);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testSupportMultiVersioningSupportByDefault()
		public virtual void TestSupportMultiVersioningSupportByDefault()
		{
			Assert.assertTrue(_writerSchema.supportSchemaVersioning());
			Assert.assertTrue(_readerSchema.supportSchemaVersioning());
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expectedExceptions = org.apache.pulsar.client.api.SchemaSerializationException.class) public void testFailDecodeWithoutMultiVersioningSupport()
		public virtual void TestFailDecodeWithoutMultiVersioningSupport()
		{
			GenericRecord DataForWriter = _writerSchema.newRecordBuilder().set("field1", SchemaTestUtils.TestMultiVersionSchemaString).set("field3", 0).build();
			_readerSchema.decode(_writerSchema.encode(DataForWriter));
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testDecodeWithMultiVersioningSupport()
		public virtual void TestDecodeWithMultiVersioningSupport()
		{
			MultiVersionSchemaInfoProvider Provider = mock(typeof(MultiVersionSchemaInfoProvider));
			_readerSchema.SchemaInfoProvider = Provider;
			when(Provider.getSchemaByVersion(any(typeof(sbyte[])))).thenReturn(CompletableFuture.completedFuture(_writerSchema.SchemaInfo));
			GenericRecord DataForWriter = _writerSchema.newRecordBuilder().set("field1", SchemaTestUtils.TestMultiVersionSchemaString).set("field3", 0).build();
			GenericRecord Record = _readerSchema.decode(_writerSchema.encode(DataForWriter), new sbyte[10]);
			Assert.assertEquals(SchemaTestUtils.TestMultiVersionSchemaString, Record.getField("field1"));
			Assert.assertEquals(0, Record.getField("field3"));
			Assert.assertEquals(SchemaTestUtils.TestMultiVersionSchemaDefaultString, Record.getField("fieldUnableNull"));
		}
	}

}