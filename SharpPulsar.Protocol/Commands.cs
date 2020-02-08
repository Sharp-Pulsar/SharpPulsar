using DotNetty.Buffers;
using SharpPulsar.Common.Schema;
using SharpPulsar.Protocol.Proto;
using System;
using System.Collections.Generic;
using Microsoft.IO;
using System.IO;
using System.Net;
using Google.Protobuf;
using System.Text;
using SharpPulsar.Protocol.Extension;
using SharpPulsar.Common;
using SharpPulsar.Util.Protobuf;
using SharpPulsar.Shared;
using AuthData = SharpPulsar.Protocol.Proto.AuthData;
using SharpPulsar.Protocol.Schema;
using SharpPulsar.Common.Entity;
using System.Linq;
using Optional;
using SharpPulsar.Api.Schema;

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
			CommandConnect.Builder connectBuilder = CommandConnect.NewBuilder();
			connectBuilder.SetClientVersion(!string.ReferenceEquals(libVersion, null) ? libVersion : "Pulsar Client");
			connectBuilder.SetAuthMethodName(authMethodName);

			if ("ycav1".Equals(authMethodName))
			{
				// Handle the case of a client that gets updated before the broker and starts sending the string auth method
				// name. An example would be in broker-to-broker replication. We need to make sure the clients are still
				// passing both the enum and the string until all brokers are upgraded.
				connectBuilder.SetAuthMethod(AuthMethod.YcaV1);
			}

			if (!string.ReferenceEquals(targetBroker, null))
			{
				// When connecting through a proxy, we need to specify which broker do we want to be proxied through
				connectBuilder.SetProxyToBrokerUrl(targetBroker);
			}

			if (!string.ReferenceEquals(authData, null))
			{
				connectBuilder.SetAuthData(ByteString.CopyFromUtf8(authData));
			}

			if (!string.ReferenceEquals(originalPrincipal, null))
			{
				connectBuilder.SetOriginalPrincipal(originalPrincipal);
			}

			if (!string.ReferenceEquals(originalAuthData, null))
			{
				connectBuilder.SetOriginalAuthData(originalAuthData);
			}

			if (!string.ReferenceEquals(originalAuthMethod, null))
			{
				connectBuilder.SetOriginalAuthMethod(originalAuthMethod);
			}
			connectBuilder.SetProtocolVersion(protocolVersion);
			CommandConnect connect = connectBuilder.Build();
			IByteBuffer res = SerializeWithSize(BaseCommand.NewBuilder().SetType(BaseCommand.Types.Type.Connect).SetConnect(connect));
			connect.Recycle();
			connectBuilder.Recycle();
			return res;
		}

		public static IByteBuffer NewConnect(string authMethodName, AuthData authData, int protocolVersion, string libVersion, string targetBroker, string originalPrincipal, AuthData originalAuthData, string originalAuthMethod)
		{
			CommandConnect.Builder connectBuilder = CommandConnect.NewBuilder();
			connectBuilder.SetClientVersion(!string.ReferenceEquals(libVersion, null) ? libVersion : "Pulsar Client");
			connectBuilder.SetAuthMethodName(authMethodName);

			if (!string.ReferenceEquals(targetBroker, null))
			{
				// When connecting through a proxy, we need to specify which broker do we want to be proxied through
				connectBuilder.SetProxyToBrokerUrl(targetBroker);
			}

			if (authData != null)
			{
				connectBuilder.SetAuthData(authData.AuthData_);
			}

			if (!string.ReferenceEquals(originalPrincipal, null))
			{
				connectBuilder.SetOriginalPrincipal(originalPrincipal);
			}

			if (originalAuthData != null)
			{
				connectBuilder.SetOriginalAuthData(originalAuthData.AuthData_.ToStringUtf8());
			}

			if (!string.ReferenceEquals(originalAuthMethod, null))
			{
				connectBuilder.SetOriginalAuthMethod(originalAuthMethod);
			}
			connectBuilder.SetProtocolVersion(protocolVersion);
			CommandConnect connect = connectBuilder.Build();
			
			IByteBuffer res = SerializeWithSize(BaseCommand.NewBuilder().SetType(BaseCommand.Types.Type.Connect).SetConnect(connect));
			connect.Recycle();
			connectBuilder.Recycle();
			return res;
		}

		public static IByteBuffer NewConnected(int clientProtocoVersion)
		{
			return NewConnected(clientProtocoVersion, InvalidMaxMessageSize);
		}

		public static IByteBuffer NewConnected(int clientProtocolVersion, int maxMessageSize)
		{
			CommandConnected.Builder connectedBuilder = CommandConnected.NewBuilder();
			connectedBuilder.SetServerVersion("Pulsar Server");
			if (InvalidMaxMessageSize != maxMessageSize)
			{
				connectedBuilder.SetMaxMessageSize(maxMessageSize);
			}

			// If the broker supports a newer version of the protocol, it will anyway advertise the max version that the
			// client supports, to avoid confusing the client.
			int currentProtocolVersion = CurrentProtocolVersion;
			int versionToAdvertise = Math.Min(CurrentProtocolVersion, clientProtocolVersion);

			CommandConnected connected = connectedBuilder.Build();
			IByteBuffer res = SerializeWithSize(BaseCommand.NewBuilder().SetType(BaseCommand.Types.Type.Connected).SetConnected(connectedBuilder));
			connected.Recycle();
			connectedBuilder.Recycle();
			return res;
		}

		public static IByteBuffer NewAuthChallenge(string authMethod, AuthData brokerData, int clientProtocolVersion)
		{
			CommandAuthChallenge.Builder challengeBuilder = CommandAuthChallenge.NewBuilder();

			// If the broker supports a newer version of the protocol, it will anyway advertise the max version that the
			// client supports, to avoid confusing the client.
			int currentProtocolVersion = CurrentProtocolVersion;
			int versionToAdvertise = Math.Min(CurrentProtocolVersion, clientProtocolVersion);

			challengeBuilder.SetProtocolVersion(versionToAdvertise);

			CommandAuthChallenge challenge = challengeBuilder.SetChallenge(AuthData.NewBuilder().SetAuthData(brokerData.AuthData_).SetAuthMethodName(authMethod).Build()).Build();

			IByteBuffer res = SerializeWithSize(BaseCommand.NewBuilder().SetType(BaseCommand.Types.Type.AuthChallenge).SetAuthChallenge(challenge));
			challenge.Recycle();
			challengeBuilder.Recycle();
			return res;
		}

		public static IByteBuffer NewAuthResponse(string authMethod, AuthData clientData, int clientProtocolVersion, string clientVersion)
		{
			CommandAuthResponse.Builder responseBuilder = CommandAuthResponse.NewBuilder();

			responseBuilder.SetClientVersion(!string.ReferenceEquals(clientVersion, null) ? clientVersion : "Pulsar Client");
			responseBuilder.SetProtocolVersion(clientProtocolVersion);

			CommandAuthResponse response = responseBuilder.SetResponse(AuthData.NewBuilder().SetAuthData(clientData.AuthData_).SetAuthMethodName(authMethod).Build()).Build();

			IByteBuffer res = SerializeWithSize(BaseCommand.NewBuilder().SetType(BaseCommand.Types.Type.AuthResponse).SetAuthResponse(response));
			response.Recycle();
			responseBuilder.Recycle();
			return res;
		}

		public static IByteBuffer NewSuccess(long requestId)
		{
			CommandSuccess.Builder successBuilder = CommandSuccess.NewBuilder();
			successBuilder.SetRequestId(requestId);
			CommandSuccess success = successBuilder.Build();
			IByteBuffer res = SerializeWithSize(BaseCommand.NewBuilder().SetType(BaseCommand.Types.Type.Success).SetSuccess(success));
			successBuilder.Recycle();
			success.Recycle();
			return res;
		}

		public static IByteBuffer NewProducerSuccess(long requestId, string producerName, SchemaVersion schemaVersion)
		{
			return NewProducerSuccess(requestId, producerName, -1, schemaVersion);
		}

		public static IByteBuffer NewProducerSuccess(long requestId, string producerName, long lastSequenceId, SchemaVersion schemaVersion)
		{
			CommandProducerSuccess.Builder producerSuccessBuilder = CommandProducerSuccess.NewBuilder();
			producerSuccessBuilder.SetRequestId(requestId);
			producerSuccessBuilder.SetProducerName(producerName);
			producerSuccessBuilder.SetLastSequenceId(lastSequenceId);
			producerSuccessBuilder.SetSchemaVersion(ByteString.CopyFrom((byte[])(object)schemaVersion.Bytes()));
			CommandProducerSuccess producerSuccess = producerSuccessBuilder.Build();
			IByteBuffer res = SerializeWithSize(BaseCommand.NewBuilder().SetType(BaseCommand.Types.Type.ProducerSuccess).SetProducerSuccess(producerSuccess));
			producerSuccess.Recycle();
			producerSuccessBuilder.Recycle();
			return res;
		}

		public static IByteBuffer NewError(long requestId, ServerError error, string message)
		{
			CommandError.Builder cmdErrorBuilder = CommandError.NewBuilder();
			cmdErrorBuilder.SetRequestId(requestId);
			cmdErrorBuilder.SetError(error);
			cmdErrorBuilder.SetMessage(message);
			CommandError cmdError = cmdErrorBuilder.Build();
			IByteBuffer res = SerializeWithSize(BaseCommand.NewBuilder().SetType(BaseCommand.Types.Type.Error).SetError(cmdError));
			cmdError.Recycle();
			cmdErrorBuilder.Recycle();
			return res;

		}

		public static IByteBuffer NewSendReceipt(long producerId, long sequenceId, long highestId, long ledgerId, long entryId)
		{
			CommandSendReceipt.Builder sendReceiptBuilder = CommandSendReceipt.NewBuilder();
			sendReceiptBuilder.SetProducerId(producerId);
			sendReceiptBuilder.SetSequenceId(sequenceId);
			sendReceiptBuilder.SetHighestSequenceId(highestId);
			MessageIdData.Builder messageIdBuilder = MessageIdData.NewBuilder();
			messageIdBuilder.LedgerId = ledgerId;
			messageIdBuilder.EntryId = entryId;
			MessageIdData messageId = messageIdBuilder.Build();
			sendReceiptBuilder.SetMessageId(messageId);
			CommandSendReceipt sendReceipt = sendReceiptBuilder.Build();
			IByteBuffer res = SerializeWithSize(BaseCommand.NewBuilder().SetType(BaseCommand.Types.Type.SendReceipt).SetSendReceipt(sendReceipt));
			messageIdBuilder.Recycle();
			messageId.Recycle();
			sendReceiptBuilder.Recycle();
			sendReceipt.Recycle();
			return res;
		}

		public static IByteBuffer NewSendError(long producerId, long sequenceId, ServerError error, string errorMsg)
		{
			CommandSendError.Builder sendErrorBuilder = CommandSendError.NewBuilder();
			sendErrorBuilder.SetProducerId(producerId);
			sendErrorBuilder.SetSequenceId(sequenceId);
			sendErrorBuilder.SetError(error);
			sendErrorBuilder.SetMessage(errorMsg);
			CommandSendError sendError = sendErrorBuilder.Build();
			IByteBuffer res = SerializeWithSize(BaseCommand.NewBuilder().SetType(BaseCommand.Types.Type.SendError).SetSendError(sendError));
			sendErrorBuilder.Recycle();
			sendError.Recycle();
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
				int metadataSize = (int) buffer.ReadUnsignedInt();

				int writerIndex = buffer.WriterIndex;
				buffer.SetWriterIndex(buffer.ReaderIndex + metadataSize);
				ByteBufCodedInputStream stream = ByteBufCodedInputStream.Get(buffer);
				MessageMetadata.Builder messageMetadataBuilder = MessageMetadata.NewBuilder();
				MessageMetadata res = ((MessageMetadata.Builder)messageMetadataBuilder.MergeFrom(stream, null)).Build();
				buffer.SetWriterIndex(writerIndex);
				messageMetadataBuilder.Recycle();
				stream.Recycle();
				return res;
			}
			catch (IOException e)
			{
				throw new System.Exception(e.Message, e);
			}
		}

		public static void SkipMessageMetadata(IByteBuffer buffer)
		{
			// initially reader-index may point to start_of_checksum : increment reader-index to start_of_metadata to parse
			// metadata
			SkipChecksumIfPresent(buffer);
			int metadataSize = (int) buffer.ReadUnsignedInt();
			buffer.SkipBytes(metadataSize);
		}

		public static ByteBufPair NewMessage(long consumerId, MessageIdData messageId, int redeliveryCount, IByteBuffer metadataAndPayload)
		{
			CommandMessage.Builder msgBuilder = CommandMessage.NewBuilder();
			msgBuilder.SetConsumerId(consumerId);
			msgBuilder.SetMessageId(messageId);
			if (redeliveryCount > 0)
			{
				msgBuilder.SetRedeliveryCount(redeliveryCount);
			}
			CommandMessage msg = msgBuilder.Build();
			BaseCommand.Builder cmdBuilder = BaseCommand.NewBuilder();
			BaseCommand cmd = cmdBuilder.SetType(BaseCommand.Types.Type.Message).SetMessage(msg).Build();

			ByteBufPair res = SerializeCommandMessageWithSize(cmd, metadataAndPayload);
			cmd.Recycle();
			cmdBuilder.Recycle();
			msg.Recycle();
			msgBuilder.Recycle();
			return res;
		}

		public static ByteBufPair NewSend(long producerId, long sequenceId, int numMessaegs, ChecksumType checksumType, MessageMetadata messageMetadata, IByteBuffer payload)
		{
			return NewSend(producerId, sequenceId, numMessaegs, 0, 0, checksumType, messageMetadata, payload);
		}

		public static ByteBufPair NewSend(long producerId, long lowestSequenceId, long highestSequenceId, int numMessaegs, ChecksumType checksumType, MessageMetadata messageMetadata, IByteBuffer payload)
		{
			return NewSend(producerId, lowestSequenceId, highestSequenceId, numMessaegs, 0, 0, checksumType, messageMetadata, payload);
		}

		public static ByteBufPair NewSend(long producerId, long sequenceId, int numMessages, long txnIdLeastBits, long txnIdMostBits, ChecksumType checksumType, MessageMetadata messageData, IByteBuffer payload)
		{
			CommandSend.Builder sendBuilder = CommandSend.NewBuilder();
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
			CommandSend send = sendBuilder.Build();

			ByteBufPair res = SerializeCommandSendWithSize(BaseCommand.NewBuilder().SetType(BaseCommand.Types.Type.Send).SetSend(send), checksumType, messageData, payload);
			send.Recycle();
			sendBuilder.Recycle();
			return res;
		}

		public static ByteBufPair NewSend(long producerId, long lowestSequenceId, long highestSequenceId, int numMessages, long txnIdLeastBits, long txnIdMostBits, ChecksumType checksumType, MessageMetadata messageData, IByteBuffer payload)
		{
			CommandSend.Builder sendBuilder = CommandSend.NewBuilder();
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
			CommandSend send = sendBuilder.Build();

			ByteBufPair res = SerializeCommandSendWithSize(BaseCommand.NewBuilder().SetType(BaseCommand.Types.Type.Send).SetSend(send), checksumType, messageData, payload);
			send.Recycle();
			sendBuilder.Recycle();
			return res;
		}

		public static IByteBuffer NewSubscribe(string topic, string subscription, long consumerId, long requestId, CommandSubscribe.Types.SubType subType, int priorityLevel, string consumerName, long resetStartMessageBackInSeconds)
		{
			return NewSubscribe(topic, subscription, consumerId, requestId, subType, priorityLevel, consumerName, true, null, new Dictionary<string,string>(), false, false, CommandSubscribe.Types.InitialPosition.Earliest, resetStartMessageBackInSeconds, null, true);
		}

		public static IByteBuffer NewSubscribe(string topic, string subscription, long consumerId, long requestId, CommandSubscribe.Types.SubType subType, int priorityLevel, string consumerName, bool isDurable, MessageIdData startMessageId, IDictionary<string, string> metadata, bool readCompacted, bool isReplicated, CommandSubscribe.Types.InitialPosition subscriptionInitialPosition, long startMessageRollbackDurationInSec, SchemaInfo schemaInfo, bool createTopicIfDoesNotExist)
		{
					return NewSubscribe(topic, subscription, consumerId, requestId, subType, priorityLevel, consumerName, isDurable, startMessageId, metadata, readCompacted, isReplicated, subscriptionInitialPosition, startMessageRollbackDurationInSec, schemaInfo, createTopicIfDoesNotExist, null);
		}

		public static IByteBuffer NewSubscribe(string topic, string subscription, long consumerId, long requestId, CommandSubscribe.Types.SubType subType, int priorityLevel, string consumerName, bool isDurable, MessageIdData startMessageId, IDictionary<string, string> metadata, bool readCompacted, bool isReplicated, CommandSubscribe.Types.InitialPosition subscriptionInitialPosition, long startMessageRollbackDurationInSec, SchemaInfo schemaInfo, bool createTopicIfDoesNotExist, KeySharedPolicy keySharedPolicy)
		{
			CommandSubscribe.Builder subscribeBuilder = CommandSubscribe.NewBuilder();
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
					case Common.Enum.KeySharedMode.AUTO_SPLIT:
						subscribeBuilder.SetKeySharedMeta(KeySharedMeta.NewBuilder().SetKeySharedMode(KeySharedMode.AutoSplit));
						break;
					case Common.Enum.KeySharedMode.STICKY:
						KeySharedMeta.Builder builder = KeySharedMeta.NewBuilder().SetKeySharedMode(KeySharedMode.Sticky);
						IList<Common.Entity.Range> ranges = ((KeySharedPolicy.KeySharedPolicySticky) keySharedPolicy).GetRanges;
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

			CommandSubscribe subscribe = subscribeBuilder.Build();
			IByteBuffer res = SerializeWithSize(BaseCommand.NewBuilder().SetType(BaseCommand.Types.Type.Subscribe).SetSubscribe(subscribe));
			subscribeBuilder.Recycle();
			subscribe.Recycle();
			if (null != schema)
			{
				schema.Recycle();
			}
			return res;
		}

		public static IByteBuffer NewUnsubscribe(long consumerId, long requestId)
		{
			CommandUnsubscribe.Builder unsubscribeBuilder = CommandUnsubscribe.NewBuilder();
			unsubscribeBuilder.SetConsumerId(consumerId);
			unsubscribeBuilder.SetRequestId(requestId);
			CommandUnsubscribe unsubscribe = unsubscribeBuilder.Build();
			IByteBuffer res = SerializeWithSize(BaseCommand.NewBuilder().SetType(BaseCommand.Types.Type.Unsubscribe).SetUnsubscribe(unsubscribe));
			unsubscribeBuilder.Recycle();
			unsubscribe.Recycle();
			return res;
		}

		public static IByteBuffer NewActiveConsumerChange(long consumerId, bool isActive)
		{
			CommandActiveConsumerChange.Builder changeBuilder = CommandActiveConsumerChange.NewBuilder().SetConsumerId(consumerId).SetIsActive(isActive);

			CommandActiveConsumerChange change = changeBuilder.Build();
			IByteBuffer res = SerializeWithSize(BaseCommand.NewBuilder().SetType(BaseCommand.Types.Type.ActiveConsumerChange).SetActiveConsumerChange(change));
			changeBuilder.Recycle();
			change.Recycle();
			return res;
		}

		public static IByteBuffer NewSeek(long consumerId, long requestId, long ledgerId, long entryId)
		{
			CommandSeek.Builder seekBuilder = CommandSeek.NewBuilder();
			seekBuilder.SetConsumerId(consumerId);
			seekBuilder.SetRequestId(requestId);

			MessageIdData.Builder messageIdBuilder = MessageIdData.NewBuilder();
			messageIdBuilder.LedgerId = ledgerId;
			messageIdBuilder.EntryId = entryId;
			MessageIdData messageId = messageIdBuilder.Build();
			seekBuilder.SetMessageId(messageId);

			CommandSeek seek = seekBuilder.Build();
			IByteBuffer res = SerializeWithSize(BaseCommand.NewBuilder().SetType(BaseCommand.Types.Type.Seek).SetSeek(seek));
			messageId.Recycle();
			messageIdBuilder.Recycle();
			seekBuilder.Recycle();
			seek.Recycle();
			return res;
		}

		public static IByteBuffer NewSeek(long consumerId, long requestId, long timestamp)
		{
			CommandSeek.Builder seekBuilder = CommandSeek.NewBuilder();
			seekBuilder.SetConsumerId(consumerId);
			seekBuilder.SetRequestId(requestId);

			seekBuilder.SetMessagePublishTime(timestamp);

			CommandSeek seek = seekBuilder.Build();
			IByteBuffer res = SerializeWithSize(BaseCommand.NewBuilder().SetType(BaseCommand.Types.Type.Seek).SetSeek(seek));

			seekBuilder.Recycle();
			seek.Recycle();
			return res;
		}

		public static IByteBuffer NewCloseConsumer(long consumerId, long requestId)
		{
			CommandCloseConsumer.Builder closeConsumerBuilder = CommandCloseConsumer.NewBuilder();
			closeConsumerBuilder.SetConsumerId(consumerId);
			closeConsumerBuilder.SetRequestId(requestId);
			CommandCloseConsumer closeConsumer = closeConsumerBuilder.Build();
			IByteBuffer res = SerializeWithSize(BaseCommand.NewBuilder().SetType(BaseCommand.Types.Type.CloseConsumer).SetCloseConsumer(closeConsumer));
			closeConsumerBuilder.Recycle();
			closeConsumer.Recycle();
			return res;
		}

		public static IByteBuffer NewReachedEndOfTopic(long consumerId)
		{
			CommandReachedEndOfTopic.Builder reachedEndOfTopicBuilder = CommandReachedEndOfTopic.NewBuilder();
			reachedEndOfTopicBuilder.SetConsumerId(consumerId);
			CommandReachedEndOfTopic reachedEndOfTopic = reachedEndOfTopicBuilder.Build();
			IByteBuffer res = SerializeWithSize(BaseCommand.NewBuilder().SetType(BaseCommand.Types.Type.ReachedEndOfTopic).SetReachedEndOfTopic(reachedEndOfTopic));
			reachedEndOfTopicBuilder.Recycle();
			reachedEndOfTopic.Recycle();
			return res;
		}

		public static IByteBuffer NewCloseProducer(long producerId, long requestId)
		{
			CommandCloseProducer.Builder closeProducerBuilder = CommandCloseProducer.NewBuilder();
			closeProducerBuilder.SetProducerId(producerId);
			closeProducerBuilder.SetRequestId(requestId);
			CommandCloseProducer closeProducer = closeProducerBuilder.Build();
			IByteBuffer res = SerializeWithSize(BaseCommand.NewBuilder().SetType(BaseCommand.Types.Type.CloseProducer).SetCloseProducer(closeProducerBuilder));
			closeProducerBuilder.Recycle();
			closeProducer.Recycle();
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

		private static Proto.Schema.Types.Type GetSchemaType(SchemaType type)
		{
			if (type.Value < 0)
			{
				return Proto.Schema.Types.Type.None;
			}
			else
			{
				return Enum.GetValues(typeof(Proto.Schema.Types.Type)).Cast<Proto.Schema.Types.Type>().ToList()[type.Value];
			}
		}

		public static SchemaType GetSchemaType(Proto.Schema.Types.Type type)
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
		private static Proto.Schema GetSchema(SchemaInfo schemaInfo)
		{
			Proto.Schema.Builder builder = Proto.Schema.NewBuilder().SetName(schemaInfo.Name).SetSchemaData(ByteString.CopyFrom((byte[])(Array)schemaInfo.Schema)).SetType(GetSchemaType(schemaInfo.Type)).AddAllProperties(schemaInfo.Properties.ToList().Select(entry => KeyValue.NewBuilder().SetKey(entry.Key).SetValue(entry.Value).Build()).ToList());
			Proto.Schema schema = builder.Build();
			builder.Recycle();
			return schema;
		}

		public static IByteBuffer NewProducer(string topic, long producerId, long requestId, string producerName, bool encrypted, IDictionary<string, string> metadata, SchemaInfo schemaInfo, long epoch, bool userProvidedProducerName)
		{
			CommandProducer.Builder producerBuilder = CommandProducer.NewBuilder();
			producerBuilder.SetTopic(topic);
			producerBuilder.SetProducerId(producerId);
			producerBuilder.SetRequestId(requestId);
			producerBuilder.SetEpoch(epoch);
			if (!string.ReferenceEquals(producerName, null))
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

			CommandProducer producer = producerBuilder.Build();
			IByteBuffer res = SerializeWithSize(BaseCommand.NewBuilder().SetType(BaseCommand.Types.Type.Producer).SetProducer(producer));
			producerBuilder.Recycle();
			producer.Recycle();
			return res;
		}

		public static IByteBuffer NewPartitionMetadataResponse(ServerError error, string errorMsg, long requestId)
		{
			CommandPartitionedTopicMetadataResponse.Builder partitionMetadataResponseBuilder = CommandPartitionedTopicMetadataResponse.NewBuilder();
			partitionMetadataResponseBuilder.SetRequestId(requestId);
			partitionMetadataResponseBuilder.SetError(error);
			partitionMetadataResponseBuilder.SetResponse(CommandPartitionedTopicMetadataResponse.Types.LookupType.Failed);
			if (!string.ReferenceEquals(errorMsg, null))
			{
				partitionMetadataResponseBuilder.SetMessage(errorMsg);
			}

			CommandPartitionedTopicMetadataResponse partitionMetadataResponse = partitionMetadataResponseBuilder.Build();
			IByteBuffer res = SerializeWithSize(BaseCommand.NewBuilder().SetType(BaseCommand.Types.Type.PartitionedMetadataResponse).SetPartitionMetadataResponse(partitionMetadataResponse));
			partitionMetadataResponseBuilder.Recycle();
			partitionMetadataResponse.Recycle();
			return res;
		}

		public static IByteBuffer NewPartitionMetadataRequest(string topic, long requestId)
		{
			CommandPartitionedTopicMetadata.Builder partitionMetadataBuilder = CommandPartitionedTopicMetadata.NewBuilder();
			partitionMetadataBuilder.SetTopic(topic);
			partitionMetadataBuilder.SetRequestId(requestId);
			CommandPartitionedTopicMetadata partitionMetadata = partitionMetadataBuilder.Build();
			IByteBuffer res = SerializeWithSize(BaseCommand.NewBuilder().SetType(BaseCommand.Types.Type.PartitionedMetadata).SetPartitionMetadata(partitionMetadata));
			partitionMetadataBuilder.Recycle();
			partitionMetadata.Recycle();
			return res;
		}

		public static IByteBuffer NewPartitionMetadataResponse(int partitions, long requestId)
		{
			CommandPartitionedTopicMetadataResponse.Builder partitionMetadataResponseBuilder = CommandPartitionedTopicMetadataResponse.NewBuilder();
			partitionMetadataResponseBuilder.SetPartitions(partitions);
			partitionMetadataResponseBuilder.SetResponse(CommandPartitionedTopicMetadataResponse.Types.LookupType.Success);
			partitionMetadataResponseBuilder.SetRequestId(requestId);

			CommandPartitionedTopicMetadataResponse partitionMetadataResponse = partitionMetadataResponseBuilder.Build();
			IByteBuffer res = SerializeWithSize(BaseCommand.NewBuilder().SetType(BaseCommand.Types.Type.PartitionedMetadataResponse).SetPartitionMetadataResponse(partitionMetadataResponse));
			partitionMetadataResponseBuilder.Recycle();
			partitionMetadataResponse.Recycle();
			return res;
		}

		public static IByteBuffer NewLookup(string topic, bool authoritative, long requestId)
		{
			CommandLookupTopic.Builder lookupTopicBuilder = CommandLookupTopic.NewBuilder();
			lookupTopicBuilder.SetTopic(topic);
			lookupTopicBuilder.SetRequestId(requestId);
			lookupTopicBuilder.SetAuthoritative(authoritative);
			CommandLookupTopic lookupBroker = lookupTopicBuilder.Build();
			IByteBuffer res = SerializeWithSize(BaseCommand.NewBuilder().SetType(BaseCommand.Types.Type.Lookup).SetLookupTopic(lookupBroker));
			lookupTopicBuilder.Recycle();
			lookupBroker.Recycle();
			return res;
		}

		public static IByteBuffer NewLookupResponse(string brokerServiceUrl, string brokerServiceUrlTls, bool authoritative, CommandLookupTopicResponse.Types.LookupType response, long requestId, bool proxyThroughServiceUrl)
		{
			CommandLookupTopicResponse.Builder commandLookupTopicResponseBuilder = CommandLookupTopicResponse.NewBuilder();
			commandLookupTopicResponseBuilder.SetBrokerServiceUrl(brokerServiceUrl);
			if (!string.ReferenceEquals(brokerServiceUrlTls, null))
			{
				commandLookupTopicResponseBuilder.SetBrokerServiceUrlTls(brokerServiceUrlTls);
			}
			commandLookupTopicResponseBuilder.SetResponse(response);
			commandLookupTopicResponseBuilder.SetRequestId(requestId);
			commandLookupTopicResponseBuilder.SetAuthoritative(authoritative);
			commandLookupTopicResponseBuilder.SetProxyThroughServiceUrl(proxyThroughServiceUrl);

			CommandLookupTopicResponse commandLookupTopicResponse = commandLookupTopicResponseBuilder.Build();
			IByteBuffer res = SerializeWithSize(BaseCommand.NewBuilder().SetType(BaseCommand.Types.Type.LookupResponse).SetLookupTopicResponse(commandLookupTopicResponse));
			commandLookupTopicResponseBuilder.Recycle();
			commandLookupTopicResponse.Recycle();
			return res;
		}

		public static IByteBuffer NewLookupErrorResponse(ServerError error, string errorMsg, long requestId)
		{
			CommandLookupTopicResponse.Builder connectionBuilder = CommandLookupTopicResponse.NewBuilder();
			connectionBuilder.SetRequestId(requestId);
			connectionBuilder.SetError(error);
			if (!string.ReferenceEquals(errorMsg, null))
			{
				connectionBuilder.SetMessage(errorMsg);
			}
			connectionBuilder.SetResponse(CommandLookupTopicResponse.Types.LookupType.Failed);

			CommandLookupTopicResponse connectionBroker = connectionBuilder.Build();
			IByteBuffer res = SerializeWithSize(BaseCommand.NewBuilder().SetType(BaseCommand.Types.Type.LookupResponse).SetLookupTopicResponse(connectionBroker));
			connectionBuilder.Recycle();
			connectionBroker.Recycle();
			return res;
		}

		public static IByteBuffer NewMultiMessageAck(long consumerId, IList<KeyValuePair<long, long>> entries)
		{
			CommandAck.Builder ackBuilder = CommandAck.NewBuilder();
			ackBuilder.SetConsumerId(consumerId);
			ackBuilder.SetAckType(CommandAck.Types.AckType.Individual);

			int entriesCount = entries.Count;
			for (int i = 0; i < entriesCount; i++)
			{
				long ledgerId = entries[i].Key;
				long entryId = entries[i].Value;

				MessageIdData.Builder messageIdDataBuilder = MessageIdData.NewBuilder();
				messageIdDataBuilder.LedgerId = ledgerId;
				messageIdDataBuilder.EntryId = entryId;
				MessageIdData messageIdData = messageIdDataBuilder.Build();
				ackBuilder.AddMessageId(messageIdData);

				messageIdDataBuilder.Recycle();
			}

			CommandAck ack = ackBuilder.Build();

			IByteBuffer res = SerializeWithSize(BaseCommand.NewBuilder().SetType(BaseCommand.Types.Type.Ack).SetAck(ack));

			for (int i = 0; i < entriesCount; i++)
			{
				ack.GetMessageId(i).Recycle();
			}
			ack.Recycle();
			ackBuilder.Recycle();
			return res;
		}

		public static IByteBuffer NewAck(long consumerId, long ledgerId, long entryId, CommandAck.Types.AckType ackType, CommandAck.Types.ValidationError validationError, IDictionary<string, long> properties)
		{
			return NewAck(consumerId, ledgerId, entryId, ackType, validationError, properties, 0, 0);
		}

		public static IByteBuffer NewAck(long consumerId, long ledgerId, long entryId, CommandAck.Types.AckType ackType, CommandAck.Types.ValidationError validationError, IDictionary<string, long> properties, long txnIdLeastBits, long txnIdMostBits)
		{
			CommandAck.Builder ackBuilder = CommandAck.NewBuilder();
			ackBuilder.SetConsumerId(consumerId);
			ackBuilder.SetAckType(ackType);
			MessageIdData.Builder messageIdDataBuilder = MessageIdData.NewBuilder();
			messageIdDataBuilder.LedgerId = ledgerId;
			messageIdDataBuilder.EntryId = entryId;
			MessageIdData messageIdData = messageIdDataBuilder.Build();
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
			CommandAck ack = ackBuilder.Build();

			IByteBuffer res = SerializeWithSize(BaseCommand.NewBuilder().SetType(BaseCommand.Types.Type.Ack).SetAck(ack));
			ack.Recycle();
			ackBuilder.Recycle();
			messageIdDataBuilder.Recycle();
			messageIdData.Recycle();
			return res;
		}

		public static IByteBuffer NewAckResponse(long consumerId, long txnIdLeastBits, long txnIdMostBits)
		{
			CommandAckResponse.Builder commandAckResponseBuilder = CommandAckResponse.NewBuilder();
			commandAckResponseBuilder.SetConsumerId(consumerId);
			commandAckResponseBuilder.SetTxnidLeastBits(txnIdLeastBits);
			commandAckResponseBuilder.SetTxnidMostBits(txnIdMostBits);
			CommandAckResponse commandAckResponse = commandAckResponseBuilder.Build();

			IByteBuffer res = SerializeWithSize(BaseCommand.NewBuilder().SetType(BaseCommand.Types.Type.AckResponse).SetAckResponse(commandAckResponse));
			commandAckResponseBuilder.Recycle();
			commandAckResponse.Recycle();

			return res;
		}

		public static IByteBuffer NewAckErrorResponse(ServerError error, string errorMsg, long consumerId)
		{
			CommandAckResponse.Builder ackErrorBuilder = CommandAckResponse.NewBuilder();
			ackErrorBuilder.SetConsumerId(consumerId);
			ackErrorBuilder.SetError(error);
			if (!string.ReferenceEquals(errorMsg, null))
			{
				ackErrorBuilder.SetMessage(errorMsg);
			}

			CommandAckResponse response = ackErrorBuilder.Build();
			IByteBuffer res = SerializeWithSize(BaseCommand.NewBuilder().SetType(BaseCommand.Types.Type.AckResponse).SetAckResponse(response));

			ackErrorBuilder.Recycle();
			response.Recycle();

			return res;
		}

		public static IByteBuffer NewFlow(long consumerId, int messagePermits)
		{
			CommandFlow.Builder flowBuilder = CommandFlow.NewBuilder();
			flowBuilder.SetConsumerId(consumerId);
			flowBuilder.SetMessagePermits(messagePermits);
			CommandFlow flow = flowBuilder.Build();

			IByteBuffer res = SerializeWithSize(BaseCommand.NewBuilder().SetType(BaseCommand.Types.Type.Flow).SetFlow(flowBuilder));
			flow.Recycle();
			flowBuilder.Recycle();
			return res;
		}

		public static IByteBuffer NewRedeliverUnacknowledgedMessages(long consumerId)
		{
			CommandRedeliverUnacknowledgedMessages.Builder redeliverBuilder = CommandRedeliverUnacknowledgedMessages.NewBuilder();
			redeliverBuilder.SetConsumerId(consumerId);
			CommandRedeliverUnacknowledgedMessages redeliver = redeliverBuilder.Build();
			IByteBuffer res = SerializeWithSize(BaseCommand.NewBuilder().SetType(BaseCommand.Types.Type.RedeliverUnacknowledgedMessages).SetRedeliverUnacknowledgedMessages(redeliverBuilder));
			redeliver.Recycle();
			redeliverBuilder.Recycle();
			return res;
		}

		public static IByteBuffer NewRedeliverUnacknowledgedMessages(long consumerId, IList<MessageIdData> messageIds)
		{
			CommandRedeliverUnacknowledgedMessages.Builder redeliverBuilder = CommandRedeliverUnacknowledgedMessages.NewBuilder();
			redeliverBuilder.SetConsumerId(consumerId);
			redeliverBuilder.AddAllMessageIds(messageIds);
			CommandRedeliverUnacknowledgedMessages redeliver = redeliverBuilder.Build();
			IByteBuffer res = SerializeWithSize(BaseCommand.NewBuilder().SetType(BaseCommand.Types.Type.RedeliverUnacknowledgedMessages).SetRedeliverUnacknowledgedMessages(redeliverBuilder));
			redeliver.Recycle();
			redeliverBuilder.Recycle();
			return res;
		}

		public static IByteBuffer NewConsumerStatsResponse(ServerError serverError, string errMsg, long requestId)
		{
			CommandConsumerStatsResponse.Builder commandConsumerStatsResponseBuilder = CommandConsumerStatsResponse.NewBuilder();
			commandConsumerStatsResponseBuilder.SetRequestId(requestId);
			commandConsumerStatsResponseBuilder.SetErrorMessage(errMsg);
			commandConsumerStatsResponseBuilder.SetErrorCode(serverError);

			CommandConsumerStatsResponse commandConsumerStatsResponse = commandConsumerStatsResponseBuilder.Build();
			IByteBuffer res = SerializeWithSize(BaseCommand.NewBuilder().SetType(BaseCommand.Types.Type.ConsumerStatsResponse).SetConsumerStatsResponse(commandConsumerStatsResponseBuilder));
			commandConsumerStatsResponse.Recycle();
			commandConsumerStatsResponseBuilder.Recycle();
			return res;
		}

		public static IByteBuffer NewConsumerStatsResponse(CommandConsumerStatsResponse.Builder builder)
		{
			CommandConsumerStatsResponse commandConsumerStatsResponse = builder.Build();
			IByteBuffer res = SerializeWithSize(BaseCommand.NewBuilder().SetType(BaseCommand.Types.Type.ConsumerStatsResponse).SetConsumerStatsResponse(builder));
			commandConsumerStatsResponse.Recycle();
			builder.Recycle();
			return res;
		}

		public static IByteBuffer NewGetTopicsOfNamespaceRequest(string @namespace, long requestId, CommandGetTopicsOfNamespace.Types.Mode mode)
		{
			CommandGetTopicsOfNamespace.Builder topicsBuilder = CommandGetTopicsOfNamespace.NewBuilder();
			topicsBuilder.SetNamespace(@namespace).SetRequestId(requestId).SetMode(mode);

			CommandGetTopicsOfNamespace topicsCommand = topicsBuilder.Build();
			IByteBuffer res = SerializeWithSize(BaseCommand.NewBuilder().SetType(BaseCommand.Types.Type.GetTopicsOfNamespace).SetGetTopicsOfNamespace(topicsCommand));
			topicsBuilder.Recycle();
			topicsCommand.Recycle();
			return res;
		}

		public static IByteBuffer NewGetTopicsOfNamespaceResponse(IList<string> topics, long requestId)
		{
			CommandGetTopicsOfNamespaceResponse.Builder topicsResponseBuilder = CommandGetTopicsOfNamespaceResponse.NewBuilder();

			topicsResponseBuilder.SetRequestId(requestId).AddAllTopics(topics);

			CommandGetTopicsOfNamespaceResponse topicsOfNamespaceResponse = topicsResponseBuilder.Build();
			IByteBuffer res = SerializeWithSize(BaseCommand.NewBuilder().SetType(BaseCommand.Types.Type.GetTopicsOfNamespaceResponse).SetGetTopicsOfNamespaceResponse(topicsOfNamespaceResponse));

			topicsResponseBuilder.Recycle();
			topicsOfNamespaceResponse.Recycle();
			return res;
		}

		private static readonly IByteBuffer CmdPing;

		static Commands()
		{
			IByteBuffer serializedCmdPing = SerializeWithSize(BaseCommand.NewBuilder().SetType(BaseCommand.Types.Type.Ping).SetPing(CommandPing.DefaultInstance));
			CmdPing = Unpooled.CopiedBuffer(serializedCmdPing);
			serializedCmdPing.Release();
			IByteBuffer serializedCmdPong = SerializeWithSize(BaseCommand.NewBuilder().SetType(BaseCommand.Types.Type.Pong).SetPong(CommandPong.DefaultInstance));
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
			CommandGetLastMessageId.Builder cmdBuilder = CommandGetLastMessageId.NewBuilder();
			cmdBuilder.SetConsumerId(consumerId).SetRequestId(requestId);

			IByteBuffer res = SerializeWithSize(BaseCommand.NewBuilder().SetType(BaseCommand.Types.Type.GetLastMessageId).SetGetLastMessageId(cmdBuilder.Build()));
			cmdBuilder.Recycle();
			return res;
		}

		public static IByteBuffer NewGetLastMessageIdResponse(long requestId, MessageIdData messageIdData)
		{
			CommandGetLastMessageIdResponse.Builder response = CommandGetLastMessageIdResponse.NewBuilder().SetLastMessageId(messageIdData).SetRequestId(requestId);

			IByteBuffer res = SerializeWithSize(BaseCommand.NewBuilder().SetType(BaseCommand.Types.Type.GetLastMessageIdResponse).SetGetLastMessageIdResponse(response.Build()));
			response.Recycle();
			return res;
		}

		public static IByteBuffer NewGetSchema(long requestId, string topic, SchemaVersion version)
		{
			CommandGetSchema.Builder schema = CommandGetSchema.NewBuilder().SetRequestId(requestId);
			schema.SetTopic(topic);
			if (version != null)
			{
				schema.SetSchemaVersion(ByteString.CopyFrom((byte[])(object)version.Bytes()));
			}

			CommandGetSchema getSchema = schema.Build();

			IByteBuffer res = SerializeWithSize(BaseCommand.NewBuilder().SetType(BaseCommand.Types.Type.GetSchema).SetGetSchema(getSchema));
			schema.Recycle();
			return res;
		}

		public static IByteBuffer NewGetSchemaResponse(long requestId, CommandGetSchemaResponse response)
		{
			CommandGetSchemaResponse.Builder schemaResponseBuilder = CommandGetSchemaResponse.NewBuilder(response).SetRequestId(requestId);

			IByteBuffer res = SerializeWithSize(BaseCommand.NewBuilder().SetType(BaseCommand.Types.Type.GetSchemaResponse).SetGetSchemaResponse(schemaResponseBuilder.Build()));
			schemaResponseBuilder.Recycle();
			return res;
		}

		public static IByteBuffer NewGetSchemaResponse(long requestId, SchemaInfo schema, SchemaVersion version)
		{
			CommandGetSchemaResponse.Builder schemaResponse = CommandGetSchemaResponse.NewBuilder().SetRequestId(requestId).SetSchemaVersion(ByteString.CopyFrom((byte[])(object)version.Bytes())).SetSchema(GetSchema(schema));

			IByteBuffer res = SerializeWithSize(BaseCommand.NewBuilder().SetType(BaseCommand.Types.Type.GetSchemaResponse).SetGetSchemaResponse(schemaResponse.Build()));
			schemaResponse.Recycle();
			return res;
		}

		public static IByteBuffer NewGetSchemaResponseError(long requestId, ServerError error, string errorMessage)
		{
			CommandGetSchemaResponse.Builder schemaResponse = CommandGetSchemaResponse.NewBuilder().SetRequestId(requestId).SetErrorCode(error).SetErrorMessage(errorMessage);

			IByteBuffer res = SerializeWithSize(BaseCommand.NewBuilder().SetType(BaseCommand.Types.Type.GetSchemaResponse).SetGetSchemaResponse(schemaResponse.Build()));
			schemaResponse.Recycle();
			return res;
		}

		public static IByteBuffer NewGetOrCreateSchema(long requestId, string topic, SchemaInfo schemaInfo)
		{
			CommandGetOrCreateSchema getOrCreateSchema = CommandGetOrCreateSchema.NewBuilder().SetRequestId(requestId).SetTopic(topic).SetSchema(GetSchema(schemaInfo).ToBuilder()).Build();
			IByteBuffer res = SerializeWithSize(BaseCommand.NewBuilder().SetType(BaseCommand.Types.Type.GetOrCreateSchema).SetGetOrCreateSchema(getOrCreateSchema));
			getOrCreateSchema.Recycle();
			return res;
		}

		public static IByteBuffer NewGetOrCreateSchemaResponse(long requestId, SchemaVersion schemaVersion)
		{
			CommandGetOrCreateSchemaResponse.Builder schemaResponse = CommandGetOrCreateSchemaResponse.NewBuilder().SetRequestId(requestId).SetSchemaVersion(ByteString.CopyFrom((byte[])(object)schemaVersion.Bytes()));
			IByteBuffer res = SerializeWithSize(BaseCommand.NewBuilder().SetType(BaseCommand.Types.Type.GetOrCreateSchemaResponse).SetGetOrCreateSchemaResponse(schemaResponse.Build()));
			schemaResponse.Recycle();
			return res;
		}

		public static IByteBuffer NewGetOrCreateSchemaResponseError(long requestId, ServerError error, string errorMessage)
		{
			CommandGetOrCreateSchemaResponse.Builder schemaResponse = CommandGetOrCreateSchemaResponse.NewBuilder().SetRequestId(requestId).SetErrorCode(error).SetErrorMessage(errorMessage);
			IByteBuffer res = SerializeWithSize(BaseCommand.NewBuilder().SetType(BaseCommand.Types.Type.GetOrCreateSchemaResponse).SetGetOrCreateSchemaResponse(schemaResponse.Build()));
			schemaResponse.Recycle();
			return res;
		}

		// ---- transaction related ----

		public static IByteBuffer NewTxn(long tcId, long requestId, long ttlSeconds)
		{
			CommandNewTxn commandNewTxn = CommandNewTxn.NewBuilder().SetTcId(tcId).SetRequestId(requestId).SetTxnTtlSeconds(ttlSeconds).Build();
			IByteBuffer res = SerializeWithSize(BaseCommand.NewBuilder().SetType(BaseCommand.Types.Type.NewTxn).SetNewTxn(commandNewTxn));
			commandNewTxn.Recycle();
			return res;
		}

		public static IByteBuffer NewTxnResponse(long requestId, long txnIdLeastBits, long txnIdMostBits)
		{
			CommandNewTxnResponse commandNewTxnResponse = CommandNewTxnResponse.NewBuilder().SetRequestId(requestId).SetTxnidMostBits(txnIdMostBits).SetTxnidLeastBits(txnIdLeastBits).Build();
			IByteBuffer res = SerializeWithSize(BaseCommand.NewBuilder().SetType(BaseCommand.Types.Type.NewTxnResponse).SetNewTxnResponse(commandNewTxnResponse));
			commandNewTxnResponse.Recycle();

			return res;
		}

		public static IByteBuffer NewTxnResponse(long requestId, long txnIdMostBits, ServerError error, string errorMsg)
		{
			CommandNewTxnResponse.Builder builder = CommandNewTxnResponse.NewBuilder();
			builder.SetRequestId(requestId);
			builder.SetTxnidMostBits(txnIdMostBits);
			builder.SetError(error);
			if (!string.ReferenceEquals(errorMsg, null))
			{
				builder.SetMessage(errorMsg);
			}
			CommandNewTxnResponse errorResponse = builder.Build();
			IByteBuffer res = SerializeWithSize(BaseCommand.NewBuilder().SetType(BaseCommand.Types.Type.NewTxnResponse).SetNewTxnResponse(errorResponse));
			builder.Recycle();
			errorResponse.Recycle();

			return res;
		}

		public static IByteBuffer NewAddPartitionToTxn(long requestId, long txnIdLeastBits, long txnIdMostBits)
		{
			CommandAddPartitionToTxn commandAddPartitionToTxn = CommandAddPartitionToTxn.NewBuilder().SetRequestId(requestId).SetTxnidLeastBits(txnIdLeastBits).SetTxnidMostBits(txnIdMostBits).Build();
			IByteBuffer res = SerializeWithSize(BaseCommand.NewBuilder().SetType(BaseCommand.Types.Type.AddPartitionToTxn).SetAddPartitionToTxn(commandAddPartitionToTxn));
			commandAddPartitionToTxn.Recycle();
			return res;
		}

		public static IByteBuffer NewAddPartitionToTxnResponse(long requestId, long txnIdLeastBits, long txnIdMostBits)
		{
			CommandAddPartitionToTxnResponse commandAddPartitionToTxnResponse = CommandAddPartitionToTxnResponse.NewBuilder().SetRequestId(requestId).SetTxnidLeastBits(txnIdLeastBits).SetTxnidMostBits(txnIdMostBits).Build();
			IByteBuffer res = SerializeWithSize(BaseCommand.NewBuilder().SetType(BaseCommand.Types.Type.AddPartitionToTxnResponse).SetAddPartitionToTxnResponse(commandAddPartitionToTxnResponse));
			commandAddPartitionToTxnResponse.Recycle();
			return res;
		}

		public static IByteBuffer NewAddPartitionToTxnResponse(long requestId, long txnIdMostBits, ServerError error, string errorMsg)
		{
			CommandAddPartitionToTxnResponse.Builder builder = CommandAddPartitionToTxnResponse.NewBuilder();
			builder.SetRequestId(requestId);
			builder.SetTxnidMostBits(txnIdMostBits);
			builder.SetError(error);
			if (!string.ReferenceEquals(errorMsg, null))
			{
				builder.SetMessage(errorMsg);
			}
			CommandAddPartitionToTxnResponse commandAddPartitionToTxnResponse = builder.Build();
			IByteBuffer res = SerializeWithSize(BaseCommand.NewBuilder().SetType(BaseCommand.Types.Type.AddPartitionToTxnResponse).SetAddPartitionToTxnResponse(commandAddPartitionToTxnResponse));
			builder.Recycle();
			commandAddPartitionToTxnResponse.Recycle();
			return res;
		}

		public static IByteBuffer NewAddSubscriptionToTxn(long requestId, long txnIdLeastBits, long txnIdMostBits, IList<Subscription> subscription)
		{
			CommandAddSubscriptionToTxn commandAddSubscriptionToTxn = CommandAddSubscriptionToTxn.NewBuilder().SetRequestId(requestId).SetTxnidLeastBits(txnIdLeastBits).SetTxnidMostBits(txnIdMostBits).AddAllSubscription(subscription).Build();
			IByteBuffer res = SerializeWithSize(BaseCommand.NewBuilder().SetType(BaseCommand.Types.Type.AddSubscriptionToTxn).SetAddSubscriptionToTxn(commandAddSubscriptionToTxn));
			commandAddSubscriptionToTxn.Recycle();
			return res;
		}

		public static IByteBuffer NewAddSubscriptionToTxnResponse(long requestId, long txnIdLeastBits, long txnIdMostBits)
		{
			CommandAddSubscriptionToTxnResponse command = CommandAddSubscriptionToTxnResponse.NewBuilder().SetRequestId(requestId).SetTxnidLeastBits(txnIdLeastBits).SetTxnidMostBits(txnIdMostBits).Build();
			IByteBuffer res = SerializeWithSize(BaseCommand.NewBuilder().SetType(BaseCommand.Types.Type.AddSubscriptionToTxnResponse).SetAddSubscriptionToTxnResponse(command));
			command.Recycle();
			return res;
		}

		public static IByteBuffer NewAddSubscriptionToTxnResponse(long requestId, long txnIdMostBits, ServerError error, string errorMsg)
		{
			CommandAddSubscriptionToTxnResponse.Builder builder = CommandAddSubscriptionToTxnResponse.NewBuilder();
			builder.SetRequestId(requestId);
			builder.SetTxnidMostBits(txnIdMostBits);
			builder.SetError(error);
			if (!string.ReferenceEquals(errorMsg, null))
			{
				builder.SetMessage(errorMsg);
			}
			CommandAddSubscriptionToTxnResponse errorResponse = builder.Build();
			IByteBuffer res = SerializeWithSize(BaseCommand.NewBuilder().SetType(BaseCommand.Types.Type.AddSubscriptionToTxnResponse).SetAddSubscriptionToTxnResponse(errorResponse));
			builder.Recycle();
			errorResponse.Recycle();
			return res;
		}

		public static IByteBuffer NewEndTxn(long requestId, long txnIdLeastBits, long txnIdMostBits, TxnAction txnAction)
		{
			CommandEndTxn commandEndTxn = CommandEndTxn.NewBuilder().SetRequestId(requestId).SetTxnidLeastBits(txnIdLeastBits).SetTxnidMostBits(txnIdMostBits).SetTxnAction(txnAction).Build();
			IByteBuffer res = SerializeWithSize(BaseCommand.NewBuilder().SetType(BaseCommand.Types.Type.EndTxn).SetEndTxn(commandEndTxn));
			commandEndTxn.Recycle();
			return res;
		}

		public static IByteBuffer NewEndTxnResponse(long requestId, long txnIdLeastBits, long txnIdMostBits)
		{
			CommandEndTxnResponse commandEndTxnResponse = CommandEndTxnResponse.NewBuilder().SetRequestId(requestId).SetTxnidLeastBits(txnIdLeastBits).SetTxnidMostBits(txnIdMostBits).Build();
			IByteBuffer res = SerializeWithSize(BaseCommand.NewBuilder().SetType(BaseCommand.Types.Type.EndTxnResponse).SetEndTxnResponse(commandEndTxnResponse));
			commandEndTxnResponse.Recycle();
			return res;
		}

		public static IByteBuffer NewEndTxnResponse(long requestId, long txnIdMostBits, ServerError error, string errorMsg)
		{
			CommandEndTxnResponse.Builder builder = CommandEndTxnResponse.NewBuilder();
			builder.SetRequestId(requestId);
			builder.SetTxnidMostBits(txnIdMostBits);
			builder.SetError(error);
			if (!string.ReferenceEquals(errorMsg, null))
			{
				builder.SetMessage(errorMsg);
			}
			CommandEndTxnResponse commandEndTxnResponse = builder.Build();
			IByteBuffer res = SerializeWithSize(BaseCommand.NewBuilder().SetType(BaseCommand.Types.Type.EndTxnResponse).SetEndTxnResponse(commandEndTxnResponse));
			builder.Recycle();
			commandEndTxnResponse.Recycle();
			return res;
		}

		public static IByteBuffer NewEndTxnOnPartition(long requestId, long txnIdLeastBits, long txnIdMostBits, string topic, TxnAction txnAction)
		{
			CommandEndTxnOnPartition.Builder txnEndOnPartition = CommandEndTxnOnPartition.NewBuilder().SetRequestId(requestId).SetTxnidLeastBits(txnIdLeastBits).SetTxnidMostBits(txnIdMostBits).SetTopic(topic).SetTxnAction(txnAction);
			IByteBuffer res = SerializeWithSize(BaseCommand.NewBuilder().SetType(BaseCommand.Types.Type.EndTxnOnPartition).SetEndTxnOnPartition(txnEndOnPartition));
			txnEndOnPartition.Recycle();
			return res;
		}

		public static IByteBuffer NewEndTxnOnPartitionResponse(long requestId, long txnIdLeastBits, long txnIdMostBits)
		{
			CommandEndTxnOnPartitionResponse commandEndTxnOnPartitionResponse = CommandEndTxnOnPartitionResponse.NewBuilder().SetRequestId(requestId).SetTxnidLeastBits(txnIdLeastBits).SetTxnidMostBits(txnIdMostBits).Build();
			IByteBuffer res = SerializeWithSize(BaseCommand.NewBuilder().SetType(BaseCommand.Types.Type.EndTxnOnPartitionResponse).SetEndTxnOnPartitionResponse(commandEndTxnOnPartitionResponse));
			commandEndTxnOnPartitionResponse.Recycle();
			return res;
		}

		public static IByteBuffer NewEndTxnOnPartitionResponse(long requestId, ServerError error, string errorMsg)
		{
			CommandEndTxnOnPartitionResponse.Builder builder = CommandEndTxnOnPartitionResponse.NewBuilder();
			builder.SetRequestId(requestId);
			builder.SetError(error);
			if (!string.ReferenceEquals(errorMsg, null))
			{
				builder.SetMessage(errorMsg);
			}
			CommandEndTxnOnPartitionResponse commandEndTxnOnPartitionResponse = builder.Build();
			IByteBuffer res = SerializeWithSize(BaseCommand.NewBuilder().SetType(BaseCommand.Types.Type.EndTxnOnPartitionResponse).SetEndTxnOnPartitionResponse(commandEndTxnOnPartitionResponse));
			builder.Recycle();
			commandEndTxnOnPartitionResponse.Recycle();
			return res;
		}

		public static IByteBuffer NewEndTxnOnSubscription(long requestId, long txnIdLeastBits, long txnIdMostBits, Subscription subscription, TxnAction txnAction)
		{
			CommandEndTxnOnSubscription commandEndTxnOnSubscription = CommandEndTxnOnSubscription.NewBuilder().SetRequestId(requestId).SetTxnidLeastBits(txnIdLeastBits).SetTxnidMostBits(txnIdMostBits).SetSubscription(subscription).SetTxnAction(txnAction).Build();
			IByteBuffer res = SerializeWithSize(BaseCommand.NewBuilder().SetType(BaseCommand.Types.Type.EndTxnOnSubscription).SetEndTxnOnSubscription(commandEndTxnOnSubscription));
			commandEndTxnOnSubscription.Recycle();
			return res;
		}

		public static IByteBuffer NewEndTxnOnSubscriptionResponse(long requestId, long txnIdLeastBits, long txnIdMostBits)
		{
			CommandEndTxnOnSubscriptionResponse response = CommandEndTxnOnSubscriptionResponse.NewBuilder().SetRequestId(requestId).SetTxnidLeastBits(txnIdLeastBits).SetTxnidMostBits(txnIdMostBits).Build();
			IByteBuffer res = SerializeWithSize(BaseCommand.NewBuilder().SetType(BaseCommand.Types.Type.EndTxnOnSubscriptionResponse).SetEndTxnOnSubscriptionResponse(response));
			response.Recycle();
			return res;
		}

		public static IByteBuffer NewEndTxnOnSubscriptionResponse(long requestId, ServerError error, string errorMsg)
		{
			CommandEndTxnOnSubscriptionResponse.Builder builder = CommandEndTxnOnSubscriptionResponse.NewBuilder();
			builder.SetRequestId(requestId);
			builder.SetError(error);
			if (!string.ReferenceEquals(errorMsg, null))
			{
				builder.SetMessage(errorMsg);
			}
			CommandEndTxnOnSubscriptionResponse response = builder.Build();
			IByteBuffer res = SerializeWithSize(BaseCommand.NewBuilder().SetType(BaseCommand.Types.Type.EndTxnOnSubscriptionResponse).SetEndTxnOnSubscriptionResponse(response));
			builder.Recycle();
			response.Recycle();
			return res;
		}

		public static IByteBuffer SerializeWithSize(BaseCommand.Builder cmdBuilder)
		{
			// / Wire format
			// [TOTAL_SIZE] [CMD_SIZE][CMD]
			var cmd = cmdBuilder.Build();

			int cmdSize = cmd.CalculateSize();
			int totalSize = cmdSize + 4;
			int frameSize = totalSize + 4;

			IByteBuffer buf = PooledByteBufferAllocator.Default.Buffer(frameSize, frameSize);

			// Prepend 2 lengths to the buffer
			buf.WriteInt(totalSize);
			buf.WriteInt(cmdSize);

			ByteBufCodedOutputStream outStream = ByteBufCodedOutputStream.Get(buf);

			try
			{
				cmd.WriteTo(outStream);
			}
			catch (IOException e)
			{
				// This is in-memory serialization, should not fail
				throw new System.Exception(e.Message, e);
			}
			finally
			{
				cmd.Recycle();
				cmdBuilder.Recycle();
				outStream.Recycle();
			}

			return buf;
		}

		private static ByteBufPair SerializeCommandSendWithSize(BaseCommand.Builder cmdBuilder, ChecksumType checksumType, MessageMetadata msgMetadata, IByteBuffer payload)
		{
			// / Wire format
			// [TOTAL_SIZE] [CMD_SIZE][CMD] [MAGIC_NUMBER][CHECKSUM] [METADATA_SIZE][METADATA] [PAYLOAD]

			BaseCommand cmd = cmdBuilder.Build();
			int cmdSize = cmd.SerializedSize;
			int msgMetadataSize = msgMetadata.CalculateSize();
			int payloadSize = payload.ReadableBytes;
			int magicAndChecksumLength = ChecksumType.Crc32C.Equals(checksumType) ? (2 + 4) : 0;
			bool includeChecksum = magicAndChecksumLength > 0;
			// cmdLength + cmdSize + magicLength +
			// checksumSize + msgMetadataLength +
			// msgMetadataSize
			int headerContentSize = 4 + cmdSize + magicAndChecksumLength + 4 + msgMetadataSize;
			int totalSize = headerContentSize + payloadSize;
			int headersSize = 4 + headerContentSize; // totalSize + headerLength
			int checksumReaderIndex = -1;

			IByteBuffer headers = PooledByteBufferAllocator.Default.Buffer(headersSize, headersSize);
			headers.WriteInt(totalSize); // External frame

			try
			{
				// Write cmd
				headers.WriteInt(cmdSize);

				ByteBufCodedOutputStream outStream = ByteBufCodedOutputStream.Get(headers);
				cmd.WriteTo(outStream);
				cmd.Recycle();
				cmdBuilder.Recycle();

				//Create checksum placeholder
				if (includeChecksum)
				{
					headers.WriteShort(MagicCrc32C);
					checksumReaderIndex = headers.WriterIndex;
					headers.SetWriterIndex(headers.WriterIndex + ChecksumSize); //skip 4 bytes of checksum
				}

				// Write metadata
				headers.WriteInt(msgMetadataSize);
				msgMetadata.WriteTo(outStream);
				outStream.Recycle();
			}
			catch (IOException e)
			{
				// This is in-memory serialization, should not fail
				throw new System.Exception(e.Message);
			}

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

		public static IByteBuffer SerializeMetadataAndPayload(ChecksumType checksumType, MessageMetadata msgMetadata, IByteBuffer payload)
		{
			// / Wire format
			// [MAGIC_NUMBER][CHECKSUM] [METADATA_SIZE][METADATA] [PAYLOAD]
			int msgMetadataSize = msgMetadata.SerializedSize;
			int payloadSize = payload.ReadableBytes;
			int magicAndChecksumLength = ChecksumType.Crc32C.Equals(checksumType) ? (2 + 4) : 0;
			bool includeChecksum = magicAndChecksumLength > 0;
			int headerContentSize = magicAndChecksumLength + 4 + msgMetadataSize; // magicLength +
																				  // checksumSize + msgMetadataLength +
																				  // msgMetadataSize
			int checksumReaderIndex = -1;
			int totalSize = headerContentSize + payloadSize;

			IByteBuffer metadataAndPayload = PooledByteBufferAllocator.Default.Buffer(totalSize, totalSize);
			try
			{
				ByteBufCodedOutputStream outStream = ByteBufCodedOutputStream.Get(metadataAndPayload);

				//Create checksum placeholder
				if (includeChecksum)
				{
					metadataAndPayload.WriteShort(MagicCrc32C);
					checksumReaderIndex = metadataAndPayload.WriterIndex;
					metadataAndPayload.SetWriterIndex(metadataAndPayload.WriterIndex + ChecksumSize); //skip 4 bytes of checksum
				}

				// Write metadata
				metadataAndPayload.WriteInt(msgMetadataSize);
				msgMetadata.WriteTo(outStream);
				outStream.Recycle();
			}
			catch (IOException e)
			{
				// This is in-memory serialization, should not fail
				throw new System.Exception(e.Message, e);
			}

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

		public static long InitBatchMessageMetadata(MessageMetadata.Builder builder)
		{
			MessageMetadata.Builder messageMetadata = MessageMetadata.NewBuilder();
			messageMetadata.SetPublishTime(builder._publishTime);
			messageMetadata.SetProducerName(builder.GetProducerName());
			messageMetadata.SetSequenceId(builder._sequenceId);
			if (builder.HasReplicatedFrom())
			{
				messageMetadata.SetReplicatedFrom(builder.GetReplicatedFrom());
			}
			if (builder.ReplicateToCount > 0)
			{
				messageMetadata.AddAllReplicateTo(builder.ReplicateToList);
			}
			if (builder.HasSchemaVersion())
			{
				messageMetadata.SetSchemaVersion(builder._schemaVersion);
			}
			return builder._sequenceId;
		}

		public static IByteBuffer SerializeSingleMessageInBatchWithPayload(SingleMessageMetadata.Builder singleMessageMetadataBuilder, IByteBuffer payload, IByteBuffer batchBuffer)
		{
			int payLoadSize = payload.ReadableBytes;
			SingleMessageMetadata singleMessageMetadata = singleMessageMetadataBuilder.SetPayloadSize(payLoadSize).Build();
			// serialize meta-data size, meta-data and payload for single message in batch
			int singleMsgMetadataSize = singleMessageMetadata.CalculateSize();
			try
			{
				batchBuffer.WriteInt(singleMsgMetadataSize);
				ByteBufCodedOutputStream outStream = ByteBufCodedOutputStream.Get(batchBuffer);
				singleMessageMetadata.WriteTo(outStream);
				singleMessageMetadata.Recycle();
				outStream.Recycle();
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
			SingleMessageMetadata.Builder singleMessageMetadataBuilder = SingleMessageMetadata.NewBuilder();
			if (msgBuilder.HasPartitionKey())
			{
				singleMessageMetadataBuilder = singleMessageMetadataBuilder.SetPartitionKey(msgBuilder.GetPartitionKey()).SetPartitionKeyB64Encoded(msgBuilder.PartitionKeyB64Encoded);
			}
			if (msgBuilder.HasOrderingKey())
			{
				singleMessageMetadataBuilder = singleMessageMetadataBuilder.SetOrderingKey(msgBuilder.GetOrderingKey().ToByteArray());
			}
			if (msgBuilder.PropertiesList.Count > 0)
			{
				singleMessageMetadataBuilder = singleMessageMetadataBuilder.AddAllProperties(msgBuilder.PropertiesList);
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
				singleMessageMetadataBuilder.Recycle();
			}
		}

		public static IByteBuffer DeSerializeSingleMessageInBatch(IByteBuffer uncompressedPayload, SingleMessageMetadata.Builder singleMessageMetadataBuilder, int index, int batchSize)
		{
			int singleMetaSize = (int) uncompressedPayload.ReadUnsignedInt();
			int writerIndex = uncompressedPayload.WriterIndex;
			int beginIndex = uncompressedPayload.ReaderIndex + singleMetaSize;
			uncompressedPayload.SetWriterIndex(beginIndex);
			ByteBufCodedInputStream stream = ByteBufCodedInputStream.Get(uncompressedPayload);
			singleMessageMetadataBuilder.MergeFrom(stream, null);
			stream.Recycle();

			int singleMessagePayloadSize = singleMessageMetadataBuilder.PayloadSize;

			int readerIndex = uncompressedPayload.ReaderIndex;
			IByteBuffer singleMessagePayload = uncompressedPayload.RetainedSlice(readerIndex, singleMessagePayloadSize);
			uncompressedPayload.SetWriterIndex(writerIndex);

			// reader now points to beginning of payload read; so move it past message payload just read
			if (index < batchSize)
			{
				uncompressedPayload.SetReaderIndex(readerIndex + singleMessagePayloadSize);
			}

			return singleMessagePayload;
		}

		private static ByteBufPair SerializeCommandMessageWithSize(BaseCommand cmd, IByteBuffer metadataAndPayload)
		{
			// / Wire format
			// [TOTAL_SIZE] [CMD_SIZE][CMD] [MAGIC_NUMBER][CHECKSUM] [METADATA_SIZE][METADATA] [PAYLOAD]
			//
			// metadataAndPayload contains from magic-number to the payload included


			int cmdSize = cmd.SerializedSize;
			int totalSize = 4 + cmdSize + metadataAndPayload.ReadableBytes;
			int headersSize = 4 + 4 + cmdSize;

			IByteBuffer headers = PooledByteBufferAllocator.Default.Buffer(headersSize);
			headers.WriteInt(totalSize); // External frame

			try
			{
				// Write cmd
				headers.WriteInt(cmdSize);

				ByteBufCodedOutputStream outStream = ByteBufCodedOutputStream.Get(headers);
				cmd.WriteTo(outStream);
				outStream.Recycle();
			}
			catch (IOException e)
			{
				// This is in-memory serialization, should not fail
				throw new System.Exception(e.Message);
			}

			return (ByteBufPair) ByteBufPair.Get(headers, metadataAndPayload);
		}

		public static int GetNumberOfMessagesInBatch(IByteBuffer metadataAndPayload, string subscription, long consumerId)
		{
			MessageMetadata msgMetadata = PeekMessageMetadata(metadataAndPayload, subscription, consumerId);
			if (msgMetadata == null)
			{
				return -1;
			}
			else
			{
				int numMessagesInBatch = msgMetadata.NumMessagesInBatch;
				msgMetadata.Recycle();
				return numMessagesInBatch;
			}
		}

		public static MessageMetadata PeekMessageMetadata(IByteBuffer metadataAndPayload, string subscription, long consumerId)
		{
			try
			{
				// save the reader index and restore after parsing
				int readerIdx = metadataAndPayload.ReaderIndex;
				MessageMetadata metadata = ParseMessageMetadata(metadataAndPayload);
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