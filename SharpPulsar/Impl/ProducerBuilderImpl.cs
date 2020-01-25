using System;
using System.Collections.Generic;

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
	using VisibleForTesting = com.google.common.annotations.VisibleForTesting;


	using AccessLevel = lombok.AccessLevel;
	using Getter = lombok.Getter;
	using StringUtils = org.apache.commons.lang3.StringUtils;
	using BatcherBuilder = SharpPulsar.Api.BatcherBuilder;
	using ICompressionType = SharpPulsar.Api.ICompressionType;
	using CryptoKeyReader = SharpPulsar.Api.CryptoKeyReader;
	using HashingScheme = SharpPulsar.Api.HashingScheme;
	using MessageRouter = SharpPulsar.Api.MessageRouter;
	using MessageRoutingMode = SharpPulsar.Api.MessageRoutingMode;
	using Producer = SharpPulsar.Api.Producer;
	using SharpPulsar.Api;
	using ProducerCryptoFailureAction = SharpPulsar.Api.ProducerCryptoFailureAction;
	using PulsarClientException = SharpPulsar.Api.PulsarClientException;
	using SharpPulsar.Api;
	using ProducerInterceptor = SharpPulsar.Api.Interceptor.ProducerInterceptor;
	using ProducerInterceptorWrapper = SharpPulsar.Api.Interceptor.ProducerInterceptorWrapper;
	using ConfigurationDataUtils = SharpPulsar.Impl.Conf.ConfigurationDataUtils;
	using ProducerConfigurationData = SharpPulsar.Impl.Conf.ProducerConfigurationData;
	using FutureUtil = Org.Apache.Pulsar.Common.Util.FutureUtil;

	using NonNull = lombok.NonNull;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.google.common.@base.Preconditions.checkArgument;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Getter(AccessLevel.PUBLIC) public class ProducerBuilderImpl<T> implements SharpPulsar.api.ProducerBuilder<T>
	public class ProducerBuilderImpl<T> : ProducerBuilder<T>
	{

		private readonly PulsarClientImpl client;
		private ProducerConfigurationData conf;
		private Schema<T> schema;
		private IList<ProducerInterceptor> interceptorList;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @VisibleForTesting public ProducerBuilderImpl(PulsarClientImpl client, SharpPulsar.api.Schema<T> schema)
		public ProducerBuilderImpl(PulsarClientImpl Client, Schema<T> Schema) : this(Client, new ProducerConfigurationData(), Schema)
		{
		}

		private ProducerBuilderImpl(PulsarClientImpl Client, ProducerConfigurationData Conf, Schema<T> Schema)
		{
			this.client = Client;
			this.conf = Conf;
			this.schema = Schema;
		}

		/// <summary>
		/// Allow to override schema in builder implementation
		/// @return
		/// </summary>
		public virtual ProducerBuilder<T> Schema(Schema<T> Schema)
		{
			this.schema = Schema;
			return this;
		}

		public override ProducerBuilder<T> Clone()
		{
			return new ProducerBuilderImpl<T>(client, conf.Clone(), schema);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public SharpPulsar.api.Producer<T> create() throws SharpPulsar.api.PulsarClientException
		public override Producer<T> Create()
		{
			try
			{
				return CreateAsync().get();
			}
			catch (Exception E)
			{
				throw PulsarClientException.unwrap(E);
			}
		}

		public override CompletableFuture<Producer<T>> CreateAsync()
		{
			if (conf.TopicName == null)
			{
				return FutureUtil.failedFuture(new System.ArgumentException("Topic name must be set on the producer builder"));
			}

			try
			{
				SetMessageRoutingMode();
			}
			catch (PulsarClientException Pce)
			{
				return FutureUtil.failedFuture(Pce);
			}

			return interceptorList == null || interceptorList.Count == 0 ? client.CreateProducerAsync(conf, schema, null) : client.CreateProducerAsync(conf, schema, new ProducerInterceptors(interceptorList));
		}

		public override ProducerBuilder<T> LoadConf(IDictionary<string, object> Config)
		{
			conf = ConfigurationDataUtils.loadData(Config, conf, typeof(ProducerConfigurationData));
			return this;
		}

		public override ProducerBuilder<T> Topic(string TopicName)
		{
			checkArgument(StringUtils.isNotBlank(TopicName), "topicName cannot be blank");
			conf.TopicName = StringUtils.Trim(TopicName);
			return this;
		}

		public override ProducerBuilder<T> ProducerName(string ProducerName)
		{
			conf.ProducerName = ProducerName;
			return this;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override public SharpPulsar.api.ProducerBuilder<T> sendTimeout(int sendTimeout, @NonNull BAMCIS.Util.Concurrent.TimeUnit unit)
		public override ProducerBuilder<T> SendTimeout(int SendTimeout, BAMCIS.Util.Concurrent.TimeUnit Unit)
		{
			conf.SetSendTimeoutMs(SendTimeout, Unit);
			return this;
		}

		public override ProducerBuilder<T> MaxPendingMessages(int MaxPendingMessages)
		{
			conf.MaxPendingMessages = MaxPendingMessages;
			return this;
		}

		public override ProducerBuilder<T> MaxPendingMessagesAcrossPartitions(int MaxPendingMessagesAcrossPartitions)
		{
			conf.MaxPendingMessagesAcrossPartitions = MaxPendingMessagesAcrossPartitions;
			return this;
		}

		public override ProducerBuilder<T> BlockIfQueueFull(bool BlockIfQueueFull)
		{
			conf.BlockIfQueueFull = BlockIfQueueFull;
			return this;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override public SharpPulsar.api.ProducerBuilder<T> messageRoutingMode(@NonNull MessageRoutingMode messageRouteMode)
		public override ProducerBuilder<T> MessageRoutingMode(MessageRoutingMode MessageRouteMode)
		{
			conf.MessageRoutingMode = MessageRouteMode;
			return this;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override public SharpPulsar.api.ProducerBuilder<T> compressionType(@NonNull CompressionType compressionType)
		public override ProducerBuilder<T> CompressionType(ICompressionType CompressionType)
		{
			conf.CompressionType = CompressionType;
			return this;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override public SharpPulsar.api.ProducerBuilder<T> hashingScheme(@NonNull HashingScheme hashingScheme)
		public override ProducerBuilder<T> HashingScheme(HashingScheme HashingScheme)
		{
			conf.HashingScheme = HashingScheme;
			return this;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override public SharpPulsar.api.ProducerBuilder<T> messageRouter(@NonNull MessageRouter messageRouter)
		public override ProducerBuilder<T> MessageRouter(MessageRouter MessageRouter)
		{
			conf.CustomMessageRouter = MessageRouter;
			return this;
		}

		public override ProducerBuilder<T> EnableBatching(bool BatchMessagesEnabled)
		{
			conf.BatchingEnabled = BatchMessagesEnabled;
			return this;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override public SharpPulsar.api.ProducerBuilder<T> cryptoKeyReader(@NonNull CryptoKeyReader cryptoKeyReader)
		public override ProducerBuilder<T> CryptoKeyReader(CryptoKeyReader CryptoKeyReader)
		{
			conf.CryptoKeyReader = CryptoKeyReader;
			return this;
		}

		public override ProducerBuilder<T> AddEncryptionKey(string Key)
		{
			checkArgument(StringUtils.isNotBlank(Key), "Encryption key cannot be blank");
			conf.EncryptionKeys.add(Key);
			return this;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override public SharpPulsar.api.ProducerBuilder<T> cryptoFailureAction(@NonNull ProducerCryptoFailureAction action)
		public override ProducerBuilder<T> CryptoFailureAction(ProducerCryptoFailureAction Action)
		{
			conf.CryptoFailureAction = Action;
			return this;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override public SharpPulsar.api.ProducerBuilder<T> batchingMaxPublishDelay(long batchDelay, @NonNull BAMCIS.Util.Concurrent.TimeUnit BAMCIS.Util.Concurrent.TimeUnit)
		public override ProducerBuilder<T> BatchingMaxPublishDelay(long BatchDelay, BAMCIS.Util.Concurrent.TimeUnit BAMCIS.Util.Concurrent.TimeUnit)
		{
			conf.SetBatchingMaxPublishDelayMicros(BatchDelay, BAMCIS.Util.Concurrent.TimeUnit);
			return this;
		}

		public override ProducerBuilder<T> RoundRobinRouterBatchingPartitionSwitchFrequency(int Frequency)
		{
			conf.BatchingPartitionSwitchFrequencyByPublishDelay = Frequency;
			return this;
		}

		public override ProducerBuilder<T> BatchingMaxMessages(int BatchMessagesMaxMessagesPerBatch)
		{
			conf.BatchingMaxMessages = BatchMessagesMaxMessagesPerBatch;
			return this;
		}

		public override ProducerBuilder<T> BatchingMaxBytes(int BatchingMaxBytes)
		{
			conf.BatchingMaxBytes = BatchingMaxBytes;
			return this;
		}

		public override ProducerBuilder<T> BatcherBuilder(BatcherBuilder BatcherBuilder)
		{
			conf.BatcherBuilder = BatcherBuilder;
			return this;
		}


		public override ProducerBuilder<T> InitialSequenceId(long InitialSequenceId)
		{
			conf.InitialSequenceId = InitialSequenceId;
			return this;
		}

		public override ProducerBuilder<T> Property(string Key, string Value)
		{
			checkArgument(StringUtils.isNotBlank(Key) && StringUtils.isNotBlank(Value), "property key/value cannot be blank");
			conf.Properties.put(Key, Value);
			return this;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override public SharpPulsar.api.ProducerBuilder<T> properties(@NonNull Map<String, String> properties)
		public override ProducerBuilder<T> Properties(IDictionary<string, string> Properties)
		{
			checkArgument(Properties.Count > 0, "properties cannot be empty");
			Properties.SetOfKeyValuePairs().forEach(entry => checkArgument(StringUtils.isNotBlank(entry.Key) && StringUtils.isNotBlank(entry.Value), "properties' key/value cannot be blank"));
			conf.Properties.putAll(Properties);
			return this;
		}

		public override ProducerBuilder<T> Intercept(params ProducerInterceptor[] Interceptors)
		{
			if (interceptorList == null)
			{
				interceptorList = new List<ProducerInterceptor>();
			}
			((List<ProducerInterceptor>)interceptorList).AddRange(Arrays.asList(Interceptors));
			return this;
		}

		[Obsolete]
		public override ProducerBuilder<T> Intercept(params ProducerInterceptor<T>[] Interceptors)
		{
			if (interceptorList == null)
			{
				interceptorList = new List<ProducerInterceptor>();
			}
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
			((List<ProducerInterceptor>)interceptorList).AddRange(java.util.interceptors.Select(ProducerInterceptorWrapper::new).ToList());
			return this;
		}
		public override ProducerBuilder<T> AutoUpdatePartitions(bool AutoUpdate)
		{
			conf.AutoUpdatePartitions = AutoUpdate;
			return this;
		}

		public override ProducerBuilder<T> EnableMultiSchema(bool MultiSchema)
		{
			conf.MultiSchema = MultiSchema;
			return this;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void setMessageRoutingMode() throws SharpPulsar.api.PulsarClientException
		private void SetMessageRoutingMode()
		{
			if (conf.MessageRoutingMode == null && conf.CustomMessageRouter == null)
			{
				MessageRoutingMode(MessageRoutingMode.RoundRobinPartition);
			}
			else if (conf.MessageRoutingMode == null && conf.CustomMessageRouter != null)
			{
				MessageRoutingMode(MessageRoutingMode.CustomPartition);
			}
			else if ((conf.MessageRoutingMode == MessageRoutingMode.CustomPartition && conf.CustomMessageRouter == null) || (conf.MessageRoutingMode != MessageRoutingMode.CustomPartition && conf.CustomMessageRouter != null))
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