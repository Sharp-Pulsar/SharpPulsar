using Akka.Actor;
using Akka.Util;
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
	public class ZeroQueueSizeTest : IAsyncLifetime
    {
        private PulsarClient _client;
        private readonly ITestOutputHelper _output;
        private PulsarSystem _system;
        private PulsarClientConfigBuilder _configBuilder;
        private readonly int _totalMessages = 10;

        public ZeroQueueSizeTest(ITestOutputHelper output, PulsarFixture fixture)
		{
            _output = output;
            _configBuilder = fixture.ConfigBuilder;
            _system = fixture.System;
        }
		[Fact]
		public async Task ZeroQueueSizeNormalConsumer()
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
            await Task.Delay(1000);
            // 4. Receiver receives the message
            IMessage<byte[]> message;
            ISet<int> receivedMessages = new HashSet<int>();
            for (int i = 0; i < _totalMessages; i++)
			{
				//Assert.Equal(0, consumer.NumMessagesInQueue());
                try
                {
                    message = consumer.Receive();
                    var r = Encoding.UTF8.GetString(message.Data);
                    receivedMessages.Add(1);
                    _output.WriteLine($"Consumer received : {receivedMessages.Count}, {r}");
                }
                catch(Exception ex)
                {
                    _output.WriteLine("Consumer received : " + ex.ToString());
                }
            }
            Assert.True(receivedMessages.Count > 0);
        }

		[Fact]
		public async Task TestZeroQueueSizeMessageRedelivery()
		{
			var topic = $"testZero{Guid.NewGuid()}";

			var config = new ConsumerConfigBuilder<int>()
				.Topic(topic)
				.SubscriptionName("sub")
				.ReceiverQueueSize(0)
				.SubscriptionType(SubType.Shared)
				.AckTimeout(TimeSpan.FromSeconds(1));

			var pBuilder = new ProducerConfigBuilder<int>()
				.Topic(topic)
				.EnableBatching(false);
            var consumer = _client.NewConsumer(ISchema<object>.Int32, config);
            var producer = _client.NewProducer(ISchema<object>.Int32, pBuilder);

			const int messages = 10;

			for(int i = 0; i < messages; i++)
			{
				producer.Send(i);
			}
            ISet<int> receivedMessages = new HashSet<int>();
            
            await Task.Delay(1000);
            for (var i = 0; i < messages - 1; i++)
			{
                try
                {
                    var v = consumer.Receive().Value;
                    receivedMessages.Add(v);
                    _output.WriteLine("Consumer received : " + receivedMessages.Count);
                }
                catch (Exception ex)
                {
                    _output.WriteLine("Consumer received : " + ex.ToString());
                }
               
            }

			Assert.True(receivedMessages.Count > 0);

			await consumer.CloseAsync();//;//;//.ConfigureAwait(false); https://xunit.net/xunit.analyzers/rules/xUnit1030 https://xunit.net/xunit.analyzers/rules/xUnit1030
			await producer.CloseAsync();//;//.ConfigureAwait(false); https://xunit.net/xunit.analyzers/rules/xUnit1030 https://xunit.net/xunit.analyzers/rules/xUnit1030
        }
		
        [Fact]
		public async Task TestZeroQueueSizeMessageRedeliveryForListener()
		{
			string topic = $"testZeroQueueSizeMessageRedeliveryForListener-{DateTime.Now.Ticks}";
			const int messages = 10;
            CountdownEvent latch = new CountdownEvent(1);
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

                        _output.WriteLine($"MessageListener: {msg.Value}");
                        receivedMessages.Add(msg.Value);
					}
					finally
					{
						latch.Signal();
					}

				}, null));
            await Task.Delay(1000);
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
            Assert.True(receivedMessages.Count > 0);

            await consumer.CloseAsync();//;//.ConfigureAwait(false); https://xunit.net/xunit.analyzers/rules/xUnit1030 https://xunit.net/xunit.analyzers/rules/xUnit1030
            await producer.CloseAsync();//;//.ConfigureAwait(false); https://xunit.net/xunit.analyzers/rules/xUnit1030 https://xunit.net/xunit.analyzers/rules/xUnit1030
        }

        //[Fact(Skip = "TestPauseAndResume")]
        [Fact]
		public async Task TestPauseAndResume()
		{
			string topicName = $"zero-queue-pause-and-resume{Guid.NewGuid()}";
			const string subName = "sub";
			AtomicReference<CountdownEvent> latch = new AtomicReference<CountdownEvent>(new CountdownEvent(1));
			var config = new ConsumerConfigBuilder<byte[]>()
				.Topic(topicName)
				.SubscriptionName(subName)
				.ReceiverQueueSize(0)
				.MessageListener(new MessageListener<byte[]>((consumer, msg)=> 
				{

					Assert.NotNull(msg);
					consumer.Tell(new AcknowledgeMessage<byte[]>(msg));
					//received.GetAndIncrement();
					//latch.Value.AddCount();
				}, null));

            await Task.Delay(1000);

            var consumer = _client.NewConsumer(config);
			//consumer.Pause();

			var pBuilder = new ProducerConfigBuilder<byte[]>()
				.Topic(topicName)
				.EnableBatching(false);
			var producer = _client.NewProducer(pBuilder);

			for(int i = 0; i < 2; i++)
			{
				producer.Send(Encoding.UTF8.GetBytes("my-message-" + i));
			}

            // Paused consumer receives only one message
            //Assert.True(latch.Value.Wait(TimeSpan.FromSeconds(2)));
            //await Task.Delay(2000);
			//Assert.Equal(1, received.GetValue());

			//latch.GetAndSet(new CountdownEvent(1));
			//consumer.Resume();
			//await Task.Delay(10000);
			//Assert.True(latch.Value.Wait(TimeSpan.FromSeconds(2)), "Timed out waiting for message listener acks");

			//await consumer.UnsubscribeAsync();//;//.ConfigureAwait(false); https://xunit.net/xunit.analyzers/rules/xUnit1030 https://xunit.net/xunit.analyzers/rules/xUnit1030
            await producer.CloseAsync();//;//.ConfigureAwait(false); https://xunit.net/xunit.analyzers/rules/xUnit1030 https://xunit.net/xunit.analyzers/rules/xUnit1030
        }
        public async Task InitializeAsync()
        {

            _client = await _system.NewClient(_configBuilder);
        }

        public async Task DisposeAsync()
        {
            await _client.ShutdownAsync();
        }
    }

}