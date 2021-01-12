using SharpPulsar.Common.Schema;
using SharpPulsar.Interfaces.ISchema;
using SharpPulsar.Schema;
using SharpPulsar.Shared;
using System.Collections.Generic;
using System.Text;
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
	/// <summary>
	/// Unit test <seealso cref="StringSchema"/>.
	/// </summary>
	public class StringSchemaTest
	{
		[Fact]
		public virtual void TestUtf8Charset()
		{
			StringSchema Schema = new StringSchema();
			ISchemaInfo Si = Schema.SchemaInfo;
			Assert.False(Si.Properties.ContainsKey(StringSchema.CHARSET_KEY));

			string MyString = "my string for test";
			sbyte[] Data = Schema.Encode(MyString);
			Assert.Equal(Data, (sbyte[])(object)Encoding.UTF8.GetBytes(MyString));

			string DecodedString = Schema.Decode(Data);
			Assert.Equal(DecodedString, MyString);
		}
		[Fact]
		public void TestAsciiCharset()
		{
			StringSchema Schema = new StringSchema(Encoding.ASCII);
			ISchemaInfo Si = Schema.SchemaInfo;
			Assert.True(Si.Properties.ContainsKey(StringSchema.CHARSET_KEY));
			Assert.Equal(Si.Properties[StringSchema.CHARSET_KEY], Encoding.ASCII.EncodingName);

			string MyString = "my string for test";
			sbyte[] Data = Schema.Encode(MyString);
			Assert.Equal(Data, (sbyte[])(object)Encoding.ASCII.GetBytes(MyString));

			string DecodedString = Schema.Decode(Data);
			Assert.Equal(DecodedString, MyString);
		}

		public virtual void TestSchemaInfoWithoutCharset()
		{
			ISchemaInfo Si = new SchemaInfo() 
			{ 			
				Name = "test-schema-info-without-charset",
				Type = SchemaType.STRING,
				Schema = new sbyte[0],
				Properties = new Dictionary<string, string>()
			};
			StringSchema Schema = StringSchema.FromSchemaInfo(Si);

			string MyString = "my string for test";
			sbyte[] Data = Schema.Encode(MyString);
			Assert.Equal(Data, (sbyte[])(object)Encoding.UTF8.GetBytes(MyString));

			string DecodedString = Schema.Decode(Data);
			Assert.Equal(DecodedString, MyString);
		}
		[Fact]
		public void TestSchemaInfoWithUtf8()
        {
			TestSchemaInfoWithCharset(Encoding.UTF8);
		}
		[Fact]
		public void TestSchemaInfoWithASCII()
        {
			TestSchemaInfoWithCharset(Encoding.ASCII);
		}
		[Fact]
		public void TestSchemaInfoWithUnicode()
        {
			TestSchemaInfoWithCharset(Encoding.Unicode);
		}
		private void TestSchemaInfoWithCharset(Encoding encoding)
		{
			IDictionary<string, string> properties = new Dictionary<string, string>();
			properties[StringSchema.CHARSET_KEY] = encoding.EncodingName;
			ISchemaInfo Si = new SchemaInfo() { 
			  Name = "test-schema-info-with-charset",
			  Type = SchemaType.STRING,
			  Schema = new sbyte[0],
			  Properties = properties
			};
			StringSchema Schema = StringSchema.FromSchemaInfo(Si);

			string MyString = "my string for test";
			sbyte[] Data = Schema.Encode(MyString);
			Assert.Equal(Data, (sbyte[])(object)encoding.GetBytes(MyString));

			string DecodedString = Schema.Decode(Data);
			Assert.Equal(DecodedString, MyString);
		}

	}

}