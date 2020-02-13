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

	using Org.Apache.Pulsar.Client.Api;
	using KeyValueEncodingType = Org.Apache.Pulsar.Common.Schema.KeyValueEncodingType;
	using SchemaInfo = Org.Apache.Pulsar.Common.Schema.SchemaInfo;
	using DataProvider = org.testng.annotations.DataProvider;
	using Test = org.testng.annotations.Test;

	/// <summary>
	/// Unit test <seealso cref="org.apache.pulsar.common.schema.SchemaInfo"/>.
	/// </summary>
	public class SchemaInfoTest
	{

		private const string Int32SchemaInfo = "{\n" + @"  ""name"": ""INT32"",\n" + @"  ""schema"": """",\n" + @"  ""type"": ""INT32"",\n" + @"  ""properties"": {}\n" + "}";

		private const string Utf8SchemaInfo = "{\n" + @"  ""name"": ""String"",\n" + @"  ""schema"": """",\n" + @"  ""type"": ""STRING"",\n" + @"  ""properties"": {}\n" + "}";

		private const string BarSchemaInfo = "{\n" + @"  ""name"": """",\n" + @"  ""schema"": {\n" + @"    ""type"": ""record"",\n" + @"    ""name"": ""Bar"",\n" + @"    ""namespace"": ""org.apache.pulsar.client.impl.schema.SchemaTestUtils$"",\n" + @"    ""fields"": [\n" + "      {\n" + @"        ""name"": ""field1"",\n" + @"        ""type"": ""boolean""\n" + "      }\n" + "    ]\n" + "  },\n" + @"  ""type"": ""JSON"",\n" + @"  ""properties"": {\n" + @"    ""__alwaysAllowNull"": ""true"",\n" + @"    ""bar1"": ""bar-value1"",\n" + @"    ""bar2"": ""bar-value2"",\n" + @"    ""bar3"": ""bar-value3""\n" + "  }\n" + "}";

		private const string FooSchemaInfo = "{\n" + @"  ""name"": """",\n" + @"  ""schema"": {\n" + @"    ""type"": ""record"",\n" + @"    ""name"": ""Foo"",\n" + @"    ""namespace"": ""org.apache.pulsar.client.impl.schema.SchemaTestUtils$"",\n" + @"    ""fields"": [\n" + "      {\n" + @"        ""name"": ""field1"",\n" + @"        ""type"": [\n" + @"          ""null"",\n" + @"          ""string""\n" + "        ]\n" + "      },\n" + "      {\n" + @"        ""name"": ""field2"",\n" + @"        ""type"": [\n" + @"          ""null"",\n" + @"          ""string""\n" + "        ]\n" + "      },\n" + "      {\n" + @"        ""name"": ""field3"",\n" + @"        ""type"": ""int""\n" + "      },\n" + "      {\n" + @"        ""name"": ""field4"",\n" + @"        ""type"": [\n" + @"          ""null"",\n" + "          {\n" + @"            ""type"": ""record"",\n" + @"            ""name"": ""Bar"",\n" + @"            ""fields"": [\n" + "              {\n" + @"                ""name"": ""field1"",\n" + @"                ""type"": ""boolean""\n" + "              }\n" + "            ]\n" + "          }\n" + "        ]\n" + "      },\n" + "      {\n" + @"        ""name"": ""color"",\n" + @"        ""type"": [\n" + @"          ""null"",\n" + "          {\n" + @"            ""type"": ""enum"",\n" + @"            ""name"": ""Color"",\n" + @"            ""symbols"": [\n" + @"              ""RED"",\n" + @"              ""BLUE""\n" + "            ]\n" + "          }\n" + "        ]\n" + "      },\n" + "      {\n" + @"        ""name"": ""fieldUnableNull"",\n" + @"        ""type"": ""string"",\n" + @"        ""default"": ""defaultValue""\n" + "      }\n" + "    ]\n" + "  },\n" + @"  ""type"": ""AVRO"",\n" + @"  ""properties"": {\n" + @"    ""__alwaysAllowNull"": ""false"",\n" + @"    ""foo1"": ""foo-value1"",\n" + @"    ""foo2"": ""foo-value2"",\n" + @"    ""foo3"": ""foo-value3""\n" + "  }\n" + "}";

		private const string KvSchemaInfo = "{\n" + @"  ""name"": ""KeyValue"",\n" + @"  ""schema"": {\n" + @"    ""key"": {\n" + @"      ""name"": """",\n" + @"      ""schema"": {\n" + @"        ""type"": ""record"",\n" + @"        ""name"": ""Foo"",\n" + @"        ""namespace"": ""org.apache.pulsar.client.impl.schema.SchemaTestUtils$"",\n" + @"        ""fields"": [\n" + "          {\n" + @"            ""name"": ""field1"",\n" + @"            ""type"": [\n" + @"              ""null"",\n" + @"              ""string""\n" + "            ]\n" + "          },\n" + "          {\n" + @"            ""name"": ""field2"",\n" + @"            ""type"": [\n" + @"              ""null"",\n" + @"              ""string""\n" + "            ]\n" + "          },\n" + "          {\n" + @"            ""name"": ""field3"",\n" + @"            ""type"": ""int""\n" + "          },\n" + "          {\n" + @"            ""name"": ""field4"",\n" + @"            ""type"": [\n" + @"              ""null"",\n" + "              {\n" + @"                ""type"": ""record"",\n" + @"                ""name"": ""Bar"",\n" + @"                ""fields"": [\n" + "                  {\n" + @"                    ""name"": ""field1"",\n" + @"                    ""type"": ""boolean""\n" + "                  }\n" + "                ]\n" + "              }\n" + "            ]\n" + "          },\n" + "          {\n" + @"            ""name"": ""color"",\n" + @"            ""type"": [\n" + @"              ""null"",\n" + "              {\n" + @"                ""type"": ""enum"",\n" + @"                ""name"": ""Color"",\n" + @"                ""symbols"": [\n" + @"                  ""RED"",\n" + @"                  ""BLUE""\n" + "                ]\n" + "              }\n" + "            ]\n" + "          },\n" + "          {\n" + @"            ""name"": ""fieldUnableNull"",\n" + @"            ""type"": ""string"",\n" + @"            ""default"": ""defaultValue""\n" + "          }\n" + "        ]\n" + "      },\n" + @"      ""type"": ""AVRO"",\n" + @"      ""properties"": {\n" + @"        ""__alwaysAllowNull"": ""false"",\n" + @"        ""foo1"": ""foo-value1"",\n" + @"        ""foo2"": ""foo-value2"",\n" + @"        ""foo3"": ""foo-value3""\n" + "      }\n" + "    },\n" + @"    ""value"": {\n" + @"      ""name"": """",\n" + @"      ""schema"": {\n" + @"        ""type"": ""record"",\n" + @"        ""name"": ""Bar"",\n" + @"        ""namespace"": ""org.apache.pulsar.client.impl.schema.SchemaTestUtils$"",\n" + @"        ""fields"": [\n" + "          {\n" + @"            ""name"": ""field1"",\n" + @"            ""type"": ""boolean""\n" + "          }\n" + "        ]\n" + "      },\n" + @"      ""type"": ""JSON"",\n" + @"      ""properties"": {\n" + @"        ""__alwaysAllowNull"": ""true"",\n" + @"        ""bar1"": ""bar-value1"",\n" + @"        ""bar2"": ""bar-value2"",\n" + @"        ""bar3"": ""bar-value3""\n" + "      }\n" + "    }\n" + "  },\n" + @"  ""type"": ""KEY_VALUE"",\n" + @"  ""properties"": {\n" + @"    ""key.schema.name"": """",\n" + @"    ""key.schema.properties"": "@"{"""__alwaysAllowNull\"@":"""false\"@","""foo1\"@":"""foo-value1\"@","""foo2\"@":"""foo-value2\"@","""foo3\"@":"""foo-value3\""}"",\n" + @"    ""key.schema.type"": ""AVRO"",\n" + @"    ""kv.encoding.type"": ""SEPARATED"",\n" + @"    ""value.schema.name"": """",\n" + @"    ""value.schema.properties"": "@"{"""__alwaysAllowNull\"@":"""true\"@","""bar1\"@":"""bar-value1\"@","""bar2\"@":"""bar-value2\"@","""bar3\"@":"""bar-value3\""}"",\n" + @"    ""value.schema.type"": ""JSON""\n" + "  }\n" + "}";

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @DataProvider(name = "schemas") public Object[][] schemas()
		public virtual object[][] Schemas()
		{
			return new object[][]
			{
				new object[] {SchemaFields.STRING.SchemaInfo, Utf8SchemaInfo},
				new object[] {SchemaFields.INT32.SchemaInfo, Int32SchemaInfo},
				new object[] {KeyValueSchemaInfoTest.FooSchema.SchemaInfo, FooSchemaInfo},
				new object[] {KeyValueSchemaInfoTest.BarSchema.SchemaInfo, BarSchemaInfo},
				new object[] {Schema.KeyValue(KeyValueSchemaInfoTest.FooSchema, KeyValueSchemaInfoTest.BarSchema, KeyValueEncodingType.SEPARATED).SchemaInfo, KvSchemaInfo}
			};
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(dataProvider = "schemas") public void testSchemaInfoToString(org.apache.pulsar.common.schema.SchemaInfo si, String jsonifiedStr)
		public virtual void TestSchemaInfoToString(SchemaInfo Si, string JsonifiedStr)
		{
			assertEquals(Si.ToString(), JsonifiedStr);
		}

	}

}