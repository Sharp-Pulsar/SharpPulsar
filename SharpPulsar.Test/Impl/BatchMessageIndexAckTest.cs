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
	using ProducerConsumerBase = ProducerConsumerBase;


    public class BatchMessageIndexAckTest : ProducerConsumerBase
	{

		public override void setup()
		{
			conf.AcknowledgmentAtBatchIndexLevelEnabled = true;
			base.internalSetup();
			base.producerBaseSetup();
		}


		public override void cleanup()
		{
			base.internalCleanup();
		}


		public virtual void testBatchMessageIndexAckForSharedSubscription()
		{
			const string topic = "testBatchMessageIndexAckForSharedSubscription";

			Consumer<int> consumer = pulsarClient.newConsumer(Schema_Fields.INT32).topic(topic).subscriptionName("sub").receiverQueueSize(100).subscriptionType(SubscriptionType.Shared).negativeAckRedeliveryDelay(2, TimeUnit.SECONDS).subscribe();

            Producer<int> producer = pulsarClient.newProducer(Schema_Fields.INT32).topic(topic).batchingMaxPublishDelay(50, TimeUnit.MILLISECONDS).create();

			const int messages = 100;
			IList<CompletableFuture<MessageId>> futures = new List<CompletableFuture<MessageId>>(messages);
			for (int i = 0; i < messages; i++)
			{
				futures.Add(producer.sendAsync(i));
			}
			FutureUtil.waitForAll(futures).get();

			for (int i = 0; i < messages; i++)
			{
				if (i % 2 == 0)
				{
					consumer.acknowledge(consumer.receive());
				}
				else
				{
					consumer.negativeAcknowledge(consumer.receive());
				}
			}

			IList<Message<int>> received = new List<Message<int>>(50);
			for (int i = 0; i < 50; i++)
			{
				received.Add(consumer.receive());
			}

			Assert.assertEquals(received.Count, 50);

			Message<int> moreMessage = consumer.receive(1, TimeUnit.SECONDS);
			Assert.assertNull(moreMessage);

			futures.Clear();
			for (int i = 0; i < 50; i++)
			{
				futures.Add(producer.sendAsync(i));
			}
			FutureUtil.waitForAll(futures).get();

			for (int i = 0; i < 50; i++)
			{
				received.Add(consumer.receive());
			}

			// Ensure the flow permit is work well since the client skip the acked batch index,
			// broker also need to handle the available permits.
			Assert.assertEquals(received.Count, 100);
		}

        public virtual void testBatchMessageIndexAckForExclusiveSubscription()
		{
			const string topic = "testBatchMessageIndexAckForExclusiveSubscription";

            Consumer<int> consumer = pulsarClient.newConsumer(Schema_Fields.INT32).topic(topic).subscriptionName("sub").receiverQueueSize(100).subscribe();

            Producer<int> producer = pulsarClient.newProducer(Schema_Fields.INT32).topic(topic).batchingMaxPublishDelay(50, TimeUnit.MILLISECONDS).create();

			const int messages = 100;
			IList<CompletableFuture<MessageId>> futures = new List<CompletableFuture<MessageId>>(messages);
			for (int i = 0; i < messages; i++)
			{
				futures.Add(producer.sendAsync(i));
			}
			FutureUtil.waitForAll(futures).get();

			for (int i = 0; i < messages; i++)
			{
				if (i == 49)
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
			consumer = pulsarClient.newConsumer(Schema_Fields.INT32).topic(topic).subscriptionName("sub").receiverQueueSize(100).subscribe();

			IList<Message<int>> received = new List<Message<int>>(50);
			for (int i = 0; i < 50; i++)
			{
				received.Add(consumer.receive());
			}

			Assert.assertEquals(received.Count, 50);

			Message<int> moreMessage = consumer.receive(1, TimeUnit.SECONDS);
			Assert.assertNull(moreMessage);

			futures.Clear();
			for (int i = 0; i < 50; i++)
			{
				futures.Add(producer.sendAsync(i));
			}
			FutureUtil.waitForAll(futures).get();

			for (int i = 0; i < 50; i++)
			{
				received.Add(consumer.receive());
			}

			// Ensure the flow permit is work well since the client skip the acked batch index,
			// broker also need to handle the available permits.
			Assert.assertEquals(received.Count, 100);
		}
	}

}