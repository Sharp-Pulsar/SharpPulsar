﻿using Akka.Actor;
using Akka.Util;
using App.Metrics.Concurrency;
using SharpPulsar.Messages.Consumer;
using System;
using System.Collections.Generic;
using System.Threading;
using Xunit;
using Xunit.Abstractions;
using System.Text;
using static SharpPulsar.Protocol.Proto.CommandSubscribe;

using SharpPulsar.Interfaces;
using SharpPulsar.Builder;
using SharpPulsar.Test.Fixture;
using SharpPulsar.TestContainer;
using System.Threading.Tasks;

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
    [Collection(nameof(PulsarCollection))]
	public class ZeroQueueSizeTest
	{		
		private readonly int _totalMessages = 10;
		private readonly ITestOutputHelper _output;
		private readonly PulsarClient _client;

        public ZeroQueueSizeTest(ITestOutputHelper output, PulsarFixture fixture)
		{
			_output = output;
			_client = fixture.Client;
		}
		[Fact]
		public void ZeroQueueSizeNormalConsumer()
		{
			string key = "nonZeroQueueSizeNormalConsumer";

			// 1. Config

			string topicName = "topic-" + key;

			string subscriptionName = "my-ex-subscription-" + key;

			string messagePredicate = "my-message-" + key + "-";

			// 2. Create Producer
			var pBuilder = new ProducerConfigBuilder<byte[]>()
				.Topic(topicName)
				.EnableBatching(false);
			var producer = _client.NewProducer(pBuilder);
			// 3. Create Consumer
			var config = new ConsumerConfigBuilder<byte[]>()
				.Topic(topicName)
				.SubscriptionName(subscriptionName)
				.ReceiverQueueSize(0);

			var consumer = _client.NewConsumer(config);
			// 3. producer publish messages
			for(int i = 0; i < _totalMessages; i++)
			{
				var msg = messagePredicate + i;
				_output.WriteLine("Producer produced: " + msg);
				producer.Send(Encoding.UTF8.GetBytes(msg));
			}

			// 4. Receiver receives the message
			IMessage<byte[]> message;
			for(int i = 0; i < _totalMessages; i++)
			{
				Assert.Equal(0, consumer.NumMessagesInQueue());
				message = consumer.Receive();
				var r = Encoding.UTF8.GetString(message.Data);
				Assert.Equal(r, messagePredicate + i);
				Assert.Equal(0, consumer.NumMessagesInQueue());
				_output.WriteLine("Consumer received : " + r);
			}
		}

		[Fact]
		public async Task TestZeroQueueSizeMessageRedelivery()
		{
			string topic = $"testZero{Guid.NewGuid()}";

			var config = new ConsumerConfigBuilder<int>()
				.Topic(topic)
				.SubscriptionName("sub")
				.ReceiverQueueSize(0)
				.SubscriptionType(SubType.Shared)
				.AckTimeout(TimeSpan.FromSeconds(1));

			var consumer = _client.NewConsumer(ISchema<object>.Int32, config);
			var pBuilder = new ProducerConfigBuilder<int>()
				.Topic(topic)
				.EnableBatching(false);
			var producer = _client.NewProducer(ISchema<object>.Int32, pBuilder);

			const int messages = 10;

			for(int i = 0; i < messages; i++)
			{
				producer.Send(i);
			}
            await Task.Delay(5000);
            ISet<int> receivedMessages = new HashSet<int>();
			for(int i = 0; i < messages * 2; i++)
			{
                var v = consumer.Receive().Value;
                receivedMessages.Add(v);
                _output.WriteLine("Consumer received : " + receivedMessages.Count);
            }

			Assert.Equal(messages, receivedMessages.Count);

			consumer.Close();
			producer.Close();
		}
		
        [Fact]
		public void TestZeroQueueSizeMessageRedeliveryForListener()
		{
			string topic = $"testZeroQueueSizeMessageRedeliveryForListener-{DateTime.Now.Ticks}";
			const int messages = 10;
            CountdownEvent latch = new CountdownEvent(messages * 2);
			ISet<int> receivedMessages = new HashSet<int>();
			var config = new ConsumerConfigBuilder<int>()
				.Topic(topic)
				.SubscriptionName("sub")
				.ReceiverQueueSize(0)
				.SubscriptionType(SubType.Shared)
				.AckTimeout(TimeSpan.FromSeconds(1))
				.MessageListener(new MessageListener<int>((consumer, msg) =>
				{
                    try
                    {
						receivedMessages.Add(msg.Value);
					}
					finally
					{
						latch.Signal();
					}

				}, null));

			var consumer = _client.NewConsumer(ISchema<object>.Int32, config);
			var pBuilder = new ProducerConfigBuilder<int>()
				.Topic(topic)
				.EnableBatching(false);
			var producer = _client.NewProducer(ISchema<object>.Int32, pBuilder);

			for(int i = 0; i < messages; i++)
			{
				producer.Send(i);
			}

			latch.Wait();
			Assert.Equal(messages, receivedMessages.Count);

			consumer.Close();
			producer.Close();
		}

        //[Fact(Skip = "TestPauseAndResume")]
        [Fact]
		public void TestPauseAndResume()
		{
			const string topicName = "zero-queue-pause-and-resume";
			const string subName = "sub";
			AtomicReference<CountdownEvent> latch = new AtomicReference<CountdownEvent>(new CountdownEvent(1));
			AtomicInteger received = new AtomicInteger();
			var config = new ConsumerConfigBuilder<byte[]>()
				.Topic(topicName)
				.SubscriptionName(subName)
				.ReceiverQueueSize(0)
				.MessageListener(new MessageListener<byte[]>((consumer, msg)=> 
				{
					Assert.NotNull(msg);
					consumer.Tell(new AcknowledgeMessage<byte[]>(msg));
					received.GetAndIncrement();
					latch.Value.AddCount();
				}, null));

			var consumer = _client.NewConsumer(config);
			consumer.Pause();

			var pBuilder = new ProducerConfigBuilder<byte[]>()
				.Topic(topicName)
				.EnableBatching(false);
			Producer<byte[]> producer = _client.NewProducer(pBuilder);

			for(int i = 0; i < 2; i++)
			{
				producer.Send(Encoding.UTF8.GetBytes("my-message-" + i));
			}

			// Paused consumer receives only one message
			//Assert.True(latch.Value.Wait(TimeSpan.FromSeconds(2)));
			//Thread.Sleep(2000);
			//Assert.Equal(1, received.GetValue());

			//latch.GetAndSet(new CountdownEvent(1));
			consumer.Resume();
			Thread.Sleep(10000);
			//Assert.True(latch.Value.Wait(TimeSpan.FromSeconds(2)), "Timed out waiting for message listener acks");

			consumer.Unsubscribe();
			producer.Close();
		}

	}

}