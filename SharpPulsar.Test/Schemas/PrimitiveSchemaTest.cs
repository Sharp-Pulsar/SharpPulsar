using System;
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
namespace SharpPulsar.Test.Schema
{

	/// <summary>
	/// Unit tests primitive schemas.
	/// </summary>
	public class PrimitiveSchemaTest
	{

		private static readonly IDictionary<Schema, IList<object>> _testData = new HashMapAnonymousInnerClass();

		private class HashMapAnonymousInnerClass : Hashtable
		{
			public HashMapAnonymousInnerClass()
			{

				this.put(BooleanSchema.of(), Arrays.asList(false, true));
				this.put(StringSchema.utf8(), Arrays.asList("my string"));
				this.put(ByteSchema.of(), Arrays.asList(unchecked((sbyte) 32767), unchecked((sbyte) -32768)));
				this.put(ShortSchema.of(), Arrays.asList((short) 32767, (short) -32768));
				this.put(IntSchema.of(), Arrays.asList((int) 423412424, (int) -41243432));
				this.put(LongSchema.of(), Arrays.asList(922337203685477580L, -922337203685477581L));
				this.put(FloatSchema.of(), Arrays.asList(5678567.12312f, -5678567.12341f));
				this.put(DoubleSchema.of(), Arrays.asList(5678567.12312d, -5678567.12341d));
				this.put(BytesSchema.of(), Arrays.asList("my string".GetBytes(UTF_8)));
				this.put(ByteBufferSchema.of(), Arrays.asList(ByteBuffer.allocate(10).put("my string".GetBytes(UTF_8))));
				this.put(ByteBufSchema.of(), Arrays.asList(Unpooled.wrappedBuffer("my string".GetBytes(UTF_8))));
				this.put(DateSchema.of(), Arrays.asList(new Date((DateTime.Now).Ticks - 10000), new Date((DateTime.Now).Ticks)));
				this.put(TimeSchema.of(), Arrays.asList(new Time((DateTime.Now).Ticks - 10000), new Time((DateTime.Now).Ticks)));
				this.put(TimestampSchema.of(), Arrays.asList(new Timestamp((DateTime.Now).Ticks), new Timestamp((DateTime.Now).Ticks)));
				this.put(InstantSchema.of(), Arrays.asList(Instant.now(), Instant.now().minusSeconds(60 * 23L)));
				this.put(LocalDateSchema.of(), Arrays.asList(LocalDate.now(), LocalDate.now().minusDays(2)));
				this.put(LocalTimeSchema.of(), Arrays.asList(LocalTime.now(), LocalTime.now().minusHours(2)));
				this.put(LocalDateTimeSchema.of(), Arrays.asList(DateTime.Now, DateTime.Now.AddDays(-2), DateTime.Now.minusWeeks(10)));
			}

		}

		private static readonly IDictionary<Schema, IList<object>> _testData2 = new HashMapAnonymousInnerClass2();

		private class HashMapAnonymousInnerClass2 : Hashtable
		{
			public HashMapAnonymousInnerClass2()
			{

				this.put(Schema.BOOL, Arrays.asList(false, true));
				this.put(Schema.STRING, Arrays.asList("my string"));
				this.put(Schema.INT8, Arrays.asList(unchecked((sbyte) 32767), unchecked((sbyte) -32768)));
				this.put(Schema.INT16, Arrays.asList((short) 32767, (short) -32768));
				this.put(Schema.INT32, Arrays.asList((int) 423412424, (int) -41243432));
				this.put(Schema.INT64, Arrays.asList(922337203685477580L, -922337203685477581L));
				this.put(Schema.FLOAT, Arrays.asList(5678567.12312f, -5678567.12341f));
				this.put(Schema.DOUBLE, Arrays.asList(5678567.12312d, -5678567.12341d));
				this.put(Schema.BYTES, Arrays.asList("my string".GetBytes(UTF_8)));
				this.put(Schema.BYTEBUFFER, Arrays.asList(ByteBuffer.allocate(10).put("my string".GetBytes(UTF_8))));
				this.put(Schema.DATE, Arrays.asList(new Date((DateTime.Now).Ticks - 10000), new Date((DateTime.Now).Ticks)));
				this.put(Schema.TIME, Arrays.asList(new Time((DateTime.Now).Ticks - 10000), new Time((DateTime.Now).Ticks)));
				this.put(Schema.TIMESTAMP, Arrays.asList(new Timestamp((DateTime.Now).Ticks - 10000), new Timestamp((DateTime.Now).Ticks)));
				this.put(Schema.INSTANT, Arrays.asList(Instant.now(), Instant.now().minusSeconds(60 * 23L)));
				this.put(Schema.LOCAL_DATE, Arrays.asList(LocalDate.now(), LocalDate.now().minusDays(2)));
				this.put(Schema.LOCAL_TIME, Arrays.asList(LocalTime.now(), LocalTime.now().minusHours(2)));
				this.put(Schema.LOCAL_DATE_TIME, Arrays.asList(DateTime.Now, DateTime.Now.AddDays(-2), DateTime.Now.minusWeeks(10)));
			}

		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @DataProvider(name = "schemas") public Object[][] schemas()
		public virtual object[][] Schemas()
		{
			return new object[][]
			{
				new object[] {_testData},
				new object[] {_testData2}
			};
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(dataProvider = "schemas") public void allSchemasShouldSupportNull(java.util.Map<org.apache.pulsar.client.api.Schema, java.util.List<Object>> testData)
		public virtual void AllSchemasShouldSupportNull(IDictionary<Schema, IList<object>> TestData)
		{
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in C#:
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
			assertEquals(SchemaType.INSTANT, InstantSchema.of().SchemaInfo.Type);
			assertEquals(SchemaType.LOCAL_DATE, LocalDateSchema.of().SchemaInfo.Type);
			assertEquals(SchemaType.LOCAL_TIME, LocalTimeSchema.of().SchemaInfo.Type);
			assertEquals(SchemaType.LOCAL_DATE_TIME, LocalDateTimeSchema.of().SchemaInfo.Type);
		}


	}

}