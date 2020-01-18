using DotNetty.Buffers;
using Google.Protobuf;
using SharpPulsar.Common.Allocator;
using SharpPulsar.Common.Protocol.Schema;
using SharpPulsar.Common.PulsarApi;
using SharpPulsar.Util.Protobuf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

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
namespace SharpPulsar.Common.Protocol
{
	/*
	
	using VisibleForTesting = com.google.common.annotations.VisibleForTesting;
	using ByteBuf = io.netty.buffer.ByteBuf;
	using Unpooled = io.netty.buffer.Unpooled;
	using UtilityClass = lombok.experimental.UtilityClass;
	using Slf4j = lombok.@extern.slf4j.Slf4j;
	using Pair = org.apache.commons.lang3.tuple.Pair;
	using KeySharedPolicy = org.apache.pulsar.client.api.KeySharedPolicy;
	using Range = org.apache.pulsar.client.api.Range;
	using PulsarByteBufAllocator = org.apache.pulsar.common.allocator.PulsarByteBufAllocator;
	using AuthData = org.apache.pulsar.common.api.AuthData;
	using PulsarApi = org.apache.pulsar.common.api.proto.PulsarApi;
	using AuthMethod = org.apache.pulsar.common.api.proto.PulsarApi.AuthMethod;
	using BaseCommand = org.apache.pulsar.common.api.proto.PulsarApi.BaseCommand;
	using Type = org.apache.pulsar.common.api.proto.PulsarApi.BaseCommand.Type;
	using CommandAck = org.apache.pulsar.common.api.proto.PulsarApi.CommandAck;
	using AckType = org.apache.pulsar.common.api.proto.PulsarApi.CommandAck.AckType;
	using ValidationError = org.apache.pulsar.common.api.proto.PulsarApi.CommandAck.ValidationError;
	using CommandAckResponse = org.apache.pulsar.common.api.proto.PulsarApi.CommandAckResponse;
	using CommandActiveConsumerChange = org.apache.pulsar.common.api.proto.PulsarApi.CommandActiveConsumerChange;
	using CommandAddPartitionToTxn = org.apache.pulsar.common.api.proto.PulsarApi.CommandAddPartitionToTxn;
	using CommandAddPartitionToTxnResponse = org.apache.pulsar.common.api.proto.PulsarApi.CommandAddPartitionToTxnResponse;
	using CommandAddSubscriptionToTxn = org.apache.pulsar.common.api.proto.PulsarApi.CommandAddSubscriptionToTxn;
	using CommandAddSubscriptionToTxnResponse = org.apache.pulsar.common.api.proto.PulsarApi.CommandAddSubscriptionToTxnResponse;
	using CommandAuthChallenge = org.apache.pulsar.common.api.proto.PulsarApi.CommandAuthChallenge;
	using CommandAuthResponse = org.apache.pulsar.common.api.proto.PulsarApi.CommandAuthResponse;
	using CommandCloseConsumer = org.apache.pulsar.common.api.proto.PulsarApi.CommandCloseConsumer;
	using CommandCloseProducer = org.apache.pulsar.common.api.proto.PulsarApi.CommandCloseProducer;
	using CommandConnect = org.apache.pulsar.common.api.proto.PulsarApi.CommandConnect;
	using CommandConnected = org.apache.pulsar.common.api.proto.PulsarApi.CommandConnected;
	using CommandConsumerStatsResponse = org.apache.pulsar.common.api.proto.PulsarApi.CommandConsumerStatsResponse;
	using CommandEndTxn = org.apache.pulsar.common.api.proto.PulsarApi.CommandEndTxn;
	using CommandEndTxnOnPartition = org.apache.pulsar.common.api.proto.PulsarApi.CommandEndTxnOnPartition;
	using CommandEndTxnOnPartitionResponse = org.apache.pulsar.common.api.proto.PulsarApi.CommandEndTxnOnPartitionResponse;
	using CommandEndTxnOnSubscription = org.apache.pulsar.common.api.proto.PulsarApi.CommandEndTxnOnSubscription;
	using CommandEndTxnOnSubscriptionResponse = org.apache.pulsar.common.api.proto.PulsarApi.CommandEndTxnOnSubscriptionResponse;
	using CommandEndTxnResponse = org.apache.pulsar.common.api.proto.PulsarApi.CommandEndTxnResponse;
	using CommandError = org.apache.pulsar.common.api.proto.PulsarApi.CommandError;
	using CommandFlow = org.apache.pulsar.common.api.proto.PulsarApi.CommandFlow;
	using CommandGetLastMessageId = org.apache.pulsar.common.api.proto.PulsarApi.CommandGetLastMessageId;
	using CommandGetOrCreateSchema = org.apache.pulsar.common.api.proto.PulsarApi.CommandGetOrCreateSchema;
	using CommandGetOrCreateSchemaResponse = org.apache.pulsar.common.api.proto.PulsarApi.CommandGetOrCreateSchemaResponse;
	using CommandGetSchema = org.apache.pulsar.common.api.proto.PulsarApi.CommandGetSchema;
	using CommandGetSchemaResponse = org.apache.pulsar.common.api.proto.PulsarApi.CommandGetSchemaResponse;
	using CommandGetTopicsOfNamespace = org.apache.pulsar.common.api.proto.PulsarApi.CommandGetTopicsOfNamespace;
	using Mode = org.apache.pulsar.common.api.proto.PulsarApi.CommandGetTopicsOfNamespace.Mode;
	using CommandGetTopicsOfNamespaceResponse = org.apache.pulsar.common.api.proto.PulsarApi.CommandGetTopicsOfNamespaceResponse;
	using CommandLookupTopic = org.apache.pulsar.common.api.proto.PulsarApi.CommandLookupTopic;
	using CommandLookupTopicResponse = org.apache.pulsar.common.api.proto.PulsarApi.CommandLookupTopicResponse;
	using LookupType = org.apache.pulsar.common.api.proto.PulsarApi.CommandLookupTopicResponse.LookupType;
	using CommandMessage = org.apache.pulsar.common.api.proto.PulsarApi.CommandMessage;
	using CommandNewTxn = org.apache.pulsar.common.api.proto.PulsarApi.CommandNewTxn;
	using CommandNewTxnResponse = org.apache.pulsar.common.api.proto.PulsarApi.CommandNewTxnResponse;
	using CommandPartitionedTopicMetadata = org.apache.pulsar.common.api.proto.PulsarApi.CommandPartitionedTopicMetadata;
	using CommandPartitionedTopicMetadataResponse = org.apache.pulsar.common.api.proto.PulsarApi.CommandPartitionedTopicMetadataResponse;
	using CommandPing = org.apache.pulsar.common.api.proto.PulsarApi.CommandPing;
	using CommandPong = org.apache.pulsar.common.api.proto.PulsarApi.CommandPong;
	using CommandProducer = org.apache.pulsar.common.api.proto.PulsarApi.CommandProducer;
	using CommandProducerSuccess = org.apache.pulsar.common.api.proto.PulsarApi.CommandProducerSuccess;
	using CommandReachedEndOfTopic = org.apache.pulsar.common.api.proto.PulsarApi.CommandReachedEndOfTopic;
	using CommandRedeliverUnacknowledgedMessages = org.apache.pulsar.common.api.proto.PulsarApi.CommandRedeliverUnacknowledgedMessages;
	using CommandSeek = org.apache.pulsar.common.api.proto.PulsarApi.CommandSeek;
	using CommandSend = org.apache.pulsar.common.api.proto.PulsarApi.CommandSend;
	using CommandSendError = org.apache.pulsar.common.api.proto.PulsarApi.CommandSendError;
	using CommandSendReceipt = org.apache.pulsar.common.api.proto.PulsarApi.CommandSendReceipt;
	using CommandSubscribe = org.apache.pulsar.common.api.proto.PulsarApi.CommandSubscribe;
	using InitialPosition = org.apache.pulsar.common.api.proto.PulsarApi.CommandSubscribe.InitialPosition;
	using SubType = org.apache.pulsar.common.api.proto.PulsarApi.CommandSubscribe.SubType;
	using CommandSuccess = org.apache.pulsar.common.api.proto.PulsarApi.CommandSuccess;
	using CommandUnsubscribe = org.apache.pulsar.common.api.proto.PulsarApi.CommandUnsubscribe;
	using KeyLongValue = org.apache.pulsar.common.api.proto.PulsarApi.KeyLongValue;
	using KeyValue = org.apache.pulsar.common.api.proto.PulsarApi.KeyValue;
	using MessageIdData = org.apache.pulsar.common.api.proto.PulsarApi.MessageIdData;
	using MessageMetadata = org.apache.pulsar.common.api.proto.PulsarApi.MessageMetadata;
	using ProtocolVersion = org.apache.pulsar.common.api.proto.PulsarApi.ProtocolVersion;
	using Schema = org.apache.pulsar.common.api.proto.PulsarApi.Schema;
	using ServerError = org.apache.pulsar.common.api.proto.PulsarApi.ServerError;
	using SingleMessageMetadata = org.apache.pulsar.common.api.proto.PulsarApi.SingleMessageMetadata;
	using Subscription = org.apache.pulsar.common.api.proto.PulsarApi.Subscription;
	using TxnAction = org.apache.pulsar.common.api.proto.PulsarApi.TxnAction;
	using SchemaVersion = org.apache.pulsar.common.protocol.schema.SchemaVersion;
	using SchemaInfo = org.apache.pulsar.common.schema.SchemaInfo;
	using SchemaType = org.apache.pulsar.common.schema.SchemaType;
	using ByteBufCodedInputStream = org.apache.pulsar.common.util.protobuf.ByteBufCodedInputStream;
	using ByteBufCodedOutputStream = org.apache.pulsar.common.util.protobuf.ByteBufCodedOutputStream;
	using ByteString = org.apache.pulsar.shaded.com.google.protobuf.v241.ByteString; 
	*/

	public class Commands
	{

		// default message size for transfer
		public const int DEFAULT_MAX_MESSAGE_SIZE = 5 * 1024 * 1024;
		public const int MESSAGE_SIZE_FRAME_PADDING = 10 * 1024;
		public const int INVALID_MAX_MESSAGE_SIZE = -1;
		public const short magicCrc32c = 0x0e01;
		private const int checksumSize = 4;

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
			CommandConnect connect = new CommandConnect
			{
				ClientVersion = (!string.ReferenceEquals(libVersion, null) ? libVersion : "Pulsar Client"),
				AuthMethodName = (authMethodName)
			};

			if ("ycav1".Equals(authMethodName))
			{
				// Handle the case of a client that gets updated before the broker and starts sending the string auth method
				// name. An example would be in broker-to-broker replication. We need to make sure the clients are still
				// passing both the enum and the string until all brokers are upgraded.
				connect.AuthMethod = AuthMethod.AuthMethodYcaV1;
			}

			if (!string.ReferenceEquals(targetBroker, null))
			{
				// When connecting through a proxy, we need to specify which broker do we want to be proxied through
				connect.ProxyToBrokerUrl = (targetBroker);
			}

			if (!string.ReferenceEquals(authData, null))
			{
				connect.AuthData = ByteString.CopyFromUtf8(authData).ToByteArray();
			}

			if (!string.ReferenceEquals(originalPrincipal, null))
			{
				connect.OriginalPrincipal = (originalPrincipal);
			}

			if (!string.ReferenceEquals(originalAuthData, null))
			{
				connect.OriginalAuthData = (originalAuthData);
			}

			if (!string.ReferenceEquals(originalAuthMethod, null))
			{
				connect.OriginalAuthMethod = (originalAuthMethod);
			}
			connect.ProtocolVersion = protocolVersion;
			PulsarApi.CommandConnect connectcmd = connect;
			IByteBuffer res = serializeWithSize(BaseCommand.newBuilder().setType(PulsarApi.BaseCommand.Type.Connect).setConnect(connect));
			connect.recycle();
			connectBuilder.recycle();
			return res;
		}

		public static IByteBuffer NewConnect(string authMethodName, AuthData authData, int protocolVersion, string libVersion, string targetBroker, string originalPrincipal, AuthData originalAuthData, string originalAuthMethod)
		{
			CommandConnect connectBuilder = new CommandConnect
			{
				ClientVersion = (!string.ReferenceEquals(libVersion, null) ? libVersion : "Pulsar Client"),
				AuthMethodName = (authMethodName)
			};

			if (!string.ReferenceEquals(targetBroker, null))
			{
				// When connecting through a proxy, we need to specify which broker do we want to be proxied through
				connectBuilder.ProxyToBrokerUrl = (targetBroker);
			}

			if (authData != null)
			{
				connectBuilder.AuthData = ByteString.CopyFrom(authData.auth_data).ToByteArray();
			}

			if (!string.ReferenceEquals(originalPrincipal, null))
			{
				connectBuilder.OriginalPrincipal = (originalPrincipal);
			}

			if (originalAuthData != null)
			{
				connectBuilder.OriginalAuthData = (new string(originalAuthData.auth_data, Encoding.UTF8));
			}

			if (!string.ReferenceEquals(originalAuthMethod, null))
			{
				connectBuilder.OriginalAuthMethod = (originalAuthMethod);
			}
			connectBuilder.ProtocolVersion = protocolVersion;
			CommandConnect connect = connectBuilder;
			var baseCommand = new BaseCommand
			{
				type = BaseCommand.Type.Connect,
				Connect = connect
			};
			IByteBuffer res = SerializeWithSize(baseCommand);
			connect.Recycle();
			connectBuilder.recycle();
			return res;
		}

		public static IByteBuffer NewConnected(int clientProtocoVersion)
		{
			return NewConnected(clientProtocoVersion, INVALID_MAX_MESSAGE_SIZE);
		}

		public static IByteBuffer NewConnected(int clientProtocolVersion, int maxMessageSize)
		{
			CommandConnected connectedBuilder = new CommandConnected();
			connectedBuilder.ServerVersion = ("Pulsar Server");
			if (INVALID_MAX_MESSAGE_SIZE != maxMessageSize)
			{
				connectedBuilder.MaxMessageSize = maxMessageSize;
			}

			// If the broker supports a newer version of the protocol, it will anyway advertise the max version that the
			// client supports, to avoid confusing the client.
			int currentProtocolVersion = CurrentProtocolVersion;
			int versionToAdvertise = Math.Min(currentProtocolVersion, clientProtocolVersion);

			connectedBuilder.ProtocolVersion = versionToAdvertise;

			CommandConnected connected = connectedBuilder;
			IByteBuffer res = SerializeWithSize(new BaseCommand
			{ type = BaseCommand.Type.Connected,
			  Connected = (connected) 
			});
			connected.recycle();
			connectedBuilder.recycle();
			return res;
		}

		public static IByteBuffer NewAuthChallenge(string authMethod, AuthData brokerData, int clientProtocolVersion)
		{
			CommandAuthChallenge challengeBuilder =  new CommandAuthChallenge();

			// If the broker supports a newer version of the protocol, it will anyway advertise the max version that the
			// client supports, to avoid confusing the client.
			int currentProtocolVersion = CurrentProtocolVersion;
			int versionToAdvertise = Math.Min(currentProtocolVersion, clientProtocolVersion);

			challengeBuilder.ProtocolVersion = versionToAdvertise;
			var authData = new AuthData {
				auth_data = ByteString.CopyFrom(brokerData.auth_data).ToByteArray(),
				AuthMethodName = authMethod
			};
			challengeBuilder.Challenge = authData;
			CommandAuthChallenge challenge = challengeBuilder;

			IByteBuffer res = SerializeWithSize(
				new BaseCommand
				{
					type = BaseCommand.Type.AuthChallenge,
					authChallenge = challenge
				});
			challenge.recycle();
			challengeBuilder.recycle();
			return res;
		}

		public static IByteBuffer NewAuthResponse(string authMethod, AuthData clientData, int clientProtocolVersion, string clientVersion)
		{
			CommandAuthResponse responseBuilder = new CommandAuthResponse
			{
				ClientVersion = (!string.ReferenceEquals(clientVersion, null) ? clientVersion : "Pulsar Client"),
				ProtocolVersion = clientProtocolVersion,
				Response =
				new AuthData
				{
					auth_data = ByteString.CopyFrom(clientData.auth_data).ToByteArray(),
					AuthMethodName = authMethod
				}
			};
			CommandAuthResponse response = responseBuilder;

			IByteBuffer res = SerializeWithSize(				
				new BaseCommand
				{
					type = BaseCommand.Type.AuthResponse,
					authResponse = response
				});
			response.recycle();
			responseBuilder.recycle();
			return res;
		}

		public static IByteBuffer NewSuccess(long requestId)
		{
			CommandSuccess successBuilder = new CommandSuccess();
			successBuilder.RequestId = (ulong)requestId;
			CommandSuccess success = successBuilder;
			IByteBuffer res = SerializeWithSize(
				new BaseCommand { type = BaseCommand.Type.Success, Success = success });
			successBuilder.recycle();
			success.recycle();
			return res;
		}

		public static IByteBuffer NewProducerSuccess(long requestId, string producerName, SchemaVersion schemaVersion)
		{
			return NewProducerSuccess(requestId, producerName, -1, schemaVersion);
		}

		public static IByteBuffer NewProducerSuccess(long requestId, string producerName, long lastSequenceId, SchemaVersion schemaVersion)
		{
			CommandProducerSuccess producerSuccessBuilder = new CommandProducerSuccess();
			producerSuccessBuilder.RequestId = (ulong)requestId;
			producerSuccessBuilder.ProducerName = (producerName);
			producerSuccessBuilder.LastSequenceId = lastSequenceId;
			producerSuccessBuilder.SchemaVersion = ByteString.CopyFrom((byte[])(Array)schemaVersion.Bytes()).ToByteArray();
			CommandProducerSuccess producerSuccess = producerSuccessBuilder;
			IByteBuffer res = SerializeWithSize(new BaseCommand { type = BaseCommand.Type.ProducerSuccess, ProducerSuccess = (producerSuccess) });
			producerSuccess.recycle();
			producerSuccessBuilder.recycle();
			return res;
		}

		public static IByteBuffer NewError(long requestId, ServerError error, string message)
		{
			CommandError cmdErrorBuilder = new CommandError
			{
				RequestId = (ulong)requestId,
				Error = error,
				Message = (message)
			};
			CommandError cmdError = cmdErrorBuilder;
			IByteBuffer res = SerializeWithSize(new BaseCommand { type = BaseCommand.Type.Error, Error = (cmdError) });
			cmdError.recycle();
			cmdErrorBuilder.recycle();
			return res;

		}

		public static IByteBuffer NewSendReceipt(long producerId, long sequenceId, long highestId, long ledgerId, long entryId)
		{
			CommandSendReceipt sendReceiptBuilder = new CommandSendReceipt
			{
				ProducerId = (ulong)producerId,
				SequenceId = (ulong)sequenceId,
				HighestSequenceId = (ulong)highestId
			};

			MessageIdData messageIdBuilder = new MessageIdData
			{
				ledgerId = (ulong)ledgerId,
				entryId = (ulong)entryId
			};
			MessageIdData messageId = messageIdBuilder;
			sendReceiptBuilder.MessageId = (messageId);

			CommandSendReceipt sendReceipt = sendReceiptBuilder;
			IByteBuffer res = SerializeWithSize(
				new BaseCommand { type = BaseCommand.Type.SendReceipt, SendReceipt = (sendReceipt) });
			messageIdBuilder.recycle();
			messageId.recycle();
			sendReceiptBuilder.recycle();
			sendReceipt.recycle();
			return res;
		}

		public static IByteBuffer NewSendError(long producerId, long sequenceId, ServerError error, string errorMsg)
		{
			CommandSendError sendErrorBuilder = new CommandSendError
			{
				ProducerId = (ulong)producerId,
				SequenceId = (ulong)sequenceId,
				Error = error,
				Message = (errorMsg)
			};
			CommandSendError sendError = sendErrorBuilder;
			IByteBuffer res = SerializeWithSize(
				new BaseCommand { type = BaseCommand.Type.SendError, SendError = (sendError) });
			sendErrorBuilder.recycle();
			sendError.recycle();
			return res;
		}


		public static bool HasChecksum(IByteBuffer buffer)
		{
			return buffer.GetShort(buffer.ReaderIndex) == magicCrc32c;
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
				buffer.writerIndex(buffer.readerIndex() + metadataSize);

				ByteBufCodedInputStream stream = ByteBufCodedInputStream.Get(buffer);
				MessageMetadata messageMetadataBuilder = new MessageMetadata();
				MessageMetadata res = messageMetadataBuilder.MergeFrom(stream, null);
				buffer.writerIndex(writerIndex);
				messageMetadataBuilder.recycle();
				stream.recycle();
				return res;
			}
			catch (IOException e)
			{
				throw new Exception(e);
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
			CommandMessage msgBuilder = new CommandMessage();
			msgBuilder.ConsumerId = (ulong)consumerId;
			msgBuilder.MessageId = (messageId);
			if (redeliveryCount > 0)
			{
				msgBuilder.RedeliveryCount = (uint)redeliveryCount;
			}
			CommandMessage msg = msgBuilder;
			BaseCommand cmdBuilder = new BaseCommand();
			cmdBuilder.type = BaseCommand.Type.Message;
			cmdBuilder.Message = msg;
			BaseCommand cmd = 

			ByteBufPair res = SerializeCommandMessageWithSize(cmd, metadataAndPayload);

			cmd.recycle();
			cmdBuilder.recycle();
			msg.recycle();
			msgBuilder.recycle();
			return res;
		}

		public static IByteBufferPair newSend(long producerId, long sequenceId, int numMessaegs, ChecksumType checksumType, PulsarApi.MessageMetadata messageMetadata, IByteBuffer payload)
		{
			return newSend(producerId, sequenceId, numMessaegs, 0, 0, checksumType, messageMetadata, payload);
		}

		public static IByteBufferPair newSend(long producerId, long lowestSequenceId, long highestSequenceId, int numMessaegs, ChecksumType checksumType, PulsarApi.MessageMetadata messageMetadata, IByteBuffer payload)
		{
			return newSend(producerId, lowestSequenceId, highestSequenceId, numMessaegs, 0, 0, checksumType, messageMetadata, payload);
		}

		public static IByteBufferPair newSend(long producerId, long sequenceId, int numMessages, long txnIdLeastBits, long txnIdMostBits, ChecksumType checksumType, PulsarApi.MessageMetadata messageData, IByteBuffer payload)
		{
			PulsarApi.CommandSend.Builder sendBuilder = PulsarApi.CommandSend.newBuilder();
			sendBuilder.ProducerId = producerId;
			sendBuilder.SequenceId = sequenceId;
			if (numMessages > 1)
			{
				sendBuilder.NumMessages = numMessages;
			}
			if (txnIdLeastBits > 0)
			{
				sendBuilder.TxnidLeastBits = txnIdLeastBits;
			}
			if (txnIdMostBits > 0)
			{
				sendBuilder.TxnidMostBits = txnIdMostBits;
			}
			PulsarApi.CommandSend send = sendBuilder.build();

			IByteBufferPair res = serializeCommandSendWithSize(PulsarApi.BaseCommand.newBuilder().setType(PulsarApi.BaseCommand.Type.SEND).setSend(send), checksumType, messageData, payload);
			send.recycle();
			sendBuilder.recycle();
			return res;
		}

		public static IByteBufferPair newSend(long producerId, long lowestSequenceId, long highestSequenceId, int numMessages, long txnIdLeastBits, long txnIdMostBits, ChecksumType checksumType, PulsarApi.MessageMetadata messageData, IByteBuffer payload)
		{
			PulsarApi.CommandSend.Builder sendBuilder = PulsarApi.CommandSend.newBuilder();
			sendBuilder.ProducerId = producerId;
			sendBuilder.SequenceId = lowestSequenceId;
			sendBuilder.HighestSequenceId = highestSequenceId;
			if (numMessages > 1)
			{
				sendBuilder.NumMessages = numMessages;
			}
			if (txnIdLeastBits > 0)
			{
				sendBuilder.TxnidLeastBits = txnIdLeastBits;
			}
			if (txnIdMostBits > 0)
			{
				sendBuilder.TxnidMostBits = txnIdMostBits;
			}
			PulsarApi.CommandSend send = sendBuilder.build();

			IByteBufferPair res = serializeCommandSendWithSize(PulsarApi.BaseCommand.newBuilder().setType(PulsarApi.BaseCommand.Type.SEND).setSend(send), checksumType, messageData, payload);
			send.recycle();
			sendBuilder.recycle();
			return res;
		}

		public static IByteBuffer newSubscribe(string topic, string subscription, long consumerId, long requestId, PulsarApi.CommandSubscribe.SubType subType, int priorityLevel, string consumerName, long resetStartMessageBackInSeconds)
		{
			return newSubscribe(topic, subscription, consumerId, requestId, subType, priorityLevel, consumerName, true, null, Collections.emptyMap(), false, false, PulsarApi.CommandSubscribe.InitialPosition.Earliest, resetStartMessageBackInSeconds, null, true);
		}

		public static IByteBuffer newSubscribe(string topic, string subscription, long consumerId, long requestId, PulsarApi.CommandSubscribe.SubType subType, int priorityLevel, string consumerName, bool isDurable, PulsarApi.MessageIdData startMessageId, IDictionary<string, string> metadata, bool readCompacted, bool isReplicated, PulsarApi.CommandSubscribe.InitialPosition subscriptionInitialPosition, long startMessageRollbackDurationInSec, SchemaInfo schemaInfo, bool createTopicIfDoesNotExist)
		{
					return newSubscribe(topic, subscription, consumerId, requestId, subType, priorityLevel, consumerName, isDurable, startMessageId, metadata, readCompacted, isReplicated, subscriptionInitialPosition, startMessageRollbackDurationInSec, schemaInfo, createTopicIfDoesNotExist, null);
		}

		public static IByteBuffer newSubscribe(string topic, string subscription, long consumerId, long requestId, PulsarApi.CommandSubscribe.SubType subType, int priorityLevel, string consumerName, bool isDurable, PulsarApi.MessageIdData startMessageId, IDictionary<string, string> metadata, bool readCompacted, bool isReplicated, PulsarApi.CommandSubscribe.InitialPosition subscriptionInitialPosition, long startMessageRollbackDurationInSec, SchemaInfo schemaInfo, bool createTopicIfDoesNotExist, KeySharedPolicy keySharedPolicy)
		{
			PulsarApi.CommandSubscribe.Builder subscribeBuilder = PulsarApi.CommandSubscribe.newBuilder();
			subscribeBuilder.setTopic(topic);
			subscribeBuilder.setSubscription(subscription);
			subscribeBuilder.SubType = subType;
			subscribeBuilder.ConsumerId = consumerId;
			subscribeBuilder.setConsumerName(consumerName);
			subscribeBuilder.RequestId = requestId;
			subscribeBuilder.PriorityLevel = priorityLevel;
			subscribeBuilder.Durable = isDurable;
			subscribeBuilder.ReadCompacted = readCompacted;
			subscribeBuilder.InitialPosition = subscriptionInitialPosition;
			subscribeBuilder.ReplicateSubscriptionState = isReplicated;
			subscribeBuilder.ForceTopicCreation = createTopicIfDoesNotExist;

			if (keySharedPolicy != null)
			{
				switch (keySharedPolicy.KeySharedMode)
				{
					case AUTO_SPLIT:
						subscribeBuilder.setKeySharedMeta(PulsarApi.KeySharedMeta.newBuilder().setKeySharedMode(PulsarApi.KeySharedMode.AUTO_SPLIT));
						break;
					case STICKY:
						PulsarApi.KeySharedMeta.Builder builder = PulsarApi.KeySharedMeta.newBuilder().setKeySharedMode(PulsarApi.KeySharedMode.STICKY);
						IList<Range> ranges = ((KeySharedPolicy.KeySharedPolicySticky) keySharedPolicy).Ranges;
						foreach (Range range in ranges)
						{
							builder.addHashRanges(PulsarApi.IntRange.newBuilder().setStart(range.Start).setEnd(range.End));
						}
						subscribeBuilder.setKeySharedMeta(builder);
						break;
				}
			}

			if (startMessageId != null)
			{
				subscribeBuilder.setStartMessageId(startMessageId);
			}
			if (startMessageRollbackDurationInSec > 0)
			{
				subscribeBuilder.StartMessageRollbackDurationSec = startMessageRollbackDurationInSec;
			}
			subscribeBuilder.addAllMetadata(CommandUtils.toKeyValueList(metadata));

			PulsarApi.Schema schema = null;
			if (schemaInfo != null)
			{
				schema = getSchema(schemaInfo);
				subscribeBuilder.setSchema(schema);
			}

			PulsarApi.CommandSubscribe subscribe = subscribeBuilder.build();
			IByteBuffer res = serializeWithSize(PulsarApi.BaseCommand.newBuilder().setType(PulsarApi.BaseCommand.Type.SUBSCRIBE).setSubscribe(subscribe));
			subscribeBuilder.recycle();
			subscribe.recycle();
			if (null != schema)
			{
				schema.recycle();
			}
			return res;
		}

		public static IByteBuffer newUnsubscribe(long consumerId, long requestId)
		{
			PulsarApi.CommandUnsubscribe.Builder unsubscribeBuilder = PulsarApi.CommandUnsubscribe.newBuilder();
			unsubscribeBuilder.ConsumerId = consumerId;
			unsubscribeBuilder.RequestId = requestId;
			PulsarApi.CommandUnsubscribe unsubscribe = unsubscribeBuilder.build();
			IByteBuffer res = serializeWithSize(PulsarApi.BaseCommand.newBuilder().setType(PulsarApi.BaseCommand.Type.UNSUBSCRIBE).setUnsubscribe(unsubscribe));
			unsubscribeBuilder.recycle();
			unsubscribe.recycle();
			return res;
		}

		public static IByteBuffer newActiveConsumerChange(long consumerId, bool isActive)
		{
			PulsarApi.CommandActiveConsumerChange.Builder changeBuilder = PulsarApi.CommandActiveConsumerChange.newBuilder().setConsumerId(consumerId).setIsActive(isActive);

			PulsarApi.CommandActiveConsumerChange change = changeBuilder.build();
			IByteBuffer res = serializeWithSize(PulsarApi.BaseCommand.newBuilder().setType(PulsarApi.BaseCommand.Type.ACTIVE_CONSUMER_CHANGE).setActiveConsumerChange(change));
			changeBuilder.recycle();
			change.recycle();
			return res;
		}

		public static IByteBuffer newSeek(long consumerId, long requestId, long ledgerId, long entryId)
		{
			PulsarApi.CommandSeek.Builder seekBuilder = PulsarApi.CommandSeek.newBuilder();
			seekBuilder.ConsumerId = consumerId;
			seekBuilder.RequestId = requestId;

			PulsarApi.MessageIdData.Builder messageIdBuilder = PulsarApi.MessageIdData.newBuilder();
			messageIdBuilder.LedgerId = ledgerId;
			messageIdBuilder.EntryId = entryId;
			PulsarApi.MessageIdData messageId = messageIdBuilder.build();
			seekBuilder.setMessageId(messageId);

			PulsarApi.CommandSeek seek = seekBuilder.build();
			IByteBuffer res = serializeWithSize(PulsarApi.BaseCommand.newBuilder().setType(PulsarApi.BaseCommand.Type.SEEK).setSeek(seek));
			messageId.recycle();
			messageIdBuilder.recycle();
			seekBuilder.recycle();
			seek.recycle();
			return res;
		}

		public static IByteBuffer newSeek(long consumerId, long requestId, long timestamp)
		{
			PulsarApi.CommandSeek.Builder seekBuilder = PulsarApi.CommandSeek.newBuilder();
			seekBuilder.ConsumerId = consumerId;
			seekBuilder.RequestId = requestId;

			seekBuilder.MessagePublishTime = timestamp;

			PulsarApi.CommandSeek seek = seekBuilder.build();
			IByteBuffer res = serializeWithSize(PulsarApi.BaseCommand.newBuilder().setType(PulsarApi.BaseCommand.Type.SEEK).setSeek(seek));

			seekBuilder.recycle();
			seek.recycle();
			return res;
		}

		public static IByteBuffer newCloseConsumer(long consumerId, long requestId)
		{
			PulsarApi.CommandCloseConsumer.Builder closeConsumerBuilder = PulsarApi.CommandCloseConsumer.newBuilder();
			closeConsumerBuilder.ConsumerId = consumerId;
			closeConsumerBuilder.RequestId = requestId;
			PulsarApi.CommandCloseConsumer closeConsumer = closeConsumerBuilder.build();
			IByteBuffer res = serializeWithSize(PulsarApi.BaseCommand.newBuilder().setType(PulsarApi.BaseCommand.Type.CLOSE_CONSUMER).setCloseConsumer(closeConsumer));
			closeConsumerBuilder.recycle();
			closeConsumer.recycle();
			return res;
		}

		public static IByteBuffer newReachedEndOfTopic(long consumerId)
		{
			PulsarApi.CommandReachedEndOfTopic.Builder reachedEndOfTopicBuilder = PulsarApi.CommandReachedEndOfTopic.newBuilder();
			reachedEndOfTopicBuilder.ConsumerId = consumerId;
			PulsarApi.CommandReachedEndOfTopic reachedEndOfTopic = reachedEndOfTopicBuilder.build();
			IByteBuffer res = serializeWithSize(PulsarApi.BaseCommand.newBuilder().setType(PulsarApi.BaseCommand.Type.REACHED_END_OF_TOPIC).setReachedEndOfTopic(reachedEndOfTopic));
			reachedEndOfTopicBuilder.recycle();
			reachedEndOfTopic.recycle();
			return res;
		}

		public static IByteBuffer newCloseProducer(long producerId, long requestId)
		{
			PulsarApi.CommandCloseProducer.Builder closeProducerBuilder = PulsarApi.CommandCloseProducer.newBuilder();
			closeProducerBuilder.ProducerId = producerId;
			closeProducerBuilder.RequestId = requestId;
			PulsarApi.CommandCloseProducer closeProducer = closeProducerBuilder.build();
			IByteBuffer res = serializeWithSize(PulsarApi.BaseCommand.newBuilder().setType(PulsarApi.BaseCommand.Type.CLOSE_PRODUCER).setCloseProducer(closeProducerBuilder));
			closeProducerBuilder.recycle();
			closeProducer.recycle();
			return res;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @VisibleForTesting public static io.netty.buffer.IByteBuffer newProducer(String topic, long producerId, long requestId, String producerName, java.util.Map<String, String> metadata)
		public static IByteBuffer newProducer(string topic, long producerId, long requestId, string producerName, IDictionary<string, string> metadata)
		{
			return newProducer(topic, producerId, requestId, producerName, false, metadata);
		}

		public static IByteBuffer newProducer(string topic, long producerId, long requestId, string producerName, bool encrypted, IDictionary<string, string> metadata)
		{
			return newProducer(topic, producerId, requestId, producerName, encrypted, metadata, null, 0, false);
		}

		private static PulsarApi.Schema.Type getSchemaType(SchemaType type)
		{
			if (type.Value < 0)
			{
				return PulsarApi.Schema.Type.None;
			}
			else
			{
				return PulsarApi.Schema.Type.valueOf(type.Value);
			}
		}

		public static SchemaType GetSchemaType(Proto.Schema.Type type)
		{
			if (type.Number < 0)
			{
				// this is unexpected
				return SchemaType.NONE;
			}
			else
			{
				return SchemaType.valueOf(type.Number);
			}
		}

		private static PulsarApi.Schema getSchema(SchemaInfo schemaInfo)
		{
			PulsarApi.Schema.Builder builder = PulsarApi.Schema.newBuilder().setName(schemaInfo.Name).setSchemaData(copyFrom(schemaInfo.Schema)).setType(getSchemaType(schemaInfo.Type)).addAllProperties(schemaInfo.Properties.entrySet().Select(entry => PulsarApi.KeyValue.newBuilder().setKey(entry.Key).setValue(entry.Value).build()).ToList());
			PulsarApi.Schema schema = builder.build();
			builder.recycle();
			return schema;
		}

		public static IByteBuffer newProducer(string topic, long producerId, long requestId, string producerName, bool encrypted, IDictionary<string, string> metadata, SchemaInfo schemaInfo, long epoch, bool userProvidedProducerName)
		{
			PulsarApi.CommandProducer.Builder producerBuilder = PulsarApi.CommandProducer.newBuilder();
			producerBuilder.setTopic(topic);
			producerBuilder.ProducerId = producerId;
			producerBuilder.RequestId = requestId;
			producerBuilder.Epoch = epoch;
			if (!string.ReferenceEquals(producerName, null))
			{
				producerBuilder.setProducerName(producerName);
			}
			producerBuilder.UserProvidedProducerName = userProvidedProducerName;
			producerBuilder.Encrypted = encrypted;

			producerBuilder.addAllMetadata(CommandUtils.ToKeyValueList(metadata));

			if (null != schemaInfo)
			{
				producerBuilder.setSchema(getSchema(schemaInfo));
			}

			PulsarApi.CommandProducer producer = producerBuilder.build();
			IByteBuffer res = serializeWithSize(PulsarApi.BaseCommand.newBuilder().setType(PulsarApi.BaseCommand.Type.PRODUCER).setProducer(producer));
			producerBuilder.recycle();
			producer.recycle();
			return res;
		}

		public static IByteBuffer newPartitionMetadataResponse(PulsarApi.ServerError error, string errorMsg, long requestId)
		{
			PulsarApi.CommandPartitionedTopicMetadataResponse.Builder partitionMetadataResponseBuilder = PulsarApi.CommandPartitionedTopicMetadataResponse.newBuilder();
			partitionMetadataResponseBuilder.RequestId = requestId;
			partitionMetadataResponseBuilder.Error = error;
			partitionMetadataResponseBuilder.Response = PulsarApi.CommandPartitionedTopicMetadataResponse.LookupType.Failed;
			if (!string.ReferenceEquals(errorMsg, null))
			{
				partitionMetadataResponseBuilder.setMessage(errorMsg);
			}

			PulsarApi.CommandPartitionedTopicMetadataResponse partitionMetadataResponse = partitionMetadataResponseBuilder.build();
			IByteBuffer res = serializeWithSize(PulsarApi.BaseCommand.newBuilder().setType(PulsarApi.BaseCommand.Type.PARTITIONED_METADATA_RESPONSE).setPartitionMetadataResponse(partitionMetadataResponse));
			partitionMetadataResponseBuilder.recycle();
			partitionMetadataResponse.recycle();
			return res;
		}

		public static IByteBuffer newPartitionMetadataRequest(string topic, long requestId)
		{
			PulsarApi.CommandPartitionedTopicMetadata.Builder partitionMetadataBuilder = PulsarApi.CommandPartitionedTopicMetadata.newBuilder();
			partitionMetadataBuilder.setTopic(topic);
			partitionMetadataBuilder.RequestId = requestId;
			PulsarApi.CommandPartitionedTopicMetadata partitionMetadata = partitionMetadataBuilder.build();
			IByteBuffer res = serializeWithSize(PulsarApi.BaseCommand.newBuilder().setType(PulsarApi.BaseCommand.Type.PARTITIONED_METADATA).setPartitionMetadata(partitionMetadata));
			partitionMetadataBuilder.recycle();
			partitionMetadata.recycle();
			return res;
		}

		public static IByteBuffer newPartitionMetadataResponse(int partitions, long requestId)
		{
			PulsarApi.CommandPartitionedTopicMetadataResponse.Builder partitionMetadataResponseBuilder = PulsarApi.CommandPartitionedTopicMetadataResponse.newBuilder();
			partitionMetadataResponseBuilder.Partitions = partitions;
			partitionMetadataResponseBuilder.Response = PulsarApi.CommandPartitionedTopicMetadataResponse.LookupType.Success;
			partitionMetadataResponseBuilder.RequestId = requestId;

			PulsarApi.CommandPartitionedTopicMetadataResponse partitionMetadataResponse = partitionMetadataResponseBuilder.build();
			IByteBuffer res = serializeWithSize(PulsarApi.BaseCommand.newBuilder().setType(PulsarApi.BaseCommand.Type.PARTITIONED_METADATA_RESPONSE).setPartitionMetadataResponse(partitionMetadataResponse));
			partitionMetadataResponseBuilder.recycle();
			partitionMetadataResponse.recycle();
			return res;
		}

		public static IByteBuffer newLookup(string topic, bool authoritative, long requestId)
		{
			PulsarApi.CommandLookupTopic.Builder lookupTopicBuilder = PulsarApi.CommandLookupTopic.newBuilder();
			lookupTopicBuilder.setTopic(topic);
			lookupTopicBuilder.RequestId = requestId;
			lookupTopicBuilder.Authoritative = authoritative;
			PulsarApi.CommandLookupTopic lookupBroker = lookupTopicBuilder.build();
			IByteBuffer res = serializeWithSize(PulsarApi.BaseCommand.newBuilder().setType(PulsarApi.BaseCommand.Type.LOOKUP).setLookupTopic(lookupBroker));
			lookupTopicBuilder.recycle();
			lookupBroker.recycle();
			return res;
		}

		public static IByteBuffer newLookupResponse(string brokerServiceUrl, string brokerServiceUrlTls, bool authoritative, PulsarApi.CommandLookupTopicResponse.LookupType response, long requestId, bool proxyThroughServiceUrl)
		{
			PulsarApi.CommandLookupTopicResponse.Builder commandLookupTopicResponseBuilder = PulsarApi.CommandLookupTopicResponse.newBuilder();
			commandLookupTopicResponseBuilder.setBrokerServiceUrl(brokerServiceUrl);
			if (!string.ReferenceEquals(brokerServiceUrlTls, null))
			{
				commandLookupTopicResponseBuilder.setBrokerServiceUrlTls(brokerServiceUrlTls);
			}
			commandLookupTopicResponseBuilder.Response = response;
			commandLookupTopicResponseBuilder.RequestId = requestId;
			commandLookupTopicResponseBuilder.Authoritative = authoritative;
			commandLookupTopicResponseBuilder.ProxyThroughServiceUrl = proxyThroughServiceUrl;

			PulsarApi.CommandLookupTopicResponse commandLookupTopicResponse = commandLookupTopicResponseBuilder.build();
			IByteBuffer res = serializeWithSize(PulsarApi.BaseCommand.newBuilder().setType(PulsarApi.BaseCommand.Type.LOOKUP_RESPONSE).setLookupTopicResponse(commandLookupTopicResponse));
			commandLookupTopicResponseBuilder.recycle();
			commandLookupTopicResponse.recycle();
			return res;
		}

		public static IByteBuffer newLookupErrorResponse(PulsarApi.ServerError error, string errorMsg, long requestId)
		{
			PulsarApi.CommandLookupTopicResponse.Builder connectionBuilder = PulsarApi.CommandLookupTopicResponse.newBuilder();
			connectionBuilder.RequestId = requestId;
			connectionBuilder.Error = error;
			if (!string.ReferenceEquals(errorMsg, null))
			{
				connectionBuilder.setMessage(errorMsg);
			}
			connectionBuilder.Response = PulsarApi.CommandLookupTopicResponse.LookupType.Failed;

			PulsarApi.CommandLookupTopicResponse connectionBroker = connectionBuilder.build();
			IByteBuffer res = serializeWithSize(PulsarApi.BaseCommand.newBuilder().setType(PulsarApi.BaseCommand.Type.LOOKUP_RESPONSE).setLookupTopicResponse(connectionBroker));
			connectionBuilder.recycle();
			connectionBroker.recycle();
			return res;
		}

		public static IByteBuffer newMultiMessageAck(long consumerId, IList<Pair<long, long>> entries)
		{
			PulsarApi.CommandAck.Builder ackBuilder = PulsarApi.CommandAck.newBuilder();
			ackBuilder.ConsumerId = consumerId;
			ackBuilder.AckType = PulsarApi.CommandAck.AckType.Individual;

			int entriesCount = entries.Count;
			for (int i = 0; i < entriesCount; i++)
			{
				long ledgerId = entries[i].Left;
				long entryId = entries[i].Right;

				PulsarApi.MessageIdData.Builder messageIdDataBuilder = PulsarApi.MessageIdData.newBuilder();
				messageIdDataBuilder.LedgerId = ledgerId;
				messageIdDataBuilder.EntryId = entryId;
				PulsarApi.MessageIdData messageIdData = messageIdDataBuilder.build();
				ackBuilder.addMessageId(messageIdData);

				messageIdDataBuilder.recycle();
			}

			PulsarApi.CommandAck ack = ackBuilder.build();

			IByteBuffer res = serializeWithSize(PulsarApi.BaseCommand.newBuilder().setType(PulsarApi.BaseCommand.Type.ACK).setAck(ack));

			for (int i = 0; i < entriesCount; i++)
			{
				ack.getMessageId(i).recycle();
			}
			ack.recycle();
			ackBuilder.recycle();
			return res;
		}

		public static IByteBuffer newAck(long consumerId, long ledgerId, long entryId, PulsarApi.CommandAck.AckType ackType, PulsarApi.CommandAck.ValidationError validationError, IDictionary<string, long> properties)
		{
			return newAck(consumerId, ledgerId, entryId, ackType, validationError, properties, 0, 0);
		}

		public static IByteBuffer newAck(long consumerId, long ledgerId, long entryId, PulsarApi.CommandAck.AckType ackType, PulsarApi.CommandAck.ValidationError validationError, IDictionary<string, long> properties, long txnIdLeastBits, long txnIdMostBits)
		{
			PulsarApi.CommandAck.Builder ackBuilder = PulsarApi.CommandAck.newBuilder();
			ackBuilder.ConsumerId = consumerId;
			ackBuilder.AckType = ackType;
			PulsarApi.MessageIdData.Builder messageIdDataBuilder = PulsarApi.MessageIdData.newBuilder();
			messageIdDataBuilder.LedgerId = ledgerId;
			messageIdDataBuilder.EntryId = entryId;
			PulsarApi.MessageIdData messageIdData = messageIdDataBuilder.build();
			ackBuilder.addMessageId(messageIdData);
			if (validationError != null)
			{
				ackBuilder.ValidationError = validationError;
			}
			if (txnIdMostBits > 0)
			{
				ackBuilder.TxnidMostBits = txnIdMostBits;
			}
			if (txnIdLeastBits > 0)
			{
				ackBuilder.TxnidLeastBits = txnIdLeastBits;
			}
			foreach (KeyValuePair<string, long> e in properties.SetOfKeyValuePairs())
			{
				ackBuilder.addProperties(PulsarApi.KeyLongValue.newBuilder().setKey(e.Key).setValue(e.Value).build());
			}
			PulsarApi.CommandAck ack = ackBuilder.build();

			IByteBuffer res = serializeWithSize(PulsarApi.BaseCommand.newBuilder().setType(PulsarApi.BaseCommand.Type.ACK).setAck(ack));
			ack.recycle();
			ackBuilder.recycle();
			messageIdDataBuilder.recycle();
			messageIdData.recycle();
			return res;
		}

		public static IByteBuffer newAckResponse(long consumerId, long txnIdLeastBits, long txnIdMostBits)
		{
			PulsarApi.CommandAckResponse.Builder commandAckResponseBuilder = PulsarApi.CommandAckResponse.newBuilder();
			commandAckResponseBuilder.ConsumerId = consumerId;
			commandAckResponseBuilder.TxnidLeastBits = txnIdLeastBits;
			commandAckResponseBuilder.TxnidMostBits = txnIdMostBits;
			PulsarApi.CommandAckResponse commandAckResponse = commandAckResponseBuilder.build();

			IByteBuffer res = serializeWithSize(PulsarApi.BaseCommand.newBuilder().setType(PulsarApi.BaseCommand.Type.ACK_RESPONSE).setAckResponse(commandAckResponse));
			commandAckResponseBuilder.recycle();
			commandAckResponse.recycle();

			return res;
		}

		public static IByteBuffer newAckErrorResponse(PulsarApi.ServerError error, string errorMsg, long consumerId)
		{
			PulsarApi.CommandAckResponse.Builder ackErrorBuilder = PulsarApi.CommandAckResponse.newBuilder();
			ackErrorBuilder.ConsumerId = consumerId;
			ackErrorBuilder.Error = error;
			if (!string.ReferenceEquals(errorMsg, null))
			{
				ackErrorBuilder.setMessage(errorMsg);
			}

			PulsarApi.CommandAckResponse response = ackErrorBuilder.build();
			IByteBuffer res = serializeWithSize(PulsarApi.BaseCommand.newBuilder().setType(PulsarApi.BaseCommand.Type.ACK_RESPONSE).setAckResponse(response));

			ackErrorBuilder.recycle();
			response.recycle();

			return res;
		}

		public static IByteBuffer newFlow(long consumerId, int messagePermits)
		{
			PulsarApi.CommandFlow.Builder flowBuilder = PulsarApi.CommandFlow.newBuilder();
			flowBuilder.ConsumerId = consumerId;
			flowBuilder.MessagePermits = messagePermits;
			PulsarApi.CommandFlow flow = flowBuilder.build();

			IByteBuffer res = serializeWithSize(PulsarApi.BaseCommand.newBuilder().setType(PulsarApi.BaseCommand.Type.FLOW).setFlow(flowBuilder));
			flow.recycle();
			flowBuilder.recycle();
			return res;
		}

		public static IByteBuffer newRedeliverUnacknowledgedMessages(long consumerId)
		{
			PulsarApi.CommandRedeliverUnacknowledgedMessages.Builder redeliverBuilder = PulsarApi.CommandRedeliverUnacknowledgedMessages.newBuilder();
			redeliverBuilder.ConsumerId = consumerId;
			PulsarApi.CommandRedeliverUnacknowledgedMessages redeliver = redeliverBuilder.build();
			IByteBuffer res = serializeWithSize(PulsarApi.BaseCommand.newBuilder().setType(PulsarApi.BaseCommand.Type.REDELIVER_UNACKNOWLEDGED_MESSAGES).setRedeliverUnacknowledgedMessages(redeliverBuilder));
			redeliver.recycle();
			redeliverBuilder.recycle();
			return res;
		}

		public static IByteBuffer newRedeliverUnacknowledgedMessages(long consumerId, IList<PulsarApi.MessageIdData> messageIds)
		{
			PulsarApi.CommandRedeliverUnacknowledgedMessages.Builder redeliverBuilder = PulsarApi.CommandRedeliverUnacknowledgedMessages.newBuilder();
			redeliverBuilder.ConsumerId = consumerId;
			redeliverBuilder.addAllMessageIds(messageIds);
			PulsarApi.CommandRedeliverUnacknowledgedMessages redeliver = redeliverBuilder.build();
			IByteBuffer res = serializeWithSize(PulsarApi.BaseCommand.newBuilder().setType(PulsarApi.BaseCommand.Type.REDELIVER_UNACKNOWLEDGED_MESSAGES).setRedeliverUnacknowledgedMessages(redeliverBuilder));
			redeliver.recycle();
			redeliverBuilder.recycle();
			return res;
		}

		public static IByteBuffer newConsumerStatsResponse(PulsarApi.ServerError serverError, string errMsg, long requestId)
		{
			PulsarApi.CommandConsumerStatsResponse.Builder commandConsumerStatsResponseBuilder = PulsarApi.CommandConsumerStatsResponse.newBuilder();
			commandConsumerStatsResponseBuilder.RequestId = requestId;
			commandConsumerStatsResponseBuilder.setErrorMessage(errMsg);
			commandConsumerStatsResponseBuilder.ErrorCode = serverError;

			PulsarApi.CommandConsumerStatsResponse commandConsumerStatsResponse = commandConsumerStatsResponseBuilder.build();
			IByteBuffer res = serializeWithSize(PulsarApi.BaseCommand.newBuilder().setType(PulsarApi.BaseCommand.Type.CONSUMER_STATS_RESPONSE).setConsumerStatsResponse(commandConsumerStatsResponseBuilder));
			commandConsumerStatsResponse.recycle();
			commandConsumerStatsResponseBuilder.recycle();
			return res;
		}

		public static IByteBuffer newConsumerStatsResponse(PulsarApi.CommandConsumerStatsResponse.Builder builder)
		{
			PulsarApi.CommandConsumerStatsResponse commandConsumerStatsResponse = builder.build();
			IByteBuffer res = serializeWithSize(PulsarApi.BaseCommand.newBuilder().setType(PulsarApi.BaseCommand.Type.CONSUMER_STATS_RESPONSE).setConsumerStatsResponse(builder));
			commandConsumerStatsResponse.recycle();
			builder.recycle();
			return res;
		}

		public static IByteBuffer newGetTopicsOfNamespaceRequest(string @namespace, long requestId, PulsarApi.CommandGetTopicsOfNamespace.Mode mode)
		{
			PulsarApi.CommandGetTopicsOfNamespace.Builder topicsBuilder = PulsarApi.CommandGetTopicsOfNamespace.newBuilder();
			topicsBuilder.setNamespace(@namespace).setRequestId(requestId).setMode(mode);

			PulsarApi.CommandGetTopicsOfNamespace topicsCommand = topicsBuilder.build();
			IByteBuffer res = serializeWithSize(PulsarApi.BaseCommand.newBuilder().setType(PulsarApi.BaseCommand.Type.GET_TOPICS_OF_NAMESPACE).setGetTopicsOfNamespace(topicsCommand));
			topicsBuilder.recycle();
			topicsCommand.recycle();
			return res;
		}

		public static IByteBuffer newGetTopicsOfNamespaceResponse(IList<string> topics, long requestId)
		{
			PulsarApi.CommandGetTopicsOfNamespaceResponse.Builder topicsResponseBuilder = PulsarApi.CommandGetTopicsOfNamespaceResponse.newBuilder();

			topicsResponseBuilder.setRequestId(requestId).addAllTopics(topics);

			PulsarApi.CommandGetTopicsOfNamespaceResponse topicsOfNamespaceResponse = topicsResponseBuilder.build();
			IByteBuffer res = serializeWithSize(PulsarApi.BaseCommand.newBuilder().setType(PulsarApi.BaseCommand.Type.GET_TOPICS_OF_NAMESPACE_RESPONSE).setGetTopicsOfNamespaceResponse(topicsOfNamespaceResponse));

			topicsResponseBuilder.recycle();
			topicsOfNamespaceResponse.recycle();
			return res;
		}

		private static readonly IByteBuffer cmdPing;

		static Commands()
		{
			IByteBuffer serializedCmdPing = serializeWithSize(PulsarApi.BaseCommand.newBuilder().setType(PulsarApi.BaseCommand.Type.PING).setPing(PulsarApi.CommandPing.DefaultInstance));
			cmdPing = Unpooled.copiedBuffer(serializedCmdPing);
			serializedCmdPing.release();
			IByteBuffer serializedCmdPong = serializeWithSize(PulsarApi.BaseCommand.newBuilder().setType(PulsarApi.BaseCommand.Type.PONG).setPong(PulsarApi.CommandPong.DefaultInstance));
			cmdPong = Unpooled.copiedBuffer(serializedCmdPong);
			serializedCmdPong.release();
		}

		internal static IByteBuffer newPing()
		{
			return cmdPing.retainedDuplicate();
		}

		private static readonly IByteBuffer cmdPong;


		internal static IByteBuffer newPong()
		{
			return cmdPong.retainedDuplicate();
		}

		public static IByteBuffer newGetLastMessageId(long consumerId, long requestId)
		{
			PulsarApi.CommandGetLastMessageId.Builder cmdBuilder = PulsarApi.CommandGetLastMessageId.newBuilder();
			cmdBuilder.setConsumerId(consumerId).setRequestId(requestId);

			IByteBuffer res = serializeWithSize(PulsarApi.BaseCommand.newBuilder().setType(PulsarApi.BaseCommand.Type.GET_LAST_MESSAGE_ID).setGetLastMessageId(cmdBuilder.build()));
			cmdBuilder.recycle();
			return res;
		}

		public static IByteBuffer newGetLastMessageIdResponse(long requestId, PulsarApi.MessageIdData messageIdData)
		{
			PulsarApi.CommandGetLastMessageIdResponse.Builder response = PulsarApi.CommandGetLastMessageIdResponse.newBuilder().setLastMessageId(messageIdData).setRequestId(requestId);

			IByteBuffer res = serializeWithSize(PulsarApi.BaseCommand.newBuilder().setType(PulsarApi.BaseCommand.Type.GET_LAST_MESSAGE_ID_RESPONSE).setGetLastMessageIdResponse(response.build()));
			response.recycle();
			return res;
		}

		public static IByteBuffer newGetSchema(long requestId, string topic, Optional<SchemaVersion> version)
		{
			PulsarApi.CommandGetSchema.Builder schema = PulsarApi.CommandGetSchema.newBuilder().setRequestId(requestId);
			schema.setTopic(topic);
			if (version.Present)
			{
				schema.SchemaVersion = ByteString.copyFrom(version.get().bytes());
			}

			PulsarApi.CommandGetSchema getSchema = schema.build();

			IByteBuffer res = serializeWithSize(PulsarApi.BaseCommand.newBuilder().setType(PulsarApi.BaseCommand.Type.GET_SCHEMA).setGetSchema(getSchema));
			schema.recycle();
			return res;
		}

		public static IByteBuffer newGetSchemaResponse(long requestId, PulsarApi.CommandGetSchemaResponse response)
		{
			PulsarApi.CommandGetSchemaResponse.Builder schemaResponseBuilder = PulsarApi.CommandGetSchemaResponse.newBuilder(response).setRequestId(requestId);

			IByteBuffer res = serializeWithSize(PulsarApi.BaseCommand.newBuilder().setType(PulsarApi.BaseCommand.Type.GET_SCHEMA_RESPONSE).setGetSchemaResponse(schemaResponseBuilder.build()));
			schemaResponseBuilder.recycle();
			return res;
		}

		public static IByteBuffer newGetSchemaResponse(long requestId, SchemaInfo schema, SchemaVersion version)
		{
			PulsarApi.CommandGetSchemaResponse.Builder schemaResponse = PulsarApi.CommandGetSchemaResponse.newBuilder().setRequestId(requestId).setSchemaVersion(ByteString.copyFrom(version.bytes())).setSchema(getSchema(schema));

			IByteBuffer res = serializeWithSize(PulsarApi.BaseCommand.newBuilder().setType(PulsarApi.BaseCommand.Type.GET_SCHEMA_RESPONSE).setGetSchemaResponse(schemaResponse.build()));
			schemaResponse.recycle();
			return res;
		}

		public static IByteBuffer newGetSchemaResponseError(long requestId, PulsarApi.ServerError error, string errorMessage)
		{
			PulsarApi.CommandGetSchemaResponse.Builder schemaResponse = PulsarApi.CommandGetSchemaResponse.newBuilder().setRequestId(requestId).setErrorCode(error).setErrorMessage(errorMessage);

			IByteBuffer res = serializeWithSize(PulsarApi.BaseCommand.newBuilder().setType(PulsarApi.BaseCommand.Type.GET_SCHEMA_RESPONSE).setGetSchemaResponse(schemaResponse.build()));
			schemaResponse.recycle();
			return res;
		}

		public static IByteBuffer newGetOrCreateSchema(long requestId, string topic, SchemaInfo schemaInfo)
		{
			PulsarApi.CommandGetOrCreateSchema getOrCreateSchema = PulsarApi.CommandGetOrCreateSchema.newBuilder().setRequestId(requestId).setTopic(topic).setSchema(getSchema(schemaInfo)).build();
			IByteBuffer res = serializeWithSize(PulsarApi.BaseCommand.newBuilder().setType(PulsarApi.BaseCommand.Type.GET_OR_CREATE_SCHEMA).setGetOrCreateSchema(getOrCreateSchema));
			getOrCreateSchema.recycle();
			return res;
		}

		public static IByteBuffer newGetOrCreateSchemaResponse(long requestId, SchemaVersion schemaVersion)
		{
			PulsarApi.CommandGetOrCreateSchemaResponse.Builder schemaResponse = PulsarApi.CommandGetOrCreateSchemaResponse.newBuilder().setRequestId(requestId).setSchemaVersion(ByteString.copyFrom(schemaVersion.bytes()));
			IByteBuffer res = serializeWithSize(PulsarApi.BaseCommand.newBuilder().setType(PulsarApi.BaseCommand.Type.GET_OR_CREATE_SCHEMA_RESPONSE).setGetOrCreateSchemaResponse(schemaResponse.build()));
			schemaResponse.recycle();
			return res;
		}

		public static IByteBuffer newGetOrCreateSchemaResponseError(long requestId, PulsarApi.ServerError error, string errorMessage)
		{
			PulsarApi.CommandGetOrCreateSchemaResponse.Builder schemaResponse = PulsarApi.CommandGetOrCreateSchemaResponse.newBuilder().setRequestId(requestId).setErrorCode(error).setErrorMessage(errorMessage);
			IByteBuffer res = serializeWithSize(PulsarApi.BaseCommand.newBuilder().setType(PulsarApi.BaseCommand.Type.GET_OR_CREATE_SCHEMA_RESPONSE).setGetOrCreateSchemaResponse(schemaResponse.build()));
			schemaResponse.recycle();
			return res;
		}

		// ---- transaction related ----

		public static IByteBuffer newTxn(long tcId, long requestId, long ttlSeconds)
		{
			PulsarApi.CommandNewTxn commandNewTxn = PulsarApi.CommandNewTxn.newBuilder().setTcId(tcId).setRequestId(requestId).setTxnTtlSeconds(ttlSeconds).build();
			IByteBuffer res = serializeWithSize(PulsarApi.BaseCommand.newBuilder().setType(PulsarApi.BaseCommand.Type.NEW_TXN).setNewTxn(commandNewTxn));
			commandNewTxn.recycle();
			return res;
		}

		public static IByteBuffer newTxnResponse(long requestId, long txnIdLeastBits, long txnIdMostBits)
		{
			PulsarApi.CommandNewTxnResponse commandNewTxnResponse = PulsarApi.CommandNewTxnResponse.newBuilder().setRequestId(requestId).setTxnidMostBits(txnIdMostBits).setTxnidLeastBits(txnIdLeastBits).build();
			IByteBuffer res = serializeWithSize(PulsarApi.BaseCommand.newBuilder().setType(PulsarApi.BaseCommand.Type.NEW_TXN_RESPONSE).setNewTxnResponse(commandNewTxnResponse));
			commandNewTxnResponse.recycle();

			return res;
		}

		public static IByteBuffer newTxnResponse(long requestId, long txnIdMostBits, PulsarApi.ServerError error, string errorMsg)
		{
			PulsarApi.CommandNewTxnResponse.Builder builder = PulsarApi.CommandNewTxnResponse.newBuilder();
			builder.RequestId = requestId;
			builder.TxnidMostBits = txnIdMostBits;
			builder.Error = error;
			if (!string.ReferenceEquals(errorMsg, null))
			{
				builder.setMessage(errorMsg);
			}
			PulsarApi.CommandNewTxnResponse errorResponse = builder.build();
			IByteBuffer res = serializeWithSize(PulsarApi.BaseCommand.newBuilder().setType(PulsarApi.BaseCommand.Type.NEW_TXN_RESPONSE).setNewTxnResponse(errorResponse));
			builder.recycle();
			errorResponse.recycle();

			return res;
		}

		public static IByteBuffer newAddPartitionToTxn(long requestId, long txnIdLeastBits, long txnIdMostBits)
		{
			PulsarApi.CommandAddPartitionToTxn commandAddPartitionToTxn = PulsarApi.CommandAddPartitionToTxn.newBuilder().setRequestId(requestId).setTxnidLeastBits(txnIdLeastBits).setTxnidMostBits(txnIdMostBits).build();
			IByteBuffer res = serializeWithSize(PulsarApi.BaseCommand.newBuilder().setType(PulsarApi.BaseCommand.Type.ADD_PARTITION_TO_TXN).setAddPartitionToTxn(commandAddPartitionToTxn));
			commandAddPartitionToTxn.recycle();
			return res;
		}

		public static IByteBuffer newAddPartitionToTxnResponse(long requestId, long txnIdLeastBits, long txnIdMostBits)
		{
			PulsarApi.CommandAddPartitionToTxnResponse commandAddPartitionToTxnResponse = PulsarApi.CommandAddPartitionToTxnResponse.newBuilder().setRequestId(requestId).setTxnidLeastBits(txnIdLeastBits).setTxnidMostBits(txnIdMostBits).build();
			IByteBuffer res = serializeWithSize(PulsarApi.BaseCommand.newBuilder().setType(PulsarApi.BaseCommand.Type.ADD_PARTITION_TO_TXN_RESPONSE).setAddPartitionToTxnResponse(commandAddPartitionToTxnResponse));
			commandAddPartitionToTxnResponse.recycle();
			return res;
		}

		public static IByteBuffer newAddPartitionToTxnResponse(long requestId, long txnIdMostBits, PulsarApi.ServerError error, string errorMsg)
		{
			PulsarApi.CommandAddPartitionToTxnResponse.Builder builder = PulsarApi.CommandAddPartitionToTxnResponse.newBuilder();
			builder.RequestId = requestId;
			builder.TxnidMostBits = txnIdMostBits;
			builder.Error = error;
			if (!string.ReferenceEquals(errorMsg, null))
			{
				builder.setMessage(errorMsg);
			}
			PulsarApi.CommandAddPartitionToTxnResponse commandAddPartitionToTxnResponse = builder.build();
			IByteBuffer res = serializeWithSize(PulsarApi.BaseCommand.newBuilder().setType(PulsarApi.BaseCommand.Type.ADD_PARTITION_TO_TXN_RESPONSE).setAddPartitionToTxnResponse(commandAddPartitionToTxnResponse));
			builder.recycle();
			commandAddPartitionToTxnResponse.recycle();
			return res;
		}

		public static IByteBuffer newAddSubscriptionToTxn(long requestId, long txnIdLeastBits, long txnIdMostBits, IList<PulsarApi.Subscription> subscription)
		{
			PulsarApi.CommandAddSubscriptionToTxn commandAddSubscriptionToTxn = PulsarApi.CommandAddSubscriptionToTxn.newBuilder().setRequestId(requestId).setTxnidLeastBits(txnIdLeastBits).setTxnidMostBits(txnIdMostBits).addAllSubscription(subscription).build();
			IByteBuffer res = serializeWithSize(PulsarApi.BaseCommand.newBuilder().setType(PulsarApi.BaseCommand.Type.ADD_SUBSCRIPTION_TO_TXN).setAddSubscriptionToTxn(commandAddSubscriptionToTxn));
			commandAddSubscriptionToTxn.recycle();
			return res;
		}

		public static IByteBuffer newAddSubscriptionToTxnResponse(long requestId, long txnIdLeastBits, long txnIdMostBits)
		{
			PulsarApi.CommandAddSubscriptionToTxnResponse command = PulsarApi.CommandAddSubscriptionToTxnResponse.newBuilder().setRequestId(requestId).setTxnidLeastBits(txnIdLeastBits).setTxnidMostBits(txnIdMostBits).build();
			IByteBuffer res = serializeWithSize(PulsarApi.BaseCommand.newBuilder().setType(PulsarApi.BaseCommand.Type.ADD_SUBSCRIPTION_TO_TXN_RESPONSE).setAddSubscriptionToTxnResponse(command));
			command.recycle();
			return res;
		}

		public static IByteBuffer newAddSubscriptionToTxnResponse(long requestId, long txnIdMostBits, PulsarApi.ServerError error, string errorMsg)
		{
			PulsarApi.CommandAddSubscriptionToTxnResponse.Builder builder = PulsarApi.CommandAddSubscriptionToTxnResponse.newBuilder();
			builder.RequestId = requestId;
			builder.TxnidMostBits = txnIdMostBits;
			builder.Error = error;
			if (!string.ReferenceEquals(errorMsg, null))
			{
				builder.setMessage(errorMsg);
			}
			PulsarApi.CommandAddSubscriptionToTxnResponse errorResponse = builder.build();
			IByteBuffer res = serializeWithSize(PulsarApi.BaseCommand.newBuilder().setType(PulsarApi.BaseCommand.Type.ADD_SUBSCRIPTION_TO_TXN_RESPONSE).setAddSubscriptionToTxnResponse(errorResponse));
			builder.recycle();
			errorResponse.recycle();
			return res;
		}

		public static IByteBuffer newEndTxn(long requestId, long txnIdLeastBits, long txnIdMostBits, PulsarApi.TxnAction txnAction)
		{
			PulsarApi.CommandEndTxn commandEndTxn = PulsarApi.CommandEndTxn.newBuilder().setRequestId(requestId).setTxnidLeastBits(txnIdLeastBits).setTxnidMostBits(txnIdMostBits).setTxnAction(txnAction).build();
			IByteBuffer res = serializeWithSize(PulsarApi.BaseCommand.newBuilder().setType(PulsarApi.BaseCommand.Type.END_TXN).setEndTxn(commandEndTxn));
			commandEndTxn.recycle();
			return res;
		}

		public static IByteBuffer newEndTxnResponse(long requestId, long txnIdLeastBits, long txnIdMostBits)
		{
			PulsarApi.CommandEndTxnResponse commandEndTxnResponse = PulsarApi.CommandEndTxnResponse.newBuilder().setRequestId(requestId).setTxnidLeastBits(txnIdLeastBits).setTxnidMostBits(txnIdMostBits).build();
			IByteBuffer res = serializeWithSize(PulsarApi.BaseCommand.newBuilder().setType(PulsarApi.BaseCommand.Type.END_TXN_RESPONSE).setEndTxnResponse(commandEndTxnResponse));
			commandEndTxnResponse.recycle();
			return res;
		}

		public static IByteBuffer newEndTxnResponse(long requestId, long txnIdMostBits, PulsarApi.ServerError error, string errorMsg)
		{
			PulsarApi.CommandEndTxnResponse.Builder builder = PulsarApi.CommandEndTxnResponse.newBuilder();
			builder.RequestId = requestId;
			builder.TxnidMostBits = txnIdMostBits;
			builder.Error = error;
			if (!string.ReferenceEquals(errorMsg, null))
			{
				builder.setMessage(errorMsg);
			}
			PulsarApi.CommandEndTxnResponse commandEndTxnResponse = builder.build();
			IByteBuffer res = serializeWithSize(PulsarApi.BaseCommand.newBuilder().setType(PulsarApi.BaseCommand.Type.END_TXN_RESPONSE).setEndTxnResponse(commandEndTxnResponse));
			builder.recycle();
			commandEndTxnResponse.recycle();
			return res;
		}

		public static IByteBuffer newEndTxnOnPartition(long requestId, long txnIdLeastBits, long txnIdMostBits, string topic, PulsarApi.TxnAction txnAction)
		{
			PulsarApi.CommandEndTxnOnPartition.Builder txnEndOnPartition = PulsarApi.CommandEndTxnOnPartition.newBuilder().setRequestId(requestId).setTxnidLeastBits(txnIdLeastBits).setTxnidMostBits(txnIdMostBits).setTopic(topic).setTxnAction(txnAction);
			IByteBuffer res = serializeWithSize(PulsarApi.BaseCommand.newBuilder().setType(PulsarApi.BaseCommand.Type.END_TXN_ON_PARTITION).setEndTxnOnPartition(txnEndOnPartition));
			txnEndOnPartition.recycle();
			return res;
		}

		public static IByteBuffer newEndTxnOnPartitionResponse(long requestId, long txnIdLeastBits, long txnIdMostBits)
		{
			PulsarApi.CommandEndTxnOnPartitionResponse commandEndTxnOnPartitionResponse = PulsarApi.CommandEndTxnOnPartitionResponse.newBuilder().setRequestId(requestId).setTxnidLeastBits(txnIdLeastBits).setTxnidMostBits(txnIdMostBits).build();
			IByteBuffer res = serializeWithSize(PulsarApi.BaseCommand.newBuilder().setType(PulsarApi.BaseCommand.Type.END_TXN_ON_PARTITION_RESPONSE).setEndTxnOnPartitionResponse(commandEndTxnOnPartitionResponse));
			commandEndTxnOnPartitionResponse.recycle();
			return res;
		}

		public static IByteBuffer newEndTxnOnPartitionResponse(long requestId, PulsarApi.ServerError error, string errorMsg)
		{
			PulsarApi.CommandEndTxnOnPartitionResponse.Builder builder = PulsarApi.CommandEndTxnOnPartitionResponse.newBuilder();
			builder.RequestId = requestId;
			builder.Error = error;
			if (!string.ReferenceEquals(errorMsg, null))
			{
				builder.setMessage(errorMsg);
			}
			PulsarApi.CommandEndTxnOnPartitionResponse commandEndTxnOnPartitionResponse = builder.build();
			IByteBuffer res = serializeWithSize(PulsarApi.BaseCommand.newBuilder().setType(PulsarApi.BaseCommand.Type.END_TXN_ON_PARTITION_RESPONSE).setEndTxnOnPartitionResponse(commandEndTxnOnPartitionResponse));
			builder.recycle();
			commandEndTxnOnPartitionResponse.recycle();
			return res;
		}

		public static IByteBuffer newEndTxnOnSubscription(long requestId, long txnIdLeastBits, long txnIdMostBits, PulsarApi.Subscription subscription, PulsarApi.TxnAction txnAction)
		{
			PulsarApi.CommandEndTxnOnSubscription commandEndTxnOnSubscription = PulsarApi.CommandEndTxnOnSubscription.newBuilder().setRequestId(requestId).setTxnidLeastBits(txnIdLeastBits).setTxnidMostBits(txnIdMostBits).setSubscription(subscription).setTxnAction(txnAction).build();
			IByteBuffer res = serializeWithSize(PulsarApi.BaseCommand.newBuilder().setType(PulsarApi.BaseCommand.Type.END_TXN_ON_SUBSCRIPTION).setEndTxnOnSubscription(commandEndTxnOnSubscription));
			commandEndTxnOnSubscription.recycle();
			return res;
		}

		public static IByteBuffer newEndTxnOnSubscriptionResponse(long requestId, long txnIdLeastBits, long txnIdMostBits)
		{
			PulsarApi.CommandEndTxnOnSubscriptionResponse response = PulsarApi.CommandEndTxnOnSubscriptionResponse.newBuilder().setRequestId(requestId).setTxnidLeastBits(txnIdLeastBits).setTxnidMostBits(txnIdMostBits).build();
			IByteBuffer res = serializeWithSize(PulsarApi.BaseCommand.newBuilder().setType(PulsarApi.BaseCommand.Type.END_TXN_ON_SUBSCRIPTION_RESPONSE).setEndTxnOnSubscriptionResponse(response));
			response.recycle();
			return res;
		}

		public static IByteBuffer newEndTxnOnSubscriptionResponse(long requestId, PulsarApi.ServerError error, string errorMsg)
		{
			PulsarApi.CommandEndTxnOnSubscriptionResponse.Builder builder = PulsarApi.CommandEndTxnOnSubscriptionResponse.newBuilder();
			builder.RequestId = requestId;
			builder.Error = error;
			if (!string.ReferenceEquals(errorMsg, null))
			{
				builder.setMessage(errorMsg);
			}
			PulsarApi.CommandEndTxnOnSubscriptionResponse response = builder.build();
			IByteBuffer res = serializeWithSize(PulsarApi.BaseCommand.newBuilder().setType(PulsarApi.BaseCommand.Type.END_TXN_ON_SUBSCRIPTION_RESPONSE).setEndTxnOnSubscriptionResponse(response));
			builder.recycle();
			response.recycle();
			return res;
		}
		public static IByteBuffer SerializeWithSize(BaseCommand cmdBuilder)
		{
			// / Wire format
			// [TOTAL_SIZE] [CMD_SIZE][CMD]
			BaseCommand cmd = cmdBuilder;

			int cmdSize = cmd.SerializedSize;
			int totalSize = cmdSize + 4;
			int frameSize = totalSize + 4;

			IByteBuffer buf = PulsarByteBufAllocator.DEFAULT.buffer(frameSize, frameSize);

			// Prepend 2 lengths to the buffer
			buf.WriteInt(totalSize);
			buf.WriteInt(cmdSize);

			ByteBufCodedOutputStream outStream = ByteBufCodedOutputStream.Get(buf);

			try
			{
				cmd.writeTo(outStream);
			}
			catch (IOException e)
			{
				// This is in-memory serialization, should not fail
				throw e;
			}
			finally
			{
				cmd.recycle();
				cmdBuilder.recycle();
				outStream.recycle();
			}

			return buf;
		}

		private static ByteBufPair SerializeCommandSendWithSize(BaseCommand cmdBuilder, ChecksumType checksumType, PulsarApi.MessageMetadata msgMetadata, IByteBuffer payload)
		{
			// / Wire format
			// [TOTAL_SIZE] [CMD_SIZE][CMD] [MAGIC_NUMBER][CHECKSUM] [METADATA_SIZE][METADATA] [PAYLOAD]

			PulsarApi.BaseCommand cmd = cmdBuilder.build();
			int cmdSize = cmd.SerializedSize;
			int msgMetadataSize = msgMetadata.SerializedSize;
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

			IByteBuffer headers = PulsarByteBufAllocator.DEFAULT.buffer(headersSize, headersSize);
			headers.WriteInt(totalSize); // External frame

			try
			{
				// Write cmd
				headers.WriteInt(cmdSize);

				ByteBufCodedOutputStream outStream = ByteBufCodedOutputStream.Get(headers);
				cmd.writeTo(outStream);
				cmd.recycle();
				cmdBuilder.recycle();

				//Create checksum placeholder
				if (includeChecksum)
				{
					headers.WriteShort(magicCrc32c);
					checksumReaderIndex = headers.WriterIndex();
					headers.writerIndex(headers.writerIndex() + checksumSize); //skip 4 bytes of checksum
				}

				// Write metadata
				headers.writeInt(msgMetadataSize);
				msgMetadata.writeTo(outStream);
				outStream.recycle();
			}
			catch (IOException e)
			{
				// This is in-memory serialization, should not fail
				throw new Exception(e);
			}

			ByteBufPair command = ByteBufPair.Get(headers, payload);

			// write checksum at created checksum-placeholder
			if (includeChecksum)
			{
				headers.MarkReaderIndex();
				headers.readerIndex(checksumReaderIndex + checksumSize);
				int metadataChecksum = ComputeChecksum(headers);
				int computedChecksum = ResumeChecksum(metadataChecksum, payload);
				// set computed checksum
				headers.SetInt(checksumReaderIndex, computedChecksum);
				headers.ResetReaderIndex();
			}
			return command;
		}

		public static IByteBuffer SerializeMetadataAndPayload(ChecksumType checksumType, PulsarApi.MessageMetadata msgMetadata, IByteBuffer payload)
		{
			// / Wire format
			// [MAGIC_NUMBER][CHECKSUM] [METADATA_SIZE][METADATA] [PAYLOAD]
			int msgMetadataSize = msgMetadata.SerializedSize;
			int payloadSize = payload.ReadableBytes;
			int magicAndChecksumLength = ChecksumType.Crc32c.Equals(checksumType) ? (2 + 4) : 0;
			bool includeChecksum = magicAndChecksumLength > 0;
			int headerContentSize = magicAndChecksumLength + 4 + msgMetadataSize; // magicLength +
																				  // checksumSize + msgMetadataLength +
																				  // msgMetadataSize
			int checksumReaderIndex = -1;
			int totalSize = headerContentSize + payloadSize;
			IByteBuffer metadataAndPayload = PulsarByteBufAllocator.DEFAULT.buffer(totalSize, totalSize);
			try
			{
				IByteBufferCodedOutputStream outStream = IByteBufferCodedOutputStream.get(metadataAndPayload);

				//Create checksum placeholder
				if (includeChecksum)
				{
					metadataAndPayload.writeShort(magicCrc32c);
					checksumReaderIndex = metadataAndPayload.writerIndex();
					metadataAndPayload.writerIndex(metadataAndPayload.writerIndex() + checksumSize); //skip 4 bytes of checksum
				}

				// Write metadata
				metadataAndPayload.writeInt(msgMetadataSize);
				msgMetadata.writeTo(outStream);
				outStream.recycle();
			}
			catch (IOException e)
			{
				// This is in-memory serialization, should not fail
				throw new Exception(e);
			}

			// write checksum at created checksum-placeholder
			if (includeChecksum)
			{
				metadataAndPayload.markReaderIndex();
				metadataAndPayload.readerIndex(checksumReaderIndex + checksumSize);
				int metadataChecksum = computeChecksum(metadataAndPayload);
				int computedChecksum = resumeChecksum(metadataChecksum, payload);
				// set computed checksum
				metadataAndPayload.setInt(checksumReaderIndex, computedChecksum);
				metadataAndPayload.resetReaderIndex();
			}
			metadataAndPayload.writeBytes(payload);

			return metadataAndPayload;
		}

		public static long initBatchMessageMetadata(PulsarApi.MessageMetadata.Builder messageMetadata, PulsarApi.MessageMetadata.Builder builder)
		{
			messageMetadata.PublishTime = builder.PublishTime;
			messageMetadata.setProducerName(builder.getProducerName());
			messageMetadata.SequenceId = builder.SequenceId;
			if (builder.hasReplicatedFrom())
			{
				messageMetadata.setReplicatedFrom(builder.getReplicatedFrom());
			}
			if (builder.ReplicateToCount > 0)
			{
				messageMetadata.addAllReplicateTo(builder.ReplicateToList);
			}
			if (builder.hasSchemaVersion())
			{
				messageMetadata.SchemaVersion = builder.SchemaVersion;
			}
			return builder.SequenceId;
		}

		public static IByteBuffer serializeSingleMessageInBatchWithPayload(PulsarApi.SingleMessageMetadata.Builder singleMessageMetadataBuilder, IByteBuffer payload, IByteBuffer batchBuffer)
		{
			int payLoadSize = payload.readableBytes();
			PulsarApi.SingleMessageMetadata singleMessageMetadata = singleMessageMetadataBuilder.setPayloadSize(payLoadSize).build();
			// serialize meta-data size, meta-data and payload for single message in batch
			int singleMsgMetadataSize = singleMessageMetadata.SerializedSize;
			try
			{
				batchBuffer.writeInt(singleMsgMetadataSize);
				IByteBufferCodedOutputStream outStream = IByteBufferCodedOutputStream.get(batchBuffer);
				singleMessageMetadata.writeTo(outStream);
				singleMessageMetadata.recycle();
				outStream.recycle();
			}
			catch (IOException e)
			{
				throw new Exception(e);
			}
			return batchBuffer.writeBytes(payload);
		}

		public static IByteBuffer serializeSingleMessageInBatchWithPayload(PulsarApi.MessageMetadata.Builder msgBuilder, IByteBuffer payload, IByteBuffer batchBuffer)
		{

			// build single message meta-data
			PulsarApi.SingleMessageMetadata.Builder singleMessageMetadataBuilder = PulsarApi.SingleMessageMetadata.newBuilder();
			if (msgBuilder.hasPartitionKey())
			{
				singleMessageMetadataBuilder = singleMessageMetadataBuilder.setPartitionKey(msgBuilder.getPartitionKey()).setPartitionKeyB64Encoded(msgBuilder.PartitionKeyB64Encoded);
			}
			if (msgBuilder.hasOrderingKey())
			{
				singleMessageMetadataBuilder = singleMessageMetadataBuilder.setOrderingKey(msgBuilder.OrderingKey);
			}
			if (msgBuilder.PropertiesList.Count > 0)
			{
				singleMessageMetadataBuilder = singleMessageMetadataBuilder.addAllProperties(msgBuilder.PropertiesList);
			}

			if (msgBuilder.hasEventTime())
			{
				singleMessageMetadataBuilder.EventTime = msgBuilder.EventTime;
			}

			if (msgBuilder.hasSequenceId())
			{
				singleMessageMetadataBuilder.SequenceId = msgBuilder.SequenceId;
			}

			try
			{
				return serializeSingleMessageInBatchWithPayload(singleMessageMetadataBuilder, payload, batchBuffer);
			}
			finally
			{
				singleMessageMetadataBuilder.recycle();
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static io.netty.buffer.IByteBuffer deSerializeSingleMessageInBatch(io.netty.buffer.IByteBuffer uncompressedPayload, org.apache.pulsar.common.api.proto.PulsarApi.SingleMessageMetadata.Builder singleMessageMetadataBuilder, int index, int batchSize) throws java.io.IOException
		public static IByteBuffer deSerializeSingleMessageInBatch(IByteBuffer uncompressedPayload, PulsarApi.SingleMessageMetadata.Builder singleMessageMetadataBuilder, int index, int batchSize)
		{
			int singleMetaSize = (int) uncompressedPayload.readUnsignedInt();
			int writerIndex = uncompressedPayload.writerIndex();
			int beginIndex = uncompressedPayload.readerIndex() + singleMetaSize;
			uncompressedPayload.writerIndex(beginIndex);
			IByteBufferCodedInputStream stream = IByteBufferCodedInputStream.get(uncompressedPayload);
			singleMessageMetadataBuilder.mergeFrom(stream, null);
			stream.recycle();

			int singleMessagePayloadSize = singleMessageMetadataBuilder.PayloadSize;

			int readerIndex = uncompressedPayload.readerIndex();
			IByteBuffer singleMessagePayload = uncompressedPayload.retainedSlice(readerIndex, singleMessagePayloadSize);
			uncompressedPayload.writerIndex(writerIndex);

			// reader now points to beginning of payload read; so move it past message payload just read
			if (index < batchSize)
			{
				uncompressedPayload.readerIndex(readerIndex + singleMessagePayloadSize);
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
			int totalSize = 4 + cmdSize + metadataAndPayload.readableBytes();
			int headersSize = 4 + 4 + cmdSize;

			IByteBuffer headers = PulsarIByteBufferAllocator.DEFAULT.buffer(headersSize);
			headers.writeInt(totalSize); // External frame

			try
			{
				// Write cmd
				headers.writeInt(cmdSize);

				IByteBufferCodedOutputStream outStream = IByteBufferCodedOutputStream.get(headers);
				cmd.writeTo(outStream);
				outStream.recycle();
			}
			catch (IOException e)
			{
				// This is in-memory serialization, should not fail
				throw new Exception(e);
			}

			return (IByteBufferPair) IByteBufferPair.get(headers, metadataAndPayload);
		}

		public static int getNumberOfMessagesInBatch(IByteBuffer metadataAndPayload, string subscription, long consumerId)
		{
			PulsarApi.MessageMetadata msgMetadata = peekMessageMetadata(metadataAndPayload, subscription, consumerId);
			if (msgMetadata == null)
			{
				return -1;
			}
			else
			{
				int numMessagesInBatch = msgMetadata.NumMessagesInBatch;
				msgMetadata.recycle();
				return numMessagesInBatch;
			}
		}

		public static PulsarApi.MessageMetadata peekMessageMetadata(IByteBuffer metadataAndPayload, string subscription, long consumerId)
		{
			try
			{
				// save the reader index and restore after parsing
				int readerIdx = metadataAndPayload.readerIndex();
				PulsarApi.MessageMetadata metadata = Commands.parseMessageMetadata(metadataAndPayload);
				metadataAndPayload.readerIndex(readerIdx);

				return metadata;
			}
			catch (Exception t)
			{
				log.error("[{}] [{}] Failed to parse message metadata", subscription, consumerId, t);
				return null;
			}
		}

		public static int CurrentProtocolVersion
		{
			get
			{
				// Return the last ProtocolVersion enum value
				return PulsarApi.ProtocolVersion.values()[PulsarApi.ProtocolVersion.values().length - 1].Number;
			}
		}

		/// <summary>
		/// Definition of possible checksum types.
		/// </summary>
		public enum ChecksumType
		{
			Crc32c,
			None
		}

		public static bool peerSupportsGetLastMessageId(int peerVersion)
		{
			return peerVersion >= PulsarApi.ProtocolVersion.v12.Number;
		}

		public static bool peerSupportsActiveConsumerListener(int peerVersion)
		{
			return peerVersion >= PulsarApi.ProtocolVersion.v12.Number;
		}

		public static bool peerSupportsMultiMessageAcknowledgment(int peerVersion)
		{
			return peerVersion >= PulsarApi.ProtocolVersion.v12.Number;
		}

		public static bool peerSupportJsonSchemaAvroFormat(int peerVersion)
		{
			return peerVersion >= PulsarApi.ProtocolVersion.v13.Number;
		}

		public static bool peerSupportsGetOrCreateSchema(int peerVersion)
		{
			return peerVersion >= PulsarApi.ProtocolVersion.v15.Number;
		}
	}

}