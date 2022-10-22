using Akka.Actor;
using Akka.Dispatch;
using Akka.Event;
using Akka.Util;
using Akka.Util.Internal;
using Avro.IO;
using DotNetty.Common.Utilities;
using NLog.Fluent;
using SharpPulsar.Batch.Api;
using SharpPulsar.Common.Enum;
using SharpPulsar.Configuration;
using SharpPulsar.Exceptions;
using SharpPulsar.Interfaces;
using SharpPulsar.Messages.Consumer;
using SharpPulsar.Messages.Requests;
using SharpPulsar.Protocol;
using SharpPulsar.Protocol.Proto;
using SharpPulsar.Stats.Consumer.Api;
using SharpPulsar.Utility;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using static SharpPulsar.Exceptions.PulsarClientException;
using static SharpPulsar.Protocol.Proto.CommandSubscribe;

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
        protected readonly ILoggingAdapter _log;
		private readonly string _subscription;
		protected internal readonly ConsumerConfigurationData<T> Conf;
		private readonly string _consumerName;
		protected internal readonly IMessageListener<T> Listener;
		protected internal readonly IConsumerEventListener ConsumerEventListener;
		protected internal BufferBlock<IMessage<T>> IncomingMessages;
		protected internal IActorRef UnAckedChunckedMessageIdSequenceMap;

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
        private bool _internalPinnedExecutor;
        private bool _externalPinnedExecutor;
        private static ActorTaskScheduler _actorTaskScheduler;
        public ConsumerActorBase(IActorRef stateActor, IActorRef lookup, IActorRef connectionPool, string topic, ConsumerConfigurationData<T> conf, int receiverQueueSize, ISchema<T> schema, TaskCompletionSource<IActorRef> subscribeFuture, bool internalPinnedExecutor = false, bool externalPinnedExecutor = false)
		{
            SubscribeFuture = subscribeFuture;
            if (conf.Interceptors != null && conf.Interceptors.Count > 0)
                Interceptors = new ConsumerInterceptors<T>(Context.GetLogger(), conf.Interceptors);
            _externalPinnedExecutor = externalPinnedExecutor;
            _internalPinnedExecutor = internalPinnedExecutor;
            StateActor = stateActor;
			_topic = topic;
			_consumerName = conf.ConsumerName ?? Utility.ConsumerName.GenerateRandomName();
			State = new HandlerState(lookup, connectionPool, topic, Context.System, _consumerName);
			_log = Context.GetLogger();
			MaxReceiverQueueSize = receiverQueueSize;
			_subscription = conf.SubscriptionName;
			Conf = conf;
			Listener = conf.MessageListener;
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
		}
        protected internal virtual void TriggerBatchReceiveTimeoutTask()
        {
            if (!HasBatchReceiveTimeout() && BatchReceivePolicy.TimeoutMs > 0)
            {
                BatchReceiveTimeout = ClientConflict.Timer().newTimeout(this.pendingBatchReceiveTask, BatchReceivePolicy.getTimeoutMs(), TimeUnit.MILLISECONDS);
            }
        }

        public virtual void InitReceiverQueueSize()
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
            int OldSize = CurrentReceiverQueueSize;
            int NewSize = Math.Max(MinReceiverQueueSize(), OldSize / 2);
            if (OldSize > NewSize)
            {
                CurrentReceiverQueueSize = NewSize;
            }
        }
        protected bool VerifyBatchReceive()
        {
            if (Listener != null)
            {
                Sender.Tell(new AskResponse(new InvalidConfigurationException("Cannot use receive() when a listener has been set")));
                return false;
            }
            if (CurrentReceiverQueueSize == 0)
            {
                Sender.Tell(new AskResponse(new InvalidConfigurationException("Can't use batch receive, if the queue size is 0")));
                return false;
            }
            return true;
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
        protected void BatchReceive()
        {
            if(VerifyConsumerState())
            {
                if(VerifyBatchReceive())
                {
                    if (HasEnoughMessagesForBatchReceive())
                    {
                        var messages = new Messages<T>(BatchReceivePolicy.MaxNumMessages, BatchReceivePolicy.MaxNumBytes);

                        while (IncomingMessages.TryReceive(out var message) && messages.CanAdd(message))
                        {
                            Self.Tell(new MessageProcessed<T>(message));

                            if (!IsValidConsumerEpoch(message))
                                continue;

                            messages.Add(BeforeConsume(message));
                        }
                        Sender.Tell(new AskResponse(messages));
                    }
                    else
                        Sender.Tell(new AskResponse());
                }
            }
        }
        
        protected void Receive()
        {
            if(VerifyConsumerState())
            {
                if (Conf.ReceiverQueueSize == 0)
                {
                    Sender.Tell(new AskResponse(new InvalidConfigurationException("Can't use receive with timeout, if the queue size is 0")));
                    return;
                }
                if (Conf.MessageListener != null)
                {
                    Sender.Tell(new AskResponse(new InvalidConfigurationException("Cannot use receive() when a listener has been set")));
                    return;
                }
                if (IncomingMessages.TryReceive(out var message))
                {
                    Self.Tell(new MessageProcessed<T>(message));
                    if (!IsValidConsumerEpoch(message))
                    {
                        Receive();
                        return;
                    }
                    Sender.Tell(new AskResponse(BeforeConsume(message)));
                }
                else
                    Sender.Tell(new AskResponse());
            }
        }
       
        
        private bool VerifyConsumerState()
        {
            var state = State.ConnectionState;
            switch (state)
            {
                case HandlerState.State.Ready:
                case HandlerState.State.Connecting:
                    return true;
                case HandlerState.State.Closing:
                case HandlerState.State.Closed:
                   Sender.Tell(new AskResponse(new AlreadyClosedException("Consumer already closed")));
                    return false;
                case HandlerState.State.Terminated:
                    Sender.Tell(new AskResponse(new TopicTerminatedException("Topic was terminated")));
                    return false;
                case HandlerState.State.Failed:
                case HandlerState.State.Uninitialized:
                default:
                    Sender.Tell(new AskResponse(new NotConnectedException()));
                    return false;
            }
        }
        private bool HasEnoughMessagesForBatchReceive()
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

		internal virtual void NegativeAcknowledge(IMessages<T> messages)
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

		/// <summary>
		/// Redelivers the given unacknowledged messages. In Failover mode, the request is ignored if the consumer is not
		/// active for the given topic. In Shared mode, the consumers messages to be redelivered are distributed across all
		/// the connected consumers. This is a non blocking call and doesn't throw an exception. In case the connection
		/// breaks, the messages are redelivered after reconnect.
		/// </summary>
		protected internal abstract Task RedeliverUnacknowledged(ISet<IMessageId> messageIds);

		protected internal virtual void OnAcknowledge(IMessageId messageId, Exception exception)
		{
			if (Interceptors != null)
			{
				Interceptors.OnAcknowledge(Self, messageId, exception);
			}
		}

		protected internal virtual void OnAcknowledgeCumulative(IMessageId messageId, Exception exception)
		{
			if (Interceptors != null)
			{
				Interceptors.OnAcknowledgeCumulative(Self, messageId, exception);
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
        protected internal virtual void CompletePendingReceive(TaskCompletionSource<IMessage<T>> receivedFuture, IMessage<T> message)
        {
            ActorTaskScheduler.RunTask(() => { });
            GetInternalExecutor(Message).execute(() =>
            {
                if (!ReceivedFuture.complete(Message))
                {
                    log.warn("Race condition detected. receive future was already completed (cancelled={}) and message was " + "dropped. message={}", ReceivedFuture.isCancelled(), Message);
                }
            });
        }
        protected internal virtual TaskCompletionSource<IMessage<T>> NextPendingReceive()
        {
            TaskCompletionSource<IMessage<T>> receivedFuture;
            do
            {
                receivedFuture = PendingReceives.poll();
                // skip done futures (cancelling a future could mark it done)
            } while (receivedFuture != null && receivedFuture.Task.IsCompleted);
            return receivedFuture;
        }

        protected internal abstract IMessage<T> InternalReceive();

        protected internal abstract TaskCompletionSource<IMessage<T>> InternalReceiveAsync();
        protected internal abstract IMessage<T> InternalReceive(TimeSpan timeOut);
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
        internal virtual Option<MemoryLimitController> MemoryLimitController
        {
            get
            {
                if (!Conf.IsAutoScaledReceiverQueueSizeEnabled)
                {
                    // disable memory limit.
                    return null;
                }
                else
                {
                    return ClientConflict.MemoryLimitController;
                }
            }
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
            ActorTaskScheduler.RunTask(() => DoPendingBatchReceiveTask(time));
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

                        _log.Error("Race condition in consumer {} (should not cause data loss). " + " Concurrent operations on pendingBatchReceives is not safe", this.ConsumerNameConflict);
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
                BatchReceiveTimeout = ClientConflict.Timer().newTimeout(this.pendingBatchReceiveTask, TimeToWaitMs, TimeUnit.MILLISECONDS);
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
        protected internal virtual void ResetIncomingMessageSize()
        {
            IncomingMessagesSize = 0;
            long oldSize = IncomingMessagesSize;
           // MemoryLimitController..ifPresent(limiter => limiter.releaseMemory(OldSize));
        }

        protected internal virtual void DecreaseIncomingMessageSize<T1>(in IMessage<T> message)
        {
            IncomingMessagesSize -= message.Size();
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

        protected internal virtual void ClearIncomingMessages()
        {
            // release messages if they are pooled messages
            IncomingMessages.forEach(Message.release);
            IncomingMessages.TryReceiveAll(out var _);
            ResetIncomingMessageSize();
        }
        protected internal abstract void CompleteOpBatchReceive(OpBatchReceive op);
        private IActorRef GetExternalExecutor(IMessage<T> msg)
        {
            var receivedConsumer = (msg is TopicMessage<T>) ? ((TopicMessage<T>)msg).ReceivedByConsumer : null;
           
            var executor = receivedConsumer != null ? receivedConsumer : Self;
            return executor;
        }

        private IActorRef GetInternalExecutor(IMessage<T> msg)
        {
            var receivedConsumer = (msg is TopicMessage<T>) ? ((TopicMessage<T>)msg).ReceivedByConsumer : null;

            var executor = receivedConsumer != null ? receivedConsumer : Self;
            return executor;
        }
        // If message consumer epoch is smaller than consumer epoch present that
        // it has been sent to the client before the user calls redeliverUnacknowledgedMessages, this message is invalid.
        // so we should release this message and receive again
        protected internal virtual bool IsValidConsumerEpoch(IMessage<T> msg)
        {
            var message = msg is Message<T> ? (Message<T>)msg : (Message<T>)((TopicMessage<T>)msg).Message;

            if ((SubType == SubType.Failover || SubType == SubType.Exclusive) && message.ConsumerEpoch != Commands.DefaultConsumerEpoch && message.ConsumerEpoch < ConsumerEpoch)
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