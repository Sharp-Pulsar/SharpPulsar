using Akka.Actor;
using Akka.Event;
using Akka.Routing;
using Akka.Util.Internal;
using SharpPulsar.Common;
using SharpPulsar.Common.Naming;
using SharpPulsar.Common.Partition;
using SharpPulsar.Configuration;
using SharpPulsar.Exceptions;
using SharpPulsar.Interfaces;
using SharpPulsar.Messages.Client;
using SharpPulsar.Messages.Consumer;
using SharpPulsar.Messages.Producer;
using SharpPulsar.Messages.Requests;
using SharpPulsar.Messages.Transaction;
using SharpPulsar.Stats.Producer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using Akka.Util;
using SharpPulsar.Precondition;
using SharpPulsar.Common.Util;

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

        private readonly ConcurrentDictionary<int, IActorRef> _producers;
		private readonly IActorRef _router;
		private readonly IActorRef _generator;
		private readonly IActorRef _self;
		private readonly IActorRef _lookup;
		private readonly IActorRef _cnxPool;
		private readonly ProducerStatsRecorder _stats;
		private TopicMetadata _topicMetadata;
        
        // timeout related to auto check and subscribe partition increasement
        private  ICancelable _partitionsAutoUpdateTimeout = null;
        private ValueTask _partitionsAutoUpdateFuture;
        private readonly ILoggingAdapter _log;
		private readonly IActorContext _context;

        private readonly int _firstPartitionIndex;
        private string _overrideProducerName;
        internal TopicsPartitionChangedListener _topicsPartitionChangedListener;

        public PartitionedProducer(IActorRef client, IActorRef lookup, IActorRef cnxPool, IActorRef idGenerator, string topic, ProducerConfigurationData conf, int numPartitions, ISchema<T> schema, ProducerInterceptors<T> interceptors, ClientConfigurationData clientConfiguration, TaskCompletionSource<IActorRef> producerCreatedFuture) : base(client, lookup, cnxPool, topic, conf, producerCreatedFuture, schema, interceptors, clientConfiguration)
		{
			_cnxPool = cnxPool;
			_lookup = lookup;
			_self = Self;
			_producers = new ConcurrentDictionary<int, IActorRef>();
			_generator = idGenerator;
			_context = Context;
			_topicMetadata = new TopicMetadata(numPartitions);
			_stats = clientConfiguration.StatsIntervalSeconds > TimeSpan.Zero ? new ProducerStatsRecorder(Context.System, "PartitionedProducer", topic, conf.MaxPendingMessages) : null;
			_log = Context.GetLogger();
			var maxPendingMessages = Math.Min(conf.MaxPendingMessages, conf.MaxPendingMessagesAcrossPartitions / numPartitions);
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
            
            IList<int> indexList;
            if (conf.LazyStartPartitionedProducers && conf.AccessMode == ProducerAccessMode.Shared)
            {
                // try to create producer at least one partition
                indexList = Collections.singletonList(routerPolicy.ChoosePartition(((TypedMessageBuilder<T>)NewMessage()).getMessage(), topicMetadata));
                NewMessage();
            }
            else
            {
                // try to create producer for all partitions
                indexList = Enumerable.Range(0, _topicMetadata.NumPartitions()).Select(x => x * x).ToList();
            }

            _firstPartitionIndex = indexList[0];
                        
            
			
			// start track and auto subscribe partition increasement
			if(conf.AutoUpdatePartitions)
			{
                _topicsPartitionChangedListener = new TopicsPartitionChangedListener(this);
                _partitionsAutoUpdateTimeout = _context.System.Scheduler.ScheduleTellOnceCancelable(TimeSpan.FromSeconds(conf.AutoUpdatePartitionsIntervalSeconds), Self,  ExtendTopics.Instance, ActorRefs.NoSender);
			}
            Receive<ExtendTopics>(_ =>
            {
                Run();
            });
            Receive<Flush>(_ => {
				Flush();
			});
			Receive<TriggerFlush>(_ => {
				TriggerFlush();
			});
			ReceiveAsync<InternalSend<T>>(async m =>
			{
				try
				{
					//get excepyion vai out
					await InternalSend(m.Message, m.Callback);
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
					InternalSendWithTxn(m.Message, m.Txn, m.Callback);
				}
				catch (Exception ex)
				{
                    Sender.Tell(ex);
					_log.Error(ex.ToString());
				}
			});
			ReceiveAny(any => _router.Forward(any));
            Akka.Dispatch.ActorTaskScheduler.RunTask(async()=> await Start(indexList));
		}
		public static Props Prop(IActorRef client, IActorRef lookup, IActorRef cnxPool, IActorRef idGenerator, string topic, ProducerConfigurationData conf, int numPartitions, ISchema<T> schema, ProducerInterceptors<T> interceptors, ClientConfigurationData clientConfiguration, TaskCompletionSource<IActorRef> producerCreatedFuture)
        {
            return Props.Create(()=> new PartitionedProducer<T>(client, lookup, cnxPool, idGenerator, topic, conf, numPartitions, schema, interceptors, clientConfiguration, producerCreatedFuture));
        }
        protected internal override async ValueTask<string> ProducerName()
		{
			//return await _producers[0].Ask<string>(GetProducerName.Instance);
			return await Task.FromResult("PartitionedProducer");
		}

		protected internal override async ValueTask<long> LastSequenceId()
		{
			// Return the highest sequence id across all partitions. This will be correct,
			// since there is a single id generator across all partitions for the same producer

			return await _producers.Values.Max(x => x.Ask<long>(GetLastSequenceId.Instance));
		}

		private async ValueTask Start(IList<int> indexList)
		{
			Exception createFail = null;
			var completed = 0;
            string overrideProducerName = null ;
            foreach (var partitionIndex in indexList)
            {
                var tcs = new TaskCompletionSource<IActorRef>(TaskCreationOptions.RunContinuationsAsynchronously);
                var producerId = await _generator.Ask<long>(NewProducerId.Instance);
                if (overrideProducerName == null)
                {
                    var id = $"{producerId}";
                    _overrideProducerName = id;
                    overrideProducerName = id;
                } 
                var partitionName = TopicName.Get(Topic).GetPartition(partitionIndex).ToString();
                _context.ActorOf(ProducerActor<T>.Prop(producerId, Client, _lookup, _cnxPool, _generator, partitionName, Conf, tcs, partitionIndex, Schema, Interceptors, ClientConfiguration, _overrideProducerName));
                try
                {
                    var producer = await tcs.Task;
                    _producers.TryAdd((int)producerId, producer);
                    var routee = Routee.FromActorRef(producer);
                    _router.Tell(new AddRoutee(routee));
                }
                catch (Exception ex)
                {
                    State.ConnectionState = HandlerState.State.Failed;
                    createFail = ex;
                }

                if (++completed == _topicMetadata.NumPartitions())
                {
                    if (createFail == null)
                    {
                        State.ConnectionState = HandlerState.State.Ready;
                        _log.Info($"[{Topic}] Created partitioned producer");
                        ProducerCreatedFuture.TrySetResult(_self);
                    }
                    else
                    {
                        _log.Error($"[{Topic}] Could not create partitioned producer: {createFail}");
                        ProducerCreatedFuture.TrySetException(createFail);
                        Client.Tell(new CleanupProducer(_self));
                    }
                }
                

			}

		}
        
        internal override async ValueTask InternalSend(IMessage<T> message, TaskCompletionSource<Message<T>> callback)
		{
			switch (State.ConnectionState)
			{
                case HandlerState.State.Ready:
                case HandlerState.State.Connecting:
                    break; // Ok
                case HandlerState.State.Closing:
                case HandlerState.State.Closed:
                    callback.TrySetException(new PulsarClientException.AlreadyClosedException("Producer already closed"));
                    return;
                case HandlerState.State.Terminated:
                    callback.TrySetException(new PulsarClientException.TopicTerminatedException("Topic was terminated"));
                    return;
                case HandlerState.State.ProducerFenced:
                    callback.TrySetException(new PulsarClientException.ProducerFencedException("Producer was fenced"));
                    return;
                case HandlerState.State.Failed:
                case HandlerState.State.Uninitialized:
                    callback.TrySetException(new PulsarClientException.NotConnectedException());
                    return;
            }

			if (Conf.MessageRoutingMode == MessageRoutingMode.ConsistentHashingMode)
			{
				var msg = new ConsistentHashableEnvelope(new InternalSend<T>(message, callback), message.Key);
				_router.Tell(msg, Sender);
			}
			else
			{
				_router.Tell(new InternalSend<T>(message, callback), Sender);
			}

            await Task.CompletedTask;
        }

        private async ValueTask InternalSendWithTxn(IMessage<T> message, IActorRef txn, TaskCompletionSource<Message<T>> callback)
		{
            
            var partition = ChoosePartition(message, _topicMetadata);
            
            Condition.CheckArgument(partition >= 0 && partition < _topicMetadata.NumPartitions(), "Illegal partition index chosen by the message routing policy: " + partition);
            Exception createFail = null;

            if (Conf.LazyStartPartitionedProducers && !_producers.ContainsKey(partition)) 
            {
                var tcs = new TaskCompletionSource<IActorRef>(TaskCreationOptions.RunContinuationsAsynchronously);
                var producerId = await _generator.Ask<long>(NewProducerId.Instance);
                var partitionName = TopicName.Get(Topic).GetPartition(partition).ToString();
                _context.ActorOf(ProducerActor<T>.Prop(producerId, Client, _lookup, _cnxPool, _generator, partitionName, Conf, tcs, partition, Schema, Interceptors, ClientConfiguration, _overrideProducerName));
                try
                {
                    var producer = await tcs.Task;
                    _producers.TryAdd((int)producerId, producer);
                    var routee = Routee.FromActorRef(producer);
                    _router.Tell(new AddRoutee(routee));
                }
                catch (Exception ex)
                {
                    State.ConnectionState = HandlerState.State.Failed;
                    createFail = ex;
                }

                var newProducer = CreateProducer(Partition, Optional.ofNullable(overrideProducerName));
                // JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
                // ORIGINAL LINE: final State createState = newProducer.producerCreatedFuture().handle((prod, createException) ->
                State CreateState = NewProducer.ProducerCreatedFuture().handle((prod, createException) =>
                {
                    if (createException != null)
                    {
                        log.error("[{}] Could not create internal producer. partitionIndex: {}", Topic, Partition, createException);
                        try
                        {
                            producers.Remove(Partition, NewProducer);
                            NewProducer.Dispose();
                        }
                        catch (PulsarClientException E)
                        {
                            log.error("[{}] Could not close internal producer. partitionIndex: {}", Topic, Partition, E);
                        }
                        return State.Failed;
                    }
                    if (log.isDebugEnabled())
                    {
                        log.debug("[{}] Created internal producer. partitionIndex: {}", Topic, Partition);
                    }
                    return State.Ready;
                }).join();
                if (CreateState == State.Failed)
                {
                    return FutureUtil.FailedFuture(new PulsarClientException.NotConnectedException());
                }
            }
                        
            switch (State.ConnectionState)
            {
                case HandlerState.State.Ready:
                case HandlerState.State.Connecting:
                    break; // Ok
                case HandlerState.State.Closing:
                case HandlerState.State.Closed:
                    callback.TrySetException(new PulsarClientException.AlreadyClosedException("Producer already closed"));
                    return;
                case HandlerState.State.Terminated:
                    callback.TrySetException(new PulsarClientException.TopicTerminatedException("Topic was terminated"));
                    return;
                case HandlerState.State.ProducerFenced:
                    callback.TrySetException(new PulsarClientException.ProducerFencedException("Producer was fenced"));
                    return;
                case HandlerState.State.Failed:
                case HandlerState.State.Uninitialized:
                    callback.TrySetException(new PulsarClientException.NotConnectedException());
                    return;
            }

            if (Conf.MessageRoutingMode == MessageRoutingMode.ConsistentHashingMode)
            {
                var msg = new ConsistentHashableEnvelope(new InternalSendWithTxn<T>(message, txn, callback), message.Key);
                _router.Tell(msg, Sender);
            }
            else
            {
                _router.Tell(new InternalSendWithTxn<T>(message, txn, callback), Sender);
            }
        }
        private int ChoosePartition(IMessage<T> msg, TopicMetadata metadata)
        {
            // If the message has a key, it supersedes the single partition routing policy
            if (msg.HasKey())
            {
                return MathUtils.SignSafeMod(msg.Key.GetHashCode(), metadata.NumPartitions());
            }

            return _firstPartitionIndex; 
        }
        private void Flush()
		{
            _producers.Values.ForEach(x => x.Tell(Messages.Producer.Flush.Instance));
		}

		private void TriggerFlush()
		{
			_producers.Values.ForEach(x => x.Tell(Messages.Producer.TriggerFlush.Instance));
		}

		protected internal override bool Connected()
		{
			foreach(var p in _producers.Values)
            {
				var x = p.Ask<bool>(IsConnected.Instance).GetAwaiter().GetResult();
				if (!x)
					return false;

			}
			return true;
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
            foreach (var p in _producers.Values)
            {
                var stats = await p.Ask<IProducerStats>(GetStats.Instance);
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
        internal class TopicsPartitionChangedListener : IPartitionsChangedListener
        {
            private readonly PartitionedProducer<T> _outerInstance;

            public TopicsPartitionChangedListener(PartitionedProducer<T> outerInstance)
            {
                _outerInstance = outerInstance;
            }

            // Check partitions changes of passed in topics, and add new topic partitions.
            public async ValueTask OnTopicsExtended(ICollection<string> topicsExtended)
            {
                
                if (topicsExtended.Count == 0 || !topicsExtended.Contains(_outerInstance.Topic))
                {                    
                    return;
                }
                var topicName = TopicName.Get(_outerInstance.Topic);

                var result = await _outerInstance._lookup.Ask<AskResponse>(new GetPartitionedTopicMetadata(topicName));

                if (result.Failed)
                {
                    _outerInstance._log.Error($"[{_outerInstance.Topic}] Auto getting partitions failed");
                    
                    throw result.Exception; 
                }

                var metadata = result.ConvertTo<PartitionedTopicMetadata>();
                var list = _outerInstance.GetPartitionsForTopic(topicName, metadata).ToList();
                var oldPartitionNumber = _outerInstance._topicMetadata.NumPartitions();
                var currentPartitionNumber = list.Count;
                if (_outerInstance._log.IsDebugEnabled)
                {
                    _outerInstance._log.Debug($"[{_outerInstance.Topic}] partitions number. old: {oldPartitionNumber}, new: {currentPartitionNumber}");
                }
                try 
                {
                    if (oldPartitionNumber == currentPartitionNumber)
                    {
                        return;
                    }
                    else if (oldPartitionNumber < currentPartitionNumber)
                    {
                        if (_outerInstance.Conf.LazyStartPartitionedProducers && _outerInstance.Conf.AccessMode == ProducerAccessMode.Shared)
                        {
                            _outerInstance._topicMetadata = new TopicMetadata(currentPartitionNumber);

                            _outerInstance.OnPartitionsChange(_outerInstance.Topic, currentPartitionNumber);
                            return;
                        }
                        else
                        {
                            var newPartitions = list.GetRange(oldPartitionNumber, currentPartitionNumber);
                            foreach (var partitionName in newPartitions)
                            {
                                var tcs = new TaskCompletionSource<IActorRef>(TaskCreationOptions.RunContinuationsAsynchronously);
                                var producerId = await _outerInstance._generator.Ask<long>(NewProducerId.Instance);
                                var partitionIndex = TopicName.GetPartitionIndex(partitionName);
                                _outerInstance._context.ActorOf(ProducerActor<T>.Prop(producerId, _outerInstance.Client, _outerInstance._lookup, _outerInstance._cnxPool, _outerInstance._generator, partitionName, _outerInstance.Conf, tcs, partitionIndex, _outerInstance.Schema, _outerInstance.Interceptors, _outerInstance.ClientConfiguration, _outerInstance._overrideProducerName));
                                try
                                {
                                    var producer = await tcs.Task;
                                    _outerInstance._producers.TryAdd(partitionIndex, producer);
                                    var routee = Routee.FromActorRef(producer);
                                    _outerInstance._router.Tell(new AddRoutee(routee));
                                    if (_outerInstance._log.IsDebugEnabled)
                                    {
                                        _outerInstance._log.Debug($"[{_outerInstance.Topic}] success create producers for extended partitions. old: {oldPartitionNumber}, new: {currentPartitionNumber}");
                                    }
                                    _outerInstance._topicMetadata = new TopicMetadata(currentPartitionNumber);
                                }
                                catch (Exception ex)
                                {
                                    _outerInstance._log.Warning($"[{_outerInstance.Topic}] fail create producers for extended partitions. old: {oldPartitionNumber}, new: {currentPartitionNumber}");
                                    Enumerable.Range(oldPartitionNumber, ((int)_outerInstance._producers.Count) - oldPartitionNumber)
                                        .ForEach( i =>  _outerInstance._producers.Remove(i, out var a));

                                    _outerInstance._log.Error(ex.ToString());
                                }
                            }

                            _outerInstance.OnPartitionsChange(_outerInstance.Topic, currentPartitionNumber);

                        }
                    }
                    else
                    {
                        _outerInstance._log.Error($"[{_outerInstance.Topic}] not support shrink topic partitions. old: {oldPartitionNumber}, new: {currentPartitionNumber}");
                        throw new PulsarClientException.NotSupportedException("not support shrink topic partitions");
                    }
                }
                catch(Exception ex) 
                {
                    _outerInstance._log.Error($"[{_outerInstance.Topic}] Auto getting partitions failed { ex}");
                    throw ex;
                }    
                
                
            }
        }
        
		private IList<string> GetPartitionsForTopic(TopicName topicName, PartitionedTopicMetadata metadata)
		{
			if (metadata.Partitions > 0)
			{
				IList<string> partitions = new List<string>(metadata.Partitions);
				for (var i = 0; i < metadata.Partitions; i++)
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
        internal void Run()
        {
            try
            {
                if (_log.IsDebugEnabled)
                {
                    _log.Debug($"[{Topic}] run partitionsAutoUpdateTimerTask for partitioned producer");
                }

                // if last auto update not completed yet, do nothing.
                if (_partitionsAutoUpdateFuture == null || _partitionsAutoUpdateFuture.IsCompleted)
                {
                    _partitionsAutoUpdateFuture = _topicsPartitionChangedListener.OnTopicsExtended(new List<string> { Topic });
                }
            }
            catch (Exception ex)
            {
                _log.Warning($"Encountered error in partition auto update timer task for partition producer. Another task will be scheduled. {ex}");
            }
            finally
            {
                // schedule the next re-check task
                _partitionsAutoUpdateTimeout = _context.System.Scheduler.ScheduleTellOnceCancelable(TimeSpan.FromSeconds(Conf.AutoUpdatePartitionsIntervalSeconds), Self, ExtendTopics.Instance, ActorRefs.NoSender);
            }
        }
        protected internal override long LastDisconnectedTimestamp()
        {
            var lastDisconnectedTimestamp = 0L;
            
            //var p = _producers.Values.Max(x => x.Ask<long>(GetLastDisconnectedTimestamp.Instance).GetAwaiter().GetType());
            foreach (var c in _producers.Values)
            {
                var x = c.Ask<long>(GetLastDisconnectedTimestamp.Instance).GetAwaiter().GetResult();

                if (x > lastDisconnectedTimestamp)
                    lastDisconnectedTimestamp = x;
            }
            return lastDisconnectedTimestamp;
        }

        internal sealed class ExtendTopics
        {
			public static ExtendTopics Instance = new ExtendTopics();
        }
	}

}