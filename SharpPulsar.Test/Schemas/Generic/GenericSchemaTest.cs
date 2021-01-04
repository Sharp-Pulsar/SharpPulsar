using System.Collections.Generic;

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
	using Lists = com.google.common.collect.Lists;
	using Slf4j = lombok.@extern.slf4j.Slf4j;
	using Schema = org.apache.pulsar.client.api.Schema;
	using GenericRecord = org.apache.pulsar.client.api.schema.GenericRecord;
	using GenericSchema = org.apache.pulsar.client.api.schema.GenericSchema;
	using AutoConsumeSchema = Org.Apache.Pulsar.Client.Impl.Schema.AutoConsumeSchema;
	using KeyValueSchema = Org.Apache.Pulsar.Client.Impl.Schema.KeyValueSchema;
	using KeyValueSchemaInfo = Org.Apache.Pulsar.Client.Impl.Schema.KeyValueSchemaInfo;
	using Bar = Org.Apache.Pulsar.Client.Impl.Schema.SchemaTestUtils.Bar;
	using Foo = Org.Apache.Pulsar.Client.Impl.Schema.SchemaTestUtils.Foo;
	using KeyValue = org.apache.pulsar.common.schema.KeyValue;
	using KeyValueEncodingType = org.apache.pulsar.common.schema.KeyValueEncodingType;
	using Test = org.testng.annotations.Test;


//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.*;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.testng.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.testng.Assert.assertTrue;

	/// <summary>
	/// Unit testing generic schemas.
	/// this test is duplicated with GenericSchemaImplTest independent of GenericSchemaImpl
	/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Slf4j public class GenericSchemaTest
	public class GenericSchemaTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testGenericAvroSchema()
		public virtual void TestGenericAvroSchema()
		{
			Schema<Foo> EncodeSchema = Schema.AVRO(typeof(Foo));
			GenericSchema DecodeSchema = GenericAvroSchema.of(EncodeSchema.SchemaInfo);
			TestEncodeAndDecodeGenericRecord(EncodeSchema, DecodeSchema);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testGenericJsonSchema()
		public virtual void TestGenericJsonSchema()
		{
			Schema<Foo> EncodeSchema = Schema.JSON(typeof(Foo));
			GenericSchema DecodeSchema = GenericJsonSchema.of(EncodeSchema.SchemaInfo);
			TestEncodeAndDecodeGenericRecord(EncodeSchema, DecodeSchema);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testAutoAvroSchema()
		public virtual void TestAutoAvroSchema()
		{
			// configure encode schema
			Schema<Foo> EncodeSchema = Schema.AVRO(typeof(Foo));

			// configure the schema info provider
			MultiVersionSchemaInfoProvider MultiVersionGenericSchemaProvider = mock(typeof(MultiVersionSchemaInfoProvider));
			when(MultiVersionGenericSchemaProvider.getSchemaByVersion(any(typeof(sbyte[])))).thenReturn(CompletableFuture.completedFuture(EncodeSchema.SchemaInfo));

			// configure decode schema
			AutoConsumeSchema DecodeSchema = new AutoConsumeSchema();
			DecodeSchema.configureSchemaInfo("test-topic", "topic", EncodeSchema.SchemaInfo);
			DecodeSchema.SchemaInfoProvider = MultiVersionGenericSchemaProvider;

			TestEncodeAndDecodeGenericRecord(EncodeSchema, DecodeSchema);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testAutoJsonSchema()
		public virtual void TestAutoJsonSchema()
		{
			// configure the schema info provider
			MultiVersionSchemaInfoProvider MultiVersionSchemaInfoProvider = mock(typeof(MultiVersionSchemaInfoProvider));
			GenericSchema GenericAvroSchema = GenericAvroSchema.of(Schema.AVRO(typeof(Foo)).SchemaInfo);
			when(MultiVersionSchemaInfoProvider.getSchemaByVersion(any(typeof(sbyte[])))).thenReturn(CompletableFuture.completedFuture(GenericAvroSchema.SchemaInfo));

			// configure encode schema
			Schema<Foo> EncodeSchema = Schema.JSON(typeof(Foo));

			// configure decode schema
			AutoConsumeSchema DecodeSchema = new AutoConsumeSchema();
			DecodeSchema.configureSchemaInfo("test-topic", "topic", EncodeSchema.SchemaInfo);
			DecodeSchema.SchemaInfoProvider = MultiVersionSchemaInfoProvider;

			TestEncodeAndDecodeGenericRecord(EncodeSchema, DecodeSchema);
		}

		private void TestEncodeAndDecodeGenericRecord(Schema<Foo> EncodeSchema, Schema<GenericRecord> DecodeSchema)
		{
			int NumRecords = 10;
			for (int I = 0; I < NumRecords; I++)
			{
				Foo Foo = NewFoo(I);
				sbyte[] Data = EncodeSchema.encode(Foo);

				log.info("Decoding : {}", StringHelper.NewString(Data, UTF_8));

				GenericRecord Record;
				if (DecodeSchema is AutoConsumeSchema)
				{
					Record = DecodeSchema.decode(Data, new sbyte[0]);
				}
				else
				{
					Record = DecodeSchema.decode(Data);
				}
				VerifyFooRecord(Record, I);
			}
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testKeyValueSchema()
		public virtual void TestKeyValueSchema()
		{
			// configure the schema info provider
			MultiVersionSchemaInfoProvider MultiVersionSchemaInfoProvider = mock(typeof(MultiVersionSchemaInfoProvider));
			GenericSchema GenericAvroSchema = GenericAvroSchema.of(Schema.AVRO(typeof(Foo)).SchemaInfo);
			when(MultiVersionSchemaInfoProvider.getSchemaByVersion(any(typeof(sbyte[])))).thenReturn(CompletableFuture.completedFuture(KeyValueSchemaInfo.encodeKeyValueSchemaInfo(GenericAvroSchema, GenericAvroSchema, KeyValueEncodingType.INLINE)));

			IList<Schema<Foo>> EncodeSchemas = Lists.newArrayList(Schema.JSON(typeof(Foo)), Schema.AVRO(typeof(Foo)));

			foreach (Schema<Foo> KeySchema in EncodeSchemas)
			{
				foreach (Schema<Foo> ValueSchema in EncodeSchemas)
				{
					// configure encode schema
					Schema<KeyValue<Foo, Foo>> KvSchema = KeyValueSchema.of(KeySchema, ValueSchema);

					// configure decode schema
					Schema<KeyValue<GenericRecord, GenericRecord>> DecodeSchema = KeyValueSchema.of(Schema.AUTO_CONSUME(), Schema.AUTO_CONSUME());
					DecodeSchema.configureSchemaInfo("test-topic", "topic",KvSchema.SchemaInfo);
					DecodeSchema.SchemaInfoProvider = MultiVersionSchemaInfoProvider;

					TestEncodeAndDecodeKeyValues(KvSchema, DecodeSchema);
				}
			}

		}

		private void TestEncodeAndDecodeKeyValues(Schema<KeyValue<Foo, Foo>> EncodeSchema, Schema<KeyValue<GenericRecord, GenericRecord>> DecodeSchema)
		{
			int NumRecords = 10;
			for (int I = 0; I < NumRecords; I++)
			{
				Foo Foo = NewFoo(I);
				sbyte[] Data = EncodeSchema.encode(new KeyValue<>(Foo, Foo));

				KeyValue<GenericRecord, GenericRecord> Kv = DecodeSchema.decode(Data, new sbyte[0]);
				VerifyFooRecord(Kv.Key, I);
				VerifyFooRecord(Kv.Value, I);
			}
		}

		private static Foo NewFoo(int I)
		{
			Foo Foo = new Foo();
			Foo.Field1 = "field-1-" + I;
			Foo.Field2 = "field-2-" + I;
			Foo.Field3 = I;
			Bar Bar = new Bar();
			Bar.Field1 = I % 2 == 0;
			Foo.Field4 = Bar;
			Foo.FieldUnableNull = "fieldUnableNull-1-" + I;

			return Foo;
		}

		private static void VerifyFooRecord(GenericRecord Record, int I)
		{
			object Field1 = Record.getField("field1");
			assertEquals("field-1-" + I, Field1, "Field 1 is " + Field1.GetType());
			object Field2 = Record.getField("field2");
			assertEquals("field-2-" + I, Field2, "Field 2 is " + Field2.GetType());
			object Field3 = Record.getField("field3");
			assertEquals(I, Field3, "Field 3 is " + Field3.GetType());
			object Field4 = Record.getField("field4");
			assertTrue(Field4 is GenericRecord);
			GenericRecord Field4Record = (GenericRecord) Field4;
			assertEquals(I % 2 == 0, Field4Record.getField("field1"));
			object FieldUnableNull = Record.getField("fieldUnableNull");
			assertEquals("fieldUnableNull-1-" + I, FieldUnableNull, "fieldUnableNull 1 is " + FieldUnableNull.GetType());
		}

	}

}