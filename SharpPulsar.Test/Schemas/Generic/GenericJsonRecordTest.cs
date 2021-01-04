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
namespace Org.Apache.Pulsar.Client.Impl.Schema.Generic
{
	using JsonNode = com.fasterxml.jackson.databind.JsonNode;
	using ObjectMapper = com.fasterxml.jackson.databind.ObjectMapper;
	using Test = org.testng.annotations.Test;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.testng.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.testng.Assert.assertTrue;


	public class GenericJsonRecordTest
	{

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void decodeLongField() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		public virtual void DecodeLongField()
		{
			string JsonStr = "{\"timestamp\":1585204833128, \"count\":2, \"value\": 1.1, \"on\":true}";
			sbyte[] JsonStrBytes = JsonStr.GetBytes();
			ObjectMapper ObjectMapper = new ObjectMapper();
			JsonNode Jn = ObjectMapper.readTree(StringHelper.NewString(JsonStrBytes, 0, JsonStrBytes.Length, UTF_8));
			GenericJsonRecord Record = new GenericJsonRecord(null, Collections.emptyList(), Jn);

			object LongValue = Record.getField("timestamp");
			assertTrue(LongValue is long?);
			assertEquals(1585204833128L, LongValue);

			object IntValue = Record.getField("count");
			assertTrue(IntValue is int?);
			assertEquals(2, IntValue);

			object Value = Record.getField("value");
			assertTrue(Value is double?);
			assertEquals(1.1, Value);

			object BoolValue = Record.getField("on");
			assertTrue((bool)BoolValue);
		}
	}
}