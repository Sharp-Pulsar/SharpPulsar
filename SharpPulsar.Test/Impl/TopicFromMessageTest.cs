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

using ProducerConsumerBase = SharpPulsar.Test.Api.ProducerConsumerBase;

namespace SharpPulsar.Test.Impl
{
	using ProducerConsumerBase = ProducerConsumerBase;

    public class TopicFromMessageTest : ProducerConsumerBase
	{

		private const long TEST_TIMEOUT = 90000; // 1.5 min
		private const int BATCHING_MAX_MESSAGES_THRESHOLD = 2;

		public override void setup()
		{
			base.internalSetup();
			base.producerBaseSetup();
		}


		public override void cleanup()
		{
			base.internalCleanup();
		}


		public virtual void testSingleTopicConsumerNoBatchShortName()
		{
			using (Consumer<sbyte[]> consumer = pulsarClient.newConsumer().topic("topic1").subscriptionName("sub1").subscribe(), Producer<sbyte[]> producer = pulsarClient.newProducer().topic("topic1").enableBatching(false).create())
			{
				producer.send("foobar".GetBytes());
				Assert.assertEquals(consumer.receive().TopicName, "persistent://public/default/topic1");
			}
		}


		public virtual void testSingleTopicConsumerNoBatchFullName()
		{
			using (Consumer<sbyte[]> consumer = pulsarClient.newConsumer().topic("my-property/my-ns/topic1").subscriptionName("sub1").subscribe(), Producer<sbyte[]> producer = pulsarClient.newProducer().topic("my-property/my-ns/topic1").enableBatching(false).create())
			{
				producer.send("foobar".GetBytes());
				Assert.assertEquals(consumer.receive().TopicName, "persistent://my-property/my-ns/topic1");
			}
		}


		public virtual void testMultiTopicConsumerNoBatchShortName()
		{
			using (Consumer<sbyte[]> consumer = pulsarClient.newConsumer().topics(Lists.newArrayList("topic1", "topic2")).subscriptionName("sub1").subscribe(), Producer<sbyte[]> producer1 = pulsarClient.newProducer().topic("topic1").enableBatching(false).create(), Producer<sbyte[]> producer2 = pulsarClient.newProducer().topic("topic2").enableBatching(false).create())
			{
				producer1.send("foobar".GetBytes());
				producer2.send("foobar".GetBytes());
				Assert.assertEquals(consumer.receive().TopicName, "persistent://public/default/topic1");
				Assert.assertEquals(consumer.receive().TopicName, "persistent://public/default/topic2");
			}
		}


		public virtual void testSingleTopicConsumerBatchShortName()
		{
			using (Consumer<sbyte[]> consumer = pulsarClient.newConsumer().topic("topic1").subscriptionName("sub1").subscribe(), Producer<sbyte[]> producer = pulsarClient.newProducer().topic("topic1").enableBatching(true).batchingMaxMessages(BATCHING_MAX_MESSAGES_THRESHOLD).create())
			{
				producer.send("foobar".GetBytes());

				Assert.assertEquals(consumer.receive().TopicName, "persistent://public/default/topic1");
			}
		}


		public virtual void testMultiTopicConsumerBatchShortName()
		{
			using (Consumer<sbyte[]> consumer = pulsarClient.newConsumer().topics(Lists.newArrayList("topic1", "topic2")).subscriptionName("sub1").subscribe(), Producer<sbyte[]> producer1 = pulsarClient.newProducer().topic("topic1").enableBatching(true).batchingMaxMessages(BATCHING_MAX_MESSAGES_THRESHOLD).create(), Producer<sbyte[]> producer2 = pulsarClient.newProducer().topic("topic2").enableBatching(true).batchingMaxMessages(BATCHING_MAX_MESSAGES_THRESHOLD).create())
			{

				producer1.send("foobar".GetBytes());
				producer2.send("foobar".GetBytes());

				Assert.assertEquals(consumer.receive().TopicName, "persistent://public/default/topic1");
				Assert.assertEquals(consumer.receive().TopicName, "persistent://public/default/topic2");
			}
		}

	}

}