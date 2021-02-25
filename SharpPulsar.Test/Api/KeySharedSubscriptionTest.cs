using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using System.Text.Json;
using Akka.Actor;
using Akka.Util.Internal;
using BAMCIS.Util.Concurrent;
using SharpPulsar.Auth;
using SharpPulsar.Batch.Api;
using SharpPulsar.Common;
using SharpPulsar.Configuration;
using SharpPulsar.Exceptions;
using SharpPulsar.Protocol.Proto;
using SharpPulsar.Schemas;
using SharpPulsar.Test.Fixtures;
using SharpPulsar.User;
using SharpPulsar.Extension;
using Xunit;
using Xunit.Abstractions;
using Murmur332Hash = SharpPulsar.Impl.Murmur332Hash;
using Range = SharpPulsar.Common.Range;

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
	[Collection(nameof(PulsarTests))]
	public class KeySharedSubscriptionTest
	{
        private readonly ITestOutputHelper _output;
		private readonly PulsarSystem _system;
		private readonly PulsarClient _client;

		public KeySharedSubscriptionTest(ITestOutputHelper output, PulsarStandaloneClusterFixture fixture)
		{
			_output = output;
			_output = output;
			var client = fixture.ClientBuilder;
			client.OperationTimeout(60000);
			client.Authentication(new AuthenticationDisabled());

			_system = PulsarSystem.GetInstance(client);
			_client = _system.NewClient();
		}
		private static readonly IList<string> Keys = new List<string>{"0", "1", "2", "3", "4", "5", "6", "7", "8", "9"};
		
		private static readonly Random Random = new Random(DateTimeOffset.Now.Millisecond);
		private const int NumberOfKeys = 300;

		private void SendAndReceiveWithHashRangeAutoSplitStickyKeyConsumerSelector(string topicType, bool enableBatch)
		{
			//this.conf.SubscriptionKeySharedEnable = true;
			var topic = topicType + "://public/default/key_shared-" + Guid.NewGuid();
			var producer = CreateProducer(topic, enableBatch);

			for (var i = 0; i < 1000; i++)
			{
				producer.NewMessage().Value(i.ToString().GetBytes()).Key(Random.Next(NumberOfKeys).ToString()).Send();
			}


            var consumer1 = CreateConsumer(topic, $"consumer1-{Guid.NewGuid()}");

            var consumer2 = CreateConsumer(topic, $"consumer2-{Guid.NewGuid()}");

            var consumer3 = CreateConsumer(topic, $"consumer3-{Guid.NewGuid()}");

			ReceiveAndCheckDistribution(new List<Consumer<sbyte[]>>{consumer1, consumer2, consumer3});
		}

		[Fact]
        public void TestSendAndReceiveWithHashRangeAutoSplitStickyKeyConsumerSelectorNoBatch()
        {
            SendAndReceiveWithHashRangeAutoSplitStickyKeyConsumerSelector("persistent", false);
		}
		[Fact]
        public void TestSendAndReceiveWithHashRangeAutoSplitStickyKeyConsumerSelectorBatch()
        {
            SendAndReceiveWithHashRangeAutoSplitStickyKeyConsumerSelector("persistent", true);
		}
		private void SendAndReceiveWithBatching(string topicType, bool enableBatch)
		{
			//this.conf.SubscriptionKeySharedEnable = true;
            var topic = topicType + "://public/default/key_shared-" + Guid.NewGuid();

            var producer = CreateProducer(topic, enableBatch);

            for (var i = 0; i < 1000; i++)
            {
				var v = i.ToString().GetBytes();
				var k = Random.Next(NumberOfKeys).ToString();
				// Send the same key twice so that we'll have a batch message
				producer.NewMessage().Value(v).Key(k).Send();
				producer.NewMessage().Value(v).Key(k).Send();
            }


            var consumer1 = CreateConsumer(topic, $"consumer1-{Guid.NewGuid()}");

            var consumer2 = CreateConsumer(topic, $"consumer2-{Guid.NewGuid()}");

            var consumer3 = CreateConsumer(topic, $"consumer3-{Guid.NewGuid()}");

			ReceiveAndCheckDistribution(new List<Consumer<sbyte[]>> { consumer1, consumer2, consumer3 });

		}

		[Fact]
        public void TestSendAndReceiveWithBatching()
        {
            SendAndReceiveWithBatching("persistent", true);
		}

        private void SendAndReceiveWithHashRangeExclusiveStickyKeyConsumerSelector(bool enableBatch)
		{
			//this.conf.SubscriptionKeySharedEnable = true;
			var topic = "persistent://public/default/key_shared_exclusive-" + Guid.NewGuid();

            var producer = CreateProducer(topic, enableBatch);

			var consumer1ExpectMessages = 0;
			var consumer2ExpectMessages = 0;
			var consumer3ExpectMessages = 0;

			for (var i = 0; i < 10; i++)
			{
				foreach (var key in Keys)
				{
					var slot = Murmur332Hash.Instance.MakeHash((sbyte[])(object)key.GetBytes()) % KeySharedPolicy.DefaultHashRangeSize;
					if (slot <= 20000)
					{
						consumer1ExpectMessages++;
					}
					else if (slot <= 40000)
					{
						consumer2ExpectMessages++;
					}
					else
					{
						consumer3ExpectMessages++;
					}
					producer.NewMessage().Value(i.ToString().GetBytes())
						.Key(key).Send();
				}
			}


            var c1 = CreateConsumer(topic, $"consumer1-{Guid.NewGuid()}", KeySharedPolicy.StickyHashRange().GetRanges(Range.Of(0, 20000)));

            var c2 = CreateConsumer(topic, $"consumer2-{Guid.NewGuid()}", KeySharedPolicy.StickyHashRange().GetRanges(Range.Of(20001, 40000)));

            var c3 = CreateConsumer(topic, $"consumer3-{Guid.NewGuid()}", KeySharedPolicy.StickyHashRange().GetRanges(Range.Of(40001, KeySharedPolicy.DefaultHashRangeSize)));

            IList<KeyValue<Consumer<sbyte[]>, int>> checkList = new List<KeyValue<Consumer<sbyte[]>, int>>
            {
                new KeyValue<Consumer<sbyte[]>, int>(c1, consumer1ExpectMessages),
                new KeyValue<Consumer<sbyte[]>, int>(c2, consumer2ExpectMessages),
                new KeyValue<Consumer<sbyte[]>, int>(c3, consumer3ExpectMessages)
            };

            ReceiveAndCheck(checkList);

		}

		[Fact]
        public void TestSendAndReceiveWithHashRangeExclusiveStickyKeyConsumerSelectorNoBatch()
        {
            SendAndReceiveWithHashRangeExclusiveStickyKeyConsumerSelector(false);
		}
		[Fact]
        public void TestSendAndReceiveWithHashRangeExclusiveStickyKeyConsumerSelectorBatch()
        {
            SendAndReceiveWithHashRangeExclusiveStickyKeyConsumerSelector(true);
		}
		
		private void NonKeySendAndReceiveWithHashRangeAutoSplitStickyKeyConsumerSelector(string topicType, bool enableBatch)
		{
			//this.conf.SubscriptionKeySharedEnable = true;
			var topic = topicType + "://public/default/key_shared_none_key-" + Guid.NewGuid();

            var consumer1 = CreateConsumer(topic, $"consumer1-{Guid.NewGuid()}");

            var consumer2 = CreateConsumer(topic, $"consumer2-{Guid.NewGuid()}");

            var consumer3 = CreateConsumer(topic, $"consumer3-{Guid.NewGuid()}");

            var producer = CreateProducer(topic, enableBatch);

            for (var i = 0; i < 1000; i++)
			{
				producer.NewMessage().Value(i.ToString().GetBytes())
					.Send();
            }
			Receive(new List<Consumer<sbyte[]>>{ consumer1, consumer2, consumer3 });
		}

		[Fact]
        public void TestNonKeySendAndReceiveWithHashRangeAutoSplitStickyKeyConsumerSelectorNoBatch()
        {
            NonKeySendAndReceiveWithHashRangeAutoSplitStickyKeyConsumerSelector("persistent", false);
		}
		[Fact]
        public void TestNonKeySendAndReceiveWithHashRangeAutoSplitStickyKeyConsumerSelectorBatch()
        {

			NonKeySendAndReceiveWithHashRangeAutoSplitStickyKeyConsumerSelector("persistent", true);
        }
		private void OrderingKeyWithHashRangeAutoSplitStickyKeyConsumerSelector(bool enableBatch)
		{
			//this.conf.SubscriptionKeySharedEnable = true;
			var topic = "persistent://public/default/key_shared_ordering_key-" + Guid.NewGuid();

            var producer = CreateProducer(topic, enableBatch, 20);


			for (var i = 0; i < 1000; i++)
			{
				var ok = Encoding.UTF8.GetBytes(Random.Next(NumberOfKeys).ToString()).ToSBytes();
				producer.NewMessage().Value(i.ToString().GetBytes()).Key("any key").OrderingKey(ok).
					Send();
			}


            var consumer1 = CreateConsumer(topic, $"consumer1-{Guid.NewGuid()}");

            var consumer2 = CreateConsumer(topic, $"consumer2-{Guid.NewGuid()}");

            var consumer3 = CreateConsumer(topic, $"consumer3-{Guid.NewGuid()}");
			ReceiveAndCheckDistribution(new List<Consumer<sbyte[]>> { consumer1, consumer2, consumer3 });
		}
        [Fact]
        public void TestOrderingKeyWithHashRangeAutoSplitStickyKeyConsumerSelectorNoBatch()
        {
             OrderingKeyWithHashRangeAutoSplitStickyKeyConsumerSelector(false);
        }
		[Fact]
        public void TestOrderingKeyWithHashRangeAutoSplitStickyKeyConsumerSelectorBatch()
        {
            OrderingKeyWithHashRangeAutoSplitStickyKeyConsumerSelector(true);
        }
		private Producer<sbyte[]> CreateProducer(string topic, bool enableBatch, int batchSize = 500)
        {
			var pBuilder = new ProducerConfigBuilder<sbyte[]>();
			pBuilder.Topic(topic);
			if (enableBatch)
            {
				pBuilder.EnableBatching(true);
				pBuilder.BatchBuilder(IBatcherBuilder.KeyBased(_client.ActorSystem));
				pBuilder.BatchingMaxMessages(batchSize);
				pBuilder.BatchingMaxPublishDelay(5000);
			}

			return _client.NewProducer(pBuilder);
        }

        private Consumer<sbyte[]> CreateConsumer(string topic, string consumerSub, KeySharedPolicy keySharedPolicy = null)
        {
			var builder = new ConsumerConfigBuilder<sbyte[]>();
			builder.Topic(topic);
			builder.SubscriptionName(consumerSub);
			builder.AckTimeout(30000, TimeUnit.MILLISECONDS);
			builder.ForceTopicCreation(true);
			if(keySharedPolicy != null)
				builder.KeySharedPolicy(keySharedPolicy);
			builder.SubscriptionType(CommandSubscribe.SubType.KeyShared);
			return _client.NewConsumer(builder);
		}

        private void Receive(IList<Consumer<sbyte[]>> consumers)
		{
			// Add a key so that we know this key was already assigned to one consumer

            IDictionary<string, Consumer<sbyte[]>> keyToConsumer = new Dictionary<string, Consumer<sbyte[]>>();

            foreach (var c in consumers)
			{
				while (true)
				{
                    var msg = c.Receive();
					if (msg == null)
					{
						// Go to next consumer
						break;
					}
					_output.WriteLine(Encoding.UTF8.GetString((byte[])(Array)msg.Data));

					c.Acknowledge(msg);

					if (msg.HasKey())
					{
                        var assignedConsumer = keyToConsumer[msg.Key];
						if (!keyToConsumer.ContainsKey(msg.Key))
						{
							// This is a new key
							keyToConsumer[msg.Key] = c;
						}
						else
						{
							// The consumer should be the same
							Assert.Equal(c, assignedConsumer);
						}
					}
				}
			}
		}

		/// <summary>
		/// Check that every consumer receives a fair number of messages and that same key is delivered to only 1 consumer
		/// </summary>
        private void ReceiveAndCheckDistribution(IList<Consumer<sbyte[]>> consumers)
		{
			// Add a key so that we know this key was already assigned to one consumer

            IDictionary<string, Consumer<sbyte[]>> keyToConsumer = new Dictionary<string, Consumer<sbyte[]>>();

            IDictionary<Consumer<sbyte[]>, int> messagesPerConsumer = new Dictionary<Consumer<sbyte[]>, int>();

			var totalMessages = 0;


            foreach (var c in consumers)
			{
				var messagesForThisConsumer = 0;
                while (true)
                {
					var msg = c.Receive(100);
                    if (msg == null)
                    {
                        // Go to next consumer
                        messagesPerConsumer[c] = messagesForThisConsumer;
                        break;
                    }

                    ++totalMessages;
                    ++messagesForThisConsumer;
                    c.Acknowledge(msg); 
                    
                    if (msg.HasKey() || msg.HasOrderingKey())
                    {
                        var orderingKey = msg.HasOrderingKey() ? Encoding.UTF8.GetString((byte[])(Array)msg.OrderingKey): string.Empty;

						string key = msg.HasOrderingKey() ? orderingKey : msg.Key;
                        
                        if (!keyToConsumer.TryGetValue(key, out var assignedConsumer))
                        {
                            // This is a new key
                            keyToConsumer[key] = c;
                        }
                        else
                        {
                            // The consumer should be the same
							_output.WriteLine($"The consumer should be the same: [{assignedConsumer}] > (OrderingKey: {orderingKey}, Key: {msg.Key})");
                            Assert.Equal(c, assignedConsumer);

                        }
                    }
				}
			}

			const double percentError = 0.40; // 40 %

			var expectedMessagesPerConsumer = (double)totalMessages / consumers.Count;

			_output.WriteLine(JsonSerializer.Serialize(messagesPerConsumer, new JsonSerializerOptions{WriteIndented = true}));
			foreach (var count in messagesPerConsumer.Values)
			{
				Assert.Equal(expectedMessagesPerConsumer, expectedMessagesPerConsumer * percentError, count);
			}
		}

        private void ReceiveAndCheck(IEnumerable<KeyValue<Consumer<sbyte[]>, int>> checkList)
		{
			var consumerKeys = new Dictionary<Consumer<sbyte[]>, ISet<string>>();
			foreach (var check in checkList)
			{
				if (check.Value % 2 != 0)
				{
					throw new ArgumentException();
				}
				var received = 0;
				var lastMessageForKey = new Dictionary<string, Message<sbyte[]>>();
				for (int? i = 0; i.Value < check.Value; i++)
				{
					var message = check.Key.Receive();
					if (i % 2 == 0)
					{
						check.Key.Acknowledge(message);
					}
					var key = message.HasOrderingKey() ? Encoding.UTF8.GetString((byte[])(Array)message.OrderingKey) : message.Key;
					_output.WriteLine($"[{check.Key}] Receive message key: {key} value: {Encoding.UTF8.GetString((byte[])(Array)message.Data)} messageId: {message.MessageId}");
					// check messages is order by key
					if (!lastMessageForKey.TryGetValue(key, out var msgO))
					{
						Assert.NotNull(message);
					}
					else
                    {
						var l = Convert.ToInt32(Encoding.UTF8.GetString((byte[])(Array)msgO.Data));
						var o = Convert.ToInt32(Encoding.UTF8.GetString((byte[])(Array)message.Data));
						Assert.True(o.CompareTo(l) > 0);
					}
					lastMessageForKey[key] = (Message<sbyte[]>)message;
					if (!consumerKeys.ContainsKey(check.Key)) 
                        consumerKeys.Add(check.Key, new HashSet<string>());
					consumerKeys[check.Key].Add(key);
					received++;
				}
				Assert.Equal(check.Value, received);
				var redeliveryCount = check.Value / 2;
				_output.WriteLine($"[{check.Key}] Consumer wait for {redeliveryCount} messages redelivery ...");
				// messages not acked, test redelivery
				lastMessageForKey = new Dictionary<string, Message<sbyte[]>>();
				for (var i = 0; i < redeliveryCount; i++)
				{
                    var message = check.Key.Receive();
					received++;
					check.Key.Acknowledge(message);
					var key = message.HasOrderingKey() ? Encoding.UTF8.GetString((byte[])(Array)message.OrderingKey) : message.Key;
                    _output.WriteLine($"[{check.Key}] Receive message key: {key} value: {Encoding.UTF8.GetString((byte[])(Array)message.Data)} messageId: {message.MessageId}");
					// check redelivery messages is order by key
					if (!lastMessageForKey.TryGetValue(key, out var msgO))
                    {
                        Assert.NotNull(message);
                    }
                    else
                    {
                        var l = Convert.ToInt32(Encoding.UTF8.GetString((byte[])(Array)msgO.Data));
                        var o = Convert.ToInt32(Encoding.UTF8.GetString((byte[])(Array)message.Data));
                        Assert.True(o.CompareTo(l) > 0);
                    }
                    lastMessageForKey[key] = (Message<sbyte[]>)message;
				}
				Message<sbyte[]> noMessages = null;
				try
				{
					noMessages = (Message<sbyte[]>)check.Key.Receive(100);
				}
				catch (PulsarClientException)
				{
				}
				Assert.Null(noMessages);//, "redeliver too many messages.");
				Assert.Equal((check.Value + redeliveryCount), received);
			}
			ISet<string> allKeys = new HashSet<string>();
			consumerKeys.ForEach(x =>
            {
				x.Value.ForEach(key =>
                {
					Assert.True(allKeys.Add(key), "Key " + key + "is distributed to multiple consumers.");
                });
            });
		}

	}

}