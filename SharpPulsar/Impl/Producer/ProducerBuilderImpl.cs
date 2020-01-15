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
namespace org.apache.pulsar.client.impl
{
	using VisibleForTesting = com.google.common.annotations.VisibleForTesting;


	using AccessLevel = lombok.AccessLevel;
	using Getter = lombok.Getter;
	using StringUtils = org.apache.commons.lang3.StringUtils;
	using BatcherBuilder = org.apache.pulsar.client.api.BatcherBuilder;
	using CompressionType = org.apache.pulsar.client.api.CompressionType;
	using CryptoKeyReader = org.apache.pulsar.client.api.CryptoKeyReader;
	using HashingScheme = org.apache.pulsar.client.api.HashingScheme;
	using MessageRouter = org.apache.pulsar.client.api.MessageRouter;
	using MessageRoutingMode = org.apache.pulsar.client.api.MessageRoutingMode;
	using Producer = org.apache.pulsar.client.api.Producer;
	using ProducerBuilder = org.apache.pulsar.client.api.ProducerBuilder;
	using ProducerCryptoFailureAction = org.apache.pulsar.client.api.ProducerCryptoFailureAction;
	using PulsarClientException = org.apache.pulsar.client.api.PulsarClientException;
	using Schema = org.apache.pulsar.client.api.Schema;
	using ProducerInterceptor = org.apache.pulsar.client.api.interceptor.ProducerInterceptor;
	using ProducerInterceptorWrapper = org.apache.pulsar.client.api.interceptor.ProducerInterceptorWrapper;
	using ConfigurationDataUtils = org.apache.pulsar.client.impl.conf.ConfigurationDataUtils;
	using ProducerConfigurationData = org.apache.pulsar.client.impl.conf.ProducerConfigurationData;
	using FutureUtil = org.apache.pulsar.common.util.FutureUtil;

	using NonNull = lombok.NonNull;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.google.common.@base.Preconditions.checkArgument;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Getter(AccessLevel.PUBLIC) public class ProducerBuilderImpl<T> implements org.apache.pulsar.client.api.ProducerBuilder<T>
	public class ProducerBuilderImpl<T> : ProducerBuilder<T>
	{

		private readonly PulsarClientImpl client;
		private ProducerConfigurationData conf;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		private Schema<T> schema_Conflict;
		private IList<ProducerInterceptor> interceptorList;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @VisibleForTesting public ProducerBuilderImpl(PulsarClientImpl client, org.apache.pulsar.client.api.Schema<T> schema)
		public ProducerBuilderImpl(PulsarClientImpl client, Schema<T> schema) : this(client, new ProducerConfigurationData(), schema)
		{
		}

		private ProducerBuilderImpl(PulsarClientImpl client, ProducerConfigurationData conf, Schema<T> schema)
		{
			this.client = client;
			this.conf = conf;
			this.schema_Conflict = schema;
		}

		/// <summary>
		/// Allow to override schema in builder implementation
		/// @return
		/// </summary>
		public virtual ProducerBuilder<T> schema(Schema<T> schema)
		{
			this.schema_Conflict = schema;
			return this;
		}

		public override ProducerBuilder<T> clone()
		{
			return new ProducerBuilderImpl<T>(client, conf.clone(), schema_Conflict);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public org.apache.pulsar.client.api.Producer<T> create() throws org.apache.pulsar.client.api.PulsarClientException
		public override Producer<T> create()
		{
			try
			{
				return createAsync().get();
			}
			catch (Exception e)
			{
				throw PulsarClientException.unwrap(e);
			}
		}

		public override CompletableFuture<Producer<T>> createAsync()
		{
			if (conf.TopicName == null)
			{
				return FutureUtil.failedFuture(new System.ArgumentException("Topic name must be set on the producer builder"));
			}

			try
			{
				setMessageRoutingMode();
			}
			catch (PulsarClientException pce)
			{
				return FutureUtil.failedFuture(pce);
			}

			return interceptorList == null || interceptorList.Count == 0 ? client.createProducerAsync(conf, schema_Conflict, null) : client.createProducerAsync(conf, schema_Conflict, new ProducerInterceptors(interceptorList));
		}

		public override ProducerBuilder<T> loadConf(IDictionary<string, object> config)
		{
			conf = ConfigurationDataUtils.loadData(config, conf, typeof(ProducerConfigurationData));
			return this;
		}

		public override ProducerBuilder<T> topic(string topicName)
		{
			checkArgument(StringUtils.isNotBlank(topicName), "topicName cannot be blank");
			conf.TopicName = StringUtils.Trim(topicName);
			return this;
		}

		public override ProducerBuilder<T> producerName(string producerName)
		{
			conf.ProducerName = producerName;
			return this;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override public org.apache.pulsar.client.api.ProducerBuilder<T> sendTimeout(int sendTimeout, @NonNull TimeUnit unit)
		public override ProducerBuilder<T> sendTimeout(int sendTimeout, TimeUnit unit)
		{
			conf.setSendTimeoutMs(sendTimeout, unit);
			return this;
		}

		public override ProducerBuilder<T> maxPendingMessages(int maxPendingMessages)
		{
			conf.MaxPendingMessages = maxPendingMessages;
			return this;
		}

		public override ProducerBuilder<T> maxPendingMessagesAcrossPartitions(int maxPendingMessagesAcrossPartitions)
		{
			conf.MaxPendingMessagesAcrossPartitions = maxPendingMessagesAcrossPartitions;
			return this;
		}

		public override ProducerBuilder<T> blockIfQueueFull(bool blockIfQueueFull)
		{
			conf.BlockIfQueueFull = blockIfQueueFull;
			return this;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override public org.apache.pulsar.client.api.ProducerBuilder<T> messageRoutingMode(@NonNull MessageRoutingMode messageRouteMode)
		public override ProducerBuilder<T> messageRoutingMode(MessageRoutingMode messageRouteMode)
		{
			conf.MessageRoutingMode = messageRouteMode;
			return this;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override public org.apache.pulsar.client.api.ProducerBuilder<T> compressionType(@NonNull CompressionType compressionType)
		public override ProducerBuilder<T> compressionType(CompressionType compressionType)
		{
			conf.CompressionType = compressionType;
			return this;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override public org.apache.pulsar.client.api.ProducerBuilder<T> hashingScheme(@NonNull HashingScheme hashingScheme)
		public override ProducerBuilder<T> hashingScheme(HashingScheme hashingScheme)
		{
			conf.HashingScheme = hashingScheme;
			return this;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override public org.apache.pulsar.client.api.ProducerBuilder<T> messageRouter(@NonNull MessageRouter messageRouter)
		public override ProducerBuilder<T> messageRouter(MessageRouter messageRouter)
		{
			conf.CustomMessageRouter = messageRouter;
			return this;
		}

		public override ProducerBuilder<T> enableBatching(bool batchMessagesEnabled)
		{
			conf.BatchingEnabled = batchMessagesEnabled;
			return this;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override public org.apache.pulsar.client.api.ProducerBuilder<T> cryptoKeyReader(@NonNull CryptoKeyReader cryptoKeyReader)
		public override ProducerBuilder<T> cryptoKeyReader(CryptoKeyReader cryptoKeyReader)
		{
			conf.CryptoKeyReader = cryptoKeyReader;
			return this;
		}

		public override ProducerBuilder<T> addEncryptionKey(string key)
		{
			checkArgument(StringUtils.isNotBlank(key), "Encryption key cannot be blank");
			conf.EncryptionKeys.add(key);
			return this;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override public org.apache.pulsar.client.api.ProducerBuilder<T> cryptoFailureAction(@NonNull ProducerCryptoFailureAction action)
		public override ProducerBuilder<T> cryptoFailureAction(ProducerCryptoFailureAction action)
		{
			conf.CryptoFailureAction = action;
			return this;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override public org.apache.pulsar.client.api.ProducerBuilder<T> batchingMaxPublishDelay(long batchDelay, @NonNull TimeUnit timeUnit)
		public override ProducerBuilder<T> batchingMaxPublishDelay(long batchDelay, TimeUnit timeUnit)
		{
			conf.setBatchingMaxPublishDelayMicros(batchDelay, timeUnit);
			return this;
		}

		public override ProducerBuilder<T> roundRobinRouterBatchingPartitionSwitchFrequency(int frequency)
		{
			conf.BatchingPartitionSwitchFrequencyByPublishDelay = frequency;
			return this;
		}

		public override ProducerBuilder<T> batchingMaxMessages(int batchMessagesMaxMessagesPerBatch)
		{
			conf.BatchingMaxMessages = batchMessagesMaxMessagesPerBatch;
			return this;
		}

		public override ProducerBuilder<T> batchingMaxBytes(int batchingMaxBytes)
		{
			conf.BatchingMaxBytes = batchingMaxBytes;
			return this;
		}

		public override ProducerBuilder<T> batcherBuilder(BatcherBuilder batcherBuilder)
		{
			conf.BatcherBuilder = batcherBuilder;
			return this;
		}


		public override ProducerBuilder<T> initialSequenceId(long initialSequenceId)
		{
			conf.InitialSequenceId = initialSequenceId;
			return this;
		}

		public override ProducerBuilder<T> property(string key, string value)
		{
			checkArgument(StringUtils.isNotBlank(key) && StringUtils.isNotBlank(value), "property key/value cannot be blank");
			conf.Properties.put(key, value);
			return this;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override public org.apache.pulsar.client.api.ProducerBuilder<T> properties(@NonNull Map<String, String> properties)
		public override ProducerBuilder<T> properties(IDictionary<string, string> properties)
		{
			checkArgument(!properties.Empty, "properties cannot be empty");
			properties.entrySet().forEach(entry => checkArgument(StringUtils.isNotBlank(entry.Key) && StringUtils.isNotBlank(entry.Value), "properties' key/value cannot be blank"));
			conf.Properties.putAll(properties);
			return this;
		}

		public override ProducerBuilder<T> intercept(params ProducerInterceptor[] interceptors)
		{
			if (interceptorList == null)
			{
				interceptorList = new List<ProducerInterceptor>();
			}
			((List<ProducerInterceptor>)interceptorList).AddRange(Arrays.asList(interceptors));
			return this;
		}

		[Obsolete]
		public override ProducerBuilder<T> intercept(params org.apache.pulsar.client.api.ProducerInterceptor<T>[] interceptors)
		{
			if (interceptorList == null)
			{
				interceptorList = new List<ProducerInterceptor>();
			}
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
			((List<ProducerInterceptor>)interceptorList).AddRange(java.util.interceptors.Select(ProducerInterceptorWrapper::new).ToList());
			return this;
		}
		public override ProducerBuilder<T> autoUpdatePartitions(bool autoUpdate)
		{
			conf.AutoUpdatePartitions = autoUpdate;
			return this;
		}

		public override ProducerBuilder<T> enableMultiSchema(bool multiSchema)
		{
			conf.MultiSchema = multiSchema;
			return this;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void setMessageRoutingMode() throws org.apache.pulsar.client.api.PulsarClientException
		private void setMessageRoutingMode()
		{
			if (conf.MessageRoutingMode == null && conf.CustomMessageRouter == null)
			{
				messageRoutingMode(MessageRoutingMode.RoundRobinPartition);
			}
			else if (conf.MessageRoutingMode == null && conf.CustomMessageRouter != null)
			{
				messageRoutingMode(MessageRoutingMode.CustomPartition);
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