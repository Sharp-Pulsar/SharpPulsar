using Akka.Actor;
using Akka.Event;
using Akka.Util.Internal;
using BAMCIS.Util.Concurrent;
using SharpPulsar.Akka;
using SharpPulsar.Batch.Api;
using SharpPulsar.Common;
using SharpPulsar.Configuration;
using SharpPulsar.Exceptions;
using SharpPulsar.Interfaces;
using SharpPulsar.Interfaces.Transaction;
using SharpPulsar.Stats.Consumer.Api;
using SharpPulsar.Utility;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using static SharpPulsar.Protocol.Proto.CommandAck;
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
		internal abstract long LastDisconnectedTimestamp { get; }
		internal abstract void NegativeAcknowledge(IMessageId messageId);
		internal abstract void Resume();
		internal abstract void Pause();
		internal abstract bool Connected { get; }
		internal abstract void Seek(long timestamp);
		internal abstract void Seek(IMessageId messageId);
		internal abstract void RedeliverUnacknowledgedMessages();
		internal abstract bool HasReachedEndOfTopic();
		internal abstract IConsumerStats Stats { get; }

		internal enum ConsumerType
		{
			PARTITIONED,
			NonPartitioned
		}
		private readonly ILoggingAdapter _log;
		private readonly string _subscription;
		protected internal readonly ConsumerConfigurationData<T> Conf;
		private readonly string _consumerName;
		protected internal readonly IMessageListener<T> Listener;
		protected internal readonly IConsumerEventListener ConsumerEventListener;
		protected internal readonly IAdvancedScheduler ListenerExecutor;
		internal readonly BlockingCollection<IMessage<T>> IncomingMessages;
		protected internal ConcurrentDictionary<IMessageId, IMessageId[]> UnAckedChunckedMessageIdSequenceMap;
		protected internal readonly ConcurrentQueue<IMessage<T>> PendingReceives;

		protected internal int MaxReceiverQueueSizeConflict;
		protected internal readonly ISchema<T> Schema;
		protected internal readonly ConsumerInterceptors<T> Interceptors;
		protected internal readonly BatchReceivePolicy BatchReceivePolicy;
		protected internal ConcurrentQueue<OpBatchReceive<T>> PendingBatchReceives;
		protected internal long IncomingMessagesSize = 0;
		protected internal ICancelable BatchReceiveTimeout = null;
		protected internal HandlerState State;

		protected internal ConsumerActorBase(IActorRef client, string topic, ConsumerConfigurationData<T> conf, int receiverQueueSize, IAdvancedScheduler listenerExecutor, ISchema<T> schema, ConsumerInterceptors<T> interceptors)
		{
			State = new HandlerState(client, topic, Context.System, "");
			_log = Context.GetLogger();
			MaxReceiverQueueSizeConflict = receiverQueueSize;
			_subscription = conf.SubscriptionName;
			Conf = conf;
			_consumerName = conf.ConsumerName ?? Utility.ConsumerName.GenerateRandomName();
			Listener = conf.MessageListener;
			ConsumerEventListener = conf.ConsumerEventListener;

			IncomingMessages = new BlockingCollection<IMessage<T>>();
			UnAckedChunckedMessageIdSequenceMap = new ConcurrentDictionary<IMessageId, IMessageId[]>();

			ListenerExecutor = listenerExecutor;
			PendingReceives = new ConcurrentQueue<IMessage<T>>();
			Schema = schema;
			Interceptors = interceptors;
			if (conf.BatchReceivePolicy != null)
			{
				BatchReceivePolicy userBatchReceivePolicy = conf.BatchReceivePolicy;
				if (userBatchReceivePolicy.MaxNumMessages > MaxReceiverQueueSizeConflict)
				{
					BatchReceivePolicy = new BatchReceivePolicy.Builder().MaxNumMessages(MaxReceiverQueueSizeConflict).MaxNumBytes(userBatchReceivePolicy.MaxNumBytes).Timeout((int)TimeUnit.MILLISECONDS.ToMilliseconds(userBatchReceivePolicy.TimeoutMs)).Build();
					_log.Warning($"BatchReceivePolicy maxNumMessages: {userBatchReceivePolicy.MaxNumMessages} is greater than maxReceiverQueueSize: {MaxReceiverQueueSizeConflict}, reset to maxReceiverQueueSize. batchReceivePolicy: {BatchReceivePolicy}");
				}
				else if (userBatchReceivePolicy.MaxNumMessages <= 0 && userBatchReceivePolicy.MaxNumBytes <= 0)
				{
					BatchReceivePolicy = new BatchReceivePolicy.Builder().MaxNumMessages(BatchReceivePolicy.DefaultPolicy.MaxNumMessages).MaxNumBytes(BatchReceivePolicy.DefaultPolicy.MaxNumBytes).Timeout((int)TimeUnit.MILLISECONDS.ToMilliseconds(userBatchReceivePolicy.TimeoutMs)).Build();
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

			if (BatchReceivePolicy.TimeoutMs > 0)
			{
				BatchReceiveTimeout = ListenerExecutor.ScheduleOnceCancelable(TimeSpan.FromMilliseconds(TimeUnit.MILLISECONDS.ToMilliseconds(BatchReceivePolicy.TimeoutMs)), PendingBatchReceiveTask);
				
			}
		}

		
		internal virtual void Receive()
		{
			if (Listener != null)
			{
				throw new PulsarClientException.InvalidConfigurationException("Cannot use receive() when a listener has been set");
			}
			VerifyConsumerState();
			InternalReceive();
		}

		
		protected internal abstract void InternalReceive();

		
		internal virtual void Receive(int timeout, TimeUnit unit)
		{
			if (Conf.ReceiverQueueSize == 0)
			{
				throw new PulsarClientException.InvalidConfigurationException("Can't use receive with timeout, if the queue size is 0");
			}
			if (Listener != null)
			{
				throw new PulsarClientException.InvalidConfigurationException("Cannot use receive() when a listener has been set");
			}

			VerifyConsumerState();
			InternalReceive(timeout, unit);
		}

		protected internal abstract void InternalReceive(int timeout, TimeUnit unit);

		
		internal virtual void BatchReceive()
		{
			VerifyBatchReceive();
			VerifyConsumerState();
			InternalBatchReceive();
		}

		internal virtual async void BatchReceiveAsync()
		{
			var task = new TaskCompletionSource<IMessages<T>>();
			try
			{
				VerifyBatchReceive();
				VerifyConsumerState();
				return await InternalBatchReceiveAsync();
			}
			catch (PulsarClientException e)
			{
				task.SetException(e);
				return await task.Task;
			}
		}

		protected internal virtual void FailPendingReceives(ConcurrentQueue<IMessage<T>> pendingReceives)
		{
			while (!pendingReceives.IsEmpty)
			{
				if (!pendingReceives.TryDequeue(out var receive))
				{
					break;
				}
				/*if (!receiveFuture.Done)
				{
					receiveFuture.completeExceptionally(new PulsarClientException.AlreadyClosedException(string.Format("The consumer which subscribes the topic {0} with subscription name {1} " + "was already closed when cleaning and closing the consumers", Topic, _subscription)));
				}*/
			}
		}

		protected internal virtual void FailPendingBatchReceives(ConcurrentQueue<OpBatchReceive<T>> pendingBatchReceives)
		{
			while (!pendingBatchReceives.IsEmpty)
			{
				OpBatchReceive<T> opBatchReceive = pendingBatchReceives.poll();
				if (opBatchReceive == null || opBatchReceive.Future == null)
				{
					break;
				}
				if (!opBatchReceive.Future.Done)
				{
					opBatchReceive.Future.completeExceptionally(new PulsarClientException.AlreadyClosedException(string.Format("The consumer which subscribes the topic {0} with subscription name {1} " + "was already closed when cleaning and closing the consumers", Topic, _subscription)));
				}
			}
		}

		protected internal abstract void InternalBatchReceive();

		protected internal abstract void InternalBatchReceiveAsync();

		
		internal virtual void Acknowledge<T1>(IMessage<T1> message)
		{
			try
			{
				Acknowledge(message.MessageId);
			}
			catch (NullReferenceException npe)
			{
				throw new PulsarClientException.InvalidMessageException(npe.Message);
			}
		}

		
		internal virtual void Acknowledge(IMessageId messageId)
		{
			try
			{
				AcknowledgeAsync(messageId);
			}
			catch (Exception e)
			{
				throw PulsarClientException.Unwrap(e);
			}
		}

		
		internal virtual void Acknowledge(IList<IMessageId> messageIdList)
		{
			try
			{
				AcknowledgeAsync(messageIdList);
			}
			catch (Exception e)
			{
				throw PulsarClientException.Unwrap(e);
			}
		}

		
		internal virtual void Acknowledge<T1>(IMessages<T1> messages)
		{
			try
			{
				AcknowledgeAsync(messages);
			}
			catch (Exception e)
			{
				throw PulsarClientException.Unwrap(e);
			}
		}

		
		internal virtual void ReconsumeLater<T1>(IMessage<T1> message, long delayTime, TimeUnit unit)
		{
			if (!Conf.RetryEnable)
			{
				throw new PulsarClientException("reconsumeLater method not support!");
			}
			try
			{
				ReconsumeLaterAsync(message, delayTime, unit);
			}
			catch (Exception e)
			{
				Exception t = e.InnerException;
				if (t is PulsarClientException)
				{
					throw (PulsarClientException)t;
				}
				else
				{
					throw new PulsarClientException(t);
				}
			}
		}

		internal virtual void ReconsumeLater<T1>(IMessages<T1> messages, long delayTime, TimeUnit unit)
		{
			try
			{
				ReconsumeLaterAsync(messages, delayTime, unit);
			}
			catch (Exception e)
			{
				throw PulsarClientException.Unwrap(e);
			}
		}

		
		internal virtual void AcknowledgeCumulative<T1>(IMessage<T1> message)
		{
			try
			{
				AcknowledgeCumulative(message.MessageId);
			}
			catch (System.NullReferenceException npe)
			{
				throw new PulsarClientException.InvalidMessageException(npe.Message);
			}
		}

		internal virtual void AcknowledgeCumulative(IMessageId messageId)
		{
			try
			{
				AcknowledgeCumulativeAsync(messageId);
			}
			catch (Exception e)
			{
				throw PulsarClientException.Unwrap(e);
			}
		}

		
		internal virtual void ReconsumeLaterCumulative<T1>(IMessage<T1> message, long delayTime, TimeUnit unit)
		{
			try
			{
				ReconsumeLaterCumulativeAsync(message, delayTime, unit).get();
			}
			catch (Exception e)
			{
				throw PulsarClientException.Unwrap(e);
			}
		}

		internal virtual Task AcknowledgeAsync<T1>(IMessage<T1> message)
		{
			try
			{
				return AcknowledgeAsync(message.MessageId);
			}
			catch (System.NullReferenceException npe)
			{
				return FutureUtil.FailedFuture(new PulsarClientException.InvalidMessageException(npe.Message));
			}
		}

		internal virtual Task AcknowledgeAsync<T1>(IMessages<T1> messages)
		{
			try
			{
				messages.ForEach(x => AcknowledgeAsync(x));
				return CompletableFuture.completedFuture(null);
			}
			catch (System.NullReferenceException npe)
			{
				return FutureUtil.FailedFuture(new PulsarClientException.InvalidMessageException(npe.Message));
			}
		}

		internal virtual Task AcknowledgeAsync(IList<IMessageId> messageIdList)
		{
			return DoAcknowledgeWithTxn(messageIdList, AckType.Individual, new Dictionary<string, long>(), null);
		}

		internal virtual Task ReconsumeLaterAsync<T1>(IMessage<T1> message, long delayTime, TimeUnit unit)
		{
			if (!Conf.RetryEnable)
			{
				return FutureUtil.FailedFuture(new PulsarClientException("reconsumeLater method not support!"));
			}
			try
			{
				return DoReconsumeLater(message, AckType.Individual, new Dictionary<string, long>(), delayTime, unit);
			}
			catch (System.NullReferenceException npe)
			{
				return FutureUtil.FailedFuture(new PulsarClientException.InvalidMessageException(npe.Message));
			}
		}

		internal virtual CompletableFuture<Void> ReconsumeLaterAsync<T1>(IMessages<T1> messages, long delayTime, TimeUnit unit)
		{
			try
			{
				messages.ForEach(message => ReconsumeLaterAsync(message, delayTime, unit));
				return CompletableFuture.completedFuture(null);
			}
			catch (NullReferenceException npe)
			{
				return FutureUtil.FailedFuture(new PulsarClientException.InvalidMessageException(npe.Message));
			}
		}

		internal virtual Task AcknowledgeCumulativeAsync<T1>(IMessage<T1> message)
		{
			try
			{
				return AcknowledgeCumulativeAsync(message.MessageId);
			}
			catch (System.NullReferenceException npe)
			{
				return FutureUtil.FailedFuture(new PulsarClientException.InvalidMessageException(npe.Message));
			}
		}

		internal virtual Task ReconsumeLaterCumulativeAsync<T1>(IMessage<T1> message, long delayTime, TimeUnit unit)
		{
			if (!Conf.RetryEnable)
			{
				return FutureUtil.FailedFuture(new PulsarClientException("reconsumeLater method not support!"));
			}
			if (!IsCumulativeAcknowledgementAllowed(Conf.SubscriptionType))
			{
				return FutureUtil.FailedFuture(new PulsarClientException.InvalidConfigurationException("Cannot use cumulative acks on a non-exclusive subscription"));
			}
			return DoReconsumeLater(message, AckType.Cumulative, new Dictionary<string, long>(), delayTime, unit);
		}

		internal virtual Task AcknowledgeAsync(IMessageId messageId)
		{
			return AcknowledgeAsync(messageId, null);
		}

		internal virtual Task AcknowledgeAsync(IMessageId messageId, ITransaction txn)
		{
			TransactionImpl txnImpl = null;
			if (null != txn)
			{
				checkArgument(txn is TransactionImpl);
				txnImpl = (TransactionImpl)txn;
			}
			return DoAcknowledgeWithTxn(messageId, AckType.Individual, new Dictionary<string, long>(), txnImpl);
		}

		internal virtual Task AcknowledgeCumulativeAsync(IMessageId messageId)
		{
			return AcknowledgeCumulativeAsync(messageId, null);
		}

		internal virtual Task AcknowledgeCumulativeAsync(IMessageId messageId, ITransaction txn)
		{
			if (!IsCumulativeAcknowledgementAllowed(Conf.SubscriptionType))
			{
				return FutureUtil.FailedFuture(new PulsarClientException.InvalidConfigurationException("Cannot use cumulative acks on a non-exclusive/non-failover subscription"));
			}

			TransactionImpl txnImpl = null;
			if (null != txn)
			{
				checkArgument(txn is TransactionImpl);
				txnImpl = (TransactionImpl)txn;
			}
			return DoAcknowledgeWithTxn(messageId, AckType.Cumulative, new Dictionary<string, long>(), txnImpl);
		}

		internal virtual void NegativeAcknowledge<T1>(IMessage<T1> message)
		{
			NegativeAcknowledge(message.MessageId);
		}

		protected internal virtual Task DoAcknowledgeWithTxn(IList<IMessageId> messageIdList, AckType ackType, IDictionary<string, long> properties, TransactionImpl txn)
		{
			CompletableFuture<Void> ackFuture;
			if (txn != null)
			{
				ackFuture = txn.RegisterAckedTopic(Topic, _subscription).thenCompose(ignored => DoAcknowledge(messageIdList, ackType, properties, txn));
				txn.RegisterAckOp(ackFuture);
			}
			else
			{
				ackFuture = DoAcknowledge(messageIdList, ackType, properties, txn);
			}
			return ackFuture;
		}

		protected internal virtual Task DoAcknowledgeWithTxn(IMessageId messageId, AckType ackType, IDictionary<string, long> properties, TransactionImpl txn)
		{
			CompletableFuture<Void> ackFuture;
			if (txn != null && (this is ConsumerImpl))
			{
				// it is okay that we register acked topic after sending the acknowledgements. because
				// the transactional ack will not be visiable for consumers until the transaction is
				// committed
				if (ackType == AckType.Cumulative)
				{
					txn.RegisterCumulativeAckConsumer((ConsumerImpl<object>)this);
				}

				ackFuture = txn.RegisterAckedTopic(Topic, _subscription).thenCompose(ignored => DoAcknowledge(messageId, ackType, properties, txn));
				// register the ackFuture as part of the transaction
				txn.RegisterAckOp(ackFuture);
				return ackFuture;
			}
			else
			{
				ackFuture = DoAcknowledge(messageId, ackType, properties, txn);
			}
			return ackFuture;
		}

		protected internal abstract Task DoAcknowledge(IMessageId messageId, AckType ackType, IDictionary<string, long> properties, TransactionImpl txn);

		protected internal abstract Task DoAcknowledge(IList<IMessageId> messageIdList, AckType ackType, IDictionary<string, long> properties, TransactionImpl txn);

		protected internal abstract Task DoReconsumeLater<T1>(IMessage<T1> message, AckType ackType, IDictionary<string, long> properties, long delayTime, TimeUnit unit);

		internal virtual void NegativeAcknowledge<T1>(IMessages<T1> messages)
		{
			messages.ForEach(negativeAcknowledge);
		}

		internal virtual void close()
		{
			try
			{
				CloseAsync();
			}
			catch (Exception e)
			{
				throw PulsarClientException.Unwrap(e);
			}
		}

		internal virtual Task CloseAsync()
        {
			return null;
        }
		
		internal virtual IMessageId LastMessageId
		{
			get
			{
				try
				{
					return LastMessageIdAsync;
				}
				catch (Exception e)
				{
					throw PulsarClientException.Unwrap(e);
				}
			}
		}

		internal virtual Task<MessageId> LastMessageIdAsync { get; }

		private bool IsCumulativeAcknowledgementAllowed(SubscriptionType type)
		{
			return SubscriptionType.Shared != type && SubscriptionType.KeyShared != type;
		}

		protected internal virtual SubType SubType
		{
			get
			{
				SubscriptionType type = (SubscriptionType)Conf.SubscriptionType;
				switch (type)
				{
					case SubscriptionType.Exclusive:
						return SubType.Exclusive;

					case SubscriptionType.Shared:
						return SubType.Shared;

					case SubscriptionType.Failover:
						return SubType.Failover;

					case SubscriptionType.KeyShared:
						return SubType.KeyShared;
				}

				// Should not happen since we cover all cases above
				return null;
			}
		}

		internal abstract int AvailablePermits { get; }

		internal abstract int NumMessagesInQueue();

		internal virtual Task<IConsumer<T>> SubscribeFuture()
		{
			return SubscribeFutureConflict;
		}

		internal virtual string Topic
		{
			get
			{
				return Topic;
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
		protected internal abstract void RedeliverUnacknowledgedMessages(ISet<IMessageId> messageIds);

		internal override string ToString()
		{
			return "ConsumerBase{" + "subscription='" + _subscription + '\'' + ", consumerName='" + _consumerName + '\'' + ", topic='" + Topic + '\'' + '}';
		}

		protected internal virtual int MaxReceiverQueueSize
		{
			set
			{
				MaxReceiverQueueSizeConflict = value;
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

		protected internal virtual bool CanEnqueueMessage(IMessage<T> message)
		{
			// Default behavior, can be overridden in subclasses
			return true;
		}

		protected internal virtual bool EnqueueMessageAndCheckBatchReceive(IMessage<T> message)
		{
			if (CanEnqueueMessage(message) && IncomingMessages.TryAdd(message))
			{
				var size = message.Data == null ? 0 : message.Data.Length;
				IncomingMessagesSize += size;
			}
			return HasEnoughMessagesForBatchReceive();
		}

		protected internal virtual bool HasEnoughMessagesForBatchReceive()
		{
			if (BatchReceivePolicy.MaxNumMessages <= 0 && BatchReceivePolicy.MaxNumBytes <= 0)
			{
				return false;
			}
			return (BatchReceivePolicy.MaxNumMessages > 0 && IncomingMessages.Count >= BatchReceivePolicy.MaxNumMessages) || (BatchReceivePolicy.MaxNumBytes > 0 && IncomingMessagesSize >= BatchReceivePolicy.MaxNumBytes);
		}

		private void VerifyConsumerState()
		{
			switch (State)
			{
				case Ready:
				case Connecting:
					break; // Ok
					goto case Closing;
				case Closing:
				case Closed:
					throw new PulsarClientException.AlreadyClosedException("Consumer already closed");
				case Terminated:
					throw new PulsarClientException.AlreadyClosedException("Topic was terminated");
				case Failed:
				case Uninitialized:
					throw new PulsarClientException.NotConnectedException();
				default:
					break;
			}
		}
		private void VerifyBatchReceive()
		{
			if (Listener != null)
			{
				throw new PulsarClientException.InvalidConfigurationException("Cannot use receive() when a listener has been set");
			}
			if (Conf.ReceiverQueueSize == 0)
			{
				throw new PulsarClientException.InvalidConfigurationException("Can't use batch receive, if the queue size is 0");
			}
		}

		protected internal sealed class OpBatchReceive<T>
		{

			internal readonly CompletableFuture<Messages<T>> Future;
			internal readonly long CreatedAt;

			internal OpBatchReceive(CompletableFuture<Messages<T>> future)
			{
				Future = future;
				CreatedAt = System.nanoTime();
			}

			internal static OpBatchReceive<T> Of<T>(CompletableFuture<Messages<T>> future)
			{
				return new OpBatchReceive<T>(future);
			}
		}

		protected internal virtual void NotifyPendingBatchReceivedCallBack()
		{
			OpBatchReceive<T> opBatchReceive = PollNextBatchReceive();
			if (opBatchReceive == null)
			{
				return;
			}
			try
			{
				ReentrantLock.@lock();
				NotifyPendingBatchReceivedCallBack(opBatchReceive);
			}
			finally
			{
				ReentrantLock.unlock();
			}
		}

		private OpBatchReceive<T> PeekNextBatchReceive()
		{
			OpBatchReceive<T> opBatchReceive = null;
			while (opBatchReceive == null)
			{
				opBatchReceive = PendingBatchReceives.peek();
				// no entry available
				if (opBatchReceive == null)
				{
					return null;
				}
				// remove entries where future is null or has been completed (cancel / timeout)
				if (opBatchReceive.Future == null || opBatchReceive.Future.Done)
				{
					OpBatchReceive<T> removed = PendingBatchReceives.poll();
					if (removed != opBatchReceive)
					{
						_log.error("Bug: Removed entry wasn't the expected one. expected={}, removed={}", opBatchReceive, removed);
					}
					opBatchReceive = null;
				}
			}
			return opBatchReceive;
		}


		private OpBatchReceive<T> PollNextBatchReceive()
		{
			OpBatchReceive<T> opBatchReceive = null;
			while (opBatchReceive == null)
			{
				opBatchReceive = PendingBatchReceives.poll();
				// no entry available
				if (opBatchReceive == null)
				{
					return null;
				}
				// skip entries where future is null or has been completed (cancel / timeout)
				if (opBatchReceive.Future == null || opBatchReceive.Future.Done)
				{
					opBatchReceive = null;
				}
			}
			return opBatchReceive;
		}

		protected internal void NotifyPendingBatchReceivedCallBack(OpBatchReceive<T> opBatchReceive)
		{
			MessagesImpl<T> messages = NewMessagesImpl;
			Message<T> msgPeeked = IncomingMessages.peek();
			while (msgPeeked != null && messages.CanAdd(msgPeeked))
			{
				Message<T> msg = IncomingMessages.poll();
				if (msg != null)
				{
					MessageProcessed(msg);
					Message<T> interceptMsg = BeforeConsume(msg);
					messages.Add(interceptMsg);
				}
				msgPeeked = IncomingMessages.peek();
			}
			CompletePendingBatchReceive(opBatchReceive.Future, messages);
		}

		protected internal virtual void CompletePendingBatchReceive(CompletableFuture<Messages<T>> future, Messages<T> messages)
		{
			if (!future.complete(messages))
			{
				_log.warn("Race condition detected. batch receive future was already completed (cancelled={}) and messages were dropped. messages={}", future.Cancelled, messages);
			}
		}

		protected internal abstract void MessageProcessed<T1>(IMessage<T1> msg);

		private void PendingBatchReceiveTask()
		{
			if (BatchReceiveTimeout.IsCancellationRequested)
			{
				return;
			}

			long timeToWaitMs;

			lock (this)
			{
				// If it's closing/closed we need to ignore this timeout and not schedule next timeout.
				if (State.ConnectionState == HandlerState.State.Closing || State.ConnectionState == HandlerState.State.Closed)
				{
					return;
				}
				if (PendingBatchReceives == null)
				{
					PendingBatchReceives = new ConcurrentQueue<OpBatchReceive<T>>();
				}
				OpBatchReceive<T> firstOpBatchReceive = PeekNextBatchReceive();
				timeToWaitMs = BatchReceivePolicy.TimeoutMs;

				while (firstOpBatchReceive != null)
				{
					// If there is at least one batch receive, calculate the diff between the batch receive timeout
					// and the elapsed time since the operation was created.
					long diff = BatchReceivePolicy.TimeoutMs - TimeUnit.NANOSECONDS.ToMilliseconds(DateTime.Now.Ticks - firstOpBatchReceive.CreatedAt);
					if (diff <= 0)
					{
						// The diff is less than or equal to zero, meaning that the batch receive has been timed out.
						// complete the OpBatchReceive and continue to check the next OpBatchReceive in pendingBatchReceives.
						OpBatchReceive<T> op = PollNextBatchReceive();
						if (op != null)
						{
							CompleteOpBatchReceive(op);
						}
						firstOpBatchReceive = PeekNextBatchReceive();
					}
					else
					{
						// The diff is greater than zero, set the timeout to the diff value
						timeToWaitMs = diff;
						break;
					}
				}
				BatchReceiveTimeout = ListenerExecutor.ScheduleOnceCancelable(TimeSpan.FromMilliseconds(TimeUnit.MILLISECONDS.ToMilliseconds(timeToWaitMs)), PendingBatchReceiveTask);
			}
		}

		protected internal virtual Messages<T> NewMessages
		{
			get
			{
				return new Messages<T>(BatchReceivePolicy.MaxNumMessages, BatchReceivePolicy.MaxNumBytes);
			}
		}

		protected internal virtual bool HasPendingBatchReceive()
		{
			return PendingBatchReceives != null && PeekNextBatchReceive() != null;
		}

		protected internal abstract void CompleteOpBatchReceive(OpBatchReceive<T> op);
	}

}