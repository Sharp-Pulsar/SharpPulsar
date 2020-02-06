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
namespace org.apache.pulsar.client.impl.schema.generic
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.testng.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.testng.Assert.assertTrue;

	using Lists = com.google.common.collect.Lists;
	using Slf4j = lombok.@extern.slf4j.Slf4j;
	using Schema = api.Schema;
	using GenericRecord = api.schema.GenericRecord;
	using GenericSchema = api.schema.GenericSchema;
	using org.apache.pulsar.client.impl.schema;
	using Bar = SchemaTestUtils.Bar;
	using Foo = SchemaTestUtils.Foo;
	using KeyValue = common.schema.KeyValue;
	using KeyValueEncodingType = common.schema.KeyValueEncodingType;
	using Test = org.testng.annotations.Test;

	/// <summary>
	/// Unit testing generic schemas.
	/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Slf4j public class GenericSchemaImplTest
	public class GenericSchemaImplTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testGenericAvroSchema()
		public virtual void testGenericAvroSchema()
		{
			Schema<Foo> encodeSchema = Schema.AVRO(typeof(Foo));
			GenericSchema decodeSchema = GenericSchemaImpl.of(encodeSchema.SchemaInfo);
			testEncodeAndDecodeGenericRecord(encodeSchema, decodeSchema);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testGenericJsonSchema()
		public virtual void testGenericJsonSchema()
		{
			Schema<Foo> encodeSchema = Schema.JSON(typeof(Foo));
			GenericSchema decodeSchema = GenericSchemaImpl.of(encodeSchema.SchemaInfo);
			testEncodeAndDecodeGenericRecord(encodeSchema, decodeSchema);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testAutoAvroSchema()
		public virtual void testAutoAvroSchema()
		{
			// configure encode schema
			Schema<Foo> encodeSchema = Schema.AVRO(typeof(Foo));

			// configure the schema info provider
			MultiVersionSchemaInfoProvider multiVersionGenericSchemaProvider = mock(typeof(MultiVersionSchemaInfoProvider));
			when(multiVersionGenericSchemaProvider.getSchemaByVersion(any(typeof(sbyte[])))).thenReturn(CompletableFuture.completedFuture(encodeSchema.SchemaInfo));

			// configure decode schema
			AutoConsumeSchema decodeSchema = new AutoConsumeSchema();
			decodeSchema.configureSchemaInfo("test-topic", "topic", encodeSchema.SchemaInfo);
			decodeSchema.SchemaInfoProvider = multiVersionGenericSchemaProvider;

			testEncodeAndDecodeGenericRecord(encodeSchema, decodeSchema);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testAutoJsonSchema()
		public virtual void testAutoJsonSchema()
		{
			// configure the schema info provider
			MultiVersionSchemaInfoProvider multiVersionSchemaInfoProvider = mock(typeof(MultiVersionSchemaInfoProvider));
			GenericSchema genericAvroSchema = GenericSchemaImpl.of(Schema.AVRO(typeof(Foo)).SchemaInfo);
			when(multiVersionSchemaInfoProvider.getSchemaByVersion(any(typeof(sbyte[])))).thenReturn(CompletableFuture.completedFuture(genericAvroSchema.SchemaInfo));

			// configure encode schema
			Schema<Foo> encodeSchema = Schema.JSON(typeof(Foo));

			// configure decode schema
			AutoConsumeSchema decodeSchema = new AutoConsumeSchema();
			decodeSchema.configureSchemaInfo("test-topic", "topic", encodeSchema.SchemaInfo);
			decodeSchema.SchemaInfoProvider = multiVersionSchemaInfoProvider;

			testEncodeAndDecodeGenericRecord(encodeSchema, decodeSchema);
		}

		private void testEncodeAndDecodeGenericRecord(Schema<Foo> encodeSchema, Schema<GenericRecord> decodeSchema)
		{
			int numRecords = 10;
			for (int i = 0; i < numRecords; i++)
			{
				Foo foo = newFoo(i);
				sbyte[] data = encodeSchema.encode(foo);

				log.info("Decoding : {}", StringHelper.NewString(data, UTF_8));

				GenericRecord record;
				if (decodeSchema is AutoConsumeSchema)
				{
					record = decodeSchema.decode(data, new sbyte[0]);
				}
				else
				{
					record = decodeSchema.decode(data);
				}
				verifyFooRecord(record, i);
			}
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testKeyValueSchema()
		public virtual void testKeyValueSchema()
		{
			// configure the schema info provider
			MultiVersionSchemaInfoProvider multiVersionSchemaInfoProvider = mock(typeof(MultiVersionSchemaInfoProvider));
			GenericSchema genericAvroSchema = GenericSchemaImpl.of(Schema.AVRO(typeof(Foo)).SchemaInfo);
			when(multiVersionSchemaInfoProvider.getSchemaByVersion(any(typeof(sbyte[])))).thenReturn(CompletableFuture.completedFuture(KeyValueSchemaInfo.encodeKeyValueSchemaInfo(genericAvroSchema, genericAvroSchema, KeyValueEncodingType.INLINE)));

			IList<Schema<Foo>> encodeSchemas = Lists.newArrayList(Schema.JSON(typeof(Foo)), Schema.AVRO(typeof(Foo)));

			foreach (Schema<Foo> keySchema in encodeSchemas)
			{
				foreach (Schema<Foo> valueSchema in encodeSchemas)
				{
					// configure encode schema
					Schema<KeyValue<Foo, Foo>> kvSchema = KeyValueSchema.of(keySchema, valueSchema);

					// configure decode schema
					Schema<KeyValue<GenericRecord, GenericRecord>> decodeSchema = KeyValueSchema.of(Schema.AUTO_CONSUME(), Schema.AUTO_CONSUME());
					decodeSchema.configureSchemaInfo("test-topic", "topic",kvSchema.SchemaInfo);
					decodeSchema.SchemaInfoProvider = multiVersionSchemaInfoProvider;

					testEncodeAndDecodeKeyValues(kvSchema, decodeSchema);
				}
			}

		}

		private void testEncodeAndDecodeKeyValues(Schema<KeyValue<Foo, Foo>> encodeSchema, Schema<KeyValue<GenericRecord, GenericRecord>> decodeSchema)
		{
			int numRecords = 10;
			for (int i = 0; i < numRecords; i++)
			{
				Foo foo = newFoo(i);
				sbyte[] data = encodeSchema.encode(new KeyValue<>(foo, foo));

				KeyValue<GenericRecord, GenericRecord> kv = decodeSchema.decode(data, new sbyte[0]);
				verifyFooRecord(kv.Key, i);
				verifyFooRecord(kv.Value, i);
			}
		}

		private static Foo newFoo(int i)
		{
			Foo foo = new Foo();
			foo.Field1 = "field-1-" + i;
			foo.Field2 = "field-2-" + i;
			foo.Field3 = i;
			Bar bar = new Bar();
			bar.Field1 = i % 2 == 0;
			foo.Field4 = bar;
			foo.FieldUnableNull = "fieldUnableNull-1-" + i;

			return foo;
		}

		private static void verifyFooRecord(GenericRecord record, int i)
		{
			object field1 = record.getField("field1");
			assertEquals("field-1-" + i, field1, "Field 1 is " + field1.GetType());
			object field2 = record.getField("field2");
			assertEquals("field-2-" + i, field2, "Field 2 is " + field2.GetType());
			object field3 = record.getField("field3");
			assertEquals(i, field3, "Field 3 is " + field3.GetType());
			object field4 = record.getField("field4");
			assertTrue(field4 is GenericRecord);
			GenericRecord field4Record = (GenericRecord) field4;
			assertEquals(i % 2 == 0, field4Record.getField("field1"));
			object fieldUnableNull = record.getField("fieldUnableNull");
			assertEquals("fieldUnableNull-1-" + i, fieldUnableNull, "fieldUnableNull 1 is " + fieldUnableNull.GetType());
		}

	}

}