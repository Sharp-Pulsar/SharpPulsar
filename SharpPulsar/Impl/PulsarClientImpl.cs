using DotNetty.Transport.Channels;
using Microsoft.Extensions.Logging;
using SharpPulsar.Api;
using SharpPulsar.Api.Schema;
using SharpPulsar.Api.Transaction;
using SharpPulsar.Common.Naming;
using SharpPulsar.Common.Partition;
using SharpPulsar.Common.Schema;
using SharpPulsar.Exception;
using SharpPulsar.Impl.Conf;
using SharpPulsar.Impl.Schema;
using SharpPulsar.Impl.Schema.Generic;
using SharpPulsar.Impl.Transaction;
using SharpPulsar.Util;
using SharpPulsar.Util.Atomic;
using SharpPulsar.Util.Netty;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DotNetty.Common.Utilities;
using static SharpPulsar.Impl.ConsumerImpl<object>;
using static SharpPulsar.Protocol.Proto.CommandGetTopicsOfNamespace.Types;

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
	public enum State
	{
		Open,
		Closing,
		Closed
	}
	public sealed class PulsarClientImpl : IPulsarClient
	{
		private static readonly ILogger Log = new LoggerFactory().CreateLogger<PulsarClientImpl>();

		public ClientConfigurationData Configuration;
		public ILookupService Lookup;
		public  ConnectionPool CnxPool;
		public HashedWheelTimer Timer;
		private readonly ScheduledThreadPoolExecutor _executor;
		private State _state;
		private readonly HashSet<ProducerBase<object>> _producers;
		private readonly HashSet<ConsumerBase<object>> _consumers;

		private readonly AtomicLong _producerIdGenerator = new AtomicLong();
		private readonly AtomicLong _consumerIdGenerator = new AtomicLong();
		private readonly AtomicLong _requestIdGenerator = new AtomicLong();
		//private readonly ConcurrentDictionary<string, ISchemaInfoProvider> _schemaInfoProviderCache = new ConcurrentDictionary<string, ISchemaInfoProvider>();

		public  DateTime ClientClock {get;}
		public readonly IEventLoopGroup EventLoopGroup;

		public PulsarClientImpl(ClientConfigurationData conf) : this(conf, GetEventLoopGroup(conf))
		{
		}
		public PulsarClientImpl(ClientConfigurationData conf, IEventLoopGroup eventLoopGroup) : this(conf, eventLoopGroup, new ConnectionPool(conf, eventLoopGroup))
		{
		}
		public PulsarClientImpl(ClientConfigurationData conf, IEventLoopGroup eventLoopGroup, ConnectionPool cnxPool)
		{
			if (conf == null || string.IsNullOrWhiteSpace(conf.ServiceUrl) || eventLoopGroup == null)
			{
				throw new PulsarClientException.InvalidConfigurationException("Invalid client configuration");
			}
			_executor = new ScheduledThreadPoolExecutor(conf.NumIoThreads);
			Auth = conf;
			EventLoopGroup = eventLoopGroup;
			Configuration = conf;
			ClientClock = conf.Clock;
			conf.Authentication.Start();
			CnxPool = cnxPool;

			Lookup = new BinaryProtoLookupService(this, conf.ServiceUrl, conf.UseTls, ExternalExecutorProvider());
			_producers = new HashSet<ProducerBase<object>>();
			_consumers = new HashSet<ConsumerBase<object>>();
			_state = State.Open;
		}

		private ClientConfigurationData Auth
		{
			set
			{
				if (string.IsNullOrWhiteSpace(value.AuthPluginClassName) || string.IsNullOrWhiteSpace(value.AuthParams))
				{
					return;
				}
    
				value.Authentication = AuthenticationFactory.Create(value.AuthPluginClassName, value.AuthParams);
			}
		}



		public IProducerBuilder<sbyte[]> NewProducer()
		{
			return new ProducerBuilderImpl<sbyte[]>(this, SchemaFields.Bytes);
		}

		public IProducerBuilder<T> NewProducer<T>(ISchema<T> schema)
		{
			return new ProducerBuilderImpl<T>(this, schema);
		}

		public IConsumerBuilder<sbyte[]> NewConsumer()
		{
			return new ConsumerBuilderImpl<sbyte[]>(this, SchemaFields.Bytes);
		}

		public IConsumerBuilder<T> NewConsumer<T>(ISchema<T> schema)
		{
			return new ConsumerBuilderImpl<T>(this, schema);
		}

		public IReaderBuilder<sbyte[]> NewReader()
		{
			return new ReaderBuilderImpl<sbyte[]>(this, SchemaFields.Bytes);
		}

		public IReaderBuilder<T> NewReader<T>(ISchema<T> schema)
		{
			return new ReaderBuilderImpl<T>(this, schema);
		}

		public ValueTask<IProducer<sbyte[]>> CreateProducerAsync(ProducerConfigurationData conf)
		{
			return CreateProducerAsync(conf, SchemaFields.Bytes, null);
		}

		public ValueTask<IProducer<T>> CreateProducerAsync<T>(ProducerConfigurationData conf, ISchema<T> schema)
		{
			return CreateProducerAsync(conf, schema, null);
		}

		public ValueTask<IProducer<T>> CreateProducerAsync<T>(ProducerConfigurationData conf, ISchema<T> schema, ProducerInterceptors interceptors)
		{
			if (conf == null)
			{
				return new ValueTask<IProducer<T>>(Task.FromException<IProducer<T>>(new PulsarClientException.InvalidConfigurationException("Producer configuration undefined")));
			}

			if (schema is AutoConsumeSchema)
			{
				return new ValueTask<IProducer<T>>(Task.FromException<IProducer<T>>(new PulsarClientException.InvalidConfigurationException("AutoConsumeSchema is only used by consumers to detect schemas automatically")));
			}

			if (_state != State.Open)
			{
				return new ValueTask<IProducer<T>>(Task.FromException<IProducer<T>>(new PulsarClientException.AlreadyClosedException("Client already closed : state = " + _state)));
			}

			var topic = conf.TopicName;

			if (!TopicName.IsValid(topic))
			{
				return new ValueTask<IProducer<T>>(Task.FromException<IProducer<T>>(new PulsarClientException.InvalidTopicNameException("Invalid topic name: '" + topic + "'")));
			}

			if (schema is AutoProduceBytesSchema<T> autoProduceBytesSchema)
			{
                if (autoProduceBytesSchema.SchemaInitialized())
				{
					return CreateProducerAsync(topic, conf, schema, interceptors);
				}
				var lkup = Lookup.GetSchema(TopicName.Get(conf.TopicName));
				var schemaInfoOptional = lkup.Result;
				autoProduceBytesSchema.Schema = schemaInfoOptional != null ? ISchema<T>.GetSchema(schemaInfoOptional) : autoProduceBytesSchema.Schema;
				return CreateProducerAsync(topic, conf, schema, interceptors);
			}
			else
			{
				return CreateProducerAsync(topic, conf, schema, interceptors);
			}

		}

		private ValueTask<IProducer<T>> CreateProducerAsync<T>(string topic, ProducerConfigurationData conf, ISchema<T> schema, ProducerInterceptors interceptors)
		{
			var producerCreated = new TaskCompletionSource<IProducer<T>>();

			GetPartitionedTopicMetadata(topic).AsTask().ContinueWith(metadataTask => 
			{
				var metadata = metadataTask.Result;
				if (metadataTask.IsFaulted){
					Log.LogWarning("[{}] Failed to get partitioned topic metadata: {}", topic, metadataTask.Exception.Message);
					producerCreated.SetException(metadataTask.Exception);
					return;
				}
				if (Log.IsEnabled(LogLevel.Debug))
				{
					Log.LogDebug("[{}] Received topic metadata. partitions: {}", topic, metadata.partitions);
				}
				ProducerBase<T> producer;
				if (metadata.partitions > 0)
				{
					producer = new PartitionedProducerImpl<T>(this, topic, conf, metadata.partitions, producerCreated, schema, interceptors);
				}
				else
				{
					producer = new ProducerImpl<T>(this, topic, conf, producerCreated, -1, schema, interceptors);
				}
				lock (_producers)
				{
					var p = (ProducerBase<object>)Convert.ChangeType(producer, typeof(IProducer<object>));
					_producers.Add(p);//pend
				}
				producerCreated.SetResult(producer);
			});

			return new ValueTask<IProducer<T>>(producerCreated.Task.Result);
		}

		public ValueTask<IConsumer<sbyte[]>> SubscribeAsync(ConsumerConfigurationData<sbyte[]> conf)
		{
			return SubscribeAsync(conf, SchemaFields.Bytes, null);
		}

		public ValueTask<IConsumer<T>> SubscribeAsync<T>(ConsumerConfigurationData<T> conf, ISchema<T> schema, ConsumerInterceptors<T> interceptors)
		{
			if (_state != State.Open)
			{
				return new ValueTask<IConsumer<T>>(Task.FromException<IConsumer<T>>(new PulsarClientException.AlreadyClosedException("Client already closed")));
			}

			if (conf == null)
			{
				return new ValueTask<IConsumer<T>>(Task.FromException<IConsumer<T>>(new PulsarClientException.InvalidConfigurationException("Consumer configuration undefined")));
			}

			if (conf.TopicNames.All(TopicName.IsValid))
			{
				return new ValueTask<IConsumer<T>>(Task.FromException<IConsumer<T>>(new PulsarClientException.InvalidTopicNameException("Invalid topic name")));
			}

			if (string.IsNullOrWhiteSpace(conf.SubscriptionName))
			{
				return new ValueTask<IConsumer<T>>(Task.FromException<IConsumer<T>>(new PulsarClientException.InvalidConfigurationException("Empty subscription name")));
			}

			if (conf.ReadCompacted && (conf.TopicNames.Any(topic => TopicName.Get(topic).Domain != TopicDomain.persistent) || (conf.SubscriptionType != SubscriptionType.Exclusive && conf.SubscriptionType != SubscriptionType.Failover)))
			{
				return new ValueTask<IConsumer<T>>(Task.FromException<IConsumer<T>>(new PulsarClientException.InvalidConfigurationException("Read compacted can only be used with exclusive of failover persistent subscriptions")));
			}

			if (conf.ConsumerEventListener != null && conf.SubscriptionType != SubscriptionType.Failover)
			{
				return new ValueTask<IConsumer<T>>(Task.FromException<IConsumer<T>>(new PulsarClientException.InvalidConfigurationException("Active consumer listener is only supported for failover subscription")));
			}

			if (conf.TopicsPattern != null)
			{
				// If use topicsPattern, we should not use topic(), and topics() method.
				if (!conf.TopicNames.Any())
				{
					return new ValueTask<IConsumer<T>>(Task.FromException<IConsumer<T>>(new ArgumentException("Topic names list must be null when use topicsPattern")));
				}
				return PatternTopicSubscribeAsync(conf, schema, interceptors);
			}
			else if (conf.TopicNames.Count == 1)
			{
				return SingleTopicSubscribeAsync(conf, schema, interceptors);
			}
			else
			{
				return MultiTopicSubscribeAsync(conf, schema, interceptors);
			}
		}

		private ValueTask<IConsumer<T>> SingleTopicSubscribeAsync<T>(ConsumerConfigurationData<T> conf, ISchema<T> schema, ConsumerInterceptors<T> interceptors)
		{
			 PreProcessSchemaBeforeSubscribe(this, schema, conf.SingleTopic);
			return DoSingleTopicSubscribeAsync(conf, schema, interceptors);
		}

		private ValueTask<IConsumer<T>> DoSingleTopicSubscribeAsync<T>(ConsumerConfigurationData<T> conf, ISchema<T> schema, ConsumerInterceptors<T> interceptors)
		{
			var consumerSubscribedTask = new TaskCompletionSource<IConsumer<T>>();

			var topic = conf.SingleTopic;

			GetPartitionedTopicMetadata(topic).AsTask().ContinueWith(task =>
			{
				if (task.IsFaulted)
				{
					Log.LogWarning("[{}] Failed to get partitioned topic metadata", topic, task.Exception);
					consumerSubscribedTask.SetException(task.Exception ?? throw new InvalidOperationException());
					return;
				}
				var metadata = task.Result;
				if (Log.IsEnabled(LogLevel.Debug))
				{
					Log.LogDebug("[{}] Received topic metadata. partitions: {}", topic, metadata.partitions);
				}
				ConsumerBase<T> consumer;
				var listenerThread = _executor;
				if (metadata.partitions > 0)
				{
					consumer = MultiTopicsConsumerImpl<T>.CreatePartitionedConsumer(this, conf, consumerSubscribedTask, metadata.partitions, schema, interceptors);
				}
				else
				{
					var durable = (ConsumerImpl<T>.SubscriptionMode)Convert.ChangeType(SubscriptionMode.Durable, typeof(ConsumerImpl<T>.SubscriptionMode));
					var partitionIndex = TopicName.GetPartitionIndex(topic);
					consumer = ConsumerImpl<T>.NewConsumerImpl(this, topic, conf, listenerThread, partitionIndex, false, consumerSubscribedTask, durable, null, schema, interceptors, true);
				}
				lock (_consumers)
				{
					var c = (ConsumerBase<object>)Convert.ChangeType(consumer, typeof(IConsumer<object>));
					_consumers.Add(c);
				}
				consumerSubscribedTask.SetResult(consumer);
			});

			return new ValueTask<IConsumer<T>>(consumerSubscribedTask.Task.Result);
		}

		private ValueTask<IConsumer<T>> MultiTopicSubscribeAsync<T>(ConsumerConfigurationData<T> conf, ISchema<T> schema, ConsumerInterceptors<T> interceptors)
		{
			var consumerSubscribedTask = new TaskCompletionSource<IConsumer<T>>();

			var consumer = new MultiTopicsConsumerImpl<T>(this, conf, consumerSubscribedTask, schema, interceptors, true, ExternalExecutorProvider());

			lock (_consumers)
			{
				var c = (ConsumerBase<object>)Convert.ChangeType(consumer, typeof(IConsumer<object>));
				_consumers.Add(c);
			}

			return new ValueTask<IConsumer<T>>(consumerSubscribedTask.Task.Result);
		}

		public ValueTask<IConsumer<sbyte[]>> PatternTopicSubscribeAsync(ConsumerConfigurationData<sbyte[]> conf)
		{
			return PatternTopicSubscribeAsync(conf, SchemaFields.Bytes, null);
		}

		private ValueTask<IConsumer<T>> PatternTopicSubscribeAsync<T>(ConsumerConfigurationData<T> conf, ISchema<T> schema, ConsumerInterceptors<T> interceptors)
		{
			var regex = conf.TopicsPattern.ToString();
			var subscriptionMode = ConvertRegexSubscriptionMode(conf.RegexSubscriptionMode);
			var destination = TopicName.Get(regex);
			var namespaceName = destination.NamespaceObject;

			var consumerSubscribedTask = new TaskCompletionSource<IConsumer<T>>();
			var lkup = Lookup.GetTopicsUnderNamespace(namespaceName, subscriptionMode);
			if(lkup.IsFaulted)
			{
				Log.LogWarning("[{}] Failed to get topics under namespace", namespaceName);
				return new ValueTask<IConsumer<T>>(Task.FromException<IConsumer<T>>(lkup.AsTask().Exception));
			}
			var topics = lkup.Result.ToList();
			if(Log.IsEnabled(LogLevel.Debug))
			{
				Log.LogDebug("Get topics under namespace {}, topics.size: {}", namespaceName.ToString(), topics.Count);
				topics.ForEach(topicName => Log.LogDebug("Get topics under namespace {}, topic: {}", namespaceName.ToString(), topicName));
			}
			var topicsList = TopicsPatternFilter(topics, conf.TopicsPattern).ToList();
			topicsList.ForEach(x => conf.TopicNames.Add(x));
			var consumer = new PatternMultiTopicsConsumerImpl<T>(conf.TopicsPattern, this, conf, consumerSubscribedTask, schema, subscriptionMode, interceptors, ExternalExecutorProvider());
			lock (_consumers)
			{
				var c = (ConsumerBase<object>)Convert.ChangeType(consumer, typeof(IConsumer<object>));
				_consumers.Add(c);
			}
			
			return new ValueTask<IConsumer<T>>(consumer);
		}

		// get topics that match 'topicsPattern' from original topics list
		// return result should contain only topic names, without partition part
		public static IList<string> TopicsPatternFilter(IList<string> original, Regex topicsPattern)
		{
			var pattern = topicsPattern.ToString().Contains("://") ? new Regex(topicsPattern.ToString().Split(@"\:\/\/")[1]) : topicsPattern;


			return original.Select(TopicName.Get).Select(x => x.ToString()).Where(topic => pattern.Match(topic.Split(@"\:\/\/")[1]).Success).ToList();
		}

		public ValueTask<IReader<sbyte[]>> CreateReaderAsync(ReaderConfigurationData<sbyte[]> conf)
		{
			return CreateReaderAsync(conf, SchemaFields.Bytes);
		}

		public ValueTask<IReader<T>> CreateReaderAsync<T>(ReaderConfigurationData<T> conf, ISchema<T> schema)
		{
			PreProcessSchemaBeforeSubscribe(this, schema, conf.TopicName);
			return DoCreateReaderAsync(conf, schema);
		}

		public ValueTask<IReader<T>> DoCreateReaderAsync<T>(ReaderConfigurationData<T> conf, ISchema<T> schema)
		{
			if (_state != State.Open)
			{
				return new ValueTask<IReader<T>>(Task.FromException<IReader<T>>(new PulsarClientException.AlreadyClosedException("Client already closed")));
			}

			if (conf == null)
			{
				return new ValueTask<IReader<T>>(Task.FromException<IReader<T>>(new PulsarClientException.InvalidConfigurationException("Consumer configuration undefined")));
			}

			var topic = conf.TopicName;

			if (!TopicName.IsValid(topic))
			{
				return new ValueTask<IReader<T>>(Task.FromException<IReader<T>>(new PulsarClientException.InvalidTopicNameException("Invalid topic name")));
			}

			if (conf.StartMessageId == null)
			{
				return new ValueTask<IReader<T>>(Task.FromException<IReader<T>>(new PulsarClientException.InvalidConfigurationException("Invalid startMessageId")));
			}

			var readerTask = new TaskCompletionSource<IReader<T>>();

			var topicRe = GetPartitionedTopicMetadata(topic);
			var astask = topicRe.AsTask();
			if(astask.IsFaulted)
			{
				Log.LogWarning("[{}] Failed to get partitioned topic metadata", topic, astask.Exception);
				readerTask.SetException(astask.Exception ?? throw new InvalidOperationException());
				return new ValueTask<IReader<T>>(Task.FromException<IReader<T>>(astask.Exception));
			}
			var metadata = topicRe.Result;
			if (Log.IsEnabled(LogLevel.Debug))
			{
				Log.LogDebug("[{}] Received topic metadata. partitions: {}", topic, metadata.partitions);
			}
			if (metadata.partitions > 0)
			{
				readerTask.SetException(new PulsarClientException("Topic reader cannot be created on a partitioned topic"));
				return new ValueTask<IReader<T>>(Task.FromException<IReader<T>>(readerTask.Task.Exception ?? throw new InvalidOperationException()));
			}
			var consumerSubscribedTask = new TaskCompletionSource<IConsumer<T>>();

			var reader = new ReaderImpl<T>(this, conf, consumerSubscribedTask, schema, ExternalExecutorProvider());
			lock (_consumers)
			{
				var c = (ConsumerBase<object>)Convert.ChangeType(reader.Consumer, typeof(IConsumer<object>));
				_consumers.Add(c);
			}
			consumerSubscribedTask.Task.ContinueWith(task=> {
				readerTask.SetResult(reader);
			});
			return new ValueTask<IReader<T>>(reader);
		}

		/// <summary>
		/// Read the schema information for a given topic.
		/// 
		/// If the topic does not exist or it has no schema associated, it will return an empty response
		/// </summary>
		public ValueTask<SchemaInfo> GetSchema(string topic)
		{
			TopicName topicName;
			try
			{
				topicName = TopicName.Get(topic);
			}
			catch (System.Exception ex)
			{
				return new ValueTask<SchemaInfo>(Task.FromException<SchemaInfo>(new PulsarClientException.InvalidTopicNameException("Invalid topic name: " + topic)));
			}

			return Lookup.GetSchema(topicName);
		}

		public void Close()
		{
			try
			{
				CloseAsync();
			}
			catch (System.Exception e)
			{
				throw PulsarClientException.Unwrap(e);
			}
		}

		public ValueTask CloseAsync()
		{
			Log.LogInformation("Client closing. URL: {}", Lookup.ServiceUrl);
			if (_state != State.Open)
			{
				return new ValueTask(Task.FromException(new PulsarClientException.AlreadyClosedException("Client already closed")));

			}

			var closeTask = new TaskCompletionSource<Task>();
			IList<Task> tasks = new List<Task>();

			lock (_producers)
			{
				// Copy to a new list, because the closing will trigger a removal from the map
				// and invalidate the iterator

				var producersToClose = new List<ProducerBase<object>>(_producers).ToList();
				producersToClose.ForEach(p => tasks.Add(p.CloseAsync().AsTask()));
			}

			lock (_consumers)
			{
				var consumersToClose = new List<ConsumerBase<object>>(_consumers);
				consumersToClose.ForEach(c => tasks.Add(c.CloseAsync().AsTask()));
			}

			// Need to run the shutdown sequence in a separate thread to prevent deadlocks
			// If there are consumers or producers that need to be shutdown we cannot use the same thread
			// to shutdown the EventLoopGroup as well as that would be trying to shutdown itself thus a deadlock
			// would happen
			Task.WhenAll(tasks.ToArray()).ContinueWith(t => {
				var rt = Task.Run(()=>
				{
					ContinueWithShutDown(closeTask);
				});
				if (rt.IsFaulted)
				{
					closeTask.SetException(rt.Exception ?? throw new InvalidOperationException());
				}
			});
			return new ValueTask(closeTask.Task);
		}
		public void ContinueWithShutDown(TaskCompletionSource<Task> task)
		{
			try
			{

				Shutdown();
				task.SetResult(null);
				_state = State.Closed;
			}
			catch (PulsarClientException e)
			{
				task.SetException(e);
			}
		}
		public void Shutdown()
		{
			try
			{
				Lookup.Close();
				CnxPool.Dispose();
				Timer.StopAsync();
				Configuration.Authentication.Dispose();
			}
			catch (System.Exception t)
			{
				Log.LogWarning("Failed to shutdown Pulsar client", t);
				throw PulsarClientException.Unwrap(t);
			}
		}

		public void UpdateServiceUrl(string serviceUrl)
		{
			lock (this)
			{
				Log.LogInformation("Updating service URL to {}", serviceUrl);
        
				Configuration.ServiceUrl = serviceUrl;
				Lookup.UpdateServiceUrl(serviceUrl);
				CnxPool.CloseAllConnections();
			}
		}

		public ValueTask<ClientCnx> GetConnection(in string topic)
		{
			var topicName = TopicName.Get(topic);
			var lkup = Lookup.GetBroker(topicName).Result;
			return CnxPool.GetConnection((IPEndPoint)lkup.Key, (IPEndPoint)lkup.Value);
		}

		/// <summary>
		/// visible for pulsar-functions * </summary>
		public HashedWheelTimer GetTimer()
		{
			return Timer;
		}

		public ScheduledThreadPoolExecutor ExternalExecutorProvider()
		{
			return _executor;
		}

		public long NewProducerId()
		{
			return _producerIdGenerator.Increment();
		}

		public long NewConsumerId()
		{
			return _consumerIdGenerator.Increment();
		}

		public long NewRequestId()
		{
			return _requestIdGenerator.Increment();
		}


		public void ReloadLookUp()
		{
			Lookup = new BinaryProtoLookupService(this, Configuration.ServiceUrl, Configuration.UseTls, ExternalExecutorProvider());
		}

		public ValueTask<int> GetNumberOfPartitions(string topic)
		{
			return new ValueTask<int>(GetPartitionedTopicMetadata(topic).Result.partitions);
		}

		public ValueTask<PartitionedTopicMetadata> GetPartitionedTopicMetadata(string topic)
		{

			var metadataTask = new TaskCompletionSource<PartitionedTopicMetadata>();

			try
			{
				var topicName = TopicName.Get(topic);
				var opTimeoutMs = new AtomicLong(Configuration.OperationTimeoutMs);
				var backoff = (new BackoffBuilder()).SetInitialTime(100, BAMCIS.Util.Concurrent.TimeUnit.MILLISECONDS).SetMandatoryStop(opTimeoutMs.Get() * 2, BAMCIS.Util.Concurrent.TimeUnit.MILLISECONDS).SetMax(0, BAMCIS.Util.Concurrent.TimeUnit.MILLISECONDS).Create();
				GetPartitionedTopicMetadata(topicName, backoff, opTimeoutMs, metadataTask);
			}
			catch (ArgumentException e)
			{
				return new ValueTask<PartitionedTopicMetadata>(Task.FromException<PartitionedTopicMetadata>(new PulsarClientException.InvalidConfigurationException(e.Message)));
			}
			return new ValueTask<PartitionedTopicMetadata>(metadataTask.Task);
		}

		private void GetPartitionedTopicMetadata(TopicName topicName, Backoff backoff, AtomicLong remainingTime, TaskCompletionSource<PartitionedTopicMetadata> task)
		{
			var lkup = Lookup.GetPartitionedTopicMetadata(topicName);
			if(lkup.IsCompletedSuccessfully)
			{
				task.SetResult(lkup.Result);
			}
			if(lkup.IsFaulted)
			{
				var ex = lkup.AsTask().Exception;
				var nextDelay = Math.Min(backoff.Next(), remainingTime.Get());
				var isLookupThrottling = ex is PulsarClientException.TooManyRequestsException;
				if (nextDelay <= 0 || isLookupThrottling)
				{
					task.SetException(ex);
					return;
				}
                _executor.Schedule(() =>
                {
                    Log.LogWarning("[topic: {}] Could not get connection while getPartitionedTopicMetadata -- Will try again in {} ms", topicName, nextDelay);
					remainingTime.AddAndGet(-nextDelay);
					GetPartitionedTopicMetadata(topicName, backoff, remainingTime, task);
                }, TimeSpan.FromSeconds(nextDelay));
			}
			
			return;
		}

		public ValueTask<IList<string>> GetPartitionsForTopic(string topic)
		{
			var lkup = GetPartitionedTopicMetadata(topic);
			var metadata = lkup.Result;
			if (metadata.partitions > 0)
			{
				var topicName = TopicName.Get(topic);
				IList<string> partitions = new List<string>(metadata.partitions);
				var topName = new TopicName();
				for (var i = 0; i < metadata.partitions; i++)
				{
					var prt = topName.GetPartition(i).ToString();
					partitions.Add(prt);
				}
				return new ValueTask<IList<string>>(partitions);
			}
			else
			{
				return new ValueTask<IList<string>>(new List<string>() { topic });
			}
		}
		private static IEventLoopGroup GetEventLoopGroup(ClientConfigurationData conf)
		{
			return EventLoopUtil.NewEventLoopGroup(conf.NumIoThreads);
		}

		
		public void CleanupProducer<T1>(ProducerBase<T1> producer)
		{
			lock (_producers)
			{
				var p = (ProducerBase<object>)Convert.ChangeType(producer, typeof(IProducer<object>));
				_producers.Remove(p);
			}
		}

		public void CleanupConsumer<T1>(ConsumerBase<T1> consumer)
		{
			lock (_consumers)
			{
				var c = (ConsumerBase<object>)Convert.ChangeType(consumer, typeof(IConsumer<object>));
				_consumers.Remove(c);
			}
		}

		public int ProducersCount()
		{
			lock (_producers)
			{
				return _producers.Count;
			}
		}

		public int ConsumersCount()
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
					return Mode.Persistent;
				case RegexSubscriptionMode.NonPersistentOnly:
					return Mode.NonPersistent;
				case RegexSubscriptionMode.AllTopics:
				default:
					return Mode.All;
			}
		}

		private ISchemaInfoProvider NewSchemaProvider(string topicName)
		{
			return new MultiVersionSchemaInfoProvider(TopicName.Get(topicName), this);
		}


		public ValueTask PreProcessSchemaBeforeSubscribe<T>(PulsarClientImpl pulsarClientImpl, ISchema<T> schema, string topicName)
		{
			if (schema != null && schema.SupportSchemaVersioning())
			{
				ISchemaInfoProvider schemaInfoProvider;
				try
				{
					schemaInfoProvider = pulsarClientImpl.NewSchemaProvider(topicName);
				}
				catch (System.Exception e)
				{
					Log.LogError("Failed to load schema info provider for topic {}", topicName, e);
					return new ValueTask(Task.FromException(e.InnerException ?? throw new InvalidOperationException()));
				}

				if (schema.RequireFetchingSchemaInfo())
				{
					var schemaProv = schemaInfoProvider.LatestSchema;
					var schemaInfo = schemaProv.Result;
					if (schemaInfo == null)
					{
						if (!(schema is AutoConsumeSchema))
						{
							throw new PulsarClientException.NotFoundException("No latest schema found for topic " + topicName);
						}
					}
					try
					{
						Log.LogInformation("Configuring schema for topic {} : {}", topicName, schemaInfo);
						schema.ConfigureSchemaInfo(topicName, "topic", schemaInfo);
					}
					catch (System.Exception re)
					{
						return new ValueTask(Task.FromException(re));
					}
					schema.SchemaInfoProvider = schemaInfoProvider;
				}
				else
				{
					schema.SchemaInfoProvider = schemaInfoProvider;
				}
			}
			return new ValueTask();
		}

		//
		// Transaction related API
		//

		// This method should be exposed in the PulsarClient interface. Only expose it when all the transaction features
		// are completed.
		// @Override
		public TransactionBuilder NewTransaction()
		{
			return new TransactionBuilderImpl(this);
		}

		ValueTask IPulsarClient.CloseAsync()
		{
			throw new NotImplementedException();
		}

		public void Dispose()
		{
			throw new NotImplementedException();
		}
	}

}