using Akka.Actor;
using Akka.Event;
using Akka.Util.Internal;
using BAMCIS.Util.Concurrent;
using SharpPulsar.Batch.Api;
using SharpPulsar.Common;
using SharpPulsar.Configuration;
using SharpPulsar.Exceptions;
using SharpPulsar.Interfaces;
using SharpPulsar.Messages.Transaction;
using SharpPulsar.Queues;
using SharpPulsar.Stats.Consumer.Api;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
    public abstract class ConsumerActorBase<T> : ReceiveActor
	{

		protected readonly ConsumerQueueCollections<T> ConsumerQueue;
		internal abstract long LastDisconnectedTimestamp { get; }
		internal abstract void NegativeAcknowledge(IMessageId messageId);
		internal abstract void Resume();
		internal abstract void Pause();
		internal abstract bool Connected { get; }
		internal abstract void Seek(long timestamp);
		internal abstract void Seek(IMessageId messageId);
		internal abstract void RedeliverUnacknowledgedMessages();
		internal abstract IConsumerStatsRecorder Stats { get; }

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
		protected internal readonly IAdvancedScheduler ListenerExecutor;
		protected internal BlockingCollection<IMessage<T>> IncomingMessages;
		protected internal Dictionary<IMessageId, IMessageId[]> UnAckedChunckedMessageIdSequenceMap;

		protected internal int MaxReceiverQueueSize;
		protected internal readonly ISchema<T> Schema;
		protected internal readonly ConsumerInterceptors<T> Interceptors;
		protected internal readonly BatchReceivePolicy BatchReceivePolicy;
		protected internal long IncomingMessagesSize = 0;
		protected internal ICancelable BatchReceiveTimeout = null;
		protected internal HandlerState State;
		private readonly string _topic;

        public ConsumerActorBase()
        {

        }
		public ConsumerActorBase(IActorRef client, string topic, ConsumerConfigurationData<T> conf, int receiverQueueSize, IAdvancedScheduler listenerExecutor, ISchema<T> schema, ConsumerInterceptors<T> interceptors, ConsumerQueueCollections<T> consumerQueue)
		{
			_topic = topic;
			ConsumerQueue = consumerQueue;
			_consumerName = conf.ConsumerName ?? Utility.ConsumerName.GenerateRandomName();
			State = new HandlerState(client, topic, Context.System, _consumerName);
			_log = Context.GetLogger();
			MaxReceiverQueueSize = receiverQueueSize;
			_subscription = conf.SubscriptionName;
			Conf = conf;
			Listener = conf.MessageListener;
			ConsumerEventListener = conf.ConsumerEventListener;

			IncomingMessages = new BlockingCollection<IMessage<T>>();
			UnAckedChunckedMessageIdSequenceMap = new Dictionary<IMessageId, IMessageId[]>();

			ListenerExecutor = listenerExecutor;
			Schema = schema;
			Interceptors = interceptors;
			if (conf.BatchReceivePolicy != null)
			{
				BatchReceivePolicy userBatchReceivePolicy = conf.BatchReceivePolicy;
				if (userBatchReceivePolicy.MaxNumMessages > MaxReceiverQueueSize)
				{
					BatchReceivePolicy = new BatchReceivePolicy.Builder().MaxNumMessages(MaxReceiverQueueSize).MaxNumBytes(userBatchReceivePolicy.MaxNumBytes).Timeout((int)TimeUnit.MILLISECONDS.ToMilliseconds(userBatchReceivePolicy.TimeoutMs)).Build();
					_log.Warning($"BatchReceivePolicy maxNumMessages: {userBatchReceivePolicy.MaxNumMessages} is greater than maxReceiverQueueSize: {MaxReceiverQueueSize}, reset to maxReceiverQueueSize. batchReceivePolicy: {BatchReceivePolicy}");
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
				//BatchReceiveTimeout = ListenerExecutor.ScheduleOnceCancelable(TimeSpan.FromMilliseconds(TimeUnit.MILLISECONDS.ToMilliseconds(BatchReceivePolicy.TimeoutMs)), PendingBatchReceiveTask);
				
			}
		}

		
		internal virtual IMessage<T> Receive()
		{			
			VerifyConsumerState();
			return InternalReceive();
		}

		
		protected internal abstract IMessage<T> InternalReceive();

		
		internal virtual IMessage<T> Receive(int timeout, TimeUnit unit)
		{
			VerifyConsumerState();
			return InternalReceive(timeout, unit);
		}

		protected internal abstract IMessage<T> InternalReceive(int timeout, TimeUnit unit);

		
		internal virtual IMessages<T> BatchReceive()
		{
			VerifyBatchReceive();
			VerifyConsumerState();
			return InternalBatchReceive();
		}

		protected internal abstract IMessages<T> InternalBatchReceive();

		internal virtual void Acknowledge<T1>(IMessage<T1> message)
		{
			try
			{
				Acknowledge(message.MessageId);
			}
			catch (Exception npe)
			{
				throw new PulsarClientException.InvalidMessageException(npe.Message);
			}
		}

		
		internal virtual void Acknowledge(IMessageId messageId)
		{
			try
			{
				Acknowledge(messageId, null);
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
				DoAcknowledgeWithTxn(messageIdList, AckType.Individual, new Dictionary<string, long>(), null);
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
				messages.ForEach(x => Acknowledge(x));
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
				DoReconsumeLater(message, AckType.Individual, new Dictionary<string, long>(), delayTime, unit);
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
				messages.ForEach(message => ReconsumeLater(message, delayTime, unit));
			}
			catch (NullReferenceException npe)
			{
				throw new PulsarClientException.InvalidMessageException(npe.Message);
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
				AcknowledgeCumulative(messageId, null); 
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
				DoReconsumeLater(message, AckType.Cumulative, new Dictionary<string, long>(), delayTime, unit);
			}
			catch (Exception e)
			{
				throw PulsarClientException.Unwrap(e);
			}
		}


		internal virtual void Acknowledge(IMessageId messageId, IActorRef txn)
		{
			DoAcknowledgeWithTxn(messageId, AckType.Individual, new Dictionary<string, long>(), txn);
		}

		internal virtual void AcknowledgeCumulative(IMessageId messageId, IActorRef txn)
		{
			if (!IsCumulativeAcknowledgementAllowed(Conf.SubscriptionType))
			{
				throw new PulsarClientException.InvalidConfigurationException("Cannot use cumulative acks on a non-exclusive/non-failover subscription");
			}

			DoAcknowledgeWithTxn(messageId, AckType.Cumulative, new Dictionary<string, long>(), txn);
		}

		internal virtual void NegativeAcknowledge<T1>(IMessage<T1> message)
		{
			NegativeAcknowledge(message.MessageId);
		}

		protected internal virtual void DoAcknowledgeWithTxn(IList<IMessageId> messageIdList, AckType ackType, IDictionary<string, long> properties, IActorRef txn)
		{
			if (txn != null)
			{
				txn.Tell(new RegisterAckedTopic(Topic, _subscription));
				DoAcknowledge(messageIdList, ackType, properties, txn);				
			}
			else
			{
				DoAcknowledge(messageIdList, ackType, properties, txn);
			}
		}

		protected internal virtual void DoAcknowledgeWithTxn(IMessageId messageId, AckType ackType, IDictionary<string, long> properties, IActorRef txn)
		{
			if (txn != null)
			{
				// it is okay that we register acked topic after sending the acknowledgements. because
				// the transactional ack will not be visiable for consumers until the transaction is
				// committed
				if (ackType == AckType.Cumulative)
				{
					txn.Tell(new RegisterCumulativeAckConsumer(Self));
				}

				txn.Tell(new RegisterAckedTopic(Topic, _subscription));				
			}
			DoAcknowledge(messageId, ackType, properties, txn);
		}

		protected internal abstract void DoAcknowledge(IMessageId messageId, AckType ackType, IDictionary<string, long> properties, IActorRef txn);

		protected internal abstract void DoAcknowledge(IList<IMessageId> messageIdList, AckType ackType, IDictionary<string, long> properties, IActorRef txn);

		protected internal abstract void DoReconsumeLater<T1>(IMessage<T1> message, AckType ackType, IDictionary<string, long> properties, long delayTime, TimeUnit unit);

		internal virtual void NegativeAcknowledge<T1>(IMessages<T1> messages)
		{
			messages.ForEach(NegativeAcknowledge);
		}

		private bool IsCumulativeAcknowledgementAllowed(SubType type)
		{
			return SubType.Shared != type && SubType.KeyShared != type;
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
					default:
						return SubType.Exclusive;
				}

			}
		}

		internal abstract int AvailablePermits { get; }

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
		protected internal abstract void RedeliverUnacknowledgedMessages(ISet<IMessageId> messageIds);

		public override string ToString()
		{
			return "ConsumerBase{" + "subscription='" + _subscription + '\'' + ", consumerName='" + _consumerName + '\'' + ", topic='" + Topic + '\'' + '}';
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

		protected internal virtual bool HasEnoughMessagesForBatchReceive()
		{
			if (BatchReceivePolicy.MaxNumMessages <= 0 && BatchReceivePolicy.MaxNumBytes <= 0)
			{
				return false;
			}
			return (BatchReceivePolicy.MaxNumMessages > 0 && IncomingMessages.Count >= BatchReceivePolicy.MaxNumMessages) || (BatchReceivePolicy.MaxNumBytes > 0 && IncomingMessagesSize >= BatchReceivePolicy.MaxNumBytes);
		}

		protected internal void VerifyConsumerState()
		{
			switch (State.ConnectionState)
			{
				case HandlerState.State.Ready:
				case HandlerState.State.Connecting:
					break; // Ok
					goto case HandlerState.State.Closing;
				case HandlerState.State.Closing:
				case HandlerState.State.Closed:
					throw new PulsarClientException.AlreadyClosedException("Consumer already closed");
				case HandlerState.State.Terminated:
					throw new PulsarClientException.AlreadyClosedException("Topic was terminated");
				case HandlerState.State.Failed:
				case HandlerState.State.Uninitialized:
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

	    protected internal abstract void MessageProcessed<T1>(IMessage<T1> msg);

		protected internal virtual Messages<T> NewMessages
		{
			get
			{
				return new Messages<T>(BatchReceivePolicy.MaxNumMessages, BatchReceivePolicy.MaxNumBytes);
			}
		}

	}

}