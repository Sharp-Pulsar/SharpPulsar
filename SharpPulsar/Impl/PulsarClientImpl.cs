using DotNetty.Transport.Channels;
using DotNetty.Transport.Libuv;
using Microsoft.Extensions.Logging;
using SharpPulsar.Api;
using SharpPulsar.Impl.Conf;
using SharpPulsar.Util;
using SharpPulsar.Util.Atomic;
using System;
using System.Collections.Generic;
using System.Threading;
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

	public class PulsarClientImpl : IPulsarClient
	{
		private static readonly ILogger log = new LoggerFactory().CreateLogger<PulsarClientImpl>();

		public virtual Configuration Configuration;
		public virtual string Lookup;
		public virtual ConnectionPool CnxPool;
		private readonly Timer timer;
		private readonly ExecutorProvider externalExecutorProvider;

		public enum State
		{
			Open,
			Closing,
			Closed
		}

		private AtomicReference<State> state = new AtomicReference<State>();
		private readonly IdentityHashMap<ProducerBase<object>, bool> producers;
		private readonly IdentityHashMap<ConsumerBase<object>, bool> consumers;

		private readonly AtomicLong producerIdGenerator = new AtomicLong();
		private readonly AtomicLong consumerIdGenerator = new AtomicLong();
		private readonly AtomicLong requestIdGenerator = new AtomicLong();

		private readonly EventLoopGroup eventLoopGroup;

		private readonly LoadingCache<string, SchemaInfoProvider> schemaProviderLoadingCache = CacheBuilder.newBuilder().maximumSize(100000).expireAfterAccess(30, BAMCIS.Util.Concurrent.TimeUnit.MINUTES).build(new CacheLoaderAnonymousInnerClass());

		public class CacheLoaderAnonymousInnerClass : CacheLoader<string, SchemaInfoProvider>
		{

			public override SchemaInfoProvider load(string TopicName)
			{
				return outerInstance.newSchemaProvider(TopicName);
			}
		}

		public virtual DateTime ClientClock {get;}
		public PulsarClientImpl(ClientConfigurationData Conf) : this(Conf, GetEventLoopGroup(Conf))
		{
		}


		public PulsarClientImpl(ClientConfigurationData Conf, EventLoopGroup EventLoopGroup) : this(Conf, EventLoopGroup, new ConnectionPool(Conf, EventLoopGroup))
		{
		}

		public PulsarClientImpl(ClientConfigurationData Conf, EventLoopGroup EventLoopGroup, ConnectionPool CnxPool)
		{
			if (Conf == null || isBlank(Conf.ServiceUrl) || EventLoopGroup == null)
			{
				throw new PulsarClientException.InvalidConfigurationException("Invalid client configuration");
			}
			this.eventLoopGroup = EventLoopGroup;
			Auth = Conf;
			this.Configuration = Conf;
			this.ClientClock = Conf.Clock;
			Conf.Authentication.start();
			this.CnxPool = CnxPool;
			externalExecutorProvider = new ExecutorProvider(Conf.NumListenerThreads, GetThreadFactory("pulsar-external-listener"));
			if (Conf.ServiceUrl.StartsWith("http"))
			{
				Lookup = new HttpLookupService(Conf, EventLoopGroup);
			}
			else
			{
				Lookup = new BinaryProtoLookupService(this, Conf.ServiceUrl, Conf.UseTls, externalExecutorProvider.Executor);
			}
			timer = new HashedWheelTimer(GetThreadFactory("pulsar-timer"), 1, BAMCIS.Util.Concurrent.TimeUnit.MILLISECONDS);
			producers = Maps.newIdentityHashMap();
			consumers = Maps.newIdentityHashMap();
			state.set(State.Open);
		}

		private ClientConfigurationData Auth
		{
			set
			{
				if (StringUtils.isBlank(value.AuthPluginClassName) || StringUtils.isBlank(value.AuthParams))
				{
					return;
				}
    
				value.Authentication = AuthenticationFactory.create(value.AuthPluginClassName, value.AuthParams);
			}
		}



		public override IProducerBuilder<sbyte[]> NewProducer()
		{
			return new ProducerBuilderImpl<sbyte[]>(this, SchemaFields.BYTES);
		}

		public override IProducerBuilder<T> NewProducer<T>(Schema<T> Schema)
		{
			return new ProducerBuilderImpl<T>(this, Schema);
		}

		public override ConsumerBuilder<sbyte[]> NewConsumer()
		{
			return new ConsumerBuilderImpl<sbyte[]>(this, SchemaFields.BYTES);
		}

		public override ConsumerBuilder<T> NewConsumer<T>(Schema<T> Schema)
		{
			return new ConsumerBuilderImpl<T>(this, Schema);
		}

		public override ReaderBuilder<sbyte[]> NewReader()
		{
			return new ReaderBuilderImpl<sbyte[]>(this, SchemaFields.BYTES);
		}

		public override ReaderBuilder<T> NewReader<T>(Schema<T> Schema)
		{
			return new ReaderBuilderImpl<T>(this, Schema);
		}

		public virtual ValueTask<IProducer<sbyte[]>> CreateProducerAsync(ProducerConfigurationData Conf)
		{
			return CreateProducerAsync(Conf, SchemaFields.BYTES, null);
		}

		public virtual ValueTask<IProducer<T>> CreateProducerAsync<T>(ProducerConfigurationData Conf, Schema<T> Schema)
		{
			return CreateProducerAsync(Conf, Schema, null);
		}

		public virtual ValueTask<IProducer<T>> CreateProducerAsync<T>(ProducerConfigurationData Conf, Schema<T> Schema, ProducerInterceptors Interceptors)
		{
			if (Conf == null)
			{
				return FutureUtil.failedFuture(new PulsarClientException.InvalidConfigurationException("Producer configuration undefined"));
			}

			if (Schema is AutoConsumeSchema)
			{
				return FutureUtil.failedFuture(new PulsarClientException.InvalidConfigurationException("AutoConsumeSchema is only used by consumers to detect schemas automatically"));
			}

			if (state.get() != State.Open)
			{
				return FutureUtil.failedFuture(new PulsarClientException.AlreadyClosedException("Client already closed : state = " + state.get()));
			}

			string Topic = Conf.TopicName;

			if (!TopicName.isValid(Topic))
			{
				return FutureUtil.failedFuture(new PulsarClientException.InvalidTopicNameException("Invalid topic name: '" + Topic + "'"));
			}

			if (Schema is AutoProduceBytesSchema)
			{
				AutoProduceBytesSchema AutoProduceBytesSchema = (AutoProduceBytesSchema) Schema;
				if (AutoProduceBytesSchema.schemaInitialized())
				{
					return CreateProducerAsync(Topic, Conf, Schema, Interceptors);
				}
				return Lookup.GetSchema(TopicName.get(Conf.TopicName)).thenCompose(schemaInfoOptional =>
				{
				if (schemaInfoOptional.Present)
				{
					AutoProduceBytesSchema.Schema = Schema.getSchema(schemaInfoOptional.get());
				}
				else
				{
					AutoProduceBytesSchema.Schema = Schema.BYTES;
				}
				return CreateProducerAsync(Topic, Conf, Schema, Interceptors);
				});
			}
			else
			{
				return CreateProducerAsync(Topic, Conf, Schema, Interceptors);
			}

		}

		private ValueTask<Producer<T>> CreateProducerAsync<T>(string Topic, ProducerConfigurationData Conf, Schema<T> Schema, ProducerInterceptors Interceptors)
		{
			ValueTask<Producer<T>> ProducerCreatedFuture = new ValueTask<Producer<T>>();

			GetPartitionedTopicMetadata(Topic).thenAccept(metadata =>
			{
			if (log.DebugEnabled)
			{
				log.debug("[{}] Received topic metadata. partitions: {}", Topic, metadata.partitions);
			}
			ProducerBase<T> Producer;
			if (metadata.partitions > 0)
			{
				Producer = new PartitionedProducerImpl<T>(PulsarClientImpl.this, Topic, Conf, metadata.partitions, ProducerCreatedFuture, Schema, Interceptors);
			}
			else
			{
				Producer = new ProducerImpl<T>(PulsarClientImpl.this, Topic, Conf, ProducerCreatedFuture, -1, Schema, Interceptors);
			}
			lock (producers)
			{
				producers.put(Producer, true);
			}
			}).exceptionally(ex =>
			{
			log.warn("[{}] Failed to get partitioned topic metadata: {}", Topic, ex.Message);
			ProducerCreatedFuture.completeExceptionally(ex);
			return null;
		});

			return ProducerCreatedFuture;
		}

		public virtual ValueTask<Consumer<sbyte[]>> SubscribeAsync(ConsumerConfigurationData<sbyte[]> Conf)
		{
			return SubscribeAsync(Conf, SchemaFields.BYTES, null);
		}

		public virtual ValueTask<Consumer<T>> SubscribeAsync<T>(ConsumerConfigurationData<T> Conf, Schema<T> Schema, ConsumerInterceptors<T> Interceptors)
		{
			if (state.get() != State.Open)
			{
				return FutureUtil.failedFuture(new PulsarClientException.AlreadyClosedException("Client already closed"));
			}

			if (Conf == null)
			{
				return FutureUtil.failedFuture(new PulsarClientException.InvalidConfigurationException("Consumer configuration undefined"));
			}

			if (!Conf.TopicNames.All(TopicName.isValid))
			{
				return FutureUtil.failedFuture(new PulsarClientException.InvalidTopicNameException("Invalid topic name"));
			}

			if (isBlank(Conf.SubscriptionName))
			{
				return FutureUtil.failedFuture(new PulsarClientException.InvalidConfigurationException("Empty subscription name"));
			}

			if (Conf.ReadCompacted && (!Conf.TopicNames.All(topic => TopicName.get(topic).Domain == TopicDomain.persistent) || (Conf.SubscriptionType != SubscriptionType.Exclusive && Conf.SubscriptionType != SubscriptionType.Failover)))
			{
				return FutureUtil.failedFuture(new PulsarClientException.InvalidConfigurationException("Read compacted can only be used with exclusive of failover persistent subscriptions"));
			}

			if (Conf.ConsumerEventListener != null && Conf.SubscriptionType != SubscriptionType.Failover)
			{
				return FutureUtil.failedFuture(new PulsarClientException.InvalidConfigurationException("Active consumer listener is only supported for failover subscription"));
			}

			if (Conf.TopicsPattern != null)
			{
				// If use topicsPattern, we should not use topic(), and topics() method.
				if (!Conf.TopicNames.Empty)
				{
					return FutureUtil.failedFuture(new System.ArgumentException("Topic names list must be null when use topicsPattern"));
				}
				return PatternTopicSubscribeAsync(Conf, Schema, Interceptors);
			}
			else if (Conf.TopicNames.size() == 1)
			{
				return SingleTopicSubscribeAsync(Conf, Schema, Interceptors);
			}
			else
			{
				return MultiTopicSubscribeAsync(Conf, Schema, Interceptors);
			}
		}

		private ValueTask<Consumer<T>> SingleTopicSubscribeAsync<T>(ConsumerConfigurationData<T> Conf, Schema<T> Schema, ConsumerInterceptors<T> Interceptors)
		{
			return PreProcessSchemaBeforeSubscribe(this, Schema, Conf.SingleTopic).thenCompose(ignored => DoSingleTopicSubscribeAsync(Conf, Schema, Interceptors));
		}

		private ValueTask<Consumer<T>> DoSingleTopicSubscribeAsync<T>(ConsumerConfigurationData<T> Conf, Schema<T> Schema, ConsumerInterceptors<T> Interceptors)
		{
			ValueTask<Consumer<T>> ConsumerSubscribedFuture = new ValueTask<Consumer<T>>();

			string Topic = Conf.SingleTopic;

			GetPartitionedTopicMetadata(Topic).thenAccept(metadata =>
			{
			if (log.DebugEnabled)
			{
				log.debug("[{}] Received topic metadata. partitions: {}", Topic, metadata.partitions);
			}
			ConsumerBase<T> Consumer;
			ExecutorService ListenerThread = externalExecutorProvider.Executor;
			if (metadata.partitions > 0)
			{
				Consumer = MultiTopicsConsumerImpl.CreatePartitionedConsumer(PulsarClientImpl.this, Conf, ListenerThread, ConsumerSubscribedFuture, metadata.partitions, Schema, Interceptors);
			}
			else
			{
				int PartitionIndex = TopicName.getPartitionIndex(Topic);
				Consumer = ConsumerImpl.NewConsumerImpl(PulsarClientImpl.this, Topic, Conf, ListenerThread, PartitionIndex, false, ConsumerSubscribedFuture, SubscriptionMode.Durable, null, Schema, Interceptors, true);
			}
			lock (consumers)
			{
				consumers.put(Consumer, true);
			}
			}).exceptionally(ex =>
			{
			log.warn("[{}] Failed to get partitioned topic metadata", Topic, ex);
			ConsumerSubscribedFuture.completeExceptionally(ex);
			return null;
		});

			return ConsumerSubscribedFuture;
		}

		private ValueTask<Consumer<T>> MultiTopicSubscribeAsync<T>(ConsumerConfigurationData<T> Conf, Schema<T> Schema, ConsumerInterceptors<T> Interceptors)
		{
			ValueTask<Consumer<T>> ConsumerSubscribedFuture = new ValueTask<Consumer<T>>();

			ConsumerBase<T> Consumer = new MultiTopicsConsumerImpl<T>(PulsarClientImpl.this, Conf, externalExecutorProvider.Executor, ConsumerSubscribedFuture, Schema, Interceptors, true);

			lock (consumers)
			{
				consumers.put(Consumer, true);
			}

			return ConsumerSubscribedFuture;
		}

		public virtual ValueTask<Consumer<sbyte[]>> PatternTopicSubscribeAsync(ConsumerConfigurationData<sbyte[]> Conf)
		{
			return PatternTopicSubscribeAsync(Conf, SchemaFields.BYTES, null);
		}

		private ValueTask<Consumer<T>> PatternTopicSubscribeAsync<T>(ConsumerConfigurationData<T> Conf, Schema<T> Schema, ConsumerInterceptors<T> Interceptors)
		{
			string Regex = Conf.TopicsPattern.pattern();
			Mode SubscriptionMode = ConvertRegexSubscriptionMode(Conf.RegexSubscriptionMode);
			TopicName Destination = TopicName.get(Regex);
			NamespaceName NamespaceName = Destination.NamespaceObject;

			ValueTask<Consumer<T>> ConsumerSubscribedFuture = new ValueTask<Consumer<T>>();
			Lookup.GetTopicsUnderNamespace(NamespaceName, SubscriptionMode).thenAccept(topics =>
			{
			if (log.DebugEnabled)
			{
				log.debug("Get topics under namespace {}, topics.size: {}", NamespaceName.ToString(), topics.size());
				topics.forEach(topicName => log.debug("Get topics under namespace {}, topic: {}", NamespaceName.ToString(), topicName));
			}
			IList<string> TopicsList = TopicsPatternFilter(topics, Conf.TopicsPattern);
			Conf.TopicNames.addAll(TopicsList);
			ConsumerBase<T> Consumer = new PatternMultiTopicsConsumerImpl<T>(Conf.TopicsPattern, PulsarClientImpl.this, Conf, externalExecutorProvider.Executor, ConsumerSubscribedFuture, Schema, SubscriptionMode, Interceptors);
			lock (consumers)
			{
				consumers.put(Consumer, true);
			}
			}).exceptionally(ex =>
			{
			log.warn("[{}] Failed to get topics under namespace", NamespaceName);
			ConsumerSubscribedFuture.completeExceptionally(ex);
			return null;
		});

			return ConsumerSubscribedFuture;
		}

		// get topics that match 'topicsPattern' from original topics list
		// return result should contain only topic names, without partition part
		public static IList<string> TopicsPatternFilter(IList<string> Original, Pattern TopicsPattern)
		{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.regex.Pattern shortenedTopicsPattern = topicsPattern.toString().contains("://") ? java.util.regex.Pattern.compile(topicsPattern.toString().split("\\:\\/\\/")[1]) : topicsPattern;
			Pattern ShortenedTopicsPattern = TopicsPattern.ToString().Contains("://") ? Pattern.compile(TopicsPattern.ToString().Split(@"\:\/\/", true)[1]) : TopicsPattern;

//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			return Original.Select(TopicName.get).Select(TopicName::toString).Where(topic => ShortenedTopicsPattern.matcher(topic.Split(@"\:\/\/")[1]).matches()).ToList();
		}

		public virtual ValueTask<Reader<sbyte[]>> CreateReaderAsync(ReaderConfigurationData<sbyte[]> Conf)
		{
			return CreateReaderAsync(Conf, SchemaFields.BYTES);
		}

		public virtual ValueTask<Reader<T>> CreateReaderAsync<T>(ReaderConfigurationData<T> Conf, Schema<T> Schema)
		{
			return PreProcessSchemaBeforeSubscribe(this, Schema, Conf.TopicName).thenCompose(ignored => DoCreateReaderAsync(Conf, Schema));
		}

		public virtual ValueTask<Reader<T>> DoCreateReaderAsync<T>(ReaderConfigurationData<T> Conf, Schema<T> Schema)
		{
			if (state.get() != State.Open)
			{
				return FutureUtil.failedFuture(new PulsarClientException.AlreadyClosedException("Client already closed"));
			}

			if (Conf == null)
			{
				return FutureUtil.failedFuture(new PulsarClientException.InvalidConfigurationException("Consumer configuration undefined"));
			}

			string Topic = Conf.TopicName;

			if (!TopicName.isValid(Topic))
			{
				return FutureUtil.failedFuture(new PulsarClientException.InvalidTopicNameException("Invalid topic name"));
			}

			if (Conf.StartMessageId == null)
			{
				return FutureUtil.failedFuture(new PulsarClientException.InvalidConfigurationException("Invalid startMessageId"));
			}

			ValueTask<Reader<T>> ReaderFuture = new ValueTask<Reader<T>>();

			GetPartitionedTopicMetadata(Topic).thenAccept(metadata =>
			{
			if (log.DebugEnabled)
			{
				log.debug("[{}] Received topic metadata. partitions: {}", Topic, metadata.partitions);
			}
			if (metadata.partitions > 0)
			{
				ReaderFuture.completeExceptionally(new PulsarClientException("Topic reader cannot be created on a partitioned topic"));
				return;
			}
			ValueTask<Consumer<T>> ConsumerSubscribedFuture = new ValueTask<Consumer<T>>();
			ExecutorService ListenerThread = externalExecutorProvider.Executor;
			ReaderImpl<T> Reader = new ReaderImpl<T>(PulsarClientImpl.this, Conf, ListenerThread, ConsumerSubscribedFuture, Schema);
			lock (consumers)
			{
				consumers.put(Reader.Consumer, true);
			}
			ConsumerSubscribedFuture.thenRun(() =>
			{
				ReaderFuture.complete(Reader);
			}).exceptionally(ex =>
			{
				log.warn("[{}] Failed to get create topic reader", Topic, ex);
				ReaderFuture.completeExceptionally(ex);
				return null;
			});
			}).exceptionally(ex =>
			{
			log.warn("[{}] Failed to get partitioned topic metadata", Topic, ex);
			ReaderFuture.completeExceptionally(ex);
			return null;
		});

			return ReaderFuture;
		}

		/// <summary>
		/// Read the schema information for a given topic.
		/// 
		/// If the topic does not exist or it has no schema associated, it will return an empty response
		/// </summary>
		public virtual ValueTask<Optional<SchemaInfo>> GetSchema(string Topic)
		{
			TopicName TopicName;
			try
			{
				TopicName = TopicName.get(Topic);
			}
			catch (Exception)
			{
				return FutureUtil.failedFuture(new PulsarClientException.InvalidTopicNameException("Invalid topic name: " + Topic));
			}

			return Lookup.GetSchema(TopicName);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void close() throws SharpPulsar.api.PulsarClientException
		public override void Close()
		{
			try
			{
				CloseAsync().get();
			}
			catch (Exception E)
			{
				throw PulsarClientException.unwrap(E);
			}
		}

		public override ValueTask<Void> CloseAsync()
		{
			log.info("Client closing. URL: {}", Lookup.ServiceUrl);
			if (!state.compareAndSet(State.Open, State.Closing))
			{
				return FutureUtil.failedFuture(new PulsarClientException.AlreadyClosedException("Client already closed"));
			}

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.ValueTask<Void> closeFuture = new java.util.concurrent.ValueTask<>();
			ValueTask<Void> CloseFuture = new ValueTask<Void>();
			IList<ValueTask<Void>> Futures = Lists.newArrayList();

			lock (producers)
			{
				// Copy to a new list, because the closing will trigger a removal from the map
				// and invalidate the iterator
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.List<ProducerBase<?>> producersToClose = com.google.common.collect.Lists.newArrayList(producers.keySet());
				IList<ProducerBase<object>> ProducersToClose = Lists.newArrayList(producers.keySet());
				ProducersToClose.ForEach(p => Futures.Add(p.closeAsync()));
			}

			lock (consumers)
			{
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.List<ConsumerBase<?>> consumersToClose = com.google.common.collect.Lists.newArrayList(consumers.keySet());
				IList<ConsumerBase<object>> ConsumersToClose = Lists.newArrayList(consumers.keySet());
				ConsumersToClose.ForEach(c => Futures.Add(c.closeAsync()));
			}

			// Need to run the shutdown sequence in a separate thread to prevent deadlocks
			// If there are consumers or producers that need to be shutdown we cannot use the same thread
			// to shutdown the EventLoopGroup as well as that would be trying to shutdown itself thus a deadlock
			// would happen
			FutureUtil.waitForAll(Futures).thenRun(() => (new Thread(() =>
			{
			try
			{
				Shutdown();
				CloseFuture.complete(null);
				state.set(State.Closed);
			}
			catch (PulsarClientException E)
			{
				CloseFuture.completeExceptionally(E);
			}
			}, "pulsar-client-shutdown-thread")).Start()).exceptionally(exception =>
			{
			CloseFuture.completeExceptionally(exception);
			return null;
		});

			return CloseFuture;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void shutdown() throws SharpPulsar.api.PulsarClientException
		public override void Shutdown()
		{
			try
			{
				Lookup.close();
				CnxPool.Dispose();
				timer.stop();
				externalExecutorProvider.ShutdownNow();
				Configuration.Authentication.Dispose();
			}
			catch (Exception T)
			{
				log.warn("Failed to shutdown Pulsar client", T);
				throw PulsarClientException.unwrap(T);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public synchronized void updateServiceUrl(String serviceUrl) throws SharpPulsar.api.PulsarClientException
		public override void UpdateServiceUrl(string ServiceUrl)
		{
			lock (this)
			{
				log.info("Updating service URL to {}", ServiceUrl);
        
				Configuration.ServiceUrl = ServiceUrl;
				Lookup.UpdateServiceUrl(ServiceUrl);
				CnxPool.CloseAllConnections();
			}
		}

		public virtual ValueTask<ClientCnx> GetConnection(in string Topic)
		{
			TopicName TopicName = TopicName.get(Topic);
			return Lookup.GetBroker(TopicName).thenCompose(pair => CnxPool.GetConnection(pair.Left, pair.Right));
		}

		/// <summary>
		/// visible for pulsar-functions * </summary>
		public virtual Timer Timer()
		{
			return timer;
		}

		public virtual ExecutorProvider ExternalExecutorProvider()
		{
			return externalExecutorProvider;
		}

		public virtual long NewProducerId()
		{
			return producerIdGenerator.AndIncrement;
		}

		public virtual long NewConsumerId()
		{
			return consumerIdGenerator.AndIncrement;
		}

		public virtual long NewRequestId()
		{
			return requestIdGenerator.AndIncrement;
		}


		public virtual EventLoopGroup EventLoopGroup()
		{
			return eventLoopGroup;
		}


//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void reloadLookUp() throws SharpPulsar.api.PulsarClientException
		public virtual void ReloadLookUp()
		{
			if (Configuration.ServiceUrl.StartsWith("http"))
			{
				Lookup = new HttpLookupService(Configuration, eventLoopGroup);
			}
			else
			{
				Lookup = new BinaryProtoLookupService(this, Configuration.ServiceUrl, Configuration.UseTls, externalExecutorProvider.Executor);
			}
		}

		public virtual ValueTask<int> GetNumberOfPartitions(string Topic)
		{
			return GetPartitionedTopicMetadata(Topic).thenApply(metadata => metadata.partitions);
		}

		public virtual ValueTask<PartitionedTopicMetadata> GetPartitionedTopicMetadata(string Topic)
		{

			ValueTask<PartitionedTopicMetadata> MetadataFuture = new ValueTask<PartitionedTopicMetadata>();

			try
			{
				TopicName TopicName = TopicName.get(Topic);
				AtomicLong OpTimeoutMs = new AtomicLong(Configuration.OperationTimeoutMs);
				Backoff Backoff = (new BackoffBuilder()).SetInitialTime(100, BAMCIS.Util.Concurrent.TimeUnit.MILLISECONDS).setMandatoryStop(OpTimeoutMs.get() * 2, BAMCIS.Util.Concurrent.TimeUnit.MILLISECONDS).setMax(0, BAMCIS.Util.Concurrent.TimeUnit.MILLISECONDS).create();
				GetPartitionedTopicMetadata(TopicName, Backoff, OpTimeoutMs, MetadataFuture);
			}
			catch (System.ArgumentException E)
			{
				return FutureUtil.failedFuture(new PulsarClientException.InvalidConfigurationException(E.Message));
			}
			return MetadataFuture;
		}

		private void GetPartitionedTopicMetadata(TopicName TopicName, Backoff Backoff, AtomicLong RemainingTime, ValueTask<PartitionedTopicMetadata> Future)
		{
			Lookup.GetPartitionedTopicMetadata(TopicName).thenAccept(Future.complete).exceptionally(e =>
			{
			long NextDelay = Math.Min(Backoff.next(), RemainingTime.get());
			bool IsLookupThrottling = e.Cause is PulsarClientException.TooManyRequestsException;
			if (NextDelay <= 0 || IsLookupThrottling)
			{
				Future.completeExceptionally(e);
				return null;
			}
			((ScheduledExecutorService) externalExecutorProvider.Executor).schedule(() =>
			{
				log.warn("[topic: {}] Could not get connection while getPartitionedTopicMetadata -- Will try again in {} ms", TopicName, NextDelay);
				RemainingTime.addAndGet(-NextDelay);
				GetPartitionedTopicMetadata(TopicName, Backoff, RemainingTime, Future);
			}, NextDelay, BAMCIS.Util.Concurrent.TimeUnit.MILLISECONDS);
			return null;
			});
		}

		public override ValueTask<IList<string>> GetPartitionsForTopic(string Topic)
		{
			return GetPartitionedTopicMetadata(Topic).thenApply(metadata =>
			{
			if (metadata.partitions > 0)
			{
				TopicName TopicName = TopicName.get(Topic);
				IList<string> Partitions = new List<string>(metadata.partitions);
				for (int I = 0; I < metadata.partitions; I++)
				{
					Partitions.Add(TopicName.getPartition(I).ToString());
				}
				return Partitions;
			}
			else
			{
				return Collections.singletonList(Topic);
			}
			});
		}

		private static EventLoopGroup GetEventLoopGroup(ClientConfigurationData Conf)
		{
			ThreadFactory ThreadFactory = GetThreadFactory("pulsar-client-io");
			return EventLoopUtil.newEventLoopGroup(Conf.NumIoThreads, ThreadFactory);
		}

		private static ThreadFactory GetThreadFactory(string PoolName)
		{
			return new DefaultThreadFactory(PoolName, Thread.CurrentThread.Daemon);
		}

		public virtual void CleanupProducer<T1>(ProducerBase<T1> Producer)
		{
			lock (producers)
			{
				producers.remove(Producer);
			}
		}

		public virtual void CleanupConsumer<T1>(ConsumerBase<T1> Consumer)
		{
			lock (consumers)
			{
				consumers.remove(Consumer);
			}
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @VisibleForTesting int producersCount()
		public virtual int ProducersCount()
		{
			lock (producers)
			{
				return producers.size();
			}
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @VisibleForTesting int consumersCount()
		public virtual int ConsumersCount()
		{
			lock (consumers)
			{
				return consumers.size();
			}
		}

		private static Mode ConvertRegexSubscriptionMode(RegexSubscriptionMode RegexSubscriptionMode)
		{
			switch (RegexSubscriptionMode)
			{
			case RegexSubscriptionMode.PersistentOnly:
				return Mode.PERSISTENT;
			case RegexSubscriptionMode.NonPersistentOnly:
				return Mode.NON_PERSISTENT;
			case RegexSubscriptionMode.AllTopics:
				return Mode.ALL;
			default:
				return null;
			}
		}

		private SchemaInfoProvider NewSchemaProvider(string TopicName)
		{
			return new MultiVersionSchemaInfoProvider(TopicName.get(TopicName), this);
		}

		private LoadingCache<string, SchemaInfoProvider> SchemaProviderLoadingCache
		{
			get
			{
				return schemaProviderLoadingCache;
			}
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") protected java.util.concurrent.ValueTask<Void> preProcessSchemaBeforeSubscribe(PulsarClientImpl pulsarClientImpl, SharpPulsar.api.Schema schema, String topicName)
		public virtual ValueTask<Void> PreProcessSchemaBeforeSubscribe(PulsarClientImpl PulsarClientImpl, Schema Schema, string TopicName)
		{
			if (Schema != null && Schema.supportSchemaVersioning())
			{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final SharpPulsar.api.schema.SchemaInfoProvider schemaInfoProvider;
				SchemaInfoProvider SchemaInfoProvider;
				try
				{
					SchemaInfoProvider = PulsarClientImpl.SchemaProviderLoadingCache.get(TopicName);
				}
				catch (ExecutionException E)
				{
					log.error("Failed to load schema info provider for topic {}", TopicName, E);
					return FutureUtil.failedFuture(E.InnerException);
				}

				if (Schema.requireFetchingSchemaInfo())
				{
					return SchemaInfoProvider.LatestSchema.thenCompose(schemaInfo =>
					{
					if (null == schemaInfo)
					{
						if (!(Schema is AutoConsumeSchema))
						{
							return FutureUtil.failedFuture(new PulsarClientException.NotFoundException("No latest schema found for topic " + TopicName));
						}
					}
					try
					{
						log.info("Configuring schema for topic {} : {}", TopicName, schemaInfo);
						Schema.configureSchemaInfo(TopicName, "topic", schemaInfo);
					}
					catch (Exception Re)
					{
						return FutureUtil.failedFuture(Re);
					}
					Schema.SchemaInfoProvider = SchemaInfoProvider;
					return ValueTask.completedFuture(null);
					});
				}
				else
				{
					Schema.SchemaInfoProvider = SchemaInfoProvider;
				}
			}
			return ValueTask.completedFuture(null);
		}

		//
		// Transaction related API
		//

		// This method should be exposed in the PulsarClient interface. Only expose it when all the transaction features
		// are completed.
		// @Override
		public virtual TransactionBuilder NewTransaction()
		{
			return new TransactionBuilderImpl(this);
		}

	}

}