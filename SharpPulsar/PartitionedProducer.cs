using System;
using System.Collections.Generic;

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
	public class PartitionedProducer<T> : ProducerBase<T>
	{

		private static readonly Logger _log = LoggerFactory.getLogger(typeof(PartitionedProducer));

		private IList<ProducerImpl<T>> _producers;
		private MessageRouter _routerPolicy;
		private readonly ProducerStatsRecorderImpl _stats;
		private TopicMetadata _topicMetadata;

		// timeout related to auto check and subscribe partition increasement
		private volatile Timeout _partitionsAutoUpdateTimeout = null;
		internal TopicsPartitionChangedListener TopicsPartitionChangedListener;
		internal CompletableFuture<Void> PartitionsAutoUpdateFuture = null;

		public PartitionedProducer(PulsarClientImpl client, string topic, ProducerConfigurationData conf, int numPartitions, CompletableFuture<Producer<T>> producerCreatedFuture, Schema<T> schema, ProducerInterceptors interceptors) : base(client, topic, conf, producerCreatedFuture, schema, interceptors)
		{
			this._producers = Lists.newArrayListWithCapacity(numPartitions);
			this._topicMetadata = new TopicMetadataImpl(numPartitions);
			this._routerPolicy = MessageRouter;
			_stats = client.Configuration.StatsIntervalSeconds > 0 ? new ProducerStatsRecorderImpl() : null;

			int maxPendingMessages = Math.Min(conf.MaxPendingMessages, conf.MaxPendingMessagesAcrossPartitions / numPartitions);
			conf.MaxPendingMessages = maxPendingMessages;
			Start();

			// start track and auto subscribe partition increasement
			if(conf.AutoUpdatePartitions)
			{
				TopicsPartitionChangedListener = new TopicsPartitionChangedListener(this);
				_partitionsAutoUpdateTimeout = client.Timer().newTimeout(_partitionsAutoUpdateTimerTask, conf.AutoUpdatePartitionsIntervalSeconds, TimeUnit.SECONDS);
			}
		}

		private MessageRouter MessageRouter
		{
			get
			{
				MessageRouter messageRouter;
    
				MessageRoutingMode messageRouteMode = Conf.MessageRoutingMode;
    
				switch(messageRouteMode)
				{
					case MessageRoutingMode.CustomPartition:
						messageRouter = checkNotNull(Conf.CustomMessageRouter);
						break;
					case MessageRoutingMode.SinglePartition:
						messageRouter = new SinglePartitionMessageRouterImpl(ThreadLocalRandom.current().Next(_topicMetadata.NumPartitions()), Conf.HashingScheme);
						break;
					case MessageRoutingMode.RoundRobinPartition:
					default:
						messageRouter = new RoundRobinPartitionMessageRouterImpl(Conf.HashingScheme, ThreadLocalRandom.current().Next(_topicMetadata.NumPartitions()), Conf.BatchingEnabled, TimeUnit.MICROSECONDS.toMillis(Conf.BatchingPartitionSwitchFrequencyIntervalMicros()));
					break;
				}
    
				return messageRouter;
			}
		}

		public override string ProducerName
		{
			get
			{
				return _producers[0].ProducerName;
			}
		}

		public override long LastSequenceId
		{
			get
			{
				// Return the highest sequence id across all partitions. This will be correct,
				// since there is a single id generator across all partitions for the same producer

				return _producers.Select(Producer::getLastSequenceId).Select(long.longValue).Max().orElse(-1);
			}
		}

		private void Start()
		{
			AtomicReference<Exception> createFail = new AtomicReference<Exception>();
			AtomicInteger completed = new AtomicInteger();
			for(int partitionIndex = 0; partitionIndex < _topicMetadata.NumPartitions(); partitionIndex++)
			{
				string partitionName = TopicName.Get(Topic).getPartition(partitionIndex).ToString();
				ProducerImpl<T> producer = new ProducerImpl<T>(ClientConflict, partitionName, Conf, new CompletableFuture<Producer<T>>(), partitionIndex, Schema, Interceptors);
				_producers.Add(producer);
				producer.ProducerCreatedFuture().handle((prod, createException) =>
				{
				if(createException != null)
				{
					State = State.Failed;
					createFail.compareAndSet(null, createException);
				}
				if(completed.incrementAndGet() == _topicMetadata.NumPartitions())
				{
					if(createFail.get() == null)
					{
						State = State.Ready;
						_log.info("[{}] Created partitioned producer", Topic);
						ProducerCreatedFuture().complete(PartitionedProducer.this);
					}
					else
					{
						_log.error("[{}] Could not create partitioned producer.", Topic, createFail.get().Cause);
						CloseAsync().handle((ok, closeException) =>
						{
							ProducerCreatedFuture().completeExceptionally(createFail.get());
							ClientConflict.CleanupProducer(this);
							return null;
						});
					}
				}
				return null;
				});
			}

		}

		internal override CompletableFuture<MessageId> InternalSendAsync<T1>(Message<T1> message)
		{
			return InternalSendWithTxnAsync(message, null);
		}

		internal override CompletableFuture<MessageId> InternalSendWithTxnAsync<T1>(Message<T1> message, Transaction txn)
		{
			switch(State)
			{
				case Ready:
				case Connecting:
					break; // Ok
					goto case Closing;
				case Closing:
				case Closed:
					return FutureUtil.FailedFuture(new PulsarClientException.AlreadyClosedException("Producer already closed"));
				case Terminated:
					return FutureUtil.FailedFuture(new PulsarClientException.TopicTerminatedException("Topic was terminated"));
				case Failed:
				case Uninitialized:
					return FutureUtil.FailedFuture(new PulsarClientException.NotConnectedException());
			}

			int partition = _routerPolicy.ChoosePartition(message, _topicMetadata);
			checkArgument(partition >= 0 && partition < _topicMetadata.NumPartitions(), "Illegal partition index chosen by the message routing policy: " + partition);
			return _producers[partition].InternalSendWithTxnAsync(message, txn);
		}

		public override CompletableFuture<Void> FlushAsync()
		{
			IList<CompletableFuture<Void>> flushFutures = _producers.Select(ProducerImpl::flushAsync).ToList();
			return CompletableFuture.allOf(((List<CompletableFuture<Void>>)flushFutures).ToArray());
		}

		internal override void TriggerFlush()
		{
			_producers.ForEach(ProducerImpl.triggerFlush);
		}

		public override bool Connected
		{
			get
			{
				// returns false if any of the partition is not connected
				return _producers.All(ProducerImpl::isConnected);
			}
		}

		public override long LastDisconnectedTimestamp
		{
			get
			{
				long lastDisconnectedTimestamp = 0;

				Optional<ProducerImpl<T>> p = _producers.Max(System.Collections.IComparer.comparingLong(ProducerImpl::getLastDisconnectedTimestamp));
				if(p.Present)
				{
					lastDisconnectedTimestamp = p.get().LastDisconnectedTimestamp;
				}
				return lastDisconnectedTimestamp;
			}
		}

		public override CompletableFuture<Void> CloseAsync()
		{
			if(State == State.Closing || State == State.Closed)
			{
				return CompletableFuture.completedFuture(null);
			}
			State = State.Closing;

			if(_partitionsAutoUpdateTimeout != null)
			{
				_partitionsAutoUpdateTimeout.cancel();
				_partitionsAutoUpdateTimeout = null;
			}

			AtomicReference<Exception> closeFail = new AtomicReference<Exception>();
			AtomicInteger completed = new AtomicInteger(_topicMetadata.NumPartitions());
			CompletableFuture<Void> closeFuture = new CompletableFuture<Void>();
			foreach(Producer<T> producer in _producers)
			{
				if(producer != null)
				{
					producer.CloseAsync().handle((closed, ex) =>
					{
					if(ex != null)
					{
						closeFail.compareAndSet(null, ex);
					}
					if(completed.decrementAndGet() == 0)
					{
						if(closeFail.get() == null)
						{
							State = State.Closed;
							closeFuture.complete(null);
							_log.info("[{}] Closed Partitioned Producer", Topic);
							ClientConflict.CleanupProducer(this);
						}
						else
						{
							State = State.Failed;
							closeFuture.completeExceptionally(closeFail.get());
							_log.error("[{}] Could not close Partitioned Producer", Topic, closeFail.get().Cause);
						}
					}
					return null;
					});
				}

			}

			return closeFuture;
		}

		public override ProducerStatsRecorderImpl Stats
		{
			get
			{
				lock(this)
				{
					if(_stats == null)
					{
						return null;
					}
					_stats.Reset();
					for(int i = 0; i < _topicMetadata.NumPartitions(); i++)
					{
						_stats.UpdateCumulativeStats(_producers[i].Stats);
					}
					return _stats;
				}
			}
		}

		public virtual IList<ProducerImpl<T>> Producers
		{
			get
			{
				return _producers.ToList();
			}
		}

		internal override string HandlerName
		{
			get
			{
				return "partition-producer";
			}
		}

		// This listener is triggered when topics partitions are updated.
		private class TopicsPartitionChangedListener : PartitionsChangedListener
		{
			private readonly PartitionedProducer<T> _outerInstance;

			public TopicsPartitionChangedListener(PartitionedProducer<T> outerInstance)
			{
				this._outerInstance = outerInstance;
			}

			// Check partitions changes of passed in topics, and add new topic partitions.
			public virtual CompletableFuture<Void> OnTopicsExtended(ICollection<string> topicsExtended)
			{
				CompletableFuture<Void> future = new CompletableFuture<Void>();
				if(topicsExtended.Count == 0 || !topicsExtended.Contains(outerInstance.Topic))
				{
					future.complete(null);
					return future;
				}

				outerInstance.ClientConflict.GetPartitionsForTopic(outerInstance.Topic).thenCompose(list =>
				{
				int oldPartitionNumber = outerInstance._topicMetadata.NumPartitions();
				int currentPartitionNumber = list.size();
				if(_log.DebugEnabled)
				{
					_log.debug("[{}] partitions number. old: {}, new: {}", outerInstance.Topic, oldPartitionNumber, currentPartitionNumber);
				}
				if(oldPartitionNumber == currentPartitionNumber)
				{
					future.complete(null);
					return future;
				}
				else if(oldPartitionNumber < currentPartitionNumber)
				{
					IList<CompletableFuture<Producer<T>>> futureList = list.subList(oldPartitionNumber, currentPartitionNumber).Select(partitionName =>
					{
						int partitionIndex = TopicName.GetPartitionIndex(partitionName);
						ProducerImpl<T> producer = new ProducerImpl<T>(outerInstance.ClientConflict, partitionName, outerInstance.Conf, new CompletableFuture<Producer<T>>(), partitionIndex, outerInstance.Schema, outerInstance.Interceptors);
						outerInstance._producers.Add(producer);
						return producer.ProducerCreatedFuture();
					}).ToList();
					FutureUtil.WaitForAll(futureList).thenAccept(finalFuture =>
					{
						if(_log.DebugEnabled)
						{
							_log.debug("[{}] success create producers for extended partitions. old: {}, new: {}", outerInstance.Topic, oldPartitionNumber, currentPartitionNumber);
						}
						outerInstance._topicMetadata = new TopicMetadataImpl(currentPartitionNumber);
						future.complete(null);
					}).exceptionally(ex =>
					{
						_log.warn("[{}] fail create producers for extended partitions. old: {}, new: {}", outerInstance.Topic, oldPartitionNumber, currentPartitionNumber);
						IList<ProducerImpl<T>> sublist = outerInstance._producers.subList(oldPartitionNumber, outerInstance._producers.Count);
						sublist.ForEach(newProducer => newProducer.closeAsync());
						sublist.Clear();
						future.completeExceptionally(ex);
						return null;
					});
					return null;
				}
				else
				{
					_log.error("[{}] not support shrink topic partitions. old: {}, new: {}", outerInstance.Topic, oldPartitionNumber, currentPartitionNumber);
					future.completeExceptionally(new PulsarClientException.NotSupportedException("not support shrink topic partitions"));
				}
				return future;
				});

				return future;
			}
		}

		private TimerTask _partitionsAutoUpdateTimerTask = new TimerTaskAnonymousInnerClass();

		private class TimerTaskAnonymousInnerClass : TimerTask
		{
			public override void Run(Timeout timeout)
			{
				if(timeout.Cancelled || outerInstance.State != State.Ready)
				{
					return;
				}

				if(_log.DebugEnabled)
				{
					_log.debug("[{}] run partitionsAutoUpdateTimerTask for partitioned producer", outerInstance.Topic);
				}

				// if last auto update not completed yet, do nothing.
				if(outerInstance.PartitionsAutoUpdateFuture == null || outerInstance.PartitionsAutoUpdateFuture.Done)
				{
					outerInstance.PartitionsAutoUpdateFuture = outerInstance.TopicsPartitionChangedListener.onTopicsExtended(ImmutableList.of(outerInstance.Topic));
				}

				// schedule the next re-check task
				outerInstance._partitionsAutoUpdateTimeout = outerInstance.ClientConflict.timer().newTimeout(outerInstance._partitionsAutoUpdateTimerTask, outerInstance.Conf.AutoUpdatePartitionsIntervalSeconds, TimeUnit.SECONDS);
			}
		}

		public virtual Timeout PartitionsAutoUpdateTimeout
		{
			get
			{
				return _partitionsAutoUpdateTimeout;
			}
		}

	}

}