using System;
using System.Collections.Generic;
using System.Linq;
using SharpPulsar.Akka.InternalCommands;
using SharpPulsar.Api;
using SharpPulsar.Api.Interceptor;
using SharpPulsar.Extension;
using SharpPulsar.Impl.Conf;
using SharpPulsar.Utils;

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

	public sealed class ProducerConfigBuilder
	{
		private ProducerConfigurationData _conf = new ProducerConfigurationData();

        public ProducerConfigurationData ProducerConfigurationData
        {
            get
            {
                if (_conf.Schema == null)
                    throw new ArgumentException("Hey, we need the schema!");
				if (_conf.ProducerEventListener == null)
                    throw new ArgumentException("ProducerEventListener cannot be null");
                return _conf;
            }
        } 
		public ProducerConfigBuilder LoadConf(IDictionary<string, object> config)
		{
			_conf = (ProducerConfigurationData)ConfigurationDataUtils.LoadData(config, _conf);
            return this;
        }
        public ProducerConfigBuilder EventListener(IProducerEventListener listener)
        {
            if (listener == null)
                throw new ArgumentException("listener is null");
            _conf.ProducerEventListener = listener;
            return this;
        }
		/// <summary>
		/// max in bytes
		/// </summary>
		/// <param name="max"></param>
		/// <returns></returns>
        public ProducerConfigBuilder MaxMessageSize(int max)
        {
            if (max < 1)
                throw new ArgumentException("max should be > 0");
            _conf.MaxMessageSize = max;
            return this;
        }
		public ProducerConfigBuilder Topic(string topicName)
		{
			if(string.IsNullOrWhiteSpace(topicName))
				throw new ArgumentException("topicName cannot be blank or null");
			_conf.TopicName = topicName.Trim();
            return this;
		}

		public ProducerConfigBuilder ProducerName(string producerName)
		{
			_conf.ProducerName = producerName;
            return this;
		}
		public ProducerConfigBuilder EnableBatching(bool enableBatching)
		{
			_conf.BatchingEnabled = enableBatching;
            return this;
		}
		public ProducerConfigBuilder BatchingMaxPublishDelay(long batchingMaxPublishDelayMs)
        {
            _conf.BatchingMaxPublishDelayMicros = (long)ConvertTimeUnits.ConvertMillisecondsToMicroseconds(batchingMaxPublishDelayMs);
            return this;
		}
		
		public ProducerConfigBuilder BatchingMaxMessages(int batchingMaxMessages)
        {
            _conf.BatchingMaxMessages = batchingMaxMessages;
            return this;
		}

		public ProducerConfigBuilder SendTimeout(long sendTimeoutMs)
		{
			_conf.SetSendTimeoutMs(sendTimeoutMs);
            return this;
		}

		public ProducerConfigBuilder MaxPendingMessages(int maxPendingMessages)
		{
			_conf.MaxPendingMessages = maxPendingMessages;
            return this;
		}

		public ProducerConfigBuilder MaxPendingMessagesAcrossPartitions(int maxPendingMessagesAcrossPartitions)
		{
			_conf.MaxPendingMessagesAcrossPartitions = maxPendingMessagesAcrossPartitions;
            return this;
		}

		public ProducerConfigBuilder EnableChunking(bool chunk)
		{
			_conf.ChunkingEnabled = chunk;
            return this;
		}

		public ProducerConfigBuilder MessageRoutingMode(MessageRoutingMode messageRouteMode)
		{
			_conf.MessageRoutingMode = messageRouteMode;
            return this;
		}

		public ProducerConfigBuilder CompressionType(ICompressionType compressionType)
		{
			_conf.CompressionType = compressionType;
            return this;
		}

		public ProducerConfigBuilder HashingScheme(HashingScheme hashingScheme)
		{
			_conf.HashingScheme = hashingScheme;
            return this;
		}
		
		public ProducerConfigBuilder CryptoKeyReader(ICryptoKeyReader cryptoKeyReader)
		{
			_conf.CryptoKeyReader = cryptoKeyReader;
            return this;
		}

		public ProducerConfigBuilder AddEncryptionKey(string key)
		{
			if(string.IsNullOrWhiteSpace(key))
				throw new ArgumentException("Encryption key cannot be blank or null");
			_conf.EncryptionKeys.Add(key);
            return this;
		}

		public ProducerConfigBuilder CryptoFailureAction(ProducerCryptoFailureAction action)
		{
			_conf.CryptoFailureAction = action;
            return this;
		}


		public ProducerConfigBuilder InitialSequenceId(long initialSequenceId)
		{
			_conf.InitialSequenceId = initialSequenceId;
            return this;
		}

		public ProducerConfigBuilder Property(string key, string value)
		{
			if(string.IsNullOrWhiteSpace(key))
				throw new ArgumentException("property key cannot be blank or null");
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("property value cannot be blank or null");
			_conf.Properties.Add(key, value);
            return this;
		}

		public ProducerConfigBuilder Properties(IDictionary<string, string> properties)
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

        public ProducerConfigBuilder Schema(ISchema schema)
        {
			if(schema == null)
				throw new ArgumentException("Schema is null");
            _conf.Schema = schema;
            return this;
		}
		public ProducerConfigBuilder Intercept(params IProducerInterceptor[] interceptors)
		{
			if (_conf.Interceptors == null)
			{
                _conf.Interceptors = new List<IProducerInterceptor>();
			}
            _conf.Interceptors.AddRange(interceptors.ToArray());
            return this;

		}

		public ProducerConfigBuilder AutoUpdatePartitions(bool autoUpdate)
		{
			_conf.AutoUpdatePartitions = autoUpdate;
            return this;
		}

		public ProducerConfigBuilder EnableMultiSchema(bool multiSchema)
		{
			_conf.MultiSchema = multiSchema;
            return this;
		}

		private ProducerConfigBuilder SetMessageRoutingMode()
		{
			MessageRoutingMode(_conf.MessageRoutingMode);
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