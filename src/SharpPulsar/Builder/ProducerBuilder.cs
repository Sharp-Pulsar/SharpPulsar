using System;
using System.Collections.Generic;
using System.Linq;
using SharpPulsar.Messages;

/* Unmerged change from project 'SharpPulsar (net5.0)'
Before:
using SharpPulsar.Extension;
After:
using SharpPulsar.Extension;
using SharpPulsar;
using SharpPulsar.Configuration;
using SharpPulsar.Builder;
*/
using SharpPulsar.Configuration;
using SharpPulsar.Crypto;
using SharpPulsar.Common.Precondition;
using SharpPulsar.API;
using SharpPulsar.Shared;
using SharpPulsar.API.Interceptor;
using SharpPulsar.Shared.Exceptions;
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

    public class ProducerBuilder<T>: IProducerBuilder<T>
    {
        private ProducerConfigurationData _conf;
        private ISchema<T> _schema;
        private List<IProducerInterceptor<T>> _interceptorList;
        private readonly PulsarClient _client;

        public ProducerBuilder(PulsarClient client, ISchema<T> schema) : this(client, new ProducerConfigurationData(), schema)
        {
        }

        private ProducerBuilder(PulsarClient client, ProducerConfigurationData conf, ISchema<T> schema)
        {
            _client = client;
            _conf = conf;
            _schema = schema;
        }

        /// <summary>
        /// Allow to override schema in builder implementation.
        /// @return
        /// </summary>
        public virtual IProducerBuilder<T> Schema(ISchema<T> schema)
        {
            _schema = schema;
            return this;
        }

        public virtual IProducerBuilder<T> Clone()
        {
            return new ProducerBuilder<T>(_client, _conf.Clone(), _schema);
        }
        public virtual IProducer<T> Create()
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

        public virtual async ValueTask<IProducer<T>> CreateAsync()
        {
            // config validation
            Condition.CheckArgument(!(_conf.BatchingEnabled && _conf.ChunkingEnabled), "Batching and chunking of messages can't be enabled together");
            if (_conf.TopicName == null)
            {
               throw new ArgumentException("Topic name must be set on the producer builder");
            }

            try
            {
                SetMessageRoutingMode();
            }
            catch (PulsarClientException)
            {
                throw;
            }

            return _interceptorList == null || _interceptorList.Count == 0 ? await _client.CreateProducerAsync(_conf, _schema, null) : await _client.CreateProducerAsync(_conf, _schema, new ProducerInterceptors(_interceptorList));
        }
        private void SetMessageRoutingMode()
        {
            if (_conf.MessageRoutingMode == null && _conf.CustomMessageRouter == null)
            {
                MessageRoutingMode(Shared.MessageRoutingMode.RoundRobinMode);
            }
            else if (_conf.MessageRoutingMode == null && _conf.CustomMessageRouter != null)
            {
                //MessageRoutingMode(Shared.MessageRoutingMode.CustomPartition);
            }
            /*else if (_conf.MessageRoutingMode() == MessageRoutingMode.CustomPartition && conf.getCustomMessageRouter() == null)
            {
                throw new PulsarClientException("When 'messageRoutingMode' is " + MessageRoutingMode.CustomPartition + ", 'messageRouter' should be set");
            }*/
            else if (_conf.MessageRoutingMode != Shared.MessageRoutingMode.RandomMode && _conf.CustomMessageRouter != null)
            {
                throw new PulsarClientException("When 'messageRouter' is set, 'messageRoutingMode' " + "should be set as " + Shared.MessageRoutingMode.RandomMode);
            }
        }


        public IProducerBuilder<T> LoadConf(IDictionary<string, object> config)
        {
            _conf = (ProducerConfigurationData)ConfigurationDataUtils.LoadData(config, _conf);
            return this;
        }
        public IProducerBuilder<T> EventListener(IProducerEventListener listener)
        {
            if (listener == null)
                throw new ArgumentException("listener is null");
            _conf.ProducerEventListener = listener;
            return this;
        }
        /// <summary>
		/// MaxMessageSize is set at the server side,
		/// But when we need a smaller size than the size set by the server when chunking
		/// we can do it here
		/// </summary>
        public virtual IProducerBuilder<T> ChunkMaxMessageSize(int max)
        {
            if (max < 1)
                throw new ArgumentException("max should be > 0");
            _conf.ChunkMaxMessageSize = max;
            return this;
        }
        public virtual IProducerBuilder<T> Topic(string topicName)
        {
            if (string.IsNullOrWhiteSpace(topicName))
                throw new ArgumentException("topicName cannot be blank or null");
            _conf.TopicName = topicName.Trim();
            return this;
        }

        public virtual IProducerBuilder<T> ProducerName(string producerName)
        {
            _conf.ProducerName = producerName;
            return this;
        }
        public virtual IProducerBuilder<T> AccessMode(ProducerAccessMode accessMode)
        {
            _conf.AccessMode = accessMode;
            return this;
        }
        public virtual IProducerBuilder<T> EnableBatching(bool enableBatching)
        {
            _conf.BatchingEnabled = enableBatching;
            return this;
        }
        /// <summary>
        /// When batching is enabled, AckReceiveListerner helps to capture acks
        /// </summary>
        /// <param name="listerner"></param>
        /// <returns></returns>
        public virtual IProducerBuilder<T> SetAckReceivedListerner(Action<AckReceived> listerner)
        {
            if (!_conf.BatchingEnabled)
                throw new InvalidOperationException("AckReceived Listerner is only allowed for batched producer!");

            _conf.AckReceivedListerner = listerner;
            return this;
        }
        public virtual IProducerBuilder<T> BatcherBuilder(IBatcherBuilder batcherBuilder)
        {
            _conf.BatcherBuilder = batcherBuilder;
            return this;
        }
        public virtual IProducerBuilder<T> BatchingMaxPublishDelay(TimeSpan batchDelay)
        {
            _conf.SetBatchingMaxPublishDelayMs(batchDelay);
            return this;
        }

        public virtual IProducerBuilder<T> BatchingMaxMessages(int batchingMaxMessages)
        {
            _conf.BatchingMaxMessages = batchingMaxMessages;
            return this;
        }

        public virtual IProducerBuilder<T> SendTimeout(TimeSpan sendTimeoutMs)
        {
            _conf.SetSendTimeoutMs(sendTimeoutMs);
            return this;
        }
        public virtual IProducerBuilder<T> BlockIfQueueFull(bool blockIfQueueFull)
        {
            _conf.BlockIfQueueFull = blockIfQueueFull;
            return this;
        }
        public virtual IProducerBuilder<T> RoundRobinRouterBatchingPartitionSwitchFrequency(int frequency)
        {
            _conf.BatchingPartitionSwitchFrequencyByPublishDelay = frequency;
            return this;
        }
        public virtual IProducerBuilder<T> BatchingMaxBytes(int batchingMaxBytes)
        {
            _conf.BatchingMaxBytes = batchingMaxBytes;
            return this;
        }

        public virtual IProducerBuilder<T> MaxPendingMessages(int maxPendingMessages)
        {
            _conf.MaxPendingMessages = maxPendingMessages;
            return this;
        }

        public virtual IProducerBuilder<T> MaxPendingMessagesAcrossPartitions(int maxPendingMessagesAcrossPartitions)
        {
            _conf.MaxPendingMessagesAcrossPartitions = maxPendingMessagesAcrossPartitions;
            return this;
        }

        public virtual IProducerBuilder<T> EnableChunking(bool chunk)
        {
            _conf.ChunkingEnabled = chunk;
            return this;
        }

        public virtual IProducerBuilder<T> MessageRoutingMode(MessageRoutingMode messageRouteMode)
        {
            _conf.MessageRoutingMode = messageRouteMode;
            return this;
        }

        public virtual IProducerBuilder<T> CompressionType(CompressionType compressionType)
        {
            _conf.CompressionType = compressionType;
            return this;
        }
        public virtual IProducerBuilder<T> CompressionMinMsgBodySize(int compressionMinMsgBodySize)
        {
            _conf.CompressMinMsgBodySize = compressionMinMsgBodySize;
            return this;
        }

        public virtual IProducerBuilder<T> HashingScheme(HashingScheme hashingScheme)
        {
            _conf.HashingScheme = hashingScheme;
            return this;
        }
        public virtual IProducerBuilder<T> MessageRouter(IMessageRouter messageRouter)
        {
            _conf.CustomMessageRouter = messageRouter;
            return this;
        }

        public virtual IProducerBuilder<T> CryptoKeyReader(ICryptoKeyReader cryptoKeyReader)
        {
            _conf.CryptoKeyReader = cryptoKeyReader;
            return this;
        }
        public virtual IProducerBuilder<T> DefaultCryptoKeyReader(string publicKey)
        {
            Condition.CheckArgument(!string.IsNullOrWhiteSpace(publicKey), "publicKey cannot be blank");
            return CryptoKeyReader(Crypto.DefaultCryptoKeyReader.Builder().DefaultPublicKey(publicKey).Build());
        }
        public virtual IProducerBuilder<T> DefaultCryptoKeyReader(IDictionary<string, string> publicKeys)
        {
            Condition.CheckArgument(publicKeys.Count > 0, "publicKeys cannot be empty");
            return CryptoKeyReader(Crypto.DefaultCryptoKeyReader.Builder().PublicKeys(publicKeys).Build());
        }
        public virtual IProducerBuilder<T> MessageCrypto(MessageCrypto messageCrypto)
        {
            _conf.MessageCrypto = messageCrypto;
            return this;
        }

        public virtual IProducerBuilder<T> AddEncryptionKey(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Encryption key cannot be blank or null");
            _conf.EncryptionKeys.Add(key);
            return this;
        }
        /// <summary>
		/// Use this config to automatically create an initial subscription when creating the topic.
		/// If this field is not set, the initial subscription will not be created.
		/// If this field is set but the broker's `allowAutoSubscriptionCreation` is disabled, the producer will fail to
		/// be created.
		/// This method is limited to internal use. This method will only be used when the consumer creates the dlq producer.
		/// </summary>
		/// <param name="initialSubscriptionName"> Name of the initial subscription of the topic. </param>
		/// <returns> the producer builder implementation instance </returns>
        public virtual IProducerBuilder<T> InitialSubscriptionName(string initialSubscriptionName)
        {
            _conf.InitialSubscriptionName = initialSubscriptionName;
            return this;
        }

        public virtual IProducerBuilder<T> CryptoFailureAction(ProducerCryptoFailureAction action)
        {
            _conf.CryptoFailureAction = action;
            return this;
        }


        public virtual IProducerBuilder<T> InitialSequenceId(long initialSequenceId)
        {
            _conf.InitialSequenceId = initialSequenceId;
            return this;
        }

        public virtual IProducerBuilder<T> Property(string key, string value)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("property key cannot be blank or null");
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("property value cannot be blank or null");
            _conf.Properties.Add(key, value);
            return this;
        }

        public virtual IProducerBuilder<T> Properties(IDictionary<string, string> properties)
        {
            if (properties == null)
                throw new ArgumentException("properties cannot be null");
            if (properties.Count == 0)
                throw new ArgumentException("properties cannot be empty");
            properties.SetOfKeyValuePairs().ToList().ForEach(entry =>
            {
                var (key, value) = entry;
                if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(value))
                {
                    throw new ArgumentException("properties' key/value cannot be blank");
                }

                _conf.Properties.Add(key, value);
            });
            return this;
        }


        public virtual IProducerBuilder<T> Intercept(params IProducerInterceptor<T>[] interceptors)
        {
            if (_interceptorList == null)
            {
                _interceptorList = new List<IProducerInterceptor<T>>();
            }
            _interceptorList.AddRange(interceptors);
            return this;
        }
        public virtual IProducerBuilder<T> AutoUpdatePartitions(bool autoUpdate)
        {
            _conf.AutoUpdatePartitions = autoUpdate;
            return this;
        }
        public virtual IProducerBuilder<T> AutoUpdatePartitionsInterval(TimeSpan interval)
        {
            _conf.SetAutoUpdatePartitionsIntervalSeconds(interval);
            return this;
        }

        public List<IProducerInterceptor<T>> GetInterceptors => _interceptorList;

        public ISchema<T> GetSchema => _schema;

        public virtual IProducerBuilder<T> EnableMultiSchema(bool multiSchema)
        {
            _conf.MultiSchema = multiSchema;
            return this;
        }
        /// <summary>
        /// This config affects Shared mode producers of partitioned topics only.It controls whether
        /// producers register and connect immediately to the owner broker of each partition
       /// or start lazily on demand.The internal producer of one partition is always
       /// started eagerly, chosen by the routing policy, but the internal producers of
        ///any additional partitions are started on demand, upon receiving their first
     ///* message.
     ///* Using this mode can reduce the strain on brokers for topics with large numbers of
     ///* partitions and when the SinglePartition or some custom partial partition routing policy
     ///* like PartialRoundRobinMessageRouterImpl is used without keyed messages.
     ///* Because producer connection can be on demand, this can produce extra send latency
     ///* for the first messages of a given partition.
     ///*
     ///* @param lazyStartPartitionedProducers
     ///*            true/false as to whether to start partition producers lazily
     ///* @return the producer builder instance
    
        /// </summary>
        /// <param name="enableLazyStartPartitionedProducers"></param>
        /// <returns></returns>
        public virtual IProducerBuilder<T> EnableLazyStartPartitionedProducers(bool lazyStartPartitionedProducers)
        {
            _conf.LazyStartPartitionedProducers =  lazyStartPartitionedProducers;
            return this;
        }
        public override string ToString()
        {
            return _conf?.ToString();
        }

        
        public IProducerBuilder<T> MessageCrypto<M, B>(IMessageCrypto<M, B> messageCrypto)
        {
            throw new NotImplementedException();
        }

        object ICloneable.Clone()
        {
            throw new NotImplementedException();
        }
    }

    public interface IProducerEventListener
    {
        public void MessageSent(SentReceipt receipt);
        public void Log(object log);
    }
}