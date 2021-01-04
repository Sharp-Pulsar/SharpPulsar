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
namespace SharpPulsar.Test.Schema
{

	public class ShortSchemaTest
	{
		public virtual void TestSchemaEncode()
		{
			ShortSchema Schema = ShortSchema.of();
			short? Data = 12345;
			sbyte[] Expected = new sbyte[] {(sbyte)((int)((uint)Data >> 8)), Data.Value};
			Assert.assertEquals(Expected, Schema.encode(Data));
		}

		public virtual void TestSchemaEncodeDecodeFidelity()
		{
			ShortSchema Schema = ShortSchema.of();
			ByteBuf ByteBuf = ByteBufAllocator.DEFAULT.buffer(2);
			short Start = 3440;
			for (short I = 0; I < 100; ++I)
			{
				sbyte[] Encode = Schema.encode((short)(Start + I));
				ByteBuf.writerIndex(0);
				ByteBuf.writeBytes(Encode);
				int Decoded = Schema.decode(Encode);
				Assert.assertEquals(Decoded, Start + I);
				Decoded = Schema.decode(ByteBuf);
				Assert.assertEquals(Decoded, Start + I);
			}
		}

		public virtual void TestSchemaDecode()
		{
			sbyte[] ByteData = new sbyte[] {24, 42};
			short? Expected = 24 * 256 + 42;
			ShortSchema Schema = ShortSchema.of();
			ByteBuf ByteBuf = ByteBufAllocator.DEFAULT.buffer(2);
			ByteBuf.writeBytes(ByteData);
			Assert.assertEquals(Expected, Schema.decode(ByteData));
			Assert.assertEquals(Expected, Schema.decode(ByteBuf));
		}

		public virtual void TestNullEncodeDecode()
		{
			ByteBuf ByteBuf = null;
			sbyte[] Bytes = null;
			Assert.assertNull(ShortSchema.of().encode(null));
			Assert.assertNull(ShortSchema.of().decode(ByteBuf));
			Assert.assertNull(ShortSchema.of().decode(Bytes));
		}

	}

}