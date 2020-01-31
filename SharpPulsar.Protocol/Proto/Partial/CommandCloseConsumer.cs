using DotNetty.Common;
using Google.Protobuf;
using SharpPulsar.Util.Protobuf;
using System;
using static SharpPulsar.Util.Protobuf.ByteBufCodedInputStream;

namespace SharpPulsar.Protocol.Proto
{
	public sealed partial class CommandCloseConsumer : ByteBufCodedOutputStream.ByteBufGeneratedMessage
	{
		// Use CommandCloseConsumer.newBuilder() to construct.
		internal static ThreadLocalPool<CommandCloseConsumer> _pool = new ThreadLocalPool<CommandCloseConsumer>(handle => new CommandCloseConsumer(handle), 1, true);

		internal ThreadLocalPool.Handle _handle;
		private CommandCloseConsumer(ThreadLocalPool.Handle handle)
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

		public CommandCloseConsumer(bool NoInit)
		{
		}

		
		internal static readonly CommandCloseConsumer _defaultInstance;
		public static CommandCloseConsumer DefaultInstance
		{
			get
			{
				return _defaultInstance;
			}
		}

		public CommandCloseConsumer DefaultInstanceForType
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
		public static Builder NewBuilder(CommandCloseConsumer Prototype)
		{
			return NewBuilder().MergeFrom(Prototype);
		}
		public Builder ToBuilder()
		{
			return NewBuilder(this);
		}

		public sealed class Builder : ByteBufCodedInputStream.ByteBufMessageBuilder
		{
			// Construct using org.apache.pulsar.common.api.proto.CommandCloseConsumer.newBuilder()
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
				return this;
			}

			public Builder Clone()
			{
				return Create().MergeFrom(BuildPartial());
			}

			public CommandCloseConsumer DefaultInstanceForType
			{
				get
				{
					return CommandCloseConsumer.DefaultInstance;
				}
			}

			public CommandCloseConsumer Build()
			{
				CommandCloseConsumer Result = BuildPartial();
				if (!Result.Initialized)
				{
					throw new NullReferenceException($"{Result.GetType().Name} not initialized");
				}
				return Result;
			}

			
			public CommandCloseConsumer BuildParsed()
			{
				CommandCloseConsumer Result = BuildPartial();
				if (!Result.Initialized)
				{
					throw new NullReferenceException($"{Result.GetType().Name} not initialized");
				}
				return Result;
			}

			public CommandCloseConsumer BuildPartial()
			{
				CommandCloseConsumer Result = CommandCloseConsumer._pool.Take();
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
				Result._hasBits0 = ToBitField0_;
				return Result;
			}

			public Builder MergeFrom(CommandCloseConsumer Other)
			{
				if (Other == CommandCloseConsumer.DefaultInstance)
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
								ConsumerId_ = Input.ReadUInt64();
								break;
							}
						case 16:
							{
								BitField0_ |= 0x00000002;
								RequestId_ = Input.ReadUInt64();
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

			// @@protoc_insertion_point(builder_scope:pulsar.proto.CommandCloseConsumer)
		}

		static CommandCloseConsumer()
		{
			_defaultInstance = new CommandCloseConsumer(true);
			_defaultInstance.InitFields();
		}

		// @@protoc_insertion_point(class_scope:pulsar.proto.CommandCloseConsumer)
	}

}
