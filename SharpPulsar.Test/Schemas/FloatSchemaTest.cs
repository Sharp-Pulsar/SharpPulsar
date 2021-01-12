using SharpPulsar.Schema;
using System;
using Xunit;
using SharpPulsar.Extension;
using System.Linq;
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
		[Fact]
		public void TestSchemaEncode()
		{
			FloatSchema schema = FloatSchema.Of();
			var data = 12345678.1234F;
			sbyte[] expected = BitConverter.GetBytes(data).Reverse().ToArray().ToSBytes();// new sbyte[] {(sbyte)((long)((ulong)longData >> 24)), (sbyte)((long)((ulong)longData >> 16)), (sbyte)((long)((ulong)longData >> 8)), (sbyte)longData };
			Assert.Equal(expected, schema.Encode(data));
		}
		[Fact]
		public void TestSchemaEncodeDecodeFidelity()
		{
			FloatSchema schema = FloatSchema.Of();
			var dbl = 1234578.8754321F;
			sbyte[] bytes = schema.Encode(dbl);
			Assert.Equal(dbl, schema.Decode(bytes));

		}
	}



}