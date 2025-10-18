using System.Buffers;
using SharpPulsar.Shared;
using System.Text;
using SharpPulsar.Common;
using SharpPulsar.API.Schema;
using SharpPulsar.Common.Helpers;
using Google.Protobuf;
using ProtoBuf;
using Serializer = SharpPulsar.Common.Helpers.Serializer;
//using SharpPulsar.Shared.Buf;
using Akka.Util.Internal;
using System;
using SharpPulsar.API;
using Pulsar.Proto;
using ProducerAccessMode = Pulsar.Proto.ProducerAccessMode;
using System.Buffers.Text;
using DotNetty.Common;
using DotNetty.Buffers;
using DotNetty.Codecs;


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
            return LOCAL_BASE_COMMAND.get().clear().setType(type);
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

        public static AbstractByteBuffer NewConnect(string authMethodName, string authData, string libVersion)
		{
			return NewConnect(authMethodName, authData, CurrentProtocolVersion, libVersion, null, null, null, null);
		}

		public static AbstractByteBuffer NewConnect(string authMethodName, string authData, string libVersion, string targetBroker)
		{
			return NewConnect(authMethodName, authData, CurrentProtocolVersion, libVersion, targetBroker, null, null, null);
		}
        
        public static AbstractByteBuffer NewConnect(string authMethodName, string authData, string libVersion, string targetBroker, 
            string originalPrincipal, string clientAuthData, string clientAuthMethod)
		{
			return NewConnect(authMethodName, authData, CurrentProtocolVersion, libVersion, targetBroker, originalPrincipal, clientAuthData, clientAuthMethod);
		}
        private static void SetFeatureFlags(FeatureFlags flags)
        {
            flags.SupportsAuthRefresh = true;
            flags.SupportsBrokerEntryMetadata = true;
            flags.SupportsPartialProducer = true;
            flags.SupportsGetPartitionedMetadataWithoutAutoCreation = true;
            flags.SupportsReplDedupByLidAndEid = true;
        }
        public static AbstractByteBuffer NewConnect(string authMethodName, string authData, int protocolVersion, string libVersion, string targetBroker, string originalPrincipal, string originalAuthData, string originalAuthMethod)
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
        public static AbstractByteBuffer NewTcClientConnectRequest(long tcId, long requestId)
        {
            var tcClientConnect = new CommandTcClientConnectRequest
            {
                TcId = (ulong)tcId,
                RequestId = (ulong)requestId
            };
            return Serializer.Serialize(tcClientConnect.ToBaseCommand());
        }
        public static AbstractByteBuffer NewConnect(string authMethodName, AuthData authData, int protocolVersion, 
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

        public static AbstractByteBuffer NewConnect(string authMethodName, AuthData authData, int protocolVersion,
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
        public static AbstractByteBuffer NewAuthResponse(string authMethod, AuthData clientData, int clientProtocolVersion, string clientVersion)
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
		public static AbstractByteBuffer NewAuthChallenge(string authMethod, AuthData brokerData, int clientProtocolVersion)
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
		
		public static AbstractByteBuffer NewSendError(long producerId, long sequenceId, ServerError error, string errorMsg)
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


		public static bool HasChecksum(AbstractByteBuffer buffer)
        {
            return buffer.GetShort(buffer.ReaderIndex) == MagicCrc32c;
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
                buffer.SkipBytes((ISchema<short>.Bytes.SchemaInfo.Schema.Length + ISchema<int>.Bytes.SchemaInfo.Schema.Length));
            }
            
		}
		
		public static MessageMetadata ParseMessageMetadata(AbstractByteBuffer buffer)
		{
			try
			{
                // initially reader-index may point to start of broker entry metadata :
                // increment reader-index to start_of_headAndPayload to parse metadata
                var skipped = SkipBrokerEntryMetadataIfExist(buffer);
                SkipChecksumIfPresent(buffer);
                var metadataSize = buffer.ReadInt();
                skipped.SkipBytes(metadataSize);
                return Serializer.Deserialize<MessageMetadata>(skipped);
            }
			catch (Exception e)
			{
				throw new Exception(e.Message, e);
			}
		}

        public static AbstractByteBuffer NewSend(long producerId, long sequenceId, int numMessaegs, ChecksumType checksumType, long ledgerId, long entryId, MessageMetadata messageMetadata, byte[] payload)
        {
            return NewSend(producerId, sequenceId, -1, numMessaegs, messageMetadata.ShouldSerializeTxnidLeastBits() ? (long)messageMetadata.TxnidLeastBits : -1, messageMetadata.ShouldSerializeTxnidMostBits() ? (long)messageMetadata.TxnidMostBits : -1, checksumType, ledgerId, entryId, messageMetadata, payload);
        }

        public static AbstractByteBuffer NewSend(long producerId, long sequenceId, int numMessaegs, ChecksumType checksumType, MessageMetadata messageMetadata, byte[] payload)
        {
            return NewSend(producerId, sequenceId, -1, numMessaegs, messageMetadata.ShouldSerializeTxnidLeastBits() ? (long)messageMetadata.TxnidLeastBits : -1, messageMetadata.ShouldSerializeTxnidMostBits() ? (long)messageMetadata.TxnidMostBits : -1, checksumType, -1, -1, messageMetadata, payload);
        }

        public static AbstractByteBuffer NewSend(long producerId, long lowestSequenceId, long highestSequenceId, int numMessaegs, ChecksumType checksumType, MessageMetadata messageMetadata, byte[] payload)
        {
            return NewSend(producerId, lowestSequenceId, highestSequenceId, numMessaegs, messageMetadata.ShouldSerializeTxnidLeastBits() ? (long)messageMetadata.TxnidLeastBits : -1, messageMetadata.ShouldSerializeTxnidMostBits() ? (long)messageMetadata.TxnidMostBits : -1, checksumType, -1, -1, messageMetadata, payload);
        }

        public static AbstractByteBuffer NewSend(long producerId, long sequenceId, long highestSequenceId, int numMessages, long txnIdLeastBits, long txnIdMostBits, ChecksumType checksumType, long ledgerId, long entryId, MessageMetadata messageData, byte[] payload)
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

		public static AbstractByteBuffer NewSubscribe(string topic, string subscription, long consumerId, long requestId, CommandSubscribe.SubType subType, int priorityLevel, string consumerName, long resetStartMessageBackInSeconds)
		{
			return NewSubscribe(topic, subscription, consumerId, requestId, subType, priorityLevel, consumerName, true, null, new Dictionary<string,string>(), false, false, CommandSubscribe.InitialPosition.Earliest, resetStartMessageBackInSeconds, null, true);
		}
		
		public static AbstractByteBuffer NewSubscribe(string topic, string subscription, long consumerId, long requestId, CommandSubscribe.SubType subType, int priorityLevel, string consumerName, bool isDurable, MessageIdData startMessageId, IDictionary<string, string> metadata, bool readCompacted, bool isReplicated, CommandSubscribe.InitialPosition subscriptionInitialPosition, long startMessageRollbackDurationInSec, ISchemaInfo schemaInfo, bool createTopicIfDoesNotExist)
		{
            return NewSubscribe(topic, subscription, consumerId, requestId, subType, priorityLevel, consumerName, isDurable, startMessageId, metadata, readCompacted, isReplicated, subscriptionInitialPosition, startMessageRollbackDurationInSec, schemaInfo, createTopicIfDoesNotExist, null, new Dictionary<string, string>(), DefaultConsumerEpoch);
		}

		public static AbstractByteBuffer NewSubscribe(string topic, string subscription, long consumerId, 
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
        public static AbstractByteBuffer NewWatchTopicList(long requestId, long watcherId, string @namespace, string topicsPattern, string topicsHash)
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

        public static AbstractByteBuffer NewWatchTopicListSuccess(long requestId, long watcherId, string topicsHash, IList<string> topics)
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

        
        public static long GetEntryTimestamp(AbstractByteBuffer headersAndPayloadWithBrokerEntryMetadata)
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

        private static KeySharedMode ConvertKeySharedMode(KeySharedMode? mode)
        {
            switch (mode)
            {
                case KeySharedMode.AutoSplit:
                    return KeySharedMode.AutoSplit;
                case KeySharedMode.Sticky:
                    return KeySharedMode.Sticky;
                default:
                    throw new ArgumentException("Unexpected key shared mode: " + mode);
            }
        }
		public static AbstractByteBuffer NewUnsubscribe(long consumerId, long requestId, bool force)
		{
            var unsubscribe = new CommandUnsubscribe
            {
                ConsumerId = (ulong) consumerId, 
                RequestId = (ulong) requestId,
                Force = force
            };
            return Serializer.Serialize(unsubscribe.ToBaseCommand());
			
		}

		public static AbstractByteBuffer NewActiveConsumerChange(long consumerId, bool isActive)
		{
            var change = new CommandActiveConsumerChange {ConsumerId = (ulong) consumerId, IsActive = isActive};
            return Serializer.Serialize(change.ToBaseCommand());
			
		}

		public static AbstractByteBuffer NewSeek(long consumerId, long requestId, long ledgerId, long entryId, List<long> ackSet)
		{
            var seek = new CommandSeek {ConsumerId = (ulong) consumerId, RequestId = (ulong) requestId};

            var messageId = new MessageIdData {ledgerId = (ulong) ledgerId, entryId = (ulong) entryId, AckSets = ackSet};
            seek.MessageId = messageId;
			return Serializer.Serialize(seek.ToBaseCommand());			
		}
        
        public static AbstractByteBuffer NewSeek(long consumerId, long requestId, long timestamp)
		{
            var seek = new CommandSeek
            {
                ConsumerId = (ulong) consumerId,
                RequestId = (ulong) requestId,
                MessagePublishTime = (ulong) timestamp
            };

            return Serializer.Serialize(seek.ToBaseCommand());

			
		}

		public static AbstractByteBuffer NewCloseConsumer(long consumerId, long requestId)
		{
            var closeConsumer = new CommandCloseConsumer
            {
                ConsumerId = (ulong) consumerId, RequestId = (ulong) requestId
            };
            return Serializer.Serialize(closeConsumer.ToBaseCommand());
			
			
		}

		public static AbstractByteBuffer NewReachedEndOfTopic(long consumerId)
		{
            var reachedEndOfTopic = new CommandReachedEndOfTopic {ConsumerId = (ulong) consumerId};
            return Serializer.Serialize(reachedEndOfTopic.ToBaseCommand());
			
			
		}

		public static AbstractByteBuffer NewCloseProducer(long producerId, long requestId)
		{
            var closeProducer = new CommandCloseProducer
            {
                ProducerId = (ulong) producerId, RequestId = (ulong) requestId
            };
            return Serializer.Serialize(closeProducer.ToBaseCommand());
			
			
		}

		public static AbstractByteBuffer NewProducer(string topic, long producerId, long requestId, string producerName, IDictionary<string, string> metadata, bool isTxnEnabled)
		{
			return NewProducer(topic, producerId, requestId, producerName, false, metadata, isTxnEnabled);
		}

		public static AbstractByteBuffer NewProducer(string topic, long producerId, long requestId, string producerName, bool encrypted, IDictionary<string, string> metadata, bool isTxnEnabled)
		{
			return NewProducer(topic, producerId, requestId, producerName, encrypted, metadata, null, 0, false, ProducerAccessMode.Shared, null, isTxnEnabled, null);
		}
        private static Type GetSchemaType(SchemaType type)
		{
            if (type == SchemaType.AutoConsume)
            {
                return Type.AutoConsume;
            }
            if (type.Value < 0)
			{
				return Type.None;
			}
            else if (type == SchemaType.External)
            {
                // This is a special case, SchemaType.EXTERNAL number is not match the Schema.Type.EXTERNAL.
                return Type.External;
            }
            else
			{
				return Enum.GetValues(typeof(Type)).Cast<Type>().ToList()[type.Value];
			}
		}

		public static SchemaType GetSchemaType(Type type)
		{
            if (type == Type.AutoConsume)
            {
                return SchemaType.AutoConsume;
            }
			else if (type < 0)
			{
				// this is unexpected
				return SchemaType.NONE;
			}
            else if (type == Type.External)
            {
                // This is a special case, SchemaType.EXTERNAL number is not match the Schema.Type.EXTERNAL.
                return SchemaType.External;
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

        private static Common.Protocol.Proto.Schema ConvertSchema(ISchemaInfo SchemaInfo)
        {
            var schema = new Common.Protocol.Proto.Schema
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

        public static AbstractByteBuffer NewProducer(string topic, long producerId, long requestId, string producerName, bool encrypted, IDictionary<string, string> metadata, ISchemaInfo schemaInfo, long epoch, bool userProvidedProducerName, Common.ProducerAccessMode accessMode, long? topicEpoch, bool isTxnEnabled, string initialSubscriptionName)
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

		public static AbstractByteBuffer NewPartitionMetadataRequest(string topic, long requestId, bool metadataAutoCreationEnabled = true)
		{
            var partitionMetadata = new CommandPartitionedTopicMetadata
            {
                Topic = topic, 
                RequestId = (ulong) requestId,
                MetadataAutoCreationEnabled = metadataAutoCreationEnabled   
            };
            return Serializer.Serialize(partitionMetadata.ToBaseCommand());
			
			
		}

		public static AbstractByteBuffer NewLookup(string topic, string listenerName, bool authoritative, long requestId)
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
		public static AbstractByteBuffer NewMultiTransactionMessageAck(long consumerId, TxnID txnID, IList<(long ledger, long entry, List<long> bitSet)> entries)
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
		public static AbstractByteBuffer NewMultiMessageAckCommon(CommandAck ackBuilder, IList<(long ledger, long entry, List<long> bitSet)> entries)
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
        public static AbstractByteBuffer NewMultiMessageAck(long consumerId, IList<(long LedgerId, long EntryId, List<long> Sets)> entries, long requestId)
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
        public static AbstractByteBuffer NewMultiMessageAck(long consumerId, IList<(long LedgerId, long EntryId, BitSet Sets)> entries)
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
        public static AbstractByteBuffer NewMultiMessageAck(long consumerId, IList<(long LedgerId, long EntryId, List<long> Sets)> entries)
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
        /// rather than <seealso cref="Commands.peekMessageMetadata(AbstractByteBuffer, string, long)"/>, which returns a thread local reference,
        /// see <seealso cref="Commands.LOCAL_MESSAGE_METADATA"/>.
        /// </summary>
        
        public static MessageMetadata PeekAndCopyMessageMetadata(AbstractByteBuffer metadataAndPayload, string subscription, long consumerId)
        {
            MessageMetadata localMetadata = PeekMessageMetadata(metadataAndPayload, subscription, consumerId);
            if (localMetadata == null)
            {
                return null;
            }

            return localMetadata;
        }
        public static MessageMetadata PeekMessageMetadata(AbstractByteBuffer metadataAndPayload, string subscription, long consumerId)
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

        
        public static AbstractByteBuffer PeekStickyKey(AbstractByteBuffer metadataAndPayload, string topic, string subscription)
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
                    return new AbstractByteBuffer(metadata.OrderingKey);
                }
                else if (metadata.ShouldSerializePartitionKey())
                {
                    if (metadata.ShouldSerializePartitionKeyB64Encoded())
                    {
                        metadata.PartitionKey = Convert.ToBase64String(Encoding.UTF8.GetBytes(metadata.PartitionKey));
                        return new AbstractByteBuffer(Encoding.UTF8.GetBytes(metadata.PartitionKey)); 
                    }

                    return new AbstractByteBuffer(Encoding.UTF8.GetBytes(metadata.PartitionKey));
                }
            }
            catch (Exception t)
            {
                throw new Exception($"[{topic}] [{subscription}] Failed to peek sticky key from the message metadata", t);
            }

            return new AbstractByteBuffer(NONE_KEY);
        }
        public static AbstractByteBuffer NewAck(long consumerId, long ledgerId, long entryId, List<long> ackSets, CommandAck.AckType ackType, CommandAck.ValidationError? validationError, IDictionary<string, long> properties)
		{
			return NewAck(consumerId, ledgerId, entryId, ackSets, ackType, validationError, properties, -1L, -1L, -1L, -1);
		}
        public static AbstractByteBuffer NewAck(long consumerId, long ledgerId, long entryId, List<long> ackSets, CommandAck.AckType ackType, CommandAck.ValidationError? validationError, IDictionary<string, long> properties, long requestId)
        {
            return NewAck(consumerId, ledgerId, entryId, ackSets, ackType, validationError, properties, -1L, -1L, requestId, -1);
        }
        public static AbstractByteBuffer NewAck(long consumerId, long ledgerId, long entryId, List<long> ackSet, CommandAck.AckType ackType, CommandAck.ValidationError? validationError, IDictionary<string, long> properties, long txnIdLeastBits, long txnIdMostBits, long requestId)
		{
			return NewAck(consumerId, ledgerId, entryId, ackSet, ackType, validationError,
					properties, txnIdLeastBits, txnIdMostBits, requestId, -1);
		}
        public static AbstractByteBuffer NewAck(long consumerId, IList<MessageIdData> messageIds, AckType ackType,
                                 ValidationError? validationError, IDictionary<string, long> properties, long txnIdLeastBits,
                                 long txnIdMostBits, long requestId)
        {
            var ack = new CommandAck { ConsumerId = (ulong)consumerId, ack_type = ackType };
            ack.MessageIds.AddRange(messageIds);

            return NewAck(validationError, properties, txnIdLeastBits, txnIdMostBits, requestId, ack);
        }
        public static AbstractByteBuffer NewAck(long consumerId, long ledgerId, long entryId, List<long> ackSets, CommandAck.AckType ackType, CommandAck.ValidationError? validationError, IDictionary<string, long> properties, long txnIdLeastBits, long txnIdMostBits, long requestId, int batchSize)
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
        private static AbstractByteBuffer NewAck(ValidationError? validationError, IDictionary<string, long> properties, long txnIdLeastBits,
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

        public static AbstractByteBuffer NewFlow(long consumerId, int messagePermits)
		{
            var flow = new CommandFlow {ConsumerId = (ulong) consumerId, messagePermits = (uint) messagePermits};

            return Serializer.Serialize(flow.ToBaseCommand());
			
			
		}

		public static AbstractByteBuffer NewRedeliverUnacknowledgedMessages(long consumerId)
		{
            var redeliver = new CommandRedeliverUnacknowledgedMessages {ConsumerId = (ulong) consumerId};
            return Serializer.Serialize(redeliver.ToBaseCommand());
			
			
		}

		public static AbstractByteBuffer NewRedeliverUnacknowledgedMessages(long consumerId, IList<MessageIdData> messageIds)
		{
            var redeliver = new CommandRedeliverUnacknowledgedMessages {ConsumerId = (ulong) consumerId};
            redeliver.MessageIds.AddRange(messageIds);
			return Serializer.Serialize(redeliver.ToBaseCommand());
		    
		}

		public static AbstractByteBuffer NewGetTopicsOfNamespaceRequest(string @namespace, long requestId, CommandGetTopicsOfNamespace.Mode mode, string topicsPattern, string topicsHash)
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
        private static readonly IByteBuffer CmdPing;

		static Commands()
		{
			var serializedCmdPing = Serializer.Serialize(new CommandPing().ToBaseCommand());
			CmdPing = serializedCmdPing;
			var serializedCmdPong = Serializer.Serialize(new CommandPong().ToBaseCommand());
			CmdPong = serializedCmdPong;
		}

		internal static AbstractByteBuffer NewPing()
		{
			return CmdPing;
		}

		private static readonly AbstractByteBuffer CmdPong;


		internal static AbstractByteBuffer NewPong()
		{
			return CmdPong;
		}

		public static AbstractByteBuffer NewGetLastMessageId(long consumerId, long requestId)
		{
            var cmd = new CommandGetLastMessageId {ConsumerId = (ulong) consumerId, RequestId = (ulong) requestId};

            return Serializer.Serialize(cmd.ToBaseCommand());
			
			
		}

		public static AbstractByteBuffer NewGetSchema(long requestId, string topic, ISchemaVersion version)
        {
            var schema = new CommandGetSchema {RequestId = (ulong) requestId, Topic = topic};
            if (version != null)
			{
				schema.SchemaVersion = version.Bytes();
			}
			
			return Serializer.Serialize(schema.ToBaseCommand());
			
			
		}

		public static AbstractByteBuffer NewGetOrCreateSchema(long requestId, string topic, ISchemaInfo schemaInfo)
		{
            var getOrCreateSchema = new CommandGetOrCreateSchema
            {
                RequestId = (ulong) requestId, Topic = topic, Schema = ConvertSchema(schemaInfo)
            };
            
            return Serializer.Serialize(getOrCreateSchema.ToBaseCommand());
			
			
		}
		
		// ---- transaction related ----

		public static AbstractByteBuffer NewTxn(long tcId, long requestId, long ttlSeconds)
		{
            var commandNewTxn = new CommandNewTxn
            {
                TcId = (ulong) tcId, RequestId = (ulong) requestId, TxnTtlSeconds = (ulong) ttlSeconds
            };
            return Serializer.Serialize(commandNewTxn.ToBaseCommand());
			
			
		}
        public static AbstractByteBuffer newConnect(string authMethodName, string authData, string libVersion)
        {
            return newConnect(authMethodName, authData, CurrentProtocolVersion, libVersion, null, null, null, null);
        }

        public static AbstractByteBuffer newConnect(string authMethodName, string authData, string libVersion, string targetBroker)
        {
            return newConnect(authMethodName, authData, CurrentProtocolVersion, libVersion, targetBroker, null, null, null);
        }

        public static AbstractByteBuffer newConnect(string authMethodName, string authData, string libVersion, string targetBroker, string originalPrincipal, string clientAuthData, string clientAuthMethod)
        {
            return newConnect(authMethodName, authData, CurrentProtocolVersion, libVersion, targetBroker, originalPrincipal, clientAuthData, clientAuthMethod);
        }

        private static FeatureFlags FeatureFlags
        {
            set
            {
                value.setSupportsAuthRefresh(true);
                value.setSupportsBrokerEntryMetadata(true);
                value.setSupportsPartialProducer(true);
                value.setSupportsGetPartitionedMetadataWithoutAutoCreation(true);
                value.setSupportsReplDedupByLidAndEid(true);
            }
        }

        public static AbstractByteBuffer newConnect(string authMethodName, string authData, int protocolVersion, string libVersion, string targetBroker, string originalPrincipal, string originalAuthData, string originalAuthMethod)
        {
            BaseCommand cmd = localCmd(BaseCommand.Type.CONNECT);
            CommandConnect connect = cmd.setConnect().setClientVersion(!string.ReferenceEquals(libVersion, null) ? libVersion : "Pulsar Client").setAuthMethodName(authMethodName);

            if ("ycav1".Equals(authMethodName))
            {
                // Handle the case of a client that gets updated before the broker and starts sending the string auth method
                // name. An example would be in broker-to-broker replication. We need to make sure the clients are still
                // passing both the enum and the string until all brokers are upgraded.
                connect.setAuthMethod(AuthMethod.AuthMethodYcaV1);
            }

            if (!string.ReferenceEquals(targetBroker, null))
            {
                // When connecting through a proxy, we need to specify which broker do we want to be proxied through
                connect.setProxyToBrokerUrl(targetBroker);
            }

            if (!string.ReferenceEquals(authData, null))
            {
                connect.setAuthData(authData.GetBytes(UTF_8));
            }

            if (!string.ReferenceEquals(originalPrincipal, null))
            {
                connect.setOriginalPrincipal(originalPrincipal);
            }

            if (!string.ReferenceEquals(originalAuthData, null))
            {
                connect.setOriginalAuthData(originalAuthData);
            }

            if (!string.ReferenceEquals(originalAuthMethod, null))
            {
                connect.setOriginalAuthMethod(originalAuthMethod);
            }
            connect.setProtocolVersion(protocolVersion);

            setFeatureFlags(connect.setFeatureFlags());
            return serializeWithSize(cmd);
        }

        public static AbstractByteBuffer newConnect(string authMethodName, AuthData authData, int protocolVersion, string libVersion, string targetBroker, string originalPrincipal, AuthData originalAuthData, string originalAuthMethod)
        {
            return newConnect(authMethodName, authData, protocolVersion, libVersion, targetBroker, originalPrincipal, originalAuthData, originalAuthMethod, null, null);
        }

        public static AbstractByteBuffer newConnect(string authMethodName, AuthData authData, int protocolVersion, string libVersion, string targetBroker, string originalPrincipal, AuthData originalAuthData, string originalAuthMethod, string proxyVersion, FeatureFlags featureFlags)
        {
            BaseCommand cmd = newConnectWithoutSerialize(authMethodName, authData, protocolVersion, libVersion, targetBroker, originalPrincipal, originalAuthData, originalAuthMethod, proxyVersion, featureFlags);
            return serializeWithSize(cmd);
        }

        public static BaseCommand newConnectWithoutSerialize(string authMethodName, AuthData authData, int protocolVersion, string libVersion, string targetBroker, string originalPrincipal, AuthData originalAuthData, string originalAuthMethod, string proxyVersion, FeatureFlags featureFlags)
        {
            BaseCommand cmd = localCmd(BaseCommand.Type.CONNECT);
            CommandConnect connect = cmd.setConnect().setClientVersion(!string.ReferenceEquals(libVersion, null) ? libVersion : "Pulsar Client").setAuthMethodName(authMethodName);

            if (!string.ReferenceEquals(proxyVersion, null))
            {
                connect.setProxyVersion(proxyVersion);
            }

            if (!string.ReferenceEquals(targetBroker, null))
            {
                // When connecting through a proxy, we need to specify which broker do we want to be proxied through
                connect.setProxyToBrokerUrl(targetBroker);
            }

            if (authData != null)
            {
                connect.setAuthData(authData.getBytes());
            }

            if (!string.ReferenceEquals(originalPrincipal, null))
            {
                connect.setOriginalPrincipal(originalPrincipal);
            }

            if (originalAuthData != null)
            {
                connect.setOriginalAuthData(new string(originalAuthData.getBytes(), UTF_8));
            }

            if (!string.ReferenceEquals(originalAuthMethod, null))
            {
                connect.setOriginalAuthMethod(originalAuthMethod);
            }
            connect.setProtocolVersion(protocolVersion);
            if (featureFlags != null)
            {
                connect.setFeatureFlags().copyFrom(featureFlags);
            }
            else
            {
                setFeatureFlags(connect.setFeatureFlags());
            }

            return cmd;
        }

        public static AbstractByteBuffer newConnected(int clientProtocoVersion, bool supportsTopicWatchers)
        {
            return newConnected(clientProtocoVersion, INVALID_MAX_MESSAGE_SIZE, supportsTopicWatchers);
        }

        public static BaseCommand newConnectedCommand(int clientProtocolVersion, int maxMessageSize, bool supportsTopicWatchers)
        {
            BaseCommand cmd = localCmd(BaseCommand.Type.CONNECTED);
            CommandConnected connected = cmd.setConnected().setServerVersion("Pulsar Server" + PulsarVersion.getVersion());

            if (INVALID_MAX_MESSAGE_SIZE != maxMessageSize)
            {
                connected.setMaxMessageSize(maxMessageSize);
            }

            // If the broker supports a newer version of the protocol, it will anyway advertise the max version that the
            // client supports, to avoid confusing the client.
            int currentProtocolVersion = CurrentProtocolVersion;
            int versionToAdvertise = Math.Min(currentProtocolVersion, clientProtocolVersion);

            connected.setProtocolVersion(versionToAdvertise);

            connected.setFeatureFlags().setSupportsTopicWatchers(supportsTopicWatchers);
            connected.setFeatureFlags().setSupportsGetPartitionedMetadataWithoutAutoCreation(true);
            connected.setFeatureFlags().setSupportsReplDedupByLidAndEid(true);
            return cmd;
        }

        public static AbstractByteBuffer newConnected(int clientProtocolVersion, int maxMessageSize, bool supportsTopicWatchers)
        {
            return serializeWithSize(newConnectedCommand(clientProtocolVersion, maxMessageSize, supportsTopicWatchers));
        }

        public static AbstractByteBuffer newAuthChallenge(string authMethod, AuthData brokerData, int clientProtocolVersion)
        {
            BaseCommand cmd = localCmd(BaseCommand.Type.AUTH_CHALLENGE);
            CommandAuthChallenge challenge = cmd.setAuthChallenge();

            // If the broker supports a newer version of the protocol, it will anyway advertise the max version that the
            // client supports, to avoid confusing the client.
            int currentProtocolVersion = CurrentProtocolVersion;
            int versionToAdvertise = Math.Min(currentProtocolVersion, clientProtocolVersion);

            challenge.setProtocolVersion(versionToAdvertise).setChallenge().setAuthData(brokerData != null ? brokerData.getBytes() : new sbyte[0]).setAuthMethodName(authMethod);
            return serializeWithSize(cmd);
        }

        public static AbstractByteBuffer newAuthResponse(string authMethod, AuthData clientData, int clientProtocolVersion, string clientVersion)
        {
            BaseCommand cmd = localCmd(BaseCommand.Type.AUTH_RESPONSE);
            cmd.setAuthResponse().setClientVersion(!string.ReferenceEquals(clientVersion, null) ? clientVersion : "Pulsar Client").setProtocolVersion(clientProtocolVersion).setResponse().setAuthData(clientData.getBytes()).setAuthMethodName(authMethod);
            return serializeWithSize(cmd);
        }

        public static BaseCommand newSuccessCommand(long requestId)
        {
            BaseCommand cmd = localCmd(BaseCommand.Type.SUCCESS);
            cmd.setSuccess().setRequestId(requestId);
            return cmd;
        }

        public static AbstractByteBuffer newSuccess(long requestId)
        {
            return serializeWithSize(newSuccessCommand(requestId));
        }

        public static BaseCommand newProducerSuccessCommand(long requestId, string producerName, SchemaVersion schemaVersion)
        {
            return newProducerSuccessCommand(requestId, producerName, -1, schemaVersion, null, true);
        }

        public static AbstractByteBuffer newProducerSuccess(long requestId, string producerName, SchemaVersion schemaVersion)
        {
            return newProducerSuccess(requestId, producerName, -1, schemaVersion, null, true);
        }

        public static BaseCommand newProducerSuccessCommand(long requestId, string producerName, long lastSequenceId, SchemaVersion schemaVersion, long? topicEpoch, bool isProducerReady)
        {
            BaseCommand cmd = localCmd(BaseCommand.Type.PRODUCER_SUCCESS);
            CommandProducerSuccess ps = cmd.setProducerSuccess().setRequestId(requestId).setProducerName(producerName).setLastSequenceId(lastSequenceId).setSchemaVersion(schemaVersion.bytes()).setProducerReady(isProducerReady);
            topicEpoch.ifPresent(ps.setTopicEpoch);
            return cmd;
        }

        public static AbstractByteBuffer newProducerSuccess(long requestId, string producerName, long lastSequenceId, SchemaVersion schemaVersion, long? topicEpoch, bool isProducerReady)
        {
            return serializeWithSize(newProducerSuccessCommand(requestId, producerName, lastSequenceId, schemaVersion, topicEpoch, isProducerReady));
        }

        public static BaseCommand newErrorCommand(long requestId, ServerError serverError, string message)
        {
            BaseCommand cmd = localCmd(BaseCommand.Type.ERROR);
            cmd.setError().setRequestId(requestId).setError(serverError).setMessage(!string.ReferenceEquals(message, null) ? message : "");
            return cmd;
        }

        public static AbstractByteBuffer newError(long requestId, ServerError serverError, string message)
        {
            return serializeWithSize(newErrorCommand(requestId, serverError, message));
        }

        public static BaseCommand newSendReceiptCommand(long producerId, long sequenceId, long highestId, long ledgerId, long entryId)
        {
            BaseCommand cmd = localCmd(BaseCommand.Type.SEND_RECEIPT);
            cmd.setSendReceipt().setProducerId(producerId).setSequenceId(sequenceId).setHighestSequenceId(highestId).setMessageId().setLedgerId(ledgerId).setEntryId(entryId);
            return cmd;
        }

        public static AbstractByteBuffer newSendReceipt(long producerId, long sequenceId, long highestId, long ledgerId, long entryId)
        {
            return serializeWithSize(newSendReceiptCommand(producerId, sequenceId, highestId, ledgerId, entryId));
        }

        public static BaseCommand newSendErrorCommand(long producerId, long sequenceId, ServerError error, string errorMsg)
        {
            BaseCommand cmd = localCmd(BaseCommand.Type.SEND_ERROR);
            cmd.setSendError().setProducerId(producerId).setSequenceId(sequenceId).setError(error).setMessage(!string.ReferenceEquals(errorMsg, null) ? errorMsg : "");
            return cmd;
        }

        public static AbstractByteBuffer newSendError(long producerId, long sequenceId, ServerError error, string errorMsg)
        {
            return serializeWithSize(newSendErrorCommand(producerId, sequenceId, error, errorMsg));
        }

        public static bool hasChecksum(AbstractByteBuffer buffer)
        {
            return buffer.getShort(buffer.readerIndex()) == magicCrc32c;
        }

        /// <summary>
        /// Read the checksum and advance the reader index in the buffer.
        /// 
        /// <para>Note: This method assume the checksum presence was already verified before.
        /// </para>
        /// </summary>
        public static int readChecksum(AbstractByteBuffer buffer)
        {
            buffer.skipBytes(2); //skip magic bytes
            return buffer.readInt();
        }

        public static void skipChecksumIfPresent(AbstractByteBuffer buffer)
        {
            if (hasChecksum(buffer))
            {
                buffer.skipBytes(Short.BYTES + Integer.BYTES);
            }
        }

        public static MessageMetadata parseMessageMetadata(AbstractByteBuffer buffer)
        {
            MessageMetadata md = LOCAL_MESSAGE_METADATA.get();
            parseMessageMetadata(buffer, md);
            return md;
        }

        public static void parseMessageMetadata(AbstractByteBuffer buffer, MessageMetadata msgMetadata)
        {
            // initially reader-index may point to start of broker entry metadata :
            // increment reader-index to start_of_headAndPayload to parse metadata
            skipBrokerEntryMetadataIfExist(buffer);
            // initially reader-index may point to start_of_checksum : increment reader-index to start_of_metadata
            // to parse metadata
            skipChecksumIfPresent(buffer);
            int metadataSize = (int)buffer.readUnsignedInt();

            msgMetadata.parseFrom(buffer, metadataSize);
        }

        public static void skipMessageMetadata(AbstractByteBuffer buffer)
        {
            // initially reader-index may point to start_of_checksum : increment reader-index to start_of_metadata to parse
            // metadata
            skipBrokerEntryMetadataIfExist(buffer);
            skipChecksumIfPresent(buffer);
            int metadataSize = (int)buffer.readUnsignedInt();
            buffer.skipBytes(metadataSize);
        }

        /// <summary>
        /// Gets the entry timestamp from either broker metadata broker timestamp or the message metadata publish time.
        /// Prefer using Managed Ledger's Entry's getEntryTimestamp() method over this method. </summary>
        /// <param name="headersAndPayloadWithBrokerEntryMetadata"> headers and payload for the message </param>
        /// <returns> the entry timestamp </returns>
        public static long getEntryTimestamp(AbstractByteBuffer headersAndPayloadWithBrokerEntryMetadata)
        {
            // get broker timestamp first if BrokerEntryMetadata is enabled with AppendBrokerTimestampMetadataInterceptor
            return peekBrokerEntryMetadataToLong(headersAndPayloadWithBrokerEntryMetadata, brokerEntryMetadata =>
            {
                if (brokerEntryMetadata != null && brokerEntryMetadata.hasBrokerTimestamp())
                {
                    return brokerEntryMetadata.getBrokerTimestamp();
                }
                // otherwise get the publish_time
                return parseMessageMetadata(headersAndPayloadWithBrokerEntryMetadata).getPublishTime();
            });
        }

        public static BaseCommand newMessageCommand(long consumerId, long ledgerId, long entryId, int partition, int redeliveryCount, long[] ackSet, long consumerEpoch)
        {
            BaseCommand cmd = localCmd(BaseCommand.Type.MESSAGE);
            CommandMessage msg = cmd.setMessage().setConsumerId(consumerId);
            msg.setMessageId().setLedgerId(ledgerId).setEntryId(entryId).setPartition(partition);

            // consumerEpoch > -1 is useful
            if (consumerEpoch > DEFAULT_CONSUMER_EPOCH)
            {
                msg.setConsumerEpoch(consumerEpoch);
            }
            if (redeliveryCount > 0)
            {
                msg.setRedeliveryCount(redeliveryCount);
            }
            if (ackSet != null)
            {
                for (int i = 0; i < ackSet.Length; i++)
                {
                    msg.addAckSet(ackSet[i]);
                }
            }
            return cmd;
        }

        public static ByteBufPair newMessage(long consumerId, long ledgerId, long entryId, int partition, int redeliveryCount, AbstractByteBuffer metadataAndPayload, long[] ackSet)
        {
            return serializeCommandMessageWithSize(newMessageCommand(consumerId, ledgerId, entryId, partition, redeliveryCount, ackSet, DEFAULT_CONSUMER_EPOCH), metadataAndPayload);
        }

        public static ByteBufPair newSend(long producerId, long sequenceId, int numMessages, ChecksumType checksumType, long ledgerId, long entryId, MessageMetadata messageMetadata, AbstractByteBuffer payload)
        {
            return newSend(producerId, sequenceId, -1, numMessages, messageMetadata.hasTxnidLeastBits() ? messageMetadata.getTxnidLeastBits() : -1, messageMetadata.hasTxnidMostBits() ? messageMetadata.getTxnidMostBits() : -1, checksumType, ledgerId, entryId, messageMetadata, payload);
        }

        public static ByteBufPair newSend(long producerId, long sequenceId, int numMessages, ChecksumType checksumType, MessageMetadata messageMetadata, AbstractByteBuffer payload)
        {
            return newSend(producerId, sequenceId, -1, numMessages, messageMetadata.hasTxnidLeastBits() ? messageMetadata.getTxnidLeastBits() : -1, messageMetadata.hasTxnidMostBits() ? messageMetadata.getTxnidMostBits() : -1, checksumType, -1, -1, messageMetadata, payload);
        }

        public static ByteBufPair newSend(long producerId, long lowestSequenceId, long highestSequenceId, int numMessages, ChecksumType checksumType, MessageMetadata messageMetadata, AbstractByteBuffer payload)
        {
            return newSend(producerId, lowestSequenceId, highestSequenceId, numMessages, messageMetadata.hasTxnidLeastBits() ? messageMetadata.getTxnidLeastBits() : -1, messageMetadata.hasTxnidMostBits() ? messageMetadata.getTxnidMostBits() : -1, checksumType, -1, -1, messageMetadata, payload);
        }

        public static ByteBufPair newSend(long producerId, long sequenceId, long highestSequenceId, int numMessages, long txnIdLeastBits, long txnIdMostBits, ChecksumType checksumType, long ledgerId, long entryId, MessageMetadata messageData, AbstractByteBuffer payload)
        {
            BaseCommand cmd = localCmd(BaseCommand.Type.SEND);
            CommandSend send = cmd.setSend().setProducerId(producerId).setSequenceId(sequenceId);
            if (highestSequenceId >= 0)
            {
                send.setHighestSequenceId(highestSequenceId);
            }
            if (numMessages > 1)
            {
                send.setNumMessages(numMessages);
            }
            if (txnIdLeastBits >= 0)
            {
                send.setTxnidLeastBits(txnIdLeastBits);
            }
            if (txnIdMostBits >= 0)
            {
                send.setTxnidMostBits(txnIdMostBits);
            }
            if (messageData.hasTotalChunkMsgSize() && messageData.getTotalChunkMsgSize() > 1)
            {
                send.setIsChunk(true);
            }

            if (messageData.hasMarkerType())
            {
                send.setMarker(true);
            }

            if (ledgerId >= 0 && entryId >= 0)
            {
                send.setMessageId().setLedgerId(ledgerId).setEntryId(entryId);
            }

            return serializeCommandSendWithSize(cmd, checksumType, messageData, payload);
        }

        public static AbstractByteBuffer newSubscribe(string topic, string subscription, long consumerId, long requestId, CommandSubscribe.SubType subType, int priorityLevel, string consumerName, long resetStartMessageBackInSeconds)
        {
            return newSubscribe(topic, subscription, consumerId, requestId, subType, priorityLevel, consumerName, true, null, Collections.emptyMap(), false, false, CommandSubscribe.InitialPosition.Earliest, resetStartMessageBackInSeconds, null, true);
        }

        public static AbstractByteBuffer newSubscribe(string topic, string subscription, long consumerId, long requestId, CommandSubscribe.SubType subType, int priorityLevel, string consumerName, bool isDurable, MessageIdData startMessageId, IDictionary<string, string> metadata, bool readCompacted, bool? isReplicated, CommandSubscribe.InitialPosition subscriptionInitialPosition, long startMessageRollbackDurationInSec, SchemaInfo schemaInfo, bool createTopicIfDoesNotExist)
        {
            return newSubscribe(topic, subscription, consumerId, requestId, subType, priorityLevel, consumerName, isDurable, startMessageId, metadata, readCompacted, isReplicated, subscriptionInitialPosition, startMessageRollbackDurationInSec, schemaInfo, createTopicIfDoesNotExist, null, Collections.emptyMap(), DEFAULT_CONSUMER_EPOCH);
        }

        public static AbstractByteBuffer newSubscribe(string topic, string subscription, long consumerId, long requestId, CommandSubscribe.SubType subType, int priorityLevel, string consumerName, bool isDurable, MessageIdData startMessageId, IDictionary<string, string> metadata, bool readCompacted, bool? isReplicated, CommandSubscribe.InitialPosition subscriptionInitialPosition, long startMessageRollbackDurationInSec, SchemaInfo schemaInfo, bool createTopicIfDoesNotExist, KeySharedPolicy keySharedPolicy, IDictionary<string, string> subscriptionProperties, long consumerEpoch)
        {
            BaseCommand cmd = localCmd(BaseCommand.Type.SUBSCRIBE);
            CommandSubscribe subscribe = cmd.setSubscribe().setTopic(topic).setSubscription(subscription).setSubType(subType).setConsumerId(consumerId).setConsumerName(consumerName).setRequestId(requestId).setPriorityLevel(priorityLevel).setDurable(isDurable).setReadCompacted(readCompacted).setInitialPosition(subscriptionInitialPosition).setForceTopicCreation(createTopicIfDoesNotExist).setConsumerEpoch(consumerEpoch);
            if (isReplicated != null)
            {
                subscribe.setReplicateSubscriptionState(isReplicated);
            }

            if (subscriptionProperties != null && subscriptionProperties.Count > 0)
            {
                IList<KeyValue> keyValues = new List<KeyValue>();
                subscriptionProperties.forEach((key, value) =>
                {
                    KeyValue keyValue = new KeyValue();
                    keyValue.setKey(key);
                    keyValue.setValue(value);
                    keyValues.Add(keyValue);
                });
                subscribe.addAllSubscriptionProperties(keyValues);
            }

            if (keySharedPolicy != null)
            {
                KeySharedMeta keySharedMeta = subscribe.setKeySharedMeta();
                keySharedMeta.setAllowOutOfOrderDelivery(keySharedPolicy.isAllowOutOfOrderDelivery());
                keySharedMeta.setKeySharedMode(convertKeySharedMode(keySharedPolicy.getKeySharedMode()));

                if (keySharedPolicy is KeySharedPolicy.KeySharedPolicySticky)
                {
                    IList<Range> ranges = ((KeySharedPolicy.KeySharedPolicySticky)keySharedPolicy).getRanges();
                    foreach (Range range in ranges)
                    {
                        IntRange r = keySharedMeta.addHashRange();
                        r.setStart(range.getStart());
                        r.setEnd(range.getEnd());
                    }
                }
            }

            if (startMessageId != null)
            {
                subscribe.setStartMessageId().copyFrom(startMessageId);
            }
            if (startMessageRollbackDurationInSec > 0)
            {
                subscribe.setStartMessageRollbackDurationSec(startMessageRollbackDurationInSec);
            }

            if (metadata.Count > 0)
            {
                metadata.SetOfKeyValuePairs().forEach(e => subscribe.addMetadata().setKey(e.getKey()).setValue(e.getValue()));
            }

            if (schemaInfo != null)
            {
                if (subscribe.hasSchema())
                {
                    throw new System.InvalidOperationException();
                }

                if (subscribe.setSchema().getPropertiesCount() > 0)
                {
                    throw new System.InvalidOperationException();
                }

                convertSchema(schemaInfo, subscribe.setSchema());
            }

            return serializeWithSize(cmd);
        }

        public static AbstractByteBuffer newTcClientConnectRequest(long tcId, long requestId)
        {
            BaseCommand cmd = localCmd(BaseCommand.Type.TC_CLIENT_CONNECT_REQUEST);
            cmd.setTcClientConnectRequest().setTcId(tcId).setRequestId(requestId);
            return serializeWithSize(cmd);
        }

        private static KeySharedMode convertKeySharedMode(org.apache.pulsar.client.api.KeySharedMode mode)
        {
            switch (mode)
            {
                case AUTO_SPLIT:
                    return KeySharedMode.AUTO_SPLIT;
                case STICKY:
                    return KeySharedMode.STICKY;
                default:
                    throw new System.ArgumentException("Unexpected key shared mode: " + mode);
            }
        }

        public static AbstractByteBuffer newUnsubscribe(long consumerId, long requestId, bool force)
        {
            BaseCommand cmd = localCmd(BaseCommand.Type.UNSUBSCRIBE);
            cmd.setUnsubscribe().setConsumerId(consumerId).setRequestId(requestId).setForce(force);
            return serializeWithSize(cmd);
        }

        public static AbstractByteBuffer newActiveConsumerChange(long consumerId, bool isActive)
        {
            BaseCommand cmd = localCmd(BaseCommand.Type.ACTIVE_CONSUMER_CHANGE);
            cmd.setActiveConsumerChange().setConsumerId(consumerId).setIsActive(isActive);
            return serializeWithSize(cmd);
        }

        public static AbstractByteBuffer newSeek(long consumerId, long requestId, long ledgerId, long entryId, long[] ackSet)
        {
            BaseCommand cmd = localCmd(BaseCommand.Type.SEEK);
            CommandSeek seek = cmd.setSeek().setConsumerId(consumerId).setRequestId(requestId);
            MessageIdData messageId = seek.setMessageId().setLedgerId(ledgerId).setEntryId(entryId);
            for (int i = 0; i < ackSet.Length; i++)
            {
                messageId.addAckSet(ackSet[i]);
            }
            return serializeWithSize(cmd);
        }

        public static AbstractByteBuffer newSeek(long consumerId, long requestId, long timestamp)
        {
            BaseCommand cmd = localCmd(BaseCommand.Type.SEEK);
            cmd.setSeek().setConsumerId(consumerId).setRequestId(requestId).setMessagePublishTime(timestamp);
            return serializeWithSize(cmd);
        }

        public static AbstractByteBuffer newCloseConsumer(long consumerId, long requestId, string assignedBrokerUrl, string assignedBrokerUrlTls)
        {
            BaseCommand cmd = localCmd(BaseCommand.Type.CLOSE_CONSUMER);
            CommandCloseConsumer commandCloseConsumer = cmd.setCloseConsumer().setConsumerId(consumerId).setRequestId(requestId);

            if (!string.ReferenceEquals(assignedBrokerUrl, null))
            {
                commandCloseConsumer.setAssignedBrokerServiceUrl(assignedBrokerUrl);
            }

            if (!string.ReferenceEquals(assignedBrokerUrlTls, null))
            {
                commandCloseConsumer.setAssignedBrokerServiceUrlTls(assignedBrokerUrlTls);
            }

            return serializeWithSize(cmd);
        }

        public static AbstractByteBuffer newReachedEndOfTopic(long consumerId)
        {
            BaseCommand cmd = localCmd(BaseCommand.Type.REACHED_END_OF_TOPIC);
            cmd.setReachedEndOfTopic().setConsumerId(consumerId);
            return serializeWithSize(cmd);
        }

        public static AbstractByteBuffer newTopicMigrated(CommandTopicMigrated.ResourceType type, long resourceId, string brokerUrl, string brokerUrlTls)
        {
            BaseCommand cmd = localCmd(BaseCommand.Type.TOPIC_MIGRATED);
            CommandTopicMigrated migratedCmd = cmd.setTopicMigrated();
            migratedCmd.setResourceType(type).setResourceId(resourceId);
            if (StringUtils.isNotBlank(brokerUrl))
            {
                migratedCmd.setBrokerServiceUrl(brokerUrl);
            }
            if (StringUtils.isNotBlank(brokerUrlTls))
            {
                migratedCmd.setBrokerServiceUrlTls(brokerUrlTls);
            }
            return serializeWithSize(cmd);
        }

        public static AbstractByteBuffer newCloseProducer(long producerId, long requestId)
        {
            return newCloseProducer(producerId, requestId, null, null);
        }

        public static AbstractByteBuffer newCloseProducer(long producerId, long requestId, string assignedBrokerUrl, string assignedBrokerUrlTls)
        {
            BaseCommand cmd = localCmd(BaseCommand.Type.CLOSE_PRODUCER);
            CommandCloseProducer commandCloseProducer = cmd.setCloseProducer().setProducerId(producerId).setRequestId(requestId);

            if (!string.ReferenceEquals(assignedBrokerUrl, null))
            {
                commandCloseProducer.setAssignedBrokerServiceUrl(assignedBrokerUrl);
            }

            if (!string.ReferenceEquals(assignedBrokerUrlTls, null))
            {
                commandCloseProducer.setAssignedBrokerServiceUrlTls(assignedBrokerUrlTls);
            }

            return serializeWithSize(cmd);
        }

        public static AbstractByteBuffer newProducer(string topic, long producerId, long requestId, string producerName, IDictionary<string, string> metadata, bool isTxnEnabled)
        {
            return newProducer(topic, producerId, requestId, producerName, false, metadata, isTxnEnabled);
        }

        public static AbstractByteBuffer newProducer(string topic, long producerId, long requestId, string producerName, bool encrypted, IDictionary<string, string> metadata, bool isTxnEnabled)
        {
            return newProducer(topic, producerId, requestId, producerName, encrypted, metadata, null, 0, false, ProducerAccessMode.Shared, null, isTxnEnabled);
        }

        private static Schema.Type getSchemaType(SchemaType type)
        {
            if (type == SchemaType.AUTO_CONSUME)
            {
                return Schema.Type.AutoConsume;
            }
            else if (type.getValue() < 0)
            {
                return Schema.Type.None;
            }
            else if (type == SchemaType.EXTERNAL)
            {
                // This is a special case, SchemaType.EXTERNAL number is not match the Schema.Type.EXTERNAL.
                return Schema.Type.External;
            }
            else
            {
                return Schema.Type.valueOf(type.getValue());
            }
        }

        public static SchemaType getSchemaType(Schema.Type type)
        {
            if (type == Schema.Type.AutoConsume)
            {
                return SchemaType.AUTO_CONSUME;
            }
            else if (type.getValue() < 0)
            {
                // this is unexpected
                return SchemaType.NONE;
            }
            else if (type == Schema.Type.External)
            {
                // This is a special case, SchemaType.EXTERNAL number is not match the Schema.Type.EXTERNAL.
                return SchemaType.EXTERNAL;
            }
            else
            {
                return SchemaType.valueOf(type.getValue());
            }
        }

        private static void convertSchema(SchemaInfo schemaInfo, Schema schema)
        {
            schema.setName(schemaInfo.getName()).setSchemaData(schemaInfo.getSchema()).setType(getSchemaType(schemaInfo.getType()));

            schemaInfo.getProperties().entrySet().ForEach(entry =>
            {
                if (entry.getKey() != null && entry.getValue() != null)
                {
                    schema.addProperty().setKey(entry.getKey()).setValue(entry.getValue());
                }
            });
        }

        public static AbstractByteBuffer newProducer(string topic, long producerId, long requestId, string producerName, bool encrypted, IDictionary<string, string> metadata, SchemaInfo schemaInfo, long epoch, bool userProvidedProducerName, ProducerAccessMode accessMode, long? topicEpoch, bool isTxnEnabled)
        {
            return newProducer(topic, producerId, requestId, producerName, encrypted, metadata, schemaInfo, epoch, userProvidedProducerName, accessMode, topicEpoch, isTxnEnabled, null);

        }

        public static AbstractByteBuffer newProducer(string topic, long producerId, long requestId, string producerName, bool encrypted, IDictionary<string, string> metadata, SchemaInfo schemaInfo, long epoch, bool userProvidedProducerName, ProducerAccessMode accessMode, long? topicEpoch, bool isTxnEnabled, string initialSubscriptionName)
        {
            BaseCommand cmd = localCmd(BaseCommand.Type.PRODUCER);
            CommandProducer producer = cmd.setProducer().setTopic(topic).setProducerId(producerId).setRequestId(requestId).setEpoch(epoch).setUserProvidedProducerName(userProvidedProducerName).setEncrypted(encrypted).setTxnEnabled(isTxnEnabled).setProducerAccessMode(convertProducerAccessMode(accessMode));
            if (!string.ReferenceEquals(producerName, null))
            {
                producer.setProducerName(producerName);
            }

            if (metadata.Count > 0)
            {
                metadata.forEach((k, v) => producer.addMetadata().setKey(k).setValue(v));
            }

            if (null != schemaInfo)
            {
                convertSchema(schemaInfo, producer.setSchema());
            }

            topicEpoch.ifPresent(producer.setTopicEpoch);

            if (!Strings.isNullOrEmpty(initialSubscriptionName))
            {
                producer.setInitialSubscriptionName(initialSubscriptionName);
            }

            return serializeWithSize(cmd);
        }

        public static AbstractByteBuffer newPartitionMetadataRequest(string topic, long requestId, bool metadataAutoCreationEnabled)
        {
            BaseCommand cmd = localCmd(BaseCommand.Type.PARTITIONED_METADATA);
            cmd.setPartitionMetadata().setTopic(topic).setRequestId(requestId).setMetadataAutoCreationEnabled(metadataAutoCreationEnabled);
            return serializeWithSize(cmd);
        }

        public static AbstractByteBuffer newLookup(string topic, bool authoritative, long requestId)
        {
            return newLookup(topic, null, authoritative, requestId, null);
        }

        public static AbstractByteBuffer newLookup(string topic, string listenerName, bool authoritative, long requestId, IDictionary<string, string> properties)
        {
            BaseCommand cmd = localCmd(BaseCommand.Type.LOOKUP);
            CommandLookupTopic lookup = cmd.setLookupTopic().setTopic(topic).setRequestId(requestId).setAuthoritative(authoritative);
            if (StringUtils.isNotBlank(listenerName))
            {
                lookup.setAdvertisedListenerName(listenerName);
            }
            if (properties != null)
            {
                properties.forEach((key, value) => lookup.addProperty().setKey(key).setValue(value));
            }
            return serializeWithSize(cmd);
        }

        public static BaseCommand newLookupResponseCommand(string brokerServiceUrl, string brokerServiceUrlTls, bool authoritative, CommandLookupTopicResponse.LookupType lookupType, long requestId, bool proxyThroughServiceUrl)
        {
            BaseCommand cmd = localCmd(BaseCommand.Type.LOOKUP_RESPONSE);
            CommandLookupTopicResponse response = cmd.setLookupTopicResponse().setResponse(lookupType).setRequestId(requestId).setAuthoritative(authoritative).setProxyThroughServiceUrl(proxyThroughServiceUrl);
            if (!string.ReferenceEquals(brokerServiceUrl, null))
            {
                response.setBrokerServiceUrl(brokerServiceUrl);
            }
            if (!string.ReferenceEquals(brokerServiceUrlTls, null))
            {
                response.setBrokerServiceUrlTls(brokerServiceUrlTls);
            }

            return cmd;
        }


        public static AbstractByteBuffer newMultiTransactionMessageAck(long consumerId, TxnID txnID, IList<Triple<long, long, ConcurrentBitSetRecyclable>> entries)
        {
            BaseCommand cmd = newMultiMessageAckCommon(entries);
            cmd.getAck().setConsumerId(consumerId).setAckType(CommandAck.AckType.Individual).setTxnidLeastBits(txnID.getLeastSigBits()).setTxnidMostBits(txnID.getMostSigBits());
            return serializeWithSize(cmd);
        }

        private static BaseCommand newMultiMessageAckCommon(IList<Triple<long, long, ConcurrentBitSetRecyclable>> entries)
        {
            BaseCommand cmd = localCmd(BaseCommand.Type.ACK);
            CommandAck ack = cmd.setAck();
            int entriesCount = entries.Count;
            for (int i = 0; i < entriesCount; i++)
            {
                long ledgerId = entries[i].getLeft();
                long entryId = entries[i].getMiddle();
                ConcurrentBitSetRecyclable bitSet = entries[i].getRight();
                MessageIdData msgId = ack.addMessageId().setLedgerId(ledgerId).setEntryId(entryId);
                if (bitSet != null)
                {
                    long[] ackSet = bitSet.toLongArray();
                    for (int j = 0; j < ackSet.Length; j++)
                    {
                        msgId.addAckSet(ackSet[j]);
                    }
                    bitSet.recycle();
                }
            }

            return cmd;
        }

        public static AbstractByteBuffer newMultiMessageAck(long consumerId, IList<Triple<long, long, ConcurrentBitSetRecyclable>> entries, long requestId)
        {
            BaseCommand cmd = newMultiMessageAckCommon(entries);
            cmd.getAck().setConsumerId(consumerId).setAckType(CommandAck.AckType.Individual);
            if (requestId >= 0)
            {
                cmd.getAck().setRequestId(requestId);
            }
            return serializeWithSize(cmd);
        }

        public static AbstractByteBuffer newAck(long consumerId, long ledgerId, long entryId, BitSetRecyclable ackSet, CommandAck.AckType ackType, CommandAck.ValidationError validationError, IDictionary<string, long> properties, long requestId)
        {
            return newAck(consumerId, ledgerId, entryId, ackSet, ackType, validationError, properties, -1L, -1L, requestId, -1);
        }

        public static AbstractByteBuffer newAck(long consumerId, long ledgerId, long entryId, BitSetRecyclable ackSet, CommandAck.AckType ackType, CommandAck.ValidationError validationError, IDictionary<string, long> properties, long txnIdLeastBits, long txnIdMostBits, long requestId, int batchSize)
        {
            BaseCommand cmd = localCmd(BaseCommand.Type.ACK);
            CommandAck ack = cmd.setAck().setConsumerId(consumerId).setAckType(ackType);
            MessageIdData messageIdData = ack.addMessageId().setLedgerId(ledgerId).setEntryId(entryId);
            if (ackSet != null)
            {
                long[] @as = ackSet.toLongArray();
                for (int i = 0; i < @as.Length; i++)
                {
                    messageIdData.addAckSet(@as[i]);
                }
            }

            if (batchSize >= 0)
            {
                messageIdData.setBatchSize(batchSize);
            }

            return newAck(validationError, properties, txnIdLeastBits, txnIdMostBits, requestId, ack, cmd);
        }

        public static AbstractByteBuffer newAck(long consumerId, IList<MessageIdData> messageIds, CommandAck.AckType ackType, CommandAck.ValidationError validationError, IDictionary<string, long> properties, long txnIdLeastBits, long txnIdMostBits, long requestId)
        {
            BaseCommand cmd = localCmd(BaseCommand.Type.ACK);
            CommandAck ack = cmd.setAck().setConsumerId(consumerId).setAckType(ackType);
            ack.addAllMessageIds(messageIds);

            return newAck(validationError, properties, txnIdLeastBits, txnIdMostBits, requestId, ack, cmd);
        }

        private static AbstractByteBuffer newAck(CommandAck.ValidationError validationError, IDictionary<string, long> properties, long txnIdLeastBits, long txnIdMostBits, long requestId, CommandAck ack, BaseCommand cmd)
        {
            if (validationError != null)
            {
                ack.setValidationError(validationError);
            }
            if (txnIdMostBits >= 0)
            {
                ack.setTxnidMostBits(txnIdMostBits);
            }
            if (txnIdLeastBits >= 0)
            {
                ack.setTxnidLeastBits(txnIdLeastBits);
            }

            if (requestId >= 0)
            {
                ack.setRequestId(requestId);
            }
            if (properties.Count > 0)
            {
                properties.forEach((k, v) =>
                {
                    ack.addProperty().setKey(k).setValue(v);
                });
            }
            return serializeWithSize(cmd);
        }

        public static AbstractByteBuffer newAck(long consumerId, long ledgerId, long entryId, BitSetRecyclable ackSet, CommandAck.AckType ackType, CommandAck.ValidationError validationError, IDictionary<string, long> properties, long txnIdLeastBits, long txnIdMostBits, long requestId)
        {
            return newAck(consumerId, ledgerId, entryId, ackSet, ackType, validationError, properties, txnIdLeastBits, txnIdMostBits, requestId, -1);
        }

        public static AbstractByteBuffer newFlow(long consumerId, int messagePermits)
        {
            BaseCommand cmd = localCmd(BaseCommand.Type.FLOW);
            cmd.setFlow().setConsumerId(consumerId).setMessagePermits(messagePermits);
            return serializeWithSize(cmd);
        }

        public static AbstractByteBuffer newRedeliverUnacknowledgedMessages(long consumerId, long consumerEpoch)
        {
            BaseCommand cmd = localCmd(BaseCommand.Type.REDELIVER_UNACKNOWLEDGED_MESSAGES);
            cmd.setRedeliverUnacknowledgedMessages().setConsumerId(consumerId).setConsumerEpoch(consumerEpoch);
            return serializeWithSize(cmd);
        }

        public static AbstractByteBuffer newRedeliverUnacknowledgedMessages(long consumerId, IList<MessageIdData> messageIds)
        {
            BaseCommand cmd = localCmd(BaseCommand.Type.REDELIVER_UNACKNOWLEDGED_MESSAGES);
            CommandRedeliverUnacknowledgedMessages req = cmd.setRedeliverUnacknowledgedMessages().setConsumerId(consumerId);
            messageIds.ForEach(msgId =>
            {
                MessageIdData m = req.addMessageId().setLedgerId(msgId.getLedgerId()).setEntryId(msgId.getEntryId());
                if (msgId.hasBatchIndex())
                {
                    m.setBatchIndex(msgId.getBatchIndex());
                }
            });
            return serializeWithSize(cmd);
        }

        public static AbstractByteBuffer newGetTopicsOfNamespaceRequest(string @namespace, long requestId, CommandGetTopicsOfNamespace.Mode mode, string topicsPattern, string topicsHash)
        {
            BaseCommand cmd = localCmd(BaseCommand.Type.GET_TOPICS_OF_NAMESPACE);
            CommandGetTopicsOfNamespace topics = cmd.setGetTopicsOfNamespace();
            topics.setNamespace(@namespace);
            topics.setRequestId(requestId);
            topics.setMode(mode);
            if (!string.ReferenceEquals(topicsPattern, null))
            {
                topics.setTopicsPattern(topicsPattern);
            }
            if (!string.ReferenceEquals(topicsHash, null))
            {
                topics.setTopicsHash(topicsHash);
            }
            return serializeWithSize(cmd);
        }

        private static readonly IByteBuffer cmdPing;

        static Commands()
        {
            BaseCommand cmd = (new BaseCommand()).setType(BaseCommand.Type.PING);
            cmd.setPing();
            AbstractByteBuffer serializedCmdPing = SerializeWithSize(cmd);
            cmdPing = Unpooled.CopiedBuffer(serializedCmdPing);
            serializedCmdPing.release();
            BaseCommand cmd = (new BaseCommand()).setType(BaseCommand.Type.PONG);
            cmd.setPong();
            AbstractByteBuffer serializedCmdPong = SerializeWithSize(cmd);
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

        public static int ComputeChecksum(  AbstractByteBuffer byteBuffer)
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
            cmd.WriteTo(buf);
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

            AbstractByteBuffer headers = PulsarByteBufAllocator.DEFAULT.buffer(headersSize, headersSize);
            headers.WriteInt(totalSize); // External frame

            // Write cmd
            headers.WriteInt(cmdSize);
            cmd.WriteTo(headers);

            // Create checksum placeholder
            if (includeChecksum)
            {
                headers.WriteShort(MagicCrc32c);
                checksumReaderIndex = headers.WriterIndex;
                headers.WriterIndex = (headers.WriterIndex + ChecksumSize); // skip 4 bytes of checksum
            }

            // Write metadata
            headers.WriteInt(msgMetadataSize);
            msgMetadata.WriteTo(headers);

            ByteBufPair command = ByteBufPair.get(headers, payload);

            // write checksum at created checksum-placeholder
            if (includeChecksum)
            {
                headers.MarkReaderIndex();
                headers.ReaderIndex = (checksumReaderIndex + ChecksumSize);
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

            BrokerEntryMetadata brokerEntryMetadata = BROKER_ENTRY_METADATA.get();
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
            AbstractByteBuffer brokerMeta = PulsarByteBufAllocator.DEFAULT.buffer(brokerMetaSize + 6, brokerMetaSize + 6);
            brokerMeta.WriteShort(Commands.MagicBrokerEntryMetadata);
            brokerMeta.WriteInt(brokerMetaSize);
            brokerEntryMetadata.WriteTo(brokerMeta);

            CompositeByteBuf compositeByteBuf = PulsarByteBufAllocator.DEFAULT.compositeBuffer();
            compositeByteBuf.addComponents(true, brokerMeta, headerAndPayload);
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
                    brokerEntryMetadata.ParseFrom(headerAndPayload, brokerEntryMetadataSize);
                    return brokerEntryMetadata;
                }
                finally
                {
                    if (peek)
                    {
                        headerAndPayload.ReaderIndex = readerIndex;
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
            BrokerEntryMetadata brokerEntryMetadata = ParseOrPeekBrokerEntryMetadataIfExist(headerAndPayload, BROKER_ENTRY_METADATA.get(), true);
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
            BrokerEntryMetadata brokerEntryMetadata = ParseOrPeekBrokerEntryMetadataIfExist(headerAndPayload, BROKER_ENTRY_METADATA.get(), true);
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
            BrokerEntryMetadata brokerEntryMetadata = ParseOrPeekBrokerEntryMetadataIfExist(headerAndPayload, BROKER_ENTRY_METADATA.get(), true);
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

            AbstractByteBuffer metadataAndPayload = PulsarByteBufAllocator.DEFAULT.buffer(totalSize, totalSize);

            // Create checksum placeholder
            if (includeChecksum)
            {
                metadataAndPayload.WriteShort(MagicCrc32c);
                checksumReaderIndex = metadataAndPayload.WriterIndex;
                metadataAndPayload.WriterIndex = metadataAndPayload.WriterIndex + ChecksumSize; // skip 4 bytes of checksum
            }

            // Write metadata
            metadataAndPayload.WriteInt(msgMetadataSize);
            msgMetadata.WriteTo(metadataAndPayload);

            // write checksum at created checksum-placeholder
            if (includeChecksum)
            {
                metadataAndPayload.MarkReaderIndex();
                metadataAndPayload.ReaderIndex = checksumReaderIndex + ChecksumSize;
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
                    messageMetadata.AddReplicateTo(builder.getReplicateToAt(i));
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
            SingleMessageMetadata smm = LOCAL_SINGLE_MESSAGE_METADATA.get();
            smm.clear();

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
                smm.AddProperty().setKey(msg.getPropertyAt(i).getKey()).setValue(msg.getPropertyAt(i).getValue());
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
            int singleMetaSize = (int)uncompressedPayload.ReadUnsignedInt;
            singleMessageMetadata.ParseFrom(uncompressedPayload, singleMetaSize);

            int singleMessagePayloadSize = singleMessageMetadata.PayloadSize;

            int readerIndex = uncompressedPayload.ReaderIndex;
            AbstractByteBuffer singleMessagePayload = uncompressedPayload.RetainedSlice(readerIndex, singleMessagePayloadSize);

            // reader now points to beginning of payload read; so move it past message payload just read
            if (index < batchSize)
            {
                uncompressedPayload.ReaderIndex = (readerIndex + singleMessagePayloadSize);
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

            AbstractByteBuffer headers = PulsarByteBufAllocator.DEFAULT.buffer(headersSize);
            headers.WriteInt(totalSize); // External frame

            // Write cmd
            headers.WriteInt(cmdSize);
            cmd.WriteTo(headers);
            return ByteBufPair.get(headers, metadataAndPayload);
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
                log.error("[{}] [{}] Failed to parse message metadata", subscription, consumerId, t);
                return null;
            }
            finally
            {
                metadataAndPayload.ReaderIndex = readerIdx;
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
                metadataAndPayload.ReaderIndex = readerIdx;
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
                log.error("[{}] [{}] Failed to parse message metadata", subscription, consumerId, t);
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
                log.error("[{}] [{}] Failed to peek sticky key from the message metadata", topic, subscription, t);
                return NONE_KEY;
            }
            finally
            {
                metadataAndPayload.ReaderIndex = readerIdx;
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
                    stickyKey = Base64.getDecoder().decode(metadata.getPartitionKey());
                }
                else
                {
                    stickyKey = metadata.getPartitionKey().getBytes(StandardCharsets.UTF_8);
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

        private static ProducerAccessMode ConvertProducerAccessMode(ProducerAccessMode accessMode)
        {
            switch (accessMode)
            {
                case ProducerAccessMode.Exclusive:
                    return ProducerAccessMode.Exclusive;
                case ProducerAccessMode.Shared:
                    return ProducerAccessMode.Shared;
                case ProducerAccessMode.WaitForExclusive:
                    return ProducerAccessMode.WaitForExclusive;
                case ProducerAccessMode.ExclusiveWithFencing:
                    return ProducerAccessMode.ExclusiveWithFencing;
                default:
                    throw new ArgumentException("Unknown access mode: " + accessMode);
            }
        }

        public static ProducerAccessMode ConvertProducerAccessMode(ProducerAccessMode accessMode)
        {
            switch (accessMode)
            {
                case ProducerAccessMode.Exclusive:
                    return ProducerAccessMode.Exclusive;
                case ProducerAccessMode.Shared:
                    return ProducerAccessMode.Shared;
                case ProducerAccessMode.WaitForExclusive:
                    return ProducerAccessMode.WaitForExclusive;
                case ProducerAccessMode.ExclusiveWithFencing:
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