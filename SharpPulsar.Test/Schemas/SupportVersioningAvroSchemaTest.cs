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
	using SchemaDefinition = org.apache.pulsar.client.api.schema.SchemaDefinition;
	using GenericAvroSchema = Org.Apache.Pulsar.Client.Impl.Schema.Generic.GenericAvroSchema;
	using MultiVersionSchemaInfoProvider = Org.Apache.Pulsar.Client.Impl.Schema.Generic.MultiVersionSchemaInfoProvider;
	using SchemaInfo = org.apache.pulsar.common.schema.SchemaInfo;
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
		private MultiVersionSchemaInfoProvider _multiVersionSchemaInfoProvider;
		private AvroSchema _schema;
		private GenericAvroSchema _genericAvroSchema;
		private AvroSchema<SchemaTestUtils.FooV2> _avroFooV2Schema;


//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @BeforeMethod public void setup()
		public virtual void Setup()
		{
			this._multiVersionSchemaInfoProvider = mock(typeof(MultiVersionSchemaInfoProvider));
			_avroFooV2Schema = AvroSchema.of(SchemaDefinition.builder<SchemaTestUtils.FooV2>().withAlwaysAllowNull(false).withPojo(typeof(SchemaTestUtils.FooV2)).build());
			this._schema = AvroSchema.of(SchemaDefinition.builder().withPojo(typeof(SchemaTestUtils.Foo)).withAlwaysAllowNull(false).withSupportSchemaVersioning(true).build());
			_schema.SchemaInfoProvider = _multiVersionSchemaInfoProvider;
			SchemaInfo SchemaInfo = _avroFooV2Schema.schemaInfo;
			_genericAvroSchema = new GenericAvroSchema(SchemaInfo);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testDecode()
		public virtual void TestDecode()
		{
			when(_multiVersionSchemaInfoProvider.getSchemaByVersion(any(typeof(sbyte[])))).thenReturn(CompletableFuture.completedFuture(_genericAvroSchema.SchemaInfo));
			SchemaTestUtils.FooV2 FooV2 = new SchemaTestUtils.FooV2();
			FooV2.Field1 = SchemaTestUtils.TestMultiVersionSchemaString;
			SchemaTestUtils.Foo Foo = (SchemaTestUtils.Foo)_schema.decode(_avroFooV2Schema.encode(FooV2), new sbyte[10]);
			assertTrue(_schema.supportSchemaVersioning());
			assertEquals(SchemaTestUtils.TestMultiVersionSchemaString, Foo.Field1);
			assertEquals(SchemaTestUtils.TestMultiVersionSchemaDefaultString, Foo.FieldUnableNull);
		}

	}

}