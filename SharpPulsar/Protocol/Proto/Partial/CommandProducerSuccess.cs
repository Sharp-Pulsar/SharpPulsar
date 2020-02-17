using DotNetty.Common;
using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.Text;
using SharpPulsar.Utility.Protobuf;
using static SharpPulsar.Utility.Protobuf.ByteBufCodedInputStream;

namespace SharpPulsar.Protocol.Proto
{
	public sealed partial class CommandProducerSuccess : ByteBufCodedOutputStream.ByteBufGeneratedMessage
	{
		// Use CommandProducerSuccess.newBuilder() to construct.
		internal static ThreadLocalPool<CommandProducerSuccess> _pool = new ThreadLocalPool<CommandProducerSuccess>(handle => new CommandProducerSuccess(handle), 1, true);

		internal ThreadLocalPool.Handle _handle;
		private CommandProducerSuccess(ThreadLocalPool.Handle handle)
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

		public CommandProducerSuccess(bool NoInit)
		{
		}

		
		internal static readonly CommandProducerSuccess _defaultInstance;
		public static CommandProducerSuccess DefaultInstance => _defaultInstance;

        public CommandProducerSuccess DefaultInstanceForType => _defaultInstance;


        public void InitFields()
		{
			RequestId = 0L;
			ProducerName = "";
			LastSequenceId = -1L;
			SchemaVersion = ByteString.Empty;
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
				if (!HasProducerName)
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
				Output.WriteBytes(2, ByteString.CopyFromUtf8(ProducerName));
			}
			if (((_hasBits0 & 0x00000004) == 0x00000004))
			{
				Output.WriteInt64(3, LastSequenceId);
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
		public static Builder NewBuilder(CommandProducerSuccess Prototype)
		{
			return NewBuilder().MergeFrom(Prototype);
		}
		public Builder ToBuilder()
		{
			return NewBuilder(this);
		}

		public sealed class Builder : ByteBufMessageBuilder
		{
			// Construct using org.apache.pulsar.common.api.proto.CommandProducerSuccess.newBuilder()
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
				ProducerName_ = "";
				BitField0_ = (BitField0_ & ~0x00000002);
				LastSequenceId_ = -1L;
				BitField0_ = (BitField0_ & ~0x00000004);
				SchemaVersion_ = ByteString.Empty;
				BitField0_ = (BitField0_ & ~0x00000008);
				return this;
			}

			public Builder Clone()
			{
				return Create().MergeFrom(BuildPartial());
			}

			public CommandProducerSuccess DefaultInstanceForType => CommandProducerSuccess.DefaultInstance;

            public CommandProducerSuccess Build()
			{
				CommandProducerSuccess Result = BuildPartial();
				if (!Result.Initialized)
				{
					throw new NullReferenceException($"{Result.GetType().Name} not initialized");
				}
				return Result;
			}

			
			public CommandProducerSuccess BuildParsed()
			{
				CommandProducerSuccess Result = BuildPartial();
				if (!Result.Initialized)
				{
					throw new NullReferenceException($"{Result.GetType().Name} not initialized");
				}
				return Result;
			}

			public CommandProducerSuccess BuildPartial()
			{
				CommandProducerSuccess Result = CommandProducerSuccess._pool.Take();
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
				Result.ProducerName = ProducerName_.ToString();
				if (((FromBitField0_ & 0x00000004) == 0x00000004))
				{
					ToBitField0_ |= 0x00000004;
				}
				Result.LastSequenceId = LastSequenceId_;
				if (((FromBitField0_ & 0x00000008) == 0x00000008))
				{
					ToBitField0_ |= 0x00000008;
				}
				Result.SchemaVersion = SchemaVersion_;
				Result._hasBits0 = ToBitField0_;
				return Result;
			}

			public Builder MergeFrom(CommandProducerSuccess Other)
			{
				if (Other == DefaultInstance)
				{
					return this;
				}
				if (Other.HasRequestId)
				{
					SetRequestId((long)Other.RequestId);
				}
				if (Other.HasProducerName)
				{
					SetProducerName(Other.ProducerName);
				}
				if (Other.HasLastSequenceId)
				{
					SetLastSequenceId(Other.LastSequenceId);
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
					if (!HasProducerName())
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
						case 18:
							{
								BitField0_ |= 0x00000002;
								ProducerName_ = input.ReadBytes();
								break;
							}
						case 24:
							{
								BitField0_ |= 0x00000004;
								LastSequenceId_ = input.ReadInt64();
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

			// required string producer_name = 2;
			internal object ProducerName_ = "";
			public bool HasProducerName()
			{
				return ((BitField0_ & 0x00000002) == 0x00000002);
			}
			public string GetProducerName()
			{
				object Ref = ProducerName_;
				if (!(Ref is string))
				{
					string S = ((ByteString)Ref).ToStringUtf8();
					ProducerName_ = S;
					return S;
				}
				else
				{
					return (string)Ref;
				}
			}
			public Builder SetProducerName(string Value)
			{
				if (string.ReferenceEquals(Value, null))
				{
					throw new NullReferenceException();
				}
				BitField0_ |= 0x00000002;
				ProducerName_ = Value;

				return this;
			}
			public Builder ClearProducerName()
			{
				BitField0_ = (BitField0_ & ~0x00000002);
				ProducerName_ = DefaultInstance.ProducerName;

				return this;
			}
			public void SetProducerName(ByteString Value)
			{
				BitField0_ |= 0x00000002;
				ProducerName_ = Value;

			}

			// optional int64 last_sequence_id = 3 [default = -1];
			internal long LastSequenceId_ = -1L;
			public bool HasLastSequenceId()
			{
				return ((BitField0_ & 0x00000004) == 0x00000004);
			}
			public long LastSequenceId => LastSequenceId_;

            public Builder SetLastSequenceId(long Value)
			{
				BitField0_ |= 0x00000004;
				LastSequenceId_ = Value;

				return this;
			}
			public Builder ClearLastSequenceId()
			{
				BitField0_ = (BitField0_ & ~0x00000004);
				LastSequenceId_ = -1L;

				return this;
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

			// @@protoc_insertion_point(builder_scope:pulsar.proto.CommandProducerSuccess)
		}

		static CommandProducerSuccess()
		{
			_defaultInstance = new CommandProducerSuccess(true);
			_defaultInstance.InitFields();
		}

		// @@protoc_insertion_point(class_scope:pulsar.proto.CommandProducerSuccess)
	}

}
