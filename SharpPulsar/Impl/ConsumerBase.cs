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
	using BatchReceivePolicy = SharpPulsar.Api.BatchReceivePolicy;
	using Consumer = SharpPulsar.Api.IConsumer;
	using ConsumerEventListener = SharpPulsar.Api.ConsumerEventListener;
	using SharpPulsar.Api;
	using IMessageId = SharpPulsar.Api.IMessageId;
	using SharpPulsar.Api;
	using SharpPulsar.Api;
	using PulsarClientException = SharpPulsar.Api.PulsarClientException;
	using SharpPulsar.Api;
	using SubscriptionType = SharpPulsar.Api.SubscriptionType;
	using Transaction = SharpPulsar.Api.Transaction.ITransaction;
	using SharpPulsar.Impl.Conf;
	using TransactionImpl = SharpPulsar.Impl.Transaction.TransactionImpl;
	using ConsumerName = SharpPulsar.Util.ConsumerName;
	using AckType = Org.Apache.Pulsar.Common.Api.Proto.PulsarApi.CommandAck.AckType;
	using SubType = Org.Apache.Pulsar.Common.Api.Proto.PulsarApi.CommandSubscribe.SubType;
	using FutureUtil = Org.Apache.Pulsar.Common.Util.FutureUtil;
	using Org.Apache.Pulsar.Common.Util.Collections;
    using System.Threading.Tasks;

    public abstract class ConsumerBase<T> : HandlerState, TimerTask, IConsumer<T>
	{
		public abstract void NegativeAcknowledge(IMessageId MessageId);
		public abstract void Resume();
		public abstract void Pause();
		public abstract bool Connected {get;}
		public abstract CompletableFuture<Void> SeekAsync(long Timestamp);
		public abstract CompletableFuture<Void> SeekAsync(IMessageId MessageId);
		public abstract void Seek(long Timestamp);
		public abstract void Seek(IMessageId MessageId);
		public abstract void RedeliverUnacknowledgedMessages();
		public abstract bool HasReachedEndOfTopic();
		public abstract ConsumerStats Stats {get;}

		public enum ConsumerType
		{
			PARTITIONED,
			NonPartitioned
		}

//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		protected internal readonly string SubscriptionConflict;
		protected internal readonly ConsumerConfigurationData<T> Conf;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		protected internal readonly string ConsumerNameConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		protected internal readonly CompletableFuture<IConsumer<T>> SubscribeFutureConflict;
		protected internal readonly MessageListener<T> Listener;
		protected internal readonly ConsumerEventListener ConsumerEventListener;
		protected internal readonly ExecutorService ListenerExecutor;
		internal readonly BlockingQueue<Message<T>> IncomingMessages;
		protected internal readonly ConcurrentLinkedQueue<CompletableFuture<Message<T>>> PendingReceives;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		protected internal int MaxReceiverQueueSizeConflict;
		protected internal readonly ISchema<T> Schema;
		protected internal readonly ConsumerInterceptors<T> Interceptors;
		protected internal readonly BatchReceivePolicy BatchReceivePolicy;
		protected internal ConcurrentLinkedQueue<OpBatchReceive<T>> PendingBatchReceives;
		protected internal static readonly AtomicLongFieldUpdater<ConsumerBase> IncomingMessagesSizeUpdater = AtomicLongFieldUpdater.newUpdater(typeof(ConsumerBase), "incomingMessagesSize");
		protected internal volatile long IncomingMessagesSize = 0;
		protected internal volatile Timeout BatchReceiveTimeout = null;

		public ConsumerBase(PulsarClientImpl Client, string Topic, ConsumerConfigurationData<T> Conf, int ReceiverQueueSize, ExecutorService ListenerExecutor, CompletableFuture<IConsumer<T>> SubscribeFuture, ISchema<T> Schema, ConsumerInterceptors Interceptors) : base(Client, Topic)
		{
			this.MaxReceiverQueueSizeConflict = ReceiverQueueSize;
			this.SubscriptionConflict = Conf.SubscriptionName;
			this.Conf = Conf;
			this.ConsumerNameConflict = Conf.ConsumerName == null ? Util.ConsumerName.GenerateRandomName() : Conf.ConsumerName;
			this.SubscribeFutureConflict = SubscribeFuture;
			this.Listener = Conf.MessageListener;
			this.ConsumerEventListener = Conf.ConsumerEventListener;
			// Always use growable queue since items can exceed the advertised size
			this.IncomingMessages = new GrowableArrayBlockingQueue<Message<T>>();

			this.ListenerExecutor = ListenerExecutor;
			this.PendingReceives = Queues.newConcurrentLinkedQueue();
			this.Schema = Schema;
			this.Interceptors = Interceptors;
			if (Conf.BatchReceivePolicy != null)
			{
				this.BatchReceivePolicy = Conf.BatchReceivePolicy;
			}
			else
			{
				this.BatchReceivePolicy = BatchReceivePolicy.DEFAULT_POLICY;
			}
			if (BatchReceivePolicy.TimeoutMs > 0)
			{
				BatchReceiveTimeout = Client.timer().newTimeout(this, BatchReceivePolicy.TimeoutMs, BAMCIS.Util.Concurrent.TimeUnit.MILLISECONDS);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public SharpPulsar.api.Message<T> receive() throws SharpPulsar.api.PulsarClientException
		public override Message<T> Receive()
		{
			if (Listener != null)
			{
				throw new PulsarClientException.InvalidConfigurationException("Cannot use receive() when a listener has been set");
			}
			VerifyConsumerState();
			return InternalReceive();
		}

		public override CompletableFuture<Message<T>> ReceiveAsync()
		{
			if (Listener != null)
			{
				return FutureUtil.failedFuture(new PulsarClientException.InvalidConfigurationException("Cannot use receive() when a listener has been set"));
			}
			try
			{
				VerifyConsumerState();
			}
			catch (PulsarClientException E)
			{
				return FutureUtil.failedFuture(E);
			}
			return InternalReceiveAsync();
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected abstract SharpPulsar.api.Message<T> internalReceive() throws SharpPulsar.api.PulsarClientException;
		public abstract Message<T> InternalReceive();

		public abstract CompletableFuture<Message<T>> InternalReceiveAsync();

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public SharpPulsar.api.Message<T> receive(int timeout, java.util.concurrent.BAMCIS.Util.Concurrent.TimeUnit unit) throws SharpPulsar.api.PulsarClientException
		public override Message<T> Receive(int Timeout, BAMCIS.Util.Concurrent.TimeUnit Unit)
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
			return InternalReceive(Timeout, Unit);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected abstract SharpPulsar.api.Message<T> internalReceive(int timeout, java.util.concurrent.BAMCIS.Util.Concurrent.TimeUnit unit) throws SharpPulsar.api.PulsarClientException;
		public abstract Message<T> InternalReceive(int Timeout, BAMCIS.Util.Concurrent.TimeUnit Unit);

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public SharpPulsar.api.Messages<T> batchReceive() throws SharpPulsar.api.PulsarClientException
		public override Messages<T> BatchReceive()
		{
			VerifyBatchReceive();
			VerifyConsumerState();
			return InternalBatchReceive();
		}

		public override CompletableFuture<Messages<T>> BatchReceiveAsync()
		{
			try
			{
				VerifyBatchReceive();
				VerifyConsumerState();
				return InternalBatchReceiveAsync();
			}
			catch (PulsarClientException E)
			{
				return FutureUtil.failedFuture(E);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected abstract SharpPulsar.api.Messages<T> internalBatchReceive() throws SharpPulsar.api.PulsarClientException;
		public abstract Messages<T> InternalBatchReceive();

		public abstract CompletableFuture<Messages<T>> InternalBatchReceiveAsync();

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void acknowledge(SharpPulsar.api.Message<?> message) throws SharpPulsar.api.PulsarClientException
		public override void Acknowledge<T1>(Message<T1> Message)
		{
			try
			{
				Acknowledge(Message.MessageId);
			}
			catch (System.NullReferenceException Npe)
			{
				throw new PulsarClientException.InvalidMessageException(Npe.Message);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void acknowledge(SharpPulsar.api.MessageId messageId) throws SharpPulsar.api.PulsarClientException
		public override void Acknowledge(IMessageId MessageId)
		{
			try
			{
				AcknowledgeAsync(MessageId).get();
			}
			catch (Exception E)
			{
				throw PulsarClientException.unwrap(E);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void acknowledge(SharpPulsar.api.Messages<?> messages) throws SharpPulsar.api.PulsarClientException
		public override void Acknowledge<T1>(Messages<T1> Messages)
		{
			try
			{
				AcknowledgeAsync(Messages).get();
			}
			catch (Exception E)
			{
				throw PulsarClientException.unwrap(E);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void acknowledgeCumulative(SharpPulsar.api.Message<?> message) throws SharpPulsar.api.PulsarClientException
		public override void AcknowledgeCumulative<T1>(Message<T1> Message)
		{
			try
			{
				AcknowledgeCumulative(Message.MessageId);
			}
			catch (System.NullReferenceException Npe)
			{
				throw new PulsarClientException.InvalidMessageException(Npe.Message);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void acknowledgeCumulative(SharpPulsar.api.MessageId messageId) throws SharpPulsar.api.PulsarClientException
		public override void AcknowledgeCumulative(IMessageId MessageId)
		{
			try
			{
				AcknowledgeCumulativeAsync(MessageId).get();
			}
			catch (Exception E)
			{
				throw PulsarClientException.unwrap(E);
			}
		}

		public override CompletableFuture<Void> AcknowledgeAsync<T1>(Message<T1> Message)
		{
			try
			{
				return AcknowledgeAsync(Message.MessageId);
			}
			catch (System.NullReferenceException Npe)
			{
				return FutureUtil.failedFuture(new PulsarClientException.InvalidMessageException(Npe.Message));
			}
		}

		public override CompletableFuture<Void> AcknowledgeAsync<T1>(Messages<T1> Messages)
		{
			try
			{
				Messages.forEach(this.acknowledgeAsync);
				return CompletableFuture.completedFuture(null);
			}
			catch (System.NullReferenceException Npe)
			{
				return FutureUtil.failedFuture(new PulsarClientException.InvalidMessageException(Npe.Message));
			}
		}

		public override CompletableFuture<Void> AcknowledgeCumulativeAsync<T1>(Message<T1> Message)
		{
			try
			{
				return AcknowledgeCumulativeAsync(Message.MessageId);
			}
			catch (System.NullReferenceException Npe)
			{
				return FutureUtil.failedFuture(new PulsarClientException.InvalidMessageException(Npe.Message));
			}
		}

		public override CompletableFuture<Void> AcknowledgeAsync(IMessageId MessageId)
		{
			return AcknowledgeAsync(MessageId, null);
		}

		// TODO: expose this method to consumer interface when the transaction feature is completed
		// @Override
		public virtual CompletableFuture<Void> AcknowledgeAsync(IMessageId MessageId, Transaction Txn)
		{
			TransactionImpl TxnImpl = null;
			if (null != Txn)
			{
				checkArgument(Txn is TransactionImpl);
				TxnImpl = (TransactionImpl) Txn;
			}
			return DoAcknowledgeWithTxn(MessageId, AckType.Individual, Collections.emptyMap(), TxnImpl);
		}

		public override CompletableFuture<Void> AcknowledgeCumulativeAsync(IMessageId MessageId)
		{
			return AcknowledgeCumulativeAsync(MessageId, null);
		}

		// TODO: expose this method to consumer interface when the transaction feature is completed
		// @Override
		public virtual CompletableFuture<Void> AcknowledgeCumulativeAsync(IMessageId MessageId, Transaction Txn)
		{
			if (!IsCumulativeAcknowledgementAllowed(Conf.SubscriptionType))
			{
				return FutureUtil.failedFuture(new PulsarClientException.InvalidConfigurationException("Cannot use cumulative acks on a non-exclusive/non-failover subscription"));
			}

			TransactionImpl TxnImpl = null;
			if (null != Txn)
			{
				checkArgument(Txn is TransactionImpl);
				TxnImpl = (TransactionImpl) Txn;
			}
			return DoAcknowledgeWithTxn(MessageId, AckType.Cumulative, Collections.emptyMap(), TxnImpl);
		}

		public override void NegativeAcknowledge<T1>(Message<T1> Message)
		{
			NegativeAcknowledge(Message.MessageId);
		}

		public virtual CompletableFuture<Void> DoAcknowledgeWithTxn(IMessageId MessageId, AckType AckType, IDictionary<string, long> Properties, TransactionImpl Txn)
		{
			CompletableFuture<Void> AckFuture = DoAcknowledge(MessageId, AckType, Properties, Txn);
			if (Txn != null)
			{
				// it is okay that we register acked topic after sending the acknowledgements. because
				// the transactional ack will not be visiable for consumers until the transaction is
				// committed
				Txn.registerAckedTopic(Topic);
				// register the ackFuture as part of the transaction
				return Txn.registerAckOp(AckFuture);
			}
			else
			{
				return AckFuture;
			}
		}

		public abstract CompletableFuture<Void> DoAcknowledge(IMessageId MessageId, AckType AckType, IDictionary<string, long> Properties, TransactionImpl Txn);
		public override void NegativeAcknowledge<T1>(Messages<T1> Messages)
		{
			Messages.forEach(this.negativeAcknowledge);
		}


//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void unsubscribe() throws SharpPulsar.api.PulsarClientException
		public override void Unsubscribe()
		{
			try
			{
				UnsubscribeAsync().get();
			}
			catch (Exception E)
			{
				throw PulsarClientException.unwrap(E);
			}
		}

		public override abstract CompletableFuture<Void> UnsubscribeAsync();

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void close() throws SharpPulsar.api.PulsarClientException
		public override void Close()
		{
			try
			{
				CloseAsync().get();
			}
			catch (Exception E)
			{
				throw PulsarClientException.unwrap(E);
			}
		}

		public abstract ValueTask CloseAsync();


//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public SharpPulsar.api.MessageId getLastMessageId() throws SharpPulsar.api.PulsarClientException
		public virtual IMessageId LastMessageId
		{
			get
			{
				try
				{
					return LastMessageIdAsync.get();
				}
				catch (Exception E)
				{
					throw PulsarClientException.unwrap(E);
				}
			}
		}

		public override abstract CompletableFuture<IMessageId> LastMessageIdAsync {get;}

		private bool IsCumulativeAcknowledgementAllowed(SubscriptionType Type)
		{
			return SubscriptionType.Shared != Type && SubscriptionType.Key_Shared != Type;
		}

		public virtual SubType SubType
		{
			get
			{
				SubscriptionType Type = Conf.SubscriptionType;
				switch (Type)
				{
				case SubscriptionType.Exclusive:
					return SubType.Exclusive;
    
				case SubscriptionType.Shared:
					return SubType.Shared;
    
				case SubscriptionType.Failover:
					return SubType.Failover;
    
				case SubscriptionType.Key_Shared:
					return SubType.Key_Shared;
				}
    
				// Should not happen since we cover all cases above
				return null;
			}
		}

		public abstract int AvailablePermits {get;}

		public abstract int NumMessagesInQueue();

		public virtual CompletableFuture<IConsumer<T>> SubscribeFuture()
		{
			return SubscribeFutureConflict;
		}

		public virtual string Topic
		{
			get
			{
				return Topic;
			}
		}

		public virtual string Subscription
		{
			get
			{
				return SubscriptionConflict;
			}
		}

		public virtual string ConsumerName
		{
			get
			{
				return this.ConsumerNameConflict;
			}
		}

		/// <summary>
		/// Redelivers the given unacknowledged messages. In Failover mode, the request is ignored if the consumer is not
		/// active for the given topic. In Shared mode, the consumers messages to be redelivered are distributed across all
		/// the connected consumers. This is a non blocking call and doesn't throw an exception. In case the connection
		/// breaks, the messages are redelivered after reconnect.
		/// </summary>
		public abstract void RedeliverUnacknowledgedMessages(ISet<IMessageId> MessageIds);

		public override string ToString()
		{
			return "ConsumerBase{" + "subscription='" + SubscriptionConflict + '\'' + ", consumerName='" + ConsumerNameConflict + '\'' + ", topic='" + Topic + '\'' + '}';
		}

		public virtual int MaxReceiverQueueSize
		{
			set
			{
				this.MaxReceiverQueueSizeConflict = value;
			}
		}

		public virtual Message<T> BeforeConsume(Message<T> Message)
		{
			if (Interceptors != null)
			{
				return Interceptors.beforeConsume(this, Message);
			}
			else
			{
				return Message;
			}
		}

		public virtual void OnAcknowledge(IMessageId MessageId, Exception Exception)
		{
			if (Interceptors != null)
			{
				Interceptors.onAcknowledge(this, MessageId, Exception);
			}
		}

		public virtual void OnAcknowledgeCumulative(IMessageId MessageId, Exception Exception)
		{
			if (Interceptors != null)
			{
				Interceptors.onAcknowledgeCumulative(this, MessageId, Exception);
			}
		}

		public virtual void OnNegativeAcksSend(ISet<IMessageId> MessageIds)
		{
			if (Interceptors != null)
			{
				Interceptors.onNegativeAcksSend(this, MessageIds);
			}
		}

		public virtual void OnAckTimeoutSend(ISet<IMessageId> MessageIds)
		{
			if (Interceptors != null)
			{
				Interceptors.onAckTimeoutSend(this, MessageIds);
			}
		}

		public virtual bool CanEnqueueMessage(Message<T> Message)
		{
			// Default behavior, can be overridden in subclasses
			return true;
		}

		public virtual bool EnqueueMessageAndCheckBatchReceive(Message<T> Message)
		{
			if (CanEnqueueMessage(Message))
			{
				IncomingMessages.add(Message);
				IncomingMessagesSizeUpdater.addAndGet(this, Message.Data.Length);
			}
			return HasEnoughMessagesForBatchReceive();
		}

		public virtual bool HasEnoughMessagesForBatchReceive()
		{
			if (BatchReceivePolicy.MaxNumMessages <= 0 && BatchReceivePolicy.MaxNumMessages <= 0)
			{
				return false;
			}
			return (BatchReceivePolicy.MaxNumMessages > 0 && IncomingMessages.size() >= BatchReceivePolicy.MaxNumMessages) || (BatchReceivePolicy.MaxNumBytes > 0 && IncomingMessagesSizeUpdater.get(this) >= BatchReceivePolicy.MaxNumBytes);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void verifyConsumerState() throws SharpPulsar.api.PulsarClientException
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

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void verifyBatchReceive() throws SharpPulsar.api.PulsarClientException
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

		public sealed class OpBatchReceive<T>
		{

			internal readonly CompletableFuture<Messages<T>> Future;
			internal readonly long CreatedAt;

			public OpBatchReceive(CompletableFuture<Messages<T>> Future)
			{
				this.Future = Future;
				this.CreatedAt = DateTimeHelper.CurrentUnixTimeMillis();
			}

			internal static OpBatchReceive<T> Of<T>(CompletableFuture<Messages<T>> Future)
			{
				return new OpBatchReceive<T>(Future);
			}
		}

		public virtual void NotifyPendingBatchReceivedCallBack()
		{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final OpBatchReceive<T> opBatchReceive = pendingBatchReceives.poll();
			OpBatchReceive<T> OpBatchReceive = PendingBatchReceives.poll();
			if (OpBatchReceive == null || OpBatchReceive.Future == null)
			{
				return;
			}
			NotifyPendingBatchReceivedCallBack(OpBatchReceive);
		}

		public virtual void NotifyPendingBatchReceivedCallBack(OpBatchReceive<T> OpBatchReceive)
		{
			MessagesImpl<T> Messages = NewMessagesImpl;
			Message<T> MsgPeeked = IncomingMessages.peek();
			while (MsgPeeked != null && Messages.canAdd(MsgPeeked))
			{
				Message<T> Msg = null;
				try
				{
					Msg = IncomingMessages.poll(0L, BAMCIS.Util.Concurrent.TimeUnit.MILLISECONDS);
				}
				catch (InterruptedException)
				{
					// ignore
				}
				if (Msg != null)
				{
					MessageProcessed(Msg);
					Message<T> InterceptMsg = BeforeConsume(Msg);
					Messages.add(InterceptMsg);
				}
				MsgPeeked = IncomingMessages.peek();
			}
			OpBatchReceive.Future.complete(Messages);
		}

		public abstract void messageProcessed<T1>(Message<T1> Msg);

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void run(io.netty.util.Timeout timeout) throws Exception
		public override void Run(Timeout Timeout)
		{
			if (Timeout.Cancelled)
			{
				return;
			}

			long TimeToWaitMs;

			lock (this)
			{
				// If it's closing/closed we need to ignore this timeout and not schedule next timeout.
				if (State == State.Closing || State == State.Closed)
				{
					return;
				}
				if (PendingBatchReceives == null)
				{
					PendingBatchReceives = Queues.newConcurrentLinkedQueue();
				}
				OpBatchReceive<T> FirstOpBatchReceive = PendingBatchReceives.peek();
				TimeToWaitMs = BatchReceivePolicy.TimeoutMs;

				while (FirstOpBatchReceive != null)
				{
					// If there is at least one batch receive, calculate the diff between the batch receive timeout
					// and the current time.
					long Diff = (FirstOpBatchReceive.CreatedAt + BatchReceivePolicy.TimeoutMs) - DateTimeHelper.CurrentUnixTimeMillis();
					if (Diff <= 0)
					{
						// The diff is less than or equal to zero, meaning that the batch receive has been timed out.
						// complete the OpBatchReceive and continue to check the next OpBatchReceive in pendingBatchReceives.
						OpBatchReceive<T> Op = PendingBatchReceives.poll();
						CompleteOpBatchReceive(Op);
						FirstOpBatchReceive = PendingBatchReceives.peek();
					}
					else
					{
						// The diff is greater than zero, set the timeout to the diff value
						TimeToWaitMs = Diff;
						break;
					}
				}
				BatchReceiveTimeout = ClientConflict.timer().newTimeout(this, TimeToWaitMs, BAMCIS.Util.Concurrent.TimeUnit.MILLISECONDS);
			}
		}

		public virtual MessagesImpl<T> NewMessagesImpl
		{
			get
			{
				return new MessagesImpl<T>(BatchReceivePolicy.MaxNumMessages, BatchReceivePolicy.MaxNumBytes);
			}
		}

		public virtual bool HasPendingBatchReceive()
		{
			return PendingBatchReceives != null && !PendingBatchReceives.Empty;
		}

		public abstract void CompleteOpBatchReceive(OpBatchReceive<T> Op);
	}

}