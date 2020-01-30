using DotNetty.Common;
using Google.Protobuf;
using SharpPulsar.Util.Protobuf;
using System;

namespace SharpPulsar.Protocol.Proto
{
	public partial class KeyValue : ByteBufCodedOutputStream.ByteBufGeneratedMessage
	{
		// Use KeyValue.newBuilder() to construct.
		internal static ThreadLocalPool<KeyValue> _pool = new ThreadLocalPool<KeyValue>(handle => new KeyValue(handle), 1, true);

		internal ThreadLocalPool.Handle _handle;
		private KeyValue(ThreadLocalPool.Handle handle)
		{
			_handle = handle;
		}

		public void Recycle()
		{
			this.InitFields();
			this.MemoizedIsInitialized = -1;
			this.BitField0_ = 0;
			this.MemoizedSerializedSize = -1;
			if (_handle != null)
			{
				_handle.Release(this);
			}
		}

		public KeyValue(bool NoInit)
		{
		}

		//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		internal static readonly KeyValue _defaultInstance;
		public static KeyValue DefaultInstance
		{
			get
			{
				return _defaultInstance;
			}
		}

		public KeyValue DefaultInstanceForType
		{
			get
			{
				return _defaultInstance;
			}
		}

		internal int BitField0_;
		public void InitFields()
		{
			Key = "";
			Value = "";
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
			var _ =SerializedSize;
			if (((BitField0_ & 0x00000001) == 0x00000001))
			{
				Output.WriteBytes(1, ByteString.CopyFromUtf8(Key));
			}
			if (((BitField0_ & 0x00000002) == 0x00000002))
			{
				Output.WriteBytes(2, ByteString.CopyFromUtf8(Value));
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
		public static Builder NewBuilder(KeyValue prototype)
		{
			return NewBuilder().MergeFrom(prototype);
		}
		public Builder ToBuilder()
		{
			return NewBuilder(this);
		}

		public sealed class Builder : ByteBufCodedInputStream.ByteBufMessageBuilder
		{
			// Construct using org.apache.pulsar.common.api.proto.KeyValue.newBuilder()
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
				Value_ = "";
				BitField0_ = (BitField0_ & ~0x00000002);
				return this;
			}

			public Builder Clone()
			{
				return Create().MergeFrom(BuildPartial());
			}

			public KeyValue DefaultInstanceForType
			{
				get
				{
					return KeyValue.DefaultInstance;
				}
			}

			public KeyValue Build()
			{
				KeyValue Result = BuildPartial();
				if (!Result.Initialized)
				{
					throw new NullReferenceException("KeyValue not initialized");
				}
				return Result;
			}

			public KeyValue BuildParsed()
			{
				KeyValue Result = BuildPartial();
				if (!Result.Initialized)
				{
					throw new NullReferenceException("KeyValue not initialized");
				}
				return Result;
			}

			public KeyValue BuildPartial()
			{
				KeyValue Result = KeyValue._pool.Take();
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
				Result.Value = Value_.ToString();
				Result.BitField0_ = ToBitField0_;
				return Result;
			}

			public Builder MergeFrom(KeyValue Other)
			{
				if (Other == KeyValue.DefaultInstance)
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
			public Builder MergeFrom(ByteBufCodedInputStream input, ExtensionRegistry extensionRegistry)
			{
				while (true)
				{
					int tag = input.ReadTag();
					switch (tag)
					{
						case 0:

							return this;
						default:
							{
								if (!input.SkipField(tag))
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
					throw new System.NullReferenceException();
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

			// required string value = 2;
			internal object Value_ = "";
			public bool HasValue()
			{
				return ((BitField0_ & 0x00000002) == 0x00000002);
			}
			public string GetValue()
			{
				object Ref = Value_;
				if (!(Ref is string))
				{
					string S = ((ByteString)Ref).ToStringUtf8();
					Value_ = S;
					return S;
				}
				else
				{
					return (string)Ref;
				}
			}
			public Builder SetValue(string Value)
			{
				if (string.ReferenceEquals(Value, null))
				{
					throw new System.NullReferenceException();
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
			public void SetValue(ByteString Value)
			{
				BitField0_ |= 0x00000002;
				Value_ = Value;

			}

			ByteBufCodedInputStream.ByteBufMessageBuilder ByteBufCodedInputStream.ByteBufMessageBuilder.MergeFrom(ByteBufCodedInputStream input, ExtensionRegistry ext)
			{
				throw new NotImplementedException();
			}

			// @@protoc_insertion_point(builder_scope:pulsar.proto.KeyValue)
		}

		static KeyValue()
		{
			_defaultInstance = new KeyValue(true);
			_defaultInstance.InitFields();
		}

		// @@protoc_insertion_point(class_scope:pulsar.proto.KeyValue)
	}

}
