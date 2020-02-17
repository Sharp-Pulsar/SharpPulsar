using DotNetty.Common;
using Google.Protobuf;
using System;
using SharpPulsar.Utility.Protobuf;
using static SharpPulsar.Utility.Protobuf.ByteBufCodedInputStream;

namespace SharpPulsar.Protocol.Proto
{
	public sealed partial class CommandGetLastMessageIdResponse : ByteBufCodedOutputStream.ByteBufGeneratedMessage
	{
		// Use CommandGetLastMessageIdResponse.newBuilder() to construct.
		internal static ThreadLocalPool<CommandGetLastMessageIdResponse> _pool = new ThreadLocalPool<CommandGetLastMessageIdResponse>(handle => new CommandGetLastMessageIdResponse(handle), 1, true);

		internal ThreadLocalPool.Handle _handle;
		private CommandGetLastMessageIdResponse(ThreadLocalPool.Handle handle)
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

		public CommandGetLastMessageIdResponse(bool NoInit)
		{
		}

		internal static readonly CommandGetLastMessageIdResponse _defaultInstance;
		public static CommandGetLastMessageIdResponse DefaultInstance => _defaultInstance;

        public CommandGetLastMessageIdResponse DefaultInstanceForType => _defaultInstance;

        public void InitFields()
		{
			LastMessageId = MessageIdData.DefaultInstance;
			RequestId = 0L;
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

				if (!HasLastMessageId)
				{
					MemoizedIsInitialized = 0;
					return false;
				}
				if (!HasRequestId)
				{
					MemoizedIsInitialized = 0;
					return false;
				}
				if (!LastMessageId.Initialized)
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
				Output.WriteMessage(1, LastMessageId);
			}
			if (((_hasBits0 & 0x00000002) == 0x00000002))
			{
				Output.WriteUInt64(2, (long)RequestId);
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
		public static Builder NewBuilder(CommandGetLastMessageIdResponse Prototype)
		{
			return NewBuilder().MergeFrom(Prototype);
		}
		public Builder ToBuilder()
		{
			return NewBuilder(this);
		}

		public sealed class Builder : ByteBufMessageBuilder
		{
			// Construct using org.apache.pulsar.common.api.proto.CommandGetLastMessageIdResponse.newBuilder()
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

				LastMessageId_ = MessageIdData.DefaultInstance;
				BitField0_ = (BitField0_ & ~0x00000001);
				RequestId_ = 0L;
				BitField0_ = (BitField0_ & ~0x00000002);
				return this;
			}

			public Builder Clone()
			{
				return Create().MergeFrom(BuildPartial());
			}

			public CommandGetLastMessageIdResponse DefaultInstanceForType => DefaultInstance;

            public CommandGetLastMessageIdResponse Build()
			{
				CommandGetLastMessageIdResponse Result = BuildPartial();
				if (!Result.Initialized)
				{
					throw new NullReferenceException($"{Result.GetType().Name} not initialized");
				}
				return Result;
			}

			public CommandGetLastMessageIdResponse BuildParsed()
			{
				CommandGetLastMessageIdResponse Result = BuildPartial();
				if (!Result.Initialized)
				{
					throw new NullReferenceException($"{Result.GetType().Name} not initialized");
				}
				return Result;
			}

			public CommandGetLastMessageIdResponse BuildPartial()
			{
				CommandGetLastMessageIdResponse Result = CommandGetLastMessageIdResponse._pool.Take();
				int FromBitField0_ = BitField0_;
				int ToBitField0_ = 0;
				if (((FromBitField0_ & 0x00000001) == 0x00000001))
				{
					ToBitField0_ |= 0x00000001;
				}
				Result.LastMessageId = LastMessageId_;
				if (((FromBitField0_ & 0x00000002) == 0x00000002))
				{
					ToBitField0_ |= 0x00000002;
				}
				Result.RequestId = (ulong)RequestId_;
				Result._hasBits0 = ToBitField0_;
				return Result;
			}

			public Builder MergeFrom(CommandGetLastMessageIdResponse Other)
			{
				if (Other == DefaultInstance)
				{
					return this;
				}
				if (Other.HasLastMessageId)
				{
					MergeLastMessageId(Other.LastMessageId);
				}
				if (Other.HasRequestId)
				{
					SetRequestId((long)Other.RequestId);
				}
				return this;
			}

			public bool Initialized
			{
				get
				{
					if (!HasLastMessageId())
					{

						return false;
					}
					if (!HasRequestId())
					{

						return false;
					}
					if (!GetLastMessageId().Initialized)
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
						case 10:
							{
								MessageIdData.Builder SubBuilder = MessageIdData.NewBuilder();
								if (HasLastMessageId())
								{
									SubBuilder.MergeFrom(GetLastMessageId());
								}
								input.ReadMessage(SubBuilder, extensionRegistry);
								SetLastMessageId(SubBuilder.BuildPartial());
								SubBuilder.Recycle();
								break;
							}
						case 16:
							{
								BitField0_ |= 0x00000002;
								RequestId_ = input.ReadUInt64();
								break;
							}
					}
				}
			}

			internal int BitField0_;

			// required .pulsar.proto.MessageIdData last_message_id = 1;
			internal MessageIdData LastMessageId_ = MessageIdData.DefaultInstance;
			public bool HasLastMessageId()
			{
				return ((BitField0_ & 0x00000001) == 0x00000001);
			}
			public MessageIdData GetLastMessageId()
			{
				return LastMessageId_;
			}
			public Builder SetLastMessageId(MessageIdData Value)
			{
				if (Value == null)
				{
					throw new NullReferenceException();
				}
				LastMessageId_ = Value;

				BitField0_ |= 0x00000001;
				return this;
			}
			public Builder setLastMessageId(MessageIdData.Builder BuilderForValue)
			{
				LastMessageId_ = BuilderForValue.Build();

				BitField0_ |= 0x00000001;
				return this;
			}
			public Builder MergeLastMessageId(MessageIdData Value)
			{
				if (((BitField0_ & 0x00000001) == 0x00000001) && LastMessageId_ != MessageIdData.DefaultInstance)
				{
					LastMessageId_ = MessageIdData.NewBuilder(LastMessageId_).MergeFrom(Value).BuildPartial();
				}
				else
				{
					LastMessageId_ = Value;
				}

				BitField0_ |= 0x00000001;
				return this;
			}
			public Builder ClearLastMessageId()
			{
				LastMessageId_ = MessageIdData.DefaultInstance;

				BitField0_ = (BitField0_ & ~0x00000001);
				return this;
			}

			// required uint64 request_id = 2;
			internal long RequestId_;
			public bool HasRequestId()
			{
				return ((BitField0_ & 0x00000002) == 0x00000002);
			}
			public long RequestId => RequestId_;

            public Builder SetRequestId(long Value)
			{
				BitField0_ |= 0x00000002;
				RequestId_ = Value;

				return this;
			}
			public Builder ClearRequestId()
			{
				BitField0_ = (BitField0_ & ~0x00000002);
				RequestId_ = 0L;

				return this;
			}

			// @@protoc_insertion_point(builder_scope:pulsar.proto.CommandGetLastMessageIdResponse)
		}

		static CommandGetLastMessageIdResponse()
		{
			_defaultInstance = new CommandGetLastMessageIdResponse(true);
			_defaultInstance.InitFields();
		}

		// @@protoc_insertion_point(class_scope:pulsar.proto.CommandGetLastMessageIdResponse)
	}

}
