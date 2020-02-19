using DotNetty.Common;
using Google.Protobuf;
using System;
using static SharpPulsar.Utility.Protobuf.ByteBufCodedInputStream;
using static SharpPulsar.Utility.Protobuf.ByteBufCodedOutputStream;
using System.Linq;
using SharpPulsar.Utility.Protobuf;

namespace SharpPulsar.Protocol.Proto
{
	public partial class BaseCommand: ByteBufGeneratedMessage
	{
		// Use BaseCommand.newBuilder() to construct.
		public static ThreadLocalPool<BaseCommand> _pool = new ThreadLocalPool<BaseCommand>(handle => new BaseCommand(handle), 1, true);

		public ThreadLocalPool.Handle _handle;
		private BaseCommand(ThreadLocalPool.Handle handle)
		{
			_handle = handle;
		}
		public int _bitField0 = 0;
		public int _bitField1 = 0;
		public void Recycle()
		{
			InitFields();
			MemoizedIsInitialized = -1;
			_hasBits0 = 0;
			MemoizedSerializedSize = -1;
            _handle?.Release(this);
        }

		public BaseCommand(bool NoInit)
		{
		}

		
		public static readonly BaseCommand _defaultInstance;
		public static BaseCommand DefaultInstance => _defaultInstance;

        public BaseCommand DefaultInstanceForType => _defaultInstance;

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
		public sbyte MemoizedIsInitialized = -1;
		public bool Initialized
		{
			get
			{
				var IsInitialized = MemoizedIsInitialized;
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

		public int MemoizedSerializedSize = -1;
		

		public const long SerialVersionUID = 0L;
		
		
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
			public static ThreadLocalPool<Builder> Pool = new ThreadLocalPool<Builder>(handle => new Builder(handle), 1, true);
			public int BitField0 = 0;
			public int BitField1 = 0;
			public ThreadLocalPool.Handle Handle;
			private Builder(ThreadLocalPool.Handle handle)
			{
				Handle = handle;
				MaybeForceBuilderInitialization();
			}
			

			public void Recycle()
			{
				Clear();
                Handle?.Release(this);

            }
			
			public void MaybeForceBuilderInitialization()
			{
			}
			public static Builder Create()
			{
				return Pool.Take();
			}

			public Builder Clear()
			{
                _type = 0;
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

			public BaseCommand DefaultInstanceForType => DefaultInstance;

            public BaseCommand Build()
			{
				var result = BuildPartial();
				//proto3 always returns true
				/*if (!result.IsInitialized())
				{
					throw new NullReferenceException("BaseCommand not initialized");
				}*/
				return result;
			}
			Types.Type _type;
			
			public BaseCommand BuildPartial()
			{
				var result = BaseCommand._pool.Take();
				var fromBitField0 = BitField0;
				var fromBitField1 = BitField1;
				var toBitField0 = 0;
				var toBitField1 = 0;
				if (((fromBitField0 & 0x00000001) == 0x00000001))
				{
					toBitField0 |= 0x00000001;
				}
				result.Type = _type;
				if (((fromBitField0 & 0x00000002) == 0x00000002))
				{
					toBitField0 |= 0x00000002;
				}
				result.Connect = Connect;
				if (((fromBitField0 & 0x00000004) == 0x00000004))
				{
					toBitField0 |= 0x00000004;
				}
				result.Connected = Connected;
				if (((fromBitField0 & 0x00000008) == 0x00000008))
				{
					toBitField0 |= 0x00000008;
				}
				result.Subscribe = Subscribe;
				if (((fromBitField0 & 0x00000010) == 0x00000010))
				{
					toBitField0 |= 0x00000010;
				}
				result.Producer = Producer;
				if (((fromBitField0 & 0x00000020) == 0x00000020))
				{
					toBitField0 |= 0x00000020;
				}
				result.Send = Send;
				if (((fromBitField0 & 0x00000040) == 0x00000040))
				{
					toBitField0 |= 0x00000040;
				}
				result.SendReceipt = SendReceipt;
				if (((fromBitField0 & 0x00000080) == 0x00000080))
				{
					toBitField0 |= 0x00000080;
				}
				result.SendError = SendError;
				if (((fromBitField0 & 0x00000100) == 0x00000100))
				{
					toBitField0 |= 0x00000100;
				}
				result.Message = Message;
				if (((fromBitField0 & 0x00000200) == 0x00000200))
				{
					toBitField0 |= 0x00000200;
				}
				result.Ack = Ack;
				if (((fromBitField0 & 0x00000400) == 0x00000400))
				{
					toBitField0 |= 0x00000400;
				}
				result.Flow = Flow;
				if (((fromBitField0 & 0x00000800) == 0x00000800))
				{
					toBitField0 |= 0x00000800;
				}
				result.Unsubscribe = Unsubscribe;
				if (((fromBitField0 & 0x00001000) == 0x00001000))
				{
					toBitField0 |= 0x00001000;
				}
				result.Success = Success;
				if (((fromBitField0 & 0x00002000) == 0x00002000))
				{
					toBitField0 |= 0x00002000;
				}
				result.Error = Error;
				if (((fromBitField0 & 0x00004000) == 0x00004000))
				{
					toBitField0 |= 0x00004000;
				}
				result.CloseProducer = CloseProducer;
				if (((fromBitField0 & 0x00008000) == 0x00008000))
				{
					toBitField0 |= 0x00008000;
				}
				result.CloseConsumer = CloseConsumer;
				if (((fromBitField0 & 0x00010000) == 0x00010000))
				{
					toBitField0 |= 0x00010000;
				}
				result.ProducerSuccess = ProducerSuccess;
				if (((fromBitField0 & 0x00020000) == 0x00020000))
				{
					toBitField0 |= 0x00020000;
				}
				result.Ping = Ping;
				if (((fromBitField0 & 0x00040000) == 0x00040000))
				{
					toBitField0 |= 0x00040000;
				}
				result.Pong = Pong;
				if (((fromBitField0 & 0x00080000) == 0x00080000))
				{
					toBitField0 |= 0x00080000;
				}
				result.RedeliverUnacknowledgedMessages = RedeliverUnacknowledgedMessages;
				if (((fromBitField0 & 0x00100000) == 0x00100000))
				{
					toBitField0 |= 0x00100000;
				}
				result.PartitionMetadata = PartitionMetadata;
				if (((fromBitField0 & 0x00200000) == 0x00200000))
				{
					toBitField0 |= 0x00200000;
				}
				result.PartitionMetadataResponse = PartitionMetadataResponse;
				if (((fromBitField0 & 0x00400000) == 0x00400000))
				{
					toBitField0 |= 0x00400000;
				}
				result.LookupTopic = LookupTopic;
				if (((fromBitField0 & 0x00800000) == 0x00800000))
				{
					toBitField0 |= 0x00800000;
				}
				result.LookupTopicResponse = LookupTopicResponse;
				if (((fromBitField0 & 0x01000000) == 0x01000000))
				{
					toBitField0 |= 0x01000000;
				}
				result.ConsumerStats = ConsumerStats;
				if (((fromBitField0 & 0x02000000) == 0x02000000))
				{
					toBitField0 |= 0x02000000;
				}
				result.ConsumerStatsResponse = ConsumerStatsResponse;
				if (((fromBitField0 & 0x04000000) == 0x04000000))
				{
					toBitField0 |= 0x04000000;
				}
				result.ReachedEndOfTopic = ReachedEndOfTopic;
				if (((fromBitField0 & 0x08000000) == 0x08000000))
				{
					toBitField0 |= 0x08000000;
				}
				result.Seek = Seek;
				if (((fromBitField0 & 0x10000000) == 0x10000000))
				{
					toBitField0 |= 0x10000000;
				}
				result.GetLastMessageId = GetLastMessageId;
				if (((fromBitField0 & 0x20000000) == 0x20000000))
				{
					toBitField0 |= 0x20000000;
				}
				result.GetLastMessageIdResponse = GetLastMessageIdResponse;
				if (((fromBitField0 & 0x40000000) == 0x40000000))
				{
					toBitField0 |= 0x40000000;
				}
				result.ActiveConsumerChange = ActiveConsumerChange;
				if (((fromBitField0 & 0x80000000) == 0x80000000))
				{
					toBitField0 |= unchecked((int)0x80000000);
				}
				result.GetTopicsOfNamespace = GetTopicsOfNamespace;
				if (((fromBitField1 & 0x00000001) == 0x00000001))
				{
					toBitField1 |= 0x00000001;
				}
				result.GetTopicsOfNamespaceResponse = GetTopicsOfNamespaceResponse;
				if (((fromBitField1 & 0x00000002) == 0x00000002))
				{
					toBitField1 |= 0x00000002;
				}
				result.GetSchema = GetSchema;
				if (((fromBitField1 & 0x00000004) == 0x00000004))
				{
					toBitField1 |= 0x00000004;
				}
				result.GetSchemaResponse = GetSchemaResponse;
				if (((fromBitField1 & 0x00000008) == 0x00000008))
				{
					toBitField1 |= 0x00000008;
				}
				result.AuthChallenge = AuthChallenge;
				if (((fromBitField1 & 0x00000010) == 0x00000010))
				{
					toBitField1 |= 0x00000010;
				}
				result.AuthResponse = AuthResponse;
				if (((fromBitField1 & 0x00000020) == 0x00000020))
				{
					toBitField1 |= 0x00000020;
				}
				result.AckResponse = AckResponse;
				if (((fromBitField1 & 0x00000040) == 0x00000040))
				{
					toBitField1 |= 0x00000040;
				}
				result.GetOrCreateSchema = GetOrCreateSchema;
				if (((fromBitField1 & 0x00000080) == 0x00000080))
				{
					toBitField1 |= 0x00000080;
				}
				result.GetOrCreateSchemaResponse = GetOrCreateSchemaResponse;
				if (((fromBitField1 & 0x00000100) == 0x00000100))
				{
					toBitField1 |= 0x00000100;
				}
				result.NewTxn = NewTxn;
				if (((fromBitField1 & 0x00000200) == 0x00000200))
				{
					toBitField1 |= 0x00000200;
				}
				result.NewTxnResponse = NewTxnResponse;
				if (((fromBitField1 & 0x00000400) == 0x00000400))
				{
					toBitField1 |= 0x00000400;
				}
				result.AddPartitionToTxn = AddPartitionToTxn;
				if (((fromBitField1 & 0x00000800) == 0x00000800))
				{
					toBitField1 |= 0x00000800;
				}
				result.AddPartitionToTxnResponse = AddPartitionToTxnResponse;
				if (((fromBitField1 & 0x00001000) == 0x00001000))
				{
					toBitField1 |= 0x00001000;
				}
				result.AddSubscriptionToTxn = AddSubscriptionToTxn;
				if (((fromBitField1 & 0x00002000) == 0x00002000))
				{
					toBitField1 |= 0x00002000;
				}
				result.AddSubscriptionToTxnResponse = AddSubscriptionToTxnResponse;
				if (((fromBitField1 & 0x00004000) == 0x00004000))
				{
					toBitField1 |= 0x00004000;
				}
				result.EndTxn = EndTxn;
				if (((fromBitField1 & 0x00008000) == 0x00008000))
				{
					toBitField1 |= 0x00008000;
				}
				result.EndTxnResponse = EndTxnResponse;
				if (((fromBitField1 & 0x00010000) == 0x00010000))
				{
					toBitField1 |= 0x00010000;
				}
				result.EndTxnOnPartition = EndTxnOnPartition;
				if (((fromBitField1 & 0x00020000) == 0x00020000))
				{
					toBitField1 |= 0x00020000;
				}
				result.EndTxnOnPartitionResponse = EndTxnOnPartitionResponse;
				if (((fromBitField1 & 0x00040000) == 0x00040000))
				{
					toBitField1 |= 0x00040000;
				}
				result.EndTxnOnSubscription = EndTxnOnSubscription;
				if (((fromBitField1 & 0x00080000) == 0x00080000))
				{
					toBitField1 |= 0x00080000;
				}
				result.EndTxnOnSubscriptionResponse = EndTxnOnSubscriptionResponse;
				result._hasBits0 = toBitField0;
				result._bitField1 = toBitField1;
				return result;
			}

			public Builder MergeFrom(BaseCommand other)
			{
				if (other == DefaultInstance)
				{
					return this;
				}
				if (other.HasType)
				{
					SetType(other.Type);
				}
				if (other.HasConnect)
				{
					MergeConnect(other.Connect);
				}
				if (other.HasConnected)
				{
					MergeConnected(other.Connected);
				}
				if (other.HasSubscribe)
				{
					MergeSubscribe(other.Subscribe);
				}
				if (other.HasProducer)
				{
					MergeProducer(other.Producer);
				}
				if (other.HasSend)
				{
					MergeSend(other.Send);
				}
				if (other.HasSendReceipt)
				{
					MergeSendReceipt(other.SendReceipt);
				}
				if (other.HasSendError)
				{
					MergeSendError(other.SendError);
				}
				if (other.HasMessage)
				{
					MergeMessage(other.Message);
				}
				if (other.HasAck)
				{
					MergeAck(other.Ack);
				}
				if (other.HasFlow)
				{
					MergeFlow(other.Flow);
				}
				if (other.HasUnsubscribe)
				{
					MergeUnsubscribe(other.Unsubscribe);
				}
				if (other.HasSuccess)
				{
					MergeSuccess(other.Success);
				}
				if (other.HasError)
				{
					MergeError(other.Error);
				}
				if (other.HasCloseProducer)
				{
					MergeCloseProducer(other.CloseProducer);
				}
				if (other.HasCloseConsumer)
				{
					MergeCloseConsumer(other.CloseConsumer);
				}
				if (other.HasProducerSuccess)
				{
					MergeProducerSuccess(other.ProducerSuccess);
				}
				if (other.HasPing)
				{
					MergePing(other.Ping);
				}
				if (other.HasPong)
				{
					MergePong(other.Pong);
				}
				if (other.HasRedeliverUnacknowledgedMessages)
				{
					MergeRedeliverUnacknowledgedMessages(other.RedeliverUnacknowledgedMessages);
				}
				if (other.HasPartitionMetadata)
				{
					MergePartitionMetadata(other.PartitionMetadata);
				}
				if (other.HasPartitionMetadataResponse)
				{
					MergePartitionMetadataResponse(other.PartitionMetadataResponse);
				}
				if (other.HasLookupTopic)
				{
					MergeLookupTopic(other.LookupTopic);
				}
				if (other.HasLookupTopicResponse)
				{
					MergeLookupTopicResponse(other.LookupTopicResponse);
				}
				if (other.HasConsumerStats)
				{
					MergeConsumerStats(other.ConsumerStats);
				}
				if (other.HasConsumerStatsResponse)
				{
					MergeConsumerStatsResponse(other.ConsumerStatsResponse);
				}
				if (other.HasReachedEndOfTopic)
				{
					MergeReachedEndOfTopic(other.ReachedEndOfTopic);
				}
				if (other.HasSeek)
				{
					MergeSeek(other.Seek);
				}
				if (other.HasGetLastMessageId)
				{
					MergeGetLastMessageId(other.GetLastMessageId);
				}
				if (other.HasGetLastMessageIdResponse)
				{
					MergeGetLastMessageIdResponse(other.GetLastMessageIdResponse);
				}
				if (other.HasActiveConsumerChange)
				{
					MergeActiveConsumerChange(other.ActiveConsumerChange);
				}
				if (other.HasGetTopicsOfNamespace)
				{
					MergeGetTopicsOfNamespace(other.GetTopicsOfNamespace);
				}
				if (other.HasGetTopicsOfNamespaceResponse)
				{
					MergeGetTopicsOfNamespaceResponse(other.GetTopicsOfNamespaceResponse);
				}
				if (other.HasGetSchema)
				{
					MergeGetSchema(other.GetSchema);
				}
				if (other.HasGetSchemaResponse)
				{
					MergeGetSchemaResponse(other.GetSchemaResponse);
				}
				if (other.HasAuthChallenge)
				{
					MergeAuthChallenge(other.AuthChallenge);
				}
				if (other.HasAuthResponse)
				{
					MergeAuthResponse(other.AuthResponse);
				}
				if (other.HasAckResponse)
				{
					MergeAckResponse(other.AckResponse);
				}
				if (other.HasGetOrCreateSchema)
				{
					MergeGetOrCreateSchema(other.GetOrCreateSchema);
				}
				if (other.HasGetOrCreateSchemaResponse)
				{
					MergeGetOrCreateSchemaResponse(other.GetOrCreateSchemaResponse);
				}
				if (other.HasNewTxn)
				{
					MergeNewTxn(other.NewTxn);
				}
				if (other.HasNewTxnResponse)
				{
					MergeNewTxnResponse(other.NewTxnResponse);
				}
				if (other.HasAddPartitionToTxn)
				{
					MergeAddPartitionToTxn(other.AddPartitionToTxn);
				}
				if (other.HasAddPartitionToTxnResponse)
				{
					MergeAddPartitionToTxnResponse(other.AddPartitionToTxnResponse);
				}
				if (other.HasAddSubscriptionToTxn)
				{
					MergeAddSubscriptionToTxn(other.AddSubscriptionToTxn);
				}
				if (other.HasAddSubscriptionToTxnResponse)
				{
					MergeAddSubscriptionToTxnResponse(other.AddSubscriptionToTxnResponse);
				}
				if (other.HasEndTxn)
				{
					MergeEndTxn(other.EndTxn);
				}
				if (other.HasEndTxnResponse)
				{
					MergeEndTxnResponse(other.EndTxnResponse);
				}
				if (other.HasEndTxnOnPartition)
				{
					MergeEndTxnOnPartition(other.EndTxnOnPartition);
				}
				if (other.HasEndTxnOnPartitionResponse)
				{
					MergeEndTxnOnPartitionResponse(other.EndTxnOnPartitionResponse);
				}
				if (other.HasEndTxnOnSubscription)
				{
					MergeEndTxnOnSubscription(other.EndTxnOnSubscription);
				}
				if (other.HasEndTxnOnSubscriptionResponse)
				{
					MergeEndTxnOnSubscriptionResponse(other.EndTxnOnSubscriptionResponse);
				}
				return this;
			}
			public ByteBufMessageBuilder MergeFrom(ByteBufCodedInputStream input, ExtensionRegistry extensionRegistry)
			{
				while (true)
				{
					var tag = input.ReadTag();
					switch (tag)
					{
						case 0:

							return this;
						default:
							{
								if (!input.SkipField(tag))
								{

									return this;
								}
								break;
							}
						case 8:
							{
								var rawValue = input.ReadEnum();
								var value = Enum.GetValues(typeof(Types.Type)).Cast<Types.Type>().ToList()[rawValue];
								if (value != null)
								{
									BitField0 |= 0x00000001;
									_type = value;
								}
								break;
							}
						case 18:
							{
								var subBuilder = CommandConnect.NewBuilder();
								if (HasConnect())
								{
									subBuilder.MergeFrom(GetConnect());
								}
								input.ReadMessage(subBuilder, extensionRegistry);
								SetConnect(subBuilder.BuildPartial());
								subBuilder.Recycle();
								break;
							}
						case 26:
							{
								var subBuilder = CommandConnected.NewBuilder();
								if (HasConnected())
								{
									subBuilder.MergeFrom(GetConnected());
								}
								input.ReadMessage(subBuilder, extensionRegistry);
								SetConnected(subBuilder);
								subBuilder.Recycle();
								break;
							}
						case 34:
							{
								var subBuilder = CommandSubscribe.NewBuilder();
								if (HasSubscribe())
								{
									subBuilder.MergeFrom(GetSubscribe());
								}
								input.ReadMessage(subBuilder, extensionRegistry);
								SetSubscribe(subBuilder.BuildPartial());
								subBuilder.Recycle();
								break;
							}
						case 42:
							{
								var subBuilder = CommandProducer.NewBuilder();
								if (HasProducer())
								{
									subBuilder.MergeFrom(GetProducer());
								}
								input.ReadMessage(subBuilder, extensionRegistry);
								SetProducer(subBuilder.BuildPartial());
								subBuilder.Recycle();
								break;
							}
						case 50:
							{
								var subBuilder = CommandSend.NewBuilder();
								if (HasSend())
								{
									subBuilder.MergeFrom(GetSend());
								}
								input.ReadMessage(subBuilder, extensionRegistry);
								SetSend(subBuilder.BuildPartial());
								subBuilder.Recycle();
								break;
							}
						case 58:
							{
								var subBuilder = CommandSendReceipt.NewBuilder();
								if (HasSendReceipt())
								{
									subBuilder.MergeFrom(GetSendReceipt());
								}
								input.ReadMessage(subBuilder, extensionRegistry);
								SetSendReceipt(subBuilder.BuildPartial());
								subBuilder.Recycle();
								break;
							}
						case 66:
							{
								var subBuilder = CommandSendError.NewBuilder();
								if (HasSendError())
								{
									subBuilder.MergeFrom(GetSendError());
								}
								input.ReadMessage(subBuilder, extensionRegistry);
								SetSendError(subBuilder.BuildPartial());
								subBuilder.Recycle();
								break;
							}
						case 74:
							{
								var subBuilder = CommandMessage.NewBuilder();
								if (HasMessage())
								{
									subBuilder.MergeFrom(GetMessage());
								}
								input.ReadMessage(subBuilder, extensionRegistry);
								SetMessage(subBuilder.BuildPartial());
								subBuilder.Recycle();
								break;
							}
						case 82:
							{
								var subBuilder = CommandAck.NewBuilder();
								if (HasAck())
								{
									subBuilder.MergeFrom(GetAck());
								}
								input.ReadMessage(subBuilder, extensionRegistry);
								SetAck(subBuilder.BuildPartial());
								subBuilder.Recycle();
								break;
							}
						case 90:
							{
								var subBuilder = CommandFlow.NewBuilder();
								if (HasFlow())
								{
									subBuilder.MergeFrom(GetFlow());
								}
								input.ReadMessage(subBuilder, extensionRegistry);
								SetFlow(subBuilder.BuildPartial());
								subBuilder.Recycle();
								break;
							}
						case 98:
							{
								var subBuilder = CommandUnsubscribe.NewBuilder();
								if (HasUnsubscribe())
								{
									subBuilder.MergeFrom(GetUnsubscribe());
								}
								input.ReadMessage(subBuilder, extensionRegistry);
								SetUnsubscribe(subBuilder.BuildPartial());
								subBuilder.Recycle();
								break;
							}
						case 106:
							{
								var subBuilder = CommandSuccess.NewBuilder();
								if (HasSuccess())
								{
									subBuilder.MergeFrom(GetSuccess());
								}
								input.ReadMessage(subBuilder, extensionRegistry);
								SetSuccess(subBuilder.BuildPartial());
								subBuilder.Recycle();
								break;
							}
						case 114:
							{
								var subBuilder = CommandError.NewBuilder();
								if (HasError())
								{
									subBuilder.MergeFrom(GetError());
								}
								input.ReadMessage(subBuilder, extensionRegistry);
								SetError(subBuilder.BuildPartial());
								subBuilder.Recycle();
								break;
							}
						case 122:
							{
								var subBuilder = CommandCloseProducer.NewBuilder();
								if (HasCloseProducer())
								{
									subBuilder.MergeFrom(GetCloseProducer());
								}
								input.ReadMessage(subBuilder, extensionRegistry);
								SetCloseProducer(subBuilder.BuildPartial());
								subBuilder.Recycle();
								break;
							}
						case 130:
							{
								var subBuilder = CommandCloseConsumer.NewBuilder();
								if (HasCloseConsumer())
								{
									subBuilder.MergeFrom(GetCloseConsumer());
								}
								input.ReadMessage(subBuilder, extensionRegistry);
								SetCloseConsumer(subBuilder.BuildPartial());
								subBuilder.Recycle();
								break;
							}
						case 138:
							{
								var subBuilder = CommandProducerSuccess.NewBuilder();
								if (HasProducerSuccess())
								{
									subBuilder.MergeFrom(GetProducerSuccess());
								}
								input.ReadMessage(subBuilder, extensionRegistry);
								SetProducerSuccess(subBuilder.BuildPartial());
								subBuilder.Recycle();
								break;
							}
						case 146:
							{
								var subBuilder = CommandPing.NewBuilder();
								if (HasPing())
								{
									subBuilder.MergeFrom(GetPing());
								}
								input.ReadMessage(subBuilder, extensionRegistry);
								SetPing(subBuilder.BuildPartial());
								subBuilder.Recycle();
								break;
							}
						case 154:
							{
								var subBuilder = CommandPong.NewBuilder();
								if (HasPong())
								{
									subBuilder.MergeFrom(GetPong());
								}
								input.ReadMessage(subBuilder, extensionRegistry);
								SetPong(subBuilder.BuildPartial());
								subBuilder.Recycle();
								break;
							}
						case 162:
							{
								var subBuilder = CommandRedeliverUnacknowledgedMessages.NewBuilder();
								if (HasRedeliverUnacknowledgedMessages())
								{
									subBuilder.MergeFrom(GetRedeliverUnacknowledgedMessages());
								}
								input.ReadMessage(subBuilder, extensionRegistry);
								SetRedeliverUnacknowledgedMessages(subBuilder.BuildPartial());
								subBuilder.Recycle();
								break;
							}
						case 170:
							{
								var subBuilder = CommandPartitionedTopicMetadata.NewBuilder();
								if (HasPartitionMetadata())
								{
									subBuilder.MergeFrom(GetPartitionMetadata());
								}
								input.ReadMessage(subBuilder, extensionRegistry);
								SetPartitionMetadata(subBuilder.BuildPartial());
								subBuilder.Recycle();
								break;
							}
						case 178:
							{
								var subBuilder = CommandPartitionedTopicMetadataResponse.NewBuilder();
								if (HasPartitionMetadataResponse())
								{
									subBuilder.MergeFrom(GetPartitionMetadataResponse());
								}
								input.ReadMessage(subBuilder, extensionRegistry);
								SetPartitionMetadataResponse(subBuilder.BuildPartial());
								subBuilder.Recycle();
								break;
							}
						case 186:
							{
								var subBuilder = CommandLookupTopic.NewBuilder();
								if (HasLookupTopic())
								{
									subBuilder.MergeFrom(GetLookupTopic());
								}
								input.ReadMessage(subBuilder, extensionRegistry);
								SetLookupTopic(subBuilder.BuildPartial());
								subBuilder.Recycle();
								break;
							}
						case 194:
							{
								var subBuilder = CommandLookupTopicResponse.NewBuilder();
								if (HasLookupTopicResponse())
								{
									subBuilder.MergeFrom(GetLookupTopicResponse());
								}
								input.ReadMessage(subBuilder, extensionRegistry);
								SetLookupTopicResponse(subBuilder.BuildPartial());
								subBuilder.Recycle();
								break;
							}
						case 202:
							{
								var subBuilder = CommandConsumerStats.NewBuilder();
								if (HasConsumerStats())
								{
									subBuilder.MergeFrom(GetConsumerStats());
								}
								input.ReadMessage(subBuilder, extensionRegistry);
								SetConsumerStats(subBuilder.BuildPartial());
								subBuilder.Recycle();
								break;
							}
						case 210:
							{
								var subBuilder = CommandConsumerStatsResponse.NewBuilder();
								if (HasConsumerStatsResponse())
								{
									subBuilder.MergeFrom(GetConsumerStatsResponse());
								}
								input.ReadMessage(subBuilder, extensionRegistry);
								SetConsumerStatsResponse(subBuilder.BuildPartial());
								subBuilder.Recycle();
								break;
							}
						case 218:
							{
								var subBuilder = CommandReachedEndOfTopic.NewBuilder();
								if (HasReachedEndOfTopic())
								{
									subBuilder.MergeFrom(GetReachedEndOfTopic());
								}
								input.ReadMessage(subBuilder, extensionRegistry);
								SetReachedEndOfTopic(subBuilder.BuildPartial());
								subBuilder.Recycle();
								break;
							}
						case 226:
							{
								var subBuilder = CommandSeek.NewBuilder();
								if (HasSeek())
								{
									subBuilder.MergeFrom(GetSeek());
								}
								input.ReadMessage(subBuilder, extensionRegistry);
								SetSeek(subBuilder.BuildPartial());
								subBuilder.Recycle();
								break;
							}
						case 234:
							{
								var subBuilder = CommandGetLastMessageId.NewBuilder();
								if (HasGetLastMessageId())
								{
									subBuilder.MergeFrom(GetGetLastMessageId());
								}
								input.ReadMessage(subBuilder, extensionRegistry);
								SetGetLastMessageId(subBuilder.BuildPartial());
								subBuilder.Recycle();
								break;
							}
						case 242:
							{
								var subBuilder = CommandGetLastMessageIdResponse.NewBuilder();
								if (HasGetLastMessageIdResponse())
								{
									subBuilder.MergeFrom(GetGetLastMessageIdResponse());
								}
								input.ReadMessage(subBuilder, extensionRegistry);
								SetGetLastMessageIdResponse(subBuilder.BuildPartial());
								subBuilder.Recycle();
								break;
							}
						case 250:
							{
								var subBuilder = CommandActiveConsumerChange.NewBuilder();
								if (HasActiveConsumerChange())
								{
									subBuilder.MergeFrom(GetActiveConsumerChange());
								}
								input.ReadMessage(subBuilder, extensionRegistry);
								SetActiveConsumerChange(subBuilder.BuildPartial());
								subBuilder.Recycle();
								break;
							}
						case 258:
							{
								var subBuilder = CommandGetTopicsOfNamespace.NewBuilder();
								if (HasGetTopicsOfNamespace())
								{
									subBuilder.MergeFrom(GetGetTopicsOfNamespace());
								}
								input.ReadMessage(subBuilder, extensionRegistry);
								SetGetTopicsOfNamespace(subBuilder.BuildPartial());
								subBuilder.Recycle();
								break;
							}
						case 266:
							{
								var subBuilder = CommandGetTopicsOfNamespaceResponse.NewBuilder();
								if (HasGetTopicsOfNamespaceResponse())
								{
									subBuilder.MergeFrom(GetGetTopicsOfNamespaceResponse());
								}
								input.ReadMessage(subBuilder, extensionRegistry);
								SetGetTopicsOfNamespaceResponse(subBuilder.BuildPartial());
								subBuilder.Recycle();
								break;
							}
						case 274:
							{
								var subBuilder = CommandGetSchema.NewBuilder();
								if (HasGetSchema())
								{
									subBuilder.MergeFrom(GetGetSchema());
								}
								input.ReadMessage(subBuilder, extensionRegistry);
								SetGetSchema(subBuilder.BuildPartial());
								subBuilder.Recycle();
								break;
							}
						case 282:
							{
								var subBuilder = CommandGetSchemaResponse.NewBuilder();
								if (HasGetSchemaResponse())
								{
									subBuilder.MergeFrom(GetGetSchemaResponse());
								}
								input.ReadMessage(subBuilder, extensionRegistry);
								SetGetSchemaResponse(subBuilder.BuildPartial());
								subBuilder.Recycle();
								break;
							}
						case 290:
							{
								var subBuilder = CommandAuthChallenge.NewBuilder();
								if (HasAuthChallenge())
								{
									subBuilder.MergeFrom(GetAuthChallenge());
								}
								input.ReadMessage(subBuilder, extensionRegistry);
								SetAuthChallenge(subBuilder.BuildPartial());
								subBuilder.Recycle();
								break;
							}
						case 298:
							{
								var subBuilder = CommandAuthResponse.NewBuilder();
								if (HasAuthResponse())
								{
									subBuilder.MergeFrom(GetAuthResponse());
								}
								input.ReadMessage(subBuilder, extensionRegistry);
								SetAuthResponse(subBuilder.BuildPartial());
								subBuilder.Recycle();
								break;
							}
						case 306:
							{
								var subBuilder = CommandAckResponse.NewBuilder();
								if (HasAckResponse())
								{
									subBuilder.MergeFrom(GetAckResponse());
								}
								input.ReadMessage(subBuilder, extensionRegistry);
								SetAckResponse(subBuilder.BuildPartial());
								subBuilder.Recycle();
								break;
							}
						case 314:
							{
								var subBuilder = CommandGetOrCreateSchema.NewBuilder();
								if (HasGetOrCreateSchema())
								{
									subBuilder.MergeFrom(GetGetOrCreateSchema());
								}
								input.ReadMessage(subBuilder, extensionRegistry);
								SetGetOrCreateSchema(subBuilder.BuildPartial());
								subBuilder.Recycle();
								break;
							}
						case 322:
							{
								var subBuilder = CommandGetOrCreateSchemaResponse.NewBuilder();
								if (HasGetOrCreateSchemaResponse())
								{
									subBuilder.MergeFrom(GetGetOrCreateSchemaResponse());
								}
								input.ReadMessage(subBuilder, extensionRegistry);
								SetGetOrCreateSchemaResponse(subBuilder.BuildPartial());
								subBuilder.Recycle();
								break;
							}
						case 402:
							{
								var subBuilder = CommandNewTxn.NewBuilder();
								if (HasNewTxn())
								{
									subBuilder.MergeFrom(GetNewTxn());
								}
								input.ReadMessage(subBuilder, extensionRegistry);
								SetNewTxn(subBuilder.BuildPartial());
								subBuilder.Recycle();
								break;
							}
						case 410:
							{
								var subBuilder = CommandNewTxnResponse.NewBuilder();
								if (HasNewTxnResponse())
								{
									subBuilder.MergeFrom(GetNewTxnResponse());
								}
								input.ReadMessage(subBuilder, extensionRegistry);
								SetNewTxnResponse(subBuilder.BuildPartial());
								subBuilder.Recycle();
								break;
							}
						case 418:
							{
								var subBuilder = CommandAddPartitionToTxn.NewBuilder();
								if (HasAddPartitionToTxn())
								{
									subBuilder.MergeFrom(GetAddPartitionToTxn());
								}
								input.ReadMessage(subBuilder, extensionRegistry);
								SetAddPartitionToTxn(subBuilder.BuildPartial());
								subBuilder.Recycle();
								break;
							}
						case 426:
							{
								var subBuilder = CommandAddPartitionToTxnResponse.NewBuilder();
								if (HasAddPartitionToTxnResponse())
								{
									subBuilder.MergeFrom(GetAddPartitionToTxnResponse());
								}
								input.ReadMessage(subBuilder, extensionRegistry);
								SetAddPartitionToTxnResponse(subBuilder.BuildPartial());
								subBuilder.Recycle();
								break;
							}
						case 434:
							{
								var subBuilder = CommandAddSubscriptionToTxn.NewBuilder();
								if (HasAddSubscriptionToTxn())
								{
									subBuilder.MergeFrom(GetAddSubscriptionToTxn());
								}
								input.ReadMessage(subBuilder, extensionRegistry);
								SetAddSubscriptionToTxn(subBuilder.BuildPartial());
								subBuilder.Recycle();
								break;
							}
						case 442:
							{
								var subBuilder = CommandAddSubscriptionToTxnResponse.NewBuilder();
								if (HasAddSubscriptionToTxnResponse())
								{
									subBuilder.MergeFrom(GetAddSubscriptionToTxnResponse());
								}
								input.ReadMessage(subBuilder, extensionRegistry);
								SetAddSubscriptionToTxnResponse(subBuilder.BuildPartial());
								subBuilder.Recycle();
								break;
							}
						case 450:
							{
								var subBuilder = CommandEndTxn.NewBuilder();
								if (HasEndTxn())
								{
									subBuilder.MergeFrom(GetEndTxn());
								}
								input.ReadMessage(subBuilder, extensionRegistry);
								SetEndTxn(subBuilder.BuildPartial());
								subBuilder.Recycle();
								break;
							}
						case 458:
							{
								var subBuilder = CommandEndTxnResponse.NewBuilder();
								if (HasEndTxnResponse())
								{
									subBuilder.MergeFrom(GetEndTxnResponse());
								}
								input.ReadMessage(subBuilder, extensionRegistry);
								SetEndTxnResponse(subBuilder.BuildPartial());
								subBuilder.Recycle();
								break;
							}
						case 466:
							{
								var subBuilder = CommandEndTxnOnPartition.NewBuilder();
								if (HasEndTxnOnPartition())
								{
									subBuilder.MergeFrom(GetEndTxnOnPartition());
								}
								input.ReadMessage(subBuilder, extensionRegistry);
								SetEndTxnOnPartition(subBuilder.BuildPartial());
								subBuilder.Recycle();
								break;
							}
						case 474:
							{
								var subBuilder = CommandEndTxnOnPartitionResponse.NewBuilder();
								if (HasEndTxnOnPartitionResponse())
								{
									subBuilder.MergeFrom(GetEndTxnOnPartitionResponse());
								}
								input.ReadMessage(subBuilder, extensionRegistry);
								SetEndTxnOnPartitionResponse(subBuilder.BuildPartial());
								subBuilder.Recycle();
								break;
							}
						case 482:
							{
								var subBuilder = CommandEndTxnOnSubscription.NewBuilder();
								if (HasEndTxnOnSubscription())
								{
									subBuilder.MergeFrom(GetEndTxnOnSubscription());
								}
								input.ReadMessage(subBuilder, extensionRegistry);
								SetEndTxnOnSubscription(subBuilder.BuildPartial());
								subBuilder.Recycle();
								break;
							}
						case 490:
							{
								var subBuilder = CommandEndTxnOnSubscriptionResponse.NewBuilder();
								if (HasEndTxnOnSubscriptionResponse())
								{
									subBuilder.MergeFrom(GetEndTxnOnSubscriptionResponse());
								}
								input.ReadMessage(subBuilder, extensionRegistry);
								SetEndTxnOnSubscriptionResponse(subBuilder.BuildPartial());
								subBuilder.Recycle();
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

			public Builder SetType(Types.Type value)
			{
                _type = value;

				return this;
			}
			public Builder ClearType()
			{
				Clear();

				return this;
			}
			public bool HasType()
			{
				return ((BitField0 & 0x00000001) == 0x00000001);
			}
			public CommandConnect Connect = CommandConnect.DefaultInstance;
			public bool HasConnect()
			{
				return (((int)_type & 0x00000002) == 0x00000002);
			}
			public CommandConnect GetConnect()
			{
				return Connect;
			}
			public Builder SetConnect(CommandConnect value)
			{
                Connect = value ?? throw new NullReferenceException();

				BitField0 |= 0x00000002;
				return this;
			}
			public Builder SetConnect(CommandConnect.Builder builder)
			{
				Connect = builder.Build();

				BitField0 |= 0x00000002;
				return this;
			}
			public Builder MergeConnect(CommandConnect value)
			{
				if (((BitField0 & 0x00000002) == 0x00000002) && Connect != CommandConnect.DefaultInstance)
				{
					Connect = CommandConnect.NewBuilder(Connect).MergeFrom(value).BuildPartial();
				}
				else
				{
					Connect = value;
				}

				BitField0 |= 0x00000002;
				return this;
			}
			
			// optional .pulsar.proto.CommandConnected connected = 3;
			public CommandConnected Connected = CommandConnected.DefaultInstance;
			public bool HasConnected()
			{
				return ((BitField0 & 0x00000004) == 0x00000004);
			}
			public CommandConnected GetConnected()
			{
				return Connected;
			}
			public Builder GetConnected(CommandConnected value)
			{
                Connected = value ?? throw new NullReferenceException();

				BitField0 |= 0x00000004;
				return this;
			}
			public Builder SetConnected(CommandConnected.Builder builderForValue)
			{
				Connected = builderForValue.Build();
				BitField0 |= 0x00000004;
				return this;
			}
			public Builder MergeConnected(CommandConnected value)
			{
				if (((BitField0 & 0x00000004) == 0x00000004) && Connected != CommandConnected.DefaultInstance)
				{
					Connected = CommandConnected.NewBuilder(Connected).MergeFrom(value).BuildPartial();
				}
				else
				{
					Connected = value;
				}

				BitField0 |= 0x00000004;
				return this;
			}

			public Builder ClearConnect()
			{
				Connect = CommandConnect.DefaultInstance;

				BitField0 = (BitField0 & ~0x00000002);
				return this;
			}
			public Builder ClearConnected()
			{
				Connected = CommandConnected.DefaultInstance;

				BitField0 = (BitField0 & ~0x00000004);
				return this;
			}

			public CommandSubscribe Subscribe = CommandSubscribe.DefaultInstance;
			public bool HasSubscribe()
			{
				return ((BitField0 & 0x00000008) == 0x00000008);
			}
			public CommandSubscribe GetSubscribe()
			{
				return Subscribe;
			}
			public Builder SetSubscribe(CommandSubscribe value)
			{
                Subscribe = value ?? throw new NullReferenceException();

				BitField0 |= 0x00000008;
				return this;
			}
			public Builder SetSubscribe(CommandSubscribe.Builder builderForValue)
			{
				Subscribe = builderForValue.Build();

				BitField0 |= 0x00000008;
				return this;
			}
			public Builder MergeSubscribe(CommandSubscribe value)
			{
				if (((BitField0 & 0x00000008) == 0x00000008) && Subscribe != CommandSubscribe.DefaultInstance)
				{
					Subscribe = CommandSubscribe.NewBuilder(Subscribe).MergeFrom(value).BuildPartial();
				}
				else
				{
					Subscribe = value;
				}

				BitField0 |= 0x00000008;
				return this;
			}
			public Builder ClearSubscribe()
			{
				Subscribe = CommandSubscribe.DefaultInstance;

				BitField0 = (BitField0 & ~0x00000008);
				return this;
			}

			// optional .pulsar.proto.CommandProducer producer = 5;
			public CommandProducer Producer = CommandProducer.DefaultInstance;
			public bool HasProducer()
			{
				return ((BitField0 & 0x00000010) == 0x00000010);
			}
			public CommandProducer GetProducer()
			{
				return Producer;
			}
			public Builder SetProducer(CommandProducer value)
			{
                Producer = value ?? throw new NullReferenceException();

				BitField0 |= 0x00000010;
				return this;
			}
			public Builder SetProducer(CommandProducer.Builder builderForValue)
			{
				Producer = builderForValue.Build();

				BitField0 |= 0x00000010;
				return this;
			}
			public Builder MergeProducer(CommandProducer value)
			{
				if (((BitField0 & 0x00000010) == 0x00000010) && Producer != CommandProducer.DefaultInstance)
				{
					Producer = CommandProducer.NewBuilder(Producer).MergeFrom(value).BuildPartial();
				}
				else
				{
					Producer = value;
				}

				BitField0 |= 0x00000010;
				return this;
			}
			public Builder ClearProducer()
			{
				Producer = CommandProducer.DefaultInstance;

				BitField0 = (BitField0 & ~0x00000010);
				return this;
			}

			// optional .pulsar.proto.CommandSend send = 6;
			public CommandSend Send = CommandSend.DefaultInstance;
			public bool HasSend()
			{
				return ((BitField0 & 0x00000020) == 0x00000020);
			}
			public CommandSend GetSend()
			{
				return Send;
			}
			public Builder SetSend(CommandSend value)
			{
                Send = value ?? throw new NullReferenceException();

				BitField0 |= 0x00000020;
				return this;
			}
			public Builder SetSend(CommandSend.Builder builderForValue)
			{
				Send = builderForValue.Build();

				BitField0 |= 0x00000020;
				return this;
			}
			public Builder MergeSend(CommandSend value)
			{
				if (((BitField0 & 0x00000020) == 0x00000020) && Send != CommandSend.DefaultInstance)
				{
					Send = CommandSend.NewBuilder(Send).MergeFrom(value).BuildPartial();
				}
				else
				{
					Send = value;
				}

				BitField0 |= 0x00000020;
				return this;
			}
			public Builder ClearSend()
			{
				Send = CommandSend.DefaultInstance;

				BitField0 = (BitField0 & ~0x00000020);
				return this;
			}

			// optional .pulsar.proto.CommandSendReceipt send_receipt = 7;
			public CommandSendReceipt SendReceipt = CommandSendReceipt.DefaultInstance;
			public bool HasSendReceipt()
			{
				return ((BitField0 & 0x00000040) == 0x00000040);
			}
			public CommandSendReceipt GetSendReceipt()
			{
				return SendReceipt;
			}
			public Builder SetSendReceipt(CommandSendReceipt value)
			{
                SendReceipt = value ?? throw new NullReferenceException();

				BitField0 |= 0x00000040;
				return this;
			}
			public Builder SetSendReceipt(CommandSendReceipt.Builder builderForValue)
			{
				SendReceipt = builderForValue.Build();

				BitField0 |= 0x00000040;
				return this;
			}
			public Builder MergeSendReceipt(CommandSendReceipt value)
			{
				if (((BitField0 & 0x00000040) == 0x00000040) && SendReceipt != CommandSendReceipt.DefaultInstance)
				{
					SendReceipt = CommandSendReceipt.NewBuilder(SendReceipt).MergeFrom(value).BuildPartial();
				}
				else
				{
					SendReceipt = value;
				}

				BitField0 |= 0x00000040;
				return this;
			}
			public Builder ClearSendReceipt()
			{
				SendReceipt = CommandSendReceipt.DefaultInstance;

				BitField0 = (BitField0 & ~0x00000040);
				return this;
			}

			// optional .pulsar.proto.CommandSendError send_error = 8;
			public CommandSendError SendError = CommandSendError.DefaultInstance;
			public bool HasSendError()
			{
				return ((BitField0 & 0x00000080) == 0x00000080);
			}
			public CommandSendError GetSendError()
			{
				return SendError;
			}
			public Builder SetSendError(CommandSendError value)
			{
                SendError = value ?? throw new NullReferenceException();

				BitField0 |= 0x00000080;
				return this;
			}
			public Builder SetSendError(CommandSendError.Builder builderForValue)
			{
				SendError = builderForValue.Build();

				BitField0 |= 0x00000080;
				return this;
			}
			public Builder MergeSendError(CommandSendError value)
			{
				if (((BitField0 & 0x00000080) == 0x00000080) && SendError != CommandSendError.DefaultInstance)
				{
					SendError = CommandSendError.NewBuilder(SendError).MergeFrom(value).BuildPartial();
				}
				else
				{
					SendError = value;
				}

				BitField0 |= 0x00000080;
				return this;
			}
			public Builder ClearSendError()
			{
				SendError = CommandSendError.DefaultInstance;

				BitField0 = (BitField0 & ~0x00000080);
				return this;
			}

			// optional .pulsar.proto.CommandMessage message = 9;
			public CommandMessage Message = CommandMessage.DefaultInstance;
			public bool HasMessage()
			{
				return ((BitField0 & 0x00000100) == 0x00000100);
			}
			public CommandMessage GetMessage()
			{
				return Message;
			}
			public Builder SetMessage(CommandMessage value)
			{
                Message = value ?? throw new NullReferenceException();

				BitField0 |= 0x00000100;
				return this;
			}
			public Builder SetMessage(CommandMessage.Builder builderForValue)
			{
				Message = builderForValue.Build();

				BitField0 |= 0x00000100;
				return this;
			}
			public Builder MergeMessage(CommandMessage value)
			{
				if (((BitField0 & 0x00000100) == 0x00000100) && Message != CommandMessage.DefaultInstance)
				{
					Message = CommandMessage.NewBuilder(Message).MergeFrom(value).BuildPartial();
				}
				else
				{
					Message = value;
				}

				BitField0 |= 0x00000100;
				return this;
			}
			public Builder ClearMessage()
			{
				Message = CommandMessage.DefaultInstance;

				BitField0 = (BitField0 & ~0x00000100);
				return this;
			}

			// optional .pulsar.proto.CommandAck ack = 10;
			public CommandAck Ack = CommandAck.DefaultInstance;
			public bool HasAck()
			{
				return ((BitField0 & 0x00000200) == 0x00000200);
			}
			public CommandAck GetAck()
			{
				return Ack;
			}
			public Builder SetAck(CommandAck value)
			{
                Ack = value ?? throw new NullReferenceException();

				BitField0 |= 0x00000200;
				return this;
			}
			public Builder SetAck(CommandAck.Builder builderForValue)
			{
				Ack = builderForValue.Build();

				BitField0 |= 0x00000200;
				return this;
			}
			public Builder MergeAck(CommandAck value)
			{
				if (((BitField0 & 0x00000200) == 0x00000200) && Ack != CommandAck.DefaultInstance)
				{
					Ack = CommandAck.NewBuilder(Ack).MergeFrom(value).BuildPartial();
				}
				else
				{
					Ack = value;
				}

				BitField0 |= 0x00000200;
				return this;
			}
			public Builder ClearAck()
			{
				Ack = CommandAck.DefaultInstance;

				BitField0 = (BitField0 & ~0x00000200);
				return this;
			}

			// optional .pulsar.proto.CommandFlow flow = 11;
			public CommandFlow Flow = CommandFlow.DefaultInstance;
			public bool HasFlow()
			{
				return ((BitField0 & 0x00000400) == 0x00000400);
			}
			public CommandFlow GetFlow()
			{
				return Flow;
			}
			public Builder SetFlow(CommandFlow value)
			{
                Flow = value ?? throw new NullReferenceException();

				BitField0 |= 0x00000400;
				return this;
			}
			public Builder SetFlow(CommandFlow.Builder builderForValue)
			{
				Flow = builderForValue.Build();

				BitField0 |= 0x00000400;
				return this;
			}
			public Builder MergeFlow(CommandFlow value)
			{
				if (((BitField0 & 0x00000400) == 0x00000400) && Flow != CommandFlow.DefaultInstance)
				{
					Flow = CommandFlow.NewBuilder(Flow).MergeFrom(value).BuildPartial();
				}
				else
				{
					Flow = value;
				}

				BitField0 |= 0x00000400;
				return this;
			}
			public Builder ClearFlow()
			{
				Flow = CommandFlow.DefaultInstance;

				BitField0 = (BitField0 & ~0x00000400);
				return this;
			}

			// optional .pulsar.proto.CommandUnsubscribe unsubscribe = 12;
			public CommandUnsubscribe Unsubscribe = CommandUnsubscribe.DefaultInstance;
			public bool HasUnsubscribe()
			{
				return ((BitField0 & 0x00000800) == 0x00000800);
			}
			public CommandUnsubscribe GetUnsubscribe()
			{
				return Unsubscribe;
			}
			public Builder SetUnsubscribe(CommandUnsubscribe value)
			{
                Unsubscribe = value ?? throw new NullReferenceException();

				BitField0 |= 0x00000800;
				return this;
			}
			public Builder SetUnsubscribe(CommandUnsubscribe.Builder builderForValue)
			{
				Unsubscribe = builderForValue.Build();

				BitField0 |= 0x00000800;
				return this;
			}
			public Builder MergeUnsubscribe(CommandUnsubscribe value)
			{
				if (((BitField0 & 0x00000800) == 0x00000800) && Unsubscribe != CommandUnsubscribe.DefaultInstance)
				{
					Unsubscribe = CommandUnsubscribe.NewBuilder(Unsubscribe).MergeFrom(value).BuildPartial();
				}
				else
				{
					Unsubscribe = value;
				}

				BitField0 |= 0x00000800;
				return this;
			}
			public Builder ClearUnsubscribe()
			{
				Unsubscribe = CommandUnsubscribe.DefaultInstance;

				BitField0 = (BitField0 & ~0x00000800);
				return this;
			}

			// optional .pulsar.proto.CommandSuccess success = 13;
			public CommandSuccess Success = CommandSuccess.DefaultInstance;
			public bool HasSuccess()
			{
				return ((BitField0 & 0x00001000) == 0x00001000);
			}
			public CommandSuccess GetSuccess()
			{
				return Success;
			}
			public Builder SetSuccess(CommandSuccess value)
			{
                Success = value ?? throw new NullReferenceException();

				BitField0 |= 0x00001000;
				return this;
			}
			public Builder SetSuccess(CommandSuccess.Builder builderForValue)
			{
				Success = builderForValue.Build();

				BitField0 |= 0x00001000;
				return this;
			}
			public Builder MergeSuccess(CommandSuccess value)
			{
				if (((BitField0 & 0x00001000) == 0x00001000) && Success != CommandSuccess.DefaultInstance)
				{
					Success = CommandSuccess.NewBuilder(Success).MergeFrom(value).BuildPartial();
				}
				else
				{
					Success = value;
				}

				BitField0 |= 0x00001000;
				return this;
			}
			public Builder ClearSuccess()
			{
				Success = CommandSuccess.DefaultInstance;

				BitField0 = (BitField0 & ~0x00001000);
				return this;
			}

			// optional .pulsar.proto.CommandError error = 14;
			public CommandError Error = CommandError.DefaultInstance;
			public bool HasError()
			{
				return ((BitField0 & 0x00002000) == 0x00002000);
			}
			public CommandError GetError()
			{
				return Error;
			}
			public Builder SetError(CommandError value)
			{
                Error = value ?? throw new NullReferenceException();

				BitField0 |= 0x00002000;
				return this;
			}
			public Builder SetError(CommandError.Builder builderForValue)
			{
				Error = builderForValue.Build();

				BitField0 |= 0x00002000;
				return this;
			}
			public Builder MergeError(CommandError value)
			{
				if (((BitField0 & 0x00002000) == 0x00002000) && Error != CommandError.DefaultInstance)
				{
					Error = CommandError.NewBuilder(Error).MergeFrom(value).BuildPartial();
				}
				else
				{
					Error = value;
				}

				BitField0 |= 0x00002000;
				return this;
			}
			public Builder ClearError()
			{
				Error = CommandError.DefaultInstance;

				BitField0 = (BitField0 & ~0x00002000);
				return this;
			}

			// optional .pulsar.proto.CommandCloseProducer close_producer = 15;
			public CommandCloseProducer CloseProducer = CommandCloseProducer.DefaultInstance;
			public bool HasCloseProducer()
			{
				return ((BitField0 & 0x00004000) == 0x00004000);
			}
			public CommandCloseProducer GetCloseProducer()
			{
				return CloseProducer;
			}
			public Builder SetCloseProducer(CommandCloseProducer value)
			{
                CloseProducer = value ?? throw new NullReferenceException();

				BitField0 |= 0x00004000;
				return this;
			}
			public Builder SetCloseProducer(CommandCloseProducer.Builder builderForValue)
			{
				CloseProducer = builderForValue.Build();

				BitField0 |= 0x00004000;
				return this;
			}
			public Builder MergeCloseProducer(CommandCloseProducer value)
			{
				if (((BitField0 & 0x00004000) == 0x00004000) && CloseProducer != CommandCloseProducer.DefaultInstance)
				{
					CloseProducer = CommandCloseProducer.NewBuilder(CloseProducer).MergeFrom(value).BuildPartial();
				}
				else
				{
					CloseProducer = value;
				}

				BitField0 |= 0x00004000;
				return this;
			}
			public Builder ClearCloseProducer()
			{
				CloseProducer = CommandCloseProducer.DefaultInstance;

				BitField0 = (BitField0 & ~0x00004000);
				return this;
			}

			// optional .pulsar.proto.CommandCloseConsumer close_consumer = 16;
			public CommandCloseConsumer CloseConsumer = CommandCloseConsumer.DefaultInstance;
			public bool HasCloseConsumer()
			{
				return ((BitField0 & 0x00008000) == 0x00008000);
			}
			public CommandCloseConsumer GetCloseConsumer()
			{
				return CloseConsumer;
			}
			public Builder SetCloseConsumer(CommandCloseConsumer value)
			{
                CloseConsumer = value ?? throw new NullReferenceException();

				BitField0 |= 0x00008000;
				return this;
			}
			public Builder SetCloseConsumer(CommandCloseConsumer.Builder builderForValue)
			{
				CloseConsumer = builderForValue.Build();

				BitField0 |= 0x00008000;
				return this;
			}
			public Builder MergeCloseConsumer(CommandCloseConsumer value)
			{
				if (((BitField0 & 0x00008000) == 0x00008000) && CloseConsumer != CommandCloseConsumer.DefaultInstance)
				{
					CloseConsumer = CommandCloseConsumer.NewBuilder(CloseConsumer).MergeFrom(value).BuildPartial();
				}
				else
				{
					CloseConsumer = value;
				}

				BitField0 |= 0x00008000;
				return this;
			}
			public Builder ClearCloseConsumer()
			{
				CloseConsumer = CommandCloseConsumer.DefaultInstance;

				BitField0 = (BitField0 & ~0x00008000);
				return this;
			}

			// optional .pulsar.proto.CommandProducerSuccess producer_success = 17;
			public CommandProducerSuccess ProducerSuccess = CommandProducerSuccess.DefaultInstance;
			public bool HasProducerSuccess()
			{
				return ((BitField0 & 0x00010000) == 0x00010000);
			}
			public CommandProducerSuccess GetProducerSuccess()
			{
				return ProducerSuccess;
			}
			public Builder SetProducerSuccess(CommandProducerSuccess value)
			{
                ProducerSuccess = value ?? throw new NullReferenceException();

				BitField0 |= 0x00010000;
				return this;
			}
			public Builder SetProducerSuccess(CommandProducerSuccess.Builder builderForValue)
			{
				ProducerSuccess = builderForValue.Build();

				BitField0 |= 0x00010000;
				return this;
			}
			public Builder MergeProducerSuccess(CommandProducerSuccess value)
			{
				if (((BitField0 & 0x00010000) == 0x00010000) && ProducerSuccess != CommandProducerSuccess.DefaultInstance)
				{
					ProducerSuccess = CommandProducerSuccess.NewBuilder(ProducerSuccess).MergeFrom(value).BuildPartial();
				}
				else
				{
					ProducerSuccess = value;
				}

				BitField0 |= 0x00010000;
				return this;
			}
			public Builder ClearProducerSuccess()
			{
				ProducerSuccess = CommandProducerSuccess.DefaultInstance;

				BitField0 = (BitField0 & ~0x00010000);
				return this;
			}

			// optional .pulsar.proto.CommandPing ping = 18;
			public CommandPing Ping = CommandPing.DefaultInstance;
			public bool HasPing()
			{
				return ((BitField0 & 0x00020000) == 0x00020000);
			}
			public CommandPing GetPing()
			{
				return Ping;
			}
			public Builder SetPing(CommandPing value)
			{
                Ping = value ?? throw new NullReferenceException();

				BitField0 |= 0x00020000;
				return this;
			}
			public Builder SetPing(CommandPing.Builder builderForValue)
			{
				Ping = builderForValue.Build();

				BitField0 |= 0x00020000;
				return this;
			}
			public Builder MergePing(CommandPing value)
			{
				if (((BitField0 & 0x00020000) == 0x00020000) && Ping != CommandPing.DefaultInstance)
				{
					Ping = CommandPing.NewBuilder(Ping).MergeFrom(value).BuildPartial();
				}
				else
				{
					Ping = value;
				}

				BitField0 |= 0x00020000;
				return this;
			}
			public Builder ClearPing()
			{
				Ping = CommandPing.DefaultInstance;

				BitField0 = (BitField0 & ~0x00020000);
				return this;
			}

			// optional .pulsar.proto.CommandPong pong = 19;
			public CommandPong Pong = CommandPong.DefaultInstance;
			public bool HasPong()
			{
				return ((BitField0 & 0x00040000) == 0x00040000);
			}
			public CommandPong GetPong()
			{
				return Pong;
			}
			public Builder SetPong(CommandPong value)
			{
                Pong = value ?? throw new NullReferenceException();

				BitField0 |= 0x00040000;
				return this;
			}
			public Builder SetPong(CommandPong.Builder builderForValue)
			{
				Pong = builderForValue.Build();

				BitField0 |= 0x00040000;
				return this;
			}
			public Builder MergePong(CommandPong value)
			{
				if (((BitField0 & 0x00040000) == 0x00040000) && Pong != CommandPong.DefaultInstance)
				{
					Pong = CommandPong.NewBuilder(Pong).MergeFrom(value).BuildPartial();
				}
				else
				{
					Pong = value;
				}

				BitField0 |= 0x00040000;
				return this;
			}
			public Builder ClearPong()
			{
				Pong = CommandPong.DefaultInstance;

				BitField0 = (BitField0 & ~0x00040000);
				return this;
			}

			// optional .pulsar.proto.CommandRedeliverUnacknowledgedMessages redeliverUnacknowledgedMessages = 20;
			public CommandRedeliverUnacknowledgedMessages RedeliverUnacknowledgedMessages = CommandRedeliverUnacknowledgedMessages.DefaultInstance;
			public bool HasRedeliverUnacknowledgedMessages()
			{
				return ((BitField0 & 0x00080000) == 0x00080000);
			}
			public CommandRedeliverUnacknowledgedMessages GetRedeliverUnacknowledgedMessages()
			{
				return RedeliverUnacknowledgedMessages;
			}
			public Builder SetRedeliverUnacknowledgedMessages(CommandRedeliverUnacknowledgedMessages value)
			{
                RedeliverUnacknowledgedMessages = value ?? throw new NullReferenceException();

				BitField0 |= 0x00080000;
				return this;
			}
			public Builder SetRedeliverUnacknowledgedMessages(CommandRedeliverUnacknowledgedMessages.Builder builderForValue)
			{
				RedeliverUnacknowledgedMessages = builderForValue.Build();

				BitField0 |= 0x00080000;
				return this;
			}
			public Builder MergeRedeliverUnacknowledgedMessages(CommandRedeliverUnacknowledgedMessages value)
			{
				if (((BitField0 & 0x00080000) == 0x00080000) && RedeliverUnacknowledgedMessages != CommandRedeliverUnacknowledgedMessages.DefaultInstance)
				{
					RedeliverUnacknowledgedMessages = CommandRedeliverUnacknowledgedMessages.NewBuilder(RedeliverUnacknowledgedMessages).MergeFrom(value).BuildPartial();
				}
				else
				{
					RedeliverUnacknowledgedMessages = value;
				}

				BitField0 |= 0x00080000;
				return this;
			}
			public Builder ClearRedeliverUnacknowledgedMessages()
			{
				RedeliverUnacknowledgedMessages = CommandRedeliverUnacknowledgedMessages.DefaultInstance;

				BitField0 = (BitField0 & ~0x00080000);
				return this;
			}

			// optional .pulsar.proto.CommandPartitionedTopicMetadata partitionMetadata = 21;
			public CommandPartitionedTopicMetadata PartitionMetadata = CommandPartitionedTopicMetadata.DefaultInstance;
			public bool HasPartitionMetadata()
			{
				return ((BitField0 & 0x00100000) == 0x00100000);
			}
			public CommandPartitionedTopicMetadata GetPartitionMetadata()
			{
				return PartitionMetadata;
			}
			public Builder SetPartitionMetadata(CommandPartitionedTopicMetadata value)
			{
                PartitionMetadata = value ?? throw new NullReferenceException();

				BitField0 |= 0x00100000;
				return this;
			}
			public Builder SetPartitionMetadata(CommandPartitionedTopicMetadata.Builder builderForValue)
			{
				PartitionMetadata = builderForValue.Build();

				BitField0 |= 0x00100000;
				return this;
			}
			public Builder MergePartitionMetadata(CommandPartitionedTopicMetadata value)
			{
				if (((BitField0 & 0x00100000) == 0x00100000) && PartitionMetadata != CommandPartitionedTopicMetadata.DefaultInstance)
				{
					PartitionMetadata = CommandPartitionedTopicMetadata.NewBuilder(PartitionMetadata).MergeFrom(value).BuildPartial();
				}
				else
				{
					PartitionMetadata = value;
				}

				BitField0 |= 0x00100000;
				return this;
			}
			public Builder ClearPartitionMetadata()
			{
				PartitionMetadata = CommandPartitionedTopicMetadata.DefaultInstance;

				BitField0 = (BitField0 & ~0x00100000);
				return this;
			}

			// optional .pulsar.proto.CommandPartitionedTopicMetadataResponse partitionMetadataResponse = 22;
			public CommandPartitionedTopicMetadataResponse PartitionMetadataResponse = CommandPartitionedTopicMetadataResponse.DefaultInstance;
			public bool HasPartitionMetadataResponse()
			{
				return ((BitField0 & 0x00200000) == 0x00200000);
			}
			public CommandPartitionedTopicMetadataResponse GetPartitionMetadataResponse()
			{
				return PartitionMetadataResponse;
			}
			public Builder SetPartitionMetadataResponse(CommandPartitionedTopicMetadataResponse value)
			{
                PartitionMetadataResponse = value ?? throw new NullReferenceException();

				BitField0 |= 0x00200000;
				return this;
			}
			public Builder SetPartitionMetadataResponse(CommandPartitionedTopicMetadataResponse.Builder builderForValue)
			{
				PartitionMetadataResponse = builderForValue.Build();

				BitField0 |= 0x00200000;
				return this;
			}
			public Builder MergePartitionMetadataResponse(CommandPartitionedTopicMetadataResponse value)
			{
				if (((BitField0 & 0x00200000) == 0x00200000) && PartitionMetadataResponse != CommandPartitionedTopicMetadataResponse.DefaultInstance)
				{
					PartitionMetadataResponse = CommandPartitionedTopicMetadataResponse.NewBuilder(PartitionMetadataResponse).MergeFrom(value).BuildPartial();
				}
				else
				{
					PartitionMetadataResponse = value;
				}

				BitField0 |= 0x00200000;
				return this;
			}
			public Builder ClearPartitionMetadataResponse()
			{
				PartitionMetadataResponse = CommandPartitionedTopicMetadataResponse.DefaultInstance;

				BitField0 = (BitField0 & ~0x00200000);
				return this;
			}

			// optional .pulsar.proto.CommandLookupTopic lookupTopic = 23;
			public CommandLookupTopic LookupTopic = CommandLookupTopic.DefaultInstance;
			public bool HasLookupTopic()
			{
				return ((BitField0 & 0x00400000) == 0x00400000);
			}
			public CommandLookupTopic GetLookupTopic()
			{
				return LookupTopic;
			}
			public Builder SetLookupTopic(CommandLookupTopic value)
			{
                LookupTopic = value ?? throw new NullReferenceException();

				BitField0 |= 0x00400000;
				return this;
			}
			public Builder SetLookupTopic(CommandLookupTopic.Builder builderForValue)
			{
				LookupTopic = builderForValue.Build();

				BitField0 |= 0x00400000;
				return this;
			}
			public Builder MergeLookupTopic(CommandLookupTopic value)
			{
				if (((BitField0 & 0x00400000) == 0x00400000) && LookupTopic != CommandLookupTopic.DefaultInstance)
				{
					LookupTopic = CommandLookupTopic.NewBuilder(LookupTopic).MergeFrom(value).BuildPartial();
				}
				else
				{
					LookupTopic = value;
				}

				BitField0 |= 0x00400000;
				return this;
			}
			public Builder ClearLookupTopic()
			{
				LookupTopic = CommandLookupTopic.DefaultInstance;

				BitField0 = (BitField0 & ~0x00400000);
				return this;
			}

			// optional .pulsar.proto.CommandLookupTopicResponse lookupTopicResponse = 24;
			public CommandLookupTopicResponse LookupTopicResponse = CommandLookupTopicResponse.DefaultInstance;
			public bool HasLookupTopicResponse()
			{
				return ((BitField0 & 0x00800000) == 0x00800000);
			}
			public CommandLookupTopicResponse GetLookupTopicResponse()
			{
				return LookupTopicResponse;
			}
			public Builder SetLookupTopicResponse(CommandLookupTopicResponse value)
			{
                LookupTopicResponse = value ?? throw new NullReferenceException();

				BitField0 |= 0x00800000;
				return this;
			}
			public Builder SetLookupTopicResponse(CommandLookupTopicResponse.Builder builderForValue)
			{
				LookupTopicResponse = builderForValue.Build();

				BitField0 |= 0x00800000;
				return this;
			}
			public Builder MergeLookupTopicResponse(CommandLookupTopicResponse value)
			{
				if (((BitField0 & 0x00800000) == 0x00800000) && LookupTopicResponse != CommandLookupTopicResponse.DefaultInstance)
				{
					LookupTopicResponse = CommandLookupTopicResponse.NewBuilder(LookupTopicResponse).MergeFrom(value).BuildPartial();
				}
				else
				{
					LookupTopicResponse = value;
				}

				BitField0 |= 0x00800000;
				return this;
			}
			public Builder ClearLookupTopicResponse()
			{
				LookupTopicResponse = CommandLookupTopicResponse.DefaultInstance;

				BitField0 = (BitField0 & ~0x00800000);
				return this;
			}

			// optional .pulsar.proto.CommandConsumerStats consumerStats = 25;
			public CommandConsumerStats ConsumerStats = CommandConsumerStats.DefaultInstance;
			public bool HasConsumerStats()
			{
				return ((BitField0 & 0x01000000) == 0x01000000);
			}
			public CommandConsumerStats GetConsumerStats()
			{
				return ConsumerStats;
			}
			public Builder SetConsumerStats(CommandConsumerStats value)
			{
                ConsumerStats = value ?? throw new NullReferenceException();

				BitField0 |= 0x01000000;
				return this;
			}
			public Builder SetConsumerStats(CommandConsumerStats.Builder builderForValue)
			{
				ConsumerStats = builderForValue.Build();

				BitField0 |= 0x01000000;
				return this;
			}
			public Builder MergeConsumerStats(CommandConsumerStats value)
			{
				if (((BitField0 & 0x01000000) == 0x01000000) && ConsumerStats != CommandConsumerStats.DefaultInstance)
				{
					ConsumerStats = CommandConsumerStats.NewBuilder(ConsumerStats).MergeFrom(value).BuildPartial();
				}
				else
				{
					ConsumerStats = value;
				}

				BitField0 |= 0x01000000;
				return this;
			}
			public Builder ClearConsumerStats()
			{
				ConsumerStats = CommandConsumerStats.DefaultInstance;

				BitField0 = (BitField0 & ~0x01000000);
				return this;
			}

			// optional .pulsar.proto.CommandConsumerStatsResponse consumerStatsResponse = 26;
			public CommandConsumerStatsResponse ConsumerStatsResponse = CommandConsumerStatsResponse.DefaultInstance;
			public bool HasConsumerStatsResponse()
			{
				return ((BitField0 & 0x02000000) == 0x02000000);
			}
			public CommandConsumerStatsResponse GetConsumerStatsResponse()
			{
				return ConsumerStatsResponse;
			}
			public Builder SetConsumerStatsResponse(CommandConsumerStatsResponse value)
			{
                ConsumerStatsResponse = value ?? throw new NullReferenceException();

				BitField0 |= 0x02000000;
				return this;
			}
			public Builder SetConsumerStatsResponse(CommandConsumerStatsResponse.Builder builderForValue)
			{
				ConsumerStatsResponse = builderForValue.Build();

				BitField0 |= 0x02000000;
				return this;
			}
			public Builder MergeConsumerStatsResponse(CommandConsumerStatsResponse value)
			{
				if (((BitField0 & 0x02000000) == 0x02000000) && ConsumerStatsResponse != CommandConsumerStatsResponse.DefaultInstance)
				{
					ConsumerStatsResponse = CommandConsumerStatsResponse.NewBuilder(ConsumerStatsResponse).MergeFrom(value).BuildPartial();
				}
				else
				{
					ConsumerStatsResponse = value;
				}

				BitField0 |= 0x02000000;
				return this;
			}
			public Builder ClearConsumerStatsResponse()
			{
				ConsumerStatsResponse = CommandConsumerStatsResponse.DefaultInstance;

				BitField0 = (BitField0 & ~0x02000000);
				return this;
			}

			// optional .pulsar.proto.CommandReachedEndOfTopic reachedEndOfTopic = 27;
			public CommandReachedEndOfTopic ReachedEndOfTopic = CommandReachedEndOfTopic.DefaultInstance;
			public bool HasReachedEndOfTopic()
			{
				return ((BitField0 & 0x04000000) == 0x04000000);
			}
			public CommandReachedEndOfTopic GetReachedEndOfTopic()
			{
				return ReachedEndOfTopic;
			}
			public Builder SetReachedEndOfTopic(CommandReachedEndOfTopic value)
			{
                ReachedEndOfTopic = value ?? throw new NullReferenceException();

				BitField0 |= 0x04000000;
				return this;
			}
			public Builder SetReachedEndOfTopic(CommandReachedEndOfTopic.Builder builderForValue)
			{
				ReachedEndOfTopic = builderForValue.Build();

				BitField0 |= 0x04000000;
				return this;
			}
			public Builder MergeReachedEndOfTopic(CommandReachedEndOfTopic value)
			{
				if (((BitField0 & 0x04000000) == 0x04000000) && ReachedEndOfTopic != CommandReachedEndOfTopic.DefaultInstance)
				{
					ReachedEndOfTopic = CommandReachedEndOfTopic.NewBuilder(ReachedEndOfTopic).MergeFrom(value).BuildPartial();
				}
				else
				{
					ReachedEndOfTopic = value;
				}

				BitField0 |= 0x04000000;
				return this;
			}
			public Builder ClearReachedEndOfTopic()
			{
				ReachedEndOfTopic = CommandReachedEndOfTopic.DefaultInstance;

				BitField0 = (BitField0 & ~0x04000000);
				return this;
			}

			// optional .pulsar.proto.CommandSeek seek = 28;
			public CommandSeek Seek = CommandSeek.DefaultInstance;
			public bool HasSeek()
			{
				return ((BitField0 & 0x08000000) == 0x08000000);
			}
			public CommandSeek GetSeek()
			{
				return Seek;
			}
			public Builder SetSeek(CommandSeek value)
			{
                Seek = value ?? throw new NullReferenceException();

				BitField0 |= 0x08000000;
				return this;
			}
			public Builder SetSeek(CommandSeek.Builder builderForValue)
			{
				Seek = builderForValue.Build();

				BitField0 |= 0x08000000;
				return this;
			}
			public Builder MergeSeek(CommandSeek value)
			{
				if (((BitField0 & 0x08000000) == 0x08000000) && Seek != CommandSeek.DefaultInstance)
				{
					Seek = CommandSeek.NewBuilder(Seek).MergeFrom(value).BuildPartial();
				}
				else
				{
					Seek = value;
				}

				BitField0 |= 0x08000000;
				return this;
			}
			public Builder ClearSeek()
			{
				Seek = CommandSeek.DefaultInstance;

				BitField0 = (BitField0 & ~0x08000000);
				return this;
			}

			// optional .pulsar.proto.CommandGetLastMessageId GetLastMessageId = 29;
			public CommandGetLastMessageId GetLastMessageId = CommandGetLastMessageId.DefaultInstance;
			public bool HasGetLastMessageId()
			{
				return ((BitField0 & 0x10000000) == 0x10000000);
			}
			public CommandGetLastMessageId GetGetLastMessageId()
			{
				return GetLastMessageId;
			}
			public Builder SetGetLastMessageId(CommandGetLastMessageId value)
			{
                GetLastMessageId = value ?? throw new NullReferenceException();

				BitField0 |= 0x10000000;
				return this;
			}
			public Builder SetGetLastMessageId(CommandGetLastMessageId.Builder builderForValue)
			{
				GetLastMessageId = builderForValue.Build();

				BitField0 |= 0x10000000;
				return this;
			}
			public Builder MergeGetLastMessageId(CommandGetLastMessageId value)
			{
				if (((BitField0 & 0x10000000) == 0x10000000) && GetLastMessageId != CommandGetLastMessageId.DefaultInstance)
				{
					GetLastMessageId = CommandGetLastMessageId.NewBuilder(GetLastMessageId).MergeFrom(value).BuildPartial();
				}
				else
				{
					GetLastMessageId = value;
				}

				BitField0 |= 0x10000000;
				return this;
			}
			public Builder ClearGetLastMessageId()
			{
				GetLastMessageId = CommandGetLastMessageId.DefaultInstance;

				BitField0 = (BitField0 & ~0x10000000);
				return this;
			}

			// optional .pulsar.proto.CommandGetLastMessageIdResponse GetLastMessageIdResponse = 30;
			public CommandGetLastMessageIdResponse GetLastMessageIdResponse = CommandGetLastMessageIdResponse.DefaultInstance;
			public bool HasGetLastMessageIdResponse()
			{
				return ((BitField0 & 0x20000000) == 0x20000000);
			}
			public CommandGetLastMessageIdResponse GetGetLastMessageIdResponse()
			{
				return GetLastMessageIdResponse;
			}
			public Builder SetGetLastMessageIdResponse(CommandGetLastMessageIdResponse value)
			{
                GetLastMessageIdResponse = value ?? throw new NullReferenceException();

				BitField0 |= 0x20000000;
				return this;
			}
			public Builder SetGetLastMessageIdResponse(CommandGetLastMessageIdResponse.Builder builderForValue)
			{
				GetLastMessageIdResponse = builderForValue.Build();

				BitField0 |= 0x20000000;
				return this;
			}
			public Builder MergeGetLastMessageIdResponse(CommandGetLastMessageIdResponse value)
			{
				if (((BitField0 & 0x20000000) == 0x20000000) && GetLastMessageIdResponse != CommandGetLastMessageIdResponse.DefaultInstance)
				{
					GetLastMessageIdResponse = CommandGetLastMessageIdResponse.NewBuilder(GetLastMessageIdResponse).MergeFrom(value).BuildPartial();
				}
				else
				{
					GetLastMessageIdResponse = value;
				}

				BitField0 |= 0x20000000;
				return this;
			}
			public Builder ClearGetLastMessageIdResponse()
			{
				GetLastMessageIdResponse = CommandGetLastMessageIdResponse.DefaultInstance;

				BitField0 = (BitField0 & ~0x20000000);
				return this;
			}

			// optional .pulsar.proto.CommandActiveConsumerChange active_consumer_change = 31;
			public CommandActiveConsumerChange ActiveConsumerChange = CommandActiveConsumerChange.DefaultInstance;
			public bool HasActiveConsumerChange()
			{
				return ((BitField0 & 0x40000000) == 0x40000000);
			}
			public CommandActiveConsumerChange GetActiveConsumerChange()
			{
				return ActiveConsumerChange;
			}
			public Builder SetActiveConsumerChange(CommandActiveConsumerChange value)
			{
                ActiveConsumerChange = value ?? throw new NullReferenceException();

				BitField0 |= 0x40000000;
				return this;
			}
			public Builder SetActiveConsumerChange(CommandActiveConsumerChange.Builder builderForValue)
			{
				ActiveConsumerChange = builderForValue.Build();

				BitField0 |= 0x40000000;
				return this;
			}
			public Builder MergeActiveConsumerChange(CommandActiveConsumerChange value)
			{
				if (((BitField0 & 0x40000000) == 0x40000000) && ActiveConsumerChange != CommandActiveConsumerChange.DefaultInstance)
				{
					ActiveConsumerChange = CommandActiveConsumerChange.NewBuilder(ActiveConsumerChange).MergeFrom(value).BuildPartial();
				}
				else
				{
					ActiveConsumerChange = value;
				}

				BitField0 |= 0x40000000;
				return this;
			}
			public Builder ClearActiveConsumerChange()
			{
				ActiveConsumerChange = CommandActiveConsumerChange.DefaultInstance;

				BitField0 = (BitField0 & ~0x40000000);
				return this;
			}

			// optional .pulsar.proto.CommandGetTopicsOfNamespace GetTopicsOfNamespace = 32;
			public CommandGetTopicsOfNamespace GetTopicsOfNamespace = CommandGetTopicsOfNamespace.DefaultInstance;
			public bool HasGetTopicsOfNamespace()
			{
				return ((BitField0 & 0x80000000) == 0x80000000);
			}
			public CommandGetTopicsOfNamespace GetGetTopicsOfNamespace()
			{
				return GetTopicsOfNamespace;
			}
			public Builder SetGetTopicsOfNamespace(CommandGetTopicsOfNamespace value)
			{
                GetTopicsOfNamespace = value ?? throw new NullReferenceException();

				BitField0 |= unchecked((int)0x80000000);
				return this;
			}
			public Builder SetGetTopicsOfNamespace(CommandGetTopicsOfNamespace.Builder builderForValue)
			{
				GetTopicsOfNamespace = builderForValue.Build();

				BitField0 |= unchecked((int)0x80000000);
				return this;
			}
			public Builder MergeGetTopicsOfNamespace(CommandGetTopicsOfNamespace value)
			{
				if (((BitField0 & 0x80000000) == 0x80000000) && GetTopicsOfNamespace != CommandGetTopicsOfNamespace.DefaultInstance)
				{
					GetTopicsOfNamespace = CommandGetTopicsOfNamespace.NewBuilder(GetTopicsOfNamespace).MergeFrom(value).BuildPartial();
				}
				else
				{
					GetTopicsOfNamespace = value;
				}

				BitField0 |= unchecked((int)0x80000000);
				return this;
			}
			public Builder ClearGetTopicsOfNamespace()
			{
				GetTopicsOfNamespace = CommandGetTopicsOfNamespace.DefaultInstance;

				BitField0 = (BitField0 & ~unchecked((int)0x80000000));
				return this;
			}

			// optional .pulsar.proto.CommandGetTopicsOfNamespaceResponse GetTopicsOfNamespaceResponse = 33;
			public CommandGetTopicsOfNamespaceResponse GetTopicsOfNamespaceResponse = CommandGetTopicsOfNamespaceResponse.DefaultInstance;
			public bool HasGetTopicsOfNamespaceResponse()
			{
				return ((BitField0 & 0x00000001) == 0x00000001);
			}
			public CommandGetTopicsOfNamespaceResponse GetGetTopicsOfNamespaceResponse()
			{
				return GetTopicsOfNamespaceResponse;
			}
			public Builder SetGetTopicsOfNamespaceResponse(CommandGetTopicsOfNamespaceResponse value)
			{
                GetTopicsOfNamespaceResponse = value ?? throw new NullReferenceException();

				BitField0 |= 0x00000001;
				return this;
			}
			public Builder SetGetTopicsOfNamespaceResponse(CommandGetTopicsOfNamespaceResponse.Builder builderForValue)
			{
				GetTopicsOfNamespaceResponse = builderForValue.Build();

				BitField0 |= 0x00000001;
				return this;
			}
			public Builder MergeGetTopicsOfNamespaceResponse(CommandGetTopicsOfNamespaceResponse value)
			{
				if (((BitField0 & 0x00000001) == 0x00000001) && GetTopicsOfNamespaceResponse != CommandGetTopicsOfNamespaceResponse.DefaultInstance)
				{
					GetTopicsOfNamespaceResponse = CommandGetTopicsOfNamespaceResponse.NewBuilder(GetTopicsOfNamespaceResponse).MergeFrom(value).BuildPartial();
				}
				else
				{
					GetTopicsOfNamespaceResponse = value;
				}

				BitField0 |= 0x00000001;
				return this;
			}
			public Builder ClearGetTopicsOfNamespaceResponse()
			{
				GetTopicsOfNamespaceResponse = CommandGetTopicsOfNamespaceResponse.DefaultInstance;

				BitField0 = (BitField0 & ~0x00000001);
				return this;
			}

			// optional .pulsar.proto.CommandGetSchema GetSchema = 34;
			public CommandGetSchema GetSchema = CommandGetSchema.DefaultInstance;
			public bool HasGetSchema()
			{
				return ((BitField0 & 0x00000002) == 0x00000002);
			}
			public CommandGetSchema GetGetSchema()
			{
				return GetSchema;
			}
			public Builder SetGetSchema(CommandGetSchema value)
			{
                GetSchema = value ?? throw new NullReferenceException();

				BitField0 |= 0x00000002;
				return this;
			}
			public Builder SetGetSchema(CommandGetSchema.Builder builderForValue)
			{
				GetSchema = builderForValue.Build();

				BitField0 |= 0x00000002;
				return this;
			}
			public Builder MergeGetSchema(CommandGetSchema value)
			{
				if (((BitField0 & 0x00000002) == 0x00000002) && GetSchema != CommandGetSchema.DefaultInstance)
				{
					GetSchema = CommandGetSchema.NewBuilder(GetSchema).MergeFrom(value).BuildPartial();
				}
				else
				{
					GetSchema = value;
				}

				BitField0 |= 0x00000002;
				return this;
			}
			public Builder ClearGetSchema()
			{
				GetSchema = CommandGetSchema.DefaultInstance;

				BitField0 = (BitField0 & ~0x00000002);
				return this;
			}

			// optional .pulsar.proto.CommandGetSchemaResponse GetSchemaResponse = 35;
			public CommandGetSchemaResponse GetSchemaResponse = CommandGetSchemaResponse.DefaultInstance;
			public bool HasGetSchemaResponse()
			{
				return ((BitField0 & 0x00000004) == 0x00000004);
			}
			public CommandGetSchemaResponse GetGetSchemaResponse()
			{
				return GetSchemaResponse;
			}
			public Builder SetGetSchemaResponse(CommandGetSchemaResponse value)
			{
                GetSchemaResponse = value ?? throw new NullReferenceException();

				BitField0 |= 0x00000004;
				return this;
			}
			public Builder SetGetSchemaResponse(CommandGetSchemaResponse.Builder builderForValue)
			{
				GetSchemaResponse = builderForValue.Build();

				BitField0 |= 0x00000004;
				return this;
			}
			public Builder MergeGetSchemaResponse(CommandGetSchemaResponse value)
			{
				if (((BitField0 & 0x00000004) == 0x00000004) && GetSchemaResponse != CommandGetSchemaResponse.DefaultInstance)
				{
					GetSchemaResponse = CommandGetSchemaResponse.NewBuilder(GetSchemaResponse).MergeFrom(value).BuildPartial();
				}
				else
				{
					GetSchemaResponse = value;
				}

				BitField0 |= 0x00000004;
				return this;
			}
			public Builder ClearGetSchemaResponse()
			{
				GetSchemaResponse = CommandGetSchemaResponse.DefaultInstance;

				BitField0 = (BitField0 & ~0x00000004);
				return this;
			}

			// optional .pulsar.proto.CommandAuthChallenge authChallenge = 36;
			public CommandAuthChallenge AuthChallenge = CommandAuthChallenge.DefaultInstance;
			public bool HasAuthChallenge()
			{
				return ((BitField0 & 0x00000008) == 0x00000008);
			}
			public CommandAuthChallenge GetAuthChallenge()
			{
				return AuthChallenge;
			}
			public Builder SetAuthChallenge(CommandAuthChallenge value)
			{
                AuthChallenge = value ?? throw new NullReferenceException();

				BitField0 |= 0x00000008;
				return this;
			}
			public Builder SetAuthChallenge(CommandAuthChallenge.Builder builderForValue)
			{
				AuthChallenge = builderForValue.Build();

				BitField0 |= 0x00000008;
				return this;
			}
			public Builder MergeAuthChallenge(CommandAuthChallenge value)
			{
				if (((BitField0 & 0x00000008) == 0x00000008) && AuthChallenge != CommandAuthChallenge.DefaultInstance)
				{
					AuthChallenge = CommandAuthChallenge.NewBuilder(AuthChallenge).MergeFrom(value).BuildPartial();
				}
				else
				{
					AuthChallenge = value;
				}

				BitField0 |= 0x00000008;
				return this;
			}
			public Builder ClearAuthChallenge()
			{
				AuthChallenge = CommandAuthChallenge.DefaultInstance;

				BitField0 = (BitField0 & ~0x00000008);
				return this;
			}

			// optional .pulsar.proto.CommandAuthResponse authResponse = 37;
			public CommandAuthResponse AuthResponse = CommandAuthResponse.DefaultInstance;
			public bool HasAuthResponse()
			{
				return ((BitField0 & 0x00000010) == 0x00000010);
			}
			public CommandAuthResponse GetAuthResponse()
			{
				return AuthResponse;
			}
			public Builder SetAuthResponse(CommandAuthResponse value)
			{
                AuthResponse = value ?? throw new NullReferenceException();

				BitField0 |= 0x00000010;
				return this;
			}
			public Builder SetAuthResponse(CommandAuthResponse.Builder builderForValue)
			{
				AuthResponse = builderForValue.Build();

				BitField0 |= 0x00000010;
				return this;
			}
			public Builder MergeAuthResponse(CommandAuthResponse value)
			{
				if (((BitField0 & 0x00000010) == 0x00000010) && AuthResponse != CommandAuthResponse.DefaultInstance)
				{
					AuthResponse = CommandAuthResponse.NewBuilder(AuthResponse).MergeFrom(value).BuildPartial();
				}
				else
				{
					AuthResponse = value;
				}

				BitField0 |= 0x00000010;
				return this;
			}
			public Builder ClearAuthResponse()
			{
				AuthResponse = CommandAuthResponse.DefaultInstance;

				BitField0 = (BitField0 & ~0x00000010);
				return this;
			}

			// optional .pulsar.proto.CommandAckResponse ackResponse = 38;
			public CommandAckResponse AckResponse = CommandAckResponse.DefaultInstance;
			public bool HasAckResponse()
			{
				return ((BitField0 & 0x00000020) == 0x00000020);
			}
			public CommandAckResponse GetAckResponse()
			{
				return AckResponse;
			}
			public Builder SetAckResponse(CommandAckResponse value)
			{
                AckResponse = value ?? throw new NullReferenceException();

				BitField0 |= 0x00000020;
				return this;
			}
			public Builder SetAckResponse(CommandAckResponse.Builder builderForValue)
			{
				AckResponse = builderForValue.Build();

				BitField0 |= 0x00000020;
				return this;
			}
			public Builder MergeAckResponse(CommandAckResponse value)
			{
				if (((BitField0 & 0x00000020) == 0x00000020) && AckResponse != CommandAckResponse.DefaultInstance)
				{
					AckResponse = CommandAckResponse.NewBuilder(AckResponse).MergeFrom(value).BuildPartial();
				}
				else
				{
					AckResponse = value;
				}

				BitField0 |= 0x00000020;
				return this;
			}
			public Builder ClearAckResponse()
			{
				AckResponse = CommandAckResponse.DefaultInstance;

				BitField0 = (BitField0 & ~0x00000020);
				return this;
			}

			// optional .pulsar.proto.CommandGetOrCreateSchema GetOrCreateSchema = 39;
			public CommandGetOrCreateSchema GetOrCreateSchema = CommandGetOrCreateSchema.DefaultInstance;
			public bool HasGetOrCreateSchema()
			{
				return ((BitField0 & 0x00000040) == 0x00000040);
			}
			public CommandGetOrCreateSchema GetGetOrCreateSchema()
			{
				return GetOrCreateSchema;
			}
			public Builder SetGetOrCreateSchema(CommandGetOrCreateSchema value)
			{
                GetOrCreateSchema = value ?? throw new NullReferenceException();

				BitField0 |= 0x00000040;
				return this;
			}
			public Builder SetGetOrCreateSchema(CommandGetOrCreateSchema.Builder builderForValue)
			{
				GetOrCreateSchema = builderForValue.Build();

				BitField0 |= 0x00000040;
				return this;
			}
			public Builder MergeGetOrCreateSchema(CommandGetOrCreateSchema value)
			{
				if (((BitField0 & 0x00000040) == 0x00000040) && GetOrCreateSchema != CommandGetOrCreateSchema.DefaultInstance)
				{
					GetOrCreateSchema = CommandGetOrCreateSchema.NewBuilder(GetOrCreateSchema).MergeFrom(value).BuildPartial();
				}
				else
				{
					GetOrCreateSchema = value;
				}

				BitField0 |= 0x00000040;
				return this;
			}
			public Builder ClearGetOrCreateSchema()
			{
				GetOrCreateSchema = CommandGetOrCreateSchema.DefaultInstance;

				BitField0 = (BitField0 & ~0x00000040);
				return this;
			}

			// optional .pulsar.proto.CommandGetOrCreateSchemaResponse GetOrCreateSchemaResponse = 40;
			public CommandGetOrCreateSchemaResponse GetOrCreateSchemaResponse = CommandGetOrCreateSchemaResponse.DefaultInstance;
			public bool HasGetOrCreateSchemaResponse()
			{
				return ((BitField0 & 0x00000080) == 0x00000080);
			}
			public CommandGetOrCreateSchemaResponse GetGetOrCreateSchemaResponse()
			{
				return GetOrCreateSchemaResponse;
			}
			public Builder SetGetOrCreateSchemaResponse(CommandGetOrCreateSchemaResponse value)
			{
                GetOrCreateSchemaResponse = value ?? throw new NullReferenceException();

				BitField0 |= 0x00000080;
				return this;
			}
			public Builder SetGetOrCreateSchemaResponse(CommandGetOrCreateSchemaResponse.Builder builderForValue)
			{
				GetOrCreateSchemaResponse = builderForValue.Build();

				BitField0 |= 0x00000080;
				return this;
			}
			public Builder MergeGetOrCreateSchemaResponse(CommandGetOrCreateSchemaResponse value)
			{
				if (((BitField0 & 0x00000080) == 0x00000080) && GetOrCreateSchemaResponse != CommandGetOrCreateSchemaResponse.DefaultInstance)
				{
					GetOrCreateSchemaResponse = CommandGetOrCreateSchemaResponse.NewBuilder(GetOrCreateSchemaResponse).MergeFrom(value).BuildPartial();
				}
				else
				{
					GetOrCreateSchemaResponse = value;
				}

				BitField0 |= 0x00000080;
				return this;
			}
			public Builder ClearGetOrCreateSchemaResponse()
			{
				GetOrCreateSchemaResponse = CommandGetOrCreateSchemaResponse.DefaultInstance;

				BitField0 = (BitField0 & ~0x00000080);
				return this;
			}

			// optional .pulsar.proto.CommandNewTxn newTxn = 50;
			public CommandNewTxn NewTxn = CommandNewTxn.DefaultInstance;
			public bool HasNewTxn()
			{
				return ((BitField0 & 0x00000100) == 0x00000100);
			}
			public CommandNewTxn GetNewTxn()
			{
				return NewTxn;
			}
			public Builder SetNewTxn(CommandNewTxn value)
			{
                NewTxn = value ?? throw new NullReferenceException();

				BitField0 |= 0x00000100;
				return this;
			}
			public Builder SetNewTxn(CommandNewTxn.Builder builderForValue)
			{
				NewTxn = builderForValue.Build();

				BitField0 |= 0x00000100;
				return this;
			}
			public Builder MergeNewTxn(CommandNewTxn value)
			{
				if (((BitField0 & 0x00000100) == 0x00000100) && NewTxn != CommandNewTxn.DefaultInstance)
				{
					NewTxn = CommandNewTxn.NewBuilder(NewTxn).MergeFrom(value).BuildPartial();
				}
				else
				{
					NewTxn = value;
				}

				BitField0 |= 0x00000100;
				return this;
			}
			public Builder ClearNewTxn()
			{
				NewTxn = CommandNewTxn.DefaultInstance;

				BitField0 = (BitField0 & ~0x00000100);
				return this;
			}

			// optional .pulsar.proto.CommandNewTxnResponse newTxnResponse = 51;
			public CommandNewTxnResponse NewTxnResponse = CommandNewTxnResponse.DefaultInstance;
			public bool HasNewTxnResponse()
			{
				return ((BitField0 & 0x00000200) == 0x00000200);
			}
			public CommandNewTxnResponse GetNewTxnResponse()
			{
				return NewTxnResponse;
			}
			public Builder SetNewTxnResponse(CommandNewTxnResponse value)
			{
                NewTxnResponse = value ?? throw new NullReferenceException();

				BitField0 |= 0x00000200;
				return this;
			}
			public Builder SetNewTxnResponse(CommandNewTxnResponse.Builder builderForValue)
			{
				NewTxnResponse = builderForValue.Build();

				BitField0 |= 0x00000200;
				return this;
			}
			public Builder MergeNewTxnResponse(CommandNewTxnResponse value)
			{
				if (((BitField0 & 0x00000200) == 0x00000200) && NewTxnResponse != CommandNewTxnResponse.DefaultInstance)
				{
					NewTxnResponse = CommandNewTxnResponse.NewBuilder(NewTxnResponse).MergeFrom(value).BuildPartial();
				}
				else
				{
					NewTxnResponse = value;
				}

				BitField0 |= 0x00000200;
				return this;
			}
			public Builder ClearNewTxnResponse()
			{
				NewTxnResponse = CommandNewTxnResponse.DefaultInstance;

				BitField0 = (BitField0 & ~0x00000200);
				return this;
			}

			// optional .pulsar.proto.CommandAddPartitionToTxn addPartitionToTxn = 52;
			public CommandAddPartitionToTxn AddPartitionToTxn = CommandAddPartitionToTxn.DefaultInstance;
			public bool HasAddPartitionToTxn()
			{
				return ((BitField0 & 0x00000400) == 0x00000400);
			}
			public CommandAddPartitionToTxn GetAddPartitionToTxn()
			{
				return AddPartitionToTxn;
			}
			public Builder SetAddPartitionToTxn(CommandAddPartitionToTxn value)
			{
                AddPartitionToTxn = value ?? throw new NullReferenceException();

				BitField0 |= 0x00000400;
				return this;
			}
			public Builder SetAddPartitionToTxn(CommandAddPartitionToTxn.Builder builderForValue)
			{
				AddPartitionToTxn = builderForValue.Build();

				BitField0 |= 0x00000400;
				return this;
			}
			public Builder MergeAddPartitionToTxn(CommandAddPartitionToTxn value)
			{
				if (((BitField0 & 0x00000400) == 0x00000400) && AddPartitionToTxn != CommandAddPartitionToTxn.DefaultInstance)
				{
					AddPartitionToTxn = CommandAddPartitionToTxn.NewBuilder(AddPartitionToTxn).MergeFrom(value).BuildPartial();
				}
				else
				{
					AddPartitionToTxn = value;
				}

				BitField0 |= 0x00000400;
				return this;
			}
			public Builder ClearAddPartitionToTxn()
			{
				AddPartitionToTxn = CommandAddPartitionToTxn.DefaultInstance;

				BitField0 = (BitField0 & ~0x00000400);
				return this;
			}

			// optional .pulsar.proto.CommandAddPartitionToTxnResponse addPartitionToTxnResponse = 53;
			public CommandAddPartitionToTxnResponse AddPartitionToTxnResponse = CommandAddPartitionToTxnResponse.DefaultInstance;
			public bool HasAddPartitionToTxnResponse()
			{
				return ((BitField0 & 0x00000800) == 0x00000800);
			}
			public CommandAddPartitionToTxnResponse GetAddPartitionToTxnResponse()
			{
				return AddPartitionToTxnResponse;
			}
			public Builder SetAddPartitionToTxnResponse(CommandAddPartitionToTxnResponse value)
			{
                AddPartitionToTxnResponse = value ?? throw new NullReferenceException();

				BitField0 |= 0x00000800;
				return this;
			}
			public Builder SetAddPartitionToTxnResponse(CommandAddPartitionToTxnResponse.Builder builderForValue)
			{
				AddPartitionToTxnResponse = builderForValue.Build();

				BitField0 |= 0x00000800;
				return this;
			}
			public Builder MergeAddPartitionToTxnResponse(CommandAddPartitionToTxnResponse value)
			{
				if (((BitField0 & 0x00000800) == 0x00000800) && AddPartitionToTxnResponse != CommandAddPartitionToTxnResponse.DefaultInstance)
				{
					AddPartitionToTxnResponse = CommandAddPartitionToTxnResponse.NewBuilder(AddPartitionToTxnResponse).MergeFrom(value).BuildPartial();
				}
				else
				{
					AddPartitionToTxnResponse = value;
				}

				BitField0 |= 0x00000800;
				return this;
			}
			public Builder ClearAddPartitionToTxnResponse()
			{
				AddPartitionToTxnResponse = CommandAddPartitionToTxnResponse.DefaultInstance;

				BitField0 = (BitField0 & ~0x00000800);
				return this;
			}

			// optional .pulsar.proto.CommandAddSubscriptionToTxn addSubscriptionToTxn = 54;
			public CommandAddSubscriptionToTxn AddSubscriptionToTxn = CommandAddSubscriptionToTxn.DefaultInstance;
			public bool HasAddSubscriptionToTxn()
			{
				return ((BitField0 & 0x00001000) == 0x00001000);
			}
			public CommandAddSubscriptionToTxn GetAddSubscriptionToTxn()
			{
				return AddSubscriptionToTxn;
			}
			public Builder SetAddSubscriptionToTxn(CommandAddSubscriptionToTxn value)
			{
                AddSubscriptionToTxn = value ?? throw new NullReferenceException();

				BitField0 |= 0x00001000;
				return this;
			}
			public Builder SetAddSubscriptionToTxn(CommandAddSubscriptionToTxn.Builder builderForValue)
			{
				AddSubscriptionToTxn = builderForValue.Build();

				BitField0 |= 0x00001000;
				return this;
			}
			public Builder MergeAddSubscriptionToTxn(CommandAddSubscriptionToTxn value)
			{
				if (((BitField0 & 0x00001000) == 0x00001000) && AddSubscriptionToTxn != CommandAddSubscriptionToTxn.DefaultInstance)
				{
					AddSubscriptionToTxn = CommandAddSubscriptionToTxn.NewBuilder(AddSubscriptionToTxn).MergeFrom(value).BuildPartial();
				}
				else
				{
					AddSubscriptionToTxn = value;
				}

				BitField0 |= 0x00001000;
				return this;
			}
			public Builder ClearAddSubscriptionToTxn()
			{
				AddSubscriptionToTxn = CommandAddSubscriptionToTxn.DefaultInstance;

				BitField0 = (BitField0 & ~0x00001000);
				return this;
			}

			// optional .pulsar.proto.CommandAddSubscriptionToTxnResponse addSubscriptionToTxnResponse = 55;
			public CommandAddSubscriptionToTxnResponse AddSubscriptionToTxnResponse = CommandAddSubscriptionToTxnResponse.DefaultInstance;
			public bool HasAddSubscriptionToTxnResponse()
			{
				return ((BitField0 & 0x00002000) == 0x00002000);
			}
			public CommandAddSubscriptionToTxnResponse GetAddSubscriptionToTxnResponse()
			{
				return AddSubscriptionToTxnResponse;
			}
			public Builder SetAddSubscriptionToTxnResponse(CommandAddSubscriptionToTxnResponse value)
			{
                AddSubscriptionToTxnResponse = value ?? throw new NullReferenceException();

				BitField0 |= 0x00002000;
				return this;
			}
			public Builder SetAddSubscriptionToTxnResponse(CommandAddSubscriptionToTxnResponse.Builder builderForValue)
			{
				AddSubscriptionToTxnResponse = builderForValue.Build();

				BitField0 |= 0x00002000;
				return this;
			}
			public Builder MergeAddSubscriptionToTxnResponse(CommandAddSubscriptionToTxnResponse value)
			{
				if (((BitField0 & 0x00002000) == 0x00002000) && AddSubscriptionToTxnResponse != CommandAddSubscriptionToTxnResponse.DefaultInstance)
				{
					AddSubscriptionToTxnResponse = CommandAddSubscriptionToTxnResponse.NewBuilder(AddSubscriptionToTxnResponse).MergeFrom(value).BuildPartial();
				}
				else
				{
					AddSubscriptionToTxnResponse = value;
				}

				BitField0 |= 0x00002000;
				return this;
			}
			public Builder ClearAddSubscriptionToTxnResponse()
			{
				AddSubscriptionToTxnResponse = CommandAddSubscriptionToTxnResponse.DefaultInstance;

				BitField0 = (BitField0 & ~0x00002000);
				return this;
			}

			// optional .pulsar.proto.CommandEndTxn endTxn = 56;
			public CommandEndTxn EndTxn = CommandEndTxn.DefaultInstance;
			public bool HasEndTxn()
			{
				return ((BitField0 & 0x00004000) == 0x00004000);
			}
			public CommandEndTxn GetEndTxn()
			{
				return EndTxn;
			}
			public Builder SetEndTxn(CommandEndTxn value)
			{
                EndTxn = value ?? throw new NullReferenceException();

				BitField0 |= 0x00004000;
				return this;
			}
			public Builder SetEndTxn(CommandEndTxn.Builder builderForValue)
			{
				EndTxn = builderForValue.Build();

				BitField0 |= 0x00004000;
				return this;
			}
			public Builder MergeEndTxn(CommandEndTxn value)
			{
				if (((BitField0 & 0x00004000) == 0x00004000) && EndTxn != CommandEndTxn.DefaultInstance)
				{
					EndTxn = CommandEndTxn.NewBuilder(EndTxn).MergeFrom(value).BuildPartial();
				}
				else
				{
					EndTxn = value;
				}

				BitField0 |= 0x00004000;
				return this;
			}
			public Builder ClearEndTxn()
			{
				EndTxn = CommandEndTxn.DefaultInstance;

				BitField0 = (BitField0 & ~0x00004000);
				return this;
			}

			// optional .pulsar.proto.CommandEndTxnResponse endTxnResponse = 57;
			public CommandEndTxnResponse EndTxnResponse = CommandEndTxnResponse.DefaultInstance;
			public bool HasEndTxnResponse()
			{
				return ((BitField0 & 0x00008000) == 0x00008000);
			}
			public CommandEndTxnResponse GetEndTxnResponse()
			{
				return EndTxnResponse;
			}
			public Builder SetEndTxnResponse(CommandEndTxnResponse value)
			{
                EndTxnResponse = value ?? throw new NullReferenceException();

				BitField0 |= 0x00008000;
				return this;
			}
			public Builder SetEndTxnResponse(CommandEndTxnResponse.Builder builderForValue)
			{
				EndTxnResponse = builderForValue.Build();

				BitField0 |= 0x00008000;
				return this;
			}
			public Builder MergeEndTxnResponse(CommandEndTxnResponse value)
			{
				if (((BitField0 & 0x00008000) == 0x00008000) && EndTxnResponse != CommandEndTxnResponse.DefaultInstance)
				{
					EndTxnResponse = CommandEndTxnResponse.NewBuilder(EndTxnResponse).MergeFrom(value).BuildPartial();
				}
				else
				{
					EndTxnResponse = value;
				}

				BitField0 |= 0x00008000;
				return this;
			}
			public Builder ClearEndTxnResponse()
			{
				EndTxnResponse = CommandEndTxnResponse.DefaultInstance;

				BitField0 = (BitField0 & ~0x00008000);
				return this;
			}

			// optional .pulsar.proto.CommandEndTxnOnPartition endTxnOnPartition = 58;
			public CommandEndTxnOnPartition EndTxnOnPartition = CommandEndTxnOnPartition.DefaultInstance;
			public bool HasEndTxnOnPartition()
			{
				return ((BitField0 & 0x00010000) == 0x00010000);
			}
			public CommandEndTxnOnPartition GetEndTxnOnPartition()
			{
				return EndTxnOnPartition;
			}
			public Builder SetEndTxnOnPartition(CommandEndTxnOnPartition value)
			{
                EndTxnOnPartition = value ?? throw new NullReferenceException();

				BitField0 |= 0x00010000;
				return this;
			}
			public Builder SetEndTxnOnPartition(CommandEndTxnOnPartition.Builder builderForValue)
			{
				EndTxnOnPartition = builderForValue.Build();

				BitField0 |= 0x00010000;
				return this;
			}
			public Builder MergeEndTxnOnPartition(CommandEndTxnOnPartition value)
			{
				if (((BitField0 & 0x00010000) == 0x00010000) && EndTxnOnPartition != CommandEndTxnOnPartition.DefaultInstance)
				{
					EndTxnOnPartition = CommandEndTxnOnPartition.NewBuilder(EndTxnOnPartition).MergeFrom(value).BuildPartial();
				}
				else
				{
					EndTxnOnPartition = value;
				}

				BitField0 |= 0x00010000;
				return this;
			}
			public Builder ClearEndTxnOnPartition()
			{
				EndTxnOnPartition = CommandEndTxnOnPartition.DefaultInstance;

				BitField0 = (BitField0 & ~0x00010000);
				return this;
			}

			// optional .pulsar.proto.CommandEndTxnOnPartitionResponse endTxnOnPartitionResponse = 59;
			public CommandEndTxnOnPartitionResponse EndTxnOnPartitionResponse = CommandEndTxnOnPartitionResponse.DefaultInstance;
			public bool HasEndTxnOnPartitionResponse()
			{
				return ((BitField0 & 0x00020000) == 0x00020000);
			}
			public CommandEndTxnOnPartitionResponse GetEndTxnOnPartitionResponse()
			{
				return EndTxnOnPartitionResponse;
			}
			public Builder SetEndTxnOnPartitionResponse(CommandEndTxnOnPartitionResponse value)
			{
                EndTxnOnPartitionResponse = value ?? throw new NullReferenceException();

				BitField0 |= 0x00020000;
				return this;
			}
			public Builder SetEndTxnOnPartitionResponse(CommandEndTxnOnPartitionResponse.Builder builderForValue)
			{
				EndTxnOnPartitionResponse = builderForValue.Build();

				BitField0 |= 0x00020000;
				return this;
			}
			public Builder MergeEndTxnOnPartitionResponse(CommandEndTxnOnPartitionResponse value)
			{
				if (((BitField0 & 0x00020000) == 0x00020000) && EndTxnOnPartitionResponse != CommandEndTxnOnPartitionResponse.DefaultInstance)
				{
					EndTxnOnPartitionResponse = CommandEndTxnOnPartitionResponse.NewBuilder(EndTxnOnPartitionResponse).MergeFrom(value).BuildPartial();
				}
				else
				{
					EndTxnOnPartitionResponse = value;
				}

				BitField0 |= 0x00020000;
				return this;
			}
			public Builder ClearEndTxnOnPartitionResponse()
			{
				EndTxnOnPartitionResponse = CommandEndTxnOnPartitionResponse.DefaultInstance;

				BitField0 = (BitField0 & ~0x00020000);
				return this;
			}

			// optional .pulsar.proto.CommandEndTxnOnSubscription endTxnOnSubscription = 60;
			public CommandEndTxnOnSubscription EndTxnOnSubscription = CommandEndTxnOnSubscription.DefaultInstance;
			public bool HasEndTxnOnSubscription()
			{
				return ((BitField0 & 0x00040000) == 0x00040000);
			}
			public CommandEndTxnOnSubscription GetEndTxnOnSubscription()
			{
				return EndTxnOnSubscription;
			}
			public Builder SetEndTxnOnSubscription(CommandEndTxnOnSubscription value)
			{
                EndTxnOnSubscription = value ?? throw new NullReferenceException();

				BitField0 |= 0x00040000;
				return this;
			}
			public Builder SetEndTxnOnSubscription(CommandEndTxnOnSubscription.Builder builderForValue)
			{
				EndTxnOnSubscription = builderForValue.Build();

				BitField0 |= 0x00040000;
				return this;
			}
			public Builder MergeEndTxnOnSubscription(CommandEndTxnOnSubscription value)
			{
				if (((BitField0 & 0x00040000) == 0x00040000) && EndTxnOnSubscription != CommandEndTxnOnSubscription.DefaultInstance)
				{
					EndTxnOnSubscription = CommandEndTxnOnSubscription.NewBuilder(EndTxnOnSubscription).MergeFrom(value).BuildPartial();
				}
				else
				{
					EndTxnOnSubscription = value;
				}

				BitField0 |= 0x00040000;
				return this;
			}
			public Builder ClearEndTxnOnSubscription()
			{
				EndTxnOnSubscription = CommandEndTxnOnSubscription.DefaultInstance;

				BitField0 = (BitField0 & ~0x00040000);
				return this;
			}

			
			public CommandEndTxnOnSubscriptionResponse EndTxnOnSubscriptionResponse = CommandEndTxnOnSubscriptionResponse.DefaultInstance;
			public bool HasEndTxnOnSubscriptionResponse()
			{
				return ((BitField0 & 0x00080000) == 0x00080000);
			}
			public CommandEndTxnOnSubscriptionResponse GetEndTxnOnSubscriptionResponse()
			{
				return EndTxnOnSubscriptionResponse;
			}
			public Builder SetEndTxnOnSubscriptionResponse(CommandEndTxnOnSubscriptionResponse value)
			{
                EndTxnOnSubscriptionResponse = value ?? throw new NullReferenceException();

				BitField0 |= 0x00080000;
				return this;
			}
			public Builder SetEndTxnOnSubscriptionResponse(CommandEndTxnOnSubscriptionResponse.Builder builderForValue)
			{
				EndTxnOnSubscriptionResponse = builderForValue.Build();

				BitField0 |= 0x00080000;
				return this;
			}
			public Builder MergeEndTxnOnSubscriptionResponse(CommandEndTxnOnSubscriptionResponse value)
			{
				if (((BitField0 & 0x00080000) == 0x00080000) && EndTxnOnSubscriptionResponse != CommandEndTxnOnSubscriptionResponse.DefaultInstance)
				{
					EndTxnOnSubscriptionResponse = CommandEndTxnOnSubscriptionResponse.NewBuilder(EndTxnOnSubscriptionResponse).MergeFrom(value).BuildPartial();
				}
				else
				{
					EndTxnOnSubscriptionResponse = value;
				}

				BitField0 |= 0x00080000;
				return this;
			}
			public Builder ClearEndTxnOnSubscriptionResponse()
			{
				EndTxnOnSubscriptionResponse = CommandEndTxnOnSubscriptionResponse.DefaultInstance;
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
