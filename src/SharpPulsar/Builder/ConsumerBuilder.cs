using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using SharpPulsar.Batch;
using SharpPulsar.Configuration;
using SharpPulsar.Common.Precondition;
using SharpPulsar.API;
using SharpPulsar.Messages;
using Akka.Util.Internal;
using DotNetty.Common.Utilities;
using SharpPulsar.Common.Protocol.Proto;
using SharpPulsar.Shared;
using SharpPulsar.Crypto;
using System.Reactive.Joins;
using System.Threading.Tasks;

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
namespace SharpPulsar.Builder
{
    public class ConsumerBuilder<T> : IConsumerBuilder<T>
    {
        private ConsumerConfigurationData<T> _conf = new ConsumerConfigurationData<T>();
        private readonly PulsarClient _client;
        private readonly ISchema<T> _schema;
        private IList<IConsumerInterceptor<T>> _interceptorList;


        private readonly long _minAckTimeoutMillis = 1000;
        private readonly long _minTickTimeMillis = 100;
        private readonly TimeSpan _defaultAckTimeoutMillisForDeadLetter = TimeSpan.FromMilliseconds(30000);

        public ConsumerBuilder(PulsarClient client, ISchema<T> schema) : this(client, new ConsumerConfigurationData<T>(), schema)
        {
        }

        internal ConsumerBuilder(PulsarClient client, ConsumerConfigurationData<T> conf, ISchema<T> schema)
        {
            Condition.CheckArgument(schema != null, "Schema should not be null.");
            _client = client;
            _conf = conf;
            _schema = schema;
        }

        public virtual IConsumerBuilder<T> Clone()
        {
            return new ConsumerBuilder<T>(_client, _conf.Clone(), _schema);
        }


        public ConsumerConfigurationData<T> ConsumerConfigurationData
        {
            get
            {
                if (_conf.StartMessageId == null)
                    _conf.StartMessageId = IMessageId.Latest;
                return _conf;
            }
        }
        public virtual IConsumerBuilder<T> LoadConf(IDictionary<string, object> config)
        {
            _conf = (ConsumerConfigurationData<T>)ConfigurationDataUtils.LoadData(config, _conf);
            return this;
        }

        public virtual IConsumerBuilder<T> AutoAckOldestChunkedMessageOnQueueFull(bool autoAck)
        {
            _conf.AutoAckOldestChunkedMessageOnQueueFull = autoAck;
            return this;
        }

        public virtual IConsumerBuilder<T> SetConsumptionType(ConsumptionType type)
        {
            _conf.ConsumptionType = type;
            return this;
        }
        public virtual IConsumerBuilder<T> ForceTopicCreation(bool force)
        {
            _conf.ForceTopicCreation = force;
            return this;
        }
       
        public virtual IConsumerBuilder<T> Topic(params string[] topicNames)
        {
            Condition.CheckArgument(topicNames != null && topicNames.Length > 0, "Passed in topicNames should not be null or empty.");
            return Topics(topicNames.ToList());
        }

        public virtual IConsumerBuilder<T> Topics(IList<string> topicNames)
        {
            Condition.CheckArgument(topicNames != null && topicNames.Count > 0, "Passed in topicNames list should not be null or empty.");
            topicNames.ToList().ForEach(topicName =>
            {
                if (string.IsNullOrWhiteSpace(topicName))
                    throw new ArgumentException("topicNames cannot have blank topic");
                _conf.TopicNames.Add(topicName.Trim());

            });
            return this;
        }
                
        public virtual IConsumerBuilder<T> TopicsPattern(Regex topicsPattern)
        {
            if (_conf.TopicsPattern != null)
                throw new ArgumentException("Pattern has already been set.");
            _conf.TopicsPattern = topicsPattern;
            return this;
        }

        public virtual IConsumerBuilder<T> TopicsPattern(string topicsPattern)
        {
            if (_conf.TopicsPattern != null)
                throw new ArgumentException("Pattern has already been set.");
            _conf.TopicsPattern = new Regex(topicsPattern);
            return this;
        }

        public virtual IConsumerBuilder<T> SubscriptionName(string subscriptionName)
        {
            if (string.IsNullOrWhiteSpace(subscriptionName))
                throw new NullReferenceException("SubscriptionName cannot be blank");
            _conf.SubscriptionName = subscriptionName;
            return this;
        }
        public virtual IConsumerBuilder<T> SubscriptionProperties(IDictionary<string, string> subscriptionProperties)
        {
            Condition.CheckArgument(subscriptionProperties != null, "subscriptionProperties cannot be null");
            _conf.SubscriptionProperties = subscriptionProperties;
            return this;
        }

        public virtual IConsumerBuilder<T> AckTimeout(long ackTimeout, TimeUnit.TimeUnit timeUnit)
        {
            Condition.CheckArgument(ackTimeout == 0 || ackTimeout >= _minAckTimeoutMillis, "Ack timeout should be greater than " + _minAckTimeoutMillis + " ms");
            _conf.AckTimeoutMillis = timeUnit.ToMilliseconds(ackTimeout);
            return this;
        }
        public virtual IConsumerBuilder<T> IsAckReceiptEnabled(bool isAckReceiptEnabled)
        {
            _conf.AckReceiptEnabled = isAckReceiptEnabled;
            return this;
        }

        public virtual IConsumerBuilder<T> AckTimeoutTickTime(long ackTimeout, TimeUnit.TimeUnit timeUnit)
        {
            Condition.CheckArgument(ackTimeout < _minTickTimeMillis, "Ack timeout tick time should be greater than " + _minTickTimeMillis + " ms");
            _conf.TickDurationMillis = timeUnit.ToMilliseconds(ackTimeout);
            return this;
        }

        public virtual IConsumerBuilder<T> NegativeAckRedeliveryDelay(long redeliveryDelay, TimeUnit.TimeUnit timeUnit)
        {
            Condition.CheckArgument(redeliveryDelay >= 0, "redeliveryDelay needs to be >= 0");
            _conf.NegativeAckRedeliveryDelayMicros = timeUnit.ToMicroseconds(redeliveryDelay);
            return this;
        }
        
        public virtual IConsumerBuilder<T> NegativeAckRedeliveryDelayPrecision(int negativeAckPrecisionBitCount)
        {
            Condition.CheckArgument(negativeAckPrecisionBitCount >= 0, "negativeAckPrecisionBitCount needs to be >= 0");
            _conf.NegativeAckPrecisionBitCnt = negativeAckPrecisionBitCount;
            return this;
        }


        public virtual IConsumerBuilder<T> SubscriptionType(SubscriptionType subscriptionType)
        {
            _conf.SubscriptionType = subscriptionType;
            return this;
        }
        public virtual IConsumerBuilder<T> SubscriptionMode(SubscriptionMode subscriptionMode)
        {
            _conf.SubscriptionMode = subscriptionMode;
            return this;
        }

        public virtual IConsumerBuilder<T> MessageListener(IMessageListener<T> messageListener)
        {
            _conf.MessageListener = messageListener;
            return this;
        }

        public IConsumerBuilder<T> MessageListenerExecutor(IMessageListenerExecutor messageListenerExecutor)
        {
            Condition.CheckArgument(messageListenerExecutor != null, "messageListenerExecutor needs to be not null");
            _conf.MessageListenerExecutor = messageListenerExecutor;
            return this;
        }

        public virtual IConsumerBuilder<T> ConsumerEventListener(IConsumerEventListener consumerEventListener)
        {
            _conf.ConsumerEventListener = consumerEventListener;
            return this;
        }

        public virtual IConsumerBuilder<T> CryptoKeyReader(ICryptoKeyReader cryptoKeyReader)
        {
            _conf.CryptoKeyReader = cryptoKeyReader;
            return this;
        }
        public virtual IConsumerBuilder<T> DefaultCryptoKeyReader(string privateKey)
        {
            Condition.CheckArgument(string.IsNullOrWhiteSpace(privateKey), "privateKey cannot be blank");
            return CryptoKeyReader(Crypto.DefaultCryptoKeyReader.Builder().DefaultPrivateKey(privateKey).Build());
        }

        public virtual IConsumerBuilder<T> DefaultCryptoKeyReader(IDictionary<string, string> privateKeys)
        {
            Condition.CheckArgument(privateKeys.Count > 0, "privateKeys cannot be empty");
            return CryptoKeyReader(Crypto.DefaultCryptoKeyReader.Builder().PrivateKeys(privateKeys).Build());
        }

        public virtual IConsumerBuilder<T> MssageCrypto(MessageCrypto messageCrypto)
        {
            _conf.MessageCrypto = messageCrypto;
            return this;
        }

        public IConsumerBuilder<T> MessageCrypto<MetadataT, BuilderT>(IMessageCrypto<MetadataT, BuilderT> messageCrypto)
        {
            _conf.MessageCrypto = (MessageCrypto)messageCrypto;
            return this;
        }

        public virtual IConsumerBuilder<T> CryptoFailureAction(ConsumerCryptoFailureAction action)
        {
            if (action != null) _conf.CryptoFailureAction = (ConsumerCryptoFailureAction)action;
            return this;
        }

        public virtual IConsumerBuilder<T> ReceiverQueueSize(int receiverQueueSize)
        {
            if (receiverQueueSize < 0)
                throw new ArgumentException("receiverQueueSize needs to be >= 0");
            _conf.ReceiverQueueSize = receiverQueueSize;
            return this;
        }

        public virtual IConsumerBuilder<T> AcknowledgmentGroupTime(long delay, TimeUnit.TimeUnit unit)
        {
            Condition.CheckArgument(delay >= 0, "acknowledgmentGroupTime needs to be >= 0");
            _conf.AcknowledgementsGroupTimeMicros = unit.ToMicroseconds(delay);
            return this;
        }
        public virtual IConsumerBuilder<T> MaxAcknowledgmentGroupSize(int messageNum)
        {
            Condition.CheckArgument(messageNum > 0, "acknowledgementsGroupSize needs to be > 0");
            _conf.MaxAcknowledgmentGroupSize = messageNum;
            return this;
        }

        public virtual IConsumerBuilder<T> ConsumerName(string consumerName)
        {
            if (string.IsNullOrWhiteSpace(consumerName))
                throw new ArgumentException("consumerName cannot be blank");
            _conf.ConsumerName = consumerName;
            return this;
        }

        public virtual IConsumerBuilder<T> PriorityLevel(int priorityLevel)
        {
            if (priorityLevel < 0)
                throw new ArgumentException("priorityLevel needs to be >= 0");
            _conf.PriorityLevel = priorityLevel;
            return this;
        }
        public virtual IConsumerBuilder<T> MaxPendingChunkedMessage(int maxPendingChunkedMessage)
        {
            //_conf.MaxPendingChunkedMessage = maxPendingChuckedMessage;
            return this;
        }
       
        public virtual IConsumerBuilder<T> Property(string key, string value)
        {
            if (string.IsNullOrWhiteSpace(key) && string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("property key/value cannot be blank");
            if (key != null) _conf.Properties.Add(key, value);
            return this;
        }

        public virtual IConsumerBuilder<T> Properties(IDictionary<string, string> properties)
        {
            if (properties.Count == 0)
                throw new ArgumentException("properties cannot be empty");
            properties.SetOfKeyValuePairs().ToList().ForEach(entry =>
            {
                if (entry.Key == null || entry.Value == null)
                    throw new ArgumentException("properties' key/value cannot be blank");
                if (string.IsNullOrWhiteSpace(entry.Key) || string.IsNullOrWhiteSpace(entry.Value))
                    throw new ArgumentException("properties' key/value cannot be blank");
                _conf.Properties.Add(entry.Key, entry.Value);

            });
            return this;
        }

        public virtual IConsumerBuilder<T> MaxTotalReceiverQueueSizeAcrossPartitions(int maxTotalReceiverQueueSizeAcrossPartitions)
        {
            if (maxTotalReceiverQueueSizeAcrossPartitions < 0)
                throw new ArgumentException("maxTotalReceiverQueueSizeAcrossPartitions needs to be >= 0");
            _conf.MaxTotalReceiverQueueSizeAcrossPartitions = maxTotalReceiverQueueSizeAcrossPartitions;
            return this;
        }

        public virtual IConsumerBuilder<T> ReadCompacted(bool readCompacted)
        {
            _conf.ReadCompacted = readCompacted;
            return this;
        }

        public virtual IConsumerBuilder<T> PatternAutoDiscoveryPeriod(int periodInSeconds)
        {
            if (periodInSeconds < 0)
                throw new ArgumentException("periodInMinutes needs to be >= 0");
            _conf.PatternAutoDiscoveryPeriod = periodInSeconds;
            return this;
        }
        public virtual IConsumerBuilder<T> PatternAutoDiscoveryPeriod(int interval, TimeUnit.TimeUnit unit)
        {
            Condition.CheckArgument(interval >= 0, "interval needs to be >= 0");
            int intervalSeconds = (int)unit.ToSeconds(interval);
            _conf.PatternAutoDiscoveryPeriod = intervalSeconds;
            return this;
        }


        public virtual IConsumerBuilder<T> SubscriptionInitialPosition(SubscriptionInitialPosition subscriptionInitialPosition)
        {
            _conf.SubscriptionInitialPosition = subscriptionInitialPosition;
            return this;
        }
        
        public virtual IConsumerBuilder<T> StartPaused(bool startPaused)
        {
            _conf.StartPaused = startPaused;
            return this;
        }
        public virtual IConsumerBuilder<T> SetPayloadProcessor(IMessagePayloadProcessor payloadProcessor)
        {
            _conf.PayloadProcessor = payloadProcessor;
            return this;
        }

        public virtual IConsumerBuilder<T> SubscriptionTopicsMode(RegexSubscriptionMode mode)
        {
            _conf.RegexSubscriptionMode = mode;
            return this;
        }
        
        public virtual IConsumerBuilder<T> ReplicateSubscriptionState(bool replicateSubscriptionState)
        {
            _conf.ReplicateSubscriptionState = replicateSubscriptionState;
            return this;
        }

        public virtual IConsumerBuilder<T> Intercept(params IConsumerInterceptor<T>[] interceptors)
        {
            if (_conf.Interceptors == null)
            {
                _conf.Interceptors = new List<IConsumerInterceptor<T>>();
            }

            _conf.Interceptors.AddRange(new List<IConsumerInterceptor<T>>(interceptors));
            return this;
        }

        public virtual IConsumerBuilder<T> DeadLetterPolicy(DeadLetterPolicy deadLetterPolicy)
        {
            if (deadLetterPolicy != null)
            {
                if (_conf.AckTimeoutMillis == 0)
                {
                    _conf.AckTimeoutMillis = TimeUnit.TimeUnit.MILLISECONDS.ToMilliseconds(_defaultAckTimeoutMillisForDeadLetter.Milliseconds);
                }
                _conf.DeadLetterPolicy = deadLetterPolicy;
            }
            return this;
        }
        public virtual IConsumerBuilder<T> EnableRetry(bool retryEnable)
        {
            _conf.RetryEnable = retryEnable;
            return this;
        }
        public virtual IConsumerBuilder<T> EnableBatchIndexAcknowledgment(bool batchIndexAcknowledgmentEnabled)
        {
            _conf.BatchIndexAckEnabled = batchIndexAcknowledgmentEnabled;
            return this;
        }

        public virtual IConsumerBuilder<T> AutoUpdatePartitionsInterval(TimeSpan timeSpan)
        {
            _conf.SetAutoUpdatePartitionsInterval(timeSpan);
            return this;
        }
        public IConsumerBuilder<T> AutoUpdatePartitionsInterval(int interval, TimeUnit.TimeUnit unit)
        {
            _conf.AutoUpdatePartitionsIntervalSeconds = unit.ToSeconds(interval);   
            return this;
        }
        public virtual IConsumerBuilder<T> AutoUpdatePartitions(bool autoUpdate)
        {
            _conf.AutoUpdatePartitions = autoUpdate;
            return this;
        }

        public virtual IConsumerBuilder<T> StartMessageIdInclusive()
        {
            _conf.ResetIncludeHead = true;
            return this;
        }

        public virtual IConsumerBuilder<T> BatchReceivePolicy(BatchReceivePolicy batchReceivePolicy)
        {
            if (batchReceivePolicy == null)
                throw new ArgumentException("batchReceivePolicy must not be null.");
            batchReceivePolicy.Verify();
            _conf.BatchReceivePolicy = batchReceivePolicy;
            return this;
        }

        public override string ToString()
        {
            return _conf?.ToString();
        }


        public virtual IConsumerBuilder<T> KeySharedPolicy(KeySharedPolicy keySharedPolicy)
        {
            keySharedPolicy.Validate();
            _conf.KeySharedPolicy = keySharedPolicy;
            return this;
        }
        public virtual IConsumerBuilder<T> ExpireTimeOfIncompleteChunkedMessage(long duration, TimeUnit.TimeUnit unit)
        {
            _conf.ExpireTimeOfIncompleteChunkedMessageMillis = unit.ToMilliseconds(duration));
            return this;
        }

        public virtual IConsumerBuilder<T> PoolMessages(bool poolMessages)
        {
           // _conf.PoolMessages = poolMessages;
            return this;
        }

        public virtual IConsumerBuilder<T> MessagePayloadProcessor(IMessagePayloadProcessor payloadProcessor)
        {
            _conf.PayloadProcessor = payloadProcessor;
            return this;
        }

        public virtual IConsumerBuilder<T> NegativeAckRedeliveryBackoff(IRedeliveryBackoff negativeAckRedeliveryBackoff)
        {
            Condition.CheckArgument(negativeAckRedeliveryBackoff != null, "negativeAckRedeliveryBackoff must not be null.");
            _conf.NegativeAckRedeliveryBackoff = negativeAckRedeliveryBackoff;
            return this;
        }

        public virtual IConsumerBuilder<T> AckTimeoutRedeliveryBackoff(IRedeliveryBackoff ackTimeoutRedeliveryBackoff)
        {
            Condition.CheckArgument(ackTimeoutRedeliveryBackoff != null, "ackTimeoutRedeliveryBackoff must not be null.");
            _conf.AckTimeoutRedeliveryBackoff = ackTimeoutRedeliveryBackoff;
            return this;
        }

        public virtual IConsumerBuilder<T> AutoScaledReceiverQueueSizeEnabled(bool enabled)
        {
            _conf.AutoScaledReceiverQueueSizeEnabled = enabled;
            return this;
        }

        public virtual ITopicConsumerBuilder<T> TopicConfiguration(string topicName)
        {
            TopicConsumerConfigurationData topicConf = TopicConsumerConfigurationData.OfTopicName(topicName, _conf);
            _conf.TopicConfigurations(topicConf);
            return new TopicConsumerBuilder<T>(this, topicConf);
        }

        public virtual IConsumerBuilder<T> TopicConfiguration(string topicName, Action<ITopicConsumerBuilder<T>> builderConsumer)
        {
            builderConsumer(TopicConfiguration(topicName));
            return this;
        }

        public virtual ITopicConsumerBuilder<T> TopicConfiguration(Regex topicsPattern)
        {
            TopicConsumerConfigurationData topicConf = TopicConsumerConfigurationData.OfTopicsPattern(topicsPattern.ToString(), _conf);
            _conf.TopicConfigurations(topicConf);
            return new TopicConsumerBuilder<T>(this, topicConf);
        }

        public virtual IConsumerBuilder<T> TopicConfiguration(Regex topicsPattern, Action<ITopicConsumerBuilder<T>> builderConsumer)
        {
            builderConsumer(TopicConfiguration(topicsPattern));
            return this;
        }

        public Action<T> Subscribe()
        {
            throw new NotImplementedException();
        }

        public ValueTask<Action<T>> SubscribeAsync()
        {
            throw new NotImplementedException();
        }
                
        object ICloneable.Clone()
        {
            throw new NotImplementedException();
        }
    }

}