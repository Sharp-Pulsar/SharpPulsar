using DotNetty.Common;
using Google.Protobuf;
using System;
using System.Linq;
using SharpPulsar.Utility.Protobuf;
using static SharpPulsar.Utility.Protobuf.ByteBufCodedInputStream;

namespace SharpPulsar.Protocol.Proto
{
	public sealed partial class CommandPartitionedTopicMetadataResponse : ByteBufCodedOutputStream.ByteBufGeneratedMessage
	{
		// Use CommandPartitionedTopicMetadataResponse.newBuilder() to construct.
		internal static ThreadLocalPool<CommandPartitionedTopicMetadataResponse> _pool = new ThreadLocalPool<CommandPartitionedTopicMetadataResponse>(handle => new CommandPartitionedTopicMetadataResponse(handle), 1, true);

		internal ThreadLocalPool.Handle _handle;
		private CommandPartitionedTopicMetadataResponse(ThreadLocalPool.Handle handle)
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

		public CommandPartitionedTopicMetadataResponse(bool NoInit)
		{
		}

		
		internal static readonly CommandPartitionedTopicMetadataResponse _defaultInstance;
		public static CommandPartitionedTopicMetadataResponse DefaultInstance => _defaultInstance;

        public CommandPartitionedTopicMetadataResponse DefaultInstanceForType => _defaultInstance;

        public void InitFields()
		{
			Partitions = 0;
			RequestId = 0L;
			Response = CommandPartitionedTopicMetadataResponse.Types.LookupType.Success;
			Error = ServerError.UnknownError;
			Message = "";
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
				Output.WriteUInt32(1, (int)Partitions);
			}
			if (((_hasBits0 & 0x00000002) == 0x00000002))
			{
				Output.WriteUInt64(2, (long)RequestId);
			}
			if (((_hasBits0 & 0x00000004) == 0x00000004))
			{
				Output.WriteEnum(3, (int)Response);
			}
			if (((_hasBits0 & 0x00000008) == 0x00000008))
			{
				Output.WriteEnum(4, (int)Error);
			}
			if (((_hasBits0 & 0x00000010) == 0x00000010))
			{
				Output.WriteBytes(5, ByteString.CopyFromUtf8(Message));
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
		public static Builder NewBuilder(CommandPartitionedTopicMetadataResponse Prototype)
		{
			return NewBuilder().MergeFrom(Prototype);
		}
		public Builder ToBuilder()
		{
			return NewBuilder(this);
		}

		public sealed class Builder : ByteBufMessageBuilder
		{
			// Construct using org.apache.pulsar.common.api.proto.CommandPartitionedTopicMetadataResponse.newBuilder()
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

				Partitions_ = 0;
				BitField0_ = (BitField0_ & ~0x00000001);
				RequestId_ = 0L;
				BitField0_ = (BitField0_ & ~0x00000002);
				Response_ = CommandPartitionedTopicMetadataResponse.Types.LookupType.Success;
				BitField0_ = (BitField0_ & ~0x00000004);
				Error_ = ServerError.UnknownError;
				BitField0_ = (BitField0_ & ~0x00000008);
				Message_ = "";
				BitField0_ = (BitField0_ & ~0x00000010);
				return this;
			}

			public Builder Clone()
			{
				return Create().MergeFrom(BuildPartial());
			}

			public CommandPartitionedTopicMetadataResponse DefaultInstanceForType => CommandPartitionedTopicMetadataResponse.DefaultInstance;

            public CommandPartitionedTopicMetadataResponse Build()
			{
				CommandPartitionedTopicMetadataResponse Result = BuildPartial();
				if (!Result.Initialized)
				{
					throw new NullReferenceException($"{Result.GetType().Name} not initialized");
				}
				return Result;
			}

			
			public CommandPartitionedTopicMetadataResponse BuildParsed()
			{
				CommandPartitionedTopicMetadataResponse Result = BuildPartial();
				if (!Result.Initialized)
				{
					throw new NullReferenceException($"{Result.GetType().Name} not initialized");
				}
				return Result;
			}

			public CommandPartitionedTopicMetadataResponse BuildPartial()
			{
				CommandPartitionedTopicMetadataResponse Result = CommandPartitionedTopicMetadataResponse._pool.Take();
				int FromBitField0_ = BitField0_;
				int ToBitField0_ = 0;
				if (((FromBitField0_ & 0x00000001) == 0x00000001))
				{
					ToBitField0_ |= 0x00000001;
				}
				Result.Partitions = (uint)Partitions_;
				if (((FromBitField0_ & 0x00000002) == 0x00000002))
				{
					ToBitField0_ |= 0x00000002;
				}
				Result.RequestId = (ulong)RequestId_;
				if (((FromBitField0_ & 0x00000004) == 0x00000004))
				{
					ToBitField0_ |= 0x00000004;
				}
				Result.Response = Response_;
				if (((FromBitField0_ & 0x00000008) == 0x00000008))
				{
					ToBitField0_ |= 0x00000008;
				}
				Result.Error = Error_;
				if (((FromBitField0_ & 0x00000010) == 0x00000010))
				{
					ToBitField0_ |= 0x00000010;
				}
				Result.Message = Message_.ToString();
				Result._hasBits0 = ToBitField0_;
				return Result;
			}

			public Builder MergeFrom(CommandPartitionedTopicMetadataResponse Other)
			{
				if (Other == DefaultInstance)
				{
					return this;
				}
				if (Other.HasPartitions)
				{
					SetPartitions((int)Other.Partitions);
				}
				if (Other.HasRequestId)
				{
					SetRequestId((long)Other.RequestId);
				}
				if (Other.HasResponse)
				{
					SetResponse(Other.Response);
				}
				if (Other.HasError)
				{
					SetError(Other.Error);
				}
				if (Other.HasMessage)
				{
					SetMessage(Other.Message);
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
								Partitions_ = input.ReadUInt32();
								break;
							}
						case 16:
							{
								BitField0_ |= 0x00000002;
								RequestId_ = input.ReadUInt64();
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
								int RawValue = input.ReadEnum();
								ServerError Value = Enum.GetValues(typeof(ServerError)).Cast<ServerError>().ToList()[RawValue];
								if (Value != null)
								{
									BitField0_ |= 0x00000008;
									Error_ = Value;
								}
								break;
							}
						case 42:
							{
								BitField0_ |= 0x00000010;
								Message_ = input.ReadBytes();
								break;
							}
					}
				}
			}

			internal int BitField0_;

			// optional uint32 partitions = 1;
			internal int Partitions_;
			public bool HasPartitions()
			{
				return ((BitField0_ & 0x00000001) == 0x00000001);
			}
			public int Partitions => Partitions_;

            public Builder SetPartitions(int Value)
			{
				BitField0_ |= 0x00000001;
				Partitions_ = Value;

				return this;
			}
			public Builder ClearPartitions()
			{
				BitField0_ = (BitField0_ & ~0x00000001);
				Partitions_ = 0;

				return this;
			}

			// required uint64 request_id = 2;
			internal long RequestId_;
			public bool HasRequestId()
			{
				return ((BitField0_ & 0x00000002) == 0x00000002);
			}
			public long RequestId => RequestId_;

            public Builder SetRequestId(long Value)
			{
				BitField0_ |= 0x00000002;
				RequestId_ = Value;

				return this;
			}
			public Builder ClearRequestId()
			{
				BitField0_ = (BitField0_ & ~0x00000002);
				RequestId_ = 0L;

				return this;
			}

			// optional .pulsar.proto.CommandPartitionedTopicMetadataResponse.LookupType response = 3;
			internal Types.LookupType Response_ = CommandPartitionedTopicMetadataResponse.Types.LookupType.Success;
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
				Response_ = CommandPartitionedTopicMetadataResponse.Types.LookupType.Success;

				return this;
			}

			// optional .pulsar.proto.ServerError error = 4;
			internal ServerError Error_ = ServerError.UnknownError;
			public bool HasError()
			{
				return ((BitField0_ & 0x00000008) == 0x00000008);
			}
			public ServerError Error => Error_;

            public Builder SetError(ServerError Value)
			{
				if (Value == null)
				{
					throw new NullReferenceException();
				}
				BitField0_ |= 0x00000008;
				Error_ = Value;

				return this;
			}
			public Builder ClearError()
			{
				BitField0_ = (BitField0_ & ~0x00000008);
				Error_ = ServerError.UnknownError;

				return this;
			}

			// optional string message = 5;
			internal object Message_ = "";
			public bool HasMessage()
			{
				return ((BitField0_ & 0x00000010) == 0x00000010);
			}
			public string GetMessage()
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
				BitField0_ |= 0x00000010;
				Message_ = Value;

				return this;
			}
			public Builder ClearMessage()
			{
				BitField0_ = (BitField0_ & ~0x00000010);
				Message_ = DefaultInstance.Message;

				return this;
			}
			public void SetMessage(ByteString Value)
			{
				BitField0_ |= 0x00000010;
				Message_ = Value;

			}

			// @@protoc_insertion_point(builder_scope:pulsar.proto.CommandPartitionedTopicMetadataResponse)
		}

		static CommandPartitionedTopicMetadataResponse()
		{
			_defaultInstance = new CommandPartitionedTopicMetadataResponse(true);
			_defaultInstance.InitFields();
		}

		// @@protoc_insertion_point(class_scope:pulsar.proto.CommandPartitionedTopicMetadataResponse)
	}

}
