using DotNetty.Transport.Channels;
using Microsoft.Extensions.Logging;
using Optional;
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
using System.Threading;
using System.Threading.Tasks;
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
	public class PulsarClientImpl : IPulsarClient
	{
		private static readonly ILogger log = new LoggerFactory().CreateLogger<PulsarClientImpl>();

		public ClientConfigurationData Configuration;
		public ILookupService Lookup;
		public  ConnectionPool CnxPool;
		public Timer Timer;
		private readonly ExecutorProvider externalExecutorProvider;
		private State state;
		private readonly HashSet<ProducerBase<object>> _producers;
		private readonly HashSet<ConsumerBase<object>> _consumers;

		private readonly AtomicLong _producerIdGenerator = new AtomicLong();
		private readonly AtomicLong _consumerIdGenerator = new AtomicLong();
		private readonly AtomicLong _requestIdGenerator = new AtomicLong();


		public virtual DateTime ClientClock {get;}
		private readonly IEventLoopGroup _eventLoopGroup;

		public PulsarClientImpl(ClientConfigurationData Conf) : this(Conf, GetEventLoopGroup(Conf))
		{
		}
		public PulsarClientImpl(ClientConfigurationData Conf, IEventLoopGroup eventLoopGroup) : this(Conf, eventLoopGroup, new ConnectionPool(Conf, eventLoopGroup))
		{
		}
		public PulsarClientImpl(ClientConfigurationData Conf, IEventLoopGroup eventLoopGroup, ConnectionPool CnxPool)
		{
			if (Conf == null || string.IsNullOrWhiteSpace(Conf.ServiceUrl) || eventLoopGroup == null)
			{
				throw new PulsarClientException.InvalidConfigurationException("Invalid client configuration");
			}
			Auth = Conf;
			_eventLoopGroup = eventLoopGroup;
			this.Configuration = Conf;
			this.ClientClock = Conf.clock;
			Conf.Authentication.Start();
			this.CnxPool = CnxPool;

			Lookup = new BinaryProtoLookupService(this, Conf.ServiceUrl, Conf.UseTls);
			_producers = new HashSet<ProducerBase<object>>();
			_consumers = new HashSet<ConsumerBase<object>>();
			state = State.Open;
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
			return new ProducerBuilderImpl<sbyte[]>(this, SchemaFields.BYTES);
		}

		public IProducerBuilder<T> NewProducer<T>(ISchema<T> Schema)
		{
			return new ProducerBuilderImpl<T>(this, Schema);
		}

		public IConsumerBuilder<sbyte[]> NewConsumer()
		{
			return new ConsumerBuilderImpl<sbyte[]>(this, SchemaFields.BYTES);
		}

		public IConsumerBuilder<T> NewConsumer<T>(ISchema<T> Schema)
		{
			return new ConsumerBuilderImpl<T>(this, Schema);
		}

		public ReaderBuilder<sbyte[]> NewReader()
		{
			return new ReaderBuilderImpl<sbyte[]>(this, SchemaFields.BYTES);
		}

		public ReaderBuilder<T> NewReader<T>(ISchema<T> Schema)
		{
			return new ReaderBuilderImpl<T>(this, Schema);
		}

		public virtual ValueTask<IProducer<sbyte[]>> CreateProducerAsync(ProducerConfigurationData Conf)
		{
			return CreateProducerAsync(Conf, SchemaFields.BYTES, null);
		}

		public virtual ValueTask<IProducer<T>> CreateProducerAsync<T>(ProducerConfigurationData Conf, ISchema<T> Schema)
		{
			return CreateProducerAsync(Conf, Schema, null);
		}

		public virtual ValueTask<IProducer<T>> CreateProducerAsync<T>(ProducerConfigurationData Conf, ISchema<T> Schema, ProducerInterceptors Interceptors)
		{
			if (Conf == null)
			{
				return new ValueTask<IProducer<T>>(Task.FromException<IProducer<T>>(new PulsarClientException.InvalidConfigurationException("Producer configuration undefined")));
			}

			if (Schema is AutoConsumeSchema)
			{
				return new ValueTask<IProducer<T>>(Task.FromException<IProducer<T>>(new PulsarClientException.InvalidConfigurationException("AutoConsumeSchema is only used by consumers to detect schemas automatically")));
			}

			if (state != State.Open)
			{
				return new ValueTask<IProducer<T>>(Task.FromException<IProducer<T>>(new PulsarClientException.AlreadyClosedException("Client already closed : state = " + state)));
			}

			string topic = Conf.TopicName;

			if (!TopicName.IsValid(topic))
			{
				return new ValueTask<IProducer<T>>(Task.FromException<IProducer<T>>(new PulsarClientException.InvalidTopicNameException("Invalid topic name: '" + topic + "'")));
			}

			if (Schema is AutoProduceBytesSchema<T>)
			{
				var autoProduceBytesSchema = (AutoProduceBytesSchema<T>) Schema;
				if (autoProduceBytesSchema.SchemaInitialized())
				{
					return CreateProducerAsync(topic, Conf, Schema, Interceptors);
				}
				var lkup = Lookup.GetSchema(TopicName.Get(Conf.TopicName));
				var schemaInfoOptional = lkup.Result;
				if (schemaInfoOptional != null)
				{
					autoProduceBytesSchema.Schema = ISchema<T>.GetSchema(schemaInfoOptional);
				}
				else
				{
					autoProduceBytesSchema.Schema = Schema;
				}
				return CreateProducerAsync(topic, Conf, Schema, Interceptors);
			}
			else
			{
				return CreateProducerAsync(topic, Conf, Schema, Interceptors);
			}

		}

		private ValueTask<IProducer<T>> CreateProducerAsync<T>(string Topic, ProducerConfigurationData Conf, ISchema<T> Schema, ProducerInterceptors Interceptors)
		{
			var producerCreated = new TaskCompletionSource<IProducer<T>>();

			GetPartitionedTopicMetadata(Topic).AsTask().ContinueWith(metadataTask => 
			{
				var metadata = metadataTask.Result;
				if (metadataTask.IsFaulted){
					log.LogWarning("[{}] Failed to get partitioned topic metadata: {}", Topic, metadataTask.Exception.Message);
					producerCreated.SetException(metadataTask.Exception);
					return;
				}
				if (log.IsEnabled(LogLevel.Debug))
				{
					log.LogDebug("[{}] Received topic metadata. partitions: {}", Topic, metadata.partitions);
				}
				ProducerBase<T> producer;
				if (metadata.partitions > 0)
				{
					producer = new PartitionedProducerImpl<T>(this, Topic, Conf, metadata.partitions, producerCreated, Schema, Interceptors);
				}
				else
				{
					producer = new ProducerImpl<T>(this, Topic, Conf, producerCreated, -1, Schema, Interceptors);
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

		public virtual ValueTask<IConsumer<sbyte[]>> SubscribeAsync(ConsumerConfigurationData<sbyte[]> Conf)
		{
			return SubscribeAsync(Conf, SchemaFields.BYTES, null);
		}

		public virtual ValueTask<IConsumer<T>> SubscribeAsync<T>(ConsumerConfigurationData<T> Conf, ISchema<T> Schema, ConsumerInterceptors<T> Interceptors)
		{
			if (state != State.Open)
			{
				return new ValueTask<IConsumer<T>>(Task.FromException<IConsumer<T>>(new PulsarClientException.AlreadyClosedException("Client already closed")));
			}

			if (Conf == null)
			{
				return new ValueTask<IConsumer<T>>(Task.FromException<IConsumer<T>>(new PulsarClientException.InvalidConfigurationException("Consumer configuration undefined")));
			}

			if (Conf.TopicNames.All(TopicName.IsValid))
			{
				return new ValueTask<IConsumer<T>>(Task.FromException<IConsumer<T>>(new PulsarClientException.InvalidTopicNameException("Invalid topic name")));
			}

			if (string.IsNullOrWhiteSpace(Conf.SubscriptionName))
			{
				return new ValueTask<IConsumer<T>>(Task.FromException<IConsumer<T>>(new PulsarClientException.InvalidConfigurationException("Empty subscription name")));
			}

			if (Conf.ReadCompacted && (!Conf.TopicNames.All(topic => TopicName.Get(topic).Domain == TopicDomain.persistent) || (Conf.SubscriptionType != SubscriptionType.Exclusive && Conf.SubscriptionType != SubscriptionType.Failover)))
			{
				return new ValueTask<IConsumer<T>>(Task.FromException<IConsumer<T>>(new PulsarClientException.InvalidConfigurationException("Read compacted can only be used with exclusive of failover persistent subscriptions")));
			}

			if (Conf.ConsumerEventListener != null && Conf.SubscriptionType != SubscriptionType.Failover)
			{
				return new ValueTask<IConsumer<T>>(Task.FromException<IConsumer<T>>(new PulsarClientException.InvalidConfigurationException("Active consumer listener is only supported for failover subscription")));
			}

			if (Conf.TopicsPattern != null)
			{
				// If use topicsPattern, we should not use topic(), and topics() method.
				if (!Conf.TopicNames.Any())
				{
					return new ValueTask<IConsumer<T>>(Task.FromException<IConsumer<T>>(new ArgumentException("Topic names list must be null when use topicsPattern")));
				}
				return PatternTopicSubscribeAsync(Conf, Schema, Interceptors);
			}
			else if (Conf.TopicNames.Count == 1)
			{
				return SingleTopicSubscribeAsync(Conf, Schema, Interceptors);
			}
			else
			{
				return MultiTopicSubscribeAsync(Conf, Schema, Interceptors);
			}
		}

		private ValueTask<IConsumer<T>> SingleTopicSubscribeAsync<T>(ConsumerConfigurationData<T> Conf, ISchema<T> Schema, ConsumerInterceptors<T> Interceptors)
		{
			 PreProcessSchemaBeforeSubscribe(this, Schema, Conf.SingleTopic);
			return DoSingleTopicSubscribeAsync(Conf, Schema, Interceptors);
		}

		private ValueTask<IConsumer<T>> DoSingleTopicSubscribeAsync<T>(ConsumerConfigurationData<T> Conf, ISchema<T> Schema, ConsumerInterceptors<T> Interceptors)
		{
			var consumerSubscribedTask = new TaskCompletionSource<IConsumer<T>>();

			string Topic = Conf.SingleTopic;

			GetPartitionedTopicMetadata(Topic).AsTask().ContinueWith(task =>
			{
				if (task.IsFaulted)
				{
					log.LogWarning("[{}] Failed to get partitioned topic metadata", Topic, task.Exception);
					consumerSubscribedTask.SetException(task.Exception);
					return;
				}
				var metadata = task.Result;
				if (log.IsEnabled(LogLevel.Debug))
				{
					log.LogDebug("[{}] Received topic metadata. partitions: {}", Topic, metadata.partitions);
				}
				ConsumerBase<T> consumer;
				var ListenerThread = externalExecutorProvider.Executor;
				if (metadata.partitions > 0)
				{
					consumer = MultiTopicsConsumerImpl<T>.CreatePartitionedConsumer(this, Conf, consumerSubscribedTask, metadata.partitions, Schema, Interceptors);
				}
				else
				{
					var durable = (ConsumerImpl<T>.SubscriptionMode)Convert.ChangeType(SubscriptionMode.Durable, typeof(ConsumerImpl<T>.SubscriptionMode));
					int PartitionIndex = TopicName.GetPartitionIndex(Topic);
					consumer = ConsumerImpl<T>.NewConsumerImpl(this, Topic, Conf, ListenerThread, PartitionIndex, false, consumerSubscribedTask, durable, null, Schema, Interceptors, true);
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

		private ValueTask<IConsumer<T>> MultiTopicSubscribeAsync<T>(ConsumerConfigurationData<T> Conf, ISchema<T> Schema, ConsumerInterceptors<T> Interceptors)
		{
			var consumerSubscribedTask = new TaskCompletionSource<IConsumer<T>>();

			var consumer = new MultiTopicsConsumerImpl<T>(this, Conf, consumerSubscribedTask, Schema, Interceptors, true);

			lock (_consumers)
			{
				var c = (ConsumerBase<object>)Convert.ChangeType(consumer, typeof(IConsumer<object>));
				_consumers.Add(c);
			}

			return new ValueTask<IConsumer<T>>(consumerSubscribedTask.Task.Result);
		}

		public virtual ValueTask<IConsumer<sbyte[]>> PatternTopicSubscribeAsync(ConsumerConfigurationData<sbyte[]> Conf)
		{
			return PatternTopicSubscribeAsync(Conf, SchemaFields.BYTES, null);
		}

		private ValueTask<IConsumer<T>> PatternTopicSubscribeAsync<T>(ConsumerConfigurationData<T> Conf, ISchema<T> Schema, ConsumerInterceptors<T> Interceptors)
		{
			string regex = Conf.TopicsPattern.ToString();
			Mode SubscriptionMode = ConvertRegexSubscriptionMode(Conf.RegexSubscriptionMode);
			TopicName Destination = TopicName.Get(regex);
			NamespaceName NamespaceName = Destination.NamespaceObject;

			var consumerSubscribedTask = new TaskCompletionSource<IConsumer<T>>();
			var lkup = Lookup.GetTopicsUnderNamespace(NamespaceName, SubscriptionMode);
			if(lkup.IsFaulted)
			{
				log.LogWarning("[{}] Failed to get topics under namespace", NamespaceName);
				return new ValueTask<IConsumer<T>>(Task.FromException<IConsumer<T>>(lkup.AsTask().Exception));
			}
			var topics = lkup.Result.ToList();
			if(log.IsEnabled(LogLevel.Debug))
			{
				log.LogDebug("Get topics under namespace {}, topics.size: {}", NamespaceName.ToString(), topics.Count);
				topics.ForEach(topicName => log.LogDebug("Get topics under namespace {}, topic: {}", NamespaceName.ToString(), topicName));
			}
			var TopicsList = TopicsPatternFilter(topics, Conf.TopicsPattern).ToList();
			TopicsList.ForEach(x => Conf.TopicNames.Add(x));
			var consumer = new PatternMultiTopicsConsumerImpl<T>(Conf.TopicsPattern, this, Conf, consumerSubscribedTask, Schema, SubscriptionMode, Interceptors);
			lock (_consumers)
			{
				var c = (ConsumerBase<object>)Convert.ChangeType(consumer, typeof(IConsumer<object>));
				_consumers.Add(c);
			}
			
			return new ValueTask<IConsumer<T>>(consumer);
		}

		// get topics that match 'topicsPattern' from original topics list
		// return result should contain only topic names, without partition part
		public static IList<string> TopicsPatternFilter(IList<string> Original, Regex TopicsPattern)
		{
			var pattern = TopicsPattern.ToString().Contains("://") ? new Regex(TopicsPattern.ToString().Split(@"\:\/\/")[1]) : TopicsPattern;


			return Original.Select(TopicName.Get).Select(x => x.ToString()).Where(topic => pattern.Match(topic.Split(@"\:\/\/")[1]).Success).ToList();
		}

		public virtual ValueTask<IReader<sbyte[]>> CreateReaderAsync(ReaderConfigurationData<sbyte[]> Conf)
		{
			return CreateReaderAsync(Conf, SchemaFields.BYTES);
		}

		public virtual ValueTask<IReader<T>> CreateReaderAsync<T>(ReaderConfigurationData<T> Conf, ISchema<T> Schema)
		{
			PreProcessSchemaBeforeSubscribe(this, Schema, Conf.TopicName);
			return DoCreateReaderAsync(Conf, Schema);
		}

		public virtual ValueTask<IReader<T>> DoCreateReaderAsync<T>(ReaderConfigurationData<T> Conf, ISchema<T> Schema)
		{
			if (state != State.Open)
			{
				return new ValueTask<IReader<T>>(Task.FromException<IReader<T>>(new PulsarClientException.AlreadyClosedException("Client already closed")));
			}

			if (Conf == null)
			{
				return new ValueTask<IReader<T>>(Task.FromException<IReader<T>>(new PulsarClientException.InvalidConfigurationException("Consumer configuration undefined")));
			}

			string topic = Conf.TopicName;

			if (!TopicName.IsValid(topic))
			{
				return new ValueTask<IReader<T>>(Task.FromException<IReader<T>>(new PulsarClientException.InvalidTopicNameException("Invalid topic name")));
			}

			if (Conf.StartMessageId == null)
			{
				return new ValueTask<IReader<T>>(Task.FromException<IReader<T>>(new PulsarClientException.InvalidConfigurationException("Invalid startMessageId")));
			}

			TaskCompletionSource<IReader<T>> readerTask = new TaskCompletionSource<IReader<T>>();

			var topicRe = GetPartitionedTopicMetadata(topic);
			var astask = topicRe.AsTask();
			if(astask.IsFaulted)
			{
				log.LogWarning("[{}] Failed to get partitioned topic metadata", topic, astask.Exception);
				readerTask.SetException(astask.Exception);
				return new ValueTask<IReader<T>>(Task.FromException<IReader<T>>(astask.Exception));
			}
			var metadata = topicRe.Result;
			if (log.IsEnabled(LogLevel.Debug))
			{
				log.LogDebug("[{}] Received topic metadata. partitions: {}", topic, metadata.partitions);
			}
			if (metadata.partitions > 0)
			{
				readerTask.SetException(new PulsarClientException("Topic reader cannot be created on a partitioned topic"));
				return new ValueTask<IReader<T>>(Task.FromException<IReader<T>>(readerTask.Task.Exception));
			}
			var consumerSubscribedTask = new TaskCompletionSource<IConsumer<T>>();

			var reader = new ReaderImpl<T>(this, Conf, consumerSubscribedTask, Schema);
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
		public virtual ValueTask<SchemaInfo> GetSchema(string topic)
		{
			TopicName TopicName;
			try
			{
				TopicName = TopicName.Get(topic);
			}
			catch (System.Exception ex)
			{
				return new ValueTask<SchemaInfo>(Task.FromException<SchemaInfo>(new PulsarClientException.InvalidTopicNameException("Invalid topic name: " + topic)));
			}

			return Lookup.GetSchema(TopicName);
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
			log.LogInformation("Client closing. URL: {}", Lookup.ServiceUrl);
			if (state != State.Open)
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
					closeTask.SetException(rt.Exception);
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
				state = State.Closed;
			}
			catch (PulsarClientException E)
			{
				task.SetException(E);
			}
		}
		public void Shutdown()
		{
			try
			{
				Lookup.Close();
				CnxPool.Dispose();
				Timer.Dispose();
				Configuration.Authentication.Dispose();
			}
			catch (System.Exception t)
			{
				log.LogWarning("Failed to shutdown Pulsar client", t);
				throw PulsarClientException.Unwrap(t);
			}
		}

		public void UpdateServiceUrl(string ServiceUrl)
		{
			lock (this)
			{
				log.LogInformation("Updating service URL to {}", ServiceUrl);
        
				Configuration.ServiceUrl = ServiceUrl;
				Lookup.UpdateServiceUrl(ServiceUrl);
				CnxPool.CloseAllConnections();
			}
		}

		public virtual ValueTask<ClientCnx> GetConnection(in string topic)
		{
			TopicName topicName = TopicName.Get(topic);
			var lkup = Lookup.GetBroker(topicName).Result;
			return CnxPool.GetConnection((IPEndPoint)lkup.Key, (IPEndPoint)lkup.Value);
		}

		/// <summary>
		/// visible for pulsar-functions * </summary>
		public virtual Timer GetTimer()
		{
			return Timer;
		}

		public virtual ExecutorProvider ExternalExecutorProvider()
		{
			return externalExecutorProvider;
		}

		public virtual long NewProducerId()
		{
			return _producerIdGenerator.Increment();
		}

		public virtual long NewConsumerId()
		{
			return _consumerIdGenerator.Increment();
		}

		public virtual long NewRequestId()
		{
			return _requestIdGenerator.Increment();
		}


		public virtual void ReloadLookUp()
		{
			Lookup = new BinaryProtoLookupService(this, Configuration.ServiceUrl, Configuration.UseTls);
		}

		public virtual ValueTask<int> GetNumberOfPartitions(string topic)
		{
			return new ValueTask<int>(GetPartitionedTopicMetadata(topic).Result.partitions);
		}

		public virtual ValueTask<PartitionedTopicMetadata> GetPartitionedTopicMetadata(string Topic)
		{

			var metadataTask = new TaskCompletionSource<PartitionedTopicMetadata>();

			try
			{
				TopicName TopicName = TopicName.Get(Topic);
				AtomicLong OpTimeoutMs = new AtomicLong(Configuration.OperationTimeoutMs);
				Backoff Backoff = (new BackoffBuilder()).SetInitialTime(100, BAMCIS.Util.Concurrent.TimeUnit.MILLISECONDS).SetMandatoryStop(OpTimeoutMs.Get() * 2, BAMCIS.Util.Concurrent.TimeUnit.MILLISECONDS).SetMax(0, BAMCIS.Util.Concurrent.TimeUnit.MILLISECONDS).Create();
				GetPartitionedTopicMetadata(TopicName, Backoff, OpTimeoutMs, metadataTask);
			}
			catch (System.ArgumentException e)
			{
				return new ValueTask<PartitionedTopicMetadata>(Task.FromException<PartitionedTopicMetadata>(new PulsarClientException.InvalidConfigurationException(e.Message)));
			}
			return new ValueTask<PartitionedTopicMetadata>(metadataTask.Task);
		}

		private void GetPartitionedTopicMetadata(TopicName topicName, Backoff Backoff, AtomicLong RemainingTime, TaskCompletionSource<PartitionedTopicMetadata> task)
		{
			var lkup = Lookup.GetPartitionedTopicMetadata(topicName);
			if(lkup.IsCompletedSuccessfully)
			{
				task.SetResult(lkup.Result);
			}
			if(lkup.IsFaulted)
			{
				var ex = lkup.AsTask().Exception;
				long nextDelay = Math.Min(Backoff.Next(), RemainingTime.Get());
				bool IsLookupThrottling = ex is PulsarClientException.TooManyRequestsException;
				if (nextDelay <= 0 || IsLookupThrottling)
				{
					task.SetException(ex);
					return;
				}
				Timer = new Timer(_=> {
					log.LogWarning("[topic: {}] Could not get connection while getPartitionedTopicMetadata -- Will try again in {} ms", topicName, nextDelay);
					RemainingTime.AddAndGet(-nextDelay);
					GetPartitionedTopicMetadata(topicName, Backoff, RemainingTime, task);
				}, null, (int)nextDelay, Timeout.Infinite);
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
				for (int i = 0; i < metadata.partitions; i++)
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
		private static IEventLoopGroup GetEventLoopGroup(ClientConfigurationData Conf)
		{
			return EventLoopUtil.NewEventLoopGroup(Conf.NumIoThreads);
		}

		
		public virtual void CleanupProducer<T1>(ProducerBase<T1> producer)
		{
			lock (_producers)
			{
				var p = (ProducerBase<object>)Convert.ChangeType(producer, typeof(IProducer<object>));
				_producers.Remove(p);
			}
		}

		public virtual void CleanupConsumer<T1>(ConsumerBase<T1> consumer)
		{
			lock (_consumers)
			{
				var c = (ConsumerBase<object>)Convert.ChangeType(consumer, typeof(IConsumer<object>));
				_consumers.Remove(c);
			}
		}

		public virtual int ProducersCount()
		{
			lock (_producers)
			{
				return _producers.Count;
			}
		}

		public virtual int ConsumersCount()
		{
			lock (_consumers)
			{
				return _consumers.Count;
			}
		}

		private static Mode ConvertRegexSubscriptionMode(RegexSubscriptionMode RegexSubscriptionMode)
		{
			switch (RegexSubscriptionMode)
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

		private SchemaInfoProvider NewSchemaProvider(string topicName)
		{
			return new MultiVersionSchemaInfoProvider(TopicName.Get(topicName), this);
		}

		/*private LoadingCache<string, SchemaInfoProvider> SchemaProviderLoadingCache
		{
			get
			{
				return schemaProviderLoadingCache;
			}
		}*/

		public virtual ValueTask PreProcessSchemaBeforeSubscribe<T>(PulsarClientImpl PulsarClientImpl, ISchema<T> Schema, string TopicName)
		{
			if (Schema != null && Schema.SupportSchemaVersioning())
			{
				SchemaInfoProvider SchemaInfoProvider;
				try
				{
					SchemaInfoProvider = PulsarClientImpl.SchemaProviderLoadingCache.get(TopicName);
				}
				catch (System.Exception e)
				{
					log.LogError("Failed to load schema info provider for topic {}", TopicName, e);
					return new ValueTask(Task.FromException(e.InnerException));
				}

				if (Schema.RequireFetchingSchemaInfo())
				{
					var schemaProv = SchemaInfoProvider.LatestSchema;
					var schemaInfo = schemaProv.Result;
					if (schemaInfo == null)
					{
						if (!(Schema is AutoConsumeSchema))
						{
							throw new PulsarClientException.NotFoundException("No latest schema found for topic " + TopicName);
						}
					}
					try
					{
						log.LogInformation("Configuring schema for topic {} : {}", TopicName, schemaInfo);
						Schema.ConfigureSchemaInfo(TopicName, "topic", schemaInfo);
					}
					catch (System.Exception re)
					{
						return new ValueTask(Task.FromException(re));
					}
					Schema.SchemaInfoProvider = SchemaInfoProvider;
				}
				else
				{
					Schema.SchemaInfoProvider = SchemaInfoProvider;
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
		public virtual TransactionBuilder NewTransaction()
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