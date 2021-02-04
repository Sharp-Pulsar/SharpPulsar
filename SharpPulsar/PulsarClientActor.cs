using Akka.Actor;
using Akka.Event;
using SharpPulsar.Configuration;
using SharpPulsar.Interfaces.ISchema;
using System;
using System.Collections.Concurrent;
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
namespace SharpPulsar
{
	public class PulsarClientActor : ReceiveActor
	{

		private readonly ClientConfigurationData _conf;
		private IActorRef _lookup;
		private readonly ConnectionPool _cnxPool;
		private readonly ICancelable _timer;
		private readonly ILoggingAdapter _log;

		public enum State
		{
			Open,
			Closing,
			Closed
		}

		private State _state;
		private readonly ISet<IActorRef> _producers;
		private readonly ISet<IActorRef> _consumers;

		private readonly long _producerIdGenerator = 0L;
		private readonly long _consumerIdGenerator = 0L;
		private readonly long _requestIdGenerator = 0L;

		private readonly LoadingCache<string, ISchemaInfoProvider> _schemaProviderLoadingCache = CacheBuilder.newBuilder().maximumSize(100000).expireAfterAccess(30, TimeUnit.MINUTES).build(new CacheLoaderAnonymousInnerClass());


		private readonly DateTime _clientClock;

		private IActorRef _tcClient;
		
		public PulsarClientActor(ClientConfigurationData conf, IAdvancedScheduler eventLoopGroup, IActorRef cnxPool)
		{
			if (conf == null || isBlank(conf.ServiceUrl) || eventLoopGroup == null)
			{
				throw new PulsarClientException.InvalidConfigurationException("Invalid client configuration");
			}
			this._eventLoopGroup = eventLoopGroup;
			Auth = conf;
			_conf = conf;
			this._clientClock = conf.Clock;
			conf.Authentication.start();
			this._cnxPool = cnxPool;
			_lookup = new BinaryProtoLookupService(this, conf.ServiceUrl, conf.ListenerName, conf.UseTls, _externalExecutorProvider.Executor);
			
			_timer = new HashedWheelTimer(GetThreadFactory("pulsar-timer"), 1, TimeUnit.MILLISECONDS);
			_producers = Collections.newSetFromMap(new ConcurrentDictionary<>());
			_consumers = Collections.newSetFromMap(new ConcurrentDictionary<>());

			if (conf.EnableTransaction)
			{
				_tcClient = new TransactionCoordinatorClientImpl(this);
				try
				{
					_tcClient.Start();
				}
				catch (Exception e)
				{
					_log.error("Start transactionCoordinatorClient error.", e);
					throw new PulsarClientException(e);
				}
			}

			_state.set(State.Open);
		}

		public static Props Prop(ClientConfigurationData conf, IAdvancedScheduler eventLoopGroup, IActorRef cnxPool)
        {
			return Props.Create(() => new PulsarClientActor(conf, eventLoopGroup, cnxPool));
        }
		private ClientConfigurationData Auth
		{
			set
			{
				if (StringUtils.isBlank(value.AuthPluginClassName) || (StringUtils.isBlank(value.AuthParams) && value.AuthParamMap == null))
				{
					return;
				}

				if (StringUtils.isNotBlank(value.AuthParams))
				{
					value.Authentication = AuthenticationFactory.create(value.AuthPluginClassName, value.AuthParams);
				}
				else if (value.AuthParamMap != null)
				{
					value.Authentication = AuthenticationFactory.create(value.AuthPluginClassName, value.AuthParamMap);
				}
			}
		}

		public virtual ClientConfigurationData Configuration
		{
			get
			{
				return _conf;
			}
		}

		public virtual Clock ClientClock
		{
			get
			{
				return _clientClock;
			}
		}

		public virtual AtomicReference<State> State
		{
			get
			{
				return _state;
			}
		}

		public virtual ProducerBuilder<sbyte[]> NewProducer()
		{
			return new ProducerBuilderImpl<sbyte[]>(this, Schema.BYTES);
		}

		public virtual ProducerBuilder<T> NewProducer<T>(Schema<T> schema)
		{
			return new ProducerBuilderImpl<T>(this, schema);
		}

		public virtual ConsumerBuilder<sbyte[]> NewConsumer()
		{
			return new ConsumerBuilderImpl<sbyte[]>(this, Schema.BYTES);
		}

		public virtual ConsumerBuilder<T> NewConsumer<T>(Schema<T> schema)
		{
			return new ConsumerBuilderImpl<T>(this, schema);
		}

		public virtual ReaderBuilder<sbyte[]> NewReader()
		{
			return new ReaderBuilderImpl<sbyte[]>(this, Schema.BYTES);
		}

		public virtual ReaderBuilder<T> NewReader<T>(Schema<T> schema)
		{
			return new ReaderBuilderImpl<T>(this, schema);
		}

		public virtual CompletableFuture<Producer<sbyte[]>> CreateProducerAsync(ProducerConfigurationData conf)
		{
			return CreateProducerAsync(conf, Schema.BYTES, null);
		}

		public virtual CompletableFuture<Producer<T>> CreateProducerAsync<T>(ProducerConfigurationData conf, Schema<T> schema)
		{
			return CreateProducerAsync(conf, schema, null);
		}

		public virtual CompletableFuture<Producer<T>> CreateProducerAsync<T>(ProducerConfigurationData conf, Schema<T> schema, ProducerInterceptors interceptors)
		{
			if (conf == null)
			{
				return FutureUtil.FailedFuture(new PulsarClientException.InvalidConfigurationException("Producer configuration undefined"));
			}

			if (schema is AutoConsumeSchema)
			{
				return FutureUtil.FailedFuture(new PulsarClientException.InvalidConfigurationException("AutoConsumeSchema is only used by consumers to detect schemas automatically"));
			}

			if (_state.get() != State.Open)
			{
				return FutureUtil.FailedFuture(new PulsarClientException.AlreadyClosedException("Client already closed : state = " + _state.get()));
			}

			string topic = conf.TopicName;

			if (!TopicName.IsValid(topic))
			{
				return FutureUtil.FailedFuture(new PulsarClientException.InvalidTopicNameException("Invalid topic name: '" + topic + "'"));
			}

			if (schema is AutoProduceBytesSchema)
			{
				AutoProduceBytesSchema autoProduceBytesSchema = (AutoProduceBytesSchema)schema;
				if (autoProduceBytesSchema.schemaInitialized())
				{
					return CreateProducerAsync(topic, conf, schema, interceptors);
				}
				return _lookup.GetSchema(TopicName.Get(conf.TopicName)).thenCompose(schemaInfoOptional =>
				{
					if (schemaInfoOptional.Present)
					{
						autoProduceBytesSchema.Schema = Schema.getSchema(schemaInfoOptional.get());
					}
					else
					{
						autoProduceBytesSchema.Schema = Schema.BYTES;
					}
					return CreateProducerAsync(topic, conf, schema, interceptors);
				});
			}
			else
			{
				return CreateProducerAsync(topic, conf, schema, interceptors);
			}

		}

		private CompletableFuture<Producer<T>> CreateProducerAsync<T>(string topic, ProducerConfigurationData conf, Schema<T> schema, ProducerInterceptors interceptors)
		{
			CompletableFuture<Producer<T>> producerCreatedFuture = new CompletableFuture<Producer<T>>();

			GetPartitionedTopicMetadata(topic).thenAccept(metadata =>
			{
				if (_log.DebugEnabled)
				{
					_log.debug("[{}] Received topic metadata. partitions: {}", topic, metadata.partitions);
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
				_producers.Add(producer);
			}).exceptionally(ex =>
			{
				_log.warn("[{}] Failed to get partitioned topic metadata: {}", topic, ex.Message);
				producerCreatedFuture.completeExceptionally(ex);
				return null;
			});

			return producerCreatedFuture;
		}

		public virtual CompletableFuture<Consumer<sbyte[]>> SubscribeAsync(ConsumerConfigurationData<sbyte[]> conf)
		{
			return SubscribeAsync(conf, Schema.BYTES, null);
		}

		public virtual CompletableFuture<Consumer<T>> SubscribeAsync<T>(ConsumerConfigurationData<T> conf, Schema<T> schema, ConsumerInterceptors<T> interceptors)
		{
			if (_state.get() != State.Open)
			{
				return FutureUtil.FailedFuture(new PulsarClientException.AlreadyClosedException("Client already closed"));
			}

			if (conf == null)
			{
				return FutureUtil.FailedFuture(new PulsarClientException.InvalidConfigurationException("Consumer configuration undefined"));
			}

			foreach (string topic in conf.TopicNames)
			{
				if (!TopicName.IsValid(topic))
				{
					return FutureUtil.FailedFuture(new PulsarClientException.InvalidTopicNameException("Invalid topic name: '" + topic + "'"));
				}
			}

			if (isBlank(conf.SubscriptionName))
			{
				return FutureUtil.FailedFuture(new PulsarClientException.InvalidConfigurationException("Empty subscription name"));
			}

			if (conf.ReadCompacted && (!conf.TopicNames.All(topic => TopicName.Get(topic).Domain == TopicDomain.Persistent) || (conf.SubscriptionType != SubscriptionType.Exclusive && conf.SubscriptionType != SubscriptionType.Failover)))
			{
				return FutureUtil.FailedFuture(new PulsarClientException.InvalidConfigurationException("Read compacted can only be used with exclusive or failover persistent subscriptions"));
			}

			if (conf.ConsumerEventListener != null && conf.SubscriptionType != SubscriptionType.Failover)
			{
				return FutureUtil.FailedFuture(new PulsarClientException.InvalidConfigurationException("Active consumer listener is only supported for failover subscription"));
			}

			if (conf.TopicsPattern != null)
			{
				// If use topicsPattern, we should not use topic(), and topics() method.
				if (!conf.TopicNames.Empty)
				{
					return FutureUtil.FailedFuture(new System.ArgumentException("Topic names list must be null when use topicsPattern"));
				}
				return PatternTopicSubscribeAsync(conf, schema, interceptors);
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

		private CompletableFuture<Consumer<T>> SingleTopicSubscribeAsync<T>(ConsumerConfigurationData<T> conf, Schema<T> schema, ConsumerInterceptors<T> interceptors)
		{
			return PreProcessSchemaBeforeSubscribe(this, schema, conf.SingleTopic).thenCompose(schemaClone => DoSingleTopicSubscribeAsync(conf, schemaClone, interceptors));
		}

		private CompletableFuture<Consumer<T>> DoSingleTopicSubscribeAsync<T>(ConsumerConfigurationData<T> conf, Schema<T> schema, ConsumerInterceptors<T> interceptors)
		{
			CompletableFuture<Consumer<T>> consumerSubscribedFuture = new CompletableFuture<Consumer<T>>();

			string topic = conf.SingleTopic;

			GetPartitionedTopicMetadata(topic).thenAccept(metadata =>
			{
				if (_log.DebugEnabled)
				{
					_log.debug("[{}] Received topic metadata. partitions: {}", topic, metadata.partitions);
				}
				ConsumerBase<T> consumer;
				ExecutorService listenerThread = _externalExecutorProvider.Executor;
				if (metadata.partitions > 0)
				{
					consumer = MultiTopicsConsumerImpl.CreatePartitionedConsumer(PulsarClientImpl.this, conf, listenerThread, consumerSubscribedFuture, metadata.partitions, schema, interceptors);
				}
				else
				{
					int partitionIndex = TopicName.GetPartitionIndex(topic);
					consumer = ConsumerImpl.NewConsumerImpl(PulsarClientImpl.this, topic, conf, listenerThread, partitionIndex, false, consumerSubscribedFuture, null, schema, interceptors, true);
				}
				_consumers.Add(consumer);
			}).exceptionally(ex =>
			{
				_log.warn("[{}] Failed to get partitioned topic metadata", topic, ex);
				consumerSubscribedFuture.completeExceptionally(ex);
				return null;
			});

			return consumerSubscribedFuture;
		}

		private CompletableFuture<Consumer<T>> MultiTopicSubscribeAsync<T>(ConsumerConfigurationData<T> conf, Schema<T> schema, ConsumerInterceptors<T> interceptors)
		{
			CompletableFuture<Consumer<T>> consumerSubscribedFuture = new CompletableFuture<Consumer<T>>();

			ConsumerBase<T> consumer = new MultiTopicsConsumerImpl<T>(PulsarClientImpl.this, conf, _externalExecutorProvider.Executor, consumerSubscribedFuture, schema, interceptors, true);

			_consumers.Add(consumer);

			return consumerSubscribedFuture;
		}

		public virtual CompletableFuture<Consumer<sbyte[]>> PatternTopicSubscribeAsync(ConsumerConfigurationData<sbyte[]> conf)
		{
			return PatternTopicSubscribeAsync(conf, Schema.BYTES, null);
		}

		private CompletableFuture<Consumer<T>> PatternTopicSubscribeAsync<T>(ConsumerConfigurationData<T> conf, Schema<T> schema, ConsumerInterceptors<T> interceptors)
		{
			string regex = conf.TopicsPattern.pattern();
			Mode subscriptionMode = ConvertRegexSubscriptionMode(conf.RegexSubscriptionMode);
			TopicName destination = TopicName.Get(regex);
			NamespaceName namespaceName = destination.NamespaceObject;

			CompletableFuture<Consumer<T>> consumerSubscribedFuture = new CompletableFuture<Consumer<T>>();
			_lookup.GetTopicsUnderNamespace(namespaceName, subscriptionMode).thenAccept(topics =>
			{
				if (_log.DebugEnabled)
				{
					_log.debug("Get topics under namespace {}, topics.size: {}", namespaceName.ToString(), topics.size());
					topics.forEach(topicName => _log.debug("Get topics under namespace {}, topic: {}", namespaceName.ToString(), topicName));
				}
				IList<string> topicsList = TopicsPatternFilter(topics, conf.TopicsPattern);
				conf.TopicNames.addAll(topicsList);
				ConsumerBase<T> consumer = new PatternMultiTopicsConsumerImpl<T>(conf.TopicsPattern, PulsarClientImpl.this, conf, _externalExecutorProvider.Executor, consumerSubscribedFuture, schema, subscriptionMode, interceptors);
				_consumers.Add(consumer);
			}).exceptionally(ex =>
			{
				_log.warn("[{}] Failed to get topics under namespace", namespaceName);
				consumerSubscribedFuture.completeExceptionally(ex);
				return null;
			});

			return consumerSubscribedFuture;
		}

		// get topics that match 'topicsPattern' from original topics list
		// return result should contain only topic names, without partition part
		public static IList<string> TopicsPatternFilter(IList<string> original, Pattern topicsPattern)
		{
			
			attern shortenedTopicsPattern = topicsPattern.ToString().Contains("://") ? Pattern.compile(topicsPattern.ToString().Split(@"\:\/\/", true)[1]) : topicsPattern;

			//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			return original.Select(TopicName.get).Select(TopicName::toString).Where(topic => shortenedTopicsPattern.matcher(topic.Split(@"\:\/\/")[1]).matches()).ToList();
		}

		public virtual CompletableFuture<Reader<sbyte[]>> CreateReaderAsync(ReaderConfigurationData<sbyte[]> conf)
		{
			return CreateReaderAsync(conf, Schema.BYTES);
		}

		public virtual CompletableFuture<Reader<T>> CreateReaderAsync<T>(ReaderConfigurationData<T> conf, Schema<T> schema)
		{
			return PreProcessSchemaBeforeSubscribe(this, schema, conf.TopicName).thenCompose(schemaClone => DoCreateReaderAsync(conf, schemaClone));
		}

		internal virtual CompletableFuture<Reader<T>> DoCreateReaderAsync<T>(ReaderConfigurationData<T> conf, Schema<T> schema)
		{
			if (_state.get() != State.Open)
			{
				return FutureUtil.FailedFuture(new PulsarClientException.AlreadyClosedException("Client already closed"));
			}

			if (conf == null)
			{
				return FutureUtil.FailedFuture(new PulsarClientException.InvalidConfigurationException("Consumer configuration undefined"));
			}

			string topic = conf.TopicName;

			if (!TopicName.IsValid(topic))
			{
				return FutureUtil.FailedFuture(new PulsarClientException.InvalidTopicNameException("Invalid topic name: '" + topic + "'"));
			}

			if (conf.StartMessageId == null)
			{
				return FutureUtil.FailedFuture(new PulsarClientException.InvalidConfigurationException("Invalid startMessageId"));
			}

			CompletableFuture<Reader<T>> readerFuture = new CompletableFuture<Reader<T>>();

			GetPartitionedTopicMetadata(topic).thenAccept(metadata =>
			{
				if (_log.DebugEnabled)
				{
					_log.debug("[{}] Received topic metadata. partitions: {}", topic, metadata.partitions);
				}
				if (metadata.partitions > 0 && MultiTopicsConsumerImpl.IsIllegalMultiTopicsMessageId(conf.StartMessageId))
				{
					readerFuture.completeExceptionally(new PulsarClientException("The partitioned topic startMessageId is illegal"));
					return;
				}
				CompletableFuture<Consumer<T>> consumerSubscribedFuture = new CompletableFuture<Consumer<T>>();
				ExecutorService listenerThread = _externalExecutorProvider.Executor;
				Reader<T> reader;
				ConsumerBase<T> consumer;
				if (metadata.partitions > 0)
				{
					reader = new MultiTopicsReaderImpl<T>(PulsarClientImpl.this, conf, listenerThread, consumerSubscribedFuture, schema);
					consumer = ((MultiTopicsReaderImpl<T>)reader).MultiTopicsConsumer;
				}
				else
				{
					reader = new ReaderImpl<T>(PulsarClientImpl.this, conf, listenerThread, consumerSubscribedFuture, schema);
					consumer = ((ReaderImpl<T>)reader).Consumer;
				}
				_consumers.Add(consumer);
				consumerSubscribedFuture.thenRun(() =>
				{
					readerFuture.complete(reader);
				}).exceptionally(ex =>
				{
					_log.warn("[{}] Failed to get create topic reader", topic, ex);
					readerFuture.completeExceptionally(ex);
					return null;
				});
			}).exceptionally(ex =>
			{
				_log.warn("[{}] Failed to get partitioned topic metadata", topic, ex);
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
		public virtual CompletableFuture<Optional<SchemaInfo>> GetSchema(string topic)
		{
			TopicName topicName;
			try
			{
				topicName = TopicName.Get(topic);
			}
			catch (Exception)
			{
				return FutureUtil.FailedFuture(new PulsarClientException.InvalidTopicNameException("Invalid topic name: '" + topic + "'"));
			}

			return _lookup.GetSchema(topicName);
		}

		public virtual void Dispose()
		{
			try
			{
				CloseAsync().get();
			}
			catch (InterruptedException e)
			{
				Thread.CurrentThread.Interrupt();
				throw PulsarClientException.Unwrap(e);
			}
			catch (ExecutionException e)
			{
				PulsarClientException unwrapped = PulsarClientException.Unwrap(e);
				if (unwrapped is PulsarClientException.AlreadyClosedException)
				{
					// this is not a problem
					return;
				}
				throw unwrapped;
			}
		}

		public virtual CompletableFuture<Void> CloseAsync()
		{
			_log.info("Client closing. URL: {}", _lookup.ServiceUrl);
			if (!_state.compareAndSet(State.Open, State.Closing))
			{
				return FutureUtil.FailedFuture(new PulsarClientException.AlreadyClosedException("Client already closed"));
			}

			CompletableFuture<Void> closeFuture = new CompletableFuture<Void>();
			IList<CompletableFuture<Void>> futures = Lists.newArrayList();

			_producers.forEach(p => futures.Add(p.closeAsync()));
			_consumers.forEach(c => futures.Add(c.closeAsync()));

			// Need to run the shutdown sequence in a separate thread to prevent deadlocks
			// If there are consumers or producers that need to be shutdown we cannot use the same thread
			// to shutdown the EventLoopGroup as well as that would be trying to shutdown itself thus a deadlock
			// would happen
			FutureUtil.WaitForAll(futures).thenRun(() => (new Thread(() =>
			{
				try
				{
					Shutdown();
					closeFuture.complete(null);
					_state.set(State.Closed);
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

		public virtual void Shutdown()
		{
			try
			{
				_lookup.close();
				_cnxPool.Dispose();
				_timer.stop();
				_externalExecutorProvider.ShutdownNow();
				_internalExecutorService.ShutdownNow();
				_conf.Authentication.Dispose();
			}
			catch (Exception t)
			{
				_log.warn("Failed to shutdown Pulsar client", t);
				throw PulsarClientException.Unwrap(t);
			}
		}

		public virtual bool Closed
		{
			get
			{
				State currentState = _state.get();
				return currentState == State.Closed || currentState == State.Closing;
			}
		}

		public virtual void UpdateServiceUrl(string serviceUrl)
		{
			lock (this)
			{
				_log.info("Updating service URL to {}", serviceUrl);

				_conf.ServiceUrl = serviceUrl;
				_lookup.UpdateServiceUrl(serviceUrl);
				_cnxPool.CloseAllConnections();
			}
		}

		protected internal virtual CompletableFuture<ClientCnx> GetConnection(in string topic)
		{
			TopicName topicName = TopicName.Get(topic);
			return _lookup.GetBroker(topicName).thenCompose(pair => _cnxPool.GetConnection(pair.Left, pair.Right));
		}

		/// <summary>
		/// visible for pulsar-functions * </summary>
		public virtual Timer Timer()
		{
			return _timer;
		}

		internal virtual ExecutorProvider ExternalExecutorProvider()
		{
			return _externalExecutorProvider;
		}

		internal virtual long NewProducerId()
		{
			return _producerIdGenerator.AndIncrement;
		}

		internal virtual long NewConsumerId()
		{
			return _consumerIdGenerator.AndIncrement;
		}

		public virtual long NewRequestId()
		{
			return _requestIdGenerator.AndIncrement;
		}

		public virtual ConnectionPool CnxPool
		{
			get
			{
				return _cnxPool;
			}
		}

		public virtual EventLoopGroup EventLoopGroup()
		{
			return _eventLoopGroup;
		}

		public virtual LookupService Lookup
		{
			get
			{
				return _lookup;
			}
		}

		public virtual void ReloadLookUp()
		{
			if (_conf.ServiceUrl.StartsWith("http"))
			{
				_lookup = new HttpLookupService(_conf, _eventLoopGroup);
			}
			else
			{
				_lookup = new BinaryProtoLookupService(this, _conf.ServiceUrl, _conf.ListenerName, _conf.UseTls, _externalExecutorProvider.Executor);
			}
		}

		public virtual CompletableFuture<int> GetNumberOfPartitions(string topic)
		{
			return GetPartitionedTopicMetadata(topic).thenApply(metadata => metadata.partitions);
		}

		public virtual CompletableFuture<PartitionedTopicMetadata> GetPartitionedTopicMetadata(string topic)
		{

			CompletableFuture<PartitionedTopicMetadata> metadataFuture = new CompletableFuture<PartitionedTopicMetadata>();

			try
			{
				TopicName topicName = TopicName.Get(topic);
				AtomicLong opTimeoutMs = new AtomicLong(_conf.OperationTimeoutMs);
				Backoff backoff = (new BackoffBuilder()).SetInitialTime(100, TimeUnit.MILLISECONDS).SetMandatoryStop(opTimeoutMs.get() * 2, TimeUnit.MILLISECONDS).SetMax(1, TimeUnit.MINUTES).Create();
				GetPartitionedTopicMetadata(topicName, backoff, opTimeoutMs, metadataFuture);
			}
			catch (System.ArgumentException e)
			{
				return FutureUtil.FailedFuture(new PulsarClientException.InvalidConfigurationException(e.Message));
			}
			return metadataFuture;
		}

		private void GetPartitionedTopicMetadata(TopicName topicName, Backoff backoff, AtomicLong remainingTime, CompletableFuture<PartitionedTopicMetadata> future)
		{
			_lookup.GetPartitionedTopicMetadata(topicName).thenAccept(future.complete).exceptionally(e =>
			{
				long nextDelay = Math.Min(backoff.Next(), remainingTime.get());
				bool isLookupThrottling = !PulsarClientException.IsRetriableError(e.Cause) || e.Cause is PulsarClientException.TooManyRequestsException || e.Cause is PulsarClientException.AuthenticationException;
				if (nextDelay <= 0 || isLookupThrottling)
				{
					future.completeExceptionally(e);
					return null;
				}
			((ScheduledExecutorService)_externalExecutorProvider.Executor).schedule(() =>
			{
				_log.warn("[topic: {}] Could not get connection while getPartitionedTopicMetadata -- Will try again in {} ms", topicName, nextDelay);
				remainingTime.addAndGet(-nextDelay);
				GetPartitionedTopicMetadata(topicName, backoff, remainingTime, future);
			}, nextDelay, TimeUnit.MILLISECONDS);
				return null;
			});
		}

		public virtual CompletableFuture<IList<string>> GetPartitionsForTopic(string topic)
		{
			return GetPartitionedTopicMetadata(topic).thenApply(metadata =>
			{
				if (metadata.partitions > 0)
				{
					TopicName topicName = TopicName.Get(topic);
					IList<string> partitions = new List<string>(metadata.partitions);
					for (int i = 0; i < metadata.partitions; i++)
					{
						partitions.Add(topicName.GetPartition(i).ToString());
					}
					return partitions;
				}
				else
				{
					return Collections.singletonList(topic);
				}
			});
		}


		internal virtual void CleanupProducer<T1>(ProducerBase<T1> producer)
		{
			lock (_producers)
			{
				_producers.remove(producer);
			}
		}

		internal virtual void CleanupConsumer<T1>(ConsumerBase<T1> consumer)
		{
			lock (_consumers)
			{
				_consumers.remove(consumer);
			}
		}

		internal virtual int ProducersCount()
		{
			lock (_producers)
			{
				return _producers.Count;
			}
		}

		internal virtual int ConsumersCount()
		{
			lock (_consumers)
			{
				return _consumers.Count;
			}
		}

		private static Mode ConvertRegexSubscriptionMode(RegexSubscriptionMode regexSubscriptionMode)
		{
			switch (regexSubscriptionMode)
			{
				case RegexSubscriptionMode.PersistentOnly:
					return Mode.PERSISTENT;
				case RegexSubscriptionMode.NonPersistentOnly:
					return Mode.NonPersistent;
				case RegexSubscriptionMode.AllTopics:
					return Mode.ALL;
				default:
					return null;
			}
		}

		private SchemaInfoProvider NewSchemaProvider(string topicName)
		{
			return new MultiVersionSchemaInfoProvider(TopicName.Get(topicName), this);
		}

		private LoadingCache<string, SchemaInfoProvider> SchemaProviderLoadingCache
		{
			get
			{
				return _schemaProviderLoadingCache;
			}
		}

		protected internal virtual CompletableFuture<Schema<T>> PreProcessSchemaBeforeSubscribe<T>(PulsarClientImpl pulsarClientImpl, Schema<T> schema, string topicName)
		{
			if (schema != null && schema.SupportSchemaVersioning())
			{
				SchemaInfoProvider schemaInfoProvider;
				try
				{
					schemaInfoProvider = pulsarClientImpl.SchemaProviderLoadingCache.get(topicName);
				}
				catch (ExecutionException e)
				{
					_log.error("Failed to load schema info provider for topic {}", topicName, e);
					return FutureUtil.FailedFuture(e.InnerException);
				}
				schema = schema.Clone();
				if (schema.RequireFetchingSchemaInfo())
				{
					Schema finalSchema = schema;
					return schemaInfoProvider.LatestSchema.thenCompose(schemaInfo =>
					{
						if (null == schemaInfo)
						{
							if (!(finalSchema is AutoConsumeSchema))
							{
								return FutureUtil.FailedFuture(new PulsarClientException.NotFoundException("No latest schema found for topic " + topicName));
							}
						}
						try
						{
							_log.info("Configuring schema for topic {} : {}", topicName, schemaInfo);
							finalSchema.configureSchemaInfo(topicName, "topic", schemaInfo);
						}
						catch (Exception re)
						{
							return FutureUtil.FailedFuture(re);
						}
						finalSchema.SchemaInfoProvider = schemaInfoProvider;
						return CompletableFuture.completedFuture(finalSchema);
					});
				}
				else
				{
					schema.SchemaInfoProvider = schemaInfoProvider;
				}
			}
			return CompletableFuture.completedFuture(schema);
		}

		public virtual ExecutorService InternalExecutorService
		{
			get
			{
				return _internalExecutorService.Executor;
			}
		}
		//
		// Transaction related API
		//

		// This method should be exposed in the PulsarClient interface. Only expose it when all the transaction features
		// are completed.
		// @Override
		public virtual TransactionBuilder NewTransaction()
		{
			return new TransactionBuilderImpl(this, _tcClient);
		}

	}

}