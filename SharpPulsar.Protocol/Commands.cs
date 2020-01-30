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
			Proto.CommandConnect.Builder ConnectBuilder = CommandConnect.NewBuilder();
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

		public static IByteBuffer NewConnect(string authMethodName, Shared.AuthData authData, int protocolVersion, string libVersion, string targetBroker, string originalPrincipal, AuthData originalAuthData, string originalAuthMethod)
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
				connectBuilder.SetAuthData((byte[])(Array)authData.Bytes);
			}

			if (!string.ReferenceEquals(originalPrincipal, null))
			{
				connectBuilder.SetOriginalPrincipal(originalPrincipal);
			}

			if (originalAuthData != null)
			{
				connectBuilder.SetOriginalAuthData(Encoding.UTF8.GetString(originalAuthData.auth_data));
			}

			if (!string.ReferenceEquals(originalAuthMethod, null))
			{
				connectBuilder.SetOriginalAuthMethod(originalAuthMethod);
			}
			connectBuilder.SetProtocolVersion(protocolVersion);
			BaseCommand baseCmd = connectBuilder.Build().ToBaseCommand();
			
			IByteBuffer Res = SerializeWithSize(baseCmd);
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
			Proto.CommandConnected.Builder ConnectedBuilder = Proto.CommandConnected.newBuilder();
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

			Proto.CommandConnected Connected = ConnectedBuilder.build();
			IByteBuffer Res = SerializeWithSize(Proto.BaseCommand.newBuilder().setType(Proto.BaseCommand.Type.CONNECTED).setConnected(Connected));
			Connected.recycle();
			ConnectedBuilder.recycle();
			return Res;
		}

		public static IByteBuffer NewAuthChallenge(string AuthMethod, AuthData BrokerData, int ClientProtocolVersion)
		{
			Proto.CommandAuthChallenge.Builder ChallengeBuilder = Proto.CommandAuthChallenge.newBuilder();

			// If the broker supports a newer version of the protocol, it will anyway advertise the max version that the
			// client supports, to avoid confusing the client.
			int CurrentProtocolVersion = CurrentProtocolVersion;
			int VersionToAdvertise = Math.Min(CurrentProtocolVersion, ClientProtocolVersion);

			ChallengeBuilder.ProtocolVersion = VersionToAdvertise;

			Proto.CommandAuthChallenge Challenge = ChallengeBuilder.setChallenge(Proto.AuthData.newBuilder().setAuthData(copyFrom(BrokerData.Bytes)).setAuthMethodName(AuthMethod).build()).build();

			IByteBuffer Res = SerializeWithSize(Proto.BaseCommand.newBuilder().setType(Proto.BaseCommand.Type.AUTH_CHALLENGE).setAuthChallenge(Challenge));
			Challenge.recycle();
			ChallengeBuilder.recycle();
			return Res;
		}

		public static IByteBuffer NewAuthResponse(string AuthMethod, AuthData ClientData, int ClientProtocolVersion, string ClientVersion)
		{
			Proto.CommandAuthResponse.Builder ResponseBuilder = Proto.CommandAuthResponse.newBuilder();

			ResponseBuilder.setClientVersion(!string.ReferenceEquals(ClientVersion, null) ? ClientVersion : "Pulsar Client");
			ResponseBuilder.ProtocolVersion = ClientProtocolVersion;

			Proto.CommandAuthResponse Response = ResponseBuilder.setResponse(Proto.AuthData.newBuilder().setAuthData(copyFrom(ClientData.Bytes)).setAuthMethodName(AuthMethod).build()).build();

			IByteBuffer Res = SerializeWithSize(Proto.BaseCommand.newBuilder().setType(Proto.BaseCommand.Type.AUTH_RESPONSE).setAuthResponse(Response));
			Response.recycle();
			ResponseBuilder.recycle();
			return Res;
		}

		public static IByteBuffer NewSuccess(long RequestId)
		{
			Proto.CommandSuccess.Builder SuccessBuilder = Proto.CommandSuccess.newBuilder();
			SuccessBuilder.RequestId = RequestId;
			Proto.CommandSuccess Success = SuccessBuilder.build();
			IByteBuffer Res = SerializeWithSize(Proto.BaseCommand.newBuilder().setType(Proto.BaseCommand.Type.SUCCESS).setSuccess(Success));
			SuccessBuilder.recycle();
			Success.recycle();
			return Res;
		}

		public static IByteBuffer NewProducerSuccess(long RequestId, string ProducerName, SchemaVersion SchemaVersion)
		{
			return NewProducerSuccess(RequestId, ProducerName, -1, SchemaVersion);
		}

		public static IByteBuffer NewProducerSuccess(long RequestId, string ProducerName, long LastSequenceId, SchemaVersion SchemaVersion)
		{
			Proto.CommandProducerSuccess.Builder ProducerSuccessBuilder = Proto.CommandProducerSuccess.newBuilder();
			ProducerSuccessBuilder.RequestId = RequestId;
			ProducerSuccessBuilder.setProducerName(ProducerName);
			ProducerSuccessBuilder.LastSequenceId = LastSequenceId;
			ProducerSuccessBuilder.SchemaVersion = ByteString.copyFrom(SchemaVersion.bytes());
			Proto.CommandProducerSuccess ProducerSuccess = ProducerSuccessBuilder.build();
			IByteBuffer Res = SerializeWithSize(Proto.BaseCommand.newBuilder().setType(Proto.BaseCommand.Type.PRODUCER_SUCCESS).setProducerSuccess(ProducerSuccess));
			ProducerSuccess.recycle();
			ProducerSuccessBuilder.recycle();
			return Res;
		}

		public static IByteBuffer NewError(long RequestId, Proto.ServerError Error, string Message)
		{
			Proto.CommandError.Builder CmdErrorBuilder = Proto.CommandError.newBuilder();
			CmdErrorBuilder.RequestId = RequestId;
			CmdErrorBuilder.Error = Error;
			CmdErrorBuilder.setMessage(Message);
			Proto.CommandError CmdError = CmdErrorBuilder.build();
			IByteBuffer Res = SerializeWithSize(Proto.BaseCommand.newBuilder().setType(Proto.BaseCommand.Type.ERROR).setError(CmdError));
			CmdError.recycle();
			CmdErrorBuilder.recycle();
			return Res;

		}

		public static IByteBuffer NewSendReceipt(long ProducerId, long SequenceId, long HighestId, long LedgerId, long EntryId)
		{
			Proto.CommandSendReceipt.Builder SendReceiptBuilder = Proto.CommandSendReceipt.newBuilder();
			SendReceiptBuilder.ProducerId = ProducerId;
			SendReceiptBuilder.SequenceId = SequenceId;
			SendReceiptBuilder.HighestSequenceId = HighestId;
			Proto.MessageIdData.Builder MessageIdBuilder = Proto.MessageIdData.newBuilder();
			MessageIdBuilder.LedgerId = LedgerId;
			MessageIdBuilder.EntryId = EntryId;
			Proto.MessageIdData MessageId = MessageIdBuilder.build();
			SendReceiptBuilder.setMessageId(MessageId);
			Proto.CommandSendReceipt SendReceipt = SendReceiptBuilder.build();
			IByteBuffer Res = SerializeWithSize(Proto.BaseCommand.newBuilder().setType(Proto.BaseCommand.Type.SEND_RECEIPT).setSendReceipt(SendReceipt));
			MessageIdBuilder.recycle();
			MessageId.recycle();
			SendReceiptBuilder.recycle();
			SendReceipt.recycle();
			return Res;
		}

		public static IByteBuffer NewSendError(long ProducerId, long SequenceId, Proto.ServerError Error, string ErrorMsg)
		{
			Proto.CommandSendError.Builder SendErrorBuilder = Proto.CommandSendError.newBuilder();
			SendErrorBuilder.ProducerId = ProducerId;
			SendErrorBuilder.SequenceId = SequenceId;
			SendErrorBuilder.Error = Error;
			SendErrorBuilder.setMessage(ErrorMsg);
			Proto.CommandSendError SendError = SendErrorBuilder.build();
			IByteBuffer Res = SerializeWithSize(Proto.BaseCommand.newBuilder().setType(Proto.BaseCommand.Type.SEND_ERROR).setSendError(SendError));
			SendErrorBuilder.recycle();
			SendError.recycle();
			return Res;
		}


		public static bool HasChecksum(IByteBuffer Buffer)
		{
			return Buffer.getShort(Buffer.readerIndex()) == MagicCrc32c;
		}

		/// <summary>
		/// Read the checksum and advance the reader index in the buffer.
		/// 
		/// <para>Note: This method assume the checksum presence was already verified before.
		/// </para>
		/// </summary>
		public static int ReadChecksum(IByteBuffer Buffer)
		{
			Buffer.skipBytes(2); //skip magic bytes
			return Buffer.readInt();
		}

		public static void SkipChecksumIfPresent(IByteBuffer Buffer)
		{
			if (HasChecksum(Buffer))
			{
				ReadChecksum(Buffer);
			}
		}

		public static Proto.MessageMetadata ParseMessageMetadata(IByteBuffer Buffer)
		{
			try
			{
				// initially reader-index may point to start_of_checksum : increment reader-index to start_of_metadata
				// to parse metadata
				SkipChecksumIfPresent(Buffer);
				int MetadataSize = (int) Buffer.readUnsignedInt();

				int WriterIndex = Buffer.writerIndex();
				Buffer.writerIndex(Buffer.readerIndex() + MetadataSize);
				IByteBufferCodedInputStream Stream = IByteBufferCodedInputStream.get(Buffer);
				Proto.MessageMetadata.Builder MessageMetadataBuilder = Proto.MessageMetadata.newBuilder();
				Proto.MessageMetadata Res = MessageMetadataBuilder.mergeFrom(Stream, null).build();
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

		public static void SkipMessageMetadata(IByteBuffer Buffer)
		{
			// initially reader-index may point to start_of_checksum : increment reader-index to start_of_metadata to parse
			// metadata
			SkipChecksumIfPresent(Buffer);
			int MetadataSize = (int) Buffer.readUnsignedInt();
			Buffer.skipBytes(MetadataSize);
		}

		public static IByteBufferPair NewMessage(long ConsumerId, Proto.MessageIdData MessageId, int RedeliveryCount, IByteBuffer MetadataAndPayload)
		{
			Proto.CommandMessage.Builder MsgBuilder = Proto.CommandMessage.newBuilder();
			MsgBuilder.ConsumerId = ConsumerId;
			MsgBuilder.setMessageId(MessageId);
			if (RedeliveryCount > 0)
			{
				MsgBuilder.RedeliveryCount = RedeliveryCount;
			}
			Proto.CommandMessage Msg = MsgBuilder.build();
			Proto.BaseCommand.Builder CmdBuilder = Proto.BaseCommand.newBuilder();
			Proto.BaseCommand Cmd = CmdBuilder.setType(Proto.BaseCommand.Type.MESSAGE).setMessage(Msg).build();

			IByteBufferPair Res = SerializeCommandMessageWithSize(Cmd, MetadataAndPayload);
			Cmd.recycle();
			CmdBuilder.recycle();
			Msg.recycle();
			MsgBuilder.recycle();
			return Res;
		}

		public static IByteBufferPair NewSend(long ProducerId, long SequenceId, int NumMessaegs, ChecksumType ChecksumType, Proto.MessageMetadata MessageMetadata, IByteBuffer Payload)
		{
			return NewSend(ProducerId, SequenceId, NumMessaegs, 0, 0, ChecksumType, MessageMetadata, Payload);
		}

		public static IByteBufferPair NewSend(long ProducerId, long LowestSequenceId, long HighestSequenceId, int NumMessaegs, ChecksumType ChecksumType, Proto.MessageMetadata MessageMetadata, IByteBuffer Payload)
		{
			return NewSend(ProducerId, LowestSequenceId, HighestSequenceId, NumMessaegs, 0, 0, ChecksumType, MessageMetadata, Payload);
		}

		public static IByteBufferPair NewSend(long ProducerId, long SequenceId, int NumMessages, long TxnIdLeastBits, long TxnIdMostBits, ChecksumType ChecksumType, Proto.MessageMetadata MessageData, IByteBuffer Payload)
		{
			Proto.CommandSend.Builder SendBuilder = Proto.CommandSend.newBuilder();
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
			Proto.CommandSend Send = SendBuilder.build();

			IByteBufferPair Res = SerializeCommandSendWithSize(Proto.BaseCommand.newBuilder().setType(Proto.BaseCommand.Type.SEND).setSend(Send), ChecksumType, MessageData, Payload);
			Send.recycle();
			SendBuilder.recycle();
			return Res;
		}

		public static IByteBufferPair NewSend(long ProducerId, long LowestSequenceId, long HighestSequenceId, int NumMessages, long TxnIdLeastBits, long TxnIdMostBits, ChecksumType ChecksumType, Proto.MessageMetadata MessageData, IByteBuffer Payload)
		{
			Proto.CommandSend.Builder SendBuilder = Proto.CommandSend.newBuilder();
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
			Proto.CommandSend Send = SendBuilder.build();

			IByteBufferPair Res = SerializeCommandSendWithSize(Proto.BaseCommand.newBuilder().setType(Proto.BaseCommand.Type.SEND).setSend(Send), ChecksumType, MessageData, Payload);
			Send.recycle();
			SendBuilder.recycle();
			return Res;
		}

		public static IByteBuffer NewSubscribe(string Topic, string Subscription, long ConsumerId, long RequestId, Proto.CommandSubscribe.SubType SubType, int PriorityLevel, string ConsumerName, long ResetStartMessageBackInSeconds)
		{
			return NewSubscribe(Topic, Subscription, ConsumerId, RequestId, SubType, PriorityLevel, ConsumerName, true, null, Collections.emptyMap(), false, false, Proto.CommandSubscribe.InitialPosition.Earliest, ResetStartMessageBackInSeconds, null, true);
		}

		public static IByteBuffer NewSubscribe(string Topic, string Subscription, long ConsumerId, long RequestId, Proto.CommandSubscribe.SubType SubType, int PriorityLevel, string ConsumerName, bool IsDurable, Proto.MessageIdData StartMessageId, IDictionary<string, string> Metadata, bool ReadCompacted, bool IsReplicated, Proto.CommandSubscribe.InitialPosition SubscriptionInitialPosition, long StartMessageRollbackDurationInSec, SchemaInfo SchemaInfo, bool CreateTopicIfDoesNotExist)
		{
					return NewSubscribe(Topic, Subscription, ConsumerId, RequestId, SubType, PriorityLevel, ConsumerName, IsDurable, StartMessageId, Metadata, ReadCompacted, IsReplicated, SubscriptionInitialPosition, StartMessageRollbackDurationInSec, SchemaInfo, CreateTopicIfDoesNotExist, null);
		}

		public static IByteBuffer NewSubscribe(string Topic, string Subscription, long ConsumerId, long RequestId, Proto.CommandSubscribe.SubType SubType, int PriorityLevel, string ConsumerName, bool IsDurable, Proto.MessageIdData StartMessageId, IDictionary<string, string> Metadata, bool ReadCompacted, bool IsReplicated, Proto.CommandSubscribe.InitialPosition SubscriptionInitialPosition, long StartMessageRollbackDurationInSec, SchemaInfo SchemaInfo, bool CreateTopicIfDoesNotExist, KeySharedPolicy KeySharedPolicy)
		{
			Proto.CommandSubscribe.Builder SubscribeBuilder = Proto.CommandSubscribe.newBuilder();
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
						SubscribeBuilder.setKeySharedMeta(Proto.KeySharedMeta.newBuilder().setKeySharedMode(Proto.KeySharedMode.AUTO_SPLIT));
						break;
					case STICKY:
						Proto.KeySharedMeta.Builder Builder = Proto.KeySharedMeta.newBuilder().setKeySharedMode(Proto.KeySharedMode.STICKY);
						IList<Range> Ranges = ((KeySharedPolicy.KeySharedPolicySticky) KeySharedPolicy).Ranges;
						foreach (Range Range in Ranges)
						{
							Builder.addHashRanges(Proto.IntRange.newBuilder().setStart(Range.Start).setEnd(Range.End));
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

			Proto.Schema Schema = null;
			if (SchemaInfo != null)
			{
				Schema = GetSchema(SchemaInfo);
				SubscribeBuilder.setSchema(Schema);
			}

			Proto.CommandSubscribe Subscribe = SubscribeBuilder.build();
			IByteBuffer Res = SerializeWithSize(Proto.BaseCommand.newBuilder().setType(Proto.BaseCommand.Type.SUBSCRIBE).setSubscribe(Subscribe));
			SubscribeBuilder.recycle();
			Subscribe.recycle();
			if (null != Schema)
			{
				Schema.recycle();
			}
			return Res;
		}

		public static IByteBuffer NewUnsubscribe(long ConsumerId, long RequestId)
		{
			Proto.CommandUnsubscribe.Builder UnsubscribeBuilder = Proto.CommandUnsubscribe.newBuilder();
			UnsubscribeBuilder.ConsumerId = ConsumerId;
			UnsubscribeBuilder.RequestId = RequestId;
			Proto.CommandUnsubscribe Unsubscribe = UnsubscribeBuilder.build();
			IByteBuffer Res = SerializeWithSize(Proto.BaseCommand.newBuilder().setType(Proto.BaseCommand.Type.UNSUBSCRIBE).setUnsubscribe(Unsubscribe));
			UnsubscribeBuilder.recycle();
			Unsubscribe.recycle();
			return Res;
		}

		public static IByteBuffer NewActiveConsumerChange(long ConsumerId, bool IsActive)
		{
			Proto.CommandActiveConsumerChange.Builder ChangeBuilder = Proto.CommandActiveConsumerChange.newBuilder().setConsumerId(ConsumerId).setIsActive(IsActive);

			Proto.CommandActiveConsumerChange Change = ChangeBuilder.build();
			IByteBuffer Res = SerializeWithSize(Proto.BaseCommand.newBuilder().setType(Proto.BaseCommand.Type.ACTIVE_CONSUMER_CHANGE).setActiveConsumerChange(Change));
			ChangeBuilder.recycle();
			Change.recycle();
			return Res;
		}

		public static IByteBuffer NewSeek(long ConsumerId, long RequestId, long LedgerId, long EntryId)
		{
			Proto.CommandSeek.Builder SeekBuilder = Proto.CommandSeek.newBuilder();
			SeekBuilder.ConsumerId = ConsumerId;
			SeekBuilder.RequestId = RequestId;

			Proto.MessageIdData.Builder MessageIdBuilder = Proto.MessageIdData.newBuilder();
			MessageIdBuilder.LedgerId = LedgerId;
			MessageIdBuilder.EntryId = EntryId;
			Proto.MessageIdData MessageId = MessageIdBuilder.build();
			SeekBuilder.setMessageId(MessageId);

			Proto.CommandSeek Seek = SeekBuilder.build();
			IByteBuffer Res = SerializeWithSize(Proto.BaseCommand.newBuilder().setType(Proto.BaseCommand.Type.SEEK).setSeek(Seek));
			MessageId.recycle();
			MessageIdBuilder.recycle();
			SeekBuilder.recycle();
			Seek.recycle();
			return Res;
		}

		public static IByteBuffer NewSeek(long ConsumerId, long RequestId, long Timestamp)
		{
			Proto.CommandSeek.Builder SeekBuilder = Proto.CommandSeek.newBuilder();
			SeekBuilder.ConsumerId = ConsumerId;
			SeekBuilder.RequestId = RequestId;

			SeekBuilder.MessagePublishTime = Timestamp;

			Proto.CommandSeek Seek = SeekBuilder.build();
			IByteBuffer Res = SerializeWithSize(Proto.BaseCommand.newBuilder().setType(Proto.BaseCommand.Type.SEEK).setSeek(Seek));

			SeekBuilder.recycle();
			Seek.recycle();
			return Res;
		}

		public static IByteBuffer NewCloseConsumer(long ConsumerId, long RequestId)
		{
			Proto.CommandCloseConsumer.Builder CloseConsumerBuilder = Proto.CommandCloseConsumer.newBuilder();
			CloseConsumerBuilder.ConsumerId = ConsumerId;
			CloseConsumerBuilder.RequestId = RequestId;
			Proto.CommandCloseConsumer CloseConsumer = CloseConsumerBuilder.build();
			IByteBuffer Res = SerializeWithSize(Proto.BaseCommand.newBuilder().setType(Proto.BaseCommand.Type.CLOSE_CONSUMER).setCloseConsumer(CloseConsumer));
			CloseConsumerBuilder.recycle();
			CloseConsumer.recycle();
			return Res;
		}

		public static IByteBuffer NewReachedEndOfTopic(long ConsumerId)
		{
			Proto.CommandReachedEndOfTopic.Builder ReachedEndOfTopicBuilder = Proto.CommandReachedEndOfTopic.newBuilder();
			ReachedEndOfTopicBuilder.ConsumerId = ConsumerId;
			Proto.CommandReachedEndOfTopic ReachedEndOfTopic = ReachedEndOfTopicBuilder.build();
			IByteBuffer Res = SerializeWithSize(Proto.BaseCommand.newBuilder().setType(Proto.BaseCommand.Type.REACHED_END_OF_TOPIC).setReachedEndOfTopic(ReachedEndOfTopic));
			ReachedEndOfTopicBuilder.recycle();
			ReachedEndOfTopic.recycle();
			return Res;
		}

		public static IByteBuffer NewCloseProducer(long ProducerId, long RequestId)
		{
			Proto.CommandCloseProducer.Builder CloseProducerBuilder = Proto.CommandCloseProducer.newBuilder();
			CloseProducerBuilder.ProducerId = ProducerId;
			CloseProducerBuilder.RequestId = RequestId;
			Proto.CommandCloseProducer CloseProducer = CloseProducerBuilder.build();
			IByteBuffer Res = SerializeWithSize(Proto.BaseCommand.newBuilder().setType(Proto.BaseCommand.Type.CLOSE_PRODUCER).setCloseProducer(CloseProducerBuilder));
			CloseProducerBuilder.recycle();
			CloseProducer.recycle();
			return Res;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @VisibleForTesting public static io.netty.buffer.IByteBuffer newProducer(String topic, long producerId, long requestId, String producerName, java.util.Map<String, String> metadata)
		public static IByteBuffer NewProducer(string Topic, long ProducerId, long RequestId, string ProducerName, IDictionary<string, string> Metadata)
		{
			return NewProducer(Topic, ProducerId, RequestId, ProducerName, false, Metadata);
		}

		public static IByteBuffer NewProducer(string Topic, long ProducerId, long RequestId, string ProducerName, bool Encrypted, IDictionary<string, string> Metadata)
		{
			return NewProducer(Topic, ProducerId, RequestId, ProducerName, Encrypted, Metadata, null, 0, false);
		}

		private static Proto.Schema.Type GetSchemaType(SchemaType Type)
		{
			if (Type.Value < 0)
			{
				return Proto.Schema.Type.None;
			}
			else
			{
				return Proto.Schema.Type.valueOf(Type.Value);
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

		private static Proto.Schema GetSchema(SchemaInfo SchemaInfo)
		{
			Proto.Schema.Builder Builder = Proto.Schema.newBuilder().setName(SchemaInfo.Name).setSchemaData(copyFrom(SchemaInfo.Schema)).setType(GetSchemaType(SchemaInfo.Type)).addAllProperties(SchemaInfo.Properties.entrySet().Select(entry => Proto.KeyValue.newBuilder().setKey(entry.Key).setValue(entry.Value).build()).ToList());
			Proto.Schema Schema = Builder.build();
			Builder.recycle();
			return Schema;
		}

		public static IByteBuffer NewProducer(string Topic, long ProducerId, long RequestId, string ProducerName, bool Encrypted, IDictionary<string, string> Metadata, SchemaInfo SchemaInfo, long Epoch, bool UserProvidedProducerName)
		{
			Proto.CommandProducer.Builder ProducerBuilder = Proto.CommandProducer.newBuilder();
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

			Proto.CommandProducer Producer = ProducerBuilder.build();
			IByteBuffer Res = SerializeWithSize(Proto.BaseCommand.newBuilder().setType(Proto.BaseCommand.Type.PRODUCER).setProducer(Producer));
			ProducerBuilder.recycle();
			Producer.recycle();
			return Res;
		}

		public static IByteBuffer NewPartitionMetadataResponse(Proto.ServerError Error, string ErrorMsg, long RequestId)
		{
			Proto.CommandPartitionedTopicMetadataResponse.Builder PartitionMetadataResponseBuilder = Proto.CommandPartitionedTopicMetadataResponse.newBuilder();
			PartitionMetadataResponseBuilder.RequestId = RequestId;
			PartitionMetadataResponseBuilder.Error = Error;
			PartitionMetadataResponseBuilder.Response = Proto.CommandPartitionedTopicMetadataResponse.LookupType.Failed;
			if (!string.ReferenceEquals(ErrorMsg, null))
			{
				PartitionMetadataResponseBuilder.setMessage(ErrorMsg);
			}

			Proto.CommandPartitionedTopicMetadataResponse PartitionMetadataResponse = PartitionMetadataResponseBuilder.build();
			IByteBuffer Res = SerializeWithSize(Proto.BaseCommand.newBuilder().setType(Proto.BaseCommand.Type.PARTITIONED_METADATA_RESPONSE).setPartitionMetadataResponse(PartitionMetadataResponse));
			PartitionMetadataResponseBuilder.recycle();
			PartitionMetadataResponse.recycle();
			return Res;
		}

		public static IByteBuffer NewPartitionMetadataRequest(string Topic, long RequestId)
		{
			Proto.CommandPartitionedTopicMetadata.Builder PartitionMetadataBuilder = Proto.CommandPartitionedTopicMetadata.newBuilder();
			PartitionMetadataBuilder.setTopic(Topic);
			PartitionMetadataBuilder.RequestId = RequestId;
			Proto.CommandPartitionedTopicMetadata PartitionMetadata = PartitionMetadataBuilder.build();
			IByteBuffer Res = SerializeWithSize(Proto.BaseCommand.newBuilder().setType(Proto.BaseCommand.Type.PARTITIONED_METADATA).setPartitionMetadata(PartitionMetadata));
			PartitionMetadataBuilder.recycle();
			PartitionMetadata.recycle();
			return Res;
		}

		public static IByteBuffer NewPartitionMetadataResponse(int Partitions, long RequestId)
		{
			Proto.CommandPartitionedTopicMetadataResponse.Builder PartitionMetadataResponseBuilder = Proto.CommandPartitionedTopicMetadataResponse.newBuilder();
			PartitionMetadataResponseBuilder.Partitions = Partitions;
			PartitionMetadataResponseBuilder.Response = Proto.CommandPartitionedTopicMetadataResponse.LookupType.Success;
			PartitionMetadataResponseBuilder.RequestId = RequestId;

			Proto.CommandPartitionedTopicMetadataResponse PartitionMetadataResponse = PartitionMetadataResponseBuilder.build();
			IByteBuffer Res = SerializeWithSize(Proto.BaseCommand.newBuilder().setType(Proto.BaseCommand.Type.PARTITIONED_METADATA_RESPONSE).setPartitionMetadataResponse(PartitionMetadataResponse));
			PartitionMetadataResponseBuilder.recycle();
			PartitionMetadataResponse.recycle();
			return Res;
		}

		public static IByteBuffer NewLookup(string Topic, bool Authoritative, long RequestId)
		{
			Proto.CommandLookupTopic.Builder LookupTopicBuilder = Proto.CommandLookupTopic.newBuilder();
			LookupTopicBuilder.setTopic(Topic);
			LookupTopicBuilder.RequestId = RequestId;
			LookupTopicBuilder.Authoritative = Authoritative;
			Proto.CommandLookupTopic LookupBroker = LookupTopicBuilder.build();
			IByteBuffer Res = SerializeWithSize(Proto.BaseCommand.newBuilder().setType(Proto.BaseCommand.Type.LOOKUP).setLookupTopic(LookupBroker));
			LookupTopicBuilder.recycle();
			LookupBroker.recycle();
			return Res;
		}

		public static IByteBuffer NewLookupResponse(string BrokerServiceUrl, string BrokerServiceUrlTls, bool Authoritative, Proto.CommandLookupTopicResponse.LookupType Response, long RequestId, bool ProxyThroughServiceUrl)
		{
			Proto.CommandLookupTopicResponse.Builder CommandLookupTopicResponseBuilder = Proto.CommandLookupTopicResponse.newBuilder();
			CommandLookupTopicResponseBuilder.setBrokerServiceUrl(BrokerServiceUrl);
			if (!string.ReferenceEquals(BrokerServiceUrlTls, null))
			{
				CommandLookupTopicResponseBuilder.setBrokerServiceUrlTls(BrokerServiceUrlTls);
			}
			CommandLookupTopicResponseBuilder.Response = Response;
			CommandLookupTopicResponseBuilder.RequestId = RequestId;
			CommandLookupTopicResponseBuilder.Authoritative = Authoritative;
			CommandLookupTopicResponseBuilder.ProxyThroughServiceUrl = ProxyThroughServiceUrl;

			Proto.CommandLookupTopicResponse CommandLookupTopicResponse = CommandLookupTopicResponseBuilder.build();
			IByteBuffer Res = SerializeWithSize(Proto.BaseCommand.newBuilder().setType(Proto.BaseCommand.Type.LOOKUP_RESPONSE).setLookupTopicResponse(CommandLookupTopicResponse));
			CommandLookupTopicResponseBuilder.recycle();
			CommandLookupTopicResponse.recycle();
			return Res;
		}

		public static IByteBuffer NewLookupErrorResponse(Proto.ServerError Error, string ErrorMsg, long RequestId)
		{
			Proto.CommandLookupTopicResponse.Builder ConnectionBuilder = Proto.CommandLookupTopicResponse.newBuilder();
			ConnectionBuilder.RequestId = RequestId;
			ConnectionBuilder.Error = Error;
			if (!string.ReferenceEquals(ErrorMsg, null))
			{
				ConnectionBuilder.setMessage(ErrorMsg);
			}
			ConnectionBuilder.Response = Proto.CommandLookupTopicResponse.LookupType.Failed;

			Proto.CommandLookupTopicResponse ConnectionBroker = ConnectionBuilder.build();
			IByteBuffer Res = SerializeWithSize(Proto.BaseCommand.newBuilder().setType(Proto.BaseCommand.Type.LOOKUP_RESPONSE).setLookupTopicResponse(ConnectionBroker));
			ConnectionBuilder.recycle();
			ConnectionBroker.recycle();
			return Res;
		}

		public static IByteBuffer NewMultiMessageAck(long ConsumerId, IList<Pair<long, long>> Entries)
		{
			Proto.CommandAck.Builder AckBuilder = Proto.CommandAck.newBuilder();
			AckBuilder.ConsumerId = ConsumerId;
			AckBuilder.AckType = Proto.CommandAck.AckType.Individual;

			int EntriesCount = Entries.Count;
			for (int I = 0; I < EntriesCount; I++)
			{
				long LedgerId = Entries[I].Left;
				long EntryId = Entries[I].Right;

				Proto.MessageIdData.Builder MessageIdDataBuilder = Proto.MessageIdData.newBuilder();
				MessageIdDataBuilder.LedgerId = LedgerId;
				MessageIdDataBuilder.EntryId = EntryId;
				Proto.MessageIdData MessageIdData = MessageIdDataBuilder.build();
				AckBuilder.addMessageId(MessageIdData);

				MessageIdDataBuilder.recycle();
			}

			Proto.CommandAck Ack = AckBuilder.build();

			IByteBuffer Res = SerializeWithSize(Proto.BaseCommand.newBuilder().setType(Proto.BaseCommand.Type.ACK).setAck(Ack));

			for (int I = 0; I < EntriesCount; I++)
			{
				Ack.getMessageId(I).recycle();
			}
			Ack.recycle();
			AckBuilder.recycle();
			return Res;
		}

		public static IByteBuffer NewAck(long ConsumerId, long LedgerId, long EntryId, Proto.CommandAck.AckType AckType, Proto.CommandAck.ValidationError ValidationError, IDictionary<string, long> Properties)
		{
			return NewAck(ConsumerId, LedgerId, EntryId, AckType, ValidationError, Properties, 0, 0);
		}

		public static IByteBuffer NewAck(long ConsumerId, long LedgerId, long EntryId, Proto.CommandAck.AckType AckType, Proto.CommandAck.ValidationError ValidationError, IDictionary<string, long> Properties, long TxnIdLeastBits, long TxnIdMostBits)
		{
			Proto.CommandAck.Builder AckBuilder = Proto.CommandAck.newBuilder();
			AckBuilder.ConsumerId = ConsumerId;
			AckBuilder.AckType = AckType;
			Proto.MessageIdData.Builder MessageIdDataBuilder = Proto.MessageIdData.newBuilder();
			MessageIdDataBuilder.LedgerId = LedgerId;
			MessageIdDataBuilder.EntryId = EntryId;
			Proto.MessageIdData MessageIdData = MessageIdDataBuilder.build();
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
				AckBuilder.addProperties(Proto.KeyLongValue.newBuilder().setKey(E.Key).setValue(E.Value).build());
			}
			Proto.CommandAck Ack = AckBuilder.build();

			IByteBuffer Res = SerializeWithSize(Proto.BaseCommand.newBuilder().setType(Proto.BaseCommand.Type.ACK).setAck(Ack));
			Ack.recycle();
			AckBuilder.recycle();
			MessageIdDataBuilder.recycle();
			MessageIdData.recycle();
			return Res;
		}

		public static IByteBuffer NewAckResponse(long ConsumerId, long TxnIdLeastBits, long TxnIdMostBits)
		{
			Proto.CommandAckResponse.Builder CommandAckResponseBuilder = Proto.CommandAckResponse.newBuilder();
			CommandAckResponseBuilder.ConsumerId = ConsumerId;
			CommandAckResponseBuilder.TxnidLeastBits = TxnIdLeastBits;
			CommandAckResponseBuilder.TxnidMostBits = TxnIdMostBits;
			Proto.CommandAckResponse CommandAckResponse = CommandAckResponseBuilder.build();

			IByteBuffer Res = SerializeWithSize(Proto.BaseCommand.newBuilder().setType(Proto.BaseCommand.Type.ACK_RESPONSE).setAckResponse(CommandAckResponse));
			CommandAckResponseBuilder.recycle();
			CommandAckResponse.recycle();

			return Res;
		}

		public static IByteBuffer NewAckErrorResponse(Proto.ServerError Error, string ErrorMsg, long ConsumerId)
		{
			Proto.CommandAckResponse.Builder AckErrorBuilder = Proto.CommandAckResponse.newBuilder();
			AckErrorBuilder.ConsumerId = ConsumerId;
			AckErrorBuilder.Error = Error;
			if (!string.ReferenceEquals(ErrorMsg, null))
			{
				AckErrorBuilder.setMessage(ErrorMsg);
			}

			Proto.CommandAckResponse Response = AckErrorBuilder.build();
			IByteBuffer Res = SerializeWithSize(Proto.BaseCommand.newBuilder().setType(Proto.BaseCommand.Type.ACK_RESPONSE).setAckResponse(Response));

			AckErrorBuilder.recycle();
			Response.recycle();

			return Res;
		}

		public static IByteBuffer NewFlow(long ConsumerId, int MessagePermits)
		{
			Proto.CommandFlow.Builder FlowBuilder = Proto.CommandFlow.newBuilder();
			FlowBuilder.ConsumerId = ConsumerId;
			FlowBuilder.MessagePermits = MessagePermits;
			Proto.CommandFlow Flow = FlowBuilder.build();

			IByteBuffer Res = SerializeWithSize(Proto.BaseCommand.newBuilder().setType(Proto.BaseCommand.Type.FLOW).setFlow(FlowBuilder));
			Flow.recycle();
			FlowBuilder.recycle();
			return Res;
		}

		public static IByteBuffer NewRedeliverUnacknowledgedMessages(long ConsumerId)
		{
			Proto.CommandRedeliverUnacknowledgedMessages.Builder RedeliverBuilder = Proto.CommandRedeliverUnacknowledgedMessages.newBuilder();
			RedeliverBuilder.ConsumerId = ConsumerId;
			Proto.CommandRedeliverUnacknowledgedMessages Redeliver = RedeliverBuilder.build();
			IByteBuffer Res = SerializeWithSize(Proto.BaseCommand.newBuilder().setType(Proto.BaseCommand.Type.REDELIVER_UNACKNOWLEDGED_MESSAGES).setRedeliverUnacknowledgedMessages(RedeliverBuilder));
			Redeliver.recycle();
			RedeliverBuilder.recycle();
			return Res;
		}

		public static IByteBuffer NewRedeliverUnacknowledgedMessages(long ConsumerId, IList<Proto.MessageIdData> MessageIds)
		{
			Proto.CommandRedeliverUnacknowledgedMessages.Builder RedeliverBuilder = Proto.CommandRedeliverUnacknowledgedMessages.newBuilder();
			RedeliverBuilder.ConsumerId = ConsumerId;
			RedeliverBuilder.addAllMessageIds(MessageIds);
			Proto.CommandRedeliverUnacknowledgedMessages Redeliver = RedeliverBuilder.build();
			IByteBuffer Res = SerializeWithSize(Proto.BaseCommand.newBuilder().setType(Proto.BaseCommand.Type.REDELIVER_UNACKNOWLEDGED_MESSAGES).setRedeliverUnacknowledgedMessages(RedeliverBuilder));
			Redeliver.recycle();
			RedeliverBuilder.recycle();
			return Res;
		}

		public static IByteBuffer NewConsumerStatsResponse(Proto.ServerError ServerError, string ErrMsg, long RequestId)
		{
			Proto.CommandConsumerStatsResponse.Builder CommandConsumerStatsResponseBuilder = Proto.CommandConsumerStatsResponse.newBuilder();
			CommandConsumerStatsResponseBuilder.RequestId = RequestId;
			CommandConsumerStatsResponseBuilder.setErrorMessage(ErrMsg);
			CommandConsumerStatsResponseBuilder.ErrorCode = ServerError;

			Proto.CommandConsumerStatsResponse CommandConsumerStatsResponse = CommandConsumerStatsResponseBuilder.build();
			IByteBuffer Res = SerializeWithSize(Proto.BaseCommand.newBuilder().setType(Proto.BaseCommand.Type.CONSUMER_STATS_RESPONSE).setConsumerStatsResponse(CommandConsumerStatsResponseBuilder));
			CommandConsumerStatsResponse.recycle();
			CommandConsumerStatsResponseBuilder.recycle();
			return Res;
		}

		public static IByteBuffer NewConsumerStatsResponse(Proto.CommandConsumerStatsResponse.Builder Builder)
		{
			Proto.CommandConsumerStatsResponse CommandConsumerStatsResponse = Builder.build();
			IByteBuffer Res = SerializeWithSize(Proto.BaseCommand.newBuilder().setType(Proto.BaseCommand.Type.CONSUMER_STATS_RESPONSE).setConsumerStatsResponse(Builder));
			CommandConsumerStatsResponse.recycle();
			Builder.recycle();
			return Res;
		}

		public static IByteBuffer NewGetTopicsOfNamespaceRequest(string Namespace, long RequestId, Proto.CommandGetTopicsOfNamespace.Mode Mode)
		{
			Proto.CommandGetTopicsOfNamespace.Builder TopicsBuilder = Proto.CommandGetTopicsOfNamespace.newBuilder();
			TopicsBuilder.setNamespace(Namespace).setRequestId(RequestId).setMode(Mode);

			Proto.CommandGetTopicsOfNamespace TopicsCommand = TopicsBuilder.build();
			IByteBuffer Res = SerializeWithSize(Proto.BaseCommand.newBuilder().setType(Proto.BaseCommand.Type.GET_TOPICS_OF_NAMESPACE).setGetTopicsOfNamespace(TopicsCommand));
			TopicsBuilder.recycle();
			TopicsCommand.recycle();
			return Res;
		}

		public static IByteBuffer NewGetTopicsOfNamespaceResponse(IList<string> Topics, long RequestId)
		{
			Proto.CommandGetTopicsOfNamespaceResponse.Builder TopicsResponseBuilder = Proto.CommandGetTopicsOfNamespaceResponse.newBuilder();

			TopicsResponseBuilder.setRequestId(RequestId).addAllTopics(Topics);

			Proto.CommandGetTopicsOfNamespaceResponse TopicsOfNamespaceResponse = TopicsResponseBuilder.build();
			IByteBuffer Res = SerializeWithSize(Proto.BaseCommand.newBuilder().setType(Proto.BaseCommand.Type.GET_TOPICS_OF_NAMESPACE_RESPONSE).setGetTopicsOfNamespaceResponse(TopicsOfNamespaceResponse));

			TopicsResponseBuilder.recycle();
			TopicsOfNamespaceResponse.recycle();
			return Res;
		}

		private static readonly IByteBuffer cmdPing;

		static Commands()
		{
			IByteBuffer SerializedCmdPing = SerializeWithSize(Proto.BaseCommand.newBuilder().setType(Proto.BaseCommand.Type.PING).setPing(Proto.CommandPing.DefaultInstance));
			cmdPing = Unpooled.copiedBuffer(SerializedCmdPing);
			SerializedCmdPing.release();
			IByteBuffer SerializedCmdPong = SerializeWithSize(Proto.BaseCommand.newBuilder().setType(Proto.BaseCommand.Type.PONG).setPong(Proto.CommandPong.DefaultInstance));
			cmdPong = Unpooled.copiedBuffer(SerializedCmdPong);
			SerializedCmdPong.release();
		}

		internal static IByteBuffer NewPing()
		{
			return cmdPing.retainedDuplicate();
		}

		private static readonly IByteBuffer cmdPong;


		internal static IByteBuffer NewPong()
		{
			return cmdPong.retainedDuplicate();
		}

		public static IByteBuffer NewGetLastMessageId(long ConsumerId, long RequestId)
		{
			Proto.CommandGetLastMessageId.Builder CmdBuilder = Proto.CommandGetLastMessageId.newBuilder();
			CmdBuilder.setConsumerId(ConsumerId).setRequestId(RequestId);

			IByteBuffer Res = SerializeWithSize(Proto.BaseCommand.newBuilder().setType(Proto.BaseCommand.Type.GET_LAST_MESSAGE_ID).setGetLastMessageId(CmdBuilder.build()));
			CmdBuilder.recycle();
			return Res;
		}

		public static IByteBuffer NewGetLastMessageIdResponse(long RequestId, Proto.MessageIdData MessageIdData)
		{
			Proto.CommandGetLastMessageIdResponse.Builder Response = Proto.CommandGetLastMessageIdResponse.newBuilder().setLastMessageId(MessageIdData).setRequestId(RequestId);

			IByteBuffer Res = SerializeWithSize(Proto.BaseCommand.newBuilder().setType(Proto.BaseCommand.Type.GET_LAST_MESSAGE_ID_RESPONSE).setGetLastMessageIdResponse(Response.build()));
			Response.recycle();
			return Res;
		}

		public static IByteBuffer NewGetSchema(long RequestId, string Topic, Optional<SchemaVersion> Version)
		{
			Proto.CommandGetSchema.Builder Schema = Proto.CommandGetSchema.newBuilder().setRequestId(RequestId);
			Schema.setTopic(Topic);
			if (Version.Present)
			{
				Schema.SchemaVersion = ByteString.copyFrom(Version.get().bytes());
			}

			Proto.CommandGetSchema GetSchema = Schema.build();

			IByteBuffer Res = SerializeWithSize(Proto.BaseCommand.newBuilder().setType(Proto.BaseCommand.Type.GET_SCHEMA).setGetSchema(GetSchema));
			Schema.recycle();
			return Res;
		}

		public static IByteBuffer NewGetSchemaResponse(long RequestId, Proto.CommandGetSchemaResponse Response)
		{
			Proto.CommandGetSchemaResponse.Builder SchemaResponseBuilder = Proto.CommandGetSchemaResponse.newBuilder(Response).setRequestId(RequestId);

			IByteBuffer Res = SerializeWithSize(Proto.BaseCommand.newBuilder().setType(Proto.BaseCommand.Type.GET_SCHEMA_RESPONSE).setGetSchemaResponse(SchemaResponseBuilder.build()));
			SchemaResponseBuilder.recycle();
			return Res;
		}

		public static IByteBuffer NewGetSchemaResponse(long RequestId, SchemaInfo Schema, SchemaVersion Version)
		{
			Proto.CommandGetSchemaResponse.Builder SchemaResponse = Proto.CommandGetSchemaResponse.newBuilder().setRequestId(RequestId).setSchemaVersion(ByteString.copyFrom(Version.bytes())).setSchema(GetSchema(Schema));

			IByteBuffer Res = SerializeWithSize(Proto.BaseCommand.newBuilder().setType(Proto.BaseCommand.Type.GET_SCHEMA_RESPONSE).setGetSchemaResponse(SchemaResponse.build()));
			SchemaResponse.recycle();
			return Res;
		}

		public static IByteBuffer NewGetSchemaResponseError(long RequestId, Proto.ServerError Error, string ErrorMessage)
		{
			Proto.CommandGetSchemaResponse.Builder SchemaResponse = Proto.CommandGetSchemaResponse.newBuilder().setRequestId(RequestId).setErrorCode(Error).setErrorMessage(ErrorMessage);

			IByteBuffer Res = SerializeWithSize(Proto.BaseCommand.newBuilder().setType(Proto.BaseCommand.Type.GET_SCHEMA_RESPONSE).setGetSchemaResponse(SchemaResponse.build()));
			SchemaResponse.recycle();
			return Res;
		}

		public static IByteBuffer NewGetOrCreateSchema(long RequestId, string Topic, SchemaInfo SchemaInfo)
		{
			Proto.CommandGetOrCreateSchema GetOrCreateSchema = Proto.CommandGetOrCreateSchema.newBuilder().setRequestId(RequestId).setTopic(Topic).setSchema(GetSchema(SchemaInfo)).build();
			IByteBuffer Res = SerializeWithSize(Proto.BaseCommand.newBuilder().setType(Proto.BaseCommand.Type.GET_OR_CREATE_SCHEMA).setGetOrCreateSchema(GetOrCreateSchema));
			GetOrCreateSchema.recycle();
			return Res;
		}

		public static IByteBuffer NewGetOrCreateSchemaResponse(long RequestId, SchemaVersion SchemaVersion)
		{
			Proto.CommandGetOrCreateSchemaResponse.Builder SchemaResponse = Proto.CommandGetOrCreateSchemaResponse.newBuilder().setRequestId(RequestId).setSchemaVersion(ByteString.copyFrom(SchemaVersion.bytes()));
			IByteBuffer Res = SerializeWithSize(Proto.BaseCommand.newBuilder().setType(Proto.BaseCommand.Type.GET_OR_CREATE_SCHEMA_RESPONSE).setGetOrCreateSchemaResponse(SchemaResponse.build()));
			SchemaResponse.recycle();
			return Res;
		}

		public static IByteBuffer NewGetOrCreateSchemaResponseError(long RequestId, Proto.ServerError Error, string ErrorMessage)
		{
			Proto.CommandGetOrCreateSchemaResponse.Builder SchemaResponse = Proto.CommandGetOrCreateSchemaResponse.newBuilder().setRequestId(RequestId).setErrorCode(Error).setErrorMessage(ErrorMessage);
			IByteBuffer Res = SerializeWithSize(Proto.BaseCommand.newBuilder().setType(Proto.BaseCommand.Type.GET_OR_CREATE_SCHEMA_RESPONSE).setGetOrCreateSchemaResponse(SchemaResponse.build()));
			SchemaResponse.recycle();
			return Res;
		}

		// ---- transaction related ----

		public static IByteBuffer NewTxn(long TcId, long RequestId, long TtlSeconds)
		{
			Proto.CommandNewTxn CommandNewTxn = Proto.CommandNewTxn.newBuilder().setTcId(TcId).setRequestId(RequestId).setTxnTtlSeconds(TtlSeconds).build();
			IByteBuffer Res = SerializeWithSize(Proto.BaseCommand.newBuilder().setType(Proto.BaseCommand.Type.NEW_TXN).setNewTxn(CommandNewTxn));
			CommandNewTxn.recycle();
			return Res;
		}

		public static IByteBuffer NewTxnResponse(long RequestId, long TxnIdLeastBits, long TxnIdMostBits)
		{
			Proto.CommandNewTxnResponse CommandNewTxnResponse = Proto.CommandNewTxnResponse.newBuilder().setRequestId(RequestId).setTxnidMostBits(TxnIdMostBits).setTxnidLeastBits(TxnIdLeastBits).build();
			IByteBuffer Res = SerializeWithSize(Proto.BaseCommand.newBuilder().setType(Proto.BaseCommand.Type.NEW_TXN_RESPONSE).setNewTxnResponse(CommandNewTxnResponse));
			CommandNewTxnResponse.recycle();

			return Res;
		}

		public static IByteBuffer NewTxnResponse(long RequestId, long TxnIdMostBits, Proto.ServerError Error, string ErrorMsg)
		{
			Proto.CommandNewTxnResponse.Builder Builder = Proto.CommandNewTxnResponse.newBuilder();
			Builder.RequestId = RequestId;
			Builder.TxnidMostBits = TxnIdMostBits;
			Builder.Error = Error;
			if (!string.ReferenceEquals(ErrorMsg, null))
			{
				Builder.setMessage(ErrorMsg);
			}
			Proto.CommandNewTxnResponse ErrorResponse = Builder.build();
			IByteBuffer Res = SerializeWithSize(Proto.BaseCommand.newBuilder().setType(Proto.BaseCommand.Type.NEW_TXN_RESPONSE).setNewTxnResponse(ErrorResponse));
			Builder.recycle();
			ErrorResponse.recycle();

			return Res;
		}

		public static IByteBuffer NewAddPartitionToTxn(long RequestId, long TxnIdLeastBits, long TxnIdMostBits)
		{
			Proto.CommandAddPartitionToTxn CommandAddPartitionToTxn = Proto.CommandAddPartitionToTxn.newBuilder().setRequestId(RequestId).setTxnidLeastBits(TxnIdLeastBits).setTxnidMostBits(TxnIdMostBits).build();
			IByteBuffer Res = SerializeWithSize(Proto.BaseCommand.newBuilder().setType(Proto.BaseCommand.Type.ADD_PARTITION_TO_TXN).setAddPartitionToTxn(CommandAddPartitionToTxn));
			CommandAddPartitionToTxn.recycle();
			return Res;
		}

		public static IByteBuffer NewAddPartitionToTxnResponse(long RequestId, long TxnIdLeastBits, long TxnIdMostBits)
		{
			Proto.CommandAddPartitionToTxnResponse CommandAddPartitionToTxnResponse = Proto.CommandAddPartitionToTxnResponse.newBuilder().setRequestId(RequestId).setTxnidLeastBits(TxnIdLeastBits).setTxnidMostBits(TxnIdMostBits).build();
			IByteBuffer Res = SerializeWithSize(Proto.BaseCommand.newBuilder().setType(Proto.BaseCommand.Type.ADD_PARTITION_TO_TXN_RESPONSE).setAddPartitionToTxnResponse(CommandAddPartitionToTxnResponse));
			CommandAddPartitionToTxnResponse.recycle();
			return Res;
		}

		public static IByteBuffer NewAddPartitionToTxnResponse(long RequestId, long TxnIdMostBits, Proto.ServerError Error, string ErrorMsg)
		{
			Proto.CommandAddPartitionToTxnResponse.Builder Builder = Proto.CommandAddPartitionToTxnResponse.newBuilder();
			Builder.RequestId = RequestId;
			Builder.TxnidMostBits = TxnIdMostBits;
			Builder.Error = Error;
			if (!string.ReferenceEquals(ErrorMsg, null))
			{
				Builder.setMessage(ErrorMsg);
			}
			Proto.CommandAddPartitionToTxnResponse CommandAddPartitionToTxnResponse = Builder.build();
			IByteBuffer Res = SerializeWithSize(Proto.BaseCommand.newBuilder().setType(Proto.BaseCommand.Type.ADD_PARTITION_TO_TXN_RESPONSE).setAddPartitionToTxnResponse(CommandAddPartitionToTxnResponse));
			Builder.recycle();
			CommandAddPartitionToTxnResponse.recycle();
			return Res;
		}

		public static IByteBuffer NewAddSubscriptionToTxn(long RequestId, long TxnIdLeastBits, long TxnIdMostBits, IList<Proto.Subscription> Subscription)
		{
			Proto.CommandAddSubscriptionToTxn CommandAddSubscriptionToTxn = Proto.CommandAddSubscriptionToTxn.newBuilder().setRequestId(RequestId).setTxnidLeastBits(TxnIdLeastBits).setTxnidMostBits(TxnIdMostBits).addAllSubscription(Subscription).build();
			IByteBuffer Res = SerializeWithSize(Proto.BaseCommand.newBuilder().setType(Proto.BaseCommand.Type.ADD_SUBSCRIPTION_TO_TXN).setAddSubscriptionToTxn(CommandAddSubscriptionToTxn));
			CommandAddSubscriptionToTxn.recycle();
			return Res;
		}

		public static IByteBuffer NewAddSubscriptionToTxnResponse(long RequestId, long TxnIdLeastBits, long TxnIdMostBits)
		{
			Proto.CommandAddSubscriptionToTxnResponse Command = Proto.CommandAddSubscriptionToTxnResponse.newBuilder().setRequestId(RequestId).setTxnidLeastBits(TxnIdLeastBits).setTxnidMostBits(TxnIdMostBits).build();
			IByteBuffer Res = SerializeWithSize(Proto.BaseCommand.newBuilder().setType(Proto.BaseCommand.Type.ADD_SUBSCRIPTION_TO_TXN_RESPONSE).setAddSubscriptionToTxnResponse(Command));
			Command.recycle();
			return Res;
		}

		public static IByteBuffer NewAddSubscriptionToTxnResponse(long RequestId, long TxnIdMostBits, Proto.ServerError Error, string ErrorMsg)
		{
			Proto.CommandAddSubscriptionToTxnResponse.Builder Builder = Proto.CommandAddSubscriptionToTxnResponse.newBuilder();
			Builder.RequestId = RequestId;
			Builder.TxnidMostBits = TxnIdMostBits;
			Builder.Error = Error;
			if (!string.ReferenceEquals(ErrorMsg, null))
			{
				Builder.setMessage(ErrorMsg);
			}
			Proto.CommandAddSubscriptionToTxnResponse ErrorResponse = Builder.build();
			IByteBuffer Res = SerializeWithSize(Proto.BaseCommand.newBuilder().setType(Proto.BaseCommand.Type.ADD_SUBSCRIPTION_TO_TXN_RESPONSE).setAddSubscriptionToTxnResponse(ErrorResponse));
			Builder.recycle();
			ErrorResponse.recycle();
			return Res;
		}

		public static IByteBuffer NewEndTxn(long RequestId, long TxnIdLeastBits, long TxnIdMostBits, Proto.TxnAction TxnAction)
		{
			Proto.CommandEndTxn CommandEndTxn = Proto.CommandEndTxn.newBuilder().setRequestId(RequestId).setTxnidLeastBits(TxnIdLeastBits).setTxnidMostBits(TxnIdMostBits).setTxnAction(TxnAction).build();
			IByteBuffer Res = SerializeWithSize(Proto.BaseCommand.newBuilder().setType(Proto.BaseCommand.Type.END_TXN).setEndTxn(CommandEndTxn));
			CommandEndTxn.recycle();
			return Res;
		}

		public static IByteBuffer NewEndTxnResponse(long RequestId, long TxnIdLeastBits, long TxnIdMostBits)
		{
			Proto.CommandEndTxnResponse CommandEndTxnResponse = Proto.CommandEndTxnResponse.newBuilder().setRequestId(RequestId).setTxnidLeastBits(TxnIdLeastBits).setTxnidMostBits(TxnIdMostBits).build();
			IByteBuffer Res = SerializeWithSize(Proto.BaseCommand.newBuilder().setType(Proto.BaseCommand.Type.END_TXN_RESPONSE).setEndTxnResponse(CommandEndTxnResponse));
			CommandEndTxnResponse.recycle();
			return Res;
		}

		public static IByteBuffer NewEndTxnResponse(long RequestId, long TxnIdMostBits, Proto.ServerError Error, string ErrorMsg)
		{
			Proto.CommandEndTxnResponse.Builder Builder = Proto.CommandEndTxnResponse.newBuilder();
			Builder.RequestId = RequestId;
			Builder.TxnidMostBits = TxnIdMostBits;
			Builder.Error = Error;
			if (!string.ReferenceEquals(ErrorMsg, null))
			{
				Builder.setMessage(ErrorMsg);
			}
			Proto.CommandEndTxnResponse CommandEndTxnResponse = Builder.build();
			IByteBuffer Res = SerializeWithSize(Proto.BaseCommand.newBuilder().setType(Proto.BaseCommand.Type.END_TXN_RESPONSE).setEndTxnResponse(CommandEndTxnResponse));
			Builder.recycle();
			CommandEndTxnResponse.recycle();
			return Res;
		}

		public static IByteBuffer NewEndTxnOnPartition(long RequestId, long TxnIdLeastBits, long TxnIdMostBits, string Topic, Proto.TxnAction TxnAction)
		{
			Proto.CommandEndTxnOnPartition.Builder TxnEndOnPartition = Proto.CommandEndTxnOnPartition.newBuilder().setRequestId(RequestId).setTxnidLeastBits(TxnIdLeastBits).setTxnidMostBits(TxnIdMostBits).setTopic(Topic).setTxnAction(TxnAction);
			IByteBuffer Res = SerializeWithSize(Proto.BaseCommand.newBuilder().setType(Proto.BaseCommand.Type.END_TXN_ON_PARTITION).setEndTxnOnPartition(TxnEndOnPartition));
			TxnEndOnPartition.recycle();
			return Res;
		}

		public static IByteBuffer NewEndTxnOnPartitionResponse(long RequestId, long TxnIdLeastBits, long TxnIdMostBits)
		{
			Proto.CommandEndTxnOnPartitionResponse CommandEndTxnOnPartitionResponse = Proto.CommandEndTxnOnPartitionResponse.newBuilder().setRequestId(RequestId).setTxnidLeastBits(TxnIdLeastBits).setTxnidMostBits(TxnIdMostBits).build();
			IByteBuffer Res = SerializeWithSize(Proto.BaseCommand.newBuilder().setType(Proto.BaseCommand.Type.END_TXN_ON_PARTITION_RESPONSE).setEndTxnOnPartitionResponse(CommandEndTxnOnPartitionResponse));
			CommandEndTxnOnPartitionResponse.recycle();
			return Res;
		}

		public static IByteBuffer NewEndTxnOnPartitionResponse(long RequestId, Proto.ServerError Error, string ErrorMsg)
		{
			Proto.CommandEndTxnOnPartitionResponse.Builder Builder = Proto.CommandEndTxnOnPartitionResponse.newBuilder();
			Builder.RequestId = RequestId;
			Builder.Error = Error;
			if (!string.ReferenceEquals(ErrorMsg, null))
			{
				Builder.setMessage(ErrorMsg);
			}
			Proto.CommandEndTxnOnPartitionResponse CommandEndTxnOnPartitionResponse = Builder.build();
			IByteBuffer Res = SerializeWithSize(Proto.BaseCommand.newBuilder().setType(Proto.BaseCommand.Type.END_TXN_ON_PARTITION_RESPONSE).setEndTxnOnPartitionResponse(CommandEndTxnOnPartitionResponse));
			Builder.recycle();
			CommandEndTxnOnPartitionResponse.recycle();
			return Res;
		}

		public static IByteBuffer NewEndTxnOnSubscription(long RequestId, long TxnIdLeastBits, long TxnIdMostBits, Proto.Subscription Subscription, Proto.TxnAction TxnAction)
		{
			Proto.CommandEndTxnOnSubscription CommandEndTxnOnSubscription = Proto.CommandEndTxnOnSubscription.newBuilder().setRequestId(RequestId).setTxnidLeastBits(TxnIdLeastBits).setTxnidMostBits(TxnIdMostBits).setSubscription(Subscription).setTxnAction(TxnAction).build();
			IByteBuffer Res = SerializeWithSize(Proto.BaseCommand.newBuilder().setType(Proto.BaseCommand.Type.END_TXN_ON_SUBSCRIPTION).setEndTxnOnSubscription(CommandEndTxnOnSubscription));
			CommandEndTxnOnSubscription.recycle();
			return Res;
		}

		public static IByteBuffer NewEndTxnOnSubscriptionResponse(long RequestId, long TxnIdLeastBits, long TxnIdMostBits)
		{
			Proto.CommandEndTxnOnSubscriptionResponse Response = Proto.CommandEndTxnOnSubscriptionResponse.newBuilder().setRequestId(RequestId).setTxnidLeastBits(TxnIdLeastBits).setTxnidMostBits(TxnIdMostBits).build();
			IByteBuffer Res = SerializeWithSize(Proto.BaseCommand.newBuilder().setType(Proto.BaseCommand.Type.END_TXN_ON_SUBSCRIPTION_RESPONSE).setEndTxnOnSubscriptionResponse(Response));
			Response.recycle();
			return Res;
		}

		public static IByteBuffer NewEndTxnOnSubscriptionResponse(long RequestId, Proto.ServerError Error, string ErrorMsg)
		{
			Proto.CommandEndTxnOnSubscriptionResponse.Builder Builder = Proto.CommandEndTxnOnSubscriptionResponse.newBuilder();
			Builder.RequestId = RequestId;
			Builder.Error = Error;
			if (!string.ReferenceEquals(ErrorMsg, null))
			{
				Builder.setMessage(ErrorMsg);
			}
			Proto.CommandEndTxnOnSubscriptionResponse Response = Builder.build();
			IByteBuffer Res = SerializeWithSize(Proto.BaseCommand.newBuilder().setType(Proto.BaseCommand.Type.END_TXN_ON_SUBSCRIPTION_RESPONSE).setEndTxnOnSubscriptionResponse(Response));
			Builder.recycle();
			Response.recycle();
			return Res;
		}

		public static Stream SerializePayloadCommand(BaseCommand command)
		{
			Stream output = null;
			using var stream = new RecyclableMemoryStreamManager().GetStream();
			// write fake totalLength
			for (var i = 0; i < 5; i++)
				stream.WriteByte(0);

			// write commandPayload
			Serializer.SerializeWithLengthPrefix(stream, command, PrefixStyle.Fixed32BigEndian);

			var frameSize = stream.Length;

			var totalSize = frameSize - 4;

			//write total size and command size
			stream.Seek(0L, SeekOrigin.Begin);

			using var binaryWriter = new BinaryWriter(stream);
			var int32ToBigEndian = IPAddress.HostToNetworkOrder(totalSize);
			binaryWriter.Write(int32ToBigEndian);
			stream.Seek(0L, SeekOrigin.Begin);
			//Log.Logger.LogDebug("Sending message of type {0}", command.``type``);

			stream.CopyToAsync(output);
			return output;


		}
		public static Stream SerializePayloadCommand(BaseCommand command, MessageMetadata metadata, byte[] payload)
		{
			Stream output = null;
			using var stream = new RecyclableMemoryStreamManager().GetStream();
			// write fake totalLength
			for (var i = 0; i < 5; i++)
				stream.WriteByte(0);

			// write commandPayload
			Serializer.SerializeWithLengthPrefix(stream, command, PrefixStyle.Fixed32BigEndian);

			var stream1Size = stream.Length;

			// write magic number 0x0e01
			stream.WriteByte(14);
			stream.WriteByte(1);

			// write fake CRC sum and fake metadata length
			for (var i = 0; i < 5; i++)
				stream.WriteByte(0);

			// write metadata
			Serializer.SerializeWithLengthPrefix(stream, metadata, PrefixStyle.Fixed32BigEndian);

			var stream2Size = stream.Length;
			var totalMetadataSize = stream2Size - stream1Size - 6;

			// write payload
			stream.Write(payload, 0, payload.Length);


			var frameSize = stream.Length;
			var totalSize = frameSize - 4;
			var payloadSize = frameSize - stream2Size;
			var crcStart = stream1Size + 2;
			var crcPayloadStart = crcStart + 4;

			// write missing sizes
			using var binaryWriter = new BinaryWriter(stream);

			//write CRC
			stream.Seek(crcPayloadStart, SeekOrigin.Begin);
			var crc = CRC32C.Get(0u, stream.ToArray(), (int)(totalMetadataSize + payloadSize));
			stream.Seek(crcStart, SeekOrigin.Begin);
			var int32ToBigEndian = IPAddress.HostToNetworkOrder(crc);
			binaryWriter.Write(int32ToBigEndian);

			//write total size and command size
			stream.Seek(0L, SeekOrigin.Begin);
			int32ToBigEndian = IPAddress.HostToNetworkOrder(totalSize);
			binaryWriter.Write(int32ToBigEndian);

			//Log.Logger.LogDebug("Sending message of type {0}", command.``type``);

			stream.CopyToAsync(output);
			return output;


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

		private static ByteBufPair SerializeCommandSendWithSize(Proto.BaseCommand.Builder CmdBuilder, ChecksumType ChecksumType, Proto.MessageMetadata MsgMetadata, IByteBuffer Payload)
		{
			// / Wire format
			// [TOTAL_SIZE] [CMD_SIZE][CMD] [MAGIC_NUMBER][CHECKSUM] [METADATA_SIZE][METADATA] [PAYLOAD]

			Proto.BaseCommand Cmd = CmdBuilder.build();
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

			IByteBuffer Headers = PulsarIByteBufferAllocator.DEFAULT.buffer(HeadersSize, HeadersSize);
			Headers.writeInt(TotalSize); // External frame

			try
			{
				// Write cmd
				Headers.writeInt(CmdSize);

				IByteBufferCodedOutputStream OutStream = IByteBufferCodedOutputStream.get(Headers);
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

			IByteBufferPair Command = IByteBufferPair.Get(Headers, Payload);

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

		public static IByteBuffer SerializeMetadataAndPayload(ChecksumType ChecksumType, Proto.MessageMetadata MsgMetadata, IByteBuffer Payload)
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

			IByteBuffer MetadataAndPayload = PulsarIByteBufferAllocator.DEFAULT.buffer(TotalSize, TotalSize);
			try
			{
				IByteBufferCodedOutputStream OutStream = IByteBufferCodedOutputStream.get(MetadataAndPayload);

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

		public static long InitBatchMessageMetadata(MessageMetadataBuilder MessageMetadata)
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

		public static IByteBuffer SerializeSingleMessageInBatchWithPayload(Proto.SingleMessageMetadata.Builder SingleMessageMetadataBuilder, IByteBuffer Payload, IByteBuffer BatchBuffer)
		{
			int PayLoadSize = Payload.readableBytes();
			Proto.SingleMessageMetadata SingleMessageMetadata = SingleMessageMetadataBuilder.setPayloadSize(PayLoadSize).build();
			// serialize meta-data size, meta-data and payload for single message in batch
			int SingleMsgMetadataSize = SingleMessageMetadata.SerializedSize;
			try
			{
				BatchBuffer.writeInt(SingleMsgMetadataSize);
				IByteBufferCodedOutputStream OutStream = IByteBufferCodedOutputStream.get(BatchBuffer);
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

		public static IByteBuffer SerializeSingleMessageInBatchWithPayload(Proto.MessageMetadata.Builder MsgBuilder, IByteBuffer Payload, IByteBuffer BatchBuffer)
		{

			// build single message meta-data
			Proto.SingleMessageMetadata.Builder SingleMessageMetadataBuilder = Proto.SingleMessageMetadata.newBuilder();
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
//ORIGINAL LINE: public static io.netty.buffer.IByteBuffer deSerializeSingleMessageInBatch(io.netty.buffer.IByteBuffer uncompressedPayload, SharpPulsar.api.proto.Proto.SingleMessageMetadata.Builder singleMessageMetadataBuilder, int index, int batchSize) throws java.io.IOException
		public static IByteBuffer DeSerializeSingleMessageInBatch(IByteBuffer UncompressedPayload, Proto.SingleMessageMetadata.Builder SingleMessageMetadataBuilder, int Index, int BatchSize)
		{
			int SingleMetaSize = (int) UncompressedPayload.readUnsignedInt();
			int WriterIndex = UncompressedPayload.writerIndex();
			int BeginIndex = UncompressedPayload.readerIndex() + SingleMetaSize;
			UncompressedPayload.writerIndex(BeginIndex);
			IByteBufferCodedInputStream Stream = IByteBufferCodedInputStream.get(UncompressedPayload);
			SingleMessageMetadataBuilder.mergeFrom(Stream, null);
			Stream.recycle();

			int SingleMessagePayloadSize = SingleMessageMetadataBuilder.PayloadSize;

			int ReaderIndex = UncompressedPayload.readerIndex();
			IByteBuffer SingleMessagePayload = UncompressedPayload.retainedSlice(ReaderIndex, SingleMessagePayloadSize);
			UncompressedPayload.writerIndex(WriterIndex);

			// reader now points to beginning of payload read; so move it past message payload just read
			if (Index < BatchSize)
			{
				UncompressedPayload.readerIndex(ReaderIndex + SingleMessagePayloadSize);
			}

			return SingleMessagePayload;
		}

		private static IByteBufferPair SerializeCommandMessageWithSize(Proto.BaseCommand Cmd, IByteBuffer MetadataAndPayload)
		{
			// / Wire format
			// [TOTAL_SIZE] [CMD_SIZE][CMD] [MAGIC_NUMBER][CHECKSUM] [METADATA_SIZE][METADATA] [PAYLOAD]
			//
			// metadataAndPayload contains from magic-number to the payload included


			int CmdSize = Cmd.SerializedSize;
			int TotalSize = 4 + CmdSize + MetadataAndPayload.readableBytes();
			int HeadersSize = 4 + 4 + CmdSize;

			IByteBuffer Headers = PulsarIByteBufferAllocator.DEFAULT.buffer(HeadersSize);
			Headers.writeInt(TotalSize); // External frame

			try
			{
				// Write cmd
				Headers.writeInt(CmdSize);

				IByteBufferCodedOutputStream OutStream = IByteBufferCodedOutputStream.get(Headers);
				Cmd.writeTo(OutStream);
				OutStream.recycle();
			}
			catch (IOException E)
			{
				// This is in-memory serialization, should not fail
				throw new Exception(E);
			}

			return (IByteBufferPair) IByteBufferPair.Get(Headers, MetadataAndPayload);
		}

		public static int GetNumberOfMessagesInBatch(IByteBuffer MetadataAndPayload, string Subscription, long ConsumerId)
		{
			Proto.MessageMetadata MsgMetadata = PeekMessageMetadata(MetadataAndPayload, Subscription, ConsumerId);
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

		public static Proto.MessageMetadata PeekMessageMetadata(IByteBuffer MetadataAndPayload, string Subscription, long ConsumerId)
		{
			try
			{
				// save the reader index and restore after parsing
				int ReaderIdx = MetadataAndPayload.readerIndex();
				Proto.MessageMetadata Metadata = Commands.ParseMessageMetadata(MetadataAndPayload);
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
				return Proto.ProtocolVersion.values()[Proto.ProtocolVersion.values().length - 1].Number;
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
			return PeerVersion >= Proto.ProtocolVersion.v12.Number;
		}

		public static bool PeerSupportsActiveConsumerListener(int PeerVersion)
		{
			return PeerVersion >= Proto.ProtocolVersion.v12.Number;
		}

		public static bool PeerSupportsMultiMessageAcknowledgment(int PeerVersion)
		{
			return PeerVersion >= Proto.ProtocolVersion.v12.Number;
		}

		public static bool PeerSupportJsonSchemaAvroFormat(int PeerVersion)
		{
			return PeerVersion >= Proto.ProtocolVersion.v13.Number;
		}

		public static bool PeerSupportsGetOrCreateSchema(int PeerVersion)
		{
			return PeerVersion >= Proto.ProtocolVersion.v15.Number;
		}
	}

}