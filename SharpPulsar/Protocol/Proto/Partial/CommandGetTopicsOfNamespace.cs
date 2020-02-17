using DotNetty.Common;
using Google.Protobuf;
using System;
using System.Linq;
using SharpPulsar.Utility.Protobuf;
using static SharpPulsar.Utility.Protobuf.ByteBufCodedInputStream;

namespace SharpPulsar.Protocol.Proto
{
	public sealed partial class CommandGetTopicsOfNamespace : ByteBufCodedOutputStream.ByteBufGeneratedMessage
	{
		// Use CommandGetTopicsOfNamespace.newBuilder() to construct.
		internal static ThreadLocalPool<CommandGetTopicsOfNamespace> _pool = new ThreadLocalPool<CommandGetTopicsOfNamespace>(handle => new CommandGetTopicsOfNamespace(handle), 1, true);

		internal ThreadLocalPool.Handle _handle;
		private CommandGetTopicsOfNamespace(ThreadLocalPool.Handle handle)
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

		public CommandGetTopicsOfNamespace(bool NoInit)
		{
		}

		
		internal static readonly CommandGetTopicsOfNamespace _defaultInstance;
		public static CommandGetTopicsOfNamespace DefaultInstance => _defaultInstance;

        public CommandGetTopicsOfNamespace DefaultInstanceForType => _defaultInstance;

        public void InitFields()
		{
			RequestId = 0L;
			Namespace = "";
			Mode = Types.Mode.Persistent;
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
				if (!HasNamespace)
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
				Output.WriteUInt64(1, (long)RequestId);
			}
			if (((_hasBits0 & 0x00000002) == 0x00000002))
			{
				Output.WriteBytes(2, ByteString.CopyFromUtf8(Namespace));
			}
			if (((_hasBits0 & 0x00000004) == 0x00000004))
			{
				Output.WriteEnum(3, (int)Mode);
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
		public static Builder NewBuilder(CommandGetTopicsOfNamespace Prototype)
		{
			return NewBuilder().MergeFrom(Prototype);
		}
		public Builder ToBuilder()
		{
			return NewBuilder(this);
		}

		public sealed class Builder : ByteBufMessageBuilder
		{
			// Construct using org.apache.pulsar.common.api.proto.CommandGetTopicsOfNamespace.newBuilder()
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
				Namespace_ = "";
				BitField0_ = (BitField0_ & ~0x00000002);
				Mode_ =Types.Mode.Persistent;
				BitField0_ = (BitField0_ & ~0x00000004);
				return this;
			}

			public Builder Clone()
			{
				return Create().MergeFrom(BuildPartial());
			}

			public CommandGetTopicsOfNamespace DefaultInstanceForType => DefaultInstance;

            public CommandGetTopicsOfNamespace Build()
			{
				CommandGetTopicsOfNamespace Result = BuildPartial();
				if (!Result.Initialized)
				{
					throw new NullReferenceException($"{Result.GetType().Name} not initialized");
				}
				return Result;
			}

			public CommandGetTopicsOfNamespace BuildParsed()
			{
				CommandGetTopicsOfNamespace Result = BuildPartial();
				if (!Result.Initialized)
				{
					throw new NullReferenceException($"{Result.GetType().Name} not initialized");
				}
				return Result;
			}

			public CommandGetTopicsOfNamespace BuildPartial()
			{
				CommandGetTopicsOfNamespace Result = CommandGetTopicsOfNamespace._pool.Take();
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
				Result.Namespace = Namespace_.ToString();
				if (((FromBitField0_ & 0x00000004) == 0x00000004))
				{
					ToBitField0_ |= 0x00000004;
				}
				Result.Mode = Mode_;
				Result._hasBits0 = ToBitField0_;
				return Result;
			}

			public Builder MergeFrom(CommandGetTopicsOfNamespace Other)
			{
				if (Other == DefaultInstance)
				{
					return this;
				}
				if (Other.HasRequestId)
				{
					SetRequestId((long)Other.RequestId);
				}
				if (Other.HasNamespace)
				{
					SetNamespace(Other.Namespace);
				}
				if (Other.HasMode)
				{
					SetMode(Other.Mode);
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
					if (!HasNamespace())
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
								RequestId_ = input.ReadUInt64();
								break;
							}
						case 18:
							{
								BitField0_ |= 0x00000002;
								Namespace_ = input.ReadBytes();
								break;
							}
						case 24:
							{
								int RawValue = input.ReadEnum();
                                Types.Mode Value = Enum.GetValues(typeof(Types.Mode)).Cast<Types.Mode>().ToList()[RawValue];
								if (Value != null)
								{
									BitField0_ |= 0x00000004;
									Mode_ = Value;
								}
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

			// required string namespace = 2;
			internal object Namespace_ = "";
			public bool HasNamespace()
			{
				return ((BitField0_ & 0x00000002) == 0x00000002);
			}
			public string GetNamespace()
			{
				object Ref = Namespace_;
				if (!(Ref is string))
				{
					string S = ((ByteString)Ref).ToStringUtf8();
					Namespace_ = S;
					return S;
				}
				else
				{
					return (string)Ref;
				}
			}
			public Builder SetNamespace(string Value)
			{
				if (string.ReferenceEquals(Value, null))
				{
					throw new NullReferenceException();
				}
				BitField0_ |= 0x00000002;
				Namespace_ = Value;

				return this;
			}
			public Builder ClearNamespace()
			{
				BitField0_ = (BitField0_ & ~0x00000002);
				Namespace_ = DefaultInstance.Namespace;

				return this;
			}
			public void SetNamespace(ByteString Value)
			{
				BitField0_ |= 0x00000002;
				Namespace_ = Value;

			}

			// optional .pulsar.proto.CommandGetTopicsOfNamespace.Mode mode = 3 [default = PERSISTENT];
			internal Types.Mode Mode_ = Types.Mode.Persistent;
			public bool HasMode()
			{
				return ((BitField0_ & 0x00000004) == 0x00000004);
			}
			public Types.Mode Mode => Mode_;

            public Builder SetMode(Types.Mode Value)
			{
				if (Value == null)
				{
					throw new NullReferenceException();
				}
				BitField0_ |= 0x00000004;
				Mode_ = Value;

				return this;
			}
			public Builder ClearMode()
			{
				BitField0_ = (BitField0_ & ~0x00000004);
				Mode_ = Types.Mode.Persistent;

				return this;
			}

			// @@protoc_insertion_point(builder_scope:pulsar.proto.CommandGetTopicsOfNamespace)
		}

		static CommandGetTopicsOfNamespace()
		{
			_defaultInstance = new CommandGetTopicsOfNamespace(true);
			_defaultInstance.InitFields();
		}

		// @@protoc_insertion_point(class_scope:pulsar.proto.CommandGetTopicsOfNamespace)
	}

}
