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

	public class LocalDateSchemaTest
	{

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testSchemaEncode()
		public virtual void TestSchemaEncode()
		{
			LocalDateSchema Schema = LocalDateSchema.of();
			LocalDate LocalDate = LocalDate.now();
			sbyte[] Expected = new sbyte[] {(sbyte)((int)((uint)LocalDate.toEpochDay() >> 56)), (sbyte)((int)((uint)LocalDate.toEpochDay() >> 48)), (sbyte)((int)((uint)LocalDate.toEpochDay() >> 40)), (sbyte)((int)((uint)LocalDate.toEpochDay() >> 32)), (sbyte)((int)((uint)LocalDate.toEpochDay() >> 24)), (sbyte)((int)((uint)LocalDate.toEpochDay() >> 16)), (sbyte)((int)((uint)LocalDate.toEpochDay() >> 8)), ((long?)LocalDate.toEpochDay()).Value};
			Assert.assertEquals(Expected, Schema.encode(LocalDate));
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testSchemaEncodeDecodeFidelity()
		public virtual void TestSchemaEncodeDecodeFidelity()
		{
			LocalDateSchema Schema = LocalDateSchema.of();
			LocalDate LocalDate = LocalDate.now();
			ByteBuf ByteBuf = ByteBufAllocator.DEFAULT.buffer(8);
			sbyte[] Bytes = Schema.encode(LocalDate);
			ByteBuf.writeBytes(Bytes);
			Assert.assertEquals(LocalDate, Schema.decode(Bytes));
			Assert.assertEquals(LocalDate, Schema.decode(ByteBuf));
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testSchemaDecode()
		public virtual void TestSchemaDecode()
		{
			sbyte[] ByteData = new sbyte[] {0, 0, 0, 0, 0, 10, 24, 42};
			long Expected = 10 * 65536 + 24 * 256 + 42;

			LocalDateSchema Schema = LocalDateSchema.of();
			ByteBuf ByteBuf = ByteBufAllocator.DEFAULT.buffer(8);
			ByteBuf.writeBytes(ByteData);
			Assert.assertEquals(Expected, Schema.decode(ByteData).toEpochDay());
			Assert.assertEquals(Expected, Schema.decode(ByteBuf).toEpochDay());
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testNullEncodeDecode()
		public virtual void TestNullEncodeDecode()
		{
			ByteBuf ByteBuf = null;
			sbyte[] Bytes = null;

			Assert.assertNull(LocalDateSchema.of().encode(null));
			Assert.assertNull(LocalDateSchema.of().decode(ByteBuf));
			Assert.assertNull(LocalDateSchema.of().decode(Bytes));
		}

	}

}