using SharpPulsar.Schema;
using System;
using Xunit;

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
			DateSchema Schema = DateSchema.Of();
			DateTime Data = DateTime.Now;
			sbyte[] Expected = new sbyte[] {(sbyte)((int)((uint)Data.Ticks >> 56)), (sbyte)((int)((uint)Data.Ticks >> 48)), (sbyte)((int)((uint)Data.Ticks >> 40)), (sbyte)((int)((uint)Data.Ticks >> 32)), (sbyte)((int)((uint)Data.Ticks >> 24)), (sbyte)((int)((uint)Data.Ticks >> 16)), (sbyte)((int)((uint)Data.Ticks >> 8)), (sbyte)((long?)Data.Ticks).Value};
			Assert.Equal(Expected, Schema.Encode(Data));
		}
		[Fact]
		public void TestSchemaEncodeDecodeFidelity()
		{
			DateSchema Schema = DateSchema.Of();
			DateTime Date = DateTime.Now;
			sbyte[] Bytes = Schema.Encode(Date);
			Assert.Equal(Date, Schema.Decode(Bytes));
		}


	}

}