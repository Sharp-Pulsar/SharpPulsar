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
namespace org.apache.pulsar.client.impl.schema
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
		public virtual void testUtf8Charset()
		{
			StringSchema schema = new StringSchema();
			SchemaInfo si = schema.SchemaInfo;
			assertFalse(si.Properties.containsKey(StringSchema.CHARSET_KEY));

			string myString = "my string for test";
			sbyte[] data = schema.encode(myString);
			assertArrayEquals(data, myString.GetBytes(UTF_8));

			string decodedString = schema.decode(data);
			assertEquals(decodedString, myString);

			ByteBuf byteBuf = ByteBufAllocator.DEFAULT.buffer(data.Length);
			byteBuf.writeBytes(data);

			assertEquals(schema.decode(byteBuf), myString);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testAsciiCharset()
		public virtual void testAsciiCharset()
		{
			StringSchema schema = new StringSchema(US_ASCII);
			SchemaInfo si = schema.SchemaInfo;
			assertTrue(si.Properties.containsKey(StringSchema.CHARSET_KEY));
			assertEquals(si.Properties.get(StringSchema.CHARSET_KEY), US_ASCII.name());

			string myString = "my string for test";
			sbyte[] data = schema.encode(myString);
			assertArrayEquals(data, myString.GetBytes(US_ASCII));

			string decodedString = schema.decode(data);
			assertEquals(decodedString, myString);

			ByteBuf byteBuf = ByteBufAllocator.DEFAULT.buffer(data.Length);
			byteBuf.writeBytes(data);

			assertEquals(schema.decode(byteBuf), myString);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testSchemaInfoWithoutCharset()
		public virtual void testSchemaInfoWithoutCharset()
		{
			SchemaInfo si = (new SchemaInfo()).setName("test-schema-info-without-charset").setType(SchemaType.STRING).setSchema(new sbyte[0]).setProperties(Collections.emptyMap());
			StringSchema schema = StringSchema.fromSchemaInfo(si);

			string myString = "my string for test";
			sbyte[] data = schema.encode(myString);
			assertArrayEquals(data, myString.GetBytes(UTF_8));

			string decodedString = schema.decode(data);
			assertEquals(decodedString, myString);

			ByteBuf byteBuf = ByteBufAllocator.DEFAULT.buffer(data.Length);
			byteBuf.writeBytes(data);
			assertEquals(schema.decode(byteBuf), myString);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @DataProvider(name = "charsets") public Object[][] charsets()
		public virtual object[][] charsets()
		{
			return new object[][]
			{
				new object[] {UTF_8},
				new object[] {US_ASCII}
			};
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(dataProvider = "charsets") public void testSchemaInfoWithCharset(java.nio.charset.Charset charset)
		public virtual void testSchemaInfoWithCharset(Charset charset)
		{
			IDictionary<string, string> properties = new Dictionary<string, string>();
			properties[StringSchema.CHARSET_KEY] = charset.name();
			SchemaInfo si = (new SchemaInfo()).setName("test-schema-info-without-charset").setType(SchemaType.STRING).setSchema(new sbyte[0]).setProperties(properties);
			StringSchema schema = StringSchema.fromSchemaInfo(si);

			string myString = "my string for test";
			sbyte[] data = schema.encode(myString);
			assertArrayEquals(data, myString.GetBytes(charset));

			string decodedString = schema.decode(data);
			assertEquals(decodedString, myString);

			ByteBuf byteBuf = ByteBufAllocator.DEFAULT.buffer(data.Length);
			byteBuf.writeBytes(data);

			assertEquals(schema.decode(byteBuf), myString);
		}

	}

}