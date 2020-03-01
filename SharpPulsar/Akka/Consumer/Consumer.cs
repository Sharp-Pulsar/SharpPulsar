using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using Akka.Actor;
using DotNetty.Buffers;
using Microsoft.Extensions.Logging;
using Pulsar.Common.Auth;
using SharpPulsar.Akka.InternalCommands;
using SharpPulsar.Akka.InternalCommands.Consumer;
using SharpPulsar.Akka.Network;
using SharpPulsar.Api;
using SharpPulsar.Common.Compression;
using SharpPulsar.Common.Naming;
using SharpPulsar.Common.Schema;
using SharpPulsar.Extension;
using SharpPulsar.Impl;
using SharpPulsar.Impl.Conf;
using SharpPulsar.Protocol;
using SharpPulsar.Protocol.Proto;
using SharpPulsar.Shared;

namespace SharpPulsar.Akka.Consumer
{
    public class Consumer:ReceiveActor, IWithUnboundedStash
    {
        private const int MaxRedeliverUnacknowledged = 1000;
        private ClientConfigurationData _clientConfiguration;
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
        public string TopicNameWithoutPartition;
        private long _currentFlowPermitsCount;
        private int _requestedFlowPermits;
        private readonly IDictionary<MessageIdImpl, IList<MessageImpl>> _possibleSendToDeadLetterTopicMessages;

        private readonly DeadLetterPolicy _deadLetterPolicy;
        private readonly bool _createTopicIfDoesNotExist;

        public UnAckedMessageTracker UnAckedMessageTracker;
        private readonly IAcknowledgmentsGroupingTracker _acknowledgmentsGroupingTracker;
        private readonly NegativeAcksTracker _negativeAcksTracker;
        private readonly SubscriptionMode _subscriptionMode;
        private volatile BatchMessageIdImpl _startMessageId;
        private readonly BatchMessageIdImpl _initialStartMessageId;
        private readonly long _startMessageRollbackDurationInSec;
        private volatile bool _hasReachedEndOfTopic;
        private readonly MessageCrypto _msgCrypto;
        private bool _hasParentConsumer;
        private long _requestId;
        private readonly long _consumerid;
        private readonly Dictionary<long, Payload> _pendingLookupRequests = new Dictionary<long, Payload>();

        public Consumer(ClientConfigurationData clientConfiguration, string topic, ConsumerConfigurationData configuration, long consumerid, IActorRef network, bool hasParentConsumer)
        {
            _hasParentConsumer = hasParentConsumer;
            _requestedFlowPermits = configuration.ReceiverQueueSize;
            _conf = configuration;
            _clientConfiguration = clientConfiguration;
            _startMessageRollbackDurationInSec = 0;
            _consumerid = consumerid;
            _network = network;
            _topicName = TopicName.Get(topic);
            // Create msgCrypto if not created already
            _msgCrypto = configuration.CryptoKeyReader == null ? new MessageCrypto($"[{configuration.SingleTopic}] [{configuration.SubscriptionName}]", false) : null;
            Become(LookUpBroker);
        }

        protected override void PostStop()
        {
            var requestid = _requestId++;
            //var unsubscribe = Commands.NewUnsubscribe(_consumerid, requestid);
            var cmd = Commands.NewCloseConsumer(_consumerid, requestid);
            var payload = new Payload(cmd.Array, requestid, "CloseConsumer");
            _broker.Tell(payload);
        }

        private void RedeliverUnacknowledgedMessages(ISet<IMessageId> messageIds)
        {
            var requestid = _requestId++;
            if (_conf.SubscriptionType != CommandSubscribe.Types.SubType.Shared && _conf.SubscriptionType != CommandSubscribe.Types.SubType.KeyShared)
            {
                // We cannot redeliver single messages if subscription type is not Shared
                var cmd = Commands.NewRedeliverUnacknowledgedMessages(_consumerid);
                var payload = new Payload(cmd.Array, requestid, "RedeliverUnacknowledgedMessages");
                _broker.Tell(payload);
            }
            else
            {
                var i = 0;
                var batches = messageIds.PartitionMessageId(MaxRedeliverUnacknowledged);
                var builder = MessageIdData.NewBuilder();
                batches.ForEach(ids =>
                {
                    var messageIdDatas = ids.Where(messageId => !ProcessPossibleToDlq(messageId)).Select(messageId =>
                    {
                        builder.SetPartition(messageId.PartitionIndex);
                        builder.SetLedgerId(messageId.LedgerId);
                        builder.SetEntryId(messageId.EntryId);
                        return builder.Build();
                    }).ToList();
                    var cmd = Commands.NewRedeliverUnacknowledgedMessages(_consumerid, messageIdDatas);
                    var payload = new Payload(cmd.Array, requestid, "RedeliverUnacknowledgedMessages");
                    _broker.Tell(payload);
                });
            }
        }
        private bool ProcessPossibleToDlq(MessageIdImpl messageId)
        {
           return false;
        }

        public static Props Prop(ClientConfigurationData clientConfiguration, string topic, ConsumerConfigurationData configuration, long consumerid, IActorRef network, bool hasParentConsumer)
        {
            return Props.Create(()=> new Consumer(clientConfiguration, topic, configuration, consumerid, network, hasParentConsumer));
        }
        private void NegativeAcknowledge(IMessageId messageId)
        {

        }

        private bool HasReachedEndOfTopic()
        {
            return false;
        }
        
        private void Seek(IMessageId messageId)
        {

        }
        private void Seek(long timestamp)
        {

        }
        private void Init()
        {
            Receive<TcpSuccess>(s =>
            {
                _consumerEventListener.Log($"Pulsar handshake completed with {s.Name}");
                Become(SubscribeConsumer);
            });
            
            ReceiveAny(_ => Stash.Stash());
        }

        private void ReceiveMessages()
        {
            Receive<MessageReceived>(m =>
            {
                var msgId = new MessageIdData
                {
                    EntryId = (ulong) m.MessageId.EntryId,
                    LedgerId = (ulong) m.MessageId.LedgerId,
                    Partition = m.MessageId.Partition,
                    BatchIndex = m.MessageId.BatchIndex
                };
                var buffer = Unpooled.WrappedBuffer(m.Data);
                HandleMessage(msgId, m.RedeliveryCount, buffer);
            });
            Receive<AckMessage>(AckMessage);
            Receive<AckMessages>(AckMessages);
            Receive<AckMultiMessage>(AckMultiMessage);
            ReceiveAny(x=> _consumerEventListener.Log($"{x.GetType().Name} was unhandled!"));
            Stash.UnstashAll();
        }

        private void HandleMessage(MessageIdData messageId, int redeliveryCount, IByteBuffer headersAndPayload)
        {
            if (Context.System.Log.IsDebugEnabled)
            {
                Context.System.Log.Debug("[{}][{}] Received message: {}/{}", _topicName.ToString(), _subscriptionName, messageId.LedgerId, messageId.EntryId);
                _consumerEventListener.Log($"[{_topicName}][{_subscriptionName}] Received message: {messageId.LedgerId}/{messageId.EntryId}");
            }

            if (!VerifyChecksum(headersAndPayload, messageId))
            {
                // discard message with checksum error
                DiscardCorruptedMessage(messageId, CommandAck.Types.ValidationError.ChecksumMismatch);
                return;
            }

            MessageMetadata msgMetadata;
            try
            {
                msgMetadata = Commands.ParseMessageMetadata(headersAndPayload);
            }
            catch (Exception)
            {
                DiscardCorruptedMessage(messageId, CommandAck.Types.ValidationError.ChecksumMismatch);
                return;
            }
            var numMessages = msgMetadata.NumMessagesInBatch;
            var msgId = new MessageIdImpl((long)messageId.LedgerId, (long)messageId.EntryId, messageId.Partition);
            
            var decryptedPayload = DecryptPayloadIfNeeded(messageId, msgMetadata, headersAndPayload);

            var isMessageUndecryptable = IsMessageUndecryptable(msgMetadata);

            if (decryptedPayload == null)
            {
                // Message was discarded or CryptoKeyReader isn't implemented
                return;
            }

            // uncompress decryptedPayload and release decryptedPayload-ByteBuf
            var uncompressedPayload = isMessageUndecryptable ? (IByteBuffer)decryptedPayload.Retain() : UncompressPayloadIfNeeded(messageId, msgMetadata, decryptedPayload);
            decryptedPayload.Release();
            if (uncompressedPayload == null)
            {
                // Message was discarded on decompression error
                return;
            }

            // if message is not decryptable then it can't be parsed as a batch-message. so, add EncyrptionCtx to message
            // and return undecrypted payload
            if (isMessageUndecryptable || (numMessages == 1 && !msgMetadata.HasNumMessagesInBatch))
            {

                if (IsResetIncludedAndSameEntryLedger(messageId) && IsPriorEntryIndex((long)messageId.EntryId))
                {
                    // We need to discard entries that were prior to startMessageId
                    if (Context.System.Log.IsDebugEnabled)
                    {
                        Context.System.Log.Debug("[{}] [{}] Ignoring message from before the startMessageId: {}", _subscriptionName, _consumerName, _startMessageId);
                    }

                    uncompressedPayload.Release();
                    return;
                }

                var message = new MessageImpl(_topicName.ToString(), msgId, msgMetadata, uncompressedPayload, CreateEncryptionContext(msgMetadata), _schema, redeliveryCount);
                uncompressedPayload.Release();
                if (_hasParentConsumer)
                    Context.Parent.Tell(new ConsumedMessage(Self, message));
                else
                    _listener.Received(Self, message);
            }
            else
            {
                // handle batch message enqueuing; uncompressed payload has all messages in batch
                ReceiveIndividualMessagesFromBatch(msgMetadata, redeliveryCount, uncompressedPayload, messageId);

                uncompressedPayload.Release();
            }

        }
        private void ReceiveIndividualMessagesFromBatch(MessageMetadata msgMetadata, int redeliveryCount, IByteBuffer uncompressedPayload, MessageIdData messageId)
        {
            var batchSize = msgMetadata.NumMessagesInBatch;

            // create ack tracker for entry aka batch
            var batchMessage = new MessageIdImpl((long)messageId.LedgerId, (long)messageId.EntryId, messageId.Partition);
            var acker = BatchMessageAcker.NewAcker(batchSize);
            IList<MessageImpl> possibleToDeadLetter = null;
            if (_deadLetterPolicy != null && redeliveryCount >= _deadLetterPolicy.MaxRedeliverCount)
            {
                possibleToDeadLetter = new List<MessageImpl>();
            }
            var skippedMessages = 0;
            try
            {
                for (var i = 0; i < batchSize; ++i)
                {
                    if (Context.System.Log.IsDebugEnabled)
                    {
                        Context.System.Log.Debug("[{}] [{}] processing message num - {} in batch", _subscriptionName, _consumerName, i);
                    }
                    var singleMessageMetadataBuilder = SingleMessageMetadata.NewBuilder();
                    var singleMessagePayload = Commands.DeSerializeSingleMessageInBatch(uncompressedPayload, singleMessageMetadataBuilder, i, batchSize);

                    if (IsResetIncludedAndSameEntryLedger(messageId) && IsPriorBatchIndex(i))
                    {
                        // If we are receiving a batch message, we need to discard messages that were prior
                        // to the startMessageId
                        if (Context.System.Log.IsDebugEnabled)
                        {
                            Context.System.Log.Debug("[{}] [{}] Ignoring message from before the startMessageId: {}", _subscriptionName, _consumerName, _startMessageId);
                        }
                        singleMessagePayload.Release();

                        ++skippedMessages;
                        continue;
                    }

                    if (singleMessageMetadataBuilder.HasCompactedOut())
                    {
                        // message has been compacted out, so don't send to the user
                        singleMessagePayload.Release();

                        ++skippedMessages;
                        continue;
                    }

                    var batchMessageIdImpl = new BatchMessageIdImpl((long)messageId.LedgerId, (long)messageId.EntryId, messageId.Partition, i, acker);

                    var message = new MessageImpl(_topicName.ToString(), batchMessageIdImpl, msgMetadata, singleMessageMetadataBuilder.Build(), singleMessagePayload, CreateEncryptionContext(msgMetadata), _schema, redeliveryCount);
                    if(_hasParentConsumer) 
                        Context.Parent.Tell(new ConsumedMessage(Self, message));
                    else
                        _listener.Received(Self, message);
                    possibleToDeadLetter?.Add(message);
                    singleMessagePayload.Release();
                }
            }
            catch (IOException)
            {
                Context.System.Log.Warning("[{}] [{}] unable to obtain message in batch", _subscriptionName, _consumerName);
                DiscardCorruptedMessage(messageId, CommandAck.Types.ValidationError.BatchDeSerializeError);
            }

            if (possibleToDeadLetter != null && _possibleSendToDeadLetterTopicMessages != null)
            {
                _possibleSendToDeadLetterTopicMessages[batchMessage] = possibleToDeadLetter;
            }

            if (Context.System.Log.IsDebugEnabled)
            {
                //Context.System.Log.Debug("[{}] [{}] enqueued messages in batch. queue size - {}, available queue size - {}", _subscriptionName, _consumerName, IncomingMessages.size(), IncomingMessages.RemainingCapacity());
            }

            if (skippedMessages > 0)
            {
                SendFlow(skippedMessages);
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
                encryptionCtx = new EncryptionContext();
                IDictionary<string, EncryptionContext.EncryptionKey> keys = msgMetadata.EncryptionKeys.ToDictionary(e => e.Key, e => new EncryptionContext.EncryptionKey { KeyValue = (sbyte[])(object)e.Value.ToByteArray(), Metadata = e.Metadata?.ToDictionary(k => k.Key, k => k.Value) });
                var encParam = new sbyte[MessageCrypto.IvLen];
                msgMetadata.EncryptionParam.CopyTo((byte[])(object)encParam, 0);
                int? batchSize = msgMetadata.HasNumMessagesInBatch ? msgMetadata.NumMessagesInBatch : 0;
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
            return !_conf.ResetIncludeHead && _startMessageId != null && (long)messageId.LedgerId == _startMessageId.LedgerId && (long)messageId.EntryId == _startMessageId.EntryId;
        }
        private bool IsMessageUndecryptable(MessageMetadata msgMetadata)
        {
            return (msgMetadata.EncryptionKeys.Count > 0 && _conf.CryptoKeyReader == null && _conf.CryptoFailureAction == ConsumerCryptoFailureAction.Consume);
        }
        private IByteBuffer DecryptPayloadIfNeeded(MessageIdData messageId, MessageMetadata msgMetadata, IByteBuffer payload)
        {

            if (msgMetadata.EncryptionKeys.Count == 0)
            {
                payload.Retain();
                return payload;
            }

            // If KeyReader is not configured throw exception based on config param
            if (_conf.CryptoKeyReader == null)
            {
                switch (_conf.CryptoFailureAction)
                {
                    case ConsumerCryptoFailureAction.Consume:
                        Context.System.Log.Warning("[{}][{}][{}] CryptoKeyReader interface is not implemented. Consuming encrypted message.", _topicName.ToString(), _subscriptionName, _consumerName);
                        payload.Retain();
                        return payload;
                    case ConsumerCryptoFailureAction.Discard:
                        Context.System.Log.Warning("[{}][{}][{}] Skipping decryption since CryptoKeyReader interface is not implemented and config is set to discard", _topicName.ToString(), _subscriptionName, _consumerName);
                        DiscardMessage(messageId, CommandAck.Types.ValidationError.DecryptionError);
                        return null;
                    case ConsumerCryptoFailureAction.Fail:
                        IMessageId m = new MessageIdImpl((long)messageId.LedgerId, (long)messageId.EntryId, messageId.Partition);
                        Context.System.Log.Error("[{}][{}][{}][{}] Message delivery failed since CryptoKeyReader interface is not implemented to consume encrypted message", _topicName.ToString(), _subscriptionName, _consumerName, m);
                        UnAckedMessageTracker.Add(m);
                        return null;
                }
            }

            var decryptedData = _msgCrypto.Decrypt(msgMetadata, payload, _conf.CryptoKeyReader);
            if (decryptedData != null)
            {
                return decryptedData;
            }

            switch (_conf.CryptoFailureAction)
            {
                case ConsumerCryptoFailureAction.Consume:
                    // Note, batch message will fail to consume even if config is set to consume
                    Context.System.Log.Warning("[{}][{}][{}][{}] Decryption failed. Consuming encrypted message since config is set to consume.", _topicName.ToString(), _subscriptionName, _consumerName, messageId);
                    payload.Retain();
                    return payload;
                case ConsumerCryptoFailureAction.Discard:
                    Context.System.Log.Warning("[{}][{}][{}][{}] Discarding message since decryption failed and config is set to discard", _topicName.ToString(), _subscriptionName, _consumerName, messageId);
                    DiscardMessage(messageId, CommandAck.Types.ValidationError.DecryptionError);
                    return null;
                case ConsumerCryptoFailureAction.Fail:
                    var m = new MessageIdImpl((long)messageId.LedgerId, (long)messageId.EntryId, messageId.Partition);
                    Context.System.Log.Error("[{}][{}][{}][{}] Message delivery failed since unable to decrypt incoming message", _topicName.ToString(), _subscriptionName, _consumerName, m);
                    UnAckedMessageTracker.Add(m);
                    return null;
            }
            return null;
        }

        private void AckMessage(AckMessage message)
        {
            var requestid = _requestId++;
            var cmd = Commands.NewAck(_consumerid, message.MessageId.LedgerId, message.MessageId.EntryId, CommandAck.Types.AckType.Individual, null, new Dictionary<string, long>());
            var payload = new Payload(cmd.Array, requestid, "AckMessages");
            _broker.Tell(payload);
        }
        private void AckMultiMessage(AckMultiMessage multiMessage)
        {
            var requestid = _requestId++;
            IList<KeyValuePair<long, long>> entriesToAck = new List<KeyValuePair<long, long>>(multiMessage.MessageIds.Count);
            foreach (var m in multiMessage.MessageIds)
            {
                entriesToAck.Add(new KeyValuePair<long, long>(m.LedgerId, m.EntryId));
            }

            var cmd = Commands.NewMultiMessageAck(_consumerid, entriesToAck);
            var payload = new Payload(cmd.Array, requestid, "AckMultiMessages");
            _broker.Tell(payload);
        }

        private void AckMessages(AckMessages message)
        {
            var requestid = _requestId++;
            var cmd = Commands.NewAck(_consumerid, message.MessageId.LedgerId, message.MessageId.EntryId, CommandAck.Types.AckType.Cumulative, null, new Dictionary<string, long>());
            var payload = new Payload(cmd.Array, requestid, "AckMessages");
            _broker.Tell(payload);
        }
        private IByteBuffer UncompressPayloadIfNeeded(MessageIdData messageId, MessageMetadata msgMetadata, IByteBuffer payload)
        {
            var compressionType = msgMetadata.Compression;
            var codec = CompressionCodecProvider.GetCompressionCodec((int)compressionType);
            var uncompressedSize = (int)msgMetadata.UncompressedSize;
            var payloadSize = payload.ReadableBytes;
            if (payloadSize > Commands.DefaultMaxMessageSize)
            {
                // payload size is itself corrupted since it cannot be bigger than the MaxMessageSize
                Context.System.Log.Error("[{}][{}] Got corrupted payload message size {} at {}", _topicName.ToString(), _subscriptionName, payloadSize, messageId);
                DiscardCorruptedMessage(messageId, CommandAck.Types.ValidationError.UncompressedSizeCorruption);
                return null;
            }

            try
            {
                var uncompressedPayload = codec.Decode(payload, uncompressedSize);
                return uncompressedPayload;
            }
            catch (IOException e)
            {
                Context.System.Log.Error("[{}][{}] Failed to decompress message with {} at {}: {}", _topicName.ToString(), _subscriptionName, compressionType, messageId, e.Message, e);
                DiscardCorruptedMessage(messageId, CommandAck.Types.ValidationError.DecompressionError);
                return null;
            }
        }

        private void DiscardCorruptedMessage(MessageIdData messageId, CommandAck.Types.ValidationError validationError)
        {
            Context.System.Log.Error("[{}][{}] Discarding corrupted message at {}:{}", _topicName.ToString(), _subscriptionName, messageId.LedgerId, messageId.EntryId);
            DiscardMessage(messageId, validationError);
        }

        private void DiscardMessage(MessageIdData messageId, CommandAck.Types.ValidationError validationError)
        {
            var cmd = Commands.NewAck(_consumerid, (long)messageId.LedgerId, (long)messageId.EntryId, CommandAck.Types.AckType.Individual, validationError, new Dictionary<string, long>());
            var payload = new Payload(cmd.Array, _requestId++, "NewAck");
            _broker.Tell(payload);
        }
        private bool VerifyChecksum(IByteBuffer headersAndPayload, MessageIdData messageId)
        {

            if (Commands.HasChecksum(headersAndPayload))
            {
                var checksum = Commands.ReadChecksum(headersAndPayload);
                int computedChecksum = Commands.ComputeChecksum(headersAndPayload);
                if (checksum != computedChecksum)
                {
                    Context.System.Log.Error("[{}][{}] Checksum mismatch for message at {}:{}. Received checksum: 0x{}, Computed checksum: 0x{}", _topicName.ToString(), _subscriptionName, messageId.LedgerId, messageId.EntryId, checksum.ToString("x"), computedChecksum.ToString("x"));
                    return false;
                }
            }

            return true;
        }
        private void SubscribeConsumer()
        {
            Receive<SubscribeSuccess>(s =>
            {
                if (s.HasSchema)
                {
                    var schemaInfo = new SchemaInfo
                    {
                        Name = s.Schema.Name,
                        Properties = s.Schema.Properties.ToDictionary(x => x.Key, x => x.Value),
                        Type = s.Schema.Type == Schema.Types.Type.Json ? SchemaType.Json : SchemaType.None,
                        Schema = (sbyte[]) (object) s.Schema.SchemaData.ToByteArray()
                    };
                    _schema = ISchema.GetSchema(schemaInfo);
                }
                SendFlow(_requestedFlowPermits);
                Become(ReceiveMessages);
                _conf.ConsumerEventListener.ConsumerCreated(new CreatedConsumer(Self, _topicName.ToString()));
            });
            ReceiveAny(x=> Stash.Stash());
            var requestid = _requestId++;
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
                var builder = MessageIdData.NewBuilder();
                builder.SetLedgerId(_startMessageId.LedgerId);
                builder.SetEntryId(_startMessageId.EntryId);
                if (_startMessageId is BatchMessageIdImpl impl)
                {
                    builder.SetBatchIndex(impl.BatchIndex);
                }

                startMessageIdData = builder.Build();
            }
            var si = (SchemaInfo)_schema.SchemaInfo;
            if (si != null && (SchemaType.Bytes == si.Type || SchemaType.None == si.Type))
            {
                // don't set schema for Schema.BYTES
                si = null;
            }
            // startMessageRollbackDurationInSec should be consider only once when consumer connects to first time
            var startMessageRollbackDuration = ( _startMessageRollbackDurationInSec > 0 && _startMessageId.Equals(_initialStartMessageId)) ? _startMessageRollbackDurationInSec : 0;
            var request = Commands.NewSubscribe(_topicName.ToString(), _subscriptionName, _consumerid, requestid, _conf.SubscriptionType, _conf.PriorityLevel, _consumerName, isDurable, startMessageIdData, _conf.Properties, _conf.ReadCompacted, _conf.ReplicateSubscriptionState, CommandSubscribe.ValueOf(_conf.SubscriptionInitialPosition.Value), startMessageRollbackDuration, si, _createTopicIfDoesNotExist, _conf.KeySharedPolicy);
            var payload = new Payload(request.Array, requestid, "NewSubscribe");
            _broker.Tell(payload);
        }

        private void SendFlow(int numbs)
        {
            var requestid = _requestId++;
            var reqt = Commands.NewFlow(_consumerid, numbs);
            var payload = new Payload(reqt.Array, requestid, "NewFlow");
            _broker.Tell(payload);
        }
        private void LookUpBroker()
        {
            Receive<BrokerLookUp>(l =>
            {
                _pendingLookupRequests.Remove(l.RequestId);
                var uri = _conf.UseTls ? new Uri(l.BrokerServiceUrlTls) : new Uri(l.BrokerServiceUrl);

                var address = new IPEndPoint(Dns.GetHostAddresses(uri.Host)[0], uri.Port);
                _broker = Context.ActorOf(ClientConnection.Prop(address, _clientConfiguration, Sender));
                Become(Init);
            });
            
            ReceiveAny(_ => Stash.Stash());
            SendBrokerLookUpCommand();
        }

        private void SendBrokerLookUpCommand()
        {
            var requestid = _requestId++;
            var request = Commands.NewLookup(_topicName.ToString(), false, requestid);
            var load = new Payload(request.Array, requestid, "BrokerLookUp");
            _network.Tell(load);
            _pendingLookupRequests.Add(requestid, load);
        }
        public IStash Stash { get; set; }
    }
}
