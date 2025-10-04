
using System;
using System.Collections.Generic;
using SharpPulsar.API;

/*
 * Licensed to the Apache Software Foundation (ASF) under one
 * or more contributor license agreements.  See the NOTICE file
 * distributed with this work for additional information
 * regarding copyright ownership.  The ASF licenses this file
 * to you under the Apache License, Version 2.0 (the
 * "License"); you may not use this file except in compliance
 * with the License.  You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing,
 * software distributed under the License is distributed on an
 * "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
 * KIND, either express or implied.  See the License for the
 * specific language governing permissions and limitations
 * under the License.
 */
namespace SharpPulsar.Builder
{
    
    public class ProducerBuilder<T> : IProducerBuilder<T>
    {

        private readonly IPulsarClient _client;
        private ProducerConfigurationData _conf = new ProducerConfigurationData();
        private ISchema<T> _schema_Conflict;
        private IList<IProducerInterceptor<T>> _interceptorList;

        public ProducerBuilder(IPulsarClient client, ISchema<T> schema) : this(client, new ProducerConfigurationData(), schema)
        {
        }

        private ProducerBuilder(IPulsarClient client, ProducerConfigurationData conf, ISchema<T> schema)
        {
            _client = client;
            _conf = conf;
            _schema_Conflict = schema;
        }

        /// <summary>
        /// Allow to override schema in builder implementation.
        /// @return
        /// </summary>
        public virtual IProducerBuilder<T> Schema(ISchema<T> schema)
        {
            _schema_Conflict = schema;
            return this;
        }

        public virtual IProducerBuilder<T> Clone()
        {
            return new ProducerBuilder<T>(_client, _conf.Clone(), _schema_Conflict);
        }

        public virtual IProducer<T> Create()
        {
            try
            {
                return FutureUtil.getAndCleanupOnInterrupt(createAsync(), Producer.closeAsync);
            }
            catch (Exception e)
            {
                throw PulsarClientException.Unwrap(e);
            }
        }

        public virtual ValueTask<IProducer<T>> CreateAsync()
        {
            // config validation
            Condition.CheckArgument(!(_conf.BatchingEnabled && _conf.ChunkingEnabled), "Batching and chunking of messages can't be enabled together");
            if (_conf.TopicName == null)
            {
                return FutureUtil.failedFuture(new System.ArgumentException("Topic name must be set on the producer builder"));
            }

            try
            {
                SetMessageRoutingMode();
            }
            catch (PulsarClientException pce)
            {
                return FutureUtil.failedFuture(pce);
            }

            return _interceptorList == null || _interceptorList.Count == 0 ? _client.CreateProducerAsync(_conf, _schema_Conflict, null) : _client.CreateProducerAsync(_conf, _schema_Conflict, new ProducerInterceptors(_interceptorList));
        }

        public virtual IProducerBuilder<T> LoadConf(IDictionary<string, object> config)
        {
            _conf = (ProducerConfigurationData)ConfigurationDataUtils.LoadData(config, _conf);
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

        public virtual IProducerBuilder<T> SendTimeout(int sendTimeout, TimeUnit.TimeUnit unit)
        {
            _conf.SendTimeoutMs = TimeSpan.FromMilliseconds(unit.ToMilliseconds(sendTimeout));
            return this;
        }

        public virtual IProducerBuilder<T> MaxPendingMessages(int maxPendingMessages)
        {
            _conf.MaxPendingMessages = maxPendingMessages;
            return this;
        }

        [Obsolete]
        public virtual IProducerBuilder<T> MaxPendingMessagesAcrossPartitions(int maxPendingMessagesAcrossPartitions)
        {
            _conf.MaxPendingMessagesAcrossPartitions = maxPendingMessagesAcrossPartitions;
            return this;
        }

        public virtual IProducerBuilder<T> AccessMode(ProducerAccessMode accessMode)
        {
            _conf.AccessMode = accessMode;
            return this;
        }

        public virtual IProducerBuilder<T> BlockIfQueueFull(bool blockIfQueueFull)
        {
            _conf.BlockIfQueueFull = blockIfQueueFull;
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

        public override ProducerBuilder<T> hashingScheme(HashingScheme hashingScheme)
        {
            conf.setHashingScheme(hashingScheme);
            return this;
        }

        public override ProducerBuilder<T> messageRouter(MessageRouter messageRouter)
        {
            conf.setCustomMessageRouter(messageRouter);
            return this;
        }

        public override ProducerBuilder<T> enableBatching(bool batchMessagesEnabled)
        {
            conf.setBatchingEnabled(batchMessagesEnabled);
            return this;
        }

        public override ProducerBuilder<T> enableChunking(bool chunkingEnabled)
        {
            conf.setChunkingEnabled(chunkingEnabled);
            return this;
        }

        public override ProducerBuilder<T> chunkMaxMessageSize(int chunkMaxMessageSize)
        {
            conf.setChunkMaxMessageSize(chunkMaxMessageSize);
            return this;
        }

        
        public override ProducerBuilder<T> cryptoKeyReader(CryptoKeyReader cryptoKeyReader)
        {
            conf.setCryptoKeyReader(cryptoKeyReader);
            return this;
        }

        public override ProducerBuilder<T> defaultCryptoKeyReader(string publicKey)
        {
            checkArgument(StringUtils.isNotBlank(publicKey), "publicKey cannot be blank");
            return cryptoKeyReader(DefaultCryptoKeyReader.builder().defaultPublicKey(publicKey).build());
        }

        public override ProducerBuilder<T> defaultCryptoKeyReader(IDictionary<string, string> publicKeys)
        {
            checkArgument(publicKeys.Count > 0, "publicKeys cannot be empty");
            return cryptoKeyReader(DefaultCryptoKeyReader.builder().publicKeys(publicKeys).build());
        }

        public override ProducerBuilder<T> messageCrypto(MessageCrypto messageCrypto)
        {
            conf.setMessageCrypto(messageCrypto);
            return this;
        }

        public override ProducerBuilder<T> addEncryptionKey(string key)
        {
            checkArgument(StringUtils.isNotBlank(key), "Encryption key cannot be blank");
            conf.getEncryptionKeys().add(key);
            return this;
        }

        public override ProducerBuilder<T> cryptoFailureAction(ProducerCryptoFailureAction action)
        {
            conf.setCryptoFailureAction(action);
            return this;
        }

        public override ProducerBuilder<T> batchingMaxPublishDelay(long batchDelay, TimeUnit timeUnit)
        {
            conf.setBatchingMaxPublishDelayMicros(batchDelay, timeUnit);
            return this;
        }

        public override ProducerBuilder<T> roundRobinRouterBatchingPartitionSwitchFrequency(int frequency)
        {
            conf.setBatchingPartitionSwitchFrequencyByPublishDelay(frequency);
            return this;
        }

        public override ProducerBuilder<T> batchingMaxMessages(int batchMessagesMaxMessagesPerBatch)
        {
            conf.setBatchingMaxMessages(batchMessagesMaxMessagesPerBatch);
            return this;
        }

        public override ProducerBuilder<T> batchingMaxBytes(int batchingMaxBytes)
        {
            conf.setBatchingMaxBytes(batchingMaxBytes);
            return this;
        }

        public override ProducerBuilder<T> batcherBuilder(BatcherBuilder batcherBuilder)
        {
            conf.setBatcherBuilder(batcherBuilder);
            return this;
        }


        public override ProducerBuilder<T> initialSequenceId(long initialSequenceId)
        {
            conf.setInitialSequenceId(initialSequenceId);
            return this;
        }

        public override ProducerBuilder<T> property(string key, string value)
        {
            checkArgument(StringUtils.isNotBlank(key) && StringUtils.isNotBlank(value), "property key/value cannot be blank");
            conf.getProperties().put(key, value);
            return this;
        }

        public override ProducerBuilder<T> properties(IDictionary<string, string> properties)
        {
            properties.SetOfKeyValuePairs().forEach(entry => checkArgument(StringUtils.isNotBlank(entry.getKey()) && StringUtils.isNotBlank(entry.getValue()), "properties' key/value cannot be blank"));
            conf.getProperties().putAll(properties);
            return this;
        }

        public override ProducerBuilder<T> intercept(params ProducerInterceptor[] interceptors)
        {
            if (interceptorList == null)
            {
                interceptorList = new List<ProducerInterceptor>();
            }
            ((List<ProducerInterceptor>)interceptorList).AddRange(new List<ProducerInterceptor> { interceptors });
            return this;
        }

        [Obsolete]
        public override ProducerBuilder<T> intercept(params org.apache.pulsar.client.api.ProducerInterceptor<T>[] interceptors)
        {
            if (interceptorList == null)
            {
                interceptorList = new List<ProducerInterceptor>();
            }
            //JAVA TO C# CONVERTER TASK: Method reference constructor syntax is not converted by Java to C# Converter:
            ((List<org.apache.pulsar.client.api.interceptor.ProducerInterceptor>)interceptorList).AddRange(java.util.Arrays.stream(interceptors).map(org.apache.pulsar.client.api.interceptor.ProducerInterceptorWrapper::new).collect(java.util.stream.Collectors.toList()));
            return this;
        }
        public override ProducerBuilder<T> autoUpdatePartitions(bool autoUpdate)
        {
            conf.setAutoUpdatePartitions(autoUpdate);
            return this;
        }

        public override ProducerBuilder<T> autoUpdatePartitionsInterval(int interval, TimeUnit unit)
        {
            conf.setAutoUpdatePartitionsIntervalSeconds(interval, unit);
            return this;
        }

        public override ProducerBuilder<T> enableMultiSchema(bool multiSchema)
        {
            conf.setMultiSchema(multiSchema);
            return this;
        }

        public override ProducerBuilder<T> enableLazyStartPartitionedProducers(bool lazyStartPartitionedProducers)
        {
            conf.setLazyStartPartitionedProducers(lazyStartPartitionedProducers);
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
        public virtual ProducerBuilderImpl<T> initialSubscriptionName(string initialSubscriptionName)
        {
            conf.setInitialSubscriptionName(initialSubscriptionName);
            return this;
        }

        private void setMessageRoutingMode()
        {
            if (conf.getMessageRoutingMode() == null && conf.getCustomMessageRouter() == null)
            {
                messageRoutingMode(MessageRoutingMode.RoundRobinPartition);
            }
            else if (conf.getMessageRoutingMode() == null && conf.getCustomMessageRouter() != null)
            {
                messageRoutingMode(MessageRoutingMode.CustomPartition);
            }
            else if (conf.getMessageRoutingMode() == MessageRoutingMode.CustomPartition && conf.getCustomMessageRouter() == null)
            {
                throw new PulsarClientException("When 'messageRoutingMode' is " + MessageRoutingMode.CustomPartition + ", 'messageRouter' should be set");
            }
            else if (conf.getMessageRoutingMode() != MessageRoutingMode.CustomPartition && conf.getCustomMessageRouter() != null)
            {
                throw new PulsarClientException("When 'messageRouter' is set, 'messageRoutingMode' " + "should be set as " + MessageRoutingMode.CustomPartition);
            }
        }

        public override string ToString()
        {
            return conf != null ? conf.ToString() : "";
        }
    }
}


