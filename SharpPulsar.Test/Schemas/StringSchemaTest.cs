using System.Collections.Generic;

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
//	import static org.testng.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.testng.Assert.assertNotEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.testng.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.testng.@internal.junit.ArrayAsserts.assertArrayEquals;


	using ByteBuf = io.netty.buffer.ByteBuf;
	using ByteBufAllocator = io.netty.buffer.ByteBufAllocator;
	using SchemaInfo = org.apache.pulsar.common.schema.SchemaInfo;
	using SchemaType = org.apache.pulsar.common.schema.SchemaType;
	using DataProvider = org.testng.annotations.DataProvider;
	using Test = org.testng.annotations.Test;

	/// <summary>
	/// Unit test <seealso cref="StringSchema"/>.
	/// </summary>
	public class StringSchemaTest
	{

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testUtf8Charset()
		public virtual void TestUtf8Charset()
		{
			StringSchema Schema = new StringSchema();
			SchemaInfo Si = Schema.SchemaInfo;
			assertFalse(Si.Properties.containsKey(StringSchema.CHARSET_KEY));

			string MyString = "my string for test";
			sbyte[] Data = Schema.encode(MyString);
			assertArrayEquals(Data, MyString.GetBytes(UTF_8));

			string DecodedString = Schema.decode(Data);
			assertEquals(DecodedString, MyString);

			ByteBuf ByteBuf = ByteBufAllocator.DEFAULT.buffer(Data.Length);
			ByteBuf.writeBytes(Data);

			assertEquals(Schema.decode(ByteBuf), MyString);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testAsciiCharset()
		public virtual void TestAsciiCharset()
		{
			StringSchema Schema = new StringSchema(US_ASCII);
			SchemaInfo Si = Schema.SchemaInfo;
			assertTrue(Si.Properties.containsKey(StringSchema.CHARSET_KEY));
			assertEquals(Si.Properties.get(StringSchema.CHARSET_KEY), US_ASCII.name());

			string MyString = "my string for test";
			sbyte[] Data = Schema.encode(MyString);
			assertArrayEquals(Data, MyString.GetBytes(US_ASCII));

			string DecodedString = Schema.decode(Data);
			assertEquals(DecodedString, MyString);

			ByteBuf ByteBuf = ByteBufAllocator.DEFAULT.buffer(Data.Length);
			ByteBuf.writeBytes(Data);

			assertEquals(Schema.decode(ByteBuf), MyString);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testSchemaInfoWithoutCharset()
		public virtual void TestSchemaInfoWithoutCharset()
		{
			SchemaInfo Si = (new SchemaInfo()).setName("test-schema-info-without-charset").setType(SchemaType.STRING).setSchema(new sbyte[0]).setProperties(Collections.emptyMap());
			StringSchema Schema = StringSchema.fromSchemaInfo(Si);

			string MyString = "my string for test";
			sbyte[] Data = Schema.encode(MyString);
			assertArrayEquals(Data, MyString.GetBytes(UTF_8));

			string DecodedString = Schema.decode(Data);
			assertEquals(DecodedString, MyString);

			ByteBuf ByteBuf = ByteBufAllocator.DEFAULT.buffer(Data.Length);
			ByteBuf.writeBytes(Data);
			assertEquals(Schema.decode(ByteBuf), MyString);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @DataProvider(name = "charsets") public Object[][] charsets()
		public virtual object[][] Charsets()
		{
			return new object[][]
			{
				new object[] {UTF_8},
				new object[] {US_ASCII}
			};
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(dataProvider = "charsets") public void testSchemaInfoWithCharset(java.nio.charset.Charset charset)
		public virtual void TestSchemaInfoWithCharset(Charset Charset)
		{
			IDictionary<string, string> Properties = new Dictionary<string, string>();
			Properties[StringSchema.CHARSET_KEY] = Charset.name();
			SchemaInfo Si = (new SchemaInfo()).setName("test-schema-info-without-charset").setType(SchemaType.STRING).setSchema(new sbyte[0]).setProperties(Properties);
			StringSchema Schema = StringSchema.fromSchemaInfo(Si);

			string MyString = "my string for test";
			sbyte[] Data = Schema.encode(MyString);
			assertArrayEquals(Data, MyString.GetBytes(Charset));

			string DecodedString = Schema.decode(Data);
			assertEquals(DecodedString, MyString);

			ByteBuf ByteBuf = ByteBufAllocator.DEFAULT.buffer(Data.Length);
			ByteBuf.writeBytes(Data);

			assertEquals(Schema.decode(ByteBuf), MyString);
		}

	}

}