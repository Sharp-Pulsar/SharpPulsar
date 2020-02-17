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
	public sealed partial class CommandAddSubscriptionToTxnResponse : ByteBufCodedOutputStream.ByteBufGeneratedMessage
	{
		internal static ThreadLocalPool<CommandAddSubscriptionToTxnResponse> _pool = new ThreadLocalPool<CommandAddSubscriptionToTxnResponse>(handle => new CommandAddSubscriptionToTxnResponse(handle), 1, true);

		internal ThreadLocalPool.Handle _handle;
		private CommandAddSubscriptionToTxnResponse(ThreadLocalPool.Handle handle)
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

		public CommandAddSubscriptionToTxnResponse(bool NoInit)
		{
		}


		internal static readonly CommandAddSubscriptionToTxnResponse _defaultInstance;
		public static CommandAddSubscriptionToTxnResponse DefaultInstance => _defaultInstance;

        public CommandAddSubscriptionToTxnResponse DefaultInstanceForType => _defaultInstance;


        public void InitFields()
		{
			RequestId = 0L;
			TxnidLeastBits = 0L;
			TxnidMostBits = 0L;
			Error = ServerError.UnknownError;
			Message = "";
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
			var _= SerializedSize;
			if (((_hasBits0 & 0x00000001) == 0x00000001))
			{
				Output.WriteUInt64(1, (long)RequestId);
			}
			if (((_hasBits0 & 0x00000002) == 0x00000002))
			{
				Output.WriteUInt64(2, (long)TxnidLeastBits);
			}
			if (((_hasBits0 & 0x00000004) == 0x00000004))
			{
				Output.WriteUInt64(3, (long)TxnidMostBits);
			}
			if (((_hasBits0 & 0x00000008) == 0x00000008))
			{
				Output.WriteEnum(4, (int)Error);
			}
			if (((_hasBits0 & 0x00000010) == 0x00000010))
			{
				Output.WriteBytes(5, ByteString.CopyFromUtf8(Message));
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
		public static Builder NewBuilder(CommandAddSubscriptionToTxnResponse Prototype)
		{
			return NewBuilder().MergeFrom(Prototype);
		}
		public Builder ToBuilder()
		{
			return NewBuilder(this);
		}

		public sealed class Builder : ByteBufMessageBuilder
		{
			// Construct using org.apache.pulsar.common.api.proto.CommandAddSubscriptionToTxnResponse.newBuilder()
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
				TxnidLeastBits_ = 0L;
				BitField0_ = (BitField0_ & ~0x00000002);
				TxnidMostBits_ = 0L;
				BitField0_ = (BitField0_ & ~0x00000004);
				Error_ = ServerError.UnknownError;
				BitField0_ = (BitField0_ & ~0x00000008);
				Message_ = "";
				BitField0_ = (BitField0_ & ~0x00000010);
				return this;
			}

			public Builder Clone()
			{
				return Create().MergeFrom(BuildPartial());
			}

			public CommandAddSubscriptionToTxnResponse DefaultInstanceForType => CommandAddSubscriptionToTxnResponse.DefaultInstance;

            public CommandAddSubscriptionToTxnResponse Build()
			{
				CommandAddSubscriptionToTxnResponse Result = BuildPartial();
				if (!Result.Initialized)
				{
					throw new NullReferenceException($"{Result.GetType().Name} not initialized");
				}
				return Result;
			}

			public CommandAddSubscriptionToTxnResponse BuildParsed()
			{
				CommandAddSubscriptionToTxnResponse Result = BuildPartial();
				if (!Result.Initialized)
				{
					throw new NullReferenceException($"{Result.GetType().Name} not initialized");
				}
				return Result;
			}

			public CommandAddSubscriptionToTxnResponse BuildPartial()
			{
				CommandAddSubscriptionToTxnResponse Result = CommandAddSubscriptionToTxnResponse._pool.Take();
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
				Result.TxnidLeastBits = (ulong)TxnidLeastBits_;
				if (((FromBitField0_ & 0x00000004) == 0x00000004))
				{
					ToBitField0_ |= 0x00000004;
				}
				Result.TxnidMostBits = (ulong)TxnidMostBits_;
				if (((FromBitField0_ & 0x00000008) == 0x00000008))
				{
					ToBitField0_ |= 0x00000008;
				}
				Result.Error = Error_;
				if (((FromBitField0_ & 0x00000010) == 0x00000010))
				{
					ToBitField0_ |= 0x00000010;
				}
				Result.Message = Message_.ToString();
				Result._hasBits0 = ToBitField0_;
				return Result;
			}

			public Builder MergeFrom(CommandAddSubscriptionToTxnResponse Other)
			{
				if (Other == DefaultInstance)
				{
					return this;
				}
				if (Other.HasRequestId)
				{
					SetRequestId((long)Other.RequestId);
				}
				if (Other.HasTxnidLeastBits)
				{
					SetTxnidLeastBits((long)Other.TxnidLeastBits);
				}
				if (Other.HasTxnidMostBits)
				{
					SetTxnidMostBits((long)Other.TxnidMostBits);
				}
				if (Other.HasError)
				{
					SetError(Other.Error);
				}
				if (Other.HasMessage)
				{
					SetMessage(Other.Message);
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
								BitField0_ |= 0x00000002;
								TxnidLeastBits_ = input.ReadUInt64();
								break;
							}
						case 24:
							{
								BitField0_ |= 0x00000004;
								TxnidMostBits_ = input.ReadUInt64();
								break;
							}
						case 32:
							{
								int RawValue = input.ReadEnum();
								ServerError Value = Enum.GetValues(typeof(ServerError)).Cast<ServerError>().ToList()[RawValue];
								if (Value != null)
								{
									BitField0_ |= 0x00000008;
									Error_ = Value;
								}
								break;
							}
						case 42:
							{
								BitField0_ |= 0x00000010;
								Message_ = input.ReadBytes();
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

			// optional uint64 txnid_least_bits = 2 [default = 0];
			internal long TxnidLeastBits_;
			public bool HasTxnidLeastBits()
			{
				return ((BitField0_ & 0x00000002) == 0x00000002);
			}
			public long TxnidLeastBits => TxnidLeastBits_;

            public Builder SetTxnidLeastBits(long Value)
			{
				BitField0_ |= 0x00000002;
				TxnidLeastBits_ = Value;

				return this;
			}
			public Builder ClearTxnidLeastBits()
			{
				BitField0_ = (BitField0_ & ~0x00000002);
				TxnidLeastBits_ = 0L;

				return this;
			}

			// optional uint64 txnid_most_bits = 3 [default = 0];
			internal long TxnidMostBits_;
			public bool HasTxnidMostBits()
			{
				return ((BitField0_ & 0x00000004) == 0x00000004);
			}
			public long TxnidMostBits => TxnidMostBits_;

            public Builder SetTxnidMostBits(long Value)
			{
				BitField0_ |= 0x00000004;
				TxnidMostBits_ = Value;

				return this;
			}
			public Builder ClearTxnidMostBits()
			{
				BitField0_ = (BitField0_ & ~0x00000004);
				TxnidMostBits_ = 0L;

				return this;
			}

			// optional .pulsar.proto.ServerError error = 4;
			internal ServerError Error_ = ServerError.UnknownError;
			public bool HasError()
			{
				return ((BitField0_ & 0x00000008) == 0x00000008);
			}
			public ServerError Error => Error_;

            public Builder SetError(ServerError Value)
			{
				if (Value == null)
				{
					throw new NullReferenceException();
				}
				BitField0_ |= 0x00000008;
				Error_ = Value;

				return this;
			}
			public Builder ClearError()
			{
				BitField0_ = (BitField0_ & ~0x00000008);
				Error_ = ServerError.UnknownError;

				return this;
			}

			// optional string message = 5;
			internal object Message_ = "";
			public bool HasMessage()
			{
				return ((BitField0_ & 0x00000010) == 0x00000010);
			}
			public string GetMessage()
			{
				object Ref = Message_;
				if (!(Ref is string))
				{
					string S = ((ByteString)Ref).ToStringUtf8();
					Message_ = S;
					return S;
				}
				else
				{
					return (string)Ref;
				}
			}
			public Builder SetMessage(string Value)
			{
				if (string.ReferenceEquals(Value, null))
				{
					throw new NullReferenceException();
				}
				BitField0_ |= 0x00000010;
				Message_ = Value;

				return this;
			}
			public Builder ClearMessage()
			{
				BitField0_ = (BitField0_ & ~0x00000010);
				Message_ = DefaultInstance.Message;

				return this;
			}
			public void SetMessage(ByteString Value)
			{
				BitField0_ |= 0x00000010;
				Message_ = Value;

			}

			
		}

		static CommandAddSubscriptionToTxnResponse()
		{
			_defaultInstance = new CommandAddSubscriptionToTxnResponse(true);
			_defaultInstance.InitFields();
		}

		
	}

}
