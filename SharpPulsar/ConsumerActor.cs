using Akka.Actor;
using Akka.Event;
using Akka.Util;
using Akka.Util.Internal;
using BAMCIS.Util.Concurrent;
using SharpPulsar.Akka;
using SharpPulsar.Auth;
using SharpPulsar.Batch;
using SharpPulsar.Common;
using SharpPulsar.Common.Naming;
using SharpPulsar.Configuration;
using SharpPulsar.Crypto;
using SharpPulsar.Exceptions;
using SharpPulsar.Extension;
using SharpPulsar.Impl;
using SharpPulsar.Interfaces;
using SharpPulsar.Interfaces.ISchema;
using SharpPulsar.Messages;
using SharpPulsar.Messages.Consumer;
using SharpPulsar.Messages.Producer;
using SharpPulsar.Messages.Requests;
using SharpPulsar.Messages.Transaction;
using SharpPulsar.Precondition;
using SharpPulsar.Protocol;
using SharpPulsar.Protocol.Proto;
using SharpPulsar.Shared;
using SharpPulsar.Stats.Consumer;
using SharpPulsar.Tracker;
using SharpPulsar.Tracker.Api;
using SharpPulsar.Tracker.Messages;
using SharpPulsar.Transaction;
using SharpPulsar.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;

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

	public class ConsumerActor<T> : ConsumerActorBase<T>
	{
		private const int MaxRedeliverUnacknowledged = 1000;

		internal readonly long ConsumerId;

		// Number of messages that have delivered to the application. Every once in a while, this number will be sent to the
		// broker to notify that we are ready to get (and store in the incoming messages queue) more messages

		private int _availablePermits = 0;

		private IMessageId _lastDequeuedMessageId = IMessageId.Earliest;
		private IMessageId _lastMessageIdInBroker = IMessageId.Earliest;

		private readonly BlockingCollection<ReceiveResponse> _receiveResponseQueue;
		private readonly BlockingCollection<BatchReceiveResponse> _batchReceiveResponseQueue;
		private readonly BlockingCollection<object> _voidRequestResponseQueue;
		private readonly BlockingCollection<object> _seekRequestResponseQueue;

		private readonly ClientConfigurationData _clientConfigurationData;
		private long _subscribeTimeout;
		private readonly int _partitionIndex;
		private readonly bool _hasParentConsumer;

		private readonly int _receiverQueueRefillThreshold;

		private readonly IActorRef _unAckedMessageTracker;
		private readonly IActorRef _acknowledgmentsGroupingTracker;
		private readonly IActorRef _negativeAcksTracker;
		private readonly CancellationTokenSource _tokenSource;
		private readonly int _priorityLevel;
		private readonly SubscriptionMode _subscriptionMode;
		private BatchMessageId _startMessageId;

		private IActorRef _cnx;

		private BatchMessageId _seekMessageId;
		private bool _duringSeek;

		private readonly BatchMessageId _initialStartMessageId;

		private readonly long _startMessageRollbackDurationInSec;
		private readonly IActorRef _client;

		private volatile bool _hasReachedEndOfTopic;

		private readonly IMessageCrypto _msgCrypto;

		private readonly ImmutableDictionary<string, string> _metadata;

		private readonly bool _readCompacted;
		private readonly bool _resetIncludeHead;

		private readonly SubscriptionInitialPosition _subscriptionInitialPosition;
		private readonly IActorRef _connectionHandler;

		private readonly TopicName _topicName;
		private readonly string _topicNameWithoutPartition;

		private readonly IDictionary<IMessageId, IList<IMessage<T>>> _possibleSendToDeadLetterTopicMessages;

		private readonly DeadLetterPolicy _deadLetterPolicy;

		private IActorRef _deadLetterProducer;

		private volatile IActorRef _retryLetterProducer;

		protected internal volatile bool Paused;

		private Dictionary<string, ChunkedMessageCtx> ChunkedMessagesMap = new Dictionary<string, ChunkedMessageCtx>();
		private int _pendingChunckedMessageCount = 0;
		protected internal long ExpireTimeOfIncompleteChunkedMessageMillis = 0;
		private bool _expireChunkMessageTaskScheduled = false;
		private int _maxPendingChuckedMessage;
		// if queue size is reasonable (most of the time equal to number of producers try to publish messages concurrently on
		// the topic) then it guards against broken chuncked message which was not fully published
		private bool _autoAckOldestChunkedMessageOnQueueFull;
		// it will be used to manage N outstanding chunked mesage buffers
		private readonly Queue<string> _pendingChunckedMessageUuidQueue;
		private readonly ILoggingAdapter _log;

		private readonly bool _createTopicIfDoesNotExist;

		private IActorRef _clientCnxUsedForConsumerRegistration;

		public static Props NewConsumer(IActorRef client, string topic, ConsumerConfigurationData<T> conf, IAdvancedScheduler listenerExecutor, int partitionIndex, bool hasParentConsumer, IMessageId startMessageId, ISchema<T> schema, ConsumerInterceptors<T> interceptors, bool createTopicIfDoesNotExist, ClientConfigurationData clientConfigurationData)
        {
			return Props.Create(() => new ConsumerActor<T>(client, topic, conf, listenerExecutor, partitionIndex, hasParentConsumer, startMessageId, 0, schema, interceptors, createTopicIfDoesNotExist, clientConfigurationData));
        }
		
		public static Props NewConsumer(IActorRef client, string topic, ConsumerConfigurationData<T> conf, IAdvancedScheduler listenerExecutor, int partitionIndex, bool hasParentConsumer, IMessageId startMessageId, ISchema<T> schema, ConsumerInterceptors<T> interceptors, bool createTopicIfDoesNotExist, long startMessageRollbackDurationInSec, ClientConfigurationData clientConfigurationData)
		{
			if(conf.ReceiverQueueSize == 0)
			{
				return ZeroQueueConsumer<T>.NewZeroQueueConsumer(client, topic, conf, listenerExecutor, partitionIndex, hasParentConsumer, startMessageId, schema, interceptors, createTopicIfDoesNotExist, clientConfigurationData);
			}
			else
			{
				return Props.Create(() => new ConsumerActor<T>(client, topic, conf, listenerExecutor, partitionIndex, hasParentConsumer, startMessageId, startMessageRollbackDurationInSec, schema, interceptors, createTopicIfDoesNotExist, clientConfigurationData));
			}
		}

		public ConsumerActor(IActorRef client, string topic, ConsumerConfigurationData<T> conf, IAdvancedScheduler listenerExecutor, int partitionIndex, bool hasParentConsumer, IMessageId startMessageId, long startMessageRollbackDurationInSec, ISchema<T> schema, ConsumerInterceptors<T> interceptors, bool createTopicIfDoesNotExist, ClientConfigurationData clientConfiguration) : base(client, topic, conf, conf.ReceiverQueueSize, listenerExecutor, schema, interceptors)
		{

			_tokenSource = new CancellationTokenSource();
			_log = Context.GetLogger();
			_client = client;
			ConsumerId = client.AskFor<long>(NewConsumerId.Instance);
			_subscriptionMode = conf.SubscriptionMode;
			_startMessageId = startMessageId != null ? new BatchMessageId((MessageId) startMessageId) : null;
			_initialStartMessageId = _startMessageId;
			_startMessageRollbackDurationInSec = startMessageRollbackDurationInSec;
			_availablePermits = 0;
			_subscribeTimeout = DateTimeHelper.CurrentUnixTimeMillis() + clientConfiguration.OperationTimeoutMs;
			_partitionIndex = partitionIndex;
			_hasParentConsumer = hasParentConsumer;
			_receiverQueueRefillThreshold = conf.ReceiverQueueSize / 2;
			_priorityLevel = conf.PriorityLevel;
			_readCompacted = conf.ReadCompacted;
			_subscriptionInitialPosition = conf.SubscriptionInitialPosition;
			_negativeAcksTracker = new NegativeAcksTracker<T>(conf, Self);
			_resetIncludeHead = conf.ResetIncludeHead;
			_createTopicIfDoesNotExist = createTopicIfDoesNotExist;
			_maxPendingChuckedMessage = conf.MaxPendingChuckedMessage;
			_pendingChunckedMessageUuidQueue = new Queue<string>();
			ExpireTimeOfIncompleteChunkedMessageMillis = conf.ExpireTimeOfIncompleteChunkedMessageMillis;
			_autoAckOldestChunkedMessageOnQueueFull = conf.AutoAckOldestChunkedMessageOnQueueFull;

			if(clientConfiguration.StatsIntervalSeconds > 0)
			{
				StatsConflict = new ConsumerStatsRecorder(client, conf, this);
			}
			else
			{
				StatsConflict = ConsumerStatsDisabled.Instance;
			}

			_duringSeek = new AtomicBoolean(false);

			if(conf.AckTimeoutMillis != 0)
			{
				if(conf.TickDurationMillis > 0)
				{
					_unAckedMessageTracker = new UnAckedMessageTracker(client, this, conf.AckTimeoutMillis, Math.Min(conf.TickDurationMillis, conf.AckTimeoutMillis));
				}
				else
				{
					_unAckedMessageTracker = new UnAckedMessageTracker(client, this, conf.AckTimeoutMillis);
				}
			}
			else
			{
				_unAckedMessageTracker = UnAckedMessageTracker.UNACKED_MESSAGE_TRACKER_DISABLED;
			}

			// Create msgCrypto if not created already
			if(conf.CryptoKeyReader != null)
			{
				if(conf.MessageCrypto != null)
				{
					_msgCrypto = conf.MessageCrypto;
				}
				else
				{
					// default to use MessageCryptoBc;
					IMessageCrypto msgCryptoBc;
					try
					{
						msgCryptoBc = new MessageCrypto($"[{topic}] [{Subscription}]", false, _log);
					}
					catch(Exception e)
					{
						_log.Error("MessageCryptoBc may not included in the jar. e:", e);
						msgCryptoBc = null;
					}
					_msgCrypto = msgCryptoBc;
				}
			}
			else
			{
				_msgCrypto = null;
			}

			if(conf.Properties.Count == 0)
			{
				_metadata = ImmutableDictionary.Create<string,string>();
			}
			else
			{
				_metadata = new Dictionary<string,string>(conf.Properties).ToImmutableDictionary();
			}

			_connectionHandler = Context.ActorOf(ConnectionHandler.Prop(State, new BackoffBuilder().SetInitialTime(clientConfiguration.InitialBackoffIntervalNanos, TimeUnit.NANOSECONDS).SetMax(clientConfiguration.MaxBackoffIntervalNanos, TimeUnit.NANOSECONDS).SetMandatoryStop(0, TimeUnit.MILLISECONDS).Create(), Self));

			_topicName = TopicName.Get(topic);
			if(_topicName.Persistent)
			{
				_acknowledgmentsGroupingTracker = Context.ActorOf(PersistentAcknowledgmentsGroupingTracker<T>.Prop(client, ConsumerId, conf));
			}
			else
			{
				_acknowledgmentsGroupingTracker = Context.ActorOf(NonPersistentAcknowledgmentGroupingTracker.Prop());
			}

			if(conf.DeadLetterPolicy != null)
			{
				_possibleSendToDeadLetterTopicMessages = new Dictionary<IMessageId, IList<IMessage<T>>>();
				if(!string.IsNullOrWhiteSpace(conf.DeadLetterPolicy.DeadLetterTopic))
				{
					_deadLetterPolicy = new DeadLetterPolicy()
					{
						MaxRedeliverCount = conf.DeadLetterPolicy.MaxRedeliverCount,
						DeadLetterTopic = conf.DeadLetterPolicy.DeadLetterTopic
					};
				}
				else
				{
					_deadLetterPolicy = new DeadLetterPolicy()
					{
						MaxRedeliverCount = conf.DeadLetterPolicy.MaxRedeliverCount,
						DeadLetterTopic = $"{RetryMessageUtil.DlqGroupTopicSuffix}-{topic} {Subscription}"
					};
				}

				if(!string.IsNullOrWhiteSpace(conf.DeadLetterPolicy.RetryLetterTopic))
				{
					_deadLetterPolicy.RetryLetterTopic = conf.DeadLetterPolicy.RetryLetterTopic;
				}
				else
				{
					_deadLetterPolicy.RetryLetterTopic = string.Format("{0}-{1}" + RetryMessageUtil.RetryGroupTopicSuffix, topic, Subscription);
				}

			}
			else
			{
				_deadLetterPolicy = null;
				_possibleSendToDeadLetterTopicMessages = null;
			}

			_topicNameWithoutPartition = _topicName.PartitionedTopicName;
			_ackRequests = new ConcurrentLongHashMap<OpForAckCallBack>(16, 1);

			 GrabCnx();
		}
		private void SetupReceives()
        {
			Receive<ConnectionOpened>(c => {
				ConnectionOpened(c.ClientCnx);
			});
        }
		internal virtual IActorRef UnAckedMessageTracker
		{
			get
			{
				return _unAckedMessageTracker;
			}
		}

		private void Unsubscribe()
		{
			if(State.ConnectionState == HandlerState.State.Closing || State.ConnectionState == HandlerState.State.Closed)
			{
				Sender.Tell(new Failure { Exception = new Exception("AlreadyClosedException: Consumer was already closed") });
			}
            else
            {
				if (Connected)
				{
					State.ConnectionState = HandlerState.State.Closing;
					var requestId =  _client.AskFor<NewRequestIdResponse>(NewRequestId.Instance).Id;
					var unsubscribe = Commands.NewUnsubscribe(ConsumerId, requestId);
				    var cnx = _connectionHandler;
					var send = new SendRequestWithId(unsubscribe, requestId);
                    if (cnx.AskFor<bool>(send))
                    {
						CloseConsumerTasks();
						DeregisterFromClientCnx();
						_client.Tell(new CleanupConsumer(Self));
						_log.Info($"[{Topic}][{Subscription}] Successfully unsubscribed from topic");
						State.ConnectionState = HandlerState.State.Closed;
						Sender.Tell(true);
					}
                    else
                    {
						_log.Error($"[{Topic}][{Subscription}] Failed to unsubscribe");
						State.ConnectionState = HandlerState.State.Ready;
						Sender.Tell(false);

					}
				}
				else
				{
					Sender.Tell(false);
					_log.Error(new PulsarClientException($"The client is not connected to the broker when unsubscribing the subscription {Subscription} of the topic {_topicName}").ToString());
				}
			}
		}
        protected override void PostStop()
        {
			_tokenSource.Cancel();
            base.PostStop();
        }
        protected internal override void InternalReceive()
		{
			IMessage<T> message;
			try
			{
				message = IncomingMessages.Take(_tokenSource.Token);
				MessageProcessed(message);
				var msg = BeforeConsume(message);
				if (_hasParentConsumer)
					Sender.Tell(new ReceiveResponse(msg));
				else
					_receiveResponseQueue.Add(new ReceiveResponse(msg));
			}
			catch(Exception e)
			{
				var error = new ReceiveResponse(new Failure { Exception = PulsarClientException.Unwrap(e) });
				Stats.IncrementNumReceiveFailed();
				if (_hasParentConsumer)
					Sender.Tell(error);
				else
					_receiveResponseQueue.Add(error);
			}
		}

		private void InternalReceive(int timeout)
		{
            try
            {
                if (!IncomingMessages.TryTake(out IMessage<T> message, timeout, _tokenSource.Token))
                {
					if (_hasParentConsumer)
						Sender.Tell(new ReceiveResponse(null));
					else
						_receiveResponseQueue.Add(new ReceiveResponse(null));
				}
                else
				{
					MessageProcessed(message);
					var msg = BeforeConsume(message); 
					if (_hasParentConsumer)
						Sender.Tell(new ReceiveResponse(msg));
					else
						_receiveResponseQueue.Add(new ReceiveResponse(msg));
				}
            }
            catch (Exception e)
            {
				HandlerState.State state = State.ConnectionState;
				if (state != HandlerState.State.Closing && state != HandlerState.State.Closed)
				{
					var error = new ReceiveResponse(new Failure { Exception = PulsarClientException.Unwrap(e) });
					Stats.IncrementNumReceiveFailed();
					if (_hasParentConsumer)
						Sender.Tell(error);
					else
						_receiveResponseQueue.Add(error);		
                }
                else
                {
                    _log.Error(e.ToString());
                }
            }
        }

		protected internal override void InternalBatchReceive()
		{
			try
			{
				var messages = NewMessages;
				if (HasEnoughMessagesForBatchReceive())
				{
					IMessage<T> msg = IncomingMessages.Take();
					while (IncomingMessages.TryTake(out msg) && messages.CanAdd(msg))
					{
						MessageProcessed(msg);
						IMessage<T> interceptMsg = BeforeConsume(msg);
						messages.Add(interceptMsg);
					}
				}
				if (_hasParentConsumer)
					Sender.Tell(new BatchReceiveResponse(messages));
				else
					_batchReceiveResponseQueue.Add(new BatchReceiveResponse(messages));
			}
			catch(Exception e) 
			{
				HandlerState.State state = State.ConnectionState;
				var error = PulsarClientException.Unwrap(e);
				if (state != HandlerState.State.Closing && state != HandlerState.State.Closed)
				{
					Stats.IncrementNumBatchReceiveFailed();
				}
				if (_hasParentConsumer)
					Sender.Tell(new BatchReceiveResponse(error));
				else
					_batchReceiveResponseQueue.Add(new BatchReceiveResponse(error));
			}
		}


		internal virtual bool MarkAckForBatchMessage(BatchMessageId batchMessageId, CommandAck.AckType ackType, IDictionary<string, long> properties, IActorRef txn)
		{
			bool isAllMsgsAcked;
			if(ackType == CommandAck.AckType.Individual)
			{
				isAllMsgsAcked = txn == null && batchMessageId.AckIndividual();
			}
			else
			{
				isAllMsgsAcked = batchMessageId.AckCumulative();
			}
			int outstandingAcks = 0;
			if(_log.IsDebugEnabled)
			{
				outstandingAcks = batchMessageId.OutstandingAcksInSameBatch;
			}

			int batchSize = batchMessageId.BatchSize;
			// all messages in this batch have been acked
			if(isAllMsgsAcked)
			{
				if(_log.IsDebugEnabled)
				{
					_log.Debug($"[{Subscription}] [{ConsumerName}] can ack message to broker {batchMessageId}, acktype {ackType}, cardinality {outstandingAcks}, length {batchSize}");
				}
				return true;
			}
			else
			{
				if(CommandAck.AckType.Cumulative == ackType && !batchMessageId.Acker.PrevBatchCumulativelyAcked)
				{
					SendAcknowledge(batchMessageId.PrevBatchMessageId(), CommandAck.AckType.Cumulative, properties, null);
					batchMessageId.Acker.PrevBatchCumulativelyAcked = true;
				}
				else
				{
					OnAcknowledge(batchMessageId, null);
				}
				if(_log.IsDebugEnabled)
				{
					_log.Debug($"[{Subscription}] [{ConsumerName}] cannot ack message to broker {batchMessageId}, acktype {ackType}, pending acks - {outstandingAcks}");
				}
			}
			return false;
		}

		private void DoAcknowledge(IMessageId messageId, CommandAck.AckType ackType, IDictionary<string, long> properties, IActorRef txn)
		{
			Condition.CheckArgument(messageId is MessageId);
			if(State.ConnectionState != HandlerState.State.Ready && State.ConnectionState != HandlerState.State.Connecting)
			{
				Stats.IncrementNumAcksFailed();
				PulsarClientException exception = new PulsarClientException("Consumer not ready. State: " + State);
				if(CommandAck.AckType.Individual.Equals(ackType))
				{
					OnAcknowledge(messageId, exception);
				}
				else if(CommandAck.AckType.Cumulative.Equals(ackType))
				{
					OnAcknowledgeCumulative(messageId, exception);
				}
				//return FutureUtil.FailedFuture(exception);
			}

			if(txn != null)
			{
				var bits = txn.AskFor<GetTxnIdBitsResponse>(GetTxnIdBits.Instance);
				DoTransactionAcknowledgeForResponse(messageId, ackType, null, properties, new TxnID(bits.MostBits, bits.LeastBits));
			}

			if(messageId is BatchMessageId)
			{
				var batchMessageId = (BatchMessageId) messageId;
				if(ackType == CommandAck.AckType.Cumulative && txn != null)
				{
					SendAcknowledge(messageId, ackType, properties, txn);
				}
				if(MarkAckForBatchMessage(batchMessageId, ackType, properties, txn))
				{
					// all messages in batch have been acked so broker can be acked via sendAcknowledge()
					if(_log.IsDebugEnabled)
					{
						_log.Debug($"[{Subscription}] [{ConsumerName}] acknowledging message - {messageId}, acktype {ackType}");
					}
				}
				else
				{
					if(Conf.BatchIndexAckEnabled)
					{
						_acknowledgmentsGroupingTracker.Tell(new AddBatchIndexAcknowledgment(batchMessageId, batchMessageId.BatchIndex, batchMessageId.BatchSize, ackType, properties, txn));
					}
					// other messages in batch are still pending ack.
					//return CompletableFuture.completedFuture(null);
				}
			}
			SendAcknowledge(messageId, ackType, properties, txn);
		}

		private void DoAcknowledge(IList<IMessageId> messageIdList, CommandAck.AckType ackType, IDictionary<string, long> properties, IActorRef txn)
		{
			if(CommandAck.AckType.Cumulative.Equals(ackType))
			{
				messageIdList.ForEach(messageId => DoAcknowledge(messageId, ackType, properties, txn));
			}
			if(State.ConnectionState != HandlerState.State.Ready && State.ConnectionState != HandlerState.State.Connecting)
			{
				Stats.IncrementNumAcksFailed();
				PulsarClientException exception = new PulsarClientException("Consumer not ready. State: " + State);
				messageIdList.ForEach(messageId => OnAcknowledge(messageId, exception));
				//return FutureUtil.FailedFuture(exception);
			}
			IList<MessageId> nonBatchMessageIds = new List<MessageId>();
			foreach(MessageId messageId in messageIdList)
			{
				MessageId messageIdImpl;
				if(messageId is BatchMessageId && !MarkAckForBatchMessage((BatchMessageId) messageId, ackType, properties, txn))
				{
					BatchMessageId batchMessageId = (BatchMessageId) messageId;
					messageIdImpl = new MessageId(batchMessageId.LedgerId, batchMessageId.EntryId, batchMessageId.PartitionIndex);
					_acknowledgmentsGroupingTracker.Tell(new AddBatchIndexAcknowledgment(batchMessageId, batchMessageId.BatchIndex, batchMessageId.BatchSize, ackType, properties, txn));
					Stats.IncrementNumAcksSent(batchMessageId.BatchSize);
				}
				else
				{
					messageIdImpl = (MessageId) messageId;
					Stats.IncrementNumAcksSent(1);
					nonBatchMessageIds.Add(messageIdImpl);
				}
				_unAckedMessageTracker.Tell(new Remove(messageIdImpl));
				if(_possibleSendToDeadLetterTopicMessages != null)
				{
					_possibleSendToDeadLetterTopicMessages.Remove(messageIdImpl);
				}
				OnAcknowledge(messageId, null);
			}
			if(nonBatchMessageIds.Count > 0)
			{
				_acknowledgmentsGroupingTracker.Tell(new AddListAcknowledgment(nonBatchMessageIds, ackType, properties));
			}
			//return CompletableFuture.completedFuture(null);
		}

		private void DoReconsumeLater<T1>(IMessage<T1> message, CommandAck.AckType ackType, IDictionary<string, long> properties, long delayTime, TimeUnit unit)
		{
			IMessageId messageId = message.MessageId;
			if(messageId is TopicMessageId)
			{
				messageId = ((TopicMessageId)messageId).InnerMessageId;
			}
			Condition.CheckArgument(messageId is MessageId);
			if(State.ConnectionState != HandlerState.State.Ready && State.ConnectionState != HandlerState.State.Connecting)
			{
				Stats.IncrementNumAcksFailed();
				PulsarClientException exception = new PulsarClientException("Consumer not ready. State: " + State);
				if(CommandAck.AckType.Individual.Equals(ackType))
				{
					OnAcknowledge(messageId, exception);
				}
				else if(CommandAck.AckType.Cumulative.Equals(ackType))
				{
					OnAcknowledgeCumulative(messageId, exception);
				}
				//return FutureUtil.FailedFuture(exception);
			}
			if(delayTime < 0)
			{
				delayTime = 0;
			}
			if(_retryLetterProducer == null)
			{
				try
				{
					if(_retryLetterProducer == null)
					{
						_retryLetterProducer = _client.AskFor<IActorRef>(new NewProducerActor<T>(Schema, _deadLetterPolicy.RetryLetterTopic,false, false));
					}
				}
				catch(Exception e)
				{
					_log.Error($"Create retry letter producer exception with topic: {_deadLetterPolicy.RetryLetterTopic}:{e}");
				}
			}
			if(_retryLetterProducer != null)
			{
				try
				{
					Message<T> retryMessage = null;
					string originMessageIdStr = null;
					string originTopicNameStr = null;
					if(message is TopicMessage<T> tm)
					{
						retryMessage = (Message<T>)tm.Message;
						originMessageIdStr = ((TopicMessageId) tm.MessageId).InnerMessageId.ToString();
						originTopicNameStr = ((TopicMessageId) tm.MessageId).TopicName;
					}
					else if(message is Message<T> m)
					{
						retryMessage = m;
						originMessageIdStr = m.MessageId.ToString();
						originTopicNameStr = m.TopicName;
					}
					SortedDictionary<string, string> propertiesMap = new SortedDictionary<string, string>();
					int reconsumetimes = 1;
					if(message.Properties != null)
					{
						message.Properties.ForEach(x=> new KeyValuePair<string, string>(x.Key, x.Value));
					}

					if(propertiesMap.ContainsKey(RetryMessageUtil.SystemPropertyReconsumetimes))
					{
						reconsumetimes = Convert.ToInt32(propertiesMap.GetValueOrNull(RetryMessageUtil.SystemPropertyReconsumetimes));
						reconsumetimes = reconsumetimes + 1;

					}
					else
					{
						propertiesMap[RetryMessageUtil.SystemPropertyRealTopic] = originTopicNameStr;
						propertiesMap[RetryMessageUtil.SystemPropertyOriginMessageId] = originMessageIdStr;
					}

					propertiesMap[RetryMessageUtil.SystemPropertyReconsumetimes] = reconsumetimes.ToString();
					propertiesMap[RetryMessageUtil.SystemPropertyDelayTime] = unit.ToMilliseconds(delayTime).ToString();

				   if(reconsumetimes > _deadLetterPolicy.MaxRedeliverCount)
				   {
					   ProcessPossibleToDLQ((MessageId)messageId);
						if(_deadLetterProducer == null)
						{
							try
							{
								if(_deadLetterProducer == null)
								{
									_deadLetterProducer = _client.AskFor<IActorRef>(new NewProducerActor<T>(Schema, _deadLetterPolicy.RetryLetterTopic, false, false)); ;
								}
							}
							catch(Exception e)
							{
							   _log.Error("Create dead letter producer exception with topic: {}", _deadLetterPolicy.DeadLetterTopic, e);
							}
						}
						if (_deadLetterProducer != null)
						{
							propertiesMap[RetryMessageUtil.SystemPropertyRealTopic] = originTopicNameStr;
							propertiesMap[RetryMessageUtil.SystemPropertyOriginMessageId] = originMessageIdStr;
							TypedMessageBuilder<T> typedMessageBuilderNew = new TypedMessageBuilder<T>(_deadLetterProducer, Schema);
							typedMessageBuilderNew.Value(retryMessage.Value);
							typedMessageBuilderNew.Properties(propertiesMap);
							typedMessageBuilderNew.Send();
							DoAcknowledge(messageId, ackType, properties, null);
						}
				   }
					else
					{
						TypedMessageBuilder<T> typedMessageBuilderNew = new TypedMessageBuilder<T>(_retryLetterProducer, Schema);
						typedMessageBuilderNew.Value(retryMessage.Value);
						typedMessageBuilderNew.Properties(propertiesMap);
						if (delayTime > 0)
						{
							typedMessageBuilderNew.DeliverAfter(delayTime, unit);
						}
						if(message.HasKey())
						{
							typedMessageBuilderNew.Key(message.Key);
						}
						typedMessageBuilderNew.Send();
						DoAcknowledge(messageId, ackType, properties, null);
					}
				}
				catch(Exception e)
				{
					_log.Error($"Send to retry letter topic exception with topic: {_deadLetterPolicy.DeadLetterTopic}, messageId: {messageId}");
                    ISet<IMessageId> messageIds = new HashSet<IMessageId>
                    {
                        messageId
                    };
                    _unAckedMessageTracker.Tell(new Remove(messageId));
					RedeliverUnacknowledgedMessages(messageIds);
				}
			}

		}

		// TODO: handle transactional acknowledgements.
		private void SendAcknowledge(IMessageId messageId, CommandAck.AckType ackType, IDictionary<string, long> properties, IActorRef txnImpl)
		{
			var msgId = (MessageId) messageId;

			if(ackType == CommandAck.AckType.Individual)
			{
				if(messageId is BatchMessageId)
				{
					var batchMessageId = (BatchMessageId) messageId;

					Stats.IncrementNumAcksSent(batchMessageId.BatchSize);
					_unAckedMessageTracker.Tell(new Remove(new MessageId(batchMessageId.LedgerId, batchMessageId.EntryId, batchMessageId.PartitionIndex)));
					if(_possibleSendToDeadLetterTopicMessages != null)
					{
						_possibleSendToDeadLetterTopicMessages.Remove(new MessageId(batchMessageId.LedgerId, batchMessageId.EntryId, batchMessageId.PartitionIndex));
					}
				}
				else
				{
					// increment counter by 1 for non-batch msg
					_unAckedMessageTracker.Tell(new Remove(msgId));
					if(_possibleSendToDeadLetterTopicMessages != null)
					{
						_possibleSendToDeadLetterTopicMessages.Remove(msgId);
					}
					Stats.IncrementNumAcksSent(1);
				}
				OnAcknowledge(messageId, null);
			}
			else if(ackType == CommandAck.AckType.Cumulative)
			{
				OnAcknowledgeCumulative(messageId, null);
				Stats.IncrementNumAcksSent(_unAckedMessageTracker.Tell(new RemoveMessagesTill(msgId)));
			}

			_acknowledgmentsGroupingTracker.Tell(new AddAcknowledgment(msgId, ackType, properties, txnImpl));

			// Consumer acknowledgment operation immediately succeeds. In any case, if we're not able to send ack to broker,
			// the messages will be re-delivered
			//return CompletableFuture.completedFuture(null);
		}

		private void NegativeAcknowledge(IMessageId messageId)
		{
			_negativeAcksTracker.Tell(new Add(messageId));

			// Ensure the message is not redelivered for ack-timeout, since we did receive an "ack"
			_unAckedMessageTracker.Tell(new Remove(messageId));
		}

		private void ConnectionOpened(IActorRef cnx)
		{
			if(State.ConnectionState == HandlerState.State.Closing || State.ConnectionState == HandlerState.State.Closed)
			{
				State.ConnectionState = HandlerState.State.Closed;
				CloseConsumerTasks();
				DeregisterFromClientCnx();
				_client.Tell(new CleanupConsumer(Self));
				FailPendingReceive();
				ClearReceiverQueue();
				return;
			}
			_cnx = cnx;

			_log.Info($"[{Topic}][{Subscription}] Subscribing to topic on cnx {Cnx().Path.Name}, consumerId {ConsumerId}");

			long requestId = _client.AskFor<NewRequestIdResponse>(NewRequestId.Instance).Id;

			int currentSize; 
			currentSize = IncomingMessages.Count;
			_startMessageId = ClearReceiverQueue();
			if (_possibleSendToDeadLetterTopicMessages != null)
			{
				_possibleSendToDeadLetterTopicMessages.Clear();
			}

			bool isDurable = _subscriptionMode == SubscriptionMode.Durable;
			MessageIdData startMessageIdData = null;
			if(isDurable)
			{
				// For regular durable subscriptions, the message id from where to restart will be determined by the broker.
				startMessageIdData = null;
			}
			else if(_startMessageId != null)
			{
                // For non-durable we are going to restart from the next entry
                var builder = new MessageIdData
                {
                    ledgerId = (ulong)_startMessageId.LedgerId,
                    entryId = (ulong)_startMessageId.EntryId
                };
                if (_startMessageId is BatchMessageId)
				{
					builder.BatchIndex = _startMessageId.BatchIndex;
				}

			}

			ISchemaInfo si = Schema.SchemaInfo;
			if(si != null && (SchemaType.BYTES == si.Type || SchemaType.NONE == si.Type))
			{
				// don't set schema for Schema.BYTES
				si = null;
			}
			// startMessageRollbackDurationInSec should be consider only once when consumer connects to first time
			long startMessageRollbackDuration = (_startMessageRollbackDurationInSec > 0 && _startMessageId != null && _startMessageId.Equals(_initialStartMessageId)) ? _startMessageRollbackDurationInSec : 0;
			var request = Commands.NewSubscribe(Topic, Subscription, ConsumerId, requestId, SubType, _priorityLevel, ConsumerName, isDurable, startMessageIdData, _metadata, _readCompacted, Conf.ReplicateSubscriptionState, CommandSubscribe.InitialPosition.ValueOf(_subscriptionInitialPosition.Value), startMessageRollbackDuration, si, _createTopicIfDoesNotExist, Conf.KeySharedPolicy);
			
			var result = cnx.AskFor<CommandSuccessResponse>(new SendRequestWithId(request, requestId));
			if(result.Success != null)
            {
				if (State.ChangeToReadyState())
				{
					ConsumerIsReconnectedToBroker(cnx, currentSize);
				}
				else
				{
					State.ConnectionState = HandlerState.State.Closed;
					DeregisterFromClientCnx();
					_client.Tell(new CleanupConsumer(Self));
					cnx.GracefulStop(TimeSpan.FromSeconds(1));
					return;
				}
				ResetBackoff();
				if (!(_hasParentConsumer && isDurable) && Conf.ReceiverQueueSize != 0)
				{
					IncreaseAvailablePermits(cnx, Conf.ReceiverQueueSize);
				}
			}
            else
            {
				DeregisterFromClientCnx();
				if (State.ConnectionState == HandlerState.State.Closing || State.ConnectionState == HandlerState.State.Closed)
				{
					cnx.GracefulStop(TimeSpan.FromSeconds(1));
					return;
				}
				
				_log.Warning($"[{Topic}][{Subscription}] Failed to subscribe to topic on");
				if (e.Cause is PulsarClientException && PulsarClientException.IsRetriableError(e.Cause) && DateTimeHelper.CurrentUnixTimeMillis() < _subscribeTimeout)
				{
					ReconnectLater(e.Cause);
				}
				else if (!SubscribeFutureConflict.Done)
				{
					State = State.Failed;
					CloseConsumerTasks();
					SubscribeFutureConflict.completeExceptionally(PulsarClientException.Wrap(e, string.Format("Failed to subscribe the topic {0} with subscription " + "name {1} when connecting to the broker", _topicName.ToString(), SubscriptionConflict)));
					ClientConflict.CleanupConsumer(this);
				}
				else if (e.Cause is PulsarClientException.TopicDoesNotExistException)
				{
					State = State.Failed;
					CloseConsumerTasks();
					ClientConflict.CleanupConsumer(this);
					_log.warn("[{}][{}] Closed consumer because topic does not exist anymore {}", Topic, SubscriptionConflict, cnx.Channel().remoteAddress());
				}
				else
				{
					ReconnectLater(e.Cause);
				}
			}
		}

		protected internal virtual void ConsumerIsReconnectedToBroker(IActorRef cnx, int currentQueueSize)
		{
			_log.Info($"[{Topic}][{Subscription}] Subscribed to topic on -- consumer: {ConsumerId}");

			_availablePermits = 0;
		}

		/// <summary>
		/// Clear the internal receiver queue and returns the message id of what was the 1st message in the queue that was
		/// not seen by the application
		/// </summary>
		private BatchMessageId ClearReceiverQueue()
		{
			IList<Message<object>> currentMessageQueue = new List<Message<object>>(IncomingMessages.size());
			IncomingMessages.drainTo(currentMessageQueue);
			IncomingMessagesSize = 0;

			if(_duringSeek.compareAndSet(true, false))
			{
				return _seekMessageId;
			}
			else if(_subscriptionMode == SubscriptionMode.Durable)
			{
				return _startMessageId;
			}

			if(currentMessageQueue.Count > 0)
			{
				var nextMessageInQueue = (MessageId) currentMessageQueue[0].MessageId;
				BatchMessageId previousMessage;
				if(nextMessageInQueue is BatchMessageId)
				{
					// Get on the previous message within the current batch
					previousMessage = new BatchMessageId(nextMessageInQueue.LedgerId, nextMessageInQueue.EntryId, nextMessageInQueue.PartitionIndex, ((BatchMessageIdImpl) nextMessageInQueue).BatchIndex - 1);
				}
				else
				{
					// Get on previous message in previous entry
					previousMessage = new BatchMessageId(nextMessageInQueue.LedgerId, nextMessageInQueue.EntryId - 1, nextMessageInQueue.PartitionIndex, -1);
				}

				return previousMessage;
			}
			else if(!_lastDequeuedMessageId.Equals(IMessageId.Earliest))
			{
				// If the queue was empty we need to restart from the message just after the last one that has been dequeued
				// in the past
				return new BatchMessageId((MessageId) _lastDequeuedMessageId);
			}
			else
			{
				// No message was received or dequeued by this consumer. Next message would still be the startMessageId
				return _startMessageId;
			}
		}

		/// <summary>
		/// send the flow command to have the broker start pushing messages
		/// </summary>
		private void SendFlowPermitsToBroker(ClientCnx cnx, int numMessages)
		{
			if(cnx != null)
			{
				if(_log.IsDebugEnabled)
				{
					_log.Debug("[{}] [{}] Adding {} additional permits", Topic, SubscriptionConflict, numMessages);
				}

				cnx.Ctx().writeAndFlush(Commands.NewFlow(ConsumerId, numMessages), cnx.Ctx().voidPromise());
			}
		}

		internal virtual void ConnectionFailed(PulsarClientException exception)
		{
			bool nonRetriableError = !PulsarClientException.IsRetriableError(exception);
			bool timeout = DateTimeHelper.CurrentUnixTimeMillis() > _subscribeTimeout;
			if((nonRetriableError || timeout) && SubscribeFutureConflict.completeExceptionally(exception))
			{
				State = State.Failed;
				if(nonRetriableError)
				{
					_log.info("[{}] Consumer creation failed for consumer {} with unretriableError {}", Topic, ConsumerId, exception);
				}
				else
				{
					_log.info("[{}] Consumer creation failed for consumer {} after timeout", Topic, ConsumerId);
				}
				CloseConsumerTasks();
				DeregisterFromClientCnx();
				ClientConflict.CleanupConsumer(this);
			}
		}

		internal override void CloseAsync()
		{
			if(State == State.Closing || State == State.Closed)
			{
				CloseConsumerTasks();
				return CompletableFuture.completedFuture(null);
			}

			if(!Connected)
			{
				_log.info("[{}] [{}] Closed Consumer (not connected)", Topic, SubscriptionConflict);
				State = State.Closed;
				CloseConsumerTasks();
				DeregisterFromClientCnx();
				ClientConflict.CleanupConsumer(this);
				return CompletableFuture.completedFuture(null);
			}

			StatsConflict.StatTimeout.ifPresent(Timeout.cancel);

			State = State.Closing;

			CloseConsumerTasks();

			long requestId = ClientConflict.NewRequestId();

			CompletableFuture<Void> closeFuture = new CompletableFuture<Void>();
			ClientCnx cnx = Cnx();
			if(null == cnx)
			{
				CleanupAtClose(closeFuture, null);
			}
			else
			{
				ByteBuf cmd = Commands.NewCloseConsumer(ConsumerId, requestId);
				cnx.SendRequestWithId(cmd, requestId).handle((v, exception) =>
				{
				bool ignoreException = !cnx.Ctx().channel().Active;
				if(ignoreException && exception != null)
				{
					_log.debug("Exception ignored in closing consumer", exception);
				}
				CleanupAtClose(closeFuture, ignoreException ? null : exception);
				return null;
				});
			}

			return closeFuture;
		}

		private void CleanupAtClose(CompletableFuture<Void> closeFuture, Exception exception)
		{
			_log.info("[{}] [{}] Closed consumer", Topic, SubscriptionConflict);
			State = State.Closed;
			CloseConsumerTasks();
			if(exception != null)
			{
				closeFuture.completeExceptionally(exception);
			}
			else
			{
				closeFuture.complete(null);
			}
			DeregisterFromClientCnx();
			ClientConflict.CleanupConsumer(this);
			// fail all pending-receive futures to notify application
			FailPendingReceive();
		}

		private void CloseConsumerTasks()
		{
			_unAckedMessageTracker.GracefulStop(TimeSpan.FromSeconds(3));
			if(_possibleSendToDeadLetterTopicMessages != null)
			{
				_possibleSendToDeadLetterTopicMessages.Clear();
			}

			if(!_ackRequests.Empty)
			{
				_ackRequests.ForEach((key, value) =>
				{
				value.callback.completeExceptionally(new PulsarClientException.MessageAcknowledgeException("Consumer has closed!"));
				value.recycle();
				});

				_ackRequests.Clear();
			}

			_acknowledgmentsGroupingTracker.GracefulStop(TimeSpan.FromSeconds(3));
			if (BatchReceiveTimeout != null)
			{
				BatchReceiveTimeout.Cancel();
			}
			StatsConflict.StatTimeout.ifPresent(Timeout.cancel);
		}

		private void FailPendingReceive()
		{
			@lock.readLock().@lock();
			try
			{
				if(ListenerExecutor != null && !ListenerExecutor.Shutdown)
				{
					FailPendingReceives(this.PendingReceives);
					FailPendingBatchReceives(this.PendingBatchReceives);
				}
			}
			finally
			{
				@lock.readLock().unlock();
			}
		}

		internal virtual void ActiveConsumerChanged(bool isActive)
		{
			if(ConsumerEventListener == null)
			{
				return;
			}

			ListenerExecutor.execute(() =>
			{
			if(isActive)
			{
				ConsumerEventListener.BecameActive(this, _partitionIndex);
			}
			else
			{
				ConsumerEventListener.BecameInactive(this, _partitionIndex);
			}
			});
		}

		internal virtual void MessageReceived(MessageIdData messageId, int redeliveryCount, IList<long> ackSet, byte[] headersAndPayload, ClientCnx cnx)
		{
			if(_log.IsDebugEnabled)
			{
				_log.Debug("[{}][{}] Received message: {}/{}", Topic, SubscriptionConflict, messageId.LedgerId, messageId.EntryId);
			}

			if(!VerifyChecksum(headersAndPayload, messageId))
			{
				// discard message with checksum error
				DiscardCorruptedMessage(messageId, cnx, CommandAck.ValidationError.ChecksumMismatch);
				return;
			}

			MessageMetadata msgMetadata;
			try
			{
				msgMetadata = Commands.ParseMessageMetadata(headersAndPayload);
			}
			catch(Exception)
			{
				DiscardCorruptedMessage(messageId, cnx, CommandAck.ValidationError.ChecksumMismatch);
				return;
			}

			int numMessages = msgMetadata.NumMessagesInBatch;
		
			bool isChunkedMessage = msgMetadata.NumChunksFromMsg > 1 && Conf.SubscriptionType != SubscriptionType.Shared;

			MessageId msgId = new MessageId((long)messageId.ledgerId, (long)messageId.entryId, PartitionIndex);
			if(_acknowledgmentsGroupingTracker.IsDuplicate(msgId))
			{
				if(_log.DebugEnabled)
				{
					_log.debug("[{}] [{}] Ignoring message as it was already being acked earlier by same consumer {}/{}", Topic, SubscriptionConflict, ConsumerNameConflict, msgId);
				}

				IncreaseAvailablePermits(cnx, numMessages);
				return;
			}

			byte[] decryptedPayload = DecryptPayloadIfNeeded(messageId, msgMetadata, headersAndPayload, cnx);

			bool isMessageUndecryptable = IsMessageUndecryptable(msgMetadata);

			if(decryptedPayload == null)
			{
				// Message was discarded or CryptoKeyReader isn't implemented
				return;
			}

			// uncompress decryptedPayload and release decryptedPayload-ByteBuf
			var uncompressedPayload = (isMessageUndecryptable || isChunkedMessage) ? decryptedPayload.retain() : UncompressPayloadIfNeeded(messageId, msgMetadata, decryptedPayload, cnx, true);
			
			if(uncompressedPayload == null)
			{
				// Message was discarded on decompression error
				return;
			}

			// if message is not decryptable then it can't be parsed as a batch-message. so, add EncyrptionCtx to message
			// and return undecrypted payload
			if(isMessageUndecryptable || (numMessages == 1 && !msgMetadata.HasNumMessagesInBatch()))
			{

				// right now, chunked messages are only supported by non-shared subscription
				if(isChunkedMessage)
				{
					uncompressedPayload = ProcessMessageChunk(uncompressedPayload, msgMetadata, msgId, messageId, cnx);
					if(uncompressedPayload == null)
					{
						msgMetadata.Recycle();
						return;
					}
				}

				if(IsSameEntry(messageId) && IsPriorEntryIndex(messageId.EntryId))
				{
					// We need to discard entries that were prior to startMessageId
					if(_log.DebugEnabled)
					{
						_log.debug("[{}] [{}] Ignoring message from before the startMessageId: {}", SubscriptionConflict, ConsumerNameConflict, _startMessageId);
					}

					uncompressedPayload.release();
					msgMetadata.Recycle();
					return;
				}

				MessageImpl<T> message = new MessageImpl<T>(_topicName.ToString(), msgId, msgMetadata, uncompressedPayload, CreateEncryptionContext(msgMetadata), cnx, Schema, redeliveryCount);
				uncompressedPayload.release();
				msgMetadata.Recycle();

				@lock.readLock().@lock();
				try
				{
					// Enqueue the message so that it can be retrieved when application calls receive()
					// if the conf.getReceiverQueueSize() is 0 then discard message if no one is waiting for it.
					// if asyncReceive is waiting then notify callback without adding to incomingMessages queue
					if(_deadLetterPolicy != null && _possibleSendToDeadLetterTopicMessages != null && redeliveryCount >= _deadLetterPolicy.MaxRedeliverCount)
					{
						_possibleSendToDeadLetterTopicMessages[(MessageIdImpl)message.getMessageId()] = Collections.singletonList(message);
					}
					if(PeekPendingReceive() != null)
					{
						NotifyPendingReceivedCallback(message, null);
					}
					else if(EnqueueMessageAndCheckBatchReceive(message))
					{
						if(HasPendingBatchReceive())
						{
							NotifyPendingBatchReceivedCallBack();
						}
					}
				}
				finally
				{
					@lock.readLock().unlock();
				}
			}
			else
			{
				// handle batch message enqueuing; uncompressed payload has all messages in batch
				ReceiveIndividualMessagesFromBatch(msgMetadata, redeliveryCount, ackSet, uncompressedPayload, messageId, cnx);

				uncompressedPayload.release();
				msgMetadata.Recycle();
			}

			if(Listener != null)
			{
				TriggerListener(numMessages);
			}
		}

		private bool IsTxnMessage(MessageMetadata messageMetadata)
		{
			return messageMetadata.TxnidMostBits > 0 && messageMetadata.TxnidLeastBits > 0;
		}

		private byte[] ProcessMessageChunk(byte[] compressedPayload, MessageMetadata msgMetadata, MessageId msgId, MessageIdData messageId, ClientCnx cnx)
		{

			// Lazy task scheduling to expire incomplete chunk message
			if(!_expireChunkMessageTaskScheduled && ExpireTimeOfIncompleteChunkedMessageMillis > 0)
			{
				((ScheduledExecutorService) ListenerExecutor).scheduleAtFixedRate(() =>
				{
				RemoveExpireIncompleteChunkedMessages();
				}, ExpireTimeOfIncompleteChunkedMessageMillis, ExpireTimeOfIncompleteChunkedMessageMillis, TimeUnit.MILLISECONDS);
				_expireChunkMessageTaskScheduled = true;
			}

			if(msgMetadata.ChunkId == 0)
			{
				var chunkedMsgBuffer = Unpooled.directBuffer(msgMetadata.TotalChunkMsgSize, msgMetadata.TotalChunkMsgSize);
				int totalChunks = msgMetadata.NumChunksFromMsg;
				ChunkedMessagesMap.ComputeIfAbsent(msgMetadata.Uuid, (key) => ChunkedMessageCtx.Get(totalChunks, chunkedMsgBuffer));
				_pendingChunckedMessageCount++;
				if(_maxPendingChuckedMessage > 0 && _pendingChunckedMessageCount > _maxPendingChuckedMessage)
				{
					RemoveOldestPendingChunkedMessage();
				}
				_pendingChunckedMessageUuidQueue.add(msgMetadata.Uuid);
			}

			ChunkedMessageCtx chunkedMsgCtx = ChunkedMessagesMap[msgMetadata.Uuid];
			// discard message if chunk is out-of-order
			if(chunkedMsgCtx == null || chunkedMsgCtx.ChunkedMsgBuffer == null || msgMetadata.ChunkId != (chunkedMsgCtx.LastChunkedMessageId + 1) || msgMetadata.ChunkId >= msgMetadata.TotalChunkMsgSize)
			{
				// means we lost the first chunk: should never happen
				_log.info("Received unexpected chunk messageId {}, last-chunk-id{}, chunkId = {}, total-chunks {}", msgId, (chunkedMsgCtx != null ? chunkedMsgCtx.LastChunkedMessageId : null), msgMetadata.ChunkId, msgMetadata.TotalChunkMsgSize);
				if(chunkedMsgCtx != null)
				{
					if(chunkedMsgCtx.ChunkedMsgBuffer != null)
					{
						ReferenceCountUtil.safeRelease(chunkedMsgCtx.ChunkedMsgBuffer);
					}
					chunkedMsgCtx.Recycle();
				}
				ChunkedMessagesMap.Remove(msgMetadata.Uuid);
				compressedPayload.release();
				IncreaseAvailablePermits(cnx);
				if(ExpireTimeOfIncompleteChunkedMessageMillis > 0 && DateTimeHelper.CurrentUnixTimeMillis() > (msgMetadata.PublishTime + ExpireTimeOfIncompleteChunkedMessageMillis))
				{
					doAcknowledge(msgId, CommandAck.AckType.Individual, Collections.emptyMap(), null);
				}
				else
				{
					TrackMessage(msgId);
				}
				return null;
			}

			chunkedMsgCtx.ChunkedMessageIds[msgMetadata.ChunkId] = msgId;
			// append the chunked payload and update lastChunkedMessage-id
			chunkedMsgCtx.ChunkedMsgBuffer.writeBytes(compressedPayload);
			chunkedMsgCtx.LastChunkedMessageId = msgMetadata.ChunkId;

			// if final chunk is not received yet then release payload and return
			if(msgMetadata.ChunkId != (msgMetadata.NumChunksFromMsg - 1))
			{
				compressedPayload.release();
				IncreaseAvailablePermits(cnx);
				return null;
			}

			// last chunk received: so, stitch chunked-messages and clear up chunkedMsgBuffer
			if(_log.DebugEnabled)
			{
				_log.debug("Chunked message completed chunkId {}, total-chunks {}, msgId {} sequenceId {}", msgMetadata.ChunkId, msgMetadata.NumChunksFromMsg, msgId, msgMetadata.SequenceId);
			}
			// remove buffer from the map, add chucked messageId to unack-message tracker, and reduce pending-chunked-message count
			ChunkedMessagesMap.Remove(msgMetadata.Uuid);
			UnAckedChunckedMessageIdSequenceMap.Put(msgId, chunkedMsgCtx.ChunkedMessageIds);
			_pendingChunckedMessageCount--;
			compressedPayload.release();
			compressedPayload = chunkedMsgCtx.ChunkedMsgBuffer;
			chunkedMsgCtx.Recycle();
			ByteBuf uncompressedPayload = UncompressPayloadIfNeeded(messageId, msgMetadata, compressedPayload, cnx, false);
			compressedPayload.release();
			return uncompressedPayload;
		}

		protected internal virtual void TriggerListener(int numMessages)
		{
			// Trigger the notification on the message listener in a separate thread to avoid blocking the networking
			// thread while the message processing happens
			ListenerExecutor.execute(() =>
			{
			for(int i = 0; i < numMessages; i++)
			{
				try
				{
					Message<T> msg = InternalReceive(0, TimeUnit.MILLISECONDS);
					if(msg == null)
					{
						if(_log.DebugEnabled)
						{
							_log.debug("[{}] [{}] Message has been cleared from the queue", Topic, SubscriptionConflict);
						}
						break;
					}
					try
					{
						if(_log.DebugEnabled)
						{
							_log.debug("[{}][{}] Calling message listener for message {}", Topic, SubscriptionConflict, msg.MessageId);
						}
						Listener.Received(ConsumerActor.this, msg);
					}
					catch(Exception t)
					{
						_log.error("[{}][{}] Message listener error in processing message: {}", Topic, SubscriptionConflict, msg.MessageId, t);
					}
				}
				catch(PulsarClientException e)
				{
					_log.warn("[{}] [{}] Failed to dequeue the message for listener", Topic, SubscriptionConflict, e);
					return;
				}
			}
			});
		}

		/// <summary>
		/// Notify waiting asyncReceive request with the received message
		/// </summary>
		/// <param name="message"> </param>
		internal virtual void NotifyPendingReceivedCallback(in Message<T> message, Exception exception)
		{
			if(PendingReceives.Empty)
			{
				return;
			}

			// fetch receivedCallback from queue
			CompletableFuture<Message<T>> receivedFuture = PollPendingReceive();
			if(receivedFuture == null)
			{
				return;
			}

			if(exception != null)
			{
				ListenerExecutor.execute(() => receivedFuture.completeExceptionally(exception));
				return;
			}

			if(message == null)
			{
				System.InvalidOperationException e = new System.InvalidOperationException("received message can't be null");
				ListenerExecutor.execute(() => receivedFuture.completeExceptionally(e));
				return;
			}

			if(Conf.ReceiverQueueSize == 0)
			{
				// call interceptor and complete received callback
				TrackMessage(message);
				InterceptAndComplete(message, receivedFuture);
				return;
			}

			// increase permits for available message-queue
			MessageProcessed(message);
			// call interceptor and complete received callback
			InterceptAndComplete(message, receivedFuture);
		}

		private void InterceptAndComplete(IMessage<T> message, in CompletableFuture<Message<T>> receivedFuture)
		{
			// call proper interceptor

			var interceptMessage = BeforeConsume(message);
			// return message to receivedCallback
			CompletePendingReceive(receivedFuture, interceptMessage);
		}

		internal virtual void ReceiveIndividualMessagesFromBatch(MessageMetadata msgMetadata, int redeliveryCount, IList<long> ackSet, ByteBuf uncompressedPayload, MessageIdData messageId, ClientCnx cnx)
		{
			int batchSize = msgMetadata.NumMessagesInBatch;

			// create ack tracker for entry aka batch
			MessageIdImpl batchMessage = new MessageIdImpl(messageId.LedgerId, messageId.EntryId, PartitionIndex);
			IList<MessageImpl<T>> possibleToDeadLetter = null;
			if(_deadLetterPolicy != null && redeliveryCount >= _deadLetterPolicy.MaxRedeliverCount)
			{
				possibleToDeadLetter = new List<MessageImpl<T>>();
			}

			BatchMessageAcker acker = BatchMessageAcker.NewAcker(batchSize);
			BitSetRecyclable ackBitSet = null;
			if(ackSet != null && ackSet.Count > 0)
			{
				ackBitSet = BitSetRecyclable.ValueOf(SafeCollectionUtils.LongListToArray(ackSet));
			}

			int skippedMessages = 0;
			try
			{
				for(int i = 0; i < batchSize; ++i)
				{
					if(_log.DebugEnabled)
					{
						_log.debug("[{}] [{}] processing message num - {} in batch", SubscriptionConflict, ConsumerNameConflict, i);
					}
					SingleMessageMetadata.Builder singleMessageMetadataBuilder = SingleMessageMetadata.NewBuilder();
					ByteBuf singleMessagePayload = Commands.DeSerializeSingleMessageInBatch(uncompressedPayload, singleMessageMetadataBuilder, i, batchSize);

					if(IsSameEntry(messageId) && IsPriorBatchIndex(i))
					{
						// If we are receiving a batch message, we need to discard messages that were prior
						// to the startMessageId
						if(_log.DebugEnabled)
						{
							_log.debug("[{}] [{}] Ignoring message from before the startMessageId: {}", SubscriptionConflict, ConsumerNameConflict, _startMessageId);
						}
						singleMessagePayload.release();
						singleMessageMetadataBuilder.Recycle();

						++skippedMessages;
						continue;
					}

					if(singleMessageMetadataBuilder.CompactedOut)
					{
						// message has been compacted out, so don't send to the user
						singleMessagePayload.release();
						singleMessageMetadataBuilder.Recycle();

						++skippedMessages;
						continue;
					}

					if(ackBitSet != null && !ackBitSet.Get(i))
					{
						singleMessagePayload.release();
						singleMessageMetadataBuilder.Recycle();
						++skippedMessages;
						continue;
					}

					BatchMessageIdImpl batchMessageIdImpl = new BatchMessageIdImpl(messageId.LedgerId, messageId.EntryId, PartitionIndex, i, batchSize, acker);

					MessageImpl<T> message = new MessageImpl<T>(_topicName.ToString(), batchMessageIdImpl, msgMetadata, singleMessageMetadataBuilder.Build(), singleMessagePayload, CreateEncryptionContext(msgMetadata), cnx, Schema, redeliveryCount);
					if(possibleToDeadLetter != null)
					{
						possibleToDeadLetter.Add(message);
					}
					@lock.readLock().@lock();
					try
					{
						if(PeekPendingReceive() != null)
						{
							NotifyPendingReceivedCallback(message, null);
						}
						else if(EnqueueMessageAndCheckBatchReceive(message))
						{
							if(HasPendingBatchReceive())
							{
								NotifyPendingBatchReceivedCallBack();
							}
						}
					}
					finally
					{
						@lock.readLock().unlock();
					}
					singleMessagePayload.release();
					singleMessageMetadataBuilder.Recycle();
				}
				if(ackBitSet != null)
				{
					ackBitSet.Recycle();
				}
			}
			catch(IOException)
			{
				_log.warn("[{}] [{}] unable to obtain message in batch", SubscriptionConflict, ConsumerNameConflict);
				DiscardCorruptedMessage(messageId, cnx, CommandAck.ValidationError.BatchDeSerializeError);
			}

			if(possibleToDeadLetter != null && _possibleSendToDeadLetterTopicMessages != null)
			{
				_possibleSendToDeadLetterTopicMessages[batchMessage] = possibleToDeadLetter;
			}

			if(_log.DebugEnabled)
			{
				_log.debug("[{}] [{}] enqueued messages in batch. queue size - {}, available queue size - {}", SubscriptionConflict, ConsumerNameConflict, IncomingMessages.size(), IncomingMessages.remainingCapacity());
			}

			if(skippedMessages > 0)
			{
				IncreaseAvailablePermits(cnx, skippedMessages);
			}
		}

		private bool IsPriorEntryIndex(long idx)
		{
			return _resetIncludeHead ? idx < _startMessageId.EntryId : idx <= _startMessageId.EntryId;
		}

		private bool IsPriorBatchIndex(long idx)
		{
			return _resetIncludeHead ? idx < _startMessageId.BatchIndex : idx <= _startMessageId.BatchIndex;
		}

		private bool IsSameEntry(MessageIdData messageId)
		{
			return _startMessageId != null && messageId.LedgerId == _startMessageId.LedgerId && messageId.EntryId == _startMessageId.EntryId;
		}

		/// <summary>
		/// Record the event that one message has been processed by the application.
		/// 
		/// Periodically, it sends a Flow command to notify the broker that it can push more messages
		/// </summary>
		protected internal override void MessageProcessed<T1>(Message<T1> msg)
		{
			lock(this)
			{
				ClientCnx currentCnx = Cnx();
				ClientCnx msgCnx = ((MessageImpl<object>) msg).Cnx;
				LastDequeuedMessageId = msg.MessageId;
        
				if(msgCnx != currentCnx)
				{
					// The processed message did belong to the old queue that was cleared after reconnection.
					return;
				}
        
				IncreaseAvailablePermits(currentCnx);
				StatsConflict.UpdateNumMsgsReceived(msg);
        
				TrackMessage(msg);
				IncomingMessagesSizeUpdater.addAndGet(this, msg.Data == null ? 0 : -msg.Data.Length);
			}
		}

		protected internal virtual void TrackMessage<T1>(Message<T1> msg)
		{
			if(msg != null)
			{
				TrackMessage(msg.MessageId);
			}
		}

		protected internal virtual void TrackMessage(MessageId messageId)
		{
			if(Conf.AckTimeoutMillis > 0 && messageId is MessageIdImpl)
			{
				MessageIdImpl id = (MessageIdImpl)messageId;
				if(id is BatchMessageIdImpl)
				{
					// do not add each item in batch message into tracker
					id = new MessageIdImpl(id.LedgerId, id.EntryId, PartitionIndex);
				}
				if(_hasParentConsumer)
				{
					//TODO: check parent consumer here
					// we should no longer track this message, TopicsConsumer will take care from now onwards
					_unAckedMessageTracker.Remove(id);
				}
				else
				{
					_unAckedMessageTracker.Tell(new Add(id));
				}
			}
		}

		internal virtual void IncreaseAvailablePermits(IActorRef currentCnx)
		{
			IncreaseAvailablePermits(currentCnx, 1);
		}

		protected internal virtual void IncreaseAvailablePermits(IActorRef currentCnx, int delta)
		{
			_availablePermits += delta;
			int available = _availablePermits;

			while(available >= _receiverQueueRefillThreshold && !Paused)
			{
				if(_availablePermitsUpdater.compareAndSet(this, available, 0))
				{
					SendFlowPermitsToBroker(currentCnx, available);
					break;
				}
				else
				{
					available = _availablePermitsUpdater.get(this);
				}
			}
		}

		internal virtual void IncreaseAvailablePermits(int delta)
		{
			IncreaseAvailablePermits(Cnx(), delta);
		}

		internal override void Pause()
		{
			Paused = true;
		}

		internal override void Resume()
		{
			if(Paused)
			{
				Paused = false;
				IncreaseAvailablePermits(Cnx(), 0);
			}
		}

		internal override long LastDisconnectedTimestamp
		{
			get
			{
				return _connectionHandler.LastConnectionClosedTimestamp;
			}
		}

		private ByteBuf DecryptPayloadIfNeeded(MessageIdData messageId, MessageMetadata msgMetadata, ByteBuf payload, ClientCnx currentCnx)
		{

			if(msgMetadata.EncryptionKeysCount == 0)
			{
				return payload.retain();
			}

			// If KeyReader is not configured throw exception based on config param
			if(Conf.CryptoKeyReader == null)
			{
				switch(Conf.CryptoFailureAction)
				{
					case CONSUME:
						_log.warn("[{}][{}][{}] CryptoKeyReader interface is not implemented. Consuming encrypted message.", Topic, SubscriptionConflict, ConsumerNameConflict);
						return payload.retain();
					case DISCARD:
						_log.warn("[{}][{}][{}] Skipping decryption since CryptoKeyReader interface is not implemented and config is set to discard", Topic, SubscriptionConflict, ConsumerNameConflict);
						DiscardMessage(messageId, currentCnx, CommandAck.ValidationError.DecryptionError);
						return null;
					case FAIL:
						MessageId m = new MessageIdImpl(messageId.LedgerId, messageId.EntryId, _partitionIndex);
						_log.error("[{}][{}][{}][{}] Message delivery failed since CryptoKeyReader interface is not implemented to consume encrypted message", Topic, SubscriptionConflict, ConsumerNameConflict, m);
						_unAckedMessageTracker.Add(m);
						return null;
				}
			}

			ByteBuf decryptedData = this._msgCrypto.decrypt(() => msgMetadata, payload, Conf.CryptoKeyReader);
			if(decryptedData != null)
			{
				return decryptedData;
			}

			switch(Conf.CryptoFailureAction)
			{
				case CONSUME:
					// Note, batch message will fail to consume even if config is set to consume
					_log.warn("[{}][{}][{}][{}] Decryption failed. Consuming encrypted message since config is set to consume.", Topic, SubscriptionConflict, ConsumerNameConflict, messageId);
					return payload.retain();
				case DISCARD:
					_log.warn("[{}][{}][{}][{}] Discarding message since decryption failed and config is set to discard", Topic, SubscriptionConflict, ConsumerNameConflict, messageId);
					DiscardMessage(messageId, currentCnx, CommandAck.ValidationError.DecryptionError);
					return null;
				case FAIL:
					MessageId m = new MessageIdImpl(messageId.LedgerId, messageId.EntryId, _partitionIndex);
					_log.error("[{}][{}][{}][{}] Message delivery failed since unable to decrypt incoming message", Topic, SubscriptionConflict, ConsumerNameConflict, m);
					_unAckedMessageTracker.Add(m);
					return null;
			}
			return null;
		}

		private ByteBuf UncompressPayloadIfNeeded(MessageIdData messageId, MessageMetadata msgMetadata, ByteBuf payload, ClientCnx currentCnx, bool checkMaxMessageSize)
		{
			CompressionType compressionType = msgMetadata.Compression;
			CompressionCodec codec = CompressionCodecProvider.GetCompressionCodec(compressionType);
			int uncompressedSize = msgMetadata.UncompressedSize;
			int payloadSize = payload.readableBytes();
			if(checkMaxMessageSize && payloadSize > ClientCnx.MaxMessageSize)
			{
				// payload size is itself corrupted since it cannot be bigger than the MaxMessageSize
				_log.error("[{}][{}] Got corrupted payload message size {} at {}", Topic, SubscriptionConflict, payloadSize, messageId);
				DiscardCorruptedMessage(messageId, currentCnx, CommandAck.ValidationError.UncompressedSizeCorruption);
				return null;
			}
			try
			{
				ByteBuf uncompressedPayload = codec.Decode(payload, uncompressedSize);
				return uncompressedPayload;
			}
			catch(IOException e)
			{
				_log.error("[{}][{}] Failed to decompress message with {} at {}: {}", Topic, SubscriptionConflict, compressionType, messageId, e.Message, e);
				DiscardCorruptedMessage(messageId, currentCnx, CommandAck.ValidationError.DecompressionError);
				return null;
			}
		}

		private bool VerifyChecksum(byte[] headersAndPayload, MessageIdData messageId)
		{

			if(hasChecksum(headersAndPayload))
			{
				int checksum = readChecksum(headersAndPayload);
				int computedChecksum = computeChecksum(headersAndPayload);
				if(checksum != computedChecksum)
				{
					_log.error("[{}][{}] Checksum mismatch for message at {}:{}. Received checksum: 0x{}, Computed checksum: 0x{}", Topic, SubscriptionConflict, messageId.LedgerId, messageId.EntryId, checksum.ToString("x"), computedChecksum.ToString("x"));
					return false;
				}
			}

			return true;
		}

		private void DiscardCorruptedMessage(MessageIdData messageId, ClientCnx currentCnx, CommandAck.ValidationError validationError)
		{
			_log.error("[{}][{}] Discarding corrupted message at {}:{}", Topic, SubscriptionConflict, messageId.LedgerId, messageId.EntryId);
			DiscardMessage(messageId, currentCnx, validationError);
		}

		private void DiscardMessage(MessageIdData messageId, ClientCnx currentCnx, CommandAck.ValidationError validationError)
		{
			ByteBuf cmd = Commands.NewAck(ConsumerId, messageId.LedgerId, messageId.EntryId, null, CommandAck.AckType.Individual, validationError, Collections.emptyMap());
			currentCnx.Ctx().writeAndFlush(cmd, currentCnx.Ctx().voidPromise());
			IncreaseAvailablePermits(currentCnx);
			StatsConflict.IncrementNumReceiveFailed();
		}

		internal override string HandlerName
		{
			get
			{
				return SubscriptionConflict;
			}
		}

		internal override bool Connected
		{
			get
			{
				return ClientCnx != null && (State == State.Ready);
			}
		}

		internal virtual int PartitionIndex
		{
			get
			{
				return _partitionIndex;
			}
		}

		internal override int AvailablePermits
		{
			get
			{
				return _availablePermitsUpdater.get(this);
			}
		}

		internal override int NumMessagesInQueue()
		{
			return IncomingMessages.size();
		}

		internal override void RedeliverUnacknowledgedMessages()
		{
			ClientCnx cnx = Cnx();
			if(Connected && cnx.RemoteEndpointProtocolVersion >= ProtocolVersion.V2.Number)
			{
				int currentSize = 0;
				lock(this)
				{
					currentSize = IncomingMessages.size();
					IncomingMessages.clear();
					IncomingMessagesSizeUpdater.set(this, 0);
					_unAckedMessageTracker.Clear();
				}
				cnx.Ctx().writeAndFlush(Commands.NewRedeliverUnacknowledgedMessages(ConsumerId), cnx.Ctx().voidPromise());
				if(currentSize > 0)
				{
					IncreaseAvailablePermits(cnx, currentSize);
				}
				if(_log.DebugEnabled)
				{
					_log.debug("[{}] [{}] [{}] Redeliver unacked messages and send {} permits", SubscriptionConflict, Topic, ConsumerNameConflict, currentSize);
				}
				return;
			}
			if(cnx == null || (State == State.Connecting))
			{
				_log.warn("[{}] Client Connection needs to be established for redelivery of unacknowledged messages", this);
			}
			else
			{
				_log.warn("[{}] Reconnecting the client to redeliver the messages.", this);
				cnx.Ctx().close();
			}
		}

		internal virtual int ClearIncomingMessagesAndGetMessageNumber()
		{
			int messagesNumber = IncomingMessages.size();
			IncomingMessages.clear();
			IncomingMessagesSizeUpdater.set(this, 0);
			_unAckedMessageTracker.Clear();
			return messagesNumber;
		}

		internal override void RedeliverUnacknowledgedMessages(ISet<MessageId> messageIds)
		{
			if(messageIds.Count == 0)
			{
				return;
			}

			checkArgument(messageIds.First().get() is MessageIdImpl);

			if(Conf.SubscriptionType != SubscriptionType.Shared && Conf.SubscriptionType != SubscriptionType.KeyShared)
			{
				// We cannot redeliver single messages if subscription type is not Shared
				RedeliverUnacknowledgedMessages();
				return;
			}
			ClientCnx cnx = Cnx();
			if(Connected && cnx.RemoteEndpointProtocolVersion >= ProtocolVersion.V2.Number)
			{
				int messagesFromQueue = RemoveExpiredMessagesFromQueue(messageIds);

				IEnumerable<IList<MessageIdImpl>> batches = Iterables.partition(messageIds.Select(messageId => (MessageIdImpl)messageId).collect(Collectors.toSet()), MaxRedeliverUnacknowledged);
				MessageIdData.Builder builder = MessageIdData.NewBuilder();
				batches.forEach(ids =>
				{
				IList<MessageIdData> messageIdDatas = ids.Where(messageId => !ProcessPossibleToDLQ(messageId)).Select(messageId =>
				{
					builder.Partition = messageId.PartitionIndex;
					builder.LedgerId = messageId.LedgerId;
					builder.EntryId = messageId.EntryId;
					return builder.Build();
				}).ToList();
				if(messageIdDatas.Count > 0)
				{
					ByteBuf cmd = Commands.NewRedeliverUnacknowledgedMessages(ConsumerId, messageIdDatas);
					cnx.Ctx().writeAndFlush(cmd, cnx.Ctx().voidPromise());
					messageIdDatas.ForEach(MessageIdData.recycle);
				}
				});
				if(messagesFromQueue > 0)
				{
					IncreaseAvailablePermits(cnx, messagesFromQueue);
				}
				builder.Recycle();
				if(_log.DebugEnabled)
				{
					_log.debug("[{}] [{}] [{}] Redeliver unacked messages and increase {} permits", SubscriptionConflict, Topic, ConsumerNameConflict, messagesFromQueue);
				}
				return;
			}
			if(cnx == null || (State == State.Connecting))
			{
				_log.warn("[{}] Client Connection needs to be established for redelivery of unacknowledged messages", this);
			}
			else
			{
				_log.warn("[{}] Reconnecting the client to redeliver the messages.", this);
				cnx.Ctx().close();
			}
		}

		protected internal override void CompleteOpBatchReceive(OpBatchReceive<T> op)
		{
			NotifyPendingBatchReceivedCallBack(op);
		}

		private bool ProcessPossibleToDLQ(IMessageId messageId)
		{
			IList<Message<T>> deadLetterMessages = null;
			if(_possibleSendToDeadLetterTopicMessages != null)
			{
				if(messageId is BatchMessageId bmid)
				{
					deadLetterMessages = _possibleSendToDeadLetterTopicMessages.GetValueOrNull(new MessageId(bmid.LedgerId, bmid.EntryId, PartitionIndex));
				}
				else
				{
					deadLetterMessages = _possibleSendToDeadLetterTopicMessages.GetValueOrNull(messageId);
				}
			}
			if(deadLetterMessages != null)
			{
				if(_deadLetterProducer == null)
				{
					try
					{
						_deadLetterProducer = ClientConflict.NewProducer(Schema).Topic(this._deadLetterPolicy.DeadLetterTopic).BlockIfQueueFull(false).create();
					}
					catch(Exception e)
					{
						_log.Error($"Create dead letter producer exception with topic: {_deadLetterPolicy.DeadLetterTopic} => {e}");
					}
				}
				if(_deadLetterProducer != null)
				{
					try
					{
						foreach(MessageImpl<T> message in deadLetterMessages)
						{
							_deadLetterProducer.NewMessage().Value(message.Value).Properties(message.Properties).send();
						}
						Acknowledge(messageId);
						return true;
					}
					catch(Exception e)
					{
						_log.error("Send to dead letter topic exception with topic: {}, messageId: {}", _deadLetterProducer.Topic, messageId, e);
					}
				}
			}
			return false;
		}

		internal override void Seek(IMessageId messageId)
		{
			try
			{
				if (State.ConnectionState == HandlerState.State.Closing || State.ConnectionState == HandlerState.State.Closed)
				{
					var error = new PulsarClientException.AlreadyClosedException($"The consumer {ConsumerName} was already closed when seeking the subscription {Subscription} of the topic {_topicName} to the message {messageId}");
					_seekRequestResponseQueue.Add(error);
				}

				if (!Connected)
				{
					var error = Task.FromException(new PulsarClientException($"The client is not connected to the broker when seeking the subscription {Subscription} of the topic {_topicName} to the message {messageId}"));
					_seekRequestResponseQueue.Add(error);
				}

				long requestId = _client.AskFor<NewRequestIdResponse>(NewRequestId.Instance).Id;
				byte[] seek = null;
				if (messageId is BatchMessageId msgId)
				{
					// Initialize ack set
					var ackSet = BitSet.Create();
					ackSet.Set(0, msgId.BatchSize);
					ackSet.Clear(0, Math.Max(msgId.BatchIndex, 0));
					long[] ackSetArr = ackSet.ToLongArray();

					seek = Commands.NewSeek(ConsumerId, requestId, msgId.LedgerId, msgId.EntryId, ackSetArr);
				}
				else
				{
					var msgid = (MessageId)messageId;
					seek = Commands.NewSeek(ConsumerId, requestId, msgid.LedgerId, msgid.EntryId, new long[0]);
				}

				var cnx = Cnx();

				_log.Info($"[{Topic}][{Subscription}] Seek subscription to message id {messageId}");
				var sent = cnx.AskFor<bool>(new SendRequestWithId(seek, requestId));
				if (sent)
				{

					_log.Info($"[{Topic}][{Subscription}] Successfully reset subscription to message id {messageId}");
					_acknowledgmentsGroupingTracker.Tell(FlushAndClean.Instance);
					_seekMessageId = new BatchMessageId((MessageId)messageId);
					_duringSeek = true;
					_lastDequeuedMessageId = IMessageId.Earliest;
					IncomingMessages = new BlockingQueue<IMessage<T>>();
					IncomingMessagesSize = 0;
					_seekRequestResponseQueue.Add(true);
				}
				else
				{
					_log.Error($"[{Topic}][{Subscription}] Failed to reset subscription");
					_seekRequestResponseQueue.Add(new Exception($"Failed to seek the subscription {Subscription} of the topic {_topicName} to the message {messageId}"));
					
				}
			}
			catch(Exception e)
			{
				_seekRequestResponseQueue.Add(PulsarClientException.Unwrap(e));
			}
		}

		internal override void Seek(long timestamp)
		{
			try
			{
				if (State.ConnectionState == HandlerState.State.Closing || State.ConnectionState == HandlerState.State.Closed)
				{
					_log.Error($"The consumer {ConsumerName} was already closed when seeking the subscription {Subscription} of the topic {_topicName} to the timestamp {timestamp:D}");
					_voidRequestResponseQueue.Add(new Failure { Exception = new Exception($"The consumer {ConsumerName} was already closed when seeking the subscription {Subscription} of the topic {_topicName} to the timestamp {timestamp:D}") });
					return;
				}

				if (!Connected)
				{
					_log.Error($"The client is not connected to the broker when seeking the subscription {Subscription} of the topic {_topicName} to the timestamp {timestamp:D}");
					_voidRequestResponseQueue.Add(new Failure { Exception = new Exception($"The client is not connected to the broker when seeking the subscription {Subscription} of the topic {_topicName} to the timestamp {timestamp:D}") });

					return;
				}

				long requestId = _client.AskFor<NewRequestIdResponse>(NewRequestId.Instance).Id;
				var seek = Commands.NewSeek(ConsumerId, requestId, timestamp);
				var cnx = Cnx();

				_log.Info($"[{Topic}][{Subscription}] Seek subscription to publish time {timestamp}");
				var sent = cnx.AskFor<bool>(new SendRequestWithId(seek, requestId));
				if(sent)
                {
					_log.Info($"[{Topic}][{Subscription}] Successfully reset subscription to publish time {timestamp}");
					_acknowledgmentsGroupingTracker.Tell(FlushAndClean.Instance);
					_seekMessageId = new BatchMessageId((MessageId)IMessageId.Earliest);
					_duringSeek = true;
					_lastDequeuedMessageId = IMessageId.Earliest;
					IncomingMessages = new BlockingQueue<IMessage<T>>();
					IncomingMessagesSize = 0;
					_voidRequestResponseQueue.Add(true);
				}
                else
                {
					_log.Error($"[{Topic}][{Subscription}] Failed to reset subscription");
					_voidRequestResponseQueue.Add(new Failure { Exception = new Exception($"Failed to seek the subscription {Subscription} of the topic {_topicName} to the timestamp {timestamp:D}") });

				}
			}
			catch(Exception e)
			{
				_voidRequestResponseQueue.Add(new Failure { Exception = PulsarClientException.Unwrap(e) });
			}
		}

		internal virtual bool HasMessageAvailable()
		{
			try
			{///hereredtry
				if (_lastDequeuedMessageId == IMessageId.Earliest)
				{
					// if we are starting from latest, we should seek to the actual last message first.
					// allow the last one to be read when read head inclusively.
					if (_startMessageId.LedgerId == long.MaxValue && _startMessageId.EntryId == long.MaxValue && _startMessageId.PartitionIndex == -1)
					{
						var lstid = LastMessageId;
						Seek(lstid);
						return true;
					}

					if (HasMoreMessages(_lastMessageIdInBroker, _startMessageId, _resetIncludeHead))
					{
						return true;
					}

					_lastMessageIdInBroker = LastMessageId;
					if (HasMoreMessages(_lastMessageIdInBroker, _startMessageId, _resetIncludeHead))
					{
						return true;
					}
					else
					{
						return false;
					}

				}
				else
				{
					// read before, use lastDequeueMessage for comparison
					if (HasMoreMessages(_lastMessageIdInBroker, _lastDequeuedMessageId, false))
					{
						return true;
					}

					_lastMessageIdInBroker = LastMessageId;
					if (HasMoreMessages(_lastMessageIdInBroker, _lastDequeuedMessageId, false))
					{
						return true;
					}
					else
					{
						return false;
					}
				}

			}
			catch(Exception e)
			{
				throw PulsarClientException.Unwrap(e);
			}
		}

		private bool HasMoreMessages(IMessageId lastMessageIdInBroker, IMessageId messageId, bool inclusive)
		{
			if(inclusive && lastMessageIdInBroker.CompareTo(messageId) >= 0 && ((MessageId)lastMessageIdInBroker).EntryId != -1)
			{
				return true;
			}

			if(!inclusive && lastMessageIdInBroker.CompareTo(messageId) > 0 && ((MessageId)lastMessageIdInBroker).EntryId != -1)
			{
				return true;
			}

			return false;
		}

		private IMessageId LastMessageId
		{
			get
			{
				if(State.ConnectionState == HandlerState.State.Closing || State.ConnectionState == HandlerState.State.Closed)
				{
					throw new PulsarClientException.AlreadyClosedException($"The consumer {ConsumerName} was already closed when the subscription {Subscription} of the topic {_topicName} getting the last message id");
				}
    
				var opTimeoutMs = _clientConfigurationData.OperationTimeoutMs;
				Backoff backoff = new BackoffBuilder().SetInitialTime(100, TimeUnit.MILLISECONDS).SetMax(opTimeoutMs * 2, TimeUnit.MILLISECONDS).SetMandatoryStop(0, TimeUnit.MILLISECONDS).Create();
    
				var getLastMessageId = new TaskCompletionSource<IMessageId>();
    
				InternalGetLastMessageId(backoff, opTimeoutMs, getLastMessageId);
				return Task.Run(() => getLastMessageId.Task).Result;
			}
		}

		private void InternalGetLastMessageId(Backoff backoff, long remainingTime, TaskCompletionSource<IMessageId> source)
		{
			///todo: add response to queue, where there is a retry, add something in the queue so that client knows we are 
			///retrying in times delay
			var cnx = Cnx();
			if(Connected && cnx != null)
			{
				var protocolversion = cnx.AskFor<int>(RemoteEndpointProtocolVersion.Instance);
				if(!Commands.PeerSupportsGetLastMessageId(protocolversion))
				{
					source.SetException(new PulsarClientException.NotSupportedException($"The command `GetLastMessageId` is not supported for the protocol version {protocolversion:D}. The consumer is {ConsumerName}, topic {_topicName}, subscription {Subscription}"));
				}

				long requestId = cnx.AskFor<NewRequestIdResponse>(NewRequestId.Instance).Id;
				var getLastIdCmd = Commands.NewGetLastMessageId(ConsumerId, requestId);
				_log.Info($"[{Topic}][{Subscription}] Get topic last message Id");
				var payload = new Payload(getLastIdCmd, requestId, "NewGetLastMessageId");
                try
                {
					var result = cnx.AskFor<LastMessageIdResponse>(payload);

					_log.Info($"[{Topic}][{Subscription}] Successfully getLastMessageId {result.LedgerId}:{result.EntryId}");
					if (result.BatchIndex < 0)
					{
						source.SetResult(new MessageId(result.LedgerId, result.EntryId, result.Partition));
					}
					else
					{
						source.SetResult(new BatchMessageId(result.LedgerId, result.EntryId, result.Partition, result.BatchIndex));
					}
				}
				catch(Exception ex)
                {
					_log.Error($"[{Topic}][{Subscription}] Failed getLastMessageId command");
					source.SetException(PulsarClientException.Wrap(ex, $"The subscription {Subscription} of the topic {_topicName} gets the last message id was failed"));
					return;
				}
			}
			else
			{
				long nextDelay = Math.Min(backoff.Next(), remainingTime);
				if(nextDelay <= 0)
				{
					source.SetException(new PulsarClientException.TimeoutException($"The subscription {Subscription} of the topic {_topicName} could not get the last message id " + "withing configured timeout"));
					return;
					
				}
				Context.System.Scheduler.Advanced.ScheduleOnce(TimeSpan.FromMilliseconds(TimeUnit.MILLISECONDS.ToMilliseconds(nextDelay)), () =>
				{
					var log = _log;
					var remaining = remainingTime - nextDelay;
					log.Warning("[{}] [{}] Could not get connection while getLastMessageId -- Will try again in {} ms", Topic, HandlerName, nextDelay);
					
					InternalGetLastMessageId(backoff, remaining, source);
				});
			}
		}

		private IMessageId GetMessageId<T1>(IMessage<T1> msg)
		{
			var messageId = (MessageId) msg.MessageId;
			if(messageId is BatchMessageId)
			{
				// messageIds contain MessageIdImpl, not BatchMessageIdImpl
				messageId = new MessageId(messageId.LedgerId, messageId.EntryId, PartitionIndex);
			}
			return messageId;
		}


		private bool IsMessageUndecryptable(MessageMetadata msgMetadata)
		{
			return (msgMetadata.EncryptionKeys.Count > 0 && Conf.CryptoKeyReader == null && Conf.CryptoFailureAction == ConsumerCryptoFailureAction.Consume);
		}

		/// <summary>
		/// Create EncryptionContext if message payload is encrypted
		/// </summary>
		/// <param name="msgMetadata"> </param>
		/// <returns> <seealso cref="Optional"/><<seealso cref="EncryptionContext"/>> </returns>
		private Option<EncryptionContext> CreateEncryptionContext(MessageMetadata msgMetadata)
		{

			EncryptionContext encryptionCtx = null;
			if(msgMetadata.EncryptionKeys.Count > 0)
			{
				encryptionCtx = new EncryptionContext(); 
				IDictionary<string, EncryptionContext.EncryptionKey> keys = new Dictionary<string, EncryptionContext.EncryptionKey>();
				foreach (var kv in msgMetadata.EncryptionKeys)
				{
					var neC = new EncryptionContext.EncryptionKey
					{
						KeyValue = (sbyte[])(object)kv.Value,
						Metadata = new Dictionary<string, string>()
					};
					foreach (var m in kv.Metadatas)
					{
						if (!neC.Metadata.ContainsKey(m.Key))
						{
							neC.Metadata.Add(m.Key, m.Value);
						}
					}

					if (!keys.ContainsKey(kv.Key))
					{
						keys.Add(kv.Key, neC);
					}
				}
				sbyte[] encParam = new sbyte[IMessageCrypto.IV_LEN];
				msgMetadata.EncryptionParam.CopyTo(encParam, 0);
				int? batchSize = msgMetadata.NumMessagesInBatch > 0 ? msgMetadata.NumMessagesInBatch : 0;
				encryptionCtx.Keys = keys;
				encryptionCtx.Param = encParam;
				encryptionCtx.Algorithm = msgMetadata.EncryptionAlgo;
				encryptionCtx.CompressionType = (int)msgMetadata.Compression;// CompressionCodecProvider.ConvertFromWireProtocol(msgMetadata.Compression);
				encryptionCtx.UncompressedMessageSize = (int)msgMetadata.UncompressedSize;
				encryptionCtx.BatchSize = batchSize;
			}
			return new Option<EncryptionContext>(encryptionCtx);
		}

		private int RemoveExpiredMessagesFromQueue(ISet<IMessageId> messageIds)
		{
			int messagesFromQueue = 0;
			var peek = IncomingMessages.Take(_tokenSource.Token);
			if(peek != null)
			{
				var messageId = GetMessageId(peek);
				if(!messageIds.Contains(messageId))
				{
					// first message is not expired, then no message is expired in queue.
					return 0;
				}

				// try not to remove elements that are added while we remove
				while(IncomingMessages.TryTake(out var message))
				{
					IncomingMessagesSize -= message.Data.Length;
					messagesFromQueue++;
					var id = GetMessageId(message);
					if(!messageIds.Contains(id))
					{
						messageIds.Add(id);
						break;
					}
				}
			}
			return messagesFromQueue;
		}

		private void SetTerminated()
		{
			_log.Info($"[{Subscription}] [{Topic}] [{ConsumerName}] Consumer has reached the end of topic");
			_hasReachedEndOfTopic = true;
			if(Listener != null)
			{
				// Propagate notification to listener
				Listener.ReachedEndOfTopic(Self);
			}
		}

		private bool HasReachedEndOfTopic()
		{
			return _hasReachedEndOfTopic;
		}


		// wrapper for connection methods
		private IActorRef Cnx()
		{
			return _connectionHandler.AskFor<IActorRef>(GetCnx.Instance);
		}

		private void ResetBackoff()
		{
			_connectionHandler.Tell(Messages.Requests.ResetBackoff.Instance);
		}

		private void ConnectionClosed(IActorRef cnx)
		{
			_connectionHandler.Tell(new ConnectionClosed(cnx));
		}


		private IActorRef ClientCnx
		{
			get
			{
				return _connectionHandler.AskFor<IActorRef>(GetCnx.Instance);
			}
			set
			{
				if(value != null)
				{
					_connectionHandler.Tell(new SetCnx(value));
					value.Tell(new RegisterConsumer(ConsumerId, Self));
				}
				var previousClientCnx = _clientCnxUsedForConsumerRegistration;
				_clientCnxUsedForConsumerRegistration = value;
				if(previousClientCnx != null && previousClientCnx != value)
				{
					var previousName = int.Parse(previousClientCnx.Path.Name);
					previousClientCnx.Tell(new RemoveConsumer(previousName));
				}
			}
		}


		internal virtual void DeregisterFromClientCnx()
		{
			ClientCnx = null;
		}

		internal virtual void ReconnectLater(Exception exception)
		{
			_connectionHandler.Tell(new ReconnectLater(exception));
		}

		internal virtual void GrabCnx()
		{
			_connectionHandler.Tell(new GrabCnx(""));
		}

		internal virtual string TopicNameWithoutPartition
		{
			get
			{
				return _topicNameWithoutPartition;
			}
		}

		internal class ChunkedMessageCtx
		{

			protected internal int TotalChunks = -1;
			protected internal byte[] ChunkedMsgBuffer;
			protected internal int LastChunkedMessageId = -1;
			protected internal MessageId[] ChunkedMessageIds;
			protected internal long ReceivedTime = 0;

			internal static ChunkedMessageCtx Get(int numChunksFromMsg, byte[] chunkedMsgBuffer)
			{
				ChunkedMessageCtx ctx = new ChunkedMessageCtx();
				ctx.TotalChunks = numChunksFromMsg;
				ctx.ChunkedMsgBuffer = chunkedMsgBuffer;
				ctx.ChunkedMessageIds = new MessageId[numChunksFromMsg];
				ctx.ReceivedTime = DateTimeHelper.CurrentUnixTimeMillis();
				return ctx;
			}

			internal virtual void Recycle()
			{
				TotalChunks = -1;
				ChunkedMsgBuffer = null;
				LastChunkedMessageId = -1;
			}
		}

		private void RemoveOldestPendingChunkedMessage()
		{
			ChunkedMessageCtx chunkedMsgCtx = null;
			string firstPendingMsgUuid = null;
			while(chunkedMsgCtx == null && _pendingChunckedMessageUuidQueue.Count > 0)
			{
				// remove oldest pending chunked-message group and free memory
				firstPendingMsgUuid = _pendingChunckedMessageUuidQueue.Dequeue();
				chunkedMsgCtx = !string.IsNullOrWhiteSpace(firstPendingMsgUuid) ? ChunkedMessagesMap[firstPendingMsgUuid] : null;
			}
			RemoveChunkMessage(firstPendingMsgUuid, chunkedMsgCtx, this._autoAckOldestChunkedMessageOnQueueFull);
		}

		protected internal virtual void RemoveExpireIncompleteChunkedMessages()
		{
			if(ExpireTimeOfIncompleteChunkedMessageMillis <= 0)
			{
				return;
			}
			ChunkedMessageCtx chunkedMsgCtx = null;
			string messageUUID;
			while(!ReferenceEquals((messageUUID = _pendingChunckedMessageUuidQueue.Dequeue()), null))
			{
				chunkedMsgCtx = !string.IsNullOrWhiteSpace(messageUUID) ? ChunkedMessagesMap[messageUUID] : null;
				if(chunkedMsgCtx != null && DateTimeHelper.CurrentUnixTimeMillis() > (chunkedMsgCtx.ReceivedTime + ExpireTimeOfIncompleteChunkedMessageMillis))
				{
					RemoveChunkMessage(messageUUID, chunkedMsgCtx, true);
				}
				else
				{
					return;
				}
			}
		}

		private void RemoveChunkMessage(string msgUUID, ChunkedMessageCtx chunkedMsgCtx, bool autoAck)
		{
			if(chunkedMsgCtx == null)
			{
				return;
			}
			// clean up pending chuncked-Message
			ChunkedMessagesMap.Remove(msgUUID);
			if(chunkedMsgCtx.ChunkedMessageIds != null)
			{
				foreach(MessageId msgId in chunkedMsgCtx.ChunkedMessageIds)
				{
					if(msgId == null)
					{
						continue;
					}
					if(autoAck)
					{
						_log.Info("Removing chunk message-id {}", msgId);
						DoAcknowledge(msgId, CommandAck.AckType.Individual, new Dictionary<string, long>(), null);
					}
					else
					{
						TrackMessage(msgId);
					}
				}
			}
			if(chunkedMsgCtx.ChunkedMsgBuffer != null)
			{
				chunkedMsgCtx.ChunkedMsgBuffer = null;
			}
			chunkedMsgCtx.Recycle();
			_pendingChunckedMessageCount--;
		}

		private void DoTransactionAcknowledgeForResponse(IMessageId messageId, CommandAck.AckType ackType, CommandAck.ValidationError? validationError, IDictionary<string, long> properties, TxnID txnID)
		{
			long ledgerId;
			long entryId;
			byte[] cmd;
			long requestId = _client.AskFor<NewRequestIdResponse>(NewRequestId.Instance).Id;
			if(messageId is BatchMessageId batchMessageId)
			{
				var bitSet = BitSet.Create();
				ledgerId = batchMessageId.LedgerId;
				entryId = batchMessageId.EntryId;
				if(ackType == CommandAck.AckType.Cumulative)
				{
					batchMessageId.AckCumulative();
					bitSet.Set(0, batchMessageId.BatchSize);
					bitSet.Clear(0, batchMessageId.BatchIndex + 1);
				}
				else
				{
					bitSet.Set(0, batchMessageId.BatchSize);
					bitSet.Clear(batchMessageId.BatchIndex);
				}
				cmd = Commands.NewAck(ConsumerId, ledgerId, entryId, bitSet.ToLongArray(), ackType, validationError, properties, txnID.LeastSigBits, txnID.MostSigBits, requestId, batchMessageId.BatchSize);
			}
			else
			{
				var singleMessage = (MessageId) messageId;
				ledgerId = singleMessage.LedgerId;
				entryId = singleMessage.EntryId;
				cmd = Commands.NewAck(ConsumerId, ledgerId, entryId, new long[]{ }, ackType, validationError, properties, txnID.LeastSigBits, txnID.MostSigBits, requestId);
			}

			//OpForAckCallBack op = OpForAckCallBack.Create(cmd, callBack, messageId, new TxnID(txnID.MostSigBits, txnID.LeastSigBits));
			//_ackRequests.Put(requestId, op);
			if(ackType == CommandAck.AckType.Cumulative)
			{
				_unAckedMessageTracker.Tell(new RemoveMessagesTill(messageId));
			}
			else
			{
				_unAckedMessageTracker.Tell(new Remove(messageId));
			}
			var payload = new Payload(cmd, requestId, "NewAck");
			_cnx.Tell(payload);
		}


	}

}