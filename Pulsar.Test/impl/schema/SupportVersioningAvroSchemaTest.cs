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
	using SchemaDefinition = api.schema.SchemaDefinition;
	using GenericAvroSchema = generic.GenericAvroSchema;
	using MultiVersionSchemaInfoProvider = generic.MultiVersionSchemaInfoProvider;
	using SchemaInfo = common.schema.SchemaInfo;
	using BeforeMethod = org.testng.annotations.BeforeMethod;
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
//	import static org.testng.Assert.assertTrue;

	public class SupportVersioningAvroSchemaTest
	{
		private MultiVersionSchemaInfoProvider multiVersionSchemaInfoProvider;
		private AvroSchema schema;
		private GenericAvroSchema genericAvroSchema;
		private AvroSchema<SchemaTestUtils.FooV2> avroFooV2Schema;


//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @BeforeMethod public void setup()
		public virtual void setup()
		{
			this.multiVersionSchemaInfoProvider = mock(typeof(MultiVersionSchemaInfoProvider));
			avroFooV2Schema = AvroSchema.of(SchemaDefinition.builder<SchemaTestUtils.FooV2>().withAlwaysAllowNull(false).withPojo(typeof(SchemaTestUtils.FooV2)).build());
			this.schema = AvroSchema.of(SchemaDefinition.builder().withPojo(typeof(SchemaTestUtils.Foo)).withAlwaysAllowNull(false).withSupportSchemaVersioning(true).build());
			schema.SchemaInfoProvider = multiVersionSchemaInfoProvider;
			SchemaInfo schemaInfo = avroFooV2Schema.schemaInfo;
			genericAvroSchema = new GenericAvroSchema(schemaInfo);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testDecode()
		public virtual void testDecode()
		{
			when(multiVersionSchemaInfoProvider.getSchemaByVersion(any(typeof(sbyte[])))).thenReturn(CompletableFuture.completedFuture(genericAvroSchema.SchemaInfo));
			SchemaTestUtils.FooV2 fooV2 = new SchemaTestUtils.FooV2();
			fooV2.Field1 = SchemaTestUtils.TEST_MULTI_VERSION_SCHEMA_STRING;
			SchemaTestUtils.Foo foo = (SchemaTestUtils.Foo)schema.decode(avroFooV2Schema.encode(fooV2), new sbyte[10]);
			assertTrue(schema.supportSchemaVersioning());
			assertEquals(SchemaTestUtils.TEST_MULTI_VERSION_SCHEMA_STRING, foo.Field1);
			assertEquals(SchemaTestUtils.TEST_MULTI_VERSION_SCHEMA_DEFAULT_STRING, foo.FieldUnableNull);
		}

	}

}