using DotNetty.Common;
using Google.Protobuf;
using SharpPulsar.Util.Protobuf;
using System;
using System.Collections.Generic;
using System.Text;
using static SharpPulsar.Util.Protobuf.ByteBufCodedInputStream;

namespace SharpPulsar.Protocol.Proto
{
	public sealed partial class CommandSendReceipt : ByteBufCodedOutputStream.ByteBufGeneratedMessage
	{
		// Use CommandSendReceipt.newBuilder() to construct.
		internal static ThreadLocalPool<CommandSendReceipt> _pool = new ThreadLocalPool<CommandSendReceipt>(handle => new CommandSendReceipt(handle), 1, true);

		internal ThreadLocalPool.Handle _handle;
		private CommandSendReceipt(ThreadLocalPool.Handle handle)
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

		public CommandSendReceipt(bool NoInit)
		{
		}

		
		internal static readonly CommandSendReceipt _defaultInstance;
		public static CommandSendReceipt DefaultInstance
		{
			get
			{
				return _defaultInstance;
			}
		}

		public CommandSendReceipt DefaultInstanceForType
		{
			get
			{
				return _defaultInstance;
			}
		}
		
		public void InitFields()
		{
			ProducerId = 0L;
			SequenceId = 0L;
			MessageId = MessageIdData.DefaultInstance;
			HighestSequenceId = 0L;
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

				if (!HasProducerId)
				{
					MemoizedIsInitialized = 0;
					return false;
				}
				if (!HasSequenceId)
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
				Output.WriteUInt64(1, (long)ProducerId);
			}
			if (((_hasBits0 & 0x00000002) == 0x00000002))
			{
				Output.WriteUInt64(2, (long)SequenceId);
			}
			if (((_hasBits0 & 0x00000004) == 0x00000004))
			{
				Output.WriteMessage(3, MessageId);
			}
			if (((_hasBits0 & 0x00000008) == 0x00000008))
			{
				Output.WriteUInt64(4, (long)HighestSequenceId);
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
		public static Builder NewBuilder(CommandSendReceipt Prototype)
		{
			return NewBuilder().MergeFrom(Prototype);
		}
		public Builder ToBuilder()
		{
			return NewBuilder(this);
		}

		public sealed class Builder : ByteBufMessageBuilder
		{
			// Construct using org.apache.pulsar.common.api.proto.CommandSendReceipt.newBuilder()
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
				ProducerId_ = 0L;
				BitField0_ = (BitField0_ & ~0x00000001);
				SequenceId_ = 0L;
				BitField0_ = (BitField0_ & ~0x00000002);
				MessageId_ = MessageIdData.DefaultInstance;
				BitField0_ = (BitField0_ & ~0x00000004);
				HighestSequenceId_ = 0L;
				BitField0_ = (BitField0_ & ~0x00000008);
				return this;
			}

			public Builder Clone()
			{
				return Create().MergeFrom(BuildPartial());
			}

			public CommandSendReceipt DefaultInstanceForType
			{
				get
				{
					return CommandSendReceipt.DefaultInstance;
				}
			}

			public CommandSendReceipt Build()
			{
				CommandSendReceipt Result = BuildPartial();
				if (!Result.Initialized)
				{
					throw new NullReferenceException($"{Result.GetType().Name} not initialized");
				}
				return Result;
			}

			public CommandSendReceipt BuildParsed()
			{
				CommandSendReceipt Result = BuildPartial();
				if (!Result.Initialized)
				{
					throw new NullReferenceException($"{Result.GetType().Name} not initialized");
				}
				return Result;
			}

			public CommandSendReceipt BuildPartial()
			{
				CommandSendReceipt Result = CommandSendReceipt._pool.Take();
				int FromBitField0_ = BitField0_;
				int ToBitField0_ = 0;
				if (((FromBitField0_ & 0x00000001) == 0x00000001))
				{
					ToBitField0_ |= 0x00000001;
				}
				Result.ProducerId = (ulong)ProducerId_;
				if (((FromBitField0_ & 0x00000002) == 0x00000002))
				{
					ToBitField0_ |= 0x00000002;
				}
				Result.SequenceId = (ulong)SequenceId_;
				if (((FromBitField0_ & 0x00000004) == 0x00000004))
				{
					ToBitField0_ |= 0x00000004;
				}
				Result.MessageId = MessageId_;
				if (((FromBitField0_ & 0x00000008) == 0x00000008))
				{
					ToBitField0_ |= 0x00000008;
				}
				Result.HighestSequenceId = (ulong)HighestSequenceId_;
				Result._hasBits0 = ToBitField0_;
				return Result;
			}

			public Builder MergeFrom(CommandSendReceipt Other)
			{
				if (Other == CommandSendReceipt.DefaultInstance)
				{
					return this;
				}
				if (Other.HasProducerId)
				{
					SetProducerId((long)Other.ProducerId);
				}
				if (Other.HasSequenceId)
				{
					SetSequenceId((long)Other.SequenceId);
				}
				if (Other.HasMessageId)
				{
					MergeMessageId(Other.MessageId);
				}
				if (Other.HasHighestSequenceId)
				{
					SetHighestSequenceId((long)Other.HighestSequenceId);
				}
				return this;
			}

			public bool Initialized
			{
				get
				{
					if (!HasProducerId())
					{

						return false;
					}
					if (!HasSequenceId())
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
			public ByteBufMessageBuilder MergeFrom(ByteBufCodedInputStream Input, ExtensionRegistry extensionRegistry)
			{
				while (true)
				{
					int Tag = Input.ReadTag();
					switch (Tag)
					{
						case 0:

							return this;
						default:
							{
								if (!Input.SkipField(Tag))
								{

									return this;
								}
								break;
							}
						case 8:
							{
								BitField0_ |= 0x00000001;
								ProducerId_ = Input.ReadUInt64();
								break;
							}
						case 16:
							{
								BitField0_ |= 0x00000002;
								SequenceId_ = Input.ReadUInt64();
								break;
							}
						case 26:
							{
								MessageIdData.Builder SubBuilder = MessageIdData.NewBuilder();
								if (HasMessageId())
								{
									SubBuilder.MergeFrom(GetMessageId());
								}
								Input.ReadMessage(SubBuilder, extensionRegistry);
								SetMessageId(SubBuilder.BuildPartial());
								SubBuilder.Recycle();
								break;
							}
						case 32:
							{
								BitField0_ |= 0x00000008;
								HighestSequenceId_ = Input.ReadUInt64();
								break;
							}
					}
				}
			}

			internal int BitField0_;

			// required uint64 producer_id = 1;
			internal long ProducerId_;
			public bool HasProducerId()
			{
				return ((BitField0_ & 0x00000001) == 0x00000001);
			}
			public long ProducerId
			{
				get
				{
					return ProducerId_;
				}
			}
			public Builder SetProducerId(long Value)
			{
				BitField0_ |= 0x00000001;
				ProducerId_ = Value;

				return this;
			}
			public Builder ClearProducerId()
			{
				BitField0_ = (BitField0_ & ~0x00000001);
				ProducerId_ = 0L;

				return this;
			}

			// required uint64 sequence_id = 2;
			internal long SequenceId_;
			public bool HasSequenceId()
			{
				return ((BitField0_ & 0x00000002) == 0x00000002);
			}
			public long SequenceId
			{
				get
				{
					return SequenceId_;
				}
			}
			public Builder SetSequenceId(long Value)
			{
				BitField0_ |= 0x00000002;
				SequenceId_ = Value;

				return this;
			}
			public Builder ClearSequenceId()
			{
				BitField0_ = (BitField0_ & ~0x00000002);
				SequenceId_ = 0L;

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

			// optional uint64 highest_sequence_id = 4 [default = 0];
			internal long HighestSequenceId_;
			public bool HasHighestSequenceId()
			{
				return ((BitField0_ & 0x00000008) == 0x00000008);
			}
			public long HighestSequenceId
			{
				get
				{
					return HighestSequenceId_;
				}
			}
			public Builder SetHighestSequenceId(long Value)
			{
				BitField0_ |= 0x00000008;
				HighestSequenceId_ = Value;

				return this;
			}
			public Builder ClearHighestSequenceId()
			{
				BitField0_ = (BitField0_ & ~0x00000008);
				HighestSequenceId_ = 0L;

				return this;
			}

			// @@protoc_insertion_point(builder_scope:pulsar.proto.CommandSendReceipt)
		}

		static CommandSendReceipt()
		{
			_defaultInstance = new CommandSendReceipt(true);
			_defaultInstance.InitFields();
		}

		// @@protoc_insertion_point(class_scope:pulsar.proto.CommandSendReceipt)
	}

}
