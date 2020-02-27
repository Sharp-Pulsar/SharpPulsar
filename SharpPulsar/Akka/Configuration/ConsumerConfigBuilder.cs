using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using SharpPulsar.Api;
using SharpPulsar.Extension;
using SharpPulsar.Impl.Conf;
using SharpPulsar.Utility;

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
namespace SharpPulsar.Akka.Configuration
{
	public sealed class ConsumerConfigBuilder
	{
		private ConsumerConfigurationData _conf = new ConsumerConfigurationData();

		private  long _minAckTimeoutMillis = 1000;
		private  long _minTickTimeMillis = 100;
		private  long _defaultAckTimeoutMillisForDeadLetter = 30000L;

        public ConsumerConfigurationData ConsumerConfigurationData => _conf;
		public void LoadConf(IDictionary<string, object> config)
		{
			_conf = ConfigurationDataUtils.LoadData(config, _conf);
			
		}
		
		public void Topic(params string[] topicNames)
		{
			if(topicNames == null || topicNames.Length < 1)
                throw new ArgumentException("Passed in topicNames should not be null or empty.");
			(topicNames).ToList().ForEach(topicName =>
            {
                if (string.IsNullOrWhiteSpace(topicName))
                    throw new ArgumentException("topicNames cannot have blank topic");
                _conf.TopicNames.Add(topicName.Trim());

            });
			
			
		}

		public void Topics(IList<string> topicNames)
		{
            if (topicNames == null || topicNames.Count < 1)
                throw new ArgumentException("Passed in topicNames should not be null or empty.");
            (topicNames).ToList().ForEach(topicName =>
            {
                if (string.IsNullOrWhiteSpace(topicName))
                    throw new ArgumentException("topicNames cannot have blank topic");
                _conf.TopicNames.Add(topicName.Trim());

            });

			
		}

		public void TopicsPattern(Regex topicsPattern)
		{
			if(_conf.TopicsPattern != null)
                throw new ArgumentException("Pattern has already been set.");
			_conf.TopicsPattern = topicsPattern;
			
		}

		public void TopicsPattern(string topicsPattern)
		{
			if(_conf.TopicsPattern != null)
                throw new ArgumentException("Pattern has already been set.");
			_conf.TopicsPattern = new Regex(topicsPattern);
			
		}

		public void SubscriptionName(string subscriptionName)
		{
			if(string.IsNullOrWhiteSpace(subscriptionName))
                throw new NullReferenceException("subscriptionName cannot be blank");
			_conf.SubscriptionName = subscriptionName;
			
		}

		public void AckTimeout(long ackTimeout, BAMCIS.Util.Concurrent.TimeUnit timeUnit)
		{
			if(ackTimeout != 0 || timeUnit.ToMillis(ackTimeout) < _minAckTimeoutMillis)
                throw new ArgumentException( "Ack timeout should be greater than " + _minAckTimeoutMillis + " ms");
			_conf.AckTimeoutMillis = timeUnit.ToMillis(ackTimeout);
			
		}

		public void AckTimeoutTickTime(long tickTime, BAMCIS.Util.Concurrent.TimeUnit timeUnit)
		{
			if(timeUnit.ToMillis(tickTime) < _minTickTimeMillis)
                throw new ArgumentException("Ack timeout tick time should be greater than " + _minTickTimeMillis + " ms");
			_conf.TickDurationMillis = timeUnit.ToMillis(tickTime);
			
		}

		public void NegativeAckRedeliveryDelay(long redeliveryDelay, BAMCIS.Util.Concurrent.TimeUnit timeUnit)
		{
			if(redeliveryDelay < 0)
                throw new ArgumentException("redeliveryDelay needs to be >= 0");
			_conf.NegativeAckRedeliveryDelayMicros = timeUnit.ToMicros(redeliveryDelay);
			
		}

		public void SubscriptionType(SubscriptionType subscriptionType)
		{
			_conf.SubscriptionType = subscriptionType;
			
		}

		public void MessageListener(IMessageListener messageListener)
		{
			_conf.MessageListener = messageListener;
			
		}

		public void ConsumerEventListener(IConsumerEventListener consumerEventListener)
		{
			_conf.ConsumerEventListener = consumerEventListener;
			
		}

		public void CryptoKeyReader(ICryptoKeyReader cryptoKeyReader)
		{
			_conf.CryptoKeyReader = cryptoKeyReader;
			
		}

		public void CryptoFailureAction(ConsumerCryptoFailureAction? action)
        {
            if (action != null) _conf.CryptoFailureAction = (ConsumerCryptoFailureAction) action;
            
        }

		public void ReceiverQueueSize(int receiverQueueSize)
		{
			if(receiverQueueSize < 0)
                throw new ArgumentException("receiverQueueSize needs to be >= 0");
			_conf.ReceiverQueueSize = receiverQueueSize;
			
		}

		public void AcknowledgmentGroupTime(long delay, BAMCIS.Util.Concurrent.TimeUnit unit)
		{
			if(delay < 0)
                throw new ArgumentException("acknowledgmentGroupTime needs to be >= 0");
			_conf.AcknowledgementsGroupTimeMicros = unit.ToMicros(delay);
			
		}

		public void ConsumerName(string consumerName)
		{
			if(string.IsNullOrWhiteSpace(consumerName))
                throw new ArgumentException("consumerName cannot be blank");
			_conf.ConsumerName = consumerName;
			
		}

		public void PriorityLevel(int priorityLevel)
		{
			if(priorityLevel < 0)
                throw new ArgumentException("priorityLevel needs to be >= 0");
			_conf.PriorityLevel = priorityLevel;
			
		}

		public void Property(string key, string value)
        {
            if(string.IsNullOrWhiteSpace(key) && string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("property key/value cannot be blank");
            if (key != null) _conf.Properties.Add(key, value);
        }

		public void Properties(IDictionary<string, string> properties)
		{
			if(properties.Count == 0)
                throw new ArgumentException("properties cannot be empty");
			properties.SetOfKeyValuePairs().ToList().ForEach(entry =>
            {
                if (entry.Key == null || entry.Value == null)
                    throw new ArgumentException("properties' key/value cannot be blank");
                if (string.IsNullOrWhiteSpace(entry.Key) || string.IsNullOrWhiteSpace(entry.Value))
                    throw new ArgumentException("properties' key/value cannot be blank");
				_conf.Properties.Add(entry.Key, entry.Value);

			});
            
		}

		public void MaxTotalReceiverQueueSizeAcrossPartitions(int maxTotalReceiverQueueSizeAcrossPartitions)
		{
			if(maxTotalReceiverQueueSizeAcrossPartitions < 0)
                throw new ArgumentException("maxTotalReceiverQueueSizeAcrossPartitions needs to be >= 0");
			_conf.MaxTotalReceiverQueueSizeAcrossPartitions = maxTotalReceiverQueueSizeAcrossPartitions;
			
		}

		public void ReadCompacted(bool readCompacted)
		{
			_conf.ReadCompacted = readCompacted;
			
		}

		public void PatternAutoDiscoveryPeriod(int periodInMinutes)
		{
			if(periodInMinutes < 0)
                throw new ArgumentException("periodInMinutes needs to be >= 0");
			_conf.PatternAutoDiscoveryPeriod = periodInMinutes;
			
		}

		public void SubscriptionInitialPosition(SubscriptionInitialPosition subscriptionInitialPosition)
		{
			_conf.SubscriptionInitialPosition = subscriptionInitialPosition;
			
		}

		public void SubscriptionTopicsMode(RegexSubscriptionMode mode)
		{
			_conf.RegexSubscriptionMode = mode;
			
		}

		public void ReplicateSubscriptionState(bool replicateSubscriptionState)
		{
			_conf.ReplicateSubscriptionState = replicateSubscriptionState;
			
		}

		public void Intercept(params IConsumerInterceptor[] interceptors)
		{
			if (_conf.Interceptors == null)
			{
                _conf.Interceptors = new List<IConsumerInterceptor>();
			}

            _conf.Interceptors.AddRange(new List<IConsumerInterceptor>(interceptors));
			
		}

		public void DeadLetterPolicy(DeadLetterPolicy deadLetterPolicy)
		{
			if (deadLetterPolicy != null)
			{
				if (_conf.AckTimeoutMillis == 0)
				{
					_conf.AckTimeoutMillis = _defaultAckTimeoutMillisForDeadLetter;
				}
				_conf.DeadLetterPolicy = deadLetterPolicy;
			}
			
		}

		public void AutoUpdatePartitions(bool autoUpdate)
		{
			_conf.AutoUpdatePartitions = autoUpdate;
			
		}

		public void StartMessageIdInclusive()
		{
			_conf.ResetIncludeHead = true;
			
		}

		public void BatchReceivePolicy(BatchReceivePolicy batchReceivePolicy)
		{
			if(batchReceivePolicy == null)
                throw new ArgumentException("batchReceivePolicy must not be null.");
			batchReceivePolicy.Verify();
			_conf.BatchReceivePolicy = batchReceivePolicy;
			
		}

        public void Schema(ISchema schema)
        {
            if (schema == null)
                throw new ArgumentException("Schama is null");
            _conf.Schema = schema;
        }
		public override string ToString()
		{
			return _conf?.ToString();
		}


        public void KeySharedPolicy(KeySharedPolicy keySharedPolicy)
		{
			keySharedPolicy.Validate();
			_conf.KeySharedPolicy = keySharedPolicy;
			
		}
	}

}