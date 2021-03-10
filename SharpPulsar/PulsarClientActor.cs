using Akka.Actor;
using Akka.Event;
using BAMCIS.Util.Concurrent;
using SharpPulsar.Auth;
using SharpPulsar.Cache;
using SharpPulsar.Common.Naming;
using SharpPulsar.Common.Partition;
using SharpPulsar.Configuration;
using SharpPulsar.Exceptions;
using SharpPulsar.Extension;
using SharpPulsar.Impl.Schema.Generic;
using SharpPulsar.Interfaces;
using SharpPulsar.Interfaces.ISchema;
using SharpPulsar.Messages;
using SharpPulsar.Messages.Client;
using SharpPulsar.Messages.Requests;
using SharpPulsar.Messages.Transaction;
using SharpPulsar.Schema;
using SharpPulsar.Transaction;
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
namespace SharpPulsar
{
	internal class PulsarClientActor : ReceiveActor
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
		private readonly DateTime _clientClock;

		private IActorRef _tcClient;
		public PulsarClientActor(ClientConfigurationData conf, IActorRef cnxPool, IActorRef txnCoordinator, IActorRef lookup, IActorRef idGenerator)
		{
			if (conf == null || string.IsNullOrWhiteSpace(conf.ServiceUrl))
			{
				throw new PulsarClientException.InvalidConfigurationException("Invalid client configuration");
			}
			_tcClient = txnCoordinator;
			_log = Context.GetLogger();
			Auth = conf;
			_conf = conf;
			_clientClock = conf.Clock;
			conf.Authentication.Start();
			_cnxPool = cnxPool;
			_lookup = lookup;
			_producers = new HashSet<IActorRef>();
			_consumers = new HashSet<IActorRef>();
			_state = State.Open;
			Receive<AddProducer>(m => _producers.Add(m.Producer));
			Receive<UpdateServiceUrl>(m => UpdateServiceUrl(m.ServiceUrl));
			Receive<AddConsumer>(m => _consumers.Add(m.Consumer));
			Receive<GetClientState>(_ => Sender.Tell((int)_state));
			Receive<CleanupConsumer>(m => _consumers.Remove(m.Consumer));
			Receive<CleanupProducer>(m => _producers.Remove(m.Producer));
			ReceiveAsync<GetBroker>(async m => {
				var cnx = await GetBroker(m.TopicName);
				Sender.Tell(cnx);
			});
			ReceiveAsync<GetConnection>(async m => {
				var cnx = await GetConnection(m.Topic);
				Sender.Tell(cnx);
			});
			Receive<GetTcClient>(_ => {
				Sender.Tell(new TcClient(_tcClient));
			});
			ReceiveAsync<GetSchema>(async s => {
				var response = await _lookup.AskFor(s);
				Sender.Tell(response);
			});
			ReceiveAsync<GetPartitionedTopicMetadata>(async p => 
			{
                try
                {
					var partition = await GetPartitionedTopicMetadata(p.TopicName.ToString());
					Sender.Tell(partition);
				}
				catch(Exception ex)
                {
					Sender.Tell(new ClientExceptions(new PulsarClientException(ex)));
                }
			});
			ReceiveAsync<GetPartitionsForTopic>(async s => 
			{
				try
				{
					var topics = await GetPartitionsForTopic(s.TopicName);
					Sender.Tell(new PartitionsForTopic(topics));
				}
				catch (Exception ex)
				{
					Sender.Tell(new ClientExceptions(new PulsarClientException(ex)));
				}
			});
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
			_conf.Authentication = null;
			base.PostStop();
        }

		public virtual void UpdateServiceUrl(string serviceUrl)
		{
			_log.Info("Updating service URL to {}", serviceUrl);

			_conf.ServiceUrl = serviceUrl;
			_lookup.Tell(new UpdateServiceUrl(serviceUrl));
			_cnxPool.Tell(CloseAllConnections.Instance);
		}

		private async ValueTask<GetBrokerResponse> GetBroker(TopicName topic)
		{
			return await _lookup.AskFor<GetBrokerResponse>(new GetBroker(topic));
		}
		private async ValueTask<GetConnectionResponse> GetConnection(string topic)
		{
			var topicName = TopicName.Get(topic);
			var broker = await _lookup.AskFor<GetBrokerResponse>(new GetBroker(topicName));
			var connection = await _cnxPool.AskFor<GetConnectionResponse>(new GetConnection(broker.LogicalAddress, broker.PhysicalAddress));
			return connection;
		}


		private IActorRef CnxPool
		{
			get
			{
				return _cnxPool;
			}
		}

		private async ValueTask<int> GetNumberOfPartitions(string topic)
		{
			var p = await GetPartitionedTopicMetadata(topic);
			return p.Partitions;
		}
		private async ValueTask<PartitionedTopicMetadata> GetPartitionedTopicMetadata(string topic)
		{
			try
			{
				TopicName topicName = TopicName.Get(topic);
				var o = await _lookup.AskFor(new GetPartitionedTopicMetadata(topicName));
				var opTimeoutMs = _conf.OperationTimeoutMs;
				Backoff backoff = (new BackoffBuilder()).SetInitialTime(100, TimeUnit.MILLISECONDS).SetMandatoryStop(opTimeoutMs * 2, TimeUnit.MILLISECONDS).SetMax(1, TimeUnit.MINUTES).Create();

				while (!(o is PartitionedTopicMetadata))
				{
					var e = o as ClientExceptions;
					long nextDelay = Math.Min(backoff.Next(), opTimeoutMs);
					bool isLookupThrottling = !PulsarClientException.IsRetriableError(e.Exception) || e.Exception is PulsarClientException.TooManyRequestsException || e.Exception is PulsarClientException.AuthenticationException;
					if (nextDelay <= 0 || isLookupThrottling)
					{
						throw new PulsarClientException.InvalidConfigurationException(e.Exception);
					}
					_log.Warning($"[topic: {topicName}] Could not get connection while getPartitionedTopicMetadata -- Will try again in {nextDelay} ms: {e.Exception.Message}");
					opTimeoutMs -= (int)nextDelay;
					Thread.Sleep(TimeSpan.FromMilliseconds(TimeUnit.MILLISECONDS.ToMilliseconds(nextDelay)));
					o = await _lookup.AskFor(new GetPartitionedTopicMetadata(topicName));
				}
				return o as PartitionedTopicMetadata;
			}
			catch (ArgumentException e)
			{
				throw new PulsarClientException.InvalidConfigurationException(e.Message);
			}
		}

		private async ValueTask<IList<string>> GetPartitionsForTopic(string topic)
		{
			var metadata = await GetPartitionedTopicMetadata(topic);
			if (metadata.Partitions > 0)
			{
				TopicName topicName = TopicName.Get(topic);
				IList<string> partitions = new List<string>(metadata.Partitions);
				for (int i = 0; i < metadata.Partitions; i++)
				{
					partitions.Add(topicName.GetPartition(i).ToString());
				}
				return partitions;
			}
			else
			{
				return new List<string> { topic };
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
		

	}

}