using DotNetty.Common;
using Google.Protobuf;
using System;
using SharpPulsar.Utility.Protobuf;
using static SharpPulsar.Utility.Protobuf.ByteBufCodedInputStream;

namespace SharpPulsar.Protocol.Proto
{
	public sealed partial class CommandMessage : ByteBufCodedOutputStream.ByteBufGeneratedMessage
	{
		// Use CommandMessage.newBuilder() to construct.
		internal static ThreadLocalPool<CommandMessage> _pool = new ThreadLocalPool<CommandMessage>(handle => new CommandMessage(handle), 1, true);

		internal ThreadLocalPool.Handle _handle;
		private CommandMessage(ThreadLocalPool.Handle handle)
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

		public CommandMessage(bool NoInit)
		{
		}

		
		internal static readonly CommandMessage _defaultInstance;
		public static CommandMessage DefaultInstance => _defaultInstance;

        public CommandMessage DefaultInstanceForType => _defaultInstance;


        public void InitFields()
		{
			ConsumerId = 0L;
			MessageId = MessageIdData.DefaultInstance;
			RedeliveryCount = 0;
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

				if (!HasConsumerId)
				{
					MemoizedIsInitialized = 0;
					return false;
				}
				if (!HasMessageId)
				{
					MemoizedIsInitialized = 0;
					return false;
				}
				if (!MessageId.Initialized)
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
				Output.WriteUInt64(1, (long)ConsumerId);
			}
			if (((_hasBits0 & 0x00000002) == 0x00000002))
			{
				Output.WriteMessage(2, MessageId);
			}
			if (((_hasBits0 & 0x00000004) == 0x00000004))
			{
				Output.WriteUInt32(3, (int)RedeliveryCount);
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
		public static Builder NewBuilder(CommandMessage Prototype)
		{
			return NewBuilder().MergeFrom(Prototype);
		}
		public Builder ToBuilder()
		{
			return NewBuilder(this);
		}

		public sealed class Builder : ByteBufMessageBuilder
		{
			// Construct using org.apache.pulsar.common.api.proto.CommandMessage.newBuilder()
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
				ConsumerId_ = 0L;
				BitField0_ = (BitField0_ & ~0x00000001);
				MessageId_ = MessageIdData.DefaultInstance;
				BitField0_ = (BitField0_ & ~0x00000002);
				RedeliveryCount_ = 0;
				BitField0_ = (BitField0_ & ~0x00000004);
				return this;
			}

			public Builder Clone()
			{
				return Create().MergeFrom(BuildPartial());
			}

			public CommandMessage DefaultInstanceForType => CommandMessage.DefaultInstance;

            public CommandMessage Build()
			{
				CommandMessage Result = BuildPartial();
				
				return Result;
			}

			public CommandMessage BuildParsed()
			{
				CommandMessage Result = BuildPartial();
				
				return Result;
			}

			public CommandMessage BuildPartial()
			{
				CommandMessage Result = CommandMessage._pool.Take();
				int FromBitField0_ = BitField0_;
				int ToBitField0_ = 0;
				if (((FromBitField0_ & 0x00000001) == 0x00000001))
				{
					ToBitField0_ |= 0x00000001;
				}
				Result.ConsumerId = (ulong)ConsumerId_;
				if (((FromBitField0_ & 0x00000002) == 0x00000002))
				{
					ToBitField0_ |= 0x00000002;
				}
				Result.MessageId = MessageId_;
				if (((FromBitField0_ & 0x00000004) == 0x00000004))
				{
					ToBitField0_ |= 0x00000004;
				}
				Result.RedeliveryCount = (uint)RedeliveryCount_;
				Result._hasBits0 = ToBitField0_;
				return Result;
			}

			public Builder MergeFrom(CommandMessage Other)
			{
				if (Other == CommandMessage.DefaultInstance)
				{
					return this;
				}
				if (Other.HasConsumerId)
				{
					SetConsumerId((long)Other.ConsumerId);
				}
				if (Other.HasMessageId)
				{
					MergeMessageId(Other.MessageId);
				}
				if (Other.HasRedeliveryCount)
				{
					SetRedeliveryCount((int)Other.RedeliveryCount);
				}
				return this;
			}

			public bool Initialized
			{
				get
				{
					if (!HasConsumerId())
					{

						return false;
					}
					if (!HasMessageId())
					{

						return false;
					}
					if (!GetMessageId().Initialized)
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
								ConsumerId_ = input.ReadUInt64();
								break;
							}
						case 18:
							{
								MessageIdData.Builder SubBuilder = MessageIdData.NewBuilder();
								if (HasMessageId())
								{
									SubBuilder.MergeFrom(GetMessageId());
								}
								input.ReadMessage(SubBuilder, extensionRegistry);
								SetMessageId(SubBuilder.BuildPartial());
								SubBuilder.Recycle();
								break;
							}
						case 24:
							{
								BitField0_ |= 0x00000004;
								RedeliveryCount_ = input.ReadUInt32();
								break;
							}
					}
				}
			}

			internal int BitField0_;

			// required uint64 consumer_id = 1;
			internal long ConsumerId_;
			public bool HasConsumerId()
			{
				return ((BitField0_ & 0x00000001) == 0x00000001);
			}
			public long ConsumerId => ConsumerId_;

            public Builder SetConsumerId(long Value)
			{
				BitField0_ |= 0x00000001;
				ConsumerId_ = Value;

				return this;
			}
			public Builder ClearConsumerId()
			{
				BitField0_ = (BitField0_ & ~0x00000001);
				ConsumerId_ = 0L;

				return this;
			}

			// required .pulsar.proto.MessageIdData message_id = 2;
			internal MessageIdData MessageId_ = MessageIdData.DefaultInstance;
			public bool HasMessageId()
			{
				return ((BitField0_ & 0x00000002) == 0x00000002);
			}
			public MessageIdData GetMessageId()
			{
				return MessageId_;
			}
			public Builder SetMessageId(MessageIdData Value)
			{
				if (Value == null)
				{
					throw new NullReferenceException();
				}
				MessageId_ = Value;

				BitField0_ |= 0x00000002;
				return this;
			}
			public Builder SetMessageId(MessageIdData.Builder BuilderForValue)
			{
				MessageId_ = BuilderForValue.Build();

				BitField0_ |= 0x00000002;
				return this;
			}
			public Builder MergeMessageId(MessageIdData Value)
			{
				if (((BitField0_ & 0x00000002) == 0x00000002) && MessageId_ != MessageIdData.DefaultInstance)
				{
					MessageId_ = MessageIdData.NewBuilder(MessageId_).MergeFrom(Value).BuildPartial();
				}
				else
				{
					MessageId_ = Value;
				}

				BitField0_ |= 0x00000002;
				return this;
			}
			public Builder ClearMessageId()
			{
				MessageId_ = MessageIdData.DefaultInstance;

				BitField0_ = (BitField0_ & ~0x00000002);
				return this;
			}

			// optional uint32 redelivery_count = 3 [default = 0];
			internal int RedeliveryCount_;
			public bool HasRedeliveryCount()
			{
				return ((BitField0_ & 0x00000004) == 0x00000004);
			}
			public int RedeliveryCount => RedeliveryCount_;

            public Builder SetRedeliveryCount(int Value)
			{
				BitField0_ |= 0x00000004;
				RedeliveryCount_ = Value;

				return this;
			}
			public Builder ClearRedeliveryCount()
			{
				BitField0_ = (BitField0_ & ~0x00000004);
				RedeliveryCount_ = 0;

				return this;
			}

			// @@protoc_insertion_point(builder_scope:pulsar.proto.CommandMessage)
		}

		static CommandMessage()
		{
			_defaultInstance = new CommandMessage(true);
			_defaultInstance.InitFields();
		}

		// @@protoc_insertion_point(class_scope:pulsar.proto.CommandMessage)
	}

}
