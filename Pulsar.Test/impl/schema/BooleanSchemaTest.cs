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

	public class BooleanSchemaTest
	{

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testSchemaEncode()
		public virtual void testSchemaEncode()
		{
			BooleanSchema schema = BooleanSchema.of();
			sbyte[] expectedTrue = new sbyte[] {1};
			sbyte[] expectedFalse = new sbyte[] {0};
			Assert.assertEquals(expectedTrue, schema.encode(true));
			Assert.assertEquals(expectedFalse, schema.encode(false));
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testSchemaEncodeDecodeFidelity()
		public virtual void testSchemaEncodeDecodeFidelity()
		{
			BooleanSchema schema = BooleanSchema.of();
			Assert.assertEquals(new bool?(true), schema.decode(schema.encode(true)));
			Assert.assertEquals(new bool?(false), schema.decode(schema.encode(false)));
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testSchemaDecode()
		public virtual void testSchemaDecode()
		{
			sbyte[] trueBytes = new sbyte[] {1};
			sbyte[] falseBytes = new sbyte[] {0};
			BooleanSchema schema = BooleanSchema.of();
			Assert.assertEquals(new bool?(true), schema.decode(trueBytes));
			Assert.assertEquals(new bool?(false), schema.decode(falseBytes));

			ByteBuf byteBuf = ByteBufAllocator.DEFAULT.buffer(1);
			byteBuf.writeBytes(trueBytes);
			Assert.assertEquals(new bool?(true), schema.decode(byteBuf));
			byteBuf.writerIndex(0);
			byteBuf.writeBytes(falseBytes);

			Assert.assertEquals(new bool?(false), schema.decode(byteBuf));
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testNullEncodeDecode()
		public virtual void testNullEncodeDecode()
		{
			ByteBuf byteBuf = null;
			sbyte[] bytes = null;
			Assert.assertNull(BooleanSchema.of().encode(null));
			Assert.assertNull(BooleanSchema.of().decode(byteBuf));
			Assert.assertNull(BooleanSchema.of().decode(bytes));
		}

	}

}