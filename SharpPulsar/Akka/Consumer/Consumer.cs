using Akka.Actor;
using SharpPulsar.Akka.InternalCommands;
using SharpPulsar.Akka.InternalCommands.Consumer;
using SharpPulsar.Akka.Network;
using SharpPulsar.Api;
using SharpPulsar.Api.Transaction;
using SharpPulsar.Common.Compression;
using SharpPulsar.Common.Naming;
using SharpPulsar.Common.Schema;
using SharpPulsar.Extension;
using SharpPulsar.Impl;
using SharpPulsar.Impl.Auth;
using SharpPulsar.Impl.Conf;
using SharpPulsar.Protocol;
using SharpPulsar.Protocol.Builder;
using SharpPulsar.Protocol.Proto;
using SharpPulsar.Protocol.Schema;
using SharpPulsar.Shared;
using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading;
using Akka.Event;
using SharpPulsar.Batch;
using SharpPulsar.Batch.Api;
using SharpPulsar.Exceptions;
using SharpPulsar.Impl.Crypto;
using SharpPulsar.Stats.Consumer;
using SharpPulsar.Stats.Consumer.Api;
using SharpPulsar.Tracker;
using SharpPulsar.Utils;
using System.Collections.Concurrent;
using Nito.AsyncEx;
using SharpPulsar.Tracker.Messages;

namespace SharpPulsar.Akka.Consumer
{
    public class Consumer:ReceiveActor, IWithUnboundedStash
    {
        private const int MaxRedeliverUnacknowledged = 1000;
        private readonly ILoggingAdapter _log;
        private readonly int _partitionIndex;
        private readonly ClientConfigurationData _clientConfiguration;
        private IActorRef _broker;
        private readonly IDictionary<string, string> _metadata;
        private readonly ConsumerConfigurationData _conf;
        private readonly string _consumerName;
        private string _subscriptionName;
        private ISchema _schema;
        private readonly ConsumerInterceptors _interceptors;
        private readonly IMessageListener _listener;
        private readonly IConsumerEventListener _consumerEventListener;
        private readonly TopicName _topicName;
        private readonly ConcurrentQueue<ConsumedMessage> _incomingMessages;
        private readonly List<string> _pendingChunckedMessageUuidQueue;
        private readonly IActorRef _network;
        private int _requestedFlowPermits = 2000;
        private readonly IDictionary<MessageId, IList<Message>> _possibleSendToDeadLetterTopicMessages;
        private Seek _seek;
        private readonly bool _createTopicIfDoesNotExist;
        private readonly SubscriptionMode _subscriptionMode;
        private volatile BatchMessageId _startMessageId;
        private readonly IMessageId _initialStartMessageId;
        private  ConnectedServerInfo _serverInfo;
        private readonly long _startMessageRollbackDurationInSec;
        private readonly IMessageCrypto _msgCrypto;
        private readonly bool _hasParentConsumer;
        private readonly long _consumerid;
        private readonly Dictionary<MessageId, MessageId[]> _unAckedChunkedMessageIdSequenceMap;
        private readonly ActorSystem _system;
        private readonly IActorRef _parent;
        private readonly IActorRef _self;

        private IMessageId _lastDequeuedMessageId = MessageIdFields.Earliest;
        private IMessageId _lastMessageIdInBroker = MessageIdFields.Earliest;
        private ICancelable _messagePusher;

        private bool _hasReachedEndOfTopic;

        private long _incomingMessagesSize;

        private readonly bool _eventSourced;
        private readonly Dictionary<string, ChunkedMessageCtx> _chunkedMessagesMap = new Dictionary<string, ChunkedMessageCtx>();
        private int _pendingChunckedMessageCount;
        private long _expireTimeOfIncompleteChunkedMessageMillis;
        private bool _expireChunkMessageTaskScheduled;
        private readonly int _maxPendingChuckedMessage;

        private ICancelable _batchReceiveTimeout;

        private readonly BatchReceivePolicy _batchReceivePolicy;
        protected readonly int MaxReceiverQueueSize;

        private readonly IActorRef _unAckedMessageTracker;
        private IActorRef _acknowledgmentsGroupingTracker;
        private readonly IActorRef _negativeAcksTracker;

        private readonly IConsumerStatsRecorder _stats;
        private ConcurrentQueue<OpBatchReceive> _pendingBatchReceives;

        private readonly string _topicNameWithoutPartition;



        private BatchMessageId _seekMessageId;
        private bool _duringSeek;

        // if queue Size is reasonable (most of the time equal to number of producers try to publish messages concurrently on
        // the topic) then it guards against broken chuncked message which was not fully published
        private readonly bool _autoAckOldestChunkedMessageOnQueueFull;
        private readonly IActorRef _pulsarManager;
        public Consumer(ClientConfigurationData clientConfiguration, string topic, ConsumerConfigurationData configuration, long consumerid, IActorRef network, bool hasParentConsumer, int partitionIndex, SubscriptionMode mode, Seek seek, IActorRef pulsarManager, bool eventSourced = false)
        {
            if(hasParentConsumer)
                _parent = Context.Parent;
            _self = Self;
            _initialStartMessageId = configuration.StartMessageId;
            _system = Context.System;
            _log = Context.GetLogger();
            _incomingMessages = new ConcurrentQueue<ConsumedMessage>();
            MaxReceiverQueueSize = Math.Max(1_000_000, configuration.ReceiverQueueSize);
            _pulsarManager = pulsarManager;
            _eventSourced = eventSourced;
            _possibleSendToDeadLetterTopicMessages = new Dictionary<MessageId, IList<Message>>();
            _listener = configuration.MessageListener;
            _createTopicIfDoesNotExist = configuration.ForceTopicCreation;
            _subscriptionName = configuration.SubscriptionName;
            _consumerEventListener = configuration.ConsumerEventListener;
            _startMessageId = configuration.StartMessageId != null ? new BatchMessageId((MessageId)configuration.StartMessageId) : null; 
            _subscriptionMode = mode;
            _partitionIndex = partitionIndex;
            _hasParentConsumer = hasParentConsumer;
            //_requestedFlowPermits = 1500;//configuration.ReceiverQueueSize;
            _conf = configuration;
            _interceptors = new ConsumerInterceptors(_system, configuration.Interceptors);
            _clientConfiguration = clientConfiguration;
            _startMessageRollbackDurationInSec = 0;
            _consumerid = consumerid;
            _network = network;
            _topicName = TopicName.Get(topic);
            _schema = configuration.Schema;
            _seek = seek;
            _consumerName = configuration.ConsumerName;
            _unAckedChunkedMessageIdSequenceMap = new Dictionary<MessageId, MessageId[]>();

            _maxPendingChuckedMessage = configuration.MaxPendingChuckedMessage;
            _pendingChunckedMessageUuidQueue = new List<string>();
            _expireTimeOfIncompleteChunkedMessageMillis = configuration.ExpireTimeOfIncompleteChunkedMessageMillis;
            _autoAckOldestChunkedMessageOnQueueFull = configuration.AutoAckOldestChunkedMessageOnQueueFull;

            if (clientConfiguration.StatsIntervalSeconds > 0)
            {
                _stats = new ConsumerStatsRecorder(_system, configuration, _topicName.ToString(), _consumerName, _subscriptionName, clientConfiguration.StatsIntervalSeconds);
            }
            else
            {
                _stats = ConsumerStatsDisabled.Instance;
            }

            _duringSeek = false;

            if (configuration.AckTimeoutMillis != 0)
            {
                if (configuration.TickDurationMillis > 0)
                {
                    _unAckedMessageTracker = Context.ActorOf(UnAckedMessageTracker.Prop(configuration.AckTimeoutMillis, Math.Min(configuration.TickDurationMillis, configuration.AckTimeoutMillis), Self), "UnAckedMessageTracker");
                }
                else
                {
                    _unAckedMessageTracker = Context.ActorOf(UnAckedMessageTracker.Prop(configuration.AckTimeoutMillis, 0, Self), "UnAckedMessageTracker");
                }
            }
            else
            {
                _unAckedMessageTracker = Context.ActorOf(UnAckedMessageTrackerDisabled.Prop(), "UnAckedMessageTrackerDisabled");
            }

            _negativeAcksTracker = Context.ActorOf(NegativeAcksTracker.Prop(configuration, _unAckedMessageTracker), "NegativeAcksTracker");
            // Create msgCrypto if not created already
            if (configuration.CryptoKeyReader != null)
            {
                if (configuration.MessageCrypto != null)
                {
                    _msgCrypto = configuration.MessageCrypto;
                }
                else
                {
                    // default to use MessageCryptoBc;
                    MessageCrypto msgCryptoBc;
                    try
                    {
                        msgCryptoBc = new MessageCrypto($"[{configuration.SingleTopic}] [{configuration.SubscriptionName}]", false, _log);
                        
                    }
                    catch (Exception e)
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
            if (configuration.Properties == null || configuration.Properties.Count == 0)
            {
                _metadata = new Dictionary<string, string>();
            }
            else
            {
                _metadata = new Dictionary<string, string>(configuration.Properties);
            }

            _topicNameWithoutPartition = _topicName.PartitionedTopicName;

            if (configuration.BatchReceivePolicy != null)
            {
                var userBatchReceivePolicy = configuration.BatchReceivePolicy;
                if (userBatchReceivePolicy.MaxNumMessages > MaxReceiverQueueSize)
                {
                    _batchReceivePolicy = BatchReceivePolicy.GetBuilder().MaxNumMessages(MaxReceiverQueueSize).MaxNumBytes(userBatchReceivePolicy.MaxNumBytes).Timeout((int)userBatchReceivePolicy.TimeoutMs).Build();
                    _log.Warning($"BatchReceivePolicy maxNumMessages: {userBatchReceivePolicy.MaxNumMessages} is greater than maxReceiverQueueSize: {MaxReceiverQueueSize}, reset to maxReceiverQueueSize. batchReceivePolicy: {_batchReceivePolicy}");
                }
                else if (userBatchReceivePolicy.MaxNumMessages <= 0 && userBatchReceivePolicy.MaxNumBytes <= 0)
                {
                    _batchReceivePolicy = BatchReceivePolicy.GetBuilder().MaxNumMessages(BatchReceivePolicy.DefaultPolicy.MaxNumMessages).MaxNumBytes(BatchReceivePolicy.DefaultPolicy.MaxNumBytes).Timeout((int)userBatchReceivePolicy.TimeoutMs).Build();
                    _log.Warning($"BatchReceivePolicy maxNumMessages: {userBatchReceivePolicy.MaxNumMessages} or maxNumBytes: {userBatchReceivePolicy.MaxNumBytes} is less than 0. Reset to DEFAULT_POLICY. batchReceivePolicy: {_batchReceivePolicy}");
                }
                else
                {
                    _batchReceivePolicy = configuration.BatchReceivePolicy;
                }
            }
            else
            {
                _batchReceivePolicy = BatchReceivePolicy.DefaultPolicy;
            }

            if (_batchReceivePolicy.TimeoutMs > 0)
            {
                _batchReceiveTimeout = _system.Scheduler.Advanced.ScheduleOnceCancelable(TimeSpan.FromMilliseconds(_batchReceivePolicy.TimeoutMs), PendingBatchReceiveTask);
            }
            _messagePusher = _system.Scheduler.Advanced.ScheduleOnceCancelable(TimeSpan.FromMilliseconds(100), PushMessages);
            ReceiveAny(x => Stash.Stash());
            BecomeLookUp();
        }
        private void PushMessages()
        {
            try
            {
               
                _incomingMessages.TryDequeue(out var message);
                while (message != null)
                {
                    var msg = (Message) message.Message;
                    MessageProcessed(msg);
                    var msg1 = BeforeConsume(msg);
                    if (_hasParentConsumer)
                    {
                        _parent.Tell(new ConsumedMessage(_self, msg1, message.AckSets, _consumerName));
                    }
                    else
                    {
                        if (_conf.ConsumptionType == ConsumptionType.Listener)
                            _listener.Received(_self, msg1);
                        else if (_conf.ConsumptionType == ConsumptionType.Queue)
                            _pulsarManager.Tell(new ConsumedMessage(_self, msg1, message.AckSets, _consumerName));
                    }
                    _incomingMessages.TryDequeue(out message);
                }
            }
            catch (Exception e)
            {
                _stats.IncrementNumReceiveFailed();
                _log.Error(PulsarClientException.Unwrap(e).ToString());
            }
            finally
            {
                _messagePusher = _system.Scheduler.Advanced.ScheduleOnceCancelable(TimeSpan.FromMilliseconds(100), PushMessages);
            }
        }
        public virtual void NotifyPendingBatchReceivedCallBack()
        {
            _pendingBatchReceives.TryPeek(out var opBatchReceive);
            if (opBatchReceive?.Messages == null)
            {
                return;
            }
            NotifyPendingBatchReceivedCallBack(opBatchReceive);
        }

        private void NotifyPendingBatchReceivedCallBack(OpBatchReceive opBatchReceive)
        {
            var messages = new Messages(_batchReceivePolicy.MaxNumMessages, _batchReceivePolicy.MaxNumBytes);
            _incomingMessages.TryPeek(out var msgPeeked);
            while (msgPeeked != null && messages.CanAdd(msgPeeked.Message))
            {
                ConsumedMessage msg = null;
                try
                {
                    _incomingMessages.TryDequeue(out msg);
                }
                catch 
                {
                    // ignore
                }
                if (msg != null)
                {
                    var interceptMsg = BeforeConsume(msg.Message);
                    messages.Add(interceptMsg);
                } 
                _incomingMessages.TryPeek(out msgPeeked);
            }
            opBatchReceive.Messages = messages;
        }
        protected override void PostStop()
        {
            var requestid = Interlocked.Increment(ref IdGenerators.RequestId);
            var cmd = Commands.NewCloseConsumer(_consumerid, requestid);
            var payload = new Payload(cmd, requestid, "CloseConsumer");
            _broker.Tell(payload);
            _batchReceiveTimeout?.Cancel();
            _messagePusher?.Cancel();
        }

        private void RedeliverUnacknowledgedMessages()
        {
            _unAckedMessageTracker.Tell(new Clear());

            var requestid = Interlocked.Increment(ref IdGenerators.RequestId);
            var cmd = Commands.NewRedeliverUnacknowledgedMessages(_consumerid);
            var payload = new Payload(cmd, requestid, "RedeliverUnacknowledgedMessages");
            _broker.Tell(payload);
        }

        private void RedeliverUnacknowledgedMessages(ISet<IMessageId> messageIds)
        {
            if (messageIds.Count == 0)
            {
                return;
            }
            
            if (_conf.SubscriptionType != CommandSubscribe.SubType.Shared && _conf.SubscriptionType != CommandSubscribe.SubType.KeyShared)
            {
                // We cannot redeliver single messages if subscription type is not Shared
                RedeliverUnacknowledgedMessages();
                return;
            }
            var messagesFromQueue = RemoveExpiredMessagesFromQueue(messageIds);
            var batches = messageIds.PartitionMessageId(MaxRedeliverUnacknowledged);
            var builder = new MessageIdData();
            batches.ForEach(ids =>
            {
                var messageIdDatas = ids.Select(messageId =>
                {
                    builder.Partition = (messageId.PartitionIndex);
                    builder.ledgerId = (ulong)(messageId.LedgerId);
                    builder.entryId = (ulong)(messageId.EntryId);
                    return builder;
                }).ToList();
                if (messageIdDatas.Count > 0)
                {
                    var requestid = Interlocked.Increment(ref IdGenerators.RequestId);
                    var cmd = Commands.NewRedeliverUnacknowledgedMessages(_consumerid, messageIdDatas);
                    var payload = new Payload(cmd, requestid, "RedeliverUnacknowledgedMessages");
                    _broker.Tell(payload);
                }
            });
            
        }
        private int RemoveExpiredMessagesFromQueue(ISet<IMessageId> messageIds)
        {
            var messagesFromQueue = 0;
            
            if (_incomingMessages.TryPeek(out var peek))
            {
                var msg = (Message) peek.Message;
                var messageId = GetMessageId(msg);
                if (!messageIds.Contains(messageId))
                {
                    // first message is not expired, then no message is expired in queue.
                    return 0;
                }

                // try not to remove elements that are added while we remove
                _incomingMessages.TryDequeue(out var message);
                while (message != null)
                {
                    _incomingMessagesSize -= msg.Data.Length;
                    messagesFromQueue++;
                    var id = GetMessageId(msg);
                    if (!messageIds.Contains(id))
                    {
                        messageIds.Add(id);
                        break;
                    }
                    _incomingMessages.TryDequeue(out message);
                }
            }
            return messagesFromQueue;
        }
        private MessageId GetMessageId(Message msg)
        {
            var messageId = msg.MessageId;
            if (messageId is BatchMessageId batch)
            {
                // messageIds contain MessageId, not BatchMessageId
                messageId = new MessageId(batch.LedgerId, batch.EntryId, _partitionIndex);
            }
            return (MessageId)messageId;
        }
        private void RedeliverUnacknowledgedMessages(ImmutableHashSet<Unacked> messageIds)
        {
            var requestid = Interlocked.Increment(ref IdGenerators.RequestId);
            if (_conf.SubscriptionType != CommandSubscribe.SubType.Shared && _conf.SubscriptionType != CommandSubscribe.SubType.KeyShared)
            {
                // We cannot redeliver single messages if subscription type is not Shared
                var cmd = Commands.NewRedeliverUnacknowledgedMessages(_consumerid);
                var payload = new Payload(cmd, requestid, "RedeliverUnacknowledgedMessages");
                _broker.Tell(payload);
            }
            else
            {
                var batches = messageIds.PartitionMessageId();
                var builder = new MessageIdData();
                batches.ForEach(ids =>
                {
                    var messageIdDatas = ids.Select(messageId =>
                    {
                        builder.Partition = (messageId.PartitionIndex);
                        builder.ledgerId = (ulong)(messageId.LedgerId);
                        builder.entryId = (ulong)(messageId.EntryId);
                        return builder;
                    }).ToList();
                    var cmd = Commands.NewRedeliverUnacknowledgedMessages(_consumerid, messageIdDatas);
                    var payload = new Payload(cmd, requestid, "RedeliverUnacknowledgedMessages");
                    _broker.Tell(payload);
                });
            }
        }
        private bool ProcessPossibleToDlq(long ledgerid, long entryid, int partitionindex, int batchindex)
        {
           return false;
        }

        public static Props Prop(ClientConfigurationData clientConfiguration, string topic, ConsumerConfigurationData configuration, long consumerid, IActorRef network, bool hasParentConsumer, int partitionIndex, SubscriptionMode mode, Seek seek, IActorRef pulsarManager, bool eventSourced = false)
        {
            return Props.Create(()=> new Consumer(clientConfiguration, topic, configuration, consumerid, network, hasParentConsumer, partitionIndex, mode, seek, pulsarManager, eventSourced));
        }
        
        private void InternalGetLastMessageId()
        {
            try
            {
                var requestid = Interlocked.Increment(ref IdGenerators.RequestId);
                var request = Commands.NewGetLastMessageId(_consumerid, requestid);
                var payload = new Payload(request, requestid, "NewGetLastMessageId");

                _log.Info($"[{_topicName}][{_subscriptionName}] Get topic last message Id");

                var ask = _broker.Ask<LastMessageIdResponse>(payload);
                var result = SynchronizationContextSwitcher.NoContext(async () => await ask).Result;

                _log.Info($"[{_topicName}][{_subscriptionName}] Successfully getLastMessageId {result.LedgerId}:{result.EntryId}");
                
                _pulsarManager.Tell(new LastMessageIdReceived(_consumerid, _topicName.ToString(), result));
            }
            catch (Exception e)
            {
                _log.Error($"[{_topicName}][{_subscriptionName}] Failed getLastMessageId command: {e}");
            }
        }
        private IMessage BeforeConsume(IMessage message)
        {
            if (_interceptors != null)
            {
                return _interceptors.BeforeConsume(_self, message);
            }

            return message;
        }
        private void MessageReceived(MessageIdData messageId, int redeliveryCount, ReadOnlySequence<byte> data, IList<long> ackSet)
        {
            //MessageIdData messageId, int redeliveryCount, ReadOnlySequence<byte> data
            try
            {
                if (_log.IsDebugEnabled)
                {
                    _log.Debug($"[{_topicName}][{_subscriptionName}] Received message: {messageId.ledgerId}/{messageId.entryId}");
                    _consumerEventListener.Log($"[{_topicName}][{_subscriptionName}] Received message: {messageId.ledgerId}/{messageId.entryId}");
                }
                if (!data.IsValid())
                {
                    // discard message with checksum error
                    DiscardCorruptedMessage(messageId, CommandAck.ValidationError.ChecksumMismatch);
                    return;
                }

                var metadataSize = data.GetMetadataSize();
                var payload = data.ExtractData(metadataSize);
                MessageMetadata msgMetadata;
                try
                {
                    msgMetadata = data.ExtractMetadata(metadataSize);
                }
                catch (Exception)
                {
                    DiscardCorruptedMessage(messageId, CommandAck.ValidationError.ChecksumMismatch);
                    return;
                }
                var numMessages = msgMetadata.NumMessagesInBatch;
                var isChunkedMessage = msgMetadata.NumChunksFromMsg > 1 && _conf.SubscriptionType != CommandSubscribe.SubType.Shared;

                var msgId = new MessageId((long)messageId.ledgerId, (long)messageId.entryId, _partitionIndex);
                var ask = _acknowledgmentsGroupingTracker.Ask<bool>(new IsDuplicate(msgId));
                var isDup = SynchronizationContextSwitcher.NoContext(async () => await ask).Result;
                if (isDup)
                {
                    if (_log.IsDebugEnabled)
                    {
                        _log.Debug($"[{_topicName}] [{_subscriptionName}] Ignoring message as it was already being acked earlier by same consumer {_consumerName}/{msgId}");
                    }
                    return;
                }

                var decryptedPayload = DecryptPayloadIfNeeded(messageId, msgMetadata, payload);

                var isMessageUndecryptable = IsMessageUndecryptable(msgMetadata);

                if (decryptedPayload == null)
                {
                    // Message was discarded or CryptoKeyReader isn't implemented
                    return;
                }

                // uncompress decryptedPayload and release decryptedPayload-ByteBuf
                var uncompressedPayload = (isMessageUndecryptable || isChunkedMessage) ? decryptedPayload : UncompressPayloadIfNeeded(messageId, msgMetadata, decryptedPayload, true);
                if (uncompressedPayload == null)
                {
                    // Message was discarded on decompression error
                    return;
                }

                // if message is not decryptable then it can't be parsed as a batch-message. so, add EncyrptionCtx to message
                // and return undecrypted payload
                if (isMessageUndecryptable || (numMessages == 1 && !HasNumMessagesInBatch(msgMetadata)))
                {

                    // right now, chunked messages are only supported by non-shared subscription
                    if (isChunkedMessage)
                    {
                        uncompressedPayload = ProcessMessageChunk(uncompressedPayload, msgMetadata, msgId, messageId);
                        if (uncompressedPayload == null)
                        {
                            return;
                        }
                    }

                    if (IsSameEntry(messageId) && IsPriorEntryIndex((long)messageId.entryId))
                    {
                        // We need to discard entries that were prior to startMessageId
                        if (_log.IsDebugEnabled)
                        {
                            _log.Debug($"[{_subscriptionName}] [{_consumerName}] Ignoring message from before the startMessageId: {_startMessageId}");
                        }

                        return;
                    }

                    var message = new Message(_topicName.ToString(), msgId, msgMetadata, uncompressedPayload, CreateEncryptionContext(msgMetadata), _schema, redeliveryCount);
                    // Enqueue the message so that it can be retrieved when application calls receive()
                    // if the conf.getReceiverQueueSize() is 0 then discard message if no one is waiting for it.
                    // if asyncReceive is waiting then notify callback without adding to incomingMessages queue

                    if (EnqueueMessageAndCheckBatchReceive(new ConsumedMessage(Self, message, ackSet, _consumerName)))
                    {
                        if (HasPendingBatchReceive())
                        {
                            NotifyPendingBatchReceivedCallBack();
                        }
                    }

                }
                else
                {
                    // handle batch message enqueuing; uncompressed payload has all messages in batch
                    ReceiveIndividualMessagesFromBatch(msgMetadata, redeliveryCount, ackSet, new ReadOnlySequence<byte>(uncompressedPayload), messageId);

                }
            }
            catch (Exception ex)
            {
                _log.Error(ex.ToString());
            }

        }

        private bool HasNumMessagesInBatch(MessageMetadata m)
        {
            var should = m.ShouldSerializeNumMessagesInBatch();
            return should;
        }
        private void ReceiveIndividualMessagesFromBatch(MessageMetadata msgMetadata, int redeliveryCount, IList<long> ackSet, ReadOnlySequence<byte> uncompressedPayload, MessageIdData messageId)
        {
            var batchSize = msgMetadata.NumMessagesInBatch;

            // create ack tracker for entry aka batch
            var acker = BatchMessageAcker.NewAcker(batchSize);

            try
            {
                long index = 0;
                for (var i = 0; i < batchSize; ++i)
                {
                    var singleMetadataSize = uncompressedPayload.ReadUInt32(index, true);
                    index += 4;
                    if (_log.IsDebugEnabled)
                    {
                        _log.Debug($"[{_subscriptionName}] [{_consumerName}] processing message num - {i} in batch");
                    }

                    var singleMessageMetadata = Serializer.Deserialize<SingleMessageMetadata>(uncompressedPayload.Slice(index, singleMetadataSize));
                    index += singleMetadataSize;
                    var singleMessagePayload = uncompressedPayload.Slice(index, singleMessageMetadata.PayloadSize);
                    if (IsSameEntry(messageId) && IsPriorBatchIndex(i))
                    {
                        // If we are receiving a batch message, we need to discard messages that were prior
                        // to the startMessageId
                        if (_log.IsDebugEnabled)
                        {
                            _log.Debug($"[{_subscriptionName}] [{_consumerName}] Ignoring message from before the startMessageId: {_startMessageId}");
                        }

                        continue;
                    }

                    if (singleMessageMetadata.CompactedOut)
                    {
                        // message has been compacted out, so don't send to the user
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
                            continue;
                        }
                    }

                    var batchMessageId = new BatchMessageId((long)messageId.ledgerId, (long)messageId.entryId, _partitionIndex, i, batchSize, acker);
                    var message = new Message(_topicName.ToString(), batchMessageId, msgMetadata, singleMessageMetadata, singleMessagePayload.ToArray(), CreateEncryptionContext(msgMetadata), _schema, redeliveryCount);
                    /*if (possibleToDeadLetter != null)
                    {
                        possibleToDeadLetter.Add(message);
                    }*/
                    if (EnqueueMessageAndCheckBatchReceive(new ConsumedMessage(Self, message, ackSet, _consumerName)))
                    {
                        if (HasPendingBatchReceive())
                        {
                            NotifyPendingBatchReceivedCallBack();
                        }
                    }
                    index += (uint)singleMessageMetadata.PayloadSize;
                }
            }
            catch (IOException)
            {
                _log.Error($"[{_subscriptionName}] [{_consumerName}] unable to obtain message in batch");
                DiscardCorruptedMessage(messageId, CommandAck.ValidationError.BatchDeSerializeError);
            }

            if (_log.IsDebugEnabled)
            {
                _log.Debug($"[{_subscriptionName}] [{_consumerName}] enqueued messages in batch. queue size - {_incomingMessages.Count}, available queue size - {MaxReceiverQueueSize - _incomingMessages.Count}");
            }

        }
        
        private bool IsPriorBatchIndex(long idx)
        {
            return _conf.ResetIncludeHead ? idx < _startMessageId.BatchIndex : idx <= _startMessageId.BatchIndex;
        }
        private bool EnqueueMessageAndCheckBatchReceive(ConsumedMessage message)
        {
            var msg = (Message) message.Message;
            if (CanEnqueueMessage(msg))
            {
                _incomingMessages.Enqueue(message);
                _incomingMessagesSize += msg.Data?.Length ?? 0;
            }
            return HasEnoughMessagesForBatchReceive();
        }

        private bool HasEnoughMessagesForBatchReceive()
        {
            if (_batchReceivePolicy.MaxNumMessages <= 0 && _batchReceivePolicy.MaxNumBytes <= 0)
            {
                return false;
            }
            return (_batchReceivePolicy.MaxNumMessages > 0 && _incomingMessages.Count >= _batchReceivePolicy.MaxNumMessages) || (_batchReceivePolicy.MaxNumBytes > 0 && _incomingMessagesSize >= _batchReceivePolicy.MaxNumBytes);
        }
        /// <summary>
        /// Record the event that one message has been processed by the application.
        /// 
        /// Periodically, it sends a Flow command to notify the broker that it can push more messages
        /// </summary>
        private void MessageProcessed(Message msg)
        {
            _lastDequeuedMessageId = msg.MessageId;
            _stats.UpdateNumMsgsReceived(msg);

            TrackMessage(msg);
            _incomingMessagesSize = -msg.Data?.Length ?? 0;
        }

        private void IncreaseAvailablePermits()
        {
            if (!_eventSourced)
                SendFlow(1);
        }
        private byte[] ProcessMessageChunk(byte[] compressedPayload, MessageMetadata msgMetadata, MessageId msgId, MessageIdData messageId)
        {

            // Lazy task scheduling to expire incomplete chunk message
            if (!_expireChunkMessageTaskScheduled && _expireTimeOfIncompleteChunkedMessageMillis > 0)
            {
                _system.Scheduler.Advanced.ScheduleRepeatedly(TimeSpan.FromMilliseconds(_expireTimeOfIncompleteChunkedMessageMillis), TimeSpan.FromMilliseconds(_expireTimeOfIncompleteChunkedMessageMillis), RemoveExpireIncompleteChunkedMessages);
                _expireChunkMessageTaskScheduled = true;
            }

            if (msgMetadata.ChunkId == 0)
            {
                var totalChunks = msgMetadata.NumChunksFromMsg;
                _chunkedMessagesMap.TryAdd(msgMetadata.Uuid, ChunkedMessageCtx.Get(totalChunks, new List<byte>()));
                _pendingChunckedMessageCount++;
                if (_maxPendingChuckedMessage > 0 && _pendingChunckedMessageCount > _maxPendingChuckedMessage)
                {
                    RemoveOldestPendingChunkedMessage();
                }
                _pendingChunckedMessageUuidQueue.Add(msgMetadata.Uuid);
            }

            var chunkedMsgCtx = _chunkedMessagesMap[msgMetadata.Uuid];
            // discard message if chunk is out-of-order
            if (chunkedMsgCtx?.ChunkedMsgBuffer == null || msgMetadata.ChunkId != (chunkedMsgCtx.LastChunkedMessageId + 1) || msgMetadata.ChunkId >= msgMetadata.TotalChunkMsgSize)
            {
                // means we lost the first chunk: should never happen
                _log.Info($"Received unexpected chunk messageId {msgId}, last-chunk-id {chunkedMsgCtx?.LastChunkedMessageId ?? 0}, chunkId = {msgMetadata.ChunkId}, total-chunks {msgMetadata.TotalChunkMsgSize}");
                chunkedMsgCtx?.Recycle();
                _chunkedMessagesMap.Remove(msgMetadata.Uuid);
                if (_expireTimeOfIncompleteChunkedMessageMillis > 0 && DateTimeHelper.CurrentUnixTimeMillis() > ((long)msgMetadata.PublishTime + _expireTimeOfIncompleteChunkedMessageMillis))
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
                return null;
            }

            // last chunk received: so, stitch chunked-messages and clear up chunkedMsgBuffer
            if (_log.IsDebugEnabled)
            {
                _log.Debug($"Chunked message completed chunkId {msgMetadata.ChunkId}, total-chunks {msgMetadata.NumChunksFromMsg}, msgId {msgId} sequenceId {msgMetadata.SequenceId}");
            }
            // remove buffer from the map, add chunked messageId to unack-message tracker, and reduce pending-chunked-message count
            _chunkedMessagesMap.Remove(msgMetadata.Uuid);
            _unAckedChunkedMessageIdSequenceMap.Add(msgId, chunkedMsgCtx.ChunkedMessageIds);
            _pendingChunckedMessageCount--;
            compressedPayload = chunkedMsgCtx.ChunkedMsgBuffer.ToArray();
            //AckMultiMessage(chunkedMsgCtx.ChunkedMessageIds);
            chunkedMsgCtx.Recycle();
            var uncompressedPayload = UncompressPayloadIfNeeded(messageId, msgMetadata, compressedPayload, false);
            return uncompressedPayload;
        }
        private void RemoveExpireIncompleteChunkedMessages()
        {
            if (_expireTimeOfIncompleteChunkedMessageMillis <= 0)
            {
                return;
            }

            string messageUuid;
            while (!string.ReferenceEquals((messageUuid = _pendingChunckedMessageUuidQueue.FirstOrDefault()), null))
            {
                var chunkedMsgCtx = !string.IsNullOrWhiteSpace(messageUuid) ? _chunkedMessagesMap[messageUuid] : null;
                if (chunkedMsgCtx != null && DateTimeHelper.CurrentUnixTimeMillis() > (chunkedMsgCtx.ReceivedTime + _expireTimeOfIncompleteChunkedMessageMillis))
                {
                    _pendingChunckedMessageUuidQueue.Remove(messageUuid);
                    RemoveChunkMessage(messageUuid, chunkedMsgCtx, true);
                }
                else
                {
                    return;
                }
            }
        }

        private void RemoveOldestPendingChunkedMessage()
        {
            ChunkedMessageCtx chunkedMsgCtx = null;
            string firstPendingMsgUuid = null;
            while (chunkedMsgCtx == null && _pendingChunckedMessageUuidQueue.Count > 0)
            {
                // remove oldest pending chunked-message group and free memory
                firstPendingMsgUuid = _pendingChunckedMessageUuidQueue.FirstOrDefault();
                chunkedMsgCtx = !string.IsNullOrWhiteSpace(firstPendingMsgUuid) ? _chunkedMessagesMap[firstPendingMsgUuid] : null;
            }
            RemoveChunkMessage(firstPendingMsgUuid, chunkedMsgCtx, _autoAckOldestChunkedMessageOnQueueFull);
        }
        private void RemoveChunkMessage(string msgUuid, ChunkedMessageCtx chunkedMsgCtx, bool autoAck)
        {
            if (chunkedMsgCtx == null)
            {
                return;
            }
            // clean up pending chuncked-Message
            _chunkedMessagesMap.Remove(msgUuid);
            if (chunkedMsgCtx.ChunkedMessageIds != null)
            {
                foreach (var msgId in chunkedMsgCtx.ChunkedMessageIds)
                {
                    if (msgId == null)
                    {
                        continue;
                    }
                    if (autoAck)
                    {
                        _log.Info($"Removing chunk message-id {msgId}");
                        DoAcknowledge(msgId, CommandAck.AckType.Individual, new Dictionary<string, long>(), null);
                    }
                    else
                    {
                        TrackMessage(msgId);
                    }
                }
            }
            _pendingChunckedMessageCount--;
        }
        
        private bool MarkAckForBatchMessage(BatchMessageId batchMessageId, CommandAck.AckType ackType, IDictionary<string, long> properties)
        {
            bool isAllMsgsAcked;
            if (ackType == CommandAck.AckType.Individual)
            {
                isAllMsgsAcked = batchMessageId.AckIndividual();
            }
            else
            {
                isAllMsgsAcked = batchMessageId.AckCumulative();
            }
            var outstandingAcks = 0;
            if (_log.IsDebugEnabled)
            {
                outstandingAcks = batchMessageId.OutstandingAcksInSameBatch;
            }

            var batchSize = batchMessageId.BatchSize;
            // all messages in this batch have been acked
            if (isAllMsgsAcked)
            {
                if (_log.IsDebugEnabled)
                {
                    _log.Debug($"[{_subscriptionName}] [{_consumerName}] can ack message to broker {batchMessageId}, acktype {ackType}, cardinality {outstandingAcks}, length {batchSize}");
                }
                return true;
            }
            else
            {
                if (CommandAck.AckType.Cumulative == ackType && !batchMessageId.Acker.PrevBatchCumulativelyAcked)
                {
                    SendAcknowledge(batchMessageId.PrevBatchMessageId(), CommandAck.AckType.Cumulative, properties, null);
                    batchMessageId.Acker.PrevBatchCumulativelyAcked = true;
                }
                else
                {
                    OnAcknowledge(batchMessageId, null);
                }
                if (_log.IsDebugEnabled)
                {
                    _log.Debug($"[{_subscriptionName}] [{_consumerName}] cannot ack message to broker {batchMessageId}, acktype {ackType}, pending acks - {outstandingAcks}");
                }
            }
            return false;
        }

        private void DoAcknowledge(IMessageId messageId, CommandAck.AckType ackType, IDictionary<string, long> properties, ITransaction txnImpl)
        {
            if (messageId is BatchMessageId id)
            {
                if (MarkAckForBatchMessage(id, ackType, properties))
                {
                    // all messages in batch have been acked so broker can be acked via sendAcknowledge()
                    if (_log.IsDebugEnabled)
                    {
                        _log.Debug($"[{_subscriptionName}] [{_consumerName}] acknowledging message - {messageId}, acktype {ackType}");
                    }
                }
                else
                {
                    var batchMessageId = id;
                    _acknowledgmentsGroupingTracker.Tell(new AddBatchIndexAcknowledgment(batchMessageId, batchMessageId.BatchIndex, batchMessageId.BatchSize, ackType, properties));
                    // other messages in batch are still pending ack.
                    return;
                }
            }
            SendAcknowledge(messageId, ackType, properties, txnImpl);
        }

        /// <summary>
        /// Clear the internal receiver queue and returns the message id of what was the 1st message in the queue that was
        /// not seen by the application
        /// </summary>
        private BatchMessageId ClearReceiverQueue()
        {
            var currentMessageQueue = new List<Message>(_incomingMessages.Count);
            var mcount = _incomingMessages.Count;
            var n = 0;
            while (n < mcount)
            {
                
                if (_incomingMessages.TryDequeue(out var m))
                    currentMessageQueue.Add((Message)m.Message);
                else
                    break;
                ++n;
            }
            _incomingMessagesSize = 0;

            if (_duringSeek)
            {
                _duringSeek = false;
                return _seekMessageId;
            }
            else if (_subscriptionMode == SubscriptionMode.Durable)
            {
                return _startMessageId;
            }

            if (currentMessageQueue.Count > 0)
            {
                var nextMessageInQueue = currentMessageQueue[0].MessageId;
                var nextMessageId = (MessageId)nextMessageInQueue;
                BatchMessageId previousMessage;
                if (nextMessageInQueue is BatchMessageId batch)
                {
                    // Get on the previous message within the current batch
                    previousMessage = new BatchMessageId(batch.LedgerId, batch.EntryId, batch.PartitionIndex, batch.BatchIndex - 1);
                }
                else
                {
                    // Get on previous message in previous entry
                    previousMessage = new BatchMessageId(nextMessageId.LedgerId, nextMessageId.EntryId - 1, nextMessageId.PartitionIndex, -1);
                }

                return previousMessage;
            }
            else if (!_lastDequeuedMessageId.Equals(MessageIdFields.Earliest))
            {
                // If the queue was empty we need to restart from the message just after the last one that has been dequeued
                // in the past
                return new BatchMessageId((MessageId)_lastDequeuedMessageId);
            }
            else
            {
                // No message was received or dequeued by this consumer. Next message would still be the startMessageId
                return _startMessageId;
            }
        }
        private void SendAcknowledge(IMessageId messageId, CommandAck.AckType ackType, IDictionary<string, long> properties, ITransaction txnImpl = null)
        {
            if (ackType == CommandAck.AckType.Individual)
            {
                if (messageId is BatchMessageId batchMessageId)
                {
                    _stats.IncrementNumAcksSent(batchMessageId.BatchSize);
                    _unAckedMessageTracker.Tell(new Remove(new MessageId(batchMessageId.LedgerId, batchMessageId.EntryId, batchMessageId.PartitionIndex)));
                }
                else
                {
                    // increment counter by 1 for non-batch msg
                    _unAckedMessageTracker.Tell(new Remove(messageId));
                    _stats.IncrementNumAcksSent(1);
                }
                OnAcknowledge(messageId, null);
            }
            else if (ackType == CommandAck.AckType.Cumulative)
            {
                OnAcknowledgeCumulative(messageId, null);
                var ask = _unAckedMessageTracker.Ask<int>(new RemoveMessagesTill(messageId));
                var removed = SynchronizationContextSwitcher.NoContext(async () => await ask).Result; ;
                _stats.IncrementNumAcksSent(removed);
            }

            _acknowledgmentsGroupingTracker.Tell(new AddAcknowledgment(messageId, ackType, properties));

            // Consumer acknowledgment operation immediately succeeds. In any case, if we're not able to send ack to broker,
            // the messages will be re-delivered
        }
        private void NegativeAcknowledge(Messages messages)
        {
            messages.ToList().ForEach(x=> NegativeAcknowledge(x.MessageId));
        }
        private void NegativeAcknowledge(IMessageId messageId)
        {
            _negativeAcksTracker.Tell(new Add(messageId));

            // Ensure the message is not redelivered for ack-timeout, since we did receive an "ack"
            _unAckedMessageTracker.Tell(new Remove(messageId));
        }


        private void OnAcknowledge(IMessageId messageId, Exception exception)
        {
            _interceptors.OnAcknowledge(Self, messageId, exception);
        }

        private void OnAcknowledgeCumulative(IMessageId messageId, Exception exception)
        {
            _interceptors.OnAcknowledgeCumulative(Self, messageId, exception);
        }

        private void OnNegativeAcksSend(ISet<IMessageId> messageIds)
        {
            _interceptors.OnNegativeAcksSend(Self, messageIds);
        }

        private void OnAckTimeoutSend(ISet<IMessageId> messageIds)
        {
            _interceptors.OnAckTimeoutSend(Self, messageIds);
        }

        private bool CanEnqueueMessage(Message message)
        {
            // Default behavior, can be overridden in subclasses
            return true;
        }

        private void TrackMessage(Message msg)
        {
            if (msg != null)
            {
                TrackMessage(msg.MessageId);
            }
        }
        private void TrackMessage(IMessageId messageId)
        {
            if (_conf.AckTimeoutMillis > 0)
            {
                MessageId msgid;
                if (messageId is BatchMessageId b)
                {
                    // do not add each item in batch message into tracker
                    msgid = new MessageId(b.LedgerId, b.EntryId, _partitionIndex);
                }
                else
                {
                    msgid = (MessageId)messageId;
                }
                if (_hasParentConsumer)
                {
                    // we should no longer track this message, TopicsConsumer will take care from now onwards
                    _unAckedMessageTracker.Tell(new Remove(msgid));
                }
                else
                {
                    _unAckedMessageTracker.Tell(new Add(msgid));
                }
            }
        }


        private EncryptionContext CreateEncryptionContext(MessageMetadata msgMetadata)
        {

            EncryptionContext encryptionCtx = null;
            if (msgMetadata.EncryptionKeys.Count > 0)
            {
                IDictionary<string, EncryptionContext.EncryptionKey> keys = new Dictionary<string, EncryptionContext.EncryptionKey>();
                foreach(var kv in msgMetadata.EncryptionKeys)
                {
                    var neC = new EncryptionContext.EncryptionKey
                    {
                        KeyValue = (sbyte[]) (object) kv.Value,
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
                encryptionCtx = new EncryptionContext();
                var encParam = new sbyte[MessageCryptoFields.IvLen];
                msgMetadata.EncryptionParam.CopyTo((byte[])(object)encParam, 0);
                int? batchSize = msgMetadata.NumMessagesInBatch > 0 ? msgMetadata.NumMessagesInBatch : 0;
                encryptionCtx.Keys = keys;
                encryptionCtx.Param = encParam;
                encryptionCtx.Algorithm = msgMetadata.EncryptionAlgo;
                encryptionCtx.CompressionType = (int)msgMetadata.Compression;//CompressionCodecProvider.ConvertFromWireProtocol(msgMetadata.Compression);
                encryptionCtx.UncompressedMessageSize = (int)msgMetadata.UncompressedSize;
                encryptionCtx.BatchSize = batchSize;
            }
            return encryptionCtx;
        }
        private bool IsPriorEntryIndex(long idx)
        {
            return _conf.ResetIncludeHead ? idx < _startMessageId.EntryId : idx <= _startMessageId.EntryId;
        }
        private bool IsSameEntry(MessageIdData messageId)
        {
            return _startMessageId != null && (long)messageId.ledgerId == _startMessageId.LedgerId && (long)messageId.entryId == _startMessageId.EntryId;
        }
        private bool IsMessageUndecryptable(MessageMetadata msgMetadata)
        {
            return (msgMetadata.EncryptionKeys.Count > 0 && _conf.CryptoKeyReader == null && _conf.CryptoFailureAction == ConsumerCryptoFailureAction.Consume);
        }
        private byte[] DecryptPayloadIfNeeded(MessageIdData messageId, MessageMetadata msgMetadata, ReadOnlySequence<byte> payload)
        {

            if (msgMetadata.EncryptionKeys.Count == 0)
            {
                return payload.ToArray();
            }

            // If KeyReader is not configured throw exception based on config param
            if (_conf.CryptoKeyReader == null)
            {
                switch (_conf.CryptoFailureAction)
                {
                    case ConsumerCryptoFailureAction.Consume:
                        _log.Warning($"[{_topicName}][{_subscriptionName}][{_consumerName}] CryptoKeyReader interface is not implemented. Consuming encrypted message.");
                        return payload.ToArray();
                    case ConsumerCryptoFailureAction.Discard:
                        _log.Warning($"[{_topicName}][{_subscriptionName}][{_consumerName}] Skipping decryption since CryptoKeyReader interface is not implemented and config is set to discard");
                        DiscardMessage(messageId, CommandAck.ValidationError.DecryptionError);
                        return null;
                    case ConsumerCryptoFailureAction.Fail:
                        var m = new MessageId((long)messageId.ledgerId, (long)messageId.entryId, _partitionIndex);
                        _log.Error($"[{_topicName}][{_subscriptionName}][{_consumerName}][{m}] Message delivery failed since CryptoKeyReader interface is not implemented to consume encrypted message");
                        _unAckedMessageTracker.Tell(new Add(m));
                        return null;
                }
            }

            var decryptedData = _msgCrypto.Decrypt(msgMetadata, payload.ToArray(), _conf.CryptoKeyReader);
            if (decryptedData != null)
            {
                return decryptedData;
            }

            switch (_conf.CryptoFailureAction)
            {
                case ConsumerCryptoFailureAction.Consume:
                    // Note, batch message will fail to consume even if config is set to consume
                    _log.Warning($"[{_topicName}][{_subscriptionName}][{_consumerName}][{messageId}] Decryption failed. Consuming encrypted message since config is set to consume.");
                    
                    return payload.ToArray();
                case ConsumerCryptoFailureAction.Discard:
                    _log.Warning($"[{_topicName}][{_subscriptionName}][{_consumerName}][{messageId}] Discarding message since decryption failed and config is set to discard");
                    DiscardMessage(messageId, CommandAck.ValidationError.DecryptionError);
                    return null;
                case ConsumerCryptoFailureAction.Fail:
                    var m = new MessageId((long)messageId.ledgerId, (long)messageId.entryId, _partitionIndex);
                    _log.Error($"[{_topicName}][{_subscriptionName}][{_consumerName}][{m}] Message delivery failed since unable to decrypt incoming message");
                    _unAckedMessageTracker.Tell( new Add(m));
                    return null;
            }
            return null;
        }
        private void AckMessage(AckMessage message)
        {
            SendAcknowledge(message.MessageId, CommandAck.AckType.Individual, new Dictionary<string, long>(), null);
        }
        private void ActiveConsumerChanged(bool isActive)
        {
            if (_consumerEventListener == null)
            {
                return;
            }

            if (isActive)
            {
                _consumerEventListener.BecameActive(_consumerName, _partitionIndex);
            }
            else
            {
                _consumerEventListener.BecameInactive(_consumerName, _partitionIndex);
            }
        }
        private bool IsCumulativeAcknowledgementAllowed(CommandSubscribe.SubType type)
        {
            return CommandSubscribe.SubType.Shared != type && CommandSubscribe.SubType.KeyShared != type;
        }
        
        private void AckMessages(AckMessages message)
        {
            SendAcknowledge(message.MessageId, CommandAck.AckType.Cumulative, new Dictionary<string, long>(), null );
        }
        private byte[] UncompressPayloadIfNeeded(MessageIdData messageId, MessageMetadata msgMetadata, byte[] payload, bool checkMaxMessageSize)
        {
            var compressionType = msgMetadata.Compression;
            var codec = CompressionCodecProvider.GetCompressionCodec((int)compressionType);
            var uncompressedSize = (int)msgMetadata.UncompressedSize;
            var payloadSize = payload.Length;
            if (checkMaxMessageSize && payloadSize > _serverInfo.MaxMessageSize)
            {
                // payload Size is itself corrupted since it cannot be bigger than the MaxMessageSize
                _log.Error($"[{_topicName}][{_subscriptionName}] Got corrupted payload message Size {payloadSize} at {messageId}");
                DiscardCorruptedMessage(messageId, CommandAck.ValidationError.UncompressedSizeCorruption);
                return null;
            }

            try
            {
                var uncompressedPayload = codec.Decode(payload, uncompressedSize);
                return uncompressedPayload;
            }
            catch (IOException e)
            {
                _log.Error($"[{_topicName}][{_subscriptionName}] Failed to decompress message with {compressionType} at {messageId}: {e.Message}");
                DiscardCorruptedMessage(messageId, CommandAck.ValidationError.DecompressionError);
                return null;
            }
        }
        private void DiscardCorruptedMessage(MessageIdData messageId, CommandAck.ValidationError validationError)
        {
            _log.Error($"[{_topicName}][{_subscriptionName}] Discarding corrupted message at {messageId.ledgerId}:{messageId.entryId}");
            DiscardMessage(messageId, validationError);
        }

        private void DiscardMessage(MessageIdData messageId, CommandAck.ValidationError validationError)
        {
            var requestId = Interlocked.Increment(ref IdGenerators.RequestId);
            var cmd = Commands.NewAck(_consumerid, (long)messageId.ledgerId, (long)messageId.entryId, messageId.AckSets, CommandAck.AckType.Individual, validationError, new Dictionary<string, long>());
            var payload = new Payload(cmd, requestId, "NewAck");
            _broker.Tell(payload);
        }
        
        private void SendGetSchemaCommand(sbyte[] version)
        {
            var requestId = Interlocked.Increment(ref IdGenerators.RequestId);
            var request = Commands.NewGetSchema(requestId, _topicName.ToString(), BytesSchemaVersion.Of(version));
            var payload = new Payload(request, requestId, "GetSchema");
            _broker.Tell(payload);
        }
        private void SendGetSchemaCommand(string topic, sbyte[] version)
        {
            var requestId = Interlocked.Increment(ref IdGenerators.RequestId);
            var request = Commands.NewGetSchema(requestId, topic, BytesSchemaVersion.Of(version));
            var payload = new Payload(request, requestId, "GetSchema");
            var ask = _broker.Ask<SchemaResponse>(payload);
            var schema = SynchronizationContextSwitcher.NoContext(async () => await ask).Result;
        }
        public void NewSubscribe()
        {
            var requestid = Interlocked.Increment(ref IdGenerators.RequestId);
            var isDurable = _subscriptionMode == SubscriptionMode.Durable;
            MessageIdData startMessageIdData;
            if (isDurable)
            {
                // For regular durable subscriptions, the message id from where to restart will be determined by the broker.
                startMessageIdData = null;

            }
            else
            {
                // For non-durable we are going to restart from the next entry
                var builder = new MessageIdData
                {
                    ledgerId = (ulong)(_startMessageId.LedgerId),
                    entryId = (ulong)(_startMessageId.EntryId),
                    BatchIndex = (_startMessageId.BatchIndex)
                };

                startMessageIdData = builder;
            }
            var si = (SchemaInfo)_schema?.SchemaInfo;
            if (si != null && (SchemaType.Bytes == si.Type || SchemaType.None == si.Type))
            {
                // don't set schema for Schema.BYTES
                si = null;
            }

            var intial = Enum.GetValues(typeof(CommandSubscribe.InitialPosition)).Cast<CommandSubscribe.InitialPosition>().ToList()[_conf.SubscriptionInitialPosition.Value];
            // startMessageRollbackDurationInSec should be consider only once when consumer connects to first time
            var startMessageRollbackDuration = (_startMessageRollbackDurationInSec > 0 && _startMessageId.Equals(_initialStartMessageId)) ? _startMessageRollbackDurationInSec : 0;
            if (_conf.SubscriptionType == CommandSubscribe.SubType.Exclusive && _hasParentConsumer)
                _subscriptionName = _subscriptionName + $"-{_consumerid}";
            var request = Commands.NewSubscribe(_topicName.ToString(), _subscriptionName, _consumerid, requestid, _conf.SubscriptionType, _conf.PriorityLevel, _consumerName, isDurable, startMessageIdData, _conf.Properties, _conf.ReadCompacted, _conf.ReplicateSubscriptionState, intial, startMessageRollbackDuration, si, _createTopicIfDoesNotExist, _conf.KeySharedPolicy);
            var payload = new Payload(request, requestid, "NewSubscribe");
            _broker.Tell(payload);
        }
        private void SendFlow(int numbs)
        {
            var requestid = Interlocked.Increment(ref IdGenerators.RequestId);
            var reqt = Commands.NewFlow(_consumerid, numbs);
            var payload = new Payload(reqt, requestid, "NewFlow");
            _broker.Tell(payload);
        }

        private void BecomeLookUp()
        {
            SendBrokerLookUpCommand();
            Become(LookUp);
        }

        private void BecomeActive()
        {
            Context.Watch(_broker);
            Become(Active);
        }

        private void Active()
        {
            Receive<Unsubscribe>(u => Unsubscribe());
            Receive<NegativeAck>(n => { NegativeAcknowledge(n.MessageId); });
            Receive<INeedBroker>(x => { Sender.Tell(_broker); });
            Receive<Terminated>(_ =>
            {
                foreach (var c in Context.GetChildren())
                {
                    Context.Stop(c);
                }

                Become(RecreatingConsumer);
            });
            Receive<SendFlow>(f =>
            {
                SendFlow(Convert.ToInt32(f.Size));
            });
            Receive<OnNegativeAcksSend>(x =>
            {
                OnNegativeAcksSend(x.MessageIds);
            });
            Receive<LastMessageId>(x =>
            {
                InternalGetLastMessageId();
            });
            Receive<ConsumerClosed>(_ =>
            {
                Become(RecreatingConsumer);
            });
            Receive<MessageReceived>(m =>
            {
                var msgId = new MessageIdData
                {
                    entryId = (ulong)m.MessageId.EntryId,
                    ledgerId = (ulong)m.MessageId.LedgerId,
                    Partition = m.MessageId.Partition,
                    BatchIndex = m.MessageId.BatchIndex,
                    AckSets = m.MessageId.AckSet
                };
                MessageReceived(msgId, m.RedeliveryCount, m.Data, m.MessageId.AckSet);
                IncreaseAvailablePermits();
            });
            Receive<AckMessage>(AckMessage);
            Receive<AckMessages>(AckMessages);
            Receive<AckTimeoutSend>(ack =>
            {
                OnAckTimeoutSend(ack.MessageIds);
            });
            Receive<RedeliverMessages>(r => { RedeliverUnacknowledgedMessages(r.Messages); });
            Receive<UnAckedChunckedMessageIdSequenceMapCmd>(r =>
            {
                MessageId msgid;
                if (r.MessageId is BatchMessageId id)
                    msgid = new MessageId(id.LedgerId, id.EntryId, id.PartitionIndex);
                else msgid = (MessageId) r.MessageId;
                if (r.Command == UnAckedCommand.Remove)
                    _unAckedChunkedMessageIdSequenceMap.Remove(msgid);
                else if(_unAckedChunkedMessageIdSequenceMap.ContainsKey(msgid))
                    Sender.Tell(new UnAckedChunckedMessageIdSequenceMapCmdResponse(_unAckedChunkedMessageIdSequenceMap[msgid]));
                else
                    Sender.Tell(new UnAckedChunckedMessageIdSequenceMapCmdResponse(Array.Empty<MessageId>()));
            });
            Receive<RedeliverUnacknowledgedMessages>(x =>
            {
                RedeliverUnacknowledgedMessages(x.MessageIds);
            });
            Receive<Seek>(s =>
            {
                switch (s.Type)
                {
                    case SeekType.Timestamp:
                        Seek(long.Parse(s.Input.ToString()));
                        break;
                    default:
                        var v = s.Input.ToString().Trim().Split(",");//format ledger,entry
                        Seek(new MessageId(long.Parse(v[0].Trim()), long.Parse(v[1].Trim()), _partitionIndex));
                        break;
                }
            });
            if (_seek != null)
            {
                switch (_seek.Type)
                {
                    case SeekType.Timestamp:
                        Seek(long.Parse(_seek.Input.ToString()));
                        break;
                    default:
                        var v = _seek.Input.ToString().Trim().Split(",");//format ledger,entry
                        Seek(new MessageId(long.Parse(v[0].Trim()), long.Parse(v[1].Trim()), _partitionIndex));
                        break;
                }
            }

        }

        protected override void PostRestart(Exception reason)
        {
            //base.PostRestart(reason);
            _seek = null;//seek seems to crash consumer, set it to null to avoid restarting more than once
        }

        private void RecreatingConsumer()
        {
            BecomeLookUp();
        }
        private void LookUp()
        {
            Receive<BrokerLookUp>(l =>
            {
                var uri = _conf.UseTls ? new Uri(l.BrokerServiceUrlTls) : new Uri(l.BrokerServiceUrl);
                if (_clientConfiguration.UseProxy)
                    _broker = Context.ActorOf(ClientConnection.Prop(new Uri(_clientConfiguration.ServiceUrl), _clientConfiguration, Self, $"{uri.Host}:{uri.Port}"));
                else
                    _broker = Context.ActorOf(ClientConnection.Prop(uri, _clientConfiguration, Self));
            });
            Receive<ConnectedServerInfo>(s =>
            {
                _consumerEventListener.Log($"Connected to Pulsar Server. Subscribing Consumer '{_consumerid}' to topic '{_topicName}'");
                _serverInfo = s;
                if (_schema != null && _schema.SupportSchemaVersioning())
                {

                    if (_schema.RequireFetchingSchemaInfo())
                    {
                        SendGetSchemaCommand(null);
                    }
                    else
                    {
                        NewSubscribe();
                    }

                }
                else
                {
                    NewSubscribe();
                }

            });
            Receive<SchemaResponse>(s =>
            {
                var schema = new SchemaDataBuilder()
                    .SetData((sbyte[])(object)s.Schema)
                    .SetProperties(s.Properties)
                    .SetType(SchemaType.ValueOf((int)s.Type))
                    .Build();
                _schema = ISchema.GetSchema(schema.ToSchemaInfo());
                NewSubscribe();
            });
            Receive<NullSchema>(n =>
            {
                NewSubscribe();
            });
            Receive<SubscribeSuccess>(s =>
            {
                if (s.HasSchema)
                {
                    var schemaInfo = new SchemaInfo
                    {
                        Name = s.Schema.Name,
                        Properties = s.Schema.Properties.ToDictionary(x => x.Key, x => x.Value),
                        Type = s.Schema.type == Schema.Type.Json ? SchemaType.Json : SchemaType.None,
                        Schema = (sbyte[])(object)s.Schema.SchemaData
                    };
                    _schema = ISchema.GetSchema(schemaInfo);
                }
                SendFlow(_requestedFlowPermits);
                var created = new CreatedConsumer(Self, _topicName.ToString(), _consumerName);
                _consumerEventListener.Created(created);
                _pulsarManager.Tell(created);

                if (_topicName.Persistent)
                {
                    _acknowledgmentsGroupingTracker = Context.ActorOf(PersistentAcknowledgmentsGroupingTracker.Prop(_broker, _consumerid, _conf), "PersistentAcknowledgmentsGroupingTracker");
                }
                else
                {
                    _acknowledgmentsGroupingTracker = Context.ActorOf(NonPersistentAcknowledgmentGroupingTracker.Prop(), "NonPersistentAcknowledgmentGroupingTracker");
                }
                BecomeActive();
                Stash.UnstashAll();
            });
            Receive<PulsarError>(e =>
            {
                _consumerEventListener.Error(new Exception($"{e.Error}: {e.Message}"));
            });
            ReceiveAny(_=> Stash.Stash());
        }
        private void SendBrokerLookUpCommand()
        {
            var requestid = Interlocked.Increment(ref IdGenerators.RequestId);
            var request = Commands.NewLookup(_topicName.ToString(), _clientConfiguration.ListenerName, false, requestid);
            var load = new Payload(request, requestid, "BrokerLookUp");
            _network.Tell(load);
        }
        
        public IStash Stash { get; set; }
        private void Unsubscribe()
        {
            var requestId = Interlocked.Increment(ref IdGenerators.RequestId);
            var cmd = Commands.NewUnsubscribe(_consumerid, requestId);
            var payload = new Payload(cmd, requestId, "NewUnsubscribe");
            _broker.Tell(payload);
        }
        private void PendingBatchReceiveTask()
        {
            long timeToWaitMs;

            if (_pendingBatchReceives == null)
            {
                _pendingBatchReceives =  new ConcurrentQueue<OpBatchReceive>();
            }

            _pendingBatchReceives.TryPeek(out var firstOpBatchReceive);
            timeToWaitMs = _batchReceivePolicy.TimeoutMs;

            while (firstOpBatchReceive != null)
            {
                // If there is at least one batch receive, calculate the diff between the batch receive timeout
                // and the elapsed time since the operation was created.
                var diff = _batchReceivePolicy.TimeoutMs - (long)ConvertTimeUnits.ConvertNanosecondsToMilliseconds(DateTime.Now.Ticks - firstOpBatchReceive.CreatedAt);
                if (diff <= 0)
                {
                    // The diff is less than or equal to zero, meaning that the batch receive has been timed out.
                    // complete the OpBatchReceive and continue to check the next OpBatchReceive in pendingBatchReceives.
                    _pendingBatchReceives.TryDequeue(out var op);
                    CompleteOpBatchReceive(op);
                    _pendingBatchReceives.TryPeek(out firstOpBatchReceive);
                }
                else
                {
                    // The diff is greater than zero, set the timeout to the diff value
                    timeToWaitMs = diff;
                    break;
                }
            }
            _batchReceiveTimeout = _system.Scheduler.Advanced.ScheduleOnceCancelable(TimeSpan.FromMilliseconds(timeToWaitMs), PendingBatchReceiveTask);
        }
        private void CompleteOpBatchReceive(OpBatchReceive op)
        {
            NotifyPendingBatchReceivedCallBack(op);
        }
        private bool HasPendingBatchReceive()
        {
            return _pendingBatchReceives != null && _pendingBatchReceives.Count > 0;
        }

        private void SetTerminated()
        {
            _log.Info($"[{_subscriptionName}] [{_topicName}] [{_consumerName}] Consumer has reached the end of topic");
            _hasReachedEndOfTopic = true;
            // Propagate notification to listener
            _listener?.ReachedEndOfTopic(Self);
        }

        private void Seek(long timestamp)
        {

            var requestId = Interlocked.Increment(ref IdGenerators.RequestId);
            var cmd = Commands.NewSeek(_consumerid, requestId, timestamp);

            _log.Info($"[{_topicName}][{_subscriptionName}] Seek subscription to publish time {timestamp}");

            var payload = new Payload(cmd, requestId, "NewSeek");
            _broker.Tell(payload);

            _log.Info($"[{_topicName}][{_subscriptionName}] Successfully reset subscription to publish time {timestamp}");

        }

        private void Seek(IMessageId messageId)
        {
            var requestId = Interlocked.Increment(ref IdGenerators.RequestId);
            var msgId = (MessageId)messageId;

            var cmd = Commands.NewSeek(_consumerid, requestId, msgId.LedgerId, msgId.EntryId);

            _log.Info($"[{_topicName}][{_subscriptionName}] Seek subscription to message id {messageId}");
            var payload = new Payload(cmd, requestId, "NewSeek");
            _broker.Tell(payload);
            _log.Info($"[{_topicName}][{_subscriptionName}] Successfully reset subscription to message id {messageId}");
            _acknowledgmentsGroupingTracker.Tell(FlushAndClean.Instance);
            _seekMessageId = new BatchMessageId((MessageId)messageId);
            _duringSeek = true;
            _lastDequeuedMessageId = MessageIdFields.Earliest;
            _incomingMessages.Clear();
            _incomingMessagesSize = 0;

        }

    }
    public class ChunkedMessageCtx
    {

        internal int TotalChunks = -1;
        internal List<byte> ChunkedMsgBuffer;
        internal int LastChunkedMessageId = -1;
        internal MessageId[] ChunkedMessageIds;
        internal long ReceivedTime;

        internal static ChunkedMessageCtx Get(int numChunksFromMsg, List<byte> chunkedMsg)
        {
            var ctx = new ChunkedMessageCtx
            {
                TotalChunks = numChunksFromMsg,
                ChunkedMsgBuffer = chunkedMsg,
                ChunkedMessageIds = new MessageId[numChunksFromMsg],
                ReceivedTime = DateTimeHelper.CurrentUnixTimeMillis()
            };
            return ctx;
        }

        internal void Recycle()
        {
            TotalChunks = -1;
            ChunkedMessageIds = null;
            ChunkedMsgBuffer = null;
            LastChunkedMessageId = -1;
        }
    }
}
