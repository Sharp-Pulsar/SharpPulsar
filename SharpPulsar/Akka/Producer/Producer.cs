using Akka.Actor;
using Org.BouncyCastle.Crypto;
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
using System.Threading;
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
        private readonly long _producerId;
        public string ProducerName;
        private readonly bool _userProvidedProducerName = false;
        private long _lastSequenceIdPushed;
        private long _lastSequenceId;
        private readonly CompressionCodec _compressor;
        private readonly MessageCrypto _msgCrypto = null;

        private readonly IDictionary<string, string> _metadata;
        private bool _multiSchemaMode;
        private BatchMessageKeyBasedContainer _batchMessageContainer;
        private Dictionary<string, ISchema> _schemas;
        private ClientConfigurationData _clientConfiguration;
        private readonly List<IProducerInterceptor> _producerInterceptor;
        private readonly Dictionary<long, Payload> _pendingLookupRequests = new Dictionary<long, Payload>();
        private Dictionary<SchemaHash, byte[]> _schemaCache = new Dictionary<SchemaHash, byte[]>();
        private Dictionary<long, Message> _pendingSchemaMessages = new Dictionary<long, Message>();
        private bool _isPartitioned;

        public Producer(ClientConfigurationData clientConfiguration, ProducerConfigurationData configuration, long producerid, IActorRef network, bool isPartitioned = false)
        {
            _listener = configuration.ProducerEventListener;
            _schemas = new Dictionary<string, ISchema>();
            _isPartitioned = isPartitioned;
            _clientConfiguration = clientConfiguration;
            _producerInterceptor = configuration.Interceptors;
            _schemas.Add("default", configuration.Schema);
            _configuration = configuration;
            if (isPartitioned)
                _producerId = Interlocked.Increment(ref IdGenerators.ProducerId);
            else
                _producerId = producerid;
            _network = network;
            ProducerName = configuration.ProducerName;
            if (!configuration.MultiSchema)
            {
                _multiSchemaMode = false;
            }
            if (!string.IsNullOrWhiteSpace(ProducerName) || isPartitioned)
            {
                _userProvidedProducerName = true;
            }

            _compressor = CompressionCodecProvider.GetCompressionCodec(configuration.CompressionType);
            if (configuration.InitialSequenceId != null)
            {
                var initialSequenceId = (long)configuration.InitialSequenceId;
                IdGenerators.SequenceId = initialSequenceId;
            }
            else
            {
                IdGenerators.SequenceId = -1L;
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
                _batchMessageContainer = new BatchMessageKeyBasedContainer
                {
                    TopicName = configuration.TopicName, ProducerName = _configuration.ProducerName,
                    Compressor = _compressor
                };
            }
            else
            {
                _batchMessageContainer = null;
            }
            if (configuration.Properties == null)
            {
                _metadata = new Dictionary<string, string>();
            }
            else
            {
                _metadata = new SortedDictionary<string, string>(configuration.Properties);
            }
            Receive<BrokerLookUp>(l =>
            {
                _pendingLookupRequests.Remove(l.RequestId);
                var uri = _configuration.UseTls ? new Uri(l.BrokerServiceUrlTls) : new Uri(l.BrokerServiceUrl);

                var address = new IPEndPoint(Dns.GetHostAddresses(uri.Host)[0], uri.Port);
                _broker = Context.ActorOf(ClientConnection.Prop(address, _clientConfiguration, Sender));

            });
            Receive<TcpSuccess>(s =>
            {
                _listener.Log($"Pulsar handshake completed with {s.Name}");
                Become(CreateProducer);
            });
            Receive<AddPublicKeyCipher>(a =>
            {
                AddKey();
            });
            Receive<AddPublicKeyCipher>(a =>
            {
                AddKey();
            });
            ReceiveAny(c => Stash.Stash());
            SendBrokerLookUpCommand();
        }

        public static Props Prop(ClientConfigurationData clientConfiguration, ProducerConfigurationData configuration, long producerid, IActorRef network, bool isPartitioned = false)
        {
            return Props.Create(() => new Producer(clientConfiguration, configuration, producerid, network, isPartitioned));
        }

        
        public void Receive()
        {
            Receive<AddPublicKeyCipher>(a =>
            {
                AddKey();
            });
            Receive<TcpClosed>(_ =>
            {
                SendBrokerLookUpCommand();
            });
            Receive<Send>(ProcessSend);
            Receive<SentReceipt>(s =>
            {
                _listener.MessageSent(s);
                Become(Receive);
            });
            Receive<GetOrCreateSchemaServerResponse>(r =>
            {
                var msg = _pendingSchemaMessages[r.RequestId];
                _pendingSchemaMessages.Remove(r.RequestId);
                var schemaHash = SchemaHash.Of(msg.Schema);
                if (!_schemaCache.ContainsKey(schemaHash))
                {
                    _schemaCache[schemaHash] = r.SchemaVersion;
                }
                msg.Metadata.SchemaVersion = r.SchemaVersion;
                msg.SetSchemaState(Message.SchemaState.Ready);
                PrepareMessage(msg);
            });
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
                builder.Topic(s.Topic);
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

                var interceptorMessage = (Message)BeforeSend(message);
                if (_producerInterceptor != null)
                {
                    _listener.Log(interceptorMessage.Properties);
                }
                PrepareMessage(interceptorMessage);
            }
            catch (Exception e)
            {
                _listener.Log(e.ToString());
            }
        }
        private void CreateProducer()
        {
            Receive<ProducerCreated>(p =>
            {
                _pendingLookupRequests.Remove(p.RequestId);
                if (string.IsNullOrWhiteSpace(ProducerName))
                    ProducerName = p.Name;
                IdGenerators.SequenceId = p.LastSequenceId;
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
            ReceiveAny(x => Stash.Stash());
            if (_isPartitioned)
            {
                var index = int.Parse(Self.Path.Name);
                ProducerName = TopicName.Get(_configuration.TopicName).GetPartition(index).ToString();
            }
            SendNewProducerCommand();
        }
        private void SendNewProducerCommand()
        {
            var requestid = Interlocked.Increment(ref IdGenerators.ReaderId);
            var schemaInfo = (SchemaInfo)_configuration.Schema.SchemaInfo;
            var request = Commands.NewProducer(_configuration.TopicName, _producerId, requestid, ProducerName, _configuration.EncryptionEnabled, _metadata, schemaInfo, DateTime.Now.Millisecond, _userProvidedProducerName);
            var payload = new Payload(request, requestid, "CommandProducer");
            _pendingLookupRequests.Add(requestid, payload);
            _broker.Tell(payload);
        }
       
        private void SendBrokerLookUpCommand()
        {
            var requestid = Interlocked.Increment(ref IdGenerators.ReaderId);
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
            SchemaInfo schemaInfo;
            if (msg.Schema != null && msg.Schema.SchemaInfo.Type.Value > 0)
            {
                schemaInfo = (SchemaInfo)msg.Schema.SchemaInfo;
            }
            else
            {
                schemaInfo = (SchemaInfo)SchemaFields.Bytes.SchemaInfo;
            }

            var requestId = Interlocked.Increment(ref IdGenerators.RequestId);
            _pendingSchemaMessages.Add(requestId, msg);
            SendGetOrCreateSchemaCommand(schemaInfo, requestId);
        }
        private void SendGetOrCreateSchemaCommand(SchemaInfo schemaInfo, long requestId)
        {
            var request = Commands.NewGetOrCreateSchema(requestId, _configuration.TopicName, schemaInfo);
            Context.System.Log.Info($"[{_configuration.TopicName}] [{ProducerName}] GetOrCreateSchema request");
            var payload = new Payload(request, requestId, "GetOrCreateSchema");
            _broker.Tell(payload);
            _pendingLookupRequests.Add(requestId, payload);
        }

        private void PrepareMessage(Message msg)
        {
            try
            {
                var metadata = msg.Metadata;
                var payload = msg.Payload;
                // If compression is enabled, we are compressing, otherwise it will simply use the same buffer
                var uncompressedSize = payload.Length;
                // Batch will be compressed when closed
                // If a message has a delayed delivery time, we'll always send it individually if (!BatchMessagingEnabled || metadata.HasDeliverAtTime())
                if (!_configuration.BatchingEnabled || metadata.DeliverAtTime > 0)
                {
                    var compressedPayload = _compressor.Encode(payload);
                    // validate msg-size (For batching this will be check at the batch completion size)
                    var compressedSize = compressedPayload.Length;
                    if (compressedSize > Commands.DefaultMaxMessageSize)
                    {
                        var compressedStr = !_configuration.BatchingEnabled && _configuration.CompressionType != ICompressionType.None ? "Compressed" : "";
                        Context.System.Log.Warning($"The producer '{ProducerName}' of the topic '{_configuration.TopicName}' sends a '{compressedStr}' message with '{compressedSize}' bytes that exceeds '{Commands.DefaultMaxMessageSize}' bytes");
                        return;
                    }

                    msg.Payload = compressedPayload;
                }
                if (!PopulateMessageSchema(msg))
                {
                    return;
                }

                if (metadata.SequenceId < 1)
                {
                    var sequenceId = Interlocked.Increment(ref IdGenerators.SequenceId);
                    metadata.SequenceId = (ulong)sequenceId;
                }
                if (metadata.PublishTime < 1)
                {
                    metadata.PublishTime = (ulong)DateTime.Now.Millisecond;

                    if (string.IsNullOrWhiteSpace(metadata.ProducerName))
                        metadata.ProducerName = ProducerName;

                    if (_configuration.CompressionType != ICompressionType.None)
                    {
                        metadata.Compression = Enum.GetValues(typeof(CompressionType)).Cast<CompressionType>().ToList()[(int)_configuration.CompressionType];
                    }
                    metadata.UncompressedSize = (uint)uncompressedSize;
                }

                SendMessage(msg);
            }
            catch (Exception e)
            {
                Context.System.Log.Error(e.ToString());
            }
        }
        private void SendMessage(Message msg)
        {
            var canAddToBatch = CanAddToBatch(msg);
            if (canAddToBatch)
            {
                BatchMessage(msg);
            }
            else
            {
                SendImmediate(msg);
            }
        }
        private void BatchMessage(Message msg)
        {
            var metadata = msg.Metadata;
            var sequenceid = (long)metadata.SequenceId;
            var canAddToCurrentBatch = CanAddToCurrentBatch(msg);
            if (canAddToCurrentBatch)
            {
                // should trigger complete the batch message, new message will add to a new batch and new batch
                // sequence id use the new message, so that broker can handle the message duplication
                if (sequenceid <= _lastSequenceIdPushed)
                {
                    if (sequenceid <= _lastSequenceId)
                    {
                        Context.System.Log.Warning($"Message with sequence id {sequenceid} is definitely a duplicate");
                    }
                    else
                    {
                        Context.System.Log.Info($"Message with sequence id {sequenceid} might be a duplicate but cannot be determined at this time.");
                    }
                    DoBatchSendAndAdd(msg);
                }
                else
                {
                    // handle boundary cases where message being added would exceed
                    // batch size and/or max message size
                    var (_, isBatchFull) = _batchMessageContainer.Add(msg);
                    //_lastSequenceIdPushed = seqid;
                    if (isBatchFull)
                    {
                        BatchMessageAndSend();
                    }
                }
            }
            else
            {
                DoBatchSendAndAdd(msg);
            }
        }
        private void SendImmediate(Message msg)
        {
            MessageMetadata metadata = msg.Metadata;
            var encryptedPayload = EncryptMessage(metadata, msg.Payload);
            // When publishing during replication, we need to set the correct number of message in batch
            // This is only used in tracking the publish rate stats
            var numMessages = metadata.NumMessagesInBatch > 0 ? metadata.NumMessagesInBatch : 1;
            OpSendMsg op = null;
            var sequenceid = (long) metadata.SequenceId;
            var schemaState = msg.GetSchemaState();
            if (schemaState == Message.SchemaState.Ready)
            {
                var msgMetadata = metadata;
                var cmd = SendMessage(_producerId, sequenceid, numMessages, msgMetadata, encryptedPayload);
                op = OpSendMsg.Create(msg, cmd, sequenceid);
            }
            else
            {
                op = OpSendMsg.Create(msg, null, sequenceid);
                op.RePopulate = () =>
                {
                    var msgMetadata = metadata;
                    op.Cmd = SendMessage(_producerId, sequenceid, numMessages, msgMetadata, encryptedPayload);

                };
            }
            op.NumMessagesInBatch = numMessages;
            op.BatchSizeByte = encryptedPayload.Length;
            ProcessOpSendMsg(op);
        }
        private void DoBatchSendAndAdd(Message msg)
        {
            var log = Context.System.Log;
            if (log.IsDebugEnabled)
            {
                log.Debug($"[{_configuration.TopicName}] [{ProducerName}] Closing out batch to accommodate large message with size {msg.Payload.Length}");
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
                log.Debug($"[{_configuration.TopicName}] [{ProducerName}] Batching the messages from the batch container with {_batchMessageContainer.NumMessagesInBatch} messages");
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
                    log.Error($"[{_configuration.TopicName}] [{ProducerName}] error while create opSendMsg by batch message container: {T}");
                }
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
                return;
            }
            try
            {
                if (_configuration.BatchingEnabled)
                {
                    BatchMessageAndSend();
                }
                if (op.Msg != null)
                {
                    //_lastSequenceIdPushed = Math.Max(_lastSequenceIdPushed, GetHighestSequenceId(op));
                }
                if (op.Msg != null && op.Msg.GetSchemaState() == 0)
                {
                    TryRegisterSchema(op.Msg);
                }
                else
                {
                    // If we do have a connection, the message is sent immediately, otherwise we'll try again once a new
                    // connection is established
                   SendCommand(op);
                }

            }

            catch (Exception T)
            {
                Context.System.Log.Error($"[{_configuration.TopicName}] [{ ProducerName}] error while closing out batch -- {T}");
                Sender.Tell(new ErrorMessage(new PulsarClientException(T.Message)));
            }
        }

        private void SendCommand(OpSendMsg op)
        {
            var requestId = op.SequenceId;
            var pay = new Payload(op.Cmd, requestId, "CommandMessage");
            _broker.Tell(pay);
        }
        private bool PopulateMessageSchema(Message msg)
        {
            var metadata = msg.Metadata;
            var schemaHash = SchemaHash.Of(msg.Schema);
            _schemaCache.TryGetValue(schemaHash, out var schemaVersion);
            if (_schemas.TryGetValue(msg.TopicName, out var s))
            {
                if (s != null)
                {
                    if (msg.Schema == s)
                    {
                        if (schemaVersion != null)
                            metadata.SchemaVersion = schemaVersion;
                        msg.SetSchemaState(Message.SchemaState.Ready);
                        return true;
                    }
                }
            }
            
            if (schemaVersion == null) return true;
            metadata.SchemaVersion = schemaVersion;
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
            msg.Metadata.SchemaVersion = schemaVersion;
            msg.SetSchemaState(Message.SchemaState.Ready);
            return true;
        }
        private byte[] EncryptMessage(MessageMetadata msgMetadata, byte[] compressedPayload)
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
                Context.System.Log.Warning($"[{_configuration.TopicName}] [{ProducerName}] Failed to encrypt message '{e.Message}'. Proceeding with publishing unencrypted message");
                return compressedPayload;
            }
            return encryptedPayload;
        }
        private bool CanAddToBatch(Message msg)
        {
            return msg.GetSchemaState() == Message.SchemaState.Ready && _configuration.BatchingEnabled && msg.Metadata.DeliverAtTime < 1;
        }

        private bool CanAddToCurrentBatch(Message msg)
        {
            return _batchMessageContainer.HaveEnoughSpace(msg) || _batchMessageContainer.HasSameSchema(msg);
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
                currentBatchSizeBytes += message.Payload.Length;
            }
            keyedBatch.MessageMetadata.NumMessagesInBatch = numMessagesInBatch;
            var cmd = SendMessage(_producerId, keyedBatch.SequenceId, numMessagesInBatch, keyedBatch.MessageMetadata, encryptedPayload);

            var op = OpSendMsg.Create(keyedBatch.Messages, cmd, keyedBatch.SequenceId);

            op.NumMessagesInBatch = numMessagesInBatch;
            op.BatchSizeByte = currentBatchSizeBytes;
            return op;
        }
    }
}
