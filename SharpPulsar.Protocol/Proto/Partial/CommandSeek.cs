using DotNetty.Common;
using Google.Protobuf;
using SharpPulsar.Util.Protobuf;
using System;

using static SharpPulsar.Util.Protobuf.ByteBufCodedInputStream;

namespace SharpPulsar.Protocol.Proto
{
	public sealed partial class CommandSeek : ByteBufCodedOutputStream.ByteBufGeneratedMessage
	{
		// Use CommandSeek.newBuilder() to construct.
		internal static ThreadLocalPool<CommandSeek> _pool = new ThreadLocalPool<CommandSeek>(handle => new CommandSeek(handle), 1, true);

		internal ThreadLocalPool.Handle _handle;
		private CommandSeek(ThreadLocalPool.Handle handle)
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

		public CommandSeek(bool NoInit)
		{
		}


		internal static readonly CommandSeek _defaultInstance;
		public static CommandSeek DefaultInstance
		{
			get
			{
				return _defaultInstance;
			}
		}

		public CommandSeek DefaultInstanceForType
		{
			get
			{
				return _defaultInstance;
			}
		}

		public void InitFields()
		{
			ConsumerId = 0L;
			RequestId = 0L;
			MessageId = MessageIdData.DefaultInstance;
			MessagePublishTime = 0L;
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
				if (!HasRequestId)
				{
					MemoizedIsInitialized = 0;
					return false;
				}
				if (HasMessageId)
				{
					if (!MessageId.Initialized)
					{
						MemoizedIsInitialized = 0;
						return false;
					}
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
				Output.WriteUInt64(2, (long)RequestId);
			}
			if (((_hasBits0 & 0x00000004) == 0x00000004))
			{
				Output.WriteMessage(3, MessageId);
			}
			if (((_hasBits0 & 0x00000008) == 0x00000008))
			{
				Output.WriteUInt64(4, (long)MessagePublishTime);
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
		public static Builder NewBuilder(CommandSeek Prototype)
		{
			return NewBuilder().MergeFrom(Prototype);
		}
		public Builder ToBuilder()
		{
			return NewBuilder(this);
		}

		public sealed class Builder : ByteBufMessageBuilder
		{
			// Construct using org.apache.pulsar.common.api.proto.CommandSeek.newBuilder()
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
				RequestId_ = 0L;
				BitField0_ = (BitField0_ & ~0x00000002);
				MessageId_ = MessageIdData.DefaultInstance;
				BitField0_ = (BitField0_ & ~0x00000004);
				MessagePublishTime_ = 0L;
				BitField0_ = (BitField0_ & ~0x00000008);
				return this;
			}

			public Builder Clone()
			{
				return Create().MergeFrom(BuildPartial());
			}

			public CommandSeek DefaultInstanceForType
			{
				get
				{
					return CommandSeek.DefaultInstance;
				}
			}

			public CommandSeek Build()
			{
				CommandSeek Result = BuildPartial();
				if (!Result.Initialized)
				{
					throw new NullReferenceException($"{Result.GetType().Name} not initialized");
				}
				return Result;
			}

			
			public CommandSeek BuildParsed()
			{
				CommandSeek Result = BuildPartial();
				if (!Result.Initialized)
				{
					throw new NullReferenceException($"{Result.GetType().Name} not initialized");
				}
				return Result;
			}

			public CommandSeek BuildPartial()
			{
				CommandSeek Result = CommandSeek._pool.Take();
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
				Result.RequestId = (ulong)RequestId_;
				if (((FromBitField0_ & 0x00000004) == 0x00000004))
				{
					ToBitField0_ |= 0x00000004;
				}
				Result.MessageId = MessageId_;
				if (((FromBitField0_ & 0x00000008) == 0x00000008))
				{
					ToBitField0_ |= 0x00000008;
				}
				Result.MessagePublishTime = (ulong)MessagePublishTime_;
				Result._hasBits0 = ToBitField0_;
				return Result;
			}

			public Builder MergeFrom(CommandSeek Other)
			{
				if (Other == CommandSeek.DefaultInstance)
				{
					return this;
				}
				if (Other.HasConsumerId)
				{
					SetConsumerId((long)Other.ConsumerId);
				}
				if (Other.HasRequestId)
				{
					SetRequestId((long)Other.RequestId);
				}
				if (Other.HasMessageId)
				{
					MergeMessageId(Other.MessageId);
				}
				if (Other.HasMessagePublishTime)
				{
					SetMessagePublishTime((long)Other.MessagePublishTime);
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
					if (!HasRequestId())
					{

						return false;
					}
					if (HasMessageId())
					{
						if (!GetMessageId().Initialized)
						{

							return false;
						}
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
						case 16:
							{
								BitField0_ |= 0x00000002;
								RequestId_ = input.ReadUInt64();
								break;
							}
						case 26:
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
						case 32:
							{
								BitField0_ |= 0x00000008;
								MessagePublishTime_ = input.ReadUInt64();
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
			public long ConsumerId
			{
				get
				{
					return ConsumerId_;
				}
			}
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

			// required uint64 request_id = 2;
			internal long RequestId_;
			public bool HasRequestId()
			{
				return ((BitField0_ & 0x00000002) == 0x00000002);
			}
			public long RequestId
			{
				get
				{
					return RequestId_;
				}
			}
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

			// optional .pulsar.proto.MessageIdData message_id = 3;
			internal MessageIdData MessageId_ = MessageIdData.DefaultInstance;
			public bool HasMessageId()
			{
				return ((BitField0_ & 0x00000004) == 0x00000004);
			}
			public MessageIdData GetMessageId()
			{
				return MessageId_;
			}
			public Builder SetMessageId(MessageIdData Value)
			{
				if (Value == null)
				{
					throw new System.NullReferenceException();
				}
				MessageId_ = Value;

				BitField0_ |= 0x00000004;
				return this;
			}
			public Builder SetMessageId(MessageIdData.Builder BuilderForValue)
			{
				MessageId_ = BuilderForValue.Build();

				BitField0_ |= 0x00000004;
				return this;
			}
			public Builder MergeMessageId(MessageIdData Value)
			{
				if (((BitField0_ & 0x00000004) == 0x00000004) && MessageId_ != MessageIdData.DefaultInstance)
				{
					MessageId_ = MessageIdData.NewBuilder(MessageId_).MergeFrom(Value).BuildPartial();
				}
				else
				{
					MessageId_ = Value;
				}

				BitField0_ |= 0x00000004;
				return this;
			}
			public Builder ClearMessageId()
			{
				MessageId_ = MessageIdData.DefaultInstance;

				BitField0_ = (BitField0_ & ~0x00000004);
				return this;
			}

			// optional uint64 message_publish_time = 4;
			internal long MessagePublishTime_;
			public bool HasMessagePublishTime()
			{
				return ((BitField0_ & 0x00000008) == 0x00000008);
			}
			public long MessagePublishTime
			{
				get
				{
					return MessagePublishTime_;
				}
			}
			public Builder SetMessagePublishTime(long Value)
			{
				BitField0_ |= 0x00000008;
				MessagePublishTime_ = Value;

				return this;
			}
			public Builder ClearMessagePublishTime()
			{
				BitField0_ = (BitField0_ & ~0x00000008);
				MessagePublishTime_ = 0L;

				return this;
			}

			// @@protoc_insertion_point(builder_scope:pulsar.proto.CommandSeek)
		}

		static CommandSeek()
		{
			_defaultInstance = new CommandSeek(true);
			_defaultInstance.InitFields();
		}

		// @@protoc_insertion_point(class_scope:pulsar.proto.CommandSeek)
	}

}
