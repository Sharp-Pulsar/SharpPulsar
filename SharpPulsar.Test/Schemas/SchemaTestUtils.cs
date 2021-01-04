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
namespace SharpPulsar.Test.Schema
{

	/// <summary>
	/// Utils for testing avro.
	/// </summary>
	public class SchemaTestUtils
	{
		public class Foo
		{
			internal string Field1;
			internal string Field2;
			internal int Field3;
			internal Bar Field4;
			internal Color Color;
			internal string FieldUnableNull;
		}

		public class FooV2
		{

			internal string Field1;
			internal int Field3;
		}

		public class Bar
		{
			internal bool Field1;
		}

		public class NestedBar
		{
			internal bool Field1;
			internal Bar Nested;
		}

		public class NestedBarList
		{
			internal bool Field1;
			internal IList<Bar> List;
		}

		public class DerivedFoo : Foo
		{
			internal string Field5;
			internal int Field6;
			internal Foo Foo;
		}

		public enum Color
		{
			RED,
			BLUE
		}

		public class DerivedDerivedFoo : DerivedFoo
		{
			internal string Field7;
			internal int Field8;
			internal DerivedFoo DerivedFoo;
			internal Foo Foo2;
		}

		public const string SCHEMA_AVRO_NOT_ALLOW_NULL = "{\"type\":\"record\",\"name\":\"Foo\",\"namespace\":\"org.apache.pulsar.client.impl.schema.SchemaTestUtils\",\"fields\":[{\"name\":\"field1\",\"type\":[\"null\",\"string\"]," + "\"default\":null},{\"name\":\"field2\",\"type\":[\"null\",\"string\"],\"default\":null},{\"name\":\"field3\",\"type\":\"int\"},{\"name\":\"field4\",\"type\":[\"null\",{\"type\":" + "\"record\",\"name\":\"Bar\",\"fields\":[{\"name\":\"field1\",\"type\":\"boolean\"}]}],\"default\":null},{\"name\":\"color\",\"type\":[\"null\",{\"type\":\"enum\",\"name\":\"Color\"," + "\"symbols\":[\"RED\",\"BLUE\"]}],\"default\":null},{\"name\":\"fieldUnableNull\",\"type\":\"string\",\"default\":\"defaultValue\"}]}";

		public const string SCHEMA_AVRO_ALLOW_NULL = "{\"type\":\"record\",\"name\":\"Foo\",\"namespace\":\"org.apache.pulsar.client.impl.schema.SchemaTestUtils\",\"fields\":[{\"name\":\"field1\"," + "\"type\":[\"null\",\"string\"],\"default\":null},{\"name\":\"field2\",\"type\":[\"null\",\"string\"],\"default\":null},{\"name\":\"field3\",\"type\":\"int\"},{\"name\":\"field4\",\"type\":[\"" + "null\",{\"type\":\"record\",\"name\":\"Bar\",\"fields\":[{\"name\":\"field1\",\"type\":\"boolean\"}]}],\"default\":null},{\"name\":\"color\",\"type\":[\"null\",{\"type\":\"enum\",\"name\":\"Color\"" + ",\"symbols\":[\"RED\",\"BLUE\"]}],\"default\":null},{\"name\":\"fieldUnableNull\",\"type\":[\"null\",\"string\"],\"default\":\"defaultValue\"}]}";

		public const string SCHEMA_JSON_NOT_ALLOW_NULL = "{\"type\":\"record\",\"name\":\"Foo\",\"namespace\":\"org.apache.pulsar.client.impl.schema.SchemaTestUtils\",\"fields\":[{\"name\":\"field1\",\"type\":[\"null\",\"string\"],\"default\":null},{\"name\"" + ":\"field2\",\"type\":[\"null\",\"string\"],\"default\":null},{\"name\":\"field3\",\"type\":\"int\"},{\"name\":\"field4\",\"type\":[\"null\",{\"type\":\"record\",\"name\":\"Bar\",\"fields\":[{\"name\":\"" + "field1\",\"type\":\"boolean\"}]}],\"default\":null},{\"name\":\"color\",\"type\":[\"null\",{\"type\":\"enum\",\"name\":\"Color\",\"symbols\":[\"RED\",\"BLUE\"]}],\"default\":null},{\"name\":\"fieldUnableNull\"," + "\"type\":\"string\",\"default\":\"defaultValue\"}]}";
		public const string SCHEMA_JSON_ALLOW_NULL = "{\"type\":\"record\",\"name\":\"Foo\",\"namespace\":\"org.apache.pulsar.client.impl.schema.SchemaTestUtils\",\"fields\":[{\"name\":\"field1\",\"type\":[\"null\",\"string\"],\"default\":null}," + "{\"name\":\"field2\",\"type\":[\"null\",\"string\"],\"default\":null},{\"name\":\"field3\",\"type\":\"int\"},{\"name\":\"field4\",\"type\":[\"null\",{\"type\":\"record\",\"name\":\"Bar\",\"fields\":" + "[{\"name\":\"field1\",\"type\":\"boolean\"}]}],\"default\":null},{\"name\":\"color\",\"type\":[\"null\",{\"type\":\"enum\",\"name\":\"Color\",\"symbols\":[\"RED\",\"BLUE\"]}],\"default\":null},{\"name\":" + "\"fieldUnableNull\",\"type\":[\"null\",\"string\"],\"default\":\"defaultValue\"}]}";
		public const string KEY_VALUE_SCHEMA_INFO_INCLUDE_PRIMITIVE = "{\"key\":{\"type\":\"record\",\"name\":\"Foo\",\"namespace\":\"org.apache.pulsar.client.impl.schema.SchemaTestUtils\",\"fields\":[{\"name\":\"" + "field1\",\"type\":[\"null\",\"string\"],\"default\":null},{\"name\":\"field2\",\"type\":[\"null\",\"string\"],\"default\":null},{\"name\":\"field3\",\"type\":\"int\"},{\"name\":\"field4\",\"type\":[\"null\"," + "{\"type\":\"record\",\"name\":\"Bar\",\"fields\":[{\"name\":\"field1\",\"type\":\"boolean\"}]}],\"default\":null},{\"name\":\"color\",\"type\":[\"null\",{\"type\":\"enum\",\"name\":\"Color\",\"symbols\":[\"RED\"" + ",\"BLUE\"]}],\"default\":null},{\"name\":\"fieldUnableNull\",\"type\":[\"null\",\"string\"],\"default\":\"defaultValue\"}]},\"value\":\"\"}";
		public const string KEY_VALUE_SCHEMA_INFO_NOT_INCLUDE_PRIMITIVE = "{\"key\":{\"type\":\"record\",\"name\":\"Foo\",\"namespace\":\"org.apache.pulsar.client.impl.schema.SchemaTestUtils\",\"fields\":[{\"name\":\"field1\"" + ",\"type\":[\"null\",\"string\"],\"default\":null},{\"name\":\"field2\",\"type\":[\"null\",\"string\"],\"default\":null},{\"name\":\"field3\",\"type\":\"int\"},{\"name\":\"field4\",\"type\":[\"null\",{\"type\":\"record\"" + ",\"name\":\"Bar\",\"fields\":[{\"name\":\"field1\",\"type\":\"boolean\"}]}],\"default\":null},{\"name\":\"color\",\"type\":[\"null\",{\"type\":\"enum\",\"name\":\"Color\",\"symbols\":[\"RED\",\"BLUE\"]}],\"default\":null}," + "{\"name\":\"fieldUnableNull\",\"type\":[\"null\",\"string\"],\"default\":\"defaultValue\"}]},\"value\":{\"type\":\"record\",\"name\":\"Foo\",\"namespace\":\"org.apache.pulsar.client.impl.schema.SchemaTestUtils\",\"fields\":" + "[{\"name\":\"field1\",\"type\":[\"null\",\"string\"],\"default\":null},{\"name\":\"field2\",\"type\":[\"null\",\"string\"],\"default\":null},{\"name\":\"field3\",\"type\":\"int\"},{\"name\":\"field4\",\"type\":[\"null\"," + "{\"type\":\"record\",\"name\":\"Bar\",\"fields\":[{\"name\":\"field1\",\"type\":\"boolean\"}]}],\"default\":null},{\"name\":\"color\",\"type\":[\"null\",{\"type\":\"enum\",\"name\":\"Color\",\"symbols\":[\"RED\",\"BLUE\"]}]," + "\"default\":null},{\"name\":\"fieldUnableNull\",\"type\":[\"null\",\"string\"],\"default\":\"defaultValue\"}]}}";

		public static string[] FooFields = new string[] {"field1", "field2", "field3", "field4", "color", "fieldUnableNull"};

		public static string TestMultiVersionSchemaString = "TEST";

		public static string TestMultiVersionSchemaDefaultString = "defaultValue";

	}

}