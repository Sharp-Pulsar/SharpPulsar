using Akka.Actor;
using DotNetty.Buffers;
using DotNetty.Common;
using Google.Protobuf;
using Org.BouncyCastle.Crypto;
using SharpPulsar.Akka.Handlers;
using SharpPulsar.Akka.InternalCommands;
using SharpPulsar.Akka.InternalCommands.Producer;
using SharpPulsar.Akka.Network;
using SharpPulsar.Api;
using SharpPulsar.Api.Interceptor;
using SharpPulsar.Common.Compression;
using SharpPulsar.Common.Naming;
using SharpPulsar.Common.Schema;
using SharpPulsar.Exceptions;
using SharpPulsar.Impl;
using SharpPulsar.Impl.Conf;
using SharpPulsar.Impl.Schema;
using SharpPulsar.Protocol;
using SharpPulsar.Protocol.Proto;
using SharpPulsar.Protocol.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using SharpPulsar.Akka.Configuration;
using IMessage = SharpPulsar.Api.IMessage;

namespace SharpPulsar.Akka.Producer
{
    public class Producer : ReceiveActor, IWithUnboundedStash
    {
        private IActorRef _broker;
        private IActorRef _network;
        private ProducerConfigurationData _configuration;
        private IProducerEventListener _listener;
        private long _producerId;
        public string ProducerName;
        private readonly bool _userProvidedProducerName = false;

        private string _connectionId;
        private string _connectedSince;
        private long _lastSequenceIdPushed;
        private long _lastSequenceId;
        private readonly CompressionCodec _compressor;
        private readonly MessageCrypto _msgCrypto = null;

        private readonly IDictionary<string, string> _metadata;
        private sbyte[] _schemaVersion;
        private long _sequenceId;
        private long _requestId;
        private bool _multiSchemaMode;
        private BatchMessageKeyBasedContainer _batchMessageContainer;
        private Dictionary<string, ISchema> _schemas;
        private ClientConfigurationData _clientConfiguration;
        private readonly List<IProducerInterceptor> _producerInterceptor;
        private readonly Dictionary<long, Payload> _pendingLookupRequests = new Dictionary<long, Payload>();
        private readonly object MultiSchemaMode;
        private Dictionary<SchemaHash, byte[]> _schemaCache = new Dictionary<SchemaHash, byte[]>();
        private bool _isPartitioned;
        private int _partitionIndex;
        public Producer(ClientConfigurationData clientConfiguration, ProducerConfigurationData configuration, long producerid, IActorRef network, bool isPartitioned = false)
        {
            _listener = configuration.ProducerEventListener;
            _schemas = new Dictionary<string, ISchema>();
            _isPartitioned = isPartitioned;
            _clientConfiguration = clientConfiguration;
            _producerInterceptor = configuration.Interceptors;
            _schemas.Add("default", configuration.Schema);
            _configuration = configuration;
            _producerId = producerid;
            _network = network;
            _producerId = producerid;
            ProducerName = configuration.ProducerName;
            if (!configuration.MultiSchema)
            {
                _multiSchemaMode = false;
            }
            if (!string.IsNullOrWhiteSpace(ProducerName) || isPartitioned)
            {
                _userProvidedProducerName = true;
            }
            _partitionIndex = configuration.Partitions;

            _compressor = CompressionCodecProvider.GetCompressionCodec(configuration.CompressionType);
            if (configuration.InitialSequenceId != null)
            {
                var initialSequenceId = (long)configuration.InitialSequenceId;
                _sequenceId = initialSequenceId;
            }
            else
            {
                _sequenceId = -1L;
            }

            if (configuration.EncryptionEnabled)
            {
                var logCtx = "[" + configuration.TopicName + "] [" + ProducerName + "] [" + _producerId + "]";
                _msgCrypto = new MessageCrypto(logCtx, true);

                // Regenerate data key cipher at fixed interval
                Context.System.Scheduler.ScheduleTellRepeatedly(TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(30), Self, new AddPublicKeyCipher(), ActorRefs.NoSender);

            }
            if (configuration.BatchingEnabled)
            {
                //var containerBuilder = configuration.BatcherBuilder ?? DefaultImplementation.NewDefaultBatcherBuilder();
                _batchMessageContainer = new BatchMessageKeyBasedContainer();
            }
            else
            {
                _batchMessageContainer = null;
            }
            if (!configuration.Properties.Any())
            {
                _metadata = new Dictionary<string, string>();
            }
            else
            {
                _metadata = new SortedDictionary<string, string>(configuration.Properties);
            }
            Become(LookUpBroker);
        }

        public static Props Prop(ClientConfigurationData clientConfiguration, ProducerConfigurationData configuration, long producerid, IActorRef network, bool isPartitioned = false)
        {
            return Props.Create(() => new Producer(clientConfiguration, configuration, producerid, network, isPartitioned));
        }

        private void Init()
        {
            Receive<TcpSuccess>(s =>
            {
                _listener.Log($"Pulsar handshake completed with {s.Name}");
                Become(CreateProducer);
            });
            Receive<AddPublicKeyCipher>(a =>
            {
                AddKey();
            });
            ReceiveAny(_ => Stash.Stash());
        }

        public void Receive()
        {
            Receive<AddPublicKeyCipher>(a =>
            {
                AddKey();
            });
            Receive<TcpClosed>(_ =>
            {
                Become(LookUpBroker);
            });
            Receive<Send>(ProcessSend);
            Receive<BulkSend>(s =>
            {
                foreach (var m in s.Messages)
                {
                    ProcessSend(m);
                }
            });
            Stash.UnstashAll();
        }

        private void ProcessSend(Send s)
        {
            try
            {
                var schemaName = s.Schema?.Name ?? "default";
                ISchema schema = null;
                if (_schemas.ContainsKey(schemaName))
                    schema = _schemas[schemaName];
                else
                {
                    schema = s.Schema?.Schema;
                    if (schema != null)
                    {
                        _schemas.Add(schemaName, s.Schema?.Schema);
                    }
                    else
                    {
                        schema = _schemas["default"];
                    }
                }
                var builder = new TypedMessageBuilder(ProducerName, schema);
                builder.Value(s.Message);
                builder.LoadConf(s.Config);
                foreach (var c in s.Config)
                {
                    switch (c.Key.ToLower())
                    {
                        case "keybytes":
                            builder.KeyBytes((sbyte[])c.Value);
                            break;
                        case "orderingkey":
                            builder.OrderingKey((sbyte[])c.Value);
                            break;
                        case "property":
                            var p = ((IDictionary<string, string>)c.Value).First();
                            builder.Property(p.Key, p.Value);
                            break;
                    }
                }

                var message = builder.Message;
                Become(() => InternalSend((Message)message));
            }
            catch (Exception e)
            {
                _listener.Log(e);
            }
        }
        private void CreateProducer()
        {
            Receive<ProducerCreated>(p =>
            {
                _pendingLookupRequests.Remove(p.RequestId);
                if (string.IsNullOrWhiteSpace(ProducerName))
                    ProducerName = p.Name;
                _sequenceId = p.LastSequenceId;
                var schemaVersion = p.SchemaVersion;
                if (schemaVersion != null)
                {
                    _schemaCache.TryAdd(SchemaHash.Of(_configuration.Schema), schemaVersion);
                }

                Become(Receive);
                if (_isPartitioned)
                {
                    Context.Parent.Tell(new RegisteredProducer(_producerId, ProducerName, _configuration.TopicName));
                }
                else
                {
                    _configuration.ProducerEventListener.ProducerCreated(new CreatedProducer(Self, _configuration.TopicName));
                }
            });
            Receive<AddPublicKeyCipher>(a =>
            {
                AddKey();
            });
            ReceiveAny(x => Stash.Stash());
            if (_isPartitioned)
            {
                var index = int.Parse(Self.Path.Name);
                _producerId = index;
                ProducerName = TopicName.Get(_configuration.TopicName).GetPartition(index).ToString();
            }
            SendNewProducerCommand();
        }
        private void SendNewProducerCommand()
        {
            var requestid = _requestId++;
            var schemaInfo = (SchemaInfo)_configuration.Schema.SchemaInfo;
            var request = Commands.NewProducer(_configuration.TopicName, _producerId, requestid, ProducerName, _configuration.EncryptionEnabled, _metadata, schemaInfo, DateTime.Now.Millisecond, _userProvidedProducerName);
            var payload = new Payload(request, requestid, "CommandProducer");
            _pendingLookupRequests.Add(requestid, payload);
            _network.Tell(payload);
        }
        private void LookUpBroker()
        {
            Receive<BrokerLookUp>(l =>
            {
                _pendingLookupRequests.Remove(l.RequestId);
                var uri = _configuration.UseTls ? new Uri(l.BrokerServiceUrlTls) : new Uri(l.BrokerServiceUrl);

                var address = new IPEndPoint(Dns.GetHostAddresses(uri.Host)[0], uri.Port);
                _broker = Context.ActorOf(ClientConnection.Prop(address, _clientConfiguration, Sender));
                Become(Init);
            });
            Receive<AddPublicKeyCipher>(a =>
            {
                AddKey();
            });
            ReceiveAny(_ => Stash.Stash());
            SendBrokerLookUpCommand();
        }

        private void SendBrokerLookUpCommand()
        {
            var requestid = _requestId++;
            var request = Commands.NewLookup(_configuration.TopicName, false, requestid);
            var load = new Payload(request, requestid, "BrokerLookUp");
            _network.Tell(load);
            _pendingLookupRequests.Add(requestid, load);
        }
        private void AddKey()
        {
            try
            {
                _msgCrypto.AddPublicKeyCipher(_configuration.EncryptionKeys, _configuration.CryptoKeyReader);
            }
            catch (CryptoException e)
            {
                Context.System.Log.Error(e.ToString());
            }
        }
        public class AddPublicKeyCipher
        {

        }
        private bool IsMultiSchemaEnabled(bool autoEnable)
        {
            
            return true;
        }
        private IMessage BeforeSend(IMessage message)
        {
            if (_producerInterceptor != null && _producerInterceptor.Count > 0)
            {
                var interceptedMessage = message;
                foreach (var p in _producerInterceptor)
                {
                    interceptedMessage = p.BeforeSend(Self, interceptedMessage);
                }
                return interceptedMessage;
            }
            return message;
        }
        public void OnSendAcknowledgement(IMessage message, IMessageId msgId, Exception exception)
        {
            if (_producerInterceptor != null)
            {
                foreach (var p in _producerInterceptor)
                {
                    p.OnSendAcknowledgement(Self, message, msgId, exception);
                }
            }
        }

        private void TryRegisterSchema(Message msg)
        {
            var m = msg;
            Receive<GetOrCreateSchemaServerResponse>(r =>
            {
                var schemaHash = SchemaHash.Of(msg.Schema);
                if (!_schemaCache.ContainsKey(schemaHash))
                {
                    _schemaCache[schemaHash] = r.SchemaVersion;
                }
                m.MessageBuilder.SetSchemaVersion(r.SchemaVersion);
                m.SetSchemaState(Message.SchemaState.Ready);
                Become(() => SendMessage(m));
            });
            ReceiveAny(_ => Stash.Stash());
            SchemaInfo schemaInfo = null;
            if (msg.Schema != null && msg.Schema.SchemaInfo.Type.Value > 0)
            {
                schemaInfo = (SchemaInfo)msg.Schema.SchemaInfo;
            }
            else
            {
                schemaInfo = (SchemaInfo)SchemaFields.Bytes.SchemaInfo;

            }

            SendGetOrCreateSchemaCommand(schemaInfo);
        }
        private void SendGetOrCreateSchemaCommand(SchemaInfo schemaInfo)
        {
            var requestId = _requestId++;
            var request = Commands.NewGetOrCreateSchema(requestId, _configuration.TopicName, schemaInfo);
            Context.System.Log.Info("[{}] [{}] GetOrCreateSchema request", _configuration.TopicName, ProducerName);
            var payload = new Payload(request, requestId, "GetOrCreateSchema");
            _broker.Tell(payload);
            _pendingLookupRequests.Add(requestId, payload);
        }

        public void InternalSend(Message message)
        {
            ReceiveAny(x => Stash.Stash());
            var interceptorMessage = (Message)BeforeSend(message);
            interceptorMessage.DataBuffer.Retain();
            if (_producerInterceptor != null)
            {
                _listener.Log(interceptorMessage.Properties);
            }

            SendMessage(interceptorMessage);
        }
        private void SendMessage(Message msg)
        {
            var msgMetadataBuilder = msg.MessageBuilder;
            var payload = msg.DataBuffer.Array;
            // If compression is enabled, we are compressing, otherwise it will simply use the same buffer
            var uncompressedSize = payload.Length;
            var compressedPayload = payload;
            // Batch will be compressed when closed
            // If a message has a delayed delivery time, we'll always send it individuallyif (!BatchMessagingEnabled || msgMetadataBuilder.HasDeliverAtTime())
            if (!_configuration.BatchingEnabled || msgMetadataBuilder.HasDeliverAtTime())
            {
                compressedPayload = _compressor.Encode(payload);
                // validate msg-size (For batching this will be check at the batch completion size)
                var compressedSize = compressedPayload.Length;
                if (compressedSize > Commands.DefaultMaxMessageSize)
                {
                    var compressedStr = (!_configuration.BatchingEnabled && _configuration.CompressionType != ICompressionType.None) ? "Compressed" : "";
                    var invalidMessageException = new PulsarClientException.InvalidMessageException($"The producer '{ProducerName}' of the topic '{_configuration.TopicName}' sends a '{compressedStr}' message with '{compressedSize}' bytes that exceeds '{Commands.DefaultMaxMessageSize}' bytes");
                    Sender.Tell(new ErrorMessage(invalidMessageException));
                    Become(Receive);
                    return;
                }
            }
            if (!msg.Replicated && msgMetadataBuilder.HasProducerName())
            {
                var invalidMessageException = new PulsarClientException.InvalidMessageException($"The producer '{ProducerName}' of the topic '{_configuration.TopicName}' can not reuse the same message");
                Sender.Tell(new ErrorMessage(invalidMessageException));
                Become(Receive);
                return;
            }

            if (!PopulateMessageSchema(msg))
            {
                Become(Receive);
                return;
            }

            try
            {
                long sequenceId;
                if (!msgMetadataBuilder.HasSequenceId())
                {
                    sequenceId = _sequenceId++;
                    msgMetadataBuilder.SetSequenceId(sequenceId);
                }
                else
                {
                    sequenceId = (long)msgMetadataBuilder.GetSequenceId();
                }
                if (!msgMetadataBuilder.HasPublishTime())
                {
                    msgMetadataBuilder.SetPublishTime(DateTime.Now.Millisecond);

                    if (!msgMetadataBuilder.HasProducerName())
                        msgMetadataBuilder.SetProducerName(ProducerName);

                    if (_configuration.CompressionType != ICompressionType.None)
                    {
                        msgMetadataBuilder.SetCompression(CompressionCodecProvider.ConvertToWireProtocol(_configuration.CompressionType));
                    }
                    msgMetadataBuilder.SetUncompressedSize(uncompressedSize);
                }
                if (CanAddToBatch(msg))
                {
                    if (CanAddToCurrentBatch(msg))
                    {
                        // should trigger complete the batch message, new message will add to a new batch and new batch
                        // sequence id use the new message, so that broker can handle the message duplication
                        if (sequenceId <= _lastSequenceIdPushed)
                        {
                            if (sequenceId <= _lastSequenceId)
                            {
                                Context.System.Log.Warning("Message with sequence id {} is definitely a duplicate", sequenceId);
                            }
                            else
                            {
                                Context.System.Log.Info("Message with sequence id {} might be a duplicate but cannot be determined at this time.", sequenceId);
                            }
                            DoBatchSendAndAdd(msg, payload);
                        }
                        else
                        {
                            // handle boundary cases where message being added would exceed
                            // batch size and/or max message size
                            var (seqid, isBatchFull) = _batchMessageContainer.Add(msg);
                            _lastSequenceIdPushed = seqid;
                            if (isBatchFull)
                            {
                                BatchMessageAndSend();
                            }
                        }
                    }
                    else
                    {
                        DoBatchSendAndAdd(msg, payload);
                    }
                    //so we can receive more messages in a batching situation
                    Become(Receive);
                }
                else
                {
                    var encryptedPayload = EncryptMessage(msgMetadataBuilder, compressedPayload);
                    // When publishing during replication, we need to set the correct number of message in batch
                    // This is only used in tracking the publish rate stats
                    var numMessages = msg.MessageBuilder.HasNumMessagesInBatch() ? msg.MessageBuilder.NumMessagesInBatch : 1;
                    OpSendMsg op = null;
                    var schemaState = msg.GetSchemaState();
                    if (schemaState == Message.SchemaState.Ready)
                    {
                        var msgMetadata = msgMetadataBuilder.Build();
                        var cmd = SendMessage(_producerId, sequenceId, numMessages, msgMetadata, encryptedPayload);
                        op = OpSendMsg.Create(msg, cmd, sequenceId);
                    }
                    else
                    {
                        op = OpSendMsg.Create(msg, null, sequenceId);
                        op.RePopulate = () =>
                        {
                            var msgMetadata = msgMetadataBuilder.Build();
                            op.Cmd = SendMessage(_producerId, sequenceId, numMessages, msgMetadata, encryptedPayload);

                        };
                    }
                    op.NumMessagesInBatch = numMessages;
                    op.BatchSizeByte = encryptedPayload.Length;
                    ProcessOpSendMsg(op);
                }
            }
            catch (Exception e)
            {
                Sender.Tell(new ErrorMessage(e));
            }
        }
        private void DoBatchSendAndAdd(Message msg, byte[] payload)
        {
            var log = Context.System.Log;
            if (log.IsDebugEnabled)
            {
                log.Debug("[{}] [{}] Closing out batch to accommodate large message with size {}", _configuration.TopicName, ProducerName, msg.DataBuffer.ReadableBytes);
            }
            try
            {
                BatchMessageAndSend();
                _batchMessageContainer.Add(msg);
            }
            finally
            {
                //payload.Release();
            }
        }
        private long GetHighestSequenceId(OpSendMsg op)
        {
            return Math.Max(op.HighestSequenceId, op.SequenceId);
        }
        private void BatchMessageAndSend()
        {
            var log = Context.System.Log;
            if (log.IsDebugEnabled)
            {
                log.Debug("[{}] [{}] Batching the messages from the batch container with {} messages", _configuration.TopicName, ProducerName, _batchMessageContainer.NumMessagesInBatch);
            }
            if (!_batchMessageContainer.Empty)
            {
                try
                {
                    IList<OpSendMsg> opSendMsgs;
                    opSendMsgs = CreateOpSendMsgs();
                    _batchMessageContainer.Clear();
                    foreach (var opSendMsg in opSendMsgs)
                    {
                        ProcessOpSendMsg(opSendMsg);
                    }
                }
                catch (Exception T)
                {
                    log.Error("[{}] [{}] error while create opSendMsg by batch message container", _configuration.TopicName, ProducerName, T);
                }
            }
            else
            {
                Become(Receive);
            }
        }
        public byte[] SendMessage(long producerId, long sequenceId, int numMessages, MessageMetadata msgMetadata, byte[] compressedPayload)
        {
            return Commands.NewSend(producerId, sequenceId, numMessages, msgMetadata, compressedPayload);
        }

        public byte[] SendMessage(long producerId, long lowestSequenceId, long highestSequenceId, int numMessages, MessageMetadata msgMetadata, byte[] compressedPayload)
        {
            return Commands.NewSend(producerId, lowestSequenceId, highestSequenceId, numMessages, msgMetadata, compressedPayload);
        }
        private void ProcessOpSendMsg(OpSendMsg op)
        {
            if (op == null)
            {
                Become(Receive);
                return;
            }
            try
            {
                if (op.Msg != null && _configuration.BatchingEnabled)
                {
                    BatchMessageAndSend();
                }
                if (op.Msg != null)
                {
                    _lastSequenceIdPushed = Math.Max(_lastSequenceIdPushed, GetHighestSequenceId(op));
                }
                if (op.Msg != null && op.Msg.GetSchemaState() == 0)
                {
                    Become(() => TryRegisterSchema(op.Msg));
                }
                else
                {
                    // If we do have a connection, the message is sent immediately, otherwise we'll try again once a new
                    // connection is established
                    Become(() => SendCommand(op));
                }

            }

            catch (Exception T)
            {
                Context.System.Log.Error("[{}] [{}] error while closing out batch -- {}", _configuration.TopicName, ProducerName, T);
                Sender.Tell(new ErrorMessage(new PulsarClientException(T.Message)));
            }
        }

        private void SendCommand(OpSendMsg op)
        {
            Receive<SentReceipt>(s =>
            {
                _listener.MessageSent(s);
                Become(Receive);
            });
            ReceiveAny(_ => Stash.Stash());
            var requestId = _requestId++;
            var pay = new Payload(op.Cmd, requestId, "CommandMessage");
            _broker.Tell(pay);
        }
        private bool PopulateMessageSchema(Message msg)
        {
            var msgMetadataBuilder = msg.MessageBuilder;
            var schemaHash = SchemaHash.Of(msg.Schema);
            var schemaVersion = _schemaCache[schemaHash];
            if (msg.Schema == _schemas[msg.TopicName])
            {
                if (schemaVersion != null)
                    msgMetadataBuilder.SetSchemaVersion(schemaVersion);
                msg.SetSchemaState(Message.SchemaState.Ready);
                return true;
            }
            if (!IsMultiSchemaEnabled(true))
            {
                Sender.Tell(new ErrorMessage(new PulsarClientException.InvalidMessageException($"The producer '{ProducerName}' of the topic '{_configuration.TopicName}' is disabled the `MultiSchema`")));
                return false;
            }

            if (schemaVersion == null) return true;
            msgMetadataBuilder.SetSchemaVersion(schemaVersion);
            msg.SetSchemaState(Message.SchemaState.Ready);
            return true;
        }
        private bool RePopulateMessageSchema(Message msg)
        {
            var schemaHash = SchemaHash.Of(msg.Schema);
            var schemaVersion = _schemaCache[schemaHash];
            if (schemaVersion == null)
            {
                return false;
            }
            msg.MessageBuilder.SetSchemaVersion(schemaVersion);
            msg.SetSchemaState(Message.SchemaState.Ready);
            return true;
        }
        private byte[] EncryptMessage(MessageMetadata.Builder msgMetadata, byte[] compressedPayload)
        {

            var encryptedPayload = compressedPayload;
            if (!_configuration.EncryptionEnabled || _msgCrypto == null)
            {
                return encryptedPayload;
            }
            try
            {
                encryptedPayload = _msgCrypto.Encrypt(_configuration.EncryptionKeys, _configuration.CryptoKeyReader, msgMetadata, compressedPayload);
            }
            catch (PulsarClientException e)
            {
                // Unless config is set to explicitly publish un-encrypted message upon failure, fail the request
                if (_configuration.CryptoFailureAction != ProducerCryptoFailureAction.Send) throw e;
                Context.System.Log.Warning("[{}] [{}] Failed to encrypt message {}. Proceeding with publishing unencrypted message", _configuration.TopicName, ProducerName, e.Message);
                return compressedPayload;
            }
            return encryptedPayload;
        }
        private bool CanAddToBatch(Message msg)
        {
            return msg.GetSchemaState() == Message.SchemaState.Ready && _configuration.BatchingEnabled && !msg.MessageBuilder.HasDeliverAtTime();
        }

        private bool CanAddToCurrentBatch(Message msg)
        {
            return _batchMessageContainer.HaveEnoughSpace(msg) && (!IsMultiSchemaEnabled(false) || _batchMessageContainer.HasSameSchema(msg));
        }

        private Commands.ChecksumType ChecksumType => Commands.ChecksumType.Crc32C;

        public override string ToString()
        {
            return "Producer{" + "topic='" + _configuration.TopicName + '\'' + '}';
        }
        public IStash Stash { get; set; }
        private IList<OpSendMsg> CreateOpSendMsgs()
        {
            var result = new List<OpSendMsg>();
            var list = new List<BatchMessageKeyBasedContainer.KeyedBatch>(_batchMessageContainer.Batches.Values);
            list.Sort();
            foreach (var keyedBatch in list)
            {
                var op = CreateOpSendMsg(keyedBatch);
                if (op != null)
                {
                    result.Add(op);
                }
            }
            return result;
        }
        private OpSendMsg CreateOpSendMsg(BatchMessageKeyBasedContainer.KeyedBatch keyedBatch)
        {
            var encryptedPayload = EncryptMessage(keyedBatch.MessageMetadata, keyedBatch.CompressedBatchMetadataAndPayload);
            if (encryptedPayload.Length > Commands.DefaultMaxMessageSize)
            {
                keyedBatch.Discard(new PulsarClientException.InvalidMessageException("Message size is bigger than " + Commands.DefaultMaxMessageSize + " bytes"));
                return null;
            }

            var numMessagesInBatch = keyedBatch.Messages.Count;
            long currentBatchSizeBytes = 0;
            foreach (var message in keyedBatch.Messages)
            {
                currentBatchSizeBytes += message.DataBuffer.ReadableBytes;
            }
            keyedBatch.MessageMetadata.NumMessagesInBatch = numMessagesInBatch;
            var cmd = SendMessage(_producerId, keyedBatch.SequenceId, numMessagesInBatch, keyedBatch.MessageMetadata.Build(), encryptedPayload);

            var op = OpSendMsg.Create(keyedBatch.Messages, cmd, keyedBatch.SequenceId);

            op.NumMessagesInBatch = numMessagesInBatch;
            op.BatchSizeByte = currentBatchSizeBytes;
            return op;
        }
    }
}
