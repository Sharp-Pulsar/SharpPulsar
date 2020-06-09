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
        private readonly Queue<OpSendMsg> _pendingMessages;
        private readonly IActorRef _network;
        private readonly ProducerConfigurationData _configuration;
        private readonly IProducerEventListener _listener;
        private readonly long _producerId;
        public string ProducerName;
        private readonly bool _userProvidedProducerName;
        private readonly CompressionCodec _compressor;
        private MessageCrypto _msgCrypto;
        private ConnectedServerInfo _serverInfo;
        private readonly string _topic;
        private long _sequenceId = 0;
        private readonly int _partitionIndex = -1;
        private readonly IActorRef _pulsarManager;

        private readonly IDictionary<string, string> _metadata;
        private readonly ClientConfigurationData _clientConfiguration;
        private readonly List<IProducerInterceptor> _producerInterceptor;
        private readonly Dictionary<SchemaHash, byte[]> _schemaCache = new Dictionary<SchemaHash, byte[]>();
        private readonly bool _isPartitioned;
        private readonly bool _isGroup;
        private ICancelable _producerRecreator;
        private readonly TopicSchema _topicSchema;
        private bool _multiSchemaEnabled;

        public Producer(ClientConfigurationData clientConfiguration, string topic, ProducerConfigurationData configuration, long producerId, IActorRef network, IActorRef pulsarManager, bool isPartitioned, bool isgroup)
        {
            _pendingMessages = new Queue<OpSendMsg>();
            _topicSchema = new TopicSchema
            {
                Schema = configuration?.Schema,
                Ready = false,
                Version = Array.Empty<byte>()
            };
            _pulsarManager = pulsarManager;
            _topic = topic;
            _listener = configuration?.ProducerEventListener;
            _isPartitioned = isPartitioned;
            _isGroup = isgroup;
            _clientConfiguration = clientConfiguration;
            _producerInterceptor = configuration?.Interceptors;
            _configuration = configuration;
            _producerId = producerId;
            _network = network;
            if (isPartitioned)
            {
                _partitionIndex = int.Parse(Self.Path.Name);
                ProducerName = topic.Split("/").Last();
            }
            else
                ProducerName = configuration.ProducerName;
            if (!string.IsNullOrWhiteSpace(ProducerName) || isPartitioned)
            {
                _userProvidedProducerName = true;
            }

            _compressor = CompressionCodecProvider.GetCompressionCodec(configuration.CompressionType);
            if (configuration.InitialSequenceId.HasValue && configuration.InitialSequenceId > 0)
            {
                var initialSequenceId = (long)configuration.InitialSequenceId;
                _sequenceId = initialSequenceId;
            }
            else
            {
                _sequenceId = 0L;
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

            _multiSchemaEnabled = configuration.MultiSchema;
            SendBrokerLookUpCommand();
            Become(LookUp);
        }

        public static Props Prop(ClientConfigurationData clientConfiguration, string topic, ProducerConfigurationData configuration, long producerid, IActorRef network, IActorRef pulsarManager, bool isPartitioned = false, bool isgroup = false)
        {
            return Props.Create(()=> new Producer(clientConfiguration, topic, configuration, producerid, network, pulsarManager, isPartitioned, isgroup));
        }

        private void LookUp()
        {
            Receive<BrokerLookUp>(p =>
            {
                var l = p;
                var failed = l.Response == CommandLookupTopicResponse.LookupType.Failed;
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
                Become(WaitingForProducer);
            });
            PulsarError();
        }
        private void WaitingForProducer()
        {
            Receive<ProducerCreated>(p =>
            {
                if(_producerRecreator != null) 
                {
                    _producerRecreator.Cancel();
                    _producerRecreator = null;
                }
                if (string.IsNullOrWhiteSpace(ProducerName))
                    ProducerName = p.Name;
                _sequenceId = p.LastSequenceId  == -1 ? 0 : p.LastSequenceId; //brokerDeduplicationEnabled must be enabled
                var schemaVersion = p.SchemaVersion;
                if (schemaVersion != null)
                {
                    _topicSchema.Ready = true;
                    _topicSchema.Version = schemaVersion;
                }

                if (_isPartitioned)
                {
                    Context.Parent.Tell(new RegisteredProducer(_producerId, ProducerName, _topic));
                }
                else if (_isGroup)
                {
                    Context.Parent.Tell(new RegisteredProducer(_producerId, ProducerName, _topic));
                }
                else
                {
                    _pulsarManager.Tell(new CreatedProducer(Self, _topic, ProducerName));
                }
                BecomeReceive();
            });
            PulsarError();
            ReceiveAny(x => Stash.Stash());
        }

        private void PulsarError()
        {

            Receive<PulsarError>(e =>
            {
                if (e.ShouldRetry)
                    SendNewProducerCommand();
                else
                {
                    if (_isPartitioned)
                    {
                        Context.Parent.Tell(e);
                    }
                    else if (_isGroup)
                    {
                        Context.Parent.Tell(e);
                    }
                    else
                    {
                        _configuration.ProducerEventListener.Log($"{e.Error}: {e.Message}");
                        _pulsarManager.Tell(new CreatedProducer(null, string.Empty, string.Empty));
                    }
                    Context.System.Stop(Self);
                }
            });
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
            Receive<RegisterSchema>(s =>
            {
                var requestid = Interlocked.Increment(ref IdGenerators.RequestId);
                SchemaInfo schemaInfo;
                if (s.Schema != null && s.Schema.SchemaInfo.Type.Value > 0)
                {
                    schemaInfo = (SchemaInfo)s.Schema.SchemaInfo;
                }
                else
                {
                    schemaInfo = (SchemaInfo)SchemaFields.Bytes.SchemaInfo;
                }
                var response = SendGetOrCreateSchemaCommand(schemaInfo, requestid, s.Topic);
                _pulsarManager.Tell(response);
            });
            Receive<AddPublicKeyCipher>(a =>
            {
                AddKey();
            });
            Receive<ProducerClosed>(p =>
            {
                foreach (var c in Context.GetChildren())
                {
                    Context.Stop(c);
                }
                Become(RecreatingProducer);
            });
            Receive<Send>(ProcessSend);
            Receive<BulkSend>(s =>
            {
                foreach (var m in s.Messages)
                {
                    Self.Tell(m);
                }
            });
            Receive<Terminated>(_ =>
            {
                foreach (var c in Context.GetChildren())
                {
                    Context.Stop(c);
                }

                Become(RecreatingProducer);
            });
        }

        private void RecreatingProducer()
        {
            _producerRecreator = Context.System.Scheduler.ScheduleTellRepeatedlyCancelable(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(15), Self, new RecreateProducer(), ActorRefs.NoSender);
            Receive<RecreateProducer>(_ =>
            {
                SendBrokerLookUpCommand();
                Become(LookUp);
            });
            ReceiveAny(any => Stash.Stash());
        }
        private void ProcessSend(Send s)
        {
            try
            {
                var builder = new TypedMessageBuilder(ProducerName, _topicSchema.Schema);
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
                PrepareMessage(interceptorMessage);
            }
            catch (Exception e)
            {
                _listener.Log(e.ToString());
            }
        }

        private void SendNewProducerCommand()
        {
            var requestid = Interlocked.Increment(ref IdGenerators.RequestId);
            var schemaInfo = (SchemaInfo)_configuration.Schema.SchemaInfo;
            var request = Commands.NewProducer(_topic, _producerId, requestid, ProducerName, _configuration.EncryptionEnabled, _metadata, schemaInfo, DateTime.Now.Millisecond, _userProvidedProducerName);
            var payload = new Payload(request, requestid, "CommandProducer");
            _broker.Tell(payload);
        }
       
        private void SendBrokerLookUpCommand()
        {
            var requestid = Interlocked.Increment(ref IdGenerators.RequestId);
            var request = Commands.NewLookup(_topic, false, requestid);
            var load = new Payload(request, requestid, "BrokerLookUp");
            _network.Tell(load);
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
        private bool IsMultiSchemaEnabled()
        {
            return _multiSchemaEnabled;
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
            var schemaResponse =  SendGetOrCreateSchemaCommand(schemaInfo, requestId, _topic);
            var schemaHash = SchemaHash.Of(msg.Schema);
            _schemaCache[schemaHash] = schemaResponse.SchemaVersion;
            msg.Metadata.SchemaVersion = schemaResponse.SchemaVersion;
            msg.SetSchemaState(Message.SchemaState.Ready);
        }
        private GetOrCreateSchemaServerResponse SendGetOrCreateSchemaCommand(SchemaInfo schemaInfo, long requestId, string topic)
        {
            var request = Commands.NewGetOrCreateSchema(requestId, topic, schemaInfo);
            var payload = new Payload(request, requestId, "GetOrCreateSchema");
            return  _broker.Ask<GetOrCreateSchemaServerResponse>(payload, TimeSpan.FromMilliseconds(_clientConfiguration.OperationTimeoutMs)).Result;
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
                if (_configuration.CompressionType != ICompressionType.None && metadata.UncompressedSize == 0)
                {
                    var compressedPayload = _compressor.Encode(payload);
                    // validate msg-size (For batching this will be check at the batch completion size)
                    var compressedSize = compressedPayload.Length;
                    if (compressedSize > _serverInfo.MaxMessageSize)
                    {
                        var compressedStr = _configuration.CompressionType != ICompressionType.None ? "Compressed" : "";
                        Context.System.Log.Warning($"The producer '{ProducerName}' of the topic '{_topic}' sends a '{compressedStr}' message with '{compressedSize}' bytes that exceeds '{_serverInfo.MaxMessageSize}' bytes");
                        return;
                    }

                    msg.Payload = compressedPayload;
                    metadata.Compression = CompressionCodecProvider.ConvertToWireProtocol(_configuration.CompressionType);
                }
                if (!PopulateMessageSchema(msg))
                {
                    return;
                }

                if (!HasSequenceId(metadata.SequenceId))
                {
                     _sequenceId += 1;
                    metadata.SequenceId = (ulong)_sequenceId;
                }
                if (!HasPublishTime(metadata.PublishTime))
                {
                    metadata.PublishTime = (ulong)DateTimeOffset.Now.ToUnixTimeMilliseconds();
                }

                if (string.IsNullOrWhiteSpace(metadata.ProducerName))
                    metadata.ProducerName = ProducerName;

                if(metadata.UncompressedSize == 0)
                    metadata.UncompressedSize = (uint)uncompressedSize;

                if(!HasEventTime(metadata.EventTime))
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
                var msgMetadata = metadata;
                op.Cmd = SendMessage(_producerId, sequenceid, numMessages, msgMetadata, encryptedPayload);
            }
            op.NumMessagesInBatch = numMessages;
            op.BatchSizeByte = encryptedPayload.Length;
            ProcessOpSendMsg(op);
        }
        private long GetHighestSequenceId(OpSendMsg op)
        {
            return Math.Max(op.HighestSequenceId, op.SequenceId);
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
                _pendingMessages.Enqueue(op);
                if (op.Msg.GetSchemaState() == 0)
                {
                    TryRegisterSchema(op.Msg);
                }
                // If we do have a connection, the message is sent immediately, otherwise we'll try again once a new
                // connection is established
                var receipt = SendCommand(op);
                OnSendAcknowledgement(op.Msg, op.Msg.MessageId, null);
                HandleSendReceipt(receipt);
                _listener.MessageSent(receipt);
            }

            catch (AskTimeoutException ex)
            {
                _listener.Log(ex.ToString());
                ProcessOpSendMsg(op);
            }
        }
        private void HandleSendReceipt(SentReceipt sendReceipt)
        {
            var producerId = sendReceipt.ProducerId;
            var sequenceId = sendReceipt.SequenceId;
            var highestSequenceId = sendReceipt.HighestSequenceId;
            var ledgerId = sendReceipt.LedgerId;
            var entryId = sendReceipt.EntryId;
            if (ledgerId == -1 && entryId == -1)
            {
                Context.System.Log.Warning($"Message has been dropped for non-persistent topic producer-id {producerId}-{sequenceId}");
            }

            if (Context.System.Log.IsDebugEnabled)
            {
                Context.System.Log.Debug($"Got receipt for producer: {producerId} -- msg: {sequenceId} -- id: {ledgerId}:{entryId}");
            }

            AckReceived(sequenceId, highestSequenceId, ledgerId, entryId);
        }
        private void AckReceived(long sequenceId, long highestSequenceId, long ledgerId, long entryId)
        {
            var log = Context.System.Log;
            var op = _pendingMessages.Peek();
            if (op == null)
            {
                if (log.IsDebugEnabled)
                {
                    log.Debug($"[{_topic}] [{ProducerName}] Got ack for timed out msg {sequenceId} - {highestSequenceId}");
                }
                return;
            }

            if (sequenceId > op.SequenceId)
            {
                log.Warning($"[{_topic}] [{ProducerName}] Got ack for msg. expecting: {op.SequenceId} - {op.HighestSequenceId} - got: {sequenceId} - {sequenceId} - queue-size: {_pendingMessages.Count}");
                // Force connection closing so that messages can be re-transmitted in a new connection
                Self.Tell(new ProducerClosed(_producerId));
            }
            else if (sequenceId < op.SequenceId)
            {
                // Ignoring the ack since it's referring to a message that has already timed out.
                if (log.IsDebugEnabled)
                {
                    log.Debug($"[{_topic}] [{ProducerName}] Got ack for timed out msg. expecting: {op.SequenceId} - {op.HighestSequenceId} - got: {sequenceId} - {highestSequenceId}");
                }
            }
            else
            {
                // Add check `sequenceId >= highestSequenceId` for backward compatibility.
                if (sequenceId >= highestSequenceId || highestSequenceId == op.HighestSequenceId)
                {
                    // Message was persisted correctly
                    if (log.IsDebugEnabled)
                    {
                        log.Debug($"[{_topic}] [{ProducerName}] Received ack for msg {sequenceId}");
                    }
                    _pendingMessages.Dequeue();
                }
                else
                {
                    log.Warning($"[{_topic}] [{ProducerName}] Got ack for batch msg error. expecting: {op.SequenceId} - {op.HighestSequenceId} - got: {sequenceId} - {highestSequenceId} - queue-size: {_pendingMessages.Count}");
                    // Force connection closing so that messages can be re-transmitted in a new connection
                    Self.Tell(new ProducerClosed(_producerId));
                }
            }
        }
        private SentReceipt SendCommand(OpSendMsg op)
        {
            var requestId = op.SequenceId;
            var pay = new Payload(op.Cmd, requestId, "CommandMessage");
            return _broker.Ask<SentReceipt>(pay, TimeSpan.FromMilliseconds(_clientConfiguration.OperationTimeoutMs)).Result;
        }
        private bool PopulateMessageSchema(Message msg)
        {
            if (msg.Schema == _topicSchema.Schema)
            {
                msg.Metadata.SchemaVersion = _topicSchema.Version;
                msg.SetSchemaState(Message.SchemaState.Ready);
                return true;
            }
            if (!IsMultiSchemaEnabled())
            {
                var e = new PulsarClientException.InvalidMessageException($"The producer '{ProducerName}' of the topic '{_topic}' is disabled the `MultiSchema`");
                Context.System.Log.Error(e.ToString());
                return false;
            }
            var schemaHash = SchemaHash.Of(msg.Schema);
            var schemaVersion = _schemaCache[schemaHash];
            if (schemaVersion != null)
            {
                msg.Metadata.SchemaVersion = schemaVersion;
                msg.SetSchemaState(Message.SchemaState.Ready);
            }
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
        private Commands.ChecksumType ChecksumType => Commands.ChecksumType.Crc32C;

        private bool HasSequenceId(ulong seq)
        {
            if (seq > 0)
                return true;
            return false;
        }
        private bool HasPublishTime(ulong seq)
        {
            if (seq > 0)
                return true;
            return false;
        }
        private bool HasEventTime(ulong seq)
        {
            if (seq > 0)
                return true;
            return false;
        }
        public override string ToString()
        {
            return "Producer{" + "topic='" + _topic + '\'' + '}';
        }
        public IStash Stash { get; set; }
    }
    public class AddPublicKeyCipher
    {

    }
    public class RecreateProducer
    {

    }

    public sealed class TopicSchema
    {
        public byte[] Version { get; set; }
        public ISchema Schema { get; set; }
        public bool Ready { get; set; }
    }
}
