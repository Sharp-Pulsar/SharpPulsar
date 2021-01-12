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
namespace SharpPulsar.Test.Schema
{

	public class DefaultSchemasTest
	{
		private PulsarClient _client;

		private const string TEST_TOPIC = "persistent://sample/standalone/ns1/test-topic";

		public virtual void Setup()
		{
			_client = PulsarClient.builder().serviceUrl("pulsar://localhost:6650").build();
		}

		public virtual void TestConsumerInstantiation()
		{
			ConsumerBuilder<string> StringConsumerBuilder = _client.newConsumer(new StringSchema()).topic(TEST_TOPIC);
			Assert.assertNotNull(StringConsumerBuilder);
		}

		public virtual void TestProducerInstantiation()
		{
			ProducerBuilder<string> StringProducerBuilder = _client.newProducer(new StringSchema()).topic(TEST_TOPIC);
			Assert.assertNotNull(StringProducerBuilder);
		}

		public virtual void TestReaderInstantiation()
		{
			ReaderBuilder<string> StringReaderBuilder = _client.newReader(new StringSchema()).topic(TEST_TOPIC);
			Assert.assertNotNull(StringReaderBuilder);
		}

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

		public virtual void TearDown()
		{
			_client.close();
		}
	}

}