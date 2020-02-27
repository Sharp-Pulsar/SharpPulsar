using BAMCIS.Util.Concurrent;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using SharpPulsar.Exceptions;

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
namespace SharpPulsar.Api
{

	/// <summary>
	/// <seealso cref="ConsumerBuilder"/> is used to configure and create instances of <seealso cref="Consumer"/>.
	/// </summary>
	/// <seealso cref= IPulsarClient#newConsumer()
	/// 
	/// @since 2.0.0 </seealso>
	public interface IConsumerBuilder : ICloneable
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
		IConsumerBuilder Clone();

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
		IConsumerBuilder LoadConf(IDictionary<string, object> config);

		/// <summary>
		/// Finalize the <seealso cref="Consumer"/> creation by subscribing to the topic.
		/// 
		/// <para>If the subscription does not exist, a new subscription will be created. By default the subscription
		/// will be created at the end of the topic. See <seealso cref="subscriptionInitialPosition(SubscriptionInitialPosition)"/>
		/// to configure the initial position behavior.
		/// 
		/// </para>
		/// <para>Once a subscription is created, it will retain the data and the subscription cursor even if the consumer
		/// is not connected.
		/// 
		/// </para>
		/// </summary>
		/// <returns> the consumer builder instance </returns>
		/// <exception cref="PulsarClientException">
		IConsumer Subscribe();

		/// <summary>
		/// Finalize the <seealso cref="Consumer"/> creation by subscribing to the topic in asynchronous mode.
		/// 
		/// <para>If the subscription does not exist, a new subscription will be created. By default the subscription
		/// will be created at the end of the topic. See <seealso cref="subscriptionInitialPosition(SubscriptionInitialPosition)"/>
		/// to configure the initial position behavior.
		/// 
		/// </para>
		/// <para>Once a subscription is created, it will retain the data and the subscription cursor even
		/// if the consumer is not connected.
		/// 
		/// </para>
		/// </summary>
		/// <returns> a future that will yield a <seealso cref="Consumer"/> instance </returns>
		/// <exception cref="PulsarClientException">
		///             if the the subscribe operation fails </exception>
		ValueTask<IConsumer> SubscribeAsync();

		/// <summary>
		/// Specify the topics this consumer will subscribe on.
		/// </summary>
		/// <param name="topicNames"> a set of topic that the consumer will subscribe on </param>
		/// <returns> the consumer builder instance </returns>
		IConsumerBuilder Topic(params string[] topicNames);

		/// <summary>
		/// Specify a list of topics that this consumer will subscribe on.
		/// </summary>
		/// <param name="topicNames"> a list of topic that the consumer will subscribe on </param>
		/// <returns> the consumer builder instance </returns>
		IConsumerBuilder Topics(IList<string> topicNames);

		/// <summary>
		/// Specify a pattern for topics that this consumer will subscribe on.
		/// 
		/// <para>The pattern will be applied to subscribe to all the topics, within a single namespace, that will match the
		/// pattern.
		/// 
		/// </para>
		/// <para>The consumer will automatically subscribe to topics created after itself.
		/// 
		/// </para>
		/// </summary>
		/// <param name="topicsPattern">
		///            a regular expression to select a list of topics to subscribe to </param>
		/// <returns> the consumer builder instance </returns>
		IConsumerBuilder TopicsPattern(Regex topicsPattern);

		/// <summary>
		/// Specify a pattern for topics that this consumer will subscribe on.
		/// 
		/// <para>It accepts regular expression and will be compiled into a pattern internally. Eg.
		/// "persistent://public/default/pattern-topic-.*"
		/// 
		/// </para>
		/// <para>The pattern will be applied to subscribe to all the topics, within a single namespace, that will match the
		/// pattern.
		/// 
		/// </para>
		/// <para>The consumer will automatically subscribe to topics created after itself.
		/// 
		/// </para>
		/// </summary>
		/// <param name="topicsPattern">
		///            given regular expression for topics pattern </param>
		/// <returns> the consumer builder instance </returns>
		IConsumerBuilder TopicsPattern(string topicsPattern);

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
		IConsumerBuilder SubscriptionName(string subscriptionName);

		/// <summary>
		/// Set the timeout for unacked messages, truncated to the nearest millisecond. The timeout needs to be greater than
		/// 1 second.
		/// 
		/// <para>By default, the acknowledge timeout is disabled and that means that messages delivered to a
		/// consumer will not be re-delivered unless the consumer crashes.
		/// 
		/// </para>
		/// <para>When enabling ack timeout, if a message is not acknowledged within the specified timeout
		/// it will be re-delivered to the consumer (possibly to a different consumer in case of
		/// a shared subscription).
		/// 
		/// </para>
		/// </summary>
		/// <param name="ackTimeout">
		///            for unacked messages. </param>
		/// <param name="timeUnit">
		///            unit in which the timeout is provided. </param>
		/// <returns> the consumer builder instance </returns>
		IConsumerBuilder AckTimeout(long ackTimeout, TimeUnit timeUnit);

		/// <summary>
		/// Define the granularity of the ack-timeout redelivery.
		/// 
		/// <para>By default, the tick time is set to 1 second. Using an higher tick time will
		/// reduce the memory overhead to track messages when the ack-timeout is set to
		/// bigger values (eg: 1hour).
		/// 
		/// </para>
		/// </summary>
		/// <param name="tickTime">
		///            the min precision for the ack timeout messages tracker </param>
		/// <param name="timeUnit">
		///            unit in which the timeout is provided. </param>
		/// <returns> the consumer builder instance </returns>
		IConsumerBuilder AckTimeoutTickTime(long tickTime, TimeUnit timeUnit);

		/// <summary>
		/// Set the delay to wait before re-delivering messages that have failed to be process.
		/// 
		/// <para>When application uses <seealso cref="IConsumer.negativeAcknowledge(IMessage)"/>, the failed message
		/// will be redelivered after a fixed timeout. The default is 1 min.
		/// 
		/// </para>
		/// </summary>
		/// <param name="redeliveryDelay">
		///            redelivery delay for failed messages </param>
		/// <param name="timeUnit">
		///            unit in which the timeout is provided. </param>
		/// <returns> the consumer builder instance </returns>
		/// <seealso cref= Consumer#negativeAcknowledge(Message) </seealso>
		IConsumerBuilder NegativeAckRedeliveryDelay(long redeliveryDelay, TimeUnit timeUnit);

		/// <summary>
		/// Select the subscription type to be used when subscribing to the topic.
		/// 
		/// <para>Options are:
		/// <ul>
		///  <li><seealso cref="SubscriptionType.Exclusive"/> (Default)</li>
		///  <li><seealso cref="SubscriptionType.Failover"/></li>
		///  <li><seealso cref="SubscriptionType.Shared"/></li>
		/// </ul>
		/// 
		/// </para>
		/// </summary>
		/// <param name="subscriptionType">
		///            the subscription type value </param>
		/// <returns> the consumer builder instance </returns>
		IConsumerBuilder SubscriptionType(SubscriptionType subscriptionType);

		/// <summary>
		/// Sets a <seealso cref="MessageListener"/> for the consumer
		/// 
		/// <para>When a <seealso cref="MessageListener"/> is set, application will receive messages through it. Calls to
		/// <seealso cref="IConsumer.receive()"/> will not be allowed.
		/// 
		/// </para>
		/// </summary>
		/// <param name="messageListener">
		///            the listener object </param>
		/// <returns> the consumer builder instance </returns>
		IConsumerBuilder MessageListener(IMessageListener messageListener);

		/// <summary>
		/// Sets a <seealso cref="CryptoKeyReader"/>.
		/// 
		/// <para>Configure the key reader to be used to decrypt the message payloads.
		/// 
		/// </para>
		/// </summary>
		/// <param name="cryptoKeyReader">
		///            CryptoKeyReader object </param>
		/// <returns> the consumer builder instance </returns>
		IConsumerBuilder CryptoKeyReader(ICryptoKeyReader cryptoKeyReader);

		/// <summary>
		/// Sets the ConsumerCryptoFailureAction to the value specified.
		/// </summary>
		/// <param name="action">
		///            the action the consumer will take in case of decryption failures </param>
		/// <returns> the consumer builder instance </returns>
		IConsumerBuilder CryptoFailureAction(ConsumerCryptoFailureAction? action);

		/// <summary>
		/// Sets the size of the consumer receive queue.
		/// 
		/// <para>The consumer receive queue controls how many messages can be accumulated by the <seealso cref="Consumer"/> before the
		/// application calls <seealso cref="IConsumer.receive()"/>. Using a higher value could potentially increase the consumer
		/// throughput at the expense of bigger memory utilization.
		/// 
		/// </para>
		/// <para><b>Setting the consumer queue size as zero</b>
		/// <ul>
		/// <li>Decreases the throughput of the consumer, by disabling pre-fetching of messages. This approach improves the
		/// message distribution on shared subscription, by pushing messages only to the consumers that are ready to process
		/// them. Neither <seealso cref="IConsumer.receive(int, TimeUnit)"/> nor Partitioned Topics can be used if the consumer queue
		/// size is zero. <seealso cref="IConsumer.receive()"/> function call should not be interrupted when the consumer queue size is
		/// zero.</li>
		/// <li>Doesn't support Batch-Message: if consumer receives any batch-message then it closes consumer connection with
		/// broker and <seealso cref="IConsumer.receive()"/> call will remain blocked while <seealso cref="IConsumer.receiveAsync()"/> receives
		/// exception in callback. <b> consumer will not be able receive any further message unless batch-message in pipeline
		/// is removed</b></li>
		/// </ul>
		/// Default value is {@code 1000} messages and should be good for most use cases.
		/// 
		/// </para>
		/// </summary>
		/// <param name="receiverQueueSize">
		///            the new receiver queue size value </param>
		/// <returns> the consumer builder instance </returns>
		IConsumerBuilder ReceiverQueueSize(int receiverQueueSize);

		/// <summary>
		/// Group the consumer acknowledgments for the specified time.
		/// 
		/// <para>By default, the consumer will use a 100 ms grouping time to send out the acknowledgments to the broker.
		/// 
		/// </para>
		/// <para>Setting a group time of 0, will send out the acknowledgments immediately. A longer ack group time
		/// will be more efficient at the expense of a slight increase in message re-deliveries after a failure.
		/// 
		/// </para>
		/// </summary>
		/// <param name="delay">
		///            the max amount of time an acknowledgemnt can be delayed </param>
		/// <param name="unit">
		///            the time unit for the delay </param>
		/// <returns> the consumer builder instance </returns>
		IConsumerBuilder AcknowledgmentGroupTime(long delay, TimeUnit unit);

		/// 
		/// <param name="replicateSubscriptionState"> </param>
		IConsumerBuilder ReplicateSubscriptionState(bool replicateSubscriptionState);

		/// <summary>
		/// Set the max total receiver queue size across partitons.
		/// 
		/// <para>This setting will be used to reduce the receiver queue size for individual partitions
		/// <seealso cref="receiverQueueSize(int)"/> if the total exceeds this value (default: 50000).
		/// The purpose of this setting is to have an upper-limit on the number
		/// of messages that a consumer can be pushed at once from a broker, across all
		/// the partitions.
		/// 
		/// </para>
		/// </summary>
		/// <param name="maxTotalReceiverQueueSizeAcrossPartitions">
		///            max pending messages across all the partitions </param>
		/// <returns> the consumer builder instance </returns>
		IConsumerBuilder MaxTotalReceiverQueueSizeAcrossPartitions(int maxTotalReceiverQueueSizeAcrossPartitions);

		/// <summary>
		/// Set the consumer name.
		/// 
		/// <para>Consumer name is informative and it can be used to indentify a particular consumer
		/// instance from the topic stats.
		/// 
		/// </para>
		/// </summary>
		/// <param name="consumerName"> </param>
		/// <returns> the consumer builder instance </returns>
		IConsumerBuilder ConsumerName(string consumerName);

		/// <summary>
		/// Sets a <seealso cref="ConsumerEventListener"/> for the consumer.
		/// 
		/// <para>The consumer group listener is used for receiving consumer state change in a consumer group for failover
		/// subscription. Application can then react to the consumer state changes.
		/// 
		/// </para>
		/// </summary>
		/// <param name="consumerEventListener">
		///            the consumer group listener object </param>
		/// <returns> the consumer builder instance </returns>
		IConsumerBuilder ConsumerEventListener(IConsumerEventListener consumerEventListener);

		/// <summary>
		/// If enabled, the consumer will read messages from the compacted topic rather than reading the full message backlog
		/// of the topic. This means that, if the topic has been compacted, the consumer will only see the latest value for
		/// each key in the topic, up until the point in the topic message backlog that has been compacted. Beyond that
		/// point, the messages will be sent as normal.
		/// 
		/// <para>readCompacted can only be enabled subscriptions to persistent topics, which have a single active consumer
		/// (i.e. failure or exclusive subscriptions). Attempting to enable it on subscriptions to a non-persistent topics
		/// or on a shared subscription, will lead to the subscription call throwing a PulsarClientException.
		/// 
		/// </para>
		/// </summary>
		/// <param name="readCompacted">
		///            whether to read from the compacted topic </param>
		/// <returns> the consumer builder instance </returns>
		IConsumerBuilder ReadCompacted(bool readCompacted);

		/// <summary>
		/// Set topics auto discovery period when using a pattern for topics consumer.
		/// The period is in minute, and default and minimum value is 1 minute.
		/// </summary>
		/// <param name="periodInMinutes">
		///            number of minutes between checks for
		///            new topics matching pattern set with <seealso cref="topicsPattern(string)"/> </param>
		/// <returns> the consumer builder instance </returns>
		IConsumerBuilder PatternAutoDiscoveryPeriod(int periodInMinutes);

		/// <summary>
		/// <b>Shared subscription</b>
		/// Sets priority level for the shared subscription consumers to which broker gives more priority while dispatching
		/// messages. Here, broker follows descending priorities. (eg: 0=max-priority, 1, 2,..)
		/// 
		/// <para>In Shared subscription mode, broker will first dispatch messages to max priority-level
		/// consumers if they have permits, else broker will consider next priority level consumers.
		/// 
		/// </para>
		/// <para>If subscription has consumer-A with priorityLevel 0 and Consumer-B with priorityLevel 1
		/// then broker will dispatch messages to only consumer-A until it runs out permit and then broker
		/// starts dispatching messages to Consumer-B.
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
		/// <para><b>Failover subscription</b>
		/// Broker selects active consumer for a failover-subscription based on consumer's priority-level and
		/// lexicographical sorting of a consumer name.
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
		/// </summary>
		/// <param name="priorityLevel"> the priority of this consumer </param>
		/// <returns> the consumer builder instance </returns>
		IConsumerBuilder PriorityLevel(int priorityLevel);

		/// <summary>
		/// Set a name/value property with this consumer.
		/// 
		/// <para>Properties are application defined metadata that can be attached to the consumer.
		/// When getting the topic stats, this metadata will be associated to the consumer stats for easier identification.
		/// 
		/// </para>
		/// </summary>
		/// <param name="key">
		///            the property key </param>
		/// <param name="value">
		///            the property value </param>
		/// <param name="key"> </param>
		/// <param name="value"> </param>
		/// <returns> the consumer builder instance </returns>
		IConsumerBuilder Property(string key, string value);

		/// <summary>
		/// Add all the properties in the provided map to the consumer.
		/// 
		/// <para>Properties are application defined metadata that can be attached to the consumer.
		/// When getting the topic stats, this metadata will be associated to the consumer stats for easier identification.
		/// 
		/// </para>
		/// </summary>
		/// <param name="properties"> the map with properties </param>
		/// <returns> the consumer builder instance </returns>
		IConsumerBuilder Properties(IDictionary<string, string> properties);

		/// <summary>
		/// Set the <seealso cref="SubscriptionInitialPosition"/> for the consumer.
		/// </summary>
		/// <param name="subscriptionInitialPosition">
		///            the position where to initialize a newly created subscription </param>
		/// <returns> the consumer builder instance </returns>
		IConsumerBuilder SubscriptionInitialPosition(SubscriptionInitialPosition subscriptionInitialPosition);

		/// <summary>
		/// Determines to which topics this consumer should be subscribed to - Persistent, Non-Persistent, or both. Only used
		/// with pattern subscriptions.
		/// </summary>
		/// <param name="regexSubscriptionMode">
		///            Pattern subscription mode </param>
		IConsumerBuilder SubscriptionTopicsMode(RegexSubscriptionMode regexSubscriptionMode);

		/// <summary>
		/// Intercept <seealso cref="Consumer"/>.
		/// </summary>
		/// <param name="interceptors"> the list of interceptors to intercept the consumer created by this builder. </param>
		IConsumerBuilder Intercept(params IConsumerInterceptor [] interceptors);

		/// <summary>
		/// Set dead letter policy for consumer.
		/// 
		/// <para>By default some message will redelivery so many times possible, even to the extent that it can be never stop.
		/// By using dead letter mechanism messages will has the max redelivery count, when message exceeding the maximum
		/// number of redeliveries, message will send to the Dead Letter Topic and acknowledged automatic.
		/// 
		/// </para>
		/// <para>You can enable the dead letter mechanism by setting dead letter policy.
		/// example:
		/// <pre>
		/// client.newConsumer()
		///          .deadLetterPolicy(DeadLetterPolicy.builder().maxRedeliverCount(10).build())
		///          .subscribe();
		/// </pre>
		/// Default dead letter topic name is {TopicName}-{Subscription}-DLQ.
		/// To setting a custom dead letter topic name
		/// <pre>
		/// client.newConsumer()
		///          .deadLetterPolicy(DeadLetterPolicy
		///              .builder()
		///              .maxRedeliverCount(10)
		///              .deadLetterTopic("your-topic-name")
		///              .build())
		///          .subscribe();
		/// </pre>
		/// When a dead letter policy is specified, and no ackTimeoutMillis is specified,
		/// then the ack timeout will be set to 30000 millisecond.
		/// </para>
		/// </summary>
		IConsumerBuilder DeadLetterPolicy(DeadLetterPolicy deadLetterPolicy);

		/// <summary>
		/// If enabled, the consumer will auto subscribe for partitions increasement.
		/// This is only for partitioned consumer.
		/// </summary>
		/// <param name="autoUpdate">
		///            whether to auto update partition increasement </param>
		IConsumerBuilder AutoUpdatePartitions(bool autoUpdate);

		/// <summary>
		/// Set KeyShared subscription policy for consumer.
		/// 
		/// <para>By default, KeyShared subscription use auto split hash range to maintain consumers. If you want to
		/// set a different KeyShared policy, you can set by following example:
		/// 
		/// <pre>
		/// client.newConsumer()
		///          .keySharedPolicy(KeySharedPolicy.stickyHashRange().ranges(Range.of(0, 10)))
		///          .subscribe();
		/// </pre>
		/// Details about sticky hash range policy, please see <seealso cref="KeySharedPolicy.KeySharedPolicySticky"/>.
		/// 
		/// </para>
		/// <para>Or
		/// <pre>
		/// client.newConsumer()
		///          .keySharedPolicy(KeySharedPolicy.autoSplitHashRange().hashRangeTotal(100))
		///          .subscribe();
		/// </pre>
		/// Details about auto split hash range policy, please see <seealso cref="KeySharedPolicy.KeySharedPolicyAutoSplit"/>.
		/// 
		/// </para>
		/// </summary>
		/// <param name="keySharedPolicy"> The <seealso cref="KeySharedPolicy"/> want to specify </param>
		IConsumerBuilder KeySharedPolicy(KeySharedPolicy keySharedPolicy);

		/// <summary>
		/// Set the consumer to include the given position of any reset operation like {@link Consumer#seek(long) or
		/// <seealso cref="IConsumer.seek(IMessageId)"/>}.
		/// </summary>
		/// <returns> the consumer builder instance </returns>
		IConsumerBuilder StartMessageIdInclusive();

		/// <summary>
		/// Set batch receive policy <seealso cref="BatchReceivePolicy"/> for consumer.
		/// By default, consumer will use <seealso cref="BatchReceivePolicy.DEFAULT_POLICY"/> as batch receive policy.
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
		IConsumerBuilder BatchReceivePolicy(BatchReceivePolicy batchReceivePolicy);
	}

}