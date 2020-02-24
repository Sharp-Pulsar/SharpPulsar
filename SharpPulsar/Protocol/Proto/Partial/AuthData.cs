using DotNetty.Common;
using Google.Protobuf;
using System;
using SharpPulsar.Utility.Protobuf;
using static SharpPulsar.Utility.Protobuf.ByteBufCodedInputStream;

namespace SharpPulsar.Protocol.Proto
{
	public sealed partial class AuthData : ByteBufCodedOutputStream.ByteBufGeneratedMessage
	{
		// Use AuthData.newBuilder() to construct.
		internal static ThreadLocalPool<AuthData> _pool = new ThreadLocalPool<AuthData>(handle => new AuthData(handle), 1, true);

		internal ThreadLocalPool.Handle _handle;
		private AuthData(ThreadLocalPool.Handle handle)
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

		public AuthData(bool NoInit)
		{
		}

		internal static readonly AuthData _defaultInstance;
		public static AuthData DefaultInstance => _defaultInstance;

        public AuthData DefaultInstanceForType => _defaultInstance;

        internal int BitField0_;
		
		public void InitFields()
		{
			AuthMethodName = "";
			AuthData_ = ByteString.Empty;
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

				MemoizedIsInitialized = 1;
				return true;
			}
		}

		public void WriteTo(ByteBufCodedOutputStream Output)
		{
			var _ = SerializedSize;
			if (((BitField0_ & 0x00000001) == 0x00000001))
			{
				Output.WriteBytes(1, ByteString.CopyFromUtf8(AuthMethodName));
			}
			if (((BitField0_ & 0x00000002) == 0x00000002))
			{
				Output.WriteBytes(2, AuthData_);
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
		public static Builder NewBuilder(AuthData Prototype)
		{
			return NewBuilder().MergeFrom(Prototype);
		}
		public Builder ToBuilder()
		{
			return NewBuilder(this);
		}

		public sealed class Builder : ByteBufMessageBuilder
		{
			// Construct using org.apache.pulsar.common.api.proto.AuthData.newBuilder()
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
				AuthMethodName_ = "";
				BitField0_ = (BitField0_ & ~0x00000001);
				AuthData_ = ByteString.Empty;
				BitField0_ = (BitField0_ & ~0x00000002);
				return this;
			}

			public Builder Clone()
			{
				return Create().MergeFrom(BuildPartial());
			}

			public AuthData DefaultInstanceForType => Proto.AuthData.DefaultInstance;

            public AuthData Build()
			{
				AuthData Result = BuildPartial();
				
				return Result;
			}

			public AuthData BuildParsed()
			{
				AuthData Result = BuildPartial();
				
				return Result;
			}

			public AuthData BuildPartial()
			{
				AuthData Result = Proto.AuthData._pool.Take();
				int FromBitField0_ = BitField0_;
				int ToBitField0_ = 0;
				if (((FromBitField0_ & 0x00000001) == 0x00000001))
				{
					ToBitField0_ |= 0x00000001;
				}
				Result.AuthMethodName = AuthMethodName_.ToString();
				if (((FromBitField0_ & 0x00000002) == 0x00000002))
				{
					ToBitField0_ |= 0x00000002;
				}
				Result.AuthData_ = AuthData_;
				Result.BitField0_ = ToBitField0_;
				return Result;
			}

			public Builder MergeFrom(AuthData Other)
			{
				if (Other == DefaultInstance)
				{
					return this;
				}
				if (Other.HasAuthMethodName)
				{
					SetAuthMethodName(Other.AuthMethodName);
				}
				if (Other.HasAuthData_)
				{
					SetAuthData(Other.AuthData_);
				}
				return this;
			}

			public bool Initialized => true;

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
								AuthMethodName_ = input.ReadBytes();
								break;
							}
						case 18:
							{
								BitField0_ |= 0x00000002;
								AuthData_ = input.ReadBytes();
								break;
							}
					}
				}
			}

			internal int BitField0_;

			// optional string auth_method_name = 1;
			internal object AuthMethodName_ = "";
			public bool HasAuthMethodName()
			{
				return ((BitField0_ & 0x00000001) == 0x00000001);
			}
			public string getAuthMethodName()
			{
				object Ref = AuthMethodName_;
				if (!(Ref is string))
				{
					string S = ((ByteString)Ref).ToStringUtf8();
					AuthMethodName_ = S;
					return S;
				}
				else
				{
					return (string)Ref;
				}
			}
			public Builder SetAuthMethodName(string Value)
			{
				if (string.ReferenceEquals(Value, null))
				{
					throw new NullReferenceException();
				}
				BitField0_ |= 0x00000001;
				AuthMethodName_ = Value;

				return this;
			}
			public Builder ClearAuthMethodName()
			{
				BitField0_ = (BitField0_ & ~0x00000001);
				AuthMethodName_ = DefaultInstance.AuthMethodName;

				return this;
			}
			public void SetAuthMethodName(ByteString Value)
			{
				BitField0_ |= 0x00000001;
				AuthMethodName_ = Value;

			}

			// optional bytes auth_data = 2;
			internal ByteString AuthData_ = ByteString.Empty;
			public bool HasAuthData()
			{
				return ((BitField0_ & 0x00000002) == 0x00000002);
			}
			public ByteString AuthData => AuthData_;

            public Builder SetAuthData(ByteString Value)
			{
				if (Value == null)
				{
					throw new NullReferenceException();
				}
				BitField0_ |= 0x00000002;
				AuthData_ = Value;

				return this;
			}
			public Builder ClearAuthData()
			{
				BitField0_ = (BitField0_ & ~0x00000002);
				AuthData_ = DefaultInstance.AuthData_;

				return this;
			}

			// @@protoc_insertion_point(builder_scope:pulsar.proto.AuthData)
		}

		static AuthData()
		{
			_defaultInstance = new AuthData(true);
			_defaultInstance.InitFields();
		}

		// @@protoc_insertion_point(class_scope:pulsar.proto.AuthData)
	}

}
