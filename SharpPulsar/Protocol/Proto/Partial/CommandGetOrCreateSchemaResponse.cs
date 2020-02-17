using DotNetty.Common;
using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpPulsar.Utility.Protobuf;
using static SharpPulsar.Utility.Protobuf.ByteBufCodedInputStream;

namespace SharpPulsar.Protocol.Proto
{
	public sealed partial class CommandGetOrCreateSchemaResponse : ByteBufCodedOutputStream.ByteBufGeneratedMessage
	{
		// Use CommandGetOrCreateSchemaResponse.newBuilder() to construct.
		internal static ThreadLocalPool<CommandGetOrCreateSchemaResponse> _pool = new ThreadLocalPool<CommandGetOrCreateSchemaResponse>(handle => new CommandGetOrCreateSchemaResponse(handle), 1, true);

		internal ThreadLocalPool.Handle _handle;
		private CommandGetOrCreateSchemaResponse(ThreadLocalPool.Handle handle)
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

		public CommandGetOrCreateSchemaResponse(bool NoInit)
		{
		}

		
		internal static readonly CommandGetOrCreateSchemaResponse _defaultInstance;
		public static CommandGetOrCreateSchemaResponse DefaultInstance => _defaultInstance;

        public CommandGetOrCreateSchemaResponse DefaultInstanceForType => _defaultInstance;


        public void InitFields()
		{
			RequestId = 0L;
			ErrorCode = ServerError.UnknownError;
			ErrorMessage = "";
			SchemaVersion = ByteString.Empty
				;
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
			var _ = SerializedSize;
			if (((_hasBits0 & 0x00000001) == 0x00000001))
			{
				Output.WriteUInt64(1, (long)RequestId);
			}
			if (((_hasBits0 & 0x00000002) == 0x00000002))
			{
				Output.WriteEnum(2, (int)ErrorCode);
			}
			if (((_hasBits0 & 0x00000004) == 0x00000004))
			{
				Output.WriteBytes(3, ByteString.CopyFromUtf8(ErrorMessage));
			}
			if (((_hasBits0 & 0x00000008) == 0x00000008))
			{
				Output.WriteBytes(4, SchemaVersion);
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
		public static Builder NewBuilder(CommandGetOrCreateSchemaResponse Prototype)
		{
			return NewBuilder().MergeFrom(Prototype);
		}
		public Builder ToBuilder()
		{
			return NewBuilder(this);
		}

		public sealed class Builder : ByteBufMessageBuilder
		{
			// Construct using org.apache.pulsar.common.api.proto.CommandGetOrCreateSchemaResponse.newBuilder()
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
				RequestId_ = 0L;
				BitField0_ = (BitField0_ & ~0x00000001);
				ErrorCode_ = ServerError.UnknownError;
				BitField0_ = (BitField0_ & ~0x00000002);
				ErrorMessage_ = "";
				BitField0_ = (BitField0_ & ~0x00000004);
				SchemaVersion_ = ByteString.Empty;
				BitField0_ = (BitField0_ & ~0x00000008);
				return this;
			}

			public Builder Clone()
			{
				return Create().MergeFrom(BuildPartial());
			}

			public CommandGetOrCreateSchemaResponse DefaultInstanceForType => CommandGetOrCreateSchemaResponse.DefaultInstance;

            public CommandGetOrCreateSchemaResponse Build()
			{
				CommandGetOrCreateSchemaResponse Result = BuildPartial();
				if (!Result.Initialized)
				{
					throw new NullReferenceException($"{Result.GetType().Name} not initialized");
				}
				return Result;
			}

			
			public CommandGetOrCreateSchemaResponse BuildParsed()
			{
				CommandGetOrCreateSchemaResponse Result = BuildPartial();
				if (!Result.Initialized)
				{
					throw new NullReferenceException($"{Result.GetType().Name} not initialized");
				}
				return Result;
			}

			public CommandGetOrCreateSchemaResponse BuildPartial()
			{
				CommandGetOrCreateSchemaResponse Result = CommandGetOrCreateSchemaResponse._pool.Take();
				int FromBitField0_ = BitField0_;
				int ToBitField0_ = 0;
				if (((FromBitField0_ & 0x00000001) == 0x00000001))
				{
					ToBitField0_ |= 0x00000001;
				}
				Result.RequestId = (ulong)RequestId_;
				if (((FromBitField0_ & 0x00000002) == 0x00000002))
				{
					ToBitField0_ |= 0x00000002;
				}
				Result.ErrorCode = ErrorCode_;
				if (((FromBitField0_ & 0x00000004) == 0x00000004))
				{
					ToBitField0_ |= 0x00000004;
				}
				Result.ErrorMessage = ErrorMessage_.ToString();
				if (((FromBitField0_ & 0x00000008) == 0x00000008))
				{
					ToBitField0_ |= 0x00000008;
				}
				Result.SchemaVersion = SchemaVersion_;
				Result._hasBits0 = ToBitField0_;
				return Result;
			}

			public Builder MergeFrom(CommandGetOrCreateSchemaResponse Other)
			{
				if (Other == DefaultInstance)
				{
					return this;
				}
				if (Other.HasRequestId)
				{
					SetRequestId((long)Other.RequestId);
				}
				if (Other.HasErrorCode)
				{
					SetErrorCode(Other.ErrorCode);
				}
				if (Other.HasErrorMessage)
				{
					SetErrorMessage(Other.ErrorMessage);
				}
				if (Other.HasSchemaVersion)
				{
					SetSchemaVersion(Other.SchemaVersion);
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
						case 8:
							{
								BitField0_ |= 0x00000001;
								RequestId_ = input.ReadUInt64();
								break;
							}
						case 16:
							{
								int RawValue = input.ReadEnum();
								ServerError Value = Enum.GetValues(typeof(ServerError)).Cast<ServerError>().ToList()[RawValue];
								if (Value != null)
								{
									BitField0_ |= 0x00000002;
									ErrorCode_ = Value;
								}
								break;
							}
						case 26:
							{
								BitField0_ |= 0x00000004;
								ErrorMessage_ = input.ReadBytes();
								break;
							}
						case 34:
							{
								BitField0_ |= 0x00000008;
								SchemaVersion_ = input.ReadBytes();
								break;
							}
					}
				}
			}

			internal int BitField0_;

			// required uint64 request_id = 1;
			internal long RequestId_;
			public bool HasRequestId()
			{
				return ((BitField0_ & 0x00000001) == 0x00000001);
			}
			public long RequestId => RequestId_;

            public Builder SetRequestId(long Value)
			{
				BitField0_ |= 0x00000001;
				RequestId_ = Value;

				return this;
			}
			public Builder ClearRequestId()
			{
				BitField0_ = (BitField0_ & ~0x00000001);
				RequestId_ = 0L;

				return this;
			}

			// optional .pulsar.proto.ServerError error_code = 2;
			internal ServerError ErrorCode_ = ServerError.UnknownError;
			public bool HasErrorCode()
			{
				return ((BitField0_ & 0x00000002) == 0x00000002);
			}
			public ServerError ErrorCode => ErrorCode_;

            public Builder SetErrorCode(ServerError Value)
			{
				if (Value == null)
				{
					throw new NullReferenceException();
				}
				BitField0_ |= 0x00000002;
				ErrorCode_ = Value;

				return this;
			}
			public Builder ClearErrorCode()
			{
				BitField0_ = (BitField0_ & ~0x00000002);
				ErrorCode_ = ServerError.UnknownError;

				return this;
			}

			// optional string error_message = 3;
			internal object ErrorMessage_ = "";
			public bool HasErrorMessage()
			{
				return ((BitField0_ & 0x00000004) == 0x00000004);
			}
			public string GetErrorMessage()
			{
				object Ref = ErrorMessage_;
				if (!(Ref is string))
				{
					string S = ((ByteString)Ref).ToStringUtf8();
					ErrorMessage_ = S;
					return S;
				}
				else
				{
					return (string)Ref;
				}
			}
			public Builder SetErrorMessage(string Value)
			{
				if (string.ReferenceEquals(Value, null))
				{
					throw new NullReferenceException();
				}
				BitField0_ |= 0x00000004;
				ErrorMessage_ = Value;

				return this;
			}
			public Builder ClearErrorMessage()
			{
				BitField0_ = (BitField0_ & ~0x00000004);
				ErrorMessage_ = DefaultInstance.ErrorMessage;

				return this;
			}
			public void SetErrorMessage(ByteString Value)
			{
				BitField0_ |= 0x00000004;
				ErrorMessage_ = Value;

			}

			// optional bytes schema_version = 4;
			internal ByteString SchemaVersion_ = ByteString.Empty;
			public bool HasSchemaVersion()
			{
				return ((BitField0_ & 0x00000008) == 0x00000008);
			}
			public ByteString SchemaVersion => SchemaVersion_;

            public Builder SetSchemaVersion(ByteString Value)
			{
				if (Value == null)
				{
					throw new NullReferenceException();
				}
				BitField0_ |= 0x00000008;
				SchemaVersion_ = Value;

				return this;
			}
			public Builder ClearSchemaVersion()
			{
				BitField0_ = (BitField0_ & ~0x00000008);
				SchemaVersion_ = DefaultInstance.SchemaVersion;

				return this;
			}

			// @@protoc_insertion_point(builder_scope:pulsar.proto.CommandGetOrCreateSchemaResponse)
		}

		static CommandGetOrCreateSchemaResponse()
		{
			_defaultInstance = new CommandGetOrCreateSchemaResponse(true);
			_defaultInstance.InitFields();
		}

		// @@protoc_insertion_point(class_scope:pulsar.proto.CommandGetOrCreateSchemaResponse)
	}

}
