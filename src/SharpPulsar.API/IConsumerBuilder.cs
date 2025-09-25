
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

/*
 * Licensed to the Apache Software Foundation (ASF) under one
 * or more contributor license agreements.  See the NOTICE file
 * distributed with this work for additional information
 * regarding copyright ownership.  The ASF licenses this file
 * to you under the Apache License, Version 2.0 (the
 * "License"); you may not use this file except in compliance
 * with the License.  You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing,
 * software distributed under the License is distributed on an
 * "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
 * KIND, either express or implied.  See the License for the
 * specific language governing permissions and limitations
 * under the License.
 */
namespace SharpPulsar.API
{

    /// <summary>
    /// <seealso cref="IConsumerBuilder"/> is used to configure and create instances of <seealso cref="IConsumer"/>.
    /// </summary>
    /// <seealso cref="PulsarClient.newConsumer()"
    /// 
    /// @since 2.0.0/>
    public interface IConsumerBuilder<T> : ICloneable
    {

        /// <summary>
        /// Create a copy of the current consumer builder.
        /// 
        /// <para>Cloning the builder can be used to share an incomplete configuration and specialize it multiple times. For
        /// example:
        /// <pre>{@code
        /// ConsumerBuilder<String> builder = client.newConsumer(Schema.STRING)
        ///         .subscriptionName("my-subscription-name")
        ///         .subscriptionType(SubscriptionType.Shared)
        ///         .receiverQueueSize(10);
        /// 
        /// Consumer<String> consumer1 = builder.clone().topic("my-topic-1").subscribe();
        /// Consumer<String> consumer2 = builder.clone().topic("my-topic-2").subscribe();
        /// }</pre>
        /// 
        /// </para>
        /// </summary>
        /// <returns> a cloned consumer builder object </returns>
        IConsumerBuilder<T> Clone();

        /// <summary>
        /// Load the configuration from provided <tt>config</tt> map.
        /// 
        /// <para>Example:
        /// <pre>{@code
        /// Map<String, Object> config = new HashMap<>();
        /// config.put("ackTimeoutMillis", 1000);
        /// config.put("receiverQueueSize", 2000);
        /// 
        /// Consumer<byte[]> builder = client.newConsumer()
        ///              .loadConf(config)
        ///              .subscribe();
        /// 
        /// Consumer<byte[]> consumer = builder.subscribe();
        /// }</pre>
        /// 
        /// </para>
        /// </summary>
        /// <param name="config"> configuration to load </param>
        /// <returns> the consumer builder instance </returns>
        IConsumerBuilder<T> LoadConf(IDictionary<string, object> config);

        /// <summary>
        /// Finalize the <seealso cref="Consumer"/> creation by subscribing to the topic.
        /// 
        /// <para>If the subscription does not exist, a new subscription is created. By default, the subscription
        /// is created at the end of the topic. See <seealso cref="subscriptionInitialPosition(SubscriptionInitialPosition)"/>
        /// to configure the initial position behavior.
        /// 
        /// </para>
        /// <para>Once a subscription is created, it retains the data and the subscription cursor even if the consumer
        /// is not connected.
        /// 
        /// </para>
        /// </summary>
        /// <returns> the consumer builder instance </returns>
        /// <exception cref="PulsarClientException">
        ///             if the subscribe operation fails </exception>
        
        Action<T> Subscribe();

        /// <summary>
        /// Finalize the <seealso cref="Consumer"/> creation by subscribing to the topic in asynchronous mode.
        /// 
        /// <para>If the subscription does not exist, a new subscription is created. By default, the subscription
        /// is created at the end of the topic. See <seealso cref="subscriptionInitialPosition(SubscriptionInitialPosition)"/>
        /// to configure the initial position behavior.
        /// 
        /// </para>
        /// <para>Once a subscription is created, it retains the data and the subscription cursor even
        /// if the consumer is not connected.
        /// 
        /// </para>
        /// </summary>
        /// <returns> a future that yields a <seealso cref="Consumer"/> instance </returns>
        /// <exception cref="PulsarClientException">
        ///             if the subscribe operation fails </exception>
        ValueTask<Action<T>> SubscribeAsync();

        /// <summary>
        /// Specify the topics this consumer subscribes to.
        /// </summary>
        /// <param name="topicNames"> a set of topics that the consumer subscribes to </param>
        /// <returns> the consumer builder instance </returns>
        IConsumerBuilder<T> Topic(params string[] topicNames);

        /// <summary>
        /// Specify a list of topics that this consumer subscribes to.
        /// </summary>
        /// <param name="topicNames"> a list of topics that the consumer subscribes to </param>
        /// <returns> the consumer builder instance </returns>
        IConsumerBuilder<T> Topics(IList<string> topicNames);

        /// <summary>
        /// Specify a pattern for topics(not contains the partition suffix) that this consumer subscribes to.
        /// 
        /// <para>Will ignore the topic domain("persistent://" or "non-persistent://") when pattern matching.
        /// 
        /// </para>
        /// <para>The pattern is applied to subscribe to all topics, within a single namespace, that match the
        /// pattern.
        /// 
        /// </para>
        /// <para>The consumer automatically subscribes to topics created after itself.
        /// 
        /// </para>
        /// </summary>
        /// <param name="topicsPattern">
        ///            a regular expression to select a list of topics(not contains the partition suffix) to subscribe to </param>
        /// <returns> the consumer builder instance </returns>
        IConsumerBuilder<T> TopicsPattern(Regex topicsPattern);

        /// <summary>
        /// Specify a pattern for topics(not contains the partition suffix) that this consumer subscribes to.
        /// 
        /// <para>It accepts a regular expression that is compiled into a pattern internally. E.g.,
        /// "persistent://public/default/pattern-topic-.*" or "public/default/pattern-topic-.*"
        /// 
        /// </para>
        /// <para>Will ignore the topic domain("persistent://" or "non-persistent://") when pattern matching.
        /// 
        /// </para>
        /// <para>The pattern is applied to subscribe to all topics, within a single namespace, that match the
        /// pattern.
        /// 
        /// </para>
        /// <para>The consumer automatically subscribes to topics created after itself.
        /// 
        /// </para>
        /// </summary>
        /// <param name="topicsPattern">
        ///            given regular expression for topics(not contains the partition suffix) pattern </param>
        /// <returns> the consumer builder instance </returns>
        IConsumerBuilder<T> TopicsPattern(string topicsPattern);

        /// <summary>
        /// Specify the subscription name for this consumer.
        /// 
        /// <para>This argument is required when constructing the consumer.
        /// 
        /// </para>
        /// </summary>
        /// <param name="subscriptionName"> the name of the subscription that this consumer should attach to
        /// </param>
        /// <returns> the consumer builder instance </returns>
        IConsumerBuilder<T> SubscriptionName(string subscriptionName);

        /// <summary>
        /// Specify the subscription properties for this subscription.
        /// Properties are immutable, and consumers under the same subscription will fail to create a subscription
        /// if they use different properties. </summary>
        /// <param name="subscriptionProperties"> the properties of the subscription </param>
        /// <returns> the consumer builder instance </returns>
        IConsumerBuilder<T> SubscriptionProperties(IDictionary<string, string> subscriptionProperties);


        /// <summary>
        /// Sets the timeout for unacknowledged messages, truncated to the nearest millisecond. The timeout must be
        /// greater than 1 second.
        /// 
        /// <para>By default, the acknowledgment timeout is disabled (set to `0`, which means infinite).
        /// When a consumer with an infinite acknowledgment timeout terminates, any unacknowledged
        /// messages that it receives are re-delivered to another consumer.
        /// 
        /// </para>
        /// <para>When enabling acknowledgment timeout, if a message is not acknowledged within the specified timeout,
        /// it is re-delivered to the consumer (possibly to a different consumer, in the case of
        /// a shared subscription).
        /// 
        /// </para>
        /// </summary>
        /// <param name="ackTimeout">
        ///            for unacked messages. </param>
        /// <param name="timeUnit">
        ///            unit in which the timeout is provided. </param>
        /// <returns> the consumer builder instance </returns>
        IConsumerBuilder<T> AckTimeout(long ackTimeout, TimeUnit.TimeUnit timeUnit);

        /// <summary>
        /// Enables or disables the acknowledgment receipt feature.
        /// 
        /// <para>When this feature is enabled, the consumer ensures that acknowledgments are processed by the broker by
        /// waiting for a receipt from the broker. Even when the broker returns a receipt, it doesn't guarantee that the
        /// message won't be redelivered later due to certain implementation details.
        /// It is recommended to use the asynchronous <seealso cref="Consumer.acknowledgeAsync(Message)"/> method for acknowledgment
        /// when this feature is enabled. This is because using the synchronous <seealso cref="Consumer.acknowledge(Message)"/> method
        /// with acknowledgment receipt can cause performance issues due to the round trip to the server, which prevents
        /// pipelining (having multiple messages in-flight). With the asynchronous method, the consumer can continue
        /// consuming other messages while waiting for the acknowledgment receipts.
        /// 
        /// </para>
        /// </summary>
        /// <param name="isAckReceiptEnabled"> {@code true} to enable acknowledgment receipt, {@code false} to disable it </param>
        /// <returns> the consumer builder instance </returns>
        IConsumerBuilder<T> IsAckReceiptEnabled(bool isAckReceiptEnabled);
        /// <summary>
        /// Define the granularity of the ack-timeout redelivery.
        /// 
        /// <para>By default, the tick time is set to 1 second. Using a higher tick time
        /// reduces the memory overhead to track messages when the ack-timeout is set to
        /// bigger values (e.g., 1 hour).
        /// 
        /// </para>
        /// </summary>
        /// <param name="tickTime">
        ///            the min precision for the acknowledgment timeout messages tracker </param>
        /// <param name="timeUnit">
        ///            unit in which the timeout is provided. </param>
        /// <returns> the consumer builder instance </returns>
        IConsumerBuilder<T> AckTimeoutTickTime(long tickTime, TimeUnit.TimeUnit timeUnit);

        /// <summary>
        /// Sets the delay to wait before re-delivering messages that have failed to be processed.
        /// 
        /// <para>When application uses <seealso cref="Consumer.negativeAcknowledge(Message)"/>, the failed message
        /// is redelivered after a fixed timeout. The default is 1 min.
        /// 
        /// </para>
        /// </summary>
        /// <param name="redeliveryDelay">
        ///            redelivery delay for failed messages </param>
        /// <param name="timeUnit">
        ///            unit in which the timeout is provided. </param>
        /// <returns> the consumer builder instance </returns>
        /// <seealso cref="Consumer.negativeAcknowledge(Message)"/>
        IConsumerBuilder<T> NegativeAckRedeliveryDelay(long redeliveryDelay, TimeUnit.TimeUnit timeUnit);

        /// <summary>
        /// Sets the redelivery time precision bit count. The lower bits of the redelivery time will be
        /// trimmed to reduce the memory occupation. The default value is 8, which means the redelivery time
        /// will be bucketed by 256ms, the redelivery time could be earlier(no later) than the expected time,
        /// but no more than 256ms. If set to k, the redelivery time will be bucketed by 2^k ms.
        /// If the value is 0, the redelivery time will be accurate to ms.
        /// </summary>
        /// <param name="negativeAckPrecisionBitCount">
        ///            The redelivery time precision bit count. </param>
        /// <returns> the consumer builder instance </returns>
        IConsumerBuilder<T> NegativeAckRedeliveryDelayPrecision(int negativeAckPrecisionBitCount);

        /// <summary>
        /// Select the subscription type to be used when subscribing to a topic.
        /// 
        /// <para>Options are:
        /// <ul>
        ///  <li><seealso cref="SubscriptionType.Exclusive"/> (Default)</li>
        ///  <li><seealso cref="SubscriptionType.Failover"/></li>
        ///  <li><seealso cref="SubscriptionType.Shared"/></li>
        ///  <li><seealso cref="SubscriptionType.Key_Shared"/></li>
        /// </ul>
        /// 
        /// </para>
        /// </summary>
        /// <param name="subscriptionType">
        ///            the subscription type value </param>
        /// <returns> the consumer builder instance </returns>
        IConsumerBuilder<T> SubscriptionType(SubscriptionType subscriptionType);

        /// <summary>
        /// Selects the subscription mode to be used when subscribing to a topic.
        /// 
        /// <para>Options are:
        /// <ul>
        ///  <li><seealso cref="SubscriptionMode.Durable"/> (Default)</li>
        ///  <li><seealso cref="SubscriptionMode.NonDurable"/></li>
        /// </ul>
        /// 
        /// </para>
        /// </summary>
        /// <param name="subscriptionMode">
        ///            the subscription mode value </param>
        /// <returns> the consumer builder instance </returns>
        IConsumerBuilder<T> SubscriptionMode(SubscriptionMode subscriptionMode);

        /// <summary>
        /// Sets a <seealso cref="MessageListener"/> for the consumer.
        /// 
        /// <para>The application receives messages through the message listener,
        /// and calls to <seealso cref="Consumer.receive()"/> are not allowed.
        /// 
        /// </para>
        /// </summary>
        /// <param name="messageListener">
        ///            the listener object </param>
        /// <returns> the consumer builder instance </returns>
        IConsumerBuilder<T> MessageListener(IMessageListener<T> messageListener);

        /// <summary>
        /// Set the <seealso cref="MessageListenerExecutor"/> to be used for message listeners of <b>current consumer</b>.
        /// <i>(default: use executor from PulsarClient,
        /// <seealso cref="org.apache.pulsar.client.impl.PulsarClientImpl.externalExecutorProvider"/>)</i>.
        /// 
        /// <para>The listener thread pool is exclusively owned by current consumer
        /// that are using a "listener" model to get messages. For a given internal consumer,
        /// the listener will always be invoked from the same thread, to ensure ordering.
        /// 
        /// </para>
        /// <para> The caller need to shut down the thread pool after closing the consumer to avoid leaks.
        /// </para>
        /// </summary>
        /// <param name="messageListenerExecutor"> the executor of the consumer message listener </param>
        /// <returns> the consumer builder instance </returns>
        IConsumerBuilder<T> MessageListenerExecutor(MessageListenerExecutor messageListenerExecutor);

        /// <summary>
        /// Sets a <seealso cref="CryptoKeyReader"/>.
        /// 
        /// <para>Configure the key reader to be used to decrypt message payloads.
        /// 
        /// </para>
        /// </summary>
        /// <param name="cryptoKeyReader">
        ///            CryptoKeyReader object </param>
        /// <returns> the consumer builder instance </returns>
        IConsumerBuilder<T> CryptoKeyReader(ICryptoKeyReader cryptoKeyReader);

        /// <summary>
        /// Sets the default implementation of <seealso cref="CryptoKeyReader"/>.
        /// 
        /// <para>Configure the key reader to be used to decrypt message payloads.
        /// 
        /// </para>
        /// </summary>
        /// <param name="privateKey">
        ///            the private key that is always used to decrypt message payloads. </param>
        /// <returns> the consumer builder instance
        /// @since 2.8.0 </returns>
        IConsumerBuilder<T> DefaultCryptoKeyReader(string privateKey);

        /// <summary>
        /// Sets the default implementation of <seealso cref="CryptoKeyReader"/>.
        /// 
        /// <para>Configure the key reader to be used to decrypt the message payloads.
        /// 
        /// </para>
        /// </summary>
        /// <param name="privateKeys">
        ///            the map of private key names and their URIs used to decrypt message payloads. </param>
        /// <returns> the consumer builder instance
        /// @since 2.8.0 </returns>
        IConsumerBuilder<T> DefaultCryptoKeyReader(IDictionary<string, string> privateKeys);

        /// <summary>
        /// Sets a <seealso cref="MessageCrypto"/>.
        /// 
        /// <para>Contains methods to encrypt/decrypt messages for end-to-end encryption.
        /// 
        /// </para>
        /// </summary>
        /// <param name="messageCrypto">
        ///            MessageCrypto object </param>
        /// <returns> the consumer builder instance </returns>
        IConsumerBuilder<T> MessageCrypto(IMessageCrypto messageCrypto);

        /// <summary>
        /// Sets the ConsumerCryptoFailureAction to the value specified.
        /// </summary>
        /// <param name="action">
        ///            the action the consumer takes in case of decryption failures </param>
        /// <returns> the consumer builder instance </returns>
        IConsumerBuilder<T> CryptoFailureAction(ConsumerCryptoFailureAction action);

        /// <summary>
        /// Sets the size of the consumer receive queue.
        /// 
        /// <para>The consumer receive queue controls how many messages can be accumulated by the <seealso cref="Consumer"/> before the
        /// application calls <seealso cref="Consumer.receive()"/>. Using a higher value can potentially increase consumer
        /// throughput at the expense of bigger memory utilization.
        /// 
        /// </para>
        /// <para>For the consumer that subscribes to the partitioned topic, the parameter
        /// <seealso cref="ConsumerBuilder.maxTotalReceiverQueueSizeAcrossPartitions"/> also affects
        /// the number of messages accumulated in the consumer.
        /// 
        /// </para>
        /// <para><b>Setting the consumer queue size as zero</b>
        /// <ul>
        /// <li>Decreases the throughput of the consumer by disabling pre-fetching of messages. This approach improves the
        /// message distribution on shared subscriptions by pushing messages only to the consumers that are ready to process
        /// them. Neither <seealso cref="Consumer.receive(int, TimeUnit)"/> nor Partitioned Topics can be used if the consumer queue
        /// size is zero. <seealso cref="Consumer.receive()"/> function call should not be interrupted when the consumer queue size is
        /// zero.</li>
        /// <li>Doesn't support Batch-Message. If a consumer receives a batch-message, it closes the consumer connection with
        /// the broker and <seealso cref="Consumer.receive()"/> calls will throw <seealso cref="PulsarClientException"/>
        /// while <seealso cref="Consumer.receiveAsync()"/> receives
        /// <seealso cref="PulsarClientException"/> in callback.
        /// 
        /// <b> The consumer is not able to receive any further messages unless batch-message in pipeline
        /// is removed.</b></li>
        /// </ul>
        /// The default value is {@code 1000} messages and should be adequate for most use cases.
        /// 
        /// </para>
        /// </summary>
        /// <param name="receiverQueueSize">
        ///            the new receiver queue size value </param>
        /// <returns> the consumer builder instance </returns>
        IConsumerBuilder<T> ReceiverQueueSize(int receiverQueueSize);

        /// <summary>
        /// Sets amount of time for group consumer acknowledgments.
        /// 
        /// <para>By default, the consumer uses a 100 ms grouping time to send out acknowledgments to the broker.
        /// 
        /// </para>
        /// <para>Setting a group time of 0 sends out acknowledgments immediately. A longer acknowledgment group time
        /// is more efficient, but at the expense of a slight increase in message re-deliveries after a failure.
        /// 
        /// </para>
        /// </summary>
        /// <param name="delay">
        ///            the max amount of time an acknowledgement can be delayed </param>
        /// <param name="unit">
        ///            the time unit for the delay </param>
        /// <returns> the consumer builder instance </returns>
        IConsumerBuilder<T> AcknowledgmentGroupTime(long delay, TimeUnit.TimeUnit unit);

        /// <summary>
        /// Set the number of messages for group consumer acknowledgments.
        /// 
        /// <para>By default, the consumer uses at most 1000 messages to send out acknowledgments to the broker.
        /// 
        /// </para>
        /// </summary>
        /// <param name="messageNum">
        /// </param>
        /// <returns> the consumer builder instance </returns>
        IConsumerBuilder<T> MaxAcknowledgmentGroupSize(int messageNum);

        /// 
        /// <param name="replicateSubscriptionState"> </param>
        IConsumerBuilder<T> ReplicateSubscriptionState(bool replicateSubscriptionState);

        /// <summary>
        /// Sets the max total receiver queue size across partitions.
        /// 
        /// <para>This setting is used to reduce the receiver queue size for individual partitions
        /// <seealso cref="receiverQueueSize(int)"/> if the total exceeds this value (default: 50000).
        /// The purpose of this setting is to have an upper-limit on the number
        /// of messages that a consumer can be pushed at once from a broker, across all
        /// the partitions.
        /// 
        /// </para>
        /// <para>This setting is applicable only to consumers subscribing to partitioned topics. In such cases, there will
        /// be multiple queues for each partition and a single queue for the parent consumer. This setting controls the
        /// queues of all partitions, not the parent queue. For instance, if a consumer subscribes to a single partitioned
        /// topic, the total number of messages accumulated in this consumer will be the sum of
        /// <seealso cref="receiverQueueSize(int)"/> and maxTotalReceiverQueueSizeAcrossPartitions.
        /// 
        /// </para>
        /// </summary>
        /// <param name="maxTotalReceiverQueueSizeAcrossPartitions"> max pending messages across all the partitions </param>
        /// <returns> the consumer builder instance </returns>
        IConsumerBuilder<T> MaxTotalReceiverQueueSizeAcrossPartitions(int maxTotalReceiverQueueSizeAcrossPartitions);

        /// <summary>
        /// Sets the consumer name.
        /// 
        /// <para>Consumer names are informative, and can be used to identify a particular consumer
        /// instance from the topic stats.
        /// 
        /// </para>
        /// </summary>
        /// <param name="consumerName"> </param>
        /// <returns> the consumer builder instance </returns>
        IConsumerBuilder<T> ConsumerName(string consumerName);

        /// <summary>
        /// Sets a <seealso cref="ConsumerEventListener"/> for the consumer.
        /// 
        /// <para>The consumer group listener is used for receiving consumer state changes in a consumer group for failover
        /// subscriptions. The application can then react to the consumer state changes.
        /// 
        /// </para>
        /// </summary>
        /// <param name="consumerEventListener">
        ///            the consumer group listener object </param>
        /// <returns> the consumer builder instance </returns>
        IConsumerBuilder<T> ConsumerEventListener(IConsumerEventListener consumerEventListener);

        /// <summary>
        /// If enabled, the consumer reads messages from the compacted topic rather than the full message topic backlog.
        /// This means that, if the topic has been compacted, the consumer will only see the latest value for
        /// each key in the topic, up until the point in the topic message backlog that has been compacted. Beyond that
        /// point, the messages are sent as normal.
        /// 
        /// <para>readCompacted can only be enabled on subscriptions to persistent topics with a single active consumer
        /// (i.e. failover or exclusive subscriptions). Enabling readCompacted on subscriptions to non-persistent
        /// topics or on shared subscriptions will cause the subscription call to throw a PulsarClientException.
        /// 
        /// </para>
        /// </summary>
        /// <param name="readCompacted">
        ///            whether to read from the compacted topic or full message topic backlog </param>
        /// <returns> the consumer builder instance </returns>
        IConsumerBuilder<T> ReadCompacted(bool readCompacted);

        /// <summary>
        /// Sets topic's auto-discovery period when using a pattern for topic's consumer.
        /// The period is in minutes, and the default and minimum values are 1 minute.
        /// </summary>
        /// <param name="periodInMinutes">
        ///            number of minutes between checks for
        ///            new topics matching pattern set with <seealso cref="topicsPattern(String)"/> </param>
        /// <returns> the consumer builder instance </returns>
        IConsumerBuilder<T> PatternAutoDiscoveryPeriod(int periodInMinutes);


        /// <summary>
        /// Sets topic's auto-discovery period when using a pattern for topic's consumer.
        /// The default value of period is 1 minute, with a minimum of 1 second.
        /// </summary>
        /// <param name="interval">
        ///            the amount of delay between checks for
        ///            new topics matching pattern set with <seealso cref="topicsPattern(String)"/> </param>
        /// <param name="unit">
        ///            the unit of the topics auto discovery period
        /// </param>
        /// <returns> the consumer builder instance </returns>
        IConsumerBuilder<T> PatternAutoDiscoveryPeriod(int interval, TimeUnit.TimeUnit unit);


        /// <summary>
        /// <b>Shared subscription</b>
        /// <para>Sets priority level for shared subscription consumers to determine which consumers the broker prioritizes when
        /// dispatching messages. Here, the broker follows descending priorities. (eg: 0=max-priority, 1, 2,..)
        /// 
        /// </para>
        /// <para>In Shared subscription mode, the broker first dispatches messages to max priority-level
        /// consumers if they have permits, otherwise the broker considers next priority level consumers.
        /// 
        /// </para>
        /// <para>If a subscription has consumer-A with priorityLevel 0 and Consumer-B with priorityLevel 1,
        /// then the broker dispatches messages to only consumer-A until it is drained, and then the broker will
        /// start dispatching messages to Consumer-B.
        /// 
        /// </para>
        /// <para><pre>
        /// Consumer PriorityLevel Permits
        /// C1       0             2
        /// C2       0             1
        /// C3       0             1
        /// C4       1             2
        /// C5       1             1
        /// Order in which broker dispatches messages to consumers: C1, C2, C3, C1, C4, C5, C4
        /// </pre>
        /// 
        /// </para>
        /// <para><b>Failover subscription for partitioned topic</b>
        /// The broker selects the active consumer for a failover subscription for a partitioned topic
        /// based on consumer's priority-level and lexicographical sorting of consumer name.
        /// eg:
        /// <pre>
        /// 1. Active consumer = C1 : Same priority-level and lexicographical sorting
        /// Consumer PriorityLevel Name
        /// C1       0             aaa
        /// C2       0             bbb
        /// 
        /// 2. Active consumer = C2 : Consumer with highest priority
        /// Consumer PriorityLevel Name
        /// C1       1             aaa
        /// C2       0             bbb
        /// 
        /// Partitioned-topics:
        /// Broker evenly assigns partitioned topics to highest priority consumers.
        /// </pre>
        /// 
        /// </para>
        /// <para>Priority level has no effect on failover subscriptions for non-partitioned topics.
        /// 
        /// </para>
        /// </summary>
        /// <param name="priorityLevel"> the priority of this consumer </param>
        /// <returns> the consumer builder instance </returns>
        IConsumerBuilder<T> PriorityLevel(int priorityLevel);

        /// <summary>
        /// Sets a name/value property with this consumer.
        /// 
        /// <para>Properties are application-defined metadata that can be attached to the consumer.
        /// When getting topic stats, this metadata is associated with the consumer stats for easier identification.
        /// 
        /// </para>
        /// </summary>
        /// <param name="key">
        ///            the property key </param>
        /// <param name="value">
        ///            the property value </param>
        /// <returns> the consumer builder instance </returns>
        IConsumerBuilder<T> Property(string key, string value);

        /// <summary>
        /// Add all the properties in the provided map to the consumer.
        /// 
        /// <para>Properties are application-defined metadata that can be attached to the consumer.
        /// When getting topic stats, this metadata is associated with the consumer stats for easier identification.
        /// 
        /// </para>
        /// </summary>
        /// <param name="properties"> the map with properties </param>
        /// <returns> the consumer builder instance </returns>
        IConsumerBuilder<T> Properties(IDictionary<string, string> properties);

        /// <summary>
        /// Sets the <seealso cref="SubscriptionInitialPosition"/> for the consumer.
        /// </summary>
        /// <param name="subscriptionInitialPosition">
        ///            the position where to initialize a newly created subscription </param>
        /// <returns> the consumer builder instance </returns>
        IConsumerBuilder<T> SubscriptionInitialPosition(SubscriptionInitialPosition subscriptionInitialPosition);

        /// <summary>
        /// Determines which topics this consumer should be subscribed to - Persistent, Non-Persistent, or both. Only used
        /// with pattern subscriptions.
        /// </summary>
        /// <param name="regexSubscriptionMode">
        ///            Pattern subscription mode </param>
        IConsumerBuilder<T> SubscriptionTopicsMode(RegexSubscriptionMode regexSubscriptionMode);

        /// <summary>
        /// Intercept <seealso cref="Consumer"/>.
        /// </summary>
        /// <param name="interceptors"> the list of interceptors to intercept the consumer created by this builder. </param>
        IConsumerBuilder<T> Intercept(params IConsumerInterceptor<T>[] interceptors);

        /// <summary>
        /// Sets dead letter policy for a consumer.
        /// 
        /// <para>By default, messages are redelivered as many times as possible until they are acknowledged.
        /// If you enable a dead letter mechanism, messages will have a maxRedeliverCount. When a message exceeds the maximum
        /// number of redeliveries, the message is sent to the Dead Letter Topic and acknowledged automatically.
        /// 
        /// </para>
        /// <para>Enable the dead letter mechanism by setting dead letter policy.
        /// example:
        /// <pre>
        /// client.newConsumer()
        ///          .deadLetterPolicy(DeadLetterPolicy.builder().maxRedeliverCount(10).build())
        ///          .subscribe();
        /// </pre>
        /// Default dead letter topic name is {TopicName}-{Subscription}-DLQ.
        /// To set a custom dead letter topic name:
        /// <pre>
        /// client.newConsumer()
        ///          .deadLetterPolicy(DeadLetterPolicy
        ///              .builder()
        ///              .maxRedeliverCount(10)
        ///              .deadLetterTopic("your-topic-name")
        ///              .build())
        ///          .subscribe();
        /// </pre>
        /// </para>
        /// </summary>
        IConsumerBuilder<T> DeadLetterPolicy(DeadLetterPolicy deadLetterPolicy);

        /// <summary>
        /// If enabled, the consumer auto-subscribes for partition increases.
        /// This is only for partitioned consumers.
        /// </summary>
        /// <param name="autoUpdate">
        ///            whether to auto-update partition increases </param>
        IConsumerBuilder<T> AutoUpdatePartitions(bool autoUpdate);

        /// <summary>
        /// Sets the interval of updating partitions <i>(default: 1 minute)</i>. This only works if autoUpdatePartitions is
        /// enabled.
        /// </summary>
        /// <param name="interval">
        ///            the interval of updating partitions </param>
        /// <param name="unit">
        ///            the time unit of the interval. </param>
        /// <returns> the consumer builder instance </returns>
        IConsumerBuilder<T> AutoUpdatePartitionsInterval(int interval, TimeUnit.TimeUnit unit);

        /// <summary>
        /// Sets KeyShared subscription policy for consumer.
        /// 
        /// <para>By default, KeyShared subscriptions use auto split hash ranges to maintain consumers. If you want to
        /// set a different KeyShared policy, set a policy by using one of the following examples:
        /// 
        /// </para>
        /// <para><b>Sticky hash range policy</b></para>
        /// <pre>
        /// client.newConsumer()
        ///          .keySharedPolicy(KeySharedPolicy.stickyHashRange().ranges(Range.of(0, 10)))
        ///          .subscribe();
        /// </pre>
        /// For details about sticky hash range policies, see <seealso cref="KeySharedPolicy.KeySharedPolicySticky"/>.
        /// 
        /// <para><b>Auto-split hash range policy</b></para>
        /// <pre>
        /// client.newConsumer()
        ///          .keySharedPolicy(KeySharedPolicy.autoSplitHashRange())
        ///          .subscribe();
        /// </pre>
        /// For details about auto-split hash range policies, see <seealso cref="KeySharedPolicy.KeySharedPolicyAutoSplit"/>.
        /// </summary>
        /// <param name="keySharedPolicy"> The <seealso cref="KeySharedPolicy"/> to specify </param>
        IConsumerBuilder<T> KeySharedPolicy(KeySharedPolicy keySharedPolicy);

        /// <summary>
        /// Sets the consumer to include the given position of reset operation <seealso cref="Consumer.seek(MessageId)"/>}.
        /// </summary>
        /// <returns> the consumer builder instance </returns>
        IConsumerBuilder<T> StartMessageIdInclusive();

        /// <summary>
        /// Sets <seealso cref="BatchReceivePolicy"/> for the consumer.
        /// By default, consumer uses <seealso cref="BatchReceivePolicy.DEFAULT_POLICY"/> as batch receive policy.
        /// 
        /// <para>Example:
        /// <pre>
        /// client.newConsumer().batchReceivePolicy(BatchReceivePolicy.builder()
        ///              .maxNumMessages(100)
        ///              .maxNumBytes(5 * 1024 * 1024)
        ///              .timeout(100, TimeUnit.MILLISECONDS)
        ///              .build()).subscribe();
        /// </pre>
        /// </para>
        /// </summary>
        IConsumerBuilder<T> BatchReceivePolicy(BatchReceivePolicy batchReceivePolicy);

        /// <summary>
        /// If enabled, the consumer auto-retries messages.
        /// Default: disabled.
        /// </summary>
        /// <param name="retryEnable">
        ///            whether to auto retry message </param>
        IConsumerBuilder<T> EnableRetry(bool retryEnable);

        /// <summary>
        /// Enable or disable batch index acknowledgment. To enable this feature, ensure batch index acknowledgment
        /// is enabled on the broker side.
        /// Default: true
        /// </summary>
        IConsumerBuilder<T> EnableBatchIndexAcknowledgment(bool batchIndexAcknowledgmentEnabled);

        /// <summary>
        /// Consumer buffers chunk messages into memory until it receives all the chunks of the original message. While
        /// consuming chunk-messages, chunks from same message might not be contiguous in the stream and they might be mixed
        /// with other messages' chunks. so, consumer has to maintain multiple buffers to manage chunks coming from different
        /// messages. This mainly happens when multiple publishers are publishing messages on the topic concurrently or
        /// publisher failed to publish all chunks of the messages.
        /// 
        /// <pre>
        /// eg: M1-C1, M2-C1, M1-C2, M2-C2
        /// Here, Messages M1-C1 and M1-C2 belong to original message M1, M2-C1 and M2-C2 messages belong to M2 message.
        /// </pre>
        /// Buffering large number of outstanding uncompleted chunked messages can create memory pressure and it can be
        /// guarded by providing this @maxPendingChuckedMessage threshold. Once, consumer reaches this threshold, it drops
        /// the outstanding unchunked-messages by silently acking or asking broker to redeliver later by marking it unacked.
        /// This behavior can be controlled by configuration: @autoAckOldestChunkedMessageOnQueueFull
        /// 
        /// The default value is 10.
        /// </summary>
        /// <param name="maxPendingChuckedMessage">
        /// @return </param>
        /// @deprecated use <seealso cref="maxPendingChunkedMessage(int)"/> 
        [Obsolete("use <seealso cref=\"maxPendingChunkedMessage(int)\"/>")]
        IConsumerBuilder<T> MaxPendingChuckedMessage(int maxPendingChuckedMessage);

        /// <summary>
        /// Consumer buffers chunk messages into memory until it receives all the chunks of the original message. While
        /// consuming chunk-messages, chunks from same message might not be contiguous in the stream and they might be mixed
        /// with other messages' chunks. so, consumer has to maintain multiple buffers to manage chunks coming from different
        /// messages. This mainly happens when multiple publishers are publishing messages on the topic concurrently or
        /// publisher failed to publish all chunks of the messages.
        /// 
        /// <pre>
        /// eg: M1-C1, M2-C1, M1-C2, M2-C2
        /// Here, Messages M1-C1 and M1-C2 belong to original message M1, M2-C1 and M2-C2 messages belong to M2 message.
        /// </pre>
        /// Buffering large number of outstanding uncompleted chunked messages can create memory pressure and it can be
        /// guarded by providing this @maxPendingChunkedMessage threshold. Once, consumer reaches this threshold, it drops
        /// the outstanding unchunked-messages by silently acking or asking broker to redeliver later by marking it unacked.
        /// This behavior can be controlled by configuration: @autoAckOldestChunkedMessageOnQueueFull
        /// 
        /// The default value is 10.
        /// </summary>
        /// <param name="maxPendingChunkedMessage">
        /// @return </param>
        IConsumerBuilder<T> MaxPendingChunkedMessage(int maxPendingChunkedMessage);

        /// <summary>
        /// Buffering large number of outstanding uncompleted chunked messages can create memory pressure and it can be
        /// guarded by providing this @maxPendingChunkedMessage threshold. Once the consumer reaches this threshold, it drops
        /// the outstanding unchunked-messages by silently acknowledging if autoAckOldestChunkedMessageOnQueueFull is true,
        /// otherwise it marks them for redelivery.
        /// 
        /// @default false
        /// </summary>
        /// <param name="autoAckOldestChunkedMessageOnQueueFull">
        /// @return </param>
        IConsumerBuilder<T> AutoAckOldestChunkedMessageOnQueueFull(bool autoAckOldestChunkedMessageOnQueueFull);

        /// <summary>
        /// If the producer fails to publish all the chunks of a message, then the consumer can expire incomplete chunks if
        /// the consumer doesn't receive all chunks during the expiration period (default 1 minute).
        /// </summary>
        /// <param name="duration"> </param>
        /// <param name="unit">
        /// @return </param>
        IConsumerBuilder<T> ExpireTimeOfIncompleteChunkedMessage(long duration, TimeUnit.TimeUnit unit);

        /// <summary>
        /// Enable pooling of messages and the underlying data buffers.
        /// <p/>
        /// When pooling is enabled, the application is responsible for calling Message.release() after the handling of every
        /// received message. If "release()” is not called on a received message, it causes a memory leak. If an
        /// application attempts to use an already "released” message, it might experience undefined behavior (eg: memory
        /// corruption, deserialization error, etc.).
        /// </summary>
        IConsumerBuilder<T> PoolMessages(bool poolMessages);

        /// <summary>
        /// If configured with a non-null value, the consumer uses the processor to process the payload, including
        /// decoding it to messages and triggering the listener.
        /// 
        /// <para><b>Special behavior when <seealso cref="receiverQueueSize(int) receiverQueueSize=0"/>:</b>
        /// When the consumer is configured with <seealso cref="receiverQueueSize(int) receiverQueueSize=0"/>:
        /// <ul>
        ///   <li>For <b>batch messages</b>:
        ///     <ul>
        ///       <li>The payload processor will <i>not</i> be invoked</li>
        ///       <li>The consumer will <b>immediately close itself</b> upon receiving batch messages</li>
        ///       <li>Pending operations will fail with:
        ///         <ul>
        ///           <li>{@code receive()}: throws <seealso cref="PulsarClientException"/></li>
        ///           <li>{@code receiveAsync()}: completes the Future with <seealso cref="PulsarClientException"/></li>
        ///           <li>Message listeners: triggers <seealso cref="Consumer.close()"/> without delivering the message</li>
        ///         </ul>
        ///       </li>
        ///     </ul>
        ///   </li>
        ///   <li>For <b>single messages</b>:
        ///     <ul>
        ///       <li>The payload processor will process messages normally</li>
        ///     </ul>
        ///   </li>
        /// </ul>
        /// 
        /// 
        /// </para>
        /// <para><b>Default behavior <seealso cref="receiverQueueSize(int) receiverQueueSize>0"/>:</b>
        /// All messages (both single and batched) will be processed by the payload processor.
        /// 
        /// Default: null
        /// </para>
        /// </summary>
        IConsumerBuilder<T> MessagePayloadProcessor(IMessagePayloadProcessor payloadProcessor);

        /// <summary>
        /// negativeAckRedeliveryBackoff sets the redelivery backoff policy for messages that are negatively acknowledged
        /// using
        /// `consumer.negativeAcknowledge(Message<?> message)` but not with `consumer.negativeAcknowledge(MessageId
        /// messageId)`.
        /// This setting allows specifying a backoff policy for messages that are negatively acknowledged,
        /// enabling more flexible control over the delay before such messages are redelivered.
        /// 
        /// <para>This configuration accepts a <seealso cref="RedeliveryBackoff"/> object that defines the backoff policy.
        /// The policy can be either a fixed delay or an exponential backoff. An exponential backoff policy
        /// is beneficial in scenarios where increasing the delay between consecutive redeliveries can help
        /// mitigate issues like temporary resource constraints or processing bottlenecks.
        /// 
        /// </para>
        /// <para>Note: This backoff policy does not apply when using `consumer.negativeAcknowledge(MessageId messageId)`
        /// because the redelivery count cannot be determined from just the message ID. It is recommended to use
        /// `consumer.negativeAcknowledge(Message<?> message)` if you want to leverage the redelivery backoff policy.
        /// 
        /// </para>
        /// <para>Example usage:
        /// <pre>{@code
        /// client.newConsumer()
        ///       .negativeAckRedeliveryBackoff(ExponentialRedeliveryBackoff.builder()
        ///           .minDelayMs(1000)   // Set minimum delay to 1 second
        ///           .maxDelayMs(60000)  // Set maximum delay to 60 seconds
        ///           .build())
        ///       .subscribe();
        /// }</pre>
        /// 
        /// </para>
        /// </summary>
        /// <param name="negativeAckRedeliveryBackoff"> the backoff policy to use for negatively acknowledged messages </param>
        /// <returns> the consumer builder instance </returns>
        IConsumerBuilder<T> NegativeAckRedeliveryBackoff(IRedeliveryBackoff negativeAckRedeliveryBackoff);


        /// <summary>
        /// Sets the redelivery backoff policy for messages that are redelivered due to acknowledgement timeout.
        /// This setting allows you to specify a backoff policy for messages that are not acknowledged within
        /// the specified ack timeout. By using a backoff policy, you can control the delay before a message
        /// is redelivered, potentially improving consumer performance by avoiding immediate redelivery of
        /// messages that might still be processing.
        /// 
        /// <para>This method accepts a <seealso cref="RedeliveryBackoff"/> object that defines the backoff policy to be used.
        /// You can use either a fixed backoff policy or an exponential backoff policy. The exponential backoff
        /// policy is particularly useful for scenarios where it may be beneficial to progressively increase the
        /// delay between redeliveries, reducing the load on the consumer and giving more time to process messages.
        /// 
        /// </para>
        /// <para>Example usage:
        /// <pre>{@code
        /// client.newConsumer()
        ///       .ackTimeout(10, TimeUnit.SECONDS)
        ///       .ackTimeoutRedeliveryBackoff(ExponentialRedeliveryBackoff.builder()
        ///           .minDelayMs(1000)   // Set minimum delay to 1 second
        ///           .maxDelayMs(60000)  // Set maximum delay to 60 seconds
        ///           .build())
        ///       .subscribe();
        /// }</pre>
        /// 
        /// </para>
        /// <para>Note: This configuration is effective only if the ack timeout is triggered. It does not apply to
        /// messages negatively acknowledged using the negative acknowledgment API.
        /// 
        /// </para>
        /// </summary>
        /// <param name="ackTimeoutRedeliveryBackoff"> the backoff policy to use for messages that exceed their ack timeout </param>
        /// <returns> the consumer builder instance </returns>
        IConsumerBuilder<T> AckTimeoutRedeliveryBackoff(IRedeliveryBackoff ackTimeoutRedeliveryBackoff);

        /// <summary>
        /// Starts the consumer in a paused state. When enabled, the consumer does not immediately fetch messages when
        /// <seealso cref="subscribe()"/> is called. Instead, the consumer waits to fetch messages until <seealso cref="Consumer.resume()"/> is
        /// called.
        /// <p/>
        /// See also <seealso cref="Consumer.pause()"/>.
        /// @default false
        /// </summary>
        IConsumerBuilder<T> StartPaused(bool paused);

        /// <summary>
        /// If this is enabled, the consumer receiver queue size is initialized as a very small value, 1 by default,
        /// and will double itself until it reaches either the value set by <seealso cref="receiverQueueSize(int)"/> or the client
        /// memory limit set by <seealso cref="ClientBuilder.memoryLimit(long, SizeUnit)"/>.
        /// 
        /// <para>The consumer receiver queue size will double if and only if:
        /// </para>
        /// <para>1) User calls receive() and there are no messages in receiver queue.
        /// </para>
        /// <para>2) The last message we put in the receiver queue took the last space available in receiver queue.
        /// 
        /// </para>
        /// <para>This is disabled by default and currentReceiverQueueSize is initialized as maxReceiverQueueSize.
        /// 
        /// </para>
        /// <para>The feature should be able to reduce client memory usage.
        /// 
        /// </para>
        /// </summary>
        /// <param name="enabled"> whether to enable AutoScaledReceiverQueueSize. </param>
        IConsumerBuilder<T> AutoScaledReceiverQueueSizeEnabled(bool enabled);

        /// <summary>
        /// Configure topic specific options to override those set at the <seealso cref="ConsumerBuilder"/> level.
        /// </summary>
        /// <param name="topicName"> a topic name </param>
        /// <returns> a <seealso cref="TopicConsumerBuilder"/> instance </returns>
        ITopicConsumerBuilder<T> TopicConfiguration(string topicName);

        /// <summary>
        /// Configure topic specific options to override those set at the <seealso cref="ConsumerBuilder"/> level.
        /// </summary>
        /// <param name="topicName"> a topic name </param>
        /// <param name="builderConsumer"> a consumer to allow the configuration of the <seealso cref="TopicConsumerBuilder"/> instance </param>
        IConsumerBuilder<T> TopicConfiguration(string topicName, System.Action<ITopicConsumerBuilder<T>> builderConsumer);

        /// <summary>
        /// Configure topic specific options to override those set at the <seealso cref="ConsumerBuilder"/> level.
        /// </summary>
        /// <param name="topicsPattern"> a regular expression to match a topic name </param>
        /// <returns> a <seealso cref="TopicConsumerBuilder"/> instance </returns>
        ITopicConsumerBuilder<T> TopicConfiguration(Regex topicsPattern);

        /// <summary>
        /// Configure topic specific options to override those set at the <seealso cref="ConsumerBuilder"/> level.
        /// </summary>
        /// <param name="topicsPattern"> a regular expression to match a topic name </param>
        /// <param name="builderConsumer"> a consumer to allow the configuration of the <seealso cref="TopicConsumerBuilder"/> instance </param>
        IConsumerBuilder<T> TopicConfiguration(Regex topicsPattern, System.Action<ITopicConsumerBuilder<T>> builderConsumer);
    }
}
