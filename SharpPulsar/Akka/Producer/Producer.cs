using Akka.Actor;
using Org.BouncyCastle.Crypto;
using SharpPulsar.Akka.InternalCommands;
using SharpPulsar.Akka.InternalCommands.Producer;
using SharpPulsar.Akka.Network;
using SharpPulsar.Api;
using SharpPulsar.Api.Interceptor;
using SharpPulsar.Common.Compression;
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
        private readonly bool _userProvidedProducerName;
        private readonly CompressionCodec _compressor;
        private MessageCrypto _msgCrypto;
        private ConnectedServerInfo _serverInfo;
        private string _topic;
        private int _partitionIndex = -1;

        private readonly IDictionary<string, string> _metadata;
        private bool _multiSchemaMode;
        private BatchMessageKeyBasedContainer _batchMessageContainer;
        private Dictionary<string, ISchema> _schemas;
        private ClientConfigurationData _clientConfiguration;
        private Dictionary<long, (long time, byte[] cmd)> _pendingReceipt = new Dictionary<long, (long time, byte[] cmd)>();
        private readonly List<IProducerInterceptor> _producerInterceptor;
        private readonly Dictionary<long, Payload> _pendingLookupRequests = new Dictionary<long, Payload>();
        private Dictionary<SchemaHash, byte[]> _schemaCache = new Dictionary<SchemaHash, byte[]>();
        private Dictionary<long, Message> _pendingSchemaMessages = new Dictionary<long, Message>();
        private bool _isPartitioned;
        private IActorRef _parent;

        public Producer(ClientConfigurationData clientConfiguration, string topic, ProducerConfigurationData configuration, long producerid, IActorRef network, bool isPartitioned = false, IActorRef parent = null)
        {
            _topic = topic;
            _parent = parent;
            _listener = configuration.ProducerEventListener;
            _schemas = new Dictionary<string, ISchema>();
            _isPartitioned = isPartitioned;
            _clientConfiguration = clientConfiguration;
            _producerInterceptor = configuration.Interceptors;
            _schemas.Add("default", configuration.Schema);
            _configuration = configuration;
            _producerId = producerid;
            _network = network;
            if (isPartitioned)
                _partitionIndex = int.Parse(Self.Path.Name);
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

            if (configuration.Properties == null)
            {
                _metadata = new Dictionary<string, string>();
            }
            else
            {
                _metadata = new SortedDictionary<string, string>(configuration.Properties);
            }
            if (_isPartitioned)
            {
                ProducerName = _topic;
            }

            SendBrokerLookUpCommand();
            Become(LookUp);
        }

        public static Props Prop(ClientConfigurationData clientConfiguration, string topic, ProducerConfigurationData configuration, long producerid, IActorRef network, bool isPartitioned = false, IActorRef parent = null)
        {
            return Props.Create(()=> new Producer(clientConfiguration, topic, configuration, producerid, network, isPartitioned, parent));
        }

        private void LookUp()
        {
            Receive<BrokerLookUp>(p =>
            {
                var l = p;
                var failed = l.Response == CommandLookupTopicResponse.LookupType.Failed;
                _pendingLookupRequests.Remove(l.RequestId);
                var uri = _configuration.UseTls ? new Uri(l.BrokerServiceUrlTls) : new Uri(l.BrokerServiceUrl);

                if(_clientConfiguration.UseProxy)
                    _broker = Context.ActorOf(ClientConnection.Prop(new Uri(_clientConfiguration.ServiceUrl), _clientConfiguration, Self, $"{uri.Host}:{uri.Port}"));
                else
                    _broker = Context.ActorOf(ClientConnection.Prop(uri, _clientConfiguration, Self));
                Become(WaitingForConnection);
            });

            ReceiveAny(x => Stash.Stash());
        }

        private void WaitingForConnection()
        {
            Receive<ConnectedServerInfo>(s =>
            {
                _serverInfo = s;
                SendNewProducerCommand();
                //SetReceiveTimeout(TimeSpan.FromMilliseconds(_clientConfiguration.OperationTimeoutMs));
                Become(WaitingForProducer);
            });
            Receive<PulsarError>(e =>
            {
                if (e.ShouldRetry)
                    SendNewProducerCommand();
            });
        }
        private void WaitingForProducer()
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

                if (_isPartitioned)
                {
                    _parent.Tell(new RegisteredProducer(_producerId, ProducerName, _topic));
                }
                else
                {
                    _configuration.ProducerEventListener.ProducerCreated(new CreatedProducer(Self, _topic));
                }
                BecomeReceive();
            });
            ReceiveAny(x => Stash.Stash());
        }
        private void BecomeReceive()
        {
            if (_configuration.EncryptionEnabled)
            {
                var logCtx = "[" + _topic + "] [" + ProducerName + "] [" + _producerId + "]";
                _msgCrypto = new MessageCrypto(logCtx, true);

                // Regenerate data key cipher at fixed interval
                Context.System.Scheduler.ScheduleTellRepeatedly(TimeSpan.FromSeconds(0), TimeSpan.FromHours(4), Self, new AddPublicKeyCipher(), ActorRefs.NoSender);
            }
            Become(Receive);
            Context.Watch(_broker);
            Stash.UnstashAll();
        }
        public void Receive()
        {
            Context.System.Scheduler.ScheduleTellRepeatedly(TimeSpan.FromMilliseconds(_configuration.SendTimeoutMs), TimeSpan.FromMilliseconds(_configuration.SendTimeoutMs), Self, new ResendMessages(), ActorRefs.NoSender);
            //Receive<Terminated>(t => t.ActorRef.Equals(_broker), b => BecomeLookUp());
            Receive<AddPublicKeyCipher>(a =>
            {
                AddKey();
            });
            Receive<ResendMessages>(a =>
            {
                var d = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                var ms = _pendingReceipt.Where(x => (d - x.Value.time) > _configuration.SendTimeoutMs).Select(x => new {x.Key, x.Value.cmd}).ToList();
                foreach (var m in ms)
                {
                    var requestId = m.Key;
                    var pay = new Payload(m.cmd, requestId, "CommandMessage");
                    _broker.Tell(pay);
                }
            });
            Receive<ProducerClosed>(_ =>
            {
                //ReceiveAny(c => Stash.Stash());
                //Context.System.Scheduler.ScheduleTellOnce(TimeSpan.FromSeconds(10), Self, new RecreateProducer(), ActorRefs.NoSender);
            });
            Receive<RecreateProducer>(_ =>
            {
                //BecomeLookUp();
            });
            Receive<Send>(ProcessSend);
            Receive<SentReceipt>(s =>
            {
                var ns = new SentReceipt(s.ProducerId, s.SequenceId, s.EntryId, s.LedgerId, s.BatchIndex, _partitionIndex);
                _pendingReceipt.Remove(s.SequenceId);
                _listener.MessageSent(ns);
            });
            Receive<GetOrCreateSchemaServerResponse>(r =>
            {
                _pendingLookupRequests.Remove(r.RequestId);
                var msg = _pendingSchemaMessages[r.RequestId];
                _pendingSchemaMessages.Remove(r.RequestId);
                var schemaHash = SchemaHash.Of(msg.Schema);
                if (!_schemaCache.ContainsKey(schemaHash))
                {
                    _schemaCache[schemaHash] = r.SchemaVersion;
                }
                msg.Metadata.SchemaVersion = r.SchemaVersion;
                msg.SetSchemaState(Message.SchemaState.Ready);
                _schemas[msg.TopicName] = msg.Schema;
                PrepareMessage(msg);
            });
            Receive<BulkSend>(s =>
            {
                foreach (var m in s.Messages)
                {
                    ProcessSend(m);
                }
            });
            
        }

        private void ProcessSend(Send s)
        {
            try
            {
                var schemaName = s.Topic;
                ISchema schema;
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
                        case "producer":
                            builder.ProducerName(c.Value.ToString());
                            break;
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
                if(_schemas.ContainsKey(s.Topic))
                    interceptorMessage.SetSchemaState(Message.SchemaState.Ready);
                PrepareMessage(interceptorMessage);
            }
            catch (Exception e)
            {
                _listener.Log(e.ToString());
            }
        }

        private void SendNewProducerCommand()
        {
            var requestid = Interlocked.Increment(ref IdGenerators.ReaderId);
            var schemaInfo = (SchemaInfo)_configuration.Schema.SchemaInfo;
            var request = Commands.NewProducer(_topic, _producerId, requestid, ProducerName, _configuration.EncryptionEnabled, _metadata, schemaInfo, DateTime.Now.Millisecond, _userProvidedProducerName);
            var payload = new Payload(request, requestid, "CommandProducer");
            _pendingLookupRequests.Add(requestid, payload);
            _broker.Tell(payload);
        }
       
        private void SendBrokerLookUpCommand()
        {
            var requestid = Interlocked.Increment(ref IdGenerators.RequestId);
            var request = Commands.NewLookup(_topic, false, requestid);
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
        public class ResendMessages
        {

        }
        public class RecreateProducer
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
            var request = Commands.NewGetOrCreateSchema(requestId, _topic, schemaInfo);
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
                    if (compressedSize > _serverInfo.MaxMessageSize)
                    {
                        var compressedStr = !_configuration.BatchingEnabled && _configuration.CompressionType != ICompressionType.None ? "Compressed" : "";
                        Context.System.Log.Warning($"The producer '{ProducerName}' of the topic '{_topic}' sends a '{compressedStr}' message with '{compressedSize}' bytes that exceeds '{_serverInfo.MaxMessageSize}' bytes");
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
                    metadata.PublishTime = (ulong)DateTimeOffset.Now.ToUnixTimeMilliseconds();
                }
                if (string.IsNullOrWhiteSpace(metadata.ProducerName))
                    metadata.ProducerName = ProducerName;

                if (_configuration.CompressionType != ICompressionType.None)
                {
                    metadata.Compression = Enum.GetValues(typeof(CompressionType)).Cast<CompressionType>().ToList()[(int)_configuration.CompressionType];
                }
                metadata.UncompressedSize = (uint)uncompressedSize;
                if(metadata.EventTime < 1)
                    metadata.EventTime = (ulong)DateTimeOffset.Now.ToUnixTimeMilliseconds();
                SendMessage(msg);
            }
            catch (Exception e)
            {
                Context.System.Log.Error(e.ToString());
            }
        }
        private void SendMessage(Message msg)
        {
            SendImmediate(msg);
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
                log.Debug($"[{_topic}] [{ProducerName}] Closing out batch to accommodate large message with size {msg.Payload.Length}");
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
                    log.Error($"[{_topic}] [{ProducerName}] error while create opSendMsg by batch message container: {T}");
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
                if (op.Msg.GetSchemaState() == 0)
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
                Context.System.Log.Error($"[{_topic}] [{ ProducerName}] error while closing out batch -- {T}");
                Sender.Tell(new ErrorMessage(new PulsarClientException(T.Message)));
            }
        }

        private void SendCommand(OpSendMsg op)
        {
            var requestId = op.SequenceId;
            var pay = new Payload(op.Cmd, requestId, "CommandMessage");
            _pendingReceipt.Add(requestId, (DateTimeOffset.Now.ToUnixTimeMilliseconds(), op.Cmd));
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
                Context.System.Log.Warning($"[{_topic}] [{ProducerName}] Failed to encrypt message '{e.Message}'. Proceeding with publishing unencrypted message");
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
            return "Producer{" + "topic='" + _topic + '\'' + '}';
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
            if (encryptedPayload.Length > _serverInfo.MaxMessageSize)
            {
                keyedBatch.Discard(new PulsarClientException.InvalidMessageException("Message size is bigger than " + _serverInfo.MaxMessageSize + " bytes"));
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
