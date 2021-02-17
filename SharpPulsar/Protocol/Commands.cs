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
using SharpPulsar.Protocol.Extension;
using KeySharedMode = SharpPulsar.Protocol.Proto.KeySharedMode;
using SharpPulsar.Interfaces.ISchema;
using SharpPulsar.Common;
using SharpPulsar.Transaction;
using SharpPulsar.Helpers;
using SharpPulsar.Extension;

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
	public class Commands
	{

		// default message Size for transfer
		public const int DefaultMaxMessageSize = 5 * 1024 * 1024;
		public const int MessageSizeFramePadding = 10 * 1024;
		public const int InvalidMaxMessageSize = -1;

		public static bool PeerSupportJsonSchemaAvroFormat(int peerVersion)
		{
			return peerVersion >= (int)ProtocolVersion.V13;
		}
		public static byte[] NewConnect(string authMethodName, string authData, string libVersion)
		{
			return NewConnect(authMethodName, authData, CurrentProtocolVersion, libVersion, null, null, null, null);
		}

		public static byte[] NewConnect(string authMethodName, string authData, string libVersion, string targetBroker)
		{
			return NewConnect(authMethodName, authData, CurrentProtocolVersion, libVersion, targetBroker, null, null, null);
		}

		public static byte[] NewConnect(string authMethodName, string authData, string libVersion, string targetBroker, string originalPrincipal, string clientAuthData, string clientAuthMethod)
		{
			return NewConnect(authMethodName, authData, CurrentProtocolVersion, libVersion, targetBroker, originalPrincipal, clientAuthData, clientAuthMethod);
		}

		public static byte[] NewConnect(string authMethodName, string authData, int protocolVersion, string libVersion, string targetBroker, string originalPrincipal, string originalAuthData, string originalAuthMethod)
		{
            var connect = new CommandConnect
            {
                ClientVersion = libVersion ?? "Pulsar Client", 
                AuthMethodName = authMethodName,
				FeatureFlags = new FeatureFlags { SupportsAuthRefresh = true}

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
			var res = Serializer.Serialize(connect.ToBaseCommand());
			
			return res.ToArray();
		}

		public static byte[] NewConnect(string authMethodName, AuthData authData, int protocolVersion, string libVersion, string targetBroker, string originalPrincipal, AuthData originalAuthData, string originalAuthMethod)
		{
            var connect = new CommandConnect
            {
                ClientVersion = libVersion, 
                AuthMethodName = authMethodName,
				FeatureFlags = new FeatureFlags { SupportsAuthRefresh = true}
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
			connect.ProtocolVersion = protocolVersion;
            var ba = connect.ToBaseCommand();
            var res = Serializer.Serialize(ba);
            return res.ToArray();
        }
        public static byte[] NewAuthResponse(string authMethod, AuthData clientData, int clientProtocolVersion, string clientVersion)
        {
            var authData = new AuthData {auth_data = clientData.auth_data, AuthMethodName = authMethod};

            var response = new CommandAuthResponse
            {
                Response = authData,
                ProtocolVersion = clientProtocolVersion,
                ClientVersion = clientVersion ?? "Pulsar Client"
            };
            var res = Serializer.Serialize(response.ToBaseCommand());
            return res.ToArray();
        }
		public static byte[] NewAuthChallenge(string authMethod, AuthData brokerData, int clientProtocolVersion)
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

			var res = Serializer.Serialize(challenge.ToBaseCommand());
			return res.ToArray();
		}
		
		public static byte[] NewSendError(long producerId, long sequenceId, ServerError error, string errorMsg)
		{
            var sendError = new CommandSendError
            {
                ProducerId = (ulong) producerId,
                SequenceId = (ulong) sequenceId,
                Error = error,
                Message = errorMsg
            };
            var res = Serializer.Serialize(sendError.ToBaseCommand());
			
			return res.ToArray();
		}


		public static bool HasChecksum(byte[] buffer)
        {
            return true; //buffer.GetShort(buffer.ReaderIndex) == MagicCrc32C;
        }

		/// <summary>
		/// Read the checksum and advance the reader index in the buffer.
		/// 
		/// <para>Note: This method assume the checksum presence was already verified before.
		/// </para>
		/// </summary>
		public static int ReadChecksum(byte[] buffer)
		{
			//buffer.SkipBytes(2); //skip magic bytes
            return -1;// buffer.ReadInt();
		}

		public static void SkipChecksumIfPresent(byte[] buffer)
		{
			if (HasChecksum(buffer))
			{
				ReadChecksum(buffer);
			}
		}
		
		public static MessageMetadata ParseMessageMetadata(ReadOnlySequence<byte> buffer)
		{
			try
			{
				//SkipChecksumIfPresent(buffer);
				var bufferbytes = buffer.ToArray();
                return bufferbytes.FromByteArray<MessageMetadata>();
			}
			catch (IOException e)
			{
				throw new Exception(e.Message, e);
			}
		}


		public static byte[] NewSend(long producerId, long sequenceId, int numMessaegs, MessageMetadata messageMetadata, byte[] payload)
		{
			return NewSend(producerId, sequenceId, numMessaegs, 0, 0, messageMetadata, payload);
		}

		public static byte[] NewSend(long producerId, long lowestSequenceId, long highestSequenceId, int numMessaegs, MessageMetadata messageMetadata, byte[] payload)
		{
			return NewSend(producerId, lowestSequenceId, highestSequenceId, numMessaegs, 0, 0, messageMetadata, payload);
		}

		public static byte[] NewSend(long producerId, long sequenceId, int numMessages, long txnIdLeastBits, long txnIdMostBits, MessageMetadata messageData, byte[] payload)
		{
            var send = new CommandSend
            {
                ProducerId = (ulong) producerId, 
                SequenceId = (ulong) sequenceId
            };
            if (numMessages > 1)
			{
				send.NumMessages = numMessages;
			}
			if (txnIdLeastBits > 0)
			{
				send.TxnidLeastBits = (ulong)txnIdLeastBits;
			}
			if (txnIdMostBits > 0)
			{
				send.TxnidMostBits = (ulong)txnIdMostBits;
			}
            var res = Serializer.Serialize(send.ToBaseCommand(), messageData, new ReadOnlySequence<byte>(payload));
			return res;
		}

		public static byte[] NewSend(long producerId, long lowestSequenceId, long highestSequenceId, int numMessages, long txnIdLeastBits, long txnIdMostBits, MessageMetadata messageData, byte[] payload)
		{
            var send = new CommandSend
            {
                ProducerId = (ulong) producerId,
                SequenceId = (ulong) lowestSequenceId,
                HighestSequenceId = (ulong) highestSequenceId
            };
            if (numMessages > 1)
			{
				send.NumMessages = numMessages;
			}
			if (txnIdLeastBits > 0)
			{
				send.TxnidLeastBits = (ulong)txnIdLeastBits;
			}
			if (txnIdMostBits > 0)
			{
				send.TxnidMostBits = (ulong)txnIdMostBits;
			}
            if (messageData.TotalChunkMsgSize > 1)
            {
                send.IsChunk = true;
            }
			var res = Serializer.Serialize(send.ToBaseCommand(), messageData, new ReadOnlySequence<byte>(payload));
			return res;
		}

		public static byte[] NewSubscribe(string topic, string subscription, long consumerId, long requestId, CommandSubscribe.SubType subType, int priorityLevel, string consumerName, long resetStartMessageBackInSeconds)
		{
			return NewSubscribe(topic, subscription, consumerId, requestId, subType, priorityLevel, consumerName, true, null, new Dictionary<string,string>(), false, false, CommandSubscribe.InitialPosition.Earliest, resetStartMessageBackInSeconds, null, true);
		}
		
		public static byte[] NewSubscribe(string topic, string subscription, long consumerId, long requestId, CommandSubscribe.SubType subType, int priorityLevel, string consumerName, bool isDurable, MessageIdData startMessageId, IDictionary<string, string> metadata, bool readCompacted, bool isReplicated, CommandSubscribe.InitialPosition subscriptionInitialPosition, long startMessageRollbackDurationInSec, ISchemaInfo schemaInfo, bool createTopicIfDoesNotExist)
		{
            return NewSubscribe(topic, subscription, consumerId, requestId, subType, priorityLevel, consumerName, isDurable, startMessageId, metadata, readCompacted, isReplicated, subscriptionInitialPosition, startMessageRollbackDurationInSec, schemaInfo, createTopicIfDoesNotExist, null);
		}

		public static byte[] NewSubscribe(string topic, string subscription, long consumerId, long requestId, CommandSubscribe.SubType subType, int priorityLevel, string consumerName, bool isDurable, MessageIdData startMessageId, IDictionary<string, string> metadata, bool readCompacted, bool isReplicated, CommandSubscribe.InitialPosition subscriptionInitialPosition, long startMessageRollbackDurationInSec, ISchemaInfo schemaInfo, bool createTopicIfDoesNotExist, KeySharedPolicy keySharedPolicy)
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
                ForceTopicCreation = createTopicIfDoesNotExist
            };

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
                var schema = GetSchema(schemaInfo);
                subscribe.Schema = schema;
            }

			var res = Serializer.Serialize(subscribe.ToBaseCommand());

            return res.ToArray();
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
		public static byte[] NewUnsubscribe(long consumerId, long requestId)
		{
            var unsubscribe = new CommandUnsubscribe
            {
                ConsumerId = (ulong) consumerId, RequestId = (ulong) requestId
            };
            var res = Serializer.Serialize(unsubscribe.ToBaseCommand());
			return res.ToArray();
		}

		public static byte[] NewActiveConsumerChange(long consumerId, bool isActive)
		{
            var change = new CommandActiveConsumerChange {ConsumerId = (ulong) consumerId, IsActive = isActive};
            var res = Serializer.Serialize(change.ToBaseCommand());
			return res.ToArray();
		}

		public static byte[] NewSeek(long consumerId, long requestId, long ledgerId, long entryId, long[] ackSet)
		{
            var seek = new CommandSeek {ConsumerId = (ulong) consumerId, RequestId = (ulong) requestId};

            var messageId = new MessageIdData {ledgerId = (ulong) ledgerId, entryId = (ulong) entryId, AckSets = ackSet};
            seek.MessageId = messageId;
			var res = Serializer.Serialize(seek.ToBaseCommand());
			
			return res.ToArray();
		}

		public static byte[] NewSeek(long consumerId, long requestId, long timestamp)
		{
            var seek = new CommandSeek
            {
                ConsumerId = (ulong) consumerId,
                RequestId = (ulong) requestId,
                MessagePublishTime = (ulong) timestamp
            };

            var res = Serializer.Serialize(seek.ToBaseCommand());

			return res.ToArray();
		}

		public static byte[] NewCloseConsumer(long consumerId, long requestId)
		{
            var closeConsumer = new CommandCloseConsumer
            {
                ConsumerId = (ulong) consumerId, RequestId = (ulong) requestId
            };
            var res = Serializer.Serialize(closeConsumer.ToBaseCommand());
			
			return res.ToArray();
		}

		public static byte[] NewReachedEndOfTopic(long consumerId)
		{
            var reachedEndOfTopic = new CommandReachedEndOfTopic {ConsumerId = (ulong) consumerId};
            var res = Serializer.Serialize(reachedEndOfTopic.ToBaseCommand());
			
			return res.ToArray();
		}

		public static byte[] NewCloseProducer(long producerId, long requestId)
		{
            var closeProducer = new CommandCloseProducer
            {
                ProducerId = (ulong) producerId, RequestId = (ulong) requestId
            };
            var res = Serializer.Serialize(closeProducer.ToBaseCommand());
			
			return res.ToArray();
		}

		public static byte[] NewProducer(string topic, long producerId, long requestId, string producerName, IDictionary<string, string> metadata)
		{
			return NewProducer(topic, producerId, requestId, producerName, false, metadata);
		}

		public static byte[] NewProducer(string topic, long producerId, long requestId, string producerName, bool encrypted, IDictionary<string, string> metadata)
		{
			return NewProducer(topic, producerId, requestId, producerName, encrypted, metadata, null, 0, false);
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
		private static Proto.Schema GetSchema(ISchemaInfo schemaInfo)
		{
            var schema = new Proto.Schema
            {
                Name = schemaInfo.Name,
                SchemaData = (byte[]) (object) schemaInfo.Schema,
                type = GetSchemaType(schemaInfo.Type)
            };
            schema.Properties.AddRange(schemaInfo.Properties.ToList().Select(entry => new KeyValue{Key = entry.Key, Value = entry.Value}).ToList());
			
			return schema;
		}

		public static byte[] NewProducer(string topic, long producerId, long requestId, string producerName, bool encrypted, IDictionary<string, string> metadata, ISchemaInfo schemaInfo, long epoch, bool userProvidedProducerName)
		{
            var producer = new CommandProducer
            {
                Topic = topic, ProducerId = (ulong) producerId, RequestId = (ulong) requestId, Epoch = (ulong) epoch
            };
			
            if (!ReferenceEquals(producerName, null))
			{
				producer.ProducerName = producerName;
			}
			producer.UserProvidedProducerName = userProvidedProducerName;
			producer.Encrypted = encrypted;

			producer.Metadatas.AddRange(CommandUtils.ToKeyValueList(metadata));

			if (null != schemaInfo)
			{
				producer.Schema = GetSchema(schemaInfo);
			}

			var res = Serializer.Serialize(producer.ToBaseCommand());
			
			return res.ToArray();
		}

		public static byte[] NewPartitionMetadataRequest(string topic, long requestId)
		{
            var partitionMetadata = new CommandPartitionedTopicMetadata
            {
                Topic = topic, RequestId = (ulong) requestId
            };
            var res = Serializer.Serialize(partitionMetadata.ToBaseCommand());
			
			return res.ToArray();
		}

		public static byte[] NewLookup(string topic, string listenerName, bool authoritative, long requestId)
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
			var res = Serializer.Serialize(lookupTopic.ToBaseCommand());
			
			return res.ToArray();
		}
		public static byte[] NewMultiTransactionMessageAck(long consumerId, TxnID txnID, IList<(long ledger, long entry, BitSet bitSet)> entries)
		{
            var ackBuilder = new CommandAck
            {
                ConsumerId = (ulong)consumerId,
                ack_type = CommandAck.AckType.Individual,
                TxnidLeastBits = (ulong)txnID.LeastSigBits,
                TxnidMostBits = (ulong)txnID.MostSigBits
            };
            return NewMultiMessageAckCommon(ackBuilder, entries);
		}
		public static byte[] NewMultiMessageAckCommon(CommandAck ackBuilder, IList<(long ledger, long entry, BitSet bitSet)> entries)
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
					messageIdDataBuilder.AckSets = bitSet.ToLongArray();
				}
				var messageIdData = messageIdDataBuilder;
				ackBuilder.MessageIds.Add(messageIdData);
			}

			var ack = ackBuilder;

			var res = Serializer.Serialize(ack.ToBaseCommand());
			return res.ToArray();
		}
		public static byte[] NewMultiMessageAck(long consumerId, IList<(long LedgerId, long EntryId, BitSet Sets)> entries)
        {
            var ackCmd = new CommandAck {ConsumerId = (ulong) consumerId, ack_type = CommandAck.AckType.Individual};

            var entriesCount = entries.Count;
            for (var i = 0; i < entriesCount; i++)
            {
                var ledgerId = entries[i].LedgerId;
                var entryId = entries[i].EntryId;
                var bitSet = entries[i].Sets;
                var messageIdData = new MessageIdData {ledgerId = (ulong) ledgerId, entryId = (ulong) entryId};
                if (bitSet != null)
                {
                    messageIdData.AckSets = bitSet.ToLongArray();
                }
                ackCmd.MessageIds.Add(messageIdData);
            }
            var res = Serializer.Serialize(ackCmd.ToBaseCommand());
            return res.ToArray();
        }
        public static byte[] NewMultiMessageAck(long consumerId, IList<(long LedgerId, long EntryId, long[] Sets)> entries)
        {
            var ackCmd = new CommandAck { ConsumerId = (ulong)consumerId, ack_type = CommandAck.AckType.Individual };

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
            var res = Serializer.Serialize(ackCmd.ToBaseCommand());
            return res.ToArray();
        }

		public static byte[] NewAck(long consumerId, long ledgerId, long entryId, long[] ackSets, CommandAck.AckType ackType, CommandAck.ValidationError? validationError, IDictionary<string, long> properties)
		{
			return NewAck(consumerId, ledgerId, entryId, ackSets, ackType, validationError, properties, -1L, -1L, -1L, -1);
		}
		public static byte[] NewAck(long consumerId, long ledgerId, long entryId, long[] ackSet, CommandAck.AckType ackType, CommandAck.ValidationError? validationError, IDictionary<string, long> properties, long txnIdLeastBits, long txnIdMostBits, long requestId)
		{
			return NewAck(consumerId, ledgerId, entryId, ackSet, ackType, validationError,
					properties, txnIdLeastBits, txnIdMostBits, requestId, -1);
		}
		public static byte[] NewAck(long consumerId, long ledgerId, long entryId, long[] ackSets, CommandAck.AckType ackType, CommandAck.ValidationError? validationError, IDictionary<string, long> properties, long txnIdLeastBits, long txnIdMostBits, long requestId, int batchSize)
		{
            var ack = new CommandAck {ConsumerId = (ulong) consumerId, ack_type = ackType};
			
            var messageIdData = new MessageIdData {ledgerId = (ulong) ledgerId, entryId = (ulong) entryId};
            if (ackSets != null)
            {
                messageIdData.AckSets = ackSets;
            }
            ack.MessageIds.Add(messageIdData);
			if (validationError != null)
			{
				ack.validation_error = (CommandAck.ValidationError) validationError;
			}

			if (batchSize >= 0)
			{
				messageIdData.BatchSize = batchSize;
			}

			if (requestId >= 0)
			{
				ack.RequestId = (ulong)requestId;
			}
			if (txnIdMostBits > 0)
			{
				ack.TxnidMostBits = (ulong)txnIdMostBits;
			}
			if (txnIdLeastBits > 0)
			{
				ack.TxnidLeastBits = (ulong)txnIdLeastBits;
			}
			foreach (var e in properties.ToList())
			{
				ack.Properties.Add(new KeyLongValue(){Key = e.Key, Value = (ulong)e.Value});
			}

			var res = Serializer.Serialize(ack.ToBaseCommand());
			
			return res.ToArray();
		}

		public static byte[] NewFlow(long consumerId, int messagePermits)
		{
            var flow = new CommandFlow {ConsumerId = (ulong) consumerId, messagePermits = (uint) messagePermits};

            var res = Serializer.Serialize(flow.ToBaseCommand());
			
			return res.ToArray();
		}

		public static byte[] NewRedeliverUnacknowledgedMessages(long consumerId)
		{
            var redeliver = new CommandRedeliverUnacknowledgedMessages {ConsumerId = (ulong) consumerId};
            var res = Serializer.Serialize(redeliver.ToBaseCommand());
			
			return res.ToArray();
		}

		public static byte[] NewRedeliverUnacknowledgedMessages(long consumerId, IList<MessageIdData> messageIds)
		{
            var redeliver = new CommandRedeliverUnacknowledgedMessages {ConsumerId = (ulong) consumerId};
            redeliver.MessageIds.AddRange(messageIds);
			var res = Serializer.Serialize(redeliver.ToBaseCommand());
		    return res.ToArray();
		}

		public static byte[] NewGetTopicsOfNamespaceRequest(string @namespace, long requestId, CommandGetTopicsOfNamespace.Mode mode)
		{
            var topics = new CommandGetTopicsOfNamespace
            {
                Namespace = @namespace, RequestId = (ulong) requestId, mode = mode
            };
			var res = Serializer.Serialize(topics.ToBaseCommand());
			
			return res.ToArray();
		}
        private static readonly byte[] CmdPing;

		static Commands()
		{
			var serializedCmdPing = Serializer.Serialize(new CommandPing().ToBaseCommand());
			CmdPing = serializedCmdPing.ToArray();
			var serializedCmdPong = Serializer.Serialize(new CommandPong().ToBaseCommand());
			CmdPong = serializedCmdPong.ToArray();
		}

		internal static byte[] NewPing()
		{
			return CmdPing;
		}

		private static readonly byte[] CmdPong;


		internal static byte[] NewPong()
		{
			return CmdPong;
		}

		public static byte[] NewGetLastMessageId(long consumerId, long requestId)
		{
            var cmd = new CommandGetLastMessageId {ConsumerId = (ulong) consumerId, RequestId = (ulong) requestId};

            var res = Serializer.Serialize(cmd.ToBaseCommand());
			
			return res.ToArray();
		}

		public static byte[] NewGetSchema(long requestId, string topic, SchemaVersion version)
        {
            var schema = new CommandGetSchema {RequestId = (ulong) requestId, Topic = topic};
            if (version != null)
			{
				schema.SchemaVersion = version.Bytes().ToBytes();
			}
			
			var res = Serializer.Serialize(schema.ToBaseCommand());
			
			return res.ToArray();
		}

		public static byte[] NewGetOrCreateSchema(long requestId, string topic, ISchemaInfo schemaInfo)
		{
            var getOrCreateSchema = new CommandGetOrCreateSchema
            {
                RequestId = (ulong) requestId, Topic = topic, Schema = GetSchema(schemaInfo)
            };
            var res = Serializer.Serialize(getOrCreateSchema.ToBaseCommand());
			
			return res.ToArray();
		}
		
		// ---- transaction related ----

		public static byte[] NewTxn(long tcId, long requestId, long ttlSeconds)
		{
            var commandNewTxn = new CommandNewTxn
            {
                TcId = (ulong) tcId, RequestId = (ulong) requestId, TxnTtlSeconds = (ulong) ttlSeconds
            };
            var res = Serializer.Serialize(commandNewTxn.ToBaseCommand());
			
			return res.ToArray();
		}

		public static byte[] NewAddPartitionToTxn(long requestId, long txnIdLeastBits, long txnIdMostBits, IList<string> partitions)
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
            var res = Serializer.Serialize(commandAddPartitionToTxn.ToBaseCommand());
			
			return res.ToArray();
		}

		public static byte[] NewAddSubscriptionToTxn(long requestId, long txnIdLeastBits, long txnIdMostBits, IList<Subscription> subscription)
		{
            var commandAddSubscriptionToTxn = new CommandAddSubscriptionToTxn
            {
                RequestId = (ulong) requestId,
                TxnidLeastBits = (ulong) txnIdLeastBits,
                TxnidMostBits = (ulong) txnIdMostBits
            };
            commandAddSubscriptionToTxn.Subscriptions.AddRange(subscription);
			var res = Serializer.Serialize(commandAddSubscriptionToTxn.ToBaseCommand());
			
			return res.ToArray();
		}

		public static byte[] NewEndTxn(long requestId, long txnIdLeastBits, long txnIdMostBits, TxnAction txnAction, IList<MessageIdData> messageIdDatas)
		{
            var commandEndTxn = new CommandEndTxn
            {
                RequestId = (ulong) requestId,
                TxnidLeastBits = (ulong) txnIdLeastBits,
                TxnidMostBits = (ulong) txnIdMostBits,
                TxnAction = txnAction
            };
			commandEndTxn.MessageIds.AddRange(messageIdDatas);
            var res = Serializer.Serialize(commandEndTxn.ToBaseCommand());
			
			return res.ToArray();
		}

		public static byte[] NewEndTxnOnPartition(long requestId, long txnIdLeastBits, long txnIdMostBits, string topic, TxnAction txnAction)
		{
            var txnEndOnPartition = new CommandEndTxnOnPartition
            {
                RequestId = (ulong) requestId,
                TxnidLeastBits = (ulong) txnIdLeastBits,
                TxnidMostBits = (ulong) txnIdMostBits,
                Topic = topic,
                TxnAction = txnAction
            };
            var res = Serializer.Serialize(txnEndOnPartition.ToBaseCommand());
			
			return res.ToArray();
		}

		
		public static byte[] NewEndTxnOnSubscription(long requestId, long txnIdLeastBits, long txnIdMostBits, Subscription subscription, TxnAction txnAction)
		{
            var commandEndTxnOnSubscription = new CommandEndTxnOnSubscription
            {
                RequestId = (ulong) requestId,
                TxnidLeastBits = (ulong) txnIdLeastBits,
                TxnidMostBits = (ulong) txnIdMostBits,
                Subscription = subscription,
                TxnAction = txnAction
            };
            var res = Serializer.Serialize(commandEndTxnOnSubscription.ToBaseCommand());
			
			return res.ToArray();
		}
        
		public static int ComputeChecksum(byte[] byteBuffer)
        {
            return 9;//Crc32CIntChecksum.ComputeChecksum(byteBuffer);
        }
        public static int ResumeChecksum(int prev, byte[] byteBuffer)
        {
            return 9; //Crc32CIntChecksum.ResumeChecksum(prev, byteBuffer);
        }
		
		public static long InitBatchMessageMetadata(MessageMetadata builder)
		{
            var messageMetadata = new MessageMetadata
            {
                PublishTime = builder.PublishTime,
                ProducerName = builder.ProducerName,
                SequenceId = (ulong) builder.SequenceId
            };
            if (!string.IsNullOrWhiteSpace(builder.ReplicatedFrom))
			{
				messageMetadata.ReplicatedFrom = builder.ReplicatedFrom;
			}
			if (builder.ReplicateToes.Count > 0)
			{
				messageMetadata.ReplicateToes.AddRange(builder.ReplicateToes);
			}
			if (builder.SchemaVersion?.Length > 0)
			{
				messageMetadata.SchemaVersion = builder.SchemaVersion;
			}
			return (long)builder.SequenceId;
		}

		public static SingleMessageMetadata SingleMessageMetadat(MessageMetadata msg, int payloadSize)
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
            singleMessageMetadata.SequenceId = msg.SequenceId;
			return singleMessageMetadata;
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
	}

}