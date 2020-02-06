using System.Collections;
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
namespace org.apache.pulsar.client.impl.schema
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.testng.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.testng.Assert.assertNull;

	using ByteBuf = io.netty.buffer.ByteBuf;
	using Unpooled = io.netty.buffer.Unpooled;
	using Slf4j = lombok.@extern.slf4j.Slf4j;
	using Schema = api.Schema;
	using SchemaType = common.schema.SchemaType;
	using DataProvider = org.testng.annotations.DataProvider;
	using Test = org.testng.annotations.Test;

	/// <summary>
	/// Unit tests primitive schemas.
	/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Slf4j public class PrimitiveSchemaTest
	public class PrimitiveSchemaTest
	{

		private static readonly IDictionary<Schema, IList<object>> testData = new HashMapAnonymousInnerClass();

		private class HashMapAnonymousInnerClass : Hashtable
		{
	//		{
	//			put(BooleanSchema.of(), Arrays.asList(false, true));
	//			put(StringSchema.utf8(), Arrays.asList("my string"));
	//			put(ByteSchema.of(), Arrays.asList((byte) 32767, (byte) -32768));
	//			put(ShortSchema.of(), Arrays.asList((short) 32767, (short) -32768));
	//			put(IntSchema.of(), Arrays.asList((int) 423412424, (int) -41243432));
	//			put(LongSchema.of(), Arrays.asList(922337203685477580L, -922337203685477581L));
	//			put(FloatSchema.of(), Arrays.asList(5678567.12312f, -5678567.12341f));
	//			put(DoubleSchema.of(), Arrays.asList(5678567.12312d, -5678567.12341d));
	//			put(BytesSchema.of(), Arrays.asList("my string".getBytes(UTF_8)));
	//			put(ByteBufferSchema.of(), Arrays.asList(ByteBuffer.allocate(10).put("my string".getBytes(UTF_8))));
	//			put(ByteBufSchema.of(), Arrays.asList(Unpooled.wrappedBuffer("my string".getBytes(UTF_8))));
	//			put(DateSchema.of(), Arrays.asList(new Date(new java.util.Date().getTime() - 10000), new Date(new java.util.Date().getTime())));
	//			put(TimeSchema.of(), Arrays.asList(new Time(new java.util.Date().getTime() - 10000), new Time(new java.util.Date().getTime())));
	//			put(TimestampSchema.of(), Arrays.asList(new Timestamp(new java.util.Date().getTime()), new Timestamp(new java.util.Date().getTime())));
	//		}
		}

		private static readonly IDictionary<Schema, IList<object>> testData2 = new HashMapAnonymousInnerClass2();

		private class HashMapAnonymousInnerClass2 : Hashtable
		{
	//		{
	//			put(Schema.BOOL, Arrays.asList(false, true));
	//			put(Schema.STRING, Arrays.asList("my string"));
	//			put(Schema.INT8, Arrays.asList((byte) 32767, (byte) -32768));
	//			put(Schema.INT16, Arrays.asList((short) 32767, (short) -32768));
	//			put(Schema.INT32, Arrays.asList((int) 423412424, (int) -41243432));
	//			put(Schema.INT64, Arrays.asList(922337203685477580L, -922337203685477581L));
	//			put(Schema.FLOAT, Arrays.asList(5678567.12312f, -5678567.12341f));
	//			put(Schema.DOUBLE, Arrays.asList(5678567.12312d, -5678567.12341d));
	//			put(Schema.BYTES, Arrays.asList("my string".getBytes(UTF_8)));
	//			put(Schema.BYTEBUFFER, Arrays.asList(ByteBuffer.allocate(10).put("my string".getBytes(UTF_8))));
	//			put(Schema.DATE, Arrays.asList(new Date(new java.util.Date().getTime() - 10000), new Date(new java.util.Date().getTime())));
	//			put(Schema.TIME, Arrays.asList(new Time(new java.util.Date().getTime() - 10000), new Time(new java.util.Date().getTime())));
	//			put(Schema.TIMESTAMP, Arrays.asList(new Timestamp(new java.util.Date().getTime() - 10000), new Timestamp(new java.util.Date().getTime())));
	//		}
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @DataProvider(name = "schemas") public Object[][] schemas()
		public virtual object[][] schemas()
		{
			return new object[][]
			{
				new object[] {testData},
				new object[] {testData2}
			};
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(dataProvider = "schemas") public void allSchemasShouldSupportNull(java.util.Map<org.apache.pulsar.client.api.Schema, java.util.List<Object>> testData)
		public virtual void allSchemasShouldSupportNull(IDictionary<Schema, IList<object>> testData)
		{
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: for (org.apache.pulsar.client.api.Schema<?> schema : testData.keySet())
			foreach (Schema<object> schema in testData.Keys)
			{
				sbyte[] bytes = null;
				ByteBuf byteBuf = null;
				try
				{
					assertNull(schema.encode(null), "Should support null in " + schema.SchemaInfo.Name + " serialization");
					assertNull(schema.decode(bytes), "Should support null in " + schema.SchemaInfo.Name + " deserialization");
					assertNull(((AbstractSchema) schema).decode(byteBuf), "Should support null in " + schema.SchemaInfo.Name + " deserialization");
				}
				catch (System.NullReferenceException npe)
				{
					throw new System.NullReferenceException("NPE when using schema " + schema + " : " + npe.Message);
				}
			}
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(dataProvider = "schemas") public void allSchemasShouldRoundtripInput(java.util.Map<org.apache.pulsar.client.api.Schema, java.util.List<Object>> testData)
		public virtual void allSchemasShouldRoundtripInput(IDictionary<Schema, IList<object>> testData)
		{
			foreach (KeyValuePair<Schema, IList<object>> test in testData.SetOfKeyValuePairs())
			{
				log.info("Test schema {}", test.Key);
				foreach (object value in test.Value)
				{
					log.info("Encode : {}", value);
					try
					{
						assertEquals(value, test.Key.decode(test.Key.encode(value)), "Should get the original " + test.Key.SchemaInfo.Name + " after serialization and deserialization");
					}
					catch (System.NullReferenceException npe)
					{
						throw new System.NullReferenceException("NPE when using schema " + test.Key + " : " + npe.Message);
					}
				}
			}
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void allSchemasShouldHaveSchemaType()
		public virtual void allSchemasShouldHaveSchemaType()
		{
			assertEquals(SchemaType.BOOLEAN, BooleanSchema.of().SchemaInfo.Type);
			assertEquals(SchemaType.INT8, ByteSchema.of().SchemaInfo.Type);
			assertEquals(SchemaType.INT16, ShortSchema.of().SchemaInfo.Type);
			assertEquals(SchemaType.INT32, IntSchema.of().SchemaInfo.Type);
			assertEquals(SchemaType.INT64, LongSchema.of().SchemaInfo.Type);
			assertEquals(SchemaType.FLOAT, FloatSchema.of().SchemaInfo.Type);
			assertEquals(SchemaType.DOUBLE, DoubleSchema.of().SchemaInfo.Type);
			assertEquals(SchemaType.STRING, StringSchema.utf8().SchemaInfo.Type);
			assertEquals(SchemaType.BYTES, BytesSchema.of().SchemaInfo.Type);
			assertEquals(SchemaType.BYTES, ByteBufferSchema.of().SchemaInfo.Type);
			assertEquals(SchemaType.BYTES, ByteBufSchema.of().SchemaInfo.Type);
			assertEquals(SchemaType.DATE, DateSchema.of().SchemaInfo.Type);
			assertEquals(SchemaType.TIME, TimeSchema.of().SchemaInfo.Type);
			assertEquals(SchemaType.TIMESTAMP, TimestampSchema.of().SchemaInfo.Type);
		}


	}

}