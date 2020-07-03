using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using SharpPulsar.Akka.InternalCommands;
using SharpPulsar.Impl.Auth;
using SharpPulsar.Impl.Schema;
using Xunit;
using Xunit.Abstractions;

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
namespace SharpPulsar.Test.Api
{
    public class BytesKeyTest : ProducerConsumerBase
	{
        private readonly ITestOutputHelper _output;
        private TestCommon.Common _common;

        public BytesKeyTest(ITestOutputHelper output)
        {
            _output = output;
			_common = new TestCommon.Common(output);
			_common.GetPulsarSystem(new AuthenticationDisabled());
            ProducerBaseSetup(_common.PulsarSystem, output);

        }

		private void ByteKeysTest(bool batching)
		{
			Random r = new Random(0);
            var consumer = _common.PulsarSystem.PulsarConsumer(_common.CreateConsumer(BytesSchema.Of(), "persistent://my-property/my-ns/my-topic", "", "my-subscriber-name"));

            var producer = _common.PulsarSystem.PulsarProducer(_common.CreateProducer(BytesSchema.Of(), "persistent://my-property/my-ns/my-topic1", "TestSyncProducerAndConsumer", batchMessageDelayMs: batching? long.MaxValue: 0, batchingMaxMessages: batching? int.MaxValue : 0));

			byte[] byteKey = new byte[1000];
			r.NextBytes(byteKey);
			var config = new Dictionary<string, object>{{ "KeyBytes", byteKey } };
			var send = new Send(Encoding.UTF8.GetBytes("TestMessage"), consumer.Topic, config.ToImmutableDictionary());
			_common.PulsarSystem.Send(send, producer.Producer);

            var messages = _common.PulsarSystem.Messages(false, customHander: (m) =>
            {
                var receivedMessage = Encoding.UTF8.GetString((byte[])(object)m.Message.Data);

                Assert.Equal(byteKey, (byte[])(object)m.Message.KeyBytes);
                Assert.True(m.Message.HasBase64EncodedKey());
				return receivedMessage;
            });
            foreach (var message in messages)
            {
                _output.WriteLine($"Received message: [{message}]");
                Assert.Equal("TestMessage", message);
			}
		}

		[Fact]
		public void TestBytesKeyBatch()
		{
			ByteKeysTest(true);
		}
		[Fact]
		public void TestBytesKeyNoBatch()
		{
			ByteKeysTest(false);
		}
	}

}