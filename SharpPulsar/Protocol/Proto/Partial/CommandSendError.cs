using DotNetty.Common;
using Google.Protobuf;
using System;
using System.Linq;
using SharpPulsar.Utility.Protobuf;
using static SharpPulsar.Utility.Protobuf.ByteBufCodedInputStream;

namespace SharpPulsar.Protocol.Proto
{
	public sealed partial class CommandSendError : ByteBufCodedOutputStream.ByteBufGeneratedMessage
	{
		// Use CommandSendError.newBuilder() to construct.
		internal static ThreadLocalPool<CommandSendError> _pool = new ThreadLocalPool<CommandSendError>(handle => new CommandSendError(handle), 1, true);

		internal ThreadLocalPool.Handle _handle;
		private CommandSendError(ThreadLocalPool.Handle handle)
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

		public CommandSendError(bool NoInit)
		{
		}

		
		internal static readonly CommandSendError _defaultInstance;
		public static CommandSendError DefaultInstance => _defaultInstance;

        public CommandSendError DefaultInstanceForType => _defaultInstance;

        public void InitFields()
		{
			ProducerId = 0L;
			SequenceId = 0L;
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

				if (!HasProducerId)
				{
					MemoizedIsInitialized = 0;
					return false;
				}
				if (!HasSequenceId)
				{
					MemoizedIsInitialized = 0;
					return false;
				}
				if (!HasError)
				{
					MemoizedIsInitialized = 0;
					return false;
				}
				if (!HasMessage)
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
				Output.WriteUInt64(1, (long)ProducerId);
			}
			if (((_hasBits0 & 0x00000002) == 0x00000002))
			{
				Output.WriteUInt64(2, (long)SequenceId);
			}
			if (((_hasBits0 & 0x00000004) == 0x00000004))
			{
				Output.WriteEnum(3, (int)Error);
			}
			if (((_hasBits0 & 0x00000008) == 0x00000008))
			{
				Output.WriteBytes(4, ByteString.CopyFromUtf8(Message));
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
		public static Builder NewBuilder(CommandSendError Prototype)
		{
			return NewBuilder().MergeFrom(Prototype);
		}
		public Builder ToBuilder()
		{
			return NewBuilder(this);
		}

		public sealed class Builder : ByteBufMessageBuilder
		{
			// Construct using org.apache.pulsar.common.api.proto.CommandSendError.newBuilder()
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
				ProducerId_ = 0L;
				BitField0_ = (BitField0_ & ~0x00000001);
				SequenceId_ = 0L;
				BitField0_ = (BitField0_ & ~0x00000002);
				Error_ = ServerError.UnknownError;
				BitField0_ = (BitField0_ & ~0x00000004);
				Message_ = "";
				BitField0_ = (BitField0_ & ~0x00000008);
				return this;
			}

			public Builder Clone()
			{
				return Create().MergeFrom(BuildPartial());
			}

			public CommandSendError DefaultInstanceForType => CommandSendError.DefaultInstance;

            public CommandSendError Build()
			{
				CommandSendError Result = BuildPartial();
				if (!Result.Initialized)
				{
					throw new NullReferenceException($"{Result.GetType().Name} not initialized");
				}
				return Result;
			}

			public CommandSendError BuildParsed()
			{
				CommandSendError Result = BuildPartial();
				if (!Result.Initialized)
				{
					throw new NullReferenceException($"{Result.GetType().Name} not initialized");
				}
				return Result;
			}

			public CommandSendError BuildPartial()
			{
				CommandSendError Result = CommandSendError._pool.Take();
				int FromBitField0_ = BitField0_;
				int ToBitField0_ = 0;
				if (((FromBitField0_ & 0x00000001) == 0x00000001))
				{
					ToBitField0_ |= 0x00000001;
				}
				Result.ProducerId = (ulong)ProducerId_;
				if (((FromBitField0_ & 0x00000002) == 0x00000002))
				{
					ToBitField0_ |= 0x00000002;
				}
				Result.SequenceId = (ulong)SequenceId_;
				if (((FromBitField0_ & 0x00000004) == 0x00000004))
				{
					ToBitField0_ |= 0x00000004;
				}
				Result.Error = Error_;
				if (((FromBitField0_ & 0x00000008) == 0x00000008))
				{
					ToBitField0_ |= 0x00000008;
				}
				Result.Message = Message_.ToString();
				Result._hasBits0 = ToBitField0_;
				return Result;
			}

			public Builder MergeFrom(CommandSendError Other)
			{
				if (Other == CommandSendError.DefaultInstance)
				{
					return this;
				}
				if (Other.HasProducerId)
				{
					SetProducerId((long)Other.ProducerId);
				}
				if (Other.HasSequenceId)
				{
					SetSequenceId((long)Other.SequenceId);
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
					if (!HasProducerId())
					{

						return false;
					}
					if (!HasSequenceId())
					{

						return false;
					}
					if (!HasError())
					{

						return false;
					}
					if (!HasMessage())
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
								ProducerId_ = input.ReadUInt64();
								break;
							}
						case 16:
							{
								BitField0_ |= 0x00000002;
								SequenceId_ = input.ReadUInt64();
								break;
							}
						case 24:
							{
								int RawValue = input.ReadEnum();
								ServerError Value = Enum.GetValues(typeof(ServerError)).Cast<ServerError>().ToList()[RawValue];
								if (Value != null)
								{
									BitField0_ |= 0x00000004;
									Error_ = Value;
								}
								break;
							}
						case 34:
							{
								BitField0_ |= 0x00000008;
								Message_ = input.ReadBytes();
								break;
							}
					}
				}
			}

			internal int BitField0_;

			// required uint64 producer_id = 1;
			internal long ProducerId_;
			public bool HasProducerId()
			{
				return ((BitField0_ & 0x00000001) == 0x00000001);
			}
			public long ProducerId => ProducerId_;

            public Builder SetProducerId(long Value)
			{
				BitField0_ |= 0x00000001;
				ProducerId_ = Value;

				return this;
			}
			public Builder ClearProducerId()
			{
				BitField0_ = (BitField0_ & ~0x00000001);
				ProducerId_ = 0L;

				return this;
			}

			// required uint64 sequence_id = 2;
			internal long SequenceId_;
			public bool HasSequenceId()
			{
				return ((BitField0_ & 0x00000002) == 0x00000002);
			}
			public long SequenceId => SequenceId_;

            public Builder SetSequenceId(long Value)
			{
				BitField0_ |= 0x00000002;
				SequenceId_ = Value;

				return this;
			}
			public Builder ClearSequenceId()
			{
				BitField0_ = (BitField0_ & ~0x00000002);
				SequenceId_ = 0L;

				return this;
			}

			// required .pulsar.proto.ServerError error = 3;
			internal ServerError Error_ = ServerError.UnknownError;
			public bool HasError()
			{
				return ((BitField0_ & 0x00000004) == 0x00000004);
			}
			public ServerError Error => Error_;

            public Builder SetError(ServerError Value)
			{
				if (Value == null)
				{
					throw new NullReferenceException();
				}
				BitField0_ |= 0x00000004;
				Error_ = Value;

				return this;
			}
			public Builder ClearError()
			{
				BitField0_ = (BitField0_ & ~0x00000004);
				Error_ = ServerError.UnknownError;

				return this;
			}

			// required string message = 4;
			internal object Message_ = "";
			public bool HasMessage()
			{
				return ((BitField0_ & 0x00000008) == 0x00000008);
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
				BitField0_ |= 0x00000008;
				Message_ = Value;

				return this;
			}
			public Builder ClearMessage()
			{
				BitField0_ = (BitField0_ & ~0x00000008);
				Message_ = DefaultInstance.Message;

				return this;
			}
			public void SetMessage(ByteString Value)
			{
				BitField0_ |= 0x00000008;
				Message_ = Value;

			}

			// @@protoc_insertion_point(builder_scope:pulsar.proto.CommandSendError)
		}

		static CommandSendError()
		{
			_defaultInstance = new CommandSendError(true);
			_defaultInstance.InitFields();
		}

		// @@protoc_insertion_point(class_scope:pulsar.proto.CommandSendError)
	}

}
