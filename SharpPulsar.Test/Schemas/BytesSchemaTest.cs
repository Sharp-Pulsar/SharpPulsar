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
namespace Org.Apache.Pulsar.Client.Impl.Schema
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.testng.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.testng.Assert.assertSame;

	using ByteBuf = io.netty.buffer.ByteBuf;
	using ByteBufAllocator = io.netty.buffer.ByteBufAllocator;
	using Schema = org.apache.pulsar.client.api.Schema;
	using Test = org.testng.annotations.Test;

	/// <summary>
	/// Unit test <seealso cref="BytesSchema"/>.
	/// </summary>
	public class BytesSchemaTest
	{

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testBytesSchemaOf()
		public virtual void TestBytesSchemaOf()
		{
			TestBytesSchema(BytesSchema.of());
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testSchemaBYTES()
		public virtual void TestSchemaBYTES()
		{
			TestBytesSchema(Schema.BYTES);
		}

		private void TestBytesSchema(Schema<sbyte[]> Schema)
		{
			sbyte[] Data = "hello world".GetBytes(UTF_8);

			sbyte[] SerializedData = Schema.encode(Data);
			assertSame(Data, SerializedData);

			sbyte[] DeserializedData = Schema.decode(SerializedData);
			assertSame(Data, DeserializedData);
			ByteBuf ByteBuf = ByteBufAllocator.DEFAULT.buffer(DeserializedData.Length);
			ByteBuf.writeBytes(DeserializedData);
			assertEquals(Data, ((BytesSchema)Schema).decode(ByteBuf));

		}

	}

}