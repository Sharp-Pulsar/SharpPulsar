using DotNetty.Common;
using Google.Protobuf;
using SharpPulsar.Util.Protobuf;
using System;
using System.IO;
using System.Text;
using static SharpPulsar.Common.Proto.Api.PulsarApi;

namespace SharpPulsar.Common.Proto.Extension
{
	public partial class CommandConnectw : CommandConnectOrBuilder, ByteBufCodedOutputStream.ByteBufGeneratedMessage
	{
		private static ThreadLocalPool<CommandConnect> pool = new ThreadLocalPool<CommandConnect>(handle => new CommandConnect(handle),1, true);
		// Use CommandConnect.NewBuilder() to construct.
		internal ThreadLocalPool.Handle _handle;
		internal CommandConnect(ThreadLocalPool.Handle handle)
		{
			_handle = handle;
		}
				
		public void Recycle()
		{
			InitFields();
			_memoizedIsInitialized = -1;
			_bitField = 0;
			this.memoizedSerializedSize = -1;
			if (_handle != null)
			{
				_handle.Release(this);
			}
		}

		internal CommandConnect(bool noInit)
		{
		}

		internal static readonly CommandConnect defaultInstance;
		public static CommandConnect DefaultInstance
		{
			get
			{
				return defaultInstance;
			}
		}

		public CommandConnect DefaultInstanceForType
		{
			get
			{
				return defaultInstance;
			}
		}

		internal int _bitField;
		// required string client_version = 1;
		public const int CLIENT_VERSION_FIELD_NUMBER = 1;
		internal object _clientVersion;
		public bool HasClientVersion()
		{
			return ((_bitField & 0x00000001) == 0x00000001);
		}
		public string ClientVersion
		{
			get
			{
				object @ref = _clientVersion;
				if (@ref is string)
				{
					return (string)@ref;
				}
				else
				{
					ByteString bs = (ByteString)@ref;
					string s = bs.ToStringUtf8();
					if (IsValidUtf8(bs))
					{
						_clientVersion = s;
					}
					return s;
				}
			}
		}

		private bool IsValidUtf8(ByteString bs)
		{
			throw new NotImplementedException();
		}

		internal ByteString ClientVersionBytes
		{
			get
			{
				object @ref = _clientVersion;
				if (@ref is string)
				{
					ByteString b = ByteString.CopyFromUtf8((string)@ref);
					_clientVersion= b;
					return b;
				}
				else
				{
					return (ByteString)@ref;
				}
			}
		}

		// optional .pulsar.proto.AuthMethod auth_method = 2;
		public const int AUTH_METHOD_FIELD_NUMBER = 2;
		internal AuthMethod _authMethod;
		public bool HasAuthMethod()
		{
			return ((_bitField & 0x00000002) == 0x00000002);
		}
		public AuthMethod AuthMethod
		{
			get
			{
				return _authMethod;
			}
		}

		// optional string auth_method_name = 5;
		public const int AUTH_METHOD_NAME_FIELD_NUMBER = 5;
		internal object _authMethodName;
		public bool HasAuthMethodName()
		{
			return ((_bitField & 0x00000004) == 0x00000004);
		}
		public string AuthMethodName
		{
			get
			{
				object @ref = _authMethodName;
				if (@ref is string)
				{
					return (string)@ref;
				}
				else
				{
					ByteString bs = (ByteString)@ref;
					string s = bs.ToStringUtf8();
					if (IsValidUtf8(bs))
					{
						_authMethodName= s;
					}
					return s;
				}
			}
		}
		internal ByteString AuthMethodNameBytes
		{
			get
			{
				object @ref = _authMethodName;
				if (@ref is string)
				{
					ByteString b = ByteString.CopyFromUtf8((string)@ref);
					_authMethodName = b;
					return b;
				}
				else
				{
					return (ByteString)@ref;
				}
			}
		}

		// optional bytes auth_data = 3;
		public const int AUTH_DATA_FIELD_NUMBER = 3;
		internal ByteString _authData;
		public bool HasAuthData()
		{
			return ((_bitField & 0x00000008) == 0x00000008);
		}
		public ByteString AuthData
		{
			get
			{
				return _authData;
			}
		}

		// optional int32 protocol_version = 4 [default = 0];
		public const int PROTOCOL_VERSION_FIELD_NUMBER = 4;
		internal int _protocolVersion;
		public bool HasProtocolVersion()
		{
			return ((_bitField & 0x00000010) == 0x00000010);
		}
		public int ProtocolVersion
		{
			get
			{
				return _protocolVersion;
			}
		}

		// optional string proxy_to_broker_url = 6;
		public const int PROXY_TO_BROKER_URL_FIELD_NUMBER = 6;
		internal object _proxyToBrokerUrl;
		public bool HasProxyToBrokerUrl()
		{
			return ((_bitField & 0x00000020) == 0x00000020);
		}
		public string ProxyToBrokerUrl
		{
			get
			{
				object @ref = _proxyToBrokerUrl;
				if (@ref is string)
				{
					return (string)@ref;
				}
				else
				{
					ByteString bs = (ByteString)@ref;
					string s = bs.ToStringUtf8();
					if (IsValidUtf8(bs))
					{
						_proxyToBrokerUrl = s;
					}
					return s;
				}
			}
		}
		internal ByteString ProxyToBrokerUrlBytes
		{
			get
			{
				object @ref = _proxyToBrokerUrl;
				if (@ref is string)
				{
					ByteString b = ByteString.CopyFromUtf8((string)@ref);
					_proxyToBrokerUrl = b;
					return b;
				}
				else
				{
					return (ByteString)@ref;
				}
			}
		}

		// optional string original_principal = 7;
		public const int ORIGINAL_PRINCIPAL_FIELD_NUMBER = 7;
		internal object _originalPrincipal;
		public bool HasOriginalPrincipal()
		{
			return ((_bitField & 0x00000040) == 0x00000040);
		}
		public string OriginalPrincipal
		{
			get
			{
				object @ref = _originalPrincipal;
				if (@ref is string)
				{
					return (string)@ref;
				}
				else
				{
					ByteString bs = (ByteString)@ref;
					string s = bs.ToStringUtf8();
					if (IsValidUtf8(bs))
					{
						_originalPrincipal = s;
					}
					return s;
				}
			}
		}
		internal ByteString OriginalPrincipalBytes
		{
			get
			{
				object @ref = _originalPrincipal;
				if (@ref is string)
				{
					ByteString b = ByteString.CopyFromUtf8((string)@ref);
					_originalPrincipal = b;
					return b;
				}
				else
				{
					return (ByteString)@ref;
				}
			}
		}

		// optional string original_auth_data = 8;
		public const int ORIGINAL_AUTH_DATA_FIELD_NUMBER = 8;
		internal object original_authData;
		public bool HasOriginalAuthData()
		{
			return ((_bitField & 0x00000080) == 0x00000080);
		}
		public string OriginalAuthData
		{
			get
			{
				object @ref = original_authData;
				if (@ref is string)
				{
					return (string)@ref;
				}
				else
				{
					ByteString bs = (ByteString)@ref;
					string s = bs.ToStringUtf8();
					if (IsValidUtf8(bs))
					{
						original_authData = s;
					}
					return s;
				}
			}
		}
		internal ByteString OriginalAuthDataBytes
		{
			get
			{
				object @ref = original_authData;
				if (@ref is string)
				{
					ByteString b = ByteString.CopyFromUtf8((string)@ref);
					original_authData = b;
					return b;
				}
				else
				{
					return (ByteString)@ref;
				}
			}
		}

		// optional string original_auth_method = 9;
		public const int ORIGINAL_AUTH_METHOD_FIELD_NUMBER = 9;
		internal object original_authMethod;
		public bool HasOriginalAuthMethod()
		{
			return ((_bitField & 0x00000100) == 0x00000100);
		}
		public string OriginalAuthMethod
		{
			get
			{
				object @ref = original_authMethod;
				if (@ref is string)
				{
					return (string)@ref;
				}
				else
				{
					ByteString bs = (ByteString)@ref;
					string s = bs.ToStringUtf8();
					if (IsValidUtf8(bs))
					{
						original_authMethod = s;
					}
					return s;
				}
			}
		}
		internal ByteString OriginalAuthMethodBytes
		{
			get
			{
				object @ref = original_authMethod;
				if (@ref is string)
				{
					ByteString b = ByteString.CopyFromUtf8((string)@ref);
					original_authMethod = b;
					return b;
				}
				else
				{
					return (ByteString)@ref;
				}
			}
		}

		internal void InitFields()
		{
			_clientVersion = "";
			_authMethod = AuthMethod.AuthMethodNone;
			_authMethodName = "";
			_authData = ByteString.Empty;
			_protocolVersion = 0;
			_proxyToBrokerUrl = "";
			_originalPrincipal = "";
			original_authData = "";
			original_authMethod = "";
		}
		internal sbyte _memoizedIsInitialized = -1;
		public bool Initialized
		{
			get
			{
				sbyte isInitialized = _memoizedIsInitialized;
				if (isInitialized != -1)
				{
					return isInitialized == 1;
				}

				if (!HasClientVersion())
				{
					_memoizedIsInitialized = 0;
					return false;
				}
				_memoizedIsInitialized = 1;
				return true;
			}
		}
		public void WriteTo(CodedOutputStream output)
		{
			throw new System.Exception("Cannot use CodedOutputStream");
		}

		public void WriteTo(ByteBufCodedOutputStream output)
		{
			//SerializedSize;
			if (((_bitField & 0x00000001) == 0x00000001))
			{
				output.WriteBytes(1, ClientVersionBytes);
			}
			if (((_bitField & 0x00000002) == 0x00000002))
			{
				output.WriteEnum(2, _authMethod.Number);
			}
			if (((_bitField & 0x00000008) == 0x00000008))
			{
				output.WriteBytes(3, _authData);
			}
			if (((_bitField & 0x00000010) == 0x00000010))
			{
				output.WriteInt32(4, _protocolVersion);
			}
			if (((_bitField & 0x00000004) == 0x00000004))
			{
				output.WriteBytes(5, AuthMethodNameBytes);
			}
			if (((_bitField & 0x00000020) == 0x00000020))
			{
				output.WriteBytes(6, ProxyToBrokerUrlBytes);
			}
			if (((_bitField & 0x00000040) == 0x00000040))
			{
				output.WriteBytes(7, OriginalPrincipalBytes);
			}
			if (((_bitField & 0x00000080) == 0x00000080))
			{
				output.WriteBytes(8, OriginalAuthDataBytes);
			}
			if (((_bitField & 0x00000100) == 0x00000100))
			{
				output.WriteBytes(9, OriginalAuthMethodBytes);
			}
		}

		internal int memoizedSerializedSize = -1;
		public int SerializedSize
		{
			get
			{
				int size = memoizedSerializedSize;
				if (size != -1)
				{
					return size;
				}

				size = 0;
				if (((_bitField & 0x00000001) == 0x00000001))
				{
					size += CodedOutputStream.ComputeBytesSize(ClientVersionBytes);
				}
				if (((_bitField & 0x00000002) == 0x00000002))
				{
					size += CodedOutputStream.ComputeEnumSize(_authMethod.Number);
				}
				if (((_bitField & 0x00000008) == 0x00000008))
				{
					size += CodedOutputStream.ComputeBytesSize(_authData);
				}
				if (((_bitField & 0x00000010) == 0x00000010))
				{
					size += CodedOutputStream.ComputeInt32Size(_protocolVersion);
				}
				if (((_bitField & 0x00000004) == 0x00000004))
				{
					size += CodedOutputStream.ComputeBytesSize(AuthMethodNameBytes);
				}
				if (((_bitField & 0x00000020) == 0x00000020))
				{
					size += CodedOutputStream.ComputeBytesSize(ProxyToBrokerUrlBytes);
				}
				if (((_bitField & 0x00000040) == 0x00000040))
				{
					size += CodedOutputStream.ComputeBytesSize(OriginalPrincipalBytes);
				}
				if (((_bitField & 0x00000080) == 0x00000080))
				{
					size += CodedOutputStream.ComputeBytesSize(OriginalAuthDataBytes);
				}
				if (((_bitField & 0x00000100) == 0x00000100))
				{
					size += CodedOutputStream.ComputeBytesSize(OriginalAuthMethodBytes);
				}
				memoizedSerializedSize = size;
				return size;
			}
		}

		internal const long serialVersionUID = 0L;
		public static CommandConnect ParseFrom(ByteString data)
		{
			throw new System.Exception("Disabled");
		}
		public static CommandConnect ParseFrom(ByteString data, ExtensionRegistry extensionRegistry)
		{
			throw new System.Exception("Disabled");
		}
		
		public static CommandConnect ParseFrom(CodedInputStream input)
		{
			return NewBuilder().MergeFrom(input).BuildParsed();
		}
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

		

		static CommandConnect()
		{
			defaultInstance = new CommandConnect(true);
			defaultInstance.InitFields();
		}

		// @@protoc_insertion_point(class_scope:pulsar.proto.CommandConnect)
	}
	public sealed class Builder : CommandConnect
	{
		// Construct using CommandConnect.NewBuilder()
		private static ThreadLocalPool<Builder> _pool = new ThreadLocalPool<Builder>(handle => new Builder(handle), 1, true);
		// Use CommandConnect.NewBuilder() to construct.
		internal ThreadLocalPool.Handle _handle;
		internal Builder(ThreadLocalPool.Handle handle)
		{
			_handle = handle;
			MaybeForceBuilderInitialization();
		}
		public Builder()
		{

		}
		public void Recycle()
		{
			Clear();
			if (_handle != null)
			{
				_handle.Release(this);
			}
		}

		internal void MaybeForceBuilderInitialization()
		{
		}
		internal static Builder Create()
		{
			return _pool.Take();
		}

		public Builder Clear()
		{
			Clear();
			clientVersion_ = "";
			_bitField = (_bitField & ~0x00000001);
			_authMethod = AuthMethod.AuthMethodNone;
			_bitField = (_bitField & ~0x00000002);
			authMethodName_ = "";
			_bitField = (_bitField & ~0x00000004);
			_authData = ByteString.Empty;
			_bitField = (_bitField & ~0x00000008);
			_protocolVersion = 0;
			_bitField = (_bitField & ~0x00000010);
			_proxyToBrokerUrl = "";
			_bitField = (_bitField & ~0x00000020);
			_originalPrincipal = "";
			_bitField = (_bitField & ~0x00000040);
			original_authData = "";
			_bitField = (_bitField & ~0x00000080);
			original_authMethod = "";
			_bitField = (_bitField & ~0x00000100);
			return this;
		}

		public Builder Clone()
		{
			return Create().MergeFrom(BuildPartial());
		}

		public CommandConnect DefaultInstanceForType
		{
			get
			{
				return DefaultInstance;
			}
		}

		public CommandConnect Build()
		{
			CommandConnect result = BuildPartial();
			if (!result.Initialized)
			{
				throw new IOException("CommandConnect is null", new NullReferenceException());
			}
			return result;
		}
		internal CommandConnect BuildParsed()
		{
			CommandConnect result = BuildPartial();
			if (!result.Initialized)
			{
				throw new IOException("CommandConnect is null", new NullReferenceException());
				//throw new UninitializedMessageException(result).asInvalidProtocolBufferException();
			}
			return result;
		}

		public CommandConnect BuildPartial()
		{
			CommandConnect result = pool.Take();
			int from__bitField = _bitField;
			int to__bitField = 0;
			if (((from__bitField & 0x00000001) == 0x00000001))
			{
				to__bitField |= 0x00000001;
			}
			result._clientVersion = clientVersion_;
			if (((from__bitField & 0x00000002) == 0x00000002))
			{
				to__bitField |= 0x00000002;
			}
			result._authMethod = _authMethod;
			if (((from__bitField & 0x00000004) == 0x00000004))
			{
				to__bitField |= 0x00000004;
			}
			result._authMethodName = authMethodName_;
			if (((from__bitField & 0x00000008) == 0x00000008))
			{
				to__bitField |= 0x00000008;
			}
			result._authData = _authData;
			if (((from__bitField & 0x00000010) == 0x00000010))
			{
				to__bitField |= 0x00000010;
			}
			result._protocolVersion = _protocolVersion;
			if (((from__bitField & 0x00000020) == 0x00000020))
			{
				to__bitField |= 0x00000020;
			}
			result._proxyToBrokerUrl = _proxyToBrokerUrl;
			if (((from__bitField & 0x00000040) == 0x00000040))
			{
				to__bitField |= 0x00000040;
			}
			result._originalPrincipal = _originalPrincipal;
			if (((from__bitField & 0x00000080) == 0x00000080))
			{
				to__bitField |= 0x00000080;
			}
			result.original_authData = original_authData;
			if (((from__bitField & 0x00000100) == 0x00000100))
			{
				to__bitField |= 0x00000100;
			}
			result.original_authMethod = original_authMethod;
			result._bitField = to__bitField;
			return result;
		}

		public Builder MergeFrom(CommandConnect other)
		{
			if (other == DefaultInstance)
			{
				return this;
			}
			if (other.HasClientVersion())
			{
				SetClientVersion(other.ClientVersion);
			}
			if (other.HasAuthMethod())
			{
				SetAuthMethod(other.AuthMethod);
			}
			if (other.HasAuthMethodName())
			{
				SetAuthMethodName(other.AuthMethodName);
			}
			if (other.HasAuthData())
			{
				SetAuthData(other.AuthData);
			}
			if (other.HasProtocolVersion())
			{
				SetProtocolVersion(other.ProtocolVersion);
			}
			if (other.HasProxyToBrokerUrl())
			{
				SetProxyToBrokerUrl(other.ProxyToBrokerUrl);
			}
			if (other.HasOriginalPrincipal())
			{
				SetOriginalPrincipal(other.OriginalPrincipal);
			}
			if (other.HasOriginalAuthData())
			{
				SetOriginalAuthData(other.OriginalAuthData);
			}
			if (other.HasOriginalAuthMethod())
			{
				SetOriginalAuthMethod(other.OriginalAuthMethod);
			}
			return this;
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

		public Builder MergeFrom(CodedInputStream input)
		{
			throw new System.Exception("Merge from CodedInputStream is disabled");
		}
		public Builder MergeFrom(ByteBufCodedInputStream input)
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
							_bitField |= 0x00000001;
							clientVersion_ = input.ReadBytes();
							break;
						}
					case 16:
						{
							int rawValue = input.ReadEnum();
							AuthMethod value = AuthMethod.ValueOf(rawValue);
							if (value != null)
							{
								_bitField |= 0x00000002;
								_authMethod = value;
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
							authMethodName_ = input.ReadBytes();
							break;
						}
					case 50:
						{
							_bitField |= 0x00000020;
							_proxyToBrokerUrl = input.ReadBytes();
							break;
						}
					case 58:
						{
							_bitField |= 0x00000040;
							_originalPrincipal = input.ReadBytes();
							break;
						}
					case 66:
						{
							_bitField |= 0x00000080;
							original_authData = input.ReadBytes();
							break;
						}
					case 74:
						{
							_bitField |= 0x00000100;
							original_authMethod = input.ReadBytes();
							break;
						}
				}
			}
		}

		internal int _bitField;

		// required string client_version = 1;
		internal object clientVersion_ = "";
		public bool HasClientVersion()
		{
			return ((_bitField & 0x00000001) == 0x00000001);
		}
		public string GetClientVersion()
		{
			object @ref = clientVersion_;
			if (!(@ref is string))
			{
				string s = ((ByteString)@ref).ToStringUtf8();
				clientVersion_ = s;
				return s;
			}
			else
			{
				return (string)@ref;
			}
		}
		public Builder SetClientVersion(string value)
		{
			if (value is null)
			{
				throw new NullReferenceException();
			}
			_bitField |= 0x00000001;
			clientVersion_ = value;

			return this;
		}
		public Builder ClearClientVersion()
		{
			_bitField = (_bitField & ~0x00000001);
			clientVersion_ = DefaultInstance.ClientVersion;

			return this;
		}
		internal void SetClientVersion(ByteString value)
		{
			_bitField |= 0x00000001;
			clientVersion_ = value;

		}

		// optional .pulsar.proto.AuthMethod auth_method = 2;
		internal AuthMethod _authMethod = AuthMethod.AuthMethodNone;
		public bool HasAuthMethod()
		{
			return ((_bitField & 0x00000002) == 0x00000002);
		}
		public AuthMethod AuthMethod
		{
			get
			{
				return _authMethod;
			}
		}
		public Builder SetAuthMethod(AuthMethod value)
		{
			if (value == null)
			{
				throw new NullReferenceException();
			}
			_bitField |= 0x00000002;
			_authMethod = value;

			return this;
		}
		public Builder ClearAuthMethod()
		{
			_bitField = (_bitField & ~0x00000002);
			_authMethod = AuthMethod.AuthMethodNone;

			return this;
		}

		// optional string auth_method_name = 5;
		internal object authMethodName_ = "";
		public bool HasAuthMethodName()
		{
			return ((_bitField & 0x00000004) == 0x00000004);
		}
		public string GetAuthMethodName()
		{
			object @ref = authMethodName_;
			if (!(@ref is string))
			{
				string s = ((ByteString)@ref).ToStringUtf8();
				authMethodName_ = s;
				return s;
			}
			else
			{
				return (string)@ref;
			}
		}
		public Builder SetAuthMethodName(string value)
		{
			if (value is null)
			{
				throw new NullReferenceException();
			}
			_bitField |= 0x00000004;
			authMethodName_ = value;

			return this;
		}
		public Builder ClearAuthMethodName()
		{
			_bitField = (_bitField & ~0x00000004);
			authMethodName_ = DefaultInstance.AuthMethodName;

			return this;
		}
		internal void SetAuthMethodName(ByteString value)
		{
			_bitField |= 0x00000004;
			authMethodName_ = value;

		}

		// optional bytes auth_data = 3;
		internal ByteString _authData = ByteString.Empty;
		public bool HasAuthData()
		{
			return ((_bitField & 0x00000008) == 0x00000008);
		}
		public ByteString AuthData
		{
			get
			{
				return _authData;
			}
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
		public int ProtocolVersion
		{
			get
			{
				return _protocolVersion;
			}
		}
		public Builder SetProtocolVersion(int value)
		{
			_bitField |= 0x00000010;
			_protocolVersion = value;

			return this;
		}
		public Builder ClearProtocolVersion()
		{
			_bitField = (_bitField & ~0x00000010);
			_protocolVersion = 0;

			return this;
		}

		// optional string proxy_to_broker_url = 6;
		internal object _proxyToBrokerUrl = "";
		public bool HasProxyToBrokerUrl()
		{
			return ((_bitField & 0x00000020) == 0x00000020);
		}
		public string GetProxyToBrokerUrl()
		{
			object @ref = _proxyToBrokerUrl;
			if (!(@ref is string))
			{
				string s = ((ByteString)@ref).ToStringUtf8();
				_proxyToBrokerUrl = s;
				return s;
			}
			else
			{
				return (string)@ref;
			}
		}
		public Builder SetProxyToBrokerUrl(string value)
		{
			if (value is null)
			{
				throw new NullReferenceException();
			}
			_bitField |= 0x00000020;
			_proxyToBrokerUrl = value;

			return this;
		}
		public Builder ClearProxyToBrokerUrl()
		{
			_bitField = (_bitField & ~0x00000020);
			_proxyToBrokerUrl = DefaultInstance.ProxyToBrokerUrl;

			return this;
		}
		internal void SetProxyToBrokerUrl(ByteString value)
		{
			_bitField |= 0x00000020;
			_proxyToBrokerUrl = value;

		}

		// optional string original_principal = 7;
		internal object _originalPrincipal = "";
		public bool HasOriginalPrincipal()
		{
			return ((_bitField & 0x00000040) == 0x00000040);
		}
		public string GetOriginalPrincipal()
		{
			object @ref = _originalPrincipal;
			if (!(@ref is string))
			{
				string s = ((ByteString)@ref).ToStringUtf8();
				_originalPrincipal = s;
				return s;
			}
			else
			{
				return (string)@ref;
			}
		}
		public Builder SetOriginalPrincipal(string value)
		{
			if (value is null)
			{
				throw new NullReferenceException();
			}
			_bitField |= 0x00000040;
			_originalPrincipal = value;

			return this;
		}
		public Builder ClearOriginalPrincipal()
		{
			_bitField = (_bitField & ~0x00000040);
			_originalPrincipal = DefaultInstance.OriginalPrincipal;

			return this;
		}
		internal void SetOriginalPrincipal(ByteString value)
		{
			_bitField |= 0x00000040;
			_originalPrincipal = value;

		}

		// optional string original_auth_data = 8;
		internal object original_authData = "";
		public bool HasOriginalAuthData()
		{
			return ((_bitField & 0x00000080) == 0x00000080);
		}
		public string GetOriginalAuthData()
		{
			object @ref = original_authData;
			if (!(@ref is string))
			{
				string s = ((ByteString)@ref).ToStringUtf8();
				original_authData = s;
				return s;
			}
			else
			{
				return (string)@ref;
			}
		}
		public Builder SetOriginalAuthData(string value)
		{
			if (ReferenceEquals(value, null))
			{
				throw new NullReferenceException();
			}
			_bitField |= 0x00000080;
			original_authData = value;

			return this;
		}
		public Builder ClearOriginalAuthData()
		{
			_bitField = (_bitField & ~0x00000080);
			original_authData = DefaultInstance.OriginalAuthData;

			return this;
		}
		internal void SetOriginalAuthData(ByteString value)
		{
			_bitField |= 0x00000080;
			original_authData = value;

		}

		// optional string original_auth_method = 9;
		internal object original_authMethod = "";
		public bool HasOriginalAuthMethod()
		{
			return ((_bitField & 0x00000100) == 0x00000100);
		}
		public string GetOriginalAuthMethod()
		{
			object @ref = original_authMethod;
			if (!(@ref is string))
			{
				string s = ((ByteString)@ref).ToStringUtf8();
				original_authMethod = s;
				return s;
			}
			else
			{
				return (string)@ref;
			}
		}
		public Builder SetOriginalAuthMethod(string value)
		{
			if (value is null)
			{
				throw new NullReferenceException();
			}
			_bitField |= 0x00000100;
			original_authMethod = value;

			return this;
		}
		public Builder ClearOriginalAuthMethod()
		{
			_bitField = (_bitField & ~0x00000100);
			original_authMethod = DefaultInstance.OriginalAuthMethod;

			return this;
		}
		internal void SetOriginalAuthMethod(ByteString value)
		{
			_bitField |= 0x00000100;
			original_authMethod = value;

		}

		// @@protoc_insertion_point(builder_scope:pulsar.proto.CommandConnect)
	}
	public interface CommandConnectOrBuilder
	{

		// required string client_version = 1;
		bool HasClientVersion();
		string ClientVersion { get; }

		// optional .pulsar.proto.AuthMethod auth_method = 2;
		bool HasAuthMethod();
		AuthMethod AuthMethod { get; }

		// optional string auth_method_name = 5;
		bool HasAuthMethodName();
		string AuthMethodName { get; }

		// optional bytes auth_data = 3;
		bool HasAuthData();
		ByteString AuthData { get; }

		// optional int32 protocol_version = 4 [default = 0];
		bool HasProtocolVersion();
		int ProtocolVersion { get; }

		// optional string proxy_to_broker_url = 6;
		bool HasProxyToBrokerUrl();
		string ProxyToBrokerUrl { get; }

		// optional string original_principal = 7;
		bool HasOriginalPrincipal();
		string OriginalPrincipal { get; }

		// optional string original_auth_data = 8;
		bool HasOriginalAuthData();
		string OriginalAuthData { get; }

		// optional string original_auth_method = 9;
		bool HasOriginalAuthMethod();
		string OriginalAuthMethod { get; }
	}
}
