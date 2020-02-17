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
namespace SharpPulsar.Test.Impl.schema
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.testng.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.testng.Assert.assertNull;

	using ByteBuf = io.netty.buffer.ByteBuf;

    /// <summary>
	/// Unit tests primitive schemas.
	/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Slf4j public class PrimitiveSchemaTest
	public class PrimitiveSchemaTest
	{

		private static readonly IDictionary<Schema, IList<object>> testData = new HashMapAnonymousInnerClass();

		public class HashMapAnonymousInnerClass : Hashtable
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

		public class HashMapAnonymousInnerClass2 : Hashtable
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
		public virtual object[][] Schemas()
		{
			return new object[][]
			{
				new object[] {testData},
				new object[] {testData2}
			};
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(dataProvider = "schemas") public void allSchemasShouldSupportNull(java.util.Map<org.apache.pulsar.client.api.Schema, java.util.List<Object>> testData)
		public virtual void AllSchemasShouldSupportNull(IDictionary<Schema, IList<object>> TestData)
		{
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: for (org.apache.pulsar.client.api.Schema<?> schema : testData.keySet())
			foreach (Schema<object> Schema in TestData.Keys)
			{
				sbyte[] Bytes = null;
				ByteBuf ByteBuf = null;
				try
				{
					assertNull(Schema.encode(null), "Should support null in " + Schema.SchemaInfo.Name + " serialization");
					assertNull(Schema.decode(Bytes), "Should support null in " + Schema.SchemaInfo.Name + " deserialization");
					assertNull(((AbstractSchema) Schema).decode(ByteBuf), "Should support null in " + Schema.SchemaInfo.Name + " deserialization");
				}
				catch (System.NullReferenceException Npe)
				{
					throw new System.NullReferenceException("NPE when using schema " + Schema + " : " + Npe.Message);
				}
			}
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(dataProvider = "schemas") public void allSchemasShouldRoundtripInput(java.util.Map<org.apache.pulsar.client.api.Schema, java.util.List<Object>> testData)
		public virtual void AllSchemasShouldRoundtripInput(IDictionary<Schema, IList<object>> TestData)
		{
			foreach (KeyValuePair<Schema, IList<object>> Test in TestData.SetOfKeyValuePairs())
			{
				log.info("Test schema {}", Test.Key);
				foreach (object Value in Test.Value)
				{
					log.info("Encode : {}", Value);
					try
					{
						assertEquals(Value, Test.Key.decode(Test.Key.encode(Value)), "Should get the original " + Test.Key.SchemaInfo.Name + " after serialization and deserialization");
					}
					catch (System.NullReferenceException Npe)
					{
						throw new System.NullReferenceException("NPE when using schema " + Test.Key + " : " + Npe.Message);
					}
				}
			}
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void allSchemasShouldHaveSchemaType()
		public virtual void AllSchemasShouldHaveSchemaType()
		{
			assertEquals(SchemaType.BOOLEAN, BooleanSchema.Of().SchemaInfo.Type);
			assertEquals(SchemaType.INT8, ByteSchema.Of().SchemaInfo.Type);
			assertEquals(SchemaType.INT16, ShortSchema.Of().SchemaInfo.Type);
			assertEquals(SchemaType.INT32, IntSchema.Of().SchemaInfo.Type);
			assertEquals(SchemaType.INT64, LongSchema.Of().SchemaInfo.Type);
			assertEquals(SchemaType.FLOAT, FloatSchema.Of().SchemaInfo.Type);
			assertEquals(SchemaType.DOUBLE, DoubleSchema.Of().SchemaInfo.Type);
			assertEquals(SchemaType.STRING, StringSchema.Utf8().SchemaInfo.Type);
			assertEquals(SchemaType.BYTES, BytesSchema.Of().SchemaInfo.Type);
			assertEquals(SchemaType.BYTES, ByteBufferSchema.Of().SchemaInfo.Type);
			assertEquals(SchemaType.BYTES, ByteBufSchema.Of().SchemaInfo.Type);
			assertEquals(SchemaType.DATE, DateSchema.Of().SchemaInfo.Type);
			assertEquals(SchemaType.TIME, TimeSchema.Of().SchemaInfo.Type);
			assertEquals(SchemaType.TIMESTAMP, TimestampSchema.Of().SchemaInfo.Type);
		}


	}

}