using System;
using System.Collections.Generic;
using System.Threading;
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

	public class MessageRedeliveryTest : ProducerConsumerBase
	{
		private static readonly Logger log = LoggerFactory.getLogger(typeof(MessageRedeliveryTest));

		public override void setup()
		{
			base.internalSetup();
			base.ProducerBaseSetup();
		}


		public override void cleanup()
		{
			base.internalCleanup();
		}


		public static object[][] useOpenRangeSet()
		{
			return new object[][]
			{
				new object[] {true},
				new object[] {false}
			};
		}

		/// <summary>
		/// It tests that ManagedCursor tracks individually deleted messages and markDeletePosition correctly with different
		/// range-set implementation and re-delivers messages as expected.
		/// </summary>
		/// <param name="useOpenRangeSet"> </param>
		/// <exception cref="Exception"> </exception>
		/// 
		public virtual void testRedelivery(bool useOpenRangeSet)
		{

			this.conf.ManagedLedgerMaxEntriesPerLedger = 5;
			this.conf.ManagedLedgerMinLedgerRolloverTimeMinutes = 0;
			this.conf.ManagedLedgerUnackedRangesOpenCacheSetEnabled = useOpenRangeSet;

			try
			{
				const string ns1 = "my-property/brok-ns1";
				const string subName = "my-subscriber-name";
				const int numMessages = 50;
				admin.namespaces().createNamespace(ns1, Sets.newHashSet("test"));


				string topic1 = "persistent://" + ns1 + "/my-topic";

				ConsumerImpl<sbyte[]> consumer1 = (ConsumerImpl<sbyte[]>) pulsarClient.newConsumer().topic(topic1).subscriptionName(subName).subscriptionType(SubscriptionType.Shared).receiverQueueSize(10).acknowledgmentGroupTime(0, TimeUnit.MILLISECONDS).subscribe();
				ConsumerImpl<sbyte[]> consumer2 = (ConsumerImpl<sbyte[]>) pulsarClient.newConsumer().topic(topic1).subscriptionName(subName).subscriptionType(SubscriptionType.Shared).receiverQueueSize(10).acknowledgmentGroupTime(0, TimeUnit.MILLISECONDS).subscribe();
				ProducerImpl<sbyte[]> producer = (ProducerImpl<sbyte[]>) pulsarClient.newProducer().topic(topic1).create();

				for (int i = 0; i < numMessages; i++)
				{
					string message = "my-message-" + i;
					producer.send(message.GetBytes());
				}

				System.Threading.CountdownEvent latch = new System.Threading.CountdownEvent(numMessages);
				AtomicBoolean consume1 = new AtomicBoolean(true);
				AtomicBoolean consume2 = new AtomicBoolean(true);
				ISet<string> ackedMessages = Sets.newConcurrentHashSet();
				AtomicInteger counter = new AtomicInteger(0);

				// (1) ack alternate message from consumer-1 which creates ack-hole.
				executor.submit(() =>
				{
				while (true)
				{
					try
					{
						Message<sbyte[]> msg = consumer1.receive(1000, TimeUnit.MILLISECONDS);
						if (msg != null)
						{
							if (counter.AndIncrement % 2 == 0)
							{
								try
								{
									consumer1.acknowledge(msg);
									ackedMessages.Add(new string(msg.Data));
								}
								catch (PulsarClientException e1)
								{
									log.warn("Failed to ack message {}", e1.Message);
								}
							}
						}
						else
						{
							break;
						}
					}
					catch (PulsarClientException)
					{
						break;
					}
					latch.Signal();
				}
				});

				// (2) ack all the consumed messages from consumer-2
				executor.submit(() =>
				{
				while (consume2.get())
				{
					try
					{
						Message<sbyte[]> msg = consumer2.receive(1000, TimeUnit.MILLISECONDS);
						if (msg != null)
						{
							consumer2.acknowledge(msg);
							ackedMessages.Add(new string(msg.Data));
						}
						else
						{
							break;
						}
					}
					catch (PulsarClientException)
					{
						break;
					}
					latch.Signal();
				}
				});

				latch.await(10000, TimeUnit.MILLISECONDS);
				consume1.set(false);

				// (3) sleep so, consumer2 should timeout on it's pending read operation and not consume more messages
				Thread.Sleep(1000);

				// (4) here we consume all messages but consumer1 only acked alternate messages.
				assertNotEquals(ackedMessages.Count, numMessages);

				PersistentTopic pTopic = (PersistentTopic) this.pulsar.BrokerService.getTopicIfExists(topic1).get().get();
				ManagedLedgerImpl ml = (ManagedLedgerImpl) pTopic.ManagedLedger;

				ManagedCursorImpl cursor = (ManagedCursorImpl) ml.Cursors.GetEnumerator().next();

				// (5) now, close consumer1 and let broker deliver consumer1's unack messages to consumer2
				consumer1.close();

				// (6) broker should redeliver all unack messages of consumer-1 and consumer-2 should ack all of them
				System.Threading.CountdownEvent latch2 = new System.Threading.CountdownEvent(1);
				executor.submit(() =>
				{
				while (true)
				{
					try
					{
						Message<sbyte[]> msg = consumer2.receive(1000, TimeUnit.MILLISECONDS);
						if (msg != null)
						{
							consumer2.acknowledge(msg);
							ackedMessages.Add(new string(msg.Data));
						}
						else
						{
							break;
						}
					}
					catch (PulsarClientException)
					{
						break;
					}
					if (ackedMessages.Count == numMessages)
					{
						latch2.Signal();
					}
				}
				});

				latch2.await(20000, TimeUnit.MILLISECONDS);

				consumer2.close();

				assertEquals(ackedMessages.Count, numMessages);

				// (7) acked message set should be empty
				assertEquals(cursor.IndividuallyDeletedMessagesSet.size(), 0);

				// markDelete position should be one position behind read position
				assertEquals(cursor.ReadPosition, cursor.MarkDeletedPosition.Next);

				producer.close();
				consumer2.close();
			}
			finally
			{
				executor.shutdown();
			}

		}


		public virtual void testDoNotRedeliveryMarkDeleteMessages()
		{
			const string topic = "testDoNotRedeliveryMarkDeleteMessages";
			const string subName = "my-sub";

			Consumer<sbyte[]> consumer = pulsarClient.newConsumer().topic(topic).subscriptionName(subName).subscriptionType(SubscriptionType.Key_Shared).ackTimeout(1, TimeUnit.SECONDS).subscribe();

			Producer<sbyte[]> producer = pulsarClient.newProducer().topic(topic).enableBatching(false).create();

			producer.send("Pulsar".GetBytes());

			for (int i = 0; i < 2; i++)
			{
				Message message = consumer.receive();
				assertNotNull(message);
			}

			admin.topics().skipAllMessages(topic, subName);

			Message message = null;

			try
			{
				message = consumer.receive(2, TimeUnit.SECONDS);
			}
			catch (Exception)
			{
			}

			assertNull(message);
		}

	}

}