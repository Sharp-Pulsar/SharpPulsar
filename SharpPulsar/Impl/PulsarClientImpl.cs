using System;
using System.Collections.Generic;
using System.Threading;

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
	using CacheBuilder = com.google.common.cache.CacheBuilder;
	using CacheLoader = com.google.common.cache.CacheLoader;
	using LoadingCache = com.google.common.cache.LoadingCache;
	using Lists = com.google.common.collect.Lists;
	using Maps = com.google.common.collect.Maps;

	using EventLoopGroup = io.netty.channel.EventLoopGroup;
	using HashedWheelTimer = io.netty.util.HashedWheelTimer;
	using Timer = io.netty.util.Timer;
	using DefaultThreadFactory = io.netty.util.concurrent.DefaultThreadFactory;


	using StringUtils = org.apache.commons.lang3.StringUtils;
	using Consumer = org.apache.pulsar.client.api.Consumer;
	using ConsumerBuilder = org.apache.pulsar.client.api.ConsumerBuilder;
	using Producer = org.apache.pulsar.client.api.Producer;
	using ProducerBuilder = org.apache.pulsar.client.api.ProducerBuilder;
	using PulsarClient = org.apache.pulsar.client.api.PulsarClient;
	using PulsarClientException = org.apache.pulsar.client.api.PulsarClientException;
	using Reader = org.apache.pulsar.client.api.Reader;
	using ReaderBuilder = org.apache.pulsar.client.api.ReaderBuilder;
	using RegexSubscriptionMode = org.apache.pulsar.client.api.RegexSubscriptionMode;
	using Schema = org.apache.pulsar.client.api.Schema;
	using SubscriptionType = org.apache.pulsar.client.api.SubscriptionType;
	using SchemaInfoProvider = org.apache.pulsar.client.api.schema.SchemaInfoProvider;
	using IAuthenticationFactory = org.apache.pulsar.client.api.IAuthenticationFactory;
	using TransactionBuilder = org.apache.pulsar.client.api.transaction.TransactionBuilder;
	using SubscriptionMode = org.apache.pulsar.client.impl.ConsumerImpl.SubscriptionMode;
	using ClientConfigurationData = org.apache.pulsar.client.impl.conf.ClientConfigurationData;
	using org.apache.pulsar.client.impl.conf;
	using ProducerConfigurationData = org.apache.pulsar.client.impl.conf.ProducerConfigurationData;
	using org.apache.pulsar.client.impl.conf;
	using AutoConsumeSchema = org.apache.pulsar.client.impl.schema.AutoConsumeSchema;
	using org.apache.pulsar.client.impl.schema;
	using MultiVersionSchemaInfoProvider = org.apache.pulsar.client.impl.schema.generic.MultiVersionSchemaInfoProvider;
	using TransactionBuilderImpl = org.apache.pulsar.client.impl.transaction.TransactionBuilderImpl;
	using ExecutorProvider = org.apache.pulsar.client.util.ExecutorProvider;
	using Mode = org.apache.pulsar.common.api.proto.PulsarApi.CommandGetTopicsOfNamespace.Mode;
	using NamespaceName = org.apache.pulsar.common.naming.NamespaceName;
	using TopicDomain = org.apache.pulsar.common.naming.TopicDomain;
	using TopicName = org.apache.pulsar.common.naming.TopicName;
	using PartitionedTopicMetadata = org.apache.pulsar.common.partition.PartitionedTopicMetadata;
	using SchemaInfo = org.apache.pulsar.common.schema.SchemaInfo;
	using FutureUtil = org.apache.pulsar.common.util.FutureUtil;
	using EventLoopUtil = org.apache.pulsar.common.util.netty.EventLoopUtil;
	using Logger = org.slf4j.Logger;
	using LoggerFactory = org.slf4j.LoggerFactory;
    using SharpPulsar.Interface;
    using SharpPulsar.Util;
    using SharpPulsar.Impl.Producer;

    public class PulsarClientImpl : IPulsarClient
	{

		private static readonly Logger log = LoggerFactory.getLogger(typeof(PulsarClientImpl));

		private readonly ClientConfigurationData conf;
		private LookupService lookup;
		private readonly ConnectionPool cnxPool;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		private readonly Timer timer_Conflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		private readonly ExecutorProvider externalExecutorProvider_Conflict;

		internal enum State
		{
			Open,
			Closing,
			Closed
		}

		private readonly AtomicReference<State> state = new AtomicReference<State>();
		private readonly IdentityHashMap<ProducerBase<object>, bool> producers;
		private readonly IdentityHashMap<ConsumerBase<object>, bool> consumers;

		private readonly AtomicLong producerIdGenerator = new AtomicLong();
		private readonly AtomicLong consumerIdGenerator = new AtomicLong();
		private readonly AtomicLong requestIdGenerator = new AtomicLong();

		private readonly EventLoopGroup eventLoopGroup_Conflict;

		private readonly LoadingCache<string, SchemaInfoProvider> schemaProviderLoadingCache = CacheBuilder.newBuilder().maximumSize(100000).expireAfterAccess(30, TimeUnit.MINUTES).build(new CacheLoaderAnonymousInnerClass());

		private class CacheLoaderAnonymousInnerClass : CacheLoader<string, SchemaInfoProvider>
		{

			public override SchemaInfoProvider load(string topicName)
			{
				return outerInstance.newSchemaProvider(topicName);
			}
		}

		private readonly Clock clientClock;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public PulsarClientImpl(org.apache.pulsar.client.impl.conf.ClientConfigurationData conf) throws org.apache.pulsar.client.api.PulsarClientException
		public PulsarClientImpl(ClientConfigurationData conf) : this(conf, getEventLoopGroup(conf))
		{
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public PulsarClientImpl(org.apache.pulsar.client.impl.conf.ClientConfigurationData conf, io.netty.channel.EventLoopGroup eventLoopGroup) throws org.apache.pulsar.client.api.PulsarClientException
		public PulsarClientImpl(ClientConfigurationData conf, EventLoopGroup eventLoopGroup) : this(conf, eventLoopGroup, new ConnectionPool(conf, eventLoopGroup))
		{
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public PulsarClientImpl(org.apache.pulsar.client.impl.conf.ClientConfigurationData conf, io.netty.channel.EventLoopGroup eventLoopGroup, ConnectionPool cnxPool) throws org.apache.pulsar.client.api.PulsarClientException
		public PulsarClientImpl(ClientConfigurationData conf, EventLoopGroup eventLoopGroup, ConnectionPool cnxPool)
		{
			if (conf == null || isBlank(conf.ServiceUrl) || eventLoopGroup == null)
			{
				throw new PulsarClientException.InvalidConfigurationException("Invalid client configuration");
			}
			this.eventLoopGroup_Conflict = eventLoopGroup;
			Auth = conf;
			this.conf = conf;
			this.clientClock = conf.Clock;
			conf.Authentication.start();
			this.cnxPool = cnxPool;
			externalExecutorProvider_Conflict = new ExecutorProvider(conf.NumListenerThreads, getThreadFactory("pulsar-external-listener"));
			if (conf.ServiceUrl.StartsWith("http"))
			{
				lookup = new HttpLookupService(conf, eventLoopGroup);
			}
			else
			{
				lookup = new BinaryProtoLookupService(this, conf.ServiceUrl, conf.UseTls, externalExecutorProvider_Conflict.Executor);
			}
			timer_Conflict = new HashedWheelTimer(getThreadFactory("pulsar-timer"), 1, TimeUnit.MILLISECONDS);
			producers = Maps.newIdentityHashMap();
			consumers = Maps.newIdentityHashMap();
			state.set(State.Open);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void setAuth(org.apache.pulsar.client.impl.conf.ClientConfigurationData conf) throws org.apache.pulsar.client.api.PulsarClientException
		private ClientConfigurationData Auth
		{
			set
			{
				if (StringUtils.isBlank(value.AuthPluginClassName) || StringUtils.isBlank(value.AuthParams))
				{
					return;
				}
    
				value.Authentication = IAuthenticationFactory.create(value.AuthPluginClassName, value.AuthParams);
			}
		}

		public virtual ClientConfigurationData Configuration
		{
			get
			{
				return conf;
			}
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @VisibleForTesting public java.time.Clock getClientClock()
		public virtual Clock ClientClock
		{
			get
			{
				return clientClock;
			}
		}

		public override ProducerBuilder<sbyte[]> newProducer()
		{
			return new ProducerBuilderImpl<sbyte[]>(this, Schema.BYTES);
		}

		public override ProducerBuilder<T> newProducer<T>(Schema<T> schema)
		{
			return new ProducerBuilderImpl<T>(this, schema);
		}

		public override ConsumerBuilder<sbyte[]> newConsumer()
		{
			return new ConsumerBuilderImpl<sbyte[]>(this, Schema.BYTES);
		}

		public override ConsumerBuilder<T> newConsumer<T>(Schema<T> schema)
		{
			return new ConsumerBuilderImpl<T>(this, schema);
		}

		public override ReaderBuilder<sbyte[]> newReader()
		{
			return new ReaderBuilderImpl<sbyte[]>(this, Schema.BYTES);
		}

		public override ReaderBuilder<T> newReader<T>(Schema<T> schema)
		{
			return new ReaderBuilderImpl<T>(this, schema);
		}

		public virtual CompletableFuture<Producer<sbyte[]>> createProducerAsync(ProducerConfigurationData conf)
		{
			return createProducerAsync(conf, Schema.BYTES, null);
		}

		public virtual CompletableFuture<Producer<T>> createProducerAsync<T>(ProducerConfigurationData conf, Schema<T> schema)
		{
			return createProducerAsync(conf, schema, null);
		}

		public virtual CompletableFuture<Producer<T>> createProducerAsync<T>(ProducerConfigurationData conf, Schema<T> schema, ProducerInterceptors interceptors)
		{
			if (conf == null)
			{
				return FutureUtil.failedFuture(new PulsarClientException.InvalidConfigurationException("Producer configuration undefined"));
			}

			if (schema is AutoConsumeSchema)
			{
				return FutureUtil.failedFuture(new PulsarClientException.InvalidConfigurationException("AutoConsumeSchema is only used by consumers to detect schemas automatically"));
			}

			if (state.get() != State.Open)
			{
				return FutureUtil.failedFuture(new PulsarClientException.AlreadyClosedException("Client already closed : state = " + state.get()));
			}

			string topic = conf.TopicName;

			if (!TopicName.isValid(topic))
			{
				return FutureUtil.failedFuture(new PulsarClientException.InvalidTopicNameException("Invalid topic name: '" + topic + "'"));
			}

			if (schema is AutoProduceBytesSchema)
			{
				AutoProduceBytesSchema autoProduceBytesSchema = (AutoProduceBytesSchema) schema;
				if (autoProduceBytesSchema.schemaInitialized())
				{
					return createProducerAsync(topic, conf, schema, interceptors);
				}
				return lookup.getSchema(TopicName.get(conf.TopicName)).thenCompose(schemaInfoOptional =>
				{
				if (schemaInfoOptional.Present)
				{
					autoProduceBytesSchema.Schema = Schema.getSchema(schemaInfoOptional.get());
				}
				else
				{
					autoProduceBytesSchema.Schema = Schema.BYTES;
				}
				return createProducerAsync(topic, conf, schema, interceptors);
				});
			}
			else
			{
				return createProducerAsync(topic, conf, schema, interceptors);
			}

		}

		private CompletableFuture<Producer<T>> createProducerAsync<T>(string topic, ProducerConfigurationData conf, Schema<T> schema, ProducerInterceptors interceptors)
		{
			CompletableFuture<Producer<T>> producerCreatedFuture = new CompletableFuture<Producer<T>>();

			getPartitionedTopicMetadata(topic).thenAccept(metadata =>
			{
			if (log.DebugEnabled)
			{
				log.debug("[{}] Received topic metadata. partitions: {}", topic, metadata.partitions);
			}
			ProducerBase<T> producer;
			if (metadata.partitions > 0)
			{
				producer = new PartitionedProducerImpl<T>(PulsarClientImpl.this, topic, conf, metadata.partitions, producerCreatedFuture, schema, interceptors);
			}
			else
			{
				producer = new ProducerImpl<T>(PulsarClientImpl.this, topic, conf, producerCreatedFuture, -1, schema, interceptors);
			}
			lock (producers)
			{
				producers.put(producer, true);
			}
			}).exceptionally(ex =>
			{
			log.warn("[{}] Failed to get partitioned topic metadata: {}", topic, ex.Message);
			producerCreatedFuture.completeExceptionally(ex);
			return null;
		});

			return producerCreatedFuture;
		}

		public virtual CompletableFuture<Consumer<sbyte[]>> subscribeAsync(ConsumerConfigurationData<sbyte[]> conf)
		{
			return subscribeAsync(conf, Schema.BYTES, null);
		}

		public virtual CompletableFuture<Consumer<T>> subscribeAsync<T>(ConsumerConfigurationData<T> conf, Schema<T> schema, ConsumerInterceptors<T> interceptors)
		{
			if (state.get() != State.Open)
			{
				return FutureUtil.failedFuture(new PulsarClientException.AlreadyClosedException("Client already closed"));
			}

			if (conf == null)
			{
				return FutureUtil.failedFuture(new PulsarClientException.InvalidConfigurationException("Consumer configuration undefined"));
			}

			if (!conf.TopicNames.All(TopicName.isValid))
			{
				return FutureUtil.failedFuture(new PulsarClientException.InvalidTopicNameException("Invalid topic name"));
			}

			if (isBlank(conf.SubscriptionName))
			{
				return FutureUtil.failedFuture(new PulsarClientException.InvalidConfigurationException("Empty subscription name"));
			}

			if (conf.ReadCompacted && (!conf.TopicNames.All(topic => TopicName.get(topic).Domain == TopicDomain.persistent) || (conf.SubscriptionType != SubscriptionType.Exclusive && conf.SubscriptionType != SubscriptionType.Failover)))
			{
				return FutureUtil.failedFuture(new PulsarClientException.InvalidConfigurationException("Read compacted can only be used with exclusive of failover persistent subscriptions"));
			}

			if (conf.ConsumerEventListener != null && conf.SubscriptionType != SubscriptionType.Failover)
			{
				return FutureUtil.failedFuture(new PulsarClientException.InvalidConfigurationException("Active consumer listener is only supported for failover subscription"));
			}

			if (conf.TopicsPattern != null)
			{
				// If use topicsPattern, we should not use topic(), and topics() method.
				if (!conf.TopicNames.Empty)
				{
					return FutureUtil.failedFuture(new System.ArgumentException("Topic names list must be null when use topicsPattern"));
				}
				return patternTopicSubscribeAsync(conf, schema, interceptors);
			}
			else if (conf.TopicNames.size() == 1)
			{
				return singleTopicSubscribeAsync(conf, schema, interceptors);
			}
			else
			{
				return multiTopicSubscribeAsync(conf, schema, interceptors);
			}
		}

		private CompletableFuture<Consumer<T>> singleTopicSubscribeAsync<T>(ConsumerConfigurationData<T> conf, Schema<T> schema, ConsumerInterceptors<T> interceptors)
		{
			return preProcessSchemaBeforeSubscribe(this, schema, conf.SingleTopic).thenCompose(ignored => doSingleTopicSubscribeAsync(conf, schema, interceptors));
		}

		private CompletableFuture<Consumer<T>> doSingleTopicSubscribeAsync<T>(ConsumerConfigurationData<T> conf, Schema<T> schema, ConsumerInterceptors<T> interceptors)
		{
			CompletableFuture<Consumer<T>> consumerSubscribedFuture = new CompletableFuture<Consumer<T>>();

			string topic = conf.SingleTopic;

			getPartitionedTopicMetadata(topic).thenAccept(metadata =>
			{
			if (log.DebugEnabled)
			{
				log.debug("[{}] Received topic metadata. partitions: {}", topic, metadata.partitions);
			}
			ConsumerBase<T> consumer;
			ExecutorService listenerThread = externalExecutorProvider_Conflict.Executor;
			if (metadata.partitions > 0)
			{
				consumer = MultiTopicsConsumerImpl.createPartitionedConsumer(PulsarClientImpl.this, conf, listenerThread, consumerSubscribedFuture, metadata.partitions, schema, interceptors);
			}
			else
			{
				int partitionIndex = TopicName.getPartitionIndex(topic);
				consumer = ConsumerImpl.newConsumerImpl(PulsarClientImpl.this, topic, conf, listenerThread, partitionIndex, false, consumerSubscribedFuture, SubscriptionMode.Durable, null, schema, interceptors, true);
			}
			lock (consumers)
			{
				consumers.put(consumer, true);
			}
			}).exceptionally(ex =>
			{
			log.warn("[{}] Failed to get partitioned topic metadata", topic, ex);
			consumerSubscribedFuture.completeExceptionally(ex);
			return null;
		});

			return consumerSubscribedFuture;
		}

		private CompletableFuture<Consumer<T>> multiTopicSubscribeAsync<T>(ConsumerConfigurationData<T> conf, Schema<T> schema, ConsumerInterceptors<T> interceptors)
		{
			CompletableFuture<Consumer<T>> consumerSubscribedFuture = new CompletableFuture<Consumer<T>>();

			ConsumerBase<T> consumer = new MultiTopicsConsumerImpl<T>(PulsarClientImpl.this, conf, externalExecutorProvider_Conflict.Executor, consumerSubscribedFuture, schema, interceptors, true);

			lock (consumers)
			{
				consumers.put(consumer, true);
			}

			return consumerSubscribedFuture;
		}

		public virtual CompletableFuture<Consumer<sbyte[]>> patternTopicSubscribeAsync(ConsumerConfigurationData<sbyte[]> conf)
		{
			return patternTopicSubscribeAsync(conf, Schema.BYTES, null);
		}

		private CompletableFuture<Consumer<T>> patternTopicSubscribeAsync<T>(ConsumerConfigurationData<T> conf, Schema<T> schema, ConsumerInterceptors<T> interceptors)
		{
			string regex = conf.TopicsPattern.pattern();
			Mode subscriptionMode = convertRegexSubscriptionMode(conf.RegexSubscriptionMode);
			TopicName destination = TopicName.get(regex);
			NamespaceName namespaceName = destination.NamespaceObject;

			CompletableFuture<Consumer<T>> consumerSubscribedFuture = new CompletableFuture<Consumer<T>>();
			lookup.getTopicsUnderNamespace(namespaceName, subscriptionMode).thenAccept(topics =>
			{
			if (log.DebugEnabled)
			{
				log.debug("Get topics under namespace {}, topics.size: {}", namespaceName.ToString(), topics.size());
				topics.forEach(topicName => log.debug("Get topics under namespace {}, topic: {}", namespaceName.ToString(), topicName));
			}
			IList<string> topicsList = topicsPatternFilter(topics, conf.TopicsPattern);
			conf.TopicNames.addAll(topicsList);
			ConsumerBase<T> consumer = new PatternMultiTopicsConsumerImpl<T>(conf.TopicsPattern, PulsarClientImpl.this, conf, externalExecutorProvider_Conflict.Executor, consumerSubscribedFuture, schema, subscriptionMode, interceptors);
			lock (consumers)
			{
				consumers.put(consumer, true);
			}
			}).exceptionally(ex =>
			{
			log.warn("[{}] Failed to get topics under namespace", namespaceName);
			consumerSubscribedFuture.completeExceptionally(ex);
			return null;
		});

			return consumerSubscribedFuture;
		}

		// get topics that match 'topicsPattern' from original topics list
		// return result should contain only topic names, without partition part
		public static IList<string> topicsPatternFilter(IList<string> original, Pattern topicsPattern)
		{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.regex.Pattern shortenedTopicsPattern = topicsPattern.toString().contains("://") ? java.util.regex.Pattern.compile(topicsPattern.toString().split("\\:\\/\\/")[1]) : topicsPattern;
			Pattern shortenedTopicsPattern = topicsPattern.ToString().Contains("://") ? Pattern.compile(topicsPattern.ToString().Split("\\:\\/\\/", true)[1]) : topicsPattern;

			return original.Select(TopicName.get).Select(TopicName.toString).Where(topic => shortenedTopicsPattern.matcher(topic.Split("\\:\\/\\/")[1]).matches()).ToList();
		}

		public virtual CompletableFuture<Reader<sbyte[]>> createReaderAsync(ReaderConfigurationData<sbyte[]> conf)
		{
			return createReaderAsync(conf, Schema.BYTES);
		}

		public virtual CompletableFuture<Reader<T>> createReaderAsync<T>(ReaderConfigurationData<T> conf, Schema<T> schema)
		{
			return preProcessSchemaBeforeSubscribe(this, schema, conf.TopicName).thenCompose(ignored => doCreateReaderAsync(conf, schema));
		}

		internal virtual CompletableFuture<Reader<T>> doCreateReaderAsync<T>(ReaderConfigurationData<T> conf, Schema<T> schema)
		{
			if (state.get() != State.Open)
			{
				return FutureUtil.failedFuture(new PulsarClientException.AlreadyClosedException("Client already closed"));
			}

			if (conf == null)
			{
				return FutureUtil.failedFuture(new PulsarClientException.InvalidConfigurationException("Consumer configuration undefined"));
			}

			string topic = conf.TopicName;

			if (!TopicName.isValid(topic))
			{
				return FutureUtil.failedFuture(new PulsarClientException.InvalidTopicNameException("Invalid topic name"));
			}

			if (conf.StartMessageId == null)
			{
				return FutureUtil.failedFuture(new PulsarClientException.InvalidConfigurationException("Invalid startMessageId"));
			}

			CompletableFuture<Reader<T>> readerFuture = new CompletableFuture<Reader<T>>();

			getPartitionedTopicMetadata(topic).thenAccept(metadata =>
			{
			if (log.DebugEnabled)
			{
				log.debug("[{}] Received topic metadata. partitions: {}", topic, metadata.partitions);
			}
			if (metadata.partitions > 0)
			{
				readerFuture.completeExceptionally(new PulsarClientException("Topic reader cannot be created on a partitioned topic"));
				return;
			}
			CompletableFuture<Consumer<T>> consumerSubscribedFuture = new CompletableFuture<Consumer<T>>();
			ExecutorService listenerThread = externalExecutorProvider_Conflict.Executor;
			ReaderImpl<T> reader = new ReaderImpl<T>(PulsarClientImpl.this, conf, listenerThread, consumerSubscribedFuture, schema);
			lock (consumers)
			{
				consumers.put(reader.Consumer, true);
			}
			consumerSubscribedFuture.thenRun(() =>
			{
				readerFuture.complete(reader);
			}).exceptionally(ex =>
			{
				log.warn("[{}] Failed to get create topic reader", topic, ex);
				readerFuture.completeExceptionally(ex);
				return null;
			});
			}).exceptionally(ex =>
			{
			log.warn("[{}] Failed to get partitioned topic metadata", topic, ex);
			readerFuture.completeExceptionally(ex);
			return null;
		});

			return readerFuture;
		}

		/// <summary>
		/// Read the schema information for a given topic.
		/// 
		/// If the topic does not exist or it has no schema associated, it will return an empty response
		/// </summary>
		public virtual CompletableFuture<Optional<SchemaInfo>> getSchema(string topic)
		{
			TopicName topicName;
			try
			{
				topicName = TopicName.get(topic);
			}
			catch (Exception)
			{
				return FutureUtil.failedFuture(new PulsarClientException.InvalidTopicNameException("Invalid topic name: " + topic));
			}

			return lookup.getSchema(topicName);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void close() throws org.apache.pulsar.client.api.PulsarClientException
		public override void close()
		{
			try
			{
				closeAsync().get();
			}
			catch (Exception e)
			{
				throw PulsarClientException.unwrap(e);
			}
		}

		public override CompletableFuture<Void> closeAsync()
		{
			log.info("Client closing. URL: {}", lookup.ServiceUrl);
			if (!state.compareAndSet(State.Open, State.Closing))
			{
				return FutureUtil.failedFuture(new PulsarClientException.AlreadyClosedException("Client already closed"));
			}

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.CompletableFuture<Void> closeFuture = new java.util.concurrent.CompletableFuture<>();
			CompletableFuture<Void> closeFuture = new CompletableFuture<Void>();
			IList<CompletableFuture<Void>> futures = Lists.newArrayList();

			lock (producers)
			{
				// Copy to a new list, because the closing will trigger a removal from the map
				// and invalidate the iterator
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.List<ProducerBase<?>> producersToClose = com.google.common.collect.Lists.newArrayList(producers.keySet());
				IList<ProducerBase<object>> producersToClose = Lists.newArrayList(producers.keySet());
				producersToClose.ForEach(p => futures.Add(p.closeAsync()));
			}

			lock (consumers)
			{
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.List<ConsumerBase<?>> consumersToClose = com.google.common.collect.Lists.newArrayList(consumers.keySet());
				IList<ConsumerBase<object>> consumersToClose = Lists.newArrayList(consumers.keySet());
				consumersToClose.ForEach(c => futures.Add(c.closeAsync()));
			}

			// Need to run the shutdown sequence in a separate thread to prevent deadlocks
			// If there are consumers or producers that need to be shutdown we cannot use the same thread
			// to shutdown the EventLoopGroup as well as that would be trying to shutdown itself thus a deadlock
			// would happen
			FutureUtil.waitForAll(futures).thenRun(() => (new Thread(() =>
			{
			try
			{
				shutdown();
				closeFuture.complete(null);
				state.set(State.Closed);
			}
			catch (PulsarClientException e)
			{
				closeFuture.completeExceptionally(e);
			}
			}, "pulsar-client-shutdown-thread")).Start()).exceptionally(exception =>
			{
			closeFuture.completeExceptionally(exception);
			return null;
		});

			return closeFuture;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void shutdown() throws org.apache.pulsar.client.api.PulsarClientException
		public override void shutdown()
		{
			try
			{
				lookup.close();
				cnxPool.Dispose();
				timer_Conflict.stop();
				externalExecutorProvider_Conflict.shutdownNow();
				conf.Authentication.close();
			}
			catch (Exception t)
			{
				log.warn("Failed to shutdown Pulsar client", t);
				throw PulsarClientException.unwrap(t);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public synchronized void updateServiceUrl(String serviceUrl) throws org.apache.pulsar.client.api.PulsarClientException
		public override void updateServiceUrl(string serviceUrl)
		{
			lock (this)
			{
				log.info("Updating service URL to {}", serviceUrl);
        
				conf.ServiceUrl = serviceUrl;
				lookup.updateServiceUrl(serviceUrl);
				cnxPool.closeAllConnections();
			}
		}

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: protected java.util.concurrent.CompletableFuture<ClientCnx> getConnection(final String topic)
		protected internal virtual CompletableFuture<ClientCnx> getConnection(string topic)
		{
			TopicName topicName = TopicName.get(topic);
			return lookup.getBroker(topicName).thenCompose(pair => cnxPool.getConnection(pair.Left, pair.Right));
		}

		/// <summary>
		/// visible for pulsar-functions * </summary>
		public virtual Timer timer()
		{
			return timer_Conflict;
		}

		internal virtual ExecutorProvider externalExecutorProvider()
		{
			return externalExecutorProvider_Conflict;
		}

		internal virtual long newProducerId()
		{
			return producerIdGenerator.AndIncrement;
		}

		internal virtual long newConsumerId()
		{
			return consumerIdGenerator.AndIncrement;
		}

		public virtual long newRequestId()
		{
			return requestIdGenerator.AndIncrement;
		}

		public virtual ConnectionPool CnxPool
		{
			get
			{
				return cnxPool;
			}
		}

		public virtual EventLoopGroup eventLoopGroup()
		{
			return eventLoopGroup_Conflict;
		}

		public virtual LookupService Lookup
		{
			get
			{
				return lookup;
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void reloadLookUp() throws org.apache.pulsar.client.api.PulsarClientException
		public virtual void reloadLookUp()
		{
			if (conf.ServiceUrl.StartsWith("http"))
			{
				lookup = new HttpLookupService(conf, eventLoopGroup_Conflict);
			}
			else
			{
				lookup = new BinaryProtoLookupService(this, conf.ServiceUrl, conf.UseTls, externalExecutorProvider_Conflict.Executor);
			}
		}

		public virtual CompletableFuture<int> getNumberOfPartitions(string topic)
		{
			return getPartitionedTopicMetadata(topic).thenApply(metadata => metadata.partitions);
		}

		public virtual CompletableFuture<PartitionedTopicMetadata> getPartitionedTopicMetadata(string topic)
		{

			CompletableFuture<PartitionedTopicMetadata> metadataFuture = new CompletableFuture<PartitionedTopicMetadata>();

			try
			{
				TopicName topicName = TopicName.get(topic);
				AtomicLong opTimeoutMs = new AtomicLong(conf.OperationTimeoutMs);
				Backoff backoff = (new BackoffBuilder()).setInitialTime(100, TimeUnit.MILLISECONDS).setMandatoryStop(opTimeoutMs.get() * 2, TimeUnit.MILLISECONDS).setMax(0, TimeUnit.MILLISECONDS).create();
				getPartitionedTopicMetadata(topicName, backoff, opTimeoutMs, metadataFuture);
			}
			catch (System.ArgumentException e)
			{
				return FutureUtil.failedFuture(new PulsarClientException.InvalidConfigurationException(e.Message));
			}
			return metadataFuture;
		}

		private void getPartitionedTopicMetadata(TopicName topicName, Backoff backoff, AtomicLong remainingTime, CompletableFuture<PartitionedTopicMetadata> future)
		{
			lookup.getPartitionedTopicMetadata(topicName).thenAccept(future.complete).exceptionally(e =>
			{
			long nextDelay = Math.Min(backoff.next(), remainingTime.get());
			bool isLookupThrottling = e.Cause is PulsarClientException.TooManyRequestsException;
			if (nextDelay <= 0 || isLookupThrottling)
			{
				future.completeExceptionally(e);
				return null;
			}
			((ScheduledExecutorService) externalExecutorProvider_Conflict.Executor).schedule(() =>
			{
				log.warn("[topic: {}] Could not get connection while getPartitionedTopicMetadata -- Will try again in {} ms", topicName, nextDelay);
				remainingTime.addAndGet(-nextDelay);
				getPartitionedTopicMetadata(topicName, backoff, remainingTime, future);
			}, nextDelay, TimeUnit.MILLISECONDS);
			return null;
			});
		}

		public override CompletableFuture<IList<string>> getPartitionsForTopic(string topic)
		{
			return getPartitionedTopicMetadata(topic).thenApply(metadata =>
			{
			if (metadata.partitions > 0)
			{
				TopicName topicName = TopicName.get(topic);
				IList<string> partitions = new List<string>(metadata.partitions);
				for (int i = 0; i < metadata.partitions; i++)
				{
					partitions.add(topicName.getPartition(i).ToString());
				}
				return partitions;
			}
			else
			{
				return Collections.singletonList(topic);
			}
			});
		}

		private static EventLoopGroup getEventLoopGroup(ClientConfigurationData conf)
		{
			ThreadFactory threadFactory = getThreadFactory("pulsar-client-io");
			return EventLoopUtil.newEventLoopGroup(conf.NumIoThreads, threadFactory);
		}

		private static ThreadFactory getThreadFactory(string poolName)
		{
			return new DefaultThreadFactory(poolName, Thread.CurrentThread.Daemon);
		}

		internal virtual void cleanupProducer<T1>(ProducerBase<T1> producer)
		{
			lock (producers)
			{
				producers.remove(producer);
			}
		}

		internal virtual void cleanupConsumer<T1>(ConsumerBase<T1> consumer)
		{
			lock (consumers)
			{
				consumers.remove(consumer);
			}
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @VisibleForTesting int producersCount()
		internal virtual int producersCount()
		{
			lock (producers)
			{
				return producers.size();
			}
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @VisibleForTesting int consumersCount()
		internal virtual int consumersCount()
		{
			lock (consumers)
			{
				return consumers.size();
			}
		}

		private static Mode convertRegexSubscriptionMode(RegexSubscriptionMode regexSubscriptionMode)
		{
			switch (regexSubscriptionMode)
			{
			case PersistentOnly:
				return Mode.PERSISTENT;
			case NonPersistentOnly:
				return Mode.NON_PERSISTENT;
			case AllTopics:
				return Mode.ALL;
			default:
				return null;
			}
		}

		private SchemaInfoProvider newSchemaProvider(string topicName)
		{
			return new MultiVersionSchemaInfoProvider(TopicName.get(topicName), this);
		}

		private LoadingCache<string, SchemaInfoProvider> SchemaProviderLoadingCache
		{
			get
			{
				return schemaProviderLoadingCache;
			}
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") protected java.util.concurrent.CompletableFuture<Void> preProcessSchemaBeforeSubscribe(PulsarClientImpl pulsarClientImpl, org.apache.pulsar.client.api.Schema schema, String topicName)
		protected internal virtual CompletableFuture<Void> preProcessSchemaBeforeSubscribe(PulsarClientImpl pulsarClientImpl, Schema schema, string topicName)
		{
			if (schema != null && schema.supportSchemaVersioning())
			{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.apache.pulsar.client.api.schema.SchemaInfoProvider schemaInfoProvider;
				SchemaInfoProvider schemaInfoProvider;
				try
				{
					schemaInfoProvider = pulsarClientImpl.SchemaProviderLoadingCache.get(topicName);
				}
				catch (ExecutionException e)
				{
					log.error("Failed to load schema info provider for topic {}", topicName, e);
					return FutureUtil.failedFuture(e.InnerException);
				}

				if (schema.requireFetchingSchemaInfo())
				{
					return schemaInfoProvider.LatestSchema.thenCompose(schemaInfo =>
					{
					if (null == schemaInfo)
					{
						if (!(schema is AutoConsumeSchema))
						{
							return FutureUtil.failedFuture(new PulsarClientException.NotFoundException("No latest schema found for topic " + topicName));
						}
					}
					try
					{
						log.info("Configuring schema for topic {} : {}", topicName, schemaInfo);
						schema.configureSchemaInfo(topicName, "topic", schemaInfo);
					}
					catch (Exception re)
					{
						return FutureUtil.failedFuture(re);
					}
					schema.SchemaInfoProvider = schemaInfoProvider;
					return CompletableFuture.completedFuture(null);
					});
				}
				else
				{
					schema.SchemaInfoProvider = schemaInfoProvider;
				}
			}
			return CompletableFuture.completedFuture(null);
		}

		//
		// Transaction related API
		//

		// This method should be exposed in the PulsarClient interface. Only expose it when all the transaction features
		// are completed.
		// @Override
		public virtual TransactionBuilder newTransaction()
		{
			return new TransactionBuilderImpl(this);
		}

	}

}