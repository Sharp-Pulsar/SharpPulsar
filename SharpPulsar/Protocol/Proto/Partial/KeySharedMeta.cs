using DotNetty.Common;
using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.Linq;
using SharpPulsar.Utility.Protobuf;
using static SharpPulsar.Utility.Protobuf.ByteBufCodedInputStream;

namespace SharpPulsar.Protocol.Proto
{
	public partial class KeySharedMeta : ByteBufCodedOutputStream.ByteBufGeneratedMessage
	{
		// Use KeySharedMeta.newBuilder() to construct.
		internal static ThreadLocalPool<KeySharedMeta> _pool = new ThreadLocalPool<KeySharedMeta>(handle => new KeySharedMeta(handle), 1, true);

		internal ThreadLocalPool.Handle _handle;
		private KeySharedMeta(ThreadLocalPool.Handle handle)
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

		public KeySharedMeta(bool NoInit)
		{
		}

		
		internal static readonly KeySharedMeta _defaultInstance;
		public static KeySharedMeta DefaultInstance => _defaultInstance;

        public KeySharedMeta DefaultInstanceForType => _defaultInstance;

        // repeated .pulsar.proto.IntRange hashRanges = 3;
		public const int HashrangesFieldNumber = 3;
		internal IList<IntRange> HashRanges_;
		public IList<IntRange> HashRangesList => HashRanges_;

        public int HashRangesCount => HashRanges_.Count;

        public IntRange GetHashRanges(int Index)
		{
			return HashRanges_[Index];
		}
		
		public void InitFields()
		{
			KeySharedMode = KeySharedMode.AutoSplit;
			HashRanges_ = new List<IntRange>();
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

				if (!HasKeySharedMode)
				{
					MemoizedIsInitialized = 0;
					return false;
				}
				for (int I = 0; I < HashRangesCount; I++)
				{
					if (!GetHashRanges(I).IsInitialized())
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
				Output.WriteEnum(1, (int)KeySharedMode);
			}
			for (int I = 0; I < HashRanges_.Count; I++)
			{
				Output.WriteMessage(3, HashRanges_[I]);
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
		public static Builder NewBuilder(KeySharedMeta Prototype)
		{
			return NewBuilder().MergeFrom(Prototype);
		}
		public Builder ToBuilder()
		{
			return NewBuilder(this);
		}

		public sealed class Builder : ByteBufMessageBuilder
        {
			// Construct using org.apache.pulsar.common.api.proto.KeySharedMeta.newBuilder()

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
				KeySharedMode_ =KeySharedMode.AutoSplit;
				BitField0_ = (BitField0_ & ~0x00000001);
				HashRanges_ = new List<IntRange>();
				BitField0_ = (BitField0_ & ~0x00000002);
				return this;
			}

			public Builder Clone()
			{
				return Create().MergeFrom(BuildPartial());
			}

			public KeySharedMeta DefaultInstanceForType => KeySharedMeta.DefaultInstance;

            public KeySharedMeta Build()
			{
				KeySharedMeta Result = BuildPartial();
				
				return Result;
			}

			
			public KeySharedMeta BuildParsed()
			{
				KeySharedMeta Result = BuildPartial();
				
				return Result;
			}

			public KeySharedMeta BuildPartial()
			{
				KeySharedMeta Result =KeySharedMeta._pool.Take();
				int FromBitField0_ = BitField0_;
				int ToBitField0_ = 0;
				if (((FromBitField0_ & 0x00000001) == 0x00000001))
				{
					ToBitField0_ |= 0x00000001;
				}
				Result.KeySharedMode = KeySharedMode_;
				if (((BitField0_ & 0x00000002) == 0x00000002))
				{
					HashRanges_ = new List<IntRange>(HashRanges_);
					BitField0_ = (BitField0_ & ~0x00000002);
				}
				Result.HashRanges_ = HashRanges_;
				Result._hasBits0 = ToBitField0_;
				return Result;
			}

			public Builder MergeFrom(KeySharedMeta Other)
			{
				if (Other ==KeySharedMeta.DefaultInstance)
				{
					return this;
				}
				if (Other.HasKeySharedMode)
				{
					SetKeySharedMode(Other.KeySharedMode);
				}
				if (Other.HashRanges_.Count > 0)
				{
					if (HashRanges_.Count == 0)
					{
						HashRanges_ = Other.HashRanges_;
						BitField0_ = (BitField0_ & ~0x00000002);
					}
					else
					{
						EnsureHashRangesIsMutable();
						((List<IntRange>)HashRanges_).AddRange(Other.HashRanges_);
					}

				}
				return this;
			}

			public bool Initialized
			{
				get
				{
					if (!HasKeySharedMode())
					{

						return false;
					}
					for (int I = 0; I < HashRangesCount; I++)
					{
						if (!GetHashRanges(I).IsInitialized())
						{

							return false;
						}
					}
					return true;
				}
			}
			public ByteBufMessageBuilder MergeFrom(ByteBufCodedInputStream Input, ExtensionRegistry ExtensionRegistry)
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
								int RawValue = Input.ReadEnum();
								KeySharedMode Value = Enum.GetValues(typeof(KeySharedMode)).Cast<KeySharedMode>().ToList()[RawValue];
								if (Value != null)
								{
									BitField0_ |= 0x00000001;
									KeySharedMode_ = Value;
								}
								break;
							}
						case 26:
							{
								IntRange.Builder SubBuilder =IntRange.NewBuilder();
								Input.ReadMessage(SubBuilder, ExtensionRegistry);
								AddHashRanges(SubBuilder.BuildPartial());
								break;
							}
					}
				}
			}

			internal int BitField0_;

			// required .pulsar.proto.KeySharedMode keySharedMode = 1;
			internal KeySharedMode KeySharedMode_ = KeySharedMode.AutoSplit;
			public bool HasKeySharedMode()
			{
				return ((BitField0_ & 0x00000001) == 0x00000001);
			}
			public KeySharedMode KeySharedMode => KeySharedMode_;

            public Builder SetKeySharedMode(KeySharedMode Value)
			{
				if (Value == null)
				{
					throw new NullReferenceException();
				}
				BitField0_ |= 0x00000001;
				KeySharedMode_ = Value;

				return this;
			}
			public Builder ClearKeySharedMode()
			{
				BitField0_ = (BitField0_ & ~0x00000001);
				KeySharedMode_ =KeySharedMode.AutoSplit;

				return this;
			}

			// repeated .pulsar.proto.IntRange hashRanges = 3;
			internal IList<IntRange> HashRanges_ = new List<IntRange>();
			public void EnsureHashRangesIsMutable()
			{
				if (!((BitField0_ & 0x00000002) == 0x00000002))
				{
					HashRanges_ = new List<IntRange>(HashRanges_);
					BitField0_ |= 0x00000002;
				}
			}

			public IList<IntRange> HashRangesList => new List<IntRange>(HashRanges_);

            public int HashRangesCount => HashRanges_.Count;

            public IntRange GetHashRanges(int Index)
			{
				return HashRanges_[Index];
			}
			public Builder SetHashRanges(int Index,IntRange Value)
			{
				if (Value == null)
				{
					throw new NullReferenceException();
				}
				EnsureHashRangesIsMutable();
				HashRanges_[Index] = Value;

				return this;
			}
			public Builder SetHashRanges(int Index, IntRange.Builder BuilderForValue)
			{
				EnsureHashRangesIsMutable();
				HashRanges_[Index] = BuilderForValue.Build();

				return this;
			}
			public Builder AddHashRanges(IntRange Value)
			{
				if (Value == null)
				{
					throw new NullReferenceException();
				}
				EnsureHashRangesIsMutable();
				HashRanges_.Add(Value);

				return this;
			}
			public Builder AddHashRanges(int Index,IntRange Value)
			{
				if (Value == null)
				{
					throw new NullReferenceException();
				}
				EnsureHashRangesIsMutable();
				HashRanges_.Insert(Index, Value);

				return this;
			}
			public Builder AddHashRanges(IntRange.Builder BuilderForValue)
			{
				EnsureHashRangesIsMutable();
				HashRanges_.Add(BuilderForValue.Build());

				return this;
			}
			public Builder AddHashRanges(int Index,IntRange.Builder BuilderForValue)
			{
				EnsureHashRangesIsMutable();
				HashRanges_.Insert(Index, BuilderForValue.Build());

				return this;
			}
			public Builder AddAllHashRanges(IEnumerable<IntRange> Values)
			{
				EnsureHashRangesIsMutable();
				Values.ToList().ForEach(x => HashRanges_.Add(x));

				return this;
			}
			public Builder ClearHashRanges()
			{
				HashRanges_.Clear();
				BitField0_ = (BitField0_ & ~0x00000002);

				return this;
			}
			public Builder RemoveHashRanges(int Index)
			{
				EnsureHashRangesIsMutable();
				HashRanges_.RemoveAt(Index);

				return this;
			}


			// @@protoc_insertion_point(builder_scope:pulsar.proto.KeySharedMeta)
		}

		static KeySharedMeta()
		{
			_defaultInstance = new KeySharedMeta(true);
			_defaultInstance.InitFields();
		}

		// @@protoc_insertion_point(class_scope:pulsar.proto.KeySharedMeta)
	}

}
