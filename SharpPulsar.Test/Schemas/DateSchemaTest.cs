using SharpPulsar.Schema;
using System;
using Xunit;
using SharpPulsar.Extension;

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
	public class DateSchemaTest
	{
		[Fact]
		public void TestSchemaEncode()
		{
			DateSchema schema = DateSchema.Of();
			DateTime data = DateTime.Now;
			var date = new DateTimeOffset(data).ToUnixTimeSeconds().LongToBigEndian();
			sbyte[] Expected = BitConverter.GetBytes(date).ToSBytes(); ;//new sbyte[] {(sbyte)((int)((uint)data.Ticks >> 56)), (sbyte)((int)((uint)data.Ticks >> 48)), (sbyte)((int)((uint)data.Ticks >> 40)), (sbyte)((int)((uint)data.Ticks >> 32)), (sbyte)((int)((uint)data.Ticks >> 24)), (sbyte)((int)((uint)data.Ticks >> 16)), (sbyte)((int)((uint)data.Ticks >> 8)), (sbyte)((long?)data.Ticks).Value};
			Assert.Equal(Expected, schema.Encode(data));
		}
		[Fact]
		public void TestSchemaEncodeDecodeFidelity()
		{
			DateSchema schema = DateSchema.Of();
			DateTime date = DateTime.Now;
			sbyte[] bytes = schema.Encode(date);
			Assert.Equal(date, schema.Decode(bytes));
		}


	}

}