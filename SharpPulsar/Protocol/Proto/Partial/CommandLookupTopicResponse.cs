using DotNetty.Common;
using Google.Protobuf;
using System;
using System.Linq;
using SharpPulsar.Utility.Protobuf;
using static SharpPulsar.Utility.Protobuf.ByteBufCodedInputStream;

namespace SharpPulsar.Protocol.Proto
{
	public sealed partial class CommandLookupTopicResponse : ByteBufCodedOutputStream.ByteBufGeneratedMessage
	{
		// Use CommandLookupTopicResponse.newBuilder() to construct.
		internal static ThreadLocalPool<CommandLookupTopicResponse> _pool = new ThreadLocalPool<CommandLookupTopicResponse>(handle => new CommandLookupTopicResponse(handle), 1, true);

		internal ThreadLocalPool.Handle _handle;
		private CommandLookupTopicResponse(ThreadLocalPool.Handle handle)
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

		public CommandLookupTopicResponse(bool NoInit)
		{
		}

		
		internal static readonly CommandLookupTopicResponse _defaultInstance;
		public static CommandLookupTopicResponse DefaultInstance => _defaultInstance;

        public CommandLookupTopicResponse DefaultInstanceForType => _defaultInstance;


        public void InitFields()
		{
			BrokerServiceUrl = "";
			BrokerServiceUrlTls = "";
			Response = Types.LookupType.Redirect;
			RequestId = 0L;
			Authoritative = false;
			Error = ServerError.UnknownError;
			Message = "";
			ProxyThroughServiceUrl = false;
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
				MemoizedIsInitialized = 1;
				return true;
			}
		}

		public void WriteTo(ByteBufCodedOutputStream Output)
		{
			var _= SerializedSize;
			if (((_hasBits0 & 0x00000001) == 0x00000001))
			{
				Output.WriteBytes(1, ByteString.CopyFromUtf8(BrokerServiceUrl));
			}
			if (((_hasBits0 & 0x00000002) == 0x00000002))
			{
				Output.WriteBytes(2, ByteString.CopyFromUtf8(BrokerServiceUrlTls));
			}
			if (((_hasBits0 & 0x00000004) == 0x00000004))
			{
				Output.WriteEnum(3, (int)Response);
			}
			if (((_hasBits0 & 0x00000008) == 0x00000008))
			{
				Output.WriteUInt64(4, (long)RequestId);
			}
			if (((_hasBits0 & 0x00000010) == 0x00000010))
			{
				Output.WriteBool(5, Authoritative);
			}
			if (((_hasBits0 & 0x00000020) == 0x00000020))
			{
				Output.WriteEnum(6, (int)Error);
			}
			if (((_hasBits0 & 0x00000040) == 0x00000040))
			{
				Output.WriteBytes(7, ByteString.CopyFromUtf8(Message));
			}
			if (((_hasBits0 & 0x00000080) == 0x00000080))
			{
				Output.WriteBool(8, ProxyThroughServiceUrl);
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
		public static Builder NewBuilder(CommandLookupTopicResponse Prototype)
		{
			return NewBuilder().MergeFrom(Prototype);
		}
		public Builder ToBuilder()
		{
			return NewBuilder(this);
		}

		public sealed class Builder : ByteBufMessageBuilder
		{
			// Construct using org.apache.pulsar.common.api.proto.CommandLookupTopicResponse.newBuilder()
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

				BrokerServiceUrl_ = "";
				BitField0_ = (BitField0_ & ~0x00000001);
				BrokerServiceUrlTls_ = "";
				BitField0_ = (BitField0_ & ~0x00000002);
				Response_ = CommandLookupTopicResponse.Types.LookupType.Redirect;
				BitField0_ = (BitField0_ & ~0x00000004);
				RequestId_ = 0L;
				BitField0_ = (BitField0_ & ~0x00000008);
				Authoritative_ = false;
				BitField0_ = (BitField0_ & ~0x00000010);
				Error_ = ServerError.UnknownError;
				BitField0_ = (BitField0_ & ~0x00000020);
				Message_ = "";
				BitField0_ = (BitField0_ & ~0x00000040);
				ProxyThroughServiceUrl_ = false;
				BitField0_ = (BitField0_ & ~0x00000080);
				return this;
			}

			public Builder Clone()
			{
				return Create().MergeFrom(BuildPartial());
			}

			public CommandLookupTopicResponse DefaultInstanceForType => CommandLookupTopicResponse.DefaultInstance;

            public CommandLookupTopicResponse Build()
			{
				CommandLookupTopicResponse Result = BuildPartial();
				
				return Result;
			}

			public CommandLookupTopicResponse BuildParsed()
			{
				CommandLookupTopicResponse Result = BuildPartial();
				
				return Result;
			}

			public CommandLookupTopicResponse BuildPartial()
			{
				CommandLookupTopicResponse Result = CommandLookupTopicResponse._pool.Take();
				int FromBitField0_ = BitField0_;
				int ToBitField0_ = 0;
				if (((FromBitField0_ & 0x00000001) == 0x00000001))
				{
					ToBitField0_ |= 0x00000001;
				}
				Result.BrokerServiceUrl = BrokerServiceUrl_.ToString();
				if (((FromBitField0_ & 0x00000002) == 0x00000002))
				{
					ToBitField0_ |= 0x00000002;
				}
				Result.BrokerServiceUrlTls = BrokerServiceUrlTls_.ToString();
				if (((FromBitField0_ & 0x00000004) == 0x00000004))
				{
					ToBitField0_ |= 0x00000004;
				}
				Result.Response = Response_;
				if (((FromBitField0_ & 0x00000008) == 0x00000008))
				{
					ToBitField0_ |= 0x00000008;
				}
				Result.RequestId = (ulong)RequestId_;
				if (((FromBitField0_ & 0x00000010) == 0x00000010))
				{
					ToBitField0_ |= 0x00000010;
				}
				Result.Authoritative = Authoritative_;
				if (((FromBitField0_ & 0x00000020) == 0x00000020))
				{
					ToBitField0_ |= 0x00000020;
				}
				Result.Error = Error_;
				if (((FromBitField0_ & 0x00000040) == 0x00000040))
				{
					ToBitField0_ |= 0x00000040;
				}
				Result.Message = Message_.ToString();
				if (((FromBitField0_ & 0x00000080) == 0x00000080))
				{
					ToBitField0_ |= 0x00000080;
				}
				Result.ProxyThroughServiceUrl = ProxyThroughServiceUrl_;
				Result._hasBits0 = ToBitField0_;
				return Result;
			}

			public Builder MergeFrom(CommandLookupTopicResponse Other)
			{
				if (Other == DefaultInstance)
				{
					return this;
				}
				if (Other.HasBrokerServiceUrl)
				{
					SetBrokerServiceUrl(Other.BrokerServiceUrl);
				}
				if (Other.HasBrokerServiceUrlTls)
				{
					SetBrokerServiceUrlTls(Other.BrokerServiceUrlTls);
				}
				if (Other.HasResponse)
				{
					SetResponse(Other.Response);
				}
				if (Other.HasRequestId)
				{
					SetRequestId((long)Other.RequestId);
				}
				if (Other.HasAuthoritative)
				{
					SetAuthoritative(Other.Authoritative);
				}
				if (Other.HasError)
				{
					SetError(Other.Error);
				}
				if (Other.HasMessage)
				{
					SetMessage(Other.Message);
				}
				if (Other.HasProxyThroughServiceUrl)
				{
					SetProxyThroughServiceUrl(Other.ProxyThroughServiceUrl);
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
								BrokerServiceUrl_ = input.ReadBytes();
								break;
							}
						case 18:
							{
								BitField0_ |= 0x00000002;
								BrokerServiceUrlTls_ = input.ReadBytes();
								break;
							}
						case 24:
							{
								int RawValue = input.ReadEnum();
								Types.LookupType Value = Enum.GetValues(typeof(Types.LookupType)).Cast<Types.LookupType>().ToList()[RawValue];
								if (Value != null)
								{
									BitField0_ |= 0x00000004;
									Response_ = Value;
								}
								break;
							}
						case 32:
							{
								BitField0_ |= 0x00000008;
								RequestId_ = input.ReadUInt64();
								break;
							}
						case 40:
							{
								BitField0_ |= 0x00000010;
								Authoritative_ = input.ReadBool();
								break;
							}
						case 48:
							{
								int RawValue = input.ReadEnum();
								ServerError Value = Enum.GetValues(typeof(ServerError)).Cast<ServerError>().ToList()[RawValue];
								if (Value != null)
								{
									BitField0_ |= 0x00000020;
									Error_ = Value;
								}
								break;
							}
						case 58:
							{
								BitField0_ |= 0x00000040;
								Message_ = input.ReadBytes();
								break;
							}
						case 64:
							{
								BitField0_ |= 0x00000080;
								ProxyThroughServiceUrl_ = input.ReadBool();
								break;
							}
					}
				}
			}

			internal int BitField0_;

			// optional string brokerServiceUrl = 1;
			internal object BrokerServiceUrl_ = "";
			public bool HasBrokerServiceUrl()
			{
				return ((BitField0_ & 0x00000001) == 0x00000001);
			}
			public string GetBrokerServiceUrl()
			{
				object Ref = BrokerServiceUrl_;
				if (!(Ref is string))
				{
					string S = ((ByteString)Ref).ToStringUtf8();
					BrokerServiceUrl_ = S;
					return S;
				}
				else
				{
					return (string)Ref;
				}
			}
			public Builder SetBrokerServiceUrl(string Value)
			{
				if (string.ReferenceEquals(Value, null))
				{
					throw new NullReferenceException();
				}
				BitField0_ |= 0x00000001;
				BrokerServiceUrl_ = Value;

				return this;
			}
			public Builder ClearBrokerServiceUrl()
			{
				BitField0_ = (BitField0_ & ~0x00000001);
				BrokerServiceUrl_ = DefaultInstance.BrokerServiceUrl;

				return this;
			}
			public void SetBrokerServiceUrl(ByteString Value)
			{
				BitField0_ |= 0x00000001;
				BrokerServiceUrl_ = Value;

			}

			// optional string brokerServiceUrlTls = 2;
			internal object BrokerServiceUrlTls_ = "";
			public bool HasBrokerServiceUrlTls()
			{
				return ((BitField0_ & 0x00000002) == 0x00000002);
			}
			public string GetBrokerServiceUrlTls()
			{
				object Ref = BrokerServiceUrlTls_;
				if (!(Ref is string))
				{
					string S = ((ByteString)Ref).ToStringUtf8();
					BrokerServiceUrlTls_ = S;
					return S;
				}
				else
				{
					return (string)Ref;
				}
			}
			public Builder SetBrokerServiceUrlTls(string Value)
			{
				if (string.ReferenceEquals(Value, null))
				{
					throw new NullReferenceException();
				}
				BitField0_ |= 0x00000002;
				BrokerServiceUrlTls_ = Value;

				return this;
			}
			public Builder ClearBrokerServiceUrlTls()
			{
				BitField0_ = (BitField0_ & ~0x00000002);
				BrokerServiceUrlTls_ = DefaultInstance.BrokerServiceUrlTls;

				return this;
			}
			public void SetBrokerServiceUrlTls(ByteString Value)
			{
				BitField0_ |= 0x00000002;
				BrokerServiceUrlTls_ = Value;

			}

			// optional .pulsar.proto.CommandLookupTopicResponse.LookupType response = 3;
			internal Types.LookupType Response_ = CommandLookupTopicResponse.Types.LookupType.Redirect;
			public bool HasResponse()
			{
				return ((BitField0_ & 0x00000004) == 0x00000004);
			}
			public Types.LookupType Response => Response_;

            public Builder SetResponse(Types.LookupType Value)
			{
				if (Value == null)
				{
					throw new NullReferenceException();
				}
				BitField0_ |= 0x00000004;
				Response_ = Value;

				return this;
			}
			public Builder ClearResponse()
			{
				BitField0_ = (BitField0_ & ~0x00000004);
				Response_ = CommandLookupTopicResponse.Types.LookupType.Redirect;

				return this;
			}

			// required uint64 request_id = 4;
			internal long RequestId_;
			public bool HasRequestId()
			{
				return ((BitField0_ & 0x00000008) == 0x00000008);
			}
			public long RequestId => RequestId_;

            public Builder SetRequestId(long Value)
			{
				BitField0_ |= 0x00000008;
				RequestId_ = Value;

				return this;
			}
			public Builder ClearRequestId()
			{
				BitField0_ = (BitField0_ & ~0x00000008);
				RequestId_ = 0L;

				return this;
			}

			// optional bool authoritative = 5 [default = false];
			internal bool Authoritative_;
			public bool HasAuthoritative()
			{
				return ((BitField0_ & 0x00000010) == 0x00000010);
			}
			public bool Authoritative => Authoritative_;

            public Builder SetAuthoritative(bool Value)
			{
				BitField0_ |= 0x00000010;
				Authoritative_ = Value;

				return this;
			}
			public Builder ClearAuthoritative()
			{
				BitField0_ = (BitField0_ & ~0x00000010);
				Authoritative_ = false;

				return this;
			}

			// optional .pulsar.proto.ServerError error = 6;
			internal ServerError Error_ = ServerError.UnknownError;
			public bool HasError()
			{
				return ((BitField0_ & 0x00000020) == 0x00000020);
			}
			public ServerError Error => Error_;

            public Builder SetError(ServerError Value)
			{
				if (Value == null)
				{
					throw new NullReferenceException();
				}
				BitField0_ |= 0x00000020;
				Error_ = Value;

				return this;
			}
			public Builder ClearError()
			{
				BitField0_ = (BitField0_ & ~0x00000020);
				Error_ = ServerError.UnknownError;

				return this;
			}

			// optional string message = 7;
			internal object Message_ = "";
			public bool HasMessage()
			{
				return ((BitField0_ & 0x00000040) == 0x00000040);
			}
			public string SetMessage()
			{
				object Ref = Message_;
				if (!(Ref is string))
				{
					string S = ((ByteString)Ref).ToStringUtf8();
					Message_ = S;
					return S;
				}
				else
				{
					return (string)Ref;
				}
			}
			public Builder SetMessage(string Value)
			{
				if (string.ReferenceEquals(Value, null))
				{
					throw new NullReferenceException();
				}
				BitField0_ |= 0x00000040;
				Message_ = Value;

				return this;
			}
			public Builder ClearMessage()
			{
				BitField0_ = (BitField0_ & ~0x00000040);
				Message_ = DefaultInstance.Message;

				return this;
			}
			public void SetMessage(ByteString Value)
			{
				BitField0_ |= 0x00000040;
				Message_ = Value;

			}

			// optional bool proxy_through_service_url = 8 [default = false];
			internal bool ProxyThroughServiceUrl_;
			public bool HasProxyThroughServiceUrl()
			{
				return ((BitField0_ & 0x00000080) == 0x00000080);
			}
			public bool ProxyThroughServiceUrl => ProxyThroughServiceUrl_;

            public Builder SetProxyThroughServiceUrl(bool Value)
			{
				BitField0_ |= 0x00000080;
				ProxyThroughServiceUrl_ = Value;

				return this;
			}
			public Builder ClearProxyThroughServiceUrl()
			{
				BitField0_ = (BitField0_ & ~0x00000080);
				ProxyThroughServiceUrl_ = false;

				return this;
			}

			// @@protoc_insertion_point(builder_scope:pulsar.proto.CommandLookupTopicResponse)
		}

		static CommandLookupTopicResponse()
		{
			_defaultInstance = new CommandLookupTopicResponse(true);
			_defaultInstance.InitFields();
		}

		// @@protoc_insertion_point(class_scope:pulsar.proto.CommandLookupTopicResponse)
	}

}
