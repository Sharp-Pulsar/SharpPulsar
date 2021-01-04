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

	public class InstantSchemaTest
	{

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testSchemaEncode()
		public virtual void TestSchemaEncode()
		{
			InstantSchema Schema = InstantSchema.of();
			Instant Instant = Instant.now();
			ByteBuffer ByteBuffer = ByteBuffer.allocate(Long.BYTES + Integer.BYTES);
			ByteBuffer.putLong(Instant.EpochSecond);
			ByteBuffer.putInt(Instant.Nano);
			sbyte[] Expected = ByteBuffer.array();
			Assert.assertEquals(Expected, Schema.encode(Instant));
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testSchemaEncodeDecodeFidelity()
		public virtual void TestSchemaEncodeDecodeFidelity()
		{
			InstantSchema Schema = InstantSchema.of();
			Instant Instant = Instant.now();
			ByteBuf ByteBuf = ByteBufAllocator.DEFAULT.buffer(Long.BYTES + Integer.BYTES);
			sbyte[] Bytes = Schema.encode(Instant);
			ByteBuf.writeBytes(Bytes);
			Assert.assertEquals(Instant, Schema.decode(Bytes));
			Assert.assertEquals(Instant, Schema.decode(ByteBuf));
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testSchemaDecode()
		public virtual void TestSchemaDecode()
		{
			Instant Instant = Instant.now();
			ByteBuffer ByteBuffer = ByteBuffer.allocate(Long.BYTES + Integer.BYTES);
			ByteBuffer.putLong(Instant.EpochSecond);
			ByteBuffer.putInt(Instant.Nano);
			sbyte[] ByteData = ByteBuffer.array();
			long EpochSecond = Instant.EpochSecond;
			long Nano = Instant.Nano;

			InstantSchema Schema = InstantSchema.of();
			ByteBuf ByteBuf = ByteBufAllocator.DEFAULT.buffer(Long.BYTES + Integer.BYTES);
			ByteBuf.writeBytes(ByteData);
			Instant Decode = Schema.decode(ByteData);
			Assert.assertEquals(EpochSecond, Decode.EpochSecond);
			Assert.assertEquals(Nano, Decode.Nano);
			Decode = Schema.decode(ByteBuf);
			Assert.assertEquals(EpochSecond, Decode.EpochSecond);
			Assert.assertEquals(Nano, Decode.Nano);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testNullEncodeDecode()
		public virtual void TestNullEncodeDecode()
		{
			ByteBuf ByteBuf = null;
			sbyte[] Bytes = null;

			Assert.assertNull(InstantSchema.of().encode(null));
			Assert.assertNull(InstantSchema.of().decode(ByteBuf));
			Assert.assertNull(InstantSchema.of().decode(Bytes));
		}

	}

}