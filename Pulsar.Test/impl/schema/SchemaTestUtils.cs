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

	using Data = lombok.Data;
	using EqualsAndHashCode = lombok.EqualsAndHashCode;
	using ToString = lombok.ToString;
	using AvroDefault = apache.avro.reflect.AvroDefault;
	using Nullable = apache.avro.reflect.Nullable;

	/// <summary>
	/// Utils for testing avro.
	/// </summary>
	public class SchemaTestUtils
	{

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Data @ToString @EqualsAndHashCode public static class Foo
		public class Foo
		{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Nullable private String field1;
			internal string field1;
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Nullable private String field2;
			internal string field2;
			internal int field3;
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Nullable private Bar field4;
			internal Bar field4;
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Nullable private Color color;
			internal Color color;
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @AvroDefault("\"defaultValue\"") private String fieldUnableNull;
			internal string fieldUnableNull;
		}
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Data @ToString @EqualsAndHashCode public static class FooV2
		public class FooV2
		{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Nullable private String field1;
			internal string field1;
			internal int field3;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Data @ToString @EqualsAndHashCode public static class Bar
		public class Bar
		{
			internal bool field1;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Data @ToString @EqualsAndHashCode public static class NestedBar
		public class NestedBar
		{
			internal bool field1;
			internal Bar nested;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Data @ToString @EqualsAndHashCode public static class NestedBarList
		public class NestedBarList
		{
			internal bool field1;
			internal IList<Bar> list;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Data @ToString @EqualsAndHashCode public static class DerivedFoo extends Foo
		public class DerivedFoo : Foo
		{
			internal string field5;
			internal int field6;
			internal Foo foo;
		}

		public enum Color
		{
			RED,
			BLUE
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Data @ToString @EqualsAndHashCode public static class DerivedDerivedFoo extends DerivedFoo
		public class DerivedDerivedFoo : DerivedFoo
		{
			internal string field7;
			internal int field8;
			internal DerivedFoo derivedFoo;
			internal Foo foo2;
		}

		public const string SCHEMA_AVRO_NOT_ALLOW_NULL = "{\"type\":\"record\",\"name\":\"Foo\",\"namespace\":\"org.apache.pulsar.client.impl.schema.SchemaTestUtils\",\"fields\":[{\"name\":\"field1\",\"type\":[\"null\",\"string\"]," + "\"default\":null},{\"name\":\"field2\",\"type\":[\"null\",\"string\"],\"default\":null},{\"name\":\"field3\",\"type\":\"int\"},{\"name\":\"field4\",\"type\":[\"null\",{\"type\":" + "\"record\",\"name\":\"Bar\",\"fields\":[{\"name\":\"field1\",\"type\":\"boolean\"}]}],\"default\":null},{\"name\":\"color\",\"type\":[\"null\",{\"type\":\"enum\",\"name\":\"Color\"," + "\"symbols\":[\"RED\",\"BLUE\"]}],\"default\":null},{\"name\":\"fieldUnableNull\",\"type\":\"string\",\"default\":\"defaultValue\"}]}";

		public const string SCHEMA_AVRO_ALLOW_NULL = "{\"type\":\"record\",\"name\":\"Foo\",\"namespace\":\"org.apache.pulsar.client.impl.schema.SchemaTestUtils\",\"fields\":[{\"name\":\"field1\"," + "\"type\":[\"null\",\"string\"],\"default\":null},{\"name\":\"field2\",\"type\":[\"null\",\"string\"],\"default\":null},{\"name\":\"field3\",\"type\":\"int\"},{\"name\":\"field4\",\"type\":[\"" + "null\",{\"type\":\"record\",\"name\":\"Bar\",\"fields\":[{\"name\":\"field1\",\"type\":\"boolean\"}]}],\"default\":null},{\"name\":\"color\",\"type\":[\"null\",{\"type\":\"enum\",\"name\":\"Color\"" + ",\"symbols\":[\"RED\",\"BLUE\"]}],\"default\":null},{\"name\":\"fieldUnableNull\",\"type\":[\"null\",\"string\"],\"default\":\"defaultValue\"}]}";

		public const string SCHEMA_JSON_NOT_ALLOW_NULL = "{\"type\":\"record\",\"name\":\"Foo\",\"namespace\":\"org.apache.pulsar.client.impl.schema.SchemaTestUtils\",\"fields\":[{\"name\":\"field1\",\"type\":[\"null\",\"string\"],\"default\":null},{\"name\"" + ":\"field2\",\"type\":[\"null\",\"string\"],\"default\":null},{\"name\":\"field3\",\"type\":\"int\"},{\"name\":\"field4\",\"type\":[\"null\",{\"type\":\"record\",\"name\":\"Bar\",\"fields\":[{\"name\":\"" + "field1\",\"type\":\"boolean\"}]}],\"default\":null},{\"name\":\"color\",\"type\":[\"null\",{\"type\":\"enum\",\"name\":\"Color\",\"symbols\":[\"RED\",\"BLUE\"]}],\"default\":null},{\"name\":\"fieldUnableNull\"," + "\"type\":\"string\",\"default\":\"defaultValue\"}]}";
		public const string SCHEMA_JSON_ALLOW_NULL = "{\"type\":\"record\",\"name\":\"Foo\",\"namespace\":\"org.apache.pulsar.client.impl.schema.SchemaTestUtils\",\"fields\":[{\"name\":\"field1\",\"type\":[\"null\",\"string\"],\"default\":null}," + "{\"name\":\"field2\",\"type\":[\"null\",\"string\"],\"default\":null},{\"name\":\"field3\",\"type\":\"int\"},{\"name\":\"field4\",\"type\":[\"null\",{\"type\":\"record\",\"name\":\"Bar\",\"fields\":" + "[{\"name\":\"field1\",\"type\":\"boolean\"}]}],\"default\":null},{\"name\":\"color\",\"type\":[\"null\",{\"type\":\"enum\",\"name\":\"Color\",\"symbols\":[\"RED\",\"BLUE\"]}],\"default\":null},{\"name\":" + "\"fieldUnableNull\",\"type\":[\"null\",\"string\"],\"default\":\"defaultValue\"}]}";
		public const string KEY_VALUE_SCHEMA_INFO_INCLUDE_PRIMITIVE = "{\"key\":{\"type\":\"record\",\"name\":\"Foo\",\"namespace\":\"org.apache.pulsar.client.impl.schema.SchemaTestUtils\",\"fields\":[{\"name\":\"" + "field1\",\"type\":[\"null\",\"string\"],\"default\":null},{\"name\":\"field2\",\"type\":[\"null\",\"string\"],\"default\":null},{\"name\":\"field3\",\"type\":\"int\"},{\"name\":\"field4\",\"type\":[\"null\"," + "{\"type\":\"record\",\"name\":\"Bar\",\"fields\":[{\"name\":\"field1\",\"type\":\"boolean\"}]}],\"default\":null},{\"name\":\"color\",\"type\":[\"null\",{\"type\":\"enum\",\"name\":\"Color\",\"symbols\":[\"RED\"" + ",\"BLUE\"]}],\"default\":null},{\"name\":\"fieldUnableNull\",\"type\":[\"null\",\"string\"],\"default\":\"defaultValue\"}]},\"value\":\"\"}";
		public const string KEY_VALUE_SCHEMA_INFO_NOT_INCLUDE_PRIMITIVE = "{\"key\":{\"type\":\"record\",\"name\":\"Foo\",\"namespace\":\"org.apache.pulsar.client.impl.schema.SchemaTestUtils\",\"fields\":[{\"name\":\"field1\"" + ",\"type\":[\"null\",\"string\"],\"default\":null},{\"name\":\"field2\",\"type\":[\"null\",\"string\"],\"default\":null},{\"name\":\"field3\",\"type\":\"int\"},{\"name\":\"field4\",\"type\":[\"null\",{\"type\":\"record\"" + ",\"name\":\"Bar\",\"fields\":[{\"name\":\"field1\",\"type\":\"boolean\"}]}],\"default\":null},{\"name\":\"color\",\"type\":[\"null\",{\"type\":\"enum\",\"name\":\"Color\",\"symbols\":[\"RED\",\"BLUE\"]}],\"default\":null}," + "{\"name\":\"fieldUnableNull\",\"type\":[\"null\",\"string\"],\"default\":\"defaultValue\"}]},\"value\":{\"type\":\"record\",\"name\":\"Foo\",\"namespace\":\"org.apache.pulsar.client.impl.schema.SchemaTestUtils\",\"fields\":" + "[{\"name\":\"field1\",\"type\":[\"null\",\"string\"],\"default\":null},{\"name\":\"field2\",\"type\":[\"null\",\"string\"],\"default\":null},{\"name\":\"field3\",\"type\":\"int\"},{\"name\":\"field4\",\"type\":[\"null\"," + "{\"type\":\"record\",\"name\":\"Bar\",\"fields\":[{\"name\":\"field1\",\"type\":\"boolean\"}]}],\"default\":null},{\"name\":\"color\",\"type\":[\"null\",{\"type\":\"enum\",\"name\":\"Color\",\"symbols\":[\"RED\",\"BLUE\"]}]," + "\"default\":null},{\"name\":\"fieldUnableNull\",\"type\":[\"null\",\"string\"],\"default\":\"defaultValue\"}]}}";

		public static string[] FOO_FIELDS = new string[] {"field1", "field2", "field3", "field4", "color", "fieldUnableNull"};

		public static string TEST_MULTI_VERSION_SCHEMA_STRING = "TEST";

		public static string TEST_MULTI_VERSION_SCHEMA_DEFAULT_STRING = "defaultValue";

	}

}