using DotNetty.Common;
using Google.Protobuf;
using System;
using SharpPulsar.Utility.Protobuf;
using static SharpPulsar.Utility.Protobuf.ByteBufCodedInputStream;

namespace SharpPulsar.Protocol.Proto
{
	public sealed partial class CommandSuccess : ByteBufCodedOutputStream.ByteBufGeneratedMessage
	{
		// Use CommandSuccess.newBuilder() to construct.
		internal static ThreadLocalPool<CommandSuccess> _pool = new ThreadLocalPool<CommandSuccess>(handle => new CommandSuccess(handle), 1, true);

		internal ThreadLocalPool.Handle _handle;
		private CommandSuccess(ThreadLocalPool.Handle handle)
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

		public CommandSuccess(bool NoInit)
		{
		}

		internal static readonly CommandSuccess _defaultInstance;
		public static CommandSuccess DefaultInstance => _defaultInstance;

        public CommandSuccess DefaultInstanceForType => _defaultInstance;

        public void InitFields()
		{
			RequestId = 0L;
			Schema = Schema.DefaultInstance;
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
				if (HasSchema)
				{
					if (!Schema.Initialized)
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
				Output.WriteUInt64(1, (long)RequestId);
			}
			if (((_hasBits0 & 0x00000002) == 0x00000002))
			{
				Output.WriteMessage(2, Schema);
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
		public static Builder NewBuilder(CommandSuccess Prototype)
		{
			return NewBuilder().MergeFrom(Prototype);
		}
		public Builder ToBuilder()
		{
			return NewBuilder(this);
		}

		public sealed class Builder : ByteBufMessageBuilder
		{
			// Construct using org.apache.pulsar.common.api.proto.CommandSuccess.newBuilder()
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
				Schema_ = Schema.DefaultInstance;
				BitField0_ = (BitField0_ & ~0x00000002);
				return this;
			}

			public Builder Clone()
			{
				return Create().MergeFrom(BuildPartial());
			}

			public CommandSuccess DefaultInstanceForType => CommandSuccess.DefaultInstance;

            public CommandSuccess Build()
			{
				CommandSuccess Result = BuildPartial();
				if (!Result.Initialized)
				{
					throw new NullReferenceException($"{Result.GetType().Name} not initialized");
				}
				return Result;
			}

			public CommandSuccess BuildParsed()
			{
				CommandSuccess Result = BuildPartial();
				if (!Result.Initialized)
				{
					throw new NullReferenceException($"{Result.GetType().Name} not initialized");
				}
				return Result;
			}

			public CommandSuccess BuildPartial()
			{
				CommandSuccess Result = CommandSuccess._pool.Take();
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
				Result.Schema = Schema_;
				Result._hasBits0 = ToBitField0_;
				return Result;
			}

			public Builder MergeFrom(CommandSuccess Other)
			{
				if (Other == CommandSuccess.DefaultInstance)
				{
					return this;
				}
				if (Other.HasRequestId)
				{
					SetRequestId((long)Other.RequestId);
				}
				if (Other.HasSchema)
				{
					MergeSchema(Other.Schema);
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
					if (HasSchema())
					{
						if (!GetSchema().Initialized)
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
								RequestId_ = input.ReadUInt64();
								break;
							}
						case 18:
							{
								Schema.Builder SubBuilder = Schema.NewBuilder();
								if (HasSchema())
								{
									SubBuilder.MergeFrom(GetSchema());
								}
								input.ReadMessage(SubBuilder, extensionRegistry);
								SetSchema(SubBuilder.BuildPartial());
								SubBuilder.Recycle();
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

			// optional .pulsar.proto.Schema schema = 2;
			internal Schema Schema_ = Schema.DefaultInstance;
			public bool HasSchema()
			{
				return ((BitField0_ & 0x00000002) == 0x00000002);
			}
			public Schema GetSchema()
			{
				return Schema_;
			}
			public Builder SetSchema(Schema Value)
			{
				if (Value == null)
				{
					throw new NullReferenceException();
				}
				Schema_ = Value;

				BitField0_ |= 0x00000002;
				return this;
			}
			public Builder SetSchema(Schema.Builder BuilderForValue)
			{
				Schema_ = BuilderForValue.Build();

				BitField0_ |= 0x00000002;
				return this;
			}
			public Builder MergeSchema(Schema Value)
			{
				if (((BitField0_ & 0x00000002) == 0x00000002) && Schema_ != Schema.DefaultInstance)
				{
					Schema_ = Schema.NewBuilder(Schema_).MergeFrom(Value).BuildPartial();
				}
				else
				{
					Schema_ = Value;
				}

				BitField0_ |= 0x00000002;
				return this;
			}
			public Builder ClearSchema()
			{
				Schema_ = Schema.DefaultInstance;

				BitField0_ = (BitField0_ & ~0x00000002);
				return this;
			}

			// @@protoc_insertion_point(builder_scope:pulsar.proto.CommandSuccess)
		}

		static CommandSuccess()
		{
			_defaultInstance = new CommandSuccess(true);
			_defaultInstance.InitFields();
		}

		// @@protoc_insertion_point(class_scope:pulsar.proto.CommandSuccess)
	}

}
