using System;

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
	using ByteBuf = io.netty.buffer.ByteBuf;

    public class DateSchemaTest
	{

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testSchemaEncode()
		public virtual void TestSchemaEncode()
		{
			DateSchema Schema = DateSchema.Of();
			DateTime Data = DateTime.Now;
			sbyte[] Expected = new sbyte[] {(sbyte)((int)((uint)Data.Ticks >> 56)), (sbyte)((int)((uint)Data.Ticks >> 48)), (sbyte)((int)((uint)Data.Ticks >> 40)), (sbyte)((int)((uint)Data.Ticks >> 32)), (sbyte)((int)((uint)Data.Ticks >> 24)), (sbyte)((int)((uint)Data.Ticks >> 16)), (sbyte)((int)((uint)Data.Ticks >> 8)), ((long?)Data.Ticks).Value};
			Assert.assertEquals(Expected, Schema.encode(Data));
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testSchemaEncodeDecodeFidelity()
		public virtual void TestSchemaEncodeDecodeFidelity()
		{
			DateSchema Schema = DateSchema.Of();
			DateTime Date = DateTime.Now;
			ByteBuf ByteBuf = ByteBufAllocator.DEFAULT.buffer(8);
			sbyte[] Bytes = Schema.encode(Date);
			ByteBuf.writeBytes(Bytes);
			Assert.assertEquals(Date, Schema.decode(Bytes));
			Assert.assertEquals(Date, Schema.decode(ByteBuf));
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testSchemaDecode()
		public virtual void TestSchemaDecode()
		{
			sbyte[] ByteData = new sbyte[] {0, 0, 0, 0, 0, 10, 24, 42};
			long Expected = 10 * 65536 + 24 * 256 + 42;
			DateSchema Schema = DateSchema.Of();
			ByteBuf ByteBuf = ByteBufAllocator.DEFAULT.buffer(8);
			ByteBuf.writeBytes(ByteData);
			Assert.assertEquals(Expected, Schema.decode(ByteData).Ticks);
			Assert.assertEquals(Expected, Schema.decode(ByteBuf).Ticks);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testNullEncodeDecode()
		public virtual void TestNullEncodeDecode()
		{
			ByteBuf ByteBuf = null;
			sbyte[] Bytes = null;

			Assert.assertNull(DateSchema.Of().encode(null));
			Assert.assertNull(DateSchema.Of().decode(ByteBuf));
			Assert.assertNull(DateSchema.Of().decode(Bytes));
		}

	}

}