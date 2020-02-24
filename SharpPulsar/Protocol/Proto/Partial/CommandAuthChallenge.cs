using DotNetty.Common;
using Google.Protobuf;
using System;
using SharpPulsar.Utility.Protobuf;
using static SharpPulsar.Utility.Protobuf.ByteBufCodedInputStream;

namespace SharpPulsar.Protocol.Proto
{
	public sealed partial class CommandAuthChallenge : ByteBufCodedOutputStream.ByteBufGeneratedMessage
	{
		// Use CommandAuthChallenge.newBuilder() to construct.
		internal static ThreadLocalPool<CommandAuthChallenge> _pool = new ThreadLocalPool<CommandAuthChallenge>(handle => new CommandAuthChallenge(handle), 1, true);

		internal ThreadLocalPool.Handle _handle;
		private CommandAuthChallenge(ThreadLocalPool.Handle handle)
		{
			_handle = handle;
		}

		public void Recycle()
		{
			InitFields();
			MemoizedIsInitialized = -1;
			_hasBits0 = 0;
			MemoizedSerializedSize = -1;
            _handle?.Release(this);
        }

		public CommandAuthChallenge(bool NoInit)
		{
		}


		internal static readonly CommandAuthChallenge _defaultInstance;
		public static CommandAuthChallenge DefaultInstance => _defaultInstance;

        public CommandAuthChallenge DefaultInstanceForType => _defaultInstance;


        public void InitFields()
		{
			ServerVersion = "";
			Challenge = AuthData.DefaultInstance;
			ProtocolVersion = 0;
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
			if (((_hasBits0 & 0x00000001) == 0x00000001))
			{
				Output.WriteBytes(1, ByteString.CopyFromUtf8(ServerVersion));
			}
			if (((_hasBits0 & 0x00000002) == 0x00000002))
			{
				Output.WriteMessage(2, Challenge);
			}
			if (((_hasBits0 & 0x00000004) == 0x00000004))
			{
				Output.WriteInt32(3, ProtocolVersion);
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
		public static Builder NewBuilder(CommandAuthChallenge Prototype)
		{
			return NewBuilder().MergeFrom(Prototype);
		}
		public Builder ToBuilder()
		{
			return NewBuilder(this);
		}

		public sealed class Builder : ByteBufMessageBuilder
		{
			// Construct using org.apache.pulsar.common.api.proto.CommandAuthChallenge.newBuilder()
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
				ServerVersion_ = "";
				BitField0_ = (BitField0_ & ~0x00000001);
				Challenge_ = AuthData.DefaultInstance;
				BitField0_ = (BitField0_ & ~0x00000002);
				ProtocolVersion_ = 0;
				BitField0_ = (BitField0_ & ~0x00000004);
				return this;
			}

			public Builder Clone()
			{
				return Create().MergeFrom(BuildPartial());
			}

			public CommandAuthChallenge DefaultInstanceForType => DefaultInstance;

            public CommandAuthChallenge Build()
			{
				CommandAuthChallenge Result = BuildPartial();
				
				return Result;
			}

			public CommandAuthChallenge BuildParsed()
			{
				CommandAuthChallenge Result = BuildPartial();
				
				return Result;
			}

			public CommandAuthChallenge BuildPartial()
			{
				CommandAuthChallenge Result = CommandAuthChallenge._pool.Take();
				int FromBitField0_ = BitField0_;
				int ToBitField0_ = 0;
				if (((FromBitField0_ & 0x00000001) == 0x00000001))
				{
					ToBitField0_ |= 0x00000001;
				}
				Result.ServerVersion = ServerVersion_.ToString();
				if (((FromBitField0_ & 0x00000002) == 0x00000002))
				{
					ToBitField0_ |= 0x00000002;
				}
				Result.Challenge = Challenge_;
				if (((FromBitField0_ & 0x00000004) == 0x00000004))
				{
					ToBitField0_ |= 0x00000004;
				}
				Result.ProtocolVersion = ProtocolVersion_;
				Result._hasBits0 = ToBitField0_;
				return Result;
			}

			public Builder MergeFrom(CommandAuthChallenge Other)
			{
				if (Other == DefaultInstance)
				{
					return this;
				}
				if (Other.HasServerVersion)
				{
					SetServerVersion(Other.ServerVersion);
				}
				if (Other.HasChallenge)
				{
					MergeChallenge(Other.Challenge);
				}
				if (Other.HasProtocolVersion)
				{
					SetProtocolVersion(Other.ProtocolVersion);
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
								ServerVersion_ = input.ReadBytes();
								break;
							}
						case 18:
							{
								AuthData.Builder SubBuilder = AuthData.NewBuilder();
								if (HasChallenge())
								{
									SubBuilder.MergeFrom(GetChallenge());
								}
								input.ReadMessage(SubBuilder, extensionRegistry);
								SetChallenge(SubBuilder.BuildPartial());
								SubBuilder.Recycle();
								break;
							}
						case 24:
							{
								BitField0_ |= 0x00000004;
								ProtocolVersion_ = input.ReadInt32();
								break;
							}
					}
				}
			}

			internal int BitField0_;

			// optional string server_version = 1;
			internal object ServerVersion_ = "";
			public bool HasServerVersion()
			{
				return ((BitField0_ & 0x00000001) == 0x00000001);
			}
			public string getServerVersion()
			{
				object Ref = ServerVersion_;
				if (!(Ref is string))
				{
					string S = ((ByteString)Ref).ToStringUtf8();
					ServerVersion_ = S;
					return S;
				}
				else
				{
					return (string)Ref;
				}
			}
			public Builder SetServerVersion(string Value)
			{
				if (string.ReferenceEquals(Value, null))
				{
					throw new NullReferenceException();
				}
				BitField0_ |= 0x00000001;
				ServerVersion_ = Value;

				return this;
			}
			public Builder ClearServerVersion()
			{
				BitField0_ = (BitField0_ & ~0x00000001);
				ServerVersion_ = DefaultInstance.ServerVersion;

				return this;
			}
			public void SetServerVersion(ByteString Value)
			{
				BitField0_ |= 0x00000001;
				ServerVersion_ = Value;

			}

			// optional .pulsar.proto.AuthData challenge = 2;
			internal AuthData Challenge_ = AuthData.DefaultInstance;
			public bool HasChallenge()
			{
				return ((BitField0_ & 0x00000002) == 0x00000002);
			}
			public AuthData GetChallenge()
			{
				return Challenge_;
			}
			public Builder SetChallenge(AuthData Value)
			{
				if (Value == null)
				{
					throw new NullReferenceException();
				}
				Challenge_ = Value;

				BitField0_ |= 0x00000002;
				return this;
			}
			public Builder SetChallenge(AuthData.Builder BuilderForValue)
			{
				Challenge_ = BuilderForValue.Build();

				BitField0_ |= 0x00000002;
				return this;
			}
			public Builder MergeChallenge(AuthData Value)
			{
				if (((BitField0_ & 0x00000002) == 0x00000002) && Challenge_ != AuthData.DefaultInstance)
				{
					Challenge_ = AuthData.NewBuilder(Challenge_).MergeFrom(Value).BuildPartial();
				}
				else
				{
					Challenge_ = Value;
				}

				BitField0_ |= 0x00000002;
				return this;
			}
			public Builder ClearChallenge()
			{
				Challenge_ = AuthData.DefaultInstance;

				BitField0_ = (BitField0_ & ~0x00000002);
				return this;
			}

			// optional int32 protocol_version = 3 [default = 0];
			internal int ProtocolVersion_;
			public bool HasProtocolVersion()
			{
				return ((BitField0_ & 0x00000004) == 0x00000004);
			}
			public int ProtocolVersion => ProtocolVersion_;

            public Builder SetProtocolVersion(int Value)
			{
				BitField0_ |= 0x00000004;
				ProtocolVersion_ = Value;

				return this;
			}
			public Builder ClearProtocolVersion()
			{
				BitField0_ = (BitField0_ & ~0x00000004);
				ProtocolVersion_ = 0;

				return this;
			}

			// @@protoc_insertion_point(builder_scope:pulsar.proto.CommandAuthChallenge)
		}

		static CommandAuthChallenge()
		{
			_defaultInstance = new CommandAuthChallenge(true);
			_defaultInstance.InitFields();
		}

		// @@protoc_insertion_point(class_scope:pulsar.proto.CommandAuthChallenge)
	}

}
