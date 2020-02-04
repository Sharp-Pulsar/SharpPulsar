using SharpPulsar.Api;
using SharpPulsar.Api.Interceptor;
using SharpPulsar.Exception;
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

		private readonly PulsarClientImpl client;
		private ProducerConfigurationData conf;
		private ISchema<T> schema;
		private IList<IProducerInterceptor> interceptorList;
		public ProducerBuilderImpl(PulsarClientImpl Client, ISchema<T> Schema) : this(Client, new ProducerConfigurationData(), Schema)
		{
		}

		private ProducerBuilderImpl(PulsarClientImpl Client, ProducerConfigurationData Conf, ISchema<T> Schema)
		{
			this.client = Client;
			this.conf = Conf;
			this.schema = Schema;
		}

		/// <summary>
		/// Allow to schema in builder implementation
		/// @return
		/// </summary>
		public virtual IProducerBuilder<T> Schema(ISchema<T> Schema)
		{
			this.schema = Schema;
			return this;
		}

		public IProducerBuilder<T> Clone()
		{
			return new ProducerBuilderImpl<T>(client, conf.Clone(), schema);
		}

		public IProducer<T> Create()
		{
			try
			{
				return CreateAsync().Result;
			}
			catch (System.Exception E)
			{
				throw PulsarClientException.Unwrap(E);
			}
		}

		public ValueTask<IProducer<T>> CreateAsync()
		{
			if (conf.TopicName == null)
			{
				return new ValueTask<IProducer<T>>(Task.FromException<IProducer<T>>(new ArgumentException("Topic name must be set on the producer builder")));
			}

			try
			{
				SetMessageRoutingMode();
			}
			catch (PulsarClientException Pce)
			{
				return new ValueTask<IProducer<T>>(Task.FromException<IProducer<T>>(Pce));
			}

			return interceptorList == null || interceptorList.Count == 0 ? client.CreateProducerAsync(conf, schema, null) : client.CreateProducerAsync(conf, schema, new ProducerInterceptors(interceptorList));
		}

		public IProducerBuilder<T> LoadConf(IDictionary<string, object> Config)
		{
			conf = ConfigurationDataUtils.LoadData(Config, conf, typeof(ProducerConfigurationData));
			return this;
		}

		public IProducerBuilder<T> Topic(string TopicName)
		{
			if(string.IsNullOrWhiteSpace(TopicName))
				throw new ArgumentNullException("topicName cannot be blank");
			conf.TopicName = TopicName.Trim();
			return this;
		}

		public IProducerBuilder<T> ProducerName(string ProducerName)
		{
			conf.ProducerName = ProducerName;
			return this;
		}

		public IProducerBuilder<T> SendTimeout(int SendTimeout, BAMCIS.Util.Concurrent.TimeUnit Unit)
		{
			conf.SetSendTimeoutMs(SendTimeout, Unit);
			return this;
		}

		public IProducerBuilder<T> MaxPendingMessages(int MaxPendingMessages)
		{
			conf.MaxPendingMessages = MaxPendingMessages;
			return this;
		}

		public IProducerBuilder<T> MaxPendingMessagesAcrossPartitions(int MaxPendingMessagesAcrossPartitions)
		{
			conf.MaxPendingMessagesAcrossPartitions = MaxPendingMessagesAcrossPartitions;
			return this;
		}

		public IProducerBuilder<T> BlockIfQueueFull(bool BlockIfQueueFull)
		{
			//conf.q.BlockIfQueueFull = BlockIfQueueFull;
			return this;
		}

		public IProducerBuilder<T> MessageRoutingMode(MessageRoutingMode MessageRouteMode)
		{
			conf.MessageRoutingMode = MessageRouteMode;
			return this;
		}

		public IProducerBuilder<T> CompressionType(ICompressionType CompressionType)
		{
			conf.CompressionType = CompressionType;
			return this;
		}

		public IProducerBuilder<T> HashingScheme(HashingScheme HashingScheme)
		{
			conf.HashingScheme = HashingScheme;
			return this;
		}

		public IProducerBuilder<T> MessageRouter(MessageRouter MessageRouter)
		{
			conf.CustomMessageRouter = MessageRouter;
			return this;
		}

		public IProducerBuilder<T> EnableBatching(bool BatchMessagesEnabled)
		{
			conf.BatchingEnabled = BatchMessagesEnabled;
			return this;
		}

		public IProducerBuilder<T> CryptoKeyReader(CryptoKeyReader CryptoKeyReader)
		{
			conf.CryptoKeyReader = CryptoKeyReader;
			return this;
		}

		public IProducerBuilder<T> AddEncryptionKey(string Key)
		{
			if(string.IsNullOrWhiteSpace(Key))
				throw new ArgumentNullException("Encryption key cannot be blank");
			conf.EncryptionKeys.Add(Key);
			return this;
		}

		public IProducerBuilder<T> CryptoFailureAction(ProducerCryptoFailureAction Action)
		{
			conf.CryptoFailureAction = Action;
			return this;
		}

		public IProducerBuilder<T> BatchingMaxPublishDelay(long BatchDelay, BAMCIS.Util.Concurrent.TimeUnit timeUnit)
		{
			conf.SetBatchingMaxPublishDelayMicros(BatchDelay, timeUnit);
			return this;
		}

		public IProducerBuilder<T> RoundRobinRouterBatchingPartitionSwitchFrequency(int Frequency)
		{
			conf.BatchingPartitionSwitchFrequencyByPublishDelay = Frequency;
			return this;
		}

		public IProducerBuilder<T> BatchingMaxMessages(int BatchMessagesMaxMessagesPerBatch)
		{
			conf.BatchingMaxMessages = BatchMessagesMaxMessagesPerBatch;
			return this;
		}

		public IProducerBuilder<T> BatchingMaxBytes(int BatchingMaxBytes)
		{
			conf.BatchingMaxBytes = BatchingMaxBytes;
			return this;
		}

		public IProducerBuilder<T> BatcherBuilder(BatcherBuilder BatcherBuilder)
		{
			conf.BatcherBuilder = BatcherBuilder;
			return this;
		}


		public IProducerBuilder<T> InitialSequenceId(long InitialSequenceId)
		{
			conf.InitialSequenceId = InitialSequenceId;
			return this;
		}

		public IProducerBuilder<T> Property(string Key, string Value)
		{
			if(string.IsNullOrWhiteSpace(Key) && string.IsNullOrWhiteSpace(Value))
				throw new ArgumentNullException("property key/value cannot be blank");
			conf.Properties.Add(Key, Value);
			return this;
		}

		public IProducerBuilder<T> Properties(IDictionary<string, string> Properties)
		{
			if(Properties.Count < 1)
				throw new ArgumentNullException("properties cannot be empty");
			Properties.SetOfKeyValuePairs().forEach(entry => checkArgument(StringUtils.isNotBlank(entry.Key) && StringUtils.isNotBlank(entry.Value), "properties' key/value cannot be blank"));
			Properties.ToList().ForEach(x => conf.Properties.Add(x.Key, x.Value));
			return this;
		}

		public IProducerBuilder<T> Intercept(params IProducerInterceptor[] Interceptors)
		{
			if (interceptorList == null)
			{
				interceptorList = new List<IProducerInterceptor>();
			}
			((List<IProducerInterceptor>)interceptorList).AddRange(Interceptors.ToArray());
			return this;
		}

		public IProducerBuilder<T> AutoUpdatePartitions(bool AutoUpdate)
		{
			conf.AutoUpdatePartitions = AutoUpdate;
			return this;
		}

		public IProducerBuilder<T> EnableMultiSchema(bool MultiSchema)
		{
			conf.MultiSchema = MultiSchema;
			return this;
		}

		private void SetMessageRoutingMode()
		{
			if (conf.MessageRoutingMode == null && conf.CustomMessageRouter == null)
			{
				MessageRoutingMode(Api.MessageRoutingMode.RoundRobinPartition);
			}
			else if (conf.MessageRoutingMode == null && conf.CustomMessageRouter != null)
			{
				MessageRoutingMode(Api.MessageRoutingMode.CustomPartition);
			}
			else if ((conf.MessageRoutingMode == Api.MessageRoutingMode.CustomPartition && conf.CustomMessageRouter == null) || (conf.MessageRoutingMode != Api.MessageRoutingMode.CustomPartition && conf.CustomMessageRouter != null))
			{
				throw new PulsarClientException("When 'messageRouter' is set, 'messageRoutingMode' " + "should be set as " + Api.MessageRoutingMode.CustomPartition);
			}
		}

		public string ToString()
		{
			return conf != null ? conf.ToString() : null;
		}

		object ICloneable.Clone()
		{
			return new ProducerBuilderImpl<T>(client, conf.Clone(), schema);
		}
	}

}