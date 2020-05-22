using System;
using System.Buffers;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading;
using Akka.Actor;
using SharpPulsar.Akka.InternalCommands;
using SharpPulsar.Akka.InternalCommands.Consumer;
using SharpPulsar.Akka.Network;
using SharpPulsar.Api;
using SharpPulsar.Api.Schema;
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

namespace SharpPulsar.Akka.Consumer
{
    public class Consumer:ReceiveActor, IWithUnboundedStash
    {
        private int _partitionIndex;
        private const int MaxRedeliverUnacknowledged = 1000;
        private readonly ClientConfigurationData _clientConfiguration;
        private IActorRef _broker;
        private ConsumerConfigurationData _conf;
        private string _consumerName;
        private string _subscriptionName;
        private ISchema _schema;
        private List<IConsumerInterceptor> _interceptors;
        private IMessageListener _listener;
        private IConsumerEventListener _consumerEventListener;
        private TopicName _topicName;
        private IActorRef _network;
        private int _requestedFlowPermits;
        private readonly IDictionary<MessageId, IList<Message>> _possibleSendToDeadLetterTopicMessages;
        private Seek _seek;
        private readonly DeadLetterPolicy _deadLetterPolicy;
        private readonly bool _createTopicIfDoesNotExist;
        private readonly SubscriptionMode _subscriptionMode;
        private volatile BatchMessageId _startMessageId;
        private readonly BatchMessageId _initialStartMessageId;
        private  ConnectedServerInfo _serverInfo;
        private readonly long _startMessageRollbackDurationInSec;
        private readonly MessageCrypto _msgCrypto;
        private bool _hasParentConsumer;
        private readonly long _consumerid;
        private ICancelable _consumerRecreator;
        private readonly Dictionary<BytesSchemaVersion, ISchemaInfo> _schemaCache = new Dictionary<BytesSchemaVersion, ISchemaInfo>();
        private IActorRef _pulsarManager;
        public Consumer(ClientConfigurationData clientConfiguration, string topic, ConsumerConfigurationData configuration, long consumerid, IActorRef network, bool hasParentConsumer, int partitionIndex, SubscriptionMode mode, Seek seek, IActorRef pulsarManager)
        {
            _pulsarManager = pulsarManager;
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
                var batches = messageIds.PartitionMessageId(MaxRedeliverUnacknowledged);
                var builder = new MessageIdData();
                batches.ForEach(ids =>
                {
                    var messageIdDatas = ids.Where(messageId => !ProcessPossibleToDlq(messageId.LedgerId, messageId.EntryId, messageId.PartitionIndex, -1)).Select(messageId =>
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

        public static Props Prop(ClientConfigurationData clientConfiguration, string topic, ConsumerConfigurationData configuration, long consumerid, IActorRef network, bool hasParentConsumer, int partitionIndex, SubscriptionMode mode, Seek seek, IActorRef pulsarManager)
        {
            return Props.Create(()=> new Consumer(clientConfiguration, topic, configuration, consumerid, network, hasParentConsumer, partitionIndex, mode, seek, pulsarManager));
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
            var msgId = new MessageId((long)messageId.ledgerId, (long)messageId.entryId, _partitionIndex);
            
            var decryptedPayload = DecryptPayloadIfNeeded(messageId, msgMetadata, payload);

            var isMessageUndecryptable = IsMessageUndecryptable(msgMetadata);

            if (decryptedPayload == null)
            {
                // Message was discarded or CryptoKeyReader isn't implemented
                return;
            }

            // uncompress decryptedPayload and release decryptedPayload-ByteBuf
            var uncompressedPayload = isMessageUndecryptable ? decryptedPayload : UncompressPayloadIfNeeded(messageId, msgMetadata, decryptedPayload);
            
            if (uncompressedPayload == null)
            {
                // Message was discarded on decompression error
                return;
            }

            // if message is not decryptable then it can't be parsed as a batch-message. so, add EncyrptionCtx to message
            // and return undecrypted payload
            if (isMessageUndecryptable || (numMessages == 1 && msgMetadata.NumMessagesInBatch > 0))
            {

                if (IsResetIncludedAndSameEntryLedger(messageId) && IsPriorEntryIndex((long)messageId.entryId))
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
                    _listener.Received(Self, message);
            }
            else
            {
                // handle batch message enqueuing; uncompressed payload has all messages in batch
                ReceiveIndividualMessagesFromBatch(msgMetadata, redeliveryCount, uncompressedPayload, messageId);

            }

        }
        private void ReceiveIndividualMessagesFromBatch(MessageMetadata msgMetadata, int redeliveryCount, byte[] uncompressedPayload, MessageIdData messageId)
        {
            var batchSize = msgMetadata.NumMessagesInBatch;
            var data = new ReadOnlySequence<byte>(uncompressedPayload);
            // create ack tracker for entry aka batch
            var batchMessage = new MessageId((long)messageId.ledgerId, (long)messageId.entryId, _partitionIndex);
            var acker = BatchMessageAcker.NewAcker(batchSize);
            IList<Message> possibleToDeadLetter = null;
            if (_deadLetterPolicy != null && redeliveryCount >= _deadLetterPolicy.MaxRedeliverCount)
            {
                possibleToDeadLetter = new List<Message>();
            }
            try
            {
                long index = 0;
                for (var i = 0; i < batchSize; ++i)
                {
                    if (Context.System.Log.IsDebugEnabled)
                    {
                        Context.System.Log.Debug($"[{_subscriptionName}] [{_consumerName}] processing message num - {i} in batch");
                    }
                    var singleMetadataSize = data.ReadUInt32(index, true);
                    index += 4;
                    var singleMetadata = Serializer.Deserialize<SingleMessageMetadata>(data.Slice(index, singleMetadataSize));
                    index += singleMetadataSize;

                    var singleMessagePayload = data.Slice(index, singleMetadata.PayloadSize);

                    if (IsResetIncludedAndSameEntryLedger(messageId) && IsPriorBatchIndex(i))
                    {
                        // If we are receiving a batch message, we need to discard messages that were prior
                        // to the startMessageId
                        if (Context.System.Log.IsDebugEnabled)
                        {
                            Context.System.Log.Debug($"[{_subscriptionName}] [{_consumerName}] Ignoring message from before the startMessageId: {_startMessageId}");
                        }
                        continue;
                    }

                    if (singleMetadata.CompactedOut)
                    {
                        continue;
                    }

                    var batchMessageIdImpl = new BatchMessageId((long)messageId.ledgerId, (long)messageId.entryId, _partitionIndex, i, acker);

                    var message = new Message(_topicName.ToString(), batchMessageIdImpl, msgMetadata, singleMetadata, singleMessagePayload.ToArray(), CreateEncryptionContext(msgMetadata), _schema, redeliveryCount);
                    if(_hasParentConsumer) 
                        Context.Parent.Tell(new ConsumedMessage(Self, message));
                    else
                        _listener.Received(Self, message);

                    possibleToDeadLetter?.Add(message);
                    index += (uint)singleMetadata.PayloadSize;
                }
            }
            catch (IOException)
            {
                Context.System.Log.Warning($"[{_subscriptionName}] [{_consumerName}] unable to obtain message in batch");
                DiscardCorruptedMessage(messageId, CommandAck.ValidationError.BatchDeSerializeError);
            }

            if (possibleToDeadLetter != null && _possibleSendToDeadLetterTopicMessages != null)
            {
                _possibleSendToDeadLetterTopicMessages[batchMessage] = possibleToDeadLetter;
            }

            if (Context.System.Log.IsDebugEnabled)
            {
                //Context.System.Log.Debug("[{}] [{}] enqueued messages in batch. queue size - {}, available queue size - {}", _subscriptionName, _consumerName, IncomingMessages.size(), IncomingMessages.RemainingCapacity());
            }

        }
        private bool IsPriorBatchIndex(long idx)
        {
            return _conf.ResetIncludeHead ? idx < _startMessageId.BatchIndex : idx <= _startMessageId.BatchIndex;
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
        private bool IsResetIncludedAndSameEntryLedger(MessageIdData messageId)
        {
            return !_conf.ResetIncludeHead && _startMessageId != null && (long)messageId.ledgerId == _startMessageId.LedgerId && (long)messageId.entryId == _startMessageId.EntryId;
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
                        IMessageId m = new MessageId((long)messageId.ledgerId, (long)messageId.entryId, _partitionIndex);
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
                    var m = new MessageId((long)messageId.ledgerId, (long)messageId.entryId, _partitionIndex);
                    Context.System.Log.Error($"[{_topicName}][{_subscriptionName}][{_consumerName}][{m}] Message delivery failed since unable to decrypt incoming message");
                    //UnAckedMessageTracker.Add(m);
                    return null;
            }
            return null;
        }
        private void AckMessage(AckMessage message)
        {
            var requestid = Interlocked.Increment(ref IdGenerators.RequestId);
            var cmd = Commands.NewAck(_consumerid, message.MessageId.LedgerId, message.MessageId.EntryId, CommandAck.AckType.Individual, null, new Dictionary<string, long>());
            var payload = new Payload(cmd, requestid, "AckMessages");
            _broker.Tell(payload);
        }
        private void AckMultiMessage(AckMultiMessage multiMessage)
        {
            var requestid = Interlocked.Increment(ref IdGenerators.RequestId);
            IList<KeyValuePair<long, long>> entriesToAck = new List<KeyValuePair<long, long>>(multiMessage.MessageIds.Count);
            foreach (var m in multiMessage.MessageIds)
            {
                entriesToAck.Add(new KeyValuePair<long, long>(m.LedgerId, m.EntryId));
            }

            var cmd = Commands.NewMultiMessageAck(_consumerid, entriesToAck);
            var payload = new Payload(cmd, requestid, "AckMultiMessages");
            _broker.Tell(payload);
        }
        private void AckMessages(AckMessages message)
        {
            var requestid = Interlocked.Increment(ref IdGenerators.RequestId);
            var cmd = Commands.NewAck(_consumerid, message.MessageId.LedgerId, message.MessageId.EntryId, CommandAck.AckType.Cumulative, null, new Dictionary<string, long>());
            var payload = new Payload(cmd, requestid, "AckMessages");
            _broker.Tell(payload);
        }
        private byte[] UncompressPayloadIfNeeded(MessageIdData messageId, MessageMetadata msgMetadata, byte[] payload)
        {
            var compressionType = msgMetadata.Compression;
            var codec = CompressionCodecProvider.GetCompressionCodec((int)compressionType);
            var uncompressedSize = (int)msgMetadata.UncompressedSize;
            var payloadSize = payload.Length;
            if (payloadSize > _serverInfo.MaxMessageSize)
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
            var cmd = Commands.NewAck(_consumerid, (long)messageId.ledgerId, (long)messageId.entryId, CommandAck.AckType.Individual, validationError, new Dictionary<string, long>());
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

            var intial = Enum.GetValues(typeof(CommandSubscribe.InitialPosition))
                .Cast<CommandSubscribe.InitialPosition>().ToList()[_conf.SubscriptionInitialPosition.Value];
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
                _consumerEventListener.LastMessageId(new LastMessageIdReceived(_consumerid, _topicName.ToString(), x));
            });
            Receive<MessageReceived>(m =>
            {
                _requestedFlowPermits--;
                var msgId = new MessageIdData
                {
                    entryId = (ulong)m.MessageId.EntryId,
                    ledgerId = (ulong)m.MessageId.LedgerId,
                    Partition = m.MessageId.Partition,
                    BatchIndex = m.MessageId.BatchIndex
                };
                HandleMessage(msgId, m.RedeliveryCount, m.Data);
                if (_requestedFlowPermits == 0)
                    SendFlow(_conf.ReceiverQueueSize);
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
                _consumerEventListener.Log($"Connected to Pulsar Server[{s.Version}]. Subscribing");
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
                _conf.ConsumerEventListener.ConsumerCreated(new CreatedConsumer(Self, _topicName.ToString()));
                BecomeActive();
                Stash.UnstashAll();
            });
            ReceiveAny(_=> Stash.Stash());
        }
        private void SendBrokerLookUpCommand()
        {
            var requestid = Interlocked.Increment(ref IdGenerators.RequestId);
            var request = Commands.NewLookup(_topicName.ToString(), false, requestid);
            var load = new Payload(request, requestid, "BrokerLookUp");
            _network.Tell(load);
        }
        public class RecreateConsumer
        {

        }
        public IStash Stash { get; set; }
    }
}
