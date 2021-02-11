using SharpPulsar.User;
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

        private const string TestTopic = "persistent://sample/standalone/ns1/test-topic";

        public virtual void Setup()
        {
            _client = PulsarClient.builder().serviceUrl("pulsar://localhost:6650").build();
        }

        public virtual void TestConsumerInstantiation()
        {
            ConsumerBuilder<string> stringConsumerBuilder = _client.NewConsumer(new StringSchema()).Topic(TestTopic);
            Assert.assertNotNull(stringConsumerBuilder);
        }

        public virtual void TestProducerInstantiation()
        {
            ProducerBuilder<string> stringProducerBuilder = _client.NewProducer(new StringSchema()).Topic(TestTopic);
            Assert.assertNotNull(stringProducerBuilder);
        }

        public virtual void TestReaderInstantiation()
        {
            ReaderBuilder<string> stringReaderBuilder = _client.NewReader(new StringSchema()).Topic(TestTopic);
            Assert.assertNotNull(stringReaderBuilder);
        }

        
        public virtual void TestStringSchema()
        {
            string testString = "hello world";
            sbyte[] testBytes = testString.GetBytes(Encoding.UTF8);
            StringSchema stringSchema = new StringSchema();
            assertEquals(testString, stringSchema.Decode(testBytes));
            assertEquals(stringSchema.Encode(testString), testBytes);

            sbyte[] bytes2 = testString.GetBytes(StandardCharsets.UTF_16);
            StringSchema stringSchemaUtf16 = new StringSchema(StandardCharsets.UTF_16);
            assertEquals(testString, stringSchemaUtf16.Decode(bytes2));
            assertEquals(stringSchemaUtf16.Encode(testString), bytes2);
        }

        public virtual void TearDown()
        {
            _client.Dispose();
        }
    }

}