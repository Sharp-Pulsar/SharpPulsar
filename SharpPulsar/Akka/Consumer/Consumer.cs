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
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading;
using SharpPulsar.Batch;
using SharpPulsar.Impl.Crypto;

namespace SharpPulsar.Akka.Consumer
{
    public class Consumer:ReceiveActor, IWithUnboundedStash
    {
        private readonly int _partitionIndex;
        private readonly ClientConfigurationData _clientConfiguration;
        private IActorRef _broker;
        private readonly ConsumerConfigurationData _conf;
        private readonly string _consumerName;
        private string _subscriptionName;
        private ISchema _schema;
        private readonly List<IConsumerInterceptor> _interceptors;
        private readonly IMessageListener _listener;
        private readonly IConsumerEventListener _consumerEventListener;
        private readonly TopicName _topicName;
        private readonly List<string> _pendingChunckedMessageUuidQueue;
        private readonly IActorRef _network;
        private int _requestedFlowPermits;
        private readonly IDictionary<MessageId, IList<Message>> _possibleSendToDeadLetterTopicMessages;
        private Seek _seek;
        private readonly DeadLetterPolicy _deadLetterPolicy;
        private readonly bool _createTopicIfDoesNotExist;
        private readonly SubscriptionMode _subscriptionMode;
        private volatile BatchMessageId _startMessageId;
        private readonly BatchMessageId _initialStartMessageId = (BatchMessageId)MessageIdFields.Earliest;
        private  ConnectedServerInfo _serverInfo;
        private readonly long _startMessageRollbackDurationInSec;
        private readonly MessageCrypto _msgCrypto;
        private readonly bool _hasParentConsumer;
        private readonly long _consumerid;
        private ICancelable _consumerRecreator;
        private readonly Dictionary<MessageId, MessageId[]> _unAckedChunkedMessageIdSequenceMap;

        private readonly bool _eventSourced;
        private readonly Dictionary<string, ChunkedMessageCtx> _chunkedMessagesMap = new Dictionary<string, ChunkedMessageCtx>();
        private int _pendingChunckedMessageCount = 0;
        private long _expireTimeOfIncompleteChunkedMessageMillis = 0;
        private bool _expireChunkMessageTaskScheduled = false;
        private readonly int _maxPendingChuckedMessage;
        // if queue size is reasonable (most of the time equal to number of producers try to publish messages concurrently on
        // the topic) then it guards against broken chuncked message which was not fully published
        private readonly bool _autoAckOldestChunkedMessageOnQueueFull;
        private readonly IActorRef _pulsarManager;
        public Consumer(ClientConfigurationData clientConfiguration, string topic, ConsumerConfigurationData configuration, long consumerid, IActorRef network, bool hasParentConsumer, int partitionIndex, SubscriptionMode mode, Seek seek, IActorRef pulsarManager, bool eventSourced = false)
        {
            _pulsarManager = pulsarManager;
            _eventSourced = eventSourced;
            _possibleSendToDeadLetterTopicMessages = new Dictionary<MessageId, IList<Message>>();
            _listener = configuration.MessageListener;
            _createTopicIfDoesNotExist = configuration.ForceTopicCreation;
            _subscriptionName = configuration.SubscriptionName;
            _consumerEventListener = configuration.ConsumerEventListener;
            _startMessageId = configuration.StartMessageId;
            _subscriptionMode = mode;
            _partitionIndex = partitionIndex;
            _hasParentConsumer = hasParentConsumer;
            _requestedFlowPermits = configuration.ReceiverQueueSize;
            _conf = configuration;
            _interceptors = configuration.Interceptors;
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

            // Create msgCrypto if not created already
            _msgCrypto = new MessageCrypto($"[{configuration.SingleTopic}] [{configuration.SubscriptionName}]", false);
            
            ReceiveAny(x => Stash.Stash());
            BecomeLookUp();
        }

        protected override void PostStop()
        {
            var requestid = Interlocked.Increment(ref IdGenerators.RequestId);
            var cmd = Commands.NewCloseConsumer(_consumerid, requestid);
            var payload = new Payload(cmd, requestid, "CloseConsumer");
            _broker.Tell(payload);
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
        
        private bool HasReachedEndOfTopic()
        {
            return false;
        }
        private void LastMessageId()
        {
            var requestid = Interlocked.Increment(ref IdGenerators.RequestId);
            var request = Commands.NewGetLastMessageId(_consumerid, requestid);
            var payload = new Payload(request, requestid, "NewGetLastMessageId");
            _broker.Tell(payload);
        }
        private void HandleMessage(MessageIdData messageId, int redeliveryCount, ReadOnlySequence<byte> data)
        {
            if (Context.System.Log.IsDebugEnabled)
            {
                Context.System.Log.Debug($"[{_topicName}][{_subscriptionName}] Received message: {messageId.ledgerId}/{messageId.entryId}");
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
            var msgId = new MessageId((long)messageId.ledgerId, (long)messageId.entryId, _partitionIndex, messageId.AckSets);
            
            var decryptedPayload = DecryptPayloadIfNeeded(messageId, msgMetadata, payload);

            var isMessageUndecryptable = IsMessageUndecryptable(msgMetadata);

            if (decryptedPayload == null)
            {
                // Message was discarded or CryptoKeyReader isn't implemented
                return;
            }

            // uncompress decryptedPayload and release decryptedPayload-ByteBuf
            var uncompressedPayload = isMessageUndecryptable ? decryptedPayload : UncompressPayloadIfNeeded(messageId, msgMetadata, decryptedPayload, true);
            
            if (uncompressedPayload == null)
            {
                // Message was discarded on decompression error
                return;
            }

            // if message is not decryptable then it can't be parsed as a batch-message. so, add EncyrptionCtx to message
            // and return undecrypted payload
            if (isMessageUndecryptable || (numMessages == 1 && msgMetadata.NumMessagesInBatch > 0))
            {
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
                    if (Context.System.Log.IsDebugEnabled)
                    {
                        Context.System.Log.Debug($"[{_subscriptionName}] [{_consumerName}] Ignoring message from before the startMessageId: {_startMessageId}");
                    }

                    return;
                }

                var message = new Message(_topicName.ToString(), msgId, msgMetadata, uncompressedPayload, CreateEncryptionContext(msgMetadata), _schema, redeliveryCount);
               if (_hasParentConsumer)
                    Context.Parent.Tell(new ConsumedMessage(Self, message));
               else
               {
                   if(_conf.ConsumptionType == ConsumptionType.Listener)
                        _listener.Received(Self, message);
                   else if(_conf.ConsumptionType == ConsumptionType.Queue)
                       _pulsarManager.Tell(new ConsumedMessage(Self, message));
                }
            }
            else
            {
                // handle batch message enqueuing; uncompressed payload has all messages in batch
                //ReceiveIndividualMessagesFromBatch(msgMetadata, redeliveryCount, uncompressedPayload, messageId);
                _consumerEventListener.Log("Batching is not supported");
            }

        }
        private byte[] ProcessMessageChunk(byte[] compressedPayload, MessageMetadata msgMetadata, MessageId msgId, MessageIdData messageId)
        {

            // Lazy task scheduling to expire incomplete chunk message
            if (!_expireChunkMessageTaskScheduled && _expireTimeOfIncompleteChunkedMessageMillis > 0)
            {
                Context.System.Scheduler.Advanced.ScheduleRepeatedly(TimeSpan.FromMilliseconds(_expireTimeOfIncompleteChunkedMessageMillis), TimeSpan.FromMilliseconds(_expireTimeOfIncompleteChunkedMessageMillis), RemoveExpireIncompleteChunkedMessages);
                _expireChunkMessageTaskScheduled = true;
            }

            if (msgMetadata.ChunkId == 0)
            {
                var chunkedMsgBuffer = compressedPayload;
                var totalChunks = msgMetadata.NumChunksFromMsg;
                _chunkedMessagesMap.TryAdd(msgMetadata.Uuid, ChunkedMessageCtx.Get(totalChunks, chunkedMsgBuffer));
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
                Context.System.Log.Info($"Received unexpected chunk messageId {msgId}, last-chunk-id {chunkedMsgCtx?.LastChunkedMessageId ?? 0}, chunkId = {msgMetadata.ChunkId}, total-chunks {msgMetadata.TotalChunkMsgSize}");
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
            if (Context.System.Log.IsDebugEnabled)
            {
                Context.System.Log.Debug($"Chunked message completed chunkId {msgMetadata.ChunkId}, total-chunks {msgMetadata.NumChunksFromMsg}, msgId {msgId} sequenceId {msgMetadata.SequenceId}");
            }
            // remove buffer from the map, add chunked messageId to unack-message tracker, and reduce pending-chunked-message count
            _chunkedMessagesMap.Remove(msgMetadata.Uuid);
            _unAckedChunkedMessageIdSequenceMap.Add(msgId, chunkedMsgCtx.ChunkedMessageIds);
            _pendingChunckedMessageCount--;
            compressedPayload = chunkedMsgCtx.ChunkedMsgBuffer.ToArray();
            AckMultiMessage(chunkedMsgCtx.ChunkedMessageIds);
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
                        Context.System.Log.Info($"Removing chunk message-id {msgId}");
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

        private void DoAcknowledge(MessageId messageId, CommandAck.AckType ackType, IDictionary<string, long> properties, ITransaction txnImpl)
        {
            if (CommandAck.AckType.Individual.Equals(ackType))
            {
                OnAcknowledge(messageId, null);
            }
            else if (CommandAck.AckType.Cumulative.Equals(ackType))
            {
                OnAcknowledgeCumulative(messageId, null);
            }
            SendAcknowledge(messageId, ackType, properties, txnImpl);
        }
        private void SendAcknowledge(MessageId messageId, CommandAck.AckType ackType, IDictionary<string, long> properties, ITransaction txnImpl)
        {
            var msgId = (MessageId)messageId;

            if (ackType == CommandAck.AckType.Individual)
            {
                OnAcknowledge(messageId, null);
            }
            else if (ackType == CommandAck.AckType.Cumulative)
            {
                OnAcknowledgeCumulative(messageId, null);
            }

            //acknowledgmentsGroupingTracker.addAcknowledgment(msgId, ackType, properties);

        }

        private void OnAcknowledge(MessageId messageId, Exception exception)
        {
            foreach (var interceptor in _interceptors)
            {
                interceptor.OnAcknowledge(Self, messageId, exception);
            }
        }

        private void OnAcknowledgeCumulative(MessageId messageId, Exception exception)
        {
            foreach (var interceptor in _interceptors)
            {
                interceptor.OnAcknowledgeCumulative(Self, messageId, exception);
            }
        }

        private void OnNegativeAcksSend(ISet<IMessageId> messageIds)
        {
            foreach (var interceptor in _interceptors)
            {
                interceptor.OnNegativeAcksSend(Self, messageIds);
            }
        }

        private void OnAckTimeoutSend(ISet<IMessageId> messageIds)
        {
            foreach (var interceptor in _interceptors)
            {
                interceptor.OnAckTimeoutSend(Self, messageIds);
            }
        }

        public bool CanEnqueueMessage(Message message)
        {
            // Default behavior, can be overridden in subclasses
            return true;
        }

        public virtual void TrackMessage(Message msg)
        {
            if (msg != null)
            {
                TrackMessage(msg.MessageId);
            }
        }
        public virtual void TrackMessage(IMessageId messageId)
        {
            if (_conf.AckTimeoutMillis > 0 && messageId is MessageId id1)
            {
                var id = id1;
                if (id is BatchMessageId)
                {
                    // do not add each item in batch message into tracker
                    id = new MessageId(id.LedgerId, id.EntryId, _partitionIndex, null);
                }
                if (_hasParentConsumer)
                {
                    //TODO: check parent consumer here
                    // we should no longer track this message, TopicsConsumer will take care from now onwards
                    //UnAckedMessageTracker.remove(id);
                }
                else
                {
                    //UnAckedMessageTracker.add(id);
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
                var encParam = new sbyte[MessageCrypto.IvLen];
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
                        Context.System.Log.Warning($"[{_topicName}][{_subscriptionName}][{_consumerName}] CryptoKeyReader interface is not implemented. Consuming encrypted message.");
                        return payload.ToArray();
                    case ConsumerCryptoFailureAction.Discard:
                        Context.System.Log.Warning($"[{_topicName}][{_subscriptionName}][{_consumerName}] Skipping decryption since CryptoKeyReader interface is not implemented and config is set to discard");
                        DiscardMessage(messageId, CommandAck.ValidationError.DecryptionError);
                        return null;
                    case ConsumerCryptoFailureAction.Fail:
                        IMessageId m = new MessageId((long)messageId.ledgerId, (long)messageId.entryId, _partitionIndex, messageId.AckSets);
                        Context.System.Log.Error($"[{_topicName}][{_subscriptionName}][{_consumerName}][{m}] Message delivery failed since CryptoKeyReader interface is not implemented to consume encrypted message");
                        //UnAckedMessageTracker.Add(m);
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
                    Context.System.Log.Warning($"[{_topicName}][{_subscriptionName}][{_consumerName}][{messageId}] Decryption failed. Consuming encrypted message since config is set to consume.");
                    
                    return payload.ToArray();
                case ConsumerCryptoFailureAction.Discard:
                    Context.System.Log.Warning($"[{_topicName}][{_subscriptionName}][{_consumerName}][{messageId}] Discarding message since decryption failed and config is set to discard");
                    DiscardMessage(messageId, CommandAck.ValidationError.DecryptionError);
                    return null;
                case ConsumerCryptoFailureAction.Fail:
                    var m = new MessageId((long)messageId.ledgerId, (long)messageId.entryId, _partitionIndex, messageId.AckSets);
                    Context.System.Log.Error($"[{_topicName}][{_subscriptionName}][{_consumerName}][{m}] Message delivery failed since unable to decrypt incoming message");
                    //UnAckedMessageTracker.Add(m);
                    return null;
            }
            return null;
        }
        private void AckMessage(AckMessage message)
        {
            var requestid = Interlocked.Increment(ref IdGenerators.RequestId);
            var cmd = Commands.NewAck(_consumerid, message.MessageId.LedgerId, message.MessageId.EntryId, message.MessageId.AckSet, CommandAck.AckType.Individual, null, new Dictionary<string, long>());
            var payload = new Payload(cmd, requestid, "AckMessages");
            _broker.Tell(payload);
        }
        private void AckMultiMessage(AckMultiMessage multiMessage)
        {
            IList<KeyValuePair<long, long>> entriesToAck = new List<KeyValuePair<long, long>>(multiMessage.MessageIds.Count);
            foreach (var m in multiMessage.MessageIds)
            {
                entriesToAck.Add(new KeyValuePair<long, long>(m.LedgerId, m.EntryId));
            }
            SendAckMultiMessages(entriesToAck);
        }
        private void AckMultiMessage(MessageId[] multiMessage)
        {
            IList<KeyValuePair<long, long>> entriesToAck = new List<KeyValuePair<long, long>>(multiMessage.Length);
            foreach (var m in multiMessage)
            {
                entriesToAck.Add(new KeyValuePair<long, long>(m.LedgerId, m.EntryId));
            }
            SendAckMultiMessages(entriesToAck);
        }
        private void SendAckMultiMessages(IList<KeyValuePair<long, long>> entries)
        {
            var requestid = Interlocked.Increment(ref IdGenerators.RequestId);
            var cmd = Commands.NewMultiMessageAck(_consumerid, entries);
            var payload = new Payload(cmd, requestid, "AckMultiMessages");
            _broker.Tell(payload);
        }
        private void AckMessages(AckMessages message)
        {
            var requestid = Interlocked.Increment(ref IdGenerators.RequestId);
            var cmd = Commands.NewAck(_consumerid, message.MessageId.LedgerId, message.MessageId.EntryId, message.MessageId.AckSet, CommandAck.AckType.Cumulative, null, new Dictionary<string, long>());
            var payload = new Payload(cmd, requestid, "AckMessages");
            _broker.Tell(payload);
        }
        private byte[] UncompressPayloadIfNeeded(MessageIdData messageId, MessageMetadata msgMetadata, byte[] payload, bool checkMaxMessageSize)
        {
            var compressionType = msgMetadata.Compression;
            var codec = CompressionCodecProvider.GetCompressionCodec((int)compressionType);
            var uncompressedSize = (int)msgMetadata.UncompressedSize;
            var payloadSize = payload.Length;
            if (checkMaxMessageSize && payloadSize > _serverInfo.MaxMessageSize)
            {
                // payload size is itself corrupted since it cannot be bigger than the MaxMessageSize
                Context.System.Log.Error($"[{_topicName}][{_subscriptionName}] Got corrupted payload message size {payloadSize} at {messageId}");
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
                Context.System.Log.Error($"[{_topicName}][{_subscriptionName}] Failed to decompress message with {compressionType} at {messageId}: {e.Message}");
                DiscardCorruptedMessage(messageId, CommandAck.ValidationError.DecompressionError);
                return null;
            }
        }
        private void DiscardCorruptedMessage(MessageIdData messageId, CommandAck.ValidationError validationError)
        {
            Context.System.Log.Error($"[{_topicName}][{_subscriptionName}] Discarding corrupted message at {messageId.ledgerId}:{messageId.entryId}");
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

            Receive<LastMessageId>(x =>
            {
                LastMessageId();
            });
            Receive<ConsumerClosed>(_ =>
            {
                Become(RecreatingConsumer);
            });
            Receive<LastMessageIdResponse>(x =>
            {
                _pulsarManager.Tell(new LastMessageIdReceived(_consumerid, _topicName.ToString(), x));
            });
            Receive<MessageReceived>(m =>
            {
                _requestedFlowPermits--;
                var msgId = new MessageIdData
                {
                    entryId = (ulong)m.MessageId.EntryId,
                    ledgerId = (ulong)m.MessageId.LedgerId,
                    Partition = m.MessageId.Partition,
                    BatchIndex = m.MessageId.BatchIndex,
                    AckSets = m.MessageId.AckSet
                };
                HandleMessage(msgId, m.RedeliveryCount, m.Data);
                if (!_eventSourced)
                    SendFlow(1);
            });
            Receive<AckMessage>(AckMessage);
            Receive<AckMessages>(AckMessages);
            Receive<AckMultiMessage>(AckMultiMessage);
            Receive<SubscribeSuccess>(s =>
            {
                if (_consumerRecreator != null)
                {
                    _consumerRecreator.Cancel();
                    _consumerRecreator = null;
                }
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
                //SendFlow(_requestedFlowPermits);
            });
            Receive<RedeliverMessages>(r => { RedeliverUnacknowledgedMessages(r.Messages); });
            Receive<Seek>(s =>
            {
                switch (s.Type)
                {
                    case SeekType.Timestamp:
                        var reqtid = Interlocked.Increment(ref IdGenerators.RequestId);
                        var req = Commands.NewSeek(_consumerid, reqtid, long.Parse(s.Input.ToString()));
                        var pay = new Payload(req, reqtid, "NewSeek");
                        _broker.Tell(pay);
                        break;
                    default:
                        var v = s.Input.ToString().Trim().Split(",");//format l,e
                        var requestid = Interlocked.Increment(ref IdGenerators.RequestId);
                        var request = Commands.NewSeek(_consumerid, requestid, long.Parse(v[0].Trim()), long.Parse(v[1].Trim()));
                        var payload = new Payload(request, requestid, "NewSeek");
                        _broker.Tell(payload);
                        break;
                }
            });
            if (_seek != null)
            {
                switch (_seek.Type)
                {
                    case SeekType.Timestamp:
                        var reqtid = Interlocked.Increment(ref IdGenerators.RequestId);
                        var req = Commands.NewSeek(_consumerid, reqtid, long.Parse(_seek.Input.ToString()));
                        var pay = new Payload(req, reqtid, "NewSeek");
                        _broker.Tell(pay);
                        break;
                    default:
                        var v = _seek.Input.ToString().Trim().Split(",");//format l,e
                        var requestid = Interlocked.Increment(ref IdGenerators.RequestId);
                        var request = Commands.NewSeek(_consumerid, requestid, long.Parse(v[0].Trim()), long.Parse(v[1].Trim()));
                        var payload = new Payload(request, requestid, "NewSeek");
                        _broker.Tell(payload);
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
            _seek = null;
            _consumerRecreator = Context.System.Scheduler.ScheduleTellRepeatedlyCancelable(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(15), Self, new RecreateConsumer(), ActorRefs.NoSender);
            Receive<RecreateConsumer>(_ =>
            {
                BecomeLookUp();
            });
            ReceiveAny(any => Stash.Stash());
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
                _pulsarManager.Tell(new CreatedConsumer(Self, _topicName.ToString()));
                BecomeActive();
                Stash.UnstashAll();
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
        public class RecreateConsumer
        {

        }
        public IStash Stash { get; set; }
    }
    public class ChunkedMessageCtx
    {

        internal int TotalChunks = -1;
        internal List<byte> ChunkedMsgBuffer;
        internal int LastChunkedMessageId = -1;
        internal MessageId[] ChunkedMessageIds;
        internal long ReceivedTime = 0;

        internal static ChunkedMessageCtx Get(int numChunksFromMsg, byte[] chunkedMsg)
        {
            var ctx = new ChunkedMessageCtx
            {
                TotalChunks = numChunksFromMsg,
                ChunkedMsgBuffer = new List<byte>(chunkedMsg),
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
