using System;
using System.Text;
using System.Text.Json;
using System.Threading;
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
    [Collection("ConsumerRedeliveryTest")]
	public class ConsumerRedeliveryTest : ProducerConsumerBase
	{
        private readonly ITestOutputHelper _output;
        private readonly TestCommon.Common _common;

        public ConsumerRedeliveryTest(ITestOutputHelper output)
        {
            _output = output;
            _output = output;
            _common = new TestCommon.Common(output);
            _common.GetPulsarSystem(new AuthenticationDisabled(), operationTime:60000);
		}
		
        [Fact]
		public void TestUnAckMessageRedeliveryWithReceive()
		{
			string topic = $"persistent://public/default/async-unack-redelivery-{Guid.NewGuid()}";

            var producer = _common.PulsarSystem.PulsarProducer(_common.CreateProducer(BytesSchema.Of(), topic, "TestUnAckMessageRedeliveryWithReceive"));
            var consumer = _common.PulsarSystem.PulsarConsumer(_common.CreateConsumer(BytesSchema.Of(), topic, "TestUnAckMessageRedeliveryWithReceive", "sub-TestUnAckMessageRedeliveryWithReceive", ackTimeout: 5000, forceTopic: true));

            const int messageCount = 10;
            
			for (int i = 0; i < messageCount; i++)
			{
				var send = new Send(Encoding.UTF8.GetBytes("my-message-" + i));
				var receipt = _common.PulsarSystem.Send(send, producer.Producer);
                _output.WriteLine(JsonSerializer.Serialize(receipt, new JsonSerializerOptions{WriteIndented = true}));
			}

            int messageReceived = 0;
            
            var messages = _common.PulsarSystem.Messages("TestUnAckMessageRedeliveryWithReceive", false, messageCount, customHander:(m) =>
            {
                var receivedMessage = Encoding.UTF8.GetString((byte[])(object)m.Message.Data);
                return receivedMessage;
            });
            foreach (var message in messages)
            {
                _output.WriteLine($"Received message: [{message}]");
				Assert.NotNull(message);
                messageReceived++;
                // Don't ack message, wait for ack timeout.
			}
			
			Assert.Equal(10,messageReceived);
            Thread.Sleep(3000); 
            messages = _common.PulsarSystem.Messages("TestUnAckMessageRedeliveryWithReceive", false, messageCount, customHander: (m) =>
            {
                var receivedMessage = Encoding.UTF8.GetString((byte[])(object)m.Message.Data);
                _common.PulsarSystem.Acknowledge(m);
                return receivedMessage;
            });
            foreach (var message in messages)
            {
                _output.WriteLine($"Received message: [{message}]");
                Assert.NotNull(message);
                messageReceived++;
			}
            Assert.Equal(20, messageReceived);
			_common.PulsarSystem.Stop();
            _common.PulsarSystem = null;
        }

	}

}