using System.Collections.Generic;
using System.Threading;

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
namespace SharpPulsar.Test.Batch
{
	public class BatchMessageIndexAckTest
	{

		protected internal override void Setup()
		{
			Conf.AcknowledgmentAtBatchIndexLevelEnabled = true;
			base.InternalSetup();
			base.ProducerBaseSetup();
		}


		protected internal override void Cleanup()
		{
			base.InternalCleanup();
		}

		public virtual void TestBatchMessageIndexAckForSharedSubscription()
		{
			const string topic = "testBatchMessageIndexAckForSharedSubscription";
			const string subscriptionName = "sub";
			
			Consumer<int> consumer = PulsarClient.NewConsumer(Schema.INT32).Topic(topic).SubscriptionName(subscriptionName).ReceiverQueueSize(100).SubscriptionType(SubscriptionType.Shared).EnableBatchIndexAcknowledgment(true).NegativeAckRedeliveryDelay(2, TimeUnit.SECONDS).Subscribe();

			Producer<int> producer = PulsarClient.NewProducer(Schema.INT32).Topic(topic).BatchingMaxPublishDelay(50, TimeUnit.MILLISECONDS).Create();

			const int messages = 100;
			IList<CompletableFuture<MessageId>> futures = new List<CompletableFuture<MessageId>>(messages);
			for(int i = 0; i < messages; i++)
			{
				futures.Add(producer.sendAsync(i));
			}
			FutureUtil.WaitForAll(futures).get();

			IList<MessageId> acked = new List<MessageId>(50);
			for(int i = 0; i < messages; i++)
			{
				Message<int> msg = consumer.receive();
				if(i % 2 == 0)
				{
					consumer.acknowledge(msg);
					acked.Add(msg.MessageId);
				}
				else
				{
					consumer.negativeAcknowledge(consumer.receive());
				}
			}

			IList<MessageId> received = new List<MessageId>(50);
			for(int i = 0; i < 50; i++)
			{
				received.Add(consumer.receive().MessageId);
			}

			Assert.assertEquals(received.Count, 50);
			acked.RetainAll(received);
			Assert.assertEquals(acked.Count, 0);

			foreach(MessageId messageId in received)
			{
				consumer.acknowledge(messageId);
			}

			Thread.Sleep(1000);

			consumer.redeliverUnacknowledgedMessages();

			Message<int> moreMessage = consumer.receive(2, TimeUnit.SECONDS);
			Assert.assertNull(moreMessage);

			// check the mark delete position was changed
			BatchMessageIdImpl ackedMessageId = (BatchMessageIdImpl) received[0];
			PersistentTopicInternalStats stats = Admin.Topics().GetInternalStats(topic, false);
			string markDeletePosition = stats.Cursors.GetValueOrNull(subscriptionName).markDeletePosition;
			Assert.assertEquals(ackedMessageId.LedgerIdConflict + ":" + ackedMessageId.EntryIdConflict, markDeletePosition);

			futures.Clear();
			for(int i = 0; i < 50; i++)
			{
				futures.Add(producer.sendAsync(i));
			}
			FutureUtil.WaitForAll(futures).get();

			for(int i = 0; i < 50; i++)
			{
				received.Add(consumer.receive().MessageId);
			}

			// Ensure the flow permit is work well since the client skip the acked batch index,
			// broker also need to handle the available permits.
			Assert.assertEquals(received.Count, 100);
		}

		public virtual void TestBatchMessageIndexAckForExclusiveSubscription()
		{
			const string topic = "testBatchMessageIndexAckForExclusiveSubscription";

			Consumer<int> consumer = PulsarClient.NewConsumer(Schema.INT32).Topic(topic).SubscriptionName("sub").ReceiverQueueSize(100).EnableBatchIndexAcknowledgment(true).Subscribe();

			Producer<int> producer = PulsarClient.NewProducer(Schema.INT32).Topic(topic).BatchingMaxPublishDelay(50, TimeUnit.MILLISECONDS).Create();

			const int messages = 100;
			IList<CompletableFuture<MessageId>> futures = new List<CompletableFuture<MessageId>>(messages);
			for(int i = 0; i < messages; i++)
			{
				futures.Add(producer.sendAsync(i));
			}
			FutureUtil.WaitForAll(futures).get();

			for(int i = 0; i < messages; i++)
			{
				if(i == 49)
				{
					consumer.acknowledgeCumulative(consumer.receive());
				}
				else
				{
					consumer.receive();
				}
			}

			//Wait ack send.
			Thread.Sleep(1000);
			consumer.close();
			consumer = PulsarClient.NewConsumer(Schema.INT32).Topic(topic).SubscriptionName("sub").ReceiverQueueSize(100).Subscribe();

			IList<Message<int>> received = new List<Message<int>>(50);
			for(int i = 0; i < 50; i++)
			{
				received.Add(consumer.receive());
			}

			Assert.assertEquals(received.Count, 50);

			Message<int> moreMessage = consumer.receive(1, TimeUnit.SECONDS);
			Assert.assertNull(moreMessage);

			futures.Clear();
			for(int i = 0; i < 50; i++)
			{
				futures.Add(producer.sendAsync(i));
			}
			FutureUtil.WaitForAll(futures).get();

			for(int i = 0; i < 50; i++)
			{
				received.Add(consumer.receive());
			}

			// Ensure the flow permit is work well since the client skip the acked batch index,
			// broker also need to handle the available permits.
			Assert.assertEquals(received.Count, 100);
		}

		public virtual void TestDoNotRecycleAckSetMultipleTimes()
		{
			const string topic = "persistent://my-property/my-ns/testSafeAckSetRecycle";

			Producer<sbyte[]> producer = PulsarClient.NewProducer().BatchingMaxMessages(10).BlockIfQueueFull(true).Topic(topic).Create();

			Consumer<sbyte[]> consumer = PulsarClient.NewConsumer().AcknowledgmentGroupTime(1, TimeUnit.MILLISECONDS).Topic(topic).EnableBatchIndexAcknowledgment(true).SubscriptionName("test").Subscribe();

			const int messages = 100;
			for(int i = 0; i < messages; i++)
			{
				producer.sendAsync("Hello Pulsar".GetBytes());
			}

			// Should not throw an exception.
			for(int i = 0; i < messages; i++)
			{
				consumer.AcknowledgeCumulative(consumer.Receive());
				// make sure the group ack flushed.
				Thread.Sleep(2);
			}

			producer.close();
			consumer.close();
		}
	}

}