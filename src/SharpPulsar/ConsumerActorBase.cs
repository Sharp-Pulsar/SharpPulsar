using Akka.Actor;
using Akka.Dispatch;
using Akka.Event;
using Akka.Util;
using Akka.Util.Internal;
using SharpPulsar.Batch.Api;
using SharpPulsar.Configuration;
using SharpPulsar.EventSource.Messages;
using SharpPulsar.Exceptions;
using SharpPulsar.Extension;
using SharpPulsar.Interfaces;
using SharpPulsar.Messages.Consumer;
using SharpPulsar.Messages.Requests;
using SharpPulsar.Messages.Transaction;
using SharpPulsar.Protocol;
using SharpPulsar.Stats.Consumer.Api;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using static SharpPulsar.Exceptions.PulsarClientException;
using static SharpPulsar.Protocol.Proto.CommandAck;
using static SharpPulsar.Protocol.Proto.CommandSubscribe;
using IScheduler = Akka.Actor.IScheduler;

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
    internal abstract class ConsumerActorBase<T> : ReceiveActor
	{
        protected internal const int InitialReceiverQueueSize = 1;
        internal abstract long LastDisconnectedTimestamp();
		internal abstract void NegativeAcknowledge(IMessageId messageId);
        internal abstract void Resume();
		internal abstract void Pause();
		internal abstract bool Connected();
		internal abstract ValueTask Seek(long timestamp);
		internal abstract ValueTask Seek(IMessageId messageId);
		internal abstract IConsumerStatsRecorder Stats { get; }
        internal readonly AtomicBoolean ScaleReceiverQueueHint = new AtomicBoolean(false);

        internal int CurrentReceiverQueueSize;
        internal enum ConsumerType
		{
			PARTITIONED,
			NonPartitioned
		}
        internal Queue<(IActorRef, Messages.Consumer.Receive)> Receives = new Queue<(IActorRef, Messages.Consumer.Receive)>();
        internal Queue<(IActorRef, BatchReceive)> BatchReceives = new Queue<(IActorRef, BatchReceive)>();

        internal bool HasParentConsumer = false;
        protected readonly ILoggingAdapter _log;
		private readonly string _subscription;
		protected internal readonly ConsumerConfigurationData<T> Conf;
		private readonly string _consumerName;
		protected internal readonly IMessageListener<T> Listener;
		protected internal readonly IConsumerEventListener ConsumerEventListener;
		protected internal BufferBlock<IMessage<T>> IncomingMessages;
		protected internal IActorRef UnAckedChunckedMessageIdSequenceMap;
        protected readonly Commands _commands = new Commands();
        protected internal readonly ConcurrentQueue<TaskCompletionSource<IMessage<T>>> PendingReceives;
        protected internal readonly ConcurrentQueue<OpBatchReceive> PendingBatchReceives;
        protected internal int MaxReceiverQueueSize;
		protected internal readonly ISchema<T> Schema;
		protected internal readonly ConsumerInterceptors<T> Interceptors;
		protected internal readonly BatchReceivePolicy BatchReceivePolicy;
		protected internal long IncomingMessagesSize = 0L;
		protected internal ICancelable BatchReceiveTimeout = null;
		protected internal readonly IActorRef StateActor;
		private readonly ICancelable _stateUpdater;
		protected internal HandlerState State;
		private readonly string _topic;
        protected internal readonly TaskCompletionSource<IActorRef> SubscribeFuture;
        protected internal long ConsumerEpoch;
        internal readonly IScheduler Scheduler;
        internal readonly ICancelable ReceiveRun;
        internal readonly ICancelable BatchRun;
        public ConsumerActorBase(IActorRef stateActor, IActorRef lookup, IActorRef connectionPool, string topic, ConsumerConfigurationData<T> conf, int receiverQueueSize, ISchema<T> schema, TaskCompletionSource<IActorRef> subscribeFuture)
		{
            SubscribeFuture = subscribeFuture;
            if (conf.Interceptors != null && conf.Interceptors.Count > 0)
                Interceptors = new ConsumerInterceptors<T>(Context.GetLogger(), conf.Interceptors);
            StateActor = stateActor;
			_topic = topic;
			_consumerName = conf.ConsumerName ?? Utility.ConsumerName.GenerateRandomName();
			State = new HandlerState(lookup, connectionPool, topic, Context.System, _consumerName);
			_log = Context.GetLogger();
			MaxReceiverQueueSize = receiverQueueSize;
			_subscription = conf.SubscriptionName;
			Conf = conf;
			Listener = conf.MessageListener;
            Scheduler = Context.System.Scheduler;   
            ConsumerEventListener = conf.ConsumerEventListener;
            PendingBatchReceives = new ConcurrentQueue<OpBatchReceive>();
            PendingReceives = new ConcurrentQueue<TaskCompletionSource<IMessage<T>>>();
            IncomingMessages = new BufferBlock<IMessage<T>>();
			UnAckedChunckedMessageIdSequenceMap = Context.ActorOf(Tracker.UnAckedChunckedMessageIdSequenceMap.Prop());
			Schema = schema;
			if (conf.BatchReceivePolicy != null)
			{
				var userBatchReceivePolicy = conf.BatchReceivePolicy;
				if (userBatchReceivePolicy.MaxNumMessages > MaxReceiverQueueSize)
				{
					BatchReceivePolicy = new BatchReceivePolicy.Builder().MaxNumMessages(MaxReceiverQueueSize).MaxNumBytes(userBatchReceivePolicy.MaxNumBytes).Timeout((int)userBatchReceivePolicy.TimeoutMs).Build();
					_log.Warning($"BatchReceivePolicy maxNumMessages: {userBatchReceivePolicy.MaxNumMessages} is greater than maxReceiverQueueSize: {MaxReceiverQueueSize}, reset to maxReceiverQueueSize. batchReceivePolicy: {BatchReceivePolicy}");
				}
				else if (userBatchReceivePolicy.MaxNumMessages <= 0 && userBatchReceivePolicy.MaxNumBytes <= 0)
				{
					BatchReceivePolicy = new BatchReceivePolicy.Builder().MaxNumMessages(BatchReceivePolicy.DefaultPolicy.MaxNumMessages).MaxNumBytes(BatchReceivePolicy.DefaultPolicy.MaxNumBytes).Timeout((int)userBatchReceivePolicy.TimeoutMs).Build();
					_log.Warning("BatchReceivePolicy maxNumMessages: {} or maxNumBytes: {} is less than 0. " + "Reset to DEFAULT_POLICY. batchReceivePolicy: {}", userBatchReceivePolicy.MaxNumMessages, userBatchReceivePolicy.MaxNumBytes, BatchReceivePolicy.ToString());
				}
				else
				{
					BatchReceivePolicy = conf.BatchReceivePolicy;
				}
			}
			else
			{
				BatchReceivePolicy = BatchReceivePolicy.DefaultPolicy;
			}

			_stateUpdater = Context.System.Scheduler.ScheduleTellRepeatedlyCancelable(TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(5), Self, SendState.Instance, ActorRefs.NoSender);
            InitReceiverQueueSize();
            ReceiveRun = Context.System.Scheduler.Advanced.ScheduleRepeatedlyCancelable(TimeSpan.FromSeconds(5), TimeSpan.FromMilliseconds(100), ()=>
            {
                Receives.TryDequeue(out var queue);
                if(queue.Item1 != null) 
                {
                    try
                    {
                        var message = queue.Item2.Time == TimeSpan.Zero ? Receive() : Receive(queue.Item2.Time);
                        queue.Item1.Tell(new AskResponse(message));
                    }
                    catch (Exception ex)
                    {
                        queue.Item1.Tell(new AskResponse(ex));
                    }
                }

            });
            BatchRun = Context.System.Scheduler.Advanced.ScheduleRepeatedlyCancelable(TimeSpan.FromSeconds(5), TimeSpan.FromMilliseconds(100), () =>
            {
                BatchReceives.TryDequeue(out var queue);
                if (queue.Item1 != null)
                {
                    try
                    {
                        var message = BatchReceive();
                        queue.Item1.Tell(new AskResponse(message));
                    }
                    catch (Exception ex)
                    {
                        queue.Item1.Tell(new AskResponse(ex));
                    }
                }

            });
        }
        protected internal virtual void TriggerBatchReceiveTimeoutTask()
        {
            if (!HasBatchReceiveTimeout() && BatchReceivePolicy.TimeoutMs > 0)
            {
                BatchReceiveTimeout = Scheduler.Advanced.ScheduleOnceCancelable(TimeSpan.FromMilliseconds(BatchReceivePolicy.TimeoutMs), () => PendingBatchReceiveTask(TimeSpan.FromMilliseconds(BatchReceivePolicy.TimeoutMs + 500)));
            }
        }

        protected internal virtual void InitReceiverQueueSize()
        {
            if (Conf.IsAutoScaledReceiverQueueSizeEnabled)
            {
                CurrentReceiverQueueSize = MinReceiverQueueSize();
            }
            else
            {
                CurrentReceiverQueueSize = MaxReceiverQueueSize;
            }
        }
        public abstract int MinReceiverQueueSize();
        protected internal virtual void ExpectMoreIncomingMessages()
        {
            if (!Conf.IsAutoScaledReceiverQueueSizeEnabled)
            {
                return;
            }
             
            if (ScaleReceiverQueueHint.CompareAndSet(true, false))
            {
                var oldSize = CurrentReceiverQueueSize;
                var newSize = Math.Min(MaxReceiverQueueSize, oldSize * 2);
                CurrentReceiverQueueSize = newSize;
            }
        }
        protected internal virtual void ReduceCurrentReceiverQueueSize()
        {
            if (!Conf.IsAutoScaledReceiverQueueSizeEnabled)
            {
                return;
            }
            var oldSize = CurrentReceiverQueueSize;
            var newSize = Math.Max(MinReceiverQueueSize(), oldSize / 2);
            if (oldSize > newSize)
            {
                CurrentReceiverQueueSize = newSize;
            }
        }
        protected internal virtual void FailPendingReceive()
        {
            //Akka.Dispatch.ChannelTaskScheduler.Get().GetScheduler().
            //Akka.Dispatch.ChannelTaskSchedulerProvider i = new ChannelTaskSchedulerProvider();
            //Akka.Dispatch.ActionRunnable f = new ActionRunnable(() => { });
            //Akka.Dispatch.ExecutorService executor = new Akka.Dispatch.ExecutorService();
            //Akka.Dispatch.PinnedDispatcher pinnedDispatcher = null;
            /* if (InternalPinnedExecutor.isShutdown())
            {
                // we need to fail any pending receives no matter what,
                // to avoid blocking user code
                FailPendingReceives();
                FailPendingBatchReceives();
                return CompletableFuture.completedFuture(null);
            }
            else
            {
                
            }*/
            try
            {
                FailPendingReceives();
                FailPendingBatchReceives();
            }
            catch (Exception ex)
            {
                _log.Error(ex.ToString());
            }
        }

        private void FailPendingReceives()
        {
            
            while (PendingReceives.Count > 0)
            {
                var receiveFuture = new TaskCompletionSource<IMessage<T>>(TaskCreationOptions.RunContinuationsAsynchronously);
                PendingReceives.TryDequeue(out receiveFuture);
                if (receiveFuture == null)
                {
                    break;
                }
                if (!receiveFuture.Task.IsCompleted)
                {
                    _log.Error(new AlreadyClosedException($"The consumer which subscribes the topic {Topic} with subscription name {Subscription} was already closed when cleaning and closing the consumers").ToString());
                }
            }
        }

        private void FailPendingBatchReceives()
        {
            while (HasNextBatchReceive())
            {
                var opBatchReceive = NextBatchReceive();
                if (opBatchReceive == null || opBatchReceive.Future == null)
                {
                    break;
                }
                if (!opBatchReceive.Future.Task.IsCompleted)
                {
                    _log.Error(new AlreadyClosedException($"The consumer which subscribes the topic {Topic} with subscription name {Subscription} was already closed when cleaning and closing the consumers").ToString());
                }
            }
        }

        private static void ValidateMessageId(IMessage<T> message)
        {
            if (message == null)
            {
                throw new PulsarClientException.InvalidMessageException("Non-null message is required");
            }
            if (message.MessageId == null)
            {
                throw new PulsarClientException.InvalidMessageException("Cannot handle message with null messageId");
            }
        }

        protected internal virtual async Task ReconsumeLater(IMessage<T> message, IDictionary<string, string> customProperties, TimeSpan delayTime)
        {           
            if (!Conf.RetryEnable)
            {
                throw new PulsarClientException("reconsumeLater method not support!");
            }
            try
            {
                ValidateMessageId(message);
            }
            catch (PulsarClientException e)
            {
                throw e;
            }
            await DoReconsumeLater(message, AckType.Individual, customProperties, delayTime).Task;
        }
        protected internal virtual void ReconsumeLater(IMessages<T> messages, TimeSpan delayTime)
        {
            foreach (var message in messages)
            {
                try
                {
                    ValidateMessageId(message);
                }
                catch (PulsarClientException e)
                {
                     throw e;
                }
            }
            try
            {
                messages.ForEach(async message => await ReconsumeLater(message, new Dictionary<string, string>(), delayTime));
            }
            catch (NullReferenceException npe)
            {
                throw new PulsarClientException.InvalidMessageException(npe.Message);
            }
            
        }
        protected internal virtual IMessage<T> Receive()
        {
            if (Listener != null)
            {
                throw new InvalidConfigurationException("Cannot use receive() when a listener has been set");
            }
            VerifyConsumerState();
            return InternalReceive();
        }

        protected internal virtual IMessage<T> Receive(TimeSpan time)
        {
            if (CurrentReceiverQueueSize == 0)
            {
                throw new InvalidConfigurationException("Can't use receive with timeout, if the queue size is 0");
            }
            if (Listener != null)
            {
                throw new InvalidConfigurationException("Cannot use receive() when a listener has been set");
            }

            VerifyConsumerState();
            return InternalReceive(time);
        }

        protected void VerifyBatchReceive()
        {
            if (Listener != null)
            { 
                throw new InvalidConfigurationException("Cannot use receive() when a listener has been set");
            }
            if (CurrentReceiverQueueSize == 0)
            {
                throw new InvalidConfigurationException("Can't use batch receive, if the queue size is 0");
            }
        }
        protected internal virtual IMessages<T> BatchReceive()
        {
            VerifyBatchReceive();
            VerifyConsumerState();
            return InternalBatchReceive();
        }

        protected internal virtual TaskCompletionSource<IMessages<T>> BatchReceiveAsync()
        {
            try
            {
                VerifyBatchReceive();
                VerifyConsumerState();
                return InternalBatchReceiveAsync();
            }
            catch (PulsarClientException e)
            {
                throw;
            }
        }
        protected internal virtual IMessage<T> BeforeConsume(IMessage<T> message)
        {
            if (Interceptors != null)
            {
                return Interceptors.BeforeConsume(Self, message);
            }
            else
            {
                return message;
            }
        }
      
        protected void VerifyConsumerState()
        {
            var state = State.ConnectionState;
            switch (state)
            {
                case HandlerState.State.Ready:
                case HandlerState.State.Connecting:
                    break; // Ok
                case HandlerState.State.Closing:
                case HandlerState.State.Closed:
                   throw new AlreadyClosedException("Consumer already closed");
                case HandlerState.State.Terminated:
                    throw new TopicTerminatedException("Topic was terminated");
                case HandlerState.State.Failed:
                case HandlerState.State.Uninitialized:
                    throw new NotConnectedException();
                default:
                    break;
            }
        }
        protected bool HasEnoughMessagesForBatchReceive()
        {
            var mesageSize = IncomingMessagesSize;
            var mesageCount = IncomingMessages.Count;
            if (BatchReceivePolicy.MaxNumMessages <= 0 && BatchReceivePolicy.MaxNumBytes <= 0)
            {
                return false;
            }
            var batch = (BatchReceivePolicy.MaxNumMessages > 0 && mesageCount >= BatchReceivePolicy.MaxNumMessages) 
                || (BatchReceivePolicy.MaxNumBytes > 0 && mesageSize >= BatchReceivePolicy.MaxNumBytes);

            return batch;   
        }
        internal void NegativeAcknowledge(IMessage<T> message)
		{
			NegativeAcknowledge(message.MessageId);
		}
        internal abstract void Unsubscribe();
        protected internal virtual void NegativeAcknowledge(IMessages<T> messages)
		{
			messages.ForEach(NegativeAcknowledge);
		}

		protected internal virtual SubType SubType
		{
			get
			{
				return Conf.SubscriptionType;

			}
		}

		internal abstract int AvailablePermits();

		internal abstract int NumMessagesInQueue();


		internal virtual string Topic
		{
			get
			{
				return _topic;
			}
		}

		internal virtual string Subscription
		{
			get
			{
				return _subscription;
			}
		}

		internal virtual string ConsumerName
		{
			get
			{
				return _consumerName;
			}
		}

		protected internal virtual void OnAcknowledge(IMessageId messageId, Exception exception)
		{
			if (Interceptors != null)
			{
				Interceptors.OnAcknowledge(Self, messageId, exception);
			}
		}
        protected internal virtual void OnAcknowledge(IList<IMessageId> messageIds, Exception exception)
        {
            if (Interceptors != null)
            {
                messageIds.ForEach(messageId => Interceptors.OnAcknowledge(Self, messageId, exception));
            }
        }
        protected internal virtual void OnAcknowledgeCumulative(IMessageId messageId, Exception exception)
		{
			if (Interceptors != null)
			{
				Interceptors.OnAcknowledgeCumulative(Self, messageId, exception);
			}
		}
        protected internal virtual void OnAcknowledgeCumulative(IList<IMessageId> messageIds, Exception exception)
        {
            if (Interceptors != null)
            {
                messageIds.ForEach(messageId => Interceptors.OnAcknowledgeCumulative(Self, messageId, exception));
            }
        }
        protected internal virtual void OnNegativeAcksSend(ISet<IMessageId> messageIds)
		{
			if (Interceptors != null)
			{
				Interceptors.OnNegativeAcksSend(Self, messageIds);
			}
		}
        
        protected internal virtual void OnAckTimeoutSend(ISet<IMessageId> messageIds)
		{
			if (Interceptors != null)
			{
				Interceptors.OnAckTimeoutSend(Self, messageIds);
			}
		}
        protected internal virtual void TryTriggerListener()
        {
            if (Listener != null)
            {
                TriggerListener();
            }
        }

        private void TriggerListener()
        {
            // The messages are added into the receiver queue by the internal pinned executor,
            // so need to use internal pinned executor to avoid race condition which message
            // might be added into the receiver queue but not able to read here.
            ActorTaskScheduler.RunTask(() =>
            {
                try
                {
                    IMessage<T> msg;
                    do
                    {
                        msg = InternalReceive(TimeSpan.FromMilliseconds(0));
                        if (msg != null)
                        {
                            IMessage<T> finalMsg = msg;
                            if (SubType.KeyShared == Conf.SubscriptionType)
                            {
                                PeekMessageKey(msg);
                                CallMessageListener(finalMsg);
                            }
                            else
                            {
                                CallMessageListener(finalMsg);
                            }
                        }
                        else
                        {
                            if (_log.IsDebugEnabled)
                            {
                                _log.Debug($"[{Topic}] [{Subscription}] Message has been cleared from the queue");
                            }
                        }
                    } while (msg != null);
                }
                catch (PulsarClientException e)
                {
                    _log.Warning($"[{Topic}] [{Subscription}] Failed to dequeue the message for listener. {e}");
                }
            });
        }

        protected internal virtual bool HasNextPendingReceive()
        {
            return PendingReceives.Count > 0;
        }
        protected internal virtual void CompletePendingReceive(TaskCompletionSource<IMessage<T>> receivedFuture, IMessage<T> msg)
        {
            //ActorTaskScheduler.RunTask(() => { });
            var receivedConsumer = (msg is TopicMessage<T>) ? ((TopicMessage<T>)msg).ReceivedByConsumer : null;

            var executor = receivedConsumer != null ? receivedConsumer : Self;
            /* return executor;
            GetInternalExecutor(message).execute(() =>
            {
                if (!ReceivedFuture.complete(Message))
                {
                    log.warn("Race condition detected. receive future was already completed (cancelled={}) and message was " + "dropped. message={}", ReceivedFuture.isCancelled(), Message);
                }
            });*/
        }
        protected internal virtual TaskCompletionSource<IMessage<T>> NextPendingReceive()
        {
            TaskCompletionSource<IMessage<T>> receivedFuture;
            do
            {
                PendingReceives.TryDequeue(out receivedFuture);
                // skip done futures (cancelling a future could mark it done)
            } while (receivedFuture != null && receivedFuture.Task.IsCompleted);
            return receivedFuture;
        }

        protected internal abstract IMessage<T> InternalReceive();

        protected internal abstract TaskCompletionSource<IMessage<T>> InternalReceiveAsync();
        protected internal abstract IMessages<T> InternalBatchReceive();
        protected internal abstract IMessage<T> InternalReceive(TimeSpan timeOut);
        protected internal abstract TaskCompletionSource<IMessages<T>> InternalBatchReceiveAsync();
        protected internal virtual void CallMessageListener(IMessage<T> msg)
        {
            try
            {
                if (_log.IsDebugEnabled)
                {
                    _log.Debug($"[{Topic}][{Subscription}] Calling message listener for message {msg.MessageId}");
                }
                var receivedConsumer = (msg is TopicMessage<T>) ? ((TopicMessage<T>)msg).ReceivedByConsumer : Self;
                // Increase the permits here since we will not increase permits while receive messages from consumer
                // after enabled message listener.
                receivedConsumer.Tell(new IncreaseAvailablePermits<T>(msg is TopicMessage<T> ? ((TopicMessage<T>)msg).Message : msg));
                Listener.Received(Self, msg);
            }
            catch (Exception ex)
            {
                _log.Error($"[{Topic}][{Subscription}] Message listener error in processing message: {msg.MessageId}. {ex}");
            }
        }

        internal static readonly byte[] NoneKey = Encoding.UTF8.GetBytes("NONE_KEY");
        protected internal virtual byte[] PeekMessageKey(IMessage<T> msg)
        {
            byte[] key = NoneKey;
            if (msg.HasKey())
            {
                key = msg.KeyBytes;
            }
            if (msg.HasOrderingKey())
            {
                key = msg.OrderingKey;
            }
            return key;
        }
        protected internal virtual bool CanEnqueueMessage(IMessage<T> message)
		{
			// Default behavior, can be overridden in subclasses
			return true;
		}
        protected internal sealed class OpBatchReceive
        {

            internal readonly TaskCompletionSource<IMessages<T>> Future;
            internal readonly long CreatedAt;

            internal OpBatchReceive(TaskCompletionSource<IMessages<T>> future)
            {
                Future = future;
                CreatedAt = NanoTime();
            }

            internal static OpBatchReceive Of(TaskCompletionSource<IMessages<T>> future)
            {
                return new OpBatchReceive(future);
            }
        }
        internal static long NanoTime()
        {
            var nano = 10000L * Stopwatch.GetTimestamp();
            nano /= TimeSpan.TicksPerMillisecond;
            nano *= 100L;
            return nano;
        }
        protected internal virtual void NotifyPendingBatchReceivedCallBack()
        {
            var opBatchReceive = NextBatchReceive();
            if (opBatchReceive == null)
            {
                return;
            }
            NotifyPendingBatchReceivedCallBack(opBatchReceive);
        }

        private bool HasNextBatchReceive()
        {
            return !PendingBatchReceives.IsEmpty;
        }


        private OpBatchReceive NextBatchReceive()
        {
            OpBatchReceive opBatchReceive = null;
            while (opBatchReceive == null)
            {
                PendingBatchReceives.TryDequeue(out opBatchReceive);

                // no entry available
                if (opBatchReceive == null)
                {
                    return null;
                }
                // skip entries where future is null or has been completed (cancel / timeout)
                if (opBatchReceive.Future == null || opBatchReceive.Future.Task.IsCompleted)
                {
                    opBatchReceive = null;
                }
            }
            return opBatchReceive;
        }

        protected internal void NotifyPendingBatchReceivedCallBack(OpBatchReceive opBatchReceive)
        {
            var messages = NewMessages;
            while (IncomingMessages.TryReceive(out var msg) && messages.CanAdd(msg))
            {
                MessageProcessed(msg);
                var interceptMsg = BeforeConsume(msg);
                messages.Add(interceptMsg);
            }

            CompletePendingBatchReceive(opBatchReceive.Future, messages);
        }

        protected internal virtual void CompletePendingBatchReceive(TaskCompletionSource<IMessages<T>> future, IMessages<T> messages)
        {
            if (!future.Task.IsCompletedSuccessfully )
            {
                future.SetResult(messages);
                _log.Warning($"Race condition detected. batch receive future was already completed (cancelled={future.Task.IsCanceled}) and messages were dropped. messages={messages}");
            }
        }

        protected internal abstract void MessageProcessed(IMessage<T> msg);


        private void PendingBatchReceiveTask(TimeSpan time)
        {
            DoPendingBatchReceiveTask(time);
        }

        private void DoPendingBatchReceiveTask(TimeSpan timeout)
        {
            long timeToWaitMs;
            bool hasPendingReceives = false;
            // If it's closing/closed we need to ignore this timeout and not schedule next timeout.
            if (State.ConnectionState == HandlerState.State.Closing || State.ConnectionState == HandlerState.State.Closed)
            {
                return;
            }

            timeToWaitMs = BatchReceivePolicy.TimeoutMs;
            PendingBatchReceives.TryPeek(out var opBatchReceive);

            while (opBatchReceive != null)
            {
                // If there is at least one batch receive, calculate the diff between the batch receive timeout
                // and the elapsed time since the operation was created.
                var diff = BatchReceivePolicy.TimeoutMs - (long)TimeSpan.FromMilliseconds(NanoTime() - opBatchReceive.CreatedAt).TotalMilliseconds;

                if (diff <= 0)
                {
                    CompleteOpBatchReceive(opBatchReceive);

                    // remove the peeked item from the queue
                    PendingBatchReceives.TryDequeue(out var removed);

                    if (removed != opBatchReceive)
                    {
                        // regression check, if this were to happen due to incorrect code changes in the future,
                        // (allowing multi-threaded calls to poll()), then ensure that the polled item is completed
                        // to avoid blocking user code

                        _log.Error($"Race condition in consumer {ConsumerName} (should not cause data loss). Concurrent operations on pendingBatchReceives is not safe");
                        if (removed != null && !removed.Future.Task.IsCompleted)
                        {
                            CompleteOpBatchReceive(removed);
                        }
                    }

                }
                else
                {
                    // The diff is greater than zero, set the timeout to the diff value
                    timeToWaitMs = diff;
                    hasPendingReceives = true;
                    break;
                }

                PendingBatchReceives.TryPeek(out opBatchReceive);
            }
            if (hasPendingReceives)
            {
                BatchReceiveTimeout = Scheduler.Advanced.ScheduleOnceCancelable(TimeSpan.FromMilliseconds(timeToWaitMs), () => PendingBatchReceiveTask(TimeSpan.FromMilliseconds(timeToWaitMs + 500)));
            }
            else
            {
                BatchReceiveTimeout = null;
            }
        }
        protected internal virtual bool HasPendingBatchReceive()
        {
            return PendingBatchReceives != null && HasNextBatchReceive();
        }
        protected internal virtual void ClearIncomingMessages()
        {
            // release messages if they are pooled messages
            IncomingMessages.Empty();
            ResetIncomingMessageSize();
        }
        protected internal virtual void ResetIncomingMessageSize()
        {
            IncomingMessagesSize = 0;
            long oldSize = IncomingMessagesSize;
           // MemoryLimitController..ifPresent(limiter => limiter.releaseMemory(OldSize));
        }

        protected internal virtual void DecreaseIncomingMessageSize(in IMessage<T> message)
        {
            IncomingMessagesSize -= message.Data.Length;
            //MemoryLimitController.ifPresent(limiter => limiter.releaseMemory(Message.size()));
        }
        protected internal virtual Messages<T> NewMessages
		{
			get
			{
				return new Messages<T>(BatchReceivePolicy.MaxNumMessages, BatchReceivePolicy.MaxNumBytes);
			}
		}
        public virtual long IncomingMessageSize
        {
            get
            {
                return IncomingMessagesSize;
            }
        }

        public virtual int TotalIncomingMessages
        {
            get
            {
                return IncomingMessages.Count;
            }
        }

        protected internal virtual TaskCompletionSource<Task> DoAcknowledgeWithTxn(IMessageId messageId, AckType ackType, IDictionary<string, long> properties, IActorRef txn)
        {
            var ackFuture = new TaskCompletionSource<Task>(TaskCreationOptions.RunContinuationsAsynchronously);
            if (txn != null)
            {
                
                txn.Ask(new RegisterAckedTopic(Topic, Subscription))
                    .ContinueWith(task =>
                    {
                        var msgid = messageId;
                        if (!task.IsFaulted)
                        {
                            DoAcknowledge(msgid, ackType, properties, txn);
                            ackFuture.TrySetResult(null);
                        }
                        else
                            ackFuture.TrySetException(task.Exception);

                    }).ContinueWith(task =>
                    {
                        txn.Tell(new RegisterAckOp(ackFuture));
                    });
            }
            else
            {
                DoAcknowledge(messageId, ackType, properties, txn);
                ackFuture.TrySetResult(null);
            }
            return ackFuture;
        }
        protected internal virtual TaskCompletionSource<Task> DoAcknowledgeWithTxn(IList<IMessageId> messageIdList, AckType ackType, IDictionary<string, long> properties, IActorRef txn)
        {
            var ackFuture = new TaskCompletionSource<Task>(TaskCreationOptions.RunContinuationsAsynchronously);
            if (txn != null)
            {
                txn.Ask(new RegisterAckedTopic(Topic, Subscription)).ContinueWith(task =>
                {
                    var msgidList = messageIdList;
                    if (task.Exception != null)
                    {
                        DoAcknowledge(msgidList, ackType, properties, txn);
                        ackFuture.TrySetResult(null);
                    }
                    else
                        ackFuture.TrySetException(task.Exception);

                }).ContinueWith(task =>
                {
                    txn.Tell(new RegisterAckOp(ackFuture));
                });
            }
            else
            {
                DoAcknowledge(messageIdList, ackType, properties, txn);
                ackFuture.TrySetResult(null);
            }
            return ackFuture;
        }
        /// <summary>
		/// Redelivers the given unacknowledged messages. In Failover mode, the request is ignored if the consumer is not
		/// active for the given topic. In Shared mode, the consumers messages to be redelivered are distributed across all
		/// the connected consumers. This is a non blocking call and doesn't throw an exception. In case the connection
		/// breaks, the messages are redelivered after reconnect.
		/// </summary>
		protected internal abstract void RedeliverUnacknowledgedMessages(ISet<IMessageId> messageIds);
        protected internal abstract void RedeliverUnacknowledgedMessages();
        protected internal abstract void  DoAcknowledge(IMessageId messageId, AckType ackType, IDictionary<string, long> properties, IActorRef txn);

        protected internal abstract void DoAcknowledge(IList<IMessageId> messageIdList, AckType ackType, IDictionary<string, long> properties, IActorRef txn);

        protected internal abstract TaskCompletionSource<object> DoReconsumeLater(IMessage<T> message, AckType ackType, IDictionary<string, string> properties, TimeSpan delayTime);

        protected internal abstract void CompleteOpBatchReceive(OpBatchReceive op);
        
        // If message consumer epoch is smaller than consumer epoch present that
        // it has been sent to the client before the user calls redeliverUnacknowledgedMessages, this message is invalid.
        // so we should release this message and receive again
        protected internal virtual bool IsValidConsumerEpoch(Message<T> message)
        {
            if ((SubType == SubType.Failover || SubType == SubType.Exclusive) && message.ConsumerEpoch != _commands.DefaultConsumerEpoch && message.ConsumerEpoch < ConsumerEpoch)
            {
                _log.Warning($"Consumer filter old epoch message, topic : [{Topic}], messageId : [{message.MessageId}], consumerEpoch : [{ConsumerEpoch}]");

                message.Recycle();
                return false;
            }
            return true;
        }
        public virtual bool HasBatchReceiveTimeout()
        {
            return BatchReceiveTimeout != null;
        }
        protected internal virtual bool EnqueueMessageAndCheckBatchReceive(IMessage<T> message)
        {
            if (CanEnqueueMessage(message))
            {
                // After we have enqueued the messages on `incomingMessages` queue, we cannot touch the message instance
                // anymore, since for pooled messages, this instance was possibly already been released and recycled.
                Push(message);
                UpdateAutoScaleReceiverQueueHint();
            }
            return HasEnoughMessagesForBatchReceive();
        }
        protected internal abstract void UpdateAutoScaleReceiverQueueHint();
        private void Push(IMessage<T> o)
        {
            //var o = (Message<T>)obj;
            if (HasParentConsumer)
            {
                //IncomingMessages.Post(o);
                Context.Parent.Tell(new ReceivedMessage<T>(o));
                _log.Info($"Pushed message with sequnceid {o.SequenceId} (topic:{Topic}) to consumer parent");

            }
            else
            {
                if (IncomingMessages.Post(o))
                    _log.Info($"Added message with sequnceid {o.SequenceId} (key:{o.Key}) to IncomingMessages. Message Count: {IncomingMessages.Count}");
                else
                    _log.Info($"Failed to add message with sequnceid {o.SequenceId} to IncomingMessages");
            }
        }
    }
	internal class ConsumerStateActor: ReceiveActor
    {
		private HandlerState.State _state;
		public ConsumerStateActor()
        {
			Receive<SetConumerState>(m => 
			{
				_state = m.State;
			});
			Receive<GetHandlerState>(_ => 
			{
				Sender.Tell(new AskResponse(_state));
			});
        }
    }
	internal sealed class SendState
    {
		public static SendState Instance = new SendState();
    }
	internal class SetConumerState
    {
		public HandlerState.State State { get; }
        public SetConumerState(HandlerState.State state)
        {
			State = state;
        }
    }
}