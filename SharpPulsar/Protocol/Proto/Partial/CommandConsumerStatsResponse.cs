using DotNetty.Common;
using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpPulsar.Utility.Protobuf;
using static SharpPulsar.Utility.Protobuf.ByteBufCodedInputStream;

namespace SharpPulsar.Protocol.Proto
{
	public  sealed partial class CommandConsumerStatsResponse : ByteBufCodedOutputStream.ByteBufGeneratedMessage
	{
		// Use CommandConsumerStatsResponse.newBuilder() to construct.
		internal static ThreadLocalPool<CommandConsumerStatsResponse> _pool = new ThreadLocalPool<CommandConsumerStatsResponse>(handle => new CommandConsumerStatsResponse(handle), 1, true);

		internal ThreadLocalPool.Handle _handle;
		private CommandConsumerStatsResponse(ThreadLocalPool.Handle handle)
		{
			_handle = handle;
		}
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

		public CommandConsumerStatsResponse(bool NoInit)
		{
		}

		
		internal static readonly CommandConsumerStatsResponse _defaultInstance;
		public static CommandConsumerStatsResponse DefaultInstance => _defaultInstance;

        public CommandConsumerStatsResponse DefaultInstanceForType => _defaultInstance;


        public void InitFields()
		{
			RequestId = 0L;
			ErrorCode = ServerError.UnknownError;
			ErrorMessage = "";
			MsgRateOut = 0D;
			MsgThroughputOut = 0D;
			MsgRateRedeliver = 0D;
			ConsumerName = "";
			AvailablePermits = 0L;
			UnackedMessages = 0L;
			BlockedConsumerOnUnackedMsgs = false;
			Address = "";
			ConnectedSince = "";
			Type = "";
			MsgRateExpired = 0D;
			MsgBacklog = 0L;
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

				if (!HasRequestId)
				{
					MemoizedIsInitialized = 0;
					return false;
				}
				MemoizedIsInitialized = 1;
				return true;
			}
		}
		public void WriteTo(ByteBufCodedOutputStream Output)
		{
			var _ = SerializedSize;
			if (((_hasBits0 & 0x00000001) == 0x00000001))
			{
				Output.WriteUInt64(1, (long)RequestId);
			}
			if (((_hasBits0 & 0x00000002) == 0x00000002))
			{
				Output.WriteEnum(2, (int)ErrorCode);
			}
			if (((_hasBits0 & 0x00000004) == 0x00000004))
			{
				Output.WriteBytes(3, ByteString.CopyFromUtf8(ErrorMessage));
			}
			if (((_hasBits0 & 0x00000008) == 0x00000008))
			{
				Output.WriteDouble(4, MsgRateOut);
			}
			if (((_hasBits0 & 0x00000010) == 0x00000010))
			{
				Output.WriteDouble(5, MsgThroughputOut);
			}
			if (((_hasBits0 & 0x00000020) == 0x00000020))
			{
				Output.WriteDouble(6, MsgRateRedeliver);
			}
			if (((_hasBits0 & 0x00000040) == 0x00000040))
			{
				Output.WriteBytes(7, ByteString.CopyFromUtf8(ConsumerName));
			}
			if (((_hasBits0 & 0x00000080) == 0x00000080))
			{
				Output.WriteUInt64(8, (long)AvailablePermits);
			}
			if (((_hasBits0 & 0x00000100) == 0x00000100))
			{
				Output.WriteUInt64(9, (long)UnackedMessages);
			}
			if (((_hasBits0 & 0x00000200) == 0x00000200))
			{
				Output.WriteBool(10, BlockedConsumerOnUnackedMsgs);
			}
			if (((_hasBits0 & 0x00000400) == 0x00000400))
			{
				Output.WriteBytes(11, ByteString.CopyFromUtf8(Address));
			}
			if (((_hasBits0 & 0x00000800) == 0x00000800))
			{
				Output.WriteBytes(12, ByteString.CopyFromUtf8(ConnectedSince));
			}
			if (((_hasBits0 & 0x00001000) == 0x00001000))
			{
				Output.WriteBytes(13, ByteString.CopyFromUtf8(Type));
			}
			if (((_hasBits0 & 0x00002000) == 0x00002000))
			{
				Output.WriteDouble(14, MsgRateExpired);
			}
			if (((_hasBits0 & 0x00004000) == 0x00004000))
			{
				Output.WriteUInt64(15, (long)MsgBacklog);
			}
		}

		internal int MemoizedSerializedSize = -1;
		public int SerializedSize => CalculateSize();

		internal const long SerialVersionUID = 0L;
		
		public static Builder NewBuilder()
		{
			return Builder.Create();
		}
		public Builder NewBuilderForType()
		{
			return NewBuilder();
		}
		public static Builder NewBuilder(CommandConsumerStatsResponse Prototype)
		{
			return NewBuilder().MergeFrom(Prototype);
		}
		public Builder ToBuilder()
		{
			return NewBuilder(this);
		}

		public sealed class Builder :  ByteBufMessageBuilder
		{
			// Construct using org.apache.pulsar.common.api.proto.CommandConsumerStatsResponse.newBuilder()
			internal static ThreadLocalPool<Builder> _pool = new ThreadLocalPool<Builder>(handle => new Builder(handle), 1, true);

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

				RequestId_ = 0L;
				BitField0_ = (BitField0_ & ~0x00000001);
				ErrorCode_ = ServerError.UnknownError;
				BitField0_ = (BitField0_ & ~0x00000002);
				ErrorMessage_ = "";
				BitField0_ = (BitField0_ & ~0x00000004);
				MsgRateOut_ = 0D;
				BitField0_ = (BitField0_ & ~0x00000008);
				MsgThroughputOut_ = 0D;
				BitField0_ = (BitField0_ & ~0x00000010);
				MsgRateRedeliver_ = 0D;
				BitField0_ = (BitField0_ & ~0x00000020);
				ConsumerName_ = "";
				BitField0_ = (BitField0_ & ~0x00000040);
				AvailablePermits_ = 0L;
				BitField0_ = (BitField0_ & ~0x00000080);
				UnackedMessages_ = 0L;
				BitField0_ = (BitField0_ & ~0x00000100);
				BlockedConsumerOnUnackedMsgs_ = false;
				BitField0_ = (BitField0_ & ~0x00000200);
				Address_ = "";
				BitField0_ = (BitField0_ & ~0x00000400);
				ConnectedSince_ = "";
				BitField0_ = (BitField0_ & ~0x00000800);
				Type_ = "";
				BitField0_ = (BitField0_ & ~0x00001000);
				MsgRateExpired_ = 0D;
				BitField0_ = (BitField0_ & ~0x00002000);
				MsgBacklog_ = 0L;
				BitField0_ = (BitField0_ & ~0x00004000);
				return this;
			}

			public Builder Clone()
			{
				return Create().MergeFrom(BuildPartial());
			}

			public CommandConsumerStatsResponse DefaultInstanceForType => CommandConsumerStatsResponse.DefaultInstance;

            public CommandConsumerStatsResponse Build()
			{
				CommandConsumerStatsResponse Result = BuildPartial();
				if (!Result.Initialized)
				{
					throw new NullReferenceException($"{Result.GetType().Name} not initialized");
				}
				return Result;
			}

			
			public CommandConsumerStatsResponse BuildParsed()
			{
				CommandConsumerStatsResponse Result = BuildPartial();
				if (!Result.Initialized)
				{
					throw new NullReferenceException($"{Result.GetType().Name} not initialized");
				}
				return Result;
			}

			public CommandConsumerStatsResponse BuildPartial()
			{
				CommandConsumerStatsResponse Result = CommandConsumerStatsResponse._pool.Take();
				int FromBitField0_ = BitField0_;
				int ToBitField0_ = 0;
				if (((FromBitField0_ & 0x00000001) == 0x00000001))
				{
					ToBitField0_ |= 0x00000001;
				}
				Result.RequestId = (ulong)RequestId_;
				if (((FromBitField0_ & 0x00000002) == 0x00000002))
				{
					ToBitField0_ |= 0x00000002;
				}
				Result.ErrorCode = ErrorCode_;
				if (((FromBitField0_ & 0x00000004) == 0x00000004))
				{
					ToBitField0_ |= 0x00000004;
				}
				Result.ErrorMessage = ErrorMessage_.ToString();
				if (((FromBitField0_ & 0x00000008) == 0x00000008))
				{
					ToBitField0_ |= 0x00000008;
				}
				Result.MsgRateOut = MsgRateOut_;
				if (((FromBitField0_ & 0x00000010) == 0x00000010))
				{
					ToBitField0_ |= 0x00000010;
				}
				Result.MsgThroughputOut = MsgThroughputOut_;
				if (((FromBitField0_ & 0x00000020) == 0x00000020))
				{
					ToBitField0_ |= 0x00000020;
				}
				Result.MsgRateRedeliver = MsgRateRedeliver_;
				if (((FromBitField0_ & 0x00000040) == 0x00000040))
				{
					ToBitField0_ |= 0x00000040;
				}
				Result.ConsumerName = ConsumerName_.ToString();
				if (((FromBitField0_ & 0x00000080) == 0x00000080))
				{
					ToBitField0_ |= 0x00000080;
				}
				Result.AvailablePermits = (ulong)AvailablePermits_;
				if (((FromBitField0_ & 0x00000100) == 0x00000100))
				{
					ToBitField0_ |= 0x00000100;
				}
				Result.UnackedMessages = (ulong)UnackedMessages_;
				if (((FromBitField0_ & 0x00000200) == 0x00000200))
				{
					ToBitField0_ |= 0x00000200;
				}
				Result.BlockedConsumerOnUnackedMsgs = BlockedConsumerOnUnackedMsgs_;
				if (((FromBitField0_ & 0x00000400) == 0x00000400))
				{
					ToBitField0_ |= 0x00000400;
				}
				Result.Address = Address_.ToString();
				if (((FromBitField0_ & 0x00000800) == 0x00000800))
				{
					ToBitField0_ |= 0x00000800;
				}
				Result.ConnectedSince = ConnectedSince_.ToString();
				if (((FromBitField0_ & 0x00001000) == 0x00001000))
				{
					ToBitField0_ |= 0x00001000;
				}
				Result.Type = Type_.ToString();
				if (((FromBitField0_ & 0x00002000) == 0x00002000))
				{
					ToBitField0_ |= 0x00002000;
				}
				Result.MsgRateExpired = MsgRateExpired_;
				if (((FromBitField0_ & 0x00004000) == 0x00004000))
				{
					ToBitField0_ |= 0x00004000;
				}
				Result.MsgBacklog = (ulong)MsgBacklog_;
				Result._hasBits0 = ToBitField0_;
				return Result;
			}

			public Builder MergeFrom(CommandConsumerStatsResponse Other)
			{
				if (Other == DefaultInstance)
				{
					return this;
				}
				if (Other.HasRequestId)
				{
					SetRequestId((long)Other.RequestId);
				}
				if (Other.HasErrorCode)
				{
					SetErrorCode(Other.ErrorCode);
				}
				if (Other.HasErrorMessage)
				{
					SetErrorMessage(Other.ErrorMessage);
				}
				if (Other.HasMsgRateOut)
				{
					SetMsgRateOut(Other.MsgRateOut);
				}
				if (Other.HasMsgThroughputOut)
				{
					SetMsgThroughputOut(Other.MsgThroughputOut);
				}
				if (Other.HasMsgRateRedeliver)
				{
					SetMsgRateRedeliver(Other.MsgRateRedeliver);
				}
				if (Other.HasConsumerName)
				{
					SetConsumerName(Other.ConsumerName);
				}
				if (Other.HasAvailablePermits)
				{
					SetAvailablePermits((long)Other.AvailablePermits);
				}
				if (Other.HasUnackedMessages)
				{
					SetUnackedMessages((long)Other.UnackedMessages);
				}
				if (Other.HasBlockedConsumerOnUnackedMsgs)
				{
					SetBlockedConsumerOnUnackedMsgs(Other.BlockedConsumerOnUnackedMsgs);
				}
				if (Other.HasAddress)
				{
					SetAddress(Other.Address);
				}
				if (Other.HasConnectedSince)
				{
					SetConnectedSince(Other.ConnectedSince);
				}
				if (Other.HasType)
				{
					SetType(Other.Type);
				}
				if (Other.HasMsgRateExpired)
				{
					SetMsgRateExpired(Other.MsgRateExpired);
				}
				if (Other.HasMsgBacklog)
				{
					SetMsgBacklog((long)Other.MsgBacklog);
				}
				return this;
			}

			public bool Initialized
			{
				get
				{
					if (!HasRequestId())
					{

						return false;
					}
					return true;
				}
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
								BitField0_ |= 0x00000001;
								RequestId_ = input.ReadUInt64();
								break;
							}
						case 16:
							{
								int RawValue = input.ReadEnum();
								ServerError Value = Enum.GetValues(typeof(ServerError)).Cast<ServerError>().ToList()[RawValue];
								if (Value != null)
								{
									BitField0_ |= 0x00000002;
									ErrorCode_ = Value;
								}
								break;
							}
						case 26:
							{
								BitField0_ |= 0x00000004;
								ErrorMessage_ = input.ReadBytes();
								break;
							}
						case 33:
							{
								BitField0_ |= 0x00000008;
								MsgRateOut_ = input.ReadDouble();
								break;
							}
						case 41:
							{
								BitField0_ |= 0x00000010;
								MsgThroughputOut_ = input.ReadDouble();
								break;
							}
						case 49:
							{
								BitField0_ |= 0x00000020;
								MsgRateRedeliver_ = input.ReadDouble();
								break;
							}
						case 58:
							{
								BitField0_ |= 0x00000040;
								ConsumerName_ = input.ReadBytes();
								break;
							}
						case 64:
							{
								BitField0_ |= 0x00000080;
								AvailablePermits_ = input.ReadUInt64();
								break;
							}
						case 72:
							{
								BitField0_ |= 0x00000100;
								UnackedMessages_ = input.ReadUInt64();
								break;
							}
						case 80:
							{
								BitField0_ |= 0x00000200;
								BlockedConsumerOnUnackedMsgs_ = input.ReadBool();
								break;
							}
						case 90:
							{
								BitField0_ |= 0x00000400;
								Address_ = input.ReadBytes();
								break;
							}
						case 98:
							{
								BitField0_ |= 0x00000800;
								ConnectedSince_ = input.ReadBytes();
								break;
							}
						case 106:
							{
								BitField0_ |= 0x00001000;
								Type_ = input.ReadBytes();
								break;
							}
						case 113:
							{
								BitField0_ |= 0x00002000;
								MsgRateExpired_ = input.ReadDouble();
								break;
							}
						case 120:
							{
								BitField0_ |= 0x00004000;
								MsgBacklog_ = input.ReadUInt64();
								break;
							}
					}
				}
			}

			internal int BitField0_;

			// required uint64 request_id = 1;
			internal long RequestId_;
			public bool HasRequestId()
			{
				return ((BitField0_ & 0x00000001) == 0x00000001);
			}
			public long RequestId => RequestId_;

            public Builder SetRequestId(long Value)
			{
				BitField0_ |= 0x00000001;
				RequestId_ = Value;

				return this;
			}
			public Builder ClearRequestId()
			{
				BitField0_ = (BitField0_ & ~0x00000001);
				RequestId_ = 0L;

				return this;
			}

			// optional .pulsar.proto.ServerError error_code = 2;
			internal ServerError ErrorCode_ = ServerError.UnknownError;
			public bool HasErrorCode()
			{
				return ((BitField0_ & 0x00000002) == 0x00000002);
			}
			public ServerError ErrorCode => ErrorCode_;

            public Builder SetErrorCode(ServerError Value)
			{
				if (Value == null)
				{
					throw new NullReferenceException();
				}
				BitField0_ |= 0x00000002;
				ErrorCode_ = Value;

				return this;
			}
			public Builder ClearErrorCode()
			{
				BitField0_ = (BitField0_ & ~0x00000002);
				ErrorCode_ = ServerError.UnknownError;

				return this;
			}

			// optional string error_message = 3;
			internal object ErrorMessage_ = "";
			public bool HasErrorMessage()
			{
				return ((BitField0_ & 0x00000004) == 0x00000004);
			}
			public string GetErrorMessage()
			{
				object Ref = ErrorMessage_;
				if (!(Ref is string))
				{
					string S = ((ByteString)Ref).ToStringUtf8();
					ErrorMessage_ = S;
					return S;
				}
				else
				{
					return (string)Ref;
				}
			}
			public Builder SetErrorMessage(string Value)
			{
				if (string.ReferenceEquals(Value, null))
				{
					throw new NullReferenceException();
				}
				BitField0_ |= 0x00000004;
				ErrorMessage_ = Value;

				return this;
			}
			public Builder ClearErrorMessage()
			{
				BitField0_ = (BitField0_ & ~0x00000004);
				ErrorMessage_ = DefaultInstance.ErrorMessage;

				return this;
			}
			public void SetErrorMessage(ByteString Value)
			{
				BitField0_ |= 0x00000004;
				ErrorMessage_ = Value;

			}

			// optional double msgRateOut = 4;
			internal double MsgRateOut_;
			public bool HasMsgRateOut()
			{
				return ((BitField0_ & 0x00000008) == 0x00000008);
			}
			public double MsgRateOut => MsgRateOut_;

            public Builder SetMsgRateOut(double Value)
			{
				BitField0_ |= 0x00000008;
				MsgRateOut_ = Value;

				return this;
			}
			public Builder ClearMsgRateOut()
			{
				BitField0_ = (BitField0_ & ~0x00000008);
				MsgRateOut_ = 0D;

				return this;
			}

			// optional double msgThroughputOut = 5;
			internal double MsgThroughputOut_;
			public bool HasMsgThroughputOut()
			{
				return ((BitField0_ & 0x00000010) == 0x00000010);
			}
			public double MsgThroughputOut => MsgThroughputOut_;

            public Builder SetMsgThroughputOut(double Value)
			{
				BitField0_ |= 0x00000010;
				MsgThroughputOut_ = Value;

				return this;
			}
			public Builder ClearMsgThroughputOut()
			{
				BitField0_ = (BitField0_ & ~0x00000010);
				MsgThroughputOut_ = 0D;

				return this;
			}

			// optional double msgRateRedeliver = 6;
			internal double MsgRateRedeliver_;
			public bool HasMsgRateRedeliver()
			{
				return ((BitField0_ & 0x00000020) == 0x00000020);
			}
			public double MsgRateRedeliver => MsgRateRedeliver_;

            public Builder SetMsgRateRedeliver(double Value)
			{
				BitField0_ |= 0x00000020;
				MsgRateRedeliver_ = Value;

				return this;
			}
			public Builder ClearMsgRateRedeliver()
			{
				BitField0_ = (BitField0_ & ~0x00000020);
				MsgRateRedeliver_ = 0D;

				return this;
			}

			// optional string consumerName = 7;
			internal object ConsumerName_ = "";
			public bool HasConsumerName()
			{
				return ((BitField0_ & 0x00000040) == 0x00000040);
			}
			public string GetConsumerName()
			{
				object Ref = ConsumerName_;
				if (!(Ref is string))
				{
					string S = ((ByteString)Ref).ToStringUtf8();
					ConsumerName_ = S;
					return S;
				}
				else
				{
					return (string)Ref;
				}
			}
			public Builder SetConsumerName(string Value)
			{
				if (string.ReferenceEquals(Value, null))
				{
					throw new NullReferenceException();
				}
				BitField0_ |= 0x00000040;
				ConsumerName_ = Value;

				return this;
			}
			public Builder ClearConsumerName()
			{
				BitField0_ = (BitField0_ & ~0x00000040);
				ConsumerName_ = DefaultInstance.ConsumerName;

				return this;
			}
			public void SetConsumerName(ByteString Value)
			{
				BitField0_ |= 0x00000040;
				ConsumerName_ = Value;

			}

			// optional uint64 availablePermits = 8;
			internal long AvailablePermits_;
			public bool HasAvailablePermits()
			{
				return ((BitField0_ & 0x00000080) == 0x00000080);
			}
			public long AvailablePermits => AvailablePermits_;

            public Builder SetAvailablePermits(long Value)
			{
				BitField0_ |= 0x00000080;
				AvailablePermits_ = Value;

				return this;
			}
			public Builder ClearAvailablePermits()
			{
				BitField0_ = (BitField0_ & ~0x00000080);
				AvailablePermits_ = 0L;

				return this;
			}

			// optional uint64 unackedMessages = 9;
			internal long UnackedMessages_;
			public bool HasUnackedMessages()
			{
				return ((BitField0_ & 0x00000100) == 0x00000100);
			}
			public long UnackedMessages => UnackedMessages_;

            public Builder SetUnackedMessages(long Value)
			{
				BitField0_ |= 0x00000100;
				UnackedMessages_ = Value;

				return this;
			}
			public Builder ClearUnackedMessages()
			{
				BitField0_ = (BitField0_ & ~0x00000100);
				UnackedMessages_ = 0L;

				return this;
			}

			// optional bool blockedConsumerOnUnackedMsgs = 10;
			internal bool BlockedConsumerOnUnackedMsgs_;
			public bool HasBlockedConsumerOnUnackedMsgs()
			{
				return ((BitField0_ & 0x00000200) == 0x00000200);
			}
			public bool BlockedConsumerOnUnackedMsgs => BlockedConsumerOnUnackedMsgs_;

            public Builder SetBlockedConsumerOnUnackedMsgs(bool Value)
			{
				BitField0_ |= 0x00000200;
				BlockedConsumerOnUnackedMsgs_ = Value;

				return this;
			}
			public Builder ClearBlockedConsumerOnUnackedMsgs()
			{
				BitField0_ = (BitField0_ & ~0x00000200);
				BlockedConsumerOnUnackedMsgs_ = false;

				return this;
			}

			// optional string address = 11;
			internal object Address_ = "";
			public bool HasAddress()
			{
				return ((BitField0_ & 0x00000400) == 0x00000400);
			}
			public string GetAddress()
			{
				object Ref = Address_;
				if (!(Ref is string))
				{
					string S = ((ByteString)Ref).ToStringUtf8();
					Address_ = S;
					return S;
				}
				else
				{
					return (string)Ref;
				}
			}
			public Builder SetAddress(string Value)
			{
				if (string.ReferenceEquals(Value, null))
				{
					throw new NullReferenceException();
				}
				BitField0_ |= 0x00000400;
				Address_ = Value;

				return this;
			}
			public Builder ClearAddress()
			{
				BitField0_ = (BitField0_ & ~0x00000400);
				Address_ = DefaultInstance.Address;

				return this;
			}
			public void SetAddress(ByteString Value)
			{
				BitField0_ |= 0x00000400;
				Address_ = Value;

			}

			// optional string connectedSince = 12;
			internal object ConnectedSince_ = "";
			public bool HasConnectedSince()
			{
				return ((BitField0_ & 0x00000800) == 0x00000800);
			}
			public string GetConnectedSince()
			{
				object Ref = ConnectedSince_;
				if (!(Ref is string))
				{
					string S = ((ByteString)Ref).ToStringUtf8();
					ConnectedSince_ = S;
					return S;
				}
				else
				{
					return (string)Ref;
				}
			}
			public Builder SetConnectedSince(string Value)
			{
				if (string.ReferenceEquals(Value, null))
				{
					throw new NullReferenceException();
				}
				BitField0_ |= 0x00000800;
				ConnectedSince_ = Value;

				return this;
			}
			public Builder ClearConnectedSince()
			{
				BitField0_ = (BitField0_ & ~0x00000800);
				ConnectedSince_ = DefaultInstance.ConnectedSince;

				return this;
			}
			public void SetConnectedSince(ByteString Value)
			{
				BitField0_ |= 0x00000800;
				ConnectedSince_ = Value;

			}

			// optional string type = 13;
			internal object Type_ = "";
			public bool HasType()
			{
				return ((BitField0_ & 0x00001000) == 0x00001000);
			}
			public string GetType()
			{
				object Ref = Type_;
				if (!(Ref is string))
				{
					string S = ((ByteString)Ref).ToStringUtf8();
					Type_ = S;
					return S;
				}
				else
				{
					return (string)Ref;
				}
			}
			public Builder SetType(string Value)
			{
				if (string.ReferenceEquals(Value, null))
				{
					throw new NullReferenceException();
				}
				BitField0_ |= 0x00001000;
				Type_ = Value;

				return this;
			}
			public Builder ClearType()
			{
				BitField0_ = (BitField0_ & ~0x00001000);
				Type_ = DefaultInstance.Type;

				return this;
			}
			public void SetType(ByteString Value)
			{
				BitField0_ |= 0x00001000;
				Type_ = Value;

			}

			// optional double msgRateExpired = 14;
			internal double MsgRateExpired_;
			public bool HasMsgRateExpired()
			{
				return ((BitField0_ & 0x00002000) == 0x00002000);
			}
			public double MsgRateExpired => MsgRateExpired_;

            public Builder SetMsgRateExpired(double Value)
			{
				BitField0_ |= 0x00002000;
				MsgRateExpired_ = Value;

				return this;
			}
			public Builder ClearMsgRateExpired()
			{
				BitField0_ = (BitField0_ & ~0x00002000);
				MsgRateExpired_ = 0D;

				return this;
			}

			// optional uint64 msgBacklog = 15;
			internal long MsgBacklog_;
			public bool HasMsgBacklog()
			{
				return ((BitField0_ & 0x00004000) == 0x00004000);
			}
			public long MsgBacklog => MsgBacklog_;

            public Builder SetMsgBacklog(long Value)
			{
				BitField0_ |= 0x00004000;
				MsgBacklog_ = Value;

				return this;
			}
			public Builder ClearMsgBacklog()
			{
				BitField0_ = (BitField0_ & ~0x00004000);
				MsgBacklog_ = 0L;

				return this;
			}

			// @@protoc_insertion_point(builder_scope:pulsar.proto.CommandConsumerStatsResponse)
		}

		static CommandConsumerStatsResponse()
		{
			_defaultInstance = new CommandConsumerStatsResponse(true);
			_defaultInstance.InitFields();
		}

		// @@protoc_insertion_point(class_scope:pulsar.proto.CommandConsumerStatsResponse)
	}

}
