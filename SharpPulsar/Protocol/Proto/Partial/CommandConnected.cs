using DotNetty.Common;
using Google.Protobuf;
using System;
using System.IO;
using SharpPulsar.Utility.Protobuf;
using static SharpPulsar.Utility.Protobuf.ByteBufCodedInputStream;
using static SharpPulsar.Utility.Protobuf.ByteBufCodedOutputStream;

namespace SharpPulsar.Protocol.Proto
{
	public partial class CommandConnected : ByteBufGeneratedMessage 
	{
		// Use CommandConnected.newBuilder() to construct.
		internal static ThreadLocalPool<CommandConnected> _pool = new ThreadLocalPool<CommandConnected>(handle => new CommandConnected(handle), 1, true);

		internal ThreadLocalPool.Handle _handle;
		private CommandConnected(ThreadLocalPool.Handle handle)
		{
			_handle = handle;
		}

		public void Recycle()
		{
			this.InitFields();
			this.MemoizedIsInitialized = -1;
			this._hasBits0 = 0;
			this.MemoizedSerializedSize = -1;
			if (_handle != null)
			{
				_handle.Release(this);
			}
		}

		public CommandConnected(bool NoInit)
		{
		}

		internal static readonly CommandConnected _defaultInstance;
		public static CommandConnected DefaultInstance => _defaultInstance;

        public CommandConnected DefaultInstanceForType => _defaultInstance;


        public void InitFields()
		{
			serverVersion_ = "";
			protocolVersion_ = 0;
			maxMessageSize_ = 0;
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

				if (!HasServerVersion)
				{
					MemoizedIsInitialized = 0;
					return false;
				}
				MemoizedIsInitialized = 1;
				return true;
			}
		}
				
		public void WriteTo(ByteBufCodedOutputStream output)
		{
			var _ = SerializedSize;
			if (((_hasBits0 & 0x00000001) == 0x00000001))
			{
				output.WriteBytes(1, ByteString.CopyFromUtf8(serverVersion_));
			}
			if (((_hasBits0 & 0x00000002) == 0x00000002))
			{
				output.WriteInt32(2, protocolVersion_);
			}
			if (((_hasBits0 & 0x00000004) == 0x00000004))
			{
				output.WriteInt32(3, maxMessageSize_);
			}
		}

		internal int MemoizedSerializedSize = -1;
		public int SerializedSize => CalculateSize();
		

		internal const long SerialVersionUID = 0L;		
					
		public static CommandConnected ParseFrom(CodedInputStream input, ExtensionRegistry extensionRegistry)
		{
			return NewBuilder().MergeFrom(input, extensionRegistry).BuildParsed();
		}

		public static Builder NewBuilder()
		{
			return Builder.Create();
		}
		public Builder NewBuilderForType()
		{
			return NewBuilder();
		}
		public static Builder NewBuilder(CommandConnected prototype)
		{
			return NewBuilder().MergeFrom(prototype);
		}
		public Builder ToBuilder()
		{
			return NewBuilder(this);
		}

		public sealed class Builder : ByteBufMessageBuilder
		{
			// Construct using org.apache.pulsar.common.api.proto.CommandConnected.newBuilder()
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
				ServerVersion_ = "";
				_hasBits0 = (_hasBits0 & ~0x00000001);
				ProtocolVersion_ = 0;
				_hasBits0 = (_hasBits0 & ~0x00000002);
				MaxMessageSize_ = 0;
				_hasBits0 = (_hasBits0 & ~0x00000004);
				return this;
			}

			public Builder Clone()
			{
				return Create().MergeFrom(BuildPartial());
			}

			public CommandConnected DefaultInstanceForType => CommandConnected.DefaultInstance;

            public CommandConnected Build()
			{
				CommandConnected Result = BuildPartial();
				
				return Result;
			}

			public CommandConnected BuildParsed()
			{
				CommandConnected Result = BuildPartial();
				
				return Result;
			}

			public CommandConnected BuildPartial()
			{
				CommandConnected Result = CommandConnected._pool.Take();
				int From_hasBits0 = _hasBits0;
				int To_hasBits0 = 0;
				if (((From_hasBits0 & 0x00000001) == 0x00000001))
				{
					To_hasBits0 |= 0x00000001;
				}
				Result.serverVersion_ = ServerVersion_.ToString();
				if (((From_hasBits0 & 0x00000002) == 0x00000002))
				{
					To_hasBits0 |= 0x00000002;
				}
				Result.protocolVersion_ = ProtocolVersion_;
				if (((From_hasBits0 & 0x00000004) == 0x00000004))
				{
					To_hasBits0 |= 0x00000004;
				}
				Result.maxMessageSize_ = MaxMessageSize_;
				Result._hasBits0 = To_hasBits0;
				return Result;
			}

			public Builder MergeFrom(CommandConnected Other)
			{
				if (Other == CommandConnected.DefaultInstance)
				{
					return this;
				}
				if (Other.HasServerVersion)
				{
					SetServerVersion(Other.ServerVersion);
				}
				if (Other.HasProtocolVersion)
				{
					SetProtocolVersion(Other.ProtocolVersion);
				}
				if (Other.HasMaxMessageSize)
				{
					SetMaxMessageSize(Other.MaxMessageSize);
				}
				return this;
			}

			public bool Initialized
			{
				get
				{
					if (!HasServerVersion())
					{

						return false;
					}
					return true;
				}
			}
			public Builder MergeFrom(CodedInputStream input, ExtensionRegistry extensionRegistry)
			{
				throw new IOException("Merge from CodedInputStream is disabled");
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
								_hasBits0 |= 0x00000001;
								ServerVersion_ = input.ReadBytes();
								break;
							}
						case 16:
							{
								_hasBits0 |= 0x00000002;
								ProtocolVersion_ = input.ReadInt32();
								break;
							}
						case 24:
							{
								_hasBits0 |= 0x00000004;
								MaxMessageSize_ = input.ReadInt32();
								break;
							}
					}
				}
			}

			internal int _hasBits0;

			// required string server_version = 1;
			internal object ServerVersion_ = "";
			public bool HasServerVersion()
			{
				return ((_hasBits0 & 0x00000001) == 0x00000001);
			}
			
			public Builder SetServerVersion(string Value)
			{
				if (string.ReferenceEquals(Value, null))
				{
					throw new NullReferenceException();
				}
				_hasBits0 |= 0x00000001;
				ServerVersion_ = Value;

				return this;
			}
			public Builder ClearServerVersion()
			{
				_hasBits0 = (_hasBits0 & ~0x00000001);
				ServerVersion_ = DefaultInstance.ServerVersion;

				return this;
			}
			public void SetServerVersion(ByteString Value)
			{
				_hasBits0 |= 0x00000001;
				ServerVersion_ = Value;

			}

			// optional int32 protocol_version = 2 [default = 0];
			internal int ProtocolVersion_;
			public bool HasProtocolVersion()
			{
				return ((_hasBits0 & 0x00000002) == 0x00000002);
			}
			public int ProtocolVersion => ProtocolVersion_;

            public Builder SetProtocolVersion(int Value)
			{
				_hasBits0 |= 0x00000002;
				ProtocolVersion_ = Value;

				return this;
			}
			public Builder ClearProtocolVersion()
			{
				_hasBits0 = (_hasBits0 & ~0x00000002);
				ProtocolVersion_ = 0;

				return this;
			}

			// optional int32 max_message_size = 3;
			internal int MaxMessageSize_;
			public bool HasMaxMessageSize()
			{
				return ((_hasBits0 & 0x00000004) == 0x00000004);
			}
			public int MaxMessageSize => MaxMessageSize_;

            public Builder SetMaxMessageSize(int Value)
			{
				_hasBits0 |= 0x00000004;
				MaxMessageSize_ = Value;

				return this;
			}
			public Builder ClearMaxMessageSize()
			{
				_hasBits0 = (_hasBits0 & ~0x00000004);
				MaxMessageSize_ = 0;

				return this;
			}

			ByteBufMessageBuilder ByteBufMessageBuilder.MergeFrom(ByteBufCodedInputStream input, ExtensionRegistry ext)
			{
				throw new NotImplementedException();
			}


			// @@protoc_insertion_point(builder_scope:pulsar.proto.CommandConnected)
		}

		static CommandConnected()
		{
			_defaultInstance = new CommandConnected(true);
			_defaultInstance.InitFields();
		}

	}

}
