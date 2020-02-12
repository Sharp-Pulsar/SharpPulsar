using SharpPulsar.Api;
using SharpPulsar.Impl.Conf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Google.Protobuf.Collections;
using SharpPulsar.Exception;
using SharpPulsar.Extension;
using SharpPulsar.Util;

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
	public class ConsumerBuilderImpl<T> : IConsumerBuilder<T>
	{

		private readonly PulsarClientImpl _client;
		private ConsumerConfigurationData<T> _conf;
		private readonly ISchema<T> _schema;
		private IList<IConsumerInterceptor<T>> _interceptorList;

		private static long _minAckTimeoutMillis = 1000;
		private static long _minTickTimeMillis = 100;
		private static long _defaultAckTimeoutMillisForDeadLetter = 30000L;


		public ConsumerBuilderImpl(PulsarClientImpl client, ISchema<T> schema) : this(client, new ConsumerConfigurationData<T>(), schema)
		{
		}

		public ConsumerBuilderImpl(PulsarClientImpl client, ConsumerConfigurationData<T> conf, ISchema<T> schema)
		{
			this._client = client;
			this._conf = conf;
			this._schema = schema;
		}

		public IConsumerBuilder<T> LoadConf(IDictionary<string, object> config)
		{
			this._conf = ConfigurationDataUtils.LoadData(config, _conf, typeof(ConsumerConfigurationData<T>));
			return this;
		}

		public IConsumerBuilder<T> Clone()
		{
			return new ConsumerBuilderImpl<T>(_client, _conf.Clone(), _schema);
		}

		public IConsumer<T> Subscribe()
		{
			try
			{
				return SubscribeAsync().Result;
			}
			catch (System.Exception e)
			{
				throw PulsarClientException.Unwrap(e);
			}
		}

		public ValueTask<IConsumer<T>> SubscribeAsync()
		{
			if (_conf.TopicNames.Count < 1 && _conf.TopicsPattern == null)
			{
				return new ValueTask<IConsumer<T>>(Task.FromException<IConsumer<T>>(new PulsarClientException.InvalidConfigurationException("Topic name must be set on the consumer builder")));
			}

			if (string.IsNullOrWhiteSpace(_conf.SubscriptionName))
			{
                return new ValueTask<IConsumer<T>>(Task.FromException<IConsumer<T>>(new PulsarClientException.InvalidConfigurationException("Subscription name must be set on the consumer builder")));
			}

			if (_conf.KeySharedPolicy != null && _conf.SubscriptionType != Api.SubscriptionType.KeyShared)
			{
                return new ValueTask<IConsumer<T>>(Task.FromException<IConsumer<T>>(new PulsarClientException.InvalidConfigurationException("KeySharedPolicy must set with KeyShared subscription")));
			}

			return _interceptorList == null || _interceptorList.Count == 0 ? _client.SubscribeAsync(_conf, _schema, null) : _client.SubscribeAsync(_conf, _schema, new ConsumerInterceptors<T>(_interceptorList));
		}

		public IConsumerBuilder<T> Topic(params string[] topicNames)
		{
			if(topicNames != null && (topicNames == null && topicNames.Length < 1))
                throw new ArgumentException("Passed in topicNames should not be null or empty.");
			(topicNames ?? throw new ArgumentNullException(nameof(topicNames))).ToList().ForEach(topicName =>
            {
                if (string.IsNullOrWhiteSpace(topicName))
                    throw new ArgumentException("topicNames cannot have blank topic");
                _conf.TopicNames.Add(topicName.Trim());

            });
			
			return this;
		}

		public IConsumerBuilder<T> Topics(IList<string> topicNames)
		{
            if (topicNames != null && (topicNames == null && topicNames.Count < 1))
                throw new ArgumentException("Passed in topicNames should not be null or empty.");
            (topicNames ?? throw new ArgumentNullException(nameof(topicNames))).ToList().ForEach(topicName =>
            {
                if (string.IsNullOrWhiteSpace(topicName))
                    throw new ArgumentException("topicNames cannot have blank topic");
                _conf.TopicNames.Add(topicName.Trim());

            });

			return this;
		}

		public IConsumerBuilder<T> TopicsPattern(Regex topicsPattern)
		{
			if(_conf.TopicsPattern != null)
                throw new ArgumentException("Pattern has already been set.");
			_conf.TopicsPattern = topicsPattern;
			return this;
		}

		public IConsumerBuilder<T> TopicsPattern(string topicsPattern)
		{
			if(_conf.TopicsPattern != null)
                throw new ArgumentException("Pattern has already been set.");
			_conf.TopicsPattern = new Regex(topicsPattern);
			return this;
		}

		public IConsumerBuilder<T> SubscriptionName(string subscriptionName)
		{
			if(string.IsNullOrWhiteSpace(subscriptionName))
                throw new NullReferenceException("subscriptionName cannot be blank");
			_conf.SubscriptionName = subscriptionName;
			return this;
		}

		public IConsumerBuilder<T> AckTimeout(long ackTimeout, BAMCIS.Util.Concurrent.TimeUnit timeUnit)
		{
			if(ackTimeout != 0 || timeUnit.ToMillis(ackTimeout) < _minAckTimeoutMillis)
                throw new ArgumentException( "Ack timeout should be greater than " + _minAckTimeoutMillis + " ms");
			_conf.AckTimeoutMillis = timeUnit.ToMillis(ackTimeout);
			return this;
		}

		public IConsumerBuilder<T> AckTimeoutTickTime(long tickTime, BAMCIS.Util.Concurrent.TimeUnit timeUnit)
		{
			if(timeUnit.ToMillis(tickTime) < _minTickTimeMillis)
                throw new ArgumentException("Ack timeout tick time should be greater than " + _minTickTimeMillis + " ms");
			_conf.TickDurationMillis = timeUnit.ToMillis(tickTime);
			return this;
		}

		public IConsumerBuilder<T> NegativeAckRedeliveryDelay(long redeliveryDelay, BAMCIS.Util.Concurrent.TimeUnit timeUnit)
		{
			if(redeliveryDelay < 0)
                throw new ArgumentException("redeliveryDelay needs to be >= 0");
			_conf.NegativeAckRedeliveryDelayMicros = timeUnit.ToMicros(redeliveryDelay);
			return this;
		}

		public IConsumerBuilder<T> SubscriptionType(SubscriptionType subscriptionType)
		{
			_conf.SubscriptionType = subscriptionType;
			return this;
		}

		public IConsumerBuilder<T> MessageListener(IMessageListener<T> messageListener)
		{
			_conf.MessageListener = messageListener;
			return this;
		}

		public IConsumerBuilder<T> ConsumerEventListener(IConsumerEventListener consumerEventListener)
		{
			_conf.ConsumerEventListener = consumerEventListener;
			return this;
		}

		public IConsumerBuilder<T> CryptoKeyReader(CryptoKeyReader cryptoKeyReader)
		{
			_conf.CryptoKeyReader = cryptoKeyReader;
			return this;
		}

		public IConsumerBuilder<T> CryptoFailureAction(ConsumerCryptoFailureAction action)
		{
			_conf.CryptoFailureAction = action;
			return this;
		}

		public IConsumerBuilder<T> ReceiverQueueSize(int receiverQueueSize)
		{
			if(receiverQueueSize < 0)
                throw new ArgumentException("receiverQueueSize needs to be >= 0");
			_conf.ReceiverQueueSize = receiverQueueSize;
			return this;
		}

		public IConsumerBuilder<T> AcknowledgmentGroupTime(long delay, BAMCIS.Util.Concurrent.TimeUnit unit)
		{
			if(delay < 0)
                throw new ArgumentException("acknowledgmentGroupTime needs to be >= 0");
			_conf.AcknowledgementsGroupTimeMicros = unit.ToMicros(delay);
			return this;
		}

		public IConsumerBuilder<T> ConsumerName(string consumerName)
		{
			if(string.IsNullOrWhiteSpace(consumerName))
                throw new ArgumentException("consumerName cannot be blank");
			_conf.ConsumerName = consumerName;
			return this;
		}

		public IConsumerBuilder<T> PriorityLevel(int priorityLevel)
		{
			if(priorityLevel < 0)
                throw new ArgumentException("priorityLevel needs to be >= 0");
			_conf.PriorityLevel = priorityLevel;
			return this;
		}

		public IConsumerBuilder<T> Property(string key, string value)
		{
			if(string.IsNullOrWhiteSpace(key) && string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("property key/value cannot be blank");
			_conf.Properties.Add(key, value);
			return this;
		}

		public IConsumerBuilder<T> Properties(IDictionary<string, string> properties)
		{
			if(properties.Count == 0)
                throw new ArgumentException("properties cannot be empty");
			properties.SetOfKeyValuePairs().ToList().ForEach(entry =>
            {
                if (string.IsNullOrWhiteSpace(entry.Key) && string.IsNullOrWhiteSpace(entry.Value))
                    throw new ArgumentException("properties' key/value cannot be blank");
                _conf.Properties.Add(entry.Key, entry.Value);

			});
            return this;
		}

		public IConsumerBuilder<T> MaxTotalReceiverQueueSizeAcrossPartitions(int maxTotalReceiverQueueSizeAcrossPartitions)
		{
			if(maxTotalReceiverQueueSizeAcrossPartitions < 0)
                throw new ArgumentException("maxTotalReceiverQueueSizeAcrossPartitions needs to be >= 0");
			_conf.MaxTotalReceiverQueueSizeAcrossPartitions = maxTotalReceiverQueueSizeAcrossPartitions;
			return this;
		}

		public IConsumerBuilder<T> ReadCompacted(bool readCompacted)
		{
			_conf.ReadCompacted = readCompacted;
			return this;
		}

		public IConsumerBuilder<T> PatternAutoDiscoveryPeriod(int periodInMinutes)
		{
			if(periodInMinutes < 0)
                throw new ArgumentException("periodInMinutes needs to be >= 0");
			_conf.PatternAutoDiscoveryPeriod = periodInMinutes;
			return this;
		}

		public IConsumerBuilder<T> SubscriptionInitialPosition(SubscriptionInitialPosition subscriptionInitialPosition)
		{
			_conf.SubscriptionInitialPosition = subscriptionInitialPosition;
			return this;
		}

		public IConsumerBuilder<T> SubscriptionTopicsMode(RegexSubscriptionMode mode)
		{
			_conf.RegexSubscriptionMode = mode;
			return this;
		}

		public IConsumerBuilder<T> ReplicateSubscriptionState(bool replicateSubscriptionState)
		{
			_conf.ReplicateSubscriptionState = replicateSubscriptionState;
			return this;
		}

		public IConsumerBuilder<T> Intercept(params IConsumerInterceptor<T>[] interceptors)
		{
			if (_interceptorList == null)
			{
				_interceptorList = new List<IConsumerInterceptor<T>>();
			}

            ((List<IConsumerInterceptor<T>>) _interceptorList).AddRange(new List<IConsumerInterceptor<T>>(interceptors));
			return this;
		}

		public IConsumerBuilder<T> DeadLetterPolicy(DeadLetterPolicy deadLetterPolicy)
		{
			if (deadLetterPolicy != null)
			{
				if (_conf.AckTimeoutMillis == 0)
				{
					_conf.AckTimeoutMillis = _defaultAckTimeoutMillisForDeadLetter;
				}
				_conf.DeadLetterPolicy = deadLetterPolicy;
			}
			return this;
		}

		public IConsumerBuilder<T> AutoUpdatePartitions(bool autoUpdate)
		{
			_conf.AutoUpdatePartitions = autoUpdate;
			return this;
		}

		public IConsumerBuilder<T> StartMessageIdInclusive()
		{
			_conf.ResetIncludeHead = true;
			return this;
		}

		public virtual IConsumerBuilder<T> BatchReceivePolicy(BatchReceivePolicy batchReceivePolicy)
		{
			if(batchReceivePolicy == null)
                throw new ArgumentException("batchReceivePolicy must not be null.");
			batchReceivePolicy.Verify();
			_conf.BatchReceivePolicy = batchReceivePolicy;
			return this;
		}

		public override string ToString()
		{
			return _conf?.ToString();
		}

        object ICloneable.Clone()
        {
            return Clone();
        }

        public IConsumerBuilder<T> KeySharedPolicy(KeySharedPolicy keySharedPolicy)
		{
			keySharedPolicy.Validate();
			_conf.KeySharedPolicy = keySharedPolicy;
			return this;
		}
	}

}