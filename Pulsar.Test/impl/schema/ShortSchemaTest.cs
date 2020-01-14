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

	public class ShortSchemaTest
	{

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testSchemaEncode()
		public virtual void testSchemaEncode()
		{
			ShortSchema schema = ShortSchema.of();
			short? data = 12345;
			sbyte[] expected = new sbyte[] {(sbyte)((int)((uint)data >> 8)), data.Value};
			Assert.assertEquals(expected, schema.encode(data));
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testSchemaEncodeDecodeFidelity()
		public virtual void testSchemaEncodeDecodeFidelity()
		{
			ShortSchema schema = ShortSchema.of();
			ByteBuf byteBuf = ByteBufAllocator.DEFAULT.buffer(2);
			short start = 3440;
			for (short i = 0; i < 100; ++i)
			{
				sbyte[] encode = schema.encode((short)(start + i));
				byteBuf.writerIndex(0);
				byteBuf.writeBytes(encode);
				int decoded = schema.decode(encode);
				Assert.assertEquals(decoded, start + i);
				decoded = schema.decode(byteBuf);
				Assert.assertEquals(decoded, start + i);
			}
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testSchemaDecode()
		public virtual void testSchemaDecode()
		{
			sbyte[] byteData = new sbyte[] {24, 42};
			short? expected = 24 * 256 + 42;
			ShortSchema schema = ShortSchema.of();
			ByteBuf byteBuf = ByteBufAllocator.DEFAULT.buffer(2);
			byteBuf.writeBytes(byteData);
			Assert.assertEquals(expected, schema.decode(byteData));
			Assert.assertEquals(expected, schema.decode(byteBuf));
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testNullEncodeDecode()
		public virtual void testNullEncodeDecode()
		{
			ByteBuf byteBuf = null;
			sbyte[] bytes = null;
			Assert.assertNull(ShortSchema.of().encode(null));
			Assert.assertNull(ShortSchema.of().decode(byteBuf));
			Assert.assertNull(ShortSchema.of().decode(bytes));
		}

	}

}