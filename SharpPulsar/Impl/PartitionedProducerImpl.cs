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
namespace org.apache.pulsar.client.impl
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
	using Message = org.apache.pulsar.client.api.Message;
	using MessageId = org.apache.pulsar.client.api.MessageId;
	using MessageRouter = org.apache.pulsar.client.api.MessageRouter;
	using MessageRoutingMode = org.apache.pulsar.client.api.MessageRoutingMode;
	using Producer = org.apache.pulsar.client.api.Producer;
	using PulsarClientException = org.apache.pulsar.client.api.PulsarClientException;
	using NotSupportedException = org.apache.pulsar.client.api.PulsarClientException.NotSupportedException;
	using Schema = org.apache.pulsar.client.api.Schema;
	using TopicMetadata = org.apache.pulsar.client.api.TopicMetadata;
	using ProducerConfigurationData = org.apache.pulsar.client.impl.conf.ProducerConfigurationData;
	using TopicName = org.apache.pulsar.common.naming.TopicName;
	using FutureUtil = org.apache.pulsar.common.util.FutureUtil;
	using Logger = org.slf4j.Logger;
	using LoggerFactory = org.slf4j.LoggerFactory;

	public class PartitionedProducerImpl<T> : ProducerBase<T>
	{

		private static readonly Logger log = LoggerFactory.getLogger(typeof(PartitionedProducerImpl));

		private IList<ProducerImpl<T>> producers;
		private MessageRouter routerPolicy;
		private readonly ProducerStatsRecorderImpl stats;
		private TopicMetadata topicMetadata;

		// timeout related to auto check and subscribe partition increasement
		private volatile Timeout partitionsAutoUpdateTimeout = null;
		internal TopicsPartitionChangedListener topicsPartitionChangedListener;
		internal CompletableFuture<Void> partitionsAutoUpdateFuture = null;

		public PartitionedProducerImpl(PulsarClientImpl client, string topic, ProducerConfigurationData conf, int numPartitions, CompletableFuture<Producer<T>> producerCreatedFuture, Schema<T> schema, ProducerInterceptors interceptors) : base(client, topic, conf, producerCreatedFuture, schema, interceptors)
		{
			this.producers = Lists.newArrayListWithCapacity(numPartitions);
			this.topicMetadata = new TopicMetadataImpl(numPartitions);
			this.routerPolicy = MessageRouter;
			stats = client.Configuration.StatsIntervalSeconds > 0 ? new ProducerStatsRecorderImpl() : null;

			int maxPendingMessages = Math.Min(conf.MaxPendingMessages, conf.MaxPendingMessagesAcrossPartitions / numPartitions);
			conf.MaxPendingMessages = maxPendingMessages;
			start();

			// start track and auto subscribe partition increasement
			if (conf.AutoUpdatePartitions)
			{
				topicsPartitionChangedListener = new TopicsPartitionChangedListener(this);
				partitionsAutoUpdateTimeout = client.timer().newTimeout(partitionsAutoUpdateTimerTask, 1, TimeUnit.MINUTES);
			}
		}

		private MessageRouter MessageRouter
		{
			get
			{
				MessageRouter messageRouter;
    
				MessageRoutingMode messageRouteMode = conf.MessageRoutingMode;
    
				switch (messageRouteMode)
				{
					case CustomPartition:
						messageRouter = checkNotNull(conf.CustomMessageRouter);
						break;
					case SinglePartition:
						messageRouter = new SinglePartitionMessageRouterImpl(ThreadLocalRandom.current().Next(topicMetadata.numPartitions()), conf.HashingScheme);
						break;
					case RoundRobinPartition:
					default:
						messageRouter = new RoundRobinPartitionMessageRouterImpl(conf.HashingScheme, ThreadLocalRandom.current().Next(topicMetadata.numPartitions()), conf.BatchingEnabled, TimeUnit.MICROSECONDS.toMillis(conf.batchingPartitionSwitchFrequencyIntervalMicros()));
					break;
				}
    
				return messageRouter;
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
				return producers.Select(Producer.getLastSequenceId).Select(long?.longValue).Max().orElse(-1);
			}
		}

		private void start()
		{
			AtomicReference<Exception> createFail = new AtomicReference<Exception>();
			AtomicInteger completed = new AtomicInteger();
			for (int partitionIndex = 0; partitionIndex < topicMetadata.numPartitions(); partitionIndex++)
			{
				string partitionName = TopicName.get(topic).getPartition(partitionIndex).ToString();
				ProducerImpl<T> producer = new ProducerImpl<T>(client, partitionName, conf, new CompletableFuture<Producer<T>>(), partitionIndex, schema, interceptors);
				producers.Add(producer);
				producer.producerCreatedFuture().handle((prod, createException) =>
				{
				if (createException != null)
				{
					State = State.Failed;
					createFail.compareAndSet(null, createException);
				}
				if (completed.incrementAndGet() == topicMetadata.numPartitions())
				{
					if (createFail.get() == null)
					{
						State = State.Ready;
						log.info("[{}] Created partitioned producer", topic);
						producerCreatedFuture().complete(PartitionedProducerImpl.this);
					}
					else
					{
						log.error("[{}] Could not create partitioned producer.", topic, createFail.get().Cause);
						closeAsync().handle((ok, closeException) =>
						{
							producerCreatedFuture().completeExceptionally(createFail.get());
							client.cleanupProducer(this);
							return null;
						});
					}
				}
				return null;
				});
			}

		}

		internal override CompletableFuture<MessageId> internalSendAsync<T1>(Message<T1> message)
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

			int partition = routerPolicy.choosePartition(message, topicMetadata);
			checkArgument(partition >= 0 && partition < topicMetadata.numPartitions(), "Illegal partition index chosen by the message routing policy: " + partition);
			return producers[partition].internalSendAsync(message);
		}

		public override CompletableFuture<Void> flushAsync()
		{
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			IList<CompletableFuture<Void>> flushFutures = producers.Select(ProducerImpl::flushAsync).ToList();
			return CompletableFuture.allOf(((List<CompletableFuture<Void>>)flushFutures).ToArray());
		}

		internal override void triggerFlush()
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

		public override CompletableFuture<Void> closeAsync()
		{
			if (State == State.Closing || State == State.Closed)
			{
				return CompletableFuture.completedFuture(null);
			}
			State = State.Closing;

			if (partitionsAutoUpdateTimeout != null)
			{
				partitionsAutoUpdateTimeout.cancel();
				partitionsAutoUpdateTimeout = null;
			}

			AtomicReference<Exception> closeFail = new AtomicReference<Exception>();
			AtomicInteger completed = new AtomicInteger(topicMetadata.numPartitions());
			CompletableFuture<Void> closeFuture = new CompletableFuture<Void>();
			foreach (Producer<T> producer in producers)
			{
				if (producer != null)
				{
					producer.closeAsync().handle((closed, ex) =>
					{
					if (ex != null)
					{
						closeFail.compareAndSet(null, ex);
					}
					if (completed.decrementAndGet() == 0)
					{
						if (closeFail.get() == null)
						{
							State = State.Closed;
							closeFuture.complete(null);
							log.info("[{}] Closed Partitioned Producer", topic);
							client.cleanupProducer(this);
						}
						else
						{
							State = State.Failed;
							closeFuture.completeExceptionally(closeFail.get());
							log.error("[{}] Could not close Partitioned Producer", topic, closeFail.get().Cause);
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
				lock (this)
				{
					if (stats == null)
					{
						return null;
					}
					stats.reset();
					for (int i = 0; i < topicMetadata.numPartitions(); i++)
					{
						stats.updateCumulativeStats(producers[i].Stats);
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
			private readonly PartitionedProducerImpl<T> outerInstance;

			public TopicsPartitionChangedListener(PartitionedProducerImpl<T> outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			// Check partitions changes of passed in topics, and add new topic partitions.
			public virtual CompletableFuture<Void> onTopicsExtended(ICollection<string> topicsExtended)
			{
				CompletableFuture<Void> future = new CompletableFuture<Void>();
				if (topicsExtended.Count == 0 || !topicsExtended.Contains(outerInstance.topic))
				{
					future.complete(null);
					return future;
				}

				outerInstance.client.getPartitionsForTopic(outerInstance.topic).thenCompose(list =>
				{
				int oldPartitionNumber = outerInstance.topicMetadata.numPartitions();
				int currentPartitionNumber = list.size();
				if (log.DebugEnabled)
				{
					log.debug("[{}] partitions number. old: {}, new: {}", outerInstance.topic, oldPartitionNumber, currentPartitionNumber);
				}
				if (oldPartitionNumber == currentPartitionNumber)
				{
					future.complete(null);
					return future;
				}
				else if (oldPartitionNumber < currentPartitionNumber)
				{
					IList<CompletableFuture<Producer<T>>> futureList = list.subList(oldPartitionNumber, currentPartitionNumber).Select(partitionName =>
					{
						int partitionIndex = TopicName.getPartitionIndex(partitionName);
						ProducerImpl<T> producer = new ProducerImpl<T>(outerInstance.client, partitionName, outerInstance.conf, new CompletableFuture<org.apache.pulsar.client.api.Producer<T>>(), partitionIndex, outerInstance.schema, outerInstance.interceptors);
						outerInstance.producers.Add(producer);
						return producer.producerCreatedFuture();
					}).ToList();
					FutureUtil.waitForAll(futureList).thenAccept(finalFuture =>
					{
						if (log.DebugEnabled)
						{
							log.debug("[{}] success create producers for extended partitions. old: {}, new: {}", outerInstance.topic, oldPartitionNumber, currentPartitionNumber);
						}
						outerInstance.topicMetadata = new TopicMetadataImpl(currentPartitionNumber);
						future.complete(null);
					}).exceptionally(ex =>
					{
						log.warn("[{}] fail create producers for extended partitions. old: {}, new: {}", outerInstance.topic, oldPartitionNumber, currentPartitionNumber);
						IList<ProducerImpl<T>> sublist = outerInstance.producers.subList(oldPartitionNumber, outerInstance.producers.Count);
						sublist.forEach(newProducer => newProducer.closeAsync());
						sublist.clear();
						future.completeExceptionally(ex);
						return null;
					});
					return null;
				}
				else
				{
					log.error("[{}] not support shrink topic partitions. old: {}, new: {}", outerInstance.topic, oldPartitionNumber, currentPartitionNumber);
					future.completeExceptionally(new NotSupportedException("not support shrink topic partitions"));
				}
				return future;
				});

				return future;
			}
		}

		private TimerTask partitionsAutoUpdateTimerTask = new TimerTaskAnonymousInnerClass();

		private class TimerTaskAnonymousInnerClass : TimerTask
		{
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void run(io.netty.util.Timeout timeout) throws Exception
			public override void run(Timeout timeout)
			{
				if (timeout.Cancelled || outerInstance.State != State.Ready)
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
				outerInstance.partitionsAutoUpdateTimeout = outerInstance.client.timer().newTimeout(partitionsAutoUpdateTimerTask, 1, TimeUnit.MINUTES);
			}
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @VisibleForTesting public io.netty.util.Timeout getPartitionsAutoUpdateTimeout()
		public virtual Timeout PartitionsAutoUpdateTimeout
		{
			get
			{
				return partitionsAutoUpdateTimeout;
			}
		}

	}

}