using System.Collections.Generic;

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
	public class ConsumerRedeliveryTest : ProducerConsumerBase
	{
		public override void setup()
		{
			conf.ManagedLedgerCacheEvictionFrequency = 0.1;
			base.internalSetup();
			base.producerBaseSetup();
		}

		public override void cleanup()
		{
			base.internalCleanup();
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
		public virtual void testOrderedRedelivery()
		{
			string topic = "persistent://my-property/my-ns/redelivery-" + DateTimeHelper.CurrentUnixTimeMillis();

			conf.ManagedLedgerMaxEntriesPerLedger = 2;
			conf.ManagedLedgerMinLedgerRolloverTimeMinutes = 0;
            
            Producer<sbyte[]> producer = pulsarClient.newProducer().topic(topic).producerName("my-producer-name").create();
			ConsumerBuilder<sbyte[]> consumerBuilder = pulsarClient.newConsumer().topic(topic).subscriptionName("s1").subscriptionType(SubscriptionType.Shared);
			ConsumerImpl<sbyte[]> consumer1 = (ConsumerImpl<sbyte[]>) consumerBuilder.subscribe();

			const int totalMsgs = 100;

			for (int i = 0; i < totalMsgs; i++)
			{
				string message = "my-message-" + i;
				producer.send(message.GetBytes());
			}


			int consumedCount = 0;
			ISet<MessageId> messageIds = Sets.newHashSet();
			for (int i = 0; i < totalMsgs; i++)
			{
				Message<sbyte[]> message = consumer1.receive(5, TimeUnit.SECONDS);
				if (message != null && (consumedCount % 2) == 0)
				{
					consumer1.acknowledge(message);
				}
				else
				{
					messageIds.Add(message.MessageId);
				}
				consumedCount += 1;
			}
			assertEquals(totalMsgs, consumedCount);

			// redeliver all unack messages
			consumer1.redeliverUnacknowledgedMessages(messageIds);

			MessageIdImpl lastMsgId = null;
			for (int i = 0; i < totalMsgs / 2; i++)
			{
				Message<sbyte[]> message = consumer1.receive(5, TimeUnit.SECONDS);
				MessageIdImpl msgId = (MessageIdImpl) message.MessageId;
				if (lastMsgId != null)
				{
					assertTrue(lastMsgId.LedgerId <= msgId.LedgerId, "lastMsgId: " + lastMsgId + " -- msgId: " + msgId);
				}
				lastMsgId = msgId;
			}

			// close consumer so, this consumer's unack messages will be redelivered to new consumer
			consumer1.close();

			Consumer<sbyte[]> consumer2 = consumerBuilder.subscribe();
			lastMsgId = null;
			for (int i = 0; i < totalMsgs / 2; i++)
			{
				Message<sbyte[]> message = consumer2.receive(5, TimeUnit.SECONDS);
				MessageIdImpl msgId = (MessageIdImpl) message.MessageId;
				if (lastMsgId != null)
				{
					assertTrue(lastMsgId.LedgerId <= msgId.LedgerId);
				}
				lastMsgId = msgId;
			}
		}

		public virtual void testUnAckMessageRedeliveryWithReceiveAsync()
		{
			string topic = "persistent://my-property/my-ns/async-unack-redelivery";
			Consumer<string> consumer = pulsarClient.newConsumer(Schema_Fields.STRING).topic(topic).subscriptionName("s1").ackTimeout(3, TimeUnit.SECONDS).subscribe();

			Producer<string> producer = pulsarClient.newProducer(Schema_Fields.STRING).topic(topic).enableBatching(true).batchingMaxMessages(5).batchingMaxPublishDelay(1, TimeUnit.SECONDS).create();

			const int messages = 10;
			IList<CompletableFuture<Message<string>>> futures = new List<CompletableFuture<Message<string>>>(10);
			for (int i = 0; i < messages; i++)
			{
				futures.Add(consumer.receiveAsync());
			}

			for (int i = 0; i < messages; i++)
			{
				producer.sendAsync("my-message-" + i);
			}

			int messageReceived = 0;
			foreach (CompletableFuture<Message<string>> future in futures)
			{
				Message<string> message = future.get();
				assertNotNull(message);
				messageReceived++;
				// Don't ack message, wait for ack timeout.
			}

			assertEquals(10, messageReceived);

			for (int i = 0; i < messages; i++)
			{
				Message<string> message = consumer.receive();
				assertNotNull(message);
				messageReceived++;
				consumer.acknowledge(message);
			}

			assertEquals(20, messageReceived);

			producer.close();
			consumer.close();
		}

	}

}