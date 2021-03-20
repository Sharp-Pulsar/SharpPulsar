using Akka.Actor;
using Akka.Event;
using Akka.Util.Internal;
using BAMCIS.Util.Concurrent;
using SharpPulsar.Batch.Api;
using SharpPulsar.Common;
using SharpPulsar.Configuration;
using SharpPulsar.Exceptions;
using SharpPulsar.Interfaces;
using SharpPulsar.Messages.Consumer;
using SharpPulsar.Messages.Transaction;
using SharpPulsar.Queues;
using SharpPulsar.Stats.Consumer.Api;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
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

		protected readonly ConsumerQueueCollections<T> ConsumerQueue;
		internal abstract ValueTask<long> LastDisconnectedTimestamp();
		internal abstract void NegativeAcknowledge(IMessageId messageId);
		internal abstract ValueTask Resume();
		internal abstract void Pause();
		internal abstract ValueTask<bool> Connected();
		internal abstract ValueTask Seek(long timestamp);
		internal abstract ValueTask Seek(IMessageId messageId);
		internal abstract ValueTask RedeliverUnacknowledgedMessages();
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
		protected internal BufferBlock<IMessage<T>> IncomingMessages;
		protected internal Dictionary<MessageId, MessageId[]> UnAckedChunckedMessageIdSequenceMap;

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
		public ConsumerActorBase(IActorRef stateActor, IActorRef client, string topic, ConsumerConfigurationData<T> conf, int receiverQueueSize, IAdvancedScheduler listenerExecutor, ISchema<T> schema, ConsumerInterceptors<T> interceptors, ConsumerQueueCollections<T> consumerQueue)
		{
			StateActor = stateActor;
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

			IncomingMessages = ConsumerQueue.IncomingMessages;
			UnAckedChunckedMessageIdSequenceMap = new Dictionary<MessageId, MessageId[]>();

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
			_stateUpdater = Context.System.Scheduler.ScheduleTellRepeatedlyCancelable(TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(1), Self, SendState.Instance, ActorRefs.NoSender);
		}
		

		internal virtual async ValueTask Acknowledge(IMessage<T> message)
		{
			try
			{
				await Acknowledge(message.MessageId);
			}
			catch (Exception npe)
			{
				throw new PulsarClientException.InvalidMessageException(npe.Message);
			}
		}

		
		internal virtual async ValueTask Acknowledge(IMessageId messageId)
		{
			try
			{
				await Acknowledge(messageId, null);
			}
			catch (Exception e)
			{
				throw PulsarClientException.Unwrap(e);
			}
		}

		
		internal virtual async ValueTask Acknowledge(IList<IMessageId> messageIdList)
		{
			try
			{
				await DoAcknowledgeWithTxn(messageIdList, AckType.Individual, new Dictionary<string, long>(), null);
			}
			catch (Exception e)
			{
				throw PulsarClientException.Unwrap(e);
			}
		}

		
		internal virtual async ValueTask Acknowledge(IMessages<T> messages)
		{
			try
			{
				foreach(var x in messages)
                {
					await Acknowledge(x);
				}
			}
			catch (Exception e)
			{
				throw PulsarClientException.Unwrap(e);
			}
		}

		
		internal virtual async ValueTask ReconsumeLater(IMessage<T> message, long delayTime)
		{
			if (!Conf.RetryEnable)
			{
				throw new PulsarClientException("reconsumeLater method not support!");
			}
			try
			{
				await DoReconsumeLater(message, AckType.Individual, new Dictionary<string, long>(), delayTime);
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

		internal virtual async ValueTask ReconsumeLater(IMessages<T> messages, long delayTime)
		{
			try
			{
				messages.ForEach(async message => await ReconsumeLater(message, delayTime));
			}
			catch (NullReferenceException npe)
			{
				throw new PulsarClientException.InvalidMessageException(npe.Message);
			}
			await Task.CompletedTask;
		}

		
		internal virtual async ValueTask AcknowledgeCumulative<T1>(IMessage<T1> message)
		{
			try
			{
				await AcknowledgeCumulative(message.MessageId);
			}
			catch (System.NullReferenceException npe)
			{
				throw new PulsarClientException.InvalidMessageException(npe.Message);
			}
		}

		internal virtual async ValueTask AcknowledgeCumulative(IMessageId messageId)
		{
			try
			{
				await AcknowledgeCumulative(messageId, null); 
			}
			catch (Exception e)
			{
				throw PulsarClientException.Unwrap(e);
			}
		}

		
		internal virtual async ValueTask ReconsumeLaterCumulative(IMessage<T> message, long delayTime)
		{
			try
			{
				await DoReconsumeLater(message, AckType.Cumulative, new Dictionary<string, long>(), delayTime);
			}
			catch (Exception e)
			{
				throw PulsarClientException.Unwrap(e);
			}
		}


		internal virtual async ValueTask Acknowledge(IMessageId messageId, IActorRef txn)
		{
			await DoAcknowledgeWithTxn(messageId, AckType.Individual, new Dictionary<string, long>(), txn);
		}

		internal virtual async ValueTask AcknowledgeCumulative(IMessageId messageId, IActorRef txn)
		{			

			await DoAcknowledgeWithTxn(messageId, AckType.Cumulative, new Dictionary<string, long>(), txn);
		}

		internal void NegativeAcknowledge(IMessage<T> message)
		{
			NegativeAcknowledge(message.MessageId);
		}

		protected internal virtual async ValueTask DoAcknowledgeWithTxn(IList<IMessageId> messageIdList, AckType ackType, IDictionary<string, long> properties, IActorRef txn)
		{
			if (txn != null)
			{
				txn.Tell(new RegisterAckedTopic(Topic, _subscription));		
			}
			await DoAcknowledge(messageIdList, ackType, properties, txn);
		}

		protected internal virtual async ValueTask DoAcknowledgeWithTxn(IMessageId messageId, AckType ackType, IDictionary<string, long> properties, IActorRef txn)
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
			await DoAcknowledge(messageId, ackType, properties, txn);
		}

		protected internal abstract ValueTask DoAcknowledge(IMessageId messageId, AckType ackType, IDictionary<string, long> properties, IActorRef txn);

		protected internal abstract ValueTask DoAcknowledge(IList<IMessageId> messageIdList, AckType ackType, IDictionary<string, long> properties, IActorRef txn);

		protected internal abstract ValueTask DoReconsumeLater(IMessage<T> message, AckType ackType, IDictionary<string, long> properties, long delayTime);

		internal virtual void NegativeAcknowledge(IMessages<T> messages)
		{
			messages.ForEach(NegativeAcknowledge);
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

		internal abstract ValueTask<int> AvailablePermits();

		internal abstract ValueTask<int> NumMessagesInQueue();


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
		protected internal abstract ValueTask RedeliverUnacknowledgedMessages(ISet<IMessageId> messageIds);

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

		protected internal virtual Messages<T> NewMessages
		{
			get
			{
				return new Messages<T>(BatchReceivePolicy.MaxNumMessages, BatchReceivePolicy.MaxNumBytes);
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
				Sender.Tell(new HandlerStateResponse(_state));
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