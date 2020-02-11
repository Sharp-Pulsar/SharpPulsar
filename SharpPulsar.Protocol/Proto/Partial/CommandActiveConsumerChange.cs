using DotNetty.Common;
using Google.Protobuf;
using SharpPulsar.Util.Protobuf;
using System;
using System.Collections.Generic;
using System.Text;
using static SharpPulsar.Util.Protobuf.ByteBufCodedInputStream;

namespace SharpPulsar.Protocol.Proto
{
	public sealed partial class CommandActiveConsumerChange : ByteBufCodedOutputStream.ByteBufGeneratedMessage
	{
		// Use CommandActiveConsumerChange.newBuilder() to construct.
		internal static ThreadLocalPool<CommandActiveConsumerChange> _pool = new ThreadLocalPool<CommandActiveConsumerChange>(handle => new CommandActiveConsumerChange(handle), 1, true);

		internal ThreadLocalPool.Handle _handle;
		private CommandActiveConsumerChange(ThreadLocalPool.Handle handle)
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

		public CommandActiveConsumerChange(bool NoInit)
		{
		}

		
		internal static readonly CommandActiveConsumerChange _defaultInstance;
		public static CommandActiveConsumerChange DefaultInstance => _defaultInstance;

        public CommandActiveConsumerChange DefaultInstanceForType => _defaultInstance;


        public void InitFields()
		{
			ConsumerId = 0L;
			IsActive = false;
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
			if (((_hasBits0 & 0x00000002) == 0x00000002))
			{
				Output.WriteBool(2, IsActive);
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
		public static Builder NewBuilder(CommandActiveConsumerChange Prototype)
		{
			return NewBuilder().MergeFrom(Prototype);
		}
		public Builder ToBuilder()
		{
			return NewBuilder(this);
		}

		public sealed class Builder : ByteBufMessageBuilder
		{
			// Construct using org.apache.pulsar.common.api.proto.CommandActiveConsumerChange.newBuilder()
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
				IsActive_ = false;
				BitField0_ = (BitField0_ & ~0x00000002);
				return this;
			}

			public Builder Clone()
			{
				return Create().MergeFrom(BuildPartial());
			}

			public CommandActiveConsumerChange DefaultInstanceForType => DefaultInstance;

            public CommandActiveConsumerChange Build()
			{
				CommandActiveConsumerChange Result = BuildPartial();
				if (!Result.Initialized)
				{
					throw new NullReferenceException($"{Result.GetType().Name} not initialized");
				}
				return Result;
			}

			
			public CommandActiveConsumerChange BuildParsed()
			{
				CommandActiveConsumerChange Result = BuildPartial();
				if (!Result.Initialized)
				{
					throw new NullReferenceException($"{Result.GetType().Name} not initialized");
				}
				return Result;
			}

			public CommandActiveConsumerChange BuildPartial()
			{
				CommandActiveConsumerChange Result = CommandActiveConsumerChange._pool.Take();
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
				Result.IsActive = IsActive_;
				Result._hasBits0 = ToBitField0_;
				return Result;
			}

			public Builder MergeFrom(CommandActiveConsumerChange Other)
			{
				if (Other == DefaultInstance)
				{
					return this;
				}
				if (Other.HasConsumerId)
				{
					SetConsumerId((long)Other.ConsumerId);
				}
				if (Other.HasIsActive)
				{
					SetIsActive(Other.IsActive);
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
						case 16:
							{
								BitField0_ |= 0x00000002;
								IsActive_ = input.ReadBool();
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

			// optional bool is_active = 2 [default = false];
			internal bool IsActive_;
			public bool HasIsActive()
			{
				return ((BitField0_ & 0x00000002) == 0x00000002);
			}
			public bool IsActive => IsActive_;

            public Builder SetIsActive(bool Value)
			{
				BitField0_ |= 0x00000002;
				IsActive_ = Value;

				return this;
			}
			public Builder ClearIsActive()
			{
				BitField0_ = (BitField0_ & ~0x00000002);
				IsActive_ = false;

				return this;
			}

			// @@protoc_insertion_point(builder_scope:pulsar.proto.CommandActiveConsumerChange)
		}

		static CommandActiveConsumerChange()
		{
			_defaultInstance = new CommandActiveConsumerChange(true);
			_defaultInstance.InitFields();
		}

		// @@protoc_insertion_point(class_scope:pulsar.proto.CommandActiveConsumerChange)
	}

}
