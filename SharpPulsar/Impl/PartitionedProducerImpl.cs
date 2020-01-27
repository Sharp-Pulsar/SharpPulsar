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
namespace SharpPulsar.Impl
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.google.common.@base.Preconditions.checkArgument;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.google.common.@base.Preconditions.checkNotNull;

	using VisibleForTesting = com.google.common.annotations.VisibleForTesting;
	using ImmutableList = com.google.common.collect.ImmutableList;
	using Lists = com.google.common.collect.Lists;
	using Timeout = io.netty.util.Timeout;
	using TimerTask = io.netty.util.TimerTask;
	using SharpPulsar.Api;
	using IMessageId = SharpPulsar.Api.IMessageId;
	using MessageRouter = SharpPulsar.Api.MessageRouter;
	using MessageRoutingMode = SharpPulsar.Api.MessageRoutingMode;
	using Producer = SharpPulsar.Api.Producer;
	using PulsarClientException = SharpPulsar.Api.PulsarClientException;
	using NotSupportedException = SharpPulsar.Api.PulsarClientException.NotSupportedException;
	using SharpPulsar.Api;
	using TopicMetadata = SharpPulsar.Api.TopicMetadata;
	using ProducerConfigurationData = SharpPulsar.Impl.Conf.ProducerConfigurationData;
	using TopicName = Org.Apache.Pulsar.Common.Naming.TopicName;
	using FutureUtil = Org.Apache.Pulsar.Common.Util.FutureUtil;
	using Logger = org.slf4j.Logger;
	using LoggerFactory = org.slf4j.LoggerFactory;
    using System.Threading.Tasks;

    public class PartitionedProducerImpl<T> : ProducerBase<T>
	{

		private static readonly Logger log = LoggerFactory.getLogger(typeof(PartitionedProducerImpl));

		private IList<ProducerImpl<T>> producers;
		private MessageRouter routerPolicy;
		private readonly ProducerStatsRecorderImpl stats;
		private TopicMetadata topicMetadata;

		// timeout related to auto check and subscribe partition increasement
		public virtual long PartitionsAutoUpdateTimeout {get;} = null;
		internal TopicsPartitionChangedListener TopicsPartitionChangedListener;
		internal CompletableFuture<Void> PartitionsAutoUpdateFuture = null;

		public PartitionedProducerImpl(PulsarClientImpl Client, string Topic, ProducerConfigurationData Conf, int NumPartitions, TaskCompletionSource<IProducer<T>> producerCreatedTask, ISchema<T> Schema, ProducerInterceptors Interceptors) : base(Client, Topic, Conf, producerCreatedTask, Schema, Interceptors)
		{
			this.producers = Lists.newArrayListWithCapacity(NumPartitions);
			this.topicMetadata = new TopicMetadataImpl(NumPartitions);
			this.routerPolicy = MessageRouter;
			stats = Client.Configuration.StatsIntervalSeconds > 0 ? new ProducerStatsRecorderImpl<T>() : null;

			int MaxPendingMessages = Math.Min(Conf.MaxPendingMessages, Conf.MaxPendingMessagesAcrossPartitions / NumPartitions);
			Conf.MaxPendingMessages = MaxPendingMessages;
			Start();

			// start track and auto subscribe partition increasement
			if (Conf.AutoUpdatePartitions)
			{
				TopicsPartitionChangedListener = new TopicsPartitionChangedListener(this);
				PartitionsAutoUpdateTimeout = Client.timer().newTimeout(partitionsAutoUpdateTimerTask, 1, BAMCIS.Util.Concurrent.TimeUnit.MINUTES);
			}
		}

		private MessageRouter MessageRouter
		{
			get
			{
				MessageRouter MessageRouter;
    
				MessageRoutingMode MessageRouteMode = Conf.MessageRoutingMode;
    
				switch (MessageRouteMode)
				{
					case MessageRoutingMode.CustomPartition:
						MessageRouter = checkNotNull(Conf.CustomMessageRouter);
						break;
					case MessageRoutingMode.SinglePartition:
						MessageRouter = new SinglePartitionMessageRouterImpl(ThreadLocalRandom.current().Next(topicMetadata.NumPartitions()), Conf.HashingScheme);
						break;
					case MessageRoutingMode.RoundRobinPartition:
					default:
						MessageRouter = new RoundRobinPartitionMessageRouterImpl(Conf.HashingScheme, ThreadLocalRandom.current().Next(topicMetadata.NumPartitions()), Conf.BatchingEnabled, BAMCIS.Util.Concurrent.TimeUnit.MICROSECONDS.toMillis(Conf.batchingPartitionSwitchFrequencyIntervalMicros()));
					break;
				}
    
				return MessageRouter;
			}
		}

		public override string ProducerName
		{
			get
			{
				return producers[0].ProducerName;
			}
		}

		public override long LastSequenceId
		{
			get
			{
				// Return the highest sequence id across all partitions. This will be correct,
				// since there is a single id generator across all partitions for the same producer
	//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
				return producers.Select(Producer::getLastSequenceId).Select(long?.longValue).Max().orElse(-1);
			}
		}

		private void Start()
		{
			AtomicReference<Exception> CreateFail = new AtomicReference<Exception>();
			AtomicInteger Completed = new AtomicInteger();
			for (int PartitionIndex = 0; PartitionIndex < topicMetadata.NumPartitions(); PartitionIndex++)
			{
				string PartitionName = TopicName.get(Topic).getPartition(PartitionIndex).ToString();
				ProducerImpl<T> Producer = new ProducerImpl<T>(ClientConflict, PartitionName, Conf, new CompletableFuture<Producer<T>>(), PartitionIndex, Schema, Interceptors);
				producers.Add(Producer);
				Producer.producerCreatedFuture().handle((prod, createException) =>
				{
				if (createException != null)
				{
					State = State.Failed;
					CreateFail.compareAndSet(null, createException);
				}
				if (Completed.incrementAndGet() == topicMetadata.NumPartitions())
				{
					if (CreateFail.get() == null)
					{
						State = State.Ready;
						log.info("[{}] Created partitioned producer", Topic);
						ProducerCreatedFuture().complete(PartitionedProducerImpl.this);
					}
					else
					{
						log.error("[{}] Could not create partitioned producer.", Topic, CreateFail.get().Cause);
						CloseAsync().handle((ok, closeException) =>
						{
							ProducerCreatedFuture().completeExceptionally(CreateFail.get());
							ClientConflict.cleanupProducer(this);
							return null;
						});
					}
				}
				return null;
				});
			}

		}

		public override CompletableFuture<IMessageId> InternalSendAsync<T1>(Message<T1> Message)
		{

			switch (State)
			{
			case Ready:
			case Connecting:
				break; // Ok
				goto case Closing;
			case Closing:
			case Closed:
				return FutureUtil.failedFuture(new PulsarClientException.AlreadyClosedException("Producer already closed"));
			case Terminated:
				return FutureUtil.failedFuture(new PulsarClientException.TopicTerminatedException("Topic was terminated"));
			case Failed:
			case Uninitialized:
				return FutureUtil.failedFuture(new PulsarClientException.NotConnectedException());
			}

			int Partition = routerPolicy.ChoosePartition(Message, topicMetadata);
			checkArgument(Partition >= 0 && Partition < topicMetadata.NumPartitions(), "Illegal partition index chosen by the message routing policy: " + Partition);
			return producers[Partition].internalSendAsync(Message);
		}

		public override CompletableFuture<Void> FlushAsync()
		{
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			IList<CompletableFuture<Void>> FlushFutures = producers.Select(ProducerImpl::flushAsync).ToList();
			return CompletableFuture.allOf(((List<CompletableFuture<Void>>)FlushFutures).ToArray());
		}

		public override void TriggerFlush()
		{
			producers.ForEach(ProducerImpl.triggerFlush);
		}

		public override bool Connected
		{
			get
			{
				// returns false if any of the partition is not connected
	//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
				return producers.All(ProducerImpl::isConnected);
			}
		}

		public override CompletableFuture<Void> CloseAsync()
		{
			if (State == State.Closing || State == State.Closed)
			{
				return CompletableFuture.completedFuture(null);
			}
			State = State.Closing;

			if (PartitionsAutoUpdateTimeout != null)
			{
				PartitionsAutoUpdateTimeout.cancel();
				PartitionsAutoUpdateTimeout = null;
			}

			AtomicReference<Exception> CloseFail = new AtomicReference<Exception>();
			AtomicInteger Completed = new AtomicInteger(topicMetadata.NumPartitions());
			CompletableFuture<Void> CloseFuture = new CompletableFuture<Void>();
			foreach (Producer<T> Producer in producers)
			{
				if (Producer != null)
				{
					Producer.closeAsync().handle((closed, ex) =>
					{
					if (ex != null)
					{
						CloseFail.compareAndSet(null, ex);
					}
					if (Completed.decrementAndGet() == 0)
					{
						if (CloseFail.get() == null)
						{
							State = State.Closed;
							CloseFuture.complete(null);
							log.info("[{}] Closed Partitioned Producer", Topic);
							ClientConflict.cleanupProducer(this);
						}
						else
						{
							State = State.Failed;
							CloseFuture.completeExceptionally(CloseFail.get());
							log.error("[{}] Could not close Partitioned Producer", Topic, CloseFail.get().Cause);
						}
					}
					return null;
					});
				}

			}

			return CloseFuture;
		}

		public override ProducerStatsRecorderImpl Stats
		{
			get
			{
				lock (this)
				{
					if (stats == null)
					{
						return null;
					}
					stats.Reset();
					for (int I = 0; I < topicMetadata.NumPartitions(); I++)
					{
						stats.UpdateCumulativeStats(producers[I].Stats);
					}
					return stats;
				}
			}
		}

		public virtual IList<ProducerImpl<T>> Producers
		{
			get
			{
				return producers.ToList();
			}
		}

		public override string HandlerName
		{
			get
			{
				return "partition-producer";
			}
		}

		// This listener is triggered when topics partitions are updated.
		public class TopicsPartitionChangedListener : PartitionsChangedListener
		{
			private readonly PartitionedProducerImpl<T> outerInstance;

			public TopicsPartitionChangedListener(PartitionedProducerImpl<T> outerInstance)
			{
				this.outerInstance = OuterInstance;
			}

			// Check partitions changes of passed in topics, and add new topic partitions.
			public override CompletableFuture<Void> OnTopicsExtended(ICollection<string> TopicsExtended)
			{
				CompletableFuture<Void> Future = new CompletableFuture<Void>();
				if (TopicsExtended.Count == 0 || !TopicsExtended.Contains(outerInstance.Topic))
				{
					Future.complete(null);
					return Future;
				}

				outerInstance.ClientConflict.getPartitionsForTopic(outerInstance.Topic).thenCompose(list =>
				{
				int OldPartitionNumber = outerInstance.topicMetadata.NumPartitions();
				int CurrentPartitionNumber = list.size();
				if (log.DebugEnabled)
				{
					log.debug("[{}] partitions number. old: {}, new: {}", outerInstance.Topic, OldPartitionNumber, CurrentPartitionNumber);
				}
				if (OldPartitionNumber == CurrentPartitionNumber)
				{
					Future.complete(null);
					return Future;
				}
				else if (OldPartitionNumber < CurrentPartitionNumber)
				{
					IList<CompletableFuture<Producer<T>>> FutureList = list.subList(OldPartitionNumber, CurrentPartitionNumber).Select(partitionName =>
					{
						int PartitionIndex = TopicName.getPartitionIndex(partitionName);
						ProducerImpl<T> Producer = new ProducerImpl<T>(outerInstance.ClientConflict, partitionName, outerInstance.Conf, new CompletableFuture<Producer<T>>(), PartitionIndex, outerInstance.Schema, outerInstance.Interceptors);
						outerInstance.producers.Add(Producer);
						return Producer.producerCreatedFuture();
					}).ToList();
					FutureUtil.waitForAll(FutureList).thenAccept(finalFuture =>
					{
						if (log.DebugEnabled)
						{
							log.debug("[{}] success create producers for extended partitions. old: {}, new: {}", outerInstance.Topic, OldPartitionNumber, CurrentPartitionNumber);
						}
						outerInstance.topicMetadata = new TopicMetadataImpl(CurrentPartitionNumber);
						Future.complete(null);
					}).exceptionally(ex =>
					{
						log.warn("[{}] fail create producers for extended partitions. old: {}, new: {}", outerInstance.Topic, OldPartitionNumber, CurrentPartitionNumber);
						IList<ProducerImpl<T>> Sublist = outerInstance.producers.subList(OldPartitionNumber, outerInstance.producers.Count);
						Sublist.ForEach(newProducer => newProducer.closeAsync());
						Sublist.Clear();
						Future.completeExceptionally(ex);
						return null;
					});
					return null;
				}
				else
				{
					log.error("[{}] not support shrink topic partitions. old: {}, new: {}", outerInstance.Topic, OldPartitionNumber, CurrentPartitionNumber);
					Future.completeExceptionally(new NotSupportedException("not support shrink topic partitions"));
				}
				return Future;
				});

				return Future;
			}
		}

		private TimerTask partitionsAutoUpdateTimerTask = new TimerTaskAnonymousInnerClass();

		public class TimerTaskAnonymousInnerClass : TimerTask
		{
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void run(io.netty.util.Timeout timeout) throws Exception
			public override void run(Timeout Timeout)
			{
				if (Timeout.Cancelled || outerInstance.State != State.Ready)
				{
					return;
				}

				if (log.DebugEnabled)
				{
					log.debug("[{}] run partitionsAutoUpdateTimerTask for partitioned producer", outerInstance.topic);
				}

				// if last auto update not completed yet, do nothing.
				if (outerInstance.partitionsAutoUpdateFuture == null || outerInstance.partitionsAutoUpdateFuture.Done)
				{
					outerInstance.partitionsAutoUpdateFuture = outerInstance.topicsPartitionChangedListener.onTopicsExtended(ImmutableList.of(outerInstance.topic));
				}

				// schedule the next re-check task
				outerInstance.PartitionsAutoUpdateTimeout = outerInstance.client.timer().newTimeout(partitionsAutoUpdateTimerTask, 1, BAMCIS.Util.Concurrent.TimeUnit.MINUTES);
			}
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @VisibleForTesting public io.netty.util.Timeout getPartitionsAutoUpdateTimeout()

	}

}