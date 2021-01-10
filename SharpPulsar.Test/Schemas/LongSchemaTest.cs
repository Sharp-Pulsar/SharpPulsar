using SharpPulsar.Schema;
using System;
using SharpPulsar.Extension;
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


	public class LongSchemaTest
	{
		[Fact]
		public void TestSchemaEncode()
		{
			LongSchema longSchema = LongSchema.Of();
			var data = 1234578L;

			sbyte[] Expected = (sbyte[])(object)BitConverter.GetBytes(data.LongToBigEndian());
			Assert.Equal(Expected, longSchema.Encode(data));
		}

		[Fact]
		public void TestSchemaEncodeDecodeFidelity()
		{
			LongSchema longSchema = LongSchema.Of();
			long Start = 348592040;
			for (int I = 0; I < 100; ++I)
			{
				sbyte[] Encode = longSchema.Encode(Start + I);
				long Decoded = longSchema.Decode(Encode);
				Assert.Equal(Decoded, Start + I);
			}
		}


	}

}