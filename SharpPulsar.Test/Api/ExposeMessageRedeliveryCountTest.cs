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
    public class ExposeMessageRedeliveryCountTest : ProducerConsumerBase
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

		public virtual void testRedeliveryCount()
		{

			const string topic = "persistent://my-property/my-ns/redeliveryCount";

			Consumer<sbyte[]> consumer = pulsarClient.newConsumer(Schema_Fields.BYTES).topic(topic).subscriptionName("my-subscription").subscriptionType(SubscriptionType.Shared).ackTimeout(1, TimeUnit.SECONDS).receiverQueueSize(100).subscriptionInitialPosition(SubscriptionInitialPosition.Earliest).subscribe();

			Producer<sbyte[]> producer = pulsarClient.newProducer(Schema_Fields.BYTES).topic(topic).create();

			producer.send("Hello Pulsar".GetBytes());

			do
			{
				Message<sbyte[]> message = consumer.receive();
				message.Properties;
				int redeliveryCount = message.RedeliveryCount;
				if (redeliveryCount > 2)
				{
					consumer.acknowledge(message);
					Assert.assertEquals(3, redeliveryCount);
					break;
				}
			} while (true);

			producer.close();
			consumer.close();
		}

		public virtual void testRedeliveryCountWithPartitionedTopic()
		{

			const string topic = "persistent://my-property/my-ns/redeliveryCount.partitioned";

			admin.topics().createPartitionedTopic(topic, 3);

			Consumer<sbyte[]> consumer = pulsarClient.newConsumer(Schema_Fields.BYTES).topic(topic).subscriptionName("my-subscription").subscriptionType(SubscriptionType.Shared).ackTimeout(1, TimeUnit.SECONDS).receiverQueueSize(100).subscriptionInitialPosition(SubscriptionInitialPosition.Earliest).subscribe();

			Producer<sbyte[]> producer = pulsarClient.newProducer(Schema_Fields.BYTES).topic(topic).create();

			producer.send("Hello Pulsar".GetBytes());

			do
			{
				Message<sbyte[]> message = consumer.receive();
				message.Properties;
				int redeliveryCount = message.RedeliveryCount;
				if (redeliveryCount > 2)
				{
					consumer.acknowledge(message);
					Assert.assertEquals(3, redeliveryCount);
					break;
				}
			} while (true);

			producer.close();
			consumer.close();

			admin.topics().deletePartitionedTopic(topic);
		}

		public virtual void testRedeliveryCountWhenConsumerDisconnected()
		{

			string topic = "persistent://my-property/my-ns/testRedeliveryCountWhenConsumerDisconnected";

			Consumer<string> consumer0 = pulsarClient.newConsumer(Schema_Fields.STRING).topic(topic).subscriptionName("s1").subscriptionType(SubscriptionType.Shared).subscribe();

			Consumer<string> consumer1 = pulsarClient.newConsumer(Schema_Fields.STRING).topic(topic).subscriptionName("s1").subscriptionType(SubscriptionType.Shared).subscribe();

			Producer<string> producer = pulsarClient.newProducer(Schema_Fields.STRING).topic(topic).enableBatching(true).batchingMaxMessages(5).batchingMaxPublishDelay(1, TimeUnit.SECONDS).create();

			const int messages = 10;
			for (int i = 0; i < messages; i++)
			{
				producer.send("my-message-" + i);
			}

			IList<Message<string>> receivedMessagesForConsumer0 = new List<Message<string>>();
			IList<Message<string>> receivedMessagesForConsumer1 = new List<Message<string>>();

			for (int i = 0; i < messages; i++)
			{
				Message<string> msg = consumer0.receive(1, TimeUnit.SECONDS);
				if (msg != null)
				{
					receivedMessagesForConsumer0.Add(msg);
				}
				else
				{
					break;
				}
			}

			for (int i = 0; i < messages; i++)
			{
				Message<string> msg = consumer1.receive(1, TimeUnit.SECONDS);
				if (msg != null)
				{
					receivedMessagesForConsumer1.Add(msg);
				}
				else
				{
					break;
				}
			}

			Assert.assertEquals(receivedMessagesForConsumer0.Count + receivedMessagesForConsumer1.Count, messages);

			consumer0.close();

			for (int i = 0; i < receivedMessagesForConsumer0.Count; i++)
			{
				Assert.assertEquals(consumer1.receive().RedeliveryCount, 1);
			}

		}
	}

}