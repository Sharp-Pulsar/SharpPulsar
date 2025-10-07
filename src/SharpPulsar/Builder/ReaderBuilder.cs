using System;
using System.Collections.Generic;
using System.Linq;
using Akka.Util.Internal;
using SharpPulsar.Batch;
using SharpPulsar.Configuration;
using SharpPulsar.Common.Precondition;
using Range = SharpPulsar.Common.Range;
using SharpPulsar.Shared;
using SharpPulsar.API;
using System.Threading.Tasks;
using SharpPulsar.Admin.v3;
using SharpPulsar.Messages;
using SharpPulsar.Shared.Exceptions;
using SharpPulsar.Crypto;

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
    public class ReaderBuilder<T>: IReaderBuilder<T>
    {
        private ReaderConfigurationData<T> _conf = new ReaderConfigurationData<T>();
        private readonly PulsarClient _client;
        private readonly ISchema<T> _schema;

        public ReaderBuilder(PulsarClient client, ISchema<T> schema) : this(client, new ReaderConfigurationData<T>(), schema)
        {
        }

        private ReaderBuilder(PulsarClient client, ReaderConfigurationData<T> conf, ISchema<T> schema)
        {
            _client = client;
            _conf = conf;
            _schema = schema;
        }

        public ReaderConfigurationData<T> ReaderConfigurationData
        {
            get
            {
                return _conf;
            }
        }
        public virtual IReaderBuilder<T> EventListener(IConsumerEventListener consumerEventListener)
        {
            _conf.EventListener = consumerEventListener;
            return this;
        }
        public virtual IReaderBuilder<T> Clone()
        {
            return new ReaderBuilder<T>(_client, _conf.Clone(), _schema);
        }

        public virtual IReader<T> Create()
        {
            try
            {
                return CreateAsync().GetAwaiter().GetResult();
            }
            catch (Exception e)
            {
                throw PulsarClientException.Unwrap(e);
            }
        }

        public virtual async ValueTask<IReader<T>> CreateAsync()
        {
            if (_conf.TopicNames.Count == 0)
            {
                throw new ArgumentException("Topic name must be set on the reader builder");
            }

            var isStartMsgIdExist = _conf.StartMessageId != null && _conf.StartMessageId != IMessageId.Earliest;
            if ((isStartMsgIdExist && _conf.StartMessageFromRollbackDurationInSec > 0) || (_conf.StartMessageId == null && _conf.StartMessageFromRollbackDurationInSec <= 0))
            {
                throw new System.ArgumentException("Start message id or start message from roll back must be specified but they cannot be" + " specified at the same time. MessageId =" + _conf.StartMessageId + ", rollback seconds =" + _conf.StartMessageFromRollbackDurationInSec);
            }

            if (_conf.StartMessageFromRollbackDurationInSec > 0)
            {
                _conf.StartMessageId = IMessageId.Earliest);
            }

            return _client.CreateReaderAsync(_conf, _schema);
        }

        public virtual IReaderBuilder<T> LoadConf(IDictionary<string, object> config)
        {
            var startMessageId = _conf.StartMessageId;
            _conf = (ReaderConfigurationData<T>)ConfigurationDataUtils.LoadData(config, _conf);
            _conf.StartMessageId = startMessageId;
            return this;
        }
        public virtual IReaderBuilder<T> KeyHashRange(params Shared.Range[] ranges)
        {
            Condition.CheckArgument(ranges != null && ranges.Length > 0, "Cannot specify a null ofr an empty key hash ranges for a reader");
            for (var i = 0; i < ranges.Length; i++)
            {
                var range1 = ranges[i];
                if (range1.Start < 0 || range1.End > KeySharedPolicy.DefaultHashRangeSize)
                {
                    throw new ArgumentException("Ranges must be [0, 65535] but provided range is " + range1);
                }
                for (var j = 0; j < ranges.Length; j++)
                {
                    var range2 = ranges[j];
                    if (i != j && range1.Intersect(range2) != null)
                    {
                        throw new ArgumentException("Key hash ranges with overlap between " + range1 + " and " + range2);
                    }
                }
            }
            _conf.KeyHashRanges = new List<Shared.Range>(ranges);
            return this;
        }
        public virtual IReaderBuilder<T> CryptoKeyReader(ICryptoKeyReader cryptoKeyReader)
        {
            _conf.CryptoKeyReader = cryptoKeyReader;
            return this;
        }
        public virtual IReaderBuilder<T> DefaultCryptoKeyReader(string privateKey)
        {
            Condition.CheckArgument(!string.IsNullOrWhiteSpace(privateKey), "privateKey cannot be blank");
            return CryptoKeyReader(Crypto.DefaultCryptoKeyReader.Builder().DefaultPrivateKey(privateKey).Build());
        }
        public virtual IReaderBuilder<T> DefaultCryptoKeyReader(IDictionary<string, string> privateKeys)
        {
            Condition.CheckArgument(privateKeys.Count > 0, "privateKeys cannot be empty");
            return CryptoKeyReader(Crypto.DefaultCryptoKeyReader.Builder().PrivateKeys(privateKeys).Build());
        }

        public virtual IReaderBuilder<T> CryptoFailureAction(ConsumerCryptoFailureAction action)
        {
            _conf.CryptoFailureAction = action;
            return this;
        }
        public virtual IReaderBuilder<T> Topic(string topicName)
        {
            _conf.TopicName = topicName.Trim();
            return this;
        }
        public virtual IReaderBuilder<T> Topics(IList<string> topicNames)
        {
            Condition.CheckArgument(topicNames != null && topicNames.Count > 0, "Passed in topicNames should not be null or empty.");
            topicNames.ForEach(topicName => Condition.CheckArgument(!string.IsNullOrWhiteSpace(topicName), "topicNames cannot have blank topic"));
            _conf.TopicNames.AddRange(topicNames.Select(x => x.Trim()).ToList());
            return this;
        }
        
        public virtual IReaderBuilder<T> StartMessageId(IMessageId id)
        {
            _conf.StartMessageId = id;

            return this;
        }

        public virtual IReaderBuilder<T> StartMessageFromRollbackDuration(int rollbackDurationSecs)
        {
            _conf.StartMessageFromRollbackDurationInSec = rollbackDurationSecs;
            return this;
        }

        public virtual IReaderBuilder<T> StartMessageIdInclusive()
        {
            _conf.ResetIncludeHead = true;
            return this;
        }

        public virtual IReaderBuilder<T> ReaderListener(IReaderListener<T> readerListener)
        {
            _conf.ReaderListener = readerListener;
            return this;
        }

        public virtual IReaderBuilder<T> ReceiverQueueSize(int receiverQueueSize)
        {
            Condition.CheckArgument(receiverQueueSize >= 0, "receiverQueueSize needs to be >= 0");
            _conf.ReceiverQueueSize = receiverQueueSize;
            return this;
        }

        public virtual IReaderBuilder<T> ReaderName(string readerName)
        {
            _conf.ReaderName = readerName;
            return this;
        }

        public virtual IReaderBuilder<T> SubscriptionRolePrefix(string subscriptionRolePrefix)
        {
            _conf.SubscriptionRolePrefix = subscriptionRolePrefix;
            return this;
        }

        public virtual IReaderBuilder<T> ReadCompacted(bool readCompacted)
        {
            _conf.ReadCompacted = readCompacted;
            return this;
        }
        public virtual IReaderBuilder<T> Schema(ISchema<T> schema)
        {
            if (schema == null)
                throw new ArgumentException("Schema is null");
            _conf.Schema = schema;
            return this;
        }
        
        public virtual IReaderBuilder<T> PoolMessages(bool poolMessages)
        {
            _conf.PoolMessages = (poolMessages);
            return this;
        }

       
        public virtual IReaderBuilder<T> AutoUpdatePartitions(bool autoUpdate)
        {
            _conf.AutoUpdatePartitions = autoUpdate;
            return this;
        }

       
        public virtual IReaderBuilder<T> AutoUpdatePartitionsInterval(long interval, TimeUnit.TimeUnit unit)
        {
            var intervalSeconds = unit.ToSeconds(interval);
            Condition.CheckArgument(intervalSeconds >= 1, "Auto update partition interval needs to be >= 1 second");
            _conf.AutoUpdatePartitionsIntervalSeconds = intervalSeconds;
            return this;
        }

        public virtual IReaderBuilder<T> Intercept(params IReaderInterceptor<T>[] interceptors)
        {
            if (interceptors != null)
            {
                _conf.ReaderInterceptorList =  interceptors;
            }
            return this;
        }

        
        public virtual IReaderBuilder<T> MaxPendingChunkedMessage(int maxPendingChunkedMessage)
        {
            _conf.MaxPendingChunkedMessage = maxPendingChunkedMessage;
            return this;
        }

        
        public virtual IReaderBuilder<T> AutoAckOldestChunkedMessageOnQueueFull(bool autoAckOldestChunkedMessageOnQueueFull)
        {
            _conf.AutoAckOldestChunkedMessageOnQueueFull = autoAckOldestChunkedMessageOnQueueFull;
            return this;
        }

       
        public virtual IReaderBuilder<T> ExpireTimeOfIncompleteChunkedMessage(long duration, TimeUnit.TimeUnit unit)
        {
            _conf.ExpireTimeOfIncompleteChunkedMessageMillis = unit.ToMilliseconds(duration);   
            return this;
        }

        public virtual IReaderBuilder<T> StartMessageFromRollbackDuration(long rollbackDuration, TimeUnit.TimeUnit timeunit)
        {
            _conf.StartMessageFromRollbackDurationInSec = timeunit.ToSeconds(rollbackDuration);
            return this;
        }

        public virtual IReaderBuilder<T> MessageCrypto(MessageCrypto messageCrypto)
        {
            _conf.MessageCrypto = messageCrypto;
            return this;
        }

        public virtual IReaderBuilder<T> SubscriptionName(string subscriptionName)
        {
            _conf.SubscriptionName = subscriptionName;
            return this;
        }

        object ICloneable.Clone()
        {
            throw new NotImplementedException();
        }
    }

}