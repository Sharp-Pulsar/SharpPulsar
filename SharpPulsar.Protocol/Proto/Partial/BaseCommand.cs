using DotNetty.Common;
using Google.Protobuf;
using SharpPulsar.Util.Protobuf;
using System;
using static SharpPulsar.Util.Protobuf.ByteBufCodedOutputStream;

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
		
		public void Recycle()
		{
			this.InitFields();
			this.MemoizedIsInitialized = -1;
			this._hasBits0 = 0;
			this.MemoizedSerializedSize = -1;
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
		

		public  class Builder
		{
			// Construct using org.apache.pulsar.common.api.proto.BaseCommand.newBuilder()
			internal static ThreadLocalPool<Builder> _pool = new ThreadLocalPool<Builder>(handle => new Builder(handle), 1, true);
			int _bitField = 0;
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
				//_type = BaseCommand.Types.Type.Connect;
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
				var result = BaseCommand._pool.Take();
				result.type_ = _type;
				result._hasBits0 = (int)_type;
				switch(_type)
				{
					case Types.Type.Connect:
						result.connect_ = _connect;
						break;
					case Types.Type.Connected:
						result.connected_ = _connected;
						break;
					case Types.Type.Subscribe:
						result.subscribe_ = Subscribe_;
						break;
					case Types.Type.Producer:
						result.producer_ = Producer_;
						break;
					case Types.Type.Send:
						result.send_ = Send_;
						break;
					case Types.Type.SendReceipt:
						result.sendReceipt_ = SendReceipt_;
						break;
					case Types.Type.SendError:
						result.sendError_ = SendError_;
						break;
					case Types.Type.Message:
						result.message_ = Message_;
						break;
					case Types.Type.Ack:
						result.ack_ = Ack_;
						break;
					case Types.Type.Flow:
						result.flow_ = Flow_;
						break;
					case Types.Type.Unsubscribe:
						result.unsubscribe_ = Unsubscribe_;
						break;
					case Types.Type.Success:
						result.success_ = Success_;
						break;
					case Types.Type.Error:
						result.error_ = Error_;
						break;
					case Types.Type.CloseProducer:
						result.closeProducer_ = CloseProducer_;
						break;
					case Types.Type.CloseConsumer:
						result.closeConsumer_ = CloseConsumer_;
						break;
					case Types.Type.ProducerSuccess:
						result.producerSuccess_ = ProducerSuccess_;
						break;
					case Types.Type.Ping:
						result.ping_ = Ping_;
						break;
					case Types.Type.Pong:
						result.pong_ = Pong_;
						break;
					case Types.Type.RedeliverUnacknowledgedMessages:
						result.redeliverUnacknowledgedMessages_ = RedeliverUnacknowledgedMessages_;
						break;
					case Types.Type.PartitionedMetadata:
						result.partitionMetadata_ = PartitionMetadata_;
						break;
					case Types.Type.PartitionedMetadataResponse:
						result.partitionMetadataResponse_ = PartitionMetadataResponse_;
						break;
					case Types.Type.Lookup:
						result.lookupTopic_ = LookupTopic_;
						break;
					case Types.Type.LookupResponse:
						result.lookupTopicResponse_ = LookupTopicResponse_;
						break;
					case Types.Type.ConsumerStats:
						result.consumerStats_ = ConsumerStats_;
						break;
					case Types.Type.ConsumerStatsResponse:
						result.consumerStatsResponse_ = ConsumerStatsResponse_;
						break;
					case Types.Type.ReachedEndOfTopic:
						result.reachedEndOfTopic_ = ReachedEndOfTopic_;
						break;
					case Types.Type.Seek:
						result.seek_ = Seek_;
						break;
					case Types.Type.GetLastMessageId:
						result.getLastMessageId_ = GetLastMessageId_;
						break;
					case Types.Type.GetLastMessageIdResponse:
						result.getLastMessageIdResponse_ = GetLastMessageIdResponse_;
						break;
					case Types.Type.ActiveConsumerChange:
						result.activeConsumerChange_ = ActiveConsumerChange_;
						break;
					case Types.Type.GetTopicsOfNamespace:
						result.getTopicsOfNamespace_ = GetTopicsOfNamespace_;
						break;
					case Types.Type.GetTopicsOfNamespaceResponse:
						result.getTopicsOfNamespaceResponse_ = GetTopicsOfNamespaceResponse_;
						break;
					case Types.Type.GetSchema:
						result.getSchema_ = GetSchema_;
						break;
					case Types.Type.GetSchemaResponse:
						result.getSchemaResponse_ = GetSchemaResponse_;
						break;
					case Types.Type.AuthChallenge:
						result.authChallenge_ = AuthChallenge_;
						break;
					case Types.Type.AuthResponse:
						result.authResponse_ = AuthResponse_;
						break;
					case Types.Type.AckResponse:
						result.ackResponse_ = AckResponse_;
						break;
					case Types.Type.GetOrCreateSchema:
						result.getOrCreateSchema_ = GetOrCreateSchema_;
						break;
					case Types.Type.GetOrCreateSchemaResponse:
						result.getOrCreateSchemaResponse_ = GetOrCreateSchemaResponse_;
						break;
					case Types.Type.NewTxn:
						result.newTxn_ = NewTxn_;
						break;
					case Types.Type.NewTxnResponse:
						result.newTxnResponse_ = NewTxnResponse_;
						break;
					case Types.Type.AddPartitionToTxn:
						result.addPartitionToTxn_ = AddPartitionToTxn_;
						break;
					case Types.Type.AddPartitionToTxnResponse:
						result.addPartitionToTxnResponse_ = AddPartitionToTxnResponse_;
						break;
					case Types.Type.AddSubscriptionToTxn:
						result.addSubscriptionToTxn_ = AddSubscriptionToTxn_;
						break;
					case Types.Type.AddSubscriptionToTxnResponse:
						result.addSubscriptionToTxnResponse_ = AddSubscriptionToTxnResponse_;
						break;
					case Types.Type.EndTxn:
						result.endTxn_ = EndTxn_;
						break;
					case Types.Type.EndTxnResponse:
						result.endTxnResponse_ = EndTxnResponse_;
						break;
					case Types.Type.EndTxnOnPartition:
						result.endTxnOnPartition_ = EndTxnOnPartition_;
						break;
					case Types.Type.EndTxnOnPartitionResponse:
						result.endTxnOnPartitionResponse_ = EndTxnOnPartitionResponse_;
						break;
					case Types.Type.EndTxnOnSubscription:
						result.endTxnOnSubscription_ = _endTxnOnSubscription;
						break;
					case Types.Type.EndTxnOnSubscriptionResponse:
						result.endTxnOnSubscriptionResponse_ = _endTxnOnSubscriptionResponse;
						break;
				}						
				return result;
			}

			public bool Initialized
			{
				get
				{
					if (!HasType)
					{

						return false;
					}
					if (HasConnect())
					{
						if (!getConnect().IsInitialized())
						{

							return false;
						}
					}
					if (HasConnected())
					{
						if (!getConnected().IsInitialized())
						{

							return false;
						}
					}
					if (HasSubscribe())
					{
						if (!getSubscribe().IsInitialized())
						{

							return false;
						}
					}
					if (HasProducer())
					{
						if (!getProducer().IsInitialized())
						{

							return false;
						}
					}
					if (HasSend())
					{
						if (!getSend().IsInitialized())
						{

							return false;
						}
					}
					if (HasSendReceipt())
					{
						if (!getSendReceipt().IsInitialized())
						{

							return false;
						}
					}
					if (HasSendError())
					{
						if (!getSendError().IsInitialized())
						{

							return false;
						}
					}
					if (HasMessage())
					{
						if (!getMessage().IsInitialized())
						{

							return false;
						}
					}
					if (HasAck())
					{
						if (!getAck().IsInitialized())
						{

							return false;
						}
					}
					if (HasFlow())
					{
						if (!getFlow().IsInitialized())
						{

							return false;
						}
					}
					if (HasUnsubscribe())
					{
						if (!getUnsubscribe().IsInitialized())
						{

							return false;
						}
					}
					if (HasSuccess())
					{
						if (!getSuccess().IsInitialized())
						{

							return false;
						}
					}
					if (HasError())
					{
						if (!getError().IsInitialized())
						{

							return false;
						}
					}
					if (HasCloseProducer())
					{
						if (!getCloseProducer().IsInitialized())
						{

							return false;
						}
					}
					if (HasCloseConsumer())
					{
						if (!getCloseConsumer().IsInitialized())
						{

							return false;
						}
					}
					if (HasProducerSuccess())
					{
						if (!getProducerSuccess().IsInitialized())
						{

							return false;
						}
					}
					if (HasRedeliverUnacknowledgedMessages())
					{
						if (!getRedeliverUnacknowledgedMessages().IsInitialized())
						{

							return false;
						}
					}
					if (HasPartitionMetadata())
					{
						if (!getPartitionMetadata().IsInitialized())
						{

							return false;
						}
					}
					if (HasPartitionMetadataResponse())
					{
						if (!getPartitionMetadataResponse().IsInitialized())
						{

							return false;
						}
					}
					if (HasLookupTopic())
					{
						if (!getLookupTopic().IsInitialized())
						{

							return false;
						}
					}
					if (HasLookupTopicResponse())
					{
						if (!getLookupTopicResponse().IsInitialized())
						{

							return false;
						}
					}
					if (HasConsumerStats())
					{
						if (!getConsumerStats().IsInitialized())
						{

							return false;
						}
					}
					if (HasConsumerStatsResponse())
					{
						if (!getConsumerStatsResponse().IsInitialized())
						{

							return false;
						}
					}
					if (HasReachedEndOfTopic())
					{
						if (!getReachedEndOfTopic().IsInitialized())
						{

							return false;
						}
					}
					if (HasSeek())
					{
						if (!getSeek().IsInitialized())
						{

							return false;
						}
					}
					if (HasGetLastMessageId())
					{
						if (!getGetLastMessageId().IsInitialized())
						{

							return false;
						}
					}
					if (HasGetLastMessageIdResponse())
					{
						if (!getGetLastMessageIdResponse().IsInitialized())
						{

							return false;
						}
					}
					if (HasActiveConsumerChange())
					{
						if (!getActiveConsumerChange().IsInitialized())
						{

							return false;
						}
					}
					if (HasGetTopicsOfNamespace())
					{
						if (!getGetTopicsOfNamespace().IsInitialized())
						{

							return false;
						}
					}
					if (HasGetTopicsOfNamespaceResponse())
					{
						if (!getGetTopicsOfNamespaceResponse().IsInitialized())
						{

							return false;
						}
					}
					if (HasGetSchema())
					{
						if (!getGetSchema().IsInitialized())
						{

							return false;
						}
					}
					if (HasGetSchemaResponse())
					{
						if (!getGetSchemaResponse().IsInitialized())
						{

							return false;
						}
					}
					if (HasAckResponse())
					{
						if (!getAckResponse().IsInitialized())
						{

							return false;
						}
					}
					if (HasGetOrCreateSchema())
					{
						if (!getGetOrCreateSchema().IsInitialized())
						{

							return false;
						}
					}
					if (HasGetOrCreateSchemaResponse())
					{
						if (!getGetOrCreateSchemaResponse().IsInitialized())
						{

							return false;
						}
					}
					if (HasNewTxn())
					{
						if (!getNewTxn().IsInitialized())
						{

							return false;
						}
					}
					if (HasNewTxnResponse())
					{
						if (!getNewTxnResponse().IsInitialized())
						{

							return false;
						}
					}
					if (HasAddPartitionToTxn())
					{
						if (!getAddPartitionToTxn().IsInitialized())
						{

							return false;
						}
					}
					if (HasAddPartitionToTxnResponse())
					{
						if (!getAddPartitionToTxnResponse().IsInitialized())
						{

							return false;
						}
					}
					if (HasAddSubscriptionToTxn())
					{
						if (!getAddSubscriptionToTxn().IsInitialized())
						{

							return false;
						}
					}
					if (HasAddSubscriptionToTxnResponse())
					{
						if (!getAddSubscriptionToTxnResponse().IsInitialized())
						{

							return false;
						}
					}
					if (HasEndTxn())
					{
						if (!getEndTxn().IsInitialized())
						{

							return false;
						}
					}
					if (HasEndTxnResponse())
					{
						if (!getEndTxnResponse().IsInitialized())
						{

							return false;
						}
					}
					if (HasEndTxnOnPartition())
					{
						if (!getEndTxnOnPartition().IsInitialized())
						{

							return false;
						}
					}
					if (HasEndTxnOnPartitionResponse())
					{
						if (!getEndTxnOnPartitionResponse().IsInitialized())
						{

							return false;
						}
					}
					if (HasEndTxnOnSubscription())
					{
						if (!getEndTxnOnSubscription().IsInitialized())
						{

							return false;
						}
					}
					if (HasEndTxnOnSubscriptionResponse())
					{
						if (!getEndTxnOnSubscriptionResponse().IsInitialized())
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

				_bitField |= 0x00000002;
				return this;
			}
			public Builder SetConnect(CommandConnect.Builder builder)
			{
				_connect = builder.Build();

				_bitField |= 0x00000002;
				return this;
			}
			public Builder MergeConnect(CommandConnect value)
			{
				if (((_bitField & 0x00000002) == 0x00000002) && _connect != CommandConnect.DefaultInstance)
				{
					_connect = CommandConnect.NewBuilder(_connect).MergeFrom(value).BuildPartial();
				}
				else
				{
					_connect = value;
				}

				_bitField |= 0x00000002;
				return this;
			}
			
			// optional .pulsar.proto.CommandConnected connected = 3;
			internal CommandConnected _connected = CommandConnected.DefaultInstance;
			public bool HasConnected()
			{
				return ((_bitField & 0x00000004) == 0x00000004);
			}
			public CommandConnected GetConnected()
			{
				return _connected;
			}
			public Builder GetConnected(CommandConnected Value)
			{
				if (Value == null)
				{
					throw new System.NullReferenceException();
				}
				_connected = Value;

				_bitField |= 0x00000004;
				return this;
			}
			public Builder SetConnected(CommandConnected.Builder BuilderForValue)
			{
				_connected = BuilderForValue.build();
				_bitField |= 0x00000004;
				return this;
			}
			public Builder MergeConnected(CommandConnected Value)
			{
				if (((_bitField & 0x00000004) == 0x00000004) && _connected != CommandConnected.DefaultInstance)
				{
					_connected = CommandConnected.NewBuilder(_connected).mergeFrom(Value).buildPartial();
				}
				else
				{
					_connected = Value;
				}

				_bitField |= 0x00000004;
				return this;
			}
			public Builder ClearConnected()
			{
				_connected = CommandConnected.DefaultInstance;

				_bitField = (_bitField & ~0x00000004);
				return this;
			}

			internal CommandSubscribe Subscribe_ = CommandSubscribe.DefaultInstance;
			public bool HasSubscribe()
			{
				return ((_bitField & 0x00000008) == 0x00000008);
			}
			public CommandSubscribe GetSubscribe()
			{
				return Subscribe_;
			}
			public Builder SetSubscribe(CommandSubscribe Value)
			{
				if (Value == null)
				{
					throw new System.NullReferenceException();
				}
				Subscribe_ = Value;

				_bitField |= 0x00000008;
				return this;
			}
			public Builder SetSubscribe(CommandSubscribe.Builder BuilderForValue)
			{
				Subscribe_ = BuilderForValue.build();

				_bitField |= 0x00000008;
				return this;
			}
			public Builder MergeSubscribe(CommandSubscribe Value)
			{
				if (((_bitField & 0x00000008) == 0x00000008) && Subscribe_ != CommandSubscribe.DefaultInstance)
				{
					Subscribe_ = CommandSubscribe.NewBuilder(Subscribe).mergeFrom(Value).buildPartial();
				}
				else
				{
					Subscribe_ = Value;
				}

				_bitField |= 0x00000008;
				return this;
			}
			public Builder ClearSubscribe()
			{
				Subscribe_ = CommandSubscribe.DefaultInstance;

				_bitField = (_bitField & ~0x00000008);
				return this;
			}

			// optional .pulsar.proto.CommandProducer producer = 5;
			internal CommandProducer Producer_ = CommandProducer.DefaultInstance;
			public bool HasProducer()
			{
				return ((_bitField & 0x00000010) == 0x00000010);
			}
			public CommandProducer GetProducer()
			{
				return Producer_;
			}
			public Builder SetProducer(CommandProducer Value)
			{
				if (Value == null)
				{
					throw new System.NullReferenceException();
				}
				Producer_ = Value;

				_bitField |= 0x00000010;
				return this;
			}
			public Builder SetProducer(CommandProducer.Builder BuilderForValue)
			{
				Producer_ = BuilderForValue.build();

				_bitField |= 0x00000010;
				return this;
			}
			public Builder MergeProducer(CommandProducer Value)
			{
				if (((_bitField & 0x00000010) == 0x00000010) && Producer_ != CommandProducer.DefaultInstance)
				{
					Producer_ = CommandProducer.NewBuilder(Producer).mergeFrom(Value).buildPartial();
				}
				else
				{
					Producer_ = Value;
				}

				_bitField |= 0x00000010;
				return this;
			}
			public Builder ClearProducer()
			{
				Producer_ = CommandProducer.DefaultInstance;

				_bitField = (_bitField & ~0x00000010);
				return this;
			}

			// optional .pulsar.proto.CommandSend send = 6;
			internal CommandSend Send_ = CommandSend.DefaultInstance;
			public bool HasSend()
			{
				return ((_bitField & 0x00000020) == 0x00000020);
			}
			public CommandSend GetSend()
			{
				return Send_;
			}
			public Builder SetSend(CommandSend Value)
			{
				if (Value == null)
				{
					throw new System.NullReferenceException();
				}
				Send_ = Value;

				_bitField |= 0x00000020;
				return this;
			}
			public Builder SetSend(CommandSend.Builder BuilderForValue)
			{
				Send_ = BuilderForValue.build();

				_bitField |= 0x00000020;
				return this;
			}
			public Builder MergeSend(CommandSend Value)
			{
				if (((_bitField & 0x00000020) == 0x00000020) && Send_ != CommandSend.DefaultInstance)
				{
					Send_ = CommandSend.NewBuilder(Send).mergeFrom(Value).buildPartial();
				}
				else
				{
					Send_ = Value;
				}

				_bitField |= 0x00000020;
				return this;
			}
			public Builder ClearSend()
			{
				Send_ = CommandSend.DefaultInstance;

				_bitField = (_bitField & ~0x00000020);
				return this;
			}

			// optional .pulsar.proto.CommandSendReceipt send_receipt = 7;
			internal CommandSendReceipt SendReceipt_ = CommandSendReceipt.DefaultInstance;
			public bool HasSendReceipt()
			{
				return ((_bitField & 0x00000040) == 0x00000040);
			}
			public CommandSendReceipt GetSendReceipt()
			{
				return SendReceipt_;
			}
			public Builder SetSendReceipt(CommandSendReceipt Value)
			{
				if (Value == null)
				{
					throw new System.NullReferenceException();
				}
				SendReceipt_ = Value;

				_bitField |= 0x00000040;
				return this;
			}
			public Builder SetSendReceipt(CommandSendReceipt.Builder BuilderForValue)
			{
				SendReceipt_ = BuilderForValue.build();

				_bitField |= 0x00000040;
				return this;
			}
			public Builder MergeSendReceipt(CommandSendReceipt Value)
			{
				if (((_bitField & 0x00000040) == 0x00000040) && SendReceipt_ != CommandSendReceipt.DefaultInstance)
				{
					SendReceipt_ = CommandSendReceipt.NewBuilder(SendReceipt).mergeFrom(Value).buildPartial();
				}
				else
				{
					SendReceipt_ = Value;
				}

				_bitField |= 0x00000040;
				return this;
			}
			public Builder ClearSendReceipt()
			{
				SendReceipt_ = CommandSendReceipt.DefaultInstance;

				_bitField = (_bitField & ~0x00000040);
				return this;
			}

			// optional .pulsar.proto.CommandSendError send_error = 8;
			internal CommandSendError SendError_ = CommandSendError.DefaultInstance;
			public bool HasSendError()
			{
				return ((_bitField & 0x00000080) == 0x00000080);
			}
			public CommandSendError GetSendError()
			{
				return SendError_;
			}
			public Builder SetSendError(CommandSendError Value)
			{
				if (Value == null)
				{
					throw new System.NullReferenceException();
				}
				SendError_ = Value;

				_bitField |= 0x00000080;
				return this;
			}
			public Builder SetSendError(CommandSendError.Builder BuilderForValue)
			{
				SendError_ = BuilderForValue.build();

				_bitField |= 0x00000080;
				return this;
			}
			public Builder MergeSendError(CommandSendError Value)
			{
				if (((_bitField & 0x00000080) == 0x00000080) && SendError_ != CommandSendError.DefaultInstance)
				{
					SendError_ = CommandSendError.NewBuilder(SendError).mergeFrom(Value).buildPartial();
				}
				else
				{
					SendError_ = Value;
				}

				_bitField |= 0x00000080;
				return this;
			}
			public Builder ClearSendError()
			{
				SendError_ = CommandSendError.DefaultInstance;

				_bitField = (_bitField & ~0x00000080);
				return this;
			}

			// optional .pulsar.proto.CommandMessage message = 9;
			internal CommandMessage Message_ = CommandMessage.DefaultInstance;
			public bool HasMessage()
			{
				return ((_bitField & 0x00000100) == 0x00000100);
			}
			public CommandMessage GetMessage()
			{
				return Message_;
			}
			public Builder SetMessage(CommandMessage Value)
			{
				if (Value == null)
				{
					throw new System.NullReferenceException();
				}
				Message_ = Value;

				_bitField |= 0x00000100;
				return this;
			}
			public Builder SetMessage(CommandMessage.Builder BuilderForValue)
			{
				Message_ = BuilderForValue.build();

				_bitField |= 0x00000100;
				return this;
			}
			public Builder MergeMessage(CommandMessage Value)
			{
				if (((_bitField & 0x00000100) == 0x00000100) && Message_ != CommandMessage.DefaultInstance)
				{
					Message_ = CommandMessage.NewBuilder(Message).mergeFrom(Value).buildPartial();
				}
				else
				{
					Message_ = Value;
				}

				_bitField |= 0x00000100;
				return this;
			}
			public Builder ClearMessage()
			{
				Message_ = CommandMessage.DefaultInstance;

				_bitField = (_bitField & ~0x00000100);
				return this;
			}

			// optional .pulsar.proto.CommandAck ack = 10;
			internal CommandAck Ack_ = CommandAck.DefaultInstance;
			public bool HasAck()
			{
				return ((_bitField & 0x00000200) == 0x00000200);
			}
			public CommandAck GetAck()
			{
				return Ack_;
			}
			public Builder SetAck(CommandAck Value)
			{
				if (Value == null)
				{
					throw new System.NullReferenceException();
				}
				Ack_ = Value;

				_bitField |= 0x00000200;
				return this;
			}
			public Builder SetAck(CommandAck.Builder BuilderForValue)
			{
				Ack_ = BuilderForValue.build();

				_bitField |= 0x00000200;
				return this;
			}
			public Builder MergeAck(CommandAck Value)
			{
				if (((_bitField & 0x00000200) == 0x00000200) && Ack_ != CommandAck.DefaultInstance)
				{
					Ack_ = CommandAck.NewBuilder(Ack).mergeFrom(Value).buildPartial();
				}
				else
				{
					Ack_ = Value;
				}

				_bitField |= 0x00000200;
				return this;
			}
			public Builder ClearAck()
			{
				Ack_ = CommandAck.DefaultInstance;

				_bitField = (_bitField & ~0x00000200);
				return this;
			}

			// optional .pulsar.proto.CommandFlow flow = 11;
			internal CommandFlow Flow_ = CommandFlow.DefaultInstance;
			public bool HasFlow()
			{
				return ((_bitField & 0x00000400) == 0x00000400);
			}
			public CommandFlow GetFlow()
			{
				return Flow_;
			}
			public Builder SetFlow(CommandFlow Value)
			{
				if (Value == null)
				{
					throw new System.NullReferenceException();
				}
				Flow_ = Value;

				_bitField |= 0x00000400;
				return this;
			}
			public Builder SetFlow(CommandFlow.Builder BuilderForValue)
			{
				Flow_ = BuilderForValue.build();

				_bitField |= 0x00000400;
				return this;
			}
			public Builder MergeFlow(CommandFlow Value)
			{
				if (((_bitField & 0x00000400) == 0x00000400) && Flow_ != CommandFlow.DefaultInstance)
				{
					Flow_ = CommandFlow.NewBuilder(Flow).mergeFrom(Value).buildPartial();
				}
				else
				{
					Flow_ = Value;
				}

				_bitField |= 0x00000400;
				return this;
			}
			public Builder ClearFlow()
			{
				Flow_ = CommandFlow.DefaultInstance;

				_bitField = (_bitField & ~0x00000400);
				return this;
			}

			// optional .pulsar.proto.CommandUnsubscribe unsubscribe = 12;
			internal CommandUnsubscribe Unsubscribe_ = CommandUnsubscribe.DefaultInstance;
			public bool HasUnsubscribe()
			{
				return ((_bitField & 0x00000800) == 0x00000800);
			}
			public CommandUnsubscribe GetUnsubscribe()
			{
				return Unsubscribe_;
			}
			public Builder SetUnsubscribe(CommandUnsubscribe Value)
			{
				if (Value == null)
				{
					throw new System.NullReferenceException();
				}
				Unsubscribe_ = Value;

				_bitField |= 0x00000800;
				return this;
			}
			public Builder SetUnsubscribe(CommandUnsubscribe.Builder BuilderForValue)
			{
				Unsubscribe_ = BuilderForValue.build();

				_bitField |= 0x00000800;
				return this;
			}
			public Builder MergeUnsubscribe(CommandUnsubscribe Value)
			{
				if (((_bitField & 0x00000800) == 0x00000800) && Unsubscribe_ != CommandUnsubscribe.DefaultInstance)
				{
					Unsubscribe_ = CommandUnsubscribe.NewBuilder(Unsubscribe).mergeFrom(Value).buildPartial();
				}
				else
				{
					Unsubscribe_ = Value;
				}

				_bitField |= 0x00000800;
				return this;
			}
			public Builder ClearUnsubscribe()
			{
				Unsubscribe_ = CommandUnsubscribe.DefaultInstance;

				_bitField = (_bitField & ~0x00000800);
				return this;
			}

			// optional .pulsar.proto.CommandSuccess success = 13;
			internal CommandSuccess Success_ = CommandSuccess.DefaultInstance;
			public bool HasSuccess()
			{
				return ((_bitField & 0x00001000) == 0x00001000);
			}
			public CommandSuccess GetSuccess()
			{
				return Success_;
			}
			public Builder SetSuccess(CommandSuccess Value)
			{
				if (Value == null)
				{
					throw new System.NullReferenceException();
				}
				Success_ = Value;

				_bitField |= 0x00001000;
				return this;
			}
			public Builder SetSuccess(CommandSuccess.Builder BuilderForValue)
			{
				Success_ = BuilderForValue.build();

				_bitField |= 0x00001000;
				return this;
			}
			public Builder MergeSuccess(CommandSuccess Value)
			{
				if (((_bitField & 0x00001000) == 0x00001000) && Success_ != CommandSuccess.DefaultInstance)
				{
					Success_ = CommandSuccess.NewBuilder(Success).mergeFrom(Value).buildPartial();
				}
				else
				{
					Success_ = Value;
				}

				_bitField |= 0x00001000;
				return this;
			}
			public Builder ClearSuccess()
			{
				Success_ = CommandSuccess.DefaultInstance;

				_bitField = (_bitField & ~0x00001000);
				return this;
			}

			// optional .pulsar.proto.CommandError error = 14;
			internal CommandError Error_ = CommandError.DefaultInstance;
			public bool HasError()
			{
				return ((_bitField & 0x00002000) == 0x00002000);
			}
			public CommandError GetError()
			{
				return Error_;
			}
			public Builder SetError(CommandError Value)
			{
				if (Value == null)
				{
					throw new System.NullReferenceException();
				}
				Error_ = Value;

				_bitField |= 0x00002000;
				return this;
			}
			public Builder SetError(CommandError.Builder BuilderForValue)
			{
				Error_ = BuilderForValue.build();

				_bitField |= 0x00002000;
				return this;
			}
			public Builder MergeError(CommandError Value)
			{
				if (((_bitField & 0x00002000) == 0x00002000) && Error_ != CommandError.DefaultInstance)
				{
					Error_ = CommandError.NewBuilder(Error).mergeFrom(Value).buildPartial();
				}
				else
				{
					Error_ = Value;
				}

				_bitField |= 0x00002000;
				return this;
			}
			public Builder ClearError()
			{
				Error_ = CommandError.DefaultInstance;

				_bitField = (_bitField & ~0x00002000);
				return this;
			}

			// optional .pulsar.proto.CommandCloseProducer close_producer = 15;
			internal CommandCloseProducer CloseProducer_ = CommandCloseProducer.DefaultInstance;
			public bool HasCloseProducer()
			{
				return ((_bitField & 0x00004000) == 0x00004000);
			}
			public CommandCloseProducer GetCloseProducer()
			{
				return CloseProducer_;
			}
			public Builder SetCloseProducer(CommandCloseProducer Value)
			{
				if (Value == null)
				{
					throw new System.NullReferenceException();
				}
				CloseProducer_ = Value;

				_bitField |= 0x00004000;
				return this;
			}
			public Builder SetCloseProducer(CommandCloseProducer.Builder BuilderForValue)
			{
				CloseProducer_ = BuilderForValue.build();

				_bitField |= 0x00004000;
				return this;
			}
			public Builder MergeCloseProducer(CommandCloseProducer Value)
			{
				if (((_bitField & 0x00004000) == 0x00004000) && CloseProducer_ != CommandCloseProducer.DefaultInstance)
				{
					CloseProducer_ = CommandCloseProducer.NewBuilder(CloseProducer).mergeFrom(Value).buildPartial();
				}
				else
				{
					CloseProducer_ = Value;
				}

				_bitField |= 0x00004000;
				return this;
			}
			public Builder ClearCloseProducer()
			{
				CloseProducer_ = CommandCloseProducer.DefaultInstance;

				_bitField = (_bitField & ~0x00004000);
				return this;
			}

			// optional .pulsar.proto.CommandCloseConsumer close_consumer = 16;
			internal CommandCloseConsumer CloseConsumer_ = CommandCloseConsumer.DefaultInstance;
			public bool HasCloseConsumer()
			{
				return ((_bitField & 0x00008000) == 0x00008000);
			}
			public CommandCloseConsumer GetCloseConsumer()
			{
				return CloseConsumer_;
			}
			public Builder SetCloseConsumer(CommandCloseConsumer Value)
			{
				if (Value == null)
				{
					throw new System.NullReferenceException();
				}
				CloseConsumer_ = Value;

				_bitField |= 0x00008000;
				return this;
			}
			public Builder SetCloseConsumer(CommandCloseConsumer.Builder BuilderForValue)
			{
				CloseConsumer_ = BuilderForValue.build();

				_bitField |= 0x00008000;
				return this;
			}
			public Builder MergeCloseConsumer(CommandCloseConsumer Value)
			{
				if (((_bitField & 0x00008000) == 0x00008000) && CloseConsumer_ != CommandCloseConsumer.DefaultInstance)
				{
					CloseConsumer_ = CommandCloseConsumer.NewBuilder(CloseConsumer).mergeFrom(Value).buildPartial();
				}
				else
				{
					CloseConsumer_ = Value;
				}

				_bitField |= 0x00008000;
				return this;
			}
			public Builder ClearCloseConsumer()
			{
				CloseConsumer_ = CommandCloseConsumer.DefaultInstance;

				_bitField = (_bitField & ~0x00008000);
				return this;
			}

			// optional .pulsar.proto.CommandProducerSuccess producer_success = 17;
			internal CommandProducerSuccess ProducerSuccess_ = CommandProducerSuccess.DefaultInstance;
			public bool HasProducerSuccess()
			{
				return ((_bitField & 0x00010000) == 0x00010000);
			}
			public CommandProducerSuccess GetProducerSuccess()
			{
				return ProducerSuccess_;
			}
			public Builder SetProducerSuccess(CommandProducerSuccess Value)
			{
				if (Value == null)
				{
					throw new System.NullReferenceException();
				}
				ProducerSuccess_ = Value;

				_bitField |= 0x00010000;
				return this;
			}
			public Builder SetProducerSuccess(CommandProducerSuccess.Builder BuilderForValue)
			{
				ProducerSuccess_ = BuilderForValue.build();

				_bitField |= 0x00010000;
				return this;
			}
			public Builder MergeProducerSuccess(CommandProducerSuccess Value)
			{
				if (((_bitField & 0x00010000) == 0x00010000) && ProducerSuccess_ != CommandProducerSuccess.DefaultInstance)
				{
					ProducerSuccess_ = CommandProducerSuccess.NewBuilder(ProducerSuccess).mergeFrom(Value).buildPartial();
				}
				else
				{
					ProducerSuccess_ = Value;
				}

				_bitField |= 0x00010000;
				return this;
			}
			public Builder ClearProducerSuccess()
			{
				ProducerSuccess_ = CommandProducerSuccess.DefaultInstance;

				_bitField = (_bitField & ~0x00010000);
				return this;
			}

			// optional .pulsar.proto.CommandPing ping = 18;
			internal CommandPing Ping_ = CommandPing.DefaultInstance;
			public bool HasPing()
			{
				return ((_bitField & 0x00020000) == 0x00020000);
			}
			public CommandPing GetPing()
			{
				return Ping_;
			}
			public Builder SetPing(CommandPing Value)
			{
				if (Value == null)
				{
					throw new System.NullReferenceException();
				}
				Ping_ = Value;

				_bitField |= 0x00020000;
				return this;
			}
			public Builder SetPing(CommandPing.Builder BuilderForValue)
			{
				Ping_ = BuilderForValue.build();

				_bitField |= 0x00020000;
				return this;
			}
			public Builder MergePing(CommandPing Value)
			{
				if (((_bitField & 0x00020000) == 0x00020000) && Ping_ != CommandPing.DefaultInstance)
				{
					Ping_ = CommandPing.NewBuilder(Ping).mergeFrom(Value).buildPartial();
				}
				else
				{
					Ping_ = Value;
				}

				_bitField |= 0x00020000;
				return this;
			}
			public Builder ClearPing()
			{
				Ping_ = CommandPing.DefaultInstance;

				_bitField = (_bitField & ~0x00020000);
				return this;
			}

			// optional .pulsar.proto.CommandPong pong = 19;
			internal CommandPong Pong_ = CommandPong.DefaultInstance;
			public bool HasPong()
			{
				return ((_bitField & 0x00040000) == 0x00040000);
			}
			public CommandPong GetPong()
			{
				return Pong_;
			}
			public Builder SetPong(CommandPong Value)
			{
				if (Value == null)
				{
					throw new System.NullReferenceException();
				}
				Pong_ = Value;

				_bitField |= 0x00040000;
				return this;
			}
			public Builder SetPong(CommandPong.Builder BuilderForValue)
			{
				Pong_ = BuilderForValue.build();

				_bitField |= 0x00040000;
				return this;
			}
			public Builder MergePong(CommandPong Value)
			{
				if (((_bitField & 0x00040000) == 0x00040000) && Pong_ != CommandPong.DefaultInstance)
				{
					Pong_ = CommandPong.NewBuilder(Pong).mergeFrom(Value).buildPartial();
				}
				else
				{
					Pong_ = Value;
				}

				_bitField |= 0x00040000;
				return this;
			}
			public Builder ClearPong()
			{
				Pong_ = CommandPong.DefaultInstance;

				_bitField = (_bitField & ~0x00040000);
				return this;
			}

			// optional .pulsar.proto.CommandRedeliverUnacknowledgedMessages redeliverUnacknowledgedMessages = 20;
			internal CommandRedeliverUnacknowledgedMessages RedeliverUnacknowledgedMessages_ = CommandRedeliverUnacknowledgedMessages.DefaultInstance;
			public bool HasRedeliverUnacknowledgedMessages()
			{
				return ((_bitField & 0x00080000) == 0x00080000);
			}
			public CommandRedeliverUnacknowledgedMessages GetRedeliverUnacknowledgedMessages()
			{
				return RedeliverUnacknowledgedMessages_;
			}
			public Builder SetRedeliverUnacknowledgedMessages(CommandRedeliverUnacknowledgedMessages Value)
			{
				if (Value == null)
				{
					throw new System.NullReferenceException();
				}
				RedeliverUnacknowledgedMessages_ = Value;

				_bitField |= 0x00080000;
				return this;
			}
			public Builder SetRedeliverUnacknowledgedMessages(CommandRedeliverUnacknowledgedMessages.Builder BuilderForValue)
			{
				RedeliverUnacknowledgedMessages_ = BuilderForValue.build();

				_bitField |= 0x00080000;
				return this;
			}
			public Builder MergeRedeliverUnacknowledgedMessages(CommandRedeliverUnacknowledgedMessages Value)
			{
				if (((_bitField & 0x00080000) == 0x00080000) && RedeliverUnacknowledgedMessages_ != CommandRedeliverUnacknowledgedMessages.DefaultInstance)
				{
					RedeliverUnacknowledgedMessages_ = CommandRedeliverUnacknowledgedMessages.NewBuilder(RedeliverUnacknowledgedMessages).mergeFrom(Value).buildPartial();
				}
				else
				{
					RedeliverUnacknowledgedMessages_ = Value;
				}

				_bitField |= 0x00080000;
				return this;
			}
			public Builder ClearRedeliverUnacknowledgedMessages()
			{
				RedeliverUnacknowledgedMessages_ = CommandRedeliverUnacknowledgedMessages.DefaultInstance;

				_bitField = (_bitField & ~0x00080000);
				return this;
			}

			// optional .pulsar.proto.CommandPartitionedTopicMetadata partitionMetadata = 21;
			internal CommandPartitionedTopicMetadata PartitionMetadata_ = CommandPartitionedTopicMetadata.DefaultInstance;
			public bool HasPartitionMetadata()
			{
				return ((_bitField & 0x00100000) == 0x00100000);
			}
			public CommandPartitionedTopicMetadata GetPartitionMetadata()
			{
				return PartitionMetadata_;
			}
			public Builder SetPartitionMetadata(CommandPartitionedTopicMetadata Value)
			{
				if (Value == null)
				{
					throw new System.NullReferenceException();
				}
				PartitionMetadata_ = Value;

				_bitField |= 0x00100000;
				return this;
			}
			public Builder SetPartitionMetadata(CommandPartitionedTopicMetadata.Builder BuilderForValue)
			{
				PartitionMetadata_ = BuilderForValue.build();

				_bitField |= 0x00100000;
				return this;
			}
			public Builder MergePartitionMetadata(CommandPartitionedTopicMetadata Value)
			{
				if (((_bitField & 0x00100000) == 0x00100000) && PartitionMetadata_ != CommandPartitionedTopicMetadata.DefaultInstance)
				{
					PartitionMetadata_ = CommandPartitionedTopicMetadata.NewBuilder(PartitionMetadata).mergeFrom(Value).buildPartial();
				}
				else
				{
					PartitionMetadata_ = Value;
				}

				_bitField |= 0x00100000;
				return this;
			}
			public Builder ClearPartitionMetadata()
			{
				PartitionMetadata_ = CommandPartitionedTopicMetadata.DefaultInstance;

				_bitField = (_bitField & ~0x00100000);
				return this;
			}

			// optional .pulsar.proto.CommandPartitionedTopicMetadataResponse partitionMetadataResponse = 22;
			internal CommandPartitionedTopicMetadataResponse PartitionMetadataResponse_ = CommandPartitionedTopicMetadataResponse.DefaultInstance;
			public bool HasPartitionMetadataResponse()
			{
				return ((_bitField & 0x00200000) == 0x00200000);
			}
			public CommandPartitionedTopicMetadataResponse GetPartitionMetadataResponse()
			{
				return PartitionMetadataResponse_;
			}
			public Builder SetPartitionMetadataResponse(CommandPartitionedTopicMetadataResponse Value)
			{
				if (Value == null)
				{
					throw new System.NullReferenceException();
				}
				PartitionMetadataResponse_ = Value;

				_bitField |= 0x00200000;
				return this;
			}
			public Builder SetPartitionMetadataResponse(CommandPartitionedTopicMetadataResponse.Builder BuilderForValue)
			{
				PartitionMetadataResponse_ = BuilderForValue.build();

				_bitField |= 0x00200000;
				return this;
			}
			public Builder MergePartitionMetadataResponse(CommandPartitionedTopicMetadataResponse Value)
			{
				if (((_bitField & 0x00200000) == 0x00200000) && PartitionMetadataResponse_ != CommandPartitionedTopicMetadataResponse.DefaultInstance)
				{
					PartitionMetadataResponse_ = CommandPartitionedTopicMetadataResponse.NewBuilder(PartitionMetadataResponse).mergeFrom(Value).buildPartial();
				}
				else
				{
					PartitionMetadataResponse_ = Value;
				}

				_bitField |= 0x00200000;
				return this;
			}
			public Builder ClearPartitionMetadataResponse()
			{
				PartitionMetadataResponse_ = CommandPartitionedTopicMetadataResponse.DefaultInstance;

				_bitField = (_bitField & ~0x00200000);
				return this;
			}

			// optional .pulsar.proto.CommandLookupTopic lookupTopic = 23;
			internal CommandLookupTopic LookupTopic_ = CommandLookupTopic.DefaultInstance;
			public bool HasLookupTopic()
			{
				return ((_bitField & 0x00400000) == 0x00400000);
			}
			public CommandLookupTopic GetLookupTopic()
			{
				return LookupTopic_;
			}
			public Builder SetLookupTopic(CommandLookupTopic Value)
			{
				if (Value == null)
				{
					throw new System.NullReferenceException();
				}
				LookupTopic_ = Value;

				_bitField |= 0x00400000;
				return this;
			}
			public Builder SetLookupTopic(CommandLookupTopic.Builder BuilderForValue)
			{
				LookupTopic_ = BuilderForValue.build();

				_bitField |= 0x00400000;
				return this;
			}
			public Builder MergeLookupTopic(CommandLookupTopic Value)
			{
				if (((_bitField & 0x00400000) == 0x00400000) && LookupTopic_ != CommandLookupTopic.DefaultInstance)
				{
					LookupTopic_ = CommandLookupTopic.NewBuilder(LookupTopic).mergeFrom(Value).buildPartial();
				}
				else
				{
					LookupTopic_ = Value;
				}

				_bitField |= 0x00400000;
				return this;
			}
			public Builder ClearLookupTopic()
			{
				LookupTopic_ = CommandLookupTopic.DefaultInstance;

				_bitField = (_bitField & ~0x00400000);
				return this;
			}

			// optional .pulsar.proto.CommandLookupTopicResponse lookupTopicResponse = 24;
			internal CommandLookupTopicResponse LookupTopicResponse_ = CommandLookupTopicResponse.DefaultInstance;
			public bool HasLookupTopicResponse()
			{
				return ((_bitField & 0x00800000) == 0x00800000);
			}
			public CommandLookupTopicResponse GetLookupTopicResponse()
			{
				return LookupTopicResponse_;
			}
			public Builder SetLookupTopicResponse(CommandLookupTopicResponse Value)
			{
				if (Value == null)
				{
					throw new System.NullReferenceException();
				}
				LookupTopicResponse_ = Value;

				_bitField |= 0x00800000;
				return this;
			}
			public Builder SetLookupTopicResponse(CommandLookupTopicResponse.Builder BuilderForValue)
			{
				LookupTopicResponse_ = BuilderForValue.build();

				_bitField |= 0x00800000;
				return this;
			}
			public Builder MergeLookupTopicResponse(CommandLookupTopicResponse Value)
			{
				if (((_bitField & 0x00800000) == 0x00800000) && LookupTopicResponse_ != CommandLookupTopicResponse.DefaultInstance)
				{
					LookupTopicResponse_ = CommandLookupTopicResponse.NewBuilder(LookupTopicResponse).mergeFrom(Value).buildPartial();
				}
				else
				{
					LookupTopicResponse_ = Value;
				}

				_bitField |= 0x00800000;
				return this;
			}
			public Builder ClearLookupTopicResponse()
			{
				LookupTopicResponse_ = CommandLookupTopicResponse.DefaultInstance;

				_bitField = (_bitField & ~0x00800000);
				return this;
			}

			// optional .pulsar.proto.CommandConsumerStats consumerStats = 25;
			internal CommandConsumerStats ConsumerStats_ = CommandConsumerStats.DefaultInstance;
			public bool HasConsumerStats()
			{
				return ((_bitField & 0x01000000) == 0x01000000);
			}
			public CommandConsumerStats GetConsumerStats()
			{
				return ConsumerStats_;
			}
			public Builder SetConsumerStats(CommandConsumerStats Value)
			{
				if (Value == null)
				{
					throw new System.NullReferenceException();
				}
				ConsumerStats_ = Value;

				_bitField |= 0x01000000;
				return this;
			}
			public Builder SetConsumerStats(CommandConsumerStats.Builder BuilderForValue)
			{
				ConsumerStats_ = BuilderForValue.build();

				_bitField |= 0x01000000;
				return this;
			}
			public Builder MergeConsumerStats(CommandConsumerStats Value)
			{
				if (((_bitField & 0x01000000) == 0x01000000) && ConsumerStats_ != CommandConsumerStats.DefaultInstance)
				{
					ConsumerStats_ = CommandConsumerStats.NewBuilder(ConsumerStats).mergeFrom(Value).buildPartial();
				}
				else
				{
					ConsumerStats_ = Value;
				}

				_bitField |= 0x01000000;
				return this;
			}
			public Builder ClearConsumerStats()
			{
				ConsumerStats_ = CommandConsumerStats.DefaultInstance;

				_bitField = (_bitField & ~0x01000000);
				return this;
			}

			// optional .pulsar.proto.CommandConsumerStatsResponse consumerStatsResponse = 26;
			internal CommandConsumerStatsResponse ConsumerStatsResponse_ = CommandConsumerStatsResponse.DefaultInstance;
			public bool HasConsumerStatsResponse()
			{
				return ((_bitField & 0x02000000) == 0x02000000);
			}
			public CommandConsumerStatsResponse GetConsumerStatsResponse()
			{
				return ConsumerStatsResponse_;
			}
			public Builder SetConsumerStatsResponse(CommandConsumerStatsResponse Value)
			{
				if (Value == null)
				{
					throw new System.NullReferenceException();
				}
				ConsumerStatsResponse_ = Value;

				_bitField |= 0x02000000;
				return this;
			}
			public Builder SetConsumerStatsResponse(CommandConsumerStatsResponse.Builder BuilderForValue)
			{
				ConsumerStatsResponse_ = BuilderForValue.build();

				_bitField |= 0x02000000;
				return this;
			}
			public Builder MergeConsumerStatsResponse(CommandConsumerStatsResponse Value)
			{
				if (((_bitField & 0x02000000) == 0x02000000) && ConsumerStatsResponse_ != CommandConsumerStatsResponse.DefaultInstance)
				{
					ConsumerStatsResponse_ = CommandConsumerStatsResponse.NewBuilder(ConsumerStatsResponse).mergeFrom(Value).buildPartial();
				}
				else
				{
					ConsumerStatsResponse_ = Value;
				}

				_bitField |= 0x02000000;
				return this;
			}
			public Builder ClearConsumerStatsResponse()
			{
				ConsumerStatsResponse_ = CommandConsumerStatsResponse.DefaultInstance;

				_bitField = (_bitField & ~0x02000000);
				return this;
			}

			// optional .pulsar.proto.CommandReachedEndOfTopic reachedEndOfTopic = 27;
			internal CommandReachedEndOfTopic ReachedEndOfTopic_ = CommandReachedEndOfTopic.DefaultInstance;
			public bool HasReachedEndOfTopic()
			{
				return ((_bitField & 0x04000000) == 0x04000000);
			}
			public CommandReachedEndOfTopic GetReachedEndOfTopic()
			{
				return ReachedEndOfTopic_;
			}
			public Builder SetReachedEndOfTopic(CommandReachedEndOfTopic Value)
			{
				if (Value == null)
				{
					throw new System.NullReferenceException();
				}
				ReachedEndOfTopic_ = Value;

				_bitField |= 0x04000000;
				return this;
			}
			public Builder SetReachedEndOfTopic(CommandReachedEndOfTopic.Builder BuilderForValue)
			{
				ReachedEndOfTopic_ = BuilderForValue.build();

				_bitField |= 0x04000000;
				return this;
			}
			public Builder MergeReachedEndOfTopic(CommandReachedEndOfTopic Value)
			{
				if (((_bitField & 0x04000000) == 0x04000000) && ReachedEndOfTopic_ != CommandReachedEndOfTopic.DefaultInstance)
				{
					ReachedEndOfTopic_ = CommandReachedEndOfTopic.NewBuilder(ReachedEndOfTopic).mergeFrom(Value).buildPartial();
				}
				else
				{
					ReachedEndOfTopic_ = Value;
				}

				_bitField |= 0x04000000;
				return this;
			}
			public Builder ClearReachedEndOfTopic()
			{
				ReachedEndOfTopic_ = CommandReachedEndOfTopic.DefaultInstance;

				_bitField = (_bitField & ~0x04000000);
				return this;
			}

			// optional .pulsar.proto.CommandSeek seek = 28;
			internal CommandSeek Seek_ = CommandSeek.DefaultInstance;
			public bool HasSeek()
			{
				return ((_bitField & 0x08000000) == 0x08000000);
			}
			public CommandSeek GetSeek()
			{
				return Seek_;
			}
			public Builder SetSeek(CommandSeek Value)
			{
				if (Value == null)
				{
					throw new System.NullReferenceException();
				}
				Seek_ = Value;

				_bitField |= 0x08000000;
				return this;
			}
			public Builder SetSeek(CommandSeek.Builder BuilderForValue)
			{
				Seek_ = BuilderForValue.build();

				_bitField |= 0x08000000;
				return this;
			}
			public Builder MergeSeek(CommandSeek Value)
			{
				if (((_bitField & 0x08000000) == 0x08000000) && Seek_ != CommandSeek.DefaultInstance)
				{
					Seek_ = CommandSeek.NewBuilder(Seek).mergeFrom(Value).buildPartial();
				}
				else
				{
					Seek_ = Value;
				}

				_bitField |= 0x08000000;
				return this;
			}
			public Builder ClearSeek()
			{
				Seek_ = CommandSeek.DefaultInstance;

				_bitField = (_bitField & ~0x08000000);
				return this;
			}

			// optional .pulsar.proto.CommandGetLastMessageId GetLastMessageId = 29;
			internal CommandGetLastMessageId GetLastMessageId_ = CommandGetLastMessageId.DefaultInstance;
			public bool HasGetLastMessageId()
			{
				return ((_bitField & 0x10000000) == 0x10000000);
			}
			public CommandGetLastMessageId GetGetLastMessageId()
			{
				return GetLastMessageId_;
			}
			public Builder SetGetLastMessageId(CommandGetLastMessageId Value)
			{
				if (Value == null)
				{
					throw new System.NullReferenceException();
				}
				GetLastMessageId_ = Value;

				_bitField |= 0x10000000;
				return this;
			}
			public Builder SetGetLastMessageId(CommandGetLastMessageId.Builder BuilderForValue)
			{
				GetLastMessageId_ = BuilderForValue.build();

				_bitField |= 0x10000000;
				return this;
			}
			public Builder MergeGetLastMessageId(CommandGetLastMessageId Value)
			{
				if (((_bitField & 0x10000000) == 0x10000000) && GetLastMessageId_ != CommandGetLastMessageId.DefaultInstance)
				{
					GetLastMessageId_ = CommandGetLastMessageId.NewBuilder(GetLastMessageId).mergeFrom(Value).buildPartial();
				}
				else
				{
					GetLastMessageId_ = Value;
				}

				_bitField |= 0x10000000;
				return this;
			}
			public Builder ClearGetLastMessageId()
			{
				GetLastMessageId_ = CommandGetLastMessageId.DefaultInstance;

				_bitField = (_bitField & ~0x10000000);
				return this;
			}

			// optional .pulsar.proto.CommandGetLastMessageIdResponse GetLastMessageIdResponse = 30;
			internal CommandGetLastMessageIdResponse GetLastMessageIdResponse_ = CommandGetLastMessageIdResponse.DefaultInstance;
			public bool HasGetLastMessageIdResponse()
			{
				return ((_bitField & 0x20000000) == 0x20000000);
			}
			public CommandGetLastMessageIdResponse GetGetLastMessageIdResponse()
			{
				return GetLastMessageIdResponse_;
			}
			public Builder SetGetLastMessageIdResponse(CommandGetLastMessageIdResponse Value)
			{
				if (Value == null)
				{
					throw new System.NullReferenceException();
				}
				GetLastMessageIdResponse_ = Value;

				_bitField |= 0x20000000;
				return this;
			}
			public Builder SetGetLastMessageIdResponse(CommandGetLastMessageIdResponse.Builder BuilderForValue)
			{
				GetLastMessageIdResponse_ = BuilderForValue.build();

				_bitField |= 0x20000000;
				return this;
			}
			public Builder MergeGetLastMessageIdResponse(CommandGetLastMessageIdResponse Value)
			{
				if (((_bitField & 0x20000000) == 0x20000000) && GetLastMessageIdResponse_ != CommandGetLastMessageIdResponse.DefaultInstance)
				{
					GetLastMessageIdResponse_ = CommandGetLastMessageIdResponse.NewBuilder(GetLastMessageIdResponse).mergeFrom(Value).buildPartial();
				}
				else
				{
					GetLastMessageIdResponse_ = Value;
				}

				_bitField |= 0x20000000;
				return this;
			}
			public Builder ClearGetLastMessageIdResponse()
			{
				GetLastMessageIdResponse_ = CommandGetLastMessageIdResponse.DefaultInstance;

				_bitField = (_bitField & ~0x20000000);
				return this;
			}

			// optional .pulsar.proto.CommandActiveConsumerChange active_consumer_change = 31;
			internal CommandActiveConsumerChange ActiveConsumerChange_ = CommandActiveConsumerChange.DefaultInstance;
			public bool HasActiveConsumerChange()
			{
				return ((_bitField & 0x40000000) == 0x40000000);
			}
			public CommandActiveConsumerChange GetActiveConsumerChange()
			{
				return ActiveConsumerChange_;
			}
			public Builder SetActiveConsumerChange(CommandActiveConsumerChange Value)
			{
				if (Value == null)
				{
					throw new System.NullReferenceException();
				}
				ActiveConsumerChange_ = Value;

				_bitField |= 0x40000000;
				return this;
			}
			public Builder SetActiveConsumerChange(CommandActiveConsumerChange.Builder BuilderForValue)
			{
				ActiveConsumerChange_ = BuilderForValue.build();

				_bitField |= 0x40000000;
				return this;
			}
			public Builder MergeActiveConsumerChange(CommandActiveConsumerChange Value)
			{
				if (((_bitField & 0x40000000) == 0x40000000) && ActiveConsumerChange_ != CommandActiveConsumerChange.DefaultInstance)
				{
					ActiveConsumerChange_ = CommandActiveConsumerChange.NewBuilder(ActiveConsumerChange).mergeFrom(Value).buildPartial();
				}
				else
				{
					ActiveConsumerChange_ = Value;
				}

				_bitField |= 0x40000000;
				return this;
			}
			public Builder ClearActiveConsumerChange()
			{
				ActiveConsumerChange_ = CommandActiveConsumerChange.DefaultInstance;

				_bitField = (_bitField & ~0x40000000);
				return this;
			}

			// optional .pulsar.proto.CommandGetTopicsOfNamespace GetTopicsOfNamespace = 32;
			internal CommandGetTopicsOfNamespace GetTopicsOfNamespace_ = CommandGetTopicsOfNamespace.DefaultInstance;
			public bool HasGetTopicsOfNamespace()
			{
				return ((_bitField & 0x80000000) == 0x80000000);
			}
			public CommandGetTopicsOfNamespace GetGetTopicsOfNamespace()
			{
				return GetTopicsOfNamespace_;
			}
			public Builder SetGetTopicsOfNamespace(CommandGetTopicsOfNamespace Value)
			{
				if (Value == null)
				{
					throw new System.NullReferenceException();
				}
				GetTopicsOfNamespace_ = Value;

				_bitField |= unchecked((int)0x80000000);
				return this;
			}
			public Builder SetGetTopicsOfNamespace(CommandGetTopicsOfNamespace.Builder BuilderForValue)
			{
				GetTopicsOfNamespace_ = BuilderForValue.build();

				_bitField |= unchecked((int)0x80000000);
				return this;
			}
			public Builder MergeGetTopicsOfNamespace(CommandGetTopicsOfNamespace Value)
			{
				if (((_bitField & 0x80000000) == 0x80000000) && GetTopicsOfNamespace_ != CommandGetTopicsOfNamespace.DefaultInstance)
				{
					GetTopicsOfNamespace_ = CommandGetTopicsOfNamespace.NewBuilder(GetTopicsOfNamespace).mergeFrom(Value).buildPartial();
				}
				else
				{
					GetTopicsOfNamespace_ = Value;
				}

				_bitField |= unchecked((int)0x80000000);
				return this;
			}
			public Builder ClearGetTopicsOfNamespace()
			{
				GetTopicsOfNamespace_ = CommandGetTopicsOfNamespace.DefaultInstance;

				_bitField = (_bitField & ~0x80000000);
				return this;
			}

			// optional .pulsar.proto.CommandGetTopicsOfNamespaceResponse GetTopicsOfNamespaceResponse = 33;
			internal CommandGetTopicsOfNamespaceResponse GetTopicsOfNamespaceResponse_ = CommandGetTopicsOfNamespaceResponse.DefaultInstance;
			public bool HasGetTopicsOfNamespaceResponse()
			{
				return ((_bitField & 0x00000001) == 0x00000001);
			}
			public CommandGetTopicsOfNamespaceResponse GetGetTopicsOfNamespaceResponse()
			{
				return GetTopicsOfNamespaceResponse_;
			}
			public Builder SetGetTopicsOfNamespaceResponse(CommandGetTopicsOfNamespaceResponse Value)
			{
				if (Value == null)
				{
					throw new System.NullReferenceException();
				}
				GetTopicsOfNamespaceResponse_ = Value;

				_bitField |= 0x00000001;
				return this;
			}
			public Builder SetGetTopicsOfNamespaceResponse(CommandGetTopicsOfNamespaceResponse.Builder BuilderForValue)
			{
				GetTopicsOfNamespaceResponse_ = BuilderForValue.build();

				_bitField |= 0x00000001;
				return this;
			}
			public Builder MergeGetTopicsOfNamespaceResponse(CommandGetTopicsOfNamespaceResponse Value)
			{
				if (((_bitField & 0x00000001) == 0x00000001) && GetTopicsOfNamespaceResponse_ != CommandGetTopicsOfNamespaceResponse.DefaultInstance)
				{
					GetTopicsOfNamespaceResponse_ = CommandGetTopicsOfNamespaceResponse.NewBuilder(GetTopicsOfNamespaceResponse).mergeFrom(Value).buildPartial();
				}
				else
				{
					GetTopicsOfNamespaceResponse_ = Value;
				}

				_bitField |= 0x00000001;
				return this;
			}
			public Builder ClearGetTopicsOfNamespaceResponse()
			{
				GetTopicsOfNamespaceResponse_ = CommandGetTopicsOfNamespaceResponse.DefaultInstance;

				_bitField = (_bitField & ~0x00000001);
				return this;
			}

			// optional .pulsar.proto.CommandGetSchema GetSchema = 34;
			internal CommandGetSchema GetSchema_ = CommandGetSchema.DefaultInstance;
			public bool HasGetSchema()
			{
				return ((_bitField & 0x00000002) == 0x00000002);
			}
			public CommandGetSchema GetGetSchema()
			{
				return GetSchema_;
			}
			public Builder SetGetSchema(CommandGetSchema Value)
			{
				if (Value == null)
				{
					throw new System.NullReferenceException();
				}
				GetSchema_ = Value;

				_bitField |= 0x00000002;
				return this;
			}
			public Builder SetGetSchema(CommandGetSchema.Builder BuilderForValue)
			{
				GetSchema_ = BuilderForValue.build();

				_bitField |= 0x00000002;
				return this;
			}
			public Builder MergeGetSchema(CommandGetSchema Value)
			{
				if (((_bitField & 0x00000002) == 0x00000002) && GetSchema_ != CommandGetSchema.DefaultInstance)
				{
					GetSchema_ = CommandGetSchema.NewBuilder(GetSchema).mergeFrom(Value).buildPartial();
				}
				else
				{
					GetSchema_ = Value;
				}

				_bitField |= 0x00000002;
				return this;
			}
			public Builder ClearGetSchema()
			{
				GetSchema_ = CommandGetSchema.DefaultInstance;

				_bitField = (_bitField & ~0x00000002);
				return this;
			}

			// optional .pulsar.proto.CommandGetSchemaResponse GetSchemaResponse = 35;
			internal CommandGetSchemaResponse GetSchemaResponse_ = CommandGetSchemaResponse.DefaultInstance;
			public bool HasGetSchemaResponse()
			{
				return ((_bitField & 0x00000004) == 0x00000004);
			}
			public CommandGetSchemaResponse GetGetSchemaResponse()
			{
				return GetSchemaResponse_;
			}
			public Builder SetGetSchemaResponse(CommandGetSchemaResponse Value)
			{
				if (Value == null)
				{
					throw new System.NullReferenceException();
				}
				GetSchemaResponse_ = Value;

				_bitField |= 0x00000004;
				return this;
			}
			public Builder SetGetSchemaResponse(CommandGetSchemaResponse.Builder BuilderForValue)
			{
				GetSchemaResponse_ = BuilderForValue.build();

				_bitField |= 0x00000004;
				return this;
			}
			public Builder MergeGetSchemaResponse(CommandGetSchemaResponse Value)
			{
				if (((_bitField & 0x00000004) == 0x00000004) && GetSchemaResponse_ != CommandGetSchemaResponse.DefaultInstance)
				{
					GetSchemaResponse_ = CommandGetSchemaResponse.NewBuilder(GetSchemaResponse).mergeFrom(Value).buildPartial();
				}
				else
				{
					GetSchemaResponse_ = Value;
				}

				_bitField |= 0x00000004;
				return this;
			}
			public Builder ClearGetSchemaResponse()
			{
				GetSchemaResponse_ = CommandGetSchemaResponse.DefaultInstance;

				_bitField = (_bitField & ~0x00000004);
				return this;
			}

			// optional .pulsar.proto.CommandAuthChallenge authChallenge = 36;
			internal CommandAuthChallenge AuthChallenge_ = CommandAuthChallenge.DefaultInstance;
			public bool HasAuthChallenge()
			{
				return ((_bitField & 0x00000008) == 0x00000008);
			}
			public CommandAuthChallenge GetAuthChallenge()
			{
				return AuthChallenge_;
			}
			public Builder SetAuthChallenge(CommandAuthChallenge Value)
			{
				if (Value == null)
				{
					throw new System.NullReferenceException();
				}
				AuthChallenge_ = Value;

				_bitField |= 0x00000008;
				return this;
			}
			public Builder SetAuthChallenge(CommandAuthChallenge.Builder BuilderForValue)
			{
				AuthChallenge_ = BuilderForValue.build();

				_bitField |= 0x00000008;
				return this;
			}
			public Builder MergeAuthChallenge(CommandAuthChallenge Value)
			{
				if (((_bitField & 0x00000008) == 0x00000008) && AuthChallenge_ != CommandAuthChallenge.DefaultInstance)
				{
					AuthChallenge_ = CommandAuthChallenge.NewBuilder(AuthChallenge).mergeFrom(Value).buildPartial();
				}
				else
				{
					AuthChallenge_ = Value;
				}

				_bitField |= 0x00000008;
				return this;
			}
			public Builder ClearAuthChallenge()
			{
				AuthChallenge_ = CommandAuthChallenge.DefaultInstance;

				_bitField = (_bitField & ~0x00000008);
				return this;
			}

			// optional .pulsar.proto.CommandAuthResponse authResponse = 37;
			internal CommandAuthResponse AuthResponse_ = CommandAuthResponse.DefaultInstance;
			public bool HasAuthResponse()
			{
				return ((_bitField & 0x00000010) == 0x00000010);
			}
			public CommandAuthResponse GetAuthResponse()
			{
				return AuthResponse_;
			}
			public Builder SetAuthResponse(CommandAuthResponse Value)
			{
				if (Value == null)
				{
					throw new System.NullReferenceException();
				}
				AuthResponse_ = Value;

				_bitField |= 0x00000010;
				return this;
			}
			public Builder SetAuthResponse(CommandAuthResponse.Builder BuilderForValue)
			{
				AuthResponse_ = BuilderForValue.build();

				_bitField |= 0x00000010;
				return this;
			}
			public Builder MergeAuthResponse(CommandAuthResponse Value)
			{
				if (((_bitField & 0x00000010) == 0x00000010) && AuthResponse_ != CommandAuthResponse.DefaultInstance)
				{
					AuthResponse_ = CommandAuthResponse.NewBuilder(AuthResponse).mergeFrom(Value).buildPartial();
				}
				else
				{
					AuthResponse_ = Value;
				}

				_bitField |= 0x00000010;
				return this;
			}
			public Builder ClearAuthResponse()
			{
				AuthResponse_ = CommandAuthResponse.DefaultInstance;

				_bitField = (_bitField & ~0x00000010);
				return this;
			}

			// optional .pulsar.proto.CommandAckResponse ackResponse = 38;
			internal CommandAckResponse AckResponse_ = CommandAckResponse.DefaultInstance;
			public bool HasAckResponse()
			{
				return ((_bitField & 0x00000020) == 0x00000020);
			}
			public CommandAckResponse GetAckResponse()
			{
				return AckResponse_;
			}
			public Builder SetAckResponse(CommandAckResponse Value)
			{
				if (Value == null)
				{
					throw new System.NullReferenceException();
				}
				AckResponse_ = Value;

				_bitField |= 0x00000020;
				return this;
			}
			public Builder SetAckResponse(CommandAckResponse.Builder BuilderForValue)
			{
				AckResponse_ = BuilderForValue.build();

				_bitField |= 0x00000020;
				return this;
			}
			public Builder MergeAckResponse(CommandAckResponse Value)
			{
				if (((_bitField & 0x00000020) == 0x00000020) && AckResponse_ != CommandAckResponse.DefaultInstance)
				{
					AckResponse_ = CommandAckResponse.NewBuilder(AckResponse).mergeFrom(Value).buildPartial();
				}
				else
				{
					AckResponse_ = Value;
				}

				_bitField |= 0x00000020;
				return this;
			}
			public Builder ClearAckResponse()
			{
				AckResponse_ = CommandAckResponse.DefaultInstance;

				_bitField = (_bitField & ~0x00000020);
				return this;
			}

			// optional .pulsar.proto.CommandGetOrCreateSchema GetOrCreateSchema = 39;
			internal CommandGetOrCreateSchema GetOrCreateSchema_ = CommandGetOrCreateSchema.DefaultInstance;
			public bool HasGetOrCreateSchema()
			{
				return ((_bitField & 0x00000040) == 0x00000040);
			}
			public CommandGetOrCreateSchema GetGetOrCreateSchema()
			{
				return GetOrCreateSchema_;
			}
			public Builder SetGetOrCreateSchema(CommandGetOrCreateSchema Value)
			{
				if (Value == null)
				{
					throw new System.NullReferenceException();
				}
				GetOrCreateSchema_ = Value;

				_bitField |= 0x00000040;
				return this;
			}
			public Builder SetGetOrCreateSchema(CommandGetOrCreateSchema.Builder BuilderForValue)
			{
				GetOrCreateSchema_ = BuilderForValue.build();

				_bitField |= 0x00000040;
				return this;
			}
			public Builder MergeGetOrCreateSchema(CommandGetOrCreateSchema Value)
			{
				if (((_bitField & 0x00000040) == 0x00000040) && GetOrCreateSchema_ != CommandGetOrCreateSchema.DefaultInstance)
				{
					GetOrCreateSchema_ = CommandGetOrCreateSchema.NewBuilder(GetOrCreateSchema).mergeFrom(Value).buildPartial();
				}
				else
				{
					GetOrCreateSchema_ = Value;
				}

				_bitField |= 0x00000040;
				return this;
			}
			public Builder ClearGetOrCreateSchema()
			{
				GetOrCreateSchema_ = CommandGetOrCreateSchema.DefaultInstance;

				_bitField = (_bitField & ~0x00000040);
				return this;
			}

			// optional .pulsar.proto.CommandGetOrCreateSchemaResponse GetOrCreateSchemaResponse = 40;
			internal CommandGetOrCreateSchemaResponse GetOrCreateSchemaResponse_ = CommandGetOrCreateSchemaResponse.DefaultInstance;
			public bool HasGetOrCreateSchemaResponse()
			{
				return ((_bitField & 0x00000080) == 0x00000080);
			}
			public CommandGetOrCreateSchemaResponse GetGetOrCreateSchemaResponse()
			{
				return GetOrCreateSchemaResponse_;
			}
			public Builder SetGetOrCreateSchemaResponse(CommandGetOrCreateSchemaResponse Value)
			{
				if (Value == null)
				{
					throw new System.NullReferenceException();
				}
				GetOrCreateSchemaResponse_ = Value;

				_bitField |= 0x00000080;
				return this;
			}
			public Builder SetGetOrCreateSchemaResponse(CommandGetOrCreateSchemaResponse.Builder BuilderForValue)
			{
				GetOrCreateSchemaResponse_ = BuilderForValue.build();

				_bitField |= 0x00000080;
				return this;
			}
			public Builder MergeGetOrCreateSchemaResponse(CommandGetOrCreateSchemaResponse Value)
			{
				if (((_bitField & 0x00000080) == 0x00000080) && GetOrCreateSchemaResponse_ != CommandGetOrCreateSchemaResponse.DefaultInstance)
				{
					GetOrCreateSchemaResponse_ = CommandGetOrCreateSchemaResponse.NewBuilder(GetOrCreateSchemaResponse).mergeFrom(Value).buildPartial();
				}
				else
				{
					GetOrCreateSchemaResponse_ = Value;
				}

				_bitField |= 0x00000080;
				return this;
			}
			public Builder ClearGetOrCreateSchemaResponse()
			{
				GetOrCreateSchemaResponse_ = CommandGetOrCreateSchemaResponse.DefaultInstance;

				_bitField = (_bitField & ~0x00000080);
				return this;
			}

			// optional .pulsar.proto.CommandNewTxn newTxn = 50;
			internal CommandNewTxn NewTxn_ = CommandNewTxn.DefaultInstance;
			public bool HasNewTxn()
			{
				return ((_bitField & 0x00000100) == 0x00000100);
			}
			public CommandNewTxn GetNewTxn()
			{
				return NewTxn_;
			}
			public Builder SetNewTxn(CommandNewTxn Value)
			{
				if (Value == null)
				{
					throw new System.NullReferenceException();
				}
				NewTxn_ = Value;

				_bitField |= 0x00000100;
				return this;
			}
			public Builder SetNewTxn(CommandNewTxn.Builder BuilderForValue)
			{
				NewTxn_ = BuilderForValue.build();

				_bitField |= 0x00000100;
				return this;
			}
			public Builder MergeNewTxn(CommandNewTxn Value)
			{
				if (((_bitField & 0x00000100) == 0x00000100) && NewTxn_ != CommandNewTxn.DefaultInstance)
				{
					NewTxn_ = CommandNewTxn.NewBuilder(NewTxn).mergeFrom(Value).buildPartial();
				}
				else
				{
					NewTxn_ = Value;
				}

				_bitField |= 0x00000100;
				return this;
			}
			public Builder ClearNewTxn()
			{
				NewTxn_ = CommandNewTxn.DefaultInstance;

				_bitField = (_bitField & ~0x00000100);
				return this;
			}

			// optional .pulsar.proto.CommandNewTxnResponse newTxnResponse = 51;
			internal CommandNewTxnResponse NewTxnResponse_ = CommandNewTxnResponse.DefaultInstance;
			public bool HasNewTxnResponse()
			{
				return ((_bitField & 0x00000200) == 0x00000200);
			}
			public CommandNewTxnResponse GetNewTxnResponse()
			{
				return NewTxnResponse_;
			}
			public Builder SetNewTxnResponse(CommandNewTxnResponse Value)
			{
				if (Value == null)
				{
					throw new System.NullReferenceException();
				}
				NewTxnResponse_ = Value;

				_bitField |= 0x00000200;
				return this;
			}
			public Builder SetNewTxnResponse(CommandNewTxnResponse.Builder BuilderForValue)
			{
				NewTxnResponse_ = BuilderForValue.build();

				_bitField |= 0x00000200;
				return this;
			}
			public Builder MergeNewTxnResponse(CommandNewTxnResponse Value)
			{
				if (((_bitField & 0x00000200) == 0x00000200) && NewTxnResponse_ != CommandNewTxnResponse.DefaultInstance)
				{
					NewTxnResponse_ = CommandNewTxnResponse.NewBuilder(NewTxnResponse).mergeFrom(Value).buildPartial();
				}
				else
				{
					NewTxnResponse_ = Value;
				}

				_bitField |= 0x00000200;
				return this;
			}
			public Builder ClearNewTxnResponse()
			{
				NewTxnResponse_ = CommandNewTxnResponse.DefaultInstance;

				_bitField = (_bitField & ~0x00000200);
				return this;
			}

			// optional .pulsar.proto.CommandAddPartitionToTxn addPartitionToTxn = 52;
			internal CommandAddPartitionToTxn AddPartitionToTxn_ = CommandAddPartitionToTxn.DefaultInstance;
			public bool HasAddPartitionToTxn()
			{
				return ((_bitField & 0x00000400) == 0x00000400);
			}
			public CommandAddPartitionToTxn GetAddPartitionToTxn()
			{
				return AddPartitionToTxn_;
			}
			public Builder SetAddPartitionToTxn(CommandAddPartitionToTxn Value)
			{
				if (Value == null)
				{
					throw new System.NullReferenceException();
				}
				AddPartitionToTxn_ = Value;

				_bitField |= 0x00000400;
				return this;
			}
			public Builder SetAddPartitionToTxn(CommandAddPartitionToTxn.Builder BuilderForValue)
			{
				AddPartitionToTxn_ = BuilderForValue.build();

				_bitField |= 0x00000400;
				return this;
			}
			public Builder MergeAddPartitionToTxn(CommandAddPartitionToTxn Value)
			{
				if (((_bitField & 0x00000400) == 0x00000400) && AddPartitionToTxn_ != CommandAddPartitionToTxn.DefaultInstance)
				{
					AddPartitionToTxn_ = CommandAddPartitionToTxn.NewBuilder(AddPartitionToTxn).mergeFrom(Value).buildPartial();
				}
				else
				{
					AddPartitionToTxn_ = Value;
				}

				_bitField |= 0x00000400;
				return this;
			}
			public Builder ClearAddPartitionToTxn()
			{
				AddPartitionToTxn_ = CommandAddPartitionToTxn.DefaultInstance;

				_bitField = (_bitField & ~0x00000400);
				return this;
			}

			// optional .pulsar.proto.CommandAddPartitionToTxnResponse addPartitionToTxnResponse = 53;
			internal CommandAddPartitionToTxnResponse AddPartitionToTxnResponse_ = CommandAddPartitionToTxnResponse.DefaultInstance;
			public bool HasAddPartitionToTxnResponse()
			{
				return ((_bitField & 0x00000800) == 0x00000800);
			}
			public CommandAddPartitionToTxnResponse GetAddPartitionToTxnResponse()
			{
				return AddPartitionToTxnResponse_;
			}
			public Builder SetAddPartitionToTxnResponse(CommandAddPartitionToTxnResponse Value)
			{
				if (Value == null)
				{
					throw new System.NullReferenceException();
				}
				AddPartitionToTxnResponse_ = Value;

				_bitField |= 0x00000800;
				return this;
			}
			public Builder SetAddPartitionToTxnResponse(CommandAddPartitionToTxnResponse.Builder BuilderForValue)
			{
				AddPartitionToTxnResponse_ = BuilderForValue.build();

				_bitField |= 0x00000800;
				return this;
			}
			public Builder MergeAddPartitionToTxnResponse(CommandAddPartitionToTxnResponse Value)
			{
				if (((_bitField & 0x00000800) == 0x00000800) && AddPartitionToTxnResponse_ != CommandAddPartitionToTxnResponse.DefaultInstance)
				{
					AddPartitionToTxnResponse_ = CommandAddPartitionToTxnResponse.NewBuilder(AddPartitionToTxnResponse).mergeFrom(Value).buildPartial();
				}
				else
				{
					AddPartitionToTxnResponse_ = Value;
				}

				_bitField |= 0x00000800;
				return this;
			}
			public Builder ClearAddPartitionToTxnResponse()
			{
				AddPartitionToTxnResponse_ = CommandAddPartitionToTxnResponse.DefaultInstance;

				_bitField = (_bitField & ~0x00000800);
				return this;
			}

			// optional .pulsar.proto.CommandAddSubscriptionToTxn addSubscriptionToTxn = 54;
			internal CommandAddSubscriptionToTxn AddSubscriptionToTxn_ = CommandAddSubscriptionToTxn.DefaultInstance;
			public bool HasAddSubscriptionToTxn()
			{
				return ((_bitField & 0x00001000) == 0x00001000);
			}
			public CommandAddSubscriptionToTxn GetAddSubscriptionToTxn()
			{
				return AddSubscriptionToTxn_;
			}
			public Builder SetAddSubscriptionToTxn(CommandAddSubscriptionToTxn Value)
			{
				if (Value == null)
				{
					throw new System.NullReferenceException();
				}
				AddSubscriptionToTxn_ = Value;

				_bitField |= 0x00001000;
				return this;
			}
			public Builder SetAddSubscriptionToTxn(CommandAddSubscriptionToTxn.Builder BuilderForValue)
			{
				AddSubscriptionToTxn_ = BuilderForValue.build();

				_bitField |= 0x00001000;
				return this;
			}
			public Builder MergeAddSubscriptionToTxn(CommandAddSubscriptionToTxn Value)
			{
				if (((_bitField & 0x00001000) == 0x00001000) && AddSubscriptionToTxn_ != CommandAddSubscriptionToTxn.DefaultInstance)
				{
					AddSubscriptionToTxn_ = CommandAddSubscriptionToTxn.NewBuilder(AddSubscriptionToTxn).mergeFrom(Value).buildPartial();
				}
				else
				{
					AddSubscriptionToTxn_ = Value;
				}

				_bitField |= 0x00001000;
				return this;
			}
			public Builder ClearAddSubscriptionToTxn()
			{
				AddSubscriptionToTxn_ = CommandAddSubscriptionToTxn.DefaultInstance;

				_bitField = (_bitField & ~0x00001000);
				return this;
			}

			// optional .pulsar.proto.CommandAddSubscriptionToTxnResponse addSubscriptionToTxnResponse = 55;
			internal CommandAddSubscriptionToTxnResponse AddSubscriptionToTxnResponse_ = CommandAddSubscriptionToTxnResponse.DefaultInstance;
			public bool HasAddSubscriptionToTxnResponse()
			{
				return ((_bitField & 0x00002000) == 0x00002000);
			}
			public CommandAddSubscriptionToTxnResponse GetAddSubscriptionToTxnResponse()
			{
				return AddSubscriptionToTxnResponse_;
			}
			public Builder SetAddSubscriptionToTxnResponse(CommandAddSubscriptionToTxnResponse Value)
			{
				if (Value == null)
				{
					throw new System.NullReferenceException();
				}
				AddSubscriptionToTxnResponse_ = Value;

				_bitField |= 0x00002000;
				return this;
			}
			public Builder SetAddSubscriptionToTxnResponse(CommandAddSubscriptionToTxnResponse.Builder BuilderForValue)
			{
				AddSubscriptionToTxnResponse_ = BuilderForValue.build();

				_bitField |= 0x00002000;
				return this;
			}
			public Builder MergeAddSubscriptionToTxnResponse(CommandAddSubscriptionToTxnResponse Value)
			{
				if (((_bitField & 0x00002000) == 0x00002000) && AddSubscriptionToTxnResponse_ != CommandAddSubscriptionToTxnResponse.DefaultInstance)
				{
					AddSubscriptionToTxnResponse_ = CommandAddSubscriptionToTxnResponse.NewBuilder(AddSubscriptionToTxnResponse).mergeFrom(Value).buildPartial();
				}
				else
				{
					AddSubscriptionToTxnResponse_ = Value;
				}

				_bitField |= 0x00002000;
				return this;
			}
			public Builder ClearAddSubscriptionToTxnResponse()
			{
				AddSubscriptionToTxnResponse_ = CommandAddSubscriptionToTxnResponse.DefaultInstance;

				_bitField = (_bitField & ~0x00002000);
				return this;
			}

			// optional .pulsar.proto.CommandEndTxn endTxn = 56;
			internal CommandEndTxn EndTxn_ = CommandEndTxn.DefaultInstance;
			public bool HasEndTxn()
			{
				return ((_bitField & 0x00004000) == 0x00004000);
			}
			public CommandEndTxn GetEndTxn()
			{
				return EndTxn_;
			}
			public Builder SetEndTxn(CommandEndTxn Value)
			{
				if (Value == null)
				{
					throw new System.NullReferenceException();
				}
				EndTxn_ = Value;

				_bitField |= 0x00004000;
				return this;
			}
			public Builder SetEndTxn(CommandEndTxn.Builder BuilderForValue)
			{
				EndTxn_ = BuilderForValue.build();

				_bitField |= 0x00004000;
				return this;
			}
			public Builder MergeEndTxn(CommandEndTxn Value)
			{
				if (((_bitField & 0x00004000) == 0x00004000) && EndTxn_ != CommandEndTxn.DefaultInstance)
				{
					EndTxn_ = CommandEndTxn.NewBuilder(EndTxn).mergeFrom(Value).buildPartial();
				}
				else
				{
					EndTxn_ = Value;
				}

				_bitField |= 0x00004000;
				return this;
			}
			public Builder ClearEndTxn()
			{
				EndTxn_ = CommandEndTxn.DefaultInstance;

				_bitField = (_bitField & ~0x00004000);
				return this;
			}

			// optional .pulsar.proto.CommandEndTxnResponse endTxnResponse = 57;
			internal CommandEndTxnResponse EndTxnResponse_ = CommandEndTxnResponse.DefaultInstance;
			public bool HasEndTxnResponse()
			{
				return ((_bitField & 0x00008000) == 0x00008000);
			}
			public CommandEndTxnResponse GetEndTxnResponse()
			{
				return EndTxnResponse_;
			}
			public Builder SetEndTxnResponse(CommandEndTxnResponse Value)
			{
				if (Value == null)
				{
					throw new System.NullReferenceException();
				}
				EndTxnResponse_ = Value;

				_bitField |= 0x00008000;
				return this;
			}
			public Builder SetEndTxnResponse(CommandEndTxnResponse.Builder BuilderForValue)
			{
				EndTxnResponse_ = BuilderForValue.build();

				_bitField |= 0x00008000;
				return this;
			}
			public Builder MergeEndTxnResponse(CommandEndTxnResponse Value)
			{
				if (((_bitField & 0x00008000) == 0x00008000) && EndTxnResponse_ != CommandEndTxnResponse.DefaultInstance)
				{
					EndTxnResponse_ = CommandEndTxnResponse.NewBuilder(EndTxnResponse_).mergeFrom(Value).buildPartial();
				}
				else
				{
					EndTxnResponse_ = Value;
				}

				_bitField |= 0x00008000;
				return this;
			}
			public Builder ClearEndTxnResponse()
			{
				EndTxnResponse_ = CommandEndTxnResponse.DefaultInstance;

				_bitField = (_bitField & ~0x00008000);
				return this;
			}

			// optional .pulsar.proto.CommandEndTxnOnPartition endTxnOnPartition = 58;
			internal CommandEndTxnOnPartition EndTxnOnPartition_ = CommandEndTxnOnPartition.DefaultInstance;
			public bool HasEndTxnOnPartition()
			{
				return ((_bitField & 0x00010000) == 0x00010000);
			}
			public CommandEndTxnOnPartition GetEndTxnOnPartition()
			{
				return EndTxnOnPartition_;
			}
			public Builder SetEndTxnOnPartition(CommandEndTxnOnPartition Value)
			{
				if (Value == null)
				{
					throw new System.NullReferenceException();
				}
				EndTxnOnPartition_ = Value;

				_bitField |= 0x00010000;
				return this;
			}
			public Builder SetEndTxnOnPartition(CommandEndTxnOnPartition.Builder BuilderForValue)
			{
				EndTxnOnPartition_ = BuilderForValue.build();

				_bitField |= 0x00010000;
				return this;
			}
			public Builder MergeEndTxnOnPartition(CommandEndTxnOnPartition Value)
			{
				if (((_bitField & 0x00010000) == 0x00010000) && EndTxnOnPartition_ != CommandEndTxnOnPartition.DefaultInstance)
				{
					EndTxnOnPartition_ = CommandEndTxnOnPartition.NewBuilder(EndTxnOnPartition_).mergeFrom(Value).buildPartial();
				}
				else
				{
					EndTxnOnPartition_ = Value;
				}

				_bitField |= 0x00010000;
				return this;
			}
			public Builder ClearEndTxnOnPartition()
			{
				EndTxnOnPartition_ = CommandEndTxnOnPartition.DefaultInstance;

				_bitField = (_bitField & ~0x00010000);
				return this;
			}

			// optional .pulsar.proto.CommandEndTxnOnPartitionResponse endTxnOnPartitionResponse = 59;
			internal CommandEndTxnOnPartitionResponse EndTxnOnPartitionResponse_ = CommandEndTxnOnPartitionResponse.DefaultInstance;
			public bool HasEndTxnOnPartitionResponse()
			{
				return ((_bitField & 0x00020000) == 0x00020000);
			}
			public CommandEndTxnOnPartitionResponse GetEndTxnOnPartitionResponse()
			{
				return EndTxnOnPartitionResponse_;
			}
			public Builder SetEndTxnOnPartitionResponse(CommandEndTxnOnPartitionResponse Value)
			{
				if (Value == null)
				{
					throw new System.NullReferenceException();
				}
				EndTxnOnPartitionResponse_ = Value;

				_bitField |= 0x00020000;
				return this;
			}
			public Builder SetEndTxnOnPartitionResponse(CommandEndTxnOnPartitionResponse.Builder BuilderForValue)
			{
				EndTxnOnPartitionResponse_ = BuilderForValue.build();

				_bitField |= 0x00020000;
				return this;
			}
			public Builder MergeEndTxnOnPartitionResponse(CommandEndTxnOnPartitionResponse Value)
			{
				if (((_bitField & 0x00020000) == 0x00020000) && EndTxnOnPartitionResponse_ != CommandEndTxnOnPartitionResponse.DefaultInstance)
				{
					EndTxnOnPartitionResponse_ = CommandEndTxnOnPartitionResponse.NewBuilder(EndTxnOnPartitionResponse).mergeFrom(Value).buildPartial();
				}
				else
				{
					EndTxnOnPartitionResponse_ = Value;
				}

				_bitField |= 0x00020000;
				return this;
			}
			public Builder ClearEndTxnOnPartitionResponse()
			{
				EndTxnOnPartitionResponse_ = CommandEndTxnOnPartitionResponse.DefaultInstance;

				_bitField = (_bitField & ~0x00020000);
				return this;
			}

			// optional .pulsar.proto.CommandEndTxnOnSubscription endTxnOnSubscription = 60;
			internal CommandEndTxnOnSubscription _endTxnOnSubscription = CommandEndTxnOnSubscription.DefaultInstance;
			public bool HasEndTxnOnSubscription()
			{
				return ((_bitField & 0x00040000) == 0x00040000);
			}
			public CommandEndTxnOnSubscription GetEndTxnOnSubscription()
			{
				return _endTxnOnSubscription;
			}
			public Builder SetEndTxnOnSubscription(CommandEndTxnOnSubscription Value)
			{
				if (Value == null)
				{
					throw new System.NullReferenceException();
				}
				_endTxnOnSubscription = Value;

				_bitField |= 0x00040000;
				return this;
			}
			public Builder SetEndTxnOnSubscription(CommandEndTxnOnSubscription.Builder BuilderForValue)
			{
				_endTxnOnSubscription = BuilderForValue.build();

				_bitField |= 0x00040000;
				return this;
			}
			public Builder MergeEndTxnOnSubscription(CommandEndTxnOnSubscription Value)
			{
				if (((_bitField & 0x00040000) == 0x00040000) && _endTxnOnSubscription != CommandEndTxnOnSubscription.DefaultInstance)
				{
					_endTxnOnSubscription = CommandEndTxnOnSubscription.NewBuilder(_endTxnOnSubscription).mergeFrom(Value).buildPartial();
				}
				else
				{
					_endTxnOnSubscription = Value;
				}

				_bitField |= 0x00040000;
				return this;
			}
			public Builder ClearEndTxnOnSubscription()
			{
				_endTxnOnSubscription = CommandEndTxnOnSubscription.DefaultInstance;

				_bitField = (_bitField & ~0x00040000);
				return this;
			}

			
			internal CommandEndTxnOnSubscriptionResponse _endTxnOnSubscriptionResponse = CommandEndTxnOnSubscriptionResponse.DefaultInstance;
			public bool HasEndTxnOnSubscriptionResponse()
			{
				return ((_bitField & 0x00080000) == 0x00080000);
			}
			public CommandEndTxnOnSubscriptionResponse GetEndTxnOnSubscriptionResponse()
			{
				return _endTxnOnSubscriptionResponse;
			}
			public Builder SetEndTxnOnSubscriptionResponse(CommandEndTxnOnSubscriptionResponse Value)
			{
				if (Value == null)
				{
					throw new System.NullReferenceException();
				}
				_endTxnOnSubscriptionResponse = Value;

				_bitField |= 0x00080000;
				return this;
			}
			public Builder SetEndTxnOnSubscriptionResponse(CommandEndTxnOnSubscriptionResponse.Builder BuilderForValue)
			{
				_endTxnOnSubscriptionResponse = BuilderForValue.build();

				_bitField |= 0x00080000;
				return this;
			}
			public Builder MergeEndTxnOnSubscriptionResponse(CommandEndTxnOnSubscriptionResponse Value)
			{
				if (((_bitField & 0x00080000) == 0x00080000) && _endTxnOnSubscriptionResponse != CommandEndTxnOnSubscriptionResponse.DefaultInstance)
				{
					_endTxnOnSubscriptionResponse = CommandEndTxnOnSubscriptionResponse.NewBuilder(_endTxnOnSubscriptionResponse).mergeFrom(Value).buildPartial();
				}
				else
				{
					_endTxnOnSubscriptionResponse = Value;
				}

				_bitField |= 0x00080000;
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
