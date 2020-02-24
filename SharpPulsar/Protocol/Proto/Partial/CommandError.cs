using DotNetty.Common;
using Google.Protobuf;
using System;
using System.Linq;
using SharpPulsar.Utility.Protobuf;
using static SharpPulsar.Utility.Protobuf.ByteBufCodedInputStream;

namespace SharpPulsar.Protocol.Proto
{
	public sealed partial class CommandError : ByteBufCodedOutputStream.ByteBufGeneratedMessage
	{
		// Use CommandError.newBuilder() to construct.
		internal static ThreadLocalPool<CommandError> _pool = new ThreadLocalPool<CommandError>(handle => new CommandError(handle), 1, true);

		internal ThreadLocalPool.Handle _handle;
		private CommandError(ThreadLocalPool.Handle handle)
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

		public CommandError(bool NoInit)
		{
		}

		
		internal static readonly CommandError _defaultInstance;
		public static CommandError DefaultInstance => _defaultInstance;

        public CommandError DefaultInstanceForType => _defaultInstance;


        public void InitFields()
		{
			RequestId = 0L;
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
				if (!HasError)
				{
					MemoizedIsInitialized = 0;
					return false;
				}
				if (!HasMessage)
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
				Output.WriteEnum(2, (int)Error);
			}
			if (((_hasBits0 & 0x00000004) == 0x00000004))
			{
				Output.WriteBytes(3, ByteString.CopyFromUtf8(Message));
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
		public static Builder NewBuilder(CommandError Prototype)
		{
			return NewBuilder().MergeFrom(Prototype);
		}
		public Builder ToBuilder()
		{
			return NewBuilder(this);
		}

		public sealed class Builder : ByteBufMessageBuilder
        {
			// Construct using org.apache.pulsar.common.api.proto.CommandError.newBuilder()
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
				Error_ = ServerError.UnknownError;
				BitField0_ = (BitField0_ & ~0x00000002);
				Message_ = "";
				BitField0_ = (BitField0_ & ~0x00000004);
				return this;
			}

			public Builder Clone()
			{
				return Create().MergeFrom(BuildPartial());
			}

			public CommandError DefaultInstanceForType => CommandError.DefaultInstance;

            public CommandError Build()
			{
				CommandError Result = BuildPartial();
				
				return Result;
			}

			public CommandError BuildParsed()
			{
				CommandError Result = BuildPartial();
				
				return Result;
			}

			public CommandError BuildPartial()
			{
				CommandError Result = CommandError._pool.Take();
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
				Result.Error = Error_;
				if (((FromBitField0_ & 0x00000004) == 0x00000004))
				{
					ToBitField0_ |= 0x00000004;
				}
				Result.Message = Message_.ToString();
				Result._hasBits0 = ToBitField0_;
				return Result;
			}

			public Builder MergeFrom(CommandError Other)
			{
				if (Other == CommandError.DefaultInstance)
				{
					return this;
				}
				if (Other.HasRequestId)
				{
					SetRequestId((long)Other.RequestId);
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
					if (!HasError())
					{

						return false;
					}
					if (!HasMessage())
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
									Error_ = Value;
								}
								break;
							}
						case 26:
							{
								BitField0_ |= 0x00000004;
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

			// required .pulsar.proto.ServerError error = 2;
			internal ServerError Error_ = ServerError.UnknownError;
			public bool HasError()
			{
				return ((BitField0_ & 0x00000002) == 0x00000002);
			}
			public ServerError Error => Error_;

            public Builder SetError(ServerError Value)
			{
				if (Value == null)
				{
					throw new NullReferenceException();
				}
				BitField0_ |= 0x00000002;
				Error_ = Value;

				return this;
			}
			public Builder ClearError()
			{
				BitField0_ = (BitField0_ & ~0x00000002);
				Error_ = ServerError.UnknownError;

				return this;
			}

			// required string message = 3;
			internal object Message_ = "";
			public bool HasMessage()
			{
				return ((BitField0_ & 0x00000004) == 0x00000004);
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
				BitField0_ |= 0x00000004;
				Message_ = Value;

				return this;
			}
			public Builder ClearMessage()
			{
				BitField0_ = (BitField0_ & ~0x00000004);
				Message_ = DefaultInstance.Message;

				return this;
			}
			public void SetMessage(ByteString Value)
			{
				BitField0_ |= 0x00000004;
				Message_ = Value;

			}

			// @@protoc_insertion_point(builder_scope:pulsar.proto.CommandError)
		}

		static CommandError()
		{
			_defaultInstance = new CommandError(true);
			_defaultInstance.InitFields();
		}

		// @@protoc_insertion_point(class_scope:pulsar.proto.CommandError)
	}

}
