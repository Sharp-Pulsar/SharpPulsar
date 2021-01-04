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
	/// Unit test <seealso cref="org.apache.pulsar.common.schema.SchemaInfo"/>.
	/// </summary>
	public class SchemaInfoTest
	{

		private const string INT32_SCHEMA_INFO = "{\n" + "  \"name\": \"INT32\",\n" + "  \"schema\": \"\",\n" + "  \"type\": \"INT32\",\n" + "  \"properties\": {}\n" + "}";

		private const string UTF8_SCHEMA_INFO = "{\n" + "  \"name\": \"String\",\n" + "  \"schema\": \"\",\n" + "  \"type\": \"STRING\",\n" + "  \"properties\": {}\n" + "}";

		private const string BAR_SCHEMA_INFO = "{\n" + "  \"name\": \"\",\n" + "  \"schema\": {\n" + "    \"type\": \"record\",\n" + "    \"name\": \"Bar\",\n" + "    \"namespace\": \"org.apache.pulsar.client.impl.schema.SchemaTestUtils\",\n" + "    \"fields\": [\n" + "      {\n" + "        \"name\": \"field1\",\n" + "        \"type\": \"boolean\"\n" + "      }\n" + "    ]\n" + "  },\n" + "  \"type\": \"JSON\",\n" + "  \"properties\": {\n" + "    \"__alwaysAllowNull\": \"true\",\n" + "    \"__jsr310ConversionEnabled\": \"false\",\n" + "    \"bar1\": \"bar-value1\",\n" + "    \"bar2\": \"bar-value2\",\n" + "    \"bar3\": \"bar-value3\"\n" + "  }\n" + "}";

		private const string FOO_SCHEMA_INFO = "{\n" + "  \"name\": \"\",\n" + "  \"schema\": {\n" + "    \"type\": \"record\",\n" + "    \"name\": \"Foo\",\n" + "    \"namespace\": \"org.apache.pulsar.client.impl.schema.SchemaTestUtils\",\n" + "    \"fields\": [\n" + "      {\n" + "        \"name\": \"field1\",\n" + "        \"type\": [\n" + "          \"null\",\n" + "          \"string\"\n" + "        ]\n" + "      },\n" + "      {\n" + "        \"name\": \"field2\",\n" + "        \"type\": [\n" + "          \"null\",\n" + "          \"string\"\n" + "        ]\n" + "      },\n" + "      {\n" + "        \"name\": \"field3\",\n" + "        \"type\": \"int\"\n" + "      },\n" + "      {\n" + "        \"name\": \"field4\",\n" + "        \"type\": [\n" + "          \"null\",\n" + "          {\n" + "            \"type\": \"record\",\n" + "            \"name\": \"Bar\",\n" + "            \"fields\": [\n" + "              {\n" + "                \"name\": \"field1\",\n" + "                \"type\": \"boolean\"\n" + "              }\n" + "            ]\n" + "          }\n" + "        ]\n" + "      },\n" + "      {\n" + "        \"name\": \"color\",\n" + "        \"type\": [\n" + "          \"null\",\n" + "          {\n" + "            \"type\": \"enum\",\n" + "            \"name\": \"Color\",\n" + "            \"symbols\": [\n" + "              \"RED\",\n" + "              \"BLUE\"\n" + "            ]\n" + "          }\n" + "        ]\n" + "      },\n" + "      {\n" + "        \"name\": \"fieldUnableNull\",\n" + "        \"type\": \"string\",\n" + "        \"default\": \"defaultValue\"\n" + "      }\n" + "    ]\n" + "  },\n" + "  \"type\": \"AVRO\",\n" + "  \"properties\": {\n" + "    \"__alwaysAllowNull\": \"false\",\n" + "    \"__jsr310ConversionEnabled\": \"false\",\n" + "    \"foo1\": \"foo-value1\",\n" + "    \"foo2\": \"foo-value2\",\n" + "    \"foo3\": \"foo-value3\"\n" + "  }\n" + "}";

		pr
		public static object[][] Schemas()
		{
			return new object[][]
			{
				new object[] {Schema.STRING.SchemaInfo, UTF8_SCHEMA_INFO},
				new object[] {Schema.INT32.SchemaInfo, INT32_SCHEMA_INFO},
				new object[] {KeyValueSchemaInfoTest.FooSchema.SchemaInfo, FOO_SCHEMA_INFO},
				new object[] {KeyValueSchemaInfoTest.BarSchema.SchemaInfo, BAR_SCHEMA_INFO},
				new object[] {Schema.KeyValue(KeyValueSchemaInfoTest.FooSchema, KeyValueSchemaInfoTest.BarSchema, KeyValueEncodingType.SEPARATED).SchemaInfo, KV_SCHEMA_INFO}
			};
		}

		public virtual void TestSchemaInfoToString(SchemaInfo Si, string JsonifiedStr)
		{
			assertEquals(Si.ToString(), JsonifiedStr);
		}

	}

}