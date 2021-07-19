using System;
using System.Collections.Generic;
using System.Text;
using BAMCIS.Util.Concurrent;
using SharpPulsar.Configuration;
using SharpPulsar.Test.Fixtures;
using SharpPulsar.User;
using Xunit;
using Xunit.Abstractions;
using SharpPulsar.Extension;
using SharpPulsar.Interfaces;
using System.Threading;

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
namespace SharpPulsar.Test
{
	[Collection(nameof(PulsarTests))]
	public class MessageChunkingTest
	{
        private readonly ITestOutputHelper _output;
		private readonly PulsarClient _client;

		public MessageChunkingTest(ITestOutputHelper output, PulsarStandaloneClusterFixture fixture)
		{
			_output = output;
			_client = fixture.Client;
		}

		[Fact]
		public void TestLargeMessage()
		{
			//this.conf.MaxMessageSize = 5;
			const int totalMessages = 3;
			var topicName = $"persistent://public/default/my-topic1-{DateTimeHelper.CurrentUnixTimeMillis()}";
			var builder = new ConsumerConfigBuilder<byte[]>();
			builder.Topic(topicName);
			builder.SubscriptionName("my-subscriber-name");
			builder.AckTimeout(TimeSpan.FromMilliseconds(20000));
			builder.ForceTopicCreation(true);
			builder.AcknowledgmentGroupTime(0);
			var consumer = _client.NewConsumer(builder);

			var pBuilder = new ProducerConfigBuilder<byte[]>();
			pBuilder.Topic(topicName);
			pBuilder.EnableChunking(true);
			pBuilder.MaxMessageSize(5);
			var producer = _client.NewProducer(pBuilder);

			IList<string> publishedMessages = new List<string>();
			for (var i = 1; i < totalMessages; i++)
			{
				var message = CreateMessagePayload(i * 10);
				publishedMessages.Add(message);
				producer.Send(Encoding.UTF8.GetBytes(message));
			}

			IMessage<byte[]> msg = null;
			ISet<string> messageSet = new HashSet<string>();
			IList<IMessage<byte[]>> msgIds = new List<IMessage<byte[]>>();
            Thread.Sleep(TimeSpan.FromSeconds(5));
			for (var i = 0; i < totalMessages - 1; i++)
			{
				msg = consumer.Receive();
				var receivedMessage = Encoding.UTF8.GetString(msg.Data);
				_output.WriteLine($"[{i}] - Published [{publishedMessages[i]}] Received message: [{receivedMessage}]");
				var expectedMessage = publishedMessages[i];
				TestMessageOrderAndDuplicates(messageSet, receivedMessage, expectedMessage);
				msgIds.Add(msg);
			}

			foreach (var msgId in msgIds)
			{
				consumer.Acknowledge(msgId);
			}


		}
		private void TestMessageOrderAndDuplicates<T>(ISet<T> messagesReceived, T receivedMessage, T expectedMessage)
		{
			// Make sure that messages are received in order
			Assert.True(receivedMessage.Equals(expectedMessage), "Received message " + receivedMessage + " did not match the expected message " + expectedMessage);

			// Make sure that there are no duplicates
			Assert.True(messagesReceived.Add(receivedMessage), "Received duplicate message " + receivedMessage);
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