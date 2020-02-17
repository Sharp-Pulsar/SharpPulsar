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
namespace SharpPulsar.Test.Impl.schema.generic
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.testng.Assert.assertEquals;

using GenericRecord = Org.Apache.Pulsar.Client.Api.Schema.GenericRecord;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Slf4j public class GenericAvroReaderTest
	public class GenericAvroReaderTest
	{

		private SchemaTestUtils.Foo foo;
		private SchemaTestUtils.FooV2 fooV2;
		private AvroSchema fooSchemaNotNull;
		private AvroSchema fooSchema;
		private AvroSchema fooV2Schema;


//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @BeforeMethod public void setup()
		public virtual void Setup()
		{
			fooSchema = AvroSchema.of(typeof(SchemaTestUtils.Foo));
			fooV2Schema = AvroSchema.of(typeof(SchemaTestUtils.FooV2));
			fooSchemaNotNull = AvroSchema.of(SchemaDefinition.builder().withAlwaysAllowNull(false).withPojo(typeof(SchemaTestUtils.Foo)).build());

			foo = new SchemaTestUtils.Foo();
			foo.Field1 = "foo1";
			foo.Field2 = "bar1";
			foo.Field4 = new SchemaTestUtils.Bar();
			foo.FieldUnableNull = "notNull";

			fooV2 = new SchemaTestUtils.FooV2();
			fooV2.Field1 = "foo1";
			fooV2.Field3 = 10;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testGenericAvroReaderByWriterSchema()
		public virtual void TestGenericAvroReaderByWriterSchema()
		{
			sbyte[] FooBytes = fooSchema.encode(foo);

			GenericAvroReader GenericAvroSchemaByWriterSchema = new GenericAvroReader(fooSchema.AvroSchema);
			GenericRecord GenericRecordByWriterSchema = GenericAvroSchemaByWriterSchema.read(FooBytes);
			assertEquals(GenericRecordByWriterSchema.getField("field1"), "foo1");
			assertEquals(GenericRecordByWriterSchema.getField("field2"), "bar1");
			assertEquals(GenericRecordByWriterSchema.getField("fieldUnableNull"), "notNull");
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testGenericAvroReaderByReaderSchema()
		public virtual void TestGenericAvroReaderByReaderSchema()
		{
			sbyte[] FooV2Bytes = fooV2Schema.encode(fooV2);

			GenericAvroReader GenericAvroSchemaByReaderSchema = new GenericAvroReader(fooV2Schema.AvroSchema, fooSchemaNotNull.AvroSchema, new sbyte[10]);
			GenericRecord GenericRecordByReaderSchema = GenericAvroSchemaByReaderSchema.read(FooV2Bytes);
			assertEquals(GenericRecordByReaderSchema.getField("fieldUnableNull"), "defaultValue");
			assertEquals(GenericRecordByReaderSchema.getField("field1"), "foo1");
			assertEquals(GenericRecordByReaderSchema.getField("field3"), 10);
		}

	}

}