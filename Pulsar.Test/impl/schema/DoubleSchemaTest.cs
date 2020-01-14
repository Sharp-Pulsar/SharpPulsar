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

	public class DoubleSchemaTest
	{

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testSchemaEncode()
		public virtual void testSchemaEncode()
		{
			DoubleSchema schema = DoubleSchema.of();
			double? data = new double?(12345678.1234);
			long longData = System.BitConverter.DoubleToInt64Bits(data);
			sbyte[] expected = new sbyte[] {(sbyte)((long)((ulong)longData >> 56)), (sbyte)((long)((ulong)longData >> 48)), (sbyte)((long)((ulong)longData >> 40)), (sbyte)((long)((ulong)longData >> 32)), (sbyte)((long)((ulong)longData >> 24)), (sbyte)((long)((ulong)longData >> 16)), (sbyte)((long)((ulong)longData >> 8)), ((long?)longData).Value};
			Assert.assertEquals(expected, schema.encode(data));
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testSchemaEncodeDecodeFidelity()
		public virtual void testSchemaEncodeDecodeFidelity()
		{
			DoubleSchema schema = DoubleSchema.of();
			double? dbl = new double?(1234578.8754321);
			ByteBuf byteBuf = ByteBufAllocator.DEFAULT.buffer(8);
			sbyte[] bytes = schema.encode(dbl);
			byteBuf.writeBytes(bytes);
			Assert.assertEquals(dbl, schema.decode(bytes));
			Assert.assertEquals(dbl, schema.decode(byteBuf));
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testNullEncodeDecode()
		public virtual void testNullEncodeDecode()
		{
			ByteBuf byteBuf = null;
			sbyte[] bytes = null;
			Assert.assertNull(DoubleSchema.of().encode(null));
			Assert.assertNull(DoubleSchema.of().decode(byteBuf));
			Assert.assertNull(DoubleSchema.of().decode(bytes));
		}

	}

}