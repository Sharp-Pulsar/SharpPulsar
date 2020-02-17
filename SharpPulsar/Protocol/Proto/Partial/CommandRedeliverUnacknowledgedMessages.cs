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
	public sealed partial class CommandRedeliverUnacknowledgedMessages : ByteBufCodedOutputStream.ByteBufGeneratedMessage
	{
		// Use CommandRedeliverUnacknowledgedMessages.newBuilder() to construct.
		internal static ThreadLocalPool<CommandRedeliverUnacknowledgedMessages> _pool = new ThreadLocalPool<CommandRedeliverUnacknowledgedMessages>(handle => new CommandRedeliverUnacknowledgedMessages(handle), 1, true);

		internal ThreadLocalPool.Handle _handle;
		private CommandRedeliverUnacknowledgedMessages(ThreadLocalPool.Handle handle)
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

		public CommandRedeliverUnacknowledgedMessages(bool NoInit)
		{
		}

		
		internal static readonly CommandRedeliverUnacknowledgedMessages _defaultInstance;
		public static CommandRedeliverUnacknowledgedMessages DefaultInstance => _defaultInstance;

        public CommandRedeliverUnacknowledgedMessages DefaultInstanceForType => _defaultInstance;

        public MessageIdData GetMessageIds(int Index)
		{
			return MessageIds[Index];
		}
		
		public void InitFields()
		{
			ConsumerId = 0L;
			MessageIds.Clear();
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
				for (int I = 0; I < MessageIds.Count; I++)
				{
					if (!GetMessageIds(I).Initialized)
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
			var _= SerializedSize;
			if (((_hasBits0 & 0x00000001) == 0x00000001))
			{
				Output.WriteUInt64(1, (long)ConsumerId);
			}
			for (int I = 0; I < MessageIds.Count; I++)
			{
				Output.WriteMessage(2, MessageIds[I]);
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
		public static Builder NewBuilder(CommandRedeliverUnacknowledgedMessages Prototype)
		{
			return NewBuilder().MergeFrom(Prototype);
		}
		public Builder ToBuilder()
		{
			return NewBuilder(this);
		}

		public sealed class Builder : ByteBufMessageBuilder
		{
			// Construct using org.apache.pulsar.common.api.proto.CommandRedeliverUnacknowledgedMessages.newBuilder()
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
				MessageIds_ = new List<MessageIdData>();
				BitField0_ = (BitField0_ & ~0x00000002);
				return this;
			}

			public Builder Clone()
			{
				return Create().MergeFrom(BuildPartial());
			}

			public CommandRedeliverUnacknowledgedMessages DefaultInstanceForType => CommandRedeliverUnacknowledgedMessages.DefaultInstance;

            public CommandRedeliverUnacknowledgedMessages Build()
			{
				CommandRedeliverUnacknowledgedMessages Result = BuildPartial();
				if (!Result.Initialized)
				{
					throw new NullReferenceException($"{Result.GetType().Name} not initialized");
				}
				return Result;
			}
			public CommandRedeliverUnacknowledgedMessages BuildParsed()
			{
				CommandRedeliverUnacknowledgedMessages Result = BuildPartial();
				if (!Result.Initialized)
				{
					throw new NullReferenceException($"{Result.GetType().Name} not initialized");
				}
				return Result;
			}

			public CommandRedeliverUnacknowledgedMessages BuildPartial()
			{
				CommandRedeliverUnacknowledgedMessages Result = CommandRedeliverUnacknowledgedMessages._pool.Take();
				int FromBitField0_ = BitField0_;
				int ToBitField0_ = 0;
				if (((FromBitField0_ & 0x00000001) == 0x00000001))
				{
					ToBitField0_ |= 0x00000001;
				}
				Result.ConsumerId = (ulong)ConsumerId_;
				if (((BitField0_ & 0x00000002) == 0x00000002))
				{
					MessageIds_ = new List<MessageIdData>(MessageIds_);
					BitField0_ = (BitField0_ & ~0x00000002);
				}
				Result.MessageIds.AddRange(MessageIds_);
				Result._hasBits0 = ToBitField0_;
				return Result;
			}

			public Builder MergeFrom(CommandRedeliverUnacknowledgedMessages Other)
			{
				if (Other == CommandRedeliverUnacknowledgedMessages.DefaultInstance)
				{
					return this;
				}
				if (Other.HasConsumerId)
				{
					SetConsumerId((long)Other.ConsumerId);
				}
				if (Other.MessageIds.Count > 0)
				{
					if (MessageIds_.Count == 0)
					{
						MessageIds_ = Other.MessageIds;
						BitField0_ = (BitField0_ & ~0x00000002);
					}
					else
					{
						EnsureMessageIdsIsMutable();
						((List<MessageIdData>)MessageIds_).AddRange(Other.MessageIds);
					}

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
					for (int I = 0; I < MessageIdsCount; I++)
					{
						if (!GetMessageIds(I).Initialized)
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
						case 18:
							{
								MessageIdData.Builder SubBuilder = MessageIdData.NewBuilder();
								input.ReadMessage(SubBuilder, extensionRegistry);
								AddMessageIds(SubBuilder.BuildPartial());
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

			// repeated .pulsar.proto.MessageIdData message_ids = 2;
			internal IList<MessageIdData> MessageIds_ = new List<MessageIdData>();
			public void EnsureMessageIdsIsMutable()
			{
				if (!((BitField0_ & 0x00000002) == 0x00000002))
				{
					MessageIds_ = new List<MessageIdData>(MessageIds_);
					BitField0_ |= 0x00000002;
				}
			}

			public IList<MessageIdData> MessageIdsList => new List<MessageIdData>(MessageIds_);

            public int MessageIdsCount => MessageIds_.Count;

            public MessageIdData GetMessageIds(int Index)
			{
				return MessageIds_[Index];
			}
			public Builder SetMessageIds(int Index, MessageIdData Value)
			{
				if (Value == null)
				{
					throw new NullReferenceException();
				}
				EnsureMessageIdsIsMutable();
				MessageIds_[Index] = Value;

				return this;
			}
			public Builder SetMessageIds(int Index, MessageIdData.Builder BuilderForValue)
			{
				EnsureMessageIdsIsMutable();
				MessageIds_[Index] = BuilderForValue.Build();

				return this;
			}
			public Builder AddMessageIds(MessageIdData Value)
			{
				if (Value == null)
				{
					throw new NullReferenceException();
				}
				EnsureMessageIdsIsMutable();
				MessageIds_.Add(Value);

				return this;
			}
			public Builder AddMessageIds(int Index, MessageIdData Value)
			{
				if (Value == null)
				{
					throw new NullReferenceException();
				}
				EnsureMessageIdsIsMutable();
				MessageIds_.Insert(Index, Value);

				return this;
			}
			public Builder AddMessageIds(MessageIdData.Builder BuilderForValue)
			{
				EnsureMessageIdsIsMutable();
				MessageIds_.Add(BuilderForValue.Build());

				return this;
			}
			public Builder AddMessageIds(int Index, MessageIdData.Builder BuilderForValue)
			{
				EnsureMessageIdsIsMutable();
				MessageIds_.Insert(Index, BuilderForValue.Build());

				return this;
			}
			public Builder AddAllMessageIds(IEnumerable<MessageIdData> Values)
			{
				EnsureMessageIdsIsMutable();
				Values.ToList().ForEach(MessageIds_.Add);

				return this;
			}
			public Builder ClearMessageIds()
			{
				MessageIds_ = new List<MessageIdData>();
				BitField0_ = (BitField0_ & ~0x00000002);

				return this;
			}
			public Builder RemoveMessageIds(int Index)
			{
				EnsureMessageIdsIsMutable();
				MessageIds_.RemoveAt(Index);

				return this;
			}

			// @@protoc_insertion_point(builder_scope:pulsar.proto.CommandRedeliverUnacknowledgedMessages)
		}

		static CommandRedeliverUnacknowledgedMessages()
		{
			_defaultInstance = new CommandRedeliverUnacknowledgedMessages(true);
			_defaultInstance.InitFields();
		}

		// @@protoc_insertion_point(class_scope:pulsar.proto.CommandRedeliverUnacknowledgedMessages)
	}

}
