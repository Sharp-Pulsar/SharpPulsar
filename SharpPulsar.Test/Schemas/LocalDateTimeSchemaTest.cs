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
namespace SharpPulsar.Test.Schema
{


	public class LocalDateTimeSchemaTest
	{

		public virtual void TestSchemaEncode()
		{
			LocalDateTimeSchema Schema = LocalDateTimeSchema.of();
			DateTime LocalDateTime = DateTime.Now;
			ByteBuffer ByteBuffer = ByteBuffer.allocate(Long.BYTES * 2);
			ByteBuffer.putLong(LocalDateTime.toLocalDate().toEpochDay());
			ByteBuffer.putLong(LocalDateTime.toLocalTime().toNanoOfDay());
			sbyte[] Expected = ByteBuffer.array();
			Assert.assertEquals(Expected, Schema.encode(LocalDateTime));
		}


		public virtual void TestSchemaEncodeDecodeFidelity()
		{
			LocalDateTimeSchema Schema = LocalDateTimeSchema.of();
			DateTime LocalDateTime = DateTime.Now;
			ByteBuf ByteBuf = ByteBufAllocator.DEFAULT.buffer(Long.BYTES * 2);
			sbyte[] Bytes = Schema.encode(LocalDateTime);
			ByteBuf.writeBytes(Bytes);
			Assert.assertEquals(LocalDateTime, Schema.decode(Bytes));
			Assert.assertEquals(LocalDateTime, Schema.decode(ByteBuf));
		}


		public virtual void TestSchemaDecode()
		{
			DateTime LocalDateTime = new DateTime(2020, 8, 22, 2, 0, 0);
			ByteBuffer ByteBuffer = ByteBuffer.allocate(Long.BYTES * 2);
			ByteBuffer.putLong(LocalDateTime.toLocalDate().toEpochDay());
			ByteBuffer.putLong(LocalDateTime.toLocalTime().toNanoOfDay());
			sbyte[] ByteData = ByteBuffer.array();
			long ExpectedEpochDay = LocalDateTime.toLocalDate().toEpochDay();
			long ExpectedNanoOfDay = LocalDateTime.toLocalTime().toNanoOfDay();

			LocalDateTimeSchema Schema = LocalDateTimeSchema.of();
			ByteBuf ByteBuf = ByteBufAllocator.DEFAULT.buffer(Long.BYTES * 2);
			ByteBuf.writeBytes(ByteData);
			DateTime Decode = Schema.decode(ByteData);
			Assert.assertEquals(ExpectedEpochDay, Decode.toLocalDate().toEpochDay());
			Assert.assertEquals(ExpectedNanoOfDay, Decode.toLocalTime().toNanoOfDay());
			Decode = Schema.decode(ByteBuf);
			Assert.assertEquals(ExpectedEpochDay, Decode.toLocalDate().toEpochDay());
			Assert.assertEquals(ExpectedNanoOfDay, Decode.toLocalTime().toNanoOfDay());
		}


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