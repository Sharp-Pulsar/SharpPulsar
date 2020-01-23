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
namespace SharpPulsar.Impl
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.google.common.@base.Preconditions.checkArgument;


	using AccessLevel = lombok.AccessLevel;
	using Getter = lombok.Getter;
	using StringUtils = org.apache.commons.lang3.StringUtils;
	using BatchReceivePolicy = SharpPulsar.Api.BatchReceivePolicy;
	using Consumer = SharpPulsar.Api.Consumer;
	using SharpPulsar.Api;
	using ConsumerCryptoFailureAction = SharpPulsar.Api.ConsumerCryptoFailureAction;
	using ConsumerEventListener = SharpPulsar.Api.ConsumerEventListener;
	using SharpPulsar.Api;
	using CryptoKeyReader = SharpPulsar.Api.CryptoKeyReader;
	using DeadLetterPolicy = SharpPulsar.Api.DeadLetterPolicy;
	using KeySharedPolicy = SharpPulsar.Api.KeySharedPolicy;
	using SharpPulsar.Api;
	using PulsarClientException = SharpPulsar.Api.PulsarClientException;
	using RegexSubscriptionMode = SharpPulsar.Api.RegexSubscriptionMode;
	using InvalidConfigurationException = SharpPulsar.Api.PulsarClientException.InvalidConfigurationException;
	using SharpPulsar.Api;
	using SubscriptionInitialPosition = SharpPulsar.Api.SubscriptionInitialPosition;
	using SubscriptionType = SharpPulsar.Api.SubscriptionType;
	using ConfigurationDataUtils = SharpPulsar.Impl.Conf.ConfigurationDataUtils;
	using SharpPulsar.Impl.Conf;
	using FutureUtil = Org.Apache.Pulsar.Common.Util.FutureUtil;

	using Lists = com.google.common.collect.Lists;
	using NonNull = lombok.NonNull;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Getter(AccessLevel.PUBLIC) public class ConsumerBuilderImpl<T> implements SharpPulsar.api.ConsumerBuilder<T>
	public class ConsumerBuilderImpl<T> : ConsumerBuilder<T>
	{

		private readonly PulsarClientImpl client;
		private ConsumerConfigurationData<T> conf;
		private readonly Schema<T> schema;
		private IList<ConsumerInterceptor<T>> interceptorList;

		private static long MIN_ACK_TIMEOUT_MILLIS = 1000;
		private static long MIN_TICK_TIME_MILLIS = 100;
		private static long DEFAULT_ACK_TIMEOUT_MILLIS_FOR_DEAD_LETTER = 30000L;


		public ConsumerBuilderImpl(PulsarClientImpl Client, Schema<T> Schema) : this(Client, new ConsumerConfigurationData<T>(), Schema)
		{
		}

		public ConsumerBuilderImpl(PulsarClientImpl Client, ConsumerConfigurationData<T> Conf, Schema<T> Schema)
		{
			this.client = Client;
			this.conf = Conf;
			this.schema = Schema;
		}

		public override ConsumerBuilder<T> LoadConf(IDictionary<string, object> Config)
		{
			this.conf = ConfigurationDataUtils.loadData(Config, conf, typeof(ConsumerConfigurationData));
			return this;
		}

		public override ConsumerBuilder<T> Clone()
		{
			return new ConsumerBuilderImpl<T>(client, conf.Clone(), schema);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public SharpPulsar.api.Consumer<T> subscribe() throws SharpPulsar.api.PulsarClientException
		public override Consumer<T> Subscribe()
		{
			try
			{
				return SubscribeAsync().get();
			}
			catch (Exception E)
			{
				throw PulsarClientException.unwrap(E);
			}
		}

		public override CompletableFuture<Consumer<T>> SubscribeAsync()
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

			return interceptorList == null || interceptorList.Count == 0 ? client.SubscribeAsync(conf, schema, null) : client.SubscribeAsync(conf, schema, new ConsumerInterceptors<>(interceptorList));
		}

		public override ConsumerBuilder<T> Topic(params string[] TopicNames)
		{
			checkArgument(TopicNames != null && TopicNames.Length > 0, "Passed in topicNames should not be null or empty.");
			java.util.topicNames.ForEach(topicName => checkArgument(StringUtils.isNotBlank(topicName), "topicNames cannot have blank topic"));
			conf.TopicNames.addAll(Lists.newArrayList(java.util.topicNames.Select(StringUtils.Trim).ToList()));
			return this;
		}

		public override ConsumerBuilder<T> Topics(IList<string> TopicNames)
		{
			checkArgument(TopicNames != null && TopicNames.Count > 0, "Passed in topicNames list should not be null or empty.");
			TopicNames.ForEach(topicName => checkArgument(StringUtils.isNotBlank(topicName), "topicNames cannot have blank topic"));
			conf.TopicNames.addAll(TopicNames.Select(StringUtils.Trim).ToList());
			return this;
		}

		public override ConsumerBuilder<T> TopicsPattern(Pattern TopicsPattern)
		{
			checkArgument(conf.TopicsPattern == null, "Pattern has already been set.");
			conf.TopicsPattern = TopicsPattern;
			return this;
		}

		public override ConsumerBuilder<T> TopicsPattern(string TopicsPattern)
		{
			checkArgument(conf.TopicsPattern == null, "Pattern has already been set.");
			conf.TopicsPattern = Pattern.compile(TopicsPattern);
			return this;
		}

		public override ConsumerBuilder<T> SubscriptionName(string SubscriptionName)
		{
			checkArgument(StringUtils.isNotBlank(SubscriptionName), "subscriptionName cannot be blank");
			conf.SubscriptionName = SubscriptionName;
			return this;
		}

		public override ConsumerBuilder<T> AckTimeout(long AckTimeout, BAMCIS.Util.Concurrent.TimeUnit BAMCIS.Util.Concurrent.TimeUnit)
		{
			checkArgument(AckTimeout == 0 || BAMCIS.Util.Concurrent.TimeUnit.toMillis(AckTimeout) >= MIN_ACK_TIMEOUT_MILLIS, "Ack timeout should be greater than " + MIN_ACK_TIMEOUT_MILLIS + " ms");
			conf.AckTimeoutMillis = BAMCIS.Util.Concurrent.TimeUnit.toMillis(AckTimeout);
			return this;
		}

		public override ConsumerBuilder<T> AckTimeoutTickTime(long TickTime, BAMCIS.Util.Concurrent.TimeUnit BAMCIS.Util.Concurrent.TimeUnit)
		{
			checkArgument(BAMCIS.Util.Concurrent.TimeUnit.toMillis(TickTime) >= MIN_TICK_TIME_MILLIS, "Ack timeout tick time should be greater than " + MIN_TICK_TIME_MILLIS + " ms");
			conf.TickDurationMillis = BAMCIS.Util.Concurrent.TimeUnit.toMillis(TickTime);
			return this;
		}

		public override ConsumerBuilder<T> NegativeAckRedeliveryDelay(long RedeliveryDelay, BAMCIS.Util.Concurrent.TimeUnit BAMCIS.Util.Concurrent.TimeUnit)
		{
			checkArgument(RedeliveryDelay >= 0, "redeliveryDelay needs to be >= 0");
			conf.NegativeAckRedeliveryDelayMicros = BAMCIS.Util.Concurrent.TimeUnit.toMicros(RedeliveryDelay);
			return this;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override public SharpPulsar.api.ConsumerBuilder<T> subscriptionType(@NonNull SubscriptionType subscriptionType)
		public override ConsumerBuilder<T> SubscriptionType(SubscriptionType SubscriptionType)
		{
			conf.SubscriptionType = SubscriptionType;
			return this;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override public SharpPulsar.api.ConsumerBuilder<T> messageListener(@NonNull MessageListener<T> messageListener)
		public override ConsumerBuilder<T> MessageListener(MessageListener<T> MessageListener)
		{
			conf.MessageListener = MessageListener;
			return this;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override public SharpPulsar.api.ConsumerBuilder<T> consumerEventListener(@NonNull ConsumerEventListener consumerEventListener)
		public override ConsumerBuilder<T> ConsumerEventListener(ConsumerEventListener ConsumerEventListener)
		{
			conf.ConsumerEventListener = ConsumerEventListener;
			return this;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override public SharpPulsar.api.ConsumerBuilder<T> cryptoKeyReader(@NonNull CryptoKeyReader cryptoKeyReader)
		public override ConsumerBuilder<T> CryptoKeyReader(CryptoKeyReader CryptoKeyReader)
		{
			conf.CryptoKeyReader = CryptoKeyReader;
			return this;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override public SharpPulsar.api.ConsumerBuilder<T> cryptoFailureAction(@NonNull ConsumerCryptoFailureAction action)
		public override ConsumerBuilder<T> CryptoFailureAction(ConsumerCryptoFailureAction Action)
		{
			conf.CryptoFailureAction = Action;
			return this;
		}

		public override ConsumerBuilder<T> ReceiverQueueSize(int ReceiverQueueSize)
		{
			checkArgument(ReceiverQueueSize >= 0, "receiverQueueSize needs to be >= 0");
			conf.ReceiverQueueSize = ReceiverQueueSize;
			return this;
		}

		public override ConsumerBuilder<T> AcknowledgmentGroupTime(long Delay, BAMCIS.Util.Concurrent.TimeUnit Unit)
		{
			checkArgument(Delay >= 0, "acknowledgmentGroupTime needs to be >= 0");
			conf.AcknowledgementsGroupTimeMicros = Unit.toMicros(Delay);
			return this;
		}

		public override ConsumerBuilder<T> ConsumerName(string ConsumerName)
		{
			checkArgument(StringUtils.isNotBlank(ConsumerName), "consumerName cannot be blank");
			conf.ConsumerName = ConsumerName;
			return this;
		}

		public override ConsumerBuilder<T> PriorityLevel(int PriorityLevel)
		{
			checkArgument(PriorityLevel >= 0, "priorityLevel needs to be >= 0");
			conf.PriorityLevel = PriorityLevel;
			return this;
		}

		public override ConsumerBuilder<T> Property(string Key, string Value)
		{
			checkArgument(StringUtils.isNotBlank(Key) && StringUtils.isNotBlank(Value), "property key/value cannot be blank");
			conf.Properties.put(Key, Value);
			return this;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override public SharpPulsar.api.ConsumerBuilder<T> properties(@NonNull Map<String, String> properties)
		public override ConsumerBuilder<T> Properties(IDictionary<string, string> Properties)
		{
			checkArgument(Properties.Count > 0, "properties cannot be empty");
			Properties.SetOfKeyValuePairs().forEach(entry => checkArgument(StringUtils.isNotBlank(entry.Key) && StringUtils.isNotBlank(entry.Value), "properties' key/value cannot be blank"));
			conf.Properties.putAll(Properties);
			return this;
		}

		public override ConsumerBuilder<T> MaxTotalReceiverQueueSizeAcrossPartitions(int MaxTotalReceiverQueueSizeAcrossPartitions)
		{
			checkArgument(MaxTotalReceiverQueueSizeAcrossPartitions >= 0, "maxTotalReceiverQueueSizeAcrossPartitions needs to be >= 0");
			conf.MaxTotalReceiverQueueSizeAcrossPartitions = MaxTotalReceiverQueueSizeAcrossPartitions;
			return this;
		}

		public override ConsumerBuilder<T> ReadCompacted(bool ReadCompacted)
		{
			conf.ReadCompacted = ReadCompacted;
			return this;
		}

		public override ConsumerBuilder<T> PatternAutoDiscoveryPeriod(int PeriodInMinutes)
		{
			checkArgument(PeriodInMinutes >= 0, "periodInMinutes needs to be >= 0");
			conf.PatternAutoDiscoveryPeriod = PeriodInMinutes;
			return this;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override public SharpPulsar.api.ConsumerBuilder<T> subscriptionInitialPosition(@NonNull SubscriptionInitialPosition subscriptionInitialPosition)
		public override ConsumerBuilder<T> SubscriptionInitialPosition(SubscriptionInitialPosition SubscriptionInitialPosition)
		{
			conf.SubscriptionInitialPosition = SubscriptionInitialPosition;
			return this;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override public SharpPulsar.api.ConsumerBuilder<T> subscriptionTopicsMode(@NonNull RegexSubscriptionMode mode)
		public override ConsumerBuilder<T> SubscriptionTopicsMode(RegexSubscriptionMode Mode)
		{
			conf.RegexSubscriptionMode = Mode;
			return this;
		}

		public override ConsumerBuilder<T> ReplicateSubscriptionState(bool ReplicateSubscriptionState)
		{
			conf.ReplicateSubscriptionState = ReplicateSubscriptionState;
			return this;
		}

		public override ConsumerBuilder<T> Intercept(params ConsumerInterceptor<T>[] Interceptors)
		{
			if (interceptorList == null)
			{
				interceptorList = new List<ConsumerInterceptor<T>>();
			}
			((List<ConsumerInterceptor<T>>)interceptorList).AddRange(Arrays.asList(Interceptors));
			return this;
		}

		public override ConsumerBuilder<T> DeadLetterPolicy(DeadLetterPolicy DeadLetterPolicy)
		{
			if (DeadLetterPolicy != null)
			{
				if (conf.AckTimeoutMillis == 0)
				{
					conf.AckTimeoutMillis = DEFAULT_ACK_TIMEOUT_MILLIS_FOR_DEAD_LETTER;
				}
				conf.DeadLetterPolicy = DeadLetterPolicy;
			}
			return this;
		}

		public override ConsumerBuilder<T> AutoUpdatePartitions(bool AutoUpdate)
		{
			conf.AutoUpdatePartitions = AutoUpdate;
			return this;
		}

		public override ConsumerBuilder<T> StartMessageIdInclusive()
		{
			conf.ResetIncludeHead = true;
			return this;
		}

		public virtual ConsumerBuilder<T> BatchReceivePolicy(BatchReceivePolicy BatchReceivePolicy)
		{
			checkArgument(BatchReceivePolicy != null, "batchReceivePolicy must not be null.");
			BatchReceivePolicy.verify();
			conf.BatchReceivePolicy = BatchReceivePolicy;
			return this;
		}

		public override string ToString()
		{
			return conf != null ? conf.ToString() : null;
		}

		public override ConsumerBuilder<T> KeySharedPolicy(KeySharedPolicy KeySharedPolicy)
		{
			KeySharedPolicy.validate();
			conf.KeySharedPolicy = KeySharedPolicy;
			return this;
		}
	}

}