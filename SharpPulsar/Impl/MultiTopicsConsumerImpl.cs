using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using App.Metrics.Concurrency;
using DotNetty.Common.Utilities;
using SharpPulsar.Api;
using SharpPulsar.Common.Naming;
using SharpPulsar.Extension;
using SharpPulsar.Impl.Conf;
using SharpPulsar.Impl.Transaction;
using SharpPulsar.Protocol.Proto;
using SharpPulsar.Utility.Atomic;
using SharpPulsar.Utils;
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
    using Microsoft.Extensions.Logging;
    using System.Linq;
    using System.Threading.Tasks;

    public class MultiTopicsConsumerImpl<T> : ConsumerBase<T>
	{

		public const string DummyTopicNamePrefix = "MultiTopicsConsumer-";

		// All topics should be in same namespace
        public NamespaceName Namespacename;

		// Map <topic+partition, consumer>, when get do ACK, consumer will by find by topic name
		private readonly ConcurrentDictionary<string, ConsumerImpl<T>> _consumers;

		// Map <topic, numPartitions>, store partition number for each topic
		private readonly ConcurrentDictionary<string, int> _topics;
        private static ScheduledThreadPoolExecutor _poolExecutor; 

		// Queue of partition consumers on which we have stopped calling receiveAsync() because the
		// shared incoming queue was full
		private readonly ConcurrentQueue<ConsumerImpl<T>> _pausedConsumers;

		// Threshold for the shared queue. When the size of the shared queue goes below the threshold, we are going to
		// resume receiving from the paused consumer partitions
		private readonly int _sharedQueueResumeThreshold;

		// sum of topicPartitions, simple topic has 1, partitioned topic equals to partition number.
		internal AtomicInt AllTopicPartitionsNumber;

		// timeout related to auto check and subscribe partition increasement
		public  ITimeout PartitionsAutoUpdateTimeout;
		internal TopicsPartitionChangedListener _topicsPartitionChangedListener;
		internal TaskCompletionSource<Task> partitionsAutoUpdateTask;

		private readonly ReaderWriterLockSlim @lock = new ReaderWriterLockSlim();
		private readonly ConsumerStatsRecorder _stats;
		public  UnAckedMessageTracker<T> UnAckedTopicMessageTracker;
		private readonly ConsumerConfigurationData<T> _internalConfig;
		private State _state = State.Closed;

		public MultiTopicsConsumerImpl(PulsarClientImpl client, ConsumerConfigurationData<T> conf, TaskCompletionSource<IConsumer<T>> subscribeTask, ISchema<T> schema, ConsumerInterceptors<T> interceptors, bool createTopicIfDoesNotExist, ScheduledThreadPoolExecutor executor) : this(client, DummyTopicNamePrefix + Utility.ConsumerName.GenerateRandomName(), conf, subscribeTask, schema, interceptors, createTopicIfDoesNotExist, executor)
		{
		}

		public MultiTopicsConsumerImpl(PulsarClientImpl client, string singleTopic, ConsumerConfigurationData<T> conf, TaskCompletionSource<IConsumer<T>> subscribeTask, ISchema<T> schema, ConsumerInterceptors<T> interceptors, bool createTopicIfDoesNotExist, ScheduledThreadPoolExecutor executor) : base(client, singleTopic, conf, Math.Max(2, conf.ReceiverQueueSize), executor, subscribeTask, schema, interceptors)
		{

			if(conf.ReceiverQueueSize < 1)
				throw new ArgumentException("Receiver queue size needs to be greater than 0 for Topics Consumer");
            _poolExecutor = ListenerExecutor;
			_topics = new ConcurrentDictionary<string, int>();
			_consumers = new ConcurrentDictionary<string, ConsumerImpl<T>>();
			_pausedConsumers = new ConcurrentQueue<ConsumerImpl<T>>();
			_sharedQueueResumeThreshold = MaxReceiverQueueSize / 2;
			AllTopicPartitionsNumber = new AtomicInt(0);

			if (conf.AckTimeoutMillis != 0)
			{
				if (conf.TickDurationMillis > 0)
				{
					UnAckedTopicMessageTracker = new UnAckedTopicMessageTracker<T>(client, this, conf.AckTimeoutMillis, conf.TickDurationMillis);
				}
				else
				{
					UnAckedTopicMessageTracker = new UnAckedTopicMessageTracker<T>(client, this, conf.AckTimeoutMillis);
				}
			}
			else
			{
				UnAckedTopicMessageTracker = UnAckedMessageTracker<T>.UnackedMessageTrackerDisabled;
			}

			_internalConfig = InternalConsumerConfig;
			_stats = client.Configuration.StatsIntervalSeconds > 0 ? new ConsumerStatsRecorderImpl<T>() : null;

			// start track and auto subscribe partition increasement
			if (conf.AutoUpdatePartitions)
			{
				var topicsPartitionChangedListener = new TopicsPartitionChangedListener(this);
				PartitionsAutoUpdateTimeout = client.Timer.NewTimeout(new TimerTaskAnonymousInnerClass(this), TimeSpan.FromMinutes(1));
			}

			if (conf.TopicNames.Count < 1)
			{
				Namespacename = null;
				_state = State.Ready;
				subscribeTask.SetResult(this);
				return;
			}

			if(conf.TopicNames.Count < 1 || !TopicNamesValid(conf.TopicNames))
				throw new ArgumentException("Topics should have same namespace.");
			Namespacename = TopicName.Get(conf.TopicNames.First()).NamespaceObject;

			IList<Task> tasks = conf.TopicNames.Select(t => SubscribeAsync(t, createTopicIfDoesNotExist)).ToList();
			Task.WhenAll(tasks).ContinueWith(task=> 
			{
				if(task.IsFaulted)
				{
					Log.LogWarning("[{}] Failed to subscribe topics: {}", Topic, task.Exception.Message);
					subscribeTask.SetException(task.Exception);
					return;
				}
				if (AllTopicPartitionsNumber.Get() > MaxReceiverQueueSize)
				{
					MaxReceiverQueueSize = AllTopicPartitionsNumber.Get();
				}
				_state = State.Ready;
				StartReceivingMessages(_consumers.Values.ToList());
				Log.LogInformation("[{}] [{}] Created topics consumer with {} sub-consumers", Topic, Subscription, AllTopicPartitionsNumber.Get());
				subscribeTask.SetResult(this);

			});
		}

		// Check topics are valid.
		// - each topic is valid,
		// - every topic has same namespace,
		// - topic names are unique.
		private static bool TopicNamesValid(ICollection<string> topics)
		{
			if(topics != null && (topics == null && topics.Count < 1))
				throw new ArgumentException("topics should contain more than 1 topic");

			var @namespace = TopicName.Get(topics.First()).Namespace;

			var result = topics.Where(topic =>
			{
				var topicInvalid = !TopicName.IsValid(topic);
				if (topicInvalid)
				{
					return true;
				}
				var newNamespace = TopicName.Get(topic).Namespace;
				return !@namespace.Equals(newNamespace);
			}).First();

			if (!string.IsNullOrWhiteSpace(result))
			{
				Log.LogWarning("Received invalid topic name: {}", result);
				return false;
			}

			// check topic names are unique
			var set = new HashSet<string>(topics);
			if (set.Count == topics.Count)
			{
				return true;
			}

            Log.LogWarning("Topic names not unique. unique/all : {}/{}", set.Count, topics.Count);
            return false;
        }

		private void StartReceivingMessages(IList<ConsumerImpl<T>> newConsumers)
		{
			if (Log.IsEnabled(LogLevel.Debug))
			{
				Log.LogDebug("[{}] startReceivingMessages for {} new consumers in topics consumer, state: {}", Topic, newConsumers.Count, _state);
			}
			if (_state == State.Ready)
			{
				newConsumers.ToList().ForEach(consumer =>
				{
					consumer.SendFlowPermitsToBroker(consumer.Handler.Cnx(), Conf.ReceiverQueueSize);
					ReceiveMessageFromConsumer(consumer);
				});
			}
		}

		private void ReceiveMessageFromConsumer(ConsumerImpl<T> consumer)
		{
			consumer.ReceiveAsync().AsTask().ContinueWith(message =>
			{
				if (Log.IsEnabled(LogLevel.Debug))
				{
					Log.LogDebug("[{}] [{}] Receive message from sub consumer:{}", Topic, Subscription, consumer.Topic);
				}
				MessageReceived(consumer, message.Result);
				@lock.EnterWriteLock();
				try
				{
					var size = IncomingMessages.size();
					if (size >= MaxReceiverQueueSize || (size > _sharedQueueResumeThreshold && !_pausedConsumers.IsEmpty))
					{
						_pausedConsumers.Enqueue(consumer);
					}
					else
					{
						Client.EventLoopGroup.Execute(() =>
						{
							ReceiveMessageFromConsumer(consumer);
						});
					}
				}
				finally
				{
					@lock.ExitWriteLock();
				}
			});
		}

		private void MessageReceived(ConsumerImpl<T> consumer, IMessage<T> message)
		{
			if (!(message is MessageImpl<T>))
				throw new ArgumentException("Message<T> is not of type MessageImpl<T>");
			@lock.EnterWriteLock();
			try
			{
				var topicMessage = new TopicMessageImpl<T>(consumer.Topic, consumer.TopicNameWithoutPartition, message);

				if (Log.IsEnabled(LogLevel.Debug))
				{
					Log.LogDebug("[{}][{}] Received message from topics-consumer {}", Topic, Subscription, message.MessageId);
				}

				// if asyncReceive is waiting : return message to callback without adding to incomingMessages queue
				if (!PendingReceives.IsEmpty)
				{
					PendingReceives.TryPeek(out var receivedTask);
                    UnAckedTopicMessageTracker.Add(topicMessage.MessageId);
                    Client.EventLoopGroup.Execute(() =>
                    {
						receivedTask.SetResult(topicMessage);
                    });
				}
				else if (EnqueueMessageAndCheckBatchReceive(topicMessage))
				{
					if (HasPendingBatchReceive())
					{
						NotifyPendingBatchReceivedCallBack();
					}
				}
			}
			finally
			{
				@lock.ExitWriteLock();
			}

			if (Listener != null)
			{
				// Trigger the notification on the message listener in a separate thread to avoid blocking the networking
				// thread while the message processing happens
                Client.EventLoopGroup.Execute(() =>
                {
					IMessage<T> msg;
                    try
                    {
                        msg = InternalReceive();
                    }
                    catch (PulsarClientException e)
                    {
                        Log.LogWarning("[{}] [{}] Failed to dequeue the message for listener", Topic, Subscription, e);
                        return;
                    }
                    try
                    {
                        if (Log.IsEnabled(LogLevel.Debug))
                        {
                            Log.LogDebug("[{}][{}] Calling message listener for message {}", Topic, Subscription, message.MessageId);
                        }
                        Listener.Received(this, msg);
                    }
                    catch (System.Exception e)
                    {
                        Log.LogError("[{}][{}] Message listener error in processing message: {}", Topic, Subscription, message, e);
                    }
				});
			}
		}

		public override void MessageProcessed(IMessage<T> msg)
		{
			lock (this)
			{
				UnAckedTopicMessageTracker.Add(msg.MessageId);
				IncomingMessagesSize[this] = -msg.Data.Length;
			}
		}

		private void ResumeReceivingFromPausedConsumersIfNeeded()
		{
			@lock.EnterReadLock();
			try
			{
				if (IncomingMessages.size() <= _sharedQueueResumeThreshold && !_pausedConsumers.IsEmpty)
				{
					while (true)
					{
						if (!_pausedConsumers.TryPeek(out var consumer))
						{
							break;
						}

						// if messages are readily available on consumer we will attempt to writeLock on the same thread
						Client.EventLoopGroup.Execute(() =>
						{
						    ReceiveMessageFromConsumer(consumer);
						});
					}
				}
			}
			finally
			{
				@lock.ExitReadLock();
			}
		}

		public override IMessage<T> InternalReceive()
		{
            try
			{
				var message = IncomingMessages.Take();
				IncomingMessagesSize[this] = -message.Data.Length;
                if (message is TopicMessageImpl<T>)
                {
                    UnAckedTopicMessageTracker.Add(message.MessageId);
                    ResumeReceivingFromPausedConsumersIfNeeded();
				}
                return message;
			}
			catch (System.Exception e)
			{
				throw PulsarClientException.Unwrap(e);
			}
		}
        public override IMessage<T> InternalReceive(int timeout, BAMCIS.Util.Concurrent.TimeUnit unit)
		{
            try
			{
				var message = IncomingMessages.Poll(timeout, unit);
				if (message != null)
				{
					IncomingMessagesSize[this] = -message.Data.Length;
					if(message is TopicMessageImpl<T>)
						throw new ArgumentException();
					UnAckedTopicMessageTracker.Add(message.MessageId);
				}
				ResumeReceivingFromPausedConsumersIfNeeded();
				return message;
			}
			catch (System.Exception e)
			{
				throw PulsarClientException.Unwrap(e);
			}
		}

		public override IMessages<T> InternalBatchReceive()
		{
			try
			{
				return InternalBatchReceiveAsync().Result;
			}
			catch (System.Exception e) when (e is ThreadInterruptedException)
			{
				var state = _state;
				if (state != State.Closing && state != State.Closed)
				{
					_stats.IncrementNumBatchReceiveFailed();
					throw PulsarClientException.Unwrap(e);
				}

                return null;
            }
		}

		public override ValueTask<IMessages<T>> InternalBatchReceiveAsync()
		{
			var result = new TaskCompletionSource<IMessages<T>>();
			try
			{
				@lock.EnterWriteLock();
				if (PendingBatchReceives == null)
				{
					PendingBatchReceives = new ConcurrentQueue<OpBatchReceive<T>>();
				}
				if (HasEnoughMessagesForBatchReceive())
				{
					var messages = NewMessagesImpl;
					var msgPeeked = IncomingMessages.Peek();
					while (msgPeeked != null && messages.CanAdd(msgPeeked))
					{
						var msg = IncomingMessages.Poll();
						if (msg != null)
						{
							IncomingMessagesSize[this] = -msg.Data.Length;
							var interceptMsg = BeforeConsume(msg);
							messages.Add(interceptMsg);
						}
						msgPeeked = IncomingMessages.Peek();
					}
					result.SetResult(messages);
				}
				else
				{
					PendingBatchReceives.Enqueue(OpBatchReceive<T>.Of<T>(result));
				}
			}
			finally
			{
				@lock.ExitWriteLock();
			}
			return new ValueTask<IMessages<T>>(result.Task); 
		}

		public override ValueTask<IMessage<T>> InternalReceiveAsync()
		{
            var result = new TaskCompletionSource<IMessage<T>>();
            try
			{
				@lock.EnterWriteLock();
				var message = IncomingMessages.Poll(0, BAMCIS.Util.Concurrent.TimeUnit.SECONDS);
				if (message == null)
				{
					PendingReceives.Enqueue(result);
				}
				else
				{
					IncomingMessagesSize[this] = -message.Data.Length;
                    if (message is TopicMessageImpl<T>)
                    {
                        UnAckedTopicMessageTracker.Add(message.MessageId);
                        ResumeReceivingFromPausedConsumersIfNeeded();
                        result.SetResult(message);
					}
					
				}
			}
			catch (ThreadInterruptedException e)
			{
				Thread.CurrentThread.Interrupt();
				result.SetException(new PulsarClientException(e.Message));
			}
			finally
			{
				@lock.ExitWriteLock();
			}

			return new ValueTask<IMessage<T>>(result.Task);
		}

		public override TaskCompletionSource<Task> DoAcknowledge(IMessageId messageId, CommandAck.Types.AckType ackType, IDictionary<string, long> properties, TransactionImpl txnImpl)
		{
			var task = new TaskCompletionSource<Task>();
            if (messageId is TopicMessageIdImpl topicMessageId)
            {
                if (_state != State.Ready)
                {
                    task.SetException(new PulsarClientException("Consumer already closed"));
					return task;
                }

                if (ackType == CommandAck.Types.AckType.Cumulative)
                {
                    var individualConsumer = _consumers[topicMessageId.TopicPartitionName];
                    if (individualConsumer != null)
                    {
                        var innerId = topicMessageId.InnerMessageId;
                        task.SetResult(individualConsumer.AcknowledgeCumulativeAsync(innerId).AsTask());
                        return task;
                    }

                    task.SetException(new PulsarClientException.NotConnectedException());
                    return task;
                }

                {
                    var consumer = _consumers[topicMessageId.TopicPartitionName];

                    var innerId = topicMessageId.InnerMessageId;
                    var t = consumer.DoAcknowledgeWithTxn(innerId, ackType, properties, txnImpl).AsTask()
                        .ContinueWith(_=>
                        {
                            var b = UnAckedTopicMessageTracker.Remove(topicMessageId);
                            return new ValueTask(Task.FromResult(b));
                        }).Result;
                    task.SetResult(t.AsTask());
                }
            }
            return task;
        }

		public override void NegativeAcknowledge(IMessageId messageId)
		{
            if (messageId is TopicMessageIdImpl topicMessageId)
            {
                var consumer = _consumers[topicMessageId.TopicPartitionName];
                consumer.NegativeAcknowledge(topicMessageId.InnerMessageId);
			}
        }

		public override ValueTask UnsubscribeAsync()
		{
			if (_state == State.Closing || _state == State.Closed)
			{
                return new ValueTask(Task.FromException(new PulsarClientException.AlreadyClosedException("Topics Consumer was already closed")));
			}
			_state = State.Closing;

			var unsubscribeTask = new TaskCompletionSource<Task>();
			var taskList = _consumers.Values.Select(c => c.UnsubscribeAsync().AsTask()).ToList();
            Task.WhenAll(taskList).ContinueWith(t =>
            {
                var ex = t.Exception;
                if (ex == null)
                {
                    _state = State.Closed;
                    UnAckedTopicMessageTracker.Dispose();
                    unsubscribeTask.SetResult(null);
                    Log.LogInformation("[{}] [{}] [{}] Unsubscribed Topics Consumer", Topic, Subscription, ConsumerName);
                }
                else
                {
                    _state = State.Failed;
                    unsubscribeTask.SetException(ex);
                    Log.LogError("[{}] [{}] [{}] Could not unsubscribe Topics Consumer", Topic, Subscription, ConsumerName, ex.InnerExceptions);
                }

			});
			Task.WhenAll(taskList).ContinueWith(t =>
			{
			
			});

			return new ValueTask(unsubscribeTask.Task);
		}

		public override ValueTask CloseAsync()
		{
			if (_state == State.Closing || _state == State.Closed)
			{
				UnAckedTopicMessageTracker.Dispose();
				return new ValueTask(null);
			}
			_state = State.Closing;

			if (PartitionsAutoUpdateTimeout != null)
			{
				PartitionsAutoUpdateTimeout.Cancel();
				PartitionsAutoUpdateTimeout = null;
			}

			var closeTask = new TaskCompletionSource<Task>();
			var taskList = _consumers.Values.Select(c => c.CloseAsync().AsTask()).ToList();

			Task.WhenAll(taskList).ContinueWith(t =>
            {
                var ex = t.Exception;
			    if (ex == null)
			    {
				    _state = State.Closed;
				    UnAckedTopicMessageTracker.Dispose();
				    closeTask.SetResult(null);
				    Log.LogInformation("[{}] [{}] Closed Topics Consumer", Topic, Subscription);
				    Client.CleanupConsumer(this);
				    FailPendingReceive();
			    }
			    else
			    {
				    _state = State.Failed;
				    closeTask.SetException(ex);
				    Log.LogError("[{}] [{}] Could not close Topics Consumer", Topic, Subscription, ex);
			    }
			});

			return new ValueTask(closeTask.Task);
		}

		private void FailPendingReceive()
		{
			@lock.EnterReadLock();
			try
			{
				if (!Client.EventLoopGroup.IsShutdown)
				{
					while (!PendingReceives.IsEmpty)
					{
						if (!PendingReceives.TryPeek(out var receiveTask))
						{
							receiveTask.SetException(new PulsarClientException.AlreadyClosedException("Consumer is already closed"));
						}
						else
						{
							break;
						}
					}
				}
			}
			finally
			{
				@lock.ExitReadLock();
			}
		}

		public override bool Connected
		{
			get
			{
				return _consumers.Values.All(consumer => consumer.Connected);
			}
		}

		public new string HandlerName => Subscription;

        private ConsumerConfigurationData<T> InternalConsumerConfig
		{
			get
			{
				var internalConsumerConfig = Conf.Clone();
				internalConsumerConfig.SubscriptionName = Subscription;
				internalConsumerConfig.ConsumerName = ConsumerName;
				internalConsumerConfig.MessageListener = null;
				return internalConsumerConfig;
			}
		}

		public override void RedeliverUnacknowledgedMessages()
		{
			@lock.EnterWriteLock();
			try
			{
				_consumers.Values.ToList().ForEach(consumer => consumer.RedeliverUnacknowledgedMessages());
				IncomingMessages.clear();
				IncomingMessagesSize[this] =  0;
				UnAckedTopicMessageTracker.Clear();
			}
			finally
			{
				@lock.ExitWriteLock();
			}
			ResumeReceivingFromPausedConsumersIfNeeded();
		}

		public override void RedeliverUnacknowledgedMessages(ISet<IMessageId> messageIds)
		{
			if (messageIds.Count == 0)
			{
				return;
			}

			if(!(messageIds.First() is TopicMessageIdImpl))
				throw new ArgumentException();

			if (Conf.SubscriptionType != SubscriptionType.Shared)
			{
				// We cannot redeliver single messages if subscription type is not Shared
				RedeliverUnacknowledgedMessages();
				return;
			}
			RemoveExpiredMessagesFromQueue(messageIds);
			messageIds.Select(messageId => (TopicMessageIdImpl)messageId).GroupBy(x => x.TopicPartitionName).ToList().ForEach(x => _consumers[x.Key].RedeliverUnacknowledgedMessages(new HashSet<IMessageId>(x.Select(mid => mid.InnerMessageId).ToList())));
			ResumeReceivingFromPausedConsumersIfNeeded();
		}

		public override void CompleteOpBatchReceive(OpBatchReceive<T> op)
		{
			NotifyPendingBatchReceivedCallBack(op);
		}

		public override void Seek(IMessageId messageId)
		{
			try
			{
				SeekAsync(messageId);
			}
			catch (System.Exception e)
			{
				throw PulsarClientException.Unwrap(e);
			}
		}

		public override void Seek(long timestamp)
		{
			try
			{
				SeekAsync(timestamp);
			}
			catch (System.Exception e)
			{
				throw PulsarClientException.Unwrap(e);
			}
		}

		public override ValueTask SeekAsync(IMessageId messageId)
		{
			return new ValueTask(Task.FromException(new PulsarClientException("Seek operation not supported on topics consumer")));
		}

		public override ValueTask SeekAsync(long timestamp)
		{
			var tasks = new List<Task>(_consumers.Count);
			_consumers.Values.ToList().ForEach(consumer => tasks.Add(consumer.SeekAsync(timestamp).AsTask()));
			var t = Task.WhenAll(tasks.ToArray());
			return new ValueTask(t);
		}

		public override int AvailablePermits
		{
			get
			{
				return _consumers.Values.Sum(x=> x.AvailablePermits);
			}
		}

		public override bool HasReachedEndOfTopic()
		{
			return _consumers.Values.All(x => x.HasReachedEndOfTopic());
		}

		public override int NumMessagesInQueue()
		{
			return IncomingMessages.size() + _consumers.Values.Sum(x => x.NumMessagesInQueue());
		}

		public override IConsumerStats Stats
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
            
					_consumers.Values.ToList().ForEach(consumer => _stats.UpdateCumulativeStats(consumer.Stats));
					return _stats;
				}
			}
		}


		private void RemoveExpiredMessagesFromQueue(ISet<IMessageId> messageIds)
		{
			var peek = IncomingMessages.Peek();
			if (peek != null)
			{
				if (!messageIds.Contains(peek.MessageId))
				{
					// first message is not expired, then no message is expired in queue.
					return;
				}

				// try not to remove elements that are added while we remove
				var message = IncomingMessages.Poll();
				if(!(message is TopicMessageImpl<T>))
					throw new ArgumentException();
				while (message != null)
				{
					IncomingMessagesSize[this] = -message.Data.Length;
					var messageId = message.MessageId;
					if (!messageIds.Contains(messageId))
					{
						messageIds.Add(messageId);
						break;
					}
					message = IncomingMessages.Poll();
				}
			}
		}

		private bool TopicNameValid(string topicName)
		{
			if(!TopicName.IsValid(topicName))
                throw new ArgumentException("Invalid topic name:" + topicName);
			if(_topics.ContainsKey(topicName))
                throw new ArgumentException("Topics already contains topic:" + topicName);

			if (Namespacename != null)
			{
				if(!TopicName.Get(topicName).Namespace.Equals(Namespacename.ToString()))
                    throw new ArgumentException("Topic " + topicName + " not in same namespace with Topics");
			}

			return true;
		}

		// subscribe one more given topic
		public Task SubscribeAsync(string topicName, bool createTopicIfDoesNotExist)
		{
			if (!TopicNameValid(topicName))
			{
				return Task.FromException(new PulsarClientException.AlreadyClosedException("Topic name not valid"));
			}

			if (_state == State.Closing || _state == State.Closed)
			{
				return Task.FromException(new PulsarClientException.AlreadyClosedException("Topics Consumer was already closed"));
			}

			var subscribeResult = new TaskCompletionSource<Task>();

			Client.GetPartitionedTopicMetadata(topicName).AsTask().ContinueWith(task =>
                {
                    if (task.IsFaulted)
                    {
                        if (task.Exception != null)
                            Log.LogWarning("[{}] Failed to get partitioned topic metadata: {}", topicName,
                                task.Exception.Message);
                        subscribeResult.SetException(task.Exception ?? throw new InvalidOperationException());
                        return;
                    }

                    var metadata = task.Result;
                    SubscribeTopicPartitions(subscribeResult, topicName, metadata.Partitions,
                        createTopicIfDoesNotExist);
                });

			return subscribeResult.Task;
		}

		// create consumer for a single topic with already known partitions.
		// first create a consumer with no topic, then do subscription for already know partitionedTopic.
		public static MultiTopicsConsumerImpl<T> CreatePartitionedConsumer(PulsarClientImpl client, ConsumerConfigurationData<T> conf, TaskCompletionSource<IConsumer<T>> subscribeTask, int numPartitions, ISchema<T> schema, ConsumerInterceptors<T> interceptors)
		{
			if(conf.TopicNames.Count != 1)
                throw new ArgumentException("Should have only 1 topic for partitioned consumer");

			// get topic name, then remove it from conf, so constructor will create a consumer with no topic.
			var cloneConf = conf.Clone();
			var topicName = cloneConf.SingleTopic;
			cloneConf.TopicNames.Remove(topicName);

			var task = new TaskCompletionSource<IConsumer<T>>();
			var consumer = new MultiTopicsConsumerImpl<T>(client, topicName, cloneConf, task, schema, interceptors, true, _poolExecutor);

			task.Task.ContinueWith(c =>
            {
                ((MultiTopicsConsumerImpl<T>) c.Result).SubscribeAsync(topicName, numPartitions)
                    .AsTask().ContinueWith(x =>
                    {
                        if (x.IsFaulted)
                        {
                            Log.LogWarning("Failed subscription for createPartitionedConsumer: {} {}, e:{}", topicName, numPartitions, x.Exception);
                            subscribeTask.SetException(PulsarClientException.Wrap(x.Exception.InnerException,
                                $"Failed to subscribe {topicName} with {numPartitions:D} partitions"));

						}

                        subscribeTask.SetResult(consumer);
                    });
            });
			return consumer;
		}

		// subscribe one more given topic, but already know the numberPartitions
		private ValueTask SubscribeAsync(string topicName, int numberPartitions)
		{
			if (!TopicNameValid(topicName))
			{
				return new ValueTask(Task.FromException(new PulsarClientException.AlreadyClosedException("Topic name not valid")));
			}

			if (_state == State.Closing || _state == State.Closed)
			{
                return new ValueTask(Task.FromException(new PulsarClientException.AlreadyClosedException("Topics Consumer was already closed")));
			}

			var subscribeResult = new TaskCompletionSource<Task>();
			SubscribeTopicPartitions(subscribeResult, topicName, numberPartitions, true);

			return new ValueTask(subscribeResult.Task);
		}

		private void SubscribeTopicPartitions(TaskCompletionSource<Task> subscribeResult, string topicName, int numPartitions, bool createIfDoesNotExist)
		{
			Client.PreProcessSchemaBeforeSubscribe(Client, Schema, topicName).AsTask().ContinueWith(task=>
			{
			    if (task.Exception == null)
			    {
				    DoSubscribeTopicPartitions(subscribeResult, topicName, numPartitions, createIfDoesNotExist);
			    }
			    else
			    {
				    subscribeResult.SetException(task.Exception);
			    }
			});
		}

		private void DoSubscribeTopicPartitions(TaskCompletionSource<Task> subscribeResult, string topicName, int numPartitions, bool createIfDoesNotExist)
		{
			if (Log.IsEnabled(LogLevel.Debug))
			{
				Log.LogDebug("Subscribe to topic {} metadata.partitions: {}", topicName, numPartitions);
			}

			IList<TaskCompletionSource<IConsumer<T>>> taskList;
			if (numPartitions > 0)
			{
				_topics.GetOrAdd(topicName, numPartitions);
				AllTopicPartitionsNumber.AddAndGet(numPartitions);

				var receiverQueueSize = Math.Min(Conf.ReceiverQueueSize, Conf.MaxTotalReceiverQueueSizeAcrossPartitions / numPartitions);
				var configurationData = InternalConsumerConfig;
				configurationData.ReceiverQueueSize = receiverQueueSize;

				taskList = Enumerable.Range(0, numPartitions).Select(partitionIndex =>
				{
				    var partitionName = TopicName.Get(topicName).GetPartition(partitionIndex).ToString();
				    var subTask = new TaskCompletionSource<IConsumer<T>>();
				    var newConsumer = ConsumerImpl<T>.NewConsumerImpl(Client, partitionName, configurationData, Client.ExternalExecutorProvider(), partitionIndex, true, subTask, ConsumerImpl<T>.SubscriptionMode.Durable, null, Schema, Interceptors, createIfDoesNotExist);
				    _consumers.GetOrAdd(newConsumer.Topic, newConsumer);
				    return subTask;
				}).ToList();
			}
			else
			{
				_topics.GetOrAdd(topicName, 1);
				AllTopicPartitionsNumber.Increment();

                var subTask = new TaskCompletionSource<IConsumer<T>>();
				var newConsumer = ConsumerImpl<T>.NewConsumerImpl(Client, topicName, _internalConfig, Client.ExternalExecutorProvider(), -1, true, subTask, ConsumerImpl<T>.SubscriptionMode.Durable, null, Schema, Interceptors, createIfDoesNotExist);
				_consumers.GetOrAdd(newConsumer.Topic, newConsumer);

				taskList = new List<TaskCompletionSource<IConsumer<T>>> {subTask};
			}

			Task.WhenAll(taskList.Select(x=> x.Task)).ContinueWith(finalTask =>
			{
                if (finalTask.IsFaulted)
                {
                    HandleSubscribeOneTopicError(topicName, finalTask.Exception, subscribeResult);
					return;
				}
			    if (AllTopicPartitionsNumber.Get() > MaxReceiverQueueSize)
			    {
				    MaxReceiverQueueSize = AllTopicPartitionsNumber.Get();
			    }
			    var numTopics = _topics.Values.Sum(x => x);
			    if(!AllTopicPartitionsNumber.Get().Equals(numTopics))
                    throw new ArgumentException("allTopicPartitionsNumber " + AllTopicPartitionsNumber.Get() + " not equals expected: " + numTopics);
			    StartReceivingMessages(_consumers.Values.Where(consumer1 =>
                {
                    var consumerTopicName = consumer1.Topic;
                    return TopicName.Get(consumerTopicName).PartitionedTopicName.Equals(TopicName.Get(topicName).PartitionedTopicName.ToString());
                }).ToList());
			    subscribeResult.SetResult(null);
			    Log.LogInformation("[{}] [{}] Success subscribe new topic {} in topics consumer, partitions: {}, allTopicPartitionsNumber: {}", Topic, Subscription, topicName, numPartitions, AllTopicPartitionsNumber.Get());
			    if (Namespacename == null)
			    {
				    Namespacename = TopicName.Get(topicName).NamespaceObject;
			    }

            });
		}

		// handling failure during subscribe new topic, unsubscribe success created partitions
		private void HandleSubscribeOneTopicError(string topicName, System.Exception error, TaskCompletionSource<Task> subscribeFuture)
		{
			Log.LogWarning("[{}] Failed to subscribe for topic [{}] in topics consumer {}", Topic, topicName, error.Message);

			Client.ExternalExecutorProvider().Schedule(() =>
			{
			    var toCloseNum = new AtomicInteger(0);
			    _consumers.Values.Where(consumer1 =>
			    {
				    var consumerTopicName = consumer1.Topic;
				    if (TopicName.Get(consumerTopicName).PartitionedTopicName.Equals(topicName))
				    {
					    toCloseNum.Increment();
					    return true;
				    }

                    return false;
                }).ToList().ForEach(consumer2 =>
			    {
				    consumer2.CloseAsync().AsTask().ContinueWith(ts =>
				    {
					    consumer2.SubscribeTask.SetException(error);
					    AllTopicPartitionsNumber.Decrement();
					    _consumers.Remove(consumer2.Topic, out var m);
					    if (toCloseNum.Decrement() == 0)
					    {
						    Log.LogWarning("[{}] Failed to subscribe for topic [{}] in topics consumer, subscribe error: {}", Topic, topicName, error.Message);
						    _topics.Remove(topicName, out var c);
						    if(AllTopicPartitionsNumber.Get() == _consumers.Values.Count)
						        subscribeFuture.SetException(error);
					    }
					    return;
				    });
			    });
			}, TimeSpan.Zero);
		}

		// un-subscribe a given topic
		public ValueTask UnsubscribeAsync(string topicName)
		{
			if(!TopicName.IsValid(topicName))
                throw new ArgumentException("Invalid topic name:" + topicName);

			if (_state == State.Closing || _state == State.Closed)
			{
				return new ValueTask(Task.FromException(new PulsarClientException.AlreadyClosedException("Topics Consumer was already closed")));
			}

			if (PartitionsAutoUpdateTimeout != null)
			{
				PartitionsAutoUpdateTimeout.Cancel();
				PartitionsAutoUpdateTimeout = null;
			}

			var unsubscribeTask = new TaskCompletionSource<Task>();
			var topicPartName = TopicName.Get(topicName).PartitionedTopicName;

			IList<ConsumerImpl<T>> consumersToUnsub = _consumers.Values.Where(consumer =>
			{
			    var consumerTopicName = consumer.Topic;
			    if (TopicName.Get(consumerTopicName).PartitionedTopicName.Equals(topicPartName))
			    {
				    return true;
			    }

                return false;
            }).ToList();

			var taskList = consumersToUnsub.Select(x => x.UnsubscribeAsync().AsTask()).ToList();

			Task.WhenAll(taskList).ContinueWith(task =>
			{
			    if (task.Exception == null)
			    {
				    consumersToUnsub.ToList().ForEach(consumer1 =>
				    {
					    _consumers.Remove(consumer1.Topic, out var consumerImpl);
					    _pausedConsumers.TryDequeue(out consumer1);
					    AllTopicPartitionsNumber.Decrement();
				    });
				    _topics.Remove(topicName, out var v);
				    ((UnAckedTopicMessageTracker<T>) UnAckedTopicMessageTracker).RemoveTopicMessages(topicName);
				    unsubscribeTask.SetResult(null);
				    Log.LogInformation("[{}] [{}] [{}] Unsubscribed Topics Consumer, allTopicPartitionsNumber: {}", topicName, Subscription, ConsumerName, AllTopicPartitionsNumber);
			    }
			    else
			    {
				    unsubscribeTask.SetException(task.Exception ?? throw new InvalidOperationException());
				    _state = State.Failed;
				    Log.LogError("[{}] [{}] [{}] Could not unsubscribe Topics Consumer", topicName, Subscription, ConsumerName, task.Exception);
			    }
			});

			return new ValueTask(unsubscribeTask.Task);
		}

		// Remove a consumer for a topic
		public ValueTask RemoveConsumerAsync(string topicName)
		{
			if(!TopicName.IsValid(topicName))
                throw new ArgumentException("Invalid topic name:" + topicName);

			if (_state == State.Closing || _state == State.Closed)
			{
				return new ValueTask(Task.FromException(new PulsarClientException.AlreadyClosedException("Topics Consumer was already closed")));
			}

			var unsubscribeTask = new TaskCompletionSource<Task>();
			var topicPartName = TopicName.Get(topicName).PartitionedTopicName;


			IList<ConsumerImpl<T>> consumersToClose = _consumers.Values.Where(consumer =>
			{
			    var consumerTopicName = consumer.Topic;
			    if (TopicName.Get(consumerTopicName).PartitionedTopicName.Equals(topicPartName))
			    {
				    return true;
			    }

                return false;
            }).ToList();

			var taskList = consumersToClose.Select(x=> x.CloseAsync().AsTask()).ToList();

			Task.WhenAll(taskList).ContinueWith(task =>
			{
			    if (task.Exception == null)
			    {
				    consumersToClose.ToList().ForEach(consumer1 =>
				    {
					    _consumers.Remove(consumer1.Topic, out var c);
					    _pausedConsumers.TryDequeue(out consumer1);
					    AllTopicPartitionsNumber.Decrement();
				    });
				    _topics.Remove(topicName, out var t);
				    ((UnAckedTopicMessageTracker<T>) UnAckedTopicMessageTracker).RemoveTopicMessages(topicName);
				    unsubscribeTask.SetResult(null);
				    Log.LogInformation("[{}] [{}] [{}] Removed Topics Consumer, allTopicPartitionsNumber: {}", topicName, Subscription, ConsumerName, AllTopicPartitionsNumber);
			    }
			    else
			    {
				    unsubscribeTask.SetException(task.Exception);
				    _state = State.Failed;
				    Log.LogError("[{}] [{}] [{}] Could not remove Topics Consumer", topicName, Subscription, ConsumerName, task.Exception);
			    }
			});

			return new ValueTask(unsubscribeTask.Task);
		}


		// get topics name
		public IList<string> Topics => _topics.Keys.ToList();

        // get partitioned topics name
		public IList<string> PartitionedTopics => _consumers.Keys.ToList();

        // get partitioned consumers
		public IList<ConsumerImpl<T>> Consumers => _consumers.Values.ToList();

        public override void Pause()
		{
			_consumers.ToList().ForEach(x => x.Value.Pause());
		}

		public override void Resume()
		{
            _consumers.ToList().ForEach(x => x.Value.Resume());
		}

		// This listener is triggered when topics partitions are updated.
		public class TopicsPartitionChangedListener : PartitionsChangedListener
		{
			private readonly MultiTopicsConsumerImpl<T> _outerInstance;

			public TopicsPartitionChangedListener(MultiTopicsConsumerImpl<T> outerInstance)
			{
				this._outerInstance = outerInstance;
			}

			// Check partitions changes of passed in topics, and subscribe new added partitions.
			public TaskCompletionSource<Task> OnTopicsExtended(ICollection<string> topicsExtended)
			{
				var task = new TaskCompletionSource<Task>();
				if (topicsExtended.Count == 0)
				{
					task.SetResult(null);
					return task;
				}

				if (Log.IsEnabled(LogLevel.Debug))
				{
					Log.LogDebug("[{}]  run onTopicsExtended: {}, size: {}", _outerInstance.Topic, topicsExtended.ToString(), topicsExtended.Count);
				}

				var taskList = new List<Task>(topicsExtended.Count);
				topicsExtended.ToList().ForEach(x => taskList.Add(_outerInstance.SubscribeIncreasedTopicPartitions(x).AsTask()));
                Task.WhenAll(taskList).ContinueWith(finalTask =>
                {
                    if (finalTask.IsFaulted)
                    {
                        Log.LogWarning("[{}] Failed to subscribe increased topics partitions: {}", _outerInstance.Topic,
                            finalTask.Exception.Message);
                        task.SetException(finalTask.Exception);
                        return;
                    }

                    task.SetResult(null);
                });
				return task;
			}
		}

		// subscribe increased partitions for a given topic
		private ValueTask SubscribeIncreasedTopicPartitions(string topicName)
		{
			var task = new TaskCompletionSource<Task>();

			Client.GetPartitionsForTopic(topicName).AsTask().ContinueWith(tas =>
            {
                var list = tas.Result;
			    var oldPartitionNumber = _topics[topicName.ToString()];
			    var currentPartitionNumber = list.Count;
			    if (Log.IsEnabled(LogLevel.Debug))
			    {
				    Log.LogDebug("[{}] partitions number. old: {}, new: {}", topicName.ToString(), oldPartitionNumber, currentPartitionNumber);
			    }
			    if (oldPartitionNumber == currentPartitionNumber)
			    {
				    task.SetResult(null);
				    return;
			    }

                if (oldPartitionNumber < currentPartitionNumber)
                {
                    IList<string> newPartitions = list.Skip(currentPartitionNumber).Take(oldPartitionNumber - currentPartitionNumber).ToList();
                    var taskList = newPartitions.Select(partitionName =>
                    {
                        var partitionIndex = TopicName.GetPartitionIndex(partitionName);
                        var subTask = new TaskCompletionSource<IConsumer<T>>();
                        var configurationData = InternalConsumerConfig;
                        var newConsumer = ConsumerImpl<T>.NewConsumerImpl(Client, partitionName, configurationData, Client.ExternalExecutorProvider(), partitionIndex, true, subTask, ConsumerImpl<T>.SubscriptionMode.Durable, null, Schema, Interceptors, true);
                        _consumers.GetOrAdd(newConsumer.Topic, newConsumer);
                        if (Log.IsEnabled(LogLevel.Debug))
                        {
                            Log.LogDebug("[{}] create consumer {} for partitionName: {}", topicName.ToString(), newConsumer.Topic, partitionName);
                        }
                        return subTask.Task;
                    }).ToList();
                    Task.WhenAll(taskList).ContinueWith(finalTask =>
                    {
                        if (finalTask.IsFaulted)
                        {
                            Log.LogWarning("[{}] Failed to subscribe {} partition: {} - {}", Topic, topicName.ToString(), oldPartitionNumber, currentPartitionNumber, finalTask.Exception.Message);
                            task.SetException(finalTask.Exception ?? throw new InvalidOperationException());
                            return;
                        }
                        IList<ConsumerImpl<T>> newConsumerList = newPartitions.Select(partitionTopic => _consumers[partitionTopic]).ToList();
                        StartReceivingMessages(newConsumerList);
                        task.SetResult(null);
                    });
                }
                else
                {
                    Log.LogError("[{}] not support shrink topic partitions. old: {}, new: {}", topicName.ToString(), oldPartitionNumber, currentPartitionNumber);
                    task.SetException(new NotSupportedException("not support shrink topic partitions"));
                }
            });

			return new ValueTask(task.Task);
		}
		
		public class TimerTaskAnonymousInnerClass: ITimerTask
        {
            private readonly MultiTopicsConsumerImpl<T> _outerInstance;

            public TimerTaskAnonymousInnerClass(MultiTopicsConsumerImpl<T> outerInstance)
            {
                _outerInstance = outerInstance;
            }

            public void Run(ITimeout timeout)
			{
				if (timeout.Canceled || _outerInstance._state != State.Ready)
				{
					return;
				}

				if (Log.IsEnabled(LogLevel.Debug))
				{
					Log.LogDebug("[{}]  run partitionsAutoUpdateTimerTask for multiTopicsConsumer: {}", _outerInstance.Topic);
				}

				// if last auto update not completed yet, do nothing.
				if (_outerInstance.partitionsAutoUpdateTask == null || _outerInstance.partitionsAutoUpdateTask.Task.IsCompleted)
                {
                    var tpcl = new TopicsPartitionChangedListener(_outerInstance);
					_outerInstance.partitionsAutoUpdateTask = tpcl.OnTopicsExtended(_outerInstance.Topics);
				}

				// schedule the next re-check task
				_outerInstance.PartitionsAutoUpdateTimeout = _outerInstance.Client.Timer.NewTimeout(this, TimeSpan.FromMinutes(1));
			}
		}


		public override ValueTask<IMessageId> LastMessageIdAsync
		{
			get
			{
				var returnTask = new TaskCompletionSource<IMessageId>();
    
				var messageIdTasks = _consumers.SetOfKeyValuePairs().Select(entry => new KeyValuePair<string, Task<IMessageId>>(entry.Key, entry.Value.LastMessageIdAsync.AsTask())).ToDictionary(x => x.Key, x=> x.Value);
    
				Task.WhenAll(messageIdTasks.SetOfKeyValuePairs().Select(x=> x.Value).ToArray()).ContinueWith(ts =>
				{
				    var builder = new Dictionary<string, IMessageId>();
                    messageIdTasks.ToList().ForEach(k =>
				    {
					    IMessageId messageId;
					    try
					    {
						    messageId = k.Value.Result;
					    }
					    catch (System.Exception e)
					    {
						    Log.LogWarning("[{}] Exception when topic {} getLastMessageId.", k.Key, e);
						    messageId = MessageIdFields.Earliest;
					    }
					    builder.Add(k.Key, messageId);
				    });
				    returnTask.SetResult(new MultiMessageIdImpl(builder));
				});
    
				return new ValueTask<IMessageId>(returnTask.Task);
			}
		}
		private static readonly ILogger Log = new LoggerFactory().CreateLogger<MultiTopicsConsumerImpl<T>>();
	}

}