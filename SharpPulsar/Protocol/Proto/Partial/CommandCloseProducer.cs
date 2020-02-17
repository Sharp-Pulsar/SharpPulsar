using DotNetty.Common;
using Google.Protobuf;
using System;
using SharpPulsar.Utility.Protobuf;
using static SharpPulsar.Utility.Protobuf.ByteBufCodedInputStream;

namespace SharpPulsar.Protocol.Proto
{
	public sealed partial class CommandCloseProducer : ByteBufCodedOutputStream.ByteBufGeneratedMessage
	{
		// Use CommandCloseProducer.newBuilder() to construct.
		internal static ThreadLocalPool<CommandCloseProducer> _pool = new ThreadLocalPool<CommandCloseProducer>(handle => new CommandCloseProducer(handle), 1, true);

		internal ThreadLocalPool.Handle _handle;
		private CommandCloseProducer(ThreadLocalPool.Handle handle)
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

		public CommandCloseProducer(bool NoInit)
		{
		}

		
		internal static readonly CommandCloseProducer _defaultInstance;
		public static CommandCloseProducer DefaultInstance => _defaultInstance;

        public CommandCloseProducer DefaultInstanceForType => _defaultInstance;


        public void InitFields()
		{
			ProducerId = 0L;
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

				if (!HasProducerId)
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
				Output.WriteUInt64(1, (long)ProducerId);
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
		public static Builder NewBuilder(CommandCloseProducer Prototype)
		{
			return NewBuilder().MergeFrom(Prototype);
		}
		public Builder ToBuilder()
		{
			return NewBuilder(this);
		}

		public sealed class Builder : ByteBufMessageBuilder
		{
			// Construct using org.apache.pulsar.common.api.proto.CommandCloseProducer.newBuilder()
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
				RequestId_ = 0L;
				BitField0_ = (BitField0_ & ~0x00000002);
				return this;
			}

			public Builder Clone()
			{
				return Create().MergeFrom(BuildPartial());
			}

			public CommandCloseProducer DefaultInstanceForType => CommandCloseProducer.DefaultInstance;

            public CommandCloseProducer Build()
			{
				CommandCloseProducer Result = BuildPartial();
				if (!Result.Initialized)
				{
					throw new NullReferenceException($"{Result.GetType().Name} not initialized");
				}
				return Result;
			}

			
			public CommandCloseProducer BuildParsed()
			{
				CommandCloseProducer Result = BuildPartial();
				if (!Result.Initialized)
				{
					throw new NullReferenceException($"{Result.GetType().Name} not initialized");
				}
				return Result;
			}

			public CommandCloseProducer BuildPartial()
			{
				CommandCloseProducer Result = CommandCloseProducer._pool.Take();
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
				Result.RequestId = (ulong)RequestId_;
				Result._hasBits0 = ToBitField0_;
				return Result;
			}

			public Builder MergeFrom(CommandCloseProducer Other)
			{
				if (Other == CommandCloseProducer.DefaultInstance)
				{
					return this;
				}
				if (Other.HasProducerId)
				{
					SetProducerId((long)Other.ProducerId);
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
					if (!HasProducerId())
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
								ProducerId_ = input.ReadUInt64();
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

			// required uint64 producer_id = 1;
			internal long ProducerId_;
			public bool HasProducerId()
			{
				return ((BitField0_ & 0x00000001) == 0x00000001);
			}
			public long ProducerId => ProducerId_;

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

			// @@protoc_insertion_point(builder_scope:pulsar.proto.CommandCloseProducer)
		}

		static CommandCloseProducer()
		{
			_defaultInstance = new CommandCloseProducer(true);
			_defaultInstance.InitFields();
		}

		// @@protoc_insertion_point(class_scope:pulsar.proto.CommandCloseProducer)
	}

}
