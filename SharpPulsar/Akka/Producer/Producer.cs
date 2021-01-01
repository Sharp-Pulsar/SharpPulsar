using Akka.Actor;
using Org.BouncyCastle.Crypto;
using SharpPulsar.Akka.InternalCommands;
using SharpPulsar.Akka.InternalCommands.Producer;
using SharpPulsar.Akka.Network;
using SharpPulsar.Api;
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
using Akka.Event;
using DotNetty.Common.Utilities;
using IdentityModel;
using Nito.AsyncEx;
using SharpPulsar.Akka.Configuration;
using SharpPulsar.Batch.Api;
using SharpPulsar.Common.Naming;
using SharpPulsar.Impl.Crypto;
using IMessage = SharpPulsar.Api.IMessage;
using SharpPulsar.Batch;
using SharpPulsar.Stats.Producer;
using SharpPulsar.Utils;
using ProducerStatsDisabled = SharpPulsar.Impl.ProducerStatsDisabled;
using SharpPulsar.Pulsar.Api.Interceptor;

namespace SharpPulsar.Akka.Producer
{
    public class Producer : ReceiveActor, IWithUnboundedStash
    {
        private IActorRef _broker;
        private readonly List<OpSendMsg> _pendingMessages;
        private readonly IActorRef _network;
        private readonly ProducerConfigurationData _configuration;
        private readonly IProducerEventListener _listener;
        private readonly long _producerId;
        public string ProducerName;
        private readonly IBatchMessageContainerBase _batchMessageContainer;
        private readonly bool _userProvidedProducerName;
        private readonly CompressionCodec _compressor;
        private readonly IMessageCrypto _msgCrypto;
        private ConnectedServerInfo _serverInfo;
        private readonly string _topic;
        private long _sequenceId = 0;
        private readonly int _partitionIndex = -1;
        private readonly IActorRef _pulsarManager;
        private readonly IProducerStatsRecorder _stats;
        public long LastSequenceId;
        public long LastSequenceIdPushed;
        private bool _isLastSequenceIdPotentialDuplicated;

        private readonly ICancelable _regenerateDataKeyCipherCancelable;
        private ICancelable _batchMessageAndSendCancelable;

        private readonly IDictionary<string, string> _metadata;
        private readonly ClientConfigurationData _clientConfiguration;
        private readonly List<IProducerInterceptor> _producerInterceptor;
        private readonly Dictionary<SchemaHash, byte[]> _schemaCache = new Dictionary<SchemaHash, byte[]>();
        private readonly bool _isPartitioned;
        private readonly bool _isGroup;
        private ICancelable _producerRecreator;
        private readonly TopicSchema _topicSchema;
        private MultiSchemaMode _multiSchemaMode = MultiSchemaMode.Auto;

        private byte[] _schemaVersion = null;

        private bool _multiSchemaEnabled;
        private ILoggingAdapter _log;

        public Producer(ClientConfigurationData clientConfiguration, string topic, ProducerConfigurationData configuration, long producerId, IActorRef network, IActorRef pulsarManager, bool isPartitioned, bool isgroup)
        {
            _log = Context.System.Log;
            _pendingMessages = new List<OpSendMsg>(configuration.MaxPendingMessages);
            _topicSchema = new TopicSchema
            {
                Schema = configuration?.Schema,
                Ready = false,
                Version = Array.Empty<byte>()
            };
            _pulsarManager = pulsarManager;
            _topic = topic.ToLower();
            _listener = configuration.ProducerEventListener;
            _isPartitioned = isPartitioned;
            _isGroup = isgroup;
            _clientConfiguration = clientConfiguration;
            _producerInterceptor = configuration.Interceptors;
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
            if (configuration.InitialSequenceId != null)
            {
                var initialSequenceId = (long)configuration.InitialSequenceId;
                _sequenceId = initialSequenceId;
                LastSequenceId = initialSequenceId;
                LastSequenceIdPushed = initialSequenceId;
            }
            else
            {
                _sequenceId = 0L;
                LastSequenceIdPushed = -1;
                LastSequenceId = -1;
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
            if (configuration.EncryptionEnabled)
            {
                var logCtx = "[" + _topic + "] [" + ProducerName + "] [" + _producerId + "]";

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
                        msgCryptoBc = new MessageCrypto(logCtx, true, _log);
                    }
                    catch (Exception e)
                    {
                        Context.System.Log.Error("MessageCryptoBc may not included in the jar in Producer. e:", e);
                        msgCryptoBc = null;
                    }
                    _msgCrypto = msgCryptoBc;
                }
            }
            else
            {
                _msgCrypto = null;
            }
            if (_msgCrypto != null)
            {
                // Regenerate data key cipher at fixed interval
                _regenerateDataKeyCipherCancelable = Context.System.Scheduler.Advanced.ScheduleRepeatedlyCancelable(TimeSpan.FromHours(0), TimeSpan.FromHours(4), () =>
                {
                    try
                    {
                        _msgCrypto.AddPublicKeyCipher(configuration.EncryptionKeys, configuration.CryptoKeyReader);
                    }
                    catch (CryptoException e)
                    {
                        Context.System.Log.Warning($"[{_topic}] [{ProducerName}] [{_producerId}] Failed to add public key cipher.");
                        Context.System.Log.Error(e.ToString());
                    }
                } );
            }
            if (configuration.BatchingEnabled)
            {
                var containerBuilder = configuration.BatcherBuilder;
                if (containerBuilder == null)
                {
                    containerBuilder = BatcherBuilderFields.Default(Context.System);
                }
                _batchMessageContainer = (IBatchMessageContainerBase)containerBuilder.Build();
                _batchMessageContainer.Producer = Self;
                _batchMessageContainer.Container = new ProducerContainer(Self, configuration, configuration.MaxMessageSize, Context.System);
            }
            else
            {
                _batchMessageContainer = null;
            }
            if (clientConfiguration.StatsIntervalSeconds > 0)
            {
                _stats = new ProducerStatsRecorder(Context.System, ProducerName, _topic, configuration.MaxPendingMessages);
            }
            else
            {
                _stats = ProducerStatsDisabled.Instance;
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
                var uri = _configuration.UseTls ? new Uri(l.BrokerServiceUrlTls) : new Uri(l.BrokerServiceUrl);

                _broker = Context.ActorOf(_clientConfiguration.UseProxy ? ClientConnection.Prop(new Uri(_clientConfiguration.ServiceUrl), _clientConfiguration, Self, $"{uri.Host}:{uri.Port}") : ClientConnection.Prop(uri, _clientConfiguration, Self));
                Become(WaitingForConnection);
            });

            ReceiveAny(x => Stash.Stash());
        }

        private void WaitingForConnection()
        {
            Receive<ConnectedServerInfo>(s =>
            {
                var max = Math.Min(_configuration.MaxMessageSize, s.MaxMessageSize);
                _serverInfo = new ConnectedServerInfo(max, s.Protocol, s.Version, s.Name);
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
                if (_sequenceId == 0 && _configuration.InitialSequenceId == null)
                {
                    LastSequenceId = p.LastSequenceId;
                    _sequenceId = p.LastSequenceId + 1; //brokerDeduplicationEnabled must be enabled
                }
                
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

                if (_batchMessageContainer != null)
                {
                    _batchMessageContainer.Container.ProducerName = p.Name;
                    _batchMessageContainer.Container.ProducerId = _producerId;
                }
                BecomeReceive();
                ResendMessages();
                if(BatchMessagingEnabled)
                    _batchMessageAndSendCancelable = Context.System.Scheduler.Advanced.ScheduleOnceCancelable(TimeSpan.FromMilliseconds(_configuration.BatchingMaxPublishDelayMillis), BatchMessageAndSendJob);
            });
            PulsarError();
            ReceiveAny(x => Stash.Stash());
        }

        private void BatchMessageAndSendJob()
        {
            if (_log.IsDebugEnabled)
            {
                _log.Debug($"[{_topic}] [{ProducerName}] Batching the messages from the batch container from job thread");
            }

            BatchMessageAndSend();
            // schedule the next batch message task
            _batchMessageAndSendCancelable =
                Context.System.Scheduler.Advanced.ScheduleOnceCancelable(TimeSpan.FromMilliseconds(ConvertTimeUnits.ConvertMicrosecondsToMilliseconds(_configuration.BatchingMaxPublishDelayMillis)), BatchMessageAndSendJob);
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
            Become(Receive);
            Context.Watch(_broker);
            Stash.UnstashAll();
        }
        public void Receive()
        {
            Receive<INeedBroker>(x => { Sender.Tell(_broker); });
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
            Receive<ProducerClosed>(p =>
            {
                foreach (var c in Context.GetChildren())
                {
                    Context.Stop(c);
                }
                Become(RecreatingProducer);
            });
            Receive<Send>(BuildMessage);
            Receive<BulkSend>(s =>
            {
                foreach (var m in s.Messages)
                {
                    BuildMessage(m);
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
        private void BuildMessage(Send s)
        {
            try
            {
                var builder = new TypedMessageBuilder(ProducerName, _topicSchema.Schema);
                builder.Value(s.Message);
                builder.LoadConf(s.Config);
                builder.Topic(_topic);
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

                var msg = (Message)builder.Message;
                if (_topicSchema.Ready)
                    msg.SetSchemaState(Message.SchemaState.Ready);

                InternalSend(msg);
            }
            catch (Exception e)
            {
                _listener.Log(e.ToString());
            }
        }
        private void InternalSend(IMessage message)
        {
            var interceptorMessage = (Message)BeforeSend(message);
            if (_producerInterceptor != null)
            {
                _ = interceptorMessage.Properties;
            }
            Send(interceptorMessage);
        }
        private void Send(Message message)
        {
            if (!CanEnqueueRequest(message.SequenceId))
            {
                return;
            }
            var msgMetadata = message.Metadata;
            var payload = message.Payload;

            // If compression is enabled, we are compressing, otherwise it will simply use the same buffer
            var uncompressedSize = payload.Length;
            var compressedPayload = payload;
            // Batch will be compressed when closed
            // If a message has a delayed delivery time, we'll always send it individually
            if (!BatchMessagingEnabled || msgMetadata.ShouldSerializeDeliverAtTime())
            {
                compressedPayload = _compressor.Encode(payload);
                // validate msg-Size (For batching this will be check at the batch completion Size)
                var compressedSize = compressedPayload.Length;
                if (compressedSize > _serverInfo.MaxMessageSize && !_configuration.ChunkingEnabled)
                {
                    var compressedStr = (!BatchMessagingEnabled && (int)_configuration.CompressionType != 0) ? "Compressed" : "";
                    var invalidMessageException = new PulsarClientException.InvalidMessageException($"The producer {ProducerName} of the topic {_topic} sends a {compressedStr} message with {compressedSize} bytes that exceeds {_serverInfo.MaxMessageSize} bytes");
                    SendComplete(message, DateTimeHelper.CurrentUnixTimeMillis(), invalidMessageException);
                    return;
                }
            }

            if (!message.Replicated && !string.IsNullOrWhiteSpace(msgMetadata.ProducerName))
            {
                var invalidMessageException = new PulsarClientException.InvalidMessageException($"The producer {ProducerName} of the topic {_topic} can not reuse the same message");
                SendComplete(message, DateTimeHelper.CurrentUnixTimeMillis(), invalidMessageException);
                return;
            }

            if (!PopulateMessageSchema(message))
            {
                //compressedPayload = null;
                return;
            }

            // send in chunks
            var totalChunks = CanAddToBatch(message) ? 1 : Math.Max(1, compressedPayload.Length) / _serverInfo.MaxMessageSize + (Math.Max(1, compressedPayload.Length) % _serverInfo.MaxMessageSize == 0 ? 0 : 1);
            // chunked message also sent individually so, try to acquire send-permits
            for (var i = 0; i < (totalChunks - 1); i++)
            {
                if (!CanEnqueueRequest(message.SequenceId))
                {
                    return;
                }
            }

            try
            {
                var readStartIndex = 0;
                var uuid = Guid.NewGuid().ToString();
                for (var chunkId = 0; chunkId < totalChunks; chunkId++)
                {
                    var mt = msgMetadata;
                    SerializeAndSendMessage(message, mt, payload, uuid, chunkId, totalChunks, readStartIndex, _serverInfo.MaxMessageSize, compressedPayload, compressedPayload.Length, uncompressedSize);
                    readStartIndex = ((chunkId + 1) * _serverInfo.MaxMessageSize);
                }
            }
            catch (PulsarClientException e)
            {
                SendComplete(message, DateTimeHelper.CurrentUnixTimeMillis(), e);
            }
            catch (Exception t)
            {
                SendComplete(message, DateTimeHelper.CurrentUnixTimeMillis(), t);
            }
        }
        private void SerializeAndSendMessage(Message msg, MessageMetadata metadata, byte[] payload, string uuid, int chunkId, int totalChunks, int readStartIndex, int chunkMaxSizeInBytes, byte[] compressedPayload, int compressedPayloadSize, int uncompressedSize)
        {
            var chunkPayload = compressedPayload;
            var chunkMsgMetadata = metadata;
            if (totalChunks > 1 && TopicName.Get(_topic).Persistent)
            {
                chunkPayload = compressedPayload.Slice(readStartIndex, Math.Min(chunkMaxSizeInBytes, chunkPayload.Length - readStartIndex));
                // don't retain last chunk payload and builder as it will be not needed for next chunk-iteration and it will
                // be released once this chunk-message is sent
                if (chunkId != totalChunks - 1)
                {
                    chunkMsgMetadata = metadata;
                }
                chunkMsgMetadata.Uuid =  uuid;
                chunkMsgMetadata.ChunkId = chunkId;
                chunkMsgMetadata.NumChunksFromMsg = totalChunks;
                chunkMsgMetadata.TotalChunkMsgSize = compressedPayloadSize;
            }
            long sequenceId;
            if (!HasSequenceId(chunkMsgMetadata.SequenceId))
            {
                sequenceId = _sequenceId++;
                chunkMsgMetadata.SequenceId = (ulong)sequenceId;
            }
            else
            {
                sequenceId = (long)chunkMsgMetadata.SequenceId;
            }
            if (!HasPublishTime(chunkMsgMetadata.PublishTime))
            {
                chunkMsgMetadata.PublishTime = (ulong)_clientConfiguration.Clock.ToEpochTime();

                if (!string.IsNullOrWhiteSpace(chunkMsgMetadata.ProducerName))
                {
                    _log.Warning($"changing producer name from '{chunkMsgMetadata.ProducerName}' to ''. Just helping out ;)");
                    chunkMsgMetadata.ProducerName = string.Empty;
                }

                chunkMsgMetadata.ProducerName = ProducerName;

                if ((int)_configuration.CompressionType != 0)
                {
                    chunkMsgMetadata.Compression = CompressionCodecProvider.ConvertToWireProtocol(_configuration.CompressionType);
                }
                chunkMsgMetadata.UncompressedSize = (uint)uncompressedSize;
            }

            var canAddToBatch = CanAddToBatch(msg);
            if (canAddToBatch && totalChunks <= 1)
            {
                if (CanAddToCurrentBatch(msg))
                {
                    // should trigger complete the batch message, new message will add to a new batch and new batch
                    // sequence id use the new message, so that broker can handle the message duplication
                    if (sequenceId <= LastSequenceIdPushed)
                    {
                        _isLastSequenceIdPotentialDuplicated = true;
                        if (sequenceId <= LastSequenceId)
                        {
                            Context.System.Log.Warning($"Message with sequence id {sequenceId} is definitely a duplicate");
                        }
                        else
                        {
                            Context.System.Log.Info($"Message with sequence id {sequenceId} might be a duplicate but cannot be determined at this time.");
                        }
                        DoBatchSendAndAdd(msg, payload);
                    }
                    else
                    {
                        // Should flush the last potential duplicated since can't combine potential duplicated messages
                        // and non-duplicated messages into a batch.
                        if (_isLastSequenceIdPotentialDuplicated)
                        {
                            DoBatchSendAndAdd(msg, payload);
                        }
                        else
                        {
                            // handle boundary cases where message being added would exceed
                            // batch Size and/or max message Size
                            var isBatchFull = _batchMessageContainer.Add(msg, (a, e) =>
                            {
                                if(a != null)
                                    LastSequenceId = (long) a;
                                if(e != null)
                                    SendComplete(msg, DateTimeHelper.CurrentUnixTimeMillis(), e);
                            });
                            if (isBatchFull)
                            {
                                BatchMessageAndSend();
                            }
                        }
                        _isLastSequenceIdPotentialDuplicated = false;
                    }
                }
                else
                {
                    DoBatchSendAndAdd(msg, payload);
                }
            }
            else
            {
                var encryptedPayload = EncryptMessage(chunkMsgMetadata, chunkPayload);

                var msgMetadata = chunkMsgMetadata;
                // When publishing during replication, we need to set the correct number of message in batch
                // This is only used in tracking the publish rate stats
                var numMessages = msg.Metadata.NumMessagesInBatch > 1 ? msg.Metadata.NumMessagesInBatch : 1;
                OpSendMsg op;
                if (msg.GetSchemaState() == Message.SchemaState.Ready)
                {
                    var cmd = SendMessage(_producerId, sequenceId, numMessages, msgMetadata, encryptedPayload);
                    op = OpSendMsg.Create(msg, cmd, sequenceId);
                }
                else
                {
                    op = OpSendMsg.Create(msg, null, sequenceId);
                    op.Cmd = SendMessage(_producerId, sequenceId, numMessages, metadata, encryptedPayload);
                }
                op.NumMessagesInBatch = numMessages;
                op.BatchSizeByte = encryptedPayload.Length;
                if (totalChunks > 1)
                {
                    op.TotalChunks = totalChunks;
                    op.ChunkId = chunkId;
                }
                ProcessOpSendMsg(op);
            }
        }
        private void BatchMessageAndSend()
        {
            if (_log.IsDebugEnabled)
            {
                _log.Debug($"[{_topic}] [{ProducerName}] Batching the messages from the batch container with {_batchMessageContainer.NumMessagesInBatch} messages");
            }
            if (!_batchMessageContainer.Empty)
            {
                try
                {
                    IList<OpSendMsg> opSendMsgs;
                    if (_batchMessageContainer.MultiBatches)
                    {
                        opSendMsgs = _batchMessageContainer.CreateOpSendMsgs();
                    }
                    else
                    {
                        opSendMsgs = new List<OpSendMsg>{ _batchMessageContainer.CreateOpSendMsg() };
                    }
                    _batchMessageContainer.Clear();
                    foreach (var opSendMsg in opSendMsgs)
                    {
                        ProcessOpSendMsg(opSendMsg);
                    }
                }
                catch (Exception t)
                {
                    _log.Warning($"[{_topic}] [{ProducerName}] error while create opSendMsg by batch message container", t);
                }
            }
        }

        private void RecoverProcessOpSendMsgFrom(Message from)
        {
            var pendingMessages = _pendingMessages.ToList();
            OpSendMsg pendingRegisteringOp = null;
            foreach (var op in pendingMessages)
            {
                if (from != null)
                {
                    if (op?.Msg == from)
                    {
                        from = null;
                    }
                    else
                    {
                        continue;
                    }
                    if (op?.Msg != null)
                    {
                        if (op.Msg.GetSchemaState() == Message.SchemaState.None)
                        {
                            if (!RePopulateMessageSchema(op.Msg))
                            {
                                pendingRegisteringOp = op;
                                break;
                            }
                        }
                        else if (op.Msg.GetSchemaState() == Message.SchemaState.Broken)
                        {
                            _pendingMessages.Remove(op);
                            op.Recycle();
                            continue;
                        }
                    }
                    
                    if (_log.IsDebugEnabled)
                    {
                        _log.Debug($"[{_topic}] [{ProducerName}] Re-Sending message in sequenceId {op.SequenceId}");
                    }
                    WriteMessageToWire(op);
                }
            }
            if (pendingRegisteringOp != null)
            {
                TryRegisterSchema(pendingRegisteringOp.Msg);
            }
        }

        public virtual long DelayInMillis
        {
            get
            {
                var firstMsg = _pendingMessages.FirstOrDefault();
                if (firstMsg != null)
                {
                    return DateTimeHelper.CurrentUnixTimeMillis() - firstMsg.CreatedAt;
                }
                return 0L;
            }
        }
        private void ResendMessages()
        {
            var messagesToResend = _pendingMessages.Count;
            if (messagesToResend == 0)
            {
                if (_log.IsDebugEnabled)
                {
                    _log.Debug($"[{_topic}] [{ProducerName}] No pending messages to resend {messagesToResend}");
                }
                return;
            }
            _log.Info($"[{_topic}] [{ProducerName}] Re-Sending {messagesToResend} messages to server");
            RecoverProcessOpSendMsgFrom(null);
        }
        private bool CanAddToBatch(Message msg)
        {
            var schemaReady = msg.GetSchemaState() == Message.SchemaState.Ready;
            var hasDeliverAtTime = msg.Metadata.DeliverAtTime > 0;
            return  schemaReady && BatchMessagingEnabled &&  !hasDeliverAtTime;
        }
        private bool CanAddToCurrentBatch(Message msg)
        {
            var hasEnoughSpace = _batchMessageContainer.HaveEnoughSpace(msg);
            var isMultiSchemaEnabled = IsMultiSchemaEnabled(false);
            var hasSameSchema = _batchMessageContainer.HasSameSchema(msg);
            return hasEnoughSpace && (!isMultiSchemaEnabled || hasSameSchema);
        }

        private void DoBatchSendAndAdd(Message msg, byte[] payload)
        {
            if (Context.System.Log.IsDebugEnabled)
            {
                Context.System.Log.Debug($"[{_topic}] [{ProducerName}] Closing out batch to accommodate large message with Size {msg.Payload.Length}");
            }
            try
            {
                BatchMessageAndSend();
                _batchMessageContainer.Add(msg, (a, e) =>
                {
                    if (a != null)
                        LastSequenceId = (long)a;
                    if (e != null)
                        SendComplete(msg, DateTimeHelper.CurrentUnixTimeMillis(), e);
                });
            }
            finally
            {
                payload = null;
            }
        }

        private void SendComplete(Message interceptorMessage, long createdAt, Exception e)
        {
            try
            {
                if (e != null)
                {
                    _stats.IncrementSendFailed();
                    OnSendAcknowledgement(interceptorMessage, null, e);
                    Context.System.Log.Error(e.ToString());
                }
                else
                {
                    OnSendAcknowledgement(interceptorMessage, interceptorMessage.MessageId, null);
                    _stats.IncrementNumAcksReceived(DateTimeHelper.CurrentUnixTimeMillis() - createdAt);
                }
            }
            finally
            {
                interceptorMessage = null;
            }
            
        }
        private bool CanEnqueueRequest(long sequenceId)
        {
            return true;
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
            var request = Commands.NewLookup(_topic, _clientConfiguration.ListenerName, false, requestid);
            var load = new Payload(request, requestid, "BrokerLookUp");
            _network.Tell(load);
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
        private void ProcessOpSendMsg(OpSendMsg op)
        {
            if (op == null)
            {
                return;
            }
            try
            {
                if (op.Msg != null && BatchMessagingEnabled)
                {
                    BatchMessageAndSend();
                }
                _pendingMessages.Add(op);
                if (op.Msg != null)
                {
                    LastSequenceIdPushed = Math.Max(LastSequenceIdPushed, GetHighestSequenceId(op));
                }
                if (op.Msg != null && op.Msg.GetSchemaState() == Message.SchemaState.None)
                {
                    TryRegisterSchema(op.Msg);
                    return;
                }
                // If we do have a connection, the message is sent immediately, otherwise we'll try again once a new
                // connection is established
                WriteMessageToWire(op);
            }
            catch (Exception t)
            {
                _log.Warning($"[{_topic}] [{ProducerName}] error while closing out batch -- {t}");
                SendComplete(op.Msg, DateTimeHelper.CurrentUnixTimeMillis(),new PulsarClientException(t.ToString(), op.SequenceId));
            }
        }

        private void WriteMessageToWire(OpSendMsg op)
        {
            try
            {
                var receipt = SendCommand(op);
                OnSendAcknowledgement(op.Msg, op.Msg?.MessageId, null);
                HandleSendReceipt(receipt);
                _listener.MessageSent(receipt);
                _stats.UpdateNumMsgsSent(op.NumMessagesInBatch, op.BatchSizeByte);
                _pulsarManager.Tell(receipt);
            }
            catch (AskTimeoutException ex)
            {
                _listener.Log(ex.ToString());
                WriteMessageToWire(op);
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

            try
            {
                var requestId = Interlocked.Increment(ref IdGenerators.RequestId);
                var schemaResponse = SendGetOrCreateSchemaCommand(schemaInfo, requestId, _topic);
                var schemaHash = SchemaHash.Of(msg.Schema);
                _schemaCache[schemaHash] = schemaResponse.SchemaVersion;
                msg.Metadata.SchemaVersion = schemaResponse.SchemaVersion;
                msg.SetSchemaState(Message.SchemaState.Ready);
                _log.Warning($"[{_topic}] [{ProducerName}] GetOrCreateSchema succeed");
            }
            catch (Exception ex)
            {
                var t = PulsarClientException.Unwrap(ex);
                _log.Error($"[{_topic}] [{ProducerName}] GetOrCreateSchema error");
                if (t is PulsarClientException.IncompatibleSchemaException exception)
                {
                    msg.SetSchemaState(Message.SchemaState.Broken);
                    SendComplete(msg, DateTimeHelper.CurrentUnixTimeMillis(), exception);
                }
            }
            RecoverProcessOpSendMsgFrom(msg);
        }
        private GetOrCreateSchemaServerResponse SendGetOrCreateSchemaCommand(SchemaInfo schemaInfo, long requestId, string topic)
        {
            var request = Commands.NewGetOrCreateSchema(requestId, topic, schemaInfo);
            var payload = new Payload(request, requestId, "GetOrCreateSchema");
            var ask = _broker.Ask<GetOrCreateSchemaServerResponse>(payload, TimeSpan.FromMilliseconds(_clientConfiguration.OperationTimeoutMs));
            return SynchronizationContextSwitcher.NoContext(async () => await ask).Result; 
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
            var op = _pendingMessages.FirstOrDefault();
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
                log.Warning($"[{_topic}] [{ProducerName}] Got ack for msg. expecting: {op.SequenceId} - {op.HighestSequenceId} - got: {sequenceId} - {sequenceId} - queue-Size: {_pendingMessages.Count}");
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
                    _pendingMessages.Remove(op);
                }
                else
                {
                    log.Warning($"[{_topic}] [{ProducerName}] Got ack for batch msg error. expecting: {op.SequenceId} - {op.HighestSequenceId} - got: {sequenceId} - {highestSequenceId} - queue-Size: {_pendingMessages.Count}");
                    // Force connection closing so that messages can be re-transmitted in a new connection
                    Self.Tell(new ProducerClosed(_producerId));
                }
            }
            op.Recycle();
        }

        protected override void PostStop()
        {
            _regenerateDataKeyCipherCancelable?.Cancel();
            _batchMessageAndSendCancelable?.Cancel();
            base.PostStop();
        }

        private SentReceipt SendCommand(OpSendMsg op)
        {
            var requestId = op.SequenceId;
            var pay = new Payload(op.Cmd, requestId, "CommandMessage");
            var ask = _broker.Ask<SentReceipt>(pay, TimeSpan.FromMilliseconds(_clientConfiguration.OperationTimeoutMs));
            return SynchronizationContextSwitcher.NoContext(async () => await ask).Result;
        }
        private bool PopulateMessageSchema(Message msg)
        {
            var msgMetadata = msg.Metadata;
            if (msg.Schema == _topicSchema.Schema)
            {
                if (_schemaVersion != null)
                {
                    msgMetadata.SchemaVersion = _schemaVersion;
                    msg.SetSchemaState(Message.SchemaState.Ready);
                    return true;
                }
                
            }
            if (!IsMultiSchemaEnabled(true))
            {
                var e = new PulsarClientException.InvalidMessageException($"The producer '{ProducerName}' of the topic '{_topic}' is disabled the `MultiSchema`");
                SendComplete(msg, DateTimeHelper.CurrentUnixTimeMillis(), e);
                return false;
            }
            var schemaHash = SchemaHash.Of(msg.Schema);
            _schemaCache.TryGetValue(schemaHash, out var schemaVersion);
            if (schemaVersion != null)
            {
                msgMetadata.SchemaVersion = schemaVersion;
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
        private bool BatchMessagingEnabled => _configuration.BatchingEnabled;

        private bool IsMultiSchemaEnabled(bool autoEnable)
        {
            if (_multiSchemaMode != MultiSchemaMode.Auto)
            {
                return _multiSchemaMode == MultiSchemaMode.Enabled;
            }
            if (autoEnable)
            {
                _multiSchemaMode = MultiSchemaMode.Enabled;
                return true;
            }
            return false;
        }
        public override string ToString()
        {
            return "Producer{" + "topic='" + _topic + '\'' + '}';
        }
        public IStash Stash { get; set; }
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
