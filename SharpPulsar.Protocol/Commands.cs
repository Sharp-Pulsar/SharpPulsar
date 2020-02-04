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
		
		public static IByteBuffer NewConnect(string AuthMethodName, string AuthData, string LibVersion)
		{
			return NewConnect(AuthMethodName, AuthData, CurrentProtocolVersion, LibVersion, null, null, null, null);
		}

		public static IByteBuffer NewConnect(string AuthMethodName, string AuthData, string LibVersion, string TargetBroker)
		{
			return NewConnect(AuthMethodName, AuthData, CurrentProtocolVersion, LibVersion, TargetBroker, null, null, null);
		}

		public static IByteBuffer NewConnect(string AuthMethodName, string AuthData, string LibVersion, string TargetBroker, string OriginalPrincipal, string ClientAuthData, string ClientAuthMethod)
		{
			return NewConnect(AuthMethodName, AuthData, CurrentProtocolVersion, LibVersion, TargetBroker, OriginalPrincipal, ClientAuthData, ClientAuthMethod);
		}

		public static IByteBuffer NewConnect(string AuthMethodName, string authData, int ProtocolVersion, string LibVersion, string TargetBroker, string OriginalPrincipal, string OriginalAuthData, string OriginalAuthMethod)
		{
			CommandConnect.Builder ConnectBuilder = CommandConnect.NewBuilder();
			ConnectBuilder.SetClientVersion(!string.ReferenceEquals(LibVersion, null) ? LibVersion : "Pulsar Client");
			ConnectBuilder.SetAuthMethodName(AuthMethodName);

			if ("ycav1".Equals(AuthMethodName))
			{
				// Handle the case of a client that gets updated before the broker and starts sending the string auth method
				// name. An example would be in broker-to-broker replication. We need to make sure the clients are still
				// passing both the enum and the string until all brokers are upgraded.
				ConnectBuilder.SetAuthMethod(AuthMethod.YcaV1);
			}

			if (!string.ReferenceEquals(TargetBroker, null))
			{
				// When connecting through a proxy, we need to specify which broker do we want to be proxied through
				ConnectBuilder.SetProxyToBrokerUrl(TargetBroker);
			}

			if (!string.ReferenceEquals(authData, null))
			{
				ConnectBuilder.SetAuthData(ByteString.CopyFromUtf8(authData));
			}

			if (!string.ReferenceEquals(OriginalPrincipal, null))
			{
				ConnectBuilder.SetOriginalPrincipal(OriginalPrincipal);
			}

			if (!string.ReferenceEquals(OriginalAuthData, null))
			{
				ConnectBuilder.SetOriginalAuthData(OriginalAuthData);
			}

			if (!string.ReferenceEquals(OriginalAuthMethod, null))
			{
				ConnectBuilder.SetOriginalAuthMethod(OriginalAuthMethod);
			}
			ConnectBuilder.SetProtocolVersion(ProtocolVersion);
			CommandConnect Connect = ConnectBuilder.Build();
			IByteBuffer Res = SerializeWithSize(BaseCommand.NewBuilder().SetType(BaseCommand.Types.Type.Connect).SetConnect(Connect));
			Connect.Recycle();
			ConnectBuilder.Recycle();
			return Res;
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
			
			IByteBuffer Res = SerializeWithSize(BaseCommand.NewBuilder().SetType(BaseCommand.Types.Type.Connect).SetConnect(connect));
			connect.Recycle();
			connectBuilder.Recycle();
			return Res;
		}

		public static IByteBuffer NewConnected(int ClientProtocoVersion)
		{
			return NewConnected(ClientProtocoVersion, InvalidMaxMessageSize);
		}

		public static IByteBuffer NewConnected(int ClientProtocolVersion, int MaxMessageSize)
		{
			CommandConnected.Builder ConnectedBuilder = CommandConnected.NewBuilder();
			ConnectedBuilder.SetServerVersion("Pulsar Server");
			if (InvalidMaxMessageSize != MaxMessageSize)
			{
				ConnectedBuilder.SetMaxMessageSize(MaxMessageSize);
			}

			// If the broker supports a newer version of the protocol, it will anyway advertise the max version that the
			// client supports, to avoid confusing the client.
			int currentProtocolVersion = CurrentProtocolVersion;
			int VersionToAdvertise = Math.Min(CurrentProtocolVersion, ClientProtocolVersion);

			CommandConnected Connected = ConnectedBuilder.Build();
			IByteBuffer Res = SerializeWithSize(BaseCommand.NewBuilder().SetType(BaseCommand.Types.Type.Connected).SetConnected(ConnectedBuilder));
			Connected.Recycle();
			ConnectedBuilder.Recycle();
			return Res;
		}

		public static IByteBuffer NewAuthChallenge(string AuthMethod, AuthData BrokerData, int ClientProtocolVersion)
		{
			CommandAuthChallenge.Builder ChallengeBuilder = CommandAuthChallenge.NewBuilder();

			// If the broker supports a newer version of the protocol, it will anyway advertise the max version that the
			// client supports, to avoid confusing the client.
			int currentProtocolVersion = CurrentProtocolVersion;
			int VersionToAdvertise = Math.Min(CurrentProtocolVersion, ClientProtocolVersion);

			ChallengeBuilder.SetProtocolVersion(VersionToAdvertise);

			CommandAuthChallenge Challenge = ChallengeBuilder.SetChallenge(AuthData.NewBuilder().SetAuthData(BrokerData.AuthData_).SetAuthMethodName(AuthMethod).Build()).Build();

			IByteBuffer Res = SerializeWithSize(BaseCommand.NewBuilder().SetType(BaseCommand.Types.Type.AuthChallenge).SetAuthChallenge(Challenge));
			Challenge.Recycle();
			ChallengeBuilder.Recycle();
			return Res;
		}

		public static IByteBuffer NewAuthResponse(string AuthMethod, AuthData ClientData, int ClientProtocolVersion, string ClientVersion)
		{
			CommandAuthResponse.Builder ResponseBuilder = CommandAuthResponse.NewBuilder();

			ResponseBuilder.SetClientVersion(!string.ReferenceEquals(ClientVersion, null) ? ClientVersion : "Pulsar Client");
			ResponseBuilder.SetProtocolVersion(ClientProtocolVersion);

			CommandAuthResponse Response = ResponseBuilder.SetResponse(AuthData.NewBuilder().SetAuthData(ClientData.AuthData_).SetAuthMethodName(AuthMethod).Build()).Build();

			IByteBuffer Res = SerializeWithSize(BaseCommand.NewBuilder().SetType(BaseCommand.Types.Type.AuthResponse).SetAuthResponse(Response));
			Response.Recycle();
			ResponseBuilder.Recycle();
			return Res;
		}

		public static IByteBuffer NewSuccess(long RequestId)
		{
			CommandSuccess.Builder SuccessBuilder = CommandSuccess.NewBuilder();
			SuccessBuilder.SetRequestId(RequestId);
			CommandSuccess Success = SuccessBuilder.Build();
			IByteBuffer Res = SerializeWithSize(BaseCommand.NewBuilder().SetType(BaseCommand.Types.Type.Success).SetSuccess(Success));
			SuccessBuilder.Recycle();
			Success.Recycle();
			return Res;
		}

		public static IByteBuffer NewProducerSuccess(long RequestId, string ProducerName, SchemaVersion SchemaVersion)
		{
			return NewProducerSuccess(RequestId, ProducerName, -1, SchemaVersion);
		}

		public static IByteBuffer NewProducerSuccess(long RequestId, string ProducerName, long LastSequenceId, SchemaVersion SchemaVersion)
		{
			CommandProducerSuccess.Builder ProducerSuccessBuilder = CommandProducerSuccess.NewBuilder();
			ProducerSuccessBuilder.SetRequestId(RequestId);
			ProducerSuccessBuilder.SetProducerName(ProducerName);
			ProducerSuccessBuilder.SetLastSequenceId(LastSequenceId);
			ProducerSuccessBuilder.SetSchemaVersion(ByteString.CopyFrom((byte[])(object)SchemaVersion.Bytes()));
			CommandProducerSuccess ProducerSuccess = ProducerSuccessBuilder.Build();
			IByteBuffer Res = SerializeWithSize(BaseCommand.NewBuilder().SetType(BaseCommand.Types.Type.ProducerSuccess).SetProducerSuccess(ProducerSuccess));
			ProducerSuccess.Recycle();
			ProducerSuccessBuilder.Recycle();
			return Res;
		}

		public static IByteBuffer NewError(long RequestId, ServerError Error, string Message)
		{
			CommandError.Builder CmdErrorBuilder = CommandError.NewBuilder();
			CmdErrorBuilder.SetRequestId(RequestId);
			CmdErrorBuilder.SetError(Error);
			CmdErrorBuilder.SetMessage(Message);
			CommandError CmdError = CmdErrorBuilder.Build();
			IByteBuffer Res = SerializeWithSize(BaseCommand.NewBuilder().SetType(BaseCommand.Types.Type.Error).SetError(CmdError));
			CmdError.Recycle();
			CmdErrorBuilder.Recycle();
			return Res;

		}

		public static IByteBuffer NewSendReceipt(long ProducerId, long SequenceId, long HighestId, long LedgerId, long EntryId)
		{
			CommandSendReceipt.Builder SendReceiptBuilder = CommandSendReceipt.NewBuilder();
			SendReceiptBuilder.SetProducerId(ProducerId);
			SendReceiptBuilder.SetSequenceId(SequenceId);
			SendReceiptBuilder.SetHighestSequenceId(HighestId);
			MessageIdData.Builder MessageIdBuilder = MessageIdData.NewBuilder();
			MessageIdBuilder.LedgerId = LedgerId;
			MessageIdBuilder.EntryId = EntryId;
			MessageIdData MessageId = MessageIdBuilder.Build();
			SendReceiptBuilder.SetMessageId(MessageId);
			CommandSendReceipt SendReceipt = SendReceiptBuilder.Build();
			IByteBuffer Res = SerializeWithSize(BaseCommand.NewBuilder().SetType(BaseCommand.Types.Type.SendReceipt).SetSendReceipt(SendReceipt));
			MessageIdBuilder.Recycle();
			MessageId.Recycle();
			SendReceiptBuilder.Recycle();
			SendReceipt.Recycle();
			return Res;
		}

		public static IByteBuffer NewSendError(long ProducerId, long SequenceId, ServerError Error, string ErrorMsg)
		{
			CommandSendError.Builder SendErrorBuilder = CommandSendError.NewBuilder();
			SendErrorBuilder.SetProducerId(ProducerId);
			SendErrorBuilder.SetSequenceId(SequenceId);
			SendErrorBuilder.SetError(Error);
			SendErrorBuilder.SetMessage(ErrorMsg);
			CommandSendError SendError = SendErrorBuilder.Build();
			IByteBuffer Res = SerializeWithSize(BaseCommand.NewBuilder().SetType(BaseCommand.Types.Type.SendError).SetSendError(SendError));
			SendErrorBuilder.Recycle();
			SendError.Recycle();
			return Res;
		}


		public static bool HasChecksum(IByteBuffer Buffer)
		{
			return Buffer.GetShort(Buffer.ReaderIndex) == MagicCrc32c;
		}

		/// <summary>
		/// Read the checksum and advance the reader index in the buffer.
		/// 
		/// <para>Note: This method assume the checksum presence was already verified before.
		/// </para>
		/// </summary>
		public static int ReadChecksum(IByteBuffer Buffer)
		{
			Buffer.SkipBytes(2); //skip magic bytes
			return Buffer.ReadInt();
		}

		public static void SkipChecksumIfPresent(IByteBuffer Buffer)
		{
			if (HasChecksum(Buffer))
			{
				ReadChecksum(Buffer);
			}
		}

		public static MessageMetadata ParseMessageMetadata(IByteBuffer Buffer)
		{
			try
			{
				// initially reader-index may point to start_of_checksum : increment reader-index to start_of_metadata
				// to parse metadata
				SkipChecksumIfPresent(Buffer);
				int MetadataSize = (int) Buffer.ReadUnsignedInt();

				int WriterIndex = Buffer.WriterIndex;
				Buffer.SetWriterIndex(Buffer.ReaderIndex + MetadataSize);
				ByteBufCodedInputStream Stream = ByteBufCodedInputStream.Get(Buffer);
				MessageMetadata.Builder MessageMetadataBuilder = MessageMetadata.NewBuilder();
				MessageMetadata Res = ((MessageMetadata.Builder)MessageMetadataBuilder.MergeFrom(Stream, null)).Build();
				Buffer.SetWriterIndex(WriterIndex);
				MessageMetadataBuilder.Recycle();
				Stream.Recycle();
				return Res;
			}
			catch (IOException E)
			{
				throw new System.Exception(E.Message, E);
			}
		}

		public static void SkipMessageMetadata(IByteBuffer Buffer)
		{
			// initially reader-index may point to start_of_checksum : increment reader-index to start_of_metadata to parse
			// metadata
			SkipChecksumIfPresent(Buffer);
			int MetadataSize = (int) Buffer.ReadUnsignedInt();
			Buffer.SkipBytes(MetadataSize);
		}

		public static ByteBufPair NewMessage(long ConsumerId, MessageIdData MessageId, int RedeliveryCount, IByteBuffer MetadataAndPayload)
		{
			CommandMessage.Builder MsgBuilder = CommandMessage.NewBuilder();
			MsgBuilder.SetConsumerId(ConsumerId);
			MsgBuilder.SetMessageId(MessageId);
			if (RedeliveryCount > 0)
			{
				MsgBuilder.SetRedeliveryCount(RedeliveryCount);
			}
			CommandMessage Msg = MsgBuilder.Build();
			BaseCommand.Builder CmdBuilder = BaseCommand.NewBuilder();
			BaseCommand Cmd = CmdBuilder.SetType(BaseCommand.Types.Type.Message).SetMessage(Msg).Build();

			ByteBufPair Res = SerializeCommandMessageWithSize(Cmd, MetadataAndPayload);
			Cmd.Recycle();
			CmdBuilder.Recycle();
			Msg.Recycle();
			MsgBuilder.Recycle();
			return Res;
		}

		public static ByteBufPair NewSend(long ProducerId, long SequenceId, int NumMessaegs, ChecksumType? ChecksumType, MessageMetadata MessageMetadata, IByteBuffer Payload)
		{
			return NewSend(ProducerId, SequenceId, NumMessaegs, 0, 0, ChecksumType, MessageMetadata, Payload);
		}

		public static ByteBufPair NewSend(long ProducerId, long LowestSequenceId, long HighestSequenceId, int NumMessaegs, ChecksumType ChecksumType, MessageMetadata MessageMetadata, IByteBuffer Payload)
		{
			return NewSend(ProducerId, LowestSequenceId, HighestSequenceId, NumMessaegs, 0, 0, ChecksumType, MessageMetadata, Payload);
		}

		public static ByteBufPair NewSend(long ProducerId, long SequenceId, int NumMessages, long TxnIdLeastBits, long TxnIdMostBits, ChecksumType ChecksumType, MessageMetadata MessageData, IByteBuffer Payload)
		{
			CommandSend.Builder SendBuilder = CommandSend.NewBuilder();
			SendBuilder.SetProducerId(ProducerId);
			SendBuilder.SetSequenceId(SequenceId);
			if (NumMessages > 1)
			{
				SendBuilder.SetNumMessages(NumMessages);
			}
			if (TxnIdLeastBits > 0)
			{
				SendBuilder.SetTxnidLeastBits(TxnIdLeastBits);
			}
			if (TxnIdMostBits > 0)
			{
				SendBuilder.SetTxnidMostBits(TxnIdMostBits);
			}
			CommandSend Send = SendBuilder.Build();

			ByteBufPair Res = SerializeCommandSendWithSize(BaseCommand.NewBuilder().SetType(BaseCommand.Types.Type.Send).SetSend(Send), ChecksumType, MessageData, Payload);
			Send.Recycle();
			SendBuilder.Recycle();
			return Res;
		}

		public static ByteBufPair NewSend(long ProducerId, long LowestSequenceId, long HighestSequenceId, int NumMessages, long TxnIdLeastBits, long TxnIdMostBits, ChecksumType ChecksumType, MessageMetadata MessageData, IByteBuffer Payload)
		{
			CommandSend.Builder SendBuilder = CommandSend.NewBuilder();
			SendBuilder.SetProducerId(ProducerId);
			SendBuilder.SetSequenceId(LowestSequenceId);
			SendBuilder.SetHighestSequenceId(HighestSequenceId);
			if (NumMessages > 1)
			{
				SendBuilder.SetNumMessages(NumMessages);
			}
			if (TxnIdLeastBits > 0)
			{
				SendBuilder.SetTxnidLeastBits(TxnIdLeastBits);
			}
			if (TxnIdMostBits > 0)
			{
				SendBuilder.SetTxnidMostBits(TxnIdMostBits);
			}
			CommandSend Send = SendBuilder.Build();

			ByteBufPair Res = SerializeCommandSendWithSize(BaseCommand.NewBuilder().SetType(BaseCommand.Types.Type.Send).SetSend(Send), ChecksumType, MessageData, Payload);
			Send.Recycle();
			SendBuilder.Recycle();
			return Res;
		}

		public static IByteBuffer NewSubscribe(string Topic, string Subscription, long ConsumerId, long RequestId, CommandSubscribe.Types.SubType SubType, int PriorityLevel, string ConsumerName, long ResetStartMessageBackInSeconds)
		{
			return NewSubscribe(Topic, Subscription, ConsumerId, RequestId, SubType, PriorityLevel, ConsumerName, true, null, new Dictionary<string,string>(), false, false, CommandSubscribe.Types.InitialPosition.Earliest, ResetStartMessageBackInSeconds, null, true);
		}

		public static IByteBuffer NewSubscribe(string Topic, string Subscription, long ConsumerId, long RequestId, CommandSubscribe.Types.SubType SubType, int PriorityLevel, string ConsumerName, bool IsDurable, MessageIdData StartMessageId, IDictionary<string, string> Metadata, bool ReadCompacted, bool IsReplicated, CommandSubscribe.Types.InitialPosition SubscriptionInitialPosition, long StartMessageRollbackDurationInSec, SchemaInfo SchemaInfo, bool CreateTopicIfDoesNotExist)
		{
					return NewSubscribe(Topic, Subscription, ConsumerId, RequestId, SubType, PriorityLevel, ConsumerName, IsDurable, StartMessageId, Metadata, ReadCompacted, IsReplicated, SubscriptionInitialPosition, StartMessageRollbackDurationInSec, SchemaInfo, CreateTopicIfDoesNotExist, null);
		}

		public static IByteBuffer NewSubscribe(string Topic, string Subscription, long ConsumerId, long RequestId, CommandSubscribe.Types.SubType SubType, int PriorityLevel, string ConsumerName, bool IsDurable, MessageIdData StartMessageId, IDictionary<string, string> Metadata, bool ReadCompacted, bool IsReplicated, CommandSubscribe.Types.InitialPosition SubscriptionInitialPosition, long StartMessageRollbackDurationInSec, SchemaInfo SchemaInfo, bool CreateTopicIfDoesNotExist, KeySharedPolicy KeySharedPolicy)
		{
			CommandSubscribe.Builder SubscribeBuilder = CommandSubscribe.NewBuilder();
			SubscribeBuilder.SetTopic(Topic);
			SubscribeBuilder.SetSubscription(Subscription);
			SubscribeBuilder.SetSubType(SubType);
			SubscribeBuilder.SetConsumerId(ConsumerId);
			SubscribeBuilder.SetConsumerName(ConsumerName);
			SubscribeBuilder.SetRequestId(RequestId);
			SubscribeBuilder.SetPriorityLevel(PriorityLevel);
			SubscribeBuilder.SetDurable(IsDurable);
			SubscribeBuilder.SetReadCompacted(ReadCompacted);
			SubscribeBuilder.SetInitialPosition(SubscriptionInitialPosition);
			SubscribeBuilder.SetReplicateSubscriptionState(IsReplicated);
			SubscribeBuilder.SetForceTopicCreation(CreateTopicIfDoesNotExist);

			if (KeySharedPolicy != null)
			{
				switch (KeySharedPolicy.KeySharedMode)
				{
					case Common.Enum.KeySharedMode.AUTO_SPLIT:
						SubscribeBuilder.SetKeySharedMeta(KeySharedMeta.NewBuilder().SetKeySharedMode(KeySharedMode.AutoSplit));
						break;
					case Common.Enum.KeySharedMode.STICKY:
						KeySharedMeta.Builder Builder = KeySharedMeta.NewBuilder().SetKeySharedMode(KeySharedMode.Sticky);
						IList<Common.Entity.Range> Ranges = ((KeySharedPolicy.KeySharedPolicySticky) KeySharedPolicy).GetRanges;
						foreach (var Range in Ranges)
						{
							Builder.AddHashRanges(IntRange.NewBuilder().SetStart(Range.Start).SetEnd(Range.End));
						}
						SubscribeBuilder.SetKeySharedMeta(Builder);
						break;
				}
			}

			if (StartMessageId != null)
			{
				SubscribeBuilder.SetStartMessageId(StartMessageId);
			}
			if (StartMessageRollbackDurationInSec > 0)
			{
				SubscribeBuilder.SetStartMessageRollbackDurationSec(StartMessageRollbackDurationInSec);
			}
			SubscribeBuilder.AddAllMetadata(CommandUtils.ToKeyValueList(Metadata));

			Proto.Schema Schema = null;
			if (SchemaInfo != null)
			{
				Schema = GetSchema(SchemaInfo);
				SubscribeBuilder.SetSchema(Schema);
			}

			CommandSubscribe Subscribe = SubscribeBuilder.Build();
			IByteBuffer Res = SerializeWithSize(BaseCommand.NewBuilder().SetType(BaseCommand.Types.Type.Subscribe).SetSubscribe(Subscribe));
			SubscribeBuilder.Recycle();
			Subscribe.Recycle();
			if (null != Schema)
			{
				Schema.Recycle();
			}
			return Res;
		}

		public static IByteBuffer NewUnsubscribe(long ConsumerId, long RequestId)
		{
			CommandUnsubscribe.Builder UnsubscribeBuilder = CommandUnsubscribe.NewBuilder();
			UnsubscribeBuilder.SetConsumerId(ConsumerId);
			UnsubscribeBuilder.SetRequestId(RequestId);
			CommandUnsubscribe Unsubscribe = UnsubscribeBuilder.Build();
			IByteBuffer Res = SerializeWithSize(BaseCommand.NewBuilder().SetType(BaseCommand.Types.Type.Unsubscribe).SetUnsubscribe(Unsubscribe));
			UnsubscribeBuilder.Recycle();
			Unsubscribe.Recycle();
			return Res;
		}

		public static IByteBuffer NewActiveConsumerChange(long ConsumerId, bool IsActive)
		{
			CommandActiveConsumerChange.Builder ChangeBuilder = CommandActiveConsumerChange.NewBuilder().SetConsumerId(ConsumerId).SetIsActive(IsActive);

			CommandActiveConsumerChange Change = ChangeBuilder.Build();
			IByteBuffer Res = SerializeWithSize(BaseCommand.NewBuilder().SetType(BaseCommand.Types.Type.ActiveConsumerChange).SetActiveConsumerChange(Change));
			ChangeBuilder.Recycle();
			Change.Recycle();
			return Res;
		}

		public static IByteBuffer NewSeek(long ConsumerId, long RequestId, long LedgerId, long EntryId)
		{
			CommandSeek.Builder SeekBuilder = CommandSeek.NewBuilder();
			SeekBuilder.SetConsumerId(ConsumerId);
			SeekBuilder.SetRequestId(RequestId);

			MessageIdData.Builder MessageIdBuilder = MessageIdData.NewBuilder();
			MessageIdBuilder.LedgerId = LedgerId;
			MessageIdBuilder.EntryId = EntryId;
			MessageIdData MessageId = MessageIdBuilder.Build();
			SeekBuilder.SetMessageId(MessageId);

			CommandSeek Seek = SeekBuilder.Build();
			IByteBuffer Res = SerializeWithSize(BaseCommand.NewBuilder().SetType(BaseCommand.Types.Type.Seek).SetSeek(Seek));
			MessageId.Recycle();
			MessageIdBuilder.Recycle();
			SeekBuilder.Recycle();
			Seek.Recycle();
			return Res;
		}

		public static IByteBuffer NewSeek(long ConsumerId, long RequestId, long Timestamp)
		{
			CommandSeek.Builder SeekBuilder = CommandSeek.NewBuilder();
			SeekBuilder.SetConsumerId(ConsumerId);
			SeekBuilder.SetRequestId(RequestId);

			SeekBuilder.SetMessagePublishTime(Timestamp);

			CommandSeek Seek = SeekBuilder.Build();
			IByteBuffer Res = SerializeWithSize(BaseCommand.NewBuilder().SetType(BaseCommand.Types.Type.Seek).SetSeek(Seek));

			SeekBuilder.Recycle();
			Seek.Recycle();
			return Res;
		}

		public static IByteBuffer NewCloseConsumer(long ConsumerId, long RequestId)
		{
			CommandCloseConsumer.Builder CloseConsumerBuilder = CommandCloseConsumer.NewBuilder();
			CloseConsumerBuilder.SetConsumerId(ConsumerId);
			CloseConsumerBuilder.SetRequestId(RequestId);
			CommandCloseConsumer CloseConsumer = CloseConsumerBuilder.Build();
			IByteBuffer Res = SerializeWithSize(BaseCommand.NewBuilder().SetType(BaseCommand.Types.Type.CloseConsumer).SetCloseConsumer(CloseConsumer));
			CloseConsumerBuilder.Recycle();
			CloseConsumer.Recycle();
			return Res;
		}

		public static IByteBuffer NewReachedEndOfTopic(long ConsumerId)
		{
			CommandReachedEndOfTopic.Builder ReachedEndOfTopicBuilder = CommandReachedEndOfTopic.NewBuilder();
			ReachedEndOfTopicBuilder.SetConsumerId(ConsumerId);
			CommandReachedEndOfTopic ReachedEndOfTopic = ReachedEndOfTopicBuilder.Build();
			IByteBuffer Res = SerializeWithSize(BaseCommand.NewBuilder().SetType(BaseCommand.Types.Type.ReachedEndOfTopic).SetReachedEndOfTopic(ReachedEndOfTopic));
			ReachedEndOfTopicBuilder.Recycle();
			ReachedEndOfTopic.Recycle();
			return Res;
		}

		public static IByteBuffer NewCloseProducer(long ProducerId, long RequestId)
		{
			CommandCloseProducer.Builder CloseProducerBuilder = CommandCloseProducer.NewBuilder();
			CloseProducerBuilder.SetProducerId(ProducerId);
			CloseProducerBuilder.SetRequestId(RequestId);
			CommandCloseProducer CloseProducer = CloseProducerBuilder.Build();
			IByteBuffer Res = SerializeWithSize(BaseCommand.NewBuilder().SetType(BaseCommand.Types.Type.CloseProducer).SetCloseProducer(CloseProducerBuilder));
			CloseProducerBuilder.Recycle();
			CloseProducer.Recycle();
			return Res;
		}

		public static IByteBuffer NewProducer(string Topic, long ProducerId, long RequestId, string ProducerName, IDictionary<string, string> Metadata)
		{
			return NewProducer(Topic, ProducerId, RequestId, ProducerName, false, Metadata);
		}

		public static IByteBuffer NewProducer(string Topic, long ProducerId, long RequestId, string ProducerName, bool Encrypted, IDictionary<string, string> Metadata)
		{
			return NewProducer(Topic, ProducerId, RequestId, ProducerName, Encrypted, Metadata, null, 0, false);
		}

		private static Proto.Schema.Types.Type GetSchemaType(SchemaType Type)
		{
			if (Type.Value < 0)
			{
				return Proto.Schema.Types.Type.None;
			}
			else
			{
				return Enum.GetValues(typeof(Proto.Schema.Types.Type)).Cast<Proto.Schema.Types.Type>().ToList()[Type.Value];
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

		private static Proto.Schema GetSchema(SchemaInfo SchemaInfo)
		{
			Proto.Schema.Builder Builder = Proto.Schema.NewBuilder().SetName(SchemaInfo.Name).SetSchemaData(ByteString.CopyFrom((byte[])(Array)SchemaInfo.Schema)).SetType(GetSchemaType(SchemaInfo.Type)).AddAllProperties(SchemaInfo.Properties.ToList().Select(entry => KeyValue.NewBuilder().SetKey(entry.Key).SetValue(entry.Value).Build()).ToList());
			Proto.Schema Schema = Builder.Build();
			Builder.Recycle();
			return Schema;
		}

		public static IByteBuffer NewProducer(string Topic, long ProducerId, long RequestId, string ProducerName, bool Encrypted, IDictionary<string, string> Metadata, SchemaInfo SchemaInfo, long Epoch, bool UserProvidedProducerName)
		{
			CommandProducer.Builder ProducerBuilder = CommandProducer.NewBuilder();
			ProducerBuilder.SetTopic(Topic);
			ProducerBuilder.SetProducerId(ProducerId);
			ProducerBuilder.SetRequestId(RequestId);
			ProducerBuilder.SetEpoch(Epoch);
			if (!string.ReferenceEquals(ProducerName, null))
			{
				ProducerBuilder.SetProducerName(ProducerName);
			}
			ProducerBuilder.SetUserProvidedProducerName(UserProvidedProducerName);
			ProducerBuilder.SetEncrypted(Encrypted);

			ProducerBuilder.AddAllMetadata(CommandUtils.ToKeyValueList(Metadata));

			if (null != SchemaInfo)
			{
				ProducerBuilder.SetSchema(GetSchema(SchemaInfo));
			}

			CommandProducer Producer = ProducerBuilder.Build();
			IByteBuffer Res = SerializeWithSize(BaseCommand.NewBuilder().SetType(BaseCommand.Types.Type.Producer).SetProducer(Producer));
			ProducerBuilder.Recycle();
			Producer.Recycle();
			return Res;
		}

		public static IByteBuffer NewPartitionMetadataResponse(ServerError Error, string ErrorMsg, long RequestId)
		{
			CommandPartitionedTopicMetadataResponse.Builder PartitionMetadataResponseBuilder = CommandPartitionedTopicMetadataResponse.NewBuilder();
			PartitionMetadataResponseBuilder.SetRequestId(RequestId);
			PartitionMetadataResponseBuilder.SetError(Error);
			PartitionMetadataResponseBuilder.SetResponse(CommandPartitionedTopicMetadataResponse.Types.LookupType.Failed);
			if (!string.ReferenceEquals(ErrorMsg, null))
			{
				PartitionMetadataResponseBuilder.SetMessage(ErrorMsg);
			}

			CommandPartitionedTopicMetadataResponse PartitionMetadataResponse = PartitionMetadataResponseBuilder.Build();
			IByteBuffer Res = SerializeWithSize(BaseCommand.NewBuilder().SetType(BaseCommand.Types.Type.PartitionedMetadataResponse).SetPartitionMetadataResponse(PartitionMetadataResponse));
			PartitionMetadataResponseBuilder.Recycle();
			PartitionMetadataResponse.Recycle();
			return Res;
		}

		public static IByteBuffer NewPartitionMetadataRequest(string Topic, long RequestId)
		{
			CommandPartitionedTopicMetadata.Builder PartitionMetadataBuilder = CommandPartitionedTopicMetadata.NewBuilder();
			PartitionMetadataBuilder.SetTopic(Topic);
			PartitionMetadataBuilder.SetRequestId(RequestId);
			CommandPartitionedTopicMetadata PartitionMetadata = PartitionMetadataBuilder.Build();
			IByteBuffer Res = SerializeWithSize(BaseCommand.NewBuilder().SetType(BaseCommand.Types.Type.PartitionedMetadata).SetPartitionMetadata(PartitionMetadata));
			PartitionMetadataBuilder.Recycle();
			PartitionMetadata.Recycle();
			return Res;
		}

		public static IByteBuffer NewPartitionMetadataResponse(int Partitions, long RequestId)
		{
			CommandPartitionedTopicMetadataResponse.Builder PartitionMetadataResponseBuilder = CommandPartitionedTopicMetadataResponse.NewBuilder();
			PartitionMetadataResponseBuilder.SetPartitions(Partitions);
			PartitionMetadataResponseBuilder.SetResponse(CommandPartitionedTopicMetadataResponse.Types.LookupType.Success);
			PartitionMetadataResponseBuilder.SetRequestId(RequestId);

			CommandPartitionedTopicMetadataResponse PartitionMetadataResponse = PartitionMetadataResponseBuilder.Build();
			IByteBuffer Res = SerializeWithSize(BaseCommand.NewBuilder().SetType(BaseCommand.Types.Type.PartitionedMetadataResponse).SetPartitionMetadataResponse(PartitionMetadataResponse));
			PartitionMetadataResponseBuilder.Recycle();
			PartitionMetadataResponse.Recycle();
			return Res;
		}

		public static IByteBuffer NewLookup(string Topic, bool Authoritative, long RequestId)
		{
			CommandLookupTopic.Builder LookupTopicBuilder = CommandLookupTopic.NewBuilder();
			LookupTopicBuilder.SetTopic(Topic);
			LookupTopicBuilder.SetRequestId(RequestId);
			LookupTopicBuilder.SetAuthoritative(Authoritative);
			CommandLookupTopic LookupBroker = LookupTopicBuilder.Build();
			IByteBuffer Res = SerializeWithSize(BaseCommand.NewBuilder().SetType(BaseCommand.Types.Type.Lookup).SetLookupTopic(LookupBroker));
			LookupTopicBuilder.Recycle();
			LookupBroker.Recycle();
			return Res;
		}

		public static IByteBuffer NewLookupResponse(string BrokerServiceUrl, string BrokerServiceUrlTls, bool Authoritative, CommandLookupTopicResponse.Types.LookupType Response, long RequestId, bool ProxyThroughServiceUrl)
		{
			CommandLookupTopicResponse.Builder CommandLookupTopicResponseBuilder = CommandLookupTopicResponse.NewBuilder();
			CommandLookupTopicResponseBuilder.SetBrokerServiceUrl(BrokerServiceUrl);
			if (!string.ReferenceEquals(BrokerServiceUrlTls, null))
			{
				CommandLookupTopicResponseBuilder.SetBrokerServiceUrlTls(BrokerServiceUrlTls);
			}
			CommandLookupTopicResponseBuilder.SetResponse(Response);
			CommandLookupTopicResponseBuilder.SetRequestId(RequestId);
			CommandLookupTopicResponseBuilder.SetAuthoritative(Authoritative);
			CommandLookupTopicResponseBuilder.SetProxyThroughServiceUrl(ProxyThroughServiceUrl);

			CommandLookupTopicResponse commandLookupTopicResponse = CommandLookupTopicResponseBuilder.Build();
			IByteBuffer Res = SerializeWithSize(BaseCommand.NewBuilder().SetType(BaseCommand.Types.Type.LookupResponse).SetLookupTopicResponse(commandLookupTopicResponse));
			CommandLookupTopicResponseBuilder.Recycle();
			commandLookupTopicResponse.Recycle();
			return Res;
		}

		public static IByteBuffer NewLookupErrorResponse(ServerError Error, string ErrorMsg, long RequestId)
		{
			CommandLookupTopicResponse.Builder ConnectionBuilder = CommandLookupTopicResponse.NewBuilder();
			ConnectionBuilder.SetRequestId(RequestId);
			ConnectionBuilder.SetError(Error);
			if (!string.ReferenceEquals(ErrorMsg, null))
			{
				ConnectionBuilder.SetMessage(ErrorMsg);
			}
			ConnectionBuilder.SetResponse(CommandLookupTopicResponse.Types.LookupType.Failed);

			CommandLookupTopicResponse ConnectionBroker = ConnectionBuilder.Build();
			IByteBuffer Res = SerializeWithSize(BaseCommand.NewBuilder().SetType(BaseCommand.Types.Type.LookupResponse).SetLookupTopicResponse(ConnectionBroker));
			ConnectionBuilder.Recycle();
			ConnectionBroker.Recycle();
			return Res;
		}

		public static IByteBuffer NewMultiMessageAck(long ConsumerId, IList<KeyValuePair<long, long>> Entries)
		{
			CommandAck.Builder AckBuilder = CommandAck.NewBuilder();
			AckBuilder.SetConsumerId(ConsumerId);
			AckBuilder.SetAckType(CommandAck.Types.AckType.Individual);

			int EntriesCount = Entries.Count;
			for (int I = 0; I < EntriesCount; I++)
			{
				long LedgerId = Entries[I].Key;
				long EntryId = Entries[I].Value;

				MessageIdData.Builder MessageIdDataBuilder = MessageIdData.NewBuilder();
				MessageIdDataBuilder.LedgerId = LedgerId;
				MessageIdDataBuilder.EntryId = EntryId;
				MessageIdData messageIdData = MessageIdDataBuilder.Build();
				AckBuilder.AddMessageId(messageIdData);

				MessageIdDataBuilder.Recycle();
			}

			CommandAck Ack = AckBuilder.Build();

			IByteBuffer Res = SerializeWithSize(BaseCommand.NewBuilder().SetType(BaseCommand.Types.Type.Ack).SetAck(Ack));

			for (int I = 0; I < EntriesCount; I++)
			{
				Ack.GetMessageId(I).Recycle();
			}
			Ack.Recycle();
			AckBuilder.Recycle();
			return Res;
		}

		public static IByteBuffer NewAck(long ConsumerId, long LedgerId, long EntryId, CommandAck.Types.AckType AckType, CommandAck.Types.ValidationError ValidationError, IDictionary<string, long> Properties)
		{
			return NewAck(ConsumerId, LedgerId, EntryId, AckType, ValidationError, Properties, 0, 0);
		}

		public static IByteBuffer NewAck(long ConsumerId, long LedgerId, long EntryId, CommandAck.Types.AckType AckType, CommandAck.Types.ValidationError ValidationError, IDictionary<string, long> Properties, long TxnIdLeastBits, long TxnIdMostBits)
		{
			CommandAck.Builder AckBuilder = CommandAck.NewBuilder();
			AckBuilder.SetConsumerId(ConsumerId);
			AckBuilder.SetAckType(AckType);
			MessageIdData.Builder MessageIdDataBuilder = MessageIdData.NewBuilder();
			MessageIdDataBuilder.LedgerId = LedgerId;
			MessageIdDataBuilder.EntryId = EntryId;
			MessageIdData messageIdData = MessageIdDataBuilder.Build();
			AckBuilder.AddMessageId(messageIdData);
			if (ValidationError != null)
			{
				AckBuilder.SetValidationError(ValidationError);
			}
			if (TxnIdMostBits > 0)
			{
				AckBuilder.SetTxnidMostBits(TxnIdMostBits);
			}
			if (TxnIdLeastBits > 0)
			{
				AckBuilder.SetTxnidLeastBits(TxnIdLeastBits);
			}
			foreach (KeyValuePair<string, long> E in Properties.SetOfKeyValuePairs())
			{
				AckBuilder.AddProperties(KeyLongValue.NewBuilder().SetKey(E.Key).SetValue(E.Value).Build());
			}
			CommandAck Ack = AckBuilder.Build();

			IByteBuffer Res = SerializeWithSize(BaseCommand.NewBuilder().SetType(BaseCommand.Types.Type.Ack).SetAck(Ack));
			Ack.Recycle();
			AckBuilder.Recycle();
			MessageIdDataBuilder.Recycle();
			messageIdData.Recycle();
			return Res;
		}

		public static IByteBuffer NewAckResponse(long ConsumerId, long TxnIdLeastBits, long TxnIdMostBits)
		{
			CommandAckResponse.Builder CommandAckResponseBuilder = CommandAckResponse.NewBuilder();
			CommandAckResponseBuilder.SetConsumerId(ConsumerId);
			CommandAckResponseBuilder.SetTxnidLeastBits(TxnIdLeastBits);
			CommandAckResponseBuilder.SetTxnidMostBits(TxnIdMostBits);
			CommandAckResponse commandAckResponse = CommandAckResponseBuilder.Build();

			IByteBuffer Res = SerializeWithSize(BaseCommand.NewBuilder().SetType(BaseCommand.Types.Type.AckResponse).SetAckResponse(commandAckResponse));
			CommandAckResponseBuilder.Recycle();
			commandAckResponse.Recycle();

			return Res;
		}

		public static IByteBuffer NewAckErrorResponse(ServerError Error, string ErrorMsg, long ConsumerId)
		{
			CommandAckResponse.Builder AckErrorBuilder = CommandAckResponse.NewBuilder();
			AckErrorBuilder.SetConsumerId(ConsumerId);
			AckErrorBuilder.SetError(Error);
			if (!string.ReferenceEquals(ErrorMsg, null))
			{
				AckErrorBuilder.SetMessage(ErrorMsg);
			}

			CommandAckResponse Response = AckErrorBuilder.Build();
			IByteBuffer Res = SerializeWithSize(BaseCommand.NewBuilder().SetType(BaseCommand.Types.Type.AckResponse).SetAckResponse(Response));

			AckErrorBuilder.Recycle();
			Response.Recycle();

			return Res;
		}

		public static IByteBuffer NewFlow(long ConsumerId, int MessagePermits)
		{
			CommandFlow.Builder FlowBuilder = CommandFlow.NewBuilder();
			FlowBuilder.SetConsumerId(ConsumerId);
			FlowBuilder.SetMessagePermits(MessagePermits);
			CommandFlow Flow = FlowBuilder.Build();

			IByteBuffer Res = SerializeWithSize(BaseCommand.NewBuilder().SetType(BaseCommand.Types.Type.Flow).SetFlow(FlowBuilder));
			Flow.Recycle();
			FlowBuilder.Recycle();
			return Res;
		}

		public static IByteBuffer NewRedeliverUnacknowledgedMessages(long ConsumerId)
		{
			CommandRedeliverUnacknowledgedMessages.Builder RedeliverBuilder = CommandRedeliverUnacknowledgedMessages.NewBuilder();
			RedeliverBuilder.SetConsumerId(ConsumerId);
			CommandRedeliverUnacknowledgedMessages Redeliver = RedeliverBuilder.Build();
			IByteBuffer Res = SerializeWithSize(BaseCommand.NewBuilder().SetType(BaseCommand.Types.Type.RedeliverUnacknowledgedMessages).SetRedeliverUnacknowledgedMessages(RedeliverBuilder));
			Redeliver.Recycle();
			RedeliverBuilder.Recycle();
			return Res;
		}

		public static IByteBuffer NewRedeliverUnacknowledgedMessages(long ConsumerId, IList<MessageIdData> MessageIds)
		{
			CommandRedeliverUnacknowledgedMessages.Builder RedeliverBuilder = CommandRedeliverUnacknowledgedMessages.NewBuilder();
			RedeliverBuilder.SetConsumerId(ConsumerId);
			RedeliverBuilder.AddAllMessageIds(MessageIds);
			CommandRedeliverUnacknowledgedMessages Redeliver = RedeliverBuilder.Build();
			IByteBuffer Res = SerializeWithSize(BaseCommand.NewBuilder().SetType(BaseCommand.Types.Type.RedeliverUnacknowledgedMessages).SetRedeliverUnacknowledgedMessages(RedeliverBuilder));
			Redeliver.Recycle();
			RedeliverBuilder.Recycle();
			return Res;
		}

		public static IByteBuffer NewConsumerStatsResponse(ServerError ServerError, string ErrMsg, long RequestId)
		{
			CommandConsumerStatsResponse.Builder CommandConsumerStatsResponseBuilder = CommandConsumerStatsResponse.NewBuilder();
			CommandConsumerStatsResponseBuilder.SetRequestId(RequestId);
			CommandConsumerStatsResponseBuilder.SetErrorMessage(ErrMsg);
			CommandConsumerStatsResponseBuilder.SetErrorCode(ServerError);

			CommandConsumerStatsResponse commandConsumerStatsResponse = CommandConsumerStatsResponseBuilder.Build();
			IByteBuffer Res = SerializeWithSize(BaseCommand.NewBuilder().SetType(BaseCommand.Types.Type.ConsumerStatsResponse).SetConsumerStatsResponse(CommandConsumerStatsResponseBuilder));
			commandConsumerStatsResponse.Recycle();
			CommandConsumerStatsResponseBuilder.Recycle();
			return Res;
		}

		public static IByteBuffer NewConsumerStatsResponse(CommandConsumerStatsResponse.Builder Builder)
		{
			CommandConsumerStatsResponse CommandConsumerStatsResponse = Builder.Build();
			IByteBuffer Res = SerializeWithSize(BaseCommand.NewBuilder().SetType(BaseCommand.Types.Type.ConsumerStatsResponse).SetConsumerStatsResponse(Builder));
			CommandConsumerStatsResponse.Recycle();
			Builder.Recycle();
			return Res;
		}

		public static IByteBuffer NewGetTopicsOfNamespaceRequest(string Namespace, long RequestId, CommandGetTopicsOfNamespace.Types.Mode Mode)
		{
			CommandGetTopicsOfNamespace.Builder TopicsBuilder = CommandGetTopicsOfNamespace.NewBuilder();
			TopicsBuilder.SetNamespace(Namespace).SetRequestId(RequestId).SetMode(Mode);

			CommandGetTopicsOfNamespace TopicsCommand = TopicsBuilder.Build();
			IByteBuffer Res = SerializeWithSize(BaseCommand.NewBuilder().SetType(BaseCommand.Types.Type.GetTopicsOfNamespace).SetGetTopicsOfNamespace(TopicsCommand));
			TopicsBuilder.Recycle();
			TopicsCommand.Recycle();
			return Res;
		}

		public static IByteBuffer NewGetTopicsOfNamespaceResponse(IList<string> Topics, long RequestId)
		{
			CommandGetTopicsOfNamespaceResponse.Builder TopicsResponseBuilder = CommandGetTopicsOfNamespaceResponse.NewBuilder();

			TopicsResponseBuilder.SetRequestId(RequestId).AddAllTopics(Topics);

			CommandGetTopicsOfNamespaceResponse TopicsOfNamespaceResponse = TopicsResponseBuilder.Build();
			IByteBuffer Res = SerializeWithSize(BaseCommand.NewBuilder().SetType(BaseCommand.Types.Type.GetTopicsOfNamespaceResponse).SetGetTopicsOfNamespaceResponse(TopicsOfNamespaceResponse));

			TopicsResponseBuilder.Recycle();
			TopicsOfNamespaceResponse.Recycle();
			return Res;
		}

		private static readonly IByteBuffer cmdPing;

		static Commands()
		{
			IByteBuffer SerializedCmdPing = SerializeWithSize(BaseCommand.NewBuilder().SetType(BaseCommand.Types.Type.Ping).SetPing(CommandPing.DefaultInstance));
			cmdPing = Unpooled.CopiedBuffer(SerializedCmdPing);
			SerializedCmdPing.Release();
			IByteBuffer SerializedCmdPong = SerializeWithSize(BaseCommand.NewBuilder().SetType(BaseCommand.Types.Type.Pong).SetPong(CommandPong.DefaultInstance));
			cmdPong = Unpooled.CopiedBuffer(SerializedCmdPong);
			SerializedCmdPong.Release();
		}

		internal static IByteBuffer NewPing()
		{
			return cmdPing.RetainedDuplicate();
		}

		private static readonly IByteBuffer cmdPong;


		internal static IByteBuffer NewPong()
		{
			return cmdPong.RetainedDuplicate();
		}

		public static IByteBuffer NewGetLastMessageId(long ConsumerId, long RequestId)
		{
			CommandGetLastMessageId.Builder CmdBuilder = CommandGetLastMessageId.NewBuilder();
			CmdBuilder.SetConsumerId(ConsumerId).SetRequestId(RequestId);

			IByteBuffer Res = SerializeWithSize(BaseCommand.NewBuilder().SetType(BaseCommand.Types.Type.GetLastMessageId).SetGetLastMessageId(CmdBuilder.Build()));
			CmdBuilder.Recycle();
			return Res;
		}

		public static IByteBuffer NewGetLastMessageIdResponse(long RequestId, MessageIdData MessageIdData)
		{
			CommandGetLastMessageIdResponse.Builder Response = CommandGetLastMessageIdResponse.NewBuilder().SetLastMessageId(MessageIdData).SetRequestId(RequestId);

			IByteBuffer Res = SerializeWithSize(BaseCommand.NewBuilder().SetType(BaseCommand.Types.Type.GetLastMessageIdResponse).SetGetLastMessageIdResponse(Response.Build()));
			Response.Recycle();
			return Res;
		}

		public static IByteBuffer NewGetSchema(long RequestId, string Topic, SchemaVersion Version)
		{
			CommandGetSchema.Builder Schema = CommandGetSchema.NewBuilder().SetRequestId(RequestId);
			Schema.SetTopic(Topic);
			if (Version != null)
			{
				Schema.SetSchemaVersion(ByteString.CopyFrom((byte[])(object)Version.Bytes()));
			}

			CommandGetSchema GetSchema = Schema.Build();

			IByteBuffer Res = SerializeWithSize(BaseCommand.NewBuilder().SetType(BaseCommand.Types.Type.GetSchema).SetGetSchema(GetSchema));
			Schema.Recycle();
			return Res;
		}

		public static IByteBuffer NewGetSchemaResponse(long RequestId, CommandGetSchemaResponse Response)
		{
			CommandGetSchemaResponse.Builder SchemaResponseBuilder = CommandGetSchemaResponse.NewBuilder(Response).SetRequestId(RequestId);

			IByteBuffer Res = SerializeWithSize(BaseCommand.NewBuilder().SetType(BaseCommand.Types.Type.GetSchemaResponse).SetGetSchemaResponse(SchemaResponseBuilder.Build()));
			SchemaResponseBuilder.Recycle();
			return Res;
		}

		public static IByteBuffer NewGetSchemaResponse(long RequestId, SchemaInfo Schema, SchemaVersion Version)
		{
			CommandGetSchemaResponse.Builder SchemaResponse = CommandGetSchemaResponse.NewBuilder().SetRequestId(RequestId).SetSchemaVersion(ByteString.CopyFrom((byte[])(object)Version.Bytes())).SetSchema(GetSchema(Schema));

			IByteBuffer Res = SerializeWithSize(BaseCommand.NewBuilder().SetType(BaseCommand.Types.Type.GetSchemaResponse).SetGetSchemaResponse(SchemaResponse.Build()));
			SchemaResponse.Recycle();
			return Res;
		}

		public static IByteBuffer NewGetSchemaResponseError(long RequestId, ServerError Error, string ErrorMessage)
		{
			CommandGetSchemaResponse.Builder SchemaResponse = CommandGetSchemaResponse.NewBuilder().SetRequestId(RequestId).SetErrorCode(Error).SetErrorMessage(ErrorMessage);

			IByteBuffer Res = SerializeWithSize(BaseCommand.NewBuilder().SetType(BaseCommand.Types.Type.GetSchemaResponse).SetGetSchemaResponse(SchemaResponse.Build()));
			SchemaResponse.Recycle();
			return Res;
		}

		public static IByteBuffer NewGetOrCreateSchema(long RequestId, string Topic, SchemaInfo SchemaInfo)
		{
			CommandGetOrCreateSchema GetOrCreateSchema = CommandGetOrCreateSchema.NewBuilder().SetRequestId(RequestId).SetTopic(Topic).SetSchema(GetSchema(SchemaInfo).ToBuilder()).Build();
			IByteBuffer Res = SerializeWithSize(BaseCommand.NewBuilder().SetType(BaseCommand.Types.Type.GetOrCreateSchema).SetGetOrCreateSchema(GetOrCreateSchema));
			GetOrCreateSchema.Recycle();
			return Res;
		}

		public static IByteBuffer NewGetOrCreateSchemaResponse(long RequestId, SchemaVersion SchemaVersion)
		{
			CommandGetOrCreateSchemaResponse.Builder SchemaResponse = CommandGetOrCreateSchemaResponse.NewBuilder().SetRequestId(RequestId).SetSchemaVersion(ByteString.CopyFrom((byte[])(object)SchemaVersion.Bytes()));
			IByteBuffer Res = SerializeWithSize(BaseCommand.NewBuilder().SetType(BaseCommand.Types.Type.GetOrCreateSchemaResponse).SetGetOrCreateSchemaResponse(SchemaResponse.Build()));
			SchemaResponse.Recycle();
			return Res;
		}

		public static IByteBuffer NewGetOrCreateSchemaResponseError(long RequestId, ServerError Error, string ErrorMessage)
		{
			CommandGetOrCreateSchemaResponse.Builder SchemaResponse = CommandGetOrCreateSchemaResponse.NewBuilder().SetRequestId(RequestId).SetErrorCode(Error).SetErrorMessage(ErrorMessage);
			IByteBuffer Res = SerializeWithSize(BaseCommand.NewBuilder().SetType(BaseCommand.Types.Type.GetOrCreateSchemaResponse).SetGetOrCreateSchemaResponse(SchemaResponse.Build()));
			SchemaResponse.Recycle();
			return Res;
		}

		// ---- transaction related ----

		public static IByteBuffer NewTxn(long TcId, long RequestId, long TtlSeconds)
		{
			CommandNewTxn CommandNewTxn = CommandNewTxn.NewBuilder().SetTcId(TcId).SetRequestId(RequestId).SetTxnTtlSeconds(TtlSeconds).Build();
			IByteBuffer Res = SerializeWithSize(BaseCommand.NewBuilder().SetType(BaseCommand.Types.Type.NewTxn).SetNewTxn(CommandNewTxn));
			CommandNewTxn.Recycle();
			return Res;
		}

		public static IByteBuffer NewTxnResponse(long RequestId, long TxnIdLeastBits, long TxnIdMostBits)
		{
			CommandNewTxnResponse CommandNewTxnResponse = CommandNewTxnResponse.NewBuilder().SetRequestId(RequestId).SetTxnidMostBits(TxnIdMostBits).SetTxnidLeastBits(TxnIdLeastBits).Build();
			IByteBuffer Res = SerializeWithSize(BaseCommand.NewBuilder().SetType(BaseCommand.Types.Type.NewTxnResponse).SetNewTxnResponse(CommandNewTxnResponse));
			CommandNewTxnResponse.Recycle();

			return Res;
		}

		public static IByteBuffer NewTxnResponse(long RequestId, long TxnIdMostBits, ServerError Error, string ErrorMsg)
		{
			CommandNewTxnResponse.Builder Builder = CommandNewTxnResponse.NewBuilder();
			Builder.SetRequestId(RequestId);
			Builder.SetTxnidMostBits(TxnIdMostBits);
			Builder.SetError(Error);
			if (!string.ReferenceEquals(ErrorMsg, null))
			{
				Builder.SetMessage(ErrorMsg);
			}
			CommandNewTxnResponse ErrorResponse = Builder.Build();
			IByteBuffer Res = SerializeWithSize(BaseCommand.NewBuilder().SetType(BaseCommand.Types.Type.NewTxnResponse).SetNewTxnResponse(ErrorResponse));
			Builder.Recycle();
			ErrorResponse.Recycle();

			return Res;
		}

		public static IByteBuffer NewAddPartitionToTxn(long RequestId, long TxnIdLeastBits, long TxnIdMostBits)
		{
			CommandAddPartitionToTxn CommandAddPartitionToTxn = CommandAddPartitionToTxn.NewBuilder().SetRequestId(RequestId).SetTxnidLeastBits(TxnIdLeastBits).SetTxnidMostBits(TxnIdMostBits).Build();
			IByteBuffer Res = SerializeWithSize(BaseCommand.NewBuilder().SetType(BaseCommand.Types.Type.AddPartitionToTxn).SetAddPartitionToTxn(CommandAddPartitionToTxn));
			CommandAddPartitionToTxn.Recycle();
			return Res;
		}

		public static IByteBuffer NewAddPartitionToTxnResponse(long RequestId, long TxnIdLeastBits, long TxnIdMostBits)
		{
			CommandAddPartitionToTxnResponse CommandAddPartitionToTxnResponse = CommandAddPartitionToTxnResponse.NewBuilder().SetRequestId(RequestId).SetTxnidLeastBits(TxnIdLeastBits).SetTxnidMostBits(TxnIdMostBits).Build();
			IByteBuffer Res = SerializeWithSize(BaseCommand.NewBuilder().SetType(BaseCommand.Types.Type.AddPartitionToTxnResponse).SetAddPartitionToTxnResponse(CommandAddPartitionToTxnResponse));
			CommandAddPartitionToTxnResponse.Recycle();
			return Res;
		}

		public static IByteBuffer NewAddPartitionToTxnResponse(long RequestId, long TxnIdMostBits, ServerError Error, string ErrorMsg)
		{
			CommandAddPartitionToTxnResponse.Builder Builder = CommandAddPartitionToTxnResponse.NewBuilder();
			Builder.SetRequestId(RequestId);
			Builder.SetTxnidMostBits(TxnIdMostBits);
			Builder.SetError(Error);
			if (!string.ReferenceEquals(ErrorMsg, null))
			{
				Builder.SetMessage(ErrorMsg);
			}
			CommandAddPartitionToTxnResponse commandAddPartitionToTxnResponse = Builder.Build();
			IByteBuffer Res = SerializeWithSize(BaseCommand.NewBuilder().SetType(BaseCommand.Types.Type.AddPartitionToTxnResponse).SetAddPartitionToTxnResponse(commandAddPartitionToTxnResponse));
			Builder.Recycle();
			commandAddPartitionToTxnResponse.Recycle();
			return Res;
		}

		public static IByteBuffer NewAddSubscriptionToTxn(long RequestId, long TxnIdLeastBits, long TxnIdMostBits, IList<Subscription> Subscription)
		{
			CommandAddSubscriptionToTxn CommandAddSubscriptionToTxn = CommandAddSubscriptionToTxn.NewBuilder().SetRequestId(RequestId).SetTxnidLeastBits(TxnIdLeastBits).SetTxnidMostBits(TxnIdMostBits).AddAllSubscription(Subscription).Build();
			IByteBuffer Res = SerializeWithSize(BaseCommand.NewBuilder().SetType(BaseCommand.Types.Type.AddSubscriptionToTxn).SetAddSubscriptionToTxn(CommandAddSubscriptionToTxn));
			CommandAddSubscriptionToTxn.Recycle();
			return Res;
		}

		public static IByteBuffer NewAddSubscriptionToTxnResponse(long RequestId, long TxnIdLeastBits, long TxnIdMostBits)
		{
			CommandAddSubscriptionToTxnResponse Command = CommandAddSubscriptionToTxnResponse.NewBuilder().SetRequestId(RequestId).SetTxnidLeastBits(TxnIdLeastBits).SetTxnidMostBits(TxnIdMostBits).Build();
			IByteBuffer Res = SerializeWithSize(BaseCommand.NewBuilder().SetType(BaseCommand.Types.Type.AddSubscriptionToTxnResponse).SetAddSubscriptionToTxnResponse(Command));
			Command.Recycle();
			return Res;
		}

		public static IByteBuffer NewAddSubscriptionToTxnResponse(long RequestId, long TxnIdMostBits, ServerError Error, string ErrorMsg)
		{
			CommandAddSubscriptionToTxnResponse.Builder Builder = CommandAddSubscriptionToTxnResponse.NewBuilder();
			Builder.SetRequestId(RequestId);
			Builder.SetTxnidMostBits(TxnIdMostBits);
			Builder.SetError(Error);
			if (!string.ReferenceEquals(ErrorMsg, null))
			{
				Builder.SetMessage(ErrorMsg);
			}
			CommandAddSubscriptionToTxnResponse ErrorResponse = Builder.Build();
			IByteBuffer Res = SerializeWithSize(BaseCommand.NewBuilder().SetType(BaseCommand.Types.Type.AddSubscriptionToTxnResponse).SetAddSubscriptionToTxnResponse(ErrorResponse));
			Builder.Recycle();
			ErrorResponse.Recycle();
			return Res;
		}

		public static IByteBuffer NewEndTxn(long RequestId, long TxnIdLeastBits, long TxnIdMostBits, TxnAction TxnAction)
		{
			CommandEndTxn CommandEndTxn = CommandEndTxn.NewBuilder().SetRequestId(RequestId).SetTxnidLeastBits(TxnIdLeastBits).SetTxnidMostBits(TxnIdMostBits).SetTxnAction(TxnAction).Build();
			IByteBuffer Res = SerializeWithSize(BaseCommand.NewBuilder().SetType(BaseCommand.Types.Type.EndTxn).SetEndTxn(CommandEndTxn));
			CommandEndTxn.Recycle();
			return Res;
		}

		public static IByteBuffer NewEndTxnResponse(long RequestId, long TxnIdLeastBits, long TxnIdMostBits)
		{
			CommandEndTxnResponse CommandEndTxnResponse = CommandEndTxnResponse.NewBuilder().SetRequestId(RequestId).SetTxnidLeastBits(TxnIdLeastBits).SetTxnidMostBits(TxnIdMostBits).Build();
			IByteBuffer Res = SerializeWithSize(BaseCommand.NewBuilder().SetType(BaseCommand.Types.Type.EndTxnResponse).SetEndTxnResponse(CommandEndTxnResponse));
			CommandEndTxnResponse.Recycle();
			return Res;
		}

		public static IByteBuffer NewEndTxnResponse(long RequestId, long TxnIdMostBits, ServerError Error, string ErrorMsg)
		{
			CommandEndTxnResponse.Builder Builder = CommandEndTxnResponse.NewBuilder();
			Builder.SetRequestId(RequestId);
			Builder.SetTxnidMostBits(TxnIdMostBits);
			Builder.SetError(Error);
			if (!string.ReferenceEquals(ErrorMsg, null))
			{
				Builder.SetMessage(ErrorMsg);
			}
			CommandEndTxnResponse commandEndTxnResponse = Builder.Build();
			IByteBuffer Res = SerializeWithSize(BaseCommand.NewBuilder().SetType(BaseCommand.Types.Type.EndTxnResponse).SetEndTxnResponse(commandEndTxnResponse));
			Builder.Recycle();
			commandEndTxnResponse.Recycle();
			return Res;
		}

		public static IByteBuffer NewEndTxnOnPartition(long RequestId, long TxnIdLeastBits, long TxnIdMostBits, string Topic, TxnAction TxnAction)
		{
			CommandEndTxnOnPartition.Builder TxnEndOnPartition = CommandEndTxnOnPartition.NewBuilder().SetRequestId(RequestId).SetTxnidLeastBits(TxnIdLeastBits).SetTxnidMostBits(TxnIdMostBits).SetTopic(Topic).SetTxnAction(TxnAction);
			IByteBuffer Res = SerializeWithSize(BaseCommand.NewBuilder().SetType(BaseCommand.Types.Type.EndTxnOnPartition).SetEndTxnOnPartition(TxnEndOnPartition));
			TxnEndOnPartition.Recycle();
			return Res;
		}

		public static IByteBuffer NewEndTxnOnPartitionResponse(long RequestId, long TxnIdLeastBits, long TxnIdMostBits)
		{
			CommandEndTxnOnPartitionResponse CommandEndTxnOnPartitionResponse = CommandEndTxnOnPartitionResponse.NewBuilder().SetRequestId(RequestId).SetTxnidLeastBits(TxnIdLeastBits).SetTxnidMostBits(TxnIdMostBits).Build();
			IByteBuffer Res = SerializeWithSize(BaseCommand.NewBuilder().SetType(BaseCommand.Types.Type.EndTxnOnPartitionResponse).SetEndTxnOnPartitionResponse(CommandEndTxnOnPartitionResponse));
			CommandEndTxnOnPartitionResponse.Recycle();
			return Res;
		}

		public static IByteBuffer NewEndTxnOnPartitionResponse(long RequestId, ServerError Error, string ErrorMsg)
		{
			CommandEndTxnOnPartitionResponse.Builder Builder = CommandEndTxnOnPartitionResponse.NewBuilder();
			Builder.SetRequestId(RequestId);
			Builder.SetError(Error);
			if (!string.ReferenceEquals(ErrorMsg, null))
			{
				Builder.SetMessage(ErrorMsg);
			}
			CommandEndTxnOnPartitionResponse commandEndTxnOnPartitionResponse = Builder.Build();
			IByteBuffer Res = SerializeWithSize(BaseCommand.NewBuilder().SetType(BaseCommand.Types.Type.EndTxnOnPartitionResponse).SetEndTxnOnPartitionResponse(commandEndTxnOnPartitionResponse));
			Builder.Recycle();
			commandEndTxnOnPartitionResponse.Recycle();
			return Res;
		}

		public static IByteBuffer NewEndTxnOnSubscription(long RequestId, long TxnIdLeastBits, long TxnIdMostBits, Subscription Subscription, TxnAction TxnAction)
		{
			CommandEndTxnOnSubscription CommandEndTxnOnSubscription = CommandEndTxnOnSubscription.NewBuilder().SetRequestId(RequestId).SetTxnidLeastBits(TxnIdLeastBits).SetTxnidMostBits(TxnIdMostBits).SetSubscription(Subscription).SetTxnAction(TxnAction).Build();
			IByteBuffer Res = SerializeWithSize(BaseCommand.NewBuilder().SetType(BaseCommand.Types.Type.EndTxnOnSubscription).SetEndTxnOnSubscription(CommandEndTxnOnSubscription));
			CommandEndTxnOnSubscription.Recycle();
			return Res;
		}

		public static IByteBuffer NewEndTxnOnSubscriptionResponse(long RequestId, long TxnIdLeastBits, long TxnIdMostBits)
		{
			CommandEndTxnOnSubscriptionResponse Response = CommandEndTxnOnSubscriptionResponse.NewBuilder().SetRequestId(RequestId).SetTxnidLeastBits(TxnIdLeastBits).SetTxnidMostBits(TxnIdMostBits).Build();
			IByteBuffer Res = SerializeWithSize(BaseCommand.NewBuilder().SetType(BaseCommand.Types.Type.EndTxnOnSubscriptionResponse).SetEndTxnOnSubscriptionResponse(Response));
			Response.Recycle();
			return Res;
		}

		public static IByteBuffer NewEndTxnOnSubscriptionResponse(long RequestId, ServerError Error, string ErrorMsg)
		{
			CommandEndTxnOnSubscriptionResponse.Builder Builder = CommandEndTxnOnSubscriptionResponse.NewBuilder();
			Builder.SetRequestId(RequestId);
			Builder.SetError(Error);
			if (!string.ReferenceEquals(ErrorMsg, null))
			{
				Builder.SetMessage(ErrorMsg);
			}
			CommandEndTxnOnSubscriptionResponse Response = Builder.Build();
			IByteBuffer Res = SerializeWithSize(BaseCommand.NewBuilder().SetType(BaseCommand.Types.Type.EndTxnOnSubscriptionResponse).SetEndTxnOnSubscriptionResponse(Response));
			Builder.Recycle();
			Response.Recycle();
			return Res;
		}

		public static IByteBuffer SerializeWithSize(BaseCommand.Builder cmdBuilder)
		{
			// / Wire format
			// [TOTAL_SIZE] [CMD_SIZE][CMD]
			var cmd = cmdBuilder.Build();

			int cmdSize = cmd.CalculateSize();
			int totalSize = cmdSize + 4;
			int frameSize = totalSize + 4;

			IByteBuffer buf = PulsarByteBufAllocator.DEFAULT.Buffer(frameSize, frameSize);

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

		private static ByteBufPair SerializeCommandSendWithSize(BaseCommand.Builder CmdBuilder, ChecksumType ChecksumType, MessageMetadata MsgMetadata, IByteBuffer Payload)
		{
			// / Wire format
			// [TOTAL_SIZE] [CMD_SIZE][CMD] [MAGIC_NUMBER][CHECKSUM] [METADATA_SIZE][METADATA] [PAYLOAD]

			BaseCommand Cmd = CmdBuilder.Build();
			int CmdSize = Cmd.SerializedSize;
			int MsgMetadataSize = MsgMetadata.CalculateSize();
			int PayloadSize = Payload.ReadableBytes;
			int MagicAndChecksumLength = ChecksumType.Crc32c.Equals(ChecksumType) ? (2 + 4) : 0;
			bool IncludeChecksum = MagicAndChecksumLength > 0;
			// cmdLength + cmdSize + magicLength +
			// checksumSize + msgMetadataLength +
			// msgMetadataSize
			int HeaderContentSize = 4 + CmdSize + MagicAndChecksumLength + 4 + MsgMetadataSize;
			int TotalSize = HeaderContentSize + PayloadSize;
			int HeadersSize = 4 + HeaderContentSize; // totalSize + headerLength
			int ChecksumReaderIndex = -1;

			IByteBuffer Headers = PulsarByteBufAllocator.DEFAULT.Buffer(HeadersSize, HeadersSize);
			Headers.WriteInt(TotalSize); // External frame

			try
			{
				// Write cmd
				Headers.WriteInt(CmdSize);

				ByteBufCodedOutputStream OutStream = ByteBufCodedOutputStream.Get(Headers);
				Cmd.WriteTo(OutStream);
				Cmd.Recycle();
				CmdBuilder.Recycle();

				//Create checksum placeholder
				if (IncludeChecksum)
				{
					Headers.WriteShort(MagicCrc32c);
					ChecksumReaderIndex = Headers.WriterIndex;
					Headers.SetWriterIndex(Headers.WriterIndex + ChecksumSize); //skip 4 bytes of checksum
				}

				// Write metadata
				Headers.WriteInt(MsgMetadataSize);
				MsgMetadata.WriteTo(OutStream);
				OutStream.Recycle();
			}
			catch (IOException E)
			{
				// This is in-memory serialization, should not fail
				throw new System.Exception(E);
			}

			ByteBufPair Command = ByteBufPair.Get(Headers, Payload);

			// write checksum at created checksum-placeholder
			if (IncludeChecksum)
			{
				Headers.MarkReaderIndex();
				Headers.SetReaderIndex(ChecksumReaderIndex + ChecksumSize);
				int MetadataChecksum = ComputeChecksum(Headers);
				int ComputedChecksum = ResumeChecksum(MetadataChecksum, Payload);
				// set computed checksum
				Headers.SetInt(ChecksumReaderIndex, ComputedChecksum);
				Headers.ResetReaderIndex();
			}
			return Command;
		}

		public static IByteBuffer SerializeMetadataAndPayload(ChecksumType ChecksumType, MessageMetadata MsgMetadata, IByteBuffer Payload)
		{
			// / Wire format
			// [MAGIC_NUMBER][CHECKSUM] [METADATA_SIZE][METADATA] [PAYLOAD]
			int MsgMetadataSize = MsgMetadata.SerializedSize;
			int PayloadSize = Payload.ReadableBytes;
			int MagicAndChecksumLength = ChecksumType.Crc32c.Equals(ChecksumType) ? (2 + 4) : 0;
			bool IncludeChecksum = MagicAndChecksumLength > 0;
			int HeaderContentSize = MagicAndChecksumLength + 4 + MsgMetadataSize; // magicLength +
																				  // checksumSize + msgMetadataLength +
																				  // msgMetadataSize
			int ChecksumReaderIndex = -1;
			int TotalSize = HeaderContentSize + PayloadSize;

			IByteBuffer MetadataAndPayload = PulsarByteBufAllocator.DEFAULT.Buffer(TotalSize, TotalSize);
			try
			{
				ByteBufCodedOutputStream OutStream = ByteBufCodedOutputStream.Get(MetadataAndPayload);

				//Create checksum placeholder
				if (IncludeChecksum)
				{
					MetadataAndPayload.WriteShort(MagicCrc32c);
					ChecksumReaderIndex = MetadataAndPayload.WriterIndex;
					MetadataAndPayload.SetWriterIndex(MetadataAndPayload.WriterIndex + ChecksumSize); //skip 4 bytes of checksum
				}

				// Write metadata
				MetadataAndPayload.WriteInt(MsgMetadataSize);
				MsgMetadata.WriteTo(OutStream);
				OutStream.Recycle();
			}
			catch (IOException E)
			{
				// This is in-memory serialization, should not fail
				throw new System.Exception(E.Message, E);
			}

			// write checksum at created checksum-placeholder
			if (IncludeChecksum)
			{
				MetadataAndPayload.MarkReaderIndex();
				MetadataAndPayload.SetReaderIndex(ChecksumReaderIndex + ChecksumSize);
				int MetadataChecksum = ComputeChecksum(MetadataAndPayload);
				int ComputedChecksum = ResumeChecksum(MetadataChecksum, Payload);
				// set computed checksum
				MetadataAndPayload.SetInt(ChecksumReaderIndex, ComputedChecksum);
				MetadataAndPayload.ResetReaderIndex();
			}
			MetadataAndPayload.WriteBytes(Payload);

			return MetadataAndPayload;
		}

		public static long InitBatchMessageMetadata(MessageMetadata.Builder Builder)
		{
			MessageMetadata.Builder messageMetadata = MessageMetadata.NewBuilder();
			messageMetadata.SetPublishTime(Builder._publishTime);
			messageMetadata.SetProducerName(Builder.GetProducerName());
			messageMetadata.SetSequenceId(Builder._sequenceId);
			if (Builder.HasReplicatedFrom())
			{
				messageMetadata.SetReplicatedFrom(Builder.getReplicatedFrom());
			}
			if (Builder.ReplicateToCount > 0)
			{
				messageMetadata.AddAllReplicateTo(Builder.ReplicateToList);
			}
			if (Builder.HasSchemaVersion())
			{
				messageMetadata.SetSchemaVersion(Builder._schemaVersion);
			}
			return Builder._sequenceId;
		}

		public static IByteBuffer SerializeSingleMessageInBatchWithPayload(SingleMessageMetadata.Builder SingleMessageMetadataBuilder, IByteBuffer Payload, IByteBuffer BatchBuffer)
		{
			int PayLoadSize = Payload.ReadableBytes;
			SingleMessageMetadata SingleMessageMetadata = SingleMessageMetadataBuilder.SetPayloadSize(PayLoadSize).Build();
			// serialize meta-data size, meta-data and payload for single message in batch
			int SingleMsgMetadataSize = SingleMessageMetadata.CalculateSize();
			try
			{
				BatchBuffer.WriteInt(SingleMsgMetadataSize);
				ByteBufCodedOutputStream OutStream = ByteBufCodedOutputStream.Get(BatchBuffer);
				SingleMessageMetadata.WriteTo(OutStream);
				SingleMessageMetadata.Recycle();
				OutStream.Recycle();
			}
			catch (IOException E)
			{
				throw new System.Exception(E.Message, E);
			}
			return BatchBuffer.WriteBytes(Payload);
		}

		public static IByteBuffer SerializeSingleMessageInBatchWithPayload(MessageMetadata.Builder MsgBuilder, IByteBuffer Payload, IByteBuffer BatchBuffer)
		{

			// build single message meta-data
			SingleMessageMetadata.Builder SingleMessageMetadataBuilder = SingleMessageMetadata.NewBuilder();
			if (MsgBuilder.HasPartitionKey())
			{
				SingleMessageMetadataBuilder = SingleMessageMetadataBuilder.SetPartitionKey(MsgBuilder.getPartitionKey()).SetPartitionKeyB64Encoded(MsgBuilder.PartitionKeyB64Encoded);
			}
			if (MsgBuilder.HasOrderingKey())
			{
				SingleMessageMetadataBuilder = SingleMessageMetadataBuilder.SetOrderingKey(MsgBuilder.OrderingKey);
			}
			if (MsgBuilder.PropertiesList.Count > 0)
			{
				SingleMessageMetadataBuilder = SingleMessageMetadataBuilder.addAllProperties(MsgBuilder.PropertiesList);
			}

			if (MsgBuilder.HasEventTime())
			{
				SingleMessageMetadataBuilder.EventTime = MsgBuilder.EventTime;
			}

			if (MsgBuilder.HasSequenceId())
			{
				SingleMessageMetadataBuilder.SequenceId = MsgBuilder.SequenceId;
			}

			try
			{
				return SerializeSingleMessageInBatchWithPayload(SingleMessageMetadataBuilder, Payload, BatchBuffer);
			}
			finally
			{
				SingleMessageMetadataBuilder.Recycle();
			}
		}

		public static IByteBuffer DeSerializeSingleMessageInBatch(IByteBuffer UncompressedPayload, SingleMessageMetadata.Builder SingleMessageMetadataBuilder, int Index, int BatchSize)
		{
			int SingleMetaSize = (int) UncompressedPayload.ReadUnsignedInt();
			int WriterIndex = UncompressedPayload.WriterIndex;
			int BeginIndex = UncompressedPayload.ReaderIndex + SingleMetaSize;
			UncompressedPayload.SetWriterIndex(BeginIndex);
			ByteBufCodedInputStream Stream = ByteBufCodedInputStream.Get(UncompressedPayload);
			SingleMessageMetadataBuilder.MergeFrom(Stream, null);
			Stream.Recycle();

			int SingleMessagePayloadSize = SingleMessageMetadataBuilder.PayloadSize;

			int ReaderIndex = UncompressedPayload.ReaderIndex;
			IByteBuffer SingleMessagePayload = UncompressedPayload.RetainedSlice(ReaderIndex, SingleMessagePayloadSize);
			UncompressedPayload.SetWriterIndex(WriterIndex);

			// reader now points to beginning of payload read; so move it past message payload just read
			if (Index < BatchSize)
			{
				UncompressedPayload.SetReaderIndex(ReaderIndex + SingleMessagePayloadSize);
			}

			return SingleMessagePayload;
		}

		private static ByteBufPair SerializeCommandMessageWithSize(BaseCommand Cmd, IByteBuffer MetadataAndPayload)
		{
			// / Wire format
			// [TOTAL_SIZE] [CMD_SIZE][CMD] [MAGIC_NUMBER][CHECKSUM] [METADATA_SIZE][METADATA] [PAYLOAD]
			//
			// metadataAndPayload contains from magic-number to the payload included


			int CmdSize = Cmd.SerializedSize;
			int TotalSize = 4 + CmdSize + MetadataAndPayload.ReadableBytes;
			int HeadersSize = 4 + 4 + CmdSize;

			IByteBuffer Headers = PulsarByteBufAllocator.DEFAULT.Buffer(HeadersSize);
			Headers.WriteInt(TotalSize); // External frame

			try
			{
				// Write cmd
				Headers.WriteInt(CmdSize);

				ByteBufCodedOutputStream OutStream = ByteBufCodedOutputStream.Get(Headers);
				Cmd.WriteTo(OutStream);
				OutStream.Recycle();
			}
			catch (IOException E)
			{
				// This is in-memory serialization, should not fail
				throw new System.Exception(E);
			}

			return (ByteBufPair) ByteBufPair.Get(Headers, MetadataAndPayload);
		}

		public static int GetNumberOfMessagesInBatch(IByteBuffer MetadataAndPayload, string Subscription, long ConsumerId)
		{
			MessageMetadata MsgMetadata = PeekMessageMetadata(MetadataAndPayload, Subscription, ConsumerId);
			if (MsgMetadata == null)
			{
				return -1;
			}
			else
			{
				int NumMessagesInBatch = MsgMetadata.NumMessagesInBatch;
				MsgMetadata.Recycle();
				return NumMessagesInBatch;
			}
		}

		public static MessageMetadata PeekMessageMetadata(IByteBuffer MetadataAndPayload, string Subscription, long ConsumerId)
		{
			try
			{
				// save the reader index and restore after parsing
				int ReaderIdx = MetadataAndPayload.ReaderIndex;
				MessageMetadata Metadata = ParseMessageMetadata(MetadataAndPayload);
				MetadataAndPayload.SetReaderIndex(ReaderIdx);

				return Metadata;
			}
			catch (System.Exception T)
			{
				log.error("[{}] [{}] Failed to parse message metadata", Subscription, ConsumerId, T);
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
			Crc32c,
			None
		}

		public static bool PeerSupportsGetLastMessageId(int PeerVersion)
		{
			return PeerVersion >= (int)ProtocolVersion.V12;
		}

		public static bool PeerSupportsActiveConsumerListener(int PeerVersion)
		{
			return PeerVersion >= (int)ProtocolVersion.V12;
		}

		public static bool PeerSupportsMultiMessageAcknowledgment(int PeerVersion)
		{
			return PeerVersion >= (int)ProtocolVersion.V12;
		}

		public static bool PeerSupportJsonSchemaAvroFormat(int PeerVersion)
		{
			return PeerVersion >= (int)ProtocolVersion.V13;
		}

		public static bool PeerSupportsGetOrCreateSchema(int PeerVersion)
		{
			return PeerVersion >= (int)ProtocolVersion.V15;
		}
	}

}