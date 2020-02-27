using System;
using System.Collections.Generic;
using System.Linq;
using SharpPulsar.Api;
using SharpPulsar.Api.Interceptor;
using SharpPulsar.Extension;
using SharpPulsar.Impl.Conf;
using PulsarClientException = SharpPulsar.Exceptions.PulsarClientException;

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

        public ProducerConfigurationData ProducerConfigurationData => _conf;
		public void LoadConf(IDictionary<string, object> config)
		{
			_conf = ConfigurationDataUtils.LoadData(config, _conf);
			
		}

		public void Topic(string topicName)
		{
			if(string.IsNullOrWhiteSpace(topicName))
				throw new ArgumentException("topicName cannot be blank or null");
			_conf.TopicName = topicName.Trim();
			
		}

		public void ProducerName(string producerName)
		{
			_conf.ProducerName = producerName;
			
		}

		public void SendTimeout(int sendTimeout, BAMCIS.Util.Concurrent.TimeUnit unit)
		{
			_conf.SetSendTimeoutMs(sendTimeout, unit);
			
		}

		public void MaxPendingMessages(int maxPendingMessages)
		{
			_conf.MaxPendingMessages = maxPendingMessages;
			
		}

		public void MaxPendingMessagesAcrossPartitions(int maxPendingMessagesAcrossPartitions)
		{
			_conf.MaxPendingMessagesAcrossPartitions = maxPendingMessagesAcrossPartitions;
			
		}

		public void BlockIfQueueFull(bool blockIfQueueFull)
		{
			//conf.q.BlockIfQueueFull = BlockIfQueueFull;
			
		}

		public void MessageRoutingMode(MessageRoutingMode messageRouteMode)
		{
			_conf.MessageRoutingMode = messageRouteMode;
			
		}

		public void CompressionType(ICompressionType compressionType)
		{
			_conf.CompressionType = compressionType;
			
		}

		public void HashingScheme(HashingScheme hashingScheme)
		{
			_conf.HashingScheme = hashingScheme;
			
		}

		public void MessageRouter(IMessageRouter messageRouter)
		{
			_conf.CustomMessageRouter = messageRouter;
			
		}

		public void EnableBatching(bool batchMessagesEnabled)
		{
			_conf.BatchingEnabled = batchMessagesEnabled;
			
		}

		public void CryptoKeyReader(ICryptoKeyReader cryptoKeyReader)
		{
			_conf.CryptoKeyReader = cryptoKeyReader;
			
		}

		public void AddEncryptionKey(string key)
		{
			if(string.IsNullOrWhiteSpace(key))
				throw new ArgumentException("Encryption key cannot be blank or null");
			_conf.EncryptionKeys.Add(key);
			
		}

		public void CryptoFailureAction(ProducerCryptoFailureAction action)
		{
			_conf.CryptoFailureAction = action;
			
		}

		public void BatchingMaxPublishDelay(long batchDelay, BAMCIS.Util.Concurrent.TimeUnit timeUnit)
		{
			_conf.SetBatchingMaxPublishDelayMicros(batchDelay, timeUnit);
			
		}

		public void RoundRobinRouterBatchingPartitionSwitchFrequency(int frequency)
		{
			_conf.BatchingPartitionSwitchFrequencyByPublishDelay = frequency;
			
		}

		public void BatchingMaxMessages(int batchMessagesMaxMessagesPerBatch)
		{
			_conf.BatchingMaxMessages = batchMessagesMaxMessagesPerBatch;
			
		}

		public void BatchingMaxBytes(int batchingMaxBytes)
		{
			_conf.BatchingMaxBytes = batchingMaxBytes;
			
		}

		public void BatcherBuilder(IBatcherBuilder batcherBuilder)
		{
			_conf.BatcherBuilder = batcherBuilder;
			
		}


		public void InitialSequenceId(long initialSequenceId)
		{
			_conf.InitialSequenceId = initialSequenceId;
			
		}

		public void Property(string key, string value)
		{
			if(string.IsNullOrWhiteSpace(key))
				throw new ArgumentException("property key cannot be blank or null");
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("property value cannot be blank or null");
			_conf.Properties.Add(key, value);
			
		}

		public void Properties(IDictionary<string, string> properties)
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
			
		}

        public void Schema(ISchema schema)
        {
			if(schema == null)
				throw new ArgumentException("Schema is null");
        }
		public void Intercept(params IProducerInterceptor[] interceptors)
		{
			if (_conf.Interceptors == null)
			{
                _conf.Interceptors = new List<IProducerInterceptor>();
			}
            _conf.Interceptors.AddRange(interceptors.ToArray());
			
		}

		public void AutoUpdatePartitions(bool autoUpdate)
		{
			_conf.AutoUpdatePartitions = autoUpdate;
			
		}

		public void EnableMultiSchema(bool multiSchema)
		{
			_conf.MultiSchema = multiSchema;
			
		}

		private void SetMessageRoutingMode()
		{

			if (_conf.MessageRoutingMode == null && _conf.CustomMessageRouter == null)
			{
				MessageRoutingMode(Api.MessageRoutingMode.RoundRobinPartition);
			}
			else if (_conf.MessageRoutingMode == null && _conf.CustomMessageRouter != null)
			{
				MessageRoutingMode(Api.MessageRoutingMode.CustomPartition);
			}
			else 
			if ((_conf.MessageRoutingMode == Api.MessageRoutingMode.CustomPartition && _conf.CustomMessageRouter == null) || (_conf.MessageRoutingMode != Api.MessageRoutingMode.CustomPartition && _conf.CustomMessageRouter != null))
			{
				throw new PulsarClientException("When 'messageRouter' is set, 'messageRoutingMode' " + "should be set as " + Api.MessageRoutingMode.CustomPartition);
			}
		}

		public override string ToString()
		{
			return _conf?.ToString();
		}

	}

}