﻿using System;
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
    using SharpPulsar.Interface;
    using SharpPulsar.Util;
    using SharpPulsar.Impl.Producer;
    using SharpPulsar.Interface.Producer;
    using SharpPulsar.Interface.Schema;
    using SharpPulsar.Interface.Consumer;
    using SharpPulsar.Interface.Reader;
    using System.Threading.Tasks;
    using SharpPulsar.Configuration;
    using Optional;
    using SharpPulsar.Common.Schema;
    using SharpPulsar.Exception;
    using SharpPulsar.Impl.Schema;
    using SharpPulsar.Util.Atomic;
    using Microsoft.Extensions.Logging;

    public class PulsarClientImpl : IPulsarClient
	{

		//private static readonly Util.Log  log = LoggerFactory.Create(typeof(PulsarClientImpl));

		private readonly ClientConfigurationData conf;
		private LookupService lookup;
		private readonly ConnectionPool cnxPool;
		private readonly Timer timer_Conflict;
		private readonly ExecutorProvider externalExecutorProvider_Conflict;

		internal enum State
		{
			Open,
			Closing,
			Closed
		}

		private readonly HashSet<ProducerBase<object>> _producers;
		private readonly HashSet<ConsumerBase<object>> _consumers;
		

		
		private readonly DateTime clientClock;
		public PulsarClientImpl(ClientConfigurationData conf) : this(conf, getEventLoopGroup(conf))
		{
		}

		public PulsarClientImpl(ClientConfigurationData conf, EventLoopGroup eventLoopGroup) : this(conf, eventLoopGroup, new ConnectionPool(conf, eventLoopGroup))
		{
		}

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
//ORIGINAL LINE: private void setAuth(SharpPulsar.Impl.conf.ClientConfigurationData conf) throws org.apache.pulsar.client.api.PulsarClientException
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

		public virtual Clock ClientClock
		{
			get
			{
				return clientClock;
			}
		}

		public IProducerBuilder<sbyte[]> NewProducer()
		{
			return new ProducerBuilderImpl<sbyte[]>(this, Schema.BYTES);
		}

		public IProducerBuilder<T> NewProducer<T>(ISchema<T> schema)
		{
			return new ProducerBuilderImpl<T>(this, schema);
		}

		public IConsumerBuilder<sbyte[]> NewConsumer()
		{
			return new ConsumerBuilderImpl<sbyte[]>(this, Schema.BYTES);
		}

		public IConsumerBuilder<T> NewConsumer<T>(ISchema<T> schema)
		{
			return new ConsumerBuilderImpl<T>(this, schema);
		}

		public IReaderBuilder<sbyte[]> NewReader()
		{
			return new ReaderBuilderImpl<sbyte[]>(this, Schema.BYTES);
		}

		public IReaderBuilder<T> NewReader<T>(ISchema<T> schema)
		{
			return new ReaderBuilderImpl<T>(this, schema);
		}

		public virtual ValueTask<IProducer<sbyte[]>> CreateProducerAsync(ProducerConfigurationData conf)
		{
			return CreateProducerAsync(conf, Schema.BYTES, null);
		}

		public virtual ValueTask<IProducer<T>> CreateProducerAsync<T>(ProducerConfigurationData conf, Schema<T> schema)
		{
			return createProducerAsync(conf, schema, null);
		}

		public virtual ValueTask<IProducer<T>> CreateProducerAsync<T>(ProducerConfigurationData conf, Schema<T> schema, ProducerInterceptors interceptors)
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

		private ValueTask<IProducer<T>> CreateProducerAsync<T>(string topic, ProducerConfigurationData conf, Schema<T> schema, ProducerInterceptors interceptors)
		{
			var producerCreatedFuture = new CompletableFuture<Producer<T>>();

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

		public virtual async ValueTask SubscribeAsync(ConsumerConfigurationData<sbyte[]> conf)
		{
			return await SubscribeAsync(conf, new BytesSchema(), interceptors: null);
		}

		public async virtual ValueTask<IConsumer<T>> SubscribeAsync<T>(ConsumerConfigurationData<T> conf, ISchema<T> schema, ConsumerInterceptors<T> interceptors)
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
				return SingleTopicSubscribeAsync(conf, schema, interceptors);
			}
			else
			{
				return MultiTopicSubscribeAsync(conf, schema, interceptors);
			}
		}

		private ValueTask<IConsumer<T>> SingleTopicSubscribeAsync<T>(ConsumerConfigurationData<T> conf, Schema<T> schema, ConsumerInterceptors<T> interceptors)
		{
			return preProcessSchemaBeforeSubscribe(this, schema, conf.SingleTopic).thenCompose(ignored => doSingleTopicSubscribeAsync(conf, schema, interceptors));
		}

		private ValueTask<IConsumer<T>> DoSingleTopicSubscribeAsync<T>(ConsumerConfigurationData<T> conf, Schema<T> schema, ConsumerInterceptors<T> interceptors)
		{
			var consumerSubscribedFuture = new CompletableFuture<Consumer<T>>();

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

		private ValueTask<IConsumer<T>> MultiTopicSubscribeAsync<T>(ConsumerConfigurationData<T> conf, Schema<T> schema, ConsumerInterceptors<T> interceptors)
		{
			var consumerSubscribedFuture = new CompletableFuture<Consumer<T>>();

			ConsumerBase<T> consumer = new MultiTopicsConsumerImpl<T>(PulsarClientImpl.this, conf, externalExecutorProvider_Conflict.Executor, consumerSubscribedFuture, schema, interceptors, true);

			lock (consumers)
			{
				consumers.put(consumer, true);
			}

			return consumerSubscribedFuture;
		}

		public virtual ValueTask<IConsumer<sbyte[]>> PatternTopicSubscribeAsync(ConsumerConfigurationData<sbyte[]> conf)
		{
			return PatternTopicSubscribeAsync(conf, Schema.BYTES, null);
		}

		private ValueTask<IConsumer<T>> PatternTopicSubscribeAsync<T>(ConsumerConfigurationData<T> conf, Schema<T> schema, ConsumerInterceptors<T> interceptors)
		{
			string regex = conf.TopicsPattern.pattern();
			Mode subscriptionMode = convertRegexSubscriptionMode(conf.RegexSubscriptionMode);
			TopicName destination = TopicName.get(regex);
			NamespaceName namespaceName = destination.NamespaceObject;

			var consumerSubscribedFuture = new CompletableFuture<Consumer<T>>();
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
		public static IList<string> TopicsPatternFilter(IList<string> original, Pattern topicsPattern)
		{
			Pattern shortenedTopicsPattern = topicsPattern.ToString().Contains("://") ? Pattern.compile(topicsPattern.ToString().Split("\\:\\/\\/", true)[1]) : topicsPattern;

			return original.Select(TopicName.get).Select(TopicName.toString).Where(topic => shortenedTopicsPattern.matcher(topic.Split("\\:\\/\\/")[1]).matches()).ToList();
		}

		public virtual ValueTask<IReader<sbyte[]>> CreateReaderAsync(ReaderConfigurationData<sbyte[]> conf)
		{
			return CreateReaderAsync(conf, Schema.BYTES);
		}

		public virtual ValueTask<IReader<T>> CreateReaderAsync<T>(ReaderConfigurationData<T> conf, Schema<T> schema)
		{
			return PreProcessSchemaBeforeSubscribe(this, schema, conf.TopicName).thenCompose(ignored => doCreateReaderAsync(conf, schema));
		}

		internal virtual ValueTask<IReader<T>> DoCreateReaderAsync<T>(ReaderConfigurationData<T> conf, Schema<T> schema)
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
		public virtual ValueTask<Option<SchemaInfo>> GetSchema(string topic)
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

		public void Close()
		{
			try
			{
				CloseAsync().get();
			}
			catch (System.Exception e)
			{
				throw PulsarClientException.Unwrap(e);
			}
		}

		public override ValueTask CloseAsync()
		{
			log.info("Client closing. URL: {}", lookup.ServiceUrl);
			if (!state.compareAndSet(State.Open, State.Closing))
			{
				return FutureUtil.failedFuture(new PulsarClientException.AlreadyClosedException("Client already closed"));
			}


			CompletableFuture<Void> closeFuture = new CompletableFuture<Void>();
			IList<CompletableFuture<Void>> futures = Lists.newArrayList();

			lock (producers)
			{
				// Copy to a new list, because the closing will trigger a removal from the map
				// and invalidate the iterator
/
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


		public void Shutdown()
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

		public void UpdateServiceUrl(string serviceUrl)
		{
			lock (this)
			{
				log.info("Updating service URL to {}", serviceUrl);
        
				conf.ServiceUrl = serviceUrl;
				lookup.updateServiceUrl(serviceUrl);
				cnxPool.closeAllConnections();
			}
		}

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

		internal virtual long NewProducerId()
		{
			return producerIdGenerator.Increment();
		}

		internal virtual long NewConsumerId()
		{
			return consumerIdGenerator.Increment();
		}

		public virtual long NewRequestId()
		{
			return requestIdGenerator.Increment();
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