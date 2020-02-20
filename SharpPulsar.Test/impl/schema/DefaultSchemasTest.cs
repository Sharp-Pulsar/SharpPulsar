using System;
using System.Runtime.InteropServices;
using SharpPulsar.Api;
using System.Text;
using SharpPulsar.Impl;
using SharpPulsar.Impl.Schema;
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
namespace SharpPulsar.Test.Impl.schema
{

public class DefaultSchemasTest:IDisposable
	{
		private IPulsarClient _client;

		private const string TestTopic = "persistent://sample/standalone/ns1/test-topic";

        public DefaultSchemasTest()
        {
			_client = new PulsarClientBuilderImpl().ServiceUrl("pulsar://localhost:6650").Build();
		}
		

		[Fact]
		public void TestConsumerInstantiation()
		{
			var stringConsumerBuilder = _client.NewConsumer(new StringSchema()).Topic(TestTopic);
			Assert.NotNull(stringConsumerBuilder);
		}

		[Fact]
		public void TestProducerInstantiation()
		{
			var stringProducerBuilder = _client.NewProducer(new StringSchema()).Topic(TestTopic);
			Assert.NotNull(stringProducerBuilder);
		}

		[Fact]
		public void TestReaderInstantiation()
		{
			var stringReaderBuilder = _client.NewReader(new StringSchema()).Topic(TestTopic);
			Assert.NotNull(stringReaderBuilder);
		}

		[Fact]
		public void TestStringSchema()
		{
			var testString = "hello world";
			var testBytes = testString.GetBytes(Encoding.UTF8);
			var stringSchema = new StringSchema();
			Assert.Equal(testString, stringSchema.Decode(testBytes));
            Assert.Equal(stringSchema.Encode(testString), testBytes);

            var bytes2 = (sbyte[])(object)Encoding.Unicode.GetBytes(testString);
			var stringSchemaUtf16 = new StringSchema(Encoding.UTF8.EncodingName);
            Assert.Equal(testString, stringSchemaUtf16.Decode(bytes2));
            Assert.Equal(stringSchemaUtf16.Encode(testString), bytes2);
		}


        public void Dispose()
        {
		    _client.Dispose();
	    }
    }

}