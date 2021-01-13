using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using SharpPulsar.Api;
using SharpPulsar.Batch;
using SharpPulsar.Batch.Api;
using SharpPulsar.Common;
using SharpPulsar.Extension;
using SharpPulsar.Impl.Conf;
using SharpPulsar.Interfaces;
using SharpPulsar.Precondition;
using SharpPulsar.Protocol.Proto;
using SharpPulsar.Utils;
using HashMapHelper = SharpPulsar.Presto.HashMapHelper;

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
	public sealed class ConsumerConfigBuilder
	{
		private ConsumerConfigurationData _conf = new ConsumerConfigurationData();

		private  long _minAckTimeoutMillis = 1000;
		private  long _minTickTimeMillis = 100;
		private  long _defaultAckTimeoutMillisForDeadLetter = 30000L;

        public ConsumerConfigurationData ConsumerConfigurationData 
        {
            get
            {
				if (string.IsNullOrWhiteSpace(_conf.SubscriptionName))
					throw new ArgumentException("Subscription Name is required!");
                if (_conf.StartMessageId == null)
                    _conf.StartMessageId = MessageIdFields.Latest;
				if (_conf.ConsumerEventListener == null || _conf.MessageListener == null)
					throw new ArgumentException("ConsumerEventListener and MessageListener cannot be null");
                return _conf;
            }
        }
		public ConsumerConfigBuilder LoadConf(IDictionary<string, object> config)
		{
			_conf = (ConsumerConfigurationData)ConfigurationDataUtils.LoadData(config, _conf);
            return this;
        }

        public ConsumerConfigBuilder MaxPendingChuckedMessage(int max)
        {
            _conf.MaxPendingChuckedMessage = max;
            return this;
        }

        public ConsumerConfigBuilder ExpireTimeOfIncompleteChunkedMessageMillis(long expireTime)
        {
            _conf.ExpireTimeOfIncompleteChunkedMessageMillis = expireTime;
            return this;
        }

        public ConsumerConfigBuilder AutoAckOldestChunkedMessageOnQueueFull(bool autoAck)
        {
            _conf.AutoAckOldestChunkedMessageOnQueueFull = autoAck;
            return this;
        }
        private ConsumerConfigBuilder BatchConsume(bool batchConsume)
        {
            _conf.BatchConsume = batchConsume;
            return this;
        }
        private ConsumerConfigBuilder BatchConsumeTimeout(long batchConsumeTimeoutMs)
        {
            _conf.BatchConsumeTimeout = batchConsumeTimeoutMs;
            return this;
        }
		public ConsumerConfigBuilder SetConsumptionType(ConsumptionType type)
        {
            _conf.ConsumptionType = type;
            return this;
        }
		public ConsumerConfigBuilder ForceTopicCreation(bool force)
        {
            _conf.ForceTopicCreation = force;
            return this;
        }
		public ConsumerConfigBuilder Topic(params string[] topicNames)
		{
			if(topicNames == null || topicNames.Length < 1)
                throw new ArgumentException("Passed in topicNames should not be null or empty.");
			(topicNames).ToList().ForEach(topicName =>
            {
                if (string.IsNullOrWhiteSpace(topicName))
                    throw new ArgumentException("topicNames cannot have blank topic");
                _conf.TopicNames.Add(topicName.Trim());

            });

			return this;
		}
        
		public ConsumerConfigBuilder StartMessageId(long ledgerId, long entryId, int partitionIndex, int batchIndex)
        {
            _conf.StartMessageId = new BatchMessageId(ledgerId, entryId, partitionIndex, batchIndex);
            return this;
        }
		public ConsumerConfigBuilder StartMessageId(IMessageId startMessageId)
        {
            _conf.StartMessageId = startMessageId;
            return this;
        }
		public ConsumerConfigBuilder Topics(IList<string> topicNames)
		{
            if (topicNames == null || topicNames.Count < 1)
                throw new ArgumentException("Passed in topicNames should not be null or empty.");
            (topicNames).ToList().ForEach(topicName =>
            {
                if (string.IsNullOrWhiteSpace(topicName))
                    throw new ArgumentException("topicNames cannot have blank topic");
                _conf.TopicNames.Add(topicName.Trim());

            });

            return this;
		}

		public ConsumerConfigBuilder TopicsPattern(Regex topicsPattern)
		{
			if(_conf.TopicsPattern != null)
                throw new ArgumentException("Pattern has already been set.");
			_conf.TopicsPattern = topicsPattern;
            return this;
		}

		public ConsumerConfigBuilder TopicsPattern(string topicsPattern)
		{
			if(_conf.TopicsPattern != null)
                throw new ArgumentException("Pattern has already been set.");
			_conf.TopicsPattern = new Regex(topicsPattern);
            return this;
		}

		public ConsumerConfigBuilder SubscriptionName(string subscriptionName)
		{
			if(string.IsNullOrWhiteSpace(subscriptionName))
                throw new NullReferenceException("SubscriptionName cannot be blank");
			_conf.SubscriptionName = subscriptionName;
            return this;
		}

		public ConsumerConfigBuilder AckTimeout(long ackTimeoutMs)
		{
			Condition.CheckArgument(ackTimeoutMs == 0 ||  ackTimeoutMs >= _minAckTimeoutMillis, "Ack timeout should be greater than " + _minAckTimeoutMillis + " ms");
			_conf.AckTimeoutMillis = ackTimeoutMs;
            return this;
		}

		public ConsumerConfigBuilder AckTimeoutTickTime(long tickTimeMs)
		{
            Condition.CheckArgument(tickTimeMs < _minTickTimeMillis, "Ack timeout tick time should be greater than " + _minTickTimeMillis + " ms");
			_conf.TickDurationMillis = tickTimeMs;
            return this;
		}

		public ConsumerConfigBuilder NegativeAckRedeliveryDelay(long redeliveryDelayMs)
        {
            Condition.CheckArgument(redeliveryDelayMs >= 0, "redeliveryDelay needs to be >= 0");
            _conf.NegativeAckRedeliveryDelayMs = redeliveryDelayMs;
            return this;
		}

		public ConsumerConfigBuilder SubscriptionType(CommandSubscribe.SubType subscriptionType)
		{
			_conf.SubscriptionType = subscriptionType;
            return this;
		}

		public ConsumerConfigBuilder MessageListener(IMessageListener messageListener)
		{
			_conf.MessageListener = messageListener;
            return this;
		}

		public ConsumerConfigBuilder ConsumerEventListener(IConsumerEventListener consumerEventListener)
		{
			_conf.ConsumerEventListener = consumerEventListener;
            return this;
		}

		public ConsumerConfigBuilder CryptoKeyReader(ICryptoKeyReader cryptoKeyReader)
		{
			_conf.CryptoKeyReader = cryptoKeyReader;
            return this;
		}

		public ConsumerConfigBuilder CryptoFailureAction(ConsumerCryptoFailureAction? action)
        {
            if (action != null) _conf.CryptoFailureAction = (ConsumerCryptoFailureAction) action;
			return this;
		}

		public ConsumerConfigBuilder ReceiverQueueSize(int receiverQueueSize)
		{
			if(receiverQueueSize < 0)
                throw new ArgumentException("receiverQueueSize needs to be >= 0");
			_conf.ReceiverQueueSize = receiverQueueSize;
            return this;
		}

		public ConsumerConfigBuilder AcknowledgmentGroupTime(long delayMs)
        {
            Condition.CheckArgument(delayMs >= 0, "acknowledgmentGroupTime needs to be >= 0");
            _conf.AcknowledgementsGroupTimeMs = delayMs;
            return this;
		}

		public ConsumerConfigBuilder ConsumerName(string consumerName)
		{
			if(string.IsNullOrWhiteSpace(consumerName))
                throw new ArgumentException("consumerName cannot be blank");
			_conf.ConsumerName = consumerName;
            return this;
		}

		public ConsumerConfigBuilder PriorityLevel(int priorityLevel)
		{
			if(priorityLevel < 0)
                throw new ArgumentException("priorityLevel needs to be >= 0");
			_conf.PriorityLevel = priorityLevel;
            return this;
		}

		public ConsumerConfigBuilder Property(string key, string value)
        {
            if(string.IsNullOrWhiteSpace(key) && string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("property key/value cannot be blank");
            if (key != null) _conf.Properties.Add(key, value);
            return this;
		}

		public ConsumerConfigBuilder Properties(IDictionary<string, string> properties)
		{
			if(properties.Count == 0)
                throw new ArgumentException("properties cannot be empty");
			HashMapHelper.SetOfKeyValuePairs(properties).ToList().ForEach(entry =>
            {
                if (entry.Key == null || entry.Value == null)
                    throw new ArgumentException("properties' key/value cannot be blank");
                if (string.IsNullOrWhiteSpace(entry.Key) || string.IsNullOrWhiteSpace(entry.Value))
                    throw new ArgumentException("properties' key/value cannot be blank");
				_conf.Properties.Add(entry.Key, entry.Value);

			});
			return this;
		}

		public ConsumerConfigBuilder MaxTotalReceiverQueueSizeAcrossPartitions(int maxTotalReceiverQueueSizeAcrossPartitions)
		{
			if(maxTotalReceiverQueueSizeAcrossPartitions < 0)
                throw new ArgumentException("maxTotalReceiverQueueSizeAcrossPartitions needs to be >= 0");
			_conf.MaxTotalReceiverQueueSizeAcrossPartitions = maxTotalReceiverQueueSizeAcrossPartitions;
            return this;
		}

		public ConsumerConfigBuilder ReadCompacted(bool readCompacted)
		{
			_conf.ReadCompacted = readCompacted;
            return this;
		}

		public ConsumerConfigBuilder PatternAutoDiscoveryPeriod(int periodInSeconds)
		{
			if(periodInSeconds< 0)
                throw new ArgumentException("periodInMinutes needs to be >= 0");
			_conf.PatternAutoDiscoveryPeriod = periodInSeconds;
            return this;
		}

		public ConsumerConfigBuilder SubscriptionInitialPosition(SubscriptionInitialPosition subscriptionInitialPosition)
		{
			_conf.SubscriptionInitialPosition = subscriptionInitialPosition;
            return this;
		}

		public ConsumerConfigBuilder SubscriptionTopicsMode(RegexSubscriptionMode mode)
		{
			_conf.RegexSubscriptionMode = mode;
            return this;
		}

		public ConsumerConfigBuilder ReplicateSubscriptionState(bool replicateSubscriptionState)
		{
			_conf.ReplicateSubscriptionState = replicateSubscriptionState;
            return this;
		}

		public ConsumerConfigBuilder Intercept(params IConsumerInterceptor[] interceptors)
		{
			if (_conf.Interceptors == null)
			{
                _conf.Interceptors = new List<IConsumerInterceptor>();
			}

            _conf.Interceptors.AddRange(new List<IConsumerInterceptor>(interceptors));
            return this;
		}

		public ConsumerConfigBuilder DeadLetterPolicy(DeadLetterPolicy deadLetterPolicy)
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

		public ConsumerConfigBuilder AutoUpdatePartitions(bool autoUpdate)
		{
			_conf.AutoUpdatePartitions = autoUpdate;
            return this;
		}

		public ConsumerConfigBuilder StartMessageIdInclusive()
		{
			_conf.ResetIncludeHead = true;
            return this;
		}

		public ConsumerConfigBuilder BatchReceivePolicy(BatchReceivePolicy batchReceivePolicy)
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


        public ConsumerConfigBuilder KeySharedPolicy(KeySharedPolicy keySharedPolicy)
		{
			keySharedPolicy.Validate();
			_conf.KeySharedPolicy = keySharedPolicy;
            return this;
		}
	}

}