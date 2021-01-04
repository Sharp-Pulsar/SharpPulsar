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
namespace SharpPulsar.Test.Schema
{
	public class SupportVersioningAvroSchemaTest
	{
		private MultiVersionSchemaInfoProvider _multiVersionSchemaInfoProvider;
		private AvroSchema _schema;
		private GenericAvroSchema _genericAvroSchema;
		private AvroSchema<SchemaTestUtils.FooV2> _avroFooV2Schema;

		public virtual void Setup()
		{
			this._multiVersionSchemaInfoProvider = mock(typeof(MultiVersionSchemaInfoProvider));
			_avroFooV2Schema = AvroSchema.of(SchemaDefinition.builder<SchemaTestUtils.FooV2>().withAlwaysAllowNull(false).withPojo(typeof(SchemaTestUtils.FooV2)).build());
			this._schema = AvroSchema.of(SchemaDefinition.builder().withPojo(typeof(SchemaTestUtils.Foo)).withAlwaysAllowNull(false).withSupportSchemaVersioning(true).build());
			_schema.SchemaInfoProvider = _multiVersionSchemaInfoProvider;
			SchemaInfo SchemaInfo = _avroFooV2Schema.schemaInfo;
			_genericAvroSchema = new GenericAvroSchema(SchemaInfo);
		}

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