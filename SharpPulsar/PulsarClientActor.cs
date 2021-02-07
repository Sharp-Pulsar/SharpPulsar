using Akka.Actor;
using Akka.Event;
using BAMCIS.Util.Concurrent;
using SharpPulsar.Auth;
using SharpPulsar.Cache;
using SharpPulsar.Common.Naming;
using SharpPulsar.Configuration;
using SharpPulsar.Exceptions;
using SharpPulsar.Extension;
using SharpPulsar.Impl.Schema.Generic;
using SharpPulsar.Interfaces;
using SharpPulsar.Interfaces.ISchema;
using SharpPulsar.Messages.Client;
using SharpPulsar.Messages.Requests;
using SharpPulsar.Messages.Transaction;
using SharpPulsar.Schema;
using SharpPulsar.Transaction;
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
namespace SharpPulsar
{
	public class PulsarClientActor : ReceiveActor
	{

		private readonly ClientConfigurationData _conf;
		private IActorRef _lookup;
		private readonly IActorRef _cnxPool;
		private readonly ICancelable _timer;
		private readonly ILoggingAdapter _log;

		public enum State
		{
			Open = 0,
			Closing = 1,
			Closed = 2
		}

		private State _state;
		private readonly ISet<IActorRef> _producers;
		private readonly ISet<IActorRef> _consumers;

		private long _producerIdGenerator = 0L;
		private long _consumerIdGenerator = 0L;
		private long _requestIdGenerator = 0L;
		private readonly Cache<string, ISchemaInfoProvider> _schemaProviderLoadingCache = new Cache<string, ISchemaInfoProvider>(TimeSpan.FromMinutes(30), 100000);


		private readonly DateTime _clientClock;

		private IActorRef _tcClient;
		
		public PulsarClientActor(ClientConfigurationData conf, IAdvancedScheduler eventLoopGroup, IActorRef cnxPool, IActorRef txnCoordinator)
		{
			
			this._eventLoopGroup = eventLoopGroup;
			Auth = conf;
			_conf = conf;
			_clientClock = conf.Clock;
			conf.Authentication.Start();
			_cnxPool = cnxPool;
			_lookup = Context.ActorOf(BinaryProtoLookupService.Prop(Self, cnxPool, conf.ServiceUrl, conf.ListenerName, conf.UseTls, conf.MaxLookupRequest, conf.OperationTimeoutMs));

			_producers = new HashSet<IActorRef>();
			_consumers = new HashSet<IActorRef>();

			if (conf.EnableTransaction)
			{
				_tcClient = Context.ActorOf(TransactionCoordinatorClient.Prop(Self, conf));
				_tcClient.Tell(StartTransactionCoordinatorClient.Instance);
				txnCoordinator = _tcClient;
			}

			_state = State.Open;
			Receive<AddProducer>(m => _producers.Add(m.Producer));
			Receive<UpdateServiceUrl>(m => UpdateServiceUrl(m.ServiceUrl));
			Receive<AddConsumer>(m => _consumers.Add(m.Consumer));
			Receive<GetClientState>(_ => Sender.Tell((int)_state));
			Receive<CleanupConsumer>(m => _consumers.Remove(m.Consumer));
			Receive<CleanupProducer>(m => _producers.Remove(m.Producer));
			Receive<GetConnection>(m => {
				var cnx = GetConnection(m.Topic);
			});
			Receive<NewRequestId>(_ => Sender.Tell(new NewRequestIdResponse(NewRequestId())));
			Receive<Messages.Consumer.NewConsumerId>(_ => Sender.Tell(NewConsumerId()));
			Receive<Messages.Producer.NewProducerId>(_ => Sender.Tell(NewProducerId()));
			Receive<GetSchema>(s => {
				var response = _lookup.AskFor(s);
				Sender.Tell(response);
			});
			Receive<PreProcessSchemaBeforeSubscribe<object>>(p=> {
                try
                {
					var schema = PreProcessSchemaBeforeSubscribe(p.Schema, p.TopicName);
					Sender.Tell(new PreProcessedSchema<object>(schema));
				}
				catch(Exception ex)
                {
					_log.Error(ex.ToString());
					Sender.Tell(ex);
                }
			});
		}

		public static Props Prop(ClientConfigurationData conf, IAdvancedScheduler eventLoopGroup, IActorRef cnxPool, IActorRef txnCoordinator)
        {
			if (conf == null || string.IsNullOrWhiteSpace(conf.ServiceUrl) || eventLoopGroup == null)
			{
				throw new PulsarClientException.InvalidConfigurationException("Invalid client configuration");
			}
			return Props.Create(() => new PulsarClientActor(conf, eventLoopGroup, cnxPool, txnCoordinator));
        }
		private ClientConfigurationData Auth
		{
			set
			{
				if (string.IsNullOrWhiteSpace(value.AuthPluginClassName) || (string.IsNullOrWhiteSpace(value.AuthParams) && value.AuthParamMap == null))
				{
					return;
				}

				if (string.IsNullOrWhiteSpace(value.AuthParams))
				{
					value.Authentication = AuthenticationFactory.Create(value.AuthPluginClassName, value.AuthParams);
				}
				else if (value.AuthParamMap != null)
				{
					value.Authentication = AuthenticationFactory.Create(value.AuthPluginClassName, value.AuthParamMap);
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
        protected override void PostStop()
        {
			_lookup.GracefulStop(TimeSpan.FromSeconds(1));
			_cnxPool.GracefulStop(TimeSpan.FromSeconds(1));
			_timer.stop();
			_conf.Authentication.Dispose();
			base.PostStop();
        }

		public virtual void UpdateServiceUrl(string serviceUrl)
		{
			_log.Info("Updating service URL to {}", serviceUrl);

			_conf.ServiceUrl = serviceUrl;
			_lookup.Tell(new UpdateServiceUrl(serviceUrl));
			_cnxPool.Tell(CloseAllConnections.Instance);
		}

		private GetConnectionResponse GetConnection(string topic)
		{
			TopicName topicName = TopicName.Get(topic);
			var broker = _lookup.AskFor<GetBrokerResponse>(new GetBroker(topicName));
			var connection = _cnxPool.AskFor<GetConnectionResponse>(new GetConnection(broker.LogicalAddress, broker.PhysicalAddress));
			return connection;
		}

		/// <summary>
		/// visible for pulsar-functions * </summary>
		public virtual Timer Timer()
		{
			return _timer;
		}

		private long NewProducerId()
		{
			return _producerIdGenerator++;
		}

		private long NewConsumerId()
		{
			return _consumerIdGenerator++;
		}

		private long NewRequestId()
		{
			return _requestIdGenerator++;
		}

		private IActorRef CnxPool
		{
			get
			{
				return _cnxPool;
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

		private IList<string> GetPartitionsForTopic(string topic)
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


		private ISchemaInfoProvider NewSchemaProvider(string topicName)
		{
			return new MultiVersionSchemaInfoProvider(TopicName.Get(topicName), _log, _lookup);
		}

		private ISchema<object> PreProcessSchemaBeforeSubscribe(ISchema<object> schema, string topicName)
		{
			if (schema != null && schema.SupportSchemaVersioning())
			{
				ISchemaInfoProvider schemaInfoProvider;
				try
				{
					schemaInfoProvider = _schemaProviderLoadingCache.Get(topicName);
					if (schemaInfoProvider == null)
						_schemaProviderLoadingCache.Put(topicName, NewSchemaProvider(topicName));
				}
				catch (Exception e)
				{
					_log.Error($"Failed to load schema info provider for topic {topicName}: {e}");
					throw e;
				}
				schema = schema.Clone();
				if (schema.RequireFetchingSchemaInfo())
				{
					var finalSchema = schema;
					var schemaInfo = schemaInfoProvider.LatestSchema;
					if (null == schemaInfo)
					{
						if (!(finalSchema is AutoConsumeSchema))
						{
							throw new PulsarClientException.NotFoundException("No latest schema found for topic " + topicName);
						}
					}
					_log.Info($"Configuring schema for topic {topicName} : {schemaInfo}");
					finalSchema.ConfigureSchemaInfo(topicName, "topic", schemaInfo);
					finalSchema.SchemaInfoProvider = schemaInfoProvider;
					return finalSchema;
				}
				else
				{
					schema.SchemaInfoProvider = schemaInfoProvider;
				}
			}
			return schema;
		}

	}

}