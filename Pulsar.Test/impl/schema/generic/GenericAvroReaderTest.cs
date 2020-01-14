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
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.testng.Assert.assertEquals;

	using Slf4j = lombok.@extern.slf4j.Slf4j;
	using GenericRecord = org.apache.pulsar.client.api.schema.GenericRecord;
	using SchemaDefinition = org.apache.pulsar.client.api.schema.SchemaDefinition;
	using org.apache.pulsar.client.impl.schema;
	using Foo = org.apache.pulsar.client.impl.schema.SchemaTestUtils.Foo;
	using FooV2 = org.apache.pulsar.client.impl.schema.SchemaTestUtils.FooV2;
	using BeforeMethod = org.testng.annotations.BeforeMethod;
	using Test = org.testng.annotations.Test;

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
		public virtual void setup()
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
		public virtual void testGenericAvroReaderByWriterSchema()
		{
			sbyte[] fooBytes = fooSchema.encode(foo);

			GenericAvroReader genericAvroSchemaByWriterSchema = new GenericAvroReader(fooSchema.AvroSchema);
			GenericRecord genericRecordByWriterSchema = genericAvroSchemaByWriterSchema.read(fooBytes);
			assertEquals(genericRecordByWriterSchema.getField("field1"), "foo1");
			assertEquals(genericRecordByWriterSchema.getField("field2"), "bar1");
			assertEquals(genericRecordByWriterSchema.getField("fieldUnableNull"), "notNull");
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testGenericAvroReaderByReaderSchema()
		public virtual void testGenericAvroReaderByReaderSchema()
		{
			sbyte[] fooV2Bytes = fooV2Schema.encode(fooV2);

			GenericAvroReader genericAvroSchemaByReaderSchema = new GenericAvroReader(fooV2Schema.AvroSchema, fooSchemaNotNull.AvroSchema, new sbyte[10]);
			GenericRecord genericRecordByReaderSchema = genericAvroSchemaByReaderSchema.read(fooV2Bytes);
			assertEquals(genericRecordByReaderSchema.getField("fieldUnableNull"), "defaultValue");
			assertEquals(genericRecordByReaderSchema.getField("field1"), "foo1");
			assertEquals(genericRecordByReaderSchema.getField("field3"), 10);
		}

	}

}