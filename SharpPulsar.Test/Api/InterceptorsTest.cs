using System;
using System.Collections.Generic;
using SharpPulsar.Impl;

namespace SharpPulsar.Test.Api
{
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
    public class InterceptorsTest : ProducerConsumerBase
    {

        private static readonly Logger log = LoggerFactory.getLogger(typeof(InterceptorsTest));

        public override void setup()
        {
            base.internalSetup();
            base.ProducerBaseSetup();
        }

        public override void cleanup()
        {
            base.internalCleanup();
        }

        public virtual object[][] ReceiverQueueSize
        {
            get
            {
                return new object[][]
                {
                    new object[] {0},
                    new object[] {1000}
                };
            }
        }

        public virtual void testProducerInterceptor()
        {
            IDictionary<MessageId, IList<string>> ackCallback = new Dictionary<MessageId, IList<string>>();

        abstract class BaseInterceptor implements ProducerInterceptor
        {
            private static final string set = "set";
        private string tag;
        private BaseInterceptor(string tag)
        {
            this.tag = tag;
        }

        public void close()
        {
        }
        public Message beforeSend(Producer producer, Message message)
        {
            MessageImpl msg = (MessageImpl) message;
            msg.MessageBuilder.addProperties(PulsarApi.KeyValue.newBuilder().setKey(tag).setValue(set));
            return message;
        }

        public void onSendAcknowledgement(Producer producer, Message message, MessageId msgId, Exception exception)
        {
            if (!set.Equals(message.Properties.get(tag)))
            {
                return;
            }
            ackCallback.computeIfAbsent(msgId, k => new List<>()).add(tag);
        }
    }

    BaseInterceptor interceptor1 = new BaseInterceptorAnonymousInnerClass(this);
    BaseInterceptor interceptor2 = new BaseInterceptorAnonymousInnerClass2(this);
    BaseInterceptor interceptor3 = new BaseInterceptorAnonymousInnerClass3(this);

    Producer<string> producer = pulsarClient.newProducer(Schema_Fields.STRING).topic("persistent://my-property/my-ns/my-topic").intercept(interceptor1, interceptor2, interceptor3).create();
    MessageId messageId = producer.newMessage().property("STR", "Y").value("Hello Pulsar!").send();
    Assert.assertEquals(ackCallback[messageId], Arrays.asList(interceptor1.tag, interceptor2.tag));
    log.info("Send result messageId: {}", messageId);
    MessageId messageId2 = producer.newMessage(Schema_Fields.INT32).property("INT", "Y").value(18).send();
    Assert.assertEquals(ackCallback[messageId2], Arrays.asList(interceptor1.tag, interceptor3.tag));
    log.info("Send result messageId: {}", messageId2);
    producer.close();

    public class BaseInterceptorAnonymousInnerClass : BaseInterceptor
    {
        private readonly InterceptorsTest outerInstance;

        public BaseInterceptorAnonymousInnerClass(InterceptorsTest outerInstance) : base("int1")
        {
            this.outerInstance = outerInstance;
        }

        public override bool eligible(Message message)
        {
            return true;
        }
    }

    public class BaseInterceptorAnonymousInnerClass2 : BaseInterceptor
    {
        private readonly InterceptorsTest outerInstance;

        public BaseInterceptorAnonymousInnerClass2(InterceptorsTest outerInstance) : base("int2")
        {
            this.outerInstance = outerInstance;
        }

        public override bool eligible(Message message)
        {
            return SchemaType.STRING.Equals(((MessageImpl)message).Schema.SchemaInfo.Type);
        }
    }

    public class BaseInterceptorAnonymousInnerClass3 : BaseInterceptor
    {
        private readonly InterceptorsTest outerInstance;

        public BaseInterceptorAnonymousInnerClass3(InterceptorsTest outerInstance) : base("int3")
        {
            this.outerInstance = outerInstance;
        }

        public override bool eligible(Message message)
        {
            return SchemaType.INT32.Equals(((MessageImpl)message).Schema.SchemaInfo.Type);
        }
    }

    public virtual void testProducerInterceptorsWithExceptions()
    {
    ProducerInterceptor<string> interceptor = new ProducerInterceptorAnonymousInnerClass(this);
    Producer<string> producer = pulsarClient.newProducer(Schema_Fields.STRING).topic("persistent://my-property/my-ns/my-topic").intercept(interceptor).create();

    MessageId messageId = producer.newMessage().value("Hello Pulsar!").send();
    Assert.assertNotNull(messageId);
    producer.close();
    }

    public class ProducerInterceptorAnonymousInnerClass : ProducerInterceptor<string>
    {
        private readonly InterceptorsTest outerInstance;

        public ProducerInterceptorAnonymousInnerClass(InterceptorsTest outerInstance)
        {
            this.outerInstance = outerInstance;
        }

        public void close()
        {

        }

        public Message<string> beforeSend(Producer<string> producer, Message<string> message)
        {
            throw new System.NullReferenceException();
        }

        public void onSendAcknowledgement(Producer<string> producer, Message<string> message, MessageId msgId, Exception exception)
        {
            throw new System.NullReferenceException();
        }
    }

    public virtual void testProducerInterceptorsWithErrors()
    {
    ProducerInterceptor<string> interceptor = new ProducerInterceptorAnonymousInnerClass2(this);
    Producer<string> producer = pulsarClient.newProducer(Schema_Fields.STRING).topic("persistent://my-property/my-ns/my-topic").intercept(interceptor).create();

    MessageId messageId = producer.newMessage().value("Hello Pulsar!").send();
    Assert.assertNotNull(messageId);
    producer.close();
    }

    public class ProducerInterceptorAnonymousInnerClass2 : ProducerInterceptor<string>
    {
        private readonly InterceptorsTest outerInstance;

        public ProducerInterceptorAnonymousInnerClass2(InterceptorsTest outerInstance)
        {
            this.outerInstance = outerInstance;
        }

        public void close()
        {

        }

        public Message<string> beforeSend(Producer<string> producer, Message<string> message)
        {
            throw new AbstractMethodError();
        }

        public void onSendAcknowledgement(Producer<string> producer, Message<string> message, MessageId msgId, Exception exception)
        {
            throw new AbstractMethodError();
        }
    }


    public virtual void testConsumerInterceptorWithErrors()
    {
    ConsumerInterceptor<string> interceptor = new ConsumerInterceptorAnonymousInnerClass(this);
    Consumer<string> consumer1 = pulsarClient.newConsumer(Schema_Fields.STRING).topic("persistent://my-property/my-ns/my-topic-exception").subscriptionType(SubscriptionType.Shared).intercept(interceptor).subscriptionName("my-subscription-ack-timeout").ackTimeout(3, TimeUnit.SECONDS).subscribe();

    Consumer<string> consumer2 = pulsarClient.newConsumer(Schema_Fields.STRING).topic("persistent://my-property/my-ns/my-topic-exception").subscriptionType(SubscriptionType.Shared).intercept(interceptor).subscriptionName("my-subscription-negative").subscribe();

    Producer<string> producer = pulsarClient.newProducer(Schema_Fields.STRING).topic("persistent://my-property/my-ns/my-topic-exception").create();

    producer.newMessage().value("Hello Pulsar!").send();

    Message<string> received = consumer1.receive();
    Assert.assertEquals(received.Value, "Hello Pulsar!");
    // wait ack timeout
    Message<string> receivedAgain = consumer1.receive();
    Assert.assertEquals(receivedAgain.Value, "Hello Pulsar!");
    consumer1.acknowledge(receivedAgain);

    received = consumer2.receive();
    Assert.assertEquals(received.Value, "Hello Pulsar!");
    consumer2.negativeAcknowledge(received);
    receivedAgain = consumer2.receive();
    Assert.assertEquals(receivedAgain.Value, "Hello Pulsar!");
    consumer2.acknowledge(receivedAgain);

    producer.close();
    consumer1.close();
    consumer2.close();
    }

    public class ConsumerInterceptorAnonymousInnerClass : ConsumerInterceptor<string>
    {
        private readonly InterceptorsTest outerInstance;

        public ConsumerInterceptorAnonymousInnerClass(InterceptorsTest outerInstance)
        {
            this.outerInstance = outerInstance;
        }

        public void close()
        {
            throw new AbstractMethodError();
        }

        public Message<string> beforeConsume(Consumer<string> consumer, Message<string> message)
        {
            throw new AbstractMethodError();
        }

        public void onAcknowledge(Consumer<string> consumer, MessageId messageId, Exception exception)
        {
            throw new AbstractMethodError();
        }

        public void onAcknowledgeCumulative(Consumer<string> consumer, MessageId messageId, Exception exception)
        {
            throw new AbstractMethodError();
        }

        public void onNegativeAcksSend(Consumer<string> consumer, ISet<MessageId> messageIds)
        {
            throw new AbstractMethodError();
        }

        public void onAckTimeoutSend(Consumer<string> consumer, ISet<MessageId> messageIds)
        {
            throw new AbstractMethodError();
        }
    }

    public virtual void testConsumerInterceptorWithSingleTopicSubscribe(int? receiverQueueSize)
    {
    ConsumerInterceptor<string> interceptor = new ConsumerInterceptorAnonymousInnerClass2(this);

    Consumer<string> consumer = pulsarClient.newConsumer(Schema_Fields.STRING).topic("persistent://my-property/my-ns/my-topic").subscriptionType(SubscriptionType.Shared).intercept(interceptor).subscriptionName("my-subscription").receiverQueueSize(receiverQueueSize.Value).subscribe();

    Producer<string> producer = pulsarClient.newProducer(Schema_Fields.STRING).topic("persistent://my-property/my-ns/my-topic").enableBatching(false).create();

    // Receive a message synchronously
    producer.newMessage().value("Hello Pulsar!").send();
    Message<string> received = consumer.receive();
    MessageImpl<string> msg = (MessageImpl<string>) received;
    bool haveKey = false;
        foreach (PulsarApi.KeyValue keyValue in msg.MessageBuilder.PropertiesList)
    {
        if ("beforeConsumer".Equals(keyValue.Key))
        {
            haveKey = true;
        }
    }
    Assert.assertTrue(haveKey);
    consumer.acknowledge(received);

    // Receive a message asynchronously
    producer.newMessage().value("Hello Pulsar!").send();
    received = consumer.receiveAsync().get();
    msg = (MessageImpl<string>) received;
    haveKey = false;
    foreach (PulsarApi.KeyValue keyValue in msg.MessageBuilder.PropertiesList)
    {
        if ("beforeConsumer".Equals(keyValue.Key))
        {
            haveKey = true;
        }
    }
    Assert.assertTrue(haveKey);
    consumer.acknowledge(received);
    consumer.close();

    CompletableFuture<Message<string>> future = new CompletableFuture<Message<string>>();
    consumer = pulsarClient.newConsumer(Schema_Fields.STRING).topic("persistent://my-property/my-ns/my-topic").subscriptionType(SubscriptionType.Shared).intercept(interceptor).subscriptionName("my-subscription").receiverQueueSize(receiverQueueSize.Value).messageListener((c, m) =>
    {
        try
        {
            c.acknowledge(m);
        }
        catch (Exception e)
        {
            Assert.fail("Failed to acknowledge", e);
        }
        future.complete(m);
    }).subscribe();

    // Receive a message using the message listener
    producer.newMessage().value("Hello Pulsar!").send();
    received = future.get();
    msg = (MessageImpl<string>) received;
    haveKey = false;
    foreach (PulsarApi.KeyValue keyValue in msg.MessageBuilder.PropertiesList)
    {
        if ("beforeConsumer".Equals(keyValue.Key))
        {
            haveKey = true;
        }
    }
    Assert.assertTrue(haveKey);

    producer.close();
    consumer.close();
    }

    public class ConsumerInterceptorAnonymousInnerClass2 : ConsumerInterceptor<string>
    {
        private readonly InterceptorsTest outerInstance;

        public ConsumerInterceptorAnonymousInnerClass2(InterceptorsTest outerInstance)
        {
            this.outerInstance = outerInstance;
        }

        public void close()
        {

        }

        public Message<string> beforeConsume(Consumer<string> consumer, Message<string> message)
        {
            MessageImpl<string> msg = (MessageImpl<string>) message;
            msg.MessageBuilder.addProperties(PulsarApi.KeyValue.newBuilder().setKey("beforeConsumer").setValue("1").build());
            return msg;
        }

        public void onAcknowledge(Consumer<string> consumer, MessageId messageId, Exception cause)
        {
            log.info("onAcknowledge messageId: {}", messageId, cause);
        }

        public void onAcknowledgeCumulative(Consumer<string> consumer, MessageId messageId, Exception cause)
        {
            log.info("onAcknowledgeCumulative messageIds: {}", messageId, cause);
        }

        public void onNegativeAcksSend(Consumer<string> consumer, ISet<MessageId> messageIds)
        {

        }

        public void onAckTimeoutSend(Consumer<string> consumer, ISet<MessageId> messageIds)
        {

        }
    }


    public virtual void testConsumerInterceptorWithMultiTopicSubscribe()
    {

    ConsumerInterceptor<string> interceptor = new ConsumerInterceptorAnonymousInnerClass3(this);

    Producer<string> producer = pulsarClient.newProducer(Schema_Fields.STRING).topic("persistent://my-property/my-ns/my-topic").create();

    Producer<string> producer1 = pulsarClient.newProducer(Schema_Fields.STRING).topic("persistent://my-property/my-ns/my-topic1").create();

    Consumer<string> consumer = pulsarClient.newConsumer(Schema_Fields.STRING).topic("persistent://my-property/my-ns/my-topic", "persistent://my-property/my-ns/my-topic1").subscriptionType(SubscriptionType.Shared).intercept(interceptor).subscriptionName("my-subscription").subscribe();

    producer.newMessage().value("Hello Pulsar!").send();
    producer1.newMessage().value("Hello Pulsar!").send();

    int keyCount = 0;
        for (int i = 0; i < 2; i++)
    {
        Message<string> received = consumer.receive();
        MessageImpl<string> msg = (MessageImpl<string>)((TopicMessageImpl<string>) received).Message;
        foreach (PulsarApi.KeyValue keyValue in msg.MessageBuilder.PropertiesList)
        {
            if ("beforeConsumer".Equals(keyValue.Key))
            {
                keyCount++;
            }
        }
        consumer.acknowledge(received);
    }
    Assert.assertEquals(2, keyCount);
    producer.close();
    producer1.close();
    consumer.close();
    }

    public class ConsumerInterceptorAnonymousInnerClass3 : ConsumerInterceptor<string>
    {
        private readonly InterceptorsTest outerInstance;

        public ConsumerInterceptorAnonymousInnerClass3(InterceptorsTest outerInstance)
        {
            this.outerInstance = outerInstance;
        }

        public void close()
        {

        }

        public Message<string> beforeConsume(Consumer<string> consumer, Message<string> message)
        {
            MessageImpl<string> msg = (MessageImpl<string>) message;
            msg.MessageBuilder.addProperties(PulsarApi.KeyValue.newBuilder().setKey("beforeConsumer").setValue("1").build());
            return msg;
        }

        public void onAcknowledge(Consumer<string> consumer, MessageId messageId, Exception cause)
        {
            log.info("onAcknowledge messageId: {}", messageId, cause);
        }

        public void onAcknowledgeCumulative(Consumer<string> consumer, MessageId messageId, Exception cause)
        {
            log.info("onAcknowledgeCumulative messageIds: {}", messageId, cause);
        }

        public void onNegativeAcksSend(Consumer<string> consumer, ISet<MessageId> messageIds)
        {

        }

        public void onAckTimeoutSend(Consumer<string> consumer, ISet<MessageId> messageIds)
        {

        }
    }

    public virtual void testConsumerInterceptorWithPatternTopicSubscribe()
    {

    ConsumerInterceptor<string> interceptor = new ConsumerInterceptorAnonymousInnerClass4(this);

    Producer<string> producer = pulsarClient.newProducer(Schema_Fields.STRING).topic("persistent://my-property/my-ns/my-topic").create();

    Producer<string> producer1 = pulsarClient.newProducer(Schema_Fields.STRING).topic("persistent://my-property/my-ns/my-topic1").create();

    Consumer<string> consumer = pulsarClient.newConsumer(Schema_Fields.STRING).topicsPattern("persistent://my-property/my-ns/my-.*").subscriptionType(SubscriptionType.Shared).intercept(interceptor).subscriptionName("my-subscription").subscribe();

    producer.newMessage().value("Hello Pulsar!").send();
    producer1.newMessage().value("Hello Pulsar!").send();

    int keyCount = 0;
        for (int i = 0; i < 2; i++)
    {
        Message<string> received = consumer.receive();
        MessageImpl<string> msg = (MessageImpl<string>)((TopicMessageImpl<string>) received).Message;
        foreach (PulsarApi.KeyValue keyValue in msg.MessageBuilder.PropertiesList)
        {
            if ("beforeConsumer".Equals(keyValue.Key))
            {
                keyCount++;
            }
        }
        consumer.acknowledge(received);
    }
    Assert.assertEquals(2, keyCount);
    producer.close();
    producer1.close();
    consumer.close();
    }

    public class ConsumerInterceptorAnonymousInnerClass4 : ConsumerInterceptor<string>
    {
        private readonly InterceptorsTest outerInstance;

        public ConsumerInterceptorAnonymousInnerClass4(InterceptorsTest outerInstance)
        {
            this.outerInstance = outerInstance;
        }

        public void close()
        {

        }

        public Message<string> beforeConsume(Consumer<string> consumer, Message<string> message)
        {
            MessageImpl<string> msg = (MessageImpl<string>) message;
            msg.MessageBuilder.addProperties(PulsarApi.KeyValue.newBuilder().setKey("beforeConsumer").setValue("1").build());
            return msg;
        }

        public void onAcknowledge(Consumer<string> consumer, MessageId messageId, Exception cause)
        {
            log.info("onAcknowledge messageId: {}", messageId, cause);
        }

        public void onAcknowledgeCumulative(Consumer<string> consumer, MessageId messageId, Exception cause)
        {
            log.info("onAcknowledgeCumulative messageIds: {}", messageId, cause);
        }

        public void onNegativeAcksSend(Consumer<string> consumer, ISet<MessageId> messageIds)
        {

        }

        public void onAckTimeoutSend(Consumer<string> consumer, ISet<MessageId> messageIds)
        {

        }
    }

    public virtual void testConsumerInterceptorForAcknowledgeCumulative()
    {

    IList<MessageId> ackHolder = new List<MessageId>();

    ConsumerInterceptor<string> interceptor = new ConsumerInterceptorAnonymousInnerClass5(this, ackHolder);

    Consumer<string> consumer = pulsarClient.newConsumer(Schema_Fields.STRING).topic("persistent://my-property/my-ns/my-topic").subscriptionType(SubscriptionType.Failover).intercept(interceptor).subscriptionName("my-subscription").subscribe();

    Producer<string> producer = pulsarClient.newProducer(Schema_Fields.STRING).topic("persistent://my-property/my-ns/my-topic").create();

        for (int i = 0; i < 100; i++)
    {
        producer.newMessage().value("Hello Pulsar!").send();
    }

    int keyCount = 0;
        for (int i = 0; i < 100; i++)
    {
        Message<string> received = consumer.receive();
        MessageImpl<string> msg = (MessageImpl<string>) received;
        foreach (PulsarApi.KeyValue keyValue in msg.MessageBuilder.PropertiesList)
        {
            if ("beforeConsumer".Equals(keyValue.Key))
            {
                keyCount++;
            }
        }
        ackHolder.Add(received.MessageId);
        if (i == 99)
        {
            consumer.acknowledgeCumulative(received);
        }
    }
    Assert.assertEquals(100, keyCount);
    producer.close();
    consumer.close();
    }

    public class ConsumerInterceptorAnonymousInnerClass5 : ConsumerInterceptor<string>
    {
        private readonly InterceptorsTest outerInstance;

        private IList<MessageId> ackHolder;

        public ConsumerInterceptorAnonymousInnerClass5(InterceptorsTest outerInstance, IList<MessageId> ackHolder)
        {
            this.outerInstance = outerInstance;
            this.ackHolder = ackHolder;
        }

        public void close()
        {

        }

        public Message<string> beforeConsume(Consumer<string> consumer, Message<string> message)
        {
            MessageImpl<string> msg = (MessageImpl<string>) message;
            msg.MessageBuilder.addProperties(PulsarApi.KeyValue.newBuilder().setKey("beforeConsumer").setValue("1").build());
            return msg;
        }

        public void onAcknowledge(Consumer<string> consumer, MessageId messageId, Exception cause)
        {
            log.info("onAcknowledge messageId: {}", messageId, cause);
        }

        public void onAcknowledgeCumulative(Consumer<string> consumer, MessageId messageId, Exception cause)
        {
            long acknowledged = ackHolder.Where(m => (m.compareTo(messageId) <= 0)).Count();
            Assert.assertEquals(acknowledged, 100);
            ackHolder.Clear();
            log.info("onAcknowledgeCumulative messageIds: {}", messageId, cause);
        }

        public void onNegativeAcksSend(Consumer<string> consumer, ISet<MessageId> messageIds)
        {

        }

        public void onAckTimeoutSend(Consumer<string> consumer, ISet<MessageId> messageIds)
        {

        }
    }

    public virtual void testConsumerInterceptorForNegativeAcksSend()
    {
    const int totalNumOfMessages = 100;
    System.Threading.CountdownEvent latch = new System.Threading.CountdownEvent(totalNumOfMessages / 2);

    ConsumerInterceptor<string> interceptor = new ConsumerInterceptorAnonymousInnerClass6(this, latch);

    Consumer<string> consumer = pulsarClient.newConsumer(Schema_Fields.STRING).topic("persistent://my-property/my-ns/my-topic").subscriptionType(SubscriptionType.Failover).intercept(interceptor).negativeAckRedeliveryDelay(100, TimeUnit.MILLISECONDS).subscriptionName("my-subscription").subscribe();

    Producer<string> producer = pulsarClient.newProducer(Schema_Fields.STRING).topic("persistent://my-property/my-ns/my-topic").create();

        for (int i = 0; i < totalNumOfMessages; i++)
    {
        producer.send("Mock message");
    }

    for (int i = 0; i < totalNumOfMessages; i++)
    {
        Message<string> message = consumer.receive();

        if (i % 2 == 0)
        {
            consumer.negativeAcknowledge(message);
        }
        else
        {
            consumer.acknowledge(message);
        }
    }

    latch.await();
    Assert.assertEquals(latch.CurrentCount, 0);

    producer.close();
    consumer.close();
    }

    public class ConsumerInterceptorAnonymousInnerClass6 : ConsumerInterceptor<string>
    {
        private readonly InterceptorsTest outerInstance;

        private System.Threading.CountdownEvent latch;

        public ConsumerInterceptorAnonymousInnerClass6(InterceptorsTest outerInstance, System.Threading.CountdownEvent latch)
        {
            this.outerInstance = outerInstance;
            this.latch = latch;
        }

        public void close()
        {

        }

        public Message<string> beforeConsume(Consumer<string> consumer, Message<string> message)
        {
            return message;
        }

        public void onAcknowledge(Consumer<string> consumer, MessageId messageId, Exception cause)
        {

        }

        public void onAcknowledgeCumulative(Consumer<string> consumer, MessageId messageId, Exception cause)
        {

        }

        public void onNegativeAcksSend(Consumer<string> consumer, ISet<MessageId> messageIds)
        {
            messageIds.forEach(messageId => latch.Signal());
        }

        public void onAckTimeoutSend(Consumer<string> consumer, ISet<MessageId> messageIds)
        {

        }
    }

    public virtual void testConsumerInterceptorForAckTimeoutSend()
    {
    const int totalNumOfMessages = 100;
    System.Threading.CountdownEvent latch = new System.Threading.CountdownEvent(totalNumOfMessages / 2);

    ConsumerInterceptor<string> interceptor = new ConsumerInterceptorAnonymousInnerClass7(this, latch);

    Producer<string> producer = pulsarClient.newProducer(Schema_Fields.STRING).topic("persistent://my-property/my-ns/my-topic").create();

    Consumer<string> consumer = pulsarClient.newConsumer(Schema_Fields.STRING).topic("persistent://my-property/my-ns/my-topic").subscriptionName("foo").intercept(interceptor).ackTimeout(2, TimeUnit.SECONDS).subscribe();

        for (int i = 0; i < totalNumOfMessages; i++)
    {
        producer.send("Mock message");
    }

    for (int i = 0; i < totalNumOfMessages; i++)
    {
        Message<string> message = consumer.receive();

        if (i % 2 == 0)
        {
            consumer.acknowledge(message);
        }
    }

    latch.await();
    Assert.assertEquals(latch.CurrentCount, 0);

    producer.close();
    consumer.close();
    }

    public class ConsumerInterceptorAnonymousInnerClass7 : ConsumerInterceptor<string>
    {
        private readonly InterceptorsTest outerInstance;

        private System.Threading.CountdownEvent latch;

        public ConsumerInterceptorAnonymousInnerClass7(InterceptorsTest outerInstance, System.Threading.CountdownEvent latch)
        {
            this.outerInstance = outerInstance;
            this.latch = latch;
        }

        public void close()
        {

        }

        public Message<string> beforeConsume(Consumer<string> consumer, Message<string> message)
        {
            return message;
        }

        public void onAcknowledge(Consumer<string> consumer, MessageId messageId, Exception cause)
        {

        }

        public void onAcknowledgeCumulative(Consumer<string> consumer, MessageId messageId, Exception cause)
        {

        }

        public void onNegativeAcksSend(Consumer<string> consumer, ISet<MessageId> messageIds)
        {
        }

        public void onAckTimeoutSend(Consumer<string> consumer, ISet<MessageId> messageIds)
        {
            messageIds.forEach(messageId => latch.Signal());
        }
    }
    }

    }
}