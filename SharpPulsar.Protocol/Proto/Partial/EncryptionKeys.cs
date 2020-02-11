using DotNetty.Common;
using Google.Protobuf;
using SharpPulsar.Util.Protobuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static SharpPulsar.Util.Protobuf.ByteBufCodedInputStream;

namespace SharpPulsar.Protocol.Proto
{
	public sealed partial class EncryptionKeys : ByteBufCodedOutputStream.ByteBufGeneratedMessage
	{
		// Use EncryptionKeys.newBuilder() to construct.
		internal static ThreadLocalPool<EncryptionKeys> _pool = new ThreadLocalPool<EncryptionKeys>(handle => new EncryptionKeys(handle), 1, true);
		internal ThreadLocalPool.Handle _handle;
		private EncryptionKeys(ThreadLocalPool.Handle handle)
		{
			_handle = handle;

		}
		public void Recycle()
		{
			InitFields();
			MemoizedIsInitialized = -1;
			BitField0_ = 0;
			MemoizedSerializedSize = -1;
			if (_handle != null)
			{
				_handle.Release(this);
			}
		}

		public EncryptionKeys(bool NoInit)
		{
		}

		internal static readonly EncryptionKeys _defaultInstance;
		public static EncryptionKeys DefaultInstance => _defaultInstance;

        public EncryptionKeys DefaultInstanceForType => _defaultInstance;

        internal int BitField0_;
		
		public void InitFields()
		{
			Key = "";
			Value = ByteString.Empty;
			Metadata.Clear();
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
				for (int I = 0; I < Metadata.Count; I++)
				{
					if (!Metadata[I].Initialized)
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
			if (((BitField0_ & 0x00000001) == 0x00000001))
			{
				Output.WriteBytes(1, ByteString.CopyFromUtf8(Key));
			}
			if (((BitField0_ & 0x00000002) == 0x00000002))
			{
				Output.WriteBytes(2, Value);
			}
			for (int I = 0; I < Metadata.Count; I++)
			{
				Output.WriteMessage(3, Metadata[I]);
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
		public static Builder NewBuilder(EncryptionKeys Prototype)
		{
			return NewBuilder().MergeFrom(Prototype);
		}
		public Builder ToBuilder()
		{
			return NewBuilder(this);
		}

		public sealed class Builder : ByteBufMessageBuilder
		{
			// Construct using org.apache.pulsar.common.api.proto.EncryptionKeys.newBuilder()
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
				Value_ = ByteString.Empty;
				BitField0_ = (BitField0_ & ~0x00000002);
				Metadata_ = new List<KeyValue>();
				BitField0_ = (BitField0_ & ~0x00000004);
				return this;
			}

			public Builder Clone()
			{
				return Create().MergeFrom(BuildPartial());
			}

			public EncryptionKeys DefaultInstanceForType => DefaultInstance;

            public EncryptionKeys Build()
			{
				EncryptionKeys Result = BuildPartial();
				if (!Result.Initialized)
				{
					throw new NullReferenceException($"{Result.GetType().Name} not initialized");
				}
				return Result;
			}

			public EncryptionKeys BuildParsed()
			{
				EncryptionKeys Result = BuildPartial();
				if (!Result.Initialized)
				{
					throw new NullReferenceException($"{Result.GetType().Name} not initialized");
				}
				return Result;
			}

			public EncryptionKeys BuildPartial()
			{
				EncryptionKeys Result = EncryptionKeys._pool.Take();
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
				Result.Value = Value_;
				if (((BitField0_ & 0x00000004) == 0x00000004))
				{
					Metadata_ = new List<KeyValue>(Metadata_);
					BitField0_ = (BitField0_ & ~0x00000004);
				}
				Result.Metadata.AddRange(Metadata_);
				Result.BitField0_ = ToBitField0_;
				return Result;
			}

			public Builder MergeFrom(EncryptionKeys Other)
			{
				if (Other == DefaultInstance)
				{
					return this;
				}
				if (Other.HasKey)
				{
					SetKey(Other.Key);
				}
				if (Other.HasValue)
				{
					SetValue(Other.Value);
				}
				if (Other.Metadata.Count > 0)
				{
					if (Metadata_.Count == 0)
					{
						Metadata_ = Other.Metadata;
						BitField0_ = (BitField0_ & ~0x00000004);
					}
					else
					{
						EnsureMetadataIsMutable();
						((List<KeyValue>)Metadata_).AddRange(Other.Metadata);
					}

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
					for (int I = 0; I < MetadataCount; I++)
					{
						if (!GetMetadata(I).Initialized)
						{

							return false;
						}
					}
					return true;
				}
			}
			public ByteBufMessageBuilder MergeFrom(ByteBufCodedInputStream input, ExtensionRegistry ext)
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
						case 18:
							{
								BitField0_ |= 0x00000002;
								Value_ = input.ReadBytes();
								break;
							}
						case 26:
							{
								KeyValue.Builder SubBuilder = KeyValue.NewBuilder();
								input.ReadMessage(SubBuilder, ext);
								AddMetadata(SubBuilder.BuildPartial());
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

			// required bytes value = 2;
			internal ByteString Value_ = ByteString.Empty;
			public bool HasValue()
			{
				return ((BitField0_ & 0x00000002) == 0x00000002);
			}
			public ByteString Value => Value_;

            public Builder SetValue(ByteString Value)
			{
				if (Value == null)
				{
					throw new NullReferenceException();
				}
				BitField0_ |= 0x00000002;
				Value_ = Value;

				return this;
			}
			public Builder ClearValue()
			{
				BitField0_ = (BitField0_ & ~0x00000002);
				Value_ = DefaultInstance.Value;

				return this;
			}

			// repeated .pulsar.proto.KeyValue metadata = 3;
			internal IList<KeyValue> Metadata_ = new List<KeyValue>();
			public void EnsureMetadataIsMutable()
			{
				if (!((BitField0_ & 0x00000004) == 0x00000004))
				{
					Metadata_ = new List<KeyValue>(Metadata_);
					BitField0_ |= 0x00000004;
				}
			}

			public IList<KeyValue> MetadataList => new List<KeyValue>(Metadata_);

            public int MetadataCount => Metadata_.Count;

            public KeyValue GetMetadata(int Index)
			{
				return Metadata_[Index];
			}
			public Builder SetMetadata(int Index, KeyValue Value)
			{
				if (Value == null)
				{
					throw new NullReferenceException();
				}
				EnsureMetadataIsMutable();
				Metadata_[Index] = Value;

				return this;
			}
			public Builder SetMetadata(int Index, KeyValue.Builder BuilderForValue)
			{
				EnsureMetadataIsMutable();
				Metadata_[Index] = BuilderForValue.Build();

				return this;
			}
			public Builder AddMetadata(KeyValue Value)
			{
				if (Value == null)
				{
					throw new NullReferenceException();
				}
				EnsureMetadataIsMutable();
				Metadata_.Add(Value);

				return this;
			}
			public Builder AddMetadata(int Index, KeyValue Value)
			{
				if (Value == null)
				{
					throw new NullReferenceException();
				}
				EnsureMetadataIsMutable();
				Metadata_.Insert(Index, Value);

				return this;
			}
			public Builder AddMetadata(KeyValue.Builder BuilderForValue)
			{
				EnsureMetadataIsMutable();
				Metadata_.Add(BuilderForValue.Build());

				return this;
			}
			public Builder AddMetadata(int Index, KeyValue.Builder BuilderForValue)
			{
				EnsureMetadataIsMutable();
				Metadata_.Insert(Index, BuilderForValue.Build());

				return this;
			}
			public Builder AddAllMetadata(IEnumerable<KeyValue> Values)
			{
				EnsureMetadataIsMutable();
				Values.ToList().ForEach(Metadata_.Add);

				return this;
			}
			public Builder ClearMetadata()
			{
				Metadata_ = new List<KeyValue>();
				BitField0_ = (BitField0_ & ~0x00000004);

				return this;
			}
			public Builder RemoveMetadata(int Index)
			{
				EnsureMetadataIsMutable();
				Metadata_.RemoveAt(Index);

				return this;
			}

		}

		static EncryptionKeys()
		{
			_defaultInstance = new EncryptionKeys(true);
			_defaultInstance.InitFields();
		}

		// @@protoc_insertion_point(class_scope:pulsar.proto.EncryptionKeys)
	}

}
