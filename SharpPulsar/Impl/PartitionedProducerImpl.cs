using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using App.Metrics.Concurrency;
using DotNetty.Common.Utilities;
using Microsoft.Extensions.Logging;
using SharpPulsar.Api;
using SharpPulsar.Common.Naming;
using SharpPulsar.Exception;
using SharpPulsar.Impl.Conf;
using SharpPulsar.Utility;
using PulsarClientException = SharpPulsar.Exceptions.PulsarClientException;

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

    public class PartitionedProducerImpl<T> : ProducerBase<T>
	{

		private static readonly ILogger Log = new LoggerFactory().CreateLogger(typeof(PartitionedProducerImpl<T>));

		private readonly IList<ProducerImpl<T>> _producers;
		private readonly IMessageRouter _routerPolicy;
		private readonly ProducerStatsRecorderImpl<T> _stats;
		private ITopicMetadata _topicMetadata;

		// timeout related to auto check and subscribe partition increasement
		public  ITimeout PartitionsAutoUpdateTimeout;
		internal TopicsPartitionChangedListener TopicPartitionChangedListener;
		internal TaskCompletionSource<Task> PartitionsAutoUpdateTask = null;

		public PartitionedProducerImpl(PulsarClientImpl client, string topic, ProducerConfigurationData conf, int numPartitions, TaskCompletionSource<IProducer<T>> producerCreatedTask, ISchema<T> schema, ProducerInterceptors interceptors) : base(client, topic, conf, producerCreatedTask, schema, interceptors)
		{
			_producers = new List<ProducerImpl<T>>(numPartitions);
			_topicMetadata = new TopicMetadataImpl(numPartitions);
			_routerPolicy = MessageRouter;
			_stats = client.Configuration.StatsIntervalSeconds > 0 ? new ProducerStatsRecorderImpl<T>() : null;

			var maxPendingMessages = Math.Min(conf.MaxPendingMessages, conf.MaxPendingMessagesAcrossPartitions / numPartitions);
			conf.MaxPendingMessages = maxPendingMessages;
			Start();

			// start track and auto subscribe partition increasement
			if (conf.AutoUpdatePartitions)
			{
				TopicPartitionChangedListener = new TopicsPartitionChangedListener(this);
				PartitionsAutoUpdateTimeout = client.Timer.NewTimeout(new TimerTaskAnonymousInnerClass(this), TimeSpan.FromMinutes(1));
			}
		}

		private IMessageRouter MessageRouter
		{
			get
            {
                var messageRouteMode = Conf.MessageRoutingMode;

                return messageRouteMode switch
                {
                    MessageRoutingMode.CustomPartition => (Conf.CustomMessageRouter ??
                                                           new RoundRobinPartitionMessageRouterImpl(Conf.HashingScheme,
                                                               ThreadLocalRandom.Next(_topicMetadata.NumPartitions()),
                                                               Conf.BatchingEnabled,
                                                               BAMCIS.Util.Concurrent.TimeUnit.MICROSECONDS.ToMillis(
                                                                   Conf
                                                                       .BatchingPartitionSwitchFrequencyIntervalMicros()))
                    ),
                    MessageRoutingMode.SinglePartition => new SinglePartitionMessageRouterImpl(
                        ThreadLocalRandom.Next(_topicMetadata.NumPartitions()), Conf.HashingScheme),
                    MessageRoutingMode.RoundRobinPartition => new RoundRobinPartitionMessageRouterImpl(
                        Conf.HashingScheme, ThreadLocalRandom.Next(_topicMetadata.NumPartitions()),
                        Conf.BatchingEnabled,
                        BAMCIS.Util.Concurrent.TimeUnit.MICROSECONDS.ToMillis(
                            Conf.BatchingPartitionSwitchFrequencyIntervalMicros())),
                    _ => new RoundRobinPartitionMessageRouterImpl(Conf.HashingScheme,
                        ThreadLocalRandom.Next(_topicMetadata.NumPartitions()), Conf.BatchingEnabled,
                        BAMCIS.Util.Concurrent.TimeUnit.MICROSECONDS.ToMillis(
                            Conf.BatchingPartitionSwitchFrequencyIntervalMicros()))
                };
            }
		}

		public override string ProducerName
        {
            get => _producers[0].ProducerName;
            set => _producers[0].ProducerName = value;
        }

        // Return the highest sequence id across all partitions. This will be correct,
        // since there is a single id generator across all partitions for the same producer

        public new long LastSequenceId => _producers.Select(p => p.LastSequenceId).DefaultIfEmpty(-1).Max();
        

        private void Start()
		{
			var createFail = new AtomicReference<System.Exception>();
			var completed = new AtomicInteger();
			for (var partitionIndex = 0; partitionIndex < _topicMetadata.NumPartitions(); partitionIndex++)
			{
				var partitionName = TopicName.Get(Topic).GetPartition(partitionIndex).ToString();
				var producer = new ProducerImpl<T>(Client, partitionName, Conf, new TaskCompletionSource<IProducer<T>>(), partitionIndex, Schema, Interceptors);
				_producers.Add(producer);
				producer.ProducerCreatedTask.Task.ContinueWith(task =>
                {
                    var createException = task.Exception;
				    if (createException != null)
				    {
					    SetState(State.Failed);
					    createFail.CompareAndSet(null, createException);
				    }
				    if (completed.Increment() == _topicMetadata.NumPartitions())
				    {
					    if (createFail.Value == null)
					    {
                            SetState(State.Ready);
						    Log.LogInformation("[{}] Created partitioned producer", Topic);
						    ProducerCreatedTask.SetResult(this);
					    }
					    else
					    {
						    Log.LogError("[{}] Could not create partitioned producer.", Topic, createFail.Value.InnerException);
						    CloseAsync().AsTask().ContinueWith(ex =>
						    {
							    ProducerCreatedTask.SetException(createFail.Value);
							    Client.CleanupProducer(this);
						    });
					    }
				    }
				});
			}

		}

		public override TaskCompletionSource<IMessageId> InternalSendAsync(IMessage<T> message)
		{
			var t = new TaskCompletionSource<IMessageId>();
			switch (GetState())
			{
			case State.Ready:
			case State.Connecting:
				break; // Ok
			case State.Closing:
			case State.Closed:
				t.SetException(new PulsarClientException.AlreadyClosedException("Producer already closed"));
				return t;
			case State.Terminated:
				t.SetException(new PulsarClientException.TopicTerminatedException("Topic was terminated"));
				return t;
			case State.Failed:
			case State.Uninitialized:
				t.SetException(new PulsarClientException.NotConnectedException());
				return t;
			}

			var partition = _routerPolicy.ChoosePartition(message, _topicMetadata);
			if(partition <= 0 && partition > _topicMetadata.NumPartitions())
                throw new ArgumentException("Illegal partition index chosen by the message routing policy: " + partition);
			return _producers[partition].InternalSendAsync(message);
		}

		public override ValueTask FlushAsync()
		{
			var flushTasks = _producers.Select(x => x.FlushAsync().AsTask()).ToList();
            var t = Task.WhenAll(flushTasks);
			return new ValueTask(t); 
        }

		public override void TriggerFlush()
		{
			_producers.ToList().ForEach(x => x.TriggerFlush());
		}

        public override bool Connected
        {
            get => _producers.All(x => x.Connected);
            set => ChangeToState(State.Closed);
        }

        public override ValueTask CloseAsync()
		{
			if (GetState() == State.Closing || GetState() == State.Closed)
			{
				return new ValueTask(null);
			}
			SetState(State.Closing);

			if (PartitionsAutoUpdateTimeout != null)
			{
				PartitionsAutoUpdateTimeout.Cancel();
				PartitionsAutoUpdateTimeout = null;
			}

			var closeFail = new AtomicReference<System.Exception>();
			var completed = new AtomicInteger(_topicMetadata.NumPartitions());
			var closeTask = new TaskCompletionSource<Task>();
			foreach (var producer in _producers)
			{
				if (producer != null)
				{
					producer.CloseAsync().AsTask().ContinueWith(task =>
                    {
                        var ex = task.Exception;
					    if (ex != null)
					    {
						    closeFail.CompareAndSet(null, ex);
					    }
					    if (completed.Decrement() == 0)
					    {
						    if (closeFail.Value == null)
						    {
							    SetState(State.Closed);
							    closeTask.SetResult(null);
							    Log.LogInformation("[{}] Closed Partitioned Producer", Topic);
							    Client.CleanupProducer(this);
						    }
						    else
						    {
							    SetState(State.Failed);
							    closeTask.SetException(closeFail.Value);
							    Log.LogError("[{}] Could not close Partitioned Producer", Topic, closeFail.Value.InnerException);
						    }
					    }
					    return;
					});
				}

			}

			return new ValueTask(closeTask.Task);
		}

		public new ProducerStatsRecorderImpl<T> Stats
		{
			get
			{
				lock (this)
				{
					if (_stats == null)
					{
						return null;
					}
					_stats.Reset();
					for (var i = 0; i < _topicMetadata.NumPartitions(); i++)
					{
						_stats.UpdateCumulativeStats(_producers[i].Stats);
					}
					return _stats;
				}
			}
		}

		public virtual IList<ProducerImpl<T>> Producers => _producers.ToList();

        public new string HandlerName => "partition-producer";

        // This listener is triggered when topics partitions are updated.
		public class TopicsPartitionChangedListener : PartitionsChangedListener
		{
			private readonly PartitionedProducerImpl<T> _outerInstance;

			public TopicsPartitionChangedListener(PartitionedProducerImpl<T> outerInstance)
			{
				_outerInstance = outerInstance;
			}

			// Check partitions changes of passed in topics, and add new topic partitions.
			public TaskCompletionSource<Task> OnTopicsExtended(ICollection<string> topicsExtended)
			{
				var task = new TaskCompletionSource<Task>();
				if (topicsExtended.Count == 0 || !topicsExtended.Contains(_outerInstance.Topic))
				{
					task.SetResult(null);
					return task;
				}

				_outerInstance.Client.GetPartitionsForTopic(_outerInstance.Topic).AsTask().ContinueWith(tasks =>
                {
                    var list = tasks.Result.ToList();

					var oldPartitionNumber = _outerInstance._topicMetadata.NumPartitions();
				    var currentPartitionNumber = list.Count;
				    if (Log.IsEnabled(LogLevel.Debug))
				    {
					    Log.LogDebug("[{}] partitions number. old: {}, new: {}", _outerInstance.Topic, oldPartitionNumber, currentPartitionNumber);
				    }
				    if (oldPartitionNumber == currentPartitionNumber)
				    {
					    task.SetResult(null);
				    }
				    else if (oldPartitionNumber < currentPartitionNumber)
				    {
					    IList<TaskCompletionSource<IProducer<T>>> taskList = list.Skip(oldPartitionNumber).Take(oldPartitionNumber-currentPartitionNumber).Select(partitionName =>
					    {
						    var partitionIndex = TopicName.GetPartitionIndex(partitionName);
						    var producer = new ProducerImpl<T>(_outerInstance.Client, partitionName, _outerInstance.Conf, new TaskCompletionSource<IProducer<T>>(), partitionIndex, _outerInstance.Schema, _outerInstance.Interceptors);
						    _outerInstance._producers.Add(producer);
						    return producer.ProducerCreatedTask;
					    }).ToList();
					    Task.WhenAll(taskList.Select(x=> x.Task)).ContinueWith(finalTask =>
					    {
                            if (finalTask.IsFaulted)
                            {
                                Log.LogWarning("[{}] fail create producers for extended partitions. old: {}, new: {}", _outerInstance.Topic, oldPartitionNumber, currentPartitionNumber);
                                var sublist = _outerInstance._producers.Skip(oldPartitionNumber).Take(_outerInstance._producers.Count - oldPartitionNumber).ToList();
                                sublist.ForEach(newProducer => newProducer.CloseAsync());
                                sublist.Clear();
                                task.SetException(finalTask.Exception ?? throw new InvalidOperationException());
                                return;
							}
						    if (Log.IsEnabled(LogLevel.Debug))
						    {
							    Log.LogDebug("[{}] success create producers for extended partitions. old: {}, new: {}", _outerInstance.Topic, oldPartitionNumber, currentPartitionNumber);
						    }
						    _outerInstance._topicMetadata = new TopicMetadataImpl(currentPartitionNumber);
						    task.SetResult(null);
					    });
				    }
				    else
				    {
					    Log.LogError("[{}] not support shrink topic partitions. old: {}, new: {}", _outerInstance.Topic, oldPartitionNumber, currentPartitionNumber);
					    task.SetException(new NotSupportedException("not support shrink topic partitions"));
				    }
				    return task;
				});

				return task;
			}
		}
		
		public class TimerTaskAnonymousInnerClass : ITimerTask
        {
            private readonly PartitionedProducerImpl<T> _outerInstance;

            public TimerTaskAnonymousInnerClass(PartitionedProducerImpl<T> outerInstance)
            {
                _outerInstance = outerInstance;
            }

            public void Run(ITimeout timeout)
			{
				if (timeout.Canceled || _outerInstance.GetState() != State.Ready)
				{
					return;
				}

				if (Log.IsEnabled(LogLevel.Debug))
				{
					Log.LogDebug("[{}] run partitionsAutoUpdateTimerTask for partitioned producer", _outerInstance.Topic);
				}

				// if last auto update not completed yet, do nothing.
				if (_outerInstance.PartitionsAutoUpdateTask == null || _outerInstance.PartitionsAutoUpdateTask.Task.IsCompleted)
                {
                    var t = new TopicsPartitionChangedListener(_outerInstance);
					_outerInstance.PartitionsAutoUpdateTask = t.OnTopicsExtended(new List<string>{_outerInstance.Topic});
				}

				// schedule the next re-check task
				_outerInstance.PartitionsAutoUpdateTimeout = _outerInstance.Client.Timer.NewTimeout(new TimerTaskAnonymousInnerClass(_outerInstance), TimeSpan.FromMinutes(1));
			}
		}

	}

}