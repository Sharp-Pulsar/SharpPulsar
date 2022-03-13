using System;
using System.Collections.Generic;
using System.Linq;
using SharpPulsar.Messages;

using SharpPulsar.Batch.Api;
using SharpPulsar.Interfaces.Interceptor;

using SharpPulsar.Interfaces;
using SharpPulsar.Common;
using SharpPulsar.Protocol.Proto;

/* Unmerged change from project 'SharpPulsar (net5.0)'
Before:
using SharpPulsar.Extension;
After:
using SharpPulsar.Extension;
using SharpPulsar;
using SharpPulsar.Configuration;
using SharpPulsar.Builder;
*/
using SharpPulsar.Extension;
using SharpPulsar.Configuration;

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

    public class ProducerConfigBuilder<T>
    {
        private ProducerConfigurationData _conf = new ProducerConfigurationData();
        private ISchema<T> _schema;
        private List<IProducerInterceptor<T>> _interceptorList;

        public virtual ProducerConfigurationData Build()
        {
            return _conf;
        }
        public ProducerConfigBuilder<T> LoadConf(IDictionary<string, object> config)
        {
            _conf = (ProducerConfigurationData)ConfigurationDataUtils.LoadData(config, _conf);
            return this;
        }
        public ProducerConfigBuilder<T> EventListener(IProducerEventListener listener)
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
        public ProducerConfigBuilder<T> MaxMessageSize(int max)
        {
            if (max < 1)
                throw new ArgumentException("max should be > 0");
            _conf.MaxMessageSize = max;
            return this;
        }
        public ProducerConfigBuilder<T> Topic(string topicName)
        {
            if (string.IsNullOrWhiteSpace(topicName))
                throw new ArgumentException("topicName cannot be blank or null");
            _conf.TopicName = topicName.Trim();
            return this;
        }

        public ProducerConfigBuilder<T> ProducerName(string producerName)
        {
            _conf.ProducerName = producerName;
            return this;
        }
        public ProducerConfigBuilder<T> AccessMode(Common.ProducerAccessMode accessMode)
        {
            _conf.AccessMode = accessMode;
            return this;
        }
        public ProducerConfigBuilder<T> EnableBatching(bool enableBatching)
        {
            _conf.BatchingEnabled = enableBatching;
            return this;
        }
        /// <summary>
        /// When batching is enabled, AckReceiveListerner helps to capture acks
        /// </summary>
        /// <param name="listerner"></param>
        /// <returns></returns>
        public ProducerConfigBuilder<T> SetAckReceivedListerner(Action<AckReceived> listerner)
        {
            if (!_conf.BatchingEnabled)
                throw new InvalidOperationException("AckReceived Listerner is only allowed for batched producer!");

            _conf.AckReceivedListerner = listerner;
            return this;
        }
        public ProducerConfigBuilder<T> BatchBuilder(IBatcherBuilder builder)
        {
            _conf.BatcherBuilder = builder;
            return this;
        }
        public ProducerConfigBuilder<T> BatchingMaxPublishDelay(TimeSpan batchDelay)
        {
            _conf.SetBatchingMaxPublishDelayMs(batchDelay);
            return this;
        }

        public ProducerConfigBuilder<T> BatchingMaxMessages(int batchingMaxMessages)
        {
            _conf.BatchingMaxMessages = batchingMaxMessages;
            return this;
        }

        public ProducerConfigBuilder<T> SendTimeout(TimeSpan sendTimeoutMs)
        {
            _conf.SetSendTimeoutMs(sendTimeoutMs);
            return this;
        }

        public ProducerConfigBuilder<T> MaxPendingMessages(int maxPendingMessages)
        {
            _conf.MaxPendingMessages = maxPendingMessages;
            return this;
        }

        public ProducerConfigBuilder<T> MaxPendingMessagesAcrossPartitions(int maxPendingMessagesAcrossPartitions)
        {
            _conf.MaxPendingMessagesAcrossPartitions = maxPendingMessagesAcrossPartitions;
            return this;
        }

        public ProducerConfigBuilder<T> EnableChunking(bool chunk)
        {
            _conf.ChunkingEnabled = chunk;
            return this;
        }

        public ProducerConfigBuilder<T> MessageRoutingMode(MessageRoutingMode messageRouteMode)
        {
            _conf.MessageRoutingMode = messageRouteMode;
            return this;
        }

        public ProducerConfigBuilder<T> CompressionType(CompressionType compressionType)
        {
            _conf.CompressionType = compressionType;
            return this;
        }

        public ProducerConfigBuilder<T> HashingScheme(HashingScheme hashingScheme)
        {
            _conf.HashingScheme = hashingScheme;
            return this;
        }

        public ProducerConfigBuilder<T> CryptoKeyReader(ICryptoKeyReader cryptoKeyReader)
        {
            _conf.CryptoKeyReader = cryptoKeyReader;
            return this;
        }

        public ProducerConfigBuilder<T> AddEncryptionKey(string key)
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
        public ProducerConfigBuilder<T> InitialSubscriptionName(string initialSubscriptionName)
        {
            _conf.InitialSubscriptionName = initialSubscriptionName;
            return this;
        }

        public ProducerConfigBuilder<T> CryptoFailureAction(ProducerCryptoFailureAction action)
        {
            _conf.CryptoFailureAction = action;
            return this;
        }


        public ProducerConfigBuilder<T> InitialSequenceId(long initialSequenceId)
        {
            _conf.InitialSequenceId = initialSequenceId;
            return this;
        }

        public ProducerConfigBuilder<T> Property(string key, string value)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("property key cannot be blank or null");
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("property value cannot be blank or null");
            _conf.Properties.Add(key, value);
            return this;
        }

        public ProducerConfigBuilder<T> Properties(IDictionary<string, string> properties)
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

        public ProducerConfigBuilder<T> Schema(ISchema<T> schema)
        {
            if (schema == null)
                throw new ArgumentException("Schema is null");
            _schema = schema;
            return this;
        }

        public ProducerConfigBuilder<T> Intercept(params IProducerInterceptor<T>[] interceptors)
        {
            if (_interceptorList == null)
            {
                _interceptorList = new List<IProducerInterceptor<T>>();
            }
            _interceptorList.AddRange(interceptors);
            return this;
        }
        public ProducerConfigBuilder<T> AutoUpdatePartitions(bool autoUpdate)
        {
            _conf.AutoUpdatePartitions = autoUpdate;
            return this;
        }
        public ProducerConfigBuilder<T> AutoUpdatePartitionsInterval(TimeSpan interval)
        {
            _conf.SetAutoUpdatePartitionsIntervalSeconds(interval);
            return this;
        }

        public List<IProducerInterceptor<T>> GetInterceptors => _interceptorList;

        public ISchema<T> GetSchema => _schema;

        public ProducerConfigBuilder<T> EnableMultiSchema(bool multiSchema)
        {
            _conf.MultiSchema = multiSchema;
            return this;
        }

        public override string ToString()
        {
            return _conf?.ToString();
        }

    }

    public interface IProducerEventListener
    {
        public void MessageSent(SentReceipt receipt);
        public void Log(object log);
    }
}