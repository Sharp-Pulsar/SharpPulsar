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
    using SharpPulsar.Interface.Consumer;
    using SharpPulsar.Configuration;
    using SharpPulsar.Interface.Schema;
    using SharpPulsar.Interface.Interceptor;
    using SharpPulsar.Exception;
    using System.Threading.Tasks;
    using System.Linq;
    using SharpPulsar.Enum;
    using System.Text.RegularExpressions;
    using SharpPulsar.Util;
    using SharpPulsar.Interface.Message;
    using SharpPulsar.Interface;
    using SharpPulsar.Entity;

    public class ConsumerBuilderImpl<T> : IConsumerBuilder<T>
	{

		private readonly PulsarClientImpl client;
		private ConsumerConfigurationData<T> conf;
		private readonly ISchema<T> schema;
		private IList<Disposeasync<T>> interceptorList;

		private static long MIN_ACK_TIMEOUT_MILLIS = 1000;
		private static long MIN_TICK_TIME_MILLIS = 100;
		private static long DEFAULT_ACK_TIMEOUT_MILLIS_FOR_DEAD_LETTER = 30000L;


		public ConsumerBuilderImpl(PulsarClientImpl client, ISchema<T> schema) : this(client, new ConsumerConfigurationData<T>(), schema)
		{
		}

		internal ConsumerBuilderImpl(PulsarClientImpl client, ConsumerConfigurationData<T> conf, ISchema<T> schema)
		{
			this.client = client;
			this.conf = conf;
			this.schema = schema;
		}

		public IConsumerBuilder<T> LoadConf(IDictionary<string, object> config)
		{
			conf = ConfigurationDataUtils.LoadData(config, conf, typeof(ConsumerConfigurationData<T>));
			return this;
		}

		public IConsumerBuilder<T> Clone()
		{
			return new ConsumerBuilderImpl<T>(client, conf, schema);
		}
		public IConsumer<T> Subscribe()
		{
			try
			{
				return SubscribeAsync().GetAwaiter().GetResult();
			}
			catch (System.Exception e)
			{
				throw PulsarClientException.Unwrap(e);
			}
		}

		public async ValueTask<IConsumer<T>> SubscribeAsync()
		{
			if (!conf.TopicNames.Any() && conf.TopicsPattern == null)
			{
				throw new PulsarClientException.InvalidConfigurationException("Topic name must be set on the consumer builder");
			}

			if (string.IsNullOrWhiteSpace(conf.SubscriptionName))
			{
				throw new PulsarClientException.InvalidConfigurationException("Subscription name must be set on the consumer builder");
			}

			if (conf.KeySharedPolicy != null && conf.SubscriptionType != Enum.SubscriptionType.Key_Shared)
			{
				throw new PulsarClientException.InvalidConfigurationException("KeySharedPolicy must set with KeyShared subscription");
			}

			return interceptorList == null || interceptorList.Count == 0 ? await client.SubscribeAsync(conf, schema, null) : await client.SubscribeAsync(conf, schema, new ConsumerInterceptors<T>(interceptorList));
		}

		public IConsumerBuilder<T> Topic(params string[] topicNames)
		{
			if(topicNames != null && topicNames.Length > 0)
				throw new NullReferenceException("Passed in topicNames should not be null or empty.");
			foreach(var topic in topicNames)
			{
				if(string.IsNullOrWhiteSpace(topic))
				{
					new NullReferenceException("topicNames cannot have blank topic");
				}
			}
			foreach(var topic in topicNames)
			{
				conf.TopicNames.Add(topic.Trim());
			}
			return this;
		}

		public IConsumerBuilder<T> Topics(IList<string> topicNames)
		{
			if (topicNames != null && topicNames.Count > 0)
				throw new NullReferenceException("Passed in topicNames list should not be null or empty.");
			foreach (var topic in topicNames)
			{
				if (string.IsNullOrWhiteSpace(topic))
				{
					new NullReferenceException("topicNames cannot have blank topic");
				}
			}
			foreach (var topic in topicNames)
			{
				conf.TopicNames.Add(topic.Trim());
			}
			return this;
		}

		public IConsumerBuilder<T> TopicsPattern(Regex topicsPattern)
		{
			if (conf.TopicsPattern != null)
				throw new System.Exception("Pattern has already been set.");
			conf.TopicsPattern = topicsPattern;
			return this;
		}

		public IConsumerBuilder<T> TopicsPattern(string topicsPattern)
		{
			if (conf.TopicsPattern != null)
				throw new System.Exception("Pattern has already been set.");
			conf.TopicsPattern = new Regex(topicsPattern);
			return this;
		}

		public IConsumerBuilder<T> SubscriptionName(string subscriptionName)
		{
			if(string.IsNullOrWhiteSpace(subscriptionName))
				throw new System.Exception("subscriptionName cannot be blank");
			conf.SubscriptionName = subscriptionName;
			return this;
		}

		public IConsumerBuilder<T> AckTimeout(long ackTimeout, BAMCIS.Util.Concurrent.TimeUnit timeUnit)
		{
			if(timeUnit.ToMillis(ackTimeout) < MIN_ACK_TIMEOUT_MILLIS)
				throw new System.Exception("Ack timeout should be greater than " + MIN_ACK_TIMEOUT_MILLIS + " ms"); 
			conf.AckTimeoutMillis = timeUnit.ToMillis(ackTimeout);
			return this;
		}

		public IConsumerBuilder<T> AckTimeoutTickTime(long tickTime, BAMCIS.Util.Concurrent.TimeUnit timeUnit)
		{
			if(timeUnit.ToMillis(tickTime) <= MIN_TICK_TIME_MILLIS)
				throw new System.Exception("Ack timeout tick time should be greater than " + MIN_TICK_TIME_MILLIS + " ms");
			conf.TickDurationMillis = timeUnit.ToMillis(tickTime);
			return this;
		}

		public IConsumerBuilder<T> NegativeAckRedeliveryDelay(long redeliveryDelay, BAMCIS.Util.Concurrent.TimeUnit timeUnit)
		{
			if(redeliveryDelay < 0)
				throw new System.Exception("redeliveryDelay needs to be >= 0");
			conf.NegativeAckRedeliveryDelayMicros = timeUnit.ToMicros(redeliveryDelay);
			return this;
		}

		public IConsumerBuilder<T> SubscriptionType(SubscriptionType subscriptionType)
		{
			conf.SubscriptionType = subscriptionType;
			return this;
		}

		public IConsumerBuilder<T> MessageListener(IMessageListener<T> messageListener)
		{
			conf.MessageListener = messageListener;
			return this;
		}

		public IConsumerBuilder<T> ConsumerEventListener(IConsumerEventListener consumerEventListener)
		{
			conf.ConsumerEventListener = consumerEventListener;
			return this;
		}

		public IConsumerBuilder<T> CryptoKeyReader(ICryptoKeyReader cryptoKeyReader)
		{
			conf.CryptoKeyReader = cryptoKeyReader;
			return this;
		}

		public IConsumerBuilder<T> CryptoFailureAction(ConsumerCryptoFailureAction action)
		{
			conf.CryptoFailureAction = action;
			return this;
		}

		public IConsumerBuilder<T> ReceiverQueueSize(int receiverQueueSize)
		{
			if(receiverQueueSize <= 0)
				throw new System.Exception("receiverQueueSize needs to be >= 0");
			conf.ReceiverQueueSize = receiverQueueSize;
			return this;
		}

		public IConsumerBuilder<T> AcknowledgmentGroupTime(long delay, BAMCIS.Util.Concurrent.TimeUnit unit)
		{
			if(delay <= 0)
				throw new System.Exception("acknowledgmentGroupTime needs to be >= 0");
			conf.AcknowledgementsGroupTimeMicros = unit.ToMicros(delay);
			return this;
		}

		public IConsumerBuilder<T> ConsumerName(string consumerName)
		{
			if(string.IsNullOrWhiteSpace(consumerName))
				throw new System.Exception("consumerName cannot be blank");
			conf.ConsumerName = consumerName;
			return this;
		}

		public IConsumerBuilder<T> PriorityLevel(int priorityLevel)
		{
			if(priorityLevel <= 0)
				throw new System.Exception("priorityLevel needs to be >= 0");
			conf.PriorityLevel = priorityLevel;
			return this;
		}

		public IConsumerBuilder<T> Property(string key, string value)
		{
			if(string.IsNullOrWhiteSpace(key) && string.IsNullOrWhiteSpace(value))
				throw new System.Exception("property key/value cannot be blank");
			conf.Properties.Add(key, value);
			return this;
		}
		public IConsumerBuilder<T> Properties(IDictionary<string, string> properties)
		{
			if(!properties.Any())
				throw new System.Exception("properties cannot be empty");
			foreach(var prop in properties)
			{
				if (string.IsNullOrWhiteSpace(prop.Key) && string.IsNullOrWhiteSpace(prop.Value))
					throw new System.Exception("properties' key/value cannot be blank");
				conf.Properties.Add(prop.Key, prop.Value);
			}
			return this;
		}

		public IConsumerBuilder<T> MaxTotalReceiverQueueSizeAcrossPartitions(int maxTotalReceiverQueueSizeAcrossPartitions)
		{
			if(maxTotalReceiverQueueSizeAcrossPartitions < 0)
				throw new System.Exception("maxTotalReceiverQueueSizeAcrossPartitions needs to be >= 0");
			conf.MaxTotalReceiverQueueSizeAcrossPartitions = maxTotalReceiverQueueSizeAcrossPartitions;
			return this;
		}

		public IConsumerBuilder<T> ReadCompacted(bool readCompacted)
		{
			conf.ReadCompacted = readCompacted;
			return this;
		}

		public IConsumerBuilder<T> PatternAutoDiscoveryPeriod(int periodInMinutes)
		{
			if(periodInMinutes < 0)
				throw new System.Exception("periodInMinutes needs to be >= 0");
			conf.PatternAutoDiscoveryPeriod = periodInMinutes;
			return this;
		}

		public IConsumerBuilder<T> SubscriptionInitialPosition(SubscriptionInitialPosition subscriptionInitialPosition)
		{
			conf.SubscriptionInitialPosition = subscriptionInitialPosition;
			return this;
		}

		public IConsumerBuilder<T> SubscriptionTopicsMode(RegexSubscriptionMode mode)
		{
			conf.RegexSubscriptionMode = mode;
			return this;
		}

		public IConsumerBuilder<T> ReplicateSubscriptionState(bool replicateSubscriptionState)
		{
			conf.ReplicateSubscriptionState = replicateSubscriptionState;
			return this;
		}

		public IConsumerBuilder<T> intercept(params Disposeasync<T>[] interceptors)
		{
			if (interceptorList == null)
			{
				interceptorList = new List<Disposeasync<T>>();
			}
			((List<Disposeasync<T>>)interceptorList).AddRange(interceptors.ToList());
			return this;
		}

		public IConsumerBuilder<T> DeadLetterPolicy(DeadLetterPolicy deadLetterPolicy)
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

		public IConsumerBuilder<T> AutoUpdatePartitions(bool autoUpdate)
		{
			conf.AutoUpdatePartitions = autoUpdate;
			return this;
		}

		public IConsumerBuilder<T> StartMessageIdInclusive()
		{
			conf.ResetIncludeHead = true;
			return this;
		}

		public virtual IConsumerBuilder<T> BatchReceivePolicy(BatchReceivePolicy batchReceivePolicy)
		{
			if(batchReceivePolicy == null)
				throw new NullReferenceException("batchReceivePolicy must not be null.");
			batchReceivePolicy.Verify();
			conf.BatchReceivePolicy = batchReceivePolicy;
			return this;
		}

		public override string ToString()
		{
			return conf != null ? conf.ToString() : null;
		}

		public IConsumerBuilder<T> KeySharedPolicy(KeySharedPolicy keySharedPolicy)
		{
			keySharedPolicy.Validate();
			conf.KeySharedPolicy = keySharedPolicy;
			return this;
		}
		
		public IConsumerBuilder<T> Intercept(params Disposeasync<T>[] interceptors)
		{
			throw new NotImplementedException();
		}

		object ICloneable.Clone()
		{
			throw new NotImplementedException();
		}
	}

}