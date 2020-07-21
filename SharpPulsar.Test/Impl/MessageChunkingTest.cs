using System;
using System.Collections.Generic;
using System.Text;
using SharpPulsar.Akka.InternalCommands;
using SharpPulsar.Akka.InternalCommands.Consumer;
using SharpPulsar.Impl.Auth;
using SharpPulsar.Impl.Schema;
using Xunit;
using Xunit.Abstractions;
using ProducerConsumerBase = SharpPulsar.Test.Api.ProducerConsumerBase;

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
namespace SharpPulsar.Test.Impl
{

    public class MessageChunkingTest : ProducerConsumerBase
	{
        private readonly ITestOutputHelper _output;
        private TestCommon.Common _common;

        public MessageChunkingTest(ITestOutputHelper output)
        {
            _output = output;
            _common = new TestCommon.Common(output);
            _common.GetPulsarSystem(new AuthenticationDisabled(), useProxy: true, operationTime: 120000, brokerService: "pulsar://52.247.20.224:6650");
        }
		[Fact]
		public void TestLargeMessage()
		{
			//this.conf.MaxMessageSize = 5;
			const int totalMessages = 2;
			var topicName = $"persistent://public/default/my-topic1-{DateTimeHelper.CurrentUnixTimeMillis()}";

			var consumer =_common.PulsarSystem.PulsarConsumer(_common.CreateConsumer(BytesSchema.Of(), topicName, "TestLargeMessage", "my-subscriber-name", acknowledgmentGroupTime: 0, forceTopic: true));


			var producer = _common.PulsarSystem.PulsarProducer(_common.CreateProducer(BytesSchema.Of(), topicName, "TestLargeMessage", enableChunking: true, maxMessageSize: 5));
			
			IList<string> publishedMessages = new List<string>();
			for (int i = 0; i < totalMessages; i++)
			{
				string message = CreateMessagePayload(i * 10);
				publishedMessages.Add(message);
				var re = _common.PulsarSystem.Send(new Send(message.GetBytes()), producer.Producer);
			}

			ConsumedMessage msg = null;
			ISet<string> messageSet = new HashSet<string>();
			IList<ConsumedMessage> msgIds = new List<ConsumedMessage>();
			for (int i = 0; i < totalMessages; i++)
			{
				msg = _common.PulsarSystem.Receive("TestLargeMessage", 5000);
				string receivedMessage = ((byte[])(object)msg.Message.Data).GetString();
				_output.WriteLine($"[{i}] - Published [{publishedMessages[i]}] Received message: [{receivedMessage}]");
				string expectedMessage = publishedMessages[i];
				TestMessageOrderAndDuplicates(messageSet, receivedMessage, expectedMessage);
				msgIds.Add(msg);
			}

			foreach (var msgId in msgIds)
			{
				_common.PulsarSystem.Acknowledge(msgId);
			}


		}
		
		private string CreateMessagePayload(int size)
		{
			var str = new StringBuilder();
			var rand = new Random();
			for (var i = 0; i < size; i++)
			{
				str.Append(rand.Next(10));
			}
			return str.ToString();
		}

	}

}