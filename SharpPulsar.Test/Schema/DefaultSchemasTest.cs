using SharpPulsar.Configuration;
using SharpPulsar.Extension;
using SharpPulsar.Schemas;
using SharpPulsar.User;
using System.Text;
using Xunit;

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
        private PulsarSystem _system;
        private PulsarClient _client;

        private const string TestTopic = "test-topic";

        public DefaultSchemasTest()
        {
            var client = new ClientConfigurationData
            {
                ServiceUrl = "pulsar://127.0.0.1:6650"
            };
            _system = PulsarSystem.GetInstance(client);
            _client = _system.NewClient();
        }
        [Fact]
        public virtual void TestConsumerInstantiation()
        {
            var consumer = new ConsumerConfigBuilder<string>();
            consumer.Topic(TestTopic);
            consumer.SubscriptionName("test-sub");
            var stringConsumerBuilder = _client.NewConsumer(new StringSchema(), consumer);
            Assert.NotNull(stringConsumerBuilder);
        }
        [Fact(Skip = "Not ready")]
        public virtual void TestProducerInstantiation()
        {
            var producer = new ProducerConfigBuilder<string>();
            producer.Topic(TestTopic);
            var stringProducerBuilder = _client.NewProducer(new StringSchema(), producer);
            Assert.NotNull(stringProducerBuilder);
        }
        [Fact(Skip = "Not ready")]
        public virtual void TestReaderInstantiation()
        {
            var reader = new ReaderConfigBuilder<string>();
            reader.Topic(TestTopic);
            var stringReaderBuilder = _client.NewReader(new StringSchema(), reader);
            Assert.NotNull(stringReaderBuilder);
        }

        [Fact]
        public virtual void TestStringSchema()
        {
            string testString = "hello world";
            sbyte[] testBytes = Encoding.UTF8.GetBytes(testString).ToSBytes();
            StringSchema stringSchema = new StringSchema();
            Assert.Equal(testString, stringSchema.Decode(testBytes));
            var act = stringSchema.Encode(testString);
            for (var i = 0; i < testBytes.Length; i++)
            {
                var expected = testBytes[i];
                var actual = act[i];
                Assert.Equal(expected, actual);
            }

            sbyte[] bytes2 = Encoding.Unicode.GetBytes(testString).ToSBytes();
            StringSchema stringSchemaUtf16 = new StringSchema(Encoding.Unicode);
            Assert.Equal(testString, stringSchemaUtf16.Decode(bytes2));
            var act2 = stringSchemaUtf16.Encode(testString);
            for (var i = 0; i < bytes2.Length; i++)
            {
                var expected = bytes2[i];
                var actual = act2[i];
                Assert.Equal(expected, actual);
            }
        }
        ~DefaultSchemasTest()
        {
            _client.Shutdown();
        }
    }

}