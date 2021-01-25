using Akka.Actor;
using Akka.Event;
using Akka.Util;
using Akka.Util.Internal;
using BAMCIS.Util.Concurrent;
using SharpPulsar.Akka;
using SharpPulsar.Akka.Network;
using SharpPulsar.Auth;
using SharpPulsar.Batch;
using SharpPulsar.Common;
using SharpPulsar.Common.Compression;
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
using SharpPulsar.Queues;
using SharpPulsar.Shared;
using SharpPulsar.Stats.Consumer;
using SharpPulsar.Stats.Consumer.Api;
using SharpPulsar.Tracker;
using SharpPulsar.Tracker.Messages;
using SharpPulsar.Transaction;
using SharpPulsar.Utils;
using System;
using System.Buffers;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
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

    public partial class ConsumerActor<T> : ConsumerActorBase<T>, IWithUnboundedStash
	{
		private const int MaxRedeliverUnacknowledged = 1000;

		internal readonly long ConsumerId;

		// Number of messages that have delivered to the application. Every once in a while, this number will be sent to the
		// broker to notify that we are ready to get (and store in the incoming messages queue) more messages

		private int _availablePermits = 0;

		protected IMessageId _lastDequeuedMessageId = IMessageId.Earliest;
		private IMessageId _lastMessageIdInBroker = IMessageId.Earliest;

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

		private readonly IConsumerStatsRecorder _stats;

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
		protected readonly ILoggingAdapter _log;

		private readonly bool _createTopicIfDoesNotExist;
		protected IActorRef _self;

		private IActorRef _clientCnxUsedForConsumerRegistration;

		public static Props NewConsumer(IActorRef client, string topic, ConsumerConfigurationData<T> conf, IAdvancedScheduler listenerExecutor, int partitionIndex, bool hasParentConsumer, IMessageId startMessageId, ISchema<T> schema, ConsumerInterceptors<T> interceptors, bool createTopicIfDoesNotExist, ClientConfigurationData clientConfigurationData, ConsumerQueueCollections<T> consumerQueue)
        {
			return Props.Create(() => new ConsumerActor<T>(client, topic, conf, listenerExecutor, partitionIndex, hasParentConsumer, startMessageId, 0, schema, interceptors, createTopicIfDoesNotExist, clientConfigurationData, consumerQueue));
        }
		
		public static Props NewConsumer(IActorRef client, string topic, ConsumerConfigurationData<T> conf, IAdvancedScheduler listenerExecutor, int partitionIndex, bool hasParentConsumer, IMessageId startMessageId, ISchema<T> schema, ConsumerInterceptors<T> interceptors, bool createTopicIfDoesNotExist, long startMessageRollbackDurationInSec, ClientConfigurationData clientConfigurationData, ConsumerQueueCollections<T> consumerQueue)
		{
			if(conf.ReceiverQueueSize == 0)
			{
				return ZeroQueueConsumer<T>.NewZeroQueueConsumer(client, topic, conf, listenerExecutor, partitionIndex, hasParentConsumer, startMessageId, schema, interceptors, createTopicIfDoesNotExist, clientConfigurationData, consumerQueue);
			}
			else
			{
				return Props.Create(() => new ConsumerActor<T>(client, topic, conf, listenerExecutor, partitionIndex, hasParentConsumer, startMessageId, startMessageRollbackDurationInSec, schema, interceptors, createTopicIfDoesNotExist, clientConfigurationData, consumerQueue));
			}
		}

		public ConsumerActor(IActorRef client, string topic, ConsumerConfigurationData<T> conf, IAdvancedScheduler listenerExecutor, int partitionIndex, bool hasParentConsumer, IMessageId startMessageId, long startMessageRollbackDurationInSec, ISchema<T> schema, ConsumerInterceptors<T> interceptors, bool createTopicIfDoesNotExist, ClientConfigurationData clientConfiguration, ConsumerQueueCollections<T> consumerQueue) : base(client, topic, conf, conf.ReceiverQueueSize, listenerExecutor, schema, interceptors, consumerQueue)
		{
			_self = Self;
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
			_negativeAcksTracker = Context.ActorOf(NegativeAcksTracker<T>.Prop(conf, Self));
			_resetIncludeHead = conf.ResetIncludeHead;
			_createTopicIfDoesNotExist = createTopicIfDoesNotExist;
			_maxPendingChuckedMessage = conf.MaxPendingChuckedMessage;
			_pendingChunckedMessageUuidQueue = new Queue<string>();
			ExpireTimeOfIncompleteChunkedMessageMillis = conf.ExpireTimeOfIncompleteChunkedMessageMillis;
			_autoAckOldestChunkedMessageOnQueueFull = conf.AutoAckOldestChunkedMessageOnQueueFull;

			if(clientConfiguration.StatsIntervalSeconds > 0)
			{
				_stats = new ConsumerStatsRecorder<T>(Context.System, conf, _topicName.ToString(), ConsumerName, Subscription, clientConfiguration.StatsIntervalSeconds);
			}
			else
			{
				_stats = ConsumerStatsDisabled.Instance;
			}

			_duringSeek = new AtomicBoolean(false);

			if(conf.AckTimeoutMillis != 0)
			{
				if(conf.TickDurationMillis > 0)
				{
					_unAckedMessageTracker = Context.ActorOf(Tracker.UnAckedMessageTracker.Prop(conf.AckTimeoutMillis, Math.Min(conf.TickDurationMillis, conf.AckTimeoutMillis), Self), "UnAckedMessageTracker");
				}
				else
				{
					_unAckedMessageTracker = Context.ActorOf(Tracker.UnAckedMessageTracker.Prop(conf.AckTimeoutMillis, 0, Self), "UnAckedMessageTracker");
				}
			}
			else
			{
				_unAckedMessageTracker = Context.ActorOf(UnAckedMessageTrackerDisabled.Prop(), "UnAckedMessageTrackerDisabled");
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

			 GrabCnx();
		}
		private void Ready()
        {			
			Receive<ClearIncomingMessagesAndGetMessageNumber>(_ => 
			{
				var cleared = ClearIncomingMessagesAndGetMessageNumber();
				Sender.Tell(cleared);
			});
			Receive<IncreaseAvailablePermits>(_ => 
			{
				var cnx = Cnx();
				IncreaseAvailablePermits(cnx);
			});
			Receive<BatchReceive>(_ => 
			{
                try
                {
					var message = BatchReceive();
					Push(ConsumerQueue.BatchReceive, message);
				}
                catch(Exception ex)
                {
					var nul = new NullMessages<T>(ex);
					Push(ConsumerQueue.BatchReceive, nul);
				}

			});
			Receive<Messages.Consumer.Receive>(_ => 
			{
                try
                {
					var message = Receive();
					Push(ConsumerQueue.Receive, message);
				}
                catch(Exception ex)
                {
					var nul = new NullMessage<T>(ex);
					Push(ConsumerQueue.Receive, nul);
				}

			});
			Receive<ReceiveWithTimeout>(t => 
			{
                try
                {
					var message = Receive(t.Timeout, t.TimeUnit);
					if (message == null)
						message = new NullMessage<T>(new NullReferenceException("We did not receive a message within the given timeout"));

					Push(ConsumerQueue.Receive, message);
				}
                catch(Exception ex)
                {
					var nul = new NullMessage<T>(ex);
					Push(ConsumerQueue.Receive, nul);
				}

			});
			Receive<AcknowledgeMessage<T>>(m => {
                try
                {
					Acknowledge(m.Message);

					Push(ConsumerQueue.AcknowledgeException, null);
				}
                catch(Exception ex)
				{
					Push(ConsumerQueue.AcknowledgeException, new ClientExceptions(new PulsarClientException(ex)));
				}
			});
			Receive<AcknowledgeMessageId>(m => {
                try
                {
					Acknowledge(m.MessageId);
					Push(ConsumerQueue.AcknowledgeException, null);
				}
                catch(Exception ex)
				{
					Push(ConsumerQueue.AcknowledgeException, new ClientExceptions(new PulsarClientException(ex)));
				}
			});
			Receive<AcknowledgeMessageIds>(m => {
                try
                {
					Acknowledge(m.MessageIds);
					Push(ConsumerQueue.AcknowledgeException, null);
				}
                catch(Exception ex)
				{
					Push(ConsumerQueue.AcknowledgeException, new ClientExceptions(new PulsarClientException(ex)));
				}
			});
			Receive<AcknowledgeMessages<T>>(m => {
                try
                {
					Acknowledge(m.Messages);
					Push(ConsumerQueue.AcknowledgeException, null);
				}
                catch(Exception ex)
				{
					Push(ConsumerQueue.AcknowledgeException, new ClientExceptions(new PulsarClientException(ex)));
				}
			});
			Receive<AcknowledgeCumulativeMessageId>(m => {
				try
				{
					AcknowledgeCumulative(m.MessageId);
					Push(ConsumerQueue.AcknowledgeCumulativeException, null);
				}
				catch (Exception ex)
				{
					Push(ConsumerQueue.AcknowledgeCumulativeException, new ClientExceptions(new PulsarClientException(ex)));
				}
			});
			Receive<AcknowledgeCumulativeTxn>(m => {
				try
				{
					AcknowledgeCumulative(m.MessageId, m.Txn);
					Push(ConsumerQueue.AcknowledgeCumulativeException, null);
				}
				catch (Exception ex)
				{
					Push(ConsumerQueue.AcknowledgeCumulativeException, new ClientExceptions(new PulsarClientException(ex)));
				}
			});
			Receive<ReconsumeLaterCumulative<T>>(m => {
				try
				{
					ReconsumeLaterCumulative(m.Message, m.DelayTime, m.TimeUnit);
					Push(ConsumerQueue.AcknowledgeCumulativeException, null);
				}
				catch (Exception ex)
				{
					Push(ConsumerQueue.AcknowledgeCumulativeException, new ClientExceptions(new PulsarClientException(ex)));
				}
			});
			Receive<GetLastDisconnectedTimestamp>(m => 
			{
				Push(ConsumerQueue.LastDisconnectedTimestamp, LastDisconnectedTimestamp);
			});
			Receive<ReconsumeLaterCumulative<T>>(m => {
				try
				{
					ReconsumeLaterCumulative(m.Message, m.DelayTime, m.TimeUnit);
					Push(ConsumerQueue.AcknowledgeCumulativeException, null);
				}
				catch (Exception ex)
				{
					Push(ConsumerQueue.AcknowledgeCumulativeException, new ClientExceptions(new PulsarClientException(ex)));
				}
			});
			Receive<GetConsumerName>(m => {
				Push(ConsumerQueue.ConsumerName, ConsumerName);
			});
			Receive<GetSubscription>(m => {
				Push(ConsumerQueue.Subscription, Subscription);
			});
			Receive<GetTopic>(m => {
				Push(ConsumerQueue.Topic, _topicName.ToString());
			});
			Receive<HasReachedEndOfTopic>(_ => {
				var hasReached = HasReachedEndOfTopic();
				Push(ConsumerQueue.HasReachedEndOfTopic, hasReached);
			});
			Receive<IsConnected>(_ => {
				Push(ConsumerQueue.Connected, Connected);
			});
			Receive<Pause>(_ => {
				Pause();
			});
			Receive<Resume>(_ => {
				Resume();
			});
			Receive<GetLastMessageId>(m => 
			{
                try
                {
					var lmsid = LastMessageId;
					Push(ConsumerQueue.LastMessageId, lmsid);
				}
                catch (Exception ex)
                {
					var nul = new NullMessageId(ex);
					Push(ConsumerQueue.LastMessageId, nul);
				}
			});
			Receive<GetStats>(m => 
			{
                try
                {
					var stats = Stats;
					Push(ConsumerQueue.Stats, stats);
				}
                catch (Exception ex)
                {
					_log.Error(ex.ToString());
					Push(ConsumerQueue.Stats, null);
				}
			});
			Receive<NegativeAcknowledgeMessage<T>>(m => 
			{
                try
                {
					NegativeAcknowledge(m.Message);
					Push(ConsumerQueue.NegativeAcknowledgeException, null);
				}
                catch (Exception ex)
                {
					Push(ConsumerQueue.NegativeAcknowledgeException, new ClientExceptions(PulsarClientException.Unwrap(ex)));
				}
			});
			Receive<NegativeAcknowledgeMessages<T>>(m => 
			{
                try
                {
					NegativeAcknowledge(m.Messages);
					Push(ConsumerQueue.NegativeAcknowledgeException, null);
				}
                catch (Exception ex)
                {
					Push(ConsumerQueue.NegativeAcknowledgeException, new ClientExceptions(PulsarClientException.Unwrap(ex)));
				}
			});
			Receive<NegativeAcknowledgeMessageId>(m => 
			{
                try
                {
					NegativeAcknowledge(m.MessageId);
					Push(ConsumerQueue.NegativeAcknowledgeException, null);
				}
                catch (Exception ex)
                {
					Push(ConsumerQueue.NegativeAcknowledgeException, new ClientExceptions(PulsarClientException.Unwrap(ex)));
				}
			});
			Receive<ReconsumeLaterMessages<T>>(m => 
			{
                try
                {
					ReconsumeLater(m.Messages, m.DelayTime, m.TimeUnit);
					Push(ConsumerQueue.ReconsumeLaterException, null);
				}
                catch (Exception ex)
                {
					Push(ConsumerQueue.ReconsumeLaterException, new ClientExceptions(PulsarClientException.Unwrap(ex)));
				}
			});
			Receive<ReconsumeLaterMessage<T>>(m => 
			{
                try
                {
					ReconsumeLater(m.Message, m.DelayTime, m.TimeUnit);
					Push(ConsumerQueue.ReconsumeLaterException, null);
				}
                catch (Exception ex)
                {
					Push(ConsumerQueue.ReconsumeLaterException, new ClientExceptions(PulsarClientException.Unwrap(ex)));
				}
			});
			Receive<Messages.Consumer.RedeliverUnacknowledgedMessages>(m => 
			{
                try
                {
					RedeliverUnacknowledgedMessages();
					Push(ConsumerQueue.RedeliverUnacknowledgedException, null);
				}
                catch (Exception ex)
                {
					Push(ConsumerQueue.RedeliverUnacknowledgedException, new ClientExceptions(PulsarClientException.Unwrap(ex)));
				}
			});
			Receive<Unsubscribe>(_ => 
			{
                try
                {
					Unsubscribe();
					Push(ConsumerQueue.UnsubscribeException, null);
				}
                catch (Exception ex)
                {
					Push(ConsumerQueue.UnsubscribeException, new ClientExceptions(PulsarClientException.Unwrap(ex)));
				}
			});
			Receive<SeekMessageId>(m => 
			{
                try
                {
					Seek(m.MessageId);
					Push(ConsumerQueue.SeekException, null);
				}
                catch (Exception ex)
                {
					Push(ConsumerQueue.SeekException, new ClientExceptions(PulsarClientException.Unwrap(ex)));
				}
			});
			Receive<SeekTimestamp>(m => 
			{
                try
                {
					Seek(m.Timestamp);
					Push(ConsumerQueue.SeekException, null);
				}
                catch (Exception ex)
                {
					Push(ConsumerQueue.SeekException, new ClientExceptions(PulsarClientException.Unwrap(ex)));
				}
			});
			Stash.UnstashAll();
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
			if (State.ConnectionState == HandlerState.State.Closing || State.ConnectionState == HandlerState.State.Closed)
			{
				CloseConsumerTasks();
			}

			if (!Connected)
			{
				_log.Info($"[{Topic}] [{Subscription}] Closed Consumer (not connected)");
				State.ConnectionState = HandlerState.State.Closed;
				CloseConsumerTasks();
				DeregisterFromClientCnx();
				_client.Tell(new CleanupConsumer(Self));
			}

			_stats.StatTimeout.Cancel();

			State.ConnectionState = HandlerState.State.Closing;

			CloseConsumerTasks();

			long requestId = _client.AskFor<NewRequestIdResponse>(NewRequestId.Instance).Id;
			var cnx = Cnx();
			if (null == cnx)
			{
				CleanupAtClose(null);
			}
			else
			{
				byte[] cmd = Commands.NewCloseConsumer(ConsumerId, requestId);
				var response = cnx.AskFor(new SendRequestWithId(cmd, requestId));
				if(response is ClientExceptions ex)
                {
					_log.Debug($"Exception ignored in closing consumer {ex.Exception}");
					CleanupAtClose(ex.Exception);
				}
			}

			base.PostStop();
        }
		protected internal override IMessages<T> InternalBatchReceive()
		{
			try
			{
				var messages = NewMessages;
				if (HasEnoughMessagesForBatchReceive())
				{
					IMessage<T> msg = IncomingMessages.Take(_tokenSource.Token);
					while (IncomingMessages.TryTake(out msg) && messages.CanAdd(msg))
					{
						MessageProcessed(msg);
						IMessage<T> interceptMsg = BeforeConsume(msg);
						messages.Add(interceptMsg);
					}
				}
				return messages;
			}
			catch(Exception e) 
			{
				HandlerState.State state = State.ConnectionState;
				var error = PulsarClientException.Unwrap(e);
				if (state != HandlerState.State.Closing && state != HandlerState.State.Closed)
				{
					_stats.IncrementNumBatchReceiveFailed();
				}
				throw error;
			}
		}
		internal override IConsumerStatsRecorder Stats
		{
			get
			{
				return _stats;
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

		protected internal override void DoAcknowledge(IMessageId messageId, CommandAck.AckType ackType, IDictionary<string, long> properties, IActorRef txn)
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

		protected internal override void DoAcknowledge(IList<IMessageId> messageIdList, CommandAck.AckType ackType, IDictionary<string, long> properties, IActorRef txn)
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

		protected internal override void DoReconsumeLater<T1>(IMessage<T1> message, CommandAck.AckType ackType, IDictionary<string, long> properties, long delayTime, TimeUnit unit)
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
				var removed = _unAckedMessageTracker.AskFor<int>(new RemoveMessagesTill(msgId));
				_stats.IncrementNumAcksSent(removed);
			}

			_acknowledgmentsGroupingTracker.Tell(new AddAcknowledgment(msgId, ackType, properties, txnImpl));

			// Consumer acknowledgment operation immediately succeeds. In any case, if we're not able to send ack to broker,
			// the messages will be re-delivered
			//return CompletableFuture.completedFuture(null);
		}

		internal override void NegativeAcknowledge(IMessageId messageId)
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
			var request = Commands.NewSubscribe(Topic, Subscription, ConsumerId, requestId, SubType, _priorityLevel, ConsumerName, isDurable, startMessageIdData, _metadata, _readCompacted, Conf.ReplicateSubscriptionState, _subscriptionInitialPosition.ValueOf(), startMessageRollbackDuration, si, _createTopicIfDoesNotExist, Conf.KeySharedPolicy);
			
			var result = cnx.AskFor(new SendRequestWithId(request, requestId));
			if(result is CommandSuccessResponse)
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
				var e = result as ClientExceptions;
				DeregisterFromClientCnx();
				if (State.ConnectionState == HandlerState.State.Closing || State.ConnectionState == HandlerState.State.Closed)
				{
					cnx.GracefulStop(TimeSpan.FromSeconds(1));
					return;
				}
				
				_log.Warning($"[{Topic}][{Subscription}] Failed to subscribe to topic on");
				if (e.Exception is PulsarClientException && PulsarClientException.IsRetriableError(e.Exception) && DateTimeHelper.CurrentUnixTimeMillis() < _subscribeTimeout)
				{
					ReconnectLater(e.Exception);
				}
				else if (e.Exception is PulsarClientException.TopicDoesNotExistException)
				{
					State.ConnectionState = HandlerState.State.Failed;
					CloseConsumerTasks();
					_client.Tell(new CleanupConsumer(Self));
					_log.Warning($"[{Topic}][{Subscription}] Closed consumer because topic does not exist anymore");
				}
				else
				{
					ReconnectLater(e.Exception);
				}
			}
		}

		protected internal virtual void ConsumerIsReconnectedToBroker(IActorRef cnx, int currentQueueSize)
		{
			_log.Info($"[{Topic}][{Subscription}] Subscribed to topic on -- consumer: {ConsumerId}");

			_availablePermits = 0;
			Become(Ready);
		}

		/// <summary>
		/// Clear the internal receiver queue and returns the message id of what was the 1st message in the queue that was
		/// not seen by the application
		/// </summary>
		private BatchMessageId ClearReceiverQueue()
		{
			var currentMessageQueue = new List<IMessage<T>>(IncomingMessages.Count);
			var mcount = IncomingMessages.Count;
			var n = 0;
			while (n < mcount)
			{

				if (IncomingMessages.TryTake(out var m))
					currentMessageQueue.Add(m);
				else
					break;
				++n;
			}
			IncomingMessagesSize = 0;

			if (_duringSeek)
			{
				_duringSeek = false;
				return _seekMessageId;
			}
			else if (_subscriptionMode == SubscriptionMode.Durable)
			{
				return _startMessageId;
			}

			if(currentMessageQueue.Count > 0)
			{
				var nextMessageInQueue = currentMessageQueue[0].MessageId;
				BatchMessageId previousMessage;
				if(nextMessageInQueue is BatchMessageId next)
				{
					// Get on the previous message within the current batch
					previousMessage = new BatchMessageId(next.LedgerId, next.EntryId, next.PartitionIndex, next.BatchIndex - 1);
				}
				else
				{
					var msgid = (MessageId)nextMessageInQueue;
					// Get on previous message in previous entry
					previousMessage = new BatchMessageId(msgid.LedgerId, msgid.EntryId - 1, msgid.PartitionIndex, -1);
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
		private void SendFlowPermitsToBroker(IActorRef cnx, int numMessages)
		{
			if(cnx != null)
			{
				if(_log.IsDebugEnabled)
				{
					_log.Debug($"[{Topic}] [{Subscription}] Adding {numMessages} additional permits");
				}
				cnx.Tell(Commands.NewFlow(ConsumerId, numMessages));
			}
		}

		internal virtual void ConnectionFailed(PulsarClientException exception)
		{
			bool nonRetriableError = !PulsarClientException.IsRetriableError(exception);
			bool timeout = DateTimeHelper.CurrentUnixTimeMillis() > _subscribeTimeout;
			if((nonRetriableError || timeout))
			{
				State.ConnectionState = HandlerState.State.Failed;
				if(nonRetriableError)
				{
					_log.Info($"[{Topic}] Consumer creation failed for consumer {ConsumerId} with unretriableError: {exception}");
				}
				else
				{
					_log.Info($"[{Topic}] Consumer creation failed for consumer {ConsumerId} after timeout");
				}
				CloseConsumerTasks();
				DeregisterFromClientCnx();
				_client.Tell(new CleanupConsumer(Self));
			}
		}

		private void CleanupAtClose(Exception exception)
		{
			_log.Info($"[{Topic}] [{Subscription}] Closed consumer");
			State.ConnectionState = HandlerState.State.Closed;
			CloseConsumerTasks();
			DeregisterFromClientCnx();
			_client.Tell(new CleanupConsumer(Self));
		}

		private void CloseConsumerTasks()
		{
			_unAckedMessageTracker.GracefulStop(TimeSpan.FromSeconds(3));
			if(_possibleSendToDeadLetterTopicMessages != null)
			{
				_possibleSendToDeadLetterTopicMessages.Clear();
			}
			_acknowledgmentsGroupingTracker.GracefulStop(TimeSpan.FromSeconds(3));
			if (BatchReceiveTimeout != null)
			{
				BatchReceiveTimeout.Cancel();
			}
			Stats.StatTimeout.Cancel();
		}

		internal virtual void ActiveConsumerChanged(bool isActive)
		{
			if(ConsumerEventListener == null)
			{
				return;
			}

			if (isActive)
			{
				ConsumerEventListener.BecameActive(Self, _partitionIndex);
			}
			else
			{
				ConsumerEventListener.BecameInactive(Self, _partitionIndex);
			}
		}

		internal virtual void MessageReceived(MessageIdData messageId, int redeliveryCount, IList<long> ackSet, byte[] headersAndPayload, IActorRef cnx)
		{
			var data = new ReadOnlySequence<byte>(headersAndPayload);
			if(_log.IsDebugEnabled)
			{
				_log.Debug($"[{Topic}][{Subscription}] Received message: {messageId.ledgerId}/{messageId.entryId}");
			}

			if(!data.IsValid())
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
		
			bool isChunkedMessage = msgMetadata.NumChunksFromMsg > 1 && Conf.SubscriptionType != CommandSubscribe.SubType.Shared;

			MessageId msgId = new MessageId((long)messageId.ledgerId, (long)messageId.entryId, PartitionIndex);
			var isDuplicate = _acknowledgmentsGroupingTracker.AskFor<bool>(new IsDuplicate(msgId));
			if (isDuplicate)
			{
				if(_log.IsDebugEnabled)
				{
					_log.Debug($"[{Topic}] [{Subscription}] Ignoring message as it was already being acked earlier by same consumer {ConsumerName}/{msgId}");
				}

				IncreaseAvailablePermits(cnx, numMessages);
				return;
			}

			byte[] decryptedPayload = DecryptPayloadIfNeeded(messageId, msgMetadata, data, cnx);

			bool isMessageUndecryptable = IsMessageUndecryptable(msgMetadata);

			if(decryptedPayload == null)
			{
				// Message was discarded or CryptoKeyReader isn't implemented
				return;
			}

			// uncompress decryptedPayload and release decryptedPayload-ByteBuf
			var uncompressedPayload = (isMessageUndecryptable || isChunkedMessage) ? decryptedPayload : UncompressPayloadIfNeeded(messageId, msgMetadata, decryptedPayload, cnx, true);
			
			if(uncompressedPayload == null)
			{
				// Message was discarded on decompression error
				return;
			}

			// if message is not decryptable then it can't be parsed as a batch-message. so, add EncyrptionCtx to message
			// and return undecrypted payload
			if(isMessageUndecryptable || (numMessages == 1 && !HasNumMessagesInBatch(msgMetadata)))
			{

				// right now, chunked messages are only supported by non-shared subscription
				if(isChunkedMessage)
				{
					uncompressedPayload = ProcessMessageChunk(uncompressedPayload, msgMetadata, msgId, messageId, cnx);
					if(uncompressedPayload == null)
					{
						return;
					}
				}

				if(IsSameEntry(messageId) && IsPriorEntryIndex((long)messageId.entryId))
				{
					// We need to discard entries that were prior to startMessageId
					if(_log.IsDebugEnabled)
					{
						_log.Debug($"[{Subscription}] [{ConsumerName}] Ignoring message from before the startMessageId: {_startMessageId}");
					}

					return;
				}

				var message = new Message<T>(_topicName.ToString(), msgId, msgMetadata, uncompressedPayload, CreateEncryptionContext(msgMetadata), cnx, Schema, redeliveryCount);
				
				try
				{
					// Enqueue the message so that it can be retrieved when application calls receive()
					// if the conf.getReceiverQueueSize() is 0 then discard message if no one is waiting for it.
					// if asyncReceive is waiting then notify callback without adding to incomingMessages queue
					if(_deadLetterPolicy != null && _possibleSendToDeadLetterTopicMessages != null && redeliveryCount >= _deadLetterPolicy.MaxRedeliverCount)
					{
						_possibleSendToDeadLetterTopicMessages[(MessageId)message.MessageId] = new List<IMessage<T>> { message };
					}
				}
				finally
				{
					
				}
			}
			else
			{
				// handle batch message enqueuing; uncompressed payload has all messages in batch
				ReceiveIndividualMessagesFromBatch(msgMetadata, redeliveryCount, ackSet, uncompressedPayload, messageId, cnx);

			}

			if(Listener != null)
			{
				TriggerListener(numMessages);
			}
		}
		private bool HasNumMessagesInBatch(MessageMetadata m)
		{
			var should = m.ShouldSerializeNumMessagesInBatch();
			return should;
		}
		private bool IsTxnMessage(MessageMetadata messageMetadata)
		{
			return messageMetadata.TxnidMostBits > 0 && messageMetadata.TxnidLeastBits > 0;
		}

		private byte[] ProcessMessageChunk(byte[] compressedPayload, MessageMetadata msgMetadata, MessageId msgId, MessageIdData messageId, IActorRef cnx)
		{
			
			// Lazy task scheduling to expire incomplete chunk message
			if (!_expireChunkMessageTaskScheduled && ExpireTimeOfIncompleteChunkedMessageMillis > 0)
			{				
				Context.System.Scheduler.Advanced.ScheduleRepeatedly(TimeSpan.FromMilliseconds(ExpireTimeOfIncompleteChunkedMessageMillis), TimeSpan.FromMilliseconds(ExpireTimeOfIncompleteChunkedMessageMillis), RemoveExpireIncompleteChunkedMessages);
				_expireChunkMessageTaskScheduled = true;
			}

			if (msgMetadata.ChunkId == 0)
			{
				var totalChunks = msgMetadata.NumChunksFromMsg;
				ChunkedMessagesMap.TryAdd(msgMetadata.Uuid, ChunkedMessageCtx.Get(totalChunks, new List<byte>()));
				_pendingChunckedMessageCount++;
				if (_maxPendingChuckedMessage > 0 && _pendingChunckedMessageCount > _maxPendingChuckedMessage)
				{
					RemoveOldestPendingChunkedMessage();
				}
				_pendingChunckedMessageUuidQueue.Enqueue(msgMetadata.Uuid);
			}

			ChunkedMessageCtx chunkedMsgCtx = ChunkedMessagesMap[msgMetadata.Uuid];
			// discard message if chunk is out-of-order
			if(chunkedMsgCtx == null || chunkedMsgCtx.ChunkedMsgBuffer == null || msgMetadata.ChunkId != (chunkedMsgCtx.LastChunkedMessageId + 1) || msgMetadata.ChunkId >= msgMetadata.TotalChunkMsgSize)
			{
				// means we lost the first chunk: should never happen
				_log.Info($"Received unexpected chunk messageId {msgId}, last-chunk-id{chunkedMsgCtx?.LastChunkedMessageId ?? 0}, chunkId = {msgMetadata.ChunkId}, total-chunks {msgMetadata.TotalChunkMsgSize}");
				chunkedMsgCtx?.Recycle();
				ChunkedMessagesMap.Remove(msgMetadata.Uuid);
				IncreaseAvailablePermits(cnx);
				if(ExpireTimeOfIncompleteChunkedMessageMillis > 0 && DateTimeHelper.CurrentUnixTimeMillis() > ((long)msgMetadata.PublishTime + ExpireTimeOfIncompleteChunkedMessageMillis))
				{
					DoAcknowledge(msgId, CommandAck.AckType.Individual, new Dictionary<string, long>(), null);
				}
				else
				{
					TrackMessage(msgId);
				}
				return null;
			}

			chunkedMsgCtx.ChunkedMessageIds[msgMetadata.ChunkId] = msgId;
			// append the chunked payload and update lastChunkedMessage-id
			chunkedMsgCtx.ChunkedMsgBuffer.AddRange(compressedPayload);
			chunkedMsgCtx.LastChunkedMessageId = msgMetadata.ChunkId;

			// if final chunk is not received yet then release payload and return
			if (msgMetadata.ChunkId != (msgMetadata.NumChunksFromMsg - 1))
			{
				IncreaseAvailablePermits(cnx);
				return null;
			}


			// last chunk received: so, stitch chunked-messages and clear up chunkedMsgBuffer
			if(_log.IsDebugEnabled)
			{
				_log.Debug($"Chunked message completed chunkId {msgMetadata.ChunkId}, total-chunks {msgMetadata.NumChunksFromMsg}, msgId {msgId} sequenceId {msgMetadata.SequenceId}");
			}
			// remove buffer from the map, add chucked messageId to unack-message tracker, and reduce pending-chunked-message count
			ChunkedMessagesMap.Remove(msgMetadata.Uuid);
			UnAckedChunckedMessageIdSequenceMap.Add(msgId, chunkedMsgCtx.ChunkedMessageIds);
			_pendingChunckedMessageCount--;
			compressedPayload = chunkedMsgCtx.ChunkedMsgBuffer.ToArray();
			chunkedMsgCtx.Recycle();
			var uncompressedPayload = UncompressPayloadIfNeeded(messageId, msgMetadata, compressedPayload, cnx, false);
			return uncompressedPayload;
		}

		protected internal virtual void TriggerListener(int numMessages)
		{
			// Trigger the notification on the message listener in a separate thread to avoid blocking the networking
			// thread while the message processing happens
			Task.Run(() =>
			{
				var self = Self;
				for(int i = 0; i < numMessages; i++)
				{
					try
					{
						IMessage<T> msg = InternalReceive(0, TimeUnit.MILLISECONDS);
						if(msg == null)
						{
							if(_log.IsDebugEnabled)
							{
								_log.Debug($"[{Topic}] [{Subscription}] Message has been cleared from the queue");
							}
							break;
						}
						try
						{
							if(_log.IsDebugEnabled)
							{
								_log.Debug($"[{Topic}][{Subscription}] Calling message listener for message {msg.MessageId}");
							}
							Listener.Received(self, msg);
						}
						catch(Exception t)
						{
							_log.Error($"[{Topic}][{Subscription}] Message listener error in processing message: {msg.MessageId} => {t}");
						}
					}
					catch(PulsarClientException e)
					{
						_log.Warning($"[{Topic}] [{Subscription}] Failed to dequeue the message for listener: {e}");
						return;
					}
				}
			});
		}
		protected internal override IMessage<T> InternalReceive(int timeout, TimeUnit unit)
		{
            try
            {
                if (!IncomingMessages.TryTake(out IMessage<T> message, (int)unit.ToMilliseconds(timeout), _tokenSource.Token))
                {
                    return null;
                }
                MessageProcessed(message);
                return BeforeConsume(message);
            }
            catch (Exception e)
            {
                HandlerState.State state = State.ConnectionState;
                if (state != HandlerState.State.Closing && state != HandlerState.State.Closed)
                {
                    Stats.IncrementNumReceiveFailed();
                    throw PulsarClientException.Unwrap(e);
                }
                else
                {
                    return null;
                }
            }
        }
		internal virtual void ReceiveIndividualMessagesFromBatch(MessageMetadata msgMetadata, int redeliveryCount, IList<long> ackSet, byte[] payload, MessageIdData messageId, IActorRef cnx)
		{
			int batchSize = msgMetadata.NumMessagesInBatch;
			var uncompressedPayload = new ReadOnlySequence<byte>(payload);
			// create ack tracker for entry aka batch
			var batchMessage = new MessageId((long)messageId.ledgerId, (long)messageId.entryId, PartitionIndex);
			IList<IMessage<T>> possibleToDeadLetter = null;
			if(_deadLetterPolicy != null && redeliveryCount >= _deadLetterPolicy.MaxRedeliverCount)
			{
				possibleToDeadLetter = new List<IMessage<T>>();
			}

			BatchMessageAcker acker = BatchMessageAcker.NewAcker(batchSize);
			BitSet ackBitSet = null;
			if(ackSet != null && ackSet.Count > 0)
			{
				ackBitSet = BitSet.ValueOf(ackSet.ToArray());
			}

			int skippedMessages = 0;
			try
			{
				long index = 0;
				for (int i = 0; i < batchSize; ++i)
				{
					var singleMetadataSize = uncompressedPayload.ReadUInt32(index, true);
					index += 4;
					if (_log.IsDebugEnabled)
					{
						_log.Debug($"[{Subscription}] [{ConsumerName}] processing message num - {i} in batch");
					}
					var singleMessageMetadata = Serializer.Deserialize<SingleMessageMetadata>(uncompressedPayload.Slice(index, singleMetadataSize));
					index += singleMetadataSize;
					var singleMessagePayload = uncompressedPayload.Slice(index, singleMessageMetadata.PayloadSize);
					if (IsSameEntry(messageId) && IsPriorBatchIndex(i))
					{
						// If we are receiving a batch message, we need to discard messages that were prior
						// to the startMessageId
						if(_log.IsDebugEnabled)
						{
							_log.Debug($"[{Subscription}] [{ConsumerName}] Ignoring message from before the startMessageId: {_startMessageId}");
						}
						++skippedMessages;
						continue;
					}

					if(singleMessageMetadata.CompactedOut)
					{
						++skippedMessages;
						continue;
					}

					var ackSetCount = ackSet?.Count ?? 0;
					var result = new byte[ackSetCount * sizeof(long)];
					var ack = ackSet?.ToArray() ?? new long[0];
					Buffer.BlockCopy(ack, 0, result, 0, result.Length);
					var bitArray = new BitArray(result);
					if (bitArray.Length > 0)
					{
						if (bitArray.Get(i))
						{
							++skippedMessages;
							continue;
						}
					}

					var batchMessageId = new BatchMessageId((long)messageId.ledgerId, (long)messageId.entryId, PartitionIndex, i, batchSize, acker);

					var message = new Message<T>(_topicName.ToString(), batchMessageId, msgMetadata, singleMessageMetadata, singleMessagePayload.ToArray(), CreateEncryptionContext(msgMetadata), cnx, Schema, redeliveryCount);
					if(possibleToDeadLetter != null)
					{
						possibleToDeadLetter.Add(message);
					}
					if (EnqueueMessageAndCheckBatchReceive(message))
					{
						//Due to the way actor works, there will not be any need for pending receives
						/*if (HasPendingBatchReceive())
						{
							NotifyPendingBatchReceivedCallBack();
						}*/
					}
					index += (uint)singleMessageMetadata.PayloadSize;
				}
				if(ackBitSet != null)
				{
					ackBitSet = null;
				}
			}
			catch(Exception)
			{
				_log.Warning($"[{Subscription}] [{ConsumerName}] unable to obtain message in batch");
				DiscardCorruptedMessage(messageId, cnx, CommandAck.ValidationError.BatchDeSerializeError);
			}

			if(possibleToDeadLetter != null && _possibleSendToDeadLetterTopicMessages != null)
			{
				_possibleSendToDeadLetterTopicMessages[batchMessage] = possibleToDeadLetter;
			}

			if(_log.IsDebugEnabled)
			{
				_log.Debug($"[{Subscription}] [{ConsumerName}] enqueued messages in batch. queue size - {IncomingMessages.Count}, available queue size - (-1)");
			}

			if(skippedMessages > 0)
			{
				IncreaseAvailablePermits(cnx, skippedMessages);
			}
		}
		private bool EnqueueMessageAndCheckBatchReceive(Message<T> message)
		{
			if (CanEnqueueMessage(message))
			{
				IncomingMessages.Add(message);
				IncomingMessagesSize += message.Data?.Length ?? 0;
			}
			return HasEnoughMessagesForBatchReceive();
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
			return _startMessageId != null && messageId.ledgerId == (ulong)_startMessageId.LedgerId && messageId.entryId == (ulong)_startMessageId.EntryId;
		}

		/// <summary>
		/// Record the event that one message has been processed by the application.
		/// 
		/// Periodically, it sends a Flow command to notify the broker that it can push more messages
		/// </summary>
		protected internal override void MessageProcessed<T1>(IMessage<T1> msg)
		{
			var currentCnx = Cnx();
			IActorRef msgCnx = ((Message<T1>)msg).Cnx();
			_lastDequeuedMessageId = msg.MessageId;

			if (msgCnx != currentCnx)
			{
				// The processed message did belong to the old queue that was cleared after reconnection.
				return;
			}

			IncreaseAvailablePermits(currentCnx);
			Stats.UpdateNumMsgsReceived(msg);

			TrackMessage(msg);
			IncomingMessagesSize -= (msg.Data == null ? 0 : msg.Data.Length);
		}

		protected internal virtual void TrackMessage<T1>(IMessage<T1> msg)
		{
			if(msg != null)
			{
				TrackMessage(msg.MessageId);
			}
		}

		protected internal virtual void TrackMessage(IMessageId messageId)
		{
			if(Conf.AckTimeoutMillis > 0)
			{
				MessageId id = null;
				if (messageId is BatchMessageId)
				{
					// do not add each item in batch message into tracker
					id = new MessageId(id.LedgerId, id.EntryId, PartitionIndex);
				}
				else
					id = (MessageId)messageId;

				if (_hasParentConsumer)
				{
					//TODO: check parent consumer here
					// we should no longer track this message, TopicsConsumer will take care from now onwards
					_unAckedMessageTracker.Tell(new Remove(id));
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
				if (_availablePermits == available)
				{
					_availablePermits = 0;
					SendFlowPermitsToBroker(currentCnx, available);
					break;
				}
				else
				{
					available = _availablePermits;
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
				return _connectionHandler.AskFor<long>(LastConnectionClosedTimestamp.Instance);
			}
		}

		private byte[] DecryptPayloadIfNeeded(MessageIdData messageId, MessageMetadata msgMetadata, ReadOnlySequence<byte> payload, IActorRef currentCnx)
		{

			if(msgMetadata.EncryptionKeys.Count == 0)
			{
				return payload.ToArray();
			}

			// If KeyReader is not configured throw exception based on config param
			if(Conf.CryptoKeyReader == null)
			{
				switch(Conf.CryptoFailureAction)
				{
					case ConsumerCryptoFailureAction.Consume:
						_log.Warning($"[{Topic}][{Subscription}][{ConsumerName}] CryptoKeyReader interface is not implemented. Consuming encrypted message.");
						return payload.ToArray();
					case ConsumerCryptoFailureAction.Discard:
						_log.Warning($"[{Topic}][{Subscription}][{ConsumerName}] Skipping decryption since CryptoKeyReader interface is not implemented and config is set to discard");
						DiscardMessage(messageId, currentCnx, CommandAck.ValidationError.DecryptionError);
						return null;
					case ConsumerCryptoFailureAction.Fail:
						MessageId m = new MessageId((long)messageId.ledgerId, (long)messageId.entryId, _partitionIndex);
						_log.Error($"[{Topic}][{Subscription}][{ConsumerName}][{m}] Message delivery failed since CryptoKeyReader interface is not implemented to consume encrypted message");
						_unAckedMessageTracker.Tell(new Add(m));
						return null;
				}
			}

			var decryptedData = _msgCrypto.Decrypt(msgMetadata, payload.ToArray(), Conf.CryptoKeyReader);
			if(decryptedData != null)
			{
				return decryptedData;
			}

			switch(Conf.CryptoFailureAction)
			{
				case ConsumerCryptoFailureAction.Consume:
					// Note, batch message will fail to consume even if config is set to consume
					_log.Warning($"[{Topic}][{Subscription}][{ConsumerName}][{messageId}] Decryption failed. Consuming encrypted message since config is set to consume.");
					return payload.ToArray();
				case ConsumerCryptoFailureAction.Discard:
					_log.Warning($"[{Topic}][{Subscription}][{ConsumerName}][{messageId}] Discarding message since decryption failed and config is set to discard");
					DiscardMessage(messageId, currentCnx, CommandAck.ValidationError.DecryptionError);
					return null;
				case ConsumerCryptoFailureAction.Fail:
					MessageId m = new MessageId((long)messageId.ledgerId, (long)messageId.entryId, _partitionIndex);
					_log.Error($"[{Topic}][{Subscription}][{ConsumerName}][{m}] Message delivery failed since unable to decrypt incoming message");
					_unAckedMessageTracker.Tell(new Add(m));
					return null;
			}
			return null;
		}

		private byte[] UncompressPayloadIfNeeded(MessageIdData messageId, MessageMetadata msgMetadata, byte[] payload, IActorRef currentCnx, bool checkMaxMessageSize)
		{
			var compressionType = msgMetadata.Compression;
			var codec = CompressionCodecProvider.GetCompressionCodec((int)compressionType);
			var uncompressedSize = (int)msgMetadata.UncompressedSize;
			var payloadSize = payload.Length;
			var maxMessageSize = currentCnx.AskFor<long>(MaxMessageSize.Instance);
			if (checkMaxMessageSize && payloadSize > maxMessageSize)
			{
				// payload size is itself corrupted since it cannot be bigger than the MaxMessageSize
				_log.Error($"[{Topic}][{Subscription}] Got corrupted payload message size {payloadSize} at {messageId}");
				DiscardCorruptedMessage(messageId, currentCnx, CommandAck.ValidationError.UncompressedSizeCorruption);
				return null;
			}
			try
			{
				var uncompressedPayload = codec.Decode(payload, uncompressedSize);
				return uncompressedPayload;
			}
			catch(Exception e)
			{
				_log.Error($"[{Topic}][{Subscription}] Failed to decompress message with {compressionType} at {messageId}: {e}");
				DiscardCorruptedMessage(messageId, currentCnx, CommandAck.ValidationError.DecompressionError);
				return null;
			}
		}


		private void DiscardCorruptedMessage(MessageIdData messageId, IActorRef currentCnx, CommandAck.ValidationError validationError)
		{
			_log.Error($"[{Topic}][{Subscription}] Discarding corrupted message at {messageId.ledgerId}:{messageId.entryId}");
			DiscardMessage(messageId, currentCnx, validationError);
		}

		private void DiscardMessage(MessageIdData messageId, IActorRef currentCnx, CommandAck.ValidationError validationError)
		{
			var cmd = Commands.NewAck(ConsumerId, (long)messageId.ledgerId, (long)messageId.entryId, null, CommandAck.AckType.Individual, validationError, new Dictionary<string, long>());
			currentCnx.Tell(new Payload(cmd, -1, "NewAck"));
			IncreaseAvailablePermits(currentCnx);
			Stats.IncrementNumReceiveFailed();
		}

		internal string HandlerName
		{
			get
			{
				return Subscription;
			}
		}

		internal override bool Connected
		{
			get
			{
				return ClientCnx != null && (State.ConnectionState == HandlerState.State.Ready);
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
				return _availablePermits;
			}
		}
		protected internal override IMessage<T> InternalReceive()
		{
			IMessage<T> message;
			try
			{
				message = IncomingMessages.Take();
				MessageProcessed(message);
				return BeforeConsume(message);
			}
			catch (Exception e)
			{
				Stats.IncrementNumReceiveFailed();
				throw PulsarClientException.Unwrap(e);
			}
		}
		internal override int NumMessagesInQueue()
		{
			return IncomingMessages.Count;
		}

		internal override void RedeliverUnacknowledgedMessages()
		{
			var cnx = Cnx();
			var protocolVersion = cnx.AskFor<int>(RemoteEndpointProtocolVersion.Instance);
			if(Connected && protocolVersion >= (int)ProtocolVersion.V2)
			{
				int currentSize = 0;
				currentSize = IncomingMessages.Count;
				IncomingMessages = new BlockingCollection<IMessage<T>>();
				IncomingMessagesSize = 0;
				_unAckedMessageTracker.Tell(new Clear());
				var cmd = Commands.NewRedeliverUnacknowledgedMessages(ConsumerId);
				var payload = new Payload(cmd, -1, "NewRedeliverUnacknowledgedMessages");
				cnx.Tell(payload);
				if(currentSize > 0)
				{
					IncreaseAvailablePermits(cnx, currentSize);
				}
				if(_log.IsDebugEnabled)
				{
					_log.Debug($"[{Subscription}] [{Topic}] [{ConsumerName}] Redeliver unacked messages and send {currentSize} permits");
				}
				return;
			}
			if(cnx == null || (State.ConnectionState == HandlerState.State.Connecting))
			{
				_log.Warning($"[{Self}] Client Connection needs to be established for redelivery of unacknowledged messages");
			}
			else
			{
				_log.Warning($"[{Self}] Reconnecting the client to redeliver the messages.");
				cnx.GracefulStop(TimeSpan.FromSeconds(5));
			}
		}

		internal virtual int ClearIncomingMessagesAndGetMessageNumber()
		{
			int messagesNumber = IncomingMessages.Count;
			IncomingMessages = new BlockingCollection<IMessage<T>>();
			IncomingMessagesSize = 0;
			_unAckedMessageTracker.Tell(Clear.Instance);
			return messagesNumber;
		}

		protected internal override void RedeliverUnacknowledgedMessages(ISet<IMessageId> messageIds)
		{
			if(messageIds.Count == 0)
			{
				return;
			}

			Condition.CheckArgument(messageIds.First() is MessageId);

			if(Conf.SubscriptionType != CommandSubscribe.SubType.Shared && Conf.SubscriptionType != CommandSubscribe.SubType.KeyShared)
			{
				// We cannot redeliver single messages if subscription type is not Shared
				RedeliverUnacknowledgedMessages();
				return;
			}
			var cnx = Cnx();
			var protocolversion = cnx.AskFor<int>(RemoteEndpointProtocolVersion.Instance);
			if (Connected && protocolversion >= (int)ProtocolVersion.V2)
			{
				int messagesFromQueue = RemoveExpiredMessagesFromQueue(messageIds);

				var batches = messageIds.PartitionMessageId(MaxRedeliverUnacknowledged);
				var builder = new MessageIdData();
				batches.ForEach(ids =>
				{
					IList<MessageIdData> messageIdDatas = ids.Where(messageId => !ProcessPossibleToDLQ(messageId)).Select(messageId =>
					{
						builder.Partition = messageId.PartitionIndex;
						builder.ledgerId = (ulong)messageId.LedgerId;
						builder.entryId = (ulong)messageId.EntryId;
						return builder;
					}).ToList();
					if(messageIdDatas.Count > 0)
					{
						var cmd = Commands.NewRedeliverUnacknowledgedMessages(ConsumerId, messageIdDatas);
						var payload = new Payload(cmd, -1, "NewRedeliverUnacknowledgedMessages");
						cnx.Tell(payload);
					}
				});
				if(messagesFromQueue > 0)
				{
					IncreaseAvailablePermits(cnx, messagesFromQueue);
				}
				if(_log.IsDebugEnabled)
				{
					_log.Debug($"[{Subscription}] [{Topic}] [{ConsumerName}] Redeliver unacked messages and increase {messagesFromQueue} permits");
				}
				return;
			}
			if(cnx == null || (State.ConnectionState == HandlerState.State.Connecting))
			{
				_log.Warning($"[{Self}] Client Connection needs to be established for redelivery of unacknowledged messages");
			}
			else
			{
				_log.Warning($"[{Self}] Reconnecting the client to redeliver the messages.");
				cnx.GracefulStop(TimeSpan.FromSeconds(5));
			}
		}

		private bool ProcessPossibleToDLQ(IMessageId messageId)
		{
			IList<IMessage<T>> deadLetterMessages = null;
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
						_deadLetterProducer = _client.AskFor<IActorRef>(new NewProducerActor<T>(Schema, _deadLetterPolicy.DeadLetterTopic, false, false));
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
						foreach(var message in deadLetterMessages)
						{
							TypedMessageBuilder<T> typedMessageBuilderNew = new TypedMessageBuilder<T>(_deadLetterProducer, Schema);
							typedMessageBuilderNew.Value(message.Value);
							typedMessageBuilderNew.Properties(message.Properties);
							typedMessageBuilderNew.Send();
						}
						Acknowledge(messageId);
						return true;
					}
					catch(Exception e)
					{
						_log.Error($"Send to dead letter topic exception with topic: {_deadLetterPolicy.DeadLetterTopic}, messageId: {messageId} => {e}");
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
					throw new PulsarClientException.AlreadyClosedException($"The consumer {ConsumerName} was already closed when seeking the subscription {Subscription} of the topic {_topicName} to the message {messageId}");
					
				}

				if (!Connected)
				{
					throw new PulsarClientException($"The client is not connected to the broker when seeking the subscription {Subscription} of the topic {_topicName} to the message {messageId}");
					
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
					IncomingMessages = new BlockingCollection<IMessage<T>>();
					IncomingMessagesSize = 0;
				}
				else
				{
					_log.Error($"[{Topic}][{Subscription}] Failed to reset subscription");
					throw new Exception($"Failed to seek the subscription {Subscription} of the topic {_topicName} to the message {messageId}");
					
				}
			}
			catch(Exception e)
			{
				throw PulsarClientException.Unwrap(e);
			}
		}

		internal override void Seek(long timestamp)
		{
			try
			{
				if (State.ConnectionState == HandlerState.State.Closing || State.ConnectionState == HandlerState.State.Closed)
				{
					_log.Error($"The consumer {ConsumerName} was already closed when seeking the subscription {Subscription} of the topic {_topicName} to the timestamp {timestamp:D}");
					throw  new Exception($"The consumer {ConsumerName} was already closed when seeking the subscription {Subscription} of the topic {_topicName} to the timestamp {timestamp:D}");
					
				}

				if (!Connected)
				{
					_log.Error($"The client is not connected to the broker when seeking the subscription {Subscription} of the topic {_topicName} to the timestamp {timestamp:D}");
					throw new Exception($"The client is not connected to the broker when seeking the subscription {Subscription} of the topic {_topicName} to the timestamp {timestamp:D}");
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
					IncomingMessages = new BlockingCollection<IMessage<T>>();
					IncomingMessagesSize = 0;
				}
                else
                {
					_log.Error($"[{Topic}][{Subscription}] Failed to reset subscription");
					throw new Exception($"Failed to seek the subscription {Subscription} of the topic {_topicName} to the timestamp {timestamp:D}");

				}
			}
			catch(Exception e)
			{
				throw PulsarClientException.Unwrap(e);
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
		protected IActorRef Cnx()
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
			_connectionHandler.Tell(new GrabCnx($"Create connection from consumer: {ConsumerName}"));
			Become(Connection);
		}
		private void Connection()
        {
			Receive<ConnectionOpened>(c => {
				ConnectionOpened(c.ClientCnx);
			});
			Receive<ConnectionFailed>(c => {
				ConnectionFailed(c.Exception);
			});
			Receive<Failure>(c => {
				_log.Error($"Connection to the server failed: {c.Exception}/{c.Timestamp}"); 
			});
			Receive<ConnectionAlreadySet>(_ => {
				Become(Ready);
			});
			ReceiveAny(a => Stash.Stash());
		}
		internal virtual string TopicNameWithoutPartition
		{
			get
			{
				return _topicNameWithoutPartition;
			}
		}

        public IStash Stash { get; set; }

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
		private void Push<T1>(BlockingCollection<T1> queue, T1 obj)
        {
			if (_hasParentConsumer)
				Sender.Tell(obj);
			else
				queue.Add(obj);
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