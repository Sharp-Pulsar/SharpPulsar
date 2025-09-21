using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using SharpPulsar.Builder;
using SharpPulsar.Test.Fixture;
using SharpPulsar.TestContainer;
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
namespace SharpPulsar.Test
{
    [Collection(nameof(PulsarCollection))]
    public class ConsumerRedeliveryTest : IAsyncLifetime
    {
        private PulsarClient _client;
        private readonly ITestOutputHelper _output;
        //private TaskCompletionSource<PulsarClient> _tcs;
        private PulsarSystem _system;
        private PulsarClientConfigBuilder _configBuilder;
        public ConsumerRedeliveryTest(ITestOutputHelper output, PulsarFixture fixture)
        {
            _output = output;
            _configBuilder = fixture.ConfigBuilder;
            _system = fixture.System;
        }

        /// <summary>
        /// It verifies that redelivered messages are sorted based on the ledger-ids.
        /// <pre>
        /// 1. client publishes 100 messages across 50 ledgers
        /// 2. broker delivers 100 messages to consumer
        /// 3. consumer ack every alternative message and doesn't ack 50 messages
        /// 4. broker sorts replay messages based on ledger and redelivers messages ledger by ledger
        /// </pre> </summary>
        /// <exception cref="Exception"> </exception>
        /// 
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task TestOrderedRedelivery(bool ackReceiptEnabled)
        {
            var topic = "persistent://public/default/redelivery-" + DateTimeHelper.CurrentUnixTimeMillis();

            //broker.conf
            //conf.setManagedLedgerMaxEntriesPerLedger(2);
            //conf.setManagedLedgerMinLedgerRolloverTimeMinutes(0);

            var pBuilder = new ProducerConfigBuilder<byte[]>()
            .Topic(topic)
            .ProducerName("my-producer-name");
            var producer = await _client.NewProducerAsync(pBuilder);

            var builder = new ConsumerConfigBuilder<byte[]>()
            .Topic(topic)
            .SubscriptionName("s1")
            .IsAckReceiptEnabled(ackReceiptEnabled)
            .SubscriptionType(CommandSubscribe.SubType.Shared);
            var consumer1 = await _client.NewConsumerAsync(builder);

            const int totalMsgs = 100;

            for (var i = 0; i < totalMsgs; i++)
            {
                var message = "my-message-" + i;
                var receipt = await producer.SendAsync(Encoding.UTF8.GetBytes(message));
               _output.WriteLine(JsonSerializer.Serialize(receipt, new JsonSerializerOptions { WriteIndented = true }));
            }


            var consumedCount = 0;
            var messageIds = new HashSet<IMessageId>();
            for (var i = 0; i < totalMsgs; i++)
            {
                var message = (Message<byte[]>)await consumer1.ReceiveAsync(TimeSpan.FromMicroseconds(5000));
                if (message != null && (consumedCount % 2) == 0)
                {
                    consumer1.Acknowledge(message);
                }
                else
                {
                    messageIds.Add(message.MessageId);
                }
                var receivedMessage = Encoding.UTF8.GetString(message.Data);
                _output.WriteLine($"Received message: [{receivedMessage}]");
                
                consumedCount += 1;
            }
            Assert.Equal(totalMsgs, consumedCount);

            // redeliver all unack messages
            await consumer1.RedeliverUnacknowledgedMessagesAsync(messageIds);
            _output.WriteLine($"MessageIds: [{messageIds.Count}]");
            //await Task.Delay(1000);
            MessageIdAdv lastMsgId = null;
            var count = 1;
            for (var i = 0; i < totalMsgs / 2; i++)
            {
                var message = (Message<byte[]>)await consumer1.ReceiveAsync(TimeSpan.FromMicroseconds(5000));
                if (message != null)
                {
                    var msgId = (MessageIdAdv)message.MessageId;
                    if (lastMsgId != null)
                    {
                        Assert.True(lastMsgId.LedgerId <= msgId.LedgerId, "lastMsgId: " + lastMsgId + " -- msgId: " + msgId);
                    }

                    lastMsgId = msgId;
                    _output.WriteLine($"{count++} MessageId: [{lastMsgId}]");
                }
                
            }

            // close consumer so, this consumer's unack messages will be redelivered to new consumer
            consumer1.Close();

           /* var consumer2 = await _client.NewConsumerAsync(builder);

            await Task.Delay(TimeSpan.FromSeconds(10));
            count = 0;
            lastMsgId = null;
            for (var i = 0; i < totalMsgs / 2; i++)
            {
                
                var message = (Message<byte[]>)await consumer2.ReceiveAsync(TimeSpan.FromMicroseconds(5000));
                if (message != null)
                {
                    var msgId = (MessageId)message.MessageId;
                    if (lastMsgId != null)
                    {
                        Assert.True(lastMsgId.LedgerId <= msgId.LedgerId);
                    }
                    lastMsgId = msgId;
                    _output.WriteLine($"{count++} RedeliverUnacknowledgedMessage MessageId: [{lastMsgId}]");
                }                
            }*/
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task TestUnAckMessageRedeliveryWithReceive(bool ackReceiptEnabled)
        {
            var topic = $"persistent://public/default/async-unack-redelivery-{Guid.NewGuid()}";

            var builder = new ConsumerConfigBuilder<byte[]>()
            .Topic(topic)
            .SubscriptionName("sub-TestUnAckMessageRedeliveryWithReceive")
            .AckTimeout(TimeSpan.FromMilliseconds(3000))
            .IsAckReceiptEnabled(ackReceiptEnabled)
            .EnableBatchIndexAcknowledgment(ackReceiptEnabled);

            var consumer = await _client.NewConsumerAsync(builder);

            var pBuilder = new ProducerConfigBuilder<byte[]>()
            .Topic(topic)
            .EnableBatching(true)
            .BatchingMaxMessages(5)
            .BatchingMaxPublishDelay(TimeSpan.FromSeconds(1));
            var producer = await _client.NewProducerAsync(pBuilder);

            const int messages = 10;

            for (var i = 0; i < messages; i++)
            {
                await producer.SendAsync(Encoding.UTF8.GetBytes("my-message-" + i));
                //_output.WriteLine(JsonSerializer.Serialize(receipt, new JsonSerializerOptions { WriteIndented = true }));
            }
            var messageIds = new HashSet<IMessageId>();
            var messageReceived = 0;
            await Task.Delay(TimeSpan.FromMilliseconds(1000));
            for (var i = 0; i < messages; ++i)
            {
                var m = (Message<byte[]>)await consumer.ReceiveAsync(TimeSpan.FromMicroseconds(5000));
                if (m != null)
                {
                    _output.WriteLine($"BrokerEntryMetadata[timestamp:{m.BrokerEntryMetadata.BrokerTimestamp} index: {m.BrokerEntryMetadata?.Index.ToString()}");
                    var receivedMessage = Encoding.UTF8.GetString(m.Data);
                    _output.WriteLine($"Received message: [{receivedMessage}]");
                    messageReceived++;
                    messageIds.Add(m.MessageId);
                }
            }
            Assert.Equal(10, messageReceived);
            // redeliver all unack messages
            await consumer.RedeliverUnacknowledgedMessagesAsync(messageIds);
            //Assert.True(messageReceived > 0);
            await Task.Delay(TimeSpan.FromSeconds(5));
            for (var i = 0; i < messages; i++)
            {
                var m = (Message<byte[]>)await consumer.ReceiveAsync(TimeSpan.FromMicroseconds(5000));
                if (m != null)
                {
                    var receivedMessage = Encoding.UTF8.GetString(m.Data);
                    _output.WriteLine($"{messageReceived} Received message: [{receivedMessage}]");
                    await consumer.AcknowledgeAsync(m);
                    messageReceived++;
                }
            }
            Assert.Equal(20, messageReceived);

            await producer.CloseAsync();
            await consumer.CloseAsync();
            //Assert.True(messageReceived > 5);
        }
        public async Task InitializeAsync()
        {
            /*_tcs = new TaskCompletionSource<PulsarClient>(TaskCreationOptions.RunContinuationsAsynchronously);
            //_client = fixture.System.NewClient(fixture.ConfigBuilder).AsTask().GetAwaiter().GetResult();
            new Action(async () =>
            {
                var client = await _system.NewClient(_configBuilder);
                _tcs.TrySetResult(client);
            })();
           _client = await _tcs.Task; */
            _client = await _system.NewClient(_configBuilder);
        }

        public async Task DisposeAsync()
        {
            await _client.ShutdownAsync();
        }
        private static object[][] AckReceiptEnabled()
        {
            return
            [
                [true],
                [false]
            ];
        }

        
        private object[][] batchedMessageAck()
        {
            // When batch index ack is disabled (by default), only after all single messages were sent would the pending
            // ACK be added into the ACK tracker.
            return
            [
                [3, 5, CommandAck.AckType.Individual],
                [5, 5, CommandAck.AckType.Individual],
                [3, 5, CommandAck.AckType.Cumulative],
                [5, 5, CommandAck.AckType.Cumulative]
            ];
        }

    }

}