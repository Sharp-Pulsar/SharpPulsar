using SharpPulsar.Impl.Schema.Generic;
using SharpPulsar.Interfaces.ISchema;
using SharpPulsar.Schema;
using Xunit;
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
namespace Org.Apache.Pulsar.Client.Impl.Schema.Generic
{

	public class GenericAvroReaderTest
	{

		private SchemaTestUtils.Foo _foo;
		private SchemaTestUtils.FooV2 _fooV2;
		private AvroSchema<SchemaTestUtils.Foo> _fooSchemaNotNull;
		private AvroSchema<SchemaTestUtils.Foo> _fooSchema;
		private AvroSchema<SchemaTestUtils.FooV2> _fooV2Schema;
		private AvroSchema<SchemaTestUtils.Foo> _fooOffsetSchema;
		public virtual void Setup()
		{
			_fooSchema = AvroSchema<SchemaTestUtils.Foo>.Of(typeof(SchemaTestUtils.Foo));

			_fooV2Schema = AvroSchema<SchemaTestUtils.FooV2>.Of(typeof(SchemaTestUtils.FooV2));
			_fooSchemaNotNull = AvroSchema<SchemaTestUtils.Foo>.Of(ISchemaDefinition<SchemaTestUtils.Foo>.Builder().WithAlwaysAllowNull(false).WithPojo(typeof(SchemaTestUtils.Foo)).Build());

			_fooOffsetSchema = AvroSchema<SchemaTestUtils.Foo>.Of(typeof(SchemaTestUtils.Foo));
			_fooOffsetSchema.AvroSchema..AddProp(GenericAvroSchema.OFFSET_PROP, 5);

			_foo = new SchemaTestUtils.Foo();
			_foo.Field1 = "foo1";
			_foo.Field2 = "bar1";
			_foo.Field4 = new SchemaTestUtils.Bar();
			_foo.FieldUnableNull = "notNull";

			_fooV2 = new SchemaTestUtils.FooV2();
			_fooV2.Field1 = "foo1";
			_fooV2.Field3 = 10;
		}
		[Fact]
		public virtual void TestGenericAvroReaderByWriterSchema()
		{
			sbyte[] fooBytes = _fooSchema.Encode(_foo);

			var genericAvroSchemaByWriterSchema = new GenericAvroReader<SchemaTestUtils.Foo>(_fooSchema.AvroSchema);
			var genericRecordByWriterSchema = genericAvroSchemaByWriterSchema.Read((byte[])(object)fooBytes);
			Assert.Equal("foo1", genericRecordByWriterSchema.Field1);
			Assert.Equal("bar1", genericRecordByWriterSchema.Field2);
			Assert.Equal("notNull", genericRecordByWriterSchema.FieldUnableNull);
		}
		[Fact]
		public virtual void TestGenericAvroReaderByReaderSchema()
		{
			sbyte[] FooV2Bytes = _fooV2Schema.Encode(_fooV2);

			var genericAvroSchemaByReaderSchema = new GenericAvroReader<SchemaTestUtils.FooV2>(_fooV2Schema.AvroSchema, _fooSchemaNotNull.AvroSchema, new sbyte[10]);
			GenericRecord GenericRecordByReaderSchema = GenericAvroSchemaByReaderSchema.read(FooV2Bytes);
			assertEquals(GenericRecordByReaderSchema.getField("fieldUnableNull"), "defaultValue");
			assertEquals(GenericRecordByReaderSchema.getField("field1"), "foo1");
			assertEquals(GenericRecordByReaderSchema.getField("field3"), 10);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testOffsetSchema()
		public virtual void TestOffsetSchema()
		{
			sbyte[] FooBytes = _fooOffsetSchema.encode(_foo);
			ByteBuf ByteBuf = Unpooled.buffer();
			ByteBuf.writeByte(0);
			ByteBuf.writeInt(10);
			ByteBuf.writeBytes(FooBytes);

			GenericAvroReader Reader = new GenericAvroReader(_fooOffsetSchema.AvroSchema);
			assertEquals(Reader.Offset, 5);
			GenericRecord Record = Reader.read(ByteBuf.array());
			assertEquals(Record.getField("field1"), "foo1");
			assertEquals(Record.getField("field2"), "bar1");
			assertEquals(Record.getField("fieldUnableNull"), "notNull");
		}

	}

}