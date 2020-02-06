using System.Text;

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
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.testng.Assert.assertEquals;

	using ConsumerBuilder = api.ConsumerBuilder;
	using ProducerBuilder = api.ProducerBuilder;
	using PulsarClient = api.PulsarClient;
	using PulsarClientException = api.PulsarClientException;
	using ReaderBuilder = api.ReaderBuilder;
	using Assert = org.testng.Assert;
	using AfterClass = org.testng.annotations.AfterClass;
	using BeforeClass = org.testng.annotations.BeforeClass;
	using Test = org.testng.annotations.Test;

	public class DefaultSchemasTest
	{
		private PulsarClient client;

		private const string TEST_TOPIC = "persistent://sample/standalone/ns1/test-topic";

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @BeforeClass public void setup() throws org.apache.pulsar.client.api.PulsarClientException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		public virtual void setup()
		{
			client = PulsarClient.builder().serviceUrl("pulsar://localhost:6650").build();
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testConsumerInstantiation()
		public virtual void testConsumerInstantiation()
		{
			ConsumerBuilder<string> stringConsumerBuilder = client.newConsumer(new StringSchema()).topic(TEST_TOPIC);
			Assert.assertNotNull(stringConsumerBuilder);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testProducerInstantiation()
		public virtual void testProducerInstantiation()
		{
			ProducerBuilder<string> stringProducerBuilder = client.newProducer(new StringSchema()).topic(TEST_TOPIC);
			Assert.assertNotNull(stringProducerBuilder);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testReaderInstantiation()
		public virtual void testReaderInstantiation()
		{
			ReaderBuilder<string> stringReaderBuilder = client.newReader(new StringSchema()).topic(TEST_TOPIC);
			Assert.assertNotNull(stringReaderBuilder);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testStringSchema() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		public virtual void testStringSchema()
		{
			string testString = "hello world";
			sbyte[] testBytes = testString.GetBytes(Encoding.UTF8);
			StringSchema stringSchema = new StringSchema();
			assertEquals(testString, stringSchema.decode(testBytes));
			assertEquals(stringSchema.encode(testString), testBytes);

			 sbyte[] bytes2 = testString.GetBytes(StandardCharsets.UTF_16);
			StringSchema stringSchemaUtf16 = new StringSchema(StandardCharsets.UTF_16);
			assertEquals(testString, stringSchemaUtf16.decode(bytes2));
			assertEquals(stringSchemaUtf16.encode(testString), bytes2);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @AfterClass public void tearDown() throws org.apache.pulsar.client.api.PulsarClientException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		public virtual void tearDown()
		{
			client.close();
		}
	}

}