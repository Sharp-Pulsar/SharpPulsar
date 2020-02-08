using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Microsoft.Extensions.Logging;
using Pulsar.Common.Auth;
using SharpPulsar.Common.Compression;
using SharpPulsar.Common.Schema;
using SharpPulsar.Exception;
using SharpPulsar.Impl.Transaction;
using SharpPulsar.Util;

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
    using DotNetty.Buffers;
    using SharpPulsar.Api;
    using SharpPulsar.Common.Naming;
    using SharpPulsar.Impl.Conf;
    using SharpPulsar.Protocol;
    using SharpPulsar.Protocol.Proto;
    using System.Threading.Tasks;

    public class ConsumerImpl<T> : ConsumerBase<T>, IConnection
	{
		private const int MaxRedeliverUnacknowledged = 1000;

		internal readonly long ConsumerId;

		private static readonly AtomicIntegerFieldUpdater<ConsumerImpl> AVAILABLE_PERMITS_UPDATER = AtomicIntegerFieldUpdater.newUpdater(typeof(ConsumerImpl), "availablePermits");
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unused") private volatile int availablePermits = 0;
		private volatile int availablePermits = 0;

		protected internal volatile IMessageId LastDequeuedMessage = MessageIdFields.Earliest;
		private volatile IMessageId lastMessageIdInBroker = MessageIdFields.Earliest;

		private long subscribeTimeout;
		internal int PartitionIndex;
		private readonly bool hasParentConsumer;

		private readonly int receiverQueueRefillThreshold;

		private readonly ReadWriteLock @lock = new ReentrantReadWriteLock();

		public virtual UnAckedMessageTracker unack;
		private readonly AcknowledgmentsGroupingTracker acknowledgmentsGroupingTracker;
		private readonly NegativeAcksTracker negativeAcksTracker;

//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		protected internal readonly ConsumerStatsRecorder StatsConflict;
		private readonly int priorityLevel;
		private readonly SubscriptionMode subscriptionMode;
		private volatile BatchMessageIdImpl startMessageId;

		private readonly BatchMessageIdImpl initialStartMessageId;
		private readonly long startMessageRollbackDurationInSec;

		private volatile bool hasReachedEndOfTopic;

		private readonly MessageCrypto msgCrypto;

		private readonly IDictionary<string, string> metadata;

		private readonly bool readCompacted;
		private readonly bool resetIncludeHead;

		private readonly SubscriptionInitialPosition subscriptionInitialPosition;
		public  ConnectionHandler Handler;

		private readonly TopicName topicName;
		public string TopicNameWithoutPartition;

		private readonly IDictionary<MessageIdImpl, IList<MessageImpl<T>>> possibleSendToDeadLetterTopicMessages;

		private readonly DeadLetterPolicy deadLetterPolicy;

		private IProducer<T> deadLetterProducer;

		protected internal volatile bool Paused;

		private readonly bool createTopicIfDoesNotExist;

		public enum SubscriptionMode
		{
			// Make the subscription to be backed by a durable cursor that will retain messages and persist the current
			// position
			Durable,

			// Lightweight subscription mode that doesn't have a durable cursor associated
			NonDurable
		}

		internal static ConsumerImpl<T> NewConsumerImpl(PulsarClientImpl Client, string Topic, ConsumerConfigurationData<T> Conf, ScheduledThreadPoolExecutor ListenerExecutor, int PartitionIndex, bool HasParentConsumer, TaskCompletionSource<IConsumer<T>> SubscribeFuture, SubscriptionMode SubscriptionMode, IMessageId StartMessageId, ISchema<T> Schema, ConsumerInterceptors<T> Interceptors, bool CreateTopicIfDoesNotExist)
		{
			if (Conf.ReceiverQueueSize == 0)
			{
				return new ZeroQueueConsumerImpl<T>(Client, Topic, Conf, ListenerExecutor, PartitionIndex, HasParentConsumer, SubscribeFuture, SubscriptionMode, StartMessageId, Schema, Interceptors, CreateTopicIfDoesNotExist);
			}
			else
			{
				return new ConsumerImpl<T>(Client, Topic, Conf, ListenerExecutor, PartitionIndex, HasParentConsumer, SubscribeFuture, SubscriptionMode, StartMessageId, 0, Schema, Interceptors, CreateTopicIfDoesNotExist);
			}
		}

		public ConsumerImpl(PulsarClientImpl Client, string Topic, ConsumerConfigurationData<T> Conf, int PartitionIndex, bool HasParentConsumer, CompletableFuture<Consumer<T>> SubscribeFuture, SubscriptionMode SubscriptionMode, MessageId StartMessageId, long StartMessageRollbackDurationInSec, Schema<T> Schema, ConsumerInterceptors<T> Interceptors, bool CreateTopicIfDoesNotExist) : base(Client, Topic, Conf, Conf.ReceiverQueueSize, ListenerExecutor, SubscribeFuture, Schema, Interceptors)
		{
			this.ConsumerId = Client.newConsumerId();
			this.subscriptionMode = SubscriptionMode;
			this.startMessageId = StartMessageId != null ? new BatchMessageIdImpl((MessageIdImpl) StartMessageId) : null;
			this.LastDequeuedMessage = StartMessageId == null ? MessageIdFields.Earliest : StartMessageId;
			this.initialStartMessageId = this.startMessageId;
			this.startMessageRollbackDurationInSec = StartMessageRollbackDurationInSec;
			AVAILABLE_PERMITS_UPDATER.set(this, 0);
			this.subscribeTimeout = DateTimeHelper.CurrentUnixTimeMillis() + Client.Configuration.OperationTimeoutMs;
			this.PartitionIndex = PartitionIndex;
			this.hasParentConsumer = HasParentConsumer;
			this.receiverQueueRefillThreshold = Conf.ReceiverQueueSize / 2;
			this.priorityLevel = Conf.PriorityLevel;
			this.readCompacted = Conf.ReadCompacted;
			this.subscriptionInitialPosition = Conf.SubscriptionInitialPosition;
			this.negativeAcksTracker = new NegativeAcksTracker(this, Conf);
			this.resetIncludeHead = Conf.ResetIncludeHead;
			this.createTopicIfDoesNotExist = CreateTopicIfDoesNotExist;

			if (Client.Configuration.StatsIntervalSeconds > 0)
			{
				StatsConflict = new ConsumerStatsRecorderImpl(Client, Conf, this);
			}
			else
			{
				StatsConflict = ConsumerStatsDisabled.INSTANCE;
			}

			if (Conf.AckTimeoutMillis != 0)
			{
				if (Conf.TickDurationMillis > 0)
				{
					this.UnAckedMessageTracker = new UnAckedMessageTracker(Client, this, Conf.AckTimeoutMillis, Math.Min(Conf.TickDurationMillis, Conf.AckTimeoutMillis));
				}
				else
				{
					this.UnAckedMessageTracker = new UnAckedMessageTracker(Client, this, Conf.AckTimeoutMillis);
				}
			}
			else
			{
				this.UnAckedMessageTracker = UnAckedMessageTracker.UnackedMessageTrackerDisabled;
			}

			// Create msgCrypto if not created already
			if (Conf.CryptoKeyReader != null)
			{
				this.msgCrypto = new MessageCrypto(string.Format("[{0}] [{1}]", Topic, SubscriptionConflict), false);
			}
			else
			{
				this.msgCrypto = null;
			}

			if (Conf.Properties.Empty)
			{
				metadata = Collections.emptyMap();
			}
			else
			{
				metadata = Collections.unmodifiableMap(new Dictionary<>(Conf.Properties));
			}

			this.ConnectionHandler = new ConnectionHandler(this, new BackoffBuilder()
									.setInitialTime(Client.Configuration.InitialBackoffIntervalNanos, BAMCIS.Util.Concurrent.TimeUnit.NANOSECONDS).setMax(Client.Configuration.MaxBackoffIntervalNanos, BAMCIS.Util.Concurrent.TimeUnit.NANOSECONDS).setMandatoryStop(0, BAMCIS.Util.Concurrent.TimeUnit.MILLISECONDS).create(), this);

			this.topicName = TopicName.get(Topic);
			if (this.topicName.Persistent)
			{
				this.acknowledgmentsGroupingTracker = new PersistentAcknowledgmentsGroupingTracker(this, Conf, Client.eventLoopGroup());
			}
			else
			{
				this.acknowledgmentsGroupingTracker = NonPersistentAcknowledgmentGroupingTracker.Of();
			}

			if (Conf.DeadLetterPolicy != null)
			{
				possibleSendToDeadLetterTopicMessages = new ConcurrentDictionary<MessageIdImpl, IList<MessageImpl<T>>>();
				if (StringUtils.isNotBlank(Conf.DeadLetterPolicy.DeadLetterTopic))
				{
					this.deadLetterPolicy = DeadLetterPolicy.builder().maxRedeliverCount(Conf.DeadLetterPolicy.MaxRedeliverCount).deadLetterTopic(Conf.DeadLetterPolicy.DeadLetterTopic).build();
				}
				else
				{
					this.deadLetterPolicy = DeadLetterPolicy.builder().maxRedeliverCount(Conf.DeadLetterPolicy.MaxRedeliverCount).deadLetterTopic(string.Format("{0}-{1}-DLQ", Topic, SubscriptionConflict)).build();
				}
			}
			else
			{
				deadLetterPolicy = null;
				possibleSendToDeadLetterTopicMessages = null;
			}

			TopicNameWithoutPartition = topicName.PartitionedTopicName;

			GrabCnx();
		}



		public override CompletableFuture<Void> UnsubscribeAsync()
		{
			if (State == State.Closing || State == State.Closed)
			{
				return FutureUtil.failedFuture(new PulsarClientException.AlreadyClosedException("Consumer was already closed"));
			}
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.CompletableFuture<Void> unsubscribeFuture = new java.util.concurrent.CompletableFuture<>();
			CompletableFuture<Void> UnsubscribeFuture = new CompletableFuture<Void>();
			if (Connected)
			{
				State = State.Closing;
				long RequestId = ClientConflict.newRequestId();
				ByteBuf Unsubscribe = Commands.newUnsubscribe(ConsumerId, RequestId);
				ClientCnx Cnx = cnx();
				Cnx.sendRequestWithId(Unsubscribe, RequestId).thenRun(() =>
				{
				Cnx.removeConsumer(ConsumerId);
				UnAckedMessageTracker.Dispose();
				if (possibleSendToDeadLetterTopicMessages != null)
				{
					possibleSendToDeadLetterTopicMessages.Clear();
				}
				ClientConflict.cleanupConsumer(ConsumerImpl.this);
				Log.info("[{}][{}] Successfully unsubscribed from topic", Topic, SubscriptionConflict);
				State = State.Closed;
				UnsubscribeFuture.complete(null);
				}).exceptionally(e =>
				{
				Log.error("[{}][{}] Failed to unsubscribe: {}", Topic, SubscriptionConflict, e.Cause.Message);
				State = State.Ready;
				UnsubscribeFuture.completeExceptionally(PulsarClientException.wrap(e.Cause, string.Format("Failed to unsubscribe the subscription {0} of topic {1}", topicName.ToString(), SubscriptionConflict)));
				return null;
			});
			}
			else
			{
				UnsubscribeFuture.completeExceptionally(new PulsarClientException(string.Format("The client is not connected to the broker when unsubscribing the " + "subscription {0} of the topic {1}", SubscriptionConflict, topicName.ToString())));
			}
			return UnsubscribeFuture;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override protected SharpPulsar.api.Message<T> internalReceive() throws SharpPulsar.api.PulsarClientException
		public override Message<T> InternalReceive()
		{
			Message<T> Message;
			try
			{
				Message = IncomingMessages.take();
				MessageProcessed(Message);
				return BeforeConsume(Message);
			}
			catch (InterruptedException E)
			{
				StatsConflict.incrementNumReceiveFailed();
				throw PulsarClientException.unwrap(E);
			}
		}

		public override CompletableFuture<Message<T>> InternalReceiveAsync()
		{

			CompletableFuture<Message<T>> Result = new CompletableFuture<Message<T>>();
			Message<T> Message = null;
			try
			{
				@lock.writeLock().@lock();
				Message = IncomingMessages.poll(0, BAMCIS.Util.Concurrent.TimeUnit.MILLISECONDS);
				if (Message == null)
				{
					PendingReceives.add(Result);
				}
			}
			catch (InterruptedException E)
			{
				Thread.CurrentThread.Interrupt();
				Result.completeExceptionally(E);
			}
			finally
			{
				@lock.writeLock().unlock();
			}

			if (Message != null)
			{
				MessageProcessed(Message);
				Result.complete(BeforeConsume(Message));
			}

			return Result;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override protected SharpPulsar.api.Message<T> internalReceive(int timeout, java.util.concurrent.BAMCIS.Util.Concurrent.TimeUnit unit) throws SharpPulsar.api.PulsarClientException
		public override Message<T> InternalReceive(int Timeout, BAMCIS.Util.Concurrent.TimeUnit Unit)
		{
			Message<T> Message;
			try
			{
				Message = IncomingMessages.poll(Timeout, Unit);
				if (Message == null)
				{
					return null;
				}
				MessageProcessed(Message);
				return BeforeConsume(Message);
			}
			catch (InterruptedException E)
			{
				State State = State;
				if (State != State.Closing && State != State.Closed)
				{
					StatsConflict.incrementNumReceiveFailed();
					throw PulsarClientException.unwrap(E);
				}
				else
				{
					return null;
				}
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override protected SharpPulsar.api.Messages<T> internalBatchReceive() throws SharpPulsar.api.PulsarClientException
		public override Messages<T> InternalBatchReceive()
		{
			try
			{
				return InternalBatchReceiveAsync().get();
			}
			catch (Exception e) when (e is InterruptedException || e is ExecutionException)
			{
				State State = State;
				if (State != State.Closing && State != State.Closed)
				{
					StatsConflict.incrementNumBatchReceiveFailed();
					throw PulsarClientException.unwrap(e);
				}
				else
				{
					return null;
				}
			}
		}

		public override CompletableFuture<Messages<T>> InternalBatchReceiveAsync()
		{
			CompletableFuture<Messages<T>> Result = new CompletableFuture<Messages<T>>();
			try
			{
				@lock.writeLock().@lock();
				if (PendingBatchReceives == null)
				{
					PendingBatchReceives = Queues.newConcurrentLinkedQueue();
				}
				if (HasEnoughMessagesForBatchReceive())
				{
					MessagesImpl<T> Messages = NewMessagesImpl;
					Message<T> MsgPeeked = IncomingMessages.peek();
					while (MsgPeeked != null && Messages.canAdd(MsgPeeked))
					{
						Message<T> Msg = IncomingMessages.poll();
						if (Msg != null)
						{
							MessageProcessed(Msg);
							Message<T> InterceptMsg = BeforeConsume(Msg);
							Messages.add(InterceptMsg);
						}
						MsgPeeked = IncomingMessages.peek();
					}
					Result.complete(Messages);
				}
				else
				{
					PendingBatchReceives.add(OpBatchReceive.Of(Result));
				}
			}
			finally
			{
				@lock.writeLock().unlock();
			}
			return Result;
		}

		public virtual bool MarkAckForBatchMessage(BatchMessageIdImpl BatchMessageId, PulsarApi.CommandAck.AckType AckType, IDictionary<string, long> Properties)
		{
			bool IsAllMsgsAcked;
			if (AckType == PulsarApi.CommandAck.AckType.Individual)
			{
				IsAllMsgsAcked = BatchMessageId.ackIndividual();
			}
			else
			{
				IsAllMsgsAcked = BatchMessageId.ackCumulative();
			}
			int OutstandingAcks = 0;
			if (Log.DebugEnabled)
			{
				OutstandingAcks = BatchMessageId.OutstandingAcksInSameBatch;
			}

			int BatchSize = BatchMessageId.BatchSize;
			// all messages in this batch have been acked
			if (IsAllMsgsAcked)
			{
				if (Log.DebugEnabled)
				{
					Log.debug("[{}] [{}] can ack message to broker {}, acktype {}, cardinality {}, length {}", SubscriptionConflict, ConsumerNameConflict, BatchMessageId, AckType, OutstandingAcks, BatchSize);
				}
				return true;
			}
			else
			{
				if (PulsarApi.CommandAck.AckType.Cumulative == AckType && !BatchMessageId.Acker.PrevBatchCumulativelyAcked)
				{
					SendAcknowledge(BatchMessageId.prevBatchMessageId(), PulsarApi.CommandAck.AckType.Cumulative, Properties, null);
					BatchMessageId.Acker.PrevBatchCumulativelyAcked = true;
				}
				else
				{
					OnAcknowledge(BatchMessageId, null);
				}
				if (Log.DebugEnabled)
				{
					Log.debug("[{}] [{}] cannot ack message to broker {}, acktype {}, pending acks - {}", SubscriptionConflict, ConsumerNameConflict, BatchMessageId, AckType, OutstandingAcks);
				}
			}
			return false;
		}

		public override CompletableFuture<Void> DoAcknowledge(MessageId MessageId, PulsarApi.CommandAck.AckType AckType, IDictionary<string, long> Properties, TransactionImpl TxnImpl)
		{
			checkArgument(MessageId is MessageIdImpl);
			if (State != State.Ready && State != State.Connecting)
			{
				StatsConflict.incrementNumAcksFailed();
				PulsarClientException Exception = new PulsarClientException("Consumer not ready. State: " + State);
				if (PulsarApi.CommandAck.AckType.Individual.Equals(AckType))
				{
					OnAcknowledge(MessageId, Exception);
				}
				else if (PulsarApi.CommandAck.AckType.Cumulative.Equals(AckType))
				{
					OnAcknowledgeCumulative(MessageId, Exception);
				}
				return FutureUtil.failedFuture(Exception);
			}

			if (MessageId is BatchMessageIdImpl)
			{
				if (MarkAckForBatchMessage((BatchMessageIdImpl) MessageId, AckType, Properties))
				{
					// all messages in batch have been acked so broker can be acked via sendAcknowledge()
					if (Log.DebugEnabled)
					{
						Log.debug("[{}] [{}] acknowledging message - {}, acktype {}", SubscriptionConflict, ConsumerNameConflict, MessageId, AckType);
					}
				}
				else
				{
					// other messages in batch are still pending ack.
					return CompletableFuture.completedFuture(null);
				}
			}
			return SendAcknowledge(MessageId, AckType, Properties, TxnImpl);
		}

		// TODO: handle transactional acknowledgements.
		private CompletableFuture<Void> SendAcknowledge(MessageId MessageId, PulsarApi.CommandAck.AckType AckType, IDictionary<string, long> Properties, TransactionImpl TxnImpl)
		{
			MessageIdImpl MsgId = (MessageIdImpl) MessageId;

			if (AckType == PulsarApi.CommandAck.AckType.Individual)
			{
				if (MessageId is BatchMessageIdImpl)
				{
					BatchMessageIdImpl BatchMessageId = (BatchMessageIdImpl) MessageId;

					StatsConflict.incrementNumAcksSent(BatchMessageId.BatchSize);
					UnAckedMessageTracker.Remove(new MessageIdImpl(BatchMessageId.LedgerId, BatchMessageId.EntryId, BatchMessageId.PartitionIndex));
					if (possibleSendToDeadLetterTopicMessages != null)
					{
						possibleSendToDeadLetterTopicMessages.Remove(new MessageIdImpl(BatchMessageId.LedgerId, BatchMessageId.EntryId, BatchMessageId.PartitionIndex));
					}
				}
				else
				{
					// increment counter by 1 for non-batch msg
					UnAckedMessageTracker.Remove(MsgId);
					if (possibleSendToDeadLetterTopicMessages != null)
					{
						possibleSendToDeadLetterTopicMessages.Remove(MsgId);
					}
					StatsConflict.incrementNumAcksSent(1);
				}
				OnAcknowledge(MessageId, null);
			}
			else if (AckType == PulsarApi.CommandAck.AckType.Cumulative)
			{
				OnAcknowledgeCumulative(MessageId, null);
				StatsConflict.incrementNumAcksSent(UnAckedMessageTracker.RemoveMessagesTill(MsgId));
			}

			acknowledgmentsGroupingTracker.AddAcknowledgment(MsgId, AckType, Properties);

			// Consumer acknowledgment operation immediately succeeds. In any case, if we're not able to send ack to broker,
			// the messages will be re-delivered
			return CompletableFuture.completedFuture(null);
		}

		public override void NegativeAcknowledge(MessageId MessageId)
		{
			negativeAcksTracker.Add(MessageId);

			// Ensure the message is not redelivered for ack-timeout, since we did receive an "ack"
			UnAckedMessageTracker.Remove(MessageId);
		}

		public override void ConnectionOpened(in ClientCnx Cnx)
		{
			ClientCnx = Cnx;
			Cnx.registerConsumer(ConsumerId, this);

			Log.info("[{}][{}] Subscribing to topic on cnx {}", Topic, SubscriptionConflict, Cnx.ctx().channel());

			long RequestId = ClientConflict.newRequestId();

			int CurrentSize;
			lock (this)
			{
				CurrentSize = IncomingMessages.size();
				startMessageId = ClearReceiverQueue();
				if (possibleSendToDeadLetterTopicMessages != null)
				{
					possibleSendToDeadLetterTopicMessages.Clear();
				}
			}

			bool IsDurable = subscriptionMode == SubscriptionMode.Durable;
			PulsarApi.MessageIdData StartMessageIdData;
			if (IsDurable)
			{
				// For regular durable subscriptions, the message id from where to restart will be determined by the broker.
				StartMessageIdData = null;
			}
			else
			{
				// For non-durable we are going to restart from the next entry
				PulsarApi.MessageIdData.Builder Builder = PulsarApi.MessageIdData.newBuilder();
				Builder.LedgerId = startMessageId.LedgerId;
				Builder.EntryId = startMessageId.EntryId;
				if (startMessageId is BatchMessageIdImpl)
				{
					Builder.BatchIndex = ((BatchMessageIdImpl) startMessageId).BatchIndex;
				}

				StartMessageIdData = Builder.build();
				Builder.recycle();
			}

			SchemaInfo Si = Schema.SchemaInfo;
			if (Si != null && (SchemaType.BYTES == Si.Type || SchemaType.NONE == Si.Type))
			{
				// don't set schema for Schema.BYTES
				Si = null;
			}
			// startMessageRollbackDurationInSec should be consider only once when consumer connects to first time
			long StartMessageRollbackDuration = (startMessageRollbackDurationInSec > 0 && startMessageId.Equals(initialStartMessageId)) ? startMessageRollbackDurationInSec : 0;
			ByteBuf Request = Commands.newSubscribe(Topic, SubscriptionConflict, ConsumerId, RequestId, SubType, priorityLevel, ConsumerNameConflict, IsDurable, StartMessageIdData, metadata, readCompacted, Conf.ReplicateSubscriptionState, PulsarApi.CommandSubscribe.InitialPosition.valueOf(subscriptionInitialPosition.Value), StartMessageRollbackDuration, Si, createTopicIfDoesNotExist, Conf.KeySharedPolicy);
			if (StartMessageIdData != null)
			{
				StartMessageIdData.recycle();
			}

			Cnx.sendRequestWithId(Request, RequestId).thenRun(() =>
			{
			lock (this)
			{
				if (ChangeToReadyState())
				{
					ConsumerIsReconnectedToBroker(Cnx, CurrentSize);
				}
				else
				{
					State = State.Closed;
					Cnx.removeConsumer(ConsumerId);
					Cnx.channel().close();
					return;
				}
			}
			ResetBackoff();
			bool FirstTimeConnect = SubscribeFutureConflict.complete(this);
			if (!(FirstTimeConnect && hasParentConsumer && IsDurable) && Conf.ReceiverQueueSize != 0)
			{
				SendFlowPermitsToBroker(Cnx, Conf.ReceiverQueueSize);
			}
			}).exceptionally((e) =>
			{
			Cnx.removeConsumer(ConsumerId);
			if (State == State.Closing || State == State.Closed)
			{
				Cnx.channel().close();
				return null;
			}
			Log.warn("[{}][{}] Failed to subscribe to topic on {}", Topic, SubscriptionConflict, Cnx.channel().remoteAddress());
			if (e.Cause is PulsarClientException && ConnectionHandler.isRetriableError((PulsarClientException) e.Cause) && DateTimeHelper.CurrentUnixTimeMillis() < subscribeTimeout)
			{
				ReconnectLater(e.Cause);
			}
			else if (!SubscribeFutureConflict.Done)
			{
				State = State.Failed;
				CloseConsumerTasks();
				SubscribeFutureConflict.completeExceptionally(PulsarClientException.wrap(e, string.Format("Failed to subscribe the topic {0} with subscription " + "name {1} when connecting to the broker", topicName.ToString(), SubscriptionConflict)));
				ClientConflict.cleanupConsumer(this);
			}
			else if (e.Cause is PulsarClientException.TopicDoesNotExistException)
			{
				State = State.Failed;
				ClientConflict.cleanupConsumer(this);
				Log.warn("[{}][{}] Closed consumer because topic does not exist anymore {}", Topic, SubscriptionConflict, Cnx.channel().remoteAddress());
			}
			else
			{
				ReconnectLater(e.Cause);
			}
			return null;
		});
		}

		public virtual void ConsumerIsReconnectedToBroker(ClientCnx Cnx, int CurrentQueueSize)
		{
			Log.info("[{}][{}] Subscribed to topic on {} -- consumer: {}", Topic, SubscriptionConflict, Cnx.channel().remoteAddress(), ConsumerId);

			AVAILABLE_PERMITS_UPDATER.set(this, 0);
		}

		/// <summary>
		/// Clear the internal receiver queue and returns the message id of what was the 1st message in the queue that was
		/// not seen by the application
		/// </summary>
		private BatchMessageIdImpl ClearReceiverQueue()
		{
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.List<SharpPulsar.api.Message<?>> currentMessageQueue = new java.util.ArrayList<>(incomingMessages.size());
			IList<Message<object>> CurrentMessageQueue = new List<Message<object>>(IncomingMessages.size());
			IncomingMessages.drainTo(CurrentMessageQueue);
			IncomingMessagesSizeUpdater.set(this, 0);
			if (CurrentMessageQueue.Count > 0)
			{
				MessageIdImpl NextMessageInQueue = (MessageIdImpl) CurrentMessageQueue[0].MessageId;
				BatchMessageIdImpl PreviousMessage;
				if (NextMessageInQueue is BatchMessageIdImpl)
				{
					// Get on the previous message within the current batch
					PreviousMessage = new BatchMessageIdImpl(NextMessageInQueue.LedgerId, NextMessageInQueue.EntryId, NextMessageInQueue.PartitionIndex, ((BatchMessageIdImpl) NextMessageInQueue).BatchIndex - 1);
				}
				else
				{
					// Get on previous message in previous entry
					PreviousMessage = new BatchMessageIdImpl(NextMessageInQueue.LedgerId, NextMessageInQueue.EntryId - 1, NextMessageInQueue.PartitionIndex, -1);
				}

				return PreviousMessage;
			}
			else if (!LastDequeuedMessage.Equals(MessageIdFields.Earliest))
			{
				// If the queue was empty we need to restart from the message just after the last one that has been dequeued
				// in the past
				return new BatchMessageIdImpl((MessageIdImpl) LastDequeuedMessage);
			}
			else
			{
				// No message was received or dequeued by this consumer. Next message would still be the startMessageId
				return startMessageId;
			}
		}

		/// <summary>
		/// send the flow command to have the broker start pushing messages
		/// </summary>
		public virtual void SendFlowPermitsToBroker(ClientCnx Cnx, int NumMessages)
		{
			if (Cnx != null)
			{
				if (Log.DebugEnabled)
				{
					Log.debug("[{}] [{}] Adding {} additional permits", Topic, SubscriptionConflict, NumMessages);
				}

				Cnx.ctx().writeAndFlush(Commands.newFlow(ConsumerId, NumMessages), Cnx.ctx().voidPromise());
			}
		}

		public override void ConnectionFailed(PulsarClientException Exception)
		{
			if (DateTimeHelper.CurrentUnixTimeMillis() > subscribeTimeout && SubscribeFutureConflict.completeExceptionally(Exception))
			{
				State = State.Failed;
				Log.info("[{}] Consumer creation failed for consumer {}", Topic, ConsumerId);
				ClientConflict.cleanupConsumer(this);
			}
		}

		public override CompletableFuture<Void> CloseAsync()
		{
			if (State == State.Closing || State == State.Closed)
			{
				CloseConsumerTasks();
				return CompletableFuture.completedFuture(null);
			}

			if (!Connected)
			{
				Log.info("[{}] [{}] Closed Consumer (not connected)", Topic, SubscriptionConflict);
				State = State.Closed;
				CloseConsumerTasks();
				ClientConflict.cleanupConsumer(this);
				return CompletableFuture.completedFuture(null);
			}

			StatsConflict.StatTimeout.ifPresent(Timeout.cancel);

			State = State.Closing;

			CloseConsumerTasks();

			long RequestId = ClientConflict.newRequestId();

			CompletableFuture<Void> CloseFuture = new CompletableFuture<Void>();
			ClientCnx Cnx = cnx();
			if (null == Cnx)
			{
				CleanupAtClose(CloseFuture);
			}
			else
			{
				ByteBuf Cmd = Commands.newCloseConsumer(ConsumerId, RequestId);
				Cnx.sendRequestWithId(Cmd, RequestId).handle((v, exception) =>
				{
				Cnx.removeConsumer(ConsumerId);
				if (exception == null || !Cnx.ctx().channel().Active)
				{
					CleanupAtClose(CloseFuture);
				}
				else
				{
					CloseFuture.completeExceptionally(exception);
				}
				return null;
				});
			}

			return CloseFuture;
		}

		private void CleanupAtClose(CompletableFuture<Void> CloseFuture)
		{
			Log.info("[{}] [{}] Closed consumer", Topic, SubscriptionConflict);
			State = State.Closed;
			CloseConsumerTasks();
			CloseFuture.complete(null);
			ClientConflict.cleanupConsumer(this);
			// fail all pending-receive futures to notify application
			FailPendingReceive();
		}

		private void CloseConsumerTasks()
		{
			UnAckedMessageTracker.Dispose();
			if (possibleSendToDeadLetterTopicMessages != null)
			{
				possibleSendToDeadLetterTopicMessages.Clear();
			}

			acknowledgmentsGroupingTracker.Close();
		}

		private void FailPendingReceive()
		{
			@lock.readLock().@lock();
			try
			{
				if (ListenerExecutor != null && !ListenerExecutor.Shutdown)
				{
					while (!PendingReceives.Empty)
					{
						CompletableFuture<Message<T>> ReceiveFuture = PendingReceives.poll();
						if (ReceiveFuture != null)
						{
							ReceiveFuture.completeExceptionally(new PulsarClientException.AlreadyClosedException(string.Format("The consumer which subscribes the topic {0} with subscription name {1} " + "was already closed when cleaning and closing the consumers", topicName.ToString(), SubscriptionConflict)));
						}
						else
						{
							break;
						}
					}
				}
			}
			finally
			{
				@lock.readLock().unlock();
			}
		}

		public virtual void ActiveConsumerChanged(bool IsActive)
		{
			if (ConsumerEventListener == null)
			{
				return;
			}

			ListenerExecutor.execute(() =>
			{
			if (IsActive)
			{
				ConsumerEventListener.becameActive(this, PartitionIndex);
			}
			else
			{
				ConsumerEventListener.becameInactive(this, PartitionIndex);
			}
			});
		}

		public virtual void MessageReceived(MessageIdData MessageId, int RedeliveryCount, IByteBuffer HeadersAndPayload, ClientCnx Cnx)
		{
			if (Log.DebugEnabled)
			{
				Log.debug("[{}][{}] Received message: {}/{}", Topic, SubscriptionConflict, MessageId.LedgerId, MessageId.EntryId);
			}

			if (!VerifyChecksum(HeadersAndPayload, MessageId))
			{
				// discard message with checksum error
				DiscardCorruptedMessage(MessageId, Cnx, CommandAck.ValidationError.ChecksumMismatch);
				return;
			}

			PulsarApi.MessageMetadata MsgMetadata;
			try
			{
				MsgMetadata = Commands.parseMessageMetadata(HeadersAndPayload);
			}
			catch (Exception)
			{
				DiscardCorruptedMessage(MessageId, Cnx, PulsarApi.CommandAck.ValidationError.ChecksumMismatch);
				return;
			}

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int numMessages = msgMetadata.getNumMessagesInBatch();
			int NumMessages = MsgMetadata.NumMessagesInBatch;

			MessageIdImpl MsgId = new MessageIdImpl(MessageId.LedgerId, MessageId.EntryId, PartitionIndex);
			if (acknowledgmentsGroupingTracker.IsDuplicate(MsgId))
			{
				if (Log.DebugEnabled)
				{
					Log.debug("[{}] [{}] Ignoring message as it was already being acked earlier by same consumer {}/{}", Topic, SubscriptionConflict, ConsumerNameConflict, MsgId);
				}

				IncreaseAvailablePermits(Cnx, NumMessages);
				return;
			}

			ByteBuf DecryptedPayload = DecryptPayloadIfNeeded(MessageId, MsgMetadata, HeadersAndPayload, Cnx);

			bool IsMessageUndecryptable = isMessageUndecryptable(MsgMetadata);

			if (DecryptedPayload == null)
			{
				// Message was discarded or CryptoKeyReader isn't implemented
				return;
			}

			// uncompress decryptedPayload and release decryptedPayload-ByteBuf
			ByteBuf UncompressedPayload = IsMessageUndecryptable ? DecryptedPayload.retain() : UncompressPayloadIfNeeded(MessageId, MsgMetadata, DecryptedPayload, Cnx);
			DecryptedPayload.release();
			if (UncompressedPayload == null)
			{
				// Message was discarded on decompression error
				return;
			}

			// if message is not decryptable then it can't be parsed as a batch-message. so, add EncyrptionCtx to message
			// and return undecrypted payload
			if (IsMessageUndecryptable || (NumMessages == 1 && !MsgMetadata.hasNumMessagesInBatch()))
			{

				if (IsResetIncludedAndSameEntryLedger(MessageId) && IsPriorEntryIndex(MessageId.EntryId))
				{
					// We need to discard entries that were prior to startMessageId
					if (Log.DebugEnabled)
					{
						Log.debug("[{}] [{}] Ignoring message from before the startMessageId: {}", SubscriptionConflict, ConsumerNameConflict, startMessageId);
					}

					UncompressedPayload.release();
					MsgMetadata.recycle();
					return;
				}

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final MessageImpl<T> message = new MessageImpl<>(topicName.toString(), msgId, msgMetadata, uncompressedPayload, createEncryptionContext(msgMetadata), cnx, schema, redeliveryCount);
				MessageImpl<T> Message = new MessageImpl<T>(topicName.ToString(), MsgId, MsgMetadata, UncompressedPayload, CreateEncryptionContext(MsgMetadata), Cnx, Schema, RedeliveryCount);
				UncompressedPayload.release();
				MsgMetadata.recycle();

				@lock.readLock().@lock();
				try
				{
					// Enqueue the message so that it can be retrieved when application calls receive()
					// if the conf.getReceiverQueueSize() is 0 then discard message if no one is waiting for it.
					// if asyncReceive is waiting then notify callback without adding to incomingMessages queue
					if (deadLetterPolicy != null && possibleSendToDeadLetterTopicMessages != null && RedeliveryCount >= deadLetterPolicy.MaxRedeliverCount)
					{
						possibleSendToDeadLetterTopicMessages[(MessageIdImpl)Message.getMessageId()] = Collections.singletonList(Message);
					}
					if (!PendingReceives.Empty)
					{
						NotifyPendingReceivedCallback(Message, null);
					}
					else if (EnqueueMessageAndCheckBatchReceive(Message))
					{
						if (HasPendingBatchReceive())
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
				ReceiveIndividualMessagesFromBatch(MsgMetadata, RedeliveryCount, UncompressedPayload, MessageId, Cnx);

				UncompressedPayload.release();
				MsgMetadata.recycle();
			}

			if (Listener != null)
			{
				TriggerListener(NumMessages);
			}
		}

		public virtual void TriggerListener(int NumMessages)
		{
			// Trigger the notification on the message listener in a separate thread to avoid blocking the networking
			// thread while the message processing happens
			ListenerExecutor.execute(() =>
			{
			for (int I = 0; I < NumMessages; I++)
			{
				try
				{
					Message<T> Msg = InternalReceive(0, BAMCIS.Util.Concurrent.TimeUnit.MILLISECONDS);
					if (Msg == null)
					{
						if (Log.DebugEnabled)
						{
							Log.debug("[{}] [{}] Message has been cleared from the queue", Topic, SubscriptionConflict);
						}
						break;
					}
					try
					{
						if (Log.DebugEnabled)
						{
							Log.debug("[{}][{}] Calling message listener for message {}", Topic, SubscriptionConflict, Msg.MessageId);
						}
						Listener.received(ConsumerImpl.this, Msg);
					}
					catch (Exception T)
					{
						Log.error("[{}][{}] Message listener error in processing message: {}", Topic, SubscriptionConflict, Msg.MessageId, T);
					}
				}
				catch (PulsarClientException E)
				{
					Log.warn("[{}] [{}] Failed to dequeue the message for listener", Topic, SubscriptionConflict, E);
					return;
				}
			}
			});
		}

		/// <summary>
		/// Notify waiting asyncReceive request with the received message
		/// </summary>
		/// <param name="message"> </param>
		public virtual void NotifyPendingReceivedCallback(in Message<T> Message, Exception Exception)
		{
			if (PendingReceives.Empty)
			{
				return;
			}

			// fetch receivedCallback from queue
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.CompletableFuture<SharpPulsar.api.Message<T>> receivedFuture = pendingReceives.poll();
			CompletableFuture<Message<T>> ReceivedFuture = PendingReceives.poll();
			if (ReceivedFuture == null)
			{
				return;
			}

			if (Exception != null)
			{
				ListenerExecutor.execute(() => ReceivedFuture.completeExceptionally(Exception));
				return;
			}

			if (Message == null)
			{
                InvalidOperationException E = new InvalidOperationException("received message can't be null");
				ListenerExecutor.execute(() => ReceivedFuture.completeExceptionally(E));
				return;
			}

			if (Conf.ReceiverQueueSize == 0)
			{
				// call interceptor and complete received callback
				InterceptAndComplete(Message, ReceivedFuture);
				return;
			}

			// increase permits for available message-queue
			MessageProcessed(Message);
			// call interceptor and complete received callback
			InterceptAndComplete(Message, ReceivedFuture);
		}

		private void InterceptAndComplete(in Message<T> Message, in CompletableFuture<Message<T>> ReceivedFuture)
		{
			// call proper interceptor
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final SharpPulsar.api.Message<T> interceptMessage = beforeConsume(message);
			Message<T> InterceptMessage = BeforeConsume(Message);
			// return message to receivedCallback
			ListenerExecutor.execute(() => ReceivedFuture.complete(InterceptMessage));
		}

		public virtual void ReceiveIndividualMessagesFromBatch(PulsarApi.MessageMetadata MsgMetadata, int RedeliveryCount, ByteBuf UncompressedPayload, PulsarApi.MessageIdData MessageId, ClientCnx Cnx)
		{
			int BatchSize = MsgMetadata.NumMessagesInBatch;

			// create ack tracker for entry aka batch
			MessageIdImpl BatchMessage = new MessageIdImpl(MessageId.LedgerId, MessageId.EntryId, PartitionIndex);
			BatchMessageAcker Acker = BatchMessageAcker.NewAcker(BatchSize);
			IList<MessageImpl<T>> PossibleToDeadLetter = null;
			if (deadLetterPolicy != null && RedeliveryCount >= deadLetterPolicy.MaxRedeliverCount)
			{
				PossibleToDeadLetter = new List<MessageImpl<T>>();
			}
			int SkippedMessages = 0;
			try
			{
				for (int I = 0; I < BatchSize; ++I)
				{
					if (Log.DebugEnabled)
					{
						Log.debug("[{}] [{}] processing message num - {} in batch", SubscriptionConflict, ConsumerNameConflict, I);
					}
					PulsarApi.SingleMessageMetadata.Builder SingleMessageMetadataBuilder = PulsarApi.SingleMessageMetadata.newBuilder();
					ByteBuf SingleMessagePayload = Commands.deSerializeSingleMessageInBatch(UncompressedPayload, SingleMessageMetadataBuilder, i, BatchSize);

					if (IsResetIncludedAndSameEntryLedger(MessageId) && IsPriorBatchIndex(i))
					{
						// If we are receiving a batch message, we need to discard messages that were prior
						// to the startMessageId
						if (Log.DebugEnabled)
						{
							Log.debug("[{}] [{}] Ignoring message from before the startMessageId: {}", SubscriptionConflict, ConsumerNameConflict, startMessageId);
						}
						SingleMessagePayload.release();
						SingleMessageMetadataBuilder.recycle();

						++SkippedMessages;
						continue;
					}

					if (SingleMessageMetadataBuilder.CompactedOut)
					{
						// message has been compacted out, so don't send to the user
						SingleMessagePayload.release();
						SingleMessageMetadataBuilder.recycle();

						++SkippedMessages;
						continue;
					}

					BatchMessageIdImpl BatchMessageIdImpl = new BatchMessageIdImpl(MessageId.LedgerId, MessageId.EntryId, PartitionIndex, i, Acker);
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final MessageImpl<T> message = new MessageImpl<>(topicName.toString(), batchMessageIdImpl, msgMetadata, singleMessageMetadataBuilder.build(), singleMessagePayload, createEncryptionContext(msgMetadata), cnx, schema, redeliveryCount);
					MessageImpl<T> Message = new MessageImpl<T>(topicName.ToString(), BatchMessageIdImpl, MsgMetadata, SingleMessageMetadataBuilder.build(), SingleMessagePayload, CreateEncryptionContext(MsgMetadata), Cnx, Schema, RedeliveryCount);
					if (PossibleToDeadLetter != null)
					{
						PossibleToDeadLetter.Add(Message);
					}
					@lock.readLock().@lock();
					try
					{
						if (!PendingReceives.Empty)
						{
							NotifyPendingReceivedCallback(Message, null);
						}
						else if (EnqueueMessageAndCheckBatchReceive(Message))
						{
							if (HasPendingBatchReceive())
							{
								NotifyPendingBatchReceivedCallBack();
							}
						}
					}
					finally
					{
						@lock.readLock().unlock();
					}
					SingleMessagePayload.release();
					SingleMessageMetadataBuilder.recycle();
				}
			}
			catch (IOException)
			{
				Log.warn("[{}] [{}] unable to obtain message in batch", SubscriptionConflict, ConsumerNameConflict);
				DiscardCorruptedMessage(MessageId, Cnx, PulsarApi.CommandAck.ValidationError.BatchDeSerializeError);
			}

			if (PossibleToDeadLetter != null && possibleSendToDeadLetterTopicMessages != null)
			{
				possibleSendToDeadLetterTopicMessages[BatchMessage] = PossibleToDeadLetter;
			}

			if (Log.DebugEnabled)
			{
				Log.debug("[{}] [{}] enqueued messages in batch. queue size - {}, available queue size - {}", SubscriptionConflict, ConsumerNameConflict, IncomingMessages.size(), IncomingMessages.remainingCapacity());
			}

			if (SkippedMessages > 0)
			{
				IncreaseAvailablePermits(Cnx, SkippedMessages);
			}
		}

		private bool IsPriorEntryIndex(long Idx)
		{
			return resetIncludeHead ? Idx < startMessageId.EntryId : Idx <= startMessageId.EntryId;
		}

		private bool IsPriorBatchIndex(long Idx)
		{
			return resetIncludeHead ? Idx < startMessageId.BatchIndex : Idx <= startMessageId.BatchIndex;
		}

		private bool IsResetIncludedAndSameEntryLedger(PulsarApi.MessageIdData MessageId)
		{
			return !resetIncludeHead && startMessageId != null && MessageId.LedgerId == startMessageId.LedgerId && MessageId.EntryId == startMessageId.EntryId;
		}

		/// <summary>
		/// Record the event that one message has been processed by the application.
		/// 
		/// Periodically, it sends a Flow command to notify the broker that it can push more messages
		/// </summary>
		public virtual void MessageProcessed<T1>(Message<T1> Msg)
		{
			lock (this)
			{
				ClientCnx CurrentCnx = Cnx();
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: ClientCnx msgCnx = ((MessageImpl<?>) msg).getCnx();
				ClientCnx MsgCnx = ((MessageImpl<object>) Msg).Cnx;
				LastDequeuedMessage = Msg.MessageId;
        
				if (MsgCnx != CurrentCnx)
				{
					// The processed message did belong to the old queue that was cleared after reconnection.
					return;
				}
        
				IncreaseAvailablePermits(CurrentCnx);
				StatsConflict.updateNumMsgsReceived(Msg);
        
				TrackMessage(Msg);
				IncomingMessagesSizeUpdater.addAndGet(this, -Msg.Data.Length);
			}
		}

		public virtual void TrackMessage<T1>(Message<T1> Msg)
		{
			if (Msg != null)
			{
				MessageId MessageId = Msg.MessageId;
				if (Conf.AckTimeoutMillis > 0 && MessageId is MessageIdImpl)
				{
					MessageIdImpl Id = (MessageIdImpl)MessageId;
					if (Id is BatchMessageIdImpl)
					{
						// do not add each item in batch message into tracker
						Id = new MessageIdImpl(Id.LedgerId, Id.EntryId, PartitionIndex);
					}
					if (hasParentConsumer)
					{
						// we should no longer track this message, TopicsConsumer will take care from now onwards
						UnAckedMessageTracker.Remove(Id);
					}
					else
					{
						UnAckedMessageTracker.Add(Id);
					}
				}
			}
		}

		public virtual void IncreaseAvailablePermits(ClientCnx CurrentCnx)
		{
			IncreaseAvailablePermits(CurrentCnx, 1);
		}

		private void IncreaseAvailablePermits(ClientCnx CurrentCnx, int Delta)
		{
			int Available = AVAILABLE_PERMITS_UPDATER.addAndGet(this, Delta);

			while (Available >= receiverQueueRefillThreshold && !Paused)
			{
				if (AVAILABLE_PERMITS_UPDATER.compareAndSet(this, Available, 0))
				{
					SendFlowPermitsToBroker(CurrentCnx, Available);
					break;
				}
				else
				{
					Available = AVAILABLE_PERMITS_UPDATER.get(this);
				}
			}
		}

		public override void Pause()
		{
			Paused = true;
		}

		public override void Resume()
		{
			if (Paused)
			{
				Paused = false;
				IncreaseAvailablePermits(Cnx(), 0);
			}
		}

		private ByteBuf DecryptPayloadIfNeeded(PulsarApi.MessageIdData MessageId, PulsarApi.MessageMetadata MsgMetadata, ByteBuf Payload, ClientCnx CurrentCnx)
		{

			if (MsgMetadata.EncryptionKeysCount == 0)
			{
				return Payload.retain();
			}

			// If KeyReader is not configured throw exception based on config param
			if (Conf.CryptoKeyReader == null)
			{
				switch (Conf.CryptoFailureAction)
				{
					case CONSUME:
						Log.warn("[{}][{}][{}] CryptoKeyReader interface is not implemented. Consuming encrypted message.", Topic, SubscriptionConflict, ConsumerNameConflict);
						return Payload.retain();
					case DISCARD:
						Log.warn("[{}][{}][{}] Skipping decryption since CryptoKeyReader interface is not implemented and config is set to discard", Topic, SubscriptionConflict, ConsumerNameConflict);
						DiscardMessage(MessageId, CurrentCnx, PulsarApi.CommandAck.ValidationError.DecryptionError);
						return null;
					case FAIL:
						MessageId M = new MessageIdImpl(MessageId.LedgerId, MessageId.EntryId, PartitionIndex);
						Log.error("[{}][{}][{}][{}] Message delivery failed since CryptoKeyReader interface is not implemented to consume encrypted message", Topic, SubscriptionConflict, ConsumerNameConflict, M);
						UnAckedMessageTracker.Add(M);
						return null;
				}
			}

			ByteBuf DecryptedData = this.msgCrypto.Decrypt(MsgMetadata, Payload, Conf.CryptoKeyReader);
			if (DecryptedData != null)
			{
				return DecryptedData;
			}

			switch (Conf.CryptoFailureAction)
			{
				case CONSUME:
					// Note, batch message will fail to consume even if config is set to consume
					Log.warn("[{}][{}][{}][{}] Decryption failed. Consuming encrypted message since config is set to consume.", Topic, SubscriptionConflict, ConsumerNameConflict, MessageId);
					return Payload.retain();
				case DISCARD:
					Log.warn("[{}][{}][{}][{}] Discarding message since decryption failed and config is set to discard", Topic, SubscriptionConflict, ConsumerNameConflict, MessageId);
					DiscardMessage(MessageId, CurrentCnx, PulsarApi.CommandAck.ValidationError.DecryptionError);
					return null;
				case FAIL:
					MessageId M = new MessageIdImpl(MessageId.LedgerId, MessageId.EntryId, PartitionIndex);
					Log.error("[{}][{}][{}][{}] Message delivery failed since unable to decrypt incoming message", Topic, SubscriptionConflict, ConsumerNameConflict, M);
					UnAckedMessageTracker.Add(M);
					return null;
			}
			return null;
		}

		private ByteBuf UncompressPayloadIfNeeded(PulsarApi.MessageIdData MessageId, PulsarApi.MessageMetadata MsgMetadata, ByteBuf Payload, ClientCnx CurrentCnx)
		{
			PulsarApi.CompressionType CompressionType = MsgMetadata.Compression;
			CompressionCodec Codec = CompressionCodecProvider.getCompressionCodec(CompressionType);
			int UncompressedSize = MsgMetadata.UncompressedSize;
			int PayloadSize = Payload.readableBytes();
			if (PayloadSize > ClientCnx.MaxMessageSize)
			{
				// payload size is itself corrupted since it cannot be bigger than the MaxMessageSize
				Log.error("[{}][{}] Got corrupted payload message size {} at {}", Topic, SubscriptionConflict, PayloadSize, MessageId);
				DiscardCorruptedMessage(MessageId, CurrentCnx, PulsarApi.CommandAck.ValidationError.UncompressedSizeCorruption);
				return null;
			}

			try
			{
				ByteBuf UncompressedPayload = Codec.decode(Payload, UncompressedSize);
				return UncompressedPayload;
			}
			catch (IOException E)
			{
				Log.error("[{}][{}] Failed to decompress message with {} at {}: {}", Topic, SubscriptionConflict, CompressionType, MessageId, E.Message, E);
				DiscardCorruptedMessage(MessageId, CurrentCnx, PulsarApi.CommandAck.ValidationError.DecompressionError);
				return null;
			}
		}

		private bool VerifyChecksum(ByteBuf HeadersAndPayload, PulsarApi.MessageIdData MessageId)
		{

			if (hasChecksum(HeadersAndPayload))
			{
				int Checksum = readChecksum(HeadersAndPayload);
				int ComputedChecksum = computeChecksum(HeadersAndPayload);
				if (Checksum != ComputedChecksum)
				{
					Log.error("[{}][{}] Checksum mismatch for message at {}:{}. Received checksum: 0x{}, Computed checksum: 0x{}", Topic, SubscriptionConflict, MessageId.LedgerId, MessageId.EntryId, Checksum.ToString("x"), ComputedChecksum.ToString("x"));
					return false;
				}
			}

			return true;
		}

		private void DiscardCorruptedMessage(MessageIdData MessageId, ClientCnx CurrentCnx, CommandAck.ValidationError ValidationError)
		{
			Log.error("[{}][{}] Discarding corrupted message at {}:{}", Topic, SubscriptionConflict, MessageId.LedgerId, MessageId.EntryId);
			DiscardMessage(MessageId, CurrentCnx, ValidationError);
		}

		private void DiscardMessage(MessageIdData MessageId, ClientCnx currentCnx, CommandAck.ValidationError ValidationError)
		{
			IByteBuffer cmd = Commands.NewAck(ConsumerId, (long)MessageId.ledgerId, (long)MessageId.entryId, CommandAck.AckType.Individual, ValidationError, new Dictionary<string,long>());
			currentCnx.Ctx().WriteAndFlushAsync(cmd);
			IncreaseAvailablePermits(currentCnx);
			StatsConflict.IncrementNumReceiveFailed();
		}

		public override string HandlerName
		{
			get
			{
				return SubscriptionConflict;
			}
		}

		public override bool Connected
		{
			get
			{
				return ClientCnx != null && (this.State == State.Ready);
			}
		}


		public override int AvailablePermits
		{
			get
			{
				return AVAILABLE_PERMITS_UPDATER.get(this);
			}
		}

		public override int NumMessagesInQueue()
		{
			return IncomingMessages.size();
		}

		public override void RedeliverUnacknowledgedMessages()
		{
			ClientCnx Cnx = Cnx();
			if (Connected && Cnx.RemoteEndpointProtocolVersion >= (int)ProtocolVersion.v2)
			{
				int CurrentSize = 0;
				lock (this)
				{
					CurrentSize = IncomingMessages.size();
					IncomingMessages.clear();
					IncomingMessagesSizeUpdater.set(this, 0);
					UnAckedMessageTracker<T>.Clear();
				}
				Cnx.ctx().writeAndFlush(Commands.newRedeliverUnacknowledgedMessages(ConsumerId), Cnx.ctx().voidPromise());
				if (CurrentSize > 0)
				{
					IncreaseAvailablePermits(Cnx, CurrentSize);
				}
				if (Log.DebugEnabled)
				{
					Log.debug("[{}] [{}] [{}] Redeliver unacked messages and send {} permits", SubscriptionConflict, Topic, ConsumerNameConflict, CurrentSize);
				}
				return;
			}
			if (Cnx == null || (State == State.Connecting))
			{
				Log.warn("[{}] Client Connection needs to be established for redelivery of unacknowledged messages", this);
			}
			else
			{
				Log.warn("[{}] Reconnecting the client to redeliver the messages.", this);
				Cnx.ctx().close();
			}
		}

		public override void RedeliverUnacknowledgedMessages(ISet<MessageId> MessageIds)
		{
			if (MessageIds.Count == 0)
			{
				return;
			}

			checkArgument(MessageIds.First().get() is MessageIdImpl);

			if (Conf.SubscriptionType != SubscriptionType.Shared && Conf.SubscriptionType != SubscriptionType.Key_Shared)
			{
				// We cannot redeliver single messages if subscription type is not Shared
				RedeliverUnacknowledgedMessages();
				return;
			}
			ClientCnx Cnx = cnx();
			if (Connected && Cnx.RemoteEndpointProtocolVersion >= PulsarApi.ProtocolVersion.v2.Number)
			{
				int MessagesFromQueue = RemoveExpiredMessagesFromQueue(MessageIds);
//JAVA TO C# CONVERTER TODO TASK: Most Java stream collectors are not converted by Java to C# Converter:
				IEnumerable<IList<MessageIdImpl>> Batches = Iterables.partition(MessageIds.Select(messageId => (MessageIdImpl)messageId).collect(Collectors.toSet()), MaxRedeliverUnacknowledged);
				PulsarApi.MessageIdData.Builder Builder = PulsarApi.MessageIdData.newBuilder();
				Batches.forEach(ids =>
				{
				IList<MessageIdData> MessageIdDatas = ids.Where(messageId => !ProcessPossibleToDLQ(messageId)).Select(messageId =>
				{
					Builder.Partition = messageId.PartitionIndex;
					Builder.LedgerId = messageId.LedgerId;
					Builder.EntryId = messageId.EntryId;
					return Builder.build();
				}).ToList();
				ByteBuf Cmd = Commands.newRedeliverUnacknowledgedMessages(ConsumerId, MessageIdDatas);
				Cnx.ctx().writeAndFlush(Cmd, Cnx.ctx().voidPromise());
				MessageIdDatas.ForEach(MessageIdData.recycle);
				});
				if (MessagesFromQueue > 0)
				{
					IncreaseAvailablePermits(Cnx, MessagesFromQueue);
				}
				Builder.recycle();
				if (Log.DebugEnabled)
				{
					Log.debug("[{}] [{}] [{}] Redeliver unacked messages and increase {} permits", SubscriptionConflict, Topic, ConsumerNameConflict, MessagesFromQueue);
				}
				return;
			}
			if (Cnx == null || (State == State.Connecting))
			{
				Log.warn("[{}] Client Connection needs to be established for redelivery of unacknowledged messages", this);
			}
			else
			{
				Log.warn("[{}] Reconnecting the client to redeliver the messages.", this);
				Cnx.ctx().close();
			}
		}

		public override void CompleteOpBatchReceive(OpBatchReceive<T> Op)
		{
			NotifyPendingBatchReceivedCallBack(Op);
		}

		private bool ProcessPossibleToDLQ(MessageIdImpl MessageId)
		{
			IList<MessageImpl<T>> DeadLetterMessages = null;
			if (possibleSendToDeadLetterTopicMessages != null)
			{
				if (MessageId is BatchMessageIdImpl)
				{
					DeadLetterMessages = possibleSendToDeadLetterTopicMessages[new MessageIdImpl(MessageId.LedgerId, MessageId.EntryId, PartitionIndex)];
				}
				else
				{
					DeadLetterMessages = possibleSendToDeadLetterTopicMessages[MessageId];
				}
			}
			if (DeadLetterMessages != null)
			{
				if (deadLetterProducer == null)
				{
					try
					{
						deadLetterProducer = ClientConflict.newProducer(Schema).topic(this.deadLetterPolicy.DeadLetterTopic).blockIfQueueFull(false).create();
					}
					catch (Exception E)
					{
						Log.error("Create dead letter producer exception with topic: {}", deadLetterPolicy.DeadLetterTopic, E);
					}
				}
				if (deadLetterProducer != null)
				{
					try
					{
						foreach (MessageImpl<T> Message in DeadLetterMessages)
						{
							deadLetterProducer.NewMessage().value(Message.Value).properties(Message.Properties).send();
						}
						Acknowledge(MessageId);
						return true;
					}
					catch (Exception E)
					{
						Log.error("Send to dead letter topic exception with topic: {}, messageId: {}", deadLetterProducer.Topic, MessageId, E);
					}
				}
			}
			return false;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void seek(SharpPulsar.api.MessageId messageId) throws SharpPulsar.api.PulsarClientException
		public override void Seek(MessageId MessageId)
		{
			try
			{
				SeekAsync(MessageId).get();
			}
			catch (Exception E)
			{
				throw PulsarClientException.unwrap(E);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void seek(long timestamp) throws SharpPulsar.api.PulsarClientException
		public override void Seek(long Timestamp)
		{
			try
			{
				SeekAsync(Timestamp).get();
			}
			catch (Exception E)
			{
				throw PulsarClientException.unwrap(E);
			}
		}

		public override CompletableFuture<Void> SeekAsync(long Timestamp)
		{
			if (State == State.Closing || State == State.Closed)
			{
				return FutureUtil.failedFuture(new PulsarClientException.AlreadyClosedException(string.Format("The consumer {0} was already closed when seeking the subscription {1} of the topic " + "{2} to the timestamp {3:D}", ConsumerNameConflict, SubscriptionConflict, topicName.ToString(), Timestamp)));
			}

			if (!Connected)
			{
				return FutureUtil.failedFuture(new PulsarClientException(string.Format("The client is not connected to the broker when seeking the subscription {0} of the " + "topic {1} to the timestamp {2:D}", SubscriptionConflict, topicName.ToString(), Timestamp)));
			}

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.CompletableFuture<Void> seekFuture = new java.util.concurrent.CompletableFuture<>();
			CompletableFuture<Void> SeekFuture = new CompletableFuture<Void>();

			long RequestId = ClientConflict.newRequestId();
			ByteBuf Seek = Commands.newSeek(ConsumerId, RequestId, Timestamp);
			ClientCnx Cnx = cnx();

			Log.info("[{}][{}] Seek subscription to publish time {}", Topic, SubscriptionConflict, Timestamp);

			Cnx.sendRequestWithId(Seek, RequestId).thenRun(() =>
			{
			Log.info("[{}][{}] Successfully reset subscription to publish time {}", Topic, SubscriptionConflict, Timestamp);
			acknowledgmentsGroupingTracker.FlushAndClean();
			LastDequeuedMessage = MessageId.earliest;
			IncomingMessages.clear();
			IncomingMessagesSizeUpdater.set(this, 0);
			SeekFuture.complete(null);
			}).exceptionally(e =>
			{
			Log.error("[{}][{}] Failed to reset subscription: {}", Topic, SubscriptionConflict, e.Cause.Message);
			SeekFuture.completeExceptionally(PulsarClientException.wrap(e.Cause, string.Format("Failed to seek the subscription {0} of the topic {1} to the timestamp {2:D}", SubscriptionConflict, topicName.ToString(), Timestamp)));
			return null;
		});
			return SeekFuture;
		}

		public override CompletableFuture<Void> SeekAsync(MessageId MessageId)
		{
			if (State == State.Closing || State == State.Closed)
			{
				return FutureUtil.failedFuture(new PulsarClientException.AlreadyClosedException(string.Format("The consumer {0} was already closed when seeking the subscription {1} of the topic " + "{2} to the message {3}", ConsumerNameConflict, SubscriptionConflict, topicName.ToString(), MessageId.ToString())));
			}

			if (!Connected)
			{
				return FutureUtil.failedFuture(new PulsarClientException(string.Format("The client is not connected to the broker when seeking the subscription {0} of the " + "topic {1} to the message {2}", SubscriptionConflict, topicName.ToString(), MessageId.ToString())));
			}

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.CompletableFuture<Void> seekFuture = new java.util.concurrent.CompletableFuture<>();
			CompletableFuture<Void> SeekFuture = new CompletableFuture<Void>();

			long RequestId = ClientConflict.newRequestId();
			MessageIdImpl MsgId = (MessageIdImpl) MessageId;
			ByteBuf Seek = Commands.newSeek(ConsumerId, RequestId, MsgId.LedgerId, MsgId.EntryId);
			ClientCnx Cnx = cnx();

			Log.info("[{}][{}] Seek subscription to message id {}", Topic, SubscriptionConflict, MessageId);

			Cnx.sendRequestWithId(Seek, RequestId).thenRun(() =>
			{
			Log.info("[{}][{}] Successfully reset subscription to message id {}", Topic, SubscriptionConflict, MessageId);
			acknowledgmentsGroupingTracker.FlushAndClean();
			LastDequeuedMessage = MessageId;
			IncomingMessages.clear();
			IncomingMessagesSizeUpdater.set(this, 0);
			SeekFuture.complete(null);
			}).exceptionally(e =>
			{
			Log.error("[{}][{}] Failed to reset subscription: {}", Topic, SubscriptionConflict, e.Cause.Message);
			SeekFuture.completeExceptionally(PulsarClientException.wrap(e.Cause, string.Format("[{0}][{1}] Failed to seek the subscription {2} of the topic {3} to the message {4}", SubscriptionConflict, topicName.ToString(), MessageId.ToString())));
			return null;
		});
			return SeekFuture;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean hasMessageAvailable() throws SharpPulsar.api.PulsarClientException
		public virtual bool HasMessageAvailable()
		{
			// we need to seek to the last position then the last message can be received when the resetIncludeHead
			// specified.
			if (LastDequeuedMessage == MessageIdFields.Latest && resetIncludeHead)
			{
				LastDequeuedMessage = LastMessageId;
				Seek(LastDequeuedMessage);
			}
			try
			{
				if (HasMoreMessages(lastMessageIdInBroker, LastDequeuedMessage))
				{
					return true;
				}

				return HasMessageAvailableAsync().get();
			}
			catch (Exception E)
			{
				throw PulsarClientException.unwrap(E);
			}
		}

		public virtual CompletableFuture<bool> HasMessageAvailableAsync()
		{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.CompletableFuture<bool> booleanFuture = new java.util.concurrent.CompletableFuture<>();
			CompletableFuture<bool> BooleanFuture = new CompletableFuture<bool>();

			if (HasMoreMessages(lastMessageIdInBroker, LastDequeuedMessage))
			{
				BooleanFuture.complete(true);
			}
			else
			{
				LastMessageIdAsync.thenAccept(messageId =>
				{
				lastMessageIdInBroker = messageId;
				if (HasMoreMessages(lastMessageIdInBroker, LastDequeuedMessage))
				{
					BooleanFuture.complete(true);
				}
				else
				{
					BooleanFuture.complete(false);
				}
				}).exceptionally(e =>
				{
				Log.error("[{}][{}] Failed getLastMessageId command", Topic, SubscriptionConflict);
				BooleanFuture.completeExceptionally(e.Cause);
				return null;
			});
			}
			return BooleanFuture;
		}

		private bool HasMoreMessages(MessageId LastMessageIdInBroker, MessageId LastDequeuedMessage)
		{
			if (LastMessageIdInBroker.CompareTo(LastDequeuedMessage) > 0 && ((MessageIdImpl)LastMessageIdInBroker).EntryId != -1)
			{
				return true;
			}
			else
			{
				// Make sure batching message can be read completely.
				return LastMessageIdInBroker.CompareTo(LastDequeuedMessage) == 0 && IncomingMessages.size() > 0;
			}
		}

		public override CompletableFuture<MessageId> LastMessageIdAsync
		{
			get
			{
				if (State == State.Closing || State == State.Closed)
				{
					return FutureUtil.failedFuture(new PulsarClientException.AlreadyClosedException(string.Format("The consumer {0} was already closed when the subscription {1} of the topic {2} " + "getting the last message id", ConsumerNameConflict, SubscriptionConflict, topicName.ToString())));
				}
    
				AtomicLong OpTimeoutMs = new AtomicLong(ClientConflict.Configuration.OperationTimeoutMs);
				Backoff Backoff = (new BackoffBuilder()).SetInitialTime(100, BAMCIS.Util.Concurrent.TimeUnit.MILLISECONDS).setMax(OpTimeoutMs.get() * 2, BAMCIS.Util.Concurrent.TimeUnit.MILLISECONDS).setMandatoryStop(0, BAMCIS.Util.Concurrent.TimeUnit.MILLISECONDS).create();
    
				CompletableFuture<MessageId> GetLastMessageIdFuture = new CompletableFuture<MessageId>();
    
				InternalGetLastMessageIdAsync(Backoff, OpTimeoutMs, GetLastMessageIdFuture);
				return GetLastMessageIdFuture;
			}
		}

		private void InternalGetLastMessageIdAsync(in Backoff Backoff, in AtomicLong RemainingTime, CompletableFuture<MessageId> Future)
		{
			ClientCnx Cnx = cnx();
			if (Connected && Cnx != null)
			{
				if (!Commands.peerSupportsGetLastMessageId(Cnx.RemoteEndpointProtocolVersion))
				{
					Future.completeExceptionally(new PulsarClientException.NotSupportedException(string.Format("The command `GetLastMessageId` is not supported for the protocol version {0:D}. " + "The consumer is {1}, topic {2}, subscription {3}", Cnx.RemoteEndpointProtocolVersion, ConsumerNameConflict, topicName.ToString(), SubscriptionConflict)));
				}

				long RequestId = ClientConflict.newRequestId();
				ByteBuf GetLastIdCmd = Commands.newGetLastMessageId(ConsumerId, RequestId);
				Log.info("[{}][{}] Get topic last message Id", Topic, SubscriptionConflict);

				Cnx.sendGetLastMessageId(GetLastIdCmd, RequestId).thenAccept((result) =>
				{
				Log.info("[{}][{}] Successfully getLastMessageId {}:{}", Topic, SubscriptionConflict, result.LedgerId, result.EntryId);
				Future.complete(new MessageIdImpl(result.LedgerId, result.EntryId, result.Partition));
				}).exceptionally(e =>
				{
				Log.error("[{}][{}] Failed getLastMessageId command", Topic, SubscriptionConflict);
				Future.completeExceptionally(PulsarClientException.wrap(e.Cause, string.Format("The subscription {0} of the topic {1} gets the last message id was failed", SubscriptionConflict, topicName.ToString())));
				return null;
			});
			}
			else
			{
				long NextDelay = Math.Min(Backoff.next(), RemainingTime.get());
				if (NextDelay <= 0)
				{
					Future.completeExceptionally(new PulsarClientException.TimeoutException(string.Format("The subscription {0} of the topic {1} could not get the last message id " + "withing configured timeout", SubscriptionConflict, topicName.ToString())));
					return;
				}

				((ScheduledExecutorService) ListenerExecutor).schedule(() =>
				{
				Log.warn("[{}] [{}] Could not get connection while getLastMessageId -- Will try again in {} ms", Topic, HandlerName, NextDelay);
				RemainingTime.addAndGet(-NextDelay);
				InternalGetLastMessageIdAsync(Backoff, RemainingTime, Future);
				}, NextDelay, BAMCIS.Util.Concurrent.TimeUnit.MILLISECONDS);
			}
		}

		private MessageIdImpl GetMessageIdImpl<T1>(Message<T1> Msg)
		{
			MessageIdImpl MessageId = (MessageIdImpl) Msg.MessageId;
			if (MessageId is BatchMessageIdImpl)
			{
				// messageIds contain MessageIdImpl, not BatchMessageIdImpl
				MessageId = new MessageIdImpl(MessageId.LedgerId, MessageId.EntryId, PartitionIndex);
			}
			return MessageId;
		}


		private bool IsMessageUndecryptable(PulsarApi.MessageMetadata MsgMetadata)
		{
			return (MsgMetadata.EncryptionKeysCount > 0 && Conf.CryptoKeyReader == null && Conf.CryptoFailureAction == ConsumerCryptoFailureAction.Consume);
		}

		/// <summary>
		/// Create EncryptionContext if message payload is encrypted
		/// </summary>
		/// <param name="msgMetadata"> </param>
		/// <returns> <seealso cref="Optional"/><<seealso cref="EncryptionContext"/>> </returns>
		private Optional<EncryptionContext> CreateEncryptionContext(PulsarApi.MessageMetadata MsgMetadata)
		{

			EncryptionContext EncryptionCtx = null;
			if (MsgMetadata.EncryptionKeysCount > 0)
			{
				EncryptionCtx = new EncryptionContext();
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
				IDictionary<string, EncryptionContext.EncryptionKey> Keys = MsgMetadata.EncryptionKeysList.ToDictionary(PulsarApi.EncryptionKeys::getKey, e => new EncryptionContext.EncryptionKey(e.Value.toByteArray(), e.MetadataList != null ? e.MetadataList.ToDictionary(PulsarApi.KeyValue::getKey, PulsarApi.KeyValue::getValue) : null));
				sbyte[] EncParam = new sbyte[MessageCrypto.IvLen];
				MsgMetadata.EncryptionParam.copyTo(EncParam, 0);
				int? BatchSize = Optional.ofNullable(MsgMetadata.hasNumMessagesInBatch() ? MsgMetadata.NumMessagesInBatch : null);
				EncryptionCtx.Keys = Keys;
				EncryptionCtx.Param = EncParam;
				EncryptionCtx.Algorithm = MsgMetadata.EncryptionAlgo;
				EncryptionCtx.CompressionType = CompressionCodecProvider.convertFromWireProtocol(MsgMetadata.Compression);
				EncryptionCtx.UncompressedMessageSize = MsgMetadata.UncompressedSize;
				EncryptionCtx.BatchSize = BatchSize;
			}
			return Optional.ofNullable(EncryptionCtx);
		}

		private int RemoveExpiredMessagesFromQueue(ISet<MessageId> MessageIds)
		{
			int MessagesFromQueue = 0;
			Message<T> Peek = IncomingMessages.peek();
			if (Peek != null)
			{
				MessageIdImpl MessageId = GetMessageIdImpl(Peek);
				if (!MessageIds.Contains(MessageId))
				{
					// first message is not expired, then no message is expired in queue.
					return 0;
				}

				// try not to remove elements that are added while we remove
				Message<T> Message = IncomingMessages.poll();
				while (Message != null)
				{
					IncomingMessagesSizeUpdater.addAndGet(this, -Message.Data.Length);
					MessagesFromQueue++;
					MessageIdImpl Id = GetMessageIdImpl(Message);
					if (!MessageIds.Contains(Id))
					{
						MessageIds.Add(Id);
						break;
					}
					Message = IncomingMessages.poll();
				}
			}
			return MessagesFromQueue;
		}

		public override ConsumerStats Stats
		{
			get
			{
				return StatsConflict;
			}
		}

		public virtual void SetTerminated()
		{
			Log.info("[{}] [{}] [{}] Consumer has reached the end of topic", SubscriptionConflict, Topic, ConsumerNameConflict);
			hasReachedEndOfTopic = true;
			if (Listener != null)
			{
				// Propagate notification to listener
				Listener.reachedEndOfTopic(this);
			}
		}

		public override bool HasReachedEndOfTopic()
		{
			return hasReachedEndOfTopic;
		}

		public override int GetHashCode()
		{
			return Objects.hash(Topic, SubscriptionConflict, ConsumerNameConflict);
		}

		// wrapper for connection methods
		public virtual ClientCnx Cnx()
		{
			return this.ConnectionHandler.Cnx();
		}

		public virtual void ResetBackoff()
		{
			this.ConnectionHandler.ResetBackoff();
		}

		public virtual void ConnectionClosed(ClientCnx Cnx)
		{
			this.ConnectionHandler.ConnectionClosed(Cnx);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @VisibleForTesting public ClientCnx getClientCnx()
		public virtual ClientCnx ClientCnx
		{
			get
			{
				return this.ConnectionHandler.ClientCnx;
			}
			set
			{
				this.ConnectionHandler.ClientCnx = value;
			}
		}


		public virtual void ReconnectLater(Exception Exception)
		{
			this.ConnectionHandler.ReconnectLater(Exception);
		}

		public virtual void GrabCnx()
		{
			this.ConnectionHandler.GrabCnx();
		}


		internal static readonly ILogger Log = new LoggerFactory().CreateLogger(typeof(ConsumerImpl));

	}

}