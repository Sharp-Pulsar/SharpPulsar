using SharpPulsar.Configuration;
using SharpPulsar.Test.Fixtures;
using SharpPulsar.User;
using System.Collections.Generic;
using System.Text;
using Xunit;
using Xunit.Abstractions;
using System;
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
			var first = $"one-topic-{Guid.NewGuid()}";
			var second = $"two-topic-{Guid.NewGuid()}";
			var third = $"three-topic-{Guid.NewGuid()}";

			PublishMessages(first, messageCount, "hello Toba");
			PublishMessages(third, messageCount, "hello Toba");
			PublishMessages(second, messageCount, "hello Toba");
            var builder = new ConsumerConfigBuilder<byte[]>()
                .Topic(first, second, third)
                .ForceTopicCreation(true)
                .SubscriptionName("multi-topic-sub");

            var consumer = _client.NewConsumer(builder);
            Thread.Sleep(TimeSpan.FromSeconds(30));
            var received = 0;
            for (var i = 0; i < messageCount; i++)
			{
				var message = (TopicMessage<byte[]>)consumer.Receive();
                if(message != null)
                {
                    consumer.Acknowledge(message);
                    _output.WriteLine($"message from topic: {message.Topic}");
                    received++;
                }
			}
			for (var i = 0; i < messageCount; i++)
			{
				var message = (TopicMessage<byte[]>)consumer.Receive();
                if (message != null)
                {
                    consumer.Acknowledge(message);
                    _output.WriteLine($"message from topic: {message.Topic}");
                    received++;
                }
            }
			for (var i = 0; i < messageCount; i++)
			{
				var message = (TopicMessage<byte[]>)consumer.Receive();
                if (message != null)
                {
                    consumer.Acknowledge(message);
                    _output.WriteLine($"message from topic: {message.Topic}");
                    received++;
                }
            }
            Assert.True(received > 0);
            consumer.Close();
        }

		private List<MessageId> PublishMessages(string topic, int count, string message)
		{
			var keys = new List<MessageId>();
			var builder = new ProducerConfigBuilder<byte[]>()
				.Topic(topic);
			var producer = _client.NewProducer(builder);
			for (var i = 0; i < count; i++)
			{
				var key = "key" + i;
				var data = Encoding.UTF8.GetBytes($"{message}-{i}");
				var id = producer.NewMessage().Key(key).Value(data).Send();
				keys.Add(id);
			}
            producer.Close();
            return keys;
		}
	}

}