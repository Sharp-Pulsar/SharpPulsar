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
namespace Org.Apache.Pulsar.Client.Impl.Schema
{
	using JsonProcessingException = com.fasterxml.jackson.core.JsonProcessingException;
	using ObjectMapper = com.fasterxml.jackson.databind.ObjectMapper;
	using ByteBuf = io.netty.buffer.ByteBuf;
	using ByteBufAllocator = io.netty.buffer.ByteBufAllocator;
	using Slf4j = lombok.@extern.slf4j.Slf4j;
	using Schema = org.apache.avro.Schema;
	using SchemaType = org.apache.pulsar.common.schema.SchemaType;
	using Function = org.apache.pulsar.functions.proto.Function;
	using Assert = org.testng.Assert;
	using Test = org.testng.annotations.Test;


//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Slf4j public class ProtobufSchemaTest
	public class ProtobufSchemaTest
	{

		private const string NAME = "foo";

		private const string EXPECTED_SCHEMA_JSON = "{\"type\":\"record\",\"name\":\"TestMessage\"," + "\"namespace\":\"org.apache.pulsar.client.schema.proto.Test\",\"fields\":[{\"name\":\"stringField\"," + "\"type\":{\"type\":\"string\",\"avro.java.string\":\"String\"},\"default\":\"\"}," + "{\"name\":\"doubleField\",\"type\":\"double\",\"default\":0},{\"name\":\"intField\"," + "\"type\":\"int\",\"default\":0},{\"name\":\"testEnum\",\"type\":{\"type\":\"enum\",\"name\":\"TestEnum\"," + "\"symbols\":[\"SHARED\",\"FAILOVER\"]},\"default\":\"SHARED\"},{\"name\":\"nestedField\",\"type\":[\"null\"," + "{\"type\":\"record\",\"name\":\"SubMessage\",\"fields\":[{\"name\":\"foo\",\"type\":{\"type\":\"string\"," + "\"avro.java.string\":\"String\"},\"default\":\"\"},{\"name\":\"bar\",\"type\":\"double\",\"default\":0}]}]," + "\"default\":null},{\"name\":\"repeatedField\",\"type\":{\"type\":\"array\",\"items\":{\"type\":\"string\"," + "\"avro.java.string\":\"String\"}}},{\"name\":\"externalMessage\",\"type\":[\"null\",{\"type\":\"record\"," + "\"name\":\"ExternalMessage\",\"namespace\":\"org.apache.pulsar.client.schema.proto.ExternalTest\"," + "\"fields\":[{\"name\":\"stringField\",\"type\":{\"type\":\"string\",\"avro.java.string\":\"String\"}," + "\"default\":\"\"},{\"name\":\"doubleField\",\"type\":\"double\",\"default\":0}]}],\"default\":null}]}";

		private const string EXPECTED_PARSING_INFO = "{\"__alwaysAllowNull\":\"true\",\"__jsr310ConversionEnabled\":\"false\"," + "\"__PARSING_INFO__\":\"[{\\\"number\\\":1,\\\"name\\\":\\\"stringField\\\",\\\"type\\\":\\\"STRING\\\"," + "\\\"label\\\":\\\"LABEL_OPTIONAL\\\",\\\"definition\\\":null},{\\\"number\\\":2,\\\"name\\\":\\\"doubleField\\\"," + "\\\"type\\\":\\\"DOUBLE\\\",\\\"label\\\":\\\"LABEL_OPTIONAL\\\",\\\"definition\\\":null},{\\\"number\\\":6," + "\\\"name\\\":\\\"intField\\\",\\\"type\\\":\\\"INT32\\\",\\\"label\\\":\\\"LABEL_OPTIONAL\\\",\\\"definition\\\":null}," + "{\\\"number\\\":4,\\\"name\\\":\\\"testEnum\\\",\\\"type\\\":\\\"ENUM\\\",\\\"label\\\":\\\"LABEL_OPTIONAL\\\"," + "\\\"definition\\\":null},{\\\"number\\\":5,\\\"name\\\":\\\"nestedField\\\",\\\"type\\\":\\\"MESSAGE\\\"," + "\\\"label\\\":\\\"LABEL_OPTIONAL\\\",\\\"definition\\\":null},{\\\"number\\\":10,\\\"name\\\":\\\"repeatedField\\\"," + "\\\"type\\\":\\\"STRING\\\",\\\"label\\\":\\\"LABEL_REPEATED\\\",\\\"definition\\\":null},{\\\"number\\\":11," + "\\\"name\\\":\\\"externalMessage\\\",\\\"type\\\":\\\"MESSAGE\\\",\\\"label\\\":\\\"LABEL_OPTIONAL\\\",\\\"definition\\\":null}]\"}";

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testEncodeAndDecode()
		public virtual void TestEncodeAndDecode()
		{
			Function.FunctionDetails FunctionDetails = Function.FunctionDetails.newBuilder().setName(NAME).build();

			ProtobufSchema<Function.FunctionDetails> ProtobufSchema = ProtobufSchema.of(typeof(Function.FunctionDetails));

			sbyte[] Bytes = ProtobufSchema.encode(FunctionDetails);

			Function.FunctionDetails Message = ProtobufSchema.decode(Bytes);

			Assert.assertEquals(Message.Name, NAME);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testSchema()
		public virtual void TestSchema()
		{
			ProtobufSchema<org.apache.pulsar.client.schema.proto.Test.TestMessage> ProtobufSchema = ProtobufSchema.of(typeof(org.apache.pulsar.client.schema.proto.Test.TestMessage));

			Assert.assertEquals(ProtobufSchema.SchemaInfo.Type, SchemaType.PROTOBUF);

			string SchemaJson = new string(ProtobufSchema.SchemaInfo.Schema);
			Schema.Parser Parser = new Schema.Parser();
			Schema Schema = Parser.parse(SchemaJson);

			Assert.assertEquals(Schema.ToString(), EXPECTED_SCHEMA_JSON);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testGenericOf()
		public virtual void TestGenericOf()
		{
			try
			{
				ProtobufSchema<org.apache.pulsar.client.schema.proto.Test.TestMessage> ProtobufSchema = ProtobufSchema.ofGenericClass(typeof(org.apache.pulsar.client.schema.proto.Test.TestMessage), new Dictionary<org.apache.pulsar.client.schema.proto.Test.TestMessage>());
			}
			catch (Exception)
			{
				Assert.fail("Should not construct a ProtobufShema over a non-protobuf-generated class");
			}

			try
			{
				ProtobufSchema<org.apache.pulsar.client.schema.proto.Test.TestMessage> ProtobufSchema = ProtobufSchema.ofGenericClass(typeof(string), Collections.emptyMap());
				Assert.fail("Should not construct a ProtobufShema over a non-protobuf-generated class");
			}
			catch (Exception)
			{

			}
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testParsingInfoProperty() throws com.fasterxml.jackson.core.JsonProcessingException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		public virtual void TestParsingInfoProperty()
		{
			ProtobufSchema<org.apache.pulsar.client.schema.proto.Test.TestMessage> ProtobufSchema = ProtobufSchema.of(typeof(org.apache.pulsar.client.schema.proto.Test.TestMessage));

			Assert.assertEquals((new ObjectMapper()).writeValueAsString(ProtobufSchema.SchemaInfo.Properties), EXPECTED_PARSING_INFO);

		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testDecodeByteBuf() throws com.fasterxml.jackson.core.JsonProcessingException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		public virtual void TestDecodeByteBuf()
		{
			ProtobufSchema<org.apache.pulsar.client.schema.proto.Test.TestMessage> ProtobufSchema = ProtobufSchema.of(typeof(org.apache.pulsar.client.schema.proto.Test.TestMessage));
			org.apache.pulsar.client.schema.proto.Test.TestMessage TestMessage = org.apache.pulsar.client.schema.proto.Test.TestMessage.newBuilder().build();
			sbyte[] Bytes = ProtobufSchema.encode(org.apache.pulsar.client.schema.proto.Test.TestMessage.newBuilder().build());
			ByteBuf ByteBuf = ByteBufAllocator.DEFAULT.buffer(Bytes.Length);
			ByteBuf.writeBytes(Bytes);

			Assert.assertEquals(TestMessage, ProtobufSchema.decode(ByteBuf));

		}
	}

}