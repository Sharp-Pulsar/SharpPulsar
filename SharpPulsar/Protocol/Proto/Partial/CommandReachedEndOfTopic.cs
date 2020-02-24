using DotNetty.Common;
using Google.Protobuf;
using System;
using SharpPulsar.Utility.Protobuf;
using static SharpPulsar.Utility.Protobuf.ByteBufCodedInputStream;

namespace SharpPulsar.Protocol.Proto
{
	public sealed partial class CommandReachedEndOfTopic : ByteBufCodedOutputStream.ByteBufGeneratedMessage
	{
		// Use CommandReachedEndOfTopic.newBuilder() to construct.
		internal static ThreadLocalPool<CommandReachedEndOfTopic> _pool = new ThreadLocalPool<CommandReachedEndOfTopic>(handle => new CommandReachedEndOfTopic(handle), 1, true);

		internal ThreadLocalPool.Handle _handle;
		private CommandReachedEndOfTopic(ThreadLocalPool.Handle handle)
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

		public CommandReachedEndOfTopic(bool NoInit)
		{
		}

		
		internal static readonly CommandReachedEndOfTopic _defaultInstance;
		public static CommandReachedEndOfTopic DefaultInstance => _defaultInstance;

        public CommandReachedEndOfTopic DefaultInstanceForType => _defaultInstance;

        public void InitFields()
		{
			ConsumerId = 0L;
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
		public static Builder NewBuilder(CommandReachedEndOfTopic Prototype)
		{
			return NewBuilder().MergeFrom(Prototype);
		}
		public Builder ToBuilder()
		{
			return NewBuilder(this);
		}

		public sealed class Builder : ByteBufMessageBuilder
		{
			// Construct using org.apache.pulsar.common.api.proto.CommandReachedEndOfTopic.newBuilder()
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
				return this;
			}

			public Builder Clone()
			{
				return Create().MergeFrom(BuildPartial());
			}

			public CommandReachedEndOfTopic DefaultInstanceForType => DefaultInstance;

            public CommandReachedEndOfTopic Build()
			{
				CommandReachedEndOfTopic Result = BuildPartial();
				
				return Result;
			}

			
			public CommandReachedEndOfTopic BuildParsed()
			{
				CommandReachedEndOfTopic Result = BuildPartial();
				
				return Result;
			}

			public CommandReachedEndOfTopic BuildPartial()
			{
				CommandReachedEndOfTopic Result = CommandReachedEndOfTopic._pool.Take();
				int FromBitField0_ = BitField0_;
				int ToBitField0_ = 0;
				if (((FromBitField0_ & 0x00000001) == 0x00000001))
				{
					ToBitField0_ |= 0x00000001;
				}
				Result.ConsumerId = (ulong)ConsumerId_;
				Result._hasBits0 = ToBitField0_;
				return Result;
			}

			public Builder MergeFrom(CommandReachedEndOfTopic Other)
			{
				if (Other == CommandReachedEndOfTopic.DefaultInstance)
				{
					return this;
				}
				if (Other.HasConsumerId)
				{
					SetConsumerId((long)Other.ConsumerId);
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

			// @@protoc_insertion_point(builder_scope:pulsar.proto.CommandReachedEndOfTopic)
		}

		static CommandReachedEndOfTopic()
		{
			_defaultInstance = new CommandReachedEndOfTopic(true);
			_defaultInstance.InitFields();
		}

		// @@protoc_insertion_point(class_scope:pulsar.proto.CommandReachedEndOfTopic)
	}

}
