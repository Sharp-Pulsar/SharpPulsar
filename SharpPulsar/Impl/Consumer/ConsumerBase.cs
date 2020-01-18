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

	using Queues = com.google.common.collect.Queues;

	using Timeout = io.netty.util.Timeout;
	using TimerTask = io.netty.util.TimerTask;
	using BatchReceivePolicy = org.apache.pulsar.client.api.BatchReceivePolicy;
	using Consumer = org.apache.pulsar.client.api.Consumer;
	using ConsumerEventListener = org.apache.pulsar.client.api.ConsumerEventListener;
	using Message = org.apache.pulsar.client.api.Message;
	using MessageId = org.apache.pulsar.client.api.MessageId;
	using MessageListener = org.apache.pulsar.client.api.MessageListener;
	using Messages = org.apache.pulsar.client.api.Messages;
	using PulsarClientException = org.apache.pulsar.client.api.PulsarClientException;
	using Schema = org.apache.pulsar.client.api.Schema;
	using SubscriptionType = org.apache.pulsar.client.api.SubscriptionType;
	using Transaction = org.apache.pulsar.client.api.transaction.Transaction;
	using SharpPulsar.Impl.conf;
	using TransactionImpl = SharpPulsar.Impl.transaction.TransactionImpl;
	using ConsumerName = org.apache.pulsar.client.util.ConsumerName;
	using AckType = org.apache.pulsar.common.api.proto.PulsarApi.CommandAck.AckType;
	using SubType = org.apache.pulsar.common.api.proto.PulsarApi.CommandSubscribe.SubType;
	using FutureUtil = org.apache.pulsar.common.util.FutureUtil;
	using GrowableArrayBlockingQueue = org.apache.pulsar.common.util.collections.GrowableArrayBlockingQueue;

	public abstract class ConsumerBase<T> : HandlerState, TimerTask, Consumer<T>
	{

		internal enum ConsumerType
		{
			PARTITIONED,
			NON_PARTITIONED
		}

		protected internal readonly string subscription;
		protected internal readonly ConsumerConfigurationData<T> conf;
		protected internal readonly string consumerName;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		protected internal readonly CompletableFuture<Consumer<T>> subscribeFuture_Conflict;
		protected internal readonly MessageListener<T> listener;
		protected internal readonly ConsumerEventListener consumerEventListener;
		protected internal readonly ExecutorService listenerExecutor;
		internal readonly BlockingQueue<Message<T>> incomingMessages;
		protected internal readonly ConcurrentLinkedQueue<CompletableFuture<Message<T>>> pendingReceives;
		protected internal int maxReceiverQueueSize;
		protected internal readonly Schema<T> schema;
		protected internal readonly ConsumerInterceptors<T> interceptors;
		protected internal readonly BatchReceivePolicy batchReceivePolicy;
		protected internal ConcurrentLinkedQueue<OpBatchReceive<T>> pendingBatchReceives;
		protected internal static readonly AtomicLongFieldUpdater<ConsumerBase> INCOMING_MESSAGES_SIZE_UPDATER = AtomicLongFieldUpdater.newUpdater(typeof(ConsumerBase), "incomingMessagesSize");
		protected internal volatile long incomingMessagesSize = 0;
		protected internal volatile Timeout batchReceiveTimeout = null;

		protected internal ConsumerBase(PulsarClientImpl client, string topic, ConsumerConfigurationData<T> conf, int receiverQueueSize, ExecutorService listenerExecutor, CompletableFuture<Consumer<T>> subscribeFuture, Schema<T> schema, ConsumerInterceptors interceptors) : base(client, topic)
		{
			this.maxReceiverQueueSize = receiverQueueSize;
			this.subscription = conf.SubscriptionName;
			this.conf = conf;
			this.consumerName = conf.ConsumerName == null ? ConsumerName.generateRandomName() : conf.ConsumerName;
			this.subscribeFuture_Conflict = subscribeFuture;
			this.listener = conf.MessageListener;
			this.consumerEventListener = conf.ConsumerEventListener;
			// Always use growable queue since items can exceed the advertised size
			this.incomingMessages = new GrowableArrayBlockingQueue<Message<T>>();

			this.listenerExecutor = listenerExecutor;
			this.pendingReceives = Queues.newConcurrentLinkedQueue();
			this.schema = schema;
			this.interceptors = interceptors;
			if (conf.BatchReceivePolicy != null)
			{
				this.batchReceivePolicy = conf.BatchReceivePolicy;
			}
			else
			{
				this.batchReceivePolicy = BatchReceivePolicy.DEFAULT_POLICY;
			}
			if (batchReceivePolicy.TimeoutMs > 0)
			{
				batchReceiveTimeout = client.timer().newTimeout(this, batchReceivePolicy.TimeoutMs, TimeUnit.MILLISECONDS);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public org.apache.pulsar.client.api.Message<T> receive() throws org.apache.pulsar.client.api.PulsarClientException
		public override Message<T> receive()
		{
			if (listener != null)
			{
				throw new PulsarClientException.InvalidConfigurationException("Cannot use receive() when a listener has been set");
			}
			verifyConsumerState();
			return internalReceive();
		}

		public override CompletableFuture<Message<T>> receiveAsync()
		{
			if (listener != null)
			{
				return FutureUtil.failedFuture(new PulsarClientException.InvalidConfigurationException("Cannot use receive() when a listener has been set"));
			}
			try
			{
				verifyConsumerState();
			}
			catch (PulsarClientException e)
			{
				return FutureUtil.failedFuture(e);
			}
			return internalReceiveAsync();
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected abstract org.apache.pulsar.client.api.Message<T> internalReceive() throws org.apache.pulsar.client.api.PulsarClientException;
		protected internal abstract Message<T> internalReceive();

		protected internal abstract CompletableFuture<Message<T>> internalReceiveAsync();

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public org.apache.pulsar.client.api.Message<T> receive(int timeout, java.util.concurrent.TimeUnit unit) throws org.apache.pulsar.client.api.PulsarClientException
		public override Message<T> receive(int timeout, TimeUnit unit)
		{
			if (conf.ReceiverQueueSize == 0)
			{
				throw new PulsarClientException.InvalidConfigurationException("Can't use receive with timeout, if the queue size is 0");
			}
			if (listener != null)
			{
				throw new PulsarClientException.InvalidConfigurationException("Cannot use receive() when a listener has been set");
			}

			verifyConsumerState();
			return internalReceive(timeout, unit);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected abstract org.apache.pulsar.client.api.Message<T> internalReceive(int timeout, java.util.concurrent.TimeUnit unit) throws org.apache.pulsar.client.api.PulsarClientException;
		protected internal abstract Message<T> internalReceive(int timeout, TimeUnit unit);

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public org.apache.pulsar.client.api.Messages<T> batchReceive() throws org.apache.pulsar.client.api.PulsarClientException
		public override Messages<T> batchReceive()
		{
			verifyBatchReceive();
			verifyConsumerState();
			return internalBatchReceive();
		}

		public override CompletableFuture<Messages<T>> batchReceiveAsync()
		{
			try
			{
				verifyBatchReceive();
				verifyConsumerState();
				return internalBatchReceiveAsync();
			}
			catch (PulsarClientException e)
			{
				return FutureUtil.failedFuture(e);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected abstract org.apache.pulsar.client.api.Messages<T> internalBatchReceive() throws org.apache.pulsar.client.api.PulsarClientException;
		protected internal abstract Messages<T> internalBatchReceive();

		protected internal abstract CompletableFuture<Messages<T>> internalBatchReceiveAsync();

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void acknowledge(org.apache.pulsar.client.api.Message<?> message) throws org.apache.pulsar.client.api.PulsarClientException
		public override void acknowledge<T1>(Message<T1> message)
		{
			try
			{
				acknowledge(message.MessageId);
			}
			catch (System.NullReferenceException npe)
			{
				throw new PulsarClientException.InvalidMessageException(npe.Message);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void acknowledge(org.apache.pulsar.client.api.MessageId messageId) throws org.apache.pulsar.client.api.PulsarClientException
		public override void acknowledge(MessageId messageId)
		{
			try
			{
				acknowledgeAsync(messageId).get();
			}
			catch (Exception e)
			{
				throw PulsarClientException.unwrap(e);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void acknowledge(org.apache.pulsar.client.api.Messages<?> messages) throws org.apache.pulsar.client.api.PulsarClientException
		public override void acknowledge<T1>(Messages<T1> messages)
		{
			try
			{
				acknowledgeAsync(messages).get();
			}
			catch (Exception e)
			{
				throw PulsarClientException.unwrap(e);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void acknowledgeCumulative(org.apache.pulsar.client.api.Message<?> message) throws org.apache.pulsar.client.api.PulsarClientException
		public override void acknowledgeCumulative<T1>(Message<T1> message)
		{
			try
			{
				acknowledgeCumulative(message.MessageId);
			}
			catch (System.NullReferenceException npe)
			{
				throw new PulsarClientException.InvalidMessageException(npe.Message);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void acknowledgeCumulative(org.apache.pulsar.client.api.MessageId messageId) throws org.apache.pulsar.client.api.PulsarClientException
		public override void acknowledgeCumulative(MessageId messageId)
		{
			try
			{
				acknowledgeCumulativeAsync(messageId).get();
			}
			catch (Exception e)
			{
				throw PulsarClientException.unwrap(e);
			}
		}

		public override CompletableFuture<Void> acknowledgeAsync<T1>(Message<T1> message)
		{
			try
			{
				return acknowledgeAsync(message.MessageId);
			}
			catch (System.NullReferenceException npe)
			{
				return FutureUtil.failedFuture(new PulsarClientException.InvalidMessageException(npe.Message));
			}
		}

		public override CompletableFuture<Void> acknowledgeAsync<T1>(Messages<T1> messages)
		{
			try
			{
				messages.forEach(this.acknowledgeAsync);
				return CompletableFuture.completedFuture(null);
			}
			catch (System.NullReferenceException npe)
			{
				return FutureUtil.failedFuture(new PulsarClientException.InvalidMessageException(npe.Message));
			}
		}

		public override CompletableFuture<Void> acknowledgeCumulativeAsync<T1>(Message<T1> message)
		{
			try
			{
				return acknowledgeCumulativeAsync(message.MessageId);
			}
			catch (System.NullReferenceException npe)
			{
				return FutureUtil.failedFuture(new PulsarClientException.InvalidMessageException(npe.Message));
			}
		}

		public override CompletableFuture<Void> acknowledgeAsync(MessageId messageId)
		{
			return acknowledgeAsync(messageId, null);
		}

		// TODO: expose this method to consumer interface when the transaction feature is completed
		// @Override
		public virtual CompletableFuture<Void> acknowledgeAsync(MessageId messageId, Transaction txn)
		{
			TransactionImpl txnImpl = null;
			if (null != txn)
			{
				checkArgument(txn is TransactionImpl);
				txnImpl = (TransactionImpl) txn;
			}
			return doAcknowledgeWithTxn(messageId, AckType.Individual, Collections.emptyMap(), txnImpl);
		}

		public override CompletableFuture<Void> acknowledgeCumulativeAsync(MessageId messageId)
		{
			return acknowledgeCumulativeAsync(messageId, null);
		}

		// TODO: expose this method to consumer interface when the transaction feature is completed
		// @Override
		public virtual CompletableFuture<Void> acknowledgeCumulativeAsync(MessageId messageId, Transaction txn)
		{
			if (!isCumulativeAcknowledgementAllowed(conf.SubscriptionType))
			{
				return FutureUtil.failedFuture(new PulsarClientException.InvalidConfigurationException("Cannot use cumulative acks on a non-exclusive/non-failover subscription"));
			}

			TransactionImpl txnImpl = null;
			if (null != txn)
			{
				checkArgument(txn is TransactionImpl);
				txnImpl = (TransactionImpl) txn;
			}
			return doAcknowledgeWithTxn(messageId, AckType.Cumulative, Collections.emptyMap(), txnImpl);
		}

		public override void negativeAcknowledge<T1>(Message<T1> message)
		{
			negativeAcknowledge(message.MessageId);
		}

		protected internal virtual CompletableFuture<Void> doAcknowledgeWithTxn(MessageId messageId, AckType ackType, IDictionary<string, long> properties, TransactionImpl txn)
		{
			CompletableFuture<Void> ackFuture = doAcknowledge(messageId, ackType, properties, txn);
			if (txn != null)
			{
				// it is okay that we register acked topic after sending the acknowledgements. because
				// the transactional ack will not be visiable for consumers until the transaction is
				// committed
				txn.registerAckedTopic(Topic);
				// register the ackFuture as part of the transaction
				return txn.registerAckOp(ackFuture);
			}
			else
			{
				return ackFuture;
			}
		}

		protected internal abstract CompletableFuture<Void> doAcknowledge(MessageId messageId, AckType ackType, IDictionary<string, long> properties, TransactionImpl txn);
		public override void negativeAcknowledge<T1>(Messages<T1> messages)
		{
			messages.forEach(this.negativeAcknowledge);
		}


//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void unsubscribe() throws org.apache.pulsar.client.api.PulsarClientException
		public override void unsubscribe()
		{
			try
			{
				unsubscribeAsync().get();
			}
			catch (Exception e)
			{
				throw PulsarClientException.unwrap(e);
			}
		}

		public override abstract CompletableFuture<Void> unsubscribeAsync();

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void close() throws org.apache.pulsar.client.api.PulsarClientException
		public override void close()
		{
			try
			{
				closeAsync().get();
			}
			catch (Exception e)
			{
				throw PulsarClientException.unwrap(e);
			}
		}

		public override abstract CompletableFuture<Void> closeAsync();


//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public org.apache.pulsar.client.api.MessageId getLastMessageId() throws org.apache.pulsar.client.api.PulsarClientException
		public override MessageId LastMessageId
		{
			get
			{
				try
				{
					return LastMessageIdAsync.get();
				}
				catch (Exception e)
				{
					throw PulsarClientException.unwrap(e);
				}
			}
		}

		public override abstract CompletableFuture<MessageId> LastMessageIdAsync {get;}

		private bool isCumulativeAcknowledgementAllowed(SubscriptionType type)
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
				case Exclusive:
					return SubType.Exclusive;
    
				case Shared:
					return SubType.Shared;
    
				case Failover:
					return SubType.Failover;
    
				case Key_Shared:
					return SubType.Key_Shared;
				}
    
				// Should not happen since we cover all cases above
				return null;
			}
		}

		public abstract int AvailablePermits {get;}

		public abstract int numMessagesInQueue();

		public virtual CompletableFuture<Consumer<T>> subscribeFuture()
		{
			return subscribeFuture_Conflict;
		}

		public override string Topic
		{
			get
			{
				return topic;
			}
		}

		public override string Subscription
		{
			get
			{
				return subscription;
			}
		}

		public override string ConsumerName
		{
			get
			{
				return this.consumerName;
			}
		}

		/// <summary>
		/// Redelivers the given unacknowledged messages. In Failover mode, the request is ignored if the consumer is not
		/// active for the given topic. In Shared mode, the consumers messages to be redelivered are distributed across all
		/// the connected consumers. This is a non blocking call and doesn't throw an exception. In case the connection
		/// breaks, the messages are redelivered after reconnect.
		/// </summary>
		protected internal abstract void redeliverUnacknowledgedMessages(ISet<MessageId> messageIds);

		public override string ToString()
		{
			return "ConsumerBase{" + "subscription='" + subscription + '\'' + ", consumerName='" + consumerName + '\'' + ", topic='" + topic + '\'' + '}';
		}

		protected internal virtual int MaxReceiverQueueSize
		{
			set
			{
				this.maxReceiverQueueSize = value;
			}
		}

		protected internal virtual Message<T> beforeConsume(Message<T> message)
		{
			if (interceptors != null)
			{
				return interceptors.beforeConsume(this, message);
			}
			else
			{
				return message;
			}
		}

		protected internal virtual void onAcknowledge(MessageId messageId, Exception exception)
		{
			if (interceptors != null)
			{
				interceptors.onAcknowledge(this, messageId, exception);
			}
		}

		protected internal virtual void onAcknowledgeCumulative(MessageId messageId, Exception exception)
		{
			if (interceptors != null)
			{
				interceptors.onAcknowledgeCumulative(this, messageId, exception);
			}
		}

		protected internal virtual void onNegativeAcksSend(ISet<MessageId> messageIds)
		{
			if (interceptors != null)
			{
				interceptors.onNegativeAcksSend(this, messageIds);
			}
		}

		protected internal virtual void onAckTimeoutSend(ISet<MessageId> messageIds)
		{
			if (interceptors != null)
			{
				interceptors.onAckTimeoutSend(this, messageIds);
			}
		}

		protected internal virtual bool canEnqueueMessage(Message<T> message)
		{
			// Default behavior, can be overridden in subclasses
			return true;
		}

		protected internal virtual bool enqueueMessageAndCheckBatchReceive(Message<T> message)
		{
			if (canEnqueueMessage(message))
			{
				incomingMessages.add(message);
				INCOMING_MESSAGES_SIZE_UPDATER.addAndGet(this, message.Data.length);
			}
			return hasEnoughMessagesForBatchReceive();
		}

		protected internal virtual bool hasEnoughMessagesForBatchReceive()
		{
			if (batchReceivePolicy.MaxNumMessages <= 0 && batchReceivePolicy.MaxNumMessages <= 0)
			{
				return false;
			}
			return (batchReceivePolicy.MaxNumMessages > 0 && incomingMessages.size() >= batchReceivePolicy.MaxNumMessages) || (batchReceivePolicy.MaxNumBytes > 0 && INCOMING_MESSAGES_SIZE_UPDATER.get(this) >= batchReceivePolicy.MaxNumBytes);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void verifyConsumerState() throws org.apache.pulsar.client.api.PulsarClientException
		private void verifyConsumerState()
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

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void verifyBatchReceive() throws org.apache.pulsar.client.api.PulsarClientException
		private void verifyBatchReceive()
		{
			if (listener != null)
			{
				throw new PulsarClientException.InvalidConfigurationException("Cannot use receive() when a listener has been set");
			}
			if (conf.ReceiverQueueSize == 0)
			{
				throw new PulsarClientException.InvalidConfigurationException("Can't use batch receive, if the queue size is 0");
			}
		}

		protected internal sealed class OpBatchReceive<T>
		{

			internal readonly CompletableFuture<Messages<T>> future;
			internal readonly long createdAt;

			internal OpBatchReceive(CompletableFuture<Messages<T>> future)
			{
				this.future = future;
				this.createdAt = DateTimeHelper.CurrentUnixTimeMillis();
			}

			internal static OpBatchReceive<T> of<T>(CompletableFuture<Messages<T>> future)
			{
				return new OpBatchReceive<T>(future);
			}
		}

		protected internal virtual void notifyPendingBatchReceivedCallBack()
		{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final OpBatchReceive<T> opBatchReceive = pendingBatchReceives.poll();
			OpBatchReceive<T> opBatchReceive = pendingBatchReceives.poll();
			if (opBatchReceive == null || opBatchReceive.future == null)
			{
				return;
			}
			notifyPendingBatchReceivedCallBack(opBatchReceive);
		}

		protected internal virtual void notifyPendingBatchReceivedCallBack(OpBatchReceive<T> opBatchReceive)
		{
			MessagesImpl<T> messages = NewMessagesImpl;
			Message<T> msgPeeked = incomingMessages.peek();
			while (msgPeeked != null && messages.canAdd(msgPeeked))
			{
				Message<T> msg = null;
				try
				{
					msg = incomingMessages.poll(0L, TimeUnit.MILLISECONDS);
				}
				catch (InterruptedException)
				{
					// ignore
				}
				if (msg != null)
				{
					messageProcessed(msg);
					Message<T> interceptMsg = beforeConsume(msg);
					messages.add(interceptMsg);
				}
				msgPeeked = incomingMessages.peek();
			}
			opBatchReceive.future.complete(messages);
		}

		protected internal abstract void messageProcessed<T1>(Message<T1> msg);

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void run(io.netty.util.Timeout timeout) throws Exception
		public override void run(Timeout timeout)
		{
			if (timeout.Cancelled)
			{
				return;
			}

			long timeToWaitMs;

			lock (this)
			{
				// If it's closing/closed we need to ignore this timeout and not schedule next timeout.
				if (State == State.Closing || State == State.Closed)
				{
					return;
				}
				if (pendingBatchReceives == null)
				{
					pendingBatchReceives = Queues.newConcurrentLinkedQueue();
				}
				OpBatchReceive<T> firstOpBatchReceive = pendingBatchReceives.peek();
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
						OpBatchReceive<T> op = pendingBatchReceives.poll();
						completeOpBatchReceive(op);
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

		protected internal virtual bool hasPendingBatchReceive()
		{
			return pendingBatchReceives != null && !pendingBatchReceives.Empty;
		}

		protected internal abstract void completeOpBatchReceive(OpBatchReceive<T> op);
	}

}