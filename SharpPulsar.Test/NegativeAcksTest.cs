using System;
using System.Collections.Generic;
using SharpPulsar.Extension;
using System.Text;
using BAMCIS.Util.Concurrent;
using SharpPulsar.Configuration;
using SharpPulsar.Protocol.Proto;
using SharpPulsar.Test.Fixtures;
using SharpPulsar.User;
using Xunit;
using Xunit.Abstractions;
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
	[Collection(nameof(PulsarTests))]
	public class NegativeAcksTest
	{
        private readonly ITestOutputHelper _output;
		private readonly PulsarClient _client;

		public NegativeAcksTest(ITestOutputHelper output, PulsarStandaloneClusterFixture fixture)
		{
			_output = output;
			_client = fixture.Client;
		}

		[Fact]
        public void TestNegativeAcksBatch()
        {
			TestNegativeAcks(true, false, CommandSubscribe.SubType.Shared, 35000, 3000);
			//TestNegativeAcks(true, false, CommandSubscribe.SubType.Exclusive, 35000, 30000);
        }
		
		[Fact]
        public void TestNegativeAcksNoBatch()
        {
			//TestNegativeAcks(false, false, CommandSubscribe.SubType.Exclusive, 35000, 30000);
			TestNegativeAcks(false, false, CommandSubscribe.SubType.Shared, 35000, 3000);
        }

		private void TestNegativeAcks(bool batching, bool usePartition, CommandSubscribe.SubType subscriptionType, int negAcksDelayMillis, int ackTimeout)
		{
			_output.WriteLine($"Test negative acks batching={batching} partitions={usePartition} subType={subscriptionType} negAckDelayMs={negAcksDelayMillis}");
			string topic = "testNegativeAcks-" + DateTime.Now.Ticks;

			var builder = new ConsumerConfigBuilder<sbyte[]>();
			builder.Topic(topic);
			builder.SubscriptionName($"sub1-{Guid.NewGuid()}");
			builder.AckTimeout(ackTimeout, TimeUnit.MILLISECONDS);
			builder.ForceTopicCreation(true);
			builder.AcknowledgmentGroupTime(0);
			builder.NegativeAckRedeliveryDelay(negAcksDelayMillis, TimeUnit.MILLISECONDS);
			builder.SubscriptionType(subscriptionType);
			var consumer = _client.NewConsumer(builder);

			var pBuilder = new ProducerConfigBuilder<sbyte[]>();
			pBuilder.Topic(topic);
			pBuilder.EnableBatching(batching);
			pBuilder.BatchingMaxPublishDelay(negAcksDelayMillis);
			pBuilder.BatchingMaxMessages(10);
			var producer = _client.NewProducer(pBuilder);

			ISet<string> sentMessages = new HashSet<string>();

			const int n = 10;
			for (int i = 0; i < n; i++)
			{
				string value = "test-" + i;
				producer.Send(Encoding.UTF8.GetBytes(value).ToSBytes());
				sentMessages.Add(value);
			}

			for (int i = 0; i < n; i++)
			{
				var msg = consumer.Receive();
				if(msg != null)
				    consumer.NegativeAcknowledge(msg);
			}

			ISet<string> receivedMessages = new HashSet<string>();

			// All the messages should be received again
			for (int i = 0; i < n; i++)
			{
                var msg = consumer.Receive();
                if (msg != null)
                {
                    receivedMessages.Add(Encoding.UTF8.GetString(msg.Data.ToBytes()));
                    consumer.Acknowledge(msg);
                }
                else
                    i--;
            }

			Assert.Equal(sentMessages, receivedMessages);
			var nu = consumer.Receive(100);
			// There should be no more messages
			Assert.Null(nu);
		}
	}

}