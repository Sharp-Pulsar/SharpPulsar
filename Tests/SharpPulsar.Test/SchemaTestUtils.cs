using System;
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
namespace SharpPulsar.Test
{

	/// <summary>
	/// Utils for testing avro.
	/// </summary>
	public class SchemaTestUtils
	{

		[Serializable]
		public class Foo : IEquatable<Foo>
		{

			public string Field1 { get; set; }
			public string Field2 { get; set; }
			public int Field3 { get; set; }

			public Bar Field4 { get; set; }

			public Color Color { get; set; }

			public string FieldUnableNull { get; set; }
			public bool Equals(Foo other)
			{
				if (Field1 == other.Field1 && Field2 == other.Field2 && Field3 == other.Field3
					&& Field4?.Field1 == other.Field4?.Field1 && Color == other.Color && FieldUnableNull == other.FieldUnableNull)
					return true;
				return false;
			}
		}

		[Serializable]
		public class FooV2: IEquatable<FooV2>
		{
			public string Field1 { get; set; }
			public int Field3 { get; set; }
			public bool Equals(FooV2 other)
			{
				return Field1 == other.Field1 && Field3 == other.Field3;
			}
		}


		[Serializable]
		public class Bar : IEquatable<Bar>
		{
			public bool Field1 { get; set; }

			public bool Equals(Bar other)
			{
				if (Field1 == other.Field1)
					return true;
				return false;
			}
		}

		public class NestedBar
		{
			public bool Field1 { get; set; }
			public Bar Nested { get; set; }
		}


		public class NestedBarList
		{
			public bool Field1 { get; set; }
			public IList<Bar> List { get; set; }
		}


		public class DerivedFoo : Foo, IEquatable<DerivedFoo>
		{
			public string Field5 { get; set; }
			public int Field6 { get; set; }
			public Foo Foo { get; set; }
			public bool Equals(DerivedFoo other)
			{
				return Field5 == other.Field5 && Field6 == other.Field6 && Foo == other.Foo;
			}
		}

		public enum Color
		{
			RED,
			BLUE
		}

		public class DerivedDerivedFoo : DerivedFoo
		{
			public string Field7 { get; set; }
			public int Field8 { get; set; }
			public DerivedFoo DerivedFoo { get; set; }
			public Foo Foo2 { get; set; }
		}

		public const string SchemaAvroNotAllowNull = @"{""type"":""record"",""name"":""Foo"",""namespace"":""org.apache.pulsar.client.impl.schema.SchemaTestUtils"",""fields"":[{""name"":""field1"",""type"":[""null"",""string""]," + @"""default"":null},{""name"":""field2"",""type"":[""null"",""string""],""default"":null},{""name"":""field3"",""type"":""int""},{""name"":""field4"",""type"":[""null"",{""type"":" + @"""record"",""name"":""Bar"",""fields"":[{""name"":""field1"",""type"":""boolean""}]}],""default"":null},{""name"":""color"",""type"":[""null"",{""type"":""enum"",""name"":""Color""," + @"""symbols"":[""RED"",""BLUE""]}],""default"":null},{""name"":""fieldUnableNull"",""type"":""string"",""default"":""defaultValue""}]}";

		public const string SchemaAvroAllowNull = @"{""type"":""record"",""name"":""Foo"",""namespace"":""org.apache.pulsar.client.impl.schema.SchemaTestUtils"",""fields"":[{""name"":""field1""," + @"""type"":[""null"",""string""],""default"":null},{""name"":""field2"",""type"":[""null"",""string""],""default"":null},{""name"":""field3"",""type"":""int""},{""name"":""field4"",""type"":[""" + @"null"",{""type"":""record"",""name"":""Bar"",""fields"":[{""name"":""field1"",""type"":""boolean""}]}],""default"":null},{""name"":""color"",""type"":[""null"",{""type"":""enum"",""name"":""Color""" + @",""symbols"":[""RED"",""BLUE""]}],""default"":null},{""name"":""fieldUnableNull"",""type"":[""null"",""string""],""default"":""defaultValue""}]}";

		public const string SchemaJsonNotAllowNull = @"{""type"":""record"",""name"":""Foo"",""namespace"":""org.apache.pulsar.client.impl.schema.SchemaTestUtils"",""fields"":[{""name"":""field1"",""type"":[""null"",""string""],""default"":null},{""name""" + @":""field2"",""type"":[""null"",""string""],""default"":null},{""name"":""field3"",""type"":""int""},{""name"":""field4"",""type"":[""null"",{""type"":""record"",""name"":""Bar"",""fields"":[{""name"":""" + @"field1"",""type"":""boolean""}]}],""default"":null},{""name"":""color"",""type"":[""null"",{""type"":""enum"",""name"":""Color"",""symbols"":[""RED"",""BLUE""]}],""default"":null},{""name"":""fieldUnableNull""," + @"""type"":""string"",""default"":""defaultValue""}]}";
		public const string SchemaJsonAllowNull = @"{""type"":""record"",""name"":""Foo"",""namespace"":""org.apache.pulsar.client.impl.schema.SchemaTestUtils"",""fields"":[{""name"":""field1"",""type"":[""null"",""string""],""default"":null}," + @"{""name"":""field2"",""type"":[""null"",""string""],""default"":null},{""name"":""field3"",""type"":""int""},{""name"":""field4"",""type"":[""null"",{""type"":""record"",""name"":""Bar"",""fields"":" + @"[{""name"":""field1"",""type"":""boolean""}]}],""default"":null},{""name"":""color"",""type"":[""null"",{""type"":""enum"",""name"":""Color"",""symbols"":[""RED"",""BLUE""]}],""default"":null},{""name"":" + @"""fieldUnableNull"",""type"":[""null"",""string""],""default"":""defaultValue""}]}";
		public const string KeyValueSchemaInfoIncludePrimitive = @"{""key"":{""type"":""record"",""name"":""Foo"",""namespace"":""org.apache.pulsar.client.impl.schema.SchemaTestUtils"",""fields"":[{""name"":""" + @"field1"",""type"":[""null"",""string""],""default"":null},{""name"":""field2"",""type"":[""null"",""string""],""default"":null},{""name"":""field3"",""type"":""int""},{""name"":""field4"",""type"":[""null""," + @"{""type"":""record"",""name"":""Bar"",""fields"":[{""name"":""field1"",""type"":""boolean""}]}],""default"":null},{""name"":""color"",""type"":[""null"",{""type"":""enum"",""name"":""Color"",""symbols"":[""RED""" + @",""BLUE""]}],""default"":null},{""name"":""fieldUnableNull"",""type"":[""null"",""string""],""default"":""defaultValue""}]},""value"":""""}";
		public const string KeyValueSchemaInfoNotIncludePrimitive = @"{""key"":{""type"":""record"",""name"":""Foo"",""namespace"":""org.apache.pulsar.client.impl.schema.SchemaTestUtils"",""fields"":[{""name"":""field1""" + @",""type"":[""null"",""string""],""default"":null},{""name"":""field2"",""type"":[""null"",""string""],""default"":null},{""name"":""field3"",""type"":""int""},{""name"":""field4"",""type"":[""null"",{""type"":""record""" + @",""name"":""Bar"",""fields"":[{""name"":""field1"",""type"":""boolean""}]}],""default"":null},{""name"":""color"",""type"":[""null"",{""type"":""enum"",""name"":""Color"",""symbols"":[""RED"",""BLUE""]}],""default"":null}," + @"{""name"":""fieldUnableNull"",""type"":[""null"",""string""],""default"":""defaultValue""}]},""value"":{""type"":""record"",""name"":""Foo"",""namespace"":""org.apache.pulsar.client.impl.schema.SchemaTestUtils"",""fields"":" + @"[{""name"":""field1"",""type"":[""null"",""string""],""default"":null},{""name"":""field2"",""type"":[""null"",""string""],""default"":null},{""name"":""field3"",""type"":""int""},{""name"":""field4"",""type"":[""null""," + @"{""type"":""record"",""name"":""Bar"",""fields"":[{""name"":""field1"",""type"":""boolean""}]}],""default"":null},{""name"":""color"",""type"":[""null"",{""type"":""enum"",""name"":""Color"",""symbols"":[""RED"",""BLUE""]}]," + @"""default"":null},{""name"":""fieldUnableNull"",""type"":[""null"",""string""],""default"":""defaultValue""}]}}";

		public static string[] FooFields = new string[] {"field1", "field2", "field3", "field4", "color", "fieldUnableNull"};

		public static string TestMultiVersionSchemaString = "TEST";

		public static string TestMultiVersionSchemaDefaultString = "defaultValue";

	}

}