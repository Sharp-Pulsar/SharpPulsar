using System.Collections.Generic;
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

		public override void setup()
		{
			base.internalSetup();
			base.producerBaseSetup();
		}


		public override void cleanup()
		{
			base.internalCleanup();
		}


        public virtual void testNegativeAcks(bool batching, bool usePartitions, SubscriptionType subscriptionType, int negAcksDelayMillis, int ackTimeout)
		{
			log.info("Test negative acks batching={} partitions={} subType={} negAckDelayMs={}", batching, usePartitions, subscriptionType, negAcksDelayMillis);
			string topic = "testNegativeAcks-" + System.nanoTime();

            Consumer<string> consumer = pulsarClient.newConsumer(Schema_Fields.STRING).topic(topic).subscriptionName("sub1").acknowledgmentGroupTime(0, TimeUnit.SECONDS).subscriptionType(subscriptionType).negativeAckRedeliveryDelay(negAcksDelayMillis, TimeUnit.MILLISECONDS).ackTimeout(ackTimeout, TimeUnit.MILLISECONDS).subscribe();

            Producer<string> producer = pulsarClient.newProducer(Schema_Fields.STRING).topic(topic).enableBatching(batching).create();

			ISet<string> sentMessages = new HashSet<string>();

			const int N = 10;
			for (int i = 0; i < N; i++)
			{
				string value = "test-" + i;
				producer.sendAsync(value);
				sentMessages.Add(value);
			}
			producer.flush();

			for (int i = 0; i < N; i++)
			{
				Message<string> msg = consumer.receive();
				consumer.negativeAcknowledge(msg);
			}

			ISet<string> receivedMessages = new HashSet<string>();

			// All the messages should be received again
			for (int i = 0; i < N; i++)
			{
				Message<string> msg = consumer.receive();
				receivedMessages.Add(msg.Value);
				consumer.acknowledge(msg);
			}

			assertEquals(receivedMessages, sentMessages);

			// There should be no more messages
			assertNull(consumer.receive(100, TimeUnit.MILLISECONDS));
			consumer.close();
			producer.close();
		}
	}

}