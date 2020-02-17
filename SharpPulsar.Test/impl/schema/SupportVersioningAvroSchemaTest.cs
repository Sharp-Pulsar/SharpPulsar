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
namespace SharpPulsar.Test.Impl.schema
{
    using GenericAvroSchema = Org.Apache.Pulsar.Client.Impl.Schema.Generic.GenericAvroSchema;
	using MultiVersionSchemaInfoProvider = Org.Apache.Pulsar.Client.Impl.Schema.Generic.MultiVersionSchemaInfoProvider;
	using SchemaInfo = Org.Apache.Pulsar.Common.Schema.SchemaInfo;

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
		public virtual void Setup()
		{
			this.multiVersionSchemaInfoProvider = mock(typeof(MultiVersionSchemaInfoProvider));
			avroFooV2Schema = AvroSchema.Of(SchemaDefinition.builder<SchemaTestUtils.FooV2>().withAlwaysAllowNull(false).withPojo(typeof(SchemaTestUtils.FooV2)).build());
			this.schema = AvroSchema.Of(SchemaDefinition.builder().withPojo(typeof(SchemaTestUtils.Foo)).withAlwaysAllowNull(false).withSupportSchemaVersioning(true).build());
			schema.SchemaInfoProvider = multiVersionSchemaInfoProvider;
			SchemaInfo SchemaInfo = avroFooV2Schema.SchemaInfoConflict;
			genericAvroSchema = new GenericAvroSchema(SchemaInfo);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testDecode()
		public virtual void TestDecode()
		{
			when(multiVersionSchemaInfoProvider.GetSchemaByVersion(any(typeof(sbyte[])))).thenReturn(CompletableFuture.completedFuture(genericAvroSchema.SchemaInfo));
			SchemaTestUtils.FooV2 FooV2 = new SchemaTestUtils.FooV2();
			FooV2.Field1 = SchemaTestUtils.TestMultiVersionSchemaString;
			SchemaTestUtils.Foo Foo = (SchemaTestUtils.Foo)schema.decode(avroFooV2Schema.Encode(FooV2), new sbyte[10]);
			assertTrue(schema.supportSchemaVersioning());
			assertEquals(SchemaTestUtils.TestMultiVersionSchemaString, Foo.Field1);
			assertEquals(SchemaTestUtils.TestMultiVersionSchemaDefaultString, Foo.FieldUnableNull);
		}

	}

}