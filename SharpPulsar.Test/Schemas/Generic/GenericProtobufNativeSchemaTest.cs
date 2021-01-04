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
	using GenericRecord = org.apache.pulsar.client.api.schema.GenericRecord;
	using SchemaDefinition = org.apache.pulsar.client.api.schema.SchemaDefinition;
	using ProtobufNativeSchema = Org.Apache.Pulsar.Client.Impl.Schema.ProtobufNativeSchema;
	using TestMessage = org.apache.pulsar.client.schema.proto.Test.TestMessage;
	using BeforeMethod = org.testng.annotations.BeforeMethod;
	using Test = org.testng.annotations.Test;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.testng.Assert.assertEquals;

	public class GenericProtobufNativeSchemaTest
	{

		private TestMessage _message;
		private GenericRecord _genericmessage;
		private GenericProtobufNativeSchema _genericProtobufNativeSchema;
		private ProtobufNativeSchema<TestMessage> _clazzBasedProtobufNativeSchema;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @BeforeMethod public void init()
		public virtual void Init()
		{
			_clazzBasedProtobufNativeSchema = ProtobufNativeSchema.of(SchemaDefinition.builder<TestMessage>().withPojo(typeof(TestMessage)).build());
			_genericProtobufNativeSchema = (GenericProtobufNativeSchema) GenericProtobufNativeSchema.of(_clazzBasedProtobufNativeSchema.SchemaInfo);

		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testGenericReaderByClazzBasedWriterSchema()
		public virtual void TestGenericReaderByClazzBasedWriterSchema()
		{
			_message = TestMessage.newBuilder().setStringField(STRING_FIELD_VLUE).setDoubleField(DOUBLE_FIELD_VLUE).build();
			sbyte[] ClazzBasedProtobufBytes = _clazzBasedProtobufNativeSchema.encode(_message);
			GenericRecord GenericRecord = _genericProtobufNativeSchema.decode(ClazzBasedProtobufBytes);
			assertEquals(GenericRecord.getField("stringField"), STRING_FIELD_VLUE);
			assertEquals(GenericRecord.getField("doubleField"), DOUBLE_FIELD_VLUE);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testClazzBasedReaderByClazzGenericWriterSchema()
		public virtual void TestClazzBasedReaderByClazzGenericWriterSchema()
		{
			_genericmessage = _genericProtobufNativeSchema.newRecordBuilder().set("stringField", STRING_FIELD_VLUE).set("doubleField", DOUBLE_FIELD_VLUE).build();
			sbyte[] MessageBytes = _genericProtobufNativeSchema.encode(_genericmessage);
			_message = _clazzBasedProtobufNativeSchema.decode(MessageBytes);
			assertEquals(_message.StringField, STRING_FIELD_VLUE);
			assertEquals(_message.DoubleField, DOUBLE_FIELD_VLUE);
		}

		private const string STRING_FIELD_VLUE = "stringFieldValue";
		private const double DOUBLE_FIELD_VLUE = 0.2D;

	}

}