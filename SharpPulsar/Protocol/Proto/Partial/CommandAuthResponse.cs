using DotNetty.Common;
using Google.Protobuf;
using System;
using SharpPulsar.Utility.Protobuf;
using static SharpPulsar.Utility.Protobuf.ByteBufCodedInputStream;

namespace SharpPulsar.Protocol.Proto
{
    public sealed partial class CommandAuthResponse : ByteBufCodedOutputStream.ByteBufGeneratedMessage
    {
        // Use CommandAuthResponse.newBuilder() to construct.
        internal static ThreadLocalPool<CommandAuthResponse> _pool = new ThreadLocalPool<CommandAuthResponse>(handle => new CommandAuthResponse(handle), 1, true);

        internal ThreadLocalPool.Handle _handle;
        private CommandAuthResponse(ThreadLocalPool.Handle handle)
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

        public CommandAuthResponse(bool NoInit)
        {
        }

        internal static readonly CommandAuthResponse _defaultInstance;
        public static CommandAuthResponse DefaultInstance => _defaultInstance;

        public CommandAuthResponse DefaultInstanceForType => _defaultInstance;


        public void InitFields()
        {
            ClientVersion = "";
            Response = AuthData.DefaultInstance;
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
                Output.WriteBytes(1, ByteString.CopyFromUtf8(ClientVersion));
            }
            if (((_hasBits0 & 0x00000002) == 0x00000002))
            {
                Output.WriteMessage(2, Response);
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
        public static Builder NewBuilder(CommandAuthResponse Prototype)
        {
            return NewBuilder().MergeFrom(Prototype);
        }
        public Builder ToBuilder()
        {
            return NewBuilder(this);
        }

        public sealed class Builder : ByteBufMessageBuilder
        {
            // Construct using org.apache.pulsar.common.api.proto.CommandAuthResponse.newBuilder()
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
                ClientVersion_ = "";
                BitField0_ = (BitField0_ & ~0x00000001);
                Response_ = AuthData.DefaultInstance;
                BitField0_ = (BitField0_ & ~0x00000002);
                ProtocolVersion_ = 0;
                BitField0_ = (BitField0_ & ~0x00000004);
                return this;
            }

            public Builder Clone()
            {
                return Create().MergeFrom(BuildPartial());
            }

            public CommandAuthResponse DefaultInstanceForType => DefaultInstance;

            public CommandAuthResponse Build()
            {
                CommandAuthResponse Result = BuildPartial();
                if (!Result.Initialized)
                {
                    throw new NullReferenceException($"{Result.GetType().Name} not initialized");
                }
                return Result;
            }

            public CommandAuthResponse BuildParsed()
            {
                CommandAuthResponse Result = BuildPartial();
                if (!Result.Initialized)
                {
                    throw new NullReferenceException($"{Result.GetType().Name} not initialized");
                }
                return Result;
            }

            public CommandAuthResponse BuildPartial()
            {
                CommandAuthResponse Result = CommandAuthResponse._pool.Take();
                int FromBitField0_ = BitField0_;
                int ToBitField0_ = 0;
                if (((FromBitField0_ & 0x00000001) == 0x00000001))
                {
                    ToBitField0_ |= 0x00000001;
                }
                Result.ClientVersion = ClientVersion_.ToString();
                if (((FromBitField0_ & 0x00000002) == 0x00000002))
                {
                    ToBitField0_ |= 0x00000002;
                }
                Result.Response = Response_;
                if (((FromBitField0_ & 0x00000004) == 0x00000004))
                {
                    ToBitField0_ |= 0x00000004;
                }
                Result.ProtocolVersion = ProtocolVersion_;
                Result._hasBits0 = ToBitField0_;
                return Result;
            }

            public Builder MergeFrom(CommandAuthResponse Other)
            {
                if (Other == DefaultInstance)
                {
                    return this;
                }
                if (Other.HasClientVersion)
                {
                    SetClientVersion(Other.ClientVersion);
                }
                if (Other.HasResponse)
                {
                    MergeResponse(Other.Response);
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
                                ClientVersion_ = input.ReadBytes();
                                break;
                            }
                        case 18:
                            {
                                AuthData.Builder SubBuilder = AuthData.NewBuilder();
                                if (HasResponse())
                                {
                                    SubBuilder.MergeFrom(GetResponse());
                                }
                                input.ReadMessage(SubBuilder, extensionRegistry);
                                SetResponse(SubBuilder.BuildPartial());
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

            // optional string client_version = 1;
            internal object ClientVersion_ = "";
            public bool HasClientVersion()
            {
                return ((BitField0_ & 0x00000001) == 0x00000001);
            }
            public string GetClientVersion()
            {
                object Ref = ClientVersion_;
                if (!(Ref is string))
                {
                    string S = ((ByteString)Ref).ToStringUtf8();
                    ClientVersion_ = S;
                    return S;
                }
                else
                {
                    return (string)Ref;
                }
            }
            public Builder SetClientVersion(string Value)
            {
                if (string.ReferenceEquals(Value, null))
                {
                    throw new NullReferenceException();
                }
                BitField0_ |= 0x00000001;
                ClientVersion_ = Value;

                return this;
            }
            public Builder ClearClientVersion()
            {
                BitField0_ = (BitField0_ & ~0x00000001);
                ClientVersion_ = DefaultInstance.ClientVersion;

                return this;
            }
            public void SetClientVersion(ByteString Value)
            {
                BitField0_ |= 0x00000001;
                ClientVersion_ = Value;

            }

            // optional .pulsar.proto.AuthData response = 2;
            internal AuthData Response_ = AuthData.DefaultInstance;
            public bool HasResponse()
            {
                return ((BitField0_ & 0x00000002) == 0x00000002);
            }
            public AuthData GetResponse()
            {
                return Response_;
            }
            public Builder SetResponse(AuthData Value)
            {
                if (Value == null)
                {
                    throw new NullReferenceException();
                }
                Response_ = Value;

                BitField0_ |= 0x00000002;
                return this;
            }
            public Builder SetResponse(AuthData.Builder BuilderForValue)
            {
                Response_ = BuilderForValue.Build();

                BitField0_ |= 0x00000002;
                return this;
            }
            public Builder MergeResponse(AuthData Value)
            {
                if (((BitField0_ & 0x00000002) == 0x00000002) && Response_ != AuthData.DefaultInstance)
                {
                    Response_ = AuthData.NewBuilder(Response_).MergeFrom(Value).BuildPartial();
                }
                else
                {
                    Response_ = Value;
                }

                BitField0_ |= 0x00000002;
                return this;
            }
            public Builder ClearResponse()
            {
                Response_ = AuthData.DefaultInstance;

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

            // @@protoc_insertion_point(builder_scope:pulsar.proto.CommandAuthResponse)
        }

        static CommandAuthResponse()
        {
            _defaultInstance = new CommandAuthResponse(true);
            _defaultInstance.InitFields();
        }

        // @@protoc_insertion_point(class_scope:pulsar.proto.CommandAuthResponse)
    }

}
