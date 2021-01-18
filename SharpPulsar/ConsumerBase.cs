using Akka.Actor;
using SharpPulsar.Akka;
using SharpPulsar.Interfaces;
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
	public abstract class ConsumerBase<T> : ReceiveActor
	{
		public abstract long LastDisconnectedTimestamp { get; }
		public abstract void NegativeAcknowledge(IMessageId messageId);
		public abstract void Resume();
		public abstract void Pause();
		public abstract bool Connected { get; }
		public abstract CompletableFuture<Void> SeekAsync(long timestamp);
		public abstract CompletableFuture<Void> SeekAsync(MessageId messageId);
		public abstract void Seek(long timestamp);
		public abstract void Seek(IMessageId messageId);
		public abstract void RedeliverUnacknowledgedMessages();
		public abstract bool HasReachedEndOfTopic();
		public abstract ConsumerStats Stats { get; }

		internal enum ConsumerType
		{
			PARTITIONED,
			NonPartitioned
		}

		private readonly string _subscription;
		private readonly ConsumerConfigurationData<T> _conf;
		private readonly string _consumerName;
		protected internal readonly CompletableFuture<Consumer<T>> SubscribeFutureConflict;
		protected internal readonly MessageListener<T> Listener;
		protected internal readonly ConsumerEventListener ConsumerEventListener;
		protected internal readonly ExecutorService ListenerExecutor;
		internal readonly BlockingQueue<IMessage> IncomingMessages;
		protected internal ConcurrentOpenHashMap<MessageIdImpl, MessageIdImpl[]> UnAckedChunckedMessageIdSequenceMap;
		protected internal readonly ConcurrentLinkedQueue<CompletableFuture<Message<T>>> PendingReceives;
		//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods of the current type:
		protected internal int MaxReceiverQueueSizeConflict;
		protected internal readonly Schema<T> Schema;
		protected internal readonly ConsumerInterceptors<T> Interceptors;
		protected internal readonly BatchReceivePolicy BatchReceivePolicy;
		protected internal ConcurrentLinkedQueue<OpBatchReceive<T>> PendingBatchReceives;
		protected internal static readonly AtomicLongFieldUpdater<ConsumerBase> IncomingMessagesSizeUpdater = AtomicLongFieldUpdater.newUpdater(typeof(ConsumerBase), "incomingMessagesSize");
		protected internal volatile long IncomingMessagesSize = 0;
		protected internal volatile Timeout BatchReceiveTimeout = null;
		protected internal readonly Lock ReentrantLock = new ReentrantLock();

		protected internal ConsumerBase(PulsarClientImpl client, string topic, ConsumerConfigurationData<T> conf, int receiverQueueSize, ExecutorService listenerExecutor, CompletableFuture<Consumer<T>> subscribeFuture, Schema<T> schema, ConsumerInterceptors interceptors) : base(client, topic)
		{
			this.MaxReceiverQueueSizeConflict = receiverQueueSize;
			this._subscription = conf.SubscriptionName;
			this._conf = conf;
			this._consumerName = conf.ConsumerName == null ? ConsumerName.GenerateRandomName() : conf.ConsumerName;
			this.SubscribeFutureConflict = subscribeFuture;
			this.Listener = conf.MessageListener;
			this.ConsumerEventListener = conf.ConsumerEventListener;
			// Always use growable queue since items can exceed the advertised size
			this.IncomingMessages = new GrowableArrayBlockingQueue<Message<T>>();
			this.UnAckedChunckedMessageIdSequenceMap = new ConcurrentOpenHashMap<MessageIdImpl, MessageIdImpl[]>();

			this.ListenerExecutor = listenerExecutor;
			this.PendingReceives = Queues.newConcurrentLinkedQueue();
			this.Schema = schema;
			this.Interceptors = interceptors;
			if (conf.BatchReceivePolicy != null)
			{
				BatchReceivePolicy userBatchReceivePolicy = conf.BatchReceivePolicy;
				if (userBatchReceivePolicy.MaxNumMessages > this.MaxReceiverQueueSizeConflict)
				{
					this.BatchReceivePolicy = BatchReceivePolicy.Builder().MaxNumMessages(this.MaxReceiverQueueSizeConflict).MaxNumBytes(userBatchReceivePolicy.MaxNumBytes).Timeout((int)userBatchReceivePolicy.TimeoutMs, TimeUnit.MILLISECONDS).build();
					_log.warn("BatchReceivePolicy maxNumMessages: {} is greater than maxReceiverQueueSize: {}, " + "reset to maxReceiverQueueSize. batchReceivePolicy: {}", userBatchReceivePolicy.MaxNumMessages, this.MaxReceiverQueueSizeConflict, this.BatchReceivePolicy.ToString());
				}
				else if (userBatchReceivePolicy.MaxNumMessages <= 0 && userBatchReceivePolicy.MaxNumBytes <= 0)
				{
					this.BatchReceivePolicy = BatchReceivePolicy.Builder().MaxNumMessages(BatchReceivePolicy.DefaultPolicy.MaxNumMessages).MaxNumBytes(BatchReceivePolicy.DefaultPolicy.MaxNumBytes).timeout((int)userBatchReceivePolicy.TimeoutMs, TimeUnit.MILLISECONDS).build();
					_log.warn("BatchReceivePolicy maxNumMessages: {} or maxNumBytes: {} is less than 0. " + "Reset to DEFAULT_POLICY. batchReceivePolicy: {}", userBatchReceivePolicy.MaxNumMessages, userBatchReceivePolicy.MaxNumBytes, this.BatchReceivePolicy.ToString());
				}
				else
				{
					this.BatchReceivePolicy = conf.BatchReceivePolicy;
				}
			}
			else
			{
				this.BatchReceivePolicy = BatchReceivePolicy.DefaultPolicy;
			}

			if (BatchReceivePolicy.TimeoutMs > 0)
			{
				BatchReceiveTimeout = client.Timer().newTimeout(this.pendingBatchReceiveTask, BatchReceivePolicy.TimeoutMs, TimeUnit.MILLISECONDS);
			}
		}

		//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		//ORIGINAL LINE: @Override public org.apache.pulsar.client.api.Message<T> receive() throws org.apache.pulsar.client.api.PulsarClientException
		public virtual Message<T> Receive()
		{
			if (Listener != null)
			{
				throw new PulsarClientException.InvalidConfigurationException("Cannot use receive() when a listener has been set");
			}
			VerifyConsumerState();
			return InternalReceive();
		}

		public virtual CompletableFuture<Message<T>> ReceiveAsync()
		{
			if (Listener != null)
			{
				return FutureUtil.FailedFuture(new PulsarClientException.InvalidConfigurationException("Cannot use receive() when a listener has been set"));
			}
			try
			{
				VerifyConsumerState();
			}
			catch (PulsarClientException e)
			{
				return FutureUtil.FailedFuture(e);
			}
			return InternalReceiveAsync();
		}

		//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		//ORIGINAL LINE: protected abstract org.apache.pulsar.client.api.Message<T> internalReceive() throws org.apache.pulsar.client.api.PulsarClientException;
		protected internal abstract Message<T> InternalReceive();

		protected internal abstract CompletableFuture<Message<T>> InternalReceiveAsync();

		//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		//ORIGINAL LINE: @Override public org.apache.pulsar.client.api.Message<T> receive(int timeout, java.util.concurrent.TimeUnit unit) throws org.apache.pulsar.client.api.PulsarClientException
		public virtual Message<T> Receive(int timeout, TimeUnit unit)
		{
			if (_conf.ReceiverQueueSize == 0)
			{
				throw new PulsarClientException.InvalidConfigurationException("Can't use receive with timeout, if the queue size is 0");
			}
			if (Listener != null)
			{
				throw new PulsarClientException.InvalidConfigurationException("Cannot use receive() when a listener has been set");
			}

			VerifyConsumerState();
			return InternalReceive(timeout, unit);
		}

		//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		//ORIGINAL LINE: protected abstract org.apache.pulsar.client.api.Message<T> internalReceive(int timeout, java.util.concurrent.TimeUnit unit) throws org.apache.pulsar.client.api.PulsarClientException;
		protected internal abstract Message<T> InternalReceive(int timeout, TimeUnit unit);

		//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		//ORIGINAL LINE: @Override public org.apache.pulsar.client.api.Messages<T> batchReceive() throws org.apache.pulsar.client.api.PulsarClientException
		public virtual Messages<T> BatchReceive()
		{
			VerifyBatchReceive();
			VerifyConsumerState();
			return InternalBatchReceive();
		}

		public virtual CompletableFuture<Messages<T>> BatchReceiveAsync()
		{
			try
			{
				VerifyBatchReceive();
				VerifyConsumerState();
				return InternalBatchReceiveAsync();
			}
			catch (PulsarClientException e)
			{
				return FutureUtil.FailedFuture(e);
			}
		}

		protected internal virtual CompletableFuture<Message<T>> PeekPendingReceive()
		{
			CompletableFuture<Message<T>> receivedFuture = null;
			while (receivedFuture == null)
			{
				receivedFuture = PendingReceives.peek();
				if (receivedFuture == null)
				{
					break;
				}
				// skip done futures (cancelling a future could mark it done)
				if (receivedFuture.Done)
				{
					CompletableFuture<Message<T>> removed = PendingReceives.poll();
					if (removed != receivedFuture)
					{
						_log.error("Bug! Removed future wasn't the expected one. expected={} removed={}", receivedFuture, removed);
					}
					receivedFuture = null;
				}
			}
			return receivedFuture;
		}

		protected internal virtual CompletableFuture<Message<T>> PollPendingReceive()
		{
			CompletableFuture<Message<T>> receivedFuture;
			while (true)
			{
				receivedFuture = PendingReceives.poll();
				// skip done futures (cancelling a future could mark it done)
				if (receivedFuture == null || !receivedFuture.Done)
				{
					break;
				}
			}
			return receivedFuture;
		}

		protected internal virtual void CompletePendingReceive(CompletableFuture<Message<T>> receivedFuture, Message<T> message)
		{
			ListenerExecutor.execute(() =>
			{
				if (!receivedFuture.complete(message))
				{
					_log.warn("Race condition detected. receive future was already completed (cancelled={}) and message was dropped. message={}", receivedFuture.Cancelled, message);
				}
			});
		}

		protected internal virtual void FailPendingReceives(ConcurrentLinkedQueue<CompletableFuture<Message<T>>> pendingReceives)
		{
			while (!pendingReceives.Empty)
			{
				CompletableFuture<Message<T>> receiveFuture = pendingReceives.poll();
				if (receiveFuture == null)
				{
					break;
				}
				if (!receiveFuture.Done)
				{
					receiveFuture.completeExceptionally(new PulsarClientException.AlreadyClosedException(string.Format("The consumer which subscribes the topic {0} with subscription name {1} " + "was already closed when cleaning and closing the consumers", Topic, _subscription)));
				}
			}
		}

		protected internal virtual void FailPendingBatchReceives(ConcurrentLinkedQueue<OpBatchReceive<T>> pendingBatchReceives)
		{
			while (!pendingBatchReceives.Empty)
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

		//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		//ORIGINAL LINE: protected abstract org.apache.pulsar.client.api.Messages<T> internalBatchReceive() throws org.apache.pulsar.client.api.PulsarClientException;
		protected internal abstract Messages<T> InternalBatchReceive();

		protected internal abstract CompletableFuture<Messages<T>> InternalBatchReceiveAsync();

		//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		//ORIGINAL LINE: @Override public void acknowledge(org.apache.pulsar.client.api.Message<?> message) throws org.apache.pulsar.client.api.PulsarClientException
		public virtual void Acknowledge<T1>(Message<T1> message)
		{
			try
			{
				Acknowledge(message.MessageId);
			}
			catch (System.NullReferenceException npe)
			{
				throw new PulsarClientException.InvalidMessageException(npe.Message);
			}
		}

		//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		//ORIGINAL LINE: @Override public void acknowledge(org.apache.pulsar.client.api.MessageId messageId) throws org.apache.pulsar.client.api.PulsarClientException
		public virtual void Acknowledge(MessageId messageId)
		{
			try
			{
				AcknowledgeAsync(messageId).get();
			}
			catch (Exception e)
			{
				throw PulsarClientException.Unwrap(e);
			}
		}

		//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		//ORIGINAL LINE: @Override public void acknowledge(java.util.List<org.apache.pulsar.client.api.MessageId> messageIdList) throws org.apache.pulsar.client.api.PulsarClientException
		public virtual void Acknowledge(IList<MessageId> messageIdList)
		{
			try
			{
				AcknowledgeAsync(messageIdList).get();
			}
			catch (Exception e)
			{
				throw PulsarClientException.Unwrap(e);
			}
		}

		//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		//ORIGINAL LINE: @Override public void acknowledge(org.apache.pulsar.client.api.Messages<?> messages) throws org.apache.pulsar.client.api.PulsarClientException
		public virtual void Acknowledge<T1>(Messages<T1> messages)
		{
			try
			{
				AcknowledgeAsync(messages).get();
			}
			catch (Exception e)
			{
				throw PulsarClientException.Unwrap(e);
			}
		}

		//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		//ORIGINAL LINE: @Override public void reconsumeLater(org.apache.pulsar.client.api.Message<?> message, long delayTime, java.util.concurrent.TimeUnit unit) throws org.apache.pulsar.client.api.PulsarClientException
		public virtual void ReconsumeLater<T1>(Message<T1> message, long delayTime, TimeUnit unit)
		{
			if (!_conf.RetryEnable)
			{
				throw new PulsarClientException("reconsumeLater method not support!");
			}
			try
			{
				ReconsumeLaterAsync(message, delayTime, unit).get();
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

		//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		//ORIGINAL LINE: @Override public void reconsumeLater(org.apache.pulsar.client.api.Messages<?> messages, long delayTime, java.util.concurrent.TimeUnit unit) throws org.apache.pulsar.client.api.PulsarClientException
		public virtual void ReconsumeLater<T1>(Messages<T1> messages, long delayTime, TimeUnit unit)
		{
			try
			{
				ReconsumeLaterAsync(messages, delayTime, unit).get();
			}
			catch (Exception e)
			{
				throw PulsarClientException.Unwrap(e);
			}
		}

		//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		//ORIGINAL LINE: @Override public void acknowledgeCumulative(org.apache.pulsar.client.api.Message<?> message) throws org.apache.pulsar.client.api.PulsarClientException
		public virtual void AcknowledgeCumulative<T1>(Message<T1> message)
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

		//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		//ORIGINAL LINE: @Override public void acknowledgeCumulative(org.apache.pulsar.client.api.MessageId messageId) throws org.apache.pulsar.client.api.PulsarClientException
		public virtual void AcknowledgeCumulative(MessageId messageId)
		{
			try
			{
				AcknowledgeCumulativeAsync(messageId).get();
			}
			catch (Exception e)
			{
				throw PulsarClientException.Unwrap(e);
			}
		}

		//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		//ORIGINAL LINE: @Override public void reconsumeLaterCumulative(org.apache.pulsar.client.api.Message<?> message, long delayTime, java.util.concurrent.TimeUnit unit) throws org.apache.pulsar.client.api.PulsarClientException
		public virtual void ReconsumeLaterCumulative<T1>(Message<T1> message, long delayTime, TimeUnit unit)
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

		public virtual CompletableFuture<Void> AcknowledgeAsync<T1>(Message<T1> message)
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

		public virtual CompletableFuture<Void> AcknowledgeAsync<T1>(Messages<T1> messages)
		{
			try
			{
				messages.forEach(this.acknowledgeAsync);
				return CompletableFuture.completedFuture(null);
			}
			catch (System.NullReferenceException npe)
			{
				return FutureUtil.FailedFuture(new PulsarClientException.InvalidMessageException(npe.Message));
			}
		}

		public virtual CompletableFuture<Void> AcknowledgeAsync(IList<MessageId> messageIdList)
		{
			return doAcknowledgeWithTxn(messageIdList, AckType.Individual, Collections.emptyMap(), null);
		}

		public virtual CompletableFuture<Void> ReconsumeLaterAsync<T1>(Message<T1> message, long delayTime, TimeUnit unit)
		{
			if (!_conf.RetryEnable)
			{
				return FutureUtil.FailedFuture(new PulsarClientException("reconsumeLater method not support!"));
			}
			try
			{
				return DoReconsumeLater(message, AckType.Individual, Collections.emptyMap(), delayTime, unit);
			}
			catch (System.NullReferenceException npe)
			{
				return FutureUtil.FailedFuture(new PulsarClientException.InvalidMessageException(npe.Message));
			}
		}

		public virtual CompletableFuture<Void> ReconsumeLaterAsync<T1>(Messages<T1> messages, long delayTime, TimeUnit unit)
		{
			try
			{
				messages.forEach(message => reconsumeLaterAsync(message, delayTime, unit));
				return CompletableFuture.completedFuture(null);
			}
			catch (System.NullReferenceException npe)
			{
				return FutureUtil.FailedFuture(new PulsarClientException.InvalidMessageException(npe.Message));
			}
		}

		public virtual CompletableFuture<Void> AcknowledgeCumulativeAsync<T1>(Message<T1> message)
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

		public virtual CompletableFuture<Void> ReconsumeLaterCumulativeAsync<T1>(Message<T1> message, long delayTime, TimeUnit unit)
		{
			if (!_conf.RetryEnable)
			{
				return FutureUtil.FailedFuture(new PulsarClientException("reconsumeLater method not support!"));
			}
			if (!IsCumulativeAcknowledgementAllowed(_conf.SubscriptionType))
			{
				return FutureUtil.FailedFuture(new PulsarClientException.InvalidConfigurationException("Cannot use cumulative acks on a non-exclusive subscription"));
			}
			return DoReconsumeLater(message, AckType.Cumulative, Collections.emptyMap(), delayTime, unit);
		}

		public virtual CompletableFuture<Void> AcknowledgeAsync(MessageId messageId)
		{
			return AcknowledgeAsync(messageId, null);
		}

		public virtual CompletableFuture<Void> AcknowledgeAsync(MessageId messageId, Transaction txn)
		{
			TransactionImpl txnImpl = null;
			if (null != txn)
			{
				checkArgument(txn is TransactionImpl);
				txnImpl = (TransactionImpl)txn;
			}
			return doAcknowledgeWithTxn(messageId, AckType.Individual, Collections.emptyMap(), txnImpl);
		}

		public virtual CompletableFuture<Void> AcknowledgeCumulativeAsync(MessageId messageId)
		{
			return AcknowledgeCumulativeAsync(messageId, null);
		}

		public virtual CompletableFuture<Void> AcknowledgeCumulativeAsync(MessageId messageId, Transaction txn)
		{
			if (!IsCumulativeAcknowledgementAllowed(_conf.SubscriptionType))
			{
				return FutureUtil.FailedFuture(new PulsarClientException.InvalidConfigurationException("Cannot use cumulative acks on a non-exclusive/non-failover subscription"));
			}

			TransactionImpl txnImpl = null;
			if (null != txn)
			{
				checkArgument(txn is TransactionImpl);
				txnImpl = (TransactionImpl)txn;
			}
			return doAcknowledgeWithTxn(messageId, AckType.Cumulative, Collections.emptyMap(), txnImpl);
		}

		public virtual void NegativeAcknowledge<T1>(Message<T1> message)
		{
			NegativeAcknowledge(message.MessageId);
		}

		protected internal virtual CompletableFuture<Void> DoAcknowledgeWithTxn(IList<MessageId> messageIdList, AckType ackType, IDictionary<string, long> properties, TransactionImpl txn)
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

		protected internal virtual CompletableFuture<Void> DoAcknowledgeWithTxn(MessageId messageId, AckType ackType, IDictionary<string, long> properties, TransactionImpl txn)
		{
			CompletableFuture<Void> ackFuture;
			if (txn != null && (this is ConsumerImpl))
			{
				// it is okay that we register acked topic after sending the acknowledgements. because
				// the transactional ack will not be visiable for consumers until the transaction is
				// committed
				if (ackType == AckType.Cumulative)
				{
					//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in C#:
					//ORIGINAL LINE: txn.registerCumulativeAckConsumer((ConsumerImpl<?>) this);
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

		protected internal abstract CompletableFuture<Void> DoAcknowledge(MessageId messageId, AckType ackType, IDictionary<string, long> properties, TransactionImpl txn);

		protected internal abstract CompletableFuture<Void> DoAcknowledge(IList<MessageId> messageIdList, AckType ackType, IDictionary<string, long> properties, TransactionImpl txn);

		protected internal abstract CompletableFuture<Void> doReconsumeLater<T1>(Message<T1> message, AckType ackType, IDictionary<string, long> properties, long delayTime, TimeUnit unit);

		public virtual void NegativeAcknowledge<T1>(Messages<T1> messages)
		{
			messages.forEach(this.negativeAcknowledge);
		}


		//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		//ORIGINAL LINE: @Override public void unsubscribe() throws org.apache.pulsar.client.api.PulsarClientException
		public virtual void Unsubscribe()
		{
			try
			{
				UnsubscribeAsync().get();
			}
			catch (Exception e)
			{
				throw PulsarClientException.Unwrap(e);
			}
		}

		public override abstract CompletableFuture<Void> UnsubscribeAsync();

		//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		//ORIGINAL LINE: @Override public void close() throws org.apache.pulsar.client.api.PulsarClientException
		public virtual void close()
		{
			try
			{
				CloseAsync().get();
			}
			catch (Exception e)
			{
				throw PulsarClientException.Unwrap(e);
			}
		}

		public override abstract CompletableFuture<Void> CloseAsync();


		//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		//ORIGINAL LINE: @Override public org.apache.pulsar.client.api.MessageId getLastMessageId() throws org.apache.pulsar.client.api.PulsarClientException
		public virtual MessageId LastMessageId
		{
			get
			{
				try
				{
					return LastMessageIdAsync.get();
				}
				catch (Exception e)
				{
					throw PulsarClientException.Unwrap(e);
				}
			}
		}

		public override abstract CompletableFuture<MessageId> LastMessageIdAsync { get; }

		private bool IsCumulativeAcknowledgementAllowed(SubscriptionType type)
		{
			return SubscriptionType.Shared != type && SubscriptionType.KeyShared != type;
		}

		protected internal virtual SubType SubType
		{
			get
			{
				SubscriptionType type = _conf.SubscriptionType;
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

		public abstract int AvailablePermits { get; }

		public abstract int NumMessagesInQueue();

		public virtual CompletableFuture<Consumer<T>> SubscribeFuture()
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
				return _subscription;
			}
		}

		public virtual string ConsumerName
		{
			get
			{
				return this._consumerName;
			}
		}

		/// <summary>
		/// Redelivers the given unacknowledged messages. In Failover mode, the request is ignored if the consumer is not
		/// active for the given topic. In Shared mode, the consumers messages to be redelivered are distributed across all
		/// the connected consumers. This is a non blocking call and doesn't throw an exception. In case the connection
		/// breaks, the messages are redelivered after reconnect.
		/// </summary>
		protected internal abstract void RedeliverUnacknowledgedMessages(ISet<MessageId> messageIds);

		public override string ToString()
		{
			return "ConsumerBase{" + "subscription='" + _subscription + '\'' + ", consumerName='" + _consumerName + '\'' + ", topic='" + Topic + '\'' + '}';
		}

		protected internal virtual int MaxReceiverQueueSize
		{
			set
			{
				this.MaxReceiverQueueSizeConflict = value;
			}
		}

		protected internal virtual Message<T> BeforeConsume(Message<T> message)
		{
			if (Interceptors != null)
			{
				return Interceptors.BeforeConsume(this, message);
			}
			else
			{
				return message;
			}
		}

		protected internal virtual void OnAcknowledge(MessageId messageId, Exception exception)
		{
			if (Interceptors != null)
			{
				Interceptors.OnAcknowledge(this, messageId, exception);
			}
		}

		protected internal virtual void OnAcknowledgeCumulative(MessageId messageId, Exception exception)
		{
			if (Interceptors != null)
			{
				Interceptors.OnAcknowledgeCumulative(this, messageId, exception);
			}
		}

		protected internal virtual void OnNegativeAcksSend(ISet<MessageId> messageIds)
		{
			if (Interceptors != null)
			{
				Interceptors.OnNegativeAcksSend(this, messageIds);
			}
		}

		protected internal virtual void OnAckTimeoutSend(ISet<MessageId> messageIds)
		{
			if (Interceptors != null)
			{
				Interceptors.OnAckTimeoutSend(this, messageIds);
			}
		}

		protected internal virtual bool CanEnqueueMessage(Message<T> message)
		{
			// Default behavior, can be overridden in subclasses
			return true;
		}

		protected internal virtual bool EnqueueMessageAndCheckBatchReceive(Message<T> message)
		{
			if (CanEnqueueMessage(message) && IncomingMessages.offer(message))
			{
				IncomingMessagesSizeUpdater.addAndGet(this, message.Data == null ? 0 : message.Data.Length);
			}
			return HasEnoughMessagesForBatchReceive();
		}

		protected internal virtual bool HasEnoughMessagesForBatchReceive()
		{
			if (BatchReceivePolicy.MaxNumMessages <= 0 && BatchReceivePolicy.MaxNumBytes <= 0)
			{
				return false;
			}
			return (BatchReceivePolicy.MaxNumMessages > 0 && IncomingMessages.size() >= BatchReceivePolicy.MaxNumMessages) || (BatchReceivePolicy.MaxNumBytes > 0 && IncomingMessagesSizeUpdater.get(this) >= BatchReceivePolicy.MaxNumBytes);
		}

		//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		//ORIGINAL LINE: private void verifyConsumerState() throws org.apache.pulsar.client.api.PulsarClientException
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
		//ORIGINAL LINE: private void verifyBatchReceive() throws org.apache.pulsar.client.api.PulsarClientException
		private void VerifyBatchReceive()
		{
			if (Listener != null)
			{
				throw new PulsarClientException.InvalidConfigurationException("Cannot use receive() when a listener has been set");
			}
			if (_conf.ReceiverQueueSize == 0)
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
				this.Future = future;
				this.CreatedAt = System.nanoTime();
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

		protected internal abstract void messageProcessed<T1>(Message<T1> msg);


		//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		//ORIGINAL LINE: private void pendingBatchReceiveTask(io.netty.util.Timeout timeout) throws Exception
		private void PendingBatchReceiveTask(Timeout timeout)
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
				if (PendingBatchReceives == null)
				{
					PendingBatchReceives = Queues.newConcurrentLinkedQueue();
				}
				OpBatchReceive<T> firstOpBatchReceive = PeekNextBatchReceive();
				timeToWaitMs = BatchReceivePolicy.TimeoutMs;

				while (firstOpBatchReceive != null)
				{
					// If there is at least one batch receive, calculate the diff between the batch receive timeout
					// and the elapsed time since the operation was created.
					long diff = BatchReceivePolicy.TimeoutMs - TimeUnit.NANOSECONDS.toMillis(System.nanoTime() - firstOpBatchReceive.CreatedAt);
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
				BatchReceiveTimeout = ClientConflict.Timer().newTimeout(this.pendingBatchReceiveTask, timeToWaitMs, TimeUnit.MILLISECONDS);
			}
		}

		protected internal virtual MessagesImpl<T> NewMessagesImpl
		{
			get
			{
				return new MessagesImpl<T>(BatchReceivePolicy.MaxNumMessages, BatchReceivePolicy.MaxNumBytes);
			}
		}

		protected internal virtual bool HasPendingBatchReceive()
		{
			return PendingBatchReceives != null && PeekNextBatchReceive() != null;
		}

		protected internal abstract void CompleteOpBatchReceive(OpBatchReceive<T> op);

		private static readonly Logger _log = LoggerFactory.getLogger(typeof(ConsumerBase));
	}

}