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

	public class TopicFromMessageTest
	{

		private const long TestTimeout = 90000; // 1.5 min
		private const int BatchingMaxMessagesThreshold = 2;
		public override void Setup()
		{
			base.InternalSetup();
			base.ProducerBaseSetup();
		}


		public override void Cleanup()
		{
			base.InternalCleanup();
		}

		public virtual void TestSingleTopicConsumerNoBatchShortName()
		{
			using(Consumer<sbyte[]> consumer = PulsarClient.NewConsumer().Topic("topic1").SubscriptionName("sub1").Subscribe(), Producer<sbyte[]> producer = PulsarClient.NewProducer().Topic("topic1").EnableBatching(false).Create())
			{
				producer.send("foobar".GetBytes());
				Assert.assertEquals(consumer.Receive().TopicName, "persistent://public/default/topic1");
			}
		}

		public virtual void TestSingleTopicConsumerNoBatchFullName()
		{
			using(Consumer<sbyte[]> consumer = PulsarClient.NewConsumer().Topic("my-property/my-ns/topic1").SubscriptionName("sub1").Subscribe(), Producer<sbyte[]> producer = PulsarClient.NewProducer().Topic("my-property/my-ns/topic1").EnableBatching(false).Create())
			{
				producer.send("foobar".GetBytes());
				Assert.assertEquals(consumer.Receive().TopicName, "persistent://my-property/my-ns/topic1");
			}
		}

		public virtual void TestMultiTopicConsumerNoBatchShortName()
		{
			using(Consumer<sbyte[]> consumer = PulsarClient.NewConsumer().Topics(Lists.newArrayList("topic1", "topic2")).SubscriptionName("sub1").Subscribe(), Producer<sbyte[]> producer1 = PulsarClient.NewProducer().Topic("topic1").EnableBatching(false).Create(), Producer<sbyte[]> producer2 = PulsarClient.NewProducer().Topic("topic2").EnableBatching(false).Create())
			{
				producer1.send("foobar".GetBytes());
				producer2.send("foobar".GetBytes());
				Assert.assertEquals(consumer.Receive().TopicName, "persistent://public/default/topic1");
				Assert.assertEquals(consumer.Receive().TopicName, "persistent://public/default/topic2");
			}
		}

		public virtual void TestSingleTopicConsumerBatchShortName()
		{
			using(Consumer<sbyte[]> consumer = PulsarClient.NewConsumer().Topic("topic1").SubscriptionName("sub1").Subscribe(), Producer<sbyte[]> producer = PulsarClient.NewProducer().Topic("topic1").EnableBatching(true).BatchingMaxMessages(BatchingMaxMessagesThreshold).Create())
			{
				producer.send("foobar".GetBytes());

				Assert.assertEquals(consumer.Receive().TopicName, "persistent://public/default/topic1");
			}
		}

		public virtual void TestMultiTopicConsumerBatchShortName()
		{
			using(Consumer<sbyte[]> consumer = PulsarClient.NewConsumer().Topics(Lists.newArrayList("topic1", "topic2")).SubscriptionName("sub1").Subscribe(), Producer<sbyte[]> producer1 = PulsarClient.NewProducer().Topic("topic1").EnableBatching(true).BatchingMaxMessages(BatchingMaxMessagesThreshold).Create(), Producer<sbyte[]> producer2 = PulsarClient.NewProducer().Topic("topic2").EnableBatching(true).BatchingMaxMessages(BatchingMaxMessagesThreshold).Create())
			{

				producer1.send("foobar".GetBytes());
				producer2.send("foobar".GetBytes());

				Assert.assertEquals(consumer.Receive().TopicName, "persistent://public/default/topic1");
				Assert.assertEquals(consumer.Receive().TopicName, "persistent://public/default/topic2");
			}
		}

	}

}