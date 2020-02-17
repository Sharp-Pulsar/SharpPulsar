using DotNetty.Common;
using Google.Protobuf;
using System;
using SharpPulsar.Utility.Protobuf;
using static SharpPulsar.Utility.Protobuf.ByteBufCodedInputStream;

namespace SharpPulsar.Protocol.Proto
{
	public sealed partial class KeyLongValue : ByteBufCodedOutputStream.ByteBufGeneratedMessage
	{
		// Use KeyLongValue.newBuilder() to construct.
		internal static ThreadLocalPool<KeyLongValue> _pool = new ThreadLocalPool<KeyLongValue>(handle => new KeyLongValue(handle), 1, true);

		internal ThreadLocalPool.Handle _handle;
		private KeyLongValue(ThreadLocalPool.Handle handle)
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

		public KeyLongValue(bool NoInit)
		{
		}

		
		internal static readonly KeyLongValue _defaultInstance;
		public static KeyLongValue DefaultInstance => _defaultInstance;

        public KeyLongValue DefaultInstanceForType => _defaultInstance;


        public void InitFields()
		{
			Key = "";
			Value = 0L;
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

				if (!HasKey)
				{
					MemoizedIsInitialized = 0;
					return false;
				}
				if (!HasValue)
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
				Output.WriteBytes(1, ByteString.CopyFromUtf8(Key));
			}
			if (((_hasBits0 & 0x00000002) == 0x00000002))
			{
				Output.WriteUInt64(2, (long)Value);
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
		public static Builder NewBuilder(KeyLongValue Prototype)
		{
			return NewBuilder().MergeFrom(Prototype);
		}
		public Builder ToBuilder()
		{
			return NewBuilder(this);
		}

		public sealed class Builder : ByteBufMessageBuilder
		{
			// Construct using org.apache.pulsar.common.api.proto.KeyLongValue.newBuilder()
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
				Key_ = "";
				BitField0_ = (BitField0_ & ~0x00000001);
				Value_ = 0L;
				BitField0_ = (BitField0_ & ~0x00000002);
				return this;
			}

			public Builder Clone()
			{
				return Create().MergeFrom(BuildPartial());
			}

			public KeyLongValue DefaultInstanceForType => KeyLongValue.DefaultInstance;

            public KeyLongValue Build()
			{
				KeyLongValue Result = BuildPartial();
				if (!Result.Initialized)
				{
					throw new NullReferenceException($"{Result.GetType().Name} not initialized");
				}
				return Result;
			}

			
			public KeyLongValue BuildParsed()
			{
				KeyLongValue Result = BuildPartial();
				if (!Result.Initialized)
				{
					throw new NullReferenceException($"{Result.GetType().Name} not initialized");
				}
				return Result;
			}

			public KeyLongValue BuildPartial()
			{
				KeyLongValue Result = KeyLongValue._pool.Take();
				int FromBitField0_ = BitField0_;
				int ToBitField0_ = 0;
				if (((FromBitField0_ & 0x00000001) == 0x00000001))
				{
					ToBitField0_ |= 0x00000001;
				}
				Result.Key = Key_.ToString();
				if (((FromBitField0_ & 0x00000002) == 0x00000002))
				{
					ToBitField0_ |= 0x00000002;
				}
				Result.Value = (ulong)Value_;
				Result._hasBits0 = ToBitField0_;
				return Result;
			}

			public Builder MergeFrom(KeyLongValue Other)
			{
				if (Other == KeyLongValue.DefaultInstance)
				{
					return this;
				}
				if (Other.HasKey)
				{
					SetKey(Other.Key);
				}
				if (Other.HasValue)
				{
					SetValue((long)Other.Value);
				}
				return this;
			}

			public bool Initialized
			{
				get
				{
					if (!HasKey())
					{

						return false;
					}
					if (!HasValue())
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
						case 10:
							{
								BitField0_ |= 0x00000001;
								Key_ = input.ReadBytes();
								break;
							}
						case 16:
							{
								BitField0_ |= 0x00000002;
								Value_ = input.ReadUInt64();
								break;
							}
					}
				}
			}

			internal int BitField0_;

			// required string key = 1;
			internal object Key_ = "";
			public bool HasKey()
			{
				return ((BitField0_ & 0x00000001) == 0x00000001);
			}
			public string GetKey()
			{
				object Ref = Key_;
				if (!(Ref is string))
				{
					string S = ((ByteString)Ref).ToStringUtf8();
					Key_ = S;
					return S;
				}
				else
				{
					return (string)Ref;
				}
			}
			public Builder SetKey(string Value)
			{
				if (string.ReferenceEquals(Value, null))
				{
					throw new NullReferenceException();
				}
				BitField0_ |= 0x00000001;
				Key_ = Value;

				return this;
			}
			public Builder ClearKey()
			{
				BitField0_ = (BitField0_ & ~0x00000001);
				Key_ = DefaultInstance.Key;

				return this;
			}
			public void SetKey(ByteString Value)
			{
				BitField0_ |= 0x00000001;
				Key_ = Value;

			}

			// required uint64 value = 2;
			internal long Value_;
			public bool HasValue()
			{
				return ((BitField0_ & 0x00000002) == 0x00000002);
			}
			public long Value => Value_;

            public Builder SetValue(long Value)
			{
				BitField0_ |= 0x00000002;
				Value_ = Value;

				return this;
			}
			public Builder ClearValue()
			{
				BitField0_ = (BitField0_ & ~0x00000002);
				Value_ = 0L;

				return this;
			}

			// @@protoc_insertion_point(builder_scope:pulsar.proto.KeyLongValue)
		}

		static KeyLongValue()
		{
			_defaultInstance = new KeyLongValue(true);
			_defaultInstance.InitFields();
		}

		// @@protoc_insertion_point(class_scope:pulsar.proto.KeyLongValue)
	}

}
