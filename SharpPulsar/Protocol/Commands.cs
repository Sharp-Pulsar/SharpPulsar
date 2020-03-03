using DotNetty.Buffers;
using SharpPulsar.Common.Schema;
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
using System.Text.Json;
using ProtoBuf;
using SharpPulsar.Extension;
using SharpPulsar.Protocol.Circe;
using SharpPulsar.Protocol.Extension;
using SharpPulsar.Stole;
using SharpPulsar.Utility;
using SharpPulsar.Utility.Protobuf;
using Serializer = SharpPulsar.Stole.Serializer;

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

		// default message size for transfer
		public const int DefaultMaxMessageSize = 5 * 1024 * 1024;
		public const int MessageSizeFramePadding = 10 * 1024;
		public const int InvalidMaxMessageSize = -1;
		public static Crc32CIntChecksum _intChecksum = new Crc32CIntChecksum();

		public const short MagicCrc32C = 0x0e01;
		private const int ChecksumSize = 4;
		
		public static IByteBuffer NewConnect(string authMethodName, string authData, string libVersion)
		{
			return NewConnect(authMethodName, authData, CurrentProtocolVersion, libVersion, null, null, null, null);
		}

		public static IByteBuffer NewConnect(string authMethodName, string authData, string libVersion, string targetBroker)
		{
			return NewConnect(authMethodName, authData, CurrentProtocolVersion, libVersion, targetBroker, null, null, null);
		}

		public static IByteBuffer NewConnect(string authMethodName, string authData, string libVersion, string targetBroker, string originalPrincipal, string clientAuthData, string clientAuthMethod)
		{
			return NewConnect(authMethodName, authData, CurrentProtocolVersion, libVersion, targetBroker, originalPrincipal, clientAuthData, clientAuthMethod);
		}

		public static IByteBuffer NewConnect(string authMethodName, string authData, int protocolVersion, string libVersion, string targetBroker, string originalPrincipal, string originalAuthData, string originalAuthMethod)
		{
			var connectBuilder = CommandConnect.NewBuilder();
			connectBuilder.SetClientVersion(libVersion ?? "Pulsar Client");
			connectBuilder.SetAuthMethodName(authMethodName);

			if ("ycav1".Equals(authMethodName))
			{
				// Handle the case of a client that gets updated before the broker and starts sending the string auth method
				// name. An example would be in broker-to-broker replication. We need to make sure the clients are still
				// passing both the enum and the string until all brokers are upgraded.
				connectBuilder.SetAuthMethod(AuthMethod.AuthMethodYcaV1);
			}

			if (!ReferenceEquals(targetBroker, null))
			{
				// When connecting through a proxy, we need to specify which broker do we want to be proxied through
				connectBuilder.SetProxyToBrokerUrl(targetBroker);
			}

			if (!ReferenceEquals(authData, null))
			{
				connectBuilder.SetAuthData(ByteString.CopyFromUtf8(authData).ToByteArray());
			}

			if (!ReferenceEquals(originalPrincipal, null))
			{
				connectBuilder.SetOriginalPrincipal(originalPrincipal);
			}

			if (!ReferenceEquals(originalAuthData, null))
			{
				connectBuilder.SetOriginalAuthData(originalAuthData);
			}

			if (!ReferenceEquals(originalAuthMethod, null))
			{
				connectBuilder.SetOriginalAuthMethod(originalAuthMethod);
			}
			connectBuilder.SetProtocolVersion(protocolVersion);
			var connect = connectBuilder.Build();
			var res = SerializeWithSize(BaseCommand.NewBuilder().SetType(BaseCommand.Type.Connect).SetConnect(connect));
			
			return res;
		}

		public static byte[] NewConnect(string authMethodName, AuthData authData, int protocolVersion, string libVersion, string targetBroker, string originalPrincipal, AuthData originalAuthData, string originalAuthMethod)
		{
			var connectBuilder = CommandConnect.NewBuilder();
			connectBuilder.SetClientVersion(libVersion ?? "Pulsar Client");
			connectBuilder.SetAuthMethodName(authMethodName);

			if (!ReferenceEquals(targetBroker, null))
			{
				// When connecting through a proxy, we need to specify which broker do we want to be proxied through
				connectBuilder.SetProxyToBrokerUrl(targetBroker);
			}

			if (authData != null)
			{
				connectBuilder.SetAuthData(authData.auth_data);
			}

			if (!ReferenceEquals(originalPrincipal, null))
			{
				connectBuilder.SetOriginalPrincipal(originalPrincipal);
			}

			if (originalAuthData != null)
			{
				connectBuilder.SetOriginalAuthData(Encoding.UTF8.GetString(originalAuthData.auth_data));
			}

			if (!ReferenceEquals(originalAuthMethod, null))
			{
				connectBuilder.SetOriginalAuthMethod(originalAuthMethod);
			}
			connectBuilder.SetProtocolVersion(protocolVersion);
			var connect = connectBuilder.Build();
            var ba = BaseCommand.NewBuilder().SetType(BaseCommand.Type.Connect).SetConnect(connect);
           
            var res = Serializer.Serialize(BaseCommand.NewBuilder().SetType(BaseCommand.Type.Connect).SetConnect(connect).Build());
            return res.ToArray();
        }

		public static IByteBuffer NewAuthChallenge(string authMethod, AuthData brokerData, int clientProtocolVersion)
		{
			var challengeBuilder = CommandAuthChallenge.NewBuilder();

			// If the broker supports a newer version of the protocol, it will anyway advertise the max version that the
			// client supports, to avoid confusing the client.
			var currentProtocolVersion = CurrentProtocolVersion;
			var versionToAdvertise = Math.Min(CurrentProtocolVersion, clientProtocolVersion);

			challengeBuilder.SetProtocolVersion(versionToAdvertise);

			var challenge = challengeBuilder.SetChallenge(AuthData.NewBuilder().SetAuthData(brokerData.auth_data).SetAuthMethodName(authMethod).Build()).Build();

			var res = SerializeWithSize(BaseCommand.NewBuilder().SetType(BaseCommand.Type.AuthChallenge).SetAuthChallenge(challenge));
			return res;
		}
		
		public static IByteBuffer NewSendError(long producerId, long sequenceId, ServerError error, string errorMsg)
		{
			var sendErrorBuilder = CommandSendError.NewBuilder();
			sendErrorBuilder.SetProducerId(producerId);
			sendErrorBuilder.SetSequenceId(sequenceId);
			sendErrorBuilder.SetError(error);
			sendErrorBuilder.SetMessage(errorMsg);
			var sendError = sendErrorBuilder.Build();
			var res = SerializeWithSize(BaseCommand.NewBuilder().SetType(BaseCommand.Type.SendError).SetSendError(sendError));
			
			return res;
		}


		public static bool HasChecksum(IByteBuffer buffer)
		{
			return buffer.GetShort(buffer.ReaderIndex) == MagicCrc32C;
		}

		/// <summary>
		/// Read the checksum and advance the reader index in the buffer.
		/// 
		/// <para>Note: This method assume the checksum presence was already verified before.
		/// </para>
		/// </summary>
		public static int ReadChecksum(IByteBuffer buffer)
		{
			buffer.SkipBytes(2); //skip magic bytes
			return buffer.ReadInt();
		}

		public static void SkipChecksumIfPresent(IByteBuffer buffer)
		{
			if (HasChecksum(buffer))
			{
				ReadChecksum(buffer);
			}
		}
		
		public static MessageMetadata ParseMessageMetadata(IByteBuffer buffer)
		{
			try
			{
				// initially reader-index may point to start_of_checksum : increment reader-index to start_of_metadata
				// to parse metadata
				SkipChecksumIfPresent(buffer);
                var bufferbytes = buffer.Array;
                return bufferbytes.FromByteArray<MessageMetadata>();
			}
			catch (IOException e)
			{
				throw new Exception(e.Message, e);
			}
		}

		public static void SkipMessageMetadata(IByteBuffer buffer)
		{
			// initially reader-index may point to start_of_checksum : increment reader-index to start_of_metadata to parse
			// metadata
			SkipChecksumIfPresent(buffer);
			var metadataSize = (int) buffer.ReadUnsignedInt();
			buffer.SkipBytes(metadataSize);
		}

		public static IByteBuffer NewSend(long producerId, long sequenceId, int numMessaegs, MessageMetadata messageMetadata, IByteBuffer payload)
		{
			return NewSend(producerId, sequenceId, numMessaegs, 0, 0, messageMetadata, payload);
		}

		public static IByteBuffer NewSend(long producerId, long lowestSequenceId, long highestSequenceId, int numMessaegs, MessageMetadata messageMetadata, IByteBuffer payload)
		{
			return NewSend(producerId, lowestSequenceId, highestSequenceId, numMessaegs, 0, 0, messageMetadata, payload);
		}

		public static IByteBuffer NewSend(long producerId, long sequenceId, int numMessages, long txnIdLeastBits, long txnIdMostBits, MessageMetadata messageData, IByteBuffer payload)
		{
			var sendBuilder = CommandSend.NewBuilder();
			sendBuilder.SetProducerId(producerId);
			sendBuilder.SetSequenceId(sequenceId);
			if (numMessages > 1)
			{
				sendBuilder.SetNumMessages(numMessages);
			}
			if (txnIdLeastBits > 0)
			{
				sendBuilder.SetTxnidLeastBits(txnIdLeastBits);
			}
			if (txnIdMostBits > 0)
			{
				sendBuilder.SetTxnidMostBits(txnIdMostBits);
			}
			var send = sendBuilder.Build();

			var res = SerializeCommandSendWithSize(BaseCommand.NewBuilder().SetType(BaseCommand.Type.Send).SetSend(send), messageData, payload.Array);
			return res;
		}

		public static IByteBuffer NewSend(long producerId, long lowestSequenceId, long highestSequenceId, int numMessages, long txnIdLeastBits, long txnIdMostBits, MessageMetadata messageData, IByteBuffer payload)
		{
			var sendBuilder = CommandSend.NewBuilder();
			sendBuilder.SetProducerId(producerId);
			sendBuilder.SetSequenceId(lowestSequenceId);
			sendBuilder.SetHighestSequenceId(highestSequenceId);
			if (numMessages > 1)
			{
				sendBuilder.SetNumMessages(numMessages);
			}
			if (txnIdLeastBits > 0)
			{
				sendBuilder.SetTxnidLeastBits(txnIdLeastBits);
			}
			if (txnIdMostBits > 0)
			{
				sendBuilder.SetTxnidMostBits(txnIdMostBits);
			}
			var send = sendBuilder.Build();

			var res = Serializer.Serialize(BaseCommand.NewBuilder().SetType(BaseCommand.Type.Send).SetSend(send).Build(), messageData, new ReadOnlySequence<byte>(payload.Array));
			return Unpooled.WrappedBuffer(res.ToArray());
		}

		public static IByteBuffer NewSubscribe(string topic, string subscription, long consumerId, long requestId, CommandSubscribe.SubType subType, int priorityLevel, string consumerName, long resetStartMessageBackInSeconds)
		{
			return NewSubscribe(topic, subscription, consumerId, requestId, subType, priorityLevel, consumerName, true, null, new Dictionary<string,string>(), false, false, CommandSubscribe.InitialPosition.Earliest, resetStartMessageBackInSeconds, null, true);
		}

		public static IByteBuffer NewSubscribe(string topic, string subscription, long consumerId, long requestId, CommandSubscribe.SubType subType, int priorityLevel, string consumerName, bool isDurable, MessageIdData startMessageId, IDictionary<string, string> metadata, bool readCompacted, bool isReplicated, CommandSubscribe.InitialPosition subscriptionInitialPosition, long startMessageRollbackDurationInSec, SchemaInfo schemaInfo, bool createTopicIfDoesNotExist)
		{
            return NewSubscribe(topic, subscription, consumerId, requestId, subType, priorityLevel, consumerName, isDurable, startMessageId, metadata, readCompacted, isReplicated, subscriptionInitialPosition, startMessageRollbackDurationInSec, schemaInfo, createTopicIfDoesNotExist, null);
		}

		public static IByteBuffer NewSubscribe(string topic, string subscription, long consumerId, long requestId, CommandSubscribe.SubType subType, int priorityLevel, string consumerName, bool isDurable, MessageIdData startMessageId, IDictionary<string, string> metadata, bool readCompacted, bool isReplicated, CommandSubscribe.InitialPosition subscriptionInitialPosition, long startMessageRollbackDurationInSec, SchemaInfo schemaInfo, bool createTopicIfDoesNotExist, Api.KeySharedPolicy keySharedPolicy)
		{
			var subscribeBuilder = CommandSubscribe.NewBuilder();
			subscribeBuilder.SetTopic(topic);
			subscribeBuilder.SetSubscription(subscription);
			subscribeBuilder.SetSubType(subType);
			subscribeBuilder.SetConsumerId(consumerId);
			subscribeBuilder.SetConsumerName(consumerName);
			subscribeBuilder.SetRequestId(requestId);
			subscribeBuilder.SetPriorityLevel(priorityLevel);
			subscribeBuilder.SetDurable(isDurable);
			subscribeBuilder.SetReadCompacted(readCompacted);
			subscribeBuilder.SetInitialPosition(subscriptionInitialPosition);
			subscribeBuilder.SetReplicateSubscriptionState(isReplicated);
			subscribeBuilder.SetForceTopicCreation(createTopicIfDoesNotExist);

			if (keySharedPolicy != null)
			{
				switch (keySharedPolicy.KeySharedMode)
				{
					case Api.KeySharedMode.AutoSplit:
						subscribeBuilder.SetKeySharedMeta(KeySharedMeta.NewBuilder().SetKeySharedMode(KeySharedMode.AutoSplit));
						break;
					case Api.KeySharedMode.Sticky:
						var builder = KeySharedMeta.NewBuilder().SetKeySharedMode(KeySharedMode.Sticky);
						var ranges = ((Api.KeySharedPolicy.KeySharedPolicySticky) keySharedPolicy).GetRanges().Ranges;
						foreach (var range in ranges)
						{
							builder.AddHashRanges(IntRange.NewBuilder().SetStart(range.Start).SetEnd(range.End));
						}
						subscribeBuilder.SetKeySharedMeta(builder);
						break;
				}
			}

			if (startMessageId != null)
			{
				subscribeBuilder.SetStartMessageId(startMessageId);
			}
			if (startMessageRollbackDurationInSec > 0)
			{
				subscribeBuilder.SetStartMessageRollbackDurationSec(startMessageRollbackDurationInSec);
			}
			subscribeBuilder.AddAllMetadata(CommandUtils.ToKeyValueList(metadata));

			Proto.Schema schema = null;
			if (schemaInfo != null)
			{
				schema = GetSchema(schemaInfo);
				subscribeBuilder.SetSchema(schema);
			}

			var subscribe = subscribeBuilder.Build();
			var res = SerializeWithSize(BaseCommand.NewBuilder().SetType(BaseCommand.Type.Subscribe).SetSubscribe(subscribe));

            return res;
		}

		public static IByteBuffer NewUnsubscribe(long consumerId, long requestId)
		{
			var unsubscribeBuilder = CommandUnsubscribe.NewBuilder();
			unsubscribeBuilder.SetConsumerId(consumerId);
			unsubscribeBuilder.SetRequestId(requestId);
			var unsubscribe = unsubscribeBuilder.Build();
			var res = SerializeWithSize(BaseCommand.NewBuilder().SetType(BaseCommand.Type.Unsubscribe).SetUnsubscribe(unsubscribe));
			return res;
		}

		public static IByteBuffer NewActiveConsumerChange(long consumerId, bool isActive)
		{
			var changeBuilder = CommandActiveConsumerChange.NewBuilder().SetConsumerId(consumerId).SetIsActive(isActive);

			var change = changeBuilder.Build();
			var res = SerializeWithSize(BaseCommand.NewBuilder().SetType(BaseCommand.Type.ActiveConsumerChange).SetActiveConsumerChange(change));
			return res;
		}

		public static IByteBuffer NewSeek(long consumerId, long requestId, long ledgerId, long entryId)
		{
			var seekBuilder = CommandSeek.NewBuilder();
			seekBuilder.SetConsumerId(consumerId);
			seekBuilder.SetRequestId(requestId);

			var messageIdBuilder = MessageIdData.NewBuilder();
			messageIdBuilder.SetLedgerId(ledgerId);
			messageIdBuilder.SetEntryId(entryId);
			var messageId = messageIdBuilder.Build();
			seekBuilder.SetMessageId(messageId);

			var seek = seekBuilder.Build();
			var res = SerializeWithSize(BaseCommand.NewBuilder().SetType(BaseCommand.Type.Seek).SetSeek(seek));
			
			return res;
		}

		public static IByteBuffer NewSeek(long consumerId, long requestId, long timestamp)
		{
			var seekBuilder = CommandSeek.NewBuilder();
			seekBuilder.SetConsumerId(consumerId);
			seekBuilder.SetRequestId(requestId);

			seekBuilder.SetMessagePublishTime(timestamp);

			var seek = seekBuilder.Build();
			var res = SerializeWithSize(BaseCommand.NewBuilder().SetType(BaseCommand.Type.Seek).SetSeek(seek));

			return res;
		}

		public static IByteBuffer NewCloseConsumer(long consumerId, long requestId)
		{
			var closeConsumerBuilder = CommandCloseConsumer.NewBuilder();
			closeConsumerBuilder.SetConsumerId(consumerId);
			closeConsumerBuilder.SetRequestId(requestId);
			var closeConsumer = closeConsumerBuilder.Build();
			var res = SerializeWithSize(BaseCommand.NewBuilder().SetType(BaseCommand.Type.CloseConsumer).SetCloseConsumer(closeConsumer));
			
			return res;
		}

		public static IByteBuffer NewReachedEndOfTopic(long consumerId)
		{
			var reachedEndOfTopicBuilder = CommandReachedEndOfTopic.NewBuilder();
			reachedEndOfTopicBuilder.SetConsumerId(consumerId);
			var reachedEndOfTopic = reachedEndOfTopicBuilder.Build();
			var res = SerializeWithSize(BaseCommand.NewBuilder().SetType(BaseCommand.Type.ReachedEndOfTopic).SetReachedEndOfTopic(reachedEndOfTopic));
			
			return res;
		}

		public static IByteBuffer NewCloseProducer(long producerId, long requestId)
		{
			var closeProducerBuilder = CommandCloseProducer.NewBuilder();
			closeProducerBuilder.SetProducerId(producerId);
			closeProducerBuilder.SetRequestId(requestId);
			var closeProducer = closeProducerBuilder.Build();
			var res = SerializeWithSize(BaseCommand.NewBuilder().SetType(BaseCommand.Type.CloseProducer).SetCloseProducer(closeProducerBuilder));
			
			return res;
		}

		public static IByteBuffer NewProducer(string topic, long producerId, long requestId, string producerName, IDictionary<string, string> metadata)
		{
			return NewProducer(topic, producerId, requestId, producerName, false, metadata);
		}

		public static IByteBuffer NewProducer(string topic, long producerId, long requestId, string producerName, bool encrypted, IDictionary<string, string> metadata)
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
				return SchemaType.None;
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
				return SchemaType.None;
			}
			else
			{
				return SchemaType.ValueOf(type.Value);
			}
		}
		private static Proto.Schema GetSchema(SchemaInfo schemaInfo)
		{
			var builder = Proto.Schema.NewBuilder().SetName(schemaInfo.Name).SetSchemaData((byte[])(object)schemaInfo.Schema).SetType(GetSchemaType(schemaInfo.Type)).AddAllProperties(schemaInfo.Properties.ToList().Select(entry => KeyValue.NewBuilder().SetKey(entry.Key).SetValue(entry.Value).Build()).ToList());
			var schema = builder.Build();
			return schema;
		}

		public static IByteBuffer NewProducer(string topic, long producerId, long requestId, string producerName, bool encrypted, IDictionary<string, string> metadata, SchemaInfo schemaInfo, long epoch, bool userProvidedProducerName)
		{
			var producerBuilder = CommandProducer.NewBuilder();
			producerBuilder.SetTopic(topic);
			producerBuilder.SetProducerId(producerId);
			producerBuilder.SetRequestId(requestId);
			producerBuilder.SetEpoch(epoch);
			if (!ReferenceEquals(producerName, null))
			{
				producerBuilder.SetProducerName(producerName);
			}
			producerBuilder.SetUserProvidedProducerName(userProvidedProducerName);
			producerBuilder.SetEncrypted(encrypted);

			producerBuilder.AddAllMetadata(CommandUtils.ToKeyValueList(metadata));

			if (null != schemaInfo)
			{
				producerBuilder.SetSchema(GetSchema(schemaInfo));
			}

			var producer = producerBuilder.Build();
			var res = SerializeWithSize(BaseCommand.NewBuilder().SetType(BaseCommand.Type.Producer).SetProducer(producer));
			
			return res;
		}

		public static IByteBuffer NewPartitionMetadataRequest(string topic, long requestId)
		{
			var partitionMetadataBuilder = CommandPartitionedTopicMetadata.NewBuilder();
			partitionMetadataBuilder.SetTopic(topic);
			partitionMetadataBuilder.SetRequestId(requestId);
			var partitionMetadata = partitionMetadataBuilder.Build();
			var res = SerializeWithSize(BaseCommand.NewBuilder().SetType(BaseCommand.Type.PartitionedMetadata).SetPartitionMetadata(partitionMetadata));
			
			return res;
		}

		public static IByteBuffer NewLookup(string topic, bool authoritative, long requestId)
		{
			var lookupTopicBuilder = CommandLookupTopic.NewBuilder();
			lookupTopicBuilder.SetTopic(topic);
			lookupTopicBuilder.SetRequestId(requestId);
			lookupTopicBuilder.SetAuthoritative(authoritative);
			var lookupBroker = lookupTopicBuilder.Build();
			var res = SerializeWithSize(BaseCommand.NewBuilder().SetType(BaseCommand.Type.Lookup).SetLookupTopic(lookupBroker));
			
			return res;
		}

		public static IByteBuffer NewMultiMessageAck(long consumerId, IList<KeyValuePair<long, long>> entries)
		{
			var ackBuilder = CommandAck.NewBuilder();
			ackBuilder.SetConsumerId(consumerId);
			ackBuilder.SetAckType(CommandAck.AckType.Individual);

			var entriesCount = entries.Count;
			for (var i = 0; i < entriesCount; i++)
			{
				var ledgerId = entries[i].Key;
				var entryId = entries[i].Value;

				var messageIdDataBuilder = MessageIdData.NewBuilder();
				messageIdDataBuilder.SetLedgerId(ledgerId);
				messageIdDataBuilder.SetEntryId(entryId);
				var messageIdData = messageIdDataBuilder.Build();
				ackBuilder.AddMessageId(messageIdData);
			}

			var ack = ackBuilder.Build();

			var res = SerializeWithSize(BaseCommand.NewBuilder().SetType(BaseCommand.Type.Ack).SetAck(ack));

			return res;
		}

		public static IByteBuffer NewAck(long consumerId, long ledgerId, long entryId, CommandAck.AckType ackType, CommandAck.ValidationError? validationError, IDictionary<string, long> properties)
		{
			return NewAck(consumerId, ledgerId, entryId, ackType, validationError, properties, 0, 0);
		}

		public static IByteBuffer NewAck(long consumerId, long ledgerId, long entryId, CommandAck.AckType ackType, CommandAck.ValidationError? validationError, IDictionary<string, long> properties, long txnIdLeastBits, long txnIdMostBits)
		{
			var ackBuilder = CommandAck.NewBuilder();
			ackBuilder.SetConsumerId(consumerId);
			ackBuilder.SetAckType(ackType);
			var messageIdDataBuilder = MessageIdData.NewBuilder();
			messageIdDataBuilder.SetLedgerId(ledgerId);
			messageIdDataBuilder.SetEntryId(entryId);
			var messageIdData = messageIdDataBuilder.Build();
			ackBuilder.AddMessageId(messageIdData);
			if (validationError != null)
			{
				ackBuilder.SetValidationError(validationError);
			}
			if (txnIdMostBits > 0)
			{
				ackBuilder.SetTxnidMostBits(txnIdMostBits);
			}
			if (txnIdLeastBits > 0)
			{
				ackBuilder.SetTxnidLeastBits(txnIdLeastBits);
			}
			foreach (KeyValuePair<string, long> e in properties.SetOfKeyValuePairs())
			{
				ackBuilder.AddProperties(KeyLongValue.NewBuilder().SetKey(e.Key).SetValue(e.Value).Build());
			}
			var ack = ackBuilder.Build();

			var res = SerializeWithSize(BaseCommand.NewBuilder().SetType(BaseCommand.Type.Ack).SetAck(ack));
			
			return res;
		}

		public static IByteBuffer NewFlow(long consumerId, int messagePermits)
		{
			var flowBuilder = CommandFlow.NewBuilder();
			flowBuilder.SetConsumerId(consumerId);
			flowBuilder.SetMessagePermits(messagePermits);
			var flow = flowBuilder.Build();

			var res = SerializeWithSize(BaseCommand.NewBuilder().SetType(BaseCommand.Type.Flow).SetFlow(flowBuilder));
			
			return res;
		}

		public static IByteBuffer NewRedeliverUnacknowledgedMessages(long consumerId)
		{
			var redeliverBuilder = CommandRedeliverUnacknowledgedMessages.NewBuilder();
			redeliverBuilder.SetConsumerId(consumerId);
			var redeliver = redeliverBuilder.Build();
			var res = SerializeWithSize(BaseCommand.NewBuilder().SetType(BaseCommand.Type.RedeliverUnacknowledgedMessages).SetRedeliverUnacknowledgedMessages(redeliverBuilder));
			
			return res;
		}

		public static IByteBuffer NewRedeliverUnacknowledgedMessages(long consumerId, IList<MessageIdData> messageIds)
		{
			var redeliverBuilder = CommandRedeliverUnacknowledgedMessages.NewBuilder();
			redeliverBuilder.SetConsumerId(consumerId);
			redeliverBuilder.AddAllMessageIds(messageIds);
			var redeliver = redeliverBuilder.Build();
			var res = SerializeWithSize(BaseCommand.NewBuilder().SetType(BaseCommand.Type.RedeliverUnacknowledgedMessages).SetRedeliverUnacknowledgedMessages(redeliverBuilder));
		    return res;
		}

		public static IByteBuffer NewGetTopicsOfNamespaceRequest(string @namespace, long requestId, CommandGetTopicsOfNamespace.Mode mode)
		{
			var topicsBuilder = CommandGetTopicsOfNamespace.NewBuilder();
			topicsBuilder.SetNamespace(@namespace).SetRequestId(requestId).SetMode(mode);

			var topicsCommand = topicsBuilder.Build();
			var res = SerializeWithSize(BaseCommand.NewBuilder().SetType(BaseCommand.Type.GetTopicsOfNamespace).SetGetTopicsOfNamespace(topicsCommand));
			
			return res;
		}
        private static readonly IByteBuffer CmdPing;

		static Commands()
		{
			var serializedCmdPing = SerializeWithSize(BaseCommand.NewBuilder().SetType(BaseCommand.Type.Ping).SetPing(CommandPing.NewBuilder().Build()));
			CmdPing = Unpooled.CopiedBuffer(serializedCmdPing);
			serializedCmdPing.Release();
			var serializedCmdPong = SerializeWithSize(BaseCommand.NewBuilder().SetType(BaseCommand.Type.Pong).SetPong(CommandPong.NewBuilder().Build()));
			CmdPong = Unpooled.CopiedBuffer(serializedCmdPong);
			serializedCmdPong.Release();
		}

		internal static IByteBuffer NewPing()
		{
			return CmdPing.RetainedDuplicate();
		}

		private static readonly IByteBuffer CmdPong;


		internal static IByteBuffer NewPong()
		{
			return CmdPong.RetainedDuplicate();
		}

		public static IByteBuffer NewGetLastMessageId(long consumerId, long requestId)
		{
			var cmdBuilder = CommandGetLastMessageId.NewBuilder();
			cmdBuilder.SetConsumerId(consumerId).SetRequestId(requestId);

			var res = SerializeWithSize(BaseCommand.NewBuilder().SetType(BaseCommand.Type.GetLastMessageId).SetGetLastMessageId(cmdBuilder.Build()));
			
			return res;
		}

		public static IByteBuffer NewGetSchema(long requestId, string topic, SchemaVersion version)
		{
			var schema = CommandGetSchema.NewBuilder().SetRequestId(requestId);
			schema.SetTopic(topic);
			if (version != null)
			{
				schema.SetSchemaVersion((byte[])(object)version.Bytes());
			}

			var getSchema = schema.Build();

			var res = SerializeWithSize(BaseCommand.NewBuilder().SetType(BaseCommand.Type.GetSchema).SetGetSchema(getSchema));
			
			return res;
		}

		public static IByteBuffer NewGetOrCreateSchema(long requestId, string topic, SchemaInfo schemaInfo)
		{
			var getOrCreateSchema = CommandGetOrCreateSchema.NewBuilder().SetRequestId(requestId).SetTopic(topic).SetSchema(GetSchema(schemaInfo)).Build();
			var res = SerializeWithSize(BaseCommand.NewBuilder().SetType(BaseCommand.Type.GetOrCreateSchema).SetGetOrCreateSchema(getOrCreateSchema));
			
			return res;
		}
		
		// ---- transaction related ----

		public static IByteBuffer NewTxn(long tcId, long requestId, long ttlSeconds)
		{
			var commandNewTxn = CommandNewTxn.NewBuilder().SetTcId(tcId).SetRequestId(requestId).SetTxnTtlSeconds(ttlSeconds).Build();
			var res = SerializeWithSize(BaseCommand.NewBuilder().SetType(BaseCommand.Type.NewTxn).SetNewTxn(commandNewTxn));
			
			return res;
		}

		public static IByteBuffer NewAddPartitionToTxn(long requestId, long txnIdLeastBits, long txnIdMostBits)
		{
			var commandAddPartitionToTxn = CommandAddPartitionToTxn.NewBuilder().SetRequestId(requestId).SetTxnidLeastBits(txnIdLeastBits).SetTxnidMostBits(txnIdMostBits).Build();
			var res = SerializeWithSize(BaseCommand.NewBuilder().SetType(BaseCommand.Type.AddPartitionToTxn).SetAddPartitionToTxn(commandAddPartitionToTxn));
			
			return res;
		}

		public static IByteBuffer NewAddSubscriptionToTxn(long requestId, long txnIdLeastBits, long txnIdMostBits, IList<Subscription> subscription)
		{
			var commandAddSubscriptionToTxn = CommandAddSubscriptionToTxn.NewBuilder().SetRequestId(requestId).SetTxnidLeastBits(txnIdLeastBits).SetTxnidMostBits(txnIdMostBits).AddAllSubscription(subscription).Build();
			var res = SerializeWithSize(BaseCommand.NewBuilder().SetType(BaseCommand.Type.AddSubscriptionToTxn).SetAddSubscriptionToTxn(commandAddSubscriptionToTxn));
			
			return res;
		}

		public static IByteBuffer NewEndTxn(long requestId, long txnIdLeastBits, long txnIdMostBits, TxnAction txnAction)
		{
			var commandEndTxn = CommandEndTxn.NewBuilder().SetRequestId(requestId).SetTxnidLeastBits(txnIdLeastBits).SetTxnidMostBits(txnIdMostBits).SetTxnAction(txnAction).Build();
			var res = SerializeWithSize(BaseCommand.NewBuilder().SetType(BaseCommand.Type.EndTxn).SetEndTxn(commandEndTxn));
			
			return res;
		}

		public static IByteBuffer NewEndTxnOnPartition(long requestId, long txnIdLeastBits, long txnIdMostBits, string topic, TxnAction txnAction)
		{
			var txnEndOnPartition = CommandEndTxnOnPartition.NewBuilder().SetRequestId(requestId).SetTxnidLeastBits(txnIdLeastBits).SetTxnidMostBits(txnIdMostBits).SetTopic(topic).SetTxnAction(txnAction);
			var res = SerializeWithSize(BaseCommand.NewBuilder().SetType(BaseCommand.Type.EndTxnOnPartition).SetEndTxnOnPartition(txnEndOnPartition));
			
			return res;
		}

		
		public static IByteBuffer NewEndTxnOnSubscription(long requestId, long txnIdLeastBits, long txnIdMostBits, Subscription subscription, TxnAction txnAction)
		{
			var commandEndTxnOnSubscription = CommandEndTxnOnSubscription.NewBuilder().SetRequestId(requestId).SetTxnidLeastBits(txnIdLeastBits).SetTxnidMostBits(txnIdMostBits).SetSubscription(subscription).SetTxnAction(txnAction).Build();
			var res = SerializeWithSize(BaseCommand.NewBuilder().SetType(BaseCommand.Type.EndTxnOnSubscription).SetEndTxnOnSubscription(commandEndTxnOnSubscription));
			
			return res;
		}
        public static IByteBuffer SerializeWithSize(BaseCommand.Builder cmdBuilder)
        {
            // / Wire format
            // [TOTAL_SIZE] [CMD_SIZE][CMD]
			var output = new MemoryStream();
            using (var stream = new MemoryStream())
            {
                // write fake totalLength
                for (var i = 0; i < 4; i++)
                {
                    stream.WriteByte(0);

                }
                
                // write commandPayload
                ProtoBuf.Serializer.SerializeWithLengthPrefix(stream, cmdBuilder.Build(), PrefixStyle.Fixed32BigEndian);

                var frameSize = (int)stream.Length;

                var totalSize = frameSize - 4;

                //write total size and command size
                stream.Seek(0L, SeekOrigin.Begin);

                using var binaryWriter = new BinaryWriter(stream);

                binaryWriter.Write(ProtoLizer.Int32ToBigEndian(totalSize));

                stream.Seek(0L, SeekOrigin.Begin);
                stream.CopyToAsync(output);

            }
            
            return Unpooled.WrappedBuffer(output.ToArray());
        }
		
		public static int ComputeChecksum(IByteBuffer byteBuffer)
        {
            return Crc32CIntChecksum.ComputeChecksum(byteBuffer);
        }
        public static int ResumeChecksum(int prev, IByteBuffer byteBuffer)
        {
            return Crc32CIntChecksum.ResumeChecksum(prev, byteBuffer);
        }
		public static IByteBuffer SerializeCommandSendWithSize(BaseCommand.Builder cmdBuilder, MessageMetadata msgMetadata, byte[] payload)
		{
			// / Wire format
			// [MAGIC_NUMBER][CHECKSUM] [METADATA_SIZE][METADATA] [PAYLOAD]
            var output = new MemoryStream();
            using (var stream = new MemoryStream())
            {
				// write fake totalLength
				for (var i = 0; i < 4; i++)
                {
                    stream.WriteByte(0);
                }

				// write commandPayload
                ProtoBuf.Serializer.SerializeWithLengthPrefix(stream, cmdBuilder.Build(), PrefixStyle.Fixed32BigEndian);


                var stream1Size = (int) stream.Length;

                // write magic number 0x0e01
                stream.WriteByte(14);

                stream.WriteByte(1);

				// write fake CRC sum and fake metadata length
				for (var i = 0; i < 4; i++)
                {
                    stream.WriteByte(0);
                }

				// write metadata
                ProtoBuf.Serializer.SerializeWithLengthPrefix(stream, msgMetadata, PrefixStyle.Fixed32BigEndian);

                var stream2Size = (int) stream.Length;
                var totalMetadataSize = stream2Size - stream1Size - 6;

                // write payload
                stream.Write(payload, 0, payload.Length);


                var frameSize = (int) stream.Length;
                var totalSize = frameSize - 4;

                var payloadSize = frameSize - stream2Size;


                var crcStart = stream1Size + 2;

                var crcPayloadStart = crcStart + 4;

                // write missing sizes
                using var binaryWriter = new BinaryWriter(stream);

                //write CRC
                stream.Seek(crcPayloadStart, SeekOrigin.Begin);

                var crc = (int) CRC32C.Get(0u, stream, totalMetadataSize + payloadSize);

                stream.Seek(crcStart, SeekOrigin.Begin);

                binaryWriter.Write(ProtoLizer.Int32ToBigEndian(crc));

                //write total size and command size
                stream.Seek(0L, SeekOrigin.Begin);

                binaryWriter.Write(ProtoLizer.Int32ToBigEndian(totalSize));


                stream.Seek(0L, SeekOrigin.Begin);
                stream.CopyToAsync(output);
            }

            return Unpooled.WrappedBuffer(output.ToArray());
        }

		public static long InitBatchMessageMetadata(MessageMetadata.Builder builder)
		{
			var messageMetadata = MessageMetadata.NewBuilder();
			messageMetadata.SetPublishTime(builder.GetPublishTime());
			messageMetadata.SetProducerName(builder.GetProducerName());
			messageMetadata.SetSequenceId((long)builder.GetSequenceId());
			if (builder.HasReplicatedFrom())
			{
				messageMetadata.SetReplicatedFrom(builder.GetReplicatedFrom());
			}
			if (builder.ReplicateToList().Count > 0)
			{
				messageMetadata.AddAllReplicateTo(builder.ReplicateToList());
			}
			if (builder.HasSchemaVersion())
			{
				messageMetadata.SetSchemaVersion(builder.GetSchemaVersion());
			}
			return (long)builder.GetSequenceId();
		}

		public static IByteBuffer SerializeSingleMessageInBatchWithPayload(SingleMessageMetadata.Builder singleMessageMetadataBuilder, IByteBuffer payload, IByteBuffer batchBuffer)
        {
			var payLoadSize = payload.ReadableBytes;
			var singleMessageMetadata = singleMessageMetadataBuilder.SetPayloadSize(payLoadSize).Build();
			// serialize meta-data size, meta-data and payload for single message in batch
			var singleMsgMetadataSize = singleMessageMetadata.ByteLength();
			try
			{
				batchBuffer.WriteInt(singleMsgMetadataSize);
				var outStream = new MemoryStream(batchBuffer.Array);
				//singleMessageMetadata.WriteTo(outStream);
			}
			catch (IOException e)
			{
				throw new System.Exception(e.Message, e);
			}
			return batchBuffer.WriteBytes(payload);
		}

		public static IByteBuffer SerializeSingleMessageInBatchWithPayload(MessageMetadata.Builder msgBuilder, IByteBuffer payload, IByteBuffer batchBuffer)
		{

			// build single message meta-data
			var singleMessageMetadataBuilder = SingleMessageMetadata.NewBuilder();
			if (msgBuilder.HasPartitionKey())
			{
				singleMessageMetadataBuilder = singleMessageMetadataBuilder.SetPartitionKey(msgBuilder.GetPartitionKey()).SetPartitionKeyB64Encoded(msgBuilder.PartitionKeyB64Encoded);
			}
			if (msgBuilder.HasOrderingKey())
			{
				singleMessageMetadataBuilder = singleMessageMetadataBuilder.SetOrderingKey(msgBuilder.GetOrderingKey());
			}
			if (msgBuilder.Properties.Count > 0)
			{
				singleMessageMetadataBuilder = singleMessageMetadataBuilder.AddAllProperties(msgBuilder.Properties.ToList().Select(x => new KeyValue(){Key = x.Key, Value = x.Value}));
			}

			if (msgBuilder.HasEventTime())
			{
				singleMessageMetadataBuilder.SetEventTime(msgBuilder.EventTime);
			}

			if (msgBuilder.HasSequenceId())
			{
				singleMessageMetadataBuilder.SetSequenceId(msgBuilder.SequenceId());
			}

			try
			{
				return SerializeSingleMessageInBatchWithPayload(singleMessageMetadataBuilder, payload, batchBuffer);
			}
			finally
			{
				//singleMessageMetadataBuilder.Recycle();
			}
		}

		public static IByteBuffer DeSerializeSingleMessageInBatch(IByteBuffer uncompressedPayload, SingleMessageMetadata.Builder singleMessageMetadataBuilder, int index, int batchSize)
        {
            using var stream = new MemoryStream(uncompressedPayload.Array);

            using var binaryReader = new BinaryReader(stream);
            var singleMessageMetadata =
                ProtoBuf.Serializer.DeserializeWithLengthPrefix<SingleMessageMetadata>(stream,
                    PrefixStyle.Fixed32BigEndian);

            var singleMessagePayload = binaryReader.ReadBytes(singleMessageMetadata.PayloadSize);
			
			var single = singleMessageMetadataBuilder.Build();
			var singleMetaSize = (int) uncompressedPayload.ReadUnsignedInt();
			var writerIndex = uncompressedPayload.WriterIndex;
			var beginIndex = uncompressedPayload.ReaderIndex + singleMetaSize;
			uncompressedPayload.SetWriterIndex(beginIndex);
			//var stream = new CodedInputStream(uncompressedPayload.Array);
            //single.MergeFrom(stream);
            var singleMessagePayloadSize = 0L;//single.CalculateSize();

			var readerIndex = uncompressedPayload.ReaderIndex;
			uncompressedPayload.SetWriterIndex(writerIndex);

			// reader now points to beginning of payload read; so move it past message payload just read
			if (index < batchSize)
			{
				//uncompressedPayload.SetReaderIndex(readerIndex + singleMessagePayloadSize);
			}

            return null; //singleMessagePayload;
        }

		public static int GetNumberOfMessagesInBatch(IByteBuffer metadataAndPayload, string subscription, long consumerId)
		{
			var msgMetadata = PeekMessageMetadata(metadataAndPayload, subscription, consumerId);
			if (msgMetadata == null)
			{
				return -1;
			}
			else
			{
				var numMessagesInBatch = msgMetadata.NumMessagesInBatch;
				return numMessagesInBatch;
			}
		}

		public static MessageMetadata PeekMessageMetadata(IByteBuffer metadataAndPayload, string subscription, long consumerId)
		{
			try
			{
				// save the reader index and restore after parsing
				var readerIdx = metadataAndPayload.ReaderIndex;
				var metadata = ParseMessageMetadata(metadataAndPayload);
				metadataAndPayload.SetReaderIndex(readerIdx);

				return metadata;
			}
			catch (System.Exception T)
			{
				//log.error("[{}] [{}] Failed to parse message metadata", Subscription, ConsumerId, T);
				return null;
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
	}

}