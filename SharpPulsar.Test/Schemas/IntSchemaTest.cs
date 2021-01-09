using SharpPulsar.Schema;
using SharpPulsar.Extension;
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


	public class IntSchemaTest
	{

		public virtual void TestSchemaEncode()
		{
			IntSchema schema = IntSchema.Of();
			var data = 1234578;
			sbyte[] expected = BitConverter.GetBytes(data.IntToBigEndian()).ToSBytes();
			Assert.Equal(expected, schema.Encode(data));
		}
		
		public virtual void TestSchemaDecode()
		{
			IntSchema schema = IntSchema.Of();
			var data = 1234578;
			var bytes = schema.Encode(data);
			Assert.Equal(data, schema.Decode(bytes));
		}

	}

}