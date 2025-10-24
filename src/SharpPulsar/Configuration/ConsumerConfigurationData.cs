
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using SharpPulsar.Common.Precondition;
using System;
using SharpPulsar.API;
using SharpPulsar.Shared;
using System.Text.Json.Serialization;
using SharpPulsar.Crypto;
using SharpPulsar.Extension;
using SharpPulsar.TimeUnit;
using Pulsar.Proto;

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
namespace SharpPulsar.Configuration
{
    public sealed class ConsumerConfigurationData<T>
	{
		public void SetAutoUpdatePartitionsInterval(TimeSpan interval)
		{
			Condition.CheckArgument(interval.TotalMilliseconds > 0, "interval needs to be > 0");
            AutoUpdatePartitionsIntervalSeconds = TimeUnit.TimeUnit.SECONDS.ToSeconds(interval.Seconds);
		}
        public long AutoUpdatePartitionsIntervalSeconds { get; set; } = TimeUnit.TimeUnit.SECONDS.ToSeconds(60);
		public IMessageCrypto<MessageMetadata, MessageMetadata> MessageCrypto { get; set; }
		public IMessageId StartMessageId { get; set; }

        /// <summary>
        /// Group a consumer acknowledgment for the number of messages.
        /// </summary>
        public int MaxAcknowledgmentGroupSize { get; set; } = 1000;
        public ConsumptionType ConsumptionType { get; set; } = ConsumptionType.Listener;

        /// <summary>
        /// Topic name
        /// </summary>
		public ISet<string> TopicNames { get; set; } = new SortedSet<string>();
		public List<IConsumerInterceptor<T>> Interceptors { get; set; }
        public bool IsAutoScaledReceiverQueueSizeEnabled { get; set; }

        /// <summary>
        /// Interface for custom message is negativeAcked policy. You can specify `RedeliveryBackoff` for a
        /// consumer.
        /// </summary>
        public IRedeliveryBackoff NegativeAckRedeliveryBackoff { get; set; }    

        /// <summary>
        /// Interface for custom message is ackTimeout policy. You can specify `RedeliveryBackoff` for a
        /// consumer.
        /// </summary>
        public IRedeliveryBackoff AckTimeoutRedeliveryBackoff { get; set; } 

        /// <summary>
        /// Subscription type.\n"
        ///            + "Four subscription types are available:\n"
        ///            + "* Exclusive\n"
        ///            + "* Failover\n"
        ///            + "* Shared\n"
        ///            + "* Key_Shared
        /// </summary>
        public SubscriptionType SubscriptionType { get; set; } = SubscriptionType.Exclusive;
		internal IMessageListener<T> MessageListener { get; set; }
        public bool ForceTopicCreation { get; set; } = false;
		public IConsumerEventListener ConsumerEventListener { get; set; }
        public bool UseTls { get; set; } = false;

        // max pending chunked message to avoid sending incomplete message into the queue and memory
        public int MaxPendingChunkedMessage { get; set; } = 10;

        /// <summary>
        /// Size of a consumer's receiver queue.
        /// For example, the number of messages accumulated by a consumer before an application calls
        /// `Receive`
        /// A value higher than the default value increases consumer throughput, though at the expense of
        /// more memory utilization.
        /// </summary>
		public int ReceiverQueueSize { get; set; } = 1_000;

        /// <summary>
        /// Group a consumer acknowledgment for a specified time.
        /// By default, a consumer uses 100ms grouping time to send out acknowledgments to a broker.
        /// Setting a group time of 0 sends out acknowledgments immediately.
        /// A longer ack group time is more efficient at the expense of a slight increase in message "
        /// re-deliveries after a failure
        /// </summary>
		public long AcknowledgementsGroupTimeMicros {get; set; } = TimeUnit.TimeUnit.MILLISECONDS.ToMicroseconds(100);

        /// <summary>
        /// Delay to wait before redelivering messages that failed to be processed.
        /// When an application uses {@link Consumer#negativeAcknowledge(Message)}, failed messages are "
        /// redelivered after a fixed timeout.
        /// </summary>
		public long NegativeAckRedeliveryDelayMicros { get; set; } = TimeUnit.TimeUnit.MINUTES.ToMicroseconds(30000);

        /// <summary>
        /// The redelivery time precision bit count. The lower bits of the redelivery time will be
        /// trimmed to reduce the memory occupation.\nThe default value is 8, which means the"
        /// redelivery time will be bucketed by 256ms, the redelivery time could be earlier(no later)
        /// than the expected time, but no more than 256ms. \nIf set to k, the redelivery time will be
        /// bucketed by 2^k ms.\nIf the value is 0, the redelivery time will be accurate to ms.
        /// </summary>
        public int NegativeAckPrecisionBitCnt { get; set; } = 8;

        /// <summary>
        /// The max total receiver queue size across partitions.
        /// This setting reduces the receiver queue size for individual partitions if the total receiver
        /// queue size exceeds this value."
        /// </summary>
        public int MaxTotalReceiverQueueSizeAcrossPartitions { get; set; } = 50000;

        /// <summary>
        /// Timeout of unacked messages
        /// </summary>
		public long AckTimeoutMillis { get; set; } = 0;

        /// <summary>
        /// Granularity of the ack-timeout redelivery.
        /// Using an higher `tickDurationMillis` reduces the memory overhead to track messages when setting
        /// ack-timeout to a bigger value (for example, 1 hour).
        /// </summary>
        public long TickDurationMillis { get; set; } = 1000;
        public bool AckReceiptEnabled { get; set; } = false;
		public bool StartPaused { get; set; } = false;

        [JsonIgnore]
        public IMessagePayloadProcessor PayloadProcessor = null;

        /// <summary>
        /// Priority level for a consumer to which a broker gives more priority while dispatching messages
        /// in Shared subscription type.
        /// The broker follows descending priorities. For example, 0=max-priority, 1, 2,...
        /// In Shared subscription type, the broker **first dispatches messages to the max priority level
        /// consumers if they have permits**. Otherwise, the broker considers next priority level consumers
        /// **Example 1**
        /// If a subscription has consumerA with `priorityLevel` 0 and consumerB with `priorityLevel` 1,
        /// then the broker **only dispatches messages to consumerA until it runs out permits** and then
        /// starts dispatching messages to consumerB.
        /// **Example 2**
        /// Consumer Priority, Level, Permits
        /// C1, 0, 2
        /// C2, 0, 1
        /// C3, 0, 1
        /// C4, 1, 2
        /// C5, 1, 1
        /// Order in which a broker dispatches messages to consumers is: C1, C2, C3, C1, C4, C5, C4."
        /// </summary>
		public int PriorityLevel { get; set; } = 0;

        /// <summary>
        /// The time interval to expire incomplete chunks if a consumer fails to receive all the chunks in the
        /// specified time period. The default value is 1 minute.
        /// </summary>
        public long ExpireTimeOfIncompleteChunkedMessageMillis { get; set; } = TimeUnit.TimeUnit.MINUTES.ToMilliseconds(1);

        /// <summary>
        /// Whether to automatically acknowledge pending chunked messages when the threshold of
        /// `maxPendingChunkedMessage` is reached. If set to `false`, these messages will be redelivered
        /// by their broker.
        /// </summary>
        public bool AutoAckOldestChunkedMessageOnQueueFull { get; set; }

        public bool BatchConsume { get; set; } = false;
        public bool BatchIndexAckEnabled { get; set; } = true;
		public TimeSpan BatchConsumeTimeout { get; set; } = TimeSpan.FromMilliseconds(30_000); //30 seconds

		public ICryptoKeyReader CryptoKeyReader { get; set; }

        /// <summary>
        /// Consumer should take action when it receives a message that can not be decrypted.
        /// * **FAIL**: this is the default option to fail messages until crypto succeeds.
        /// * **DISCARD**:silently acknowledge and not deliver message to an application.
        /// * **CONSUME**: deliver encrypted messages to applications. It is the application's
        /// responsibility to decrypt the message.
        /// The decompression of message fails.
        /// If messages contain batch messages, a client is not be able to retrieve individual messages in
        /// batch.
        /// Delivered encrypted message contains {@link EncryptionContext} which contains encryption and
        /// compression information in it using which application can decrypt consumed message payload.
        /// </summary>
		public ConsumerCryptoFailureAction CryptoFailureAction { get; set; } = ConsumerCryptoFailureAction.FAIL;

        /// <summary>
        /// Topic auto discovery period when using a pattern for topic's consumer.
        /// The default value is 1 minute, with a minimum of 1 second
        /// </summary>
		public int PatternAutoDiscoveryPeriod { get; set; } = 60;

	    public SubscriptionMode SubscriptionMode = SubscriptionMode.Durable;

        public IDictionary<string, string> SubscriptionProperties;

        public IMessageListenerExecutor MessageListenerExecutor { get; set; }

        /// <summary>
        /// When subscribing to a topic using a regular expression, you can pick a certain type of topics.
        /// * **PersistentOnly**: only subscribe to persistent topics.
        /// * **NonPersistentOnly**: only subscribe to non-persistent topics.
        /// * **AllTopics**: subscribe to both persistent and non-persistent topics.
        /// </summary>
        public RegexSubscriptionMode RegexSubscriptionMode { get; set; } = RegexSubscriptionMode.PersistentOnly;


        public BatchReceivePolicy BatchReceivePolicy { get; set; } 

        /// <summary>
        /// If `autoUpdatePartitions` is enabled, a consumer subscribes to partition increasement
        /// automatically.
        /// **Note**: this is only for partitioned consumers.
        /// </summary>
		public bool AutoUpdatePartitions { get; set; } = true;

        /// <summary>
        /// If `replicateSubscriptionState` is enabled, a subscription state is replicated to geo-replicated
        /// clusters.
        /// </summary>
		public bool ReplicateSubscriptionState { get; set; } = false;
		public bool RetryEnable { get; set; } = false;

		public bool ResetIncludeHead { get; set; } = false;

        public KeySharedPolicy KeySharedPolicy { get; set; }

        /// <summary>
        /// If enabling `readCompacted`, a consumer reads messages from a compacted topic rather than reading "
        /// a full message backlog of a topic.
        /// A consumer only sees the latest value for each key in the compacted topic, up until reaching
        /// the point in the topic message when compacting backlog. Beyond that point, send messages as
        /// normal.
        /// Only enabling `readCompacted` on subscriptions to persistent topics, which have a single active
        /// consumer (like failure or exclusive subscriptions).
        /// Attempting to enable it on subscriptions to non-persistent topics or on shared subscriptions
        /// leads to a subscription call throwing a `PulsarClientException`.
        /// </summary>
		public bool ReadCompacted { get; set; } = false;

        /// <summary>
        /// Dead letter policy for consumers.
        /// By default, some messages are probably redelivered many times, even to the extent that it
        /// never stops.
        /// By using the dead letter mechanism, messages have the max redelivery count. **When exceeding the
        /// maximum number of redeliveries, messages are sent to the Dead Letter Topic and acknowledged
        /// automatically**.
        /// You can enable the dead letter mechanism by setting `deadLetterPolicy`.
        /// **Example**
        /// "```java
        /// client.newConsumer()
        /// .deadLetterPolicy(DeadLetterPolicy.builder().maxRedeliverCount(10).build())
        /// .subscribe();
        /// Default dead letter topic name is `{TopicName}-{Subscription}-DLQ`.
        /// To set a custom dead letter topic name:
        /// ```java
        /// client.newConsumer()
        /// .deadLetterPolicy(DeadLetterPolicy.builder().maxRedeliverCount(10)
        /// .deadLetterTopic(\"your-topic-name\").build())
        /// .subscribe();
        /// When specifying the dead letter policy while not specifying `ackTimeoutMillis`, you can set the
        /// ack timeout to 30000 millisecond.
        /// </summary>
		public DeadLetterPolicy DeadLetterPolicy { get; set; }

        /// <summary>
        /// Initial position at which to set cursor when subscribing to a topic at first time.
        /// </summary>
        public SubscriptionInitialPosition SubscriptionInitialPosition { get; set; } =
            SubscriptionInitialPosition.Earliest;

        /// <summary>
        /// The regexp for the topic name(not contains partition suffix).
        /// </summary>
		public Regex TopicsPattern { get; set; }

        /// <summary>
        /// A name or value property of this consumer.
        /// `properties` is application defined metadata attached to a consumer.
        /// When getting a topic stats, associate this metadata with the consumer stats for easier "
        /// identification.
        /// </summary>
		public SortedDictionary<string, string> Properties { get; set; } = new SortedDictionary<string, string>();

        /// <summary>
        /// Consumer name
        /// </summary>
		public string ConsumerName { get; set; }

        /// <summary>
        /// Subscription name
        /// </summary>
		public string SubscriptionName { get; set; }

        private IList<TopicConsumerConfigurationData> _topicConfigurations = new List<TopicConsumerConfigurationData>();

        public TopicConsumerConfigurationData GetMatchingTopicConfiguration(string topicName)
        {
            return _topicConfigurations.Where(topicConf => topicConf.GetTopicNameMatcher().Matches(topicName)).Count() > 0 
                ? _topicConfigurations.First() 
                : TopicConsumerConfigurationData.OfTopicName(topicName, this);
        }

        public void TopicConfigurations(List<TopicConsumerConfigurationData> topicConfigurations)
        {
            Condition.CheckArgument(topicConfigurations != null, "topicConfigurations should not be null.");
            _topicConfigurations.AddRange(topicConfigurations);
        }
        public void TopicConfigurations(TopicConsumerConfigurationData topicConfigurations)
        {
            Condition.CheckArgument(topicConfigurations != null, "topicConfigurations should not be null.");
            _topicConfigurations.Add(topicConfigurations);
        }
        public string SingleTopic
		{
			//get => TopicNames.Count == 1 ? TopicNames.First() : string.Empty;
            set => TopicNames = new HashSet<string> {value};
            get
            {
                Condition.CheckArgument(TopicNames.Count == 1, "topicNames needs to be = 1");
                return TopicNames.First();
            }

        }

    }

    public enum ConsumptionType
    {
		Queue,
		Listener
    }
}