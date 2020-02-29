using SharpPulsar.Api;
using SharpPulsar.Api.Interceptor;
using SharpPulsar.Extension;
using SharpPulsar.Impl.Conf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
namespace SharpPulsar.Impl
{

	public class ProducerBuilderImpl : IProducerBuilder
	{

		private readonly PulsarClientImpl _client;
		private ProducerConfigurationData _conf;
		private ISchema _schema;
		private IList<IProducerInterceptor> _interceptorList;
		public ProducerBuilderImpl(PulsarClientImpl client, ISchema schema) : this(client, new ProducerConfigurationData(), schema)
		{
		}

		private ProducerBuilderImpl(PulsarClientImpl client, ProducerConfigurationData conf, ISchema schema)
		{
			_client = client;
			_conf = conf;
			_schema = schema;
		}

		/// <summary>
		/// Allow to schema in builder implementation
		/// @return
		/// </summary>
		public virtual IProducerBuilder Schema(ISchema schema)
		{
			_schema = schema;
			return this;
		}

		public IProducerBuilder Clone()
		{
			return new ProducerBuilderImpl(_client, _conf.Clone(), _schema);
		}
        public IProducer Create()
        {
            var p = CreateAsync().ConfigureAwait(false).GetAwaiter().GetResult();
            return p;
        }
		public async ValueTask<IProducer> CreateAsync()
		{
			if (_conf.TopicName == null)
            {
                throw new ArgumentException("Topic name must be set on the producer builder");
            }

			try
			{
				SetMessageRoutingMode();
			}
			catch (PulsarClientException pce)
            {
                throw;
            }

            if (_interceptorList == null || _interceptorList.Count == 0)
            {
                return await _client.CreateProducerAsync(_conf, _schema, null);

            }
			return await _client.CreateProducerAsync(_conf, _schema, new ProducerInterceptors(_interceptorList));
		}

		public IProducerBuilder LoadConf(IDictionary<string, object> config)
		{
			_conf = ConfigurationDataUtils.LoadData(config, _conf);
			return this;
		}

		public IProducerBuilder Topic(string topicName)
		{
			if(string.IsNullOrWhiteSpace(topicName))
				throw new ArgumentException("topicName cannot be blank or null");
			_conf.TopicName = topicName.Trim();
			return this;
		}

		public IProducerBuilder ProducerName(string producerName)
		{
			_conf.ProducerName = producerName;
			return this;
		}

		public IProducerBuilder SendTimeout(int sendTimeout, BAMCIS.Util.Concurrent.TimeUnit unit)
		{
			_conf.SetSendTimeoutMs(sendTimeout, unit);
			return this;
		}

		public IProducerBuilder MaxPendingMessages(int maxPendingMessages)
		{
			_conf.MaxPendingMessages = maxPendingMessages;
			return this;
		}

		public IProducerBuilder MaxPendingMessagesAcrossPartitions(int maxPendingMessagesAcrossPartitions)
		{
			_conf.MaxPendingMessagesAcrossPartitions = maxPendingMessagesAcrossPartitions;
			return this;
		}

		public IProducerBuilder BlockIfQueueFull(bool blockIfQueueFull)
		{
			//conf.q.BlockIfQueueFull = BlockIfQueueFull;
			return this;
		}

		public IProducerBuilder MessageRoutingMode(MessageRoutingMode messageRouteMode)
		{
			_conf.MessageRoutingMode = messageRouteMode;
			return this;
		}

		public IProducerBuilder CompressionType(ICompressionType compressionType)
		{
			_conf.CompressionType = compressionType;
			return this;
		}

		public IProducerBuilder HashingScheme(HashingScheme hashingScheme)
		{
			_conf.HashingScheme = hashingScheme;
			return this;
		}

		public IProducerBuilder MessageRouter(IMessageRouter messageRouter)
		{
			_conf.CustomMessageRouter = messageRouter;
			return this;
		}

		public IProducerBuilder EnableBatching(bool batchMessagesEnabled)
		{
			_conf.BatchingEnabled = batchMessagesEnabled;
			return this;
		}

		public IProducerBuilder CryptoKeyReader(ICryptoKeyReader cryptoKeyReader)
		{
			_conf.CryptoKeyReader = cryptoKeyReader;
			return this;
		}

		public IProducerBuilder AddEncryptionKey(string key)
		{
			if(string.IsNullOrWhiteSpace(key))
				throw new ArgumentException("Encryption key cannot be blank or null");
			_conf.EncryptionKeys.Add(key);
			return this;
		}

		public IProducerBuilder CryptoFailureAction(ProducerCryptoFailureAction action)
		{
			_conf.CryptoFailureAction = action;
			return this;
		}

		public IProducerBuilder BatchingMaxPublishDelay(long batchDelay, BAMCIS.Util.Concurrent.TimeUnit timeUnit)
		{
			_conf.SetBatchingMaxPublishDelayMicros(batchDelay, timeUnit);
			return this;
		}

		public IProducerBuilder RoundRobinRouterBatchingPartitionSwitchFrequency(int frequency)
		{
			_conf.BatchingPartitionSwitchFrequencyByPublishDelay = frequency;
			return this;
		}

		public IProducerBuilder BatchingMaxMessages(int batchMessagesMaxMessagesPerBatch)
		{
			_conf.BatchingMaxMessages = batchMessagesMaxMessagesPerBatch;
			return this;
		}

		public IProducerBuilder BatchingMaxBytes(int batchingMaxBytes)
		{
			_conf.BatchingMaxBytes = batchingMaxBytes;
			return this;
		}

		public IProducerBuilder BatcherBuilder(IBatcherBuilder batcherBuilder)
		{
			_conf.BatcherBuilder = batcherBuilder;
			return this;
		}


		public IProducerBuilder InitialSequenceId(long initialSequenceId)
		{
			_conf.InitialSequenceId = initialSequenceId;
			return this;
		}

		public IProducerBuilder Property(string key, string value)
		{
			if(string.IsNullOrWhiteSpace(key))
				throw new ArgumentException("property key cannot be blank or null");
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("property value cannot be blank or null");
			_conf.Properties.Add(key, value);
			return this;
		}

		public IProducerBuilder Properties(IDictionary<string, string> properties)
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

		public IProducerBuilder Intercept(params IProducerInterceptor[] interceptors)
		{
			if (_interceptorList == null)
			{
				_interceptorList = new List<IProducerInterceptor>();
			}
			((List<IProducerInterceptor>)_interceptorList).AddRange(interceptors.ToArray());
			return this;
		}

		public IProducerBuilder AutoUpdatePartitions(bool autoUpdate)
		{
			_conf.AutoUpdatePartitions = autoUpdate;
			return this;
		}

		public IProducerBuilder EnableMultiSchema(bool multiSchema)
		{
			_conf.MultiSchema = multiSchema;
			return this;
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

		object ICloneable.Clone()
		{
			return new ProducerBuilderImpl(_client, _conf.Clone(), _schema);
		}
	}

}