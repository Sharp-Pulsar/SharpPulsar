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

	public class BooleanSchemaTest
	{

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testSchemaEncode()
		public virtual void TestSchemaEncode()
		{
			BooleanSchema Schema = BooleanSchema.Of();
			sbyte[] ExpectedTrue = new sbyte[] {1};
			sbyte[] ExpectedFalse = new sbyte[] {0};
			Assert.assertEquals(ExpectedTrue, Schema.encode(true));
			Assert.assertEquals(ExpectedFalse, Schema.encode(false));
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testSchemaEncodeDecodeFidelity()
		public virtual void TestSchemaEncodeDecodeFidelity()
		{
			BooleanSchema Schema = BooleanSchema.Of();
			Assert.assertEquals(new bool?(true), Schema.decode(Schema.encode(true)));
			Assert.assertEquals(new bool?(false), Schema.decode(Schema.encode(false)));
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testSchemaDecode()
		public virtual void TestSchemaDecode()
		{
			sbyte[] TrueBytes = new sbyte[] {1};
			sbyte[] FalseBytes = new sbyte[] {0};
			BooleanSchema Schema = BooleanSchema.Of();
			Assert.assertEquals(new bool?(true), Schema.decode(TrueBytes));
			Assert.assertEquals(new bool?(false), Schema.decode(FalseBytes));

			ByteBuf ByteBuf = ByteBufAllocator.DEFAULT.buffer(1);
			ByteBuf.writeBytes(TrueBytes);
			Assert.assertEquals(new bool?(true), Schema.decode(ByteBuf));
			ByteBuf.writerIndex(0);
			ByteBuf.writeBytes(FalseBytes);

			Assert.assertEquals(new bool?(false), Schema.decode(ByteBuf));
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testNullEncodeDecode()
		public virtual void TestNullEncodeDecode()
		{
			ByteBuf ByteBuf = null;
			sbyte[] Bytes = null;
			Assert.assertNull(BooleanSchema.Of().encode(null));
			Assert.assertNull(BooleanSchema.Of().decode(ByteBuf));
			Assert.assertNull(BooleanSchema.Of().decode(Bytes));
		}

	}

}