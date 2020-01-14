using System;
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
namespace org.apache.pulsar.client.impl
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.google.common.@base.Preconditions.checkArgument;


	using AccessLevel = lombok.AccessLevel;
	using Getter = lombok.Getter;
	using StringUtils = org.apache.commons.lang3.StringUtils;
	using BatchReceivePolicy = org.apache.pulsar.client.api.BatchReceivePolicy;
	using Consumer = org.apache.pulsar.client.api.Consumer;
	using ConsumerBuilder = org.apache.pulsar.client.api.ConsumerBuilder;
	using ConsumerCryptoFailureAction = org.apache.pulsar.client.api.ConsumerCryptoFailureAction;
	using ConsumerEventListener = org.apache.pulsar.client.api.ConsumerEventListener;
	using ConsumerInterceptor = org.apache.pulsar.client.api.ConsumerInterceptor;
	using CryptoKeyReader = org.apache.pulsar.client.api.CryptoKeyReader;
	using DeadLetterPolicy = org.apache.pulsar.client.api.DeadLetterPolicy;
	using KeySharedPolicy = org.apache.pulsar.client.api.KeySharedPolicy;
	using MessageListener = org.apache.pulsar.client.api.MessageListener;
	using PulsarClientException = org.apache.pulsar.client.api.PulsarClientException;
	using RegexSubscriptionMode = org.apache.pulsar.client.api.RegexSubscriptionMode;
	using InvalidConfigurationException = org.apache.pulsar.client.api.PulsarClientException.InvalidConfigurationException;
	using Schema = org.apache.pulsar.client.api.Schema;
	using SubscriptionInitialPosition = org.apache.pulsar.client.api.SubscriptionInitialPosition;
	using SubscriptionType = org.apache.pulsar.client.api.SubscriptionType;
	using ConfigurationDataUtils = org.apache.pulsar.client.impl.conf.ConfigurationDataUtils;
	using org.apache.pulsar.client.impl.conf;
	using FutureUtil = org.apache.pulsar.common.util.FutureUtil;

	using Lists = com.google.common.collect.Lists;
	using NonNull = lombok.NonNull;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Getter(AccessLevel.PUBLIC) public class ConsumerBuilderImpl<T> implements org.apache.pulsar.client.api.ConsumerBuilder<T>
	public class ConsumerBuilderImpl<T> : ConsumerBuilder<T>
	{

		private readonly PulsarClientImpl client;
		private ConsumerConfigurationData<T> conf;
		private readonly Schema<T> schema;
		private IList<ConsumerInterceptor<T>> interceptorList;

		private static long MIN_ACK_TIMEOUT_MILLIS = 1000;
		private static long MIN_TICK_TIME_MILLIS = 100;
		private static long DEFAULT_ACK_TIMEOUT_MILLIS_FOR_DEAD_LETTER = 30000L;


		public ConsumerBuilderImpl(PulsarClientImpl client, Schema<T> schema) : this(client, new ConsumerConfigurationData<T>(), schema)
		{
		}

		internal ConsumerBuilderImpl(PulsarClientImpl client, ConsumerConfigurationData<T> conf, Schema<T> schema)
		{
			this.client = client;
			this.conf = conf;
			this.schema = schema;
		}

		public override ConsumerBuilder<T> loadConf(IDictionary<string, object> config)
		{
			this.conf = ConfigurationDataUtils.loadData(config, conf, typeof(ConsumerConfigurationData));
			return this;
		}

		public override ConsumerBuilder<T> clone()
		{
			return new ConsumerBuilderImpl<T>(client, conf.clone(), schema);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public org.apache.pulsar.client.api.Consumer<T> subscribe() throws org.apache.pulsar.client.api.PulsarClientException
		public override Consumer<T> subscribe()
		{
			try
			{
				return subscribeAsync().get();
			}
			catch (Exception e)
			{
				throw PulsarClientException.unwrap(e);
			}
		}

		public override CompletableFuture<Consumer<T>> subscribeAsync()
		{
			if (conf.TopicNames.Empty && conf.TopicsPattern == null)
			{
				return FutureUtil.failedFuture(new PulsarClientException.InvalidConfigurationException("Topic name must be set on the consumer builder"));
			}

			if (StringUtils.isBlank(conf.SubscriptionName))
			{
				return FutureUtil.failedFuture(new PulsarClientException.InvalidConfigurationException("Subscription name must be set on the consumer builder"));
			}

			if (conf.KeySharedPolicy != null && conf.SubscriptionType != SubscriptionType.Key_Shared)
			{
				return FutureUtil.failedFuture(new PulsarClientException.InvalidConfigurationException("KeySharedPolicy must set with KeyShared subscription"));
			}

			return interceptorList == null || interceptorList.Count == 0 ? client.subscribeAsync(conf, schema, null) : client.subscribeAsync(conf, schema, new ConsumerInterceptors<>(interceptorList));
		}

		public override ConsumerBuilder<T> topic(params string[] topicNames)
		{
			checkArgument(topicNames != null && topicNames.Length > 0, "Passed in topicNames should not be null or empty.");
			java.util.topicNames.ForEach(topicName => checkArgument(StringUtils.isNotBlank(topicName), "topicNames cannot have blank topic"));
			conf.TopicNames.addAll(Lists.newArrayList(java.util.topicNames.Select(StringUtils.Trim).ToList()));
			return this;
		}

		public override ConsumerBuilder<T> topics(IList<string> topicNames)
		{
			checkArgument(topicNames != null && topicNames.Count > 0, "Passed in topicNames list should not be null or empty.");
			topicNames.ForEach(topicName => checkArgument(StringUtils.isNotBlank(topicName), "topicNames cannot have blank topic"));
			conf.TopicNames.addAll(topicNames.Select(StringUtils.Trim).ToList());
			return this;
		}

		public override ConsumerBuilder<T> topicsPattern(Pattern topicsPattern)
		{
			checkArgument(conf.TopicsPattern == null, "Pattern has already been set.");
			conf.TopicsPattern = topicsPattern;
			return this;
		}

		public override ConsumerBuilder<T> topicsPattern(string topicsPattern)
		{
			checkArgument(conf.TopicsPattern == null, "Pattern has already been set.");
			conf.TopicsPattern = Pattern.compile(topicsPattern);
			return this;
		}

		public override ConsumerBuilder<T> subscriptionName(string subscriptionName)
		{
			checkArgument(StringUtils.isNotBlank(subscriptionName), "subscriptionName cannot be blank");
			conf.SubscriptionName = subscriptionName;
			return this;
		}

		public override ConsumerBuilder<T> ackTimeout(long ackTimeout, TimeUnit timeUnit)
		{
			checkArgument(ackTimeout == 0 || timeUnit.toMillis(ackTimeout) >= MIN_ACK_TIMEOUT_MILLIS, "Ack timeout should be greater than " + MIN_ACK_TIMEOUT_MILLIS + " ms");
			conf.AckTimeoutMillis = timeUnit.toMillis(ackTimeout);
			return this;
		}

		public override ConsumerBuilder<T> ackTimeoutTickTime(long tickTime, TimeUnit timeUnit)
		{
			checkArgument(timeUnit.toMillis(tickTime) >= MIN_TICK_TIME_MILLIS, "Ack timeout tick time should be greater than " + MIN_TICK_TIME_MILLIS + " ms");
			conf.TickDurationMillis = timeUnit.toMillis(tickTime);
			return this;
		}

		public override ConsumerBuilder<T> negativeAckRedeliveryDelay(long redeliveryDelay, TimeUnit timeUnit)
		{
			checkArgument(redeliveryDelay >= 0, "redeliveryDelay needs to be >= 0");
			conf.NegativeAckRedeliveryDelayMicros = timeUnit.toMicros(redeliveryDelay);
			return this;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override public org.apache.pulsar.client.api.ConsumerBuilder<T> subscriptionType(@NonNull SubscriptionType subscriptionType)
		public override ConsumerBuilder<T> subscriptionType(SubscriptionType subscriptionType)
		{
			conf.SubscriptionType = subscriptionType;
			return this;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override public org.apache.pulsar.client.api.ConsumerBuilder<T> messageListener(@NonNull MessageListener<T> messageListener)
		public override ConsumerBuilder<T> messageListener(MessageListener<T> messageListener)
		{
			conf.MessageListener = messageListener;
			return this;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override public org.apache.pulsar.client.api.ConsumerBuilder<T> consumerEventListener(@NonNull ConsumerEventListener consumerEventListener)
		public override ConsumerBuilder<T> consumerEventListener(ConsumerEventListener consumerEventListener)
		{
			conf.ConsumerEventListener = consumerEventListener;
			return this;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override public org.apache.pulsar.client.api.ConsumerBuilder<T> cryptoKeyReader(@NonNull CryptoKeyReader cryptoKeyReader)
		public override ConsumerBuilder<T> cryptoKeyReader(CryptoKeyReader cryptoKeyReader)
		{
			conf.CryptoKeyReader = cryptoKeyReader;
			return this;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override public org.apache.pulsar.client.api.ConsumerBuilder<T> cryptoFailureAction(@NonNull ConsumerCryptoFailureAction action)
		public override ConsumerBuilder<T> cryptoFailureAction(ConsumerCryptoFailureAction action)
		{
			conf.CryptoFailureAction = action;
			return this;
		}

		public override ConsumerBuilder<T> receiverQueueSize(int receiverQueueSize)
		{
			checkArgument(receiverQueueSize >= 0, "receiverQueueSize needs to be >= 0");
			conf.ReceiverQueueSize = receiverQueueSize;
			return this;
		}

		public override ConsumerBuilder<T> acknowledgmentGroupTime(long delay, TimeUnit unit)
		{
			checkArgument(delay >= 0, "acknowledgmentGroupTime needs to be >= 0");
			conf.AcknowledgementsGroupTimeMicros = unit.toMicros(delay);
			return this;
		}

		public override ConsumerBuilder<T> consumerName(string consumerName)
		{
			checkArgument(StringUtils.isNotBlank(consumerName), "consumerName cannot be blank");
			conf.ConsumerName = consumerName;
			return this;
		}

		public override ConsumerBuilder<T> priorityLevel(int priorityLevel)
		{
			checkArgument(priorityLevel >= 0, "priorityLevel needs to be >= 0");
			conf.PriorityLevel = priorityLevel;
			return this;
		}

		public override ConsumerBuilder<T> property(string key, string value)
		{
			checkArgument(StringUtils.isNotBlank(key) && StringUtils.isNotBlank(value), "property key/value cannot be blank");
			conf.Properties.put(key, value);
			return this;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override public org.apache.pulsar.client.api.ConsumerBuilder<T> properties(@NonNull Map<String, String> properties)
		public override ConsumerBuilder<T> properties(IDictionary<string, string> properties)
		{
			checkArgument(!properties.Empty, "properties cannot be empty");
			properties.entrySet().forEach(entry => checkArgument(StringUtils.isNotBlank(entry.Key) && StringUtils.isNotBlank(entry.Value), "properties' key/value cannot be blank"));
			conf.Properties.putAll(properties);
			return this;
		}

		public override ConsumerBuilder<T> maxTotalReceiverQueueSizeAcrossPartitions(int maxTotalReceiverQueueSizeAcrossPartitions)
		{
			checkArgument(maxTotalReceiverQueueSizeAcrossPartitions >= 0, "maxTotalReceiverQueueSizeAcrossPartitions needs to be >= 0");
			conf.MaxTotalReceiverQueueSizeAcrossPartitions = maxTotalReceiverQueueSizeAcrossPartitions;
			return this;
		}

		public override ConsumerBuilder<T> readCompacted(bool readCompacted)
		{
			conf.ReadCompacted = readCompacted;
			return this;
		}

		public override ConsumerBuilder<T> patternAutoDiscoveryPeriod(int periodInMinutes)
		{
			checkArgument(periodInMinutes >= 0, "periodInMinutes needs to be >= 0");
			conf.PatternAutoDiscoveryPeriod = periodInMinutes;
			return this;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override public org.apache.pulsar.client.api.ConsumerBuilder<T> subscriptionInitialPosition(@NonNull SubscriptionInitialPosition subscriptionInitialPosition)
		public override ConsumerBuilder<T> subscriptionInitialPosition(SubscriptionInitialPosition subscriptionInitialPosition)
		{
			conf.SubscriptionInitialPosition = subscriptionInitialPosition;
			return this;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override public org.apache.pulsar.client.api.ConsumerBuilder<T> subscriptionTopicsMode(@NonNull RegexSubscriptionMode mode)
		public override ConsumerBuilder<T> subscriptionTopicsMode(RegexSubscriptionMode mode)
		{
			conf.RegexSubscriptionMode = mode;
			return this;
		}

		public override ConsumerBuilder<T> replicateSubscriptionState(bool replicateSubscriptionState)
		{
			conf.ReplicateSubscriptionState = replicateSubscriptionState;
			return this;
		}

		public override ConsumerBuilder<T> intercept(params ConsumerInterceptor<T>[] interceptors)
		{
			if (interceptorList == null)
			{
				interceptorList = new List<ConsumerInterceptor<T>>();
			}
			((List<ConsumerInterceptor<T>>)interceptorList).AddRange(Arrays.asList(interceptors));
			return this;
		}

		public override ConsumerBuilder<T> deadLetterPolicy(DeadLetterPolicy deadLetterPolicy)
		{
			if (deadLetterPolicy != null)
			{
				if (conf.AckTimeoutMillis == 0)
				{
					conf.AckTimeoutMillis = DEFAULT_ACK_TIMEOUT_MILLIS_FOR_DEAD_LETTER;
				}
				conf.DeadLetterPolicy = deadLetterPolicy;
			}
			return this;
		}

		public override ConsumerBuilder<T> autoUpdatePartitions(bool autoUpdate)
		{
			conf.AutoUpdatePartitions = autoUpdate;
			return this;
		}

		public override ConsumerBuilder<T> startMessageIdInclusive()
		{
			conf.ResetIncludeHead = true;
			return this;
		}

		public virtual ConsumerBuilder<T> batchReceivePolicy(BatchReceivePolicy batchReceivePolicy)
		{
			checkArgument(batchReceivePolicy != null, "batchReceivePolicy must not be null.");
			batchReceivePolicy.verify();
			conf.BatchReceivePolicy = batchReceivePolicy;
			return this;
		}

		public override string ToString()
		{
			return conf != null ? conf.ToString() : null;
		}

		public override ConsumerBuilder<T> keySharedPolicy(KeySharedPolicy keySharedPolicy)
		{
			keySharedPolicy.validate();
			conf.KeySharedPolicy = keySharedPolicy;
			return this;
		}
	}

}