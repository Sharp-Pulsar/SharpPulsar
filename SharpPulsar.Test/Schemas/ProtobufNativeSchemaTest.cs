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
	using ByteBuf = io.netty.buffer.ByteBuf;
	using ByteBufAllocator = io.netty.buffer.ByteBufAllocator;
	using Slf4j = lombok.@extern.slf4j.Slf4j;
	using SchemaType = org.apache.pulsar.common.schema.SchemaType;
	using Assert = org.testng.Assert;
	using Test = org.testng.annotations.Test;


//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Slf4j public class ProtobufNativeSchemaTest
	public class ProtobufNativeSchemaTest
	{


		private const string EXPECTED_SCHEMA_JSON = "{\"fileDescriptorSet\":\"CtMDCgpUZXN0LnByb3RvEgVwcm90bxoSRXh0ZXJuYWxUZXN0LnByb3RvImUKClN1Yk1lc3NhZ2U" + "SCwoDZm9vGAEgASgJEgsKA2JhchgCIAEoARo9Cg1OZXN0ZWRNZXNzYWdlEgsKA3VybBgBIAEoCRINCgV0aXRsZRgCIAEoCRIQCghzbmlwcGV0cxgDIAMoCSLlAQoLV" + "GVzdE1lc3NhZ2USEwoLc3RyaW5nRmllbGQYASABKAkSEwoLZG91YmxlRmllbGQYAiABKAESEAoIaW50RmllbGQYBiABKAUSIQoIdGVzdEVudW0YBCABKA4yDy5wcm90by5U" + "ZXN0RW51bRImCgtuZXN0ZWRGaWVsZBgFIAEoCzIRLnByb3RvLlN1Yk1lc3NhZ2USFQoNcmVwZWF0ZWRGaWVsZBgKIAMoCRI4Cg9leHRlcm5hbE1lc3NhZ2UYCyABKAsyHy5wcm90by" + "5leHRlcm5hbC5FeHRlcm5hbE1lc3NhZ2UqJAoIVGVzdEVudW0SCgoGU0hBUkVEEAASDAoIRkFJTE9WRVIQAUItCiVvcmcuYXBhY2hlLnB1bHNhci5jbGllbnQuc2NoZW1hLnByb3" + "RvQgRUZXN0YgZwcm90bzMKoAEKEkV4dGVybmFsVGVzdC5wcm90bxIOcHJvdG8uZXh0ZXJuYWwiOwoPRXh0ZXJuYWxNZXNzYWdlEhMKC3N0cmluZ0ZpZWxkGAEgA" + "SgJEhMKC2RvdWJsZUZpZWxkGAIgASgBQjUKJW9yZy5hcGFjaGUucHVsc2FyLmNsaWVudC5zY2hlbWEucHJvdG9CDEV4dGVybmFsVGVzdGIGcHJvdG8z\"," + "\"rootMessageTypeName\":\"proto.TestMessage\",\"rootFileDescriptorName\":\"Test.proto\"}";

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testEncodeAndDecode()
		public virtual void TestEncodeAndDecode()
		{
			const string StringFieldValue = "StringFieldValue";
			org.apache.pulsar.client.schema.proto.Test.TestMessage TestMessage = org.apache.pulsar.client.schema.proto.Test.TestMessage.newBuilder().setStringField(StringFieldValue).build();
			ProtobufNativeSchema<org.apache.pulsar.client.schema.proto.Test.TestMessage> ProtobufSchema = ProtobufNativeSchema.of(typeof(org.apache.pulsar.client.schema.proto.Test.TestMessage));

			sbyte[] Bytes = ProtobufSchema.encode(TestMessage);
			org.apache.pulsar.client.schema.proto.Test.TestMessage Message = ProtobufSchema.decode(Bytes);

			Assert.assertEquals(Message.StringField, StringFieldValue);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testSchema()
		public virtual void TestSchema()
		{
			ProtobufNativeSchema<org.apache.pulsar.client.schema.proto.Test.TestMessage> ProtobufSchema = ProtobufNativeSchema.of(typeof(org.apache.pulsar.client.schema.proto.Test.TestMessage));

			Assert.assertEquals(ProtobufSchema.SchemaInfo.Type, SchemaType.PROTOBUF_NATIVE);

			Assert.assertNotNull(ProtobufNativeSchemaUtils.deserialize(ProtobufSchema.SchemaInfo.Schema));
			Assert.assertEquals(new string(ProtobufSchema.SchemaInfo.Schema, StandardCharsets.UTF_8), EXPECTED_SCHEMA_JSON);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testGenericOf()
		public virtual void TestGenericOf()
		{
			try
			{
				ProtobufNativeSchema<org.apache.pulsar.client.schema.proto.Test.TestMessage> ProtobufNativeSchema = ProtobufNativeSchema.ofGenericClass(typeof(org.apache.pulsar.client.schema.proto.Test.TestMessage), new Dictionary<org.apache.pulsar.client.schema.proto.Test.TestMessage>());
			}
			catch (Exception)
			{
				Assert.fail("Should not construct a ProtobufShema over a non-protobuf-generated class");
			}

			try
			{
				ProtobufSchema<org.apache.pulsar.client.schema.proto.Test.TestMessage> ProtobufSchema = ProtobufSchema.ofGenericClass(typeof(string), Collections.emptyMap());
				Assert.fail("Should not construct a ProtobufNativeShema over a non-protobuf-generated class");
			}
			catch (Exception)
			{

			}
		}


//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testDecodeByteBuf()
		public virtual void TestDecodeByteBuf()
		{
			ProtobufNativeSchema<org.apache.pulsar.client.schema.proto.Test.TestMessage> ProtobufSchema = ProtobufNativeSchema.of(typeof(org.apache.pulsar.client.schema.proto.Test.TestMessage));
			org.apache.pulsar.client.schema.proto.Test.TestMessage TestMessage = org.apache.pulsar.client.schema.proto.Test.TestMessage.newBuilder().build();
			sbyte[] Bytes = ProtobufSchema.encode(org.apache.pulsar.client.schema.proto.Test.TestMessage.newBuilder().build());
			ByteBuf ByteBuf = ByteBufAllocator.DEFAULT.buffer(Bytes.Length);
			ByteBuf.writeBytes(Bytes);

			Assert.assertEquals(TestMessage, ProtobufSchema.decode(ByteBuf));

		}

	}

}