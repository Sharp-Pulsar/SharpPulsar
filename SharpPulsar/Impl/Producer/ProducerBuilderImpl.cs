using BAMCIS.Util.Concurrent;
using SharpPulsar.Configuration;
using SharpPulsar.Enum;
using SharpPulsar.Exception;
using SharpPulsar.Interface;
using SharpPulsar.Interface.Batch;
using SharpPulsar.Interface.Interceptor;
using SharpPulsar.Interface.Message;
using SharpPulsar.Interface.Producer;
using SharpPulsar.Interface.Schema;
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
namespace SharpPulsar.Impl.Producer
{

	public class ProducerBuilderImpl<T> : IProducerBuilder<T>
	{

		private readonly PulsarClientImpl client;
		private ProducerConfigurationData conf;
		private ISchema<T> schema_Conflict;
		private IList<IProducerInterceptor> interceptorList;
		public ProducerBuilderImpl(PulsarClientImpl client, ISchema<T> schema) : this(client, new ProducerConfigurationData(), schema)
		{
		}

		private ProducerBuilderImpl(PulsarClientImpl client, ProducerConfigurationData conf, ISchema<T> schema)
		{
			this.client = client;
			this.conf = conf;
			this.schema_Conflict = schema;
		}

		/// <summary>
		/// Allow to override schema in builder implementation
		/// @return
		/// </summary>
		public virtual IProducerBuilder<T> Schema(ISchema<T> schema)
		{
			this.schema_Conflict = schema;
			return this;
		}

		public IProducerBuilder<T> Clone()
		{
			return new ProducerBuilderImpl<T>(client, conf.Clone(), schema_Conflict);
		}
		public IProducer<T> Create()
		{
			try
			{
				return CreateAsync().GetAwaiter().GetResult();
			}
			catch (System.Exception e)
			{
				throw PulsarClientException.Unwrap(e);
			}
		}

		public ValueTask<IProducer<T>> CreateAsync()
		{
			if (conf.TopicName == null)
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

			return interceptorList == null || interceptorList.Count == 0 ? client.CreateProducerAsync(conf, schema_Conflict, null) : client.createProducerAsync(conf, schema_Conflict, new ProducerInterceptors(interceptorList));
		}

		public IProducerBuilder<T> LoadConf(IDictionary<string, object> config)
		{
			conf = ConfigurationDataUtils.LoadData(config, conf, typeof(ProducerConfigurationData));
			return this;
		}

		public IProducerBuilder<T> Topic(string topicName)
		{
			checkArgument(StringUtils.isNotBlank(topicName), "topicName cannot be blank");
			conf.TopicName = topicName.Trim();
			return this;
		}

		public IProducerBuilder<T> ProducerName(string producerName)
		{
			conf.ProducerName = producerName;
			return this;
		}

		public IProducerBuilder<T> SendTimeout(int sendTimeout, TimeUnit unit)
		{
			conf.SetSendTimeoutMs(sendTimeout, unit);
			return this;
		}

		public IProducerBuilder<T> MaxPendingMessages(int maxPendingMessages)
		{
			conf.MaxPendingMessages = maxPendingMessages;
			return this;
		}

		public IProducerBuilder<T> MaxPendingMessagesAcrossPartitions(int maxPendingMessagesAcrossPartitions)
		{
			conf.MaxPendingMessagesAcrossPartitions = maxPendingMessagesAcrossPartitions;
			return this;
		}

		public IProducerBuilder<T> BlockIfQueueFull(bool blockIfQueueFull)
		{
			conf.BlockIfQueueFull = blockIfQueueFull;
			return this;
		}


		public IProducerBuilder<T> MessageRoutingMode(MessageRoutingMode messageRouteMode)
		{
			conf.MessageRoutingMode = messageRouteMode;
			return this;
		}

		public IProducerBuilder<T> CompressionType(CompressionType compressionType)
		{
			conf.CompressionType = compressionType;
			return this;
		}


		public IProducerBuilder<T> HashingScheme(HashingScheme hashingScheme)
		{
			conf.HashingScheme = hashingScheme;
			return this;
		}


		public IProducerBuilder<T> MessageRouter(IMessageRouter messageRouter)
		{
			conf.CustomMessageRouter = messageRouter;
			return this;
		}

		public IProducerBuilder<T> EnableBatching(bool batchMessagesEnabled)
		{
			conf.BatchingEnabled = batchMessagesEnabled;
			return this;
		}

		public IProducerBuilder<T> CryptoKeyReader(ICryptoKeyReader cryptoKeyReader)
		{
			conf.CryptoKeyReader = cryptoKeyReader;
			return this;
		}

		public IProducerBuilder<T> AddEncryptionKey(string key)
		{
			if(string.IsNullOrWhiteSpace(key))
				throw new ArgumentNullException("Encryption key cannot be blank");
			conf.EncryptionKeys.Add(key);
			return this;
		}


		public IProducerBuilder<T> CryptoFailureAction(ProducerCryptoFailureAction action)
		{
			conf.CryptoFailureAction = action;
			return this;
		}


		public IProducerBuilder<T> BatchingMaxPublishDelay(long batchDelay, TimeUnit timeUnit)
		{
			conf.SetBatchingMaxPublishDelayMicros(batchDelay, timeUnit);
			return this;
		}

		public IProducerBuilder<T> RoundRobinRouterBatchingPartitionSwitchFrequency(int frequency)
		{
			conf.BatchingPartitionSwitchFrequencyByPublishDelay = frequency;
			return this;
		}

		public IProducerBuilder<T> BatchingMaxMessages(int batchMessagesMaxMessagesPerBatch)
		{
			conf.BatchingMaxMessages = batchMessagesMaxMessagesPerBatch;
			return this;
		}

		public IProducerBuilder<T> BatchingMaxBytes(int batchingMaxBytes)
		{
			conf.BatchingMaxBytes = batchingMaxBytes;
			return this;
		}

		public IProducerBuilder<T> BatcherBuilder(IBatcherBuilder batcherBuilder)
		{
			conf.BatcherBuilder = batcherBuilder;
			return this;
		}


		public IProducerBuilder<T> InitialSequenceId(long initialSequenceId)
		{
			conf.InitialSequenceId = initialSequenceId;
			return this;
		}

		public IProducerBuilder<T> Property(string key, string value)
		{
			checkArgument(StringUtils.isNotBlank(key) && StringUtils.isNotBlank(value), "property key/value cannot be blank");
			conf.Properties.Add(key, value);
			return this;
		}
		public IProducerBuilder<T> Properties(IDictionary<string, string> properties)
		{
			checkArgument(!properties.Count() < 1, "properties cannot be empty");
			properties.entrySet().forEach(entry => checkArgument(StringUtils.isNotBlank(entry.Key) && StringUtils.isNotBlank(entry.Value), "properties' key/value cannot be blank"));
			conf.Properties.putAll(properties);
			return this;
		}

		public IProducerBuilder<T> Intercept(params IProducerInterceptor[] interceptors)
		{
			if (interceptorList == null)
			{
				interceptorList = new List<IProducerInterceptor>();
			}
			((List<IProducerInterceptor>)interceptorList).AddRange(interceptors);
			return this;
		}

		public IProducerBuilder<T> AutoUpdatePartitions(bool autoUpdate)
		{
			conf.AutoUpdatePartitions = autoUpdate;
			return this;
		}

		public IProducerBuilder<T> EnableMultiSchema(bool multiSchema)
		{
			conf.MultiSchema = multiSchema;
			return this;
		}

		private void SetMessageRoutingMode()
		{
			if (conf.MessageRoutingMode == null && conf.CustomMessageRouter == null)
			{
				MessageRoutingMode(Enum.MessageRoutingMode.RoundRobinPartition);
			}
			else if (conf.MessageRoutingMode == null && conf.CustomMessageRouter != null)
			{
				MessageRoutingMode(Enum.MessageRoutingMode.CustomPartition);
			}
			else if ((conf.MessageRoutingMode == Enum.MessageRoutingMode.CustomPartition && conf.CustomMessageRouter == null) || (conf.MessageRoutingMode != MessageRoutingMode.CustomPartition && conf.CustomMessageRouter != null))
			{
				throw new PulsarClientException("When 'messageRouter' is set, 'messageRoutingMode' " + "should be set as " + MessageRoutingMode.CustomPartition);
			}
		}

		public override string ToString()
		{
			return conf != null ? conf.ToString() : null;
		}
	}

}