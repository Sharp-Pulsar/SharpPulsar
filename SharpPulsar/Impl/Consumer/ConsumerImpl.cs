using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

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
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.scurrilous.circe.checksum.Crc32cIntChecksum.computeChecksum;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.apache.pulsar.common.protocol.Commands.hasChecksum;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.apache.pulsar.common.protocol.Commands.readChecksum;

	using VisibleForTesting = com.google.common.annotations.VisibleForTesting;
	using Iterables = com.google.common.collect.Iterables;

	using Queues = com.google.common.collect.Queues;
	using ByteBuf = io.netty.buffer.ByteBuf;
	using Timeout = io.netty.util.Timeout;


	using StringUtils = org.apache.commons.lang3.StringUtils;
	using Consumer = org.apache.pulsar.client.api.Consumer;
	using ConsumerCryptoFailureAction = org.apache.pulsar.client.api.ConsumerCryptoFailureAction;
	using ConsumerStats = org.apache.pulsar.client.api.ConsumerStats;
	using DeadLetterPolicy = org.apache.pulsar.client.api.DeadLetterPolicy;
	using Message = org.apache.pulsar.client.api.Message;
	using MessageId = org.apache.pulsar.client.api.MessageId;
	using Messages = org.apache.pulsar.client.api.Messages;
	using Producer = org.apache.pulsar.client.api.Producer;
	using PulsarClientException = org.apache.pulsar.client.api.PulsarClientException;
	using Schema = org.apache.pulsar.client.api.Schema;
	using SubscriptionInitialPosition = org.apache.pulsar.client.api.SubscriptionInitialPosition;
	using SubscriptionType = org.apache.pulsar.client.api.SubscriptionType;
	using TopicDoesNotExistException = org.apache.pulsar.client.api.PulsarClientException.TopicDoesNotExistException;
	using SharpPulsar.Impl.conf;
	using TransactionImpl = SharpPulsar.Impl.transaction.TransactionImpl;
	using Commands = org.apache.pulsar.common.protocol.Commands;
	using EncryptionContext = org.apache.pulsar.common.api.EncryptionContext;
	using EncryptionKey = org.apache.pulsar.common.api.EncryptionContext.EncryptionKey;
	using PulsarApi = org.apache.pulsar.common.api.proto.PulsarApi;
	using AckType = org.apache.pulsar.common.api.proto.PulsarApi.CommandAck.AckType;
	using ValidationError = org.apache.pulsar.common.api.proto.PulsarApi.CommandAck.ValidationError;
	using InitialPosition = org.apache.pulsar.common.api.proto.PulsarApi.CommandSubscribe.InitialPosition;
	using CompressionType = org.apache.pulsar.common.api.proto.PulsarApi.CompressionType;
	using EncryptionKeys = org.apache.pulsar.common.api.proto.PulsarApi.EncryptionKeys;
	using KeyValue = org.apache.pulsar.common.api.proto.PulsarApi.KeyValue;
	using MessageIdData = org.apache.pulsar.common.api.proto.PulsarApi.MessageIdData;
	using MessageMetadata = org.apache.pulsar.common.api.proto.PulsarApi.MessageMetadata;
	using ProtocolVersion = org.apache.pulsar.common.api.proto.PulsarApi.ProtocolVersion;
	using CompressionCodec = org.apache.pulsar.common.compression.CompressionCodec;
	using CompressionCodecProvider = org.apache.pulsar.common.compression.CompressionCodecProvider;
	using TopicName = org.apache.pulsar.common.naming.TopicName;
	using SchemaInfo = org.apache.pulsar.common.schema.SchemaInfo;
	using SchemaType = org.apache.pulsar.common.schema.SchemaType;
	using FutureUtil = org.apache.pulsar.common.util.FutureUtil;
	using Logger = org.slf4j.Logger;
	using LoggerFactory = org.slf4j.LoggerFactory;
    using SharpPulsar.Interface;
    using SharpPulsar.Impl.Batch;
    using SharpPulsar.Impl.Message;
    using SharpPulsar.Interface.Producer;
    using SharpPulsar.Configuration;
    using System.Threading.Tasks;
    using SharpPulsar.Interface.Consumer;
    using SharpPulsar.Interface.Schema;
    using SharpPulsar.Interface.Message;

    public class ConsumerImpl<T> : ConsumerBase<T>, ConnectionHandler.Connection
	{
		private const int MAX_REDELIVER_UNACKNOWLEDGED = 1000;

		internal readonly long consumerId;

		// Number of messages that have delivered to the application. Every once in a while, this number will be sent to the
		// broker to notify that we are ready to get (and store in the incoming messages queue) more messages
		private static readonly AtomicIntegerFieldUpdater<ConsumerImpl> AVAILABLE_PERMITS_UPDATER = AtomicIntegerFieldUpdater.newUpdater(typeof(ConsumerImpl), "availablePermits");
		private volatile int availablePermits = 0;

		protected internal volatile MessageId lastDequeuedMessage = MessageId.earliest;
		private volatile MessageId lastMessageIdInBroker = MessageId.earliest;

		private long subscribeTimeout;
		private readonly int partitionIndex;
		private readonly bool hasParentConsumer;

		private readonly int receiverQueueRefillThreshold;

		private readonly ReadWriteLock @lock = new ReentrantReadWriteLock();

		private readonly UnAckedMessageTracker unAckedMessageTracker;
		private readonly AcknowledgmentsGroupingTracker acknowledgmentsGroupingTracker;
		private readonly NegativeAcksTracker negativeAcksTracker;

		protected internal readonly ConsumerStatsRecorder stats;
		private readonly int priorityLevel;
		private readonly SubscriptionMode subscriptionMode;
		private volatile BatchMessageIdImpl startMessageId;

		private readonly BatchMessageIdImpl initialStartMessageId;
		private readonly long startMessageRollbackDurationInSec;

//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		private volatile bool hasReachedEndOfTopic_Conflict;

		private readonly MessageCrypto msgCrypto;

		private readonly IDictionary<string, string> metadata;

		private readonly bool readCompacted;
		private readonly bool resetIncludeHead;

		private readonly SubscriptionInitialPosition subscriptionInitialPosition;
		private readonly ConnectionHandler connectionHandler;

		private readonly TopicName topicName;
		private readonly string topicNameWithoutPartition;

		private readonly IDictionary<MessageIdImpl, IList<MessageImpl<T>>> possibleSendToDeadLetterTopicMessages;

		private readonly DeadLetterPolicy deadLetterPolicy;

		private IProducer<T> deadLetterProducer;

		protected internal volatile bool paused;

		private readonly bool createTopicIfDoesNotExist;

		internal enum SubscriptionMode
		{
			// Make the subscription to be backed by a durable cursor that will retain messages and persist the current
			// position
			Durable,

			// Lightweight subscription mode that doesn't have a durable cursor associated
			NonDurable
		}

		internal static ConsumerImpl<T> NewConsumerImpl(PulsarClientImpl client, string topic, ConsumerConfigurationData<T> conf, ExecutorService listenerExecutor, int partitionIndex, bool hasParentConsumer, ValueTask<IConsumer<T>> subscribeFuture, SubscriptionMode subscriptionMode, MessageId startMessageId, ISchema<T> schema, ConsumerInterceptors<T> interceptors, bool createTopicIfDoesNotExist)
		{
			if (conf.ReceiverQueueSize == 0)
			{
				return new ZeroQueueConsumerImpl<T>(client, topic, conf, listenerExecutor, partitionIndex, hasParentConsumer, subscribeFuture, subscriptionMode, startMessageId, schema, interceptors, createTopicIfDoesNotExist);
			}
			else
			{
				return new ConsumerImpl<T>(client, topic, conf, listenerExecutor, partitionIndex, hasParentConsumer, subscribeFuture, subscriptionMode, startMessageId, 0, schema, interceptors, createTopicIfDoesNotExist);
			}
		}

		protected internal ConsumerImpl(PulsarClientImpl client, string topic, ConsumerConfigurationData<T> conf, ExecutorService listenerExecutor, int partitionIndex, bool hasParentConsumer, ValueTask<IConsumer<T>> subscribeFuture, SubscriptionMode subscriptionMode, IMessageId startMessageId, long startMessageRollbackDurationInSec, ISchema<T> schema, ConsumerInterceptors<T> interceptors, bool createTopicIfDoesNotExist) : base(client, topic, conf, conf.ReceiverQueueSize, listenerExecutor, subscribeFuture, schema, interceptors)
		{
			this.consumerId = client.NewConsumerId();
			this.subscriptionMode = subscriptionMode;
			this.startMessageId = startMessageId != null ? new BatchMessageIdImpl((MessageIdImpl) startMessageId) : null;
			this.lastDequeuedMessage = startMessageId == null ? MessageId.earliest : startMessageId;
			this.initialStartMessageId = this.startMessageId;
			this.startMessageRollbackDurationInSec = startMessageRollbackDurationInSec;
			AVAILABLE_PERMITS_UPDATER.set(this, 0);
			this.subscribeTimeout = DateTimeHelper.CurrentUnixTimeMillis() + client.Configuration.OperationTimeoutMs;
			this.partitionIndex = partitionIndex;
			this.hasParentConsumer = hasParentConsumer;
			this.receiverQueueRefillThreshold = conf.ReceiverQueueSize / 2;
			this.priorityLevel = conf.PriorityLevel;
			this.readCompacted = conf.ReadCompacted;
			this.subscriptionInitialPosition = conf.SubscriptionInitialPosition;
			this.negativeAcksTracker = new NegativeAcksTracker(this, conf);
			this.resetIncludeHead = conf.ResetIncludeHead;
			this.createTopicIfDoesNotExist = createTopicIfDoesNotExist;

			if (client.Configuration.StatsIntervalSeconds > 0)
			{
				stats = new ConsumerStatsRecorderImpl(client, conf, this);
			}
			else
			{
				stats = ConsumerStatsDisabled.INSTANCE;
			}

			if (conf.AckTimeoutMillis != 0)
			{
				if (conf.TickDurationMillis > 0)
				{
					this.unAckedMessageTracker = new UnAckedMessageTracker(client, this, conf.AckTimeoutMillis, Math.Min(conf.TickDurationMillis, conf.AckTimeoutMillis));
				}
				else
				{
					this.unAckedMessageTracker = new UnAckedMessageTracker(client, this, conf.AckTimeoutMillis);
				}
			}
			else
			{
				this.unAckedMessageTracker = UnAckedMessageTracker.UNACKED_MESSAGE_TRACKER_DISABLED;
			}

			// Create msgCrypto if not created already
			if (conf.CryptoKeyReader != null)
			{
				this.msgCrypto = new MessageCrypto(string.Format("[{0}] [{1}]", topic, subscription), false);
			}
			else
			{
				this.msgCrypto = null;
			}

			if (conf.Properties.Empty)
			{
				metadata = Collections.emptyMap();
			}
			else
			{
				metadata = Collections.unmodifiableMap(new Dictionary<>(conf.Properties));
			}

			this.connectionHandler = new ConnectionHandler(this, new BackoffBuilder()
									.setInitialTime(client.Configuration.InitialBackoffIntervalNanos, TimeUnit.NANOSECONDS).setMax(client.Configuration.MaxBackoffIntervalNanos, TimeUnit.NANOSECONDS).setMandatoryStop(0, TimeUnit.MILLISECONDS).create(), this);

			this.topicName = TopicName.get(topic);
			if (this.topicName.Persistent)
			{
				this.acknowledgmentsGroupingTracker = new PersistentAcknowledgmentsGroupingTracker(this, conf, client.eventLoopGroup());
			}
			else
			{
				this.acknowledgmentsGroupingTracker = NonPersistentAcknowledgmentGroupingTracker.of();
			}

			if (conf.DeadLetterPolicy != null)
			{
				possibleSendToDeadLetterTopicMessages = new ConcurrentDictionary<MessageIdImpl, IList<MessageImpl<T>>>();
				if (StringUtils.isNotBlank(conf.DeadLetterPolicy.DeadLetterTopic))
				{
					this.deadLetterPolicy = DeadLetterPolicy.builder().maxRedeliverCount(conf.DeadLetterPolicy.MaxRedeliverCount).deadLetterTopic(conf.DeadLetterPolicy.DeadLetterTopic).build();
				}
				else
				{
					this.deadLetterPolicy = DeadLetterPolicy.builder().maxRedeliverCount(conf.DeadLetterPolicy.MaxRedeliverCount).deadLetterTopic(string.Format("{0}-{1}-DLQ", topic, subscription)).build();
				}
			}
			else
			{
				deadLetterPolicy = null;
				possibleSendToDeadLetterTopicMessages = null;
			}

			topicNameWithoutPartition = topicName.PartitionedTopicName;

			grabCnx();
		}

		public virtual ConnectionHandler ConnectionHandler
		{
			get
			{
				return connectionHandler;
			}
		}

		public virtual UnAckedMessageTracker UnAckedMessageTracker
		{
			get
			{
				return unAckedMessageTracker;
			}
		}

		public override CompletableFuture<Void> unsubscribeAsync()
		{
			if (State == State.Closing || State == State.Closed)
			{
				return FutureUtil.failedFuture(new PulsarClientException.AlreadyClosedException("Consumer was already closed"));
			}
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.CompletableFuture<Void> unsubscribeFuture = new java.util.concurrent.CompletableFuture<>();
			CompletableFuture<Void> unsubscribeFuture = new CompletableFuture<Void>();
			if (Connected)
			{
				State = State.Closing;
				long requestId = client.newRequestId();
				ByteBuf unsubscribe = Commands.newUnsubscribe(consumerId, requestId);
				ClientCnx cnx = cnx();
				cnx.sendRequestWithId(unsubscribe, requestId).thenRun(() =>
				{
				cnx.removeConsumer(consumerId);
				unAckedMessageTracker.Dispose();
				if (possibleSendToDeadLetterTopicMessages != null)
				{
					possibleSendToDeadLetterTopicMessages.Clear();
				}
				client.cleanupConsumer(ConsumerImpl.this);
				log.info("[{}][{}] Successfully unsubscribed from topic", topic, subscription);
				State = State.Closed;
				unsubscribeFuture.complete(null);
				}).exceptionally(e =>
				{
				log.error("[{}][{}] Failed to unsubscribe: {}", topic, subscription, e.Cause.Message);
				State = State.Ready;
				unsubscribeFuture.completeExceptionally(PulsarClientException.wrap(e.Cause, string.Format("Failed to unsubscribe the subscription {0} of topic {1}", topicName.ToString(), subscription)));
				return null;
			});
			}
			else
			{
				unsubscribeFuture.completeExceptionally(new PulsarClientException(string.Format("The client is not connected to the broker when unsubscribing the " + "subscription {0} of the topic {1}", subscription, topicName.ToString())));
			}
			return unsubscribeFuture;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override protected org.apache.pulsar.client.api.Message<T> internalReceive() throws org.apache.pulsar.client.api.PulsarClientException
		protected internal override Message<T> internalReceive()
		{
			Message<T> message;
			try
			{
				message = incomingMessages.take();
				messageProcessed(message);
				return beforeConsume(message);
			}
			catch (InterruptedException e)
			{
				stats.incrementNumReceiveFailed();
				throw PulsarClientException.unwrap(e);
			}
		}

		protected internal override CompletableFuture<Message<T>> internalReceiveAsync()
		{

			CompletableFuture<Message<T>> result = new CompletableFuture<Message<T>>();
			Message<T> message = null;
			try
			{
				@lock.writeLock().@lock();
				message = incomingMessages.poll(0, TimeUnit.MILLISECONDS);
				if (message == null)
				{
					pendingReceives.add(result);
				}
			}
			catch (InterruptedException e)
			{
				Thread.CurrentThread.Interrupt();
				result.completeExceptionally(e);
			}
			finally
			{
				@lock.writeLock().unlock();
			}

			if (message != null)
			{
				messageProcessed(message);
				result.complete(beforeConsume(message));
			}

			return result;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override protected org.apache.pulsar.client.api.Message<T> internalReceive(int timeout, java.util.concurrent.TimeUnit unit) throws org.apache.pulsar.client.api.PulsarClientException
		protected internal override Message<T> internalReceive(int timeout, TimeUnit unit)
		{
			Message<T> message;
			try
			{
				message = incomingMessages.poll(timeout, unit);
				if (message == null)
				{
					return null;
				}
				messageProcessed(message);
				return beforeConsume(message);
			}
			catch (InterruptedException e)
			{
				State state = State;
				if (state != State.Closing && state != State.Closed)
				{
					stats.incrementNumReceiveFailed();
					throw PulsarClientException.unwrap(e);
				}
				else
				{
					return null;
				}
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override protected org.apache.pulsar.client.api.Messages<T> internalBatchReceive() throws org.apache.pulsar.client.api.PulsarClientException
		protected internal override Messages<T> internalBatchReceive()
		{
			try
			{
				return internalBatchReceiveAsync().get();
			}
			catch (Exception e) when (e is InterruptedException || e is ExecutionException)
			{
				State state = State;
				if (state != State.Closing && state != State.Closed)
				{
					stats.incrementNumBatchReceiveFailed();
					throw PulsarClientException.unwrap(e);
				}
				else
				{
					return null;
				}
			}
		}

		protected internal override CompletableFuture<Messages<T>> internalBatchReceiveAsync()
		{
			CompletableFuture<Messages<T>> result = new CompletableFuture<Messages<T>>();
			try
			{
				@lock.writeLock().@lock();
				if (pendingBatchReceives == null)
				{
					pendingBatchReceives = Queues.newConcurrentLinkedQueue();
				}
				if (hasEnoughMessagesForBatchReceive())
				{
					MessagesImpl<T> messages = NewMessagesImpl;
					Message<T> msgPeeked = incomingMessages.peek();
					while (msgPeeked != null && messages.canAdd(msgPeeked))
					{
						Message<T> msg = incomingMessages.poll();
						if (msg != null)
						{
							messageProcessed(msg);
							Message<T> interceptMsg = beforeConsume(msg);
							messages.add(interceptMsg);
						}
						msgPeeked = incomingMessages.peek();
					}
					result.complete(messages);
				}
				else
				{
					pendingBatchReceives.add(OpBatchReceive.of(result));
				}
			}
			finally
			{
				@lock.writeLock().unlock();
			}
			return result;
		}

		internal virtual bool markAckForBatchMessage(BatchMessageIdImpl batchMessageId, PulsarApi.CommandAck.AckType ackType, IDictionary<string, long> properties)
		{
			bool isAllMsgsAcked;
			if (ackType == PulsarApi.CommandAck.AckType.Individual)
			{
				isAllMsgsAcked = batchMessageId.ackIndividual();
			}
			else
			{
				isAllMsgsAcked = batchMessageId.ackCumulative();
			}
			int outstandingAcks = 0;
			if (log.DebugEnabled)
			{
				outstandingAcks = batchMessageId.OutstandingAcksInSameBatch;
			}

			int batchSize = batchMessageId.BatchSize;
			// all messages in this batch have been acked
			if (isAllMsgsAcked)
			{
				if (log.DebugEnabled)
				{
					log.debug("[{}] [{}] can ack message to broker {}, acktype {}, cardinality {}, length {}", subscription, consumerName, batchMessageId, ackType, outstandingAcks, batchSize);
				}
				return true;
			}
			else
			{
				if (PulsarApi.CommandAck.AckType.Cumulative == ackType && !batchMessageId.Acker.PrevBatchCumulativelyAcked)
				{
					sendAcknowledge(batchMessageId.prevBatchMessageId(), PulsarApi.CommandAck.AckType.Cumulative, properties, null);
					batchMessageId.Acker.PrevBatchCumulativelyAcked = true;
				}
				else
				{
					onAcknowledge(batchMessageId, null);
				}
				if (log.DebugEnabled)
				{
					log.debug("[{}] [{}] cannot ack message to broker {}, acktype {}, pending acks - {}", subscription, consumerName, batchMessageId, ackType, outstandingAcks);
				}
			}
			return false;
		}

		protected internal override CompletableFuture<Void> doAcknowledge(MessageId messageId, PulsarApi.CommandAck.AckType ackType, IDictionary<string, long> properties, TransactionImpl txnImpl)
		{
			checkArgument(messageId is MessageIdImpl);
			if (State != State.Ready && State != State.Connecting)
			{
				stats.incrementNumAcksFailed();
				PulsarClientException exception = new PulsarClientException("Consumer not ready. State: " + State);
				if (PulsarApi.CommandAck.AckType.Individual.Equals(ackType))
				{
					onAcknowledge(messageId, exception);
				}
				else if (PulsarApi.CommandAck.AckType.Cumulative.Equals(ackType))
				{
					onAcknowledgeCumulative(messageId, exception);
				}
				return FutureUtil.failedFuture(exception);
			}

			if (messageId is BatchMessageIdImpl)
			{
				if (markAckForBatchMessage((BatchMessageIdImpl) messageId, ackType, properties))
				{
					// all messages in batch have been acked so broker can be acked via sendAcknowledge()
					if (log.DebugEnabled)
					{
						log.debug("[{}] [{}] acknowledging message - {}, acktype {}", subscription, consumerName, messageId, ackType);
					}
				}
				else
				{
					// other messages in batch are still pending ack.
					return CompletableFuture.completedFuture(null);
				}
			}
			return sendAcknowledge(messageId, ackType, properties, txnImpl);
		}

		// TODO: handle transactional acknowledgements.
		private CompletableFuture<Void> sendAcknowledge(MessageId messageId, PulsarApi.CommandAck.AckType ackType, IDictionary<string, long> properties, TransactionImpl txnImpl)
		{
			MessageIdImpl msgId = (MessageIdImpl) messageId;

			if (ackType == PulsarApi.CommandAck.AckType.Individual)
			{
				if (messageId is BatchMessageIdImpl)
				{
					BatchMessageIdImpl batchMessageId = (BatchMessageIdImpl) messageId;

					stats.incrementNumAcksSent(batchMessageId.BatchSize);
					unAckedMessageTracker.remove(new MessageIdImpl(batchMessageId.LedgerId, batchMessageId.EntryId, batchMessageId.PartitionIndex));
					if (possibleSendToDeadLetterTopicMessages != null)
					{
						possibleSendToDeadLetterTopicMessages.Remove(new MessageIdImpl(batchMessageId.LedgerId, batchMessageId.EntryId, batchMessageId.PartitionIndex));
					}
				}
				else
				{
					// increment counter by 1 for non-batch msg
					unAckedMessageTracker.remove(msgId);
					if (possibleSendToDeadLetterTopicMessages != null)
					{
						possibleSendToDeadLetterTopicMessages.Remove(msgId);
					}
					stats.incrementNumAcksSent(1);
				}
				onAcknowledge(messageId, null);
			}
			else if (ackType == PulsarApi.CommandAck.AckType.Cumulative)
			{
				onAcknowledgeCumulative(messageId, null);
				stats.incrementNumAcksSent(unAckedMessageTracker.removeMessagesTill(msgId));
			}

			acknowledgmentsGroupingTracker.addAcknowledgment(msgId, ackType, properties);

			// Consumer acknowledgment operation immediately succeeds. In any case, if we're not able to send ack to broker,
			// the messages will be re-delivered
			return CompletableFuture.completedFuture(null);
		}

		public override void negativeAcknowledge(MessageId messageId)
		{
			negativeAcksTracker.add(messageId);

			// Ensure the message is not redelivered for ack-timeout, since we did receive an "ack"
			unAckedMessageTracker.remove(messageId);
		}

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: @Override public void connectionOpened(final ClientCnx cnx)
		public virtual void connectionOpened(ClientCnx cnx)
		{
			ClientCnx = cnx;
			cnx.registerConsumer(consumerId, this);

			log.info("[{}][{}] Subscribing to topic on cnx {}", topic, subscription, cnx.ctx().channel());

			long requestId = client.newRequestId();

			int currentSize;
			lock (this)
			{
				currentSize = incomingMessages.size();
				startMessageId = clearReceiverQueue();
				if (possibleSendToDeadLetterTopicMessages != null)
				{
					possibleSendToDeadLetterTopicMessages.Clear();
				}
			}

			bool isDurable = subscriptionMode == SubscriptionMode.Durable;
			PulsarApi.MessageIdData startMessageIdData;
			if (isDurable)
			{
				// For regular durable subscriptions, the message id from where to restart will be determined by the broker.
				startMessageIdData = null;
			}
			else
			{
				// For non-durable we are going to restart from the next entry
				PulsarApi.MessageIdData.Builder builder = PulsarApi.MessageIdData.newBuilder();
				builder.LedgerId = startMessageId.LedgerId;
				builder.EntryId = startMessageId.EntryId;
				if (startMessageId is BatchMessageIdImpl)
				{
					builder.BatchIndex = ((BatchMessageIdImpl) startMessageId).BatchIndex;
				}

				startMessageIdData = builder.build();
				builder.recycle();
			}

			SchemaInfo si = schema.SchemaInfo;
			if (si != null && (SchemaType.BYTES == si.Type || SchemaType.NONE == si.Type))
			{
				// don't set schema for Schema.BYTES
				si = null;
			}
			// startMessageRollbackDurationInSec should be consider only once when consumer connects to first time
			long startMessageRollbackDuration = (startMessageRollbackDurationInSec > 0 && startMessageId.Equals(initialStartMessageId)) ? startMessageRollbackDurationInSec : 0;
			ByteBuf request = Commands.newSubscribe(topic, subscription, consumerId, requestId, SubType, priorityLevel, consumerName, isDurable, startMessageIdData, metadata, readCompacted, conf.ReplicateSubscriptionState, PulsarApi.CommandSubscribe.InitialPosition.valueOf(subscriptionInitialPosition.Value), startMessageRollbackDuration, si, createTopicIfDoesNotExist, conf.KeySharedPolicy);
			if (startMessageIdData != null)
			{
				startMessageIdData.recycle();
			}

			cnx.sendRequestWithId(request, requestId).thenRun(() =>
			{
			lock (ConsumerImpl.this)
			{
				if (changeToReadyState())
				{
					consumerIsReconnectedToBroker(cnx, currentSize);
				}
				else
				{
					State = State.Closed;
					cnx.removeConsumer(consumerId);
					cnx.channel().close();
					return;
				}
			}
			resetBackoff();
			bool firstTimeConnect = subscribeFuture_Conflict.complete(this);
			if (!(firstTimeConnect && hasParentConsumer && isDurable) && conf.ReceiverQueueSize != 0)
			{
				sendFlowPermitsToBroker(cnx, conf.ReceiverQueueSize);
			}
			}).exceptionally((e) =>
			{
			cnx.removeConsumer(consumerId);
			if (State == State.Closing || State == State.Closed)
			{
				cnx.channel().close();
				return null;
			}
			log.warn("[{}][{}] Failed to subscribe to topic on {}", topic, subscription, cnx.channel().remoteAddress());
			if (e.Cause is PulsarClientException && ConnectionHandler.isRetriableError((PulsarClientException) e.Cause) && DateTimeHelper.CurrentUnixTimeMillis() < subscribeTimeout)
			{
				reconnectLater(e.Cause);
			}
			else if (!subscribeFuture_Conflict.Done)
			{
				State = State.Failed;
				closeConsumerTasks();
				subscribeFuture_Conflict.completeExceptionally(PulsarClientException.wrap(e, string.Format("Failed to subscribe the topic {0} with subscription " + "name {1} when connecting to the broker", topicName.ToString(), subscription)));
				client.cleanupConsumer(this);
			}
			else if (e.Cause is TopicDoesNotExistException)
			{
				State = State.Failed;
				client.cleanupConsumer(this);
				log.warn("[{}][{}] Closed consumer because topic does not exist anymore {}", topic, subscription, cnx.channel().remoteAddress());
			}
			else
			{
				reconnectLater(e.Cause);
			}
			return null;
		});
		}

		protected internal virtual void consumerIsReconnectedToBroker(ClientCnx cnx, int currentQueueSize)
		{
			log.info("[{}][{}] Subscribed to topic on {} -- consumer: {}", topic, subscription, cnx.channel().remoteAddress(), consumerId);

			AVAILABLE_PERMITS_UPDATER.set(this, 0);
		}

		/// <summary>
		/// Clear the internal receiver queue and returns the message id of what was the 1st message in the queue that was
		/// not seen by the application
		/// </summary>
		private BatchMessageIdImpl clearReceiverQueue()
		{
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.List<org.apache.pulsar.client.api.Message<?>> currentMessageQueue = new java.util.ArrayList<>(incomingMessages.size());
			IList<Message<object>> currentMessageQueue = new List<Message<object>>(incomingMessages.size());
			incomingMessages.drainTo(currentMessageQueue);
			INCOMING_MESSAGES_SIZE_UPDATER.set(this, 0);
			if (currentMessageQueue.Count > 0)
			{
				MessageIdImpl nextMessageInQueue = (MessageIdImpl) currentMessageQueue[0].MessageId;
				BatchMessageIdImpl previousMessage;
				if (nextMessageInQueue is BatchMessageIdImpl)
				{
					// Get on the previous message within the current batch
					previousMessage = new BatchMessageIdImpl(nextMessageInQueue.LedgerId, nextMessageInQueue.EntryId, nextMessageInQueue.PartitionIndex, ((BatchMessageIdImpl) nextMessageInQueue).BatchIndex - 1);
				}
				else
				{
					// Get on previous message in previous entry
					previousMessage = new BatchMessageIdImpl(nextMessageInQueue.LedgerId, nextMessageInQueue.EntryId - 1, nextMessageInQueue.PartitionIndex, -1);
				}

				return previousMessage;
			}
			else if (!lastDequeuedMessage.Equals(MessageId.earliest))
			{
				// If the queue was empty we need to restart from the message just after the last one that has been dequeued
				// in the past
				return new BatchMessageIdImpl((MessageIdImpl) lastDequeuedMessage);
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
		internal virtual void sendFlowPermitsToBroker(ClientCnx cnx, int numMessages)
		{
			if (cnx != null)
			{
				if (log.DebugEnabled)
				{
					log.debug("[{}] [{}] Adding {} additional permits", topic, subscription, numMessages);
				}

				cnx.ctx().writeAndFlush(Commands.newFlow(consumerId, numMessages), cnx.ctx().voidPromise());
			}
		}

		public virtual void connectionFailed(PulsarClientException exception)
		{
			if (DateTimeHelper.CurrentUnixTimeMillis() > subscribeTimeout && subscribeFuture_Conflict.completeExceptionally(exception))
			{
				State = State.Failed;
				log.info("[{}] Consumer creation failed for consumer {}", topic, consumerId);
				client.cleanupConsumer(this);
			}
		}

		public override CompletableFuture<Void> closeAsync()
		{
			if (State == State.Closing || State == State.Closed)
			{
				closeConsumerTasks();
				return CompletableFuture.completedFuture(null);
			}

			if (!Connected)
			{
				log.info("[{}] [{}] Closed Consumer (not connected)", topic, subscription);
				State = State.Closed;
				closeConsumerTasks();
				client.cleanupConsumer(this);
				return CompletableFuture.completedFuture(null);
			}

			stats.StatTimeout.ifPresent(Timeout.cancel);

			State = State.Closing;

			closeConsumerTasks();

			long requestId = client.newRequestId();

			CompletableFuture<Void> closeFuture = new CompletableFuture<Void>();
			ClientCnx cnx = cnx();
			if (null == cnx)
			{
				cleanupAtClose(closeFuture);
			}
			else
			{
				ByteBuf cmd = Commands.newCloseConsumer(consumerId, requestId);
				cnx.sendRequestWithId(cmd, requestId).handle((v, exception) =>
				{
				cnx.removeConsumer(consumerId);
				if (exception == null || !cnx.ctx().channel().Active)
				{
					cleanupAtClose(closeFuture);
				}
				else
				{
					closeFuture.completeExceptionally(exception);
				}
				return null;
				});
			}

			return closeFuture;
		}

		private void cleanupAtClose(CompletableFuture<Void> closeFuture)
		{
			log.info("[{}] [{}] Closed consumer", topic, subscription);
			State = State.Closed;
			closeConsumerTasks();
			closeFuture.complete(null);
			client.cleanupConsumer(this);
			// fail all pending-receive futures to notify application
			failPendingReceive();
		}

		private void closeConsumerTasks()
		{
			unAckedMessageTracker.Dispose();
			if (possibleSendToDeadLetterTopicMessages != null)
			{
				possibleSendToDeadLetterTopicMessages.Clear();
			}

			acknowledgmentsGroupingTracker.close();
		}

		private void failPendingReceive()
		{
			@lock.readLock().@lock();
			try
			{
				if (listenerExecutor != null && !listenerExecutor.Shutdown)
				{
					while (!pendingReceives.Empty)
					{
						CompletableFuture<Message<T>> receiveFuture = pendingReceives.poll();
						if (receiveFuture != null)
						{
							receiveFuture.completeExceptionally(new PulsarClientException.AlreadyClosedException(string.Format("The consumer which subscribes the topic {0} with subscription name {1} " + "was already closed when cleaning and closing the consumers", topicName.ToString(), subscription)));
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

		internal virtual void activeConsumerChanged(bool isActive)
		{
			if (consumerEventListener == null)
			{
				return;
			}

			listenerExecutor.execute(() =>
			{
			if (isActive)
			{
				consumerEventListener.becameActive(this, partitionIndex);
			}
			else
			{
				consumerEventListener.becameInactive(this, partitionIndex);
			}
			});
		}

		internal virtual void messageReceived(PulsarApi.MessageIdData messageId, int redeliveryCount, ByteBuf headersAndPayload, ClientCnx cnx)
		{
			if (log.DebugEnabled)
			{
				log.debug("[{}][{}] Received message: {}/{}", topic, subscription, messageId.LedgerId, messageId.EntryId);
			}

			if (!verifyChecksum(headersAndPayload, messageId))
			{
				// discard message with checksum error
				discardCorruptedMessage(messageId, cnx, PulsarApi.CommandAck.ValidationError.ChecksumMismatch);
				return;
			}

			PulsarApi.MessageMetadata msgMetadata;
			try
			{
				msgMetadata = Commands.parseMessageMetadata(headersAndPayload);
			}
			catch (Exception)
			{
				discardCorruptedMessage(messageId, cnx, PulsarApi.CommandAck.ValidationError.ChecksumMismatch);
				return;
			}

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int numMessages = msgMetadata.getNumMessagesInBatch();
			int numMessages = msgMetadata.NumMessagesInBatch;

			MessageIdImpl msgId = new MessageIdImpl(messageId.LedgerId, messageId.EntryId, PartitionIndex);
			if (acknowledgmentsGroupingTracker.isDuplicate(msgId))
			{
				if (log.DebugEnabled)
				{
					log.debug("[{}] [{}] Ignoring message as it was already being acked earlier by same consumer {}/{}", topic, subscription, consumerName, msgId);
				}

				increaseAvailablePermits(cnx, numMessages);
				return;
			}

			ByteBuf decryptedPayload = decryptPayloadIfNeeded(messageId, msgMetadata, headersAndPayload, cnx);

			bool isMessageUndecryptable = isMessageUndecryptable(msgMetadata);

			if (decryptedPayload == null)
			{
				// Message was discarded or CryptoKeyReader isn't implemented
				return;
			}

			// uncompress decryptedPayload and release decryptedPayload-ByteBuf
			ByteBuf uncompressedPayload = isMessageUndecryptable ? decryptedPayload.retain() : uncompressPayloadIfNeeded(messageId, msgMetadata, decryptedPayload, cnx);
			decryptedPayload.release();
			if (uncompressedPayload == null)
			{
				// Message was discarded on decompression error
				return;
			}

			// if message is not decryptable then it can't be parsed as a batch-message. so, add EncyrptionCtx to message
			// and return undecrypted payload
			if (isMessageUndecryptable || (numMessages == 1 && !msgMetadata.hasNumMessagesInBatch()))
			{

				if (isResetIncludedAndSameEntryLedger(messageId) && isPriorEntryIndex(messageId.EntryId))
				{
					// We need to discard entries that were prior to startMessageId
					if (log.DebugEnabled)
					{
						log.debug("[{}] [{}] Ignoring message from before the startMessageId: {}", subscription, consumerName, startMessageId);
					}

					uncompressedPayload.release();
					msgMetadata.recycle();
					return;
				}

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final MessageImpl<T> message = new MessageImpl<>(topicName.toString(), msgId, msgMetadata, uncompressedPayload, createEncryptionContext(msgMetadata), cnx, schema, redeliveryCount);
				MessageImpl<T> message = new MessageImpl<T>(topicName.ToString(), msgId, msgMetadata, uncompressedPayload, createEncryptionContext(msgMetadata), cnx, schema, redeliveryCount);
				uncompressedPayload.release();
				msgMetadata.recycle();

				@lock.readLock().@lock();
				try
				{
					// Enqueue the message so that it can be retrieved when application calls receive()
					// if the conf.getReceiverQueueSize() is 0 then discard message if no one is waiting for it.
					// if asyncReceive is waiting then notify callback without adding to incomingMessages queue
					if (deadLetterPolicy != null && possibleSendToDeadLetterTopicMessages != null && redeliveryCount >= deadLetterPolicy.MaxRedeliverCount)
					{
						possibleSendToDeadLetterTopicMessages[(MessageIdImpl)message.getMessageId()] = Collections.singletonList(message);
					}
					if (!pendingReceives.Empty)
					{
						notifyPendingReceivedCallback(message, null);
					}
					else if (enqueueMessageAndCheckBatchReceive(message))
					{
						if (hasPendingBatchReceive())
						{
							notifyPendingBatchReceivedCallBack();
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
				receiveIndividualMessagesFromBatch(msgMetadata, redeliveryCount, uncompressedPayload, messageId, cnx);

				uncompressedPayload.release();
				msgMetadata.recycle();
			}

			if (listener != null)
			{
				triggerListener(numMessages);
			}
		}

		protected internal virtual void triggerListener(int numMessages)
		{
			// Trigger the notification on the message listener in a separate thread to avoid blocking the networking
			// thread while the message processing happens
			listenerExecutor.execute(() =>
			{
			for (int i = 0; i < numMessages; i++)
			{
				try
				{
					Message<T> msg = internalReceive(0, TimeUnit.MILLISECONDS);
					if (msg == null)
					{
						if (log.DebugEnabled)
						{
							log.debug("[{}] [{}] Message has been cleared from the queue", topic, subscription);
						}
						break;
					}
					try
					{
						if (log.DebugEnabled)
						{
							log.debug("[{}][{}] Calling message listener for message {}", topic, subscription, msg.MessageId);
						}
						listener.received(ConsumerImpl.this, msg);
					}
					catch (Exception t)
					{
						log.error("[{}][{}] Message listener error in processing message: {}", topic, subscription, msg.MessageId, t);
					}
				}
				catch (PulsarClientException e)
				{
					log.warn("[{}] [{}] Failed to dequeue the message for listener", topic, subscription, e);
					return;
				}
			}
			});
		}

		/// <summary>
		/// Notify waiting asyncReceive request with the received message
		/// </summary>
		/// <param name="message"> </param>
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: void notifyPendingReceivedCallback(final org.apache.pulsar.client.api.Message<T> message, Exception exception)
		internal virtual void notifyPendingReceivedCallback(Message<T> message, Exception exception)
		{
			if (pendingReceives.Empty)
			{
				return;
			}

			// fetch receivedCallback from queue
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.CompletableFuture<org.apache.pulsar.client.api.Message<T>> receivedFuture = pendingReceives.poll();
			CompletableFuture<Message<T>> receivedFuture = pendingReceives.poll();
			if (receivedFuture == null)
			{
				return;
			}

			if (exception != null)
			{
				listenerExecutor.execute(() => receivedFuture.completeExceptionally(exception));
				return;
			}

			if (message == null)
			{
				System.InvalidOperationException e = new System.InvalidOperationException("received message can't be null");
				listenerExecutor.execute(() => receivedFuture.completeExceptionally(e));
				return;
			}

			if (conf.ReceiverQueueSize == 0)
			{
				// call interceptor and complete received callback
				interceptAndComplete(message, receivedFuture);
				return;
			}

			// increase permits for available message-queue
			messageProcessed(message);
			// call interceptor and complete received callback
			interceptAndComplete(message, receivedFuture);
		}

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private void interceptAndComplete(final org.apache.pulsar.client.api.Message<T> message, final java.util.concurrent.CompletableFuture<org.apache.pulsar.client.api.Message<T>> receivedFuture)
		private void interceptAndComplete(Message<T> message, CompletableFuture<Message<T>> receivedFuture)
		{
			// call proper interceptor
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.apache.pulsar.client.api.Message<T> interceptMessage = beforeConsume(message);
			Message<T> interceptMessage = beforeConsume(message);
			// return message to receivedCallback
			listenerExecutor.execute(() => receivedFuture.complete(interceptMessage));
		}

		internal virtual void receiveIndividualMessagesFromBatch(PulsarApi.MessageMetadata msgMetadata, int redeliveryCount, ByteBuf uncompressedPayload, PulsarApi.MessageIdData messageId, ClientCnx cnx)
		{
			int batchSize = msgMetadata.NumMessagesInBatch;

			// create ack tracker for entry aka batch
			MessageIdImpl batchMessage = new MessageIdImpl(messageId.LedgerId, messageId.EntryId, PartitionIndex);
			BatchMessageAcker acker = BatchMessageAcker.newAcker(batchSize);
			IList<MessageImpl<T>> possibleToDeadLetter = null;
			if (deadLetterPolicy != null && redeliveryCount >= deadLetterPolicy.MaxRedeliverCount)
			{
				possibleToDeadLetter = new List<MessageImpl<T>>();
			}
			int skippedMessages = 0;
			try
			{
				for (int i = 0; i < batchSize; ++i)
				{
					if (log.DebugEnabled)
					{
						log.debug("[{}] [{}] processing message num - {} in batch", subscription, consumerName, i);
					}
					PulsarApi.SingleMessageMetadata.Builder singleMessageMetadataBuilder = PulsarApi.SingleMessageMetadata.newBuilder();
					ByteBuf singleMessagePayload = Commands.deSerializeSingleMessageInBatch(uncompressedPayload, singleMessageMetadataBuilder, i, batchSize);

					if (isResetIncludedAndSameEntryLedger(messageId) && isPriorBatchIndex(i))
					{
						// If we are receiving a batch message, we need to discard messages that were prior
						// to the startMessageId
						if (log.DebugEnabled)
						{
							log.debug("[{}] [{}] Ignoring message from before the startMessageId: {}", subscription, consumerName, startMessageId);
						}
						singleMessagePayload.release();
						singleMessageMetadataBuilder.recycle();

						++skippedMessages;
						continue;
					}

					if (singleMessageMetadataBuilder.CompactedOut)
					{
						// message has been compacted out, so don't send to the user
						singleMessagePayload.release();
						singleMessageMetadataBuilder.recycle();

						++skippedMessages;
						continue;
					}

					BatchMessageIdImpl batchMessageIdImpl = new BatchMessageIdImpl(messageId.LedgerId, messageId.EntryId, PartitionIndex, i, acker);
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final MessageImpl<T> message = new MessageImpl<>(topicName.toString(), batchMessageIdImpl, msgMetadata, singleMessageMetadataBuilder.build(), singleMessagePayload, createEncryptionContext(msgMetadata), cnx, schema, redeliveryCount);
					MessageImpl<T> message = new MessageImpl<T>(topicName.ToString(), batchMessageIdImpl, msgMetadata, singleMessageMetadataBuilder.build(), singleMessagePayload, createEncryptionContext(msgMetadata), cnx, schema, redeliveryCount);
					if (possibleToDeadLetter != null)
					{
						possibleToDeadLetter.Add(message);
					}
					@lock.readLock().@lock();
					try
					{
						if (!pendingReceives.Empty)
						{
							notifyPendingReceivedCallback(message, null);
						}
						else if (enqueueMessageAndCheckBatchReceive(message))
						{
							if (hasPendingBatchReceive())
							{
								notifyPendingBatchReceivedCallBack();
							}
						}
					}
					finally
					{
						@lock.readLock().unlock();
					}
					singleMessagePayload.release();
					singleMessageMetadataBuilder.recycle();
				}
			}
			catch (IOException)
			{
				log.warn("[{}] [{}] unable to obtain message in batch", subscription, consumerName);
				discardCorruptedMessage(messageId, cnx, PulsarApi.CommandAck.ValidationError.BatchDeSerializeError);
			}

			if (possibleToDeadLetter != null && possibleSendToDeadLetterTopicMessages != null)
			{
				possibleSendToDeadLetterTopicMessages[batchMessage] = possibleToDeadLetter;
			}

			if (log.DebugEnabled)
			{
				log.debug("[{}] [{}] enqueued messages in batch. queue size - {}, available queue size - {}", subscription, consumerName, incomingMessages.size(), incomingMessages.remainingCapacity());
			}

			if (skippedMessages > 0)
			{
				increaseAvailablePermits(cnx, skippedMessages);
			}
		}

		private bool isPriorEntryIndex(long idx)
		{
			return resetIncludeHead ? idx < startMessageId.EntryId : idx <= startMessageId.EntryId;
		}

		private bool isPriorBatchIndex(long idx)
		{
			return resetIncludeHead ? idx < startMessageId.BatchIndex : idx <= startMessageId.BatchIndex;
		}

		private bool isResetIncludedAndSameEntryLedger(PulsarApi.MessageIdData messageId)
		{
			return !resetIncludeHead && startMessageId != null && messageId.LedgerId == startMessageId.LedgerId && messageId.EntryId == startMessageId.EntryId;
		}

		/// <summary>
		/// Record the event that one message has been processed by the application.
		/// 
		/// Periodically, it sends a Flow command to notify the broker that it can push more messages
		/// </summary>
		protected internal override void messageProcessed<T1>(Message<T1> msg)
		{
			lock (this)
			{
				ClientCnx currentCnx = cnx();
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: ClientCnx msgCnx = ((MessageImpl<?>) msg).getCnx();
				ClientCnx msgCnx = ((MessageImpl<object>) msg).Cnx;
				lastDequeuedMessage = msg.MessageId;
        
				if (msgCnx != currentCnx)
				{
					// The processed message did belong to the old queue that was cleared after reconnection.
					return;
				}
        
				increaseAvailablePermits(currentCnx);
				stats.updateNumMsgsReceived(msg);
        
				trackMessage(msg);
				INCOMING_MESSAGES_SIZE_UPDATER.addAndGet(this, -msg.Data.length);
			}
		}

		protected internal virtual void trackMessage<T1>(Message<T1> msg)
		{
			if (msg != null)
			{
				MessageId messageId = msg.MessageId;
				if (conf.AckTimeoutMillis > 0 && messageId is MessageIdImpl)
				{
					MessageIdImpl id = (MessageIdImpl)messageId;
					if (id is BatchMessageIdImpl)
					{
						// do not add each item in batch message into tracker
						id = new MessageIdImpl(id.LedgerId, id.EntryId, PartitionIndex);
					}
					if (hasParentConsumer)
					{
						// we should no longer track this message, TopicsConsumer will take care from now onwards
						unAckedMessageTracker.remove(id);
					}
					else
					{
						unAckedMessageTracker.add(id);
					}
				}
			}
		}

		internal virtual void increaseAvailablePermits(ClientCnx currentCnx)
		{
			increaseAvailablePermits(currentCnx, 1);
		}

		private void increaseAvailablePermits(ClientCnx currentCnx, int delta)
		{
			int available = AVAILABLE_PERMITS_UPDATER.addAndGet(this, delta);

			while (available >= receiverQueueRefillThreshold && !paused)
			{
				if (AVAILABLE_PERMITS_UPDATER.compareAndSet(this, available, 0))
				{
					sendFlowPermitsToBroker(currentCnx, available);
					break;
				}
				else
				{
					available = AVAILABLE_PERMITS_UPDATER.get(this);
				}
			}
		}

		public override void pause()
		{
			paused = true;
		}

		public override void resume()
		{
			if (paused)
			{
				paused = false;
				increaseAvailablePermits(cnx(), 0);
			}
		}

		private ByteBuf decryptPayloadIfNeeded(PulsarApi.MessageIdData messageId, PulsarApi.MessageMetadata msgMetadata, ByteBuf payload, ClientCnx currentCnx)
		{

			if (msgMetadata.EncryptionKeysCount == 0)
			{
				return payload.retain();
			}

			// If KeyReader is not configured throw exception based on config param
			if (conf.CryptoKeyReader == null)
			{
				switch (conf.CryptoFailureAction)
				{
					case CONSUME:
						log.warn("[{}][{}][{}] CryptoKeyReader interface is not implemented. Consuming encrypted message.", topic, subscription, consumerName);
						return payload.retain();
					case DISCARD:
						log.warn("[{}][{}][{}] Skipping decryption since CryptoKeyReader interface is not implemented and config is set to discard", topic, subscription, consumerName);
						discardMessage(messageId, currentCnx, PulsarApi.CommandAck.ValidationError.DecryptionError);
						return null;
					case FAIL:
						MessageId m = new MessageIdImpl(messageId.LedgerId, messageId.EntryId, partitionIndex);
						log.error("[{}][{}][{}][{}] Message delivery failed since CryptoKeyReader interface is not implemented to consume encrypted message", topic, subscription, consumerName, m);
						unAckedMessageTracker.add(m);
						return null;
				}
			}

			ByteBuf decryptedData = this.msgCrypto.decrypt(msgMetadata, payload, conf.CryptoKeyReader);
			if (decryptedData != null)
			{
				return decryptedData;
			}

			switch (conf.CryptoFailureAction)
			{
				case CONSUME:
					// Note, batch message will fail to consume even if config is set to consume
					log.warn("[{}][{}][{}][{}] Decryption failed. Consuming encrypted message since config is set to consume.", topic, subscription, consumerName, messageId);
					return payload.retain();
				case DISCARD:
					log.warn("[{}][{}][{}][{}] Discarding message since decryption failed and config is set to discard", topic, subscription, consumerName, messageId);
					discardMessage(messageId, currentCnx, PulsarApi.CommandAck.ValidationError.DecryptionError);
					return null;
				case FAIL:
					MessageId m = new MessageIdImpl(messageId.LedgerId, messageId.EntryId, partitionIndex);
					log.error("[{}][{}][{}][{}] Message delivery failed since unable to decrypt incoming message", topic, subscription, consumerName, m);
					unAckedMessageTracker.add(m);
					return null;
			}
			return null;
		}

		private ByteBuf uncompressPayloadIfNeeded(PulsarApi.MessageIdData messageId, PulsarApi.MessageMetadata msgMetadata, ByteBuf payload, ClientCnx currentCnx)
		{
			PulsarApi.CompressionType compressionType = msgMetadata.Compression;
			CompressionCodec codec = CompressionCodecProvider.getCompressionCodec(compressionType);
			int uncompressedSize = msgMetadata.UncompressedSize;
			int payloadSize = payload.readableBytes();
			if (payloadSize > ClientCnx.MaxMessageSize)
			{
				// payload size is itself corrupted since it cannot be bigger than the MaxMessageSize
				log.error("[{}][{}] Got corrupted payload message size {} at {}", topic, subscription, payloadSize, messageId);
				discardCorruptedMessage(messageId, currentCnx, PulsarApi.CommandAck.ValidationError.UncompressedSizeCorruption);
				return null;
			}

			try
			{
				ByteBuf uncompressedPayload = codec.decode(payload, uncompressedSize);
				return uncompressedPayload;
			}
			catch (IOException e)
			{
				log.error("[{}][{}] Failed to decompress message with {} at {}: {}", topic, subscription, compressionType, messageId, e.Message, e);
				discardCorruptedMessage(messageId, currentCnx, PulsarApi.CommandAck.ValidationError.DecompressionError);
				return null;
			}
		}

		private bool verifyChecksum(ByteBuf headersAndPayload, PulsarApi.MessageIdData messageId)
		{

			if (hasChecksum(headersAndPayload))
			{
				int checksum = readChecksum(headersAndPayload);
				int computedChecksum = computeChecksum(headersAndPayload);
				if (checksum != computedChecksum)
				{
					log.error("[{}][{}] Checksum mismatch for message at {}:{}. Received checksum: 0x{}, Computed checksum: 0x{}", topic, subscription, messageId.LedgerId, messageId.EntryId, checksum.ToString("x"), computedChecksum.ToString("x"));
					return false;
				}
			}

			return true;
		}

		private void discardCorruptedMessage(PulsarApi.MessageIdData messageId, ClientCnx currentCnx, PulsarApi.CommandAck.ValidationError validationError)
		{
			log.error("[{}][{}] Discarding corrupted message at {}:{}", topic, subscription, messageId.LedgerId, messageId.EntryId);
			discardMessage(messageId, currentCnx, validationError);
		}

		private void discardMessage(PulsarApi.MessageIdData messageId, ClientCnx currentCnx, PulsarApi.CommandAck.ValidationError validationError)
		{
			ByteBuf cmd = Commands.newAck(consumerId, messageId.LedgerId, messageId.EntryId, PulsarApi.CommandAck.AckType.Individual, validationError, Collections.emptyMap());
			currentCnx.ctx().writeAndFlush(cmd, currentCnx.ctx().voidPromise());
			increaseAvailablePermits(currentCnx);
			stats.incrementNumReceiveFailed();
		}

		internal override string HandlerName
		{
			get
			{
				return subscription;
			}
		}

		public override bool Connected
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
				return partitionIndex;
			}
		}

		public override int AvailablePermits
		{
			get
			{
				return AVAILABLE_PERMITS_UPDATER.get(this);
			}
		}

		public override int numMessagesInQueue()
		{
			return incomingMessages.size();
		}

		public override void redeliverUnacknowledgedMessages()
		{
			ClientCnx cnx = cnx();
			if (Connected && cnx.RemoteEndpointProtocolVersion >= PulsarApi.ProtocolVersion.v2.Number)
			{
				int currentSize = 0;
				lock (this)
				{
					currentSize = incomingMessages.size();
					incomingMessages.clear();
					INCOMING_MESSAGES_SIZE_UPDATER.set(this, 0);
					unAckedMessageTracker.clear();
				}
				cnx.ctx().writeAndFlush(Commands.newRedeliverUnacknowledgedMessages(consumerId), cnx.ctx().voidPromise());
				if (currentSize > 0)
				{
					increaseAvailablePermits(cnx, currentSize);
				}
				if (log.DebugEnabled)
				{
					log.debug("[{}] [{}] [{}] Redeliver unacked messages and send {} permits", subscription, topic, consumerName, currentSize);
				}
				return;
			}
			if (cnx == null || (State == State.Connecting))
			{
				log.warn("[{}] Client Connection needs to be established for redelivery of unacknowledged messages", this);
			}
			else
			{
				log.warn("[{}] Reconnecting the client to redeliver the messages.", this);
				cnx.ctx().close();
			}
		}

		public override void redeliverUnacknowledgedMessages(ISet<MessageId> messageIds)
		{
			if (messageIds.Count == 0)
			{
				return;
			}

			checkArgument(messageIds.First().get() is MessageIdImpl);

			if (conf.SubscriptionType != SubscriptionType.Shared && conf.SubscriptionType != SubscriptionType.Key_Shared)
			{
				// We cannot redeliver single messages if subscription type is not Shared
				redeliverUnacknowledgedMessages();
				return;
			}
			ClientCnx cnx = cnx();
			if (Connected && cnx.RemoteEndpointProtocolVersion >= PulsarApi.ProtocolVersion.v2.Number)
			{
				int messagesFromQueue = removeExpiredMessagesFromQueue(messageIds);
//JAVA TO C# CONVERTER TODO TASK: Most Java stream collectors are not converted by Java to C# Converter:
				IEnumerable<IList<MessageIdImpl>> batches = Iterables.partition(messageIds.Select(messageId => (MessageIdImpl)messageId).collect(Collectors.toSet()), MAX_REDELIVER_UNACKNOWLEDGED);
				PulsarApi.MessageIdData.Builder builder = PulsarApi.MessageIdData.newBuilder();
				batches.forEach(ids =>
				{
				IList<MessageIdData> messageIdDatas = ids.Where(messageId => !processPossibleToDLQ(messageId)).Select(messageId =>
				{
					builder.Partition = messageId.PartitionIndex;
					builder.LedgerId = messageId.LedgerId;
					builder.EntryId = messageId.EntryId;
					return builder.build();
				}).ToList();
				ByteBuf cmd = Commands.newRedeliverUnacknowledgedMessages(consumerId, messageIdDatas);
				cnx.ctx().writeAndFlush(cmd, cnx.ctx().voidPromise());
				messageIdDatas.forEach(MessageIdData.recycle);
				});
				if (messagesFromQueue > 0)
				{
					increaseAvailablePermits(cnx, messagesFromQueue);
				}
				builder.recycle();
				if (log.DebugEnabled)
				{
					log.debug("[{}] [{}] [{}] Redeliver unacked messages and increase {} permits", subscription, topic, consumerName, messagesFromQueue);
				}
				return;
			}
			if (cnx == null || (State == State.Connecting))
			{
				log.warn("[{}] Client Connection needs to be established for redelivery of unacknowledged messages", this);
			}
			else
			{
				log.warn("[{}] Reconnecting the client to redeliver the messages.", this);
				cnx.ctx().close();
			}
		}

		protected internal override void completeOpBatchReceive(OpBatchReceive<T> op)
		{
			notifyPendingBatchReceivedCallBack(op);
		}

		private bool processPossibleToDLQ(MessageIdImpl messageId)
		{
			IList<MessageImpl<T>> deadLetterMessages = null;
			if (possibleSendToDeadLetterTopicMessages != null)
			{
				if (messageId is BatchMessageIdImpl)
				{
					deadLetterMessages = possibleSendToDeadLetterTopicMessages[new MessageIdImpl(messageId.LedgerId, messageId.EntryId, PartitionIndex)];
				}
				else
				{
					deadLetterMessages = possibleSendToDeadLetterTopicMessages[messageId];
				}
			}
			if (deadLetterMessages != null)
			{
				if (deadLetterProducer == null)
				{
					try
					{
						deadLetterProducer = client.newProducer(schema).topic(this.deadLetterPolicy.DeadLetterTopic).blockIfQueueFull(false).create();
					}
					catch (Exception e)
					{
						log.error("Create dead letter producer exception with topic: {}", deadLetterPolicy.DeadLetterTopic, e);
					}
				}
				if (deadLetterProducer != null)
				{
					try
					{
						foreach (MessageImpl<T> message in deadLetterMessages)
						{
							deadLetterProducer.newMessage().value(message.Value).properties(message.Properties).send();
						}
						acknowledge(messageId);
						return true;
					}
					catch (Exception e)
					{
						log.error("Send to dead letter topic exception with topic: {}, messageId: {}", deadLetterProducer.Topic, messageId, e);
					}
				}
			}
			return false;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void seek(org.apache.pulsar.client.api.MessageId messageId) throws org.apache.pulsar.client.api.PulsarClientException
		public override void seek(MessageId messageId)
		{
			try
			{
				seekAsync(messageId).get();
			}
			catch (Exception e)
			{
				throw PulsarClientException.unwrap(e);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void seek(long timestamp) throws org.apache.pulsar.client.api.PulsarClientException
		public override void seek(long timestamp)
		{
			try
			{
				seekAsync(timestamp).get();
			}
			catch (Exception e)
			{
				throw PulsarClientException.unwrap(e);
			}
		}

		public override CompletableFuture<Void> seekAsync(long timestamp)
		{
			if (State == State.Closing || State == State.Closed)
			{
				return FutureUtil.failedFuture(new PulsarClientException.AlreadyClosedException(string.Format("The consumer {0} was already closed when seeking the subscription {1} of the topic " + "{2} to the timestamp {3:D}", consumerName, subscription, topicName.ToString(), timestamp)));
			}

			if (!Connected)
			{
				return FutureUtil.failedFuture(new PulsarClientException(string.Format("The client is not connected to the broker when seeking the subscription {0} of the " + "topic {1} to the timestamp {2:D}", subscription, topicName.ToString(), timestamp)));
			}

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.CompletableFuture<Void> seekFuture = new java.util.concurrent.CompletableFuture<>();
			CompletableFuture<Void> seekFuture = new CompletableFuture<Void>();

			long requestId = client.newRequestId();
			ByteBuf seek = Commands.newSeek(consumerId, requestId, timestamp);
			ClientCnx cnx = cnx();

			log.info("[{}][{}] Seek subscription to publish time {}", topic, subscription, timestamp);

			cnx.sendRequestWithId(seek, requestId).thenRun(() =>
			{
			log.info("[{}][{}] Successfully reset subscription to publish time {}", topic, subscription, timestamp);
			acknowledgmentsGroupingTracker.flushAndClean();
			lastDequeuedMessage = MessageId.earliest;
			incomingMessages.clear();
			INCOMING_MESSAGES_SIZE_UPDATER.set(this, 0);
			seekFuture.complete(null);
			}).exceptionally(e =>
			{
			log.error("[{}][{}] Failed to reset subscription: {}", topic, subscription, e.Cause.Message);
			seekFuture.completeExceptionally(PulsarClientException.wrap(e.Cause, string.Format("Failed to seek the subscription {0} of the topic {1} to the timestamp {2:D}", subscription, topicName.ToString(), timestamp)));
			return null;
		});
			return seekFuture;
		}

		public override CompletableFuture<Void> seekAsync(MessageId messageId)
		{
			if (State == State.Closing || State == State.Closed)
			{
				return FutureUtil.failedFuture(new PulsarClientException.AlreadyClosedException(string.Format("The consumer {0} was already closed when seeking the subscription {1} of the topic " + "{2} to the message {3}", consumerName, subscription, topicName.ToString(), messageId.ToString())));
			}

			if (!Connected)
			{
				return FutureUtil.failedFuture(new PulsarClientException(string.Format("The client is not connected to the broker when seeking the subscription {0} of the " + "topic {1} to the message {2}", subscription, topicName.ToString(), messageId.ToString())));
			}

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.CompletableFuture<Void> seekFuture = new java.util.concurrent.CompletableFuture<>();
			CompletableFuture<Void> seekFuture = new CompletableFuture<Void>();

			long requestId = client.newRequestId();
			MessageIdImpl msgId = (MessageIdImpl) messageId;
			ByteBuf seek = Commands.newSeek(consumerId, requestId, msgId.LedgerId, msgId.EntryId);
			ClientCnx cnx = cnx();

			log.info("[{}][{}] Seek subscription to message id {}", topic, subscription, messageId);

			cnx.sendRequestWithId(seek, requestId).thenRun(() =>
			{
			log.info("[{}][{}] Successfully reset subscription to message id {}", topic, subscription, messageId);
			acknowledgmentsGroupingTracker.flushAndClean();
			lastDequeuedMessage = messageId;
			incomingMessages.clear();
			INCOMING_MESSAGES_SIZE_UPDATER.set(this, 0);
			seekFuture.complete(null);
			}).exceptionally(e =>
			{
			log.error("[{}][{}] Failed to reset subscription: {}", topic, subscription, e.Cause.Message);
			seekFuture.completeExceptionally(PulsarClientException.wrap(e.Cause, string.Format("[{0}][{1}] Failed to seek the subscription {2} of the topic {3} to the message {4}", subscription, topicName.ToString(), messageId.ToString())));
			return null;
		});
			return seekFuture;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean hasMessageAvailable() throws org.apache.pulsar.client.api.PulsarClientException
		public virtual bool hasMessageAvailable()
		{
			// we need to seek to the last position then the last message can be received when the resetIncludeHead
			// specified.
			if (lastDequeuedMessage == MessageId.latest && resetIncludeHead)
			{
				lastDequeuedMessage = LastMessageId;
				seek(lastDequeuedMessage);
			}
			try
			{
				if (hasMoreMessages(lastMessageIdInBroker, lastDequeuedMessage))
				{
					return true;
				}

				return hasMessageAvailableAsync().get();
			}
			catch (Exception e)
			{
				throw PulsarClientException.unwrap(e);
			}
		}

		public virtual CompletableFuture<bool> hasMessageAvailableAsync()
		{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.CompletableFuture<bool> booleanFuture = new java.util.concurrent.CompletableFuture<>();
			CompletableFuture<bool> booleanFuture = new CompletableFuture<bool>();

			if (hasMoreMessages(lastMessageIdInBroker, lastDequeuedMessage))
			{
				booleanFuture.complete(true);
			}
			else
			{
				LastMessageIdAsync.thenAccept(messageId =>
				{
				lastMessageIdInBroker = messageId;
				if (hasMoreMessages(lastMessageIdInBroker, lastDequeuedMessage))
				{
					booleanFuture.complete(true);
				}
				else
				{
					booleanFuture.complete(false);
				}
				}).exceptionally(e =>
				{
				log.error("[{}][{}] Failed getLastMessageId command", topic, subscription);
				booleanFuture.completeExceptionally(e.Cause);
				return null;
			});
			}
			return booleanFuture;
		}

		private bool hasMoreMessages(MessageId lastMessageIdInBroker, MessageId lastDequeuedMessage)
		{
			if (lastMessageIdInBroker.compareTo(lastDequeuedMessage) > 0 && ((MessageIdImpl)lastMessageIdInBroker).EntryId != -1)
			{
				return true;
			}
			else
			{
				// Make sure batching message can be read completely.
				return lastMessageIdInBroker.compareTo(lastDequeuedMessage) == 0 && incomingMessages.size() > 0;
			}
		}

		public override CompletableFuture<MessageId> LastMessageIdAsync
		{
			get
			{
				if (State == State.Closing || State == State.Closed)
				{
					return FutureUtil.failedFuture(new PulsarClientException.AlreadyClosedException(string.Format("The consumer {0} was already closed when the subscription {1} of the topic {2} " + "getting the last message id", consumerName, subscription, topicName.ToString())));
				}
    
				AtomicLong opTimeoutMs = new AtomicLong(client.Configuration.OperationTimeoutMs);
				Backoff backoff = (new BackoffBuilder()).setInitialTime(100, TimeUnit.MILLISECONDS).setMax(opTimeoutMs.get() * 2, TimeUnit.MILLISECONDS).setMandatoryStop(0, TimeUnit.MILLISECONDS).create();
    
				CompletableFuture<MessageId> getLastMessageIdFuture = new CompletableFuture<MessageId>();
    
				internalGetLastMessageIdAsync(backoff, opTimeoutMs, getLastMessageIdFuture);
				return getLastMessageIdFuture;
			}
		}

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private void internalGetLastMessageIdAsync(final Backoff backoff, final java.util.concurrent.atomic.AtomicLong remainingTime, java.util.concurrent.CompletableFuture<org.apache.pulsar.client.api.MessageId> future)
		private void internalGetLastMessageIdAsync(Backoff backoff, AtomicLong remainingTime, CompletableFuture<MessageId> future)
		{
			ClientCnx cnx = cnx();
			if (Connected && cnx != null)
			{
				if (!Commands.peerSupportsGetLastMessageId(cnx.RemoteEndpointProtocolVersion))
				{
					future.completeExceptionally(new PulsarClientException.NotSupportedException(string.Format("The command `GetLastMessageId` is not supported for the protocol version {0:D}. " + "The consumer is {1}, topic {2}, subscription {3}", cnx.RemoteEndpointProtocolVersion, consumerName, topicName.ToString(), subscription)));
				}

				long requestId = client.newRequestId();
				ByteBuf getLastIdCmd = Commands.newGetLastMessageId(consumerId, requestId);
				log.info("[{}][{}] Get topic last message Id", topic, subscription);

				cnx.sendGetLastMessageId(getLastIdCmd, requestId).thenAccept((result) =>
				{
				log.info("[{}][{}] Successfully getLastMessageId {}:{}", topic, subscription, result.LedgerId, result.EntryId);
				future.complete(new MessageIdImpl(result.LedgerId, result.EntryId, result.Partition));
				}).exceptionally(e =>
				{
				log.error("[{}][{}] Failed getLastMessageId command", topic, subscription);
				future.completeExceptionally(PulsarClientException.wrap(e.Cause, string.Format("The subscription {0} of the topic {1} gets the last message id was failed", subscription, topicName.ToString())));
				return null;
			});
			}
			else
			{
				long nextDelay = Math.Min(backoff.next(), remainingTime.get());
				if (nextDelay <= 0)
				{
					future.completeExceptionally(new PulsarClientException.TimeoutException(string.Format("The subscription {0} of the topic {1} could not get the last message id " + "withing configured timeout", subscription, topicName.ToString())));
					return;
				}

				((ScheduledExecutorService) listenerExecutor).schedule(() =>
				{
				log.warn("[{}] [{}] Could not get connection while getLastMessageId -- Will try again in {} ms", topic, HandlerName, nextDelay);
				remainingTime.addAndGet(-nextDelay);
				internalGetLastMessageIdAsync(backoff, remainingTime, future);
				}, nextDelay, TimeUnit.MILLISECONDS);
			}
		}

		private MessageIdImpl getMessageIdImpl<T1>(Message<T1> msg)
		{
			MessageIdImpl messageId = (MessageIdImpl) msg.MessageId;
			if (messageId is BatchMessageIdImpl)
			{
				// messageIds contain MessageIdImpl, not BatchMessageIdImpl
				messageId = new MessageIdImpl(messageId.LedgerId, messageId.EntryId, PartitionIndex);
			}
			return messageId;
		}


		private bool isMessageUndecryptable(PulsarApi.MessageMetadata msgMetadata)
		{
			return (msgMetadata.EncryptionKeysCount > 0 && conf.CryptoKeyReader == null && conf.CryptoFailureAction == ConsumerCryptoFailureAction.CONSUME);
		}

		/// <summary>
		/// Create EncryptionContext if message payload is encrypted
		/// </summary>
		/// <param name="msgMetadata"> </param>
		/// <returns> <seealso cref="Optional"/><<seealso cref="EncryptionContext"/>> </returns>
		private Optional<EncryptionContext> createEncryptionContext(PulsarApi.MessageMetadata msgMetadata)
		{

			EncryptionContext encryptionCtx = null;
			if (msgMetadata.EncryptionKeysCount > 0)
			{
				encryptionCtx = new EncryptionContext();
				IDictionary<string, EncryptionContext.EncryptionKey> keys = msgMetadata.EncryptionKeysList.ToDictionary(PulsarApi.EncryptionKeys.getKey, e => new EncryptionContext.EncryptionKey(e.Value.toByteArray(), e.MetadataList != null ? e.MetadataList.ToDictionary(PulsarApi.KeyValue.getKey, PulsarApi.KeyValue.getValue) : null));
				sbyte[] encParam = new sbyte[MessageCrypto.ivLen];
				msgMetadata.EncryptionParam.copyTo(encParam, 0);
				int? batchSize = Optional.ofNullable(msgMetadata.hasNumMessagesInBatch() ? msgMetadata.NumMessagesInBatch : null);
				encryptionCtx.Keys = keys;
				encryptionCtx.Param = encParam;
				encryptionCtx.Algorithm = msgMetadata.EncryptionAlgo;
				encryptionCtx.CompressionType = CompressionCodecProvider.convertFromWireProtocol(msgMetadata.Compression);
				encryptionCtx.UncompressedMessageSize = msgMetadata.UncompressedSize;
				encryptionCtx.BatchSize = batchSize;
			}
			return Optional.ofNullable(encryptionCtx);
		}

		private int removeExpiredMessagesFromQueue(ISet<MessageId> messageIds)
		{
			int messagesFromQueue = 0;
			Message<T> peek = incomingMessages.peek();
			if (peek != null)
			{
				MessageIdImpl messageId = getMessageIdImpl(peek);
				if (!messageIds.Contains(messageId))
				{
					// first message is not expired, then no message is expired in queue.
					return 0;
				}

				// try not to remove elements that are added while we remove
				Message<T> message = incomingMessages.poll();
				while (message != null)
				{
					INCOMING_MESSAGES_SIZE_UPDATER.addAndGet(this, -message.Data.length);
					messagesFromQueue++;
					MessageIdImpl id = getMessageIdImpl(message);
					if (!messageIds.Contains(id))
					{
						messageIds.Add(id);
						break;
					}
					message = incomingMessages.poll();
				}
			}
			return messagesFromQueue;
		}

		public override ConsumerStats Stats
		{
			get
			{
				return stats;
			}
		}

		internal virtual void setTerminated()
		{
			log.info("[{}] [{}] [{}] Consumer has reached the end of topic", subscription, topic, consumerName);
			hasReachedEndOfTopic_Conflict = true;
			if (listener != null)
			{
				// Propagate notification to listener
				listener.reachedEndOfTopic(this);
			}
		}

		public override bool hasReachedEndOfTopic()
		{
			return hasReachedEndOfTopic_Conflict;
		}

		public override int GetHashCode()
		{
			return Objects.hash(topic, subscription, consumerName);
		}

		// wrapper for connection methods
		internal virtual ClientCnx cnx()
		{
			return this.connectionHandler.cnx();
		}

		internal virtual void resetBackoff()
		{
			this.connectionHandler.resetBackoff();
		}

		internal virtual void connectionClosed(ClientCnx cnx)
		{
			this.connectionHandler.connectionClosed(cnx);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @VisibleForTesting public ClientCnx getClientCnx()
		public virtual ClientCnx ClientCnx
		{
			get
			{
				return this.connectionHandler.ClientCnx;
			}
			set
			{
				this.connectionHandler.ClientCnx = value;
			}
		}


		internal virtual void reconnectLater(Exception exception)
		{
			this.connectionHandler.reconnectLater(exception);
		}

		internal virtual void grabCnx()
		{
			this.connectionHandler.grabCnx();
		}

		public virtual string TopicNameWithoutPartition
		{
			get
			{
				return topicNameWithoutPartition;
			}
		}

		private static readonly Logger log = LoggerFactory.getLogger(typeof(ConsumerImpl));

	}

}