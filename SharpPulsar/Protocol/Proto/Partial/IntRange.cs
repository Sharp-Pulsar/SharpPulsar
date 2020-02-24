using DotNetty.Common;
using Google.Protobuf;
using System;
using SharpPulsar.Utility.Protobuf;

namespace SharpPulsar.Protocol.Proto
{
	public partial class IntRange : ByteBufCodedOutputStream.ByteBufGeneratedMessage
	{
		// Use IntRange.newBuilder() to construct.
		internal static ThreadLocalPool<IntRange> _pool = new ThreadLocalPool<IntRange>(handle => new IntRange(handle), 1, true);

		internal ThreadLocalPool.Handle _handle;
		private IntRange(ThreadLocalPool.Handle handle)
		{
			_handle = handle;
		}

		public void Recycle()
		{
			this.InitFields();
			this.MemoizedIsInitialized = -1;
			this._hasBits0 = 0;
			this.MemoizedSerializedSize = -1;
            _handle?.Release(this);
        }

		public IntRange(bool NoInit)
		{
		}

		internal static readonly IntRange _defaultInstance;
		public static IntRange DefaultInstance => _defaultInstance;

        public IntRange DefaultInstanceForType => _defaultInstance;

        public void InitFields()
		{
			Start = 0;
			End = 0;
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

				if (!HasStart)
				{
					MemoizedIsInitialized = 0;
					return false;
				}
				if (!HasEnd)
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
				Output.WriteInt32(1, Start);
			}
			if (((_hasBits0 & 0x00000002) == 0x00000002))
			{
				Output.WriteInt32(2, End);
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
		public static Builder NewBuilder(IntRange Prototype)
		{
			return NewBuilder().MergeFrom(Prototype);
		}
		public Builder ToBuilder()
		{
			return NewBuilder(this);
		}

		public sealed class Builder : ByteBufCodedInputStream.ByteBufMessageBuilder
		{
			// Construct using org.apache.pulsar.common.api.proto.IntRange.newBuilder()
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
                _handle?.Release(this);
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
				Start_ = 0;
				BitField0_ = (BitField0_ & ~0x00000001);
				End_ = 0;
				BitField0_ = (BitField0_ & ~0x00000002);
				return this;
			}

			public Builder Clone()
			{
				return Create().MergeFrom(BuildPartial());
			}

			public IntRange DefaultInstanceForType => IntRange.DefaultInstance;

            public IntRange Build()
			{
				IntRange Result = BuildPartial();
				
				return Result;
			}

			//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
			//ORIGINAL LINE: private org.apache.pulsar.common.api.proto.IntRange buildParsed() throws org.apache.pulsar.shaded.com.google.protobuf.v241.InvalidProtocolBufferException
			public IntRange BuildParsed()
			{
				IntRange Result = BuildPartial();
				
				return Result;
			}

			public IntRange BuildPartial()
			{
				IntRange Result = IntRange._pool.Take();
				int FromBitField0_ = BitField0_;
				int ToBitField0_ = 0;
				if (((FromBitField0_ & 0x00000001) == 0x00000001))
				{
					ToBitField0_ |= 0x00000001;
				}
				Result.Start = Start_;
				if (((FromBitField0_ & 0x00000002) == 0x00000002))
				{
					ToBitField0_ |= 0x00000002;
				}
				Result.End = End_;
				Result._hasBits0 = ToBitField0_;
				return Result;
			}

			public Builder MergeFrom(IntRange Other)
			{
				if (Other == DefaultInstance)
				{
					return this;
				}
				if (Other.HasStart)
				{
					SetStart(Other.Start);
				}
				if (Other.HasEnd)
				{
					SetEnd(Other.End);
				}
				return this;
			}

			public bool Initialized
			{
				get
				{
					if (!HasStart())
					{

						return false;
					}
					if (!HasEnd())
					{

						return false;
					}
					return true;
				}
			}

			public Builder MergeFrom(ByteBufCodedInputStream Input, ExtensionRegistry extensionRegistry)
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
								Start_ = Input.ReadInt32();
								break;
							}
						case 16:
							{
								BitField0_ |= 0x00000002;
								End_ = Input.ReadInt32();
								break;
							}
					}
				}
			}

			internal int BitField0_;

			// required int32 start = 1;
			internal int Start_;
			public bool HasStart()
			{
				return ((BitField0_ & 0x00000001) == 0x00000001);
			}
			public int Start => Start_;

            public Builder SetStart(int Value)
			{
				BitField0_ |= 0x00000001;
				Start_ = Value;

				return this;
			}
			public Builder ClearStart()
			{
				BitField0_ = (BitField0_ & ~0x00000001);
				Start_ = 0;

				return this;
			}

			// required int32 end = 2;
			internal int End_;
			public bool HasEnd()
			{
				return ((BitField0_ & 0x00000002) == 0x00000002);
			}
			public int End => End_;

            public Builder SetEnd(int Value)
			{
				BitField0_ |= 0x00000002;
				End_ = Value;

				return this;
			}
			public Builder ClearEnd()
			{
				BitField0_ = (BitField0_ & ~0x00000002);
				End_ = 0;

				return this;
			}

			ByteBufCodedInputStream.ByteBufMessageBuilder ByteBufCodedInputStream.ByteBufMessageBuilder.MergeFrom(ByteBufCodedInputStream input, ExtensionRegistry ext)
			{
				throw new NotImplementedException();
			}

			// @@protoc_insertion_point(builder_scope:pulsar.proto.IntRange)
		}

		static IntRange()
		{
			_defaultInstance = new IntRange(true);
			_defaultInstance.InitFields();
		}

		// @@protoc_insertion_point(class_scope:pulsar.proto.IntRange)
	}

}
