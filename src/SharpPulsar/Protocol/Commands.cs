using SharpPulsar.Protocol.Proto;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using Google.Protobuf;
using SharpPulsar.Shared;
using AuthData = SharpPulsar.Protocol.Proto.AuthData;
using SharpPulsar.Protocol.Schema;
using System.Linq;
using System.Text;
using ProtoBuf;
using KeySharedMode = SharpPulsar.Protocol.Proto.KeySharedMode;
using SharpPulsar.Common;
using SharpPulsar.Helpers;
using SharpPulsar.Batch;
using Serializer = SharpPulsar.Helpers.Serializer;
using SharpPulsar.Extension;
using Akka.Util.Internal;
using SharpPulsar.Interfaces.Schema;
using SharpPulsar.TransactionImpl;
using static SharpPulsar.Protocol.Proto.CommandAck;

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
namespace SharpPulsar.Protocol
{
    internal static class Commands
	{

		// default message Size for transfer
		public static int DefaultMaxMessageSize = 5 * 1024 * 1024;
        public static int MessageSizeFramePadding = 10 * 1024;
		public static int InvalidMaxMessageSize = -1;
        // this present broker version don't have consumerEpoch feature,
        // so client don't need to think about consumerEpoch feature
        public static long DefaultConsumerEpoch = -1L;
        public static short MagicBrokerEntryMetadata = 0x0e02;

        public static bool PeerSupportJsonSchemaAvroFormat(int peerVersion)
		{
			return peerVersion >= (int)ProtocolVersion.V13;
		}
		public static ReadOnlySequence<byte> NewConnect(string authMethodName, string authData, string libVersion)
		{
			return NewConnect(authMethodName, authData, CurrentProtocolVersion, libVersion, null, null, null, null);
		}

		public static ReadOnlySequence<byte> NewConnect(string authMethodName, string authData, string libVersion, string targetBroker)
		{
			return NewConnect(authMethodName, authData, CurrentProtocolVersion, libVersion, targetBroker, null, null, null);
		}
        
        public static ReadOnlySequence<byte> NewConnect(string authMethodName, string authData, string libVersion, string targetBroker, string originalPrincipal, string clientAuthData, string clientAuthMethod)
		{
			return NewConnect(authMethodName, authData, CurrentProtocolVersion, libVersion, targetBroker, originalPrincipal, clientAuthData, clientAuthMethod);
		}
        private static void SetFeatureFlags(FeatureFlags flags)
        {
            flags.SupportsAuthRefresh = true;
            flags.SupportsBrokerEntryMetadata = true;
            flags.SupportsPartialProducer = true;   
        }
        public static ReadOnlySequence<byte> NewConnect(string authMethodName, string authData, int protocolVersion, string libVersion, string targetBroker, string originalPrincipal, string originalAuthData, string originalAuthMethod)
		{
            
            var connect = new CommandConnect
            {
                ClientVersion = libVersion ?? "Pulsar Client", 
                AuthMethodName = authMethodName,
                FeatureFlags = new FeatureFlags()
            };
            if ("ycav1".Equals(authMethodName))
			{
				// Handle the case of a client that gets updated before the broker and starts sending the string auth method
				// name. An example would be in broker-to-broker replication. We need to make sure the clients are still
				// passing both the enum and the string until all brokers are upgraded.
				connect.AuthMethod = AuthMethod.AuthMethodYcaV1;
			}

			if (!ReferenceEquals(targetBroker, null))
			{
				// When connecting through a proxy, we need to specify which broker do we want to be proxied through
				connect.ProxyToBrokerUrl = targetBroker;
			}

			if (!ReferenceEquals(authData, null))
			{
				connect.AuthData = ByteString.CopyFromUtf8(authData).ToByteArray();
			}

			if (!ReferenceEquals(originalPrincipal, null))
			{
				connect.OriginalPrincipal = originalPrincipal;
			}

			if (!ReferenceEquals(originalAuthData, null))
			{
				connect.OriginalAuthData = originalAuthData;
			}

			if (!ReferenceEquals(originalAuthMethod, null))
			{
				connect.OriginalAuthMethod = originalAuthMethod;
			}
			connect.ProtocolVersion = protocolVersion;
            SetFeatureFlags(connect.FeatureFlags);
			return Serializer.Serialize(connect.ToBaseCommand());
		}
        public static ReadOnlySequence<byte> NewTcClientConnectRequest(long tcId, long requestId)
        {
            var tcClientConnect = new CommandTcClientConnectRequest
            {
                TcId = (ulong)tcId,
                RequestId = (ulong)requestId
            };
            return Serializer.Serialize(tcClientConnect.ToBaseCommand());
        }
        public static ReadOnlySequence<byte> NewConnect(string authMethodName, AuthData authData, int protocolVersion, 
            string libVersion, string targetBroker, string originalPrincipal, AuthData originalAuthData, string originalAuthMethod, string proxyVersion)
		{
            var connect = new CommandConnect
            {
                ClientVersion = libVersion,
                AuthMethodName = authMethodName,
                FeatureFlags = new FeatureFlags(),
                ProtocolVersion = protocolVersion
            };
            if (proxyVersion != null)
            {
                connect.ProxyVersion = proxyVersion;
            }
            if (!string.IsNullOrWhiteSpace(targetBroker))
			{
				// When connecting through a proxy, we need to specify which broker do we want to be proxied through
				connect.ProxyToBrokerUrl = targetBroker;
			}

			if (authData != null)
			{
				connect.AuthData = authData.auth_data;
			}

			if (!string.IsNullOrWhiteSpace(originalPrincipal))
			{
				connect.OriginalPrincipal = originalPrincipal;
			}

			if (originalAuthData != null)
			{
				connect.OriginalAuthData = Encoding.UTF8.GetString(originalAuthData.auth_data);
			}

			if (!string.IsNullOrWhiteSpace(originalAuthMethod))
			{
				connect.OriginalAuthMethod = originalAuthMethod;
			}
            SetFeatureFlags(connect.FeatureFlags);
            var ba = connect.ToBaseCommand();
            return Serializer.Serialize(ba);
        }

        public static ReadOnlySequence<byte> NewConnect(string authMethodName, AuthData authData, int protocolVersion,
            string libVersion, string targetBroker, string originalPrincipal, AuthData originalAuthData, string originalAuthMethod)
        {
            var connect = new CommandConnect
            {
                ClientVersion = libVersion,
                AuthMethodName = authMethodName,
                FeatureFlags = new FeatureFlags(),
                ProtocolVersion = protocolVersion
            };
            if (!string.IsNullOrWhiteSpace(targetBroker))
            {
                // When connecting through a proxy, we need to specify which broker do we want to be proxied through
                connect.ProxyToBrokerUrl = targetBroker;
            }

            if (authData != null)
            {
                connect.AuthData = authData.auth_data;
            }

            if (!string.IsNullOrWhiteSpace(originalPrincipal))
            {
                connect.OriginalPrincipal = originalPrincipal;
            }

            if (originalAuthData != null)
            {
                connect.OriginalAuthData = Encoding.UTF8.GetString(originalAuthData.auth_data);
            }

            if (!string.IsNullOrWhiteSpace(originalAuthMethod))
            {
                connect.OriginalAuthMethod = originalAuthMethod;
            }
            SetFeatureFlags(connect.FeatureFlags);
            var ba = connect.ToBaseCommand();
            return Serializer.Serialize(ba);
        }
        public static ReadOnlySequence<byte> NewAuthResponse(string authMethod, AuthData clientData, int clientProtocolVersion, string clientVersion)
        {
            var authData = new AuthData {auth_data = clientData.auth_data, AuthMethodName = authMethod};

            var response = new CommandAuthResponse
            {
                Response = authData,
                ProtocolVersion = clientProtocolVersion,
                ClientVersion = clientVersion ?? "Pulsar Client"
            };
            return Serializer.Serialize(response.ToBaseCommand());
            
        }
		public static ReadOnlySequence<byte> NewAuthChallenge(string authMethod, AuthData brokerData, int clientProtocolVersion)
		{
			var challenge = new CommandAuthChallenge();

			// If the broker supports a newer version of the protocol, it will anyway advertise the max version that the
			// client supports, to avoid confusing the client.
            var versionToAdvertise = Math.Min(Enum.GetValues(typeof(ProtocolVersion)).Cast<int>().Max(), clientProtocolVersion);

			challenge.ProtocolVersion = versionToAdvertise;

            challenge.Challenge = new AuthData
            {
                auth_data = brokerData.auth_data,
                AuthMethodName = authMethod
            };
			//var challenge = challenge.Challenge().Build();

			return Serializer.Serialize(challenge.ToBaseCommand());
			
		}
		
		public static ReadOnlySequence<byte> NewSendError(long producerId, long sequenceId, ServerError error, string errorMsg)
		{
            var sendError = new CommandSendError
            {
                ProducerId = (ulong) producerId,
                SequenceId = (ulong) sequenceId,
                Error = error,
                Message = errorMsg
            };
            return Serializer.Serialize(sendError.ToBaseCommand());
			
			
		}


		public static bool HasChecksum(ReadOnlySequence<byte> reader)
        {
            if (reader.StartsWith(Constants.MagicNumber))
            {
                return reader.ReadUInt32(Constants.MagicNumber.Length, true) == CRC32C.Calculate(reader.Slice(Constants.MetadataSizeOffset));
            }
            return false;
        }

		/// <summary>
		/// Read the checksum and advance the reader index in the buffer.
		/// 
		/// <para>Note: This method assume the checksum presence was already verified before.
		/// </para>
		/// </summary>
		public static int ReadChecksum(ReadOnlySequence<byte> reader)
		{
            //reader.SkipBytes(2);
            //return reader.ReadInt32();
            return 1;
		}

		public static void SkipChecksumIfPresent(ReadOnlySequence<byte> reader)
		{
			if (HasChecksum(reader))
			{
				ReadChecksum(reader);
			}
		}
		
		public static MessageMetadata ParseMessageMetadata(ReadOnlySequence<byte> reader)
		{
			try
			{
                // initially reader-index may point to start of broker entry metadata :
                // increment reader-index to start_of_headAndPayload to parse metadata
                var skipped = SkipBrokerEntryMetadataIfExist(reader);
                SkipChecksumIfPresent(skipped);
                var metadataSize = skipped.ReadUInt32(Constants.MetadataSizeOffset, true);
                return Serializer.Deserialize<MessageMetadata>(skipped.Slice(Constants.MetadataOffset, metadataSize));
            }
			catch (Exception e)
			{
				throw new Exception(e.Message, e);
			}
		}

        public static ReadOnlySequence<byte> NewSend(long producerId, long sequenceId, int numMessaegs, ChecksumType checksumType, long ledgerId, long entryId, MessageMetadata messageMetadata, byte[] payload)
        {
            return NewSend(producerId, sequenceId, -1, numMessaegs, messageMetadata.ShouldSerializeTxnidLeastBits() ? (long)messageMetadata.TxnidLeastBits : -1, messageMetadata.ShouldSerializeTxnidMostBits() ? (long)messageMetadata.TxnidMostBits : -1, checksumType, ledgerId, entryId, messageMetadata, payload);
        }

        public static ReadOnlySequence<byte> NewSend(long producerId, long sequenceId, int numMessaegs, ChecksumType checksumType, MessageMetadata messageMetadata, byte[] payload)
        {
            return NewSend(producerId, sequenceId, -1, numMessaegs, messageMetadata.ShouldSerializeTxnidLeastBits() ? (long)messageMetadata.TxnidLeastBits : -1, messageMetadata.ShouldSerializeTxnidMostBits() ? (long)messageMetadata.TxnidMostBits : -1, checksumType, -1, -1, messageMetadata, payload);
        }

        public static ReadOnlySequence<byte> NewSend(long producerId, long lowestSequenceId, long highestSequenceId, int numMessaegs, ChecksumType checksumType, MessageMetadata messageMetadata, byte[] payload)
        {
            return NewSend(producerId, lowestSequenceId, highestSequenceId, numMessaegs, messageMetadata.ShouldSerializeTxnidLeastBits() ? (long)messageMetadata.TxnidLeastBits : -1, messageMetadata.ShouldSerializeTxnidMostBits() ? (long)messageMetadata.TxnidMostBits : -1, checksumType, -1, -1, messageMetadata, payload);
        }

        public static ReadOnlySequence<byte> NewSend(long producerId, long sequenceId, long highestSequenceId, int numMessages, long txnIdLeastBits, long txnIdMostBits, ChecksumType checksumType, long ledgerId, long entryId, MessageMetadata messageData, byte[] payload)
		{
            var send = new CommandSend
            {
                ProducerId = (ulong) producerId, 
                SequenceId = (ulong) sequenceId
            };
            if (highestSequenceId >= 0)
            {
                send.HighestSequenceId = (ulong)highestSequenceId;
            }
            if (numMessages > 1)
			{
				send.NumMessages = numMessages;
			}
			if (txnIdLeastBits >= 0)
			{
				send.TxnidLeastBits = (ulong)txnIdLeastBits;
			}
			if (txnIdMostBits >= 0)
			{
				send.TxnidMostBits = (ulong)txnIdMostBits;
			}
            if (messageData.ShouldSerializeTotalChunkMsgSize() && messageData.TotalChunkMsgSize > 1)
            {
                send.IsChunk = true;
            }

            if (messageData.ShouldSerializeMarkerType())
            {
                send.Marker = true;
            }
            if (ledgerId >= 0 && entryId >= 0)
            {
                send.MessageId = new MessageIdData { ledgerId = (ulong)ledgerId, entryId = (ulong)entryId };
            }
                        
            return Serializer.Serialize(send.ToBaseCommand(), checksumType, messageData, payload);
		}

		public static ReadOnlySequence<byte> NewSubscribe(string topic, string subscription, long consumerId, long requestId, CommandSubscribe.SubType subType, int priorityLevel, string consumerName, long resetStartMessageBackInSeconds)
		{
			return NewSubscribe(topic, subscription, consumerId, requestId, subType, priorityLevel, consumerName, true, null, new Dictionary<string,string>(), false, false, CommandSubscribe.InitialPosition.Earliest, resetStartMessageBackInSeconds, null, true);
		}
		
		public static ReadOnlySequence<byte> NewSubscribe(string topic, string subscription, long consumerId, long requestId, CommandSubscribe.SubType subType, int priorityLevel, string consumerName, bool isDurable, MessageIdData startMessageId, IDictionary<string, string> metadata, bool readCompacted, bool isReplicated, CommandSubscribe.InitialPosition subscriptionInitialPosition, long startMessageRollbackDurationInSec, ISchemaInfo schemaInfo, bool createTopicIfDoesNotExist)
		{
            return NewSubscribe(topic, subscription, consumerId, requestId, subType, priorityLevel, consumerName, isDurable, startMessageId, metadata, readCompacted, isReplicated, subscriptionInitialPosition, startMessageRollbackDurationInSec, schemaInfo, createTopicIfDoesNotExist, null, new Dictionary<string, string>(), DefaultConsumerEpoch);
		}

		public static ReadOnlySequence<byte> NewSubscribe(string topic, string subscription, long consumerId, 
            long requestId, CommandSubscribe.SubType subType, int priorityLevel, string consumerName, 
            bool isDurable, MessageIdData startMessageId, IDictionary<string, string> metadata, bool readCompacted, 
            bool isReplicated, CommandSubscribe.InitialPosition subscriptionInitialPosition, 
            long startMessageRollbackDurationInSec, ISchemaInfo schemaInfo, 
            bool createTopicIfDoesNotExist, KeySharedPolicy keySharedPolicy, 
            IDictionary<string, string> subscriptionProperties, long consumerEpoch)
		{
            var subscribe = new CommandSubscribe
            {
                Topic = topic,
                Subscription = subscription,
                subType = subType,
                ConsumerId = (ulong) consumerId,
                ConsumerName = consumerName,
                RequestId = (ulong) requestId,
                PriorityLevel = priorityLevel,
                Durable = isDurable,
                ReadCompacted = readCompacted,
                initialPosition = subscriptionInitialPosition,
                ReplicateSubscriptionState = isReplicated,
                ForceTopicCreation = createTopicIfDoesNotExist,
                ConsumerEpoch = (ulong)consumerEpoch
                
            };
            if(subscriptionProperties != null && subscriptionProperties.Count > 0)
            {
                var kv = new List<KeyValue>();
                subscriptionProperties.ForEach(k =>
                {
                    var keyValue = new KeyValue
                    {
                        Key = k.Key,
                        Value = k.Value
                    };
                    kv.Add(keyValue);
                });
                subscribe.SubscriptionProperties.AddRange(kv);
            }
            if (keySharedPolicy != null)
            {
                var keySharedMeta = new KeySharedMeta
                {
                    allowOutOfOrderDelivery = keySharedPolicy.AllowOutOfOrderDelivery,
                    keySharedMode = ConvertKeySharedMode(keySharedPolicy.KeySharedMode)
                };
                
                if (keySharedPolicy is KeySharedPolicy.KeySharedPolicySticky sticky)
                {
                    var ranges = sticky.GetRanges().Ranges;
                    foreach (var range in ranges)
                    {
                        keySharedMeta.hashRanges.Add(new IntRange { Start = range.Start, End = range.End });
                    }
				}

                subscribe.keySharedMeta = keySharedMeta;
            }
			if (startMessageId != null)
			{
				subscribe.StartMessageId = startMessageId;
			}
			if (startMessageRollbackDurationInSec > 0)
			{
				subscribe.StartMessageRollbackDurationSec = (ulong)startMessageRollbackDurationInSec;
			}
			subscribe.Metadatas.AddRange(CommandUtils.ToKeyValueList(metadata));

            if (schemaInfo != null)
            {
                subscribe.Schema = ConvertSchema(schemaInfo);
            }

			return Serializer.Serialize(subscribe.ToBaseCommand());

            
		}
        public static ReadOnlySequence<byte> NewWatchTopicList(long requestId, long watcherId, string @namespace, string topicsPattern, string topicsHash)
        {
            var watchTopic = new CommandWatchTopicList 
            { 
                RequestId =(ulong) requestId,
                Namespace = @namespace, 
                TopicsPattern = topicsPattern, 
                WatcherId = (ulong) watcherId,

            };
            if (topicsHash != null)
            {
                watchTopic.TopicsHash = topicsHash;
            }

            return Serializer.Serialize(watchTopic.ToBaseCommand());
        }

        public static ReadOnlySequence<byte> NewWatchTopicListSuccess(long requestId, long watcherId, string topicsHash, IList<string> topics)
        {
            var success = new CommandWatchTopicListSuccess 
            { 
                RequestId = (ulong) requestId,
                WatcherId= (ulong) watcherId,
            };
            if (topicsHash != null)
            {
                success.TopicsHash = topicsHash;
            }

            if (topics != null && topics.Count > 0)
            {
                success.Topics.AddRange(topics);
            }

            return Serializer.Serialize(success.ToBaseCommand());
        }

        public static ReadOnlySequence<byte> NewWatchTopicUpdate(long watcherId, IList<string> newTopics, IList<string> deletedTopics, string topicsHash)
        {
            var update = new CommandWatchTopicUpdate
            {
                WatcherId = (ulong) watcherId,  
                TopicsHash = topicsHash
            };
            update.NewTopics.AddRange(newTopics);
            update.DeletedTopics.AddRange(deletedTopics);
            return Serializer.Serialize(update.ToBaseCommand());
        }

        public static ReadOnlySequence<byte> NewWatchTopicListClose(long watcherId, long requestId)
        {
            var close = new CommandWatchTopicListClose
            {
                RequestId= (ulong) requestId,   
                WatcherId = (ulong) watcherId,
            };
            return Serializer.Serialize(close.ToBaseCommand());
        }
        public static long GetEntryTimestamp(ReadOnlySequence<byte> headersAndPayloadWithBrokerEntryMetadata)
        {
            // get broker timestamp first if BrokerEntryMetadata is enabled with AppendBrokerTimestampMetadataInterceptor
            BrokerEntryMetadata brokerEntryMetadata = ParseBrokerEntryMetadataIfExist(headersAndPayloadWithBrokerEntryMetadata);
            if (brokerEntryMetadata != null && brokerEntryMetadata.ShouldSerializeBrokerTimestamp())
            {
                return (long)brokerEntryMetadata.BrokerTimestamp;
            }
            // otherwise get the publish_time
            return (long)ParseMessageMetadata(headersAndPayloadWithBrokerEntryMetadata).PublishTime;
        }

        private static KeySharedMode ConvertKeySharedMode(Common.KeySharedMode? mode)
        {
            switch (mode)
            {
                case Common.KeySharedMode.AutoSplit:
                    return KeySharedMode.AutoSplit;
                case Common.KeySharedMode.Sticky:
                    return KeySharedMode.Sticky;
                default:
                    throw new ArgumentException("Unexpected key shared mode: " + mode);
            }
        }
		public static ReadOnlySequence<byte> NewUnsubscribe(long consumerId, long requestId)
		{
            var unsubscribe = new CommandUnsubscribe
            {
                ConsumerId = (ulong) consumerId, RequestId = (ulong) requestId
            };
            return Serializer.Serialize(unsubscribe.ToBaseCommand());
			
		}

		public static ReadOnlySequence<byte> NewActiveConsumerChange(long consumerId, bool isActive)
		{
            var change = new CommandActiveConsumerChange {ConsumerId = (ulong) consumerId, IsActive = isActive};
            return Serializer.Serialize(change.ToBaseCommand());
			
		}

		public static ReadOnlySequence<byte> NewSeek(long consumerId, long requestId, long ledgerId, long entryId, List<long> ackSet)
		{
            var seek = new CommandSeek {ConsumerId = (ulong) consumerId, RequestId = (ulong) requestId};

            var messageId = new MessageIdData {ledgerId = (ulong) ledgerId, entryId = (ulong) entryId, AckSets = ackSet};
            seek.MessageId = messageId;
			return Serializer.Serialize(seek.ToBaseCommand());			
		}
        public static ReadOnlySequence<byte> SkipBrokerEntryMetadataIfExist(ReadOnlySequence<byte> reader)
        {
            var magic = reader.ReadUInt32(2, true);
            if (magic == MagicBrokerEntryMetadata)
            {
                var brokerEntryMetadataSize = reader.Slice(2).ReadUInt32(0, true);
                return reader.Slice(1, brokerEntryMetadataSize);
            }
            return reader;
        }
        public static BrokerEntryMetadata ParseBrokerEntryMetadataIfExist(ReadOnlySequence<byte> headerAndPayload)
        {
            var magic = headerAndPayload.ReadUInt32(2, true);
            if (magic == MagicBrokerEntryMetadata)
            {
                var brokerEntryMetadataSize = headerAndPayload.Slice(2).ReadUInt32(0, true);
                var brokerEntryMetadata = Serializer.Deserialize<BrokerEntryMetadata>(headerAndPayload.Slice(4, brokerEntryMetadataSize));
                return brokerEntryMetadata;
            }

            return null;
        }
        public static BrokerEntryMetadata ParseBrokerEntryMetadataIfExist(BinaryReader reader)
        {
            var magic = reader.ReadInt16();
            if (magic == MagicBrokerEntryMetadata)
            {
                var brokerEntryMetadataSize = reader.ReadInt32();
                var brokerEntryMetadata = ProtoBuf.Serializer.DeserializeWithLengthPrefix<BrokerEntryMetadata>(reader.BaseStream, PrefixStyle.Fixed32BigEndian);
                return brokerEntryMetadata;
            }

            return null;
        }
        
        public static BrokerEntryMetadata PeekBrokerEntryMetadataIfExist(ReadOnlySequence<byte> headerAndPayloadWithBrokerEntryMetadata)
        {
            var payload = headerAndPayloadWithBrokerEntryMetadata.ToArray();
            var memory = Serializer.MemoryManager.GetStream();
            memory.Write(payload, 0, payload.Length);
            var reader = new BinaryReader(memory);
            var readerIndex = reader.BaseStream.Position;
            var entryMetadata = ParseBrokerEntryMetadataIfExist(reader);
            memory.Seek(readerIndex, SeekOrigin.Current);
            return entryMetadata;
        }
        public static ReadOnlySequence<byte> NewSeek(long consumerId, long requestId, long timestamp)
		{
            var seek = new CommandSeek
            {
                ConsumerId = (ulong) consumerId,
                RequestId = (ulong) requestId,
                MessagePublishTime = (ulong) timestamp
            };

            return Serializer.Serialize(seek.ToBaseCommand());

			
		}

		public static ReadOnlySequence<byte> NewCloseConsumer(long consumerId, long requestId)
		{
            var closeConsumer = new CommandCloseConsumer
            {
                ConsumerId = (ulong) consumerId, RequestId = (ulong) requestId
            };
            return Serializer.Serialize(closeConsumer.ToBaseCommand());
			
			
		}

		public static ReadOnlySequence<byte> NewReachedEndOfTopic(long consumerId)
		{
            var reachedEndOfTopic = new CommandReachedEndOfTopic {ConsumerId = (ulong) consumerId};
            return Serializer.Serialize(reachedEndOfTopic.ToBaseCommand());
			
			
		}

		public static ReadOnlySequence<byte> NewCloseProducer(long producerId, long requestId)
		{
            var closeProducer = new CommandCloseProducer
            {
                ProducerId = (ulong) producerId, RequestId = (ulong) requestId
            };
            return Serializer.Serialize(closeProducer.ToBaseCommand());
			
			
		}

		public static ReadOnlySequence<byte> NewProducer(string topic, long producerId, long requestId, string producerName, IDictionary<string, string> metadata, bool isTxnEnabled)
		{
			return NewProducer(topic, producerId, requestId, producerName, false, metadata, isTxnEnabled);
		}

		public static ReadOnlySequence<byte> NewProducer(string topic, long producerId, long requestId, string producerName, bool encrypted, IDictionary<string, string> metadata, bool isTxnEnabled)
		{
			return NewProducer(topic, producerId, requestId, producerName, encrypted, metadata, null, 0, false, Common.ProducerAccessMode.Shared, null, isTxnEnabled, null);
		}
        private static Proto.Schema.Type GetSchemaType(SchemaType type)
		{
			if (type.Value < 0)
			{
				return Proto.Schema.Type.None;
			}
			else
			{
				return Enum.GetValues(typeof(Proto.Schema.Type)).Cast<Proto.Schema.Type>().ToList()[type.Value];
			}
		}

		public static SchemaType GetSchemaType(Proto.Schema.Type type)
		{
			if (type < 0)
			{
				// this is unexpected
				return SchemaType.NONE;
			}
			else
			{
				return SchemaType.ValueOf((int)type);
			}
		}
		public static SchemaType GetSchemaTypeFor(SchemaType type)
		{
			if (type.Value < 0)
			{
				// this is unexpected
				return SchemaType.NONE;
			}
			else
			{
				return SchemaType.ValueOf(type.Value);
			}
		}

        private static Proto.Schema ConvertSchema(ISchemaInfo SchemaInfo)
        {
            var schema = new Proto.Schema
            {
                Name = SchemaInfo.Name,
                SchemaData = SchemaInfo.Schema,
                type = GetSchemaType(SchemaInfo.Type)
            };

            SchemaInfo.Properties.SetOfKeyValuePairs().ForEach(entry =>
            {
                if (entry.Key != null && entry.Value != null)
                {
                    schema.Properties.Add(new KeyValue { Key = entry.Key, Value = entry.Value });
                }
            });
            return schema;
        }

        public static ReadOnlySequence<byte> NewProducer(string topic, long producerId, long requestId, string producerName, bool encrypted, IDictionary<string, string> metadata, ISchemaInfo schemaInfo, long epoch, bool userProvidedProducerName, Common.ProducerAccessMode accessMode, long? topicEpoch, bool isTxnEnabled, string initialSubscriptionName)
		{
            var producer = new CommandProducer
            {
                Topic = topic,
                ProducerId = (ulong)producerId,
                RequestId = (ulong)requestId,
                Epoch = (ulong)epoch,
                ProducerAccessMode = ConvertProducerAccessMode(accessMode),
                TxnEnabled = isTxnEnabled,
                UserProvidedProducerName = userProvidedProducerName,
                Encrypted = encrypted
            };
			
            if (!string.IsNullOrWhiteSpace(producerName))
			{
				producer.ProducerName = producerName;
			}
            if (metadata.Count > 0)
                metadata.ForEach(x => producer.Metadatas.Add(new KeyValue { Key = x.Key, Value = x.Value}));

			if (schemaInfo != null)
			{
                producer.Schema = ConvertSchema(schemaInfo);
			}
            if (topicEpoch.HasValue)
                producer.TopicEpoch = (ulong)topicEpoch.Value;

            if(!string.IsNullOrEmpty(initialSubscriptionName))
                producer.InitialSubscriptionName = initialSubscriptionName;

			return Serializer.Serialize(producer.ToBaseCommand());			
		}

		public static ReadOnlySequence<byte> NewPartitionMetadataRequest(string topic, long requestId)
		{
            var partitionMetadata = new CommandPartitionedTopicMetadata
            {
                Topic = topic, RequestId = (ulong) requestId
            };
            return Serializer.Serialize(partitionMetadata.ToBaseCommand());
			
			
		}

		public static ReadOnlySequence<byte> NewLookup(string topic, string listenerName, bool authoritative, long requestId)
		{
            var lookupTopic = new CommandLookupTopic
            {
                Topic = topic, 
                RequestId = (ulong) requestId, 
                Authoritative = authoritative
            };
            if (!string.IsNullOrWhiteSpace(listenerName))
            {
                lookupTopic.AdvertisedListenerName = listenerName;
            }
			return Serializer.Serialize(lookupTopic.ToBaseCommand());
			
			
		}
		public static ReadOnlySequence<byte> NewMultiTransactionMessageAck(long consumerId, TxnID txnID, IList<(long ledger, long entry, List<long> bitSet)> entries)
		{
            var ackBuilder = new CommandAck
            {
                ConsumerId = (ulong)consumerId,
                ack_type = AckType.Individual,
                TxnidLeastBits = (ulong)txnID.LeastSigBits,
                TxnidMostBits = (ulong)txnID.MostSigBits
            };
            return NewMultiMessageAckCommon(ackBuilder, entries);
		}
		public static ReadOnlySequence<byte> NewMultiMessageAckCommon(CommandAck ackBuilder, IList<(long ledger, long entry, List<long> bitSet)> entries)
		{
			int entriesCount = entries.Count;
			for (int i = 0; i < entriesCount; i++)
			{
				long ledgerId = entries[i].ledger;
				long entryId = entries[i].entry;
				var bitSet = entries[i].bitSet;
                var messageIdDataBuilder = new MessageIdData
                {
                    ledgerId = (ulong)ledgerId,
                    entryId = (ulong)entryId
                };
                if (bitSet != null)
				{
					messageIdDataBuilder.AckSets = bitSet;
				}
				var messageIdData = messageIdDataBuilder;
				ackBuilder.MessageIds.Add(messageIdData);
			}

			var ack = ackBuilder;

			return Serializer.Serialize(ack.ToBaseCommand());
			
		}
        public static ReadOnlySequence<byte> NewMultiMessageAck(long consumerId, IList<(long LedgerId, long EntryId, List<long> Sets)> entries, long requestId)
        {
            var ackBuilder = new CommandAck
            {
                ConsumerId = (ulong)consumerId,
                ack_type = AckType.Individual
            };
            if (requestId >= 0)
            {
                ackBuilder.RequestId = (ulong)requestId;
            }
            return NewMultiMessageAckCommon(ackBuilder, entries);
        }
        public static ReadOnlySequence<byte> NewMultiMessageAck(long consumerId, IList<(long LedgerId, long EntryId, BitSet Sets)> entries)
        {
            var ackCmd = new CommandAck {ConsumerId = (ulong) consumerId, ack_type = AckType.Individual};

            var entriesCount = entries.Count;
            for (var i = 0; i < entriesCount; i++)
            {
                var ledgerId = entries[i].LedgerId;
                var entryId = entries[i].EntryId;
                var bitSet = entries[i].Sets;
                var messageIdData = new MessageIdData {ledgerId = (ulong) ledgerId, entryId = (ulong) entryId};
                if (bitSet != null)
                {
                    messageIdData.AckSets = bitSet.ToLongArray().ToList();
                }
                ackCmd.MessageIds.Add(messageIdData);
            }
            return Serializer.Serialize(ackCmd.ToBaseCommand());
            
        }
        public static ReadOnlySequence<byte> NewMultiMessageAck(long consumerId, IList<(long LedgerId, long EntryId, List<long> Sets)> entries)
        {
            var ackCmd = new CommandAck { ConsumerId = (ulong)consumerId, ack_type = AckType.Individual };

            var entriesCount = entries.Count;
            for (var i = 0; i < entriesCount; i++)
            {
                var ledgerId = entries[i].LedgerId;
                var entryId = entries[i].EntryId;
                var bitSet = entries[i].Sets;
                var messageIdData = new MessageIdData { ledgerId = (ulong)ledgerId, entryId = (ulong)entryId };
                if (bitSet != null)
                {
                    messageIdData.AckSets = bitSet;
                }
                ackCmd.MessageIds.Add(messageIdData);
            }
            return Serializer.Serialize(ackCmd.ToBaseCommand());
            
        }
        /// <summary>
        /// Peek the message metadata from the buffer and return a deep copy of the metadata.
        ///  
        /// If you want to hold multiple <seealso cref="MessageMetadata"/> instances from multiple buffers, you must call this method
        /// rather than <seealso cref="Commands.peekMessageMetadata(ByteBuf, string, long)"/>, which returns a thread local reference,
        /// see <seealso cref="Commands.LOCAL_MESSAGE_METADATA"/>.
        /// </summary>
        
        public static MessageMetadata PeekAndCopyMessageMetadata(ReadOnlySequence<byte> metadataAndPayload, string subscription, long consumerId)
        {
            MessageMetadata localMetadata = PeekMessageMetadata(metadataAndPayload, subscription, consumerId);
            if (localMetadata == null)
            {
                return null;
            }

            return localMetadata;
        }
        public static MessageMetadata PeekMessageMetadata(ReadOnlySequence<byte> metadataAndPayload, string subscription, long consumerId)
        {
            try
            {

                // save the reader index and restore after parsing
                var payload = metadataAndPayload.ToArray();
                var memory = Serializer.MemoryManager.GetStream();
                memory.Write(payload, 0, payload.Length);
                var reader = new BinaryReader(memory);
                var readerIdx = reader.BaseStream.Position;
                MessageMetadata metadata = ParseMessageMetadata(metadataAndPayload);
                metadataAndPayload.ReadUInt32(readerIdx, true);
                return metadata;
            }
            catch (Exception t)
            {
                throw new Exception($"[{subscription}] [{consumerId}] Failed to parse message metadata", t);
            }
        }

        private static readonly byte[] NONE_KEY = Encoding.UTF8.GetBytes("NONE_KEY");
        public static ReadOnlySequence<byte> PeekStickyKey(ReadOnlySequence<byte> metadataAndPayload, string topic, string subscription)
        {
            try
            {
                var payload = metadataAndPayload.ToArray();
                var memory = Serializer.MemoryManager.GetStream();
                memory.Write(payload, 0, payload.Length);
                var reader = new BinaryReader(memory);
                var readerIdx = reader.BaseStream.Position;
                MessageMetadata metadata = ParseMessageMetadata(metadataAndPayload);
                metadataAndPayload.ReadUInt32(readerIdx, true);
                if (metadata.ShouldSerializeOrderingKey())
                {
                    return new ReadOnlySequence<byte>(metadata.OrderingKey);
                }
                else if (metadata.ShouldSerializePartitionKey())
                {
                    if (metadata.ShouldSerializePartitionKeyB64Encoded())
                    {
                        metadata.PartitionKey = Convert.ToBase64String(Encoding.UTF8.GetBytes(metadata.PartitionKey));
                        return new ReadOnlySequence<byte>(Encoding.UTF8.GetBytes(metadata.PartitionKey)); 
                    }

                    return new ReadOnlySequence<byte>(Encoding.UTF8.GetBytes(metadata.PartitionKey));
                }
            }
            catch (Exception t)
            {
                throw new Exception($"[{topic}] [{subscription}] Failed to peek sticky key from the message metadata", t);
            }

            return new ReadOnlySequence<byte>(NONE_KEY);
        }
        public static ReadOnlySequence<byte> NewAck(long consumerId, long ledgerId, long entryId, List<long> ackSets, CommandAck.AckType ackType, CommandAck.ValidationError? validationError, IDictionary<string, long> properties)
		{
			return NewAck(consumerId, ledgerId, entryId, ackSets, ackType, validationError, properties, -1L, -1L, -1L, -1);
		}
        public static ReadOnlySequence<byte> NewAck(long consumerId, long ledgerId, long entryId, List<long> ackSets, CommandAck.AckType ackType, CommandAck.ValidationError? validationError, IDictionary<string, long> properties, long requestId)
        {
            return NewAck(consumerId, ledgerId, entryId, ackSets, ackType, validationError, properties, -1L, -1L, requestId, -1);
        }
        public static ReadOnlySequence<byte> NewAck(long consumerId, long ledgerId, long entryId, List<long> ackSet, CommandAck.AckType ackType, CommandAck.ValidationError? validationError, IDictionary<string, long> properties, long txnIdLeastBits, long txnIdMostBits, long requestId)
		{
			return NewAck(consumerId, ledgerId, entryId, ackSet, ackType, validationError,
					properties, txnIdLeastBits, txnIdMostBits, requestId, -1);
		}
        public static ReadOnlySequence<byte> NewAck(long consumerId, IList<MessageIdData> messageIds, AckType ackType,
                                 ValidationError? validationError, IDictionary<string, long> properties, long txnIdLeastBits,
                                 long txnIdMostBits, long requestId)
        {
            var ack = new CommandAck { ConsumerId = (ulong)consumerId, ack_type = ackType };
            ack.MessageIds.AddRange(messageIds);

            return NewAck(validationError, properties, txnIdLeastBits, txnIdMostBits, requestId, ack);
        }
        public static ReadOnlySequence<byte> NewAck(long consumerId, long ledgerId, long entryId, List<long> ackSets, CommandAck.AckType ackType, CommandAck.ValidationError? validationError, IDictionary<string, long> properties, long txnIdLeastBits, long txnIdMostBits, long requestId, int batchSize)
		{
            var ack = new CommandAck {ConsumerId = (ulong) consumerId, ack_type = ackType};
			
            var messageIdData = new MessageIdData {ledgerId = (ulong) ledgerId, entryId = (ulong) entryId};
            if (ackSets != null)
            {
                messageIdData.AckSets = ackSets;
            }
            ack.MessageIds.Add(messageIdData);
			if (batchSize >= 0)
			{
				messageIdData.BatchSize = batchSize;
			}
            return NewAck(validationError, properties, txnIdLeastBits, txnIdMostBits, requestId, ack);
        }
        private static ReadOnlySequence<byte> NewAck(ValidationError? validationError, IDictionary<string, long> properties, long txnIdLeastBits,
                                  long txnIdMostBits, long requestId, CommandAck ack)
        {
            if (validationError != null)    
                ack.validation_error = validationError.Value;

            if (txnIdMostBits >= 0)
            {
                ack.TxnidMostBits = (ulong) txnIdMostBits;
            }
            if (txnIdLeastBits >= 0)
            {
                ack.TxnidLeastBits = (ulong)txnIdLeastBits;
            }

            if (requestId >= 0)
            {
                ack.RequestId = (ulong)requestId;
            }
            foreach (var e in properties.ToList())
            {
                ack.Properties.Add(new KeyLongValue() { Key = e.Key, Value = (ulong)e.Value });
            }

            return Serializer.Serialize(ack.ToBaseCommand());
        }

        public static ReadOnlySequence<byte> NewFlow(long consumerId, int messagePermits)
		{
            var flow = new CommandFlow {ConsumerId = (ulong) consumerId, messagePermits = (uint) messagePermits};

            return Serializer.Serialize(flow.ToBaseCommand());
			
			
		}

		public static ReadOnlySequence<byte> NewRedeliverUnacknowledgedMessages(long consumerId)
		{
            var redeliver = new CommandRedeliverUnacknowledgedMessages {ConsumerId = (ulong) consumerId};
            return Serializer.Serialize(redeliver.ToBaseCommand());
			
			
		}

		public static ReadOnlySequence<byte> NewRedeliverUnacknowledgedMessages(long consumerId, IList<MessageIdData> messageIds)
		{
            var redeliver = new CommandRedeliverUnacknowledgedMessages {ConsumerId = (ulong) consumerId};
            redeliver.MessageIds.AddRange(messageIds);
			return Serializer.Serialize(redeliver.ToBaseCommand());
		    
		}

		public static ReadOnlySequence<byte> NewGetTopicsOfNamespaceRequest(string @namespace, long requestId, CommandGetTopicsOfNamespace.Mode mode, string topicsPattern, string topicsHash)
		{
            var topics = new CommandGetTopicsOfNamespace
            {
                Namespace = @namespace, RequestId = (ulong) requestId, mode = mode
            };
            if (topicsPattern != null)
            {
                topics.TopicsPattern = topicsPattern;
            }
            if (topicsHash != null)
            {
                topics.TopicsHash = topicsHash;
            }
            return Serializer.Serialize(topics.ToBaseCommand());
			
			
		}
        private static readonly ReadOnlySequence<byte> CmdPing;

		static Commands()
		{
			var serializedCmdPing = Serializer.Serialize(new CommandPing().ToBaseCommand());
			CmdPing = serializedCmdPing;
			var serializedCmdPong = Serializer.Serialize(new CommandPong().ToBaseCommand());
			CmdPong = serializedCmdPong;
		}

		internal static ReadOnlySequence<byte> NewPing()
		{
			return CmdPing;
		}

		private static readonly ReadOnlySequence<byte> CmdPong;


		internal static ReadOnlySequence<byte> NewPong()
		{
			return CmdPong;
		}

		public static ReadOnlySequence<byte> NewGetLastMessageId(long consumerId, long requestId)
		{
            var cmd = new CommandGetLastMessageId {ConsumerId = (ulong) consumerId, RequestId = (ulong) requestId};

            return Serializer.Serialize(cmd.ToBaseCommand());
			
			
		}

		public static ReadOnlySequence<byte> NewGetSchema(long requestId, string topic, ISchemaVersion version)
        {
            var schema = new CommandGetSchema {RequestId = (ulong) requestId, Topic = topic};
            if (version != null)
			{
				schema.SchemaVersion = version.Bytes();
			}
			
			return Serializer.Serialize(schema.ToBaseCommand());
			
			
		}

		public static ReadOnlySequence<byte> NewGetOrCreateSchema(long requestId, string topic, ISchemaInfo schemaInfo)
		{
            var getOrCreateSchema = new CommandGetOrCreateSchema
            {
                RequestId = (ulong) requestId, Topic = topic, Schema = ConvertSchema(schemaInfo)
            };
            
            return Serializer.Serialize(getOrCreateSchema.ToBaseCommand());
			
			
		}
		
		// ---- transaction related ----

		public static ReadOnlySequence<byte> NewTxn(long tcId, long requestId, long ttlSeconds)
		{
            var commandNewTxn = new CommandNewTxn
            {
                TcId = (ulong) tcId, RequestId = (ulong) requestId, TxnTtlSeconds = (ulong) ttlSeconds
            };
            return Serializer.Serialize(commandNewTxn.ToBaseCommand());
			
			
		}

		public static ReadOnlySequence<byte> NewAddPartitionToTxn(long requestId, long txnIdLeastBits, long txnIdMostBits, IList<string> partitions)
		{
            var commandAddPartitionToTxn = new CommandAddPartitionToTxn
            {
                RequestId = (ulong) requestId,
                TxnidLeastBits = (ulong) txnIdLeastBits,
                TxnidMostBits = (ulong) txnIdMostBits
            };
			if(partitions != null)
            {
				commandAddPartitionToTxn.Partitions.AddRange(partitions);
            }				
            return Serializer.Serialize(commandAddPartitionToTxn.ToBaseCommand());
			
			
		}

		public static ReadOnlySequence<byte> NewAddSubscriptionToTxn(long requestId, long txnIdLeastBits, long txnIdMostBits, IList<Subscription> subscription)
		{
            var commandAddSubscriptionToTxn = new CommandAddSubscriptionToTxn
            {
                RequestId = (ulong) requestId,
                TxnidLeastBits = (ulong) txnIdLeastBits,
                TxnidMostBits = (ulong) txnIdMostBits
            };
            commandAddSubscriptionToTxn.Subscriptions.AddRange(subscription);
			return Serializer.Serialize(commandAddSubscriptionToTxn.ToBaseCommand());
			
			
		}

		public static ReadOnlySequence<byte> NewEndTxn(long requestId, long txnIdLeastBits, long txnIdMostBits, TxnAction txnAction)
		{
            var commandEndTxn = new CommandEndTxn
            {
                RequestId = (ulong) requestId,
                TxnidLeastBits = (ulong) txnIdLeastBits,
                TxnidMostBits = (ulong) txnIdMostBits,
                TxnAction = txnAction
            };
            return Serializer.Serialize(commandEndTxn.ToBaseCommand());
			
			
		}

		public static ReadOnlySequence<byte> NewEndTxnOnPartition(long requestId, long txnIdLeastBits, long txnIdMostBits, string topic, TxnAction txnAction, long lowWaterMark)
		{
            var txnEndOnPartition = new CommandEndTxnOnPartition
            {
                RequestId = (ulong) requestId,
                TxnidLeastBits = (ulong) txnIdLeastBits,
                TxnidMostBits = (ulong) txnIdMostBits,
                Topic = topic,
                TxnAction = txnAction,
                TxnidLeastBitsOfLowWatermark = (ulong)lowWaterMark
            };
            return Serializer.Serialize(txnEndOnPartition.ToBaseCommand());
			
			
		}

		
		public static ReadOnlySequence<byte> NewEndTxnOnSubscription(long requestId, long txnIdLeastBits, long txnIdMostBits, Subscription subscription, TxnAction txnAction, long lowWaterMark)
		{
            var commandEndTxnOnSubscription = new CommandEndTxnOnSubscription
            {
                RequestId = (ulong) requestId,
                TxnidLeastBits = (ulong) txnIdLeastBits,
                TxnidMostBits = (ulong) txnIdMostBits,
                Subscription = subscription,
                TxnAction = txnAction,
                TxnidLeastBitsOfLowWatermark = (ulong)lowWaterMark
            };
            return Serializer.Serialize(commandEndTxnOnSubscription.ToBaseCommand());
			
			
		}
        
		public static int ComputeChecksum(  ReadOnlySequence<byte> byteBuffer)
        {
            return 9;//Crc32CIntChecksum.ComputeChecksum(byteBuffer);
        }
        public static int ResumeChecksum(int prev,   ReadOnlySequence<byte> byteBuffer)
        {
            return 9; //Crc32CIntChecksum.ResumeChecksum(prev, byteBuffer);
        }
		
		public static long InitBatchMessageMetadata(MessageMetadata builder)
		{
            var messageMetadata = new MessageMetadata
            {
                PublishTime = builder.PublishTime,
                ProducerName = builder.ProducerName,
                SequenceId =  builder.SequenceId
            };
            // Attach the key to the message metadata.
            if (builder.ShouldSerializePartitionKey())
            {
                messageMetadata.PartitionKey = builder.PartitionKey;
                messageMetadata.PartitionKeyB64Encoded = builder.PartitionKeyB64Encoded;
            }
            if (builder.ShouldSerializeOrderingKey())
            {
                messageMetadata.OrderingKey = builder.OrderingKey;
            }
            if (builder.ShouldSerializeReplicatedFrom())
			{
				messageMetadata.ReplicatedFrom = builder.ReplicatedFrom;
			}
			if (builder.ReplicateToes.Count > 0)
			{
				messageMetadata.ReplicateToes.AddRange(builder.ReplicateToes);
			}
			if (builder.ShouldSerializeSchemaVersion())
			{
				messageMetadata.SchemaVersion = builder.SchemaVersion;
			}
			return (long)builder.SequenceId;
		}

		public static SingleMessageMetadata SingleMessageMetadat(MessageMetadata msg, int payloadSize, long sequenceId)
		{

			// build single message meta-data
			var singleMessageMetadata = new SingleMessageMetadata 
			{
				PayloadSize = payloadSize
			};
			if (!string.IsNullOrWhiteSpace(msg.PartitionKey))
			{
				singleMessageMetadata.PartitionKey = msg.PartitionKey;
                singleMessageMetadata.PartitionKeyB64Encoded = msg.PartitionKeyB64Encoded;
			}
			if (msg.OrderingKey?.Length > 0)
			{
				singleMessageMetadata.OrderingKey = msg.OrderingKey;
			}
			if (msg.EventTime > 0)
			{
				singleMessageMetadata.EventTime = msg.EventTime;
			}
			if (msg.Properties.Count > 0)
			{
				singleMessageMetadata.Properties.AddRange(msg.Properties.ToList().Select(x => new KeyValue(){Key = x.Key, Value = x.Value}));
			}

            singleMessageMetadata.EventTime = msg.EventTime;
            singleMessageMetadata.SequenceId = (ulong)sequenceId;
			return singleMessageMetadata;
		}
		public static ReadOnlySequence<byte> SerializeSingleMessageInBatchWithPayload(SingleMessageMetadata singleMessageMetadata, ReadOnlySequence<byte> payload)
		{
			singleMessageMetadata.PayloadSize = (int)payload.Length;
			var metadataBytes = Serializer.GetBytes(singleMessageMetadata);
            var metadataSizeBytes = Serializer.ToBigEndianBytes((uint)metadataBytes.Length);
            try
            {
               return new SequenceBuilder<byte>()
                    .Append(metadataSizeBytes)
                    .Append(metadataBytes)
                    .Append(payload)
                    .Build();
               
            }
            catch (IOException e)
            {
                throw new Exception(e.Message, e);
            }
		}
		public static int CurrentProtocolVersion
		{
			get
			{
				var versions = Enum.GetValues(typeof(ProtocolVersion)).Length;
				// Return the last ProtocolVersion enum value
				return Enum.GetValues(typeof(ProtocolVersion)).Cast<int>().ToList()[versions - 1];
			}
		}

		/// <summary>
		/// Definition of possible checksum types.
		/// </summary>
		public enum ChecksumType
		{
			Crc32C,
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
		public static bool PeerSupportsAckReceipt(int peerVersion)
		{
			return peerVersion >= (int)ProtocolVersion.V17;
		}

		public static bool PeerSupportsMultiMessageAcknowledgment(int peerVersion)
		{
			return peerVersion >= (int)ProtocolVersion.V12;
		}

		public static bool PeerSupportAvroSchemaAvroFormat(int peerVersion)
		{
			return peerVersion >= (int)ProtocolVersion.V13;
		}

		public static bool PeerSupportsGetOrCreateSchema(int peerVersion)
		{
			return peerVersion >= (int)ProtocolVersion.V15;
		}
        private static Proto.ProducerAccessMode ConvertProducerAccessMode(Common.ProducerAccessMode accessMode)
        {
            switch (accessMode)
            {
                case Common.ProducerAccessMode.Exclusive:
                    return Proto.ProducerAccessMode.Exclusive;
                case Common.ProducerAccessMode.Shared:
                    return Proto.ProducerAccessMode.Shared;
                case Common.ProducerAccessMode.WaitForExclusive:
                    return Proto.ProducerAccessMode.WaitForExclusive;
                case Common.ProducerAccessMode.ExclusiveWithFencing:
                    return Proto.ProducerAccessMode.ExclusiveWithFencing;
                default:
                    throw new ArgumentException("Unknown access mode: " + accessMode);
            }
        }

        public static Common.ProducerAccessMode ConvertProducerAccessMode(Proto.ProducerAccessMode accessMode)
        {
            switch (accessMode)
            {
                case Proto.ProducerAccessMode.Exclusive:
                    return Common.ProducerAccessMode.Exclusive;
                case Proto.ProducerAccessMode.Shared:
                    return Common.ProducerAccessMode.Shared;
                case Proto.ProducerAccessMode.WaitForExclusive:
                    return Common.ProducerAccessMode.WaitForExclusive;
                case Proto.ProducerAccessMode.ExclusiveWithFencing:
                    return Common.ProducerAccessMode.ExclusiveWithFencing;
                default:
                    throw new ArgumentException("Unknown access mode: " + accessMode);
            }
        }

    }

}