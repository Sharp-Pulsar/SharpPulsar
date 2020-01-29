using DotNetty.Common;
using Google.Protobuf;
using SharpPulsar.Util.Protobuf;
using System;
using System.Collections.Generic;
using System.Text;

namespace SharpPulsar.Protocol.Proto
{
	public partial class BaseCommand : ByteBufCodedOutputStream.ByteBufGeneratedMessage
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
			this._bitField = 0;
			this._bitField1 = 0;
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

		public sealed class Type
		{
			public static readonly Type CONNECT = new Type("CONNECT", InnerEnum.CONNECT, 0, 2);
			public static readonly Type CONNECTED = new Type("CONNECTED", InnerEnum.CONNECTED, 1, 3);
			public static readonly Type SUBSCRIBE = new Type("SUBSCRIBE", InnerEnum.SUBSCRIBE, 2, 4);
			public static readonly Type PRODUCER = new Type("PRODUCER", InnerEnum.PRODUCER, 3, 5);
			public static readonly Type SEND = new Type("SEND", InnerEnum.SEND, 4, 6);
			public static readonly Type SendReceipt = new Type("SendReceipt", InnerEnum.SendReceipt, 5, 7);
			public static readonly Type SendError = new Type("SendError", InnerEnum.SendError, 6, 8);
			public static readonly Type MESSAGE = new Type("MESSAGE", InnerEnum.MESSAGE, 7, 9);
			public static readonly Type ACK = new Type("ACK", InnerEnum.ACK, 8, 10);
			public static readonly Type FLOW = new Type("FLOW", InnerEnum.FLOW, 9, 11);
			public static readonly Type UNSUBSCRIBE = new Type("UNSUBSCRIBE", InnerEnum.UNSUBSCRIBE, 10, 12);
			public static readonly Type SUCCESS = new Type("SUCCESS", InnerEnum.SUCCESS, 11, 13);
			public static readonly Type ERROR = new Type("ERROR", InnerEnum.ERROR, 12, 14);
			public static readonly Type CloseProducer = new Type("CloseProducer", InnerEnum.CloseProducer, 13, 15);
			public static readonly Type CloseConsumer = new Type("CloseConsumer", InnerEnum.CloseConsumer, 14, 16);
			public static readonly Type ProducerSuccess = new Type("ProducerSuccess", InnerEnum.ProducerSuccess, 15, 17);
			public static readonly Type PING = new Type("PING", InnerEnum.PING, 16, 18);
			public static readonly Type PONG = new Type("PONG", InnerEnum.PONG, 17, 19);
			public static readonly Type RedeliverUnacknowledgedMessages = new Type("RedeliverUnacknowledgedMessages", InnerEnum.RedeliverUnacknowledgedMessages, 18, 20);
			public static readonly Type PartitionedMetadata = new Type("PartitionedMetadata", InnerEnum.PartitionedMetadata, 19, 21);
			public static readonly Type PartitionedMetadataResponse = new Type("PartitionedMetadataResponse", InnerEnum.PartitionedMetadataResponse, 20, 22);
			public static readonly Type LOOKUP = new Type("LOOKUP", InnerEnum.LOOKUP, 21, 23);
			public static readonly Type LookupResponse = new Type("LookupResponse", InnerEnum.LookupResponse, 22, 24);
			public static readonly Type ConsumerStats = new Type("ConsumerStats", InnerEnum.ConsumerStats, 23, 25);
			public static readonly Type ConsumerStatsResponse = new Type("ConsumerStatsResponse", InnerEnum.ConsumerStatsResponse, 24, 26);
			public static readonly Type ReachedEndOfTopic = new Type("ReachedEndOfTopic", InnerEnum.ReachedEndOfTopic, 25, 27);
			public static readonly Type SEEK = new Type("SEEK", InnerEnum.SEEK, 26, 28);
			public static readonly Type GetLastMessageId = new Type("GetLastMessageId", InnerEnum.GetLastMessageId, 27, 29);
			public static readonly Type GetLastMessageIdResponse = new Type("GetLastMessageIdResponse", InnerEnum.GetLastMessageIdResponse, 28, 30);
			public static readonly Type ActiveConsumerChange = new Type("ActiveConsumerChange", InnerEnum.ActiveConsumerChange, 29, 31);
			public static readonly Type GetTopicsOfNamespace = new Type("GetTopicsOfNamespace", InnerEnum.GetTopicsOfNamespace, 30, 32);
			public static readonly Type GetTopicsOfNamespaceResponse = new Type("GetTopicsOfNamespaceResponse", InnerEnum.GetTopicsOfNamespaceResponse, 31, 33);
			public static readonly Type GetSchema = new Type("GetSchema", InnerEnum.GetSchema, 32, 34);
			public static readonly Type GetSchemaResponse = new Type("GetSchemaResponse", InnerEnum.GetSchemaResponse, 33, 35);
			public static readonly Type AuthChallenge = new Type("AuthChallenge", InnerEnum.AuthChallenge, 34, 36);
			public static readonly Type AuthResponse = new Type("AuthResponse", InnerEnum.AuthResponse, 35, 37);
			public static readonly Type AckResponse = new Type("AckResponse", InnerEnum.AckResponse, 36, 38);
			public static readonly Type GetOrCreateSchema = new Type("GetOrCreateSchema", InnerEnum.GetOrCreateSchema, 37, 39);
			public static readonly Type GetOrCreateSchemaResponse = new Type("GetOrCreateSchemaResponse", InnerEnum.GetOrCreateSchemaResponse, 38, 40);
			public static readonly Type NewTxn = new Type("NewTxn", InnerEnum.NewTxn, 39, 50);
			public static readonly Type NewTxnResponse = new Type("NewTxnResponse", InnerEnum.NewTxnResponse, 40, 51);
			public static readonly Type AddPartitionToTxn = new Type("AddPartitionToTxn", InnerEnum.AddPartitionToTxn, 41, 52);
			public static readonly Type AddPartitionToTxnResponse = new Type("AddPartitionToTxnResponse", InnerEnum.AddPartitionToTxnResponse, 42, 53);
			public static readonly Type AddSubscriptionToTxn = new Type("AddSubscriptionToTxn", InnerEnum.AddSubscriptionToTxn, 43, 54);
			public static readonly Type AddSubscriptionToTxnResponse = new Type("AddSubscriptionToTxnResponse", InnerEnum.AddSubscriptionToTxnResponse, 44, 55);
			public static readonly Type EndTxn = new Type("EndTxn", InnerEnum.EndTxn, 45, 56);
			public static readonly Type EndTxnResponse = new Type("EndTxnResponse", InnerEnum.EndTxnResponse, 46, 57);
			public static readonly Type EndTxnOnPartition = new Type("EndTxnOnPartition", InnerEnum.EndTxnOnPartition, 47, 58);
			public static readonly Type EndTxnOnPartitionResponse = new Type("EndTxnOnPartitionResponse", InnerEnum.EndTxnOnPartitionResponse, 48, 59);
			public static readonly Type EndTxnOnSubscription = new Type("EndTxnOnSubscription", InnerEnum.EndTxnOnSubscription, 49, 60);
			public static readonly Type EndTxnOnSubscriptionResponse = new Type("EndTxnOnSubscriptionResponse", InnerEnum.EndTxnOnSubscriptionResponse, 50, 61);

			private static readonly IList<Type> valueList = new List<Type>();

			static Type()
			{
				valueList.Add(CONNECT);
				valueList.Add(CONNECTED);
				valueList.Add(SUBSCRIBE);
				valueList.Add(PRODUCER);
				valueList.Add(SEND);
				valueList.Add(SendReceipt);
				valueList.Add(SendError);
				valueList.Add(MESSAGE);
				valueList.Add(ACK);
				valueList.Add(FLOW);
				valueList.Add(UNSUBSCRIBE);
				valueList.Add(SUCCESS);
				valueList.Add(ERROR);
				valueList.Add(CloseProducer);
				valueList.Add(CloseConsumer);
				valueList.Add(ProducerSuccess);
				valueList.Add(PING);
				valueList.Add(PONG);
				valueList.Add(RedeliverUnacknowledgedMessages);
				valueList.Add(PartitionedMetadata);
				valueList.Add(PartitionedMetadataResponse);
				valueList.Add(LOOKUP);
				valueList.Add(LookupResponse);
				valueList.Add(ConsumerStats);
				valueList.Add(ConsumerStatsResponse);
				valueList.Add(ReachedEndOfTopic);
				valueList.Add(SEEK);
				valueList.Add(GetLastMessageId);
				valueList.Add(GetLastMessageIdResponse);
				valueList.Add(ActiveConsumerChange);
				valueList.Add(GetTopicsOfNamespace);
				valueList.Add(GetTopicsOfNamespaceResponse);
				valueList.Add(GetSchema);
				valueList.Add(GetSchemaResponse);
				valueList.Add(AuthChallenge);
				valueList.Add(AuthResponse);
				valueList.Add(AckResponse);
				valueList.Add(GetOrCreateSchema);
				valueList.Add(GetOrCreateSchemaResponse);
				valueList.Add(NewTxn);
				valueList.Add(NewTxnResponse);
				valueList.Add(AddPartitionToTxn);
				valueList.Add(AddPartitionToTxnResponse);
				valueList.Add(AddSubscriptionToTxn);
				valueList.Add(AddSubscriptionToTxnResponse);
				valueList.Add(EndTxn);
				valueList.Add(EndTxnResponse);
				valueList.Add(EndTxnOnPartition);
				valueList.Add(EndTxnOnPartitionResponse);
				valueList.Add(EndTxnOnSubscription);
				valueList.Add(EndTxnOnSubscriptionResponse);
			}

			public enum InnerEnum
			{
				CONNECT,
				CONNECTED,
				SUBSCRIBE,
				PRODUCER,
				SEND,
				SendReceipt,
				SendError,
				MESSAGE,
				ACK,
				FLOW,
				UNSUBSCRIBE,
				SUCCESS,
				ERROR,
				CloseProducer,
				CloseConsumer,
				ProducerSuccess,
				PING,
				PONG,
				RedeliverUnacknowledgedMessages,
				PartitionedMetadata,
				PartitionedMetadataResponse,
				LOOKUP,
				LookupResponse,
				ConsumerStats,
				ConsumerStatsResponse,
				ReachedEndOfTopic,
				SEEK,
				GetLastMessageId,
				GetLastMessageIdResponse,
				ActiveConsumerChange,
				GetTopicsOfNamespace,
				GetTopicsOfNamespaceResponse,
				GetSchema,
				GetSchemaResponse,
				AuthChallenge,
				AuthResponse,
				AckResponse,
				GetOrCreateSchema,
				GetOrCreateSchemaResponse,
				NewTxn,
				NewTxnResponse,
				AddPartitionToTxn,
				AddPartitionToTxnResponse,
				AddSubscriptionToTxn,
				AddSubscriptionToTxnResponse,
				EndTxn,
				EndTxnResponse,
				EndTxnOnPartition,
				EndTxnOnPartitionResponse,
				EndTxnOnSubscription,
				EndTxnOnSubscriptionResponse
			}

			public readonly InnerEnum innerEnumValue;
			private readonly string nameValue;
			private readonly int ordinalValue;
			private static int nextOrdinal = 0;

			public static Type ValueOf(int Value)
			{
				switch (Value)
				{
					case 2:
						return CONNECT;
					case 3:
						return CONNECTED;
					case 4:
						return SUBSCRIBE;
					case 5:
						return PRODUCER;
					case 6:
						return SEND;
					case 7:
						return SendReceipt;
					case 8:
						return SendError;
					case 9:
						return MESSAGE;
					case 10:
						return ACK;
					case 11:
						return FLOW;
					case 12:
						return UNSUBSCRIBE;
					case 13:
						return SUCCESS;
					case 14:
						return ERROR;
					case 15:
						return CloseProducer;
					case 16:
						return CloseConsumer;
					case 17:
						return ProducerSuccess;
					case 18:
						return PING;
					case 19:
						return PONG;
					case 20:
						return RedeliverUnacknowledgedMessages;
					case 21:
						return PartitionedMetadata;
					case 22:
						return PartitionedMetadataResponse;
					case 23:
						return LOOKUP;
					case 24:
						return LookupResponse;
					case 25:
						return ConsumerStats;
					case 26:
						return ConsumerStatsResponse;
					case 27:
						return ReachedEndOfTopic;
					case 28:
						return SEEK;
					case 29:
						return GetLastMessageId;
					case 30:
						return GetLastMessageIdResponse;
					case 31:
						return ActiveConsumerChange;
					case 32:
						return GetTopicsOfNamespace;
					case 33:
						return GetTopicsOfNamespaceResponse;
					case 34:
						return GetSchema;
					case 35:
						return GetSchemaResponse;
					case 36:
						return AuthChallenge;
					case 37:
						return AuthResponse;
					case 38:
						return AckResponse;
					case 39:
						return GetOrCreateSchema;
					case 40:
						return GetOrCreateSchemaResponse;
					case 50:
						return NewTxn;
					case 51:
						return NewTxnResponse;
					case 52:
						return AddPartitionToTxn;
					case 53:
						return AddPartitionToTxnResponse;
					case 54:
						return AddSubscriptionToTxn;
					case 55:
						return AddSubscriptionToTxnResponse;
					case 56:
						return EndTxn;
					case 57:
						return EndTxnResponse;
					case 58:
						return EndTxnOnPartition;
					case 59:
						return EndTxnOnPartitionResponse;
					case 60:
						return EndTxnOnSubscription;
					case 61:
						return EndTxnOnSubscriptionResponse;
					default:
						return null;
				}
			}

			public static Type InternalGetValueMap()
			{
				return internalValueMap;
			}

		  public Type(string name, InnerEnum innerEnum, int Index, int Value)
			{
				this.Value = Value;

				nameValue = name;
				ordinalValue = nextOrdinal++;
				innerEnumValue = innerEnum;
			}

			//JAVA TO C# CONVERTER TODO TASK: Java to C# Converter does not convert types within enums:
			//		  private static class EnumLiteMapAnonymousInnerClass extends org.apache.pulsar.Internal.EnumLiteMap<Type>
			//	  {
			//		  public Type findValueByNumber(int number)
			//		  {
			//			return Type.valueOf(number);
			//		  }
			//	  }

			// @@protoc_insertion_point(enum_scope:pulsar.proto.BaseCommand.Type)

			public static IList<Type> Values()
			{
				return valueList;
			}

			public int Ordinal()
			{
				return ordinalValue;
			}

			public override string ToString()
			{
				return nameValue;
			}

			public static Type ValueOf(string name)
			{
				foreach (Type enumInstance in Type.valueList)
				{
					if (enumInstance.nameValue == name)
					{
						return enumInstance;
					}
				}
				throw new ArgumentException(name);
			}
		}

		internal int _bitField;
		internal int _bitField1;
		// required .pulsar.proto.BaseCommand.Type type = 1;
		public const int TypeFieldNumber = 1;
		internal BaseCommand.Type _type;
		public bool HasType()
		{
			return ((_bitField & 0x00000001) == 0x00000001);
		}
		public BaseCommand.Type GetType()
		{
			return _type;
		}

		// optional .pulsar.proto.CommandConnect connect = 2;
		public const int ConnectFieldNumber = 2;
		internal CommandConnect _connect;
		public bool HasConnect()
		{
			return ((_bitField & 0x00000002) == 0x00000002);
		}
		public CommandConnect Connect
		{
			get
			{
				return _connect;
			}
		}

		// optional .pulsar.proto.CommandConnected connected = 3;
		public const int ConnectedFieldNumber = 3;
		internal CommandConnected Connected_;
		public bool HasConnected()
		{
			return ((_bitField & 0x00000004) == 0x00000004);
		}
		public CommandConnected Connected
		{
			get
			{
				return Connected_;
			}
		}

		// optional .pulsar.proto.CommandSubscribe subscribe = 4;
		public const int SubscribeFieldNumber = 4;
		internal CommandSubscribe Subscribe_;
		public bool HasSubscribe()
		{
			return ((_bitField & 0x00000008) == 0x00000008);
		}
		public CommandSubscribe Subscribe
		{
			get
			{
				return Subscribe_;
			}
		}

		// optional .pulsar.proto.CommandProducer producer = 5;
		public const int ProducerFieldNumber = 5;
		internal CommandProducer Producer_;
		public bool HasProducer()
		{
			return ((_bitField & 0x00000010) == 0x00000010);
		}
		public CommandProducer Producer
		{
			get
			{
				return Producer_;
			}
		}

		// optional .pulsar.proto.CommandSend send = 6;
		public const int SendFieldNumber = 6;
		internal CommandSend Send_;
		public bool HasSend()
		{
			return ((_bitField & 0x00000020) == 0x00000020);
		}
		public CommandSend Send
		{
			get
			{
				return Send_;
			}
		}

		// optional .pulsar.proto.CommandSendReceipt send_receipt = 7;
		public const int SendReceiptFieldNumber = 7;
		internal CommandSendReceipt SendReceipt_;
		public bool HasSendReceipt()
		{
			return ((_bitField & 0x00000040) == 0x00000040);
		}
		public CommandSendReceipt SendReceipt
		{
			get
			{
				return SendReceipt_;
			}
		}

		// optional .pulsar.proto.CommandSendError send_error = 8;
		public const int SendErrorFieldNumber = 8;
		internal CommandSendError SendError_;
		public bool HasSendError()
		{
			return ((_bitField & 0x00000080) == 0x00000080);
		}
		public CommandSendError SendError
		{
			get
			{
				return SendError_;
			}
		}

		// optional .pulsar.proto.CommandMessage message = 9;
		public const int MessageFieldNumber = 9;
		internal CommandMessage Message_;
		public bool HasMessage()
		{
			return ((_bitField & 0x00000100) == 0x00000100);
		}
		public CommandMessage Message
		{
			get
			{
				return Message_;
			}
		}

		// optional .pulsar.proto.CommandAck ack = 10;
		public const int AckFieldNumber = 10;
		internal CommandAck Ack_;
		public bool HasAck()
		{
			return ((_bitField & 0x00000200) == 0x00000200);
		}
		public CommandAck Ack
		{
			get
			{
				return Ack_;
			}
		}

		// optional .pulsar.proto.CommandFlow flow = 11;
		public const int FlowFieldNumber = 11;
		internal CommandFlow Flow_;
		public bool HasFlow()
		{
			return ((_bitField & 0x00000400) == 0x00000400);
		}
		public CommandFlow Flow
		{
			get
			{
				return Flow_;
			}
		}

		// optional .pulsar.proto.CommandUnsubscribe unsubscribe = 12;
		public const int UnsubscribeFieldNumber = 12;
		internal CommandUnsubscribe Unsubscribe_;
		public bool HasUnsubscribe()
		{
			return ((_bitField & 0x00000800) == 0x00000800);
		}
		public CommandUnsubscribe Unsubscribe
		{
			get
			{
				return Unsubscribe_;
			}
		}

		// optional .pulsar.proto.CommandSuccess success = 13;
		public const int SuccessFieldNumber = 13;
		internal CommandSuccess Success_;
		public bool HasSuccess()
		{
			return ((_bitField & 0x00001000) == 0x00001000);
		}
		public CommandSuccess Success
		{
			get
			{
				return Success_;
			}
		}

		// optional .pulsar.proto.CommandError error = 14;
		public const int ErrorFieldNumber = 14;
		internal CommandError Error_;
		public bool HasError()
		{
			return ((_bitField & 0x00002000) == 0x00002000);
		}
		public CommandError Error
		{
			get
			{
				return Error_;
			}
		}

		// optional .pulsar.proto.CommandCloseProducer close_producer = 15;
		public const int CloseProducerFieldNumber = 15;
		internal CommandCloseProducer CloseProducer_;
		public bool HasCloseProducer()
		{
			return ((_bitField & 0x00004000) == 0x00004000);
		}
		public CommandCloseProducer CloseProducer
		{
			get
			{
				return CloseProducer_;
			}
		}

		// optional .pulsar.proto.CommandCloseConsumer close_consumer = 16;
		public const int CloseConsumerFieldNumber = 16;
		internal CommandCloseConsumer CloseConsumer_;
		public bool HasCloseConsumer()
		{
			return ((_bitField & 0x00008000) == 0x00008000);
		}
		public CommandCloseConsumer CloseConsumer
		{
			get
			{
				return CloseConsumer_;
			}
		}

		// optional .pulsar.proto.CommandProducerSuccess producer_success = 17;
		public const int ProducerSuccessFieldNumber = 17;
		internal CommandProducerSuccess ProducerSuccess_;
		public bool HasProducerSuccess()
		{
			return ((_bitField & 0x00010000) == 0x00010000);
		}
		public CommandProducerSuccess ProducerSuccess
		{
			get
			{
				return ProducerSuccess_;
			}
		}

		// optional .pulsar.proto.CommandPing ping = 18;
		public const int PingFieldNumber = 18;
		internal CommandPing Ping_;
		public bool HasPing()
		{
			return ((_bitField & 0x00020000) == 0x00020000);
		}
		public CommandPing Ping
		{
			get
			{
				return Ping_;
			}
		}

		// optional .pulsar.proto.CommandPong pong = 19;
		public const int PongFieldNumber = 19;
		internal CommandPong Pong_;
		public bool HasPong()
		{
			return ((_bitField & 0x00040000) == 0x00040000);
		}
		public CommandPong Pong
		{
			get
			{
				return Pong_;
			}
		}

		// optional .pulsar.proto.CommandRedeliverUnacknowledgedMessages redeliverUnacknowledgedMessages = 20;
		public const int RedeliverunacknowledgedmessagesFieldNumber = 20;
		internal CommandRedeliverUnacknowledgedMessages RedeliverUnacknowledgedMessages_;
		public bool HasRedeliverUnacknowledgedMessages()
		{
			return ((_bitField & 0x00080000) == 0x00080000);
		}
		public CommandRedeliverUnacknowledgedMessages RedeliverUnacknowledgedMessages
		{
			get
			{
				return RedeliverUnacknowledgedMessages_;
			}
		}

		// optional .pulsar.proto.CommandPartitionedTopicMetadata partitionMetadata = 21;
		public const int PartitionmetadataFieldNumber = 21;
		internal CommandPartitionedTopicMetadata PartitionMetadata_;
		public bool HasPartitionMetadata()
		{
			return ((_bitField & 0x00100000) == 0x00100000);
		}
		public CommandPartitionedTopicMetadata PartitionMetadata
		{
			get
			{
				return PartitionMetadata_;
			}
		}

		// optional .pulsar.proto.CommandPartitionedTopicMetadataResponse partitionMetadataResponse = 22;
		public const int PartitionmetadataresponseFieldNumber = 22;
		internal CommandPartitionedTopicMetadataResponse PartitionMetadataResponse_;
		public bool HasPartitionMetadataResponse()
		{
			return ((_bitField & 0x00200000) == 0x00200000);
		}
		public CommandPartitionedTopicMetadataResponse PartitionMetadataResponse
		{
			get
			{
				return PartitionMetadataResponse_;
			}
		}

		// optional .pulsar.proto.CommandLookupTopic lookupTopic = 23;
		public const int LookuptopicFieldNumber = 23;
		internal CommandLookupTopic LookupTopic_;
		public bool HasLookupTopic()
		{
			return ((_bitField & 0x00400000) == 0x00400000);
		}
		public CommandLookupTopic LookupTopic
		{
			get
			{
				return LookupTopic_;
			}
		}

		// optional .pulsar.proto.CommandLookupTopicResponse lookupTopicResponse = 24;
		public const int LookuptopicresponseFieldNumber = 24;
		internal CommandLookupTopicResponse LookupTopicResponse_;
		public bool HasLookupTopicResponse()
		{
			return ((_bitField & 0x00800000) == 0x00800000);
		}
		public CommandLookupTopicResponse LookupTopicResponse
		{
			get
			{
				return LookupTopicResponse_;
			}
		}

		// optional .pulsar.proto.CommandConsumerStats consumerStats = 25;
		public const int ConsumerstatsFieldNumber = 25;
		internal CommandConsumerStats ConsumerStats_;
		public bool HasConsumerStats()
		{
			return ((_bitField & 0x01000000) == 0x01000000);
		}
		public CommandConsumerStats ConsumerStats
		{
			get
			{
				return ConsumerStats_;
			}
		}

		// optional .pulsar.proto.CommandConsumerStatsResponse consumerStatsResponse = 26;
		public const int ConsumerstatsresponseFieldNumber = 26;
		internal CommandConsumerStatsResponse ConsumerStatsResponse_;
		public bool HasConsumerStatsResponse()
		{
			return ((_bitField & 0x02000000) == 0x02000000);
		}
		public CommandConsumerStatsResponse ConsumerStatsResponse
		{
			get
			{
				return ConsumerStatsResponse_;
			}
		}

		// optional .pulsar.proto.CommandReachedEndOfTopic reachedEndOfTopic = 27;
		public const int ReachedendoftopicFieldNumber = 27;
		internal CommandReachedEndOfTopic ReachedEndOfTopic_;
		public bool HasReachedEndOfTopic()
		{
			return ((_bitField & 0x04000000) == 0x04000000);
		}
		public CommandReachedEndOfTopic ReachedEndOfTopic
		{
			get
			{
				return ReachedEndOfTopic_;
			}
		}

		// optional .pulsar.proto.CommandSeek seek = 28;
		public const int SeekFieldNumber = 28;
		internal CommandSeek Seek_;
		public bool HasSeek()
		{
			return ((_bitField & 0x08000000) == 0x08000000);
		}
		public CommandSeek Seek
		{
			get
			{
				return Seek_;
			}
		}

		// optional .pulsar.proto.CommandGetLastMessageId getLastMessageId = 29;
		public const int GetlastmessageidFieldNumber = 29;
		internal CommandGetLastMessageId GetLastMessageId_;
		public bool HasGetLastMessageId()
		{
			return ((_bitField & 0x10000000) == 0x10000000);
		}
		public CommandGetLastMessageId GetLastMessageId
		{
			get
			{
				return GetLastMessageId_;
			}
		}

		// optional .pulsar.proto.CommandGetLastMessageIdResponse getLastMessageIdResponse = 30;
		public const int GetlastmessageidresponseFieldNumber = 30;
		internal CommandGetLastMessageIdResponse GetLastMessageIdResponse_;
		public bool HasGetLastMessageIdResponse()
		{
			return ((_bitField & 0x20000000) == 0x20000000);
		}
		public CommandGetLastMessageIdResponse GetLastMessageIdResponse
		{
			get
			{
				return GetLastMessageIdResponse_;
			}
		}

		// optional .pulsar.proto.CommandActiveConsumerChange active_consumer_change = 31;
		public const int ActiveConsumerChangeFieldNumber = 31;
		internal CommandActiveConsumerChange ActiveConsumerChange_;
		public bool HasActiveConsumerChange()
		{
			return ((_bitField & 0x40000000) == 0x40000000);
		}
		public CommandActiveConsumerChange ActiveConsumerChange
		{
			get
			{
				return ActiveConsumerChange_;
			}
		}

		// optional .pulsar.proto.CommandGetTopicsOfNamespace getTopicsOfNamespace = 32;
		public const int GettopicsofnamespaceFieldNumber = 32;
		internal CommandGetTopicsOfNamespace GetTopicsOfNamespace_;
		public bool HasGetTopicsOfNamespace()
		{
			return ((_bitField & 0x80000000) == 0x80000000);
		}
		public CommandGetTopicsOfNamespace GetTopicsOfNamespace
		{
			get
			{
				return GetTopicsOfNamespace_;
			}
		}

		// optional .pulsar.proto.CommandGetTopicsOfNamespaceResponse getTopicsOfNamespaceResponse = 33;
		public const int GettopicsofnamespaceresponseFieldNumber = 33;
		internal CommandGetTopicsOfNamespaceResponse GetTopicsOfNamespaceResponse_;
		public bool HasGetTopicsOfNamespaceResponse()
		{
			return ((_bitField1 & 0x00000001) == 0x00000001);
		}
		public CommandGetTopicsOfNamespaceResponse GetTopicsOfNamespaceResponse
		{
			get
			{
				return GetTopicsOfNamespaceResponse_;
			}
		}

		// optional .pulsar.proto.CommandGetSchema getSchema = 34;
		public const int GetschemaFieldNumber = 34;
		internal CommandGetSchema GetSchema_;
		public bool HasGetSchema()
		{
			return ((_bitField1 & 0x00000002) == 0x00000002);
		}
		public CommandGetSchema GetSchema
		{
			get
			{
				return GetSchema_;
			}
		}

		// optional .pulsar.proto.CommandGetSchemaResponse getSchemaResponse = 35;
		public const int GetschemaresponseFieldNumber = 35;
		internal CommandGetSchemaResponse GetSchemaResponse_;
		public bool HasGetSchemaResponse()
		{
			return ((_bitField1 & 0x00000004) == 0x00000004);
		}
		public CommandGetSchemaResponse GetSchemaResponse
		{
			get
			{
				return GetSchemaResponse_;
			}
		}

		// optional .pulsar.proto.CommandAuthChallenge authChallenge = 36;
		public const int AuthchallengeFieldNumber = 36;
		internal CommandAuthChallenge AuthChallenge_;
		public bool HasAuthChallenge()
		{
			return ((_bitField1 & 0x00000008) == 0x00000008);
		}
		public CommandAuthChallenge AuthChallenge
		{
			get
			{
				return AuthChallenge_;
			}
		}

		// optional .pulsar.proto.CommandAuthResponse authResponse = 37;
		public const int AuthresponseFieldNumber = 37;
		internal CommandAuthResponse AuthResponse_;
		public bool HasAuthResponse()
		{
			return ((_bitField1 & 0x00000010) == 0x00000010);
		}
		public CommandAuthResponse AuthResponse
		{
			get
			{
				return AuthResponse_;
			}
		}

		// optional .pulsar.proto.CommandAckResponse ackResponse = 38;
		public const int AckresponseFieldNumber = 38;
		internal CommandAckResponse AckResponse_;
		public bool HasAckResponse()
		{
			return ((_bitField1 & 0x00000020) == 0x00000020);
		}
		public CommandAckResponse AckResponse
		{
			get
			{
				return AckResponse_;
			}
		}

		// optional .pulsar.proto.CommandGetOrCreateSchema getOrCreateSchema = 39;
		public const int GetorcreateschemaFieldNumber = 39;
		internal CommandGetOrCreateSchema GetOrCreateSchema_;
		public bool HasGetOrCreateSchema()
		{
			return ((_bitField1 & 0x00000040) == 0x00000040);
		}
		public CommandGetOrCreateSchema GetOrCreateSchema
		{
			get
			{
				return GetOrCreateSchema_;
			}
		}

		// optional .pulsar.proto.CommandGetOrCreateSchemaResponse getOrCreateSchemaResponse = 40;
		public const int GetorcreateschemaresponseFieldNumber = 40;
		internal CommandGetOrCreateSchemaResponse GetOrCreateSchemaResponse_;
		public bool HasGetOrCreateSchemaResponse()
		{
			return ((_bitField1 & 0x00000080) == 0x00000080);
		}
		public CommandGetOrCreateSchemaResponse GetOrCreateSchemaResponse
		{
			get
			{
				return GetOrCreateSchemaResponse_;
			}
		}

		// optional .pulsar.proto.CommandNewTxn newTxn = 50;
		public const int NewtxnFieldNumber = 50;
		internal CommandNewTxn NewTxn_;
		public bool HasNewTxn()
		{
			return ((_bitField1 & 0x00000100) == 0x00000100);
		}
		public CommandNewTxn NewTxn
		{
			get
			{
				return NewTxn_;
			}
		}

		// optional .pulsar.proto.CommandNewTxnResponse newTxnResponse = 51;
		public const int NewtxnresponseFieldNumber = 51;
		internal CommandNewTxnResponse NewTxnResponse_;
		public bool HasNewTxnResponse()
		{
			return ((_bitField1 & 0x00000200) == 0x00000200);
		}
		public CommandNewTxnResponse NewTxnResponse
		{
			get
			{
				return NewTxnResponse_;
			}
		}

		// optional .pulsar.proto.CommandAddPartitionToTxn addPartitionToTxn = 52;
		public const int AddpartitiontotxnFieldNumber = 52;
		internal CommandAddPartitionToTxn AddPartitionToTxn_;
		public bool HasAddPartitionToTxn()
		{
			return ((_bitField1 & 0x00000400) == 0x00000400);
		}
		public CommandAddPartitionToTxn AddPartitionToTxn
		{
			get
			{
				return AddPartitionToTxn_;
			}
		}

		// optional .pulsar.proto.CommandAddPartitionToTxnResponse addPartitionToTxnResponse = 53;
		public const int AddpartitiontotxnresponseFieldNumber = 53;
		internal CommandAddPartitionToTxnResponse AddPartitionToTxnResponse_;
		public bool HasAddPartitionToTxnResponse()
		{
			return ((_bitField1 & 0x00000800) == 0x00000800);
		}
		public CommandAddPartitionToTxnResponse AddPartitionToTxnResponse
		{
			get
			{
				return AddPartitionToTxnResponse_;
			}
		}

		// optional .pulsar.proto.CommandAddSubscriptionToTxn addSubscriptionToTxn = 54;
		public const int AddsubscriptiontotxnFieldNumber = 54;
		internal CommandAddSubscriptionToTxn AddSubscriptionToTxn_;
		public bool HasAddSubscriptionToTxn()
		{
			return ((_bitField1 & 0x00001000) == 0x00001000);
		}
		public CommandAddSubscriptionToTxn AddSubscriptionToTxn
		{
			get
			{
				return AddSubscriptionToTxn_;
			}
		}

		// optional .pulsar.proto.CommandAddSubscriptionToTxnResponse addSubscriptionToTxnResponse = 55;
		public const int AddsubscriptiontotxnresponseFieldNumber = 55;
		internal CommandAddSubscriptionToTxnResponse AddSubscriptionToTxnResponse_;
		public bool HasAddSubscriptionToTxnResponse()
		{
			return ((_bitField1 & 0x00002000) == 0x00002000);
		}
		public CommandAddSubscriptionToTxnResponse AddSubscriptionToTxnResponse
		{
			get
			{
				return AddSubscriptionToTxnResponse_;
			}
		}

		// optional .pulsar.proto.CommandEndTxn endTxn = 56;
		public const int EndtxnFieldNumber = 56;
		internal CommandEndTxn EndTxn_;
		public bool HasEndTxn()
		{
			return ((_bitField1 & 0x00004000) == 0x00004000);
		}
		public CommandEndTxn EndTxn
		{
			get
			{
				return EndTxn_;
			}
		}

		// optional .pulsar.proto.CommandEndTxnResponse endTxnResponse = 57;
		public const int EndtxnresponseFieldNumber = 57;
		internal CommandEndTxnResponse EndTxnResponse_;
		public bool HasEndTxnResponse()
		{
			return ((_bitField1 & 0x00008000) == 0x00008000);
		}
		public CommandEndTxnResponse EndTxnResponse
		{
			get
			{
				return EndTxnResponse_;
			}
		}

		// optional .pulsar.proto.CommandEndTxnOnPartition endTxnOnPartition = 58;
		public const int EndtxnonpartitionFieldNumber = 58;
		internal CommandEndTxnOnPartition EndTxnOnPartition_;
		public bool HasEndTxnOnPartition()
		{
			return ((_bitField1 & 0x00010000) == 0x00010000);
		}
		public CommandEndTxnOnPartition EndTxnOnPartition
		{
			get
			{
				return EndTxnOnPartition_;
			}
		}

		// optional .pulsar.proto.CommandEndTxnOnPartitionResponse endTxnOnPartitionResponse = 59;
		public const int EndtxnonpartitionresponseFieldNumber = 59;
		internal CommandEndTxnOnPartitionResponse EndTxnOnPartitionResponse_;
		public bool HasEndTxnOnPartitionResponse()
		{
			return ((_bitField1 & 0x00020000) == 0x00020000);
		}
		public CommandEndTxnOnPartitionResponse EndTxnOnPartitionResponse
		{
			get
			{
				return EndTxnOnPartitionResponse_;
			}
		}

		// optional .pulsar.proto.CommandEndTxnOnSubscription endTxnOnSubscription = 60;
		public const int EndtxnonsubscriptionFieldNumber = 60;
		internal CommandEndTxnOnSubscription EndTxnOnSubscription_;
		public bool HasEndTxnOnSubscription()
		{
			return ((_bitField1 & 0x00040000) == 0x00040000);
		}
		public CommandEndTxnOnSubscription EndTxnOnSubscription
		{
			get
			{
				return EndTxnOnSubscription_;
			}
		}

		// optional .pulsar.proto.CommandEndTxnOnSubscriptionResponse endTxnOnSubscriptionResponse = 61;
		public const int EndtxnonsubscriptionresponseFieldNumber = 61;
		internal CommandEndTxnOnSubscriptionResponse _endTxnOnSubscriptionResponse;
		public bool HasEndTxnOnSubscriptionResponse()
		{
			return ((_bitField1 & 0x00080000) == 0x00080000);
		}
		public CommandEndTxnOnSubscriptionResponse EndTxnOnSubscriptionResponse
		{
			get
			{
				return _endTxnOnSubscriptionResponse;
			}
		}

		public void InitFields()
		{
			_type = BaseCommand.Type.CONNECT;
			_connect = CommandConnect.DefaultInstance;
			Connected_ = CommandConnected.DefaultInstance;
			Subscribe_ = CommandSubscribe.DefaultInstance;
			Producer_ = CommandProducer.DefaultInstance;
			Send_ = CommandSend.DefaultInstance;
			SendReceipt_ = CommandSendReceipt.DefaultInstance;
			SendError_ = CommandSendError.DefaultInstance;
			Message_ = CommandMessage.DefaultInstance;
			Ack_ = CommandAck.DefaultInstance;
			Flow_ = CommandFlow.DefaultInstance;
			Unsubscribe_ = CommandUnsubscribe.DefaultInstance;
			Success_ = CommandSuccess.DefaultInstance;
			Error_ = CommandError.DefaultInstance;
			CloseProducer_ = CommandCloseProducer.DefaultInstance;
			CloseConsumer_ = CommandCloseConsumer.DefaultInstance;
			ProducerSuccess_ = CommandProducerSuccess.DefaultInstance;
			Ping_ = CommandPing.DefaultInstance;
			Pong_ = CommandPong.DefaultInstance;
			RedeliverUnacknowledgedMessages_ = CommandRedeliverUnacknowledgedMessages.DefaultInstance;
			PartitionMetadata_ = CommandPartitionedTopicMetadata.DefaultInstance;
			PartitionMetadataResponse_ = CommandPartitionedTopicMetadataResponse.DefaultInstance;
			LookupTopic_ = CommandLookupTopic.DefaultInstance;
			LookupTopicResponse_ = CommandLookupTopicResponse.DefaultInstance;
			ConsumerStats_ = CommandConsumerStats.DefaultInstance;
			ConsumerStatsResponse_ = CommandConsumerStatsResponse.DefaultInstance;
			ReachedEndOfTopic_ = CommandReachedEndOfTopic.DefaultInstance;
			Seek_ = CommandSeek.DefaultInstance;
			GetLastMessageId_ = CommandGetLastMessageId.DefaultInstance;
			GetLastMessageIdResponse_ = CommandGetLastMessageIdResponse.DefaultInstance;
			ActiveConsumerChange_ = CommandActiveConsumerChange.DefaultInstance;
			GetTopicsOfNamespace_ = CommandGetTopicsOfNamespace.DefaultInstance;
			GetTopicsOfNamespaceResponse_ = CommandGetTopicsOfNamespaceResponse.DefaultInstance;
			GetSchema_ = CommandGetSchema.DefaultInstance;
			GetSchemaResponse_ = CommandGetSchemaResponse.DefaultInstance;
			AuthChallenge_ = CommandAuthChallenge.DefaultInstance;
			AuthResponse_ = CommandAuthResponse.DefaultInstance;
			AckResponse_ = CommandAckResponse.DefaultInstance;
			GetOrCreateSchema_ = CommandGetOrCreateSchema.DefaultInstance;
			GetOrCreateSchemaResponse_ = CommandGetOrCreateSchemaResponse.DefaultInstance;
			NewTxn_ = CommandNewTxn.DefaultInstance;
			NewTxnResponse_ = CommandNewTxnResponse.DefaultInstance;
			AddPartitionToTxn_ = CommandAddPartitionToTxn.DefaultInstance;
			AddPartitionToTxnResponse_ = CommandAddPartitionToTxnResponse.DefaultInstance;
			AddSubscriptionToTxn_ = CommandAddSubscriptionToTxn.DefaultInstance;
			AddSubscriptionToTxnResponse_ = CommandAddSubscriptionToTxnResponse.DefaultInstance;
			EndTxn_ = CommandEndTxn.DefaultInstance;
			EndTxnResponse_ = CommandEndTxnResponse.DefaultInstance;
			EndTxnOnPartition_ = CommandEndTxnOnPartition.DefaultInstance;
			EndTxnOnPartitionResponse_ = CommandEndTxnOnPartitionResponse.DefaultInstance;
			EndTxnOnSubscription_ = CommandEndTxnOnSubscription.DefaultInstance;
			_endTxnOnSubscriptionResponse = CommandEndTxnOnSubscriptionResponse.DefaultInstance;
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

				if (!HasType())
				{
					MemoizedIsInitialized = 0;
					return false;
				}
				if (HasConnect())
				{
					if (!Connect.Initialized)
					{
						MemoizedIsInitialized = 0;
						return false;
					}
				}
				if (HasConnected())
				{
					if (!Connected.Initialized)
					{
						MemoizedIsInitialized = 0;
						return false;
					}
				}
				if (HasSubscribe())
				{
					if (!Subscribe.Initialized)
					{
						MemoizedIsInitialized = 0;
						return false;
					}
				}
				if (HasProducer())
				{
					if (!Producer.Initialized)
					{
						MemoizedIsInitialized = 0;
						return false;
					}
				}
				if (HasSend())
				{
					if (!Send.Initialized)
					{
						MemoizedIsInitialized = 0;
						return false;
					}
				}
				if (HasSendReceipt())
				{
					if (!SendReceipt.Initialized)
					{
						MemoizedIsInitialized = 0;
						return false;
					}
				}
				if (HasSendError())
				{
					if (!SendError.Initialized)
					{
						MemoizedIsInitialized = 0;
						return false;
					}
				}
				if (HasMessage())
				{
					if (!Message.Initialized)
					{
						MemoizedIsInitialized = 0;
						return false;
					}
				}
				if (HasAck())
				{
					if (!Ack.Initialized)
					{
						MemoizedIsInitialized = 0;
						return false;
					}
				}
				if (HasFlow())
				{
					if (!Flow.Initialized)
					{
						MemoizedIsInitialized = 0;
						return false;
					}
				}
				if (HasUnsubscribe())
				{
					if (!Unsubscribe.Initialized)
					{
						MemoizedIsInitialized = 0;
						return false;
					}
				}
				if (HasSuccess())
				{
					if (!Success.Initialized)
					{
						MemoizedIsInitialized = 0;
						return false;
					}
				}
				if (HasError())
				{
					if (!Exception.Initialized)
					{
						MemoizedIsInitialized = 0;
						return false;
					}
				}
				if (HasCloseProducer())
				{
					if (!CloseProducer.Initialized)
					{
						MemoizedIsInitialized = 0;
						return false;
					}
				}
				if (HasCloseConsumer())
				{
					if (!CloseConsumer.Initialized)
					{
						MemoizedIsInitialized = 0;
						return false;
					}
				}
				if (HasProducerSuccess())
				{
					if (!ProducerSuccess.Initialized)
					{
						MemoizedIsInitialized = 0;
						return false;
					}
				}
				if (HasRedeliverUnacknowledgedMessages())
				{
					if (!RedeliverUnacknowledgedMessages.Initialized)
					{
						MemoizedIsInitialized = 0;
						return false;
					}
				}
				if (HasPartitionMetadata())
				{
					if (!PartitionMetadata.Initialized)
					{
						MemoizedIsInitialized = 0;
						return false;
					}
				}
				if (HasPartitionMetadataResponse())
				{
					if (!PartitionMetadataResponse.Initialized)
					{
						MemoizedIsInitialized = 0;
						return false;
					}
				}
				if (HasLookupTopic())
				{
					if (!LookupTopic.Initialized)
					{
						MemoizedIsInitialized = 0;
						return false;
					}
				}
				if (HasLookupTopicResponse())
				{
					if (!LookupTopicResponse.Initialized)
					{
						MemoizedIsInitialized = 0;
						return false;
					}
				}
				if (HasConsumerStats())
				{
					if (!ConsumerStats.Initialized)
					{
						MemoizedIsInitialized = 0;
						return false;
					}
				}
				if (HasConsumerStatsResponse())
				{
					if (!ConsumerStatsResponse.Initialized)
					{
						MemoizedIsInitialized = 0;
						return false;
					}
				}
				if (HasReachedEndOfTopic())
				{
					if (!ReachedEndOfTopic.Initialized)
					{
						MemoizedIsInitialized = 0;
						return false;
					}
				}
				if (HasSeek())
				{
					if (!Seek.Initialized)
					{
						MemoizedIsInitialized = 0;
						return false;
					}
				}
				if (HasGetLastMessageId())
				{
					if (!GetLastMessageId.Initialized)
					{
						MemoizedIsInitialized = 0;
						return false;
					}
				}
				if (HasGetLastMessageIdResponse())
				{
					if (!GetLastMessageIdResponse.Initialized)
					{
						MemoizedIsInitialized = 0;
						return false;
					}
				}
				if (HasActiveConsumerChange())
				{
					if (!ActiveConsumerChange.Initialized)
					{
						MemoizedIsInitialized = 0;
						return false;
					}
				}
				if (HasGetTopicsOfNamespace())
				{
					if (!GetTopicsOfNamespace.Initialized)
					{
						MemoizedIsInitialized = 0;
						return false;
					}
				}
				if (HasGetTopicsOfNamespaceResponse())
				{
					if (!GetTopicsOfNamespaceResponse.Initialized)
					{
						MemoizedIsInitialized = 0;
						return false;
					}
				}
				if (HasGetSchema())
				{
					if (!GetSchema.Initialized)
					{
						MemoizedIsInitialized = 0;
						return false;
					}
				}
				if (HasGetSchemaResponse())
				{
					if (!GetSchemaResponse.Initialized)
					{
						MemoizedIsInitialized = 0;
						return false;
					}
				}
				if (HasAckResponse())
				{
					if (!AckResponse.Initialized)
					{
						MemoizedIsInitialized = 0;
						return false;
					}
				}
				if (HasGetOrCreateSchema())
				{
					if (!GetOrCreateSchema.Initialized)
					{
						MemoizedIsInitialized = 0;
						return false;
					}
				}
				if (HasGetOrCreateSchemaResponse())
				{
					if (!GetOrCreateSchemaResponse.Initialized)
					{
						MemoizedIsInitialized = 0;
						return false;
					}
				}
				if (HasNewTxn())
				{
					if (!NewTxn.Initialized)
					{
						MemoizedIsInitialized = 0;
						return false;
					}
				}
				if (HasNewTxnResponse())
				{
					if (!NewTxnResponse.Initialized)
					{
						MemoizedIsInitialized = 0;
						return false;
					}
				}
				if (HasAddPartitionToTxn())
				{
					if (!AddPartitionToTxn.Initialized)
					{
						MemoizedIsInitialized = 0;
						return false;
					}
				}
				if (HasAddPartitionToTxnResponse())
				{
					if (!AddPartitionToTxnResponse.Initialized)
					{
						MemoizedIsInitialized = 0;
						return false;
					}
				}
				if (HasAddSubscriptionToTxn())
				{
					if (!AddSubscriptionToTxn.Initialized)
					{
						MemoizedIsInitialized = 0;
						return false;
					}
				}
				if (HasAddSubscriptionToTxnResponse())
				{
					if (!AddSubscriptionToTxnResponse.Initialized)
					{
						MemoizedIsInitialized = 0;
						return false;
					}
				}
				if (HasEndTxn())
				{
					if (!EndTxn.Initialized)
					{
						MemoizedIsInitialized = 0;
						return false;
					}
				}
				if (HasEndTxnResponse())
				{
					if (!EndTxnResponse.Initialized)
					{
						MemoizedIsInitialized = 0;
						return false;
					}
				}
				if (HasEndTxnOnPartition())
				{
					if (!EndTxnOnPartition.Initialized)
					{
						MemoizedIsInitialized = 0;
						return false;
					}
				}
				if (HasEndTxnOnPartitionResponse())
				{
					if (!EndTxnOnPartitionResponse.Initialized)
					{
						MemoizedIsInitialized = 0;
						return false;
					}
				}
				if (HasEndTxnOnSubscription())
				{
					if (!EndTxnOnSubscription.Initialized)
					{
						MemoizedIsInitialized = 0;
						return false;
					}
				}
				if (HasEndTxnOnSubscriptionResponse())
				{
					if (!EndTxnOnSubscriptionResponse.Initialized)
					{
						MemoizedIsInitialized = 0;
						return false;
					}
				}
				MemoizedIsInitialized = 1;
				return true;
			}
		}

		//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		//ORIGINAL LINE: public void writeTo(org.apache.pulsar.CodedOutputStream output) throws java.io.IOException
		public void WriteTo(CodedOutputStream Output)
		{
			throw new Exception("Cannot use CodedOutputStream");
		}

		//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		//ORIGINAL LINE: public void writeTo(org.apache.pulsar.common.util.protobuf.ByteBufCodedOutputStream output) throws java.io.IOException
		public void WriteTo(ByteBufCodedOutputStream Output)
		{
			SerializedSize;
			if (((_bitField & 0x00000001) == 0x00000001))
			{
				Output.writeEnum(1, _type.Number);
			}
			if (((_bitField & 0x00000002) == 0x00000002))
			{
				Output.writeMessage(2, _connect);
			}
			if (((_bitField & 0x00000004) == 0x00000004))
			{
				Output.writeMessage(3, Connected_);
			}
			if (((_bitField & 0x00000008) == 0x00000008))
			{
				Output.writeMessage(4, Subscribe_);
			}
			if (((_bitField & 0x00000010) == 0x00000010))
			{
				Output.writeMessage(5, Producer_);
			}
			if (((_bitField & 0x00000020) == 0x00000020))
			{
				Output.writeMessage(6, Send_);
			}
			if (((_bitField & 0x00000040) == 0x00000040))
			{
				Output.writeMessage(7, SendReceipt_);
			}
			if (((_bitField & 0x00000080) == 0x00000080))
			{
				Output.writeMessage(8, SendError_);
			}
			if (((_bitField & 0x00000100) == 0x00000100))
			{
				Output.writeMessage(9, Message_);
			}
			if (((_bitField & 0x00000200) == 0x00000200))
			{
				Output.writeMessage(10, Ack_);
			}
			if (((_bitField & 0x00000400) == 0x00000400))
			{
				Output.writeMessage(11, Flow_);
			}
			if (((_bitField & 0x00000800) == 0x00000800))
			{
				Output.writeMessage(12, Unsubscribe_);
			}
			if (((_bitField & 0x00001000) == 0x00001000))
			{
				Output.writeMessage(13, Success_);
			}
			if (((_bitField & 0x00002000) == 0x00002000))
			{
				Output.writeMessage(14, Error_);
			}
			if (((_bitField & 0x00004000) == 0x00004000))
			{
				Output.writeMessage(15, CloseProducer_);
			}
			if (((_bitField & 0x00008000) == 0x00008000))
			{
				Output.writeMessage(16, CloseConsumer_);
			}
			if (((_bitField & 0x00010000) == 0x00010000))
			{
				Output.writeMessage(17, ProducerSuccess_);
			}
			if (((_bitField & 0x00020000) == 0x00020000))
			{
				Output.writeMessage(18, Ping_);
			}
			if (((_bitField & 0x00040000) == 0x00040000))
			{
				Output.writeMessage(19, Pong_);
			}
			if (((_bitField & 0x00080000) == 0x00080000))
			{
				Output.writeMessage(20, RedeliverUnacknowledgedMessages_);
			}
			if (((_bitField & 0x00100000) == 0x00100000))
			{
				Output.writeMessage(21, PartitionMetadata_);
			}
			if (((_bitField & 0x00200000) == 0x00200000))
			{
				Output.writeMessage(22, PartitionMetadataResponse_);
			}
			if (((_bitField & 0x00400000) == 0x00400000))
			{
				Output.writeMessage(23, LookupTopic_);
			}
			if (((_bitField & 0x00800000) == 0x00800000))
			{
				Output.writeMessage(24, LookupTopicResponse_);
			}
			if (((_bitField & 0x01000000) == 0x01000000))
			{
				Output.writeMessage(25, ConsumerStats_);
			}
			if (((_bitField & 0x02000000) == 0x02000000))
			{
				Output.writeMessage(26, ConsumerStatsResponse_);
			}
			if (((_bitField & 0x04000000) == 0x04000000))
			{
				Output.writeMessage(27, ReachedEndOfTopic_);
			}
			if (((_bitField & 0x08000000) == 0x08000000))
			{
				Output.writeMessage(28, Seek_);
			}
			if (((_bitField & 0x10000000) == 0x10000000))
			{
				Output.writeMessage(29, GetLastMessageId_);
			}
			if (((_bitField & 0x20000000) == 0x20000000))
			{
				Output.writeMessage(30, GetLastMessageIdResponse_);
			}
			if (((_bitField & 0x40000000) == 0x40000000))
			{
				Output.writeMessage(31, ActiveConsumerChange_);
			}
			if (((_bitField & 0x80000000) == 0x80000000))
			{
				Output.writeMessage(32, GetTopicsOfNamespace_);
			}
			if (((_bitField1 & 0x00000001) == 0x00000001))
			{
				Output.writeMessage(33, GetTopicsOfNamespaceResponse_);
			}
			if (((_bitField1 & 0x00000002) == 0x00000002))
			{
				Output.writeMessage(34, GetSchema_);
			}
			if (((_bitField1 & 0x00000004) == 0x00000004))
			{
				Output.writeMessage(35, GetSchemaResponse_);
			}
			if (((_bitField1 & 0x00000008) == 0x00000008))
			{
				Output.writeMessage(36, AuthChallenge_);
			}
			if (((_bitField1 & 0x00000010) == 0x00000010))
			{
				Output.writeMessage(37, AuthResponse_);
			}
			if (((_bitField1 & 0x00000020) == 0x00000020))
			{
				Output.writeMessage(38, AckResponse_);
			}
			if (((_bitField1 & 0x00000040) == 0x00000040))
			{
				Output.writeMessage(39, GetOrCreateSchema_);
			}
			if (((_bitField1 & 0x00000080) == 0x00000080))
			{
				Output.writeMessage(40, GetOrCreateSchemaResponse_);
			}
			if (((_bitField1 & 0x00000100) == 0x00000100))
			{
				Output.writeMessage(50, NewTxn_);
			}
			if (((_bitField1 & 0x00000200) == 0x00000200))
			{
				Output.writeMessage(51, NewTxnResponse_);
			}
			if (((_bitField1 & 0x00000400) == 0x00000400))
			{
				Output.writeMessage(52, AddPartitionToTxn_);
			}
			if (((_bitField1 & 0x00000800) == 0x00000800))
			{
				Output.writeMessage(53, AddPartitionToTxnResponse_);
			}
			if (((_bitField1 & 0x00001000) == 0x00001000))
			{
				Output.writeMessage(54, AddSubscriptionToTxn_);
			}
			if (((_bitField1 & 0x00002000) == 0x00002000))
			{
				Output.writeMessage(55, AddSubscriptionToTxnResponse_);
			}
			if (((_bitField1 & 0x00004000) == 0x00004000))
			{
				Output.writeMessage(56, EndTxn_);
			}
			if (((_bitField1 & 0x00008000) == 0x00008000))
			{
				Output.writeMessage(57, EndTxnResponse_);
			}
			if (((_bitField1 & 0x00010000) == 0x00010000))
			{
				Output.writeMessage(58, EndTxnOnPartition_);
			}
			if (((_bitField1 & 0x00020000) == 0x00020000))
			{
				Output.writeMessage(59, EndTxnOnPartitionResponse_);
			}
			if (((_bitField1 & 0x00040000) == 0x00040000))
			{
				Output.writeMessage(60, EndTxnOnSubscription_);
			}
			if (((_bitField1 & 0x00080000) == 0x00080000))
			{
				Output.writeMessage(61, _endTxnOnSubscriptionResponse);
			}
		}

		internal int MemoizedSerializedSize = -1;
		public int SerializedSize
		{
			get
			{
				int Size = MemoizedSerializedSize;
				if (Size != -1)
				{
					return Size;
				}

				Size = 0;
				if (((_bitField & 0x00000001) == 0x00000001))
				{
					Size += CodedOutputStream.computeEnumSize(1, _type.Number);
				}
				if (((_bitField & 0x00000002) == 0x00000002))
				{
					Size += CodedOutputStream.ComputeMessageSize(2, _connect);
				}
				if (((_bitField & 0x00000004) == 0x00000004))
				{
					Size += CodedOutputStream.ComputeMessageSize(3, Connected_);
				}
				if (((_bitField & 0x00000008) == 0x00000008))
				{
					Size += CodedOutputStream.ComputeMessageSize(4, Subscribe_);
				}
				if (((_bitField & 0x00000010) == 0x00000010))
				{
					Size += CodedOutputStream.ComputeMessageSize(5, Producer_);
				}
				if (((_bitField & 0x00000020) == 0x00000020))
				{
					Size += CodedOutputStream.ComputeMessageSize(6, Send_);
				}
				if (((_bitField & 0x00000040) == 0x00000040))
				{
					Size += CodedOutputStream.ComputeMessageSize(7, SendReceipt_);
				}
				if (((_bitField & 0x00000080) == 0x00000080))
				{
					Size += CodedOutputStream.ComputeMessageSize(8, SendError_);
				}
				if (((_bitField & 0x00000100) == 0x00000100))
				{
					Size += CodedOutputStream.ComputeMessageSize(9, Message_);
				}
				if (((_bitField & 0x00000200) == 0x00000200))
				{
					Size += CodedOutputStream.ComputeMessageSize(10, Ack_);
				}
				if (((_bitField & 0x00000400) == 0x00000400))
				{
					Size += CodedOutputStream.ComputeMessageSize(11, Flow_);
				}
				if (((_bitField & 0x00000800) == 0x00000800))
				{
					Size += CodedOutputStream.ComputeMessageSize(12, Unsubscribe_);
				}
				if (((_bitField & 0x00001000) == 0x00001000))
				{
					Size += CodedOutputStream.ComputeMessageSize(13, Success_);
				}
				if (((_bitField & 0x00002000) == 0x00002000))
				{
					Size += CodedOutputStream.ComputeMessageSize(14, Error_);
				}
				if (((_bitField & 0x00004000) == 0x00004000))
				{
					Size += CodedOutputStream.ComputeMessageSize(15, CloseProducer_);
				}
				if (((_bitField & 0x00008000) == 0x00008000))
				{
					Size += CodedOutputStream.ComputeMessageSize(16, CloseConsumer_);
				}
				if (((_bitField & 0x00010000) == 0x00010000))
				{
					Size += CodedOutputStream.ComputeMessageSize(17, ProducerSuccess_);
				}
				if (((_bitField & 0x00020000) == 0x00020000))
				{
					Size += CodedOutputStream.ComputeMessageSize(18, Ping_);
				}
				if (((_bitField & 0x00040000) == 0x00040000))
				{
					Size += CodedOutputStream.ComputeMessageSize(19, Pong_);
				}
				if (((_bitField & 0x00080000) == 0x00080000))
				{
					Size += CodedOutputStream.ComputeMessageSize(20, RedeliverUnacknowledgedMessages_);
				}
				if (((_bitField & 0x00100000) == 0x00100000))
				{
					Size += CodedOutputStream.ComputeMessageSize(21, PartitionMetadata_);
				}
				if (((_bitField & 0x00200000) == 0x00200000))
				{
					Size += CodedOutputStream.ComputeMessageSize(22, PartitionMetadataResponse_);
				}
				if (((_bitField & 0x00400000) == 0x00400000))
				{
					Size += CodedOutputStream.ComputeMessageSize(23, LookupTopic_);
				}
				if (((_bitField & 0x00800000) == 0x00800000))
				{
					Size += CodedOutputStream.ComputeMessageSize(24, LookupTopicResponse_);
				}
				if (((_bitField & 0x01000000) == 0x01000000))
				{
					Size += CodedOutputStream.ComputeMessageSize(25, ConsumerStats_);
				}
				if (((_bitField & 0x02000000) == 0x02000000))
				{
					Size += CodedOutputStream.ComputeMessageSize(26, ConsumerStatsResponse_);
				}
				if (((_bitField & 0x04000000) == 0x04000000))
				{
					Size += CodedOutputStream.ComputeMessageSize(27, ReachedEndOfTopic_);
				}
				if (((_bitField & 0x08000000) == 0x08000000))
				{
					Size += CodedOutputStream.ComputeMessageSize(28, Seek_);
				}
				if (((_bitField & 0x10000000) == 0x10000000))
				{
					Size += CodedOutputStream.ComputeMessageSize(29, GetLastMessageId_);
				}
				if (((_bitField & 0x20000000) == 0x20000000))
				{
					Size += CodedOutputStream.ComputeMessageSize(30, GetLastMessageIdResponse_);
				}
				if (((_bitField & 0x40000000) == 0x40000000))
				{
					Size += CodedOutputStream.ComputeMessageSize(31, ActiveConsumerChange_);
				}
				if (((_bitField & 0x80000000) == 0x80000000))
				{
					Size += CodedOutputStream.ComputeMessageSize(32, GetTopicsOfNamespace_);
				}
				if (((_bitField1 & 0x00000001) == 0x00000001))
				{
					Size += CodedOutputStream.ComputeMessageSize(33, GetTopicsOfNamespaceResponse_);
				}
				if (((_bitField1 & 0x00000002) == 0x00000002))
				{
					Size += CodedOutputStream.ComputeMessageSize(34, GetSchema_);
				}
				if (((_bitField1 & 0x00000004) == 0x00000004))
				{
					Size += CodedOutputStream.ComputeMessageSize(35, GetSchemaResponse_);
				}
				if (((_bitField1 & 0x00000008) == 0x00000008))
				{
					Size += CodedOutputStream.ComputeMessageSize(36, AuthChallenge_);
				}
				if (((_bitField1 & 0x00000010) == 0x00000010))
				{
					Size += CodedOutputStream.ComputeMessageSize(37, AuthResponse_);
				}
				if (((_bitField1 & 0x00000020) == 0x00000020))
				{
					Size += CodedOutputStream.ComputeMessageSize(38, AckResponse_);
				}
				if (((_bitField1 & 0x00000040) == 0x00000040))
				{
					Size += CodedOutputStream.ComputeMessageSize(39, GetOrCreateSchema_);
				}
				if (((_bitField1 & 0x00000080) == 0x00000080))
				{
					Size += CodedOutputStream.ComputeMessageSize(40, GetOrCreateSchemaResponse_);
				}
				if (((_bitField1 & 0x00000100) == 0x00000100))
				{
					Size += CodedOutputStream.ComputeMessageSize(50, NewTxn_);
				}
				if (((_bitField1 & 0x00000200) == 0x00000200))
				{
					Size += CodedOutputStream.ComputeMessageSize(51, NewTxnResponse_);
				}
				if (((_bitField1 & 0x00000400) == 0x00000400))
				{
					Size += CodedOutputStream.ComputeMessageSize(52, AddPartitionToTxn_);
				}
				if (((_bitField1 & 0x00000800) == 0x00000800))
				{
					Size += CodedOutputStream.ComputeMessageSize(53, AddPartitionToTxnResponse_);
				}
				if (((_bitField1 & 0x00001000) == 0x00001000))
				{
					Size += CodedOutputStream.ComputeMessageSize(54, AddSubscriptionToTxn_);
				}
				if (((_bitField1 & 0x00002000) == 0x00002000))
				{
					Size += CodedOutputStream.ComputeMessageSize(55, AddSubscriptionToTxnResponse_);
				}
				if (((_bitField1 & 0x00004000) == 0x00004000))
				{
					Size += CodedOutputStream.ComputeMessageSize(56, EndTxn_);
				}
				if (((_bitField1 & 0x00008000) == 0x00008000))
				{
					Size += CodedOutputStream.ComputeMessageSize(57, EndTxnResponse_);
				}
				if (((_bitField1 & 0x00010000) == 0x00010000))
				{
					Size += CodedOutputStream.ComputeMessageSize(58, EndTxnOnPartition_);
				}
				if (((_bitField1 & 0x00020000) == 0x00020000))
				{
					Size += CodedOutputStream.ComputeMessageSize(59, EndTxnOnPartitionResponse_);
				}
				if (((_bitField1 & 0x00040000) == 0x00040000))
				{
					Size += CodedOutputStream.ComputeMessageSize(60, EndTxnOnSubscription_);
				}
				if (((_bitField1 & 0x00080000) == 0x00080000))
				{
					Size += CodedOutputStream.ComputeMessageSize(_endTxnOnSubscriptionResponse);
				}
				MemoizedSerializedSize = Size;
				return Size;
			}
		}

		internal const long SerialVersionUID = 0L;
		//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		//ORIGINAL LINE: @java.lang.Override protected java.lang.Object writeReplace() throws java.io.ObjectStreamException
		public override object WriteReplace()
		{
			return base.WriteReplace();
		}

		//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		//ORIGINAL LINE: public static org.apache.pulsar.common.api.proto.BaseCommand parseFrom(org.apache.pulsar.ByteString data) throws org.apache.pulsar.InvalidProtocolBufferException
		public static BaseCommand ParseFrom(ByteString Data)
		{
			throw new Exception("Disabled");
		}
		//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		//ORIGINAL LINE: public static org.apache.pulsar.common.api.proto.BaseCommand parseFrom(org.apache.pulsar.ByteString data, org.apache.pulsar.ExtensionRegistryLite extensionRegistry) throws org.apache.pulsar.InvalidProtocolBufferException
		public static BaseCommand ParseFrom(ByteString Data, ExtensionRegistryLite ExtensionRegistry)
		{
			throw new Exception("Disabled");
		}
		//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		//ORIGINAL LINE: public static org.apache.pulsar.common.api.proto.BaseCommand parseFrom(byte[] data) throws org.apache.pulsar.InvalidProtocolBufferException
		public static BaseCommand ParseFrom(sbyte[] Data)
		{
			return NewBuilder().mergeFrom(Data).buildParsed();
		}
		//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		//ORIGINAL LINE: public static org.apache.pulsar.common.api.proto.BaseCommand parseFrom(byte[] data, org.apache.pulsar.ExtensionRegistryLite extensionRegistry) throws org.apache.pulsar.InvalidProtocolBufferException
		public static BaseCommand ParseFrom(sbyte[] Data, ExtensionRegistryLite ExtensionRegistry)
		{
			return NewBuilder().mergeFrom(Data, ExtensionRegistry).buildParsed();
		}
		//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		//ORIGINAL LINE: public static org.apache.pulsar.common.api.proto.BaseCommand parseFrom(java.io.InputStream input) throws java.io.IOException
		public static BaseCommand ParseFrom(Stream Input)
		{
			return NewBuilder().mergeFrom(Input).buildParsed();
		}
		//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		//ORIGINAL LINE: public static org.apache.pulsar.common.api.proto.BaseCommand parseFrom(java.io.InputStream input, org.apache.pulsar.ExtensionRegistryLite extensionRegistry) throws java.io.IOException
		public static BaseCommand ParseFrom(Stream Input, ExtensionRegistryLite ExtensionRegistry)
		{
			return NewBuilder().mergeFrom(Input, ExtensionRegistry).buildParsed();
		}
		//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		//ORIGINAL LINE: public static org.apache.pulsar.common.api.proto.BaseCommand parseDelimitedFrom(java.io.InputStream input) throws java.io.IOException
		public static BaseCommand ParseDelimitedFrom(Stream Input)
		{
			Builder Builder = NewBuilder();
			if (Builder.mergeDelimitedFrom(Input))
			{
				return Builder.buildParsed();
			}
			else
			{
				return null;
			}
		}
		//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		//ORIGINAL LINE: public static org.apache.pulsar.common.api.proto.BaseCommand parseDelimitedFrom(java.io.InputStream input, org.apache.pulsar.ExtensionRegistryLite extensionRegistry) throws java.io.IOException
		public static BaseCommand ParseDelimitedFrom(Stream Input, ExtensionRegistryLite ExtensionRegistry)
		{
			Builder Builder = NewBuilder();
			if (Builder.mergeDelimitedFrom(Input, ExtensionRegistry))
			{
				return Builder.buildParsed();
			}
			else
			{
				return null;
			}
		}
		//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		//ORIGINAL LINE: public static org.apache.pulsar.common.api.proto.BaseCommand parseFrom(org.apache.pulsar.CodedInputStream input) throws java.io.IOException
		public static BaseCommand ParseFrom(CodedInputStream Input)
		{
			return NewBuilder().mergeFrom(Input).buildParsed();
		}
		//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		//ORIGINAL LINE: public static org.apache.pulsar.common.api.proto.BaseCommand parseFrom(org.apache.pulsar.CodedInputStream input, org.apache.pulsar.ExtensionRegistryLite extensionRegistry) throws java.io.IOException
		public static BaseCommand ParseFrom(CodedInputStream Input, ExtensionRegistryLite ExtensionRegistry)
		{
			return NewBuilder().mergeFrom(Input, ExtensionRegistry).buildParsed();
		}

		public static Builder NewBuilder()
		{
			return Builder.Create();
		}
		public Builder NewBuilderForType()
		{
			return NewBuilder();
		}
		public static Builder NewBuilder(BaseCommand Prototype)
		{
			return NewBuilder().mergeFrom(Prototype);
		}
		public Builder ToBuilder()
		{
			return NewBuilder(this);
		}

		public sealed class Builder : GeneratedMessageLite.Builder<BaseCommand, Builder>, BaseCommandOrBuilder, ByteBufCodedInputStream.ByteBufMessageBuilder
		{
			// Construct using org.apache.pulsar.common.api.proto.BaseCommand.newBuilder()
			internal readonly io.netty.util.Recycler.Handle Handle;
			public Builder(io.netty.util.Recycler.Handle Handle)
			{
				this.Handle = Handle;
				MaybeForceBuilderInitialization();
			}
			internal static readonly io.netty.util.Recycler<Builder> RECYCLER = new RecyclerAnonymousInnerClass();

			public class RecyclerAnonymousInnerClass : io.netty.util.Recycler<Builder>
			{
				public Builder newObject(io.netty.util.Recycler.Handle Handle)
				{
					return new Builder(Handle);
				}
			}

			public void Recycle()
			{
				Clear();
				if (Handle != null)
				{
					RECYCLER.recycle(this, Handle);
				}
			}

			public void MaybeForceBuilderInitialization()
			{
			}
			internal static Builder Create()
			{
				return RECYCLER.get();
			}

			public Builder Clear()
			{
				base.Clear();
				_type = BaseCommand.Type.CONNECT;
				_bitField = (_bitField & ~0x00000001);
				_connect = CommandConnect.DefaultInstance;
				_bitField = (_bitField & ~0x00000002);
				Connected_ = CommandConnected.DefaultInstance;
				_bitField = (_bitField & ~0x00000004);
				Subscribe_ = CommandSubscribe.DefaultInstance;
				_bitField = (_bitField & ~0x00000008);
				Producer_ = CommandProducer.DefaultInstance;
				_bitField = (_bitField & ~0x00000010);
				Send_ = CommandSend.DefaultInstance;
				_bitField = (_bitField & ~0x00000020);
				SendReceipt_ = CommandSendReceipt.DefaultInstance;
				_bitField = (_bitField & ~0x00000040);
				SendError_ = CommandSendError.DefaultInstance;
				_bitField = (_bitField & ~0x00000080);
				Message_ = CommandMessage.DefaultInstance;
				_bitField = (_bitField & ~0x00000100);
				Ack_ = CommandAck.DefaultInstance;
				_bitField = (_bitField & ~0x00000200);
				Flow_ = CommandFlow.DefaultInstance;
				_bitField = (_bitField & ~0x00000400);
				Unsubscribe_ = CommandUnsubscribe.DefaultInstance;
				_bitField = (_bitField & ~0x00000800);
				Success_ = CommandSuccess.DefaultInstance;
				_bitField = (_bitField & ~0x00001000);
				Error_ = CommandError.DefaultInstance;
				_bitField = (_bitField & ~0x00002000);
				CloseProducer_ = CommandCloseProducer.DefaultInstance;
				_bitField = (_bitField & ~0x00004000);
				CloseConsumer_ = CommandCloseConsumer.DefaultInstance;
				_bitField = (_bitField & ~0x00008000);
				ProducerSuccess_ = CommandProducerSuccess.DefaultInstance;
				_bitField = (_bitField & ~0x00010000);
				Ping_ = CommandPing.DefaultInstance;
				_bitField = (_bitField & ~0x00020000);
				Pong_ = CommandPong.DefaultInstance;
				_bitField = (_bitField & ~0x00040000);
				RedeliverUnacknowledgedMessages_ = CommandRedeliverUnacknowledgedMessages.DefaultInstance;
				_bitField = (_bitField & ~0x00080000);
				PartitionMetadata_ = CommandPartitionedTopicMetadata.DefaultInstance;
				_bitField = (_bitField & ~0x00100000);
				PartitionMetadataResponse_ = CommandPartitionedTopicMetadataResponse.DefaultInstance;
				_bitField = (_bitField & ~0x00200000);
				LookupTopic_ = CommandLookupTopic.DefaultInstance;
				_bitField = (_bitField & ~0x00400000);
				LookupTopicResponse_ = CommandLookupTopicResponse.DefaultInstance;
				_bitField = (_bitField & ~0x00800000);
				ConsumerStats_ = CommandConsumerStats.DefaultInstance;
				_bitField = (_bitField & ~0x01000000);
				ConsumerStatsResponse_ = CommandConsumerStatsResponse.DefaultInstance;
				_bitField = (_bitField & ~0x02000000);
				ReachedEndOfTopic_ = CommandReachedEndOfTopic.DefaultInstance;
				_bitField = (_bitField & ~0x04000000);
				Seek_ = CommandSeek.DefaultInstance;
				_bitField = (_bitField & ~0x08000000);
				GetLastMessageId_ = CommandGetLastMessageId.DefaultInstance;
				_bitField = (_bitField & ~0x10000000);
				GetLastMessageIdResponse_ = CommandGetLastMessageIdResponse.DefaultInstance;
				_bitField = (_bitField & ~0x20000000);
				ActiveConsumerChange_ = CommandActiveConsumerChange.DefaultInstance;
				_bitField = (_bitField & ~0x40000000);
				GetTopicsOfNamespace_ = CommandGetTopicsOfNamespace.DefaultInstance;
				_bitField = (_bitField & ~0x80000000);
				GetTopicsOfNamespaceResponse_ = CommandGetTopicsOfNamespaceResponse.DefaultInstance;
				_bitField1 = (_bitField1 & ~0x00000001);
				GetSchema_ = CommandGetSchema.DefaultInstance;
				_bitField1 = (_bitField1 & ~0x00000002);
				GetSchemaResponse_ = CommandGetSchemaResponse.DefaultInstance;
				_bitField1 = (_bitField1 & ~0x00000004);
				AuthChallenge_ = CommandAuthChallenge.DefaultInstance;
				_bitField1 = (_bitField1 & ~0x00000008);
				AuthResponse_ = CommandAuthResponse.DefaultInstance;
				_bitField1 = (_bitField1 & ~0x00000010);
				AckResponse_ = CommandAckResponse.DefaultInstance;
				_bitField1 = (_bitField1 & ~0x00000020);
				GetOrCreateSchema_ = CommandGetOrCreateSchema.DefaultInstance;
				_bitField1 = (_bitField1 & ~0x00000040);
				GetOrCreateSchemaResponse_ = CommandGetOrCreateSchemaResponse.DefaultInstance;
				_bitField1 = (_bitField1 & ~0x00000080);
				NewTxn_ = CommandNewTxn.DefaultInstance;
				_bitField1 = (_bitField1 & ~0x00000100);
				NewTxnResponse_ = CommandNewTxnResponse.DefaultInstance;
				_bitField1 = (_bitField1 & ~0x00000200);
				AddPartitionToTxn_ = CommandAddPartitionToTxn.DefaultInstance;
				_bitField1 = (_bitField1 & ~0x00000400);
				AddPartitionToTxnResponse_ = CommandAddPartitionToTxnResponse.DefaultInstance;
				_bitField1 = (_bitField1 & ~0x00000800);
				AddSubscriptionToTxn_ = CommandAddSubscriptionToTxn.DefaultInstance;
				_bitField1 = (_bitField1 & ~0x00001000);
				AddSubscriptionToTxnResponse_ = CommandAddSubscriptionToTxnResponse.DefaultInstance;
				_bitField1 = (_bitField1 & ~0x00002000);
				EndTxn_ = CommandEndTxn.DefaultInstance;
				_bitField1 = (_bitField1 & ~0x00004000);
				EndTxnResponse_ = CommandEndTxnResponse.DefaultInstance;
				_bitField1 = (_bitField1 & ~0x00008000);
				EndTxnOnPartition_ = CommandEndTxnOnPartition.DefaultInstance;
				_bitField1 = (_bitField1 & ~0x00010000);
				EndTxnOnPartitionResponse_ = CommandEndTxnOnPartitionResponse.DefaultInstance;
				_bitField1 = (_bitField1 & ~0x00020000);
				EndTxnOnSubscription_ = CommandEndTxnOnSubscription.DefaultInstance;
				_bitField1 = (_bitField1 & ~0x00040000);
				_endTxnOnSubscriptionResponse = CommandEndTxnOnSubscriptionResponse.DefaultInstance;
				_bitField1 = (_bitField1 & ~0x00080000);
				return this;
			}

			public Builder Clone()
			{
				return Create().mergeFrom(BuildPartial());
			}

			public BaseCommand DefaultInstanceForType
			{
				get
				{
					return BaseCommand.DefaultInstance;
				}
			}

			public BaseCommand Build()
			{
				BaseCommand Result = BuildPartial();
				if (!Result.Initialized)
				{
					throw newUninitializedMessageException(Result);
				}
				return Result;
			}

			//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
			//ORIGINAL LINE: private org.apache.pulsar.common.api.proto.BaseCommand buildParsed() throws org.apache.pulsar.InvalidProtocolBufferException
			public BaseCommand BuildParsed()
			{
				BaseCommand Result = BuildPartial();
				if (!Result.Initialized)
				{
					throw newUninitializedMessageException(Result).asInvalidProtocolBufferException();
				}
				return Result;
			}

			public BaseCommand BuildPartial()
			{
				BaseCommand Result = BaseCommand.RECYCLER.get();
				int From_bitField = _bitField;
				int From_bitField1 = _bitField1;
				int To_bitField = 0;
				int To_bitField1 = 0;
				if (((From_bitField & 0x00000001) == 0x00000001))
				{
					To_bitField |= 0x00000001;
				}
				Result._type = _type;
				if (((From_bitField & 0x00000002) == 0x00000002))
				{
					To_bitField |= 0x00000002;
				}
				Result._connect = _connect;
				if (((From_bitField & 0x00000004) == 0x00000004))
				{
					To_bitField |= 0x00000004;
				}
				Result.Connected_ = Connected_;
				if (((From_bitField & 0x00000008) == 0x00000008))
				{
					To_bitField |= 0x00000008;
				}
				Result.Subscribe_ = Subscribe_;
				if (((From_bitField & 0x00000010) == 0x00000010))
				{
					To_bitField |= 0x00000010;
				}
				Result.Producer_ = Producer_;
				if (((From_bitField & 0x00000020) == 0x00000020))
				{
					To_bitField |= 0x00000020;
				}
				Result.Send_ = Send_;
				if (((From_bitField & 0x00000040) == 0x00000040))
				{
					To_bitField |= 0x00000040;
				}
				Result.SendReceipt_ = SendReceipt_;
				if (((From_bitField & 0x00000080) == 0x00000080))
				{
					To_bitField |= 0x00000080;
				}
				Result.SendError_ = SendError_;
				if (((From_bitField & 0x00000100) == 0x00000100))
				{
					To_bitField |= 0x00000100;
				}
				Result.Message_ = Message_;
				if (((From_bitField & 0x00000200) == 0x00000200))
				{
					To_bitField |= 0x00000200;
				}
				Result.Ack_ = Ack_;
				if (((From_bitField & 0x00000400) == 0x00000400))
				{
					To_bitField |= 0x00000400;
				}
				Result.Flow_ = Flow_;
				if (((From_bitField & 0x00000800) == 0x00000800))
				{
					To_bitField |= 0x00000800;
				}
				Result.Unsubscribe_ = Unsubscribe_;
				if (((From_bitField & 0x00001000) == 0x00001000))
				{
					To_bitField |= 0x00001000;
				}
				Result.Success_ = Success_;
				if (((From_bitField & 0x00002000) == 0x00002000))
				{
					To_bitField |= 0x00002000;
				}
				Result.Error_ = Error_;
				if (((From_bitField & 0x00004000) == 0x00004000))
				{
					To_bitField |= 0x00004000;
				}
				Result.CloseProducer_ = CloseProducer_;
				if (((From_bitField & 0x00008000) == 0x00008000))
				{
					To_bitField |= 0x00008000;
				}
				Result.CloseConsumer_ = CloseConsumer_;
				if (((From_bitField & 0x00010000) == 0x00010000))
				{
					To_bitField |= 0x00010000;
				}
				Result.ProducerSuccess_ = ProducerSuccess_;
				if (((From_bitField & 0x00020000) == 0x00020000))
				{
					To_bitField |= 0x00020000;
				}
				Result.Ping_ = Ping_;
				if (((From_bitField & 0x00040000) == 0x00040000))
				{
					To_bitField |= 0x00040000;
				}
				Result.Pong_ = Pong_;
				if (((From_bitField & 0x00080000) == 0x00080000))
				{
					To_bitField |= 0x00080000;
				}
				Result.RedeliverUnacknowledgedMessages_ = RedeliverUnacknowledgedMessages_;
				if (((From_bitField & 0x00100000) == 0x00100000))
				{
					To_bitField |= 0x00100000;
				}
				Result.PartitionMetadata_ = PartitionMetadata_;
				if (((From_bitField & 0x00200000) == 0x00200000))
				{
					To_bitField |= 0x00200000;
				}
				Result.PartitionMetadataResponse_ = PartitionMetadataResponse_;
				if (((From_bitField & 0x00400000) == 0x00400000))
				{
					To_bitField |= 0x00400000;
				}
				Result.LookupTopic_ = LookupTopic_;
				if (((From_bitField & 0x00800000) == 0x00800000))
				{
					To_bitField |= 0x00800000;
				}
				Result.LookupTopicResponse_ = LookupTopicResponse_;
				if (((From_bitField & 0x01000000) == 0x01000000))
				{
					To_bitField |= 0x01000000;
				}
				Result.ConsumerStats_ = ConsumerStats_;
				if (((From_bitField & 0x02000000) == 0x02000000))
				{
					To_bitField |= 0x02000000;
				}
				Result.ConsumerStatsResponse_ = ConsumerStatsResponse_;
				if (((From_bitField & 0x04000000) == 0x04000000))
				{
					To_bitField |= 0x04000000;
				}
				Result.ReachedEndOfTopic_ = ReachedEndOfTopic_;
				if (((From_bitField & 0x08000000) == 0x08000000))
				{
					To_bitField |= 0x08000000;
				}
				Result.Seek_ = Seek_;
				if (((From_bitField & 0x10000000) == 0x10000000))
				{
					To_bitField |= 0x10000000;
				}
				Result.GetLastMessageId_ = GetLastMessageId_;
				if (((From_bitField & 0x20000000) == 0x20000000))
				{
					To_bitField |= 0x20000000;
				}
				Result.GetLastMessageIdResponse_ = GetLastMessageIdResponse_;
				if (((From_bitField & 0x40000000) == 0x40000000))
				{
					To_bitField |= 0x40000000;
				}
				Result.ActiveConsumerChange_ = ActiveConsumerChange_;
				if (((From_bitField & 0x80000000) == 0x80000000))
				{
					To_bitField |= unchecked((int)0x80000000);
				}
				Result.GetTopicsOfNamespace_ = GetTopicsOfNamespace_;
				if (((From_bitField1 & 0x00000001) == 0x00000001))
				{
					To_bitField1 |= 0x00000001;
				}
				Result.GetTopicsOfNamespaceResponse_ = GetTopicsOfNamespaceResponse_;
				if (((From_bitField1 & 0x00000002) == 0x00000002))
				{
					To_bitField1 |= 0x00000002;
				}
				Result.GetSchema_ = GetSchema_;
				if (((From_bitField1 & 0x00000004) == 0x00000004))
				{
					To_bitField1 |= 0x00000004;
				}
				Result.GetSchemaResponse_ = GetSchemaResponse_;
				if (((From_bitField1 & 0x00000008) == 0x00000008))
				{
					To_bitField1 |= 0x00000008;
				}
				Result.AuthChallenge_ = AuthChallenge_;
				if (((From_bitField1 & 0x00000010) == 0x00000010))
				{
					To_bitField1 |= 0x00000010;
				}
				Result.AuthResponse_ = AuthResponse_;
				if (((From_bitField1 & 0x00000020) == 0x00000020))
				{
					To_bitField1 |= 0x00000020;
				}
				Result.AckResponse_ = AckResponse_;
				if (((From_bitField1 & 0x00000040) == 0x00000040))
				{
					To_bitField1 |= 0x00000040;
				}
				Result.GetOrCreateSchema_ = GetOrCreateSchema_;
				if (((From_bitField1 & 0x00000080) == 0x00000080))
				{
					To_bitField1 |= 0x00000080;
				}
				Result.GetOrCreateSchemaResponse_ = GetOrCreateSchemaResponse_;
				if (((From_bitField1 & 0x00000100) == 0x00000100))
				{
					To_bitField1 |= 0x00000100;
				}
				Result.NewTxn_ = NewTxn_;
				if (((From_bitField1 & 0x00000200) == 0x00000200))
				{
					To_bitField1 |= 0x00000200;
				}
				Result.NewTxnResponse_ = NewTxnResponse_;
				if (((From_bitField1 & 0x00000400) == 0x00000400))
				{
					To_bitField1 |= 0x00000400;
				}
				Result.AddPartitionToTxn_ = AddPartitionToTxn_;
				if (((From_bitField1 & 0x00000800) == 0x00000800))
				{
					To_bitField1 |= 0x00000800;
				}
				Result.AddPartitionToTxnResponse_ = AddPartitionToTxnResponse_;
				if (((From_bitField1 & 0x00001000) == 0x00001000))
				{
					To_bitField1 |= 0x00001000;
				}
				Result.AddSubscriptionToTxn_ = AddSubscriptionToTxn_;
				if (((From_bitField1 & 0x00002000) == 0x00002000))
				{
					To_bitField1 |= 0x00002000;
				}
				Result.AddSubscriptionToTxnResponse_ = AddSubscriptionToTxnResponse_;
				if (((From_bitField1 & 0x00004000) == 0x00004000))
				{
					To_bitField1 |= 0x00004000;
				}
				Result.EndTxn_ = EndTxn_;
				if (((From_bitField1 & 0x00008000) == 0x00008000))
				{
					To_bitField1 |= 0x00008000;
				}
				Result.EndTxnResponse_ = EndTxnResponse_;
				if (((From_bitField1 & 0x00010000) == 0x00010000))
				{
					To_bitField1 |= 0x00010000;
				}
				Result.EndTxnOnPartition_ = EndTxnOnPartition_;
				if (((From_bitField1 & 0x00020000) == 0x00020000))
				{
					To_bitField1 |= 0x00020000;
				}
				Result.EndTxnOnPartitionResponse_ = EndTxnOnPartitionResponse_;
				if (((From_bitField1 & 0x00040000) == 0x00040000))
				{
					To_bitField1 |= 0x00040000;
				}
				Result.EndTxnOnSubscription_ = EndTxnOnSubscription_;
				if (((From_bitField1 & 0x00080000) == 0x00080000))
				{
					To_bitField1 |= 0x00080000;
				}
				Result._endTxnOnSubscriptionResponse = _endTxnOnSubscriptionResponse;
				Result._bitField = To_bitField;
				Result._bitField1 = To_bitField1;
				return Result;
			}

			public Builder MergeFrom(BaseCommand Other)
			{
				if (Other == BaseCommand.DefaultInstance)
				{
					return this;
				}
				if (Other.hasType())
				{
					Type = Other.getType();
				}
				if (Other.hasConnect())
				{
					MergeConnect(Other.Connect);
				}
				if (Other.hasConnected())
				{
					MergeConnected(Other.Connected);
				}
				if (Other.hasSubscribe())
				{
					MergeSubscribe(Other.Subscribe);
				}
				if (Other.hasProducer())
				{
					MergeProducer(Other.Producer);
				}
				if (Other.hasSend())
				{
					MergeSend(Other.Send);
				}
				if (Other.hasSendReceipt())
				{
					MergeSendReceipt(Other.SendReceipt);
				}
				if (Other.hasSendError())
				{
					MergeSendError(Other.SendError);
				}
				if (Other.hasMessage())
				{
					MergeMessage(Other.Message);
				}
				if (Other.hasAck())
				{
					MergeAck(Other.Ack);
				}
				if (Other.hasFlow())
				{
					MergeFlow(Other.Flow);
				}
				if (Other.hasUnsubscribe())
				{
					MergeUnsubscribe(Other.Unsubscribe);
				}
				if (Other.hasSuccess())
				{
					MergeSuccess(Other.Success);
				}
				if (Other.hasError())
				{
					MergeError(Other.Error);
				}
				if (Other.hasCloseProducer())
				{
					MergeCloseProducer(Other.CloseProducer);
				}
				if (Other.hasCloseConsumer())
				{
					MergeCloseConsumer(Other.CloseConsumer);
				}
				if (Other.hasProducerSuccess())
				{
					MergeProducerSuccess(Other.ProducerSuccess);
				}
				if (Other.hasPing())
				{
					MergePing(Other.Ping);
				}
				if (Other.hasPong())
				{
					MergePong(Other.Pong);
				}
				if (Other.hasRedeliverUnacknowledgedMessages())
				{
					MergeRedeliverUnacknowledgedMessages(Other.RedeliverUnacknowledgedMessages);
				}
				if (Other.hasPartitionMetadata())
				{
					MergePartitionMetadata(Other.PartitionMetadata);
				}
				if (Other.hasPartitionMetadataResponse())
				{
					MergePartitionMetadataResponse(Other.PartitionMetadataResponse);
				}
				if (Other.hasLookupTopic())
				{
					MergeLookupTopic(Other.LookupTopic);
				}
				if (Other.hasLookupTopicResponse())
				{
					MergeLookupTopicResponse(Other.LookupTopicResponse);
				}
				if (Other.hasConsumerStats())
				{
					MergeConsumerStats(Other.ConsumerStats);
				}
				if (Other.hasConsumerStatsResponse())
				{
					MergeConsumerStatsResponse(Other.ConsumerStatsResponse);
				}
				if (Other.hasReachedEndOfTopic())
				{
					MergeReachedEndOfTopic(Other.ReachedEndOfTopic);
				}
				if (Other.hasSeek())
				{
					MergeSeek(Other.Seek);
				}
				if (Other.hasGetLastMessageId())
				{
					MergeGetLastMessageId(Other.GetLastMessageId);
				}
				if (Other.hasGetLastMessageIdResponse())
				{
					MergeGetLastMessageIdResponse(Other.GetLastMessageIdResponse);
				}
				if (Other.hasActiveConsumerChange())
				{
					MergeActiveConsumerChange(Other.ActiveConsumerChange);
				}
				if (Other.hasGetTopicsOfNamespace())
				{
					MergeGetTopicsOfNamespace(Other.GetTopicsOfNamespace);
				}
				if (Other.hasGetTopicsOfNamespaceResponse())
				{
					MergeGetTopicsOfNamespaceResponse(Other.GetTopicsOfNamespaceResponse);
				}
				if (Other.hasGetSchema())
				{
					MergeGetSchema(Other.GetSchema);
				}
				if (Other.hasGetSchemaResponse())
				{
					MergeGetSchemaResponse(Other.GetSchemaResponse);
				}
				if (Other.hasAuthChallenge())
				{
					MergeAuthChallenge(Other.AuthChallenge);
				}
				if (Other.hasAuthResponse())
				{
					MergeAuthResponse(Other.AuthResponse);
				}
				if (Other.hasAckResponse())
				{
					MergeAckResponse(Other.AckResponse);
				}
				if (Other.hasGetOrCreateSchema())
				{
					MergeGetOrCreateSchema(Other.GetOrCreateSchema);
				}
				if (Other.hasGetOrCreateSchemaResponse())
				{
					MergeGetOrCreateSchemaResponse(Other.GetOrCreateSchemaResponse);
				}
				if (Other.hasNewTxn())
				{
					MergeNewTxn(Other.NewTxn);
				}
				if (Other.hasNewTxnResponse())
				{
					MergeNewTxnResponse(Other.NewTxnResponse);
				}
				if (Other.hasAddPartitionToTxn())
				{
					MergeAddPartitionToTxn(Other.AddPartitionToTxn);
				}
				if (Other.hasAddPartitionToTxnResponse())
				{
					MergeAddPartitionToTxnResponse(Other.AddPartitionToTxnResponse);
				}
				if (Other.hasAddSubscriptionToTxn())
				{
					MergeAddSubscriptionToTxn(Other.AddSubscriptionToTxn);
				}
				if (Other.hasAddSubscriptionToTxnResponse())
				{
					MergeAddSubscriptionToTxnResponse(Other.AddSubscriptionToTxnResponse);
				}
				if (Other.hasEndTxn())
				{
					MergeEndTxn(Other.EndTxn);
				}
				if (Other.hasEndTxnResponse())
				{
					MergeEndTxnResponse(Other.EndTxnResponse);
				}
				if (Other.hasEndTxnOnPartition())
				{
					MergeEndTxnOnPartition(Other.EndTxnOnPartition);
				}
				if (Other.hasEndTxnOnPartitionResponse())
				{
					MergeEndTxnOnPartitionResponse(Other.EndTxnOnPartitionResponse);
				}
				if (Other.hasEndTxnOnSubscription())
				{
					MergeEndTxnOnSubscription(Other.EndTxnOnSubscription);
				}
				if (Other.hasEndTxnOnSubscriptionResponse())
				{
					MergeEndTxnOnSubscriptionResponse(Other.EndTxnOnSubscriptionResponse);
				}
				return this;
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
						if (!getConnect().Initialized)
						{

							return false;
						}
					}
					if (HasConnected())
					{
						if (!getConnected().Initialized)
						{

							return false;
						}
					}
					if (HasSubscribe())
					{
						if (!getSubscribe().Initialized)
						{

							return false;
						}
					}
					if (HasProducer())
					{
						if (!getProducer().Initialized)
						{

							return false;
						}
					}
					if (HasSend())
					{
						if (!getSend().Initialized)
						{

							return false;
						}
					}
					if (HasSendReceipt())
					{
						if (!getSendReceipt().Initialized)
						{

							return false;
						}
					}
					if (HasSendError())
					{
						if (!getSendError().Initialized)
						{

							return false;
						}
					}
					if (HasMessage())
					{
						if (!getMessage().Initialized)
						{

							return false;
						}
					}
					if (HasAck())
					{
						if (!getAck().Initialized)
						{

							return false;
						}
					}
					if (HasFlow())
					{
						if (!getFlow().Initialized)
						{

							return false;
						}
					}
					if (HasUnsubscribe())
					{
						if (!getUnsubscribe().Initialized)
						{

							return false;
						}
					}
					if (HasSuccess())
					{
						if (!getSuccess().Initialized)
						{

							return false;
						}
					}
					if (HasError())
					{
						if (!getError().Initialized)
						{

							return false;
						}
					}
					if (HasCloseProducer())
					{
						if (!getCloseProducer().Initialized)
						{

							return false;
						}
					}
					if (HasCloseConsumer())
					{
						if (!getCloseConsumer().Initialized)
						{

							return false;
						}
					}
					if (HasProducerSuccess())
					{
						if (!getProducerSuccess().Initialized)
						{

							return false;
						}
					}
					if (HasRedeliverUnacknowledgedMessages())
					{
						if (!getRedeliverUnacknowledgedMessages().Initialized)
						{

							return false;
						}
					}
					if (HasPartitionMetadata())
					{
						if (!getPartitionMetadata().Initialized)
						{

							return false;
						}
					}
					if (HasPartitionMetadataResponse())
					{
						if (!getPartitionMetadataResponse().Initialized)
						{

							return false;
						}
					}
					if (HasLookupTopic())
					{
						if (!getLookupTopic().Initialized)
						{

							return false;
						}
					}
					if (HasLookupTopicResponse())
					{
						if (!getLookupTopicResponse().Initialized)
						{

							return false;
						}
					}
					if (HasConsumerStats())
					{
						if (!getConsumerStats().Initialized)
						{

							return false;
						}
					}
					if (HasConsumerStatsResponse())
					{
						if (!getConsumerStatsResponse().Initialized)
						{

							return false;
						}
					}
					if (HasReachedEndOfTopic())
					{
						if (!getReachedEndOfTopic().Initialized)
						{

							return false;
						}
					}
					if (HasSeek())
					{
						if (!getSeek().Initialized)
						{

							return false;
						}
					}
					if (HasGetLastMessageId())
					{
						if (!getGetLastMessageId().Initialized)
						{

							return false;
						}
					}
					if (HasGetLastMessageIdResponse())
					{
						if (!getGetLastMessageIdResponse().Initialized)
						{

							return false;
						}
					}
					if (HasActiveConsumerChange())
					{
						if (!getActiveConsumerChange().Initialized)
						{

							return false;
						}
					}
					if (HasGetTopicsOfNamespace())
					{
						if (!getGetTopicsOfNamespace().Initialized)
						{

							return false;
						}
					}
					if (HasGetTopicsOfNamespaceResponse())
					{
						if (!getGetTopicsOfNamespaceResponse().Initialized)
						{

							return false;
						}
					}
					if (HasGetSchema())
					{
						if (!getGetSchema().Initialized)
						{

							return false;
						}
					}
					if (HasGetSchemaResponse())
					{
						if (!getGetSchemaResponse().Initialized)
						{

							return false;
						}
					}
					if (HasAckResponse())
					{
						if (!getAckResponse().Initialized)
						{

							return false;
						}
					}
					if (HasGetOrCreateSchema())
					{
						if (!getGetOrCreateSchema().Initialized)
						{

							return false;
						}
					}
					if (HasGetOrCreateSchemaResponse())
					{
						if (!getGetOrCreateSchemaResponse().Initialized)
						{

							return false;
						}
					}
					if (HasNewTxn())
					{
						if (!getNewTxn().Initialized)
						{

							return false;
						}
					}
					if (HasNewTxnResponse())
					{
						if (!getNewTxnResponse().Initialized)
						{

							return false;
						}
					}
					if (HasAddPartitionToTxn())
					{
						if (!getAddPartitionToTxn().Initialized)
						{

							return false;
						}
					}
					if (HasAddPartitionToTxnResponse())
					{
						if (!getAddPartitionToTxnResponse().Initialized)
						{

							return false;
						}
					}
					if (HasAddSubscriptionToTxn())
					{
						if (!getAddSubscriptionToTxn().Initialized)
						{

							return false;
						}
					}
					if (HasAddSubscriptionToTxnResponse())
					{
						if (!getAddSubscriptionToTxnResponse().Initialized)
						{

							return false;
						}
					}
					if (HasEndTxn())
					{
						if (!getEndTxn().Initialized)
						{

							return false;
						}
					}
					if (HasEndTxnResponse())
					{
						if (!getEndTxnResponse().Initialized)
						{

							return false;
						}
					}
					if (HasEndTxnOnPartition())
					{
						if (!getEndTxnOnPartition().Initialized)
						{

							return false;
						}
					}
					if (HasEndTxnOnPartitionResponse())
					{
						if (!getEndTxnOnPartitionResponse().Initialized)
						{

							return false;
						}
					}
					if (HasEndTxnOnSubscription())
					{
						if (!getEndTxnOnSubscription().Initialized)
						{

							return false;
						}
					}
					if (HasEndTxnOnSubscriptionResponse())
					{
						if (!getEndTxnOnSubscriptionResponse().Initialized)
						{

							return false;
						}
					}
					return true;
				}
			}

			//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
			//ORIGINAL LINE: public Builder mergeFrom(org.apache.pulsar.CodedInputStream input, org.apache.pulsar.ExtensionRegistryLite extensionRegistry) throws java.io.IOException
			public Builder MergeFrom(CodedInputStream Input, ExtensionRegistryLite ExtensionRegistry)
			{
				throw new java.io.IOException("Merge from CodedInputStream is disabled");
			}
			//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
			//ORIGINAL LINE: public Builder mergeFrom(org.apache.pulsar.common.util.protobuf.ByteBufCodedInputStream input, org.apache.pulsar.ExtensionRegistryLite extensionRegistry) throws java.io.IOException
			public Builder MergeFrom(ByteBufCodedInputStream Input, ExtensionRegistryLite ExtensionRegistry)
			{
				while (true)
				{
					int Tag = Input.readTag();
					switch (Tag)
					{
						case 0:

							return this;
						default:
							{
								if (!Input.skipField(Tag))
								{

									return this;
								}
								break;
							}
						case 8:
							{
								int RawValue = Input.readEnum();
								BaseCommand.Type Value = BaseCommand.Type.valueOf(RawValue);
								if (Value != null)
								{
									_bitField |= 0x00000001;
									_type = Value;
								}
								break;
							}
						case 18:
							{
								CommandConnect.Builder SubBuilder = CommandConnect.NewBuilder();
								if (HasConnect())
								{
									SubBuilder.mergeFrom(getConnect());
								}
								Input.readMessage(SubBuilder, ExtensionRegistry);
								SetConnect(SubBuilder.buildPartial());
								SubBuilder.recycle();
								break;
							}
						case 26:
							{
								CommandConnected.Builder SubBuilder = CommandConnected.NewBuilder();
								if (HasConnected())
								{
									SubBuilder.mergeFrom(getConnected());
								}
								Input.readMessage(SubBuilder, ExtensionRegistry);
								SetConnected(SubBuilder.buildPartial());
								SubBuilder.recycle();
								break;
							}
						case 34:
							{
								CommandSubscribe.Builder SubBuilder = CommandSubscribe.NewBuilder();
								if (HasSubscribe())
								{
									SubBuilder.mergeFrom(getSubscribe());
								}
								Input.readMessage(SubBuilder, ExtensionRegistry);
								SetSubscribe(SubBuilder.buildPartial());
								SubBuilder.recycle();
								break;
							}
						case 42:
							{
								CommandProducer.Builder SubBuilder = CommandProducer.NewBuilder();
								if (HasProducer())
								{
									SubBuilder.mergeFrom(getProducer());
								}
								Input.readMessage(SubBuilder, ExtensionRegistry);
								SetProducer(SubBuilder.buildPartial());
								SubBuilder.recycle();
								break;
							}
						case 50:
							{
								CommandSend.Builder SubBuilder = CommandSend.NewBuilder();
								if (HasSend())
								{
									SubBuilder.mergeFrom(getSend());
								}
								Input.readMessage(SubBuilder, ExtensionRegistry);
								SetSend(SubBuilder.buildPartial());
								SubBuilder.recycle();
								break;
							}
						case 58:
							{
								CommandSendReceipt.Builder SubBuilder = CommandSendReceipt.NewBuilder();
								if (HasSendReceipt())
								{
									SubBuilder.mergeFrom(getSendReceipt());
								}
								Input.readMessage(SubBuilder, ExtensionRegistry);
								SetSendReceipt(SubBuilder.buildPartial());
								SubBuilder.recycle();
								break;
							}
						case 66:
							{
								CommandSendError.Builder SubBuilder = CommandSendError.NewBuilder();
								if (HasSendError())
								{
									SubBuilder.mergeFrom(getSendError());
								}
								Input.readMessage(SubBuilder, ExtensionRegistry);
								SetSendError(SubBuilder.buildPartial());
								SubBuilder.recycle();
								break;
							}
						case 74:
							{
								CommandMessage.Builder SubBuilder = CommandMessage.NewBuilder();
								if (HasMessage())
								{
									SubBuilder.mergeFrom(getMessage());
								}
								Input.readMessage(SubBuilder, ExtensionRegistry);
								SetMessage(SubBuilder.buildPartial());
								SubBuilder.recycle();
								break;
							}
						case 82:
							{
								CommandAck.Builder SubBuilder = CommandAck.NewBuilder();
								if (HasAck())
								{
									SubBuilder.mergeFrom(getAck());
								}
								Input.readMessage(SubBuilder, ExtensionRegistry);
								SetAck(SubBuilder.buildPartial());
								SubBuilder.recycle();
								break;
							}
						case 90:
							{
								CommandFlow.Builder SubBuilder = CommandFlow.NewBuilder();
								if (HasFlow())
								{
									SubBuilder.mergeFrom(getFlow());
								}
								Input.readMessage(SubBuilder, ExtensionRegistry);
								SetFlow(SubBuilder.buildPartial());
								SubBuilder.recycle();
								break;
							}
						case 98:
							{
								CommandUnsubscribe.Builder SubBuilder = CommandUnsubscribe.NewBuilder();
								if (HasUnsubscribe())
								{
									SubBuilder.mergeFrom(getUnsubscribe());
								}
								Input.readMessage(SubBuilder, ExtensionRegistry);
								SetUnsubscribe(SubBuilder.buildPartial());
								SubBuilder.recycle();
								break;
							}
						case 106:
							{
								CommandSuccess.Builder SubBuilder = CommandSuccess.NewBuilder();
								if (HasSuccess())
								{
									SubBuilder.mergeFrom(getSuccess());
								}
								Input.readMessage(SubBuilder, ExtensionRegistry);
								SetSuccess(SubBuilder.buildPartial());
								SubBuilder.recycle();
								break;
							}
						case 114:
							{
								CommandError.Builder SubBuilder = CommandError.NewBuilder();
								if (HasError())
								{
									SubBuilder.mergeFrom(getError());
								}
								Input.readMessage(SubBuilder, ExtensionRegistry);
								SetError(SubBuilder.buildPartial());
								SubBuilder.recycle();
								break;
							}
						case 122:
							{
								CommandCloseProducer.Builder SubBuilder = CommandCloseProducer.NewBuilder();
								if (HasCloseProducer())
								{
									SubBuilder.mergeFrom(getCloseProducer());
								}
								Input.readMessage(SubBuilder, ExtensionRegistry);
								SetCloseProducer(SubBuilder.buildPartial());
								SubBuilder.recycle();
								break;
							}
						case 130:
							{
								CommandCloseConsumer.Builder SubBuilder = CommandCloseConsumer.NewBuilder();
								if (HasCloseConsumer())
								{
									SubBuilder.mergeFrom(getCloseConsumer());
								}
								Input.readMessage(SubBuilder, ExtensionRegistry);
								SetCloseConsumer(SubBuilder.buildPartial());
								SubBuilder.recycle();
								break;
							}
						case 138:
							{
								CommandProducerSuccess.Builder SubBuilder = CommandProducerSuccess.NewBuilder();
								if (HasProducerSuccess())
								{
									SubBuilder.mergeFrom(getProducerSuccess());
								}
								Input.readMessage(SubBuilder, ExtensionRegistry);
								SetProducerSuccess(SubBuilder.buildPartial());
								SubBuilder.recycle();
								break;
							}
						case 146:
							{
								CommandPing.Builder SubBuilder = CommandPing.NewBuilder();
								if (HasPing())
								{
									SubBuilder.mergeFrom(getPing());
								}
								Input.readMessage(SubBuilder, ExtensionRegistry);
								SetPing(SubBuilder.buildPartial());
								SubBuilder.recycle();
								break;
							}
						case 154:
							{
								CommandPong.Builder SubBuilder = CommandPong.NewBuilder();
								if (HasPong())
								{
									SubBuilder.mergeFrom(getPong());
								}
								Input.readMessage(SubBuilder, ExtensionRegistry);
								SetPong(SubBuilder.buildPartial());
								SubBuilder.recycle();
								break;
							}
						case 162:
							{
								CommandRedeliverUnacknowledgedMessages.Builder SubBuilder = CommandRedeliverUnacknowledgedMessages.NewBuilder();
								if (HasRedeliverUnacknowledgedMessages())
								{
									SubBuilder.mergeFrom(getRedeliverUnacknowledgedMessages());
								}
								Input.readMessage(SubBuilder, ExtensionRegistry);
								SetRedeliverUnacknowledgedMessages(SubBuilder.buildPartial());
								SubBuilder.recycle();
								break;
							}
						case 170:
							{
								CommandPartitionedTopicMetadata.Builder SubBuilder = CommandPartitionedTopicMetadata.NewBuilder();
								if (HasPartitionMetadata())
								{
									SubBuilder.mergeFrom(getPartitionMetadata());
								}
								Input.readMessage(SubBuilder, ExtensionRegistry);
								SetPartitionMetadata(SubBuilder.buildPartial());
								SubBuilder.recycle();
								break;
							}
						case 178:
							{
								CommandPartitionedTopicMetadataResponse.Builder SubBuilder = CommandPartitionedTopicMetadataResponse.NewBuilder();
								if (HasPartitionMetadataResponse())
								{
									SubBuilder.mergeFrom(getPartitionMetadataResponse());
								}
								Input.readMessage(SubBuilder, ExtensionRegistry);
								SetPartitionMetadataResponse(SubBuilder.buildPartial());
								SubBuilder.recycle();
								break;
							}
						case 186:
							{
								CommandLookupTopic.Builder SubBuilder = CommandLookupTopic.NewBuilder();
								if (HasLookupTopic())
								{
									SubBuilder.mergeFrom(getLookupTopic());
								}
								Input.readMessage(SubBuilder, ExtensionRegistry);
								SetLookupTopic(SubBuilder.buildPartial());
								SubBuilder.recycle();
								break;
							}
						case 194:
							{
								CommandLookupTopicResponse.Builder SubBuilder = CommandLookupTopicResponse.NewBuilder();
								if (HasLookupTopicResponse())
								{
									SubBuilder.mergeFrom(getLookupTopicResponse());
								}
								Input.readMessage(SubBuilder, ExtensionRegistry);
								SetLookupTopicResponse(SubBuilder.buildPartial());
								SubBuilder.recycle();
								break;
							}
						case 202:
							{
								CommandConsumerStats.Builder SubBuilder = CommandConsumerStats.NewBuilder();
								if (HasConsumerStats())
								{
									SubBuilder.mergeFrom(getConsumerStats());
								}
								Input.readMessage(SubBuilder, ExtensionRegistry);
								SetConsumerStats(SubBuilder.buildPartial());
								SubBuilder.recycle();
								break;
							}
						case 210:
							{
								CommandConsumerStatsResponse.Builder SubBuilder = CommandConsumerStatsResponse.NewBuilder();
								if (HasConsumerStatsResponse())
								{
									SubBuilder.mergeFrom(getConsumerStatsResponse());
								}
								Input.readMessage(SubBuilder, ExtensionRegistry);
								SetConsumerStatsResponse(SubBuilder.buildPartial());
								SubBuilder.recycle();
								break;
							}
						case 218:
							{
								CommandReachedEndOfTopic.Builder SubBuilder = CommandReachedEndOfTopic.NewBuilder();
								if (HasReachedEndOfTopic())
								{
									SubBuilder.mergeFrom(getReachedEndOfTopic());
								}
								Input.readMessage(SubBuilder, ExtensionRegistry);
								SetReachedEndOfTopic(SubBuilder.buildPartial());
								SubBuilder.recycle();
								break;
							}
						case 226:
							{
								CommandSeek.Builder SubBuilder = CommandSeek.NewBuilder();
								if (HasSeek())
								{
									SubBuilder.mergeFrom(getSeek());
								}
								Input.readMessage(SubBuilder, ExtensionRegistry);
								SetSeek(SubBuilder.buildPartial());
								SubBuilder.recycle();
								break;
							}
						case 234:
							{
								CommandGetLastMessageId.Builder SubBuilder = CommandGetLastMessageId.NewBuilder();
								if (HasGetLastMessageId())
								{
									SubBuilder.mergeFrom(getGetLastMessageId());
								}
								Input.readMessage(SubBuilder, ExtensionRegistry);
								SetGetLastMessageId(SubBuilder.buildPartial());
								SubBuilder.recycle();
								break;
							}
						case 242:
							{
								CommandGetLastMessageIdResponse.Builder SubBuilder = CommandGetLastMessageIdResponse.NewBuilder();
								if (HasGetLastMessageIdResponse())
								{
									SubBuilder.mergeFrom(getGetLastMessageIdResponse());
								}
								Input.readMessage(SubBuilder, ExtensionRegistry);
								SetGetLastMessageIdResponse(SubBuilder.buildPartial());
								SubBuilder.recycle();
								break;
							}
						case 250:
							{
								CommandActiveConsumerChange.Builder SubBuilder = CommandActiveConsumerChange.NewBuilder();
								if (HasActiveConsumerChange())
								{
									SubBuilder.mergeFrom(getActiveConsumerChange());
								}
								Input.readMessage(SubBuilder, ExtensionRegistry);
								SetActiveConsumerChange(SubBuilder.buildPartial());
								SubBuilder.recycle();
								break;
							}
						case 258:
							{
								CommandGetTopicsOfNamespace.Builder SubBuilder = CommandGetTopicsOfNamespace.NewBuilder();
								if (HasGetTopicsOfNamespace())
								{
									SubBuilder.mergeFrom(getGetTopicsOfNamespace());
								}
								Input.readMessage(SubBuilder, ExtensionRegistry);
								SetGetTopicsOfNamespace(SubBuilder.buildPartial());
								SubBuilder.recycle();
								break;
							}
						case 266:
							{
								CommandGetTopicsOfNamespaceResponse.Builder SubBuilder = CommandGetTopicsOfNamespaceResponse.NewBuilder();
								if (HasGetTopicsOfNamespaceResponse())
								{
									SubBuilder.mergeFrom(getGetTopicsOfNamespaceResponse());
								}
								Input.readMessage(SubBuilder, ExtensionRegistry);
								SetGetTopicsOfNamespaceResponse(SubBuilder.buildPartial());
								SubBuilder.recycle();
								break;
							}
						case 274:
							{
								CommandGetSchema.Builder SubBuilder = CommandGetSchema.NewBuilder();
								if (HasGetSchema())
								{
									SubBuilder.mergeFrom(getGetSchema());
								}
								Input.readMessage(SubBuilder, ExtensionRegistry);
								SetGetSchema(SubBuilder.buildPartial());
								SubBuilder.recycle();
								break;
							}
						case 282:
							{
								CommandGetSchemaResponse.Builder SubBuilder = CommandGetSchemaResponse.NewBuilder();
								if (HasGetSchemaResponse())
								{
									SubBuilder.mergeFrom(getGetSchemaResponse());
								}
								Input.readMessage(SubBuilder, ExtensionRegistry);
								SetGetSchemaResponse(SubBuilder.buildPartial());
								SubBuilder.recycle();
								break;
							}
						case 290:
							{
								CommandAuthChallenge.Builder SubBuilder = CommandAuthChallenge.NewBuilder();
								if (HasAuthChallenge())
								{
									SubBuilder.mergeFrom(getAuthChallenge());
								}
								Input.readMessage(SubBuilder, ExtensionRegistry);
								SetAuthChallenge(SubBuilder.buildPartial());
								SubBuilder.recycle();
								break;
							}
						case 298:
							{
								CommandAuthResponse.Builder SubBuilder = CommandAuthResponse.NewBuilder();
								if (HasAuthResponse())
								{
									SubBuilder.mergeFrom(getAuthResponse());
								}
								Input.readMessage(SubBuilder, ExtensionRegistry);
								SetAuthResponse(SubBuilder.buildPartial());
								SubBuilder.recycle();
								break;
							}
						case 306:
							{
								CommandAckResponse.Builder SubBuilder = CommandAckResponse.NewBuilder();
								if (HasAckResponse())
								{
									SubBuilder.mergeFrom(getAckResponse());
								}
								Input.readMessage(SubBuilder, ExtensionRegistry);
								SetAckResponse(SubBuilder.buildPartial());
								SubBuilder.recycle();
								break;
							}
						case 314:
							{
								CommandGetOrCreateSchema.Builder SubBuilder = CommandGetOrCreateSchema.NewBuilder();
								if (HasGetOrCreateSchema())
								{
									SubBuilder.mergeFrom(getGetOrCreateSchema());
								}
								Input.readMessage(SubBuilder, ExtensionRegistry);
								SetGetOrCreateSchema(SubBuilder.buildPartial());
								SubBuilder.recycle();
								break;
							}
						case 322:
							{
								CommandGetOrCreateSchemaResponse.Builder SubBuilder = CommandGetOrCreateSchemaResponse.NewBuilder();
								if (HasGetOrCreateSchemaResponse())
								{
									SubBuilder.mergeFrom(getGetOrCreateSchemaResponse());
								}
								Input.readMessage(SubBuilder, ExtensionRegistry);
								SetGetOrCreateSchemaResponse(SubBuilder.buildPartial());
								SubBuilder.recycle();
								break;
							}
						case 402:
							{
								CommandNewTxn.Builder SubBuilder = CommandNewTxn.NewBuilder();
								if (HasNewTxn())
								{
									SubBuilder.mergeFrom(getNewTxn());
								}
								Input.readMessage(SubBuilder, ExtensionRegistry);
								SetNewTxn(SubBuilder.buildPartial());
								SubBuilder.recycle();
								break;
							}
						case 410:
							{
								CommandNewTxnResponse.Builder SubBuilder = CommandNewTxnResponse.NewBuilder();
								if (HasNewTxnResponse())
								{
									SubBuilder.mergeFrom(getNewTxnResponse());
								}
								Input.readMessage(SubBuilder, ExtensionRegistry);
								SetNewTxnResponse(SubBuilder.buildPartial());
								SubBuilder.recycle();
								break;
							}
						case 418:
							{
								CommandAddPartitionToTxn.Builder SubBuilder = CommandAddPartitionToTxn.NewBuilder();
								if (HasAddPartitionToTxn())
								{
									SubBuilder.mergeFrom(getAddPartitionToTxn());
								}
								Input.readMessage(SubBuilder, ExtensionRegistry);
								SetAddPartitionToTxn(SubBuilder.buildPartial());
								SubBuilder.recycle();
								break;
							}
						case 426:
							{
								CommandAddPartitionToTxnResponse.Builder SubBuilder = CommandAddPartitionToTxnResponse.NewBuilder();
								if (HasAddPartitionToTxnResponse())
								{
									SubBuilder.mergeFrom(getAddPartitionToTxnResponse());
								}
								Input.readMessage(SubBuilder, ExtensionRegistry);
								SetAddPartitionToTxnResponse(SubBuilder.buildPartial());
								SubBuilder.recycle();
								break;
							}
						case 434:
							{
								CommandAddSubscriptionToTxn.Builder SubBuilder = CommandAddSubscriptionToTxn.NewBuilder();
								if (HasAddSubscriptionToTxn())
								{
									SubBuilder.mergeFrom(getAddSubscriptionToTxn());
								}
								Input.readMessage(SubBuilder, ExtensionRegistry);
								SetAddSubscriptionToTxn(SubBuilder.buildPartial());
								SubBuilder.recycle();
								break;
							}
						case 442:
							{
								CommandAddSubscriptionToTxnResponse.Builder SubBuilder = CommandAddSubscriptionToTxnResponse.NewBuilder();
								if (HasAddSubscriptionToTxnResponse())
								{
									SubBuilder.mergeFrom(getAddSubscriptionToTxnResponse());
								}
								Input.readMessage(SubBuilder, ExtensionRegistry);
								SetAddSubscriptionToTxnResponse(SubBuilder.buildPartial());
								SubBuilder.recycle();
								break;
							}
						case 450:
							{
								CommandEndTxn.Builder SubBuilder = CommandEndTxn.NewBuilder();
								if (HasEndTxn())
								{
									SubBuilder.mergeFrom(getEndTxn());
								}
								Input.readMessage(SubBuilder, ExtensionRegistry);
								SetEndTxn(SubBuilder.buildPartial());
								SubBuilder.recycle();
								break;
							}
						case 458:
							{
								CommandEndTxnResponse.Builder SubBuilder = CommandEndTxnResponse.NewBuilder();
								if (HasEndTxnResponse())
								{
									SubBuilder.mergeFrom(getEndTxnResponse());
								}
								Input.readMessage(SubBuilder, ExtensionRegistry);
								SetEndTxnResponse(SubBuilder.buildPartial());
								SubBuilder.recycle();
								break;
							}
						case 466:
							{
								CommandEndTxnOnPartition.Builder SubBuilder = CommandEndTxnOnPartition.NewBuilder();
								if (HasEndTxnOnPartition())
								{
									SubBuilder.mergeFrom(getEndTxnOnPartition());
								}
								Input.readMessage(SubBuilder, ExtensionRegistry);
								SetEndTxnOnPartition(SubBuilder.buildPartial());
								SubBuilder.recycle();
								break;
							}
						case 474:
							{
								CommandEndTxnOnPartitionResponse.Builder SubBuilder = CommandEndTxnOnPartitionResponse.NewBuilder();
								if (HasEndTxnOnPartitionResponse())
								{
									SubBuilder.mergeFrom(getEndTxnOnPartitionResponse());
								}
								Input.readMessage(SubBuilder, ExtensionRegistry);
								SetEndTxnOnPartitionResponse(SubBuilder.buildPartial());
								SubBuilder.recycle();
								break;
							}
						case 482:
							{
								CommandEndTxnOnSubscription.Builder SubBuilder = CommandEndTxnOnSubscription.NewBuilder();
								if (HasEndTxnOnSubscription())
								{
									SubBuilder.mergeFrom(getEndTxnOnSubscription());
								}
								Input.readMessage(SubBuilder, ExtensionRegistry);
								SetEndTxnOnSubscription(SubBuilder.buildPartial());
								SubBuilder.recycle();
								break;
							}
						case 490:
							{
								CommandEndTxnOnSubscriptionResponse.Builder SubBuilder = CommandEndTxnOnSubscriptionResponse.NewBuilder();
								if (HasEndTxnOnSubscriptionResponse())
								{
									SubBuilder.mergeFrom(getEndTxnOnSubscriptionResponse());
								}
								Input.readMessage(SubBuilder, ExtensionRegistry);
								SetEndTxnOnSubscriptionResponse(SubBuilder.buildPartial());
								SubBuilder.recycle();
								break;
							}
					}
				}
			}

			internal int _bitField;
			internal int _bitField1;

			// required .pulsar.proto.BaseCommand.Type type = 1;
			internal BaseCommand.Type _type = BaseCommand.Type.CONNECT;
			public bool HasType()
			{
				return ((_bitField & 0x00000001) == 0x00000001);
			}
			public BaseCommand.Type Type
			{
				get
				{
					return _type;
				}
			}
			public Builder setType(BaseCommand.Type Value)
			{
				if (Value == null)
				{
					throw new System.NullReferenceException();
				}
				_bitField |= 0x00000001;
				_type = Value;

				return this;
			}
			public Builder ClearType()
			{
				_bitField = (_bitField & ~0x00000001);
				_type = BaseCommand.Type.CONNECT;

				return this;
			}

			// optional .pulsar.proto.CommandConnect connect = 2;
			internal CommandConnect _connect = CommandConnect.DefaultInstance;
			public bool HasConnect()
			{
				return ((_bitField & 0x00000002) == 0x00000002);
			}
			public CommandConnect getConnect()
			{
				return _connect;
			}
			public Builder setConnect(CommandConnect Value)
			{
				if (Value == null)
				{
					throw new System.NullReferenceException();
				}
				_connect = Value;

				_bitField |= 0x00000002;
				return this;
			}
			public Builder setConnect(CommandConnect.Builder BuilderForValue)
			{
				_connect = BuilderForValue.build();

				_bitField |= 0x00000002;
				return this;
			}
			public Builder MergeConnect(CommandConnect Value)
			{
				if (((_bitField & 0x00000002) == 0x00000002) && _connect != CommandConnect.DefaultInstance)
				{
					_connect = CommandConnect.NewBuilder(_connect).mergeFrom(Value).buildPartial();
				}
				else
				{
					_connect = Value;
				}

				_bitField |= 0x00000002;
				return this;
			}
			public Builder ClearConnect()
			{
				_connect = CommandConnect.DefaultInstance;

				_bitField = (_bitField & ~0x00000002);
				return this;
			}

			// optional .pulsar.proto.CommandConnected connected = 3;
			internal CommandConnected Connected_ = CommandConnected.DefaultInstance;
			public bool HasConnected()
			{
				return ((_bitField & 0x00000004) == 0x00000004);
			}
			public CommandConnected getConnected()
			{
				return Connected_;
			}
			public Builder setConnected(CommandConnected Value)
			{
				if (Value == null)
				{
					throw new System.NullReferenceException();
				}
				Connected_ = Value;

				_bitField |= 0x00000004;
				return this;
			}
			public Builder setConnected(CommandConnected.Builder BuilderForValue)
			{
				Connected_ = BuilderForValue.build();

				_bitField |= 0x00000004;
				return this;
			}
			public Builder MergeConnected(CommandConnected Value)
			{
				if (((_bitField & 0x00000004) == 0x00000004) && Connected_ != CommandConnected.DefaultInstance)
				{
					Connected_ = CommandConnected.NewBuilder(Connected_).mergeFrom(Value).buildPartial();
				}
				else
				{
					Connected_ = Value;
				}

				_bitField |= 0x00000004;
				return this;
			}
			public Builder ClearConnected()
			{
				Connected_ = CommandConnected.DefaultInstance;

				_bitField = (_bitField & ~0x00000004);
				return this;
			}

			// optional .pulsar.proto.CommandSubscribe subscribe = 4;
			internal CommandSubscribe Subscribe_ = CommandSubscribe.DefaultInstance;
			public bool HasSubscribe()
			{
				return ((_bitField & 0x00000008) == 0x00000008);
			}
			public CommandSubscribe getSubscribe()
			{
				return Subscribe_;
			}
			public Builder setSubscribe(CommandSubscribe Value)
			{
				if (Value == null)
				{
					throw new System.NullReferenceException();
				}
				Subscribe_ = Value;

				_bitField |= 0x00000008;
				return this;
			}
			public Builder setSubscribe(CommandSubscribe.Builder BuilderForValue)
			{
				Subscribe_ = BuilderForValue.build();

				_bitField |= 0x00000008;
				return this;
			}
			public Builder MergeSubscribe(CommandSubscribe Value)
			{
				if (((_bitField & 0x00000008) == 0x00000008) && Subscribe_ != CommandSubscribe.DefaultInstance)
				{
					Subscribe_ = CommandSubscribe.NewBuilder(Subscribe_).mergeFrom(Value).buildPartial();
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
			public CommandProducer getProducer()
			{
				return Producer_;
			}
			public Builder setProducer(CommandProducer Value)
			{
				if (Value == null)
				{
					throw new System.NullReferenceException();
				}
				Producer_ = Value;

				_bitField |= 0x00000010;
				return this;
			}
			public Builder setProducer(CommandProducer.Builder BuilderForValue)
			{
				Producer_ = BuilderForValue.build();

				_bitField |= 0x00000010;
				return this;
			}
			public Builder MergeProducer(CommandProducer Value)
			{
				if (((_bitField & 0x00000010) == 0x00000010) && Producer_ != CommandProducer.DefaultInstance)
				{
					Producer_ = CommandProducer.NewBuilder(Producer_).mergeFrom(Value).buildPartial();
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
			public CommandSend getSend()
			{
				return Send_;
			}
			public Builder setSend(CommandSend Value)
			{
				if (Value == null)
				{
					throw new System.NullReferenceException();
				}
				Send_ = Value;

				_bitField |= 0x00000020;
				return this;
			}
			public Builder setSend(CommandSend.Builder BuilderForValue)
			{
				Send_ = BuilderForValue.build();

				_bitField |= 0x00000020;
				return this;
			}
			public Builder MergeSend(CommandSend Value)
			{
				if (((_bitField & 0x00000020) == 0x00000020) && Send_ != CommandSend.DefaultInstance)
				{
					Send_ = CommandSend.NewBuilder(Send_).mergeFrom(Value).buildPartial();
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
			public CommandSendReceipt getSendReceipt()
			{
				return SendReceipt_;
			}
			public Builder setSendReceipt(CommandSendReceipt Value)
			{
				if (Value == null)
				{
					throw new System.NullReferenceException();
				}
				SendReceipt_ = Value;

				_bitField |= 0x00000040;
				return this;
			}
			public Builder setSendReceipt(CommandSendReceipt.Builder BuilderForValue)
			{
				SendReceipt_ = BuilderForValue.build();

				_bitField |= 0x00000040;
				return this;
			}
			public Builder MergeSendReceipt(CommandSendReceipt Value)
			{
				if (((_bitField & 0x00000040) == 0x00000040) && SendReceipt_ != CommandSendReceipt.DefaultInstance)
				{
					SendReceipt_ = CommandSendReceipt.NewBuilder(SendReceipt_).mergeFrom(Value).buildPartial();
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
			public CommandSendError getSendError()
			{
				return SendError_;
			}
			public Builder setSendError(CommandSendError Value)
			{
				if (Value == null)
				{
					throw new System.NullReferenceException();
				}
				SendError_ = Value;

				_bitField |= 0x00000080;
				return this;
			}
			public Builder setSendError(CommandSendError.Builder BuilderForValue)
			{
				SendError_ = BuilderForValue.build();

				_bitField |= 0x00000080;
				return this;
			}
			public Builder MergeSendError(CommandSendError Value)
			{
				if (((_bitField & 0x00000080) == 0x00000080) && SendError_ != CommandSendError.DefaultInstance)
				{
					SendError_ = CommandSendError.NewBuilder(SendError_).mergeFrom(Value).buildPartial();
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
			public CommandMessage getMessage()
			{
				return Message_;
			}
			public Builder setMessage(CommandMessage Value)
			{
				if (Value == null)
				{
					throw new System.NullReferenceException();
				}
				Message_ = Value;

				_bitField |= 0x00000100;
				return this;
			}
			public Builder setMessage(CommandMessage.Builder BuilderForValue)
			{
				Message_ = BuilderForValue.build();

				_bitField |= 0x00000100;
				return this;
			}
			public Builder MergeMessage(CommandMessage Value)
			{
				if (((_bitField & 0x00000100) == 0x00000100) && Message_ != CommandMessage.DefaultInstance)
				{
					Message_ = CommandMessage.NewBuilder(Message_).mergeFrom(Value).buildPartial();
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
			public CommandAck getAck()
			{
				return Ack_;
			}
			public Builder setAck(CommandAck Value)
			{
				if (Value == null)
				{
					throw new System.NullReferenceException();
				}
				Ack_ = Value;

				_bitField |= 0x00000200;
				return this;
			}
			public Builder setAck(CommandAck.Builder BuilderForValue)
			{
				Ack_ = BuilderForValue.build();

				_bitField |= 0x00000200;
				return this;
			}
			public Builder MergeAck(CommandAck Value)
			{
				if (((_bitField & 0x00000200) == 0x00000200) && Ack_ != CommandAck.DefaultInstance)
				{
					Ack_ = CommandAck.NewBuilder(Ack_).mergeFrom(Value).buildPartial();
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
			public CommandFlow getFlow()
			{
				return Flow_;
			}
			public Builder setFlow(CommandFlow Value)
			{
				if (Value == null)
				{
					throw new System.NullReferenceException();
				}
				Flow_ = Value;

				_bitField |= 0x00000400;
				return this;
			}
			public Builder setFlow(CommandFlow.Builder BuilderForValue)
			{
				Flow_ = BuilderForValue.build();

				_bitField |= 0x00000400;
				return this;
			}
			public Builder MergeFlow(CommandFlow Value)
			{
				if (((_bitField & 0x00000400) == 0x00000400) && Flow_ != CommandFlow.DefaultInstance)
				{
					Flow_ = CommandFlow.NewBuilder(Flow_).mergeFrom(Value).buildPartial();
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
			public CommandUnsubscribe getUnsubscribe()
			{
				return Unsubscribe_;
			}
			public Builder setUnsubscribe(CommandUnsubscribe Value)
			{
				if (Value == null)
				{
					throw new System.NullReferenceException();
				}
				Unsubscribe_ = Value;

				_bitField |= 0x00000800;
				return this;
			}
			public Builder setUnsubscribe(CommandUnsubscribe.Builder BuilderForValue)
			{
				Unsubscribe_ = BuilderForValue.build();

				_bitField |= 0x00000800;
				return this;
			}
			public Builder MergeUnsubscribe(CommandUnsubscribe Value)
			{
				if (((_bitField & 0x00000800) == 0x00000800) && Unsubscribe_ != CommandUnsubscribe.DefaultInstance)
				{
					Unsubscribe_ = CommandUnsubscribe.NewBuilder(Unsubscribe_).mergeFrom(Value).buildPartial();
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
			public CommandSuccess getSuccess()
			{
				return Success_;
			}
			public Builder setSuccess(CommandSuccess Value)
			{
				if (Value == null)
				{
					throw new System.NullReferenceException();
				}
				Success_ = Value;

				_bitField |= 0x00001000;
				return this;
			}
			public Builder setSuccess(CommandSuccess.Builder BuilderForValue)
			{
				Success_ = BuilderForValue.build();

				_bitField |= 0x00001000;
				return this;
			}
			public Builder MergeSuccess(CommandSuccess Value)
			{
				if (((_bitField & 0x00001000) == 0x00001000) && Success_ != CommandSuccess.DefaultInstance)
				{
					Success_ = CommandSuccess.NewBuilder(Success_).mergeFrom(Value).buildPartial();
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
			public CommandError getError()
			{
				return Error_;
			}
			public Builder setError(CommandError Value)
			{
				if (Value == null)
				{
					throw new System.NullReferenceException();
				}
				Error_ = Value;

				_bitField |= 0x00002000;
				return this;
			}
			public Builder setError(CommandError.Builder BuilderForValue)
			{
				Error_ = BuilderForValue.build();

				_bitField |= 0x00002000;
				return this;
			}
			public Builder MergeError(CommandError Value)
			{
				if (((_bitField & 0x00002000) == 0x00002000) && Error_ != CommandError.DefaultInstance)
				{
					Error_ = CommandError.NewBuilder(Error_).mergeFrom(Value).buildPartial();
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
			public CommandCloseProducer getCloseProducer()
			{
				return CloseProducer_;
			}
			public Builder setCloseProducer(CommandCloseProducer Value)
			{
				if (Value == null)
				{
					throw new System.NullReferenceException();
				}
				CloseProducer_ = Value;

				_bitField |= 0x00004000;
				return this;
			}
			public Builder setCloseProducer(CommandCloseProducer.Builder BuilderForValue)
			{
				CloseProducer_ = BuilderForValue.build();

				_bitField |= 0x00004000;
				return this;
			}
			public Builder MergeCloseProducer(CommandCloseProducer Value)
			{
				if (((_bitField & 0x00004000) == 0x00004000) && CloseProducer_ != CommandCloseProducer.DefaultInstance)
				{
					CloseProducer_ = CommandCloseProducer.NewBuilder(CloseProducer_).mergeFrom(Value).buildPartial();
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
			public CommandCloseConsumer getCloseConsumer()
			{
				return CloseConsumer_;
			}
			public Builder setCloseConsumer(CommandCloseConsumer Value)
			{
				if (Value == null)
				{
					throw new System.NullReferenceException();
				}
				CloseConsumer_ = Value;

				_bitField |= 0x00008000;
				return this;
			}
			public Builder setCloseConsumer(CommandCloseConsumer.Builder BuilderForValue)
			{
				CloseConsumer_ = BuilderForValue.build();

				_bitField |= 0x00008000;
				return this;
			}
			public Builder MergeCloseConsumer(CommandCloseConsumer Value)
			{
				if (((_bitField & 0x00008000) == 0x00008000) && CloseConsumer_ != CommandCloseConsumer.DefaultInstance)
				{
					CloseConsumer_ = CommandCloseConsumer.NewBuilder(CloseConsumer_).mergeFrom(Value).buildPartial();
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
			public CommandProducerSuccess getProducerSuccess()
			{
				return ProducerSuccess_;
			}
			public Builder setProducerSuccess(CommandProducerSuccess Value)
			{
				if (Value == null)
				{
					throw new System.NullReferenceException();
				}
				ProducerSuccess_ = Value;

				_bitField |= 0x00010000;
				return this;
			}
			public Builder setProducerSuccess(CommandProducerSuccess.Builder BuilderForValue)
			{
				ProducerSuccess_ = BuilderForValue.build();

				_bitField |= 0x00010000;
				return this;
			}
			public Builder MergeProducerSuccess(CommandProducerSuccess Value)
			{
				if (((_bitField & 0x00010000) == 0x00010000) && ProducerSuccess_ != CommandProducerSuccess.DefaultInstance)
				{
					ProducerSuccess_ = CommandProducerSuccess.NewBuilder(ProducerSuccess_).mergeFrom(Value).buildPartial();
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
			public CommandPing getPing()
			{
				return Ping_;
			}
			public Builder setPing(CommandPing Value)
			{
				if (Value == null)
				{
					throw new System.NullReferenceException();
				}
				Ping_ = Value;

				_bitField |= 0x00020000;
				return this;
			}
			public Builder setPing(CommandPing.Builder BuilderForValue)
			{
				Ping_ = BuilderForValue.build();

				_bitField |= 0x00020000;
				return this;
			}
			public Builder MergePing(CommandPing Value)
			{
				if (((_bitField & 0x00020000) == 0x00020000) && Ping_ != CommandPing.DefaultInstance)
				{
					Ping_ = CommandPing.NewBuilder(Ping_).mergeFrom(Value).buildPartial();
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
			public CommandPong getPong()
			{
				return Pong_;
			}
			public Builder setPong(CommandPong Value)
			{
				if (Value == null)
				{
					throw new System.NullReferenceException();
				}
				Pong_ = Value;

				_bitField |= 0x00040000;
				return this;
			}
			public Builder setPong(CommandPong.Builder BuilderForValue)
			{
				Pong_ = BuilderForValue.build();

				_bitField |= 0x00040000;
				return this;
			}
			public Builder MergePong(CommandPong Value)
			{
				if (((_bitField & 0x00040000) == 0x00040000) && Pong_ != CommandPong.DefaultInstance)
				{
					Pong_ = CommandPong.NewBuilder(Pong_).mergeFrom(Value).buildPartial();
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
			public CommandRedeliverUnacknowledgedMessages getRedeliverUnacknowledgedMessages()
			{
				return RedeliverUnacknowledgedMessages_;
			}
			public Builder setRedeliverUnacknowledgedMessages(CommandRedeliverUnacknowledgedMessages Value)
			{
				if (Value == null)
				{
					throw new System.NullReferenceException();
				}
				RedeliverUnacknowledgedMessages_ = Value;

				_bitField |= 0x00080000;
				return this;
			}
			public Builder setRedeliverUnacknowledgedMessages(CommandRedeliverUnacknowledgedMessages.Builder BuilderForValue)
			{
				RedeliverUnacknowledgedMessages_ = BuilderForValue.build();

				_bitField |= 0x00080000;
				return this;
			}
			public Builder MergeRedeliverUnacknowledgedMessages(CommandRedeliverUnacknowledgedMessages Value)
			{
				if (((_bitField & 0x00080000) == 0x00080000) && RedeliverUnacknowledgedMessages_ != CommandRedeliverUnacknowledgedMessages.DefaultInstance)
				{
					RedeliverUnacknowledgedMessages_ = CommandRedeliverUnacknowledgedMessages.NewBuilder(RedeliverUnacknowledgedMessages_).mergeFrom(Value).buildPartial();
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
			public CommandPartitionedTopicMetadata getPartitionMetadata()
			{
				return PartitionMetadata_;
			}
			public Builder setPartitionMetadata(CommandPartitionedTopicMetadata Value)
			{
				if (Value == null)
				{
					throw new System.NullReferenceException();
				}
				PartitionMetadata_ = Value;

				_bitField |= 0x00100000;
				return this;
			}
			public Builder setPartitionMetadata(CommandPartitionedTopicMetadata.Builder BuilderForValue)
			{
				PartitionMetadata_ = BuilderForValue.build();

				_bitField |= 0x00100000;
				return this;
			}
			public Builder MergePartitionMetadata(CommandPartitionedTopicMetadata Value)
			{
				if (((_bitField & 0x00100000) == 0x00100000) && PartitionMetadata_ != CommandPartitionedTopicMetadata.DefaultInstance)
				{
					PartitionMetadata_ = CommandPartitionedTopicMetadata.NewBuilder(PartitionMetadata_).mergeFrom(Value).buildPartial();
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
			public CommandPartitionedTopicMetadataResponse getPartitionMetadataResponse()
			{
				return PartitionMetadataResponse_;
			}
			public Builder setPartitionMetadataResponse(CommandPartitionedTopicMetadataResponse Value)
			{
				if (Value == null)
				{
					throw new System.NullReferenceException();
				}
				PartitionMetadataResponse_ = Value;

				_bitField |= 0x00200000;
				return this;
			}
			public Builder setPartitionMetadataResponse(CommandPartitionedTopicMetadataResponse.Builder BuilderForValue)
			{
				PartitionMetadataResponse_ = BuilderForValue.build();

				_bitField |= 0x00200000;
				return this;
			}
			public Builder MergePartitionMetadataResponse(CommandPartitionedTopicMetadataResponse Value)
			{
				if (((_bitField & 0x00200000) == 0x00200000) && PartitionMetadataResponse_ != CommandPartitionedTopicMetadataResponse.DefaultInstance)
				{
					PartitionMetadataResponse_ = CommandPartitionedTopicMetadataResponse.NewBuilder(PartitionMetadataResponse_).mergeFrom(Value).buildPartial();
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
			public CommandLookupTopic getLookupTopic()
			{
				return LookupTopic_;
			}
			public Builder setLookupTopic(CommandLookupTopic Value)
			{
				if (Value == null)
				{
					throw new System.NullReferenceException();
				}
				LookupTopic_ = Value;

				_bitField |= 0x00400000;
				return this;
			}
			public Builder setLookupTopic(CommandLookupTopic.Builder BuilderForValue)
			{
				LookupTopic_ = BuilderForValue.build();

				_bitField |= 0x00400000;
				return this;
			}
			public Builder MergeLookupTopic(CommandLookupTopic Value)
			{
				if (((_bitField & 0x00400000) == 0x00400000) && LookupTopic_ != CommandLookupTopic.DefaultInstance)
				{
					LookupTopic_ = CommandLookupTopic.NewBuilder(LookupTopic_).mergeFrom(Value).buildPartial();
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
			public CommandLookupTopicResponse getLookupTopicResponse()
			{
				return LookupTopicResponse_;
			}
			public Builder setLookupTopicResponse(CommandLookupTopicResponse Value)
			{
				if (Value == null)
				{
					throw new System.NullReferenceException();
				}
				LookupTopicResponse_ = Value;

				_bitField |= 0x00800000;
				return this;
			}
			public Builder setLookupTopicResponse(CommandLookupTopicResponse.Builder BuilderForValue)
			{
				LookupTopicResponse_ = BuilderForValue.build();

				_bitField |= 0x00800000;
				return this;
			}
			public Builder MergeLookupTopicResponse(CommandLookupTopicResponse Value)
			{
				if (((_bitField & 0x00800000) == 0x00800000) && LookupTopicResponse_ != CommandLookupTopicResponse.DefaultInstance)
				{
					LookupTopicResponse_ = CommandLookupTopicResponse.NewBuilder(LookupTopicResponse_).mergeFrom(Value).buildPartial();
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
			public CommandConsumerStats getConsumerStats()
			{
				return ConsumerStats_;
			}
			public Builder setConsumerStats(CommandConsumerStats Value)
			{
				if (Value == null)
				{
					throw new System.NullReferenceException();
				}
				ConsumerStats_ = Value;

				_bitField |= 0x01000000;
				return this;
			}
			public Builder setConsumerStats(CommandConsumerStats.Builder BuilderForValue)
			{
				ConsumerStats_ = BuilderForValue.build();

				_bitField |= 0x01000000;
				return this;
			}
			public Builder MergeConsumerStats(CommandConsumerStats Value)
			{
				if (((_bitField & 0x01000000) == 0x01000000) && ConsumerStats_ != CommandConsumerStats.DefaultInstance)
				{
					ConsumerStats_ = CommandConsumerStats.NewBuilder(ConsumerStats_).mergeFrom(Value).buildPartial();
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
			public CommandConsumerStatsResponse getConsumerStatsResponse()
			{
				return ConsumerStatsResponse_;
			}
			public Builder setConsumerStatsResponse(CommandConsumerStatsResponse Value)
			{
				if (Value == null)
				{
					throw new System.NullReferenceException();
				}
				ConsumerStatsResponse_ = Value;

				_bitField |= 0x02000000;
				return this;
			}
			public Builder setConsumerStatsResponse(CommandConsumerStatsResponse.Builder BuilderForValue)
			{
				ConsumerStatsResponse_ = BuilderForValue.build();

				_bitField |= 0x02000000;
				return this;
			}
			public Builder MergeConsumerStatsResponse(CommandConsumerStatsResponse Value)
			{
				if (((_bitField & 0x02000000) == 0x02000000) && ConsumerStatsResponse_ != CommandConsumerStatsResponse.DefaultInstance)
				{
					ConsumerStatsResponse_ = CommandConsumerStatsResponse.NewBuilder(ConsumerStatsResponse_).mergeFrom(Value).buildPartial();
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
			public CommandReachedEndOfTopic getReachedEndOfTopic()
			{
				return ReachedEndOfTopic_;
			}
			public Builder setReachedEndOfTopic(CommandReachedEndOfTopic Value)
			{
				if (Value == null)
				{
					throw new System.NullReferenceException();
				}
				ReachedEndOfTopic_ = Value;

				_bitField |= 0x04000000;
				return this;
			}
			public Builder setReachedEndOfTopic(CommandReachedEndOfTopic.Builder BuilderForValue)
			{
				ReachedEndOfTopic_ = BuilderForValue.build();

				_bitField |= 0x04000000;
				return this;
			}
			public Builder MergeReachedEndOfTopic(CommandReachedEndOfTopic Value)
			{
				if (((_bitField & 0x04000000) == 0x04000000) && ReachedEndOfTopic_ != CommandReachedEndOfTopic.DefaultInstance)
				{
					ReachedEndOfTopic_ = CommandReachedEndOfTopic.NewBuilder(ReachedEndOfTopic_).mergeFrom(Value).buildPartial();
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
			public CommandSeek getSeek()
			{
				return Seek_;
			}
			public Builder setSeek(CommandSeek Value)
			{
				if (Value == null)
				{
					throw new System.NullReferenceException();
				}
				Seek_ = Value;

				_bitField |= 0x08000000;
				return this;
			}
			public Builder setSeek(CommandSeek.Builder BuilderForValue)
			{
				Seek_ = BuilderForValue.build();

				_bitField |= 0x08000000;
				return this;
			}
			public Builder MergeSeek(CommandSeek Value)
			{
				if (((_bitField & 0x08000000) == 0x08000000) && Seek_ != CommandSeek.DefaultInstance)
				{
					Seek_ = CommandSeek.NewBuilder(Seek_).mergeFrom(Value).buildPartial();
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

			// optional .pulsar.proto.CommandGetLastMessageId getLastMessageId = 29;
			internal CommandGetLastMessageId GetLastMessageId_ = CommandGetLastMessageId.DefaultInstance;
			public bool HasGetLastMessageId()
			{
				return ((_bitField & 0x10000000) == 0x10000000);
			}
			public CommandGetLastMessageId getGetLastMessageId()
			{
				return GetLastMessageId_;
			}
			public Builder setGetLastMessageId(CommandGetLastMessageId Value)
			{
				if (Value == null)
				{
					throw new System.NullReferenceException();
				}
				GetLastMessageId_ = Value;

				_bitField |= 0x10000000;
				return this;
			}
			public Builder setGetLastMessageId(CommandGetLastMessageId.Builder BuilderForValue)
			{
				GetLastMessageId_ = BuilderForValue.build();

				_bitField |= 0x10000000;
				return this;
			}
			public Builder MergeGetLastMessageId(CommandGetLastMessageId Value)
			{
				if (((_bitField & 0x10000000) == 0x10000000) && GetLastMessageId_ != CommandGetLastMessageId.DefaultInstance)
				{
					GetLastMessageId_ = CommandGetLastMessageId.NewBuilder(GetLastMessageId_).mergeFrom(Value).buildPartial();
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

			// optional .pulsar.proto.CommandGetLastMessageIdResponse getLastMessageIdResponse = 30;
			internal CommandGetLastMessageIdResponse GetLastMessageIdResponse_ = CommandGetLastMessageIdResponse.DefaultInstance;
			public bool HasGetLastMessageIdResponse()
			{
				return ((_bitField & 0x20000000) == 0x20000000);
			}
			public CommandGetLastMessageIdResponse getGetLastMessageIdResponse()
			{
				return GetLastMessageIdResponse_;
			}
			public Builder setGetLastMessageIdResponse(CommandGetLastMessageIdResponse Value)
			{
				if (Value == null)
				{
					throw new System.NullReferenceException();
				}
				GetLastMessageIdResponse_ = Value;

				_bitField |= 0x20000000;
				return this;
			}
			public Builder setGetLastMessageIdResponse(CommandGetLastMessageIdResponse.Builder BuilderForValue)
			{
				GetLastMessageIdResponse_ = BuilderForValue.build();

				_bitField |= 0x20000000;
				return this;
			}
			public Builder MergeGetLastMessageIdResponse(CommandGetLastMessageIdResponse Value)
			{
				if (((_bitField & 0x20000000) == 0x20000000) && GetLastMessageIdResponse_ != CommandGetLastMessageIdResponse.DefaultInstance)
				{
					GetLastMessageIdResponse_ = CommandGetLastMessageIdResponse.NewBuilder(GetLastMessageIdResponse_).mergeFrom(Value).buildPartial();
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
			public CommandActiveConsumerChange getActiveConsumerChange()
			{
				return ActiveConsumerChange_;
			}
			public Builder setActiveConsumerChange(CommandActiveConsumerChange Value)
			{
				if (Value == null)
				{
					throw new System.NullReferenceException();
				}
				ActiveConsumerChange_ = Value;

				_bitField |= 0x40000000;
				return this;
			}
			public Builder setActiveConsumerChange(CommandActiveConsumerChange.Builder BuilderForValue)
			{
				ActiveConsumerChange_ = BuilderForValue.build();

				_bitField |= 0x40000000;
				return this;
			}
			public Builder MergeActiveConsumerChange(CommandActiveConsumerChange Value)
			{
				if (((_bitField & 0x40000000) == 0x40000000) && ActiveConsumerChange_ != CommandActiveConsumerChange.DefaultInstance)
				{
					ActiveConsumerChange_ = CommandActiveConsumerChange.NewBuilder(ActiveConsumerChange_).mergeFrom(Value).buildPartial();
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

			// optional .pulsar.proto.CommandGetTopicsOfNamespace getTopicsOfNamespace = 32;
			internal CommandGetTopicsOfNamespace GetTopicsOfNamespace_ = CommandGetTopicsOfNamespace.DefaultInstance;
			public bool HasGetTopicsOfNamespace()
			{
				return ((_bitField & 0x80000000) == 0x80000000);
			}
			public CommandGetTopicsOfNamespace getGetTopicsOfNamespace()
			{
				return GetTopicsOfNamespace_;
			}
			public Builder setGetTopicsOfNamespace(CommandGetTopicsOfNamespace Value)
			{
				if (Value == null)
				{
					throw new System.NullReferenceException();
				}
				GetTopicsOfNamespace_ = Value;

				_bitField |= unchecked((int)0x80000000);
				return this;
			}
			public Builder setGetTopicsOfNamespace(CommandGetTopicsOfNamespace.Builder BuilderForValue)
			{
				GetTopicsOfNamespace_ = BuilderForValue.build();

				_bitField |= unchecked((int)0x80000000);
				return this;
			}
			public Builder MergeGetTopicsOfNamespace(CommandGetTopicsOfNamespace Value)
			{
				if (((_bitField & 0x80000000) == 0x80000000) && GetTopicsOfNamespace_ != CommandGetTopicsOfNamespace.DefaultInstance)
				{
					GetTopicsOfNamespace_ = CommandGetTopicsOfNamespace.NewBuilder(GetTopicsOfNamespace_).mergeFrom(Value).buildPartial();
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

			// optional .pulsar.proto.CommandGetTopicsOfNamespaceResponse getTopicsOfNamespaceResponse = 33;
			internal CommandGetTopicsOfNamespaceResponse GetTopicsOfNamespaceResponse_ = CommandGetTopicsOfNamespaceResponse.DefaultInstance;
			public bool HasGetTopicsOfNamespaceResponse()
			{
				return ((_bitField1 & 0x00000001) == 0x00000001);
			}
			public CommandGetTopicsOfNamespaceResponse getGetTopicsOfNamespaceResponse()
			{
				return GetTopicsOfNamespaceResponse_;
			}
			public Builder setGetTopicsOfNamespaceResponse(CommandGetTopicsOfNamespaceResponse Value)
			{
				if (Value == null)
				{
					throw new System.NullReferenceException();
				}
				GetTopicsOfNamespaceResponse_ = Value;

				_bitField1 |= 0x00000001;
				return this;
			}
			public Builder setGetTopicsOfNamespaceResponse(CommandGetTopicsOfNamespaceResponse.Builder BuilderForValue)
			{
				GetTopicsOfNamespaceResponse_ = BuilderForValue.build();

				_bitField1 |= 0x00000001;
				return this;
			}
			public Builder MergeGetTopicsOfNamespaceResponse(CommandGetTopicsOfNamespaceResponse Value)
			{
				if (((_bitField1 & 0x00000001) == 0x00000001) && GetTopicsOfNamespaceResponse_ != CommandGetTopicsOfNamespaceResponse.DefaultInstance)
				{
					GetTopicsOfNamespaceResponse_ = CommandGetTopicsOfNamespaceResponse.NewBuilder(GetTopicsOfNamespaceResponse_).mergeFrom(Value).buildPartial();
				}
				else
				{
					GetTopicsOfNamespaceResponse_ = Value;
				}

				_bitField1 |= 0x00000001;
				return this;
			}
			public Builder ClearGetTopicsOfNamespaceResponse()
			{
				GetTopicsOfNamespaceResponse_ = CommandGetTopicsOfNamespaceResponse.DefaultInstance;

				_bitField1 = (_bitField1 & ~0x00000001);
				return this;
			}

			// optional .pulsar.proto.CommandGetSchema getSchema = 34;
			internal CommandGetSchema GetSchema_ = CommandGetSchema.DefaultInstance;
			public bool HasGetSchema()
			{
				return ((_bitField1 & 0x00000002) == 0x00000002);
			}
			public CommandGetSchema getGetSchema()
			{
				return GetSchema_;
			}
			public Builder setGetSchema(CommandGetSchema Value)
			{
				if (Value == null)
				{
					throw new System.NullReferenceException();
				}
				GetSchema_ = Value;

				_bitField1 |= 0x00000002;
				return this;
			}
			public Builder setGetSchema(CommandGetSchema.Builder BuilderForValue)
			{
				GetSchema_ = BuilderForValue.build();

				_bitField1 |= 0x00000002;
				return this;
			}
			public Builder MergeGetSchema(CommandGetSchema Value)
			{
				if (((_bitField1 & 0x00000002) == 0x00000002) && GetSchema_ != CommandGetSchema.DefaultInstance)
				{
					GetSchema_ = CommandGetSchema.NewBuilder(GetSchema_).mergeFrom(Value).buildPartial();
				}
				else
				{
					GetSchema_ = Value;
				}

				_bitField1 |= 0x00000002;
				return this;
			}
			public Builder ClearGetSchema()
			{
				GetSchema_ = CommandGetSchema.DefaultInstance;

				_bitField1 = (_bitField1 & ~0x00000002);
				return this;
			}

			// optional .pulsar.proto.CommandGetSchemaResponse getSchemaResponse = 35;
			internal CommandGetSchemaResponse GetSchemaResponse_ = CommandGetSchemaResponse.DefaultInstance;
			public bool HasGetSchemaResponse()
			{
				return ((_bitField1 & 0x00000004) == 0x00000004);
			}
			public CommandGetSchemaResponse getGetSchemaResponse()
			{
				return GetSchemaResponse_;
			}
			public Builder setGetSchemaResponse(CommandGetSchemaResponse Value)
			{
				if (Value == null)
				{
					throw new System.NullReferenceException();
				}
				GetSchemaResponse_ = Value;

				_bitField1 |= 0x00000004;
				return this;
			}
			public Builder setGetSchemaResponse(CommandGetSchemaResponse.Builder BuilderForValue)
			{
				GetSchemaResponse_ = BuilderForValue.build();

				_bitField1 |= 0x00000004;
				return this;
			}
			public Builder MergeGetSchemaResponse(CommandGetSchemaResponse Value)
			{
				if (((_bitField1 & 0x00000004) == 0x00000004) && GetSchemaResponse_ != CommandGetSchemaResponse.DefaultInstance)
				{
					GetSchemaResponse_ = CommandGetSchemaResponse.NewBuilder(GetSchemaResponse_).mergeFrom(Value).buildPartial();
				}
				else
				{
					GetSchemaResponse_ = Value;
				}

				_bitField1 |= 0x00000004;
				return this;
			}
			public Builder ClearGetSchemaResponse()
			{
				GetSchemaResponse_ = CommandGetSchemaResponse.DefaultInstance;

				_bitField1 = (_bitField1 & ~0x00000004);
				return this;
			}

			// optional .pulsar.proto.CommandAuthChallenge authChallenge = 36;
			internal CommandAuthChallenge AuthChallenge_ = CommandAuthChallenge.DefaultInstance;
			public bool HasAuthChallenge()
			{
				return ((_bitField1 & 0x00000008) == 0x00000008);
			}
			public CommandAuthChallenge getAuthChallenge()
			{
				return AuthChallenge_;
			}
			public Builder setAuthChallenge(CommandAuthChallenge Value)
			{
				if (Value == null)
				{
					throw new System.NullReferenceException();
				}
				AuthChallenge_ = Value;

				_bitField1 |= 0x00000008;
				return this;
			}
			public Builder setAuthChallenge(CommandAuthChallenge.Builder BuilderForValue)
			{
				AuthChallenge_ = BuilderForValue.build();

				_bitField1 |= 0x00000008;
				return this;
			}
			public Builder MergeAuthChallenge(CommandAuthChallenge Value)
			{
				if (((_bitField1 & 0x00000008) == 0x00000008) && AuthChallenge_ != CommandAuthChallenge.DefaultInstance)
				{
					AuthChallenge_ = CommandAuthChallenge.NewBuilder(AuthChallenge_).mergeFrom(Value).buildPartial();
				}
				else
				{
					AuthChallenge_ = Value;
				}

				_bitField1 |= 0x00000008;
				return this;
			}
			public Builder ClearAuthChallenge()
			{
				AuthChallenge_ = CommandAuthChallenge.DefaultInstance;

				_bitField1 = (_bitField1 & ~0x00000008);
				return this;
			}

			// optional .pulsar.proto.CommandAuthResponse authResponse = 37;
			internal CommandAuthResponse AuthResponse_ = CommandAuthResponse.DefaultInstance;
			public bool HasAuthResponse()
			{
				return ((_bitField1 & 0x00000010) == 0x00000010);
			}
			public CommandAuthResponse getAuthResponse()
			{
				return AuthResponse_;
			}
			public Builder setAuthResponse(CommandAuthResponse Value)
			{
				if (Value == null)
				{
					throw new System.NullReferenceException();
				}
				AuthResponse_ = Value;

				_bitField1 |= 0x00000010;
				return this;
			}
			public Builder setAuthResponse(CommandAuthResponse.Builder BuilderForValue)
			{
				AuthResponse_ = BuilderForValue.build();

				_bitField1 |= 0x00000010;
				return this;
			}
			public Builder MergeAuthResponse(CommandAuthResponse Value)
			{
				if (((_bitField1 & 0x00000010) == 0x00000010) && AuthResponse_ != CommandAuthResponse.DefaultInstance)
				{
					AuthResponse_ = CommandAuthResponse.NewBuilder(AuthResponse_).mergeFrom(Value).buildPartial();
				}
				else
				{
					AuthResponse_ = Value;
				}

				_bitField1 |= 0x00000010;
				return this;
			}
			public Builder ClearAuthResponse()
			{
				AuthResponse_ = CommandAuthResponse.DefaultInstance;

				_bitField1 = (_bitField1 & ~0x00000010);
				return this;
			}

			// optional .pulsar.proto.CommandAckResponse ackResponse = 38;
			internal CommandAckResponse AckResponse_ = CommandAckResponse.DefaultInstance;
			public bool HasAckResponse()
			{
				return ((_bitField1 & 0x00000020) == 0x00000020);
			}
			public CommandAckResponse getAckResponse()
			{
				return AckResponse_;
			}
			public Builder setAckResponse(CommandAckResponse Value)
			{
				if (Value == null)
				{
					throw new System.NullReferenceException();
				}
				AckResponse_ = Value;

				_bitField1 |= 0x00000020;
				return this;
			}
			public Builder setAckResponse(CommandAckResponse.Builder BuilderForValue)
			{
				AckResponse_ = BuilderForValue.build();

				_bitField1 |= 0x00000020;
				return this;
			}
			public Builder MergeAckResponse(CommandAckResponse Value)
			{
				if (((_bitField1 & 0x00000020) == 0x00000020) && AckResponse_ != CommandAckResponse.DefaultInstance)
				{
					AckResponse_ = CommandAckResponse.NewBuilder(AckResponse_).mergeFrom(Value).buildPartial();
				}
				else
				{
					AckResponse_ = Value;
				}

				_bitField1 |= 0x00000020;
				return this;
			}
			public Builder ClearAckResponse()
			{
				AckResponse_ = CommandAckResponse.DefaultInstance;

				_bitField1 = (_bitField1 & ~0x00000020);
				return this;
			}

			// optional .pulsar.proto.CommandGetOrCreateSchema getOrCreateSchema = 39;
			internal CommandGetOrCreateSchema GetOrCreateSchema_ = CommandGetOrCreateSchema.DefaultInstance;
			public bool HasGetOrCreateSchema()
			{
				return ((_bitField1 & 0x00000040) == 0x00000040);
			}
			public CommandGetOrCreateSchema getGetOrCreateSchema()
			{
				return GetOrCreateSchema_;
			}
			public Builder setGetOrCreateSchema(CommandGetOrCreateSchema Value)
			{
				if (Value == null)
				{
					throw new System.NullReferenceException();
				}
				GetOrCreateSchema_ = Value;

				_bitField1 |= 0x00000040;
				return this;
			}
			public Builder setGetOrCreateSchema(CommandGetOrCreateSchema.Builder BuilderForValue)
			{
				GetOrCreateSchema_ = BuilderForValue.build();

				_bitField1 |= 0x00000040;
				return this;
			}
			public Builder MergeGetOrCreateSchema(CommandGetOrCreateSchema Value)
			{
				if (((_bitField1 & 0x00000040) == 0x00000040) && GetOrCreateSchema_ != CommandGetOrCreateSchema.DefaultInstance)
				{
					GetOrCreateSchema_ = CommandGetOrCreateSchema.NewBuilder(GetOrCreateSchema_).mergeFrom(Value).buildPartial();
				}
				else
				{
					GetOrCreateSchema_ = Value;
				}

				_bitField1 |= 0x00000040;
				return this;
			}
			public Builder ClearGetOrCreateSchema()
			{
				GetOrCreateSchema_ = CommandGetOrCreateSchema.DefaultInstance;

				_bitField1 = (_bitField1 & ~0x00000040);
				return this;
			}

			// optional .pulsar.proto.CommandGetOrCreateSchemaResponse getOrCreateSchemaResponse = 40;
			internal CommandGetOrCreateSchemaResponse GetOrCreateSchemaResponse_ = CommandGetOrCreateSchemaResponse.DefaultInstance;
			public bool HasGetOrCreateSchemaResponse()
			{
				return ((_bitField1 & 0x00000080) == 0x00000080);
			}
			public CommandGetOrCreateSchemaResponse getGetOrCreateSchemaResponse()
			{
				return GetOrCreateSchemaResponse_;
			}
			public Builder setGetOrCreateSchemaResponse(CommandGetOrCreateSchemaResponse Value)
			{
				if (Value == null)
				{
					throw new System.NullReferenceException();
				}
				GetOrCreateSchemaResponse_ = Value;

				_bitField1 |= 0x00000080;
				return this;
			}
			public Builder setGetOrCreateSchemaResponse(CommandGetOrCreateSchemaResponse.Builder BuilderForValue)
			{
				GetOrCreateSchemaResponse_ = BuilderForValue.build();

				_bitField1 |= 0x00000080;
				return this;
			}
			public Builder MergeGetOrCreateSchemaResponse(CommandGetOrCreateSchemaResponse Value)
			{
				if (((_bitField1 & 0x00000080) == 0x00000080) && GetOrCreateSchemaResponse_ != CommandGetOrCreateSchemaResponse.DefaultInstance)
				{
					GetOrCreateSchemaResponse_ = CommandGetOrCreateSchemaResponse.NewBuilder(GetOrCreateSchemaResponse_).mergeFrom(Value).buildPartial();
				}
				else
				{
					GetOrCreateSchemaResponse_ = Value;
				}

				_bitField1 |= 0x00000080;
				return this;
			}
			public Builder ClearGetOrCreateSchemaResponse()
			{
				GetOrCreateSchemaResponse_ = CommandGetOrCreateSchemaResponse.DefaultInstance;

				_bitField1 = (_bitField1 & ~0x00000080);
				return this;
			}

			// optional .pulsar.proto.CommandNewTxn newTxn = 50;
			internal CommandNewTxn NewTxn_ = CommandNewTxn.DefaultInstance;
			public bool HasNewTxn()
			{
				return ((_bitField1 & 0x00000100) == 0x00000100);
			}
			public CommandNewTxn getNewTxn()
			{
				return NewTxn_;
			}
			public Builder setNewTxn(CommandNewTxn Value)
			{
				if (Value == null)
				{
					throw new System.NullReferenceException();
				}
				NewTxn_ = Value;

				_bitField1 |= 0x00000100;
				return this;
			}
			public Builder setNewTxn(CommandNewTxn.Builder BuilderForValue)
			{
				NewTxn_ = BuilderForValue.build();

				_bitField1 |= 0x00000100;
				return this;
			}
			public Builder MergeNewTxn(CommandNewTxn Value)
			{
				if (((_bitField1 & 0x00000100) == 0x00000100) && NewTxn_ != CommandNewTxn.DefaultInstance)
				{
					NewTxn_ = CommandNewTxn.NewBuilder(NewTxn_).mergeFrom(Value).buildPartial();
				}
				else
				{
					NewTxn_ = Value;
				}

				_bitField1 |= 0x00000100;
				return this;
			}
			public Builder ClearNewTxn()
			{
				NewTxn_ = CommandNewTxn.DefaultInstance;

				_bitField1 = (_bitField1 & ~0x00000100);
				return this;
			}

			// optional .pulsar.proto.CommandNewTxnResponse newTxnResponse = 51;
			internal CommandNewTxnResponse NewTxnResponse_ = CommandNewTxnResponse.DefaultInstance;
			public bool HasNewTxnResponse()
			{
				return ((_bitField1 & 0x00000200) == 0x00000200);
			}
			public CommandNewTxnResponse getNewTxnResponse()
			{
				return NewTxnResponse_;
			}
			public Builder setNewTxnResponse(CommandNewTxnResponse Value)
			{
				if (Value == null)
				{
					throw new System.NullReferenceException();
				}
				NewTxnResponse_ = Value;

				_bitField1 |= 0x00000200;
				return this;
			}
			public Builder setNewTxnResponse(CommandNewTxnResponse.Builder BuilderForValue)
			{
				NewTxnResponse_ = BuilderForValue.build();

				_bitField1 |= 0x00000200;
				return this;
			}
			public Builder MergeNewTxnResponse(CommandNewTxnResponse Value)
			{
				if (((_bitField1 & 0x00000200) == 0x00000200) && NewTxnResponse_ != CommandNewTxnResponse.DefaultInstance)
				{
					NewTxnResponse_ = CommandNewTxnResponse.NewBuilder(NewTxnResponse_).mergeFrom(Value).buildPartial();
				}
				else
				{
					NewTxnResponse_ = Value;
				}

				_bitField1 |= 0x00000200;
				return this;
			}
			public Builder ClearNewTxnResponse()
			{
				NewTxnResponse_ = CommandNewTxnResponse.DefaultInstance;

				_bitField1 = (_bitField1 & ~0x00000200);
				return this;
			}

			// optional .pulsar.proto.CommandAddPartitionToTxn addPartitionToTxn = 52;
			internal CommandAddPartitionToTxn AddPartitionToTxn_ = CommandAddPartitionToTxn.DefaultInstance;
			public bool HasAddPartitionToTxn()
			{
				return ((_bitField1 & 0x00000400) == 0x00000400);
			}
			public CommandAddPartitionToTxn getAddPartitionToTxn()
			{
				return AddPartitionToTxn_;
			}
			public Builder setAddPartitionToTxn(CommandAddPartitionToTxn Value)
			{
				if (Value == null)
				{
					throw new System.NullReferenceException();
				}
				AddPartitionToTxn_ = Value;

				_bitField1 |= 0x00000400;
				return this;
			}
			public Builder setAddPartitionToTxn(CommandAddPartitionToTxn.Builder BuilderForValue)
			{
				AddPartitionToTxn_ = BuilderForValue.build();

				_bitField1 |= 0x00000400;
				return this;
			}
			public Builder MergeAddPartitionToTxn(CommandAddPartitionToTxn Value)
			{
				if (((_bitField1 & 0x00000400) == 0x00000400) && AddPartitionToTxn_ != CommandAddPartitionToTxn.DefaultInstance)
				{
					AddPartitionToTxn_ = CommandAddPartitionToTxn.NewBuilder(AddPartitionToTxn_).mergeFrom(Value).buildPartial();
				}
				else
				{
					AddPartitionToTxn_ = Value;
				}

				_bitField1 |= 0x00000400;
				return this;
			}
			public Builder ClearAddPartitionToTxn()
			{
				AddPartitionToTxn_ = CommandAddPartitionToTxn.DefaultInstance;

				_bitField1 = (_bitField1 & ~0x00000400);
				return this;
			}

			// optional .pulsar.proto.CommandAddPartitionToTxnResponse addPartitionToTxnResponse = 53;
			internal CommandAddPartitionToTxnResponse AddPartitionToTxnResponse_ = CommandAddPartitionToTxnResponse.DefaultInstance;
			public bool HasAddPartitionToTxnResponse()
			{
				return ((_bitField1 & 0x00000800) == 0x00000800);
			}
			public CommandAddPartitionToTxnResponse getAddPartitionToTxnResponse()
			{
				return AddPartitionToTxnResponse_;
			}
			public Builder setAddPartitionToTxnResponse(CommandAddPartitionToTxnResponse Value)
			{
				if (Value == null)
				{
					throw new System.NullReferenceException();
				}
				AddPartitionToTxnResponse_ = Value;

				_bitField1 |= 0x00000800;
				return this;
			}
			public Builder setAddPartitionToTxnResponse(CommandAddPartitionToTxnResponse.Builder BuilderForValue)
			{
				AddPartitionToTxnResponse_ = BuilderForValue.build();

				_bitField1 |= 0x00000800;
				return this;
			}
			public Builder MergeAddPartitionToTxnResponse(CommandAddPartitionToTxnResponse Value)
			{
				if (((_bitField1 & 0x00000800) == 0x00000800) && AddPartitionToTxnResponse_ != CommandAddPartitionToTxnResponse.DefaultInstance)
				{
					AddPartitionToTxnResponse_ = CommandAddPartitionToTxnResponse.NewBuilder(AddPartitionToTxnResponse_).mergeFrom(Value).buildPartial();
				}
				else
				{
					AddPartitionToTxnResponse_ = Value;
				}

				_bitField1 |= 0x00000800;
				return this;
			}
			public Builder ClearAddPartitionToTxnResponse()
			{
				AddPartitionToTxnResponse_ = CommandAddPartitionToTxnResponse.DefaultInstance;

				_bitField1 = (_bitField1 & ~0x00000800);
				return this;
			}

			// optional .pulsar.proto.CommandAddSubscriptionToTxn addSubscriptionToTxn = 54;
			internal CommandAddSubscriptionToTxn AddSubscriptionToTxn_ = CommandAddSubscriptionToTxn.DefaultInstance;
			public bool HasAddSubscriptionToTxn()
			{
				return ((_bitField1 & 0x00001000) == 0x00001000);
			}
			public CommandAddSubscriptionToTxn getAddSubscriptionToTxn()
			{
				return AddSubscriptionToTxn_;
			}
			public Builder setAddSubscriptionToTxn(CommandAddSubscriptionToTxn Value)
			{
				if (Value == null)
				{
					throw new System.NullReferenceException();
				}
				AddSubscriptionToTxn_ = Value;

				_bitField1 |= 0x00001000;
				return this;
			}
			public Builder setAddSubscriptionToTxn(CommandAddSubscriptionToTxn.Builder BuilderForValue)
			{
				AddSubscriptionToTxn_ = BuilderForValue.build();

				_bitField1 |= 0x00001000;
				return this;
			}
			public Builder MergeAddSubscriptionToTxn(CommandAddSubscriptionToTxn Value)
			{
				if (((_bitField1 & 0x00001000) == 0x00001000) && AddSubscriptionToTxn_ != CommandAddSubscriptionToTxn.DefaultInstance)
				{
					AddSubscriptionToTxn_ = CommandAddSubscriptionToTxn.NewBuilder(AddSubscriptionToTxn_).mergeFrom(Value).buildPartial();
				}
				else
				{
					AddSubscriptionToTxn_ = Value;
				}

				_bitField1 |= 0x00001000;
				return this;
			}
			public Builder ClearAddSubscriptionToTxn()
			{
				AddSubscriptionToTxn_ = CommandAddSubscriptionToTxn.DefaultInstance;

				_bitField1 = (_bitField1 & ~0x00001000);
				return this;
			}

			// optional .pulsar.proto.CommandAddSubscriptionToTxnResponse addSubscriptionToTxnResponse = 55;
			internal CommandAddSubscriptionToTxnResponse AddSubscriptionToTxnResponse_ = CommandAddSubscriptionToTxnResponse.DefaultInstance;
			public bool HasAddSubscriptionToTxnResponse()
			{
				return ((_bitField1 & 0x00002000) == 0x00002000);
			}
			public CommandAddSubscriptionToTxnResponse getAddSubscriptionToTxnResponse()
			{
				return AddSubscriptionToTxnResponse_;
			}
			public Builder setAddSubscriptionToTxnResponse(CommandAddSubscriptionToTxnResponse Value)
			{
				if (Value == null)
				{
					throw new System.NullReferenceException();
				}
				AddSubscriptionToTxnResponse_ = Value;

				_bitField1 |= 0x00002000;
				return this;
			}
			public Builder setAddSubscriptionToTxnResponse(CommandAddSubscriptionToTxnResponse.Builder BuilderForValue)
			{
				AddSubscriptionToTxnResponse_ = BuilderForValue.build();

				_bitField1 |= 0x00002000;
				return this;
			}
			public Builder MergeAddSubscriptionToTxnResponse(CommandAddSubscriptionToTxnResponse Value)
			{
				if (((_bitField1 & 0x00002000) == 0x00002000) && AddSubscriptionToTxnResponse_ != CommandAddSubscriptionToTxnResponse.DefaultInstance)
				{
					AddSubscriptionToTxnResponse_ = CommandAddSubscriptionToTxnResponse.NewBuilder(AddSubscriptionToTxnResponse_).mergeFrom(Value).buildPartial();
				}
				else
				{
					AddSubscriptionToTxnResponse_ = Value;
				}

				_bitField1 |= 0x00002000;
				return this;
			}
			public Builder ClearAddSubscriptionToTxnResponse()
			{
				AddSubscriptionToTxnResponse_ = CommandAddSubscriptionToTxnResponse.DefaultInstance;

				_bitField1 = (_bitField1 & ~0x00002000);
				return this;
			}

			// optional .pulsar.proto.CommandEndTxn endTxn = 56;
			internal CommandEndTxn EndTxn_ = CommandEndTxn.DefaultInstance;
			public bool HasEndTxn()
			{
				return ((_bitField1 & 0x00004000) == 0x00004000);
			}
			public CommandEndTxn getEndTxn()
			{
				return EndTxn_;
			}
			public Builder setEndTxn(CommandEndTxn Value)
			{
				if (Value == null)
				{
					throw new System.NullReferenceException();
				}
				EndTxn_ = Value;

				_bitField1 |= 0x00004000;
				return this;
			}
			public Builder setEndTxn(CommandEndTxn.Builder BuilderForValue)
			{
				EndTxn_ = BuilderForValue.build();

				_bitField1 |= 0x00004000;
				return this;
			}
			public Builder MergeEndTxn(CommandEndTxn Value)
			{
				if (((_bitField1 & 0x00004000) == 0x00004000) && EndTxn_ != CommandEndTxn.DefaultInstance)
				{
					EndTxn_ = CommandEndTxn.NewBuilder(EndTxn_).mergeFrom(Value).buildPartial();
				}
				else
				{
					EndTxn_ = Value;
				}

				_bitField1 |= 0x00004000;
				return this;
			}
			public Builder ClearEndTxn()
			{
				EndTxn_ = CommandEndTxn.DefaultInstance;

				_bitField1 = (_bitField1 & ~0x00004000);
				return this;
			}

			// optional .pulsar.proto.CommandEndTxnResponse endTxnResponse = 57;
			internal CommandEndTxnResponse EndTxnResponse_ = CommandEndTxnResponse.DefaultInstance;
			public bool HasEndTxnResponse()
			{
				return ((_bitField1 & 0x00008000) == 0x00008000);
			}
			public CommandEndTxnResponse getEndTxnResponse()
			{
				return EndTxnResponse_;
			}
			public Builder setEndTxnResponse(CommandEndTxnResponse Value)
			{
				if (Value == null)
				{
					throw new System.NullReferenceException();
				}
				EndTxnResponse_ = Value;

				_bitField1 |= 0x00008000;
				return this;
			}
			public Builder setEndTxnResponse(CommandEndTxnResponse.Builder BuilderForValue)
			{
				EndTxnResponse_ = BuilderForValue.build();

				_bitField1 |= 0x00008000;
				return this;
			}
			public Builder MergeEndTxnResponse(CommandEndTxnResponse Value)
			{
				if (((_bitField1 & 0x00008000) == 0x00008000) && EndTxnResponse_ != CommandEndTxnResponse.DefaultInstance)
				{
					EndTxnResponse_ = CommandEndTxnResponse.NewBuilder(EndTxnResponse_).mergeFrom(Value).buildPartial();
				}
				else
				{
					EndTxnResponse_ = Value;
				}

				_bitField1 |= 0x00008000;
				return this;
			}
			public Builder ClearEndTxnResponse()
			{
				EndTxnResponse_ = CommandEndTxnResponse.DefaultInstance;

				_bitField1 = (_bitField1 & ~0x00008000);
				return this;
			}

			// optional .pulsar.proto.CommandEndTxnOnPartition endTxnOnPartition = 58;
			internal CommandEndTxnOnPartition EndTxnOnPartition_ = CommandEndTxnOnPartition.DefaultInstance;
			public bool HasEndTxnOnPartition()
			{
				return ((_bitField1 & 0x00010000) == 0x00010000);
			}
			public CommandEndTxnOnPartition getEndTxnOnPartition()
			{
				return EndTxnOnPartition_;
			}
			public Builder setEndTxnOnPartition(CommandEndTxnOnPartition Value)
			{
				if (Value == null)
				{
					throw new System.NullReferenceException();
				}
				EndTxnOnPartition_ = Value;

				_bitField1 |= 0x00010000;
				return this;
			}
			public Builder setEndTxnOnPartition(CommandEndTxnOnPartition.Builder BuilderForValue)
			{
				EndTxnOnPartition_ = BuilderForValue.build();

				_bitField1 |= 0x00010000;
				return this;
			}
			public Builder MergeEndTxnOnPartition(CommandEndTxnOnPartition Value)
			{
				if (((_bitField1 & 0x00010000) == 0x00010000) && EndTxnOnPartition_ != CommandEndTxnOnPartition.DefaultInstance)
				{
					EndTxnOnPartition_ = CommandEndTxnOnPartition.NewBuilder(EndTxnOnPartition_).mergeFrom(Value).buildPartial();
				}
				else
				{
					EndTxnOnPartition_ = Value;
				}

				_bitField1 |= 0x00010000;
				return this;
			}
			public Builder ClearEndTxnOnPartition()
			{
				EndTxnOnPartition_ = CommandEndTxnOnPartition.DefaultInstance;

				_bitField1 = (_bitField1 & ~0x00010000);
				return this;
			}

			// optional .pulsar.proto.CommandEndTxnOnPartitionResponse endTxnOnPartitionResponse = 59;
			internal CommandEndTxnOnPartitionResponse EndTxnOnPartitionResponse_ = CommandEndTxnOnPartitionResponse.DefaultInstance;
			public bool HasEndTxnOnPartitionResponse()
			{
				return ((_bitField1 & 0x00020000) == 0x00020000);
			}
			public CommandEndTxnOnPartitionResponse getEndTxnOnPartitionResponse()
			{
				return EndTxnOnPartitionResponse_;
			}
			public Builder setEndTxnOnPartitionResponse(CommandEndTxnOnPartitionResponse Value)
			{
				if (Value == null)
				{
					throw new System.NullReferenceException();
				}
				EndTxnOnPartitionResponse_ = Value;

				_bitField1 |= 0x00020000;
				return this;
			}
			public Builder setEndTxnOnPartitionResponse(CommandEndTxnOnPartitionResponse.Builder BuilderForValue)
			{
				EndTxnOnPartitionResponse_ = BuilderForValue.build();

				_bitField1 |= 0x00020000;
				return this;
			}
			public Builder MergeEndTxnOnPartitionResponse(CommandEndTxnOnPartitionResponse Value)
			{
				if (((_bitField1 & 0x00020000) == 0x00020000) && EndTxnOnPartitionResponse_ != CommandEndTxnOnPartitionResponse.DefaultInstance)
				{
					EndTxnOnPartitionResponse_ = CommandEndTxnOnPartitionResponse.NewBuilder(EndTxnOnPartitionResponse_).mergeFrom(Value).buildPartial();
				}
				else
				{
					EndTxnOnPartitionResponse_ = Value;
				}

				_bitField1 |= 0x00020000;
				return this;
			}
			public Builder ClearEndTxnOnPartitionResponse()
			{
				EndTxnOnPartitionResponse_ = CommandEndTxnOnPartitionResponse.DefaultInstance;

				_bitField1 = (_bitField1 & ~0x00020000);
				return this;
			}

			// optional .pulsar.proto.CommandEndTxnOnSubscription endTxnOnSubscription = 60;
			internal CommandEndTxnOnSubscription EndTxnOnSubscription_ = CommandEndTxnOnSubscription.DefaultInstance;
			public bool HasEndTxnOnSubscription()
			{
				return ((_bitField1 & 0x00040000) == 0x00040000);
			}
			public CommandEndTxnOnSubscription getEndTxnOnSubscription()
			{
				return EndTxnOnSubscription_;
			}
			public Builder setEndTxnOnSubscription(CommandEndTxnOnSubscription Value)
			{
				if (Value == null)
				{
					throw new System.NullReferenceException();
				}
				EndTxnOnSubscription_ = Value;

				_bitField1 |= 0x00040000;
				return this;
			}
			public Builder setEndTxnOnSubscription(CommandEndTxnOnSubscription.Builder BuilderForValue)
			{
				EndTxnOnSubscription_ = BuilderForValue.build();

				_bitField1 |= 0x00040000;
				return this;
			}
			public Builder MergeEndTxnOnSubscription(CommandEndTxnOnSubscription Value)
			{
				if (((_bitField1 & 0x00040000) == 0x00040000) && EndTxnOnSubscription_ != CommandEndTxnOnSubscription.DefaultInstance)
				{
					EndTxnOnSubscription_ = CommandEndTxnOnSubscription.NewBuilder(EndTxnOnSubscription_).mergeFrom(Value).buildPartial();
				}
				else
				{
					EndTxnOnSubscription_ = Value;
				}

				_bitField1 |= 0x00040000;
				return this;
			}
			public Builder ClearEndTxnOnSubscription()
			{
				EndTxnOnSubscription_ = CommandEndTxnOnSubscription.DefaultInstance;

				_bitField1 = (_bitField1 & ~0x00040000);
				return this;
			}

			// optional .pulsar.proto.CommandEndTxnOnSubscriptionResponse endTxnOnSubscriptionResponse = 61;
			internal CommandEndTxnOnSubscriptionResponse _endTxnOnSubscriptionResponse = CommandEndTxnOnSubscriptionResponse.DefaultInstance;
			public bool HasEndTxnOnSubscriptionResponse()
			{
				return ((_bitField1 & 0x00080000) == 0x00080000);
			}
			public CommandEndTxnOnSubscriptionResponse getEndTxnOnSubscriptionResponse()
			{
				return _endTxnOnSubscriptionResponse;
			}
			public Builder setEndTxnOnSubscriptionResponse(CommandEndTxnOnSubscriptionResponse Value)
			{
				if (Value == null)
				{
					throw new System.NullReferenceException();
				}
				_endTxnOnSubscriptionResponse = Value;

				_bitField1 |= 0x00080000;
				return this;
			}
			public Builder setEndTxnOnSubscriptionResponse(CommandEndTxnOnSubscriptionResponse.Builder BuilderForValue)
			{
				_endTxnOnSubscriptionResponse = BuilderForValue.build();

				_bitField1 |= 0x00080000;
				return this;
			}
			public Builder MergeEndTxnOnSubscriptionResponse(CommandEndTxnOnSubscriptionResponse Value)
			{
				if (((_bitField1 & 0x00080000) == 0x00080000) && _endTxnOnSubscriptionResponse != CommandEndTxnOnSubscriptionResponse.DefaultInstance)
				{
					_endTxnOnSubscriptionResponse = CommandEndTxnOnSubscriptionResponse.NewBuilder(_endTxnOnSubscriptionResponse).mergeFrom(Value).buildPartial();
				}
				else
				{
					_endTxnOnSubscriptionResponse = Value;
				}

				_bitField1 |= 0x00080000;
				return this;
			}
			public Builder ClearEndTxnOnSubscriptionResponse()
			{
				_endTxnOnSubscriptionResponse = CommandEndTxnOnSubscriptionResponse.DefaultInstance;

				_bitField1 = (_bitField1 & ~0x00080000);
				return this;
			}

			// @@protoc_insertion_point(builder_scope:pulsar.proto.BaseCommand)
		}

		static BaseCommand()
		{
			_defaultInstance = new BaseCommand(true);
			_defaultInstance.initFields();
		}

		// @@protoc_insertion_point(class_scope:pulsar.proto.BaseCommand)
	}

}
