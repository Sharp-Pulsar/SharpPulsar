using System;
using System.Collections.Generic;
using System.Threading;
using DotNetty.Common.Utilities;
using SharpPulsar.Api;
using SharpPulsar.Api.Transaction;
using SharpPulsar.Exception;
using SharpPulsar.Impl.Conf;
using SharpPulsar.Impl.Transaction;
using SharpPulsar.Protocol.Proto;
using SharpPulsar.Util;
using SharpPulsar.Util.Atomic.Collections.Concurrent;
using SharpPulsar.Util.Collections;
using SharpPulsar.Utils;

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
    using System.Threading.Tasks;
    using System.Collections.Concurrent;

    public abstract class ConsumerBase<T> : HandlerState, ITimerTask, IConsumer<T>
	{
		public abstract void NegativeAcknowledge(IMessageId messageId);
		public abstract void Resume();
		public abstract void Pause();
		public abstract bool Connected {get;}
		public abstract ValueTask SeekAsync(long timestamp);
		public abstract ValueTask SeekAsync(IMessageId messageId);
		public abstract void Seek(long timestamp);
		public abstract void Seek(IMessageId messageId);
		public abstract void RedeliverUnacknowledgedMessages();
		public abstract bool HasReachedEndOfTopic();
		public abstract IConsumerStats Stats {get;}

		public enum ConsumerType
		{
			Partitioned,
			NonPartitioned
		}
		private readonly string _subscription;
		public readonly ConsumerConfigurationData<T> Conf;
		private readonly string _consumerName;
        public TaskCompletionSource<IConsumer<T>> SubscribeTask { get; set; } 
		protected internal readonly IMessageListener<T> Listener;
		protected internal readonly IConsumerEventListener ConsumerEventListener;
		protected internal readonly ScheduledThreadPoolExecutor ListenerExecutor;
		internal readonly GrowableArrayBlockingQueue<IMessage<T>> IncomingMessages;
		protected internal readonly ConcurrentQueue<TaskCompletionSource<IMessage<T>>> PendingReceives;
        private int _maxReceiverQueueSize;
		protected internal readonly ISchema<T> Schema;
		protected internal readonly ConsumerInterceptors<T> Interceptors;
		protected internal readonly BatchReceivePolicy BatchReceivePolicy;
		protected internal ConcurrentQueue<OpBatchReceive<T>> PendingBatchReceives;
		//protected internal static readonly AtomicLongFieldUpdater<ConsumerBase> IncomingMessagesSizeUpdater = AtomicLongFieldUpdater.newUpdater(typeof(ConsumerBase), "incomingMessagesSize");
		protected internal ConcurrentDictionary<ConsumerBase<T>, long> IncomingMessagesSize = new ConcurrentDictionary<ConsumerBase<T>, long>();
		protected internal volatile ITimeout BatchReceiveTimeout = null;
        public State ConsumerState;

        protected ConsumerBase(PulsarClientImpl client, string topic, ConsumerConfigurationData<T> conf, int receiverQueueSize, ScheduledThreadPoolExecutor listenerExecutor, TaskCompletionSource<IConsumer<T>> subscribeTask, ISchema<T> schema, ConsumerInterceptors<T> interceptors) : base(client, topic)
        {
            ConsumerState = State.Uninitialized;
			_maxReceiverQueueSize = receiverQueueSize;
			_subscription = conf.SubscriptionName;
			Conf = conf;
			_consumerName = conf.ConsumerName ?? Util.ConsumerName.GenerateRandomName();
			SubscribeTask = subscribeTask;
			Listener = conf.MessageListener;
			ConsumerEventListener = conf.ConsumerEventListener;
			// Always use growable queue since items can exceed the advertised size
			IncomingMessages = new GrowableArrayBlockingQueue<IMessage<T>>();

			ListenerExecutor = listenerExecutor;
			PendingReceives = new ConcurrentQueue<TaskCompletionSource<IMessage<T>>>();
			Schema = schema;
			Interceptors = interceptors;
			BatchReceivePolicy = conf.BatchReceivePolicy ?? BatchReceivePolicy.DefaultPolicy;
			if (BatchReceivePolicy.TimeoutMs > 0)
			{
				BatchReceiveTimeout = client.Timer.NewTimeout(this, TimeSpan.FromMilliseconds(BatchReceivePolicy.TimeoutMs));
			}
		}

		public IMessage<T> Receive()
		{
			if (Listener != null)
			{
				throw new PulsarClientException.InvalidConfigurationException("Cannot use receive() when a listener has been set");
			}
			VerifyConsumerState();
			return InternalReceive();
		}

		public ValueTask<IMessage<T>> ReceiveAsync()
		{
			if (Listener != null)
			{
				return new ValueTask<IMessage<T>>(Task.FromException<IMessage<T>>(new PulsarClientException.InvalidConfigurationException("Cannot use receive() when a listener has been set")));
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

		public abstract IMessage<T> InternalReceive();

		public abstract ValueTask<IMessage<T>> InternalReceiveAsync();

		public IMessage<T> Receive(int timeout, BAMCIS.Util.Concurrent.TimeUnit unit)
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
			return InternalReceive(timeout, unit);
		}

		public abstract IMessage<T> InternalReceive(int timeout, BAMCIS.Util.Concurrent.TimeUnit unit);

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

		public abstract IMessages<T> InternalBatchReceive();

		public abstract ValueTask<IMessages<T>> InternalBatchReceiveAsync();

		public void Acknowledge<T1>(IMessage<T1> message)
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

		public void Acknowledge(IMessageId messageId)
		{
			try
			{
				AcknowledgeAsync(messageId);
			}
			catch (System.Exception e)
			{
				throw PulsarClientException.Unwrap(e);
			}
		}

		public void Acknowledge<T1>(IMessages<T1> messages)
		{
			try
			{
				AcknowledgeAsync(messages);
			}
			catch (System.Exception e)
			{
				throw PulsarClientException.Unwrap(e);
			}
		}

		public void AcknowledgeCumulative<T1>(IMessage<T1> message)
		{
			try
			{
				AcknowledgeCumulative(message.MessageId);
			}
			catch (NullReferenceException npe)
			{
				throw new PulsarClientException.InvalidMessageException(npe.Message);
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
				throw PulsarClientException.Unwrap(e);
			}
		}

		public ValueTask AcknowledgeAsync<T1>(IMessage<T1> message)
		{
			try
			{
				return AcknowledgeAsync(message.MessageId);
			}
			catch (NullReferenceException npe)
			{
				return new ValueTask(Task.FromException(new PulsarClientException.InvalidMessageException(npe.Message)));
			}
		}

		public ValueTask AcknowledgeAsync<T1>(IMessages<T1> messages)
		{
			try
            {
                using var msgs = messages.GetEnumerator();
                while (msgs.MoveNext())
                {
                    AcknowledgeAsync(msgs.Current);
                }
				return new ValueTask();
			}
			catch (NullReferenceException npe)
			{
                return new ValueTask(Task.FromException(new PulsarClientException.InvalidMessageException(npe.Message)));
			}
		}

		public ValueTask AcknowledgeCumulativeAsync<T1>(IMessage<T1> message)
		{
			try
			{
				return AcknowledgeCumulativeAsync(message.MessageId);
			}
			catch (NullReferenceException npe)
			{
                return new ValueTask(Task.FromException(new PulsarClientException.InvalidMessageException(npe.Message)));
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
            if (null == txn)
                return DoAcknowledgeWithTxn(messageId, CommandAck.Types.AckType.Individual, new Dictionary<string, long>(),
                    txnImpl);
            if(txn is TransactionImpl impl)
                txnImpl = impl;
            return DoAcknowledgeWithTxn(messageId, CommandAck.Types.AckType.Individual, new Dictionary<string, long>(), txnImpl);
		}

		public ValueTask AcknowledgeCumulativeAsync(IMessageId messageId)
		{
			return AcknowledgeCumulativeAsync(messageId, null);
		}

		// TODO: expose this method to consumer interface when the transaction feature is completed
		// @Override
		public virtual ValueTask AcknowledgeCumulativeAsync(IMessageId messageId, ITransaction txn)
		{
			if (!IsCumulativeAcknowledgementAllowed(Conf.SubscriptionType))
			{
                return new ValueTask(Task.FromException(new PulsarClientException.InvalidConfigurationException("Cannot use cumulative acks on a non-exclusive/non-failover subscription")));
			}

			TransactionImpl txnImpl = null;
            if (null != txn)
            {
                if (txn is TransactionImpl impl)
                    txnImpl = impl;
                return DoAcknowledgeWithTxn(messageId, CommandAck.Types.AckType.Cumulative,
                    new Dictionary<string, long>(), txnImpl);
            }

            return DoAcknowledgeWithTxn(messageId, CommandAck.Types.AckType.Cumulative, new Dictionary<string, long>(), txnImpl);
        }

		public void NegativeAcknowledge<T1>(IMessage<T1> message)
		{
			NegativeAcknowledge(message.MessageId);
		}

		public virtual ValueTask DoAcknowledgeWithTxn(IMessageId messageId, CommandAck.Types.AckType ackType, IDictionary<string, long> properties, TransactionImpl txn)
		{
			var doAcknowledge = DoAcknowledge(messageId, ackType, properties, txn);
			if (txn != null)
			{
				// it is okay that we register acked topic after sending the acknowledgements. because
				// the transactional ack will not be visiable for consumers until the transaction is
				// committed
				txn.RegisterAckedTopic(Topic);
				// register the ackFuture as part of the transaction
				return new ValueTask(Task.FromResult(txn.RegisterAckOp(doAcknowledge)));
			}
			else
			{
				return new ValueTask(Task.FromResult(doAcknowledge.Task));
			}
		}

		public abstract TaskCompletionSource<Task> DoAcknowledge(IMessageId messageId, CommandAck.Types.AckType ackType, IDictionary<string, long> properties, TransactionImpl txn);
		public void NegativeAcknowledge<T1>(IMessages<T1> messages)
        {
            using var msgs = messages.GetEnumerator();
            while (msgs.MoveNext())
            {
				NegativeAcknowledge(msgs.Current);

			}
        }

		public void Unsubscribe()
		{
			try
			{
				UnsubscribeAsync();
			}
			catch (System.Exception e)
			{
				throw PulsarClientException.Unwrap(e);
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
				throw PulsarClientException.Unwrap(e);
			}
		}

		public abstract ValueTask CloseAsync();

		public virtual IMessageId LastMessageId
		{
			get
			{
				try
				{
					return LastMessageIdAsync.Result;
				}
				catch (System.Exception e)
				{
					throw PulsarClientException.Unwrap(e);
				}
			}
		}

		public abstract ValueTask<IMessageId> LastMessageIdAsync {get;}

		private bool IsCumulativeAcknowledgementAllowed(SubscriptionType type)
		{
			return SubscriptionType.Shared != type && SubscriptionType.KeyShared != type;
		}

		public virtual CommandSubscribe.Types.SubType SubType
		{
			get
			{
				var type = Conf.SubscriptionType;
				switch (type)
				{
				    case SubscriptionType.Exclusive:
					    return CommandSubscribe.Types.SubType.Exclusive;
        
				    case SubscriptionType.Shared:
					    return CommandSubscribe.Types.SubType.Shared;
        
				    case SubscriptionType.Failover:
					    return CommandSubscribe.Types.SubType.Failover;
        
				    case SubscriptionType.KeyShared:
					    return CommandSubscribe.Types.SubType.KeyShared;
					default:
                        return CommandSubscribe.Types.SubType.Exclusive;
				}
                
			}
		}

		public abstract int AvailablePermits {get;}

		public abstract int NumMessagesInQueue();

		
		public  string Topic => Topic;

        public  string Subscription => _subscription;

        public virtual string ConsumerName => _consumerName;

        /// <summary>
		/// Redelivers the given unacknowledged messages. In Failover mode, the request is ignored if the consumer is not
		/// active for the given topic. In Shared mode, the consumers messages to be redelivered are distributed across all
		/// the connected consumers. This is a non blocking call and doesn't throw an exception. In case the connection
		/// breaks, the messages are redelivered after reconnect.
		/// </summary>
		public abstract void RedeliverUnacknowledgedMessages(ISet<IMessageId> messageIds);

		public override string ToString()
		{
			return "ConsumerBase{" + "subscription='" + _subscription + '\'' + ", consumerName='" + _consumerName + '\'' + ", topic='" + Topic + '\'' + '}';
		}

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public virtual int MaxReceiverQueueSize
		{
			get => _maxReceiverQueueSize;
            set => _maxReceiverQueueSize = value;
        }

		public virtual IMessage<T> BeforeConsume(IMessage<T> message)
        {
            return Interceptors != null ? Interceptors.BeforeConsume(this, message) : message;
        }

		public virtual void OnAcknowledge(IMessageId messageId, System.Exception exception)
        {
            Interceptors?.OnAcknowledge(this, messageId, exception);
        }

		public virtual void OnAcknowledgeCumulative(IMessageId messageId, System.Exception exception)
        {
            Interceptors?.OnAcknowledgeCumulative(this, messageId, exception);
        }

		public virtual void OnNegativeAcksSend(ISet<IMessageId> messageIds)
        {
            Interceptors?.OnNegativeAcksSend(this, messageIds);
        }

		public virtual void OnAckTimeoutSend(ISet<IMessageId> messageIds)
        {
            Interceptors?.OnAckTimeoutSend(this, messageIds);
        }

		public virtual bool CanEnqueueMessage(IMessage<T> message)
		{
			// Default behavior, can be overridden in subclasses
			return true;
		}

		public virtual bool EnqueueMessageAndCheckBatchReceive(IMessage<T> message)
		{
            if (!CanEnqueueMessage(message)) return HasEnoughMessagesForBatchReceive();
            IncomingMessages.Add(message);
            IncomingMessagesSize[this] = message.Data.Length;
            return HasEnoughMessagesForBatchReceive();
		}

		public virtual bool HasEnoughMessagesForBatchReceive()
		{
			if (BatchReceivePolicy.MaxNumMessages <= 0 && BatchReceivePolicy.MaxNumMessages <= 0)
			{
				return false;
			}
			return (BatchReceivePolicy.MaxNumMessages > 0 && IncomingMessages.size() >= BatchReceivePolicy.MaxNumMessages) || (BatchReceivePolicy.MaxNumBytes > 0 && IncomingMessagesSize[this] >= BatchReceivePolicy.MaxNumBytes);
		}

		private void VerifyConsumerState()
		{
			switch (ConsumerState)
			{
				case State.Ready:
				case State.Connecting:
					break; // Ok
				case State.Closing:
				case State.Closed:
					throw new PulsarClientException.AlreadyClosedException("Consumer already closed");
				case State.Terminated:
					throw new PulsarClientException.AlreadyClosedException("Topic was terminated");
				case State.Failed:
				case State.Uninitialized:
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

		public sealed class OpBatchReceive<T1>
		{

			internal readonly TaskCompletionSource<IMessages<T>> Task;
			internal readonly long CreatedAt;

			public OpBatchReceive(TaskCompletionSource<IMessages<T>> task)
			{
				Task = task;
				CreatedAt = DateTimeHelper.CurrentUnixTimeMillis();
			}

			internal static OpBatchReceive<T1> Of<T1>(TaskCompletionSource<IMessages<T>> task)
			{
				return new OpBatchReceive<T1>(task);
			}
		}

		public virtual void NotifyPendingBatchReceivedCallBack()
		{
			if (!PendingBatchReceives.TryPeek(out var opBatchReceive))
			{
				return;
			}
			NotifyPendingBatchReceivedCallBack(opBatchReceive);
		}

		public virtual void NotifyPendingBatchReceivedCallBack(OpBatchReceive<T> opBatchReceive)
		{
			var messages = NewMessagesImpl;
			var msgPeeked = IncomingMessages.Peek();
			while (msgPeeked != null && messages.CanAdd(msgPeeked))
			{
				IMessage<T> msg = null;
				try
				{
					msg = IncomingMessages.Poll(0L, BAMCIS.Util.Concurrent.TimeUnit.MILLISECONDS);
				}
				catch (ThreadInterruptedException)
				{
					// ignore
				}
				if (msg != null)
				{
					MessageProcessed(msg);
					var interceptMsg = BeforeConsume(msg);
					messages.Add(interceptMsg);
				}
				msgPeeked = IncomingMessages.Peek();
			}
			opBatchReceive.Task.SetResult(messages);
		}

		public abstract void MessageProcessed<T1>(IMessage<T1> msg);

		public void Run(ITimeout timeout)
		{
			if (timeout.Canceled)
			{
				return;
			}

			long timeToWaitMs;

			lock (this)
			{
				// If it's closing/closed we need to ignore this timeout and not schedule next timeout.
				if (ConsumerState == State.Closing || ConsumerState == State.Closed)
				{
					return;
				}
				if (PendingBatchReceives == null)
				{
					PendingBatchReceives = new ConcurrentQueue<OpBatchReceive<T>>();
				}

                timeToWaitMs = BatchReceivePolicy.TimeoutMs;
                PendingBatchReceives.TryPeek(out var firstOpBatchReceive);

				while (firstOpBatchReceive != null)
				{
					// If there is at least one batch receive, calculate the diff between the batch receive timeout
					// and the current time.
					var diff = (firstOpBatchReceive.CreatedAt + BatchReceivePolicy.TimeoutMs) - DateTimeHelper.CurrentUnixTimeMillis();
					if (diff <= 0)
					{
						// The diff is less than or equal to zero, meaning that the batch receive has been timed out.
						// complete the OpBatchReceive and continue to check the next OpBatchReceive in pendingBatchReceives.
						//OpBatchReceive<T> op = PendingBatchReceives.Poll();
                        PendingBatchReceives.TryPeek(out var op);
						CompleteOpBatchReceive(op);
                        PendingBatchReceives.TryPeek(out firstOpBatchReceive);
					}
					else
					{
						// The diff is greater than zero, set the timeout to the diff value
						timeToWaitMs = diff;
						break;
					}
				}
				BatchReceiveTimeout = Client.Timer.NewTimeout(this, TimeSpan.FromMilliseconds(timeToWaitMs));
			}
		}

		public virtual MessagesImpl<T> NewMessagesImpl => new MessagesImpl<T>((int)BatchReceivePolicy.MaxNumMessages, BatchReceivePolicy.MaxNumBytes);

        public virtual bool HasPendingBatchReceive()
		{
			return PendingBatchReceives != null && !PendingBatchReceives.IsEmpty;
		}

		public abstract void CompleteOpBatchReceive(OpBatchReceive<T> Op);
	}

}