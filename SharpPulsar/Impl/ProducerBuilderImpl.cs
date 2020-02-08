using SharpPulsar.Api;
using SharpPulsar.Api.Interceptor;
using SharpPulsar.Exception;
using SharpPulsar.Extension;
using SharpPulsar.Impl.Conf;
using System;
using System.Collections.Generic;
using System.Linq;
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
namespace SharpPulsar.Impl
{

	public class ProducerBuilderImpl<T> : IProducerBuilder<T>
	{

		private readonly PulsarClientImpl _client;
		private ProducerConfigurationData _conf;
		private ISchema<T> _schema;
		private IList<IProducerInterceptor> _interceptorList;
		public ProducerBuilderImpl(PulsarClientImpl client, ISchema<T> schema) : this(client, new ProducerConfigurationData(), schema)
		{
		}

		private ProducerBuilderImpl(PulsarClientImpl client, ProducerConfigurationData conf, ISchema<T> schema)
		{
			_client = client;
			_conf = conf;
			_schema = schema;
		}

		/// <summary>
		/// Allow to schema in builder implementation
		/// @return
		/// </summary>
		public virtual IProducerBuilder<T> Schema(ISchema<T> schema)
		{
			_schema = schema;
			return this;
		}

		public IProducerBuilder<T> Clone()
		{
			return new ProducerBuilderImpl<T>(_client, _conf.Clone(), _schema);
		}

		public IProducer<T> Create()
		{
			try
			{
				return CreateAsync().Result;
			}
			catch (System.Exception e)
			{
				throw PulsarClientException.Unwrap(e);
			}
		}

		public ValueTask<IProducer<T>> CreateAsync()
		{
			if (_conf.TopicName == null)
			{
				return new ValueTask<IProducer<T>>(Task.FromException<IProducer<T>>(new ArgumentException("Topic name must be set on the producer builder")));
			}

			try
			{
				SetMessageRoutingMode();
			}
			catch (PulsarClientException pce)
			{
				return new ValueTask<IProducer<T>>(Task.FromException<IProducer<T>>(pce));
			}

			return _interceptorList == null || _interceptorList.Count == 0 ? _client.CreateProducerAsync(_conf, _schema, null) : _client.CreateProducerAsync(_conf, _schema, new ProducerInterceptors(_interceptorList));
		}

		public IProducerBuilder<T> LoadConf(IDictionary<string, object> config)
		{
			_conf = ConfigurationDataUtils.LoadData(config, _conf, typeof(ProducerConfigurationData));
			return this;
		}

		public IProducerBuilder<T> Topic(string topicName)
		{
			if(string.IsNullOrWhiteSpace(topicName))
				throw new ArgumentNullException("topicName cannot be blank");
			_conf.TopicName = topicName.Trim();
			return this;
		}

		public IProducerBuilder<T> ProducerName(string producerName)
		{
			_conf.ProducerName = producerName;
			return this;
		}

		public IProducerBuilder<T> SendTimeout(int sendTimeout, BAMCIS.Util.Concurrent.TimeUnit unit)
		{
			_conf.SetSendTimeoutMs(sendTimeout, unit);
			return this;
		}

		public IProducerBuilder<T> MaxPendingMessages(int maxPendingMessages)
		{
			_conf.MaxPendingMessages = maxPendingMessages;
			return this;
		}

		public IProducerBuilder<T> MaxPendingMessagesAcrossPartitions(int maxPendingMessagesAcrossPartitions)
		{
			_conf.MaxPendingMessagesAcrossPartitions = maxPendingMessagesAcrossPartitions;
			return this;
		}

		public IProducerBuilder<T> BlockIfQueueFull(bool blockIfQueueFull)
		{
			//conf.q.BlockIfQueueFull = BlockIfQueueFull;
			return this;
		}

		public IProducerBuilder<T> MessageRoutingMode(MessageRoutingMode messageRouteMode)
		{
			_conf.MessageRoutingMode = messageRouteMode;
			return this;
		}

		public IProducerBuilder<T> CompressionType(ICompressionType compressionType)
		{
			_conf.CompressionType = compressionType;
			return this;
		}

		public IProducerBuilder<T> HashingScheme(HashingScheme hashingScheme)
		{
			_conf.HashingScheme = hashingScheme;
			return this;
		}

		public IProducerBuilder<T> MessageRouter(MessageRouter messageRouter)
		{
			_conf.CustomMessageRouter = messageRouter;
			return this;
		}

		public IProducerBuilder<T> EnableBatching(bool batchMessagesEnabled)
		{
			_conf.BatchingEnabled = batchMessagesEnabled;
			return this;
		}

		public IProducerBuilder<T> CryptoKeyReader(CryptoKeyReader cryptoKeyReader)
		{
			_conf.CryptoKeyReader = cryptoKeyReader;
			return this;
		}

		public IProducerBuilder<T> AddEncryptionKey(string key)
		{
			if(string.IsNullOrWhiteSpace(key))
				throw new ArgumentNullException("Encryption key cannot be blank");
			_conf.EncryptionKeys.Add(key);
			return this;
		}

		public IProducerBuilder<T> CryptoFailureAction(ProducerCryptoFailureAction action)
		{
			_conf.CryptoFailureAction = action;
			return this;
		}

		public IProducerBuilder<T> BatchingMaxPublishDelay(long batchDelay, BAMCIS.Util.Concurrent.TimeUnit timeUnit)
		{
			_conf.SetBatchingMaxPublishDelayMicros(batchDelay, timeUnit);
			return this;
		}

		public IProducerBuilder<T> RoundRobinRouterBatchingPartitionSwitchFrequency(int frequency)
		{
			_conf.BatchingPartitionSwitchFrequencyByPublishDelay = frequency;
			return this;
		}

		public IProducerBuilder<T> BatchingMaxMessages(int batchMessagesMaxMessagesPerBatch)
		{
			_conf.BatchingMaxMessages = batchMessagesMaxMessagesPerBatch;
			return this;
		}

		public IProducerBuilder<T> BatchingMaxBytes(int batchingMaxBytes)
		{
			_conf.BatchingMaxBytes = batchingMaxBytes;
			return this;
		}

		public IProducerBuilder<T> BatcherBuilder(BatcherBuilder batcherBuilder)
		{
			_conf.BatcherBuilder = batcherBuilder;
			return this;
		}


		public IProducerBuilder<T> InitialSequenceId(long initialSequenceId)
		{
			_conf.InitialSequenceId = initialSequenceId;
			return this;
		}

		public IProducerBuilder<T> Property(string key, string value)
		{
			if(string.IsNullOrWhiteSpace(key) && string.IsNullOrWhiteSpace(value))
				throw new ArgumentNullException("property key/value cannot be blank");
			_conf.Properties.Add(key, value);
			return this;
		}

		public IProducerBuilder<T> Properties(IDictionary<string, string> properties)
		{
			if(properties.Count < 1)
				throw new ArgumentNullException("properties cannot be empty");
			properties.SetOfKeyValuePairs().ToList().ForEach(entry =>
            {
                var (key, value) = entry;
                if (string.IsNullOrWhiteSpace(key) && string.IsNullOrWhiteSpace(value))
                {
                    throw new NullReferenceException("properties' key/value cannot be blank");
                }
                else
                {
					_conf.Properties.Add(key, value);
				}
            });
			return this;
		}

		public IProducerBuilder<T> Intercept(params IProducerInterceptor[] interceptors)
		{
			if (_interceptorList == null)
			{
				_interceptorList = new List<IProducerInterceptor>();
			}
			((List<IProducerInterceptor>)_interceptorList).AddRange(interceptors.ToArray());
			return this;
		}

		public IProducerBuilder<T> AutoUpdatePartitions(bool autoUpdate)
		{
			_conf.AutoUpdatePartitions = autoUpdate;
			return this;
		}

		public IProducerBuilder<T> EnableMultiSchema(bool multiSchema)
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
			else if ((_conf.MessageRoutingMode == Api.MessageRoutingMode.CustomPartition && _conf.CustomMessageRouter == null) || (_conf.MessageRoutingMode != Api.MessageRoutingMode.CustomPartition && _conf.CustomMessageRouter != null))
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
			return new ProducerBuilderImpl<T>(_client, _conf.Clone(), _schema);
		}
	}

}