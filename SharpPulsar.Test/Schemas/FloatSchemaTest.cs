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

	public class FloatSchemaTest
	{

		public virtual void TestSchemaEncode()
		{
			FloatSchema Schema = FloatSchema.of();
			float? Data = new float?(12345678.1234);
			long LongData = Float.floatToRawIntBits(Data);
			sbyte[] Expected = new sbyte[] {(sbyte)((long)((ulong)LongData >> 24)), (sbyte)((long)((ulong)LongData >> 16)), (sbyte)((long)((ulong)LongData >> 8)), ((long?)LongData).Value};
			Assert.assertEquals(Expected, Schema.encode(Data));
		}

		public virtual void TestSchemaEncodeDecodeFidelity()
		{
			FloatSchema Schema = FloatSchema.of();
			ByteBuf ByteBuf = ByteBufAllocator.DEFAULT.buffer(4);
			float? Dbl = new float?(1234578.8754321);
			sbyte[] Bytes = Schema.encode(Dbl);
			ByteBuf.writeBytes(Schema.encode(Dbl));
			Assert.assertEquals(Dbl, Schema.decode(Bytes));
			Assert.assertEquals(Dbl, Schema.decode(ByteBuf));

		}

		public virtual void TestNullEncodeDecode()
		{
			ByteBuf ByteBuf = null;
			sbyte[] Bytes = null;
			Assert.assertNull(FloatSchema.of().encode(null));
			Assert.assertNull(FloatSchema.of().decode(Bytes));
			Assert.assertNull(FloatSchema.of().decode(ByteBuf));
		}
	}



}