using Akka.Actor;
using Akka.Event;
using Akka.Util.Internal;
using BAMCIS.Util.Concurrent;
using SharpPulsar.Batch.Api;
using SharpPulsar.Configuration;
using SharpPulsar.Interfaces;
using SharpPulsar.Messages.Consumer;
using SharpPulsar.Stats.Consumer.Api;
using System;
using System.Collections.Generic;
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
		internal abstract void Seek(long timestamp);
		internal abstract void Seek(IMessageId messageId);
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
		protected internal IActorRef UnAckedChunckedMessageIdSequenceMap;

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
		public ConsumerActorBase(IActorRef stateActor, IActorRef lookup, IActorRef connectionPool, string topic, ConsumerConfigurationData<T> conf, int receiverQueueSize, IAdvancedScheduler listenerExecutor, ISchema<T> schema)
		{
            if (conf.Interceptors != null && conf.Interceptors.Count > 0)
                Interceptors = new ConsumerInterceptors<T>(Context.System, conf.Interceptors);

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

            IncomingMessages = new BufferBlock<IMessage<T>>();
			UnAckedChunckedMessageIdSequenceMap = Context.ActorOf(Inter.UnAckedChunckedMessageIdSequenceMap.Prop());

			ListenerExecutor = listenerExecutor;
			Schema = schema;
			
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
			_stateUpdater = Context.System.Scheduler.ScheduleTellRepeatedlyCancelable(TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(5), Self, SendState.Instance, ActorRefs.NoSender);
		}
        protected bool VerifyBatchReceive()
        {
            if (Conf.MessageListener != null)
            {
                Sender.Tell(new AskResponse(new InvalidConfigurationException("Cannot use receive() when a listener has been set")));
                return false;
            }
            if (Conf.ReceiverQueueSize == 0)
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
                        var messages = new Messages<T>(Conf.BatchReceivePolicy.MaxNumMessages, Conf.BatchReceivePolicy.MaxNumBytes);

                        while (IncomingMessages.TryReceive(out var message) && messages.CanAdd(message))
                        {
                            Self.Tell(new MessageProcessed<T>(message));
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
            if (Conf.BatchReceivePolicy.MaxNumMessages <= 0 && Conf.BatchReceivePolicy.MaxNumBytes <= 0)
            {
                return false;
            }
            return (Conf.BatchReceivePolicy.MaxNumMessages > 0 && mesageCount >= Conf.BatchReceivePolicy.MaxNumMessages) 
                || (Conf.BatchReceivePolicy.MaxNumBytes > 0 && mesageSize >= Conf.BatchReceivePolicy.MaxNumBytes);
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
		protected internal abstract void RedeliverUnacknowledged(ISet<IMessageId> messageIds);

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