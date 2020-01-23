using System;
using System.Collections.Generic;

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

		public const short MagicCrc32c = 0x0e01;
		private const int ChecksumSize = 4;

		public static ByteBuf NewConnect(string AuthMethodName, string AuthData, string LibVersion)
		{
			return NewConnect(AuthMethodName, AuthData, CurrentProtocolVersion, LibVersion, null, null, null, null);
		}

		public static ByteBuf NewConnect(string AuthMethodName, string AuthData, string LibVersion, string TargetBroker)
		{
			return NewConnect(AuthMethodName, AuthData, CurrentProtocolVersion, LibVersion, TargetBroker, null, null, null);
		}

		public static ByteBuf NewConnect(string AuthMethodName, string AuthData, string LibVersion, string TargetBroker, string OriginalPrincipal, string ClientAuthData, string ClientAuthMethod)
		{
			return NewConnect(AuthMethodName, AuthData, CurrentProtocolVersion, LibVersion, TargetBroker, OriginalPrincipal, ClientAuthData, ClientAuthMethod);
		}

		public static ByteBuf NewConnect(string AuthMethodName, string AuthData, int ProtocolVersion, string LibVersion, string TargetBroker, string OriginalPrincipal, string OriginalAuthData, string OriginalAuthMethod)
		{
			PulsarApi.CommandConnect.Builder ConnectBuilder = PulsarApi.CommandConnect.newBuilder();
			ConnectBuilder.setClientVersion(!string.ReferenceEquals(LibVersion, null) ? LibVersion : "Pulsar Client");
			ConnectBuilder.setAuthMethodName(AuthMethodName);

			if ("ycav1".Equals(AuthMethodName))
			{
				// Handle the case of a client that gets updated before the broker and starts sending the string auth method
				// name. An example would be in broker-to-broker replication. We need to make sure the clients are still
				// passing both the enum and the string until all brokers are upgraded.
				ConnectBuilder.AuthMethod = PulsarApi.AuthMethod.AuthMethodYcaV1;
			}

			if (!string.ReferenceEquals(TargetBroker, null))
			{
				// When connecting through a proxy, we need to specify which broker do we want to be proxied through
				ConnectBuilder.setProxyToBrokerUrl(TargetBroker);
			}

			if (!string.ReferenceEquals(AuthData, null))
			{
				ConnectBuilder.AuthData = copyFromUtf8(AuthData);
			}

			if (!string.ReferenceEquals(OriginalPrincipal, null))
			{
				ConnectBuilder.setOriginalPrincipal(OriginalPrincipal);
			}

			if (!string.ReferenceEquals(OriginalAuthData, null))
			{
				ConnectBuilder.setOriginalAuthData(OriginalAuthData);
			}

			if (!string.ReferenceEquals(OriginalAuthMethod, null))
			{
				ConnectBuilder.setOriginalAuthMethod(OriginalAuthMethod);
			}
			ConnectBuilder.ProtocolVersion = ProtocolVersion;
			PulsarApi.CommandConnect Connect = ConnectBuilder.build();
			ByteBuf Res = SerializeWithSize(PulsarApi.BaseCommand.newBuilder().setType(PulsarApi.BaseCommand.Type.CONNECT).setConnect(Connect));
			Connect.recycle();
			ConnectBuilder.recycle();
			return Res;
		}

		public static ByteBuf NewConnect(string AuthMethodName, AuthData AuthData, int ProtocolVersion, string LibVersion, string TargetBroker, string OriginalPrincipal, AuthData OriginalAuthData, string OriginalAuthMethod)
		{
			PulsarApi.CommandConnect.Builder ConnectBuilder = PulsarApi.CommandConnect.newBuilder();
			ConnectBuilder.setClientVersion(!string.ReferenceEquals(LibVersion, null) ? LibVersion : "Pulsar Client");
			ConnectBuilder.setAuthMethodName(AuthMethodName);

			if (!string.ReferenceEquals(TargetBroker, null))
			{
				// When connecting through a proxy, we need to specify which broker do we want to be proxied through
				ConnectBuilder.setProxyToBrokerUrl(TargetBroker);
			}

			if (AuthData != null)
			{
				ConnectBuilder.AuthData = ByteString.copyFrom(AuthData.Bytes);
			}

			if (!string.ReferenceEquals(OriginalPrincipal, null))
			{
				ConnectBuilder.setOriginalPrincipal(OriginalPrincipal);
			}

			if (OriginalAuthData != null)
			{
				ConnectBuilder.setOriginalAuthData(new string(OriginalAuthData.Bytes, UTF_8));
			}

			if (!string.ReferenceEquals(OriginalAuthMethod, null))
			{
				ConnectBuilder.setOriginalAuthMethod(OriginalAuthMethod);
			}
			ConnectBuilder.ProtocolVersion = ProtocolVersion;
			PulsarApi.CommandConnect Connect = ConnectBuilder.build();
			ByteBuf Res = SerializeWithSize(PulsarApi.BaseCommand.newBuilder().setType(PulsarApi.BaseCommand.Type.CONNECT).setConnect(Connect));
			Connect.recycle();
			ConnectBuilder.recycle();
			return Res;
		}

		public static ByteBuf NewConnected(int ClientProtocoVersion)
		{
			return NewConnected(ClientProtocoVersion, InvalidMaxMessageSize);
		}

		public static ByteBuf NewConnected(int ClientProtocolVersion, int MaxMessageSize)
		{
			PulsarApi.CommandConnected.Builder ConnectedBuilder = PulsarApi.CommandConnected.newBuilder();
			ConnectedBuilder.setServerVersion("Pulsar Server");
			if (InvalidMaxMessageSize != MaxMessageSize)
			{
				ConnectedBuilder.MaxMessageSize = MaxMessageSize;
			}

			// If the broker supports a newer version of the protocol, it will anyway advertise the max version that the
			// client supports, to avoid confusing the client.
			int CurrentProtocolVersion = CurrentProtocolVersion;
			int VersionToAdvertise = Math.Min(CurrentProtocolVersion, ClientProtocolVersion);

			ConnectedBuilder.ProtocolVersion = VersionToAdvertise;

			PulsarApi.CommandConnected Connected = ConnectedBuilder.build();
			ByteBuf Res = SerializeWithSize(PulsarApi.BaseCommand.newBuilder().setType(PulsarApi.BaseCommand.Type.CONNECTED).setConnected(Connected));
			Connected.recycle();
			ConnectedBuilder.recycle();
			return Res;
		}

		public static ByteBuf NewAuthChallenge(string AuthMethod, AuthData BrokerData, int ClientProtocolVersion)
		{
			PulsarApi.CommandAuthChallenge.Builder ChallengeBuilder = PulsarApi.CommandAuthChallenge.newBuilder();

			// If the broker supports a newer version of the protocol, it will anyway advertise the max version that the
			// client supports, to avoid confusing the client.
			int CurrentProtocolVersion = CurrentProtocolVersion;
			int VersionToAdvertise = Math.Min(CurrentProtocolVersion, ClientProtocolVersion);

			ChallengeBuilder.ProtocolVersion = VersionToAdvertise;

			PulsarApi.CommandAuthChallenge Challenge = ChallengeBuilder.setChallenge(PulsarApi.AuthData.newBuilder().setAuthData(copyFrom(BrokerData.Bytes)).setAuthMethodName(AuthMethod).build()).build();

			ByteBuf Res = SerializeWithSize(PulsarApi.BaseCommand.newBuilder().setType(PulsarApi.BaseCommand.Type.AUTH_CHALLENGE).setAuthChallenge(Challenge));
			Challenge.recycle();
			ChallengeBuilder.recycle();
			return Res;
		}

		public static ByteBuf NewAuthResponse(string AuthMethod, AuthData ClientData, int ClientProtocolVersion, string ClientVersion)
		{
			PulsarApi.CommandAuthResponse.Builder ResponseBuilder = PulsarApi.CommandAuthResponse.newBuilder();

			ResponseBuilder.setClientVersion(!string.ReferenceEquals(ClientVersion, null) ? ClientVersion : "Pulsar Client");
			ResponseBuilder.ProtocolVersion = ClientProtocolVersion;

			PulsarApi.CommandAuthResponse Response = ResponseBuilder.setResponse(PulsarApi.AuthData.newBuilder().setAuthData(copyFrom(ClientData.Bytes)).setAuthMethodName(AuthMethod).build()).build();

			ByteBuf Res = SerializeWithSize(PulsarApi.BaseCommand.newBuilder().setType(PulsarApi.BaseCommand.Type.AUTH_RESPONSE).setAuthResponse(Response));
			Response.recycle();
			ResponseBuilder.recycle();
			return Res;
		}

		public static ByteBuf NewSuccess(long RequestId)
		{
			PulsarApi.CommandSuccess.Builder SuccessBuilder = PulsarApi.CommandSuccess.newBuilder();
			SuccessBuilder.RequestId = RequestId;
			PulsarApi.CommandSuccess Success = SuccessBuilder.build();
			ByteBuf Res = SerializeWithSize(PulsarApi.BaseCommand.newBuilder().setType(PulsarApi.BaseCommand.Type.SUCCESS).setSuccess(Success));
			SuccessBuilder.recycle();
			Success.recycle();
			return Res;
		}

		public static ByteBuf NewProducerSuccess(long RequestId, string ProducerName, SchemaVersion SchemaVersion)
		{
			return NewProducerSuccess(RequestId, ProducerName, -1, SchemaVersion);
		}

		public static ByteBuf NewProducerSuccess(long RequestId, string ProducerName, long LastSequenceId, SchemaVersion SchemaVersion)
		{
			PulsarApi.CommandProducerSuccess.Builder ProducerSuccessBuilder = PulsarApi.CommandProducerSuccess.newBuilder();
			ProducerSuccessBuilder.RequestId = RequestId;
			ProducerSuccessBuilder.setProducerName(ProducerName);
			ProducerSuccessBuilder.LastSequenceId = LastSequenceId;
			ProducerSuccessBuilder.SchemaVersion = ByteString.copyFrom(SchemaVersion.bytes());
			PulsarApi.CommandProducerSuccess ProducerSuccess = ProducerSuccessBuilder.build();
			ByteBuf Res = SerializeWithSize(PulsarApi.BaseCommand.newBuilder().setType(PulsarApi.BaseCommand.Type.PRODUCER_SUCCESS).setProducerSuccess(ProducerSuccess));
			ProducerSuccess.recycle();
			ProducerSuccessBuilder.recycle();
			return Res;
		}

		public static ByteBuf NewError(long RequestId, PulsarApi.ServerError Error, string Message)
		{
			PulsarApi.CommandError.Builder CmdErrorBuilder = PulsarApi.CommandError.newBuilder();
			CmdErrorBuilder.RequestId = RequestId;
			CmdErrorBuilder.Error = Error;
			CmdErrorBuilder.setMessage(Message);
			PulsarApi.CommandError CmdError = CmdErrorBuilder.build();
			ByteBuf Res = SerializeWithSize(PulsarApi.BaseCommand.newBuilder().setType(PulsarApi.BaseCommand.Type.ERROR).setError(CmdError));
			CmdError.recycle();
			CmdErrorBuilder.recycle();
			return Res;

		}

		public static ByteBuf NewSendReceipt(long ProducerId, long SequenceId, long HighestId, long LedgerId, long EntryId)
		{
			PulsarApi.CommandSendReceipt.Builder SendReceiptBuilder = PulsarApi.CommandSendReceipt.newBuilder();
			SendReceiptBuilder.ProducerId = ProducerId;
			SendReceiptBuilder.SequenceId = SequenceId;
			SendReceiptBuilder.HighestSequenceId = HighestId;
			PulsarApi.MessageIdData.Builder MessageIdBuilder = PulsarApi.MessageIdData.newBuilder();
			MessageIdBuilder.LedgerId = LedgerId;
			MessageIdBuilder.EntryId = EntryId;
			PulsarApi.MessageIdData MessageId = MessageIdBuilder.build();
			SendReceiptBuilder.setMessageId(MessageId);
			PulsarApi.CommandSendReceipt SendReceipt = SendReceiptBuilder.build();
			ByteBuf Res = SerializeWithSize(PulsarApi.BaseCommand.newBuilder().setType(PulsarApi.BaseCommand.Type.SEND_RECEIPT).setSendReceipt(SendReceipt));
			MessageIdBuilder.recycle();
			MessageId.recycle();
			SendReceiptBuilder.recycle();
			SendReceipt.recycle();
			return Res;
		}

		public static ByteBuf NewSendError(long ProducerId, long SequenceId, PulsarApi.ServerError Error, string ErrorMsg)
		{
			PulsarApi.CommandSendError.Builder SendErrorBuilder = PulsarApi.CommandSendError.newBuilder();
			SendErrorBuilder.ProducerId = ProducerId;
			SendErrorBuilder.SequenceId = SequenceId;
			SendErrorBuilder.Error = Error;
			SendErrorBuilder.setMessage(ErrorMsg);
			PulsarApi.CommandSendError SendError = SendErrorBuilder.build();
			ByteBuf Res = SerializeWithSize(PulsarApi.BaseCommand.newBuilder().setType(PulsarApi.BaseCommand.Type.SEND_ERROR).setSendError(SendError));
			SendErrorBuilder.recycle();
			SendError.recycle();
			return Res;
		}


		public static bool HasChecksum(ByteBuf Buffer)
		{
			return Buffer.getShort(Buffer.readerIndex()) == MagicCrc32c;
		}

		/// <summary>
		/// Read the checksum and advance the reader index in the buffer.
		/// 
		/// <para>Note: This method assume the checksum presence was already verified before.
		/// </para>
		/// </summary>
		public static int ReadChecksum(ByteBuf Buffer)
		{
			Buffer.skipBytes(2); //skip magic bytes
			return Buffer.readInt();
		}

		public static void SkipChecksumIfPresent(ByteBuf Buffer)
		{
			if (HasChecksum(Buffer))
			{
				ReadChecksum(Buffer);
			}
		}

		public static PulsarApi.MessageMetadata ParseMessageMetadata(ByteBuf Buffer)
		{
			try
			{
				// initially reader-index may point to start_of_checksum : increment reader-index to start_of_metadata
				// to parse metadata
				SkipChecksumIfPresent(Buffer);
				int MetadataSize = (int) Buffer.readUnsignedInt();

				int WriterIndex = Buffer.writerIndex();
				Buffer.writerIndex(Buffer.readerIndex() + MetadataSize);
				ByteBufCodedInputStream Stream = ByteBufCodedInputStream.get(Buffer);
				PulsarApi.MessageMetadata.Builder MessageMetadataBuilder = PulsarApi.MessageMetadata.newBuilder();
				PulsarApi.MessageMetadata Res = MessageMetadataBuilder.mergeFrom(Stream, null).build();
				Buffer.writerIndex(WriterIndex);
				MessageMetadataBuilder.recycle();
				Stream.recycle();
				return Res;
			}
			catch (IOException E)
			{
				throw new Exception(E);
			}
		}

		public static void SkipMessageMetadata(ByteBuf Buffer)
		{
			// initially reader-index may point to start_of_checksum : increment reader-index to start_of_metadata to parse
			// metadata
			SkipChecksumIfPresent(Buffer);
			int MetadataSize = (int) Buffer.readUnsignedInt();
			Buffer.skipBytes(MetadataSize);
		}

		public static ByteBufPair NewMessage(long ConsumerId, PulsarApi.MessageIdData MessageId, int RedeliveryCount, ByteBuf MetadataAndPayload)
		{
			PulsarApi.CommandMessage.Builder MsgBuilder = PulsarApi.CommandMessage.newBuilder();
			MsgBuilder.ConsumerId = ConsumerId;
			MsgBuilder.setMessageId(MessageId);
			if (RedeliveryCount > 0)
			{
				MsgBuilder.RedeliveryCount = RedeliveryCount;
			}
			PulsarApi.CommandMessage Msg = MsgBuilder.build();
			PulsarApi.BaseCommand.Builder CmdBuilder = PulsarApi.BaseCommand.newBuilder();
			PulsarApi.BaseCommand Cmd = CmdBuilder.setType(PulsarApi.BaseCommand.Type.MESSAGE).setMessage(Msg).build();

			ByteBufPair Res = SerializeCommandMessageWithSize(Cmd, MetadataAndPayload);
			Cmd.recycle();
			CmdBuilder.recycle();
			Msg.recycle();
			MsgBuilder.recycle();
			return Res;
		}

		public static ByteBufPair NewSend(long ProducerId, long SequenceId, int NumMessaegs, ChecksumType ChecksumType, PulsarApi.MessageMetadata MessageMetadata, ByteBuf Payload)
		{
			return NewSend(ProducerId, SequenceId, NumMessaegs, 0, 0, ChecksumType, MessageMetadata, Payload);
		}

		public static ByteBufPair NewSend(long ProducerId, long LowestSequenceId, long HighestSequenceId, int NumMessaegs, ChecksumType ChecksumType, PulsarApi.MessageMetadata MessageMetadata, ByteBuf Payload)
		{
			return NewSend(ProducerId, LowestSequenceId, HighestSequenceId, NumMessaegs, 0, 0, ChecksumType, MessageMetadata, Payload);
		}

		public static ByteBufPair NewSend(long ProducerId, long SequenceId, int NumMessages, long TxnIdLeastBits, long TxnIdMostBits, ChecksumType ChecksumType, PulsarApi.MessageMetadata MessageData, ByteBuf Payload)
		{
			PulsarApi.CommandSend.Builder SendBuilder = PulsarApi.CommandSend.newBuilder();
			SendBuilder.ProducerId = ProducerId;
			SendBuilder.SequenceId = SequenceId;
			if (NumMessages > 1)
			{
				SendBuilder.NumMessages = NumMessages;
			}
			if (TxnIdLeastBits > 0)
			{
				SendBuilder.TxnidLeastBits = TxnIdLeastBits;
			}
			if (TxnIdMostBits > 0)
			{
				SendBuilder.TxnidMostBits = TxnIdMostBits;
			}
			PulsarApi.CommandSend Send = SendBuilder.build();

			ByteBufPair Res = SerializeCommandSendWithSize(PulsarApi.BaseCommand.newBuilder().setType(PulsarApi.BaseCommand.Type.SEND).setSend(Send), ChecksumType, MessageData, Payload);
			Send.recycle();
			SendBuilder.recycle();
			return Res;
		}

		public static ByteBufPair NewSend(long ProducerId, long LowestSequenceId, long HighestSequenceId, int NumMessages, long TxnIdLeastBits, long TxnIdMostBits, ChecksumType ChecksumType, PulsarApi.MessageMetadata MessageData, ByteBuf Payload)
		{
			PulsarApi.CommandSend.Builder SendBuilder = PulsarApi.CommandSend.newBuilder();
			SendBuilder.ProducerId = ProducerId;
			SendBuilder.SequenceId = LowestSequenceId;
			SendBuilder.HighestSequenceId = HighestSequenceId;
			if (NumMessages > 1)
			{
				SendBuilder.NumMessages = NumMessages;
			}
			if (TxnIdLeastBits > 0)
			{
				SendBuilder.TxnidLeastBits = TxnIdLeastBits;
			}
			if (TxnIdMostBits > 0)
			{
				SendBuilder.TxnidMostBits = TxnIdMostBits;
			}
			PulsarApi.CommandSend Send = SendBuilder.build();

			ByteBufPair Res = SerializeCommandSendWithSize(PulsarApi.BaseCommand.newBuilder().setType(PulsarApi.BaseCommand.Type.SEND).setSend(Send), ChecksumType, MessageData, Payload);
			Send.recycle();
			SendBuilder.recycle();
			return Res;
		}

		public static ByteBuf NewSubscribe(string Topic, string Subscription, long ConsumerId, long RequestId, PulsarApi.CommandSubscribe.SubType SubType, int PriorityLevel, string ConsumerName, long ResetStartMessageBackInSeconds)
		{
			return NewSubscribe(Topic, Subscription, ConsumerId, RequestId, SubType, PriorityLevel, ConsumerName, true, null, Collections.emptyMap(), false, false, PulsarApi.CommandSubscribe.InitialPosition.Earliest, ResetStartMessageBackInSeconds, null, true);
		}

		public static ByteBuf NewSubscribe(string Topic, string Subscription, long ConsumerId, long RequestId, PulsarApi.CommandSubscribe.SubType SubType, int PriorityLevel, string ConsumerName, bool IsDurable, PulsarApi.MessageIdData StartMessageId, IDictionary<string, string> Metadata, bool ReadCompacted, bool IsReplicated, PulsarApi.CommandSubscribe.InitialPosition SubscriptionInitialPosition, long StartMessageRollbackDurationInSec, SchemaInfo SchemaInfo, bool CreateTopicIfDoesNotExist)
		{
					return NewSubscribe(Topic, Subscription, ConsumerId, RequestId, SubType, PriorityLevel, ConsumerName, IsDurable, StartMessageId, Metadata, ReadCompacted, IsReplicated, SubscriptionInitialPosition, StartMessageRollbackDurationInSec, SchemaInfo, CreateTopicIfDoesNotExist, null);
		}

		public static ByteBuf NewSubscribe(string Topic, string Subscription, long ConsumerId, long RequestId, PulsarApi.CommandSubscribe.SubType SubType, int PriorityLevel, string ConsumerName, bool IsDurable, PulsarApi.MessageIdData StartMessageId, IDictionary<string, string> Metadata, bool ReadCompacted, bool IsReplicated, PulsarApi.CommandSubscribe.InitialPosition SubscriptionInitialPosition, long StartMessageRollbackDurationInSec, SchemaInfo SchemaInfo, bool CreateTopicIfDoesNotExist, KeySharedPolicy KeySharedPolicy)
		{
			PulsarApi.CommandSubscribe.Builder SubscribeBuilder = PulsarApi.CommandSubscribe.newBuilder();
			SubscribeBuilder.setTopic(Topic);
			SubscribeBuilder.setSubscription(Subscription);
			SubscribeBuilder.SubType = SubType;
			SubscribeBuilder.ConsumerId = ConsumerId;
			SubscribeBuilder.setConsumerName(ConsumerName);
			SubscribeBuilder.RequestId = RequestId;
			SubscribeBuilder.PriorityLevel = PriorityLevel;
			SubscribeBuilder.Durable = IsDurable;
			SubscribeBuilder.ReadCompacted = ReadCompacted;
			SubscribeBuilder.InitialPosition = SubscriptionInitialPosition;
			SubscribeBuilder.ReplicateSubscriptionState = IsReplicated;
			SubscribeBuilder.ForceTopicCreation = CreateTopicIfDoesNotExist;

			if (KeySharedPolicy != null)
			{
				switch (KeySharedPolicy.KeySharedMode)
				{
					case AUTO_SPLIT:
						SubscribeBuilder.setKeySharedMeta(PulsarApi.KeySharedMeta.newBuilder().setKeySharedMode(PulsarApi.KeySharedMode.AUTO_SPLIT));
						break;
					case STICKY:
						PulsarApi.KeySharedMeta.Builder Builder = PulsarApi.KeySharedMeta.newBuilder().setKeySharedMode(PulsarApi.KeySharedMode.STICKY);
						IList<Range> Ranges = ((KeySharedPolicy.KeySharedPolicySticky) KeySharedPolicy).Ranges;
						foreach (Range Range in Ranges)
						{
							Builder.addHashRanges(PulsarApi.IntRange.newBuilder().setStart(Range.Start).setEnd(Range.End));
						}
						SubscribeBuilder.setKeySharedMeta(Builder);
						break;
				}
			}

			if (StartMessageId != null)
			{
				SubscribeBuilder.setStartMessageId(StartMessageId);
			}
			if (StartMessageRollbackDurationInSec > 0)
			{
				SubscribeBuilder.StartMessageRollbackDurationSec = StartMessageRollbackDurationInSec;
			}
			SubscribeBuilder.addAllMetadata(CommandUtils.ToKeyValueList(Metadata));

			PulsarApi.Schema Schema = null;
			if (SchemaInfo != null)
			{
				Schema = GetSchema(SchemaInfo);
				SubscribeBuilder.setSchema(Schema);
			}

			PulsarApi.CommandSubscribe Subscribe = SubscribeBuilder.build();
			ByteBuf Res = SerializeWithSize(PulsarApi.BaseCommand.newBuilder().setType(PulsarApi.BaseCommand.Type.SUBSCRIBE).setSubscribe(Subscribe));
			SubscribeBuilder.recycle();
			Subscribe.recycle();
			if (null != Schema)
			{
				Schema.recycle();
			}
			return Res;
		}

		public static ByteBuf NewUnsubscribe(long ConsumerId, long RequestId)
		{
			PulsarApi.CommandUnsubscribe.Builder UnsubscribeBuilder = PulsarApi.CommandUnsubscribe.newBuilder();
			UnsubscribeBuilder.ConsumerId = ConsumerId;
			UnsubscribeBuilder.RequestId = RequestId;
			PulsarApi.CommandUnsubscribe Unsubscribe = UnsubscribeBuilder.build();
			ByteBuf Res = SerializeWithSize(PulsarApi.BaseCommand.newBuilder().setType(PulsarApi.BaseCommand.Type.UNSUBSCRIBE).setUnsubscribe(Unsubscribe));
			UnsubscribeBuilder.recycle();
			Unsubscribe.recycle();
			return Res;
		}

		public static ByteBuf NewActiveConsumerChange(long ConsumerId, bool IsActive)
		{
			PulsarApi.CommandActiveConsumerChange.Builder ChangeBuilder = PulsarApi.CommandActiveConsumerChange.newBuilder().setConsumerId(ConsumerId).setIsActive(IsActive);

			PulsarApi.CommandActiveConsumerChange Change = ChangeBuilder.build();
			ByteBuf Res = SerializeWithSize(PulsarApi.BaseCommand.newBuilder().setType(PulsarApi.BaseCommand.Type.ACTIVE_CONSUMER_CHANGE).setActiveConsumerChange(Change));
			ChangeBuilder.recycle();
			Change.recycle();
			return Res;
		}

		public static ByteBuf NewSeek(long ConsumerId, long RequestId, long LedgerId, long EntryId)
		{
			PulsarApi.CommandSeek.Builder SeekBuilder = PulsarApi.CommandSeek.newBuilder();
			SeekBuilder.ConsumerId = ConsumerId;
			SeekBuilder.RequestId = RequestId;

			PulsarApi.MessageIdData.Builder MessageIdBuilder = PulsarApi.MessageIdData.newBuilder();
			MessageIdBuilder.LedgerId = LedgerId;
			MessageIdBuilder.EntryId = EntryId;
			PulsarApi.MessageIdData MessageId = MessageIdBuilder.build();
			SeekBuilder.setMessageId(MessageId);

			PulsarApi.CommandSeek Seek = SeekBuilder.build();
			ByteBuf Res = SerializeWithSize(PulsarApi.BaseCommand.newBuilder().setType(PulsarApi.BaseCommand.Type.SEEK).setSeek(Seek));
			MessageId.recycle();
			MessageIdBuilder.recycle();
			SeekBuilder.recycle();
			Seek.recycle();
			return Res;
		}

		public static ByteBuf NewSeek(long ConsumerId, long RequestId, long Timestamp)
		{
			PulsarApi.CommandSeek.Builder SeekBuilder = PulsarApi.CommandSeek.newBuilder();
			SeekBuilder.ConsumerId = ConsumerId;
			SeekBuilder.RequestId = RequestId;

			SeekBuilder.MessagePublishTime = Timestamp;

			PulsarApi.CommandSeek Seek = SeekBuilder.build();
			ByteBuf Res = SerializeWithSize(PulsarApi.BaseCommand.newBuilder().setType(PulsarApi.BaseCommand.Type.SEEK).setSeek(Seek));

			SeekBuilder.recycle();
			Seek.recycle();
			return Res;
		}

		public static ByteBuf NewCloseConsumer(long ConsumerId, long RequestId)
		{
			PulsarApi.CommandCloseConsumer.Builder CloseConsumerBuilder = PulsarApi.CommandCloseConsumer.newBuilder();
			CloseConsumerBuilder.ConsumerId = ConsumerId;
			CloseConsumerBuilder.RequestId = RequestId;
			PulsarApi.CommandCloseConsumer CloseConsumer = CloseConsumerBuilder.build();
			ByteBuf Res = SerializeWithSize(PulsarApi.BaseCommand.newBuilder().setType(PulsarApi.BaseCommand.Type.CLOSE_CONSUMER).setCloseConsumer(CloseConsumer));
			CloseConsumerBuilder.recycle();
			CloseConsumer.recycle();
			return Res;
		}

		public static ByteBuf NewReachedEndOfTopic(long ConsumerId)
		{
			PulsarApi.CommandReachedEndOfTopic.Builder ReachedEndOfTopicBuilder = PulsarApi.CommandReachedEndOfTopic.newBuilder();
			ReachedEndOfTopicBuilder.ConsumerId = ConsumerId;
			PulsarApi.CommandReachedEndOfTopic ReachedEndOfTopic = ReachedEndOfTopicBuilder.build();
			ByteBuf Res = SerializeWithSize(PulsarApi.BaseCommand.newBuilder().setType(PulsarApi.BaseCommand.Type.REACHED_END_OF_TOPIC).setReachedEndOfTopic(ReachedEndOfTopic));
			ReachedEndOfTopicBuilder.recycle();
			ReachedEndOfTopic.recycle();
			return Res;
		}

		public static ByteBuf NewCloseProducer(long ProducerId, long RequestId)
		{
			PulsarApi.CommandCloseProducer.Builder CloseProducerBuilder = PulsarApi.CommandCloseProducer.newBuilder();
			CloseProducerBuilder.ProducerId = ProducerId;
			CloseProducerBuilder.RequestId = RequestId;
			PulsarApi.CommandCloseProducer CloseProducer = CloseProducerBuilder.build();
			ByteBuf Res = SerializeWithSize(PulsarApi.BaseCommand.newBuilder().setType(PulsarApi.BaseCommand.Type.CLOSE_PRODUCER).setCloseProducer(CloseProducerBuilder));
			CloseProducerBuilder.recycle();
			CloseProducer.recycle();
			return Res;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @VisibleForTesting public static io.netty.buffer.ByteBuf newProducer(String topic, long producerId, long requestId, String producerName, java.util.Map<String, String> metadata)
		public static ByteBuf NewProducer(string Topic, long ProducerId, long RequestId, string ProducerName, IDictionary<string, string> Metadata)
		{
			return NewProducer(Topic, ProducerId, RequestId, ProducerName, false, Metadata);
		}

		public static ByteBuf NewProducer(string Topic, long ProducerId, long RequestId, string ProducerName, bool Encrypted, IDictionary<string, string> Metadata)
		{
			return NewProducer(Topic, ProducerId, RequestId, ProducerName, Encrypted, Metadata, null, 0, false);
		}

		private static PulsarApi.Schema.Type GetSchemaType(SchemaType Type)
		{
			if (Type.Value < 0)
			{
				return PulsarApi.Schema.Type.None;
			}
			else
			{
				return PulsarApi.Schema.Type.valueOf(Type.Value);
			}
		}

		public static SchemaType GetSchemaType(PulsarApi.Schema.Type Type)
		{
			if (Type.Number < 0)
			{
				// this is unexpected
				return SchemaType.NONE;
			}
			else
			{
				return SchemaType.valueOf(Type.Number);
			}
		}

		private static PulsarApi.Schema GetSchema(SchemaInfo SchemaInfo)
		{
			PulsarApi.Schema.Builder Builder = PulsarApi.Schema.newBuilder().setName(SchemaInfo.Name).setSchemaData(copyFrom(SchemaInfo.Schema)).setType(GetSchemaType(SchemaInfo.Type)).addAllProperties(SchemaInfo.Properties.entrySet().Select(entry => PulsarApi.KeyValue.newBuilder().setKey(entry.Key).setValue(entry.Value).build()).ToList());
			PulsarApi.Schema Schema = Builder.build();
			Builder.recycle();
			return Schema;
		}

		public static ByteBuf NewProducer(string Topic, long ProducerId, long RequestId, string ProducerName, bool Encrypted, IDictionary<string, string> Metadata, SchemaInfo SchemaInfo, long Epoch, bool UserProvidedProducerName)
		{
			PulsarApi.CommandProducer.Builder ProducerBuilder = PulsarApi.CommandProducer.newBuilder();
			ProducerBuilder.setTopic(Topic);
			ProducerBuilder.ProducerId = ProducerId;
			ProducerBuilder.RequestId = RequestId;
			ProducerBuilder.Epoch = Epoch;
			if (!string.ReferenceEquals(ProducerName, null))
			{
				ProducerBuilder.setProducerName(ProducerName);
			}
			ProducerBuilder.UserProvidedProducerName = UserProvidedProducerName;
			ProducerBuilder.Encrypted = Encrypted;

			ProducerBuilder.addAllMetadata(CommandUtils.ToKeyValueList(Metadata));

			if (null != SchemaInfo)
			{
				ProducerBuilder.setSchema(GetSchema(SchemaInfo));
			}

			PulsarApi.CommandProducer Producer = ProducerBuilder.build();
			ByteBuf Res = SerializeWithSize(PulsarApi.BaseCommand.newBuilder().setType(PulsarApi.BaseCommand.Type.PRODUCER).setProducer(Producer));
			ProducerBuilder.recycle();
			Producer.recycle();
			return Res;
		}

		public static ByteBuf NewPartitionMetadataResponse(PulsarApi.ServerError Error, string ErrorMsg, long RequestId)
		{
			PulsarApi.CommandPartitionedTopicMetadataResponse.Builder PartitionMetadataResponseBuilder = PulsarApi.CommandPartitionedTopicMetadataResponse.newBuilder();
			PartitionMetadataResponseBuilder.RequestId = RequestId;
			PartitionMetadataResponseBuilder.Error = Error;
			PartitionMetadataResponseBuilder.Response = PulsarApi.CommandPartitionedTopicMetadataResponse.LookupType.Failed;
			if (!string.ReferenceEquals(ErrorMsg, null))
			{
				PartitionMetadataResponseBuilder.setMessage(ErrorMsg);
			}

			PulsarApi.CommandPartitionedTopicMetadataResponse PartitionMetadataResponse = PartitionMetadataResponseBuilder.build();
			ByteBuf Res = SerializeWithSize(PulsarApi.BaseCommand.newBuilder().setType(PulsarApi.BaseCommand.Type.PARTITIONED_METADATA_RESPONSE).setPartitionMetadataResponse(PartitionMetadataResponse));
			PartitionMetadataResponseBuilder.recycle();
			PartitionMetadataResponse.recycle();
			return Res;
		}

		public static ByteBuf NewPartitionMetadataRequest(string Topic, long RequestId)
		{
			PulsarApi.CommandPartitionedTopicMetadata.Builder PartitionMetadataBuilder = PulsarApi.CommandPartitionedTopicMetadata.newBuilder();
			PartitionMetadataBuilder.setTopic(Topic);
			PartitionMetadataBuilder.RequestId = RequestId;
			PulsarApi.CommandPartitionedTopicMetadata PartitionMetadata = PartitionMetadataBuilder.build();
			ByteBuf Res = SerializeWithSize(PulsarApi.BaseCommand.newBuilder().setType(PulsarApi.BaseCommand.Type.PARTITIONED_METADATA).setPartitionMetadata(PartitionMetadata));
			PartitionMetadataBuilder.recycle();
			PartitionMetadata.recycle();
			return Res;
		}

		public static ByteBuf NewPartitionMetadataResponse(int Partitions, long RequestId)
		{
			PulsarApi.CommandPartitionedTopicMetadataResponse.Builder PartitionMetadataResponseBuilder = PulsarApi.CommandPartitionedTopicMetadataResponse.newBuilder();
			PartitionMetadataResponseBuilder.Partitions = Partitions;
			PartitionMetadataResponseBuilder.Response = PulsarApi.CommandPartitionedTopicMetadataResponse.LookupType.Success;
			PartitionMetadataResponseBuilder.RequestId = RequestId;

			PulsarApi.CommandPartitionedTopicMetadataResponse PartitionMetadataResponse = PartitionMetadataResponseBuilder.build();
			ByteBuf Res = SerializeWithSize(PulsarApi.BaseCommand.newBuilder().setType(PulsarApi.BaseCommand.Type.PARTITIONED_METADATA_RESPONSE).setPartitionMetadataResponse(PartitionMetadataResponse));
			PartitionMetadataResponseBuilder.recycle();
			PartitionMetadataResponse.recycle();
			return Res;
		}

		public static ByteBuf NewLookup(string Topic, bool Authoritative, long RequestId)
		{
			PulsarApi.CommandLookupTopic.Builder LookupTopicBuilder = PulsarApi.CommandLookupTopic.newBuilder();
			LookupTopicBuilder.setTopic(Topic);
			LookupTopicBuilder.RequestId = RequestId;
			LookupTopicBuilder.Authoritative = Authoritative;
			PulsarApi.CommandLookupTopic LookupBroker = LookupTopicBuilder.build();
			ByteBuf Res = SerializeWithSize(PulsarApi.BaseCommand.newBuilder().setType(PulsarApi.BaseCommand.Type.LOOKUP).setLookupTopic(LookupBroker));
			LookupTopicBuilder.recycle();
			LookupBroker.recycle();
			return Res;
		}

		public static ByteBuf NewLookupResponse(string BrokerServiceUrl, string BrokerServiceUrlTls, bool Authoritative, PulsarApi.CommandLookupTopicResponse.LookupType Response, long RequestId, bool ProxyThroughServiceUrl)
		{
			PulsarApi.CommandLookupTopicResponse.Builder CommandLookupTopicResponseBuilder = PulsarApi.CommandLookupTopicResponse.newBuilder();
			CommandLookupTopicResponseBuilder.setBrokerServiceUrl(BrokerServiceUrl);
			if (!string.ReferenceEquals(BrokerServiceUrlTls, null))
			{
				CommandLookupTopicResponseBuilder.setBrokerServiceUrlTls(BrokerServiceUrlTls);
			}
			CommandLookupTopicResponseBuilder.Response = Response;
			CommandLookupTopicResponseBuilder.RequestId = RequestId;
			CommandLookupTopicResponseBuilder.Authoritative = Authoritative;
			CommandLookupTopicResponseBuilder.ProxyThroughServiceUrl = ProxyThroughServiceUrl;

			PulsarApi.CommandLookupTopicResponse CommandLookupTopicResponse = CommandLookupTopicResponseBuilder.build();
			ByteBuf Res = SerializeWithSize(PulsarApi.BaseCommand.newBuilder().setType(PulsarApi.BaseCommand.Type.LOOKUP_RESPONSE).setLookupTopicResponse(CommandLookupTopicResponse));
			CommandLookupTopicResponseBuilder.recycle();
			CommandLookupTopicResponse.recycle();
			return Res;
		}

		public static ByteBuf NewLookupErrorResponse(PulsarApi.ServerError Error, string ErrorMsg, long RequestId)
		{
			PulsarApi.CommandLookupTopicResponse.Builder ConnectionBuilder = PulsarApi.CommandLookupTopicResponse.newBuilder();
			ConnectionBuilder.RequestId = RequestId;
			ConnectionBuilder.Error = Error;
			if (!string.ReferenceEquals(ErrorMsg, null))
			{
				ConnectionBuilder.setMessage(ErrorMsg);
			}
			ConnectionBuilder.Response = PulsarApi.CommandLookupTopicResponse.LookupType.Failed;

			PulsarApi.CommandLookupTopicResponse ConnectionBroker = ConnectionBuilder.build();
			ByteBuf Res = SerializeWithSize(PulsarApi.BaseCommand.newBuilder().setType(PulsarApi.BaseCommand.Type.LOOKUP_RESPONSE).setLookupTopicResponse(ConnectionBroker));
			ConnectionBuilder.recycle();
			ConnectionBroker.recycle();
			return Res;
		}

		public static ByteBuf NewMultiMessageAck(long ConsumerId, IList<Pair<long, long>> Entries)
		{
			PulsarApi.CommandAck.Builder AckBuilder = PulsarApi.CommandAck.newBuilder();
			AckBuilder.ConsumerId = ConsumerId;
			AckBuilder.AckType = PulsarApi.CommandAck.AckType.Individual;

			int EntriesCount = Entries.Count;
			for (int I = 0; I < EntriesCount; I++)
			{
				long LedgerId = Entries[I].Left;
				long EntryId = Entries[I].Right;

				PulsarApi.MessageIdData.Builder MessageIdDataBuilder = PulsarApi.MessageIdData.newBuilder();
				MessageIdDataBuilder.LedgerId = LedgerId;
				MessageIdDataBuilder.EntryId = EntryId;
				PulsarApi.MessageIdData MessageIdData = MessageIdDataBuilder.build();
				AckBuilder.addMessageId(MessageIdData);

				MessageIdDataBuilder.recycle();
			}

			PulsarApi.CommandAck Ack = AckBuilder.build();

			ByteBuf Res = SerializeWithSize(PulsarApi.BaseCommand.newBuilder().setType(PulsarApi.BaseCommand.Type.ACK).setAck(Ack));

			for (int I = 0; I < EntriesCount; I++)
			{
				Ack.getMessageId(I).recycle();
			}
			Ack.recycle();
			AckBuilder.recycle();
			return Res;
		}

		public static ByteBuf NewAck(long ConsumerId, long LedgerId, long EntryId, PulsarApi.CommandAck.AckType AckType, PulsarApi.CommandAck.ValidationError ValidationError, IDictionary<string, long> Properties)
		{
			return NewAck(ConsumerId, LedgerId, EntryId, AckType, ValidationError, Properties, 0, 0);
		}

		public static ByteBuf NewAck(long ConsumerId, long LedgerId, long EntryId, PulsarApi.CommandAck.AckType AckType, PulsarApi.CommandAck.ValidationError ValidationError, IDictionary<string, long> Properties, long TxnIdLeastBits, long TxnIdMostBits)
		{
			PulsarApi.CommandAck.Builder AckBuilder = PulsarApi.CommandAck.newBuilder();
			AckBuilder.ConsumerId = ConsumerId;
			AckBuilder.AckType = AckType;
			PulsarApi.MessageIdData.Builder MessageIdDataBuilder = PulsarApi.MessageIdData.newBuilder();
			MessageIdDataBuilder.LedgerId = LedgerId;
			MessageIdDataBuilder.EntryId = EntryId;
			PulsarApi.MessageIdData MessageIdData = MessageIdDataBuilder.build();
			AckBuilder.addMessageId(MessageIdData);
			if (ValidationError != null)
			{
				AckBuilder.ValidationError = ValidationError;
			}
			if (TxnIdMostBits > 0)
			{
				AckBuilder.TxnidMostBits = TxnIdMostBits;
			}
			if (TxnIdLeastBits > 0)
			{
				AckBuilder.TxnidLeastBits = TxnIdLeastBits;
			}
			foreach (KeyValuePair<string, long> E in Properties.SetOfKeyValuePairs())
			{
				AckBuilder.addProperties(PulsarApi.KeyLongValue.newBuilder().setKey(E.Key).setValue(E.Value).build());
			}
			PulsarApi.CommandAck Ack = AckBuilder.build();

			ByteBuf Res = SerializeWithSize(PulsarApi.BaseCommand.newBuilder().setType(PulsarApi.BaseCommand.Type.ACK).setAck(Ack));
			Ack.recycle();
			AckBuilder.recycle();
			MessageIdDataBuilder.recycle();
			MessageIdData.recycle();
			return Res;
		}

		public static ByteBuf NewAckResponse(long ConsumerId, long TxnIdLeastBits, long TxnIdMostBits)
		{
			PulsarApi.CommandAckResponse.Builder CommandAckResponseBuilder = PulsarApi.CommandAckResponse.newBuilder();
			CommandAckResponseBuilder.ConsumerId = ConsumerId;
			CommandAckResponseBuilder.TxnidLeastBits = TxnIdLeastBits;
			CommandAckResponseBuilder.TxnidMostBits = TxnIdMostBits;
			PulsarApi.CommandAckResponse CommandAckResponse = CommandAckResponseBuilder.build();

			ByteBuf Res = SerializeWithSize(PulsarApi.BaseCommand.newBuilder().setType(PulsarApi.BaseCommand.Type.ACK_RESPONSE).setAckResponse(CommandAckResponse));
			CommandAckResponseBuilder.recycle();
			CommandAckResponse.recycle();

			return Res;
		}

		public static ByteBuf NewAckErrorResponse(PulsarApi.ServerError Error, string ErrorMsg, long ConsumerId)
		{
			PulsarApi.CommandAckResponse.Builder AckErrorBuilder = PulsarApi.CommandAckResponse.newBuilder();
			AckErrorBuilder.ConsumerId = ConsumerId;
			AckErrorBuilder.Error = Error;
			if (!string.ReferenceEquals(ErrorMsg, null))
			{
				AckErrorBuilder.setMessage(ErrorMsg);
			}

			PulsarApi.CommandAckResponse Response = AckErrorBuilder.build();
			ByteBuf Res = SerializeWithSize(PulsarApi.BaseCommand.newBuilder().setType(PulsarApi.BaseCommand.Type.ACK_RESPONSE).setAckResponse(Response));

			AckErrorBuilder.recycle();
			Response.recycle();

			return Res;
		}

		public static ByteBuf NewFlow(long ConsumerId, int MessagePermits)
		{
			PulsarApi.CommandFlow.Builder FlowBuilder = PulsarApi.CommandFlow.newBuilder();
			FlowBuilder.ConsumerId = ConsumerId;
			FlowBuilder.MessagePermits = MessagePermits;
			PulsarApi.CommandFlow Flow = FlowBuilder.build();

			ByteBuf Res = SerializeWithSize(PulsarApi.BaseCommand.newBuilder().setType(PulsarApi.BaseCommand.Type.FLOW).setFlow(FlowBuilder));
			Flow.recycle();
			FlowBuilder.recycle();
			return Res;
		}

		public static ByteBuf NewRedeliverUnacknowledgedMessages(long ConsumerId)
		{
			PulsarApi.CommandRedeliverUnacknowledgedMessages.Builder RedeliverBuilder = PulsarApi.CommandRedeliverUnacknowledgedMessages.newBuilder();
			RedeliverBuilder.ConsumerId = ConsumerId;
			PulsarApi.CommandRedeliverUnacknowledgedMessages Redeliver = RedeliverBuilder.build();
			ByteBuf Res = SerializeWithSize(PulsarApi.BaseCommand.newBuilder().setType(PulsarApi.BaseCommand.Type.REDELIVER_UNACKNOWLEDGED_MESSAGES).setRedeliverUnacknowledgedMessages(RedeliverBuilder));
			Redeliver.recycle();
			RedeliverBuilder.recycle();
			return Res;
		}

		public static ByteBuf NewRedeliverUnacknowledgedMessages(long ConsumerId, IList<PulsarApi.MessageIdData> MessageIds)
		{
			PulsarApi.CommandRedeliverUnacknowledgedMessages.Builder RedeliverBuilder = PulsarApi.CommandRedeliverUnacknowledgedMessages.newBuilder();
			RedeliverBuilder.ConsumerId = ConsumerId;
			RedeliverBuilder.addAllMessageIds(MessageIds);
			PulsarApi.CommandRedeliverUnacknowledgedMessages Redeliver = RedeliverBuilder.build();
			ByteBuf Res = SerializeWithSize(PulsarApi.BaseCommand.newBuilder().setType(PulsarApi.BaseCommand.Type.REDELIVER_UNACKNOWLEDGED_MESSAGES).setRedeliverUnacknowledgedMessages(RedeliverBuilder));
			Redeliver.recycle();
			RedeliverBuilder.recycle();
			return Res;
		}

		public static ByteBuf NewConsumerStatsResponse(PulsarApi.ServerError ServerError, string ErrMsg, long RequestId)
		{
			PulsarApi.CommandConsumerStatsResponse.Builder CommandConsumerStatsResponseBuilder = PulsarApi.CommandConsumerStatsResponse.newBuilder();
			CommandConsumerStatsResponseBuilder.RequestId = RequestId;
			CommandConsumerStatsResponseBuilder.setErrorMessage(ErrMsg);
			CommandConsumerStatsResponseBuilder.ErrorCode = ServerError;

			PulsarApi.CommandConsumerStatsResponse CommandConsumerStatsResponse = CommandConsumerStatsResponseBuilder.build();
			ByteBuf Res = SerializeWithSize(PulsarApi.BaseCommand.newBuilder().setType(PulsarApi.BaseCommand.Type.CONSUMER_STATS_RESPONSE).setConsumerStatsResponse(CommandConsumerStatsResponseBuilder));
			CommandConsumerStatsResponse.recycle();
			CommandConsumerStatsResponseBuilder.recycle();
			return Res;
		}

		public static ByteBuf NewConsumerStatsResponse(PulsarApi.CommandConsumerStatsResponse.Builder Builder)
		{
			PulsarApi.CommandConsumerStatsResponse CommandConsumerStatsResponse = Builder.build();
			ByteBuf Res = SerializeWithSize(PulsarApi.BaseCommand.newBuilder().setType(PulsarApi.BaseCommand.Type.CONSUMER_STATS_RESPONSE).setConsumerStatsResponse(Builder));
			CommandConsumerStatsResponse.recycle();
			Builder.recycle();
			return Res;
		}

		public static ByteBuf NewGetTopicsOfNamespaceRequest(string Namespace, long RequestId, PulsarApi.CommandGetTopicsOfNamespace.Mode Mode)
		{
			PulsarApi.CommandGetTopicsOfNamespace.Builder TopicsBuilder = PulsarApi.CommandGetTopicsOfNamespace.newBuilder();
			TopicsBuilder.setNamespace(Namespace).setRequestId(RequestId).setMode(Mode);

			PulsarApi.CommandGetTopicsOfNamespace TopicsCommand = TopicsBuilder.build();
			ByteBuf Res = SerializeWithSize(PulsarApi.BaseCommand.newBuilder().setType(PulsarApi.BaseCommand.Type.GET_TOPICS_OF_NAMESPACE).setGetTopicsOfNamespace(TopicsCommand));
			TopicsBuilder.recycle();
			TopicsCommand.recycle();
			return Res;
		}

		public static ByteBuf NewGetTopicsOfNamespaceResponse(IList<string> Topics, long RequestId)
		{
			PulsarApi.CommandGetTopicsOfNamespaceResponse.Builder TopicsResponseBuilder = PulsarApi.CommandGetTopicsOfNamespaceResponse.newBuilder();

			TopicsResponseBuilder.setRequestId(RequestId).addAllTopics(Topics);

			PulsarApi.CommandGetTopicsOfNamespaceResponse TopicsOfNamespaceResponse = TopicsResponseBuilder.build();
			ByteBuf Res = SerializeWithSize(PulsarApi.BaseCommand.newBuilder().setType(PulsarApi.BaseCommand.Type.GET_TOPICS_OF_NAMESPACE_RESPONSE).setGetTopicsOfNamespaceResponse(TopicsOfNamespaceResponse));

			TopicsResponseBuilder.recycle();
			TopicsOfNamespaceResponse.recycle();
			return Res;
		}

		private static readonly ByteBuf cmdPing;

		static Commands()
		{
			ByteBuf SerializedCmdPing = SerializeWithSize(PulsarApi.BaseCommand.newBuilder().setType(PulsarApi.BaseCommand.Type.PING).setPing(PulsarApi.CommandPing.DefaultInstance));
			cmdPing = Unpooled.copiedBuffer(SerializedCmdPing);
			SerializedCmdPing.release();
			ByteBuf SerializedCmdPong = SerializeWithSize(PulsarApi.BaseCommand.newBuilder().setType(PulsarApi.BaseCommand.Type.PONG).setPong(PulsarApi.CommandPong.DefaultInstance));
			cmdPong = Unpooled.copiedBuffer(SerializedCmdPong);
			SerializedCmdPong.release();
		}

		internal static ByteBuf NewPing()
		{
			return cmdPing.retainedDuplicate();
		}

		private static readonly ByteBuf cmdPong;


		internal static ByteBuf NewPong()
		{
			return cmdPong.retainedDuplicate();
		}

		public static ByteBuf NewGetLastMessageId(long ConsumerId, long RequestId)
		{
			PulsarApi.CommandGetLastMessageId.Builder CmdBuilder = PulsarApi.CommandGetLastMessageId.newBuilder();
			CmdBuilder.setConsumerId(ConsumerId).setRequestId(RequestId);

			ByteBuf Res = SerializeWithSize(PulsarApi.BaseCommand.newBuilder().setType(PulsarApi.BaseCommand.Type.GET_LAST_MESSAGE_ID).setGetLastMessageId(CmdBuilder.build()));
			CmdBuilder.recycle();
			return Res;
		}

		public static ByteBuf NewGetLastMessageIdResponse(long RequestId, PulsarApi.MessageIdData MessageIdData)
		{
			PulsarApi.CommandGetLastMessageIdResponse.Builder Response = PulsarApi.CommandGetLastMessageIdResponse.newBuilder().setLastMessageId(MessageIdData).setRequestId(RequestId);

			ByteBuf Res = SerializeWithSize(PulsarApi.BaseCommand.newBuilder().setType(PulsarApi.BaseCommand.Type.GET_LAST_MESSAGE_ID_RESPONSE).setGetLastMessageIdResponse(Response.build()));
			Response.recycle();
			return Res;
		}

		public static ByteBuf NewGetSchema(long RequestId, string Topic, Optional<SchemaVersion> Version)
		{
			PulsarApi.CommandGetSchema.Builder Schema = PulsarApi.CommandGetSchema.newBuilder().setRequestId(RequestId);
			Schema.setTopic(Topic);
			if (Version.Present)
			{
				Schema.SchemaVersion = ByteString.copyFrom(Version.get().bytes());
			}

			PulsarApi.CommandGetSchema GetSchema = Schema.build();

			ByteBuf Res = SerializeWithSize(PulsarApi.BaseCommand.newBuilder().setType(PulsarApi.BaseCommand.Type.GET_SCHEMA).setGetSchema(GetSchema));
			Schema.recycle();
			return Res;
		}

		public static ByteBuf NewGetSchemaResponse(long RequestId, PulsarApi.CommandGetSchemaResponse Response)
		{
			PulsarApi.CommandGetSchemaResponse.Builder SchemaResponseBuilder = PulsarApi.CommandGetSchemaResponse.newBuilder(Response).setRequestId(RequestId);

			ByteBuf Res = SerializeWithSize(PulsarApi.BaseCommand.newBuilder().setType(PulsarApi.BaseCommand.Type.GET_SCHEMA_RESPONSE).setGetSchemaResponse(SchemaResponseBuilder.build()));
			SchemaResponseBuilder.recycle();
			return Res;
		}

		public static ByteBuf NewGetSchemaResponse(long RequestId, SchemaInfo Schema, SchemaVersion Version)
		{
			PulsarApi.CommandGetSchemaResponse.Builder SchemaResponse = PulsarApi.CommandGetSchemaResponse.newBuilder().setRequestId(RequestId).setSchemaVersion(ByteString.copyFrom(Version.bytes())).setSchema(GetSchema(Schema));

			ByteBuf Res = SerializeWithSize(PulsarApi.BaseCommand.newBuilder().setType(PulsarApi.BaseCommand.Type.GET_SCHEMA_RESPONSE).setGetSchemaResponse(SchemaResponse.build()));
			SchemaResponse.recycle();
			return Res;
		}

		public static ByteBuf NewGetSchemaResponseError(long RequestId, PulsarApi.ServerError Error, string ErrorMessage)
		{
			PulsarApi.CommandGetSchemaResponse.Builder SchemaResponse = PulsarApi.CommandGetSchemaResponse.newBuilder().setRequestId(RequestId).setErrorCode(Error).setErrorMessage(ErrorMessage);

			ByteBuf Res = SerializeWithSize(PulsarApi.BaseCommand.newBuilder().setType(PulsarApi.BaseCommand.Type.GET_SCHEMA_RESPONSE).setGetSchemaResponse(SchemaResponse.build()));
			SchemaResponse.recycle();
			return Res;
		}

		public static ByteBuf NewGetOrCreateSchema(long RequestId, string Topic, SchemaInfo SchemaInfo)
		{
			PulsarApi.CommandGetOrCreateSchema GetOrCreateSchema = PulsarApi.CommandGetOrCreateSchema.newBuilder().setRequestId(RequestId).setTopic(Topic).setSchema(GetSchema(SchemaInfo)).build();
			ByteBuf Res = SerializeWithSize(PulsarApi.BaseCommand.newBuilder().setType(PulsarApi.BaseCommand.Type.GET_OR_CREATE_SCHEMA).setGetOrCreateSchema(GetOrCreateSchema));
			GetOrCreateSchema.recycle();
			return Res;
		}

		public static ByteBuf NewGetOrCreateSchemaResponse(long RequestId, SchemaVersion SchemaVersion)
		{
			PulsarApi.CommandGetOrCreateSchemaResponse.Builder SchemaResponse = PulsarApi.CommandGetOrCreateSchemaResponse.newBuilder().setRequestId(RequestId).setSchemaVersion(ByteString.copyFrom(SchemaVersion.bytes()));
			ByteBuf Res = SerializeWithSize(PulsarApi.BaseCommand.newBuilder().setType(PulsarApi.BaseCommand.Type.GET_OR_CREATE_SCHEMA_RESPONSE).setGetOrCreateSchemaResponse(SchemaResponse.build()));
			SchemaResponse.recycle();
			return Res;
		}

		public static ByteBuf NewGetOrCreateSchemaResponseError(long RequestId, PulsarApi.ServerError Error, string ErrorMessage)
		{
			PulsarApi.CommandGetOrCreateSchemaResponse.Builder SchemaResponse = PulsarApi.CommandGetOrCreateSchemaResponse.newBuilder().setRequestId(RequestId).setErrorCode(Error).setErrorMessage(ErrorMessage);
			ByteBuf Res = SerializeWithSize(PulsarApi.BaseCommand.newBuilder().setType(PulsarApi.BaseCommand.Type.GET_OR_CREATE_SCHEMA_RESPONSE).setGetOrCreateSchemaResponse(SchemaResponse.build()));
			SchemaResponse.recycle();
			return Res;
		}

		// ---- transaction related ----

		public static ByteBuf NewTxn(long TcId, long RequestId, long TtlSeconds)
		{
			PulsarApi.CommandNewTxn CommandNewTxn = PulsarApi.CommandNewTxn.newBuilder().setTcId(TcId).setRequestId(RequestId).setTxnTtlSeconds(TtlSeconds).build();
			ByteBuf Res = SerializeWithSize(PulsarApi.BaseCommand.newBuilder().setType(PulsarApi.BaseCommand.Type.NEW_TXN).setNewTxn(CommandNewTxn));
			CommandNewTxn.recycle();
			return Res;
		}

		public static ByteBuf NewTxnResponse(long RequestId, long TxnIdLeastBits, long TxnIdMostBits)
		{
			PulsarApi.CommandNewTxnResponse CommandNewTxnResponse = PulsarApi.CommandNewTxnResponse.newBuilder().setRequestId(RequestId).setTxnidMostBits(TxnIdMostBits).setTxnidLeastBits(TxnIdLeastBits).build();
			ByteBuf Res = SerializeWithSize(PulsarApi.BaseCommand.newBuilder().setType(PulsarApi.BaseCommand.Type.NEW_TXN_RESPONSE).setNewTxnResponse(CommandNewTxnResponse));
			CommandNewTxnResponse.recycle();

			return Res;
		}

		public static ByteBuf NewTxnResponse(long RequestId, long TxnIdMostBits, PulsarApi.ServerError Error, string ErrorMsg)
		{
			PulsarApi.CommandNewTxnResponse.Builder Builder = PulsarApi.CommandNewTxnResponse.newBuilder();
			Builder.RequestId = RequestId;
			Builder.TxnidMostBits = TxnIdMostBits;
			Builder.Error = Error;
			if (!string.ReferenceEquals(ErrorMsg, null))
			{
				Builder.setMessage(ErrorMsg);
			}
			PulsarApi.CommandNewTxnResponse ErrorResponse = Builder.build();
			ByteBuf Res = SerializeWithSize(PulsarApi.BaseCommand.newBuilder().setType(PulsarApi.BaseCommand.Type.NEW_TXN_RESPONSE).setNewTxnResponse(ErrorResponse));
			Builder.recycle();
			ErrorResponse.recycle();

			return Res;
		}

		public static ByteBuf NewAddPartitionToTxn(long RequestId, long TxnIdLeastBits, long TxnIdMostBits)
		{
			PulsarApi.CommandAddPartitionToTxn CommandAddPartitionToTxn = PulsarApi.CommandAddPartitionToTxn.newBuilder().setRequestId(RequestId).setTxnidLeastBits(TxnIdLeastBits).setTxnidMostBits(TxnIdMostBits).build();
			ByteBuf Res = SerializeWithSize(PulsarApi.BaseCommand.newBuilder().setType(PulsarApi.BaseCommand.Type.ADD_PARTITION_TO_TXN).setAddPartitionToTxn(CommandAddPartitionToTxn));
			CommandAddPartitionToTxn.recycle();
			return Res;
		}

		public static ByteBuf NewAddPartitionToTxnResponse(long RequestId, long TxnIdLeastBits, long TxnIdMostBits)
		{
			PulsarApi.CommandAddPartitionToTxnResponse CommandAddPartitionToTxnResponse = PulsarApi.CommandAddPartitionToTxnResponse.newBuilder().setRequestId(RequestId).setTxnidLeastBits(TxnIdLeastBits).setTxnidMostBits(TxnIdMostBits).build();
			ByteBuf Res = SerializeWithSize(PulsarApi.BaseCommand.newBuilder().setType(PulsarApi.BaseCommand.Type.ADD_PARTITION_TO_TXN_RESPONSE).setAddPartitionToTxnResponse(CommandAddPartitionToTxnResponse));
			CommandAddPartitionToTxnResponse.recycle();
			return Res;
		}

		public static ByteBuf NewAddPartitionToTxnResponse(long RequestId, long TxnIdMostBits, PulsarApi.ServerError Error, string ErrorMsg)
		{
			PulsarApi.CommandAddPartitionToTxnResponse.Builder Builder = PulsarApi.CommandAddPartitionToTxnResponse.newBuilder();
			Builder.RequestId = RequestId;
			Builder.TxnidMostBits = TxnIdMostBits;
			Builder.Error = Error;
			if (!string.ReferenceEquals(ErrorMsg, null))
			{
				Builder.setMessage(ErrorMsg);
			}
			PulsarApi.CommandAddPartitionToTxnResponse CommandAddPartitionToTxnResponse = Builder.build();
			ByteBuf Res = SerializeWithSize(PulsarApi.BaseCommand.newBuilder().setType(PulsarApi.BaseCommand.Type.ADD_PARTITION_TO_TXN_RESPONSE).setAddPartitionToTxnResponse(CommandAddPartitionToTxnResponse));
			Builder.recycle();
			CommandAddPartitionToTxnResponse.recycle();
			return Res;
		}

		public static ByteBuf NewAddSubscriptionToTxn(long RequestId, long TxnIdLeastBits, long TxnIdMostBits, IList<PulsarApi.Subscription> Subscription)
		{
			PulsarApi.CommandAddSubscriptionToTxn CommandAddSubscriptionToTxn = PulsarApi.CommandAddSubscriptionToTxn.newBuilder().setRequestId(RequestId).setTxnidLeastBits(TxnIdLeastBits).setTxnidMostBits(TxnIdMostBits).addAllSubscription(Subscription).build();
			ByteBuf Res = SerializeWithSize(PulsarApi.BaseCommand.newBuilder().setType(PulsarApi.BaseCommand.Type.ADD_SUBSCRIPTION_TO_TXN).setAddSubscriptionToTxn(CommandAddSubscriptionToTxn));
			CommandAddSubscriptionToTxn.recycle();
			return Res;
		}

		public static ByteBuf NewAddSubscriptionToTxnResponse(long RequestId, long TxnIdLeastBits, long TxnIdMostBits)
		{
			PulsarApi.CommandAddSubscriptionToTxnResponse Command = PulsarApi.CommandAddSubscriptionToTxnResponse.newBuilder().setRequestId(RequestId).setTxnidLeastBits(TxnIdLeastBits).setTxnidMostBits(TxnIdMostBits).build();
			ByteBuf Res = SerializeWithSize(PulsarApi.BaseCommand.newBuilder().setType(PulsarApi.BaseCommand.Type.ADD_SUBSCRIPTION_TO_TXN_RESPONSE).setAddSubscriptionToTxnResponse(Command));
			Command.recycle();
			return Res;
		}

		public static ByteBuf NewAddSubscriptionToTxnResponse(long RequestId, long TxnIdMostBits, PulsarApi.ServerError Error, string ErrorMsg)
		{
			PulsarApi.CommandAddSubscriptionToTxnResponse.Builder Builder = PulsarApi.CommandAddSubscriptionToTxnResponse.newBuilder();
			Builder.RequestId = RequestId;
			Builder.TxnidMostBits = TxnIdMostBits;
			Builder.Error = Error;
			if (!string.ReferenceEquals(ErrorMsg, null))
			{
				Builder.setMessage(ErrorMsg);
			}
			PulsarApi.CommandAddSubscriptionToTxnResponse ErrorResponse = Builder.build();
			ByteBuf Res = SerializeWithSize(PulsarApi.BaseCommand.newBuilder().setType(PulsarApi.BaseCommand.Type.ADD_SUBSCRIPTION_TO_TXN_RESPONSE).setAddSubscriptionToTxnResponse(ErrorResponse));
			Builder.recycle();
			ErrorResponse.recycle();
			return Res;
		}

		public static ByteBuf NewEndTxn(long RequestId, long TxnIdLeastBits, long TxnIdMostBits, PulsarApi.TxnAction TxnAction)
		{
			PulsarApi.CommandEndTxn CommandEndTxn = PulsarApi.CommandEndTxn.newBuilder().setRequestId(RequestId).setTxnidLeastBits(TxnIdLeastBits).setTxnidMostBits(TxnIdMostBits).setTxnAction(TxnAction).build();
			ByteBuf Res = SerializeWithSize(PulsarApi.BaseCommand.newBuilder().setType(PulsarApi.BaseCommand.Type.END_TXN).setEndTxn(CommandEndTxn));
			CommandEndTxn.recycle();
			return Res;
		}

		public static ByteBuf NewEndTxnResponse(long RequestId, long TxnIdLeastBits, long TxnIdMostBits)
		{
			PulsarApi.CommandEndTxnResponse CommandEndTxnResponse = PulsarApi.CommandEndTxnResponse.newBuilder().setRequestId(RequestId).setTxnidLeastBits(TxnIdLeastBits).setTxnidMostBits(TxnIdMostBits).build();
			ByteBuf Res = SerializeWithSize(PulsarApi.BaseCommand.newBuilder().setType(PulsarApi.BaseCommand.Type.END_TXN_RESPONSE).setEndTxnResponse(CommandEndTxnResponse));
			CommandEndTxnResponse.recycle();
			return Res;
		}

		public static ByteBuf NewEndTxnResponse(long RequestId, long TxnIdMostBits, PulsarApi.ServerError Error, string ErrorMsg)
		{
			PulsarApi.CommandEndTxnResponse.Builder Builder = PulsarApi.CommandEndTxnResponse.newBuilder();
			Builder.RequestId = RequestId;
			Builder.TxnidMostBits = TxnIdMostBits;
			Builder.Error = Error;
			if (!string.ReferenceEquals(ErrorMsg, null))
			{
				Builder.setMessage(ErrorMsg);
			}
			PulsarApi.CommandEndTxnResponse CommandEndTxnResponse = Builder.build();
			ByteBuf Res = SerializeWithSize(PulsarApi.BaseCommand.newBuilder().setType(PulsarApi.BaseCommand.Type.END_TXN_RESPONSE).setEndTxnResponse(CommandEndTxnResponse));
			Builder.recycle();
			CommandEndTxnResponse.recycle();
			return Res;
		}

		public static ByteBuf NewEndTxnOnPartition(long RequestId, long TxnIdLeastBits, long TxnIdMostBits, string Topic, PulsarApi.TxnAction TxnAction)
		{
			PulsarApi.CommandEndTxnOnPartition.Builder TxnEndOnPartition = PulsarApi.CommandEndTxnOnPartition.newBuilder().setRequestId(RequestId).setTxnidLeastBits(TxnIdLeastBits).setTxnidMostBits(TxnIdMostBits).setTopic(Topic).setTxnAction(TxnAction);
			ByteBuf Res = SerializeWithSize(PulsarApi.BaseCommand.newBuilder().setType(PulsarApi.BaseCommand.Type.END_TXN_ON_PARTITION).setEndTxnOnPartition(TxnEndOnPartition));
			TxnEndOnPartition.recycle();
			return Res;
		}

		public static ByteBuf NewEndTxnOnPartitionResponse(long RequestId, long TxnIdLeastBits, long TxnIdMostBits)
		{
			PulsarApi.CommandEndTxnOnPartitionResponse CommandEndTxnOnPartitionResponse = PulsarApi.CommandEndTxnOnPartitionResponse.newBuilder().setRequestId(RequestId).setTxnidLeastBits(TxnIdLeastBits).setTxnidMostBits(TxnIdMostBits).build();
			ByteBuf Res = SerializeWithSize(PulsarApi.BaseCommand.newBuilder().setType(PulsarApi.BaseCommand.Type.END_TXN_ON_PARTITION_RESPONSE).setEndTxnOnPartitionResponse(CommandEndTxnOnPartitionResponse));
			CommandEndTxnOnPartitionResponse.recycle();
			return Res;
		}

		public static ByteBuf NewEndTxnOnPartitionResponse(long RequestId, PulsarApi.ServerError Error, string ErrorMsg)
		{
			PulsarApi.CommandEndTxnOnPartitionResponse.Builder Builder = PulsarApi.CommandEndTxnOnPartitionResponse.newBuilder();
			Builder.RequestId = RequestId;
			Builder.Error = Error;
			if (!string.ReferenceEquals(ErrorMsg, null))
			{
				Builder.setMessage(ErrorMsg);
			}
			PulsarApi.CommandEndTxnOnPartitionResponse CommandEndTxnOnPartitionResponse = Builder.build();
			ByteBuf Res = SerializeWithSize(PulsarApi.BaseCommand.newBuilder().setType(PulsarApi.BaseCommand.Type.END_TXN_ON_PARTITION_RESPONSE).setEndTxnOnPartitionResponse(CommandEndTxnOnPartitionResponse));
			Builder.recycle();
			CommandEndTxnOnPartitionResponse.recycle();
			return Res;
		}

		public static ByteBuf NewEndTxnOnSubscription(long RequestId, long TxnIdLeastBits, long TxnIdMostBits, PulsarApi.Subscription Subscription, PulsarApi.TxnAction TxnAction)
		{
			PulsarApi.CommandEndTxnOnSubscription CommandEndTxnOnSubscription = PulsarApi.CommandEndTxnOnSubscription.newBuilder().setRequestId(RequestId).setTxnidLeastBits(TxnIdLeastBits).setTxnidMostBits(TxnIdMostBits).setSubscription(Subscription).setTxnAction(TxnAction).build();
			ByteBuf Res = SerializeWithSize(PulsarApi.BaseCommand.newBuilder().setType(PulsarApi.BaseCommand.Type.END_TXN_ON_SUBSCRIPTION).setEndTxnOnSubscription(CommandEndTxnOnSubscription));
			CommandEndTxnOnSubscription.recycle();
			return Res;
		}

		public static ByteBuf NewEndTxnOnSubscriptionResponse(long RequestId, long TxnIdLeastBits, long TxnIdMostBits)
		{
			PulsarApi.CommandEndTxnOnSubscriptionResponse Response = PulsarApi.CommandEndTxnOnSubscriptionResponse.newBuilder().setRequestId(RequestId).setTxnidLeastBits(TxnIdLeastBits).setTxnidMostBits(TxnIdMostBits).build();
			ByteBuf Res = SerializeWithSize(PulsarApi.BaseCommand.newBuilder().setType(PulsarApi.BaseCommand.Type.END_TXN_ON_SUBSCRIPTION_RESPONSE).setEndTxnOnSubscriptionResponse(Response));
			Response.recycle();
			return Res;
		}

		public static ByteBuf NewEndTxnOnSubscriptionResponse(long RequestId, PulsarApi.ServerError Error, string ErrorMsg)
		{
			PulsarApi.CommandEndTxnOnSubscriptionResponse.Builder Builder = PulsarApi.CommandEndTxnOnSubscriptionResponse.newBuilder();
			Builder.RequestId = RequestId;
			Builder.Error = Error;
			if (!string.ReferenceEquals(ErrorMsg, null))
			{
				Builder.setMessage(ErrorMsg);
			}
			PulsarApi.CommandEndTxnOnSubscriptionResponse Response = Builder.build();
			ByteBuf Res = SerializeWithSize(PulsarApi.BaseCommand.newBuilder().setType(PulsarApi.BaseCommand.Type.END_TXN_ON_SUBSCRIPTION_RESPONSE).setEndTxnOnSubscriptionResponse(Response));
			Builder.recycle();
			Response.recycle();
			return Res;
		}
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @VisibleForTesting public static io.netty.buffer.ByteBuf serializeWithSize(SharpPulsar.api.proto.PulsarApi.BaseCommand.Builder cmdBuilder)
		public static ByteBuf SerializeWithSize(PulsarApi.BaseCommand.Builder CmdBuilder)
		{
			// / Wire format
			// [TOTAL_SIZE] [CMD_SIZE][CMD]
			PulsarApi.BaseCommand Cmd = CmdBuilder.build();

			int CmdSize = Cmd.SerializedSize;
			int TotalSize = CmdSize + 4;
			int FrameSize = TotalSize + 4;

			ByteBuf Buf = PulsarByteBufAllocator.DEFAULT.buffer(FrameSize, FrameSize);

			// Prepend 2 lengths to the buffer
			Buf.writeInt(TotalSize);
			Buf.writeInt(CmdSize);

			ByteBufCodedOutputStream OutStream = ByteBufCodedOutputStream.get(Buf);

			try
			{
				Cmd.writeTo(OutStream);
			}
			catch (IOException E)
			{
				// This is in-memory serialization, should not fail
				throw new Exception(E);
			}
			finally
			{
				Cmd.recycle();
				CmdBuilder.recycle();
				OutStream.recycle();
			}

			return Buf;
		}

		private static ByteBufPair SerializeCommandSendWithSize(PulsarApi.BaseCommand.Builder CmdBuilder, ChecksumType ChecksumType, PulsarApi.MessageMetadata MsgMetadata, ByteBuf Payload)
		{
			// / Wire format
			// [TOTAL_SIZE] [CMD_SIZE][CMD] [MAGIC_NUMBER][CHECKSUM] [METADATA_SIZE][METADATA] [PAYLOAD]

			PulsarApi.BaseCommand Cmd = CmdBuilder.build();
			int CmdSize = Cmd.SerializedSize;
			int MsgMetadataSize = MsgMetadata.SerializedSize;
			int PayloadSize = Payload.readableBytes();
			int MagicAndChecksumLength = ChecksumType.Crc32c.Equals(ChecksumType) ? (2 + 4) : 0;
			bool IncludeChecksum = MagicAndChecksumLength > 0;
			// cmdLength + cmdSize + magicLength +
			// checksumSize + msgMetadataLength +
			// msgMetadataSize
			int HeaderContentSize = 4 + CmdSize + MagicAndChecksumLength + 4 + MsgMetadataSize;
			int TotalSize = HeaderContentSize + PayloadSize;
			int HeadersSize = 4 + HeaderContentSize; // totalSize + headerLength
			int ChecksumReaderIndex = -1;

			ByteBuf Headers = PulsarByteBufAllocator.DEFAULT.buffer(HeadersSize, HeadersSize);
			Headers.writeInt(TotalSize); // External frame

			try
			{
				// Write cmd
				Headers.writeInt(CmdSize);

				ByteBufCodedOutputStream OutStream = ByteBufCodedOutputStream.get(Headers);
				Cmd.writeTo(OutStream);
				Cmd.recycle();
				CmdBuilder.recycle();

				//Create checksum placeholder
				if (IncludeChecksum)
				{
					Headers.writeShort(MagicCrc32c);
					ChecksumReaderIndex = Headers.writerIndex();
					Headers.writerIndex(Headers.writerIndex() + ChecksumSize); //skip 4 bytes of checksum
				}

				// Write metadata
				Headers.writeInt(MsgMetadataSize);
				MsgMetadata.writeTo(OutStream);
				OutStream.recycle();
			}
			catch (IOException E)
			{
				// This is in-memory serialization, should not fail
				throw new Exception(E);
			}

			ByteBufPair Command = ByteBufPair.Get(Headers, Payload);

			// write checksum at created checksum-placeholder
			if (IncludeChecksum)
			{
				Headers.markReaderIndex();
				Headers.readerIndex(ChecksumReaderIndex + ChecksumSize);
				int MetadataChecksum = computeChecksum(Headers);
				int ComputedChecksum = resumeChecksum(MetadataChecksum, Payload);
				// set computed checksum
				Headers.setInt(ChecksumReaderIndex, ComputedChecksum);
				Headers.resetReaderIndex();
			}
			return Command;
		}

		public static ByteBuf SerializeMetadataAndPayload(ChecksumType ChecksumType, PulsarApi.MessageMetadata MsgMetadata, ByteBuf Payload)
		{
			// / Wire format
			// [MAGIC_NUMBER][CHECKSUM] [METADATA_SIZE][METADATA] [PAYLOAD]
			int MsgMetadataSize = MsgMetadata.SerializedSize;
			int PayloadSize = Payload.readableBytes();
			int MagicAndChecksumLength = ChecksumType.Crc32c.Equals(ChecksumType) ? (2 + 4) : 0;
			bool IncludeChecksum = MagicAndChecksumLength > 0;
			int HeaderContentSize = MagicAndChecksumLength + 4 + MsgMetadataSize; // magicLength +
																				  // checksumSize + msgMetadataLength +
																				  // msgMetadataSize
			int ChecksumReaderIndex = -1;
			int TotalSize = HeaderContentSize + PayloadSize;

			ByteBuf MetadataAndPayload = PulsarByteBufAllocator.DEFAULT.buffer(TotalSize, TotalSize);
			try
			{
				ByteBufCodedOutputStream OutStream = ByteBufCodedOutputStream.get(MetadataAndPayload);

				//Create checksum placeholder
				if (IncludeChecksum)
				{
					MetadataAndPayload.writeShort(MagicCrc32c);
					ChecksumReaderIndex = MetadataAndPayload.writerIndex();
					MetadataAndPayload.writerIndex(MetadataAndPayload.writerIndex() + ChecksumSize); //skip 4 bytes of checksum
				}

				// Write metadata
				MetadataAndPayload.writeInt(MsgMetadataSize);
				MsgMetadata.writeTo(OutStream);
				OutStream.recycle();
			}
			catch (IOException E)
			{
				// This is in-memory serialization, should not fail
				throw new Exception(E);
			}

			// write checksum at created checksum-placeholder
			if (IncludeChecksum)
			{
				MetadataAndPayload.markReaderIndex();
				MetadataAndPayload.readerIndex(ChecksumReaderIndex + ChecksumSize);
				int MetadataChecksum = computeChecksum(MetadataAndPayload);
				int ComputedChecksum = resumeChecksum(MetadataChecksum, Payload);
				// set computed checksum
				MetadataAndPayload.setInt(ChecksumReaderIndex, ComputedChecksum);
				MetadataAndPayload.resetReaderIndex();
			}
			MetadataAndPayload.writeBytes(Payload);

			return MetadataAndPayload;
		}

		public static long InitBatchMessageMetadata(PulsarApi.MessageMetadata.Builder MessageMetadata, PulsarApi.MessageMetadata.Builder Builder)
		{
			MessageMetadata.PublishTime = Builder.PublishTime;
			MessageMetadata.setProducerName(Builder.getProducerName());
			MessageMetadata.SequenceId = Builder.SequenceId;
			if (Builder.hasReplicatedFrom())
			{
				MessageMetadata.setReplicatedFrom(Builder.getReplicatedFrom());
			}
			if (Builder.ReplicateToCount > 0)
			{
				MessageMetadata.addAllReplicateTo(Builder.ReplicateToList);
			}
			if (Builder.hasSchemaVersion())
			{
				MessageMetadata.SchemaVersion = Builder.SchemaVersion;
			}
			return Builder.SequenceId;
		}

		public static ByteBuf SerializeSingleMessageInBatchWithPayload(PulsarApi.SingleMessageMetadata.Builder SingleMessageMetadataBuilder, ByteBuf Payload, ByteBuf BatchBuffer)
		{
			int PayLoadSize = Payload.readableBytes();
			PulsarApi.SingleMessageMetadata SingleMessageMetadata = SingleMessageMetadataBuilder.setPayloadSize(PayLoadSize).build();
			// serialize meta-data size, meta-data and payload for single message in batch
			int SingleMsgMetadataSize = SingleMessageMetadata.SerializedSize;
			try
			{
				BatchBuffer.writeInt(SingleMsgMetadataSize);
				ByteBufCodedOutputStream OutStream = ByteBufCodedOutputStream.get(BatchBuffer);
				SingleMessageMetadata.writeTo(OutStream);
				SingleMessageMetadata.recycle();
				OutStream.recycle();
			}
			catch (IOException E)
			{
				throw new Exception(E);
			}
			return BatchBuffer.writeBytes(Payload);
		}

		public static ByteBuf SerializeSingleMessageInBatchWithPayload(PulsarApi.MessageMetadata.Builder MsgBuilder, ByteBuf Payload, ByteBuf BatchBuffer)
		{

			// build single message meta-data
			PulsarApi.SingleMessageMetadata.Builder SingleMessageMetadataBuilder = PulsarApi.SingleMessageMetadata.newBuilder();
			if (MsgBuilder.hasPartitionKey())
			{
				SingleMessageMetadataBuilder = SingleMessageMetadataBuilder.setPartitionKey(MsgBuilder.getPartitionKey()).setPartitionKeyB64Encoded(MsgBuilder.PartitionKeyB64Encoded);
			}
			if (MsgBuilder.hasOrderingKey())
			{
				SingleMessageMetadataBuilder = SingleMessageMetadataBuilder.setOrderingKey(MsgBuilder.OrderingKey);
			}
			if (MsgBuilder.PropertiesList.Count > 0)
			{
				SingleMessageMetadataBuilder = SingleMessageMetadataBuilder.addAllProperties(MsgBuilder.PropertiesList);
			}

			if (MsgBuilder.hasEventTime())
			{
				SingleMessageMetadataBuilder.EventTime = MsgBuilder.EventTime;
			}

			if (MsgBuilder.hasSequenceId())
			{
				SingleMessageMetadataBuilder.SequenceId = MsgBuilder.SequenceId;
			}

			try
			{
				return SerializeSingleMessageInBatchWithPayload(SingleMessageMetadataBuilder, Payload, BatchBuffer);
			}
			finally
			{
				SingleMessageMetadataBuilder.recycle();
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static io.netty.buffer.ByteBuf deSerializeSingleMessageInBatch(io.netty.buffer.ByteBuf uncompressedPayload, SharpPulsar.api.proto.PulsarApi.SingleMessageMetadata.Builder singleMessageMetadataBuilder, int index, int batchSize) throws java.io.IOException
		public static ByteBuf DeSerializeSingleMessageInBatch(ByteBuf UncompressedPayload, PulsarApi.SingleMessageMetadata.Builder SingleMessageMetadataBuilder, int Index, int BatchSize)
		{
			int SingleMetaSize = (int) UncompressedPayload.readUnsignedInt();
			int WriterIndex = UncompressedPayload.writerIndex();
			int BeginIndex = UncompressedPayload.readerIndex() + SingleMetaSize;
			UncompressedPayload.writerIndex(BeginIndex);
			ByteBufCodedInputStream Stream = ByteBufCodedInputStream.get(UncompressedPayload);
			SingleMessageMetadataBuilder.mergeFrom(Stream, null);
			Stream.recycle();

			int SingleMessagePayloadSize = SingleMessageMetadataBuilder.PayloadSize;

			int ReaderIndex = UncompressedPayload.readerIndex();
			ByteBuf SingleMessagePayload = UncompressedPayload.retainedSlice(ReaderIndex, SingleMessagePayloadSize);
			UncompressedPayload.writerIndex(WriterIndex);

			// reader now points to beginning of payload read; so move it past message payload just read
			if (Index < BatchSize)
			{
				UncompressedPayload.readerIndex(ReaderIndex + SingleMessagePayloadSize);
			}

			return SingleMessagePayload;
		}

		private static ByteBufPair SerializeCommandMessageWithSize(PulsarApi.BaseCommand Cmd, ByteBuf MetadataAndPayload)
		{
			// / Wire format
			// [TOTAL_SIZE] [CMD_SIZE][CMD] [MAGIC_NUMBER][CHECKSUM] [METADATA_SIZE][METADATA] [PAYLOAD]
			//
			// metadataAndPayload contains from magic-number to the payload included


			int CmdSize = Cmd.SerializedSize;
			int TotalSize = 4 + CmdSize + MetadataAndPayload.readableBytes();
			int HeadersSize = 4 + 4 + CmdSize;

			ByteBuf Headers = PulsarByteBufAllocator.DEFAULT.buffer(HeadersSize);
			Headers.writeInt(TotalSize); // External frame

			try
			{
				// Write cmd
				Headers.writeInt(CmdSize);

				ByteBufCodedOutputStream OutStream = ByteBufCodedOutputStream.get(Headers);
				Cmd.writeTo(OutStream);
				OutStream.recycle();
			}
			catch (IOException E)
			{
				// This is in-memory serialization, should not fail
				throw new Exception(E);
			}

			return (ByteBufPair) ByteBufPair.Get(Headers, MetadataAndPayload);
		}

		public static int GetNumberOfMessagesInBatch(ByteBuf MetadataAndPayload, string Subscription, long ConsumerId)
		{
			PulsarApi.MessageMetadata MsgMetadata = PeekMessageMetadata(MetadataAndPayload, Subscription, ConsumerId);
			if (MsgMetadata == null)
			{
				return -1;
			}
			else
			{
				int NumMessagesInBatch = MsgMetadata.NumMessagesInBatch;
				MsgMetadata.recycle();
				return NumMessagesInBatch;
			}
		}

		public static PulsarApi.MessageMetadata PeekMessageMetadata(ByteBuf MetadataAndPayload, string Subscription, long ConsumerId)
		{
			try
			{
				// save the reader index and restore after parsing
				int ReaderIdx = MetadataAndPayload.readerIndex();
				PulsarApi.MessageMetadata Metadata = Commands.ParseMessageMetadata(MetadataAndPayload);
				MetadataAndPayload.readerIndex(ReaderIdx);

				return Metadata;
			}
			catch (Exception T)
			{
				log.error("[{}] [{}] Failed to parse message metadata", Subscription, ConsumerId, T);
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

		public static bool PeerSupportsGetLastMessageId(int PeerVersion)
		{
			return PeerVersion >= PulsarApi.ProtocolVersion.v12.Number;
		}

		public static bool PeerSupportsActiveConsumerListener(int PeerVersion)
		{
			return PeerVersion >= PulsarApi.ProtocolVersion.v12.Number;
		}

		public static bool PeerSupportsMultiMessageAcknowledgment(int PeerVersion)
		{
			return PeerVersion >= PulsarApi.ProtocolVersion.v12.Number;
		}

		public static bool PeerSupportJsonSchemaAvroFormat(int PeerVersion)
		{
			return PeerVersion >= PulsarApi.ProtocolVersion.v13.Number;
		}

		public static bool PeerSupportsGetOrCreateSchema(int PeerVersion)
		{
			return PeerVersion >= PulsarApi.ProtocolVersion.v15.Number;
		}
	}

}