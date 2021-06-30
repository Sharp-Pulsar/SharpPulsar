using Akka.Actor;
using Akka.Event;
using Akka.Routing;
using Akka.Util.Internal;
using SharpPulsar.Common;
using SharpPulsar.Common.Naming;
using SharpPulsar.Common.Partition;
using SharpPulsar.Configuration;
using SharpPulsar.Interfaces;
using SharpPulsar.Messages.Client;
using SharpPulsar.Messages.Consumer;
using SharpPulsar.Messages.Producer;
using SharpPulsar.Messages.Requests;
using SharpPulsar.Messages.Transaction;
using SharpPulsar.Queues;
using SharpPulsar.Stats.Producer;
using System;
using System.Collections.Generic;
using System.Linq;
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
    internal class PartitionedProducer<T> : ProducerActorBase<T>
	{
		private IList<IActorRef> _producers;
		private IMessageRouter _routerPolicy;
		private readonly IActorRef _router;
		private readonly IActorRef _generator;
		private readonly IActorRef _self;
		private readonly IActorRef _lookup;
		private readonly IActorRef _cnxPool;
		private readonly ProducerStatsRecorder _stats;
		private TopicMetadata _topicMetadata;

		// timeout related to auto check and subscribe partition increasement
		private volatile ICancelable _partitionsAutoUpdateTimeout = null;
		private readonly ILoggingAdapter _log;
		private readonly IActorContext _context;

		public PartitionedProducer(IActorRef client, IActorRef lookup, IActorRef cnxPool, IActorRef idGenerator, string topic, ProducerConfigurationData conf, int numPartitions, ISchema<T> schema, ProducerInterceptors<T> interceptors, ClientConfigurationData clientConfiguration, ProducerQueueCollection<T> queue) : base(client, lookup, cnxPool, topic, conf, schema, interceptors, clientConfiguration, queue)
		{
			_cnxPool = cnxPool;
			_lookup = lookup;
			_self = Self;
			_producers = new List<IActorRef>();
			_generator = idGenerator;
			_context = Context;
			_producers = new List<IActorRef>(numPartitions);
			_topicMetadata = new TopicMetadata(numPartitions);
			_stats = clientConfiguration.StatsIntervalSeconds > 0 ? new ProducerStatsRecorder(Context.System, "PartitionedProducer", topic, conf.MaxPendingMessages) : null;
			_log = Context.GetLogger();
			int maxPendingMessages = Math.Min(conf.MaxPendingMessages, conf.MaxPendingMessagesAcrossPartitions / numPartitions);
			conf.MaxPendingMessages = maxPendingMessages;

			switch (conf.MessageRoutingMode)
			{
				case MessageRoutingMode.ConsistentHashingMode:
					_router = Context.System.ActorOf(Props.Empty.WithRouter(new ConsistentHashingGroup()), $"Partition{DateTimeHelper.CurrentUnixTimeMillis()}");
					break;
				case MessageRoutingMode.BroadcastMode:
					_router = Context.System.ActorOf(Props.Empty.WithRouter(new BroadcastGroup()), $"Partition{DateTimeHelper.CurrentUnixTimeMillis()}");
					break;
				case MessageRoutingMode.RandomMode:
					_router = Context.System.ActorOf(Props.Empty.WithRouter(new RandomGroup()), $"Partition{DateTimeHelper.CurrentUnixTimeMillis()}");
					break;
				default:
					_router = Context.System.ActorOf(Props.Empty.WithRouter(new RoundRobinGroup()), $"Partition{DateTimeHelper.CurrentUnixTimeMillis()}");
					break;
			}
			Start().ConfigureAwait(false);

			// start track and auto subscribe partition increasement
			if(conf.AutoUpdatePartitions)
			{
				_partitionsAutoUpdateTimeout = _context.System.Scheduler.ScheduleTellRepeatedlyCancelable(TimeSpan.FromSeconds(conf.AutoUpdatePartitionsIntervalSeconds), TimeSpan.FromSeconds(conf.AutoUpdatePartitionsIntervalSeconds), Self, ExtendTopics.Instance, ActorRefs.NoSender);
			}
			Receive<Flush>(_ => {
				Flush();
			});
			Receive<TriggerFlush>(_ => {
				TriggerFlush();
			});
			ReceiveAsync<ExtendTopics>(async _ => 
			{
				var t = topic;
				await OnTopicsExtended(new List<string> { t });
			});

			Receive<InternalSend<T>>(m =>
			{
				try
				{
					//get excepyion vai out
					InternalSend(m.Message);
				}
				catch (Exception ex)
				{
					_log.Error(ex.ToString());
				}
			});
			Receive<InternalSendWithTxn<T>>(m =>
			{
				try
				{
					InternalSendWithTxn(m.Message, m.Txn);
				}
				catch (Exception ex)
				{
					_log.Error(ex.ToString());
				}
			});
			ReceiveAny(any => _router.Forward(any));
		}
		protected internal override async ValueTask<string> ProducerName()
		{
			//return await _producers[0].Ask<string>(GetProducerName.Instance);
			return "PartitionedProducer";
		}

		protected internal override async ValueTask<long> LastSequenceId()
		{
			// Return the highest sequence id across all partitions. This will be correct,
			// since there is a single id generator across all partitions for the same producer

			return await _producers.Max(x => x.Ask<long>(GetLastSequenceId.Instance));
		}

		private async ValueTask Start()
		{
			Exception createFail = null;
			int completed = 0;
			for(int partitionIndex = 0; partitionIndex < _topicMetadata.NumPartitions(); partitionIndex++)
			{
				var producerId = await _generator.Ask<long>(NewProducerId.Instance);
				string partitionName = TopicName.Get(Topic).GetPartition(partitionIndex).ToString();
				var producer = _context.ActorOf(Props.Create(()=> new ProducerActor<T>(producerId, Client, _lookup, _cnxPool, _generator, partitionName, Conf, partitionIndex, Schema, Interceptors, ClientConfiguration, ProducerQueue)));
				_producers.Add(producer);
				var routee = Routee.FromActorRef(producer);
				_router.Tell(new AddRoutee(routee));
				var prod = ProducerQueue.Producer.Take();
				if(prod.Errored)
                {
					State.ConnectionState = HandlerState.State.Failed;
					createFail = prod.Exception;
				}
				if (++completed == _topicMetadata.NumPartitions())
				{
					if (createFail == null)
					{
						State.ConnectionState = HandlerState.State.Ready;
						_log.Info($"[{Topic}] Created partitioned producer");
						ProducerQueue.PartitionedProducer.Add(_self);
					}
					else
					{
						_log.Error($"[{Topic}] Could not create partitioned producer: {createFail}");
						ProducerQueue.PartitionedProducer.Add(null);
						Client.Tell(new CleanupProducer(_self));
					}
				}
			}

		}

		internal override void InternalSend(IMessage<T> message)
		{
			switch (State.ConnectionState)
			{
				case HandlerState.State.Ready:
				case HandlerState.State.Connecting:
					break; // Ok
					goto case HandlerState.State.Closing;
				case HandlerState.State.Closing:
				case HandlerState.State.Closed:
					_log.Error("Producer already closed");
					break;
				case HandlerState.State.Terminated:
					_log.Error("Topic was terminated");
					break;
				case HandlerState.State.Failed:
				case HandlerState.State.Uninitialized:
					_log.Error("NotConnectedException"); break;
			}

			if (Conf.MessageRoutingMode == MessageRoutingMode.ConsistentHashingMode)
			{
				var msg = new ConsistentHashableEnvelope(new InternalSend<T>(message), message.Key);
				_router.Tell(msg);
			}
			else
			{
				_router.Tell(new InternalSend<T>(message));
			}
		}

		private void InternalSendWithTxn(IMessage<T> message, IActorRef txn)
		{
			switch(State.ConnectionState)
			{
				case HandlerState.State.Ready:
				case HandlerState.State.Connecting:
					break; // Ok
					goto case HandlerState.State.Closing;
				case HandlerState.State.Closing:
				case HandlerState.State.Closed:
					 _log.Error("Producer already closed");
					break;
				case HandlerState.State.Terminated:
					_log.Error("Topic was terminated");
					break;
				case HandlerState.State.Failed:
				case HandlerState.State.Uninitialized:
					_log.Error("NotConnectedException");break;
			}

			if (Conf.MessageRoutingMode == MessageRoutingMode.ConsistentHashingMode)
			{
				var msg = new ConsistentHashableEnvelope(new InternalSendWithTxn<T>(message, txn), message.Key);
				_router.Tell(msg);
			}
            else
			{
				_router.Tell(new InternalSendWithTxn<T>(message, txn));
			}
		}

		private void Flush()
		{
			 _producers.ForEach(x => x.Tell(Messages.Producer.Flush.Instance));
		}

		private void TriggerFlush()
		{
			_producers.ForEach(x => x.Tell(Messages.Producer.TriggerFlush.Instance));
		}

		protected internal override bool Connected()
		{
			foreach(var p in _producers)
            {
				var x = p.Ask<bool>(IsConnected.Instance).GetAwaiter().GetResult();
				if (!x)
					return false;

			}
			return true;
		}

		private LastConnectionClosedTimestampResponse DisconnectedTimestamp()
		{
			LastConnectionClosedTimestampResponse lastDisconnectedTimestamp = null;
			foreach(var pr in _producers)
            {
				var x = pr.Ask<LastConnectionClosedTimestampResponse>(GetLastDisconnectedTimestamp.Instance).GetAwaiter().GetResult();
				if (lastDisconnectedTimestamp == null)
					lastDisconnectedTimestamp = x;
				if (x?.TimeStamp > lastDisconnectedTimestamp.TimeStamp)
					lastDisconnectedTimestamp = x;
			}
			return lastDisconnectedTimestamp;
		}
        protected override void PostStop()
        {
			_partitionsAutoUpdateTimeout?.Cancel();
			_producers.ForEach(x => 
			{
				Client.Tell(new CleanupProducer(x));
			});
			base.PostStop();
        }

		protected internal override async ValueTask<IProducerStats> Stats()
		{
			if (_stats == null)
			{
				return null;
			}
			_stats.Reset();
			for (int i = 0; i < _topicMetadata.NumPartitions(); i++)
			{
				var stats = await _producers[i].Ask<IProducerStats>(GetStats.Instance);
				_stats.UpdateCumulativeStats(stats);
			}
			return _stats;
		}

		internal string HandlerName
		{
			get
			{
				return "partition-producer";
			}
		}
		private async ValueTask OnTopicsExtended(ICollection<string> topicsExtended)
		{
			if (topicsExtended.Count == 0 || !topicsExtended.Contains(Topic))
			{
				return;
			}
			TopicName topicName = TopicName.Get(Topic);
			var metadata = await _lookup.Ask<PartitionedTopicMetadata>(new GetPartitionedTopicMetadata(topicName));
			var topics = GetPartitionsForTopic(topicName, metadata).ToList();
			int oldPartitionNumber = _topicMetadata.NumPartitions();
			int currentPartitionNumber = topics.Count;
			if (_log.IsDebugEnabled)
			{
				_log.Debug($"[{Topic}] partitions number. old: {oldPartitionNumber}, new: {currentPartitionNumber}");
			}
			if (oldPartitionNumber == currentPartitionNumber)
			{
				return;
			}
			else if (oldPartitionNumber < currentPartitionNumber)
			{
				var newPartitions = topics.GetRange(oldPartitionNumber, currentPartitionNumber);
				foreach (var partitionName in newPartitions)
				{
					var producerId = await _generator.Ask<long>(NewProducerId.Instance);
					int partitionIndex = TopicName.GetPartitionIndex(partitionName);
					var producer = _context.ActorOf(Props.Create(()=> new ProducerActor<T>(producerId, Client, _lookup, _cnxPool, _generator, partitionName, Conf, partitionIndex, Schema, Interceptors, ClientConfiguration, ProducerQueue)));
					_producers.Add(producer);
					var routee = Routee.FromActorRef(producer);
					_router.Tell(new AddRoutee(routee));
				}
				if (_log.IsDebugEnabled)
				{
					_log.Debug($"[{Topic}] success create producers for extended partitions. old: {oldPartitionNumber}, new: {currentPartitionNumber}");
				}
				_topicMetadata = new TopicMetadata(currentPartitionNumber);
			}
			else
			{
				_log.Error($"[{Topic}] not support shrink topic partitions. old: {oldPartitionNumber}, new: {currentPartitionNumber}");
			}
		}

		private IList<string> GetPartitionsForTopic(TopicName topicName, PartitionedTopicMetadata metadata)
		{
			if (metadata.Partitions > 0)
			{
				IList<string> partitions = new List<string>(metadata.Partitions);
				for (int i = 0; i < metadata.Partitions; i++)
				{
					partitions.Add(topicName.GetPartition(i).ToString());
				}
				return partitions;
			}
			else
			{
				return new List<string> { topicName.ToString() };
			}
		}

        protected internal override void LastDisconnectedTimestamp()
        {
            throw new NotImplementedException();
        }

        internal sealed class ExtendTopics
        {
			public static ExtendTopics Instance = new ExtendTopics();
        }
	}

}