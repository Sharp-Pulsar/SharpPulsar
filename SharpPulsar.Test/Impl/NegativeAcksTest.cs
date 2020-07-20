using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using SharpPulsar.Akka.InternalCommands;
using SharpPulsar.Impl.Auth;
using SharpPulsar.Impl.Schema;
using SharpPulsar.Protocol.Proto;
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

	public class NegativeAcksTest : ProducerConsumerBase
	{
        private readonly ITestOutputHelper _output;
        private TestCommon.Common _common;

        public NegativeAcksTest(ITestOutputHelper output)
        {
            _output = output;
            _common = new TestCommon.Common(output);
            _common.GetPulsarSystem(new AuthenticationDisabled(), useProxy: true, operationTime: 60000, brokerService: "pulsar://52.184.218.188:6650");
        }
		[Fact]
        public void TestNegativeAcksBatch()
        {
			TestNegativeAcks(true, false, CommandSubscribe.SubType.Exclusive, 3000, 30000);
        }
		
		[Fact]
        public void TestNegativeAcksNoBatch()
        {
			TestNegativeAcks(false, false, CommandSubscribe.SubType.Exclusive, 3000, 30000);
        }

		private void TestNegativeAcks(bool batching, bool usePartition, CommandSubscribe.SubType subscriptionType, int negAcksDelayMillis, int ackTimeout)
		{
			_output.WriteLine($"Test negative acks batching={batching} partitions={usePartition} subType={subscriptionType} negAckDelayMs={negAcksDelayMillis}");
			string topic = "testNegativeAcks-" + DateTime.Now.Ticks;

            var consumer = _common.PulsarSystem.PulsarConsumer(_common.CreateConsumer(BytesSchema.Of(), topic, "TestNegativeAcks", "sub1", subType: subscriptionType, acknowledgmentGroupTime: 0, negativeAckRedeliveryDelay: negAcksDelayMillis, ackTimeout: ackTimeout, forceTopic: true));

            var producer = _common.PulsarSystem.PulsarProducer(_common.CreateProducer(BytesSchema.Of(), topic, DateTimeHelper.CurrentUnixTimeMillis().ToString(), batchMessageDelayMs: negAcksDelayMillis, batchingMaxMessages:10, enableBatching: batching));

			ISet<string> sentMessages = new HashSet<string>();

			const int n = 10;
			for (int i = 0; i < n; i++)
			{
				string value = "test-" + i;
				var send = new Send(value.GetBytes(), ImmutableDictionary<string, object>.Empty);
				var receipt = _common.PulsarSystem.Send(send, producer.Producer);
				sentMessages.Add(value);
			}

			for (int i = 0; i < n; i++)
			{
				var msg = _common.PulsarSystem.Receive("TestNegativeAcks");
				if(msg != null)
				    _common.PulsarSystem.NegativeAcknowledge(msg);
			}

			ISet<string> receivedMessages = new HashSet<string>();

			// All the messages should be received again
			for (int i = 0; i < n; i++)
			{
                var msg = _common.PulsarSystem.Receive("TestNegativeAcks");
                if (msg != null)
                {
                    receivedMessages.Add(((byte[]) msg.Message.Value).GetString());
                    _common.PulsarSystem.Acknowledge(msg);
                }
                else
                    i--;
            }

			Assert.Equal(sentMessages, receivedMessages);

			// There should be no more messages
			Assert.Null(_common.PulsarSystem.Receive("TestNegativeAcks", 100));
		}
	}

}