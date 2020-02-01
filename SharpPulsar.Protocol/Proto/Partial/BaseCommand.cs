using DotNetty.Common;
using Google.Protobuf;
using SharpPulsar.Util.Protobuf;
using System;
using static SharpPulsar.Util.Protobuf.ByteBufCodedInputStream;
using static SharpPulsar.Util.Protobuf.ByteBufCodedOutputStream;
using System.Linq;

namespace SharpPulsar.Protocol.Proto
{
	public partial class BaseCommand: ByteBufGeneratedMessage
	{
		// Use BaseCommand.newBuilder() to construct.
		internal static ThreadLocalPool<BaseCommand> _pool = new ThreadLocalPool<BaseCommand>(handle => new BaseCommand(handle), 1, true);

		internal ThreadLocalPool.Handle _handle;
		private BaseCommand(ThreadLocalPool.Handle handle)
		{
			_handle = handle;
		}
		internal int _bitField0 = 0;
		internal int _bitField1 = 0;
		public void Recycle()
		{
			InitFields();
			MemoizedIsInitialized = -1;
			_hasBits0 = 0;
			MemoizedSerializedSize = -1;
			if (_handle != null)
			{
				_handle.Release(this);
			}
		}

		public BaseCommand(bool NoInit)
		{
		}

		
		internal static readonly BaseCommand _defaultInstance;
		public static BaseCommand DefaultInstance
		{
			get
			{
				return _defaultInstance;
			}
		}

		public BaseCommand DefaultInstanceForType
		{
			get
			{
				return _defaultInstance;
			}
		}

		public void InitFields()
		{
			Type = Types.Type.Connect;
			Connect = CommandConnect.DefaultInstance;
			Connected = CommandConnected.DefaultInstance;
			Subscribe = CommandSubscribe.DefaultInstance;
			Producer = CommandProducer.DefaultInstance;
			Send = CommandSend.DefaultInstance;
			SendReceipt = CommandSendReceipt.DefaultInstance;
			SendError = CommandSendError.DefaultInstance;
			Message = CommandMessage.DefaultInstance;
			Ack = CommandAck.DefaultInstance;
			Flow = CommandFlow.DefaultInstance;
			Unsubscribe = CommandUnsubscribe.DefaultInstance;
			Success = CommandSuccess.DefaultInstance;
			Error = CommandError.DefaultInstance;
			CloseProducer = CommandCloseProducer.DefaultInstance;
			CloseConsumer = CommandCloseConsumer.DefaultInstance;
			ProducerSuccess = CommandProducerSuccess.DefaultInstance;
			Ping = CommandPing.DefaultInstance;
			Pong = CommandPong.DefaultInstance;
			RedeliverUnacknowledgedMessages = CommandRedeliverUnacknowledgedMessages.DefaultInstance;
			PartitionMetadata = CommandPartitionedTopicMetadata.DefaultInstance;
			PartitionMetadataResponse = CommandPartitionedTopicMetadataResponse.DefaultInstance;
			LookupTopic = CommandLookupTopic.DefaultInstance;
			LookupTopicResponse = CommandLookupTopicResponse.DefaultInstance;
			ConsumerStats = CommandConsumerStats.DefaultInstance;
			ConsumerStatsResponse = CommandConsumerStatsResponse.DefaultInstance;
			ReachedEndOfTopic = CommandReachedEndOfTopic.DefaultInstance;
			Seek = CommandSeek.DefaultInstance;
			GetLastMessageId = CommandGetLastMessageId.DefaultInstance;
			GetLastMessageIdResponse = CommandGetLastMessageIdResponse.DefaultInstance;
			ActiveConsumerChange = CommandActiveConsumerChange.DefaultInstance;
			GetTopicsOfNamespace = CommandGetTopicsOfNamespace.DefaultInstance;
			GetTopicsOfNamespaceResponse = CommandGetTopicsOfNamespaceResponse.DefaultInstance;
			GetSchema = CommandGetSchema.DefaultInstance;
			GetSchemaResponse = CommandGetSchemaResponse.DefaultInstance;
			AuthChallenge = CommandAuthChallenge.DefaultInstance;
			AuthResponse = CommandAuthResponse.DefaultInstance;
			AckResponse = CommandAckResponse.DefaultInstance;
			GetOrCreateSchema = CommandGetOrCreateSchema.DefaultInstance;
			GetOrCreateSchemaResponse = CommandGetOrCreateSchemaResponse.DefaultInstance;
			NewTxn = CommandNewTxn.DefaultInstance;
			NewTxnResponse = CommandNewTxnResponse.DefaultInstance;
			AddPartitionToTxn = CommandAddPartitionToTxn.DefaultInstance;
			AddPartitionToTxnResponse = CommandAddPartitionToTxnResponse.DefaultInstance;
			AddSubscriptionToTxn = CommandAddSubscriptionToTxn.DefaultInstance;
			AddSubscriptionToTxnResponse = CommandAddSubscriptionToTxnResponse.DefaultInstance;
			EndTxn = CommandEndTxn.DefaultInstance;
			EndTxnResponse = CommandEndTxnResponse.DefaultInstance;
			EndTxnOnPartition = CommandEndTxnOnPartition.DefaultInstance;
			EndTxnOnPartitionResponse = CommandEndTxnOnPartitionResponse.DefaultInstance;
			EndTxnOnSubscription = CommandEndTxnOnSubscription.DefaultInstance;
			EndTxnOnSubscriptionResponse = CommandEndTxnOnSubscriptionResponse.DefaultInstance;
		}
		internal sbyte MemoizedIsInitialized = -1;
		public bool Initialized
		{
			get
			{
				sbyte IsInitialized = MemoizedIsInitialized;
				if (IsInitialized != -1)
				{
					return IsInitialized == 1;
				}

				if (!HasType)
				{
					MemoizedIsInitialized = 0;
					return false;
				}
				if (HasConnect)
				{
					if (!Connect.IsInitialized())
					{
						MemoizedIsInitialized = 0;
						return false;
					}
				}
				if (HasConnected)
				{
					if (!Connected.IsInitialized())
					{
						MemoizedIsInitialized = 0;
						return false;
					}
				}
				if (HasSubscribe)
				{
					if (!Subscribe.IsInitialized())
					{
						MemoizedIsInitialized = 0;
						return false;
					}
				}
				if (HasProducer)
				{
					if (!Producer.IsInitialized())
					{
						MemoizedIsInitialized = 0;
						return false;
					}
				}
				if (HasSend)
				{
					if (!Send.IsInitialized())
					{
						MemoizedIsInitialized = 0;
						return false;
					}
				}
				if (HasSendReceipt)
				{
					if (!SendReceipt.IsInitialized())
					{
						MemoizedIsInitialized = 0;
						return false;
					}
				}
				if (HasSendError)
				{
					if (!SendError.IsInitialized())
					{
						MemoizedIsInitialized = 0;
						return false;
					}
				}
				if (HasMessage)
				{
					if (!Message.IsInitialized())
					{
						MemoizedIsInitialized = 0;
						return false;
					}
				}
				if (HasAck)
				{
					if (!Ack.IsInitialized())
					{
						MemoizedIsInitialized = 0;
						return false;
					}
				}
				if (HasFlow)
				{
					if (!Flow.IsInitialized())
					{
						MemoizedIsInitialized = 0;
						return false;
					}
				}
				if (HasUnsubscribe)
				{
					if (!Unsubscribe.IsInitialized())
					{
						MemoizedIsInitialized = 0;
						return false;
					}
				}
				if (HasSuccess)
				{
					if (!Success.IsInitialized())
					{
						MemoizedIsInitialized = 0;
						return false;
					}
				}
				if (HasError)
				{
					if (!Error.IsInitialized())
					{
						MemoizedIsInitialized = 0;
						return false;
					}
				}
				if (HasCloseProducer)
				{
					if (!CloseProducer.IsInitialized())
					{
						MemoizedIsInitialized = 0;
						return false;
					}
				}
				if (HasCloseConsumer)
				{
					if (!CloseConsumer.IsInitialized())
					{
						MemoizedIsInitialized = 0;
						return false;
					}
				}
				if (HasProducerSuccess)
				{
					if (!ProducerSuccess.IsInitialized())
					{
						MemoizedIsInitialized = 0;
						return false;
					}
				}
				if (HasRedeliverUnacknowledgedMessages)
				{
					if (!RedeliverUnacknowledgedMessages.IsInitialized())
					{
						MemoizedIsInitialized = 0;
						return false;
					}
				}
				if (HasPartitionMetadata)
				{
					if (!PartitionMetadata.IsInitialized())
					{
						MemoizedIsInitialized = 0;
						return false;
					}
				}
				if (HasPartitionMetadataResponse)
				{
					if (!PartitionMetadataResponse.IsInitialized())
					{
						MemoizedIsInitialized = 0;
						return false;
					}
				}
				if (HasLookupTopic)
				{
					if (!LookupTopic.IsInitialized())
					{
						MemoizedIsInitialized = 0;
						return false;
					}
				}
				if (HasLookupTopicResponse)
				{
					if (!LookupTopicResponse.IsInitialized())
					{
						MemoizedIsInitialized = 0;
						return false;
					}
				}
				if (HasConsumerStats)
				{
					if (!ConsumerStats.IsInitialized())
					{
						MemoizedIsInitialized = 0;
						return false;
					}
				}
				if (HasConsumerStatsResponse)
				{
					if (!ConsumerStatsResponse.IsInitialized())
					{
						MemoizedIsInitialized = 0;
						return false;
					}
				}
				if (HasReachedEndOfTopic)
				{
					if (!ReachedEndOfTopic.IsInitialized())
					{
						MemoizedIsInitialized = 0;
						return false;
					}
				}
				if (HasSeek)
				{
					if (!Seek.IsInitialized())
					{
						MemoizedIsInitialized = 0;
						return false;
					}
				}
				if (HasGetLastMessageId)
				{
					if (!GetLastMessageId.IsInitialized())
					{
						MemoizedIsInitialized = 0;
						return false;
					}
				}
				if (HasGetLastMessageIdResponse)
				{
					if (!GetLastMessageIdResponse.IsInitialized())
					{
						MemoizedIsInitialized = 0;
						return false;
					}
				}
				if (HasActiveConsumerChange)
				{
					if (!ActiveConsumerChange.IsInitialized())
					{
						MemoizedIsInitialized = 0;
						return false;
					}
				}
				if (HasGetTopicsOfNamespace)
				{
					if (!GetTopicsOfNamespace.IsInitialized())
					{
						MemoizedIsInitialized = 0;
						return false;
					}
				}
				if (HasGetTopicsOfNamespaceResponse)
				{
					if (!GetTopicsOfNamespaceResponse.IsInitialized())
					{
						MemoizedIsInitialized = 0;
						return false;
					}
				}
				if (HasGetSchema)
				{
					if (!GetSchema.IsInitialized())
					{
						MemoizedIsInitialized = 0;
						return false;
					}
				}
				if (HasGetSchemaResponse)
				{
					if (!GetSchemaResponse.IsInitialized())
					{
						MemoizedIsInitialized = 0;
						return false;
					}
				}
				if (HasAckResponse)
				{
					if (!AckResponse.IsInitialized())
					{
						MemoizedIsInitialized = 0;
						return false;
					}
				}
				if (HasGetOrCreateSchema)
				{
					if (!GetOrCreateSchema.IsInitialized())
					{
						MemoizedIsInitialized = 0;
						return false;
					}
				}
				if (HasGetOrCreateSchemaResponse)
				{
					if (!GetOrCreateSchemaResponse.IsInitialized())
					{
						MemoizedIsInitialized = 0;
						return false;
					}
				}
				if (HasNewTxn)
				{
					if (!NewTxn.IsInitialized())
					{
						MemoizedIsInitialized = 0;
						return false;
					}
				}
				if (HasNewTxnResponse)
				{
					if (!NewTxnResponse.IsInitialized())
					{
						MemoizedIsInitialized = 0;
						return false;
					}
				}
				if (HasAddPartitionToTxn)
				{
					if (!AddPartitionToTxn.IsInitialized())
					{
						MemoizedIsInitialized = 0;
						return false;
					}
				}
				if (HasAddPartitionToTxnResponse)
				{
					if (!AddPartitionToTxnResponse.IsInitialized())
					{
						MemoizedIsInitialized = 0;
						return false;
					}
				}
				if (HasAddSubscriptionToTxn)
				{
					if (!AddSubscriptionToTxn.IsInitialized())
					{
						MemoizedIsInitialized = 0;
						return false;
					}
				}
				if (HasAddSubscriptionToTxnResponse)
				{
					if (!AddSubscriptionToTxnResponse.IsInitialized())
					{
						MemoizedIsInitialized = 0;
						return false;
					}
				}
				if (HasEndTxn)
				{
					if (!EndTxn.IsInitialized())
					{
						MemoizedIsInitialized = 0;
						return false;
					}
				}
				if (HasEndTxnResponse)
				{
					if (!EndTxnResponse.IsInitialized())
					{
						MemoizedIsInitialized = 0;
						return false;
					}
				}
				if (HasEndTxnOnPartition)
				{
					if (!EndTxnOnPartition.IsInitialized())
					{
						MemoizedIsInitialized = 0;
						return false;
					}
				}
				if (HasEndTxnOnPartitionResponse)
				{
					if (!EndTxnOnPartitionResponse.IsInitialized())
					{
						MemoizedIsInitialized = 0;
						return false;
					}
				}
				if (HasEndTxnOnSubscription)
				{
					if (!EndTxnOnSubscription.IsInitialized())
					{
						MemoizedIsInitialized = 0;
						return false;
					}
				}
				if (HasEndTxnOnSubscriptionResponse)
				{
					if (!EndTxnOnSubscriptionResponse.IsInitialized())
					{
						MemoizedIsInitialized = 0;
						return false;
					}
				}
				MemoizedIsInitialized = 1;
				return true;
			}
		}

		public int SerializedSize => CalculateSize();

		internal int MemoizedSerializedSize = -1;
		

		internal const long SerialVersionUID = 0L;
		
		
		public static Builder NewBuilder()
		{
			return Builder.Create();
		}
		public Builder NewBuilderForType()
		{
			return NewBuilder();
		}
		public static Builder NewBuilder(BaseCommand prototype)
		{
			return NewBuilder().MergeFrom(prototype);
		}
		public Builder ToBuilder()
		{
			return NewBuilder(this);
		}
		public void WriteTo(ByteBufCodedOutputStream output)
		{
			var _ = SerializedSize;
			if (((_hasBits0 & 0x00000001) == 0x00000001))
			{
				output.WriteEnum(1, (int)type_);
			}
			if (((_hasBits0 & 0x00000002) == 0x00000002))
			{
				output.WriteMessage(2, Connect);
			}
			if (((_hasBits0 & 0x00000004) == 0x00000004))
			{
				output.WriteMessage(3, Connected);
			}
			if (((_hasBits0 & 0x00000008) == 0x00000008))
			{
				output.WriteMessage(4, Subscribe);
			}
			if (((_hasBits0 & 0x00000010) == 0x00000010))
			{
				output.WriteMessage(5, Producer);
			}
			if (((_hasBits0 & 0x00000020) == 0x00000020))
			{
				output.WriteMessage(6, Send);
			}
			if (((_hasBits0 & 0x00000040) == 0x00000040))
			{
				output.WriteMessage(7, SendReceipt);
			}
			if (((_hasBits0 & 0x00000080) == 0x00000080))
			{
				output.WriteMessage(8, SendError);
			}
			if (((_hasBits0 & 0x00000100) == 0x00000100))
			{
				output.WriteMessage(9, Message);
			}
			if (((_hasBits0 & 0x00000200) == 0x00000200))
			{
				output.WriteMessage(10, Ack);
			}
			if (((_hasBits0 & 0x00000400) == 0x00000400))
			{
				output.WriteMessage(11, Flow);
			}
			if (((_hasBits0 & 0x00000800) == 0x00000800))
			{
				output.WriteMessage(12, Unsubscribe);
			}
			if (((_hasBits0 & 0x00001000) == 0x00001000))
			{
				output.WriteMessage(13, Success);
			}
			if (((_hasBits0 & 0x00002000) == 0x00002000))
			{
				output.WriteMessage(14, Error);
			}
			if (((_hasBits0 & 0x00004000) == 0x00004000))
			{
				output.WriteMessage(15, CloseProducer);
			}
			if (((_hasBits0 & 0x00008000) == 0x00008000))
			{
				output.WriteMessage(16, CloseConsumer);
			}
			if (((_hasBits0 & 0x00010000) == 0x00010000))
			{
				output.WriteMessage(17, ProducerSuccess);
			}
			if (((_hasBits0 & 0x00020000) == 0x00020000))
			{
				output.WriteMessage(18, Ping);
			}
			if (((_hasBits0 & 0x00040000) == 0x00040000))
			{
				output.WriteMessage(19, Pong);
			}
			if (((_hasBits0 & 0x00080000) == 0x00080000))
			{
				output.WriteMessage(20, RedeliverUnacknowledgedMessages);
			}
			if (((_hasBits0 & 0x00100000) == 0x00100000))
			{
				output.WriteMessage(21, PartitionMetadata);
			}
			if (((_hasBits0 & 0x00200000) == 0x00200000))
			{
				output.WriteMessage(22, PartitionMetadataResponse);
			}
			if (((_hasBits0 & 0x00400000) == 0x00400000))
			{
				output.WriteMessage(23, LookupTopic);
			}
			if (((_hasBits0 & 0x00800000) == 0x00800000))
			{
				output.WriteMessage(24, LookupTopicResponse);
			}
			if (((_hasBits0 & 0x01000000) == 0x01000000))
			{
				output.WriteMessage(25, ConsumerStats);
			}
			if (((_hasBits0 & 0x02000000) == 0x02000000))
			{
				output.WriteMessage(26, ConsumerStatsResponse);
			}
			if (((_hasBits0 & 0x04000000) == 0x04000000))
			{
				output.WriteMessage(27, ReachedEndOfTopic);
			}
			if (((_hasBits0 & 0x08000000) == 0x08000000))
			{
				output.WriteMessage(28, Seek);
			}
			if (((_hasBits0 & 0x10000000) == 0x10000000))
			{
				output.WriteMessage(29, GetLastMessageId);
			}
			if (((_hasBits0 & 0x20000000) == 0x20000000))
			{
				output.WriteMessage(30, GetLastMessageIdResponse);
			}
			if (((_hasBits0 & 0x40000000) == 0x40000000))
			{
				output.WriteMessage(31, ActiveConsumerChange);
			}
			if (((_hasBits0 & 0x80000000) == 0x80000000))
			{
				output.WriteMessage(32, GetTopicsOfNamespace);
			}
			if (((_hasBits0 & 0x00000001) == 0x00000001))
			{
				output.WriteMessage(33, GetTopicsOfNamespaceResponse);
			}
			if (((_hasBits0 & 0x00000002) == 0x00000002))
			{
				output.WriteMessage(34, GetSchema);
			}
			if (((_hasBits0 & 0x00000004) == 0x00000004))
			{
				output.WriteMessage(35, GetSchemaResponse);
			}
			if (((_hasBits0 & 0x00000008) == 0x00000008))
			{
				output.WriteMessage(36, AuthChallenge);
			}
			if (((_hasBits0 & 0x00000010) == 0x00000010))
			{
				output.WriteMessage(37, AuthResponse);
			}
			if (((_hasBits0 & 0x00000020) == 0x00000020))
			{
				output.WriteMessage(38, AckResponse);
			}
			if (((_hasBits0 & 0x00000040) == 0x00000040))
			{
				output.WriteMessage(39, GetOrCreateSchema);
			}
			if (((_hasBits0 & 0x00000080) == 0x00000080))
			{
				output.WriteMessage(40, GetOrCreateSchemaResponse);
			}
			if (((_hasBits0 & 0x00000100) == 0x00000100))
			{
				output.WriteMessage(50, NewTxn);
			}
			if (((_hasBits0 & 0x00000200) == 0x00000200))
			{
				output.WriteMessage(51, NewTxnResponse);
			}
			if (((_hasBits0 & 0x00000400) == 0x00000400))
			{
				output.WriteMessage(52, AddPartitionToTxn);
			}
			if (((_hasBits0 & 0x00000800) == 0x00000800))
			{
				output.WriteMessage(53, AddPartitionToTxnResponse);
			}
			if (((_hasBits0 & 0x00001000) == 0x00001000))
			{
				output.WriteMessage(54, AddSubscriptionToTxn);
			}
			if (((_hasBits0 & 0x00002000) == 0x00002000))
			{
				output.WriteMessage(55, AddSubscriptionToTxnResponse);
			}
			if (((_hasBits0 & 0x00004000) == 0x00004000))
			{
				output.WriteMessage(56, EndTxn);
			}
			if (((_hasBits0 & 0x00008000) == 0x00008000))
			{
				output.WriteMessage(57, EndTxnResponse);
			}
			if (((_hasBits0 & 0x00010000) == 0x00010000))
			{
				output.WriteMessage(58, EndTxnOnPartition);
			}
			if (((_hasBits0 & 0x00020000) == 0x00020000))
			{
				output.WriteMessage(59, EndTxnOnPartitionResponse);
			}
			if (((_hasBits0 & 0x00040000) == 0x00040000))
			{
				output.WriteMessage(60, EndTxnOnSubscription);
			}
			if (((_hasBits0 & 0x00080000) == 0x00080000))
			{
				output.WriteMessage(61, EndTxnOnSubscriptionResponse);
			}
		}
		

		public  class Builder: ByteBufMessageBuilder
		{
			// Construct using org.apache.pulsar.common.api.proto.BaseCommand.newBuilder()
			internal static ThreadLocalPool<Builder> _pool = new ThreadLocalPool<Builder>(handle => new Builder(handle), 1, true);
			internal int _bitField0 = 0;
			internal int _bitField1 = 0;
			internal ThreadLocalPool.Handle _handle;
			private Builder(ThreadLocalPool.Handle handle)
			{
				_handle = handle;
				MaybeForceBuilderInitialization();
			}
			

			public void Recycle()
			{
				Clear();
				if (_handle != null)
				{
					_handle.Release(this);
				}
				
			}
			
			public void MaybeForceBuilderInitialization()
			{
			}
			internal static Builder Create()
			{
				return _pool.Take();
			}

			public Builder Clear()
			{
				ClearType();
				ClearConnect();
				ClearConnected();
				ClearSubscribe();
				ClearProducer();
				ClearSend();
				ClearSendReceipt();
				ClearSendError();
				ClearMessage();
				ClearAck();
				ClearFlow();
				ClearUnsubscribe();
				ClearSuccess();
				ClearError();
				ClearCloseProducer();
				ClearCloseConsumer();
				ClearProducerSuccess();
				ClearPing();
				ClearPong();
				ClearRedeliverUnacknowledgedMessages();
				ClearPartitionMetadata();
				ClearPartitionMetadataResponse();
				ClearLookupTopic();
				ClearLookupTopicResponse();
				ClearConsumerStats();
				ClearConsumerStatsResponse();
				ClearReachedEndOfTopic();
				ClearSeek();
				ClearGetLastMessageId();
				ClearGetLastMessageIdResponse();
				ClearActiveConsumerChange();
				ClearGetTopicsOfNamespace();
				ClearGetTopicsOfNamespaceResponse();
				ClearGetSchema();
				ClearGetSchemaResponse();
				ClearAuthChallenge();
				ClearAuthResponse();
				ClearAckResponse();
				ClearGetOrCreateSchema();
				ClearGetOrCreateSchemaResponse();
				ClearNewTxn();
				ClearNewTxnResponse();
				ClearAddPartitionToTxn();
				ClearAddPartitionToTxnResponse();
				ClearAddSubscriptionToTxn();
				ClearAddSubscriptionToTxnResponse();
				ClearEndTxn();
				ClearEndTxnResponse();
				ClearEndTxnOnPartition();
				ClearEndTxnOnPartitionResponse();
				ClearEndTxnOnSubscription();
				ClearEndTxnOnSubscriptionResponse();
				return this;
			}

			public BaseCommand DefaultInstanceForType
			{
				get
				{
					return DefaultInstance;
				}
			}

			public BaseCommand Build()
			{
				BaseCommand result = BuildPartial();
				if (!result.IsInitialized())
				{
					throw new NullReferenceException("BaseCommand not initialized");
				}
				return result;
			}
			Types.Type _type;
			
			public BaseCommand BuildPartial()
			{
				BaseCommand Result = BaseCommand._pool.Take();
				int From_bitField0 = _bitField0;
				int FromBitField1_ = _bitField1;
				int To_bitField0 = 0;
				int ToBitField1_ = 0;
				if (((From_bitField0 & 0x00000001) == 0x00000001))
				{
					To_bitField0 |= 0x00000001;
				}
				Result.Type = _type;
				if (((From_bitField0 & 0x00000002) == 0x00000002))
				{
					To_bitField0 |= 0x00000002;
				}
				Result.Connect = _connect;
				if (((From_bitField0 & 0x00000004) == 0x00000004))
				{
					To_bitField0 |= 0x00000004;
				}
				Result.Connected = _connected;
				if (((From_bitField0 & 0x00000008) == 0x00000008))
				{
					To_bitField0 |= 0x00000008;
				}
				Result.Subscribe = Subscribe_;
				if (((From_bitField0 & 0x00000010) == 0x00000010))
				{
					To_bitField0 |= 0x00000010;
				}
				Result.Producer = Producer_;
				if (((From_bitField0 & 0x00000020) == 0x00000020))
				{
					To_bitField0 |= 0x00000020;
				}
				Result.Send = Send_;
				if (((From_bitField0 & 0x00000040) == 0x00000040))
				{
					To_bitField0 |= 0x00000040;
				}
				Result.SendReceipt = SendReceipt_;
				if (((From_bitField0 & 0x00000080) == 0x00000080))
				{
					To_bitField0 |= 0x00000080;
				}
				Result.SendError = SendError_;
				if (((From_bitField0 & 0x00000100) == 0x00000100))
				{
					To_bitField0 |= 0x00000100;
				}
				Result.Message = Message_;
				if (((From_bitField0 & 0x00000200) == 0x00000200))
				{
					To_bitField0 |= 0x00000200;
				}
				Result.Ack = Ack_;
				if (((From_bitField0 & 0x00000400) == 0x00000400))
				{
					To_bitField0 |= 0x00000400;
				}
				Result.Flow = Flow_;
				if (((From_bitField0 & 0x00000800) == 0x00000800))
				{
					To_bitField0 |= 0x00000800;
				}
				Result.Unsubscribe = Unsubscribe_;
				if (((From_bitField0 & 0x00001000) == 0x00001000))
				{
					To_bitField0 |= 0x00001000;
				}
				Result.Success = Success_;
				if (((From_bitField0 & 0x00002000) == 0x00002000))
				{
					To_bitField0 |= 0x00002000;
				}
				Result.Error = Error_;
				if (((From_bitField0 & 0x00004000) == 0x00004000))
				{
					To_bitField0 |= 0x00004000;
				}
				Result.CloseProducer = CloseProducer_;
				if (((From_bitField0 & 0x00008000) == 0x00008000))
				{
					To_bitField0 |= 0x00008000;
				}
				Result.CloseConsumer = CloseConsumer_;
				if (((From_bitField0 & 0x00010000) == 0x00010000))
				{
					To_bitField0 |= 0x00010000;
				}
				Result.ProducerSuccess = ProducerSuccess_;
				if (((From_bitField0 & 0x00020000) == 0x00020000))
				{
					To_bitField0 |= 0x00020000;
				}
				Result.Ping = Ping_;
				if (((From_bitField0 & 0x00040000) == 0x00040000))
				{
					To_bitField0 |= 0x00040000;
				}
				Result.Pong = Pong_;
				if (((From_bitField0 & 0x00080000) == 0x00080000))
				{
					To_bitField0 |= 0x00080000;
				}
				Result.RedeliverUnacknowledgedMessages = RedeliverUnacknowledgedMessages_;
				if (((From_bitField0 & 0x00100000) == 0x00100000))
				{
					To_bitField0 |= 0x00100000;
				}
				Result.PartitionMetadata = PartitionMetadata_;
				if (((From_bitField0 & 0x00200000) == 0x00200000))
				{
					To_bitField0 |= 0x00200000;
				}
				Result.PartitionMetadataResponse = PartitionMetadataResponse_;
				if (((From_bitField0 & 0x00400000) == 0x00400000))
				{
					To_bitField0 |= 0x00400000;
				}
				Result.LookupTopic = LookupTopic_;
				if (((From_bitField0 & 0x00800000) == 0x00800000))
				{
					To_bitField0 |= 0x00800000;
				}
				Result.LookupTopicResponse = LookupTopicResponse_;
				if (((From_bitField0 & 0x01000000) == 0x01000000))
				{
					To_bitField0 |= 0x01000000;
				}
				Result.ConsumerStats = ConsumerStats_;
				if (((From_bitField0 & 0x02000000) == 0x02000000))
				{
					To_bitField0 |= 0x02000000;
				}
				Result.ConsumerStatsResponse = ConsumerStatsResponse_;
				if (((From_bitField0 & 0x04000000) == 0x04000000))
				{
					To_bitField0 |= 0x04000000;
				}
				Result.ReachedEndOfTopic = ReachedEndOfTopic_;
				if (((From_bitField0 & 0x08000000) == 0x08000000))
				{
					To_bitField0 |= 0x08000000;
				}
				Result.Seek = Seek_;
				if (((From_bitField0 & 0x10000000) == 0x10000000))
				{
					To_bitField0 |= 0x10000000;
				}
				Result.GetLastMessageId = GetLastMessageId_;
				if (((From_bitField0 & 0x20000000) == 0x20000000))
				{
					To_bitField0 |= 0x20000000;
				}
				Result.GetLastMessageIdResponse = GetLastMessageIdResponse_;
				if (((From_bitField0 & 0x40000000) == 0x40000000))
				{
					To_bitField0 |= 0x40000000;
				}
				Result.ActiveConsumerChange = ActiveConsumerChange_;
				if (((From_bitField0 & 0x80000000) == 0x80000000))
				{
					To_bitField0 |= unchecked((int)0x80000000);
				}
				Result.GetTopicsOfNamespace = GetTopicsOfNamespace_;
				if (((FromBitField1_ & 0x00000001) == 0x00000001))
				{
					ToBitField1_ |= 0x00000001;
				}
				Result.GetTopicsOfNamespaceResponse = GetTopicsOfNamespaceResponse_;
				if (((FromBitField1_ & 0x00000002) == 0x00000002))
				{
					ToBitField1_ |= 0x00000002;
				}
				Result.GetSchema = GetSchema_;
				if (((FromBitField1_ & 0x00000004) == 0x00000004))
				{
					ToBitField1_ |= 0x00000004;
				}
				Result.GetSchemaResponse = GetSchemaResponse_;
				if (((FromBitField1_ & 0x00000008) == 0x00000008))
				{
					ToBitField1_ |= 0x00000008;
				}
				Result.AuthChallenge = AuthChallenge_;
				if (((FromBitField1_ & 0x00000010) == 0x00000010))
				{
					ToBitField1_ |= 0x00000010;
				}
				Result.AuthResponse = AuthResponse_;
				if (((FromBitField1_ & 0x00000020) == 0x00000020))
				{
					ToBitField1_ |= 0x00000020;
				}
				Result.AckResponse = AckResponse_;
				if (((FromBitField1_ & 0x00000040) == 0x00000040))
				{
					ToBitField1_ |= 0x00000040;
				}
				Result.GetOrCreateSchema = GetOrCreateSchema_;
				if (((FromBitField1_ & 0x00000080) == 0x00000080))
				{
					ToBitField1_ |= 0x00000080;
				}
				Result.GetOrCreateSchemaResponse = GetOrCreateSchemaResponse_;
				if (((FromBitField1_ & 0x00000100) == 0x00000100))
				{
					ToBitField1_ |= 0x00000100;
				}
				Result.NewTxn = NewTxn_;
				if (((FromBitField1_ & 0x00000200) == 0x00000200))
				{
					ToBitField1_ |= 0x00000200;
				}
				Result.NewTxnResponse = NewTxnResponse_;
				if (((FromBitField1_ & 0x00000400) == 0x00000400))
				{
					ToBitField1_ |= 0x00000400;
				}
				Result.AddPartitionToTxn = AddPartitionToTxn_;
				if (((FromBitField1_ & 0x00000800) == 0x00000800))
				{
					ToBitField1_ |= 0x00000800;
				}
				Result.AddPartitionToTxnResponse = AddPartitionToTxnResponse_;
				if (((FromBitField1_ & 0x00001000) == 0x00001000))
				{
					ToBitField1_ |= 0x00001000;
				}
				Result.AddSubscriptionToTxn = AddSubscriptionToTxn_;
				if (((FromBitField1_ & 0x00002000) == 0x00002000))
				{
					ToBitField1_ |= 0x00002000;
				}
				Result.AddSubscriptionToTxnResponse = AddSubscriptionToTxnResponse_;
				if (((FromBitField1_ & 0x00004000) == 0x00004000))
				{
					ToBitField1_ |= 0x00004000;
				}
				Result.EndTxn = EndTxn_;
				if (((FromBitField1_ & 0x00008000) == 0x00008000))
				{
					ToBitField1_ |= 0x00008000;
				}
				Result.EndTxnResponse = EndTxnResponse_;
				if (((FromBitField1_ & 0x00010000) == 0x00010000))
				{
					ToBitField1_ |= 0x00010000;
				}
				Result.EndTxnOnPartition = EndTxnOnPartition_;
				if (((FromBitField1_ & 0x00020000) == 0x00020000))
				{
					ToBitField1_ |= 0x00020000;
				}
				Result.EndTxnOnPartitionResponse = EndTxnOnPartitionResponse_;
				if (((FromBitField1_ & 0x00040000) == 0x00040000))
				{
					ToBitField1_ |= 0x00040000;
				}
				Result.EndTxnOnSubscription = _endTxnOnSubscription;
				if (((FromBitField1_ & 0x00080000) == 0x00080000))
				{
					ToBitField1_ |= 0x00080000;
				}
				Result.EndTxnOnSubscriptionResponse = _endTxnOnSubscriptionResponse;
				Result._hasBits0 = To_bitField0;
				Result._bitField1 = ToBitField1_;
				return Result;
			}

			public Builder MergeFrom(BaseCommand Other)
			{
				if (Other == DefaultInstance)
				{
					return this;
				}
				if (Other.HasType)
				{
					SetType(Other.Type);
				}
				if (Other.HasConnect)
				{
					MergeConnect(Other.Connect);
				}
				if (Other.HasConnected)
				{
					MergeConnected(Other.Connected);
				}
				if (Other.HasSubscribe)
				{
					MergeSubscribe(Other.Subscribe);
				}
				if (Other.HasProducer)
				{
					MergeProducer(Other.Producer);
				}
				if (Other.HasSend)
				{
					MergeSend(Other.Send);
				}
				if (Other.HasSendReceipt)
				{
					MergeSendReceipt(Other.SendReceipt);
				}
				if (Other.HasSendError)
				{
					MergeSendError(Other.SendError);
				}
				if (Other.HasMessage)
				{
					MergeMessage(Other.Message);
				}
				if (Other.HasAck)
				{
					MergeAck(Other.Ack);
				}
				if (Other.HasFlow)
				{
					MergeFlow(Other.Flow);
				}
				if (Other.HasUnsubscribe)
				{
					MergeUnsubscribe(Other.Unsubscribe);
				}
				if (Other.HasSuccess)
				{
					MergeSuccess(Other.Success);
				}
				if (Other.HasError)
				{
					MergeError(Other.Error);
				}
				if (Other.HasCloseProducer)
				{
					MergeCloseProducer(Other.CloseProducer);
				}
				if (Other.HasCloseConsumer)
				{
					MergeCloseConsumer(Other.CloseConsumer);
				}
				if (Other.HasProducerSuccess)
				{
					MergeProducerSuccess(Other.ProducerSuccess);
				}
				if (Other.HasPing)
				{
					MergePing(Other.Ping);
				}
				if (Other.HasPong)
				{
					MergePong(Other.Pong);
				}
				if (Other.HasRedeliverUnacknowledgedMessages)
				{
					MergeRedeliverUnacknowledgedMessages(Other.RedeliverUnacknowledgedMessages);
				}
				if (Other.HasPartitionMetadata)
				{
					MergePartitionMetadata(Other.PartitionMetadata);
				}
				if (Other.HasPartitionMetadataResponse)
				{
					MergePartitionMetadataResponse(Other.PartitionMetadataResponse);
				}
				if (Other.HasLookupTopic)
				{
					MergeLookupTopic(Other.LookupTopic);
				}
				if (Other.HasLookupTopicResponse)
				{
					MergeLookupTopicResponse(Other.LookupTopicResponse);
				}
				if (Other.HasConsumerStats)
				{
					MergeConsumerStats(Other.ConsumerStats);
				}
				if (Other.HasConsumerStatsResponse)
				{
					MergeConsumerStatsResponse(Other.ConsumerStatsResponse);
				}
				if (Other.HasReachedEndOfTopic)
				{
					MergeReachedEndOfTopic(Other.ReachedEndOfTopic);
				}
				if (Other.HasSeek)
				{
					MergeSeek(Other.Seek);
				}
				if (Other.HasGetLastMessageId)
				{
					MergeGetLastMessageId(Other.GetLastMessageId);
				}
				if (Other.HasGetLastMessageIdResponse)
				{
					MergeGetLastMessageIdResponse(Other.GetLastMessageIdResponse);
				}
				if (Other.HasActiveConsumerChange)
				{
					MergeActiveConsumerChange(Other.ActiveConsumerChange);
				}
				if (Other.HasGetTopicsOfNamespace)
				{
					MergeGetTopicsOfNamespace(Other.GetTopicsOfNamespace);
				}
				if (Other.HasGetTopicsOfNamespaceResponse)
				{
					MergeGetTopicsOfNamespaceResponse(Other.GetTopicsOfNamespaceResponse);
				}
				if (Other.HasGetSchema)
				{
					MergeGetSchema(Other.GetSchema);
				}
				if (Other.HasGetSchemaResponse)
				{
					MergeGetSchemaResponse(Other.GetSchemaResponse);
				}
				if (Other.HasAuthChallenge)
				{
					MergeAuthChallenge(Other.AuthChallenge);
				}
				if (Other.HasAuthResponse)
				{
					MergeAuthResponse(Other.AuthResponse);
				}
				if (Other.HasAckResponse)
				{
					MergeAckResponse(Other.AckResponse);
				}
				if (Other.HasGetOrCreateSchema)
				{
					MergeGetOrCreateSchema(Other.GetOrCreateSchema);
				}
				if (Other.HasGetOrCreateSchemaResponse)
				{
					MergeGetOrCreateSchemaResponse(Other.GetOrCreateSchemaResponse);
				}
				if (Other.HasNewTxn)
				{
					MergeNewTxn(Other.NewTxn);
				}
				if (Other.HasNewTxnResponse)
				{
					MergeNewTxnResponse(Other.NewTxnResponse);
				}
				if (Other.HasAddPartitionToTxn)
				{
					MergeAddPartitionToTxn(Other.AddPartitionToTxn);
				}
				if (Other.HasAddPartitionToTxnResponse)
				{
					MergeAddPartitionToTxnResponse(Other.AddPartitionToTxnResponse);
				}
				if (Other.HasAddSubscriptionToTxn)
				{
					MergeAddSubscriptionToTxn(Other.AddSubscriptionToTxn);
				}
				if (Other.HasAddSubscriptionToTxnResponse)
				{
					MergeAddSubscriptionToTxnResponse(Other.AddSubscriptionToTxnResponse);
				}
				if (Other.HasEndTxn)
				{
					MergeEndTxn(Other.EndTxn);
				}
				if (Other.HasEndTxnResponse)
				{
					MergeEndTxnResponse(Other.EndTxnResponse);
				}
				if (Other.HasEndTxnOnPartition)
				{
					MergeEndTxnOnPartition(Other.EndTxnOnPartition);
				}
				if (Other.HasEndTxnOnPartitionResponse)
				{
					MergeEndTxnOnPartitionResponse(Other.EndTxnOnPartitionResponse);
				}
				if (Other.HasEndTxnOnSubscription)
				{
					MergeEndTxnOnSubscription(Other.EndTxnOnSubscription);
				}
				if (Other.HasEndTxnOnSubscriptionResponse)
				{
					MergeEndTxnOnSubscriptionResponse(Other.EndTxnOnSubscriptionResponse);
				}
				return this;
			}
			public ByteBufMessageBuilder MergeFrom(ByteBufCodedInputStream input, ExtensionRegistry extensionRegistry)
			{
				while (true)
				{
					int Tag = input.ReadTag();
					switch (Tag)
					{
						case 0:

							return this;
						default:
							{
								if (!input.SkipField(Tag))
								{

									return this;
								}
								break;
							}
						case 8:
							{
								int RawValue = input.ReadEnum();
								Types.Type Value = Enum.GetValues(typeof(Types.Type)).Cast<Types.Type>().ToList()[RawValue];
								if (Value != null)
								{
									_bitField0 |= 0x00000001;
									_type = Value;
								}
								break;
							}
						case 18:
							{
								CommandConnect.Builder SubBuilder = CommandConnect.NewBuilder();
								if (HasConnect())
								{
									SubBuilder.MergeFrom(GetConnect());
								}
								input.ReadMessage(SubBuilder, extensionRegistry);
								SetConnect(SubBuilder.BuildPartial());
								SubBuilder.Recycle();
								break;
							}
						case 26:
							{
								CommandConnected.Builder SubBuilder = CommandConnected.NewBuilder();
								if (HasConnected())
								{
									SubBuilder.MergeFrom(GetConnected());
								}
								input.ReadMessage(SubBuilder, extensionRegistry);
								SetConnected(SubBuilder);
								SubBuilder.Recycle();
								break;
							}
						case 34:
							{
								CommandSubscribe.Builder SubBuilder = CommandSubscribe.NewBuilder();
								if (HasSubscribe())
								{
									SubBuilder.MergeFrom(GetSubscribe());
								}
								input.ReadMessage(SubBuilder, extensionRegistry);
								SetSubscribe(SubBuilder.BuildPartial());
								SubBuilder.Recycle();
								break;
							}
						case 42:
							{
								CommandProducer.Builder SubBuilder = CommandProducer.NewBuilder();
								if (HasProducer())
								{
									SubBuilder.MergeFrom(GetProducer());
								}
								input.ReadMessage(SubBuilder, extensionRegistry);
								SetProducer(SubBuilder.BuildPartial());
								SubBuilder.Recycle();
								break;
							}
						case 50:
							{
								CommandSend.Builder SubBuilder = CommandSend.NewBuilder();
								if (HasSend())
								{
									SubBuilder.MergeFrom(GetSend());
								}
								input.ReadMessage(SubBuilder, extensionRegistry);
								SetSend(SubBuilder.BuildPartial());
								SubBuilder.Recycle();
								break;
							}
						case 58:
							{
								CommandSendReceipt.Builder SubBuilder = CommandSendReceipt.NewBuilder();
								if (HasSendReceipt())
								{
									SubBuilder.MergeFrom(GetSendReceipt());
								}
								input.ReadMessage(SubBuilder, extensionRegistry);
								SetSendReceipt(SubBuilder.BuildPartial());
								SubBuilder.Recycle();
								break;
							}
						case 66:
							{
								CommandSendError.Builder SubBuilder = CommandSendError.NewBuilder();
								if (HasSendError())
								{
									SubBuilder.MergeFrom(GetSendError());
								}
								input.ReadMessage(SubBuilder, extensionRegistry);
								SetSendError(SubBuilder.BuildPartial());
								SubBuilder.Recycle();
								break;
							}
						case 74:
							{
								CommandMessage.Builder SubBuilder = CommandMessage.NewBuilder();
								if (HasMessage())
								{
									SubBuilder.MergeFrom(GetMessage());
								}
								input.ReadMessage(SubBuilder, extensionRegistry);
								SetMessage(SubBuilder.BuildPartial());
								SubBuilder.Recycle();
								break;
							}
						case 82:
							{
								CommandAck.Builder SubBuilder = CommandAck.NewBuilder();
								if (HasAck())
								{
									SubBuilder.MergeFrom(GetAck());
								}
								input.ReadMessage(SubBuilder, extensionRegistry);
								SetAck(SubBuilder.BuildPartial());
								SubBuilder.Recycle();
								break;
							}
						case 90:
							{
								CommandFlow.Builder SubBuilder = CommandFlow.NewBuilder();
								if (HasFlow())
								{
									SubBuilder.MergeFrom(GetFlow());
								}
								input.ReadMessage(SubBuilder, extensionRegistry);
								SetFlow(SubBuilder.BuildPartial());
								SubBuilder.Recycle();
								break;
							}
						case 98:
							{
								CommandUnsubscribe.Builder SubBuilder = CommandUnsubscribe.NewBuilder();
								if (HasUnsubscribe())
								{
									SubBuilder.MergeFrom(GetUnsubscribe());
								}
								input.ReadMessage(SubBuilder, extensionRegistry);
								SetUnsubscribe(SubBuilder.BuildPartial());
								SubBuilder.Recycle();
								break;
							}
						case 106:
							{
								CommandSuccess.Builder SubBuilder = CommandSuccess.NewBuilder();
								if (HasSuccess())
								{
									SubBuilder.MergeFrom(GetSuccess());
								}
								input.ReadMessage(SubBuilder, extensionRegistry);
								SetSuccess(SubBuilder.BuildPartial());
								SubBuilder.Recycle();
								break;
							}
						case 114:
							{
								CommandError.Builder SubBuilder = CommandError.NewBuilder();
								if (HasError())
								{
									SubBuilder.MergeFrom(GetError());
								}
								input.ReadMessage(SubBuilder, extensionRegistry);
								SetError(SubBuilder.BuildPartial());
								SubBuilder.Recycle();
								break;
							}
						case 122:
							{
								CommandCloseProducer.Builder SubBuilder = CommandCloseProducer.NewBuilder();
								if (HasCloseProducer())
								{
									SubBuilder.MergeFrom(GetCloseProducer());
								}
								input.ReadMessage(SubBuilder, extensionRegistry);
								SetCloseProducer(SubBuilder.BuildPartial());
								SubBuilder.Recycle();
								break;
							}
						case 130:
							{
								CommandCloseConsumer.Builder SubBuilder = CommandCloseConsumer.NewBuilder();
								if (HasCloseConsumer())
								{
									SubBuilder.MergeFrom(GetCloseConsumer());
								}
								input.ReadMessage(SubBuilder, extensionRegistry);
								SetCloseConsumer(SubBuilder.BuildPartial());
								SubBuilder.Recycle();
								break;
							}
						case 138:
							{
								CommandProducerSuccess.Builder SubBuilder = CommandProducerSuccess.NewBuilder();
								if (HasProducerSuccess())
								{
									SubBuilder.MergeFrom(GetProducerSuccess());
								}
								input.ReadMessage(SubBuilder, extensionRegistry);
								SetProducerSuccess(SubBuilder.BuildPartial());
								SubBuilder.Recycle();
								break;
							}
						case 146:
							{
								CommandPing.Builder SubBuilder = CommandPing.NewBuilder();
								if (HasPing())
								{
									SubBuilder.MergeFrom(GetPing());
								}
								input.ReadMessage(SubBuilder, extensionRegistry);
								SetPing(SubBuilder.BuildPartial());
								SubBuilder.Recycle();
								break;
							}
						case 154:
							{
								CommandPong.Builder SubBuilder = CommandPong.NewBuilder();
								if (HasPong())
								{
									SubBuilder.MergeFrom(GetPong());
								}
								input.ReadMessage(SubBuilder, extensionRegistry);
								SetPong(SubBuilder.BuildPartial());
								SubBuilder.Recycle();
								break;
							}
						case 162:
							{
								CommandRedeliverUnacknowledgedMessages.Builder SubBuilder = CommandRedeliverUnacknowledgedMessages.NewBuilder();
								if (HasRedeliverUnacknowledgedMessages())
								{
									SubBuilder.MergeFrom(GetRedeliverUnacknowledgedMessages());
								}
								input.ReadMessage(SubBuilder, extensionRegistry);
								SetRedeliverUnacknowledgedMessages(SubBuilder.BuildPartial());
								SubBuilder.Recycle();
								break;
							}
						case 170:
							{
								CommandPartitionedTopicMetadata.Builder SubBuilder = CommandPartitionedTopicMetadata.NewBuilder();
								if (HasPartitionMetadata())
								{
									SubBuilder.MergeFrom(GetPartitionMetadata());
								}
								input.ReadMessage(SubBuilder, extensionRegistry);
								SetPartitionMetadata(SubBuilder.BuildPartial());
								SubBuilder.Recycle();
								break;
							}
						case 178:
							{
								CommandPartitionedTopicMetadataResponse.Builder SubBuilder = CommandPartitionedTopicMetadataResponse.NewBuilder();
								if (HasPartitionMetadataResponse())
								{
									SubBuilder.MergeFrom(GetPartitionMetadataResponse());
								}
								input.ReadMessage(SubBuilder, extensionRegistry);
								SetPartitionMetadataResponse(SubBuilder.BuildPartial());
								SubBuilder.Recycle();
								break;
							}
						case 186:
							{
								CommandLookupTopic.Builder SubBuilder = CommandLookupTopic.NewBuilder();
								if (HasLookupTopic())
								{
									SubBuilder.MergeFrom(GetLookupTopic());
								}
								input.ReadMessage(SubBuilder, extensionRegistry);
								SetLookupTopic(SubBuilder.BuildPartial());
								SubBuilder.Recycle();
								break;
							}
						case 194:
							{
								CommandLookupTopicResponse.Builder SubBuilder = CommandLookupTopicResponse.NewBuilder();
								if (HasLookupTopicResponse())
								{
									SubBuilder.MergeFrom(GetLookupTopicResponse());
								}
								input.ReadMessage(SubBuilder, extensionRegistry);
								SetLookupTopicResponse(SubBuilder.BuildPartial());
								SubBuilder.Recycle();
								break;
							}
						case 202:
							{
								CommandConsumerStats.Builder SubBuilder = CommandConsumerStats.NewBuilder();
								if (HasConsumerStats())
								{
									SubBuilder.MergeFrom(GetConsumerStats());
								}
								input.ReadMessage(SubBuilder, extensionRegistry);
								SetConsumerStats(SubBuilder.BuildPartial());
								SubBuilder.Recycle();
								break;
							}
						case 210:
							{
								CommandConsumerStatsResponse.Builder SubBuilder = CommandConsumerStatsResponse.NewBuilder();
								if (HasConsumerStatsResponse())
								{
									SubBuilder.MergeFrom(GetConsumerStatsResponse());
								}
								input.ReadMessage(SubBuilder, extensionRegistry);
								SetConsumerStatsResponse(SubBuilder.BuildPartial());
								SubBuilder.Recycle();
								break;
							}
						case 218:
							{
								CommandReachedEndOfTopic.Builder SubBuilder = CommandReachedEndOfTopic.NewBuilder();
								if (HasReachedEndOfTopic())
								{
									SubBuilder.MergeFrom(GetReachedEndOfTopic());
								}
								input.ReadMessage(SubBuilder, extensionRegistry);
								SetReachedEndOfTopic(SubBuilder.BuildPartial());
								SubBuilder.Recycle();
								break;
							}
						case 226:
							{
								CommandSeek.Builder SubBuilder = CommandSeek.NewBuilder();
								if (HasSeek())
								{
									SubBuilder.MergeFrom(GetSeek());
								}
								input.ReadMessage(SubBuilder, extensionRegistry);
								SetSeek(SubBuilder.BuildPartial());
								SubBuilder.Recycle();
								break;
							}
						case 234:
							{
								CommandGetLastMessageId.Builder SubBuilder = CommandGetLastMessageId.NewBuilder();
								if (HasGetLastMessageId())
								{
									SubBuilder.MergeFrom(GetGetLastMessageId());
								}
								input.ReadMessage(SubBuilder, extensionRegistry);
								SetGetLastMessageId(SubBuilder.BuildPartial());
								SubBuilder.Recycle();
								break;
							}
						case 242:
							{
								CommandGetLastMessageIdResponse.Builder SubBuilder = CommandGetLastMessageIdResponse.NewBuilder();
								if (HasGetLastMessageIdResponse())
								{
									SubBuilder.MergeFrom(GetGetLastMessageIdResponse());
								}
								input.ReadMessage(SubBuilder, extensionRegistry);
								SetGetLastMessageIdResponse(SubBuilder.BuildPartial());
								SubBuilder.Recycle();
								break;
							}
						case 250:
							{
								CommandActiveConsumerChange.Builder SubBuilder = CommandActiveConsumerChange.NewBuilder();
								if (HasActiveConsumerChange())
								{
									SubBuilder.MergeFrom(GetActiveConsumerChange());
								}
								input.ReadMessage(SubBuilder, extensionRegistry);
								SetActiveConsumerChange(SubBuilder.BuildPartial());
								SubBuilder.Recycle();
								break;
							}
						case 258:
							{
								CommandGetTopicsOfNamespace.Builder SubBuilder = CommandGetTopicsOfNamespace.NewBuilder();
								if (HasGetTopicsOfNamespace())
								{
									SubBuilder.MergeFrom(GetGetTopicsOfNamespace());
								}
								input.ReadMessage(SubBuilder, extensionRegistry);
								SetGetTopicsOfNamespace(SubBuilder.BuildPartial());
								SubBuilder.Recycle();
								break;
							}
						case 266:
							{
								CommandGetTopicsOfNamespaceResponse.Builder SubBuilder = CommandGetTopicsOfNamespaceResponse.NewBuilder();
								if (HasGetTopicsOfNamespaceResponse())
								{
									SubBuilder.MergeFrom(GetGetTopicsOfNamespaceResponse());
								}
								input.ReadMessage(SubBuilder, extensionRegistry);
								SetGetTopicsOfNamespaceResponse(SubBuilder.BuildPartial());
								SubBuilder.Recycle();
								break;
							}
						case 274:
							{
								CommandGetSchema.Builder SubBuilder = CommandGetSchema.NewBuilder();
								if (HasGetSchema())
								{
									SubBuilder.MergeFrom(GetGetSchema());
								}
								input.ReadMessage(SubBuilder, extensionRegistry);
								SetGetSchema(SubBuilder.BuildPartial());
								SubBuilder.Recycle();
								break;
							}
						case 282:
							{
								CommandGetSchemaResponse.Builder SubBuilder = CommandGetSchemaResponse.NewBuilder();
								if (HasGetSchemaResponse())
								{
									SubBuilder.MergeFrom(GetGetSchemaResponse());
								}
								input.ReadMessage(SubBuilder, extensionRegistry);
								SetGetSchemaResponse(SubBuilder.BuildPartial());
								SubBuilder.Recycle();
								break;
							}
						case 290:
							{
								CommandAuthChallenge.Builder SubBuilder = CommandAuthChallenge.NewBuilder();
								if (HasAuthChallenge())
								{
									SubBuilder.MergeFrom(GetAuthChallenge());
								}
								input.ReadMessage(SubBuilder, extensionRegistry);
								SetAuthChallenge(SubBuilder.BuildPartial());
								SubBuilder.Recycle();
								break;
							}
						case 298:
							{
								CommandAuthResponse.Builder SubBuilder = CommandAuthResponse.NewBuilder();
								if (HasAuthResponse())
								{
									SubBuilder.MergeFrom(GetAuthResponse());
								}
								input.ReadMessage(SubBuilder, extensionRegistry);
								SetAuthResponse(SubBuilder.BuildPartial());
								SubBuilder.Recycle();
								break;
							}
						case 306:
							{
								CommandAckResponse.Builder SubBuilder = CommandAckResponse.NewBuilder();
								if (HasAckResponse())
								{
									SubBuilder.MergeFrom(GetAckResponse());
								}
								input.ReadMessage(SubBuilder, extensionRegistry);
								SetAckResponse(SubBuilder.BuildPartial());
								SubBuilder.Recycle();
								break;
							}
						case 314:
							{
								CommandGetOrCreateSchema.Builder SubBuilder = CommandGetOrCreateSchema.NewBuilder();
								if (HasGetOrCreateSchema())
								{
									SubBuilder.MergeFrom(GetGetOrCreateSchema());
								}
								input.ReadMessage(SubBuilder, extensionRegistry);
								SetGetOrCreateSchema(SubBuilder.BuildPartial());
								SubBuilder.Recycle();
								break;
							}
						case 322:
							{
								CommandGetOrCreateSchemaResponse.Builder SubBuilder = CommandGetOrCreateSchemaResponse.NewBuilder();
								if (HasGetOrCreateSchemaResponse())
								{
									SubBuilder.MergeFrom(GetGetOrCreateSchemaResponse());
								}
								input.ReadMessage(SubBuilder, extensionRegistry);
								SetGetOrCreateSchemaResponse(SubBuilder.BuildPartial());
								SubBuilder.Recycle();
								break;
							}
						case 402:
							{
								CommandNewTxn.Builder SubBuilder = CommandNewTxn.NewBuilder();
								if (HasNewTxn())
								{
									SubBuilder.MergeFrom(GetNewTxn());
								}
								input.ReadMessage(SubBuilder, extensionRegistry);
								SetNewTxn(SubBuilder.BuildPartial());
								SubBuilder.Recycle();
								break;
							}
						case 410:
							{
								CommandNewTxnResponse.Builder SubBuilder = CommandNewTxnResponse.NewBuilder();
								if (HasNewTxnResponse())
								{
									SubBuilder.MergeFrom(GetNewTxnResponse());
								}
								input.ReadMessage(SubBuilder, extensionRegistry);
								SetNewTxnResponse(SubBuilder.BuildPartial());
								SubBuilder.Recycle();
								break;
							}
						case 418:
							{
								CommandAddPartitionToTxn.Builder SubBuilder = CommandAddPartitionToTxn.NewBuilder();
								if (HasAddPartitionToTxn())
								{
									SubBuilder.MergeFrom(GetAddPartitionToTxn());
								}
								input.ReadMessage(SubBuilder, extensionRegistry);
								SetAddPartitionToTxn(SubBuilder.BuildPartial());
								SubBuilder.Recycle();
								break;
							}
						case 426:
							{
								CommandAddPartitionToTxnResponse.Builder SubBuilder = CommandAddPartitionToTxnResponse.NewBuilder();
								if (HasAddPartitionToTxnResponse())
								{
									SubBuilder.MergeFrom(GetAddPartitionToTxnResponse());
								}
								input.ReadMessage(SubBuilder, extensionRegistry);
								SetAddPartitionToTxnResponse(SubBuilder.BuildPartial());
								SubBuilder.Recycle();
								break;
							}
						case 434:
							{
								CommandAddSubscriptionToTxn.Builder SubBuilder = CommandAddSubscriptionToTxn.NewBuilder();
								if (HasAddSubscriptionToTxn())
								{
									SubBuilder.MergeFrom(GetAddSubscriptionToTxn());
								}
								input.ReadMessage(SubBuilder, extensionRegistry);
								SetAddSubscriptionToTxn(SubBuilder.BuildPartial());
								SubBuilder.Recycle();
								break;
							}
						case 442:
							{
								CommandAddSubscriptionToTxnResponse.Builder SubBuilder = CommandAddSubscriptionToTxnResponse.NewBuilder();
								if (HasAddSubscriptionToTxnResponse())
								{
									SubBuilder.MergeFrom(GetAddSubscriptionToTxnResponse());
								}
								input.ReadMessage(SubBuilder, extensionRegistry);
								SetAddSubscriptionToTxnResponse(SubBuilder.BuildPartial());
								SubBuilder.Recycle();
								break;
							}
						case 450:
							{
								CommandEndTxn.Builder SubBuilder = CommandEndTxn.NewBuilder();
								if (HasEndTxn())
								{
									SubBuilder.MergeFrom(GetEndTxn());
								}
								input.ReadMessage(SubBuilder, extensionRegistry);
								SetEndTxn(SubBuilder.BuildPartial());
								SubBuilder.Recycle();
								break;
							}
						case 458:
							{
								CommandEndTxnResponse.Builder SubBuilder = CommandEndTxnResponse.NewBuilder();
								if (HasEndTxnResponse())
								{
									SubBuilder.MergeFrom(GetEndTxnResponse());
								}
								input.ReadMessage(SubBuilder, extensionRegistry);
								SetEndTxnResponse(SubBuilder.BuildPartial());
								SubBuilder.Recycle();
								break;
							}
						case 466:
							{
								CommandEndTxnOnPartition.Builder SubBuilder = CommandEndTxnOnPartition.NewBuilder();
								if (HasEndTxnOnPartition())
								{
									SubBuilder.MergeFrom(GetEndTxnOnPartition());
								}
								input.ReadMessage(SubBuilder, extensionRegistry);
								SetEndTxnOnPartition(SubBuilder.BuildPartial());
								SubBuilder.Recycle();
								break;
							}
						case 474:
							{
								CommandEndTxnOnPartitionResponse.Builder SubBuilder = CommandEndTxnOnPartitionResponse.NewBuilder();
								if (HasEndTxnOnPartitionResponse())
								{
									SubBuilder.MergeFrom(GetEndTxnOnPartitionResponse());
								}
								input.ReadMessage(SubBuilder, extensionRegistry);
								SetEndTxnOnPartitionResponse(SubBuilder.BuildPartial());
								SubBuilder.Recycle();
								break;
							}
						case 482:
							{
								CommandEndTxnOnSubscription.Builder SubBuilder = CommandEndTxnOnSubscription.NewBuilder();
								if (HasEndTxnOnSubscription())
								{
									SubBuilder.MergeFrom(GetEndTxnOnSubscription());
								}
								input.ReadMessage(SubBuilder, extensionRegistry);
								SetEndTxnOnSubscription(SubBuilder.BuildPartial());
								SubBuilder.Recycle();
								break;
							}
						case 490:
							{
								CommandEndTxnOnSubscriptionResponse.Builder SubBuilder = CommandEndTxnOnSubscriptionResponse.NewBuilder();
								if (HasEndTxnOnSubscriptionResponse())
								{
									SubBuilder.MergeFrom(GetEndTxnOnSubscriptionResponse());
								}
								input.ReadMessage(SubBuilder, extensionRegistry);
								SetEndTxnOnSubscriptionResponse(SubBuilder.BuildPartial());
								SubBuilder.Recycle();
								break;
							}
					}
				}
			}

			public bool Initialized
			{
				get
				{
					if (!HasType())
					{

						return false;
					}
					if (HasConnect())
					{
						if (!GetConnect().IsInitialized())
						{

							return false;
						}
					}
					if (HasConnected())
					{
						if (!GetConnected().IsInitialized())
						{

							return false;
						}
					}
					if (HasSubscribe())
					{
						if (!GetSubscribe().IsInitialized())
						{

							return false;
						}
					}
					if (HasProducer())
					{
						if (!GetProducer().IsInitialized())
						{

							return false;
						}
					}
					if (HasSend())
					{
						if (!GetSend().IsInitialized())
						{

							return false;
						}
					}
					if (HasSendReceipt())
					{
						if (!GetSendReceipt().IsInitialized())
						{

							return false;
						}
					}
					if (HasSendError())
					{
						if (!GetSendError().IsInitialized())
						{

							return false;
						}
					}
					if (HasMessage())
					{
						if (!GetMessage().IsInitialized())
						{

							return false;
						}
					}
					if (HasAck())
					{
						if (!GetAck().IsInitialized())
						{

							return false;
						}
					}
					if (HasFlow())
					{
						if (!GetFlow().IsInitialized())
						{

							return false;
						}
					}
					if (HasUnsubscribe())
					{
						if (!GetUnsubscribe().IsInitialized())
						{

							return false;
						}
					}
					if (HasSuccess())
					{
						if (!GetSuccess().IsInitialized())
						{

							return false;
						}
					}
					if (HasError())
					{
						if (!GetError().IsInitialized())
						{

							return false;
						}
					}
					if (HasCloseProducer())
					{
						if (!GetCloseProducer().IsInitialized())
						{

							return false;
						}
					}
					if (HasCloseConsumer())
					{
						if (!GetCloseConsumer().IsInitialized())
						{

							return false;
						}
					}
					if (HasProducerSuccess())
					{
						if (!GetProducerSuccess().IsInitialized())
						{

							return false;
						}
					}
					if (HasRedeliverUnacknowledgedMessages())
					{
						if (!GetRedeliverUnacknowledgedMessages().IsInitialized())
						{

							return false;
						}
					}
					if (HasPartitionMetadata())
					{
						if (!GetPartitionMetadata().IsInitialized())
						{

							return false;
						}
					}
					if (HasPartitionMetadataResponse())
					{
						if (!GetPartitionMetadataResponse().IsInitialized())
						{

							return false;
						}
					}
					if (HasLookupTopic())
					{
						if (!GetLookupTopic().IsInitialized())
						{

							return false;
						}
					}
					if (HasLookupTopicResponse())
					{
						if (!GetLookupTopicResponse().IsInitialized())
						{

							return false;
						}
					}
					if (HasConsumerStats())
					{
						if (!GetConsumerStats().IsInitialized())
						{

							return false;
						}
					}
					if (HasConsumerStatsResponse())
					{
						if (!GetConsumerStatsResponse().IsInitialized())
						{

							return false;
						}
					}
					if (HasReachedEndOfTopic())
					{
						if (!GetReachedEndOfTopic().IsInitialized())
						{

							return false;
						}
					}
					if (HasSeek())
					{
						if (!GetSeek().IsInitialized())
						{

							return false;
						}
					}
					if (HasGetLastMessageId())
					{
						if (!GetGetLastMessageId().IsInitialized())
						{

							return false;
						}
					}
					if (HasGetLastMessageIdResponse())
					{
						if (!GetGetLastMessageIdResponse().IsInitialized())
						{

							return false;
						}
					}
					if (HasActiveConsumerChange())
					{
						if (!GetActiveConsumerChange().IsInitialized())
						{

							return false;
						}
					}
					if (HasGetTopicsOfNamespace())
					{
						if (!GetGetTopicsOfNamespace().IsInitialized())
						{

							return false;
						}
					}
					if (HasGetTopicsOfNamespaceResponse())
					{
						if (!GetGetTopicsOfNamespaceResponse().IsInitialized())
						{

							return false;
						}
					}
					if (HasGetSchema())
					{
						if (!GetGetSchema().IsInitialized())
						{

							return false;
						}
					}
					if (HasGetSchemaResponse())
					{
						if (!GetGetSchemaResponse().IsInitialized())
						{

							return false;
						}
					}
					if (HasAckResponse())
					{
						if (!GetAckResponse().IsInitialized())
						{

							return false;
						}
					}
					if (HasGetOrCreateSchema())
					{
						if (!GetGetOrCreateSchema().IsInitialized())
						{

							return false;
						}
					}
					if (HasGetOrCreateSchemaResponse())
					{
						if (!GetGetOrCreateSchemaResponse().IsInitialized())
						{

							return false;
						}
					}
					if (HasNewTxn())
					{
						if (!GetNewTxn().IsInitialized())
						{

							return false;
						}
					}
					if (HasNewTxnResponse())
					{
						if (!GetNewTxnResponse().IsInitialized())
						{

							return false;
						}
					}
					if (HasAddPartitionToTxn())
					{
						if (!GetAddPartitionToTxn().IsInitialized())
						{

							return false;
						}
					}
					if (HasAddPartitionToTxnResponse())
					{
						if (!GetAddPartitionToTxnResponse().IsInitialized())
						{

							return false;
						}
					}
					if (HasAddSubscriptionToTxn())
					{
						if (!GetAddSubscriptionToTxn().IsInitialized())
						{

							return false;
						}
					}
					if (HasAddSubscriptionToTxnResponse())
					{
						if (!GetAddSubscriptionToTxnResponse().IsInitialized())
						{

							return false;
						}
					}
					if (HasEndTxn())
					{
						if (!GetEndTxn().IsInitialized())
						{

							return false;
						}
					}
					if (HasEndTxnResponse())
					{
						if (!GetEndTxnResponse().IsInitialized())
						{

							return false;
						}
					}
					if (HasEndTxnOnPartition())
					{
						if (!GetEndTxnOnPartition().IsInitialized())
						{

							return false;
						}
					}
					if (HasEndTxnOnPartitionResponse())
					{
						if (!GetEndTxnOnPartitionResponse().IsInitialized())
						{

							return false;
						}
					}
					if (HasEndTxnOnSubscription())
					{
						if (!GetEndTxnOnSubscription().IsInitialized())
						{

							return false;
						}
					}
					if (HasEndTxnOnSubscriptionResponse())
					{
						if (!GetEndTxnOnSubscriptionResponse().IsInitialized())
						{

							return false;
						}
					}
					return true;
				}
			}

			public Builder SetType(Types.Type Value)
			{
				if (Value == null)
				{
					throw new NullReferenceException();
				}
				_type = Value;

				return this;
			}
			public Builder ClearType()
			{
				ClearType();

				return this;
			}
			public bool HasType()
			{
				return ((_bitField0 & 0x00000001) == 0x00000001);
			}
			internal CommandConnect _connect = CommandConnect.DefaultInstance;
			public bool HasConnect()
			{
				return (((int)_type & 0x00000002) == 0x00000002);
			}
			public CommandConnect GetConnect()
			{
				return _connect;
			}
			public Builder SetConnect(CommandConnect Value)
			{
				if (Value == null)
				{
					throw new NullReferenceException();
				}
				_connect = Value;

				_bitField0 |= 0x00000002;
				return this;
			}
			public Builder SetConnect(CommandConnect.Builder builder)
			{
				_connect = builder.Build();

				_bitField0 |= 0x00000002;
				return this;
			}
			public Builder MergeConnect(CommandConnect value)
			{
				if (((_bitField0 & 0x00000002) == 0x00000002) && _connect != CommandConnect.DefaultInstance)
				{
					_connect = CommandConnect.NewBuilder(_connect).MergeFrom(value).BuildPartial();
				}
				else
				{
					_connect = value;
				}

				_bitField0 |= 0x00000002;
				return this;
			}
			
			// optional .pulsar.proto.CommandConnected connected = 3;
			internal CommandConnected _connected = CommandConnected.DefaultInstance;
			public bool HasConnected()
			{
				return ((_bitField0 & 0x00000004) == 0x00000004);
			}
			public CommandConnected GetConnected()
			{
				return _connected;
			}
			public Builder GetConnected(CommandConnected Value)
			{
				if (Value == null)
				{
					throw new NullReferenceException();
				}
				_connected = Value;

				_bitField0 |= 0x00000004;
				return this;
			}
			public Builder SetConnected(CommandConnected.Builder BuilderForValue)
			{
				_connected = BuilderForValue.Build();
				_bitField0 |= 0x00000004;
				return this;
			}
			public Builder MergeConnected(CommandConnected Value)
			{
				if (((_bitField0 & 0x00000004) == 0x00000004) && _connected != CommandConnected.DefaultInstance)
				{
					_connected = CommandConnected.NewBuilder(_connected).MergeFrom(Value).BuildPartial();
				}
				else
				{
					_connected = Value;
				}

				_bitField0 |= 0x00000004;
				return this;
			}

			public Builder ClearConnect()
			{
				_connect = CommandConnect.DefaultInstance;

				_bitField0 = (_bitField0 & ~0x00000002);
				return this;
			}
			public Builder ClearConnected()
			{
				_connected = CommandConnected.DefaultInstance;

				_bitField0 = (_bitField0 & ~0x00000004);
				return this;
			}

			internal CommandSubscribe Subscribe_ = CommandSubscribe.DefaultInstance;
			public bool HasSubscribe()
			{
				return ((_bitField0 & 0x00000008) == 0x00000008);
			}
			public CommandSubscribe GetSubscribe()
			{
				return Subscribe_;
			}
			public Builder SetSubscribe(CommandSubscribe Value)
			{
				if (Value == null)
				{
					throw new NullReferenceException();
				}
				Subscribe_ = Value;

				_bitField0 |= 0x00000008;
				return this;
			}
			public Builder SetSubscribe(CommandSubscribe.Builder BuilderForValue)
			{
				Subscribe_ = BuilderForValue.Build();

				_bitField0 |= 0x00000008;
				return this;
			}
			public Builder MergeSubscribe(CommandSubscribe Value)
			{
				if (((_bitField0 & 0x00000008) == 0x00000008) && Subscribe_ != CommandSubscribe.DefaultInstance)
				{
					Subscribe_ = CommandSubscribe.NewBuilder(Subscribe_).MergeFrom(Value).BuildPartial();
				}
				else
				{
					Subscribe_ = Value;
				}

				_bitField0 |= 0x00000008;
				return this;
			}
			public Builder ClearSubscribe()
			{
				Subscribe_ = CommandSubscribe.DefaultInstance;

				_bitField0 = (_bitField0 & ~0x00000008);
				return this;
			}

			// optional .pulsar.proto.CommandProducer producer = 5;
			internal CommandProducer Producer_ = CommandProducer.DefaultInstance;
			public bool HasProducer()
			{
				return ((_bitField0 & 0x00000010) == 0x00000010);
			}
			public CommandProducer GetProducer()
			{
				return Producer_;
			}
			public Builder SetProducer(CommandProducer Value)
			{
				if (Value == null)
				{
					throw new NullReferenceException();
				}
				Producer_ = Value;

				_bitField0 |= 0x00000010;
				return this;
			}
			public Builder SetProducer(CommandProducer.Builder BuilderForValue)
			{
				Producer_ = BuilderForValue.Build();

				_bitField0 |= 0x00000010;
				return this;
			}
			public Builder MergeProducer(CommandProducer Value)
			{
				if (((_bitField0 & 0x00000010) == 0x00000010) && Producer_ != CommandProducer.DefaultInstance)
				{
					Producer_ = CommandProducer.NewBuilder(Producer_).MergeFrom(Value).BuildPartial();
				}
				else
				{
					Producer_ = Value;
				}

				_bitField0 |= 0x00000010;
				return this;
			}
			public Builder ClearProducer()
			{
				Producer_ = CommandProducer.DefaultInstance;

				_bitField0 = (_bitField0 & ~0x00000010);
				return this;
			}

			// optional .pulsar.proto.CommandSend send = 6;
			internal CommandSend Send_ = CommandSend.DefaultInstance;
			public bool HasSend()
			{
				return ((_bitField0 & 0x00000020) == 0x00000020);
			}
			public CommandSend GetSend()
			{
				return Send_;
			}
			public Builder SetSend(CommandSend Value)
			{
				if (Value == null)
				{
					throw new NullReferenceException();
				}
				Send_ = Value;

				_bitField0 |= 0x00000020;
				return this;
			}
			public Builder SetSend(CommandSend.Builder BuilderForValue)
			{
				Send_ = BuilderForValue.Build();

				_bitField0 |= 0x00000020;
				return this;
			}
			public Builder MergeSend(CommandSend Value)
			{
				if (((_bitField0 & 0x00000020) == 0x00000020) && Send_ != CommandSend.DefaultInstance)
				{
					Send_ = CommandSend.NewBuilder(Send_).MergeFrom(Value).BuildPartial();
				}
				else
				{
					Send_ = Value;
				}

				_bitField0 |= 0x00000020;
				return this;
			}
			public Builder ClearSend()
			{
				Send_ = CommandSend.DefaultInstance;

				_bitField0 = (_bitField0 & ~0x00000020);
				return this;
			}

			// optional .pulsar.proto.CommandSendReceipt send_receipt = 7;
			internal CommandSendReceipt SendReceipt_ = CommandSendReceipt.DefaultInstance;
			public bool HasSendReceipt()
			{
				return ((_bitField0 & 0x00000040) == 0x00000040);
			}
			public CommandSendReceipt GetSendReceipt()
			{
				return SendReceipt_;
			}
			public Builder SetSendReceipt(CommandSendReceipt Value)
			{
				if (Value == null)
				{
					throw new NullReferenceException();
				}
				SendReceipt_ = Value;

				_bitField0 |= 0x00000040;
				return this;
			}
			public Builder SetSendReceipt(CommandSendReceipt.Builder BuilderForValue)
			{
				SendReceipt_ = BuilderForValue.Build();

				_bitField0 |= 0x00000040;
				return this;
			}
			public Builder MergeSendReceipt(CommandSendReceipt Value)
			{
				if (((_bitField0 & 0x00000040) == 0x00000040) && SendReceipt_ != CommandSendReceipt.DefaultInstance)
				{
					SendReceipt_ = CommandSendReceipt.NewBuilder(SendReceipt_).MergeFrom(Value).BuildPartial();
				}
				else
				{
					SendReceipt_ = Value;
				}

				_bitField0 |= 0x00000040;
				return this;
			}
			public Builder ClearSendReceipt()
			{
				SendReceipt_ = CommandSendReceipt.DefaultInstance;

				_bitField0 = (_bitField0 & ~0x00000040);
				return this;
			}

			// optional .pulsar.proto.CommandSendError send_error = 8;
			internal CommandSendError SendError_ = CommandSendError.DefaultInstance;
			public bool HasSendError()
			{
				return ((_bitField0 & 0x00000080) == 0x00000080);
			}
			public CommandSendError GetSendError()
			{
				return SendError_;
			}
			public Builder SetSendError(CommandSendError Value)
			{
				if (Value == null)
				{
					throw new NullReferenceException();
				}
				SendError_ = Value;

				_bitField0 |= 0x00000080;
				return this;
			}
			public Builder SetSendError(CommandSendError.Builder BuilderForValue)
			{
				SendError_ = BuilderForValue.Build();

				_bitField0 |= 0x00000080;
				return this;
			}
			public Builder MergeSendError(CommandSendError Value)
			{
				if (((_bitField0 & 0x00000080) == 0x00000080) && SendError_ != CommandSendError.DefaultInstance)
				{
					SendError_ = CommandSendError.NewBuilder(SendError_).MergeFrom(Value).BuildPartial();
				}
				else
				{
					SendError_ = Value;
				}

				_bitField0 |= 0x00000080;
				return this;
			}
			public Builder ClearSendError()
			{
				SendError_ = CommandSendError.DefaultInstance;

				_bitField0 = (_bitField0 & ~0x00000080);
				return this;
			}

			// optional .pulsar.proto.CommandMessage message = 9;
			internal CommandMessage Message_ = CommandMessage.DefaultInstance;
			public bool HasMessage()
			{
				return ((_bitField0 & 0x00000100) == 0x00000100);
			}
			public CommandMessage GetMessage()
			{
				return Message_;
			}
			public Builder SetMessage(CommandMessage Value)
			{
				if (Value == null)
				{
					throw new NullReferenceException();
				}
				Message_ = Value;

				_bitField0 |= 0x00000100;
				return this;
			}
			public Builder SetMessage(CommandMessage.Builder BuilderForValue)
			{
				Message_ = BuilderForValue.Build();

				_bitField0 |= 0x00000100;
				return this;
			}
			public Builder MergeMessage(CommandMessage Value)
			{
				if (((_bitField0 & 0x00000100) == 0x00000100) && Message_ != CommandMessage.DefaultInstance)
				{
					Message_ = CommandMessage.NewBuilder(Message_).MergeFrom(Value).BuildPartial();
				}
				else
				{
					Message_ = Value;
				}

				_bitField0 |= 0x00000100;
				return this;
			}
			public Builder ClearMessage()
			{
				Message_ = CommandMessage.DefaultInstance;

				_bitField0 = (_bitField0 & ~0x00000100);
				return this;
			}

			// optional .pulsar.proto.CommandAck ack = 10;
			internal CommandAck Ack_ = CommandAck.DefaultInstance;
			public bool HasAck()
			{
				return ((_bitField0 & 0x00000200) == 0x00000200);
			}
			public CommandAck GetAck()
			{
				return Ack_;
			}
			public Builder SetAck(CommandAck Value)
			{
				if (Value == null)
				{
					throw new NullReferenceException();
				}
				Ack_ = Value;

				_bitField0 |= 0x00000200;
				return this;
			}
			public Builder SetAck(CommandAck.Builder BuilderForValue)
			{
				Ack_ = BuilderForValue.Build();

				_bitField0 |= 0x00000200;
				return this;
			}
			public Builder MergeAck(CommandAck Value)
			{
				if (((_bitField0 & 0x00000200) == 0x00000200) && Ack_ != CommandAck.DefaultInstance)
				{
					Ack_ = CommandAck.NewBuilder(Ack_).MergeFrom(Value).BuildPartial();
				}
				else
				{
					Ack_ = Value;
				}

				_bitField0 |= 0x00000200;
				return this;
			}
			public Builder ClearAck()
			{
				Ack_ = CommandAck.DefaultInstance;

				_bitField0 = (_bitField0 & ~0x00000200);
				return this;
			}

			// optional .pulsar.proto.CommandFlow flow = 11;
			internal CommandFlow Flow_ = CommandFlow.DefaultInstance;
			public bool HasFlow()
			{
				return ((_bitField0 & 0x00000400) == 0x00000400);
			}
			public CommandFlow GetFlow()
			{
				return Flow_;
			}
			public Builder SetFlow(CommandFlow Value)
			{
				if (Value == null)
				{
					throw new NullReferenceException();
				}
				Flow_ = Value;

				_bitField0 |= 0x00000400;
				return this;
			}
			public Builder SetFlow(CommandFlow.Builder BuilderForValue)
			{
				Flow_ = BuilderForValue.Build();

				_bitField0 |= 0x00000400;
				return this;
			}
			public Builder MergeFlow(CommandFlow Value)
			{
				if (((_bitField0 & 0x00000400) == 0x00000400) && Flow_ != CommandFlow.DefaultInstance)
				{
					Flow_ = CommandFlow.NewBuilder(Flow_).MergeFrom(Value).BuildPartial();
				}
				else
				{
					Flow_ = Value;
				}

				_bitField0 |= 0x00000400;
				return this;
			}
			public Builder ClearFlow()
			{
				Flow_ = CommandFlow.DefaultInstance;

				_bitField0 = (_bitField0 & ~0x00000400);
				return this;
			}

			// optional .pulsar.proto.CommandUnsubscribe unsubscribe = 12;
			internal CommandUnsubscribe Unsubscribe_ = CommandUnsubscribe.DefaultInstance;
			public bool HasUnsubscribe()
			{
				return ((_bitField0 & 0x00000800) == 0x00000800);
			}
			public CommandUnsubscribe GetUnsubscribe()
			{
				return Unsubscribe_;
			}
			public Builder SetUnsubscribe(CommandUnsubscribe Value)
			{
				if (Value == null)
				{
					throw new NullReferenceException();
				}
				Unsubscribe_ = Value;

				_bitField0 |= 0x00000800;
				return this;
			}
			public Builder SetUnsubscribe(CommandUnsubscribe.Builder BuilderForValue)
			{
				Unsubscribe_ = BuilderForValue.Build();

				_bitField0 |= 0x00000800;
				return this;
			}
			public Builder MergeUnsubscribe(CommandUnsubscribe Value)
			{
				if (((_bitField0 & 0x00000800) == 0x00000800) && Unsubscribe_ != CommandUnsubscribe.DefaultInstance)
				{
					Unsubscribe_ = CommandUnsubscribe.NewBuilder(Unsubscribe_).MergeFrom(Value).BuildPartial();
				}
				else
				{
					Unsubscribe_ = Value;
				}

				_bitField0 |= 0x00000800;
				return this;
			}
			public Builder ClearUnsubscribe()
			{
				Unsubscribe_ = CommandUnsubscribe.DefaultInstance;

				_bitField0 = (_bitField0 & ~0x00000800);
				return this;
			}

			// optional .pulsar.proto.CommandSuccess success = 13;
			internal CommandSuccess Success_ = CommandSuccess.DefaultInstance;
			public bool HasSuccess()
			{
				return ((_bitField0 & 0x00001000) == 0x00001000);
			}
			public CommandSuccess GetSuccess()
			{
				return Success_;
			}
			public Builder SetSuccess(CommandSuccess Value)
			{
				if (Value == null)
				{
					throw new NullReferenceException();
				}
				Success_ = Value;

				_bitField0 |= 0x00001000;
				return this;
			}
			public Builder SetSuccess(CommandSuccess.Builder BuilderForValue)
			{
				Success_ = BuilderForValue.Build();

				_bitField0 |= 0x00001000;
				return this;
			}
			public Builder MergeSuccess(CommandSuccess Value)
			{
				if (((_bitField0 & 0x00001000) == 0x00001000) && Success_ != CommandSuccess.DefaultInstance)
				{
					Success_ = CommandSuccess.NewBuilder(Success_).MergeFrom(Value).BuildPartial();
				}
				else
				{
					Success_ = Value;
				}

				_bitField0 |= 0x00001000;
				return this;
			}
			public Builder ClearSuccess()
			{
				Success_ = CommandSuccess.DefaultInstance;

				_bitField0 = (_bitField0 & ~0x00001000);
				return this;
			}

			// optional .pulsar.proto.CommandError error = 14;
			internal CommandError Error_ = CommandError.DefaultInstance;
			public bool HasError()
			{
				return ((_bitField0 & 0x00002000) == 0x00002000);
			}
			public CommandError GetError()
			{
				return Error_;
			}
			public Builder SetError(CommandError Value)
			{
				if (Value == null)
				{
					throw new NullReferenceException();
				}
				Error_ = Value;

				_bitField0 |= 0x00002000;
				return this;
			}
			public Builder SetError(CommandError.Builder BuilderForValue)
			{
				Error_ = BuilderForValue.Build();

				_bitField0 |= 0x00002000;
				return this;
			}
			public Builder MergeError(CommandError Value)
			{
				if (((_bitField0 & 0x00002000) == 0x00002000) && Error_ != CommandError.DefaultInstance)
				{
					Error_ = CommandError.NewBuilder(Error_).MergeFrom(Value).BuildPartial();
				}
				else
				{
					Error_ = Value;
				}

				_bitField0 |= 0x00002000;
				return this;
			}
			public Builder ClearError()
			{
				Error_ = CommandError.DefaultInstance;

				_bitField0 = (_bitField0 & ~0x00002000);
				return this;
			}

			// optional .pulsar.proto.CommandCloseProducer close_producer = 15;
			internal CommandCloseProducer CloseProducer_ = CommandCloseProducer.DefaultInstance;
			public bool HasCloseProducer()
			{
				return ((_bitField0 & 0x00004000) == 0x00004000);
			}
			public CommandCloseProducer GetCloseProducer()
			{
				return CloseProducer_;
			}
			public Builder SetCloseProducer(CommandCloseProducer Value)
			{
				if (Value == null)
				{
					throw new NullReferenceException();
				}
				CloseProducer_ = Value;

				_bitField0 |= 0x00004000;
				return this;
			}
			public Builder SetCloseProducer(CommandCloseProducer.Builder BuilderForValue)
			{
				CloseProducer_ = BuilderForValue.Build();

				_bitField0 |= 0x00004000;
				return this;
			}
			public Builder MergeCloseProducer(CommandCloseProducer Value)
			{
				if (((_bitField0 & 0x00004000) == 0x00004000) && CloseProducer_ != CommandCloseProducer.DefaultInstance)
				{
					CloseProducer_ = CommandCloseProducer.NewBuilder(CloseProducer_).MergeFrom(Value).BuildPartial();
				}
				else
				{
					CloseProducer_ = Value;
				}

				_bitField0 |= 0x00004000;
				return this;
			}
			public Builder ClearCloseProducer()
			{
				CloseProducer_ = CommandCloseProducer.DefaultInstance;

				_bitField0 = (_bitField0 & ~0x00004000);
				return this;
			}

			// optional .pulsar.proto.CommandCloseConsumer close_consumer = 16;
			internal CommandCloseConsumer CloseConsumer_ = CommandCloseConsumer.DefaultInstance;
			public bool HasCloseConsumer()
			{
				return ((_bitField0 & 0x00008000) == 0x00008000);
			}
			public CommandCloseConsumer GetCloseConsumer()
			{
				return CloseConsumer_;
			}
			public Builder SetCloseConsumer(CommandCloseConsumer Value)
			{
				if (Value == null)
				{
					throw new NullReferenceException();
				}
				CloseConsumer_ = Value;

				_bitField0 |= 0x00008000;
				return this;
			}
			public Builder SetCloseConsumer(CommandCloseConsumer.Builder BuilderForValue)
			{
				CloseConsumer_ = BuilderForValue.Build();

				_bitField0 |= 0x00008000;
				return this;
			}
			public Builder MergeCloseConsumer(CommandCloseConsumer Value)
			{
				if (((_bitField0 & 0x00008000) == 0x00008000) && CloseConsumer_ != CommandCloseConsumer.DefaultInstance)
				{
					CloseConsumer_ = CommandCloseConsumer.NewBuilder(CloseConsumer_).MergeFrom(Value).BuildPartial();
				}
				else
				{
					CloseConsumer_ = Value;
				}

				_bitField0 |= 0x00008000;
				return this;
			}
			public Builder ClearCloseConsumer()
			{
				CloseConsumer_ = CommandCloseConsumer.DefaultInstance;

				_bitField0 = (_bitField0 & ~0x00008000);
				return this;
			}

			// optional .pulsar.proto.CommandProducerSuccess producer_success = 17;
			internal CommandProducerSuccess ProducerSuccess_ = CommandProducerSuccess.DefaultInstance;
			public bool HasProducerSuccess()
			{
				return ((_bitField0 & 0x00010000) == 0x00010000);
			}
			public CommandProducerSuccess GetProducerSuccess()
			{
				return ProducerSuccess_;
			}
			public Builder SetProducerSuccess(CommandProducerSuccess Value)
			{
				if (Value == null)
				{
					throw new NullReferenceException();
				}
				ProducerSuccess_ = Value;

				_bitField0 |= 0x00010000;
				return this;
			}
			public Builder SetProducerSuccess(CommandProducerSuccess.Builder BuilderForValue)
			{
				ProducerSuccess_ = BuilderForValue.Build();

				_bitField0 |= 0x00010000;
				return this;
			}
			public Builder MergeProducerSuccess(CommandProducerSuccess Value)
			{
				if (((_bitField0 & 0x00010000) == 0x00010000) && ProducerSuccess_ != CommandProducerSuccess.DefaultInstance)
				{
					ProducerSuccess_ = CommandProducerSuccess.NewBuilder(ProducerSuccess_).MergeFrom(Value).BuildPartial();
				}
				else
				{
					ProducerSuccess_ = Value;
				}

				_bitField0 |= 0x00010000;
				return this;
			}
			public Builder ClearProducerSuccess()
			{
				ProducerSuccess_ = CommandProducerSuccess.DefaultInstance;

				_bitField0 = (_bitField0 & ~0x00010000);
				return this;
			}

			// optional .pulsar.proto.CommandPing ping = 18;
			internal CommandPing Ping_ = CommandPing.DefaultInstance;
			public bool HasPing()
			{
				return ((_bitField0 & 0x00020000) == 0x00020000);
			}
			public CommandPing GetPing()
			{
				return Ping_;
			}
			public Builder SetPing(CommandPing Value)
			{
				if (Value == null)
				{
					throw new NullReferenceException();
				}
				Ping_ = Value;

				_bitField0 |= 0x00020000;
				return this;
			}
			public Builder SetPing(CommandPing.Builder BuilderForValue)
			{
				Ping_ = BuilderForValue.Build();

				_bitField0 |= 0x00020000;
				return this;
			}
			public Builder MergePing(CommandPing Value)
			{
				if (((_bitField0 & 0x00020000) == 0x00020000) && Ping_ != CommandPing.DefaultInstance)
				{
					Ping_ = CommandPing.NewBuilder(Ping_).MergeFrom(Value).BuildPartial();
				}
				else
				{
					Ping_ = Value;
				}

				_bitField0 |= 0x00020000;
				return this;
			}
			public Builder ClearPing()
			{
				Ping_ = CommandPing.DefaultInstance;

				_bitField0 = (_bitField0 & ~0x00020000);
				return this;
			}

			// optional .pulsar.proto.CommandPong pong = 19;
			internal CommandPong Pong_ = CommandPong.DefaultInstance;
			public bool HasPong()
			{
				return ((_bitField0 & 0x00040000) == 0x00040000);
			}
			public CommandPong GetPong()
			{
				return Pong_;
			}
			public Builder SetPong(CommandPong Value)
			{
				if (Value == null)
				{
					throw new NullReferenceException();
				}
				Pong_ = Value;

				_bitField0 |= 0x00040000;
				return this;
			}
			public Builder SetPong(CommandPong.Builder BuilderForValue)
			{
				Pong_ = BuilderForValue.Build();

				_bitField0 |= 0x00040000;
				return this;
			}
			public Builder MergePong(CommandPong Value)
			{
				if (((_bitField0 & 0x00040000) == 0x00040000) && Pong_ != CommandPong.DefaultInstance)
				{
					Pong_ = CommandPong.NewBuilder(Pong_).MergeFrom(Value).BuildPartial();
				}
				else
				{
					Pong_ = Value;
				}

				_bitField0 |= 0x00040000;
				return this;
			}
			public Builder ClearPong()
			{
				Pong_ = CommandPong.DefaultInstance;

				_bitField0 = (_bitField0 & ~0x00040000);
				return this;
			}

			// optional .pulsar.proto.CommandRedeliverUnacknowledgedMessages redeliverUnacknowledgedMessages = 20;
			internal CommandRedeliverUnacknowledgedMessages RedeliverUnacknowledgedMessages_ = CommandRedeliverUnacknowledgedMessages.DefaultInstance;
			public bool HasRedeliverUnacknowledgedMessages()
			{
				return ((_bitField0 & 0x00080000) == 0x00080000);
			}
			public CommandRedeliverUnacknowledgedMessages GetRedeliverUnacknowledgedMessages()
			{
				return RedeliverUnacknowledgedMessages_;
			}
			public Builder SetRedeliverUnacknowledgedMessages(CommandRedeliverUnacknowledgedMessages Value)
			{
				if (Value == null)
				{
					throw new NullReferenceException();
				}
				RedeliverUnacknowledgedMessages_ = Value;

				_bitField0 |= 0x00080000;
				return this;
			}
			public Builder SetRedeliverUnacknowledgedMessages(CommandRedeliverUnacknowledgedMessages.Builder BuilderForValue)
			{
				RedeliverUnacknowledgedMessages_ = BuilderForValue.Build();

				_bitField0 |= 0x00080000;
				return this;
			}
			public Builder MergeRedeliverUnacknowledgedMessages(CommandRedeliverUnacknowledgedMessages Value)
			{
				if (((_bitField0 & 0x00080000) == 0x00080000) && RedeliverUnacknowledgedMessages_ != CommandRedeliverUnacknowledgedMessages.DefaultInstance)
				{
					RedeliverUnacknowledgedMessages_ = CommandRedeliverUnacknowledgedMessages.NewBuilder(RedeliverUnacknowledgedMessages_).MergeFrom(Value).BuildPartial();
				}
				else
				{
					RedeliverUnacknowledgedMessages_ = Value;
				}

				_bitField0 |= 0x00080000;
				return this;
			}
			public Builder ClearRedeliverUnacknowledgedMessages()
			{
				RedeliverUnacknowledgedMessages_ = CommandRedeliverUnacknowledgedMessages.DefaultInstance;

				_bitField0 = (_bitField0 & ~0x00080000);
				return this;
			}

			// optional .pulsar.proto.CommandPartitionedTopicMetadata partitionMetadata = 21;
			internal CommandPartitionedTopicMetadata PartitionMetadata_ = CommandPartitionedTopicMetadata.DefaultInstance;
			public bool HasPartitionMetadata()
			{
				return ((_bitField0 & 0x00100000) == 0x00100000);
			}
			public CommandPartitionedTopicMetadata GetPartitionMetadata()
			{
				return PartitionMetadata_;
			}
			public Builder SetPartitionMetadata(CommandPartitionedTopicMetadata Value)
			{
				if (Value == null)
				{
					throw new NullReferenceException();
				}
				PartitionMetadata_ = Value;

				_bitField0 |= 0x00100000;
				return this;
			}
			public Builder SetPartitionMetadata(CommandPartitionedTopicMetadata.Builder BuilderForValue)
			{
				PartitionMetadata_ = BuilderForValue.Build();

				_bitField0 |= 0x00100000;
				return this;
			}
			public Builder MergePartitionMetadata(CommandPartitionedTopicMetadata Value)
			{
				if (((_bitField0 & 0x00100000) == 0x00100000) && PartitionMetadata_ != CommandPartitionedTopicMetadata.DefaultInstance)
				{
					PartitionMetadata_ = CommandPartitionedTopicMetadata.NewBuilder(PartitionMetadata_).MergeFrom(Value).BuildPartial();
				}
				else
				{
					PartitionMetadata_ = Value;
				}

				_bitField0 |= 0x00100000;
				return this;
			}
			public Builder ClearPartitionMetadata()
			{
				PartitionMetadata_ = CommandPartitionedTopicMetadata.DefaultInstance;

				_bitField0 = (_bitField0 & ~0x00100000);
				return this;
			}

			// optional .pulsar.proto.CommandPartitionedTopicMetadataResponse partitionMetadataResponse = 22;
			internal CommandPartitionedTopicMetadataResponse PartitionMetadataResponse_ = CommandPartitionedTopicMetadataResponse.DefaultInstance;
			public bool HasPartitionMetadataResponse()
			{
				return ((_bitField0 & 0x00200000) == 0x00200000);
			}
			public CommandPartitionedTopicMetadataResponse GetPartitionMetadataResponse()
			{
				return PartitionMetadataResponse_;
			}
			public Builder SetPartitionMetadataResponse(CommandPartitionedTopicMetadataResponse Value)
			{
				if (Value == null)
				{
					throw new NullReferenceException();
				}
				PartitionMetadataResponse_ = Value;

				_bitField0 |= 0x00200000;
				return this;
			}
			public Builder SetPartitionMetadataResponse(CommandPartitionedTopicMetadataResponse.Builder BuilderForValue)
			{
				PartitionMetadataResponse_ = BuilderForValue.Build();

				_bitField0 |= 0x00200000;
				return this;
			}
			public Builder MergePartitionMetadataResponse(CommandPartitionedTopicMetadataResponse Value)
			{
				if (((_bitField0 & 0x00200000) == 0x00200000) && PartitionMetadataResponse_ != CommandPartitionedTopicMetadataResponse.DefaultInstance)
				{
					PartitionMetadataResponse_ = CommandPartitionedTopicMetadataResponse.NewBuilder(PartitionMetadataResponse_).MergeFrom(Value).BuildPartial();
				}
				else
				{
					PartitionMetadataResponse_ = Value;
				}

				_bitField0 |= 0x00200000;
				return this;
			}
			public Builder ClearPartitionMetadataResponse()
			{
				PartitionMetadataResponse_ = CommandPartitionedTopicMetadataResponse.DefaultInstance;

				_bitField0 = (_bitField0 & ~0x00200000);
				return this;
			}

			// optional .pulsar.proto.CommandLookupTopic lookupTopic = 23;
			internal CommandLookupTopic LookupTopic_ = CommandLookupTopic.DefaultInstance;
			public bool HasLookupTopic()
			{
				return ((_bitField0 & 0x00400000) == 0x00400000);
			}
			public CommandLookupTopic GetLookupTopic()
			{
				return LookupTopic_;
			}
			public Builder SetLookupTopic(CommandLookupTopic Value)
			{
				if (Value == null)
				{
					throw new NullReferenceException();
				}
				LookupTopic_ = Value;

				_bitField0 |= 0x00400000;
				return this;
			}
			public Builder SetLookupTopic(CommandLookupTopic.Builder BuilderForValue)
			{
				LookupTopic_ = BuilderForValue.Build();

				_bitField0 |= 0x00400000;
				return this;
			}
			public Builder MergeLookupTopic(CommandLookupTopic Value)
			{
				if (((_bitField0 & 0x00400000) == 0x00400000) && LookupTopic_ != CommandLookupTopic.DefaultInstance)
				{
					LookupTopic_ = CommandLookupTopic.NewBuilder(LookupTopic_).MergeFrom(Value).BuildPartial();
				}
				else
				{
					LookupTopic_ = Value;
				}

				_bitField0 |= 0x00400000;
				return this;
			}
			public Builder ClearLookupTopic()
			{
				LookupTopic_ = CommandLookupTopic.DefaultInstance;

				_bitField0 = (_bitField0 & ~0x00400000);
				return this;
			}

			// optional .pulsar.proto.CommandLookupTopicResponse lookupTopicResponse = 24;
			internal CommandLookupTopicResponse LookupTopicResponse_ = CommandLookupTopicResponse.DefaultInstance;
			public bool HasLookupTopicResponse()
			{
				return ((_bitField0 & 0x00800000) == 0x00800000);
			}
			public CommandLookupTopicResponse GetLookupTopicResponse()
			{
				return LookupTopicResponse_;
			}
			public Builder SetLookupTopicResponse(CommandLookupTopicResponse Value)
			{
				if (Value == null)
				{
					throw new NullReferenceException();
				}
				LookupTopicResponse_ = Value;

				_bitField0 |= 0x00800000;
				return this;
			}
			public Builder SetLookupTopicResponse(CommandLookupTopicResponse.Builder BuilderForValue)
			{
				LookupTopicResponse_ = BuilderForValue.Build();

				_bitField0 |= 0x00800000;
				return this;
			}
			public Builder MergeLookupTopicResponse(CommandLookupTopicResponse Value)
			{
				if (((_bitField0 & 0x00800000) == 0x00800000) && LookupTopicResponse_ != CommandLookupTopicResponse.DefaultInstance)
				{
					LookupTopicResponse_ = CommandLookupTopicResponse.NewBuilder(LookupTopicResponse_).MergeFrom(Value).BuildPartial();
				}
				else
				{
					LookupTopicResponse_ = Value;
				}

				_bitField0 |= 0x00800000;
				return this;
			}
			public Builder ClearLookupTopicResponse()
			{
				LookupTopicResponse_ = CommandLookupTopicResponse.DefaultInstance;

				_bitField0 = (_bitField0 & ~0x00800000);
				return this;
			}

			// optional .pulsar.proto.CommandConsumerStats consumerStats = 25;
			internal CommandConsumerStats ConsumerStats_ = CommandConsumerStats.DefaultInstance;
			public bool HasConsumerStats()
			{
				return ((_bitField0 & 0x01000000) == 0x01000000);
			}
			public CommandConsumerStats GetConsumerStats()
			{
				return ConsumerStats_;
			}
			public Builder SetConsumerStats(CommandConsumerStats Value)
			{
				if (Value == null)
				{
					throw new NullReferenceException();
				}
				ConsumerStats_ = Value;

				_bitField0 |= 0x01000000;
				return this;
			}
			public Builder SetConsumerStats(CommandConsumerStats.Builder BuilderForValue)
			{
				ConsumerStats_ = BuilderForValue.Build();

				_bitField0 |= 0x01000000;
				return this;
			}
			public Builder MergeConsumerStats(CommandConsumerStats Value)
			{
				if (((_bitField0 & 0x01000000) == 0x01000000) && ConsumerStats_ != CommandConsumerStats.DefaultInstance)
				{
					ConsumerStats_ = CommandConsumerStats.NewBuilder(ConsumerStats_).MergeFrom(Value).BuildPartial();
				}
				else
				{
					ConsumerStats_ = Value;
				}

				_bitField0 |= 0x01000000;
				return this;
			}
			public Builder ClearConsumerStats()
			{
				ConsumerStats_ = CommandConsumerStats.DefaultInstance;

				_bitField0 = (_bitField0 & ~0x01000000);
				return this;
			}

			// optional .pulsar.proto.CommandConsumerStatsResponse consumerStatsResponse = 26;
			internal CommandConsumerStatsResponse ConsumerStatsResponse_ = CommandConsumerStatsResponse.DefaultInstance;
			public bool HasConsumerStatsResponse()
			{
				return ((_bitField0 & 0x02000000) == 0x02000000);
			}
			public CommandConsumerStatsResponse GetConsumerStatsResponse()
			{
				return ConsumerStatsResponse_;
			}
			public Builder SetConsumerStatsResponse(CommandConsumerStatsResponse Value)
			{
				if (Value == null)
				{
					throw new NullReferenceException();
				}
				ConsumerStatsResponse_ = Value;

				_bitField0 |= 0x02000000;
				return this;
			}
			public Builder SetConsumerStatsResponse(CommandConsumerStatsResponse.Builder BuilderForValue)
			{
				ConsumerStatsResponse_ = BuilderForValue.Build();

				_bitField0 |= 0x02000000;
				return this;
			}
			public Builder MergeConsumerStatsResponse(CommandConsumerStatsResponse Value)
			{
				if (((_bitField0 & 0x02000000) == 0x02000000) && ConsumerStatsResponse_ != CommandConsumerStatsResponse.DefaultInstance)
				{
					ConsumerStatsResponse_ = CommandConsumerStatsResponse.NewBuilder(ConsumerStatsResponse_).MergeFrom(Value).BuildPartial();
				}
				else
				{
					ConsumerStatsResponse_ = Value;
				}

				_bitField0 |= 0x02000000;
				return this;
			}
			public Builder ClearConsumerStatsResponse()
			{
				ConsumerStatsResponse_ = CommandConsumerStatsResponse.DefaultInstance;

				_bitField0 = (_bitField0 & ~0x02000000);
				return this;
			}

			// optional .pulsar.proto.CommandReachedEndOfTopic reachedEndOfTopic = 27;
			internal CommandReachedEndOfTopic ReachedEndOfTopic_ = CommandReachedEndOfTopic.DefaultInstance;
			public bool HasReachedEndOfTopic()
			{
				return ((_bitField0 & 0x04000000) == 0x04000000);
			}
			public CommandReachedEndOfTopic GetReachedEndOfTopic()
			{
				return ReachedEndOfTopic_;
			}
			public Builder SetReachedEndOfTopic(CommandReachedEndOfTopic Value)
			{
				if (Value == null)
				{
					throw new NullReferenceException();
				}
				ReachedEndOfTopic_ = Value;

				_bitField0 |= 0x04000000;
				return this;
			}
			public Builder SetReachedEndOfTopic(CommandReachedEndOfTopic.Builder BuilderForValue)
			{
				ReachedEndOfTopic_ = BuilderForValue.Build();

				_bitField0 |= 0x04000000;
				return this;
			}
			public Builder MergeReachedEndOfTopic(CommandReachedEndOfTopic Value)
			{
				if (((_bitField0 & 0x04000000) == 0x04000000) && ReachedEndOfTopic_ != CommandReachedEndOfTopic.DefaultInstance)
				{
					ReachedEndOfTopic_ = CommandReachedEndOfTopic.NewBuilder(ReachedEndOfTopic_).MergeFrom(Value).BuildPartial();
				}
				else
				{
					ReachedEndOfTopic_ = Value;
				}

				_bitField0 |= 0x04000000;
				return this;
			}
			public Builder ClearReachedEndOfTopic()
			{
				ReachedEndOfTopic_ = CommandReachedEndOfTopic.DefaultInstance;

				_bitField0 = (_bitField0 & ~0x04000000);
				return this;
			}

			// optional .pulsar.proto.CommandSeek seek = 28;
			internal CommandSeek Seek_ = CommandSeek.DefaultInstance;
			public bool HasSeek()
			{
				return ((_bitField0 & 0x08000000) == 0x08000000);
			}
			public CommandSeek GetSeek()
			{
				return Seek_;
			}
			public Builder SetSeek(CommandSeek Value)
			{
				if (Value == null)
				{
					throw new NullReferenceException();
				}
				Seek_ = Value;

				_bitField0 |= 0x08000000;
				return this;
			}
			public Builder SetSeek(CommandSeek.Builder BuilderForValue)
			{
				Seek_ = BuilderForValue.Build();

				_bitField0 |= 0x08000000;
				return this;
			}
			public Builder MergeSeek(CommandSeek Value)
			{
				if (((_bitField0 & 0x08000000) == 0x08000000) && Seek_ != CommandSeek.DefaultInstance)
				{
					Seek_ = CommandSeek.NewBuilder(Seek_).MergeFrom(Value).BuildPartial();
				}
				else
				{
					Seek_ = Value;
				}

				_bitField0 |= 0x08000000;
				return this;
			}
			public Builder ClearSeek()
			{
				Seek_ = CommandSeek.DefaultInstance;

				_bitField0 = (_bitField0 & ~0x08000000);
				return this;
			}

			// optional .pulsar.proto.CommandGetLastMessageId GetLastMessageId = 29;
			internal CommandGetLastMessageId GetLastMessageId_ = CommandGetLastMessageId.DefaultInstance;
			public bool HasGetLastMessageId()
			{
				return ((_bitField0 & 0x10000000) == 0x10000000);
			}
			public CommandGetLastMessageId GetGetLastMessageId()
			{
				return GetLastMessageId_;
			}
			public Builder SetGetLastMessageId(CommandGetLastMessageId Value)
			{
				if (Value == null)
				{
					throw new NullReferenceException();
				}
				GetLastMessageId_ = Value;

				_bitField0 |= 0x10000000;
				return this;
			}
			public Builder SetGetLastMessageId(CommandGetLastMessageId.Builder BuilderForValue)
			{
				GetLastMessageId_ = BuilderForValue.Build();

				_bitField0 |= 0x10000000;
				return this;
			}
			public Builder MergeGetLastMessageId(CommandGetLastMessageId Value)
			{
				if (((_bitField0 & 0x10000000) == 0x10000000) && GetLastMessageId_ != CommandGetLastMessageId.DefaultInstance)
				{
					GetLastMessageId_ = CommandGetLastMessageId.NewBuilder(GetLastMessageId_).MergeFrom(Value).BuildPartial();
				}
				else
				{
					GetLastMessageId_ = Value;
				}

				_bitField0 |= 0x10000000;
				return this;
			}
			public Builder ClearGetLastMessageId()
			{
				GetLastMessageId_ = CommandGetLastMessageId.DefaultInstance;

				_bitField0 = (_bitField0 & ~0x10000000);
				return this;
			}

			// optional .pulsar.proto.CommandGetLastMessageIdResponse GetLastMessageIdResponse = 30;
			internal CommandGetLastMessageIdResponse GetLastMessageIdResponse_ = CommandGetLastMessageIdResponse.DefaultInstance;
			public bool HasGetLastMessageIdResponse()
			{
				return ((_bitField0 & 0x20000000) == 0x20000000);
			}
			public CommandGetLastMessageIdResponse GetGetLastMessageIdResponse()
			{
				return GetLastMessageIdResponse_;
			}
			public Builder SetGetLastMessageIdResponse(CommandGetLastMessageIdResponse Value)
			{
				if (Value == null)
				{
					throw new NullReferenceException();
				}
				GetLastMessageIdResponse_ = Value;

				_bitField0 |= 0x20000000;
				return this;
			}
			public Builder SetGetLastMessageIdResponse(CommandGetLastMessageIdResponse.Builder BuilderForValue)
			{
				GetLastMessageIdResponse_ = BuilderForValue.Build();

				_bitField0 |= 0x20000000;
				return this;
			}
			public Builder MergeGetLastMessageIdResponse(CommandGetLastMessageIdResponse Value)
			{
				if (((_bitField0 & 0x20000000) == 0x20000000) && GetLastMessageIdResponse_ != CommandGetLastMessageIdResponse.DefaultInstance)
				{
					GetLastMessageIdResponse_ = CommandGetLastMessageIdResponse.NewBuilder(GetLastMessageIdResponse_).MergeFrom(Value).BuildPartial();
				}
				else
				{
					GetLastMessageIdResponse_ = Value;
				}

				_bitField0 |= 0x20000000;
				return this;
			}
			public Builder ClearGetLastMessageIdResponse()
			{
				GetLastMessageIdResponse_ = CommandGetLastMessageIdResponse.DefaultInstance;

				_bitField0 = (_bitField0 & ~0x20000000);
				return this;
			}

			// optional .pulsar.proto.CommandActiveConsumerChange active_consumer_change = 31;
			internal CommandActiveConsumerChange ActiveConsumerChange_ = CommandActiveConsumerChange.DefaultInstance;
			public bool HasActiveConsumerChange()
			{
				return ((_bitField0 & 0x40000000) == 0x40000000);
			}
			public CommandActiveConsumerChange GetActiveConsumerChange()
			{
				return ActiveConsumerChange_;
			}
			public Builder SetActiveConsumerChange(CommandActiveConsumerChange Value)
			{
				if (Value == null)
				{
					throw new NullReferenceException();
				}
				ActiveConsumerChange_ = Value;

				_bitField0 |= 0x40000000;
				return this;
			}
			public Builder SetActiveConsumerChange(CommandActiveConsumerChange.Builder BuilderForValue)
			{
				ActiveConsumerChange_ = BuilderForValue.Build();

				_bitField0 |= 0x40000000;
				return this;
			}
			public Builder MergeActiveConsumerChange(CommandActiveConsumerChange Value)
			{
				if (((_bitField0 & 0x40000000) == 0x40000000) && ActiveConsumerChange_ != CommandActiveConsumerChange.DefaultInstance)
				{
					ActiveConsumerChange_ = CommandActiveConsumerChange.NewBuilder(ActiveConsumerChange_).MergeFrom(Value).BuildPartial();
				}
				else
				{
					ActiveConsumerChange_ = Value;
				}

				_bitField0 |= 0x40000000;
				return this;
			}
			public Builder ClearActiveConsumerChange()
			{
				ActiveConsumerChange_ = CommandActiveConsumerChange.DefaultInstance;

				_bitField0 = (_bitField0 & ~0x40000000);
				return this;
			}

			// optional .pulsar.proto.CommandGetTopicsOfNamespace GetTopicsOfNamespace = 32;
			internal CommandGetTopicsOfNamespace GetTopicsOfNamespace_ = CommandGetTopicsOfNamespace.DefaultInstance;
			public bool HasGetTopicsOfNamespace()
			{
				return ((_bitField0 & 0x80000000) == 0x80000000);
			}
			public CommandGetTopicsOfNamespace GetGetTopicsOfNamespace()
			{
				return GetTopicsOfNamespace_;
			}
			public Builder SetGetTopicsOfNamespace(CommandGetTopicsOfNamespace Value)
			{
				if (Value == null)
				{
					throw new NullReferenceException();
				}
				GetTopicsOfNamespace_ = Value;

				_bitField0 |= unchecked((int)0x80000000);
				return this;
			}
			public Builder SetGetTopicsOfNamespace(CommandGetTopicsOfNamespace.Builder BuilderForValue)
			{
				GetTopicsOfNamespace_ = BuilderForValue.Build();

				_bitField0 |= unchecked((int)0x80000000);
				return this;
			}
			public Builder MergeGetTopicsOfNamespace(CommandGetTopicsOfNamespace Value)
			{
				if (((_bitField0 & 0x80000000) == 0x80000000) && GetTopicsOfNamespace_ != CommandGetTopicsOfNamespace.DefaultInstance)
				{
					GetTopicsOfNamespace_ = CommandGetTopicsOfNamespace.NewBuilder(GetTopicsOfNamespace_).MergeFrom(Value).BuildPartial();
				}
				else
				{
					GetTopicsOfNamespace_ = Value;
				}

				_bitField0 |= unchecked((int)0x80000000);
				return this;
			}
			public Builder ClearGetTopicsOfNamespace()
			{
				GetTopicsOfNamespace_ = CommandGetTopicsOfNamespace.DefaultInstance;

				_bitField0 = (_bitField0 & ~unchecked((int)0x80000000));
				return this;
			}

			// optional .pulsar.proto.CommandGetTopicsOfNamespaceResponse GetTopicsOfNamespaceResponse = 33;
			internal CommandGetTopicsOfNamespaceResponse GetTopicsOfNamespaceResponse_ = CommandGetTopicsOfNamespaceResponse.DefaultInstance;
			public bool HasGetTopicsOfNamespaceResponse()
			{
				return ((_bitField0 & 0x00000001) == 0x00000001);
			}
			public CommandGetTopicsOfNamespaceResponse GetGetTopicsOfNamespaceResponse()
			{
				return GetTopicsOfNamespaceResponse_;
			}
			public Builder SetGetTopicsOfNamespaceResponse(CommandGetTopicsOfNamespaceResponse Value)
			{
				if (Value == null)
				{
					throw new NullReferenceException();
				}
				GetTopicsOfNamespaceResponse_ = Value;

				_bitField0 |= 0x00000001;
				return this;
			}
			public Builder SetGetTopicsOfNamespaceResponse(CommandGetTopicsOfNamespaceResponse.Builder BuilderForValue)
			{
				GetTopicsOfNamespaceResponse_ = BuilderForValue.Build();

				_bitField0 |= 0x00000001;
				return this;
			}
			public Builder MergeGetTopicsOfNamespaceResponse(CommandGetTopicsOfNamespaceResponse Value)
			{
				if (((_bitField0 & 0x00000001) == 0x00000001) && GetTopicsOfNamespaceResponse_ != CommandGetTopicsOfNamespaceResponse.DefaultInstance)
				{
					GetTopicsOfNamespaceResponse_ = CommandGetTopicsOfNamespaceResponse.NewBuilder(GetTopicsOfNamespaceResponse_).MergeFrom(Value).BuildPartial();
				}
				else
				{
					GetTopicsOfNamespaceResponse_ = Value;
				}

				_bitField0 |= 0x00000001;
				return this;
			}
			public Builder ClearGetTopicsOfNamespaceResponse()
			{
				GetTopicsOfNamespaceResponse_ = CommandGetTopicsOfNamespaceResponse.DefaultInstance;

				_bitField0 = (_bitField0 & ~0x00000001);
				return this;
			}

			// optional .pulsar.proto.CommandGetSchema GetSchema = 34;
			internal CommandGetSchema GetSchema_ = CommandGetSchema.DefaultInstance;
			public bool HasGetSchema()
			{
				return ((_bitField0 & 0x00000002) == 0x00000002);
			}
			public CommandGetSchema GetGetSchema()
			{
				return GetSchema_;
			}
			public Builder SetGetSchema(CommandGetSchema Value)
			{
				if (Value == null)
				{
					throw new NullReferenceException();
				}
				GetSchema_ = Value;

				_bitField0 |= 0x00000002;
				return this;
			}
			public Builder SetGetSchema(CommandGetSchema.Builder BuilderForValue)
			{
				GetSchema_ = BuilderForValue.Build();

				_bitField0 |= 0x00000002;
				return this;
			}
			public Builder MergeGetSchema(CommandGetSchema Value)
			{
				if (((_bitField0 & 0x00000002) == 0x00000002) && GetSchema_ != CommandGetSchema.DefaultInstance)
				{
					GetSchema_ = CommandGetSchema.NewBuilder(GetSchema_).MergeFrom(Value).BuildPartial();
				}
				else
				{
					GetSchema_ = Value;
				}

				_bitField0 |= 0x00000002;
				return this;
			}
			public Builder ClearGetSchema()
			{
				GetSchema_ = CommandGetSchema.DefaultInstance;

				_bitField0 = (_bitField0 & ~0x00000002);
				return this;
			}

			// optional .pulsar.proto.CommandGetSchemaResponse GetSchemaResponse = 35;
			internal CommandGetSchemaResponse GetSchemaResponse_ = CommandGetSchemaResponse.DefaultInstance;
			public bool HasGetSchemaResponse()
			{
				return ((_bitField0 & 0x00000004) == 0x00000004);
			}
			public CommandGetSchemaResponse GetGetSchemaResponse()
			{
				return GetSchemaResponse_;
			}
			public Builder SetGetSchemaResponse(CommandGetSchemaResponse Value)
			{
				if (Value == null)
				{
					throw new NullReferenceException();
				}
				GetSchemaResponse_ = Value;

				_bitField0 |= 0x00000004;
				return this;
			}
			public Builder SetGetSchemaResponse(CommandGetSchemaResponse.Builder BuilderForValue)
			{
				GetSchemaResponse_ = BuilderForValue.Build();

				_bitField0 |= 0x00000004;
				return this;
			}
			public Builder MergeGetSchemaResponse(CommandGetSchemaResponse Value)
			{
				if (((_bitField0 & 0x00000004) == 0x00000004) && GetSchemaResponse_ != CommandGetSchemaResponse.DefaultInstance)
				{
					GetSchemaResponse_ = CommandGetSchemaResponse.NewBuilder(GetSchemaResponse_).MergeFrom(Value).BuildPartial();
				}
				else
				{
					GetSchemaResponse_ = Value;
				}

				_bitField0 |= 0x00000004;
				return this;
			}
			public Builder ClearGetSchemaResponse()
			{
				GetSchemaResponse_ = CommandGetSchemaResponse.DefaultInstance;

				_bitField0 = (_bitField0 & ~0x00000004);
				return this;
			}

			// optional .pulsar.proto.CommandAuthChallenge authChallenge = 36;
			internal CommandAuthChallenge AuthChallenge_ = CommandAuthChallenge.DefaultInstance;
			public bool HasAuthChallenge()
			{
				return ((_bitField0 & 0x00000008) == 0x00000008);
			}
			public CommandAuthChallenge GetAuthChallenge()
			{
				return AuthChallenge_;
			}
			public Builder SetAuthChallenge(CommandAuthChallenge Value)
			{
				if (Value == null)
				{
					throw new NullReferenceException();
				}
				AuthChallenge_ = Value;

				_bitField0 |= 0x00000008;
				return this;
			}
			public Builder SetAuthChallenge(CommandAuthChallenge.Builder BuilderForValue)
			{
				AuthChallenge_ = BuilderForValue.Build();

				_bitField0 |= 0x00000008;
				return this;
			}
			public Builder MergeAuthChallenge(CommandAuthChallenge Value)
			{
				if (((_bitField0 & 0x00000008) == 0x00000008) && AuthChallenge_ != CommandAuthChallenge.DefaultInstance)
				{
					AuthChallenge_ = CommandAuthChallenge.NewBuilder(AuthChallenge_).MergeFrom(Value).BuildPartial();
				}
				else
				{
					AuthChallenge_ = Value;
				}

				_bitField0 |= 0x00000008;
				return this;
			}
			public Builder ClearAuthChallenge()
			{
				AuthChallenge_ = CommandAuthChallenge.DefaultInstance;

				_bitField0 = (_bitField0 & ~0x00000008);
				return this;
			}

			// optional .pulsar.proto.CommandAuthResponse authResponse = 37;
			internal CommandAuthResponse AuthResponse_ = CommandAuthResponse.DefaultInstance;
			public bool HasAuthResponse()
			{
				return ((_bitField0 & 0x00000010) == 0x00000010);
			}
			public CommandAuthResponse GetAuthResponse()
			{
				return AuthResponse_;
			}
			public Builder SetAuthResponse(CommandAuthResponse Value)
			{
				if (Value == null)
				{
					throw new NullReferenceException();
				}
				AuthResponse_ = Value;

				_bitField0 |= 0x00000010;
				return this;
			}
			public Builder SetAuthResponse(CommandAuthResponse.Builder BuilderForValue)
			{
				AuthResponse_ = BuilderForValue.Build();

				_bitField0 |= 0x00000010;
				return this;
			}
			public Builder MergeAuthResponse(CommandAuthResponse Value)
			{
				if (((_bitField0 & 0x00000010) == 0x00000010) && AuthResponse_ != CommandAuthResponse.DefaultInstance)
				{
					AuthResponse_ = CommandAuthResponse.NewBuilder(AuthResponse_).MergeFrom(Value).BuildPartial();
				}
				else
				{
					AuthResponse_ = Value;
				}

				_bitField0 |= 0x00000010;
				return this;
			}
			public Builder ClearAuthResponse()
			{
				AuthResponse_ = CommandAuthResponse.DefaultInstance;

				_bitField0 = (_bitField0 & ~0x00000010);
				return this;
			}

			// optional .pulsar.proto.CommandAckResponse ackResponse = 38;
			internal CommandAckResponse AckResponse_ = CommandAckResponse.DefaultInstance;
			public bool HasAckResponse()
			{
				return ((_bitField0 & 0x00000020) == 0x00000020);
			}
			public CommandAckResponse GetAckResponse()
			{
				return AckResponse_;
			}
			public Builder SetAckResponse(CommandAckResponse Value)
			{
				if (Value == null)
				{
					throw new NullReferenceException();
				}
				AckResponse_ = Value;

				_bitField0 |= 0x00000020;
				return this;
			}
			public Builder SetAckResponse(CommandAckResponse.Builder BuilderForValue)
			{
				AckResponse_ = BuilderForValue.Build();

				_bitField0 |= 0x00000020;
				return this;
			}
			public Builder MergeAckResponse(CommandAckResponse Value)
			{
				if (((_bitField0 & 0x00000020) == 0x00000020) && AckResponse_ != CommandAckResponse.DefaultInstance)
				{
					AckResponse_ = CommandAckResponse.NewBuilder(AckResponse_).MergeFrom(Value).BuildPartial();
				}
				else
				{
					AckResponse_ = Value;
				}

				_bitField0 |= 0x00000020;
				return this;
			}
			public Builder ClearAckResponse()
			{
				AckResponse_ = CommandAckResponse.DefaultInstance;

				_bitField0 = (_bitField0 & ~0x00000020);
				return this;
			}

			// optional .pulsar.proto.CommandGetOrCreateSchema GetOrCreateSchema = 39;
			internal CommandGetOrCreateSchema GetOrCreateSchema_ = CommandGetOrCreateSchema.DefaultInstance;
			public bool HasGetOrCreateSchema()
			{
				return ((_bitField0 & 0x00000040) == 0x00000040);
			}
			public CommandGetOrCreateSchema GetGetOrCreateSchema()
			{
				return GetOrCreateSchema_;
			}
			public Builder SetGetOrCreateSchema(CommandGetOrCreateSchema Value)
			{
				if (Value == null)
				{
					throw new NullReferenceException();
				}
				GetOrCreateSchema_ = Value;

				_bitField0 |= 0x00000040;
				return this;
			}
			public Builder SetGetOrCreateSchema(CommandGetOrCreateSchema.Builder BuilderForValue)
			{
				GetOrCreateSchema_ = BuilderForValue.Build();

				_bitField0 |= 0x00000040;
				return this;
			}
			public Builder MergeGetOrCreateSchema(CommandGetOrCreateSchema Value)
			{
				if (((_bitField0 & 0x00000040) == 0x00000040) && GetOrCreateSchema_ != CommandGetOrCreateSchema.DefaultInstance)
				{
					GetOrCreateSchema_ = CommandGetOrCreateSchema.NewBuilder(GetOrCreateSchema_).MergeFrom(Value).BuildPartial();
				}
				else
				{
					GetOrCreateSchema_ = Value;
				}

				_bitField0 |= 0x00000040;
				return this;
			}
			public Builder ClearGetOrCreateSchema()
			{
				GetOrCreateSchema_ = CommandGetOrCreateSchema.DefaultInstance;

				_bitField0 = (_bitField0 & ~0x00000040);
				return this;
			}

			// optional .pulsar.proto.CommandGetOrCreateSchemaResponse GetOrCreateSchemaResponse = 40;
			internal CommandGetOrCreateSchemaResponse GetOrCreateSchemaResponse_ = CommandGetOrCreateSchemaResponse.DefaultInstance;
			public bool HasGetOrCreateSchemaResponse()
			{
				return ((_bitField0 & 0x00000080) == 0x00000080);
			}
			public CommandGetOrCreateSchemaResponse GetGetOrCreateSchemaResponse()
			{
				return GetOrCreateSchemaResponse_;
			}
			public Builder SetGetOrCreateSchemaResponse(CommandGetOrCreateSchemaResponse Value)
			{
				if (Value == null)
				{
					throw new NullReferenceException();
				}
				GetOrCreateSchemaResponse_ = Value;

				_bitField0 |= 0x00000080;
				return this;
			}
			public Builder SetGetOrCreateSchemaResponse(CommandGetOrCreateSchemaResponse.Builder BuilderForValue)
			{
				GetOrCreateSchemaResponse_ = BuilderForValue.Build();

				_bitField0 |= 0x00000080;
				return this;
			}
			public Builder MergeGetOrCreateSchemaResponse(CommandGetOrCreateSchemaResponse Value)
			{
				if (((_bitField0 & 0x00000080) == 0x00000080) && GetOrCreateSchemaResponse_ != CommandGetOrCreateSchemaResponse.DefaultInstance)
				{
					GetOrCreateSchemaResponse_ = CommandGetOrCreateSchemaResponse.NewBuilder(GetOrCreateSchemaResponse_).MergeFrom(Value).BuildPartial();
				}
				else
				{
					GetOrCreateSchemaResponse_ = Value;
				}

				_bitField0 |= 0x00000080;
				return this;
			}
			public Builder ClearGetOrCreateSchemaResponse()
			{
				GetOrCreateSchemaResponse_ = CommandGetOrCreateSchemaResponse.DefaultInstance;

				_bitField0 = (_bitField0 & ~0x00000080);
				return this;
			}

			// optional .pulsar.proto.CommandNewTxn newTxn = 50;
			internal CommandNewTxn NewTxn_ = CommandNewTxn.DefaultInstance;
			public bool HasNewTxn()
			{
				return ((_bitField0 & 0x00000100) == 0x00000100);
			}
			public CommandNewTxn GetNewTxn()
			{
				return NewTxn_;
			}
			public Builder SetNewTxn(CommandNewTxn Value)
			{
				if (Value == null)
				{
					throw new NullReferenceException();
				}
				NewTxn_ = Value;

				_bitField0 |= 0x00000100;
				return this;
			}
			public Builder SetNewTxn(CommandNewTxn.Builder BuilderForValue)
			{
				NewTxn_ = BuilderForValue.Build();

				_bitField0 |= 0x00000100;
				return this;
			}
			public Builder MergeNewTxn(CommandNewTxn Value)
			{
				if (((_bitField0 & 0x00000100) == 0x00000100) && NewTxn_ != CommandNewTxn.DefaultInstance)
				{
					NewTxn_ = CommandNewTxn.NewBuilder(NewTxn_).MergeFrom(Value).BuildPartial();
				}
				else
				{
					NewTxn_ = Value;
				}

				_bitField0 |= 0x00000100;
				return this;
			}
			public Builder ClearNewTxn()
			{
				NewTxn_ = CommandNewTxn.DefaultInstance;

				_bitField0 = (_bitField0 & ~0x00000100);
				return this;
			}

			// optional .pulsar.proto.CommandNewTxnResponse newTxnResponse = 51;
			internal CommandNewTxnResponse NewTxnResponse_ = CommandNewTxnResponse.DefaultInstance;
			public bool HasNewTxnResponse()
			{
				return ((_bitField0 & 0x00000200) == 0x00000200);
			}
			public CommandNewTxnResponse GetNewTxnResponse()
			{
				return NewTxnResponse_;
			}
			public Builder SetNewTxnResponse(CommandNewTxnResponse Value)
			{
				if (Value == null)
				{
					throw new NullReferenceException();
				}
				NewTxnResponse_ = Value;

				_bitField0 |= 0x00000200;
				return this;
			}
			public Builder SetNewTxnResponse(CommandNewTxnResponse.Builder BuilderForValue)
			{
				NewTxnResponse_ = BuilderForValue.Build();

				_bitField0 |= 0x00000200;
				return this;
			}
			public Builder MergeNewTxnResponse(CommandNewTxnResponse Value)
			{
				if (((_bitField0 & 0x00000200) == 0x00000200) && NewTxnResponse_ != CommandNewTxnResponse.DefaultInstance)
				{
					NewTxnResponse_ = CommandNewTxnResponse.NewBuilder(NewTxnResponse_).MergeFrom(Value).BuildPartial();
				}
				else
				{
					NewTxnResponse_ = Value;
				}

				_bitField0 |= 0x00000200;
				return this;
			}
			public Builder ClearNewTxnResponse()
			{
				NewTxnResponse_ = CommandNewTxnResponse.DefaultInstance;

				_bitField0 = (_bitField0 & ~0x00000200);
				return this;
			}

			// optional .pulsar.proto.CommandAddPartitionToTxn addPartitionToTxn = 52;
			internal CommandAddPartitionToTxn AddPartitionToTxn_ = CommandAddPartitionToTxn.DefaultInstance;
			public bool HasAddPartitionToTxn()
			{
				return ((_bitField0 & 0x00000400) == 0x00000400);
			}
			public CommandAddPartitionToTxn GetAddPartitionToTxn()
			{
				return AddPartitionToTxn_;
			}
			public Builder SetAddPartitionToTxn(CommandAddPartitionToTxn Value)
			{
				if (Value == null)
				{
					throw new NullReferenceException();
				}
				AddPartitionToTxn_ = Value;

				_bitField0 |= 0x00000400;
				return this;
			}
			public Builder SetAddPartitionToTxn(CommandAddPartitionToTxn.Builder BuilderForValue)
			{
				AddPartitionToTxn_ = BuilderForValue.Build();

				_bitField0 |= 0x00000400;
				return this;
			}
			public Builder MergeAddPartitionToTxn(CommandAddPartitionToTxn Value)
			{
				if (((_bitField0 & 0x00000400) == 0x00000400) && AddPartitionToTxn_ != CommandAddPartitionToTxn.DefaultInstance)
				{
					AddPartitionToTxn_ = CommandAddPartitionToTxn.NewBuilder(AddPartitionToTxn_).MergeFrom(Value).BuildPartial();
				}
				else
				{
					AddPartitionToTxn_ = Value;
				}

				_bitField0 |= 0x00000400;
				return this;
			}
			public Builder ClearAddPartitionToTxn()
			{
				AddPartitionToTxn_ = CommandAddPartitionToTxn.DefaultInstance;

				_bitField0 = (_bitField0 & ~0x00000400);
				return this;
			}

			// optional .pulsar.proto.CommandAddPartitionToTxnResponse addPartitionToTxnResponse = 53;
			internal CommandAddPartitionToTxnResponse AddPartitionToTxnResponse_ = CommandAddPartitionToTxnResponse.DefaultInstance;
			public bool HasAddPartitionToTxnResponse()
			{
				return ((_bitField0 & 0x00000800) == 0x00000800);
			}
			public CommandAddPartitionToTxnResponse GetAddPartitionToTxnResponse()
			{
				return AddPartitionToTxnResponse_;
			}
			public Builder SetAddPartitionToTxnResponse(CommandAddPartitionToTxnResponse Value)
			{
				if (Value == null)
				{
					throw new NullReferenceException();
				}
				AddPartitionToTxnResponse_ = Value;

				_bitField0 |= 0x00000800;
				return this;
			}
			public Builder SetAddPartitionToTxnResponse(CommandAddPartitionToTxnResponse.Builder BuilderForValue)
			{
				AddPartitionToTxnResponse_ = BuilderForValue.Build();

				_bitField0 |= 0x00000800;
				return this;
			}
			public Builder MergeAddPartitionToTxnResponse(CommandAddPartitionToTxnResponse Value)
			{
				if (((_bitField0 & 0x00000800) == 0x00000800) && AddPartitionToTxnResponse_ != CommandAddPartitionToTxnResponse.DefaultInstance)
				{
					AddPartitionToTxnResponse_ = CommandAddPartitionToTxnResponse.NewBuilder(AddPartitionToTxnResponse_).MergeFrom(Value).BuildPartial();
				}
				else
				{
					AddPartitionToTxnResponse_ = Value;
				}

				_bitField0 |= 0x00000800;
				return this;
			}
			public Builder ClearAddPartitionToTxnResponse()
			{
				AddPartitionToTxnResponse_ = CommandAddPartitionToTxnResponse.DefaultInstance;

				_bitField0 = (_bitField0 & ~0x00000800);
				return this;
			}

			// optional .pulsar.proto.CommandAddSubscriptionToTxn addSubscriptionToTxn = 54;
			internal CommandAddSubscriptionToTxn AddSubscriptionToTxn_ = CommandAddSubscriptionToTxn.DefaultInstance;
			public bool HasAddSubscriptionToTxn()
			{
				return ((_bitField0 & 0x00001000) == 0x00001000);
			}
			public CommandAddSubscriptionToTxn GetAddSubscriptionToTxn()
			{
				return AddSubscriptionToTxn_;
			}
			public Builder SetAddSubscriptionToTxn(CommandAddSubscriptionToTxn Value)
			{
				if (Value == null)
				{
					throw new NullReferenceException();
				}
				AddSubscriptionToTxn_ = Value;

				_bitField0 |= 0x00001000;
				return this;
			}
			public Builder SetAddSubscriptionToTxn(CommandAddSubscriptionToTxn.Builder BuilderForValue)
			{
				AddSubscriptionToTxn_ = BuilderForValue.Build();

				_bitField0 |= 0x00001000;
				return this;
			}
			public Builder MergeAddSubscriptionToTxn(CommandAddSubscriptionToTxn Value)
			{
				if (((_bitField0 & 0x00001000) == 0x00001000) && AddSubscriptionToTxn_ != CommandAddSubscriptionToTxn.DefaultInstance)
				{
					AddSubscriptionToTxn_ = CommandAddSubscriptionToTxn.NewBuilder(AddSubscriptionToTxn_).MergeFrom(Value).BuildPartial();
				}
				else
				{
					AddSubscriptionToTxn_ = Value;
				}

				_bitField0 |= 0x00001000;
				return this;
			}
			public Builder ClearAddSubscriptionToTxn()
			{
				AddSubscriptionToTxn_ = CommandAddSubscriptionToTxn.DefaultInstance;

				_bitField0 = (_bitField0 & ~0x00001000);
				return this;
			}

			// optional .pulsar.proto.CommandAddSubscriptionToTxnResponse addSubscriptionToTxnResponse = 55;
			internal CommandAddSubscriptionToTxnResponse AddSubscriptionToTxnResponse_ = CommandAddSubscriptionToTxnResponse.DefaultInstance;
			public bool HasAddSubscriptionToTxnResponse()
			{
				return ((_bitField0 & 0x00002000) == 0x00002000);
			}
			public CommandAddSubscriptionToTxnResponse GetAddSubscriptionToTxnResponse()
			{
				return AddSubscriptionToTxnResponse_;
			}
			public Builder SetAddSubscriptionToTxnResponse(CommandAddSubscriptionToTxnResponse Value)
			{
				if (Value == null)
				{
					throw new NullReferenceException();
				}
				AddSubscriptionToTxnResponse_ = Value;

				_bitField0 |= 0x00002000;
				return this;
			}
			public Builder SetAddSubscriptionToTxnResponse(CommandAddSubscriptionToTxnResponse.Builder BuilderForValue)
			{
				AddSubscriptionToTxnResponse_ = BuilderForValue.Build();

				_bitField0 |= 0x00002000;
				return this;
			}
			public Builder MergeAddSubscriptionToTxnResponse(CommandAddSubscriptionToTxnResponse Value)
			{
				if (((_bitField0 & 0x00002000) == 0x00002000) && AddSubscriptionToTxnResponse_ != CommandAddSubscriptionToTxnResponse.DefaultInstance)
				{
					AddSubscriptionToTxnResponse_ = CommandAddSubscriptionToTxnResponse.NewBuilder(AddSubscriptionToTxnResponse_).MergeFrom(Value).BuildPartial();
				}
				else
				{
					AddSubscriptionToTxnResponse_ = Value;
				}

				_bitField0 |= 0x00002000;
				return this;
			}
			public Builder ClearAddSubscriptionToTxnResponse()
			{
				AddSubscriptionToTxnResponse_ = CommandAddSubscriptionToTxnResponse.DefaultInstance;

				_bitField0 = (_bitField0 & ~0x00002000);
				return this;
			}

			// optional .pulsar.proto.CommandEndTxn endTxn = 56;
			internal CommandEndTxn EndTxn_ = CommandEndTxn.DefaultInstance;
			public bool HasEndTxn()
			{
				return ((_bitField0 & 0x00004000) == 0x00004000);
			}
			public CommandEndTxn GetEndTxn()
			{
				return EndTxn_;
			}
			public Builder SetEndTxn(CommandEndTxn Value)
			{
				if (Value == null)
				{
					throw new NullReferenceException();
				}
				EndTxn_ = Value;

				_bitField0 |= 0x00004000;
				return this;
			}
			public Builder SetEndTxn(CommandEndTxn.Builder BuilderForValue)
			{
				EndTxn_ = BuilderForValue.Build();

				_bitField0 |= 0x00004000;
				return this;
			}
			public Builder MergeEndTxn(CommandEndTxn Value)
			{
				if (((_bitField0 & 0x00004000) == 0x00004000) && EndTxn_ != CommandEndTxn.DefaultInstance)
				{
					EndTxn_ = CommandEndTxn.NewBuilder(EndTxn_).MergeFrom(Value).BuildPartial();
				}
				else
				{
					EndTxn_ = Value;
				}

				_bitField0 |= 0x00004000;
				return this;
			}
			public Builder ClearEndTxn()
			{
				EndTxn_ = CommandEndTxn.DefaultInstance;

				_bitField0 = (_bitField0 & ~0x00004000);
				return this;
			}

			// optional .pulsar.proto.CommandEndTxnResponse endTxnResponse = 57;
			internal CommandEndTxnResponse EndTxnResponse_ = CommandEndTxnResponse.DefaultInstance;
			public bool HasEndTxnResponse()
			{
				return ((_bitField0 & 0x00008000) == 0x00008000);
			}
			public CommandEndTxnResponse GetEndTxnResponse()
			{
				return EndTxnResponse_;
			}
			public Builder SetEndTxnResponse(CommandEndTxnResponse Value)
			{
				if (Value == null)
				{
					throw new NullReferenceException();
				}
				EndTxnResponse_ = Value;

				_bitField0 |= 0x00008000;
				return this;
			}
			public Builder SetEndTxnResponse(CommandEndTxnResponse.Builder BuilderForValue)
			{
				EndTxnResponse_ = BuilderForValue.Build();

				_bitField0 |= 0x00008000;
				return this;
			}
			public Builder MergeEndTxnResponse(CommandEndTxnResponse Value)
			{
				if (((_bitField0 & 0x00008000) == 0x00008000) && EndTxnResponse_ != CommandEndTxnResponse.DefaultInstance)
				{
					EndTxnResponse_ = CommandEndTxnResponse.NewBuilder(EndTxnResponse_).MergeFrom(Value).BuildPartial();
				}
				else
				{
					EndTxnResponse_ = Value;
				}

				_bitField0 |= 0x00008000;
				return this;
			}
			public Builder ClearEndTxnResponse()
			{
				EndTxnResponse_ = CommandEndTxnResponse.DefaultInstance;

				_bitField0 = (_bitField0 & ~0x00008000);
				return this;
			}

			// optional .pulsar.proto.CommandEndTxnOnPartition endTxnOnPartition = 58;
			internal CommandEndTxnOnPartition EndTxnOnPartition_ = CommandEndTxnOnPartition.DefaultInstance;
			public bool HasEndTxnOnPartition()
			{
				return ((_bitField0 & 0x00010000) == 0x00010000);
			}
			public CommandEndTxnOnPartition GetEndTxnOnPartition()
			{
				return EndTxnOnPartition_;
			}
			public Builder SetEndTxnOnPartition(CommandEndTxnOnPartition Value)
			{
				if (Value == null)
				{
					throw new NullReferenceException();
				}
				EndTxnOnPartition_ = Value;

				_bitField0 |= 0x00010000;
				return this;
			}
			public Builder SetEndTxnOnPartition(CommandEndTxnOnPartition.Builder BuilderForValue)
			{
				EndTxnOnPartition_ = BuilderForValue.Build();

				_bitField0 |= 0x00010000;
				return this;
			}
			public Builder MergeEndTxnOnPartition(CommandEndTxnOnPartition Value)
			{
				if (((_bitField0 & 0x00010000) == 0x00010000) && EndTxnOnPartition_ != CommandEndTxnOnPartition.DefaultInstance)
				{
					EndTxnOnPartition_ = CommandEndTxnOnPartition.NewBuilder(EndTxnOnPartition_).MergeFrom(Value).BuildPartial();
				}
				else
				{
					EndTxnOnPartition_ = Value;
				}

				_bitField0 |= 0x00010000;
				return this;
			}
			public Builder ClearEndTxnOnPartition()
			{
				EndTxnOnPartition_ = CommandEndTxnOnPartition.DefaultInstance;

				_bitField0 = (_bitField0 & ~0x00010000);
				return this;
			}

			// optional .pulsar.proto.CommandEndTxnOnPartitionResponse endTxnOnPartitionResponse = 59;
			internal CommandEndTxnOnPartitionResponse EndTxnOnPartitionResponse_ = CommandEndTxnOnPartitionResponse.DefaultInstance;
			public bool HasEndTxnOnPartitionResponse()
			{
				return ((_bitField0 & 0x00020000) == 0x00020000);
			}
			public CommandEndTxnOnPartitionResponse GetEndTxnOnPartitionResponse()
			{
				return EndTxnOnPartitionResponse_;
			}
			public Builder SetEndTxnOnPartitionResponse(CommandEndTxnOnPartitionResponse Value)
			{
				if (Value == null)
				{
					throw new NullReferenceException();
				}
				EndTxnOnPartitionResponse_ = Value;

				_bitField0 |= 0x00020000;
				return this;
			}
			public Builder SetEndTxnOnPartitionResponse(CommandEndTxnOnPartitionResponse.Builder BuilderForValue)
			{
				EndTxnOnPartitionResponse_ = BuilderForValue.Build();

				_bitField0 |= 0x00020000;
				return this;
			}
			public Builder MergeEndTxnOnPartitionResponse(CommandEndTxnOnPartitionResponse Value)
			{
				if (((_bitField0 & 0x00020000) == 0x00020000) && EndTxnOnPartitionResponse_ != CommandEndTxnOnPartitionResponse.DefaultInstance)
				{
					EndTxnOnPartitionResponse_ = CommandEndTxnOnPartitionResponse.NewBuilder(EndTxnOnPartitionResponse_).MergeFrom(Value).BuildPartial();
				}
				else
				{
					EndTxnOnPartitionResponse_ = Value;
				}

				_bitField0 |= 0x00020000;
				return this;
			}
			public Builder ClearEndTxnOnPartitionResponse()
			{
				EndTxnOnPartitionResponse_ = CommandEndTxnOnPartitionResponse.DefaultInstance;

				_bitField0 = (_bitField0 & ~0x00020000);
				return this;
			}

			// optional .pulsar.proto.CommandEndTxnOnSubscription endTxnOnSubscription = 60;
			internal CommandEndTxnOnSubscription _endTxnOnSubscription = CommandEndTxnOnSubscription.DefaultInstance;
			public bool HasEndTxnOnSubscription()
			{
				return ((_bitField0 & 0x00040000) == 0x00040000);
			}
			public CommandEndTxnOnSubscription GetEndTxnOnSubscription()
			{
				return _endTxnOnSubscription;
			}
			public Builder SetEndTxnOnSubscription(CommandEndTxnOnSubscription Value)
			{
				if (Value == null)
				{
					throw new NullReferenceException();
				}
				_endTxnOnSubscription = Value;

				_bitField0 |= 0x00040000;
				return this;
			}
			public Builder SetEndTxnOnSubscription(CommandEndTxnOnSubscription.Builder BuilderForValue)
			{
				_endTxnOnSubscription = BuilderForValue.Build();

				_bitField0 |= 0x00040000;
				return this;
			}
			public Builder MergeEndTxnOnSubscription(CommandEndTxnOnSubscription Value)
			{
				if (((_bitField0 & 0x00040000) == 0x00040000) && _endTxnOnSubscription != CommandEndTxnOnSubscription.DefaultInstance)
				{
					_endTxnOnSubscription = CommandEndTxnOnSubscription.NewBuilder(_endTxnOnSubscription).MergeFrom(Value).BuildPartial();
				}
				else
				{
					_endTxnOnSubscription = Value;
				}

				_bitField0 |= 0x00040000;
				return this;
			}
			public Builder ClearEndTxnOnSubscription()
			{
				_endTxnOnSubscription = CommandEndTxnOnSubscription.DefaultInstance;

				_bitField0 = (_bitField0 & ~0x00040000);
				return this;
			}

			
			internal CommandEndTxnOnSubscriptionResponse _endTxnOnSubscriptionResponse = CommandEndTxnOnSubscriptionResponse.DefaultInstance;
			public bool HasEndTxnOnSubscriptionResponse()
			{
				return ((_bitField0 & 0x00080000) == 0x00080000);
			}
			public CommandEndTxnOnSubscriptionResponse GetEndTxnOnSubscriptionResponse()
			{
				return _endTxnOnSubscriptionResponse;
			}
			public Builder SetEndTxnOnSubscriptionResponse(CommandEndTxnOnSubscriptionResponse Value)
			{
				if (Value == null)
				{
					throw new NullReferenceException();
				}
				_endTxnOnSubscriptionResponse = Value;

				_bitField0 |= 0x00080000;
				return this;
			}
			public Builder SetEndTxnOnSubscriptionResponse(CommandEndTxnOnSubscriptionResponse.Builder BuilderForValue)
			{
				_endTxnOnSubscriptionResponse = BuilderForValue.Build();

				_bitField0 |= 0x00080000;
				return this;
			}
			public Builder MergeEndTxnOnSubscriptionResponse(CommandEndTxnOnSubscriptionResponse Value)
			{
				if (((_bitField0 & 0x00080000) == 0x00080000) && _endTxnOnSubscriptionResponse != CommandEndTxnOnSubscriptionResponse.DefaultInstance)
				{
					_endTxnOnSubscriptionResponse = CommandEndTxnOnSubscriptionResponse.NewBuilder(_endTxnOnSubscriptionResponse).MergeFrom(Value).BuildPartial();
				}
				else
				{
					_endTxnOnSubscriptionResponse = Value;
				}

				_bitField0 |= 0x00080000;
				return this;
			}
			public Builder ClearEndTxnOnSubscriptionResponse()
			{
				_endTxnOnSubscriptionResponse = CommandEndTxnOnSubscriptionResponse.DefaultInstance;
				_type = 0;
				return this;
			}

			// @@protoc_insertion_point(builder_scope:pulsar.proto.BaseCommand)
		}

		static BaseCommand()
		{
			_defaultInstance = new BaseCommand(true);
			_defaultInstance.InitFields();
		}

		// @@protoc_insertion_point(class_scope:pulsar.proto.BaseCommand)
	}

}
