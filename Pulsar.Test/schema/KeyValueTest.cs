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
namespace org.apache.pulsar.common.schema
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.testng.Assert.assertEquals;

	using Unpooled = io.netty.buffer.Unpooled;
	using Schema = client.api.Schema;
	using BooleanSchema = client.impl.schema.BooleanSchema;
	using ByteBufSchema = client.impl.schema.ByteBufSchema;
	using ByteBufferSchema = client.impl.schema.ByteBufferSchema;
	using ByteSchema = client.impl.schema.ByteSchema;
	using BytesSchema = client.impl.schema.BytesSchema;
	using DateSchema = client.impl.schema.DateSchema;
	using DoubleSchema = client.impl.schema.DoubleSchema;
	using FloatSchema = client.impl.schema.FloatSchema;
	using IntSchema = client.impl.schema.IntSchema;
	using LongSchema = client.impl.schema.LongSchema;
	using ShortSchema = client.impl.schema.ShortSchema;
	using StringSchema = client.impl.schema.StringSchema;
	using TimeSchema = client.impl.schema.TimeSchema;
	using TimestampSchema = client.impl.schema.TimestampSchema;
	using DataProvider = org.testng.annotations.DataProvider;
	using Test = org.testng.annotations.Test;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public class KeyValueTest
	public class KeyValueTest
	{

		private static readonly IDictionary<Schema, IList<object>> testData = new HashMapAnonymousInnerClass();

		private class HashMapAnonymousInnerClass : Hashtable
		{

			private const long serialVersionUID = -3081991052949960650L;

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

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @DataProvider(name = "schemas") public Object[][] schemas()
		public virtual object[][] schemas()
		{
			return new object[][]
			{
				new object[] {testData}
			};
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(dataProvider = "schemas") public void testAllSchemas(java.util.Map<org.apache.pulsar.client.api.Schema, java.util.List<Object>> schemas)
		public virtual void testAllSchemas(IDictionary<Schema, IList<object>> schemas)
		{
			foreach (KeyValuePair<Schema, IList<object>> keyEntry in schemas.SetOfKeyValuePairs())
			{
				foreach (KeyValuePair<Schema, IList<object>> valueEntry in schemas.SetOfKeyValuePairs())
				{
					testEncodeDecodeKeyValue(keyEntry.Key, valueEntry.Key, keyEntry.Value, valueEntry.Value);
				}
			}
		}

		private void testEncodeDecodeKeyValue<K, V>(Schema<K> keySchema, Schema<V> valueSchema, IList<K> keys, IList<V> values)
		{
			foreach (K key in keys)
			{
				foreach (V value in values)
				{
					sbyte[] data = KeyValue.encode(key, keySchema, value, valueSchema);

					KeyValue<K, V> kv = KeyValue.decode(data, (keyBytes, valueBytes) => new KeyValue<K, V>(keySchema.decode(keyBytes), valueSchema.decode(valueBytes))
				   );

					assertEquals(kv.Key, key);
					assertEquals(kv.Value, value);
				}
			}
		}

	}

}