using DotNetty.Common;
using Google.Protobuf;
using System;
using System.Linq;
using SharpPulsar.Utility.Protobuf;
using static SharpPulsar.Utility.Protobuf.ByteBufCodedInputStream;
using static SharpPulsar.Utility.Protobuf.ByteBufCodedOutputStream;

namespace SharpPulsar.Protocol.Proto
{
	public partial class CommandConnect: ByteBufGeneratedMessage
	{
		// Use CommandConnect.newBuilder() to construct.
		internal static ThreadLocalPool<CommandConnect> _pool = new ThreadLocalPool<CommandConnect>(handle => new CommandConnect(handle), 1, true);

		internal ThreadLocalPool.Handle _handle;
		private CommandConnect(ThreadLocalPool.Handle handle)
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

		public CommandConnect(bool NoInit)
		{
		}

		//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		internal static readonly CommandConnect _defaultInstance;
		public static CommandConnect DefaultInstance => _defaultInstance;

        public CommandConnect DefaultInstanceForType => _defaultInstance;

        public void InitFields()
		{
			ClientVersion = "";
			AuthMethod = AuthMethod.None;
			AuthMethodName = "";
			AuthData =  ByteString.Empty;
			ProtocolVersion= 0;
			ProxyToBrokerUrl = "";
			OriginalPrincipal = "";
			OriginalAuthData = "";
			OriginalAuthMethod = "";
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

				if (!HasClientVersion)
				{
					MemoizedIsInitialized = 0;
					return false;
				}
				MemoizedIsInitialized = 1;
				return true;
			}
		}

		public int SerializedSize => CalculateSize();

		internal int MemoizedSerializedSize = -1;
		
		internal const long SerialVersionUID = 0L;
		
		public static Builder NewBuilder()
		{
			return Builder.Create();
		}
		public Builder NewBuilderForType()
		{
			return NewBuilder();
		}
		public static Builder NewBuilder(CommandConnect prototype)
		{
			return NewBuilder().MergeFrom(prototype);
		}
		public Builder ToBuilder()
		{
			return NewBuilder(this);
		}
		public void WriteTo(ByteBufCodedOutputStream output)
		{
			var _ = SerializedSize;
			if (((_hasBits0 & 0x00000001) == 0x00000001))
			{
				output.WriteBytes(1, ByteString.CopyFromUtf8(ClientVersion));
			}
			if (((_hasBits0 & 0x00000002) == 0x00000002))
			{
				output.WriteEnum(2, (int)AuthMethod);
			}
			if (((_hasBits0 & 0x00000008) == 0x00000008))
			{
				output.WriteBytes(3, authData_);
			}
			if (((_hasBits0 & 0x00000010) == 0x00000010))
			{
				output.WriteInt32(4, ProtocolVersion);
			}
			if (((_hasBits0 & 0x00000004) == 0x00000004))
			{
				output.WriteBytes(5, ByteString.CopyFromUtf8(AuthMethodName));
			}
			if (((_hasBits0 & 0x00000020) == 0x00000020))
			{
				output.WriteBytes(6, ByteString.CopyFromUtf8(ProxyToBrokerUrl));
			}
			if (((_hasBits0 & 0x00000040) == 0x00000040))
			{
				output.WriteBytes(7, ByteString.CopyFromUtf8(OriginalPrincipal));
			}
			if (((_hasBits0 & 0x00000080) == 0x00000080))
			{
				output.WriteBytes(8, ByteString.CopyFromUtf8(OriginalAuthData));
			}
			if (((_hasBits0 & 0x00000100) == 0x00000100))
			{
				output.WriteBytes(9, ByteString.CopyFromUtf8(OriginalAuthMethod));
			}
		}

		public sealed class Builder : ByteBufMessageBuilder
		{
			internal static ThreadLocalPool<Builder> _pool = new ThreadLocalPool<Builder>(handle => new Builder(handle), 1, true);

			internal ThreadLocalPool.Handle _handle;
			private Builder(ThreadLocalPool.Handle handle)
			{
				_handle = handle;
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
				_clientVersion = "";
				_bitField = (_bitField & ~0x00000001);
				_authMethod = AuthMethod.None;
				_bitField = (_bitField & ~0x00000002);
				_authMethodName = "";
				_bitField = (_bitField & ~0x00000004);
				_authData = ByteString.Empty;
				_bitField = (_bitField & ~0x00000008);
				_protocolVersion = 0;
				_bitField = (_bitField & ~0x00000010);
				_proxyToBrokerUrl = "";
				_bitField = (_bitField & ~0x00000020);
				_originalPrincipal = "";
				_bitField = (_bitField & ~0x00000040);
				Original_authData = "";
				_bitField = (_bitField & ~0x00000080);
				Original_authMethod = "";
				_bitField = (_bitField & ~0x00000100);
				return this;
			}

			

			public CommandConnect DefaultInstanceForType => DefaultInstance;

            public CommandConnect Build()
			{
				var result = BuildPartial();
				
				return result;
			}

			
			public CommandConnect BuildPartial()
			{
				var result = CommandConnect._pool.Take();
				int frombitField = _bitField;
				int tobitField = 0;
				if (((frombitField & 0x00000001) == 0x00000001))
				{
					tobitField |= 0x00000001;
				}
				result.ClientVersion = _clientVersion;
				if (((frombitField & 0x00000002) == 0x00000002))
				{
					tobitField |= 0x00000002;
				}
				result.AuthMethod = _authMethod;
				if (((frombitField & 0x00000004) == 0x00000004))
				{
					tobitField |= 0x00000004;
				}
				result.AuthMethodName = _authMethodName;
				if (((frombitField & 0x00000008) == 0x00000008))
				{
					tobitField |= 0x00000008;
				}
				result.AuthData = _authData;
				if (((frombitField & 0x00000010) == 0x00000010))
				{
					tobitField |= 0x00000010;
				}
				result.ProtocolVersion = _protocolVersion;
				if (((frombitField & 0x00000020) == 0x00000020))
				{
					tobitField |= 0x00000020;
				}
				result.ProxyToBrokerUrl = _proxyToBrokerUrl;
				if (((frombitField & 0x00000040) == 0x00000040))
				{
					tobitField |= 0x00000040;
				}
				result.OriginalPrincipal = _originalPrincipal;
				if (((frombitField & 0x00000080) == 0x00000080))
				{
					tobitField |= 0x00000080;
				}
				result.OriginalAuthData = Original_authData;
				if (((frombitField & 0x00000100) == 0x00000100))
				{
					tobitField |= 0x00000100;
				}
				result.OriginalAuthMethod = Original_authMethod;
				result._hasBits0 = tobitField;
				return result;
			}

			public Builder MergeFrom(CommandConnect Other)
			{
				if (Other ==  DefaultInstance)
				{
					return this;
				}
				if (Other.HasClientVersion)
				{
					SetClientVersion(Other.ClientVersion);
				}
				if (Other.HasAuthMethod)
				{
					SetAuthMethod(Other.AuthMethod);
				}
				if (Other.HasAuthMethodName)
				{
					SetAuthMethodName(Other.AuthMethodName);
				}
				if (Other.HasAuthData)
				{
					SetAuthData(Other.AuthData);
				}
				if (Other.HasProtocolVersion)
				{
					SetProtocolVersion(Other.ProtocolVersion);
				}
				if (Other.HasProxyToBrokerUrl)
				{
					SetProxyToBrokerUrl(Other.ProxyToBrokerUrl);
				}
				if (Other.HasOriginalPrincipal)
				{
					SetOriginalPrincipal(Other.OriginalPrincipal);
				}
				if (Other.HasOriginalAuthData)
				{
					SetOriginalAuthData(Other.OriginalAuthData);
				}
				if (Other.HasOriginalAuthMethod)
				{
					SetOriginalAuthMethod(Other.OriginalAuthMethod);
				}
				return this;
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
								_bitField |= 0x00000001;
								_clientVersion = input.ReadBytes().ToStringUtf8();
								break;
							}
						case 16:
							{
								int RawValue = input.ReadEnum();
								AuthMethod Value = Enum.GetValues(typeof(AuthMethod)).Cast<AuthMethod>().ToList()[RawValue];
								if (Value != null)
								{
									_bitField |= 0x00000002;
									_authMethod = Value;
								}
								break;
							}
						case 26:
							{
								_bitField |= 0x00000008;
								_authData = input.ReadBytes();
								break;
							}
						case 32:
							{
								_bitField |= 0x00000010;
								_protocolVersion = input.ReadInt32();
								break;
							}
						case 42:
							{
								_bitField |= 0x00000004;
								_authMethodName = input.ReadBytes().ToStringUtf8();
								break;
							}
						case 50:
							{
								_bitField |= 0x00000020;
								_proxyToBrokerUrl = input.ReadBytes().ToStringUtf8();
								break;
							}
						case 58:
							{
								_bitField |= 0x00000040;
								_originalPrincipal = input.ReadBytes().ToStringUtf8();
								break;
							}
						case 66:
							{
								_bitField |= 0x00000080;
								Original_authData = input.ReadBytes().ToStringUtf8();
								break;
							}
						case 74:
							{
								_bitField |= 0x00000100;
								Original_authMethod = input.ReadBytes().ToStringUtf8();
								break;
							}
					}
				}
			}
			public bool Initialized
			{
				get
				{
					if (!HasClientVersion())
					{

						return false;
					}
					return true;
				}
			}

			internal int _bitField;

			// required string client_version = 1;
			private string _clientVersion = "";
			public bool HasClientVersion()
			{
				return ((_bitField & 0x00000001) == 0x00000001);
			}
			public string GetClientVersion()
			{
				return _clientVersion;
			}
			public Builder SetClientVersion(string Value)
			{
				if (string.ReferenceEquals(Value, null))
				{
					throw new NullReferenceException();
				}
				_bitField |= 0x00000001;
				_clientVersion = Value;

				return this;
			}
			public Builder ClearClientVersion()
			{
				_bitField = (_bitField & ~0x00000001);
				_clientVersion = DefaultInstance.ClientVersion;

				return this;
			}
			

			// optional .pulsar.proto.AuthMethod auth_method = 2;
			internal AuthMethod _authMethod = AuthMethod.None;
			public bool HasAuthMethod()
			{
				return ((_bitField & 0x00000002) == 0x00000002);
			}
			
			public Builder SetAuthMethod(AuthMethod Value)
			{
				if (Value == null)
				{
					throw new NullReferenceException();
				}
				_bitField |= 0x00000002;
				_authMethod = Value;

				return this;
			}
			public Builder ClearAuthMethod()
			{
				_bitField = (_bitField & ~0x00000002);
				_authMethod = AuthMethod.None;

				return this;
			}

			// optional string auth_method_name = 5;
			private string _authMethodName = "";
			public bool HasAuthMethodName()
			{
				return ((_bitField & 0x00000004) == 0x00000004);
			}
			public string GetAuthMethodName()
			{
				return _authMethodName;
			}
			public Builder SetAuthMethodName(string Value)
			{
				if (string.ReferenceEquals(Value, null))
				{
					throw new NullReferenceException();
				}
				_bitField |= 0x00000004;
				_authMethodName = Value;

				return this;
			}
			public Builder ClearAuthMethodName()
			{
				_bitField = (_bitField & ~0x00000004);
				_authMethodName = DefaultInstance.AuthMethodName;

				return this;
			}
			

			// optional bytes auth_data = 3;
			internal ByteString _authData = ByteString.Empty;
			public bool HasAuthData()
			{
				return ((_bitField & 0x00000008) == 0x00000008);
			}
			
			public Builder SetAuthData(ByteString value)
			{
				if (value == null)
				{
					throw new NullReferenceException();
				}
				_bitField |= 0x00000008;
				_authData = value;

				return this;
			}
			public Builder ClearAuthData()
			{
				_bitField = (_bitField & ~0x00000008);
				_authData = DefaultInstance.AuthData;

				return this;
			}

			// optional int32 protocol_version = 4 [default = 0];
			internal int _protocolVersion;
			public bool HasProtocolVersion()
			{
				return ((_bitField & 0x00000010) == 0x00000010);
			}
			public int ProtocolVersion => _protocolVersion;

            public Builder SetProtocolVersion(int Value)
			{
				_bitField |= 0x00000010;
				_protocolVersion = Value;

				return this;
			}
			public Builder ClearProtocolVersion()
			{
				_bitField = (_bitField & ~0x00000010);
				_protocolVersion = 0;

				return this;
			}

			// optional string proxy_to_broker_url = 6;
			private string _proxyToBrokerUrl = "";
			public bool HasProxyToBrokerUrl()
			{
				return ((_bitField & 0x00000020) == 0x00000020);
			}
			public string GetProxyToBrokerUrl()
			{
				return _proxyToBrokerUrl;
			}
			public Builder SetProxyToBrokerUrl(string Value)
			{
				if (string.ReferenceEquals(Value, null))
				{
					throw new NullReferenceException();
				}
				_bitField |= 0x00000020;
				_proxyToBrokerUrl = Value;

				return this;
			}
			public Builder ClearProxyToBrokerUrl()
			{
				_bitField = (_bitField & ~0x00000020);
				_proxyToBrokerUrl = DefaultInstance.ProxyToBrokerUrl;

				return this;
			}
			
			// optional string original_principal = 7;
			private string _originalPrincipal = "";
			public bool HasOriginalPrincipal()
			{
				return ((_bitField & 0x00000040) == 0x00000040);
			}
			public string GetOriginalPrincipal()
			{
				return _originalPrincipal;
			}
			public Builder SetOriginalPrincipal(string Value)
			{
				if (string.ReferenceEquals(Value, null))
				{
					throw new NullReferenceException();
				}
				_bitField |= 0x00000040;
				_originalPrincipal = Value;

				return this;
			}
			public Builder ClearOriginalPrincipal()
			{
				_bitField = (_bitField & ~0x00000040);
				_originalPrincipal = DefaultInstance.OriginalPrincipal;

				return this;
			}
			
			// optional string original_auth_data = 8;
			private string  Original_authData = "";
			public bool HasOriginalAuthData()
			{
				return ((_bitField & 0x00000080) == 0x00000080);
			}
			public string GetOriginalAuthData()
			{
				return Original_authData;
			}
			public Builder SetOriginalAuthData(string Value)
			{
				if (string.ReferenceEquals(Value, null))
				{
					throw new NullReferenceException();
				}
				_bitField |= 0x00000080;
				Original_authData = Value;

				return this;
			}
			public Builder ClearOriginalAuthData()
			{
				_bitField = (_bitField & ~0x00000080);
				Original_authData = DefaultInstance.OriginalAuthData;

				return this;
			}
			
			// optional string original_auth_method = 9;
			private string Original_authMethod = "";
			public bool HasOriginalAuthMethod()
			{
				return ((_bitField & 0x00000100) == 0x00000100);
			}
			public string GetOriginalAuthMethod()
			{
				return Original_authMethod;
			}
			public Builder SetOriginalAuthMethod(string Value)
			{
				if (string.ReferenceEquals(Value, null))
				{
					throw new NullReferenceException();
				}
				_bitField |= 0x00000100;
				Original_authMethod = Value;

				return this;
			}
			public Builder ClearOriginalAuthMethod()
			{
				_bitField = (_bitField & ~0x00000100);
				Original_authMethod = DefaultInstance.OriginalAuthMethod;

				return this;
			}
			

			// @@protoc_insertion_point(builder_scope:pulsar.proto.CommandConnect)
		}

		static CommandConnect()
		{
			_defaultInstance = new CommandConnect(true);
			_defaultInstance.InitFields();
		}

		// @@protoc_insertion_point(class_scope:pulsar.proto.CommandConnect)
	}


}
