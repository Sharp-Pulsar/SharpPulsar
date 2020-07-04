using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using System.Text.Json;
using Akka.Actor;
using Akka.Util.Internal;
using SharpPulsar.Akka.InternalCommands;
using SharpPulsar.Api;
using SharpPulsar.Batch.Api;
using SharpPulsar.Common.Schema;
using SharpPulsar.Exceptions;
using SharpPulsar.Impl;
using SharpPulsar.Impl.Auth;
using SharpPulsar.Impl.Schema;
using SharpPulsar.Protocol.Proto;
using Xunit;
using Xunit.Abstractions;
using Murmur332Hash = SharpPulsar.Impl.Murmur332Hash;
using Range = SharpPulsar.Api.Range;

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
    public class KeySharedSubscriptionTest : ProducerConsumerBase
	{
        private readonly ITestOutputHelper _output;
        private readonly TestCommon.Common _common;

        public KeySharedSubscriptionTest(ITestOutputHelper output)
        {
            _output = output;
            _output = output;
            _common = new TestCommon.Common(output);
            _common.GetPulsarSystem(new AuthenticationDisabled());
            ProducerBaseSetup(_common.PulsarSystem, output);
        }
		private static readonly IList<string> Keys = new List<string>{"0", "1", "2", "3", "4", "5", "6", "7", "8", "9"};
		
		private static readonly Random Random = new Random(DateTimeOffset.Now.Millisecond);
		private const int NumberOfKeys = 300;

		private void SendAndReceiveWithHashRangeAutoSplitStickyKeyConsumerSelector(string topicType, bool enableBatch)
		{
			//this.conf.SubscriptionKeySharedEnable = true;
			string topic = topicType + "://public/default/key_shared-" + Guid.NewGuid();

			var consumer1 = CreateConsumer(topic, "consumer1");

			var consumer2 = CreateConsumer(topic, "consumer2");

			var consumer3 = CreateConsumer(topic, "consumer3");

			var producer = CreateProducer(topic, enableBatch, "HashRangeAutoSplitSticky");

			for (int i = 0; i < 1000; i++)
			{
                var config = new Dictionary<string, object> {["key"] = Random.Next(NumberOfKeys).ToString()};
                var send = new Send(i.ToString(), topic, config.ToImmutableDictionary());
				_common.PulsarSystem.Send(send, producer);
			}

			ReceiveAndCheckDistribution(new List<(IActorRef consr, string name)>{(consumer1.consumer, "consumer1"), (consumer2.consumer, "consumer2"), (consumer3.consumer, "consumer3") });
		}

		[Fact]
        public void TestSendAndReceiveWithHashRangeAutoSplitStickyKeyConsumerSelectorNoBatch()
        {
            SendAndReceiveWithHashRangeAutoSplitStickyKeyConsumerSelector("persistent", false);
            _common.PulsarSystem.Stop();
            _common.PulsarSystem = null;
		}
		[Fact]
        public void TestSendAndReceiveWithHashRangeAutoSplitStickyKeyConsumerSelectorBatch()
        {
            SendAndReceiveWithHashRangeAutoSplitStickyKeyConsumerSelector("persistent", true);
            _common.PulsarSystem.Stop();
            _common.PulsarSystem = null;
		}
		private void SendAndReceiveWithBatching(string topicType, bool enableBatch)
		{
			//this.conf.SubscriptionKeySharedEnable = true;
            string topic = topicType + "://public/default/key_shared-" + Guid.NewGuid();

            var consumer1 = CreateConsumer(topic, "consumer1");

            var consumer2 = CreateConsumer(topic, "consumer2");

            var consumer3 = CreateConsumer(topic, "consumer3");

            var producer = CreateProducer(topic, enableBatch, "TestSendAndReceiveWithBatching");

            for (int i = 0; i < 1000; i++)
            {
                var config = new Dictionary<string, object> { ["key"] = Random.Next(NumberOfKeys).ToString() };
                var send = new Send(i.ToString(), topic, config.ToImmutableDictionary());

                // Send the same key twice so that we'll have a batch message
				_common.PulsarSystem.Send(send, producer);
				_common.PulsarSystem.Send(send, producer);
            }

            ReceiveAndCheckDistribution(new List<(IActorRef consr, string name)> { (consumer1.consumer, "consumer1"), (consumer2.consumer, "consumer2"), (consumer3.consumer, "consumer3") });

		}

		[Fact]
        public void TestSendAndReceiveWithBatching()
        {
            SendAndReceiveWithBatching("persistent", true);
            _common.PulsarSystem.Stop();
            _common.PulsarSystem = null;
		}

        private void SendAndReceiveWithHashRangeExclusiveStickyKeyConsumerSelector(bool enableBatch)
		{
			//this.conf.SubscriptionKeySharedEnable = true;
			var topic = "persistent://public/default/key_shared_exclusive-" + Guid.NewGuid();

            CreateConsumer(topic, "consumer1", KeySharedPolicy.StickyHashRange().GetRanges(Range.Of(0, 20000)));

            CreateConsumer(topic, "consumer2", KeySharedPolicy.StickyHashRange().GetRanges(Range.Of(20001, 40000)));

            CreateConsumer(topic, "consumer3", KeySharedPolicy.StickyHashRange().GetRanges(Range.Of(40001, KeySharedPolicy.DefaultHashRangeSize)));
			
            var producer = CreateProducer(topic, enableBatch, "TestSendAndReceiveWithHashRangeExclusiveStickyKeyConsumerSelector");

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
                    var config = new Dictionary<string, object> { ["key"] = key };
                    var send = new Send(i.ToString(), topic, config.ToImmutableDictionary());

                    _common.PulsarSystem.Send(send, producer);
				}
			}

			IList<KeyValue<string, int>> checkList = new List<KeyValue<string, int>>();
			checkList.Add(new KeyValue<string, int>("consumer1", consumer1ExpectMessages));
			checkList.Add(new KeyValue<string, int>("consumer2", consumer2ExpectMessages));
			checkList.Add(new KeyValue<string, int>("consumer3", consumer3ExpectMessages));

			ReceiveAndCheck(checkList);

		}

		[Fact]
        public void TestSendAndReceiveWithHashRangeExclusiveStickyKeyConsumerSelectorNoBatch()
        {
            SendAndReceiveWithHashRangeExclusiveStickyKeyConsumerSelector(false);
            _common.PulsarSystem.Stop();
            _common.PulsarSystem = null;

		}
		[Fact]
        public void TestSendAndReceiveWithHashRangeExclusiveStickyKeyConsumerSelectorBatch()
        {
            SendAndReceiveWithHashRangeExclusiveStickyKeyConsumerSelector(true);
            _common.PulsarSystem.Stop();
            _common.PulsarSystem = null;

		}
		
		private void NonKeySendAndReceiveWithHashRangeAutoSplitStickyKeyConsumerSelector(string topicType, bool enableBatch)
		{
			//this.conf.SubscriptionKeySharedEnable = true;
			string topic = topicType + "://public/default/key_shared_none_key-" + Guid.NewGuid();

            var consumer1 = CreateConsumer(topic, "consumer1");

            var consumer2 = CreateConsumer(topic, "consumer2");

            var consumer3 = CreateConsumer(topic, "consumer3");

            var producer = CreateProducer(topic, enableBatch, "TestNonKeySendAndReceiveWithHashRangeAutoSplitStickyKeyConsumerSelector");

            for (int i = 0; i < 1000; i++)
			{ 
                var send = new Send(i.ToString(), topic, ImmutableDictionary<string, object>.Empty);

                _common.PulsarSystem.Send(send, producer);
            }
			Receive(new List<string>{ "consumer1", "consumer2", "consumer3" });
		}

		[Fact]
        public void TestNonKeySendAndReceiveWithHashRangeAutoSplitStickyKeyConsumerSelectorNoBatch()
        {
            NonKeySendAndReceiveWithHashRangeAutoSplitStickyKeyConsumerSelector("persistent", false);
            _common.PulsarSystem.Stop();
            _common.PulsarSystem = null;
		}
		[Fact]
        public void TestNonKeySendAndReceiveWithHashRangeAutoSplitStickyKeyConsumerSelectorBatch()
        {

			NonKeySendAndReceiveWithHashRangeAutoSplitStickyKeyConsumerSelector("persistent", true);
            _common.PulsarSystem.Stop();
            _common.PulsarSystem = null;
        }
		private void OrderingKeyWithHashRangeAutoSplitStickyKeyConsumerSelector(bool enableBatch)
		{
			//this.conf.SubscriptionKeySharedEnable = true;
			string topic = "persistent://public/default/key_shared_ordering_key-" + Guid.NewGuid();

			var consumer1 = CreateConsumer(topic, "consumer1");

            var consumer2 = CreateConsumer(topic, "consumer2");

            var consumer3 = CreateConsumer(topic, "consumer3");

            var producer = CreateProducer(topic, enableBatch, "TestNonKeySendAndReceiveWithHashRangeAutoSplitStickyKeyConsumerSelector");


			for (int i = 0; i < 1000; i++)
			{
                var config = new Dictionary<string, object>
                {
                    ["key"] = "any key",
					["orderingKey"] = Random.Next(NumberOfKeys).ToString().GetBytes()
				};
                var send = new Send(i.ToString(), topic, config.ToImmutableDictionary());
                _common.PulsarSystem.Send(send, producer);

			}

            ReceiveAndCheckDistribution(new List<(IActorRef consr, string name)> { (consumer1.consumer, "consumer1"), (consumer2.consumer, "consumer2"), (consumer3.consumer, "consumer3") });
		}
        [Fact]
        public void TestOrderingKeyWithHashRangeAutoSplitStickyKeyConsumerSelectorNoBatch()
        {
             OrderingKeyWithHashRangeAutoSplitStickyKeyConsumerSelector(false);
             _common.PulsarSystem.Stop();
            _common.PulsarSystem = null;
        }
		[Fact]
        public void TestOrderingKeyWithHashRangeAutoSplitStickyKeyConsumerSelectorBatch()
        {
            OrderingKeyWithHashRangeAutoSplitStickyKeyConsumerSelector(true);
			_common.PulsarSystem.Stop();
            _common.PulsarSystem = null;
        }
		private IActorRef CreateProducer(string topic, bool enableBatch, string producerName)
        {
            if (enableBatch)
            {
                var system = _common.PulsarSystem.GeTestObject().ActorSystem;
				return _common.PulsarSystem.PulsarProducer(_common.CreateProducer(BytesSchema.Of(), topic, producerName, batchingMaxMessages: 5000, batcherBuilder: BatcherBuilderFields.KeyBased(system))).Producer;
			}

            return _common.PulsarSystem.PulsarProducer(_common.CreateProducer(BytesSchema.Of(), topic, producerName)).Producer;
        }

        private (IActorRef consumer, string topic) CreateConsumer(string topic, string consumerName, KeySharedPolicy keySharedPolicy = null)
        {
            var con = _common.PulsarSystem.PulsarConsumer(_common.CreateConsumer(BytesSchema.Of(), topic, consumerName, $"{consumerName}-Sub", subType: CommandSubscribe.SubType.KeyShared, ackTimeout: 3000, keySharedPolicy: keySharedPolicy));
			return con;
		}

        private void Receive(IList<string> consumers)
		{
			// Add a key so that we know this key was already assigned to one consumer

            IDictionary<string, string> keyToConsumer = new Dictionary<string, string>();

            foreach (var c in consumers)
			{
				while (true)
				{
                    var msg = _common.PulsarSystem.Receive(c, 100);
					if (msg == null)
					{
						// Go to next consumer
						break;
					}

					_common.PulsarSystem.Acknowledge(msg);

					if (msg.Message.HasKey())
					{
                        var assignedConsumer = keyToConsumer[msg.Message.Key];
						if (!keyToConsumer.ContainsKey(msg.Message.Key))
						{
							// This is a new key
							keyToConsumer[msg.Message.Key] = c;
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
        private void ReceiveAndCheckDistribution(IList<(IActorRef consr, string name)> consumers)
		{
			// Add a key so that we know this key was already assigned to one consumer

            IDictionary<string, IActorRef> keyToConsumer = new Dictionary<string, IActorRef>();

            IDictionary<IActorRef, int> messagesPerConsumer = new Dictionary<IActorRef, int>();

			int totalMessages = 0;


            foreach (var c in consumers)
			{
				int messagesForThisConsumer = 0;
                var messages = _common.PulsarSystem.Messages(c.name, true, customHander: (m) =>
                {
                    var receivedMessage = Convert.ToInt32(Encoding.UTF8.GetString((byte[])(object)m.Message.Data));
                    
                    if (m.Message.HasKey() || m.Message.HasOrderingKey())
                    {
                        var key = m.Message.HasOrderingKey() ? ((byte[])(object)m.Message.OrderingKey).GetString() : m.Message.Key;


                        if (keyToConsumer.ContainsKey(key))
                        {
                            // This is a new key
                            keyToConsumer[key] = c.consr;
                        }
                        else
                        {
                            // The consumer should be the same
                            Assert.Equal(c.consr, keyToConsumer[key]);
                        }
                    }
					return receivedMessage;
                });
                foreach (var message in messages)
                {
                    _output.WriteLine($"Received message: [{message}]");
                    ++totalMessages;
                    ++messagesForThisConsumer;
				}
                messagesPerConsumer[c.consr] = messagesForThisConsumer;
			}

			const double percentError = 0.40; // 40 %

			double expectedMessagesPerConsumer = (double)totalMessages / consumers.Count;

			_output.WriteLine(JsonSerializer.Serialize(messagesPerConsumer, new JsonSerializerOptions{WriteIndented = true}));
			foreach (int count in messagesPerConsumer.Values)
			{
				Assert.assertEquals(count, expectedMessagesPerConsumer, expectedMessagesPerConsumer * percentError);
			}
		}

        private void ReceiveAndCheck(IEnumerable<KeyValue<string, int>> checkList)
		{
			var consumerKeys = new Dictionary<string, ISet<string>>();
			foreach (var check in checkList)
			{
				if (check.Value % 2 != 0)
				{
					throw new System.ArgumentException();
				}
				int received = 0;
				var lastMessageForKey = new Dictionary<string, Message>();
				for (int? i = 0; i.Value < check.Value; i++)
				{
					var message = _common.PulsarSystem.Receive(check.Key);
					if (i % 2 == 0)
					{
						_common.PulsarSystem.Acknowledge(message);
					}
					string key = message.Message.HasOrderingKey() ? ((byte[])(object)message.Message.OrderingKey).GetString() : message.Message.Key;
					_output.WriteLine($"[{check.Key}] Receive message key: {key} value: {((byte[])message.Message.Value).GetString()} messageId: {message.Message.MessageId}");
					// check messages is order by key
					if (!lastMessageForKey.TryGetValue(key, out var msgO))
					{
						Assert.NotNull(message);
					}
					else
                    {
                        var l = Convert.ToInt32(((byte[])msgO.Value).GetString());
						var o = Convert.ToInt32(((byte[])message.Message.Value).GetString());
						Assert.True(o.CompareTo(l) > 0);
					}
					lastMessageForKey[key] = (Message)message.Message;
					if (!consumerKeys.ContainsKey(check.Key)) 
                        consumerKeys.Add(check.Key, new HashSet<string>());
					consumerKeys[check.Key].Add(key);
					received++;
				}
				Assert.Equal(check.Value, received);
				int redeliveryCount = check.Value / 2;
				_output.WriteLine($"[{check.Key}] Consumer wait for {redeliveryCount} messages redelivery ...");
				// messages not acked, test redelivery
				lastMessageForKey = new Dictionary<string, Message>();
				for (int i = 0; i < redeliveryCount; i++)
				{
                    var message = _common.PulsarSystem.Receive(check.Key);
					received++;
					_common.PulsarSystem.Acknowledge(message);
					string key = message.Message.HasOrderingKey() ? ((byte[])(object)message.Message.OrderingKey).GetString() : message.Message.Key;
                    _output.WriteLine($"[{check.Key}] Receive message key: {key} value: {((byte[])message.Message.Value).GetString()} messageId: {message.Message.MessageId}");
					// check redelivery messages is order by key
					if (!lastMessageForKey.TryGetValue(key, out var msgO))
                    {
                        Assert.NotNull(message);
                    }
                    else
                    {
                        var l = Convert.ToInt32(((byte[])msgO.Value).GetString());
                        var o = Convert.ToInt32(((byte[])message.Message.Value).GetString());
                        Assert.True(o.CompareTo(l) > 0);
                    }
                    lastMessageForKey[key] = (Message)message.Message;
				}
				Message noMessages = null;
				try
				{
					noMessages = (Message)_common.PulsarSystem.Receive(check.Key, 100).Message;
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