
using SharpPulsar.Shared;
using System.Text;
using SharpPulsar.Common;
using SharpPulsar.API.Schema;
using SharpPulsar.Common.Helpers;
using Google.Protobuf;
using Serializer = SharpPulsar.Common.Helpers.Serializer;
//using SharpPulsar.Shared.Buf;
using Akka.Util.Internal;
using System;
using SharpPulsar.API;
using Pulsar.Proto;
using ProducerAccessMode = Pulsar.Proto.ProducerAccessMode;
using DotNetty.Common;
using DotNetty.Buffers;
using System.Collections;
using SharpPulsar.Common.Util;
using Akka.Actor.Dsl;
using SharpPulsar.Common.Schema;
using DotNetty.Codecs.Base64;
using System.Text.Json;
using AuthData = SharpPulsar.Shared.AuthData;


/// <summary>
/// Licensed to the Apache Software Foundation (ASF) under one
/// or more contributor license agreements.  See the NOTICE file
/// distributed with this work for additional information
/// regarding copyright ownership.  The ASF licenses this file
/// to you under the Apache License, Version 2.0 (the
/// "License"); you may not use this file except in compliance
/// with the License.  You may obtain a copy of the License at
/// 
///   http://www.apache.org/licenses/LICENSE-2.0
/// 
/// Unless required by applicable law or agreed to in writing,
/// software distributed under the License is distributed on an
/// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
/// KIND, either express or implied.  See the License for the
/// specific language governing permissions and limitations
/// under the License.
/// </summary>
namespace SharpPulsar.Protocol.Schema
{
    public class Commands
	{

		// default message Size for transfer
		public static int DefaultMaxMessageSize = 5 * 1024 * 1024;
        public static int MessageSizeFramePadding = 10 * 1024;
		public static int InvalidMaxMessageSize = -1;
        // this present broker version don't have consumerEpoch feature,
        // so client don't need to think about consumerEpoch feature
        public static long DefaultConsumerEpoch = -1L;
        public static short MagicCrc32c = 0x0e01;
        public static short MagicBrokerEntryMetadata = 0x0e02;
        private static int ChecksumSize = 4;

        // Return the last ProtocolVersion enum value
        private static readonly int CURRENT_PROTOCOL_VERSION = Enum.GetValues(typeof(ProtocolVersion)).Cast<int>().ToList()[Enum.GetValues(typeof(ProtocolVersion)).Length - 1];
        internal static readonly FastThreadLocal<BaseCommand> LOCAL_BASE_COMMAND = new ThreadLocalBaseCommand();

        private class ThreadLocalBaseCommand : FastThreadLocal<BaseCommand>
        {
            protected internal BaseCommand InitialValue()
            {
                return new BaseCommand();
            }
        }

        private static BaseCommand LocalCmd(BaseCommand.Types.Type type)
        {
            LOCAL_BASE_COMMAND.Value.ClearType();
            LOCAL_BASE_COMMAND.Value.Type = type;
            return LOCAL_BASE_COMMAND.Value;
        }

        private static readonly FastThreadLocal<SingleMessageMetadata> LOCAL_SINGLE_MESSAGE_METADATA = new ThreadLocalSingleMessageMetadata();

        private class ThreadLocalSingleMessageMetadata : FastThreadLocal<SingleMessageMetadata>
        {
            
            protected internal SingleMessageMetadata InitialValue()
            {
                return new SingleMessageMetadata();
            }
        }

        private static readonly FastThreadLocal<MessageMetadata> LOCAL_MESSAGE_METADATA = new ThreadLocalMessageMetadata();

        private class ThreadLocalMessageMetadata : FastThreadLocal<MessageMetadata>
        {
            protected internal MessageMetadata InitialValue()
            {
                return new MessageMetadata();
            }
        }

        private static readonly FastThreadLocal<BrokerEntryMetadata> BROKER_ENTRY_METADATA = new ThreadLocalBrokerEntryMetadata();

        private class ThreadLocalBrokerEntryMetadata : FastThreadLocal<BrokerEntryMetadata>
        {
            protected internal BrokerEntryMetadata InitialValue()
            {
                return new BrokerEntryMetadata();
            }
        }

        private static void SetFeatureFlags(FeatureFlags flags)
        {
            flags.SupportsAuthRefresh = true;
            flags.SupportsBrokerEntryMetadata = true;
            flags.SupportsPartialProducer = true;
            flags.SupportsGetPartitionedMetadataWithoutAutoCreation = true;
            flags.SupportsReplDedupByLidAndEid = true;
        }
        
		public static bool HasChecksum(AbstractByteBuffer buffer)
        {
            return buffer.GetShort(buffer.ReaderIndex) == MagicCrc32c;
        }
                       
        public static AbstractByteBuffer NewConnect(string authMethodName, string authData, string libVersion)
        {
            return NewConnect(authMethodName, authData, CurrentProtocolVersion, libVersion, null, null, null, null);
        }

        public static AbstractByteBuffer NewConnect(string authMethodName, string authData, string libVersion, string targetBroker)
        {
            return NewConnect(authMethodName, authData, CurrentProtocolVersion, libVersion, targetBroker, null, null, null);
        }

        public static AbstractByteBuffer NewConnect(string authMethodName, string authData, string libVersion, string targetBroker, string originalPrincipal, string clientAuthData, string clientAuthMethod)
        {
            return NewConnect(authMethodName, authData, CurrentProtocolVersion, libVersion, targetBroker, originalPrincipal, clientAuthData, clientAuthMethod);
        }

        public static AbstractByteBuffer NewConnect(string authMethodName, string authData, int protocolVersion, string libVersion, string targetBroker, string originalPrincipal, string originalAuthData, string originalAuthMethod)
        {
            BaseCommand cmd = LocalCmd(BaseCommand.Types.Type.Connect);
            cmd.Connect.ClientVersion = (!string.ReferenceEquals(libVersion, null) ? libVersion : "Pulsar Client");
            cmd.Connect.AuthMethodName = (authMethodName);
            CommandConnect connect = cmd.Connect;

            if ("ycav1".Equals(authMethodName))
            {
                // Handle the case of a client that gets updated before the broker and starts sending the string auth method
                // name. An example would be in broker-to-broker replication. We need to make sure the clients are still
                // passing both the enum and the string until all brokers are upgraded.
                connect.AuthMethod = (AuthMethod.YcaV1);
            }

            if (!string.ReferenceEquals(targetBroker, null))
            {
                // When connecting through a proxy, we need to specify which broker do we want to be proxied through
                connect.ProxyToBrokerUrl = (targetBroker);
            }

            if (!string.ReferenceEquals(authData, null))
            {
                connect.AuthData = Bytes(authData.GetBytes());
            }

            if (!string.ReferenceEquals(originalPrincipal, null))
            {
                connect.OriginalPrincipal =(originalPrincipal);
            }

            if (!string.ReferenceEquals(originalAuthData, null))
            {
                connect.OriginalAuthData = (originalAuthData);
            }

            if (!string.ReferenceEquals(originalAuthMethod, null))
            {
                connect.OriginalAuthMethod = (originalAuthMethod);
            }
            connect.ProtocolVersion = (protocolVersion);

            SetFeatureFlags(connect.FeatureFlags);
            return SerializeWithSize(cmd);
        }

        public static AbstractByteBuffer NewConnect(string authMethodName, AuthData authData, int protocolVersion, string libVersion, string targetBroker, string originalPrincipal, AuthData originalAuthData, string originalAuthMethod)
        {
            return NewConnect(authMethodName, authData, protocolVersion, libVersion, targetBroker, originalPrincipal, originalAuthData, originalAuthMethod, null, null);
        }

        public static AbstractByteBuffer NewConnect(string authMethodName, AuthData authData, int protocolVersion, string libVersion, string targetBroker, string originalPrincipal, AuthData originalAuthData, string originalAuthMethod, string proxyVersion, FeatureFlags featureFlags)
        {
            BaseCommand cmd = NewConnectWithoutSerialize(authMethodName, authData, protocolVersion, libVersion, targetBroker, originalPrincipal, originalAuthData, originalAuthMethod, proxyVersion, featureFlags);
            return SerializeWithSize(cmd);
        }

        public static BaseCommand NewConnectWithoutSerialize(string authMethodName, AuthData authData, int protocolVersion, string libVersion, string targetBroker, string originalPrincipal, AuthData originalAuthData, string originalAuthMethod, string proxyVersion, FeatureFlags featureFlags)
        {
            BaseCommand cmd = LocalCmd(BaseCommand.Types.Type.Connect);
            cmd.Connect.ClientVersion = (!string.ReferenceEquals(libVersion, null) ? libVersion : "Pulsar Client");
            cmd.Connect.AuthMethodName = (authMethodName);
            CommandConnect connect = cmd.Connect;

            if (!string.ReferenceEquals(proxyVersion, null))
            {
                connect.ProxyVersion = (proxyVersion);
            }

            if (!string.ReferenceEquals(targetBroker, null))
            {
                // When connecting through a proxy, we need to specify which broker do we want to be proxied through
                connect.ProxyToBrokerUrl = targetBroker;
            }

            if (authData != null)
            {
                connect.AuthData = Bytes(authData.Bytes);
            }

            if (!string.ReferenceEquals(originalPrincipal, null))
            {
                connect.OriginalPrincipal = (originalPrincipal);
            }

            if (originalAuthData != null)
            {
                connect.OriginalAuthData = Encoding.UTF8.GetString(originalAuthData.Bytes);
            }

            if (!string.ReferenceEquals(originalAuthMethod, null))
            {
                connect.OriginalAuthMethod = (originalAuthMethod);
            }
            connect.ProtocolVersion = (protocolVersion);
            if (featureFlags != null)
            {
                connect.FeatureFlags = (featureFlags);
            }
            else
            {
                SetFeatureFlags(connect.FeatureFlags);
            }

            return cmd;
        }

        public static AbstractByteBuffer NewConnected(int clientProtocoVersion, bool supportsTopicWatchers)
        {
            return NewConnected(clientProtocoVersion, InvalidMaxMessageSize, supportsTopicWatchers);
        }

        public static BaseCommand NewConnectedCommand(int clientProtocolVersion, int maxMessageSize, bool supportsTopicWatchers)
        {
            BaseCommand cmd = LocalCmd(BaseCommand.Types.Type.Connected);
            cmd.Connected.ServerVersion = ("Pulsar Server" + clientProtocolVersion);
            CommandConnected connected = cmd.Connected;

            if (InvalidMaxMessageSize != maxMessageSize)
            {
                connected.MaxMessageSize = (maxMessageSize);
            }

            // If the broker supports a newer version of the protocol, it will anyway advertise the max version that the
            // client supports, to avoid confusing the client.
            int currentProtocolVersion = CurrentProtocolVersion;
            int versionToAdvertise = Math.Min(currentProtocolVersion, clientProtocolVersion);

            connected.ProtocolVersion = (versionToAdvertise);

            connected.FeatureFlags.SupportsTopicWatchers = (supportsTopicWatchers);
            connected.FeatureFlags.SupportsGetPartitionedMetadataWithoutAutoCreation = (true);
            connected.FeatureFlags.SupportsReplDedupByLidAndEid = (true);
            return cmd;
        }

        public static AbstractByteBuffer NewConnected(int clientProtocolVersion, int maxMessageSize, bool supportsTopicWatchers)
        {
            return SerializeWithSize(NewConnectedCommand(clientProtocolVersion, maxMessageSize, supportsTopicWatchers));
        }

        public static AbstractByteBuffer NewAuthChallenge(string authMethod, AuthData brokerData, int clientProtocolVersion)
        {
            BaseCommand cmd = LocalCmd(BaseCommand.Types.Type.AuthChallenge);
            CommandAuthChallenge challenge = cmd.AuthChallenge;

            // If the broker supports a newer version of the protocol, it will anyway advertise the max version that the
            // client supports, to avoid confusing the client.
            int currentProtocolVersion = CurrentProtocolVersion;
            int versionToAdvertise = Math.Min(currentProtocolVersion, clientProtocolVersion);

            challenge.ProtocolVersion = (versionToAdvertise);
            challenge.Challenge.AuthData_ = Bytes(brokerData != null ? brokerData.Bytes : new byte[0]);
            challenge.Challenge.AuthMethodName = (authMethod);
            return SerializeWithSize(cmd);
        }


        public static BaseCommand NewSuccessCommand(long requestId)
        {
            BaseCommand cmd = LocalCmd(BaseCommand.Types.Type.Success);
            cmd.Success.RequestId = (ulong)(requestId);
            return cmd;
        }

        public static AbstractByteBuffer NewSuccess(long requestId)
        {
            return SerializeWithSize(NewSuccessCommand(requestId));
        }

        public static BaseCommand NewProducerSuccessCommand(long requestId, string producerName, ISchemaVersion schemaVersion)
        {
            return NewProducerSuccessCommand(requestId, producerName, -1, schemaVersion, null, true);
        }

        public static AbstractByteBuffer newProducerSuccess(long requestId, string producerName, ISchemaVersion schemaVersion)
        {
            return NewProducerSuccess(requestId, producerName, -1, schemaVersion, null, true);
        }

        public static BaseCommand NewProducerSuccessCommand(long requestId, string producerName, long lastSequenceId, ISchemaVersion schemaVersion, long? topicEpoch, bool isProducerReady)
        {
            BaseCommand cmd = LocalCmd(BaseCommand.Types.Type.ProducerSuccess);
            cmd.ProducerSuccess.RequestId = (ulong)(requestId);
            cmd.ProducerSuccess.ProducerName = (producerName);
            cmd.ProducerSuccess.LastSequenceId = (lastSequenceId);
            cmd.ProducerSuccess.SchemaVersion = Bytes(schemaVersion.Bytes());
            cmd.ProducerSuccess.ProducerReady = (isProducerReady);
            CommandProducerSuccess ps = cmd.ProducerSuccess;
            if(topicEpoch != null)
                  ps.TopicEpoch = (ulong)topicEpoch;
            return cmd;
        }

        public static AbstractByteBuffer NewProducerSuccess(long requestId, string producerName, long lastSequenceId, ISchemaVersion schemaVersion, long? topicEpoch, bool isProducerReady)
        {
            return SerializeWithSize(NewProducerSuccessCommand(requestId, producerName, lastSequenceId, schemaVersion, topicEpoch, isProducerReady));
        }

        public static BaseCommand NewErrorCommand(long requestId, ServerError serverError, string message)
        {
            BaseCommand cmd = LocalCmd(BaseCommand.Types.Type.Error);
            cmd.Error.RequestId = (ulong)(requestId);
            cmd.Error.Error = (serverError);
            cmd.Error.Message = (!string.ReferenceEquals(message, null) ? message : "");
            return cmd;
        }

        public static AbstractByteBuffer NewError(long requestId, ServerError serverError, string message)
        {
            return SerializeWithSize(NewErrorCommand(requestId, serverError, message));
        }

        public static BaseCommand NewSendReceiptCommand(long producerId, long sequenceId, long highestId, long ledgerId, long entryId)
        {
            BaseCommand cmd = LocalCmd(BaseCommand.Types.Type.SendReceipt);
            cmd.SendReceipt.ProducerId = (ulong)(producerId);
            cmd.SendReceipt.SequenceId = (ulong)(sequenceId);
            cmd.SendReceipt.HighestSequenceId = (ulong)(highestId);
            cmd.SendReceipt.MessageId.LedgerId = (ulong)(ledgerId);
            cmd.SendReceipt.MessageId.EntryId = (ulong)(entryId);
            return cmd;
        }

        public static AbstractByteBuffer NewSendReceipt(long producerId, long sequenceId, long highestId, long ledgerId, long entryId)
        {
            return SerializeWithSize(NewSendReceiptCommand(producerId, sequenceId, highestId, ledgerId, entryId));
        }

        public static BaseCommand NewSendErrorCommand(long producerId, long sequenceId, ServerError error, string errorMsg)
        {
            BaseCommand cmd = LocalCmd(BaseCommand.Types.Type.SendError);
            cmd.SendError.ProducerId = (ulong)(producerId);
            cmd.SendError.SequenceId = (ulong)(sequenceId);
            cmd.SendError.Error = (error);
            cmd.SendError.Message = (!string.ReferenceEquals(errorMsg, null) ? errorMsg : "");
            return cmd;
        }

        public static AbstractByteBuffer NewSendError(long producerId, long sequenceId, ServerError error, string errorMsg)
        {
            return SerializeWithSize(NewSendErrorCommand(producerId, sequenceId, error, errorMsg));
        }

        /// <summary>
        /// Read the checksum and advance the reader index in the buffer.
        /// 
        /// <para>Note: This method assume the checksum presence was already verified before.
        /// </para>
        /// </summary>
        public static int ReadChecksum(AbstractByteBuffer buffer)
        {
            buffer.SkipBytes(2); //skip magic bytes
            return buffer.ReadInt();
        }

        public static void SkipChecksumIfPresent(AbstractByteBuffer buffer)
        {
            if (HasChecksum(buffer))
            {
                buffer.SkipBytes((ISchema<short>.Bytes). + ISchema<int>.Bytes);
            }
        }

        public static MessageMetadata ParseMessageMetadata(AbstractByteBuffer buffer)
        {
            MessageMetadata md = LOCAL_MESSAGE_METADATA.Value;
            ParseMessageMetadata(buffer, md);
            return md;
        }

        public static void ParseMessageMetadata(AbstractByteBuffer buffer, MessageMetadata msgMetadata)
        {
            // initially reader-index may point to start of broker entry metadata :
            // increment reader-index to start_of_headAndPayload to parse metadata
            SkipBrokerEntryMetadataIfExist(buffer);
            // initially reader-index may point to start_of_checksum : increment reader-index to start_of_metadata
            // to parse metadata
            SkipChecksumIfPresent(buffer);
            int metadataSize = (int)buffer.ReadUnsignedInt();

            var m = MessageMetadata.Parser.ParseFrom(buffer.Array);
            m.UncompressedSize = (uint)metadataSize;
            msgMetadata.MergeFrom(m);
        }

        public static void SkipMessageMetadata(AbstractByteBuffer buffer)
        {
            // initially reader-index may point to start_of_checksum : increment reader-index to start_of_metadata to parse
            // metadata
            SkipBrokerEntryMetadataIfExist(buffer);
            SkipChecksumIfPresent(buffer);
            int metadataSize = (int)buffer.ReadUnsignedInt();
            buffer.SkipBytes(metadataSize);
        }

        /// <summary>
        /// Gets the entry timestamp from either broker metadata broker timestamp or the message metadata publish time.
        /// Prefer using Managed Ledger's Entry's getEntryTimestamp() method over this method. </summary>
        /// <param name="headersAndPayloadWithBrokerEntryMetadata"> headers and payload for the message </param>
        /// <returns> the entry timestamp </returns>
        public static long GetEntryTimestamp(AbstractByteBuffer headersAndPayloadWithBrokerEntryMetadata)
        {
            // get broker timestamp first if BrokerEntryMetadata is enabled with AppendBrokerTimestampMetadataInterceptor
            return PeekBrokerEntryMetadataToLong(headersAndPayloadWithBrokerEntryMetadata, brokerEntryMetadata =>
            {
                if (brokerEntryMetadata != null && brokerEntryMetadata.HasBrokerTimestamp)
                {
                    return (long)brokerEntryMetadata.BrokerTimestamp;
                }
                // otherwise get the publish_time
                return (long)ParseMessageMetadata(headersAndPayloadWithBrokerEntryMetadata).PublishTime;
            });
        }

        public static BaseCommand NewMessageCommand(long consumerId, long ledgerId, long entryId, int partition, int redeliveryCount, long[] ackSet, long consumerEpoch)
        {
            BaseCommand cmd = LocalCmd(BaseCommand.Types.Type.Message);
            cmd.Message.ConsumerId = (ulong)(consumerId);
            CommandMessage msg = cmd.Message;
            msg.MessageId.LedgerId = (ulong)(ledgerId);
            msg.MessageId.EntryId = (ulong)(entryId);
            msg.MessageId.Partition = (partition);

            // consumerEpoch > -1 is useful
            if (consumerEpoch > DefaultConsumerEpoch)
            {
                msg.ConsumerEpoch = (ulong)(consumerEpoch);
            }
            if (redeliveryCount > 0)
            {
                msg.RedeliveryCount = (uint)(redeliveryCount);
            }
            if (ackSet != null)
            {
                for (int i = 0; i < ackSet.Length; i++)
                {
                    msg.AckSet.Add(ackSet[i]);
                }
            }
            return cmd;
        }

        public static ByteBufPair NewMessage(long consumerId, long ledgerId, long entryId, int partition, int redeliveryCount, AbstractByteBuffer metadataAndPayload, long[] ackSet)
        {
            return SerializeCommandMessageWithSize(NewMessageCommand(consumerId, ledgerId, entryId, partition, redeliveryCount, ackSet, DefaultConsumerEpoch), metadataAndPayload);
        }

        public static ByteBufPair NewSend(long producerId, long sequenceId, int numMessages, ChecksumType checksumType, long ledgerId, long entryId, MessageMetadata messageMetadata, AbstractByteBuffer payload)
        {
            return NewSend(producerId, sequenceId, -1, numMessages, messageMetadata.HasTxnidLeastBits ? (long)messageMetadata.TxnidLeastBits : -1, messageMetadata.HasTxnidMostBits ? (long)messageMetadata.TxnidMostBits : -1, checksumType, ledgerId, entryId, messageMetadata, payload);
        }

        public static ByteBufPair NewSend(long producerId, long sequenceId, int numMessages, ChecksumType checksumType, MessageMetadata messageMetadata, AbstractByteBuffer payload)
        {
            return NewSend(producerId, sequenceId, -1, numMessages, messageMetadata.HasTxnidLeastBits ? (long)messageMetadata.TxnidLeastBits : -1, messageMetadata.HasTxnidMostBits ? (long)messageMetadata.TxnidMostBits : -1, checksumType, -1, -1, messageMetadata, payload);
        }

        public static ByteBufPair NewSend(long producerId, long lowestSequenceId, long highestSequenceId, int numMessages, ChecksumType checksumType, MessageMetadata messageMetadata, AbstractByteBuffer payload)
        {
            return NewSend(producerId, lowestSequenceId, highestSequenceId, numMessages, messageMetadata.HasTxnidLeastBits ? (long)messageMetadata.TxnidLeastBits : -1, messageMetadata.HasTxnidMostBits ? (long)messageMetadata.TxnidMostBits : -1, checksumType, -1, -1, messageMetadata, payload);
        }

        public static ByteBufPair NewSend(long producerId, long sequenceId, long highestSequenceId, int numMessages, long txnIdLeastBits, long txnIdMostBits, ChecksumType checksumType, long ledgerId, long entryId, MessageMetadata messageData, AbstractByteBuffer payload)
        {
            BaseCommand cmd = LocalCmd(BaseCommand.Types.Type.Send);
            cmd.Send.ProducerId = (ulong)producerId;
            cmd.Send.SequenceId = (ulong)(sequenceId);
            CommandSend send = cmd.Send;
            if (highestSequenceId >= 0)
            {
                send.HighestSequenceId = (ulong)(highestSequenceId);
            }
            if (numMessages > 1)
            {
                send.NumMessages = (numMessages);
            }
            if (txnIdLeastBits >= 0)
            {
                send.TxnidLeastBits = (ulong)(txnIdLeastBits);
            }
            if (txnIdMostBits >= 0)
            {
                send.TxnidMostBits = (ulong)(txnIdMostBits);
            }
            if (messageData.HasTotalChunkMsgSize && messageData.TotalChunkMsgSize > 1)
            {
                send.IsChunk = (true);
            }

            if (messageData.HasMarkerType)
            {
                send.Marker = (true);
            }

            if (ledgerId >= 0 && entryId >= 0)
            {
                send.MessageId.LedgerId = (ulong)(ledgerId);
                send.MessageId.EntryId = (ulong)(entryId);
            }

            return SerializeCommandSendWithSize(cmd, checksumType, messageData, payload);
        }

        public static AbstractByteBuffer NewSubscribe(string topic, string subscription, long consumerId, long requestId, CommandSubscribe.Types.SubType subType, int priorityLevel, string consumerName, long resetStartMessageBackInSeconds)
        {
            return NewSubscribe(topic, subscription, consumerId, requestId, subType, priorityLevel, consumerName, true, null, Collections.emptyMap(), false, false, CommandSubscribe.Types.InitialPosition.Earliest, resetStartMessageBackInSeconds, null, true);
        }

        public static AbstractByteBuffer NewSubscribe(string topic, string subscription, long consumerId, long requestId, CommandSubscribe.Types.SubType subType, int priorityLevel, string consumerName, bool isDurable, MessageIdData startMessageId, IDictionary<string, string> metadata, bool readCompacted, bool? isReplicated, CommandSubscribe.Types.InitialPosition subscriptionInitialPosition, long startMessageRollbackDurationInSec, SchemaInfo schemaInfo, bool createTopicIfDoesNotExist)
        {
            return NewSubscribe(topic, subscription, consumerId, requestId, subType, priorityLevel, consumerName, isDurable, startMessageId, metadata, readCompacted, isReplicated, subscriptionInitialPosition, startMessageRollbackDurationInSec, schemaInfo, createTopicIfDoesNotExist, null, Collections.emptyMap(), DefaultConsumerEpoch);
        }

        public static AbstractByteBuffer NewSubscribe(string topic, string subscription, long consumerId, long requestId, CommandSubscribe.Types.SubType subType, int priorityLevel, string consumerName, bool isDurable, MessageIdData startMessageId, IDictionary<string, string> metadata, bool readCompacted, bool? isReplicated, CommandSubscribe.Types.InitialPosition subscriptionInitialPosition, long startMessageRollbackDurationInSec, SchemaInfo schemaInfo, bool createTopicIfDoesNotExist, KeySharedPolicy keySharedPolicy, IDictionary<string, string> subscriptionProperties, long consumerEpoch)
        {
            BaseCommand cmd = LocalCmd(BaseCommand.Types.Type.Subscribe);
            cmd.Subscribe.Topic = (topic);
            cmd.Subscribe.Subscription = (subscription);
            cmd.Subscribe.SubType = (subType);
            cmd.Subscribe.ConsumerId = (ulong)(consumerId);
            cmd.Subscribe.ConsumerName = (consumerName);
            cmd.Subscribe.RequestId = (ulong)(requestId);
            cmd.Subscribe.PriorityLevel = (priorityLevel);
            cmd.Subscribe.Durable = (isDurable);
            cmd.Subscribe.ReadCompacted = (readCompacted);
            cmd.Subscribe.InitialPosition = (subscriptionInitialPosition);
            cmd.Subscribe.ForceTopicCreation = (createTopicIfDoesNotExist);
            cmd.Subscribe.ConsumerEpoch = (ulong)(consumerEpoch);
            CommandSubscribe subscribe = cmd.Subscribe;
            if (isReplicated != null)
            {
                subscribe.ReplicateSubscriptionState = ((bool)isReplicated);
            }

            if (subscriptionProperties != null && subscriptionProperties.Count > 0)
            {
                IList<KeyValue> keyValues = new List<KeyValue>();
                subscriptionProperties.ForEach(kv =>
                {
                    KeyValue keyValue = new KeyValue();
                    keyValue.Key = kv.Key;
                    keyValue.Value = kv.Value;
                    keyValues.Add(keyValue);
                });
                subscribe.SubscriptionProperties.AddRange(keyValues);
            }

            if (keySharedPolicy != null)
            {
                KeySharedMeta keySharedMeta = subscribe.KeySharedMeta;
                keySharedMeta.AllowOutOfOrderDelivery = (keySharedPolicy.AllowOutOfOrderDelivery);
                keySharedMeta.KeySharedMode = (ConvertKeySharedMode(keySharedPolicy.KeySharedMode));

                if (keySharedPolicy is KeySharedPolicy.KeySharedPolicySticky)
                {
                    IList<Shared.Range> ranges = ((KeySharedPolicy.KeySharedPolicySticky)keySharedPolicy).GetRanges;
                    foreach (Shared.Range range in ranges)
                    {
                        IntRange r = keySharedMeta.addHashRange();
                        r.setStart(range.getStart());
                        r.setEnd(range.getEnd());
                    }
                }
            }

            if (startMessageId != null)
            {
                subscribe.StartMessageId.MergeFrom(startMessageId);
            }
            if (startMessageRollbackDurationInSec > 0)
            {
                subscribe.StartMessageRollbackDurationSec = (ulong)(startMessageRollbackDurationInSec);
            }

            if (metadata.Count > 0)
            {
                metadata.SetOfKeyValuePairs().ForEach(e => 
                {
                    KeyValue k = new KeyValue();
                    k.Key = e.Key;
                    k.Value = e.Value;
                    subscribe.Metadata.Add(k);
                }); 
            }

            if (schemaInfo != null)
            {
                if (subscribe.Schema == null)
                {
                    throw new InvalidOperationException();
                }

                if (subscribe.Schema.Properties.Count > 0)
                {
                    throw new System.InvalidOperationException();
                }

                ConvertSchema(schemaInfo, subscribe.Schema);
            }

            return SerializeWithSize(cmd);
        }

        public static AbstractByteBuffer NewTcClientConnectRequest(long tcId, long requestId)
        {
            BaseCommand cmd = LocalCmd(BaseCommand.Types.Type.TcClientConnectRequest);
            cmd.TcClientConnectRequest.TcId = (ulong)(tcId);
            cmd.TcClientConnectRequest.RequestId = (ulong)(requestId);
            return SerializeWithSize(cmd);
        }

        private static Pulsar.Proto.KeySharedMode ConvertKeySharedMode(Shared.KeySharedMode mode)
        {
            switch (mode)
            {
                case Shared.KeySharedMode.AutoSplit:
                    return Pulsar.Proto.KeySharedMode.AutoSplit;
                case Shared.KeySharedMode.Sticky:
                    return Pulsar.Proto.KeySharedMode.Sticky;
                default:
                    throw new System.ArgumentException("Unexpected key shared mode: " + mode);
            }
        }

        public static AbstractByteBuffer NewUnsubscribe(long consumerId, long requestId, bool force)
        {
            BaseCommand cmd = LocalCmd(BaseCommand.Types.Type.Unsubscribe);
            cmd.Unsubscribe.ConsumerId = (ulong)(consumerId);
            cmd.Unsubscribe.RequestId = (ulong)(requestId);
            cmd.Unsubscribe.Force = (force);
            return SerializeWithSize(cmd);
        }

        public static AbstractByteBuffer NewActiveConsumerChange(long consumerId, bool isActive)
        {
            BaseCommand cmd = LocalCmd(BaseCommand.Types.Type.ActiveConsumerChange);
            cmd.ActiveConsumerChange.ConsumerId = (ulong)(consumerId);
            cmd.ActiveConsumerChange.IsActive = (isActive);
            return SerializeWithSize(cmd);
        }

        public static AbstractByteBuffer NewSeek(long consumerId, long requestId, long ledgerId, long entryId, long[] ackSet)
        {
            BaseCommand cmd = LocalCmd(BaseCommand.Types.Type.Seek);
            cmd.Seek.ConsumerId = (ulong)(consumerId);
            cmd.Seek.RequestId = (ulong)(requestId);
            CommandSeek seek = cmd.Seek;
            MessageIdData messageId = new MessageIdData();
            messageId.LedgerId = (ulong)ledgerId;
            messageId.EntryId = (ulong)entryId;
            seek.MessageId = messageId;
            for (int i = 0; i < ackSet.Length; i++)
            {
                messageId.AckSet.Add(ackSet[i]);
            }
            return SerializeWithSize(cmd);
        }

        public static AbstractByteBuffer NewSeek(long consumerId, long requestId, long timestamp)
        {
            BaseCommand cmd = LocalCmd(BaseCommand.Types.Type.Seek);
            cmd.Seek.ConsumerId = (ulong)(consumerId);
            cmd.Seek.RequestId = (ulong)(requestId);
            cmd.Seek.MessagePublishTime = (ulong)(timestamp);
            return SerializeWithSize(cmd);
        }

        public static AbstractByteBuffer NewCloseConsumer(long consumerId, long requestId, string assignedBrokerUrl, string assignedBrokerUrlTls)
        {
            BaseCommand cmd = LocalCmd(BaseCommand.Types.Type.CloseConsumer);
            cmd.CloseConsumer.ConsumerId = (ulong)(consumerId);
            cmd.CloseConsumer.RequestId = (ulong)(requestId);
            CommandCloseConsumer commandCloseConsumer = cmd.CloseConsumer;

            if (!string.ReferenceEquals(assignedBrokerUrl, null))
            {
                commandCloseConsumer.AssignedBrokerServiceUrl = (assignedBrokerUrl);
            }

            if (!string.ReferenceEquals(assignedBrokerUrlTls, null))
            {
                commandCloseConsumer.AssignedBrokerServiceUrlTls = (assignedBrokerUrlTls);
            }

            return SerializeWithSize(cmd);
        }

        public static AbstractByteBuffer NewReachedEndOfTopic(long consumerId)
        {
            BaseCommand cmd = LocalCmd(BaseCommand.Types.Type.ReachedEndOfTopic);
            cmd.ReachedEndOfTopic.ConsumerId = (ulong)(consumerId);
            return SerializeWithSize(cmd);
        }

        public static AbstractByteBuffer NewTopicMigrated(CommandTopicMigrated.Types.ResourceType type, long resourceId, string brokerUrl, string brokerUrlTls)
        {
            BaseCommand cmd = LocalCmd(BaseCommand.Types.Type.TopicMigrated);
            CommandTopicMigrated migratedCmd = cmd.TopicMigrated;
            migratedCmd.ResourceType = (type);
            migratedCmd.ResourceId = (ulong)(resourceId);
            if (!string.IsNullOrWhiteSpace(brokerUrl))
            {
                migratedCmd.BrokerServiceUrl  = (brokerUrl);
            }
            if (!string.IsNullOrWhiteSpace(brokerUrlTls))
            {
                migratedCmd.BrokerServiceUrlTls = (brokerUrlTls);
            }
            return SerializeWithSize(cmd);
        }

        public static AbstractByteBuffer NewCloseProducer(long producerId, long requestId)
        {
            return NewCloseProducer(producerId, requestId, null, null);
        }

        public static AbstractByteBuffer NewCloseProducer(long producerId, long requestId, string assignedBrokerUrl, string assignedBrokerUrlTls)
        {
            BaseCommand cmd = LocalCmd(BaseCommand.Types.Type.CloseProducer);
            cmd.CloseProducer.ProducerId = (ulong)(producerId);
            cmd.CloseProducer.RequestId = (ulong)(requestId);
            CommandCloseProducer commandCloseProducer = cmd.CloseProducer;

            if (!string.ReferenceEquals(assignedBrokerUrl, null))
            {
                commandCloseProducer.AssignedBrokerServiceUrl = (assignedBrokerUrl);
            }

            if (!string.ReferenceEquals(assignedBrokerUrlTls, null))
            {
                commandCloseProducer.AssignedBrokerServiceUrlTls = (assignedBrokerUrlTls);
            }

            return SerializeWithSize(cmd);
        }

        public static AbstractByteBuffer NewProducer(string topic, long producerId, long requestId, string producerName, IDictionary<string, string> metadata, bool isTxnEnabled)
        {
            return NewProducer(topic, producerId, requestId, producerName, false, metadata, isTxnEnabled);
        }

        public static AbstractByteBuffer NewProducer(string topic, long producerId, long requestId, string producerName, bool encrypted, IDictionary<string, string> metadata, bool isTxnEnabled)
        {
            return NewProducer(topic, producerId, requestId, producerName, encrypted, metadata, null, 0, false, ProducerAccessMode.Shared, null, isTxnEnabled);
        }

        private static Pulsar.Proto.Schema.Types.Type GetSchemaType(SchemaType type)
        {
            if (type == SchemaType.AutoConsume)
            {
                return Pulsar.Proto.Schema.Types.Type.AutoConsume;
            }
            else if (type.Value < 0)
            {
                return Pulsar.Proto.Schema.Types.Type.None;
            }
            else if (type == SchemaType.External)
            {
                // This is a special case, SchemaType.EXTERNAL number is not match the Schema.Type.EXTERNAL.
                return Pulsar.Proto.Schema.Types.Type.External;
            }
            else
            {
                return Schema.Type.valueOf(type.getValue());
            }
        }

        public static SchemaType GetSchemaType(Pulsar.Proto.Schema.Types.Type type)
        {
            if (type == Pulsar.Proto.Schema.Types.Type.AutoConsume)
            {
                return SchemaType.AutoConsume;
            }
            else if (type.getValue() < 0)
            {
                // this is unexpected
                return SchemaType.NONE;
            }
            else if (type == Pulsar.Proto.Schema.Types.Type.External)
            {
                // This is a special case, SchemaType.EXTERNAL number is not match the Schema.Type.EXTERNAL.
                return SchemaType.External;
            }
            else
            {
                return SchemaType.ValueOf(type);
            }
        }

        private static void ConvertSchema(ISchemaInfo schemaInfo, Pulsar.Proto.Schema schema)
        {
            schema.Name = (schemaInfo.Name);
            schema.SchemaData = (schemaInfo.Schema);
            schema.Type = (GetSchemaType(schemaInfo.Type));

            schemaInfo.getProperties().entrySet().ForEach(entry =>
            {
                if (entry.getKey() != null && entry.getValue() != null)
                {
                    schema.addProperty().setKey(entry.getKey()).setValue(entry.getValue());
                }
            });
        }

        public static AbstractByteBuffer NewProducer(string topic, long producerId, long requestId, string producerName, bool encrypted, IDictionary<string, string> metadata, SchemaInfo schemaInfo, long epoch, bool userProvidedProducerName, Shared.ProducerAccessMode accessMode, long? topicEpoch, bool isTxnEnabled)
        {
            return NewProducer(topic, producerId, requestId, producerName, encrypted, metadata, schemaInfo, epoch, userProvidedProducerName, accessMode, topicEpoch, isTxnEnabled, null);

        }

        public static AbstractByteBuffer NewProducer(string topic, long producerId, long requestId, string producerName, bool encrypted, IDictionary<string, string> metadata, SchemaInfo schemaInfo, long epoch, bool userProvidedProducerName, Shared.ProducerAccessMode accessMode, long? topicEpoch, bool isTxnEnabled, string initialSubscriptionName)
        {
            BaseCommand cmd = LocalCmd(BaseCommand.Types.Type.Producer);
            cmd.Producer.Topic = (topic);
            cmd.Producer.ProducerId = (ulong)(producerId);
            cmd.Producer.RequestId = (ulong)(requestId);
            cmd.Producer.Epoch = (ulong)(epoch);
            cmd.Producer.UserProvidedProducerName = (userProvidedProducerName);
            cmd.Producer.Encrypted = (encrypted);
            cmd.Producer.TxnEnabled = (isTxnEnabled);
            cmd.Producer.ProducerAccessMode = ConvertProducerAccessMode(accessMode);
            CommandProducer producer = cmd.Producer;
            if (!string.ReferenceEquals(producerName, null))
            {
                producer.ProducerName = (producerName);
            }

            if (metadata.Count > 0)
            {
                metadata.ForEach(kv => 
                {
                    var k = new KeyValue();
                    k.Key = kv.Key;
                    k.Value = kv.Value;
                    producer.Metadata.Add(k);
                 });
            }

            if (null != schemaInfo)
            {
                ConvertSchema(schemaInfo, producer.Schema);
            }
            if(topicEpoch != null)  
                producer.TopicEpoch = (ulong)topicEpoch;

            if (!string.IsNullOrWhiteSpace(initialSubscriptionName))
            {
                producer.InitialSubscriptionName = (initialSubscriptionName);
            }

            return SerializeWithSize(cmd);
        }

        public static AbstractByteBuffer NewPartitionMetadataRequest(string topic, long requestId, bool metadataAutoCreationEnabled)
        {
            BaseCommand cmd = LocalCmd(BaseCommand.Types.Type.PartitionedMetadata);
            cmd.PartitionMetadata.Topic = (topic);
            cmd.PartitionMetadata.RequestId = (ulong)(requestId);
            cmd.PartitionMetadata.MetadataAutoCreationEnabled = (metadataAutoCreationEnabled);
            return SerializeWithSize(cmd);
        }

        public static AbstractByteBuffer NewLookup(string topic, bool authoritative, long requestId)
        {
            return NewLookup(topic, null, authoritative, requestId, null);
        }

        public static AbstractByteBuffer NewLookup(string topic, string listenerName, bool authoritative, long requestId, IDictionary<string, string> properties)
        {
            BaseCommand cmd = LocalCmd(BaseCommand.Types.Type.Lookup);
            cmd.LookupTopic.Topic = (topic);
            cmd.LookupTopic.RequestId = (ulong)(requestId);
            cmd.LookupTopic.Authoritative = (authoritative);
            CommandLookupTopic lookup = cmd.LookupTopic;
            if (!string.IsNullOrWhiteSpace(listenerName))
            {
                lookup.AdvertisedListenerName = (listenerName);
            }
            if (properties != null)
            {
                properties.ForEach(kv =>
                {
                    var b = new KeyValue();
                    b.Key = kv.Key;
                    b.Value = kv.Value;
                    lookup.Properties.Add(b);
                });
            }
            return SerializeWithSize(cmd);
        }

        public static AbstractByteBuffer NewMultiTransactionMessageAck(long consumerId, TxnID txnID, IList<(long ledger, long entry, List<long> bitSet)> entries)
        {
            BaseCommand cmd = NewMultiMessageAckCommon(entries);
            cmd.Ack.ConsumerId = (ulong)(consumerId);
            cmd.Ack.AckType = (CommandAck.Types.AckType.Individual);
            cmd.Ack.TxnidLeastBits = (ulong)(txnID.LeastSigBits);
            cmd.Ack.TxnidMostBits = (ulong)(txnID.MostSigBits);
            return SerializeWithSize(cmd);
        }

        private static BaseCommand NewMultiMessageAckCommon(IList<(long ledger, long entry, List<long> bitSet)> entries)
        {
            BaseCommand cmd = LocalCmd(BaseCommand.Types.Type.Ack);
            CommandAck ack = cmd.Ack;
            int entriesCount = entries.Count;
            for (int i = 0; i < entriesCount; i++)
            {
                long ledgerId = entries[i].ledger;
                long entryId = entries[i].entry;
                List<long> bitSet = entries[i].bitSet;
                MessageIdData msgId = new MessageIdData();
                msgId.LedgerId = (ulong)ledgerId;
                msgId.EntryId = (ulong)entryId;
                ack.MessageId.Add(msgId);
                if (bitSet != null)
                {
                    long[] ackSet = bitSet.ToArray();
                    for (int j = 0; j < ackSet.Length; j++)
                    {
                        msgId.AckSet.Add(ackSet[j]);
                    }
                    //bitSet.Recycle();
                }
            }

            return cmd;
        }

        public static AbstractByteBuffer NewMultiMessageAck(long consumerId, IList<(long ledger, long entry, List<long> bitSet)> entries, long requestId)
        {
            BaseCommand cmd = NewMultiMessageAckCommon(entries);
            cmd.Ack.AckType = CommandAck.Types.AckType.Individual;
            if (requestId >= 0)
            {
                cmd.Ack.RequestId = (ulong)(requestId);
            }
            return SerializeWithSize(cmd);
        }

        public static AbstractByteBuffer NewAck(long consumerId, long ledgerId, long entryId, List<long> ackSet, CommandAck.Types.AckType ackType, CommandAck.Types.ValidationError validationError, IDictionary<string, long> properties, long requestId)
        {
            return NewAck(consumerId, ledgerId, entryId, ackSet, ackType, validationError, properties, -1L, -1L, requestId, -1);
        }

        public static AbstractByteBuffer NewAck(long consumerId, long ledgerId, long entryId, List<long> ackSet, CommandAck.Types.AckType ackType, CommandAck.Types.ValidationError validationError, IDictionary<string, long> properties, long txnIdLeastBits, long txnIdMostBits, long requestId, int batchSize)
        {
            BaseCommand cmd = LocalCmd(BaseCommand.Types.Type.Ack);
            cmd.Ack.ConsumerId = (ulong)(consumerId);
            cmd.Ack.AckType = (ackType);
            CommandAck ack = cmd.Ack;
            MessageIdData messageIdData = new MessageIdData();
            messageIdData.LedgerId = (ulong)(ledgerId); 
            messageIdData.EntryId = (ulong)(entryId);
            ack.MessageId.Add(messageIdData);
            if (ackSet != null)
            {
                long[] @as = ackSet.ToArray();
                for (int i = 0; i < @as.Length; i++)
                {
                    messageIdData.AckSet.Add(@as[i]);
                }
            }

            if (batchSize >= 0)
            {
                messageIdData.BatchSize = (batchSize);
            }

            return NewAck(validationError, properties, txnIdLeastBits, txnIdMostBits, requestId, ack, cmd);
        }

        public static AbstractByteBuffer NewAck(long consumerId, IList<MessageIdData> messageIds, CommandAck.Types.AckType ackType, CommandAck.Types.ValidationError validationError, IDictionary<string, long> properties, long txnIdLeastBits, long txnIdMostBits, long requestId)
        {
            BaseCommand cmd = LocalCmd(BaseCommand.Types.Type.Ack);
            cmd.Ack.ConsumerId = (ulong)(consumerId);
            cmd.Ack.AckType = (ackType);
            CommandAck ack = cmd.Ack;
            ack.MessageId.AddRange(messageIds);

            return NewAck(validationError, properties, txnIdLeastBits, txnIdMostBits, requestId, ack, cmd);
        }

        private static AbstractByteBuffer NewAck(CommandAck.Types.ValidationError validationError, IDictionary<string, long> properties, long txnIdLeastBits, long txnIdMostBits, long requestId, CommandAck ack, BaseCommand cmd)
        {
            if (validationError != null)
            {
                ack.ValidationError = (validationError);
            }
            if (txnIdMostBits >= 0)
            {
                ack.TxnidMostBits = (ulong)(txnIdMostBits);
            }
            if (txnIdLeastBits >= 0)
            {
                ack.TxnidLeastBits = (ulong)(txnIdLeastBits);
            }

            if (requestId >= 0)
            {
                ack.RequestId = (ulong)(requestId);
            }
            if (properties.Count > 0)
            {
                properties.ForEach(kv =>
                {
                    var b = new KeyLongValue();
                    b.Key = kv.Key;
                    b.Value = (ulong)kv.Value; 
                    ack.Properties.Add(b);
                });
            }
            return SerializeWithSize(cmd);
        }

        public static AbstractByteBuffer NewAck(long consumerId, long ledgerId, long entryId, List<long> ackSet, CommandAck.Types.AckType ackType, CommandAck.Types.ValidationError validationError, IDictionary<string, long> properties, long txnIdLeastBits, long txnIdMostBits, long requestId)
        {
            return NewAck(consumerId, ledgerId, entryId, ackSet, ackType, validationError, properties, txnIdLeastBits, txnIdMostBits, requestId, -1);
        }

        public static AbstractByteBuffer NewFlow(long consumerId, int messagePermits)
        {
            BaseCommand cmd = LocalCmd(BaseCommand.Types.Type.Flow);
            cmd.Flow.ConsumerId = (ulong)(consumerId);
            cmd.Flow.MessagePermits = (uint)messagePermits;
            return SerializeWithSize(cmd);
        }

        public static AbstractByteBuffer NewRedeliverUnacknowledgedMessages(long consumerId, long consumerEpoch)
        {
            BaseCommand cmd = LocalCmd(BaseCommand.Types.Type.RedeliverUnacknowledgedMessages);
            cmd.RedeliverUnacknowledgedMessages.ConsumerId = (ulong)(consumerId);
            cmd.RedeliverUnacknowledgedMessages.ConsumerEpoch = (ulong)(consumerEpoch);
            return SerializeWithSize(cmd);
        }

        public static AbstractByteBuffer NewRedeliverUnacknowledgedMessages(long consumerId, IList<MessageIdData> messageIds)
        {
            BaseCommand cmd = LocalCmd(BaseCommand.Types.Type.RedeliverUnacknowledgedMessages);
            cmd.RedeliverUnacknowledgedMessages.ConsumerId = (ulong)(consumerId);
            CommandRedeliverUnacknowledgedMessages req = cmd.RedeliverUnacknowledgedMessages;
            messageIds.ForEach(msgId =>
            {
                MessageIdData m = new MessageIdData();  
                m.LedgerId = msgId.LedgerId;    
                m.EntryId = msgId.EntryId;
                req.MessageIds.Add(m);
                if (msgId.HasBatchIndex)
                {
                    m.BatchIndex = msgId.BatchIndex;
                }
            });
            return SerializeWithSize(cmd);
        }

        public static AbstractByteBuffer NewGetTopicsOfNamespaceRequest(string @namespace, long requestId, CommandGetTopicsOfNamespace.Types.Mode mode, string topicsPattern, string topicsHash)
        {
            BaseCommand cmd = LocalCmd(BaseCommand.Types.Type.GetTopicsOfNamespace);
            CommandGetTopicsOfNamespace topics = cmd.GetTopicsOfNamespace;
            topics.Namespace = (@namespace);
            topics.RequestId = (ulong)(requestId);
            topics.Mode = (mode);
            if (!string.ReferenceEquals(topicsPattern, null))
            {
                topics.TopicsPattern = (topicsPattern);
            }
            if (!string.ReferenceEquals(topicsHash, null))
            {
                topics.TopicsHash = (topicsHash);
            }
            return SerializeWithSize(cmd);
        }

        private static readonly IByteBuffer cmdPing;

        static Commands()
        {
            var ping = new BaseCommand();
            ping.Type = BaseCommand.Types.Type.Ping;
            BaseCommand cmd = ping;
            AbstractByteBuffer serializedCmdPing = SerializeWithSize(cmd);
            cmdPing = Unpooled.CopiedBuffer(serializedCmdPing);
            serializedCmdPing.Release();

            var pong = new BaseCommand();
            pong.Type = BaseCommand.Types.Type.Pong;
            BaseCommand cmdP = pong;
            AbstractByteBuffer serializedCmdPong = SerializeWithSize(cmdP);
            cmdPong = Unpooled.CopiedBuffer(serializedCmdPong);
            serializedCmdPong.Release();
        }

        internal static IByteBuffer NewPing()
        {
            return cmdPing.RetainedDuplicate();
        }

        private static readonly IByteBuffer cmdPong;


        public static IByteBuffer NewPong()
        {
            return cmdPong.RetainedDuplicate();
        }

        public static AbstractByteBuffer NewGetLastMessageId(long consumerId, long requestId)
        {
            BaseCommand cmd = LocalCmd(BaseCommand.Types.Type.GetLastMessageId);
            cmd.GetLastMessageId.RequestId = (ulong)(requestId);
            cmd.GetLastMessageId.ConsumerId = (ulong)(consumerId);
            return SerializeWithSize(cmd);
        }

        public static AbstractByteBuffer NewGetSchema(long requestId, string topic, ISchemaVersion version)
        {
            BaseCommand cmd = LocalCmd(BaseCommand.Types.Type.GetSchema);
            cmd.GetSchema.RequestId = (ulong)(requestId);
            cmd.GetSchema.Topic = (topic);
            CommandGetSchema schema = cmd.GetSchema;
            if (version != null)
            {
              schema.SchemaVersion = Bytes(version.Bytes());
            }
            return SerializeWithSize(cmd);
        }

        private static ByteString Bytes(byte[] bytes)
        {
            using (var str = new MemoryStream(bytes))
                return ByteString.FromStream(str);
        }
		
        public static AbstractByteBuffer NewGetOrCreateSchema(long requestId, string topic, Common.Schema.SchemaInfo schemaInfo)
        {
            BaseCommand cmd = LocalCmd(BaseCommand.Types.Type.GetOrCreateSchema);
            cmd.GetOrCreateSchema.RequestId = (ulong)(requestId);
            cmd.GetOrCreateSchema.Topic = (topic);
            
            var schema = cmd.GetOrCreateSchema.Schema; 
            ConvertSchema(schemaInfo, schema);
            return SerializeWithSize(cmd);
        }

        // ---- transaction related ----

        public static AbstractByteBuffer NewTxn(long tcId, long requestId, long ttlSeconds)
        {
            BaseCommand cmd = LocalCmd(BaseCommand.Types.Type.NewTxn);
            cmd.NewTxn.TcId = (ulong)(tcId);
            cmd.NewTxn.RequestId = (ulong)(requestId);
            cmd.NewTxn.TxnTtlSeconds = (ulong)(ttlSeconds);
            return SerializeWithSize(cmd);
        }

        public static AbstractByteBuffer NewAddPartitionToTxn(long requestId, long txnIdLeastBits, long txnIdMostBits, IList<string> partitions)
        {
            BaseCommand cmd = LocalCmd(BaseCommand.Types.Type.AddPartitionToTxn);
            cmd.AddPartitionToTxn.RequestId = (ulong)(requestId);
            cmd.AddPartitionToTxn.TxnidLeastBits = (ulong)(txnIdLeastBits);
            cmd.AddPartitionToTxn.TxnidMostBits = (ulong)(txnIdMostBits);
            cmd.AddPartitionToTxn.Partitions.Add(partitions);
            return SerializeWithSize(cmd);
        }


        public static AbstractByteBuffer NewAddSubscriptionToTxn(long requestId, long txnIdLeastBits, long txnIdMostBits, IList<Subscription> subscriptions)
        {
            BaseCommand cmd = LocalCmd(BaseCommand.Types.Type.AddSubscriptionToTxn);
            cmd.AddSubscriptionToTxn.RequestId = (ulong)(requestId);
            cmd.AddSubscriptionToTxn.TxnidLeastBits = (ulong)(txnIdLeastBits);
            cmd.AddSubscriptionToTxn.TxnidMostBits = (ulong)(txnIdMostBits);
            cmd.AddSubscriptionToTxn.Subscription.AddRange(subscriptions);
            return SerializeWithSize(cmd);
        }

        
        public static BaseCommand NewEndTxn(long requestId, long txnIdLeastBits, long txnIdMostBits, TxnAction txnAction)
        {
            BaseCommand cmd = LocalCmd(BaseCommand.Types.Type.EndTxn);
            cmd.EndTxn.RequestId =  (ulong)requestId;
            cmd.EndTxn.TxnidLeastBits = (ulong)txnIdLeastBits;
            cmd.EndTxn.TxnidMostBits = (ulong)txnIdMostBits;
            cmd.EndTxn.TxnAction = txnAction;
            return cmd;
        }

        
        public static AbstractByteBuffer NewEndTxnOnPartition(long requestId, long txnIdLeastBits, long txnIdMostBits, string topic, TxnAction txnAction, long lowWaterMark)
        {
            BaseCommand cmd = LocalCmd(BaseCommand.Types.Type.EndTxnOnPartition);
            cmd.EndTxnOnPartition.RequestId = (ulong)requestId;
            cmd.EndTxnOnPartition.TxnidLeastBits = (ulong)txnIdLeastBits;
            cmd.EndTxnOnPartition.TxnidMostBits = (ulong)txnIdMostBits;
            cmd.EndTxnOnPartition.Topic = topic;
            cmd.EndTxnOnPartition.TxnAction = txnAction;
            cmd.EndTxnOnPartition.TxnidLeastBitsOfLowWatermark = (ulong)lowWaterMark;
            return SerializeWithSize(cmd);
        }


        public static AbstractByteBuffer NewEndTxnOnSubscription(long requestId, long txnIdLeastBits, long txnIdMostBits, string topic, string subscription, TxnAction txnAction, long lowWaterMark)
        {
            BaseCommand cmd = LocalCmd(BaseCommand.Types.Type.EndTxnOnSubscription);
            cmd.EndTxnOnSubscription.RequestId = (ulong)requestId;
            cmd.EndTxnOnSubscription.TxnidLeastBits = (ulong)txnIdLeastBits;
            cmd.EndTxnOnSubscription.TxnidMostBits = (ulong)txnIdMostBits;
            cmd.EndTxnOnSubscription.TxnAction = txnAction;
            cmd.EndTxnOnSubscription.TxnidLeastBitsOfLowWatermark = (ulong)lowWaterMark;
            cmd.EndTxnOnSubscription.Subscription.Topic = topic;
            cmd.EndTxnOnSubscription.Subscription.Subscription_ = subscription;
            return SerializeWithSize(cmd);
        }


        public static BaseCommand NewWatchTopicList(long requestId, long watcherId, string @namespace, string topicsPattern, string topicsHash)
        {
            BaseCommand cmd = LocalCmd(BaseCommand.Types.Type.WatchTopicList);
            cmd.WatchTopicList.RequestId = (ulong)requestId;
            cmd.WatchTopicList.Namespace = @namespace;
            cmd.WatchTopicList.TopicsPattern = topicsPattern;
            cmd.WatchTopicList.WatcherId = (ulong)watcherId;
            if (!string.ReferenceEquals(topicsHash, null))
            {
                cmd.WatchTopicList.TopicsHash = topicsHash;
            }
            return cmd;
        }

        /// <summary>
        ///* </summary>
        /// <param name="topics"> topic names which are matching, the topic name contains the partition suffix. </param>
        public static BaseCommand NewWatchTopicListSuccess(long requestId, long watcherId, string topicsHash, IList<string> topics)
        {
            BaseCommand cmd = LocalCmd(BaseCommand.Types.Type.WatchTopicListSuccess);
            cmd.WatchTopicListSuccess.RequestId = (ulong)requestId;
            cmd.WatchTopicListSuccess.WatcherId = (ulong)watcherId;
            if (!string.ReferenceEquals(topicsHash, null))
            {
                cmd.WatchTopicListSuccess.TopicsHash = topicsHash;
            }
            if (topics != null && topics.Count > 0)
            {
                cmd.WatchTopicListSuccess.Topic.AddRange(topics);
            }
            return cmd;
        }

        /// <param name="deletedTopics"> topic names deleted(contains the partition suffix). </param>
        /// <param name="newTopics"> topics names added(contains the partition suffix). </param>
        public static BaseCommand NewWatchTopicUpdate(long watcherId, IList<string> newTopics, IList<string> deletedTopics, string topicsHash)
        {
            BaseCommand cmd = LocalCmd(BaseCommand.Types.Type.WatchTopicUpdate);
            cmd.WatchTopicUpdate.WatcherId = (ulong)watcherId;
            cmd.WatchTopicUpdate.TopicsHash = topicsHash;
            cmd.WatchTopicUpdate.NewTopics.AddRange(newTopics);
            cmd.WatchTopicUpdate.DeletedTopics.AddRange(deletedTopics);
            return cmd;
        }

        public static BaseCommand NewWatchTopicListClose(long watcherId, long requestId)
        {
            BaseCommand cmd = LocalCmd(BaseCommand.Types.Type.WatchTopicListClose);
            cmd.WatchTopicListClose.RequestId = (ulong)requestId;
            cmd.WatchTopicListClose.WatcherId = (ulong)watcherId;
            return cmd;
        }

        public static int ComputeChecksum(AbstractByteBuffer byteBuffer)
        {
            return 9;//Crc32CIntChecksum.ComputeChecksum(byteBuffer);
        }
        public static int ResumeChecksum(int prev,   AbstractByteBuffer byteBuffer)
        {
            return 9; //Crc32CIntChecksum.ResumeChecksum(prev, byteBuffer);
        }
        public static AbstractByteBuffer SerializeWithSize(BaseCommand cmd)
        {
            // / Wire format
            // [TOTAL_SIZE] [CMD_SIZE][CMD]
            int cmdSize = cmd.CalculateSize();
            int totalSize = cmdSize + 4;
            int frameSize = totalSize + 4;

            AbstractByteBuffer buf = PulsarByteBufAllocator.DEFAULT.buffer(frameSize, frameSize);

            // Prepend 2 lengths to the buffer
            buf.WriteInt(totalSize);
            buf.WriteInt(cmdSize);
            cmd.WriteTo(new CodedOutputStream(buf.Array));
            return buf;
        }

        private static ByteBufPair SerializeCommandSendWithSize(BaseCommand cmd, ChecksumType checksumType, MessageMetadata msgMetadata, AbstractByteBuffer payload)
        {
            // / Wire format
            // [TOTAL_SIZE] [CMD_SIZE][CMD] [MAGIC_NUMBER][CHECKSUM] [METADATA_SIZE][METADATA] [PAYLOAD]

            int cmdSize = cmd.CalculateSize();
            int msgMetadataSize = msgMetadata.CalculateSize();
            int payloadSize = payload.ReadableBytes;
            int magicAndChecksumLength = ChecksumType.Crc32c.Equals(checksumType) ? (2 + 4) : 0;
            bool includeChecksum = magicAndChecksumLength > 0;
            // cmdLength + cmdSize + magicLength +
            // checksumSize + msgMetadataLength +
            // msgMetadataSize
            int headerContentSize = 4 + cmdSize + magicAndChecksumLength + 4 + msgMetadataSize;
            int totalSize = headerContentSize + payloadSize;
            int headersSize = 4 + headerContentSize; // totalSize + headerLength
            int checksumReaderIndex = -1;

            AbstractByteBuffer headers = (AbstractByteBuffer)payload.Allocator.Buffer(headersSize, headersSize);
            headers.WriteInt(totalSize); // External frame

            // Write cmd
            headers.WriteInt(cmdSize);
            cmd.WriteTo(new CodedOutputStream(headers.Array));

            // Create checksum placeholder
            if (includeChecksum)
            {
                headers.WriteShort(MagicCrc32c);
                checksumReaderIndex = headers.WriterIndex;
                headers.SetWriterIndex(headers.WriterIndex + ChecksumSize); // skip 4 bytes of checksum
            }

            // Write metadata
            headers.WriteInt(msgMetadataSize);
            msgMetadata.WriteTo(new CodedOutputStream(headers.Array));

            ByteBufPair command = ByteBufPair.Get(headers, payload);

            // write checksum at created checksum-placeholder
            if (includeChecksum)
            {
                headers.MarkReaderIndex();
                headers.SetReaderIndex(checksumReaderIndex + ChecksumSize);
                int metadataChecksum = ComputeChecksum(headers);
                int computedChecksum = ResumeChecksum(metadataChecksum, payload);
                // set computed checksum
                headers.SetInt(checksumReaderIndex, computedChecksum);
                headers.ResetReaderIndex();
            }
            return command;
        }

        public static AbstractByteBuffer AddBrokerEntryMetadata(AbstractByteBuffer headerAndPayload, ISet<BrokerEntryMetadataInterceptor> interceptors)
        {
            return AddBrokerEntryMetadata(headerAndPayload, interceptors, -1);
        }

        public static AbstractByteBuffer AddBrokerEntryMetadata(AbstractByteBuffer headerAndPayload, ISet<BrokerEntryMetadataInterceptor> brokerInterceptors, int numberOfMessages)
        {
            //   | BROKER_ENTRY_METADATA_MAGIC_NUMBER | BROKER_ENTRY_METADATA_SIZE |         BROKER_ENTRY_METADATA         |
            //   |         2 bytes                    |       4 bytes              |    BROKER_ENTRY_METADATA_SIZE bytes   |

            BrokerEntryMetadata brokerEntryMetadata = BROKER_ENTRY_METADATA.Get();
            brokerEntryMetadata.Clear();
            foreach (BrokerEntryMetadataInterceptor interceptor in brokerInterceptors)
            {
                interceptor.intercept(brokerEntryMetadata);
                if (numberOfMessages >= 0)
                {
                    interceptor.interceptWithNumberOfMessages(brokerEntryMetadata, numberOfMessages);
                }
            }

            int brokerMetaSize = brokerEntryMetadata.CalculateSize();
            AbstractByteBuffer brokerMeta = (AbstractByteBuffer)headerAndPayload.Allocator.Buffer(brokerMetaSize + 6, brokerMetaSize + 6);
            brokerMeta.WriteShort(Commands.MagicBrokerEntryMetadata);
            brokerMeta.WriteInt(brokerMetaSize);
            brokerEntryMetadata.WriteTo(new CodedOutputStream(brokerMeta.Array));

            CompositeByteBuffer compositeByteBuf = headerAndPayload.Allocator.CompositeBuffer();
            compositeByteBuf.AddComponents(true, brokerMeta, headerAndPayload);
            return compositeByteBuf;
        }

        /// <summary>
        /// Moves the readerIndex ahead skipping possible BrokerEntryMetadata if it exists in the header and payload
        /// buffer. </summary>
        /// <param name="headerAndPayload"> the header and payload buffer </param>
        /// <returns> the header and payload buffer passed as parameter </returns>
        public static AbstractByteBuffer SkipBrokerEntryMetadataIfExist(AbstractByteBuffer headerAndPayload)
        {
            int readerIndex = headerAndPayload.ReaderIndex;
            if (headerAndPayload.GetShort(readerIndex) == MagicBrokerEntryMetadata)
            {
                headerAndPayload.SkipBytes(Short.BYTES);
                int brokerEntryMetadataSize = headerAndPayload.ReadInt();
                headerAndPayload.SkipBytes(brokerEntryMetadataSize);
            }
            return headerAndPayload;
        }

        /// <summary>
        /// Parses the broker entry metadata from the header and payload buffer and returns a new BrokerEntryMetadata
        /// instance if the broker entry metadata exists in the header and payload buffer. Null is returned if the
        /// broker entry metadata does not exist in the header and payload buffer.
        /// The readerIndex of the headerAndPayload buffer is advanced.
        /// </summary>
        /// <param name="headerAndPayload"> the header and payload buffer </param>
        /// <returns> broker entry metadata or null </returns>
        public static BrokerEntryMetadata ParseBrokerEntryMetadataIfExist(AbstractByteBuffer headerAndPayload)
        {
            return ParseOrPeekBrokerEntryMetadataIfExist(headerAndPayload, null, false);
        }

        /// <summary>
        /// Parses the broker entry metadata from the header and payload buffer and returns a new BrokerEntryMetadata
        /// instance if the broker entry metadata exists in the header and payload buffer. Null is returned if the
        /// broker entry metadata does not exist in the header and payload buffer.
        /// The readerIndex of the headerAndPayload buffer is not advanced.
        /// </summary>
        /// <param name="headerAndPayload"> the header and payload buffer </param>
        /// <returns> broker entry metadata or null </returns>
        public static BrokerEntryMetadata PeekBrokerEntryMetadataIfExist(AbstractByteBuffer headerAndPayload)
        {
            return ParseOrPeekBrokerEntryMetadataIfExist(headerAndPayload, null, true);
        }

        /// <summary>
        /// Internal method for parsing and peeking broker entry metadata. </summary>
        /// <param name="headerAndPayload"> header and payload buffer </param>
        /// <param name="brokerEntryMetadata"> the broker entry metadata instance to reuse, null if a new instance should be created </param>
        /// <param name="peek"> when true, the readerIndex of the headerAndPayload buffer is resetted to the original </param>
        /// <returns> the broker entry metadata instance or null </returns>
        private static BrokerEntryMetadata ParseOrPeekBrokerEntryMetadataIfExist(AbstractByteBuffer headerAndPayload, BrokerEntryMetadata brokerEntryMetadata, bool peek)
        {
            int readerIndex = headerAndPayload.ReaderIndex;
            if (headerAndPayload.GetShort(readerIndex) == MagicBrokerEntryMetadata)
            {
                headerAndPayload.SkipBytes(ISchema<short>.Bytes);
                try
                {
                    int brokerEntryMetadataSize = headerAndPayload.ReadInt();
                    if (brokerEntryMetadata == null)
                    {
                        brokerEntryMetadata = new BrokerEntryMetadata();
                    }
                    var d = BrokerEntryMetadata.Parser.ParseFrom(headerAndPayload.Array);
                    brokerEntryMetadata.MergeFrom(d);
                    //brokerEntryMetadata.Parser.ParseFrom(headerAndPayload, brokerEntryMetadataSize);
                    return brokerEntryMetadata;
                }
                finally
                {
                    if (peek)
                    {
                        headerAndPayload.SetReaderIndex(readerIndex);
                    }
                }
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Peeks the BrokerEntryMetadata from the given payload and applies the function to the result.
        /// null will be passed to the function if no BrokerEntryMetadata is found.
        /// The function shouldn't return the BrokerEntryMetadata instance or reference it after the function completes
        /// since it's a ThreadLocal instance that is reused.
        /// </summary>
        /// <param name="headerAndPayload"> the header and payload of the message </param>
        /// <param name="function"> the function to apply to the BrokerEntryMetadata </param>
        /// @param <T> the return type of the function </param>
        /// <returns> the result of the function </returns>
        public static T PeekBrokerEntryMetadataToObject<T>(AbstractByteBuffer headerAndPayload, Func<BrokerEntryMetadata, T> function)
        {
            BrokerEntryMetadata brokerEntryMetadata = ParseOrPeekBrokerEntryMetadataIfExist(headerAndPayload, BROKER_ENTRY_METADATA.Value, true);
            return function(brokerEntryMetadata);
        }

        /// <summary>
        /// Peeks the BrokerEntryMetadata from the given payload and applies a function returning a long value to the result.
        /// null will be passed to the function if no BrokerEntryMetadata is found. The function shouldn't reference the
        /// BrokerEntryMetadata instance after the function completes since it's a ThreadLocal instance that is reused.
        /// </summary>
        /// <param name="headerAndPayload"> the header and payload of the message </param>
        /// <param name="function"> the function to apply to the BrokerEntryMetadata </param>
        /// <returns> the result of the function </returns>
        public static long PeekBrokerEntryMetadataToLong(AbstractByteBuffer headerAndPayload, System.Func<BrokerEntryMetadata, long> function)
        {
            BrokerEntryMetadata brokerEntryMetadata = ParseOrPeekBrokerEntryMetadataIfExist(headerAndPayload, BROKER_ENTRY_METADATA.Value, true);
            return function(brokerEntryMetadata);
        }

        /// <summary>
        /// Peeks the BrokerEntryMetadata from the given payload and consumes the value using a function.
        /// null will be passed to the function if no BrokerEntryMetadata is found.
        /// The function shouldn't keep a reference to the BrokerEntryMetadata instance after the call completes
        /// since it's a ThreadLocal instance that is reused.
        /// </summary>
        /// <param name="headerAndPayload"> the header and payload of the message </param>
        /// <param name="function"> the function to apply to the BrokerEntryMetadata </param>
        public static void PeekBrokerEntryMetadataAndConsume(AbstractByteBuffer headerAndPayload, System.Action<BrokerEntryMetadata> function)
        {
            BrokerEntryMetadata brokerEntryMetadata = ParseOrPeekBrokerEntryMetadataIfExist(headerAndPayload, BROKER_ENTRY_METADATA.Value, true);
            function(brokerEntryMetadata);
        }

        public static AbstractByteBuffer SerializeMetadataAndPayload(ChecksumType checksumType, MessageMetadata msgMetadata, AbstractByteBuffer payload)
        {
            // / Wire format
            // [MAGIC_NUMBER][CHECKSUM] [METADATA_SIZE][METADATA] [PAYLOAD]
            int msgMetadataSize = msgMetadata.CalculateSize();
            int payloadSize = payload.ReadableBytes;
            int magicAndChecksumLength = ChecksumType.Crc32c.Equals(checksumType) ? (2 + 4) : 0;
            bool includeChecksum = magicAndChecksumLength > 0;
            int headerContentSize = magicAndChecksumLength + 4 + msgMetadataSize; // magicLength +
                                                                                  // checksumSize + msgMetadataLength +
                                                                                  // msgMetadataSize
            int checksumReaderIndex = -1;
            int totalSize = headerContentSize + payloadSize;

            AbstractByteBuffer metadataAndPayload = (AbstractByteBuffer)payload.Allocator.Buffer(totalSize, totalSize);

            // Create checksum placeholder
            if (includeChecksum)
            {
                metadataAndPayload.WriteShort(MagicCrc32c);
                checksumReaderIndex = metadataAndPayload.WriterIndex;
                metadataAndPayload.SetWriterIndex(metadataAndPayload.WriterIndex + ChecksumSize); // skip 4 bytes of checksum
            }

            // Write metadata
            metadataAndPayload.WriteInt(msgMetadataSize);
            msgMetadata.WriteTo(new CodedOutputStream(metadataAndPayload.Array));

            // write checksum at created checksum-placeholder
            if (includeChecksum)
            {
                metadataAndPayload.MarkReaderIndex();
                metadataAndPayload.SetReaderIndex(checksumReaderIndex + ChecksumSize);
                int metadataChecksum = ComputeChecksum(metadataAndPayload);
                int computedChecksum = ResumeChecksum(metadataChecksum, payload);
                // set computed checksum
                metadataAndPayload.SetInt(checksumReaderIndex, computedChecksum);
                metadataAndPayload.ResetReaderIndex();
            }
            metadataAndPayload.WriteBytes(payload);

            return metadataAndPayload;
        }

        public static long InitBatchMessageMetadata(MessageMetadata messageMetadata, MessageMetadata builder)
        {
            messageMetadata.PublishTime = builder.PublishTime;
            messageMetadata.ProducerName = builder.ProducerName;
            messageMetadata.SequenceId = builder.SequenceId;

            // Attach the key to the message metadata.
            if (builder.HasPartitionKey)
            {
                messageMetadata.PartitionKey = builder.PartitionKey;
                messageMetadata.PartitionKeyB64Encoded = builder.PartitionKeyB64Encoded;
            }
            if (builder.HasOrderingKey)
            {
                messageMetadata.OrderingKey = builder.OrderingKey;
            }
            if (builder.HasReplicatedFrom)
            {
                messageMetadata.ReplicatedFrom = builder.ReplicatedFrom;
            }
            if (builder.ReplicateTo.Count > 0)
            {
                for (int i = 0; i < builder.ReplicateTo.Count; i++)
                {
                    messageMetadata.ReplicateTo.Add(builder.ReplicateTo[i]);
                }
            }
            if (builder.HasSchemaVersion)
            {
                messageMetadata.SchemaVersion = builder.SchemaVersion;
            }
            if (builder.HasSchemaId)
            {
                messageMetadata.SchemaId = builder.SchemaId;
            }

            return (long)builder.SequenceId;
        }

        public static AbstractByteBuffer SerializeSingleMessageInBatchWithPayload(SingleMessageMetadata singleMessageMetadata, AbstractByteBuffer payload, AbstractByteBuffer batchBuffer)
        {
            singleMessageMetadata.PayloadSize = payload.ReadableBytes;

            // serialize meta-data size, meta-data and payload for single message in batch
            batchBuffer.WriteInt(singleMessageMetadata.CalculateSize());
            ToByteBuf(singleMessageMetadata, batchBuffer);
            return batchBuffer;
        }

        public static AbstractByteBuffer SerializeSingleMessageInBatchWithPayload(MessageMetadata msg, AbstractByteBuffer payload, AbstractByteBuffer batchBuffer)
        {
            // build single message meta-data
            SingleMessageMetadata smm = LOCAL_SINGLE_MESSAGE_METADATA.Value;
            //smm.Clear;

            if (msg.HasPartitionKey)
            {
                smm.PartitionKey = msg.PartitionKey;
                smm.PartitionKeyB64Encoded = msg.PartitionKeyB64Encoded;
            }
            if (msg.HasOrderingKey)
            {
                smm.OrderingKey = msg.OrderingKey;
            }
            for (int i = 0; i < msg.Properties.Count; i++)
            {
                var kv = new KeyValue(msg.Properties[i]);
                kv.Key = msg.Properties[i].Key;
                kv.Value = msg.Properties[i].Value;
                smm.Properties.Add(kv);
            }

            if (msg.HasEventTime)
            {
                smm.EventTime = msg.EventTime;
            }

            if (msg.HasSequenceId)
            {
                smm.SequenceId = msg.SequenceId;
            }

            if (msg.HasNullValue)
            {
                smm.NullValue = msg.NullValue;
            }

            if (msg.HasNullPartitionKey)
            {
                smm.NullPartitionKey = msg.NullPartitionKey;
            }

            return SerializeSingleMessageInBatchWithPayload(smm, payload, batchBuffer);
        }

        public static AbstractByteBuffer DeSerializeSingleMessageInBatch(AbstractByteBuffer uncompressedPayload, SingleMessageMetadata singleMessageMetadata, int index, int batchSize)
        {
            int singleMetaSize = (int)uncompressedPayload.ReadUnsignedInt();
            var d = SingleMessageMetadata.Parser.ParseFrom(uncompressedPayload.Array);
            d.PayloadSize = singleMetaSize;
            singleMessageMetadata.MergeFrom(d);

            int singleMessagePayloadSize = singleMessageMetadata.PayloadSize;

            int readerIndex = uncompressedPayload.ReaderIndex;
            AbstractByteBuffer singleMessagePayload = (AbstractByteBuffer)uncompressedPayload.RetainedSlice(readerIndex, singleMessagePayloadSize);

            // reader now points to beginning of payload read; so move it past message payload just read
            if (index < batchSize)
            {
                uncompressedPayload.SetReaderIndex(readerIndex + singleMessagePayloadSize);
            }

            return singleMessagePayload;
        }

        public static ByteBufPair SerializeCommandMessageWithSize(BaseCommand cmd, AbstractByteBuffer metadataAndPayload)
        {
            // / Wire format
            // [TOTAL_SIZE] [CMD_SIZE][CMD] [MAGIC_NUMBER][CHECKSUM] [METADATA_SIZE][METADATA] [PAYLOAD]
            //
            // metadataAndPayload contains from magic-number to the payload included

            int cmdSize = cmd.CalculateSize();
            int totalSize = 4 + cmdSize + metadataAndPayload.ReadableBytes;
            int headersSize = 4 + 4 + cmdSize;
            AbstractByteBuffer headers = (AbstractByteBuffer)metadataAndPayload.Allocator.Buffer(headersSize);
            headers.WriteInt(totalSize); // External frame

            // Write cmd
            headers.WriteInt(cmdSize);
            cmd.WriteTo(new CodedOutputStream(headers.Array));
            return ByteBufPair.Get(headers, metadataAndPayload);
        }

        public static MessageMetadata PeekMessageMetadata(AbstractByteBuffer metadataAndPayload, string subscription, long consumerId)
        {
            // save the reader index and restore after parsing
            int readerIdx = metadataAndPayload.ReaderIndex;
            try
            {
                MessageMetadata metadata = ParseMessageMetadata(metadataAndPayload);
                return metadata;
            }
            catch (Exception t)
            {
                //log.error("[{}] [{}] Failed to parse message metadata", subscription, consumerId, t);
                return null;
            }
            finally
            {
                metadataAndPayload.SetReaderIndex(readerIdx);
            }
        }

        public static void PeekMessageMetadata(AbstractByteBuffer metadataAndPayload, MessageMetadata msgMetadata)
        {
            // save the reader index and restore after parsing
            int readerIdx = metadataAndPayload.ReaderIndex;
            try
            {
                ParseMessageMetadata(metadataAndPayload, msgMetadata);
            }
            finally
            {
                metadataAndPayload.SetReaderIndex(readerIdx);
            }
        }

        /// <summary>
        /// Peek the message metadata from the buffer and return a deep copy of the metadata.
        /// 
        /// If you want to hold multiple <seealso cref="MessageMetadata"/> instances from multiple buffers, you must call this method
        /// rather than <seealso cref="Commands.peekMessageMetadata(AbstractByteBuffer, String, long)"/>, which returns a thread local reference,
        /// see <seealso cref="Commands.LOCAL_MESSAGE_METADATA"/>.
        /// </summary>
        public static MessageMetadata PeekAndCopyMessageMetadata(AbstractByteBuffer metadataAndPayload, string subscription, long consumerId)
        {
            MessageMetadata metadata = new MessageMetadata();
            try
            {
                PeekMessageMetadata(metadataAndPayload, metadata);
            }
            catch (Exception t)
            {
                //log.error("[{}] [{}] Failed to parse message metadata", subscription, consumerId, t);
                return null;
            }
            return metadata;
        }

        private static readonly byte[] NONE_KEY = Encoding.UTF8.GetBytes("NONE_KEY");
        public static byte[] PeekStickyKey(AbstractByteBuffer metadataAndPayload, string topic, string subscription)
        {
            int readerIdx = metadataAndPayload.ReaderIndex;
            try
            {
                MessageMetadata metadata = ParseMessageMetadata(metadataAndPayload);
                return ResolveStickyKey(metadata);
            }
            catch (Exception t)
            {
                //log.error("[{}] [{}] Failed to peek sticky key from the message metadata", topic, subscription, t);
                return NONE_KEY;
            }
            finally
            {
                metadataAndPayload.SetReaderIndex(readerIdx);
            }
        }
        public static byte[] ResolveStickyKey(MessageMetadata metadata)
        {
            byte[] stickyKey;
            if (metadata.HasOrderingKey)
            {
                stickyKey = metadata.OrderingKey.ToArray();
            }
            else if (metadata.HasPartitionKey)
            {
                if (metadata.PartitionKeyB64Encoded)
                {
                    string met = JsonSerializer.Serialize(metadata);
                    stickyKey = Convert.FromBase64String(met);
                }
                else
                {                    
                    stickyKey = Encoding.UTF8.GetBytes(metadata.PartitionKey);
                }
            }
            else if (metadata.HasProducerName && metadata.HasSequenceId)
            {
                string fallbackKey = metadata.ProducerName + "-" + metadata.SequenceId;
                stickyKey = fallbackKey.GetBytes(Encoding.UTF8);
            }
            else
            {
                stickyKey = NONE_KEY;
            }
            return stickyKey;
        }

        public static int CurrentProtocolVersion
		{
			get
			{
				// Return the last ProtocolVersion enum value
				return CURRENT_PROTOCOL_VERSION;
            }
        }
        private static void ToByteBuf(IMessage message, AbstractByteBuffer AbstractByteBuffer)
        {
            byte[] messageBytes;
            using (var stream = new MemoryStream())
            {
                message.WriteTo(stream);
                messageBytes = stream.ToArray();
            }
            AbstractByteBuffer.WriteBytes(messageBytes);
        }
        /// <summary>
		/// Definition of possible checksum types.
		/// </summary>
		public enum ChecksumType
        {
            Crc32c,
            None
        }

        public static bool PeerSupportsGetLastMessageId(int peerVersion)
        {
            return peerVersion >= (int)ProtocolVersion.V12;
        }

        public static bool PeerSupportsActiveConsumerListener(int peerVersion)
        {
            return peerVersion >= (int)ProtocolVersion.V12;
        }

        public static bool PeerSupportsMultiMessageAcknowledgment(int peerVersion)
        {
            return peerVersion >= (int)ProtocolVersion.V12;
        }

        public static bool PeerSupportJsonSchemaAvroFormat(int peerVersion)
        {
            return peerVersion >= (int)ProtocolVersion.V13;
        }

        public static bool PeerSupportsGetOrCreateSchema(int peerVersion)
        {
            return peerVersion >= (int)ProtocolVersion.V15;
        }

        public static bool PeerSupportsAckReceipt(int peerVersion)
        {
            return peerVersion >= (int)ProtocolVersion.V17;
        }

        public static bool PeerSupportsCarryAutoConsumeSchemaToBroker(int peerVersion)
        {
            return peerVersion >= (int)ProtocolVersion.V21;
        }

        private static Shared.ProducerAccessMode ConvertProducerAccessMode(Pulsar.Proto.ProducerAccessMode accessMode)
        {
            switch (accessMode)
            {
                case ProducerAccessMode.Exclusive:
                    return Shared.ProducerAccessMode.Exclusive;
                case ProducerAccessMode.Shared:
                    return Shared.ProducerAccessMode.Shared;
                case ProducerAccessMode.WaitForExclusive:
                    return Shared.ProducerAccessMode.WaitForExclusive;
                case ProducerAccessMode.ExclusiveWithFencing:
                    return Shared.ProducerAccessMode.ExclusiveWithFencing;
                default:
                    throw new ArgumentException("Unknown access mode: " + accessMode);
            }
        }

        public static ProducerAccessMode ConvertProducerAccessMode(Shared.ProducerAccessMode accessMode)
        {
            switch (accessMode)
            {
                case Shared.ProducerAccessMode.Exclusive:
                    return ProducerAccessMode.Exclusive;
                case Shared.ProducerAccessMode.Shared:
                    return ProducerAccessMode.Shared;
                case Shared.ProducerAccessMode.WaitForExclusive:
                    return ProducerAccessMode.WaitForExclusive;
                case Shared.ProducerAccessMode.ExclusiveWithFencing:
                    return ProducerAccessMode.ExclusiveWithFencing;
                default:
                    throw new ArgumentException("Unknown access mode: " + accessMode);
            }
        }

        public static bool PeerSupportsBrokerMetadata(int peerVersion)
        {
            return peerVersion >= (int)ProtocolVersion.V16;
        }

    }

}