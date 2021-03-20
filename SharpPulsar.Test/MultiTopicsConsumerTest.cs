﻿using SharpPulsar.Configuration;
using SharpPulsar.Messages;
using SharpPulsar.Test.Fixtures;
using SharpPulsar.User;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Xunit;
using Xunit.Abstractions;
using SharpPulsar.Extension;
using System.Linq;

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


	/// <summary>
	/// Unit Tests of <seealso cref="MultiTopicsConsumerImpl"/>.
	/// </summary>
	[Collection(nameof(PulsarTests))]
	public class MultiTopicsConsumerTest
	{
		private const string Subscription = "reader-multi-topics-sub";
		private readonly ITestOutputHelper _output;
		private readonly PulsarClient _client;

		public MultiTopicsConsumerTest(ITestOutputHelper output, PulsarStandaloneClusterFixture fixture)
		{
			_output = output;
			_client = fixture.Client;
		}
		[Fact]
		public void TestMultiTopicConsumer()
        {
			var messageCount = 5;

			var builder = new ConsumerConfigBuilder<sbyte[]>()
				.Topic("one-topic", "two-topic", "three-topic")
				.ForceTopicCreation(true)
				.SubscriptionName("multi-topic-sub");

			var consumer = _client.NewConsumer(builder);

			var acks = PublishMessages("one-topic", messageCount, "hello Toba"); 

			for(var i = 0; i < messageCount; i++)
			{
				var message = (TopicMessage<sbyte[]>)consumer.Receive();
				Assert.NotNull(message);
				var messageId = (MessageId)((TopicMessageId)message.MessageId).InnerMessageId;
				consumer.Acknowledge(message);
				Assert.Contains("one-topic", message.TopicName);
            }
			acks.Clear();
			acks = PublishMessages("two-topic", messageCount, "hello Toba");
			for (var i = 0; i < messageCount; i++)
			{
				var message = (TopicMessage<sbyte[]>)consumer.Receive();
				Assert.NotNull(message);
				var messageId = (MessageId)message.MessageId;
				var same = acks.FirstOrDefault(x => x.EntryId == messageId.EntryId && x.LedgerId == messageId.LedgerId && x.SequenceId == message.SequenceId);
				Assert.NotNull(same);
				Assert.Contains("two-topic", message.TopicName);
			}
			acks.Clear();
			acks = PublishMessages("three-topic", messageCount, "hello Toba");
			for (var i = 0; i < messageCount; i++)
			{
				var message = (TopicMessage<sbyte[]>)consumer.Receive();
				Assert.NotNull(message);
				var messageId = (MessageId)message.MessageId;
				var same = acks.FirstOrDefault(x => x.EntryId == messageId.EntryId && x.LedgerId == messageId.LedgerId && x.SequenceId == message.SequenceId);
				Assert.NotNull(same);
				Assert.Contains("three-topic", message.TopicName);
			}
		}

		private List<AckReceived> PublishMessages(string topic, int count, string message)
		{
			List<AckReceived> keys = new List<AckReceived>();
			var builder = new ProducerConfigBuilder<sbyte[]>()
				.Topic(topic);
			var producer = _client.NewProducer(builder);
			for (int i = 0; i < count; i++)
			{
				string key = "key" + i;
				sbyte[] data = Encoding.UTF8.GetBytes($"{message}-{i}").ToSBytes();
				producer.NewMessage().Key(key).Value(data).Send();
				var receipt = producer.SendReceipt();
				keys.Add(receipt);
			}
			return keys;
		}
	}

}