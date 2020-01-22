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
    using SharpPulsar.Configuration;
    using System.Threading.Tasks;
    using SharpPulsar.Interface.Consumer;
    using SharpPulsar.Interface.Message;
    using SharpPulsar.Interface.Schema;
    using BAMCIS.Util.Concurrent;
    using SharpPulsar.Impl.Message;
    using SharpPulsar.Util.Collections;
    using SharpPulsar.Exception;
    using static SharpPulsar.Exception.PulsarClientException;
    using SharpPulsar.Interface.Transaction;
    using SharpPulsar.Impl.Transaction;
    using static SharpPulsar.Common.PulsarApi.CommandAck;
    using SharpPulsar.Enum;
    using static SharpPulsar.Common.PulsarApi.CommandSubscribe;
    using DotNetty.Common.Internal;

    public abstract class ConsumerBase<T> : HandlerState, IConsumer<T>
	{

		internal enum ConsumerType
		{
			PARTITIONED,
			NON_PARTITIONED
		}

		protected internal readonly string subscription;
		protected internal readonly ConsumerConfigurationData<T> conf;
		protected internal readonly string consumerName;
		protected internal readonly ValueTask<IConsumer<T>> subscribeAsync;
		protected internal readonly IMessageListener<T> listener;
		protected internal readonly IConsumerEventListener consumerEventListener;
		protected internal readonly ExecutorService listenerExecutor;
		internal readonly GrowableArrayBlockingQueue<IMessage<T>> incomingMessages;
		protected internal readonly ConcurrentLinkedQueue<CompletableFuture<Message<T>>> pendingReceives;
		protected internal int maxReceiverQueueSize;
		protected internal readonly ISchema<T> schema;
		protected internal readonly ConsumerInterceptors<T> interceptors;
		protected internal readonly BatchReceivePolicy batchReceivePolicy;
		protected internal ConcurrentLinkedQueue<OpBatchReceive<T>> pendingBatchReceives;
		protected internal static readonly AtomicLongFieldUpdater<ConsumerBase> INCOMING_MESSAGES_SIZE_UPDATER = AtomicLongFieldUpdater.newUpdater(typeof(ConsumerBase), "incomingMessagesSize");
		protected internal volatile int incomingMessagesSize = 0;
		protected internal volatile Timeout batchReceiveTimeout = null;

		protected internal ConsumerBase(PulsarClientImpl client, string topic, ConsumerConfigurationData<T> conf, int receiverQueueSize, ExecutorService listenerExecutor, CompletableFuture<Consumer<T>> subscribeFuture, ISchema<T> schema, Disposeasync interceptors) : base(client, topic)
		{
			maxReceiverQueueSize = receiverQueueSize;
			subscription = conf.SubscriptionName;
			this.conf = conf;
			consumerName = conf.ConsumerName == null ? ConsumerName.generateRandomName() : conf.ConsumerName;
			subscribeAsync = subscribeFuture;
			listener = conf.MessageListener;
			consumerEventListener = conf.ConsumerEventListener;
			// Always use growable queue since items can exceed the advertised size
			incomingMessages = new GrowableArrayBlockingQueue<IMessage<T>>();

			this.listenerExecutor = listenerExecutor;
			pendingReceives = Queues.newConcurrentLinkedQueue();
			this.schema = schema;
			this.interceptors = interceptors;
			if (conf.BatchReceivePolicy != null)
			{
				batchReceivePolicy = conf.BatchReceivePolicy;
			}
			else
			{
				batchReceivePolicy = BatchReceivePolicy.DEFAULT_POLICY;
			}
			if (batchReceivePolicy.TimeoutMs > 0)
			{
				batchReceiveTimeout = client.timer().newTimeout(this, batchReceivePolicy.TimeoutMs, TimeUnit.MILLISECONDS);
			}
		}


		public IMessage<T> Receive()
		{
			if (listener != null)
			{
				throw new InvalidConfigurationException("Cannot use receive() when a listener has been set");
			}
			VerifyConsumerState();
			return InternalReceive();
		}

		public ValueTask<IMessage<T>> ReceiveAsync()
		{
			if (listener != null)
			{
				return new ValueTask<IMessage<T>>(Task.FromException<IMessage<T>>(new InvalidConfigurationException("Cannot use receive() when a listener has been set")));
			}
			try
			{
				VerifyConsumerState();
			}
			catch (PulsarClientException e)
			{
				return new ValueTask<IMessage<T>>(Task.FromException<IMessage<T>>(e));
			}
			return InternalReceiveAsync();
		}

		protected internal abstract IMessage<T> InternalReceive();

		protected internal abstract ValueTask<IMessage<T>> InternalReceiveAsync();


		public IMessage<T> Receive(int timeout, TimeUnit unit)
		{
			if (conf.ReceiverQueueSize == 0)
			{
				throw new InvalidConfigurationException("Can't use receive with timeout, if the queue size is 0");
			}
			if (listener != null)
			{
				throw new InvalidConfigurationException("Cannot use receive() when a listener has been set");
			}

			VerifyConsumerState();
			return InternalReceive(timeout, unit);
		}

		protected internal abstract IMessage<T> InternalReceive(int timeout, TimeUnit unit);

		public IMessages<T> BatchReceive()
		{
			VerifyBatchReceive();
			VerifyConsumerState();
			return InternalBatchReceive();
		}

		public ValueTask<IMessages<T>> BatchReceiveAsync()
		{
			try
			{
				VerifyBatchReceive();
				VerifyConsumerState();
				return InternalBatchReceiveAsync();
			}
			catch (PulsarClientException e)
			{
				return new ValueTask<IMessages<T>>(Task.FromException<IMessages<T>>(e));
			}
		}

		protected internal abstract IMessages<T> InternalBatchReceive();

		protected internal abstract ValueTask<IMessages<T>> InternalBatchReceiveAsync();

		public void Acknowledge(IMessage<T> message)
		{
			try
			{
				Acknowledge(message.MessageId);
			}
			catch (NullReferenceException npe)
			{
				throw new InvalidMessageException(npe.Message);
			}
		}

		public void Acknowledge(IMessageId messageId)
		{
			try
			{
				AcknowledgeAsync(messageId);
			}
			catch (System.Exception e)
			{
				throw Unwrap(e);
			}
		}

		public void Acknowledge(IMessages<T> messages)
		{
			try
			{
				AcknowledgeAsync(messages);
			}
			catch (System.Exception e)
			{
				throw Unwrap(e);
			}
		}

		public void AcknowledgeCumulative(IMessage<T> message)
		{
			try
			{
				AcknowledgeCumulative(message.MessageId);
			}
			catch (NullReferenceException npe)
			{
				throw new InvalidMessageException(npe.Message);
			}
		}

		public void AcknowledgeCumulative(IMessageId messageId)
		{
			try
			{
				AcknowledgeCumulativeAsync(messageId);
			}
			catch (System.Exception e)
			{
				throw Unwrap(e);
			}
		}

		public ValueTask AcknowledgeAsync(IMessage<T> message)
		{
			try
			{
				return AcknowledgeAsync(message.MessageId);
			}
			catch (NullReferenceException npe)
			{
				return new ValueTask(Task.FromException<IMessage<T>>(new InvalidMessageException(npe.Message)));
			}
		}

		public ValueTask AcknowledgeAsync(IMessages<T> messages)
		{
			try
			{
				foreach(var m in messages)
				{
					this.AcknowledgeAsync(m);
				}
				return new ValueTask(Task.CompletedTask);
			}
			catch (NullReferenceException npe)
			{
				return new ValueTask(Task.FromException(new InvalidMessageException(npe.Message)));
			}
		}

		public ValueTask AcknowledgeCumulativeAsync(IMessage<T> message)
		{
			try
			{
				return AcknowledgeCumulativeAsync(message.MessageId);
			}
			catch (NullReferenceException npe)
			{
				return new ValueTask(Task.FromException(new InvalidMessageException(npe.Message)));
			}
		}

		public ValueTask AcknowledgeAsync(IMessageId messageId)
		{
			return AcknowledgeAsync(messageId, null);
		}

		// TODO: expose this method to consumer interface when the transaction feature is completed
		// @Override
		public virtual ValueTask AcknowledgeAsync(IMessageId messageId, ITransaction txn)
		{
			TransactionImpl txnImpl = null;
			if (null != txn)
			{
				if(txn is TransactionImpl);
					txnImpl = (TransactionImpl) txn;
			}
			return DoAcknowledgeWithTxn(messageId, AckType.Individual, new Dictionary<string, long>(), txnImpl);
		}

		public ValueTask AcknowledgeCumulativeAsync(IMessageId messageId)
		{
			return AcknowledgeCumulativeAsync(messageId, null);
		}

		// TODO: expose this method to consumer interface when the transaction feature is completed
		// @Override
		public virtual ValueTask AcknowledgeCumulativeAsync(IMessageId messageId, ITransaction txn)
		{
			if (!IsCumulativeAcknowledgementAllowed(conf.SubscriptionType))
			{
				return new ValueTask(Task.FromException(new InvalidConfigurationException("Cannot use cumulative acks on a non-exclusive/non-failover subscription")));
			}

			TransactionImpl txnImpl = null;
			if (null != txn)
			{
				if(txn is TransactionImpl);
					txnImpl = (TransactionImpl) txn;
			}
			return DoAcknowledgeWithTxn(messageId, AckType.Cumulative, new Dictionary<string, long>(), txnImpl);
		}

		public void NegativeAcknowledge(IMessage<T> message)
		{
			NegativeAcknowledge(message.MessageId);
		}

		protected internal virtual ValueTask DoAcknowledgeWithTxn(IMessageId messageId, AckType ackType, IDictionary<string, long> properties, TransactionImpl txn)
		{
			var ack = DoAcknowledge(messageId, ackType, properties, txn);
			if (txn != null)
			{
				// it is okay that we register acked topic after sending the acknowledgements. because
				// the transactional ack will not be visiable for consumers until the transaction is
				// committed
				txn.RegisterAckedTopic(Topic);
				// register the ackFuture as part of the transaction
				return txn.RegisterAckOp(ack);
			}
			else
			{
				return ack;
			}
		}

		protected internal abstract ValueTask DoAcknowledge(IMessageId messageId, AckType ackType, IDictionary<string, long> properties, TransactionImpl txn);
		public void NegativeAcknowledge(IMessages<T> messages)
		{
			foreach(var m in messages)
				this.NegativeAcknowledge(m);
		}

		public void Unsubscribe()
		{
			try
			{
				UnsubscribeAsync();
			}
			catch (System.Exception e)
			{
				throw Unwrap(e);
			}
		}

		public abstract ValueTask UnsubscribeAsync();

		public void Close()
		{
			try
			{
				CloseAsync();
			}
			catch (System.Exception e)
			{
				throw Unwrap(e);
			}
		}

		public abstract ValueTask CloseAsync();

		public IMessageId LastMessageId
		{
			get
			{
				try
				{
					return LastMessageIdAsync.Result;
				}
				catch (System.Exception e)
				{
					throw Unwrap(e);
				}
			}
		}

		public abstract ValueTask<IMessageId> LastMessageIdAsync {get;}

		private bool IsCumulativeAcknowledgementAllowed(SubscriptionType type)
		{
			return SubscriptionType.Shared != type && SubscriptionType.Key_Shared != type;
		}

		protected internal virtual SubType SubType
		{
			get
			{
				SubscriptionType type = conf.SubscriptionType;
				switch (type)
				{
					case SubscriptionType.Exclusive:
						return SubType.Exclusive;
    
					case SubscriptionType.Shared:
						return SubType.Shared;
    
					case SubscriptionType.Failover:
						return SubType.Failover;
    
					case SubscriptionType.Key_Shared:
						return SubType.KeyShared;
					default: return SubType.Exclusive;
				}
			}
		}

		public abstract int AvailablePermits {get;}

		public abstract int NumMessagesInQueue();

		public virtual ValueTask<IConsumer<T>> SubscribeAsync()
		{
			return subscribeAsync;
		}

		public string Topic
		{
			get
			{
				return topic;
			}
		}

		public string Subscription
		{
			get
			{
				return subscription;
			}
		}

		public string ConsumerName
		{
			get
			{
				return consumerName;
			}
		}

		/// <summary>
		/// Redelivers the given unacknowledged messages. In Failover mode, the request is ignored if the consumer is not
		/// active for the given topic. In Shared mode, the consumers messages to be redelivered are distributed across all
		/// the connected consumers. This is a non blocking call and doesn't throw an exception. In case the connection
		/// breaks, the messages are redelivered after reconnect.
		/// </summary>
		public abstract void RedeliverUnacknowledgedMessages(ISet<IMessageId> messageIds);

		public override string ToString()
		{
			return "ConsumerBase{" + "subscription='" + subscription + '\'' + ", consumerName='" + consumerName + '\'' + ", topic='" + topic + '\'' + '}';
		}

		protected internal virtual int MaxReceiverQueueSize
		{
			set
			{
				maxReceiverQueueSize = value;
			}
		}

		protected internal virtual IMessage<T> BeforeConsume(IMessage<T> message)
		{
			if (interceptors != null)
			{
				return interceptors.BeforeConsume(this, message);
			}
			else
			{
				return message;
			}
		}

		protected internal virtual void OnAcknowledge(IMessageId messageId, System.Exception exception)
		{
			if (interceptors != null)
			{
				interceptors.OnAcknowledge(this, messageId, exception);
			}
		}

		protected internal virtual void OnAcknowledgeCumulative(IMessageId messageId, System.Exception exception)
		{
			if (interceptors != null)
			{
				interceptors.OnAcknowledgeCumulative(this, messageId, exception);
			}
		}

		protected internal virtual void OnNegativeAcksSend(ISet<IMessageId> messageIds)
		{
			if (interceptors != null)
			{
				interceptors.OnNegativeAcksSend(this, messageIds);
			}
		}

		protected internal virtual void OnAckTimeoutSend(ISet<IMessageId> messageIds)
		{
			if (interceptors != null)
			{
				interceptors.OnAckTimeoutSend(this, messageIds);
			}
		}

		protected internal virtual bool CanEnqueueMessage(IMessage<T> message)
		{
			// Default behavior, can be overridden in subclasses
			return true;
		}

		protected internal virtual bool EnqueueMessageAndCheckBatchReceive(IMessage<T> message)
		{
			if (CanEnqueueMessage(message))
			{
				incomingMessages.add(message);
				INCOMING_MESSAGES_SIZE_UPDATER.addAndGet(this, message.Data.Length);
			}
			return HasEnoughMessagesForBatchReceive();
		}

		protected internal virtual bool HasEnoughMessagesForBatchReceive()
		{
			if (batchReceivePolicy.MaxNumMessages <= 0 && batchReceivePolicy.MaxNumMessages <= 0)
			{
				return false;
			}
			return (batchReceivePolicy.MaxNumMessages > 0 && incomingMessages.size() >= batchReceivePolicy.MaxNumMessages) || (batchReceivePolicy.MaxNumBytes > 0 && INCOMING_MESSAGES_SIZE_UPDATER.get(this) >= batchReceivePolicy.MaxNumBytes);
		}

		private void VerifyConsumerState()
		{
			switch (State)
			{
				case State.Ready:
				case State.Connecting:
					break; // Ok
				case State.Closing:
				case State.Closed:
					throw new AlreadyClosedException("Consumer already closed");
				case State.Terminated:
					throw new AlreadyClosedException("Topic was terminated");
				case State.Failed:
				case State.Uninitialized:
					throw new NotConnectedException();
				default:
					break;
			}
		}

		private void VerifyBatchReceive()
		{
			if (listener != null)
			{
				throw new InvalidConfigurationException("Cannot use receive() when a listener has been set");
			}
			if (conf.ReceiverQueueSize == 0)
			{
				throw new InvalidConfigurationException("Can't use batch receive, if the queue size is 0");
			}
		}

		protected internal sealed class OpBatchReceive
		{

			internal readonly ValueTask<IMessages<T>> future;
			internal readonly long createdAt;

			internal OpBatchReceive(ValueTask<IMessages<T>> future)
			{
				this.future = future;
				createdAt = DateTimeHelper.CurrentUnixTimeMillis();
			}

			internal static OpBatchReceive Of(ValueTask<IMessages<T>> future)
			{
				return new OpBatchReceive(future);
			}
		}

		protected internal virtual void NotifyPendingBatchReceivedCallBack()
		{
			OpBatchReceive opBatchReceive = pendingBatchReceives.poll();
			if (opBatchReceive == null || opBatchReceive.future == null)
			{
				return;
			}
			NotifyPendingBatchReceivedCallBack(opBatchReceive);
		}

		protected internal virtual void NotifyPendingBatchReceivedCallBack(OpBatchReceive opBatchReceive)
		{
			MessagesImpl<T> messages = NewMessagesImpl;
			IMessage<T> msgPeeked = incomingMessages.Peek();
			while (msgPeeked != null && messages.CanAdd(msgPeeked))
			{
				IMessage<T> msg = null;
				try
				{
					msg = incomingMessages.Poll(0L, TimeUnit.MILLISECONDS);
				}
				catch (InterruptedException)
				{
					// ignore
				}
				if (msg != null)
				{
					MessageProcessed(msg);
					IMessage<T> interceptMsg = BeforeConsume(msg);
					messages.Add(interceptMsg);
				}
				msgPeeked = incomingMessages.Peek();
			}
			opBatchReceive.future.IsCompleted;
		}

		protected internal abstract void MessageProcessed<T1>(IMessage<T1> msg);
		public override void Run(Timeout timeout)
		{
			if (timeout.Cancelled)
			{
				return;
			}

			long timeToWaitMs;

			lock (this)
			{
				// If it's closing/closed we need to ignore this timeout and not schedule next timeout.
				if (HandlerState.State == State.Closing || State == State.Closed)
				{
					return;
				}
				if (pendingBatchReceives == null)
				{
					pendingBatchReceives = IQueue.newConcurrentLinkedQueue();
				}
				OpBatchReceive firstOpBatchReceive = pendingBatchReceives.peek();
				timeToWaitMs = batchReceivePolicy.TimeoutMs;

				while (firstOpBatchReceive != null)
				{
					// If there is at least one batch receive, calculate the diff between the batch receive timeout
					// and the current time.
					long diff = (firstOpBatchReceive.createdAt + batchReceivePolicy.TimeoutMs) - DateTimeHelper.CurrentUnixTimeMillis();
					if (diff <= 0)
					{
						// The diff is less than or equal to zero, meaning that the batch receive has been timed out.
						// complete the OpBatchReceive and continue to check the next OpBatchReceive in pendingBatchReceives.
						OpBatchReceive op = pendingBatchReceives.Poll();
						CompleteOpBatchReceive(op);
						firstOpBatchReceive = pendingBatchReceives.peek();
					}
					else
					{
						// The diff is greater than zero, set the timeout to the diff value
						timeToWaitMs = diff;
						break;
					}
				}
				batchReceiveTimeout = client.timer().newTimeout(this, timeToWaitMs, TimeUnit.MILLISECONDS);
			}
		}

		protected internal virtual MessagesImpl<T> NewMessagesImpl
		{
			get
			{
				return new MessagesImpl<T>(batchReceivePolicy.MaxNumMessages, batchReceivePolicy.MaxNumBytes);
			}
		}

		public abstract IConsumerStats Stats { get; }
		public abstract bool Connected { get; }

		protected internal virtual bool HasPendingBatchReceive()
		{
			return pendingBatchReceives != null && !pendingBatchReceives.Empty;
		}

		protected internal abstract void CompleteOpBatchReceive(OpBatchReceive op);
		public abstract void NegativeAcknowledge(IMessageId messageId);
		public abstract bool HasReachedEndOfTopic();
		public abstract void RedeliverUnacknowledgedMessages();
		public abstract void Seek(IMessageId messageId);
		public abstract void Seek(long timestamp);
		public abstract ValueTask SeekAsync(IMessageId messageId);
		public abstract ValueTask SeekAsync(long timestamp);
		public abstract void Pause();
		public abstract void Resume();
		public abstract ValueTask DisposeAsync();
	}

}