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

	public class DoubleSchemaTest
	{

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testSchemaEncode()
		public virtual void TestSchemaEncode()
		{
			DoubleSchema Schema = DoubleSchema.Of();
			double? Data = new double?(12345678.1234);
			long LongData = System.BitConverter.DoubleToInt64Bits(Data);
			sbyte[] Expected = new sbyte[] {(sbyte)((long)((ulong)LongData >> 56)), (sbyte)((long)((ulong)LongData >> 48)), (sbyte)((long)((ulong)LongData >> 40)), (sbyte)((long)((ulong)LongData >> 32)), (sbyte)((long)((ulong)LongData >> 24)), (sbyte)((long)((ulong)LongData >> 16)), (sbyte)((long)((ulong)LongData >> 8)), ((long?)LongData).Value};
			Assert.assertEquals(Expected, Schema.encode(Data));
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testSchemaEncodeDecodeFidelity()
		public virtual void TestSchemaEncodeDecodeFidelity()
		{
			DoubleSchema Schema = DoubleSchema.Of();
			double? Dbl = new double?(1234578.8754321);
			ByteBuf ByteBuf = ByteBufAllocator.DEFAULT.buffer(8);
			sbyte[] Bytes = Schema.encode(Dbl);
			ByteBuf.writeBytes(Bytes);
			Assert.assertEquals(Dbl, Schema.decode(Bytes));
			Assert.assertEquals(Dbl, Schema.decode(ByteBuf));
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testNullEncodeDecode()
		public virtual void TestNullEncodeDecode()
		{
			ByteBuf ByteBuf = null;
			sbyte[] Bytes = null;
			Assert.assertNull(DoubleSchema.Of().encode(null));
			Assert.assertNull(DoubleSchema.Of().decode(ByteBuf));
			Assert.assertNull(DoubleSchema.Of().decode(Bytes));
		}

	}

}