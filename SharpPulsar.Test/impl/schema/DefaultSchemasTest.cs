﻿using System.Text;

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
	using Org.Apache.Pulsar.Client.Api;
	using PulsarClient = Org.Apache.Pulsar.Client.Api.PulsarClient;
	using PulsarClientException = Org.Apache.Pulsar.Client.Api.PulsarClientException;
	using Org.Apache.Pulsar.Client.Api;
	using Assert = org.testng.Assert;
	using AfterClass = org.testng.annotations.AfterClass;
	using BeforeClass = org.testng.annotations.BeforeClass;
	using Test = org.testng.annotations.Test;

	public class DefaultSchemasTest
	{
		private PulsarClient client;

		private const string TestTopic = "persistent://sample/standalone/ns1/test-topic";

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @BeforeClass public void setup() throws org.apache.pulsar.client.api.PulsarClientException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		public virtual void Setup()
		{
			client = PulsarClient.builder().serviceUrl("pulsar://localhost:6650").build();
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testConsumerInstantiation()
		public virtual void TestConsumerInstantiation()
		{
			ConsumerBuilder<string> StringConsumerBuilder = client.NewConsumer(new StringSchema()).topic(TestTopic);
			Assert.assertNotNull(StringConsumerBuilder);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testProducerInstantiation()
		public virtual void TestProducerInstantiation()
		{
			ProducerBuilder<string> StringProducerBuilder = client.NewProducer(new StringSchema()).topic(TestTopic);
			Assert.assertNotNull(StringProducerBuilder);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testReaderInstantiation()
		public virtual void TestReaderInstantiation()
		{
			ReaderBuilder<string> StringReaderBuilder = client.NewReader(new StringSchema()).topic(TestTopic);
			Assert.assertNotNull(StringReaderBuilder);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testStringSchema() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		public virtual void TestStringSchema()
		{
			string TestString = "hello world";
			sbyte[] TestBytes = TestString.GetBytes(Encoding.UTF8);
			StringSchema StringSchema = new StringSchema();
			assertEquals(TestString, StringSchema.decode(TestBytes));
			assertEquals(StringSchema.encode(TestString), TestBytes);

			 sbyte[] Bytes2 = TestString.GetBytes(StandardCharsets.UTF_16);
			StringSchema StringSchemaUtf16 = new StringSchema(StandardCharsets.UTF_16);
			assertEquals(TestString, StringSchemaUtf16.decode(Bytes2));
			assertEquals(StringSchemaUtf16.encode(TestString), Bytes2);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @AfterClass public void tearDown() throws org.apache.pulsar.client.api.PulsarClientException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		public virtual void TearDown()
		{
			client.Dispose();
		}
	}

}