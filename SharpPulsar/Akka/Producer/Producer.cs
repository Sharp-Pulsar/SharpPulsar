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

        private readonly IDictionary<string, string> _metadata;
        private readonly Dictionary<string, ISchema> _schemas;
        private readonly ClientConfigurationData _clientConfiguration;
        private readonly List<IProducerInterceptor> _producerInterceptor;
        private readonly Dictionary<long, Payload> _pendingLookupRequests = new Dictionary<long, Payload>();
        private readonly Dictionary<SchemaHash, byte[]> _schemaCache = new Dictionary<SchemaHash, byte[]>();
        private readonly Dictionary<long, Message> _pendingSchemaMessages = new Dictionary<long, Message>();
        private readonly bool _isPartitioned;
        private readonly IActorRef _parent;
        private ICancelable _producerRecreator;

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
            if (configuration.InitialSequenceId != null)
            {
                var initialSequenceId = (long)configuration.InitialSequenceId;
                _sequenceId = initialSequenceId;
            }
            else
            {
                _sequenceId = -1L;
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
                if(_producerRecreator != null) 
                {
                    _producerRecreator.Cancel();
                    _producerRecreator = null;
                }
                _pendingLookupRequests.Remove(p.RequestId);
                if (string.IsNullOrWhiteSpace(ProducerName))
                    ProducerName = p.Name;
                _sequenceId = p.LastSequenceId  == -1 ? 0 : p.LastSequenceId; //brokerDeduplicationEnabled must be enabled
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
                    _configuration.ProducerEventListener.ProducerCreated(new CreatedProducer(Self, _topic, ProducerName));
                }
                var receiptActor = Context.ActorOf(ReceiptActor.Prop(_listener, _partitionIndex));
                _broker.Tell(receiptActor);
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
                if (metadata.DeliverAtTime > 0)
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
                     _sequenceId += 1;
                    metadata.SequenceId = (ulong)_sequenceId;
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

            catch (Exception ex)
            {
                Context.System.Log.Error($"[{_topic}] [{ ProducerName}] error while closing out batch -- {ex}");
                _listener.Log(ex.ToString());
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
                Context.System.Log.Warning($"[{_topic}] [{ProducerName}] Failed to encrypt message '{e.Message}'. Proceeding with publishing unencrypted message");
                return compressedPayload;
            }
            return encryptedPayload;
        }
        private Commands.ChecksumType ChecksumType => Commands.ChecksumType.Crc32C;

        public override string ToString()
        {
            return "Producer{" + "topic='" + _topic + '\'' + '}';
        }
        public IStash Stash { get; set; }
    }
}
