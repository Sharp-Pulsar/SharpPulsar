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

	public class LongSchemaTest
	{

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testSchemaEncode()
		public virtual void testSchemaEncode()
		{
			LongSchema longSchema = LongSchema.of();
			long? data = 1234578l;
			sbyte[] expected = new sbyte[] {(sbyte)((int)((uint)data >> 56)), (sbyte)((int)((uint)data >> 48)), (sbyte)((int)((uint)data >> 40)), (sbyte)((int)((uint)data >> 32)), (sbyte)((int)((uint)data >> 24)), (sbyte)((int)((uint)data >> 16)), (sbyte)((int)((uint)data >> 8)), data.Value};
			Assert.assertEquals(expected, longSchema.encode(data));
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testSchemaEncodeDecodeFidelity()
		public virtual void testSchemaEncodeDecodeFidelity()
		{
			LongSchema longSchema = LongSchema.of();
			ByteBuf byteBuf = ByteBufAllocator.DEFAULT.buffer(8);
			long start = 348592040;
			for (int i = 0; i < 100; ++i)
			{
				sbyte[] encode = longSchema.encode(start + i);
				long decoded = longSchema.decode(encode).Value;
				Assert.assertEquals(decoded, start + i);
				byteBuf.writerIndex(0);
				byteBuf.writeBytes(encode);

				decoded = longSchema.decode(byteBuf).Value;
				Assert.assertEquals(decoded, start + i);
			}
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testSchemaDecode()
		public virtual void testSchemaDecode()
		{
			sbyte[] byteData = new sbyte[] {0, 0, 0, 0, 0, 10, 24, 42};
			long? expected = 10 * 65536l + 24 * 256 + 42;
			LongSchema longSchema = LongSchema.of();
			ByteBuf byteBuf = ByteBufAllocator.DEFAULT.buffer(8);
			byteBuf.writeBytes(byteData);

			Assert.assertEquals(expected, longSchema.decode(byteData));
			Assert.assertEquals(expected, longSchema.decode(byteBuf));
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testNullEncodeDecode()
		public virtual void testNullEncodeDecode()
		{
			ByteBuf byteBuf = null;
			sbyte[] bytes = null;
			Assert.assertNull(LongSchema.of().encode(null));
			Assert.assertNull(LongSchema.of().decode(byteBuf));
			Assert.assertNull(LongSchema.of().decode(bytes));
		}

	}

}