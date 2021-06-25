using BAMCIS.Util.Concurrent;
using SharpPulsar.Common;
using SharpPulsar.Configuration;
using SharpPulsar.Test.Fixtures;
using SharpPulsar.User;
using System;
using System.Text;
using System.Text.RegularExpressions;
using SharpPulsar.Extension;
using Xunit;
using Xunit.Abstractions;
using static SharpPulsar.Protocol.Proto.CommandSubscribe;

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
	public class PatternTopicsConsumerTest
	{
		private readonly ITestOutputHelper _output;
		private readonly PulsarClient _client;

		public PatternTopicsConsumerTest(ITestOutputHelper output, PulsarStandaloneClusterFixture fixture)
		{
			_output = output;
			_client = fixture.Client;
		}
		[Fact]
		public virtual void TestBinaryProtoToGetTopicsOfNamespacePersistent()
		{
			var key = Guid.NewGuid().ToString();
			var subscriptionName = "regex-subscription";
			var topicName1 = "persistent://public/default/reg-topic-1-" + key;
			var topicName2 = "persistent://public/default/reg-topic-2-" + key;
			var topicName3 = "persistent://public/default/reg-topic-3-" + key;
			var topicName4 = "non-persistent://public/default/reg-topic-4-" + key;
			var pattern = new Regex("public/default/reg-topic.*");


			// 2. create producer
			var messagePredicate = "my-message-" + key + "-";
			var totalMessages = 30;

			var producer1 = _client.NewProducer(new ProducerConfigBuilder<byte[]>()
				.Topic(topicName1));

			var producer2 = _client.NewProducer(new ProducerConfigBuilder<byte[]>()
				.Topic(topicName2));

			var producer3 = _client.NewProducer(new ProducerConfigBuilder<byte[]>()
				.Topic(topicName3));

			var producer4 = _client.NewProducer(new ProducerConfigBuilder<byte[]>()
				.Topic(topicName4));

			// 5. produce data
			for(var i = 0; i < 10; i++)
			{
				producer1.Send(Encoding.UTF8.GetBytes(messagePredicate + "producer1-" + i));
				producer2.Send(Encoding.UTF8.GetBytes(messagePredicate + "producer2-" + i));
				producer3.Send(Encoding.UTF8.GetBytes(messagePredicate + "producer3-" + i));
				producer4.Send(Encoding.UTF8.GetBytes(messagePredicate + "producer4-" + i));
			}

			var consumer = _client.NewConsumer(new ConsumerConfigBuilder<byte[]>()
				.TopicsPattern(pattern)
				.PatternAutoDiscoveryPeriod(2)
				.SubscriptionName(subscriptionName)
				.SubscriptionType(SubType.Shared)
				.AckTimeout(2000, TimeUnit.MILLISECONDS));

			// 6. should receive all the message
			var messageSet = 0;
			var message = consumer.Receive(TimeSpan.FromSeconds(10));
			do
			{
				var m = (TopicMessage<byte[]>)message;
				messageSet++;
				consumer.Acknowledge(message);
				_output.WriteLine($"Consumer acknowledged : {Encoding.UTF8.GetString(message.Data)} from topic: {m.TopicName}");
				message = consumer.Receive(TimeSpan.FromSeconds(20));
			} while(message != null);

			consumer.Unsubscribe();
			consumer.Close();
			producer1.Close();
			producer2.Close();
			producer3.Close();
			producer4.Close();
		}

	}

}