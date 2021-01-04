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
namespace Org.Apache.Pulsar.Client.Impl.Schema
{
	using ByteBuf = io.netty.buffer.ByteBuf;
	using ByteBufAllocator = io.netty.buffer.ByteBufAllocator;
	using Assert = org.testng.Assert;
	using Test = org.testng.annotations.Test;

	public class LongSchemaTest
	{

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testSchemaEncode()
		public virtual void TestSchemaEncode()
		{
			LongSchema LongSchema = LongSchema.of();
			long? Data = 1234578l;
			sbyte[] Expected = new sbyte[] {(sbyte)((int)((uint)Data >> 56)), (sbyte)((int)((uint)Data >> 48)), (sbyte)((int)((uint)Data >> 40)), (sbyte)((int)((uint)Data >> 32)), (sbyte)((int)((uint)Data >> 24)), (sbyte)((int)((uint)Data >> 16)), (sbyte)((int)((uint)Data >> 8)), Data.Value};
			Assert.assertEquals(Expected, LongSchema.encode(Data));
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testSchemaEncodeDecodeFidelity()
		public virtual void TestSchemaEncodeDecodeFidelity()
		{
			LongSchema LongSchema = LongSchema.of();
			ByteBuf ByteBuf = ByteBufAllocator.DEFAULT.buffer(8);
			long Start = 348592040;
			for (int I = 0; I < 100; ++I)
			{
				sbyte[] Encode = LongSchema.encode(Start + I);
				long Decoded = LongSchema.decode(Encode);
				Assert.assertEquals(Decoded, Start + I);
				ByteBuf.writerIndex(0);
				ByteBuf.writeBytes(Encode);

				Decoded = LongSchema.decode(ByteBuf);
				Assert.assertEquals(Decoded, Start + I);
			}
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testSchemaDecode()
		public virtual void TestSchemaDecode()
		{
			sbyte[] ByteData = new sbyte[] {0, 0, 0, 0, 0, 10, 24, 42};
			long? Expected = 10 * 65536l + 24 * 256 + 42;
			LongSchema LongSchema = LongSchema.of();
			ByteBuf ByteBuf = ByteBufAllocator.DEFAULT.buffer(8);
			ByteBuf.writeBytes(ByteData);

			Assert.assertEquals(Expected, LongSchema.decode(ByteData));
			Assert.assertEquals(Expected, LongSchema.decode(ByteBuf));
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testNullEncodeDecode()
		public virtual void TestNullEncodeDecode()
		{
			ByteBuf ByteBuf = null;
			sbyte[] Bytes = null;
			Assert.assertNull(LongSchema.of().encode(null));
			Assert.assertNull(LongSchema.of().decode(ByteBuf));
			Assert.assertNull(LongSchema.of().decode(Bytes));
		}

	}

}