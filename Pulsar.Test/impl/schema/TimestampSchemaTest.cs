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
	using ByteBuf = io.netty.buffer.ByteBuf;
	using ByteBufAllocator = io.netty.buffer.ByteBufAllocator;
	using Assert = org.testng.Assert;
	using Test = org.testng.annotations.Test;

	public class TimestampSchemaTest
	{

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testSchemaEncode()
		public virtual void testSchemaEncode()
		{
			TimestampSchema schema = TimestampSchema.of();
			Timestamp data = new Timestamp(DateTimeHelper.CurrentUnixTimeMillis());
			sbyte[] expected = new sbyte[] {(sbyte)((int)((uint)data.Time >> 56)), (sbyte)((int)((uint)data.Time >> 48)), (sbyte)((int)((uint)data.Time >> 40)), (sbyte)((int)((uint)data.Time >> 32)), (sbyte)((int)((uint)data.Time >> 24)), (sbyte)((int)((uint)data.Time >> 16)), (sbyte)((int)((uint)data.Time >> 8)), ((long?)data.Time).Value};
			Assert.assertEquals(expected, schema.encode(data));
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testSchemaEncodeDecodeFidelity()
		public virtual void testSchemaEncodeDecodeFidelity()
		{
			TimestampSchema schema = TimestampSchema.of();
			Timestamp timestamp = new Timestamp(DateTimeHelper.CurrentUnixTimeMillis());
			Assert.assertEquals(timestamp, schema.decode(schema.encode(timestamp)));
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testSchemaDecode()
		public virtual void testSchemaDecode()
		{
			sbyte[] byteData = new sbyte[] {0, 0, 0, 0, 0, 10, 24, 42};

			ByteBuf byteBuf = ByteBufAllocator.DEFAULT.buffer(byteData.Length);
			byteBuf.writeBytes(byteData);
			long expected = 10 * 65536 + 24 * 256 + 42;
			TimestampSchema schema = TimestampSchema.of();
			Assert.assertEquals(expected, schema.decode(byteData).Time);
			Assert.assertEquals(expected, schema.decode(byteBuf).Time);

		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testNullEncodeDecode()
		public virtual void testNullEncodeDecode()
		{
			ByteBuf byteBuf = null;
			sbyte[] bytes = null;
			Assert.assertNull(TimestampSchema.of().encode(null));
			Assert.assertNull(TimestampSchema.of().decode(byteBuf));
			Assert.assertNull(TimestampSchema.of().decode(bytes));
		}

	}

}